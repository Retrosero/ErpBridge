# -*- coding: utf-8 -*-
import sys
path = r'C:\Users\retro\Documents\GitHub\ErpBridge\saha-satış\app\src\main\java\com\example\ui\screens\BridgeSyncHelper.kt'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

old = '''        /* log removed */
        return Exception(userFriendlyMessage)




    }
    suspend fun syncCariler('''

new = '''        /* log removed */
        return Exception(userFriendlyMessage)




    }

    /**
     * Merkezi API bu entity i\u00e7in endpoint sunmuyorsa (404) veya API anahtar\u0131n\u0131n
     * bu u\u00e7 noktaya eri\u015fim yetkisi yoksa (403) sync fonksiyonu bilgilendirici
     * log d\u00fc\u015ferek ba\u015far\u0131yla d\u00f6ns\u00fcn. T\u00fcm sync zincirini k\u0131rmas\u0131n; sadece o tablo
     * bo\u015f kals\u0131n. UI taraf\u0131nda "bu \u00f6zellik tenant'ta yok" \u015feklinde g\u00f6sterilir.
     */
    private fun isUnsupportedEndpoint(
        response: retrofit2.Response<*>,
        entity: String,
        log: (String) -> Unit
    ): Boolean {
        val code = response.code()
        if (code == 404) {
            log("\u26a0\u2019\u2018$entity\u2019 endpoint\u2019i merkezi API\u2019de mevcut de\u011fil (HTTP 404). Bu tablo i\u00e7in sync atlan\u0131yor.")
            return true
        }
        if (code == 403) {
            log("\u26a0\u2019\u2018$entity\u2019 endpoint\u2019ine bu API anahtar\u0131yla eri\u015fim yok (HTTP 403). Bu tablo i\u00e7in sync atlan\u0131yor.")
            return true
        }
        return false
    }

    suspend fun syncCariler('''

count = text.count(old)
if count != 1:
    print(f'ERROR: expected 1 match, found {count}')
    sys.exit(1)

text = text.replace(old, new, 1)
with open(path, 'w', encoding='utf-8') as f:
    f.write(text)
print('OK: patch applied')
