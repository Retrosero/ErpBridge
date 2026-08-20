import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the fallback block
content = re.sub(
    r'\} else \{\s*log\("Güncellenecek standart fiyat listesi verisi bulunamadı \(veya sunucu desteklemiyor\)\."\)\s*log\("Sistem alternatif gelişmiş \'fiyatListesi\' metodunu deniyor\.\.\."\)\s*syncFiyatListesiNew\(context, apiUrl, apiKey, log, updateProgress\)\s*\}',
    r'} else {\n                log("Güncellenecek standart fiyat listesi verisi bulunamadı (veya sunucu desteklemiyor).")\n            }',
    content
)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(content)
