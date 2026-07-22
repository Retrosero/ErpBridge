import json

log_file = '/.aistudio/artifacts/brain/3a87d648-e2ae-4e73-a93f-8b70d102863b/.system_generated/logs/transcript.jsonl'
target_file = 'app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt'

content_found = None

with open(log_file, 'r') as f:
    for line in f:
        try:
            data = json.loads(line)
            # Find tool responses for view_file
            if 'tool_responses' in data:
                for resp in data['tool_responses']:
                    if 'call:default_api:view_file' in str(resp):
                         # Unfortunately the log format is complex, let's just search strings.
                         pass
        except Exception:
            pass

# Easier approach: just search the raw text of the jsonl for a unique string from ErpIntegrationScreen.kt
# then extract it.
