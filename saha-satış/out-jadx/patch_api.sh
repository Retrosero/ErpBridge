sed -i '/@POST("api\/v1\/android\/sync\/satisSartlari")/i \
    @POST("api/v1/mobile/activate")\
    suspend fun activateDevice(@Body request: ActivationRequest): Response<ActivationResponse>\
\
    @POST("api/v1/mobile/migrate")\
    suspend fun migrateDevice(@Body request: ActivationRequest): Response<ActivationResponse>\
\
    @POST("api/v1/mobile/renew")\
    suspend fun renewDeviceToken(): Response<ActivationResponse>\
' app/src/main/java/com/example/data/api/FieldOpsApiService.kt
