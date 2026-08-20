with open('app/src/main/java/com/example/ui/screens/CustomersScreen.kt', 'r') as f:
    content = f.read()

content = content.replace("key = { (record, _) -> record.id }", "key = { (record, _) -> record.date + \"_\" + record.productBarcode }")

with open('app/src/main/java/com/example/ui/screens/CustomersScreen.kt', 'w') as f:
    f.write(content)
