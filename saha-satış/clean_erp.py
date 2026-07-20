import sys

def remove_blocks(file_path):
    with open(file_path, 'r') as f:
        content = f.read()

    # Find all occurrences of `else if (selectedErp == "GOAPP ERP") {`
    # or `if (selectedErp == "GOAPP ERP") {`
    
    search_strs = [
        'if (selectedErp == "GOAPP ERP") {',
        '} else if (selectedErp == "GOAPP ERP") {'
    ]
    
    for search_str in search_strs:
        while True:
            idx = content.find(search_str)
            if idx == -1:
                break
                
            # Find the matching closing brace
            start_brace = content.find('{', idx)
            brace_count = 1
            end_brace = start_brace + 1
            
            while brace_count > 0 and end_brace < len(content):
                if content[end_brace] == '{':
                    brace_count += 1
                elif content[end_brace] == '}':
                    brace_count -= 1
                end_brace += 1
                
            if brace_count == 0:
                # Replace the block with an empty block, but if it was an `else if`, we need to preserve syntax
                if "else if" in search_str:
                    content = content[:idx] + " } else if (false) { " + content[end_brace:]
                else:
                    content = content[:idx] + " if (false) { " + content[end_brace:]
            else:
                break # Should not happen

    with open(file_path, 'w') as f:
        f.write(content)

remove_blocks('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt')

