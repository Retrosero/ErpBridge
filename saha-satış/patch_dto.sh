sed -i '/\/\/ DTOs/a \
@androidx.annotation.Keep\
data class ActivationRequest(\
    val code: String,\
    val installationId: String,\
    val deviceName: String,\
    val appVersion: String\
)\
\
@androidx.annotation.Keep\
data class ActivationResponse(\
    val token: String,\
    val tenantId: String,\
    val deviceId: String,\
    val expiresAtUtc: String\
)' app/src/main/java/com/example/data/api/FieldOpsApiService.kt
