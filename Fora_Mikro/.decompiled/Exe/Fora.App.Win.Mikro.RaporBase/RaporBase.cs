using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Fora.Mikro;

namespace Fora.App.Win.Mikro.RaporBase;

public class RaporBase : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private MemoryStream master_defaultlayoutStream;

	private MemoryStream detail_defaultlayoutStream;

	public GridControl gridControl1;

	private string _tag;

	private string _baslik;

	private AdvBandedGridView masterview;

	private AdvBandedGridView detailview;

	private IContainer components;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem kapatToolStripMenuItem;

	private GridView gridView1;

	private ToolStripMenuItem görünümToolStripMenuItem;

	private ToolStripMenuItem anaToolStripMenuItem;

	private ToolStripMenuItem ayrıntıToolStripMenuItem;

	private ToolStripMenuItem kolonlaraGoreGruplamaToolStripMenuItem;

	private ToolStripMenuItem kolonSeciciyiGosterToolStripMenuItem;

	private ToolStripMenuItem butunGruplariAcToolStripMenuItem;

	private ToolStripMenuItem butunGruplariKapatToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem görünümüSaklaToolStripMenuItem;

	private ToolStripMenuItem otomatikDosyayaToolStripMenuItem;

	private ToolStripMenuItem farkliDosyayaToolStripMenuItem;

	private ToolStripMenuItem görünümüYükleToolStripMenuItem;

	private ToolStripMenuItem otomatikDosyadanToolStripMenuItem;

	private ToolStripMenuItem farkliDosyadanToolStripMenuItem;

	private ToolStripMenuItem otomatikDosyayiSilToolStripMenuItem;

	private ToolStripMenuItem varsayilanaGeriDonToolStripMenuItem;

	private ToolStripMenuItem kolonlaraGoreGruplamaToolStripMenuItem_detail;

	private ToolStripMenuItem kolonSeciciyiGosterToolStripMenuItem_detail;

	private ToolStripMenuItem butunGruplariAcToolStripMenuItem_detail;

	private ToolStripMenuItem butunGruplariKapatToolStripMenuItem_detail;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripMenuItem görünümüSaklaToolStripMenuItem1;

	private ToolStripMenuItem otomatikDosyayaToolStripMenuItem_detail;

	private ToolStripMenuItem farkliDosyayaToolStripMenuItem_detail;

	private ToolStripMenuItem görünümüYükleToolStripMenuItem1;

	private ToolStripMenuItem otomatikDosyadanToolStripMenuItem_detail;

	private ToolStripMenuItem farkliDosyadanToolStripMenuItem_detail;

	private ToolStripMenuItem otomatikDosyayiSilToolStripMenuItem_detail;

	private ToolStripMenuItem varsayilanaGeriDonToolStripMenuItem_detail;

	private ToolStripMenuItem exceleAktarToolStripMenuItem;

	public RaporBase(MikroUygulamaBilgileri mikrouygulamabilgileri, string tag, string baslik, GridControl gridControlToShow)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		base.FormBorderStyle = FormBorderStyle.Sizable;
		base.StartPosition = FormStartPosition.CenterScreen;
		_tag = tag;
		_baslik = baslik;
		Text = baslik;
		gridControl1 = gridControlToShow;
		((ISupportInitialize)gridControl1).BeginInit();
		SuspendLayout();
		gridControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		gridControl1.Location = new Point(12, 27);
		gridControl1.MainView = null;
		gridControl1.Name = "gridControl1";
		gridControl1.Size = new Size(925, 517);
		gridControl1.TabIndex = 17;
		base.Controls.Add(gridControl1);
		((ISupportInitialize)gridControl1).EndInit();
		ResumeLayout(performLayout: false);
		PerformLayout();
		masterview = (AdvBandedGridView)gridControl1.ViewCollection[1];
		detailview = (AdvBandedGridView)gridControl1.ViewCollection[0];
		master_defaultlayoutStream = new MemoryStream();
		masterview.SaveLayoutToStream(master_defaultlayoutStream);
		detail_defaultlayoutStream = new MemoryStream();
		detailview.SaveLayoutToStream(detail_defaultlayoutStream);
		if (File.Exists("data\\views\\" + _tag + ".lgm"))
		{
			masterview.RestoreLayoutFromXml("data\\views\\" + _tag + ".rpm");
		}
		if (File.Exists("data\\views\\" + _tag + ".lgs"))
		{
			detailview.RestoreLayoutFromXml("data\\views\\" + _tag + ".rpd");
		}
	}

	private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void kolonlaraGoreGruplamaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (masterview.OptionsView.ShowGroupPanel)
		{
			masterview.OptionsView.ShowGroupPanel = false;
		}
		else
		{
			masterview.OptionsView.ShowGroupPanel = true;
		}
	}

	private void kolonSeciciyiGosterToolStripMenuItem_Click(object sender, EventArgs e)
	{
		masterview.ShowCustomization();
	}

	private void butunGruplariAcToolStripMenuItem_Click(object sender, EventArgs e)
	{
		masterview.ExpandAllGroups();
	}

	private void butunGruplariKapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		masterview.CollapseAllGroups();
	}

	private void otomatikDosyayaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		masterview.SaveLayoutToXml("data\\views\\" + _tag + ".rpm");
	}

	private void farkliDosyayaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		saveFileDialog.Filter = "Rapor Ana Görünüm |*.rpm|Tüm dosyalar|*.*";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			masterview.SaveLayoutToXml(saveFileDialog.FileName);
		}
	}

	private void otomatikDosyadanToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + ".rpm"))
		{
			masterview.RestoreLayoutFromXml("data\\views\\" + _tag + ".rpm");
		}
	}

	private void farkliDosyadanToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		openFileDialog.Filter = "Rapor Ana Görünüm |*.rpm|Tüm dosyalar|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			masterview.RestoreLayoutFromXml(openFileDialog.FileName);
		}
	}

	private void otomatikDosyayiSilToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + ".rpm"))
		{
			File.Delete("data\\views\\" + _tag + ".rpm");
		}
	}

	private void varsayilanaGeriDonToolStripMenuItem_Click(object sender, EventArgs e)
	{
		master_defaultlayoutStream.Position = 0L;
		masterview.RestoreLayoutFromStream(master_defaultlayoutStream);
	}

	private void kolonlaraGoreGruplamaToolStripMenuItem_detail_Click(object sender, EventArgs e)
	{
		if (detailview.OptionsView.ShowGroupPanel)
		{
			detailview.OptionsView.ShowGroupPanel = false;
		}
		else
		{
			detailview.OptionsView.ShowGroupPanel = true;
		}
	}

	private void kolonSeciciyiGosterToolStripMenuItem_detail_Click(object sender, EventArgs e)
	{
		detailview.ShowCustomization();
	}

	private void butunGruplariAcToolStripMenuItem_detail_Click(object sender, EventArgs e)
	{
		detailview.ExpandAllGroups();
	}

	private void butunGruplariKapatToolStripMenuItem_detail_Click(object sender, EventArgs e)
	{
		detailview.CollapseAllGroups();
	}

	private void otomatikDosyayaToolStripMenuItem_detail_Click(object sender, EventArgs e)
	{
		detailview.SaveLayoutToXml("data\\views\\" + _tag + ".rpd");
	}

	private void farkliDosyayaToolStripMenuItem_detail_Click(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		saveFileDialog.Filter = "Rapor Detay Görünüm |*.rpd|Tüm dosyalar|*.*";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			detailview.SaveLayoutToXml(saveFileDialog.FileName);
		}
	}

	private void otomatikDosyadanToolStripMenuItem_detail_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + ".rpd"))
		{
			detailview.RestoreLayoutFromXml("data\\views\\" + _tag + ".rpd");
		}
	}

	private void farkliDosyadanToolStripMenuItem_detail_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		openFileDialog.Filter = "Rapor Detay Görünüm |*.rpd|Tüm dosyalar|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			detailview.RestoreLayoutFromXml(openFileDialog.FileName);
		}
	}

	private void otomatikDosyayiSilToolStripMenuItem_detail_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + ".rpd"))
		{
			File.Delete("data\\views\\" + _tag + ".rpd");
		}
	}

	private void varsayilanaGeriDonToolStripMenuItem_detail_Click(object sender, EventArgs e)
	{
		detail_defaultlayoutStream.Position = 0L;
		detailview.RestoreLayoutFromStream(detail_defaultlayoutStream);
	}

	private void exceleAktarToolStripMenuItem_Click(object sender, EventArgs e)
	{
		try
		{
			string text = ShowSaveFileDialog("Microsoft Excel Document", "Microsoft Excel|*.xls");
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			bool printDetails = masterview.OptionsPrint.PrintDetails;
			bool expandAllDetails = masterview.OptionsPrint.ExpandAllDetails;
			bool expandAllGroups = masterview.OptionsPrint.ExpandAllGroups;
			MemoryStream memoryStream = new MemoryStream();
			MemoryStream memoryStream2 = new MemoryStream();
			masterview.SaveLayoutToStream(memoryStream);
			masterview.SaveLayoutToStream(memoryStream2);
			masterview.OptionsPrint.ExpandAllDetails = true;
			masterview.OptionsPrint.ExpandAllGroups = true;
			masterview.OptionsPrint.PrintDetails = true;
			RepositoryItemMemoEdit repositoryItemMemoEdit = new RepositoryItemMemoEdit();
			gridControl1.RepositoryItems.Add(repositoryItemMemoEdit);
			foreach (AdvBandedGridView view in masterview.ViewRepository.Views)
			{
				view.OptionsPrint.AutoWidth = false;
				view.OptionsView.RowAutoHeight = true;
				foreach (GridColumn column in view.Columns)
				{
					column.ColumnEdit = repositoryItemMemoEdit;
				}
			}
			masterview.ExportToXls(text);
			memoryStream.Position = 0L;
			memoryStream2.Position = 0L;
			masterview.RestoreLayoutFromStream(memoryStream);
			masterview.RestoreLayoutFromStream(memoryStream2);
			gridControl1.RepositoryItems.Remove(repositoryItemMemoEdit);
			masterview.OptionsPrint.PrintDetails = printDetails;
			masterview.OptionsPrint.ExpandAllDetails = expandAllDetails;
			masterview.OptionsPrint.ExpandAllGroups = expandAllGroups;
			OpenFile(text);
		}
		catch (Exception)
		{
		}
	}

	private string ShowSaveFileDialog(string title, string filter)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		string fileName = DateTime.Now.ToString("dd-MM-yyyy") + "-" + _tag;
		saveFileDialog.Title = "Export To " + title;
		saveFileDialog.FileName = fileName;
		saveFileDialog.Filter = filter;
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			return saveFileDialog.FileName;
		}
		return "";
	}

	internal static void OpenFile(string fileName)
	{
		if (XtraMessageBox.Show("Dosyayı açmak istiyor musunuz?", "Aktar...", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			try
			{
				Process process = new Process();
				process.StartInfo.FileName = fileName;
				process.StartInfo.Verb = "Open";
				process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
				process.Start();
			}
			catch
			{
				XtraMessageBox.Show("Aktarılan dosya tipini açacak uygun bir program bulunamadı.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
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
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.görünümToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.anaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kolonlaraGoreGruplamaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kolonSeciciyiGosterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.butunGruplariAcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.butunGruplariKapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.görünümüSaklaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyayaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.farkliDosyayaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.görünümüYükleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyadanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.farkliDosyadanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyayiSilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.varsayilanaGeriDonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.ayrıntıToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.gridControl1 = new DevExpress.XtraGrid.GridControl();
		this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.kolonlaraGoreGruplamaToolStripMenuItem_detail = new System.Windows.Forms.ToolStripMenuItem();
		this.kolonSeciciyiGosterToolStripMenuItem_detail = new System.Windows.Forms.ToolStripMenuItem();
		this.butunGruplariAcToolStripMenuItem_detail = new System.Windows.Forms.ToolStripMenuItem();
		this.butunGruplariKapatToolStripMenuItem_detail = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.görünümüSaklaToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyayaToolStripMenuItem_detail = new System.Windows.Forms.ToolStripMenuItem();
		this.farkliDosyayaToolStripMenuItem_detail = new System.Windows.Forms.ToolStripMenuItem();
		this.görünümüYükleToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyadanToolStripMenuItem_detail = new System.Windows.Forms.ToolStripMenuItem();
		this.farkliDosyadanToolStripMenuItem_detail = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyayiSilToolStripMenuItem_detail = new System.Windows.Forms.ToolStripMenuItem();
		this.varsayilanaGeriDonToolStripMenuItem_detail = new System.Windows.Forms.ToolStripMenuItem();
		this.exceleAktarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.menuStrip1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).BeginInit();
		base.SuspendLayout();
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.dosyaToolStripMenuItem, this.görünümToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(949, 24);
		this.menuStrip1.TabIndex = 16;
		this.menuStrip1.Text = "menuStrip1";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.exceleAktarToolStripMenuItem, this.kapatToolStripMenuItem });
		this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
		this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
		this.dosyaToolStripMenuItem.Text = "Dosya";
		this.kapatToolStripMenuItem.Name = "kapatToolStripMenuItem";
		this.kapatToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
		this.kapatToolStripMenuItem.Text = "Kapat";
		this.kapatToolStripMenuItem.Click += new System.EventHandler(kapatToolStripMenuItem_Click);
		this.görünümToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.anaToolStripMenuItem, this.ayrıntıToolStripMenuItem });
		this.görünümToolStripMenuItem.Name = "görünümToolStripMenuItem";
		this.görünümToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
		this.görünümToolStripMenuItem.Text = "Görünüm";
		this.anaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.kolonlaraGoreGruplamaToolStripMenuItem, this.kolonSeciciyiGosterToolStripMenuItem, this.butunGruplariAcToolStripMenuItem, this.butunGruplariKapatToolStripMenuItem, this.toolStripSeparator1, this.görünümüSaklaToolStripMenuItem, this.görünümüYükleToolStripMenuItem, this.otomatikDosyayiSilToolStripMenuItem, this.varsayilanaGeriDonToolStripMenuItem });
		this.anaToolStripMenuItem.Name = "anaToolStripMenuItem";
		this.anaToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.anaToolStripMenuItem.Text = "Ana";
		this.kolonlaraGoreGruplamaToolStripMenuItem.Name = "kolonlaraGoreGruplamaToolStripMenuItem";
		this.kolonlaraGoreGruplamaToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.kolonlaraGoreGruplamaToolStripMenuItem.Text = "Kolonlara göre gruplama";
		this.kolonlaraGoreGruplamaToolStripMenuItem.Click += new System.EventHandler(kolonlaraGoreGruplamaToolStripMenuItem_Click);
		this.kolonSeciciyiGosterToolStripMenuItem.Name = "kolonSeciciyiGosterToolStripMenuItem";
		this.kolonSeciciyiGosterToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.kolonSeciciyiGosterToolStripMenuItem.Text = "Kolon seçiciyi göster";
		this.kolonSeciciyiGosterToolStripMenuItem.Click += new System.EventHandler(kolonSeciciyiGosterToolStripMenuItem_Click);
		this.butunGruplariAcToolStripMenuItem.Name = "butunGruplariAcToolStripMenuItem";
		this.butunGruplariAcToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.butunGruplariAcToolStripMenuItem.Text = "Bütün grupları aç";
		this.butunGruplariAcToolStripMenuItem.Click += new System.EventHandler(butunGruplariAcToolStripMenuItem_Click);
		this.butunGruplariKapatToolStripMenuItem.Name = "butunGruplariKapatToolStripMenuItem";
		this.butunGruplariKapatToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.butunGruplariKapatToolStripMenuItem.Text = "Bütün grupları kapat";
		this.butunGruplariKapatToolStripMenuItem.Click += new System.EventHandler(butunGruplariKapatToolStripMenuItem_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(202, 6);
		this.görünümüSaklaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.otomatikDosyayaToolStripMenuItem, this.farkliDosyayaToolStripMenuItem });
		this.görünümüSaklaToolStripMenuItem.Name = "görünümüSaklaToolStripMenuItem";
		this.görünümüSaklaToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.görünümüSaklaToolStripMenuItem.Text = "Görünümü sakla";
		this.otomatikDosyayaToolStripMenuItem.Name = "otomatikDosyayaToolStripMenuItem";
		this.otomatikDosyayaToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
		this.otomatikDosyayaToolStripMenuItem.Text = "Otomatik dosyaya";
		this.otomatikDosyayaToolStripMenuItem.Click += new System.EventHandler(otomatikDosyayaToolStripMenuItem_Click);
		this.farkliDosyayaToolStripMenuItem.Name = "farkliDosyayaToolStripMenuItem";
		this.farkliDosyayaToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
		this.farkliDosyayaToolStripMenuItem.Text = "Farklı dosyaya...";
		this.farkliDosyayaToolStripMenuItem.Click += new System.EventHandler(farkliDosyayaToolStripMenuItem_Click);
		this.görünümüYükleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.otomatikDosyadanToolStripMenuItem, this.farkliDosyadanToolStripMenuItem });
		this.görünümüYükleToolStripMenuItem.Name = "görünümüYükleToolStripMenuItem";
		this.görünümüYükleToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.görünümüYükleToolStripMenuItem.Text = "Görünümü yükle";
		this.otomatikDosyadanToolStripMenuItem.Name = "otomatikDosyadanToolStripMenuItem";
		this.otomatikDosyadanToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
		this.otomatikDosyadanToolStripMenuItem.Text = "Otomatik dosyadan";
		this.otomatikDosyadanToolStripMenuItem.Click += new System.EventHandler(otomatikDosyadanToolStripMenuItem_Click);
		this.farkliDosyadanToolStripMenuItem.Name = "farkliDosyadanToolStripMenuItem";
		this.farkliDosyadanToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
		this.farkliDosyadanToolStripMenuItem.Text = "Farklı dosyadan...";
		this.farkliDosyadanToolStripMenuItem.Click += new System.EventHandler(farkliDosyadanToolStripMenuItem_Click);
		this.otomatikDosyayiSilToolStripMenuItem.Name = "otomatikDosyayiSilToolStripMenuItem";
		this.otomatikDosyayiSilToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.otomatikDosyayiSilToolStripMenuItem.Text = "Otomatik dosyayı sil";
		this.otomatikDosyayiSilToolStripMenuItem.Click += new System.EventHandler(otomatikDosyayiSilToolStripMenuItem_Click);
		this.varsayilanaGeriDonToolStripMenuItem.Name = "varsayilanaGeriDonToolStripMenuItem";
		this.varsayilanaGeriDonToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.varsayilanaGeriDonToolStripMenuItem.Text = "Varsayılana geri dön";
		this.varsayilanaGeriDonToolStripMenuItem.Click += new System.EventHandler(varsayilanaGeriDonToolStripMenuItem_Click);
		this.ayrıntıToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.kolonlaraGoreGruplamaToolStripMenuItem_detail, this.kolonSeciciyiGosterToolStripMenuItem_detail, this.butunGruplariAcToolStripMenuItem_detail, this.butunGruplariKapatToolStripMenuItem_detail, this.toolStripSeparator2, this.görünümüSaklaToolStripMenuItem1, this.görünümüYükleToolStripMenuItem1, this.otomatikDosyayiSilToolStripMenuItem_detail, this.varsayilanaGeriDonToolStripMenuItem_detail });
		this.ayrıntıToolStripMenuItem.Name = "ayrıntıToolStripMenuItem";
		this.ayrıntıToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.ayrıntıToolStripMenuItem.Text = "Ayrıntı";
		this.gridControl1.Location = new System.Drawing.Point(0, 0);
		this.gridControl1.MainView = this.gridView1;
		this.gridControl1.Name = "gridControl1";
		this.gridControl1.Size = new System.Drawing.Size(400, 200);
		this.gridControl1.TabIndex = 0;
		this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.gridView1 });
		this.gridView1.GridControl = this.gridControl1;
		this.gridView1.Name = "gridView1";
		this.kolonlaraGoreGruplamaToolStripMenuItem_detail.Name = "kolonlaraGoreGruplamaToolStripMenuItem_detail";
		this.kolonlaraGoreGruplamaToolStripMenuItem_detail.Size = new System.Drawing.Size(205, 22);
		this.kolonlaraGoreGruplamaToolStripMenuItem_detail.Text = "Kolonlara göre gruplama";
		this.kolonlaraGoreGruplamaToolStripMenuItem_detail.Click += new System.EventHandler(kolonlaraGoreGruplamaToolStripMenuItem_detail_Click);
		this.kolonSeciciyiGosterToolStripMenuItem_detail.Name = "kolonSeciciyiGosterToolStripMenuItem_detail";
		this.kolonSeciciyiGosterToolStripMenuItem_detail.Size = new System.Drawing.Size(205, 22);
		this.kolonSeciciyiGosterToolStripMenuItem_detail.Text = "Kolon seçiciyi göster";
		this.kolonSeciciyiGosterToolStripMenuItem_detail.Click += new System.EventHandler(kolonSeciciyiGosterToolStripMenuItem_detail_Click);
		this.butunGruplariAcToolStripMenuItem_detail.Name = "butunGruplariAcToolStripMenuItem_detail";
		this.butunGruplariAcToolStripMenuItem_detail.Size = new System.Drawing.Size(205, 22);
		this.butunGruplariAcToolStripMenuItem_detail.Text = "Bütün grupları aç";
		this.butunGruplariAcToolStripMenuItem_detail.Click += new System.EventHandler(butunGruplariAcToolStripMenuItem_detail_Click);
		this.butunGruplariKapatToolStripMenuItem_detail.Name = "butunGruplariKapatToolStripMenuItem_detail";
		this.butunGruplariKapatToolStripMenuItem_detail.Size = new System.Drawing.Size(205, 22);
		this.butunGruplariKapatToolStripMenuItem_detail.Text = "Bütün grupları kapat";
		this.butunGruplariKapatToolStripMenuItem_detail.Click += new System.EventHandler(butunGruplariKapatToolStripMenuItem_detail_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(202, 6);
		this.görünümüSaklaToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.otomatikDosyayaToolStripMenuItem_detail, this.farkliDosyayaToolStripMenuItem_detail });
		this.görünümüSaklaToolStripMenuItem1.Name = "görünümüSaklaToolStripMenuItem1";
		this.görünümüSaklaToolStripMenuItem1.Size = new System.Drawing.Size(205, 22);
		this.görünümüSaklaToolStripMenuItem1.Text = "Görünümü sakla";
		this.otomatikDosyayaToolStripMenuItem_detail.Name = "otomatikDosyayaToolStripMenuItem_detail";
		this.otomatikDosyayaToolStripMenuItem_detail.Size = new System.Drawing.Size(170, 22);
		this.otomatikDosyayaToolStripMenuItem_detail.Text = "Otomatik dosyaya";
		this.otomatikDosyayaToolStripMenuItem_detail.Click += new System.EventHandler(otomatikDosyayaToolStripMenuItem_detail_Click);
		this.farkliDosyayaToolStripMenuItem_detail.Name = "farkliDosyayaToolStripMenuItem_detail";
		this.farkliDosyayaToolStripMenuItem_detail.Size = new System.Drawing.Size(170, 22);
		this.farkliDosyayaToolStripMenuItem_detail.Text = "Farklı dosyaya...";
		this.farkliDosyayaToolStripMenuItem_detail.Click += new System.EventHandler(farkliDosyayaToolStripMenuItem_detail_Click);
		this.görünümüYükleToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.otomatikDosyadanToolStripMenuItem_detail, this.farkliDosyadanToolStripMenuItem_detail });
		this.görünümüYükleToolStripMenuItem1.Name = "görünümüYükleToolStripMenuItem1";
		this.görünümüYükleToolStripMenuItem1.Size = new System.Drawing.Size(205, 22);
		this.görünümüYükleToolStripMenuItem1.Text = "Görünümü yükle";
		this.otomatikDosyadanToolStripMenuItem_detail.Name = "otomatikDosyadanToolStripMenuItem_detail";
		this.otomatikDosyadanToolStripMenuItem_detail.Size = new System.Drawing.Size(178, 22);
		this.otomatikDosyadanToolStripMenuItem_detail.Text = "Otomatik dosyadan";
		this.otomatikDosyadanToolStripMenuItem_detail.Click += new System.EventHandler(otomatikDosyadanToolStripMenuItem_detail_Click);
		this.farkliDosyadanToolStripMenuItem_detail.Name = "farkliDosyadanToolStripMenuItem_detail";
		this.farkliDosyadanToolStripMenuItem_detail.Size = new System.Drawing.Size(178, 22);
		this.farkliDosyadanToolStripMenuItem_detail.Text = "Farklı dosyadan...";
		this.farkliDosyadanToolStripMenuItem_detail.Click += new System.EventHandler(farkliDosyadanToolStripMenuItem_detail_Click);
		this.otomatikDosyayiSilToolStripMenuItem_detail.Name = "otomatikDosyayiSilToolStripMenuItem_detail";
		this.otomatikDosyayiSilToolStripMenuItem_detail.Size = new System.Drawing.Size(205, 22);
		this.otomatikDosyayiSilToolStripMenuItem_detail.Text = "Otomatik dosyayı sil";
		this.otomatikDosyayiSilToolStripMenuItem_detail.Click += new System.EventHandler(otomatikDosyayiSilToolStripMenuItem_detail_Click);
		this.varsayilanaGeriDonToolStripMenuItem_detail.Name = "varsayilanaGeriDonToolStripMenuItem_detail";
		this.varsayilanaGeriDonToolStripMenuItem_detail.Size = new System.Drawing.Size(205, 22);
		this.varsayilanaGeriDonToolStripMenuItem_detail.Text = "Varsayılana geri dön";
		this.varsayilanaGeriDonToolStripMenuItem_detail.Click += new System.EventHandler(varsayilanaGeriDonToolStripMenuItem_detail_Click);
		this.exceleAktarToolStripMenuItem.Name = "exceleAktarToolStripMenuItem";
		this.exceleAktarToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.exceleAktarToolStripMenuItem.Text = "Excel'e aktar";
		this.exceleAktarToolStripMenuItem.Click += new System.EventHandler(exceleAktarToolStripMenuItem_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(949, 556);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "RaporBase";
		this.Text = "BAŞLIK";
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
