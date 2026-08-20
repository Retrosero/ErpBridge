stack = [24, 28, 32]
indent = 28
while len(stack) > 0 and indent <= stack[-1]:
    if False and indent == stack[-1]: break
    opener_indent = stack.pop()
    print("popped", opener_indent)
