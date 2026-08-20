with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    lines = f.read().split('\n')

# First, strip the blindly added `                }` from the previous script
# Wait, the previous script added it to EVERY line that had ONLY spaces.
# So if a line ends with `                }`, and the rest is just spaces, we strip it!
cleaned_lines = []
for line in lines:
    if line.endswith('                }'):
        prefix = line[:-17]
        if prefix.strip() == '':
            cleaned_lines.append(prefix)
            continue
    cleaned_lines.append(line)

def get_indent(s):
    return len(s) - len(s.lstrip())

# Now, we do smart restoration
restored = []
for i, line in enumerate(cleaned_lines):
    if line.strip() == '':
        spaces = len(line)
        # If the line is pure whitespace, it might be a deleted brace.
        # The deleted brace was EXACTLY `                }` (16 spaces and `}`).
        # This means if we restore it, the line's indentation will be spaces + 16.
        # Let's check the next non-empty line.
        next_indent = 0
        for j in range(i+1, len(cleaned_lines)):
            if cleaned_lines[j].strip() != '':
                next_indent = get_indent(cleaned_lines[j])
                # If the next line is a closing brace, its indent is what matters
                break
        
        # If we insert `}`, the brace will be at indentation `spaces + 16`.
        # This means the block inside had indentation `spaces + 20`.
        # The block outside has indentation `spaces + 16`.
        # Therefore, the NEXT line should have indentation <= spaces + 16.
        # If the next line has indentation >= spaces + 20, then the block is STILL CONTINUING!
        # So we SHOULD NOT insert a `}` here.
        # However, what if the next line is exactly `spaces + 16`? Then it's a sibling statement. So we SHOULD insert `}`.
        # What if it is < `spaces + 16`? Then the outer block also closed, so we SHOULD insert `}`.
        # SO: We restore `}` IF AND ONLY IF next_indent <= spaces + 16.
        # EXCEPT: What if the empty line was just a blank line at indentation `spaces + 16`?
        # If it was a blank line inside the block, the next line would be `spaces + 20`. We won't insert. Correct.
        # If it was a blank line BETWEEN blocks, the next line would be `spaces + 16`. We WOULD insert, which is WRONG!
        # Ah. If it was a blank line between blocks, `next_indent` == `spaces + 16`.
        pass
