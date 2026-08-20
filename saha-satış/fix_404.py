import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    content = f.read()

pattern = r'(\s*)\}\s*else\s*\{\s*handleApiError\(response, log\)\s*throw Exception\((.*?)\)\s*\}'

def replacer(match):
    indent = match.group(1)
    msg = match.group(2)
    return f'{indent}}} else {{{indent}    handleApiError(response, log){indent}    if (response.code() == 404) {{{indent}        log("Uç nokta bulunamadı (404). Senkronizasyon atlanıyor."){indent}        hasMore = false{indent}    }} else {{{indent}        throw Exception({msg}){indent}    }}{indent}}}'

new_content = re.sub(pattern, replacer, content)

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "w") as f:
    f.write(new_content)
