import re

with open("app/src/main/java/com/example/NavApp.kt", "r") as f:
    content = f.read()

nav_listener = """
    val navController = rememberNavController()
    androidx.compose.runtime.LaunchedEffect(navController) {
        navController.addOnDestinationChangedListener { _, destination, _ ->
            com.example.util.TelemetryReporter.setCurrentScreen(destination.route ?: "unknown")
            com.example.util.TelemetryReporter.addBreadcrumb("Navigation", "Navigated to ${destination.route}")
        }
    }
"""

content = content.replace("val navController = rememberNavController()", nav_listener)

with open("app/src/main/java/com/example/NavApp.kt", "w") as f:
    f.write(content)
