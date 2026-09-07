using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using Fora.App.Win.Mikro.GenelFormlar;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Siparis;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Yazdirma;

namespace Fora.App.Win.Mikro.Ayarlar;

public class YaziciAyarlariForm : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private YaziciAyarlari _yaziciayarlari;

	private bool DegisiklikVar;

	private string AktifSablon;

	private string AktifAlan;

	private List<string> _yazilacaktext;

	private IContainer components;

	private ListBoxControl lb_sablonlar;

	private XtraTabControl tc_parametreler;

	private XtraTabPage Alanlar;

	private XtraTabPage OnIzleme;

	private SimpleButton yazdir;

	private RichTextBox rtb_preview;

	private XtraTabPage xtraTabPage_Genel;

	private TextEdit te_kullanici_adi;

	private LabelControl labelControl1;

	private LabelControl labelControl2;

	private SpinEdit SayfaKolonSayisi;

	private LabelControl labelControl4;

	private SpinEdit SayfaDokumSayisi;

	private LabelControl labelControl3;

	private SpinEdit SayfaSatirSayisi;

	private LabelControl labelControl7;

	private SpinEdit DetayBirSayfadakiKayitSayisi;

	private LabelControl labelControl6;

	private SpinEdit DetayBasiSatirSayisi;

	private LabelControl labelControl5;

	private SpinEdit DetayBaslangicSatiri;

	private ListBoxControl lbc_alanlar;

	private LabelControl labelControl8;

	private TextEdit te_eklenecek_alan;

	private SimpleButton sb_alan_ekle;

	private LabelControl labelControl21;

	private LabelControl labelControl15;

	private LabelControl labelControl14;

	private SpinEdit se_genislik;

	private LabelControl labelControl13;

	private SpinEdit se_satir;

	private LabelControl labelControl12;

	private SpinEdit se_kolon;

	private TextEdit te_statik_metin;

	private LabelControl labelControl11;

	private LabelControl labelControl9;

	private TextEdit te_alan_adi;

	private LabelControl labelControl10;

	private LabelControl labelControl16;

	private System.Windows.Forms.ComboBox cb_hizalama;

	private System.Windows.Forms.ComboBox cb_veritipi;

	private CheckEdit ce_detayin_bittigi_yere_kaydir;

	private System.Windows.Forms.ComboBox cb_basilacak_alan;

	private LabelControl labelControl18;

	private SimpleButton sb_alan_sil;

	private LabelControl labelControl20;

	private LabelControl labelControl24;

	private TextEdit te_com_port;

	private XtraTabPage xtraTabPage1;

	private LabelControl labelControl28;

	private LabelControl labelControl29;

	private TextEdit te_sira;

	private LabelControl labelControl30;

	private System.Windows.Forms.ComboBox EvrakTipi;

	private LabelControl labelControl31;

	private TextEdit te_seri;

	private System.Windows.Forms.ComboBox cb_statik_veri;

	private LabelControl labelControl19;

	private System.Windows.Forms.ComboBox cb_dinamik_veri;

	private LabelControl labelControl22;

	private CheckEdit ce_sonuna_para_birimi_ekle;

	private LabelControl labelControl25;

	private SpinEdit se_ondalik_hane_sayisi;

	private LabelControl labelControl23;

	private CheckEdit ce_basina_para_birimi_ekle;

	private TextEdit te_ondalik_ayraci;

	private LabelControl labelControl27;

	private TextEdit te_binlik_ayraci;

	private LabelControl labelControl26;

	private LabelControl labelControl34;

	private TextEdit te_son_ek;

	private LabelControl labelControl32;

	private TextEdit te_on_ek;

	private LabelControl labelControl33;

	private CheckEdit AltBasliklarSadeceSonSayfadaYazilsin;

	private Label label718;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem degisiklikleriKaydetToolStripMenuItem;

	private ToolStripMenuItem sablonSilToolStripMenuItem;

	private ToolStripMenuItem sablonEkleToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem dosyayaYazToolStripMenuItem;

	private ToolStripMenuItem dosyadanOkuToolStripMenuItem;

	private System.Windows.Forms.ComboBox cb_gruplama_secenegi;

	private LabelControl labelControl17;

	public YaziciAyarlariForm(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
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
				new object[2] { 0, "Üst başlık" },
				new object[2] { 1, "Satır" },
				new object[2] { 2, "Alt başlık" }
			}
		};
		cb_basilacak_alan.DataSource = dataSource;
		cb_basilacak_alan.ValueMember = "ID";
		cb_basilacak_alan.DisplayMember = "Isim";
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
				new object[2] { 0, "Sola daya" },
				new object[2] { 1, "Ortala" },
				new object[2] { 2, "Sağa daya" }
			}
		};
		cb_hizalama.DataSource = dataSource2;
		cb_hizalama.ValueMember = "ID";
		cb_hizalama.DisplayMember = "Isim";
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
				new object[2] { 0, "Stok kodu" },
				new object[2] { 1, "Ana grup" },
				new object[2] { 2, "Alt grup" },
				new object[2] { 3, "Sektor" },
				new object[2] { 4, "Marka" },
				new object[2] { 5, "Model" },
				new object[2] { 6, "Uretici" },
				new object[2] { 7, "Reyon" }
			}
		};
		cb_gruplama_secenegi.DataSource = dataSource3;
		cb_gruplama_secenegi.ValueMember = "ID";
		cb_gruplama_secenegi.DisplayMember = "Isim";
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
				new object[2] { 0, "Statik Metin" },
				new object[2] { 1, "Statik veri" },
				new object[2] { 2, "Dinamik veri" }
			}
		};
		cb_veritipi.DataSource = dataSource4;
		cb_veritipi.ValueMember = "ID";
		cb_veritipi.DisplayMember = "Isim";
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
				new object[2] { 0, "Evrak tarihi" },
				new object[2] { 1, "Saat" },
				new object[2] { 2, "Evrak seri" },
				new object[2] { 3, "Evrak sira" },
				new object[2] { 4, "Evrak seri-sira" },
				new object[2] { 5, "Belge no" },
				new object[2] { 6, "Belge tarihi" },
				new object[2] { 7, "Cari kod" },
				new object[2] { 8, "Cari ünvan 1" },
				new object[2] { 9, "Cari ünvan 2" },
				new object[2] { 10, "Cari ünvan birleşik" },
				new object[2] { 11, "Cari vergi dairesi no" },
				new object[2] { 12, "Cari vergi dairesi adı" },
				new object[2] { 13, "Cari vergi kimlik no" },
				new object[2] { 14, "Cari bölge kodu" },
				new object[2] { 15, "Cari cep telefonu" },
				new object[2] { 16, "Cari e-posta" },
				new object[2] { 17, "Cari grup kodu" },
				new object[2] { 18, "Cari sektör kodu" },
				new object[2] { 19, "Cari sicil no" },
				new object[2] { 20, "Cari web adresi" },
				new object[2] { 21, "Temsilci kodu" },
				new object[2] { 22, "Temsilci adı" },
				new object[2] { 23, "Temsilci soyadı" },
				new object[2] { 24, "Temsilci adı soyadı" },
				new object[2] { 25, "Döviz cinsi adı" },
				new object[2] { 26, "Döviz cinsi sembol" },
				new object[2] { 27, "Kur" },
				new object[2] { 28, "Alternatif döviz cinsi adı" },
				new object[2] { 29, "Alternatif döviz cinsi sembol" },
				new object[2] { 30, "Alternatif döviz kuru" },
				new object[2] { 31, "Çıkış depo no" },
				new object[2] { 32, "Çıkış depo adı" },
				new object[2] { 33, "Giriş depo no" },
				new object[2] { 34, "Giriş depo adı" },
				new object[2] { 35, "Nakliye depo no" },
				new object[2] { 36, "Nakliye depo adı" },
				new object[2] { 37, "Proje kodu" },
				new object[2] { 38, "Proje adı" },
				new object[2] { 39, "Sorumluluk merkezi kodu" },
				new object[2] { 40, "Sorumluluk merkezi adı" },
				new object[2] { 41, "Açıklama 1" },
				new object[2] { 42, "Açıklama 2" },
				new object[2] { 43, "Açıklama 3" },
				new object[2] { 44, "Açıklama 4" },
				new object[2] { 45, "Açıklama 5" },
				new object[2] { 46, "Açıklama 6" },
				new object[2] { 47, "Açıklama 7" },
				new object[2] { 48, "Açıklama 8" },
				new object[2] { 49, "Açıklama 9" },
				new object[2] { 50, "Açıklama 10" },
				new object[2] { 51, "Çek toplamı (Tahsilat)" },
				new object[2] { 52, "Nakit toplamı (Tahsilat)" },
				new object[2] { 53, "Çek sayısı (Tahsilat)" },
				new object[2] { 54, "Yekün" },
				new object[2] { 55, "Yekün (yazı ile)" },
				new object[2] { 56, "Ara toplam" },
				new object[2] { 57, "Toplam iskonto tutarı" },
				new object[2] { 58, "Toplam iskonto yüzdesi" },
				new object[2] { 59, "Toplam masraf tutarı" },
				new object[2] { 60, "Toplam masraf yüzdesi" },
				new object[2] { 61, "Toplam vergi tutarı" },
				new object[2] { 62, "Toplam vergi yüzdesi " },
				new object[2] { 63, "İskonto 1 toplamı" },
				new object[2] { 64, "İskonto 1 adı" },
				new object[2] { 65, "İskonto 1 yüzdesi" },
				new object[2] { 67, "İskonto 2 toplamı" },
				new object[2] { 68, "İskonto 2 adı" },
				new object[2] { 69, "İskonto 2 yüzdesi" },
				new object[2] { 70, "İskonto 3 toplamı" },
				new object[2] { 71, "İskonto 3 adı" },
				new object[2] { 72, "İskonto 3 yüzdesi" },
				new object[2] { 73, "İskonto 4 toplamı" },
				new object[2] { 74, "İskonto 4 adı" },
				new object[2] { 75, "İskonto 4 yüzdesi" },
				new object[2] { 76, "İskonto 5 toplamı" },
				new object[2] { 77, "İskonto 5 adı" },
				new object[2] { 78, "İskonto 5 yüzdesi" },
				new object[2] { 79, "İskonto 6 toplamı" },
				new object[2] { 80, "İskonto 6 adı" },
				new object[2] { 81, "İskonto 6 yüzdesi" },
				new object[2] { 82, "Masraf 1 toplamı" },
				new object[2] { 83, "Masraf 1 adı" },
				new object[2] { 84, "Masraf 1 yüzdesi" },
				new object[2] { 85, "Masraf 2 toplamı" },
				new object[2] { 86, "Masraf 2 adı" },
				new object[2] { 87, "Masraf 2 yüzdesi" },
				new object[2] { 88, "Masraf 3 toplamı" },
				new object[2] { 89, "Masraf 3 adı" },
				new object[2] { 90, "Masraf 3 yüzdesi" },
				new object[2] { 91, "Masraf 4 toplamı" },
				new object[2] { 92, "Masraf 4 adı" },
				new object[2] { 93, "Masraf 4 yüzdesi" },
				new object[2] { 94, "Vergi 1 toplamı" },
				new object[2] { 95, "Vergi 1 matrahı" },
				new object[2] { 96, "Vergi 1 adı" },
				new object[2] { 97, "Vergi 1 yüzdesi" },
				new object[2] { 98, "Vergi 2 toplamı" },
				new object[2] { 99, "Vergi 2 matrahı" },
				new object[2] { 100, "Vergi 2 adı" },
				new object[2] { 101, "Vergi 2 yüzdesi" },
				new object[2] { 102, "Vergi 3 toplamı" },
				new object[2] { 103, "Vergi 3 matrahı" },
				new object[2] { 104, "Vergi 3 adı" },
				new object[2] { 105, "Vergi 3 yüzdesi" },
				new object[2] { 106, "Vergi 4 toplamı" },
				new object[2] { 107, "Vergi 4 matrahı" },
				new object[2] { 108, "Vergi 4 adı" },
				new object[2] { 109, "Vergi 4 yüzdesi" },
				new object[2] { 110, "Vergi 5 toplamı" },
				new object[2] { 111, "Vergi 5 matrahı" },
				new object[2] { 112, "Vergi 5 adı" },
				new object[2] { 113, "Vergi 5 yüzdesi" },
				new object[2] { 114, "Vergi 6 toplamı" },
				new object[2] { 115, "Vergi 6 matrahı" },
				new object[2] { 116, "Vergi 6 adı" },
				new object[2] { 117, "Vergi 6 yüzdesi" },
				new object[2] { 118, "Vergi 7 toplamı" },
				new object[2] { 119, "Vergi 7 matrahı" },
				new object[2] { 120, "Vergi 7 adı" },
				new object[2] { 121, "Vergi 7 yüzdesi" },
				new object[2] { 122, "Vergi 8 toplamı" },
				new object[2] { 123, "Vergi 8 matrahı" },
				new object[2] { 124, "Vergi 8 adı" },
				new object[2] { 125, "Vergi 8 yüzdesi" },
				new object[2] { 126, "Vergi 9 toplamı" },
				new object[2] { 127, "Vergi 9 matrahı" },
				new object[2] { 128, "Vergi 9 adı" },
				new object[2] { 129, "Vergi 9 yüzdesi" },
				new object[2] { 130, "Vergi 10 toplamı" },
				new object[2] { 131, "Vergi 10 matrahı" },
				new object[2] { 132, "Vergi 10 adı" },
				new object[2] { 133, "Vergi 10 yüzdesi" },
				new object[2] { 134, "Miktar 1 toplamı" },
				new object[2] { 135, "Miktar 2 toplamı" },
				new object[2] { 136, "Miktar 3 toplamı" },
				new object[2] { 137, "Miktar 4 toplamı" },
				new object[2] { 138, "Sevk adresi sokak" },
				new object[2] { 139, "Sevk adresi cadde" },
				new object[2] { 140, "Sevk adresi sokak cadde" },
				new object[2] { 141, "Sevk adresi posta kodu" },
				new object[2] { 142, "Sevk adresi ilçe" },
				new object[2] { 143, "Sevk adresi il" },
				new object[2] { 144, "Sevk adresi posta kodu ilçe il" },
				new object[2] { 145, "Sevk adresi ilçe il" },
				new object[2] { 146, "Sevk adresi ülke" },
				new object[2] { 147, "Sevk adresi telefon 1" },
				new object[2] { 148, "Sevk adresi telefon 2" },
				new object[2] { 149, "Fatura adresi sokak" },
				new object[2] { 150, "Fatura adresi cadde" },
				new object[2] { 151, "Fatura adresi sokak cadde" },
				new object[2] { 152, "Fatura adresi posta kodu" },
				new object[2] { 153, "Fatura adresi ilçe" },
				new object[2] { 154, "Fatura adresi il" },
				new object[2] { 155, "Fatura adresi posta kodu ilçe il" },
				new object[2] { 156, "Fatura adresi ilçe il" },
				new object[2] { 157, "Fatura adresi ülke" },
				new object[2] { 158, "Fatura adresi telefon 1" },
				new object[2] { 159, "Fatura adresi telefon 2" },
				new object[2] { 160, "Toplam vergi matrahı" },
				new object[2] { 161, "Önceki bakiye" },
				new object[2] { 162, "Şimdiki bakiye" },
				new object[2] { 163, "Miktar2 (Bağımsız birim) toplamı" }
			}
		};
		cb_statik_veri.DataSource = dataSource5;
		cb_statik_veri.ValueMember = "ID";
		cb_statik_veri.DisplayMember = "Isim";
		DataTable dataSource6 = new DataTable
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
				new object[2] { 0, "Satır cinsi" },
				new object[2] { 1, "Satır cinsi kısa" },
				new object[2] { 2, "Hesap kodu" },
				new object[2] { 3, "Hesap adı" },
				new object[2] { 4, "Hesap yabancı adı" },
				new object[2] { 5, "Hesap kısa adı" },
				new object[2] { 6, "Beden kodu" },
				new object[2] { 7, "Beden adı" },
				new object[2] { 8, "Renk kodu" },
				new object[2] { 9, "Renk adı" },
				new object[2] { 10, "Alt grup kodu" },
				new object[2] { 11, "Alt grup adı" },
				new object[2] { 12, "Ana grup kodu" },
				new object[2] { 13, "Ana grup adı" },
				new object[2] { 14, "Sektör kodu" },
				new object[2] { 15, "Sektör adı" },
				new object[2] { 16, "Marka kodu" },
				new object[2] { 17, "Marka adı" },
				new object[2] { 18, "Model kodu" },
				new object[2] { 19, "Model adı" },
				new object[2] { 20, "Üretici kodu" },
				new object[2] { 21, "Üretici adı" },
				new object[2] { 22, "Reyon kodu" },
				new object[2] { 23, "Reyon adı" },
				new object[2] { 24, "Yer kodu (ambar adresi)" },
				new object[2] { 25, "Parti kodu" },
				new object[2] { 26, "Satır açıklama" },
				new object[2] { 27, "Satır açıklama 2" },
				new object[2] { 28, "Döviz cinsi adı" },
				new object[2] { 29, "Döviz cinsi sembol" },
				new object[2] { 30, "Kur" },
				new object[2] { 31, "Birim fiyat brüt" },
				new object[2] { 32, "Miktar" },
				new object[2] { 33, "Miktar 2" },
				new object[2] { 34, "Tutar" },
				new object[2] { 35, "Cari sorumluluk merkezi adı" },
				new object[2] { 36, "Cari sorumluluk merkezi kodu" },
				new object[2] { 37, "Stok sorumluluk merkezi adı" },
				new object[2] { 38, "Stok sorumluluk merkezi kodu" },
				new object[2] { 39, "Karşı sorumluluk merkezi adı" },
				new object[2] { 40, "Karşı sorumluluk merkezi kodu" },
				new object[2] { 42, "Proje kodu" },
				new object[2] { 43, "Proje adı" },
				new object[2] { 44, "İskonto 1 tutarı" },
				new object[2] { 45, "İskonto 2 tutarı" },
				new object[2] { 46, "İskonto 3 tutarı" },
				new object[2] { 47, "İskonto 4 tutarı" },
				new object[2] { 48, "İskonto 5 tutarı" },
				new object[2] { 49, "İskonto 6 tutarı" },
				new object[2] { 92, "İskonto 1 yüzdesi" },
				new object[2] { 93, "İskonto 2 yüzdesi" },
				new object[2] { 94, "İskonto 3 yüzdesi" },
				new object[2] { 95, "İskonto 4 yüzdesi" },
				new object[2] { 96, "İskonto 5 yüzdesi" },
				new object[2] { 97, "İskonto 6 yüzdesi" },
				new object[2] { 50, "Masraf 1 tutarı" },
				new object[2] { 51, "Masraf 2 tutarı" },
				new object[2] { 52, "Masraf 3 tutarı" },
				new object[2] { 53, "Masraf 4 tutarı" },
				new object[2] { 54, "Vergi tutarı" },
				new object[2] { 55, "Masraf vergi tutarı" },
				new object[2] { 56, "Ötv vergi" },
				new object[2] { 57, "Öiv vergi" },
				new object[2] { 58, "Birim 1 adı" },
				new object[2] { 59, "Birim 1 fiyat brüt" },
				new object[2] { 60, "Birim 1 fiyat net" },
				new object[2] { 61, "Birim 1 fiyat net kdv dahil" },
				new object[2] { 62, "Birim 1 miktar" },
				new object[2] { 63, "Birim 2 adı" },
				new object[2] { 64, "Birim 2 fiyat brüt" },
				new object[2] { 65, "Birim 2 fiyat net" },
				new object[2] { 66, "Birim 2 fiyat net kdv dahil" },
				new object[2] { 67, "Birim 2 miktar" },
				new object[2] { 68, "Birim 3 adı" },
				new object[2] { 69, "Birim 3 fiyat brüt" },
				new object[2] { 70, "Birim 3 fiyat net" },
				new object[2] { 71, "Birim 3 fiyat net kdv dahil" },
				new object[2] { 72, "Birim 3 miktar" },
				new object[2] { 73, "Birim 4 adı" },
				new object[2] { 74, "Birim 4 fiyat brüt" },
				new object[2] { 75, "Birim 4 fiyat net" },
				new object[2] { 76, "Birim 4 fiyat net kdv dahil" },
				new object[2] { 77, "Birim 4 miktar" },
				new object[2] { 78, "Satis birimi adı" },
				new object[2] { 79, "Satis birimi fiyat brüt" },
				new object[2] { 80, "Satis birimi fiyat net" },
				new object[2] { 81, "Satis birimi fiyat net kdv dahil" },
				new object[2] { 82, "Satis birimi miktar" },
				new object[2] { 83, "Toplam fiyat brüt" },
				new object[2] { 84, "Toplam fiyat net" },
				new object[2] { 106, "Toplam fiyat net kdv dahil" },
				new object[2] { 85, "Toplam iskonto tutarı" },
				new object[2] { 86, "Toplam iskonto yuzdesi" },
				new object[2] { 87, "Toplam masraf tutarı" },
				new object[2] { 88, "Toplam masraf yüzdesi" },
				new object[2] { 89, "Toplam vergi tutarı" },
				new object[2] { 90, "Toplam vergi yüzdesi" },
				new object[2] { 91, "Yekün" },
				new object[2] { 98, "Vergi adı" },
				new object[2] { 99, "Vergi yüzdesi (Karttan)" },
				new object[2] { 100, "Barkod 1. birim" },
				new object[2] { 101, "Barkod 2. birim" },
				new object[2] { 102, "Barkod 3. birim" },
				new object[2] { 103, "Barkod 4. birim" },
				new object[2] { 104, "Barkod satış birimi" },
				new object[2] { 105, "Vade" }
			}
		};
		cb_dinamik_veri.DataSource = dataSource6;
		cb_dinamik_veri.ValueMember = "ID";
		cb_dinamik_veri.DisplayMember = "Isim";
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		EvrakTipi.SelectedIndex = 0;
		SablonlariListele();
	}

	private void ParametreTextEdit_KeyDown(object sender, KeyEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void ParametreCheckEditMouseClick(object sender, MouseEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void SablonlariListele()
	{
		lb_sablonlar.Items.Clear();
		foreach (string item in YaziciAyarlariData.SablonIsimleriniGetir(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			lb_sablonlar.Items.Add(item);
		}
	}

	private void KullaniciParametreKaydet()
	{
		YaziciAyarlariData.SaveYaziciAyarlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _yaziciayarlari);
		_yaziciayarlari = YaziciAyarlariData.GetYaziciAyarlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, lb_sablonlar.SelectedValue.ToString());
		EkranBilgiGuncelle();
		DegisiklikVar = false;
	}

	private void lb_sablonlar_SelectedValueChanged(object sender, EventArgs e)
	{
		if (lb_sablonlar.SelectedValue == null)
		{
			return;
		}
		bool flag = true;
		if (DegisiklikVar && AktifSablon != lb_sablonlar.SelectedValue.ToString())
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
		if (AktifSablon == lb_sablonlar.SelectedValue.ToString())
		{
			flag = false;
		}
		if (flag)
		{
			tc_parametreler.Enabled = true;
			AktifSablon = lb_sablonlar.SelectedValue.ToString();
			_yaziciayarlari = YaziciAyarlariData.GetYaziciAyarlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, lb_sablonlar.SelectedValue.ToString());
			EkranBilgiGuncelle();
			DegisiklikVar = false;
		}
		else
		{
			lb_sablonlar.SelectedItem = AktifSablon;
		}
		if (tc_parametreler.SelectedTabPage.Name == "OnIzleme")
		{
			yaziciciktisiolustur();
		}
	}

	private void tc_parametreler_Selecting(object sender, TabPageCancelEventArgs e)
	{
		if (e.Page.Name == "OnIzleme")
		{
			yaziciciktisiolustur();
		}
	}

	private void yaziciciktisiolustur()
	{
		string text = EvrakTipi.SelectedItem.ToString();
		string evrakSeri = te_seri.Text;
		string s = te_sira.Text;
		GenelEvrak genelEvrak = new GenelEvrak();
		int dB_alternatif_doviz = _mikrouygulamabilgileri.veritabani.DB_alternatif_doviz;
		Evrak evrak = new Evrak();
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
		enum_GenelEvrakTipleri evrakTipi = enum_GenelEvrakTipleri.Tanimsiz;
		enum_sth_cins sth_cins = enum_sth_cins.Toptan;
		switch (text)
		{
		case "Alınan sipariş":
			evrakTipi = enum_GenelEvrakTipleri.AlinanSiparis;
			break;
		case "Satış faturası":
			evrakTipi = enum_GenelEvrakTipleri.SatisFaturasi;
			break;
		case "Satış irsaliyesi":
			evrakTipi = enum_GenelEvrakTipleri.SatisIrsaliyesi;
			break;
		case "Tahsilat makbuzu":
			evrakTipi = enum_GenelEvrakTipleri.Tahsilat;
			break;
		case "Tediye makbuzu":
			evrakTipi = enum_GenelEvrakTipleri.Tediye;
			break;
		case "Depolar arası sevk":
			evrakTipi = enum_GenelEvrakTipleri.DepolarArasiSevk;
			sth_cins = enum_sth_cins.Transfer;
			break;
		case "Depolar arası nakliye":
			evrakTipi = enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi;
			sth_cins = enum_sth_cins.Transfer;
			break;
		case "Depolar arası sipariş":
			evrakTipi = enum_GenelEvrakTipleri.DepolarArasiSiparis;
			break;
		case "Alış faturası":
			evrakTipi = enum_GenelEvrakTipleri.AlisFaturasi;
			break;
		case "Alış irsaliyesi":
			evrakTipi = enum_GenelEvrakTipleri.AlisIrsaliyesi;
			break;
		case "Proforma sipariş":
			evrakTipi = enum_GenelEvrakTipleri.ProformaSiparis;
			break;
		}
		rtb_preview.Text = "";
		try
		{
			evrak = EvrakData.GetEvrak(sqlConnection, evrakSeri, int.Parse(s), evrakTipi, dB_alternatif_doviz, enum_sip_cins.NormalSiparis, sth_cins);
		}
		catch
		{
			MessageBox.Show("Evrak oluşturulamadı. Evrak seri ve sıranın doğru olduğundan emin olunuz.");
			return;
		}
		enum_satir_gruplandirma_secenekleri gruplandirma_secenekleri = (enum_satir_gruplandirma_secenekleri)(int)cb_gruplama_secenegi.SelectedValue;
		genelEvrak = GenelEvrakData.ConvertEvrak(sqlConnection, evrak, gruplandirma_secenekleri);
		genelEvrak.simdiki_bakiye = CariData.GetBakiye(sqlConnection, genelEvrak.cari.cari_kod, -1, -1, -1, SorumlulukMerkeziDetayli: false, "");
		if (evrak.evraktipi != enum_GenelEvrakTipleri.Tahsilat && evrak.evraktipi != enum_GenelEvrakTipleri.Tediye)
		{
			genelEvrak.onceki_bakiye = genelEvrak.simdiki_bakiye - genelEvrak.yekun;
		}
		else
		{
			genelEvrak.onceki_bakiye = genelEvrak.simdiki_bakiye + genelEvrak.yekun;
		}
		sqlConnection.Close();
		_yazilacaktext = genelEvrak.GetDotMatrixText(_yaziciayarlari, _mikrouygulamabilgileri.vergitanimlari, _mikrouygulamabilgileri.doviz_cinsi_tanimlari);
		foreach (string item in _yazilacaktext)
		{
			rtb_preview.AppendText(item);
			rtb_preview.AppendText("\n");
		}
	}

	private void yazdir_Click(object sender, EventArgs e)
	{
		SerialPort serialPort = new SerialPort(te_com_port.Text, 9600, Parity.None, 8, StopBits.One);
		serialPort.Open();
		serialPort.Write(new byte[2] { 27, 64 }, 0, 2);
		serialPort.Write(new byte[1] { 10 }, 0, 1);
		foreach (string item in _yazilacaktext)
		{
			byte[] bytes = Encoding.GetEncoding(857).GetBytes(item + "\n");
			serialPort.Write(bytes, 0, bytes.Length);
			Thread.Sleep(500);
		}
		serialPort.Write(new byte[1] { 12 }, 0, 1);
		serialPort.Close();
	}

	private void EkranBilgiGuncelle()
	{
		te_kullanici_adi.Text = AktifSablon;
		SayfaKolonSayisi.Value = _yaziciayarlari.genelayarlar._GetParametre("SayfaKolonSayisi")._GetInt;
		SayfaSatirSayisi.Value = _yaziciayarlari.genelayarlar._GetParametre("SayfaSatirSayisi")._GetInt;
		SayfaDokumSayisi.Value = _yaziciayarlari.genelayarlar._GetParametre("SayfaDokumSayisi")._GetInt;
		DetayBaslangicSatiri.Value = _yaziciayarlari.genelayarlar._GetParametre("DetayBaslangicSatiri")._GetInt;
		DetayBasiSatirSayisi.Value = _yaziciayarlari.genelayarlar._GetParametre("DetayBasiSatirSayisi")._GetInt;
		DetayBirSayfadakiKayitSayisi.Value = _yaziciayarlari.genelayarlar._GetParametre("DetayBirSayfadakiKayitSayisi")._GetInt;
		AltBasliklarSadeceSonSayfadaYazilsin.Checked = _yaziciayarlari.genelayarlar._GetParametre("AltBasliklarSadeceSonSayfadaYazilsin")._GetBoolean;
		cb_gruplama_secenegi.SelectedValue = _yaziciayarlari.genelayarlar._GetParametre("StokGruplandirmaSecenegi")._GetInt;
		lbc_alanlar.Items.Clear();
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			lbc_alanlar.Items.Add(item._GetParametre("Isim")._GetString);
		}
	}

	private void SayfaKolonSayisi_EditValueChanged(object sender, EventArgs e)
	{
		_yaziciayarlari.genelayarlar._GetParametre("SayfaKolonSayisi")._SetInt = (int)SayfaKolonSayisi.Value;
	}

	private void SayfaSatirSayisi_EditValueChanged(object sender, EventArgs e)
	{
		_yaziciayarlari.genelayarlar._GetParametre("SayfaSatirSayisi")._SetInt = (int)SayfaSatirSayisi.Value;
	}

	private void SayfaDokumSayisi_EditValueChanged(object sender, EventArgs e)
	{
		_yaziciayarlari.genelayarlar._GetParametre("SayfaDokumSayisi")._SetInt = (int)SayfaDokumSayisi.Value;
	}

	private void DetayBaslangicSatiri_EditValueChanged(object sender, EventArgs e)
	{
		_yaziciayarlari.genelayarlar._GetParametre("DetayBaslangicSatiri")._SetInt = (int)DetayBaslangicSatiri.Value;
	}

	private void DetayBasiSatirSayisi_EditValueChanged(object sender, EventArgs e)
	{
		_yaziciayarlari.genelayarlar._GetParametre("DetayBasiSatirSayisi")._SetInt = (int)DetayBasiSatirSayisi.Value;
	}

	private void DetayBirSayfadakiKayitSayisi_EditValueChanged(object sender, EventArgs e)
	{
		_yaziciayarlari.genelayarlar._GetParametre("DetayBirSayfadakiKayitSayisi")._SetInt = (int)DetayBirSayfadakiKayitSayisi.Value;
	}

	private void lbc_alanlar_SelectedValueChanged(object sender, EventArgs e)
	{
		if (lbc_alanlar.SelectedValue != null)
		{
			AktifAlan = lbc_alanlar.SelectedValue.ToString();
			AlanGuncelle();
		}
	}

	private void AlanGuncelle()
	{
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				te_alan_adi.Text = AktifAlan;
				cb_basilacak_alan.SelectedValue = item._GetParametre("BasilacakAlan")._GetInt;
				int getInt = item._GetParametre("VeriTipi")._GetInt;
				cb_veritipi.SelectedValue = getInt;
				switch (getInt)
				{
				case 0:
					te_statik_metin.Text = item._GetParametre("Veri")._GetString;
					cb_statik_veri.SelectedIndex = 0;
					cb_dinamik_veri.SelectedIndex = 0;
					break;
				case 1:
					cb_statik_veri.SelectedValue = item._GetParametre("Veri")._GetInt;
					te_statik_metin.Text = "";
					cb_dinamik_veri.SelectedIndex = 0;
					break;
				case 2:
					cb_dinamik_veri.SelectedValue = item._GetParametre("Veri")._GetInt;
					te_statik_metin.Text = "";
					cb_statik_veri.SelectedIndex = 0;
					break;
				}
				se_kolon.Value = item._GetParametre("Kolon")._GetInt;
				se_satir.Value = item._GetParametre("Satir")._GetInt;
				se_genislik.Value = item._GetParametre("Genislik")._GetInt;
				cb_hizalama.SelectedValue = item._GetParametre("Hizalama")._GetInt;
				ce_detayin_bittigi_yere_kaydir.Checked = item._GetParametre("DetayinBittigiYereKaydir")._GetBoolean;
				se_ondalik_hane_sayisi.Value = item._GetParametre("OndalikHaneSayisi")._GetInt;
				ce_sonuna_para_birimi_ekle.Checked = item._GetParametre("SonunaParaBirimiEkle")._GetBoolean;
				ce_basina_para_birimi_ekle.Checked = item._GetParametre("BasinaParaBirimiEkle")._GetBoolean;
				te_binlik_ayraci.Text = item._GetParametre("BinlikAyraci")._GetString;
				te_ondalik_ayraci.Text = item._GetParametre("OndalikAyraci")._GetString;
				te_on_ek.Text = item._GetParametre("OnEk")._GetString;
				te_son_ek.Text = item._GetParametre("SonEk")._GetString;
				VeriTipiDegisti();
			}
		}
	}

	private void VeriTipiDegisti()
	{
		switch ((int)cb_veritipi.SelectedValue)
		{
		case 0:
			te_statik_metin.Enabled = true;
			te_statik_metin.Properties.Appearance.ForeColor = Color.Black;
			te_statik_metin.Properties.AppearanceDisabled.ForeColor = Color.Black;
			te_statik_metin.Properties.AppearanceFocused.ForeColor = Color.Black;
			cb_statik_veri.Enabled = false;
			cb_dinamik_veri.Enabled = false;
			break;
		case 1:
			te_statik_metin.Enabled = false;
			te_statik_metin.Text = "";
			te_statik_metin.Properties.Appearance.ForeColor = Color.Yellow;
			te_statik_metin.Properties.AppearanceDisabled.ForeColor = Color.Yellow;
			te_statik_metin.Properties.AppearanceFocused.ForeColor = Color.Yellow;
			cb_statik_veri.Enabled = true;
			cb_dinamik_veri.Enabled = false;
			break;
		case 2:
			te_statik_metin.Enabled = false;
			te_statik_metin.Text = "";
			te_statik_metin.Properties.Appearance.ForeColor = Color.Yellow;
			te_statik_metin.Properties.AppearanceDisabled.ForeColor = Color.Yellow;
			te_statik_metin.Properties.AppearanceFocused.ForeColor = Color.Yellow;
			cb_statik_veri.Enabled = false;
			cb_dinamik_veri.Enabled = true;
			break;
		}
	}

	private void cb_veritipi_SelectedValueChanged(object sender, EventArgs e)
	{
		if (cb_veritipi.SelectedValue == null || _yaziciayarlari == null)
		{
			return;
		}
		VeriTipiDegisti();
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("VeriTipi")._SetInt = (int)cb_veritipi.SelectedValue;
			}
		}
	}

	private void cb_basilacak_alan_SelectedValueChanged(object sender, EventArgs e)
	{
		if (cb_basilacak_alan.SelectedValue == null || _yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("BasilacakAlan")._SetInt = (int)cb_basilacak_alan.SelectedValue;
			}
		}
	}

	private void te_veri_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan && item._GetParametre("VeriTipi")._GetInt == 0)
			{
				item._GetParametre("Veri")._SetString = te_statik_metin.Text;
			}
		}
	}

	private void se_kolon_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("Kolon")._SetInt = (int)se_kolon.Value;
			}
		}
	}

	private void se_satir_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("Satir")._SetInt = (int)se_satir.Value;
			}
		}
	}

	private void se_genislik_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("Genislik")._SetInt = (int)se_genislik.Value;
			}
		}
	}

	private void cb_hizalama_SelectedValueChanged(object sender, EventArgs e)
	{
		if (cb_hizalama.SelectedValue == null || _yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("Hizalama")._SetInt = (int)cb_hizalama.SelectedValue;
			}
		}
	}

	private void ce_detayin_bittigi_yere_kaydir_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("DetayinBittigiYereKaydir")._SetBoolean = ce_detayin_bittigi_yere_kaydir.Checked;
			}
		}
	}

	private void sb_alan_ekle_Click(object sender, EventArgs e)
	{
		if (te_eklenecek_alan.Text != "")
		{
			bool flag = false;
			foreach (Parametreler item in _yaziciayarlari.alanlar)
			{
				if (item._GetParametre("Isim")._GetString == te_eklenecek_alan.Text)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_yaziciayarlari.alanekle(te_eklenecek_alan.Text);
				DegisiklikVar = true;
				EkranBilgiGuncelle();
			}
			else
			{
				MessageBox.Show("Alan adı kullanılmış. Lütfen başka bir alan adı seçiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		else
		{
			MessageBox.Show("Eklenecek kaydın adını girmelisiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void sb_alan_sil_Click(object sender, EventArgs e)
	{
		if (lbc_alanlar.SelectedValue != null)
		{
			string text = lbc_alanlar.SelectedValue.ToString();
			DialogResult dialogResult = MessageBox.Show(text + " alanını silmek istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult != DialogResult.Yes)
			{
				_ = 7;
				return;
			}
			_yaziciayarlari.alansil(text);
			DegisiklikVar = true;
			EkranBilgiGuncelle();
		}
	}

	private void cb_statik_veri_SelectedValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan && item._GetParametre("VeriTipi")._GetInt == 1)
			{
				item._GetParametre("Veri")._SetInt = (int)cb_statik_veri.SelectedValue;
			}
		}
	}

	private void cb_dinamik_veri_SelectedValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan && item._GetParametre("VeriTipi")._GetInt == 2)
			{
				item._GetParametre("Veri")._SetInt = (int)cb_dinamik_veri.SelectedValue;
			}
		}
	}

	private void se_ondalik_hane_sayisi_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("OndalikHaneSayisi")._SetInt = (int)se_ondalik_hane_sayisi.Value;
			}
		}
	}

	private void ce_sonuna_para_birimi_ekle_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("SonunaParaBirimiEkle")._SetBoolean = ce_sonuna_para_birimi_ekle.Checked;
			}
		}
	}

	private void ce_basina_para_birimi_ekle_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("BasinaParaBirimiEkle")._SetBoolean = ce_basina_para_birimi_ekle.Checked;
			}
		}
	}

	private void te_binlik_ayraci_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("BinlikAyraci")._SetString = te_binlik_ayraci.Text;
			}
		}
	}

	private void te_ondalik_ayraci_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("OndalikAyraci")._SetString = te_ondalik_ayraci.Text;
			}
		}
	}

	private void te_on_ek_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("OnEk")._SetString = te_on_ek.Text;
			}
		}
	}

	private void te_son_ek_EditValueChanged(object sender, EventArgs e)
	{
		if (_yaziciayarlari == null)
		{
			return;
		}
		foreach (Parametreler item in _yaziciayarlari.alanlar)
		{
			if (item._GetParametre("Isim")._GetString == AktifAlan)
			{
				item._GetParametre("SonEk")._SetString = te_son_ek.Text;
			}
		}
	}

	private void AltBasliklarSadeceSonSayfadaYazilsin_EditValueChanged(object sender, EventArgs e)
	{
		_yaziciayarlari.genelayarlar._GetParametre("AltBasliklarSadeceSonSayfadaYazilsin")._SetBoolean = AltBasliklarSadeceSonSayfadaYazilsin.Checked;
	}

	private void degisiklikleriKaydetToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (lb_sablonlar.SelectedValue != null)
		{
			KullaniciParametreKaydet();
		}
	}

	private void sablonSilToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (lb_sablonlar.SelectedValue == null)
		{
			return;
		}
		bool flag = true;
		if (DegisiklikVar)
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
		string text = lb_sablonlar.SelectedValue.ToString();
		if (flag)
		{
			DialogResult dialogResult = MessageBox.Show(text + " kullanıcısını silmek istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult != DialogResult.Yes)
			{
				_ = 7;
				return;
			}
			YaziciAyarlariData.YaziciAyariSil(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text);
			SablonlariListele();
		}
	}

	private void sablonEkleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				break;
			case DialogResult.Cancel:
				return;
			}
		}
		TextSor textSor = new TextSor("Şablon adı", "DEVAM");
		if (textSor.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		foreach (string item in lb_sablonlar.Items)
		{
			if (item == textSor.te_cevap.Text)
			{
				MessageBox.Show("Bu şablon adı daha önce kullanılmış. İşlem tamamlanamadı!");
				return;
			}
		}
		YaziciAyarlari yaziciAyarlari = new YaziciAyarlari(textSor.te_cevap.Text);
		yaziciAyarlari.genelayarlar._GetParametre("SablonAdi")._SetString = textSor.te_cevap.Text;
		YaziciAyarlariData.SaveYaziciAyarlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, yaziciAyarlari);
		SablonlariListele();
		lb_sablonlar.SelectedItem = textSor.te_cevap.Text;
	}

	private void dosyayaYazToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				break;
			case DialogResult.Cancel:
				return;
			}
		}
		try
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Fora parametreler|*.fpt|Tüm dosyalar|*.*";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.CreateNew);
				YaziciAyarlari.WriteToStream(_yaziciayarlari, fileStream, 1);
				fileStream.Close();
				fileStream.Dispose();
			}
		}
		catch
		{
			MessageBox.Show("Dosya oluşturulamadı!");
		}
	}

	private void dosyadanOkuToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Fora parametreler|*.fpt|Tüm dosyalar|*.*";
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		TextSor textSor = new TextSor("Şablon adı", "DEVAM");
		if (textSor.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		foreach (string item in lb_sablonlar.Items)
		{
			if (item == textSor.te_cevap.Text)
			{
				MessageBox.Show("Bu şablon adı daha önce kullanılmış. İşlem tamamlanamadı!");
				return;
			}
		}
		FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
		YaziciAyarlari yaziciAyarlari = YaziciAyarlari.ReadFromStream(fileStream);
		fileStream.Close();
		fileStream.Dispose();
		yaziciAyarlari.genelayarlar._GetParametre("SablonAdi")._SetString = textSor.te_cevap.Text;
		foreach (Parametre item2 in yaziciAyarlari.genelayarlar.ParametreListesi)
		{
			item2.EskiID = 0;
			item2.IDGuid = null;
			item2.ParametreUser = textSor.te_cevap.Text;
		}
		foreach (Parametreler item3 in yaziciAyarlari.alanlar)
		{
			foreach (Parametre item4 in item3.ParametreListesi)
			{
				item4.EskiID = 0;
				item4.IDGuid = null;
				item4.ParametreUser = textSor.te_cevap.Text;
			}
		}
		YaziciAyarlariData.SaveYaziciAyarlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, yaziciAyarlari);
		SablonlariListele();
		lb_sablonlar.SelectedItem = textSor.te_cevap.Text;
	}

	private void cb_gruplama_secenegi_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			_yaziciayarlari.genelayarlar._GetParametre("StokGruplandirmaSecenegi")._SetInt = (int)cb_gruplama_secenegi.SelectedValue;
		}
		catch
		{
		}
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
		this.lb_sablonlar = new DevExpress.XtraEditors.ListBoxControl();
		this.tc_parametreler = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage_Genel = new DevExpress.XtraTab.XtraTabPage();
		this.cb_gruplama_secenegi = new System.Windows.Forms.ComboBox();
		this.labelControl17 = new DevExpress.XtraEditors.LabelControl();
		this.AltBasliklarSadeceSonSayfadaYazilsin = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl20 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		this.DetayBirSayfadakiKayitSayisi = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.DetayBasiSatirSayisi = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.DetayBaslangicSatiri = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.SayfaDokumSayisi = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.SayfaSatirSayisi = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.SayfaKolonSayisi = new DevExpress.XtraEditors.SpinEdit();
		this.te_kullanici_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.Alanlar = new DevExpress.XtraTab.XtraTabPage();
		this.labelControl34 = new DevExpress.XtraEditors.LabelControl();
		this.te_son_ek = new DevExpress.XtraEditors.TextEdit();
		this.labelControl32 = new DevExpress.XtraEditors.LabelControl();
		this.te_on_ek = new DevExpress.XtraEditors.TextEdit();
		this.labelControl33 = new DevExpress.XtraEditors.LabelControl();
		this.te_ondalik_ayraci = new DevExpress.XtraEditors.TextEdit();
		this.labelControl27 = new DevExpress.XtraEditors.LabelControl();
		this.te_binlik_ayraci = new DevExpress.XtraEditors.TextEdit();
		this.labelControl26 = new DevExpress.XtraEditors.LabelControl();
		this.ce_basina_para_birimi_ekle = new DevExpress.XtraEditors.CheckEdit();
		this.ce_sonuna_para_birimi_ekle = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl25 = new DevExpress.XtraEditors.LabelControl();
		this.se_ondalik_hane_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl23 = new DevExpress.XtraEditors.LabelControl();
		this.cb_dinamik_veri = new System.Windows.Forms.ComboBox();
		this.labelControl22 = new DevExpress.XtraEditors.LabelControl();
		this.cb_statik_veri = new System.Windows.Forms.ComboBox();
		this.labelControl19 = new DevExpress.XtraEditors.LabelControl();
		this.sb_alan_sil = new DevExpress.XtraEditors.SimpleButton();
		this.ce_detayin_bittigi_yere_kaydir = new DevExpress.XtraEditors.CheckEdit();
		this.cb_basilacak_alan = new System.Windows.Forms.ComboBox();
		this.labelControl18 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl16 = new DevExpress.XtraEditors.LabelControl();
		this.cb_hizalama = new System.Windows.Forms.ComboBox();
		this.cb_veritipi = new System.Windows.Forms.ComboBox();
		this.labelControl15 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl14 = new DevExpress.XtraEditors.LabelControl();
		this.se_genislik = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl13 = new DevExpress.XtraEditors.LabelControl();
		this.se_satir = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
		this.se_kolon = new DevExpress.XtraEditors.SpinEdit();
		this.te_statik_metin = new DevExpress.XtraEditors.TextEdit();
		this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
		this.te_alan_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		this.te_eklenecek_alan = new DevExpress.XtraEditors.TextEdit();
		this.sb_alan_ekle = new DevExpress.XtraEditors.SimpleButton();
		this.labelControl21 = new DevExpress.XtraEditors.LabelControl();
		this.lbc_alanlar = new DevExpress.XtraEditors.ListBoxControl();
		this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
		this.labelControl28 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl29 = new DevExpress.XtraEditors.LabelControl();
		this.te_sira = new DevExpress.XtraEditors.TextEdit();
		this.labelControl30 = new DevExpress.XtraEditors.LabelControl();
		this.EvrakTipi = new System.Windows.Forms.ComboBox();
		this.labelControl31 = new DevExpress.XtraEditors.LabelControl();
		this.te_seri = new DevExpress.XtraEditors.TextEdit();
		this.OnIzleme = new DevExpress.XtraTab.XtraTabPage();
		this.labelControl24 = new DevExpress.XtraEditors.LabelControl();
		this.te_com_port = new DevExpress.XtraEditors.TextEdit();
		this.yazdir = new DevExpress.XtraEditors.SimpleButton();
		this.rtb_preview = new System.Windows.Forms.RichTextBox();
		this.label718 = new System.Windows.Forms.Label();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.degisiklikleriKaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sablonEkleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sablonSilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.dosyayaYazToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.dosyadanOkuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		((System.ComponentModel.ISupportInitialize)this.lb_sablonlar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).BeginInit();
		this.tc_parametreler.SuspendLayout();
		this.xtraTabPage_Genel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.AltBasliklarSadeceSonSayfadaYazilsin.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DetayBirSayfadakiKayitSayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DetayBasiSatirSayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DetayBaslangicSatiri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SayfaDokumSayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SayfaSatirSayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SayfaKolonSayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		this.Alanlar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.te_son_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_on_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_ondalik_ayraci.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_binlik_ayraci.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ce_basina_para_birimi_ekle.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ce_sonuna_para_birimi_ekle.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_ondalik_hane_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ce_detayin_bittigi_yere_kaydir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_genislik.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_satir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_kolon.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_statik_metin.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_alan_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_eklenecek_alan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lbc_alanlar).BeginInit();
		this.xtraTabPage1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.te_sira.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_seri.Properties).BeginInit();
		this.OnIzleme.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.te_com_port.Properties).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.lb_sablonlar.Location = new System.Drawing.Point(13, 58);
		this.lb_sablonlar.Name = "lb_sablonlar";
		this.lb_sablonlar.Size = new System.Drawing.Size(153, 495);
		this.lb_sablonlar.TabIndex = 5;
		this.lb_sablonlar.SelectedValueChanged += new System.EventHandler(lb_sablonlar_SelectedValueChanged);
		this.tc_parametreler.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tc_parametreler.Location = new System.Drawing.Point(171, 35);
		this.tc_parametreler.Name = "tc_parametreler";
		this.tc_parametreler.SelectedTabPage = this.xtraTabPage_Genel;
		this.tc_parametreler.Size = new System.Drawing.Size(760, 523);
		this.tc_parametreler.TabIndex = 0;
		this.tc_parametreler.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[4] { this.xtraTabPage_Genel, this.Alanlar, this.xtraTabPage1, this.OnIzleme });
		this.tc_parametreler.Selecting += new DevExpress.XtraTab.TabPageCancelEventHandler(tc_parametreler_Selecting);
		this.xtraTabPage_Genel.Controls.Add(this.cb_gruplama_secenegi);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl17);
		this.xtraTabPage_Genel.Controls.Add(this.AltBasliklarSadeceSonSayfadaYazilsin);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl20);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl7);
		this.xtraTabPage_Genel.Controls.Add(this.DetayBirSayfadakiKayitSayisi);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl6);
		this.xtraTabPage_Genel.Controls.Add(this.DetayBasiSatirSayisi);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl5);
		this.xtraTabPage_Genel.Controls.Add(this.DetayBaslangicSatiri);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl4);
		this.xtraTabPage_Genel.Controls.Add(this.SayfaDokumSayisi);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl3);
		this.xtraTabPage_Genel.Controls.Add(this.SayfaSatirSayisi);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl2);
		this.xtraTabPage_Genel.Controls.Add(this.SayfaKolonSayisi);
		this.xtraTabPage_Genel.Controls.Add(this.te_kullanici_adi);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl1);
		this.xtraTabPage_Genel.Name = "xtraTabPage_Genel";
		this.xtraTabPage_Genel.Size = new System.Drawing.Size(754, 495);
		this.xtraTabPage_Genel.Text = "GENEL";
		this.cb_gruplama_secenegi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_gruplama_secenegi.FormattingEnabled = true;
		this.cb_gruplama_secenegi.Location = new System.Drawing.Point(203, 271);
		this.cb_gruplama_secenegi.Name = "cb_gruplama_secenegi";
		this.cb_gruplama_secenegi.Size = new System.Drawing.Size(219, 21);
		this.cb_gruplama_secenegi.TabIndex = 122;
		this.cb_gruplama_secenegi.SelectedIndexChanged += new System.EventHandler(cb_gruplama_secenegi_SelectedIndexChanged);
		this.labelControl17.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl17.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl17.Location = new System.Drawing.Point(52, 274);
		this.labelControl17.Name = "labelControl17";
		this.labelControl17.Size = new System.Drawing.Size(145, 13);
		this.labelControl17.TabIndex = 121;
		this.labelControl17.Text = "Gruplama seçeneği :";
		this.AltBasliklarSadeceSonSayfadaYazilsin.Location = new System.Drawing.Point(203, 246);
		this.AltBasliklarSadeceSonSayfadaYazilsin.Name = "AltBasliklarSadeceSonSayfadaYazilsin";
		this.AltBasliklarSadeceSonSayfadaYazilsin.Properties.Caption = "Alt başlıklar sadece son sayfada yazılsın";
		this.AltBasliklarSadeceSonSayfadaYazilsin.Size = new System.Drawing.Size(219, 19);
		this.AltBasliklarSadeceSonSayfadaYazilsin.TabIndex = 118;
		this.AltBasliklarSadeceSonSayfadaYazilsin.EditValueChanged += new System.EventHandler(AltBasliklarSadeceSonSayfadaYazilsin_EditValueChanged);
		this.labelControl20.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl20.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl20.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl20.Location = new System.Drawing.Point(114, 39);
		this.labelControl20.Name = "labelControl20";
		this.labelControl20.Size = new System.Drawing.Size(153, 19);
		this.labelControl20.TabIndex = 109;
		this.labelControl20.Text = "Genel parametreler";
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl7.Location = new System.Drawing.Point(16, 223);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(181, 13);
		this.labelControl7.TabIndex = 106;
		this.labelControl7.Text = "Detay bir sayfadaki kayıt sayısı :";
		this.DetayBirSayfadakiKayitSayisi.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.DetayBirSayfadakiKayitSayisi.Location = new System.Drawing.Point(203, 220);
		this.DetayBirSayfadakiKayitSayisi.Name = "DetayBirSayfadakiKayitSayisi";
		this.DetayBirSayfadakiKayitSayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.DetayBirSayfadakiKayitSayisi.Properties.IsFloatValue = false;
		this.DetayBirSayfadakiKayitSayisi.Properties.Mask.EditMask = "N00";
		this.DetayBirSayfadakiKayitSayisi.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.DetayBirSayfadakiKayitSayisi.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.DetayBirSayfadakiKayitSayisi.Size = new System.Drawing.Size(54, 20);
		this.DetayBirSayfadakiKayitSayisi.TabIndex = 105;
		this.DetayBirSayfadakiKayitSayisi.EditValueChanged += new System.EventHandler(DetayBirSayfadakiKayitSayisi_EditValueChanged);
		this.DetayBirSayfadakiKayitSayisi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DetayBirSayfadakiKayitSayisi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(16, 197);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(181, 13);
		this.labelControl6.TabIndex = 104;
		this.labelControl6.Text = "Detay başı satır sayısı :";
		this.DetayBasiSatirSayisi.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.DetayBasiSatirSayisi.Location = new System.Drawing.Point(203, 194);
		this.DetayBasiSatirSayisi.Name = "DetayBasiSatirSayisi";
		this.DetayBasiSatirSayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.DetayBasiSatirSayisi.Properties.IsFloatValue = false;
		this.DetayBasiSatirSayisi.Properties.Mask.EditMask = "N00";
		this.DetayBasiSatirSayisi.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.DetayBasiSatirSayisi.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.DetayBasiSatirSayisi.Size = new System.Drawing.Size(54, 20);
		this.DetayBasiSatirSayisi.TabIndex = 103;
		this.DetayBasiSatirSayisi.EditValueChanged += new System.EventHandler(DetayBasiSatirSayisi_EditValueChanged);
		this.DetayBasiSatirSayisi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DetayBasiSatirSayisi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(16, 171);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(181, 13);
		this.labelControl5.TabIndex = 102;
		this.labelControl5.Text = "Detay başlangıç satırı :";
		this.DetayBaslangicSatiri.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.DetayBaslangicSatiri.Location = new System.Drawing.Point(203, 168);
		this.DetayBaslangicSatiri.Name = "DetayBaslangicSatiri";
		this.DetayBaslangicSatiri.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.DetayBaslangicSatiri.Properties.IsFloatValue = false;
		this.DetayBaslangicSatiri.Properties.Mask.EditMask = "N00";
		this.DetayBaslangicSatiri.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.DetayBaslangicSatiri.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.DetayBaslangicSatiri.Size = new System.Drawing.Size(54, 20);
		this.DetayBaslangicSatiri.TabIndex = 101;
		this.DetayBaslangicSatiri.EditValueChanged += new System.EventHandler(DetayBaslangicSatiri_EditValueChanged);
		this.DetayBaslangicSatiri.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DetayBaslangicSatiri.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(16, 145);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(181, 13);
		this.labelControl4.TabIndex = 100;
		this.labelControl4.Text = "Sayfa döküm sayısı :";
		this.SayfaDokumSayisi.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.SayfaDokumSayisi.Location = new System.Drawing.Point(203, 142);
		this.SayfaDokumSayisi.Name = "SayfaDokumSayisi";
		this.SayfaDokumSayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.SayfaDokumSayisi.Properties.IsFloatValue = false;
		this.SayfaDokumSayisi.Properties.Mask.EditMask = "N00";
		this.SayfaDokumSayisi.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.SayfaDokumSayisi.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.SayfaDokumSayisi.Size = new System.Drawing.Size(54, 20);
		this.SayfaDokumSayisi.TabIndex = 99;
		this.SayfaDokumSayisi.EditValueChanged += new System.EventHandler(SayfaDokumSayisi_EditValueChanged);
		this.SayfaDokumSayisi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.SayfaDokumSayisi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(16, 119);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(181, 13);
		this.labelControl3.TabIndex = 98;
		this.labelControl3.Text = "Sayfa satır sayısı :";
		this.SayfaSatirSayisi.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.SayfaSatirSayisi.Location = new System.Drawing.Point(203, 116);
		this.SayfaSatirSayisi.Name = "SayfaSatirSayisi";
		this.SayfaSatirSayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.SayfaSatirSayisi.Properties.IsFloatValue = false;
		this.SayfaSatirSayisi.Properties.Mask.EditMask = "N00";
		this.SayfaSatirSayisi.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.SayfaSatirSayisi.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.SayfaSatirSayisi.Size = new System.Drawing.Size(54, 20);
		this.SayfaSatirSayisi.TabIndex = 97;
		this.SayfaSatirSayisi.EditValueChanged += new System.EventHandler(SayfaSatirSayisi_EditValueChanged);
		this.SayfaSatirSayisi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.SayfaSatirSayisi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(16, 93);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(181, 13);
		this.labelControl2.TabIndex = 96;
		this.labelControl2.Text = "Sayfa kolon sayısı :";
		this.SayfaKolonSayisi.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.SayfaKolonSayisi.Location = new System.Drawing.Point(203, 90);
		this.SayfaKolonSayisi.Name = "SayfaKolonSayisi";
		this.SayfaKolonSayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.SayfaKolonSayisi.Properties.IsFloatValue = false;
		this.SayfaKolonSayisi.Properties.Mask.EditMask = "N00";
		this.SayfaKolonSayisi.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.SayfaKolonSayisi.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.SayfaKolonSayisi.Size = new System.Drawing.Size(54, 20);
		this.SayfaKolonSayisi.TabIndex = 95;
		this.SayfaKolonSayisi.EditValueChanged += new System.EventHandler(SayfaKolonSayisi_EditValueChanged);
		this.SayfaKolonSayisi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.SayfaKolonSayisi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.te_kullanici_adi.Enabled = false;
		this.te_kullanici_adi.Location = new System.Drawing.Point(203, 64);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.te_kullanici_adi.Properties.Appearance.Options.UseForeColor = true;
		this.te_kullanici_adi.Size = new System.Drawing.Size(211, 20);
		this.te_kullanici_adi.TabIndex = 4;
		this.te_kullanici_adi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(80, 67);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(117, 13);
		this.labelControl1.TabIndex = 3;
		this.labelControl1.Text = "Şablon adı :";
		this.Alanlar.Controls.Add(this.labelControl34);
		this.Alanlar.Controls.Add(this.te_son_ek);
		this.Alanlar.Controls.Add(this.labelControl32);
		this.Alanlar.Controls.Add(this.te_on_ek);
		this.Alanlar.Controls.Add(this.labelControl33);
		this.Alanlar.Controls.Add(this.te_ondalik_ayraci);
		this.Alanlar.Controls.Add(this.labelControl27);
		this.Alanlar.Controls.Add(this.te_binlik_ayraci);
		this.Alanlar.Controls.Add(this.labelControl26);
		this.Alanlar.Controls.Add(this.ce_basina_para_birimi_ekle);
		this.Alanlar.Controls.Add(this.ce_sonuna_para_birimi_ekle);
		this.Alanlar.Controls.Add(this.labelControl25);
		this.Alanlar.Controls.Add(this.se_ondalik_hane_sayisi);
		this.Alanlar.Controls.Add(this.labelControl23);
		this.Alanlar.Controls.Add(this.cb_dinamik_veri);
		this.Alanlar.Controls.Add(this.labelControl22);
		this.Alanlar.Controls.Add(this.cb_statik_veri);
		this.Alanlar.Controls.Add(this.labelControl19);
		this.Alanlar.Controls.Add(this.sb_alan_sil);
		this.Alanlar.Controls.Add(this.ce_detayin_bittigi_yere_kaydir);
		this.Alanlar.Controls.Add(this.cb_basilacak_alan);
		this.Alanlar.Controls.Add(this.labelControl18);
		this.Alanlar.Controls.Add(this.labelControl16);
		this.Alanlar.Controls.Add(this.cb_hizalama);
		this.Alanlar.Controls.Add(this.cb_veritipi);
		this.Alanlar.Controls.Add(this.labelControl15);
		this.Alanlar.Controls.Add(this.labelControl14);
		this.Alanlar.Controls.Add(this.se_genislik);
		this.Alanlar.Controls.Add(this.labelControl13);
		this.Alanlar.Controls.Add(this.se_satir);
		this.Alanlar.Controls.Add(this.labelControl12);
		this.Alanlar.Controls.Add(this.se_kolon);
		this.Alanlar.Controls.Add(this.te_statik_metin);
		this.Alanlar.Controls.Add(this.labelControl11);
		this.Alanlar.Controls.Add(this.labelControl9);
		this.Alanlar.Controls.Add(this.te_alan_adi);
		this.Alanlar.Controls.Add(this.labelControl10);
		this.Alanlar.Controls.Add(this.labelControl8);
		this.Alanlar.Controls.Add(this.te_eklenecek_alan);
		this.Alanlar.Controls.Add(this.sb_alan_ekle);
		this.Alanlar.Controls.Add(this.labelControl21);
		this.Alanlar.Controls.Add(this.lbc_alanlar);
		this.Alanlar.Name = "Alanlar";
		this.Alanlar.Size = new System.Drawing.Size(754, 495);
		this.Alanlar.Text = "ALANLAR";
		this.labelControl34.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl34.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl34.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl34.Location = new System.Drawing.Point(504, 282);
		this.labelControl34.Name = "labelControl34";
		this.labelControl34.Size = new System.Drawing.Size(106, 19);
		this.labelControl34.TabIndex = 136;
		this.labelControl34.Text = "Sayı alanları";
		this.te_son_ek.Location = new System.Drawing.Point(504, 256);
		this.te_son_ek.Name = "te_son_ek";
		this.te_son_ek.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.te_son_ek.Properties.Appearance.Options.UseForeColor = true;
		this.te_son_ek.Size = new System.Drawing.Size(219, 20);
		this.te_son_ek.TabIndex = 135;
		this.te_son_ek.EditValueChanged += new System.EventHandler(te_son_ek_EditValueChanged);
		this.te_son_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl32.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl32.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl32.Location = new System.Drawing.Point(422, 259);
		this.labelControl32.Name = "labelControl32";
		this.labelControl32.Size = new System.Drawing.Size(76, 13);
		this.labelControl32.TabIndex = 134;
		this.labelControl32.Text = "Son ek :";
		this.te_on_ek.Location = new System.Drawing.Point(504, 230);
		this.te_on_ek.Name = "te_on_ek";
		this.te_on_ek.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.te_on_ek.Properties.Appearance.Options.UseForeColor = true;
		this.te_on_ek.Size = new System.Drawing.Size(219, 20);
		this.te_on_ek.TabIndex = 133;
		this.te_on_ek.EditValueChanged += new System.EventHandler(te_on_ek_EditValueChanged);
		this.te_on_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl33.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl33.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl33.Location = new System.Drawing.Point(422, 233);
		this.labelControl33.Name = "labelControl33";
		this.labelControl33.Size = new System.Drawing.Size(76, 13);
		this.labelControl33.TabIndex = 132;
		this.labelControl33.Text = "Ön ek :";
		this.te_ondalik_ayraci.Location = new System.Drawing.Point(504, 411);
		this.te_ondalik_ayraci.Name = "te_ondalik_ayraci";
		this.te_ondalik_ayraci.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.te_ondalik_ayraci.Properties.Appearance.Options.UseForeColor = true;
		this.te_ondalik_ayraci.Size = new System.Drawing.Size(106, 20);
		this.te_ondalik_ayraci.TabIndex = 131;
		this.te_ondalik_ayraci.EditValueChanged += new System.EventHandler(te_ondalik_ayraci_EditValueChanged);
		this.labelControl27.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl27.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl27.Location = new System.Drawing.Point(404, 414);
		this.labelControl27.Name = "labelControl27";
		this.labelControl27.Size = new System.Drawing.Size(94, 13);
		this.labelControl27.TabIndex = 130;
		this.labelControl27.Text = "Ondalık ayracı :";
		this.te_binlik_ayraci.Location = new System.Drawing.Point(504, 385);
		this.te_binlik_ayraci.Name = "te_binlik_ayraci";
		this.te_binlik_ayraci.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.te_binlik_ayraci.Properties.Appearance.Options.UseForeColor = true;
		this.te_binlik_ayraci.Size = new System.Drawing.Size(106, 20);
		this.te_binlik_ayraci.TabIndex = 129;
		this.te_binlik_ayraci.EditValueChanged += new System.EventHandler(te_binlik_ayraci_EditValueChanged);
		this.labelControl26.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl26.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl26.Location = new System.Drawing.Point(404, 388);
		this.labelControl26.Name = "labelControl26";
		this.labelControl26.Size = new System.Drawing.Size(94, 13);
		this.labelControl26.TabIndex = 128;
		this.labelControl26.Text = "Binlik ayracı :";
		this.ce_basina_para_birimi_ekle.Location = new System.Drawing.Point(504, 360);
		this.ce_basina_para_birimi_ekle.Name = "ce_basina_para_birimi_ekle";
		this.ce_basina_para_birimi_ekle.Properties.Caption = "Başına para birimi ekle";
		this.ce_basina_para_birimi_ekle.Size = new System.Drawing.Size(136, 19);
		this.ce_basina_para_birimi_ekle.TabIndex = 127;
		this.ce_basina_para_birimi_ekle.EditValueChanged += new System.EventHandler(ce_basina_para_birimi_ekle_EditValueChanged);
		this.ce_sonuna_para_birimi_ekle.Location = new System.Drawing.Point(504, 335);
		this.ce_sonuna_para_birimi_ekle.Name = "ce_sonuna_para_birimi_ekle";
		this.ce_sonuna_para_birimi_ekle.Properties.Caption = "Sonuna para birimi ekle";
		this.ce_sonuna_para_birimi_ekle.Size = new System.Drawing.Size(136, 19);
		this.ce_sonuna_para_birimi_ekle.TabIndex = 126;
		this.ce_sonuna_para_birimi_ekle.EditValueChanged += new System.EventHandler(ce_sonuna_para_birimi_ekle_EditValueChanged);
		this.labelControl25.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl25.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl25.Location = new System.Drawing.Point(394, 312);
		this.labelControl25.Name = "labelControl25";
		this.labelControl25.Size = new System.Drawing.Size(104, 13);
		this.labelControl25.TabIndex = 125;
		this.labelControl25.Text = "Ondalık hane sayısı :";
		this.se_ondalik_hane_sayisi.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.se_ondalik_hane_sayisi.Location = new System.Drawing.Point(504, 309);
		this.se_ondalik_hane_sayisi.Name = "se_ondalik_hane_sayisi";
		this.se_ondalik_hane_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.se_ondalik_hane_sayisi.Properties.IsFloatValue = false;
		this.se_ondalik_hane_sayisi.Properties.Mask.EditMask = "N00";
		this.se_ondalik_hane_sayisi.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.se_ondalik_hane_sayisi.Size = new System.Drawing.Size(106, 20);
		this.se_ondalik_hane_sayisi.TabIndex = 124;
		this.se_ondalik_hane_sayisi.EditValueChanged += new System.EventHandler(se_ondalik_hane_sayisi_EditValueChanged);
		this.labelControl23.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl23.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl23.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl23.Location = new System.Drawing.Point(504, 96);
		this.labelControl23.Name = "labelControl23";
		this.labelControl23.Size = new System.Drawing.Size(106, 19);
		this.labelControl23.TabIndex = 123;
		this.labelControl23.Text = "Veri";
		this.cb_dinamik_veri.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_dinamik_veri.FormattingEnabled = true;
		this.cb_dinamik_veri.Location = new System.Drawing.Point(504, 177);
		this.cb_dinamik_veri.Name = "cb_dinamik_veri";
		this.cb_dinamik_veri.Size = new System.Drawing.Size(219, 21);
		this.cb_dinamik_veri.TabIndex = 122;
		this.cb_dinamik_veri.SelectedValueChanged += new System.EventHandler(cb_dinamik_veri_SelectedValueChanged);
		this.cb_dinamik_veri.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cb_dinamik_veri.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl22.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl22.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl22.Location = new System.Drawing.Point(422, 180);
		this.labelControl22.Name = "labelControl22";
		this.labelControl22.Size = new System.Drawing.Size(76, 13);
		this.labelControl22.TabIndex = 121;
		this.labelControl22.Text = "Dinamik veri :";
		this.cb_statik_veri.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_statik_veri.FormattingEnabled = true;
		this.cb_statik_veri.Location = new System.Drawing.Point(504, 150);
		this.cb_statik_veri.Name = "cb_statik_veri";
		this.cb_statik_veri.Size = new System.Drawing.Size(219, 21);
		this.cb_statik_veri.TabIndex = 120;
		this.cb_statik_veri.SelectedValueChanged += new System.EventHandler(cb_statik_veri_SelectedValueChanged);
		this.cb_statik_veri.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cb_statik_veri.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl19.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl19.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl19.Location = new System.Drawing.Point(422, 153);
		this.labelControl19.Name = "labelControl19";
		this.labelControl19.Size = new System.Drawing.Size(76, 13);
		this.labelControl19.TabIndex = 119;
		this.labelControl19.Text = "Statik veri :";
		this.sb_alan_sil.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_alan_sil.Appearance.Options.UseFont = true;
		this.sb_alan_sil.Location = new System.Drawing.Point(7, 282);
		this.sb_alan_sil.Name = "sb_alan_sil";
		this.sb_alan_sil.Size = new System.Drawing.Size(215, 23);
		this.sb_alan_sil.TabIndex = 118;
		this.sb_alan_sil.Text = "Alan Sil";
		this.sb_alan_sil.Click += new System.EventHandler(sb_alan_sil_Click);
		this.ce_detayin_bittigi_yere_kaydir.Location = new System.Drawing.Point(264, 253);
		this.ce_detayin_bittigi_yere_kaydir.Name = "ce_detayin_bittigi_yere_kaydir";
		this.ce_detayin_bittigi_yere_kaydir.Properties.Caption = "Detayın bittiği yere kaydır";
		this.ce_detayin_bittigi_yere_kaydir.Size = new System.Drawing.Size(152, 19);
		this.ce_detayin_bittigi_yere_kaydir.TabIndex = 117;
		this.ce_detayin_bittigi_yere_kaydir.EditValueChanged += new System.EventHandler(ce_detayin_bittigi_yere_kaydir_EditValueChanged);
		this.ce_detayin_bittigi_yere_kaydir.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.ce_detayin_bittigi_yere_kaydir.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cb_basilacak_alan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_basilacak_alan.FormattingEnabled = true;
		this.cb_basilacak_alan.Location = new System.Drawing.Point(310, 121);
		this.cb_basilacak_alan.Name = "cb_basilacak_alan";
		this.cb_basilacak_alan.Size = new System.Drawing.Size(106, 21);
		this.cb_basilacak_alan.TabIndex = 116;
		this.cb_basilacak_alan.SelectedValueChanged += new System.EventHandler(cb_basilacak_alan_SelectedValueChanged);
		this.cb_basilacak_alan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cb_basilacak_alan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl18.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl18.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl18.Location = new System.Drawing.Point(228, 124);
		this.labelControl18.Name = "labelControl18";
		this.labelControl18.Size = new System.Drawing.Size(76, 13);
		this.labelControl18.TabIndex = 115;
		this.labelControl18.Text = "Basılacak alan :";
		this.labelControl16.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl16.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl16.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl16.Location = new System.Drawing.Point(310, 96);
		this.labelControl16.Name = "labelControl16";
		this.labelControl16.Size = new System.Drawing.Size(106, 19);
		this.labelControl16.TabIndex = 114;
		this.labelControl16.Text = "Yerleşim";
		this.cb_hizalama.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_hizalama.FormattingEnabled = true;
		this.cb_hizalama.Location = new System.Drawing.Point(310, 226);
		this.cb_hizalama.Name = "cb_hizalama";
		this.cb_hizalama.Size = new System.Drawing.Size(106, 21);
		this.cb_hizalama.TabIndex = 113;
		this.cb_hizalama.SelectedValueChanged += new System.EventHandler(cb_hizalama_SelectedValueChanged);
		this.cb_hizalama.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cb_hizalama.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cb_veritipi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_veritipi.FormattingEnabled = true;
		this.cb_veritipi.Location = new System.Drawing.Point(504, 121);
		this.cb_veritipi.Name = "cb_veritipi";
		this.cb_veritipi.Size = new System.Drawing.Size(106, 21);
		this.cb_veritipi.TabIndex = 112;
		this.cb_veritipi.SelectedValueChanged += new System.EventHandler(cb_veritipi_SelectedValueChanged);
		this.cb_veritipi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cb_veritipi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl15.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl15.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl15.Location = new System.Drawing.Point(228, 229);
		this.labelControl15.Name = "labelControl15";
		this.labelControl15.Size = new System.Drawing.Size(76, 13);
		this.labelControl15.TabIndex = 111;
		this.labelControl15.Text = "Hizalama :";
		this.labelControl14.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl14.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl14.Location = new System.Drawing.Point(228, 203);
		this.labelControl14.Name = "labelControl14";
		this.labelControl14.Size = new System.Drawing.Size(76, 13);
		this.labelControl14.TabIndex = 109;
		this.labelControl14.Text = "Genişlik :";
		this.se_genislik.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.se_genislik.Location = new System.Drawing.Point(310, 200);
		this.se_genislik.Name = "se_genislik";
		this.se_genislik.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.se_genislik.Properties.IsFloatValue = false;
		this.se_genislik.Properties.Mask.EditMask = "N00";
		this.se_genislik.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.se_genislik.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.se_genislik.Size = new System.Drawing.Size(106, 20);
		this.se_genislik.TabIndex = 108;
		this.se_genislik.EditValueChanged += new System.EventHandler(se_genislik_EditValueChanged);
		this.se_genislik.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.se_genislik.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl13.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl13.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl13.Location = new System.Drawing.Point(228, 177);
		this.labelControl13.Name = "labelControl13";
		this.labelControl13.Size = new System.Drawing.Size(76, 13);
		this.labelControl13.TabIndex = 107;
		this.labelControl13.Text = "Satır :";
		this.se_satir.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.se_satir.Location = new System.Drawing.Point(310, 174);
		this.se_satir.Name = "se_satir";
		this.se_satir.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.se_satir.Properties.IsFloatValue = false;
		this.se_satir.Properties.Mask.EditMask = "N00";
		this.se_satir.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.se_satir.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.se_satir.Size = new System.Drawing.Size(106, 20);
		this.se_satir.TabIndex = 106;
		this.se_satir.EditValueChanged += new System.EventHandler(se_satir_EditValueChanged);
		this.se_satir.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.se_satir.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl12.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl12.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl12.Location = new System.Drawing.Point(228, 151);
		this.labelControl12.Name = "labelControl12";
		this.labelControl12.Size = new System.Drawing.Size(76, 13);
		this.labelControl12.TabIndex = 105;
		this.labelControl12.Text = "Kolon :";
		this.se_kolon.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.se_kolon.Location = new System.Drawing.Point(310, 148);
		this.se_kolon.Name = "se_kolon";
		this.se_kolon.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.se_kolon.Properties.IsFloatValue = false;
		this.se_kolon.Properties.Mask.EditMask = "N00";
		this.se_kolon.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.se_kolon.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.se_kolon.Size = new System.Drawing.Size(106, 20);
		this.se_kolon.TabIndex = 104;
		this.se_kolon.EditValueChanged += new System.EventHandler(se_kolon_EditValueChanged);
		this.se_kolon.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.se_kolon.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.te_statik_metin.Enabled = false;
		this.te_statik_metin.Location = new System.Drawing.Point(504, 204);
		this.te_statik_metin.Name = "te_statik_metin";
		this.te_statik_metin.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.te_statik_metin.Properties.Appearance.Options.UseForeColor = true;
		this.te_statik_metin.Size = new System.Drawing.Size(219, 20);
		this.te_statik_metin.TabIndex = 103;
		this.te_statik_metin.EditValueChanged += new System.EventHandler(te_veri_EditValueChanged);
		this.te_statik_metin.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.te_statik_metin.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl11.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl11.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl11.Location = new System.Drawing.Point(422, 207);
		this.labelControl11.Name = "labelControl11";
		this.labelControl11.Size = new System.Drawing.Size(76, 13);
		this.labelControl11.TabIndex = 102;
		this.labelControl11.Text = "Statik metin :";
		this.labelControl9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl9.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl9.Location = new System.Drawing.Point(422, 124);
		this.labelControl9.Name = "labelControl9";
		this.labelControl9.Size = new System.Drawing.Size(76, 13);
		this.labelControl9.TabIndex = 100;
		this.labelControl9.Text = "Veri tipi :";
		this.te_alan_adi.Enabled = false;
		this.te_alan_adi.Location = new System.Drawing.Point(310, 54);
		this.te_alan_adi.Name = "te_alan_adi";
		this.te_alan_adi.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.te_alan_adi.Properties.Appearance.Options.UseForeColor = true;
		this.te_alan_adi.Size = new System.Drawing.Size(300, 20);
		this.te_alan_adi.TabIndex = 98;
		this.te_alan_adi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl10.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl10.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl10.Location = new System.Drawing.Point(228, 57);
		this.labelControl10.Name = "labelControl10";
		this.labelControl10.Size = new System.Drawing.Size(76, 13);
		this.labelControl10.TabIndex = 97;
		this.labelControl10.Text = "Alan adı :";
		this.labelControl8.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl8.Location = new System.Drawing.Point(7, 321);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(215, 19);
		this.labelControl8.TabIndex = 95;
		this.labelControl8.Text = "Eklenecek alan adı";
		this.te_eklenecek_alan.Location = new System.Drawing.Point(7, 346);
		this.te_eklenecek_alan.Name = "te_eklenecek_alan";
		this.te_eklenecek_alan.Size = new System.Drawing.Size(215, 20);
		this.te_eklenecek_alan.TabIndex = 94;
		this.sb_alan_ekle.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_alan_ekle.Appearance.Options.UseFont = true;
		this.sb_alan_ekle.Location = new System.Drawing.Point(7, 372);
		this.sb_alan_ekle.Name = "sb_alan_ekle";
		this.sb_alan_ekle.Size = new System.Drawing.Size(215, 23);
		this.sb_alan_ekle.TabIndex = 93;
		this.sb_alan_ekle.Text = "Alan Ekle";
		this.sb_alan_ekle.Click += new System.EventHandler(sb_alan_ekle_Click);
		this.labelControl21.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl21.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl21.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl21.Location = new System.Drawing.Point(7, 30);
		this.labelControl21.Name = "labelControl21";
		this.labelControl21.Size = new System.Drawing.Size(215, 19);
		this.labelControl21.TabIndex = 92;
		this.labelControl21.Text = "Alanlar";
		this.lbc_alanlar.Location = new System.Drawing.Point(7, 55);
		this.lbc_alanlar.Name = "lbc_alanlar";
		this.lbc_alanlar.Size = new System.Drawing.Size(215, 221);
		this.lbc_alanlar.TabIndex = 91;
		this.lbc_alanlar.SelectedValueChanged += new System.EventHandler(lbc_alanlar_SelectedValueChanged);
		this.xtraTabPage1.Controls.Add(this.labelControl28);
		this.xtraTabPage1.Controls.Add(this.labelControl29);
		this.xtraTabPage1.Controls.Add(this.te_sira);
		this.xtraTabPage1.Controls.Add(this.labelControl30);
		this.xtraTabPage1.Controls.Add(this.EvrakTipi);
		this.xtraTabPage1.Controls.Add(this.labelControl31);
		this.xtraTabPage1.Controls.Add(this.te_seri);
		this.xtraTabPage1.Name = "xtraTabPage1";
		this.xtraTabPage1.Size = new System.Drawing.Size(754, 495);
		this.xtraTabPage1.Text = "ÖN İZLEME BİLGİLERİ";
		this.labelControl28.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl28.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl28.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl28.Location = new System.Drawing.Point(219, 30);
		this.labelControl28.Name = "labelControl28";
		this.labelControl28.Size = new System.Drawing.Size(173, 19);
		this.labelControl28.TabIndex = 131;
		this.labelControl28.Text = "ÖRNEK DOSYA BİLGİLERİ";
		this.labelControl29.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl29.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl29.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl29.Location = new System.Drawing.Point(219, 109);
		this.labelControl29.Name = "labelControl29";
		this.labelControl29.Size = new System.Drawing.Size(73, 19);
		this.labelControl29.TabIndex = 130;
		this.labelControl29.Text = "Evrak sıra : ";
		this.te_sira.EditValue = "1";
		this.te_sira.Location = new System.Drawing.Point(298, 109);
		this.te_sira.Name = "te_sira";
		this.te_sira.Size = new System.Drawing.Size(95, 20);
		this.te_sira.TabIndex = 127;
		this.labelControl30.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl30.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl30.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl30.Location = new System.Drawing.Point(219, 83);
		this.labelControl30.Name = "labelControl30";
		this.labelControl30.Size = new System.Drawing.Size(73, 19);
		this.labelControl30.TabIndex = 129;
		this.labelControl30.Text = "Evrak seri : ";
		this.EvrakTipi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.EvrakTipi.FormattingEnabled = true;
		this.EvrakTipi.Items.AddRange(new object[10] { "Satış faturası", "Satış irsaliyesi", "Alınan sipariş", "Proforma sipariş", "Tahsilat makbuzu", "Depolar arası sevk", "Depolar arası nakliye", "Depolar arası sipariş", "Alış faturası", "Alış irsaliyesi" });
		this.EvrakTipi.Location = new System.Drawing.Point(298, 55);
		this.EvrakTipi.Name = "EvrakTipi";
		this.EvrakTipi.Size = new System.Drawing.Size(172, 21);
		this.EvrakTipi.TabIndex = 125;
		this.labelControl31.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl31.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl31.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl31.Location = new System.Drawing.Point(219, 55);
		this.labelControl31.Name = "labelControl31";
		this.labelControl31.Size = new System.Drawing.Size(73, 19);
		this.labelControl31.TabIndex = 128;
		this.labelControl31.Text = "Evrak tipi : ";
		this.te_seri.EditValue = "";
		this.te_seri.Location = new System.Drawing.Point(298, 83);
		this.te_seri.Name = "te_seri";
		this.te_seri.Size = new System.Drawing.Size(95, 20);
		this.te_seri.TabIndex = 126;
		this.OnIzleme.Controls.Add(this.labelControl24);
		this.OnIzleme.Controls.Add(this.te_com_port);
		this.OnIzleme.Controls.Add(this.yazdir);
		this.OnIzleme.Controls.Add(this.rtb_preview);
		this.OnIzleme.Name = "OnIzleme";
		this.OnIzleme.Size = new System.Drawing.Size(754, 495);
		this.OnIzleme.Text = "ÖN İZLEME";
		this.labelControl24.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl24.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl24.Location = new System.Drawing.Point(53, 14);
		this.labelControl24.Name = "labelControl24";
		this.labelControl24.Size = new System.Drawing.Size(181, 13);
		this.labelControl24.TabIndex = 110;
		this.labelControl24.Text = "Com port :";
		this.te_com_port.EditValue = "COM41";
		this.te_com_port.Location = new System.Drawing.Point(240, 11);
		this.te_com_port.Name = "te_com_port";
		this.te_com_port.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.te_com_port.Properties.Appearance.Options.UseForeColor = true;
		this.te_com_port.Size = new System.Drawing.Size(72, 20);
		this.te_com_port.TabIndex = 109;
		this.yazdir.Location = new System.Drawing.Point(318, 8);
		this.yazdir.Name = "yazdir";
		this.yazdir.Size = new System.Drawing.Size(75, 23);
		this.yazdir.TabIndex = 4;
		this.yazdir.Text = "Yazdır";
		this.yazdir.Click += new System.EventHandler(yazdir_Click);
		this.rtb_preview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.rtb_preview.Font = new System.Drawing.Font("Courier New", 8.25f);
		this.rtb_preview.Location = new System.Drawing.Point(3, 37);
		this.rtb_preview.Name = "rtb_preview";
		this.rtb_preview.Size = new System.Drawing.Size(734, 412);
		this.rtb_preview.TabIndex = 3;
		this.rtb_preview.Text = "";
		this.label718.AutoSize = true;
		this.label718.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.label718.Location = new System.Drawing.Point(12, 35);
		this.label718.Name = "label718";
		this.label718.Size = new System.Drawing.Size(101, 14);
		this.label718.TabIndex = 104;
		this.label718.Text = "Yazıcı şablonları";
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.dosyaToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(934, 24);
		this.menuStrip1.TabIndex = 105;
		this.menuStrip1.Text = "menuStrip1";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.degisiklikleriKaydetToolStripMenuItem, this.sablonEkleToolStripMenuItem, this.sablonSilToolStripMenuItem, this.toolStripSeparator1, this.dosyayaYazToolStripMenuItem, this.dosyadanOkuToolStripMenuItem });
		this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
		this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
		this.dosyaToolStripMenuItem.Text = "Dosya";
		this.degisiklikleriKaydetToolStripMenuItem.Name = "degisiklikleriKaydetToolStripMenuItem";
		this.degisiklikleriKaydetToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.degisiklikleriKaydetToolStripMenuItem.Text = "Kaydet";
		this.degisiklikleriKaydetToolStripMenuItem.Click += new System.EventHandler(degisiklikleriKaydetToolStripMenuItem_Click);
		this.sablonEkleToolStripMenuItem.Name = "sablonEkleToolStripMenuItem";
		this.sablonEkleToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.sablonEkleToolStripMenuItem.Text = "Ekle";
		this.sablonEkleToolStripMenuItem.Click += new System.EventHandler(sablonEkleToolStripMenuItem_Click);
		this.sablonSilToolStripMenuItem.Name = "sablonSilToolStripMenuItem";
		this.sablonSilToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.sablonSilToolStripMenuItem.Text = "Sil";
		this.sablonSilToolStripMenuItem.Click += new System.EventHandler(sablonSilToolStripMenuItem_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(146, 6);
		this.dosyayaYazToolStripMenuItem.Name = "dosyayaYazToolStripMenuItem";
		this.dosyayaYazToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.dosyayaYazToolStripMenuItem.Text = "Dosyaya yaz";
		this.dosyayaYazToolStripMenuItem.Click += new System.EventHandler(dosyayaYazToolStripMenuItem_Click);
		this.dosyadanOkuToolStripMenuItem.Name = "dosyadanOkuToolStripMenuItem";
		this.dosyadanOkuToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.dosyadanOkuToolStripMenuItem.Text = "Dosyadan oku";
		this.dosyadanOkuToolStripMenuItem.Click += new System.EventHandler(dosyadanOkuToolStripMenuItem_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(934, 562);
		base.Controls.Add(this.label718);
		base.Controls.Add(this.tc_parametreler);
		base.Controls.Add(this.lb_sablonlar);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "YaziciAyarlariForm";
		this.Text = "Yazıcı Ayarları";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		((System.ComponentModel.ISupportInitialize)this.lb_sablonlar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).EndInit();
		this.tc_parametreler.ResumeLayout(false);
		this.xtraTabPage_Genel.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.AltBasliklarSadeceSonSayfadaYazilsin.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DetayBirSayfadakiKayitSayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DetayBasiSatirSayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DetayBaslangicSatiri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SayfaDokumSayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SayfaSatirSayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SayfaKolonSayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		this.Alanlar.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.te_son_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_on_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_ondalik_ayraci.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_binlik_ayraci.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ce_basina_para_birimi_ekle.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ce_sonuna_para_birimi_ekle.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_ondalik_hane_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ce_detayin_bittigi_yere_kaydir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_genislik.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_satir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_kolon.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_statik_metin.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_alan_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_eklenecek_alan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lbc_alanlar).EndInit();
		this.xtraTabPage1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.te_sira.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_seri.Properties).EndInit();
		this.OnIzleme.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.te_com_port.Properties).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
