import re

content = open("app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt").read()

content = content.replace("                val sharedPrefs = context.getSharedPreferences(\"erp_settings\", android.content.Context.MODE_PRIVATE)", "                val sharedPrefsInner = context.getSharedPreferences(\"erp_settings\", android.content.Context.MODE_PRIVATE)")
content = content.replace("val sharedPrefs = context.getSharedPreferences(\"erp_settings\", android.content.Context.MODE_PRIVATE)", "val sharedPrefs = context.getSharedPreferences(\"erp_settings\", android.content.Context.MODE_PRIVATE)")

# That might just replace all of them. Better: replace 'val ' with '' if it's already declared in that scope?
# Actually it's faster to just replace `val sharedPrefs` with `val sharedPrefsXYZ` inside loops, but we need to also rename `tenantId`, `apiKeyVal`, `deviceId` inside loops.
