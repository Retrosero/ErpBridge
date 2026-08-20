import re

with open("app/src/main/java/com/example/NavApp.kt", "r") as f:
    content = f.read()

nav_listener = """
    LaunchedEffect(navController) {
        navController.addOnDestinationChangedListener { _, destination, _ ->
            com.example.util.TelemetryReporter.setCurrentScreen(destination.route ?: "unknown")
            com.example.util.TelemetryReporter.addBreadcrumb("Navigation", "Navigated to ${destination.route}")
        }
    }
"""

if "addOnDestinationChangedListener" not in content:
    content = content.replace("val scope = rememberCoroutineScope()", "val scope = rememberCoroutineScope()\n" + nav_listener)

with open("app/src/main/java/com/example/NavApp.kt", "w") as f:
    f.write(content)
