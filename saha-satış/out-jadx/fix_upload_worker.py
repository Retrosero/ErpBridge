import re

with open("app/src/main/java/com/example/util/TelemetryUploadWorker.kt", "r") as f:
    content = f.read()

content = content.replace("com.example.data.api.RetrofitClient", "okhttp3.OkHttpClient")
content = content.replace("val client = RetrofitClient.client", "val client = OkHttpClient()")
content = content.replace("RetrofitClient.BASE_URL", "prefs.getString(\"api_url\", \"https://lisans.appsgo.cloud/\") ?: \"https://lisans.appsgo.cloud/\"")

with open("app/src/main/java/com/example/util/TelemetryUploadWorker.kt", "w") as f:
    f.write(content)
