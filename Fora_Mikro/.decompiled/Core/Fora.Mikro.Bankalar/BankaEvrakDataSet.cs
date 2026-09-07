using System;
using System.Data;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Evraklar;

namespace Fora.Mikro.Bankalar;

public class BankaEvrakDataSet
{
	public BankaEvrakCalismaTipi _CalismaTipi;

	public string _DosyaAdi;

	public string _ArsivKlasoru;

	public DateTime _AktarimDosyasiIlkTarih;

	public double _AktarimDosyasiAcilisBakiyesi;

	public DateTime _AktarimDosyasiSonTarih;

	public double _AktarimDosyasiKapanisBakiyesi;

	public Banka banka { get; set; }

	public DataSet dataset { get; set; }

	public string metin1_baslik { get; set; }

	public string metin2_baslik { get; set; }

	public string metin3_baslik { get; set; }

	public string metin4_baslik { get; set; }

	public string metin5_baslik { get; set; }

	public string metin6_baslik { get; set; }

	public string metin7_baslik { get; set; }

	public string metin8_baslik { get; set; }

	public string metin9_baslik { get; set; }

	public string metin10_baslik { get; set; }

	public string metin11_baslik { get; set; }

	public string metin12_baslik { get; set; }

	public string metin13_baslik { get; set; }

	public string metin14_baslik { get; set; }

	public string metin15_baslik { get; set; }

	public string metin16_baslik { get; set; }

	public string metin17_baslik { get; set; }

	public string metin18_baslik { get; set; }

	public string metin19_baslik { get; set; }

	public string metin20_baslik { get; set; }

	public string metin21_baslik { get; set; }

	public string metin22_baslik { get; set; }

	public string metin23_baslik { get; set; }

	public string metin24_baslik { get; set; }

	public string metin25_baslik { get; set; }

	public string metin26_baslik { get; set; }

	public string metin27_baslik { get; set; }

	public string metin28_baslik { get; set; }

	public string metin29_baslik { get; set; }

	public string metin30_baslik { get; set; }

	public string metin31_baslik { get; set; }

	public string metin32_baslik { get; set; }

	public string metin33_baslik { get; set; }

	public string metin34_baslik { get; set; }

	public string metin35_baslik { get; set; }

	public string metin36_baslik { get; set; }

	public string metin37_baslik { get; set; }

	public string metin38_baslik { get; set; }

	public string metin39_baslik { get; set; }

	public string metin40_baslik { get; set; }

	public string metin41_baslik { get; set; }

	public string metin42_baslik { get; set; }

	public string metin43_baslik { get; set; }

	public string metin44_baslik { get; set; }

	public string metin45_baslik { get; set; }

	public string metin46_baslik { get; set; }

	public string metin47_baslik { get; set; }

	public string metin48_baslik { get; set; }

	public string metin49_baslik { get; set; }

	public string metin50_baslik { get; set; }

	public string dropbox1_baslik { get; set; }

	public string dropbox2_baslik { get; set; }

	public string dropbox3_baslik { get; set; }

	public string dropbox4_baslik { get; set; }

	public string dropbox5_baslik { get; set; }

	public string dropbox6_baslik { get; set; }

	public string dropbox7_baslik { get; set; }

	public string dropbox8_baslik { get; set; }

	public string dropbox9_baslik { get; set; }

	public string dropbox10_baslik { get; set; }

	public string checkbox1_baslik { get; set; }

	public string checkbox2_baslik { get; set; }

	public string checkbox3_baslik { get; set; }

	public string checkbox4_baslik { get; set; }

	public string checkbox5_baslik { get; set; }

	public string checkbox6_baslik { get; set; }

	public string checkbox7_baslik { get; set; }

	public string checkbox8_baslik { get; set; }

	public string checkbox9_baslik { get; set; }

	public string checkbox10_baslik { get; set; }

	public string dropbox1_secenekler_yazi { get; set; }

	public string dropbox1_secenekler_veri { get; set; }

	public string dropbox2_secenekler_yazi { get; set; }

	public string dropbox2_secenekler_veri { get; set; }

	public string dropbox3_secenekler_yazi { get; set; }

	public string dropbox3_secenekler_veri { get; set; }

	public string dropbox4_secenekler_yazi { get; set; }

	public string dropbox4_secenekler_veri { get; set; }

	public string dropbox5_secenekler_yazi { get; set; }

	public string dropbox5_secenekler_veri { get; set; }

	public string dropbox6_secenekler_yazi { get; set; }

	public string dropbox6_secenekler_veri { get; set; }

	public string dropbox7_secenekler_yazi { get; set; }

	public string dropbox7_secenekler_veri { get; set; }

	public string dropbox8_secenekler_yazi { get; set; }

	public string dropbox8_secenekler_veri { get; set; }

	public string dropbox9_secenekler_yazi { get; set; }

	public string dropbox9_secenekler_veri { get; set; }

	public string dropbox10_secenekler_yazi { get; set; }

	public string dropbox10_secenekler_veri { get; set; }

	public int NewEvrakID
	{
		get
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			int num = 0;
			foreach (DataRow item in (InternalDataCollectionBase)dataset.Tables[0].Rows)
			{
				DataRow val = item;
				if (Convert.ToInt32(val["EvrakID"]) > num)
				{
					num = Convert.ToInt32(val["EvrakID"]);
				}
			}
			return num + 1;
		}
	}

	public int NewSatirID
	{
		get
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			int num = 0;
			foreach (DataRow item in (InternalDataCollectionBase)dataset.Tables[1].Rows)
			{
				DataRow val = item;
				if (Convert.ToInt32(val["SatirID"]) > num)
				{
					num = Convert.ToInt32(val["SatirID"]);
				}
			}
			return num + 1;
		}
	}

	public BankaEvrakDataSet()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Expected O, but got Unknown
		banka = new Banka();
		_CalismaTipi = BankaEvrakCalismaTipi.Serbest;
		_DosyaAdi = "";
		_ArsivKlasoru = "";
		_AktarimDosyasiIlkTarih = DateTime.MinValue;
		_AktarimDosyasiAcilisBakiyesi = 0.0;
		_AktarimDosyasiSonTarih = DateTime.MinValue;
		_AktarimDosyasiKapanisBakiyesi = 0.0;
		dataset = new DataSet();
		DataTable val = new DataTable("Evraklar");
		val.Columns.Add("EvrakID", typeof(int));
		val.Columns.Add("Aktar", typeof(bool));
		val.Columns.Add("AktarimDurumu", typeof(int));
		val.Columns.Add("EvrakTipi", typeof(int));
		val.Columns.Add("EvrakSeri", typeof(string));
		val.Columns.Add("EvrakSira", typeof(int));
		val.Columns.Add("Tarih", typeof(DateTime));
		val.Columns.Add("BelgeNo", typeof(string));
		val.Columns.Add("BelgeTarihi", typeof(DateTime));
		val.Columns.Add("Kur", typeof(double));
		val.Columns.Add("SorumlulukMerkezi", typeof(string));
		val.Columns.Add("special1", typeof(string));
		val.Columns.Add("special2", typeof(string));
		val.Columns.Add("special3", typeof(string));
		DataTable val2 = new DataTable("Satirlar");
		val2.Columns.Add("SatirID", typeof(int));
		val2.Columns.Add("EvrakID", typeof(int));
		val2.Columns.Add("Cinsi", typeof(int));
		val2.Columns.Add("HesapKodu", typeof(string));
		val2.Columns.Add("Tutar", typeof(double));
		val2.Columns.Add("Aciklama", typeof(string));
		val2.Columns.Add("FaturaOlusturmaDurumu", typeof(int));
		val2.Columns.Add("FaturaHesapKodu", typeof(string));
		val2.Columns.Add("FaturaMiktari", typeof(double));
		val2.Columns.Add("FaturaSeri", typeof(string));
		val2.Columns.Add("FaturaSira", typeof(int));
		val2.Columns.Add("FaturaSorumlulukMerkezi", typeof(string));
		val2.Columns.Add("FaturaProje", typeof(string));
		val2.Columns.Add("BV_Aciklama", typeof(string));
		val2.Columns.Add("BV_IslemKodu", typeof(string));
		val2.Columns.Add("BV_HesapNo", typeof(string));
		val2.Columns.Add("BV_Unvan", typeof(string));
		val2.Columns.Add("BV_Unvan2", typeof(string));
		val2.Columns.Add("BV_Adres", typeof(string));
		val2.Columns.Add("BV_Mahalle", typeof(string));
		val2.Columns.Add("BV_Ilce", typeof(string));
		val2.Columns.Add("BV_Il", typeof(string));
		val2.Columns.Add("BV_Ulke", typeof(string));
		val2.Columns.Add("BV_PostaKodu", typeof(string));
		val2.Columns.Add("BV_Telefon", typeof(string));
		val2.Columns.Add("BV_EPosta", typeof(string));
		val2.Columns.Add("BV_TcVergiNo", typeof(string));
		val2.Columns.Add("OnayDurumuHesapKodu", typeof(bool));
		val2.Columns.Add("OnayDurumuFaturaHesapKodu", typeof(bool));
		val2.Columns.Add("OnayDurumuFaturaSorumlulukMerkezi", typeof(bool));
		val2.Columns.Add("OnayDurumuFaturaProje", typeof(bool));
		val2.Columns.Add("TempCari", typeof(object));
		val2.Columns.Add("GrupNo", typeof(int));
		val2.Columns.Add("Aktar", typeof(bool));
		val2.Columns.Add("mikro_disi_ek_bilgileri_kullan", typeof(bool));
		val2.Columns.Add("metin1", typeof(string));
		val2.Columns.Add("metin2", typeof(string));
		val2.Columns.Add("metin3", typeof(string));
		val2.Columns.Add("metin4", typeof(string));
		val2.Columns.Add("metin5", typeof(string));
		val2.Columns.Add("metin6", typeof(string));
		val2.Columns.Add("metin7", typeof(string));
		val2.Columns.Add("metin8", typeof(string));
		val2.Columns.Add("metin9", typeof(string));
		val2.Columns.Add("metin10", typeof(string));
		val2.Columns.Add("metin11", typeof(string));
		val2.Columns.Add("metin12", typeof(string));
		val2.Columns.Add("metin13", typeof(string));
		val2.Columns.Add("metin14", typeof(string));
		val2.Columns.Add("metin15", typeof(string));
		val2.Columns.Add("metin16", typeof(string));
		val2.Columns.Add("metin17", typeof(string));
		val2.Columns.Add("metin18", typeof(string));
		val2.Columns.Add("metin19", typeof(string));
		val2.Columns.Add("metin20", typeof(string));
		val2.Columns.Add("metin21", typeof(string));
		val2.Columns.Add("metin22", typeof(string));
		val2.Columns.Add("metin23", typeof(string));
		val2.Columns.Add("metin24", typeof(string));
		val2.Columns.Add("metin25", typeof(string));
		val2.Columns.Add("metin26", typeof(string));
		val2.Columns.Add("metin27", typeof(string));
		val2.Columns.Add("metin28", typeof(string));
		val2.Columns.Add("metin29", typeof(string));
		val2.Columns.Add("metin30", typeof(string));
		val2.Columns.Add("metin31", typeof(string));
		val2.Columns.Add("metin32", typeof(string));
		val2.Columns.Add("metin33", typeof(string));
		val2.Columns.Add("metin34", typeof(string));
		val2.Columns.Add("metin35", typeof(string));
		val2.Columns.Add("metin36", typeof(string));
		val2.Columns.Add("metin37", typeof(string));
		val2.Columns.Add("metin38", typeof(string));
		val2.Columns.Add("metin39", typeof(string));
		val2.Columns.Add("metin40", typeof(string));
		val2.Columns.Add("metin41", typeof(string));
		val2.Columns.Add("metin42", typeof(string));
		val2.Columns.Add("metin43", typeof(string));
		val2.Columns.Add("metin44", typeof(string));
		val2.Columns.Add("metin45", typeof(string));
		val2.Columns.Add("metin46", typeof(string));
		val2.Columns.Add("metin47", typeof(string));
		val2.Columns.Add("metin48", typeof(string));
		val2.Columns.Add("metin49", typeof(string));
		val2.Columns.Add("metin50", typeof(string));
		val2.Columns.Add("dropbox1", typeof(string));
		val2.Columns.Add("dropbox2", typeof(string));
		val2.Columns.Add("dropbox3", typeof(string));
		val2.Columns.Add("dropbox4", typeof(string));
		val2.Columns.Add("dropbox5", typeof(string));
		val2.Columns.Add("dropbox6", typeof(string));
		val2.Columns.Add("dropbox7", typeof(string));
		val2.Columns.Add("dropbox8", typeof(string));
		val2.Columns.Add("dropbox9", typeof(string));
		val2.Columns.Add("dropbox10", typeof(string));
		val2.Columns.Add("checkbox1", typeof(bool));
		val2.Columns.Add("checkbox2", typeof(bool));
		val2.Columns.Add("checkbox3", typeof(bool));
		val2.Columns.Add("checkbox4", typeof(bool));
		val2.Columns.Add("checkbox5", typeof(bool));
		val2.Columns.Add("checkbox6", typeof(bool));
		val2.Columns.Add("checkbox7", typeof(bool));
		val2.Columns.Add("checkbox8", typeof(bool));
		val2.Columns.Add("checkbox9", typeof(bool));
		val2.Columns.Add("checkbox10", typeof(bool));
		dataset.Tables.Add(val);
		dataset.Tables.Add(val2);
		RelationEkle();
		metin1_baslik = "Metin 1";
		metin2_baslik = "Metin 2";
		metin3_baslik = "Metin 3";
		metin4_baslik = "Metin 4";
		metin5_baslik = "Metin 5";
		metin6_baslik = "Metin 6";
		metin7_baslik = "Metin 7";
		metin8_baslik = "Metin 8";
		metin9_baslik = "Metin 9";
		metin10_baslik = "Metin 10";
		metin11_baslik = "Metin 11";
		metin12_baslik = "Metin 12";
		metin13_baslik = "Metin 13";
		metin14_baslik = "Metin 14";
		metin15_baslik = "Metin 15";
		metin16_baslik = "Metin 16";
		metin17_baslik = "Metin 17";
		metin18_baslik = "Metin 18";
		metin19_baslik = "Metin 19";
		metin20_baslik = "Metin 20";
		metin21_baslik = "Metin 21";
		metin22_baslik = "Metin 22";
		metin23_baslik = "Metin 23";
		metin24_baslik = "Metin 24";
		metin25_baslik = "Metin 25";
		metin26_baslik = "Metin 26";
		metin27_baslik = "Metin 27";
		metin28_baslik = "Metin 28";
		metin29_baslik = "Metin 29";
		metin30_baslik = "Metin 30";
		metin31_baslik = "Metin 31";
		metin32_baslik = "Metin 32";
		metin33_baslik = "Metin 33";
		metin34_baslik = "Metin 34";
		metin35_baslik = "Metin 35";
		metin36_baslik = "Metin 36";
		metin37_baslik = "Metin 37";
		metin38_baslik = "Metin 38";
		metin39_baslik = "Metin 39";
		metin40_baslik = "Metin 40";
		metin41_baslik = "Metin 41";
		metin42_baslik = "Metin 42";
		metin43_baslik = "Metin 43";
		metin44_baslik = "Metin 44";
		metin45_baslik = "Metin 45";
		metin46_baslik = "Metin 46";
		metin47_baslik = "Metin 47";
		metin48_baslik = "Metin 48";
		metin49_baslik = "Metin 49";
		metin50_baslik = "Metin 50";
		dropbox1_baslik = "Açılır kutu 1";
		dropbox2_baslik = "Açılır kutu 2";
		dropbox3_baslik = "Açılır kutu 3";
		dropbox4_baslik = "Açılır kutu 4";
		dropbox5_baslik = "Açılır kutu 5";
		dropbox6_baslik = "Açılır kutu 6";
		dropbox7_baslik = "Açılır kutu 7";
		dropbox8_baslik = "Açılır kutu 8";
		dropbox9_baslik = "Açılır kutu 9";
		dropbox10_baslik = "Açılır kutu 10";
		checkbox1_baslik = "Onay kutusu 1";
		checkbox2_baslik = "Onay kutusu 2";
		checkbox3_baslik = "Onay kutusu 3";
		checkbox4_baslik = "Onay kutusu 4";
		checkbox5_baslik = "Onay kutusu 5";
		checkbox6_baslik = "Onay kutusu 6";
		checkbox7_baslik = "Onay kutusu 7";
		checkbox8_baslik = "Onay kutusu 8";
		checkbox9_baslik = "Onay kutusu 9";
		checkbox10_baslik = "Onay kutusu 10";
		dropbox1_secenekler_yazi = "SEÇİNİZ,BİRİNCİ SEÇENEK,İKİNCİ SEÇENEK";
		dropbox1_secenekler_veri = "0,1,2";
		dropbox2_secenekler_yazi = "SEÇİNİZ,BİRİNCİ SEÇENEK,İKİNCİ SEÇENEK";
		dropbox2_secenekler_veri = "0,1,2";
		dropbox3_secenekler_yazi = "SEÇİNİZ,BİRİNCİ SEÇENEK,İKİNCİ SEÇENEK";
		dropbox3_secenekler_veri = "0,1,2";
		dropbox4_secenekler_yazi = "SEÇİNİZ,BİRİNCİ SEÇENEK,İKİNCİ SEÇENEK";
		dropbox4_secenekler_veri = "0,1,2";
		dropbox5_secenekler_yazi = "SEÇİNİZ,BİRİNCİ SEÇENEK,İKİNCİ SEÇENEK";
		dropbox5_secenekler_veri = "0,1,2";
		dropbox6_secenekler_yazi = "SEÇİNİZ,BİRİNCİ SEÇENEK,İKİNCİ SEÇENEK";
		dropbox6_secenekler_veri = "0,1,2";
		dropbox7_secenekler_yazi = "SEÇİNİZ,BİRİNCİ SEÇENEK,İKİNCİ SEÇENEK";
		dropbox7_secenekler_veri = "0,1,2";
		dropbox8_secenekler_yazi = "SEÇİNİZ,BİRİNCİ SEÇENEK,İKİNCİ SEÇENEK";
		dropbox8_secenekler_veri = "0,1,2";
		dropbox9_secenekler_yazi = "SEÇİNİZ,BİRİNCİ SEÇENEK,İKİNCİ SEÇENEK";
		dropbox9_secenekler_veri = "0,1,2";
		dropbox10_secenekler_yazi = "SEÇİNİZ,BİRİNCİ SEÇENEK,İKİNCİ SEÇENEK";
		dropbox10_secenekler_veri = "0,1,2";
	}

	public void RelationEkle()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		DataColumn val = dataset.Tables[0].Columns["EvrakID"];
		DataColumn val2 = dataset.Tables[1].Columns["EvrakID"];
		DataRelation val3 = new DataRelation("Satirlar", val, val2);
		dataset.Relations.Add(val3);
	}

	public double GetEvrakYekun(DataRow row)
	{
		double num = 0.0;
		enum_GenelEvrakTipleri enum_GenelEvrakTipleri = (enum_GenelEvrakTipleri)row[3];
		DataRow[] childRows = row.GetChildRows(dataset.Relations[0]);
		foreach (DataRow val in childRows)
		{
			num += (double)val[4];
		}
		if (enum_GenelEvrakTipleri == enum_GenelEvrakTipleri.GidenHavale || enum_GenelEvrakTipleri == enum_GenelEvrakTipleri.BankadanKasayaNakitCekmeMakbuzu || enum_GenelEvrakTipleri == enum_GenelEvrakTipleri.VerilenFirmaCekiOdemeBordrosu || enum_GenelEvrakTipleri == enum_GenelEvrakTipleri.VerilenFirmaSenediOdemeBordrosu)
		{
			num *= -1.0;
		}
		return num;
	}

	public void EvrakEkle(int EvrakID, bool Aktar, enum_GenelEvrakAktarimDurumu AktarimDurumu, enum_GenelEvrakTipleri EvrakTipi, string EvrakSeri, int EvrakSira, DateTime Tarih, string BelgeNo, DateTime BelgeTarihi, double Kur, string SorumlulukMerkezi, string special1, string special2, string special3)
	{
		dataset.Tables[0].Rows.Add(new object[14]
		{
			EvrakID, Aktar, AktarimDurumu, EvrakTipi, EvrakSeri, EvrakSira, Tarih, BelgeNo, BelgeTarihi, Kur,
			SorumlulukMerkezi, special1, special2, special3
		});
	}

	public void SatirEkle(int EvrakID, enum_BankaSatirCinsi Cinsi, string HesapKodu, double Tutar, string Aciklama, enum_FaturaOlusturmaDurumu FaturaOlusturmaDurumu, string FaturaHesapKodu, double FaturaMiktari, string FaturaSeri, int FaturaSira, string FaturaSorumlulukMerkezi, string FaturaProje, string BV_Aciklama, string BV_IslemKodu, string BV_HesapNo, string BV_Unvan, string BV_Unvan2, string BV_Adres, string BV_Mahalle, string BV_Ilce, string BV_Il, string BV_Ulke, string BV_PostaKodu, string BV_Telefon, string BV_EPosta, string BV_TcVergiNo, bool OnayDurumuHesapKodu, bool OnayDurumuFaturaHesapKodu, bool OnayDurumuFaturaSorumlulukMerkezi, bool OnayDurumuFaturaProje, int GrupNo, bool mikro_disi_ek_bilgileri_kullan, string metin1, string metin2, string metin3, string metin4, string metin5, string metin6, string metin7, string metin8, string metin9, string metin10, string metin11, string metin12, string metin13, string metin14, string metin15, string metin16, string metin17, string metin18, string metin19, string metin20, string metin21, string metin22, string metin23, string metin24, string metin25, string metin26, string metin27, string metin28, string metin29, string metin30, string metin31, string metin32, string metin33, string metin34, string metin35, string metin36, string metin37, string metin38, string metin39, string metin40, string metin41, string metin42, string metin43, string metin44, string metin45, string metin46, string metin47, string metin48, string metin49, string metin50, string dropbox1, string dropbox2, string dropbox3, string dropbox4, string dropbox5, string dropbox6, string dropbox7, string dropbox8, string dropbox9, string dropbox10, bool checkbox1, bool checkbox2, bool checkbox3, bool checkbox4, bool checkbox5, bool checkbox6, bool checkbox7, bool checkbox8, bool checkbox9, bool checkbox10)
	{
		dataset.Tables[1].Rows.Add(new object[105]
		{
			NewSatirID,
			EvrakID,
			Cinsi,
			HesapKodu,
			Tutar,
			Aciklama,
			FaturaOlusturmaDurumu,
			FaturaHesapKodu,
			FaturaMiktari,
			FaturaSeri,
			FaturaSira,
			FaturaSorumlulukMerkezi,
			FaturaProje,
			BV_Aciklama,
			BV_IslemKodu,
			BV_HesapNo,
			BV_Unvan,
			BV_Unvan2,
			BV_Adres,
			BV_Mahalle,
			BV_Ilce,
			BV_Il,
			BV_Ulke,
			BV_PostaKodu,
			BV_Telefon,
			BV_EPosta,
			BV_TcVergiNo,
			OnayDurumuHesapKodu,
			OnayDurumuFaturaHesapKodu,
			OnayDurumuFaturaSorumlulukMerkezi,
			OnayDurumuFaturaProje,
			new Cari(),
			GrupNo,
			true,
			mikro_disi_ek_bilgileri_kullan,
			metin1,
			metin2,
			metin3,
			metin4,
			metin5,
			metin6,
			metin7,
			metin8,
			metin9,
			metin10,
			metin11,
			metin12,
			metin13,
			metin14,
			metin15,
			metin16,
			metin17,
			metin18,
			metin19,
			metin20,
			metin21,
			metin22,
			metin23,
			metin24,
			metin25,
			metin26,
			metin27,
			metin28,
			metin29,
			metin30,
			metin31,
			metin32,
			metin33,
			metin34,
			metin35,
			metin36,
			metin37,
			metin38,
			metin39,
			metin40,
			metin41,
			metin42,
			metin43,
			metin44,
			metin45,
			metin46,
			metin47,
			metin48,
			metin49,
			metin50,
			dropbox1,
			dropbox2,
			dropbox3,
			dropbox4,
			dropbox5,
			dropbox6,
			dropbox7,
			dropbox8,
			dropbox9,
			dropbox10,
			checkbox1,
			checkbox2,
			checkbox3,
			checkbox4,
			checkbox5,
			checkbox6,
			checkbox7,
			checkbox8,
			checkbox9,
			checkbox10
		});
	}
}
