using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Text;
using Fora.Mikro.Database.Sqlite;
using Fora.Mikro.Enumler;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;
using Newtonsoft.Json;

namespace Fora.Mikro.Senkronizasyon;

public class ServisKontrolu
{
	public enum_servis_kontrol_sonuc Sonuc;

	public int ServisVersiyonu;

	public string ServisAdresi;

	public ServisKontrolu(int MinimumVersiyon)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Invalid comparison between Unknown and I4
		ServisVersiyonu = 1;
		ServisAdresi = "";
		Encoding uTF = Encoding.UTF8;
		HttpWebResponse webResponseGet = HttpWebUtility.GetWebResponseGet("http://www.forayazilim.com/networktest.html", KeepAlive: false, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8", 5000, 5000);
		if (webResponseGet == null)
		{
			Sonuc = enum_servis_kontrol_sonuc.AgBaglantisiYok;
			return;
		}
		if ((int)webResponseGet.StatusCode != 200)
		{
			Sonuc = enum_servis_kontrol_sonuc.AgBaglantisiYok;
			return;
		}
		ServisAdresi = GetKullanicilacakServisIP(AppBase._FirmaID, AppBase.GetFirmalarDbPath());
		if (ServisAdresi == "")
		{
			Sonuc = enum_servis_kontrol_sonuc.ServiseErisilemedi;
			return;
		}
		string url = "http://" + ServisAdresi + "/GetVersion";
		try
		{
			HttpWebResponse webResponseGet2 = HttpWebUtility.GetWebResponseGet(url, KeepAlive: true, "gzip, deflate", 10000, 10000);
			if (webResponseGet2 != null && (int)webResponseGet2.StatusCode == 200)
			{
				uTF = Encoding.UTF8;
				StreamReader streamReader = new StreamReader(((WebResponse)webResponseGet2).GetResponseStream(), uTF);
				string s = streamReader.ReadToEnd();
				streamReader.Dispose();
				((WebResponse)webResponseGet2).Dispose();
				ServisVersiyonu = int.Parse(s);
			}
		}
		catch
		{
		}
		if (ServisVersiyonu < MinimumVersiyon)
		{
			Sonuc = enum_servis_kontrol_sonuc.ServisVersiyonuEski;
		}
		else
		{
			Sonuc = enum_servis_kontrol_sonuc.ServisCalisiyor;
		}
	}

	private string GetKullanicilacakServisIP(string FirmaID, string FirmalarDbPath)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Invalid comparison between Unknown and I4
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Invalid comparison between Unknown and I4
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Invalid comparison between Unknown and I4
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Invalid comparison between Unknown and I4
		string result = "";
		Encoding uTF = Encoding.UTF8;
		string text = "";
		string text2 = "";
		SqliteHelper sqliteHelper = new SqliteHelper();
		sqliteHelper.Open(FirmalarDbPath, ReadOnly: true);
		SqliteCommand val = new SqliteCommand();
		((DbCommand)val).CommandText = "SELECT FirmaID,ServiceIP,ServiceLocalIP,MikroDBName FROM Firmalar WHERE FirmaID='" + FirmaID + "'";
		val.Connection = sqliteHelper.GetConnection();
		SqliteDataReader val2 = val.ExecuteReader();
		try
		{
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				((DbDataReader)val2)[1].ToString();
				text = ((DbDataReader)val2).GetString(1);
				text2 = ((DbDataReader)val2).GetString(2);
			}
		}
		catch
		{
		}
		((DbDataReader)val2).Close();
		((DbDataReader)val2).Dispose();
		val2 = null;
		((Component)val).Dispose();
		val = null;
		sqliteHelper.Dispose();
		sqliteHelper = null;
		bool flag = false;
		HttpWebResponse webResponseGet = HttpWebUtility.GetWebResponseGet("http://www.forayazilim.com/networktest.html", KeepAlive: false, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8", 5000, 5000);
		if (webResponseGet != null && (int)webResponseGet.StatusCode == 200)
		{
			string url = "http://" + text + "/IsAlive";
			try
			{
				HttpWebResponse webResponseGet2 = HttpWebUtility.GetWebResponseGet(url, KeepAlive: false, "gzip, deflate", 5000, 5000);
				if (webResponseGet2 != null && (int)webResponseGet2.StatusCode == 200)
				{
					StreamReader streamReader = new StreamReader(((WebResponse)webResponseGet2).GetResponseStream(), uTF);
					string text3 = streamReader.ReadToEnd();
					streamReader.Dispose();
					((WebResponse)webResponseGet2).Dispose();
					if (!text3.StartsWith("Hata"))
					{
						flag = JsonConvert.DeserializeObject<bool>(text3);
					}
				}
			}
			catch
			{
			}
		}
		bool flag2 = false;
		if (!flag)
		{
			HttpWebResponse webResponseGet3 = HttpWebUtility.GetWebResponseGet("http://www.forayazilim.com/networktest.html", KeepAlive: false, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8", 5000, 5000);
			if (webResponseGet3 != null && (int)webResponseGet3.StatusCode == 200)
			{
				string url = "http://" + text2 + "/IsAlive";
				try
				{
					HttpWebResponse webResponseGet2 = HttpWebUtility.GetWebResponseGet(url, KeepAlive: false, "gzip, deflate", 5000, 5000);
					if (webResponseGet2 != null && (int)webResponseGet2.StatusCode == 200)
					{
						StreamReader streamReader2 = new StreamReader(((WebResponse)webResponseGet2).GetResponseStream(), uTF);
						string text3 = streamReader2.ReadToEnd();
						streamReader2.Dispose();
						((WebResponse)webResponseGet2).Dispose();
						if (!text3.StartsWith("Hata"))
						{
							flag2 = JsonConvert.DeserializeObject<bool>(text3);
						}
					}
				}
				catch
				{
				}
			}
		}
		if (flag)
		{
			result = text;
		}
		else if (flag2)
		{
			result = text2;
		}
		return result;
	}
}
