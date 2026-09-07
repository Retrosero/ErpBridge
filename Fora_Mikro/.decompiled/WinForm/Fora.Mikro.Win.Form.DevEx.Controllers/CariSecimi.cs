using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Win.Form.DevEx.F10;

namespace Fora.Mikro.Win.Form.DevEx.Controllers;

public class CariSecimi : UserControl
{
	private Cari _private_selecteditem;

	private string _private_CodeEmptyMessage = "Cari seçiniz";

	private string _private_NameEmptyMessage = "Cari seçiniz";

	private IContainer components;

	private ButtonEdit be_kod;

	private ButtonEdit be_adi;

	private Label label_code;

	private Label label_name;

	public MikroUygulamaBilgileri _mikrouygulamabilgileri { get; set; }

	public Cari SelectedItem
	{
		get
		{
			return _private_selecteditem;
		}
		set
		{
			_private_selecteditem = value;
			if (_private_selecteditem != null)
			{
				be_kod.Text = _private_selecteditem.cari_kod;
				be_adi.Text = _private_selecteditem.cari_unvan1;
			}
			else
			{
				be_kod.Text = _private_CodeEmptyMessage;
				be_adi.Text = _private_NameEmptyMessage;
			}
		}
	}

	[Description("SizeCode")]
	[Category("Layout")]
	public Size SizeCode
	{
		get
		{
			return be_kod.Size;
		}
		set
		{
			be_kod.Size = value;
		}
	}

	[Description("SizeName")]
	[Category("Layout")]
	public Size SizeName
	{
		get
		{
			return be_adi.Size;
		}
		set
		{
			be_adi.Size = value;
		}
	}

	[Description("LocationCode")]
	[Category("Layout")]
	public Point LocationCode
	{
		get
		{
			return be_kod.Location;
		}
		set
		{
			be_kod.Location = value;
		}
	}

	[Description("LocationCodeLabel")]
	[Category("Layout")]
	public Point LocationCodeLabel
	{
		get
		{
			return label_code.Location;
		}
		set
		{
			label_code.Location = value;
		}
	}

	[Description("LocationName")]
	[Category("Layout")]
	public Point LocationName
	{
		get
		{
			return be_adi.Location;
		}
		set
		{
			be_adi.Location = value;
		}
	}

	[Description("LocationNameLabel")]
	[Category("Layout")]
	public Point LocationNameLabel
	{
		get
		{
			return label_name.Location;
		}
		set
		{
			label_name.Location = value;
		}
	}

	[Description("FontCode")]
	[Category("Appearance")]
	public Font FontCode
	{
		get
		{
			return be_kod.Font;
		}
		set
		{
			be_kod.Font = value;
		}
	}

	[Description("FontCodeLabel")]
	[Category("Appearance")]
	public Font FontCodeLabel
	{
		get
		{
			return label_code.Font;
		}
		set
		{
			label_code.Font = value;
		}
	}

	[Description("FontName")]
	[Category("Appearance")]
	public Font FontName
	{
		get
		{
			return be_adi.Font;
		}
		set
		{
			be_adi.Font = value;
		}
	}

	[Description("FontNameLabel")]
	[Category("Appearance")]
	public Font FontNameLabel
	{
		get
		{
			return label_name.Font;
		}
		set
		{
			label_name.Font = value;
		}
	}

	[Description("EnabledCode")]
	[Category("Behavior")]
	public bool EnabledCode
	{
		get
		{
			return be_kod.Enabled;
		}
		set
		{
			be_kod.Enabled = value;
		}
	}

	[Description("EnabledName")]
	[Category("Behavior")]
	public bool EnabledName
	{
		get
		{
			return be_adi.Enabled;
		}
		set
		{
			be_adi.Enabled = value;
		}
	}

	[Description("VisibleCode")]
	[Category("Behavior")]
	public bool VisibleCode
	{
		get
		{
			return be_kod.Visible;
		}
		set
		{
			be_kod.Visible = value;
		}
	}

	[Description("VisibleCodeLabel")]
	[Category("Behavior")]
	public bool VisibleCodeLabel
	{
		get
		{
			return label_code.Visible;
		}
		set
		{
			label_code.Visible = value;
		}
	}

	[Description("VisibleName")]
	[Category("Behavior")]
	public bool VisibleName
	{
		get
		{
			return be_adi.Visible;
		}
		set
		{
			be_adi.Visible = value;
		}
	}

	[Description("VisibleNameLabel")]
	[Category("Behavior")]
	public bool VisibleNameLabel
	{
		get
		{
			return label_name.Visible;
		}
		set
		{
			label_name.Visible = value;
		}
	}

	[Description("TextCodeLabel")]
	[Category("Data")]
	public string TextCodeLabel
	{
		get
		{
			return label_code.Text;
		}
		set
		{
			label_code.Text = value;
		}
	}

	[Description("TextNameLabel")]
	[Category("Data")]
	public string TextNameLabel
	{
		get
		{
			return label_name.Text;
		}
		set
		{
			label_name.Text = value;
		}
	}

	[Description("CodeEmptyMessage")]
	[Category("Data")]
	public string CodeEmptyMessage
	{
		get
		{
			return _private_CodeEmptyMessage;
		}
		set
		{
			_private_CodeEmptyMessage = value;
		}
	}

	[Description("NameEmptyMessage")]
	[Category("Data")]
	public string NameEmptyMessage
	{
		get
		{
			return _private_NameEmptyMessage;
		}
		set
		{
			_private_NameEmptyMessage = value;
		}
	}

	public CariSecimi()
	{
		InitializeComponent();
	}

	private void be_kod_ButtonClick(object sender, ButtonPressedEventArgs e)
	{
		if (AppBase.MikroVersiyonu >= 16)
		{
			F10_Cari_Secimi_V16 f10_Cari_Secimi_V = new F10_Cari_Secimi_V16();
			f10_Cari_Secimi_V.Setup(_mikrouygulamabilgileri, "CARI_HESAPLAR_CHOOSE_2", be_kod.Text, AllowMultiSelect: false, IlkAramaAktifOlsun: false);
			if (f10_Cari_Secimi_V.ShowDialog() == DialogResult.OK)
			{
				SelectedItem = f10_Cari_Secimi_V._selecteditems[0];
			}
		}
		else
		{
			F10_Cari_Secimi f10_Cari_Secimi = new F10_Cari_Secimi();
			f10_Cari_Secimi.Setup(_mikrouygulamabilgileri, "CARI_HESAPLAR_CHOOSE_2", be_kod.Text, AllowMultiSelect: false, IlkAramaAktifOlsun: false);
			if (f10_Cari_Secimi.ShowDialog() == DialogResult.OK)
			{
				SelectedItem = f10_Cari_Secimi._selecteditems[0];
			}
		}
	}

	private void be_adi_Properties_ButtonClick(object sender, ButtonPressedEventArgs e)
	{
		if (AppBase.MikroVersiyonu >= 16)
		{
			F10_Cari_Secimi_V16 f10_Cari_Secimi_V = new F10_Cari_Secimi_V16();
			f10_Cari_Secimi_V.Setup(_mikrouygulamabilgileri, "CARI_HESAPLAR_CHOOSE_3", be_adi.Text, AllowMultiSelect: false, IlkAramaAktifOlsun: false);
			if (f10_Cari_Secimi_V.ShowDialog() == DialogResult.OK)
			{
				SelectedItem = f10_Cari_Secimi_V._selecteditems[0];
			}
		}
		else
		{
			F10_Cari_Secimi f10_Cari_Secimi = new F10_Cari_Secimi();
			f10_Cari_Secimi.Setup(_mikrouygulamabilgileri, "CARI_HESAPLAR_CHOOSE_3", be_adi.Text, AllowMultiSelect: false, IlkAramaAktifOlsun: false);
			if (f10_Cari_Secimi.ShowDialog() == DialogResult.OK)
			{
				SelectedItem = f10_Cari_Secimi._selecteditems[0];
			}
		}
	}

	private void be_kod_Validating(object sender, CancelEventArgs e)
	{
		if (be_kod.Text != "" && be_kod.Text != _private_CodeEmptyMessage)
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			Cari cariByCariKod = CariData.GetCariByCariKod(sqlDB.Connection, be_kod.Text, AdreslerTemsilciyeGore: false, "");
			sqlDB.ConnectionClose();
			if (cariByCariKod.cari_kod == "")
			{
				e.Cancel = true;
			}
			else
			{
				SelectedItem = cariByCariKod;
			}
		}
		else
		{
			SelectedItem = null;
		}
	}

	private void be_adi_Validating(object sender, CancelEventArgs e)
	{
		if (be_adi.Text != "" && be_kod.Text != _private_CodeEmptyMessage)
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			Cari cariByCariUnvan = CariData.GetCariByCariUnvan(sqlDB.Connection, be_adi.Text, AdreslerTemsilciyeGore: false, "");
			sqlDB.ConnectionClose();
			if (cariByCariUnvan.cari_kod == "")
			{
				e.Cancel = true;
			}
			else
			{
				SelectedItem = cariByCariUnvan;
			}
		}
		else
		{
			SelectedItem = null;
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
		DevExpress.Utils.SerializableAppearanceObject appearance = new DevExpress.Utils.SerializableAppearanceObject();
		DevExpress.Utils.SerializableAppearanceObject appearance2 = new DevExpress.Utils.SerializableAppearanceObject();
		this.be_kod = new DevExpress.XtraEditors.ButtonEdit();
		this.be_adi = new DevExpress.XtraEditors.ButtonEdit();
		this.label_code = new System.Windows.Forms.Label();
		this.label_name = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.be_kod.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.be_adi.Properties).BeginInit();
		base.SuspendLayout();
		this.be_kod.EnterMoveNextControl = true;
		this.be_kod.Location = new System.Drawing.Point(67, 0);
		this.be_kod.Name = "be_kod";
		this.be_kod.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Ellipsis, "", -1, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F10), appearance, "", null, null, true)
		});
		this.be_kod.Properties.ButtonsStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
		this.be_kod.Properties.MaxLength = 25;
		this.be_kod.Size = new System.Drawing.Size(110, 20);
		this.be_kod.TabIndex = 0;
		this.be_kod.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(be_kod_ButtonClick);
		this.be_kod.Validating += new System.ComponentModel.CancelEventHandler(be_kod_Validating);
		this.be_adi.EnterMoveNextControl = true;
		this.be_adi.Location = new System.Drawing.Point(253, 0);
		this.be_adi.Name = "be_adi";
		this.be_adi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Ellipsis, "", -1, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F10), appearance2, "", null, null, true)
		});
		this.be_adi.Properties.ButtonsStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
		this.be_adi.Properties.MaxLength = 50;
		this.be_adi.Properties.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(be_adi_Properties_ButtonClick);
		this.be_adi.Size = new System.Drawing.Size(172, 20);
		this.be_adi.TabIndex = 1;
		this.be_adi.Validating += new System.ComponentModel.CancelEventHandler(be_adi_Validating);
		this.label_code.AutoSize = true;
		this.label_code.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.label_code.Location = new System.Drawing.Point(3, 3);
		this.label_code.Name = "label_code";
		this.label_code.Size = new System.Drawing.Size(58, 13);
		this.label_code.TabIndex = 10;
		this.label_code.Text = "Cari kodu :";
		this.label_name.AutoSize = true;
		this.label_name.Location = new System.Drawing.Point(183, 3);
		this.label_name.Name = "label_name";
		this.label_name.Size = new System.Drawing.Size(64, 13);
		this.label_name.TabIndex = 12;
		this.label_name.Text = "Cari ünvan :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.label_name);
		base.Controls.Add(this.label_code);
		base.Controls.Add(this.be_adi);
		base.Controls.Add(this.be_kod);
		base.Name = "CariSecimi";
		base.Size = new System.Drawing.Size(426, 20);
		((System.ComponentModel.ISupportInitialize)this.be_kod.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.be_adi.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
