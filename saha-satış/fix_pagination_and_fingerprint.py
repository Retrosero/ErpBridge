import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

content = re.sub(
    r'(var hasMore\w* = true)',
    r'\1\n            var lastFingerprint = ""',
    content
)

def replacer(match):
    val_name = match.group(1)
    expr = match.group(2)
    has_more_var = match.group(3)
    return f"""val {val_name} = {expr}
                    val currentFingerprint = {val_name}.joinToString(",") {{ it.hashCode().toString() }}
                    if ({val_name}.isEmpty() || (currentFingerprint == lastFingerprint && lastFingerprint.isNotEmpty())) {{
                        if (currentFingerprint == lastFingerprint && lastFingerprint.isNotEmpty()) log("Tekrarlayan sayfa algılandı, sayfalama durduruluyor.")
                        {has_more_var} = false
                    }} else {{
                        lastFingerprint = currentFingerprint"""

content = re.sub(
    r'val (\w+) = (.*?)\s+if \(\1\.isEmpty\(\)\) \{\s+(\w+) = false\s+\}\ else \{',
    replacer,
    content
)

# For the end of the loops
content = re.sub(
    r'if \((\w+)\.isEmpty\(\) \|\| \(\(syncRes\.total \?: 0\) > 0 && (.*?) >= \(\w+\.total \?: 0\)\)\) \{',
    r'if (\1.size < pageSize || ((syncRes.total ?: 0) > 0 && \2 >= (syncRes.total ?: 0))) {',
    content
)

# Replace items.size < pageSize that we added previously just to be clean
content = re.sub(
    r'if \(items\.size < pageSize\) \{\s+hasMore\w* = false\s+\}\ else \{\s+(\w+)\+\+\s+\}',
    r'if (items.size < pageSize) {\n                            hasMore = false\n                        } else {\n                            \1++\n                        }',
    content
)


with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(content)
