import os

for root, dirs, files in os.walk("app/src/main/java/com/example"):
    for file in files:
        if file.endswith(".kt"):
            filepath = os.path.join(root, file)
            with open(filepath, "r") as f:
                content = f.read()
            
            # Split into lines and reconstruct
            lines = content.split('\n')
            new_lines = []
            for line in lines:
                if 'throw  ;' in line or 'throw ;' in line:
                    if 'is kotlinx.coroutines.CancellationException' in line:
                        continue # Drop it
                    if 'catch' in line:
                        new_lines.append(line.replace('throw  ;', '').replace('throw ;', ''))
                        continue
                    # Just drop the line entirely if it's mostly "throw ;"
                    if len(line.strip()) <= 10:
                        continue
                new_lines.append(line)
                
            new_content = '\n'.join(new_lines)
            if new_content != content:
                with open(filepath, "w") as f:
                    f.write(new_content)
