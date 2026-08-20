import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    content = f.read()

pattern = r'handleApiError\(response, log\)\s*if \(response\.code\(\) == 404\) \{\s*log\("Uç nokta bulunamadı \(404\)\. Senkronizasyon atlanıyor\."\)\s*hasMore = false\s*\} else \{\s*throw Exception\("API Hatası.*?"\)\s*\}'

def replacer(match):
    return 'val err = handleApiError(response, log)\n                    if (response.code() == 404) {\n                        log("Uç nokta bulunamadı (404). Senkronizasyon atlanıyor.")\n                        hasMore = false\n                    } else {\n                        throw err\n                    }'

new_content = re.sub(pattern, replacer, content, flags=re.DOTALL)

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
    f.write(new_content)
