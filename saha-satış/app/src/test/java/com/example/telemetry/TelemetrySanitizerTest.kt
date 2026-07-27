package com.example.telemetry

import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class TelemetrySanitizerTest {
    @Test
    fun secretsAndPersonalDataAreRemoved() {
        val input = "Authorization: Bearer secret.jwt token=abc AK-1234567890ABCDEF test@example.com 05551234567"
        val sanitized = TelemetrySanitizer.clean(input).orEmpty()

        assertFalse(sanitized.contains("secret.jwt"))
        assertFalse(sanitized.contains("1234567890ABCDEF"))
        assertFalse(sanitized.contains("test@example.com"))
        assertFalse(sanitized.contains("05551234567"))
        assertTrue(sanitized.contains("[REDACTED]"))
    }

    @Test
    fun routeDropsQueryAndNormalizesIdentifiers() {
        val route = TelemetrySanitizer.route(
            "https://lisans.appsgo.cloud/api/v1/items/4314eaba-593e-4420-8647-022fd601ceb9?apiKey=secret"
        )
        assertTrue(route?.endsWith("/api/v1/items/{id}") == true)
        assertFalse(route.orEmpty().contains("secret"))
    }
}
