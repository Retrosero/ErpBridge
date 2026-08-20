import re
import os

files_to_fix = [
    'app/src/main/java/com/example/data/SyncRepository.kt',
    'app/src/main/java/com/example/ui/screens/CatalogScreen.kt',
    'app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt',
    'app/src/main/java/com/example/ui/screens/AppDataStore.kt'
]

for filepath in files_to_fix:
    with open(filepath, 'r') as f:
        content = f.read()
    
    # We replace "cartonQuantity = prod.cartonQuantity\n                    )" 
    # Or "cartonQuantity = product.cartonQuantity\n                )"
    # With a generic regex replacement
    
    # Matches: cartonQuantity = <something> \n ) 
    # capturing <something>
    content = re.sub(
        r'(cartonQuantity\s*=\s*)([^,\n\)]+)(\s*\n\s*\))',
        r'\1\2,\n    imageLinksJson = null,\n    localImagePathsJson = null\3',
        content
    )
    
    with open(filepath, 'w') as f:
        f.write(content)
