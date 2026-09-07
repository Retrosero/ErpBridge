using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Fora.Mikro.Senkronizasyon;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Stoklar;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct StokRestClient
{
	public static string StokAmbarAdresiDegistir(string servis_adresi, string firmaid, string kullanici_adi, string sifre, string stok_kodu, string ambar_adresleri)
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Invalid comparison between Unknown and I4
		string text = "";
		Encoding uTF = Encoding.UTF8;
		string text2 = "";
		text2 = ((new ServisKontrolu(1).ServisVersiyonu <= 30005) ? ("http://" + servis_adresi + "/StokAmbarAdresiDegistir/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/" + stok_kodu + "/" + ambar_adresleri) : ("http://" + servis_adresi + "/StokAmbarAdresiDegistirV2/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/" + stok_kodu + "/" + ambar_adresleri + "/" + AppBase._MikroDBName));
		HttpWebResponse webResponseGet = HttpWebUtility.GetWebResponseGet(text2, KeepAlive: false, "gzip, deflate", 20000, 20000);
		if (webResponseGet != null)
		{
			if ((int)webResponseGet.StatusCode == 200)
			{
				uTF = Encoding.UTF8;
				new StreamReader(((WebResponse)webResponseGet).GetResponseStream());
				StreamReader streamReader = new StreamReader(((WebResponse)webResponseGet).GetResponseStream(), uTF);
				string text3 = streamReader.ReadToEnd();
				streamReader.Dispose();
				((WebResponse)webResponseGet).Dispose();
				if (!text3.StartsWith("Hata"))
				{
					return "";
				}
				return text3;
			}
			throw new Exception();
		}
		throw new Exception();
	}
}
