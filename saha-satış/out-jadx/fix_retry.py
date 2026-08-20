import re

with open('app/src/main/java/com/example/data/api/ApiClient.kt', 'r', encoding='utf-8') as f:
    content = f.read()

new_retry = """private class RetryInterceptor : Interceptor {
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
                if (attempt < 3) response.close()
            } catch (e: java.io.IOException) {
                exception = e
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
}"""

# Use regex to replace the RetryInterceptor block
content = re.sub(
    r'private class RetryInterceptor : Interceptor \{.*?return response\s*\}',
    new_retry,
    content,
    flags=re.DOTALL
)

# And increase timeouts to 60 seconds
content = content.replace('.connectTimeout(30, TimeUnit.SECONDS)', '.connectTimeout(60, TimeUnit.SECONDS)')
content = content.replace('.readTimeout(30, TimeUnit.SECONDS)', '.readTimeout(60, TimeUnit.SECONDS)')

with open('app/src/main/java/com/example/data/api/ApiClient.kt', 'w', encoding='utf-8') as f:
    f.write(content)
