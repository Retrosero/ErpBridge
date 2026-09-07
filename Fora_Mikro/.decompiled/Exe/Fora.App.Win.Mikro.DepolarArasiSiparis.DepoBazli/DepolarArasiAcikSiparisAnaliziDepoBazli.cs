using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Fora.App.Win.Mikro.RaporBase;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;

namespace Fora.App.Win.Mikro.DepolarArasiSiparis.DepoBazli;

public class DepolarArasiAcikSiparisAnaliziDepoBazli : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private List<RaporMaster> rapormaster;

	private Color RenkBeyaz = Color.White;

	private Color RenkYesil = Color.LightGreen;

	private Color RenkSari = Color.Yellow;

	private Color RenkKirmizi = Color.Salmon;

	private IContainer components;

	private SimpleButton sb_RaporOlustur;

	private MenuStrip menuStrip1;

	public GridControl gridControl1;

	private GridLookUpEdit gridLookUpEdit_depo;

	private GridView gridLookUpEdit1View;

	private LabelControl labelControl1;

	private AdvBandedGridView advBandedGridView_detail;

	private BandedGridColumn gridColumn7;

	private BandedGridColumn gridColumn8;

	private BandedGridColumn gridColumn9;

	private BandedGridColumn gridColumn10;

	private BandedGridColumn gridColumn11;

	private BandedGridColumn gridColumn12;

	private BandedGridColumn gridColumn13;

	private BandedGridColumn gridColumn14;

	private BandedGridColumn gridColumn15;

	private AdvBandedGridView advBandedGridView_master;

	private BandedGridColumn gridColumn1;

	private BandedGridColumn gridColumn2;

	private GridBand gridBand2;

	private SpinEdit spinEdit_OrtalamaSatisSonKacGun;

	private LabelControl labelControl2;

	private LabelControl labelControl3;

	private SpinEdit spinEdit_HedefMiktarKacGunluk;

	private LabelControl labelControl4;

	private SpinEdit spinEdit_MinimumYuzde;

	private LabelControl labelControl5;

	private SpinEdit spinEdit_MaksimumYuzde;

	private BandedGridColumn bandedGridColumn1;

	private GridBand gridBand1;

	public DepolarArasiAcikSiparisAnaliziDepoBazli(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.Size = new Size(400, 300);
		new DataSet();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		gridLookUpEdit_depo.Properties.DataSource = DepoData.GetDepolarDataTable(sqlDB.Connection);
		gridLookUpEdit_depo.Properties.DisplayMember = "DEPO ADI";
		gridLookUpEdit_depo.Properties.ValueMember = "DEPO NO";
		GridColumn gridColumn = gridLookUpEdit_depo.Properties.View.Columns.AddField("DEPO NO");
		gridColumn.Caption = "DEPO NO";
		gridColumn.VisibleIndex = 0;
		gridLookUpEdit_depo.Properties.View.Columns.Add(gridColumn);
		GridColumn gridColumn2 = gridLookUpEdit_depo.Properties.View.Columns.AddField("DEPO ADI");
		gridColumn2.Caption = "DEPO ADI";
		gridColumn2.VisibleIndex = 1;
		gridLookUpEdit_depo.Properties.View.Columns.Add(gridColumn2);
		sqlDB.ConnectionClose();
		gridLookUpEdit_depo.EditValue = 1;
		advBandedGridView_master.BeginInit();
		advBandedGridView_master.Appearance.SelectedRow.BackColor = Color.FromArgb(50, 0, 0, 0);
		advBandedGridView_master.EndInit();
		advBandedGridView_detail.BeginInit();
		advBandedGridView_detail.Appearance.SelectedRow.BackColor = Color.FromArgb(50, 0, 0, 0);
		advBandedGridView_detail.EndInit();
	}

	private void sb_RaporOlustur_Click(object sender, EventArgs e)
	{
		RaporOlustur();
		gridControl1.DataSource = rapormaster;
		advBandedGridView_master.BestFitColumns();
		advBandedGridView_detail.BestFitColumns();
		new Fora.App.Win.Mikro.RaporBase.RaporBase(_mikrouygulamabilgileri, "depolar-arasi-acik-siparis-analizi-depo-bazli", "Depolar arası açık sipariş analizi (Depo bazlı)", gridControl1).ShowDialog();
	}

	private void RaporOlustur()
	{
		rapormaster = new List<RaporMaster>();
		int num = (int)gridLookUpEdit_depo.EditValue;
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		string commandText = "SELECT ssip_stok_kod,sto_isim,SUM(ssip_miktar-ssip_teslim_miktar) AS kalanmiktar,sto_birim1_ad,ssip_girdepo, (SELECT TOP 1 dep_adi FROM DEPOLAR WITH(NOLOCK) WHERE dep_no=ssip_girdepo) AS ssip_girdepo_adi FROM DEPOLAR_ARASI_SIPARISLER AS siparis WITH (NOLOCK) INNER JOIN STOKLAR AS stoklar WITH (NOLOCK) ON ssip_stok_kod=sto_kod WHERE (ssip_miktar-ssip_teslim_miktar)>0 AND ssip_cikdepo=@ssip_cikdepo GROUP BY ssip_stok_kod,sto_isim,sto_birim1_ad,ssip_girdepo ORDER BY ssip_stok_kod";
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@ssip_cikdepo", num);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				RaporDetail raporDetail = new RaporDetail();
				raporDetail.Birim1Adi = sqlDataReader.GetSafeString(3);
				raporDetail.DepoIsmi = sqlDataReader.GetSafeString(5);
				raporDetail.DepoNo = sqlDataReader.GetSafeInt32(4);
				raporDetail.StokIsmi = sqlDataReader.GetSafeString(1);
				raporDetail.StokKodu = sqlDataReader.GetSafeString(0);
				raporDetail.SiparisMiktari = sqlDataReader.GetSafeDouble(2);
				bool flag = false;
				foreach (RaporMaster item in rapormaster)
				{
					if (item.DepoNo == raporDetail.DepoNo)
					{
						item.Detaylar.Add(raporDetail);
						item.ToplamSiparisMiktari += raporDetail.SiparisMiktari;
						flag = true;
					}
				}
				if (!flag)
				{
					RaporMaster raporMaster = new RaporMaster();
					raporMaster.DepoIsmi = raporDetail.DepoIsmi;
					raporMaster.DepoNo = raporDetail.DepoNo;
					raporMaster.Durum = true;
					raporMaster.ToplamSiparisMiktari = raporDetail.SiparisMiktari;
					raporMaster.Detaylar = new List<RaporDetail>();
					raporMaster.Detaylar.Add(raporDetail);
					rapormaster.Add(raporMaster);
				}
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		foreach (RaporMaster item2 in rapormaster)
		{
			bool durum = true;
			foreach (RaporDetail item3 in item2.Detaylar)
			{
				item3.DepoMevcudu = StokData.GetDepoMiktar(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item3.StokKodu, item3.DepoNo);
				DateTime sontarih = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
				DateTime ilktarih = sontarih.AddDays(-1.0 * (double)spinEdit_OrtalamaSatisSonKacGun.Value);
				double satisMiktari = StokData.GetSatisMiktari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item3.StokKodu, item3.DepoNo, ilktarih, sontarih);
				item3.DepoOrtalamaGunlukSatisMiktari = Math.Round(satisMiktari / (double)spinEdit_OrtalamaSatisSonKacGun.Value, 2);
				item3.SiparisSonrasiDepoMevcudu = item3.DepoMevcudu + item3.SiparisMiktari;
				item3.HedefMiktar = Math.Round(item3.DepoOrtalamaGunlukSatisMiktari * (double)spinEdit_HedefMiktarKacGunluk.Value, 2);
				if (item3.HedefMiktar != 0.0)
				{
					item3.DurumYuzde = (int)Math.Round(item3.SiparisSonrasiDepoMevcudu * 100.0 / item3.HedefMiktar, 0);
				}
				else
				{
					item3.DurumYuzde = 1000;
				}
				item3.Durum = true;
				if (item3.DurumYuzde > (int)spinEdit_MaksimumYuzde.Value)
				{
					item3.Durum = false;
					durum = false;
				}
				if (item3.DurumYuzde < (int)spinEdit_MinimumYuzde.Value)
				{
					item3.Durum = false;
					durum = false;
				}
			}
			item2.Durum = durum;
		}
	}

	private void advBandedGridView_master_RowStyle(object sender, RowStyleEventArgs e)
	{
		AdvBandedGridView advBandedGridView = sender as AdvBandedGridView;
		if (e.RowHandle >= 0)
		{
			if (!rapormaster[advBandedGridView.GetDataSourceRowIndex(e.RowHandle)].Durum)
			{
				e.Appearance.BackColor = RenkKirmizi;
				e.Appearance.BackColor2 = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkYesil;
				e.Appearance.BackColor2 = RenkYesil;
			}
		}
	}

	private void advBandedGridView_detail_RowStyle(object sender, RowStyleEventArgs e)
	{
		AdvBandedGridView advBandedGridView = sender as AdvBandedGridView;
		if (e.RowHandle >= 0)
		{
			if (!((RaporDetail)advBandedGridView.GetRow(e.RowHandle)).Durum)
			{
				e.Appearance.BackColor = RenkKirmizi;
				e.Appearance.BackColor2 = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkYesil;
				e.Appearance.BackColor2 = RenkYesil;
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
		DevExpress.XtraGrid.GridLevelNode gridLevelNode = new DevExpress.XtraGrid.GridLevelNode();
		this.advBandedGridView_detail = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridBand2 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn8 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn9 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn10 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn11 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn12 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn13 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn14 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn15 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridControl1 = new DevExpress.XtraGrid.GridControl();
		this.advBandedGridView_master = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.sb_RaporOlustur = new DevExpress.XtraEditors.SimpleButton();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.gridLookUpEdit_depo = new DevExpress.XtraEditors.GridLookUpEdit();
		this.gridLookUpEdit1View = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.spinEdit_OrtalamaSatisSonKacGun = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.spinEdit_HedefMiktarKacGunluk = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.spinEdit_MinimumYuzde = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.spinEdit_MaksimumYuzde = new DevExpress.XtraEditors.SpinEdit();
		this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_detail).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridLookUpEdit_depo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridLookUpEdit1View).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.spinEdit_OrtalamaSatisSonKacGun.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.spinEdit_HedefMiktarKacGunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.spinEdit_MinimumYuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.spinEdit_MaksimumYuzde.Properties).BeginInit();
		base.SuspendLayout();
		this.advBandedGridView_detail.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[1] { this.gridBand2 });
		this.advBandedGridView_detail.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[9] { this.gridColumn7, this.gridColumn8, this.gridColumn9, this.gridColumn10, this.gridColumn11, this.gridColumn12, this.gridColumn13, this.gridColumn14, this.gridColumn15 });
		this.advBandedGridView_detail.GridControl = this.gridControl1;
		this.advBandedGridView_detail.Name = "advBandedGridView_detail";
		this.advBandedGridView_detail.OptionsView.ShowGroupPanel = false;
		this.advBandedGridView_detail.OptionsView.ShowIndicator = false;
		this.advBandedGridView_detail.RowStyle += new DevExpress.XtraGrid.Views.Grid.RowStyleEventHandler(advBandedGridView_detail_RowStyle);
		this.gridBand2.Caption = "Siparişler";
		this.gridBand2.Columns.Add(this.gridColumn7);
		this.gridBand2.Columns.Add(this.gridColumn8);
		this.gridBand2.Columns.Add(this.gridColumn9);
		this.gridBand2.Columns.Add(this.gridColumn10);
		this.gridBand2.Columns.Add(this.gridColumn11);
		this.gridBand2.Columns.Add(this.gridColumn12);
		this.gridBand2.Columns.Add(this.gridColumn13);
		this.gridBand2.Columns.Add(this.gridColumn14);
		this.gridBand2.Columns.Add(this.gridColumn15);
		this.gridBand2.Name = "gridBand2";
		this.gridBand2.Width = 675;
		this.gridColumn7.Caption = "Stok kodu";
		this.gridColumn7.FieldName = "StokKodu";
		this.gridColumn7.Name = "gridColumn7";
		this.gridColumn7.OptionsColumn.AllowEdit = false;
		this.gridColumn7.Visible = true;
		this.gridColumn8.Caption = "Stok İsmi";
		this.gridColumn8.FieldName = "StokIsmi";
		this.gridColumn8.Name = "gridColumn8";
		this.gridColumn8.OptionsColumn.AllowEdit = false;
		this.gridColumn8.Visible = true;
		this.gridColumn9.Caption = "Birim";
		this.gridColumn9.FieldName = "Birim1Adi";
		this.gridColumn9.Name = "gridColumn9";
		this.gridColumn9.OptionsColumn.AllowEdit = false;
		this.gridColumn9.Visible = true;
		this.gridColumn10.Caption = "Sipariş miktarı";
		this.gridColumn10.FieldName = "SiparisMiktari";
		this.gridColumn10.Name = "gridColumn10";
		this.gridColumn10.OptionsColumn.AllowEdit = false;
		this.gridColumn10.Visible = true;
		this.gridColumn11.Caption = "Depo mevcudu";
		this.gridColumn11.FieldName = "DepoMevcudu";
		this.gridColumn11.Name = "gridColumn11";
		this.gridColumn11.OptionsColumn.AllowEdit = false;
		this.gridColumn11.Visible = true;
		this.gridColumn12.Caption = "Sipariş sonrası depo mevcudu";
		this.gridColumn12.FieldName = "SiparisSonrasiDepoMevcudu";
		this.gridColumn12.Name = "gridColumn12";
		this.gridColumn12.OptionsColumn.AllowEdit = false;
		this.gridColumn12.Visible = true;
		this.gridColumn13.Caption = "Hedef miktar";
		this.gridColumn13.FieldName = "HedefMiktar";
		this.gridColumn13.Name = "gridColumn13";
		this.gridColumn13.OptionsColumn.AllowEdit = false;
		this.gridColumn13.Visible = true;
		this.gridColumn14.Caption = "Depo ortalama günlük satış miktarı";
		this.gridColumn14.FieldName = "DepoOrtalamaGunlukSatisMiktari";
		this.gridColumn14.Name = "gridColumn14";
		this.gridColumn14.OptionsColumn.AllowEdit = false;
		this.gridColumn14.Visible = true;
		this.gridColumn15.Caption = "Durum";
		this.gridColumn15.FieldName = "DurumYuzde";
		this.gridColumn15.Name = "gridColumn15";
		this.gridColumn15.OptionsColumn.AllowEdit = false;
		this.gridColumn15.Visible = true;
		gridLevelNode.LevelTemplate = this.advBandedGridView_detail;
		gridLevelNode.RelationName = "Detaylar";
		this.gridControl1.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[1] { gridLevelNode });
		this.gridControl1.Location = new System.Drawing.Point(467, 172);
		this.gridControl1.MainView = this.advBandedGridView_master;
		this.gridControl1.Name = "gridControl1";
		this.gridControl1.Size = new System.Drawing.Size(469, 300);
		this.gridControl1.TabIndex = 15;
		this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[2] { this.advBandedGridView_master, this.advBandedGridView_detail });
		this.advBandedGridView_master.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[1] { this.gridBand1 });
		this.advBandedGridView_master.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[3] { this.gridColumn1, this.gridColumn2, this.bandedGridColumn1 });
		this.advBandedGridView_master.GridControl = this.gridControl1;
		this.advBandedGridView_master.Name = "advBandedGridView_master";
		this.advBandedGridView_master.OptionsView.ShowGroupPanel = false;
		this.advBandedGridView_master.OptionsView.ShowIndicator = false;
		this.advBandedGridView_master.RowStyle += new DevExpress.XtraGrid.Views.Grid.RowStyleEventHandler(advBandedGridView_master_RowStyle);
		this.gridColumn1.Caption = "Depo no";
		this.gridColumn1.FieldName = "DepoNo";
		this.gridColumn1.Name = "gridColumn1";
		this.gridColumn1.OptionsColumn.AllowEdit = false;
		this.gridColumn1.Visible = true;
		this.gridColumn1.Width = 65;
		this.gridColumn2.Caption = "Depo ismi";
		this.gridColumn2.FieldName = "DepoIsmi";
		this.gridColumn2.Name = "gridColumn2";
		this.gridColumn2.OptionsColumn.AllowEdit = false;
		this.gridColumn2.Visible = true;
		this.gridColumn2.Width = 230;
		this.sb_RaporOlustur.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_RaporOlustur.Appearance.Options.UseFont = true;
		this.sb_RaporOlustur.Location = new System.Drawing.Point(160, 212);
		this.sb_RaporOlustur.Name = "sb_RaporOlustur";
		this.sb_RaporOlustur.Size = new System.Drawing.Size(143, 23);
		this.sb_RaporOlustur.TabIndex = 7;
		this.sb_RaporOlustur.Text = "RAPOR OLUŞTUR";
		this.sb_RaporOlustur.Click += new System.EventHandler(sb_RaporOlustur_Click);
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(954, 24);
		this.menuStrip1.TabIndex = 1;
		this.menuStrip1.Text = "menuStrip1";
		this.gridLookUpEdit_depo.Location = new System.Drawing.Point(160, 61);
		this.gridLookUpEdit_depo.Name = "gridLookUpEdit_depo";
		this.gridLookUpEdit_depo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.gridLookUpEdit_depo.Properties.View = this.gridLookUpEdit1View;
		this.gridLookUpEdit_depo.Size = new System.Drawing.Size(143, 20);
		this.gridLookUpEdit_depo.TabIndex = 2;
		this.gridLookUpEdit1View.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridLookUpEdit1View.Name = "gridLookUpEdit1View";
		this.gridLookUpEdit1View.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridLookUpEdit1View.OptionsView.ShowGroupPanel = false;
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(160, 42);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(143, 13);
		this.labelControl1.TabIndex = 5;
		this.labelControl1.Text = "Kaynak Depo";
		this.spinEdit_OrtalamaSatisSonKacGun.EditValue = new decimal(new int[4] { 15, 0, 0, 0 });
		this.spinEdit_OrtalamaSatisSonKacGun.Location = new System.Drawing.Point(236, 98);
		this.spinEdit_OrtalamaSatisSonKacGun.Name = "spinEdit_OrtalamaSatisSonKacGun";
		this.spinEdit_OrtalamaSatisSonKacGun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.spinEdit_OrtalamaSatisSonKacGun.Size = new System.Drawing.Size(67, 20);
		this.spinEdit_OrtalamaSatisSonKacGun.TabIndex = 3;
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(45, 87);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(185, 31);
		this.labelControl2.TabIndex = 7;
		this.labelControl2.Text = "Ortalama satış miktarı hesaplanırken son kaç gün dikkate alınsın :";
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(45, 125);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(185, 17);
		this.labelControl3.TabIndex = 9;
		this.labelControl3.Text = "Hedef miktar kaç günlük satış olsun :";
		this.spinEdit_HedefMiktarKacGunluk.EditValue = new decimal(new int[4] { 2, 0, 0, 0 });
		this.spinEdit_HedefMiktarKacGunluk.Location = new System.Drawing.Point(236, 124);
		this.spinEdit_HedefMiktarKacGunluk.Name = "spinEdit_HedefMiktarKacGunluk";
		this.spinEdit_HedefMiktarKacGunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.spinEdit_HedefMiktarKacGunluk.Size = new System.Drawing.Size(67, 20);
		this.spinEdit_HedefMiktarKacGunluk.TabIndex = 4;
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(45, 151);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(185, 17);
		this.labelControl4.TabIndex = 11;
		this.labelControl4.Text = "Sipariş durumu minimum yüzde :";
		this.spinEdit_MinimumYuzde.EditValue = new decimal(new int[4] { 100, 0, 0, 0 });
		this.spinEdit_MinimumYuzde.Location = new System.Drawing.Point(236, 150);
		this.spinEdit_MinimumYuzde.Name = "spinEdit_MinimumYuzde";
		this.spinEdit_MinimumYuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.spinEdit_MinimumYuzde.Size = new System.Drawing.Size(67, 20);
		this.spinEdit_MinimumYuzde.TabIndex = 5;
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(45, 177);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(185, 17);
		this.labelControl5.TabIndex = 13;
		this.labelControl5.Text = "Sipariş durumu maksimum yüzde :";
		this.spinEdit_MaksimumYuzde.EditValue = new decimal(new int[4] { 130, 0, 0, 0 });
		this.spinEdit_MaksimumYuzde.Location = new System.Drawing.Point(236, 176);
		this.spinEdit_MaksimumYuzde.Name = "spinEdit_MaksimumYuzde";
		this.spinEdit_MaksimumYuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.spinEdit_MaksimumYuzde.Size = new System.Drawing.Size(67, 20);
		this.spinEdit_MaksimumYuzde.TabIndex = 6;
		this.bandedGridColumn1.Caption = "Toplam sipariş miktarı";
		this.bandedGridColumn1.FieldName = "ToplamSiparisMiktari";
		this.bandedGridColumn1.Name = "bandedGridColumn1";
		this.bandedGridColumn1.Visible = true;
		this.bandedGridColumn1.Width = 162;
		this.gridBand1.Caption = "Depolar";
		this.gridBand1.Columns.Add(this.gridColumn1);
		this.gridBand1.Columns.Add(this.gridColumn2);
		this.gridBand1.Columns.Add(this.bandedGridColumn1);
		this.gridBand1.Name = "gridBand1";
		this.gridBand1.Width = 457;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(954, 484);
		base.Controls.Add(this.labelControl5);
		base.Controls.Add(this.spinEdit_MaksimumYuzde);
		base.Controls.Add(this.labelControl4);
		base.Controls.Add(this.spinEdit_MinimumYuzde);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.spinEdit_HedefMiktarKacGunluk);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.spinEdit_OrtalamaSatisSonKacGun);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.gridLookUpEdit_depo);
		base.Controls.Add(this.gridControl1);
		base.Controls.Add(this.sb_RaporOlustur);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "DepolarArasiAcikSiparisAnaliziDepoBazli";
		this.Text = "Depolar arası açık sipariş analizi (Depo bazlı)";
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_detail).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridLookUpEdit_depo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridLookUpEdit1View).EndInit();
		((System.ComponentModel.ISupportInitialize)this.spinEdit_OrtalamaSatisSonKacGun.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.spinEdit_HedefMiktarKacGunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.spinEdit_MinimumYuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.spinEdit_MaksimumYuzde.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
