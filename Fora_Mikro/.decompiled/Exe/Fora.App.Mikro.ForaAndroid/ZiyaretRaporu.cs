using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using Fora.App.Win.Mikro.ForaAndroid;
using Fora.App.Win.Mikro.Properties;
using Fora.Mikro;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace Fora.App.Mikro.ForaAndroid;

public class ZiyaretRaporu : Form
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _KullaniciParametreleri;

	private List<string> secili_temsilciler;

	private List<List<CariRotaListItem>> liste;

	private List<List<Ziyaretler>> ziyaretliste;

	private GMapOverlay marker_overlay;

	private GMapOverlay routes_Overlay;

	private List<GMapMarker> markers = new List<GMapMarker>();

	private List<GMapRoute> routes;

	private List<GMapRoute> routes_ziyaretler;

	private IContainer components;

	private GMapControl MapControl;

	private BindingSource bindingSource1;

	private DateTimePicker dtp_tarih;

	private Label label1;

	private Label label2;

	private TabControl tabControl1;

	private TabPage tabPage1;

	private System.Windows.Forms.ComboBox cb_map_providers;

	private Label label3;

	private PictureBox pictureBox1;

	private Label label5;

	private PictureBox pictureBox3;

	private Label label4;

	private PictureBox pictureBox2;

	private Label label7;

	private PictureBox pictureBox5;

	private Label label6;

	private PictureBox pictureBox4;

	private CheckedComboBoxEdit ccb_temsilciler;

	private CheckedComboBoxEdit ccb_bolgeler;

	private Label label8;

	private CheckBox cb_rut_cizgisi_goster;

	private CheckBox cb_ziyaret_cizgisi_goster;

	private CheckBox cb_butun_carilerini_goster;

	public ZiyaretRaporu(MikroUygulamaBilgileri mikrouygulamabilgileri, Parametreler KullaniciParametreleri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_KullaniciParametreleri = KullaniciParametreleri;
		InitializeComponent();
		if (_KullaniciParametreleri._GetParametre("HakListelemeCariBolgeTumBolgeler")._GetBoolean)
		{
			string commandText = "SELECT ParametreDegeri FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram='akilli' AND ParametreAdi='CariBolgeKodu' GROUP BY ParametreDegeri ORDER BY ParametreDegeri";
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
						ccb_bolgeler.Properties.Items.Add(sqlDataReader.GetSafeString(0), CheckState.Checked, enabled: true);
					}
					sqlDataReader.Close();
				}
				sqlDB.ConnectionClose();
			}
			catch
			{
			}
		}
		else
		{
			ccb_bolgeler.Properties.Items.Add(_KullaniciParametreleri._GetParametre("CariBolgeKodu")._GetString, CheckState.Checked, enabled: true);
		}
		TemsilcilerGuncelle();
		dtp_tarih.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		dtp_tarih.ValueChanged += Dtp_tarih_ValueChanged;
		marker_overlay = new GMapOverlay("CARİLER");
		routes_Overlay = new GMapOverlay("routes");
		MapControl.Overlays.Add(marker_overlay);
		MapControl.Overlays.Add(routes_Overlay);
		markers = new List<GMapMarker>();
		MapControl.MapProvider = GoogleHybridMapProvider.Instance;
		DataTable dataSource = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(string)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { "GoogleHybridMapProvider", "Google Hybrid Map" },
				new object[2] { "GoogleMapProvider", "Google Map" },
				new object[2] { "GoogleSatelliteMapProvider", "Google Satellite Map" },
				new object[2] { "BingHybridMapProvider", "Bing Hybrid Map" },
				new object[2] { "BingMapProvider", "Bing Map" },
				new object[2] { "BingSatelliteMapProvider", "Bing Satellite Map" },
				new object[2] { "YandexHybridMapProvider", "Yandex Hybrid Map" },
				new object[2] { "YandexMapProvider", "Yandex Map" },
				new object[2] { "YandexSatelliteMapProvider", "Yandex Satellite Map" }
			}
		};
		cb_map_providers.DataSource = dataSource;
		cb_map_providers.ValueMember = "ID";
		cb_map_providers.DisplayMember = "Isim";
		cb_map_providers.SelectedIndexChanged += Cb_map_providers_SelectedIndexChanged;
		MapControl.Manager.Mode = AccessMode.ServerAndCache;
		MapControl.Position = new PointLatLng(39.406633, 35.032354);
		MapControl.MinZoom = 3;
		MapControl.MaxZoom = 16;
		MapControl.Zoom = 8.0;
	}

	private void TemsilcilerGuncelle()
	{
		ccb_temsilciler.Properties.Items.Clear();
		if (_KullaniciParametreleri._GetParametre("GorebilecegiTemsilciler")._GetString == "")
		{
			string commandText = "SELECT ParametreUser,ISNULL((SELECT TOP 1 ParametreDegeri FROM _FORA_PARAMETRELER AS IC WITH (NOLOCK) WHERE ParametreProgram='akilli' AND ParametreAdi='CariBolgeKodu' AND IC.ParametreUser=DIS.ParametreUser),'TANIMSIZ') AS BOLGE FROM _FORA_PARAMETRELER AS DIS WITH (NOLOCK) WHERE ParametreProgram='akilli' GROUP BY ParametreUser ORDER BY ParametreUser";
			try
			{
				List<string> list = new List<string>();
				string[] array = ((string)ccb_bolgeler.EditValue).Split(',');
				foreach (string text in array)
				{
					if (text != "")
					{
						list.Add(text.Trim());
					}
				}
				SqlDB sqlDB = new SqlDB();
				sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
				using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
				{
					sqlCommand.CommandText = commandText;
					SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
					while (sqlDataReader.Read())
					{
						string safeString = sqlDataReader.GetSafeString(0);
						string safeString2 = sqlDataReader.GetSafeString(1);
						foreach (string item in list)
						{
							if (item.ToString() == safeString2)
							{
								Parametreler parametreler = ParametrelerDefault.MobilKullanici(safeString);
								ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "akilli", safeString, "", "");
								ccb_temsilciler.Properties.Items.Add(parametreler._GetParametre("CariPersonelKodu")._GetString, CheckState.Unchecked, enabled: true);
							}
						}
					}
					sqlDataReader.Close();
				}
				sqlDB.ConnectionClose();
			}
			catch
			{
			}
			ccb_temsilciler.Properties.PopupFormMinSize = new Size(200, 100);
			ccb_temsilciler.Properties.PopupFormSize = new Size(200, 200);
		}
		else
		{
			string[] array = _KullaniciParametreleri._GetParametre("GorebilecegiTemsilciler")._GetString.Split(',');
			foreach (string value in array)
			{
				ccb_temsilciler.Properties.Items.Add(value, CheckState.Unchecked, enabled: true);
			}
		}
	}

	private void Cb_map_providers_SelectedIndexChanged(object sender, EventArgs e)
	{
		switch (cb_map_providers.SelectedValue.ToString())
		{
		case "GoogleHybridMapProvider":
			MapControl.MapProvider = GoogleHybridMapProvider.Instance;
			break;
		case "GoogleMapProvider":
			MapControl.MapProvider = GoogleMapProvider.Instance;
			break;
		case "GoogleSatelliteMapProvider":
			MapControl.MapProvider = GoogleSatelliteMapProvider.Instance;
			break;
		case "BingHybridMapProvider":
			MapControl.MapProvider = BingHybridMapProvider.Instance;
			break;
		case "BingMapProvider":
			MapControl.MapProvider = BingMapProvider.Instance;
			break;
		case "BingSatelliteMapProvider":
			MapControl.MapProvider = BingSatelliteMapProvider.Instance;
			break;
		case "YandexHybridMapProvider":
			MapControl.MapProvider = YandexHybridMapProvider.Instance;
			break;
		case "YandexMapProvider":
			MapControl.MapProvider = YandexMapProvider.Instance;
			break;
		case "YandexSatelliteMapProvider":
			MapControl.MapProvider = YandexSatelliteMapProvider.Instance;
			break;
		}
	}

	private void Dtp_tarih_ValueChanged(object sender, EventArgs e)
	{
		BilgiGuncelle();
	}

	private void BilgiGuncelle()
	{
		Cursor.Current = Cursors.WaitCursor;
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		secili_temsilciler = new List<string>();
		string[] array = ((string)ccb_temsilciler.EditValue).Split(',');
		foreach (string text in array)
		{
			if (text != "")
			{
				secili_temsilciler.Add(text.Trim());
			}
		}
		liste = new List<List<CariRotaListItem>>();
		ziyaretliste = new List<List<Ziyaretler>>();
		for (int j = 0; j < secili_temsilciler.Count; j++)
		{
			liste.Add(new List<CariRotaListItem>());
			ziyaretliste.Add(new List<Ziyaretler>());
		}
		int num = 0;
		foreach (string item in secili_temsilciler)
		{
			ziyaretliste[num] = new List<Ziyaretler>();
			liste[num] = new List<CariRotaListItem>();
			if (!cb_butun_carilerini_goster.Checked)
			{
				liste[num].AddRange(TemsilciData.GetCariRotaListItems(sqlDB.Connection, item, dtp_tarih.Value));
			}
			else
			{
				for (int k = 0; k < 7; k++)
				{
					liste[num].AddRange(TemsilciData.GetCariRotaListItems(sqlDB.Connection, item, dtp_tarih.Value.AddDays(k)));
				}
			}
			foreach (CariRotaListItem item2 in liste[num])
			{
				Ziyaretler ziyaretler = new Ziyaretler();
				ziyaretler.cari_kodu = item2.cari_kod;
				ziyaretler.cari_ismi = item2.cari_unvan1 + " " + item2.cari_unvan2;
				ziyaretler.adres = item2.CariAdres.GosterAdres;
				ziyaretler.adresno = item2.CariAdres.adr_adres_no;
				ziyaretler.cari_adres_enlem = item2.CariAdres.adr_gps_enlem;
				ziyaretler.cari_adres_boylam = item2.CariAdres.adr_gps_boylam;
				ziyaretler.baslama_zamani = default(TimeSpan);
				ziyaretler.bitis_zamani = default(TimeSpan);
				ziyaretler.rotada_var = true;
				ziyaretler.ziyaret_edildi = false;
				ziyaretliste[num].Add(ziyaretler);
			}
			string commandText = "SELECT zyrt_Cari_Kodu,cari_unvan1,zyrt_Cari_Adres_No,adr_cadde + ' ' + adr_sokak + ' ' + adr_posta_kodu + ' ' + adr_ilce + ' ' + adr_il + ' ' + adr_ulke AS Adres,zyrt_Baslama_Saati,zyrt_Bitis_Saati,zyrt_Baslama_Enlem,zyrt_Baslama_Boylam,zyrt_Bitis_Enlem,zyrt_Bitis_Boylam,adr_gps_enlem,adr_gps_boylam FROM _ZIYARET_HAREKETLERI  LEFT JOIN CARI_HESAPLAR ON zyrt_Cari_Kodu=cari_kod LEFT JOIN CARI_HESAP_ADRESLERI ON zyrt_Cari_Adres_No=adr_adres_no AND zyrt_Cari_Kodu=adr_cari_kod WHERE zyrt_Temsilci_Kodu=@zyrt_Temsilci_Kodu AND zyrt_Tarihi=@zyrt_Tarihi";
			try
			{
				using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Kodu", item);
				sqlCommand.Parameters.AddWithValue("@zyrt_Tarihi", dtp_tarih.Value);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					string safeString = sqlDataReader.GetSafeString(0);
					string safeString2 = sqlDataReader.GetSafeString(1);
					int safeInt = sqlDataReader.GetSafeInt32(2);
					string safeString3 = sqlDataReader.GetSafeString(3);
					double safeDouble = sqlDataReader.GetSafeDouble(10);
					double safeDouble2 = sqlDataReader.GetSafeDouble(11);
					double safeDouble3 = sqlDataReader.GetSafeDouble(6);
					double safeDouble4 = sqlDataReader.GetSafeDouble(7);
					double safeDouble5 = sqlDataReader.GetSafeDouble(8);
					double safeDouble6 = sqlDataReader.GetSafeDouble(9);
					bool flag = false;
					foreach (Ziyaretler item3 in ziyaretliste[num])
					{
						if (item3.cari_kodu == safeString && item3.adres == safeString3)
						{
							flag = true;
							item3.baslama_zamani = new TimeSpan(sqlDataReader.GetSafeDateTime(4).Hour, sqlDataReader.GetSafeDateTime(4).Minute, sqlDataReader.GetSafeDateTime(4).Second);
							item3.bitis_zamani = new TimeSpan(sqlDataReader.GetSafeDateTime(5).Hour, sqlDataReader.GetSafeDateTime(5).Minute, sqlDataReader.GetSafeDateTime(5).Second);
							item3.ziyaret_edildi = true;
							item3.cari_adres_enlem = safeDouble;
							item3.cari_adres_boylam = safeDouble2;
							item3.ziyaret_baslama_enlem = safeDouble3;
							item3.ziyaret_baslama_boylam = safeDouble4;
							item3.ziyaret_bitis_enlem = safeDouble5;
							item3.ziyaret_bitis_boylam = safeDouble6;
							flag = true;
						}
					}
					if (!flag)
					{
						Ziyaretler ziyaretler2 = new Ziyaretler();
						ziyaretler2.cari_kodu = safeString;
						ziyaretler2.cari_ismi = safeString2;
						ziyaretler2.adres = safeString3;
						ziyaretler2.adresno = safeInt;
						ziyaretler2.cari_adres_enlem = safeDouble;
						ziyaretler2.cari_adres_boylam = safeDouble2;
						ziyaretler2.ziyaret_baslama_enlem = safeDouble3;
						ziyaretler2.ziyaret_baslama_boylam = safeDouble4;
						ziyaretler2.ziyaret_bitis_enlem = safeDouble5;
						ziyaretler2.ziyaret_bitis_boylam = safeDouble6;
						ziyaretler2.baslama_zamani = new TimeSpan(sqlDataReader.GetSafeDateTime(4).Hour, sqlDataReader.GetSafeDateTime(4).Minute, sqlDataReader.GetSafeDateTime(4).Second);
						ziyaretler2.bitis_zamani = new TimeSpan(sqlDataReader.GetSafeDateTime(5).Hour, sqlDataReader.GetSafeDateTime(5).Minute, sqlDataReader.GetSafeDateTime(5).Second);
						ziyaretler2.ziyaret_edildi = true;
						ziyaretler2.rotada_var = false;
						ziyaretliste[num].Add(ziyaretler2);
					}
				}
				sqlDataReader.Close();
				sqlDataReader.Dispose();
				sqlDataReader = null;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			num++;
		}
		sqlDB.ConnectionClose();
		Cursor.Current = Cursors.Default;
		HaritaGuncelle();
	}

	private void HaritaGuncelle()
	{
		Cursor.Current = Cursors.WaitCursor;
		foreach (GMapMarker marker in markers)
		{
			marker_overlay.Markers.Remove(marker);
		}
		markers = new List<GMapMarker>();
		int num = 0;
		foreach (string item in secili_temsilciler)
		{
			List<PointLatLng> list = new List<PointLatLng>();
			List<PointLatLng> list2 = new List<PointLatLng>();
			int num2 = 0;
			num2 = 1;
			foreach (Ziyaretler item2 in ziyaretliste[num])
			{
				if (item2.cari_adres_enlem == 0.0)
				{
					continue;
				}
				PointLatLng p = new PointLatLng(item2.cari_adres_enlem, item2.cari_adres_boylam);
				GMapMarker gMapMarker = ((!item2.ziyaret_edildi) ? new GMarkerGoogle(p, Resources.cari_ziyaret_edilmemis) : ((!item2.rotada_var) ? new GMarkerGoogle(p, Resources.cari_rota_disi_ziyaret) : new GMarkerGoogle(p, Resources.cari_ziyaret_edilmis)));
				double num3 = 100000.0;
				if (item2.ziyaret_baslama_enlem != 0.0)
				{
					try
					{
						num3 = new Coordinates(item2.cari_adres_enlem, item2.cari_adres_boylam).DistanceTo(new Coordinates(item2.ziyaret_baslama_enlem, item2.ziyaret_baslama_boylam), UnitOfLength.Kilometers);
					}
					catch
					{
					}
					list2.Add(new PointLatLng(item2.ziyaret_baslama_enlem, item2.ziyaret_baslama_boylam));
				}
				double num4 = 100000.0;
				if (item2.ziyaret_bitis_enlem != 0.0)
				{
					try
					{
						num4 = new Coordinates(item2.cari_adres_enlem, item2.cari_adres_boylam).DistanceTo(new Coordinates(item2.ziyaret_bitis_enlem, item2.ziyaret_bitis_boylam), UnitOfLength.Kilometers);
					}
					catch
					{
					}
					list2.Add(new PointLatLng(item2.ziyaret_bitis_enlem, item2.ziyaret_bitis_boylam));
				}
				gMapMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
				gMapMarker.ToolTipText = item + " (" + num2 + ")\n";
				gMapMarker.ToolTipText = gMapMarker.ToolTipText + item2.cari_ismi + "\n";
				gMapMarker.ToolTipText = gMapMarker.ToolTipText + item2.adres + "\n";
				GMapMarker gMapMarker2 = gMapMarker;
				gMapMarker2.ToolTipText = gMapMarker2.ToolTipText + "Ziyaret başlama : " + item2.baslama_zamani.ToString() + " uzaklık : " + (num3 * 1000.0).ToString("N0") + " metre\n";
				gMapMarker2 = gMapMarker;
				gMapMarker2.ToolTipText = gMapMarker2.ToolTipText + "Ziyaret bitirme : " + item2.bitis_zamani.ToString() + " uzaklık : " + (num4 * 1000.0).ToString("N0") + " metre\n";
				markers.Add(gMapMarker);
				if (item2.rotada_var)
				{
					list.Add(new PointLatLng(item2.cari_adres_enlem, item2.cari_adres_boylam));
				}
				num2++;
			}
			foreach (Ziyaretler item3 in ziyaretliste[num])
			{
				if (item3.ziyaret_baslama_enlem != 0.0)
				{
					GMapMarker gMapMarker3 = new GMarkerGoogle(new PointLatLng(item3.ziyaret_baslama_enlem, item3.ziyaret_baslama_boylam), Resources.ziyaret_baslangic);
					gMapMarker3.ToolTipMode = MarkerTooltipMode.Never;
					markers.Add(gMapMarker3);
				}
				if (item3.ziyaret_bitis_enlem != 0.0)
				{
					GMapMarker gMapMarker4 = new GMarkerGoogle(new PointLatLng(item3.ziyaret_bitis_enlem, item3.ziyaret_bitis_boylam), Resources.ziyaret_bitis);
					gMapMarker4.ToolTipMode = MarkerTooltipMode.Never;
					markers.Add(gMapMarker4);
				}
			}
			if (routes != null)
			{
				foreach (GMapRoute route in routes)
				{
					routes_Overlay.Routes.Remove(route);
				}
			}
			if (routes_ziyaretler != null)
			{
				foreach (GMapRoute item4 in routes_ziyaretler)
				{
					routes_Overlay.Routes.Remove(item4);
				}
			}
			routes = new List<GMapRoute>();
			routes_ziyaretler = new List<GMapRoute>();
			GMapRoute gMapRoute = new GMapRoute(list, "RUT");
			gMapRoute.Stroke = new Pen(Color.Blue, 3f);
			routes.Add(gMapRoute);
			GMapRoute gMapRoute2 = new GMapRoute(list2, "ZIYARETLER");
			gMapRoute2.Stroke = new Pen(Color.Green, 4f);
			routes_ziyaretler.Add(gMapRoute2);
			num++;
		}
		foreach (GMapMarker marker2 in markers)
		{
			marker_overlay.Markers.Add(marker2);
		}
		routes_Overlay = new GMapOverlay("routes");
		if (cb_rut_cizgisi_goster.Checked && routes != null)
		{
			foreach (GMapRoute route2 in routes)
			{
				routes_Overlay.Routes.Add(route2);
			}
		}
		if (cb_ziyaret_cizgisi_goster.Checked && routes_ziyaretler != null)
		{
			foreach (GMapRoute item5 in routes_ziyaretler)
			{
				routes_Overlay.Routes.Add(item5);
			}
		}
		MapControl.Overlays.Add(routes_Overlay);
		MapControl.ZoomAndCenterMarkers("CARİLER");
		Cursor.Current = Cursors.Default;
	}

	private void MapControl_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		MapControl.FromLocalToLatLng(e.X, e.Y);
	}

	private void ccb_temsilciler_EditValueChanged(object sender, EventArgs e)
	{
		BilgiGuncelle();
	}

	private void ccb_bolgeler_EditValueChanged(object sender, EventArgs e)
	{
		TemsilcilerGuncelle();
		BilgiGuncelle();
	}

	private void cb_rut_cizgisi_goster_CheckedChanged(object sender, EventArgs e)
	{
		HaritaGuncelle();
	}

	private void cb_ziyaret_cizgisi_goster_CheckedChanged(object sender, EventArgs e)
	{
		HaritaGuncelle();
	}

	private void cb_butun_carilerini_goster_CheckedChanged(object sender, EventArgs e)
	{
		BilgiGuncelle();
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
		this.components = new System.ComponentModel.Container();
		this.MapControl = new GMap.NET.WindowsForms.GMapControl();
		this.dtp_tarih = new System.Windows.Forms.DateTimePicker();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.tabControl1 = new System.Windows.Forms.TabControl();
		this.tabPage1 = new System.Windows.Forms.TabPage();
		this.cb_map_providers = new System.Windows.Forms.ComboBox();
		this.label7 = new System.Windows.Forms.Label();
		this.pictureBox5 = new System.Windows.Forms.PictureBox();
		this.label6 = new System.Windows.Forms.Label();
		this.pictureBox4 = new System.Windows.Forms.PictureBox();
		this.label5 = new System.Windows.Forms.Label();
		this.pictureBox3 = new System.Windows.Forms.PictureBox();
		this.label4 = new System.Windows.Forms.Label();
		this.pictureBox2 = new System.Windows.Forms.PictureBox();
		this.label3 = new System.Windows.Forms.Label();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
		this.ccb_temsilciler = new DevExpress.XtraEditors.CheckedComboBoxEdit();
		this.ccb_bolgeler = new DevExpress.XtraEditors.CheckedComboBoxEdit();
		this.label8 = new System.Windows.Forms.Label();
		this.cb_rut_cizgisi_goster = new System.Windows.Forms.CheckBox();
		this.cb_ziyaret_cizgisi_goster = new System.Windows.Forms.CheckBox();
		this.cb_butun_carilerini_goster = new System.Windows.Forms.CheckBox();
		this.tabControl1.SuspendLayout();
		this.tabPage1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.bindingSource1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ccb_temsilciler.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ccb_bolgeler.Properties).BeginInit();
		base.SuspendLayout();
		this.MapControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.MapControl.Bearing = 0f;
		this.MapControl.CanDragMap = true;
		this.MapControl.EmptyTileColor = System.Drawing.Color.Navy;
		this.MapControl.GrayScaleMode = false;
		this.MapControl.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
		this.MapControl.LevelsKeepInMemmory = 5;
		this.MapControl.Location = new System.Drawing.Point(0, 0);
		this.MapControl.MarkersEnabled = true;
		this.MapControl.MaxZoom = 2;
		this.MapControl.MinZoom = 2;
		this.MapControl.MouseWheelZoomEnabled = true;
		this.MapControl.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
		this.MapControl.Name = "MapControl";
		this.MapControl.NegativeMode = false;
		this.MapControl.PolygonsEnabled = true;
		this.MapControl.RetryLoadTile = 0;
		this.MapControl.RoutesEnabled = true;
		this.MapControl.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
		this.MapControl.SelectedAreaFillColor = System.Drawing.Color.FromArgb(33, 65, 105, 225);
		this.MapControl.ShowTileGridLines = false;
		this.MapControl.Size = new System.Drawing.Size(971, 450);
		this.MapControl.TabIndex = 5;
		this.MapControl.Zoom = 0.0;
		this.MapControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MapControl_MouseDoubleClick);
		this.dtp_tarih.Location = new System.Drawing.Point(477, 13);
		this.dtp_tarih.Name = "dtp_tarih";
		this.dtp_tarih.Size = new System.Drawing.Size(107, 20);
		this.dtp_tarih.TabIndex = 190;
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label1.Location = new System.Drawing.Point(186, 15);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(64, 13);
		this.label1.TabIndex = 191;
		this.label1.Text = "Personel :";
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label2.Location = new System.Drawing.Point(427, 15);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(44, 13);
		this.label2.TabIndex = 192;
		this.label2.Text = "Tarih :";
		this.tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tabControl1.Controls.Add(this.tabPage1);
		this.tabControl1.Location = new System.Drawing.Point(2, 39);
		this.tabControl1.Name = "tabControl1";
		this.tabControl1.SelectedIndex = 0;
		this.tabControl1.Size = new System.Drawing.Size(979, 510);
		this.tabControl1.TabIndex = 193;
		this.tabPage1.Controls.Add(this.cb_map_providers);
		this.tabPage1.Controls.Add(this.label7);
		this.tabPage1.Controls.Add(this.pictureBox5);
		this.tabPage1.Controls.Add(this.label6);
		this.tabPage1.Controls.Add(this.pictureBox4);
		this.tabPage1.Controls.Add(this.label5);
		this.tabPage1.Controls.Add(this.pictureBox3);
		this.tabPage1.Controls.Add(this.label4);
		this.tabPage1.Controls.Add(this.pictureBox2);
		this.tabPage1.Controls.Add(this.label3);
		this.tabPage1.Controls.Add(this.pictureBox1);
		this.tabPage1.Controls.Add(this.MapControl);
		this.tabPage1.Location = new System.Drawing.Point(4, 22);
		this.tabPage1.Name = "tabPage1";
		this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage1.Size = new System.Drawing.Size(971, 484);
		this.tabPage1.TabIndex = 0;
		this.tabPage1.Text = "Harita";
		this.tabPage1.UseVisualStyleBackColor = true;
		this.cb_map_providers.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.cb_map_providers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_map_providers.FormattingEnabled = true;
		this.cb_map_providers.Location = new System.Drawing.Point(816, 457);
		this.cb_map_providers.Name = "cb_map_providers";
		this.cb_map_providers.Size = new System.Drawing.Size(149, 21);
		this.cb_map_providers.TabIndex = 194;
		this.label7.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label7.AutoSize = true;
		this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label7.Location = new System.Drawing.Point(681, 461);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(76, 13);
		this.label7.TabIndex = 15;
		this.label7.Text = "Ziyaret bitişi";
		this.pictureBox5.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.pictureBox5.Image = Fora.App.Win.Mikro.Properties.Resources.ziyaret_bitis;
		this.pictureBox5.Location = new System.Drawing.Point(651, 454);
		this.pictureBox5.Name = "pictureBox5";
		this.pictureBox5.Size = new System.Drawing.Size(24, 25);
		this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureBox5.TabIndex = 14;
		this.pictureBox5.TabStop = false;
		this.label6.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label6.AutoSize = true;
		this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label6.Location = new System.Drawing.Point(532, 461);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(107, 13);
		this.label6.TabIndex = 13;
		this.label6.Text = "Ziyaret başlangıcı";
		this.pictureBox4.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.pictureBox4.Image = Fora.App.Win.Mikro.Properties.Resources.ziyaret_baslangic;
		this.pictureBox4.Location = new System.Drawing.Point(502, 454);
		this.pictureBox4.Name = "pictureBox4";
		this.pictureBox4.Size = new System.Drawing.Size(24, 25);
		this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureBox4.TabIndex = 12;
		this.pictureBox4.TabStop = false;
		this.label5.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label5.AutoSize = true;
		this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label5.Location = new System.Drawing.Point(379, 461);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(112, 13);
		this.label5.TabIndex = 11;
		this.label5.Text = "Rota dışı ziyaretler";
		this.pictureBox3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.pictureBox3.Image = Fora.App.Win.Mikro.Properties.Resources.cari_rota_disi_ziyaret;
		this.pictureBox3.Location = new System.Drawing.Point(349, 454);
		this.pictureBox3.Name = "pictureBox3";
		this.pictureBox3.Size = new System.Drawing.Size(24, 25);
		this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureBox3.TabIndex = 10;
		this.pictureBox3.TabStop = false;
		this.label4.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label4.AutoSize = true;
		this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label4.Location = new System.Drawing.Point(198, 461);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(143, 13);
		this.label4.TabIndex = 9;
		this.label4.Text = "Ziyaret edilmemiş cariler";
		this.pictureBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.pictureBox2.Image = Fora.App.Win.Mikro.Properties.Resources.cari_ziyaret_edilmemis;
		this.pictureBox2.Location = new System.Drawing.Point(168, 454);
		this.pictureBox2.Name = "pictureBox2";
		this.pictureBox2.Size = new System.Drawing.Size(24, 25);
		this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureBox2.TabIndex = 8;
		this.pictureBox2.TabStop = false;
		this.label3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label3.AutoSize = true;
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label3.Location = new System.Drawing.Point(36, 461);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(127, 13);
		this.label3.TabIndex = 7;
		this.label3.Text = "Ziyaret edilmiş cariler";
		this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.pictureBox1.Image = Fora.App.Win.Mikro.Properties.Resources.cari_ziyaret_edilmis;
		this.pictureBox1.Location = new System.Drawing.Point(6, 454);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(24, 25);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureBox1.TabIndex = 6;
		this.pictureBox1.TabStop = false;
		this.bindingSource1.DataSource = typeof(Fora.Mikro.CariHesaplar.CariRotaListItem);
		this.ccb_temsilciler.Location = new System.Drawing.Point(256, 12);
		this.ccb_temsilciler.Name = "ccb_temsilciler";
		this.ccb_temsilciler.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.ccb_temsilciler.Properties.DropDownRows = 20;
		this.ccb_temsilciler.Size = new System.Drawing.Size(151, 20);
		this.ccb_temsilciler.TabIndex = 194;
		this.ccb_temsilciler.EditValueChanged += new System.EventHandler(ccb_temsilciler_EditValueChanged);
		this.ccb_bolgeler.Location = new System.Drawing.Point(60, 12);
		this.ccb_bolgeler.Name = "ccb_bolgeler";
		this.ccb_bolgeler.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.ccb_bolgeler.Size = new System.Drawing.Size(109, 20);
		this.ccb_bolgeler.TabIndex = 196;
		this.ccb_bolgeler.EditValueChanged += new System.EventHandler(ccb_bolgeler_EditValueChanged);
		this.label8.AutoSize = true;
		this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label8.Location = new System.Drawing.Point(7, 15);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(47, 13);
		this.label8.TabIndex = 195;
		this.label8.Text = "Bölge :";
		this.cb_rut_cizgisi_goster.AutoSize = true;
		this.cb_rut_cizgisi_goster.Checked = true;
		this.cb_rut_cizgisi_goster.CheckState = System.Windows.Forms.CheckState.Checked;
		this.cb_rut_cizgisi_goster.Location = new System.Drawing.Point(736, 14);
		this.cb_rut_cizgisi_goster.Name = "cb_rut_cizgisi_goster";
		this.cb_rut_cizgisi_goster.Size = new System.Drawing.Size(106, 17);
		this.cb_rut_cizgisi_goster.TabIndex = 197;
		this.cb_rut_cizgisi_goster.Text = "Rut çizgisi göster";
		this.cb_rut_cizgisi_goster.UseVisualStyleBackColor = true;
		this.cb_rut_cizgisi_goster.CheckedChanged += new System.EventHandler(cb_rut_cizgisi_goster_CheckedChanged);
		this.cb_ziyaret_cizgisi_goster.AutoSize = true;
		this.cb_ziyaret_cizgisi_goster.Checked = true;
		this.cb_ziyaret_cizgisi_goster.CheckState = System.Windows.Forms.CheckState.Checked;
		this.cb_ziyaret_cizgisi_goster.Location = new System.Drawing.Point(860, 15);
		this.cb_ziyaret_cizgisi_goster.Name = "cb_ziyaret_cizgisi_goster";
		this.cb_ziyaret_cizgisi_goster.Size = new System.Drawing.Size(121, 17);
		this.cb_ziyaret_cizgisi_goster.TabIndex = 198;
		this.cb_ziyaret_cizgisi_goster.Text = "Ziyaret çizgisi göster";
		this.cb_ziyaret_cizgisi_goster.UseVisualStyleBackColor = true;
		this.cb_ziyaret_cizgisi_goster.CheckedChanged += new System.EventHandler(cb_ziyaret_cizgisi_goster_CheckedChanged);
		this.cb_butun_carilerini_goster.AutoSize = true;
		this.cb_butun_carilerini_goster.Location = new System.Drawing.Point(590, 14);
		this.cb_butun_carilerini_goster.Name = "cb_butun_carilerini_goster";
		this.cb_butun_carilerini_goster.Size = new System.Drawing.Size(127, 17);
		this.cb_butun_carilerini_goster.TabIndex = 199;
		this.cb_butun_carilerini_goster.Text = "Bütün carilerini göster";
		this.cb_butun_carilerini_goster.UseVisualStyleBackColor = true;
		this.cb_butun_carilerini_goster.CheckedChanged += new System.EventHandler(cb_butun_carilerini_goster_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(984, 551);
		base.Controls.Add(this.cb_butun_carilerini_goster);
		base.Controls.Add(this.cb_ziyaret_cizgisi_goster);
		base.Controls.Add(this.cb_rut_cizgisi_goster);
		base.Controls.Add(this.ccb_bolgeler);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.ccb_temsilciler);
		base.Controls.Add(this.tabControl1);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.dtp_tarih);
		this.MinimumSize = new System.Drawing.Size(1000, 590);
		base.Name = "ZiyaretRaporu";
		this.Text = "Temsilci rut düzenleme";
		this.tabControl1.ResumeLayout(false);
		this.tabPage1.ResumeLayout(false);
		this.tabPage1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.bindingSource1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ccb_temsilciler.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ccb_bolgeler.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
