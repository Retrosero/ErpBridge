import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Add import
if 'import androidx.compose.foundation.lazy.items' not in content:
    content = content.replace('import androidx.compose.foundation.lazy.LazyColumn', 'import androidx.compose.foundation.lazy.LazyColumn\nimport androidx.compose.foundation.lazy.items')
    
# Fix bracket mismatch
open_braces = content.count('{')
close_braces = content.count('}')
print(f"Open: {open_braces}, Close: {close_braces}")

if close_braces < open_braces:
    content += '}' * (open_braces - close_braces)
elif close_braces > open_braces:
    # Too many closing braces? This shouldn't happen unless my replace did it
    pass

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w', encoding='utf-8') as f:
    f.write(content)
