import re

with open('app/src/main/java/com/example/ui/screens/CustomersScreen.kt', 'r') as f:
    content = f.read()

old_list = """val filteredPurchasedList = purchasedList.filter { (record, product) ->
                            val title = product?.title ?: record.productBarcode
                            val code = product?.code ?: ""
                            title.contains(productSearchQuery, ignoreCase = true) || code.contains(productSearchQuery, ignoreCase = true)
                        }"""

new_list = """val filteredPurchasedList = remember(purchasedList, productSearchQuery) {
                            purchasedList.filter { (record, product) ->
                                val title = product?.title ?: record.productBarcode
                                val code = product?.code ?: ""
                                title.contains(productSearchQuery, ignoreCase = true) || code.contains(productSearchQuery, ignoreCase = true)
                            }
                        }"""

content = content.replace(old_list, new_list)

content = content.replace("items(filteredPurchasedList) { (record, product) ->", "items(items = filteredPurchasedList, key = { (record, _) -> record.id }) { (record, product) ->")

with open('app/src/main/java/com/example/ui/screens/CustomersScreen.kt', 'w') as f:
    f.write(content)
