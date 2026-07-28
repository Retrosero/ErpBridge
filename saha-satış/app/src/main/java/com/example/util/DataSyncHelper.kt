package com.example.util

import android.content.Context
import android.net.Uri
import android.os.Environment
import android.provider.OpenableColumns
import android.util.Log
import androidx.compose.ui.graphics.Color
import com.example.ui.screens.Customer
import com.example.ui.screens.ProductCatalog
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.BufferedReader
import java.io.File
import java.io.FileOutputStream
import java.io.InputStream
import java.io.InputStreamReader
import java.net.URL
import java.util.UUID

object DataSyncHelper {

    fun generateCustomerCsvTemplate(): String {
        return "id,name,contact,phone,address,taxOffice,taxNumber,gpsLocation,riskLimit,balance\n" +
                "CUST-${UUID.randomUUID().toString().take(6).uppercase()},Örnek Market A.Ş.,Ali Yılmaz,0532 123 45 67,Örnek Mah. No:1 İstanbul,Kadıköy,1234567890,40.9, 29.0,50000.0,0.0"
    }

    fun generateRealCustomersCsv(): String {
        val companyPrefix = listOf(
            "Özdemir", "Yılmazlar", "Has Bahçe", "Mavi", "Can", "Ege", "Akdeniz", "Yıldız", "Aras", "Kardeşler",
            "Baran", "Doruk", "Elit", "Mega", "Umut", "Başak", "Güçlü", "Doğan", "Zirve", "Arda",
            "Tekin", "Şahinler", "Karaca", "Aksoy", "Kaya", "Birlik", "Merkez", "Özgür", "Köşe", "Park"
        )
        val companyTypes = listOf(
            "Gıda Ticaret", "Süpermarket Zinciri", "Şarküteri ve Şarkütericilik", "Yapı Market", "Ecza Deposu",
            "Unlu Mamülleri", "Otelcilik ve Turizm", "Kırtasiye Ltd. Şti.", "Aydınlatma Sanayi", "Kasap Entegre",
            "Teknoloji Elektronik", "İnşaat Malzemeleri", "Tekstil ve Giyim", "Lojistik Hizmetleri", "Otomotiv Ticaret"
        )
        val contacts = listOf(
            "Ahmet Öztürk", "Mustafa Demir", "Mehmet Kaya", "Fatma Çelik", "Ayşe Yıldız", "Hüseyin Şahin",
            "Ali Yılmaz", "Zeynep Koç", "Ender Aslan", "Murat Bulut", "Seda Kılıç", "Hakan Özer", "Sibel Doğan"
        )
        val taxOffices = listOf("Beyoğlu", "Kadıköy", "Şişli", "Üsküdar", "Bornova", "Çankaya", "Osmangazi", "Nilüfer", "Karatay")
        
        val sb = java.lang.StringBuilder()
        sb.append("id,name,contact,phone,address,taxOffice,taxNumber,gpsLocation,riskLimit,balance\n")
        
        for (i in 1..50) {
            val pref = companyPrefix[i % companyPrefix.size]
            val type = companyTypes[(i * 3 + 2) % companyTypes.size]
            val suffix = if (i % 2 == 0) "A.Ş." else "Ltd. Şti."
            val name = "$pref $type $suffix"
            
            val contact = contacts[(i * i + 1) % contacts.size]
            val phone = "0532 ${100 + i} ${40 + i % 60} ${i * i % 100}"
            val address = "${pref} Caddesi No:${i * 3}, ${taxOffices[i % taxOffices.size]}"
            val taxOffice = taxOffices[i % taxOffices.size]
            val taxNum = (1000000000 + i * 1234567).toString().take(10)
            val gps = "41.${i % 100},29.${(i * 2) % 100}"
            val riskLimit = 10000.0 * ((i % 5) + 1) * 3
            val balance = 1500.0 * (i % 10)
            
            sb.append("CUST-${1000 + i},$name,$contact,$phone,$address,$taxOffice,$taxNum,$gps,$riskLimit,$balance\n")
        }
        return sb.toString()
    }

    fun generateProductCsvTemplate(): String {
        return "barcode,code,title,category,basePrice,dealerPrice,wholesalePrice,kdvPercent,boxQty,packageQty,imageUrl1,imageUrl2,imageUrl3,imageUrl4,imageUrl5,imageUrl6,imageUrl7,imageUrl8,imageUrl9,imageUrl10\n" +
                "869000000001,PRD-001,Tek Resimli Gıda Ürünü,Temel Gıda,150.0,140.0,130.0,1,12,1,https://images.unsplash.com/photo-1542838132-92c53300491e\n" +
                "869000000002,PRD-002,Çok Resimli Ayakkabı (10 Farklı Resim),Giyim,499.9,450.0,420.0,20,30,1,\"https://images.unsplash.com/photo-1542291026-7eec264c27ff;https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a;https://images.unsplash.com/photo-1523275335684-37898b6baf30;https://images.unsplash.com/photo-1572635196237-14b3f281503f;https://images.unsplash.com/photo-1505740420928-5e560c06d30e;https://images.unsplash.com/photo-1560343090-f0409e92791a;https://images.unsplash.com/photo-1581655353564-df123a1eb820;https://images.unsplash.com/photo-1525966222134-fcfa99b8ae77;https://images.unsplash.com/photo-1491553895911-0055eca6402d;https://images.unsplash.com/photo-1549298916-b41d501d3772\""
    }

    fun generateRealProductsCsv(): String {
        val categories = listOf(
            "Temel Gıda", "Sıvı Yağlar", "Atıştırmalık & Bisküvi", "Süt & Kahvaltılık", "Gazlı & Gazsız İçecekler",
            "Kozmetik & Bakım", "Ev & Ev Temizlik", "Bebek & Çocuk", "Çay & Kahve", "Unlu Mamüller"
        )
        
        val categoryBrands = mapOf(
            "Temel Gıda" to listOf("Ankara Makarnası", "Yudum", "Duru", "Tat", "Tukaş", "Filiz", "Reis"),
            "Sıvı Yağlar" to listOf("Yudum Ayçiçek", "Komili Sızma", "Kristal Riviera", "Biryağ"),
            "Atıştırmalık & Bisküvi" to listOf("Ülker Albeni", "Eti Negro", "Ülker Çikolatalı Gofret", "Eti Burçak", "Lays Baharat", "Doritos Taco", "Eti Cici Bebe"),
            "Süt & Kahvaltılık" to listOf("Sütaş Süzme Peynir", "İçim Kaşar", "Torku Süzme", "Yörsan Yoğurt", "Pınar Süzme Süt"),
            "Gazlı & Gazsız İçecekler" to listOf("Coca-Cola Şişe", "Fanta Portakal", "Uludağ Gazoz", "Sırma Soda", "Pepsi Max", "Cappy Şeftali"),
            "Kozmetik & Bakım" to listOf("Nivea Roll-On", "Arko Nem Krem", "Duru Duş Jeli", "Sensodyne Diş Macunu", "Head & Shoulders Şampuan"),
            "Ev & Ev Temizlik" to listOf("Ariel Matik Dağ Esintisi", "Domestos Çamaşır Suyu", "Fairy Elde Yıkama", "Pril Limonlu Hijyen", "Vernel Yumuşatıcı"),
            "Bebek & Çocuk" to listOf("Prima Premium", "Dalin Şampuan", "Milupa Aptamil", "Bebelac Gold"),
            "Çay & Kahve" to listOf("Çaykur Tiryaki Çay", "Lipton Yellow Label", "Nescafe Gold", "Kurukahveci Mehmet Efendi Türk Kahvesi"),
            "Unlu Mamüller" to listOf("Uno Tost Ekmeği", "Eti Ekmek Kadayıfı", "Dr. Oetker Kabartma Tozu")
        )

        val productDetails = mapOf(
            "Ankara Makarnası" to listOf("Burgu 500g", "Fiyonk 500g", "Kalem 500g", "Spagetti 500g"),
            "Yudum" to listOf("Mısırözü Yağı 1L", "Ayçiçek Yağı 2L", "Sızma Zeytinyağı 500ml"),
            "Duru" to listOf("Pilavlık Bulgur 1Kg", "Kırmızı Mercimek 1Kg", "Baldo Pirinç 1Kg", "Nohut 1Kg"),
            "Tat" to listOf("Domates Salçası 830g", "Ketçap Acılı 400g", "Mayonez Sıkma 350g"),
            "Tukaş" to listOf("Biber Salçası 700g", "Bezelye Konservesi 400g"),
            "Reis" to listOf("Yeşil Mercimek 1Kg", "Kuru Fasulye 1Kg"),
            "Yudum Ayçiçek" to listOf("Yağı 1L", "Yağı 2L", "Yağı 5L Teneke"),
            "Komili Sızma" to listOf("Zeytinyağı 1L", "Zeytinyağı 2L"),
            "Kristal Riviera" to listOf("Zeytinyağı 1L", "Zeytinyağı 5L"),
            "Biryağ" to listOf("Ayçiçek Yağı 1L", "Ayçiçek Yağı 5L"),
            "Ülker Albeni" to listOf("Atıştırmalık 40g", "Karma Paket 5'li", "Karamel Kek"),
            "Eti Negro" to listOf("Kakaolu Bisküvi 110g", "Mini 50g", "Aile Boyu 3'lü"),
            "Ülker Çikolatalı Gofret" to listOf("Klasik 36g", "Fındıklı 36g", "Mini 100g Bag"),
            "Eti Burçak" to listOf("Yulaflı Bisküvi 131g", "Sütlü Çikolatalı 114g"),
            "Lays Baharat" to listOf("Klasik Parti Şekli", "Süper Boy 150g"),
            "Doritos Taco" to listOf("Süper Boy 150g", "Mega Paket"),
            "Eti Cici Bebe" to listOf("Bebek Bisküvi 125g", "Bisküvi 500g Aile", "Bisküvi 1Kg"),
            "Sütaş Süzme Peynir" to listOf("Peynir 500g", "Peynir 1Kg", "Mavi Dilimli 250g"),
            "İçim Kaşar" to listOf("Kaşar Peyniri 300g", "Peyniri 500g", "Tostluk 700g"),
            "Torku Süzme" to listOf("Süzme Peynir 450g", "Yoğurt 1Kg"),
            "Yörsan Yoğurt" to listOf("Kaymaklı 1.5Kg", "Süzme Yoğurt 1Kg"),
            "Pınar Süzme Süt" to listOf("Yarım Yağlı 1L", "Tam Yağlı Süt 1L"),
            "Coca-Cola Şişe" to listOf("Klasik 250ml", "Ortaboy 1L", "Pet 1.5L", "Kutu 330ml"),
            "Fanta Portakal" to listOf("Kutu 330ml", "Soda Pet 1L", "Portakal 1.5L"),
            "Uludağ Gazoz" to listOf("Kutu 330ml", "Pet 1L", "Klasik Cam Şişe"),
            "Sırma Soda" to listOf("Doğal Maden Suyu 6'lı", "Limonlu Maden Suyu 6'lı", "Elmalı Soda"),
            "Pepsi Max" to listOf("Kutu 330ml", "Şekersiz 1L", "Gazlı İçecek 1.5L"),
            "Cappy Şeftali" to listOf("Meyve Suyu 200ml", "Meyve Suyu 1L"),
            "Nivea Roll-On" to listOf("Invisible ErkekDeo", "Dry Comfort KadınDeo", "Silver Protect Sıkma"),
            "Arko Nem Krem" to listOf("Klasik Tüp Krem 20ml", "Hızlı Emilen Krem 250ml", "Zeytinyağlı Kavanoz"),
            "Duru Duş Jeli" to listOf("Okyanus Esintisi 500ml", "Avokado Yağlı 500ml"),
            "Sensodyne Diş Macunu" to listOf("Hızlı Rahatlama 75ml", "Çok Yönlü Koruma 75ml"),
            "Head & Shoulders Şampuan" to listOf("Mentol Ferahlığı 350ml", "Klasik Bakım 400ml", "Dökülme Karşıtı 400ml"),
            "Ariel Matik Dağ Esintisi" to listOf("Toz Deterjan 1.5Kg", "Toz Deterjan 5Kg", "Sıvı Deterjan 20 Yıkama"),
            "Domestos Çamaşır Suyu" to listOf("Kar Beyazlığı 750ml", "Okaliptus Ferahlığı 750ml", "Dağ Esintisi 1.25L"),
            "Fairy Elde Yıkama" to listOf("Limon 650ml", "Platin Tablet 24'lü", "Platin Tablet 48'li"),
            "Pril Limonlu Hijyen" to listOf("Klasik 675ml", "Elma Ferahlığı 675ml"),
            "Vernel Yumuşatıcı" to listOf("Gülün Gizemi 1.2L", "Lavanta Esintisi 1.5L"),
            "Prima Premium" to listOf("Bebek Bezi No.3 40'lı", "Bebek Bezi No.4 36'lı", "Bebek Bezi No.5 30'lu"),
            "Dalin Şampuan" to listOf("Klasik Şampuan 200ml", "Şampuan Klasik 500ml", "Kolay Tarama Spreyi"),
            "Milupa Aptamil" to listOf("Mama No.1 350g", "Mama No.2 350g", "Çocuk Devam Sütü 800g"),
            "Bebelac Gold" to listOf("Mama No.1 500g", "Mama No.3 Devam Sütü"),
            "Çaykur Tiryaki Çay" to listOf("Siyah Çay 500g", "Siyah Çay 1Kg", "Siyah Rize Çayı 1Kg"),
            "Lipton Yellow Label" to listOf("Dökme Çay 500g", "Bardak Poşet 25'li", "Demlik Poşet 100'lü"),
            "Nescafe Gold" to listOf("Eko Paket 100g", "Eko Paket 200g", "Cam Kavanoz 150g"),
            "Kurukahveci Mehmet Efendi Türk Kahvesi" to listOf("Paket 100g", "Metal Kutu 250g", "Paket 100g 5'li Set"),
            "Uno Tost Ekmeği" to listOf("Klasik Büyük 670g", "Tam Buğdaylı Tost Ekmeği", "Çavdarlı Ekmeği 400g"),
            "Dr. Oetker Kabartma Tozu" to listOf("10'lu Paket", "Şekerli Vanilin 10'lu", "Puding Çikolatalı")
        )

        val sb = java.lang.StringBuilder()
        sb.append("barcode,code,title,category,basePrice,dealerPrice,wholesalePrice,kdvPercent,boxQty,packageQty,imageUrl1,imageUrl2,imageUrl3,imageUrl4,imageUrl5,imageUrl6,imageUrl7,imageUrl8,imageUrl9,imageUrl10\n")

        var count = 0
        val targetSize = 300
        
        while (count < targetSize) {
            val cat = categories[count % categories.size]
            val brands = categoryBrands[cat] ?: continue
            val brand = brands[(count / categories.size) % brands.size]
            val details = productDetails[brand] ?: listOf("Özel Seri")
            val detail = details[(count / (categories.size * brands.size)) % details.size]
            
            val title = "$brand $detail"
            val barcodeValue = 869000000000L + count + 100
            val code = "PRD-${2000 + count}"
            
            val basePrice = 25.0 + (count % 40) * 12.5 + (count % 3) * 5.0
            val dealerPrice = basePrice * 0.90
            val wholesalePrice = basePrice * 0.85
            
            val kdv = when (cat) {
                "Temel Gıda", "Sıvı Yağlar", "Süt & Kahvaltılık", "Unlu Mamüller" -> 1
                "Gazlı & Gazsız İçecekler", "Çay & Kahve" -> 10
                "Kozmetik & Bakım", "Ev & Ev Temizlik", "Bebek & Çocuk" -> 20
                else -> 20
            }
            
            val boxQty = when {
                count % 4 == 0 -> 24
                count % 3 == 0 -> 12
                count % 2 == 0 -> 6
                else -> 12
            }
            val pkgQty = 1
            
            sb.append("$barcodeValue,$code,$title,$cat,$basePrice,$dealerPrice,$wholesalePrice,$kdv,$boxQty,$pkgQty, , , , , , , , , , \n")
            count++
        }
        
        return sb.toString()
    }

    suspend fun saveCsvTemplateToDownloads(context: Context, fileName: String, content: String): Boolean {
        return withContext(Dispatchers.IO) {
            try {
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.Q) {
                    val resolver = context.contentResolver
                    val contentValues = android.content.ContentValues().apply {
                        put(android.provider.MediaStore.MediaColumns.DISPLAY_NAME, fileName)
                        put(android.provider.MediaStore.MediaColumns.MIME_TYPE, "text/csv")
                        put(android.provider.MediaStore.MediaColumns.RELATIVE_PATH, Environment.DIRECTORY_DOWNLOADS)
                        put(android.provider.MediaStore.MediaColumns.IS_PENDING, 1)
                    }
                    val uri = resolver.insert(android.provider.MediaStore.Downloads.EXTERNAL_CONTENT_URI, contentValues)
                    if (uri != null) {
                        resolver.openOutputStream(uri).use { outStream ->
                            outStream?.write(content.toByteArray(Charsets.UTF_8))
                        }
                        contentValues.clear()
                        contentValues.put(android.provider.MediaStore.MediaColumns.IS_PENDING, 0)
                        resolver.update(uri, contentValues, null, null)
                        true
                    } else {
                        false
                    }
                } else {
                    val downloadsDir = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS)
                    if (!downloadsDir.exists()) {
                        downloadsDir.mkdirs()
                    }
                    val file = File(downloadsDir, fileName)
                    FileOutputStream(file).use {
                        it.write(content.toByteArray(Charsets.UTF_8))
                    }
                    true
                }
            } catch (e: Exception) {
                Log.e("DataSync", "Error saving CSV via MediaStore", e)
                try {
                    val fallbackFile = File(context.getExternalFilesDir(Environment.DIRECTORY_DOWNLOADS), fileName)
                    FileOutputStream(fallbackFile).use {
                        it.write(content.toByteArray(Charsets.UTF_8))
                    }
                    true
                } catch (ex: Exception) {
                    Log.e("DataSync", "Error saving CSV to fallback", ex)
                    false
                }
            }
        }
    }

    suspend fun parseCustomerCsv(context: Context, uri: Uri): List<Customer> {
        return withContext(Dispatchers.IO) {
            val list = mutableListOf<Customer>()
            try {
                val inputStream: InputStream? = context.contentResolver.openInputStream(uri)
                val reader = BufferedReader(InputStreamReader(inputStream, Charsets.UTF_8))
                
                // Read header
                val header = reader.readLine()
                var line = reader.readLine()
                
                while (line != null) {
                    val tokens = parseCsvLineRobustly(line)
                    if (tokens.size >= 10) {
                        try {
                            val cust = Customer(
                                id = tokens[0].trim(),
                                name = tokens[1].trim(),
                                balance = tokens[9].trim().toDoubleOrNull() ?: 0.0,
                                lastVisit = "",
                                contact = tokens[2].trim(),
                                phone = tokens[3].trim(),
                                address = tokens[4].trim(),
                                taxOffice = tokens[5].trim(),
                                taxNumber = tokens[6].trim(),
                                gpsLocation = tokens[7].trim(),
                                riskLimit = tokens[8].trim().toDoubleOrNull() ?: 0.0,
                                priceGroup = "1",
                                specialDiscountPercent = 0.0,
                                transactions = androidx.compose.runtime.mutableStateListOf()
                            )
                            list.add(cust)
                        } catch (e: Exception) {
                            Log.e("DataSync", "Error parsing customer row", e)
                        }
                    }
                    line = reader.readLine()
                }
                reader.close()
            } catch (e: Exception) {
                Log.e("DataSync", "Error parsing customer CSV", e)
            }
            list
        }
    }

    suspend fun parseProductCsv(context: Context, uri: Uri): List<ProductCatalog> {
        return withContext(Dispatchers.IO) {
            val list = mutableListOf<ProductCatalog>()
            try {
                val inputStream: InputStream? = context.contentResolver.openInputStream(uri)
                val reader = BufferedReader(InputStreamReader(inputStream, Charsets.UTF_8))
                
                // Read header
                var headerLine = reader.readLine()
                if (headerLine != null && headerLine.startsWith("\uFEFF")) {
                    headerLine = headerLine.substring(1)
                }
                
                var line = reader.readLine()
                while (line != null) {
                    val tokens = parseCsvLineRobustly(line)
                    if (tokens.size >= 10) {
                        try {
                            val imgUrlList = mutableListOf<String>()
                            if (tokens.size > 10) {
                                for (i in 10 until tokens.size) {
                                    val token = tokens[i].trim()
                                    if (token.isNotBlank() && (token.startsWith("http", ignoreCase = true) || token.contains("/") || token.contains("."))) {
                                        imgUrlList.add(token)
                                    }
                                }
                            }
                            val imgUrl = if (imgUrlList.isNotEmpty()) imgUrlList.joinToString(",") else null
                            
                            val prod = ProductCatalog(
                                barcode = tokens[0].trim(),
                                code = tokens[1].trim(),
                                title = tokens[2].trim(),
                                category = tokens[3].trim(),
                                desc = "",
                                basePrice = if (tokens[4].trim().isEmpty()) -1.0 else (tokens[4].trim().toDoubleOrNull() ?: 0.0),
                                dealerPrice = if (tokens[5].trim().isEmpty()) -1.0 else (tokens[5].trim().toDoubleOrNull() ?: 0.0),
                                wholesalePrice = if (tokens[6].trim().isEmpty()) -1.0 else (tokens[6].trim().toDoubleOrNull() ?: 0.0),
                                kdvPercent = if (tokens[7].trim().isEmpty()) -1 else (tokens[7].trim().toIntOrNull() ?: 20),
                                imageUrlColor = Color(0xFF607D8B),
                                stockByWarehouse = emptyMap(),
                                boxQty = tokens[8].trim().toIntOrNull(),
                                packageQty = tokens[9].trim().toIntOrNull(),
                                imageUrl = imgUrl
                            )
                            list.add(prod)
                        } catch (e: Exception) {
                            Log.e("DataSync", "Error parsing product row: $line", e)
                        }
                    }
                    line = reader.readLine()
                }
                reader.close()
            } catch (e: Exception) {
                Log.e("DataSync", "Error parsing product CSV", e)
            }
            list
        }
    }

    suspend fun downloadImageToLocal(context: Context, urlString: String, fileName: String): String? {
        return withContext(Dispatchers.IO) {
            try {
                val url = URL(urlString)
                val connection = url.openConnection()
                connection.connectTimeout = 5000
                connection.readTimeout = 5000
                val inputStream = connection.getInputStream()
                
                val imagesDir = File(context.filesDir, "product_images")
                if (!imagesDir.exists()) {
                    imagesDir.mkdirs()
                }
                
                val file = File(imagesDir, fileName)
                val outputStream = FileOutputStream(file)
                
                inputStream.copyTo(outputStream)
                
                inputStream.close()
                outputStream.close()
                
                file.absolutePath
            } catch (e: Exception) {
                Log.e("DataSync", "Error downloading image: $urlString", e)
                null
            }
        }
    }

    fun parseCsvLineRobustly(line: String): List<String> {
        val result = mutableListOf<String>()
        val delimiter = if (line.count { it == ';' } > line.count { it == ',' }) ';' else ','
        val currentToken = StringBuilder()
        var inQuotes = false
        var i = 0
        while (i < line.length) {
            val c = line[i]
            if (c == '"') {
                inQuotes = !inQuotes
            } else if (c == delimiter && !inQuotes) {
                result.add(cleanCsvToken(currentToken.toString()))
                currentToken.setLength(0)
            } else {
                currentToken.append(c)
            }
            i++
        }
        result.add(cleanCsvToken(currentToken.toString()))
        return result
    }

    fun cleanCsvToken(token: String): String {
        var s = token.trim()
        if (s.startsWith("\"") && s.endsWith("\"") && s.length >= 2) {
            s = s.substring(1, s.length - 1).trim()
        }
        return s
    }
}
