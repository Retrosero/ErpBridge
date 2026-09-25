"""GOAL_PANEL_ERPSIZ E8b: cold/warm timings of the ERP-less panel endpoints.

Only against a local or test server. The company must be seeded with native-panel-seed.sql and have an ADMIN user
"patron":

    ERPB_PATRON_PASSWORD=... python scripts/perf/native-panel-measure.py <tenant code> [read index]

ERPB_API overrides the server address (default http://localhost:5281).

A cold figure is only honest for the first request a freshly started server answers: the pages share their mirrors,
so one page's first read warms the next (Codex #194). With a read index (0-6) the script measures only that read —
cold, then warm — and nothing else; restart the server before each such run. Without it, the full sequence (reads
twice, writes, reads after writes) measures the warm paths and the saves.
"""
import json
import os
import sys
import time
import urllib.request
import uuid

API = os.environ.get("ERPB_API", "http://localhost:5281")


def call(method, path, body=None, token=None):
    data = None if body is None else json.dumps(body).encode()
    req = urllib.request.Request(API + path, data=data, method=method)
    req.add_header("Content-Type", "application/json")
    if token:
        req.add_header("Authorization", "Bearer " + token)
    started = time.perf_counter()
    try:
        with urllib.request.urlopen(req, timeout=120) as r:
            text, status = r.read().decode(), r.status
    except urllib.error.HTTPError as e:
        text, status = e.read().decode(), e.code
    return status, (time.perf_counter() - started) * 1000, text


def main():
    code = sys.argv[1]
    _, _, body = call("POST", "/api/v1/android/account/login", {
        "tenantCode": code, "username": "patron", "password": os.environ["ERPB_PATRON_PASSWORD"],
        "deviceId": "perf-dev", "appVersion": "perf"})
    token = json.loads(body)["token"]

    reads = [
        ("stok listesi", "/api/v1/portal/stock/search?sort=name&dir=asc&page=1&pageSize=50"),
        ("stok arama", "/api/v1/portal/stock/search?q=%C3%9Cr%C3%BCn%201999&sort=name&dir=asc&page=1&pageSize=50"),
        ("cari listesi", "/api/v1/portal/customers?sort=title&dir=asc&page=1&pageSize=50"),
        ("cari ekstresi (K0001)", "/api/v1/portal/customers/ledger?code=K0001&page=1&pageSize=50"),
        ("evrak listesi (iki yıl)", "/api/v1/portal/native/documents?from=2025-01-01&to=2026-12-31&page=1&pageSize=50"),
        ("tahsilat listesi (iki yıl)", "/api/v1/portal/payments?from=2025-01-01&to=2026-12-31&page=1&pageSize=50"),
        ("ürün hareketleri (P00001)", "/api/v1/portal/native/stock-cards/P00001/movements?page=1&pageSize=50"),
    ]
    writes = [
        ("satış kaydet (3 kalem)", "/api/v1/portal/native/sales-orders", lambda: {
            "partyCode": "K0001", "operationId": uuid.uuid4().hex,
            "lines": [{"productCode": "P00001", "quantity": 1, "unitPrice": 10},
                      {"productCode": "P00002", "quantity": 2, "unitPrice": 10},
                      {"productCode": "P00003", "quantity": 1, "unitPrice": 10}]}),
        ("tahsilat kaydet", "/api/v1/portal/native/collections", lambda: {
            "customerCode": "K0001", "amount": 50, "paymentType": "Nakit", "operationId": uuid.uuid4().hex}),
    ]

    rows = []
    if len(sys.argv) > 2:
        name, path = reads[int(sys.argv[2])]
        for round_name in ("soğuk (tek)", "sıcak"):
            status, ms, _ = call("GET", path, token=token)
            rows.append((round_name, name, status, ms))
        for round_name, name, status, ms in rows:
            print(f"{round_name:15} {name:28} {status} {ms:8.0f} ms")
        return

    for round_name in ("ilk tur", "sıcak"):
        for name, path in reads:
            status, ms, _ = call("GET", path, token=token)
            rows.append((round_name, name, status, ms))
    for round_name in ("1.", "2."):
        for name, path, body_of in writes:
            status, ms, _ = call("POST", path, body_of(), token)
            rows.append((round_name, name, status, ms))
    for name, path in reads:
        status, ms, _ = call("GET", path, token=token)
        rows.append(("kayıttan sonra", name, status, ms))

    for round_name, name, status, ms in rows:
        print(f"{round_name:15} {name:28} {status} {ms:8.0f} ms")


if __name__ == "__main__":
    main()
