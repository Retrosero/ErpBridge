using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraReports.UI;
using Fora.Mikro;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariPersoneller;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Rapor.Genel;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class TemsilciRaporuSecim : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public Parametreler _kullaniciparametreleri;

	private IContainer components;

	private CheckedListBoxControl clb_bolgeler;

	private LabelControl labelControl1;

	private LabelControl labelControl2;

	private CheckedListBoxControl clb_temsilciler;

	private SimpleButton sb_cari_bolge_hepsini_sec;

	private SimpleButton sb_cari_bolge_secimi_kaldir;

	private SimpleButton sb_temsilciler_secimi_kaldir;

	private SimpleButton sb_temsilciler_hepsini_sec;

	private DateEdit de_baslangic_tarihi;

	private LabelControl labelControl3;

	private LabelControl labelControl4;

	private DateEdit de_bitis_tarihi;

	private SimpleButton sb_raporu_olustur;

	private SimpleButton sb_gunler_secimi_kaldir;

	private SimpleButton sb_gunler_hepsini_sec;

	private LabelControl labelControl5;

	private CheckedListBoxControl clb_gunler;

	private SimpleButton sb_rapor_duzenle;

	private System.Windows.Forms.ComboBox cb_raporlar;

	public TemsilciRaporuSecim(MikroUygulamaBilgileri mikrouygulamabilgileri, Parametreler kullaniciparametreleri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_kullaniciparametreleri = kullaniciparametreleri;
		if (!Directory.Exists("data\\raporlar\\TemsilciRaporu"))
		{
			Directory.CreateDirectory("data\\raporlar\\TemsilciRaporu");
		}
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void TemsilciRaporuSecim_Load(object sender, EventArgs e)
	{
		de_baslangic_tarihi.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		de_bitis_tarihi.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		CariBolgeleriniListele();
		RaporlariListele();
	}

	private void RaporlariListele()
	{
		cb_raporlar.Items.Clear();
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("ID", typeof(string));
		dataTable.Columns.Add("Isim", typeof(string));
		dataTable.Rows.Add("Varsayılan", "Varsayılan");
		string[] files = Directory.GetFiles("data\\raporlar\\TemsilciRaporu", "*.repx");
		foreach (string text in files)
		{
			string text2 = text.Replace("data\\raporlar\\TemsilciRaporu\\", "").Replace(".repx", "");
			dataTable.Rows.Add(text, text2);
		}
		cb_raporlar.DataSource = dataTable;
		cb_raporlar.ValueMember = "ID";
		cb_raporlar.DisplayMember = "Isim";
		if (cb_raporlar.Items.Count != 0)
		{
			cb_raporlar.SelectedIndex = 0;
		}
	}

	private void CariBolgeleriniListele()
	{
		Cursor.Current = Cursors.WaitCursor;
		clb_bolgeler.Items.Add("TANIMSIZ");
		if (_kullaniciparametreleri._GetParametre("HakListelemeCariBolgeTumBolgeler")._GetBoolean)
		{
			string commandText = "SELECT ParametreDegeri FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram='akilli' AND ParametreAdi='CariBolgeKodu' GROUP BY ParametreDegeri ORDER BY ParametreDegeri";
			try
			{
				SqlDB sqlDB = new SqlDB();
				sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
				using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
				{
					sqlCommand.CommandText = commandText;
					SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
					while (sqlDataReader.Read())
					{
						clb_bolgeler.Items.Add(sqlDataReader.GetSafeString(0));
					}
					sqlDataReader.Close();
				}
				sqlDB.ConnectionClose();
			}
			catch
			{
			}
		}
		else
		{
			clb_bolgeler.Items.Add(_kullaniciparametreleri._GetParametre("CariBolgeKodu")._GetString);
		}
		Cursor.Current = Cursors.Default;
	}

	private void clb_bolgeler_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
	{
		TemsilciListele();
	}

	private void TemsilciListele()
	{
		Cursor.Current = Cursors.WaitCursor;
		Console.WriteLine("Temsilciler listeleniyor");
		string text = "";
		bool flag = true;
		foreach (object item in (IEnumerable)clb_bolgeler.CheckedItems)
		{
			if (!flag)
			{
				text += ",";
			}
			text = text + "'" + item.ToString() + "'";
			flag = false;
		}
		Console.WriteLine(text);
		clb_temsilciler.Items.Clear();
		if (text == "")
		{
			return;
		}
		string commandText = "SELECT ParametreUser,ISNULL((SELECT TOP 1 ParametreDegeri FROM _FORA_PARAMETRELER AS IC WITH (NOLOCK) WHERE ParametreProgram='akilli' AND ParametreAdi='CariBolgeKodu' AND IC.ParametreUser=DIS.ParametreUser),'TANIMSIZ') AS BOLGE FROM _FORA_PARAMETRELER AS DIS WITH (NOLOCK) WHERE ParametreProgram='akilli' GROUP BY ParametreUser ORDER BY ParametreUser";
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					string safeString = sqlDataReader.GetSafeString(0);
					string safeString2 = sqlDataReader.GetSafeString(1);
					foreach (object item2 in (IEnumerable)clb_bolgeler.CheckedItems)
					{
						if (item2.ToString() == safeString2)
						{
							clb_temsilciler.Items.Add(safeString);
						}
					}
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		Cursor.Current = Cursors.Default;
	}

	private void sb_cari_bolge_hepsini_sec_Click(object sender, EventArgs e)
	{
		clb_bolgeler.ItemCheck -= clb_bolgeler_ItemCheck;
		clb_bolgeler.CheckAll();
		clb_bolgeler.ItemCheck += clb_bolgeler_ItemCheck;
		TemsilciListele();
	}

	private void sb_cari_bolge_secimi_kaldir_Click(object sender, EventArgs e)
	{
		clb_bolgeler.ItemCheck -= clb_bolgeler_ItemCheck;
		clb_bolgeler.UnCheckAll();
		clb_bolgeler.ItemCheck += clb_bolgeler_ItemCheck;
		TemsilciListele();
	}

	private void sb_temsilciler_hepsini_sec_Click(object sender, EventArgs e)
	{
		clb_temsilciler.CheckAll();
	}

	private void sb_temsilciler_secimi_kaldir_Click(object sender, EventArgs e)
	{
		clb_temsilciler.UnCheckAll();
	}

	private void sb_gunler_hepsini_sec_Click(object sender, EventArgs e)
	{
		clb_gunler.CheckAll();
	}

	private void sb_gunler_secimi_kaldir_Click(object sender, EventArgs e)
	{
		clb_gunler.UnCheckAll();
	}

	private void sb_raporu_olustur_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		DxRaporTemsilci dxRaporTemsilci = new DxRaporTemsilci();
		dxRaporTemsilci.DataSource = RaporOlustur();
		string text = "";
		if (cb_raporlar.SelectedValue != null)
		{
			text = cb_raporlar.SelectedValue.ToString();
		}
		if (text != "Varsayılan")
		{
			dxRaporTemsilci.LoadLayout(text);
		}
		ReportPrintTool reportPrintTool = new ReportPrintTool(dxRaporTemsilci);
		try
		{
			reportPrintTool.ShowPreview();
		}
		catch
		{
			MessageBox.Show("Seçili raporun tipi uygun değil. Lütfen uygun bir rapor seçiniz.");
		}
		Cursor.Current = Cursors.Default;
	}

	private void sb_rapor_duzenle_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		DxRaporTemsilci dxRaporTemsilci = new DxRaporTemsilci();
		dxRaporTemsilci.DataSource = RaporOlustur();
		string text = "";
		if (cb_raporlar.SelectedValue != null)
		{
			text = cb_raporlar.SelectedValue.ToString();
		}
		if (text != "Varsayılan")
		{
			dxRaporTemsilci.LoadLayout(text);
		}
		new ReportDesignTool(dxRaporTemsilci).ShowDesignerDialog();
		Cursor.Current = Cursors.Default;
	}

	private List<Bolgeler> RaporOlustur()
	{
		Bolgeler bolgeler = new Bolgeler();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		DateTime dateTime = de_baslangic_tarihi.DateTime;
		DateTime dateTime2 = de_bitis_tarihi.DateTime;
		foreach (object item in (IEnumerable)clb_temsilciler.CheckedItems)
		{
			string text = item.ToString();
			Console.WriteLine(text);
			Bolgeler.Bolge.Temsilci temsilci = new Bolgeler.Bolge.Temsilci();
			Parametreler parametreler = ParametrelerDefault.MobilKullanici(text);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "akilli", text, "", "");
			temsilci.temsilci_kodu = parametreler._GetParametre("CariPersonelKodu")._GetString;
			CariPersonel cariPersonel = CariPersonelData.GetCariPersonel(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler._GetParametre("CariPersonelKodu")._GetString);
			temsilci.temsilci_adi = cariPersonel.cari_per_adi + " " + cariPersonel.cari_per_soyadi;
			temsilci.evrak_seri_alinan_siparis = parametreler._GetParametre("EvrakSeri_AlinanSiparis")._GetString;
			temsilci.evrak_seri_masraf = parametreler._GetParametre("EvrakSeri_Masraf")._GetString;
			temsilci.evrak_seri_satis_faturasi = parametreler._GetParametre("EvrakSeri_SatisFaturasi")._GetString;
			temsilci.evrak_seri_tahsilat = parametreler._GetParametre("EvrakSeri_Tahsilat")._GetString;
			temsilci.bolge_kodu = parametreler._GetParametre("CariBolgeKodu")._GetString;
			temsilci.bolge_adi = temsilci.bolge_kodu;
			bool flag = false;
			foreach (Bolgeler.Bolge item2 in bolgeler.bolgeler)
			{
				if (item2.bolge_kodu == temsilci.bolge_kodu)
				{
					item2.temsilciler.Add(temsilci);
					flag = true;
				}
			}
			if (!flag)
			{
				Bolgeler.Bolge bolge = new Bolgeler.Bolge();
				bolge.bolge_kodu = temsilci.bolge_kodu;
				bolge.bolge_adi = temsilci.bolge_adi;
				bolge.temsilciler = new List<Bolgeler.Bolge.Temsilci>();
				bolge.temsilciler.Add(temsilci);
				bolgeler.bolgeler.Add(bolge);
			}
		}
		foreach (Bolgeler.Bolge item3 in bolgeler.bolgeler)
		{
			foreach (Bolgeler.Bolge.Temsilci item4 in item3.temsilciler)
			{
				DateTime dateTime3 = dateTime;
				while (true)
				{
					bool flag2 = false;
					switch (dateTime3.DayOfWeek)
					{
					case DayOfWeek.Monday:
						if (clb_gunler.Items[0].CheckState == CheckState.Checked)
						{
							flag2 = true;
						}
						break;
					case DayOfWeek.Tuesday:
						if (clb_gunler.Items[1].CheckState == CheckState.Checked)
						{
							flag2 = true;
						}
						break;
					case DayOfWeek.Wednesday:
						if (clb_gunler.Items[2].CheckState == CheckState.Checked)
						{
							flag2 = true;
						}
						break;
					case DayOfWeek.Thursday:
						if (clb_gunler.Items[3].CheckState == CheckState.Checked)
						{
							flag2 = true;
						}
						break;
					case DayOfWeek.Friday:
						if (clb_gunler.Items[4].CheckState == CheckState.Checked)
						{
							flag2 = true;
						}
						break;
					case DayOfWeek.Saturday:
						if (clb_gunler.Items[5].CheckState == CheckState.Checked)
						{
							flag2 = true;
						}
						break;
					case DayOfWeek.Sunday:
						if (clb_gunler.Items[6].CheckState == CheckState.Checked)
						{
							flag2 = true;
						}
						break;
					}
					if (flag2)
					{
						Bolgeler.Bolge.Temsilci.GunlukHareketler gunlukHareketler = new Bolgeler.Bolge.Temsilci.GunlukHareketler();
						gunlukHareketler.tarih = dateTime3;
						item4.gunler.Add(gunlukHareketler);
					}
					if (dateTime3 == dateTime2)
					{
						break;
					}
					dateTime3 = dateTime3.AddDays(1.0);
				}
			}
		}
		foreach (Bolgeler.Bolge item5 in bolgeler.bolgeler)
		{
			foreach (Bolgeler.Bolge.Temsilci item6 in item5.temsilciler)
			{
				foreach (Bolgeler.Bolge.Temsilci.GunlukHareketler item7 in item6.gunler)
				{
					string commandText = "SELECT Tarih,Baslama_Saati,Baslama_Arac_Km,Baslama_Mesaj,Bitis_Saati,Bitis_Arac_Km,Bitis_Mesaj FROM _TEMSILCI_GUNLUK_HAREKETLER WHERE Temsilci_Kodu=@Temsilci_Kodu AND Tarih=@Tarih";
					try
					{
						using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
						sqlCommand.CommandText = commandText;
						sqlCommand.Parameters.AddWithValue("@Temsilci_Kodu", item6.temsilci_kodu);
						sqlCommand.Parameters.AddWithValue("@Tarih", item7.tarih);
						SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
						if (sqlDataReader.HasRows)
						{
							sqlDataReader.Read();
							item7.baslama_saati = new TimeSpan(sqlDataReader.GetSafeDateTime(1).Hour, sqlDataReader.GetSafeDateTime(1).Minute, sqlDataReader.GetSafeDateTime(1).Second);
							item7.baslama_arac_km = sqlDataReader.GetSafeInt32(2);
							item7.baslama_mesaj = sqlDataReader.GetSafeString(3);
							item7.bitis_saati = new TimeSpan(sqlDataReader.GetSafeDateTime(4).Hour, sqlDataReader.GetSafeDateTime(4).Minute, sqlDataReader.GetSafeDateTime(4).Second);
							item7.bitis_arac_km = sqlDataReader.GetSafeInt32(5);
							item7.bitis_mesaj = sqlDataReader.GetSafeString(6);
						}
						sqlDataReader.Close();
						sqlDataReader.Dispose();
						sqlDataReader = null;
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.ToString());
					}
				}
			}
		}
		foreach (Bolgeler.Bolge item8 in bolgeler.bolgeler)
		{
			foreach (Bolgeler.Bolge.Temsilci item9 in item8.temsilciler)
			{
				foreach (Bolgeler.Bolge.Temsilci.GunlukHareketler item10 in item9.gunler)
				{
					foreach (CariRotaListItem cariRotaListItem in TemsilciData.GetCariRotaListItems(sqlDB.Connection, item9.temsilci_kodu, item10.tarih))
					{
						Bolgeler.Bolge.Temsilci.GunlukHareketler.Ziyaret ziyaret = new Bolgeler.Bolge.Temsilci.GunlukHareketler.Ziyaret();
						ziyaret.cari_kodu = cariRotaListItem.cari_kod;
						ziyaret.cari_ismi = cariRotaListItem.cari_unvan1 + " " + cariRotaListItem.cari_unvan2;
						ziyaret.adres = cariRotaListItem.CariAdres.GosterAdres;
						ziyaret.baslama_zamani = default(TimeSpan);
						ziyaret.bitis_zamani = default(TimeSpan);
						ziyaret.rotada_var = true;
						ziyaret.ziyaret_edildi = false;
						item10.ziyaret_listesi.Add(ziyaret);
					}
					string commandText2 = "SELECT zyrt_Cari_Kodu,cari_unvan1,zyrt_Cari_Adres_No,adr_cadde + ' ' + adr_sokak + ' ' + adr_posta_kodu + ' ' + adr_ilce + ' ' + adr_il + ' ' + adr_ulke AS Adres,zyrt_Baslama_Saati,zyrt_Bitis_Saati FROM _ZIYARET_HAREKETLERI  LEFT JOIN CARI_HESAPLAR ON zyrt_Cari_Kodu=cari_kod LEFT JOIN CARI_HESAP_ADRESLERI ON zyrt_Cari_Adres_No=adr_adres_no AND zyrt_Cari_Kodu=adr_cari_kod WHERE zyrt_Temsilci_Kodu=@zyrt_Temsilci_Kodu AND zyrt_Tarihi=@zyrt_Tarihi";
					try
					{
						using SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand();
						sqlCommand2.CommandText = commandText2;
						sqlCommand2.Parameters.AddWithValue("@zyrt_Temsilci_Kodu", item9.temsilci_kodu);
						sqlCommand2.Parameters.AddWithValue("@zyrt_Tarihi", item10.tarih);
						SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
						while (sqlDataReader2.Read())
						{
							string safeString = sqlDataReader2.GetSafeString(0);
							string safeString2 = sqlDataReader2.GetSafeString(1);
							string safeString3 = sqlDataReader2.GetSafeString(3);
							bool flag3 = false;
							foreach (Bolgeler.Bolge.Temsilci.GunlukHareketler.Ziyaret item11 in item10.ziyaret_listesi)
							{
								if (item11.cari_kodu == safeString && item11.adres == safeString3)
								{
									flag3 = true;
									item11.baslama_zamani = new TimeSpan(sqlDataReader2.GetSafeDateTime(4).Hour, sqlDataReader2.GetSafeDateTime(4).Minute, sqlDataReader2.GetSafeDateTime(4).Second);
									item11.bitis_zamani = new TimeSpan(sqlDataReader2.GetSafeDateTime(5).Hour, sqlDataReader2.GetSafeDateTime(5).Minute, sqlDataReader2.GetSafeDateTime(5).Second);
									item11.ziyaret_edildi = true;
									flag3 = true;
								}
							}
							if (!flag3)
							{
								Bolgeler.Bolge.Temsilci.GunlukHareketler.Ziyaret ziyaret2 = new Bolgeler.Bolge.Temsilci.GunlukHareketler.Ziyaret();
								ziyaret2.cari_kodu = safeString;
								ziyaret2.cari_ismi = safeString2;
								ziyaret2.adres = safeString3;
								ziyaret2.baslama_zamani = new TimeSpan(sqlDataReader2.GetSafeDateTime(4).Hour, sqlDataReader2.GetSafeDateTime(4).Minute, sqlDataReader2.GetSafeDateTime(4).Second);
								ziyaret2.bitis_zamani = new TimeSpan(sqlDataReader2.GetSafeDateTime(5).Hour, sqlDataReader2.GetSafeDateTime(5).Minute, sqlDataReader2.GetSafeDateTime(5).Second);
								ziyaret2.ziyaret_edildi = true;
								ziyaret2.rotada_var = false;
								item10.ziyaret_listesi.Add(ziyaret2);
							}
						}
						sqlDataReader2.Close();
						sqlDataReader2.Dispose();
						sqlDataReader2 = null;
					}
					catch (Exception ex2)
					{
						MessageBox.Show(ex2.ToString());
					}
					string evrak_seri_tahsilat = item9.evrak_seri_tahsilat;
					evrak_seri_tahsilat = "'" + evrak_seri_tahsilat + "'";
					evrak_seri_tahsilat = evrak_seri_tahsilat.Replace(",", "','");
					string commandText3 = "SELECT cha_cinsi,cha_kod,cari_unvan1,cha_evrakno_seri,cha_evrakno_sira,cha_meblag,cha_aciklama FROM CARI_HESAP_HAREKETLERI WITH(NOLOCK) INNER JOIN CARI_HESAPLAR WITH(NOLOCK) ON cha_kod=cari_kod WHERE cha_evrak_tip=1 AND cha_tarihi=@cha_tarihi AND cha_evrakno_seri in (" + evrak_seri_tahsilat + ")";
					try
					{
						using SqlCommand sqlCommand3 = sqlDB.Connection.CreateCommand();
						sqlCommand3.CommandText = commandText3;
						sqlCommand3.Parameters.AddWithValue("@cha_tarihi", item10.tarih);
						SqlDataReader sqlDataReader3 = sqlCommand3.ExecuteReader();
						while (sqlDataReader3.Read())
						{
							Bolgeler.Bolge.Temsilci.GunlukHareketler.Tahsilat tahsilat = new Bolgeler.Bolge.Temsilci.GunlukHareketler.Tahsilat();
							switch (sqlDataReader3.GetSafeByte(0))
							{
							case 0:
								tahsilat.tipi = Bolgeler.Bolge.Temsilci.GunlukHareketler.tahsilat_tipleri.Nakit;
								break;
							case 1:
								tahsilat.tipi = Bolgeler.Bolge.Temsilci.GunlukHareketler.tahsilat_tipleri.Cek;
								break;
							case 2:
								tahsilat.tipi = Bolgeler.Bolge.Temsilci.GunlukHareketler.tahsilat_tipleri.Senet;
								break;
							case 19:
								tahsilat.tipi = Bolgeler.Bolge.Temsilci.GunlukHareketler.tahsilat_tipleri.KrediKarti;
								break;
							default:
								tahsilat.tipi = Bolgeler.Bolge.Temsilci.GunlukHareketler.tahsilat_tipleri.Tanimsiz;
								break;
							}
							tahsilat.cari_kodu = sqlDataReader3.GetSafeString(1);
							if (!sqlDataReader3.IsDBNull(2))
							{
								tahsilat.cari_ismi = sqlDataReader3.GetSafeString(2);
							}
							tahsilat.evrak_seri = sqlDataReader3.GetSafeString(3);
							tahsilat.evrak_sira = sqlDataReader3.GetSafeInt32(4);
							tahsilat.tutar = sqlDataReader3.GetSafeDouble(5);
							tahsilat.aciklama = sqlDataReader3.GetSafeString(6);
							item10.tahsilatlar.Add(tahsilat);
						}
						sqlDataReader3.Close();
						sqlDataReader3.Dispose();
						sqlDataReader3 = null;
					}
					catch (Exception ex3)
					{
						MessageBox.Show(ex3.ToString());
					}
					evrak_seri_tahsilat = item9.evrak_seri_alinan_siparis;
					evrak_seri_tahsilat = "'" + evrak_seri_tahsilat + "'";
					evrak_seri_tahsilat = evrak_seri_tahsilat.Replace(",", "','");
					string commandText4 = "SELECT sip_musteri_kod,cari_unvan1,sip_evrakno_seri,sip_evrakno_sira,SUM(sip_tutar-sip_iskonto_1-sip_iskonto_2-sip_iskonto_3-sip_iskonto_4-sip_iskonto_5-sip_iskonto_6+sip_masraf_1+sip_masraf_2+sip_masraf_3+sip_masraf_4+sip_vergi+sip_masvergi) AS sip_tutar FROM SIPARISLER WITH(NOLOCK) INNER JOIN CARI_HESAPLAR WITH(NOLOCK) ON sip_musteri_kod=cari_kod WHERE sip_tarih=@sip_tarih AND sip_evrakno_seri in (" + evrak_seri_tahsilat + ") GROUP BY sip_evrakno_seri,sip_evrakno_sira,sip_musteri_kod,cari_unvan1";
					try
					{
						using SqlCommand sqlCommand4 = sqlDB.Connection.CreateCommand();
						sqlCommand4.CommandText = commandText4;
						sqlCommand4.Parameters.AddWithValue("@sip_tarih", item10.tarih);
						SqlDataReader sqlDataReader4 = sqlCommand4.ExecuteReader();
						while (sqlDataReader4.Read())
						{
							Bolgeler.Bolge.Temsilci.GunlukHareketler.Siparis siparis = new Bolgeler.Bolge.Temsilci.GunlukHareketler.Siparis();
							siparis.cari_kodu = sqlDataReader4.GetSafeString(0);
							siparis.cari_ismi = sqlDataReader4.GetSafeString(1);
							siparis.evrak_seri = sqlDataReader4.GetSafeString(2);
							siparis.evrak_sira = sqlDataReader4.GetSafeInt32(3);
							siparis.tutar = sqlDataReader4.GetSafeDouble(4);
							item10.siparisler.Add(siparis);
						}
						sqlDataReader4.Close();
						sqlDataReader4.Dispose();
						sqlDataReader4 = null;
					}
					catch (Exception ex4)
					{
						MessageBox.Show(ex4.ToString());
					}
					evrak_seri_tahsilat = item9.evrak_seri_masraf;
					evrak_seri_tahsilat = "'" + evrak_seri_tahsilat + "'";
					evrak_seri_tahsilat = evrak_seri_tahsilat.Replace(",", "','");
					string commandText5 = "SELECT cha_kasa_hizkod,his_isim,cha_evrakno_seri,cha_evrakno_sira,SUM(cha_meblag) AS cha_meblag,cha_aciklama FROM CARI_HESAP_HAREKETLERI WITH(NOLOCK) INNER JOIN MASRAF_HESAPLARI WITH(NOLOCK) ON cha_kasa_hizkod=his_kod WHERE cha_tip=1 AND cha_normal_Iade=0 AND cha_evrak_tip=0 AND cha_cinsi=8 AND cha_kasa_hizmet=5 AND cha_tarihi=@cha_tarihi AND cha_evrakno_seri in (" + evrak_seri_tahsilat + ") GROUP BY cha_evrakno_seri,cha_evrakno_sira,cha_kasa_hizkod,his_isim,cha_aciklama";
					try
					{
						using SqlCommand sqlCommand5 = sqlDB.Connection.CreateCommand();
						sqlCommand5.CommandText = commandText5;
						sqlCommand5.Parameters.AddWithValue("@cha_tarihi", item10.tarih);
						SqlDataReader sqlDataReader5 = sqlCommand5.ExecuteReader();
						while (sqlDataReader5.Read())
						{
							Bolgeler.Bolge.Temsilci.GunlukHareketler.Masraf masraf = new Bolgeler.Bolge.Temsilci.GunlukHareketler.Masraf();
							masraf.masraf_kodu = sqlDataReader5.GetSafeString(0);
							masraf.masraf_ismi = sqlDataReader5.GetSafeString(1);
							masraf.evrak_seri = sqlDataReader5.GetSafeString(2);
							masraf.evrak_sira = sqlDataReader5.GetSafeInt32(3);
							masraf.tutar = sqlDataReader5.GetSafeDouble(4);
							masraf.aciklama = sqlDataReader5.GetSafeString(5);
							item10.masraflar.Add(masraf);
						}
						sqlDataReader5.Close();
						sqlDataReader5.Dispose();
						sqlDataReader5 = null;
					}
					catch (Exception ex5)
					{
						MessageBox.Show(ex5.ToString());
					}
					evrak_seri_tahsilat = item9.evrak_seri_satis_faturasi;
					evrak_seri_tahsilat = "'" + evrak_seri_tahsilat + "'";
					evrak_seri_tahsilat = evrak_seri_tahsilat.Replace(",", "','");
					string commandText6 = "SELECT cha_kod,cari_unvan1,cha_evrakno_seri,cha_evrakno_sira,SUM(cha_meblag) AS cha_meblag,cha_aciklama FROM CARI_HESAP_HAREKETLERI WITH(NOLOCK) INNER JOIN CARI_HESAPLAR WITH(NOLOCK) ON cha_kod=cari_kod WHERE cha_tip=0 AND cha_normal_Iade=0 AND cha_evrak_tip=63 AND cha_cinsi in (6,7,8) AND cha_tarihi=@cha_tarihi AND cha_evrakno_seri in (" + evrak_seri_tahsilat + ") GROUP BY cha_evrakno_seri,cha_evrakno_sira,cha_kod,cari_unvan1,cha_aciklama";
					try
					{
						using SqlCommand sqlCommand6 = sqlDB.Connection.CreateCommand();
						sqlCommand6.CommandText = commandText6;
						sqlCommand6.Parameters.AddWithValue("@cha_tarihi", item10.tarih);
						SqlDataReader sqlDataReader6 = sqlCommand6.ExecuteReader();
						while (sqlDataReader6.Read())
						{
							Bolgeler.Bolge.Temsilci.GunlukHareketler.Fatura fatura = new Bolgeler.Bolge.Temsilci.GunlukHareketler.Fatura();
							fatura.cari_kodu = sqlDataReader6.GetSafeString(0);
							fatura.cari_ismi = sqlDataReader6.GetSafeString(1);
							fatura.evrak_seri = sqlDataReader6.GetSafeString(2);
							fatura.evrak_sira = sqlDataReader6.GetSafeInt32(3);
							fatura.tutar = sqlDataReader6.GetSafeDouble(4);
							fatura.aciklama = sqlDataReader6.GetSafeString(5);
							item10.faturalar.Add(fatura);
						}
						sqlDataReader6.Close();
						sqlDataReader6.Dispose();
						sqlDataReader6 = null;
					}
					catch (Exception ex6)
					{
						MessageBox.Show(ex6.ToString());
					}
				}
			}
		}
		sqlDB.ConnectionClose();
		return new List<Bolgeler> { bolgeler };
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
		this.clb_bolgeler = new DevExpress.XtraEditors.CheckedListBoxControl();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.clb_temsilciler = new DevExpress.XtraEditors.CheckedListBoxControl();
		this.sb_cari_bolge_hepsini_sec = new DevExpress.XtraEditors.SimpleButton();
		this.sb_cari_bolge_secimi_kaldir = new DevExpress.XtraEditors.SimpleButton();
		this.sb_temsilciler_secimi_kaldir = new DevExpress.XtraEditors.SimpleButton();
		this.sb_temsilciler_hepsini_sec = new DevExpress.XtraEditors.SimpleButton();
		this.de_baslangic_tarihi = new DevExpress.XtraEditors.DateEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.de_bitis_tarihi = new DevExpress.XtraEditors.DateEdit();
		this.sb_raporu_olustur = new DevExpress.XtraEditors.SimpleButton();
		this.sb_gunler_secimi_kaldir = new DevExpress.XtraEditors.SimpleButton();
		this.sb_gunler_hepsini_sec = new DevExpress.XtraEditors.SimpleButton();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.clb_gunler = new DevExpress.XtraEditors.CheckedListBoxControl();
		this.sb_rapor_duzenle = new DevExpress.XtraEditors.SimpleButton();
		this.cb_raporlar = new System.Windows.Forms.ComboBox();
		((System.ComponentModel.ISupportInitialize)this.clb_bolgeler).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.clb_temsilciler).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.de_baslangic_tarihi.Properties.CalendarTimeProperties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.de_baslangic_tarihi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.de_bitis_tarihi.Properties.CalendarTimeProperties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.de_bitis_tarihi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.clb_gunler).BeginInit();
		base.SuspendLayout();
		this.clb_bolgeler.CheckOnClick = true;
		this.clb_bolgeler.Location = new System.Drawing.Point(25, 43);
		this.clb_bolgeler.Name = "clb_bolgeler";
		this.clb_bolgeler.Size = new System.Drawing.Size(134, 134);
		this.clb_bolgeler.TabIndex = 0;
		this.clb_bolgeler.ItemCheck += new DevExpress.XtraEditors.Controls.ItemCheckEventHandler(clb_bolgeler_ItemCheck);
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(25, 15);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(134, 22);
		this.labelControl1.TabIndex = 1;
		this.labelControl1.Text = "Cari bölgeleri";
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(239, 15);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(134, 22);
		this.labelControl2.TabIndex = 3;
		this.labelControl2.Text = "Temsilciler";
		this.clb_temsilciler.CheckOnClick = true;
		this.clb_temsilciler.Location = new System.Drawing.Point(239, 43);
		this.clb_temsilciler.Name = "clb_temsilciler";
		this.clb_temsilciler.Size = new System.Drawing.Size(134, 134);
		this.clb_temsilciler.TabIndex = 2;
		this.sb_cari_bolge_hepsini_sec.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_cari_bolge_hepsini_sec.Appearance.Options.UseFont = true;
		this.sb_cari_bolge_hepsini_sec.Location = new System.Drawing.Point(25, 183);
		this.sb_cari_bolge_hepsini_sec.Name = "sb_cari_bolge_hepsini_sec";
		this.sb_cari_bolge_hepsini_sec.Size = new System.Drawing.Size(134, 23);
		this.sb_cari_bolge_hepsini_sec.TabIndex = 4;
		this.sb_cari_bolge_hepsini_sec.Text = "Hepsini seç";
		this.sb_cari_bolge_hepsini_sec.Click += new System.EventHandler(sb_cari_bolge_hepsini_sec_Click);
		this.sb_cari_bolge_secimi_kaldir.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_cari_bolge_secimi_kaldir.Appearance.Options.UseFont = true;
		this.sb_cari_bolge_secimi_kaldir.Location = new System.Drawing.Point(25, 212);
		this.sb_cari_bolge_secimi_kaldir.Name = "sb_cari_bolge_secimi_kaldir";
		this.sb_cari_bolge_secimi_kaldir.Size = new System.Drawing.Size(134, 23);
		this.sb_cari_bolge_secimi_kaldir.TabIndex = 5;
		this.sb_cari_bolge_secimi_kaldir.Text = "Seçimi kaldır";
		this.sb_cari_bolge_secimi_kaldir.Click += new System.EventHandler(sb_cari_bolge_secimi_kaldir_Click);
		this.sb_temsilciler_secimi_kaldir.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_temsilciler_secimi_kaldir.Appearance.Options.UseFont = true;
		this.sb_temsilciler_secimi_kaldir.Location = new System.Drawing.Point(239, 212);
		this.sb_temsilciler_secimi_kaldir.Name = "sb_temsilciler_secimi_kaldir";
		this.sb_temsilciler_secimi_kaldir.Size = new System.Drawing.Size(134, 23);
		this.sb_temsilciler_secimi_kaldir.TabIndex = 7;
		this.sb_temsilciler_secimi_kaldir.Text = "Seçimi kaldır";
		this.sb_temsilciler_secimi_kaldir.Click += new System.EventHandler(sb_temsilciler_secimi_kaldir_Click);
		this.sb_temsilciler_hepsini_sec.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_temsilciler_hepsini_sec.Appearance.Options.UseFont = true;
		this.sb_temsilciler_hepsini_sec.Location = new System.Drawing.Point(239, 183);
		this.sb_temsilciler_hepsini_sec.Name = "sb_temsilciler_hepsini_sec";
		this.sb_temsilciler_hepsini_sec.Size = new System.Drawing.Size(134, 23);
		this.sb_temsilciler_hepsini_sec.TabIndex = 6;
		this.sb_temsilciler_hepsini_sec.Text = "Hepsini seç";
		this.sb_temsilciler_hepsini_sec.Click += new System.EventHandler(sb_temsilciler_hepsini_sec_Click);
		this.de_baslangic_tarihi.EditValue = null;
		this.de_baslangic_tarihi.Location = new System.Drawing.Point(239, 278);
		this.de_baslangic_tarihi.Name = "de_baslangic_tarihi";
		this.de_baslangic_tarihi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.de_baslangic_tarihi.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.de_baslangic_tarihi.Properties.CalendarTimeProperties.CloseUpKey = new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F4);
		this.de_baslangic_tarihi.Properties.CalendarTimeProperties.PopupBorderStyle = DevExpress.XtraEditors.Controls.PopupBorderStyles.Default;
		this.de_baslangic_tarihi.Size = new System.Drawing.Size(134, 20);
		this.de_baslangic_tarihi.TabIndex = 8;
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(239, 254);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(134, 22);
		this.labelControl3.TabIndex = 9;
		this.labelControl3.Text = "Başlangıç tarihi";
		this.labelControl4.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(239, 300);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(134, 22);
		this.labelControl4.TabIndex = 11;
		this.labelControl4.Text = "Bitiş tarihi";
		this.de_bitis_tarihi.EditValue = null;
		this.de_bitis_tarihi.Location = new System.Drawing.Point(239, 323);
		this.de_bitis_tarihi.Name = "de_bitis_tarihi";
		this.de_bitis_tarihi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.de_bitis_tarihi.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.de_bitis_tarihi.Properties.CalendarTimeProperties.CloseUpKey = new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F4);
		this.de_bitis_tarihi.Properties.CalendarTimeProperties.PopupBorderStyle = DevExpress.XtraEditors.Controls.PopupBorderStyles.Default;
		this.de_bitis_tarihi.Size = new System.Drawing.Size(134, 20);
		this.de_bitis_tarihi.TabIndex = 10;
		this.sb_raporu_olustur.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_raporu_olustur.Appearance.Options.UseFont = true;
		this.sb_raporu_olustur.Location = new System.Drawing.Point(172, 412);
		this.sb_raporu_olustur.Name = "sb_raporu_olustur";
		this.sb_raporu_olustur.Size = new System.Drawing.Size(229, 23);
		this.sb_raporu_olustur.TabIndex = 12;
		this.sb_raporu_olustur.Text = "Raporu oluştur";
		this.sb_raporu_olustur.Click += new System.EventHandler(sb_raporu_olustur_Click);
		this.sb_gunler_secimi_kaldir.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_gunler_secimi_kaldir.Appearance.Options.UseFont = true;
		this.sb_gunler_secimi_kaldir.Location = new System.Drawing.Point(25, 441);
		this.sb_gunler_secimi_kaldir.Name = "sb_gunler_secimi_kaldir";
		this.sb_gunler_secimi_kaldir.Size = new System.Drawing.Size(134, 23);
		this.sb_gunler_secimi_kaldir.TabIndex = 20;
		this.sb_gunler_secimi_kaldir.Text = "Seçimi kaldır";
		this.sb_gunler_secimi_kaldir.Click += new System.EventHandler(sb_gunler_secimi_kaldir_Click);
		this.sb_gunler_hepsini_sec.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_gunler_hepsini_sec.Appearance.Options.UseFont = true;
		this.sb_gunler_hepsini_sec.Location = new System.Drawing.Point(25, 412);
		this.sb_gunler_hepsini_sec.Name = "sb_gunler_hepsini_sec";
		this.sb_gunler_hepsini_sec.Size = new System.Drawing.Size(134, 23);
		this.sb_gunler_hepsini_sec.TabIndex = 19;
		this.sb_gunler_hepsini_sec.Text = "Hepsini seç";
		this.sb_gunler_hepsini_sec.Click += new System.EventHandler(sb_gunler_hepsini_sec_Click);
		this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(25, 244);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(134, 22);
		this.labelControl5.TabIndex = 14;
		this.labelControl5.Text = "Günler";
		this.clb_gunler.CheckOnClick = true;
		this.clb_gunler.Items.AddRange(new DevExpress.XtraEditors.Controls.CheckedListBoxItem[7]
		{
			new DevExpress.XtraEditors.Controls.CheckedListBoxItem("Pazartesi", System.Windows.Forms.CheckState.Checked),
			new DevExpress.XtraEditors.Controls.CheckedListBoxItem("Salı", System.Windows.Forms.CheckState.Checked),
			new DevExpress.XtraEditors.Controls.CheckedListBoxItem("Çarşamba", System.Windows.Forms.CheckState.Checked),
			new DevExpress.XtraEditors.Controls.CheckedListBoxItem("Perşembe", System.Windows.Forms.CheckState.Checked),
			new DevExpress.XtraEditors.Controls.CheckedListBoxItem("Cuma", System.Windows.Forms.CheckState.Checked),
			new DevExpress.XtraEditors.Controls.CheckedListBoxItem("Cumartesi", System.Windows.Forms.CheckState.Checked),
			new DevExpress.XtraEditors.Controls.CheckedListBoxItem("Pazar")
		});
		this.clb_gunler.Location = new System.Drawing.Point(25, 272);
		this.clb_gunler.Name = "clb_gunler";
		this.clb_gunler.Size = new System.Drawing.Size(134, 134);
		this.clb_gunler.TabIndex = 18;
		this.sb_rapor_duzenle.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_rapor_duzenle.Appearance.Options.UseFont = true;
		this.sb_rapor_duzenle.Location = new System.Drawing.Point(172, 441);
		this.sb_rapor_duzenle.Name = "sb_rapor_duzenle";
		this.sb_rapor_duzenle.Size = new System.Drawing.Size(229, 23);
		this.sb_rapor_duzenle.TabIndex = 13;
		this.sb_rapor_duzenle.Text = "Rapor düzenle";
		this.sb_rapor_duzenle.Click += new System.EventHandler(sb_rapor_duzenle_Click);
		this.cb_raporlar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_raporlar.FormattingEnabled = true;
		this.cb_raporlar.Location = new System.Drawing.Point(172, 385);
		this.cb_raporlar.Name = "cb_raporlar";
		this.cb_raporlar.Size = new System.Drawing.Size(229, 21);
		this.cb_raporlar.TabIndex = 11;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(413, 493);
		base.Controls.Add(this.cb_raporlar);
		base.Controls.Add(this.sb_rapor_duzenle);
		base.Controls.Add(this.sb_gunler_secimi_kaldir);
		base.Controls.Add(this.sb_gunler_hepsini_sec);
		base.Controls.Add(this.labelControl5);
		base.Controls.Add(this.clb_gunler);
		base.Controls.Add(this.sb_raporu_olustur);
		base.Controls.Add(this.labelControl4);
		base.Controls.Add(this.de_bitis_tarihi);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.de_baslangic_tarihi);
		base.Controls.Add(this.sb_temsilciler_secimi_kaldir);
		base.Controls.Add(this.sb_temsilciler_hepsini_sec);
		base.Controls.Add(this.sb_cari_bolge_secimi_kaldir);
		base.Controls.Add(this.sb_cari_bolge_hepsini_sec);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.clb_temsilciler);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.clb_bolgeler);
		base.Name = "TemsilciRaporuSecim";
		this.Text = "Temsilci Raporu Seçim";
		base.Load += new System.EventHandler(TemsilciRaporuSecim_Load);
		((System.ComponentModel.ISupportInitialize)this.clb_bolgeler).EndInit();
		((System.ComponentModel.ISupportInitialize)this.clb_temsilciler).EndInit();
		((System.ComponentModel.ISupportInitialize)this.de_baslangic_tarihi.Properties.CalendarTimeProperties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.de_baslangic_tarihi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.de_bitis_tarihi.Properties.CalendarTimeProperties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.de_bitis_tarihi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.clb_gunler).EndInit();
		base.ResumeLayout(false);
	}
}
