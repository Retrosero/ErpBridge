import re

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'r') as f:
    content = f.read()

target1 = """                AppDataStore.bridgeBankalar.clear()
                AppDataStore.bridgeBankalar.addAll(loadedItems)
                AppDataStore.persist(context)"""

rep1 = """                AppDataStore.bridgeBankalar.clear()
                AppDataStore.bridgeBankalar.addAll(loadedItems)
                AppDataStore.mapBridgeDataToAppModels()
                AppDataStore.persist(context)"""

content = content.replace(target1, rep1)

target2 = """                AppDataStore.bridgeKasalar.clear()
                AppDataStore.bridgeKasalar.addAll(loadedItems)
                AppDataStore.persist(context)"""

rep2 = """                AppDataStore.bridgeKasalar.clear()
                AppDataStore.bridgeKasalar.addAll(loadedItems)
                AppDataStore.mapBridgeDataToAppModels()
                AppDataStore.persist(context)"""

content = content.replace(target2, rep2)

with open('app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt', 'w') as f:
    f.write(content)
