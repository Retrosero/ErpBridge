using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using Fora.Mikro;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Depolar;
using Fora.Mikro.Ekipler;
using Fora.Mikro.Kargolar;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Win.Form.DevEx.Controllers;

namespace Fora.App.Win.Mikro.Ayarlar;

public class KullaniciDuzenleme : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _kullaniciparametreleri = new Parametreler();

	private bool DegisiklikVar;

	private string AktifKullanici;

	private IContainer components;

	private XtraTabControl tc_parametrevehaklar;

	private XtraTabPage xtraTabPage1;

	private XtraTabControl xtraTabControl2;

	private XtraTabPage xtraTabPage3;

	private ListBoxControl lb_kullanicilar;

	private TextEdit te_kullanici_adi;

	private LabelControl labelControl1;

	private SimpleButton sb_ayarlari_kaydet;

	private XtraTabPage xtraTabPage2;

	private CheckEdit DepoKaliteKontrolYonetimiKayitHikayesiGorebilir;

	private LabelControl labelControl19;

	private LabelControl labelControl20;

	private System.Windows.Forms.ComboBox DepoKaliteKontrolYonetimiIadeDepo;

	private LabelControl labelControl17;

	private LabelControl labelControl18;

	private System.Windows.Forms.ComboBox DepoKaliteKontrolYonetimiHurdaDepo;

	private LabelControl labelControl16;

	private LabelControl labelControl15;

	private LabelControl labelControl13;

	private LabelControl labelControl14;

	private TextEdit DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri;

	private LabelControl labelControl7;

	private LabelControl labelControl8;

	private System.Windows.Forms.ComboBox DepoKaliteKontrolYonetimiRetDepo;

	private LabelControl labelControl5;

	private LabelControl labelControl6;

	private System.Windows.Forms.ComboBox DepoKaliteKontrolYonetimiOnayDepo;

	private LabelControl labelControl38;

	private LabelControl labelControl4;

	private System.Windows.Forms.ComboBox DepoKaliteKontrolYonetimiKaynakDepo;

	private CheckEdit DepoKaliteKontrolYonetimiIslemGirisiYapabilir;

	private LabelControl labelControl9;

	private CheckEdit DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar;

	private CheckEdit DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar;

	private CheckEdit DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir;

	private SpinEdit DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi;

	private LabelControl labelControl10;

	private LabelControl labelControl11;

	private LabelControl labelControl12;

	private SpinEdit DepoKaliteKontrolYonetimiGunSayisi;

	private LabelControl labelControl21;

	private XtraTabPage xtraTabPage4;

	private CheckEdit BankaAktarimiParametreleriDuzenleyebilir;

	private CheckEdit BankaAktarimiYapabilir;

	private CheckEdit DepoKaliteKontrolYonetimiIadeDepoDegistirebilir;

	private CheckEdit DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir;

	private CheckEdit DepoKaliteKontrolYonetimiRetDepoDegistirebilir;

	private CheckEdit DepoKaliteKontrolYonetimiOnayDepoDegistirebilir;

	private CheckEdit DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir;

	private XtraTabPage xtraTabPage5;

	private XtraTabControl xtraTabControl1;

	private XtraTabPage xtraTabPage6;

	private LabelControl labelControl35;

	private LabelControl labelControl34;

	private LabelControl labelControl33;

	private LabelControl labelControl32;

	private LabelControl labelControl31;

	private TextEdit BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri;

	private LabelControl labelControl30;

	private LabelControl labelControl28;

	private TextEdit BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu;

	private LabelControl labelControl27;

	private TextEdit BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName;

	private LabelControl labelControl26;

	private TextEdit BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre;

	private LabelControl labelControl25;

	private TextEdit BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi;

	private LabelControl labelControl24;

	private LabelControl labelControl22;

	private LabelControl labelControl23;

	private TextEdit BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer;

	private LabelControl labelControl40;

	private LabelControl labelControl39;

	private LabelControl labelControl37;

	private SpinEdit BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira;

	private SpinEdit BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir;

	private SpinEdit BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo;

	private CheckEdit BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir;

	private XtraTabPage xtraTabPage7;

	private DepoSecimi TopluBakimTalepEvragiGirisiDepo;

	private EkipKoduSecimi TopluBakimTalepEvragiGirisiEkipKodu;

	private System.Windows.Forms.ComboBox TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli;

	private Label label14;

	private System.Windows.Forms.ComboBox TopluBakimTalepEvragiGirisiTeslimEdilmeSekli;

	private Label label13;

	private System.Windows.Forms.ComboBox TopluBakimTalepEvragiGirisiServisYeri;

	private Label label12;

	private System.Windows.Forms.ComboBox TopluBakimTalepEvragiGirisiServisTuru;

	private Label label11;

	private System.Windows.Forms.ComboBox TopluBakimTalepEvragiGirisiTalepGelisSekli;

	private Label label7;

	private TextEdit TopluBakimTalepEvragiGirisiEvrakSeri;

	private Label label1;

	private CariSecimi TopluBakimTalepEvragiGirisiCari;

	private KargoSecimi TopluBakimTalepEvragiGirisiKargo;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopDepo;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopEkipKodu;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopTeslimSekli;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopServisYeri;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopServisTuru;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopKargoNo;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopBayi;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopKargo;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopBelgeTarihi;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopBelgeNo;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopEvrakTarih;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopEvrakSeri;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopTuketiciKodu;

	private LabelControl labelControl3;

	private LabelControl labelControl2;

	private CheckEdit TopluBakimTalepEvragiGirisiTabStopTuketiciAdi;

	private LabelControl labelControl36;

	private SpinEdit TopluBakimTalepEvragiGirisiVarsayilanSure;

	private LabelControl labelControl29;

	private TextEdit TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu;

	private SpinEdit TopluBakimTalepEvragiGirisiVarsayilanMiktar;

	private LabelControl labelControl41;

	public KullaniciDuzenleme(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		TopluBakimTalepEvragiGirisiEkipKodu._mikrouygulamabilgileri = _mikrouygulamabilgileri;
		TopluBakimTalepEvragiGirisiDepo._mikrouygulamabilgileri = _mikrouygulamabilgileri;
		TopluBakimTalepEvragiGirisiKargo._mikrouygulamabilgileri = _mikrouygulamabilgileri;
		TopluBakimTalepEvragiGirisiCari._mikrouygulamabilgileri = _mikrouygulamabilgileri;
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		DataTable depolarDataTable = DepoData.GetDepolarDataTable(sqlDB.Connection);
		DepoKaliteKontrolYonetimiKaynakDepo.DataSource = depolarDataTable;
		DepoKaliteKontrolYonetimiKaynakDepo.ValueMember = "DEPO NO";
		DepoKaliteKontrolYonetimiKaynakDepo.DisplayMember = "DEPO ADI";
		DataTable depolarDataTable2 = DepoData.GetDepolarDataTable(sqlDB.Connection);
		DepoKaliteKontrolYonetimiOnayDepo.DataSource = depolarDataTable2;
		DepoKaliteKontrolYonetimiOnayDepo.ValueMember = "DEPO NO";
		DepoKaliteKontrolYonetimiOnayDepo.DisplayMember = "DEPO ADI";
		DataTable depolarDataTable3 = DepoData.GetDepolarDataTable(sqlDB.Connection);
		DepoKaliteKontrolYonetimiRetDepo.DataSource = depolarDataTable3;
		DepoKaliteKontrolYonetimiRetDepo.ValueMember = "DEPO NO";
		DepoKaliteKontrolYonetimiRetDepo.DisplayMember = "DEPO ADI";
		DataTable depolarDataTable4 = DepoData.GetDepolarDataTable(sqlDB.Connection);
		DepoKaliteKontrolYonetimiHurdaDepo.DataSource = depolarDataTable4;
		DepoKaliteKontrolYonetimiHurdaDepo.ValueMember = "DEPO NO";
		DepoKaliteKontrolYonetimiHurdaDepo.DisplayMember = "DEPO ADI";
		DataTable depolarDataTable5 = DepoData.GetDepolarDataTable(sqlDB.Connection);
		DepoKaliteKontrolYonetimiIadeDepo.DataSource = depolarDataTable5;
		DepoKaliteKontrolYonetimiIadeDepo.ValueMember = "DEPO NO";
		DepoKaliteKontrolYonetimiIadeDepo.DisplayMember = "DEPO ADI";
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
		TopluBakimTalepEvragiGirisiTalepGelisSekli.DataSource = dataSource;
		TopluBakimTalepEvragiGirisiTalepGelisSekli.ValueMember = "ID";
		TopluBakimTalepEvragiGirisiTalepGelisSekli.DisplayMember = "Isim";
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
		TopluBakimTalepEvragiGirisiServisTuru.DataSource = dataSource2;
		TopluBakimTalepEvragiGirisiServisTuru.ValueMember = "ID";
		TopluBakimTalepEvragiGirisiServisTuru.DisplayMember = "Isim";
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
		TopluBakimTalepEvragiGirisiServisYeri.DataSource = dataSource3;
		TopluBakimTalepEvragiGirisiServisYeri.ValueMember = "ID";
		TopluBakimTalepEvragiGirisiServisYeri.DisplayMember = "Isim";
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
		TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.DataSource = dataSource4;
		TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.ValueMember = "ID";
		TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.DisplayMember = "Isim";
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
		TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.DataSource = dataSource5;
		TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.ValueMember = "ID";
		TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.DisplayMember = "Isim";
		sqlDB.ConnectionClose();
		sqlDB = null;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		KullanicilariListele();
	}

	private void KullanicilariListele()
	{
		lb_kullanicilar.Items.Clear();
		foreach (DataRow row in MikroKullaniciData.GetKullanicilarDataTable(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroAnaDBName).Rows)
		{
			lb_kullanicilar.Items.Add(row[1].ToString());
		}
	}

	private void lb_kullanicilar_SelectedValueChanged(object sender, EventArgs e)
	{
		if (lb_kullanicilar.SelectedValue == null)
		{
			return;
		}
		bool flag = true;
		if (DegisiklikVar && AktifKullanici != lb_kullanicilar.SelectedValue.ToString())
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				flag = true;
				break;
			case DialogResult.No:
				flag = true;
				break;
			case DialogResult.Cancel:
				flag = false;
				break;
			}
		}
		if (AktifKullanici == lb_kullanicilar.SelectedValue.ToString())
		{
			flag = false;
		}
		if (flag)
		{
			tc_parametrevehaklar.Enabled = true;
			sb_ayarlari_kaydet.Enabled = true;
			AktifKullanici = lb_kullanicilar.SelectedValue.ToString();
			_kullaniciparametreleri = ParametrelerDefault.ForaMikroKullanici(AktifKullanici);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri, "foramikro", AktifKullanici, "", "");
			EkranBilgiGuncelle();
			DegisiklikVar = false;
		}
		else
		{
			lb_kullanicilar.SelectedItem = AktifKullanici;
		}
	}

	private void EkranBilgiGuncelle()
	{
		te_kullanici_adi.Text = AktifKullanici;
		DepoKaliteKontrolYonetimiIslemGirisiYapabilir.Checked = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiIslemGirisiYapabilir")._GetBoolean;
		DepoKaliteKontrolYonetimiKaynakDepo.SelectedValue = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKaynakDepo")._GetInt;
		DepoKaliteKontrolYonetimiOnayDepo.SelectedValue = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiOnayDepo")._GetInt;
		DepoKaliteKontrolYonetimiRetDepo.SelectedValue = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiRetDepo")._GetInt;
		DepoKaliteKontrolYonetimiHurdaDepo.SelectedValue = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiHurdaDepo")._GetInt;
		DepoKaliteKontrolYonetimiIadeDepo.SelectedValue = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiIadeDepo")._GetInt;
		DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri.Text = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri")._GetString;
		DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.Checked = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiGorebilir")._GetBoolean;
		DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.Checked = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar")._GetBoolean;
		DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.Checked = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir")._GetBoolean;
		DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.Checked = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar")._GetBoolean;
		DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.Value = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi")._GetInt;
		DepoKaliteKontrolYonetimiGunSayisi.Value = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiGunSayisi")._GetInt;
		BankaAktarimiYapabilir.Checked = _kullaniciparametreleri._GetParametre("BankaAktarimiYapabilir")._GetBoolean;
		BankaAktarimiParametreleriDuzenleyebilir.Checked = _kullaniciparametreleri._GetParametre("BankaAktarimiParametreleriDuzenleyebilir")._GetBoolean;
		DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir.Checked = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir")._GetBoolean;
		DepoKaliteKontrolYonetimiOnayDepoDegistirebilir.Checked = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiOnayDepoDegistirebilir")._GetBoolean;
		DepoKaliteKontrolYonetimiRetDepoDegistirebilir.Checked = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiRetDepoDegistirebilir")._GetBoolean;
		DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir.Checked = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir")._GetBoolean;
		DepoKaliteKontrolYonetimiIadeDepoDegistirebilir.Checked = _kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiIadeDepoDegistirebilir")._GetBoolean;
		BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir.Checked = _kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir")._GetBoolean;
		BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer.Text = _kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer")._GetString;
		BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi.Text = _kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi")._GetString;
		BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre.Text = _kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre")._GetString;
		BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName.Text = _kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName")._GetString;
		BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu.Text = _kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu")._GetString;
		BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.Value = _kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo")._GetInt;
		BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri.Text = _kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri")._GetString;
		BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.Value = _kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira")._GetInt;
		BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Value = _kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir")._GetInt;
		TopluBakimTalepEvragiGirisiEvrakSeri.Text = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiEvrakSeri")._GetString;
		TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.SelectedItem = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTeslimEdilmeSekli")._GetInt;
		TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.SelectedValue = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli")._GetInt;
		TopluBakimTalepEvragiGirisiTalepGelisSekli.SelectedValue = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTalepGelisSekli")._GetInt;
		if (_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiKargo")._GetString != "")
		{
			TopluBakimTalepEvragiGirisiKargo.SelectedItem = KargoData.GetKargo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiKargo")._GetString);
		}
		else
		{
			TopluBakimTalepEvragiGirisiKargo.SelectedItem = new Kargo();
		}
		if (_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiCari")._GetString != "")
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			TopluBakimTalepEvragiGirisiCari.SelectedItem = CariData.GetCariByCariKod(sqlDB.Connection, _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiCari")._GetString, AdreslerTemsilciyeGore: false, "");
			sqlDB.ConnectionClose();
		}
		else
		{
			TopluBakimTalepEvragiGirisiCari.SelectedItem = new Cari();
		}
		TopluBakimTalepEvragiGirisiServisTuru.SelectedValue = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiServisTuru")._GetInt;
		TopluBakimTalepEvragiGirisiServisYeri.SelectedValue = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiServisYeri")._GetInt;
		if (_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiEkipKodu")._GetString != "")
		{
			TopluBakimTalepEvragiGirisiEkipKodu.SelectedItem = EkipData.GetEkip(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiEkipKodu")._GetString);
		}
		else
		{
			TopluBakimTalepEvragiGirisiEkipKodu.SelectedItem = new Ekip();
		}
		if (_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiDepo")._GetInt != 0)
		{
			TopluBakimTalepEvragiGirisiDepo.SelectedItem = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiDepo")._GetInt);
		}
		else
		{
			TopluBakimTalepEvragiGirisiDepo.SelectedItem = new Depo();
		}
		TopluBakimTalepEvragiGirisiTabStopTuketiciKodu.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTuketiciKodu")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopTuketiciAdi.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTuketiciAdi")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopEvrakSeri.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopEvrakSeri")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopEvrakTarih.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopEvrakTarih")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopBelgeNo.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBelgeNo")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopBelgeTarihi.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBelgeTarihi")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopKargo.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopKargo")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopBayi.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBayi")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopKargoNo.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopKargoNo")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopServisTuru.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopServisTuru")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopServisYeri.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopServisYeri")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopTeslimSekli.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTeslimSekli")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopEkipKodu.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopEkipKodu")._GetBoolean;
		TopluBakimTalepEvragiGirisiTabStopDepo.Checked = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopDepo")._GetBoolean;
		TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu.Text = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu")._GetString;
		TopluBakimTalepEvragiGirisiVarsayilanSure.Value = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanSure")._GetInt;
		TopluBakimTalepEvragiGirisiVarsayilanMiktar.Value = _kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanMiktar")._GetInt;
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
		KullaniciParametreKaydet();
	}

	private void KullaniciParametreKaydet()
	{
		te_kullanici_adi.Text = AktifKullanici;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiIslemGirisiYapabilir")._SetBoolean = DepoKaliteKontrolYonetimiIslemGirisiYapabilir.Checked;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKaynakDepo")._SetInt = (int)DepoKaliteKontrolYonetimiKaynakDepo.SelectedValue;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiOnayDepo")._SetInt = (int)DepoKaliteKontrolYonetimiOnayDepo.SelectedValue;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiRetDepo")._SetInt = (int)DepoKaliteKontrolYonetimiRetDepo.SelectedValue;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiHurdaDepo")._SetInt = (int)DepoKaliteKontrolYonetimiHurdaDepo.SelectedValue;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiIadeDepo")._SetInt = (int)DepoKaliteKontrolYonetimiIadeDepo.SelectedValue;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri")._SetString = DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri.Text;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiGorebilir")._SetBoolean = DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.Checked;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar")._SetBoolean = DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.Checked;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir")._SetBoolean = DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.Checked;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar")._SetBoolean = DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.Checked;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi")._SetInt = (int)DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.Value;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiGunSayisi")._SetInt = (int)DepoKaliteKontrolYonetimiGunSayisi.Value;
		_kullaniciparametreleri._GetParametre("BankaAktarimiYapabilir")._SetBoolean = BankaAktarimiYapabilir.Checked;
		_kullaniciparametreleri._GetParametre("BankaAktarimiParametreleriDuzenleyebilir")._SetBoolean = BankaAktarimiParametreleriDuzenleyebilir.Checked;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir")._SetBoolean = DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir.Checked;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiOnayDepoDegistirebilir")._SetBoolean = DepoKaliteKontrolYonetimiOnayDepoDegistirebilir.Checked;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiRetDepoDegistirebilir")._SetBoolean = DepoKaliteKontrolYonetimiRetDepoDegistirebilir.Checked;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir")._SetBoolean = DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir.Checked;
		_kullaniciparametreleri._GetParametre("DepoKaliteKontrolYonetimiIadeDepoDegistirebilir")._SetBoolean = DepoKaliteKontrolYonetimiIadeDepoDegistirebilir.Checked;
		_kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir")._SetBoolean = BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir.Checked;
		_kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer")._SetString = BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer.Text;
		_kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi")._SetString = BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi.Text;
		_kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre")._SetString = BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre.Text;
		_kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName")._SetString = BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName.Text;
		_kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu")._SetString = BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu.Text;
		_kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo")._SetInt = (int)BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.Value;
		_kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri")._SetString = BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri.Text;
		_kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira")._SetInt = (int)BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.Value;
		_kullaniciparametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir")._SetInt = (int)BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Value;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiEvrakSeri")._SetString = TopluBakimTalepEvragiGirisiEvrakSeri.Text;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTeslimEdilmeSekli")._SetInt = (int)TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.SelectedValue;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli")._SetInt = (int)TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.SelectedValue;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli")._SetInt = (int)TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.SelectedValue;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTalepGelisSekli")._SetInt = (int)TopluBakimTalepEvragiGirisiTalepGelisSekli.SelectedValue;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiKargo")._SetString = TopluBakimTalepEvragiGirisiKargo.SelectedItem.krg_kodu;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiCari")._SetString = TopluBakimTalepEvragiGirisiCari.SelectedItem.cari_kod;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiServisTuru")._SetInt = (int)TopluBakimTalepEvragiGirisiServisTuru.SelectedValue;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiServisYeri")._SetInt = (int)TopluBakimTalepEvragiGirisiServisYeri.SelectedValue;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiEkipKodu")._SetString = TopluBakimTalepEvragiGirisiEkipKodu.SelectedItem.ekp_kodu;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiDepo")._SetInt = TopluBakimTalepEvragiGirisiDepo.SelectedItem.dep_no;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTuketiciKodu")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopTuketiciKodu.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTuketiciAdi")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopTuketiciAdi.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopEvrakSeri")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopEvrakSeri.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopEvrakTarih")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopEvrakTarih.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBelgeNo")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopBelgeNo.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBelgeTarihi")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopBelgeTarihi.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopKargo")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopKargo.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBayi")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopBayi.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopKargoNo")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopKargoNo.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopServisTuru")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopServisTuru.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopServisYeri")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopServisYeri.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopTeslimSekli")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopTeslimSekli.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopEkipKodu")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopEkipKodu.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiTabStopDepo")._SetBoolean = TopluBakimTalepEvragiGirisiTabStopDepo.Checked;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu")._SetString = TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu.Text;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanSure")._SetInt = (int)TopluBakimTalepEvragiGirisiVarsayilanSure.Value;
		_kullaniciparametreleri._GetParametre("TopluBakimTalepEvragiGirisiVarsayilanMiktar")._SetInt = (int)TopluBakimTalepEvragiGirisiVarsayilanMiktar.Value;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri);
		_kullaniciparametreleri = ParametrelerDefault.ForaMikroKullanici(AktifKullanici);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri, "foramikro", AktifKullanici, "", "");
		EkranBilgiGuncelle();
		DegisiklikVar = false;
	}

	private void Sifre_KeyDown(object sender, KeyEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void ParametreTextEdit_KeyDown(object sender, KeyEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void Goster_AnaMenu_Stok_MouseClick(object sender, MouseEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void ParametreCheckEditMouseClick(object sender, MouseEventArgs e)
	{
		DegisiklikVar = true;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Fora.App.Win.Mikro.Ayarlar.KullaniciDuzenleme));
		this.tc_parametrevehaklar = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
		this.xtraTabControl2 = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage3 = new DevExpress.XtraTab.XtraTabPage();
		this.te_kullanici_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
		this.DepoKaliteKontrolYonetimiIadeDepoDegistirebilir = new DevExpress.XtraEditors.CheckEdit();
		this.DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir = new DevExpress.XtraEditors.CheckEdit();
		this.DepoKaliteKontrolYonetimiRetDepoDegistirebilir = new DevExpress.XtraEditors.CheckEdit();
		this.DepoKaliteKontrolYonetimiOnayDepoDegistirebilir = new DevExpress.XtraEditors.CheckEdit();
		this.DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
		this.DepoKaliteKontrolYonetimiGunSayisi = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl21 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar = new DevExpress.XtraEditors.CheckEdit();
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir = new DevExpress.XtraEditors.CheckEdit();
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl19 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl20 = new DevExpress.XtraEditors.LabelControl();
		this.DepoKaliteKontrolYonetimiIadeDepo = new System.Windows.Forms.ComboBox();
		this.labelControl17 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl18 = new DevExpress.XtraEditors.LabelControl();
		this.DepoKaliteKontrolYonetimiHurdaDepo = new System.Windows.Forms.ComboBox();
		this.labelControl16 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl15 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl13 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl14 = new DevExpress.XtraEditors.LabelControl();
		this.DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri = new DevExpress.XtraEditors.TextEdit();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		this.DepoKaliteKontrolYonetimiRetDepo = new System.Windows.Forms.ComboBox();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.DepoKaliteKontrolYonetimiOnayDepo = new System.Windows.Forms.ComboBox();
		this.labelControl38 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.DepoKaliteKontrolYonetimiKaynakDepo = new System.Windows.Forms.ComboBox();
		this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir = new DevExpress.XtraEditors.CheckEdit();
		this.xtraTabPage4 = new DevExpress.XtraTab.XtraTabPage();
		this.BankaAktarimiParametreleriDuzenleyebilir = new DevExpress.XtraEditors.CheckEdit();
		this.BankaAktarimiYapabilir = new DevExpress.XtraEditors.CheckEdit();
		this.xtraTabPage5 = new DevExpress.XtraTab.XtraTabPage();
		this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage6 = new DevExpress.XtraTab.XtraTabPage();
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir = new DevExpress.XtraEditors.CheckEdit();
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira = new DevExpress.XtraEditors.SpinEdit();
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir = new DevExpress.XtraEditors.SpinEdit();
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl40 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl39 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl37 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl35 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl34 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl33 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl32 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl31 = new DevExpress.XtraEditors.LabelControl();
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri = new DevExpress.XtraEditors.TextEdit();
		this.labelControl30 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl28 = new DevExpress.XtraEditors.LabelControl();
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl27 = new DevExpress.XtraEditors.LabelControl();
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName = new DevExpress.XtraEditors.TextEdit();
		this.labelControl26 = new DevExpress.XtraEditors.LabelControl();
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre = new DevExpress.XtraEditors.TextEdit();
		this.labelControl25 = new DevExpress.XtraEditors.LabelControl();
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl24 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl22 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl23 = new DevExpress.XtraEditors.LabelControl();
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer = new DevExpress.XtraEditors.TextEdit();
		this.xtraTabPage7 = new DevExpress.XtraTab.XtraTabPage();
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciAdi = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopDepo = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopEkipKodu = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopTeslimSekli = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopServisYeri = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopServisTuru = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopKargoNo = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopBayi = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopKargo = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopBelgeTarihi = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopBelgeNo = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopEvrakTarih = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopEvrakSeri = new DevExpress.XtraEditors.CheckEdit();
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciKodu = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.TopluBakimTalepEvragiGirisiCari = new Fora.Mikro.Win.Form.DevEx.Controllers.CariSecimi();
		this.TopluBakimTalepEvragiGirisiKargo = new Fora.Mikro.Win.Form.DevEx.Controllers.KargoSecimi();
		this.TopluBakimTalepEvragiGirisiDepo = new Fora.Mikro.Win.Form.DevEx.Controllers.DepoSecimi();
		this.TopluBakimTalepEvragiGirisiEkipKodu = new Fora.Mikro.Win.Form.DevEx.Controllers.EkipKoduSecimi();
		this.TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli = new System.Windows.Forms.ComboBox();
		this.label14 = new System.Windows.Forms.Label();
		this.TopluBakimTalepEvragiGirisiTeslimEdilmeSekli = new System.Windows.Forms.ComboBox();
		this.label13 = new System.Windows.Forms.Label();
		this.TopluBakimTalepEvragiGirisiServisYeri = new System.Windows.Forms.ComboBox();
		this.label12 = new System.Windows.Forms.Label();
		this.TopluBakimTalepEvragiGirisiServisTuru = new System.Windows.Forms.ComboBox();
		this.label11 = new System.Windows.Forms.Label();
		this.TopluBakimTalepEvragiGirisiTalepGelisSekli = new System.Windows.Forms.ComboBox();
		this.label7 = new System.Windows.Forms.Label();
		this.TopluBakimTalepEvragiGirisiEvrakSeri = new DevExpress.XtraEditors.TextEdit();
		this.label1 = new System.Windows.Forms.Label();
		this.lb_kullanicilar = new DevExpress.XtraEditors.ListBoxControl();
		this.sb_ayarlari_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu = new DevExpress.XtraEditors.TextEdit();
		this.TopluBakimTalepEvragiGirisiVarsayilanSure = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl29 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl36 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl41 = new DevExpress.XtraEditors.LabelControl();
		this.TopluBakimTalepEvragiGirisiVarsayilanMiktar = new DevExpress.XtraEditors.SpinEdit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametrevehaklar).BeginInit();
		this.tc_parametrevehaklar.SuspendLayout();
		this.xtraTabPage1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl2).BeginInit();
		this.xtraTabControl2.SuspendLayout();
		this.xtraTabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		this.xtraTabPage2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiIadeDepoDegistirebilir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiRetDepoDegistirebilir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiOnayDepoDegistirebilir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiGunSayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir.Properties).BeginInit();
		this.xtraTabPage4.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.BankaAktarimiParametreleriDuzenleyebilir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BankaAktarimiYapabilir.Properties).BeginInit();
		this.xtraTabPage5.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl1).BeginInit();
		this.xtraTabControl1.SuspendLayout();
		this.xtraTabPage6.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer.Properties).BeginInit();
		this.xtraTabPage7.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopTuketiciAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopDepo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopEkipKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopTeslimSekli.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopServisYeri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopServisTuru.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopKargoNo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopBayi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopKargo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopBelgeTarihi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopBelgeNo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopEvrakTarih.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopEvrakSeri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopTuketiciKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiEvrakSeri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiVarsayilanSure.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiVarsayilanMiktar.Properties).BeginInit();
		base.SuspendLayout();
		this.tc_parametrevehaklar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tc_parametrevehaklar.Enabled = false;
		this.tc_parametrevehaklar.Location = new System.Drawing.Point(144, 12);
		this.tc_parametrevehaklar.Name = "tc_parametrevehaklar";
		this.tc_parametrevehaklar.SelectedTabPage = this.xtraTabPage1;
		this.tc_parametrevehaklar.Size = new System.Drawing.Size(778, 631);
		this.tc_parametrevehaklar.TabIndex = 4;
		this.tc_parametrevehaklar.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[1] { this.xtraTabPage1 });
		this.xtraTabPage1.Appearance.PageClient.BackColor = System.Drawing.Color.White;
		this.xtraTabPage1.Appearance.PageClient.Options.UseBackColor = true;
		this.xtraTabPage1.Controls.Add(this.xtraTabControl2);
		this.xtraTabPage1.Name = "xtraTabPage1";
		this.xtraTabPage1.Size = new System.Drawing.Size(772, 603);
		this.xtraTabPage1.Text = "Parametreler";
		this.xtraTabControl2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.xtraTabControl2.Appearance.BackColor = System.Drawing.Color.White;
		this.xtraTabControl2.Appearance.Options.UseBackColor = true;
		this.xtraTabControl2.Location = new System.Drawing.Point(3, 3);
		this.xtraTabControl2.Name = "xtraTabControl2";
		this.xtraTabControl2.SelectedTabPage = this.xtraTabPage3;
		this.xtraTabControl2.Size = new System.Drawing.Size(766, 597);
		this.xtraTabControl2.TabIndex = 0;
		this.xtraTabControl2.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[5] { this.xtraTabPage3, this.xtraTabPage2, this.xtraTabPage4, this.xtraTabPage5, this.xtraTabPage7 });
		this.xtraTabPage3.Controls.Add(this.te_kullanici_adi);
		this.xtraTabPage3.Controls.Add(this.labelControl1);
		this.xtraTabPage3.Name = "xtraTabPage3";
		this.xtraTabPage3.Size = new System.Drawing.Size(760, 569);
		this.xtraTabPage3.Text = "Genel";
		this.te_kullanici_adi.Enabled = false;
		this.te_kullanici_adi.Location = new System.Drawing.Point(144, 16);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.te_kullanici_adi.Properties.Appearance.Options.UseForeColor = true;
		this.te_kullanici_adi.Size = new System.Drawing.Size(167, 20);
		this.te_kullanici_adi.TabIndex = 2;
		this.te_kullanici_adi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(21, 19);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(117, 13);
		this.labelControl1.TabIndex = 0;
		this.labelControl1.Text = "Kullanıcı adı :";
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiIadeDepoDegistirebilir);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiRetDepoDegistirebilir);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiOnayDepoDegistirebilir);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir);
		this.xtraTabPage2.Controls.Add(this.labelControl12);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiGunSayisi);
		this.xtraTabPage2.Controls.Add(this.labelControl21);
		this.xtraTabPage2.Controls.Add(this.labelControl11);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi);
		this.xtraTabPage2.Controls.Add(this.labelControl10);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar);
		this.xtraTabPage2.Controls.Add(this.labelControl9);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir);
		this.xtraTabPage2.Controls.Add(this.labelControl19);
		this.xtraTabPage2.Controls.Add(this.labelControl20);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiIadeDepo);
		this.xtraTabPage2.Controls.Add(this.labelControl17);
		this.xtraTabPage2.Controls.Add(this.labelControl18);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiHurdaDepo);
		this.xtraTabPage2.Controls.Add(this.labelControl16);
		this.xtraTabPage2.Controls.Add(this.labelControl15);
		this.xtraTabPage2.Controls.Add(this.labelControl13);
		this.xtraTabPage2.Controls.Add(this.labelControl14);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri);
		this.xtraTabPage2.Controls.Add(this.labelControl7);
		this.xtraTabPage2.Controls.Add(this.labelControl8);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiRetDepo);
		this.xtraTabPage2.Controls.Add(this.labelControl5);
		this.xtraTabPage2.Controls.Add(this.labelControl6);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiOnayDepo);
		this.xtraTabPage2.Controls.Add(this.labelControl38);
		this.xtraTabPage2.Controls.Add(this.labelControl4);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiKaynakDepo);
		this.xtraTabPage2.Controls.Add(this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir);
		this.xtraTabPage2.Name = "xtraTabPage2";
		this.xtraTabPage2.Size = new System.Drawing.Size(760, 569);
		this.xtraTabPage2.Text = "Kalite kontrol yönetimi";
		this.DepoKaliteKontrolYonetimiIadeDepoDegistirebilir.Location = new System.Drawing.Point(304, 182);
		this.DepoKaliteKontrolYonetimiIadeDepoDegistirebilir.Name = "DepoKaliteKontrolYonetimiIadeDepoDegistirebilir";
		this.DepoKaliteKontrolYonetimiIadeDepoDegistirebilir.Properties.Caption = "Değiştirebilir";
		this.DepoKaliteKontrolYonetimiIadeDepoDegistirebilir.Size = new System.Drawing.Size(86, 19);
		this.DepoKaliteKontrolYonetimiIadeDepoDegistirebilir.TabIndex = 189;
		this.DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir.Location = new System.Drawing.Point(304, 155);
		this.DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir.Name = "DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir";
		this.DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir.Properties.Caption = "Değiştirebilir";
		this.DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir.Size = new System.Drawing.Size(86, 19);
		this.DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir.TabIndex = 188;
		this.DepoKaliteKontrolYonetimiRetDepoDegistirebilir.Location = new System.Drawing.Point(304, 128);
		this.DepoKaliteKontrolYonetimiRetDepoDegistirebilir.Name = "DepoKaliteKontrolYonetimiRetDepoDegistirebilir";
		this.DepoKaliteKontrolYonetimiRetDepoDegistirebilir.Properties.Caption = "Değiştirebilir";
		this.DepoKaliteKontrolYonetimiRetDepoDegistirebilir.Size = new System.Drawing.Size(86, 19);
		this.DepoKaliteKontrolYonetimiRetDepoDegistirebilir.TabIndex = 187;
		this.DepoKaliteKontrolYonetimiOnayDepoDegistirebilir.Location = new System.Drawing.Point(302, 101);
		this.DepoKaliteKontrolYonetimiOnayDepoDegistirebilir.Name = "DepoKaliteKontrolYonetimiOnayDepoDegistirebilir";
		this.DepoKaliteKontrolYonetimiOnayDepoDegistirebilir.Properties.Caption = "Değiştirebilir";
		this.DepoKaliteKontrolYonetimiOnayDepoDegistirebilir.Size = new System.Drawing.Size(86, 19);
		this.DepoKaliteKontrolYonetimiOnayDepoDegistirebilir.TabIndex = 186;
		this.DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir.Location = new System.Drawing.Point(302, 74);
		this.DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir.Name = "DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir";
		this.DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir.Properties.Caption = "Değiştirebilir";
		this.DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir.Size = new System.Drawing.Size(86, 19);
		this.DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir.TabIndex = 185;
		this.labelControl12.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl12.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl12.Location = new System.Drawing.Point(238, 237);
		this.labelControl12.Name = "labelControl12";
		this.labelControl12.Size = new System.Drawing.Size(509, 13);
		this.labelControl12.TabIndex = 184;
		this.labelControl12.Text = "gün öncesine kadar olan kayıtları getir.";
		this.DepoKaliteKontrolYonetimiGunSayisi.EditValue = new decimal(new int[4]);
		this.DepoKaliteKontrolYonetimiGunSayisi.Location = new System.Drawing.Point(165, 234);
		this.DepoKaliteKontrolYonetimiGunSayisi.Name = "DepoKaliteKontrolYonetimiGunSayisi";
		this.DepoKaliteKontrolYonetimiGunSayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.DepoKaliteKontrolYonetimiGunSayisi.Properties.IsFloatValue = false;
		this.DepoKaliteKontrolYonetimiGunSayisi.Properties.Mask.EditMask = "N00";
		this.DepoKaliteKontrolYonetimiGunSayisi.Size = new System.Drawing.Size(67, 20);
		this.DepoKaliteKontrolYonetimiGunSayisi.TabIndex = 183;
		this.DepoKaliteKontrolYonetimiGunSayisi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiGunSayisi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl21.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl21.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl21.Location = new System.Drawing.Point(3, 237);
		this.labelControl21.Name = "labelControl21";
		this.labelControl21.Size = new System.Drawing.Size(156, 13);
		this.labelControl21.TabIndex = 182;
		this.labelControl21.Text = "Gün sayısı :";
		this.labelControl11.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl11.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl11.Location = new System.Drawing.Point(238, 408);
		this.labelControl11.Name = "labelControl11";
		this.labelControl11.Size = new System.Drawing.Size(140, 13);
		this.labelControl11.TabIndex = 181;
		this.labelControl11.Text = "gün öncesi";
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.EditValue = new decimal(new int[4]);
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.Location = new System.Drawing.Point(165, 405);
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.Name = "DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi";
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.Properties.IsFloatValue = false;
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.Properties.Mask.EditMask = "N00";
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.Size = new System.Drawing.Size(67, 20);
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.TabIndex = 180;
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl10.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl10.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl10.Location = new System.Drawing.Point(16, 408);
		this.labelControl10.Name = "labelControl10";
		this.labelControl10.Size = new System.Drawing.Size(143, 13);
		this.labelControl10.TabIndex = 179;
		this.labelControl10.Text = "Varsayılan başlangıç tarihi :";
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.Location = new System.Drawing.Point(189, 372);
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.Name = "DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar";
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.Properties.Caption = "Bütün kullanıcıların işlemlerini silebilir";
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.Size = new System.Drawing.Size(263, 19);
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.TabIndex = 177;
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.Location = new System.Drawing.Point(163, 347);
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.Name = "DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir";
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.Properties.Caption = "İşlem silebilir";
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.Size = new System.Drawing.Size(263, 19);
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.TabIndex = 176;
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.Location = new System.Drawing.Point(189, 322);
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.Name = "DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar";
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.Properties.Caption = "Kayıt hikayesini bütün kullanıcıları görebilir";
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.Size = new System.Drawing.Size(263, 19);
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.TabIndex = 175;
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl9.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl9.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl9.Location = new System.Drawing.Point(165, 272);
		this.labelControl9.Name = "labelControl9";
		this.labelControl9.Size = new System.Drawing.Size(315, 19);
		this.labelControl9.TabIndex = 174;
		this.labelControl9.Text = "Kayıt hikayesi";
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.Location = new System.Drawing.Point(163, 297);
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.Name = "DepoKaliteKontrolYonetimiKayitHikayesiGorebilir";
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.Properties.Caption = "Kayıt hikayesini görebilir";
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.Size = new System.Drawing.Size(263, 19);
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.TabIndex = 173;
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl19.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl19.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl19.Location = new System.Drawing.Point(396, 185);
		this.labelControl19.Name = "labelControl19";
		this.labelControl19.Size = new System.Drawing.Size(229, 13);
		this.labelControl19.TabIndex = 172;
		this.labelControl19.Text = "İadeye ayrılmış ürünlerin gönderileceği depo";
		this.labelControl20.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl20.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl20.Location = new System.Drawing.Point(42, 185);
		this.labelControl20.Name = "labelControl20";
		this.labelControl20.Size = new System.Drawing.Size(117, 13);
		this.labelControl20.TabIndex = 171;
		this.labelControl20.Text = "İade depo :";
		this.DepoKaliteKontrolYonetimiIadeDepo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DepoKaliteKontrolYonetimiIadeDepo.FormattingEnabled = true;
		this.DepoKaliteKontrolYonetimiIadeDepo.Location = new System.Drawing.Point(165, 182);
		this.DepoKaliteKontrolYonetimiIadeDepo.Name = "DepoKaliteKontrolYonetimiIadeDepo";
		this.DepoKaliteKontrolYonetimiIadeDepo.Size = new System.Drawing.Size(131, 21);
		this.DepoKaliteKontrolYonetimiIadeDepo.TabIndex = 170;
		this.DepoKaliteKontrolYonetimiIadeDepo.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiIadeDepo.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl17.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl17.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl17.Location = new System.Drawing.Point(396, 158);
		this.labelControl17.Name = "labelControl17";
		this.labelControl17.Size = new System.Drawing.Size(229, 13);
		this.labelControl17.TabIndex = 169;
		this.labelControl17.Text = "Hurdaya ayrılmış ürünlerin gönderileceği depo";
		this.labelControl18.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl18.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl18.Location = new System.Drawing.Point(42, 158);
		this.labelControl18.Name = "labelControl18";
		this.labelControl18.Size = new System.Drawing.Size(117, 13);
		this.labelControl18.TabIndex = 168;
		this.labelControl18.Text = "Hurda depo :";
		this.DepoKaliteKontrolYonetimiHurdaDepo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DepoKaliteKontrolYonetimiHurdaDepo.FormattingEnabled = true;
		this.DepoKaliteKontrolYonetimiHurdaDepo.Location = new System.Drawing.Point(165, 155);
		this.DepoKaliteKontrolYonetimiHurdaDepo.Name = "DepoKaliteKontrolYonetimiHurdaDepo";
		this.DepoKaliteKontrolYonetimiHurdaDepo.Size = new System.Drawing.Size(131, 21);
		this.DepoKaliteKontrolYonetimiHurdaDepo.TabIndex = 167;
		this.DepoKaliteKontrolYonetimiHurdaDepo.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiHurdaDepo.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl16.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl16.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
		this.labelControl16.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl16.Location = new System.Drawing.Point(9, 467);
		this.labelControl16.Name = "labelControl16";
		this.labelControl16.Size = new System.Drawing.Size(731, 62);
		this.labelControl16.TabIndex = 166;
		this.labelControl16.Text = resources.GetString("labelControl16.Text");
		this.labelControl15.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl15.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl15.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl15.Location = new System.Drawing.Point(165, 24);
		this.labelControl15.Name = "labelControl15";
		this.labelControl15.Size = new System.Drawing.Size(315, 19);
		this.labelControl15.TabIndex = 165;
		this.labelControl15.Text = "İşlem girişi";
		this.labelControl13.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl13.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl13.Location = new System.Drawing.Point(302, 211);
		this.labelControl13.Name = "labelControl13";
		this.labelControl13.Size = new System.Drawing.Size(365, 13);
		this.labelControl13.TabIndex = 164;
		this.labelControl13.Text = "Depolar arası sevk fişi oluşturalacağı zaman kullanılacak olan evrak serisi";
		this.labelControl14.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl14.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl14.Location = new System.Drawing.Point(9, 211);
		this.labelControl14.Name = "labelControl14";
		this.labelControl14.Size = new System.Drawing.Size(150, 13);
		this.labelControl14.TabIndex = 163;
		this.labelControl14.Text = "Depolar arası sevk evrak seri :";
		this.DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri.Location = new System.Drawing.Point(165, 208);
		this.DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri.Name = "DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri";
		this.DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri.Properties.MaxLength = 4;
		this.DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri.Size = new System.Drawing.Size(131, 20);
		this.DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri.TabIndex = 162;
		this.DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl7.Location = new System.Drawing.Point(396, 131);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(229, 13);
		this.labelControl7.TabIndex = 161;
		this.labelControl7.Text = "Ret edilmiş ürünlerin gönderileceği depo";
		this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl8.Location = new System.Drawing.Point(42, 131);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(117, 13);
		this.labelControl8.TabIndex = 160;
		this.labelControl8.Text = "Ret depo :";
		this.DepoKaliteKontrolYonetimiRetDepo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DepoKaliteKontrolYonetimiRetDepo.FormattingEnabled = true;
		this.DepoKaliteKontrolYonetimiRetDepo.Location = new System.Drawing.Point(165, 128);
		this.DepoKaliteKontrolYonetimiRetDepo.Name = "DepoKaliteKontrolYonetimiRetDepo";
		this.DepoKaliteKontrolYonetimiRetDepo.Size = new System.Drawing.Size(131, 21);
		this.DepoKaliteKontrolYonetimiRetDepo.TabIndex = 159;
		this.DepoKaliteKontrolYonetimiRetDepo.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiRetDepo.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(396, 104);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(229, 13);
		this.labelControl5.TabIndex = 158;
		this.labelControl5.Text = "Onaylanmış ürünlerin gönderileceği depo";
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(42, 104);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(117, 13);
		this.labelControl6.TabIndex = 157;
		this.labelControl6.Text = "Onay depo :";
		this.DepoKaliteKontrolYonetimiOnayDepo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DepoKaliteKontrolYonetimiOnayDepo.FormattingEnabled = true;
		this.DepoKaliteKontrolYonetimiOnayDepo.Location = new System.Drawing.Point(165, 101);
		this.DepoKaliteKontrolYonetimiOnayDepo.Name = "DepoKaliteKontrolYonetimiOnayDepo";
		this.DepoKaliteKontrolYonetimiOnayDepo.Size = new System.Drawing.Size(131, 21);
		this.DepoKaliteKontrolYonetimiOnayDepo.TabIndex = 156;
		this.DepoKaliteKontrolYonetimiOnayDepo.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiOnayDepo.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl38.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl38.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl38.Location = new System.Drawing.Point(396, 77);
		this.labelControl38.Name = "labelControl38";
		this.labelControl38.Size = new System.Drawing.Size(229, 13);
		this.labelControl38.TabIndex = 155;
		this.labelControl38.Text = "İşlem yapılacak depo";
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(42, 77);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(117, 13);
		this.labelControl4.TabIndex = 154;
		this.labelControl4.Text = "Kaynak depo :";
		this.DepoKaliteKontrolYonetimiKaynakDepo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DepoKaliteKontrolYonetimiKaynakDepo.FormattingEnabled = true;
		this.DepoKaliteKontrolYonetimiKaynakDepo.Location = new System.Drawing.Point(165, 74);
		this.DepoKaliteKontrolYonetimiKaynakDepo.Name = "DepoKaliteKontrolYonetimiKaynakDepo";
		this.DepoKaliteKontrolYonetimiKaynakDepo.Size = new System.Drawing.Size(131, 21);
		this.DepoKaliteKontrolYonetimiKaynakDepo.TabIndex = 153;
		this.DepoKaliteKontrolYonetimiKaynakDepo.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiKaynakDepo.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir.Location = new System.Drawing.Point(163, 49);
		this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir.Name = "DepoKaliteKontrolYonetimiIslemGirisiYapabilir";
		this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir.Properties.Caption = "Kalite kontrol yönetimi işlem girişi yapabilir";
		this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir.Size = new System.Drawing.Size(263, 19);
		this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir.TabIndex = 152;
		this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.xtraTabPage4.Controls.Add(this.BankaAktarimiParametreleriDuzenleyebilir);
		this.xtraTabPage4.Controls.Add(this.BankaAktarimiYapabilir);
		this.xtraTabPage4.Name = "xtraTabPage4";
		this.xtraTabPage4.Size = new System.Drawing.Size(760, 569);
		this.xtraTabPage4.Text = "Banka aktarımı";
		this.BankaAktarimiParametreleriDuzenleyebilir.Location = new System.Drawing.Point(39, 56);
		this.BankaAktarimiParametreleriDuzenleyebilir.Name = "BankaAktarimiParametreleriDuzenleyebilir";
		this.BankaAktarimiParametreleriDuzenleyebilir.Properties.Caption = "Parametreleri düzenleyebilir";
		this.BankaAktarimiParametreleriDuzenleyebilir.Size = new System.Drawing.Size(263, 19);
		this.BankaAktarimiParametreleriDuzenleyebilir.TabIndex = 154;
		this.BankaAktarimiParametreleriDuzenleyebilir.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.BankaAktarimiParametreleriDuzenleyebilir.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.BankaAktarimiYapabilir.Location = new System.Drawing.Point(39, 31);
		this.BankaAktarimiYapabilir.Name = "BankaAktarimiYapabilir";
		this.BankaAktarimiYapabilir.Properties.Caption = "Banka aktarımını giriş yapabilir";
		this.BankaAktarimiYapabilir.Size = new System.Drawing.Size(263, 19);
		this.BankaAktarimiYapabilir.TabIndex = 153;
		this.BankaAktarimiYapabilir.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.BankaAktarimiYapabilir.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.xtraTabPage5.Controls.Add(this.xtraTabControl1);
		this.xtraTabPage5.Name = "xtraTabPage5";
		this.xtraTabPage5.Size = new System.Drawing.Size(760, 569);
		this.xtraTabPage5.Text = "Zamanlanmış görevler";
		this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.xtraTabControl1.Location = new System.Drawing.Point(0, 0);
		this.xtraTabControl1.Name = "xtraTabControl1";
		this.xtraTabControl1.SelectedTabPage = this.xtraTabPage6;
		this.xtraTabControl1.Size = new System.Drawing.Size(760, 569);
		this.xtraTabControl1.TabIndex = 0;
		this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[1] { this.xtraTabPage6 });
		this.xtraTabPage6.Controls.Add(this.BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir);
		this.xtraTabPage6.Controls.Add(this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira);
		this.xtraTabPage6.Controls.Add(this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir);
		this.xtraTabPage6.Controls.Add(this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo);
		this.xtraTabPage6.Controls.Add(this.labelControl40);
		this.xtraTabPage6.Controls.Add(this.labelControl39);
		this.xtraTabPage6.Controls.Add(this.labelControl37);
		this.xtraTabPage6.Controls.Add(this.labelControl35);
		this.xtraTabPage6.Controls.Add(this.labelControl34);
		this.xtraTabPage6.Controls.Add(this.labelControl33);
		this.xtraTabPage6.Controls.Add(this.labelControl32);
		this.xtraTabPage6.Controls.Add(this.labelControl31);
		this.xtraTabPage6.Controls.Add(this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri);
		this.xtraTabPage6.Controls.Add(this.labelControl30);
		this.xtraTabPage6.Controls.Add(this.labelControl28);
		this.xtraTabPage6.Controls.Add(this.BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu);
		this.xtraTabPage6.Controls.Add(this.labelControl27);
		this.xtraTabPage6.Controls.Add(this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName);
		this.xtraTabPage6.Controls.Add(this.labelControl26);
		this.xtraTabPage6.Controls.Add(this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre);
		this.xtraTabPage6.Controls.Add(this.labelControl25);
		this.xtraTabPage6.Controls.Add(this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi);
		this.xtraTabPage6.Controls.Add(this.labelControl24);
		this.xtraTabPage6.Controls.Add(this.labelControl22);
		this.xtraTabPage6.Controls.Add(this.labelControl23);
		this.xtraTabPage6.Controls.Add(this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer);
		this.xtraTabPage6.Name = "xtraTabPage6";
		this.xtraTabPage6.Size = new System.Drawing.Size(754, 541);
		this.xtraTabPage6.Text = "Başka bir firmadan stok miktarlarını aktar";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir.Location = new System.Drawing.Point(167, 68);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir.Properties.Caption = "Giriş yapabilir";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir.Size = new System.Drawing.Size(110, 19);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir.TabIndex = 197;
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.EditValue = new decimal(new int[4]);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.Location = new System.Drawing.Point(169, 303);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.Properties.IsFloatValue = false;
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.Properties.Mask.EditMask = "N00";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.Size = new System.Drawing.Size(108, 20);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.TabIndex = 196;
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Location = new System.Drawing.Point(169, 329);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Properties.IsFloatValue = false;
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Properties.Mask.EditMask = "N00";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Properties.MaxValue = new decimal(new int[4] { 500000, 0, 0, 0 });
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Size = new System.Drawing.Size(108, 20);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.TabIndex = 195;
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.EditValue = new decimal(new int[4]);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.Location = new System.Drawing.Point(169, 251);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.Properties.IsFloatValue = false;
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.Properties.Mask.EditMask = "N00";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.Size = new System.Drawing.Size(108, 20);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.TabIndex = 194;
		this.labelControl40.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl40.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl40.Location = new System.Drawing.Point(283, 306);
		this.labelControl40.Name = "labelControl40";
		this.labelControl40.Size = new System.Drawing.Size(461, 13);
		this.labelControl40.TabIndex = 192;
		this.labelControl40.Text = "Oluşturulacak 'Devir girişi ambar fişi'nin sırası.";
		this.labelControl39.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl39.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl39.Location = new System.Drawing.Point(283, 280);
		this.labelControl39.Name = "labelControl39";
		this.labelControl39.Size = new System.Drawing.Size(461, 13);
		this.labelControl39.TabIndex = 191;
		this.labelControl39.Text = "Oluşturulacak 'Devir girişi ambar fişi'nin serisi.";
		this.labelControl37.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl37.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl37.Location = new System.Drawing.Point(283, 254);
		this.labelControl37.Name = "labelControl37";
		this.labelControl37.Size = new System.Drawing.Size(461, 13);
		this.labelControl37.TabIndex = 190;
		this.labelControl37.Text = "Aktif firmada hangi depoya aktarılacak";
		this.labelControl35.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl35.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl35.Location = new System.Drawing.Point(169, 223);
		this.labelControl35.Name = "labelControl35";
		this.labelControl35.Size = new System.Drawing.Size(321, 13);
		this.labelControl35.TabIndex = 188;
		this.labelControl35.Text = "Hangi stokların miktarları aktarılacak sorgu cümlesi ile değiştirilebilir.";
		this.labelControl34.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl34.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl34.Location = new System.Drawing.Point(283, 174);
		this.labelControl34.Name = "labelControl34";
		this.labelControl34.Size = new System.Drawing.Size(186, 13);
		this.labelControl34.TabIndex = 187;
		this.labelControl34.Text = "Stok miktarlarının çekileceği veritabanı";
		this.labelControl33.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl33.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl33.Location = new System.Drawing.Point(13, 332);
		this.labelControl33.Name = "labelControl33";
		this.labelControl33.Size = new System.Drawing.Size(150, 13);
		this.labelControl33.TabIndex = 186;
		this.labelControl33.Text = "Kaç dakikada bir güncellensin :";
		this.labelControl32.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl32.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl32.Location = new System.Drawing.Point(13, 306);
		this.labelControl32.Name = "labelControl32";
		this.labelControl32.Size = new System.Drawing.Size(150, 13);
		this.labelControl32.TabIndex = 184;
		this.labelControl32.Text = "Evrak sıra :";
		this.labelControl31.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl31.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl31.Location = new System.Drawing.Point(13, 280);
		this.labelControl31.Name = "labelControl31";
		this.labelControl31.Size = new System.Drawing.Size(150, 13);
		this.labelControl31.TabIndex = 182;
		this.labelControl31.Text = "Evrak seri :";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri.Location = new System.Drawing.Point(169, 277);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri.Properties.MaxLength = 4;
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri.Size = new System.Drawing.Size(108, 20);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri.TabIndex = 181;
		this.labelControl30.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl30.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl30.Location = new System.Drawing.Point(13, 254);
		this.labelControl30.Name = "labelControl30";
		this.labelControl30.Size = new System.Drawing.Size(150, 13);
		this.labelControl30.TabIndex = 180;
		this.labelControl30.Text = "Hedef depo no :";
		this.labelControl28.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl28.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl28.Location = new System.Drawing.Point(13, 200);
		this.labelControl28.Name = "labelControl28";
		this.labelControl28.Size = new System.Drawing.Size(150, 13);
		this.labelControl28.TabIndex = 176;
		this.labelControl28.Text = "Stok sorgu cümlesi :";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu.Location = new System.Drawing.Point(169, 197);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu.Size = new System.Drawing.Size(575, 20);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu.TabIndex = 175;
		this.labelControl27.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl27.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl27.Location = new System.Drawing.Point(13, 174);
		this.labelControl27.Name = "labelControl27";
		this.labelControl27.Size = new System.Drawing.Size(150, 13);
		this.labelControl27.TabIndex = 174;
		this.labelControl27.Text = "Sql veritabanı adı :";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName.Location = new System.Drawing.Point(169, 171);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName.Size = new System.Drawing.Size(108, 20);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName.TabIndex = 173;
		this.labelControl26.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl26.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl26.Location = new System.Drawing.Point(13, 148);
		this.labelControl26.Name = "labelControl26";
		this.labelControl26.Size = new System.Drawing.Size(150, 13);
		this.labelControl26.TabIndex = 172;
		this.labelControl26.Text = "Sql şifresi :";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre.Location = new System.Drawing.Point(169, 145);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre.Size = new System.Drawing.Size(108, 20);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre.TabIndex = 171;
		this.labelControl25.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl25.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl25.Location = new System.Drawing.Point(13, 122);
		this.labelControl25.Name = "labelControl25";
		this.labelControl25.Size = new System.Drawing.Size(150, 13);
		this.labelControl25.TabIndex = 170;
		this.labelControl25.Text = "Sql Kullanıcı adı :";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi.Location = new System.Drawing.Point(169, 119);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi.Size = new System.Drawing.Size(108, 20);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi.TabIndex = 169;
		this.labelControl24.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl24.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
		this.labelControl24.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl24.Location = new System.Drawing.Point(13, 3);
		this.labelControl24.Name = "labelControl24";
		this.labelControl24.Size = new System.Drawing.Size(731, 32);
		this.labelControl24.TabIndex = 168;
		this.labelControl24.Text = "Başka bir firmanın stok miktarlarının bilgilerini aktif firmada belirtilen depoya 'Devir girişi ambar fişi' olarak girişini sağlar.";
		this.labelControl22.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl22.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl22.Location = new System.Drawing.Point(283, 122);
		this.labelControl22.Name = "labelControl22";
		this.labelControl22.Size = new System.Drawing.Size(76, 13);
		this.labelControl22.TabIndex = 167;
		this.labelControl22.Text = "Boş bırakılabilir.";
		this.labelControl23.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl23.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl23.Location = new System.Drawing.Point(13, 96);
		this.labelControl23.Name = "labelControl23";
		this.labelControl23.Size = new System.Drawing.Size(150, 13);
		this.labelControl23.TabIndex = 166;
		this.labelControl23.Text = "Sql Server :";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer.Location = new System.Drawing.Point(169, 93);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer.Name = "BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer";
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer.Size = new System.Drawing.Size(108, 20);
		this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer.TabIndex = 165;
		this.xtraTabPage7.Appearance.PageClient.BackColor = System.Drawing.Color.Gray;
		this.xtraTabPage7.Appearance.PageClient.Options.UseBackColor = true;
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiVarsayilanMiktar);
		this.xtraTabPage7.Controls.Add(this.labelControl41);
		this.xtraTabPage7.Controls.Add(this.labelControl36);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiVarsayilanSure);
		this.xtraTabPage7.Controls.Add(this.labelControl29);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopTuketiciAdi);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopDepo);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopEkipKodu);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopTeslimSekli);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopServisYeri);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopServisTuru);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopKargoNo);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopBayi);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopKargo);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopBelgeTarihi);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopBelgeNo);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopEvrakTarih);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopEvrakSeri);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTabStopTuketiciKodu);
		this.xtraTabPage7.Controls.Add(this.labelControl3);
		this.xtraTabPage7.Controls.Add(this.labelControl2);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiCari);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiKargo);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiDepo);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiEkipKodu);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli);
		this.xtraTabPage7.Controls.Add(this.label14);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTeslimEdilmeSekli);
		this.xtraTabPage7.Controls.Add(this.label13);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiServisYeri);
		this.xtraTabPage7.Controls.Add(this.label12);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiServisTuru);
		this.xtraTabPage7.Controls.Add(this.label11);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiTalepGelisSekli);
		this.xtraTabPage7.Controls.Add(this.label7);
		this.xtraTabPage7.Controls.Add(this.TopluBakimTalepEvragiGirisiEvrakSeri);
		this.xtraTabPage7.Controls.Add(this.label1);
		this.xtraTabPage7.Name = "xtraTabPage7";
		this.xtraTabPage7.Size = new System.Drawing.Size(760, 569);
		this.xtraTabPage7.Text = "Toplu bakım talep evrağı girişi";
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciAdi.Location = new System.Drawing.Point(349, 78);
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciAdi.Name = "TopluBakimTalepEvragiGirisiTabStopTuketiciAdi";
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciAdi.Properties.Caption = "Tüketici adı";
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciAdi.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciAdi.TabIndex = 536;
		this.TopluBakimTalepEvragiGirisiTabStopDepo.Location = new System.Drawing.Point(349, 451);
		this.TopluBakimTalepEvragiGirisiTabStopDepo.Name = "TopluBakimTalepEvragiGirisiTabStopDepo";
		this.TopluBakimTalepEvragiGirisiTabStopDepo.Properties.Caption = "Depo";
		this.TopluBakimTalepEvragiGirisiTabStopDepo.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopDepo.TabIndex = 535;
		this.TopluBakimTalepEvragiGirisiTabStopEkipKodu.Location = new System.Drawing.Point(349, 426);
		this.TopluBakimTalepEvragiGirisiTabStopEkipKodu.Name = "TopluBakimTalepEvragiGirisiTabStopEkipKodu";
		this.TopluBakimTalepEvragiGirisiTabStopEkipKodu.Properties.Caption = "Ekip kodu";
		this.TopluBakimTalepEvragiGirisiTabStopEkipKodu.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopEkipKodu.TabIndex = 534;
		this.TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli.Location = new System.Drawing.Point(349, 401);
		this.TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli.Name = "TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli";
		this.TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli.Properties.Caption = "Bilgilendirme şekli";
		this.TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli.TabIndex = 533;
		this.TopluBakimTalepEvragiGirisiTabStopTeslimSekli.Location = new System.Drawing.Point(349, 376);
		this.TopluBakimTalepEvragiGirisiTabStopTeslimSekli.Name = "TopluBakimTalepEvragiGirisiTabStopTeslimSekli";
		this.TopluBakimTalepEvragiGirisiTabStopTeslimSekli.Properties.Caption = "Teslim şekli";
		this.TopluBakimTalepEvragiGirisiTabStopTeslimSekli.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopTeslimSekli.TabIndex = 532;
		this.TopluBakimTalepEvragiGirisiTabStopServisYeri.Location = new System.Drawing.Point(349, 351);
		this.TopluBakimTalepEvragiGirisiTabStopServisYeri.Name = "TopluBakimTalepEvragiGirisiTabStopServisYeri";
		this.TopluBakimTalepEvragiGirisiTabStopServisYeri.Properties.Caption = "Servis yeri";
		this.TopluBakimTalepEvragiGirisiTabStopServisYeri.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopServisYeri.TabIndex = 531;
		this.TopluBakimTalepEvragiGirisiTabStopServisTuru.Location = new System.Drawing.Point(349, 326);
		this.TopluBakimTalepEvragiGirisiTabStopServisTuru.Name = "TopluBakimTalepEvragiGirisiTabStopServisTuru";
		this.TopluBakimTalepEvragiGirisiTabStopServisTuru.Properties.Caption = "Servis türü";
		this.TopluBakimTalepEvragiGirisiTabStopServisTuru.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopServisTuru.TabIndex = 530;
		this.TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo.Location = new System.Drawing.Point(349, 301);
		this.TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo.Name = "TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo";
		this.TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo.Properties.Caption = "İrsaliye no";
		this.TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo.TabIndex = 529;
		this.TopluBakimTalepEvragiGirisiTabStopKargoNo.Location = new System.Drawing.Point(349, 276);
		this.TopluBakimTalepEvragiGirisiTabStopKargoNo.Name = "TopluBakimTalepEvragiGirisiTabStopKargoNo";
		this.TopluBakimTalepEvragiGirisiTabStopKargoNo.Properties.Caption = "Kargo no";
		this.TopluBakimTalepEvragiGirisiTabStopKargoNo.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopKargoNo.TabIndex = 528;
		this.TopluBakimTalepEvragiGirisiTabStopBayi.Location = new System.Drawing.Point(349, 251);
		this.TopluBakimTalepEvragiGirisiTabStopBayi.Name = "TopluBakimTalepEvragiGirisiTabStopBayi";
		this.TopluBakimTalepEvragiGirisiTabStopBayi.Properties.Caption = "Bayi";
		this.TopluBakimTalepEvragiGirisiTabStopBayi.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopBayi.TabIndex = 527;
		this.TopluBakimTalepEvragiGirisiTabStopKargo.Location = new System.Drawing.Point(349, 226);
		this.TopluBakimTalepEvragiGirisiTabStopKargo.Name = "TopluBakimTalepEvragiGirisiTabStopKargo";
		this.TopluBakimTalepEvragiGirisiTabStopKargo.Properties.Caption = "Kargo";
		this.TopluBakimTalepEvragiGirisiTabStopKargo.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopKargo.TabIndex = 526;
		this.TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli.Location = new System.Drawing.Point(349, 201);
		this.TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli.Name = "TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli";
		this.TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli.Properties.Caption = "Talep geliş şekli";
		this.TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli.TabIndex = 525;
		this.TopluBakimTalepEvragiGirisiTabStopBelgeTarihi.Location = new System.Drawing.Point(349, 178);
		this.TopluBakimTalepEvragiGirisiTabStopBelgeTarihi.Name = "TopluBakimTalepEvragiGirisiTabStopBelgeTarihi";
		this.TopluBakimTalepEvragiGirisiTabStopBelgeTarihi.Properties.Caption = "Belge tarihi";
		this.TopluBakimTalepEvragiGirisiTabStopBelgeTarihi.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopBelgeTarihi.TabIndex = 524;
		this.TopluBakimTalepEvragiGirisiTabStopBelgeNo.Location = new System.Drawing.Point(349, 153);
		this.TopluBakimTalepEvragiGirisiTabStopBelgeNo.Name = "TopluBakimTalepEvragiGirisiTabStopBelgeNo";
		this.TopluBakimTalepEvragiGirisiTabStopBelgeNo.Properties.Caption = "Belge no";
		this.TopluBakimTalepEvragiGirisiTabStopBelgeNo.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopBelgeNo.TabIndex = 523;
		this.TopluBakimTalepEvragiGirisiTabStopEvrakTarih.Location = new System.Drawing.Point(349, 128);
		this.TopluBakimTalepEvragiGirisiTabStopEvrakTarih.Name = "TopluBakimTalepEvragiGirisiTabStopEvrakTarih";
		this.TopluBakimTalepEvragiGirisiTabStopEvrakTarih.Properties.Caption = "Evrak tarihi";
		this.TopluBakimTalepEvragiGirisiTabStopEvrakTarih.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopEvrakTarih.TabIndex = 522;
		this.TopluBakimTalepEvragiGirisiTabStopEvrakSeri.Location = new System.Drawing.Point(349, 103);
		this.TopluBakimTalepEvragiGirisiTabStopEvrakSeri.Name = "TopluBakimTalepEvragiGirisiTabStopEvrakSeri";
		this.TopluBakimTalepEvragiGirisiTabStopEvrakSeri.Properties.Caption = "Evrak seri";
		this.TopluBakimTalepEvragiGirisiTabStopEvrakSeri.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopEvrakSeri.TabIndex = 521;
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciKodu.Location = new System.Drawing.Point(349, 53);
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciKodu.Name = "TopluBakimTalepEvragiGirisiTabStopTuketiciKodu";
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciKodu.Properties.Caption = "Tüketici kodu";
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciKodu.Size = new System.Drawing.Size(110, 19);
		this.TopluBakimTalepEvragiGirisiTabStopTuketiciKodu.TabIndex = 520;
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(349, 28);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(267, 19);
		this.labelControl3.TabIndex = 519;
		this.labelControl3.Text = "Tab Stop";
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(26, 28);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(267, 19);
		this.labelControl2.TabIndex = 518;
		this.labelControl2.Text = "Varsayılan değerler";
		this.TopluBakimTalepEvragiGirisiCari._mikrouygulamabilgileri = null;
		this.TopluBakimTalepEvragiGirisiCari.CodeEmptyMessage = "Bayi seçiniz";
		this.TopluBakimTalepEvragiGirisiCari.EnabledCode = true;
		this.TopluBakimTalepEvragiGirisiCari.EnabledName = false;
		this.TopluBakimTalepEvragiGirisiCari.FontCode = new System.Drawing.Font("Tahoma", 8.25f);
		this.TopluBakimTalepEvragiGirisiCari.FontCodeLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.TopluBakimTalepEvragiGirisiCari.FontName = new System.Drawing.Font("Tahoma", 8.25f);
		this.TopluBakimTalepEvragiGirisiCari.FontNameLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.TopluBakimTalepEvragiGirisiCari.Location = new System.Drawing.Point(82, 196);
		this.TopluBakimTalepEvragiGirisiCari.LocationCode = new System.Drawing.Point(40, 0);
		this.TopluBakimTalepEvragiGirisiCari.LocationCodeLabel = new System.Drawing.Point(3, 3);
		this.TopluBakimTalepEvragiGirisiCari.LocationName = new System.Drawing.Point(253, 0);
		this.TopluBakimTalepEvragiGirisiCari.LocationNameLabel = new System.Drawing.Point(183, 3);
		this.TopluBakimTalepEvragiGirisiCari.Name = "TopluBakimTalepEvragiGirisiCari";
		this.TopluBakimTalepEvragiGirisiCari.NameEmptyMessage = "Bayi seçiniz";
		this.TopluBakimTalepEvragiGirisiCari.SelectedItem = null;
		this.TopluBakimTalepEvragiGirisiCari.Size = new System.Drawing.Size(185, 20);
		this.TopluBakimTalepEvragiGirisiCari.SizeCode = new System.Drawing.Size(140, 20);
		this.TopluBakimTalepEvragiGirisiCari.SizeName = new System.Drawing.Size(172, 20);
		this.TopluBakimTalepEvragiGirisiCari.TabIndex = 15;
		this.TopluBakimTalepEvragiGirisiCari.TextCodeLabel = "Bayi :";
		this.TopluBakimTalepEvragiGirisiCari.TextNameLabel = "Cari ünvan :";
		this.TopluBakimTalepEvragiGirisiCari.VisibleCode = true;
		this.TopluBakimTalepEvragiGirisiCari.VisibleCodeLabel = true;
		this.TopluBakimTalepEvragiGirisiCari.VisibleName = false;
		this.TopluBakimTalepEvragiGirisiCari.VisibleNameLabel = false;
		this.TopluBakimTalepEvragiGirisiKargo._mikrouygulamabilgileri = null;
		this.TopluBakimTalepEvragiGirisiKargo.CodeEmptyMessage = "Kargo seçiniz";
		this.TopluBakimTalepEvragiGirisiKargo.EnabledCode = true;
		this.TopluBakimTalepEvragiGirisiKargo.FontCode = new System.Drawing.Font("Tahoma", 8.25f);
		this.TopluBakimTalepEvragiGirisiKargo.FontCodeLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.TopluBakimTalepEvragiGirisiKargo.Location = new System.Drawing.Point(73, 170);
		this.TopluBakimTalepEvragiGirisiKargo.LocationCode = new System.Drawing.Point(50, 0);
		this.TopluBakimTalepEvragiGirisiKargo.LocationCodeLabel = new System.Drawing.Point(3, 3);
		this.TopluBakimTalepEvragiGirisiKargo.Name = "TopluBakimTalepEvragiGirisiKargo";
		this.TopluBakimTalepEvragiGirisiKargo.SelectedItem = null;
		this.TopluBakimTalepEvragiGirisiKargo.Size = new System.Drawing.Size(181, 20);
		this.TopluBakimTalepEvragiGirisiKargo.SizeCode = new System.Drawing.Size(128, 20);
		this.TopluBakimTalepEvragiGirisiKargo.TabIndex = 14;
		this.TopluBakimTalepEvragiGirisiKargo.TextCodeLabel = "Kargo :";
		this.TopluBakimTalepEvragiGirisiKargo.VisibleCode = true;
		this.TopluBakimTalepEvragiGirisiKargo.VisibleCodeLabel = true;
		this.TopluBakimTalepEvragiGirisiDepo._mikrouygulamabilgileri = null;
		this.TopluBakimTalepEvragiGirisiDepo.CodeEmptyMessage = "Depo seçiniz";
		this.TopluBakimTalepEvragiGirisiDepo.EnabledCode = true;
		this.TopluBakimTalepEvragiGirisiDepo.FontCode = new System.Drawing.Font("Tahoma", 8.25f);
		this.TopluBakimTalepEvragiGirisiDepo.FontCodeLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.TopluBakimTalepEvragiGirisiDepo.Location = new System.Drawing.Point(76, 302);
		this.TopluBakimTalepEvragiGirisiDepo.LocationCode = new System.Drawing.Point(48, 0);
		this.TopluBakimTalepEvragiGirisiDepo.LocationCodeLabel = new System.Drawing.Point(3, 3);
		this.TopluBakimTalepEvragiGirisiDepo.Name = "TopluBakimTalepEvragiGirisiDepo";
		this.TopluBakimTalepEvragiGirisiDepo.SelectedItem = null;
		this.TopluBakimTalepEvragiGirisiDepo.Size = new System.Drawing.Size(198, 20);
		this.TopluBakimTalepEvragiGirisiDepo.SizeCode = new System.Drawing.Size(150, 20);
		this.TopluBakimTalepEvragiGirisiDepo.TabIndex = 19;
		this.TopluBakimTalepEvragiGirisiDepo.TextCodeLabel = "Depo :";
		this.TopluBakimTalepEvragiGirisiDepo.VisibleCode = true;
		this.TopluBakimTalepEvragiGirisiDepo.VisibleCodeLabel = true;
		this.TopluBakimTalepEvragiGirisiEkipKodu._mikrouygulamabilgileri = null;
		this.TopluBakimTalepEvragiGirisiEkipKodu.CodeEmptyMessage = "Ekip seçiniz";
		this.TopluBakimTalepEvragiGirisiEkipKodu.EnabledCode = true;
		this.TopluBakimTalepEvragiGirisiEkipKodu.FontCode = new System.Drawing.Font("Tahoma", 8.25f);
		this.TopluBakimTalepEvragiGirisiEkipKodu.FontCodeLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.TopluBakimTalepEvragiGirisiEkipKodu.Location = new System.Drawing.Point(53, 276);
		this.TopluBakimTalepEvragiGirisiEkipKodu.LocationCode = new System.Drawing.Point(70, 0);
		this.TopluBakimTalepEvragiGirisiEkipKodu.LocationCodeLabel = new System.Drawing.Point(3, 3);
		this.TopluBakimTalepEvragiGirisiEkipKodu.Name = "TopluBakimTalepEvragiGirisiEkipKodu";
		this.TopluBakimTalepEvragiGirisiEkipKodu.SelectedItem = null;
		this.TopluBakimTalepEvragiGirisiEkipKodu.Size = new System.Drawing.Size(231, 20);
		this.TopluBakimTalepEvragiGirisiEkipKodu.SizeCode = new System.Drawing.Size(110, 20);
		this.TopluBakimTalepEvragiGirisiEkipKodu.TabIndex = 18;
		this.TopluBakimTalepEvragiGirisiEkipKodu.TextCodeLabel = "Ekip kodu :";
		this.TopluBakimTalepEvragiGirisiEkipKodu.VisibleCode = true;
		this.TopluBakimTalepEvragiGirisiEkipKodu.VisibleCodeLabel = true;
		this.TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.FormattingEnabled = true;
		this.TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.Location = new System.Drawing.Point(124, 116);
		this.TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.Name = "TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli";
		this.TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.Size = new System.Drawing.Size(160, 21);
		this.TopluBakimTalepEvragiGirisiMusteriBilgilendirmeSekli.TabIndex = 12;
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(23, 119);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(95, 13);
		this.label14.TabIndex = 517;
		this.label14.Text = "Bilgilendirme şekli :";
		this.TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.FormattingEnabled = true;
		this.TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.Location = new System.Drawing.Point(124, 89);
		this.TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.Name = "TopluBakimTalepEvragiGirisiTeslimEdilmeSekli";
		this.TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.Size = new System.Drawing.Size(160, 21);
		this.TopluBakimTalepEvragiGirisiTeslimEdilmeSekli.TabIndex = 11;
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(51, 92);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(66, 13);
		this.label13.TabIndex = 516;
		this.label13.Text = "Teslim şekli :";
		this.TopluBakimTalepEvragiGirisiServisYeri.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.TopluBakimTalepEvragiGirisiServisYeri.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.TopluBakimTalepEvragiGirisiServisYeri.FormattingEnabled = true;
		this.TopluBakimTalepEvragiGirisiServisYeri.Location = new System.Drawing.Point(124, 249);
		this.TopluBakimTalepEvragiGirisiServisYeri.Name = "TopluBakimTalepEvragiGirisiServisYeri";
		this.TopluBakimTalepEvragiGirisiServisYeri.Size = new System.Drawing.Size(160, 21);
		this.TopluBakimTalepEvragiGirisiServisYeri.TabIndex = 17;
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(54, 252);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(64, 13);
		this.label12.TabIndex = 515;
		this.label12.Text = "Servis yeri :";
		this.TopluBakimTalepEvragiGirisiServisTuru.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.TopluBakimTalepEvragiGirisiServisTuru.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.TopluBakimTalepEvragiGirisiServisTuru.FormattingEnabled = true;
		this.TopluBakimTalepEvragiGirisiServisTuru.Location = new System.Drawing.Point(124, 222);
		this.TopluBakimTalepEvragiGirisiServisTuru.Name = "TopluBakimTalepEvragiGirisiServisTuru";
		this.TopluBakimTalepEvragiGirisiServisTuru.Size = new System.Drawing.Size(160, 21);
		this.TopluBakimTalepEvragiGirisiServisTuru.TabIndex = 16;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(50, 225);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(66, 13);
		this.label11.TabIndex = 514;
		this.label11.Text = "Servis türü :";
		this.TopluBakimTalepEvragiGirisiTalepGelisSekli.BackColor = System.Drawing.SystemColors.Window;
		this.TopluBakimTalepEvragiGirisiTalepGelisSekli.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.TopluBakimTalepEvragiGirisiTalepGelisSekli.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.TopluBakimTalepEvragiGirisiTalepGelisSekli.FormattingEnabled = true;
		this.TopluBakimTalepEvragiGirisiTalepGelisSekli.Location = new System.Drawing.Point(124, 143);
		this.TopluBakimTalepEvragiGirisiTalepGelisSekli.Name = "TopluBakimTalepEvragiGirisiTalepGelisSekli";
		this.TopluBakimTalepEvragiGirisiTalepGelisSekli.Size = new System.Drawing.Size(160, 21);
		this.TopluBakimTalepEvragiGirisiTalepGelisSekli.TabIndex = 13;
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(30, 146);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(87, 13);
		this.label7.TabIndex = 508;
		this.label7.Text = "Talep geliş şekli :";
		this.TopluBakimTalepEvragiGirisiEvrakSeri.EnterMoveNextControl = true;
		this.TopluBakimTalepEvragiGirisiEvrakSeri.Location = new System.Drawing.Point(124, 63);
		this.TopluBakimTalepEvragiGirisiEvrakSeri.Name = "TopluBakimTalepEvragiGirisiEvrakSeri";
		this.TopluBakimTalepEvragiGirisiEvrakSeri.Properties.MaxLength = 6;
		this.TopluBakimTalepEvragiGirisiEvrakSeri.Size = new System.Drawing.Size(86, 20);
		this.TopluBakimTalepEvragiGirisiEvrakSeri.TabIndex = 10;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(57, 66);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(61, 13);
		this.label1.TabIndex = 502;
		this.label1.Text = "Evrak seri :";
		this.lb_kullanicilar.Location = new System.Drawing.Point(12, 35);
		this.lb_kullanicilar.Name = "lb_kullanicilar";
		this.lb_kullanicilar.Size = new System.Drawing.Size(120, 221);
		this.lb_kullanicilar.TabIndex = 5;
		this.lb_kullanicilar.SelectedValueChanged += new System.EventHandler(lb_kullanicilar_SelectedValueChanged);
		this.sb_ayarlari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlari_kaydet.Appearance.Options.UseFont = true;
		this.sb_ayarlari_kaydet.Enabled = false;
		this.sb_ayarlari_kaydet.Location = new System.Drawing.Point(790, 644);
		this.sb_ayarlari_kaydet.Name = "sb_ayarlari_kaydet";
		this.sb_ayarlari_kaydet.Size = new System.Drawing.Size(132, 23);
		this.sb_ayarlari_kaydet.TabIndex = 8;
		this.sb_ayarlari_kaydet.Text = "Ayarları kaydet";
		this.sb_ayarlari_kaydet.Click += new System.EventHandler(sb_ayarlari_kaydet_Click);
		this.TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu.EnterMoveNextControl = true;
		this.TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu.Location = new System.Drawing.Point(161, 341);
		this.TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu.Name = "TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu";
		this.TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu.Properties.MaxLength = 25;
		this.TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu.Size = new System.Drawing.Size(86, 20);
		this.TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu.TabIndex = 537;
		this.TopluBakimTalepEvragiGirisiVarsayilanSure.EditValue = new decimal(new int[4]);
		this.TopluBakimTalepEvragiGirisiVarsayilanSure.Location = new System.Drawing.Point(161, 366);
		this.TopluBakimTalepEvragiGirisiVarsayilanSure.Name = "TopluBakimTalepEvragiGirisiVarsayilanSure";
		this.TopluBakimTalepEvragiGirisiVarsayilanSure.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.TopluBakimTalepEvragiGirisiVarsayilanSure.Properties.IsFloatValue = false;
		this.TopluBakimTalepEvragiGirisiVarsayilanSure.Properties.Mask.EditMask = "N00";
		this.TopluBakimTalepEvragiGirisiVarsayilanSure.Size = new System.Drawing.Size(86, 20);
		this.TopluBakimTalepEvragiGirisiVarsayilanSure.TabIndex = 540;
		this.labelControl29.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl29.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl29.Location = new System.Drawing.Point(26, 369);
		this.labelControl29.Name = "labelControl29";
		this.labelControl29.Size = new System.Drawing.Size(129, 13);
		this.labelControl29.TabIndex = 539;
		this.labelControl29.Text = "Varsayılan süre (dakika) :";
		this.labelControl36.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl36.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl36.Location = new System.Drawing.Point(26, 395);
		this.labelControl36.Name = "labelControl36";
		this.labelControl36.Size = new System.Drawing.Size(129, 13);
		this.labelControl36.TabIndex = 541;
		this.labelControl36.Text = "Varsayılan miktar :";
		this.labelControl41.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl41.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl41.Location = new System.Drawing.Point(26, 344);
		this.labelControl41.Name = "labelControl41";
		this.labelControl41.Size = new System.Drawing.Size(129, 13);
		this.labelControl41.TabIndex = 542;
		this.labelControl41.Text = "Varsayılan hizmet kodu :";
		this.TopluBakimTalepEvragiGirisiVarsayilanMiktar.EditValue = new decimal(new int[4]);
		this.TopluBakimTalepEvragiGirisiVarsayilanMiktar.Location = new System.Drawing.Point(161, 392);
		this.TopluBakimTalepEvragiGirisiVarsayilanMiktar.Name = "TopluBakimTalepEvragiGirisiVarsayilanMiktar";
		this.TopluBakimTalepEvragiGirisiVarsayilanMiktar.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.TopluBakimTalepEvragiGirisiVarsayilanMiktar.Properties.IsFloatValue = false;
		this.TopluBakimTalepEvragiGirisiVarsayilanMiktar.Properties.Mask.EditMask = "N00";
		this.TopluBakimTalepEvragiGirisiVarsayilanMiktar.Size = new System.Drawing.Size(86, 20);
		this.TopluBakimTalepEvragiGirisiVarsayilanMiktar.TabIndex = 543;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(934, 673);
		base.Controls.Add(this.sb_ayarlari_kaydet);
		base.Controls.Add(this.lb_kullanicilar);
		base.Controls.Add(this.tc_parametrevehaklar);
		base.Name = "KullaniciDuzenleme";
		this.Text = "Kullanıcı Düzenle";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		((System.ComponentModel.ISupportInitialize)this.tc_parametrevehaklar).EndInit();
		this.tc_parametrevehaklar.ResumeLayout(false);
		this.xtraTabPage1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl2).EndInit();
		this.xtraTabControl2.ResumeLayout(false);
		this.xtraTabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		this.xtraTabPage2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiIadeDepoDegistirebilir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiRetDepoDegistirebilir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiOnayDepoDegistirebilir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiGunSayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiKayitHikayesiGorebilir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DepoKaliteKontrolYonetimiIslemGirisiYapabilir.Properties).EndInit();
		this.xtraTabPage4.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.BankaAktarimiParametreleriDuzenleyebilir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BankaAktarimiYapabilir.Properties).EndInit();
		this.xtraTabPage5.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl1).EndInit();
		this.xtraTabControl1.ResumeLayout(false);
		this.xtraTabPage6.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSira.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_KacDakikadaBir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_HedefDepoNo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_EvrakSeri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_StokSorgusu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlDBName.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlSifre.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlKullaniciAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BaskaFirmaninStokMiktarlariniZamanliAktarim_SqlServer.Properties).EndInit();
		this.xtraTabPage7.ResumeLayout(false);
		this.xtraTabPage7.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopTuketiciAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopDepo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopEkipKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopBilgilendirmeSekli.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopTeslimSekli.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopServisYeri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopServisTuru.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopIrsaliyeNo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopKargoNo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopBayi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopKargo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopTalepGelisSekli.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopBelgeTarihi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopBelgeNo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopEvrakTarih.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopEvrakSeri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiTabStopTuketiciKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiEvrakSeri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiVarsayilanHizmetKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiVarsayilanSure.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TopluBakimTalepEvragiGirisiVarsayilanMiktar.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
