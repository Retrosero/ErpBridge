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

namespace Fora.App.Win.Mikro.DepolarArasiSiparis.StokBazli;

public class DepolarArasiAcikSiparisAnaliziStokBazli : XtraForm
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

	private BandedGridColumn gridColumn16;

	private BandedGridColumn gridColumn17;

	private BandedGridColumn gridColumn18;

	private AdvBandedGridView advBandedGridView_master;

	private BandedGridColumn gridColumn1;

	private BandedGridColumn gridColumn2;

	private BandedGridColumn gridColumn3;

	private BandedGridColumn gridColumn4;

	private BandedGridColumn gridColumn5;

	private BandedGridColumn gridColumn6;

	private GridBand gridBand1;

	private GridBand gridBand2;

	public DepolarArasiAcikSiparisAnaliziStokBazli(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.Size = new Size(300, 200);
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
		new Fora.App.Win.Mikro.RaporBase.RaporBase(_mikrouygulamabilgileri, "depolar-arasi-acik-siparis-analizi-stok-bazli", "Depolar arası açık sipariş analizi (Stok bazlı)", gridControl1).ShowDialog();
	}

	private void RaporOlustur()
	{
		rapormaster = new List<RaporMaster>();
		int num = (int)gridLookUpEdit_depo.EditValue;
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		string commandText = "SELECT ssip_tarih,ssip_teslim_tarih,ssip_evrakno_seri,ssip_evrakno_sira,ssip_stok_kod,sto_isim,ssip_miktar,ssip_teslim_miktar,(ssip_miktar-ssip_teslim_miktar) AS kalanmiktar,sto_birim1_ad,ssip_b_fiyat,ssip_tutar,ssip_aciklama,ssip_girdepo,(SELECT TOP 1 dep_adi FROM DEPOLAR WITH(NOLOCK) WHERE dep_no=ssip_girdepo) AS ssip_girdepo_adi,ssip_cikdepo,(SELECT TOP 1 dep_adi FROM DEPOLAR WITH(NOLOCK) WHERE dep_no=ssip_cikdepo) AS ssip_cikdepo_adi\t FROM DEPOLAR_ARASI_SIPARISLER AS siparis WITH (NOLOCK) INNER JOIN STOKLAR AS stoklar WITH (NOLOCK) ON ssip_stok_kod=sto_kod WHERE (ssip_miktar-ssip_teslim_miktar)>0 AND ssip_cikdepo=@ssip_cikdepo ORDER BY ssip_teslim_tarih";
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@ssip_cikdepo", num);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				RaporDetail raporDetail = new RaporDetail();
				raporDetail.Tarih = sqlDataReader.GetSafeDateTime(0);
				raporDetail.TeslimTarihi = sqlDataReader.GetSafeDateTime(1);
				raporDetail.EvraknoSeri = sqlDataReader.GetSafeString(2);
				raporDetail.EvraknoSira = sqlDataReader.GetSafeInt32(3);
				raporDetail.StokKodu = sqlDataReader.GetSafeString(4);
				raporDetail.StokIsmi = sqlDataReader.GetSafeString(5);
				raporDetail.Miktar = sqlDataReader.GetSafeDouble(6);
				raporDetail.TeslimMiktar = sqlDataReader.GetSafeDouble(7);
				raporDetail.KalanMiktar = sqlDataReader.GetSafeDouble(8);
				raporDetail.Birim1Adi = sqlDataReader.GetSafeString(9);
				raporDetail.BirimFiyat = sqlDataReader.GetSafeDouble(10);
				raporDetail.Tutar = sqlDataReader.GetSafeDouble(11);
				raporDetail.Aciklama = sqlDataReader.GetSafeString(12);
				raporDetail.GirenDepoNo = sqlDataReader.GetSafeInt32(13);
				raporDetail.GirenDepoAdi = sqlDataReader.GetSafeString(14);
				raporDetail.CikanDepoNo = sqlDataReader.GetSafeInt32(15);
				raporDetail.CikanDepoAdi = sqlDataReader.GetSafeString(16);
				bool flag = false;
				foreach (RaporMaster item in rapormaster)
				{
					if (item.StokKodu == raporDetail.StokKodu)
					{
						item.Detaylar.Add(raporDetail);
						flag = true;
					}
				}
				if (!flag)
				{
					RaporMaster raporMaster = new RaporMaster();
					raporMaster.Birim1Adi = raporDetail.Birim1Adi;
					raporMaster.MerkezDepoMiktari = 0.0;
					raporMaster.StokIsmi = raporDetail.StokIsmi;
					raporMaster.StokKodu = raporDetail.StokKodu;
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
			item2.MerkezDepoMiktari = StokData.GetDepoMiktar(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item2.StokKodu, num);
		}
	}

	private void advBandedGridView_master_RowStyle(object sender, RowStyleEventArgs e)
	{
		AdvBandedGridView advBandedGridView = sender as AdvBandedGridView;
		if (e.RowHandle >= 0)
		{
			if (rapormaster[advBandedGridView.GetDataSourceRowIndex(e.RowHandle)].EksikMiktar > 0.0)
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
		this.gridColumn16 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn17 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn18 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridControl1 = new DevExpress.XtraGrid.GridControl();
		this.advBandedGridView_master = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.sb_RaporOlustur = new DevExpress.XtraEditors.SimpleButton();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.gridLookUpEdit_depo = new DevExpress.XtraEditors.GridLookUpEdit();
		this.gridLookUpEdit1View = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_detail).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridLookUpEdit_depo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridLookUpEdit1View).BeginInit();
		base.SuspendLayout();
		this.advBandedGridView_detail.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[1] { this.gridBand2 });
		this.advBandedGridView_detail.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[12]
		{
			this.gridColumn7, this.gridColumn8, this.gridColumn9, this.gridColumn10, this.gridColumn11, this.gridColumn12, this.gridColumn13, this.gridColumn14, this.gridColumn15, this.gridColumn16,
			this.gridColumn17, this.gridColumn18
		});
		this.advBandedGridView_detail.GridControl = this.gridControl1;
		this.advBandedGridView_detail.Name = "advBandedGridView_detail";
		this.advBandedGridView_detail.OptionsView.ShowGroupPanel = false;
		this.advBandedGridView_detail.OptionsView.ShowIndicator = false;
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
		this.gridBand2.Columns.Add(this.gridColumn16);
		this.gridBand2.Columns.Add(this.gridColumn17);
		this.gridBand2.Columns.Add(this.gridColumn18);
		this.gridBand2.Name = "gridBand2";
		this.gridBand2.Width = 900;
		this.gridColumn7.Caption = "Tarih";
		this.gridColumn7.FieldName = "Tarih";
		this.gridColumn7.Name = "gridColumn7";
		this.gridColumn7.OptionsColumn.AllowEdit = false;
		this.gridColumn7.Visible = true;
		this.gridColumn8.Caption = "Teslim tarihi";
		this.gridColumn8.FieldName = "TeslimTarihi";
		this.gridColumn8.Name = "gridColumn8";
		this.gridColumn8.OptionsColumn.AllowEdit = false;
		this.gridColumn8.Visible = true;
		this.gridColumn9.Caption = "Evrak seri-sıra";
		this.gridColumn9.FieldName = "EvrakSeriSira";
		this.gridColumn9.Name = "gridColumn9";
		this.gridColumn9.OptionsColumn.AllowEdit = false;
		this.gridColumn9.Visible = true;
		this.gridColumn10.Caption = "Sipariş miktarı";
		this.gridColumn10.FieldName = "Miktar";
		this.gridColumn10.Name = "gridColumn10";
		this.gridColumn10.OptionsColumn.AllowEdit = false;
		this.gridColumn10.Visible = true;
		this.gridColumn11.Caption = "Teslim edilen miktar";
		this.gridColumn11.FieldName = "TeslimMiktar";
		this.gridColumn11.Name = "gridColumn11";
		this.gridColumn11.OptionsColumn.AllowEdit = false;
		this.gridColumn11.Visible = true;
		this.gridColumn12.Caption = "Kalan miktar";
		this.gridColumn12.FieldName = "KalanMiktar";
		this.gridColumn12.Name = "gridColumn12";
		this.gridColumn12.OptionsColumn.AllowEdit = false;
		this.gridColumn12.Visible = true;
		this.gridColumn13.Caption = "Birim";
		this.gridColumn13.FieldName = "Birim1Adi";
		this.gridColumn13.Name = "gridColumn13";
		this.gridColumn13.OptionsColumn.AllowEdit = false;
		this.gridColumn13.Visible = true;
		this.gridColumn14.Caption = "Birim fiyat";
		this.gridColumn14.FieldName = "BirimFiyat";
		this.gridColumn14.Name = "gridColumn14";
		this.gridColumn14.OptionsColumn.AllowEdit = false;
		this.gridColumn14.Visible = true;
		this.gridColumn15.Caption = "Tutar";
		this.gridColumn15.FieldName = "Tutar";
		this.gridColumn15.Name = "gridColumn15";
		this.gridColumn15.OptionsColumn.AllowEdit = false;
		this.gridColumn15.Visible = true;
		this.gridColumn16.Caption = "Açıklama";
		this.gridColumn16.FieldName = "Aciklama";
		this.gridColumn16.Name = "gridColumn16";
		this.gridColumn16.OptionsColumn.AllowEdit = false;
		this.gridColumn16.Visible = true;
		this.gridColumn17.Caption = "Depo no";
		this.gridColumn17.FieldName = "GirenDepoNo";
		this.gridColumn17.Name = "gridColumn17";
		this.gridColumn17.OptionsColumn.AllowEdit = false;
		this.gridColumn17.Visible = true;
		this.gridColumn18.Caption = "Depo adı";
		this.gridColumn18.FieldName = "GirenDepoAdi";
		this.gridColumn18.Name = "gridColumn18";
		this.gridColumn18.OptionsColumn.AllowEdit = false;
		this.gridColumn18.Visible = true;
		gridLevelNode.LevelTemplate = this.advBandedGridView_detail;
		gridLevelNode.RelationName = "Detaylar";
		this.gridControl1.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[1] { gridLevelNode });
		this.gridControl1.Location = new System.Drawing.Point(419, 138);
		this.gridControl1.MainView = this.advBandedGridView_master;
		this.gridControl1.Name = "gridControl1";
		this.gridControl1.Size = new System.Drawing.Size(469, 300);
		this.gridControl1.TabIndex = 4;
		this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[2] { this.advBandedGridView_master, this.advBandedGridView_detail });
		this.advBandedGridView_master.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[1] { this.gridBand1 });
		this.advBandedGridView_master.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[6] { this.gridColumn1, this.gridColumn2, this.gridColumn3, this.gridColumn4, this.gridColumn5, this.gridColumn6 });
		this.advBandedGridView_master.GridControl = this.gridControl1;
		this.advBandedGridView_master.Name = "advBandedGridView_master";
		this.advBandedGridView_master.OptionsView.ShowGroupPanel = false;
		this.advBandedGridView_master.OptionsView.ShowIndicator = false;
		this.advBandedGridView_master.RowStyle += new DevExpress.XtraGrid.Views.Grid.RowStyleEventHandler(advBandedGridView_master_RowStyle);
		this.gridBand1.Caption = "Stoklar";
		this.gridBand1.Columns.Add(this.gridColumn1);
		this.gridBand1.Columns.Add(this.gridColumn2);
		this.gridBand1.Columns.Add(this.gridColumn3);
		this.gridBand1.Columns.Add(this.gridColumn4);
		this.gridBand1.Columns.Add(this.gridColumn5);
		this.gridBand1.Columns.Add(this.gridColumn6);
		this.gridBand1.Name = "gridBand1";
		this.gridBand1.Width = 450;
		this.gridColumn1.Caption = "Stok kodu";
		this.gridColumn1.FieldName = "StokKodu";
		this.gridColumn1.Name = "gridColumn1";
		this.gridColumn1.OptionsColumn.AllowEdit = false;
		this.gridColumn1.Visible = true;
		this.gridColumn2.Caption = "Stok ismi";
		this.gridColumn2.FieldName = "StokIsmi";
		this.gridColumn2.Name = "gridColumn2";
		this.gridColumn2.OptionsColumn.AllowEdit = false;
		this.gridColumn2.Visible = true;
		this.gridColumn3.Caption = "Birim";
		this.gridColumn3.FieldName = "Birim1Adi";
		this.gridColumn3.Name = "gridColumn3";
		this.gridColumn3.OptionsColumn.AllowEdit = false;
		this.gridColumn3.Visible = true;
		this.gridColumn4.Caption = "Merkez depo miktarı";
		this.gridColumn4.FieldName = "MerkezDepoMiktari";
		this.gridColumn4.Name = "gridColumn4";
		this.gridColumn4.OptionsColumn.AllowEdit = false;
		this.gridColumn4.Visible = true;
		this.gridColumn5.Caption = "Açık sipariş miktarı";
		this.gridColumn5.FieldName = "AcikSiparisMiktari";
		this.gridColumn5.Name = "gridColumn5";
		this.gridColumn5.OptionsColumn.AllowEdit = false;
		this.gridColumn5.Visible = true;
		this.gridColumn6.Caption = "Eksik miktar";
		this.gridColumn6.FieldName = "EksikMiktar";
		this.gridColumn6.Name = "gridColumn6";
		this.gridColumn6.OptionsColumn.AllowEdit = false;
		this.gridColumn6.Visible = true;
		this.sb_RaporOlustur.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_RaporOlustur.Appearance.Options.UseFont = true;
		this.sb_RaporOlustur.Location = new System.Drawing.Point(86, 105);
		this.sb_RaporOlustur.Name = "sb_RaporOlustur";
		this.sb_RaporOlustur.Size = new System.Drawing.Size(143, 23);
		this.sb_RaporOlustur.TabIndex = 3;
		this.sb_RaporOlustur.Text = "RAPOR OLUŞTUR";
		this.sb_RaporOlustur.Click += new System.EventHandler(sb_RaporOlustur_Click);
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(900, 24);
		this.menuStrip1.TabIndex = 1;
		this.menuStrip1.Text = "menuStrip1";
		this.gridLookUpEdit_depo.Location = new System.Drawing.Point(86, 79);
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
		this.labelControl1.Location = new System.Drawing.Point(86, 60);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(143, 13);
		this.labelControl1.TabIndex = 5;
		this.labelControl1.Text = "Kaynak Depo";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(900, 450);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.gridLookUpEdit_depo);
		base.Controls.Add(this.gridControl1);
		base.Controls.Add(this.sb_RaporOlustur);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "DepolarArasiAcikSiparisAnaliziStokBazli";
		this.Text = "Depolar arası açık sipariş analizi (Stok bazlı)";
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_detail).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridLookUpEdit_depo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridLookUpEdit1View).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
