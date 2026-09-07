using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace Fora.Mikro.Utility;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct HttpWebUtility
{
	public static HttpWebResponse GetWebResponsePost(string url, byte[] post_bilgisi, bool KeepAlive, string AcceptEncoding, int Timeout, int ReadWriteTimeout, string req_ContentType)
	{
		HttpWebResponse result = null;
		WebRequest obj = WebRequest.Create(url);
		HttpWebRequest val = (HttpWebRequest)(object)((obj is HttpWebRequest) ? obj : null);
		val.KeepAlive = KeepAlive;
		((WebRequest)val).Headers.Add((HttpRequestHeader)22, AcceptEncoding);
		val.AutomaticDecompression = (DecompressionMethods)1;
		((WebRequest)val).Credentials = CredentialCache.DefaultCredentials;
		((WebRequest)val).Timeout = Timeout;
		val.ReadWriteTimeout = ReadWriteTimeout;
		((WebRequest)val).Method = "POST";
		((WebRequest)val).ContentLength = post_bilgisi.Length;
		((WebRequest)val).ContentType = req_ContentType;
		Stream requestStream = ((WebRequest)val).GetRequestStream();
		requestStream.Write(post_bilgisi, 0, post_bilgisi.Length);
		requestStream.Close();
		try
		{
			WebResponse response = ((WebRequest)val).GetResponse();
			result = (HttpWebResponse)(object)((response is HttpWebResponse) ? response : null);
		}
		catch
		{
		}
		return result;
	}

	public static HttpWebResponse GetWebResponseGet(string url, bool KeepAlive, string AcceptEncoding, int Timeout, int ReadWriteTimeout)
	{
		HttpWebResponse result = null;
		WebRequest obj = WebRequest.Create(url);
		HttpWebRequest val = (HttpWebRequest)(object)((obj is HttpWebRequest) ? obj : null);
		val.KeepAlive = KeepAlive;
		((WebRequest)val).Headers.Add((HttpRequestHeader)22, AcceptEncoding);
		val.AutomaticDecompression = (DecompressionMethods)1;
		((WebRequest)val).Credentials = CredentialCache.DefaultCredentials;
		((WebRequest)val).Timeout = Timeout;
		val.ReadWriteTimeout = ReadWriteTimeout;
		((WebRequest)val).Method = "GET";
		try
		{
			WebResponse response = ((WebRequest)val).GetResponse();
			result = (HttpWebResponse)(object)((response is HttpWebResponse) ? response : null);
		}
		catch
		{
		}
		return result;
	}
}
