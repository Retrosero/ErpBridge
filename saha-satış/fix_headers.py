import re
with open("app/src/main/java/com/example/util/TelemetryUploadWorker.kt", "r") as f:
    content = f.read()

old_headers = """                        .addHeader("Content-Type", "application/json")
                        .build()"""
                        
new_headers = """                        .addHeader("Content-Type", "application/json")
                        .apply {
                            if (!tenantId.isNullOrBlank()) {
                                addHeader("X-Tenant-Id", tenantId)
                            }
                        }
                        .build()"""

if old_headers in content:
    content = content.replace(old_headers, new_headers)
    with open("app/src/main/java/com/example/util/TelemetryUploadWorker.kt", "w") as f:
        f.write(content)
    print("Fixed headers")
