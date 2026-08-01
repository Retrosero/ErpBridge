package com.example

import com.example.data.api.PullJobsRequest
import org.junit.Assert.assertEquals
import org.junit.Test

class ExampleUnitTest {

    @Test
    fun addition_isCorrect() {
        assertEquals(4, 2 + 2)
    }

    @Test
    fun pull_request_uses_a_bounded_catalog_page_by_default() {
        val request = PullJobsRequest(
            tenant_id = "test-tenant",
            api_key = "",
            device_id = "test-device",
            agent_version = "test"
        )

        assertEquals(1, request.page)
        assertEquals(200, request.pageSize)
    }
}
