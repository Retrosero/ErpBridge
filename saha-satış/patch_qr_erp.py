import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    content = f.read()

# Add trailing icon to apiKey field in GOAPP ERP
target_1 = """label = { Text("Aktivasyon Kodu", style = MaterialTheme.typography.bodySmall) },
                                    placeholder = { Text("örn: AK-API_ANAHTARI") },
                                    supportingText = { Text("Güvenli veri senkronizasyonu için lisans API anahtarınız.", style = MaterialTheme.typography.labelSmall) },
                                    modifier = Modifier.fillMaxWidth(),
                                    textStyle = MaterialTheme.typography.bodySmall"""
replacement_1 = """label = { Text("Aktivasyon Kodu", style = MaterialTheme.typography.bodySmall) },
                                    placeholder = { Text("Aktivasyon kodunu girin veya QR taratın") },
                                    supportingText = { Text("Güvenli veri senkronizasyonu için aktivasyon kodunuz.", style = MaterialTheme.typography.labelSmall) },
                                    modifier = Modifier.fillMaxWidth(),
                                    textStyle = MaterialTheme.typography.bodySmall,
                                    trailingIcon = {
                                        IconButton(onClick = { AppDataStore.globalShowBarcodeScanner = true }) {
                                            Icon(Icons.Filled.QrCodeScanner, contentDescription = "QR Tarat")
                                        }
                                    }"""

if target_1 in content:
    content = content.replace(target_1, replacement_1)

# Add showBarcodeScanner dialog if globalShowBarcodeScanner is true, wait it's already in NavApp, but since we are replacing the barcode scanning, maybe it just sets the value? But wait, how do we get the scanned barcode back into the `apiKey` field?
# If `AppDataStore.globalShowBarcodeScanner` is used, where does the result go?
