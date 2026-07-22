with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    lines = f.read().split('\n')

def get_indent(line):
    return len(line) - len(line.lstrip())

new_lines = []
for i in range(len(lines)):
    line = lines[i]
    if line.strip() == '':
        spaces = len(line)
        # sed removed exactly 16 spaces and }
        # so if the line length is N, the original line had N + 17 characters (N + 16 spaces + })
        # This means the original `}` was at indentation N + 16.
        # But wait! The `s/                }//g` matches anywhere.
        # If the original line was `                }` (16 spaces), then spaces=0.
        # If original was `                    }` (20 spaces), spaces=4.
        # If original was `                        }` (24 spaces), spaces=8.
        
        # Let's check the next non-empty line's indentation
        next_indent = 0
        for j in range(i+1, len(lines)):
            if lines[j].strip() != '':
                next_indent = get_indent(lines[j])
                break
                
        # If we insert the missing brace, its indentation will be spaces + 16.
        # In a well-formatted file, a closing brace at indentation X means the block inside was at X + 4,
        # and the next line should be at indentation <= X.
        # So we should insert it IF next_indent <= spaces + 16.
        # What if next_indent > spaces + 16? Then the block inside hasn't finished yet, so this was just a blank line.
        
        if next_indent <= spaces + 16:
            new_lines.append(line + "                }")
        else:
            new_lines.append(line)
    else:
        new_lines.append(line)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write('\n'.join(new_lines))
