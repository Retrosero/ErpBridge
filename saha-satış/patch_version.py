with open("app/build.gradle.kts", "r") as f:
    content = f.read()

content = content.replace('versionCode = 357', 'versionCode = 358')
content = content.replace('versionName = "357.0"', 'versionName = "358.0"')

with open("app/build.gradle.kts", "w") as f:
    f.write(content)

print("Version updated")
