with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    lines = f.readlines()

lines_to_delete = [1985, 2038, 2052, 2066, 2186, 2219, 2252, 2281]

# delete in reverse order to keep line numbers valid
for line_num in sorted(lines_to_delete, reverse=True):
    index = line_num - 1
    if lines[index].strip() == '}':
        del lines[index]
    else:
        print(f"Warning: line {line_num} is not '}}', it is {lines[index].strip()}")

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.writelines(lines)
