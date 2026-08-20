sed -i '/import androidx.compose.ui.Modifier/i import kotlinx.coroutines.launch\nimport kotlinx.coroutines.GlobalScope\nimport kotlinx.coroutines.Dispatchers' app/src/main/java/com/example/MainActivity.kt
sed -i 's/onManualEntry = {/onSimulateScan = {/g' app/src/main/java/com/example/ui/screens/LicenseScreen.kt
