import re

with open('app/src/main/java/com/example/ui/screens/CustomersScreen.kt', 'r') as f:
    content = f.read()

# I will find `val filteredCustomers = AppDataStore.customers.filter` and replace it with derived state.
search_pattern = r"(val filteredCustomers = AppDataStore\.customers\.filter \{.*?\}\.sortedWith\(.*?\))"

derived_code = """val filteredCustomers by remember {
            derivedStateOf {
                AppDataStore.customers.filter { customer ->
                    val matchesQuery = searchQuery.isEmpty() ||
                            customer.name.contains(searchQuery, ignoreCase = true) ||
                            customer.id.contains(searchQuery, ignoreCase = true) ||
                            customer.taxNumber?.contains(searchQuery, ignoreCase = true) == true
                    val matchesStatus = selectedFilter == "Tümü" ||
                            (selectedFilter == "Açık Hesap" && customer.balance > 0) ||
                            (selectedFilter == "Alacaklı" && customer.balance < 0) ||
                            (selectedFilter == "Borçsuz" && customer.balance == 0.0)
                    matchesQuery && matchesStatus
                }.sortedWith(
                    if (isSortAscending) compareBy { it.name } else compareByDescending { it.name }
                )
            }
        }"""

content = re.sub(search_pattern, derived_code, content, flags=re.DOTALL)

with open('app/src/main/java/com/example/ui/screens/CustomersScreen.kt', 'w') as f:
    f.write(content)
