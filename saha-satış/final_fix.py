import re
import os

files_to_fix = [
    'app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt'
]

for filepath in files_to_fix:
    with open(filepath, 'r') as f:
        content = f.read()

    # We need to replace imageLinks with imageLinksJson = null and localImagePaths with localImagePathsJson = null
    # But wait, earlier I replaced imageLinksJson with imageLinks.
    content = content.replace("imageLinks = emptyList(),\n    localImagePaths = emptyList()", "imageLinksJson = null,\n    localImagePathsJson = null")
    
    with open(filepath, 'w') as f:
        f.write(content)
