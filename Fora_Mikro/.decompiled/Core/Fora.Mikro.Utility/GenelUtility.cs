using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Utility;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GenelUtility
{
	public static class RandomNumber
	{
		private static readonly RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider();

		public static int Between(int minimumValue, int maximumValue)
		{
			byte[] array = new byte[1];
			_generator.GetBytes(array);
			double num = Convert.ToDouble(array[0]);
			double num2 = Math.Max(0.0, num / 255.0 - 1E-11);
			int num3 = maximumValue - minimumValue + 1;
			double num4 = Math.Floor(num2 * (double)num3);
			return (int)((double)minimumValue + num4);
		}
	}

	public static int GetMikroVersiyon(string MikroDBName)
	{
		int result = 12;
		if (MikroDBName.StartsWith("MikroDB_V14"))
		{
			result = 14;
		}
		if (MikroDBName.StartsWith("MikroDB_V15"))
		{
			result = 15;
		}
		if (MikroDBName.StartsWith("MikroDB_V16"))
		{
			result = 16;
		}
		if (MikroDBName.StartsWith("MikroDB_V17"))
		{
			result = 17;
		}
		return result;
	}

	public static string SHA1Encode(string value)
	{
		SHA1 sHA = SHA1.Create();
		byte[] bytes = new ASCIIEncoding().GetBytes(value ?? "");
		return BitConverter.ToString(sHA.ComputeHash(bytes)).ToLower().Replace("-", "");
	}

	public static string EnumToString(Enum en)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		MemberInfo[] member = en.GetType().GetMember(en.ToString());
		if (member != null && member.Length != 0)
		{
			object[] customAttributes = member[0].GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				string description = ((DescriptionAttribute)customAttributes[0]).Description;
				customAttributes = null;
				return description;
			}
		}
		member = null;
		return en.ToString();
	}

	public static T ParseEnum<T>(string value)
	{
		return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
	}

	public static DataTable EnumToDataTable(Type EnumObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		DataTable val = null;
		DataRow val2 = null;
		DataColumn val3 = null;
		val = new DataTable();
		val3 = new DataColumn("ID", typeof(int));
		val.Columns.Add(val3);
		val3 = new DataColumn("Isim", typeof(string));
		val.Columns.Add(val3);
		foreach (object value in Enum.GetValues(EnumObject))
		{
			val2 = val.NewRow();
			val2["ID"] = (int)value;
			val2["Isim"] = EnumToString((Enum)value);
			val.Rows.Add(val2);
		}
		return val;
	}

	public static string MakeMD5(string metin)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(metin);
		return Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(bytes));
	}

	public static string EncryptText(string key, string toEncrypt, bool useHashing)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(toEncrypt);
		byte[] key2;
		if (useHashing)
		{
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			key2 = mD5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(key));
			mD5CryptoServiceProvider.Clear();
		}
		else
		{
			key2 = Encoding.UTF8.GetBytes(key);
		}
		TripleDESCryptoServiceProvider obj = new TripleDESCryptoServiceProvider
		{
			Key = key2,
			Mode = CipherMode.ECB,
			Padding = PaddingMode.PKCS7
		};
		byte[] array = obj.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
		obj.Clear();
		return Convert.ToBase64String(array, 0, array.Length);
	}

	public static string DecryptText(string key, string EncryptedText, bool useHashing)
	{
		byte[] array = Convert.FromBase64String(EncryptedText);
		byte[] key2;
		if (useHashing)
		{
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			key2 = mD5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(key));
			mD5CryptoServiceProvider.Clear();
		}
		else
		{
			key2 = Encoding.UTF8.GetBytes(key);
		}
		TripleDESCryptoServiceProvider obj = new TripleDESCryptoServiceProvider
		{
			Key = key2,
			Mode = CipherMode.ECB,
			Padding = PaddingMode.PKCS7
		};
		byte[] bytes = obj.CreateDecryptor().TransformFinalBlock(array, 0, array.Length);
		obj.Clear();
		return Encoding.UTF8.GetString(bytes);
	}

	public static string Serialize<T>(T entity)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		StringBuilder stringBuilder = new StringBuilder();
		if (typeof(T).IsSerializable)
		{
			XmlSerializer val = new XmlSerializer(typeof(T));
			using (TextWriter textWriter = new StringWriter(stringBuilder))
			{
				val.Serialize(textWriter, (object)entity);
			}
			return stringBuilder.ToString();
		}
		return null;
	}

	public static T Deserialize<T>(string xml)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (typeof(T).IsSerializable)
		{
			new XmlSerializer(typeof(T));
			using (new StringReader(xml))
			{
			}
			return default(T);
		}
		return default(T);
	}

	public static byte[] ToByteArrayGzip(object o)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (o == null)
		{
			return new byte[0];
		}
		using MemoryStream memoryStream = new MemoryStream();
		GZipStream val = new GZipStream((Stream)memoryStream, (CompressionMode)1);
		try
		{
			using MemoryStream memoryStream2 = new MemoryStream();
			new BinaryFormatter().Serialize((Stream)memoryStream2, o);
			memoryStream2.Position = 0L;
			memoryStream2.CopyTo((Stream)(object)val);
			((Stream)(object)val).Close();
			return memoryStream.ToArray();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static object ToObjectGZip(byte[] byteArray)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (byteArray.Length == 0)
		{
			return null;
		}
		using MemoryStream memoryStream = new MemoryStream(byteArray);
		using MemoryStream memoryStream2 = new MemoryStream();
		GZipStream val = new GZipStream((Stream)memoryStream, (CompressionMode)0);
		try
		{
			((Stream)(object)val).CopyTo((Stream)memoryStream2);
			memoryStream.Close();
			((Stream)(object)val).Close();
			memoryStream2.Position = 0L;
			return new BinaryFormatter().Deserialize((Stream)memoryStream2);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static string ConvertDoubleToString()
	{
		return "";
	}

	public static bool SendMail(MailBilgileri mailbilgileri, string Subject, string Body)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		try
		{
			SmtpClient smtpClient = new SmtpClient();
			smtpClient.Host = mailbilgileri.SmtpAddress;
			smtpClient.Port = mailbilgileri.SmtpPort;
			smtpClient.EnableSsl = false;
			smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
			smtpClient.Credentials = (ICredentialsByHost?)new NetworkCredential(mailbilgileri.FromAddress, mailbilgileri.FromPassWord);
			smtpClient.Timeout = 20000;
			smtpClient.Send(mailbilgileri.FromAddress, mailbilgileri.ToAddress, Subject, Body);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static string BasiniSifirlaTamamla(string deger, int ToplamUzunluk)
	{
		while (deger.Length != ToplamUzunluk)
		{
			deger = "0" + deger;
		}
		return deger;
	}

	public static string MiktariArtiliSekildeGoster(double Miktar, string VirgulileAyrilmisDegerler)
	{
		if (Miktar <= 0.0)
		{
			return Math.Round(Miktar, 2).ToString();
		}
		string result = Math.Round(Miktar, 2).ToString();
		string[] array = VirgulileAyrilmisDegerler.Split(new char[1] { ',' });
		for (int i = 0; i < array.Length; i++)
		{
			double num = double.Parse(array[i]);
			if (Miktar >= num)
			{
				result = "+" + Math.Round(num, 2);
			}
		}
		return result;
	}

	public static string YaziIleTutar(double tutar)
	{
		string text = tutar.ToString("F2").Replace('.', ',');
		string text2 = text.Substring(0, text.IndexOf(','));
		string text3 = text.Substring(text.IndexOf(',') + 1, 2);
		string text4 = "";
		string[] array = new string[10] { "", "BİR", "İKİ", "ÜÇ", "DÖRT", "BEŞ", "ALTI", "YEDİ", "SEKİZ", "DOKUZ" };
		string[] array2 = new string[10] { "", "ON", "YİRMİ", "OTUZ", "KIRK", "ELLİ", "ALTMIŞ", "YETMİŞ", "SEKSEN", "DOKSAN" };
		string[] array3 = new string[6] { "KATRİLYON", "TRİLYON", "MİLYAR", "MİLYON", "BİN", "" };
		int num = 6;
		text2 = text2.PadLeft(num * 3, '0');
		for (int i = 0; i < num * 3; i += 3)
		{
			string text5 = "";
			if (text2.Substring(i, 1) != "0")
			{
				text5 = text5 + array[Convert.ToInt32(text2.Substring(i, 1))] + "YÜZ";
			}
			if (text5 == "BİRYÜZ")
			{
				text5 = "YÜZ";
			}
			text5 += array2[Convert.ToInt32(text2.Substring(i + 1, 1))];
			text5 += array[Convert.ToInt32(text2.Substring(i + 2, 1))];
			if (text5 != "")
			{
				text5 += array3[i / 3];
			}
			if (text5 == "BİRBİN")
			{
				text5 = "BİN";
			}
			text4 += text5;
		}
		if (text4 != "")
		{
			text4 += " TL ";
		}
		int length = text4.Length;
		if (text3.Substring(0, 1) != "0")
		{
			text4 += array2[Convert.ToInt32(text3.Substring(0, 1))];
		}
		if (text3.Substring(1, 1) != "0")
		{
			text4 += array[Convert.ToInt32(text3.Substring(1, 1))];
		}
		if (text4.Length > length)
		{
			return text4 + " Kr.";
		}
		return text4 + "SIFIR Kr.";
	}

	public static string DotMatrixTextHazirla(string source, string hizalama, int karaktersayisi, string kirpmayeri)
	{
		if (source.Length > karaktersayisi)
		{
			if (kirpmayeri == "right")
			{
				return source.Substring(0, karaktersayisi);
			}
			return source.Substring(source.Length - karaktersayisi);
		}
		if (!(hizalama == "right"))
		{
			if (hizalama == "center")
			{
				int num = (karaktersayisi - source.Length) / 2;
				_ = source.Length;
				source = source.PadRight(source.Length + num);
				return source.PadLeft(karaktersayisi);
			}
			return source.PadRight(karaktersayisi);
		}
		return source.PadLeft(karaktersayisi);
	}

	public static string DoubleToString(double value, string binlik_ayraci, string ondalik_ayraci, int ondalik_hane_sayisi, bool sonuna_para_birimi_ekle, bool basina_para_birimi_ekle, string para_birimi_sembolu)
	{
		NumberFormatInfo numberFormatInfo = (NumberFormatInfo)new CultureInfo("tr-TR").NumberFormat.Clone();
		numberFormatInfo.NumberDecimalDigits = ondalik_hane_sayisi;
		numberFormatInfo.NumberDecimalSeparator = ondalik_ayraci;
		numberFormatInfo.NumberGroupSeparator = binlik_ayraci;
		string text = value.ToString("N", numberFormatInfo);
		if (basina_para_birimi_ekle)
		{
			text = para_birimi_sembolu + " " + text;
		}
		if (sonuna_para_birimi_ekle)
		{
			text = text + " " + para_birimi_sembolu;
		}
		return text;
	}

	public static int GetKdvPntr(int vergiorani)
	{
		return vergiorani switch
		{
			0 => 0, 
			1 => 2, 
			8 => 3, 
			18 => 4, 
			26 => 5, 
			_ => 0, 
		};
	}

	public static DateTime GetTarihCinsi(enum_tarih_cinsi tarihcinsi, bool BaslangicMi)
	{
		DateTime result = DateTime.Now;
		DateTime result2 = DateTime.Now;
		DateTime dateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
		DateTime dateTime2 = dateTime.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.0)
			.AddMilliseconds(99.0);
		DateTime dateTime3 = new DateTime(dateTime.Year, dateTime.Month, 1);
		DateTime dateTime4 = dateTime3.AddMonths(1).AddDays(-1.0).AddHours(23.0)
			.AddMinutes(59.0)
			.AddSeconds(59.0)
			.AddMilliseconds(99.0);
		DateTime dateTime5 = new DateTime(dateTime.Year, 1, 1);
		DateTime dateTime6 = dateTime5.AddYears(1).AddMilliseconds(-1.0);
		switch (tarihcinsi)
		{
		case enum_tarih_cinsi.TumZamanlar:
			result = dateTime5.AddYears(-10);
			result2 = dateTime5.AddYears(10);
			break;
		case enum_tarih_cinsi.Dun:
			result = dateTime.AddDays(-1.0);
			result2 = dateTime2.AddDays(-1.0);
			break;
		case enum_tarih_cinsi.Bugun:
			result = dateTime;
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.BuHafta:
			result = StartOfWeek(dateTime, DayOfWeek.Monday);
			result2 = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(6.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.BuAy:
			result = dateTime3;
			result2 = dateTime4;
			break;
		case enum_tarih_cinsi.BuYil:
			result = dateTime5;
			result2 = dateTime6;
			break;
		case enum_tarih_cinsi.GecenHafta:
			result = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(-7.0);
			result2 = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GecenAy:
			result = dateTime3.AddMonths(-1);
			result2 = dateTime3.AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GecenYil:
			result = dateTime5.AddYears(-1);
			result2 = dateTime5.AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Son3Ay:
			result = dateTime3.AddMonths(-2);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son6Ay:
			result = dateTime3.AddMonths(-5);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son12Ay:
			result = dateTime3.AddMonths(-11);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son7Gun:
			result = dateTime.AddDays(-6.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son15Gun:
			result = dateTime.AddDays(-14.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son30Gun:
			result = dateTime.AddDays(-29.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son60Gun:
			result = dateTime.AddDays(-59.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son90Gun:
			result = dateTime.AddDays(-89.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son365Gun:
			result = dateTime.AddDays(-364.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son24Saat:
			result = DateTime.Now.AddHours(-24.0);
			result2 = DateTime.Now;
			break;
		case enum_tarih_cinsi.Yarin:
			result = dateTime.AddDays(1.0);
			result2 = dateTime.AddDays(1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GelecekHafta:
			result = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(7.0);
			result2 = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(13.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek7Gun:
			result = dateTime;
			result2 = dateTime.AddDays(6.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek15Gun:
			result = dateTime;
			result2 = dateTime.AddDays(14.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek30Gun:
			result = dateTime;
			result2 = dateTime.AddDays(29.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek60Gun:
			result = dateTime;
			result2 = dateTime.AddDays(59.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek90Gun:
			result = dateTime;
			result2 = dateTime.AddDays(89.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek365Gun:
			result = dateTime;
			result2 = dateTime.AddDays(364.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GelecekAy:
			result = dateTime3.AddMonths(1);
			result2 = dateTime3.AddMonths(2).AddDays(-1.0).AddHours(23.0)
				.AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek3Ay:
			result = dateTime3.AddMonths(1);
			result2 = dateTime3.AddMonths(4).AddDays(-1.0).AddHours(23.0)
				.AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek6Ay:
			result = dateTime3.AddMonths(1);
			result2 = dateTime3.AddMonths(7).AddDays(-1.0).AddHours(23.0)
				.AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek12Ay:
			result = dateTime3.AddMonths(1);
			result2 = dateTime3.AddMonths(13).AddDays(-1.0).AddHours(23.0)
				.AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GelecekYil:
			result = dateTime5.AddYears(1);
			result2 = dateTime5.AddYears(2).AddDays(-1.0).AddHours(23.0)
				.AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		}
		if (BaslangicMi)
		{
			return result;
		}
		return result2;
	}

	private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
	{
		int num = dt.DayOfWeek - startOfWeek;
		if (num < 0)
		{
			num += 7;
		}
		return dt.AddDays(-1 * num).Date;
	}
}
