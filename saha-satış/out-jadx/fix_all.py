import re

with open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt", "r") as f:
    code = f.read()

def fix_pagination(func_body, item_var, id_field, table_name):
    # Fix PullJobsRequest pageSize field, ensure no duplicate fields
    # It already only has `page` and `pageSize` in Kotlin data class, but maybe they were instantiated differently.
    # Actually, `page = currentPage, pageSize = 1000` is already used. We just ensure `pageSize = pageSize`
    
    # Fix fingerprint
    old_fp_pattern = r'val currentFingerprint = ' + item_var + r'\.joinToString\(\",\"\) \{ it\.hashCode\(\)\.toString\(\) \}'
    new_fp = f'val currentFingerprint = {item_var}.joinToString(",") {{ ({id_field}).toString() }}'
    func_body = re.sub(old_fp_pattern, new_fp, func_body)

    # Replace the exit condition
    # Old logic: if (cariler.isEmpty() || (currentFingerprint == lastFingerprint && lastFingerprint.isNotEmpty())) { ... }
    # Let's replace the whole block carefully.
    
    # We can use regex to find the block:
    # val currentFingerprint = ...
    # if (...\.isEmpty\(\) || .*? == lastFingerprint.*?\) \{
    #     .*?
    #     hasMore = false
    # \} else \{
    
    # It's better to replace the `hasMore = false` else branch size check.
    # We find:
    # if (cariler.size < pageSize || ((syncRes.total ?: 0) > 0 && totalFetched >= (syncRes.total ?: 0))) {
    #     hasMore = false
    # } else {
    #     currentPage++
    # }
    
    old_size_check_pattern = r'if \(' + item_var + r'\.size < pageSize \|\| \(\(.*?total \?: 0\) > 0 && totalFetched >= \(.*?total \?: 0\)\)\) \{\s*hasMore = false\s*\} else \{\s*currentPage\+\+\s*\}'
    new_size_check = f'if ({item_var}.size < pageSize) {{\n                            hasMore = false\n                        }} else {{\n                            currentPage++\n                        }}'
    
    func_body = re.sub(old_size_check_pattern, new_size_check, func_body)
    
    # Also update the repeating page log:
    func_body = re.sub(r'log\("Tekrarlayan sayfa algılandı, sayfalama durduruluyor\."\)', f'log("{table_name} tekrarlayan sayfa algılandı, senkronizasyon tamamlandı.")', func_body)
    
    return func_body

def replace_db_write(func_body, mem_list, map_val, app_store_list, entity_class, mapper_code, dao_method):
    # Find the block where it does AppDataStore.persist(context)
    # Usually:
    # if (allMappedCustomers.isNotEmpty()) {
    # ...
    # AppDataStore.persist(context)
    # }
    
    block_pattern = r'if \(' + mem_list + r'\.isNotEmpty\(\)\) \{.*?AppDataStore\.persist\(context\)\s*\}'
    
    new_block = f"""if ({mem_list}.isNotEmpty()) {{
                val currentList = com.example.ui.screens.AppDataStore.{app_store_list}.toList()
                val itemMap = currentList.associateBy {{ it.id }}.toMutableMap()
                for (mapped in {mem_list}) {{
                    itemMap[mapped.id] = mapped
                }}
                val mergedList = itemMap.values.toList()
                
                val db = com.example.data.database.DatabaseProvider.getDatabase(context)
                val converter = com.example.data.database.Converters()
                val entities = {mem_list}.map {{ item ->
                    {mapper_code}
                }}
                
                androidx.room.withTransaction(db) {{
                    db.{dao_method}().deleteAll()
                    entities.chunked(100).forEach {{ db.{dao_method}().insertAll(it) }}
                }}
                
                kotlinx.coroutines.withContext(kotlinx.coroutines.Dispatchers.Main) {{
                    com.example.ui.screens.AppDataStore.{app_store_list}.clear()
                    com.example.ui.screens.AppDataStore.{app_store_list}.addAll(mergedList)
                }}
            }}"""
            
    func_body = re.sub(block_pattern, new_block, func_body, flags=re.DOTALL)
    return func_body

# I'll just write Kotlin replacements manually if python gets too messy.
