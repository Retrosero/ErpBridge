import os
import re

for root, dirs, files in os.walk("app/src/main/java/com/example"):
    for file in files:
        if file.endswith(".kt"):
            filepath = os.path.join(root, file)
            with open(filepath, "r") as f:
                content = f.read()
            
            new_content = re.sub(r'catch\s*\(\s*([a-zA-Z0-9_]+)\s*:\s*Exception\s*\)\s*\{\s*throw\s+\1;', r'catch (\1: Exception) {', content)
            
            # Also just replace `throw e;` if it's on a line alone
            lines = new_content.split('\n')
            final_lines = []
            for line in lines:
                if line.strip() == 'throw e;':
                    continue
                if line.strip() == '} catch (e: Exception) {  throw e;':
                    final_lines.append(line.replace('  throw e;', ''))
                    continue
                if 'throw e; rawDate }' in line:
                    final_lines.append(line.replace('throw e; ', ''))
                    continue
                
                final_lines.append(line)
            
            final_content = '\n'.join(final_lines)
            
            if final_content != content:
                with open(filepath, "w") as f:
                    f.write(final_content)

