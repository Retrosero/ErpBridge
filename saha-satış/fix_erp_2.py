import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    content = f.read()

start_marker = 'val appVersion = try { context.packageManager.getPackageInfo(context.packageName, 0).versionName ?: "1.0.0" } catch (e: Exception) { "1.0.0" }'
end_marker = 'connectionTestResult = localized\n                                                    log("❌ Bağlantı testi başarısız. HTTP: $code")\n                                                }'

new_block = """
val appVersion = try { context.packageManager.getPackageInfo(context.packageName, 0).versionName ?: "1.0.0" } catch (e: Exception) { "1.0.0" }
val isValid = com.example.data.LicenseRepository.authenticateLicense(context, apiKey, appVersion)
if (isValid) { 
    isConnectionSuccess = true
    connectionTestResult = "Bağlantı Başarılı!"
    startSyncAll()
    log("✅ Bağlantı testi başarılı!")
} else { 
    isConnectionSuccess = false
    connectionTestResult = "Bağlantı Hatası: Aktivasyon kodu geçersiz veya yetkiniz yok."
    log("❌ Bağlantı testi başarısız")
}
"""

content = re.sub(re.escape(start_marker) + r'.*?' + re.escape(end_marker), new_block.strip(), content, flags=re.DOTALL)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.write(content)
