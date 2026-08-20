import re

with open('app/src/main/java/com/example/ui/screens/WarehouseScreen.kt', 'r') as f:
    content = f.read()

content = content.replace("items(filteredOrders) { order ->", "items(items = filteredOrders, key = { order -> order.id }) { order ->")
content = content.replace("items(filteredItems) { item ->", "items(items = filteredItems, key = { item -> item.id }) { item ->")
content = content.replace("items(activeOrderItems) { item ->", "items(items = activeOrderItems, key = { item -> item.id }) { item ->")

with open('app/src/main/java/com/example/ui/screens/WarehouseScreen.kt', 'w') as f:
    f.write(content)
