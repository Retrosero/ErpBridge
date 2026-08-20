sed -i '/if (com.example.data.LicenseRepository.getApiKey(this) != null) {/a \
                kotlinx.coroutines.GlobalScope.launch(kotlinx.coroutines.Dispatchers.IO) {\
                    try {\
                        val pInfo = packageManager.getPackageInfo(packageName, 0)\
                        val appVersion = pInfo.versionName ?: "1.0.0"\
                        com.example.data.LicenseRepository.checkAndMigrateIfNecessary(this@MainActivity, appVersion)\
                    } catch (e: Exception) {}\
                }\
' app/src/main/java/com/example/MainActivity.kt
