using System;
using System.IO;
using System.Net;
using System.Text;

namespace Fora.Mikro;

public class Lisans
{
	public DateTime LisansSonlanmaTarihi { get; set; }

	public string LisansliModuller { get; set; }

	public string LisansSahibi { get; set; }

	public Lisans()
	{
		LisansSonlanmaTarihi = new DateTime(1980, 1, 1);
		LisansliModuller = "";
		LisansSahibi = "";
	}

	public bool ModulLisansli(string ModulAdi)
	{
		if (LisansliModuller.Contains(ModulAdi))
		{
			return true;
		}
		return false;
	}

	public static Lisans GetLisans(string SqlServer, string modul)
	{
		Lisans lisans = new Lisans();
		string text = "";
		try
		{
			Encoding uTF = Encoding.UTF8;
			string text2 = SqlServer;
			if (text2.Contains("\\"))
			{
				text2 = text2.Substring(0, text2.IndexOf("\\"));
			}
			HttpWebRequest httpWebRequest = WebRequest.Create("http://" + text2 + ":1409/GetLisans/" + modul) as HttpWebRequest;
			httpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
			httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
			httpWebRequest.Credentials = CredentialCache.DefaultCredentials;
			httpWebRequest.Timeout = 20000;
			httpWebRequest.ReadWriteTimeout = 20000;
			httpWebRequest.Method = "GET";
			try
			{
				if (httpWebRequest.GetResponse() is HttpWebResponse { StatusCode: HttpStatusCode.OK } httpWebResponse)
				{
					StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), uTF);
					text = streamReader.ReadToEnd();
					streamReader.Close();
					httpWebResponse.Close();
				}
				string[] array = text.Split('|');
				lisans.LisansSahibi = array[1];
				lisans.LisansliModuller = array[0];
			}
			catch
			{
			}
		}
		catch
		{
		}
		return lisans;
	}
}
