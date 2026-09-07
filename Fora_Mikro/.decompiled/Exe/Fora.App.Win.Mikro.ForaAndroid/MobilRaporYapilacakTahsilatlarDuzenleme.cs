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
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using Fora.App.Win.Mikro.GenelFormlar;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Rapor.Genel;
using Fora.Mikro.Rapor.YapilacakTahsilat;
using Newtonsoft.Json;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class MobilRaporYapilacakTahsilatlarDuzenleme : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _raporparametreleri = new Parametreler();

	private bool DegisiklikVar;

	private string AktifKullanici;

	private RaporYapilacakTahsilatlarSecenekleri AktifRapor;

	private IContainer components;

	private XtraTabControl tc_parametrevehaklar;

	private XtraTabPage xtraTabPage2;

	private ListBoxControl lb_kullanicilar;

	private LabelControl labelControl57;

	private System.Windows.Forms.ComboBox vade_cinsi;

	private CheckEdit degistirebilir_vade_cinsi;

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

	private XtraTabPage xtraTabPage15;

	private LabelControl labelControl107;

	private TextEdit sorumluluk_merkezleri;

	private System.Windows.Forms.ComboBox sorumluluk_merkezleri_secenek;

	private LabelControl labelControl110;

	private LabelControl labelControl97;

	private TextEdit proje_kodlari;

	private System.Windows.Forms.ComboBox proje_kodlari_secenek;

	private LabelControl labelControl106;

	private LabelControl labelControl47;

	private TextEdit cari_grup_kodlari;

	private System.Windows.Forms.ComboBox cari_grup_kodu_secenek;

	private LabelControl labelControl50;

	private LabelControl labelControl34;

	private TextEdit cari_bolge_kodlari;

	private System.Windows.Forms.ComboBox cari_bolge_kodu_secenek;

	private LabelControl labelControl43;

	private LabelControl labelControl51;

	private TextEdit cari_temsilci_kodlari;

	private System.Windows.Forms.ComboBox cari_temsilci_kodu_secenek;

	private LabelControl labelControl54;

	private TextEdit cari_arama_metin;

	private System.Windows.Forms.ComboBox cari_arama_secenekleri;

	private LabelControl labelControl46;

	private LabelControl labelControl8;

	private TextEdit doviz_cinsleri;

	private System.Windows.Forms.ComboBox doviz_cinsi_secenek;

	private LabelControl labelControl9;

	private LabelControl labelControl6;

	private TextEdit sube_nolari;

	private System.Windows.Forms.ComboBox sube_secenek;

	private LabelControl labelControl7;

	private LabelControl labelControl4;

	private TextEdit firma_nolari;

	private System.Windows.Forms.ComboBox firma_secenek;

	private LabelControl labelControl5;

	private CheckEdit degistirebilir_minimum_bakiye;

	private LabelControl labelControl12;

	private CheckEdit degistirebilir_proje_detayli;

	private LabelControl labelControl11;

	private System.Windows.Forms.ComboBox proje_detayli;

	private CheckEdit degistirebilir_sorumluluk_merkezi_detayli;

	private LabelControl labelControl10;

	private System.Windows.Forms.ComboBox sorumluluk_merkezi_detayli;

	private SpinEdit minimum_bakiye;

	private Label label718;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem kaydetToolStripMenuItem;

	private ToolStripMenuItem ekleToolStripMenuItem;

	private ToolStripMenuItem silToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem dosyayaYazToolStripMenuItem;

	private ToolStripMenuItem dosyadanOkuToolStripMenuItem;

	public MobilRaporYapilacakTahsilatlarDuzenleme(MikroUygulamaBilgileri mikrouygulamabilgileri)
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
				new object[2] { 32, "Tüm Zamanlar" },
				new object[2] { 2, "Dün" },
				new object[2] { 1, "Bugün" },
				new object[2] { 18, "Yarın" },
				new object[2] { 4, "Bu Hafta" },
				new object[2] { 7, "Bu Ay" },
				new object[2] { 10, "Bu Yıl" },
				new object[2] { 5, "Geçen Hafta" },
				new object[2] { 8, "Geçen Ay" },
				new object[2] { 11, "Geçen Yıl" },
				new object[2] { 13, "Geçen 3 Ay" },
				new object[2] { 16, "Geçen 6 Ay" },
				new object[2] { 17, "Geçen 12 Ay" },
				new object[2] { 6, "Geçen 7 Gün" },
				new object[2] { 31, "Geçen 15 Gün" },
				new object[2] { 9, "Geçen 30 Gün" },
				new object[2] { 14, "Geçen 60 Gün" },
				new object[2] { 15, "Geçen 90 Gün" },
				new object[2] { 12, "Geçen 365 Gün" },
				new object[2] { 19, "Gelecek Hafta" },
				new object[2] { 26, "Gelecek Ay" },
				new object[2] { 30, "Gelecek Yıl" },
				new object[2] { 27, "Gelecek 3 Ay" },
				new object[2] { 28, "Gelecek 6 Ay" },
				new object[2] { 29, "Gelecek 12 Ay" },
				new object[2] { 20, "Gelecek 7 Gün" },
				new object[2] { 21, "Gelecek 15 Gün" },
				new object[2] { 22, "Gelecek 30 Gün" },
				new object[2] { 23, "Gelecek 60 Gün" },
				new object[2] { 24, "Gelecek 90 Gün" },
				new object[2] { 25, "Gelecek 365 Gün" }
			}
		};
		vade_cinsi.DataSource = dataSource;
		vade_cinsi.ValueMember = "ID";
		vade_cinsi.DisplayMember = "Isim";
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
				new object[2] { 0, "Cari" },
				new object[2] { 1, "Ay" },
				new object[2] { 2, "Gün" },
				new object[2] { 3, "Temsilci" },
				new object[2] { 4, "Cari bölge" },
				new object[2] { 5, "Cari grupları" },
				new object[2] { 6, "Firma" },
				new object[2] { 7, "Şube" },
				new object[2] { 8, "Döviz cinsi" },
				new object[2] { 9, "Sorumluluk merkezi" },
				new object[2] { 10, "Proje kodu" }
			}
		};
		gruplandirma_secenegi.DataSource = dataSource2;
		gruplandirma_secenegi.ValueMember = "ID";
		gruplandirma_secenegi.DisplayMember = "Isim";
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
				new object[2] { 0, "Kod" },
				new object[2] { 1, "İsim" },
				new object[2] { 2, "Tutar" },
				new object[2] { 3, "Ortalama vade" }
			}
		};
		siralama_secenegi.DataSource = dataSource3;
		siralama_secenegi.ValueMember = "ID";
		siralama_secenegi.DisplayMember = "Isim";
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
		proje_kodlari_secenek.DataSource = dataSource4;
		proje_kodlari_secenek.ValueMember = "ID";
		proje_kodlari_secenek.DisplayMember = "Isim";
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
		sorumluluk_merkezleri_secenek.DataSource = dataSource5;
		sorumluluk_merkezleri_secenek.ValueMember = "ID";
		sorumluluk_merkezleri_secenek.DisplayMember = "Isim";
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
		cari_bolge_kodu_secenek.DataSource = dataSource6;
		cari_bolge_kodu_secenek.ValueMember = "ID";
		cari_bolge_kodu_secenek.DisplayMember = "Isim";
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
		cari_grup_kodu_secenek.DataSource = dataSource7;
		cari_grup_kodu_secenek.ValueMember = "ID";
		cari_grup_kodu_secenek.DisplayMember = "Isim";
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
		cari_temsilci_kodu_secenek.DataSource = dataSource8;
		cari_temsilci_kodu_secenek.ValueMember = "ID";
		cari_temsilci_kodu_secenek.DisplayMember = "Isim";
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
		firma_secenek.DataSource = dataSource9;
		firma_secenek.ValueMember = "ID";
		firma_secenek.DisplayMember = "Isim";
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
		sube_secenek.DataSource = dataSource10;
		sube_secenek.ValueMember = "ID";
		sube_secenek.DisplayMember = "Isim";
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
				new object[2] { 2, "Listeden seç" }
			}
		};
		doviz_cinsi_secenek.DataSource = dataSource11;
		doviz_cinsi_secenek.ValueMember = "ID";
		doviz_cinsi_secenek.DisplayMember = "Isim";
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
				new object[2] { 0, "Tüm cariler" },
				new object[2] { 1, "Eşittir" },
				new object[2] { 2, "İle başlar" },
				new object[2] { 3, "İçerir" },
				new object[2] { 4, "Tanımlı olan" }
			}
		};
		cari_arama_secenekleri.DataSource = dataSource12;
		cari_arama_secenekleri.ValueMember = "ID";
		cari_arama_secenekleri.DisplayMember = "Isim";
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
				new object[2] { 0, "Hayır" },
				new object[2] { 1, "Evet" }
			}
		};
		sorumluluk_merkezi_detayli.DataSource = dataSource13;
		sorumluluk_merkezi_detayli.ValueMember = "ID";
		sorumluluk_merkezi_detayli.DisplayMember = "Isim";
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
				new object[2] { 0, "Hayır" },
				new object[2] { 1, "Evet" }
			}
		};
		proje_detayli.DataSource = dataSource14;
		proje_detayli.ValueMember = "ID";
		proje_detayli.DisplayMember = "Isim";
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		proje_kodlari.Enabled = false;
		sorumluluk_merkezleri.Enabled = false;
		cari_bolge_kodlari.Enabled = false;
		cari_grup_kodlari.Enabled = false;
		cari_temsilci_kodlari.Enabled = false;
		cari_arama_metin.Enabled = false;
		doviz_cinsleri.Enabled = false;
		sube_nolari.Enabled = false;
		firma_nolari.Enabled = false;
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		KullanicilariListele();
	}

	private void KullanicilariListele()
	{
		lb_kullanicilar.Items.Clear();
		List<string> list = new List<string>();
		string commandText = "SELECT ParametreUser FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram='MobilRaporYapilacakTahsilatlar' GROUP BY ParametreUser ORDER BY ParametreUser";
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
			_raporparametreleri = ParametrelerDefault.MobilRaporYapilacakTahsilatlar(AktifKullanici);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _raporparametreleri, "MobilRaporYapilacakTahsilatlar", AktifKullanici, "", "");
			AktifRapor = JsonConvert.DeserializeObject<RaporYapilacakTahsilatlarSecenekleri>(_raporparametreleri._GetParametre("RaporJson")._GetString);
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
		degistirebilir_vade_cinsi.Checked = AktifRapor.degistirebilir_tarih_cinsi;
		vade_cinsi.SelectedValue = (int)AktifRapor.tarih_cinsi;
		degistirebilir_gruplandirma_secenegi.Checked = AktifRapor.degistirebilir_gruplandirma_secenegi;
		gruplandirma_secenegi.SelectedValue = (int)AktifRapor.gruplandirma_secenegi;
		degistirebilir_siralama_secenegi.Checked = AktifRapor.degistirebilir_siralama_secenegi;
		siralama_secenegi.SelectedValue = (int)AktifRapor.siralama_secenegi;
		proje_kodlari_secenek.SelectedValue = (int)AktifRapor.proje_kodlari_secenek;
		proje_kodlari.Text = AktifRapor.proje_kodlari;
		sorumluluk_merkezleri_secenek.SelectedValue = (int)AktifRapor.sorumluluk_merkezleri_secenek;
		sorumluluk_merkezleri.Text = AktifRapor.sorumluluk_merkezleri;
		cari_arama_secenekleri.SelectedValue = (int)AktifRapor.cari_arama_secenekleri;
		cari_arama_metin.Text = AktifRapor.cari_arama_metin;
		cari_bolge_kodu_secenek.SelectedValue = (int)AktifRapor.cari_bolge_kodu_secenek;
		cari_bolge_kodlari.Text = AktifRapor.cari_bolge_kodlari;
		cari_grup_kodu_secenek.SelectedValue = (int)AktifRapor.cari_grup_kodu_secenek;
		cari_grup_kodlari.Text = AktifRapor.cari_grup_kodlari;
		cari_temsilci_kodu_secenek.SelectedValue = (int)AktifRapor.cari_temsilci_kodu_secenek;
		cari_temsilci_kodlari.Text = AktifRapor.cari_temsilci_kodlari;
		firma_secenek.SelectedValue = (int)AktifRapor.firma_secenek;
		firma_nolari.Text = AktifRapor.firma_nolari;
		sube_secenek.SelectedValue = (int)AktifRapor.sube_secenek;
		sube_nolari.Text = AktifRapor.sube_nolari;
		doviz_cinsi_secenek.SelectedValue = (int)AktifRapor.doviz_cinsi_secenek;
		doviz_cinsleri.Text = AktifRapor.doviz_cinsleri;
		degistirebilir_sorumluluk_merkezi_detayli.Checked = AktifRapor.degistirebilir_sorumluluk_merkezi_detayli;
		if (!AktifRapor.SorumlulukMerkeziDetayli)
		{
			sorumluluk_merkezi_detayli.SelectedValue = 0;
		}
		else
		{
			sorumluluk_merkezi_detayli.SelectedValue = 1;
		}
		degistirebilir_proje_detayli.Checked = AktifRapor.degistirebilir_proje_detayli;
		if (!AktifRapor.ProjeDetayli)
		{
			proje_detayli.SelectedValue = 0;
		}
		else
		{
			proje_detayli.SelectedValue = 1;
		}
		degistirebilir_minimum_bakiye.Checked = AktifRapor.degistirebilir_minimum_bakiye;
		minimum_bakiye.Value = AktifRapor.MinimumBakiye;
	}

	private void KullaniciParametreKaydet()
	{
		te_kullanici_adi.Text = AktifKullanici;
		_raporparametreleri._GetParametre("RaporAdi")._SetString = RaporAdi.Text;
		AktifRapor.degistirebilir_tarih_cinsi = degistirebilir_vade_cinsi.Checked;
		AktifRapor.tarih_cinsi = (enum_tarih_cinsi)(int)vade_cinsi.SelectedValue;
		AktifRapor.proje_kodlari_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)proje_kodlari_secenek.SelectedValue;
		AktifRapor.proje_kodlari = proje_kodlari.Text;
		AktifRapor.sorumluluk_merkezleri_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)sorumluluk_merkezleri_secenek.SelectedValue;
		AktifRapor.sorumluluk_merkezleri = sorumluluk_merkezleri.Text;
		AktifRapor.cari_arama_secenekleri = (enum_cari_arama_secenekleri)(int)cari_arama_secenekleri.SelectedValue;
		AktifRapor.cari_arama_metin = cari_arama_metin.Text;
		AktifRapor.cari_bolge_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)cari_bolge_kodu_secenek.SelectedValue;
		AktifRapor.cari_bolge_kodlari = cari_bolge_kodlari.Text;
		AktifRapor.cari_grup_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)cari_grup_kodu_secenek.SelectedValue;
		AktifRapor.cari_grup_kodlari = cari_grup_kodlari.Text;
		AktifRapor.cari_temsilci_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)cari_temsilci_kodu_secenek.SelectedValue;
		AktifRapor.cari_temsilci_kodlari = cari_temsilci_kodlari.Text;
		AktifRapor.degistirebilir_gruplandirma_secenegi = degistirebilir_gruplandirma_secenegi.Checked;
		AktifRapor.gruplandirma_secenegi = (enum_yapilacak_tahsilatlar_gruplandirma_secenekleri)(int)gruplandirma_secenegi.SelectedValue;
		AktifRapor.degistirebilir_siralama_secenegi = degistirebilir_siralama_secenegi.Checked;
		AktifRapor.siralama_secenegi = (enum_yapilacak_tahsilatlar_siralama_secenekleri)(int)siralama_secenegi.SelectedValue;
		AktifRapor.firma_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)firma_secenek.SelectedValue;
		AktifRapor.firma_nolari = firma_nolari.Text;
		AktifRapor.sube_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)sube_secenek.SelectedValue;
		AktifRapor.sube_nolari = sube_nolari.Text;
		AktifRapor.doviz_cinsi_secenek = (enum_tumu_tanimli_olan_listeden_sec)(int)doviz_cinsi_secenek.SelectedValue;
		AktifRapor.doviz_cinsleri = doviz_cinsleri.Text;
		AktifRapor.degistirebilir_sorumluluk_merkezi_detayli = degistirebilir_sorumluluk_merkezi_detayli.Checked;
		if ((int)sorumluluk_merkezi_detayli.SelectedValue == 0)
		{
			AktifRapor.SorumlulukMerkeziDetayli = false;
		}
		else
		{
			AktifRapor.SorumlulukMerkeziDetayli = true;
		}
		AktifRapor.degistirebilir_proje_detayli = degistirebilir_proje_detayli.Checked;
		if ((int)proje_detayli.SelectedValue == 0)
		{
			AktifRapor.ProjeDetayli = false;
		}
		else
		{
			AktifRapor.ProjeDetayli = true;
		}
		AktifRapor.degistirebilir_minimum_bakiye = degistirebilir_minimum_bakiye.Checked;
		AktifRapor.MinimumBakiye = (int)minimum_bakiye.Value;
		_raporparametreleri._GetParametre("RaporJson")._SetString = JsonConvert.SerializeObject(AktifRapor);
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _raporparametreleri);
		_raporparametreleri = ParametrelerDefault.MobilRaporYapilacakTahsilatlar(AktifKullanici);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _raporparametreleri, "MobilRaporYapilacakTahsilatlar", AktifKullanici, "", "");
		AktifRapor = JsonConvert.DeserializeObject<RaporYapilacakTahsilatlarSecenekleri>(_raporparametreleri._GetParametre("RaporJson")._GetString);
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

	private void Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			System.Windows.Forms.ComboBox comboBox = (System.Windows.Forms.ComboBox)sender;
			TextEdit textEdit = null;
			switch (comboBox.Name)
			{
			case "proje_kodlari_secenek":
				textEdit = proje_kodlari;
				break;
			case "sorumluluk_merkezleri_secenek":
				textEdit = sorumluluk_merkezleri;
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
			case "firma_secenek":
				textEdit = firma_nolari;
				break;
			case "sube_secenek":
				textEdit = sube_nolari;
				break;
			case "doviz_cinsi_secenek":
				textEdit = doviz_cinsleri;
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
			if (name == "cari_arama_secenekleri")
			{
				textEdit = cari_arama_metin;
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
		MobilRaporYapilacakTahsilatlarEkle mobilRaporYapilacakTahsilatlarEkle = new MobilRaporYapilacakTahsilatlarEkle(_mikrouygulamabilgileri);
		mobilRaporYapilacakTahsilatlarEkle.ShowDialog();
		if (mobilRaporYapilacakTahsilatlarEkle.DialogResult == DialogResult.OK)
		{
			Parametreler parametreler = ParametrelerDefault.MobilRaporYapilacakTahsilatlar(mobilRaporYapilacakTahsilatlarEkle._RaporKodu);
			parametreler._GetParametre("RaporAdi")._SetString = mobilRaporYapilacakTahsilatlarEkle._RaporAdi;
			parametreler._GetParametre("RaporJson")._SetString = JsonConvert.SerializeObject(new RaporYapilacakTahsilatlarSecenekleri());
			ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler);
			KullanicilariListele();
			lb_kullanicilar.SelectedItem = mobilRaporYapilacakTahsilatlarEkle._RaporKodu;
		}
		mobilRaporYapilacakTahsilatlarEkle.Dispose();
		mobilRaporYapilacakTahsilatlarEkle = null;
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
		string commandText = "DELETE _FORA_PARAMETRELER WHERE ParametreProgram='MobilRaporYapilacakTahsilatlar' AND ParametreUser=@ParametreUser";
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
				_raporparametreleri.KullanimAlani = "MobilRaporYapilacakTahsilatlar";
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
		if (parametreler.KullanimAlani != "MobilRaporYapilacakTahsilatlar")
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
		this.minimum_bakiye = new DevExpress.XtraEditors.SpinEdit();
		this.degistirebilir_minimum_bakiye = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
		this.degistirebilir_proje_detayli = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
		this.proje_detayli = new System.Windows.Forms.ComboBox();
		this.degistirebilir_sorumluluk_merkezi_detayli = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
		this.sorumluluk_merkezi_detayli = new System.Windows.Forms.ComboBox();
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
		this.degistirebilir_vade_cinsi = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl57 = new DevExpress.XtraEditors.LabelControl();
		this.vade_cinsi = new System.Windows.Forms.ComboBox();
		this.xtraTabPage15 = new DevExpress.XtraTab.XtraTabPage();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		this.doviz_cinsleri = new DevExpress.XtraEditors.TextEdit();
		this.doviz_cinsi_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.sube_nolari = new DevExpress.XtraEditors.TextEdit();
		this.sube_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.firma_nolari = new DevExpress.XtraEditors.TextEdit();
		this.firma_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl107 = new DevExpress.XtraEditors.LabelControl();
		this.sorumluluk_merkezleri = new DevExpress.XtraEditors.TextEdit();
		this.sorumluluk_merkezleri_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl110 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl97 = new DevExpress.XtraEditors.LabelControl();
		this.proje_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.proje_kodlari_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl106 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl47 = new DevExpress.XtraEditors.LabelControl();
		this.cari_grup_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.cari_grup_kodu_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl50 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl34 = new DevExpress.XtraEditors.LabelControl();
		this.cari_bolge_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.cari_bolge_kodu_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl43 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl51 = new DevExpress.XtraEditors.LabelControl();
		this.cari_temsilci_kodlari = new DevExpress.XtraEditors.TextEdit();
		this.cari_temsilci_kodu_secenek = new System.Windows.Forms.ComboBox();
		this.labelControl54 = new DevExpress.XtraEditors.LabelControl();
		this.cari_arama_metin = new DevExpress.XtraEditors.TextEdit();
		this.cari_arama_secenekleri = new System.Windows.Forms.ComboBox();
		this.labelControl46 = new DevExpress.XtraEditors.LabelControl();
		this.lb_kullanicilar = new DevExpress.XtraEditors.ListBoxControl();
		this.label718 = new System.Windows.Forms.Label();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
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
		((System.ComponentModel.ISupportInitialize)this.minimum_bakiye.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_minimum_bakiye.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_proje_detayli.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_sorumluluk_merkezi_detayli.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.RaporAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_siralama_secenegi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_gruplandirma_secenegi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_vade_cinsi.Properties).BeginInit();
		this.xtraTabPage15.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.doviz_cinsleri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.sube_nolari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.firma_nolari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.sorumluluk_merkezleri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.proje_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_grup_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_bolge_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_temsilci_kodlari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_arama_metin.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.tc_parametrevehaklar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tc_parametrevehaklar.Enabled = false;
		this.tc_parametrevehaklar.Location = new System.Drawing.Point(144, 35);
		this.tc_parametrevehaklar.Name = "tc_parametrevehaklar";
		this.tc_parametrevehaklar.SelectedTabPage = this.xtraTabPage2;
		this.tc_parametrevehaklar.Size = new System.Drawing.Size(788, 638);
		this.tc_parametrevehaklar.TabIndex = 4;
		this.tc_parametrevehaklar.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[2] { this.xtraTabPage2, this.xtraTabPage15 });
		this.xtraTabPage2.Controls.Add(this.minimum_bakiye);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_minimum_bakiye);
		this.xtraTabPage2.Controls.Add(this.labelControl12);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_proje_detayli);
		this.xtraTabPage2.Controls.Add(this.labelControl11);
		this.xtraTabPage2.Controls.Add(this.proje_detayli);
		this.xtraTabPage2.Controls.Add(this.degistirebilir_sorumluluk_merkezi_detayli);
		this.xtraTabPage2.Controls.Add(this.labelControl10);
		this.xtraTabPage2.Controls.Add(this.sorumluluk_merkezi_detayli);
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
		this.xtraTabPage2.Controls.Add(this.degistirebilir_vade_cinsi);
		this.xtraTabPage2.Controls.Add(this.labelControl57);
		this.xtraTabPage2.Controls.Add(this.vade_cinsi);
		this.xtraTabPage2.Name = "xtraTabPage2";
		this.xtraTabPage2.Size = new System.Drawing.Size(782, 610);
		this.xtraTabPage2.Text = "Seçenekler";
		this.minimum_bakiye.EditValue = new decimal(new int[4]);
		this.minimum_bakiye.Location = new System.Drawing.Point(203, 265);
		this.minimum_bakiye.Name = "minimum_bakiye";
		this.minimum_bakiye.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.minimum_bakiye.Size = new System.Drawing.Size(202, 20);
		this.minimum_bakiye.TabIndex = 226;
		this.degistirebilir_minimum_bakiye.Location = new System.Drawing.Point(411, 265);
		this.degistirebilir_minimum_bakiye.Name = "degistirebilir_minimum_bakiye";
		this.degistirebilir_minimum_bakiye.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_minimum_bakiye.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_minimum_bakiye.TabIndex = 208;
		this.labelControl12.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl12.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl12.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl12.Location = new System.Drawing.Point(14, 265);
		this.labelControl12.Name = "labelControl12";
		this.labelControl12.Size = new System.Drawing.Size(183, 19);
		this.labelControl12.TabIndex = 207;
		this.labelControl12.Text = "Minimum bakiye :";
		this.degistirebilir_proje_detayli.Location = new System.Drawing.Point(411, 238);
		this.degistirebilir_proje_detayli.Name = "degistirebilir_proje_detayli";
		this.degistirebilir_proje_detayli.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_proje_detayli.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_proje_detayli.TabIndex = 205;
		this.labelControl11.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl11.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl11.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl11.Location = new System.Drawing.Point(14, 238);
		this.labelControl11.Name = "labelControl11";
		this.labelControl11.Size = new System.Drawing.Size(183, 19);
		this.labelControl11.TabIndex = 204;
		this.labelControl11.Text = "Proje detaylı :";
		this.proje_detayli.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.proje_detayli.FormattingEnabled = true;
		this.proje_detayli.Location = new System.Drawing.Point(203, 238);
		this.proje_detayli.Name = "proje_detayli";
		this.proje_detayli.Size = new System.Drawing.Size(202, 21);
		this.proje_detayli.TabIndex = 203;
		this.degistirebilir_sorumluluk_merkezi_detayli.Location = new System.Drawing.Point(411, 211);
		this.degistirebilir_sorumluluk_merkezi_detayli.Name = "degistirebilir_sorumluluk_merkezi_detayli";
		this.degistirebilir_sorumluluk_merkezi_detayli.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_sorumluluk_merkezi_detayli.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_sorumluluk_merkezi_detayli.TabIndex = 202;
		this.labelControl10.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl10.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl10.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl10.Location = new System.Drawing.Point(3, 211);
		this.labelControl10.Name = "labelControl10";
		this.labelControl10.Size = new System.Drawing.Size(194, 19);
		this.labelControl10.TabIndex = 201;
		this.labelControl10.Text = "Sorumluluk merkezi detaylı :";
		this.sorumluluk_merkezi_detayli.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.sorumluluk_merkezi_detayli.FormattingEnabled = true;
		this.sorumluluk_merkezi_detayli.Location = new System.Drawing.Point(203, 211);
		this.sorumluluk_merkezi_detayli.Name = "sorumluluk_merkezi_detayli";
		this.sorumluluk_merkezi_detayli.Size = new System.Drawing.Size(202, 21);
		this.sorumluluk_merkezi_detayli.TabIndex = 200;
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(14, 13);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(126, 19);
		this.labelControl3.TabIndex = 199;
		this.labelControl3.Text = "Rapor tanımı";
		this.RaporAdi.Location = new System.Drawing.Point(203, 62);
		this.RaporAdi.Name = "RaporAdi";
		this.RaporAdi.Size = new System.Drawing.Size(358, 20);
		this.RaporAdi.TabIndex = 198;
		this.RaporAdi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.RaporAdi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(80, 65);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(117, 13);
		this.labelControl2.TabIndex = 197;
		this.labelControl2.Text = "Rapor adı :";
		this.te_kullanici_adi.Enabled = false;
		this.te_kullanici_adi.Location = new System.Drawing.Point(203, 36);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.te_kullanici_adi.Properties.Appearance.Options.UseForeColor = true;
		this.te_kullanici_adi.Size = new System.Drawing.Size(106, 20);
		this.te_kullanici_adi.TabIndex = 196;
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(80, 39);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(117, 13);
		this.labelControl1.TabIndex = 195;
		this.labelControl1.Text = "Rapor kodu :";
		this.degistirebilir_siralama_secenegi.Location = new System.Drawing.Point(411, 184);
		this.degistirebilir_siralama_secenegi.Name = "degistirebilir_siralama_secenegi";
		this.degistirebilir_siralama_secenegi.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_siralama_secenegi.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_siralama_secenegi.TabIndex = 194;
		this.degistirebilir_siralama_secenegi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_siralama_secenegi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl111.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl111.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl111.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl111.Location = new System.Drawing.Point(14, 184);
		this.labelControl111.Name = "labelControl111";
		this.labelControl111.Size = new System.Drawing.Size(183, 19);
		this.labelControl111.TabIndex = 193;
		this.labelControl111.Text = "Sıralama :";
		this.siralama_secenegi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.siralama_secenegi.FormattingEnabled = true;
		this.siralama_secenegi.Location = new System.Drawing.Point(203, 184);
		this.siralama_secenegi.Name = "siralama_secenegi";
		this.siralama_secenegi.Size = new System.Drawing.Size(202, 21);
		this.siralama_secenegi.TabIndex = 192;
		this.siralama_secenegi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.siralama_secenegi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.degistirebilir_gruplandirma_secenegi.Location = new System.Drawing.Point(411, 157);
		this.degistirebilir_gruplandirma_secenegi.Name = "degistirebilir_gruplandirma_secenegi";
		this.degistirebilir_gruplandirma_secenegi.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_gruplandirma_secenegi.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_gruplandirma_secenegi.TabIndex = 190;
		this.degistirebilir_gruplandirma_secenegi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_gruplandirma_secenegi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl90.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl90.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl90.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl90.Location = new System.Drawing.Point(14, 157);
		this.labelControl90.Name = "labelControl90";
		this.labelControl90.Size = new System.Drawing.Size(183, 19);
		this.labelControl90.TabIndex = 189;
		this.labelControl90.Text = "Gruplandırma :";
		this.gruplandirma_secenegi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.gruplandirma_secenegi.FormattingEnabled = true;
		this.gruplandirma_secenegi.Location = new System.Drawing.Point(203, 157);
		this.gruplandirma_secenegi.Name = "gruplandirma_secenegi";
		this.gruplandirma_secenegi.Size = new System.Drawing.Size(202, 21);
		this.gruplandirma_secenegi.TabIndex = 188;
		this.gruplandirma_secenegi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.gruplandirma_secenegi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.degistirebilir_vade_cinsi.Location = new System.Drawing.Point(411, 130);
		this.degistirebilir_vade_cinsi.Name = "degistirebilir_vade_cinsi";
		this.degistirebilir_vade_cinsi.Properties.Caption = "Değiştirebilir";
		this.degistirebilir_vade_cinsi.Size = new System.Drawing.Size(84, 19);
		this.degistirebilir_vade_cinsi.TabIndex = 146;
		this.degistirebilir_vade_cinsi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.degistirebilir_vade_cinsi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl57.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl57.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl57.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl57.Location = new System.Drawing.Point(14, 130);
		this.labelControl57.Name = "labelControl57";
		this.labelControl57.Size = new System.Drawing.Size(183, 19);
		this.labelControl57.TabIndex = 145;
		this.labelControl57.Text = "Vade :";
		this.vade_cinsi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.vade_cinsi.FormattingEnabled = true;
		this.vade_cinsi.Location = new System.Drawing.Point(203, 130);
		this.vade_cinsi.Name = "vade_cinsi";
		this.vade_cinsi.Size = new System.Drawing.Size(202, 21);
		this.vade_cinsi.TabIndex = 144;
		this.vade_cinsi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.vade_cinsi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.xtraTabPage15.Controls.Add(this.labelControl8);
		this.xtraTabPage15.Controls.Add(this.doviz_cinsleri);
		this.xtraTabPage15.Controls.Add(this.doviz_cinsi_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl9);
		this.xtraTabPage15.Controls.Add(this.labelControl6);
		this.xtraTabPage15.Controls.Add(this.sube_nolari);
		this.xtraTabPage15.Controls.Add(this.sube_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl7);
		this.xtraTabPage15.Controls.Add(this.labelControl4);
		this.xtraTabPage15.Controls.Add(this.firma_nolari);
		this.xtraTabPage15.Controls.Add(this.firma_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl5);
		this.xtraTabPage15.Controls.Add(this.labelControl107);
		this.xtraTabPage15.Controls.Add(this.sorumluluk_merkezleri);
		this.xtraTabPage15.Controls.Add(this.sorumluluk_merkezleri_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl110);
		this.xtraTabPage15.Controls.Add(this.labelControl97);
		this.xtraTabPage15.Controls.Add(this.proje_kodlari);
		this.xtraTabPage15.Controls.Add(this.proje_kodlari_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl106);
		this.xtraTabPage15.Controls.Add(this.labelControl47);
		this.xtraTabPage15.Controls.Add(this.cari_grup_kodlari);
		this.xtraTabPage15.Controls.Add(this.cari_grup_kodu_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl50);
		this.xtraTabPage15.Controls.Add(this.labelControl34);
		this.xtraTabPage15.Controls.Add(this.cari_bolge_kodlari);
		this.xtraTabPage15.Controls.Add(this.cari_bolge_kodu_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl43);
		this.xtraTabPage15.Controls.Add(this.labelControl51);
		this.xtraTabPage15.Controls.Add(this.cari_temsilci_kodlari);
		this.xtraTabPage15.Controls.Add(this.cari_temsilci_kodu_secenek);
		this.xtraTabPage15.Controls.Add(this.labelControl54);
		this.xtraTabPage15.Controls.Add(this.cari_arama_metin);
		this.xtraTabPage15.Controls.Add(this.cari_arama_secenekleri);
		this.xtraTabPage15.Controls.Add(this.labelControl46);
		this.xtraTabPage15.Name = "xtraTabPage15";
		this.xtraTabPage15.Size = new System.Drawing.Size(772, 603);
		this.xtraTabPage15.Text = "Filitreler";
		this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl8.Location = new System.Drawing.Point(476, 218);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(222, 13);
		this.labelControl8.TabIndex = 259;
		this.labelControl8.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.doviz_cinsleri.Location = new System.Drawing.Point(338, 215);
		this.doviz_cinsleri.Name = "doviz_cinsleri";
		this.doviz_cinsleri.Size = new System.Drawing.Size(132, 20);
		this.doviz_cinsleri.TabIndex = 258;
		this.doviz_cinsi_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.doviz_cinsi_secenek.FormattingEnabled = true;
		this.doviz_cinsi_secenek.Location = new System.Drawing.Point(199, 215);
		this.doviz_cinsi_secenek.Name = "doviz_cinsi_secenek";
		this.doviz_cinsi_secenek.Size = new System.Drawing.Size(133, 21);
		this.doviz_cinsi_secenek.TabIndex = 257;
		this.doviz_cinsi_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.labelControl9.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl9.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl9.Location = new System.Drawing.Point(50, 215);
		this.labelControl9.Name = "labelControl9";
		this.labelControl9.Size = new System.Drawing.Size(143, 19);
		this.labelControl9.TabIndex = 256;
		this.labelControl9.Text = "Döviz cinsi :";
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(475, 191);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(222, 13);
		this.labelControl6.TabIndex = 255;
		this.labelControl6.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.sube_nolari.Location = new System.Drawing.Point(337, 188);
		this.sube_nolari.Name = "sube_nolari";
		this.sube_nolari.Size = new System.Drawing.Size(132, 20);
		this.sube_nolari.TabIndex = 254;
		this.sube_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.sube_secenek.FormattingEnabled = true;
		this.sube_secenek.Location = new System.Drawing.Point(198, 188);
		this.sube_secenek.Name = "sube_secenek";
		this.sube_secenek.Size = new System.Drawing.Size(133, 21);
		this.sube_secenek.TabIndex = 253;
		this.sube_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.labelControl7.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl7.Location = new System.Drawing.Point(49, 188);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(143, 19);
		this.labelControl7.TabIndex = 252;
		this.labelControl7.Text = "Şube :";
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(475, 164);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(222, 13);
		this.labelControl4.TabIndex = 251;
		this.labelControl4.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.firma_nolari.Location = new System.Drawing.Point(337, 161);
		this.firma_nolari.Name = "firma_nolari";
		this.firma_nolari.Size = new System.Drawing.Size(132, 20);
		this.firma_nolari.TabIndex = 250;
		this.firma_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.firma_secenek.FormattingEnabled = true;
		this.firma_secenek.Location = new System.Drawing.Point(198, 161);
		this.firma_secenek.Name = "firma_secenek";
		this.firma_secenek.Size = new System.Drawing.Size(133, 21);
		this.firma_secenek.TabIndex = 249;
		this.firma_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(49, 161);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(143, 19);
		this.labelControl5.TabIndex = 248;
		this.labelControl5.Text = "Firma :";
		this.labelControl107.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl107.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl107.Location = new System.Drawing.Point(476, 272);
		this.labelControl107.Name = "labelControl107";
		this.labelControl107.Size = new System.Drawing.Size(222, 13);
		this.labelControl107.TabIndex = 247;
		this.labelControl107.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.sorumluluk_merkezleri.Location = new System.Drawing.Point(336, 269);
		this.sorumluluk_merkezleri.Name = "sorumluluk_merkezleri";
		this.sorumluluk_merkezleri.Size = new System.Drawing.Size(132, 20);
		this.sorumluluk_merkezleri.TabIndex = 246;
		this.sorumluluk_merkezleri_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.sorumluluk_merkezleri_secenek.FormattingEnabled = true;
		this.sorumluluk_merkezleri_secenek.Location = new System.Drawing.Point(197, 269);
		this.sorumluluk_merkezleri_secenek.Name = "sorumluluk_merkezleri_secenek";
		this.sorumluluk_merkezleri_secenek.Size = new System.Drawing.Size(133, 21);
		this.sorumluluk_merkezleri_secenek.TabIndex = 242;
		this.sorumluluk_merkezleri_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.labelControl110.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl110.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl110.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl110.Location = new System.Drawing.Point(49, 269);
		this.labelControl110.Name = "labelControl110";
		this.labelControl110.Size = new System.Drawing.Size(143, 19);
		this.labelControl110.TabIndex = 240;
		this.labelControl110.Text = "Sorumluluk merkezi :";
		this.labelControl97.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl97.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl97.Location = new System.Drawing.Point(475, 245);
		this.labelControl97.Name = "labelControl97";
		this.labelControl97.Size = new System.Drawing.Size(222, 13);
		this.labelControl97.TabIndex = 239;
		this.labelControl97.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.proje_kodlari.Location = new System.Drawing.Point(337, 242);
		this.proje_kodlari.Name = "proje_kodlari";
		this.proje_kodlari.Size = new System.Drawing.Size(132, 20);
		this.proje_kodlari.TabIndex = 238;
		this.proje_kodlari_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.proje_kodlari_secenek.FormattingEnabled = true;
		this.proje_kodlari_secenek.Location = new System.Drawing.Point(198, 242);
		this.proje_kodlari_secenek.Name = "proje_kodlari_secenek";
		this.proje_kodlari_secenek.Size = new System.Drawing.Size(133, 21);
		this.proje_kodlari_secenek.TabIndex = 234;
		this.proje_kodlari_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.labelControl106.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl106.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl106.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl106.Location = new System.Drawing.Point(49, 242);
		this.labelControl106.Name = "labelControl106";
		this.labelControl106.Size = new System.Drawing.Size(143, 19);
		this.labelControl106.TabIndex = 232;
		this.labelControl106.Text = "Proje :";
		this.labelControl47.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl47.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl47.Location = new System.Drawing.Point(476, 104);
		this.labelControl47.Name = "labelControl47";
		this.labelControl47.Size = new System.Drawing.Size(222, 13);
		this.labelControl47.TabIndex = 231;
		this.labelControl47.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.cari_grup_kodlari.Location = new System.Drawing.Point(337, 100);
		this.cari_grup_kodlari.Name = "cari_grup_kodlari";
		this.cari_grup_kodlari.Size = new System.Drawing.Size(132, 20);
		this.cari_grup_kodlari.TabIndex = 230;
		this.cari_grup_kodu_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_grup_kodu_secenek.FormattingEnabled = true;
		this.cari_grup_kodu_secenek.Location = new System.Drawing.Point(199, 101);
		this.cari_grup_kodu_secenek.Name = "cari_grup_kodu_secenek";
		this.cari_grup_kodu_secenek.Size = new System.Drawing.Size(133, 21);
		this.cari_grup_kodu_secenek.TabIndex = 228;
		this.cari_grup_kodu_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.labelControl50.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl50.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl50.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl50.Location = new System.Drawing.Point(50, 101);
		this.labelControl50.Name = "labelControl50";
		this.labelControl50.Size = new System.Drawing.Size(142, 19);
		this.labelControl50.TabIndex = 226;
		this.labelControl50.Text = "Cari grupları :";
		this.labelControl34.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl34.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl34.Location = new System.Drawing.Point(476, 77);
		this.labelControl34.Name = "labelControl34";
		this.labelControl34.Size = new System.Drawing.Size(222, 13);
		this.labelControl34.TabIndex = 225;
		this.labelControl34.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.cari_bolge_kodlari.Location = new System.Drawing.Point(338, 74);
		this.cari_bolge_kodlari.Name = "cari_bolge_kodlari";
		this.cari_bolge_kodlari.Size = new System.Drawing.Size(132, 20);
		this.cari_bolge_kodlari.TabIndex = 224;
		this.cari_bolge_kodu_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_bolge_kodu_secenek.FormattingEnabled = true;
		this.cari_bolge_kodu_secenek.Location = new System.Drawing.Point(199, 74);
		this.cari_bolge_kodu_secenek.Name = "cari_bolge_kodu_secenek";
		this.cari_bolge_kodu_secenek.Size = new System.Drawing.Size(133, 21);
		this.cari_bolge_kodu_secenek.TabIndex = 222;
		this.cari_bolge_kodu_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.labelControl43.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl43.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl43.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl43.Location = new System.Drawing.Point(50, 74);
		this.labelControl43.Name = "labelControl43";
		this.labelControl43.Size = new System.Drawing.Size(143, 19);
		this.labelControl43.TabIndex = 220;
		this.labelControl43.Text = "Cari bölgeleri :";
		this.labelControl51.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl51.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl51.Location = new System.Drawing.Point(475, 50);
		this.labelControl51.Name = "labelControl51";
		this.labelControl51.Size = new System.Drawing.Size(222, 13);
		this.labelControl51.TabIndex = 219;
		this.labelControl51.Text = "Birden fazla seçeneği virgül ile ayırınız";
		this.cari_temsilci_kodlari.Location = new System.Drawing.Point(338, 47);
		this.cari_temsilci_kodlari.Name = "cari_temsilci_kodlari";
		this.cari_temsilci_kodlari.Size = new System.Drawing.Size(132, 20);
		this.cari_temsilci_kodlari.TabIndex = 218;
		this.cari_temsilci_kodu_secenek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_temsilci_kodu_secenek.FormattingEnabled = true;
		this.cari_temsilci_kodu_secenek.Location = new System.Drawing.Point(198, 47);
		this.cari_temsilci_kodu_secenek.Name = "cari_temsilci_kodu_secenek";
		this.cari_temsilci_kodu_secenek.Size = new System.Drawing.Size(133, 21);
		this.cari_temsilci_kodu_secenek.TabIndex = 216;
		this.cari_temsilci_kodu_secenek.SelectedIndexChanged += new System.EventHandler(Tumu_Tanimli_Olan_Listeden_Sec_SelectedIndexChanged);
		this.labelControl54.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl54.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl54.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl54.Location = new System.Drawing.Point(50, 47);
		this.labelControl54.Name = "labelControl54";
		this.labelControl54.Size = new System.Drawing.Size(142, 19);
		this.labelControl54.TabIndex = 214;
		this.labelControl54.Text = "Cari temsilcileri :";
		this.cari_arama_metin.Location = new System.Drawing.Point(338, 20);
		this.cari_arama_metin.Name = "cari_arama_metin";
		this.cari_arama_metin.Size = new System.Drawing.Size(132, 20);
		this.cari_arama_metin.TabIndex = 213;
		this.cari_arama_secenekleri.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_arama_secenekleri.FormattingEnabled = true;
		this.cari_arama_secenekleri.Location = new System.Drawing.Point(198, 20);
		this.cari_arama_secenekleri.Name = "cari_arama_secenekleri";
		this.cari_arama_secenekleri.Size = new System.Drawing.Size(133, 21);
		this.cari_arama_secenekleri.TabIndex = 211;
		this.cari_arama_secenekleri.SelectedIndexChanged += new System.EventHandler(Arama_SelectedIndexChanged);
		this.labelControl46.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl46.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl46.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl46.Location = new System.Drawing.Point(50, 20);
		this.labelControl46.Name = "labelControl46";
		this.labelControl46.Size = new System.Drawing.Size(143, 19);
		this.labelControl46.TabIndex = 209;
		this.labelControl46.Text = "Cari arama :";
		this.lb_kullanicilar.Location = new System.Drawing.Point(12, 58);
		this.lb_kullanicilar.Name = "lb_kullanicilar";
		this.lb_kullanicilar.Size = new System.Drawing.Size(126, 610);
		this.lb_kullanicilar.TabIndex = 5;
		this.lb_kullanicilar.SelectedValueChanged += new System.EventHandler(lb_kullanicilar_SelectedValueChanged);
		this.label718.AutoSize = true;
		this.label718.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.label718.Location = new System.Drawing.Point(12, 41);
		this.label718.Name = "label718";
		this.label718.Size = new System.Drawing.Size(59, 14);
		this.label718.TabIndex = 106;
		this.label718.Text = "Raporlar";
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.dosyaToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(934, 24);
		this.menuStrip1.TabIndex = 107;
		this.menuStrip1.Text = "menuStrip1";
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
		base.Name = "MobilRaporYapilacakTahsilatlarDuzenleme";
		this.Text = "Yapılacak Tahsilatlar Raporu Düzenleme";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		((System.ComponentModel.ISupportInitialize)this.tc_parametrevehaklar).EndInit();
		this.tc_parametrevehaklar.ResumeLayout(false);
		this.xtraTabPage2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.minimum_bakiye.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_minimum_bakiye.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_proje_detayli.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_sorumluluk_merkezi_detayli.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.RaporAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_siralama_secenegi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_gruplandirma_secenegi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.degistirebilir_vade_cinsi.Properties).EndInit();
		this.xtraTabPage15.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.doviz_cinsleri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.sube_nolari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.firma_nolari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.sorumluluk_merkezleri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.proje_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_grup_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_bolge_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_temsilci_kodlari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_arama_metin.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
