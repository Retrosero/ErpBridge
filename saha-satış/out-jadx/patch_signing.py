import re
with open("app/build.gradle.kts", "r") as f:
    content = f.read()

old_signing = """    create("release") {
      val keystorePath = System.getenv("KEYSTORE_PATH") ?: "${rootDir}/my-upload-key.jks"
      storeFile = file(keystorePath)
      storePassword = System.getenv("STORE_PASSWORD")
      keyAlias = "upload"
      keyPassword = System.getenv("KEY_PASSWORD")
    }"""
    
new_signing = """    create("release") {
      val keystorePath = System.getenv("KEYSTORE_PATH") ?: "${rootDir}/my-upload-key.jks"
      storeFile = file(keystorePath)
      storePassword = System.getenv("STORE_PASSWORD") ?: System.getenv("KEY_PASSWORD") ?: "android"
      keyAlias = System.getenv("KEY_ALIAS") ?: "upload"
      keyPassword = System.getenv("KEY_PASSWORD") ?: System.getenv("STORE_PASSWORD") ?: "android"
    }"""

if old_signing in content:
    content = content.replace(old_signing, new_signing)
    with open("app/build.gradle.kts", "w") as f:
        f.write(content)
    print("Fixed signing")
else:
    print("Not found")
