import os

for filepath in [
    'app/src/main/java/com/example/ui/screens/AppDataStore.kt',
    'app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt',
    'app/src/main/java/com/example/ui/screens/CatalogScreen.kt',
    'app/src/main/java/com/example/data/SyncRepository.kt'
]:
    with open(filepath, 'r') as f:
        content = f.read()

    # Let's fix the ProductCatalog instantiation manually where imageLinksJson was added incorrectly
    content = content.replace("cartonQuantity = prod.cartonQuantity,\n    imageLinksJson = null,\n    localImagePathsJson = null", "cartonQuantity = prod.cartonQuantity,\n    imageLinks = emptyList(),\n    localImagePaths = emptyList()")
    
    # In BridgeSyncHelper:
    content = content.replace("cartonQuantity = u.actualKoliAdet,\n    imageLinksJson = null,\n    localImagePathsJson = null", "cartonQuantity = u.actualKoliAdet")

    content = content.replace("cartonQuantity = it.cartonQuantity,\n    imageLinksJson = null,\n    localImagePathsJson = null", "cartonQuantity = it.cartonQuantity")

    with open(filepath, 'w') as f:
        f.write(content)
