import re

with open("app/src/main/java/com/example/data/api/ApiClient.kt", "r") as f:
    content = f.read()

# Add TelemetryInterceptor
telemetry_interceptor_code = """
class TelemetryInterceptor : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        val route = request.url.encodedPath
        
        if (route.contains("/mobile/telemetry/batch")) {
            return chain.proceed(request)
        }
        
        val response: Response
        try {
            response = chain.proceed(request)
        } catch (e: Exception) {
            com.example.util.TelemetryReporter.reportHttpError(
                method = request.method,
                route = route,
                statusCode = null,
                operation = "OkHttp_Request_Exception",
                message = e.message ?: "Network error"
            )
            throw e
        }
        
        if (!response.isSuccessful && response.code in 400..599) {
            com.example.util.TelemetryReporter.reportHttpError(
                method = request.method,
                route = route,
                statusCode = response.code,
                operation = "OkHttp_Error_Response",
                message = "HTTP ${response.code} ${response.message}"
            )
        }
        return response
    }
}
"""

if "class TelemetryInterceptor" not in content:
    content = content.replace("object ApiClient {", telemetry_interceptor_code + "\nobject ApiClient {")

# Update HttpLoggingInterceptor to depend on BuildConfig.DEBUG
content = content.replace(
    "level = HttpLoggingInterceptor.Level.BODY", 
    "level = if (com.example.BuildConfig.DEBUG) HttpLoggingInterceptor.Level.BODY else HttpLoggingInterceptor.Level.NONE"
)

# Add TelemetryInterceptor to client builds
content = content.replace(
    ".addInterceptor(loggingInterceptor)",
    ".addInterceptor(loggingInterceptor)\n            .addInterceptor(TelemetryInterceptor())"
)

with open("app/src/main/java/com/example/data/api/ApiClient.kt", "w") as f:
    f.write(content)
