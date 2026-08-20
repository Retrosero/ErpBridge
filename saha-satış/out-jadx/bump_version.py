import re

with open('app/build.gradle.kts', 'r') as f:
    content = f.read()

content = re.sub(r'versionCode = 353', 'versionCode = 354', content)
content = re.sub(r'versionName = "353.0"', 'versionName = "354.0"', content)

with open('app/build.gradle.kts', 'w') as f:
    f.write(content)
