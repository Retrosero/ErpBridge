package com.example.data.api

import retrofit2.http.Body
import retrofit2.http.POST

typealias CariResponse = FieldOpsSyncResponse<CariDto>
typealias UrunResponse = FieldOpsSyncResponse<UrunDto>
typealias LicenseStatusResponse = LicenseStatusDto
typealias PullResponse = PullJobsResponse
typealias PushResponse = PushJobsResponse

data class SyncRequest(
    @com.squareup.moshi.Json(name = "tenant_id") val tenantId: String,
    @com.squareup.moshi.Json(name = "api_key") val apiKey: String,
    @com.squareup.moshi.Json(name = "device_id") val deviceId: String,
    @com.squareup.moshi.Json(name = "agent_version") val agentVersion: String
)

data class PushRequest(
    @com.squareup.moshi.Json(name = "tenant_id") val tenantId: String,
    @com.squareup.moshi.Json(name = "api_key") val apiKey: String,
    @com.squareup.moshi.Json(name = "device_id") val deviceId: String,
    @com.squareup.moshi.Json(name = "agent_version") val agentVersion: String,
    @com.squareup.moshi.Json(name = "data") val data: Map<String, Any?>
)

interface ErpBridgeApi {

    @POST("api/v1/android/bootstrap")
    suspend fun bootstrap(@Body request: SyncRequest): BootstrapResponse

    @POST("api/v1/android/sync/cari")
    suspend fun syncCari(@Body request: SyncRequest): CariResponse

    @POST("api/v1/android/sync/urun")
    suspend fun syncUrun(@Body request: SyncRequest): UrunResponse

    @POST("api/v1/android/sync/stokSeviye")
    suspend fun syncStokSeviye(@Body request: SyncRequest): StokSeviyeResponse

    @POST("api/v1/android/sync/cariHareketleri")
    suspend fun syncCariHareketleri(@Body request: SyncRequest): CariHareketResponse

    @POST("api/v1/android/sync/faturaHareket")
    suspend fun syncFaturaHareket(@Body request: SyncRequest): FaturaHareketResponse

    @POST("api/v1/android/license/status")
    suspend fun licenseStatus(@Body request: SyncRequest): LicenseStatusResponse

    @POST("api/v1/android/pull")
    suspend fun pull(@Body request: SyncRequest): PullResponse

    @POST("api/v1/android/push")
    suspend fun push(@Body request: PushRequest): PushResponse
}
