using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Fora.App.Mikro.Cekilis;
using Fora.Mikro;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;

namespace Fora.App.Win.Mikro.Cekilis;

public class CekilisKuponVerme : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public Parametreler _KullaniciParametreleri;

	public Cari _cari;

	private int _Minimum_Stok_Reyon_Kodu_Sayisi = 10;

	private double _HerKacLiralikAlisverise = 10000.0;

	private int _ilk_kupon_kodu = 2480;

	private double _ciro;

	private double _stok_reyon_sayisi;

	private double _mevcut_kupon_sayisi;

	private double _acik_bakiye;

	private double _alabilecegi_kupon_sayisi;

	private IContainer components;

	private LabelControl labelControl4;

	private SimpleButton sb_kupon_kodu_olustur;

	private SimpleButton sb_yazdir;

	private LabelControl label_cari_kod;

	private LabelControl label_cari_unvan1;

	private LabelControl labelControl2;

	private LabelControl label_cari_unvan2;

	private LabelControl labelControl5;

	private LabelControl label_ciro;

	private LabelControl labelControl3;

	private LabelControl label_stok_reyon_sayisi;

	private LabelControl labelControl6;

	private LabelControl label_mevcut_kupon_sayisi;

	private LabelControl labelControl7;

	private LabelControl label_acik_bakiye;

	private LabelControl labelControl8;

	private LabelControl label_alabilecegi_kupon_sayisi;

	private LabelControl labelControl9;

	public CekilisKuponVerme(MikroUygulamaBilgileri mikrouygulamabilgileri, Cari cari)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_cari = cari;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		sb_kupon_kodu_olustur.Enabled = false;
		sb_yazdir.Enabled = false;
	}

	private void Giris_Load(object sender, EventArgs e)
	{
		Hesaplama_Yap();
		EkranGuncelle();
	}

	private void EkranGuncelle()
	{
		label_cari_kod.Text = _cari.cari_kod;
		label_cari_unvan1.Text = _cari.cari_unvan1;
		label_cari_unvan2.Text = _cari.cari_unvan2;
		label_ciro.Text = _ciro.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR"));
		label_stok_reyon_sayisi.Text = _stok_reyon_sayisi.ToString();
		label_mevcut_kupon_sayisi.Text = _mevcut_kupon_sayisi.ToString();
		label_acik_bakiye.Text = _acik_bakiye.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR"));
		label_alabilecegi_kupon_sayisi.Text = _alabilecegi_kupon_sayisi.ToString();
		if (_alabilecegi_kupon_sayisi > 0.0 && _stok_reyon_sayisi >= (double)_Minimum_Stok_Reyon_Kodu_Sayisi)
		{
			sb_kupon_kodu_olustur.Enabled = true;
		}
		else
		{
			sb_kupon_kodu_olustur.Enabled = false;
		}
		if (_mevcut_kupon_sayisi > 0.0)
		{
			sb_yazdir.Enabled = true;
		}
		else
		{
			sb_yazdir.Enabled = false;
		}
	}

	private void Hesaplama_Yap()
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		_ciro = 0.0;
		SqlCommand sqlCommand = new SqlCommand("SELECT ROUND (( ISNULL ((SELECT SUM(((cha_aratoplam -cha_ft_iskonto1- cha_ft_iskonto2-cha_ft_iskonto3 -cha_ft_iskonto4- cha_ft_iskonto5-cha_ft_iskonto6 ) * case when ( cha_tip=1 AND cha_normal_Iade= 1) THEN - 1 ELSE 1 END)*cha_d_kur ) FROM CARI_HESAP_HAREKETLERI WITH(NOLOCK) WHERE (cha_ciro_cari_kodu =@car_kod) AND ( (( cha_tip= 1 AND cha_normal_Iade = 1 AND cha_evrak_tip=0) OR (cha_tip= 0 AND cha_normal_Iade =0 AND cha_evrak_tip=63)) AND ((cha_grupno in (0 ,1, 2)) or ( cha_ciro_cari_kodu <> cha_kod)))), 0)), 2)", sqlDB.Connection);
		sqlCommand.Parameters.AddWithValue("@car_kod", _cari.cari_kod);
		object obj = sqlCommand.ExecuteScalar();
		_ciro = double.Parse(obj.ToString());
		sqlCommand.Dispose();
		_stok_reyon_sayisi = 0.0;
		SqlCommand sqlCommand2 = new SqlCommand("SELECT ISNULL (COUNT(*),0) FROM STOK_REYONLARI WHERE ryn_kod in ( SELECT sto_reyon_kodu FROM STOKLAR WHERE sto_kod in (  SELECT sth_stok_kod FROM STOK_HAREKETLERI WHERE sth_cari_kodu=@car_kod GROUP BY sth_stok_kod) GROUP BY sto_reyon_kodu)", sqlDB.Connection);
		sqlCommand2.Parameters.AddWithValue("@car_kod", _cari.cari_kod);
		obj = sqlCommand2.ExecuteScalar();
		_stok_reyon_sayisi = double.Parse(obj.ToString());
		sqlCommand2.Dispose();
		_mevcut_kupon_sayisi = 0.0;
		SqlCommand sqlCommand3 = new SqlCommand("SELECT ISNULL (COUNT(*),0) FROM _FORA_KUPON_KODLARI WHERE cari_kod=@car_kod", sqlDB.Connection);
		sqlCommand3.Parameters.AddWithValue("@car_kod", _cari.cari_kod);
		obj = sqlCommand3.ExecuteScalar();
		_mevcut_kupon_sayisi = double.Parse(obj.ToString());
		sqlCommand3.Dispose();
		_acik_bakiye = _ciro - _mevcut_kupon_sayisi * _HerKacLiralikAlisverise;
		_alabilecegi_kupon_sayisi = Math.Floor(_acik_bakiye / _HerKacLiralikAlisverise);
		if (_stok_reyon_sayisi < (double)_Minimum_Stok_Reyon_Kodu_Sayisi)
		{
			_alabilecegi_kupon_sayisi = 0.0;
		}
		sqlDB.ConnectionClose();
	}

	private void sb_kupon_kodu_olustur_Click(object sender, EventArgs e)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		int num = 0;
		try
		{
			SqlCommand sqlCommand = new SqlCommand("SELECT TOP 1 kupon_kodu FROM _FORA_KUPON_KODLARI ORDER BY kupon_kodu DESC", sqlDB.Connection);
			sqlCommand.Parameters.AddWithValue("@car_kod", _cari.cari_kod);
			num = int.Parse(sqlCommand.ExecuteScalar().ToString());
			sqlCommand.Dispose();
		}
		catch
		{
			num = _ilk_kupon_kodu;
		}
		List<SqlCommand> list = new List<SqlCommand>();
		for (int i = 1; (double)i <= _alabilecegi_kupon_sayisi; i++)
		{
			SqlCommand sqlCommand2 = new SqlCommand();
			sqlCommand2.CommandText = "INSERT INTO _FORA_KUPON_KODLARI  (cari_kod, kupon_kodu, tarih) VALUES  (@cari_kod, @kupon_kodu, @tarih)";
			sqlCommand2.Parameters.AddWithValue("@cari_kod", _cari.cari_kod);
			sqlCommand2.Parameters.AddWithValue("@kupon_kodu", num + 1);
			sqlCommand2.Parameters.AddWithValue("@tarih", DateTime.Now);
			list.Add(sqlCommand2);
			num++;
		}
		SqlTransaction sqlTransaction = sqlDB.Connection.BeginTransaction();
		try
		{
			foreach (SqlCommand item in list)
			{
				item.Connection = sqlDB.Connection;
				item.Transaction = sqlTransaction;
				item.ExecuteNonQuery();
			}
			sqlTransaction.Commit();
		}
		catch (Exception ex)
		{
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			sqlTransaction.Rollback();
		}
		finally
		{
			foreach (SqlCommand item2 in list)
			{
				item2.Dispose();
			}
		}
		sqlDB.ConnectionClose();
		Hesaplama_Yap();
		EkranGuncelle();
	}

	private void sb_yazdir_Click(object sender, EventArgs e)
	{
		CekilisListe cekilisListe = new CekilisListe();
		cekilisListe.cari = _cari;
		cekilisListe.cekilis_numaralari = new List<GenelList>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		SqlCommand sqlCommand = new SqlCommand("SELECT kupon_kodu FROM _FORA_KUPON_KODLARI WHERE cari_kod=@cari_kodu", sqlDB.Connection);
		sqlCommand.Parameters.AddWithValue("@cari_kodu", _cari.cari_kod);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		while (sqlDataReader.Read())
		{
			GenelList genelList = new GenelList();
			genelList.Kod = sqlDataReader.GetSafeInt32(0).ToString();
			cekilisListe.cekilis_numaralari.Add(genelList);
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlCommand.Dispose();
		sqlCommand = null;
		sqlDB.ConnectionClose();
		DxRaporCekilisHakki dxRaporCekilisHakki = new DxRaporCekilisHakki();
		List<CekilisListe> list = new List<CekilisListe>();
		list.Add(cekilisListe);
		dxRaporCekilisHakki.DataSource = list;
		try
		{
			dxRaporCekilisHakki.ExportOptions.Pdf.Compressed = true;
			dxRaporCekilisHakki.ExportOptions.Pdf.ConvertImagesToJpeg = true;
			dxRaporCekilisHakki.ExportOptions.Pdf.ImageQuality = PdfJpegImageQuality.High;
			new ReportPrintTool(dxRaporCekilisHakki).Print();
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
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.sb_kupon_kodu_olustur = new DevExpress.XtraEditors.SimpleButton();
		this.sb_yazdir = new DevExpress.XtraEditors.SimpleButton();
		this.label_cari_kod = new DevExpress.XtraEditors.LabelControl();
		this.label_cari_unvan1 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.label_cari_unvan2 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.label_ciro = new DevExpress.XtraEditors.LabelControl();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.label_stok_reyon_sayisi = new DevExpress.XtraEditors.LabelControl();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.label_mevcut_kupon_sayisi = new DevExpress.XtraEditors.LabelControl();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		this.label_acik_bakiye = new DevExpress.XtraEditors.LabelControl();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		this.label_alabilecegi_kupon_sayisi = new DevExpress.XtraEditors.LabelControl();
		this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
		base.SuspendLayout();
		this.labelControl4.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.Location = new System.Drawing.Point(12, 12);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(130, 33);
		this.labelControl4.TabIndex = 7;
		this.labelControl4.Text = "Cari kodu :";
		this.sb_kupon_kodu_olustur.Appearance.Font = new System.Drawing.Font("Tahoma", 30f, System.Drawing.FontStyle.Bold);
		this.sb_kupon_kodu_olustur.Appearance.Options.UseFont = true;
		this.sb_kupon_kodu_olustur.Location = new System.Drawing.Point(12, 533);
		this.sb_kupon_kodu_olustur.Name = "sb_kupon_kodu_olustur";
		this.sb_kupon_kodu_olustur.Size = new System.Drawing.Size(434, 65);
		this.sb_kupon_kodu_olustur.TabIndex = 10;
		this.sb_kupon_kodu_olustur.Text = "Kupon kodu oluştur";
		this.sb_kupon_kodu_olustur.Click += new System.EventHandler(sb_kupon_kodu_olustur_Click);
		this.sb_yazdir.Appearance.Font = new System.Drawing.Font("Tahoma", 30f, System.Drawing.FontStyle.Bold);
		this.sb_yazdir.Appearance.Options.UseFont = true;
		this.sb_yazdir.Location = new System.Drawing.Point(502, 533);
		this.sb_yazdir.Name = "sb_yazdir";
		this.sb_yazdir.Size = new System.Drawing.Size(434, 65);
		this.sb_yazdir.TabIndex = 11;
		this.sb_yazdir.Text = "Yazdır";
		this.sb_yazdir.Click += new System.EventHandler(sb_yazdir_Click);
		this.label_cari_kod.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.label_cari_kod.Location = new System.Drawing.Point(148, 12);
		this.label_cari_kod.Name = "label_cari_kod";
		this.label_cari_kod.Size = new System.Drawing.Size(100, 33);
		this.label_cari_kod.TabIndex = 12;
		this.label_cari_kod.Text = "cari_kod";
		this.label_cari_unvan1.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.label_cari_unvan1.Location = new System.Drawing.Point(185, 51);
		this.label_cari_unvan1.Name = "label_cari_unvan1";
		this.label_cari_unvan1.Size = new System.Drawing.Size(144, 33);
		this.label_cari_unvan1.TabIndex = 14;
		this.label_cari_unvan1.Text = "cari_unvan1";
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.Location = new System.Drawing.Point(12, 51);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(167, 33);
		this.labelControl2.TabIndex = 13;
		this.labelControl2.Text = "Cari ünvan 1 :";
		this.label_cari_unvan2.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.label_cari_unvan2.Location = new System.Drawing.Point(185, 90);
		this.label_cari_unvan2.Name = "label_cari_unvan2";
		this.label_cari_unvan2.Size = new System.Drawing.Size(144, 33);
		this.label_cari_unvan2.TabIndex = 16;
		this.label_cari_unvan2.Text = "cari_unvan2";
		this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.Location = new System.Drawing.Point(12, 90);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(167, 33);
		this.labelControl5.TabIndex = 15;
		this.labelControl5.Text = "Cari ünvan 2 :";
		this.label_ciro.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.label_ciro.Location = new System.Drawing.Point(445, 157);
		this.label_ciro.Name = "label_ciro";
		this.label_ciro.Size = new System.Drawing.Size(43, 33);
		this.label_ciro.TabIndex = 18;
		this.label_ciro.Text = "ciro";
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.Location = new System.Drawing.Point(374, 157);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(65, 33);
		this.labelControl3.TabIndex = 17;
		this.labelControl3.Text = "Ciro :";
		this.label_stok_reyon_sayisi.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.label_stok_reyon_sayisi.Location = new System.Drawing.Point(445, 196);
		this.label_stok_reyon_sayisi.Name = "label_stok_reyon_sayisi";
		this.label_stok_reyon_sayisi.Size = new System.Drawing.Size(145, 33);
		this.label_stok_reyon_sayisi.TabIndex = 20;
		this.label_stok_reyon_sayisi.Text = "reyon_sayisi";
		this.labelControl6.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl6.Location = new System.Drawing.Point(208, 196);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(231, 33);
		this.labelControl6.TabIndex = 19;
		this.labelControl6.Text = "Stok reyonu sayısı :";
		this.label_mevcut_kupon_sayisi.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.label_mevcut_kupon_sayisi.Location = new System.Drawing.Point(447, 235);
		this.label_mevcut_kupon_sayisi.Name = "label_mevcut_kupon_sayisi";
		this.label_mevcut_kupon_sayisi.Size = new System.Drawing.Size(238, 33);
		this.label_mevcut_kupon_sayisi.TabIndex = 22;
		this.label_mevcut_kupon_sayisi.Text = "mevcut kupon sayısı";
		this.labelControl7.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl7.Location = new System.Drawing.Point(185, 235);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(254, 33);
		this.labelControl7.TabIndex = 21;
		this.labelControl7.Text = "Mevcut kupon sayısı :";
		this.label_acik_bakiye.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.label_acik_bakiye.Location = new System.Drawing.Point(447, 274);
		this.label_acik_bakiye.Name = "label_acik_bakiye";
		this.label_acik_bakiye.Size = new System.Drawing.Size(128, 33);
		this.label_acik_bakiye.TabIndex = 24;
		this.label_acik_bakiye.Text = "açık bakiye";
		this.labelControl8.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl8.Location = new System.Drawing.Point(291, 274);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(148, 33);
		this.labelControl8.TabIndex = 23;
		this.labelControl8.Text = "Açık bakiye :";
		this.label_alabilecegi_kupon_sayisi.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.label_alabilecegi_kupon_sayisi.Location = new System.Drawing.Point(445, 313);
		this.label_alabilecegi_kupon_sayisi.Name = "label_alabilecegi_kupon_sayisi";
		this.label_alabilecegi_kupon_sayisi.Size = new System.Drawing.Size(274, 33);
		this.label_alabilecegi_kupon_sayisi.TabIndex = 26;
		this.label_alabilecegi_kupon_sayisi.Text = "alabilecegi kupon sayısı";
		this.labelControl9.Appearance.Font = new System.Drawing.Font("Tahoma", 20f);
		this.labelControl9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl9.Location = new System.Drawing.Point(145, 313);
		this.labelControl9.Name = "labelControl9";
		this.labelControl9.Size = new System.Drawing.Size(294, 33);
		this.labelControl9.TabIndex = 25;
		this.labelControl9.Text = "Alabileceği kupon sayısı :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(948, 610);
		base.Controls.Add(this.label_alabilecegi_kupon_sayisi);
		base.Controls.Add(this.labelControl9);
		base.Controls.Add(this.label_acik_bakiye);
		base.Controls.Add(this.labelControl8);
		base.Controls.Add(this.label_mevcut_kupon_sayisi);
		base.Controls.Add(this.labelControl7);
		base.Controls.Add(this.label_stok_reyon_sayisi);
		base.Controls.Add(this.labelControl6);
		base.Controls.Add(this.label_ciro);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.label_cari_unvan2);
		base.Controls.Add(this.labelControl5);
		base.Controls.Add(this.label_cari_unvan1);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.label_cari_kod);
		base.Controls.Add(this.sb_yazdir);
		base.Controls.Add(this.sb_kupon_kodu_olustur);
		base.Controls.Add(this.labelControl4);
		base.Name = "CekilisKuponVerme";
		this.Text = "Kupon kodu verme";
		base.Load += new System.EventHandler(Giris_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
