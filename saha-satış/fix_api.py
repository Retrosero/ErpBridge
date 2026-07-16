import re

content = open("app/src/main/java/com/example/data/api/FieldOpsApiService.kt").read()

missing_methods = """
    @POST("api/v1/android/sync/stokHareket")
    suspend fun getStokHareket(@Body request: PullJobsRequest): Response<StokHareketResponse>

    @POST("api/v1/android/sync/cariHareket")
    suspend fun getCariHareket(@Body request: PullJobsRequest): Response<CariHareketResponse>

    @POST("api/v1/android/sync/barkodTanimi")
    suspend fun getBarkodTanimi(@Body request: PullJobsRequest): Response<BarkodTanimiResponseDto>

    @POST("api/v1/android/sync/cariAdresleri")
    suspend fun getCariAdresleri(@Body request: PullJobsRequest): Response<CariAdresResponseDto>

    @POST("api/v1/android/sync/cariBankaHesaplari")
    suspend fun getCariBankaHesaplari(@Body request: PullJobsRequest): Response<CariBankaHesapResponseDto>

    @POST("api/v1/android/sync/trigger")
    suspend fun triggerSync(@Body request: PullJobsRequest): Response<FieldOpsTriggerResponse>

    @POST("api/v1/android/sync/status")
    suspend fun getSyncStatus(@Body request: PullJobsRequest): Response<SyncStatusResponseDto>

    @POST("api/v1/android/sync/pushStatus")
    suspend fun getPushStatus(@Body request: PullJobsRequest): Response<FieldOpsPushStatusResponse>

    @POST("api/v1/android/sync/fiyatListesi")
    suspend fun getFiyatListesi(@Body request: PullJobsRequest): Response<okhttp3.ResponseBody>
"""

content = content.replace("interface FieldOpsApiService {", "interface FieldOpsApiService {" + missing_methods)
open("app/src/main/java/com/example/data/api/FieldOpsApiService.kt", "w").write(content)
