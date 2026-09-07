using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using DevExpress.XtraEditors;
using Fora.App.Win.Mikro.SR_EDI;
using Fora.Mikro;
using Fora.Mikro.Barkodlar;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Depolar;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Projeler;
using Fora.Mikro.SorumlulukMerkezleri;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.FiyatListeleri;

namespace Fora.App.Win.Mikro.Aktarimlar.ComarchEdi;

public class ComarchEdi : Form
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler GenelParametreler;

	private string KullaniciAdi;

	private string Sifre;

	private int ZamanAsimi;

	private int SenkronizasyonSuresi;

	private string SpecialAlan1;

	private string SpecialAlan2;

	private string SpecialAlan3;

	private string EMailSmtpServer;

	private int EMailSmtpPort;

	private string EMailAdress;

	private string EMailSmtpPassword;

	private bool EMailSmtpUseSSL;

	private string EMailGorunenAd;

	private bool AktarilanKayitlariOkunduOlarakIsaretle;

	private int isleme_kalan_sure;

	private IContainer components;

	private SimpleButton sb_ayarlar;

	private TextBox tb_gecmis;

	private Timer timer_islem_baslatma;

	private Label label_Guncellemeye_Kalan_Sure;

	private Label label6;

	private SimpleButton sb_iliski_yonetimi;

	private SimpleButton sb_simdi_senkronize_et;

	private SimpleButton sb_yardim;

	public ComarchEdi(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		Application.DoEvents();
		GenelParametreleriOku();
		timer_islem_baslatma.Start();
	}

	public void GenelParametreleriOku()
	{
		GenelParametreler = ParametrelerDefault.ComarchEdiGenelParametreler();
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, GenelParametreler, "ComarchEdiGenel", "", "", "");
		KullaniciAdi = GenelParametreler._GetParametre("KullaniciAdi")._GetString;
		Sifre = GenelParametreler._GetParametre("Sifre")._GetString;
		ZamanAsimi = (int)GenelParametreler._GetParametre("ZamanAsimi")._GetDouble * 1000;
		SenkronizasyonSuresi = (int)GenelParametreler._GetParametre("SenkronizasyonSuresi")._GetDouble;
		SpecialAlan1 = GenelParametreler._GetParametre("SpecialAlan1")._GetString;
		SpecialAlan2 = GenelParametreler._GetParametre("SpecialAlan2")._GetString;
		SpecialAlan3 = GenelParametreler._GetParametre("SpecialAlan3")._GetString;
		EMailSmtpServer = GenelParametreler._GetParametre("EMailSmtpServer")._GetString;
		EMailSmtpPort = (int)GenelParametreler._GetParametre("EMailSmtpPort")._GetDouble;
		EMailAdress = GenelParametreler._GetParametre("EMailAdress")._GetString;
		EMailSmtpPassword = GenelParametreler._GetParametre("EMailSmtpPassword")._GetString;
		EMailSmtpUseSSL = GenelParametreler._GetParametre("EMailSmtpUseSSL")._GetBoolean;
		EMailGorunenAd = GenelParametreler._GetParametre("EMailGorunenAd")._GetString;
		AktarilanKayitlariOkunduOlarakIsaretle = GenelParametreler._GetParametre("AktarilanKayitlariOkunduOlarakIsaretle")._GetBoolean;
	}

	public void IslemBitir()
	{
		timer_islem_baslatma.Start();
	}

	public void SenkronizasyonMain()
	{
		isleme_kalan_sure = SenkronizasyonSuresi * 60;
		timer_islem_baslatma.Stop();
		string text = "genel_evrak_aktarimi";
		if (AppBase.MikroVersiyonu >= 16)
		{
			text = text + "_v" + AppBase.MikroVersiyonu;
		}
		if (!Lisans.GetLisans(_mikrouygulamabilgileri.baglantibilgileri.SqlServer, text).ModulLisansli(text))
		{
			tb_gecmis.AppendText(DateTime.Now.ToString() + " -> Lisans bulunamadı!" + Environment.NewLine);
			IslemBitir();
			return;
		}
		tb_gecmis.AppendText(DateTime.Now.ToString() + " -> Senkronizasyon başladı." + Environment.NewLine);
		Application.DoEvents();
		EDIServiceSoapClient eDIServiceSoapClient = new EDIServiceSoapClient();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		List<string> list4 = new List<string>();
		List<string> list5 = new List<string>();
		List<string> list6 = new List<string>();
		try
		{
			RetRes retRes = eDIServiceSoapClient.Relationships(KullaniciAdi, Sifre, ZamanAsimi);
			if (retRes.Res != "00000000")
			{
				string text2 = "İlişkiler çekilemedi!";
				switch (retRes.Res)
				{
				case "00000001":
					text2 = text2 + " Kullanıcı adı veya şifre hatalı! Hata kodu : " + retRes.Res;
					break;
				case "00000003":
					text2 = text2 + " Servis hatası! Hata kodu : " + retRes.Res;
					break;
				case "00000004":
					text2 = text2 + " Servis hatası! Hata kodu : " + retRes.Res;
					break;
				case "00000006":
					text2 = text2 + " Servis hatası! Hata kodu : " + retRes.Res;
					break;
				case "00000005":
					text2 = text2 + " Zaman aşımı süresi doldu! Hata kodu : " + retRes.Res;
					break;
				}
				tb_gecmis.AppendText(DateTime.Now.ToString() + " -> " + text2 + Environment.NewLine);
				IslemBitir();
				return;
			}
			using XmlReader reader = XmlReader.Create(new StringReader(retRes.Cnt));
			foreach (XElement item in XDocument.Load(reader, LoadOptions.None).Descendants("relation"))
			{
				list.Add(item.Element("relation-id").Value);
				list2.Add(item.Element("partner-iln").Value);
				list3.Add(item.Element("partner-name").Value);
				list4.Add(item.Element("document-version").Value);
				list5.Add(item.Element("document-standard").Value);
				list6.Add(item.Element("document-test").Value);
			}
		}
		catch
		{
			tb_gecmis.AppendText(DateTime.Now.ToString() + " -> İlişkiler çekilemedi! Hata : Servise erişilemiyor. İnternet bağlantısını kontrol ediniz." + Environment.NewLine);
			IslemBitir();
			return;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		for (int i = 0; i < list.Count; i++)
		{
			string text3 = "";
			Parametreler parametreler = ParametrelerDefault.ComarchEdiIliskiParametreleri(list3[i]);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "ComarchEdiIliski", list3[i], "", "");
			bool getBoolean = parametreler._GetParametre("SiparisAktarimiAktif")._GetBoolean;
			Proje proje = ProjeData.GetProje(sqlDB.Connection, parametreler._GetParametre("ProjeKodu")._GetString);
			string getString = parametreler._GetParametre("EvrakSeri")._GetString;
			string getString2 = parametreler._GetParametre("YeniSiparisBilgilendirmeEpostaAdresi")._GetString;
			string getString3 = parametreler._GetParametre("HataBilgilendirmeEpostaAdresi")._GetString;
			bool getBoolean2 = parametreler._GetParametre("BirimFiyatiBirim2KatsayisinaBol")._GetBoolean;
			if (!getBoolean)
			{
				tb_gecmis.AppendText(DateTime.Now.ToString() + " -> " + list3[i] + " için ilişki yönetiminden tanımlama yapılması gerekmektedir." + Environment.NewLine);
				continue;
			}
			tb_gecmis.AppendText(DateTime.Now.ToString() + " -> " + list3[i] + " sipariş bilgisi çekiliyor." + Environment.NewLine);
			Application.DoEvents();
			List<string> list7 = new List<string>();
			List<string> list8 = new List<string>();
			List<string> list9 = new List<string>();
			using (XmlReader reader2 = XmlReader.Create(new StringReader(eDIServiceSoapClient.ListMB(KullaniciAdi, Sifre, list2[i], "ORDER", list4[i], list5[i], list6[i], "N", ZamanAsimi).Cnt)))
			{
				foreach (XElement item2 in XDocument.Load(reader2, LoadOptions.None).Descendants("document-info"))
				{
					list7.Add(item2.Element("tracking-id").Value);
					list8.Add(item2.Element("document-number").Value);
					list9.Add(item2.Element("document-date").Value);
				}
			}
			tb_gecmis.AppendText(DateTime.Now.ToString() + " -> " + list3[i] + " için " + list7.Count + " adet sipariş bulundu." + Environment.NewLine);
			Application.DoEvents();
			int num = 0;
			for (int j = 0; j < list7.Count; j++)
			{
				using XmlReader reader3 = XmlReader.Create(new StringReader(eDIServiceSoapClient.Receive(KullaniciAdi, Sifre, list2[i], "ORDER", list7[j], list5[i], "N", ZamanAsimi).Cnt));
				foreach (XElement item3 in XDocument.Load(reader3, LoadOptions.None).Descendants("Document-Order"))
				{
					bool flag = true;
					string value = item3.Element("Order-Header").Element("ExpectedDeliveryDate").Value;
					string value2 = item3.Element("Order-Parties").Element("DeliveryPoint").Element("ILN")
						.Value;
					string value3 = item3.Element("Order-Parties").Element("Seller").Element("CodeByBuyer")
						.Value;
					Console.WriteLine("CodeByBuyer : " + value3 + Environment.NewLine);
					string text4 = parametreler._GetParametre("SorumlulukMerkeziKodu")._GetString;
					if (text4.Contains(","))
					{
						try
						{
							string[] array = text4.Split(',');
							int num2 = 0;
							string[] array2 = array;
							for (int k = 0; k < array2.Length; k++)
							{
								if (array2[k] == value3)
								{
									text4 = array[num2 + 1];
									break;
								}
								num2++;
							}
						}
						catch
						{
						}
					}
					SorumlulukMerkezi sorumlulukMerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(sqlDB.Connection, text4);
					Cari cari = new Cari();
					cari = CariData.GetCariByCariKod(sqlDB.Connection, value2, AdreslerTemsilciyeGore: false, "");
					if (cari.cari_kod == "")
					{
						cari = CariData.GetCariByVergiTcKimlikNo(sqlDB.Connection, value2, AdreslerTemsilciyeGore: false, "");
					}
					if (cari.cari_kod == "")
					{
						cari = CariData.GetCariByEMail(sqlDB.Connection, value2, AdreslerTemsilciyeGore: false, "");
					}
					if (cari.cari_kod == "")
					{
						cari = CariData.GetCariByBankaHesapNo(sqlDB.Connection, value2, AdreslerTemsilciyeGore: false, "");
					}
					if (cari.cari_kod == "")
					{
						tb_gecmis.AppendText(DateTime.Now.ToString() + " -> " + list3[i] + " için " + value2 + " cari hesabı bulunamadı." + Environment.NewLine);
						text3 = text3 + DateTime.Now.ToString() + " -> " + list3[i] + " için " + value2 + " cari hesabı bulunamadı." + Environment.NewLine;
						flag = false;
					}
					List<Stok> list10 = new List<Stok>();
					foreach (XElement item4 in item3.Descendants("Order-Lines").Descendants("Line"))
					{
						string value4 = item4.Element("Line-Item").Element("EAN").Value;
						double miktar = double.Parse(item4.Element("Line-Item").Element("OrderedQuantity").Value.Replace(".", ","));
						double num3 = double.Parse(item4.Element("Line-Item").Element("OrderedUnitNetPrice").Value.Replace(".", ","));
						Stok stok = new Stok();
						Barkod barkodBilgisi = BarkodData.GetBarkodBilgisi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, value4);
						if (barkodBilgisi.bar_stokkodu != "")
						{
							stok = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, barkodBilgisi.bar_stokkodu, enum_toptan_perakende.Toptan);
						}
						if (stok.sto_kod == "" || stok.sto_kod == null)
						{
							tb_gecmis.AppendText(DateTime.Now.ToString() + " -> " + list3[i] + " için " + list9[j] + " tarihli " + list8[j] + " numaralı siparişte yer alan " + value4 + " barkodlu ürün bulunamadı." + Environment.NewLine);
							text3 = text3 + DateTime.Now.ToString() + " -> " + list3[i] + " için " + list9[j] + " tarihli " + list8[j] + " numaralı siparişte yer alan " + value4 + " barkodlu ürün bulunamadı." + Environment.NewLine;
							flag = false;
						}
						else
						{
							if (getBoolean2)
							{
								num3 /= stok.sto_birim2_katsayi * -1.0;
							}
							stok.ekleme_bilgileri.Miktar = miktar;
							stok.ekleme_bilgileri.BirimFiyat.FiyatBrut = num3;
							list10.Add(stok);
						}
					}
					if (!flag)
					{
						continue;
					}
					Evrak evrak = new Evrak(enum_GenelEvrakTipleri.AlinanSiparis, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
					evrak.SetAlternatifDovizCinsi(_mikrouygulamabilgileri.veritabani.DB_alternatif_doviz);
					evrak.alternatifdovizkuru = new Kur();
					evrak.SetBelgeNo(list8[j]);
					try
					{
						evrak.SetBelgeTarihi(DateTime.Parse(list9[j]));
					}
					catch
					{
					}
					try
					{
						evrak.SetEvrakTarihi(DateTime.Parse(list9[j]));
					}
					catch
					{
					}
					try
					{
						evrak.SetSevkTeslimTarihi(DateTime.Parse(value));
					}
					catch
					{
					}
					int cari_odemeplan_no = cari.cari_odemeplan_no;
					string cari_temsilci_kodu = cari.cari_temsilci_kodu;
					SqlConnection sqlConnection = new SqlConnection();
					if (_mikrouygulamabilgileri.baglantibilgileri.SqlUserName == "")
					{
						sqlConnection.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; Trusted_Connection=true;";
					}
					else
					{
						sqlConnection.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; User Id=" + _mikrouygulamabilgileri.baglantibilgileri.SqlUserName + ";Password=" + _mikrouygulamabilgileri.baglantibilgileri.SqlPassword + ";";
					}
					sqlConnection.Open();
					FiyatListesi fiyatListesi = FiyatListesiData.GetFiyatListesi(sqlConnection, cari.cari_satis_fk);
					int cari_doviz_cinsi = cari.cari_doviz_cinsi;
					Kur kur = new Kur();
					if (cari_doviz_cinsi != 0)
					{
						kur = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, cari_doviz_cinsi, "1", _mikrouygulamabilgileri.MikroAnaDBName);
					}
					int cariIlkAdresNo = CariData.GetCariIlkAdresNo(sqlConnection, cari.cari_kod);
					sqlConnection.Close();
					evrak.SetCari(cari, cariIlkAdresNo, cari_odemeplan_no, cari_temsilci_kodu, fiyatListesi, cari_doviz_cinsi, kur);
					evrak.SetDegistirSpecialAlan1(SpecialAlan1);
					evrak.SetDegistirSpecialAlan2(SpecialAlan2);
					evrak.SetDegistirSpecialAlan3(SpecialAlan3);
					evrak.SetDovizCinsi(0);
					evrak.SetEvrakNoSeri(getString);
					evrak.SetKaynakDepo(new Depo(cari.cari_VarsayilanCikisDepo));
					evrak.kur = new Kur();
					evrak.SetMikroUserNo(_mikrouygulamabilgileri.mikrokullanici.User_no);
					evrak.Parametreler.vergitanimlari = _mikrouygulamabilgileri.vergitanimlari;
					evrak.SetSorumlulukMerkezi(sorumlulukMerkezi);
					evrak.SetProje(proje);
					foreach (Stok item5 in list10)
					{
						item5.ekleme_bilgileri.proje_kodu = evrak.proje.pro_kodu;
						item5.ekleme_bilgileri.sorumluluk_merkezi_kodu = evrak.sorumlulukmerkezi.som_kod;
					}
					evrak.AddUrun(list10, BarkodOkuma: false);
					SqlConnection sqlConnection2 = new SqlConnection();
					if (_mikrouygulamabilgileri.baglantibilgileri.SqlUserName == "")
					{
						sqlConnection2.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; Trusted_Connection=true;";
					}
					else
					{
						sqlConnection2.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; User Id=" + _mikrouygulamabilgileri.baglantibilgileri.SqlUserName + ";Password=" + _mikrouygulamabilgileri.baglantibilgileri.SqlPassword + ";";
					}
					sqlConnection2.Open();
					SqlTransaction sqlTransaction = sqlConnection2.BeginTransaction();
					try
					{
						int num4 = EvrakData.EvrakKaydet(sqlConnection2, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak, EArsivAktif: false);
						if (num4 < 0)
						{
							tb_gecmis.AppendText(DateTime.Now.ToString() + " -> " + list3[i] + " için " + list9[j] + " tarihli " + list8[j] + " numaralı sipariş aktarılamadı." + Environment.NewLine);
							text3 = text3 + DateTime.Now.ToString() + " -> " + list3[i] + " için " + list9[j] + " tarihli " + list8[j] + " numaralı sipariş aktarılamadı." + Environment.NewLine;
							sqlTransaction.Commit();
							continue;
						}
						bool flag2 = true;
						if (AktarilanKayitlariOkunduOlarakIsaretle && eDIServiceSoapClient.ChangeDocumentStatus(KullaniciAdi, Sifre, list7[j], "R").Res != "00000000")
						{
							flag2 = false;
						}
						if (flag2)
						{
							num++;
							try
							{
								if (getString2 != "")
								{
									MailMessage mailMessage = new MailMessage();
									SmtpClient smtpClient = new SmtpClient(EMailSmtpServer);
									mailMessage.From = new MailAddress(EMailAdress, EMailGorunenAd, Encoding.UTF8);
									string[] array2 = getString2.Split(';');
									foreach (string text5 in array2)
									{
										if (text5 != "")
										{
											mailMessage.To.Add(text5);
										}
									}
									mailMessage.Subject = list3[i] + " için " + list9[j] + " tarihli " + list8[j] + " numaralı sipariş " + evrak.EvrakNoSeri + "-" + num4 + " no ile Mikro'ya aktarıldı";
									mailMessage.Body = "Evrak seri-sıra : " + evrak.EvrakNoSeri + "-" + num4 + Environment.NewLine;
									mailMessage.Body = mailMessage.Body + "Tarih : " + evrak.EvrakTarihi.ToString() + Environment.NewLine;
									mailMessage.Body = mailMessage.Body + "Beklenen teslim tarihi : " + evrak.SevkTeslimTarihi.ToString() + Environment.NewLine;
									mailMessage.Body = mailMessage.Body + "Cari kodu : " + evrak.cari.cari_kod + Environment.NewLine;
									MailMessage mailMessage2 = mailMessage;
									mailMessage2.Body = mailMessage2.Body + "Cari ünvan : " + evrak.cari.cari_unvan1 + " " + evrak.cari.cari_unvan2 + Environment.NewLine;
									mailMessage.Body = mailMessage.Body + Environment.NewLine + Environment.NewLine + Environment.NewLine;
									mailMessage.Body = mailMessage.Body + "Ürünler" + Environment.NewLine;
									mailMessage.Body = mailMessage.Body + "================================" + Environment.NewLine;
									foreach (Stok item6 in list10)
									{
										mailMessage2 = mailMessage;
										mailMessage2.Body = mailMessage2.Body + item6.ekleme_bilgileri.Miktar + " " + item6.sto_birim1_ad + " " + item6.sto_isim + " (" + item6.sto_kod + ")" + Environment.NewLine;
									}
									smtpClient.Port = EMailSmtpPort;
									smtpClient.Credentials = new NetworkCredential(EMailAdress, EMailSmtpPassword);
									if (EMailSmtpUseSSL)
									{
										smtpClient.EnableSsl = true;
									}
									else
									{
										smtpClient.EnableSsl = false;
									}
									smtpClient.Send(mailMessage);
								}
							}
							catch
							{
								tb_gecmis.AppendText(DateTime.Now.ToString() + " -> Hata e-postası gönderilemedi. Ayarları kontrol ediniz." + Environment.NewLine);
							}
							sqlTransaction.Commit();
						}
						else
						{
							sqlTransaction.Rollback();
						}
					}
					catch (Exception ex)
					{
						tb_gecmis.AppendText(DateTime.Now.ToString() + " -> " + list3[i] + " için " + list9[j] + " tarihli " + list8[j] + " numaralı sipariş aktarılamadı. Hata : " + ex.ToString() + Environment.NewLine);
						text3 = text3 + DateTime.Now.ToString() + " -> " + list3[i] + " için " + list9[j] + " tarihli " + list8[j] + " numaralı sipariş aktarılamadı. Hata : " + ex.ToString() + Environment.NewLine;
						sqlTransaction.Rollback();
					}
					finally
					{
						sqlConnection2.Close();
					}
				}
			}
			tb_gecmis.AppendText(DateTime.Now.ToString() + " -> " + list3[i] + " için " + num + " adet sipariş aktarıldı." + Environment.NewLine);
			Application.DoEvents();
			if (!(text3 != ""))
			{
				continue;
			}
			try
			{
				if (getString3 != "")
				{
					MailMessage mailMessage3 = new MailMessage();
					SmtpClient smtpClient2 = new SmtpClient(EMailSmtpServer);
					mailMessage3.From = new MailAddress(EMailAdress, EMailGorunenAd, Encoding.UTF8);
					string[] array2 = getString3.Split(';');
					foreach (string text6 in array2)
					{
						if (text6 != "")
						{
							mailMessage3.To.Add(text6);
						}
					}
					mailMessage3.Subject = "EDI Aktarım hatalar (" + list3[i] + ")";
					mailMessage3.Body = text3;
					smtpClient2.Port = EMailSmtpPort;
					smtpClient2.Credentials = new NetworkCredential(EMailAdress, EMailSmtpPassword);
					if (EMailSmtpUseSSL)
					{
						smtpClient2.EnableSsl = true;
					}
					else
					{
						smtpClient2.EnableSsl = false;
					}
					smtpClient2.Send(mailMessage3);
				}
				else
				{
					tb_gecmis.AppendText(DateTime.Now.ToString() + " -> " + list3[i] + " için hata e-posta adresi tanımlanmadığı için hatalar gönderilemedi." + Environment.NewLine);
				}
			}
			catch
			{
				tb_gecmis.AppendText(DateTime.Now.ToString() + " -> Hata e-postası gönderilemedi. Ayarları kontrol ediniz." + Environment.NewLine);
			}
		}
		sqlDB.ConnectionClose();
		IslemBitir();
	}

	private void sb_ayarlar_Click(object sender, EventArgs e)
	{
		if (new ComarchEdiGenelParametrelerForm(_mikrouygulamabilgileri).ShowDialog() == DialogResult.OK)
		{
			IslemBitir();
			GenelParametreleriOku();
			SenkronizasyonMain();
		}
	}

	private void timer_ekran_guncelleme_Tick(object sender, EventArgs e)
	{
		isleme_kalan_sure--;
		TimeSpan timeSpan = new TimeSpan(0, 0, isleme_kalan_sure);
		label_Guncellemeye_Kalan_Sure.Text = timeSpan.ToString();
		if (isleme_kalan_sure < 1)
		{
			SenkronizasyonMain();
		}
	}

	private void sb_iliski_yonetimi_Click(object sender, EventArgs e)
	{
		if (new ComarchEdiIliskiYonetimi(_mikrouygulamabilgileri, GenelParametreler).ShowDialog() == DialogResult.OK)
		{
			IslemBitir();
			GenelParametreleriOku();
			SenkronizasyonMain();
		}
	}

	private void sb_simdi_senkronize_et_Click(object sender, EventArgs e)
	{
		SenkronizasyonMain();
	}

	private void sb_yardim_Click(object sender, EventArgs e)
	{
		new ComarchEdiYardim().Show();
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
		this.sb_ayarlar = new DevExpress.XtraEditors.SimpleButton();
		this.tb_gecmis = new System.Windows.Forms.TextBox();
		this.timer_islem_baslatma = new System.Windows.Forms.Timer(this.components);
		this.label_Guncellemeye_Kalan_Sure = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.sb_iliski_yonetimi = new DevExpress.XtraEditors.SimpleButton();
		this.sb_simdi_senkronize_et = new DevExpress.XtraEditors.SimpleButton();
		this.sb_yardim = new DevExpress.XtraEditors.SimpleButton();
		base.SuspendLayout();
		this.sb_ayarlar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.sb_ayarlar.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlar.Appearance.Options.UseFont = true;
		this.sb_ayarlar.Location = new System.Drawing.Point(671, 47);
		this.sb_ayarlar.Name = "sb_ayarlar";
		this.sb_ayarlar.Size = new System.Drawing.Size(122, 23);
		this.sb_ayarlar.TabIndex = 3;
		this.sb_ayarlar.Text = "GENEL AYARLAR";
		this.sb_ayarlar.Click += new System.EventHandler(sb_ayarlar_Click);
		this.tb_gecmis.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tb_gecmis.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.tb_gecmis.Location = new System.Drawing.Point(12, 76);
		this.tb_gecmis.MaxLength = 32;
		this.tb_gecmis.Multiline = true;
		this.tb_gecmis.Name = "tb_gecmis";
		this.tb_gecmis.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.tb_gecmis.Size = new System.Drawing.Size(918, 335);
		this.tb_gecmis.TabIndex = 1;
		this.timer_islem_baslatma.Interval = 1000;
		this.timer_islem_baslatma.Tick += new System.EventHandler(timer_ekran_guncelleme_Tick);
		this.label_Guncellemeye_Kalan_Sure.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_Guncellemeye_Kalan_Sure.Location = new System.Drawing.Point(202, 15);
		this.label_Guncellemeye_Kalan_Sure.Name = "label_Guncellemeye_Kalan_Sure";
		this.label_Guncellemeye_Kalan_Sure.Size = new System.Drawing.Size(168, 16);
		this.label_Guncellemeye_Kalan_Sure.TabIndex = 13;
		this.label_Guncellemeye_Kalan_Sure.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label6.Location = new System.Drawing.Point(12, 15);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(184, 16);
		this.label6.TabIndex = 12;
		this.label6.Text = "Bir sonraki güncellemeye kalan süre :";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.sb_iliski_yonetimi.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.sb_iliski_yonetimi.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_iliski_yonetimi.Appearance.Options.UseFont = true;
		this.sb_iliski_yonetimi.Location = new System.Drawing.Point(799, 47);
		this.sb_iliski_yonetimi.Name = "sb_iliski_yonetimi";
		this.sb_iliski_yonetimi.Size = new System.Drawing.Size(131, 23);
		this.sb_iliski_yonetimi.TabIndex = 4;
		this.sb_iliski_yonetimi.Text = "İLİŞKİ YÖNETİMİ";
		this.sb_iliski_yonetimi.Click += new System.EventHandler(sb_iliski_yonetimi_Click);
		this.sb_simdi_senkronize_et.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_simdi_senkronize_et.Appearance.Options.UseFont = true;
		this.sb_simdi_senkronize_et.Location = new System.Drawing.Point(12, 47);
		this.sb_simdi_senkronize_et.Name = "sb_simdi_senkronize_et";
		this.sb_simdi_senkronize_et.Size = new System.Drawing.Size(152, 23);
		this.sb_simdi_senkronize_et.TabIndex = 2;
		this.sb_simdi_senkronize_et.Text = "ŞİMDİ SENKRONİZE ET";
		this.sb_simdi_senkronize_et.Click += new System.EventHandler(sb_simdi_senkronize_et_Click);
		this.sb_yardim.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.sb_yardim.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_yardim.Appearance.Options.UseFont = true;
		this.sb_yardim.Location = new System.Drawing.Point(808, 8);
		this.sb_yardim.Name = "sb_yardim";
		this.sb_yardim.Size = new System.Drawing.Size(122, 23);
		this.sb_yardim.TabIndex = 14;
		this.sb_yardim.Text = "YARDIM";
		this.sb_yardim.Click += new System.EventHandler(sb_yardim_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(942, 423);
		base.Controls.Add(this.sb_yardim);
		base.Controls.Add(this.sb_simdi_senkronize_et);
		base.Controls.Add(this.sb_iliski_yonetimi);
		base.Controls.Add(this.label_Guncellemeye_Kalan_Sure);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.tb_gecmis);
		base.Controls.Add(this.sb_ayarlar);
		base.Name = "ComarchEdi";
		this.Text = "Comarch Edi Aktarım";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
