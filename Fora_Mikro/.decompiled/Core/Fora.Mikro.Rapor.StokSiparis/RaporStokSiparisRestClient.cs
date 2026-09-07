using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Fora.Mikro.Senkronizasyon;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Rapor.StokSiparis;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RaporStokSiparisRestClient
{
	public static RaporStokSiparisSonucHam GetRaporStokSiparis(string servis_adresi, string firmaid, string kullanici_adi, string sifre, RaporStokSiparisSecenekleri rapor_secenekleri, string DepoNo, string ProjeKodu, string SorumlulukMerkeziKodu, string CariPersonelKodu, string StokKodu, string StokAnaGrupKodu, string StokUreticiKodu, string StokMarkaKodu, string StokReyonKodu, string StokKategoriKodu, string CariKodu, string CariBolgeKodu, string CariGrupKodu)
	{
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Invalid comparison between Unknown and I4
		RaporStokSiparisSonucHam raporStokSiparisSonucHam = new RaporStokSiparisSonucHam();
		string text = "";
		text = ((new ServisKontrolu(1).ServisVersiyonu <= 30005) ? ("http://" + servis_adresi + "/GetRaporStokSiparis/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre)) : ("http://" + servis_adresi + "/GetRaporStokSiparisV2/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/" + AppBase._MikroDBName));
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(DepoNo);
		binaryWriter.Write(ProjeKodu);
		binaryWriter.Write(SorumlulukMerkeziKodu);
		binaryWriter.Write(CariPersonelKodu);
		binaryWriter.Write(StokKodu);
		binaryWriter.Write(StokAnaGrupKodu);
		binaryWriter.Write(StokUreticiKodu);
		binaryWriter.Write(StokMarkaKodu);
		binaryWriter.Write(StokReyonKodu);
		binaryWriter.Write(StokKategoriKodu);
		binaryWriter.Write(CariKodu);
		binaryWriter.Write(CariBolgeKodu);
		binaryWriter.Write(CariGrupKodu);
		RaporStokSiparisSecenekleri.WriteToBinaryWriter(rapor_secenekleri, binaryWriter, 1);
		byte[] post_bilgisi = memoryStream.ToArray();
		HttpWebResponse webResponsePost = HttpWebUtility.GetWebResponsePost(text, post_bilgisi, KeepAlive: false, "gzip, deflate", 120000, 120000, "application");
		if (webResponsePost != null)
		{
			if ((int)webResponsePost.StatusCode == 200)
			{
				MemoryStream memoryStream2 = new MemoryStream();
				((WebResponse)webResponsePost).GetResponseStream().CopyTo(memoryStream2);
				memoryStream2.Position = 0L;
				byte[] buffer = (byte[])AppBase.ForaPlatformTools.ToObjectGZip(memoryStream2.ToArray());
				memoryStream2.Dispose();
				memoryStream2 = null;
				MemoryStream obj = new MemoryStream(buffer)
				{
					Position = 0L
				};
				raporStokSiparisSonucHam = RaporStokSiparisSonucHam.ReadFromStream(obj);
				obj.Dispose();
				return raporStokSiparisSonucHam;
			}
			throw new Exception();
		}
		throw new Exception();
	}

	public static MemoryStream GetRaporStokSiparisFile(string servis_adresi, string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, RaporStokSiparisSonuc rapor)
	{
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Invalid comparison between Unknown and I4
		MemoryStream memoryStream = new MemoryStream();
		string text = "";
		text = ((new ServisKontrolu(1).ServisVersiyonu <= 30005) ? ("http://" + servis_adresi + "/GetRaporFileStokSiparis/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/" + format + "/" + dosyaismi) : ("http://" + servis_adresi + "/GetRaporFileStokSiparisV2/" + firmaid + "/" + AppBase.ForaPlatformTools.UrlEncode(kullanici_adi) + "/" + AppBase.ForaPlatformTools.UrlEncode(sifre) + "/" + format + "/" + dosyaismi + "/" + AppBase._MikroDBName));
		MemoryStream memoryStream2 = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(memoryStream2);
		RaporStokSiparisSonuc.WriteToBinaryWriter(rapor, writer, 1);
		byte[] post_bilgisi = memoryStream2.ToArray();
		HttpWebResponse webResponsePost = HttpWebUtility.GetWebResponsePost(text, post_bilgisi, KeepAlive: false, "gzip, deflate", 20000, 20000, "application");
		if (webResponsePost != null && (int)webResponsePost.StatusCode == 200)
		{
			((WebResponse)webResponsePost).GetResponseStream().CopyTo(memoryStream);
		}
		return memoryStream;
	}
}
