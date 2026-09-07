using System;
using System.ComponentModel;
using System.Data;
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

namespace Fora.Mikro.Utility;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GenelUtilityWin
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

	public static string SHA1Encode(string value)
	{
		SHA1 sHA = SHA1.Create();
		byte[] bytes = new ASCIIEncoding().GetBytes(value ?? "");
		return BitConverter.ToString(sHA.ComputeHash(bytes)).ToLower().Replace("-", "");
	}

	public static string EnumToString(Enum en)
	{
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
		DataTable dataTable = null;
		DataRow dataRow = null;
		DataColumn dataColumn = null;
		dataTable = new DataTable();
		dataColumn = new DataColumn("ID", typeof(int));
		dataTable.Columns.Add(dataColumn);
		dataColumn = new DataColumn("Isim", typeof(string));
		dataTable.Columns.Add(dataColumn);
		foreach (object value in Enum.GetValues(EnumObject))
		{
			dataRow = dataTable.NewRow();
			dataRow["ID"] = (int)value;
			dataRow["Isim"] = EnumToString((Enum)value);
			dataTable.Rows.Add(dataRow);
		}
		return dataTable;
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
		StringBuilder stringBuilder = new StringBuilder();
		if (typeof(T).IsSerializable)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			using (TextWriter textWriter = new StringWriter(stringBuilder))
			{
				xmlSerializer.Serialize(textWriter, entity);
			}
			return stringBuilder.ToString();
		}
		return null;
	}

	public static T Deserialize<T>(string xml)
	{
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
		if (o == null)
		{
			return new byte[0];
		}
		using MemoryStream memoryStream = new MemoryStream();
		using GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
		using MemoryStream memoryStream2 = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream2, o);
		memoryStream2.Position = 0L;
		memoryStream2.CopyTo(gZipStream);
		gZipStream.Close();
		return memoryStream.ToArray();
	}

	public static object ToObjectGZip(byte[] byteArray)
	{
		if (byteArray.Length == 0)
		{
			return null;
		}
		using MemoryStream memoryStream = new MemoryStream(byteArray);
		using MemoryStream memoryStream2 = new MemoryStream();
		using GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
		gZipStream.CopyTo(memoryStream2);
		memoryStream.Close();
		gZipStream.Close();
		memoryStream2.Position = 0L;
		return new BinaryFormatter().Deserialize(memoryStream2);
	}

	public static bool SendMail(MailBilgileri mailbilgileri, string Subject, string Body)
	{
		try
		{
			SmtpClient smtpClient = new SmtpClient();
			smtpClient.Host = mailbilgileri.SmtpAddress;
			smtpClient.Port = mailbilgileri.SmtpPort;
			smtpClient.EnableSsl = false;
			smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
			smtpClient.Credentials = new NetworkCredential(mailbilgileri.FromAddress, mailbilgileri.FromPassWord);
			smtpClient.Timeout = 20000;
			smtpClient.Send(mailbilgileri.FromAddress, mailbilgileri.ToAddress, Subject, Body);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
