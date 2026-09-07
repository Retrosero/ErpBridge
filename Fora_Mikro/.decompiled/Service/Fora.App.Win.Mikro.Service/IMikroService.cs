using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace Fora.App.Win.Mikro.Service;

[ServiceContract]
public interface IMikroService
{
	[OperationContract]
	[WebGet(UriTemplate = "IsAlive", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message IsAlive();

	[OperationContract]
	[WebGet(UriTemplate = "GetVersion", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetVersion();

	[OperationContract]
	[WebGet(UriTemplate = "GetErrorLog")]
	Stream GetErrorLog();

	[OperationContract]
	[WebGet(UriTemplate = "GetFirmalar", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetFirmalar();

	[OperationContract]
	[WebGet(UriTemplate = "CheckSqlConnection", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message CheckSqlConnection();

	[OperationContract]
	[WebGet(UriTemplate = "CheckSqlRead/{firmaid}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message CheckSqlRead(string firmaid);

	[OperationContract]
	[WebGet(UriTemplate = "CheckSqlWrite/{firmaid}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message CheckSqlWrite(string firmaid);

	[OperationContract]
	[WebGet(UriTemplate = "foraandroid.apk")]
	Stream GetApkFile();

	[OperationContract]
	[WebGet(UriTemplate = "GetMikroDBName/{firmaid}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetMikroDBName(string firmaid);

	[OperationContract]
	[WebGet(UriTemplate = "KullaniciKontroluV2/{firmaid}/{kullanici_adi}/{sifre}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message KullaniciKontroluV2(string firmaid, string kullanici_adi, string sifre);

	[OperationContract]
	[WebGet(UriTemplate = "KullaniciKontroluV3/{firmaid}/{kullanici_adi}/{sifre}/{mikrodbname}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message KullaniciKontroluV3(string firmaid, string kullanici_adi, string sifre, string mikrodbname);

	[OperationContract]
	[WebGet(UriTemplate = "GetLastTriggerRecNo/{firmaid}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetLastTriggerRecNo(string firmaid);

	[OperationContract]
	[WebGet(UriTemplate = "GetLastTriggerRecNoV2/{firmaid}/{mikrodbname}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetLastTriggerRecNoV2(string firmaid, string mikrodbname);

	[OperationContract]
	[WebGet(UriTemplate = "GetRecordCountV2/{firmaid}/{tablo_adi}/{tablo_id_field}/{last_rec_no}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetRecordCountV2(string firmaid, string tablo_adi, string tablo_id_field, string last_rec_no);

	[OperationContract]
	[WebGet(UriTemplate = "GetRecordCountV3/{firmaid}/{tablo_adi}/{tablo_id_field}/{last_rec_no}/{mikrodbname}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetRecordCountV3(string firmaid, string tablo_adi, string tablo_id_field, string last_rec_no, string mikrodbname);

	[OperationContract]
	[WebGet(UriTemplate = "V16_GetRecordCountV3/{firmaid}/{tablo_adi}/{mikrodbname}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message V16_GetRecordCountV3(string firmaid, string tablo_adi, string mikrodbname);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetTabloYeniKayitSayilari/{firmaid}")]
	Stream GetTabloYeniKayitSayilari(string firmaid, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetTabloYeniKayitSayilariV2/{firmaid}/{mikrodbname}")]
	Stream GetTabloYeniKayitSayilariV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetTabloDegisenKayitSayilari/{firmaid}")]
	Stream GetTabloDegisenKayitSayilari(string firmaid, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetTabloDegisenKayitSayilariV2/{firmaid}/{mikrodbname}")]
	Stream GetTabloDegisenKayitSayilariV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "V16_GetTabloDegisenKayitSayilariV2/{firmaid}/{mikrodbname}")]
	Stream V16_GetTabloDegisenKayitSayilariV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetTabloYapisiToplu/{firmaid}")]
	Stream GetTabloYapisiToplu(string firmaid, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetTabloYapisiTopluV2/{firmaid}/{mikrodbname}")]
	Stream GetTabloYapisiTopluV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "V16_GetTabloYapisiTopluV2/{firmaid}/{mikrodbname}")]
	Stream V16_GetTabloYapisiTopluV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetYeniKayitlarToplu/{firmaid}")]
	Stream GetYeniKayitlarToplu(string firmaid, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetYeniKayitlarTopluV2/{firmaid}/{mikrodbname}")]
	Stream GetYeniKayitlarTopluV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetDegisenKayitlarToplu/{firmaid}")]
	Stream GetDegisenKayitlarToplu(string firmaid, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetDegisenKayitlarTopluV2/{firmaid}/{mikrodbname}")]
	Stream GetDegisenKayitlarTopluV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "V16_GetDegisenKayitlarTopluV2/{firmaid}/{mikrodbname}")]
	Stream V16_GetDegisenKayitlarTopluV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetSilinenKayitlarToplu/{firmaid}")]
	Stream GetSilinenKayitlarToplu(string firmaid, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetSilinenKayitlarTopluV2/{firmaid}/{mikrodbname}")]
	Stream GetSilinenKayitlarTopluV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "V16_GetSilinenKayitlarTopluV2/{firmaid}/{mikrodbname}")]
	Stream V16_GetSilinenKayitlarTopluV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebGet(UriTemplate = "SaveLog/{firmaid}/{kullanici_adi}/{sifre}/{etkinlik_adi}")]
	Message SaveLog(string firmaid, string kullanici_adi, string sifre, string etkinlik_adi);

	[OperationContract]
	[WebGet(UriTemplate = "GetSiparisOnaylamaListItems/{firmaid}/{kullanici_adi}/{sifre}/{CariListelemeSecenek}/{CariListelemeAktifGrup}/{TemsilciKodu}")]
	Stream GetSiparisOnaylamaListItems(string firmaid, string kullanici_adi, string sifre, string CariListelemeSecenek, string CariListelemeAktifGrup, string TemsilciKodu);

	[OperationContract]
	[WebGet(UriTemplate = "GetSiparisOnaylamaListItemsV2/{firmaid}/{kullanici_adi}/{sifre}/{CariListelemeSecenek}/{CariListelemeAktifGrup}/{TemsilciKodu}/{mikrodbname}")]
	Stream GetSiparisOnaylamaListItemsV2(string firmaid, string kullanici_adi, string sifre, string CariListelemeSecenek, string CariListelemeAktifGrup, string TemsilciKodu, string mikrodbname);

	[OperationContract]
	[WebInvoke(UriTemplate = "SaveOfflineEvrakV2/{firmaid}")]
	Stream SaveOfflineEvrakV2(string firmaid, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "SaveOfflineEvrakV3/{firmaid}/{mikrodbname}")]
	Stream SaveOfflineEvrakV3(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebGet(UriTemplate = "GetSiparisOnaylamaOnayla/{firmaid}/{kullanici_adi}/{sifre}/{evrak_seri}/{evrak_sira}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetSiparisOnaylamaOnayla(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira);

	[OperationContract]
	[WebGet(UriTemplate = "GetSiparisOnaylamaOnaylaV2/{firmaid}/{kullanici_adi}/{sifre}/{evrak_seri}/{evrak_sira}/{mikrodbname}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetSiparisOnaylamaOnaylaV2(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira, string mikrodbname);

	[OperationContract]
	[WebGet(UriTemplate = "GetSiparisOnaylamaReddet/{firmaid}/{kullanici_adi}/{sifre}/{evrak_seri}/{evrak_sira}/{kapama_kodu}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetSiparisOnaylamaReddet(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira, string kapama_kodu);

	[OperationContract]
	[WebGet(UriTemplate = "GetSiparisOnaylamaReddetV2/{firmaid}/{kullanici_adi}/{sifre}/{evrak_seri}/{evrak_sira}/{kapama_kodu}/{mikrodbname}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetSiparisOnaylamaReddetV2(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira, string kapama_kodu, string mikrodbname);

	[OperationContract]
	[WebGet(UriTemplate = "GetDepolarArasiSiparisEvrakListV3/{firmaid}/{kullanici_adi}/{sifre}/{kaynak_depo_no}/{hedef_depo_no}/{siralama}")]
	Stream GetDepolarArasiSiparisEvrakListV3(string firmaid, string kullanici_adi, string sifre, string kaynak_depo_no, string hedef_depo_no, string siralama);

	[OperationContract]
	[WebGet(UriTemplate = "GetDepolarArasiSiparisEvrakListV4/{firmaid}/{kullanici_adi}/{sifre}/{kaynak_depo_no}/{hedef_depo_no}/{siralama}/{mikrodbname}")]
	Stream GetDepolarArasiSiparisEvrakListV4(string firmaid, string kullanici_adi, string sifre, string kaynak_depo_no, string hedef_depo_no, string siralama, string mikrodbname);

	[OperationContract]
	[WebGet(UriTemplate = "GetDepolarArasiSiparisEvrakV2/{firmaid}/{kullanici_adi}/{sifre}/{evrak_seri}/{evrak_sira}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Stream GetDepolarArasiSiparisEvrakV2(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira);

	[OperationContract]
	[WebGet(UriTemplate = "GetDepolarArasiSiparisEvrakV3/{firmaid}/{kullanici_adi}/{sifre}/{evrak_seri}/{evrak_sira}/{mikrodbname}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Stream GetDepolarArasiSiparisEvrakV3(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira, string mikrodbname);

	[OperationContract]
	[WebGet(UriTemplate = "GetNakliyedekiUrunlerV2/{firmaid}/{kullanici_adi}/{sifre}/{nakliye_depo_no}/{hedef_depo_no}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Stream GetNakliyedekiUrunlerV2(string firmaid, string kullanici_adi, string sifre, string nakliye_depo_no, string hedef_depo_no);

	[OperationContract]
	[WebGet(UriTemplate = "GetNakliyedekiUrunlerV3/{firmaid}/{kullanici_adi}/{sifre}/{nakliye_depo_no}/{hedef_depo_no}/{mikrodbname}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Stream GetNakliyedekiUrunlerV3(string firmaid, string kullanici_adi, string sifre, string nakliye_depo_no, string hedef_depo_no, string mikrodbname);

	[OperationContract]
	[WebGet(UriTemplate = "GetGenelEvrakRapor/{firmaid}/{kullanici_adi}/{sifre}/{format}/{dosyaismi}/{evraktipi}/{evrakseri}/{evraksira}")]
	Stream GetGenelEvrakRapor(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string evraktipi, string evrakseri, string evraksira);

	[OperationContract]
	[WebGet(UriTemplate = "GetGenelEvrakRaporV2/{firmaid}/{kullanici_adi}/{sifre}/{format}/{dosyaismi}/{evraktipi}/{evrakseri}/{evraksira}/{mikrodbname}")]
	Stream GetGenelEvrakRaporV2(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string evraktipi, string evrakseri, string evraksira, string mikrodbname);

	[OperationContract]
	[WebGet(UriTemplate = "GetEvrak/{firmaid}/{kullanici_adi}/{sifre}/{evraktipi}/{evrakseri}/{evraksira}")]
	Stream GetEvrak(string firmaid, string kullanici_adi, string sifre, string evraktipi, string evrakseri, string evraksira);

	[OperationContract]
	[WebGet(UriTemplate = "GetEvrakV2/{firmaid}/{kullanici_adi}/{sifre}/{evraktipi}/{evrakseri}/{evraksira}/{mikrodbname}")]
	Stream GetEvrakV2(string firmaid, string kullanici_adi, string sifre, string evraktipi, string evrakseri, string evraksira, string mikrodbname);

	[OperationContract]
	[WebGet(UriTemplate = "StokAmbarAdresiDegistir/{firmaid}/{kullanici_adi}/{sifre}/{stok_kodu}/{ambaradresi}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message StokAmbarAdresiDegistir(string firmaid, string kullanici_adi, string sifre, string stok_kodu, string ambaradresi);

	[OperationContract]
	[WebGet(UriTemplate = "StokAmbarAdresiDegistirV2/{firmaid}/{kullanici_adi}/{sifre}/{stok_kodu}/{ambaradresi}/{mikrodbname}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message StokAmbarAdresiDegistirV2(string firmaid, string kullanici_adi, string sifre, string stok_kodu, string ambaradresi, string mikrodbname);

	[OperationContract]
	[WebInvoke(UriTemplate = "SendCariEkstre/{firmaid}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message SendCariEkstre(string firmaid, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "SendCariEkstreV2/{firmaid}/{mikrodbname}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message SendCariEkstreV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebGet(UriTemplate = "GetStokFotoListV3/{firmaid}")]
	Stream GetStokFotoListV3(string firmaid);

	[OperationContract]
	[WebGet(UriTemplate = "GetStokFoto/{firmaid}/{filename}")]
	Stream GetStokFoto(string firmaid, string filename);

	[OperationContract]
	[WebInvoke(UriTemplate = "Getmye_TextData/{firmaid}")]
	Stream Getmye_TextData(string firmaid, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "Getmye_TextDataV2/{firmaid}/{mikrodbname}")]
	Stream Getmye_TextDataV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "V16_Getmye_TextDataV2/{firmaid}/{mikrodbname}")]
	Stream V16_Getmye_TextDataV2(string firmaid, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporFileStokSatis/{firmaid}/{kullanici_adi}/{sifre}/{format}/{dosyaismi}")]
	Stream GetRaporFileStokSatis(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporFileStokSatisV2/{firmaid}/{kullanici_adi}/{sifre}/{format}/{dosyaismi}/{mikrodbname}")]
	Stream GetRaporFileStokSatisV2(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporFileStokEnvanterV2/{firmaid}/{kullanici_adi}/{sifre}/{format}/{dosyaismi}/{mikrodbname}")]
	Stream GetRaporFileStokEnvanterV2(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporStokSatis/{firmaid}/{kullanici_adi}/{sifre}")]
	Stream GetRaporStokSatis(string firmaid, string kullanici_adi, string sifre, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporStokSatisV2/{firmaid}/{kullanici_adi}/{sifre}/{mikrodbname}")]
	Stream GetRaporStokSatisV2(string firmaid, string kullanici_adi, string sifre, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporStokEnvanterV2/{firmaid}/{kullanici_adi}/{sifre}/{mikrodbname}")]
	Stream GetRaporStokEnvanterV2(string firmaid, string kullanici_adi, string sifre, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporFileStokSiparis/{firmaid}/{kullanici_adi}/{sifre}/{format}/{dosyaismi}")]
	Stream GetRaporFileStokSiparis(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporFileStokSiparisV2/{firmaid}/{kullanici_adi}/{sifre}/{format}/{dosyaismi}/{mikrodbname}")]
	Stream GetRaporFileStokSiparisV2(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporStokSiparis/{firmaid}/{kullanici_adi}/{sifre}")]
	Stream GetRaporStokSiparis(string firmaid, string kullanici_adi, string sifre, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporStokSiparisV2/{firmaid}/{kullanici_adi}/{sifre}/{mikrodbname}")]
	Stream GetRaporStokSiparisV2(string firmaid, string kullanici_adi, string sifre, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporFileYapilacakTahsilatlar/{firmaid}/{kullanici_adi}/{sifre}/{format}/{dosyaismi}")]
	Stream GetRaporFileYapilacakTahsilatlar(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporFileYapilacakTahsilatlarV2/{firmaid}/{kullanici_adi}/{sifre}/{format}/{dosyaismi}/{mikrodbname}")]
	Stream GetRaporFileYapilacakTahsilatlarV2(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string mikrodbname, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporYapilacakTahsilatlar/{firmaid}/{kullanici_adi}/{sifre}")]
	Stream GetRaporYapilacakTahsilatlar(string firmaid, string kullanici_adi, string sifre, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetRaporYapilacakTahsilatlarV2/{firmaid}/{kullanici_adi}/{sifre}/{mikrodbname}")]
	Stream GetRaporYapilacakTahsilatlarV2(string firmaid, string kullanici_adi, string sifre, string mikrodbname, Stream Content);
}
