import re
with open('app/src/main/java/com/example/data/api/ApiClient.kt', 'r', encoding='utf-8') as f:
    content = f.read()

retry_class_new = """
private class RetryInterceptor : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        var response: Response? = null
        var exception: java.io.IOException? = null
        var attempt = 1
        var delayMs = 2_000L
        while (attempt <= 3) {
            try {
                response = chain.proceed(request)
                if (response.code != 429 && response.code !in 500..599) {
                    return response
                }
                android.util.Log.w("ApiClient", "API hatası, deneme $attempt/3: HTTP ${response.code}")
                if (attempt < 3) response.close()
            } catch (e: java.io.IOException) {
                exception = e
                android.util.Log.e("ApiClient", "Ağ hatası, deneme $attempt/3: ${e.message}")
                if (attempt == 3) throw e
            }
            if (attempt < 3) {
                try {
                    Thread.sleep(delayMs)
                } catch (_: InterruptedException) {
                    Thread.currentThread().interrupt()
                    break
                }
                delayMs *= 2
            }
            attempt++
        }
        return response ?: throw exception ?: java.io.IOException("Bilinmeyen ağ hatası")
    }
}
"""

content = re.sub(
    r'private class RetryInterceptor : Interceptor \{.*?return response \?: throw exception \?: java.io.IOException\("Bilinmeyen ağ hatası"\)\s*\}\s*\}',
    retry_class_new.strip(),
    content,
    flags=re.DOTALL
)

content = content.replace(
    ".connectTimeout(60, TimeUnit.SECONDS)\n            .readTimeout(60, TimeUnit.SECONDS)",
    ".connectTimeout(60, TimeUnit.SECONDS)\n            .readTimeout(60, TimeUnit.SECONDS)\n            .writeTimeout(60, TimeUnit.SECONDS)"
)

with open('app/src/main/java/com/example/data/api/ApiClient.kt', 'w', encoding='utf-8') as f:
    f.write(content)
