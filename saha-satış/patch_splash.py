import re

with open('app/src/main/java/com/example/ui/screens/SplashScreen.kt', 'r') as f:
    content = f.read()

target = """if (activeUser != null) {
                navController.navigate("dashboard") {
                    popUpTo("splash") { inclusive = true }
                }
            } else {
                navController.navigate("license") {
                    popUpTo("splash") { inclusive = true }
                }
            }"""

replacement = """val secPrefs = context.getSharedPreferences("secure_license_prefs", android.content.Context.MODE_PRIVATE)
            val isLicenseValid = secPrefs.getBoolean("is_license_valid", true)
            val hasApiKey = secPrefs.getString("api_key", null) != null
            
            if (activeUser != null && isLicenseValid && hasApiKey) {
                navController.navigate("dashboard") {
                    popUpTo("splash") { inclusive = true }
                }
            } else {
                navController.navigate("license") {
                    popUpTo("splash") { inclusive = true }
                }
            }"""

content = content.replace(target, replacement)

with open('app/src/main/java/com/example/ui/screens/SplashScreen.kt', 'w') as f:
    f.write(content)
