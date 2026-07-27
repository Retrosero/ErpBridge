package com.example.telemetry

object TelemetrySanitizer {
    private val patterns = listOf(
        Regex("""(?i)bearer\s+[A-Za-z0-9._~+/=-]+"""),
        Regex("""(?i)(authorization|token|api[-_ ]?key|password|parola|cookie|pwd)\s*[:=]\s*[^\s,;]+"""),
        Regex("""(?i)\bAK-[A-Za-z0-9-]{12,}\b"""),
        Regex("""(?i)(server|host|database|user\s*id|uid|password|pwd)\s*=\s*[^;]+"""),
        Regex("""\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b""", RegexOption.IGNORE_CASE),
        Regex("""(?<!\d)(?:\+?90)?0?5\d{9}(?!\d)""")
    )

    fun clean(value: String?, maxLength: Int = 8_000): String? {
        if (value.isNullOrBlank()) return null
        var result: String = value
        patterns.forEach { result = it.replace(result, "[REDACTED]") }
        return result.take(maxLength)
    }

    fun route(url: String?): String? = url
        ?.substringBefore('?')
        ?.replace(Regex("""/[0-9a-fA-F]{8}-[0-9a-fA-F-]{27,}"""), "/{id}")
        ?.take(300)
}
