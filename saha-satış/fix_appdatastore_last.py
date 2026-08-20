import re

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'r') as f:
    content = f.read()

# I am completely overriding lines 1240 to 1260 that had the leftover issue with mapping
content = content.replace("cartonQuantity = prod.cartonQuantity,\n    imageLinksJson = null,\n    localImagePathsJson = null\n                            )", "cartonQuantity = prod.cartonQuantity,\n                                        imageLinks = emptyList(),\n                                        localImagePaths = emptyList()\n                                    )")

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'w') as f:
    f.write(content)
