import re
with open('app/src/main/java/com/example/util/DataSyncHelper.kt', 'r') as f:
    content = f.read()

# Make generateProductCsvTemplate support 10 image urls headers
content = content.replace("barcode,code,title,category,basePrice,dealerPrice,wholesalePrice,kdvPercent,boxQty,packageQty,imageUrl\\n", "barcode,code,title,category,basePrice,dealerPrice,wholesalePrice,kdvPercent,boxQty,packageQty,imageUrl1,imageUrl2,imageUrl3,imageUrl4,imageUrl5,imageUrl6,imageUrl7,imageUrl8,imageUrl9,imageUrl10\\n")

with open('app/src/main/java/com/example/util/DataSyncHelper.kt', 'w') as f:
    f.write(content)

