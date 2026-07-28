import re

with open('app/src/main/java/com/example/util/DataSyncHelper.kt', 'r') as f:
    content = f.read()

content = content.replace("sb.append(\"$barcodeValue,$code,$title,$cat,$basePrice,$dealerPrice,$wholesalePrice,$kdv,$boxQty,$pkgQty,\\n\")", "sb.append(\"$barcodeValue,$code,$title,$cat,$basePrice,$dealerPrice,$wholesalePrice,$kdv,$boxQty,$pkgQty, , , , , , , , , , \\n\")")

with open('app/src/main/java/com/example/util/DataSyncHelper.kt', 'w') as f:
    f.write(content)
