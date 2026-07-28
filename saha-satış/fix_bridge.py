import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    content = f.read()

content = content.replace("cartonQuantity = prod.cartonQuantity\\n                    )", "cartonQuantity = prod.cartonQuantity,\\n                            imageLinksJson = converter.fromBarcodeList(prod.imageLinks),\\n                            localImagePathsJson = converter.fromBarcodeList(prod.localImagePaths)\\n                    )")

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(content)

