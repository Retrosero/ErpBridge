using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using Fora.Mikro.Enumler;

namespace Fora.App.Win.Mikro.Classes;

public class DevExpressRepositoryItemGridLookupEditExtended
{
	public RepositoryItemGridLookUpEdit _RepositoryItem;

	public string _tag;

	public ToolStripMenuItem _ToolStripMenuItemAnaBaslik;

	private enum_DevExpressRepositoryItemGridLookUpEdit _Role;

	private GridView gridView_Arama;

	private MemoryStream _temp_defaultlayoutStream;

	private GridView _tempview;

	public DevExpressRepositoryItemGridLookupEditExtended(string tag, enum_DevExpressRepositoryItemGridLookUpEdit role)
	{
		_RepositoryItem = new RepositoryItemGridLookUpEdit();
		_tag = tag;
		_Role = role;
		_ToolStripMenuItemAnaBaslik = new ToolStripMenuItem();
	}

	public void Init(DataSet _lookuptablolar)
	{
		gridView_Arama = new GridView();
		gridView_Arama.BeginInit();
		gridView_Arama.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama.OptionsView.ShowGroupPanel = false;
		gridView_Arama.EndInit();
		_RepositoryItem.BeginInit();
		_RepositoryItem.AutoHeight = false;
		_RepositoryItem.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		_RepositoryItem.View = gridView_Arama;
		_RepositoryItem.EndInit();
		_RepositoryItem.AccessibleName = _Role.ToString();
		DataGuncelle(_lookuptablolar);
		_RepositoryItem.PopulateViewColumns();
		_RepositoryItem.BestFitMode = BestFitMode.BestFitResizePopup;
		_RepositoryItem.ValidateOnEnterKey = true;
		_RepositoryItem.TextEditStyle = TextEditStyles.Standard;
		_RepositoryItem.AutoComplete = false;
		_RepositoryItem.CloseUp += repositoryItem_CloseUp;
		_RepositoryItem.QueryPopUp += repositoryItem_QueryPopUp;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += otomatikDosyayiSil_Click;
		_ToolStripMenuItemAnaBaslik = new ToolStripMenuItem();
		_ToolStripMenuItemAnaBaslik.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		_ToolStripMenuItemAnaBaslik.Size = new Size(152, 22);
		_ToolStripMenuItemAnaBaslik.Text = _Role.ToString();
	}

	private void repositoryItem_CloseUp(object sender, CloseUpEventArgs e)
	{
		_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(_temp_defaultlayoutStream);
	}

	private void repositoryItem_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_" + _Role.ToString() + ".lcr"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_" + _Role.ToString() + ".lcr");
		}
		else
		{
			_tempview.PopulateColumns();
			_tempview.BestFitColumns();
		}
	}

	private void otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_" + _Role.ToString() + ".lcr", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[_temp_defaultlayoutStream.Length];
		_temp_defaultlayoutStream.Read(array, 0, (int)_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_" + _Role.ToString() + ".lcr"))
		{
			File.Delete("data\\views\\" + _tag + "_" + _Role.ToString() + ".lcr");
		}
	}

	public void DataGuncelle(DataSet _lookuptablolar)
	{
		string name = "";
		string valueMember = "";
		string displayMember = "";
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.CariKodu)
		{
			name = "CARI_HESAPLAR";
			valueMember = "KOD";
			displayMember = "KOD";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.CariUnvan)
		{
			name = "CARI_HESAPLAR";
			valueMember = "KOD";
			displayMember = "İSİM";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.StokKodu)
		{
			name = "STOKLAR";
			valueMember = "KOD";
			displayMember = "KOD";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.StokAdi)
		{
			name = "STOKLAR";
			valueMember = "KOD";
			displayMember = "İSİM";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.HizmetKodu)
		{
			name = "HIZMET_HESAPLARI";
			valueMember = "KOD";
			displayMember = "KOD";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.HizmetAdi)
		{
			name = "HIZMET_HESAPLARI";
			valueMember = "KOD";
			displayMember = "İSİM";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.KasaKodu)
		{
			name = "KASALAR";
			valueMember = "KOD";
			displayMember = "KOD";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.KasaAdi)
		{
			name = "KASALAR";
			valueMember = "KOD";
			displayMember = "İSİM";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.BankaKodu)
		{
			name = "BANKALAR";
			valueMember = "KOD";
			displayMember = "KOD";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.BankaAdi)
		{
			name = "BANKALAR";
			valueMember = "KOD";
			displayMember = "İSİM";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.CariPersonelKodu)
		{
			name = "CARI_PERSONEL_TANIMLARI";
			valueMember = "KOD";
			displayMember = "KOD";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.CariPersonelAdi)
		{
			name = "CARI_PERSONEL_TANIMLARI";
			valueMember = "KOD";
			displayMember = "İSİM";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.ProjeKodu)
		{
			name = "PROJELER";
			valueMember = "PROJE KODU";
			displayMember = "PROJE KODU";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.ProjeAdi)
		{
			name = "PROJELER";
			valueMember = "PROJE KODU";
			displayMember = "PROJE ADI";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.SorumlulukMerkeziKodu)
		{
			name = "SORUMLULUK_MERKEZLERI";
			valueMember = "KODU";
			displayMember = "KODU";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.SorumlulukMerkeziAdi)
		{
			name = "SORUMLULUK_MERKEZLERI";
			valueMember = "KODU";
			displayMember = "ADI";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.DemirbasKodu)
		{
			name = "DEMIRBASLAR";
			valueMember = "KOD";
			displayMember = "KOD";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.DemirbasAdi)
		{
			name = "DEMIRBASLAR";
			valueMember = "KOD";
			displayMember = "İSİM";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.MasrafKodu)
		{
			name = "MASRAF_HESAPLARI";
			valueMember = "KOD";
			displayMember = "KOD";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.MasrafAdi)
		{
			name = "MASRAF_HESAPLARI";
			valueMember = "KOD";
			displayMember = "İSİM";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.PersonelKodu)
		{
			name = "PERSONELLER";
			valueMember = "KOD";
			displayMember = "KOD";
		}
		if (_Role == enum_DevExpressRepositoryItemGridLookUpEdit.PersonelAdi)
		{
			name = "PERSONELLER";
			valueMember = "KOD";
			displayMember = "İSİM";
		}
		_RepositoryItem.DataSource = _lookuptablolar.Tables[name];
		_RepositoryItem.ValueMember = valueMember;
		_RepositoryItem.DisplayMember = displayMember;
	}
}
