#!/bin/bash
FILE="app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt"

# We will look for:
# } else {
#     handleApiError(response, log)
#     throw Exception("API Hatası

# And we will do a targeted sed replacement.
awk '
{
    if ($0 ~ /handleApiError\(response, log\)/) {
        print $0
        getline next_line
        if (next_line ~ /throw Exception\(\"API Hatası/) {
            print "                    if (response.code() == 404) {"
            print "                        log(\"Uç nokta bulunamadı (404). Senkronizasyon atlanıyor.\")"
            print "                        hasMore = false"
            print "                    } else {"
            print next_line
            print "                    }"
        } else {
            print next_line
        }
    } else {
        print $0
    }
}
' $FILE > temp.kt
mv temp.kt $FILE
