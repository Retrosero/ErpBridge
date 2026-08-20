import re

with open("app/src/main/java/com/example/util/TelemetryUploadWorker.kt", "r") as f:
    content = f.read()

content = content.replace(
    ".url(prefs.getString(\"api_url\", \"https://lisans.appsgo.cloud/\") ?: \"https://lisans.appsgo.cloud/\" + \"api/v1/mobile/telemetry/batch\")",
    """
                        .url(
                            (prefs.getString("api_url", "https://lisans.appsgo.cloud/") ?: "https://lisans.appsgo.cloud/").let { 
                                val b = if(it.endsWith("/")) it else "$it/"
                                b + "api/v1/mobile/telemetry/batch"
                            }
                        )"""
)

with open("app/src/main/java/com/example/util/TelemetryUploadWorker.kt", "w") as f:
    f.write(content)
