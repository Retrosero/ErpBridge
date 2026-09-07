using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Kargolar;
using Fora.Mikro.Win.Form.DevEx.F10;

namespace Fora.Mikro.Win.Form.DevEx.Controllers;

public class KargoSecimi : UserControl
{
	private Kargo _private_selecteditem;

	private string _private_CodeEmptyMessage = "Kargo seçiniz";

	private IContainer components;

	private ButtonEdit be_kod;

	private Label label_code;

	public MikroUygulamaBilgileri _mikrouygulamabilgileri { get; set; }

	public Kargo SelectedItem
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
				be_kod.Text = _private_selecteditem.krg_kodu;
			}
			else
			{
				be_kod.Text = _private_CodeEmptyMessage;
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

	public KargoSecimi()
	{
		InitializeComponent();
	}

	private void be_kod_ButtonClick(object sender, ButtonPressedEventArgs e)
	{
		if (AppBase.MikroVersiyonu >= 16)
		{
			F10_Kargo_Secimi_V16 f10_Kargo_Secimi_V = new F10_Kargo_Secimi_V16();
			f10_Kargo_Secimi_V.Setup(_mikrouygulamabilgileri, "KARGO_TANIMLARI_CHOOSE_2", be_kod.Text, AllowMultiSelect: false, IlkAramaAktifOlsun: false);
			if (f10_Kargo_Secimi_V.ShowDialog() == DialogResult.OK)
			{
				SelectedItem = f10_Kargo_Secimi_V._selecteditems[0];
			}
		}
		else
		{
			F10_Kargo_Secimi f10_Kargo_Secimi = new F10_Kargo_Secimi();
			f10_Kargo_Secimi.Setup(_mikrouygulamabilgileri, "KARGO_TANIMLARI_CHOOSE_2", be_kod.Text, AllowMultiSelect: false, IlkAramaAktifOlsun: false);
			if (f10_Kargo_Secimi.ShowDialog() == DialogResult.OK)
			{
				SelectedItem = f10_Kargo_Secimi._selecteditems[0];
			}
		}
	}

	private void be_kod_Validating(object sender, CancelEventArgs e)
	{
		if (be_kod.Text != "" && be_kod.Text != _private_CodeEmptyMessage)
		{
			Kargo kargo = KargoData.GetKargo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, be_kod.Text);
			if (kargo.krg_kodu == "")
			{
				e.Cancel = true;
			}
			else
			{
				SelectedItem = kargo;
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
		this.be_kod = new DevExpress.XtraEditors.ButtonEdit();
		this.label_code = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.be_kod.Properties).BeginInit();
		base.SuspendLayout();
		this.be_kod.EnterMoveNextControl = true;
		this.be_kod.Location = new System.Drawing.Point(50, 0);
		this.be_kod.Name = "be_kod";
		this.be_kod.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Ellipsis, "", -1, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F10), appearance, "", null, null, true)
		});
		this.be_kod.Properties.ButtonsStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
		this.be_kod.Properties.MaxLength = 25;
		this.be_kod.Size = new System.Drawing.Size(128, 20);
		this.be_kod.TabIndex = 0;
		this.be_kod.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(be_kod_ButtonClick);
		this.be_kod.Validating += new System.ComponentModel.CancelEventHandler(be_kod_Validating);
		this.label_code.AutoSize = true;
		this.label_code.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.label_code.Location = new System.Drawing.Point(3, 3);
		this.label_code.Name = "label_code";
		this.label_code.Size = new System.Drawing.Size(41, 13);
		this.label_code.TabIndex = 10;
		this.label_code.Text = "Kargo :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.label_code);
		base.Controls.Add(this.be_kod);
		base.Name = "KargoSecimi";
		base.Size = new System.Drawing.Size(181, 20);
		((System.ComponentModel.ISupportInitialize)this.be_kod.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
