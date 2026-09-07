using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using Fora.App.Mikro;
using Fora.App.Mikro.ForaAndroid;
using Fora.App.Mikro.Kurulum;
using Fora.App.Win.Mikro.Aktarimlar.BankaAktarimi;
using Fora.App.Win.Mikro.Aktarimlar.ComarchEdi;
using Fora.App.Win.Mikro.Ayarlar;
using Fora.App.Win.Mikro.B2B;
using Fora.App.Win.Mikro.CariHesaplar;
using Fora.App.Win.Mikro.Cekilis;
using Fora.App.Win.Mikro.Debug;
using Fora.App.Win.Mikro.DepolarArasiSiparis.DepoBazli;
using Fora.App.Win.Mikro.DepolarArasiSiparis.StokBazli;
using Fora.App.Win.Mikro.ForaAndroid;
using Fora.App.Win.Mikro.KaliteKontrolYonetimi;
using Fora.App.Win.Mikro.PerformansTest;
using Fora.App.Win.Mikro.TeknikServisYonetimi;
using Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli;
using Fora.App.Win.Mikro.TopluEvrakGirisi.TahsilatEvrakSatirBazli;
using Fora.App.Win.Mikro.ZamanlanmisGorevler;
using Fora.Mikro;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.CariHesaplar.CariEkstresi;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Enumler;
using Fora.Mikro.Win.Form.DevEx;
using Fora.Mikro.Win.Form.DevEx.F10;
using Fora.Mikro.Win.Form.DevEx.Raporlar;

namespace Fora.App.Win.Mikro;

public class AnaMenu : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public DataSet _lookuptablolar;

	private string _uygulamaid;

	private IContainer components;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem cikisToolStripMenuItem;

	private TreeList tl_uygulamalar;

	private TreeListColumn colName;

	private TreeListColumn uygulamaid;

	private LabelControl labelControl1;

	private ToolStripMenuItem sqlBaglantiBilgileriToolStripMenuItem;

	private ToolStripMenuItem yardımToolStripMenuItem;

	private ToolStripMenuItem hakkindaToolStripMenuItem;

	public AnaMenu(MikroUygulamaBilgileri mikrouygulamabilgileri, string UygulamaID)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_uygulamaid = UygulamaID;
		InitializeComponent();
		Text = Text + " - " + _mikrouygulamabilgileri.veritabani.DB_kod;
		MenuOlustur();
	}

	private void AnaMenu_Load(object sender, EventArgs e)
	{
		if (_uygulamaid != "00000000")
		{
			UygulamaAc(_uygulamaid, AsDialog: true);
			Close();
		}
	}

	private void MenuOlustur()
	{
		for (int i = 1; i < _mikrouygulamabilgileri.anamenu.Count; i++)
		{
			ForaAppWinAnaMenu foraAppWinAnaMenu = _mikrouygulamabilgileri.anamenu[i];
			TreeListNode treeListNode = null;
			foreach (TreeListNode node in tl_uygulamalar.Nodes)
			{
				if (node.GetValue("uygulamaid").ToString() == foraAppWinAnaMenu.ParentKod)
				{
					treeListNode = node;
					break;
				}
				foreach (TreeListNode node2 in node.Nodes)
				{
					if (node2.GetValue("uygulamaid").ToString() == foraAppWinAnaMenu.ParentKod)
					{
						treeListNode = node2;
						break;
					}
					foreach (TreeListNode node3 in node2.Nodes)
					{
						if (node3.GetValue("uygulamaid").ToString() == foraAppWinAnaMenu.ParentKod)
						{
							treeListNode = node3;
							break;
						}
						foreach (TreeListNode node4 in node3.Nodes)
						{
							if (node4.GetValue("uygulamaid").ToString() == foraAppWinAnaMenu.ParentKod)
							{
								treeListNode = node4;
								break;
							}
							if (treeListNode != null)
							{
								break;
							}
						}
						if (treeListNode != null)
						{
							break;
						}
					}
					if (treeListNode != null)
					{
						break;
					}
				}
				if (treeListNode != null)
				{
					break;
				}
			}
			TreeListNode treeListNode6 = tl_uygulamalar.AppendNode(null, treeListNode);
			string text = foraAppWinAnaMenu.Adi;
			if (foraAppWinAnaMenu.Tipi != enum_ForaAppWinAnaMenuTipi.Menu)
			{
				text = text + " (" + foraAppWinAnaMenu.Kod + ")";
			}
			treeListNode6.SetValue("name", text);
			treeListNode6.SetValue("uygulamaid", foraAppWinAnaMenu.Kod);
		}
	}

	private void cikisToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Dispose();
	}

	private void tl_uygulamalar_DoubleClick(object sender, EventArgs e)
	{
		UygulamaAc(tl_uygulamalar.FocusedNode.GetValue("uygulamaid").ToString(), AsDialog: false);
	}

	private void tl_uygulamalar_KeyDown(object sender, KeyEventArgs e)
	{
		TreeListNode focusedNode = tl_uygulamalar.FocusedNode;
		if (e.KeyData == Keys.Return)
		{
			UygulamaAc(tl_uygulamalar.FocusedNode.GetValue("uygulamaid").ToString(), AsDialog: false);
			e.Handled = true;
		}
		if (focusedNode.HasChildren)
		{
			if (e.KeyData == Keys.Left && focusedNode.Expanded)
			{
				focusedNode.Expanded = false;
				e.Handled = true;
			}
			else if (e.KeyData == Keys.Right && !focusedNode.Expanded)
			{
				focusedNode.Expanded = true;
				e.Handled = true;
			}
		}
	}

	public void UygulamaAc(string UygulamaID, bool AsDialog)
	{
		switch (UygulamaID)
		{
		case "20050000":
		{
			string text4 = "cari_ekleme";
			if (AppBase.MikroVersiyonu >= 16)
			{
				text4 = text4 + "_v" + AppBase.MikroVersiyonu;
			}
			if (LisansKontrol(text4))
			{
				Cari cari2 = new Cari();
				cari2.CariAdresleri = new List<CariAdres>();
				CariAdres item = new CariAdres();
				cari2.CariAdresleri.Add(item);
				YeniCariEkle yeniCariEkle = new YeniCariEkle(_mikrouygulamabilgileri, cari2);
				if (AsDialog)
				{
					yeniCariEkle.ShowDialog();
				}
				else
				{
					yeniCariEkle.Show();
				}
			}
			break;
		}
		case "25050000":
		{
			SatirBazliGenelEvrakGirisi satirBazliGenelEvrakGirisi = new SatirBazliGenelEvrakGirisi(_mikrouygulamabilgileri, "satirbazligenelevrakgirisi", "Genel evrak girişi (Satır bazlı)");
			if (AsDialog)
			{
				satirBazliGenelEvrakGirisi.ShowDialog();
			}
			else
			{
				satirBazliGenelEvrakGirisi.Show();
			}
			break;
		}
		case "25060000":
		{
			string text2 = "genel_evrak_aktarimi";
			if (AppBase.MikroVersiyonu >= 16)
			{
				text2 = text2 + "_v" + AppBase.MikroVersiyonu;
			}
			if (LisansKontrol(text2))
			{
				SatirBazliTahsilatEvrakGirisi satirBazliTahsilatEvrakGirisi = new SatirBazliTahsilatEvrakGirisi(_mikrouygulamabilgileri, "satirbazlitahsilatevrakgirisi", "Tahsilat evrak girişi (Satır bazlı)");
				if (AsDialog)
				{
					satirBazliTahsilatEvrakGirisi.ShowDialog();
				}
				else
				{
					satirBazliTahsilatEvrakGirisi.Show();
				}
			}
			break;
		}
		case "35201010":
		{
			string text7 = "depolar_arasi_siparis";
			if (AppBase.MikroVersiyonu >= 16)
			{
				text7 = text7 + "_v" + AppBase.MikroVersiyonu;
			}
			if (LisansKontrol(text7))
			{
				DepolarArasiAcikSiparisAnaliziStokBazli depolarArasiAcikSiparisAnaliziStokBazli = new DepolarArasiAcikSiparisAnaliziStokBazli(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					depolarArasiAcikSiparisAnaliziStokBazli.ShowDialog();
				}
				else
				{
					depolarArasiAcikSiparisAnaliziStokBazli.Show();
				}
			}
			break;
		}
		case "35201020":
		{
			string text9 = "depolar_arasi_siparis";
			if (AppBase.MikroVersiyonu >= 16)
			{
				text9 = text9 + "_v" + AppBase.MikroVersiyonu;
			}
			if (LisansKontrol(text9))
			{
				DepolarArasiAcikSiparisAnaliziDepoBazli depolarArasiAcikSiparisAnaliziDepoBazli = new DepolarArasiAcikSiparisAnaliziDepoBazli(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					depolarArasiAcikSiparisAnaliziDepoBazli.ShowDialog();
				}
				else
				{
					depolarArasiAcikSiparisAnaliziDepoBazli.Show();
				}
			}
			break;
		}
		case "25100000":
		{
			string text3 = "banka_aktarimi";
			if (AppBase.MikroVersiyonu >= 16)
			{
				text3 = text3 + "_v" + AppBase.MikroVersiyonu;
			}
			if (!LisansKontrol(text3))
			{
				break;
			}
			if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BankaAktarimiYapabilir")._GetBoolean)
			{
				if (AppBase.MikroVersiyonu >= 16)
				{
					F10_Banka_Secimi_V16 f10_Banka_Secimi_V = new F10_Banka_Secimi_V16();
					f10_Banka_Secimi_V.Setup(_mikrouygulamabilgileri, "BANKALAR_CHOOSE_3", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
					if (f10_Banka_Secimi_V.ShowDialog() == DialogResult.OK)
					{
						Aktarim_TxtCsv aktarim_TxtCsv = new Aktarim_TxtCsv(_mikrouygulamabilgileri, _lookuptablolar, f10_Banka_Secimi_V._selecteditems[0]);
						if (AsDialog)
						{
							aktarim_TxtCsv.ShowDialog(this);
						}
						else
						{
							aktarim_TxtCsv.Show(this);
						}
					}
					break;
				}
				F10_Banka_Secimi f10_Banka_Secimi = new F10_Banka_Secimi();
				f10_Banka_Secimi.Setup(_mikrouygulamabilgileri, "BANKALAR_CHOOSE_3", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Banka_Secimi.ShowDialog() == DialogResult.OK)
				{
					Aktarim_TxtCsv aktarim_TxtCsv2 = new Aktarim_TxtCsv(_mikrouygulamabilgileri, _lookuptablolar, f10_Banka_Secimi._selecteditems[0]);
					if (AsDialog)
					{
						aktarim_TxtCsv2.ShowDialog(this);
					}
					else
					{
						aktarim_TxtCsv2.Show(this);
					}
				}
			}
			else
			{
				MessageBox.Show("Giriş yetkiniz bulunmamaktadır.");
			}
			break;
		}
		case "80600000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				AktarimBankaKurulum aktarimBankaKurulum = new AktarimBankaKurulum(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					aktarimBankaKurulum.ShowDialog();
				}
				else
				{
					aktarimBankaKurulum.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "45100000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				ForaAndroidKullaniciDuzenleme foraAndroidKullaniciDuzenleme = new ForaAndroidKullaniciDuzenleme(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					foraAndroidKullaniciDuzenleme.ShowDialog();
				}
				else
				{
					foraAndroidKullaniciDuzenleme.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "45051000":
		{
			TemsilciRaporuGiris temsilciRaporuGiris = new TemsilciRaporuGiris(_mikrouygulamabilgileri);
			if (AsDialog)
			{
				temsilciRaporuGiris.ShowDialog();
			}
			else
			{
				temsilciRaporuGiris.Show();
			}
			break;
		}
		case "45061000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				MobilRaporStokSatisDuzenleme mobilRaporStokSatisDuzenleme = new MobilRaporStokSatisDuzenleme(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					mobilRaporStokSatisDuzenleme.ShowDialog();
				}
				else
				{
					mobilRaporStokSatisDuzenleme.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "45061100":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				MobilRaporStokSiparisDuzenleme mobilRaporStokSiparisDuzenleme = new MobilRaporStokSiparisDuzenleme(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					mobilRaporStokSiparisDuzenleme.ShowDialog();
				}
				else
				{
					mobilRaporStokSiparisDuzenleme.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "45061200":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				MobilRaporYapilacakTahsilatlarDuzenleme mobilRaporYapilacakTahsilatlarDuzenleme = new MobilRaporYapilacakTahsilatlarDuzenleme(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					mobilRaporYapilacakTahsilatlarDuzenleme.ShowDialog();
				}
				else
				{
					mobilRaporYapilacakTahsilatlarDuzenleme.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "45061300":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				MobilRaporStokEnvanterDuzenleme mobilRaporStokEnvanterDuzenleme = new MobilRaporStokEnvanterDuzenleme(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					mobilRaporStokEnvanterDuzenleme.ShowDialog();
				}
				else
				{
					mobilRaporStokEnvanterDuzenleme.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "45052000":
		{
			if (_mikrouygulamabilgileri.KullaniciAdi == "deryafuar")
			{
				MessageBox.Show("Çıkış yapmak için CTRL+ALT+SHIFT+D tuşlarına basınız.");
			}
			SatisRaporuGiris satisRaporuGiris = new SatisRaporuGiris(_mikrouygulamabilgileri);
			if (AsDialog)
			{
				satisRaporuGiris.ShowDialog();
			}
			else
			{
				satisRaporuGiris.Show();
			}
			break;
		}
		case "45053000":
		{
			ZiyaretRaporuGiris ziyaretRaporuGiris = new ZiyaretRaporuGiris(_mikrouygulamabilgileri);
			if (AsDialog)
			{
				ziyaretRaporuGiris.ShowDialog();
			}
			else
			{
				ziyaretRaporuGiris.Show();
			}
			break;
		}
		case "46050000":
		{
			string text8 = "teknik_servis_yonetimi";
			if (AppBase.MikroVersiyonu >= 16)
			{
				text8 = text8 + "_v" + AppBase.MikroVersiyonu;
			}
			if (LisansKontrol(text8))
			{
				TopluBakimTalepEvragiGirisi topluBakimTalepEvragiGirisi = new TopluBakimTalepEvragiGirisi(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					topluBakimTalepEvragiGirisi.ShowDialog();
				}
				else
				{
					topluBakimTalepEvragiGirisi.Show();
				}
			}
			break;
		}
		case "6006":
		{
			CekilisGiris cekilisGiris = new CekilisGiris(_mikrouygulamabilgileri);
			if (AsDialog)
			{
				cekilisGiris.ShowDialog();
			}
			else
			{
				cekilisGiris.Show();
			}
			break;
		}
		case "6007":
		{
			CekilisYapma cekilisYapma = new CekilisYapma(_mikrouygulamabilgileri);
			if (AsDialog)
			{
				cekilisYapma.ShowDialog();
			}
			else
			{
				cekilisYapma.Show();
			}
			break;
		}
		case "6008":
		{
			CekilisGosterim cekilisGosterim = new CekilisGosterim(_mikrouygulamabilgileri);
			if (AsDialog)
			{
				cekilisGosterim.ShowDialog();
			}
			else
			{
				cekilisGosterim.Show();
			}
			break;
		}
		case "65100000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				if (new GenelParametrelerForm(_mikrouygulamabilgileri).ShowDialog() == DialogResult.OK)
				{
					MessageBox.Show("Değişikliklerin aktif olması için lütfen programı kapatıp tekrar açınız.");
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "65200000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				YaziciAyarlariForm yaziciAyarlariForm = new YaziciAyarlariForm(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					yaziciAyarlariForm.ShowDialog();
				}
				else
				{
					yaziciAyarlariForm.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "65300000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				KullaniciDuzenleme kullaniciDuzenleme = new KullaniciDuzenleme(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					kullaniciDuzenleme.ShowDialog();
				}
				else
				{
					kullaniciDuzenleme.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "40050000":
		{
			string text = "kalite_kontrol_yonetimi";
			if (AppBase.MikroVersiyonu >= 16)
			{
				text = text + "_v" + AppBase.MikroVersiyonu;
			}
			if (!LisansKontrol(text))
			{
				break;
			}
			if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiIslemGirisiYapabilir")._GetBoolean)
			{
				KaliteKontrolKurulum();
				IslemGirisi islemGirisi = new IslemGirisi(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					islemGirisi.ShowDialog();
				}
				else
				{
					islemGirisi.Show();
				}
			}
			else
			{
				MessageBox.Show("Giriş yetkiniz bulunmamaktadır.");
			}
			break;
		}
		case "40100000":
		{
			string text6 = "kalite_kontrol_yonetimi";
			if (AppBase.MikroVersiyonu >= 16)
			{
				text6 = text6 + "_v" + AppBase.MikroVersiyonu;
			}
			if (!LisansKontrol(text6))
			{
				break;
			}
			if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiGorebilir")._GetBoolean)
			{
				KaliteKontrolKurulum();
				KayitHikayesi kayitHikayesi = new KayitHikayesi(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					kayitHikayesi.ShowDialog();
				}
				else
				{
					kayitHikayesi.Show();
				}
			}
			else
			{
				MessageBox.Show("Giriş yetkiniz bulunmamaktadır.");
			}
			break;
		}
		case "55100000":
		{
			string text5 = "zamanlanmis_gorevler";
			if (AppBase.MikroVersiyonu >= 16)
			{
				text5 = text5 + "_v" + AppBase.MikroVersiyonu;
			}
			if (!LisansKontrol(text5))
			{
				break;
			}
			if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("BaskaFirmaninStokMiktarlariniZamanliAktarim_GirisiYapabilir")._GetBoolean)
			{
				BaskaFirmaninStokMiktarlariniZamanliAktarim baskaFirmaninStokMiktarlariniZamanliAktarim = new BaskaFirmaninStokMiktarlariniZamanliAktarim(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					baskaFirmaninStokMiktarlariniZamanliAktarim.ShowDialog();
				}
				else
				{
					baskaFirmaninStokMiktarlariniZamanliAktarim.Show();
				}
			}
			else
			{
				MessageBox.Show("Giriş yetkiniz bulunmamaktadır.");
			}
			break;
		}
		case "60100000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				B2BPArametreleriForm b2BPArametreleriForm = new B2BPArametreleriForm(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					b2BPArametreleriForm.ShowDialog();
				}
				else
				{
					b2BPArametreleriForm.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "65400000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				GenelEvrakFormTasarimiForm genelEvrakFormTasarimiForm = new GenelEvrakFormTasarimiForm(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					genelEvrakFormTasarimiForm.ShowDialog();
				}
				else
				{
					genelEvrakFormTasarimiForm.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "80100000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				LisansYoneticisi lisansYoneticisi = new LisansYoneticisi();
				if (AsDialog)
				{
					lisansYoneticisi.ShowDialog();
				}
				else
				{
					lisansYoneticisi.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "80200000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				ForaMikroServisKurulumu foraMikroServisKurulumu = new ForaMikroServisKurulumu();
				if (AsDialog)
				{
					foraMikroServisKurulumu.ShowDialog();
				}
				else
				{
					foraMikroServisKurulumu.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "80300000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				if (AppBase.MikroVersiyonu >= 16)
				{
					ForaMikroVeritabaniKurulumu foraMikroVeritabaniKurulumu = new ForaMikroVeritabaniKurulumu(_mikrouygulamabilgileri);
					if (AsDialog)
					{
						foraMikroVeritabaniKurulumu.ShowDialog();
					}
					else
					{
						foraMikroVeritabaniKurulumu.Show();
					}
				}
				else
				{
					MessageBox.Show("Mikro V16 ve sonrası için gereklidir!");
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "80400000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				PerformansTestKurulum performansTestKurulum = new PerformansTestKurulum(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					performansTestKurulum.ShowDialog();
				}
				else
				{
					performansTestKurulum.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "80500000":
		{
			DebugIslemleri debugIslemleri = new DebugIslemleri(_mikrouygulamabilgileri);
			if (AsDialog)
			{
				debugIslemleri.ShowDialog();
			}
			else
			{
				debugIslemleri.Show();
			}
			break;
		}
		case "45081000":
			if (_mikrouygulamabilgileri.mikrokullanici.User_name.ToUpper() == "SRV")
			{
				TemsilciRutDuzenleme temsilciRutDuzenleme = new TemsilciRutDuzenleme(_mikrouygulamabilgileri);
				if (AsDialog)
				{
					temsilciRutDuzenleme.ShowDialog();
				}
				else
				{
					temsilciRutDuzenleme.Show();
				}
			}
			else
			{
				MessageBox.Show("Sisteme SRV ile giriş yapmanız gerekmektedir.");
			}
			break;
		case "25200000":
		{
			ComarchEdi comarchEdi = new ComarchEdi(_mikrouygulamabilgileri);
			if (AsDialog)
			{
				comarchEdi.ShowDialog();
			}
			else
			{
				comarchEdi.Show();
			}
			break;
		}
		case "45110000":
		{
			RaporEkstre raporEkstre = new RaporEkstre();
			CariEkstreButun cariEkstreButun = new CariEkstreButun();
			cariEkstreButun.tarih = DateTime.Now;
			Cari cari = new Cari();
			cari.cari_kod = "CARİ KOD";
			cari.cari_unvan1 = "CARİ ÜNVAN 1";
			cari.cari_unvan2 = "CARİ ÜNVAN 2";
			cariEkstreButun.cari = cari;
			cariEkstreButun.gruplar = new List<CariEkstreGrup>();
			CariEkstreGrup cariEkstreGrup = new CariEkstreGrup();
			cariEkstreGrup.GrupBaslik = "TL EKSTRESİ";
			cariEkstreGrup.Satirlar = new List<CariEkstreString>();
			CariEkstreString cariEkstreString = new CariEkstreString();
			cariEkstreString.EvrakCinsi = "Borç";
			cariEkstreString.Bakiye = 125985245.0;
			cariEkstreString.EvrakSeriSira = "A-8575498";
			cariEkstreString.EvrakTipi = "Satış faturası";
			cariEkstreString.Meblag = 25000000.56;
			cariEkstreString.Tarih = DateTime.Now;
			cariEkstreString.VadeTarihi = DateTime.Now;
			cariEkstreGrup.Satirlar.Add(cariEkstreString);
			CariEkstreString cariEkstreString2 = new CariEkstreString();
			cariEkstreString2.EvrakCinsi = "Alacak";
			cariEkstreString2.Bakiye = 47254214.0;
			cariEkstreString2.EvrakSeriSira = "A-345634";
			cariEkstreString2.EvrakTipi = "Tahsilat Makbuzu";
			cariEkstreString2.Meblag = 125124.67;
			cariEkstreString2.Tarih = DateTime.Now;
			cariEkstreString2.VadeTarihi = DateTime.Now;
			cariEkstreGrup.Satirlar.Add(cariEkstreString2);
			CariEkstreGrup cariEkstreGrup2 = new CariEkstreGrup();
			cariEkstreGrup2.GrupBaslik = "USD EKSTRESİ";
			cariEkstreGrup2.Satirlar = new List<CariEkstreString>();
			CariEkstreString cariEkstreString3 = new CariEkstreString();
			cariEkstreString3.EvrakCinsi = "Borç";
			cariEkstreString3.Bakiye = 125985245.0;
			cariEkstreString3.EvrakSeriSira = "A-8575498";
			cariEkstreString3.EvrakTipi = "Satış faturası";
			cariEkstreString3.Meblag = 25000000.56;
			cariEkstreString3.Tarih = DateTime.Now;
			cariEkstreString3.VadeTarihi = DateTime.Now;
			cariEkstreGrup2.Satirlar.Add(cariEkstreString3);
			CariEkstreString cariEkstreString4 = new CariEkstreString();
			cariEkstreString4.EvrakCinsi = "Alacak";
			cariEkstreString4.Bakiye = 47254214.0;
			cariEkstreString4.EvrakSeriSira = "A-345634";
			cariEkstreString4.EvrakTipi = "Tahsilat Makbuzu";
			cariEkstreString4.Meblag = 125124.67;
			cariEkstreString4.Tarih = DateTime.Now;
			cariEkstreString4.VadeTarihi = DateTime.Now;
			cariEkstreGrup2.Satirlar.Add(cariEkstreString4);
			cariEkstreButun.gruplar.Add(cariEkstreGrup);
			cariEkstreButun.gruplar.Add(cariEkstreGrup2);
			List<CariEkstreButun> list = new List<CariEkstreButun>();
			list.Add(cariEkstreButun);
			raporEkstre.DataSource = list;
			if (File.Exists("data\\cari_ekstre.repx"))
			{
				raporEkstre.LoadLayout("data\\cari_ekstre.repx");
			}
			else
			{
				MessageBox.Show("Cari ekstre tasarımını yaptıktan sonra, 'data' klasörü içinde 'cari_ekstre.repx' olarak kayıt ediniz.");
			}
			ReportDesignTool reportDesignTool = new ReportDesignTool(raporEkstre);
			try
			{
				raporEkstre.DisplayName = "cari_ekstre";
				reportDesignTool.ShowDesignerDialog();
				break;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Seçili raporun tipi uygun değil. Lütfen uygun bir rapor seçiniz. ex : " + ex.ToString());
				break;
			}
		}
		}
	}

	private bool LisansKontrol(string modul)
	{
		if (!Lisans.GetLisans(_mikrouygulamabilgileri.baglantibilgileri.SqlServer, modul).ModulLisansli(modul))
		{
			MessageBox.Show("Bu modül için lisans bulunamadı.");
			return false;
		}
		return true;
	}

	private void KaliteKontrolKurulum()
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		string commandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_FORA_KALITE_KONTROL]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[_FORA_KALITE_KONTROL]([RECno] [int] IDENTITY(1,1) NOT NULL,[sth_RECid_RECno] [int] NOT NULL,[sth_stok_kod] [nvarchar](25) NOT NULL,[sth_parti_kodu] [nvarchar](25) NOT NULL,[sth_aciklama] [nvarchar](50) NOT NULL,[sth_miktar] [float] NOT NULL,[sth_tarih] [datetime] NOT NULL,[kullanici_adi] [nvarchar](50) NOT NULL,[islem_tarih] [datetime] NOT NULL,[islem_yapildi] [bit] NOT NULL,[islem_aciklama] [nvarchar](50) NOT NULL,[kaynak_depo_no] [int] NOT NULL,[onay_depo_no] [int] NOT NULL,[onay_miktar] [float] NOT NULL,[ret_depo_no] [int] NOT NULL,[ret_miktar] [float] NOT NULL,[ret_aciklama] [nvarchar](50) NOT NULL,[ret_evrak_seri] [nvarchar](10) NOT NULL,[ret_evrak_sira] [int] NOT NULL,[hurda_depo_no] [int] NOT NULL,[hurda_miktar] [float] NOT NULL,[hurda_aciklama] [nvarchar](50) NOT NULL,[hurda_evrak_seri] [nvarchar](10) NOT NULL,[hurda_evrak_sira] [int] NOT NULL,[iade_depo_no] [int] NOT NULL,[iade_miktar] [float] NOT NULL,[iade_aciklama] [nvarchar](50) NOT NULL,[iade_evrak_seri] [nvarchar](10) NOT NULL,[iade_evrak_sira] [int] NOT NULL,CONSTRAINT [PK__FORA_KALITE_KONTROL] PRIMARY KEY CLUSTERED ([RECno] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY] CREATE NONCLUSTERED INDEX [NDX_FORA_KALITE_KONTROL_01] ON [dbo].[_FORA_KALITE_KONTROL] ([islem_tarih] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]  END";
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.ExecuteNonQuery();
		}
		sqlDB.ConnectionClose();
	}

	private void sqlBaglantiBilgileriToolStripMenuItem_Click(object sender, EventArgs e)
	{
		SqlBaglantiAyarlariAc();
	}

	private void SqlBaglantiAyarlariAc()
	{
		BaglantiAyarlari baglantiAyarlari = new BaglantiAyarlari("Sql Bağlantı Bilgileri", _mikrouygulamabilgileri.MikroAnaDBName);
		baglantiAyarlari._BaglantiBilgileri = _mikrouygulamabilgileri.baglantibilgileri;
		if (baglantiAyarlari.ShowDialog() == DialogResult.OK)
		{
			_mikrouygulamabilgileri.baglantibilgileri = baglantiAyarlari._BaglantiBilgileri;
			if (!_mikrouygulamabilgileri.baglantibilgileri.Save("data\\sqlbaglantibilgileri.xml"))
			{
				MessageBox.Show("Bağlantı Ayarları Kayıt Edilemedi!", "HATA");
			}
		}
	}

	private void hakkindaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		new Hakkinda(_mikrouygulamabilgileri).ShowDialog();
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
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sqlBaglantiBilgileriToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.cikisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.yardımToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.hakkindaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.tl_uygulamalar = new DevExpress.XtraTreeList.TreeList();
		this.colName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
		this.uygulamaid = new DevExpress.XtraTreeList.Columns.TreeListColumn();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.menuStrip1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.tl_uygulamalar).BeginInit();
		base.SuspendLayout();
		this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.dosyaToolStripMenuItem, this.yardımToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Padding = new System.Windows.Forms.Padding(6, 1, 0, 1);
		this.menuStrip1.Size = new System.Drawing.Size(334, 24);
		this.menuStrip1.TabIndex = 0;
		this.menuStrip1.Text = "menuStrip1";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.sqlBaglantiBilgileriToolStripMenuItem, this.cikisToolStripMenuItem });
		this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
		this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 22);
		this.dosyaToolStripMenuItem.Text = "Dosya";
		this.sqlBaglantiBilgileriToolStripMenuItem.Name = "sqlBaglantiBilgileriToolStripMenuItem";
		this.sqlBaglantiBilgileriToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
		this.sqlBaglantiBilgileriToolStripMenuItem.Text = "Sql bağlantı bilgileri";
		this.sqlBaglantiBilgileriToolStripMenuItem.Click += new System.EventHandler(sqlBaglantiBilgileriToolStripMenuItem_Click);
		this.cikisToolStripMenuItem.Name = "cikisToolStripMenuItem";
		this.cikisToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
		this.cikisToolStripMenuItem.Text = "Çıkış";
		this.cikisToolStripMenuItem.Click += new System.EventHandler(cikisToolStripMenuItem_Click);
		this.yardımToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.hakkindaToolStripMenuItem });
		this.yardımToolStripMenuItem.Name = "yardımToolStripMenuItem";
		this.yardımToolStripMenuItem.Size = new System.Drawing.Size(56, 22);
		this.yardımToolStripMenuItem.Text = "Yardım";
		this.hakkindaToolStripMenuItem.Name = "hakkindaToolStripMenuItem";
		this.hakkindaToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
		this.hakkindaToolStripMenuItem.Text = "Hakkında";
		this.hakkindaToolStripMenuItem.Click += new System.EventHandler(hakkindaToolStripMenuItem_Click);
		this.tl_uygulamalar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.tl_uygulamalar.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[2] { this.colName, this.uygulamaid });
		this.tl_uygulamalar.Location = new System.Drawing.Point(12, 51);
		this.tl_uygulamalar.Name = "tl_uygulamalar";
		this.tl_uygulamalar.OptionsBehavior.Editable = false;
		this.tl_uygulamalar.OptionsView.ShowColumns = false;
		this.tl_uygulamalar.OptionsView.ShowHorzLines = false;
		this.tl_uygulamalar.OptionsView.ShowIndicator = false;
		this.tl_uygulamalar.Size = new System.Drawing.Size(315, 389);
		this.tl_uygulamalar.TabIndex = 2;
		this.tl_uygulamalar.DoubleClick += new System.EventHandler(tl_uygulamalar_DoubleClick);
		this.tl_uygulamalar.KeyDown += new System.Windows.Forms.KeyEventHandler(tl_uygulamalar_KeyDown);
		this.colName.Caption = "treeListColumn1";
		this.colName.FieldName = "name";
		this.colName.Name = "colName";
		this.colName.Visible = true;
		this.colName.VisibleIndex = 0;
		this.uygulamaid.Caption = "treeListColumn1";
		this.uygulamaid.FieldName = "uygulamaid";
		this.uygulamaid.Name = "uygulamaid";
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Appearance.Options.UseFont = true;
		this.labelControl1.Location = new System.Drawing.Point(12, 31);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(72, 13);
		this.labelControl1.TabIndex = 3;
		this.labelControl1.Text = "Uygulamalar";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoScroll = true;
		base.ClientSize = new System.Drawing.Size(334, 452);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.tl_uygulamalar);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "AnaMenu";
		this.Text = "Fora Mikro - v3.02.14";
		base.Load += new System.EventHandler(AnaMenu_Load);
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.tl_uygulamalar).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
