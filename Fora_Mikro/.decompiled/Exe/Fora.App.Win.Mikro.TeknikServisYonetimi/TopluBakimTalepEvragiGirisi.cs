using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Fora.Mikro;
using Fora.Mikro.Bakim;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Depolar;
using Fora.Mikro.Ekipler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Kargolar;
using Fora.Mikro.Utility;
using Fora.Mikro.Win.Form.DevEx.Controllers;

namespace Fora.App.Win.Mikro.TeknikServisYonetimi;

public class TopluBakimTalepEvragiGirisi : Form
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private DataSet _satirlar;

	private Color RenkBeyaz = Color.White;

	private Color RenkYesil = Color.LightGreen;

	private Color RenkSari = Color.Yellow;

	private Color RenkKirmizi = Color.Salmon;

	private IContainer components;

	private Label label1;

	private TextEdit te_evrak_seri;

	private DateTimePicker dtp_tarih;

	private TextEdit te_belge_no;

	private Label label3;

	private DateTimePicker dtp_belgetarihi;

	private Label label7;

	private System.Windows.Forms.ComboBox cb_talep_gelis_sekli;

	private TextEdit te_kargo_no;

	private TextEdit te_irsaliye_no;

	private Label label10;

	private System.Windows.Forms.ComboBox cb_servis_turu;

	private Label label11;

	private System.Windows.Forms.ComboBox cb_servis_yeri;

	private Label label12;

	private System.Windows.Forms.ComboBox cb_teslim_edilme_sekli;

	private Label label13;

	private System.Windows.Forms.ComboBox cb_musteri_bilgilendirme_sekli;

	private Label label14;

	private GridControl gc;

	private GridView gv;

	private GridColumn gc_evrak_sira;

	private GridColumn gc_cihaz;

	private GridColumn gc_stok_kodu;

	private GridColumn gc_stok_adi;

	private GridColumn gc_aksesuarlar;

	private GridColumn gc_bildirilen_arizalar;

	private GridColumn gc_hizmet_kodu;

	private GridColumn gc_sure;

	private GridColumn gc_miktar;

	private GridColumn gc_aciklama;

	private TextEdit te_cihaz_seri_no;

	private Label label2;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem yeniToolStripMenuItem;

	private ToolStripMenuItem mikroyaKaydetToolStripMenuItem;

	private ToolStripMenuItem kapatToolStripMenuItem;

	private SonKullaniciSecimi sonKullaniciSecimi1;

	private EkipKoduSecimi ekipKoduSecimi1;

	private DepoSecimi depoSecimi1;

	private KargoSecimi kargoSecimi1;

	private CariSecimi cariSecimi1;

	private GridColumn gc_kayit_durumu;

	private RepositoryItemTextEdit repositoryItemTextEdit1;

	private ToolStripMenuItem işlemlerToolStripMenuItem;

	private ToolStripMenuItem aktifSatırıSilToolStripMenuItem;

	public TopluBakimTalepEvragiGirisi(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		sonKullaniciSecimi1._mikrouygulamabilgileri = _mikrouygulamabilgileri;
		ekipKoduSecimi1._mikrouygulamabilgileri = _mikrouygulamabilgileri;
		depoSecimi1._mikrouygulamabilgileri = _mikrouygulamabilgileri;
		kargoSecimi1._mikrouygulamabilgileri = _mikrouygulamabilgileri;
		cariSecimi1._mikrouygulamabilgileri = _mikrouygulamabilgileri;
		Focus();
		DataTable dataSource = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 0, "Telefon" },
				new object[2] { 1, "Faks" },
				new object[2] { 2, "Mail" },
				new object[2] { 3, "Elden" },
				new object[2] { 4, "Kargo" },
				new object[2] { 5, "Kurye" },
				new object[2] { 6, "Bayii" }
			}
		};
		cb_talep_gelis_sekli.DataSource = dataSource;
		cb_talep_gelis_sekli.ValueMember = "ID";
		cb_talep_gelis_sekli.DisplayMember = "Isim";
		cb_talep_gelis_sekli.SelectedValue = 4;
		DataTable dataSource2 = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 0, "Ücretli" },
				new object[2] { 1, "Garanti dahili" },
				new object[2] { 2, "Diğer" }
			}
		};
		cb_servis_turu.DataSource = dataSource2;
		cb_servis_turu.ValueMember = "ID";
		cb_servis_turu.DisplayMember = "Isim";
		cb_servis_turu.SelectedValue = 0;
		DataTable dataSource3 = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 0, "Şirkette" },
				new object[2] { 1, "Müşteride" },
				new object[2] { 2, "Dışarda" },
				new object[2] { 3, "Diğer" },
				new object[2] { 4, "Şubede" }
			}
		};
		cb_servis_yeri.DataSource = dataSource3;
		cb_servis_yeri.ValueMember = "ID";
		cb_servis_yeri.DisplayMember = "Isim";
		cb_servis_yeri.SelectedValue = 0;
		DataTable dataSource4 = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 0, "Elden" },
				new object[2] { 1, "Kargo" },
				new object[2] { 2, "Kurye" }
			}
		};
		cb_teslim_edilme_sekli.DataSource = dataSource4;
		cb_teslim_edilme_sekli.ValueMember = "ID";
		cb_teslim_edilme_sekli.DisplayMember = "Isim";
		cb_teslim_edilme_sekli.SelectedValue = 1;
		DataTable dataSource5 = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 0, "Mail" },
				new object[2] { 1, "Sms" },
				new object[2] { 2, "Mail ve Sms" }
			}
		};
		cb_musteri_bilgilendirme_sekli.DataSource = dataSource5;
		cb_musteri_bilgilendirme_sekli.ValueMember = "ID";
		cb_musteri_bilgilendirme_sekli.DisplayMember = "Isim";
		cb_musteri_bilgilendirme_sekli.SelectedValue = 0;
		sonKullaniciSecimi1.TabStopCode = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTuketiciKodu")._GetBoolean;
		sonKullaniciSecimi1.TabStopName = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTuketiciAdi")._GetBoolean;
		te_evrak_seri.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopEvrakSeri")._GetBoolean;
		dtp_tarih.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopEvrakTarih")._GetBoolean;
		te_belge_no.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBelgeNo")._GetBoolean;
		dtp_belgetarihi.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBelgeTarihi")._GetBoolean;
		cb_talep_gelis_sekli.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli")._GetBoolean;
		kargoSecimi1.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopKargo")._GetBoolean;
		cariSecimi1.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBayi")._GetBoolean;
		te_kargo_no.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopKargoNo")._GetBoolean;
		te_irsaliye_no.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo")._GetBoolean;
		cb_servis_turu.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopServisTuru")._GetBoolean;
		cb_servis_yeri.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopServisYeri")._GetBoolean;
		cb_teslim_edilme_sekli.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTeslimSekli")._GetBoolean;
		cb_musteri_bilgilendirme_sekli.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli")._GetBoolean;
		ekipKoduSecimi1.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopEkipKodu")._GetBoolean;
		depoSecimi1.TabStop = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopDepo")._GetBoolean;
		YeniEvrak();
	}

	private void dtp_tarih_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			SelectNextControl((Control)sender, forward: true, tabStopOnly: true, nested: true, wrap: true);
		}
	}

	private void cb_talep_gelis_sekli_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			SelectNextControl((Control)sender, forward: true, tabStopOnly: true, nested: true, wrap: true);
		}
	}

	private void cb_servis_turu_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			SelectNextControl((Control)sender, forward: true, tabStopOnly: true, nested: true, wrap: true);
		}
	}

	private void cb_servis_yeri_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			SelectNextControl((Control)sender, forward: true, tabStopOnly: true, nested: true, wrap: true);
		}
	}

	private void cb_teslim_edilme_sekli_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			SelectNextControl((Control)sender, forward: true, tabStopOnly: true, nested: true, wrap: true);
		}
	}

	private void cb_musteri_bilgilendirme_sekli_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			SelectNextControl((Control)sender, forward: true, tabStopOnly: true, nested: true, wrap: true);
		}
	}

	private void yeniToolStripMenuItem_Click(object sender, EventArgs e)
	{
		YeniEvrak();
	}

	private void YeniEvrak()
	{
		sonKullaniciSecimi1.SelectedItem = null;
		DefaultDegerleriAta();
		_satirlar = new DataSet();
		DataTable dataTable = new DataTable("Satirlar");
		dataTable.Columns.Add("KayitDurumu", typeof(int));
		dataTable.Columns.Add("EvrakSira", typeof(int));
		dataTable.Columns.Add("Cihaz", typeof(string));
		dataTable.Columns.Add("StokKodu", typeof(string));
		dataTable.Columns.Add("StokAdi", typeof(string));
		dataTable.Columns.Add("Aksesuarlar", typeof(string));
		dataTable.Columns.Add("BildirilenArizalar", typeof(string));
		dataTable.Columns.Add("HizmetKodu", typeof(string));
		dataTable.Columns.Add("Sure", typeof(TimeSpan));
		dataTable.Columns.Add("Miktar", typeof(float));
		dataTable.Columns.Add("Aciklama", typeof(string));
		_satirlar.Tables.Add(dataTable);
		BindingSource bindingSource = new BindingSource();
		bindingSource.DataSource = _satirlar;
		bindingSource.DataMember = _satirlar.Tables[0].TableName;
		gc.DataSource = bindingSource;
	}

	private void DefaultDegerleriAta()
	{
		te_evrak_seri.Text = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiEvrakSeri")._GetString;
		cb_teslim_edilme_sekli.SelectedItem = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTeslimEdilmeSekli")._GetInt;
		cb_musteri_bilgilendirme_sekli.SelectedValue = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli")._GetInt;
		cb_talep_gelis_sekli.SelectedValue = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiTalepGelisSekli")._GetInt;
		if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiKargo")._GetString != "")
		{
			kargoSecimi1.SelectedItem = KargoData.GetKargo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiKargo")._GetString);
		}
		else
		{
			kargoSecimi1.SelectedItem = new Kargo();
		}
		if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiCari")._GetString != "")
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			cariSecimi1.SelectedItem = CariData.GetCariByCariKod(sqlDB.Connection, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiCari")._GetString, AdreslerTemsilciyeGore: false, "");
			sqlDB.ConnectionClose();
		}
		else
		{
			cariSecimi1.SelectedItem = new Cari();
		}
		cb_servis_turu.SelectedValue = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiServisTuru")._GetInt;
		cb_servis_yeri.SelectedValue = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiServisYeri")._GetInt;
		if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiEkipKodu")._GetString != "")
		{
			ekipKoduSecimi1.SelectedItem = EkipData.GetEkip(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiEkipKodu")._GetString);
		}
		else
		{
			ekipKoduSecimi1.SelectedItem = new Ekip();
		}
		if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiDepo")._GetInt != 0)
		{
			depoSecimi1.SelectedItem = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiDepo")._GetInt);
		}
		else
		{
			depoSecimi1.SelectedItem = new Depo();
		}
	}

	private void mikroyaKaydetToolStripMenuItem_Click(object sender, EventArgs e)
	{
		EvrakKaydet();
	}

	private bool EvrakKaydet()
	{
		if (sonKullaniciSecimi1.SelectedItem == null)
		{
			MessageBox.Show("Tüketici seçilmek zorunda!", "HATA");
			return false;
		}
		if (_satirlar.Tables[0].Rows.Count == 0)
		{
			MessageBox.Show("Cihaz eklenmemiş durumda", "HATA");
			return false;
		}
		foreach (DataRow row in _satirlar.Tables[0].Rows)
		{
			if ((string)row[2] == "")
			{
				MessageBox.Show("Cihaz seri no boş bırakılamaz!");
				return false;
			}
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		foreach (DataRow row2 in _satirlar.Tables[0].Rows)
		{
			_ = (int)row2["KayitDurumu"];
			int num = (int)row2["EvrakSira"];
			string bkmkb_cihaz_serino = (string)row2["Cihaz"];
			string bkmkb_fis_stok_kodu = (string)row2["StokKodu"];
			_ = (string)row2["StokAdi"];
			string bkmkb_aksesuarlar = (string)row2["Aksesuarlar"];
			string bkmkb_bildirilen_arizalar = (string)row2["BildirilenArizalar"];
			string bkmkb_stok_hizmet_kodu = (string)row2["HizmetKodu"];
			TimeSpan timeSpan = (TimeSpan)row2["Sure"];
			float num2 = (float)row2["Miktar"];
			string text = (string)row2["Aciklama"];
			BAKIM_KABUL_HAREKETLERI bAKIM_KABUL_HAREKETLERI = new BAKIM_KABUL_HAREKETLERI();
			bAKIM_KABUL_HAREKETLERI.bkmkb_RECno = 0;
			bAKIM_KABUL_HAREKETLERI.bkmkb_RECid_DBCno = 0;
			bAKIM_KABUL_HAREKETLERI.bkmkb_RECid_RECno = 0;
			bAKIM_KABUL_HAREKETLERI.bkmkb_Spec_Rec_no = 0;
			bAKIM_KABUL_HAREKETLERI.bkmkb_iptal = false;
			bAKIM_KABUL_HAREKETLERI.bkmkb_fileid = 147;
			bAKIM_KABUL_HAREKETLERI.bkmkb_hidden = false;
			bAKIM_KABUL_HAREKETLERI.bkmkb_kilitli = false;
			bAKIM_KABUL_HAREKETLERI.bkmkb_degisti = false;
			bAKIM_KABUL_HAREKETLERI.bkmkb_checksum = 0;
			bAKIM_KABUL_HAREKETLERI.bkmkb_create_user = 2;
			bAKIM_KABUL_HAREKETLERI.bkmkb_create_date = DateTime.Now;
			bAKIM_KABUL_HAREKETLERI.bkmkb_lastup_user = 2;
			bAKIM_KABUL_HAREKETLERI.bkmkb_lastup_date = DateTime.Now;
			bAKIM_KABUL_HAREKETLERI.bkmkb_special1 = "FORA";
			bAKIM_KABUL_HAREKETLERI.bkmkb_special2 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_special3 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_firmano = _mikrouygulamabilgileri.FirmaNo;
			bAKIM_KABUL_HAREKETLERI.bkmkb_subeno = _mikrouygulamabilgileri.SubeNo;
			bAKIM_KABUL_HAREKETLERI.bkmkb_tarihi = new DateTime(dtp_tarih.Value.Year, dtp_tarih.Value.Month, dtp_tarih.Value.Day);
			bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_seri = te_evrak_seri.Text;
			bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_sira = num;
			bAKIM_KABUL_HAREKETLERI.bkmkb_satirno = 0;
			bAKIM_KABUL_HAREKETLERI.bkmkb_belgeno = te_belge_no.Text;
			bAKIM_KABUL_HAREKETLERI.bkmkb_belge_tarihi = new DateTime(dtp_belgetarihi.Value.Year, dtp_belgetarihi.Value.Month, dtp_belgetarihi.Value.Day);
			bAKIM_KABUL_HAREKETLERI.bkmkb_cihaz_serino = bkmkb_cihaz_serino;
			bAKIM_KABUL_HAREKETLERI.bkmkb_fis_stok_kodu = bkmkb_fis_stok_kodu;
			bAKIM_KABUL_HAREKETLERI.bkmkb_tuketici_kodu = sonKullaniciSecimi1.SelectedItem.tuk_kodu;
			bAKIM_KABUL_HAREKETLERI.bkmkb_talep_gelis_sekli = (int)cb_talep_gelis_sekli.SelectedValue;
			bAKIM_KABUL_HAREKETLERI.bkmkb_gelis_kargo_kodu = kargoSecimi1.SelectedItem.krg_kodu;
			bAKIM_KABUL_HAREKETLERI.bkmkb_gelis_kargo_belgeno = te_kargo_no.Text;
			bAKIM_KABUL_HAREKETLERI.bkmkb_gelis_irsaliyeno = te_irsaliye_no.Text;
			bAKIM_KABUL_HAREKETLERI.bkmkb_servis_turu = (int)cb_servis_turu.SelectedValue;
			bAKIM_KABUL_HAREKETLERI.bkmkb_servis_yeri = (int)cb_servis_yeri.SelectedValue;
			bAKIM_KABUL_HAREKETLERI.bkmkb_aksesuarlar = bkmkb_aksesuarlar;
			bAKIM_KABUL_HAREKETLERI.bkmkb_bildirilen_arizalar = bkmkb_bildirilen_arizalar;
			bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_alinma_tarihi = DateTime.Now;
			bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_edilme_tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_edilme_sekli = (int)cb_teslim_edilme_sekli.SelectedValue;
			bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu1 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu2 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu3 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu4 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu5 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu6 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu7 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu8 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu9 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu10 = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_bilgilendirme_sekli = (int)cb_musteri_bilgilendirme_sekli.SelectedValue;
			bAKIM_KABUL_HAREKETLERI.bkmkb_inceleyecek_ekip_kodu = "";
			bAKIM_KABUL_HAREKETLERI.bkmkb_depono = depoSecimi1.SelectedItem.dep_no;
			bAKIM_KABUL_HAREKETLERI.bkmkb_aciklama = text;
			bAKIM_KABUL_HAREKETLERI.bkmkb_hareket_tipi = 1;
			bAKIM_KABUL_HAREKETLERI.bkmkb_stok_hizmet_kodu = bkmkb_stok_hizmet_kodu;
			bAKIM_KABUL_HAREKETLERI.bkmkb_operasyon_suresi = int.Parse(timeSpan.TotalSeconds.ToString());
			bAKIM_KABUL_HAREKETLERI.bkmkb_miktari = num2;
			bAKIM_KABUL_HAREKETLERI.bkmkb_satir_aciklama = text;
			bAKIM_KABUL_HAREKETLERI.bkmkb_planlandi_fl = false;
			bAKIM_KABUL_HAREKETLERI.bkmkb_adres_no = 0;
			Evrak evrak = new Evrak(enum_GenelEvrakTipleri.BakimTalep, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
			evrak.SetEvrakNoSeri(te_evrak_seri.Text);
			evrak.SetEvraknoSira(num);
			evrak.AddBakimKabulHareketi(bAKIM_KABUL_HAREKETLERI);
			SqlTransaction sqlTransaction = sqlDB.Connection.BeginTransaction();
			try
			{
				int num3 = EvrakData.EvrakKaydet(sqlDB.Connection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak, EArsivAktif: false);
				if (num3 > 0)
				{
					row2["EvrakSira"] = num3;
					row2["KayitDurumu"] = enum_GenelEvrakAktarimDurumu.Aktarilmis;
				}
				else
				{
					row2["KayitDurumu"] = enum_GenelEvrakAktarimDurumu.EvrakVar;
				}
				sqlTransaction.Commit();
			}
			catch
			{
				sqlTransaction.Rollback();
				row2["KayitDurumu"] = enum_GenelEvrakAktarimDurumu.BeklenmedikHata;
			}
		}
		sqlDB.ConnectionClose();
		return true;
	}

	private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void TopluBakimTalepEvragiGirisi_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (base.DialogResult != DialogResult.OK && MessageBox.Show("Çıkmak istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
		{
			e.Cancel = true;
		}
	}

	private void cb_talep_gelis_sekli_SelectedValueChanged(object sender, EventArgs e)
	{
		try
		{
			switch ((int)cb_talep_gelis_sekli.SelectedValue)
			{
			case 4:
				kargoSecimi1.Enabled = true;
				cariSecimi1.Enabled = false;
				break;
			case 5:
				kargoSecimi1.Enabled = true;
				cariSecimi1.Enabled = false;
				break;
			case 6:
				kargoSecimi1.Enabled = false;
				cariSecimi1.Enabled = true;
				break;
			default:
				kargoSecimi1.Enabled = false;
				cariSecimi1.Enabled = false;
				break;
			}
		}
		catch
		{
		}
	}

	private void gv_InitNewRow(object sender, InitNewRowEventArgs e)
	{
		GridView gridView = sender as GridView;
		if (gridView.FocusedColumn.FieldName != "EvrakSira")
		{
			gridView.SetRowCellValue(gridView.FocusedRowHandle, "EvrakSira", 0);
		}
		if (gridView.FocusedColumn.FieldName != "Cihaz")
		{
			gridView.SetRowCellValue(gridView.FocusedRowHandle, "Cihaz", "");
		}
		if (gridView.FocusedColumn.FieldName != "Aksesuarlar")
		{
			gridView.SetRowCellValue(gridView.FocusedRowHandle, "Aksesuarlar", "");
		}
		if (gridView.FocusedColumn.FieldName != "BildirilenArizalar")
		{
			gridView.SetRowCellValue(gridView.FocusedRowHandle, "BildirilenArizalar", "");
		}
		if (gridView.FocusedColumn.FieldName != "HizmetKodu")
		{
			gridView.SetRowCellValue(gridView.FocusedRowHandle, "HizmetKodu", _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu")._GetString);
		}
		if (gridView.FocusedColumn.FieldName != "Sure")
		{
			gridView.SetRowCellValue(gridView.FocusedRowHandle, "Sure", new TimeSpan(0, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanSure")._GetInt, 0));
		}
		if (gridView.FocusedColumn.FieldName != "Miktar")
		{
			gridView.SetRowCellValue(gridView.FocusedRowHandle, "Miktar", _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanMiktar")._GetInt);
		}
		if (gridView.FocusedColumn.FieldName != "Aciklama")
		{
			gridView.SetRowCellValue(gridView.FocusedRowHandle, "Aciklama", "");
		}
		gridView.SetRowCellValue(gridView.FocusedRowHandle, "KayitDurumu", 0);
		gridView.SetRowCellValue(gridView.FocusedRowHandle, "StokKodu", "");
		gridView.SetRowCellValue(gridView.FocusedRowHandle, "StokAdi", "");
	}

	private void gv_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
	{
		_ = (GridView)sender;
		DataRow row = ((DataRowView)e.Row).Row;
		if (Convert.IsDBNull(row["KayitDurumu"]))
		{
			row["KayitDurumu"] = enum_GenelEvrakAktarimDurumu.Aktarilmamis;
		}
		_ = e.IsSetData;
		if (!e.IsGetData)
		{
			return;
		}
		string fieldName = e.Column.FieldName;
		if (fieldName == "gc_kayit_durumu")
		{
			enum_GenelEvrakAktarimDurumu enum_GenelEvrakAktarimDurumu = (enum_GenelEvrakAktarimDurumu)row[0];
			if (enum_GenelEvrakAktarimDurumu == enum_GenelEvrakAktarimDurumu.Aktarilmamis)
			{
				e.Value = "";
			}
			else
			{
				e.Value = GenelUtilityWin.EnumToString(enum_GenelEvrakAktarimDurumu);
			}
		}
	}

	private void gv_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
	{
		if (e.RowHandle < 0)
		{
			return;
		}
		string name = e.Column.Name;
		if (name == "gc_kayit_durumu")
		{
			switch ((enum_GenelEvrakAktarimDurumu)gv.GetDataRow(e.RowHandle)[0])
			{
			case enum_GenelEvrakAktarimDurumu.Aktarilmamis:
				e.Appearance.BackColor = RenkBeyaz;
				break;
			case enum_GenelEvrakAktarimDurumu.Aktarilmis:
				e.Appearance.BackColor = RenkYesil;
				break;
			default:
				e.Appearance.BackColor = RenkKirmizi;
				break;
			}
		}
	}

	private void te_cihaz_seri_no_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return && te_cihaz_seri_no.Text != "")
		{
			string value = te_cihaz_seri_no.Text;
			te_cihaz_seri_no.Text = "";
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT sst.chz_serino, sst.chz_stok_kodu, s.sto_isim FROM STOK_SERINO_TANIMLARI AS sst WITH(NOLOCK) INNER JOIN STOKLAR AS s WITH(NOLOCK) ON s.sto_kod=sst.chz_stok_kodu WHERE chz_serino=@chz_serino";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@chz_serino", value);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				if (sqlDataReader.HasRows)
				{
					sqlDataReader.Read();
					SatirEkle(sqlDataReader.GetSafeString(0), sqlDataReader.GetSafeString(1), sqlDataReader.GetSafeString(2));
				}
			}
			sqlDB.ConnectionClose();
		}
		else if (e.KeyCode == Keys.Return)
		{
			SelectNextControl((Control)sender, forward: true, tabStopOnly: true, nested: true, wrap: true);
		}
	}

	private void SatirEkle(string cihaz_serino, string cihaz_stok_kodu, string cihaz_stok_ismi)
	{
		_satirlar.Tables[0].Rows.Add(0, 0, cihaz_serino, cihaz_stok_kodu, cihaz_stok_ismi, "", "", _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu")._GetString, new TimeSpan(0, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanSure")._GetInt, 0), _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanMiktar")._GetInt, "");
		gv.RefreshData();
	}

	private void repositoryItemTextEdit1_Validating(object sender, CancelEventArgs e)
	{
		string value = ((TextEdit)sender).Text;
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT sst.chz_serino, sst.chz_stok_kodu, s.sto_isim FROM STOK_SERINO_TANIMLARI AS sst WITH(NOLOCK) INNER JOIN STOKLAR AS s WITH(NOLOCK) ON s.sto_kod=sst.chz_stok_kodu WHERE chz_serino=@chz_serino";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@chz_serino", value);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				DataRow focusedDataRow = ((GridView)gc.FocusedView).GetFocusedDataRow();
				focusedDataRow[3] = sqlDataReader.GetSafeString(1);
				focusedDataRow[4] = sqlDataReader.GetSafeString(2);
			}
			else
			{
				e.Cancel = true;
			}
		}
		sqlDB.ConnectionClose();
	}

	private void aktifSatırıSilToolStripMenuItem_Click(object sender, EventArgs e)
	{
		gv.DeleteSelectedRows();
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
		this.label1 = new System.Windows.Forms.Label();
		this.te_evrak_seri = new DevExpress.XtraEditors.TextEdit();
		this.dtp_tarih = new System.Windows.Forms.DateTimePicker();
		this.te_belge_no = new DevExpress.XtraEditors.TextEdit();
		this.label3 = new System.Windows.Forms.Label();
		this.dtp_belgetarihi = new System.Windows.Forms.DateTimePicker();
		this.label7 = new System.Windows.Forms.Label();
		this.cb_talep_gelis_sekli = new System.Windows.Forms.ComboBox();
		this.te_kargo_no = new DevExpress.XtraEditors.TextEdit();
		this.te_irsaliye_no = new DevExpress.XtraEditors.TextEdit();
		this.label10 = new System.Windows.Forms.Label();
		this.cb_servis_turu = new System.Windows.Forms.ComboBox();
		this.label11 = new System.Windows.Forms.Label();
		this.cb_servis_yeri = new System.Windows.Forms.ComboBox();
		this.label12 = new System.Windows.Forms.Label();
		this.cb_teslim_edilme_sekli = new System.Windows.Forms.ComboBox();
		this.label13 = new System.Windows.Forms.Label();
		this.cb_musteri_bilgilendirme_sekli = new System.Windows.Forms.ComboBox();
		this.label14 = new System.Windows.Forms.Label();
		this.gc = new DevExpress.XtraGrid.GridControl();
		this.gv = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_kayit_durumu = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_evrak_sira = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_cihaz = new DevExpress.XtraGrid.Columns.GridColumn();
		this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.gc_stok_kodu = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_stok_adi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_hizmet_kodu = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_sure = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_miktar = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_aksesuarlar = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_bildirilen_arizalar = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_aciklama = new DevExpress.XtraGrid.Columns.GridColumn();
		this.te_cihaz_seri_no = new DevExpress.XtraEditors.TextEdit();
		this.label2 = new System.Windows.Forms.Label();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.yeniToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.mikroyaKaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sonKullaniciSecimi1 = new Fora.Mikro.Win.Form.DevEx.Controllers.SonKullaniciSecimi();
		this.ekipKoduSecimi1 = new Fora.Mikro.Win.Form.DevEx.Controllers.EkipKoduSecimi();
		this.depoSecimi1 = new Fora.Mikro.Win.Form.DevEx.Controllers.DepoSecimi();
		this.kargoSecimi1 = new Fora.Mikro.Win.Form.DevEx.Controllers.KargoSecimi();
		this.cariSecimi1 = new Fora.Mikro.Win.Form.DevEx.Controllers.CariSecimi();
		this.işlemlerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.aktifSatırıSilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		((System.ComponentModel.ISupportInitialize)this.te_evrak_seri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_belge_no.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kargo_no.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_irsaliye_no.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gc).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gv).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_cihaz_seri_no.Properties).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(447, 40);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(91, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Evrak seri / tarih :";
		this.te_evrak_seri.EnterMoveNextControl = true;
		this.te_evrak_seri.Location = new System.Drawing.Point(544, 37);
		this.te_evrak_seri.Name = "te_evrak_seri";
		this.te_evrak_seri.Properties.MaxLength = 6;
		this.te_evrak_seri.Size = new System.Drawing.Size(56, 20);
		this.te_evrak_seri.TabIndex = 3;
		this.dtp_tarih.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dtp_tarih.Location = new System.Drawing.Point(606, 37);
		this.dtp_tarih.Name = "dtp_tarih";
		this.dtp_tarih.Size = new System.Drawing.Size(98, 20);
		this.dtp_tarih.TabIndex = 4;
		this.dtp_tarih.KeyDown += new System.Windows.Forms.KeyEventHandler(dtp_tarih_KeyDown);
		this.te_belge_no.EnterMoveNextControl = true;
		this.te_belge_no.Location = new System.Drawing.Point(814, 37);
		this.te_belge_no.Name = "te_belge_no";
		this.te_belge_no.Properties.MaxLength = 20;
		this.te_belge_no.Size = new System.Drawing.Size(77, 20);
		this.te_belge_no.TabIndex = 5;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(722, 40);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(86, 13);
		this.label3.TabIndex = 4;
		this.label3.Text = "Belge no / tarih :";
		this.dtp_belgetarihi.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dtp_belgetarihi.Location = new System.Drawing.Point(897, 37);
		this.dtp_belgetarihi.Name = "dtp_belgetarihi";
		this.dtp_belgetarihi.Size = new System.Drawing.Size(93, 20);
		this.dtp_belgetarihi.TabIndex = 6;
		this.dtp_belgetarihi.KeyDown += new System.Windows.Forms.KeyEventHandler(dtp_tarih_KeyDown);
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(9, 66);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(88, 13);
		this.label7.TabIndex = 12;
		this.label7.Text = "Talep geliş şekli :";
		this.cb_talep_gelis_sekli.BackColor = System.Drawing.SystemColors.Window;
		this.cb_talep_gelis_sekli.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_talep_gelis_sekli.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.cb_talep_gelis_sekli.FormattingEnabled = true;
		this.cb_talep_gelis_sekli.Location = new System.Drawing.Point(103, 63);
		this.cb_talep_gelis_sekli.Name = "cb_talep_gelis_sekli";
		this.cb_talep_gelis_sekli.Size = new System.Drawing.Size(110, 21);
		this.cb_talep_gelis_sekli.TabIndex = 7;
		this.cb_talep_gelis_sekli.SelectedValueChanged += new System.EventHandler(cb_talep_gelis_sekli_SelectedValueChanged);
		this.cb_talep_gelis_sekli.KeyDown += new System.Windows.Forms.KeyEventHandler(cb_talep_gelis_sekli_KeyDown);
		this.te_kargo_no.EnterMoveNextControl = true;
		this.te_kargo_no.Location = new System.Drawing.Point(814, 63);
		this.te_kargo_no.Name = "te_kargo_no";
		this.te_kargo_no.Properties.MaxLength = 15;
		this.te_kargo_no.Size = new System.Drawing.Size(77, 20);
		this.te_kargo_no.TabIndex = 10;
		this.te_irsaliye_no.EnterMoveNextControl = true;
		this.te_irsaliye_no.Location = new System.Drawing.Point(897, 63);
		this.te_irsaliye_no.Name = "te_irsaliye_no";
		this.te_irsaliye_no.Properties.MaxLength = 15;
		this.te_irsaliye_no.Size = new System.Drawing.Size(93, 20);
		this.te_irsaliye_no.TabIndex = 11;
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(694, 66);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(114, 13);
		this.label10.TabIndex = 493;
		this.label10.Text = "Kargo no / İrsaliye no :";
		this.cb_servis_turu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_servis_turu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.cb_servis_turu.FormattingEnabled = true;
		this.cb_servis_turu.Location = new System.Drawing.Point(103, 90);
		this.cb_servis_turu.Name = "cb_servis_turu";
		this.cb_servis_turu.Size = new System.Drawing.Size(110, 21);
		this.cb_servis_turu.TabIndex = 12;
		this.cb_servis_turu.KeyDown += new System.Windows.Forms.KeyEventHandler(cb_servis_turu_KeyDown);
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(34, 93);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(63, 13);
		this.label11.TabIndex = 495;
		this.label11.Text = "Servis türü :";
		this.cb_servis_yeri.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_servis_yeri.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.cb_servis_yeri.FormattingEnabled = true;
		this.cb_servis_yeri.Location = new System.Drawing.Point(293, 90);
		this.cb_servis_yeri.Name = "cb_servis_yeri";
		this.cb_servis_yeri.Size = new System.Drawing.Size(150, 21);
		this.cb_servis_yeri.TabIndex = 13;
		this.cb_servis_yeri.KeyDown += new System.Windows.Forms.KeyEventHandler(cb_servis_yeri_KeyDown);
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(226, 93);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(61, 13);
		this.label12.TabIndex = 497;
		this.label12.Text = "Servis yeri :";
		this.cb_teslim_edilme_sekli.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_teslim_edilme_sekli.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.cb_teslim_edilme_sekli.FormattingEnabled = true;
		this.cb_teslim_edilme_sekli.Location = new System.Drawing.Point(544, 90);
		this.cb_teslim_edilme_sekli.Name = "cb_teslim_edilme_sekli";
		this.cb_teslim_edilme_sekli.Size = new System.Drawing.Size(160, 21);
		this.cb_teslim_edilme_sekli.TabIndex = 14;
		this.cb_teslim_edilme_sekli.KeyDown += new System.Windows.Forms.KeyEventHandler(cb_teslim_edilme_sekli_KeyDown);
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(471, 93);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(67, 13);
		this.label13.TabIndex = 499;
		this.label13.Text = "Teslim şekli :";
		this.cb_musteri_bilgilendirme_sekli.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_musteri_bilgilendirme_sekli.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.cb_musteri_bilgilendirme_sekli.FormattingEnabled = true;
		this.cb_musteri_bilgilendirme_sekli.Location = new System.Drawing.Point(814, 90);
		this.cb_musteri_bilgilendirme_sekli.Name = "cb_musteri_bilgilendirme_sekli";
		this.cb_musteri_bilgilendirme_sekli.Size = new System.Drawing.Size(176, 21);
		this.cb_musteri_bilgilendirme_sekli.TabIndex = 15;
		this.cb_musteri_bilgilendirme_sekli.KeyDown += new System.Windows.Forms.KeyEventHandler(cb_musteri_bilgilendirme_sekli_KeyDown);
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(713, 93);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(95, 13);
		this.label14.TabIndex = 501;
		this.label14.Text = "Bilgilendirme şekli :";
		this.gc.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.gc.Cursor = System.Windows.Forms.Cursors.Default;
		this.gc.Location = new System.Drawing.Point(15, 143);
		this.gc.MainView = this.gv;
		this.gc.Name = "gc";
		this.gc.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[1] { this.repositoryItemTextEdit1 });
		this.gc.Size = new System.Drawing.Size(981, 393);
		this.gc.TabIndex = 19;
		this.gc.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.gv });
		this.gv.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[11]
		{
			this.gc_kayit_durumu, this.gc_evrak_sira, this.gc_cihaz, this.gc_stok_kodu, this.gc_stok_adi, this.gc_hizmet_kodu, this.gc_sure, this.gc_miktar, this.gc_aksesuarlar, this.gc_bildirilen_arizalar,
			this.gc_aciklama
		});
		this.gv.GridControl = this.gc;
		this.gv.Name = "gv";
		this.gv.NewItemRowText = "Yeni cihaz ekleyin";
		this.gv.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
		this.gv.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
		this.gv.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
		this.gv.OptionsView.ShowFooter = true;
		this.gv.OptionsView.ShowGroupPanel = false;
		this.gv.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(gv_CustomDrawCell);
		this.gv.InitNewRow += new DevExpress.XtraGrid.Views.Grid.InitNewRowEventHandler(gv_InitNewRow);
		this.gv.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(gv_CustomUnboundColumnData);
		this.gc_kayit_durumu.Caption = "Kayıt durumu";
		this.gc_kayit_durumu.FieldName = "gc_kayit_durumu";
		this.gc_kayit_durumu.Name = "gc_kayit_durumu";
		this.gc_kayit_durumu.OptionsColumn.AllowEdit = false;
		this.gc_kayit_durumu.OptionsColumn.AllowFocus = false;
		this.gc_kayit_durumu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_kayit_durumu.Visible = true;
		this.gc_kayit_durumu.VisibleIndex = 0;
		this.gc_kayit_durumu.Width = 79;
		this.gc_evrak_sira.Caption = "Evrak sıra";
		this.gc_evrak_sira.FieldName = "EvrakSira";
		this.gc_evrak_sira.Name = "gc_evrak_sira";
		this.gc_evrak_sira.Visible = true;
		this.gc_evrak_sira.VisibleIndex = 1;
		this.gc_evrak_sira.Width = 57;
		this.gc_cihaz.Caption = "Cihaz";
		this.gc_cihaz.ColumnEdit = this.repositoryItemTextEdit1;
		this.gc_cihaz.FieldName = "Cihaz";
		this.gc_cihaz.Name = "gc_cihaz";
		this.gc_cihaz.Visible = true;
		this.gc_cihaz.VisibleIndex = 2;
		this.gc_cihaz.Width = 94;
		this.repositoryItemTextEdit1.AutoHeight = false;
		this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
		this.repositoryItemTextEdit1.Validating += new System.ComponentModel.CancelEventHandler(repositoryItemTextEdit1_Validating);
		this.gc_stok_kodu.Caption = "Stok kodu";
		this.gc_stok_kodu.FieldName = "StokKodu";
		this.gc_stok_kodu.Name = "gc_stok_kodu";
		this.gc_stok_kodu.OptionsColumn.AllowEdit = false;
		this.gc_stok_kodu.OptionsColumn.AllowFocus = false;
		this.gc_stok_kodu.Visible = true;
		this.gc_stok_kodu.VisibleIndex = 3;
		this.gc_stok_kodu.Width = 68;
		this.gc_stok_adi.Caption = "Stok adı";
		this.gc_stok_adi.FieldName = "StokAdi";
		this.gc_stok_adi.Name = "gc_stok_adi";
		this.gc_stok_adi.OptionsColumn.AllowEdit = false;
		this.gc_stok_adi.OptionsColumn.AllowFocus = false;
		this.gc_stok_adi.Visible = true;
		this.gc_stok_adi.VisibleIndex = 4;
		this.gc_stok_adi.Width = 95;
		this.gc_hizmet_kodu.Caption = "Hizmet kodu";
		this.gc_hizmet_kodu.FieldName = "HizmetKodu";
		this.gc_hizmet_kodu.Name = "gc_hizmet_kodu";
		this.gc_hizmet_kodu.Visible = true;
		this.gc_hizmet_kodu.VisibleIndex = 5;
		this.gc_hizmet_kodu.Width = 72;
		this.gc_sure.Caption = "Süre";
		this.gc_sure.FieldName = "Sure";
		this.gc_sure.Name = "gc_sure";
		this.gc_sure.Visible = true;
		this.gc_sure.VisibleIndex = 6;
		this.gc_sure.Width = 37;
		this.gc_miktar.Caption = "Miktar";
		this.gc_miktar.FieldName = "Miktar";
		this.gc_miktar.Name = "gc_miktar";
		this.gc_miktar.Visible = true;
		this.gc_miktar.VisibleIndex = 7;
		this.gc_miktar.Width = 39;
		this.gc_aksesuarlar.Caption = "Aksesuarlar";
		this.gc_aksesuarlar.FieldName = "Aksesuarlar";
		this.gc_aksesuarlar.Name = "gc_aksesuarlar";
		this.gc_aksesuarlar.Visible = true;
		this.gc_aksesuarlar.VisibleIndex = 8;
		this.gc_aksesuarlar.Width = 107;
		this.gc_bildirilen_arizalar.Caption = "Bildirilen arızalar";
		this.gc_bildirilen_arizalar.FieldName = "BildirilenArizalar";
		this.gc_bildirilen_arizalar.Name = "gc_bildirilen_arizalar";
		this.gc_bildirilen_arizalar.Visible = true;
		this.gc_bildirilen_arizalar.VisibleIndex = 9;
		this.gc_bildirilen_arizalar.Width = 146;
		this.gc_aciklama.Caption = "Açıklama";
		this.gc_aciklama.FieldName = "Aciklama";
		this.gc_aciklama.Name = "gc_aciklama";
		this.gc_aciklama.Visible = true;
		this.gc_aciklama.VisibleIndex = 10;
		this.gc_aciklama.Width = 169;
		this.te_cihaz_seri_no.Location = new System.Drawing.Point(814, 117);
		this.te_cihaz_seri_no.Name = "te_cihaz_seri_no";
		this.te_cihaz_seri_no.Properties.MaxLength = 15;
		this.te_cihaz_seri_no.Size = new System.Drawing.Size(176, 20);
		this.te_cihaz_seri_no.TabIndex = 18;
		this.te_cihaz_seri_no.KeyDown += new System.Windows.Forms.KeyEventHandler(te_cihaz_seri_no_KeyDown);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(682, 120);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(126, 13);
		this.label2.TabIndex = 508;
		this.label2.Text = "Eklenecek cihaz seri no :";
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.dosyaToolStripMenuItem, this.işlemlerToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(1002, 24);
		this.menuStrip1.TabIndex = 510;
		this.menuStrip1.Text = "menuStrip1";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.yeniToolStripMenuItem, this.mikroyaKaydetToolStripMenuItem, this.kapatToolStripMenuItem });
		this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
		this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
		this.dosyaToolStripMenuItem.Text = "Dosya";
		this.yeniToolStripMenuItem.Name = "yeniToolStripMenuItem";
		this.yeniToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.E | System.Windows.Forms.Keys.Alt;
		this.yeniToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
		this.yeniToolStripMenuItem.Text = "Yeni";
		this.yeniToolStripMenuItem.Click += new System.EventHandler(yeniToolStripMenuItem_Click);
		this.mikroyaKaydetToolStripMenuItem.Name = "mikroyaKaydetToolStripMenuItem";
		this.mikroyaKaydetToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.S | System.Windows.Forms.Keys.Alt;
		this.mikroyaKaydetToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
		this.mikroyaKaydetToolStripMenuItem.Text = "Mikro'ya kaydet";
		this.mikroyaKaydetToolStripMenuItem.Click += new System.EventHandler(mikroyaKaydetToolStripMenuItem_Click);
		this.kapatToolStripMenuItem.Name = "kapatToolStripMenuItem";
		this.kapatToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4 | System.Windows.Forms.Keys.Alt;
		this.kapatToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
		this.kapatToolStripMenuItem.Text = "Kapat";
		this.kapatToolStripMenuItem.Click += new System.EventHandler(kapatToolStripMenuItem_Click);
		this.sonKullaniciSecimi1._mikrouygulamabilgileri = null;
		this.sonKullaniciSecimi1.CodeEmptyMessage = "Tüketici seçiniz";
		this.sonKullaniciSecimi1.EnabledCode = true;
		this.sonKullaniciSecimi1.EnabledName = true;
		this.sonKullaniciSecimi1.FontCode = new System.Drawing.Font("Tahoma", 8.25f);
		this.sonKullaniciSecimi1.FontCodeLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.sonKullaniciSecimi1.FontName = new System.Drawing.Font("Tahoma", 8.25f);
		this.sonKullaniciSecimi1.FontNameLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.sonKullaniciSecimi1.Location = new System.Drawing.Point(15, 37);
		this.sonKullaniciSecimi1.LocationCode = new System.Drawing.Point(87, 0);
		this.sonKullaniciSecimi1.LocationCodeLabel = new System.Drawing.Point(3, 3);
		this.sonKullaniciSecimi1.LocationName = new System.Drawing.Point(277, 0);
		this.sonKullaniciSecimi1.LocationNameLabel = new System.Drawing.Point(203, 3);
		this.sonKullaniciSecimi1.Name = "sonKullaniciSecimi1";
		this.sonKullaniciSecimi1.NameEmptyMessage = "Tüketici seçiniz";
		this.sonKullaniciSecimi1.SelectedItem = null;
		this.sonKullaniciSecimi1.Size = new System.Drawing.Size(428, 20);
		this.sonKullaniciSecimi1.SizeCode = new System.Drawing.Size(110, 20);
		this.sonKullaniciSecimi1.SizeName = new System.Drawing.Size(150, 20);
		this.sonKullaniciSecimi1.TabIndex = 1;
		this.sonKullaniciSecimi1.TabStopCode = true;
		this.sonKullaniciSecimi1.TabStopName = true;
		this.sonKullaniciSecimi1.TextCodeLabel = "Tüketici kodu :";
		this.sonKullaniciSecimi1.TextNameLabel = "Tüketici adı :";
		this.sonKullaniciSecimi1.VisibleCode = true;
		this.sonKullaniciSecimi1.VisibleCodeLabel = true;
		this.sonKullaniciSecimi1.VisibleName = true;
		this.sonKullaniciSecimi1.VisibleNameLabel = true;
		this.ekipKoduSecimi1._mikrouygulamabilgileri = null;
		this.ekipKoduSecimi1.CodeEmptyMessage = "Ekip seçiniz";
		this.ekipKoduSecimi1.EnabledCode = true;
		this.ekipKoduSecimi1.FontCode = new System.Drawing.Font("Tahoma", 8.25f);
		this.ekipKoduSecimi1.FontCodeLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.ekipKoduSecimi1.Location = new System.Drawing.Point(32, 117);
		this.ekipKoduSecimi1.LocationCode = new System.Drawing.Point(70, 0);
		this.ekipKoduSecimi1.LocationCodeLabel = new System.Drawing.Point(3, 3);
		this.ekipKoduSecimi1.Name = "ekipKoduSecimi1";
		this.ekipKoduSecimi1.SelectedItem = null;
		this.ekipKoduSecimi1.Size = new System.Drawing.Size(181, 20);
		this.ekipKoduSecimi1.SizeCode = new System.Drawing.Size(110, 20);
		this.ekipKoduSecimi1.TabIndex = 16;
		this.ekipKoduSecimi1.TextCodeLabel = "Ekip kodu :";
		this.ekipKoduSecimi1.VisibleCode = true;
		this.ekipKoduSecimi1.VisibleCodeLabel = true;
		this.depoSecimi1._mikrouygulamabilgileri = null;
		this.depoSecimi1.CodeEmptyMessage = "Depo seçiniz";
		this.depoSecimi1.EnabledCode = true;
		this.depoSecimi1.FontCode = new System.Drawing.Font("Tahoma", 8.25f);
		this.depoSecimi1.FontCodeLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.depoSecimi1.Location = new System.Drawing.Point(245, 117);
		this.depoSecimi1.LocationCode = new System.Drawing.Point(48, 0);
		this.depoSecimi1.LocationCodeLabel = new System.Drawing.Point(3, 3);
		this.depoSecimi1.Name = "depoSecimi1";
		this.depoSecimi1.SelectedItem = null;
		this.depoSecimi1.Size = new System.Drawing.Size(198, 20);
		this.depoSecimi1.SizeCode = new System.Drawing.Size(150, 20);
		this.depoSecimi1.TabIndex = 17;
		this.depoSecimi1.TextCodeLabel = "Depo :";
		this.depoSecimi1.VisibleCode = true;
		this.depoSecimi1.VisibleCodeLabel = true;
		this.kargoSecimi1._mikrouygulamabilgileri = null;
		this.kargoSecimi1.CodeEmptyMessage = "Kargo seçiniz";
		this.kargoSecimi1.EnabledCode = true;
		this.kargoSecimi1.FontCode = new System.Drawing.Font("Tahoma", 8.25f);
		this.kargoSecimi1.FontCodeLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.kargoSecimi1.Location = new System.Drawing.Point(242, 64);
		this.kargoSecimi1.LocationCode = new System.Drawing.Point(50, 0);
		this.kargoSecimi1.LocationCodeLabel = new System.Drawing.Point(3, 3);
		this.kargoSecimi1.Name = "kargoSecimi1";
		this.kargoSecimi1.SelectedItem = null;
		this.kargoSecimi1.Size = new System.Drawing.Size(181, 20);
		this.kargoSecimi1.SizeCode = new System.Drawing.Size(128, 20);
		this.kargoSecimi1.TabIndex = 8;
		this.kargoSecimi1.TextCodeLabel = "Kargo :";
		this.kargoSecimi1.VisibleCode = true;
		this.kargoSecimi1.VisibleCodeLabel = true;
		this.cariSecimi1._mikrouygulamabilgileri = null;
		this.cariSecimi1.CodeEmptyMessage = "Bayi seçiniz";
		this.cariSecimi1.EnabledCode = true;
		this.cariSecimi1.EnabledName = false;
		this.cariSecimi1.FontCode = new System.Drawing.Font("Tahoma", 8.25f);
		this.cariSecimi1.FontCodeLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.cariSecimi1.FontName = new System.Drawing.Font("Tahoma", 8.25f);
		this.cariSecimi1.FontNameLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.cariSecimi1.Location = new System.Drawing.Point(503, 64);
		this.cariSecimi1.LocationCode = new System.Drawing.Point(40, 0);
		this.cariSecimi1.LocationCodeLabel = new System.Drawing.Point(3, 3);
		this.cariSecimi1.LocationName = new System.Drawing.Point(253, 0);
		this.cariSecimi1.LocationNameLabel = new System.Drawing.Point(183, 3);
		this.cariSecimi1.Name = "cariSecimi1";
		this.cariSecimi1.NameEmptyMessage = "Bayi seçiniz";
		this.cariSecimi1.SelectedItem = null;
		this.cariSecimi1.Size = new System.Drawing.Size(185, 20);
		this.cariSecimi1.SizeCode = new System.Drawing.Size(140, 20);
		this.cariSecimi1.SizeName = new System.Drawing.Size(172, 20);
		this.cariSecimi1.TabIndex = 9;
		this.cariSecimi1.TextCodeLabel = "Bayi :";
		this.cariSecimi1.TextNameLabel = "Cari ünvan :";
		this.cariSecimi1.VisibleCode = true;
		this.cariSecimi1.VisibleCodeLabel = true;
		this.cariSecimi1.VisibleName = false;
		this.cariSecimi1.VisibleNameLabel = false;
		this.işlemlerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.aktifSatırıSilToolStripMenuItem });
		this.işlemlerToolStripMenuItem.Name = "işlemlerToolStripMenuItem";
		this.işlemlerToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
		this.işlemlerToolStripMenuItem.Text = "İşlemler";
		this.aktifSatırıSilToolStripMenuItem.Name = "aktifSatırıSilToolStripMenuItem";
		this.aktifSatırıSilToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Q | System.Windows.Forms.Keys.Alt;
		this.aktifSatırıSilToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.aktifSatırıSilToolStripMenuItem.Text = "Aktif satırı sil";
		this.aktifSatırıSilToolStripMenuItem.Click += new System.EventHandler(aktifSatırıSilToolStripMenuItem_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1002, 538);
		base.Controls.Add(this.cariSecimi1);
		base.Controls.Add(this.kargoSecimi1);
		base.Controls.Add(this.depoSecimi1);
		base.Controls.Add(this.ekipKoduSecimi1);
		base.Controls.Add(this.sonKullaniciSecimi1);
		base.Controls.Add(this.te_cihaz_seri_no);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.gc);
		base.Controls.Add(this.cb_musteri_bilgilendirme_sekli);
		base.Controls.Add(this.label14);
		base.Controls.Add(this.cb_teslim_edilme_sekli);
		base.Controls.Add(this.label13);
		base.Controls.Add(this.cb_servis_yeri);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.cb_servis_turu);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.te_irsaliye_no);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.te_kargo_no);
		base.Controls.Add(this.cb_talep_gelis_sekli);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.dtp_belgetarihi);
		base.Controls.Add(this.te_belge_no);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.dtp_tarih);
		base.Controls.Add(this.te_evrak_seri);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "TopluBakimTalepEvragiGirisi";
		this.Text = "Toplu bakım talep evrağı girişi";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(TopluBakimTalepEvragiGirisi_FormClosing);
		((System.ComponentModel.ISupportInitialize)this.te_evrak_seri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_belge_no.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kargo_no.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_irsaliye_no.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gc).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gv).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_cihaz_seri_no.Properties).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
