import re

files_to_fix = [
    'app/src/main/java/com/example/ui/screens/AppDataStore.kt',
    'app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt',
    'app/src/main/java/com/example/ui/screens/CatalogScreen.kt',
    'app/src/main/java/com/example/data/SyncRepository.kt'
]

for filepath in files_to_fix:
    with open(filepath, 'r') as f:
        content = f.read()

    # ProductEntity constructor is called but has invalid args (imageLinks instead of imageLinksJson)
    content = content.replace("imageLinks = emptyList(),\n    localImagePaths = emptyList()", "imageLinksJson = null,\n    localImagePathsJson = null")
    content = content.replace("imageLinks = emptyList(),\n                            localImagePaths = emptyList()", "imageLinksJson = null,\n                            localImagePathsJson = null")
    
    with open(filepath, 'w') as f:
        f.write(content)

