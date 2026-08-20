import re

with open('app/src/main/java/com/example/data/api/ApiClient.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Remove host check in ApiKeyInterceptor
content = re.sub(
    r'if \(!request\.url\.host\.endsWith\("lisans\.appsgo\.cloud"\)\) \{\s*return chain\.proceed\(request\)\s*\}',
    '',
    content
)

# Update retrofit function signature and baseUrl usage
content = re.sub(
    r'private fun retrofit\(tenantId: String\?, apiKey: String\): Retrofit \{',
    r'private fun retrofit(baseUrl: String, tenantId: String?, apiKey: String): Retrofit {',
    content
)

# Clean baseUrl to have exactly one trailing slash
content = re.sub(
    r'\.baseUrl\(CENTRAL_API_URL\)',
    r'.baseUrl(if (baseUrl.endsWith("/")) baseUrl else "$baseUrl/")',
    content
)

# Update usages of retrofit() to pass baseUrl
content = re.sub(
    r'retrofit\(tenantId, apiKey\)\.create\(',
    r'retrofit(baseUrl, tenantId, apiKey).create(',
    content
)

# Also remove the @Suppress("UNUSED_PARAMETER") for baseUrl
content = re.sub(
    r'@Suppress\("UNUSED_PARAMETER"\)\s*baseUrl: String,',
    r'baseUrl: String,',
    content
)

with open('app/src/main/java/com/example/data/api/ApiClient.kt', 'w', encoding='utf-8') as f:
    f.write(content)
