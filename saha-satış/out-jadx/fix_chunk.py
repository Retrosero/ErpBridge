import re

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('db.bankDao().insertAll(bankEntities)', 'bankEntities.chunked(100).forEach { db.bankDao().insertAll(it) }')
content = content.replace('db.kasaLogDao().insertAll(kasaEntities)', 'kasaEntities.chunked(100).forEach { db.kasaLogDao().insertAll(it) }')
content = content.replace('db.salesRecordDao().insertAll(salesEntities)', 'salesEntities.chunked(100).forEach { db.salesRecordDao().insertAll(it) }')
content = content.replace('db.productDao().insertAll(productEntities)', 'productEntities.chunked(100).forEach { db.productDao().insertAll(it) }')
content = content.replace('db.customerDao().insertAll(customerEntities)', 'customerEntities.chunked(100).forEach { db.customerDao().insertAll(it) }')

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'w', encoding='utf-8') as f:
    f.write(content)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('db.productDao().insertAll(productEntities)', 'productEntities.chunked(100).forEach { db.productDao().insertAll(it) }')
content = content.replace('db.wmsOrderItemDao().insertAll(orderItemsList)', 'orderItemsList.chunked(100).forEach { db.wmsOrderItemDao().insertAll(it) }')

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(content)
