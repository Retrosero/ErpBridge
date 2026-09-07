using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Fora.Mikro.CariHesaplar.CariEkstresi;
using Fora.Mikro.Senkronizasyon;
using Fora.Mikro.Utility;
using Newtonsoft.Json;

namespace Fora.Mikro.CariHesaplar;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CariRestclient
{
	public static string SendCariEkstre(CariEkstreButun ekstre, string servis_adresi, string firmaid, string kullanici_adi, string sifre, string alicilar, string konu, string mesaj, string format)
	{
		//IL_00f8: Expected O, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Invalid comparison between Unknown and I4
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		string result = "";
		Encoding uTF = Encoding.UTF8;
		string text = "";
		text = ((new ServisKontrolu(1).ServisVersiyonu <= 30005) ? ("http://" + servis_adresi + "/SendCariEkstre/" + firmaid) : ("http://" + servis_adresi + "/SendCariEkstreV2/" + firmaid + "/" + AppBase._MikroDBName));
		string item = JsonConvert.SerializeObject(ekstre);
		string s = JsonConvert.SerializeObject(new List<string>
		{
			kullanici_adi,
			AppBase.ForaPlatformTools.EncryptText("drjbq8777!#45", sifre, useHashing: true),
			alicilar,
			konu,
			mesaj,
			format,
			item
		});
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		HttpWebResponse webResponsePost;
		try
		{
			webResponsePost = HttpWebUtility.GetWebResponsePost(text, bytes, KeepAlive: false, "gzip, deflate", 60000, 60000, "text/plain");
		}
		catch (WebException ex)
		{
			WebException ex2 = ex;
			return "Hata : " + ((object)ex2).ToString();
		}
		catch (Exception ex3)
		{
			return "Hata : " + ex3.ToString();
		}
		if (webResponsePost == null)
		{
			return "Hata : ";
		}
		if ((int)webResponsePost.StatusCode != 200)
		{
			return "Hata : " + ((object)webResponsePost.StatusCode/*cast due to .constrained prefix*/).ToString();
		}
		StreamReader streamReader = new StreamReader(((WebResponse)webResponsePost).GetResponseStream(), uTF);
		string value = streamReader.ReadToEnd();
		streamReader.Dispose();
		((WebResponse)webResponsePost).Dispose();
		try
		{
			result = JsonConvert.DeserializeObject<string>(value);
		}
		catch
		{
		}
		return result;
	}
}
