import re

with open('app/src/main/java/com/example/data/SyncRepository.kt', 'r') as f:
    content = f.read()

content = content.replace("imageLinksJson = null,\n    localImagePathsJson = null", "imageLinks = emptyList(),\n    localImagePaths = emptyList()")
content = content.replace("imageLinksJson = null,\n                            localImagePathsJson = null", "imageLinks = emptyList(),\n                            localImagePaths = emptyList()")

with open('app/src/main/java/com/example/data/SyncRepository.kt', 'w') as f:
    f.write(content)
