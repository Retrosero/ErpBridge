using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Senkronizasyon;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Siparis;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SiparislerRestClient
{
	public static List<SiparisOnaylamaListItem> GetSiparisOnaylamaListItem(string servis_adresi, string firmaid, string kullanici_adi, string sifre, enum_CariListelemeSecenekleri CariListelemeSecenek, string CariListelemeAktifGrup, string TemsilciKodu)
	{
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Invalid comparison between Unknown and I4
		List<SiparisOnaylamaListItem> list = new List<SiparisOnaylamaListItem>();
		_ = Encoding.UTF8;
		string url;
		if (new ServisKontrolu(1).ServisVersiyonu > 30005)
		{
			string[] obj = new string[16]
			{
				"http://",
				servis_adresi,
				"/GetSiparisOnaylamaListItemsV2/",
				firmaid,
				"/",
				AppBase.ForaPlatformTools.UrlEncode(kullanici_adi),
				"/",
				AppBase.ForaPlatformTools.UrlEncode(sifre),
				"/",
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			int num = (int)CariListelemeSecenek;
			obj[9] = num.ToString();
			obj[10] = "/_";
			obj[11] = CariListelemeAktifGrup;
			obj[12] = "/_";
			obj[13] = TemsilciKodu;
			obj[14] = "/";
			obj[15] = AppBase._MikroDBName;
			url = string.Concat(obj);
		}
		else
		{
			string[] obj2 = new string[14]
			{
				"http://",
				servis_adresi,
				"/GetSiparisOnaylamaListItems/",
				firmaid,
				"/",
				AppBase.ForaPlatformTools.UrlEncode(kullanici_adi),
				"/",
				AppBase.ForaPlatformTools.UrlEncode(sifre),
				"/",
				null,
				null,
				null,
				null,
				null
			};
			int num = (int)CariListelemeSecenek;
			obj2[9] = num.ToString();
			obj2[10] = "/_";
			obj2[11] = CariListelemeAktifGrup;
			obj2[12] = "/_";
			obj2[13] = TemsilciKodu;
			url = string.Concat(obj2);
		}
		HttpWebResponse webResponseGet = HttpWebUtility.GetWebResponseGet(url, KeepAlive: false, "gzip, deflate", 20000, 20000);
		if (webResponseGet != null && (int)webResponseGet.StatusCode == 200)
		{
			Stream stream = new MemoryStream();
			((WebResponse)webResponseGet).GetResponseStream().CopyTo(stream);
			stream.Position = 0L;
			BinaryReader binaryReader = new BinaryReader(stream);
			int num2 = binaryReader.ReadInt32();
			try
			{
				for (int i = 0; i < num2; i++)
				{
					list.Add(SiparisOnaylamaListItem.ReadFromBinaryReader(binaryReader));
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			binaryReader.Dispose();
			binaryReader = null;
			stream.Dispose();
			stream = null;
		}
		return list;
	}

	public static Evrak GetEvrak(string servis_adresi, string firmaid, string kullanici_adi, string sifre, enum_GenelEvrakTipleri evraktipi, string evrakseri, string evraksira)
	{
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Invalid comparison between Unknown and I4
		Evrak result = new Evrak();
		_ = Encoding.UTF8;
		string url;
		if (new ServisKontrolu(1).ServisVersiyonu > 30005)
		{
			string[] obj = new string[16]
			{
				"http://",
				servis_adresi,
				"/GetEvrakV2/",
				firmaid,
				"/",
				AppBase.ForaPlatformTools.UrlEncode(kullanici_adi),
				"/",
				AppBase.ForaPlatformTools.UrlEncode(sifre),
				"/",
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			int num = (int)evraktipi;
			obj[9] = num.ToString();
			obj[10] = "/_";
			obj[11] = evrakseri;
			obj[12] = "/";
			obj[13] = evraksira.ToString();
			obj[14] = "/";
			obj[15] = AppBase._MikroDBName;
			url = string.Concat(obj);
		}
		else
		{
			string[] obj2 = new string[14]
			{
				"http://",
				servis_adresi,
				"/GetEvrak/",
				firmaid,
				"/",
				AppBase.ForaPlatformTools.UrlEncode(kullanici_adi),
				"/",
				AppBase.ForaPlatformTools.UrlEncode(sifre),
				"/",
				null,
				null,
				null,
				null,
				null
			};
			int num = (int)evraktipi;
			obj2[9] = num.ToString();
			obj2[10] = "/_";
			obj2[11] = evrakseri;
			obj2[12] = "/";
			obj2[13] = evraksira.ToString();
			url = string.Concat(obj2);
		}
		HttpWebResponse webResponseGet = HttpWebUtility.GetWebResponseGet(url, KeepAlive: false, "gzip, deflate", 20000, 20000);
		if (webResponseGet != null && (int)webResponseGet.StatusCode == 200)
		{
			Stream stream = new MemoryStream();
			((WebResponse)webResponseGet).GetResponseStream().CopyTo(stream);
			stream.Position = 0L;
			BinaryReader binaryReader = new BinaryReader(stream);
			try
			{
				result = Evrak.ReadFromBinaryReader(binaryReader);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			binaryReader.Dispose();
			binaryReader = null;
			stream.Dispose();
			stream = null;
		}
		return result;
	}

	public static string GetSiparisOnaylamaOnayla(string servis_adresi, string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Invalid comparison between Unknown and I4
		_ = Encoding.UTF8;
		string url = ((new ServisKontrolu(1).ServisVersiyonu <= 30005) ? ("http://" + servis_adresi + "/GetSiparisOnaylamaOnayla/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/_" + evrak_seri + "/" + evrak_sira) : ("http://" + servis_adresi + "/GetSiparisOnaylamaOnaylaV2/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/_" + evrak_seri + "/" + evrak_sira + "/" + AppBase._MikroDBName));
		HttpWebResponse webResponseGet = HttpWebUtility.GetWebResponseGet(url, KeepAlive: false, "gzip, deflate", 20000, 20000);
		if (webResponseGet != null)
		{
			if ((int)webResponseGet.StatusCode == 200)
			{
				return "true";
			}
			return "Hata : ";
		}
		return "Hata : ";
	}

	public static string GetSiparisOnaylamaReddet(string servis_adresi, string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira, string kapama_nedeni)
	{
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Invalid comparison between Unknown and I4
		_ = Encoding.UTF8;
		string url = ((new ServisKontrolu(1).ServisVersiyonu <= 30005) ? ("http://" + servis_adresi + "/GetSiparisOnaylamaReddet/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/_" + evrak_seri + "/" + evrak_sira + "/" + kapama_nedeni) : ("http://" + servis_adresi + "/GetSiparisOnaylamaReddetV2/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/_" + evrak_seri + "/" + evrak_sira + "/" + kapama_nedeni + "/" + AppBase._MikroDBName));
		HttpWebResponse webResponseGet = HttpWebUtility.GetWebResponseGet(url, KeepAlive: false, "gzip, deflate", 20000, 20000);
		if (webResponseGet != null)
		{
			if ((int)webResponseGet.StatusCode == 200)
			{
				return "true";
			}
			return "Hata : ";
		}
		return "Hata : ";
	}
}
