import re

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'r') as f:
    lines = f.readlines()

for i, line in enumerate(lines):
    if "imageLinks = emptyList()" in line and "ProductEntity" in "".join(lines[i-15:i+1]):
        lines[i] = line.replace("imageLinks = emptyList()", "imageLinksJson = null")
    if "localImagePaths = emptyList()" in line and "ProductEntity" in "".join(lines[i-16:i+1]):
        lines[i] = line.replace("localImagePaths = emptyList()", "localImagePathsJson = null")

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'w') as f:
    f.writelines(lines)
