using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Fora.Mikro.Data.Sql;

namespace Fora.Mikro.Win.Form.DevEx.F10;

public class F10_Base_V16 : XtraForm
{
	private List<string> _ChooseListesi;

	private string _SeciliChoose;

	private MemoryStream defaultlayoutStream;

	protected List<Guid> _selecteditemsguids;

	protected MikroUygulamaBilgileri _mikrouygulamabilgileri;

	protected string _tag;

	protected string _gorunum;

	protected int _tableid;

	protected string _tabloadi;

	protected string _chooseprefix;

	protected string _defaultchoose;

	protected string _searchstring;

	protected bool _allowmultiselect;

	protected string _CustomWhereString;

	protected bool _AramaYapilabilir;

	protected bool _IlkAramaAktifOlsun;

	protected bool _sqlstringkullan;

	protected string _sqlstring;

	private SqlDataAdapter dataadapter;

	private DataTable datatable;

	private IContainer components;

	private GridControl gridControl1;

	private GridView gridView1;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem GorunumToolStripMenuItem;

	private ToolStripMenuItem GorunumuSaklaToolStripMenuItem;

	private ToolStripMenuItem gorunumukaydetToolStripMenuItem;

	private ToolStripMenuItem sorguCumlesiToolStripMenuItem;

	private Label label1;

	private TextEdit te_ara;

	private ToolStripMenuItem görünümüYükleToolStripMenuItem;

	private ToolStripMenuItem otomatikDosyadanYukleToolStripMenuItem;

	private ToolStripMenuItem otomatikDosyaSilToolStripMenuItem;

	private ToolStripMenuItem varsayilanaGeriDonToolStripMenuItem;

	private ToolStripMenuItem farkliDosyayaKaydetToolStripMenuItem;

	private SaveFileDialog saveFileDialog1;

	private OpenFileDialog openFileDialog1;

	private ToolStripMenuItem farkliDosyadanYukleToolStripMenuItem;

	private ToolStripMenuItem kolonlaraGoreGruplamaToolStripMenuItem;

	private ToolStripMenuItem kolonSeciciyiGosterToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private string FindDBName => _tabloadi switch
	{
		"VERI_TABANLARI" => _mikrouygulamabilgileri.MikroAnaDBName, 
		"KULLANICILAR" => _mikrouygulamabilgileri.MikroAnaDBName, 
		"DOVIZ_KURLARI" => _mikrouygulamabilgileri.MikroAnaDBName, 
		_ => _mikrouygulamabilgileri.MikroFirmaDBName, 
	};

	public F10_Base_V16()
	{
		InitializeComponent();
	}

	protected virtual void ItemSelected()
	{
	}

	private void F10_Base_Load(object sender, EventArgs e)
	{
		gridView1.Appearance.FocusedRow.BackColor = Color.FromArgb(50, 0, 0, 0);
		gridView1.OptionsSelection.MultiSelect = _allowmultiselect;
		_SeciliChoose = "";
		if (!_sqlstringkullan)
		{
			ChooseListesiOlustur();
			if (_ChooseListesi.Count > 0)
			{
				IEnumerable<string> source = _ChooseListesi.Where((string a) => a.Contains(_defaultchoose));
				if (source.Count() > 0)
				{
					_SeciliChoose = source.ElementAt(0);
				}
				else
				{
					_SeciliChoose = _ChooseListesi[0];
				}
			}
			sorgucumlesicheckayarlama();
		}
		else
		{
			sorguCumlesiToolStripMenuItem.Visible = false;
		}
		te_ara.Text = _searchstring;
		if (!_AramaYapilabilir)
		{
			te_ara.Enabled = false;
		}
		if (!_IlkAramaAktifOlsun)
		{
			GridOlustur();
		}
		else
		{
			te_ara.Select();
		}
	}

	private void gorunumukaydetToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!Directory.Exists("data"))
		{
			Directory.CreateDirectory("data");
		}
		gridView1.SaveLayoutToXml("data\\" + _tag + "_" + _SeciliChoose + "_otomatik.xml");
	}

	private void ChooseListesiOlustur()
	{
		string commandText = "select TABLE_NAME from INFORMATION_SCHEMA.VIEWS where TABLE_NAME like '" + _chooseprefix + "%'";
		_ChooseListesi = new List<string>();
		new SqlCommand().CommandText = commandText;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, FindDBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					_ChooseListesi.Add(sqlDataReader[0].ToString());
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		foreach (string item in _ChooseListesi)
		{
			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(item);
			toolStripMenuItem.Click += sorgucumlesiitem_Click;
			sorguCumlesiToolStripMenuItem.DropDownItems.Add(toolStripMenuItem);
		}
	}

	private void sorgucumlesicheckayarlama()
	{
		foreach (ToolStripMenuItem dropDownItem in sorguCumlesiToolStripMenuItem.DropDownItems)
		{
			if (dropDownItem.Text == _SeciliChoose)
			{
				dropDownItem.Checked = true;
			}
			else
			{
				dropDownItem.Checked = false;
			}
		}
	}

	private void sorgucumlesiitem_Click(object sender, EventArgs e)
	{
		string seciliChoose = ((ToolStripMenuItem)sender).Text;
		_SeciliChoose = seciliChoose;
		sorgucumlesicheckayarlama();
		GridOlustur();
	}

	private string inttomesaj(int integermesaj)
	{
		return integermesaj switch
		{
			1032 => "CARİ KODU", 
			1033 => "CARİ ÜNVANI", 
			1034 => "CARİ ÜNVANI 2", 
			77 => "TİPİ", 
			888 => "HAREKET TİPİ", 
			1530 => "BAKİYE veya HAREKET SAYISI", 
			88 => "KAYIT NO", 
			1035 => "SEKTÖR KODU", 
			135 => "GRUP KODU", 
			977 => "TEMSİLCİ KODU", 
			1036 => "BÖLGE KODU", 
			658 => "KAYIT TARİHİ", 
			1037 => "ANA CARİ KODU", 
			1029 => "VD. NO", 
			1028 => "VD. ADI", 
			1038 => "E-POSTA ADRESİ", 
			1039 => "CEP TELEFON NO", 
			443 => "FAKS", 
			78 => "KODU", 
			70 => "İSMİ", 
			1263 => "FİRMA NO", 
			822 => "ŞUBE", 
			771 => "HESAP NO", 
			849 => "DÖVİZ CİNSİ", 
			843 => "TCMB KODU", 
			1444 => "CARİ PERSONEL KODU", 
			1445 => "CARİ PERSONEL ADI", 
			1446 => "CARİ PERSONEL SOYADI", 
			2776 => "KASİYER", 
			873 => "DEPO NO", 
			427 => "HİZMET KODU", 
			901 => "HİZMET ADI", 
			955 => "KASA KODU", 
			956 => "KASA İSMİ", 
			954 => "KASA TİPİ", 
			133 => "HESAP KODU", 
			134 => "HESAP İSMİ", 
			870 => "ADI", 
			297 => "SON POZİSYON", 
			298 => "REFERANS NO", 
			299 => "ÇEK NO", 
			300 => "VADE TARİHİ", 
			293 => "TUTAR", 
			238 => "ÖDENEN", 
			301 => "KALAN", 
			254 => "DÖVİZ", 
			302 => "DÖVİZ KURU", 
			1413 => "SAHİBİ", 
			303 => "BANKASI", 
			304 => "ŞUBESİ", 
			305 => "İL KODU", 
			306 => "BANKALAR NO", 
			307 => "VERİLİŞ TARİHİ", 
			327 => "EVRAK NO", 
			212 => "SATIR NO", 
			199 => "CARİ CİNSİ", 
			200 => "CARİ KODU", 
			201 => "CARİ İSMİ", 
			308 => "CARİ DOVİZ CİNSİ", 
			1407 => "BORÇLU ADI", 
			1408 => "BANKA/ADRES1", 
			1409 => "ŞUBE/ADRES2", 
			1410 => "HESAP NO/ŞEHİR", 
			85 => "AÇIKLAMA", 
			190 => "MASRAF 1", 
			1411 => "MASRAF1 İŞLEME", 
			191 => "MASRAF 2", 
			1412 => "MASRAF2 İŞLEME", 
			309 => "KUR FARKI DEĞERLEME DURUMU", 
			1422 => "NEREDE CİNSİ", 
			1423 => "NEREDE KODU", 
			1424 => "NEREDE İSMİ", 
			1544 => "ÖDÜL KATKISI", 
			1652 => "ÖDL.KAT. İŞLENDİ", 
			1546 => "SERVİS KOMİSYON TUTARI", 
			1653 => "SRV.KOM. İŞLENDİ", 
			1548 => "ERKEN ÖDEME FAİZİ", 
			1654 => "ERK.ÖDM. İŞLENDİ", 
			1541 => "ÜYE İŞYERİ NO", 
			1550 => "KARTI TİPİ", 
			1551 => "TAKSİT SAYISI", 
			1552 => "TAKSİT NO", 
			118 => "SRM.MRK.KODU", 
			119 => "SRM.MRK.İSMİ", 
			1660 => "KREDİ KART NO", 
			6 => "FİYAT", 
			1173 => "DVZ", 
			165 => "MİKTAR", 
			46 => "ANA GRUP", 
			47 => "ALT GRUP", 
			1117 => "ÜRÜN SORUMLUSU", 
			1 => "STOK KODU", 
			2 => "STOK İSMİ", 
			1118 => "ÜRETİCİ KODU", 
			20 => "REYON KODU", 
			22 => "SEKTÖR KODU", 
			24 => "MARKA KODU", 
			26 => "MUHASEBE GRUP KODU", 
			28 => "AMBALAJ KODU", 
			30 => "KALİTE KONTROL KODU", 
			36 => "RENK KODU", 
			38 => "MODEL KODU", 
			1119 => "YIL SEZON KODU", 
			1120 => "ANAHAMMADDE KODU", 
			34 => "BEDEN KODU", 
			11 => "KATEGORİ KODU", 
			1121 => "KATEGORİ ADI", 
			1122 => "PRİM KODU", 
			1123 => "PRİM ADI", 
			2681 => "MODÜL", 
			2507 => "PROGRAM", 
			1361 => "VERİTABANI", 
			1107 => "NO", 
			1360 => "UZUN ADI", 
			1355 => "VERİTABANI KODU", 
			1356 => "VERİTABANI ADI", 
			341 => "TÜKETİCİ KODU", 
			1137 => "TÜKETİCİ ADI", 
			874 => "DEPO ADI", 
			3041 => "DEPO GRUP NO", 
			3042 => "DEPO GRUP İSMİ", 
			1935 => "DEPO TİPİ", 
			875 => "ŞUBE NO", 
			137 => "SORUMLULUK MERKEZİ", 
			_ => "", 
		};
	}

	private string mesajdegistir(string text)
	{
		int startIndex = text.IndexOf("msg_S_") + 6;
		string text2 = text.Substring(startIndex, 4);
		int result = -1;
		string text3 = "";
		if (!int.TryParse(text2, out result))
		{
			MessageBox.Show(text2 + " bir sayı değil.");
			return text;
		}
		text3 = inttomesaj(result);
		if (text3 != "")
		{
			text = text.Replace("msg_S_" + text2, text3);
		}
		return text;
	}

	private void GridOlustur()
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, FindDBName);
		string text = "";
		if (_sqlstringkullan)
		{
			text = _sqlstring;
		}
		else
		{
			dataadapter = new SqlDataAdapter("SELECT * FROM " + _SeciliChoose + " WHERE 1=0", sqlDB.Connection);
			datatable = new DataTable();
			dataadapter.Fill(datatable);
			string columnName = datatable.Columns[1].ColumnName;
			string text2 = "";
			if (_searchstring != "" && _searchstring.Contains("*"))
			{
				text2 = " WHERE (" + columnName + " LIKE N'" + _searchstring.Replace("*", "%") + "') ";
			}
			datatable.Dispose();
			dataadapter.Dispose();
			text = "SELECT * FROM " + _SeciliChoose + text2 + " ORDER BY " + columnName;
			if (_CustomWhereString != "")
			{
				dataadapter = new SqlDataAdapter("EXEC sp_helptext N'" + _SeciliChoose + "'", sqlDB.Connection);
				datatable = new DataTable();
				dataadapter.Fill(datatable);
				foreach (DataRow row in datatable.Rows)
				{
					row[0].ToString().Contains("ORDER BY");
				}
				string text3 = "";
				bool flag = false;
				int num = 0;
				foreach (DataRow row2 in datatable.Rows)
				{
					if (num > 1)
					{
						string text4 = row2[0].ToString();
						if (text4.Contains("SELECT") && text2 != "")
						{
							string text5 = "";
							text5 = text4.Substring(text4.IndexOf("," + 1), text4.Length - text4.IndexOf("," + 1));
							text5 = text5.Substring(0, text5.IndexOf(" "));
							_CustomWhereString = _CustomWhereString + " AND  (" + text5 + " LIKE N'" + _searchstring.Replace("*", "%") + "')  ";
						}
						if (text4.Contains("WHERE"))
						{
							flag = true;
							text4 = text4.Substring(0, text4.IndexOf("WHERE") + 6) + " (" + _CustomWhereString + ") AND " + text4.Substring(text4.IndexOf("WHERE") + 7, text4.Length - (text4.IndexOf("WHERE") + 7));
						}
						if (text4.StartsWith("ORDER") && !flag)
						{
							text3 = text3 + "WHERE (" + _CustomWhereString + ") ";
						}
						text3 = text3 + " " + text4 + " ";
					}
					num++;
				}
				datatable.Dispose();
				dataadapter.Dispose();
				text = text3;
			}
		}
		dataadapter = new SqlDataAdapter(text, sqlDB.Connection);
		datatable = new DataTable();
		dataadapter.Fill(datatable);
		sqlDB.ConnectionClose();
		foreach (DataColumn column in datatable.Columns)
		{
			if (column.Caption.Contains("msg_S_"))
			{
				column.Caption = mesajdegistir(column.Caption);
			}
		}
		gridControl1.DataSource = datatable;
		gridView1.PopulateColumns();
		gridView1.Columns[0].VisibleIndex = -1;
		gridView1.BestFitColumns();
		defaultlayoutStream = new MemoryStream();
		gridView1.SaveLayoutToStream(defaultlayoutStream);
		if (File.Exists("data\\" + _tag + "_" + _SeciliChoose + "_" + _gorunum + ".xml"))
		{
			gridView1.RestoreLayoutFromXml("data\\" + _tag + "_" + _SeciliChoose + "_" + _gorunum + ".xml");
		}
		else
		{
			defaultlayoutStream.Position = 0L;
			gridView1.RestoreLayoutFromStream(defaultlayoutStream);
		}
	}

	private void F10_Base_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode == Keys.Escape)
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		}
	}

	private void gridView1_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode == Keys.Return)
		{
			SecimAyarla();
		}
	}

	private void SecimAyarla()
	{
		bool flag = false;
		_selecteditemsguids = new List<Guid>();
		int[] selectedRows = gridView1.GetSelectedRows();
		if (selectedRows.Length != 0)
		{
			List<int> list = new List<int>();
			int[] array = selectedRows;
			foreach (int num in array)
			{
				if (num >= 0)
				{
					flag = true;
					object obj = gridView1.GetDataRow(num)[0];
					_selecteditemsguids.Add(Guid.Parse(obj.ToString()));
					list.Add(gridView1.GetDataSourceRowIndex(num));
				}
			}
		}
		if (flag)
		{
			ItemSelected();
		}
	}

	private void te_ara_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			_searchstring = te_ara.Text;
			GridOlustur();
			gridView1.Focus();
		}
	}

	private void gridView1_DoubleClick(object sender, EventArgs e)
	{
		SecimAyarla();
	}

	private void otomatikDosyadanYukleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\" + _tag + "_" + _SeciliChoose + "_otomatik.xml"))
		{
			gridView1.RestoreLayoutFromXml("data\\" + _tag + "_" + _SeciliChoose + "_otomatik.xml");
		}
	}

	private void otomatikDosyaSilToolStripMenuItem_Click(object sender, EventArgs e)
	{
		try
		{
			if (File.Exists("data\\" + _tag + "_" + _SeciliChoose + "_otomatik.xml"))
			{
				File.Delete("data\\" + _tag + "_" + _SeciliChoose + "_otomatik.xml");
			}
		}
		catch
		{
			MessageBox.Show("Otomatik doya silinemedi.");
		}
	}

	private void varsayilanaGeriDonToolStripMenuItem_Click(object sender, EventArgs e)
	{
		defaultlayoutStream.Position = 0L;
		gridView1.RestoreLayoutFromStream(defaultlayoutStream);
	}

	private void farkliDosyayaKaydetToolStripMenuItem_Click(object sender, EventArgs e)
	{
		saveFileDialog1.Filter = "Görünüm Dosyaları|*.grn";
		saveFileDialog1.InitialDirectory = Path.Combine(Application.StartupPath, "data");
		saveFileDialog1.FileName = "farkligorunum";
		saveFileDialog1.ShowDialog();
		if (saveFileDialog1.FileName != "")
		{
			if (File.Exists(saveFileDialog1.FileName))
			{
				File.Delete(saveFileDialog1.FileName);
			}
			gridView1.SaveLayoutToXml(saveFileDialog1.FileName);
		}
	}

	private void farkliDosyadanYukleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		openFileDialog1.Filter = "Görünüm Dosyaları|*.grn";
		openFileDialog1.InitialDirectory = Path.Combine(Application.StartupPath, "data");
		openFileDialog1.FileName = "";
		openFileDialog1.ShowDialog();
		if (openFileDialog1.FileName != "")
		{
			gridView1.RestoreLayoutFromXml(openFileDialog1.FileName);
		}
	}

	private void kolonlaraGoreGruplamaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (gridView1.OptionsView.ShowGroupPanel)
		{
			gridView1.OptionsView.ShowGroupPanel = false;
		}
		else
		{
			gridView1.OptionsView.ShowGroupPanel = true;
		}
	}

	private void kolonSeciciyiGosterToolStripMenuItem_Click(object sender, EventArgs e)
	{
		gridView1.ShowCustomization();
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
		this.gridControl1 = new DevExpress.XtraGrid.GridControl();
		this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.GorunumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.GorunumuSaklaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.gorunumukaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sorguCumlesiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.label1 = new System.Windows.Forms.Label();
		this.te_ara = new DevExpress.XtraEditors.TextEdit();
		this.görünümüYükleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyadanYukleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyaSilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.varsayilanaGeriDonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.farkliDosyayaKaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
		this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
		this.farkliDosyadanYukleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kolonlaraGoreGruplamaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kolonSeciciyiGosterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).BeginInit();
		this.menuStrip1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.te_ara.Properties).BeginInit();
		base.SuspendLayout();
		this.gridControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.gridControl1.Location = new System.Drawing.Point(0, 53);
		this.gridControl1.MainView = this.gridView1;
		this.gridControl1.Name = "gridControl1";
		this.gridControl1.Size = new System.Drawing.Size(917, 420);
		this.gridControl1.TabIndex = 0;
		this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.gridView1 });
		this.gridView1.Appearance.SelectedRow.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.gridView1.Appearance.SelectedRow.Options.UseBackColor = true;
		this.gridView1.GridControl = this.gridControl1;
		this.gridView1.HorzScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Always;
		this.gridView1.Name = "gridView1";
		this.gridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
		this.gridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
		this.gridView1.OptionsBehavior.Editable = false;
		this.gridView1.OptionsBehavior.ReadOnly = true;
		this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView1.OptionsView.ColumnAutoWidth = false;
		this.gridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(gridView1_KeyDown);
		this.gridView1.DoubleClick += new System.EventHandler(gridView1_DoubleClick);
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.GorunumToolStripMenuItem, this.sorguCumlesiToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(917, 24);
		this.menuStrip1.TabIndex = 1;
		this.menuStrip1.Text = "menuStrip1";
		this.GorunumToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[7] { this.kolonlaraGoreGruplamaToolStripMenuItem, this.kolonSeciciyiGosterToolStripMenuItem, this.toolStripSeparator1, this.GorunumuSaklaToolStripMenuItem, this.görünümüYükleToolStripMenuItem, this.otomatikDosyaSilToolStripMenuItem, this.varsayilanaGeriDonToolStripMenuItem });
		this.GorunumToolStripMenuItem.Name = "GorunumToolStripMenuItem";
		this.GorunumToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
		this.GorunumToolStripMenuItem.Text = "Görünüm";
		this.GorunumuSaklaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.gorunumukaydetToolStripMenuItem, this.farkliDosyayaKaydetToolStripMenuItem });
		this.GorunumuSaklaToolStripMenuItem.Name = "GorunumuSaklaToolStripMenuItem";
		this.GorunumuSaklaToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.GorunumuSaklaToolStripMenuItem.Text = "Görünümü Sakla";
		this.gorunumukaydetToolStripMenuItem.Name = "gorunumukaydetToolStripMenuItem";
		this.gorunumukaydetToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
		this.gorunumukaydetToolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		this.gorunumukaydetToolStripMenuItem.Click += new System.EventHandler(gorunumukaydetToolStripMenuItem_Click);
		this.sorguCumlesiToolStripMenuItem.Name = "sorguCumlesiToolStripMenuItem";
		this.sorguCumlesiToolStripMenuItem.Size = new System.Drawing.Size(96, 20);
		this.sorguCumlesiToolStripMenuItem.Text = "Sorgu Cümlesi";
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(11, 32);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(34, 13);
		this.label1.TabIndex = 2;
		this.label1.Text = "Ara : ";
		this.te_ara.Location = new System.Drawing.Point(51, 29);
		this.te_ara.Name = "te_ara";
		this.te_ara.Size = new System.Drawing.Size(232, 20);
		this.te_ara.TabIndex = 1;
		this.te_ara.KeyDown += new System.Windows.Forms.KeyEventHandler(te_ara_KeyDown);
		this.görünümüYükleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.otomatikDosyadanYukleToolStripMenuItem, this.farkliDosyadanYukleToolStripMenuItem });
		this.görünümüYükleToolStripMenuItem.Name = "görünümüYükleToolStripMenuItem";
		this.görünümüYükleToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.görünümüYükleToolStripMenuItem.Text = "Görünümü Yükle";
		this.otomatikDosyadanYukleToolStripMenuItem.Name = "otomatikDosyadanYukleToolStripMenuItem";
		this.otomatikDosyadanYukleToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
		this.otomatikDosyadanYukleToolStripMenuItem.Text = "Otomatik dosyadan yükle";
		this.otomatikDosyadanYukleToolStripMenuItem.Click += new System.EventHandler(otomatikDosyadanYukleToolStripMenuItem_Click);
		this.otomatikDosyaSilToolStripMenuItem.Name = "otomatikDosyaSilToolStripMenuItem";
		this.otomatikDosyaSilToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.otomatikDosyaSilToolStripMenuItem.Text = "Otomatik dosya sil";
		this.otomatikDosyaSilToolStripMenuItem.Click += new System.EventHandler(otomatikDosyaSilToolStripMenuItem_Click);
		this.varsayilanaGeriDonToolStripMenuItem.Name = "varsayilanaGeriDonToolStripMenuItem";
		this.varsayilanaGeriDonToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.varsayilanaGeriDonToolStripMenuItem.Text = "Varsayılana geri dön";
		this.varsayilanaGeriDonToolStripMenuItem.Click += new System.EventHandler(varsayilanaGeriDonToolStripMenuItem_Click);
		this.farkliDosyayaKaydetToolStripMenuItem.Name = "farkliDosyayaKaydetToolStripMenuItem";
		this.farkliDosyayaKaydetToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
		this.farkliDosyayaKaydetToolStripMenuItem.Text = "Farklı dosyaya kaydet...";
		this.farkliDosyayaKaydetToolStripMenuItem.Click += new System.EventHandler(farkliDosyayaKaydetToolStripMenuItem_Click);
		this.openFileDialog1.FileName = "openFileDialog1";
		this.farkliDosyadanYukleToolStripMenuItem.Name = "farkliDosyadanYukleToolStripMenuItem";
		this.farkliDosyadanYukleToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
		this.farkliDosyadanYukleToolStripMenuItem.Text = "Farklı dosyadan yükle...";
		this.farkliDosyadanYukleToolStripMenuItem.Click += new System.EventHandler(farkliDosyadanYukleToolStripMenuItem_Click);
		this.kolonlaraGoreGruplamaToolStripMenuItem.Name = "kolonlaraGoreGruplamaToolStripMenuItem";
		this.kolonlaraGoreGruplamaToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.kolonlaraGoreGruplamaToolStripMenuItem.Text = "Kolonlara göre gruplama";
		this.kolonlaraGoreGruplamaToolStripMenuItem.Click += new System.EventHandler(kolonlaraGoreGruplamaToolStripMenuItem_Click);
		this.kolonSeciciyiGosterToolStripMenuItem.Name = "kolonSeciciyiGosterToolStripMenuItem";
		this.kolonSeciciyiGosterToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.kolonSeciciyiGosterToolStripMenuItem.Text = "Kolon seçiciyi göster";
		this.kolonSeciciyiGosterToolStripMenuItem.Click += new System.EventHandler(kolonSeciciyiGosterToolStripMenuItem_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(202, 6);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(917, 473);
		base.Controls.Add(this.te_ara);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.gridControl1);
		base.Controls.Add(this.menuStrip1);
		base.KeyPreview = true;
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "F10_Base";
		this.Text = "Kayıt seçimi";
		base.Load += new System.EventHandler(F10_Base_Load);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(F10_Base_KeyDown);
		((System.ComponentModel.ISupportInitialize)this.gridControl1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.te_ara.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
