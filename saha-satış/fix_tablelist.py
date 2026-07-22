import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    code = f.read()

# I will find "val tableList = listOf(" and replace it with a clean version.
# Then I will find where tableList ends and chop everything out!
