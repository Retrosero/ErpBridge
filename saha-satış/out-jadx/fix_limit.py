import re
with open('app/src/main/java/com/example/data/api/FieldOpsApiService.kt', 'r', encoding='utf-8') as f:
    content = f.read()

new_props = """
    val page: Int? = 1,
    val pageSize: Int? = 1000,
    @com.squareup.moshi.Json(name = "PageSize") val pageSizeCap: Int? = 1000,
    @com.squareup.moshi.Json(name = "page_size") val page_size_param: Int? = 1000,
    @com.squareup.moshi.Json(name = "limit") val limit_param: Int? = 1000,
    @com.squareup.moshi.Json(name = "Limit") val limitCap: Int? = 1000,
    @com.squareup.moshi.Json(name = "take") val take_param: Int? = 1000,
    @com.squareup.moshi.Json(name = "size") val size_param: Int? = 1000,
    @com.squareup.moshi.Json(name = "Size") val sizeCap: Int? = 1000
"""

content = re.sub(
    r'val page: Int\? = 1,.*val Size: Int\? = 1000',
    new_props.strip(),
    content,
    flags=re.DOTALL
)

with open('app/src/main/java/com/example/data/api/FieldOpsApiService.kt', 'w', encoding='utf-8') as f:
    f.write(content)
