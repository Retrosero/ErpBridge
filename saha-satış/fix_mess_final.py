import re
import os

for root, dirs, files in os.walk("app/src/main/java/com/example"):
    for file in files:
        if file.endswith(".kt"):
            filepath = os.path.join(root, file)
            with open(filepath, "r") as f:
                content = f.read()
            
            # Remove the injected code block
            # The block looks like:
            # catch (VAR: Exception) {
            #             com.example.util.TelemetryReporter.reportException(VAR, "CatchBlock_in_...")
            #             if (VAR is kotlinx.coroutines.CancellationException) throw VAR
            
            # Using regex to remove this mess
            new_content = re.sub(
                r'com\.example\.util\.TelemetryReporter\.reportException\([a-zA-Z0-9_]+,\s*"CatchBlock_in_[^"]*"\)\s*if\s*\([a-zA-Z0-9_]+\s*is\s*kotlinx\.coroutines\.CancellationException\)\s*throw\s*[a-zA-Z0-9_]*\s*',
                '',
                content
            )
            
            # Sometimes it has a semicolon? No.
            # Let's just catch the precise pattern
            new_content = re.sub(
                r'com\.example\.util\.TelemetryReporter\.reportException\([^\)]+\)\n\s*if\s*\([^)]+\)\s*throw\s*[^\n]+\n',
                '',
                new_content
            )
            
            # What if `throw ` is left? Let's check for `throw \n` or `throw  \n`
            new_content = re.sub(
                r'if\s*\(\s*is\s*kotlinx\.coroutines\.CancellationException\)\s*throw\s*\n',
                '',
                new_content
            )
            
            new_content = re.sub(
                r'com\.example\.util\.TelemetryReporter\.reportException\([^)]+\)\n\s*if\s*\(\s*is\s*kotlinx\.coroutines\.CancellationException\)\s*throw\s*\n',
                '',
                new_content
            )

            if new_content != content:
                with open(filepath, "w") as f:
                    f.write(new_content)
