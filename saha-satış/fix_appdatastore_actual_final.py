import re

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'r') as f:
    content = f.read()

# Replace any lingering imageLinks = emptyList() when inside ProductEntity mapping
content = content.replace("imageLinks = emptyList(),\n    localImagePaths = emptyList()", "imageLinksJson = null,\n    localImagePathsJson = null")
content = content.replace("imageLinks = emptyList(),\n                            localImagePaths = emptyList()", "imageLinksJson = null,\n                            localImagePathsJson = null")


with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'w') as f:
    f.write(content)
