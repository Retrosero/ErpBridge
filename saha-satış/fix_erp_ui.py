import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r', encoding='utf-8') as f:
    content = f.read()

target_logs_row = """                                            Text("İşlem Günlüğü", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                            IconButton("""

replacement_logs_row = """                                            Text("İşlem Günlüğü", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold)
                                            Row {
                                                if (!isSyncAllRunning && isSyncAllFinished) {
                                                    IconButton(onClick = { SyncManager.resetSyncState() }, modifier = Modifier.size(24.dp)) {
                                                        Icon(Icons.Filled.Close, contentDescription = "Kapat", modifier = Modifier.size(16.dp))
                                                    }
                                                    Spacer(modifier = Modifier.width(16.dp))
                                                }
                                                IconButton("""

content = content.replace(target_logs_row, replacement_logs_row)

target_add_list = """                                }
                            }
                            
                            if (isSyncAllRunning || isSyncAllFinished) {"""

replacement_add_list = """                                }
                            }
                            
                            if (!(isSyncAllRunning || isSyncAllFinished)) {
                                Text(
                                    "Bireysel Tablo Senkronizasyonu",
                                    style = MaterialTheme.typography.titleSmall,
                                    fontWeight = FontWeight.Bold,
                                    color = MaterialTheme.colorScheme.onBackground
                                )
                                LazyColumn(
                                    modifier = Modifier.fillMaxWidth().weight(1f),
                                    verticalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    items(syncTasks) { task ->
                                        Card(
                                            modifier = Modifier.fillMaxWidth(),
                                            colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)),
                                            shape = RoundedCornerShape(8.dp)
                                        ) {
                                            Row(
                                                modifier = Modifier.fillMaxWidth().padding(12.dp),
                                                horizontalArrangement = Arrangement.SpaceBetween,
                                                verticalAlignment = Alignment.CenterVertically
                                            ) {
                                                Column(modifier = Modifier.weight(1f)) {
                                                    Text(task.name, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
                                                    Text(task.description, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                                }
                                                Spacer(modifier = Modifier.width(8.dp))
                                                Button(
                                                    onClick = {
                                                        SyncManager.startSyncAll(context.applicationContext, apiUrl, apiKey, listOf(task))
                                                    },
                                                    enabled = !isSyncAllRunning,
                                                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary),
                                                    contentPadding = androidx.compose.foundation.layout.PaddingValues(horizontal = 12.dp, vertical = 6.dp)
                                                ) {
                                                    Text("İndir", style = MaterialTheme.typography.labelMedium)
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            
                            if (isSyncAllRunning || isSyncAllFinished) {"""

content = content.replace(target_add_list, replacement_add_list)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w', encoding='utf-8') as f:
    f.write(content)
