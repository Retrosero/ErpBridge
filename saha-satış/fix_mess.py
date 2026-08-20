import os
import re

for root, dirs, files in os.walk("app/src/main/java/com/example"):
    for file in files:
        if file.endswith(".kt"):
            filepath = os.path.join(root, file)
            with open(filepath, "r") as f:
                content = f.read()
            
            # Remove any leftover "throw  ;" or "throw ;" or "if (  is kotlinx.coroutines.CancellationException) throw  ;"
            new_content = re.sub(r'if\s*\(\s*is\s*kotlinx\.coroutines\.CancellationException\)\s*throw\s*;', '', content)
            new_content = new_content.replace('throw  ;', '')
            new_content = new_content.replace('throw ;', '')
            
            if new_content != content:
                with open(filepath, "w") as f:
                    f.write(new_content)
