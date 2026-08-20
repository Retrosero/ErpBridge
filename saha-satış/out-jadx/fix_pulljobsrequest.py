import re
with open('app/src/main/java/com/example/data/api/FieldOpsApiService.kt', 'r', encoding='utf-8') as f:
    content = f.read()

new_props = """
    val page: Int? = 1,
    val pageSize: Int? = 1000
"""

content = re.sub(
    r'val page: Int\? = 1,.*val sizeCap: Int\? = 1000',
    new_props.strip(),
    content,
    flags=re.DOTALL
)

with open('app/src/main/java/com/example/data/api/FieldOpsApiService.kt', 'w', encoding='utf-8') as f:
    f.write(content)
