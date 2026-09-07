using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Evraklar;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Utility;

namespace Fora.App.Win.Mikro.ZamanlanmisGorevler;

public class BaskaFirmaninStokMiktarlariniZamanliAktarim : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public string _KoyalanacakKullanici = "YOK";

	public string _KullaniciAdi = "";

	public string _Sifre = "";

	public int _MikroKullaniciNo;

	private string _SqlServer = "";

	private string _SqlKullaniciAdi = "";

	private string _SqlSifre = "";

	private string _SqlDBName = "";

	private string _StokSorgusu = "";

	private int _HedefDepoNo;

	private string _EvrakSeri = "";

	private int _EvrakSira;

	private int _KacDakikadaBir;

	private int isleme_kalan_sure;

	private IContainer components;

	private SimpleButton sb_islemi_baslat;

	private Label label1;

	private Label label_EvrakSeri;

	private Label label_EvrakSira;

	private Label label3;

	private Label label_HedefDepoNo;

	private Label label4;

	private Label label_KacDakikadaBir;

	private Label label5;

	private Label label_Guncellemeye_Kalan_Sure;

	private Label label6;

	private Timer timer_islem_baslatma;

	private Label label_son_guncelleme_zamani;

	private Label label7;

	private Label label_islem_aciklamasi;

	private Label label_sonislemsuresi;

	private Label label8;

	private RichTextBox richTextBox_HataliIslemler;

	private Label label2;

	private SimpleButton sb_gecmisi_temizle;

	public BaskaFirmaninStokMiktarlariniZamanliAktarim(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void KullaniciEkle_Load(object sender, EventArgs e)
	{
		_SqlServer = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer")._GetString;
		_SqlKullaniciAdi = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi")._GetString;
		_SqlSifre = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre")._GetString;
		_SqlDBName = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName")._GetString;
		_StokSorgusu = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu")._GetString;
		_HedefDepoNo = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo")._GetInt;
		_EvrakSeri = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri")._GetString;
		_EvrakSira = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira")._GetInt;
		_KacDakikadaBir = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir")._GetInt;
		label_EvrakSeri.Text = _EvrakSeri;
		label_HedefDepoNo.Text = _HedefDepoNo.ToString();
		label_EvrakSira.Text = _EvrakSira.ToString();
		label_KacDakikadaBir.Text = _KacDakikadaBir + " dakika";
		label_islem_aciklamasi.Text = "";
		if (_EvrakSeri == "")
		{
			MessageBox.Show("'Ayarlar -> Kullanıcı Tanımlama' bölümünden gerekli tanımlamaları yapınız. Evrak seri boş bırakılmamalıdır.");
			sb_islemi_baslat.Enabled = false;
		}
		else
		{
			sb_islemi_baslat.Enabled = true;
		}
	}

	private void sb_islemi_baslat_Click(object sender, EventArgs e)
	{
		sb_islemi_baslat.Enabled = false;
		isleme_basla();
	}

	private void timer_ekran_guncelleme_Tick(object sender, EventArgs e)
	{
		isleme_kalan_sure--;
		TimeSpan timeSpan = new TimeSpan(0, 0, isleme_kalan_sure);
		label_Guncellemeye_Kalan_Sure.Text = timeSpan.ToString();
		if (isleme_kalan_sure < 1)
		{
			isleme_basla();
		}
	}

	private void isleme_basla()
	{
		isleme_kalan_sure = _KacDakikadaBir * 60;
		timer_islem_baslatma.Stop();
		IslemiYap();
	}

	private void IslemiYap()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		List<double> list = new List<double>();
		List<string> list2 = new List<string>();
		SqlBaglantiBilgileri sqlBaglantiBilgileri = new SqlBaglantiBilgileri(_SqlServer, _SqlKullaniciAdi, _SqlSifre);
		SqlConnection sqlConnection = new SqlConnection();
		if (sqlBaglantiBilgileri.SqlUserName == "")
		{
			sqlConnection.ConnectionString = "Integrated Security=SSPI;Data Source=" + sqlBaglantiBilgileri.SqlServer + "; Persist Security Info=False; Database=" + _SqlDBName + " ; Initial Catalog=" + _SqlDBName + ";";
		}
		else
		{
			sqlConnection.ConnectionString = "Password=" + sqlBaglantiBilgileri.SqlPassword + "; User ID=" + sqlBaglantiBilgileri.SqlUserName + "; Initial Catalog=" + _SqlDBName + "; Data Source=" + sqlBaglantiBilgileri.SqlServer;
		}
		sqlConnection.Open();
		SqlCommand sqlCommand = sqlConnection.CreateCommand();
		sqlCommand.CommandText = _StokSorgusu;
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		while (sqlDataReader.Read())
		{
			list2.Add(sqlDataReader.GetSafeString(0));
			list.Add(sqlDataReader.GetSafeDouble(1));
			label_islem_aciklamasi.Text = list2[list2.Count - 1] + " için stok miktarı " + list[list.Count - 1].ToString("N", CultureInfo.CreateSpecificCulture("tr-TR"));
			Application.DoEvents();
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlConnection.Close();
		sqlConnection.Dispose();
		SqlConnection sqlConnection2 = new SqlConnection();
		if (_mikrouygulamabilgileri.baglantibilgileri.SqlUserName == "")
		{
			sqlConnection2.ConnectionString = "Integrated Security=SSPI;Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Persist Security Info=False; Database=" + _mikrouygulamabilgileri.MikroFirmaDBName + " ; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + ";";
		}
		else
		{
			sqlConnection2.ConnectionString = "Password=" + _mikrouygulamabilgileri.baglantibilgileri.SqlPassword + "; User ID=" + _mikrouygulamabilgileri.baglantibilgileri.SqlUserName + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer;
		}
		sqlConnection2.Open();
		int num = 0;
		foreach (string item in list2)
		{
			label_islem_aciklamasi.Text = item + " için kontrol yapılıyor";
			Application.DoEvents();
			double num2 = list[num];
			int num3 = 0;
			num++;
			if (StokData.StokVarMi(sqlConnection2, _mikrouygulamabilgileri.MikroFirmaDBName, item))
			{
				if (num2 <= 0.0)
				{
					num3 = 1;
				}
				else
				{
					SqlCommand sqlCommand2 = new SqlCommand("SELECT sth_miktar FROM STOK_HAREKETLERI WHERE sth_tip=0 AND sth_cins=11 AND sth_normal_iade=0 AND sth_evraktip=12 AND sth_evrakno_seri='" + _EvrakSeri + "' AND sth_evrakno_sira=" + _EvrakSira + " AND sth_stok_kod=@sto_kod", sqlConnection2);
					sqlCommand2.Parameters.AddWithValue("@sto_kod", item);
					object obj = sqlCommand2.ExecuteScalar();
					num3 = ((obj == null || obj is DBNull) ? 3 : ((double.Parse(obj.ToString()) != num2) ? 2 : 0));
					sqlCommand2.Dispose();
				}
			}
			else
			{
				richTextBox_HataliIslemler.AppendText(DateTime.Now.ToString() + " Stok kartı bulunamadı (" + item + ")" + Environment.NewLine);
				Application.DoEvents();
				num3 = 1;
			}
			switch (num3)
			{
			case 1:
				label_islem_aciklamasi.Text = item + " kaydı siliniyor";
				Application.DoEvents();
				try
				{
					SqlCommand sqlCommand4 = new SqlCommand("DELETE STOK_HAREKETLERI WHERE sth_tip=0 AND sth_cins=11 AND sth_normal_iade=0 AND sth_evraktip=12 AND sth_evrakno_seri='" + _EvrakSeri + "' AND sth_evrakno_sira=" + _EvrakSira + " AND sth_stok_kod=@sto_kod", sqlConnection2);
					sqlCommand4.Parameters.AddWithValue("@sto_kod", item);
					sqlCommand4.ExecuteScalar();
					sqlCommand4.Dispose();
					richTextBox_HataliIslemler.AppendText(DateTime.Now.ToString() + " Stok kartı miktar sıfırlandı (" + item + ")" + Environment.NewLine);
					Application.DoEvents();
				}
				catch
				{
				}
				break;
			case 2:
				label_islem_aciklamasi.Text = item + " kaydı güncelleniyor";
				Application.DoEvents();
				try
				{
					SqlCommand sqlCommand3 = new SqlCommand("UPDATE STOK_HAREKETLERI SET sth_miktar=@sth_miktar WHERE sth_tip=0 AND sth_cins=11 AND sth_normal_iade=0 AND sth_evraktip=12 AND sth_evrakno_seri='" + _EvrakSeri + "' AND sth_evrakno_sira=" + _EvrakSira + " AND sth_stok_kod=@sto_kod", sqlConnection2);
					sqlCommand3.Parameters.AddWithValue("@sto_kod", item);
					sqlCommand3.Parameters.AddWithValue("@sth_miktar", num2);
					sqlCommand3.ExecuteScalar();
					sqlCommand3.Dispose();
					richTextBox_HataliIslemler.AppendText(DateTime.Now.ToString() + " Stok kartı miktarı güncellendi (" + item + ")" + Environment.NewLine);
					Application.DoEvents();
				}
				catch
				{
				}
				break;
			case 3:
				label_islem_aciklamasi.Text = item + " kaydı oluşturuluyor";
				Application.DoEvents();
				try
				{
					int num4 = 0;
					object obj2 = new SqlCommand("SELECT TOP 1 sth_satirno FROM STOK_HAREKETLERI WHERE sth_tip=0 AND sth_cins=11 AND sth_normal_iade=0 AND sth_evraktip=12 AND sth_evrakno_seri='" + _EvrakSeri + "' AND sth_evrakno_sira=" + _EvrakSira + " ORDER BY sth_satirno DESC", sqlConnection2).ExecuteScalar();
					num4 = ((obj2 != null && !(obj2 is DBNull)) ? ((int)obj2 + 1) : 0);
					DateTime dateTime = new DateTime(DateTime.Now.Year, 1, 1);
					STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI.sth_create_user = _mikrouygulamabilgileri.mikrokullanici.User_no;
					sTOK_HAREKETLERI.sth_lastup_user = _mikrouygulamabilgileri.mikrokullanici.User_no;
					sTOK_HAREKETLERI.sth_firmano = _mikrouygulamabilgileri.FirmaNo;
					sTOK_HAREKETLERI.sth_subeno = _mikrouygulamabilgileri.SubeNo;
					sTOK_HAREKETLERI.sth_tarih = dateTime;
					sTOK_HAREKETLERI.sth_special1 = "FORA";
					sTOK_HAREKETLERI.sth_special2 = "TMP";
					sTOK_HAREKETLERI.sth_special3 = "DVR";
					sTOK_HAREKETLERI.sth_evrakno_seri = _EvrakSeri;
					sTOK_HAREKETLERI.sth_evrakno_sira = _EvrakSira;
					sTOK_HAREKETLERI.sth_satirno = num4;
					sTOK_HAREKETLERI.sth_belge_no = "";
					sTOK_HAREKETLERI.sth_belge_tarih = dateTime;
					sTOK_HAREKETLERI.sth_stok_kod = item;
					sTOK_HAREKETLERI.sth_netagirlik = 0.0;
					sTOK_HAREKETLERI.sth_cikis_depo_no = 0;
					sTOK_HAREKETLERI.sth_giris_depo_no = _HedefDepoNo;
					sTOK_HAREKETLERI.sth_nakliyedeposu = 0;
					sTOK_HAREKETLERI.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
					sTOK_HAREKETLERI.sth_maliyet_ana = 0.0;
					sTOK_HAREKETLERI.sth_maliyet_alternatif = 0.0;
					sTOK_HAREKETLERI.sth_maliyet_orjinal = 0.0;
					sTOK_HAREKETLERI.sth_parti_kodu = "";
					sTOK_HAREKETLERI.sth_lot_no = 0;
					sTOK_HAREKETLERI.sth_cari_grup_no = 0;
					sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Giris;
					sTOK_HAREKETLERI.sth_cins = enum_sth_cins.StokAcilis;
					sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
					sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.DepoGirisFisi;
					sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
					sTOK_HAREKETLERI.sth_kons_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_kons_recid_recno = 0;
					sTOK_HAREKETLERI.sth_subesip_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_subesip_recid_recno = 0;
					sTOK_HAREKETLERI.sth_satistipi = enum_sth_satistipi.Kredilisatis;
					sTOK_HAREKETLERI.sth_proje_kodu = "";
					sTOK_HAREKETLERI.sth_ihracat_kredi_kodu = "";
					sTOK_HAREKETLERI.sth_otv_pntr = 0;
					sTOK_HAREKETLERI.sth_otv_vergi = 0.0;
					sTOK_HAREKETLERI.sth_bkm_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_bkm_recid_recno = 0;
					sTOK_HAREKETLERI.sth_karsikons_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_karsikons_recid_recno = 0;
					sTOK_HAREKETLERI.sth_iade_evrak_seri = "";
					sTOK_HAREKETLERI.sth_iade_evrak_sira = 0;
					sTOK_HAREKETLERI.sth_diib_belge_no = "";
					sTOK_HAREKETLERI.sth_diib_satir_no = 0;
					sTOK_HAREKETLERI.sth_mensey_ulke_tipi = 0;
					sTOK_HAREKETLERI.sth_mensey_ulke_kodu = "";
					sTOK_HAREKETLERI.sth_brutagirlik = 0.0;
					sTOK_HAREKETLERI.sth_halrehmiktari = 0.0;
					sTOK_HAREKETLERI.sth_halrehfiyati = 0.0;
					sTOK_HAREKETLERI.sth_halsandikmiktari = 0.0;
					sTOK_HAREKETLERI.sth_halsandikfiyati = 0.0;
					sTOK_HAREKETLERI.sth_halsandikkdvtutari = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_1 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_2 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_3 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_4 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_5 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_1 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_2 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_3 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_4 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_5 = 0.0;
					sTOK_HAREKETLERI.sth_yat_tes_kodu = "";
					sTOK_HAREKETLERI.sth_oiv_pntr = 0;
					sTOK_HAREKETLERI.sth_oiv_vergi = 0.0;
					sTOK_HAREKETLERI.sth_malkbl_sevk_tarihi = dateTime;
					sTOK_HAREKETLERI.sth_oivvergisiz_fl = false;
					sTOK_HAREKETLERI.sth_otvvergisiz_fl = false;
					sTOK_HAREKETLERI.sth_disticaret_turu = enum_sth_disticaret_turu.ToptanYurticiTicaret;
					sTOK_HAREKETLERI.sth_fiyat_liste_no = 0;
					sTOK_HAREKETLERI.sth_fis_sirano2 = 0;
					sTOK_HAREKETLERI.sth_rez_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_rez_recid_recno = 0;
					sTOK_HAREKETLERI.sth_fiyfark_esas_evrak_seri = "";
					sTOK_HAREKETLERI.sth_fiyfark_esas_evrak_sira = 0;
					sTOK_HAREKETLERI.sth_fiyfark_esas_satir_no = 0;
					sTOK_HAREKETLERI.sth_optamam_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_optamam_recid_recno = 0;
					sTOK_HAREKETLERI.sth_oivtutari = 0.0;
					sTOK_HAREKETLERI.sth_Tevkifat_turu = enum_sth_Tevkifat_turu.TevkifatYok;
					sTOK_HAREKETLERI.sth_HalKomisyonuKdv = 0.0;
					sTOK_HAREKETLERI.sth_iadeTlp_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_iadeTlp_recid_recno = 0;
					sTOK_HAREKETLERI.sth_HalSatisRecid_dbcno = 0;
					sTOK_HAREKETLERI.sth_HalSatisRecid_recno = 0;
					sTOK_HAREKETLERI.sth_ciroprim_dbcno = 0;
					sTOK_HAREKETLERI.sth_ciroprim_recno = 0;
					sTOK_HAREKETLERI.sth_yetkili_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_yetkili_recid_recno = 0;
					sTOK_HAREKETLERI.sth_taxfree_fl = false;
					sTOK_HAREKETLERI.sth_HalRusum = 0.0;
					sTOK_HAREKETLERI.sth_isk_mas1 = 0;
					sTOK_HAREKETLERI.sth_isk_mas2 = 0;
					sTOK_HAREKETLERI.sth_isk_mas3 = 0;
					sTOK_HAREKETLERI.sth_isk_mas4 = 0;
					sTOK_HAREKETLERI.sth_isk_mas5 = 0;
					sTOK_HAREKETLERI.sth_isk_mas6 = 0;
					sTOK_HAREKETLERI.sth_isk_mas7 = 0;
					sTOK_HAREKETLERI.sth_isk_mas8 = 0;
					sTOK_HAREKETLERI.sth_isk_mas9 = 0;
					sTOK_HAREKETLERI.sth_isk_mas10 = 0;
					sTOK_HAREKETLERI.sth_miktar = num2;
					sTOK_HAREKETLERI.sth_miktar2 = 0.0;
					sTOK_HAREKETLERI.sth_birim_pntr = 1;
					sTOK_HAREKETLERI.sth_tutar = 0.0;
					sTOK_HAREKETLERI.sth_otvtutari = 0.0;
					sTOK_HAREKETLERI.sth_otv_pntr = 0;
					sTOK_HAREKETLERI.sth_otv_vergi = 0.0;
					sTOK_HAREKETLERI.sth_iskonto1 = 0.0;
					sTOK_HAREKETLERI.sth_iskonto2 = 0.0;
					sTOK_HAREKETLERI.sth_iskonto3 = 0.0;
					sTOK_HAREKETLERI.sth_iskonto4 = 0.0;
					sTOK_HAREKETLERI.sth_iskonto5 = 0.0;
					sTOK_HAREKETLERI.sth_iskonto6 = 0.0;
					sTOK_HAREKETLERI.sth_masraf1 = 0.0;
					sTOK_HAREKETLERI.sth_masraf2 = 0.0;
					sTOK_HAREKETLERI.sth_masraf3 = 0.0;
					sTOK_HAREKETLERI.sth_masraf4 = 0.0;
					sTOK_HAREKETLERI.sth_vergi_pntr = 0;
					sTOK_HAREKETLERI.sth_vergi = 0.0;
					sTOK_HAREKETLERI.sth_masraf_vergi_pntr = 0;
					sTOK_HAREKETLERI.sth_masraf_vergi = 0.0;
					sTOK_HAREKETLERI.sth_plasiyer_kodu = "";
					sTOK_HAREKETLERI.sth_cari_kodu = "";
					sTOK_HAREKETLERI.sth_kur_tarihi = DateTime.Now;
					sTOK_HAREKETLERI.sth_har_doviz_cinsi = 0;
					sTOK_HAREKETLERI.sth_har_doviz_kuru = 1.0;
					sTOK_HAREKETLERI.sth_alt_doviz_kuru = 1.0;
					sTOK_HAREKETLERI.sth_stok_doviz_cinsi = 0;
					sTOK_HAREKETLERI.sth_stok_doviz_kuru = 1.0;
					sTOK_HAREKETLERI.sth_odeme_op = 0;
					sTOK_HAREKETLERI.sth_aciklama = "";
					sTOK_HAREKETLERI.sth_cari_srm_merkezi = "";
					sTOK_HAREKETLERI.sth_stok_srm_merkezi = "";
					sTOK_HAREKETLERI.sth_adres_no = 1;
					sTOK_HAREKETLERI.sth_sip_recid_recno = 0;
					sTOK_HAREKETLERI.sth_fat_recid_recno = 0;
					List<STOK_HAREKETLERI> list3 = new List<STOK_HAREKETLERI>();
					list3.Add(sTOK_HAREKETLERI);
					SqlTransaction sqlTransaction = sqlConnection2.BeginTransaction();
					EvrakData.Stok_Hareketleri_Yaz(sqlConnection2, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, new Evrak(), list3);
					sqlTransaction.Commit();
					richTextBox_HataliIslemler.AppendText(DateTime.Now.ToString() + " Stok kartı miktarı oluşturuldu (" + item + ")" + Environment.NewLine);
					Application.DoEvents();
				}
				catch
				{
				}
				break;
			}
		}
		sqlConnection2.Close();
		sqlConnection2.Dispose();
		stopwatch.Stop();
		label_sonislemsuresi.Text = stopwatch.Elapsed.ToString();
		label_islem_aciklamasi.Text = "";
		label_son_guncelleme_zamani.Text = DateTime.Now.ToString();
		timer_islem_baslatma.Start();
	}

	private void sb_gecmisi_temizle_Click(object sender, EventArgs e)
	{
		richTextBox_HataliIslemler.Text = "";
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
		this.components = new System.ComponentModel.Container();
		this.sb_islemi_baslat = new DevExpress.XtraEditors.SimpleButton();
		this.label1 = new System.Windows.Forms.Label();
		this.label_EvrakSeri = new System.Windows.Forms.Label();
		this.label_EvrakSira = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label_HedefDepoNo = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label_KacDakikadaBir = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label_Guncellemeye_Kalan_Sure = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.timer_islem_baslatma = new System.Windows.Forms.Timer(this.components);
		this.label_son_guncelleme_zamani = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label_islem_aciklamasi = new System.Windows.Forms.Label();
		this.label_sonislemsuresi = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.richTextBox_HataliIslemler = new System.Windows.Forms.RichTextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.sb_gecmisi_temizle = new DevExpress.XtraEditors.SimpleButton();
		base.SuspendLayout();
		this.sb_islemi_baslat.Appearance.Font = new System.Drawing.Font("Tahoma", 10.5f, System.Drawing.FontStyle.Bold);
		this.sb_islemi_baslat.Appearance.Options.UseFont = true;
		this.sb_islemi_baslat.Enabled = false;
		this.sb_islemi_baslat.Location = new System.Drawing.Point(149, 194);
		this.sb_islemi_baslat.Name = "sb_islemi_baslat";
		this.sb_islemi_baslat.Size = new System.Drawing.Size(138, 28);
		this.sb_islemi_baslat.TabIndex = 0;
		this.sb_islemi_baslat.Text = "İŞLEMİ BAŞLAT";
		this.sb_islemi_baslat.Click += new System.EventHandler(sb_islemi_baslat_Click);
		this.label1.Location = new System.Drawing.Point(107, 11);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(100, 16);
		this.label1.TabIndex = 1;
		this.label1.Text = "Evrak seri :";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label_EvrakSeri.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_EvrakSeri.Location = new System.Drawing.Point(213, 11);
		this.label_EvrakSeri.Name = "label_EvrakSeri";
		this.label_EvrakSeri.Size = new System.Drawing.Size(143, 16);
		this.label_EvrakSeri.TabIndex = 2;
		this.label_EvrakSeri.Text = "evrak seri";
		this.label_EvrakSeri.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label_EvrakSira.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_EvrakSira.Location = new System.Drawing.Point(213, 37);
		this.label_EvrakSira.Name = "label_EvrakSira";
		this.label_EvrakSira.Size = new System.Drawing.Size(143, 16);
		this.label_EvrakSira.TabIndex = 4;
		this.label_EvrakSira.Text = "evrak sıra";
		this.label_EvrakSira.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label3.Location = new System.Drawing.Point(107, 37);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(100, 16);
		this.label3.TabIndex = 3;
		this.label3.Text = "Evrak sıra :";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label_HedefDepoNo.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_HedefDepoNo.Location = new System.Drawing.Point(213, 62);
		this.label_HedefDepoNo.Name = "label_HedefDepoNo";
		this.label_HedefDepoNo.Size = new System.Drawing.Size(143, 16);
		this.label_HedefDepoNo.TabIndex = 6;
		this.label_HedefDepoNo.Text = "hedef depo no";
		this.label_HedefDepoNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label4.Location = new System.Drawing.Point(107, 62);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(100, 16);
		this.label4.TabIndex = 5;
		this.label4.Text = "Hedef depo no :";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label_KacDakikadaBir.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_KacDakikadaBir.Location = new System.Drawing.Point(213, 84);
		this.label_KacDakikadaBir.Name = "label_KacDakikadaBir";
		this.label_KacDakikadaBir.Size = new System.Drawing.Size(143, 16);
		this.label_KacDakikadaBir.TabIndex = 8;
		this.label_KacDakikadaBir.Text = "kaç dakikada bir";
		this.label_KacDakikadaBir.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label5.Location = new System.Drawing.Point(55, 84);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(152, 16);
		this.label5.TabIndex = 7;
		this.label5.Text = "Kaç dakikada bir güncellensin :";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label_Guncellemeye_Kalan_Sure.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_Guncellemeye_Kalan_Sure.Location = new System.Drawing.Point(213, 158);
		this.label_Guncellemeye_Kalan_Sure.Name = "label_Guncellemeye_Kalan_Sure";
		this.label_Guncellemeye_Kalan_Sure.Size = new System.Drawing.Size(143, 16);
		this.label_Guncellemeye_Kalan_Sure.TabIndex = 10;
		this.label_Guncellemeye_Kalan_Sure.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label6.Location = new System.Drawing.Point(12, 158);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(195, 16);
		this.label6.TabIndex = 9;
		this.label6.Text = "Bir sonraki güncellemeye kalan süre :";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.timer_islem_baslatma.Interval = 1000;
		this.timer_islem_baslatma.Tick += new System.EventHandler(timer_ekran_guncelleme_Tick);
		this.label_son_guncelleme_zamani.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_son_guncelleme_zamani.Location = new System.Drawing.Point(213, 117);
		this.label_son_guncelleme_zamani.Name = "label_son_guncelleme_zamani";
		this.label_son_guncelleme_zamani.Size = new System.Drawing.Size(143, 16);
		this.label_son_guncelleme_zamani.TabIndex = 12;
		this.label_son_guncelleme_zamani.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label7.Location = new System.Drawing.Point(12, 117);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(195, 16);
		this.label7.TabIndex = 11;
		this.label7.Text = "Son güncelleme zamanı :";
		this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label_islem_aciklamasi.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_islem_aciklamasi.Location = new System.Drawing.Point(12, 225);
		this.label_islem_aciklamasi.Name = "label_islem_aciklamasi";
		this.label_islem_aciklamasi.Size = new System.Drawing.Size(416, 23);
		this.label_islem_aciklamasi.TabIndex = 13;
		this.label_islem_aciklamasi.Text = "işlem açıklaması";
		this.label_islem_aciklamasi.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label_sonislemsuresi.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_sonislemsuresi.Location = new System.Drawing.Point(213, 138);
		this.label_sonislemsuresi.Name = "label_sonislemsuresi";
		this.label_sonislemsuresi.Size = new System.Drawing.Size(143, 16);
		this.label_sonislemsuresi.TabIndex = 15;
		this.label_sonislemsuresi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label8.Location = new System.Drawing.Point(12, 138);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(195, 16);
		this.label8.TabIndex = 14;
		this.label8.Text = "Son işlem süresi :";
		this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.richTextBox_HataliIslemler.Location = new System.Drawing.Point(12, 282);
		this.richTextBox_HataliIslemler.Name = "richTextBox_HataliIslemler";
		this.richTextBox_HataliIslemler.Size = new System.Drawing.Size(416, 202);
		this.richTextBox_HataliIslemler.TabIndex = 16;
		this.richTextBox_HataliIslemler.Text = "";
		this.label2.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label2.Location = new System.Drawing.Point(12, 263);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(143, 16);
		this.label2.TabIndex = 17;
		this.label2.Text = "İşlem geçmişi";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.sb_gecmisi_temizle.Appearance.Font = new System.Drawing.Font("Tahoma", 10.5f, System.Drawing.FontStyle.Bold);
		this.sb_gecmisi_temizle.Appearance.Options.UseFont = true;
		this.sb_gecmisi_temizle.Location = new System.Drawing.Point(99, 258);
		this.sb_gecmisi_temizle.Name = "sb_gecmisi_temizle";
		this.sb_gecmisi_temizle.Size = new System.Drawing.Size(20, 20);
		this.sb_gecmisi_temizle.TabIndex = 18;
		this.sb_gecmisi_temizle.Text = "X";
		this.sb_gecmisi_temizle.Click += new System.EventHandler(sb_gecmisi_temizle_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(440, 496);
		base.Controls.Add(this.sb_gecmisi_temizle);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.richTextBox_HataliIslemler);
		base.Controls.Add(this.label_sonislemsuresi);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label_islem_aciklamasi);
		base.Controls.Add(this.label_son_guncelleme_zamani);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.label_Guncellemeye_Kalan_Sure);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label_KacDakikadaBir);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label_HedefDepoNo);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label_EvrakSira);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label_EvrakSeri);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.sb_islemi_baslat);
		base.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim";
		this.Text = "Başka bir firmanın stok miktarlarının zamanlı aktarımı";
		base.Load += new System.EventHandler(KullaniciEkle_Load);
		base.ResumeLayout(false);
	}
}
