sed -i '/kasaYonetimList.addAll(deserializeKasaYonetimList(kasaYonetimListStr))/a \
                    mapBridgeDataToAppModels()' app/src/main/java/com/example/ui/screens/AppDataStore.kt
