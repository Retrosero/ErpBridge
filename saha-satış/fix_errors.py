import re

content = open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt").read()

helper = """
    private fun handleApiError(response: retrofit2.Response<*>, log: (String) -> Unit): Exception {
        val code = response.code()
        val errorBody = response.errorBody()?.string() ?: ""
        var safeMessage = "Bilinmeyen Hata"
        try {
            if (errorBody.isNotEmpty()) {
                val json = org.json.JSONObject(errorBody)
                val msg = json.optString("message", json.optString("error", "Bilinmeyen API Hatası"))
                val errCode = json.optString("code", "")
                safeMessage = if (errCode.isNotEmpty()) "$errCode - $msg" else msg
            }
        } catch (e: Exception) {
            safeMessage = "Yanıt okunamadı"
        }
        
        val userFriendlyMessage = when (code) {
            401, 403 -> "Yetkilendirme Hatası: API Anahtarı veya Tenant ID geçersiz ($safeMessage)"
            422 -> "Doğrulama Hatası: Gönderilen parametreler hatalı ($safeMessage)"
            429 -> "İstek Sınırı Aşıldı: Çok fazla istek gönderdiniz ($safeMessage)"
            in 500..599 -> "Sunucu Hatası: GoApp Cloud sunucusunda bir sorun oluştu ($safeMessage)"
            else -> "Ağ Hatası [$code] ($safeMessage)"
        }
        log("Hata [$code]: $safeMessage")
        return Exception(userFriendlyMessage)
    }
"""

if "private fun handleApiError" not in content:
    content = content.replace("object BridgeSyncHelper {", "object BridgeSyncHelper {" + helper)

content = re.sub(r'throw Exception\("API Yanıt Hatası"\)', 'throw handleApiError(response, log)', content)
content = re.sub(r'log\("Hata: [^"]*Kod: \$\{response\.code\(\)\}"\)', 'handleApiError(response, log)', content)
content = re.sub(r'throw Exception\("API Hatası \\"[^"]*Kod: \$\{response\.code\(\)\}\\"\)"\)', 'throw handleApiError(response, log)', content)

open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w").write(content)
