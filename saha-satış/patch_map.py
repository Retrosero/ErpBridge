import re

with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'r') as f:
    content = f.read()

func = """
    fun mapBridgeDataToAppModels() {
        if (bridgeBankalar.isNotEmpty()) {
            val mappedBanks = bridgeBankalar.mapNotNull { b ->
                val id = b.id ?: b.kod ?: b.erpRef
                val namePart = b.isim ?: b.bankaAd ?: ""
                val name = if (b.kod != null && namePart.isNotEmpty()) "${b.kod} - $namePart" else (namePart.takeIf { it.isNotEmpty() } ?: b.kod ?: id ?: "Bilinmeyen Banka")
                val accountNo = b.hesapNumarasi ?: b.iBANKodu ?: ""
                if (id.isNullOrBlank() || name.isBlank()) null
                else Bank(id = id, name = name, accountNo = accountNo, iban = b.iBANKodu ?: "", balance = 0.0)
            }.distinctBy { it.id }
            if (mappedBanks.isNotEmpty()) {
                banks.clear()
                banks.addAll(mappedBanks)
            }
        }
        
        if (bridgeKasalar.isNotEmpty()) {
            val mappedCash = bridgeKasalar.mapNotNull { k ->
                val id = k.id ?: k.kod ?: k.erpRef
                val namePart = k.isim ?: ""
                val name = if (k.kod != null && namePart.isNotEmpty()) "${k.kod} - $namePart" else (namePart.takeIf { it.isNotEmpty() } ?: k.kod ?: id ?: "Bilinmeyen Kasa")
                val currency = if (k.dovizCinsi == null || k.dovizCinsi == 0) "TRY" else k.dovizCinsi.toString()
                if (id.isNullOrBlank() || name.isBlank()) null
                else CashAccount(id = id, name = name, currency = currency, balance = 0.0)
            }.distinctBy { it.id }
            if (mappedCash.isNotEmpty()) {
                cashAccounts.clear()
                cashAccounts.addAll(mappedCash)
            }
        }
    }
"""

target = "fun deserializeKasaYonetimList(json: String): List<KasaYonetimDto> {\n        if (json.isBlank()) return emptyList()\n        val type = Types.newParameterizedType(List::class.java, KasaYonetimDto::class.java)\n        return moshiStore.adapter<List<KasaYonetimDto>>(type).fromJson(json) ?: emptyList()\n    }"

if target in content:
    content = content.replace(target, target + "\n" + func)
    with open('app/src/main/java/com/example/ui/screens/AppDataStore.kt', 'w') as f:
        f.write(content)
    print("Patched!")
else:
    print("Target not found!")
