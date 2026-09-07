using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Fora.App.Win.Mikro.TopluEvrakGirisi;
using Fora.Mikro;
using Fora.Mikro.Bankalar;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Evraklar;
using Fora.Mikro.OdemeEmri;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;

namespace Fora.App.Win.Mikro.Aktarimlar.BankaAktarimi;

public class Aktarim_TxtCsv : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private DataSet _lookuptablolar;

	private Banka _banka;

	private BankaEvrakDataSet _bankaevrakdataset;

	private Parametreler _bankagenelparametreler;

	private Parametreler _bankadetayparametreler;

	private DosyaTipi dosyatipi;

	private bool OtomatikDosyadanOku;

	private List<string> aranacakkelimeler;

	private bool KurulumSorunuVar;

	private List<AktarimSatirlar> _tempSatirlar;

	private IContainer components;

	private LabelControl labelControl1;

	private LabelControl labelControl_ban_kod;

	private LabelControl labelControl2;

	private LabelControl labelControl3;

	private LabelControl labelControl4;

	private LabelControl labelControl5;

	private OpenFileDialog openFileDialog1;

	private SimpleButton simpleButton_DosyaSec;

	private LabelControl labelControl_ban_ismi;

	private LabelControl labelControl_ban_sube;

	private LabelControl labelControl_ban_hesapno;

	private LabelControl labelControl_doviz_cinsi;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem ayarlarToolStripMenuItem;

	private ToolStripMenuItem ParametrelerToolStripMenuItem;

	private System.Windows.Forms.ComboBox cb_dosyatipi;

	public Aktarim_TxtCsv(MikroUygulamaBilgileri mikrouygulamabilgileri, DataSet lookuptablolar, Banka banka)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_lookuptablolar = lookuptablolar;
		_banka = banka;
		labelControl_ban_kod.Text = _banka.ban_kod;
		labelControl_ban_ismi.Text = _banka.ban_ismi;
		labelControl_ban_sube.Text = _banka.ban_sube;
		labelControl_ban_hesapno.Text = _banka.ban_hesapno;
		labelControl_doviz_cinsi.Text = mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(_banka.ban_doviz_cinsi).Kur_adi;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		KurulumSorunuVar = false;
		cb_dosyatipi.SelectedIndex = 0;
	}

	private void ParametreTanimlaveOku()
	{
		string text = GenelUtility.EnumToString(dosyatipi);
		_bankagenelparametreler = new Parametreler();
		_bankagenelparametreler.ParametreListesi.Add(new Parametre("BankaAktarim", "", "Banka", _banka.ban_kod + "_" + text, 1, "DosyaYolu", ""));
		_bankagenelparametreler.ParametreListesi.Add(new Parametre("BankaAktarim", "", "Banka", _banka.ban_kod + "_" + text, 2, "ArsivDosyaYolu", ""));
		_bankagenelparametreler.ParametreListesi.Add(new Parametre("BankaAktarim", "", "Banka", _banka.ban_kod + "_" + text, 3, "GenelParametrelerSablonAdi", ""));
		_bankagenelparametreler.ParametreListesi.Add(new Parametre("BankaAktarim", "", "Banka", _banka.ban_kod + "_" + text, 4, "KriterListesi", ""));
		_bankagenelparametreler.ParametreListesi.Add(new Parametre("BankaAktarim", "", "Banka", _banka.ban_kod + "_" + text, 5, "AktarimParametreleriSablonAdi", ""));
		_bankagenelparametreler.ParametreListesi.Add(new Parametre("BankaAktarim", "", "Banka", _banka.ban_kod + "_" + text, 6, "ButunVerileriBuyukKarakterYap", ""));
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _bankagenelparametreler, "BankaAktarim", "", "Banka", _banka.ban_kod + "_" + text);
		string getString = _bankagenelparametreler._GetParametre("GenelParametrelerSablonAdi")._GetString;
		_bankadetayparametreler = new Parametreler();
		_bankadetayparametreler = ParametrelerDefault.BankaAktarimGenel(getString);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _bankadetayparametreler, "BankaAktarim", "", "GenelSablon", getString);
	}

	private void ParametrelerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BankaAktarimiParametreleriDuzenleyebilir")._GetBoolean)
		{
			ParametreTanimlaveOku();
			if (new Aktarim_Banka_Parametreler(_mikrouygulamabilgileri, _bankagenelparametreler, _banka.ban_kod, dosyatipi).ShowDialog() == DialogResult.OK)
			{
				ParametreTanimlaveOku();
			}
		}
		else
		{
			MessageBox.Show("Parametre değiştirme yetkiniz bulunmamaktadır.");
		}
	}

	private void simpleButton_DosyaSec_Click(object sender, EventArgs e)
	{
		ParametreTanimlaveOku();
		openFileDialog1.Filter = "TXT Dosyaları|*.txt|CSV Dosyaları|*.csv|Excel Dosyaları|*.xls|Tüm dosyalar|*.*";
		openFileDialog1.InitialDirectory = _bankagenelparametreler._GetParametre("DosyaYolu")._GetString;
		openFileDialog1.FileName = "";
		openFileDialog1.ShowDialog();
		if (!(openFileDialog1.FileName != ""))
		{
			return;
		}
		_bankaevrakdataset = new BankaEvrakDataSet();
		_bankaevrakdataset._CalismaTipi = BankaEvrakCalismaTipi.DosyaAktarimi;
		_bankaevrakdataset._DosyaAdi = openFileDialog1.FileName;
		OtomatikDosyadanOku = false;
		if (File.Exists(_bankaevrakdataset._DosyaAdi + ".work"))
		{
			if (MessageBox.Show("Çalışma dosyası bulundu. açılsın mı?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				OtomatikDosyadanOku = true;
			}
			else
			{
				EvraklariDosyadanOku();
			}
		}
		else
		{
			EvraklariDosyadanOku();
		}
		_bankaevrakdataset._ArsivKlasoru = _bankagenelparametreler._GetParametre("ArsivDosyaYolu")._GetString;
		_bankaevrakdataset._DosyaAdi += ".work";
		EvrakDuzenlemeAc();
	}

	private void EvraklariDosyadanOku()
	{
		switch (dosyatipi)
		{
		case DosyaTipi.Mt940:
			Mt940Oku();
			break;
		case DosyaTipi.TextCsv:
			TextCsvOku();
			break;
		case DosyaTipi.Excel:
			ExcelOku();
			break;
		}
		if (_bankagenelparametreler._GetParametre("ButunVerileriBuyukKarakterYap")._GetBoolean)
		{
			foreach (AktarimSatirlar item in _tempSatirlar)
			{
				item.Aciklama = item.Aciklama.ToUpper();
				item.BV_Aciklama = item.BV_Aciklama.ToUpper();
				item.BV_Adres = item.BV_Adres.ToUpper();
				item.BV_HesapNo = item.BV_HesapNo.ToUpper();
				item.BV_Il = item.BV_Il.ToUpper();
				item.BV_Ilce = item.BV_Ilce.ToUpper();
				item.BV_IslemKodu = item.BV_IslemKodu.ToUpper();
				item.BV_Mahalle = item.BV_Mahalle.ToUpper();
				item.BV_PostaKodu = item.BV_PostaKodu.ToUpper();
				item.BV_TcVergiNo = item.BV_TcVergiNo.ToUpper();
				item.BV_Telefon = item.BV_Telefon.ToUpper();
				item.BV_Ulke = item.BV_Ulke.ToUpper();
				item.BV_Unvan = item.BV_Unvan.ToUpper();
				item.BV_Unvan2 = item.BV_Unvan2.ToUpper();
			}
		}
		DosyaIsleme();
		TempSatirlariCevirveAyarla();
	}

	private void TempSatirlariCevirveAyarla()
	{
		foreach (AktarimSatirlar item in _tempSatirlar)
		{
			int num = 0;
			foreach (DataRow row in _bankaevrakdataset.dataset.Tables[0].Rows)
			{
				if (item.EvrakTipi == (enum_GenelEvrakTipleri)row[3] && item.EvrakTarihi.Day == ((DateTime)row[6]).Day && item.EvrakTarihi.Month == ((DateTime)row[6]).Month && item.EvrakTarihi.Year == ((DateTime)row[6]).Year && item.EvrakSrmMerkeziKodu == (string)row[10])
				{
					num = (int)row[0];
				}
			}
			if (num == 0)
			{
				num = _bankaevrakdataset.NewEvrakID;
				string evrakSeri = "";
				switch (item.EvrakTipi)
				{
				case enum_GenelEvrakTipleri.GelenHavale:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriGelenHavale")._GetString;
					break;
				case enum_GenelEvrakTipleri.GidenHavale:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriGidenHavale")._GetString;
					break;
				case enum_GenelEvrakTipleri.BankadanKasayaNakitCekmeMakbuzu:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriNakitCekme")._GetString;
					break;
				case enum_GenelEvrakTipleri.KasadanBankayaNakitYatirmaMakbuzu:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriNakitYatirma")._GetString;
					break;
				case enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriTahsildekiCekOdeme")._GetString;
					break;
				case enum_GenelEvrakTipleri.TahsileCekCikisBordrosu:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriTahsileCekCikis")._GetString;
					break;
				case enum_GenelEvrakTipleri.TahsildekiSenetOdemeBordrosu:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriTahsildekiSenetOdeme")._GetString;
					break;
				case enum_GenelEvrakTipleri.TahsileSenetCikisBordrosu:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriTahsileSenetCikis")._GetString;
					break;
				case enum_GenelEvrakTipleri.VerilenFirmaCekiOdemeBordrosu:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu")._GetString;
					break;
				case enum_GenelEvrakTipleri.VerilenFirmaSenediOdemeBordrosu:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu")._GetString;
					break;
				case enum_GenelEvrakTipleri.BankalarArasiVirmanDekontu:
					evrakSeri = _bankadetayparametreler._GetParametre("DefaultEvrakSeriBankalarArasiVirmanDekontu")._GetString;
					break;
				}
				string getString = _bankadetayparametreler._GetParametre("DefaultSpecialAlan1")._GetString;
				string getString2 = _bankadetayparametreler._GetParametre("DefaultSpecialAlan2")._GetString;
				string getString3 = _bankadetayparametreler._GetParametre("DefaultSpecialAlan3")._GetString;
				double dov_fiyat = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, _banka.ban_doviz_cinsi, "1", item.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName).dov_fiyat;
				_bankaevrakdataset.EvrakEkle(num, Aktar: true, enum_GenelEvrakAktarimDurumu.Aktarilmamis, item.EvrakTipi, evrakSeri, 0, item.EvrakTarihi, "", item.EvrakTarihi, dov_fiyat, item.EvrakSrmMerkeziKodu, getString, getString2, getString3);
			}
			_bankaevrakdataset.SatirEkle(num, item.Cinsi, item.HesapKodu, item.Tutar, item.Aciklama, item.FaturaOlusturmaDurumu, item.FaturaHesapKodu, item.FaturaMiktari, item.FaturaSeri, item.FaturaSira, item.FaturaSorumlulukMerkezi, item.FaturaProje, item.BV_Aciklama, item.BV_IslemKodu, item.BV_HesapNo, item.BV_Unvan, item.BV_Unvan2, item.BV_Adres, item.BV_Mahalle, item.BV_Ilce, item.BV_Il, item.BV_Ulke, item.BV_PostaKodu, item.BV_Telefon, item.BV_EPosta, item.BV_TcVergiNo, item.OnayDurumuHesapKodu, item.OnayDurumuFaturaHesapKodu, item.OnayDurumuFaturaSorumlulukMerkezi, item.OnayDurumuFaturaProje, item.GrupNo, item.mikro_disi_ek_bilgileri_kullan, item.metin1, item.metin2, item.metin3, item.metin4, item.metin5, item.metin6, item.metin7, item.metin8, item.metin9, item.metin10, item.metin11, item.metin12, item.metin13, item.metin14, item.metin15, item.metin16, item.metin17, item.metin18, item.metin19, item.metin20, item.metin21, item.metin22, item.metin23, item.metin24, item.metin25, item.metin26, item.metin27, item.metin28, item.metin29, item.metin30, item.metin31, item.metin32, item.metin33, item.metin34, item.metin35, item.metin36, item.metin37, item.metin38, item.metin39, item.metin40, item.metin41, item.metin42, item.metin43, item.metin44, item.metin45, item.metin46, item.metin47, item.metin48, item.metin49, item.metin50, item.dropbox1, item.dropbox2, item.dropbox3, item.dropbox4, item.dropbox5, item.dropbox6, item.dropbox7, item.dropbox8, item.dropbox9, item.dropbox10, item.checkbox1, item.checkbox2, item.checkbox3, item.checkbox4, item.checkbox5, item.checkbox6, item.checkbox7, item.checkbox8, item.checkbox9, item.checkbox10);
		}
	}

	private void EvrakDuzenlemeAc()
	{
		string text = GenelUtility.EnumToString(dosyatipi);
		string tag = "duzenleme_" + text;
		_bankaevrakdataset.banka = _banka;
		new TopluBankaEvrakGirisi(_bankaevrakdataset, _mikrouygulamabilgileri, tag, "Banka evrak girişi (" + text + " Dosya Aktarımı)", OtomatikDosyadanOku).ShowDialog(this);
	}

	private void DosyaIsleme()
	{
		foreach (AktarimSatirlar item in _tempSatirlar)
		{
			if (_bankadetayparametreler._GetParametre("EvrakAciklamaBankaKullan")._GetBoolean)
			{
				int getInt = _bankadetayparametreler._GetParametre("EvrakAciklamaBankaBaslangic")._GetInt;
				int num = _bankadetayparametreler._GetParametre("EvrakAciklamaBankaUzunluk")._GetInt;
				if (getInt + num > item.BV_Aciklama.Length)
				{
					num = item.BV_Aciklama.Length - getInt;
				}
				item.Aciklama = item.BV_Aciklama.Substring(getInt, num);
			}
			FullTextAramasiYap(item);
			if (item.Cinsi == enum_BankaSatirCinsi.CariHesap)
			{
				item.FaturaOlusturmaDurumu = (enum_FaturaOlusturmaDurumu)_bankadetayparametreler._GetParametre("DefaultSatirFaturaOlusturmaDurumu")._GetInt;
			}
			KritereGoreIslemYap(item);
		}
		if (KurulumSorunuVar)
		{
			MessageBox.Show("Banka aktarım ile ilgili kurulum yapılmamış durumda. Bu yüzden otomatik hesap bulma özelliği etkinleştirilemedi. Lütfen Kurulum->Banka Aktarim Kurulumu adımından kurulumu yapınız");
		}
	}

	private void FullTextAramasiYap(AktarimSatirlar satir)
	{
		if (KurulumSorunuVar)
		{
			return;
		}
		aranacakkelimeler = new List<string>();
		FullTextIsleme(aranacakkelimeler, satir.BV_TcVergiNo, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikTcVergiNo")._GetInt);
		FullTextIsleme(aranacakkelimeler, satir.BV_HesapNo, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikHesapNo")._GetInt);
		FullTextIsleme(aranacakkelimeler, satir.BV_EPosta, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikEPosta")._GetInt);
		FullTextIsleme(aranacakkelimeler, satir.BV_Unvan, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikUnvan")._GetInt);
		FullTextIsleme(aranacakkelimeler, satir.BV_Unvan2, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikUnvan2")._GetInt);
		FullTextIsleme(aranacakkelimeler, satir.BV_Aciklama, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikAciklama")._GetInt);
		FullTextIsleme(aranacakkelimeler, satir.BV_Telefon, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikTelefon")._GetInt);
		FullTextIsleme(aranacakkelimeler, satir.BV_Mahalle, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikMahalle")._GetInt);
		FullTextIsleme(aranacakkelimeler, satir.BV_Ilce, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikIlce")._GetInt);
		FullTextIsleme(aranacakkelimeler, satir.BV_Il, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikIl")._GetInt);
		FullTextIsleme(aranacakkelimeler, satir.BV_EPosta, _bankadetayparametreler._GetParametre("GelismisAramaAgirlikPostaKodu")._GetInt);
		if (aranacakkelimeler.Count <= 0)
		{
			return;
		}
		string text = "TableRECno";
		if (AppBase.MikroVersiyonu >= 16)
		{
			text = "TableGuid";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("SELECT TOP 1 TableID," + text + ",CON.[RANK] FROM _FORA_BANKA_AKTARIM_SEARCH INNER JOIN CONTAINSTABLE(_FORA_BANKA_AKTARIM_SEARCH,*,'ISABOUT( \n");
		string value = "";
		foreach (string item in aranacakkelimeler)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(item).Append("\n");
			value = ",";
		}
		stringBuilder.Append(")') AS CON ON ID = CON.[KEY] WHERE TableID in (");
		stringBuilder.Append("0");
		if (_bankadetayparametreler._GetParametre("AraCariHesaplar")._GetBoolean)
		{
			stringBuilder.Append(",31");
		}
		if (_bankadetayparametreler._GetParametre("AraCariPersonel")._GetBoolean)
		{
			stringBuilder.Append(",104");
		}
		if (_bankadetayparametreler._GetParametre("AraPersonel")._GetBoolean)
		{
			stringBuilder.Append(",71");
		}
		if (_bankadetayparametreler._GetParametre("AraMasraf")._GetBoolean)
		{
			stringBuilder.Append(",62");
		}
		if (_bankadetayparametreler._GetParametre("AraCekSenet")._GetBoolean)
		{
			stringBuilder.Append(",54");
		}
		if (_bankadetayparametreler._GetParametre("AraBankalar")._GetBoolean)
		{
			stringBuilder.Append(",52");
		}
		stringBuilder.Append(") ORDER BY CON.[RANK] DESC," + text + " DESC");
		int num = 0;
		int num2 = 0;
		Guid guid = Guid.Empty;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = stringBuilder.ToString();
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				if (sqlDataReader.HasRows)
				{
					sqlDataReader.Read();
					num = sqlDataReader.GetSafeInt32(0);
					if (AppBase.MikroVersiyonu >= 16)
					{
						guid = sqlDataReader.GetGuid(1);
					}
					else
					{
						num2 = sqlDataReader.GetSafeInt32(1);
					}
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception)
		{
			KurulumSorunuVar = true;
		}
		if (num == 0 && !(guid != Guid.Empty))
		{
			return;
		}
		SqlDB sqlDB2 = new SqlDB();
		sqlDB2.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		switch (num)
		{
		case 31:
		{
			satir.Cinsi = enum_BankaSatirCinsi.CariHesap;
			satir.OnayDurumuHesapKodu = false;
			Cari cari = ((AppBase.MikroVersiyonu < 16) ? CariData.GetCariByRECno(sqlDB2.Connection, num2, AdreslerTemsilciyeGore: false, "") : CariData.GetCariByGuid(sqlDB2.Connection, guid, AdreslerTemsilciyeGore: false, ""));
			satir.HesapKodu = cari.cari_kod;
			satir.GrupNo = 0;
			if (_banka.ban_doviz_cinsi == cari.cari_doviz_cinsi)
			{
				satir.GrupNo = 0;
			}
			if (_banka.ban_doviz_cinsi == cari.cari_doviz_cinsi1)
			{
				satir.GrupNo = 1;
			}
			if (_banka.ban_doviz_cinsi == cari.cari_doviz_cinsi2)
			{
				satir.GrupNo = 2;
			}
			break;
		}
		case 104:
			satir.Cinsi = enum_BankaSatirCinsi.CariPersonel;
			satir.OnayDurumuHesapKodu = false;
			if (AppBase.MikroVersiyonu >= 16)
			{
				satir.HesapKodu = CariPersonelData.GetCariPersonel(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, guid).cari_per_kod;
			}
			else
			{
				satir.HesapKodu = CariPersonelData.GetCariPersonel(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, num2).cari_per_kod;
			}
			satir.GrupNo = 0;
			break;
		case 71:
			satir.Cinsi = enum_BankaSatirCinsi.Personel;
			satir.OnayDurumuHesapKodu = false;
			if (AppBase.MikroVersiyonu >= 16)
			{
				satir.HesapKodu = PersonelData.GetPersonel(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, guid).per_kod;
			}
			else
			{
				satir.HesapKodu = PersonelData.GetPersonel(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, num2).per_kod;
			}
			satir.GrupNo = 0;
			break;
		case 62:
			satir.Cinsi = enum_BankaSatirCinsi.Masraf;
			satir.OnayDurumuHesapKodu = false;
			if (AppBase.MikroVersiyonu >= 16)
			{
				satir.HesapKodu = MasrafData.GetMasraf(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, guid).his_kod;
			}
			else
			{
				satir.HesapKodu = MasrafData.GetMasraf(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, num2).his_kod;
			}
			satir.GrupNo = 0;
			break;
		case 52:
			if (satir.EvrakTipi == enum_GenelEvrakTipleri.GidenHavale)
			{
				satir.Tutar *= -1.0;
			}
			satir.EvrakTipi = enum_GenelEvrakTipleri.BankalarArasiVirmanDekontu;
			satir.Cinsi = enum_BankaSatirCinsi.Banka;
			satir.OnayDurumuHesapKodu = false;
			if (AppBase.MikroVersiyonu >= 16)
			{
				satir.HesapKodu = BankaData.GetBanka(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, guid).ban_kod;
			}
			else
			{
				satir.HesapKodu = BankaData.GetBanka(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, num2).ban_kod;
			}
			satir.GrupNo = 0;
			break;
		case 54:
		{
			OdemeEmirleri odemeEmirleri = ((AppBase.MikroVersiyonu < 16) ? OdemeEmirleriData.GetOdemeEmiri(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, num2) : OdemeEmirleriData.GetOdemeEmiri(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, guid));
			if (odemeEmirleri.sck_tutar == satir.Tutar)
			{
				switch (odemeEmirleri.sck_tip)
				{
				case enum_sck_tip.KendiCekimiz:
					satir.EvrakTipi = enum_GenelEvrakTipleri.VerilenFirmaCekiOdemeBordrosu;
					satir.Cinsi = enum_BankaSatirCinsi.FirmaCeki;
					satir.HesapKodu = odemeEmirleri.sck_refno;
					satir.EvrakSrmMerkeziKodu = odemeEmirleri.sck_srmmrk;
					satir.GrupNo = 2;
					break;
				case enum_sck_tip.KendiSenedimiz:
					satir.EvrakTipi = enum_GenelEvrakTipleri.VerilenFirmaSenediOdemeBordrosu;
					satir.Cinsi = enum_BankaSatirCinsi.FirmaSeneti;
					satir.HesapKodu = odemeEmirleri.sck_refno;
					satir.EvrakSrmMerkeziKodu = odemeEmirleri.sck_srmmrk;
					satir.GrupNo = 1;
					break;
				case enum_sck_tip.MusteriCeki:
					satir.EvrakTipi = enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu;
					satir.Cinsi = enum_BankaSatirCinsi.MusteriCeki;
					satir.HesapKodu = odemeEmirleri.sck_refno;
					satir.EvrakSrmMerkeziKodu = odemeEmirleri.sck_srmmrk;
					satir.GrupNo = 3;
					break;
				case enum_sck_tip.MusteriSenedi:
					satir.EvrakTipi = enum_GenelEvrakTipleri.TahsildekiSenetOdemeBordrosu;
					satir.Cinsi = enum_BankaSatirCinsi.MusteriSeneti;
					satir.HesapKodu = odemeEmirleri.sck_refno;
					satir.EvrakSrmMerkeziKodu = odemeEmirleri.sck_srmmrk;
					satir.GrupNo = 4;
					break;
				}
			}
			break;
		}
		}
		sqlDB2.ConnectionClose();
	}

	private void FullTextIsleme(List<string> aranacakkelimeler, string veri, int weight)
	{
		if (!(veri != "") || weight == 0)
		{
			return;
		}
		veri = veri.Replace(":", " ");
		veri = veri.Replace("-", " ");
		veri = veri.Replace(".", " ");
		veri = veri.Replace("/", " ");
		veri = veri.Replace("\\", " ");
		veri = veri.Replace("*", " ");
		veri = veri.Replace("'", "");
		string[] array = veri.Split(' ');
		foreach (string text in array)
		{
			bool flag = true;
			if (text.Length < 2)
			{
				flag = false;
			}
			if (flag)
			{
				string[] array2 = _bankadetayparametreler._GetParametre("GelismisAramaHaricTutulacakKelimeler")._GetString.Split(',');
				foreach (string text2 in array2)
				{
					if (text.ToLower() == text2.ToLower())
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				aranacakkelimeler.Add("\"" + text + "\" weight (." + weight + ")");
			}
		}
	}

	private void cb_dosyatipi_SelectedIndexChanged(object sender, EventArgs e)
	{
		dosyatipi = (DosyaTipi)cb_dosyatipi.SelectedIndex;
		ParametreTanimlaveOku();
	}

	private void Mt940Oku()
	{
		_tempSatirlar = new List<AktarimSatirlar>();
		AktarimSatirlar aktarimSatirlar = null;
		using StreamReader streamReader = new StreamReader(_bankaevrakdataset._DosyaAdi, Encoding.GetEncoding("windows-1254"));
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			if (text.StartsWith(":60F"))
			{
				int year = int.Parse("20" + text.Substring(6, 2));
				int month = int.Parse(text.Substring(8, 2));
				int day = int.Parse(text.Substring(10, 2));
				_bankaevrakdataset._AktarimDosyasiIlkTarih = new DateTime(year, month, day);
				text.Substring(11, 3);
				string s = text.Substring(15, text.Length - 15);
				_bankaevrakdataset._AktarimDosyasiAcilisBakiyesi = double.Parse(s);
				if (text.Substring(5, 1) == "D")
				{
					_bankaevrakdataset._AktarimDosyasiAcilisBakiyesi *= -1.0;
				}
			}
			if (text.StartsWith(":61:"))
			{
				if (aktarimSatirlar != null)
				{
					_tempSatirlar.Add(aktarimSatirlar);
				}
				aktarimSatirlar = new AktarimSatirlar();
				aktarimSatirlar.Aciklama = _bankadetayparametreler._GetParametre("DefaultEvrakAciklama")._GetString;
				aktarimSatirlar.Cinsi = (enum_BankaSatirCinsi)_bankadetayparametreler._GetParametre("DefaultSatirCinsi")._GetInt;
				aktarimSatirlar.EvrakSrmMerkeziKodu = _bankadetayparametreler._GetParametre("DefaultEvrakSorumlulukMerkezi")._GetString;
				aktarimSatirlar.EvrakTipi = (enum_GenelEvrakTipleri)_bankadetayparametreler._GetParametre("DefaultEvrakTipi")._GetInt;
				aktarimSatirlar.HesapKodu = _bankadetayparametreler._GetParametre("DefaultSatirHesapKodu")._GetString;
				aktarimSatirlar.FaturaHesapKodu = _bankadetayparametreler._GetParametre("DefaultSatirFaturaHesapKodu")._GetString;
				aktarimSatirlar.FaturaMiktari = _bankadetayparametreler._GetParametre("DefaultSatirFaturaMiktari")._GetDouble;
				aktarimSatirlar.FaturaProje = _bankadetayparametreler._GetParametre("DefaultSatirFaturaProjeKodu")._GetString;
				aktarimSatirlar.FaturaSeri = _bankadetayparametreler._GetParametre("DefaultSatirFaturaEvrakSeri")._GetString;
				aktarimSatirlar.FaturaSorumlulukMerkezi = _bankadetayparametreler._GetParametre("DefaultSatirFaturaSorumlulukMerkeziKodu")._GetString;
				int year2 = int.Parse("20" + text.Substring(4, 2));
				int month2 = int.Parse(text.Substring(6, 2));
				int day2 = int.Parse(text.Substring(8, 2));
				aktarimSatirlar.EvrakTarihi = new DateTime(year2, month2, day2);
				string text2 = text.Substring(10, 1);
				if (!(text2 == "C"))
				{
					if (text2 == "D")
					{
						aktarimSatirlar.EvrakTipi = enum_GenelEvrakTipleri.GidenHavale;
					}
				}
				else
				{
					aktarimSatirlar.EvrakTipi = enum_GenelEvrakTipleri.GelenHavale;
				}
				string text3 = text.Substring(12, text.Length - 12);
				aktarimSatirlar.Tutar = double.Parse(text3.Substring(0, text3.IndexOf("N")));
				aktarimSatirlar.BV_IslemKodu = text3.Substring(text3.IndexOf("N") + 1, 3);
			}
			if (text.StartsWith(":86:"))
			{
				aktarimSatirlar.BV_Aciklama = text.Substring(4, text.Length - 4);
			}
			if (text.StartsWith(":62F"))
			{
				int year3 = int.Parse("20" + text.Substring(6, 2));
				int month3 = int.Parse(text.Substring(8, 2));
				int day3 = int.Parse(text.Substring(10, 2));
				_bankaevrakdataset._AktarimDosyasiSonTarih = new DateTime(year3, month3, day3);
				text.Substring(11, 3);
				string s2 = text.Substring(15, text.Length - 15);
				_bankaevrakdataset._AktarimDosyasiKapanisBakiyesi = double.Parse(s2);
				if (text.Substring(5, 1) == "D")
				{
					_bankaevrakdataset._AktarimDosyasiKapanisBakiyesi *= -1.0;
				}
			}
			Console.WriteLine(text);
		}
		if (aktarimSatirlar != null)
		{
			_tempSatirlar.Add(aktarimSatirlar);
		}
	}

	private void TextCsvOku()
	{
		_tempSatirlar = new List<AktarimSatirlar>();
		AktarimSatirlar aktarimSatirlar = null;
		string getString = _bankagenelparametreler._GetParametre("AktarimParametreleriSablonAdi")._GetString;
		Parametreler parametreler = new Parametreler();
		parametreler = ParametrelerDefault.BankaAktarim(getString);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "BankaAktarim", "", "AktarimSablon", getString);
		string getString2 = _bankadetayparametreler._GetParametre("DefaultEvrakAciklama")._GetString;
		enum_BankaSatirCinsi getInt = (enum_BankaSatirCinsi)_bankadetayparametreler._GetParametre("DefaultSatirCinsi")._GetInt;
		string getString3 = _bankadetayparametreler._GetParametre("DefaultEvrakSorumlulukMerkezi")._GetString;
		enum_GenelEvrakTipleri getInt2 = (enum_GenelEvrakTipleri)_bankadetayparametreler._GetParametre("DefaultEvrakTipi")._GetInt;
		string getString4 = _bankadetayparametreler._GetParametre("DefaultSatirHesapKodu")._GetString;
		string getString5 = _bankadetayparametreler._GetParametre("DefaultSatirFaturaHesapKodu")._GetString;
		double getDouble = _bankadetayparametreler._GetParametre("DefaultSatirFaturaMiktari")._GetDouble;
		string getString6 = _bankadetayparametreler._GetParametre("DefaultSatirFaturaProjeKodu")._GetString;
		string getString7 = _bankadetayparametreler._GetParametre("DefaultSatirFaturaEvrakSeri")._GetString;
		string getString8 = _bankadetayparametreler._GetParametre("DefaultSatirFaturaSorumlulukMerkeziKodu")._GetString;
		bool getBoolean = parametreler._GetParametre("bilgilerin_baslangic_satiri_kullan")._GetBoolean;
		int getInt3 = parametreler._GetParametre("bilgilerin_baslangic_satiri")._GetInt;
		bool getBoolean2 = parametreler._GetParametre("alinacak_satirlarin_baslangic_karakteri_kullan")._GetBoolean;
		string getString9 = parametreler._GetParametre("alinacak_satirlarin_baslangic_karakteri")._GetString;
		bool getBoolean3 = parametreler._GetParametre("ayrac_karakteri_kullan")._GetBoolean;
		string getString10 = parametreler._GetParametre("ayrac_karakteri")._GetString;
		int getInt4 = parametreler._GetParametre("unvan_baslangic")._GetInt;
		int getInt5 = parametreler._GetParametre("unvan_uzunluk")._GetInt;
		int getInt6 = parametreler._GetParametre("unvan2_baslangic")._GetInt;
		int getInt7 = parametreler._GetParametre("unvan2_uzunluk")._GetInt;
		int getInt8 = parametreler._GetParametre("adres_baslangic")._GetInt;
		int getInt9 = parametreler._GetParametre("adres_uzunluk")._GetInt;
		int getInt10 = parametreler._GetParametre("mahalle_baslangic")._GetInt;
		int getInt11 = parametreler._GetParametre("mahalle_uzunluk")._GetInt;
		int getInt12 = parametreler._GetParametre("ilce_baslangic")._GetInt;
		int getInt13 = parametreler._GetParametre("ilce_uzunluk")._GetInt;
		int getInt14 = parametreler._GetParametre("il_baslangic")._GetInt;
		int getInt15 = parametreler._GetParametre("il_uzunluk")._GetInt;
		int getInt16 = parametreler._GetParametre("ulke_baslangic")._GetInt;
		int getInt17 = parametreler._GetParametre("ulke_uzunluk")._GetInt;
		int getInt18 = parametreler._GetParametre("postakodu_baslangic")._GetInt;
		int getInt19 = parametreler._GetParametre("postakodu_uzunluk")._GetInt;
		int getInt20 = parametreler._GetParametre("telefon_baslangic")._GetInt;
		int getInt21 = parametreler._GetParametre("telefon_uzunluk")._GetInt;
		int getInt22 = parametreler._GetParametre("eposta_baslangic")._GetInt;
		int getInt23 = parametreler._GetParametre("eposta_uzunluk")._GetInt;
		int getInt24 = parametreler._GetParametre("tckimlik_vergino_baslangic")._GetInt;
		int getInt25 = parametreler._GetParametre("tckimlik_vergino_uzunluk")._GetInt;
		int getInt26 = parametreler._GetParametre("aciklama_baslangic")._GetInt;
		int getInt27 = parametreler._GetParametre("aciklama_uzunluk")._GetInt;
		int getInt28 = parametreler._GetParametre("banka_hesap_no_baslangic")._GetInt;
		int getInt29 = parametreler._GetParametre("banka_hesap_no_uzunluk")._GetInt;
		int getInt30 = parametreler._GetParametre("tarih_yil_baslangic")._GetInt;
		int getInt31 = parametreler._GetParametre("tarih_yil_uzunluk")._GetInt;
		int getInt32 = parametreler._GetParametre("tarih_ay_baslangic")._GetInt;
		int getInt33 = parametreler._GetParametre("tarih_ay_uzunluk")._GetInt;
		int getInt34 = parametreler._GetParametre("tarih_gun_baslangic")._GetInt;
		int getInt35 = parametreler._GetParametre("tarih_gun_uzunluk")._GetInt;
		int getInt36 = parametreler._GetParametre("tutar_baslangic")._GetInt;
		int getInt37 = parametreler._GetParametre("tutar_uzunluk")._GetInt;
		bool getBoolean4 = parametreler._GetParametre("mikro_disi_ek_bilgileri_kullan")._GetBoolean;
		string getString11 = parametreler._GetParametre("metin1_gorunen_adi")._GetString;
		int getInt38 = parametreler._GetParametre("metin1_baslangic")._GetInt;
		int getInt39 = parametreler._GetParametre("metin1_uzunluk")._GetInt;
		string getString12 = parametreler._GetParametre("metin1_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin1_zorunlu")._GetBoolean;
		string getString13 = parametreler._GetParametre("metin2_gorunen_adi")._GetString;
		int getInt40 = parametreler._GetParametre("metin2_baslangic")._GetInt;
		int getInt41 = parametreler._GetParametre("metin2_uzunluk")._GetInt;
		string getString14 = parametreler._GetParametre("metin2_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin2_zorunlu")._GetBoolean;
		string getString15 = parametreler._GetParametre("metin3_gorunen_adi")._GetString;
		int getInt42 = parametreler._GetParametre("metin3_baslangic")._GetInt;
		int getInt43 = parametreler._GetParametre("metin3_uzunluk")._GetInt;
		string getString16 = parametreler._GetParametre("metin3_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin3_zorunlu")._GetBoolean;
		string getString17 = parametreler._GetParametre("metin4_gorunen_adi")._GetString;
		int getInt44 = parametreler._GetParametre("metin4_baslangic")._GetInt;
		int getInt45 = parametreler._GetParametre("metin4_uzunluk")._GetInt;
		string getString18 = parametreler._GetParametre("metin4_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin4_zorunlu")._GetBoolean;
		string getString19 = parametreler._GetParametre("metin5_gorunen_adi")._GetString;
		int getInt46 = parametreler._GetParametre("metin5_baslangic")._GetInt;
		int getInt47 = parametreler._GetParametre("metin5_uzunluk")._GetInt;
		string getString20 = parametreler._GetParametre("metin5_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin5_zorunlu")._GetBoolean;
		string getString21 = parametreler._GetParametre("metin6_gorunen_adi")._GetString;
		int getInt48 = parametreler._GetParametre("metin6_baslangic")._GetInt;
		int getInt49 = parametreler._GetParametre("metin6_uzunluk")._GetInt;
		string getString22 = parametreler._GetParametre("metin6_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin6_zorunlu")._GetBoolean;
		string getString23 = parametreler._GetParametre("metin7_gorunen_adi")._GetString;
		int getInt50 = parametreler._GetParametre("metin7_baslangic")._GetInt;
		int getInt51 = parametreler._GetParametre("metin7_uzunluk")._GetInt;
		string getString24 = parametreler._GetParametre("metin7_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin7_zorunlu")._GetBoolean;
		string getString25 = parametreler._GetParametre("metin8_gorunen_adi")._GetString;
		int getInt52 = parametreler._GetParametre("metin8_baslangic")._GetInt;
		int getInt53 = parametreler._GetParametre("metin8_uzunluk")._GetInt;
		string getString26 = parametreler._GetParametre("metin8_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin8_zorunlu")._GetBoolean;
		string getString27 = parametreler._GetParametre("metin9_gorunen_adi")._GetString;
		int getInt54 = parametreler._GetParametre("metin9_baslangic")._GetInt;
		int getInt55 = parametreler._GetParametre("metin9_uzunluk")._GetInt;
		string getString28 = parametreler._GetParametre("metin9_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin9_zorunlu")._GetBoolean;
		string getString29 = parametreler._GetParametre("metin10_gorunen_adi")._GetString;
		int getInt56 = parametreler._GetParametre("metin10_baslangic")._GetInt;
		int getInt57 = parametreler._GetParametre("metin10_uzunluk")._GetInt;
		string getString30 = parametreler._GetParametre("metin10_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin10_zorunlu")._GetBoolean;
		string getString31 = parametreler._GetParametre("metin11_gorunen_adi")._GetString;
		int getInt58 = parametreler._GetParametre("metin11_baslangic")._GetInt;
		int getInt59 = parametreler._GetParametre("metin11_uzunluk")._GetInt;
		string getString32 = parametreler._GetParametre("metin11_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin11_zorunlu")._GetBoolean;
		string getString33 = parametreler._GetParametre("metin12_gorunen_adi")._GetString;
		int getInt60 = parametreler._GetParametre("metin12_baslangic")._GetInt;
		int getInt61 = parametreler._GetParametre("metin12_uzunluk")._GetInt;
		string getString34 = parametreler._GetParametre("metin12_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin12_zorunlu")._GetBoolean;
		string getString35 = parametreler._GetParametre("metin13_gorunen_adi")._GetString;
		int getInt62 = parametreler._GetParametre("metin13_baslangic")._GetInt;
		int getInt63 = parametreler._GetParametre("metin13_uzunluk")._GetInt;
		string getString36 = parametreler._GetParametre("metin13_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin13_zorunlu")._GetBoolean;
		string getString37 = parametreler._GetParametre("metin14_gorunen_adi")._GetString;
		int getInt64 = parametreler._GetParametre("metin14_baslangic")._GetInt;
		int getInt65 = parametreler._GetParametre("metin14_uzunluk")._GetInt;
		string getString38 = parametreler._GetParametre("metin14_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin14_zorunlu")._GetBoolean;
		string getString39 = parametreler._GetParametre("metin15_gorunen_adi")._GetString;
		int getInt66 = parametreler._GetParametre("metin15_baslangic")._GetInt;
		int getInt67 = parametreler._GetParametre("metin15_uzunluk")._GetInt;
		string getString40 = parametreler._GetParametre("metin15_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin15_zorunlu")._GetBoolean;
		string getString41 = parametreler._GetParametre("metin16_gorunen_adi")._GetString;
		int getInt68 = parametreler._GetParametre("metin16_baslangic")._GetInt;
		int getInt69 = parametreler._GetParametre("metin16_uzunluk")._GetInt;
		string getString42 = parametreler._GetParametre("metin16_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin16_zorunlu")._GetBoolean;
		string getString43 = parametreler._GetParametre("metin17_gorunen_adi")._GetString;
		int getInt70 = parametreler._GetParametre("metin17_baslangic")._GetInt;
		int getInt71 = parametreler._GetParametre("metin17_uzunluk")._GetInt;
		string getString44 = parametreler._GetParametre("metin17_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin17_zorunlu")._GetBoolean;
		string getString45 = parametreler._GetParametre("metin18_gorunen_adi")._GetString;
		int getInt72 = parametreler._GetParametre("metin18_baslangic")._GetInt;
		int getInt73 = parametreler._GetParametre("metin18_uzunluk")._GetInt;
		string getString46 = parametreler._GetParametre("metin18_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin18_zorunlu")._GetBoolean;
		string getString47 = parametreler._GetParametre("metin19_gorunen_adi")._GetString;
		int getInt74 = parametreler._GetParametre("metin19_baslangic")._GetInt;
		int getInt75 = parametreler._GetParametre("metin19_uzunluk")._GetInt;
		string getString48 = parametreler._GetParametre("metin19_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin19_zorunlu")._GetBoolean;
		string getString49 = parametreler._GetParametre("metin20_gorunen_adi")._GetString;
		int getInt76 = parametreler._GetParametre("metin20_baslangic")._GetInt;
		int getInt77 = parametreler._GetParametre("metin20_uzunluk")._GetInt;
		string getString50 = parametreler._GetParametre("metin20_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin20_zorunlu")._GetBoolean;
		string getString51 = parametreler._GetParametre("metin21_gorunen_adi")._GetString;
		int getInt78 = parametreler._GetParametre("metin21_baslangic")._GetInt;
		int getInt79 = parametreler._GetParametre("metin21_uzunluk")._GetInt;
		string getString52 = parametreler._GetParametre("metin21_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin21_zorunlu")._GetBoolean;
		string getString53 = parametreler._GetParametre("metin22_gorunen_adi")._GetString;
		int getInt80 = parametreler._GetParametre("metin22_baslangic")._GetInt;
		int getInt81 = parametreler._GetParametre("metin22_uzunluk")._GetInt;
		string getString54 = parametreler._GetParametre("metin22_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin22_zorunlu")._GetBoolean;
		string getString55 = parametreler._GetParametre("metin23_gorunen_adi")._GetString;
		int getInt82 = parametreler._GetParametre("metin23_baslangic")._GetInt;
		int getInt83 = parametreler._GetParametre("metin23_uzunluk")._GetInt;
		string getString56 = parametreler._GetParametre("metin23_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin23_zorunlu")._GetBoolean;
		string getString57 = parametreler._GetParametre("metin24_gorunen_adi")._GetString;
		int getInt84 = parametreler._GetParametre("metin24_baslangic")._GetInt;
		int getInt85 = parametreler._GetParametre("metin24_uzunluk")._GetInt;
		string getString58 = parametreler._GetParametre("metin24_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin24_zorunlu")._GetBoolean;
		string getString59 = parametreler._GetParametre("metin25_gorunen_adi")._GetString;
		int getInt86 = parametreler._GetParametre("metin25_baslangic")._GetInt;
		int getInt87 = parametreler._GetParametre("metin25_uzunluk")._GetInt;
		string getString60 = parametreler._GetParametre("metin25_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin25_zorunlu")._GetBoolean;
		string getString61 = parametreler._GetParametre("metin26_gorunen_adi")._GetString;
		int getInt88 = parametreler._GetParametre("metin26_baslangic")._GetInt;
		int getInt89 = parametreler._GetParametre("metin26_uzunluk")._GetInt;
		string getString62 = parametreler._GetParametre("metin26_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin26_zorunlu")._GetBoolean;
		string getString63 = parametreler._GetParametre("metin27_gorunen_adi")._GetString;
		int getInt90 = parametreler._GetParametre("metin27_baslangic")._GetInt;
		int getInt91 = parametreler._GetParametre("metin27_uzunluk")._GetInt;
		string getString64 = parametreler._GetParametre("metin27_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin27_zorunlu")._GetBoolean;
		string getString65 = parametreler._GetParametre("metin28_gorunen_adi")._GetString;
		int getInt92 = parametreler._GetParametre("metin28_baslangic")._GetInt;
		int getInt93 = parametreler._GetParametre("metin28_uzunluk")._GetInt;
		string getString66 = parametreler._GetParametre("metin28_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin28_zorunlu")._GetBoolean;
		string getString67 = parametreler._GetParametre("metin29_gorunen_adi")._GetString;
		int getInt94 = parametreler._GetParametre("metin29_baslangic")._GetInt;
		int getInt95 = parametreler._GetParametre("metin29_uzunluk")._GetInt;
		string getString68 = parametreler._GetParametre("metin29_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin29_zorunlu")._GetBoolean;
		string getString69 = parametreler._GetParametre("metin30_gorunen_adi")._GetString;
		int getInt96 = parametreler._GetParametre("metin30_baslangic")._GetInt;
		int getInt97 = parametreler._GetParametre("metin30_uzunluk")._GetInt;
		string getString70 = parametreler._GetParametre("metin30_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin30_zorunlu")._GetBoolean;
		string getString71 = parametreler._GetParametre("metin31_gorunen_adi")._GetString;
		int getInt98 = parametreler._GetParametre("metin31_baslangic")._GetInt;
		int getInt99 = parametreler._GetParametre("metin31_uzunluk")._GetInt;
		string getString72 = parametreler._GetParametre("metin31_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin31_zorunlu")._GetBoolean;
		string getString73 = parametreler._GetParametre("metin32_gorunen_adi")._GetString;
		int getInt100 = parametreler._GetParametre("metin32_baslangic")._GetInt;
		int getInt101 = parametreler._GetParametre("metin32_uzunluk")._GetInt;
		string getString74 = parametreler._GetParametre("metin32_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin32_zorunlu")._GetBoolean;
		string getString75 = parametreler._GetParametre("metin33_gorunen_adi")._GetString;
		int getInt102 = parametreler._GetParametre("metin33_baslangic")._GetInt;
		int getInt103 = parametreler._GetParametre("metin33_uzunluk")._GetInt;
		string getString76 = parametreler._GetParametre("metin33_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin33_zorunlu")._GetBoolean;
		string getString77 = parametreler._GetParametre("metin34_gorunen_adi")._GetString;
		int getInt104 = parametreler._GetParametre("metin34_baslangic")._GetInt;
		int getInt105 = parametreler._GetParametre("metin34_uzunluk")._GetInt;
		string getString78 = parametreler._GetParametre("metin34_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin34_zorunlu")._GetBoolean;
		string getString79 = parametreler._GetParametre("metin35_gorunen_adi")._GetString;
		int getInt106 = parametreler._GetParametre("metin35_baslangic")._GetInt;
		int getInt107 = parametreler._GetParametre("metin35_uzunluk")._GetInt;
		string getString80 = parametreler._GetParametre("metin35_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin35_zorunlu")._GetBoolean;
		string getString81 = parametreler._GetParametre("metin36_gorunen_adi")._GetString;
		int getInt108 = parametreler._GetParametre("metin36_baslangic")._GetInt;
		int getInt109 = parametreler._GetParametre("metin36_uzunluk")._GetInt;
		string getString82 = parametreler._GetParametre("metin36_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin36_zorunlu")._GetBoolean;
		string getString83 = parametreler._GetParametre("metin37_gorunen_adi")._GetString;
		int getInt110 = parametreler._GetParametre("metin37_baslangic")._GetInt;
		int getInt111 = parametreler._GetParametre("metin37_uzunluk")._GetInt;
		string getString84 = parametreler._GetParametre("metin37_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin37_zorunlu")._GetBoolean;
		string getString85 = parametreler._GetParametre("metin38_gorunen_adi")._GetString;
		int getInt112 = parametreler._GetParametre("metin38_baslangic")._GetInt;
		int getInt113 = parametreler._GetParametre("metin38_uzunluk")._GetInt;
		string getString86 = parametreler._GetParametre("metin38_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin38_zorunlu")._GetBoolean;
		string getString87 = parametreler._GetParametre("metin39_gorunen_adi")._GetString;
		int getInt114 = parametreler._GetParametre("metin39_baslangic")._GetInt;
		int getInt115 = parametreler._GetParametre("metin39_uzunluk")._GetInt;
		string getString88 = parametreler._GetParametre("metin39_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin39_zorunlu")._GetBoolean;
		string getString89 = parametreler._GetParametre("metin40_gorunen_adi")._GetString;
		int getInt116 = parametreler._GetParametre("metin40_baslangic")._GetInt;
		int getInt117 = parametreler._GetParametre("metin40_uzunluk")._GetInt;
		string getString90 = parametreler._GetParametre("metin40_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin40_zorunlu")._GetBoolean;
		string getString91 = parametreler._GetParametre("metin41_gorunen_adi")._GetString;
		int getInt118 = parametreler._GetParametre("metin41_baslangic")._GetInt;
		int getInt119 = parametreler._GetParametre("metin41_uzunluk")._GetInt;
		string getString92 = parametreler._GetParametre("metin41_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin41_zorunlu")._GetBoolean;
		string getString93 = parametreler._GetParametre("metin42_gorunen_adi")._GetString;
		int getInt120 = parametreler._GetParametre("metin42_baslangic")._GetInt;
		int getInt121 = parametreler._GetParametre("metin42_uzunluk")._GetInt;
		string getString94 = parametreler._GetParametre("metin42_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin42_zorunlu")._GetBoolean;
		string getString95 = parametreler._GetParametre("metin43_gorunen_adi")._GetString;
		int getInt122 = parametreler._GetParametre("metin43_baslangic")._GetInt;
		int getInt123 = parametreler._GetParametre("metin43_uzunluk")._GetInt;
		string getString96 = parametreler._GetParametre("metin43_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin43_zorunlu")._GetBoolean;
		string getString97 = parametreler._GetParametre("metin44_gorunen_adi")._GetString;
		int getInt124 = parametreler._GetParametre("metin44_baslangic")._GetInt;
		int getInt125 = parametreler._GetParametre("metin44_uzunluk")._GetInt;
		string getString98 = parametreler._GetParametre("metin44_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin44_zorunlu")._GetBoolean;
		string getString99 = parametreler._GetParametre("metin45_gorunen_adi")._GetString;
		int getInt126 = parametreler._GetParametre("metin45_baslangic")._GetInt;
		int getInt127 = parametreler._GetParametre("metin45_uzunluk")._GetInt;
		string getString100 = parametreler._GetParametre("metin45_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin45_zorunlu")._GetBoolean;
		string getString101 = parametreler._GetParametre("metin46_gorunen_adi")._GetString;
		int getInt128 = parametreler._GetParametre("metin46_baslangic")._GetInt;
		int getInt129 = parametreler._GetParametre("metin46_uzunluk")._GetInt;
		string getString102 = parametreler._GetParametre("metin46_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin46_zorunlu")._GetBoolean;
		string getString103 = parametreler._GetParametre("metin47_gorunen_adi")._GetString;
		int getInt130 = parametreler._GetParametre("metin47_baslangic")._GetInt;
		int getInt131 = parametreler._GetParametre("metin47_uzunluk")._GetInt;
		string getString104 = parametreler._GetParametre("metin47_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin47_zorunlu")._GetBoolean;
		string getString105 = parametreler._GetParametre("metin48_gorunen_adi")._GetString;
		int getInt132 = parametreler._GetParametre("metin48_baslangic")._GetInt;
		int getInt133 = parametreler._GetParametre("metin48_uzunluk")._GetInt;
		string getString106 = parametreler._GetParametre("metin48_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin48_zorunlu")._GetBoolean;
		string getString107 = parametreler._GetParametre("metin49_gorunen_adi")._GetString;
		int getInt134 = parametreler._GetParametre("metin49_baslangic")._GetInt;
		int getInt135 = parametreler._GetParametre("metin49_uzunluk")._GetInt;
		string getString108 = parametreler._GetParametre("metin49_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin49_zorunlu")._GetBoolean;
		string getString109 = parametreler._GetParametre("metin50_gorunen_adi")._GetString;
		int getInt136 = parametreler._GetParametre("metin50_baslangic")._GetInt;
		int getInt137 = parametreler._GetParametre("metin50_uzunluk")._GetInt;
		string getString110 = parametreler._GetParametre("metin50_varsayilan_deger")._GetString;
		_ = parametreler._GetParametre("metin50_zorunlu")._GetBoolean;
		_ = parametreler._GetParametre("dropbox1_zorunlu")._GetBoolean;
		string getString111 = parametreler._GetParametre("dropbox1_gorunen_adi")._GetString;
		int getInt138 = parametreler._GetParametre("dropbox1_baslangic")._GetInt;
		int getInt139 = parametreler._GetParametre("dropbox1_uzunluk")._GetInt;
		string getString112 = parametreler._GetParametre("dropbox1_varsayilan_deger")._GetString;
		string getString113 = parametreler._GetParametre("dropbox1_secenekler_yazi")._GetString;
		string getString114 = parametreler._GetParametre("dropbox1_secenekler_veri")._GetString;
		_ = parametreler._GetParametre("dropbox2_zorunlu")._GetBoolean;
		string getString115 = parametreler._GetParametre("dropbox2_gorunen_adi")._GetString;
		int getInt140 = parametreler._GetParametre("dropbox2_baslangic")._GetInt;
		int getInt141 = parametreler._GetParametre("dropbox2_uzunluk")._GetInt;
		string getString116 = parametreler._GetParametre("dropbox2_varsayilan_deger")._GetString;
		string getString117 = parametreler._GetParametre("dropbox2_secenekler_yazi")._GetString;
		string getString118 = parametreler._GetParametre("dropbox2_secenekler_veri")._GetString;
		_ = parametreler._GetParametre("dropbox3_zorunlu")._GetBoolean;
		string getString119 = parametreler._GetParametre("dropbox3_gorunen_adi")._GetString;
		int getInt142 = parametreler._GetParametre("dropbox3_baslangic")._GetInt;
		int getInt143 = parametreler._GetParametre("dropbox3_uzunluk")._GetInt;
		string getString120 = parametreler._GetParametre("dropbox3_varsayilan_deger")._GetString;
		string getString121 = parametreler._GetParametre("dropbox3_secenekler_yazi")._GetString;
		string getString122 = parametreler._GetParametre("dropbox3_secenekler_veri")._GetString;
		_ = parametreler._GetParametre("dropbox4_zorunlu")._GetBoolean;
		string getString123 = parametreler._GetParametre("dropbox4_gorunen_adi")._GetString;
		int getInt144 = parametreler._GetParametre("dropbox4_baslangic")._GetInt;
		int getInt145 = parametreler._GetParametre("dropbox4_uzunluk")._GetInt;
		string getString124 = parametreler._GetParametre("dropbox4_varsayilan_deger")._GetString;
		string getString125 = parametreler._GetParametre("dropbox4_secenekler_yazi")._GetString;
		string getString126 = parametreler._GetParametre("dropbox4_secenekler_veri")._GetString;
		_ = parametreler._GetParametre("dropbox5_zorunlu")._GetBoolean;
		string getString127 = parametreler._GetParametre("dropbox5_gorunen_adi")._GetString;
		int getInt146 = parametreler._GetParametre("dropbox5_baslangic")._GetInt;
		int getInt147 = parametreler._GetParametre("dropbox5_uzunluk")._GetInt;
		string getString128 = parametreler._GetParametre("dropbox5_varsayilan_deger")._GetString;
		string getString129 = parametreler._GetParametre("dropbox5_secenekler_yazi")._GetString;
		string getString130 = parametreler._GetParametre("dropbox5_secenekler_veri")._GetString;
		_ = parametreler._GetParametre("dropbox6_zorunlu")._GetBoolean;
		string getString131 = parametreler._GetParametre("dropbox6_gorunen_adi")._GetString;
		int getInt148 = parametreler._GetParametre("dropbox6_baslangic")._GetInt;
		int getInt149 = parametreler._GetParametre("dropbox6_uzunluk")._GetInt;
		string getString132 = parametreler._GetParametre("dropbox6_varsayilan_deger")._GetString;
		string getString133 = parametreler._GetParametre("dropbox6_secenekler_yazi")._GetString;
		string getString134 = parametreler._GetParametre("dropbox6_secenekler_veri")._GetString;
		_ = parametreler._GetParametre("dropbox7_zorunlu")._GetBoolean;
		string getString135 = parametreler._GetParametre("dropbox7_gorunen_adi")._GetString;
		int getInt150 = parametreler._GetParametre("dropbox7_baslangic")._GetInt;
		int getInt151 = parametreler._GetParametre("dropbox7_uzunluk")._GetInt;
		string getString136 = parametreler._GetParametre("dropbox7_varsayilan_deger")._GetString;
		string getString137 = parametreler._GetParametre("dropbox7_secenekler_yazi")._GetString;
		string getString138 = parametreler._GetParametre("dropbox7_secenekler_veri")._GetString;
		_ = parametreler._GetParametre("dropbox8_zorunlu")._GetBoolean;
		string getString139 = parametreler._GetParametre("dropbox8_gorunen_adi")._GetString;
		int getInt152 = parametreler._GetParametre("dropbox8_baslangic")._GetInt;
		int getInt153 = parametreler._GetParametre("dropbox8_uzunluk")._GetInt;
		string getString140 = parametreler._GetParametre("dropbox8_varsayilan_deger")._GetString;
		string getString141 = parametreler._GetParametre("dropbox8_secenekler_yazi")._GetString;
		string getString142 = parametreler._GetParametre("dropbox8_secenekler_veri")._GetString;
		_ = parametreler._GetParametre("dropbox9_zorunlu")._GetBoolean;
		string getString143 = parametreler._GetParametre("dropbox9_gorunen_adi")._GetString;
		int getInt154 = parametreler._GetParametre("dropbox9_baslangic")._GetInt;
		int getInt155 = parametreler._GetParametre("dropbox9_uzunluk")._GetInt;
		string getString144 = parametreler._GetParametre("dropbox9_varsayilan_deger")._GetString;
		string getString145 = parametreler._GetParametre("dropbox9_secenekler_yazi")._GetString;
		string getString146 = parametreler._GetParametre("dropbox9_secenekler_veri")._GetString;
		_ = parametreler._GetParametre("dropbox10_zorunlu")._GetBoolean;
		string getString147 = parametreler._GetParametre("dropbox10_gorunen_adi")._GetString;
		int getInt156 = parametreler._GetParametre("dropbox10_baslangic")._GetInt;
		int getInt157 = parametreler._GetParametre("dropbox10_uzunluk")._GetInt;
		string getString148 = parametreler._GetParametre("dropbox10_varsayilan_deger")._GetString;
		string getString149 = parametreler._GetParametre("dropbox10_secenekler_yazi")._GetString;
		string getString150 = parametreler._GetParametre("dropbox10_secenekler_veri")._GetString;
		string getString151 = parametreler._GetParametre("checkbox1_gorunen_adi")._GetString;
		int getInt158 = parametreler._GetParametre("checkbox1_baslangic")._GetInt;
		int getInt159 = parametreler._GetParametre("checkbox1_uzunluk")._GetInt;
		bool getBoolean5 = parametreler._GetParametre("checkbox1_varsayilan_deger")._GetBoolean;
		_ = parametreler._GetParametre("checkbox1_isaretli_icin_deger")._GetString;
		_ = parametreler._GetParametre("checkbox1_isaretsiz_icin_deger")._GetString;
		string getString152 = parametreler._GetParametre("checkbox2_gorunen_adi")._GetString;
		int getInt160 = parametreler._GetParametre("checkbox2_baslangic")._GetInt;
		int getInt161 = parametreler._GetParametre("checkbox2_uzunluk")._GetInt;
		bool getBoolean6 = parametreler._GetParametre("checkbox2_varsayilan_deger")._GetBoolean;
		_ = parametreler._GetParametre("checkbox2_isaretli_icin_deger")._GetString;
		_ = parametreler._GetParametre("checkbox2_isaretsiz_icin_deger")._GetString;
		string getString153 = parametreler._GetParametre("checkbox3_gorunen_adi")._GetString;
		int getInt162 = parametreler._GetParametre("checkbox3_baslangic")._GetInt;
		int getInt163 = parametreler._GetParametre("checkbox3_uzunluk")._GetInt;
		bool getBoolean7 = parametreler._GetParametre("checkbox3_varsayilan_deger")._GetBoolean;
		_ = parametreler._GetParametre("checkbox3_isaretli_icin_deger")._GetString;
		_ = parametreler._GetParametre("checkbox3_isaretsiz_icin_deger")._GetString;
		string getString154 = parametreler._GetParametre("checkbox4_gorunen_adi")._GetString;
		int getInt164 = parametreler._GetParametre("checkbox4_baslangic")._GetInt;
		int getInt165 = parametreler._GetParametre("checkbox4_uzunluk")._GetInt;
		bool getBoolean8 = parametreler._GetParametre("checkbox4_varsayilan_deger")._GetBoolean;
		_ = parametreler._GetParametre("checkbox4_isaretli_icin_deger")._GetString;
		_ = parametreler._GetParametre("checkbox4_isaretsiz_icin_deger")._GetString;
		string getString155 = parametreler._GetParametre("checkbox5_gorunen_adi")._GetString;
		int getInt166 = parametreler._GetParametre("checkbox5_baslangic")._GetInt;
		int getInt167 = parametreler._GetParametre("checkbox5_uzunluk")._GetInt;
		bool getBoolean9 = parametreler._GetParametre("checkbox5_varsayilan_deger")._GetBoolean;
		_ = parametreler._GetParametre("checkbox5_isaretli_icin_deger")._GetString;
		_ = parametreler._GetParametre("checkbox5_isaretsiz_icin_deger")._GetString;
		string getString156 = parametreler._GetParametre("checkbox6_gorunen_adi")._GetString;
		int getInt168 = parametreler._GetParametre("checkbox6_baslangic")._GetInt;
		int getInt169 = parametreler._GetParametre("checkbox6_uzunluk")._GetInt;
		bool getBoolean10 = parametreler._GetParametre("checkbox6_varsayilan_deger")._GetBoolean;
		_ = parametreler._GetParametre("checkbox6_isaretli_icin_deger")._GetString;
		_ = parametreler._GetParametre("checkbox6_isaretsiz_icin_deger")._GetString;
		string getString157 = parametreler._GetParametre("checkbox7_gorunen_adi")._GetString;
		int getInt170 = parametreler._GetParametre("checkbox7_baslangic")._GetInt;
		int getInt171 = parametreler._GetParametre("checkbox7_uzunluk")._GetInt;
		bool getBoolean11 = parametreler._GetParametre("checkbox7_varsayilan_deger")._GetBoolean;
		_ = parametreler._GetParametre("checkbox7_isaretli_icin_deger")._GetString;
		_ = parametreler._GetParametre("checkbox7_isaretsiz_icin_deger")._GetString;
		string getString158 = parametreler._GetParametre("checkbox8_gorunen_adi")._GetString;
		int getInt172 = parametreler._GetParametre("checkbox8_baslangic")._GetInt;
		int getInt173 = parametreler._GetParametre("checkbox8_uzunluk")._GetInt;
		bool getBoolean12 = parametreler._GetParametre("checkbox8_varsayilan_deger")._GetBoolean;
		_ = parametreler._GetParametre("checkbox8_isaretli_icin_deger")._GetString;
		_ = parametreler._GetParametre("checkbox8_isaretsiz_icin_deger")._GetString;
		string getString159 = parametreler._GetParametre("checkbox9_gorunen_adi")._GetString;
		int getInt174 = parametreler._GetParametre("checkbox9_baslangic")._GetInt;
		int getInt175 = parametreler._GetParametre("checkbox9_uzunluk")._GetInt;
		bool getBoolean13 = parametreler._GetParametre("checkbox9_varsayilan_deger")._GetBoolean;
		_ = parametreler._GetParametre("checkbox9_isaretli_icin_deger")._GetString;
		_ = parametreler._GetParametre("checkbox9_isaretsiz_icin_deger")._GetString;
		string getString160 = parametreler._GetParametre("checkbox10_gorunen_adi")._GetString;
		int getInt176 = parametreler._GetParametre("checkbox10_baslangic")._GetInt;
		int getInt177 = parametreler._GetParametre("checkbox10_uzunluk")._GetInt;
		bool getBoolean14 = parametreler._GetParametre("checkbox10_varsayilan_deger")._GetBoolean;
		_ = parametreler._GetParametre("checkbox10_isaretli_icin_deger")._GetString;
		_ = parametreler._GetParametre("checkbox10_isaretsiz_icin_deger")._GetString;
		_bankaevrakdataset.metin1_baslik = getString11;
		_bankaevrakdataset.metin2_baslik = getString13;
		_bankaevrakdataset.metin3_baslik = getString15;
		_bankaevrakdataset.metin4_baslik = getString17;
		_bankaevrakdataset.metin5_baslik = getString19;
		_bankaevrakdataset.metin6_baslik = getString21;
		_bankaevrakdataset.metin7_baslik = getString23;
		_bankaevrakdataset.metin8_baslik = getString25;
		_bankaevrakdataset.metin9_baslik = getString27;
		_bankaevrakdataset.metin10_baslik = getString29;
		_bankaevrakdataset.metin11_baslik = getString31;
		_bankaevrakdataset.metin12_baslik = getString33;
		_bankaevrakdataset.metin13_baslik = getString35;
		_bankaevrakdataset.metin14_baslik = getString37;
		_bankaevrakdataset.metin15_baslik = getString39;
		_bankaevrakdataset.metin16_baslik = getString41;
		_bankaevrakdataset.metin17_baslik = getString43;
		_bankaevrakdataset.metin18_baslik = getString45;
		_bankaevrakdataset.metin19_baslik = getString47;
		_bankaevrakdataset.metin20_baslik = getString49;
		_bankaevrakdataset.metin21_baslik = getString51;
		_bankaevrakdataset.metin22_baslik = getString53;
		_bankaevrakdataset.metin23_baslik = getString55;
		_bankaevrakdataset.metin24_baslik = getString57;
		_bankaevrakdataset.metin25_baslik = getString59;
		_bankaevrakdataset.metin26_baslik = getString61;
		_bankaevrakdataset.metin27_baslik = getString63;
		_bankaevrakdataset.metin28_baslik = getString65;
		_bankaevrakdataset.metin29_baslik = getString67;
		_bankaevrakdataset.metin30_baslik = getString69;
		_bankaevrakdataset.metin31_baslik = getString71;
		_bankaevrakdataset.metin32_baslik = getString73;
		_bankaevrakdataset.metin33_baslik = getString75;
		_bankaevrakdataset.metin34_baslik = getString77;
		_bankaevrakdataset.metin35_baslik = getString79;
		_bankaevrakdataset.metin36_baslik = getString81;
		_bankaevrakdataset.metin37_baslik = getString83;
		_bankaevrakdataset.metin38_baslik = getString85;
		_bankaevrakdataset.metin39_baslik = getString87;
		_bankaevrakdataset.metin40_baslik = getString89;
		_bankaevrakdataset.metin41_baslik = getString91;
		_bankaevrakdataset.metin42_baslik = getString93;
		_bankaevrakdataset.metin43_baslik = getString95;
		_bankaevrakdataset.metin44_baslik = getString97;
		_bankaevrakdataset.metin45_baslik = getString99;
		_bankaevrakdataset.metin46_baslik = getString101;
		_bankaevrakdataset.metin47_baslik = getString103;
		_bankaevrakdataset.metin48_baslik = getString105;
		_bankaevrakdataset.metin49_baslik = getString107;
		_bankaevrakdataset.metin50_baslik = getString109;
		_bankaevrakdataset.dropbox1_baslik = getString111;
		_bankaevrakdataset.dropbox2_baslik = getString115;
		_bankaevrakdataset.dropbox3_baslik = getString119;
		_bankaevrakdataset.dropbox4_baslik = getString123;
		_bankaevrakdataset.dropbox5_baslik = getString127;
		_bankaevrakdataset.dropbox6_baslik = getString131;
		_bankaevrakdataset.dropbox7_baslik = getString135;
		_bankaevrakdataset.dropbox8_baslik = getString139;
		_bankaevrakdataset.dropbox9_baslik = getString143;
		_bankaevrakdataset.dropbox10_baslik = getString147;
		_bankaevrakdataset.checkbox1_baslik = getString151;
		_bankaevrakdataset.checkbox2_baslik = getString152;
		_bankaevrakdataset.checkbox3_baslik = getString153;
		_bankaevrakdataset.checkbox4_baslik = getString154;
		_bankaevrakdataset.checkbox5_baslik = getString155;
		_bankaevrakdataset.checkbox6_baslik = getString156;
		_bankaevrakdataset.checkbox7_baslik = getString157;
		_bankaevrakdataset.checkbox8_baslik = getString158;
		_bankaevrakdataset.checkbox9_baslik = getString159;
		_bankaevrakdataset.checkbox10_baslik = getString160;
		_bankaevrakdataset.dropbox1_secenekler_veri = getString114;
		_bankaevrakdataset.dropbox1_secenekler_yazi = getString113;
		_bankaevrakdataset.dropbox2_secenekler_veri = getString118;
		_bankaevrakdataset.dropbox2_secenekler_yazi = getString117;
		_bankaevrakdataset.dropbox3_secenekler_veri = getString122;
		_bankaevrakdataset.dropbox3_secenekler_yazi = getString121;
		_bankaevrakdataset.dropbox4_secenekler_veri = getString126;
		_bankaevrakdataset.dropbox4_secenekler_yazi = getString125;
		_bankaevrakdataset.dropbox5_secenekler_veri = getString130;
		_bankaevrakdataset.dropbox5_secenekler_yazi = getString129;
		_bankaevrakdataset.dropbox6_secenekler_veri = getString134;
		_bankaevrakdataset.dropbox6_secenekler_yazi = getString133;
		_bankaevrakdataset.dropbox7_secenekler_veri = getString138;
		_bankaevrakdataset.dropbox7_secenekler_yazi = getString137;
		_bankaevrakdataset.dropbox8_secenekler_veri = getString142;
		_bankaevrakdataset.dropbox8_secenekler_yazi = getString141;
		_bankaevrakdataset.dropbox9_secenekler_veri = getString146;
		_bankaevrakdataset.dropbox9_secenekler_yazi = getString145;
		_bankaevrakdataset.dropbox10_secenekler_veri = getString150;
		_bankaevrakdataset.dropbox10_secenekler_yazi = getString149;
		string text = "[cari_har_rec_no] [int] NOT NULL,";
		string text2 = "cari_har_rec_no";
		if (AppBase.MikroVersiyonu >= 16)
		{
			text = "[cari_har_guid] [uniqueidentifier] NOT NULL,";
			text2 = "cari_har_guid";
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		string commandText = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[_FORA_BANKA_AKTARIM_EK_BILGILER]')) BEGIN CREATE TABLE [dbo].[_FORA_BANKA_AKTARIM_EK_BILGILER]([ID] [int] IDENTITY(1,1) NOT NULL," + text + "[Tarih] [datetime] NOT NULL,[metin1] [nvarchar](50) NULL,[metin2] [nvarchar](50) NULL,[metin3] [nvarchar](50) NULL,[metin4] [nvarchar](50) NULL,[metin5] [nvarchar](50) NULL,[metin6] [nvarchar](50) NULL,[metin7] [nvarchar](50) NULL,[metin8] [nvarchar](50) NULL,[metin9] [nvarchar](50) NULL,[metin10] [nvarchar](50) NULL,[metin11] [nvarchar](50) NULL,[metin12] [nvarchar](50) NULL,[metin13] [nvarchar](50) NULL,[metin14] [nvarchar](50) NULL,[metin15] [nvarchar](50) NULL,[metin16] [nvarchar](50) NULL,[metin17] [nvarchar](50) NULL,[metin18] [nvarchar](50) NULL,[metin19] [nvarchar](50) NULL,[metin20] [nvarchar](50) NULL,[metin21] [nvarchar](50) NULL,[metin22] [nvarchar](50) NULL,[metin23] [nvarchar](50) NULL,[metin24] [nvarchar](50) NULL,[metin25] [nvarchar](50) NULL,[metin26] [nvarchar](50) NULL,[metin27] [nvarchar](50) NULL,[metin28] [nvarchar](50) NULL,[metin29] [nvarchar](50) NULL,[metin30] [nvarchar](50) NULL,[metin31] [nvarchar](50) NULL,[metin32] [nvarchar](50) NULL,[metin33] [nvarchar](50) NULL,[metin34] [nvarchar](50) NULL,[metin35] [nvarchar](50) NULL,[metin36] [nvarchar](50) NULL,[metin37] [nvarchar](50) NULL,[metin38] [nvarchar](50) NULL,[metin39] [nvarchar](50) NULL,[metin40] [nvarchar](50) NULL,[metin41] [nvarchar](50) NULL,[metin42] [nvarchar](50) NULL,[metin43] [nvarchar](50) NULL,[metin44] [nvarchar](50) NULL,[metin45] [nvarchar](50) NULL,[metin46] [nvarchar](50) NULL,[metin47] [nvarchar](50) NULL,[metin48] [nvarchar](50) NULL,[metin49] [nvarchar](50) NULL,[metin50] [nvarchar](50) NULL,[dropbox1] [nvarchar](50) NULL,[dropbox2] [nvarchar](50) NULL,[dropbox3] [nvarchar](50) NULL,[dropbox4] [nvarchar](50) NULL,[dropbox5] [nvarchar](50) NULL,[dropbox6] [nvarchar](50) NULL,[dropbox7] [nvarchar](50) NULL,[dropbox8] [nvarchar](50) NULL,[dropbox9] [nvarchar](50) NULL,[dropbox10] [nvarchar](50) NULL,[checkbox1] [bit] NULL,[checkbox2] [bit] NULL,[checkbox3] [bit] NULL,[checkbox4] [bit] NULL,[checkbox5] [bit] NULL,[checkbox6] [bit] NULL,[checkbox7] [bit] NULL,[checkbox8] [bit] NULL,[checkbox9] [bit] NULL,[checkbox10] [bit] NULL, CONSTRAINT [PK__FORA_BANKA_AKTARIM_EK_BILGILER] PRIMARY KEY CLUSTERED  ([ID] ASC ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] ) ON [PRIMARY] CREATE NONCLUSTERED INDEX [01] ON [dbo].[_FORA_BANKA_AKTARIM_EK_BILGILER] ([" + text2 + "] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [02] ON [dbo].[_FORA_BANKA_AKTARIM_EK_BILGILER] ([Tarih] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] SELECT 1 END ELSE SELECT 0";
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.ExecuteScalar();
		}
		sqlDB.ConnectionClose();
		using StreamReader streamReader = new StreamReader(_bankaevrakdataset._DosyaAdi, Encoding.GetEncoding("windows-1254"));
		int num = 0;
		string text3;
		while ((text3 = streamReader.ReadLine()) != null)
		{
			bool flag = true;
			if (getBoolean && num < getInt3)
			{
				flag = false;
			}
			if (getBoolean2 && !text3.StartsWith(getString9))
			{
				flag = false;
			}
			if (flag)
			{
				aktarimSatirlar = new AktarimSatirlar();
				aktarimSatirlar.Aciklama = getString2;
				aktarimSatirlar.Cinsi = getInt;
				aktarimSatirlar.EvrakSrmMerkeziKodu = getString3;
				aktarimSatirlar.EvrakTipi = getInt2;
				aktarimSatirlar.HesapKodu = getString4;
				aktarimSatirlar.FaturaHesapKodu = getString5;
				aktarimSatirlar.FaturaMiktari = getDouble;
				aktarimSatirlar.FaturaProje = getString6;
				aktarimSatirlar.FaturaSeri = getString7;
				aktarimSatirlar.FaturaSorumlulukMerkezi = getString8;
				string bV_Unvan = "";
				string bV_Unvan2 = "";
				string bV_Adres = "";
				string bV_Mahalle = "";
				string bV_Ilce = "";
				string bV_Il = "";
				string bV_Ulke = "";
				string bV_PostaKodu = "";
				string bV_Telefon = "";
				string bV_EPosta = "";
				string bV_TcVergiNo = "";
				string bV_Aciklama = "";
				string bV_HesapNo = "";
				DateTime evrakTarihi = DateTime.Now;
				double num2 = 0.0;
				string metin = "";
				string metin2 = "";
				string metin3 = "";
				string metin4 = "";
				string metin5 = "";
				string metin6 = "";
				string metin7 = "";
				string metin8 = "";
				string metin9 = "";
				string metin10 = "";
				string metin11 = "";
				string metin12 = "";
				string metin13 = "";
				string metin14 = "";
				string metin15 = "";
				string metin16 = "";
				string metin17 = "";
				string metin18 = "";
				string metin19 = "";
				string metin20 = "";
				string metin21 = "";
				string metin22 = "";
				string metin23 = "";
				string metin24 = "";
				string metin25 = "";
				string metin26 = "";
				string metin27 = "";
				string metin28 = "";
				string metin29 = "";
				string metin30 = "";
				string metin31 = "";
				string metin32 = "";
				string metin33 = "";
				string metin34 = "";
				string metin35 = "";
				string metin36 = "";
				string metin37 = "";
				string metin38 = "";
				string metin39 = "";
				string metin40 = "";
				string metin41 = "";
				string metin42 = "";
				string metin43 = "";
				string metin44 = "";
				string metin45 = "";
				string metin46 = "";
				string metin47 = "";
				string metin48 = "";
				string metin49 = "";
				string metin50 = "";
				string dropbox = "";
				string dropbox2 = "";
				string dropbox3 = "";
				string dropbox4 = "";
				string dropbox5 = "";
				string dropbox6 = "";
				string dropbox7 = "";
				string dropbox8 = "";
				string dropbox9 = "";
				string dropbox10 = "";
				bool checkbox = false;
				bool checkbox2 = false;
				bool checkbox3 = false;
				bool checkbox4 = false;
				bool checkbox5 = false;
				bool checkbox6 = false;
				bool checkbox7 = false;
				bool checkbox8 = false;
				bool checkbox9 = false;
				bool checkbox10 = false;
				if (getBoolean3)
				{
					try
					{
						string[] array = text3.Split(getString10.ToCharArray()[0]);
						if (getInt4 != 0)
						{
							bV_Unvan = array[getInt4 - 1];
						}
						if (getInt6 != 0)
						{
							bV_Unvan2 = array[getInt6 - 1];
						}
						if (getInt8 != 0)
						{
							bV_Adres = array[getInt8 - 1];
						}
						if (getInt10 != 0)
						{
							bV_Mahalle = array[getInt10 - 1];
						}
						if (getInt12 != 0)
						{
							bV_Ilce = array[getInt12 - 1];
						}
						if (getInt14 != 0)
						{
							bV_Il = array[getInt14 - 1];
						}
						if (getInt16 != 0)
						{
							bV_Ulke = array[getInt16 - 1];
						}
						if (getInt18 != 0)
						{
							bV_PostaKodu = array[getInt18 - 1];
						}
						if (getInt20 != 0)
						{
							bV_Telefon = array[getInt20 - 1];
						}
						if (getInt22 != 0)
						{
							bV_EPosta = array[getInt22 - 1];
						}
						if (getInt24 != 0)
						{
							bV_TcVergiNo = array[getInt24 - 1];
						}
						if (getInt26 != 0)
						{
							bV_Aciklama = array[getInt26 - 1];
						}
						if (getInt28 != 0)
						{
							bV_HesapNo = array[getInt28 - 1];
						}
						if (getInt30 != 0)
						{
							evrakTarihi = DateTime.Parse(array[getInt30 - 1]);
						}
						if (getInt36 != 0)
						{
							num2 = double.Parse(array[getInt36 - 1]);
						}
						metin = getString12;
						if (getInt38 != 0)
						{
							metin = array[getInt38 - 1];
						}
						metin2 = getString14;
						if (getInt40 != 0)
						{
							metin2 = array[getInt40 - 1];
						}
						metin3 = getString16;
						if (getInt42 != 0)
						{
							metin3 = array[getInt42 - 1];
						}
						metin4 = getString18;
						if (getInt44 != 0)
						{
							metin4 = array[getInt44 - 1];
						}
						metin5 = getString20;
						if (getInt46 != 0)
						{
							metin5 = array[getInt46 - 1];
						}
						metin6 = getString22;
						if (getInt48 != 0)
						{
							metin6 = array[getInt48 - 1];
						}
						metin7 = getString24;
						if (getInt50 != 0)
						{
							metin7 = array[getInt50 - 1];
						}
						metin8 = getString26;
						if (getInt52 != 0)
						{
							metin8 = array[getInt52 - 1];
						}
						metin9 = getString28;
						if (getInt54 != 0)
						{
							metin9 = array[getInt54 - 1];
						}
						metin10 = getString30;
						if (getInt56 != 0)
						{
							metin10 = array[getInt56 - 1];
						}
						metin11 = getString32;
						if (getInt58 != 0)
						{
							metin11 = array[getInt58 - 1];
						}
						metin12 = getString34;
						if (getInt60 != 0)
						{
							metin12 = array[getInt60 - 1];
						}
						metin13 = getString36;
						if (getInt62 != 0)
						{
							metin13 = array[getInt62 - 1];
						}
						metin14 = getString38;
						if (getInt64 != 0)
						{
							metin14 = array[getInt64 - 1];
						}
						metin15 = getString40;
						if (getInt66 != 0)
						{
							metin15 = array[getInt66 - 1];
						}
						metin16 = getString42;
						if (getInt68 != 0)
						{
							metin16 = array[getInt68 - 1];
						}
						metin17 = getString44;
						if (getInt70 != 0)
						{
							metin17 = array[getInt70 - 1];
						}
						metin18 = getString46;
						if (getInt72 != 0)
						{
							metin18 = array[getInt72 - 1];
						}
						metin19 = getString48;
						if (getInt74 != 0)
						{
							metin19 = array[getInt74 - 1];
						}
						metin20 = getString50;
						if (getInt76 != 0)
						{
							metin20 = array[getInt76 - 1];
						}
						metin21 = getString52;
						if (getInt78 != 0)
						{
							metin21 = array[getInt78 - 1];
						}
						metin22 = getString54;
						if (getInt80 != 0)
						{
							metin22 = array[getInt80 - 1];
						}
						metin23 = getString56;
						if (getInt82 != 0)
						{
							metin23 = array[getInt82 - 1];
						}
						metin24 = getString58;
						if (getInt84 != 0)
						{
							metin24 = array[getInt84 - 1];
						}
						metin25 = getString60;
						if (getInt86 != 0)
						{
							metin25 = array[getInt86 - 1];
						}
						metin26 = getString62;
						if (getInt88 != 0)
						{
							metin26 = array[getInt88 - 1];
						}
						metin27 = getString64;
						if (getInt90 != 0)
						{
							metin27 = array[getInt90 - 1];
						}
						metin28 = getString66;
						if (getInt92 != 0)
						{
							metin28 = array[getInt92 - 1];
						}
						metin29 = getString68;
						if (getInt94 != 0)
						{
							metin29 = array[getInt94 - 1];
						}
						metin30 = getString70;
						if (getInt96 != 0)
						{
							metin30 = array[getInt96 - 1];
						}
						metin31 = getString72;
						if (getInt98 != 0)
						{
							metin31 = array[getInt98 - 1];
						}
						metin32 = getString74;
						if (getInt100 != 0)
						{
							metin32 = array[getInt100 - 1];
						}
						metin33 = getString76;
						if (getInt102 != 0)
						{
							metin33 = array[getInt102 - 1];
						}
						metin34 = getString78;
						if (getInt104 != 0)
						{
							metin34 = array[getInt104 - 1];
						}
						metin35 = getString80;
						if (getInt106 != 0)
						{
							metin35 = array[getInt106 - 1];
						}
						metin36 = getString82;
						if (getInt108 != 0)
						{
							metin36 = array[getInt108 - 1];
						}
						metin37 = getString84;
						if (getInt110 != 0)
						{
							metin37 = array[getInt110 - 1];
						}
						metin38 = getString86;
						if (getInt112 != 0)
						{
							metin38 = array[getInt112 - 1];
						}
						metin39 = getString88;
						if (getInt114 != 0)
						{
							metin39 = array[getInt114 - 1];
						}
						metin40 = getString90;
						if (getInt116 != 0)
						{
							metin40 = array[getInt116 - 1];
						}
						metin41 = getString92;
						if (getInt118 != 0)
						{
							metin41 = array[getInt118 - 1];
						}
						metin42 = getString94;
						if (getInt120 != 0)
						{
							metin42 = array[getInt120 - 1];
						}
						metin43 = getString96;
						if (getInt122 != 0)
						{
							metin43 = array[getInt122 - 1];
						}
						metin44 = getString98;
						if (getInt124 != 0)
						{
							metin44 = array[getInt124 - 1];
						}
						metin45 = getString100;
						if (getInt126 != 0)
						{
							metin45 = array[getInt126 - 1];
						}
						metin46 = getString102;
						if (getInt128 != 0)
						{
							metin46 = array[getInt128 - 1];
						}
						metin47 = getString104;
						if (getInt130 != 0)
						{
							metin47 = array[getInt130 - 1];
						}
						metin48 = getString106;
						if (getInt132 != 0)
						{
							metin48 = array[getInt132 - 1];
						}
						metin49 = getString108;
						if (getInt134 != 0)
						{
							metin49 = array[getInt134 - 1];
						}
						metin50 = getString110;
						if (getInt136 != 0)
						{
							metin50 = array[getInt136 - 1];
						}
						dropbox = getString112;
						if (getInt138 != 0)
						{
							dropbox = array[getInt138 - 1];
						}
						dropbox2 = getString116;
						if (getInt140 != 0)
						{
							dropbox2 = array[getInt140 - 1];
						}
						dropbox3 = getString120;
						if (getInt142 != 0)
						{
							dropbox3 = array[getInt142 - 1];
						}
						dropbox4 = getString124;
						if (getInt144 != 0)
						{
							dropbox4 = array[getInt144 - 1];
						}
						dropbox5 = getString128;
						if (getInt146 != 0)
						{
							dropbox5 = array[getInt146 - 1];
						}
						dropbox6 = getString132;
						if (getInt148 != 0)
						{
							dropbox6 = array[getInt148 - 1];
						}
						dropbox7 = getString136;
						if (getInt150 != 0)
						{
							dropbox7 = array[getInt150 - 1];
						}
						dropbox8 = getString140;
						if (getInt152 != 0)
						{
							dropbox8 = array[getInt152 - 1];
						}
						dropbox9 = getString144;
						if (getInt154 != 0)
						{
							dropbox9 = array[getInt154 - 1];
						}
						dropbox10 = getString148;
						if (getInt156 != 0)
						{
							dropbox10 = array[getInt156 - 1];
						}
						checkbox = getBoolean5;
						if (getInt158 != 0)
						{
							string text4 = array[getInt158 - 1].ToUpper();
							switch (text4)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
							case "TRUE":
								checkbox = true;
								break;
							}
							switch (text4)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
							case "FALSE":
								checkbox = false;
								break;
							}
						}
						checkbox2 = getBoolean6;
						if (getInt160 != 0)
						{
							string text5 = array[getInt160 - 1].ToUpper();
							switch (text5)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
							case "TRUE":
								checkbox2 = true;
								break;
							}
							switch (text5)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
							case "FALSE":
								checkbox2 = false;
								break;
							}
						}
						checkbox3 = getBoolean7;
						if (getInt162 != 0)
						{
							string text6 = array[getInt162 - 1].ToUpper();
							switch (text6)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
							case "TRUE":
								checkbox3 = true;
								break;
							}
							switch (text6)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
							case "FALSE":
								checkbox3 = false;
								break;
							}
						}
						checkbox4 = getBoolean8;
						if (getInt164 != 0)
						{
							string text7 = array[getInt164 - 1].ToUpper();
							switch (text7)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
							case "TRUE":
								checkbox4 = true;
								break;
							}
							switch (text7)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
							case "FALSE":
								checkbox4 = false;
								break;
							}
						}
						checkbox5 = getBoolean9;
						if (getInt166 != 0)
						{
							string text8 = array[getInt166 - 1].ToUpper();
							switch (text8)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
							case "TRUE":
								checkbox5 = true;
								break;
							}
							switch (text8)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
							case "FALSE":
								checkbox5 = false;
								break;
							}
						}
						checkbox6 = getBoolean10;
						if (getInt168 != 0)
						{
							string text9 = array[getInt168 - 1].ToUpper();
							switch (text9)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
							case "TRUE":
								checkbox6 = true;
								break;
							}
							switch (text9)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
							case "FALSE":
								checkbox6 = false;
								break;
							}
						}
						checkbox7 = getBoolean11;
						if (getInt170 != 0)
						{
							string text10 = array[getInt170 - 1].ToUpper();
							switch (text10)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
							case "TRUE":
								checkbox7 = true;
								break;
							}
							switch (text10)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
							case "FALSE":
								checkbox7 = false;
								break;
							}
						}
						checkbox8 = getBoolean12;
						if (getInt172 != 0)
						{
							string text11 = array[getInt172 - 1].ToUpper();
							switch (text11)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
							case "TRUE":
								checkbox8 = true;
								break;
							}
							switch (text11)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
							case "FALSE":
								checkbox8 = false;
								break;
							}
						}
						checkbox9 = getBoolean13;
						if (getInt174 != 0)
						{
							string text12 = array[getInt174 - 1].ToUpper();
							switch (text12)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
							case "TRUE":
								checkbox9 = true;
								break;
							}
							switch (text12)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
							case "FALSE":
								checkbox9 = false;
								break;
							}
						}
						checkbox10 = getBoolean14;
						if (getInt176 != 0)
						{
							string text13 = array[getInt176 - 1].ToUpper();
							switch (text13)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
							case "TRUE":
								checkbox10 = true;
								break;
							}
							switch (text13)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
							case "FALSE":
								checkbox10 = false;
								break;
							}
						}
					}
					catch
					{
					}
				}
				else
				{
					try
					{
						if (getInt5 != 0)
						{
							bV_Unvan = text3.Substring(getInt4 - 1, getInt5).Trim();
						}
						if (getInt7 != 0)
						{
							bV_Unvan2 = text3.Substring(getInt6 - 1, getInt7).Trim();
						}
						if (getInt9 != 0)
						{
							bV_Adres = text3.Substring(getInt8 - 1, getInt9).Trim();
						}
						if (getInt11 != 0)
						{
							bV_Mahalle = text3.Substring(getInt10 - 1, getInt11).Trim();
						}
						if (getInt13 != 0)
						{
							bV_Ilce = text3.Substring(getInt12 - 1, getInt13).Trim();
						}
						if (getInt15 != 0)
						{
							bV_Il = text3.Substring(getInt14 - 1, getInt15).Trim();
						}
						if (getInt17 != 0)
						{
							bV_Ulke = text3.Substring(getInt16 - 1, getInt17).Trim();
						}
						if (getInt19 != 0)
						{
							bV_PostaKodu = text3.Substring(getInt18 - 1, getInt19).Trim();
						}
						if (getInt21 != 0)
						{
							bV_Telefon = text3.Substring(getInt20 - 1, getInt21).Trim();
						}
						if (getInt23 != 0)
						{
							bV_EPosta = text3.Substring(getInt22 - 1, getInt23).Trim();
						}
						if (getInt25 != 0)
						{
							bV_TcVergiNo = text3.Substring(getInt24 - 1, getInt25).Trim();
						}
						if (getInt27 != 0)
						{
							bV_Aciklama = text3.Substring(getInt26 - 1, getInt27).Trim();
						}
						if (getInt29 != 0)
						{
							bV_HesapNo = text3.Substring(getInt28 - 1, getInt29).Trim();
						}
						if (getInt31 != 0)
						{
							int num3 = int.Parse(text3.Substring(getInt30 - 1, getInt31).Trim());
							if (getInt31 == 2)
							{
								num3 += 2000;
							}
							int month = int.Parse(text3.Substring(getInt32 - 1, getInt33).Trim());
							int day = int.Parse(text3.Substring(getInt34 - 1, getInt35).Trim());
							evrakTarihi = new DateTime(num3, month, day);
						}
						if (getInt37 != 0)
						{
							num2 = double.Parse(text3.Substring(getInt36 - 1, getInt37).Trim().Replace(".", ","));
						}
						metin = getString12;
						if (getInt39 != 0)
						{
							metin = text3.Substring(getInt38 - 1, getInt39).Trim();
						}
						metin2 = getString14;
						if (getInt41 != 0)
						{
							metin2 = text3.Substring(getInt40 - 1, getInt41).Trim();
						}
						metin3 = getString16;
						if (getInt43 != 0)
						{
							metin3 = text3.Substring(getInt42 - 1, getInt43).Trim();
						}
						metin4 = getString18;
						if (getInt45 != 0)
						{
							metin4 = text3.Substring(getInt44 - 1, getInt45).Trim();
						}
						metin5 = getString20;
						if (getInt47 != 0)
						{
							metin5 = text3.Substring(getInt46 - 1, getInt47).Trim();
						}
						metin6 = getString22;
						if (getInt49 != 0)
						{
							metin6 = text3.Substring(getInt48 - 1, getInt49).Trim();
						}
						metin7 = getString24;
						if (getInt51 != 0)
						{
							metin7 = text3.Substring(getInt50 - 1, getInt51).Trim();
						}
						metin8 = getString26;
						if (getInt53 != 0)
						{
							metin8 = text3.Substring(getInt52 - 1, getInt53).Trim();
						}
						metin9 = getString28;
						if (getInt55 != 0)
						{
							metin9 = text3.Substring(getInt54 - 1, getInt55).Trim();
						}
						metin10 = getString30;
						if (getInt57 != 0)
						{
							metin10 = text3.Substring(getInt56 - 1, getInt57).Trim();
						}
						metin11 = getString32;
						if (getInt59 != 0)
						{
							metin11 = text3.Substring(getInt58 - 1, getInt59).Trim();
						}
						metin12 = getString34;
						if (getInt61 != 0)
						{
							metin12 = text3.Substring(getInt60 - 1, getInt61).Trim();
						}
						metin13 = getString36;
						if (getInt63 != 0)
						{
							metin13 = text3.Substring(getInt62 - 1, getInt63).Trim();
						}
						metin14 = getString38;
						if (getInt65 != 0)
						{
							metin14 = text3.Substring(getInt64 - 1, getInt65).Trim();
						}
						metin15 = getString40;
						if (getInt67 != 0)
						{
							metin15 = text3.Substring(getInt66 - 1, getInt67).Trim();
						}
						metin16 = getString42;
						if (getInt69 != 0)
						{
							metin16 = text3.Substring(getInt68 - 1, getInt69).Trim();
						}
						metin17 = getString44;
						if (getInt71 != 0)
						{
							metin17 = text3.Substring(getInt70 - 1, getInt71).Trim();
						}
						metin18 = getString46;
						if (getInt73 != 0)
						{
							metin18 = text3.Substring(getInt72 - 1, getInt73).Trim();
						}
						metin19 = getString48;
						if (getInt75 != 0)
						{
							metin19 = text3.Substring(getInt74 - 1, getInt75).Trim();
						}
						metin20 = getString50;
						if (getInt77 != 0)
						{
							metin20 = text3.Substring(getInt76 - 1, getInt77).Trim();
						}
						metin21 = getString52;
						if (getInt79 != 0)
						{
							metin21 = text3.Substring(getInt78 - 1, getInt79).Trim();
						}
						metin22 = getString54;
						if (getInt81 != 0)
						{
							metin22 = text3.Substring(getInt80 - 1, getInt81).Trim();
						}
						metin23 = getString56;
						if (getInt83 != 0)
						{
							metin23 = text3.Substring(getInt82 - 1, getInt83).Trim();
						}
						metin24 = getString58;
						if (getInt85 != 0)
						{
							metin24 = text3.Substring(getInt84 - 1, getInt85).Trim();
						}
						metin25 = getString60;
						if (getInt87 != 0)
						{
							metin25 = text3.Substring(getInt86 - 1, getInt87).Trim();
						}
						metin26 = getString62;
						if (getInt89 != 0)
						{
							metin26 = text3.Substring(getInt88 - 1, getInt89).Trim();
						}
						metin27 = getString64;
						if (getInt91 != 0)
						{
							metin27 = text3.Substring(getInt90 - 1, getInt91).Trim();
						}
						metin28 = getString66;
						if (getInt93 != 0)
						{
							metin28 = text3.Substring(getInt92 - 1, getInt93).Trim();
						}
						metin29 = getString68;
						if (getInt95 != 0)
						{
							metin29 = text3.Substring(getInt94 - 1, getInt95).Trim();
						}
						metin30 = getString70;
						if (getInt97 != 0)
						{
							metin30 = text3.Substring(getInt96 - 1, getInt97).Trim();
						}
						metin31 = getString72;
						if (getInt99 != 0)
						{
							metin31 = text3.Substring(getInt98 - 1, getInt99).Trim();
						}
						metin32 = getString74;
						if (getInt101 != 0)
						{
							metin32 = text3.Substring(getInt100 - 1, getInt101).Trim();
						}
						metin33 = getString76;
						if (getInt103 != 0)
						{
							metin33 = text3.Substring(getInt102 - 1, getInt103).Trim();
						}
						metin34 = getString78;
						if (getInt105 != 0)
						{
							metin34 = text3.Substring(getInt104 - 1, getInt105).Trim();
						}
						metin35 = getString80;
						if (getInt107 != 0)
						{
							metin35 = text3.Substring(getInt106 - 1, getInt107).Trim();
						}
						metin36 = getString82;
						if (getInt109 != 0)
						{
							metin36 = text3.Substring(getInt108 - 1, getInt109).Trim();
						}
						metin37 = getString84;
						if (getInt111 != 0)
						{
							metin37 = text3.Substring(getInt110 - 1, getInt111).Trim();
						}
						metin38 = getString86;
						if (getInt113 != 0)
						{
							metin38 = text3.Substring(getInt112 - 1, getInt113).Trim();
						}
						metin39 = getString88;
						if (getInt115 != 0)
						{
							metin39 = text3.Substring(getInt114 - 1, getInt115).Trim();
						}
						metin40 = getString90;
						if (getInt117 != 0)
						{
							metin40 = text3.Substring(getInt116 - 1, getInt117).Trim();
						}
						metin41 = getString92;
						if (getInt119 != 0)
						{
							metin41 = text3.Substring(getInt118 - 1, getInt119).Trim();
						}
						metin42 = getString94;
						if (getInt121 != 0)
						{
							metin42 = text3.Substring(getInt120 - 1, getInt121).Trim();
						}
						metin43 = getString96;
						if (getInt123 != 0)
						{
							metin43 = text3.Substring(getInt122 - 1, getInt123).Trim();
						}
						metin44 = getString98;
						if (getInt125 != 0)
						{
							metin44 = text3.Substring(getInt124 - 1, getInt125).Trim();
						}
						metin45 = getString100;
						if (getInt127 != 0)
						{
							metin45 = text3.Substring(getInt126 - 1, getInt127).Trim();
						}
						metin46 = getString102;
						if (getInt129 != 0)
						{
							metin46 = text3.Substring(getInt128 - 1, getInt129).Trim();
						}
						metin47 = getString104;
						if (getInt131 != 0)
						{
							metin47 = text3.Substring(getInt130 - 1, getInt131).Trim();
						}
						metin48 = getString106;
						if (getInt133 != 0)
						{
							metin48 = text3.Substring(getInt132 - 1, getInt133).Trim();
						}
						metin49 = getString108;
						if (getInt135 != 0)
						{
							metin49 = text3.Substring(getInt134 - 1, getInt135).Trim();
						}
						metin50 = getString110;
						if (getInt137 != 0)
						{
							metin50 = text3.Substring(getInt136 - 1, getInt137).Trim();
						}
						dropbox = getString112;
						if (getInt139 != 0)
						{
							dropbox = text3.Substring(getInt138 - 1, getInt139).Trim();
						}
						dropbox2 = getString116;
						if (getInt141 != 0)
						{
							dropbox2 = text3.Substring(getInt140 - 1, getInt141).Trim();
						}
						dropbox3 = getString120;
						if (getInt143 != 0)
						{
							dropbox3 = text3.Substring(getInt142 - 1, getInt143).Trim();
						}
						dropbox4 = getString124;
						if (getInt145 != 0)
						{
							dropbox4 = text3.Substring(getInt144 - 1, getInt145).Trim();
						}
						dropbox5 = getString128;
						if (getInt147 != 0)
						{
							dropbox5 = text3.Substring(getInt146 - 1, getInt147).Trim();
						}
						dropbox6 = getString132;
						if (getInt149 != 0)
						{
							dropbox6 = text3.Substring(getInt148 - 1, getInt149).Trim();
						}
						dropbox7 = getString136;
						if (getInt151 != 0)
						{
							dropbox7 = text3.Substring(getInt150 - 1, getInt151).Trim();
						}
						dropbox8 = getString140;
						if (getInt153 != 0)
						{
							dropbox8 = text3.Substring(getInt152 - 1, getInt153).Trim();
						}
						dropbox9 = getString144;
						if (getInt155 != 0)
						{
							dropbox9 = text3.Substring(getInt154 - 1, getInt155).Trim();
						}
						dropbox10 = getString148;
						if (getInt157 != 0)
						{
							dropbox10 = text3.Substring(getInt156 - 1, getInt157).Trim();
						}
						checkbox = getBoolean5;
						if (getInt159 != 0)
						{
							string text14 = text3.Substring(getInt158 - 1, getInt159).Trim();
							switch (text14)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
								checkbox = true;
								break;
							}
							switch (text14)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
								checkbox = false;
								break;
							}
						}
						checkbox2 = getBoolean6;
						if (getInt161 != 0)
						{
							string text15 = text3.Substring(getInt160 - 1, getInt161).Trim();
							switch (text15)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
								checkbox2 = true;
								break;
							}
							switch (text15)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
								checkbox2 = false;
								break;
							}
						}
						checkbox3 = getBoolean7;
						if (getInt163 != 0)
						{
							string text16 = text3.Substring(getInt162 - 1, getInt163).Trim();
							switch (text16)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
								checkbox3 = true;
								break;
							}
							switch (text16)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
								checkbox3 = false;
								break;
							}
						}
						checkbox4 = getBoolean8;
						if (getInt165 != 0)
						{
							string text17 = text3.Substring(getInt164 - 1, getInt165).Trim();
							switch (text17)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
								checkbox4 = true;
								break;
							}
							switch (text17)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
								checkbox4 = false;
								break;
							}
						}
						checkbox5 = getBoolean9;
						if (getInt167 != 0)
						{
							string text18 = text3.Substring(getInt166 - 1, getInt167).Trim();
							switch (text18)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
								checkbox5 = true;
								break;
							}
							switch (text18)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
								checkbox5 = false;
								break;
							}
						}
						checkbox6 = getBoolean10;
						if (getInt169 != 0)
						{
							string text19 = text3.Substring(getInt168 - 1, getInt169).Trim();
							switch (text19)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
								checkbox6 = true;
								break;
							}
							switch (text19)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
								checkbox6 = false;
								break;
							}
						}
						checkbox7 = getBoolean11;
						if (getInt171 != 0)
						{
							string text20 = text3.Substring(getInt170 - 1, getInt171).Trim();
							switch (text20)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
								checkbox7 = true;
								break;
							}
							switch (text20)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
								checkbox7 = false;
								break;
							}
						}
						checkbox8 = getBoolean12;
						if (getInt173 != 0)
						{
							string text21 = text3.Substring(getInt172 - 1, getInt173).Trim();
							switch (text21)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
								checkbox8 = true;
								break;
							}
							switch (text21)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
								checkbox8 = false;
								break;
							}
						}
						checkbox9 = getBoolean13;
						if (getInt175 != 0)
						{
							string text22 = text3.Substring(getInt174 - 1, getInt175).Trim();
							switch (text22)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
								checkbox9 = true;
								break;
							}
							switch (text22)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
								checkbox9 = false;
								break;
							}
						}
						checkbox10 = getBoolean14;
						if (getInt177 != 0)
						{
							string text23 = text3.Substring(getInt176 - 1, getInt177).Trim();
							switch (text23)
							{
							case "1":
							case "Y":
							case "YES":
							case "EVET":
							case "E":
							case "VAR":
								checkbox10 = true;
								break;
							}
							switch (text23)
							{
							case "0":
							case "N":
							case "NO":
							case "HAYIR":
							case "H":
							case "YOK":
								checkbox10 = false;
								break;
							}
						}
					}
					catch
					{
					}
				}
				if (num2 < 0.0)
				{
					num2 *= -1.0;
					aktarimSatirlar.EvrakTipi = enum_GenelEvrakTipleri.GidenHavale;
				}
				aktarimSatirlar.BV_Unvan = bV_Unvan;
				aktarimSatirlar.BV_Unvan2 = bV_Unvan2;
				aktarimSatirlar.BV_Adres = bV_Adres;
				aktarimSatirlar.BV_Mahalle = bV_Mahalle;
				aktarimSatirlar.BV_Ilce = bV_Ilce;
				aktarimSatirlar.BV_Il = bV_Il;
				aktarimSatirlar.BV_Ulke = bV_Ulke;
				aktarimSatirlar.BV_PostaKodu = bV_PostaKodu;
				aktarimSatirlar.BV_Telefon = bV_Telefon;
				aktarimSatirlar.BV_EPosta = bV_EPosta;
				aktarimSatirlar.BV_TcVergiNo = bV_TcVergiNo;
				aktarimSatirlar.BV_Aciklama = bV_Aciklama;
				aktarimSatirlar.BV_HesapNo = bV_HesapNo;
				aktarimSatirlar.EvrakTarihi = evrakTarihi;
				aktarimSatirlar.Tutar = num2;
				aktarimSatirlar.mikro_disi_ek_bilgileri_kullan = getBoolean4;
				aktarimSatirlar.metin1 = metin;
				aktarimSatirlar.metin2 = metin2;
				aktarimSatirlar.metin3 = metin3;
				aktarimSatirlar.metin4 = metin4;
				aktarimSatirlar.metin5 = metin5;
				aktarimSatirlar.metin6 = metin6;
				aktarimSatirlar.metin7 = metin7;
				aktarimSatirlar.metin8 = metin8;
				aktarimSatirlar.metin9 = metin9;
				aktarimSatirlar.metin10 = metin10;
				aktarimSatirlar.metin11 = metin11;
				aktarimSatirlar.metin12 = metin12;
				aktarimSatirlar.metin13 = metin13;
				aktarimSatirlar.metin14 = metin14;
				aktarimSatirlar.metin15 = metin15;
				aktarimSatirlar.metin16 = metin16;
				aktarimSatirlar.metin17 = metin17;
				aktarimSatirlar.metin18 = metin18;
				aktarimSatirlar.metin19 = metin19;
				aktarimSatirlar.metin20 = metin20;
				aktarimSatirlar.metin21 = metin21;
				aktarimSatirlar.metin22 = metin22;
				aktarimSatirlar.metin23 = metin23;
				aktarimSatirlar.metin24 = metin24;
				aktarimSatirlar.metin25 = metin25;
				aktarimSatirlar.metin26 = metin26;
				aktarimSatirlar.metin27 = metin27;
				aktarimSatirlar.metin28 = metin28;
				aktarimSatirlar.metin29 = metin29;
				aktarimSatirlar.metin30 = metin30;
				aktarimSatirlar.metin31 = metin31;
				aktarimSatirlar.metin32 = metin32;
				aktarimSatirlar.metin33 = metin33;
				aktarimSatirlar.metin34 = metin34;
				aktarimSatirlar.metin35 = metin35;
				aktarimSatirlar.metin36 = metin36;
				aktarimSatirlar.metin37 = metin37;
				aktarimSatirlar.metin38 = metin38;
				aktarimSatirlar.metin39 = metin39;
				aktarimSatirlar.metin40 = metin40;
				aktarimSatirlar.metin41 = metin41;
				aktarimSatirlar.metin42 = metin42;
				aktarimSatirlar.metin43 = metin43;
				aktarimSatirlar.metin44 = metin44;
				aktarimSatirlar.metin45 = metin45;
				aktarimSatirlar.metin46 = metin46;
				aktarimSatirlar.metin47 = metin47;
				aktarimSatirlar.metin48 = metin48;
				aktarimSatirlar.metin49 = metin49;
				aktarimSatirlar.metin50 = metin50;
				aktarimSatirlar.dropbox1 = dropbox;
				aktarimSatirlar.dropbox2 = dropbox2;
				aktarimSatirlar.dropbox3 = dropbox3;
				aktarimSatirlar.dropbox4 = dropbox4;
				aktarimSatirlar.dropbox5 = dropbox5;
				aktarimSatirlar.dropbox6 = dropbox6;
				aktarimSatirlar.dropbox7 = dropbox7;
				aktarimSatirlar.dropbox8 = dropbox8;
				aktarimSatirlar.dropbox9 = dropbox9;
				aktarimSatirlar.dropbox10 = dropbox10;
				aktarimSatirlar.checkbox1 = checkbox;
				aktarimSatirlar.checkbox2 = checkbox2;
				aktarimSatirlar.checkbox3 = checkbox3;
				aktarimSatirlar.checkbox4 = checkbox4;
				aktarimSatirlar.checkbox5 = checkbox5;
				aktarimSatirlar.checkbox6 = checkbox6;
				aktarimSatirlar.checkbox7 = checkbox7;
				aktarimSatirlar.checkbox8 = checkbox8;
				aktarimSatirlar.checkbox9 = checkbox9;
				aktarimSatirlar.checkbox10 = checkbox10;
				_tempSatirlar.Add(aktarimSatirlar);
			}
			num++;
		}
	}

	private void KritereGoreIslemYap(AktarimSatirlar satir)
	{
		string[] array = _bankagenelparametreler._GetParametre("KriterListesi")._GetString.Split(',');
		foreach (string text in array)
		{
			Parametreler parametreler = ParametrelerDefault.BankaAktarimKriter(text);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "BankaAktarim", "", "Kriter", text);
			string getString = parametreler._GetParametre("AramaIslemKodu")._GetString;
			bool getBoolean = parametreler._GetParametre("AramaIslemKoduKullan")._GetBoolean;
			string getString2 = parametreler._GetParametre("AramaAciklama")._GetString;
			bool getBoolean2 = parametreler._GetParametre("AramaAciklamaKullan")._GetBoolean;
			int getInt = parametreler._GetParametre("AramaMinimumTutar")._GetInt;
			bool getBoolean3 = parametreler._GetParametre("AramaMinimumTutarKullan")._GetBoolean;
			int getInt2 = parametreler._GetParametre("AramaMaksimumTutar")._GetInt;
			bool getBoolean4 = parametreler._GetParametre("AramaMaksimumTutarKullan")._GetBoolean;
			bool flag = true;
			if (getBoolean && getString != satir.BV_IslemKodu)
			{
				flag = false;
			}
			if (getBoolean2 && !satir.BV_Aciklama.Contains(getString2))
			{
				flag = false;
			}
			if (getBoolean3 && (double)getInt > satir.Tutar)
			{
				flag = false;
			}
			if (getBoolean4 && (double)getInt2 < satir.Tutar)
			{
				flag = false;
			}
			if (!getBoolean2 && !getBoolean && !getBoolean3 && !getBoolean4)
			{
				flag = false;
			}
			if (!flag)
			{
				continue;
			}
			int getInt3 = parametreler._GetParametre("DegistirmeEvrakTipi")._GetInt;
			if (parametreler._GetParametre("DegistirmeEvrakTipiKullan")._GetBoolean)
			{
				satir.EvrakTipi = (enum_GenelEvrakTipleri)getInt3;
			}
			string getString3 = parametreler._GetParametre("DegistirmeEvrakSorumlulukMerkezi")._GetString;
			if (parametreler._GetParametre("DegistirmeEvrakSorumlulukMerkeziKullan")._GetBoolean)
			{
				satir.EvrakSrmMerkeziKodu = getString3;
			}
			int getInt4 = parametreler._GetParametre("DegistirmeSatirCinsi")._GetInt;
			if (parametreler._GetParametre("DegistirmeSatirCinsiKullan")._GetBoolean)
			{
				satir.Cinsi = (enum_BankaSatirCinsi)getInt4;
			}
			string getString4 = parametreler._GetParametre("DegistirmeHesapKodu")._GetString;
			if (parametreler._GetParametre("DegistirmeHesapKoduKullan")._GetBoolean)
			{
				satir.HesapKodu = getString4;
				switch (satir.Cinsi)
				{
				case enum_BankaSatirCinsi.CariHesap:
				{
					SqlDB sqlDB = new SqlDB();
					sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
					Cari cariByCariKod = CariData.GetCariByCariKod(sqlDB.Connection, satir.HesapKodu, AdreslerTemsilciyeGore: false, "");
					sqlDB.ConnectionClose();
					satir.HesapKodu = cariByCariKod.cari_kod;
					satir.GrupNo = 0;
					if (_banka.ban_doviz_cinsi == cariByCariKod.cari_doviz_cinsi)
					{
						satir.GrupNo = 0;
					}
					if (_banka.ban_doviz_cinsi == cariByCariKod.cari_doviz_cinsi1)
					{
						satir.GrupNo = 1;
					}
					if (_banka.ban_doviz_cinsi == cariByCariKod.cari_doviz_cinsi2)
					{
						satir.GrupNo = 2;
					}
					break;
				}
				case enum_BankaSatirCinsi.Banka:
					satir.GrupNo = 0;
					break;
				case enum_BankaSatirCinsi.CariPersonel:
					satir.GrupNo = 0;
					break;
				case enum_BankaSatirCinsi.Demirbas:
					satir.GrupNo = 0;
					break;
				case enum_BankaSatirCinsi.Hizmet:
					satir.GrupNo = 0;
					break;
				case enum_BankaSatirCinsi.Ithalat:
					satir.GrupNo = 0;
					break;
				case enum_BankaSatirCinsi.Kasa:
					satir.GrupNo = 0;
					break;
				case enum_BankaSatirCinsi.Masraf:
					satir.GrupNo = 0;
					break;
				case enum_BankaSatirCinsi.MuhasebeHesabi:
					satir.GrupNo = 0;
					break;
				case enum_BankaSatirCinsi.Personel:
					satir.GrupNo = 0;
					break;
				}
			}
			string getString5 = parametreler._GetParametre("DegistirmeFaturaSorumlulukMerkezi")._GetString;
			if (parametreler._GetParametre("DegistirmeFaturaSorumlulukMerkeziKullan")._GetBoolean)
			{
				satir.FaturaSorumlulukMerkezi = getString5;
			}
			string getString6 = parametreler._GetParametre("DegistirmeFaturaProjeKodu")._GetString;
			if (parametreler._GetParametre("DegistirmeFaturaProjeKoduKullan")._GetBoolean)
			{
				satir.FaturaProje = getString6;
			}
			string getString7 = parametreler._GetParametre("DegistirmeAciklama")._GetString;
			if (parametreler._GetParametre("DegistirmeAciklamaKullan")._GetBoolean)
			{
				satir.Aciklama = getString7;
			}
			int getInt5 = parametreler._GetParametre("DegistirmeFaturaOlusturmaDurumu")._GetInt;
			if (parametreler._GetParametre("DegistirmeFaturaOlusturmaDurumuKullan")._GetBoolean)
			{
				satir.FaturaOlusturmaDurumu = (enum_FaturaOlusturmaDurumu)getInt5;
			}
			string getString8 = parametreler._GetParametre("DegistirmeFaturaHesapKodu")._GetString;
			if (parametreler._GetParametre("DegistirmeFaturaHesapKoduKullan")._GetBoolean)
			{
				satir.FaturaHesapKodu = getString8;
			}
			int getInt6 = parametreler._GetParametre("DegistirmeFaturaMiktar")._GetInt;
			if (parametreler._GetParametre("DegistirmeFaturaMiktarKullan")._GetBoolean)
			{
				satir.FaturaMiktari = getInt6;
			}
			string getString9 = parametreler._GetParametre("DegistirmeFaturaEvrakSeri")._GetString;
			if (parametreler._GetParametre("DegistirmeFaturaEvrakSeriKullan")._GetBoolean)
			{
				satir.FaturaSeri = getString9;
			}
		}
	}

	private void ExcelOku()
	{
		_tempSatirlar = new List<AktarimSatirlar>();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl_ban_kod = new DevExpress.XtraEditors.LabelControl();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
		this.simpleButton_DosyaSec = new DevExpress.XtraEditors.SimpleButton();
		this.labelControl_ban_ismi = new DevExpress.XtraEditors.LabelControl();
		this.labelControl_ban_sube = new DevExpress.XtraEditors.LabelControl();
		this.labelControl_ban_hesapno = new DevExpress.XtraEditors.LabelControl();
		this.labelControl_doviz_cinsi = new DevExpress.XtraEditors.LabelControl();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.ayarlarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.ParametrelerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.cb_dosyatipi = new System.Windows.Forms.ComboBox();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(12, 50);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(97, 13);
		this.labelControl1.TabIndex = 1;
		this.labelControl1.Text = "Kodu :";
		this.labelControl_ban_kod.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl_ban_kod.Location = new System.Drawing.Point(124, 50);
		this.labelControl_ban_kod.Name = "labelControl_ban_kod";
		this.labelControl_ban_kod.Size = new System.Drawing.Size(5, 13);
		this.labelControl_ban_kod.TabIndex = 2;
		this.labelControl_ban_kod.Text = "-";
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(12, 69);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(97, 13);
		this.labelControl2.TabIndex = 3;
		this.labelControl2.Text = "İsmi :";
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(12, 88);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(97, 13);
		this.labelControl3.TabIndex = 4;
		this.labelControl3.Text = "Şube :";
		this.labelControl4.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(12, 107);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(97, 13);
		this.labelControl4.TabIndex = 5;
		this.labelControl4.Text = "Hesap No :";
		this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(12, 126);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(97, 13);
		this.labelControl5.TabIndex = 6;
		this.labelControl5.Text = "Döviz Cinsi :";
		this.openFileDialog1.FileName = "openFileDialog1";
		this.simpleButton_DosyaSec.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.simpleButton_DosyaSec.Appearance.Options.UseFont = true;
		this.simpleButton_DosyaSec.Location = new System.Drawing.Point(156, 167);
		this.simpleButton_DosyaSec.Name = "simpleButton_DosyaSec";
		this.simpleButton_DosyaSec.Size = new System.Drawing.Size(143, 23);
		this.simpleButton_DosyaSec.TabIndex = 9;
		this.simpleButton_DosyaSec.Text = "DOSYA SEÇ";
		this.simpleButton_DosyaSec.Click += new System.EventHandler(simpleButton_DosyaSec_Click);
		this.labelControl_ban_ismi.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl_ban_ismi.Location = new System.Drawing.Point(124, 69);
		this.labelControl_ban_ismi.Name = "labelControl_ban_ismi";
		this.labelControl_ban_ismi.Size = new System.Drawing.Size(5, 13);
		this.labelControl_ban_ismi.TabIndex = 11;
		this.labelControl_ban_ismi.Text = "-";
		this.labelControl_ban_sube.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl_ban_sube.Location = new System.Drawing.Point(124, 88);
		this.labelControl_ban_sube.Name = "labelControl_ban_sube";
		this.labelControl_ban_sube.Size = new System.Drawing.Size(5, 13);
		this.labelControl_ban_sube.TabIndex = 12;
		this.labelControl_ban_sube.Text = "-";
		this.labelControl_ban_hesapno.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl_ban_hesapno.Location = new System.Drawing.Point(124, 107);
		this.labelControl_ban_hesapno.Name = "labelControl_ban_hesapno";
		this.labelControl_ban_hesapno.Size = new System.Drawing.Size(5, 13);
		this.labelControl_ban_hesapno.TabIndex = 13;
		this.labelControl_ban_hesapno.Text = "-";
		this.labelControl_doviz_cinsi.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl_doviz_cinsi.Location = new System.Drawing.Point(124, 126);
		this.labelControl_doviz_cinsi.Name = "labelControl_doviz_cinsi";
		this.labelControl_doviz_cinsi.Size = new System.Drawing.Size(5, 13);
		this.labelControl_doviz_cinsi.TabIndex = 14;
		this.labelControl_doviz_cinsi.Text = "-";
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.ayarlarToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(455, 24);
		this.menuStrip1.TabIndex = 16;
		this.menuStrip1.Text = "menuStrip1";
		this.ayarlarToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.ParametrelerToolStripMenuItem });
		this.ayarlarToolStripMenuItem.Name = "ayarlarToolStripMenuItem";
		this.ayarlarToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
		this.ayarlarToolStripMenuItem.Text = "Ayarlar";
		this.ParametrelerToolStripMenuItem.Name = "ParametrelerToolStripMenuItem";
		this.ParametrelerToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
		this.ParametrelerToolStripMenuItem.Text = "Parametreler";
		this.ParametrelerToolStripMenuItem.Click += new System.EventHandler(ParametrelerToolStripMenuItem_Click);
		this.cb_dosyatipi.FormattingEnabled = true;
		this.cb_dosyatipi.Items.AddRange(new object[2] { "MT940", "Text / Csv" });
		this.cb_dosyatipi.Location = new System.Drawing.Point(156, 140);
		this.cb_dosyatipi.Name = "cb_dosyatipi";
		this.cb_dosyatipi.Size = new System.Drawing.Size(143, 21);
		this.cb_dosyatipi.TabIndex = 17;
		this.cb_dosyatipi.SelectedIndexChanged += new System.EventHandler(cb_dosyatipi_SelectedIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(455, 214);
		base.Controls.Add(this.cb_dosyatipi);
		base.Controls.Add(this.labelControl_doviz_cinsi);
		base.Controls.Add(this.labelControl_ban_hesapno);
		base.Controls.Add(this.labelControl_ban_sube);
		base.Controls.Add(this.labelControl_ban_ismi);
		base.Controls.Add(this.simpleButton_DosyaSec);
		base.Controls.Add(this.labelControl5);
		base.Controls.Add(this.labelControl4);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.labelControl_ban_kod);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "Aktarim_TxtCsv";
		this.Text = "Banka MT940 Aktarımı";
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
