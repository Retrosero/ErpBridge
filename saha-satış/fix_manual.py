import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    lines = f.readlines()

def debug_lines(start, end):
    for i in range(start-1, end):
        print(f"{i+1}: {lines[i]}", end='')

debug_lines(2145, 2165)
