cat >> app/src/main/java/com/example/data/database/DatabaseModels.kt << 'INNEREOF'

@Entity(tableName = "cari_hesap_hareketleri")
data class CariHesapHareketEntity(
    @PrimaryKey val id: String,
    val cariKod: String,
    val tarih: String,
    val evrakTip: Int,
    val evrakNo: String,
    val tip: Int,
    val tutar: Double,
    val borcMu: Boolean,
    val aciklama: String
)

@Entity(tableName = "stok_hareketleri")
data class StokHareketEntity(
    @PrimaryKey val id: String,
    val stokKod: String,
    val tarih: String,
    val tip: Int,
    val evrakTip: Int,
    val evrakNo: String,
    val miktar: Double,
    val birimFiyat: Double,
    val tutar: Double,
    val cariKod: String,
    val depoNo: Int
)

@Entity(tableName = "bridge_bankalar")
data class BridgeBankaEntity(
    @PrimaryKey val id: String,
    val kod: String,
    val isim: String,
    val sube: String,
    val iban: String,
    val hesapNumarasi: String
)

@Entity(tableName = "cari_adresleri")
data class CariAdresEntity(
    @PrimaryKey val id: String,
    val cariKod: String,
    val adresNo: Int,
    val il: String,
    val ilce: String,
    val mahalle: String,
    val cadde: String,
    val sokak: String
)
INNEREOF
