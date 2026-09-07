using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using Fora.App.Win.Mikro.GenelFormlar;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Rapor.Genel;
using Fora.Mikro.Rapor.StokSiparis;
using Newtonsoft.Json;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class MobilRaporStokSiparisDuzenleme : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _raporparametreleri = new Parametreler();

	private bool DegisiklikVar;

	private string AktifKullanici;

	private RaporStokSiparisSecenekleri AktifRapor;

	private IContainer components;

	private XtraTabControl tc_parametrevehaklar;

	private XtraTabPage xtraTabPage2;

	private ListBoxControl lb_kullanicilar;

	private XtraTabPage xtraTabPage15;

	private LabelControl labelControl57;

	private System.Windows.Forms.ComboBox tarih_cinsi;

	private CheckEdit degistirebilir_tarih_cinsi;

	private CheckEdit degistirebilir_teslim_durumu;

	private LabelControl labelControl105;

	private TextEdit stok_arama_metin;

	private System.Windows.Forms.ComboBox stok_arama_secenekleri;

	private LabelControl labelControl12;

	private LabelControl labelControl9;

	private TextEdit stok_ana_gruplari;

	private System.Windows.Forms.ComboBox stok_ana_gruplari_secenek;

	private LabelControl labelControl15;

	private LabelControl labelControl16;

	private TextEdit stok_uretici_kodlari;

	private System.Windows.Forms.ComboBox stok_uretici_kodu_secenek;

	private LabelControl labelControl20;

	private LabelControl labelControl22;

	private TextEdit stok_marka_kodlari;

	private System.Windows.Forms.ComboBox stok_marka_kodu_secenek;

	private LabelControl labelControl25;

	private LabelControl labelControl26;

	private TextEdit stok_reyon_kodlari;

	private System.Windows.Forms.ComboBox stok_reyon_kodu_secenek;

	private LabelControl labelControl29;

	private LabelControl labelControl30;

	private TextEdit stok_kategori_kodlari;

	private System.Windows.Forms.ComboBox stok_kategori_kodu_secenek;

	private LabelControl labelControl33;

	private LabelControl labelControl3;

	private TextEdit RaporAdi;

	private LabelControl labelControl2;

	private TextEdit te_kullanici_adi;

	private LabelControl labelControl1;

	private CheckEdit degistirebilir_siralama_secenegi;

	private LabelControl labelControl111;

	private System.Windows.Forms.ComboBox siralama_secenegi;

	private CheckEdit degistirebilir_gruplandirma_secenegi;

	private LabelControl labelControl90;

	private System.Windows.Forms.ComboBox gruplandirma_secenegi;

	private System.Windows.Forms.ComboBox teslim_durumu;

	private CheckEdit degistirebilir_miktar3_secenek;

	private System.Windows.Forms.ComboBox miktar3_secenek;

	private CheckEdit degistirebilir_miktar2_secenek;

	private System.Windows.Forms.ComboBox miktar2_secenek;

	private CheckEdit degistirebilir_miktar1_secenek;

	private System.Windows.Forms.ComboBox miktar1_secenek;

	private CheckEdit degistirebilir_tutar3_secenek;

	private System.Windows.Forms.ComboBox tutar3_secenek;

	private CheckEdit degistirebilir_tutar2_secenek;

	private System.Windows.Forms.ComboBox tutar2_secenek;

	private CheckEdit degistirebilir_tutar1_secenek;

	private System.Windows.Forms.ComboBox tutar1_secenek;

	private LabelControl labelControl107;

	private TextEdit sorumluluk_merkezleri;

	private System.Windows.Forms.ComboBox sorumluluk_merkezleri_secenek;

	private LabelControl labelControl110;

	private LabelControl labelControl96;

	private TextEdit depolar;

	private System.Windows.Forms.ComboBox depolar_secenek;

	private LabelControl labelControl93;

	private LabelControl labelControl97;

	private TextEdit proje_kodlari;

	private System.Windows.Forms.ComboBox proje_kodlari_secenek;

	private LabelControl labelControl106;

	private LabelControl labelControl51;

	private TextEdit cari_temsilci_kodlari;

	private System.Windows.Forms.ComboBox cari_temsilci_kodu_secenek;

	private LabelControl labelControl54;

	private LabelControl labelControl47;

	private TextEdit cari_grup_kodlari;

	private System.Windows.Forms.ComboBox cari_grup_kodu_secenek;

	private LabelControl labelControl50;

	private LabelControl labelControl34;

	private TextEdit cari_bolge_kodlari;

	private System.Windows.Forms.ComboBox cari_bolge_kodu_secenek;

	private LabelControl labelControl43;

	private TextEdit cari_arama_metin;

	private System.Windows.Forms.ComboBox cari_arama_secenekleri;

	private LabelControl labelControl46;

	private LabelControl labelControl10;

	private LabelControl labelControl8;

	private LabelControl labelControl7;

	private LabelControl labelControl6;

	private LabelControl labelControl5;

	private LabelControl labelControl4;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem kaydetToolStripMenuItem;

	private ToolStripMenuItem ekleToolStripMenuItem;

	private ToolStripMenuItem silToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem dosyayaYazToolStripMenuItem;

	private ToolStripMenuItem dosyadanOkuToolStripMenuItem;

	private Label label718;

	public MobilRaporStokSiparisDuzenleme(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
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
				new object[2] { 0, "Özel tarih" },
				new object[2] { 1, "Bugün" },
				new object[2] { 2, "Dün" },
				new object[2] { 3, "Son 24 Saat" },
				new object[2] { 4, "Bu Hafta" },
				new object[2] { 5, "Geçen Hafta" },
				new object[2] { 6, "Son 7 Gün" },
				new object[2] { 7, "Bu Ay" },
				new object[2] { 8, "Geçen Ay" },
				new object[2] { 9, "Son 30 Gün" },
				new object[2] { 10, "Bu Yıl" },
				new object[2] { 11, "Geçen Yıl" },
				new object[2] { 12, "Son 365 Gün" },
				new object[2] { 13, "Son 3 Ay" },
				new object[2] { 14, "Son 60 Gün" },
				new object[2] { 15, "Son 90 Gün" },
				new object[2] { 16, "Son 6 Ay" },
				new object[2] { 17, "Son 12 Ay" }
			}
		};
		tarih_cinsi.DataSource = dataSource;
		tarih_cinsi.ValueMember = "ID";
		tarih_cinsi.DisplayMember = "Isim";
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		depolar_secenek.DataSource = dataSource2;
		depolar_secenek.ValueMember = "ID";
		depolar_secenek.DisplayMember = "Isim";
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		proje_kodlari_secenek.DataSource = dataSource3;
		proje_kodlari_secenek.ValueMember = "ID";
		proje_kodlari_secenek.DisplayMember = "Isim";
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		sorumluluk_merkezleri_secenek.DataSource = dataSource4;
		sorumluluk_merkezleri_secenek.ValueMember = "ID";
		sorumluluk_merkezleri_secenek.DisplayMember = "Isim";
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		stok_ana_gruplari_secenek.DataSource = dataSource5;
		stok_ana_gruplari_secenek.ValueMember = "ID";
		stok_ana_gruplari_secenek.DisplayMember = "Isim";
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		stok_uretici_kodu_secenek.DataSource = dataSource6;
		stok_uretici_kodu_secenek.ValueMember = "ID";
		stok_uretici_kodu_secenek.DisplayMember = "Isim";
		DataTable dataSource7 = new DataTable
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		stok_marka_kodu_secenek.DataSource = dataSource7;
		stok_marka_kodu_secenek.ValueMember = "ID";
		stok_marka_kodu_secenek.DisplayMember = "Isim";
		DataTable dataSource8 = new DataTable
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		stok_reyon_kodu_secenek.DataSource = dataSource8;
		stok_reyon_kodu_secenek.ValueMember = "ID";
		stok_reyon_kodu_secenek.DisplayMember = "Isim";
		DataTable dataSource9 = new DataTable
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		stok_kategori_kodu_secenek.DataSource = dataSource9;
		stok_kategori_kodu_secenek.ValueMember = "ID";
		stok_kategori_kodu_secenek.DisplayMember = "Isim";
		DataTable dataSource10 = new DataTable
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		cari_bolge_kodu_secenek.DataSource = dataSource10;
		cari_bolge_kodu_secenek.ValueMember = "ID";
		cari_bolge_kodu_secenek.DisplayMember = "Isim";
		DataTable dataSource11 = new DataTable
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		cari_grup_kodu_secenek.DataSource = dataSource11;
		cari_grup_kodu_secenek.ValueMember = "ID";
		cari_grup_kodu_secenek.DisplayMember = "Isim";
		DataTable dataSource12 = new DataTable
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
				new object[2] { 0, "Tümü" },
				new object[2] { 1, "Tanımlı olan" },
				new object[2] { 2, "Listeden seç" }
			}
		};
		cari_temsilci_kodu_secenek.DataSource = dataSource12;
		cari_temsilci_kodu_secenek.ValueMember = "ID";
		cari_temsilci_kodu_secenek.DisplayMember = "Isim";
		DataTable dataSource13 = new DataTable
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
				new object[2] { 0, "Stok" },
				new object[2] { 1, "Cari" },
				new object[2] { 2, "Temsilci" },
				new object[2] { 3, "Ay" },
				new object[2] { 4, "Gun" },
				new object[2] { 5, "Proje" },
				new object[2] { 6, "Sorumluluk merkezi" },
				new object[2] { 7, "Stok ana grubu" },
				new object[2] { 8, "Stok üretici" },
				new object[2] { 9, "Stok marka" },
				new object[2] { 10, "Stok reyon" },
				new object[2] { 11, "Stok kategori" },
				new object[2] { 12, "Cari bölge" },
				new object[2] { 13, "Cari grup" },
				new object[2] { 14, "Depo" }
			}
		};
		gruplandirma_secenegi.DataSource = dataSource13;
		gruplandirma_secenegi.ValueMember = "ID";
		gruplandirma_secenegi.DisplayMember = "Isim";
		DataTable dataSource14 = new DataTable
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
				new object[2] { 0, "Tüm cariler" },
				new object[2] { 1, "Eşittir" },
				new object[2] { 2, "İle başlar" },
				new object[2] { 3, "İçerir" },
				new object[2] { 4, "Tanımlı olan" }
			}
		};
		cari_arama_secenekleri.DataSource = dataSource14;
		cari_arama_secenekleri.ValueMember = "ID";
		cari_arama_secenekleri.DisplayMember = "Isim";
		DataTable dataSource15 = new DataTable
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
				new object[2] { 0, "Tüm stoklar" },
				new object[2] { 1, "Eşittir" },
				new object[2] { 2, "İle başlar" },
				new object[2] { 3, "İçerir" },
				new object[2] { 4, "Tanımlı olan" }
			}
		};
		stok_arama_secenekleri.DataSource = dataSource15;
		stok_arama_secenekleri.ValueMember = "ID";
		stok_arama_secenekleri.DisplayMember = "Isim";
		DataTable dataSource16 = new DataTable
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
				new object[2] { 0, "Kod" },
				new object[2] { 1, "İsim" },
				new object[2] { 2, "Tutar 1" },
				new object[2] { 3, "Tutar 2" },
				new object[2] { 4, "Tutar 3" },
				new object[2] { 5, "Miktar 1" },
				new object[2] { 6, "Miktar 2" },
				new object[2] { 7, "Miktar 3" }
			}
		};
		siralama_secenegi.DataSource = dataSource16;
		siralama_secenegi.ValueMember = "ID";
		siralama_secenegi.DisplayMember = "Isim";
		DataTable dataSource17 = new DataTable
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
				new object[2] { 0, "Gösterme" },
				new object[2] { 1, "Sipariş brüt tutarı" },
				new object[2] { 2, "Sipariş iskonto tutarı" },
				new object[2] { 3, "Sipariş net tutar" },
				new object[2] { 4, "Sipariş net tutar (Kdv dahil)" },
				new object[2] { 5, "Teslim edilen brüt tutarı" },
				new object[2] { 6, "Teslim edilen iskonto tutarı" },
				new object[2] { 7, "Teslim edilen net tutar" },
				new object[2] { 8, "Teslim edilen net tutar (Kdv dahil)" },
				new object[2] { 9, "Bekleyen brüt tutarı" },
				new object[2] { 10, "Bekleyen iskonto tutarı" },
				new object[2] { 11, "Bekleyen net tutar" },
				new object[2] { 12, "Bekleyen net tutar (Kdv dahil)" },
				new object[2] { 13, "Vazgeçilen brüt tutarı" },
				new object[2] { 14, "Vazgeçilen iskonto tutarı" },
				new object[2] { 15, "Vazgeçilen net tutar" },
				new object[2] { 16, "Vazgeçilen net tutar (Kdv dahil)" }
			}
		};
		tutar1_secenek.DataSource = dataSource17;
		tutar1_secenek.ValueMember = "ID";
		tutar1_secenek.DisplayMember = "Isim";
		DataTable dataSource18 = new DataTable
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
				new object[2] { 0, "Gösterme" },
				new object[2] { 1, "Sipariş brüt tutarı" },
				new object[2] { 2, "Sipariş iskonto tutarı" },
				new object[2] { 3, "Sipariş net tutar" },
				new object[2] { 4, "Sipariş net tutar (Kdv dahil)" },
				new object[2] { 5, "Teslim edilen brüt tutarı" },
				new object[2] { 6, "Teslim edilen iskonto tutarı" },
				new object[2] { 7, "Teslim edilen net tutar" },
				new object[2] { 8, "Teslim edilen net tutar (Kdv dahil)" },
				new object[2] { 9, "Bekleyen brüt tutarı" },
				new object[2] { 10, "Bekleyen iskonto tutarı" },
				new object[2] { 11, "Bekleyen net tutar" },
				new object[2] { 12, "Bekleyen net tutar (Kdv dahil)" },
				new object[2] { 13, "Vazgeçilen brüt tutarı" },
				new object[2] { 14, "Vazgeçilen iskonto tutarı" },
				new object[2] { 15, "Vazgeçilen net tutar" },
				new object[2] { 16, "Vazgeçilen net tutar (Kdv dahil)" }
			}
		};
		tutar2_secenek.DataSource = dataSource18;
		tutar2_secenek.ValueMember = "ID";
		tutar2_secenek.DisplayMember = "Isim";
		DataTable dataSource19 = new DataTable
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
				new object[2] { 0, "Gösterme" },
				new object[2] { 1, "Sipariş brüt tutarı" },
				new object[2] { 2, "Sipariş iskonto tutarı" },
				new object[2] { 3, "Sipariş net tutar" },
				new object[2] { 4, "Sipariş net tutar (Kdv dahil)" },
				new object[2] { 5, "Teslim edilen brüt tutarı" },
				new object[2] { 6, "Teslim edilen iskonto tutarı" },
				new object[2] { 7, "Teslim edilen net tutar" },
				new object[2] { 8, "Teslim edilen net tutar (Kdv dahil)" },
				new object[2] { 9, "Bekleyen brüt tutarı" },
				new object[2] { 10, "Bekleyen iskonto tutarı" },
				new object[2] { 11, "Bekleyen net tutar" },
				new object[2] { 12, "Bekleyen net tutar (Kdv dahil)" },
				new object[2] { 13, "Vazgeçilen brüt tutarı" },
				new object[2] { 14, "Vazgeçilen iskonto tutarı" },
				new object[2] { 15, "Vazgeçilen net tutar" },
				new object[2] { 16, "Vazgeçilen net tutar (Kdv dahil)" }
			}
		};
		tutar3_secenek.DataSource = dataSource19;
		tutar3_secenek.ValueMember = "ID";
		tutar3_secenek.DisplayMember = "Isim";
		DataTable dataSource20 = new DataTable
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
				new object[2] { 0, "Gösterme" },
				new object[2] { 1, "Sipariş miktarı" },
				new object[2] { 2, "Teslim edilen miktar" },
				new object[2] { 3, "Bekleyen miktar" },
				new object[2] { 4, "Vazgeçilen miktar" }
			}
		};
		miktar1_secenek.DataSource = dataSource20;
		miktar1_secenek.ValueMember = "ID";
		miktar1_secenek.DisplayMember = "Isim";
		DataTable dataSource21 = new DataTable
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
				new object[2] { 0, "Gösterme" },
				new object[2] { 1, "Sipariş miktarı" },
				new object[2] { 2, "Teslim edilen miktar" },
				new object[2] { 3, "Bekleyen miktar" },
				new object[2] { 4, "Vazgeçilen miktar" }
			}
		};
		miktar2_secenek.DataSource = dataSource21;
		miktar2_secenek.ValueMember = "ID";
		miktar2_secenek.DisplayMember = "Isim";
		DataTable dataSource22 = new DataTable
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
				new object[2] { 0, "Gösterme" },
				new object[2] { 1, "Sipariş miktarı" },
				new object[2] { 2, "Teslim edilen miktar" },
				new object[2] { 3, "Bekleyen miktar" },
				new object[2] { 4, "Vazgeçilen miktar" }
			}
		};
		miktar3_secenek.DataSource = dataSource22;
		miktar3_secenek.ValueMember = "ID";
		miktar3_secenek.DisplayMember = "Isim";
		DataTable dataSource23 = new DataTable
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
				new object[2] { 0, "Hepsi" },
				new object[2] { 1, "Tamamlananlar" },
				new object[2] { 2, "Bekleyenler" },
				new object[2] { 3, "Vazgeçilenler" }
			}
		};
		teslim_durumu.DataSource = dataSource23;
		teslim_durumu.ValueMember = "ID";
		teslim_durumu.DisplayMember = "Isim";
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		depolar.Enabled = false;
		proje_kodlari.Enabled = false;
		sorumluluk_merkezleri.Enabled = false;
		stok_ana_gruplari.Enabled = false;
		stok_uretici_kodlari.Enabled = false;
		stok_marka_kodlari.Enabled = false;
		stok_reyon_kodlari.Enabled = false;
		stok_kategori_kodlari.Enabled = false;
		cari_bolge_kodlari.Enabled = false;
		cari_grup_kodlari.Enabled = false;
		cari_temsilci_kodlari.Enabled = false;
		stok_arama_metin.Enabled = false;
		cari_arama_metin.Enabled = false;
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		KullanicilariListele();
	}

	private void KullanicilariListele()
	{
		lb_kullanicilar.Items.Clear();
		List<string> list = new List<string>();
		string commandText = "SELECT ParametreUser FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram='MobilRaporStokSiparis' GROUP BY ParametreUser ORDER BY ParametreUser";
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
					list.Add(sqlDataReader.GetSafeString(0));
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		foreach (string item in list)
		{
			lb_kullanicilar.Items.Add(item);
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
			AktifKullanici = lb_kullanicilar.SelectedValue.ToString();
			_raporparametreleri = ParametrelerDefault.MobilRaporStokSiparis(AktifKullanici);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _raporparametreleri, "MobilRaporStokSiparis", AktifKullanici, "", "");
			AktifRapor = JsonConvert.DeserializeObject<RaporStokSiparisSecenekleri>(_raporparametreleri._GetParametre("RaporJson")._GetString);
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
		RaporAdi.Text = _raporparametreleri._GetParametre("RaporAdi")._GetString;
		degistirebilir_tarih_cinsi.Checked = AktifRapor.degistirebilir_tarih_cinsi;
		tarih_cinsi.SelectedValue = (int)AktifRapor.tarih_cinsi;
		depolar_secenek.SelectedValue = (int)AktifRapor.depolar_secenek;
		depolar.Text = AktifRapor.depolar;
		degistirebilir_teslim_durumu.Checked = AktifRapor.degistirebilir_teslim_durumu;
		teslim_durumu.SelectedValue = (int)AktifRapor.teslim_durumu;
		proje_kodlari_secenek.SelectedValue = (int)AktifRapor.proje_kodlari_secenek;
		proje_kodlari.Text = AktifRapor.proje_kodlari;
		sorumluluk_merkezleri_secenek.SelectedValue = (int)AktifRapor.sorumluluk_merkezleri_secenek;
		sorumluluk_merkezleri.Text = AktifRapor.sorumluluk_merkezleri;
		stok_arama_secenekleri.SelectedValue = (int)AktifRapor.stok_arama_secenekleri;
		stok_arama_metin.Text = AktifRapor.stok_arama_metin;
		stok_ana_gruplari_secenek.SelectedValue = (int)AktifRapor.stok_ana_gruplari_secenek;
		stok_ana_gruplari.Text = AktifRapor.stok_ana_gruplari;
		stok_uretici_kodu_secenek.SelectedValue = (int)AktifRapor.stok_uretici_kodu_secenek;
		stok_uretici_kodlari.Text = AktifRapor.stok_uretici_kodlari;
		stok_marka_kodu_secenek.SelectedValue = (int)AktifRapor.stok_marka_kodu_secenek;
		stok_marka_kodlari.Text = AktifRapor.stok_marka_kodlari;
		stok_reyon_kodu_secenek.SelectedValue = (int)AktifRapor.stok_reyon_kodu_secenek;
		stok_reyon_kodlari.Text = AktifRapor.stok_reyon_kodlari;
		stok_kategori_kodu_secenek.SelectedValue = (int)AktifRapor.stok_kategori_kodu_secenek;
		stok_kategori_kodlari.Text = AktifRapor.stok_kategori_kodlari;
		cari_arama_secenekleri.SelectedValue = (int)AktifRapor.cari_arama_secenekleri;
		cari_arama_metin.Text = AktifRapor.cari_arama_metin;
		cari_bolge_kodu_secenek.SelectedValue = (int)AktifRapor.cari_bolge_kodu_secenek;
		cari_bolge_kodlari.Text = AktifRapor.cari_bolge_kodlari;
		cari_grup_kodu_secenek.SelectedValue = (int)AktifRapor.cari_grup_kodu_secenek;
		cari_grup_kodlari.Text = AktifRapor.cari_grup_kodlari;
		cari_temsilci_kodu_secenek.SelectedValue = (int)AktifRapor.cari_temsilci_kodu_secenek;
		cari_temsilci_kodlari.Text = AktifRapor.cari_temsilci_kodlari;
		degistirebilir_tutar1_secenek.Checked = AktifRapor.degistirebilir_tutar1_secenek;
		tutar1_secenek.SelectedValue = (int)AktifRapor.tutar1_secenek;
		degistirebilir_tutar2_secenek.Checked = AktifRapor.degistirebilir_tutar2_secenek;
		tutar2_secenek.SelectedValue = (int)AktifRapor.tutar2_secenek;
		degistirebilir_tutar3_secenek.Checked = AktifRapor.degistirebilir_tutar3_secenek;
		tutar3_secenek.SelectedValue = (int)AktifRapor.tutar3_secenek;
		degistirebilir_miktar1_secenek.Checked = AktifRapor.degistirebilir_miktar1_secenek;
		miktar1_secenek.SelectedValue = (int)AktifRapor.miktar1_secenek;
		degistirebilir_miktar2_secenek.Checked = AktifRapor.degistirebilir_miktar2_secenek;
		miktar2_secenek.SelectedValue = (int)AktifRapor.miktar2_secenek;
		degistirebilir_miktar3_secenek.Checked = AktifRapor.degistirebilir_miktar3_secenek;
		miktar3_secenek.SelectedValue = (int)AktifRapor.miktar3_secenek;
		degistirebilir_gruplandirma_secenegi.Checked = AktifRapor.degistirebilir_gruplandirma_secenegi;
		gruplandirma_secenegi.SelectedValue = (int)AktifRapor.gruplandirma_secenegi;
		degistirebilir_siralama_secenegi.Checked = AktifRapor.degistirebilir_siralama_secenegi;
		siralama_secenegi.SelectedValue = (int)AktifRapor.siralama_secenegi;
	}

	private void KullaniciParametreKaydet()
	{
		te_kullanici_adi.Text = AktifKullanici;
		_raporparametreleri._GetParametre("RaporAdi")._SetString = RaporAdi.Text;
		AktifRapor.degistirebilir_tarih_cinsi = degistirebilir_tarih_cinsi.Checked;
		AktifRapor.tarih_cinsi = (enum_tarih_cinsi)(int)tarih_cinsi.SelectedValue;
		AktifRapor.depolar_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)depolar_secenek.SelectedValue;
		AktifRapor.depolar = depolar.Text;
		AktifRapor.degistirebilir_teslim_durumu = degistirebilir_teslim_durumu.Checked;
		AktifRapor.teslim_durumu = (enum_siparis_teslim_durumu_secenekleri)(int)teslim_durumu.SelectedValue;
		AktifRapor.proje_kodlari_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)proje_kodlari_secenek.SelectedValue;
		AktifRapor.proje_kodlari = proje_kodlari.Text;
		AktifRapor.sorumluluk_merkezleri_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)sorumluluk_merkezleri_secenek.SelectedValue;
		AktifRapor.sorumluluk_merkezleri = sorumluluk_merkezleri.Text;
		AktifRapor.stok_arama_secenekleri = (enum_stok_arama_secenekleri)(int)stok_arama_secenekleri.SelectedValue;
		AktifRapor.stok_arama_metin = stok_arama_metin.Text;
		AktifRapor.stok_ana_gruplari_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)stok_ana_gruplari_secenek.SelectedValue;
		AktifRapor.stok_ana_gruplari = stok_ana_gruplari.Text;
		AktifRapor.stok_uretici_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)stok_uretici_kodu_secenek.SelectedValue;
		AktifRapor.stok_uretici_kodlari = stok_uretici_kodlari.Text;
		AktifRapor.stok_marka_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)stok_marka_kodu_secenek.SelectedValue;
		AktifRapor.stok_marka_kodlari = stok_marka_kodlari.Text;
		AktifRapor.stok_reyon_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)stok_reyon_kodu_secenek.SelectedValue;
		AktifRapor.stok_reyon_kodlari = stok_reyon_kodlari.Text;
		AktifRapor.stok_kategori_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)stok_kategori_kodu_secenek.SelectedValue;
		AktifRapor.stok_kategori_kodlari = stok_kategori_kodlari.Text;
		AktifRapor.cari_arama_secenekleri = (enum_cari_arama_secenekleri)(int)cari_arama_secenekleri.SelectedValue;
		AktifRapor.cari_arama_metin = cari_arama_metin.Text;
		AktifRapor.cari_bolge_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)cari_bolge_kodu_secenek.SelectedValue;
		AktifRapor.cari_bolge_kodlari = cari_bolge_kodlari.Text;
		AktifRapor.cari_grup_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)cari_grup_kodu_secenek.SelectedValue;
		AktifRapor.cari_grup_kodlari = cari_grup_kodlari.Text;
		AktifRapor.cari_temsilci_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)cari_temsilci_kodu_secenek.SelectedValue;
		AktifRapor.cari_temsilci_kodlari = cari_temsilci_kodlari.Text;
		AktifRapor.degistirebilir_tutar1_secenek = degistirebilir_tutar1_secenek.Checked;
		AktifRapor.tutar1_secenek = (enum_siparis_tutar_secenekleri)(int)tutar1_secenek.SelectedValue;
		AktifRapor.degistirebilir_tutar2_secenek = degistirebilir_tutar2_secenek.Checked;
		AktifRapor.tutar2_secenek = (enum_siparis_tutar_secenekleri)(int)tutar2_secenek.SelectedValue;
		AktifRapor.degistirebilir_tutar3_secenek = degistirebilir_tutar3_secenek.Checked;
		AktifRapor.tutar3_secenek = (enum_siparis_tutar_secenekleri)(int)tutar3_secenek.SelectedValue;
		AktifRapor.degistirebilir_miktar1_secenek = degistirebilir_miktar1_secenek.Checked;
		AktifRapor.miktar1_secenek = (enum_siparis_miktar_secenekleri)(int)miktar1_secenek.SelectedValue;
		AktifRapor.degistirebilir_miktar2_secenek = degistirebilir_miktar2_secenek.Checked;
		AktifRapor.miktar2_secenek = (enum_siparis_miktar_secenekleri)(int)miktar2_secenek.SelectedValue;
		AktifRapor.degistirebilir_miktar3_secenek = degistirebilir_miktar3_secenek.Checked;
		AktifRapor.miktar3_secenek = (enum_siparis_miktar_secenekleri)(int)miktar3_secenek.SelectedValue;
		AktifRapor.degistirebilir_gruplandirma_secenegi = degistirebilir_gruplandirma_secenegi.Checked;
		AktifRapor.gruplandirma_secenegi = (enum_gruplandirma_secenekleri)(int)gruplandirma_secenegi.SelectedValue;
		AktifRapor.degistirebilir_siralama_secenegi = degistirebilir_siralama_secenegi.Checked;
		AktifRapor.siralama_secenegi = (enum_siralama_secenekleri)(int)siralama_secenegi.SelectedValue;
		_raporparametreleri._GetParametre("RaporJson")._SetString = JsonConvert.SerializeObject(AktifRapor);
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _raporparametreleri);
		_raporparametreleri = ParametrelerDefault.MobilRaporStokSiparis(AktifKullanici);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _raporparametreleri, "MobilRaporStokSiparis", AktifKullanici, "", "");
		AktifRapor = JsonConvert.DeserializeObject<RaporStokSiparisSecenekleri>(_raporparametreleri._GetParametre("RaporJson")._GetString);
		EkranBilgiGuncelle();
		DegisiklikVar = false;
	}

	private void ParametreTextEdit_KeyDown(object sender, KeyEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void ParametreCheckEditMouseClick(object sender, MouseEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			System.Windows.Forms.ComboBox comboBox = (System.Windows.Forms.ComboBox)sender;
			TextEdit textEdit = null;
			switch (comboBox.Name)
			{
			case "depolar_secenek":
				textEdit = depolar;
				break;
			case "proje_kodlari_secenek":
				textEdit = proje_kodlari;
				break;
			case "sorumluluk_merkezleri_secenek":
				textEdit = sorumluluk_merkezleri;
				break;
			case "stok_ana_gruplari_secenek":
				textEdit = stok_ana_gruplari;
				break;
			case "stok_uretici_kodu_secenek":
				textEdit = stok_uretici_kodlari;
				break;
			case "stok_marka_kodu_secenek":
				textEdit = stok_marka_kodlari;
				break;
			case "stok_reyon_kodu_secenek":
				textEdit = stok_reyon_kodlari;
				break;
			case "stok_kategori_kodu_secenek":
				textEdit = stok_kategori_kodlari;
				break;
			case "cari_bolge_kodu_secenek":
				textEdit = cari_bolge_kodlari;
				break;
			case "cari_grup_kodu_secenek":
				textEdit = cari_grup_kodlari;
				break;
			case "cari_temsilci_kodu_secenek":
				textEdit = cari_temsilci_kodlari;
				break;
			}
			enum_tumu_tanimli_olan_listeden_sec enum_tumu_tanimli_olan_listeden_sec = (enum_tumu_tanimli_olan_listeden_sec)(int)comboBox.SelectedValue;
			if (enum_tumu_tanimli_olan_listeden_sec == enum_tumu_tanimli_olan_listeden_sec.ListedenSec)
			{
				textEdit.Enabled = true;
			}
			else
			{
				textEdit.Enabled = false;
			}
		}
		catch
		{
		}
	}

	private void Arama_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			System.Windows.Forms.ComboBox comboBox = (System.Windows.Forms.ComboBox)sender;
			TextEdit textEdit = null;
			string name = comboBox.Name;
			if (!(name == "stok_arama_secenekleri"))
			{
				if (name == "cari_arama_secenekleri")
				{
					textEdit = cari_arama_metin;
				}
			}
			else
			{
				textEdit = stok_arama_metin;
			}
			switch ((enum_stok_arama_secenekleri)(int)comboBox.SelectedValue)
			{
			case enum_stok_arama_secenekleri.Esittir:
				textEdit.Enabled = true;
				break;
			case enum_stok_arama_secenekleri.Icerir:
				textEdit.Enabled = true;
				break;
			case enum_stok_arama_secenekleri.IleBaslar:
				textEdit.Enabled = true;
				break;
			default:
				textEdit.Enabled = false;
				break;
			}
		}
		catch
		{
		}
	}

	private void kaydetToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (lb_kullanicilar.SelectedValue != null)
		{
			KullaniciParametreKaydet();
		}
	}

	private void ekleToolStripMenuItem_Click(object sender, EventArgs e)
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
		MobilRaporStokSiparisEkle mobilRaporStokSiparisEkle = new MobilRaporStokSiparisEkle(_mikrouygulamabilgileri);
		mobilRaporStokSiparisEkle.ShowDialog();
		if (mobilRaporStokSiparisEkle.DialogResult == DialogResult.OK)
		{
			Parametreler parametreler = ParametrelerDefault.MobilRaporStokSiparis(mobilRaporStokSiparisEkle._RaporKodu);
			parametreler._GetParametre("RaporAdi")._SetString = mobilRaporStokSiparisEkle._RaporAdi;
			parametreler._GetParametre("RaporJson")._SetString = JsonConvert.SerializeObject(new RaporStokSiparisSecenekleri());
			ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler);
			KullanicilariListele();
			lb_kullanicilar.SelectedItem = mobilRaporStokSiparisEkle._RaporKodu;
		}
		mobilRaporStokSiparisEkle.Dispose();
		mobilRaporStokSiparisEkle = null;
	}

	private void silToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (lb_kullanicilar.SelectedValue == null)
		{
			return;
		}
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
		string text = lb_kullanicilar.SelectedValue.ToString();
		DialogResult dialogResult = MessageBox.Show(text + " raporunu silmek istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		if (dialogResult != DialogResult.Yes)
		{
			_ = 7;
			return;
		}
		string commandText = "DELETE _FORA_PARAMETRELER WHERE ParametreProgram='MobilRaporStokSiparis' AND ParametreUser=@ParametreUser";
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.ExecuteNonQuery();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		KullanicilariListele();
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
				_raporparametreleri.KullanimAlani = "MobilRaporStokSiparis";
				Parametreler.WriteToStream(_raporparametreleri, fileStream, 2);
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
		TextSor textSor = new TextSor("Rapor kodu", "DEVAM");
		if (textSor.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		foreach (string item in lb_kullanicilar.Items)
		{
			if (item == textSor.te_cevap.Text)
			{
				MessageBox.Show("Bu rapor kodu daha önce kullanılmış. İşlem tamamlanamadı!");
				return;
			}
		}
		FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
		Parametreler parametreler = Parametreler.ReadFromStream(fileStream);
		fileStream.Close();
		fileStream.Dispose();
		if (parametreler.KullanimAlani != "MobilRaporStokSiparis")
		{
			MessageBox.Show("Dosya uygun değil. İşlem tamamlanamadı!");
			return;
		}
		parametreler._GetParametre("RaporAdi")._SetString = textSor.te_cevap.Text;
		foreach (Parametre item2 in parametreler.ParametreListesi)
		{
			item2.EskiID = 0;
			item2.IDGuid = null;
			item2.ParametreUser = textSor.te_cevap.Text;
		}
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler);
		KullanicilariListele();
		lb_kullanicilar.SelectedItem = textSor.te_cevap.Text;
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
		this.tc_parametrevehaklar = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
		this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.degistirebilir_miktar3_secenek = new DevExpress.XtraEditors.CheckEdit();
		this.miktar3_secenek = new System.Windows.Forms.ComboBox();
		this.degistirebilir_miktar2_secenek = new DevExpress.XtraEditors.CheckEdit();
		this.miktar2_secenek = new System.Windows.Forms.ComboBox();
		this.degistirebilir_miktar1_secenek = new DevExpress.XtraEditors.CheckEdit();
		this.miktar1_secenek = new System.Windows.Forms.ComboBox();
		this.degistirebilir_tutar3_secenek = new DevExpress.XtraEditors.CheckEdit();
		this.tutar3_secenek = new System.Windows.Forms.ComboBox();
		this.degistirebilir_tutar2_secenek = new DevExpress.XtraEditors.CheckEdit();
		this.tutar2_secenek = new System.Windows.Forms.ComboBox();
		this.degistirebilir_tutar1_secenek = new DevExpress.XtraEditors.CheckEdit();
		this.tutar1_secenek = new System.Windows.Forms.ComboBox();
		this.teslim_durumu = new System.Windows.Forms.ComboBox();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.RaporAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.te_kullanici_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.degistirebilir_siralama_secenegi = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl111 = new DevExpress.XtraEditors.LabelControl();
		this.siralama_secenegi = new System.Windows.Forms.ComboBox();
		this.degistirebilir_gruplandirma_secenegi = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl90 = new DevExpress.XtraEditors.LabelControl();
		this.gruplandirma_secenegi = new System.Windows.Forms.ComboBox();
		this.degistirebilir_teslim_durumu = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl105 = new DevExpress.XtraEditors.LabelControl();
		this.degistirebilir_tarih_cinsi = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl57 = new DevExpress.XtraEditors.LabelControl();
		this.tarih_cinsi = new System.Windows.Forms.ComboBox();
		this.xtraTabPage15 = new DevExpress.XtraTab.XtraTabPage();
		this.labelControl107 = new DevExpress.XtraEditors.LabelControl();
		this.sorumluluk_merkezleri = new DevExpress.XtraEditors.TextEdit();
		this.sorumluluk_merkezleri_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl110 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl96 = new DevExpress.XtraEditors.LabelControl();
		this.depolar = new DevExpress.XtraEditors.TextEdit();
		this.depolar_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl93 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl97 = new DevExpress.XtraEditors.LabelControl();
		this.proje_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.proje_kodlari_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl106 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl51 = new DevExpress.XtraEditors.LabelControl();
		this.cari_temsilci_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.cari_temsilci_kodu_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl54 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl47 = new DevExpress.XtraEditors.LabelControl();
		this.cari_grup_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.cari_grup_kodu_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl50 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl34 = new DevExpress.XtraEditors.LabelControl();
		this.cari_bolge_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.cari_bolge_kodu_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl43 = new DevExpress.XtraEditors.LabelControl();
		this.cari_arama_metin = new DevExpress.XtraEditors.TextEdit();
		this.cari_arama_secenekleri = new System.Windows.Forms.ComboBox();
		this.labelControl46 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl30 = new DevExpress.XtraEditors.LabelControl();
		this.stok_kategori_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.stok_kategori_kodu_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl33 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl26 = new DevExpress.XtraEditors.LabelControl();
		this.stok_reyon_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.stok_reyon_kodu_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl29 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl22 = new DevExpress.XtraEditors.LabelControl();
		this.stok_marka_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.stok_marka_kodu_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl25 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl16 = new DevExpress.XtraEditors.LabelControl();
		this.stok_uretici_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.stok_uretici_kodu_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl20 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
		this.stok_ana_gruplari = new DevExpress.XtraEditors.TextEdit();
		this.stok_ana_gruplari_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl15 = new DevExpress.XtraEditors.LabelControl();
		this.stok_arama_metin = new DevExpress.XtraEditors.TextEdit();
		this.stok_arama_secenekleri = new System.Windows.Forms.ComboBox();
		this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
		this.lb_kullanicilar = new DevExpress.XtraEditors.ListBoxControl();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.label718 = new System.Windows.Forms.Label();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.ekleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.silToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.dosyayaYazToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.dosyadanOkuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		((System.ComponentModel.ISupportInitialize)this.tc_parametrevehaklar).BeginInit();
		this.tc_parametrevehaklar.SuspendLayout();
		this.xtraTabPage2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_miktar3_secenek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_miktar2_secenek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_miktar1_secenek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_tutar3_secenek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_tutar2_secenek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_tutar1_secenek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.RaporAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_siralama_secenegi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_gruplandirma_secenegi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_teslim_durumu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_tarih_cinsi.Properties).BeginInit();
		this.xtraTabPage15.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.sorumluluk_merkezleri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.depolar.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.proje_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_temsilci_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_grup_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_bolge_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_arama_metin.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.stok_kategori_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.stok_reyon_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.stok_marka_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.stok_uretici_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.stok_ana_gruplari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.stok_arama_metin.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.tc_parametrevehaklar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tc_parametrevehaklar.Enabled = false;
		this.tc_parametrevehaklar.Location = new System.Drawing.Point(144, 35);
		this.tc_parametrevehaklar.Name = "tc_parametrevehaklar";
		this.tc_parametrevehaklar.SelectedTabPage = this.xtraTabPage2;
		this.tc_parametrevehaklar.Size = new System.Drawing.Size(789, 638);
		this.tc_parametrevehaklar.TabIndex = 4;
		this.tc_parametrevehaklar.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[2] { this.xtraTabPage2, this.xtraTabPage15 });
		this.xtraTabPage2.Controls.Add(this.labelControl10);
		this.xtraTabPage2.Controls.Add(this.labelControl8);
		this.xtraTabPage2.Controls.Add(this.labelControl7);
		this.xtraTabPage2.Controls.Add(this.labelControl6);
		this.xtraTabPage2.Controls.Add(this.labelControl5);
		this.xtraTabPage2.Controls.Add(this.labelControl4);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_miktar3_secenek);
		this.xtraTabPage2.Controls.Add(this.miktar3_secenek);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_miktar2_secenek);
		this.xtraTabPage2.Controls.Add(this.miktar2_secenek);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_miktar1_secenek);
		this.xtraTabPage2.Controls.Add(this.miktar1_secenek);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_tutar3_secenek);
		this.xtraTabPage2.Controls.Add(this.tutar3_secenek);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_tutar2_secenek);
		this.xtraTabPage2.Controls.Add(this.tutar2_secenek);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_tutar1_secenek);
		this.xtraTabPage2.Controls.Add(this.tutar1_secenek);
		this.xtraTabPage2.Controls.Add(this.teslim_durumu);
		this.xtraTabPage2.Controls.Add(this.labelControl3);
		this.xtraTabPage2.Controls.Add(this.RaporAdi);
		this.xtraTabPage2.Controls.Add(this.labelControl2);
		this.xtraTabPage2.Controls.Add(this.te_kullanici_adi);
		this.xtraTabPage2.Controls.Add(this.labelControl1);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_siralama_secenegi);
		this.xtraTabPage2.Controls.Add(this.labelControl111);
		this.xtraTabPage2.Controls.Add(this.siralama_secenegi);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_gruplandirma_secenegi);
		this.xtraTabPage2.Controls.Add(this.labelControl90);
		this.xtraTabPage2.Controls.Add(this.gruplandirma_secenegi);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_teslim_durumu);
		this.xtraTabPage2.Controls.Add(this.labelControl105);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_tarih_cinsi);
		this.xtraTabPage2.Controls.Add(this.labelControl57);
		this.xtraTabPage2.Controls.Add(this.tarih_cinsi);
		this.xtraTabPage2.Name = "xtraTabPage2";
		this.xtraTabPage2.Size = new System.Drawing.Size(783, 610);
		this.xtraTabPage2.Text = "Seçenekler";
		this.labelControl10.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl10.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl10.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl10.Location = new System.Drawing.Point(22, 428);
		this.labelControl10.Name = "labelControl10";
		this.labelControl10.Size = new System.Drawing.Size(115, 19);
		this.labelControl10.TabIndex = 232;
		this.labelControl10.Text = "Miktar 3 :";
		this.labelControl8.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl8.Location = new System.Drawing.Point(22, 401);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(115, 19);
		this.labelControl8.TabIndex = 231;
		this.labelControl8.Text = "Miktar 2 :";
		this.labelControl7.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl7.Location = new System.Drawing.Point(22, 374);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(115, 19);
		this.labelControl7.TabIndex = 230;
		this.labelControl7.Text = "Miktar 1 :";
		this.labelControl6.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(22, 322);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(115, 19);
		this.labelControl6.TabIndex = 229;
		this.labelControl6.Text = "Tutar 3 :";
		this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(22, 295);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(115, 19);
		this.labelControl5.TabIndex = 228;
		this.labelControl5.Text = "Tutar 2 :";
		this.labelControl4.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(22, 270);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(115, 19);
		this.labelControl4.TabIndex = 227;
		this.labelControl4.Text = "Tutar 1 :";
		this.degistirebilir_miktar3_secenek.Location = new System.Drawing.Point(281, 428);
		this.degistirebilir_miktar3_secenek.Name = "degistirebilir_miktar3_secenek";
		this.degistirebilir_miktar3_secenek.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_miktar3_secenek.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_miktar3_secenek.TabIndex = 226;
		this.degistirebilir_miktar3_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_miktar3_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.miktar3_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.miktar3_secenek.FormattingEnabled = true;
		this.miktar3_secenek.Location = new System.Drawing.Point(143, 428);
		this.miktar3_secenek.Name = "miktar3_secenek";
		this.miktar3_secenek.Size = new System.Drawing.Size(133, 21);
		this.miktar3_secenek.TabIndex = 225;
		this.miktar3_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.miktar3_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.degistirebilir_miktar2_secenek.Location = new System.Drawing.Point(281, 401);
		this.degistirebilir_miktar2_secenek.Name = "degistirebilir_miktar2_secenek";
		this.degistirebilir_miktar2_secenek.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_miktar2_secenek.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_miktar2_secenek.TabIndex = 223;
		this.degistirebilir_miktar2_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_miktar2_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.miktar2_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.miktar2_secenek.FormattingEnabled = true;
		this.miktar2_secenek.Location = new System.Drawing.Point(143, 401);
		this.miktar2_secenek.Name = "miktar2_secenek";
		this.miktar2_secenek.Size = new System.Drawing.Size(133, 21);
		this.miktar2_secenek.TabIndex = 222;
		this.miktar2_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.miktar2_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.degistirebilir_miktar1_secenek.Location = new System.Drawing.Point(281, 374);
		this.degistirebilir_miktar1_secenek.Name = "degistirebilir_miktar1_secenek";
		this.degistirebilir_miktar1_secenek.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_miktar1_secenek.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_miktar1_secenek.TabIndex = 220;
		this.degistirebilir_miktar1_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_miktar1_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.miktar1_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.miktar1_secenek.FormattingEnabled = true;
		this.miktar1_secenek.Location = new System.Drawing.Point(143, 374);
		this.miktar1_secenek.Name = "miktar1_secenek";
		this.miktar1_secenek.Size = new System.Drawing.Size(133, 21);
		this.miktar1_secenek.TabIndex = 219;
		this.miktar1_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.miktar1_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.degistirebilir_tutar3_secenek.Location = new System.Drawing.Point(281, 322);
		this.degistirebilir_tutar3_secenek.Name = "degistirebilir_tutar3_secenek";
		this.degistirebilir_tutar3_secenek.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_tutar3_secenek.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_tutar3_secenek.TabIndex = 217;
		this.degistirebilir_tutar3_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_tutar3_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.tutar3_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.tutar3_secenek.FormattingEnabled = true;
		this.tutar3_secenek.Location = new System.Drawing.Point(143, 322);
		this.tutar3_secenek.Name = "tutar3_secenek";
		this.tutar3_secenek.Size = new System.Drawing.Size(133, 21);
		this.tutar3_secenek.TabIndex = 216;
		this.tutar3_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.tutar3_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.degistirebilir_tutar2_secenek.Location = new System.Drawing.Point(281, 295);
		this.degistirebilir_tutar2_secenek.Name = "degistirebilir_tutar2_secenek";
		this.degistirebilir_tutar2_secenek.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_tutar2_secenek.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_tutar2_secenek.TabIndex = 214;
		this.degistirebilir_tutar2_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_tutar2_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.tutar2_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.tutar2_secenek.FormattingEnabled = true;
		this.tutar2_secenek.Location = new System.Drawing.Point(143, 295);
		this.tutar2_secenek.Name = "tutar2_secenek";
		this.tutar2_secenek.Size = new System.Drawing.Size(133, 21);
		this.tutar2_secenek.TabIndex = 213;
		this.tutar2_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.tutar2_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.degistirebilir_tutar1_secenek.Location = new System.Drawing.Point(281, 268);
		this.degistirebilir_tutar1_secenek.Name = "degistirebilir_tutar1_secenek";
		this.degistirebilir_tutar1_secenek.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_tutar1_secenek.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_tutar1_secenek.TabIndex = 211;
		this.degistirebilir_tutar1_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_tutar1_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.tutar1_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.tutar1_secenek.FormattingEnabled = true;
		this.tutar1_secenek.Location = new System.Drawing.Point(143, 268);
		this.tutar1_secenek.Name = "tutar1_secenek";
		this.tutar1_secenek.Size = new System.Drawing.Size(133, 21);
		this.tutar1_secenek.TabIndex = 210;
		this.tutar1_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.tutar1_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.teslim_durumu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.teslim_durumu.FormattingEnabled = true;
		this.teslim_durumu.Location = new System.Drawing.Point(143, 162);
		this.teslim_durumu.Name = "teslim_durumu";
		this.teslim_durumu.Size = new System.Drawing.Size(133, 21);
		this.teslim_durumu.TabIndex = 201;
		this.teslim_durumu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.teslim_durumu.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(15, 25);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(126, 19);
		this.labelControl3.TabIndex = 199;
		this.labelControl3.Text = "Rapor tanımı";
		this.RaporAdi.Location = new System.Drawing.Point(143, 76);
		this.RaporAdi.Name = "RaporAdi";
		this.RaporAdi.Size = new System.Drawing.Size(358, 20);
		this.RaporAdi.TabIndex = 198;
		this.RaporAdi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.RaporAdi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(20, 79);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(117, 13);
		this.labelControl2.TabIndex = 197;
		this.labelControl2.Text = "Rapor adı :";
		this.te_kullanici_adi.Enabled = false;
		this.te_kullanici_adi.Location = new System.Drawing.Point(143, 50);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.te_kullanici_adi.Properties.Appearance.Options.UseForeColor = true;
		this.te_kullanici_adi.Size = new System.Drawing.Size(106, 20);
		this.te_kullanici_adi.TabIndex = 196;
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(20, 53);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(117, 13);
		this.labelControl1.TabIndex = 195;
		this.labelControl1.Text = "Rapor kodu :";
		this.degistirebilir_siralama_secenegi.Location = new System.Drawing.Point(281, 216);
		this.degistirebilir_siralama_secenegi.Name = "degistirebilir_siralama_secenegi";
		this.degistirebilir_siralama_secenegi.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_siralama_secenegi.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_siralama_secenegi.TabIndex = 194;
		this.degistirebilir_siralama_secenegi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_siralama_secenegi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl111.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl111.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl111.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl111.Location = new System.Drawing.Point(22, 216);
		this.labelControl111.Name = "labelControl111";
		this.labelControl111.Size = new System.Drawing.Size(115, 19);
		this.labelControl111.TabIndex = 193;
		this.labelControl111.Text = "Sıralama :";
		this.siralama_secenegi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.siralama_secenegi.FormattingEnabled = true;
		this.siralama_secenegi.Location = new System.Drawing.Point(143, 216);
		this.siralama_secenegi.Name = "siralama_secenegi";
		this.siralama_secenegi.Size = new System.Drawing.Size(133, 21);
		this.siralama_secenegi.TabIndex = 192;
		this.siralama_secenegi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.siralama_secenegi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.degistirebilir_gruplandirma_secenegi.Location = new System.Drawing.Point(282, 189);
		this.degistirebilir_gruplandirma_secenegi.Name = "degistirebilir_gruplandirma_secenegi";
		this.degistirebilir_gruplandirma_secenegi.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_gruplandirma_secenegi.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_gruplandirma_secenegi.TabIndex = 190;
		this.degistirebilir_gruplandirma_secenegi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_gruplandirma_secenegi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl90.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl90.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl90.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl90.Location = new System.Drawing.Point(22, 189);
		this.labelControl90.Name = "labelControl90";
		this.labelControl90.Size = new System.Drawing.Size(115, 19);
		this.labelControl90.TabIndex = 189;
		this.labelControl90.Text = "Gruplandırma :";
		this.gruplandirma_secenegi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.gruplandirma_secenegi.FormattingEnabled = true;
		this.gruplandirma_secenegi.Location = new System.Drawing.Point(143, 189);
		this.gruplandirma_secenegi.Name = "gruplandirma_secenegi";
		this.gruplandirma_secenegi.Size = new System.Drawing.Size(133, 21);
		this.gruplandirma_secenegi.TabIndex = 188;
		this.gruplandirma_secenegi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.gruplandirma_secenegi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.degistirebilir_teslim_durumu.Location = new System.Drawing.Point(282, 162);
		this.degistirebilir_teslim_durumu.Name = "degistirebilir_teslim_durumu";
		this.degistirebilir_teslim_durumu.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_teslim_durumu.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_teslim_durumu.TabIndex = 165;
		this.degistirebilir_teslim_durumu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_teslim_durumu.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl105.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl105.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl105.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl105.Location = new System.Drawing.Point(22, 162);
		this.labelControl105.Name = "labelControl105";
		this.labelControl105.Size = new System.Drawing.Size(115, 19);
		this.labelControl105.TabIndex = 162;
		this.labelControl105.Text = "Teslim durumu :";
		this.degistirebilir_tarih_cinsi.Location = new System.Drawing.Point(281, 135);
		this.degistirebilir_tarih_cinsi.Name = "degistirebilir_tarih_cinsi";
		this.degistirebilir_tarih_cinsi.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_tarih_cinsi.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_tarih_cinsi.TabIndex = 146;
		this.degistirebilir_tarih_cinsi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_tarih_cinsi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl57.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl57.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl57.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl57.Location = new System.Drawing.Point(22, 135);
		this.labelControl57.Name = "labelControl57";
		this.labelControl57.Size = new System.Drawing.Size(115, 19);
		this.labelControl57.TabIndex = 145;
		this.labelControl57.Text = "Tarih :";
		this.tarih_cinsi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.tarih_cinsi.FormattingEnabled = true;
		this.tarih_cinsi.Location = new System.Drawing.Point(143, 135);
		this.tarih_cinsi.Name = "tarih_cinsi";
		this.tarih_cinsi.Size = new System.Drawing.Size(133, 21);
		this.tarih_cinsi.TabIndex = 144;
		this.tarih_cinsi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.tarih_cinsi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.xtraTabPage15.Controls.Add(this.labelControl107);
		this.xtraTabPage15.Controls.Add(this.sorumluluk_merkezleri);
		this.xtraTabPage15.Controls.Add(this.sorumluluk_merkezleri_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl110);
		this.xtraTabPage15.Controls.Add(this.labelControl96);
		this.xtraTabPage15.Controls.Add(this.depolar);
		this.xtraTabPage15.Controls.Add(this.depolar_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl93);
		this.xtraTabPage15.Controls.Add(this.labelControl97);
		this.xtraTabPage15.Controls.Add(this.proje_kodlari);
		this.xtraTabPage15.Controls.Add(this.proje_kodlari_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl106);
		this.xtraTabPage15.Controls.Add(this.labelControl51);
		this.xtraTabPage15.Controls.Add(this.cari_temsilci_kodlari);
		this.xtraTabPage15.Controls.Add(this.cari_temsilci_kodu_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl54);
		this.xtraTabPage15.Controls.Add(this.labelControl47);
		this.xtraTabPage15.Controls.Add(this.cari_grup_kodlari);
		this.xtraTabPage15.Controls.Add(this.cari_grup_kodu_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl50);
		this.xtraTabPage15.Controls.Add(this.labelControl34);
		this.xtraTabPage15.Controls.Add(this.cari_bolge_kodlari);
		this.xtraTabPage15.Controls.Add(this.cari_bolge_kodu_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl43);
		this.xtraTabPage15.Controls.Add(this.cari_arama_metin);
		this.xtraTabPage15.Controls.Add(this.cari_arama_secenekleri);
		this.xtraTabPage15.Controls.Add(this.labelControl46);
		this.xtraTabPage15.Controls.Add(this.labelControl30);
		this.xtraTabPage15.Controls.Add(this.stok_kategori_kodlari);
		this.xtraTabPage15.Controls.Add(this.stok_kategori_kodu_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl33);
		this.xtraTabPage15.Controls.Add(this.labelControl26);
		this.xtraTabPage15.Controls.Add(this.stok_reyon_kodlari);
		this.xtraTabPage15.Controls.Add(this.stok_reyon_kodu_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl29);
		this.xtraTabPage15.Controls.Add(this.labelControl22);
		this.xtraTabPage15.Controls.Add(this.stok_marka_kodlari);
		this.xtraTabPage15.Controls.Add(this.stok_marka_kodu_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl25);
		this.xtraTabPage15.Controls.Add(this.labelControl16);
		this.xtraTabPage15.Controls.Add(this.stok_uretici_kodlari);
		this.xtraTabPage15.Controls.Add(this.stok_uretici_kodu_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl20);
		this.xtraTabPage15.Controls.Add(this.labelControl9);
		this.xtraTabPage15.Controls.Add(this.stok_ana_gruplari);
		this.xtraTabPage15.Controls.Add(this.stok_ana_gruplari_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl15);
		this.xtraTabPage15.Controls.Add(this.stok_arama_metin);
		this.xtraTabPage15.Controls.Add(this.stok_arama_secenekleri);
		this.xtraTabPage15.Controls.Add(this.labelControl12);
		this.xtraTabPage15.Name = "xtraTabPage15";
		this.xtraTabPage15.Size = new System.Drawing.Size(772, 603);
		this.xtraTabPage15.Text = "Filitreler";
		this.labelControl107.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl107.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl107.Location = new System.Drawing.Point(491, 395);
		this.labelControl107.Name = "labelControl107";
		this.labelControl107.Size = new System.Drawing.Size(222, 13);
		this.labelControl107.TabIndex = 263;
		this.labelControl107.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.sorumluluk_merkezleri.Location = new System.Drawing.Point(353, 392);
		this.sorumluluk_merkezleri.Name = "sorumluluk_merkezleri";
		this.sorumluluk_merkezleri.Size = new System.Drawing.Size(132, 20);
		this.sorumluluk_merkezleri.TabIndex = 262;
		this.sorumluluk_merkezleri.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.sorumluluk_merkezleri.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.sorumluluk_merkezleri_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.sorumluluk_merkezleri_secenek.FormattingEnabled = true;
		this.sorumluluk_merkezleri_secenek.Location = new System.Drawing.Point(214, 393);
		this.sorumluluk_merkezleri_secenek.Name = "sorumluluk_merkezleri_secenek";
		this.sorumluluk_merkezleri_secenek.Size = new System.Drawing.Size(133, 21);
		this.sorumluluk_merkezleri_secenek.TabIndex = 258;
		this.sorumluluk_merkezleri_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.sorumluluk_merkezleri_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.sorumluluk_merkezleri_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl110.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl110.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl110.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl110.Location = new System.Drawing.Point(52, 393);
		this.labelControl110.Name = "labelControl110";
		this.labelControl110.Size = new System.Drawing.Size(156, 19);
		this.labelControl110.TabIndex = 256;
		this.labelControl110.Text = "Sorumluluk merkezi :";
		this.labelControl96.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl96.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl96.Location = new System.Drawing.Point(491, 423);
		this.labelControl96.Name = "labelControl96";
		this.labelControl96.Size = new System.Drawing.Size(222, 13);
		this.labelControl96.TabIndex = 255;
		this.labelControl96.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.depolar.Location = new System.Drawing.Point(353, 420);
		this.depolar.Name = "depolar";
		this.depolar.Size = new System.Drawing.Size(132, 20);
		this.depolar.TabIndex = 254;
		this.depolar.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.depolar.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.depolar_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.depolar_secenek.FormattingEnabled = true;
		this.depolar_secenek.Location = new System.Drawing.Point(214, 420);
		this.depolar_secenek.Name = "depolar_secenek";
		this.depolar_secenek.Size = new System.Drawing.Size(133, 21);
		this.depolar_secenek.TabIndex = 250;
		this.depolar_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.depolar_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.depolar_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl93.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl93.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl93.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl93.Location = new System.Drawing.Point(52, 420);
		this.labelControl93.Name = "labelControl93";
		this.labelControl93.Size = new System.Drawing.Size(156, 19);
		this.labelControl93.TabIndex = 248;
		this.labelControl93.Text = "Depo :";
		this.labelControl97.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl97.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl97.Location = new System.Drawing.Point(491, 369);
		this.labelControl97.Name = "labelControl97";
		this.labelControl97.Size = new System.Drawing.Size(222, 13);
		this.labelControl97.TabIndex = 247;
		this.labelControl97.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.proje_kodlari.Location = new System.Drawing.Point(353, 366);
		this.proje_kodlari.Name = "proje_kodlari";
		this.proje_kodlari.Size = new System.Drawing.Size(132, 20);
		this.proje_kodlari.TabIndex = 246;
		this.proje_kodlari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.proje_kodlari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.proje_kodlari_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.proje_kodlari_secenek.FormattingEnabled = true;
		this.proje_kodlari_secenek.Location = new System.Drawing.Point(214, 366);
		this.proje_kodlari_secenek.Name = "proje_kodlari_secenek";
		this.proje_kodlari_secenek.Size = new System.Drawing.Size(133, 21);
		this.proje_kodlari_secenek.TabIndex = 242;
		this.proje_kodlari_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.proje_kodlari_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.proje_kodlari_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl106.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl106.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl106.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl106.Location = new System.Drawing.Point(52, 366);
		this.labelControl106.Name = "labelControl106";
		this.labelControl106.Size = new System.Drawing.Size(156, 19);
		this.labelControl106.TabIndex = 240;
		this.labelControl106.Text = "Proje :";
		this.labelControl51.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl51.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl51.Location = new System.Drawing.Point(491, 267);
		this.labelControl51.Name = "labelControl51";
		this.labelControl51.Size = new System.Drawing.Size(222, 13);
		this.labelControl51.TabIndex = 239;
		this.labelControl51.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.cari_temsilci_kodlari.Location = new System.Drawing.Point(353, 264);
		this.cari_temsilci_kodlari.Name = "cari_temsilci_kodlari";
		this.cari_temsilci_kodlari.Size = new System.Drawing.Size(132, 20);
		this.cari_temsilci_kodlari.TabIndex = 238;
		this.cari_temsilci_kodlari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_temsilci_kodlari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_temsilci_kodu_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_temsilci_kodu_secenek.FormattingEnabled = true;
		this.cari_temsilci_kodu_secenek.Location = new System.Drawing.Point(214, 264);
		this.cari_temsilci_kodu_secenek.Name = "cari_temsilci_kodu_secenek";
		this.cari_temsilci_kodu_secenek.Size = new System.Drawing.Size(133, 21);
		this.cari_temsilci_kodu_secenek.TabIndex = 234;
		this.cari_temsilci_kodu_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.cari_temsilci_kodu_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_temsilci_kodu_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl54.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl54.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl54.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl54.Location = new System.Drawing.Point(52, 267);
		this.labelControl54.Name = "labelControl54";
		this.labelControl54.Size = new System.Drawing.Size(156, 19);
		this.labelControl54.TabIndex = 232;
		this.labelControl54.Text = "Cari temsilcileri :";
		this.labelControl47.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl47.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl47.Location = new System.Drawing.Point(491, 321);
		this.labelControl47.Name = "labelControl47";
		this.labelControl47.Size = new System.Drawing.Size(222, 13);
		this.labelControl47.TabIndex = 231;
		this.labelControl47.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.cari_grup_kodlari.Location = new System.Drawing.Point(353, 318);
		this.cari_grup_kodlari.Name = "cari_grup_kodlari";
		this.cari_grup_kodlari.Size = new System.Drawing.Size(132, 20);
		this.cari_grup_kodlari.TabIndex = 230;
		this.cari_grup_kodlari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_grup_kodlari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_grup_kodu_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_grup_kodu_secenek.FormattingEnabled = true;
		this.cari_grup_kodu_secenek.Location = new System.Drawing.Point(214, 318);
		this.cari_grup_kodu_secenek.Name = "cari_grup_kodu_secenek";
		this.cari_grup_kodu_secenek.Size = new System.Drawing.Size(133, 21);
		this.cari_grup_kodu_secenek.TabIndex = 226;
		this.cari_grup_kodu_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.cari_grup_kodu_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_grup_kodu_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl50.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl50.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl50.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl50.Location = new System.Drawing.Point(52, 318);
		this.labelControl50.Name = "labelControl50";
		this.labelControl50.Size = new System.Drawing.Size(156, 19);
		this.labelControl50.TabIndex = 224;
		this.labelControl50.Text = "Cari grupları :";
		this.labelControl34.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl34.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl34.Location = new System.Drawing.Point(491, 294);
		this.labelControl34.Name = "labelControl34";
		this.labelControl34.Size = new System.Drawing.Size(222, 13);
		this.labelControl34.TabIndex = 223;
		this.labelControl34.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.cari_bolge_kodlari.Location = new System.Drawing.Point(353, 291);
		this.cari_bolge_kodlari.Name = "cari_bolge_kodlari";
		this.cari_bolge_kodlari.Size = new System.Drawing.Size(132, 20);
		this.cari_bolge_kodlari.TabIndex = 222;
		this.cari_bolge_kodlari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_bolge_kodlari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_bolge_kodu_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_bolge_kodu_secenek.FormattingEnabled = true;
		this.cari_bolge_kodu_secenek.Location = new System.Drawing.Point(214, 291);
		this.cari_bolge_kodu_secenek.Name = "cari_bolge_kodu_secenek";
		this.cari_bolge_kodu_secenek.Size = new System.Drawing.Size(133, 21);
		this.cari_bolge_kodu_secenek.TabIndex = 218;
		this.cari_bolge_kodu_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.cari_bolge_kodu_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_bolge_kodu_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl43.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl43.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl43.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl43.Location = new System.Drawing.Point(52, 296);
		this.labelControl43.Name = "labelControl43";
		this.labelControl43.Size = new System.Drawing.Size(156, 19);
		this.labelControl43.TabIndex = 216;
		this.labelControl43.Text = "Cari bölgeleri :";
		this.cari_arama_metin.Location = new System.Drawing.Point(353, 237);
		this.cari_arama_metin.Name = "cari_arama_metin";
		this.cari_arama_metin.Size = new System.Drawing.Size(132, 20);
		this.cari_arama_metin.TabIndex = 215;
		this.cari_arama_metin.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_arama_metin.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_arama_secenekleri.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_arama_secenekleri.FormattingEnabled = true;
		this.cari_arama_secenekleri.Location = new System.Drawing.Point(214, 237);
		this.cari_arama_secenekleri.Name = "cari_arama_secenekleri";
		this.cari_arama_secenekleri.Size = new System.Drawing.Size(133, 21);
		this.cari_arama_secenekleri.TabIndex = 211;
		this.cari_arama_secenekleri.SelectedIndexChanged += new System.EventHandler(Arama_SelectedIndexChanged);
		this.cari_arama_secenekleri.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_arama_secenekleri.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl46.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl46.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl46.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl46.Location = new System.Drawing.Point(52, 238);
		this.labelControl46.Name = "labelControl46";
		this.labelControl46.Size = new System.Drawing.Size(156, 19);
		this.labelControl46.TabIndex = 209;
		this.labelControl46.Text = "Cari arama :";
		this.labelControl30.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl30.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl30.Location = new System.Drawing.Point(491, 194);
		this.labelControl30.Name = "labelControl30";
		this.labelControl30.Size = new System.Drawing.Size(222, 13);
		this.labelControl30.TabIndex = 208;
		this.labelControl30.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.stok_kategori_kodlari.Location = new System.Drawing.Point(353, 191);
		this.stok_kategori_kodlari.Name = "stok_kategori_kodlari";
		this.stok_kategori_kodlari.Size = new System.Drawing.Size(132, 20);
		this.stok_kategori_kodlari.TabIndex = 207;
		this.stok_kategori_kodlari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_kategori_kodlari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.stok_kategori_kodu_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.stok_kategori_kodu_secenek.FormattingEnabled = true;
		this.stok_kategori_kodu_secenek.Location = new System.Drawing.Point(214, 191);
		this.stok_kategori_kodu_secenek.Name = "stok_kategori_kodu_secenek";
		this.stok_kategori_kodu_secenek.Size = new System.Drawing.Size(133, 21);
		this.stok_kategori_kodu_secenek.TabIndex = 203;
		this.stok_kategori_kodu_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.stok_kategori_kodu_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_kategori_kodu_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl33.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl33.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl33.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl33.Location = new System.Drawing.Point(52, 191);
		this.labelControl33.Name = "labelControl33";
		this.labelControl33.Size = new System.Drawing.Size(156, 19);
		this.labelControl33.TabIndex = 201;
		this.labelControl33.Text = "Stok kategorileri :";
		this.labelControl26.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl26.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl26.Location = new System.Drawing.Point(491, 167);
		this.labelControl26.Name = "labelControl26";
		this.labelControl26.Size = new System.Drawing.Size(222, 13);
		this.labelControl26.TabIndex = 200;
		this.labelControl26.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.stok_reyon_kodlari.Location = new System.Drawing.Point(353, 164);
		this.stok_reyon_kodlari.Name = "stok_reyon_kodlari";
		this.stok_reyon_kodlari.Size = new System.Drawing.Size(132, 20);
		this.stok_reyon_kodlari.TabIndex = 199;
		this.stok_reyon_kodlari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_reyon_kodlari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.stok_reyon_kodu_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.stok_reyon_kodu_secenek.FormattingEnabled = true;
		this.stok_reyon_kodu_secenek.Location = new System.Drawing.Point(214, 164);
		this.stok_reyon_kodu_secenek.Name = "stok_reyon_kodu_secenek";
		this.stok_reyon_kodu_secenek.Size = new System.Drawing.Size(133, 21);
		this.stok_reyon_kodu_secenek.TabIndex = 195;
		this.stok_reyon_kodu_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.stok_reyon_kodu_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_reyon_kodu_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl29.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl29.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl29.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl29.Location = new System.Drawing.Point(52, 164);
		this.labelControl29.Name = "labelControl29";
		this.labelControl29.Size = new System.Drawing.Size(156, 19);
		this.labelControl29.TabIndex = 193;
		this.labelControl29.Text = "Stok reyonları :";
		this.labelControl22.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl22.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl22.Location = new System.Drawing.Point(491, 140);
		this.labelControl22.Name = "labelControl22";
		this.labelControl22.Size = new System.Drawing.Size(222, 13);
		this.labelControl22.TabIndex = 192;
		this.labelControl22.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.stok_marka_kodlari.Location = new System.Drawing.Point(353, 137);
		this.stok_marka_kodlari.Name = "stok_marka_kodlari";
		this.stok_marka_kodlari.Size = new System.Drawing.Size(132, 20);
		this.stok_marka_kodlari.TabIndex = 191;
		this.stok_marka_kodlari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_marka_kodlari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.stok_marka_kodu_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.stok_marka_kodu_secenek.FormattingEnabled = true;
		this.stok_marka_kodu_secenek.Location = new System.Drawing.Point(214, 137);
		this.stok_marka_kodu_secenek.Name = "stok_marka_kodu_secenek";
		this.stok_marka_kodu_secenek.Size = new System.Drawing.Size(133, 21);
		this.stok_marka_kodu_secenek.TabIndex = 187;
		this.stok_marka_kodu_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.stok_marka_kodu_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_marka_kodu_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl25.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl25.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl25.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl25.Location = new System.Drawing.Point(52, 137);
		this.labelControl25.Name = "labelControl25";
		this.labelControl25.Size = new System.Drawing.Size(156, 19);
		this.labelControl25.TabIndex = 185;
		this.labelControl25.Text = "Stok markaları :";
		this.labelControl16.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl16.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl16.Location = new System.Drawing.Point(491, 113);
		this.labelControl16.Name = "labelControl16";
		this.labelControl16.Size = new System.Drawing.Size(222, 13);
		this.labelControl16.TabIndex = 184;
		this.labelControl16.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.stok_uretici_kodlari.Location = new System.Drawing.Point(353, 110);
		this.stok_uretici_kodlari.Name = "stok_uretici_kodlari";
		this.stok_uretici_kodlari.Size = new System.Drawing.Size(132, 20);
		this.stok_uretici_kodlari.TabIndex = 183;
		this.stok_uretici_kodlari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_uretici_kodlari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.stok_uretici_kodu_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.stok_uretici_kodu_secenek.FormattingEnabled = true;
		this.stok_uretici_kodu_secenek.Location = new System.Drawing.Point(214, 110);
		this.stok_uretici_kodu_secenek.Name = "stok_uretici_kodu_secenek";
		this.stok_uretici_kodu_secenek.Size = new System.Drawing.Size(133, 21);
		this.stok_uretici_kodu_secenek.TabIndex = 179;
		this.stok_uretici_kodu_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.stok_uretici_kodu_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_uretici_kodu_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl20.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl20.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl20.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl20.Location = new System.Drawing.Point(52, 110);
		this.labelControl20.Name = "labelControl20";
		this.labelControl20.Size = new System.Drawing.Size(156, 19);
		this.labelControl20.TabIndex = 177;
		this.labelControl20.Text = "Stok üreticileri :";
		this.labelControl9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl9.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl9.Location = new System.Drawing.Point(491, 86);
		this.labelControl9.Name = "labelControl9";
		this.labelControl9.Size = new System.Drawing.Size(222, 13);
		this.labelControl9.TabIndex = 176;
		this.labelControl9.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.stok_ana_gruplari.Location = new System.Drawing.Point(353, 83);
		this.stok_ana_gruplari.Name = "stok_ana_gruplari";
		this.stok_ana_gruplari.Size = new System.Drawing.Size(132, 20);
		this.stok_ana_gruplari.TabIndex = 175;
		this.stok_ana_gruplari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_ana_gruplari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.stok_ana_gruplari_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.stok_ana_gruplari_secenek.FormattingEnabled = true;
		this.stok_ana_gruplari_secenek.Location = new System.Drawing.Point(214, 83);
		this.stok_ana_gruplari_secenek.Name = "stok_ana_gruplari_secenek";
		this.stok_ana_gruplari_secenek.Size = new System.Drawing.Size(133, 21);
		this.stok_ana_gruplari_secenek.TabIndex = 171;
		this.stok_ana_gruplari_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.stok_ana_gruplari_secenek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_ana_gruplari_secenek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl15.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl15.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl15.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl15.Location = new System.Drawing.Point(52, 83);
		this.labelControl15.Name = "labelControl15";
		this.labelControl15.Size = new System.Drawing.Size(156, 19);
		this.labelControl15.TabIndex = 169;
		this.labelControl15.Text = "Stok ana grupları :";
		this.stok_arama_metin.Location = new System.Drawing.Point(353, 56);
		this.stok_arama_metin.Name = "stok_arama_metin";
		this.stok_arama_metin.Size = new System.Drawing.Size(132, 20);
		this.stok_arama_metin.TabIndex = 168;
		this.stok_arama_metin.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_arama_metin.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.stok_arama_secenekleri.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.stok_arama_secenekleri.FormattingEnabled = true;
		this.stok_arama_secenekleri.Location = new System.Drawing.Point(214, 56);
		this.stok_arama_secenekleri.Name = "stok_arama_secenekleri";
		this.stok_arama_secenekleri.Size = new System.Drawing.Size(133, 21);
		this.stok_arama_secenekleri.TabIndex = 164;
		this.stok_arama_secenekleri.SelectedIndexChanged += new System.EventHandler(Arama_SelectedIndexChanged);
		this.stok_arama_secenekleri.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.stok_arama_secenekleri.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl12.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl12.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl12.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl12.Location = new System.Drawing.Point(52, 56);
		this.labelControl12.Name = "labelControl12";
		this.labelControl12.Size = new System.Drawing.Size(156, 19);
		this.labelControl12.TabIndex = 162;
		this.labelControl12.Text = "Stok arama :";
		this.lb_kullanicilar.Location = new System.Drawing.Point(12, 58);
		this.lb_kullanicilar.Name = "lb_kullanicilar";
		this.lb_kullanicilar.Size = new System.Drawing.Size(127, 610);
		this.lb_kullanicilar.TabIndex = 5;
		this.lb_kullanicilar.SelectedValueChanged += new System.EventHandler(lb_kullanicilar_SelectedValueChanged);
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.dosyaToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(934, 24);
		this.menuStrip1.TabIndex = 9;
		this.menuStrip1.Text = "menuStrip1";
		this.label718.AutoSize = true;
		this.label718.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.label718.Location = new System.Drawing.Point(12, 41);
		this.label718.Name = "label718";
		this.label718.Size = new System.Drawing.Size(59, 14);
		this.label718.TabIndex = 106;
		this.label718.Text = "Raporlar";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.kaydetToolStripMenuItem, this.ekleToolStripMenuItem, this.silToolStripMenuItem, this.toolStripSeparator1, this.dosyayaYazToolStripMenuItem, this.dosyadanOkuToolStripMenuItem });
		this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
		this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
		this.dosyaToolStripMenuItem.Text = "Dosya";
		this.kaydetToolStripMenuItem.Name = "kaydetToolStripMenuItem";
		this.kaydetToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.kaydetToolStripMenuItem.Text = "Kaydet";
		this.kaydetToolStripMenuItem.Click += new System.EventHandler(kaydetToolStripMenuItem_Click);
		this.ekleToolStripMenuItem.Name = "ekleToolStripMenuItem";
		this.ekleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.ekleToolStripMenuItem.Text = "Ekle";
		this.ekleToolStripMenuItem.Click += new System.EventHandler(ekleToolStripMenuItem_Click);
		this.silToolStripMenuItem.Name = "silToolStripMenuItem";
		this.silToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.silToolStripMenuItem.Text = "Sil";
		this.silToolStripMenuItem.Click += new System.EventHandler(silToolStripMenuItem_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
		this.dosyayaYazToolStripMenuItem.Name = "dosyayaYazToolStripMenuItem";
		this.dosyayaYazToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.dosyayaYazToolStripMenuItem.Text = "Dosyaya yaz";
		this.dosyayaYazToolStripMenuItem.Click += new System.EventHandler(dosyayaYazToolStripMenuItem_Click);
		this.dosyadanOkuToolStripMenuItem.Name = "dosyadanOkuToolStripMenuItem";
		this.dosyadanOkuToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.dosyadanOkuToolStripMenuItem.Text = "Dosyadan oku";
		this.dosyadanOkuToolStripMenuItem.Click += new System.EventHandler(dosyadanOkuToolStripMenuItem_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(934, 673);
		base.Controls.Add(this.label718);
		base.Controls.Add(this.lb_kullanicilar);
		base.Controls.Add(this.tc_parametrevehaklar);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "MobilRaporStokSiparisDuzenleme";
		this.Text = "Stok Sipariş Raporu Düzenleme";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		((System.ComponentModel.ISupportInitialize)this.tc_parametrevehaklar).EndInit();
		this.tc_parametrevehaklar.ResumeLayout(false);
		this.xtraTabPage2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_miktar3_secenek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_miktar2_secenek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_miktar1_secenek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_tutar3_secenek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_tutar2_secenek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_tutar1_secenek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.RaporAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_siralama_secenegi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_gruplandirma_secenegi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_teslim_durumu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_tarih_cinsi.Properties).EndInit();
		this.xtraTabPage15.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.sorumluluk_merkezleri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.depolar.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.proje_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_temsilci_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_grup_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_bolge_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_arama_metin.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.stok_kategori_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.stok_reyon_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.stok_marka_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.stok_uretici_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.stok_ana_gruplari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.stok_arama_metin.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
