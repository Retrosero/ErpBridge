using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Senkronizasyon;
using Fora.Mikro.Siparis;
using Fora.Mikro.Utility;

namespace Fora.Mikro.StokHareket;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DepolarArasiSevkRestClient
{
	public static List<STOK_HAREKETLERI> GetNakliyedekiUrunlerV2(string servis_adresi, string firmaid, string kullanici_adi, string sifre, int nakliye_depo_no, int hedef_depo_no)
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Invalid comparison between Unknown and I4
		List<STOK_HAREKETLERI> list = new List<STOK_HAREKETLERI>();
		_ = Encoding.UTF8;
		string text = "";
		text = ((new ServisKontrolu(1).ServisVersiyonu <= 30005) ? ("http://" + servis_adresi + "/GetNakliyedekiUrunlerV2/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/" + nakliye_depo_no + "/" + hedef_depo_no) : ("http://" + servis_adresi + "/GetNakliyedekiUrunlerV3/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/" + nakliye_depo_no + "/" + hedef_depo_no + "/" + AppBase._MikroDBName));
		HttpWebResponse webResponseGet = HttpWebUtility.GetWebResponseGet(text, KeepAlive: false, "gzip, deflate", 20000, 20000);
		if (webResponseGet != null)
		{
			if ((int)webResponseGet.StatusCode == 200)
			{
				Stream stream = new MemoryStream();
				((WebResponse)webResponseGet).GetResponseStream().CopyTo(stream);
				stream.Position = 0L;
				BinaryReader binaryReader = new BinaryReader(stream);
				int num = binaryReader.ReadInt32();
				try
				{
					for (int i = 0; i < num; i++)
					{
						list.Add(STOK_HAREKETLERI.ReadFromBinaryReader(binaryReader));
					}
					try
					{
						for (int j = 0; j < num; j++)
						{
							list[j].sth_Guid = new Guid(binaryReader.ReadBytes(16));
							list[j].sth_fat_uid = new Guid(binaryReader.ReadBytes(16));
							list[j].sth_sip_uid = new Guid(binaryReader.ReadBytes(16));
							list[j].sth_kons_uid = new Guid(binaryReader.ReadBytes(16));
							list[j].sth_subesip_uid = new Guid(binaryReader.ReadBytes(16));
							list[j].sth_yetkili_uid = new Guid(binaryReader.ReadBytes(16));
						}
					}
					catch
					{
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
				return list;
			}
			throw new Exception();
		}
		throw new Exception();
	}

	public static List<SiparisEvrakListItem> GetDepolarArasiSiparisEvrakListV3(string servis_adresi, string firmaid, string kullanici_adi, string sifre, int kaynak_depo_no, int hedef_depo_no, enum_sip_orderby sip_orderby)
	{
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Invalid comparison between Unknown and I4
		List<SiparisEvrakListItem> list = new List<SiparisEvrakListItem>();
		_ = Encoding.UTF8;
		string url;
		if (new ServisKontrolu(1).ServisVersiyonu > 30005)
		{
			string[] obj = new string[16]
			{
				"http://",
				servis_adresi,
				"/GetDepolarArasiSiparisEvrakListV4/",
				firmaid,
				"/",
				AppBase.ForaPlatformTools.UrlEncode(kullanici_adi),
				"/",
				AppBase.ForaPlatformTools.UrlEncode(sifre),
				"/",
				kaynak_depo_no.ToString(),
				"/",
				hedef_depo_no.ToString(),
				"/",
				null,
				null,
				null
			};
			int num = (int)sip_orderby;
			obj[13] = num.ToString();
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
				"/GetDepolarArasiSiparisEvrakListV3/",
				firmaid,
				"/",
				AppBase.ForaPlatformTools.UrlEncode(kullanici_adi),
				"/",
				AppBase.ForaPlatformTools.UrlEncode(sifre),
				"/",
				kaynak_depo_no.ToString(),
				"/",
				hedef_depo_no.ToString(),
				"/",
				null
			};
			int num = (int)sip_orderby;
			obj2[13] = num.ToString();
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
					list.Add(SiparisEvrakListItem.ReadFromStream(binaryReader));
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

	public static Evrak GetDepolarArasiSiparisEvrakV2(string servis_adresi, string firmaid, string kullanici_adi, string sifre, string sip_evrakno_seri, int sip_evrakno_sira)
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Invalid comparison between Unknown and I4
		Evrak evrak = new Evrak();
		_ = Encoding.UTF8;
		string url = ((new ServisKontrolu(1).ServisVersiyonu <= 30005) ? ("http://" + servis_adresi + "/GetDepolarArasiSiparisEvrakV2/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/_" + sip_evrakno_seri + "/" + sip_evrakno_sira) : ("http://" + servis_adresi + "/GetDepolarArasiSiparisEvrakV3/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/_" + sip_evrakno_seri + "/" + sip_evrakno_sira + "/" + AppBase._MikroDBName));
		HttpWebResponse webResponseGet = HttpWebUtility.GetWebResponseGet(url, KeepAlive: false, "gzip, deflate", 20000, 20000);
		if (webResponseGet != null)
		{
			if ((int)webResponseGet.StatusCode == 200)
			{
				Stream stream = new MemoryStream();
				((WebResponse)webResponseGet).GetResponseStream().CopyTo(stream);
				stream.Position = 0L;
				return Evrak.ReadFromStream(stream);
			}
			throw new Exception();
		}
		throw new Exception();
	}
}
