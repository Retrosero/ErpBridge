import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the broken mapping
bad_loop = """                                    com.example.data.database.Transaction(
                                        id = dto.id,
                                        date = formattedDate,
                                        desc = dto.aciklama ?: dto.evrakNo ?: "Hareket",
                                        amount = amt,
                                        type = tType,
                                        vade = formattedDate // fallback to date as no vadeTarihi
                                    )"""

good_loop = """                                    com.example.ui.screens.CustomerTx(
                                        id = dto.id,
                                        date = formattedDate,
                                        type = tType,
                                        amount = amt,
                                        description = dto.aciklama ?: dto.evrakNo ?: "Hareket",
                                        erpRef = dto.erpRef,
                                        recNo = dto.evrakNo,
                                        cha_recno = dto.cha_recno ?: dto.recno ?: dto.chaRecNo ?: dto.cha_RECno
                                    )"""

content = content.replace(bad_loop, good_loop)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w', encoding='utf-8') as f:
    f.write(content)
