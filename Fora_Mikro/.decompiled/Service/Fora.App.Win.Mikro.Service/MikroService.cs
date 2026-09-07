using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using DevExpress.XtraPrinting;
using Fora.Mikro;
using Fora.Mikro.Bakim;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.CariHesaplar.CariEkstresi;
using Fora.Mikro.CariHesaplar.CariYetkilileri;
using Fora.Mikro.Cihazlar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Depolar;
using Fora.Mikro.Dovizler;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Rapor.StokEnvanter;
using Fora.Mikro.Rapor.StokSatis;
using Fora.Mikro.Rapor.StokSiparis;
using Fora.Mikro.Rapor.YapilacakTahsilat;
using Fora.Mikro.Siparis;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Tablolar;
using Fora.Mikro.TeknikTesisYonetimi;
using Fora.Mikro.Temsilciler;
using Fora.Mikro.Utility;
using Fora.Mikro.Vergiler;
using Fora.Mikro.Win.Form.DevEx.Raporlar;
using Newtonsoft.Json;

namespace Fora.App.Win.Mikro.Service;

[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
[CallbackBehavior(UseSynchronizationContext = false)]
public class MikroService : IMikroService, ITeknikTesisAdmin, ITeknikTesis, ILisansYoneticisiService
{
	public Message IsAlive()
	{
		return WebOperationContext.Current.CreateTextResponse("true", "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message GetVersion()
	{
		return WebOperationContext.Current.CreateTextResponse("30214", "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Stream GetErrorLog()
	{
		return File.OpenRead(string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\", "data\\logs\\errorlog.log"));
	}

	public Message GetFirmalar()
	{
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		string text2 = "";
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(text + "data\\servisbilgileri.xml");
			XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Firma");
			for (int i = 0; i < elementsByTagName.Count; i++)
			{
				if (i != 0)
				{
					text2 += ",";
				}
				text2 += elementsByTagName[i].Attributes["id"].Value;
				text2 = text2 + "," + elementsByTagName[i].Attributes["MikroDbName"].Value;
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			throw ex;
		}
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message CheckSqlConnection()
	{
		string text = "true";
		try
		{
			SqlBaglantiBilgileri sqlBaglantiBilgileri = new SqlBaglantiBilgileri(string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\", "data\\sqlbaglantibilgileri.xml"));
			SqlConnection sqlConnection = new SqlConnection();
			if (sqlBaglantiBilgileri.SqlUserName == "")
			{
				sqlConnection.ConnectionString = "Data Source=" + sqlBaglantiBilgileri.SqlServer + "; Initial Catalog=master; Trusted_Connection=true;MultipleActiveResultSets=True;";
			}
			else
			{
				sqlConnection.ConnectionString = "Data Source=" + sqlBaglantiBilgileri.SqlServer + "; Initial Catalog=master; User Id=" + sqlBaglantiBilgileri.SqlUserName + ";Password=" + sqlBaglantiBilgileri.SqlPassword + ";MultipleActiveResultSets=True;";
			}
			sqlConnection.Open();
			sqlConnection.Close();
		}
		catch (Exception ex)
		{
			text = "false";
			Log_Error(ex.ToString());
		}
		return WebOperationContext.Current.CreateTextResponse(text, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message CheckSqlRead(string firmaid)
	{
		string text = "false";
		List<string> list = new List<string>();
		string text2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text2 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text2 + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				list.Add((string)xElement.Attribute("MikroDbName"));
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			throw ex2;
		}
		text = "true";
		try
		{
			foreach (string item in list)
			{
				SqlDB sqlDB = new SqlDB();
				sqlDB.ConnectionOpen(baglantiBilgileri, item);
				using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
				{
					string commandText = "SELECT TOP 1 * FROM _FORA_PARAMETRELER";
					sqlCommand.CommandText = commandText;
					sqlCommand.ExecuteNonQuery();
				}
				sqlDB.ConnectionClose();
			}
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			text = "false";
		}
		return WebOperationContext.Current.CreateTextResponse(text, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message CheckSqlWrite(string firmaid)
	{
		string text = "false";
		List<string> list = new List<string>();
		string text2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text2 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text2 + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				list.Add((string)xElement.Attribute("MikroDbName"));
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			throw ex2;
		}
		try
		{
			text = "true";
			foreach (string item in list)
			{
				SqlDB sqlDB = new SqlDB();
				sqlDB.ConnectionOpen(baglantiBilgileri, item);
				using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
				{
					sqlCommand.CommandText = "CREATE TABLE [_FORA_TEST]([ID] [int] IDENTITY(1,1) NOT NULL,[Test] [nvarchar](40) NOT NULL,CONSTRAINT [PK_FORA_TEST] PRIMARY KEY CLUSTERED ([ID] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]";
					sqlCommand.ExecuteNonQuery();
					sqlCommand.CommandText = "INSERT INTO _FORA_TEST (Test) VALUES ('test') ";
					sqlCommand.ExecuteNonQuery();
					sqlCommand.CommandText = "DELETE _FORA_TEST";
					sqlCommand.ExecuteNonQuery();
					sqlCommand.CommandText = "DROP TABLE _FORA_TEST";
					sqlCommand.ExecuteNonQuery();
				}
				sqlDB.ConnectionClose();
			}
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			text = "false";
		}
		return WebOperationContext.Current.CreateTextResponse(text, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Stream GetApkFile()
	{
		return File.OpenRead(string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\", "data\\foraandroid.apk"));
	}

	public Message GetMikroDBName(string firmaid)
	{
		string text = "";
		string text2 = "";
		SqlBaglantiBilgileri baglantibilgileri;
		try
		{
			text2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
			baglantibilgileri = new SqlBaglantiBilgileri(text2 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			return WebOperationContext.Current.CreateTextResponse("Hata : " + ex.ToString(), "application/json;charset=utf-8", Encoding.UTF8);
		}
		try
		{
			using FileStream stream = File.OpenRead(text2 + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				text = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			return WebOperationContext.Current.CreateTextResponse("Hata : " + ex2.ToString(), "application/json;charset=utf-8", Encoding.UTF8);
		}
		try
		{
			new List<string>();
			string[] array = text.Split(',');
			foreach (string text3 in array)
			{
				bool flag = false;
				if (text3.StartsWith("MikroDB_V12"))
				{
					flag = true;
				}
				if (text3.StartsWith("MikroDB_V14"))
				{
					flag = true;
				}
				if (text3.StartsWith("MikroDB_V15"))
				{
					flag = true;
				}
				if (flag)
				{
					string mikroAnaDBName = "MikroDB_V14";
					if (text.StartsWith("MikroDB_V15"))
					{
						mikroAnaDBName = "MikroDB_V15";
					}
					if (text.StartsWith("MikroDB_V12"))
					{
						mikroAnaDBName = "MikroDB_V12";
					}
					if (text.StartsWith("MikroDB_V16"))
					{
						mikroAnaDBName = "MikroDB_V16";
					}
					DataBaseKurulumu(baglantibilgileri, text3, mikroAnaDBName);
				}
			}
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			return WebOperationContext.Current.CreateTextResponse("Hata : " + ex3.ToString(), "application/json;charset=utf-8", Encoding.UTF8);
		}
		string text4 = JsonConvert.SerializeObject(text, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text4, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message KullaniciKontroluV2(string firmaid, string kullanici_adi, string sifre)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			throw ex;
		}
		return KullaniciKontroluV3(firmaid, kullanici_adi, sifre, mikroDbName);
	}

	public Message KullaniciKontroluV3(string firmaid, string kullanici_adi, string sifre, string MikroDbName)
	{
		string text = "false";
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text2 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			throw ex;
		}
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser COLLATE Turkish_CS_AS AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					text = "true";
				}
			}
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse(text, "application/json;charset=utf-8", Encoding.UTF8);
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			return WebOperationContext.Current.CreateTextResponse("Hata : " + ex2.ToString(), "application/json;charset=utf-8", Encoding.UTF8);
		}
	}

	public Message GetLastTriggerRecNo(string firmaid)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetLastTriggerRecNoV2(firmaid, mikroDbName);
	}

	public Message GetLastTriggerRecNoV2(string firmaid, string MikroDbName)
	{
		int num = 0;
		firmaid = HttpUtility.UrlDecode(firmaid);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT TOP 1 * FROM _FORA_SENKRONIZASYON WHERE Islem<>2 ORDER BY TriggerRECno DESC";
				sqlCommand.CommandText = commandText;
				try
				{
					num = int.Parse(sqlCommand.ExecuteScalar().ToString());
				}
				catch
				{
					num = 0;
				}
			}
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse(num.ToString(), "application/json;charset=utf-8", Encoding.UTF8);
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string text2 = JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
		}
	}

	public Stream GetTabloDegisenKayitSayilari(string firmaid, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		return GetTabloDegisenKayitSayilariV2(firmaid, mikroDbName, Content);
	}

	public Stream GetTabloDegisenKayitSayilariV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		try
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				list.Add(binaryReader.ReadInt32());
				list2.Add(binaryReader.ReadInt32());
				list3.Add(binaryReader.ReadInt32());
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		try
		{
			SqlDB sqlDB2 = new SqlDB();
			List<int> list4 = new List<int>();
			sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
			for (int j = 0; j < list.Count; j++)
			{
				string commandText2 = "SELECT COUNT(KayitRECno) FROM _FORA_SENKRONIZASYON WHERE TriggerRECno>@TriggerRECno AND TabloID=@TabloID AND Islem=1 AND KayitRECno<=@OfflineSonRecNo";
				using SqlCommand sqlCommand2 = sqlDB2.Connection.CreateCommand();
				sqlCommand2.CommandText = commandText2;
				sqlCommand2.Parameters.AddWithValue("@TriggerRECno", list3[j]);
				sqlCommand2.Parameters.AddWithValue("@TabloID", list[j]);
				sqlCommand2.Parameters.AddWithValue("@OfflineSonRecNo", list2[j]);
				try
				{
					list4.Add(int.Parse(sqlCommand2.ExecuteScalar().ToString()));
				}
				catch
				{
					list4.Add(0);
				}
			}
			sqlDB2.ConnectionClose();
			Stream stream = new MemoryStream();
			BinaryWriter binaryWriter5 = new BinaryWriter(stream);
			binaryWriter5.Write(value: true);
			foreach (int item in list4)
			{
				binaryWriter5.Write(item);
			}
			binaryWriter5.Flush();
			stream.Position = 0L;
			return stream;
		}
		catch (Exception ex4)
		{
			Log_Error(ex4.ToString());
			_ = "Hata: " + ex4.ToString();
			string value5 = "Hata : " + ex4.ToString();
			MemoryStream memoryStream5 = new MemoryStream();
			BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream5);
			binaryWriter6.Write(value: false);
			binaryWriter6.Write(value5);
			binaryWriter6.Flush();
			memoryStream5.Position = 0L;
			return memoryStream5;
		}
	}

	public Stream V16_GetTabloDegisenKayitSayilariV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		try
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				list.Add(binaryReader.ReadInt32());
				list2.Add(binaryReader.ReadInt32());
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		try
		{
			SqlDB sqlDB2 = new SqlDB();
			List<int> list3 = new List<int>();
			sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
			for (int j = 0; j < list.Count; j++)
			{
				string commandText2 = "SELECT COUNT(KayitGuid) FROM _FORA_SYNC WHERE TriggerRECno>@TriggerRECno AND TabloID=@TabloID";
				using SqlCommand sqlCommand2 = sqlDB2.Connection.CreateCommand();
				sqlCommand2.CommandText = commandText2;
				sqlCommand2.Parameters.AddWithValue("@TriggerRECno", list2[j]);
				sqlCommand2.Parameters.AddWithValue("@TabloID", list[j]);
				try
				{
					list3.Add(int.Parse(sqlCommand2.ExecuteScalar().ToString()));
				}
				catch
				{
					list3.Add(0);
				}
			}
			sqlDB2.ConnectionClose();
			Stream stream = new MemoryStream();
			BinaryWriter binaryWriter5 = new BinaryWriter(stream);
			binaryWriter5.Write(value: true);
			foreach (int item in list3)
			{
				binaryWriter5.Write(item);
			}
			binaryWriter5.Flush();
			stream.Position = 0L;
			return stream;
		}
		catch (Exception ex4)
		{
			Log_Error(ex4.ToString());
			_ = "Hata: " + ex4.ToString();
			string value5 = "Hata : " + ex4.ToString();
			MemoryStream memoryStream5 = new MemoryStream();
			BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream5);
			binaryWriter6.Write(value: false);
			binaryWriter6.Write(value5);
			binaryWriter6.Flush();
			memoryStream5.Position = 0L;
			return memoryStream5;
		}
	}

	public Stream GetTabloYapisiToplu(string firmaid, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servis bilgileri çekilemedi.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		return GetTabloYapisiTopluV2(firmaid, mikroDbName, Content);
	}

	public Stream GetTabloYapisiTopluV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		List<string> list = new List<string>();
		try
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				list.Add(binaryReader.ReadString());
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		MemoryStream memoryStream5 = new MemoryStream();
		BinaryWriter binaryWriter5 = new BinaryWriter(memoryStream5);
		binaryWriter5.Write(value: true);
		for (int j = 0; j < list.Count; j++)
		{
			try
			{
				SqlDB sqlDB2 = new SqlDB();
				DataTable dataTable = new DataTable();
				if (list[j] == "DOVIZ_KURLARI" || list[j] == "YEREL_BANKA_KODLARI" || list[j] == "KUR_ISIMLERI")
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName.Substring(0, MikroDbName.IndexOf("_", MikroDbName.IndexOf("_", 0) + 1)));
				}
				else
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
				}
				if (list[j] == "YEREL_BANKA_KODLARI" && MikroDbName.StartsWith("MikroDB_V12"))
				{
					list[j] = "BANKA_TCMB_KODLARI";
				}
				SqlCommand sqlCommand2 = new SqlCommand("SELECT TOP 1 * FROM " + list[j] + " WITH (NOLOCK) WHERE 1=0", sqlDB2.Connection);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand2);
				sqlDataAdapter.Fill(dataTable);
				if (list[j] == "CARI_HESAPLAR" && MikroDbName.StartsWith("MikroDB_V15"))
				{
					dataTable.Columns["cari_baglanti_tipi"].ColumnName = "cari_tipi";
				}
				if (list[j] == "YEREL_BANKA_KODLARI" && MikroDbName.StartsWith("MikroDB_V12"))
				{
					list[j] = "BANKA_TCMB_KODLARI";
				}
				sqlDataAdapter.Dispose();
				sqlCommand2.Dispose();
				string text4 = "";
				string text5 = "";
				foreach (DataColumn column in dataTable.Columns)
				{
					text4 = text4 + text5 + column.ColumnName;
					text5 = ",";
				}
				binaryWriter5.Write(text4);
				sqlDB2.ConnectionClose();
			}
			catch (Exception ex4)
			{
				Log_Error(ex4.ToString());
				_ = "Hata: " + ex4.ToString();
				string value5 = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
				MemoryStream memoryStream6 = new MemoryStream();
				BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream6);
				binaryWriter6.Write(value: false);
				binaryWriter6.Write(value5);
				binaryWriter6.Flush();
				memoryStream6.Position = 0L;
				return memoryStream6;
			}
		}
		return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream5.ToArray()));
	}

	public Stream V16_GetTabloYapisiTopluV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		List<string> list = new List<string>();
		try
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				list.Add(binaryReader.ReadString());
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		MemoryStream memoryStream5 = new MemoryStream();
		BinaryWriter binaryWriter5 = new BinaryWriter(memoryStream5);
		binaryWriter5.Write(value: true);
		for (int j = 0; j < list.Count; j++)
		{
			try
			{
				SqlDB sqlDB2 = new SqlDB();
				DataTable dataTable = new DataTable();
				if (list[j] == "DOVIZ_KURLARI" || list[j] == "YEREL_BANKA_KODLARI" || list[j] == "KUR_ISIMLERI")
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName.Substring(0, MikroDbName.IndexOf("_", MikroDbName.IndexOf("_", 0) + 1)));
				}
				else
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
				}
				SqlCommand sqlCommand2 = new SqlCommand("SELECT TOP 1 * FROM " + list[j] + " WITH (NOLOCK) WHERE 1=0", sqlDB2.Connection);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand2);
				sqlDataAdapter.Fill(dataTable);
				if (list[j] == "CARI_HESAPLAR")
				{
					dataTable.Columns["cari_baglanti_tipi"].ColumnName = "cari_tipi";
				}
				sqlDataAdapter.Dispose();
				sqlCommand2.Dispose();
				string text4 = "";
				string text5 = "";
				foreach (DataColumn column in dataTable.Columns)
				{
					text4 = text4 + text5 + column.ColumnName;
					text5 = ",";
				}
				binaryWriter5.Write(text4);
				sqlDB2.ConnectionClose();
			}
			catch (Exception ex4)
			{
				Log_Error(ex4.ToString());
				_ = "Hata: " + ex4.ToString();
				string value5 = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
				MemoryStream memoryStream6 = new MemoryStream();
				BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream6);
				binaryWriter6.Write(value: false);
				binaryWriter6.Write(value5);
				binaryWriter6.Flush();
				memoryStream6.Position = 0L;
				return memoryStream6;
			}
		}
		return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream5.ToArray()));
	}

	public Stream GetYeniKayitlarToplu(string firmaid, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servis bilgileri çekilemedi.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		return GetYeniKayitlarTopluV2(firmaid, mikroDbName, Content);
	}

	public Stream GetYeniKayitlarTopluV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		int num = 1000;
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		num = binaryReader.ReadInt32();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<int> list3 = new List<int>();
		List<string> list4 = new List<string>();
		try
		{
			int num2 = binaryReader.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				list.Add(binaryReader.ReadString());
				list2.Add(binaryReader.ReadString());
				list3.Add(binaryReader.ReadInt32());
				list4.Add(binaryReader.ReadString());
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		MemoryStream memoryStream5 = new MemoryStream();
		BinaryWriter binaryWriter5 = new BinaryWriter(memoryStream5);
		binaryWriter5.Write(value: true);
		for (int j = 0; j < list.Count; j++)
		{
			try
			{
				SqlDB sqlDB2 = new SqlDB();
				DataTable dataTable = new DataTable();
				if (list[j] == "DOVIZ_KURLARI" || list[j] == "YEREL_BANKA_KODLARI" || list[j] == "KUR_ISIMLERI")
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName.Substring(0, MikroDbName.IndexOf("_", MikroDbName.IndexOf("_", 0) + 1)));
				}
				else
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
				}
				if (list[j] == "CARI_HESAPLAR" && MikroDbName.StartsWith("MikroDB_V15"))
				{
					list4[j] = list4[j].Replace("cari_tipi", "cari_baglanti_tipi");
				}
				if (list[j] == "YEREL_BANKA_KODLARI" && MikroDbName.StartsWith("MikroDB_V12"))
				{
					list[j] = "BANKA_TCMB_KODLARI";
				}
				SqlCommand sqlCommand2 = new SqlCommand("SELECT TOP 1 " + list4[j] + " FROM " + list[j] + " WITH (NOLOCK) WHERE " + list2[j] + ">" + list3[j] + " ORDER BY " + list2[j], sqlDB2.Connection);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand2);
				sqlDataAdapter.Fill(dataTable);
				if (list[j] == "CARI_HESAPLAR" && MikroDbName.StartsWith("MikroDB_V15"))
				{
					dataTable.Columns["cari_baglanti_tipi"].ColumnName = "cari_tipi";
				}
				sqlDataAdapter.Dispose();
				sqlCommand2.Dispose();
				binaryWriter5.Write(dataTable.Columns.Count);
				foreach (DataColumn column in dataTable.Columns)
				{
					binaryWriter5.Write(column.ColumnName);
					binaryWriter5.Write(column.DataType.ToString());
				}
				SqlDataReader r = new SqlCommand("SELECT TOP " + num + " " + list4[j] + " FROM " + list[j] + " WITH (NOLOCK) WHERE " + list2[j] + ">" + list3[j].ToString() + " ORDER BY " + list2[j], sqlDB2.Connection).ExecuteReader();
				SqlReaderStreamYaz(dataTable, binaryWriter5, r);
				sqlDB2.ConnectionClose();
			}
			catch (Exception ex4)
			{
				Log_Error(ex4.ToString());
				_ = "Hata: " + ex4.ToString();
				string value5 = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
				MemoryStream memoryStream6 = new MemoryStream();
				BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream6);
				binaryWriter6.Write(value: false);
				binaryWriter6.Write(value5);
				binaryWriter6.Flush();
				memoryStream6.Position = 0L;
				return memoryStream6;
			}
		}
		return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream5.ToArray()));
	}

	public Stream GetTabloYeniKayitSayilari(string firmaid, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		return GetTabloYeniKayitSayilariV2(firmaid, mikroDbName, Content);
	}

	public Stream GetTabloYeniKayitSayilariV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<int> list3 = new List<int>();
		try
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				list.Add(binaryReader.ReadString());
				list2.Add(binaryReader.ReadString());
				list3.Add(binaryReader.ReadInt32());
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		try
		{
			SqlDB sqlDB2 = new SqlDB();
			List<int> list4 = new List<int>();
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] == "DOVIZ_KURLARI" || list[j] == "YEREL_BANKA_KODLARI" || list[j] == "KUR_ISIMLERI")
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName.Substring(0, MikroDbName.IndexOf("_", MikroDbName.IndexOf("_", 0) + 1)));
				}
				else
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
				}
				if (list[j] == "YEREL_BANKA_KODLARI" && MikroDbName.StartsWith("MikroDB_V12"))
				{
					list[j] = "BANKA_TCMB_KODLARI";
				}
				using (SqlCommand sqlCommand2 = sqlDB2.Connection.CreateCommand())
				{
					string commandText2 = "SELECT COUNT(" + list2[j] + ") FROM " + list[j] + " WITH(NOLOCK) WHERE " + list2[j] + ">" + list3[j];
					sqlCommand2.CommandText = commandText2;
					list4.Add(int.Parse(sqlCommand2.ExecuteScalar().ToString()));
				}
				sqlDB2.ConnectionClose();
			}
			Stream stream = new MemoryStream();
			BinaryWriter binaryWriter5 = new BinaryWriter(stream);
			binaryWriter5.Write(value: true);
			foreach (int item in list4)
			{
				binaryWriter5.Write(item);
			}
			binaryWriter5.Flush();
			stream.Position = 0L;
			return stream;
		}
		catch (Exception ex4)
		{
			Log_Error(ex4.ToString());
			_ = "Hata: " + ex4.ToString();
			string value5 = "Hata : " + ex4.ToString();
			MemoryStream memoryStream5 = new MemoryStream();
			BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream5);
			binaryWriter6.Write(value: false);
			binaryWriter6.Write(value5);
			binaryWriter6.Flush();
			memoryStream5.Position = 0L;
			return memoryStream5;
		}
	}

	public Stream GetDegisenKayitlarToplu(string firmaid, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		return GetDegisenKayitlarTopluV2(firmaid, mikroDbName, Content);
	}

	public Stream GetDegisenKayitlarTopluV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		int num = 1000;
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		num = binaryReader.ReadInt32();
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		List<string> list5 = new List<string>();
		List<string> list6 = new List<string>();
		try
		{
			int num2 = binaryReader.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				list.Add(binaryReader.ReadString());
				list2.Add(binaryReader.ReadInt32());
				list3.Add(binaryReader.ReadInt32());
				list4.Add(binaryReader.ReadInt32());
				list5.Add(binaryReader.ReadString());
				list6.Add(binaryReader.ReadString());
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		MemoryStream memoryStream5 = new MemoryStream();
		BinaryWriter binaryWriter5 = new BinaryWriter(memoryStream5);
		binaryWriter5.Write(value: true);
		for (int j = 0; j < list.Count; j++)
		{
			string text4 = "";
			try
			{
				SqlDB sqlDB2 = new SqlDB();
				DataTable dataTable = new DataTable();
				if (list[j] == "DOVIZ_KURLARI" || list[j] == "YEREL_BANKA_KODLARI" || list[j] == "KUR_ISIMLERI")
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName.Substring(0, MikroDbName.IndexOf("_", MikroDbName.IndexOf("_", 0) + 1)));
				}
				else
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
				}
				if (list[j] == "CARI_HESAPLAR" && MikroDbName.StartsWith("MikroDB_V15"))
				{
					list6[j] = list6[j].Replace("cari_tipi", "cari_baglanti_tipi");
				}
				if (list[j] == "YEREL_BANKA_KODLARI" && MikroDbName.StartsWith("MikroDB_V12"))
				{
					list[j] = "BANKA_TCMB_KODLARI";
				}
				text4 = "SELECT TOP 1 T." + list6[j].Replace(",", ",T.") + ",S.TriggerRECno  FROM " + list[j] + " AS T LEFT JOIN _FORA_SENKRONIZASYON AS S ON T." + list5[j] + "=S.KayitRECno  WHERE S.TriggerRECno>" + list3[j] + " AND S.TabloID=" + list2[j] + " AND S.Islem=1 AND S.KayitRECno<=" + list4[j] + "  ORDER BY S.TriggerRECno";
				if (list[j] == "DOVIZ_KURLARI" || list[j] == "YEREL_BANKA_KODLARI" || list[j] == "BANKA_TCMB_KODLARI" || list[j] == "KUR_ISIMLERI")
				{
					text4 = text4.Replace("_FORA_SENKRONIZASYON", "[" + MikroDbName + "].[dbo].[_FORA_SENKRONIZASYON]");
				}
				SqlCommand sqlCommand2 = new SqlCommand(text4, sqlDB2.Connection);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand2);
				sqlDataAdapter.Fill(dataTable);
				if (list[j] == "CARI_HESAPLAR" && MikroDbName.StartsWith("MikroDB_V15"))
				{
					dataTable.Columns["cari_baglanti_tipi"].ColumnName = "cari_tipi";
				}
				sqlDataAdapter.Dispose();
				sqlCommand2.Dispose();
				binaryWriter5.Write(dataTable.Columns.Count);
				foreach (DataColumn column in dataTable.Columns)
				{
					binaryWriter5.Write(column.ColumnName);
					binaryWriter5.Write(column.DataType.ToString());
				}
				text4 = "SELECT TOP " + num + " T." + list6[j].Replace(",", ",T.") + ",S.TriggerRECno  FROM " + list[j] + " AS T LEFT JOIN _FORA_SENKRONIZASYON AS S ON T." + list5[j] + "=S.KayitRECno  WHERE S.TriggerRECno>" + list3[j].ToString() + " AND S.TabloID=" + list2[j].ToString() + " AND S.Islem=1 AND S.KayitRECno<=" + list4[j].ToString() + "  ORDER BY S.TriggerRECno";
				if (list[j] == "DOVIZ_KURLARI" || list[j] == "YEREL_BANKA_KODLARI" || list[j] == "BANKA_TCMB_KODLARI" || list[j] == "KUR_ISIMLERI")
				{
					text4 = text4.Replace("_FORA_SENKRONIZASYON", "[" + MikroDbName + "].[dbo].[_FORA_SENKRONIZASYON]");
				}
				SqlDataReader r = new SqlCommand(text4, sqlDB2.Connection).ExecuteReader();
				SqlReaderStreamYaz(dataTable, binaryWriter5, r);
				sqlDB2.ConnectionClose();
			}
			catch (Exception ex4)
			{
				Log_Error(ex4.ToString());
				_ = "Hata: " + ex4.ToString() + " SQL : " + text4;
				string value5 = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
				MemoryStream memoryStream6 = new MemoryStream();
				BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream6);
				binaryWriter6.Write(value: false);
				binaryWriter6.Write(value5);
				binaryWriter6.Flush();
				memoryStream6.Position = 0L;
				return memoryStream6;
			}
		}
		return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream5.ToArray()));
	}

	public Stream V16_GetDegisenKayitlarTopluV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		int num = 5000;
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		num = binaryReader.ReadInt32();
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<string> list4 = new List<string>();
		List<string> list5 = new List<string>();
		try
		{
			int num2 = binaryReader.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				list.Add(binaryReader.ReadString());
				list2.Add(binaryReader.ReadInt32());
				list3.Add(binaryReader.ReadInt32());
				list4.Add(binaryReader.ReadString());
				list5.Add(binaryReader.ReadString());
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		MemoryStream memoryStream5 = new MemoryStream();
		BinaryWriter binaryWriter5 = new BinaryWriter(memoryStream5);
		binaryWriter5.Write(value: true);
		for (int j = 0; j < list.Count; j++)
		{
			string text4 = "";
			try
			{
				SqlDB sqlDB2 = new SqlDB();
				DataTable dataTable = new DataTable();
				if (list[j] == "DOVIZ_KURLARI" || list[j] == "YEREL_BANKA_KODLARI" || list[j] == "KUR_ISIMLERI")
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName.Substring(0, MikroDbName.IndexOf("_", MikroDbName.IndexOf("_", 0) + 1)));
				}
				else
				{
					sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
				}
				if (list[j] == "CARI_HESAPLAR")
				{
					list5[j] = list5[j].Replace("cari_tipi", "cari_baglanti_tipi");
				}
				text4 = "SELECT TOP 1 T." + list5[j].Replace(",", ",T.") + ",S.TriggerRECno  FROM " + list[j] + " AS T LEFT JOIN _FORA_SYNC AS S ON T." + list4[j] + "=S.KayitGuid  WHERE S.TriggerRECno>" + list3[j] + " AND S.TabloID=" + list2[j] + "  ORDER BY S.TriggerRECno";
				if (list[j] == "DOVIZ_KURLARI" || list[j] == "YEREL_BANKA_KODLARI" || list[j] == "BANKA_TCMB_KODLARI" || list[j] == "KUR_ISIMLERI")
				{
					text4 = text4.Replace("_FORA_SYNC", "[" + MikroDbName + "].[dbo].[_FORA_SYNC]");
				}
				SqlCommand sqlCommand2 = new SqlCommand(text4, sqlDB2.Connection);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand2);
				sqlDataAdapter.Fill(dataTable);
				if (list[j] == "CARI_HESAPLAR")
				{
					dataTable.Columns["cari_baglanti_tipi"].ColumnName = "cari_tipi";
				}
				sqlDataAdapter.Dispose();
				sqlCommand2.Dispose();
				binaryWriter5.Write(dataTable.Columns.Count);
				foreach (DataColumn column in dataTable.Columns)
				{
					binaryWriter5.Write(column.ColumnName);
					binaryWriter5.Write(column.DataType.ToString());
				}
				text4 = "SELECT TOP " + num + " T." + list5[j].Replace(",", ",T.") + ",S.TriggerRECno  FROM " + list[j] + " AS T LEFT JOIN _FORA_SYNC AS S ON T." + list4[j] + "=S.KayitGuid  WHERE S.TriggerRECno>" + list3[j].ToString() + " AND S.TabloID=" + list2[j].ToString() + "  ORDER BY S.TriggerRECno";
				if (list[j] == "DOVIZ_KURLARI" || list[j] == "YEREL_BANKA_KODLARI" || list[j] == "BANKA_TCMB_KODLARI" || list[j] == "KUR_ISIMLERI")
				{
					text4 = text4.Replace("_FORA_SYNC", "[" + MikroDbName + "].[dbo].[_FORA_SYNC]");
				}
				SqlDataReader r = new SqlCommand(text4, sqlDB2.Connection).ExecuteReader();
				SqlReaderStreamYaz(dataTable, binaryWriter5, r);
				sqlDB2.ConnectionClose();
			}
			catch (Exception ex4)
			{
				Log_Error(ex4.ToString());
				_ = "Hata: " + ex4.ToString() + " SQL : " + text4;
				string value5 = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
				MemoryStream memoryStream6 = new MemoryStream();
				BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream6);
				binaryWriter6.Write(value: false);
				binaryWriter6.Write(value5);
				binaryWriter6.Flush();
				memoryStream6.Position = 0L;
				return memoryStream6;
			}
		}
		return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream5.ToArray()));
	}

	public Stream GetSilinenKayitlarToplu(string firmaid, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		return GetSilinenKayitlarTopluV2(firmaid, mikroDbName, Content);
	}

	public Stream GetSilinenKayitlarTopluV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		binaryReader.ReadInt32();
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		try
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				list.Add(binaryReader.ReadInt32());
				list2.Add(binaryReader.ReadInt32());
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		MemoryStream memoryStream5 = new MemoryStream();
		BinaryWriter binaryWriter5 = new BinaryWriter(memoryStream5);
		binaryWriter5.Write(value: true);
		SqlDB sqlDB2 = new SqlDB();
		sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
		for (int j = 0; j < list.Count; j++)
		{
			try
			{
				DataTable dataTable = new DataTable();
				SqlCommand sqlCommand2 = new SqlCommand("SELECT KayitRECno,TriggerRECno FROM _FORA_SENKRONIZASYON WHERE TriggerRECno>" + list2[j] + " AND TabloID=" + list[j] + " AND Islem=0 ORDER BY TriggerRECno", sqlDB2.Connection);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand2);
				sqlDataAdapter.Fill(dataTable);
				sqlDataAdapter.Dispose();
				sqlCommand2.Dispose();
				binaryWriter5.Write(dataTable.Columns.Count);
				foreach (DataColumn column in dataTable.Columns)
				{
					binaryWriter5.Write(column.ColumnName);
					binaryWriter5.Write(column.DataType.ToString());
				}
				SqlDataReader r = new SqlCommand("SELECT KayitRECno,TriggerRECno FROM _FORA_SENKRONIZASYON WHERE TriggerRECno>" + list2[j] + " AND TabloID=" + list[j] + " AND Islem=0 ORDER BY TriggerRECno", sqlDB2.Connection).ExecuteReader();
				SqlReaderStreamYaz(dataTable, binaryWriter5, r);
			}
			catch (Exception ex4)
			{
				Log_Error(ex4.ToString());
				_ = "Hata: " + ex4.ToString();
				string value5 = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
				MemoryStream memoryStream6 = new MemoryStream();
				BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream6);
				binaryWriter6.Write(value: false);
				binaryWriter6.Write(value5);
				binaryWriter6.Flush();
				memoryStream6.Position = 0L;
				return memoryStream6;
			}
		}
		sqlDB2.ConnectionClose();
		return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream5.ToArray()));
	}

	public Stream V16_GetSilinenKayitlarTopluV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		binaryReader.ReadInt32();
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		try
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				list.Add(binaryReader.ReadInt32());
				list2.Add(binaryReader.ReadInt32());
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		MemoryStream memoryStream5 = new MemoryStream();
		BinaryWriter binaryWriter5 = new BinaryWriter(memoryStream5);
		binaryWriter5.Write(value: true);
		SqlDB sqlDB2 = new SqlDB();
		sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
		for (int j = 0; j < list.Count; j++)
		{
			try
			{
				DataTable dataTable = new DataTable();
				SqlCommand sqlCommand2 = new SqlCommand("SELECT KayitGuid,TriggerRECno FROM _FORA_SYNC_DEL WHERE TriggerRECno>" + list2[j] + " AND TabloID=" + list[j] + " ORDER BY TriggerRECno", sqlDB2.Connection);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand2);
				sqlDataAdapter.Fill(dataTable);
				sqlDataAdapter.Dispose();
				sqlCommand2.Dispose();
				binaryWriter5.Write(dataTable.Columns.Count);
				foreach (DataColumn column in dataTable.Columns)
				{
					binaryWriter5.Write(column.ColumnName);
					binaryWriter5.Write(column.DataType.ToString());
				}
				SqlDataReader r = new SqlCommand("SELECT KayitGuid,TriggerRECno FROM _FORA_SYNC_DEL WHERE TriggerRECno>" + list2[j] + " AND TabloID=" + list[j] + " ORDER BY TriggerRECno", sqlDB2.Connection).ExecuteReader();
				SqlReaderStreamYaz(dataTable, binaryWriter5, r);
			}
			catch (Exception ex4)
			{
				Log_Error(ex4.ToString());
				_ = "Hata: " + ex4.ToString();
				string value5 = "Sunucu servis bilgileri çekilemedi.";
				MemoryStream memoryStream6 = new MemoryStream();
				BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream6);
				binaryWriter6.Write(value: false);
				binaryWriter6.Write(value5);
				binaryWriter6.Flush();
				memoryStream6.Position = 0L;
				return memoryStream6;
			}
		}
		sqlDB2.ConnectionClose();
		return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream5.ToArray()));
	}

	public Stream Getmye_TextData(string firmaid, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servis bilgileri çekilemedi.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		return Getmye_TextDataV2(firmaid, mikroDbName, Content);
	}

	public Stream Getmye_TextDataV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Hata : " + ex2.ToString();
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		if (!flag)
		{
			string value3 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		MemoryStream memoryStream4 = new MemoryStream();
		BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
		binaryWriter4.Write(value: true);
		try
		{
			SqlDB sqlDB2 = new SqlDB();
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("TableID", typeof(int));
			dataTable.Columns.Add("RecID_DBCno", typeof(int));
			dataTable.Columns.Add("RecID_RECno", typeof(int));
			dataTable.Columns.Add("Data", typeof(string));
			binaryWriter4.Write(dataTable.Columns.Count);
			foreach (DataColumn column in dataTable.Columns)
			{
				binaryWriter4.Write(column.ColumnName);
				binaryWriter4.Write(column.DataType.ToString());
			}
			sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
			SqlDataReader r = new SqlCommand("SELECT TableID,RecID_DBCno,RecID_RECno,Data FROM mye_TextData WITH (NOLOCK)", sqlDB2.Connection).ExecuteReader();
			SqlReaderStreamYaz(dataTable, binaryWriter4, r);
			sqlDB2.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value4 = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
			MemoryStream memoryStream5 = new MemoryStream();
			BinaryWriter binaryWriter5 = new BinaryWriter(memoryStream5);
			binaryWriter5.Write(value: false);
			binaryWriter5.Write(value4);
			binaryWriter5.Flush();
			memoryStream5.Position = 0L;
			return memoryStream5;
		}
		return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream4.ToArray()));
	}

	public Stream V16_Getmye_TextDataV2(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Hata : " + ex2.ToString();
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		if (!flag)
		{
			string value3 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		MemoryStream memoryStream4 = new MemoryStream();
		BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
		binaryWriter4.Write(value: true);
		try
		{
			SqlDB sqlDB2 = new SqlDB();
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("TableID", typeof(int));
			dataTable.Columns.Add("Record_uid", typeof(Guid));
			dataTable.Columns.Add("Data", typeof(string));
			binaryWriter4.Write(dataTable.Columns.Count);
			foreach (DataColumn column in dataTable.Columns)
			{
				binaryWriter4.Write(column.ColumnName);
				binaryWriter4.Write(column.DataType.ToString());
			}
			sqlDB2.ConnectionOpen(baglantiBilgileri, MikroDbName);
			SqlDataReader r = new SqlCommand("SELECT TableID,Record_uid,Data FROM mye_TextData WITH (NOLOCK)", sqlDB2.Connection).ExecuteReader();
			SqlReaderStreamYaz(dataTable, binaryWriter4, r);
			sqlDB2.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value4 = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
			MemoryStream memoryStream5 = new MemoryStream();
			BinaryWriter binaryWriter5 = new BinaryWriter(memoryStream5);
			binaryWriter5.Write(value: false);
			binaryWriter5.Write(value4);
			binaryWriter5.Flush();
			memoryStream5.Position = 0L;
			return memoryStream5;
		}
		return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream4.ToArray()));
	}

	public Message GetRecordCountV2(string firmaid, string tablo_adi, string tablo_id_field, string last_rec_no)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetRecordCountV3(firmaid, tablo_adi, tablo_id_field, last_rec_no, mikroDbName);
	}

	public Message GetRecordCountV3(string firmaid, string tablo_adi, string tablo_id_field, string last_rec_no, string MikroDbName)
	{
		int num = 0;
		firmaid = HttpUtility.UrlDecode(firmaid);
		tablo_adi = HttpUtility.UrlDecode(tablo_adi);
		tablo_id_field = HttpUtility.UrlDecode(tablo_id_field);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			SqlDB sqlDB = new SqlDB();
			switch (tablo_adi)
			{
			case "DOVIZ_KURLARI":
			case "YEREL_BANKA_KODLARI":
			case "BANKA_TCMB_KODLARI":
			case "KUR_ISIMLERI":
				sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName.Substring(0, MikroDbName.IndexOf("_", MikroDbName.IndexOf("_", 0) + 1)));
				break;
			default:
				sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
				break;
			}
			if (tablo_adi == "YEREL_BANKA_KODLARI" && MikroDbName.StartsWith("MikroDB_V12"))
			{
				tablo_adi = "BANKA_TCMB_KODLARI";
			}
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT COUNT(*) FROM " + tablo_adi + " WITH(NOLOCK) WHERE " + tablo_id_field + "<=" + last_rec_no;
				sqlCommand.CommandText = commandText;
				num = int.Parse(sqlCommand.ExecuteScalar().ToString());
			}
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse(num.ToString(), "application/json;charset=utf-8", Encoding.UTF8);
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string text2 = JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
		}
	}

	public Message V16_GetRecordCountV3(string firmaid, string tablo_adi, string MikroDbName)
	{
		int num = 0;
		firmaid = HttpUtility.UrlDecode(firmaid);
		tablo_adi = HttpUtility.UrlDecode(tablo_adi);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			SqlDB sqlDB = new SqlDB();
			switch (tablo_adi)
			{
			case "DOVIZ_KURLARI":
			case "YEREL_BANKA_KODLARI":
			case "BANKA_TCMB_KODLARI":
			case "KUR_ISIMLERI":
				sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName.Substring(0, MikroDbName.IndexOf("_", MikroDbName.IndexOf("_", 0) + 1)));
				break;
			default:
				sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
				break;
			}
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT COUNT(*) FROM " + tablo_adi + " WITH(NOLOCK)";
				sqlCommand.CommandText = commandText;
				num = int.Parse(sqlCommand.ExecuteScalar().ToString());
			}
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse(num.ToString(), "application/json;charset=utf-8", Encoding.UTF8);
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string text2 = JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
		}
	}

	public Stream GetStokFotoListV3(string firmaid)
	{
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			if (!Directory.Exists(text + "data\\stoklar"))
			{
				Directory.CreateDirectory(text + "data\\stoklar");
			}
			if (!Directory.Exists(text + "data\\stoklar\\" + firmaid))
			{
				Directory.CreateDirectory(text + "data\\stoklar\\" + firmaid);
			}
			Stream stream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			string[] files = Directory.GetFiles(text + "data\\stoklar\\" + firmaid);
			binaryWriter.Write(files.Length);
			string[] array = files;
			foreach (string text2 in array)
			{
				binaryWriter.Write(text2.Replace(text + "data\\stoklar\\" + firmaid + "\\", ""));
				binaryWriter.Write(File.GetLastWriteTime(text2).Ticks);
			}
			binaryWriter.Flush();
			stream.Position = 0L;
			return stream;
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetStokFoto(string firmaid, string filename)
	{
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		return File.OpenRead(text + "data\\stoklar\\" + firmaid + "\\" + filename);
	}

	public Stream SaveOfflineEvrakV2(string firmaid, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servis bilgileri çekilemedi. Evrak yazılamadı.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		return SaveOfflineEvrakV3(firmaid, mikroDbName, Content);
	}

	public Stream SaveOfflineEvrakV3(string firmaid, string MikroDbName, Stream Content)
	{
		string text = "";
		string text2 = "";
		BinaryReader binaryReader = new BinaryReader(Content);
		text = binaryReader.ReadString();
		text2 = binaryReader.ReadString();
		OfflineEvrakV2 offlineEvrakV;
		try
		{
			offlineEvrakV = OfflineEvrakV2.ReadFromBinaryReader(binaryReader);
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			string value = "Sunucu servisi eski. Evrak yazılamadı.";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value: false);
			binaryWriter.Write(value);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		Log_Evrak(text, offlineEvrakV);
		string text3 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text3 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string value2 = "Sunucu bağlantı bilgileri okunamadı.";
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			binaryWriter2.Write(value: false);
			binaryWriter2.Write(value2);
			binaryWriter2.Flush();
			memoryStream2.Position = 0L;
			return memoryStream2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", text2);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string value3 = "Hata : " + ex3.ToString();
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(value: false);
			binaryWriter3.Write(value3);
			binaryWriter3.Flush();
			memoryStream3.Position = 0L;
			return memoryStream3;
		}
		if (!flag)
		{
			string value4 = "Kullanıcı girişi başarısız.";
			MemoryStream memoryStream4 = new MemoryStream();
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
			binaryWriter4.Write(value: false);
			binaryWriter4.Write(value4);
			binaryWriter4.Flush();
			memoryStream4.Position = 0L;
			return memoryStream4;
		}
		try
		{
			try
			{
				SqlDB sqlDB2 = new SqlDB();
				sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
				switch (offlineEvrakV.Tipi)
				{
				case enum_AndroidAktarimTipi.CariLokasyon:
					offlineEvrakV = CariLokasyonGuncelleV2(offlineEvrakV, sqlDB2.Connection);
					break;
				case enum_AndroidAktarimTipi.GunAcilisi:
					offlineEvrakV = GunAcilisiYapV2(offlineEvrakV, sqlDB2.Connection);
					break;
				case enum_AndroidAktarimTipi.GunKapanisi:
					offlineEvrakV = GunKapanisiYapV2(offlineEvrakV, sqlDB2.Connection);
					break;
				case enum_AndroidAktarimTipi.Ziyaret:
					offlineEvrakV = ZiyaretKaydetV2(offlineEvrakV, sqlDB2.Connection, MikroDbName);
					break;
				case enum_AndroidAktarimTipi.Evrak:
					offlineEvrakV = EvrakKaydetV2(offlineEvrakV, sqlBaglantiBilgileri, MikroDbName, text);
					break;
				case enum_AndroidAktarimTipi.YazBozTahtasi:
					offlineEvrakV = CariYazBozGuncelle(offlineEvrakV, sqlDB2.Connection);
					break;
				case enum_AndroidAktarimTipi.YeniCariOlusturma:
				{
					Parametreler parametreler = ParametrelerDefault.MobilKullanici(text);
					ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", text, "", "");
					offlineEvrakV = YeniCariOlustur(offlineEvrakV, sqlDB2.Connection, MikroDbName, parametreler._GetParametre("MikroUserNo")._GetInt);
					break;
				}
				}
				sqlDB2.ConnectionClose();
			}
			catch (Exception ex4)
			{
				Log_Error(ex4.ToString());
				_ = "Hata: " + ex4.ToString();
				offlineEvrakV.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
				string value5 = "Servis hatası. Hata : " + ex4.ToString();
				MemoryStream memoryStream5 = new MemoryStream();
				BinaryWriter binaryWriter5 = new BinaryWriter(memoryStream5);
				binaryWriter5.Write(value: false);
				binaryWriter5.Write(value5);
				binaryWriter5.Flush();
				memoryStream5.Position = 0L;
				return memoryStream5;
			}
			MemoryStream memoryStream6 = new MemoryStream();
			BinaryWriter binaryWriter6 = new BinaryWriter(memoryStream6);
			binaryWriter6.Write(value: true);
			OfflineEvrakV2.WriteToBinaryWriter(offlineEvrakV, binaryWriter6, 1);
			binaryWriter6.Flush();
			memoryStream6.Position = 0L;
			return memoryStream6;
		}
		catch (Exception ex5)
		{
			Log_Error(ex5.ToString());
			_ = "Hata: " + ex5.ToString();
			string value6 = "Servis hatası. Hata : " + ex5.ToString();
			MemoryStream memoryStream7 = new MemoryStream();
			BinaryWriter binaryWriter7 = new BinaryWriter(memoryStream7);
			binaryWriter7.Write(value: false);
			binaryWriter7.Write(value6);
			binaryWriter7.Flush();
			memoryStream7.Position = 0L;
			return memoryStream7;
		}
	}

	public Message SendCariEkstre(string firmaid, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return SendCariEkstreV2(firmaid, mikroDbName, Content);
	}

	public Message SendCariEkstreV2(string firmaid, string MikroDbName, Stream Content)
	{
		List<string> list = JsonConvert.DeserializeObject<List<string>>(new StreamReader(Content).ReadToEnd());
		string text = list[0];
		string value = list[1];
		string text2 = list[2];
		string subject = list[3];
		string text3 = list[4];
		string text4 = list[5];
		string value2 = list[6];
		text3 += "\n\n\n Bu E-posta Fora Mikro programı ile mobil olarak gönderilmiştir.";
		string text5 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text5 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", text);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", value);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string text6 = JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text6, "application/json;charset=utf-8", Encoding.UTF8);
		}
		if (!flag)
		{
			string text6 = JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text6, "application/json;charset=utf-8", Encoding.UTF8);
		}
		Parametreler parametreler = ParametrelerDefault.MobilKullanici(text);
		ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", text, "", "");
		string getString = parametreler._GetParametre("EMailSmtpServer")._GetString;
		string getString2 = parametreler._GetParametre("EMailAdress")._GetString;
		string text7 = parametreler._GetParametre("EMailGorunenAd")._GetString;
		string getString3 = parametreler._GetParametre("CariEkstreBCC")._GetString;
		int getInt = parametreler._GetParametre("EMailSmtpPort")._GetInt;
		string getString4 = parametreler._GetParametre("EMailSmtpPassword")._GetString;
		bool getBoolean = parametreler._GetParametre("EMailSmtpUseSSL")._GetBoolean;
		string text8 = parametreler._GetParametre("CariEkstreDosyaAdi")._GetString;
		if (getString == "")
		{
			string text6 = JsonConvert.SerializeObject("Hata : Smtp sunucu bilgisi ayarlanmamış durumda.", Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text6, "application/json;charset=utf-8", Encoding.UTF8);
		}
		if (getString2 == "")
		{
			string text6 = JsonConvert.SerializeObject("Hata : Gönderen e-posta adres bilgisi ayarlanmamış durumda.", Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text6, "application/json;charset=utf-8", Encoding.UTF8);
		}
		if (text7 == "")
		{
			text7 = getString2;
		}
		if (text8 == "")
		{
			text8 = "cariekstre";
		}
		try
		{
			CariEkstreButun item = JsonConvert.DeserializeObject<CariEkstreButun>(value2);
			RaporEkstre raporEkstre = new RaporEkstre();
			List<CariEkstreButun> list2 = new List<CariEkstreButun>();
			list2.Add(item);
			raporEkstre.DataSource = list2;
			try
			{
				if (File.Exists(text5 + "data\\cari_ekstre.repx"))
				{
					raporEkstre.LoadLayout(text5 + "data\\cari_ekstre.repx");
				}
			}
			catch (Exception ex3)
			{
				Log_Error(ex3.ToString());
				_ = "Hata: " + ex3.ToString();
			}
			string text6;
			try
			{
				MailMessage mailMessage = new MailMessage();
				SmtpClient smtpClient = new SmtpClient(getString);
				mailMessage.From = new MailAddress(getString2, text7, Encoding.UTF8);
				string[] array = text2.Split(';');
				foreach (string text9 in array)
				{
					if (text9 != "")
					{
						mailMessage.To.Add(text9);
					}
				}
				array = getString3.Split(';');
				foreach (string text10 in array)
				{
					if (text10 != "")
					{
						mailMessage.Bcc.Add(text10);
					}
				}
				mailMessage.Subject = subject;
				mailMessage.Body = text3;
				smtpClient.Port = getInt;
				smtpClient.Credentials = new NetworkCredential(getString2, getString4);
				if (getBoolean)
				{
					smtpClient.EnableSsl = true;
				}
				else
				{
					smtpClient.EnableSsl = false;
				}
				string text11 = "";
				MemoryStream memoryStream = new MemoryStream();
				switch (text4)
				{
				case "csv":
					raporEkstre.ExportOptions.Csv.Encoding = Encoding.UTF8;
					raporEkstre.ExportToCsv(memoryStream);
					text11 = "csv";
					break;
				case "jpg":
					raporEkstre.ExportOptions.Image.ExportMode = ImageExportMode.SingleFile;
					raporEkstre.ExportToImage(memoryStream);
					text11 = "jpg";
					break;
				case "rtf":
					raporEkstre.ExportOptions.Rtf.ExportMode = RtfExportMode.SingleFile;
					raporEkstre.ExportToRtf(memoryStream);
					text11 = "rtf";
					break;
				case "pdf":
					raporEkstre.ExportOptions.Pdf.Compressed = true;
					raporEkstre.ExportOptions.Pdf.ConvertImagesToJpeg = true;
					raporEkstre.ExportOptions.Pdf.ImageQuality = PdfJpegImageQuality.High;
					raporEkstre.ExportToPdf(memoryStream);
					text11 = "pdf";
					break;
				case "xls":
					raporEkstre.ExportOptions.Xls.ExportMode = XlsExportMode.SingleFile;
					raporEkstre.ExportOptions.Xls.SheetName = text8;
					raporEkstre.ExportToXls(memoryStream);
					text11 = "xls";
					break;
				case "txt":
					raporEkstre.ExportOptions.Text.Encoding = Encoding.UTF8;
					raporEkstre.ExportToText(memoryStream);
					text11 = "txt";
					break;
				}
				memoryStream.Seek(0L, SeekOrigin.Begin);
				Attachment item2 = new Attachment(memoryStream, text8 + "." + text11, "application/" + text11);
				mailMessage.Attachments.Add(item2);
				smtpClient.Send(mailMessage);
			}
			catch (Exception ex4)
			{
				Log_Error(ex4.ToString());
				_ = "Hata: " + ex4.ToString();
				text6 = JsonConvert.SerializeObject("Hata : Oluşturulamadı", Newtonsoft.Json.Formatting.None);
				return WebOperationContext.Current.CreateTextResponse(text6, "application/json;charset=utf-8", Encoding.UTF8);
			}
			text6 = JsonConvert.SerializeObject("E-posta gönderildi.", Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text6, "application/json;charset=utf-8", Encoding.UTF8);
		}
		catch (Exception ex5)
		{
			Log_Error(ex5.ToString());
			_ = "Hata: " + ex5.ToString();
			string text6 = JsonConvert.SerializeObject("Hata : " + ex5.Message);
			return WebOperationContext.Current.CreateTextResponse(text6, "application/json;charset=utf-8", Encoding.UTF8);
		}
	}

	public Message SaveLog(string firmaid, string kullanici_adi, string sifre, string etkinlik_adi)
	{
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		etkinlik_adi = HttpUtility.UrlDecode(etkinlik_adi);
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			return WebOperationContext.Current.CreateTextResponse("false", "application/json;charset=utf-8", Encoding.UTF8);
		}
		if (!flag)
		{
			return WebOperationContext.Current.CreateTextResponse("false", "application/json;charset=utf-8", Encoding.UTF8);
		}
		try
		{
			SqlDB sqlDB2 = new SqlDB();
			sqlDB2.ConnectionOpen(baglantiBilgileri, dBName);
			string commandText2 = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_FORA_MOBIL_ETKINLIK_LOG]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[_FORA_MOBIL_ETKINLIK_LOG]([ID] [int] IDENTITY(1,1) NOT NULL,[Kullanici] [nvarchar](50) NOT NULL,[EtkinlikAdi] [nvarchar](200) NOT NULL,[Tarih] [datetime] NOT NULL,CONSTRAINT [PK__FORA_MOBIL_ETKINLIK_LOG] PRIMARY KEY CLUSTERED ([ID] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY] CREATE NONCLUSTERED INDEX [01] ON [dbo].[_FORA_MOBIL_ETKINLIK_LOG] ([Kullanici] ASC,[Tarih] DESC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]  END";
			using (SqlCommand sqlCommand2 = sqlDB2.Connection.CreateCommand())
			{
				sqlCommand2.CommandText = commandText2;
				sqlCommand2.ExecuteNonQuery();
			}
			string commandText3 = "INSERT INTO _FORA_MOBIL_ETKINLIK_LOG (Kullanici,EtkinlikAdi,Tarih) VALUES (@Kullanici,@EtkinlikAdi,@Tarih)";
			using (SqlCommand sqlCommand3 = sqlDB2.Connection.CreateCommand())
			{
				sqlCommand3.CommandText = commandText3;
				sqlCommand3.Parameters.AddWithValue("@Kullanici", kullanici_adi);
				sqlCommand3.Parameters.AddWithValue("@EtkinlikAdi", etkinlik_adi);
				sqlCommand3.Parameters.AddWithValue("@Tarih", DateTime.Now);
				sqlCommand3.ExecuteNonQuery();
			}
			sqlDB2.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("true", "application/json;charset=utf-8", Encoding.UTF8);
		}
		catch (Exception ex4)
		{
			Log_Error(ex4.ToString());
			_ = "Hata: " + ex4.ToString();
			return WebOperationContext.Current.CreateTextResponse("false", "application/json;charset=utf-8", Encoding.UTF8);
		}
	}

	public Stream GetSiparisOnaylamaListItems(string firmaid, string kullanici_adi, string sifre, string CariListelemeSecenek, string CariListelemeAktifGrup, string TemsilciKodu)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetSiparisOnaylamaListItemsV2(firmaid, kullanici_adi, sifre, CariListelemeSecenek, CariListelemeAktifGrup, TemsilciKodu, mikroDbName);
	}

	public Stream GetSiparisOnaylamaListItemsV2(string firmaid, string kullanici_adi, string sifre, string CariListelemeSecenek, string CariListelemeAktifGrup, string TemsilciKodu, string MikroDbName)
	{
		List<SiparisOnaylamaListItem> list = new List<SiparisOnaylamaListItem>();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		CariListelemeAktifGrup = CariListelemeAktifGrup.Substring(1, CariListelemeAktifGrup.Length - 1);
		TemsilciKodu = TemsilciKodu.Substring(1, TemsilciKodu.Length - 1);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.")));
		}
		try
		{
			Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
			ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
			SqlDB sqlDB2 = new SqlDB();
			sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			int getInt = parametreler._GetParametre("SiparisOnaylamaMaliyetHesaplamaSekli")._GetInt;
			list = SiparislerData.GetSiparisOnaylamaListItem(sqlDB2.Connection, (enum_CariListelemeSecenekleri)int.Parse(CariListelemeSecenek), CariListelemeAktifGrup, TemsilciKodu, getInt);
			foreach (SiparisOnaylamaListItem item in list)
			{
				Cari cariByCariKod = CariData.GetCariByCariKod(sqlDB2.Connection, item.cari_kodu, AdreslerTemsilciyeGore: false, "");
				item.cari_bakiyesi = CariData.GetBakiye(sqlDB2.Connection, item.cari_kodu, -1, -1, -1, SorumlulukMerkeziDetayli: false, "");
				item.toplam_teminat_tutari = CariData.GetTeminatTutari(sqlDB2.Connection, item.cari_kodu, -1, SorumlulukMerkeziDetayli: false, "", 0).GetToplamTeminat();
				double num = CariData.GetFaturalasmamisIrsaliyeTutari(sqlDB2.Connection, cariByCariKod, item.cari_kodu, -1, -1, -1, SorumlulukMerkeziDetayli: false, "") / 100.0 * parametreler._GetParametre("risk_hesabi_irsaliye_yuzdesi")._GetDouble;
				double num2 = CariData.GetKarsilanmamisSiparisTutari(sqlDB2.Connection, item.cari_kodu, -1, -1, -1, SorumlulukMerkeziDetayli: false, "") / 100.0 * parametreler._GetParametre("risk_hesabi_siparis_yuzdesi")._GetDouble;
				double num3 = CariData.GetOdenmemisCekTutari(sqlDB2.Connection, cariByCariKod, -10, enum_sck_imza.Tumu, -1, -1, -1, SorumlulukMerkeziDetayli: false, "") / 100.0 * parametreler._GetParametre("risk_hesabi_kendi_ceki_yuzdesi")._GetDouble;
				item.toplam_risk_tutari = item.cari_bakiyesi + num + num2 + num3;
				item.kalan_kredisi = item.toplam_teminat_tutari - item.toplam_risk_tutari;
			}
			sqlDB2.ConnectionClose();
			Stream stream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(list.Count);
			foreach (SiparisOnaylamaListItem item2 in list)
			{
				SiparisOnaylamaListItem.WriteToBinaryWriter(item2, binaryWriter, 2);
			}
			binaryWriter.Flush();
			stream.Position = 0L;
			return stream;
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex3.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Message GetSiparisOnaylamaOnayla(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			string text2 = "Hata: " + ex.ToString();
			return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
		}
		return GetSiparisOnaylamaOnaylaV2(firmaid, kullanici_adi, sifre, evrak_seri, evrak_sira, mikroDbName);
	}

	public Message GetSiparisOnaylamaOnaylaV2(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira, string MikroDbName)
	{
		string text = "false";
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		evrak_seri = evrak_seri.Substring(1, evrak_seri.Length - 1);
		string text2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text2 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			string text3 = "Hata: " + ex.ToString();
			return WebOperationContext.Current.CreateTextResponse(text3, "application/json;charset=utf-8", Encoding.UTF8);
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			string text4 = "Hata: " + ex2.ToString();
			return WebOperationContext.Current.CreateTextResponse(text4, "application/json;charset=utf-8", Encoding.UTF8);
		}
		if (!flag)
		{
			text = JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.");
			return WebOperationContext.Current.CreateTextResponse(text, "application/json;charset=utf-8", Encoding.UTF8);
		}
		try
		{
			Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
			ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
			SqlDB sqlDB2 = new SqlDB();
			sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			SiparislerData.GetSiparisOnaylamaOnayla(sqlDB2.Connection, evrak_seri, int.Parse(evrak_sira), parametreler._GetParametre("MikroUserNo")._GetInt);
			sqlDB2.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("true", "application/json;charset=utf-8", Encoding.UTF8);
		}
		catch (Exception ex3)
		{
			text = JsonConvert.SerializeObject("Hata : " + ex3.ToString());
			return WebOperationContext.Current.CreateTextResponse(text, "application/json;charset=utf-8", Encoding.UTF8);
		}
	}

	public Message GetSiparisOnaylamaReddet(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira, string kapama_kodu)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			string text2 = "Hata: " + ex.ToString();
			return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
		}
		return GetSiparisOnaylamaReddetV2(firmaid, kullanici_adi, sifre, evrak_seri, evrak_sira, kapama_kodu, mikroDbName);
	}

	public Message GetSiparisOnaylamaReddetV2(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira, string kapama_kodu, string MikroDbName)
	{
		string text = "false";
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		evrak_seri = evrak_seri.Substring(1, evrak_seri.Length - 1);
		string text2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text2 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			string text3 = "Hata: " + ex.ToString();
			return WebOperationContext.Current.CreateTextResponse(text3, "application/json;charset=utf-8", Encoding.UTF8);
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			string text4 = "Hata: " + ex2.ToString();
			return WebOperationContext.Current.CreateTextResponse(text4, "application/json;charset=utf-8", Encoding.UTF8);
		}
		if (!flag)
		{
			text = JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.");
			return WebOperationContext.Current.CreateTextResponse(text, "application/json;charset=utf-8", Encoding.UTF8);
		}
		try
		{
			Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
			ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
			SqlDB sqlDB2 = new SqlDB();
			sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			SiparislerData.GetSiparisOnaylamareddet(sqlDB2.Connection, evrak_seri, int.Parse(evrak_sira), parametreler._GetParametre("MikroUserNo")._GetInt, kapama_kodu);
			sqlDB2.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("true", "application/json;charset=utf-8", Encoding.UTF8);
		}
		catch (Exception ex3)
		{
			text = JsonConvert.SerializeObject("Hata : " + ex3.ToString());
			return WebOperationContext.Current.CreateTextResponse(text, "application/json;charset=utf-8", Encoding.UTF8);
		}
	}

	public Stream GetDepolarArasiSiparisEvrakListV3(string firmaid, string kullanici_adi, string sifre, string kaynak_depo_no, string hedef_depo_no, string siralama)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetDepolarArasiSiparisEvrakListV4(firmaid, kullanici_adi, sifre, kaynak_depo_no, hedef_depo_no, siralama, mikroDbName);
	}

	public Stream GetDepolarArasiSiparisEvrakListV4(string firmaid, string kullanici_adi, string sifre, string kaynak_depo_no, string hedef_depo_no, string siralama, string MikroDbName)
	{
		List<SiparisEvrakListItem> list = new List<SiparisEvrakListItem>();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.")));
		}
		try
		{
			list = DepolarArasiSiparislerData.GetDepolarArasiSiparisEvrakList(baglantiBilgileri, MikroDbName, "", int.Parse(kaynak_depo_no), int.Parse(hedef_depo_no), (enum_sip_orderby)int.Parse(siralama));
			Stream stream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(list.Count);
			foreach (SiparisEvrakListItem item in list)
			{
				SiparisEvrakListItem.WriteToStream(item, binaryWriter);
			}
			binaryWriter.Flush();
			stream.Position = 0L;
			return stream;
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex3.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Message StokAmbarAdresiDegistir(string firmaid, string kullanici_adi, string sifre, string stok_kodu, string ambaradresi)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return StokAmbarAdresiDegistirV2(firmaid, kullanici_adi, sifre, stok_kodu, ambaradresi, mikroDbName);
	}

	public Message StokAmbarAdresiDegistirV2(string firmaid, string kullanici_adi, string sifre, string stok_kodu, string ambaradresi, string MikroDbName)
	{
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			string text2 = JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
		}
		if (!flag)
		{
			string text2 = JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
		}
		try
		{
			Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
			ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
			int getInt = parametreler._GetParametre("MikroUserNo")._GetInt;
			string text2 = ((!StokData.StokAmbarAdresiDegistir(sqlBaglantiBilgileri, MikroDbName, getInt, stok_kodu, ambaradresi)) ? JsonConvert.SerializeObject("OK", Newtonsoft.Json.Formatting.None) : JsonConvert.SerializeObject("Hata : Bilgiler kayıt edilemedi", Newtonsoft.Json.Formatting.None));
			return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			string text2 = JsonConvert.SerializeObject("Hata : " + ex3.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None);
			return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
		}
	}

	public Stream GetNakliyedekiUrunlerV2(string firmaid, string kullanici_adi, string sifre, string nakliye_depo_no, string hedef_depo_no)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetNakliyedekiUrunlerV3(firmaid, kullanici_adi, sifre, nakliye_depo_no, hedef_depo_no, mikroDbName);
	}

	public Stream GetNakliyedekiUrunlerV3(string firmaid, string kullanici_adi, string sifre, string nakliye_depo_no, string hedef_depo_no, string MikroDbName)
	{
		List<STOK_HAREKETLERI> list = new List<STOK_HAREKETLERI>();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.")));
		}
		try
		{
			list = StokHareketleriData.GetNakliyedekiUrunler(baglantiBilgileri, MikroDbName, int.Parse(nakliye_depo_no), int.Parse(hedef_depo_no));
			Stream stream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(list.Count);
			foreach (STOK_HAREKETLERI item in list)
			{
				STOK_HAREKETLERI.WriteToBinaryWriter(item, binaryWriter, 1);
			}
			foreach (STOK_HAREKETLERI item2 in list)
			{
				binaryWriter.Write(item2.sth_Guid.ToByteArray());
				binaryWriter.Write(item2.sth_fat_uid.ToByteArray());
				binaryWriter.Write(item2.sth_sip_uid.ToByteArray());
				binaryWriter.Write(item2.sth_kons_uid.ToByteArray());
				binaryWriter.Write(item2.sth_subesip_uid.ToByteArray());
				binaryWriter.Write(item2.sth_yetkili_uid.ToByteArray());
			}
			binaryWriter.Flush();
			stream.Position = 0L;
			return stream;
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex3.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetDepolarArasiSiparisEvrakV2(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetDepolarArasiSiparisEvrakV3(firmaid, kullanici_adi, sifre, evrak_seri, evrak_sira, mikroDbName);
	}

	public Stream GetDepolarArasiSiparisEvrakV3(string firmaid, string kullanici_adi, string sifre, string evrak_seri, string evrak_sira, string MikroDbName)
	{
		Evrak evrak = new Evrak();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		evrak_seri = evrak_seri.Substring(1, evrak_seri.Length - 1);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.")));
		}
		Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
		ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
		try
		{
			SqlDB sqlDB2 = new SqlDB();
			sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			evrak = EvrakData.GetEvrak(sqlDB2.Connection, evrak_seri, int.Parse(evrak_sira), enum_GenelEvrakTipleri.DepolarArasiSiparis, parametreler._GetParametre("AlternatifDovizCinsi")._GetInt, enum_sip_cins.NormalSiparis, enum_sth_cins.Toptan);
			sqlDB2.ConnectionClose();
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			Evrak.WriteToBinaryWriter(evrak, binaryWriter, 999);
			binaryWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex3.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetGenelEvrakRapor(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string evraktipi, string evrakseri, string evraksira)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetGenelEvrakRaporV2(firmaid, kullanici_adi, sifre, format, dosyaismi, evraktipi, evrakseri, evraksira, mikroDbName);
	}

	public Stream GetGenelEvrakRaporV2(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string evraktipi, string evrakseri, string evraksira, string MikroDbName)
	{
		new DataTable();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		evrakseri = evrakseri.Remove(0, 1);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None)));
		}
		Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
		ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
		Evrak evrak = new Evrak();
		SqlDB sqlDB2 = new SqlDB();
		sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
		enum_GenelEvrakTipleri enum_GenelEvrakTipleri = (enum_GenelEvrakTipleri)int.Parse(evraktipi);
		enum_sth_cins sth_cins = enum_sth_cins.Toptan;
		switch (enum_GenelEvrakTipleri)
		{
		case enum_GenelEvrakTipleri.DepolarArasiSevk:
			sth_cins = enum_sth_cins.Transfer;
			break;
		case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
			sth_cins = enum_sth_cins.Transfer;
			break;
		}
		try
		{
			evrak = EvrakData.GetEvrak(sqlDB2.Connection, evrakseri, int.Parse(evraksira), enum_GenelEvrakTipleri, parametreler._GetParametre("AlternatifDovizCinsi")._GetInt, enum_sip_cins.NormalSiparis, sth_cins);
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: Evrak oluşturulamadı. " + ex3.ToString();
		}
		GenelEvrak item = new GenelEvrak();
		try
		{
			item = GenelEvrakData.ConvertEvrak(sqlDB2.Connection, evrak, enum_satir_gruplandirma_secenekleri.StokKodu);
		}
		catch (Exception ex4)
		{
			Log_Error(ex4.ToString());
			_ = "Hata: Genel evrak oluşturulamadı. " + ex4.ToString();
		}
		sqlDB2.ConnectionClose();
		RaporGenelEvrak raporGenelEvrak = new RaporGenelEvrak();
		List<GenelEvrak> list = new List<GenelEvrak>();
		list.Add(item);
		raporGenelEvrak.DataSource = list;
		try
		{
			if (File.Exists(text + "data\\formlar\\" + dosyaismi))
			{
				raporGenelEvrak.LoadLayout(text + "data\\formlar\\" + dosyaismi);
			}
		}
		catch (Exception ex5)
		{
			Log_Error(ex5.ToString());
			_ = "Hata: " + ex5.ToString();
		}
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			switch (format)
			{
			case "csv":
				raporGenelEvrak.ExportOptions.Csv.Encoding = Encoding.UTF8;
				raporGenelEvrak.ExportToCsv(memoryStream);
				break;
			case "jpg":
				raporGenelEvrak.ExportOptions.Image.ExportMode = ImageExportMode.SingleFile;
				raporGenelEvrak.ExportToImage(memoryStream);
				break;
			case "rtf":
				raporGenelEvrak.ExportOptions.Rtf.ExportMode = RtfExportMode.SingleFile;
				raporGenelEvrak.ExportToRtf(memoryStream);
				break;
			case "pdf":
				raporGenelEvrak.ExportOptions.Pdf.Compressed = true;
				raporGenelEvrak.ExportOptions.Pdf.ConvertImagesToJpeg = true;
				raporGenelEvrak.ExportOptions.Pdf.ImageQuality = PdfJpegImageQuality.High;
				raporGenelEvrak.ExportToPdf(memoryStream);
				break;
			case "xls":
				raporGenelEvrak.ExportOptions.Xls.ExportMode = XlsExportMode.SingleFile;
				raporGenelEvrak.ExportToXls(memoryStream);
				break;
			case "txt":
				raporGenelEvrak.ExportOptions.Text.Encoding = Encoding.UTF8;
				raporGenelEvrak.ExportToText(memoryStream);
				break;
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream;
		}
		catch (Exception ex6)
		{
			Log_Error(ex6.ToString());
			_ = "Hata: " + ex6.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex6.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetEvrak(string firmaid, string kullanici_adi, string sifre, string evraktipi, string evrakseri, string evraksira)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetEvrakV2(firmaid, kullanici_adi, sifre, evraktipi, evrakseri, evraksira, mikroDbName);
	}

	public Stream GetEvrakV2(string firmaid, string kullanici_adi, string sifre, string evraktipi, string evrakseri, string evraksira, string MikroDbName)
	{
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		evrakseri = evrakseri.Remove(0, 1);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None)));
		}
		Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
		ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
		Evrak evrak = new Evrak();
		SqlDB sqlDB2 = new SqlDB();
		sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
		enum_GenelEvrakTipleri enum_GenelEvrakTipleri = (enum_GenelEvrakTipleri)int.Parse(evraktipi);
		enum_sth_cins sth_cins = enum_sth_cins.Toptan;
		switch (enum_GenelEvrakTipleri)
		{
		case enum_GenelEvrakTipleri.DepolarArasiSevk:
			sth_cins = enum_sth_cins.Transfer;
			break;
		case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
			sth_cins = enum_sth_cins.Transfer;
			break;
		}
		evrak = EvrakData.GetEvrak(sqlDB2.Connection, evrakseri, int.Parse(evraksira), enum_GenelEvrakTipleri, parametreler._GetParametre("AlternatifDovizCinsi")._GetInt, enum_sip_cins.NormalSiparis, sth_cins);
		sqlDB2.ConnectionClose();
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			Evrak.WriteToStream(evrak, memoryStream, 999);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream;
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex3.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetRaporFileStokSatis(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetRaporFileStokSatisV2(firmaid, kullanici_adi, sifre, format, dosyaismi, mikroDbName, Content);
	}

	public Stream GetRaporFileStokSatisV2(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string MikroDbName, Stream Content)
	{
		new DataTable();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None)));
		}
		BinaryReader binaryReader = new BinaryReader(Content);
		RaporStokSatisSonuc raporStokSatisSonuc = RaporStokSatisSonuc.ReadFromBinaryReader(binaryReader);
		binaryReader.Close();
		binaryReader.Dispose();
		Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
		ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
		RaporStokSatis raporStokSatis = new RaporStokSatis();
		raporStokSatis.DataSource = raporStokSatisSonuc;
		List<RaporStokSatisSonuc> list = new List<RaporStokSatisSonuc>();
		list.Add(raporStokSatisSonuc);
		raporStokSatis.DataSource = list;
		try
		{
			if (File.Exists(text + "data\\formlar\\" + dosyaismi))
			{
				raporStokSatis.LoadLayout(text + "data\\formlar\\" + dosyaismi);
			}
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
		}
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			switch (format)
			{
			case "csv":
				raporStokSatis.ExportOptions.Csv.Encoding = Encoding.UTF8;
				raporStokSatis.ExportToCsv(memoryStream);
				break;
			case "jpg":
				raporStokSatis.ExportOptions.Image.ExportMode = ImageExportMode.SingleFile;
				raporStokSatis.ExportToImage(memoryStream);
				break;
			case "rtf":
				raporStokSatis.ExportOptions.Rtf.ExportMode = RtfExportMode.SingleFile;
				raporStokSatis.ExportToRtf(memoryStream);
				break;
			case "pdf":
				raporStokSatis.ExportOptions.Pdf.Compressed = true;
				raporStokSatis.ExportOptions.Pdf.ConvertImagesToJpeg = true;
				raporStokSatis.ExportOptions.Pdf.ImageQuality = PdfJpegImageQuality.High;
				raporStokSatis.ExportToPdf(memoryStream);
				break;
			case "xls":
				raporStokSatis.ExportOptions.Xls.ExportMode = XlsExportMode.SingleFile;
				raporStokSatis.ExportToXls(memoryStream);
				break;
			case "txt":
				raporStokSatis.ExportOptions.Text.Encoding = Encoding.UTF8;
				raporStokSatis.ExportToText(memoryStream);
				break;
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream;
		}
		catch (Exception ex4)
		{
			Log_Error(ex4.ToString());
			_ = "Hata: " + ex4.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex4.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetRaporFileStokEnvanterV2(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string MikroDbName, Stream Content)
	{
		new DataTable();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None)));
		}
		BinaryReader binaryReader = new BinaryReader(Content);
		RaporStokEnvanterSonuc raporStokEnvanterSonuc = RaporStokEnvanterSonuc.ReadFromBinaryReader(binaryReader);
		binaryReader.Close();
		binaryReader.Dispose();
		Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
		ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
		RaporStokEnvanter raporStokEnvanter = new RaporStokEnvanter();
		raporStokEnvanter.DataSource = raporStokEnvanterSonuc;
		List<RaporStokEnvanterSonuc> list = new List<RaporStokEnvanterSonuc>();
		list.Add(raporStokEnvanterSonuc);
		raporStokEnvanter.DataSource = list;
		try
		{
			if (File.Exists(text + "data\\formlar\\" + dosyaismi))
			{
				raporStokEnvanter.LoadLayout(text + "data\\formlar\\" + dosyaismi);
			}
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
		}
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			switch (format)
			{
			case "csv":
				raporStokEnvanter.ExportOptions.Csv.Encoding = Encoding.UTF8;
				raporStokEnvanter.ExportToCsv(memoryStream);
				break;
			case "jpg":
				raporStokEnvanter.ExportOptions.Image.ExportMode = ImageExportMode.SingleFile;
				raporStokEnvanter.ExportToImage(memoryStream);
				break;
			case "rtf":
				raporStokEnvanter.ExportOptions.Rtf.ExportMode = RtfExportMode.SingleFile;
				raporStokEnvanter.ExportToRtf(memoryStream);
				break;
			case "pdf":
				raporStokEnvanter.ExportOptions.Pdf.Compressed = true;
				raporStokEnvanter.ExportOptions.Pdf.ConvertImagesToJpeg = true;
				raporStokEnvanter.ExportOptions.Pdf.ImageQuality = PdfJpegImageQuality.High;
				raporStokEnvanter.ExportToPdf(memoryStream);
				break;
			case "xls":
				raporStokEnvanter.ExportOptions.Xls.ExportMode = XlsExportMode.SingleFile;
				raporStokEnvanter.ExportToXls(memoryStream);
				break;
			case "txt":
				raporStokEnvanter.ExportOptions.Text.Encoding = Encoding.UTF8;
				raporStokEnvanter.ExportToText(memoryStream);
				break;
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream;
		}
		catch (Exception ex4)
		{
			Log_Error(ex4.ToString());
			_ = "Hata: " + ex4.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex4.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetRaporStokSatis(string firmaid, string kullanici_adi, string sifre, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetRaporStokSatisV2(firmaid, kullanici_adi, sifre, mikroDbName, Content);
	}

	public Stream GetRaporStokSatisV2(string firmaid, string kullanici_adi, string sifre, string MikroDbName, Stream Content)
	{
		new RaporStokSatisSonucHam();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None)));
		}
		try
		{
			BinaryReader binaryReader = new BinaryReader(Content);
			string depoNo = binaryReader.ReadString();
			string projeKodu = binaryReader.ReadString();
			string sorumlulukMerkeziKodu = binaryReader.ReadString();
			string cariPersonelKodu = binaryReader.ReadString();
			string stokKodu = binaryReader.ReadString();
			string stokAnaGrupKodu = binaryReader.ReadString();
			string stokUreticiKodu = binaryReader.ReadString();
			string stokMarkaKodu = binaryReader.ReadString();
			string stokReyonKodu = binaryReader.ReadString();
			string stokKategoriKodu = binaryReader.ReadString();
			string cariKodu = binaryReader.ReadString();
			string cariBolgeKodu = binaryReader.ReadString();
			string cariGrupKodu = binaryReader.ReadString();
			int raporStokSatisMaliyetHesaplamaSekli = binaryReader.ReadInt32();
			RaporStokSatisSecenekleri rapor_secenekleri = RaporStokSatisSecenekleri.ReadFromBinaryReader(binaryReader);
			binaryReader.Close();
			binaryReader.Dispose();
			Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
			ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
			SqlDB sqlDB2 = new SqlDB();
			sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			RaporStokSatisSonucHam rapor = RaporStokSatisData.GetRapor(sqlDB2.Connection, rapor_secenekleri, depoNo, projeKodu, sorumlulukMerkeziKodu, cariPersonelKodu, stokKodu, stokAnaGrupKodu, stokUreticiKodu, stokMarkaKodu, stokReyonKodu, stokKategoriKodu, cariKodu, cariBolgeKodu, cariGrupKodu, raporStokSatisMaliyetHesaplamaSekli);
			sqlDB2.ConnectionClose();
			MemoryStream memoryStream = new MemoryStream();
			RaporStokSatisSonucHam.WriteToStream(rapor, memoryStream, 2);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream.ToArray()));
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : (Server side) " + ex3.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetRaporStokEnvanterV2(string firmaid, string kullanici_adi, string sifre, string MikroDbName, Stream Content)
	{
		new RaporStokEnvanterSonucHam();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None)));
		}
		try
		{
			string dBName = "MikroDB_V14";
			if (MikroDbName.StartsWith("MikroDB_V15"))
			{
				dBName = "MikroDB_V15";
			}
			if (MikroDbName.StartsWith("MikroDB_V12"))
			{
				dBName = "MikroDB_V12";
			}
			if (MikroDbName.StartsWith("MikroDB_V16"))
			{
				dBName = "MikroDB_V16";
			}
			if (MikroDbName.StartsWith("MikroDB_V17"))
			{
				dBName = "MikroDB_V17";
			}
			if (MikroDbName.StartsWith("MikroDB_V18"))
			{
				dBName = "MikroDB_V18";
			}
			if (MikroDbName.StartsWith("MikroDB_V19"))
			{
				dBName = "MikroDB_V19";
			}
			if (MikroDbName.StartsWith("MikroDB_V20"))
			{
				dBName = "MikroDB_V20";
			}
			BinaryReader binaryReader = new BinaryReader(Content);
			string depoNo = binaryReader.ReadString();
			string stokKodu = binaryReader.ReadString();
			string stokAnaGrupKodu = binaryReader.ReadString();
			string stokUreticiKodu = binaryReader.ReadString();
			string stokMarkaKodu = binaryReader.ReadString();
			string stokReyonKodu = binaryReader.ReadString();
			string stokKategoriKodu = binaryReader.ReadString();
			RaporStokEnvanterSecenekleri rapor_secenekleri = RaporStokEnvanterSecenekleri.ReadFromBinaryReader(binaryReader);
			binaryReader.Close();
			binaryReader.Dispose();
			Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
			ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
			SqlDB sqlDB2 = new SqlDB();
			sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			SqlDB sqlDB3 = new SqlDB();
			sqlDB3.ConnectionOpen(sqlBaglantiBilgileri, dBName);
			RaporStokEnvanterSonucHam rapor = RaporStokEnvanterData.GetRapor(sqlDB2.Connection, sqlDB3.Connection, rapor_secenekleri, depoNo, stokKodu, stokAnaGrupKodu, stokUreticiKodu, stokMarkaKodu, stokReyonKodu, stokKategoriKodu);
			sqlDB2.ConnectionClose();
			sqlDB3.ConnectionClose();
			MemoryStream memoryStream = new MemoryStream();
			RaporStokEnvanterSonucHam.WriteToStream(rapor, memoryStream, 1);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream.ToArray()));
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : (Server side) " + ex3.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetRaporFileStokSiparis(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetRaporFileStokSiparisV2(firmaid, kullanici_adi, sifre, format, dosyaismi, mikroDbName, Content);
	}

	public Stream GetRaporFileStokSiparisV2(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string MikroDbName, Stream Content)
	{
		new DataTable();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None)));
		}
		BinaryReader binaryReader = new BinaryReader(Content);
		RaporStokSiparisSonuc raporStokSiparisSonuc = RaporStokSiparisSonuc.ReadFromBinaryReader(binaryReader);
		binaryReader.Close();
		binaryReader.Dispose();
		Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
		ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
		RaporStokSiparis raporStokSiparis = new RaporStokSiparis();
		raporStokSiparis.DataSource = raporStokSiparisSonuc;
		List<RaporStokSiparisSonuc> list = new List<RaporStokSiparisSonuc>();
		list.Add(raporStokSiparisSonuc);
		raporStokSiparis.DataSource = list;
		try
		{
			if (File.Exists(text + "data\\formlar\\" + dosyaismi))
			{
				raporStokSiparis.LoadLayout(text + "data\\formlar\\" + dosyaismi);
			}
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
		}
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			switch (format)
			{
			case "csv":
				raporStokSiparis.ExportOptions.Csv.Encoding = Encoding.UTF8;
				raporStokSiparis.ExportToCsv(memoryStream);
				break;
			case "jpg":
				raporStokSiparis.ExportOptions.Image.ExportMode = ImageExportMode.SingleFile;
				raporStokSiparis.ExportToImage(memoryStream);
				break;
			case "rtf":
				raporStokSiparis.ExportOptions.Rtf.ExportMode = RtfExportMode.SingleFile;
				raporStokSiparis.ExportToRtf(memoryStream);
				break;
			case "pdf":
				raporStokSiparis.ExportOptions.Pdf.Compressed = true;
				raporStokSiparis.ExportOptions.Pdf.ConvertImagesToJpeg = true;
				raporStokSiparis.ExportOptions.Pdf.ImageQuality = PdfJpegImageQuality.High;
				raporStokSiparis.ExportToPdf(memoryStream);
				break;
			case "xls":
				raporStokSiparis.ExportOptions.Xls.ExportMode = XlsExportMode.SingleFile;
				raporStokSiparis.ExportToXls(memoryStream);
				break;
			case "txt":
				raporStokSiparis.ExportOptions.Text.Encoding = Encoding.UTF8;
				raporStokSiparis.ExportToText(memoryStream);
				break;
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream;
		}
		catch (Exception ex4)
		{
			Log_Error(ex4.ToString());
			_ = "Hata: " + ex4.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex4.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetRaporStokSiparis(string firmaid, string kullanici_adi, string sifre, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetRaporStokSiparisV2(firmaid, kullanici_adi, sifre, mikroDbName, Content);
	}

	public Stream GetRaporStokSiparisV2(string firmaid, string kullanici_adi, string sifre, string MikroDbName, Stream Content)
	{
		new RaporStokSiparisSonucHam();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None)));
		}
		try
		{
			BinaryReader binaryReader = new BinaryReader(Content);
			string depoNo = binaryReader.ReadString();
			string projeKodu = binaryReader.ReadString();
			string sorumlulukMerkeziKodu = binaryReader.ReadString();
			string cariPersonelKodu = binaryReader.ReadString();
			string stokKodu = binaryReader.ReadString();
			string stokAnaGrupKodu = binaryReader.ReadString();
			string stokUreticiKodu = binaryReader.ReadString();
			string stokMarkaKodu = binaryReader.ReadString();
			string stokReyonKodu = binaryReader.ReadString();
			string stokKategoriKodu = binaryReader.ReadString();
			string cariKodu = binaryReader.ReadString();
			string cariBolgeKodu = binaryReader.ReadString();
			string cariGrupKodu = binaryReader.ReadString();
			RaporStokSiparisSecenekleri rapor_secenekleri = RaporStokSiparisSecenekleri.ReadFromBinaryReader(binaryReader);
			binaryReader.Close();
			binaryReader.Dispose();
			Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
			ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
			SqlDB sqlDB2 = new SqlDB();
			sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			RaporStokSiparisSonucHam rapor = RaporStokSiparisData.GetRapor(sqlDB2.Connection, rapor_secenekleri, depoNo, projeKodu, sorumlulukMerkeziKodu, cariPersonelKodu, stokKodu, stokAnaGrupKodu, stokUreticiKodu, stokMarkaKodu, stokReyonKodu, stokKategoriKodu, cariKodu, cariBolgeKodu, cariGrupKodu);
			sqlDB2.ConnectionClose();
			MemoryStream memoryStream = new MemoryStream();
			RaporStokSiparisSonucHam.WriteToStream(rapor, memoryStream, 1);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream.ToArray()));
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex3.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetRaporFileYapilacakTahsilatlar(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetRaporFileYapilacakTahsilatlarV2(firmaid, kullanici_adi, sifre, format, dosyaismi, mikroDbName, Content);
	}

	public Stream GetRaporFileYapilacakTahsilatlarV2(string firmaid, string kullanici_adi, string sifre, string format, string dosyaismi, string MikroDbName, Stream Content)
	{
		new DataTable();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None)));
		}
		BinaryReader binaryReader = new BinaryReader(Content);
		RaporYapilacakTahsilatlarSonuc raporYapilacakTahsilatlarSonuc = RaporYapilacakTahsilatlarSonuc.ReadFromBinaryReader(binaryReader);
		binaryReader.Close();
		binaryReader.Dispose();
		Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
		ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
		RaporYapilacakTahsilatlar raporYapilacakTahsilatlar = new RaporYapilacakTahsilatlar();
		raporYapilacakTahsilatlar.DataSource = raporYapilacakTahsilatlarSonuc;
		List<RaporYapilacakTahsilatlarSonuc> list = new List<RaporYapilacakTahsilatlarSonuc>();
		list.Add(raporYapilacakTahsilatlarSonuc);
		raporYapilacakTahsilatlar.DataSource = list;
		try
		{
			if (File.Exists(text + "data\\formlar\\" + dosyaismi))
			{
				raporYapilacakTahsilatlar.LoadLayout(text + "data\\formlar\\" + dosyaismi);
			}
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			_ = "Hata: " + ex3.ToString();
		}
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			switch (format)
			{
			case "csv":
				raporYapilacakTahsilatlar.ExportOptions.Csv.Encoding = Encoding.UTF8;
				raporYapilacakTahsilatlar.ExportToCsv(memoryStream);
				break;
			case "jpg":
				raporYapilacakTahsilatlar.ExportOptions.Image.ExportMode = ImageExportMode.SingleFile;
				raporYapilacakTahsilatlar.ExportToImage(memoryStream);
				break;
			case "rtf":
				raporYapilacakTahsilatlar.ExportOptions.Rtf.ExportMode = RtfExportMode.SingleFile;
				raporYapilacakTahsilatlar.ExportToRtf(memoryStream);
				break;
			case "pdf":
				raporYapilacakTahsilatlar.ExportOptions.Pdf.Compressed = true;
				raporYapilacakTahsilatlar.ExportOptions.Pdf.ConvertImagesToJpeg = true;
				raporYapilacakTahsilatlar.ExportOptions.Pdf.ImageQuality = PdfJpegImageQuality.High;
				raporYapilacakTahsilatlar.ExportToPdf(memoryStream);
				break;
			case "xls":
				raporYapilacakTahsilatlar.ExportOptions.Xls.ExportMode = XlsExportMode.SingleFile;
				raporYapilacakTahsilatlar.ExportToXls(memoryStream);
				break;
			case "txt":
				raporYapilacakTahsilatlar.ExportOptions.Text.Encoding = Encoding.UTF8;
				raporYapilacakTahsilatlar.ExportToText(memoryStream);
				break;
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream;
		}
		catch (Exception ex4)
		{
			Log_Error(ex4.ToString());
			_ = "Hata: " + ex4.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex4.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public Stream GetRaporYapilacakTahsilatlar(string firmaid, string kullanici_adi, string sifre, Stream Content)
	{
		string mikroDbName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				mikroDbName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		return GetRaporYapilacakTahsilatlarV2(firmaid, kullanici_adi, sifre, mikroDbName, Content);
	}

	public Stream GetRaporYapilacakTahsilatlarV2(string firmaid, string kullanici_adi, string sifre, string MikroDbName, Stream Content)
	{
		new RaporStokSatisSonucHam();
		kullanici_adi = HttpUtility.UrlDecode(kullanici_adi);
		sifre = GenelUtilityWin.EncryptText("drjbq8777!#45", HttpUtility.UrlDecode(sifre), useHashing: true);
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri sqlBaglantiBilgileri;
		try
		{
			sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		bool flag = false;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@ParametreUser AND ParametreAdi='Sifre' AND ParametreDegeri=@ParametreDegeri";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", kullanici_adi);
				sqlCommand.Parameters.AddWithValue("@ParametreDegeri", sifre);
				if (int.Parse(sqlCommand.ExecuteScalar().ToString()) != 0)
				{
					flag = true;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : " + ex2.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
		if (!flag)
		{
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : Kullanıcı girişi başarısız.", Newtonsoft.Json.Formatting.None)));
		}
		DovizCinsiTanimlari dovizcinsitanimlari = new DovizCinsiTanimlari();
		try
		{
			dovizcinsitanimlari = DovizCinsiTanimlariData.GetDovizCinsiTanimlari(sqlBaglantiBilgileri, "MikroDB_V15");
		}
		catch
		{
		}
		try
		{
			BinaryReader binaryReader = new BinaryReader(Content);
			string projeKodu = binaryReader.ReadString();
			string sorumlulukMerkeziKodu = binaryReader.ReadString();
			string cariPersonelKodu = binaryReader.ReadString();
			string cariKodu = binaryReader.ReadString();
			string cariBolgeKodu = binaryReader.ReadString();
			string cariGrupKodu = binaryReader.ReadString();
			RaporYapilacakTahsilatlarSecenekleri rapor_secenekleri = RaporYapilacakTahsilatlarSecenekleri.ReadFromBinaryReader(binaryReader);
			binaryReader.Close();
			binaryReader.Dispose();
			Parametreler parametreler = ParametrelerDefault.MobilKullanici(kullanici_adi);
			ParametreData.ParametreOku(sqlBaglantiBilgileri, MikroDbName, parametreler, "akilli", kullanici_adi, "", "");
			SqlDB sqlDB2 = new SqlDB();
			sqlDB2.ConnectionOpen(sqlBaglantiBilgileri, MikroDbName);
			RaporYapilacakTahsilatlarSonucHam rapor = RaporYapilacakTahsilatlarData.GetRapor(sqlDB2.Connection, rapor_secenekleri, projeKodu, sorumlulukMerkeziKodu, cariPersonelKodu, cariKodu, cariBolgeKodu, cariGrupKodu, dovizcinsitanimlari);
			sqlDB2.ConnectionClose();
			MemoryStream memoryStream = new MemoryStream();
			RaporYapilacakTahsilatlarSonucHam.WriteToStream(rapor, memoryStream, 1);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(memoryStream.ToArray()));
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			return new MemoryStream(GenelUtilityWin.ToByteArrayGzip(JsonConvert.SerializeObject("Hata : (Server side) " + ex3.Message.Replace("\"", "").Replace("'", "").Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\", "")
				.Replace("\\", ""), Newtonsoft.Json.Formatting.None)));
		}
	}

	public OfflineEvrakV2 CariLokasyonGuncelleV2(OfflineEvrakV2 offlineevrak, SqlConnection opened_connection)
	{
		CariAdres adres = CariAdres.ReadFromStream(new MemoryStream(offlineevrak.Evrak));
		if (CariData.SetCariLokasyon(opened_connection, adres))
		{
			offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Aktarildi;
			offlineevrak.AktarilmaTarihi = DateTime.Now;
		}
		else
		{
			offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
			offlineevrak.HataString = "SQL Bağlantı Hatası";
		}
		return offlineevrak;
	}

	public OfflineEvrakV2 GunAcilisiYapV2(OfflineEvrakV2 offlineevrak, SqlConnection opened_connection)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(opened_connection.Database);
		GuneBaslaBitir guneBaslaBitir = GuneBaslaBitir.ReadFromStream(new MemoryStream(offlineevrak.Evrak));
		string text = "";
		try
		{
			bool flag = false;
			using (SqlCommand sqlCommand = opened_connection.CreateCommand())
			{
				string text2 = "";
				text2 = ((mikroVersiyon <= 15) ? "RecNO" : "Guid");
				sqlCommand.CommandText = "SELECT " + text2 + " FROM _TEMSILCI_GUNLUK_HAREKETLER WITH (NOLOCK) WHERE Temsilci_Kodu=@Temsilci_Kodu AND Tarih=@Tarih";
				sqlCommand.Parameters.AddWithValue("@Temsilci_Kodu", guneBaslaBitir.Temsilci_Kodu);
				sqlCommand.Parameters.AddWithValue("@Tarih", guneBaslaBitir.Tarih);
				object obj = sqlCommand.ExecuteScalar();
				if (obj != null)
				{
					flag = true;
					text = obj.ToString();
				}
			}
			if (flag)
			{
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
				offlineevrak.HataString = "Gün açılışı daha önce " + text + " kayıt numarası ile yapılmış!";
			}
			else
			{
				string text3 = "";
				string text4 = "";
				if (mikroVersiyon > 15)
				{
					text3 = "Guid,";
					text4 = "NEWID(),";
				}
				using (SqlCommand sqlCommand2 = opened_connection.CreateCommand())
				{
					sqlCommand2.CommandText = "INSERT INTO _TEMSILCI_GUNLUK_HAREKETLER(" + text3 + "Temsilci_Kodu,Tarih,Baslama_Yapildi,Baslama_Saati,Baslama_Kayit_Saati,Baslama_Enlem,Baslama_Boylam,Baslama_Arac_Km,Baslama_Mesaj,Ogle_Arasi_Baslama_Saati,Ogle_Arasi_Bitis_Saati,Bitis_Yapildi,Bitis_Saati,Bitis_Kayit_Saati,Bitis_Enlem,Bitis_Boylam,Bitis_Arac_Km,Bitis_Mesaj,Create_Date,Lastup_Date) VALUES(" + text4 + "@Temsilci_Kodu,@Tarih,@Baslama_Yapildi,@Baslama_Saati,@Baslama_Kayit_Saati,@Baslama_Enlem,@Baslama_Boylam,@Baslama_Arac_Km,@Baslama_Mesaj,@Ogle_Arasi_Baslama_Saati,@Ogle_Arasi_Bitis_Saati,@Bitis_Yapildi,@Bitis_Saati,@Bitis_Kayit_Saati,@Bitis_Enlem,@Bitis_Boylam,@Bitis_Arac_Km,@Bitis_Mesaj,getdate(),getdate()) ";
					sqlCommand2.Parameters.AddWithValue("@Temsilci_Kodu", guneBaslaBitir.Temsilci_Kodu);
					sqlCommand2.Parameters.AddWithValue("@Tarih", guneBaslaBitir.Tarih);
					sqlCommand2.Parameters.AddWithValue("@Baslama_Yapildi", true);
					sqlCommand2.Parameters.AddWithValue("@Baslama_Saati", guneBaslaBitir.Saati);
					sqlCommand2.Parameters.AddWithValue("@Baslama_Kayit_Saati", new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
					sqlCommand2.Parameters.AddWithValue("@Baslama_Enlem", guneBaslaBitir.Enlem);
					sqlCommand2.Parameters.AddWithValue("@Baslama_Boylam", guneBaslaBitir.Boylam);
					sqlCommand2.Parameters.AddWithValue("@Baslama_Arac_Km", guneBaslaBitir.Arac_Km);
					sqlCommand2.Parameters.AddWithValue("@Baslama_Mesaj", guneBaslaBitir.Mesaj);
					sqlCommand2.Parameters.AddWithValue("@Ogle_Arasi_Baslama_Saati", new TimeSpan(0, 0, 0));
					sqlCommand2.Parameters.AddWithValue("@Ogle_Arasi_Bitis_Saati", new TimeSpan(0, 0, 0));
					sqlCommand2.Parameters.AddWithValue("@Bitis_Yapildi", false);
					sqlCommand2.Parameters.AddWithValue("@Bitis_Saati", guneBaslaBitir.Saati);
					sqlCommand2.Parameters.AddWithValue("@Bitis_Kayit_Saati", new TimeSpan(0, 0, 0));
					sqlCommand2.Parameters.AddWithValue("@Bitis_Enlem", 0);
					sqlCommand2.Parameters.AddWithValue("@Bitis_Boylam", 0);
					sqlCommand2.Parameters.AddWithValue("@Bitis_Arac_Km", guneBaslaBitir.Arac_Km);
					sqlCommand2.Parameters.AddWithValue("@Bitis_Mesaj", "");
					sqlCommand2.ExecuteScalar();
				}
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Aktarildi;
				offlineevrak.AktarilmaTarihi = DateTime.Now;
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
			offlineevrak.HataString = "SQL Bağlantı Hatası";
		}
		return offlineevrak;
	}

	public OfflineEvrakV2 GunKapanisiYapV2(OfflineEvrakV2 offlineevrak, SqlConnection opened_connection)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(opened_connection.Database);
		GuneBaslaBitir guneBaslaBitir = GuneBaslaBitir.ReadFromStream(new MemoryStream(offlineevrak.Evrak));
		int num = 0;
		Guid guid = Guid.Empty;
		try
		{
			bool flag = false;
			using (SqlCommand sqlCommand = opened_connection.CreateCommand())
			{
				string text = "";
				text = ((mikroVersiyon <= 15) ? "RecNO" : "Guid");
				sqlCommand.CommandText = "SELECT " + text + " FROM _TEMSILCI_GUNLUK_HAREKETLER WITH (NOLOCK) WHERE Temsilci_Kodu=@Temsilci_Kodu AND Tarih=@Tarih";
				sqlCommand.Parameters.AddWithValue("@Temsilci_Kodu", guneBaslaBitir.Temsilci_Kodu);
				sqlCommand.Parameters.AddWithValue("@Tarih", guneBaslaBitir.Tarih);
				object obj = sqlCommand.ExecuteScalar();
				if (obj != null)
				{
					flag = true;
					if (mikroVersiyon > 15)
					{
						guid = (Guid)obj;
					}
					else
					{
						num = int.Parse(obj.ToString());
					}
				}
			}
			if (!flag)
			{
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
				offlineevrak.HataString = "Gün açılışı yapılmamış!";
			}
			else
			{
				using (SqlCommand sqlCommand2 = opened_connection.CreateCommand())
				{
					string text2 = "";
					text2 = ((mikroVersiyon <= 15) ? "RecNO" : "Guid");
					sqlCommand2.CommandText = "UPDATE _TEMSILCI_GUNLUK_HAREKETLER SET Bitis_Yapildi=@Bitis_Yapildi,Bitis_Saati=@Bitis_Saati,Bitis_Kayit_Saati=@Bitis_Kayit_Saati,Bitis_Enlem=@Bitis_Enlem,Bitis_Boylam=@Bitis_Boylam,Bitis_Arac_Km=@Bitis_Arac_Km,Bitis_Mesaj=@Bitis_Mesaj,Lastup_Date=getdate() WHERE " + text2 + "=@kayitid";
					sqlCommand2.Parameters.AddWithValue("@Bitis_Yapildi", true);
					sqlCommand2.Parameters.AddWithValue("@Bitis_Saati", guneBaslaBitir.Saati);
					sqlCommand2.Parameters.AddWithValue("@Bitis_Kayit_Saati", new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
					sqlCommand2.Parameters.AddWithValue("@Bitis_Enlem", guneBaslaBitir.Enlem);
					sqlCommand2.Parameters.AddWithValue("@Bitis_Boylam", guneBaslaBitir.Boylam);
					sqlCommand2.Parameters.AddWithValue("@Bitis_Arac_Km", guneBaslaBitir.Arac_Km);
					sqlCommand2.Parameters.AddWithValue("@Bitis_Mesaj", guneBaslaBitir.Mesaj);
					if (mikroVersiyon > 15)
					{
						sqlCommand2.Parameters.AddWithValue("@kayitid", guid);
					}
					else
					{
						sqlCommand2.Parameters.AddWithValue("@kayitid", num);
					}
					sqlCommand2.ExecuteScalar();
				}
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Aktarildi;
				offlineevrak.AktarilmaTarihi = DateTime.Now;
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
			offlineevrak.HataString = "SQL Bağlantı Hatası";
		}
		return offlineevrak;
	}

	public OfflineEvrakV2 CariYazBozGuncelle(OfflineEvrakV2 offlineevrak, SqlConnection opened_connection)
	{
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(offlineevrak.Evrak));
		binaryReader.ReadInt32();
		string cari_kod = binaryReader.ReadString();
		string data = binaryReader.ReadString();
		binaryReader.Close();
		binaryReader.Dispose();
		try
		{
			if (CariData.UpdateTextData(opened_connection, cari_kod, data))
			{
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Aktarildi;
				offlineevrak.AktarilmaTarihi = DateTime.Now;
			}
			else
			{
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
				offlineevrak.HataString = "işlem gerçekleştirilemedi!";
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
			offlineevrak.HataString = "SQL Bağlantı Hatası";
		}
		return offlineevrak;
	}

	public OfflineEvrakV2 YeniCariOlustur(OfflineEvrakV2 offlineevrak, SqlConnection opened_connection, string DBName, int MikroUserNo)
	{
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(offlineevrak.Evrak));
		Cari cari = Cari.ReadFromBinaryReader(binaryReader);
		binaryReader.Close();
		binaryReader.Dispose();
		try
		{
			string cari_kod = cari.cari_kod_prefix;
			if (cari.cari_kod == "")
			{
				SqlDB sqlDB = new SqlDB();
				try
				{
					using (SqlCommand sqlCommand = opened_connection.CreateCommand())
					{
						sqlCommand.CommandText = "SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WITH (NOLOCK) WHERE cari_kod like '" + cari.cari_kod_prefix + "%' ORDER BY cari_kod DESC";
						object obj = sqlCommand.ExecuteScalar();
						if (obj != null)
						{
							string text = obj.ToString();
							if (cari.cari_kod_prefix.Length != 0)
							{
								text = text.Replace(cari.cari_kod_prefix, "");
							}
							string text2 = "";
							text.Substring(text.Length - 1, 1);
							int result = 0;
							for (int i = 0; i < text.Length; i++)
							{
								string s = text.Substring(i, text.Length - i);
								result = 0;
								if (int.TryParse(s, out result))
								{
									break;
								}
							}
							text2 = result.ToString();
							if (text2.Length != 0)
							{
								long result2 = 0L;
								long.TryParse(text2, out result2);
								result2++;
								string text3 = text.Replace(text2, "");
								if ((result2 == 10 || result2 == 100 || result2 == 1000 || result2 == 10000 || result2 == 100000 || result2 == 1000000 || result2 == 10000000 || result2 == 100000000 || result2 == 1000000000 || result2 == 10000000000L || result2 == 100000000000L || result2 == 1000000000000L || result2 == 10000000000000L || result2 == 100000000000000L || result2 == 1000000000000000L || result2 == 10000000000000000L) && text3.Length > 0)
								{
									text3 = text3.Substring(0, text3.Length - 1);
								}
								cari_kod = cari.cari_kod_prefix + text3 + result2;
							}
							else
							{
								cari_kod = cari.cari_kod_prefix + "0";
							}
						}
						else
						{
							cari_kod = cari.cari_kod_prefix + "0";
						}
					}
					sqlDB.ConnectionClose();
				}
				catch
				{
					offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
					offlineevrak.HataString = "Hata : Yeni cari kodu belirlenemedi";
					return offlineevrak;
				}
			}
			else
			{
				cari_kod = cari.cari_kod_prefix + cari.cari_kod;
			}
			cari.cari_kod = cari_kod;
			foreach (CariAdres item in cari.CariAdresleri)
			{
				item.adr_cari_kod = cari.cari_kod;
			}
			foreach (CariYetkili item2 in cari.CariYetkilileri)
			{
				item2.mye_cari_kod = cari.cari_kod;
			}
			if (cari.KaydetYeniCari(opened_connection, DBName, MikroUserNo))
			{
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Aktarildi;
				offlineevrak.AktarilmaTarihi = DateTime.Now;
			}
			else
			{
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
				offlineevrak.HataString = "işlem gerçekleştirilemedi!";
			}
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
			offlineevrak.HataString = "SQL Bağlantı Hatası";
		}
		return offlineevrak;
	}

	public OfflineEvrakV2 ZiyaretKaydetV2(OfflineEvrakV2 offlineevrak, SqlConnection opened_connection, string MikroFirmaDBName)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(MikroFirmaDBName);
		Ziyaret ziyaret = Ziyaret.ReadFromStream(new MemoryStream(offlineevrak.Evrak));
		try
		{
			string text = "";
			string text2 = "";
			if (mikroVersiyon > 15)
			{
				text = "zyrt_Guid,";
				text2 = "NEWID(),";
			}
			using (SqlCommand sqlCommand = opened_connection.CreateCommand())
			{
				sqlCommand.CommandText = "INSERT INTO _ZIYARET_HAREKETLERI(" + text + "zyrt_Create_Date,zyrt_Lastup_Date,zyrt_iptal,zyrt_Tarihi,zyrt_Temsilci_Kodu,zyrt_Bolge_Kodu,zyrt_Cari_Kodu,zyrt_Cari_Adres_No,zyrt_Cari_Adres_Enlem,zyrt_Cari_Adres_Boylam,zyrt_Baslama_Saati,zyrt_Baslama_Enlem,zyrt_Baslama_Boylam,zyrt_Bitis_Saati,zyrt_Bitis_Enlem,zyrt_Bitis_Boylam,zyrt_Tamamlandi,zyrt_Fotograf_Id,zyrt_Proje_Kodu,zyrt_Sor_Mer_Kodu,zyrt_Bakim_Evrak_Seri,zyrt_Bakim_Evrak_Sira,zyrt_Satis_Yapildi,zyrt_Siparis_Alindi,zyrt_Urun_Teslim_Edildi,zyrt_Tahsilat_Yapildi,zyrt_Katalog_Birakildi,zyrt_Fiyat_Listesi_Birakildi,zyrt_Numune_Urun_Birakildi,zyrt_Konsinye_Urun_Birakildi,zyrt_Promosyon_Birakildi,zyrt_Firma_Durumu,zyrt_Rakip_Firma_Var,zyrt_Rakip_Firma_Durumu,zyrt_Urun_Yerlesim_Durumu,zyrt_Rakip_Urun_Yerlesim_Durumu,zyrt_Temsilci_Aciklama,zyrt_Cari_Firma_Memnuniyeti,zyrt_Cari_Urun_Memnuniyeti,zyrt_Cari_Fiyat_Memnuniyeti,zyrt_Cari_Temsilci_Memnuniyeti,zyrt_Cari_Aciklama,zyrt_Temsilci_Ozel_01_Var_Yok,zyrt_Temsilci_Ozel_01_Evet_Hayir,zyrt_Temsilci_Ozel_01_Derece,zyrt_Temsilci_Ozel_01_TamSayi,zyrt_Temsilci_Ozel_01_OndalikliSayi,zyrt_Temsilci_Ozel_01_Metin,zyrt_Temsilci_Ozel_01_Fotograf_Id,zyrt_Temsilci_Ozel_02_Var_Yok,zyrt_Temsilci_Ozel_02_Evet_Hayir,zyrt_Temsilci_Ozel_02_Derece,zyrt_Temsilci_Ozel_02_TamSayi,zyrt_Temsilci_Ozel_02_OndalikliSayi,zyrt_Temsilci_Ozel_02_Metin,zyrt_Temsilci_Ozel_02_Fotograf_Id,zyrt_Temsilci_Ozel_03_Var_Yok,zyrt_Temsilci_Ozel_03_Evet_Hayir,zyrt_Temsilci_Ozel_03_Derece,zyrt_Temsilci_Ozel_03_TamSayi,zyrt_Temsilci_Ozel_03_OndalikliSayi,zyrt_Temsilci_Ozel_03_Metin,zyrt_Temsilci_Ozel_03_Fotograf_Id,zyrt_Temsilci_Ozel_04_Var_Yok,zyrt_Temsilci_Ozel_04_Evet_Hayir,zyrt_Temsilci_Ozel_04_Derece,zyrt_Temsilci_Ozel_04_TamSayi,zyrt_Temsilci_Ozel_04_OndalikliSayi,zyrt_Temsilci_Ozel_04_Metin,zyrt_Temsilci_Ozel_04_Fotograf_Id,zyrt_Temsilci_Ozel_05_Var_Yok,zyrt_Temsilci_Ozel_05_Evet_Hayir,zyrt_Temsilci_Ozel_05_Derece,zyrt_Temsilci_Ozel_05_TamSayi,zyrt_Temsilci_Ozel_05_OndalikliSayi,zyrt_Temsilci_Ozel_05_Metin,zyrt_Temsilci_Ozel_05_Fotograf_Id,zyrt_Cari_Ozel_01_Var_Yok,zyrt_Cari_Ozel_01_Evet_Hayir,zyrt_Cari_Ozel_01_Derece,zyrt_Cari_Ozel_01_TamSayi,zyrt_Cari_Ozel_01_OndalikliSayi,zyrt_Cari_Ozel_01_Metin,zyrt_Cari_Ozel_01_Fotograf_Id,zyrt_Cari_Ozel_02_Var_Yok,zyrt_Cari_Ozel_02_Evet_Hayir,zyrt_Cari_Ozel_02_Derece,zyrt_Cari_Ozel_02_TamSayi,zyrt_Cari_Ozel_02_OndalikliSayi,zyrt_Cari_Ozel_02_Metin,zyrt_Cari_Ozel_02_Fotograf_Id,zyrt_Temsilci_Ozel_06_Var_Yok,zyrt_Temsilci_Ozel_06_Evet_Hayir,zyrt_Temsilci_Ozel_06_Derece,zyrt_Temsilci_Ozel_06_Metin,zyrt_Temsilci_Ozel_07_Var_Yok,zyrt_Temsilci_Ozel_07_Evet_Hayir,zyrt_Temsilci_Ozel_07_Derece,zyrt_Temsilci_Ozel_07_Metin,zyrt_Temsilci_Ozel_08_Var_Yok,zyrt_Temsilci_Ozel_08_Evet_Hayir,zyrt_Temsilci_Ozel_08_Derece,zyrt_Temsilci_Ozel_08_Metin,zyrt_Temsilci_Ozel_09_Var_Yok,zyrt_Temsilci_Ozel_09_Evet_Hayir,zyrt_Temsilci_Ozel_09_Derece,zyrt_Temsilci_Ozel_09_Metin,zyrt_Temsilci_Ozel_10_Var_Yok,zyrt_Temsilci_Ozel_10_Evet_Hayir,zyrt_Temsilci_Ozel_10_Derece,zyrt_Temsilci_Ozel_10_Metin,zyrt_Temsilci_Ozel_11_Var_Yok,zyrt_Temsilci_Ozel_11_Evet_Hayir,zyrt_Temsilci_Ozel_11_Derece,zyrt_Temsilci_Ozel_11_Metin,zyrt_Temsilci_Ozel_12_Var_Yok,zyrt_Temsilci_Ozel_12_Evet_Hayir,zyrt_Temsilci_Ozel_12_Derece,zyrt_Temsilci_Ozel_12_Metin,zyrt_Temsilci_Ozel_13_Var_Yok,zyrt_Temsilci_Ozel_13_Evet_Hayir,zyrt_Temsilci_Ozel_13_Derece,zyrt_Temsilci_Ozel_13_Metin,zyrt_Temsilci_Ozel_14_Var_Yok,zyrt_Temsilci_Ozel_14_Evet_Hayir,zyrt_Temsilci_Ozel_14_Derece,zyrt_Temsilci_Ozel_14_Metin,zyrt_Temsilci_Ozel_15_Var_Yok,zyrt_Temsilci_Ozel_15_Evet_Hayir,zyrt_Temsilci_Ozel_15_Derece,zyrt_Temsilci_Ozel_15_Metin,zyrt_Temsilci_Ozel_16_Var_Yok,zyrt_Temsilci_Ozel_16_Evet_Hayir,zyrt_Temsilci_Ozel_16_Derece,zyrt_Temsilci_Ozel_16_Metin,zyrt_Temsilci_Ozel_17_Var_Yok,zyrt_Temsilci_Ozel_17_Evet_Hayir,zyrt_Temsilci_Ozel_17_Derece,zyrt_Temsilci_Ozel_17_Metin,zyrt_Temsilci_Ozel_18_Var_Yok,zyrt_Temsilci_Ozel_18_Evet_Hayir,zyrt_Temsilci_Ozel_18_Derece,zyrt_Temsilci_Ozel_18_Metin,zyrt_Temsilci_Ozel_19_Var_Yok,zyrt_Temsilci_Ozel_19_Evet_Hayir,zyrt_Temsilci_Ozel_19_Derece,zyrt_Temsilci_Ozel_19_Metin,zyrt_Temsilci_Ozel_20_Var_Yok,zyrt_Temsilci_Ozel_20_Evet_Hayir,zyrt_Temsilci_Ozel_20_Derece,zyrt_Temsilci_Ozel_20_Metin,zyrt_Temsilci_Ozel_21_Var_Yok,zyrt_Temsilci_Ozel_21_Evet_Hayir,zyrt_Temsilci_Ozel_21_Derece,zyrt_Temsilci_Ozel_21_Metin,zyrt_Temsilci_Ozel_22_Var_Yok,zyrt_Temsilci_Ozel_22_Evet_Hayir,zyrt_Temsilci_Ozel_22_Derece,zyrt_Temsilci_Ozel_22_Metin,zyrt_Temsilci_Ozel_23_Var_Yok,zyrt_Temsilci_Ozel_23_Evet_Hayir,zyrt_Temsilci_Ozel_23_Derece,zyrt_Temsilci_Ozel_23_Metin,zyrt_Temsilci_Ozel_24_Var_Yok,zyrt_Temsilci_Ozel_24_Evet_Hayir,zyrt_Temsilci_Ozel_24_Derece,zyrt_Temsilci_Ozel_24_Metin,zyrt_Temsilci_Ozel_25_Var_Yok,zyrt_Temsilci_Ozel_25_Evet_Hayir,zyrt_Temsilci_Ozel_25_Derece,zyrt_Temsilci_Ozel_25_Metin,zyrt_Temsilci_Ozel_26_Var_Yok,zyrt_Temsilci_Ozel_26_Evet_Hayir,zyrt_Temsilci_Ozel_26_Derece,zyrt_Temsilci_Ozel_26_Metin,zyrt_Temsilci_Ozel_27_Var_Yok,zyrt_Temsilci_Ozel_27_Evet_Hayir,zyrt_Temsilci_Ozel_27_Derece,zyrt_Temsilci_Ozel_27_Metin,zyrt_Temsilci_Ozel_28_Var_Yok,zyrt_Temsilci_Ozel_28_Evet_Hayir,zyrt_Temsilci_Ozel_28_Derece,zyrt_Temsilci_Ozel_28_Metin,zyrt_Temsilci_Ozel_29_Var_Yok,zyrt_Temsilci_Ozel_29_Evet_Hayir,zyrt_Temsilci_Ozel_29_Derece,zyrt_Temsilci_Ozel_29_Metin,zyrt_Temsilci_Ozel_30_Var_Yok,zyrt_Temsilci_Ozel_30_Evet_Hayir,zyrt_Temsilci_Ozel_30_Derece,zyrt_Temsilci_Ozel_30_Metin,zyrt_Temsilci_Ozel_31_Var_Yok,zyrt_Temsilci_Ozel_31_Evet_Hayir,zyrt_Temsilci_Ozel_31_Derece,zyrt_Temsilci_Ozel_31_Metin,zyrt_Temsilci_Ozel_32_Var_Yok,zyrt_Temsilci_Ozel_32_Evet_Hayir,zyrt_Temsilci_Ozel_32_Derece,zyrt_Temsilci_Ozel_32_Metin,zyrt_Temsilci_Ozel_33_Var_Yok,zyrt_Temsilci_Ozel_33_Evet_Hayir,zyrt_Temsilci_Ozel_33_Derece,zyrt_Temsilci_Ozel_33_Metin,zyrt_Temsilci_Ozel_34_Var_Yok,zyrt_Temsilci_Ozel_34_Evet_Hayir,zyrt_Temsilci_Ozel_34_Derece,zyrt_Temsilci_Ozel_34_Metin,zyrt_Temsilci_Ozel_35_Var_Yok,zyrt_Temsilci_Ozel_35_Evet_Hayir,zyrt_Temsilci_Ozel_35_Derece,zyrt_Temsilci_Ozel_35_Metin,zyrt_Temsilci_Ozel_36_Var_Yok,zyrt_Temsilci_Ozel_36_Evet_Hayir,zyrt_Temsilci_Ozel_36_Derece,zyrt_Temsilci_Ozel_36_Metin,zyrt_Temsilci_Ozel_37_Var_Yok,zyrt_Temsilci_Ozel_37_Evet_Hayir,zyrt_Temsilci_Ozel_37_Derece,zyrt_Temsilci_Ozel_37_Metin,zyrt_Temsilci_Ozel_38_Var_Yok,zyrt_Temsilci_Ozel_38_Evet_Hayir,zyrt_Temsilci_Ozel_38_Derece,zyrt_Temsilci_Ozel_38_Metin,zyrt_Temsilci_Ozel_39_Var_Yok,zyrt_Temsilci_Ozel_39_Evet_Hayir,zyrt_Temsilci_Ozel_39_Derece,zyrt_Temsilci_Ozel_39_Metin,zyrt_Temsilci_Ozel_40_Var_Yok,zyrt_Temsilci_Ozel_40_Evet_Hayir,zyrt_Temsilci_Ozel_40_Derece,zyrt_Temsilci_Ozel_40_Metin,zyrt_Temsilci_Ozel_41_Var_Yok,zyrt_Temsilci_Ozel_41_Evet_Hayir,zyrt_Temsilci_Ozel_41_Derece,zyrt_Temsilci_Ozel_41_Metin,zyrt_Temsilci_Ozel_42_Var_Yok,zyrt_Temsilci_Ozel_42_Evet_Hayir,zyrt_Temsilci_Ozel_42_Derece,zyrt_Temsilci_Ozel_42_Metin,zyrt_Temsilci_Ozel_43_Var_Yok,zyrt_Temsilci_Ozel_43_Evet_Hayir,zyrt_Temsilci_Ozel_43_Derece,zyrt_Temsilci_Ozel_43_Metin,zyrt_Temsilci_Ozel_44_Var_Yok,zyrt_Temsilci_Ozel_44_Evet_Hayir,zyrt_Temsilci_Ozel_44_Derece,zyrt_Temsilci_Ozel_44_Metin,zyrt_Temsilci_Ozel_45_Var_Yok,zyrt_Temsilci_Ozel_45_Evet_Hayir,zyrt_Temsilci_Ozel_45_Derece,zyrt_Temsilci_Ozel_45_Metin,zyrt_Temsilci_Ozel_46_Var_Yok,zyrt_Temsilci_Ozel_46_Evet_Hayir,zyrt_Temsilci_Ozel_46_Derece,zyrt_Temsilci_Ozel_46_Metin,zyrt_Temsilci_Ozel_47_Var_Yok,zyrt_Temsilci_Ozel_47_Evet_Hayir,zyrt_Temsilci_Ozel_47_Derece,zyrt_Temsilci_Ozel_47_Metin,zyrt_Temsilci_Ozel_48_Var_Yok,zyrt_Temsilci_Ozel_48_Evet_Hayir,zyrt_Temsilci_Ozel_48_Derece,zyrt_Temsilci_Ozel_48_Metin,zyrt_Temsilci_Ozel_49_Var_Yok,zyrt_Temsilci_Ozel_49_Evet_Hayir,zyrt_Temsilci_Ozel_49_Derece,zyrt_Temsilci_Ozel_49_Metin,zyrt_Temsilci_Ozel_50_Var_Yok,zyrt_Temsilci_Ozel_50_Evet_Hayir,zyrt_Temsilci_Ozel_50_Derece,zyrt_Temsilci_Ozel_50_Metin,zyrt_Temsilci_Ozel_51_Var_Yok,zyrt_Temsilci_Ozel_51_Evet_Hayir,zyrt_Temsilci_Ozel_51_Derece,zyrt_Temsilci_Ozel_51_Metin,zyrt_Temsilci_Ozel_52_Var_Yok,zyrt_Temsilci_Ozel_52_Evet_Hayir,zyrt_Temsilci_Ozel_52_Derece,zyrt_Temsilci_Ozel_52_Metin,zyrt_Temsilci_Ozel_53_Var_Yok,zyrt_Temsilci_Ozel_53_Evet_Hayir,zyrt_Temsilci_Ozel_53_Derece,zyrt_Temsilci_Ozel_53_Metin,zyrt_Temsilci_Ozel_54_Var_Yok,zyrt_Temsilci_Ozel_54_Evet_Hayir,zyrt_Temsilci_Ozel_54_Derece,zyrt_Temsilci_Ozel_54_Metin,zyrt_Temsilci_Ozel_55_Var_Yok,zyrt_Temsilci_Ozel_55_Evet_Hayir,zyrt_Temsilci_Ozel_55_Derece,zyrt_Temsilci_Ozel_55_Metin,zyrt_Temsilci_Ozel_56_Var_Yok,zyrt_Temsilci_Ozel_56_Evet_Hayir,zyrt_Temsilci_Ozel_56_Derece,zyrt_Temsilci_Ozel_56_Metin,zyrt_Temsilci_Ozel_57_Var_Yok,zyrt_Temsilci_Ozel_57_Evet_Hayir,zyrt_Temsilci_Ozel_57_Derece,zyrt_Temsilci_Ozel_57_Metin,zyrt_Temsilci_Ozel_58_Var_Yok,zyrt_Temsilci_Ozel_58_Evet_Hayir,zyrt_Temsilci_Ozel_58_Derece,zyrt_Temsilci_Ozel_58_Metin,zyrt_Temsilci_Ozel_59_Var_Yok,zyrt_Temsilci_Ozel_59_Evet_Hayir,zyrt_Temsilci_Ozel_59_Derece,zyrt_Temsilci_Ozel_59_Metin,zyrt_Temsilci_Ozel_60_Var_Yok,zyrt_Temsilci_Ozel_60_Evet_Hayir,zyrt_Temsilci_Ozel_60_Derece,zyrt_Temsilci_Ozel_60_Metin,zyrt_Temsilci_Ozel_61_Var_Yok,zyrt_Temsilci_Ozel_61_Evet_Hayir,zyrt_Temsilci_Ozel_61_Derece,zyrt_Temsilci_Ozel_61_Metin,zyrt_Temsilci_Ozel_62_Var_Yok,zyrt_Temsilci_Ozel_62_Evet_Hayir,zyrt_Temsilci_Ozel_62_Derece,zyrt_Temsilci_Ozel_62_Metin,zyrt_Temsilci_Ozel_63_Var_Yok,zyrt_Temsilci_Ozel_63_Evet_Hayir,zyrt_Temsilci_Ozel_63_Derece,zyrt_Temsilci_Ozel_63_Metin,zyrt_Temsilci_Ozel_64_Var_Yok,zyrt_Temsilci_Ozel_64_Evet_Hayir,zyrt_Temsilci_Ozel_64_Derece,zyrt_Temsilci_Ozel_64_Metin,zyrt_Temsilci_Ozel_65_Var_Yok,zyrt_Temsilci_Ozel_65_Evet_Hayir,zyrt_Temsilci_Ozel_65_Derece,zyrt_Temsilci_Ozel_65_Metin,zyrt_Temsilci_Ozel_66_Var_Yok,zyrt_Temsilci_Ozel_66_Evet_Hayir,zyrt_Temsilci_Ozel_66_Derece,zyrt_Temsilci_Ozel_66_Metin,zyrt_Temsilci_Ozel_67_Var_Yok,zyrt_Temsilci_Ozel_67_Evet_Hayir,zyrt_Temsilci_Ozel_67_Derece,zyrt_Temsilci_Ozel_67_Metin,zyrt_Temsilci_Ozel_68_Var_Yok,zyrt_Temsilci_Ozel_68_Evet_Hayir,zyrt_Temsilci_Ozel_68_Derece,zyrt_Temsilci_Ozel_68_Metin,zyrt_Temsilci_Ozel_69_Var_Yok,zyrt_Temsilci_Ozel_69_Evet_Hayir,zyrt_Temsilci_Ozel_69_Derece,zyrt_Temsilci_Ozel_69_Metin,zyrt_Temsilci_Ozel_70_Var_Yok,zyrt_Temsilci_Ozel_70_Evet_Hayir,zyrt_Temsilci_Ozel_70_Derece,zyrt_Temsilci_Ozel_70_Metin,zyrt_Temsilci_Ozel_71_Var_Yok,zyrt_Temsilci_Ozel_71_Evet_Hayir,zyrt_Temsilci_Ozel_71_Derece,zyrt_Temsilci_Ozel_71_Metin,zyrt_Temsilci_Ozel_72_Var_Yok,zyrt_Temsilci_Ozel_72_Evet_Hayir,zyrt_Temsilci_Ozel_72_Derece,zyrt_Temsilci_Ozel_72_Metin,zyrt_Temsilci_Ozel_73_Var_Yok,zyrt_Temsilci_Ozel_73_Evet_Hayir,zyrt_Temsilci_Ozel_73_Derece,zyrt_Temsilci_Ozel_73_Metin,zyrt_Temsilci_Ozel_74_Var_Yok,zyrt_Temsilci_Ozel_74_Evet_Hayir,zyrt_Temsilci_Ozel_74_Derece,zyrt_Temsilci_Ozel_74_Metin,zyrt_Temsilci_Ozel_75_Var_Yok,zyrt_Temsilci_Ozel_75_Evet_Hayir,zyrt_Temsilci_Ozel_75_Derece,zyrt_Temsilci_Ozel_75_Metin,zyrt_Temsilci_Ozel_76_Var_Yok,zyrt_Temsilci_Ozel_76_Evet_Hayir,zyrt_Temsilci_Ozel_76_Derece,zyrt_Temsilci_Ozel_76_Metin,zyrt_Temsilci_Ozel_77_Var_Yok,zyrt_Temsilci_Ozel_77_Evet_Hayir,zyrt_Temsilci_Ozel_77_Derece,zyrt_Temsilci_Ozel_77_Metin,zyrt_Temsilci_Ozel_78_Var_Yok,zyrt_Temsilci_Ozel_78_Evet_Hayir,zyrt_Temsilci_Ozel_78_Derece,zyrt_Temsilci_Ozel_78_Metin,zyrt_Temsilci_Ozel_79_Var_Yok,zyrt_Temsilci_Ozel_79_Evet_Hayir,zyrt_Temsilci_Ozel_79_Derece,zyrt_Temsilci_Ozel_79_Metin,zyrt_Temsilci_Ozel_80_Var_Yok,zyrt_Temsilci_Ozel_80_Evet_Hayir,zyrt_Temsilci_Ozel_80_Derece,zyrt_Temsilci_Ozel_80_Metin,zyrt_Temsilci_Ozel_81_Var_Yok,zyrt_Temsilci_Ozel_81_Evet_Hayir,zyrt_Temsilci_Ozel_81_Derece,zyrt_Temsilci_Ozel_81_Metin,zyrt_Temsilci_Ozel_82_Var_Yok,zyrt_Temsilci_Ozel_82_Evet_Hayir,zyrt_Temsilci_Ozel_82_Derece,zyrt_Temsilci_Ozel_82_Metin,zyrt_Temsilci_Ozel_83_Var_Yok,zyrt_Temsilci_Ozel_83_Evet_Hayir,zyrt_Temsilci_Ozel_83_Derece,zyrt_Temsilci_Ozel_83_Metin,zyrt_Temsilci_Ozel_84_Var_Yok,zyrt_Temsilci_Ozel_84_Evet_Hayir,zyrt_Temsilci_Ozel_84_Derece,zyrt_Temsilci_Ozel_84_Metin,zyrt_Temsilci_Ozel_85_Var_Yok,zyrt_Temsilci_Ozel_85_Evet_Hayir,zyrt_Temsilci_Ozel_85_Derece,zyrt_Temsilci_Ozel_85_Metin,zyrt_Temsilci_Ozel_86_Var_Yok,zyrt_Temsilci_Ozel_86_Evet_Hayir,zyrt_Temsilci_Ozel_86_Derece,zyrt_Temsilci_Ozel_86_Metin,zyrt_Temsilci_Ozel_87_Var_Yok,zyrt_Temsilci_Ozel_87_Evet_Hayir,zyrt_Temsilci_Ozel_87_Derece,zyrt_Temsilci_Ozel_87_Metin,zyrt_Temsilci_Ozel_88_Var_Yok,zyrt_Temsilci_Ozel_88_Evet_Hayir,zyrt_Temsilci_Ozel_88_Derece,zyrt_Temsilci_Ozel_88_Metin,zyrt_Temsilci_Ozel_89_Var_Yok,zyrt_Temsilci_Ozel_89_Evet_Hayir,zyrt_Temsilci_Ozel_89_Derece,zyrt_Temsilci_Ozel_89_Metin,zyrt_Temsilci_Ozel_90_Var_Yok,zyrt_Temsilci_Ozel_90_Evet_Hayir,zyrt_Temsilci_Ozel_90_Derece,zyrt_Temsilci_Ozel_90_Metin,zyrt_Temsilci_Ozel_91_Var_Yok,zyrt_Temsilci_Ozel_91_Evet_Hayir,zyrt_Temsilci_Ozel_91_Derece,zyrt_Temsilci_Ozel_91_Metin,zyrt_Temsilci_Ozel_92_Var_Yok,zyrt_Temsilci_Ozel_92_Evet_Hayir,zyrt_Temsilci_Ozel_92_Derece,zyrt_Temsilci_Ozel_92_Metin,zyrt_Temsilci_Ozel_93_Var_Yok,zyrt_Temsilci_Ozel_93_Evet_Hayir,zyrt_Temsilci_Ozel_93_Derece,zyrt_Temsilci_Ozel_93_Metin,zyrt_Temsilci_Ozel_94_Var_Yok,zyrt_Temsilci_Ozel_94_Evet_Hayir,zyrt_Temsilci_Ozel_94_Derece,zyrt_Temsilci_Ozel_94_Metin,zyrt_Temsilci_Ozel_95_Var_Yok,zyrt_Temsilci_Ozel_95_Evet_Hayir,zyrt_Temsilci_Ozel_95_Derece,zyrt_Temsilci_Ozel_95_Metin,zyrt_Temsilci_Ozel_96_Var_Yok,zyrt_Temsilci_Ozel_96_Evet_Hayir,zyrt_Temsilci_Ozel_96_Derece,zyrt_Temsilci_Ozel_96_Metin,zyrt_Temsilci_Ozel_97_Var_Yok,zyrt_Temsilci_Ozel_97_Evet_Hayir,zyrt_Temsilci_Ozel_97_Derece,zyrt_Temsilci_Ozel_97_Metin,zyrt_Temsilci_Ozel_98_Var_Yok,zyrt_Temsilci_Ozel_98_Evet_Hayir,zyrt_Temsilci_Ozel_98_Derece,zyrt_Temsilci_Ozel_98_Metin,zyrt_Temsilci_Ozel_99_Var_Yok,zyrt_Temsilci_Ozel_99_Evet_Hayir,zyrt_Temsilci_Ozel_99_Derece,zyrt_Temsilci_Ozel_99_Metin,zyrt_Temsilci_Ozel_100_Var_Yok,zyrt_Temsilci_Ozel_100_Evet_Hayir,zyrt_Temsilci_Ozel_100_Derece,zyrt_Temsilci_Ozel_100_Metin) VALUES(" + text2 + "getdate(),getdate(),@zyrt_iptal,@zyrt_Tarihi,@zyrt_Temsilci_Kodu,@zyrt_Bolge_Kodu,@zyrt_Cari_Kodu,@zyrt_Cari_Adres_No,@zyrt_Cari_Adres_Enlem,@zyrt_Cari_Adres_Boylam,@zyrt_Baslama_Saati,@zyrt_Baslama_Enlem,@zyrt_Baslama_Boylam,@zyrt_Bitis_Saati,@zyrt_Bitis_Enlem,@zyrt_Bitis_Boylam,@zyrt_Tamamlandi,@zyrt_Fotograf_Id,@zyrt_Proje_Kodu,@zyrt_Sor_Mer_Kodu,@zyrt_Bakim_Evrak_Seri,@zyrt_Bakim_Evrak_Sira,@zyrt_Satis_Yapildi,@zyrt_Siparis_Alindi,@zyrt_Urun_Teslim_Edildi,@zyrt_Tahsilat_Yapildi,@zyrt_Katalog_Birakildi,@zyrt_Fiyat_Listesi_Birakildi,@zyrt_Numune_Urun_Birakildi,@zyrt_Konsinye_Urun_Birakildi,@zyrt_Promosyon_Birakildi,@zyrt_Firma_Durumu,@zyrt_Rakip_Firma_Var,@zyrt_Rakip_Firma_Durumu,@zyrt_Urun_Yerlesim_Durumu,@zyrt_Rakip_Urun_Yerlesim_Durumu,@zyrt_Temsilci_Aciklama,@zyrt_Cari_Firma_Memnuniyeti,@zyrt_Cari_Urun_Memnuniyeti,@zyrt_Cari_Fiyat_Memnuniyeti,@zyrt_Cari_Temsilci_Memnuniyeti,@zyrt_Cari_Aciklama,@zyrt_Temsilci_Ozel_01_Var_Yok,@zyrt_Temsilci_Ozel_01_Evet_Hayir,@zyrt_Temsilci_Ozel_01_Derece,@zyrt_Temsilci_Ozel_01_TamSayi,@zyrt_Temsilci_Ozel_01_OndalikliSayi,@zyrt_Temsilci_Ozel_01_Metin,@zyrt_Temsilci_Ozel_01_Fotograf_Id,@zyrt_Temsilci_Ozel_02_Var_Yok,@zyrt_Temsilci_Ozel_02_Evet_Hayir,@zyrt_Temsilci_Ozel_02_Derece,@zyrt_Temsilci_Ozel_02_TamSayi,@zyrt_Temsilci_Ozel_02_OndalikliSayi,@zyrt_Temsilci_Ozel_02_Metin,@zyrt_Temsilci_Ozel_02_Fotograf_Id,@zyrt_Temsilci_Ozel_03_Var_Yok,@zyrt_Temsilci_Ozel_03_Evet_Hayir,@zyrt_Temsilci_Ozel_03_Derece,@zyrt_Temsilci_Ozel_03_TamSayi,@zyrt_Temsilci_Ozel_03_OndalikliSayi,@zyrt_Temsilci_Ozel_03_Metin,@zyrt_Temsilci_Ozel_03_Fotograf_Id,@zyrt_Temsilci_Ozel_04_Var_Yok,@zyrt_Temsilci_Ozel_04_Evet_Hayir,@zyrt_Temsilci_Ozel_04_Derece,@zyrt_Temsilci_Ozel_04_TamSayi,@zyrt_Temsilci_Ozel_04_OndalikliSayi,@zyrt_Temsilci_Ozel_04_Metin,@zyrt_Temsilci_Ozel_04_Fotograf_Id,@zyrt_Temsilci_Ozel_05_Var_Yok,@zyrt_Temsilci_Ozel_05_Evet_Hayir,@zyrt_Temsilci_Ozel_05_Derece,@zyrt_Temsilci_Ozel_05_TamSayi,@zyrt_Temsilci_Ozel_05_OndalikliSayi,@zyrt_Temsilci_Ozel_05_Metin,@zyrt_Temsilci_Ozel_05_Fotograf_Id,@zyrt_Cari_Ozel_01_Var_Yok,@zyrt_Cari_Ozel_01_Evet_Hayir,@zyrt_Cari_Ozel_01_Derece,@zyrt_Cari_Ozel_01_TamSayi,@zyrt_Cari_Ozel_01_OndalikliSayi,@zyrt_Cari_Ozel_01_Metin,@zyrt_Cari_Ozel_01_Fotograf_Id,@zyrt_Cari_Ozel_02_Var_Yok,@zyrt_Cari_Ozel_02_Evet_Hayir,@zyrt_Cari_Ozel_02_Derece,@zyrt_Cari_Ozel_02_TamSayi,@zyrt_Cari_Ozel_02_OndalikliSayi,@zyrt_Cari_Ozel_02_Metin,@zyrt_Cari_Ozel_02_Fotograf_Id,@zyrt_Temsilci_Ozel_06_Var_Yok,@zyrt_Temsilci_Ozel_06_Evet_Hayir,@zyrt_Temsilci_Ozel_06_Derece,@zyrt_Temsilci_Ozel_06_Metin,@zyrt_Temsilci_Ozel_07_Var_Yok,@zyrt_Temsilci_Ozel_07_Evet_Hayir,@zyrt_Temsilci_Ozel_07_Derece,@zyrt_Temsilci_Ozel_07_Metin,@zyrt_Temsilci_Ozel_08_Var_Yok,@zyrt_Temsilci_Ozel_08_Evet_Hayir,@zyrt_Temsilci_Ozel_08_Derece,@zyrt_Temsilci_Ozel_08_Metin,@zyrt_Temsilci_Ozel_09_Var_Yok,@zyrt_Temsilci_Ozel_09_Evet_Hayir,@zyrt_Temsilci_Ozel_09_Derece,@zyrt_Temsilci_Ozel_09_Metin,@zyrt_Temsilci_Ozel_10_Var_Yok,@zyrt_Temsilci_Ozel_10_Evet_Hayir,@zyrt_Temsilci_Ozel_10_Derece,@zyrt_Temsilci_Ozel_10_Metin,@zyrt_Temsilci_Ozel_11_Var_Yok,@zyrt_Temsilci_Ozel_11_Evet_Hayir,@zyrt_Temsilci_Ozel_11_Derece,@zyrt_Temsilci_Ozel_11_Metin,@zyrt_Temsilci_Ozel_12_Var_Yok,@zyrt_Temsilci_Ozel_12_Evet_Hayir,@zyrt_Temsilci_Ozel_12_Derece,@zyrt_Temsilci_Ozel_12_Metin,@zyrt_Temsilci_Ozel_13_Var_Yok,@zyrt_Temsilci_Ozel_13_Evet_Hayir,@zyrt_Temsilci_Ozel_13_Derece,@zyrt_Temsilci_Ozel_13_Metin,@zyrt_Temsilci_Ozel_14_Var_Yok,@zyrt_Temsilci_Ozel_14_Evet_Hayir,@zyrt_Temsilci_Ozel_14_Derece,@zyrt_Temsilci_Ozel_14_Metin,@zyrt_Temsilci_Ozel_15_Var_Yok,@zyrt_Temsilci_Ozel_15_Evet_Hayir,@zyrt_Temsilci_Ozel_15_Derece,@zyrt_Temsilci_Ozel_15_Metin,@zyrt_Temsilci_Ozel_16_Var_Yok,@zyrt_Temsilci_Ozel_16_Evet_Hayir,@zyrt_Temsilci_Ozel_16_Derece,@zyrt_Temsilci_Ozel_16_Metin,@zyrt_Temsilci_Ozel_17_Var_Yok,@zyrt_Temsilci_Ozel_17_Evet_Hayir,@zyrt_Temsilci_Ozel_17_Derece,@zyrt_Temsilci_Ozel_17_Metin,@zyrt_Temsilci_Ozel_18_Var_Yok,@zyrt_Temsilci_Ozel_18_Evet_Hayir,@zyrt_Temsilci_Ozel_18_Derece,@zyrt_Temsilci_Ozel_18_Metin,@zyrt_Temsilci_Ozel_19_Var_Yok,@zyrt_Temsilci_Ozel_19_Evet_Hayir,@zyrt_Temsilci_Ozel_19_Derece,@zyrt_Temsilci_Ozel_19_Metin,@zyrt_Temsilci_Ozel_20_Var_Yok,@zyrt_Temsilci_Ozel_20_Evet_Hayir,@zyrt_Temsilci_Ozel_20_Derece,@zyrt_Temsilci_Ozel_20_Metin,@zyrt_Temsilci_Ozel_21_Var_Yok,@zyrt_Temsilci_Ozel_21_Evet_Hayir,@zyrt_Temsilci_Ozel_21_Derece,@zyrt_Temsilci_Ozel_21_Metin,@zyrt_Temsilci_Ozel_22_Var_Yok,@zyrt_Temsilci_Ozel_22_Evet_Hayir,@zyrt_Temsilci_Ozel_22_Derece,@zyrt_Temsilci_Ozel_22_Metin,@zyrt_Temsilci_Ozel_23_Var_Yok,@zyrt_Temsilci_Ozel_23_Evet_Hayir,@zyrt_Temsilci_Ozel_23_Derece,@zyrt_Temsilci_Ozel_23_Metin,@zyrt_Temsilci_Ozel_24_Var_Yok,@zyrt_Temsilci_Ozel_24_Evet_Hayir,@zyrt_Temsilci_Ozel_24_Derece,@zyrt_Temsilci_Ozel_24_Metin,@zyrt_Temsilci_Ozel_25_Var_Yok,@zyrt_Temsilci_Ozel_25_Evet_Hayir,@zyrt_Temsilci_Ozel_25_Derece,@zyrt_Temsilci_Ozel_25_Metin,@zyrt_Temsilci_Ozel_26_Var_Yok,@zyrt_Temsilci_Ozel_26_Evet_Hayir,@zyrt_Temsilci_Ozel_26_Derece,@zyrt_Temsilci_Ozel_26_Metin,@zyrt_Temsilci_Ozel_27_Var_Yok,@zyrt_Temsilci_Ozel_27_Evet_Hayir,@zyrt_Temsilci_Ozel_27_Derece,@zyrt_Temsilci_Ozel_27_Metin,@zyrt_Temsilci_Ozel_28_Var_Yok,@zyrt_Temsilci_Ozel_28_Evet_Hayir,@zyrt_Temsilci_Ozel_28_Derece,@zyrt_Temsilci_Ozel_28_Metin,@zyrt_Temsilci_Ozel_29_Var_Yok,@zyrt_Temsilci_Ozel_29_Evet_Hayir,@zyrt_Temsilci_Ozel_29_Derece,@zyrt_Temsilci_Ozel_29_Metin,@zyrt_Temsilci_Ozel_30_Var_Yok,@zyrt_Temsilci_Ozel_30_Evet_Hayir,@zyrt_Temsilci_Ozel_30_Derece,@zyrt_Temsilci_Ozel_30_Metin,@zyrt_Temsilci_Ozel_31_Var_Yok,@zyrt_Temsilci_Ozel_31_Evet_Hayir,@zyrt_Temsilci_Ozel_31_Derece,@zyrt_Temsilci_Ozel_31_Metin,@zyrt_Temsilci_Ozel_32_Var_Yok,@zyrt_Temsilci_Ozel_32_Evet_Hayir,@zyrt_Temsilci_Ozel_32_Derece,@zyrt_Temsilci_Ozel_32_Metin,@zyrt_Temsilci_Ozel_33_Var_Yok,@zyrt_Temsilci_Ozel_33_Evet_Hayir,@zyrt_Temsilci_Ozel_33_Derece,@zyrt_Temsilci_Ozel_33_Metin,@zyrt_Temsilci_Ozel_34_Var_Yok,@zyrt_Temsilci_Ozel_34_Evet_Hayir,@zyrt_Temsilci_Ozel_34_Derece,@zyrt_Temsilci_Ozel_34_Metin,@zyrt_Temsilci_Ozel_35_Var_Yok,@zyrt_Temsilci_Ozel_35_Evet_Hayir,@zyrt_Temsilci_Ozel_35_Derece,@zyrt_Temsilci_Ozel_35_Metin,@zyrt_Temsilci_Ozel_36_Var_Yok,@zyrt_Temsilci_Ozel_36_Evet_Hayir,@zyrt_Temsilci_Ozel_36_Derece,@zyrt_Temsilci_Ozel_36_Metin,@zyrt_Temsilci_Ozel_37_Var_Yok,@zyrt_Temsilci_Ozel_37_Evet_Hayir,@zyrt_Temsilci_Ozel_37_Derece,@zyrt_Temsilci_Ozel_37_Metin,@zyrt_Temsilci_Ozel_38_Var_Yok,@zyrt_Temsilci_Ozel_38_Evet_Hayir,@zyrt_Temsilci_Ozel_38_Derece,@zyrt_Temsilci_Ozel_38_Metin,@zyrt_Temsilci_Ozel_39_Var_Yok,@zyrt_Temsilci_Ozel_39_Evet_Hayir,@zyrt_Temsilci_Ozel_39_Derece,@zyrt_Temsilci_Ozel_39_Metin,@zyrt_Temsilci_Ozel_40_Var_Yok,@zyrt_Temsilci_Ozel_40_Evet_Hayir,@zyrt_Temsilci_Ozel_40_Derece,@zyrt_Temsilci_Ozel_40_Metin,@zyrt_Temsilci_Ozel_41_Var_Yok,@zyrt_Temsilci_Ozel_41_Evet_Hayir,@zyrt_Temsilci_Ozel_41_Derece,@zyrt_Temsilci_Ozel_41_Metin,@zyrt_Temsilci_Ozel_42_Var_Yok,@zyrt_Temsilci_Ozel_42_Evet_Hayir,@zyrt_Temsilci_Ozel_42_Derece,@zyrt_Temsilci_Ozel_42_Metin,@zyrt_Temsilci_Ozel_43_Var_Yok,@zyrt_Temsilci_Ozel_43_Evet_Hayir,@zyrt_Temsilci_Ozel_43_Derece,@zyrt_Temsilci_Ozel_43_Metin,@zyrt_Temsilci_Ozel_44_Var_Yok,@zyrt_Temsilci_Ozel_44_Evet_Hayir,@zyrt_Temsilci_Ozel_44_Derece,@zyrt_Temsilci_Ozel_44_Metin,@zyrt_Temsilci_Ozel_45_Var_Yok,@zyrt_Temsilci_Ozel_45_Evet_Hayir,@zyrt_Temsilci_Ozel_45_Derece,@zyrt_Temsilci_Ozel_45_Metin,@zyrt_Temsilci_Ozel_46_Var_Yok,@zyrt_Temsilci_Ozel_46_Evet_Hayir,@zyrt_Temsilci_Ozel_46_Derece,@zyrt_Temsilci_Ozel_46_Metin,@zyrt_Temsilci_Ozel_47_Var_Yok,@zyrt_Temsilci_Ozel_47_Evet_Hayir,@zyrt_Temsilci_Ozel_47_Derece,@zyrt_Temsilci_Ozel_47_Metin,@zyrt_Temsilci_Ozel_48_Var_Yok,@zyrt_Temsilci_Ozel_48_Evet_Hayir,@zyrt_Temsilci_Ozel_48_Derece,@zyrt_Temsilci_Ozel_48_Metin,@zyrt_Temsilci_Ozel_49_Var_Yok,@zyrt_Temsilci_Ozel_49_Evet_Hayir,@zyrt_Temsilci_Ozel_49_Derece,@zyrt_Temsilci_Ozel_49_Metin,@zyrt_Temsilci_Ozel_50_Var_Yok,@zyrt_Temsilci_Ozel_50_Evet_Hayir,@zyrt_Temsilci_Ozel_50_Derece,@zyrt_Temsilci_Ozel_50_Metin,@zyrt_Temsilci_Ozel_51_Var_Yok,@zyrt_Temsilci_Ozel_51_Evet_Hayir,@zyrt_Temsilci_Ozel_51_Derece,@zyrt_Temsilci_Ozel_51_Metin,@zyrt_Temsilci_Ozel_52_Var_Yok,@zyrt_Temsilci_Ozel_52_Evet_Hayir,@zyrt_Temsilci_Ozel_52_Derece,@zyrt_Temsilci_Ozel_52_Metin,@zyrt_Temsilci_Ozel_53_Var_Yok,@zyrt_Temsilci_Ozel_53_Evet_Hayir,@zyrt_Temsilci_Ozel_53_Derece,@zyrt_Temsilci_Ozel_53_Metin,@zyrt_Temsilci_Ozel_54_Var_Yok,@zyrt_Temsilci_Ozel_54_Evet_Hayir,@zyrt_Temsilci_Ozel_54_Derece,@zyrt_Temsilci_Ozel_54_Metin,@zyrt_Temsilci_Ozel_55_Var_Yok,@zyrt_Temsilci_Ozel_55_Evet_Hayir,@zyrt_Temsilci_Ozel_55_Derece,@zyrt_Temsilci_Ozel_55_Metin,@zyrt_Temsilci_Ozel_56_Var_Yok,@zyrt_Temsilci_Ozel_56_Evet_Hayir,@zyrt_Temsilci_Ozel_56_Derece,@zyrt_Temsilci_Ozel_56_Metin,@zyrt_Temsilci_Ozel_57_Var_Yok,@zyrt_Temsilci_Ozel_57_Evet_Hayir,@zyrt_Temsilci_Ozel_57_Derece,@zyrt_Temsilci_Ozel_57_Metin,@zyrt_Temsilci_Ozel_58_Var_Yok,@zyrt_Temsilci_Ozel_58_Evet_Hayir,@zyrt_Temsilci_Ozel_58_Derece,@zyrt_Temsilci_Ozel_58_Metin,@zyrt_Temsilci_Ozel_59_Var_Yok,@zyrt_Temsilci_Ozel_59_Evet_Hayir,@zyrt_Temsilci_Ozel_59_Derece,@zyrt_Temsilci_Ozel_59_Metin,@zyrt_Temsilci_Ozel_60_Var_Yok,@zyrt_Temsilci_Ozel_60_Evet_Hayir,@zyrt_Temsilci_Ozel_60_Derece,@zyrt_Temsilci_Ozel_60_Metin,@zyrt_Temsilci_Ozel_61_Var_Yok,@zyrt_Temsilci_Ozel_61_Evet_Hayir,@zyrt_Temsilci_Ozel_61_Derece,@zyrt_Temsilci_Ozel_61_Metin,@zyrt_Temsilci_Ozel_62_Var_Yok,@zyrt_Temsilci_Ozel_62_Evet_Hayir,@zyrt_Temsilci_Ozel_62_Derece,@zyrt_Temsilci_Ozel_62_Metin,@zyrt_Temsilci_Ozel_63_Var_Yok,@zyrt_Temsilci_Ozel_63_Evet_Hayir,@zyrt_Temsilci_Ozel_63_Derece,@zyrt_Temsilci_Ozel_63_Metin,@zyrt_Temsilci_Ozel_64_Var_Yok,@zyrt_Temsilci_Ozel_64_Evet_Hayir,@zyrt_Temsilci_Ozel_64_Derece,@zyrt_Temsilci_Ozel_64_Metin,@zyrt_Temsilci_Ozel_65_Var_Yok,@zyrt_Temsilci_Ozel_65_Evet_Hayir,@zyrt_Temsilci_Ozel_65_Derece,@zyrt_Temsilci_Ozel_65_Metin,@zyrt_Temsilci_Ozel_66_Var_Yok,@zyrt_Temsilci_Ozel_66_Evet_Hayir,@zyrt_Temsilci_Ozel_66_Derece,@zyrt_Temsilci_Ozel_66_Metin,@zyrt_Temsilci_Ozel_67_Var_Yok,@zyrt_Temsilci_Ozel_67_Evet_Hayir,@zyrt_Temsilci_Ozel_67_Derece,@zyrt_Temsilci_Ozel_67_Metin,@zyrt_Temsilci_Ozel_68_Var_Yok,@zyrt_Temsilci_Ozel_68_Evet_Hayir,@zyrt_Temsilci_Ozel_68_Derece,@zyrt_Temsilci_Ozel_68_Metin,@zyrt_Temsilci_Ozel_69_Var_Yok,@zyrt_Temsilci_Ozel_69_Evet_Hayir,@zyrt_Temsilci_Ozel_69_Derece,@zyrt_Temsilci_Ozel_69_Metin,@zyrt_Temsilci_Ozel_70_Var_Yok,@zyrt_Temsilci_Ozel_70_Evet_Hayir,@zyrt_Temsilci_Ozel_70_Derece,@zyrt_Temsilci_Ozel_70_Metin,@zyrt_Temsilci_Ozel_71_Var_Yok,@zyrt_Temsilci_Ozel_71_Evet_Hayir,@zyrt_Temsilci_Ozel_71_Derece,@zyrt_Temsilci_Ozel_71_Metin,@zyrt_Temsilci_Ozel_72_Var_Yok,@zyrt_Temsilci_Ozel_72_Evet_Hayir,@zyrt_Temsilci_Ozel_72_Derece,@zyrt_Temsilci_Ozel_72_Metin,@zyrt_Temsilci_Ozel_73_Var_Yok,@zyrt_Temsilci_Ozel_73_Evet_Hayir,@zyrt_Temsilci_Ozel_73_Derece,@zyrt_Temsilci_Ozel_73_Metin,@zyrt_Temsilci_Ozel_74_Var_Yok,@zyrt_Temsilci_Ozel_74_Evet_Hayir,@zyrt_Temsilci_Ozel_74_Derece,@zyrt_Temsilci_Ozel_74_Metin,@zyrt_Temsilci_Ozel_75_Var_Yok,@zyrt_Temsilci_Ozel_75_Evet_Hayir,@zyrt_Temsilci_Ozel_75_Derece,@zyrt_Temsilci_Ozel_75_Metin,@zyrt_Temsilci_Ozel_76_Var_Yok,@zyrt_Temsilci_Ozel_76_Evet_Hayir,@zyrt_Temsilci_Ozel_76_Derece,@zyrt_Temsilci_Ozel_76_Metin,@zyrt_Temsilci_Ozel_77_Var_Yok,@zyrt_Temsilci_Ozel_77_Evet_Hayir,@zyrt_Temsilci_Ozel_77_Derece,@zyrt_Temsilci_Ozel_77_Metin,@zyrt_Temsilci_Ozel_78_Var_Yok,@zyrt_Temsilci_Ozel_78_Evet_Hayir,@zyrt_Temsilci_Ozel_78_Derece,@zyrt_Temsilci_Ozel_78_Metin,@zyrt_Temsilci_Ozel_79_Var_Yok,@zyrt_Temsilci_Ozel_79_Evet_Hayir,@zyrt_Temsilci_Ozel_79_Derece,@zyrt_Temsilci_Ozel_79_Metin,@zyrt_Temsilci_Ozel_80_Var_Yok,@zyrt_Temsilci_Ozel_80_Evet_Hayir,@zyrt_Temsilci_Ozel_80_Derece,@zyrt_Temsilci_Ozel_80_Metin,@zyrt_Temsilci_Ozel_81_Var_Yok,@zyrt_Temsilci_Ozel_81_Evet_Hayir,@zyrt_Temsilci_Ozel_81_Derece,@zyrt_Temsilci_Ozel_81_Metin,@zyrt_Temsilci_Ozel_82_Var_Yok,@zyrt_Temsilci_Ozel_82_Evet_Hayir,@zyrt_Temsilci_Ozel_82_Derece,@zyrt_Temsilci_Ozel_82_Metin,@zyrt_Temsilci_Ozel_83_Var_Yok,@zyrt_Temsilci_Ozel_83_Evet_Hayir,@zyrt_Temsilci_Ozel_83_Derece,@zyrt_Temsilci_Ozel_83_Metin,@zyrt_Temsilci_Ozel_84_Var_Yok,@zyrt_Temsilci_Ozel_84_Evet_Hayir,@zyrt_Temsilci_Ozel_84_Derece,@zyrt_Temsilci_Ozel_84_Metin,@zyrt_Temsilci_Ozel_85_Var_Yok,@zyrt_Temsilci_Ozel_85_Evet_Hayir,@zyrt_Temsilci_Ozel_85_Derece,@zyrt_Temsilci_Ozel_85_Metin,@zyrt_Temsilci_Ozel_86_Var_Yok,@zyrt_Temsilci_Ozel_86_Evet_Hayir,@zyrt_Temsilci_Ozel_86_Derece,@zyrt_Temsilci_Ozel_86_Metin,@zyrt_Temsilci_Ozel_87_Var_Yok,@zyrt_Temsilci_Ozel_87_Evet_Hayir,@zyrt_Temsilci_Ozel_87_Derece,@zyrt_Temsilci_Ozel_87_Metin,@zyrt_Temsilci_Ozel_88_Var_Yok,@zyrt_Temsilci_Ozel_88_Evet_Hayir,@zyrt_Temsilci_Ozel_88_Derece,@zyrt_Temsilci_Ozel_88_Metin,@zyrt_Temsilci_Ozel_89_Var_Yok,@zyrt_Temsilci_Ozel_89_Evet_Hayir,@zyrt_Temsilci_Ozel_89_Derece,@zyrt_Temsilci_Ozel_89_Metin,@zyrt_Temsilci_Ozel_90_Var_Yok,@zyrt_Temsilci_Ozel_90_Evet_Hayir,@zyrt_Temsilci_Ozel_90_Derece,@zyrt_Temsilci_Ozel_90_Metin,@zyrt_Temsilci_Ozel_91_Var_Yok,@zyrt_Temsilci_Ozel_91_Evet_Hayir,@zyrt_Temsilci_Ozel_91_Derece,@zyrt_Temsilci_Ozel_91_Metin,@zyrt_Temsilci_Ozel_92_Var_Yok,@zyrt_Temsilci_Ozel_92_Evet_Hayir,@zyrt_Temsilci_Ozel_92_Derece,@zyrt_Temsilci_Ozel_92_Metin,@zyrt_Temsilci_Ozel_93_Var_Yok,@zyrt_Temsilci_Ozel_93_Evet_Hayir,@zyrt_Temsilci_Ozel_93_Derece,@zyrt_Temsilci_Ozel_93_Metin,@zyrt_Temsilci_Ozel_94_Var_Yok,@zyrt_Temsilci_Ozel_94_Evet_Hayir,@zyrt_Temsilci_Ozel_94_Derece,@zyrt_Temsilci_Ozel_94_Metin,@zyrt_Temsilci_Ozel_95_Var_Yok,@zyrt_Temsilci_Ozel_95_Evet_Hayir,@zyrt_Temsilci_Ozel_95_Derece,@zyrt_Temsilci_Ozel_95_Metin,@zyrt_Temsilci_Ozel_96_Var_Yok,@zyrt_Temsilci_Ozel_96_Evet_Hayir,@zyrt_Temsilci_Ozel_96_Derece,@zyrt_Temsilci_Ozel_96_Metin,@zyrt_Temsilci_Ozel_97_Var_Yok,@zyrt_Temsilci_Ozel_97_Evet_Hayir,@zyrt_Temsilci_Ozel_97_Derece,@zyrt_Temsilci_Ozel_97_Metin,@zyrt_Temsilci_Ozel_98_Var_Yok,@zyrt_Temsilci_Ozel_98_Evet_Hayir,@zyrt_Temsilci_Ozel_98_Derece,@zyrt_Temsilci_Ozel_98_Metin,@zyrt_Temsilci_Ozel_99_Var_Yok,@zyrt_Temsilci_Ozel_99_Evet_Hayir,@zyrt_Temsilci_Ozel_99_Derece,@zyrt_Temsilci_Ozel_99_Metin,@zyrt_Temsilci_Ozel_100_Var_Yok,@zyrt_Temsilci_Ozel_100_Evet_Hayir,@zyrt_Temsilci_Ozel_100_Derece,@zyrt_Temsilci_Ozel_100_Metin) ";
				sqlCommand.Parameters.AddWithValue("@zyrt_iptal", ziyaret.zyrt_iptal);
				sqlCommand.Parameters.AddWithValue("@zyrt_Tarihi", ziyaret.zyrt_Tarihi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Kodu", ziyaret.zyrt_Temsilci_Kodu);
				sqlCommand.Parameters.AddWithValue("@zyrt_Bolge_Kodu", ziyaret.zyrt_Bolge_Kodu);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Kodu", ziyaret.zyrt_Cari_Kodu);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Adres_No", ziyaret.zyrt_Cari_Adres_No);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Adres_Enlem", ziyaret.zyrt_Cari_Adres_Enlem);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Adres_Boylam", ziyaret.zyrt_Cari_Adres_Boylam);
				sqlCommand.Parameters.AddWithValue("@zyrt_Baslama_Saati", ziyaret.zyrt_Baslama_Saati);
				sqlCommand.Parameters.AddWithValue("@zyrt_Baslama_Enlem", ziyaret.zyrt_Baslama_Enlem);
				sqlCommand.Parameters.AddWithValue("@zyrt_Baslama_Boylam", ziyaret.zyrt_Baslama_Boylam);
				sqlCommand.Parameters.AddWithValue("@zyrt_Bitis_Saati", ziyaret.zyrt_Bitis_Saati);
				sqlCommand.Parameters.AddWithValue("@zyrt_Bitis_Enlem", ziyaret.zyrt_Bitis_Enlem);
				sqlCommand.Parameters.AddWithValue("@zyrt_Bitis_Boylam", ziyaret.zyrt_Bitis_Boylam);
				sqlCommand.Parameters.AddWithValue("@zyrt_Tamamlandi", ziyaret.zyrt_Tamamlandi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Fotograf_Id", ziyaret.zyrt_Fotograf_Id);
				sqlCommand.Parameters.AddWithValue("@zyrt_Proje_Kodu", ziyaret.zyrt_Proje_Kodu);
				sqlCommand.Parameters.AddWithValue("@zyrt_Sor_Mer_Kodu", ziyaret.zyrt_Sor_Mer_Kodu);
				sqlCommand.Parameters.AddWithValue("@zyrt_Bakim_Evrak_Seri", ziyaret.zyrt_Bakim_Evrak_Seri);
				sqlCommand.Parameters.AddWithValue("@zyrt_Bakim_Evrak_Sira", ziyaret.zyrt_Bakim_Evrak_Sira);
				sqlCommand.Parameters.AddWithValue("@zyrt_Satis_Yapildi", ziyaret.zyrt_Satis_Yapildi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Siparis_Alindi", ziyaret.zyrt_Siparis_Alindi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Urun_Teslim_Edildi", ziyaret.zyrt_Urun_Teslim_Edildi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Tahsilat_Yapildi", ziyaret.zyrt_Tahsilat_Yapildi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Katalog_Birakildi", ziyaret.zyrt_Katalog_Birakildi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Fiyat_Listesi_Birakildi", ziyaret.zyrt_Fiyat_Listesi_Birakildi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Numune_Urun_Birakildi", ziyaret.zyrt_Numune_Urun_Birakildi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Konsinye_Urun_Birakildi", ziyaret.zyrt_Konsinye_Urun_Birakildi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Promosyon_Birakildi", ziyaret.zyrt_Promosyon_Birakildi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Firma_Durumu", (int)ziyaret.zyrt_Firma_Durumu);
				sqlCommand.Parameters.AddWithValue("@zyrt_Rakip_Firma_Var", ziyaret.zyrt_Rakip_Firma_Var);
				sqlCommand.Parameters.AddWithValue("@zyrt_Rakip_Firma_Durumu", (int)ziyaret.zyrt_Rakip_Firma_Durumu);
				sqlCommand.Parameters.AddWithValue("@zyrt_Urun_Yerlesim_Durumu", (int)ziyaret.zyrt_Urun_Yerlesim_Durumu);
				sqlCommand.Parameters.AddWithValue("@zyrt_Rakip_Urun_Yerlesim_Durumu", (int)ziyaret.zyrt_Rakip_Urun_Yerlesim_Durumu);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Aciklama", ziyaret.zyrt_Temsilci_Aciklama);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Firma_Memnuniyeti", (int)ziyaret.zyrt_Cari_Firma_Memnuniyeti);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Urun_Memnuniyeti", (int)ziyaret.zyrt_Cari_Urun_Memnuniyeti);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Fiyat_Memnuniyeti", (int)ziyaret.zyrt_Cari_Fiyat_Memnuniyeti);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Temsilci_Memnuniyeti", (int)ziyaret.zyrt_Cari_Temsilci_Memnuniyeti);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Aciklama", ziyaret.zyrt_Cari_Aciklama);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_01_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_01_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_01_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_01_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_01_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_01_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_01_TamSayi", ziyaret.zyrt_Temsilci_Ozel_01_TamSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_01_OndalikliSayi", ziyaret.zyrt_Temsilci_Ozel_01_OndalikliSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_01_Metin", ziyaret.zyrt_Temsilci_Ozel_01_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_01_Fotograf_Id", ziyaret.zyrt_Temsilci_Ozel_01_Fotograf_Id);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_02_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_02_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_02_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_02_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_02_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_02_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_02_TamSayi", ziyaret.zyrt_Temsilci_Ozel_02_TamSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_02_OndalikliSayi", ziyaret.zyrt_Temsilci_Ozel_02_OndalikliSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_02_Metin", ziyaret.zyrt_Temsilci_Ozel_02_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_02_Fotograf_Id", ziyaret.zyrt_Temsilci_Ozel_02_Fotograf_Id);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_03_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_03_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_03_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_03_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_03_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_03_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_03_TamSayi", ziyaret.zyrt_Temsilci_Ozel_03_TamSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_03_OndalikliSayi", ziyaret.zyrt_Temsilci_Ozel_03_OndalikliSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_03_Metin", ziyaret.zyrt_Temsilci_Ozel_03_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_03_Fotograf_Id", ziyaret.zyrt_Temsilci_Ozel_03_Fotograf_Id);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_04_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_04_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_04_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_04_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_04_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_04_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_04_TamSayi", ziyaret.zyrt_Temsilci_Ozel_04_TamSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_04_OndalikliSayi", ziyaret.zyrt_Temsilci_Ozel_04_OndalikliSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_04_Metin", ziyaret.zyrt_Temsilci_Ozel_04_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_04_Fotograf_Id", ziyaret.zyrt_Temsilci_Ozel_04_Fotograf_Id);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_05_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_05_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_05_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_05_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_05_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_05_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_05_TamSayi", ziyaret.zyrt_Temsilci_Ozel_05_TamSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_05_OndalikliSayi", ziyaret.zyrt_Temsilci_Ozel_05_OndalikliSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_05_Metin", ziyaret.zyrt_Temsilci_Ozel_05_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_05_Fotograf_Id", ziyaret.zyrt_Temsilci_Ozel_05_Fotograf_Id);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_01_Var_Yok", ziyaret.zyrt_Cari_Ozel_01_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_01_Evet_Hayir", ziyaret.zyrt_Cari_Ozel_01_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_01_Derece", (int)ziyaret.zyrt_Cari_Ozel_01_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_01_TamSayi", ziyaret.zyrt_Cari_Ozel_01_TamSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_01_OndalikliSayi", ziyaret.zyrt_Cari_Ozel_01_OndalikliSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_01_Metin", ziyaret.zyrt_Cari_Ozel_01_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_01_Fotograf_Id", ziyaret.zyrt_Cari_Ozel_01_Fotograf_Id);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_02_Var_Yok", ziyaret.zyrt_Cari_Ozel_02_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_02_Evet_Hayir", ziyaret.zyrt_Cari_Ozel_02_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_02_Derece", (int)ziyaret.zyrt_Cari_Ozel_02_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_02_TamSayi", ziyaret.zyrt_Cari_Ozel_02_TamSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_02_OndalikliSayi", ziyaret.zyrt_Cari_Ozel_02_OndalikliSayi);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_02_Metin", ziyaret.zyrt_Cari_Ozel_02_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Ozel_02_Fotograf_Id", ziyaret.zyrt_Cari_Ozel_02_Fotograf_Id);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_06_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_06_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_06_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_06_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_06_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_06_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_06_Metin", ziyaret.zyrt_Temsilci_Ozel_06_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_07_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_07_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_07_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_07_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_07_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_07_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_07_Metin", ziyaret.zyrt_Temsilci_Ozel_07_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_08_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_08_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_08_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_08_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_08_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_08_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_08_Metin", ziyaret.zyrt_Temsilci_Ozel_08_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_09_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_09_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_09_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_09_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_09_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_09_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_09_Metin", ziyaret.zyrt_Temsilci_Ozel_09_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_10_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_10_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_10_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_10_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_10_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_10_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_10_Metin", ziyaret.zyrt_Temsilci_Ozel_10_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_11_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_11_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_11_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_11_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_11_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_11_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_11_Metin", ziyaret.zyrt_Temsilci_Ozel_11_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_12_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_12_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_12_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_12_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_12_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_12_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_12_Metin", ziyaret.zyrt_Temsilci_Ozel_12_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_13_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_13_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_13_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_13_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_13_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_13_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_13_Metin", ziyaret.zyrt_Temsilci_Ozel_13_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_14_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_14_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_14_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_14_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_14_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_14_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_14_Metin", ziyaret.zyrt_Temsilci_Ozel_14_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_15_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_15_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_15_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_15_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_15_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_15_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_15_Metin", ziyaret.zyrt_Temsilci_Ozel_15_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_16_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_16_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_16_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_16_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_16_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_16_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_16_Metin", ziyaret.zyrt_Temsilci_Ozel_16_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_17_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_17_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_17_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_17_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_17_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_17_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_17_Metin", ziyaret.zyrt_Temsilci_Ozel_17_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_18_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_18_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_18_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_18_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_18_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_18_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_18_Metin", ziyaret.zyrt_Temsilci_Ozel_18_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_19_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_19_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_19_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_19_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_19_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_19_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_19_Metin", ziyaret.zyrt_Temsilci_Ozel_19_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_20_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_20_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_20_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_20_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_20_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_20_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_20_Metin", ziyaret.zyrt_Temsilci_Ozel_20_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_21_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_21_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_21_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_21_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_21_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_21_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_21_Metin", ziyaret.zyrt_Temsilci_Ozel_21_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_22_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_22_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_22_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_22_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_22_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_22_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_22_Metin", ziyaret.zyrt_Temsilci_Ozel_22_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_23_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_23_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_23_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_23_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_23_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_23_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_23_Metin", ziyaret.zyrt_Temsilci_Ozel_23_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_24_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_24_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_24_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_24_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_24_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_24_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_24_Metin", ziyaret.zyrt_Temsilci_Ozel_24_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_25_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_25_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_25_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_25_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_25_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_25_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_25_Metin", ziyaret.zyrt_Temsilci_Ozel_25_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_26_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_26_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_26_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_26_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_26_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_26_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_26_Metin", ziyaret.zyrt_Temsilci_Ozel_26_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_27_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_27_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_27_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_27_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_27_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_27_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_27_Metin", ziyaret.zyrt_Temsilci_Ozel_27_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_28_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_28_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_28_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_28_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_28_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_28_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_28_Metin", ziyaret.zyrt_Temsilci_Ozel_28_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_29_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_29_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_29_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_29_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_29_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_29_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_29_Metin", ziyaret.zyrt_Temsilci_Ozel_29_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_30_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_30_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_30_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_30_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_30_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_30_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_30_Metin", ziyaret.zyrt_Temsilci_Ozel_30_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_31_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_31_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_31_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_31_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_31_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_31_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_31_Metin", ziyaret.zyrt_Temsilci_Ozel_31_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_32_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_32_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_32_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_32_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_32_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_32_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_32_Metin", ziyaret.zyrt_Temsilci_Ozel_32_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_33_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_33_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_33_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_33_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_33_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_33_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_33_Metin", ziyaret.zyrt_Temsilci_Ozel_33_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_34_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_34_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_34_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_34_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_34_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_34_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_34_Metin", ziyaret.zyrt_Temsilci_Ozel_34_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_35_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_35_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_35_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_35_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_35_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_35_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_35_Metin", ziyaret.zyrt_Temsilci_Ozel_35_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_36_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_36_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_36_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_36_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_36_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_36_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_36_Metin", ziyaret.zyrt_Temsilci_Ozel_36_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_37_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_37_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_37_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_37_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_37_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_37_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_37_Metin", ziyaret.zyrt_Temsilci_Ozel_37_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_38_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_38_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_38_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_38_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_38_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_38_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_38_Metin", ziyaret.zyrt_Temsilci_Ozel_38_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_39_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_39_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_39_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_39_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_39_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_39_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_39_Metin", ziyaret.zyrt_Temsilci_Ozel_39_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_40_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_40_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_40_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_40_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_40_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_40_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_40_Metin", ziyaret.zyrt_Temsilci_Ozel_40_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_41_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_41_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_41_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_41_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_41_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_41_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_41_Metin", ziyaret.zyrt_Temsilci_Ozel_41_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_42_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_42_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_42_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_42_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_42_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_42_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_42_Metin", ziyaret.zyrt_Temsilci_Ozel_42_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_43_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_43_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_43_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_43_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_43_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_43_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_43_Metin", ziyaret.zyrt_Temsilci_Ozel_43_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_44_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_44_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_44_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_44_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_44_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_44_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_44_Metin", ziyaret.zyrt_Temsilci_Ozel_44_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_45_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_45_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_45_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_45_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_45_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_45_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_45_Metin", ziyaret.zyrt_Temsilci_Ozel_45_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_46_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_46_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_46_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_46_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_46_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_46_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_46_Metin", ziyaret.zyrt_Temsilci_Ozel_46_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_47_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_47_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_47_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_47_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_47_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_47_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_47_Metin", ziyaret.zyrt_Temsilci_Ozel_47_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_48_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_48_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_48_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_48_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_48_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_48_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_48_Metin", ziyaret.zyrt_Temsilci_Ozel_48_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_49_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_49_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_49_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_49_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_49_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_49_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_49_Metin", ziyaret.zyrt_Temsilci_Ozel_49_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_50_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_50_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_50_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_50_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_50_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_50_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_50_Metin", ziyaret.zyrt_Temsilci_Ozel_50_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_51_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_51_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_51_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_51_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_51_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_51_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_51_Metin", ziyaret.zyrt_Temsilci_Ozel_51_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_52_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_52_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_52_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_52_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_52_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_52_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_52_Metin", ziyaret.zyrt_Temsilci_Ozel_52_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_53_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_53_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_53_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_53_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_53_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_53_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_53_Metin", ziyaret.zyrt_Temsilci_Ozel_53_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_54_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_54_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_54_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_54_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_54_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_54_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_54_Metin", ziyaret.zyrt_Temsilci_Ozel_54_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_55_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_55_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_55_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_55_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_55_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_55_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_55_Metin", ziyaret.zyrt_Temsilci_Ozel_55_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_56_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_56_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_56_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_56_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_56_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_56_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_56_Metin", ziyaret.zyrt_Temsilci_Ozel_56_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_57_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_57_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_57_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_57_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_57_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_57_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_57_Metin", ziyaret.zyrt_Temsilci_Ozel_57_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_58_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_58_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_58_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_58_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_58_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_58_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_58_Metin", ziyaret.zyrt_Temsilci_Ozel_58_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_59_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_59_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_59_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_59_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_59_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_59_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_59_Metin", ziyaret.zyrt_Temsilci_Ozel_59_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_60_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_60_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_60_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_60_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_60_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_60_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_60_Metin", ziyaret.zyrt_Temsilci_Ozel_60_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_61_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_61_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_61_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_61_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_61_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_61_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_61_Metin", ziyaret.zyrt_Temsilci_Ozel_61_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_62_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_62_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_62_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_62_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_62_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_62_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_62_Metin", ziyaret.zyrt_Temsilci_Ozel_62_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_63_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_63_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_63_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_63_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_63_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_63_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_63_Metin", ziyaret.zyrt_Temsilci_Ozel_63_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_64_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_64_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_64_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_64_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_64_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_64_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_64_Metin", ziyaret.zyrt_Temsilci_Ozel_64_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_65_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_65_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_65_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_65_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_65_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_65_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_65_Metin", ziyaret.zyrt_Temsilci_Ozel_65_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_66_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_66_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_66_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_66_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_66_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_66_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_66_Metin", ziyaret.zyrt_Temsilci_Ozel_66_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_67_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_67_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_67_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_67_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_67_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_67_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_67_Metin", ziyaret.zyrt_Temsilci_Ozel_67_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_68_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_68_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_68_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_68_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_68_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_68_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_68_Metin", ziyaret.zyrt_Temsilci_Ozel_68_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_69_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_69_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_69_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_69_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_69_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_69_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_69_Metin", ziyaret.zyrt_Temsilci_Ozel_69_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_70_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_70_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_70_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_70_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_70_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_70_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_70_Metin", ziyaret.zyrt_Temsilci_Ozel_70_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_71_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_71_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_71_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_71_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_71_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_71_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_71_Metin", ziyaret.zyrt_Temsilci_Ozel_71_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_72_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_72_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_72_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_72_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_72_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_72_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_72_Metin", ziyaret.zyrt_Temsilci_Ozel_72_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_73_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_73_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_73_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_73_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_73_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_73_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_73_Metin", ziyaret.zyrt_Temsilci_Ozel_73_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_74_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_74_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_74_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_74_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_74_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_74_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_74_Metin", ziyaret.zyrt_Temsilci_Ozel_74_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_75_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_75_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_75_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_75_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_75_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_75_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_75_Metin", ziyaret.zyrt_Temsilci_Ozel_75_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_76_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_76_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_76_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_76_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_76_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_76_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_76_Metin", ziyaret.zyrt_Temsilci_Ozel_76_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_77_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_77_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_77_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_77_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_77_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_77_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_77_Metin", ziyaret.zyrt_Temsilci_Ozel_77_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_78_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_78_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_78_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_78_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_78_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_78_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_78_Metin", ziyaret.zyrt_Temsilci_Ozel_78_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_79_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_79_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_79_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_79_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_79_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_79_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_79_Metin", ziyaret.zyrt_Temsilci_Ozel_79_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_80_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_80_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_80_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_80_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_80_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_80_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_80_Metin", ziyaret.zyrt_Temsilci_Ozel_80_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_81_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_81_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_81_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_81_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_81_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_81_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_81_Metin", ziyaret.zyrt_Temsilci_Ozel_81_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_82_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_82_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_82_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_82_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_82_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_82_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_82_Metin", ziyaret.zyrt_Temsilci_Ozel_82_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_83_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_83_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_83_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_83_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_83_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_83_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_83_Metin", ziyaret.zyrt_Temsilci_Ozel_83_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_84_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_84_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_84_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_84_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_84_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_84_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_84_Metin", ziyaret.zyrt_Temsilci_Ozel_84_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_85_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_85_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_85_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_85_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_85_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_85_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_85_Metin", ziyaret.zyrt_Temsilci_Ozel_85_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_86_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_86_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_86_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_86_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_86_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_86_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_86_Metin", ziyaret.zyrt_Temsilci_Ozel_86_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_87_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_87_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_87_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_87_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_87_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_87_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_87_Metin", ziyaret.zyrt_Temsilci_Ozel_87_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_88_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_88_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_88_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_88_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_88_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_88_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_88_Metin", ziyaret.zyrt_Temsilci_Ozel_88_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_89_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_89_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_89_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_89_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_89_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_89_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_89_Metin", ziyaret.zyrt_Temsilci_Ozel_89_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_90_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_90_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_90_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_90_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_90_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_90_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_90_Metin", ziyaret.zyrt_Temsilci_Ozel_90_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_91_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_91_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_91_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_91_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_91_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_91_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_91_Metin", ziyaret.zyrt_Temsilci_Ozel_91_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_92_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_92_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_92_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_92_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_92_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_92_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_92_Metin", ziyaret.zyrt_Temsilci_Ozel_92_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_93_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_93_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_93_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_93_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_93_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_93_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_93_Metin", ziyaret.zyrt_Temsilci_Ozel_93_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_94_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_94_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_94_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_94_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_94_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_94_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_94_Metin", ziyaret.zyrt_Temsilci_Ozel_94_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_95_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_95_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_95_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_95_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_95_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_95_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_95_Metin", ziyaret.zyrt_Temsilci_Ozel_95_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_96_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_96_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_96_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_96_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_96_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_96_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_96_Metin", ziyaret.zyrt_Temsilci_Ozel_96_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_97_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_97_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_97_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_97_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_97_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_97_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_97_Metin", ziyaret.zyrt_Temsilci_Ozel_97_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_98_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_98_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_98_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_98_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_98_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_98_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_98_Metin", ziyaret.zyrt_Temsilci_Ozel_98_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_99_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_99_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_99_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_99_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_99_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_99_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_99_Metin", ziyaret.zyrt_Temsilci_Ozel_99_Metin);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_100_Var_Yok", ziyaret.zyrt_Temsilci_Ozel_100_Var_Yok);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_100_Evet_Hayir", ziyaret.zyrt_Temsilci_Ozel_100_Evet_Hayir);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_100_Derece", (int)ziyaret.zyrt_Temsilci_Ozel_100_Derece);
				sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Ozel_100_Metin", ziyaret.zyrt_Temsilci_Ozel_100_Metin);
				sqlCommand.ExecuteScalar();
			}
			offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Aktarildi;
			offlineevrak.AktarilmaTarihi = DateTime.Now;
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
			offlineevrak.HataString = "SQL Bağlantı Hatası";
		}
		return offlineevrak;
	}

	public OfflineEvrakV2 EvrakKaydetV2(OfflineEvrakV2 offlineevrak, SqlBaglantiBilgileri baglantibilgileri, string MikroFirmaDBName, string KullaniciAdi)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(MikroFirmaDBName);
		Parametreler parametreler = ParametrelerDefault.MobilKullanici(KullaniciAdi);
		ParametreData.ParametreOku(baglantibilgileri, MikroFirmaDBName, parametreler, "akilli", KullaniciAdi, "", "");
		bool flag = true;
		Evrak evrak = Evrak.ReadFromByteArray(offlineevrak.Evrak);
		if (parametreler._GetParametre("SadeceBuguneEvrakGirebilir")._GetBoolean)
		{
			DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			DateTime dateTime2 = new DateTime(evrak.EvrakTarihi.Year, evrak.EvrakTarihi.Month, evrak.EvrakTarihi.Day);
			if (dateTime != dateTime2)
			{
				flag = false;
				offlineevrak.HataString = "Evrak Tarihi uygun değil. Sadece bugüne kayıt girilebilir.";
			}
		}
		bool getBoolean = parametreler._GetParametre("EvrakGirisiEArsivAktif")._GetBoolean;
		if (mikroVersiyon > 15)
		{
			if (evrak.sipariskarsilamami)
			{
				if (evrak.evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || evrak.evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
				{
					List<Guid> list = new List<Guid>();
					List<double> list2 = new List<double>();
					foreach (STOK_HAREKETLERI item in evrak.GetStokHareketleri())
					{
						if (!(item.sth_subesip_uid != Guid.Empty))
						{
							continue;
						}
						int num = 0;
						bool flag2 = false;
						foreach (Guid item2 in list)
						{
							if (item.sth_subesip_uid == item2)
							{
								list2[num] += item.sth_miktar;
								flag2 = true;
							}
							num++;
						}
						if (!flag2)
						{
							list.Add(item.sth_subesip_uid);
							list2.Add(item.sth_miktar);
						}
					}
					int num2 = 0;
					foreach (Guid item3 in list)
					{
						double num3 = list2[num2];
						double depolarArasiSiparisKalanMiktar = DepolarArasiSiparislerData.GetDepolarArasiSiparisKalanMiktar(baglantibilgileri, MikroFirmaDBName, item3);
						if (num3 > depolarArasiSiparisKalanMiktar)
						{
							flag = false;
							offlineevrak.HataString = "Seçtiğiniz siparişler karşılanmış.";
							break;
						}
						num2++;
					}
				}
				else
				{
					List<Guid> list3 = new List<Guid>();
					List<double> list4 = new List<double>();
					foreach (STOK_HAREKETLERI item4 in evrak.GetStokHareketleri())
					{
						if (!(item4.sth_sip_uid != Guid.Empty))
						{
							continue;
						}
						int num4 = 0;
						bool flag3 = false;
						foreach (Guid item5 in list3)
						{
							if (item4.sth_sip_uid == item5)
							{
								list4[num4] += item4.sth_miktar;
								flag3 = true;
							}
							num4++;
						}
						if (!flag3)
						{
							list3.Add(item4.sth_sip_uid);
							list4.Add(item4.sth_miktar);
						}
					}
					int num5 = 0;
					foreach (Guid item6 in list3)
					{
						double num6 = list4[num5];
						double siparisKalanMiktar = SiparislerData.GetSiparisKalanMiktar(baglantibilgileri, MikroFirmaDBName, item6);
						if (num6 > siparisKalanMiktar)
						{
							flag = false;
							offlineevrak.HataString = "Seçtiğiniz siparişler karşılanmış.";
							break;
						}
						num5++;
					}
				}
			}
		}
		else if (evrak.sipariskarsilamami)
		{
			if (evrak.evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || evrak.evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
			{
				List<int> list5 = new List<int>();
				List<double> list6 = new List<double>();
				foreach (STOK_HAREKETLERI item7 in evrak.GetStokHareketleri())
				{
					if (item7.sth_subesip_recid_recno == 0)
					{
						continue;
					}
					int num7 = 0;
					bool flag4 = false;
					foreach (int item8 in list5)
					{
						if (item7.sth_subesip_recid_recno == item8)
						{
							list6[num7] += item7.sth_miktar;
							flag4 = true;
						}
						num7++;
					}
					if (!flag4)
					{
						list5.Add(item7.sth_subesip_recid_recno);
						list6.Add(item7.sth_miktar);
					}
				}
				int num8 = 0;
				foreach (int item9 in list5)
				{
					double num9 = list6[num8];
					double depolarArasiSiparisKalanMiktar2 = DepolarArasiSiparislerData.GetDepolarArasiSiparisKalanMiktar(baglantibilgileri, MikroFirmaDBName, item9);
					if (num9 > depolarArasiSiparisKalanMiktar2)
					{
						flag = false;
						offlineevrak.HataString = "Seçtiğiniz siparişler karşılanmış.";
						break;
					}
					num8++;
				}
			}
			else
			{
				List<int> list7 = new List<int>();
				List<double> list8 = new List<double>();
				foreach (STOK_HAREKETLERI item10 in evrak.GetStokHareketleri())
				{
					if (item10.sth_sip_recid_recno == 0)
					{
						continue;
					}
					int num10 = 0;
					bool flag5 = false;
					foreach (int item11 in list7)
					{
						if (item10.sth_sip_recid_recno == item11)
						{
							list8[num10] += item10.sth_miktar;
							flag5 = true;
						}
						num10++;
					}
					if (!flag5)
					{
						list7.Add(item10.sth_sip_recid_recno);
						list8.Add(item10.sth_miktar);
					}
				}
				int num11 = 0;
				foreach (int item12 in list7)
				{
					double num12 = list8[num11];
					double siparisKalanMiktar2 = SiparislerData.GetSiparisKalanMiktar(baglantibilgileri, MikroFirmaDBName, item12);
					if (num12 > siparisKalanMiktar2)
					{
						flag = false;
						offlineevrak.HataString = "Seçtiğiniz siparişler karşılanmış.";
						break;
					}
					num11++;
				}
			}
		}
		switch (evrak.evraktipi)
		{
		case enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama:
		{
			string text2 = "";
			int num14 = 0;
			SqlConnection sqlConnection3 = new SqlConnection();
			if (baglantibilgileri.SqlUserName == "")
			{
				sqlConnection3.ConnectionString = "Data Source=" + baglantibilgileri.SqlServer + "; Initial Catalog=" + MikroFirmaDBName + "; Trusted_Connection=true;";
			}
			else
			{
				sqlConnection3.ConnectionString = "Data Source=" + baglantibilgileri.SqlServer + "; Initial Catalog=" + MikroFirmaDBName + "; User Id=" + baglantibilgileri.SqlUserName + ";Password=" + baglantibilgileri.SqlPassword + ";";
			}
			sqlConnection3.Open();
			SqlTransaction sqlTransaction3 = sqlConnection3.BeginTransaction();
			try
			{
				List<STOK_HAREKETLERI> list9 = EvrakData.DepolarArasiNakliyeOnaylamaKaydet(sqlConnection3, sqlTransaction3, MikroFirmaDBName, evrak);
				if (list9.Count != 0)
				{
					Evrak evrak2 = new Evrak(enum_GenelEvrakTipleri.DepolarArasiSevk, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
					evrak2.SetAciklama1(evrak.aciklama1);
					evrak2.SetAciklama2(evrak.aciklama2);
					evrak2.SetAciklama3(evrak.aciklama3);
					evrak2.SetAciklama4(evrak.aciklama4);
					evrak2.SetAciklama5(evrak.aciklama5);
					evrak2.SetAciklama6(evrak.aciklama6);
					evrak2.SetAciklama7(evrak.aciklama7);
					evrak2.SetAciklama8(evrak.aciklama8);
					evrak2.SetAciklama9(evrak.aciklama9);
					evrak2.SetAciklama10(evrak.aciklama10);
					evrak2.aktarimdurumu = evrak.aktarimdurumu;
					evrak2.SetAlternatifDovizCinsi(evrak.alternatifdovizcinsi);
					evrak2.alternatifdovizkuru = evrak.alternatifdovizkuru;
					evrak2.SetBelgeNo(evrak.BelgeNo);
					evrak2.SetBelgeTarihi(evrak.BelgeTarihi);
					evrak2.cari = evrak.cari;
					evrak2.SetDegistirSpecialAlan1(evrak.degistirspecialalan1);
					evrak2.SetDegistirSpecialAlan2(evrak.degistirspecialalan2);
					evrak2.SetDegistirSpecialAlan3(evrak.degistirspecialalan3);
					evrak2.SetDovizCinsi(evrak.dovizcinsi);
					evrak2.SetEvrakNoSeri(evrak.EvrakNoSeri);
					evrak2.SetEvraknoSira(evrak.EvrakNoSira);
					evrak2.SetEvrakTarihi(evrak.EvrakTarihi);
					evrak2.SetFirma(evrak.Firma);
					evrak2.SetFiyatListesi(evrak.FiyatListesi);
					evrak2.SetKapamaHesapKodu(evrak.kapamahesapkodu);
					evrak2.kapamasekli = evrak.kapamasekli;
					evrak2.kur = evrak.kur;
					evrak2.SetMikroUserNo(evrak.mikrouserno);
					evrak2.SetOdemePlani(evrak.odemeplani);
					evrak2.SetOfflineRecNo(evrak.offlinerecno);
					evrak2.SetProje(evrak.proje);
					evrak2.SetSevkAdresNo(evrak.sevkadresno);
					evrak2.SetSevkTeslimTarihi(evrak.SevkTeslimTarihi);
					evrak2.SetSip_TeslimTuru(evrak.sip_teslimturu);
					evrak2.SetSorumlulukMerkezi(evrak.sorumlulukmerkezi);
					evrak2.SetSube(evrak.Sube);
					evrak2.SetTemsilciKodu(evrak.TemsilciKodu);
					evrak2.SetHedefDepo(evrak.NakliyeDepo);
					evrak2.SetKaynakDepo(evrak.HedefDepo);
					Parametreler parametreler2 = ParametrelerDefault.ForaMikro();
					ParametreData.ParametreOku(baglantibilgileri, MikroFirmaDBName, parametreler2, "ForaMikro", "", "", "");
					List<VergiTanimi> list10 = new List<VergiTanimi>();
					VergiTanimi vergiTanimi = new VergiTanimi();
					vergiTanimi.KisaAdi = parametreler2._GetParametre("Vergi0KisaAdi")._GetString;
					vergiTanimi.UzunAdi = parametreler2._GetParametre("Vergi0UzunAdi")._GetString;
					vergiTanimi.Yuzde = parametreler2._GetParametre("Vergi0Yuzde")._GetDouble;
					VergiTanimi vergiTanimi2 = new VergiTanimi();
					vergiTanimi2.KisaAdi = parametreler2._GetParametre("Vergi1KisaAdi")._GetString;
					vergiTanimi2.UzunAdi = parametreler2._GetParametre("Vergi1UzunAdi")._GetString;
					vergiTanimi2.Yuzde = parametreler2._GetParametre("Vergi1Yuzde")._GetDouble;
					VergiTanimi vergiTanimi3 = new VergiTanimi();
					vergiTanimi3.KisaAdi = parametreler2._GetParametre("Vergi2KisaAdi")._GetString;
					vergiTanimi3.UzunAdi = parametreler2._GetParametre("Vergi2UzunAdi")._GetString;
					vergiTanimi3.Yuzde = parametreler2._GetParametre("Vergi2Yuzde")._GetDouble;
					VergiTanimi vergiTanimi4 = new VergiTanimi();
					vergiTanimi4.KisaAdi = parametreler2._GetParametre("Vergi3KisaAdi")._GetString;
					vergiTanimi4.UzunAdi = parametreler2._GetParametre("Vergi3UzunAdi")._GetString;
					vergiTanimi4.Yuzde = parametreler2._GetParametre("Vergi3Yuzde")._GetDouble;
					VergiTanimi vergiTanimi5 = new VergiTanimi();
					vergiTanimi5.KisaAdi = parametreler2._GetParametre("Vergi4KisaAdi")._GetString;
					vergiTanimi5.UzunAdi = parametreler2._GetParametre("Vergi4UzunAdi")._GetString;
					vergiTanimi5.Yuzde = parametreler2._GetParametre("Vergi4Yuzde")._GetDouble;
					VergiTanimi vergiTanimi6 = new VergiTanimi();
					vergiTanimi6.KisaAdi = parametreler2._GetParametre("Vergi5KisaAdi")._GetString;
					vergiTanimi6.UzunAdi = parametreler2._GetParametre("Vergi5UzunAdi")._GetString;
					vergiTanimi6.Yuzde = parametreler2._GetParametre("Vergi5Yuzde")._GetDouble;
					VergiTanimi vergiTanimi7 = new VergiTanimi();
					vergiTanimi7.KisaAdi = parametreler2._GetParametre("Vergi6KisaAdi")._GetString;
					vergiTanimi7.UzunAdi = parametreler2._GetParametre("Vergi6UzunAdi")._GetString;
					vergiTanimi7.Yuzde = parametreler2._GetParametre("Vergi6Yuzde")._GetDouble;
					VergiTanimi vergiTanimi8 = new VergiTanimi();
					vergiTanimi8.KisaAdi = parametreler2._GetParametre("Vergi7KisaAdi")._GetString;
					vergiTanimi8.UzunAdi = parametreler2._GetParametre("Vergi7UzunAdi")._GetString;
					vergiTanimi8.Yuzde = parametreler2._GetParametre("Vergi7Yuzde")._GetDouble;
					VergiTanimi vergiTanimi9 = new VergiTanimi();
					vergiTanimi9.KisaAdi = parametreler2._GetParametre("Vergi8KisaAdi")._GetString;
					vergiTanimi9.UzunAdi = parametreler2._GetParametre("Vergi8UzunAdi")._GetString;
					vergiTanimi9.Yuzde = parametreler2._GetParametre("Vergi8Yuzde")._GetDouble;
					VergiTanimi vergiTanimi10 = new VergiTanimi();
					vergiTanimi10.KisaAdi = parametreler2._GetParametre("Vergi9KisaAdi")._GetString;
					vergiTanimi10.UzunAdi = parametreler2._GetParametre("Vergi9UzunAdi")._GetString;
					vergiTanimi10.Yuzde = parametreler2._GetParametre("Vergi9Yuzde")._GetDouble;
					VergiTanimi vergiTanimi11 = new VergiTanimi();
					vergiTanimi11.KisaAdi = parametreler2._GetParametre("Vergi10KisaAdi")._GetString;
					vergiTanimi11.UzunAdi = parametreler2._GetParametre("Vergi10UzunAdi")._GetString;
					vergiTanimi11.Yuzde = parametreler2._GetParametre("Vergi10Yuzde")._GetDouble;
					list10.Add(vergiTanimi);
					list10.Add(vergiTanimi2);
					list10.Add(vergiTanimi3);
					list10.Add(vergiTanimi4);
					list10.Add(vergiTanimi5);
					list10.Add(vergiTanimi6);
					list10.Add(vergiTanimi7);
					list10.Add(vergiTanimi8);
					list10.Add(vergiTanimi9);
					list10.Add(vergiTanimi10);
					list10.Add(vergiTanimi11);
					evrak2.Parametreler.vergitanimlari = list10;
					AppBase._vergitanimlari = list10;
					foreach (STOK_HAREKETLERI item13 in list9)
					{
						Stok stok = StokData.GetStok(baglantibilgileri, MikroFirmaDBName, item13.sth_stok_kod, evrak2.GetToptanPerakende());
						stok.ekleme_bilgileri.BirimFiyat = new FiyatTanimlamasi();
						stok.ekleme_bilgileri.BirimFiyat.FiyatBrut = item13.sth_tutar / item13.sth_miktar;
						stok.ekleme_bilgileri.Miktar = item13.sth_miktar - item13.OkutulanMiktar;
						stok.ekleme_bilgileri.proje_kodu = evrak2.proje.pro_kodu;
						stok.ekleme_bilgileri.sorumluluk_merkezi_kodu = evrak2.sorumlulukmerkezi.som_kod;
						evrak2.AddUrun(stok);
					}
					num14 = EvrakData.EvrakKaydet(sqlConnection3, sqlTransaction3, MikroFirmaDBName, evrak2, getBoolean);
					if (num14 == -1 || num14 == -2)
					{
						if (num14 == -1)
						{
							text2 = "SQL Bağlantı Hatası";
						}
						if (num14 == -2)
						{
							text2 = "Aynı seri ve sıra nolu başka bir evrak bulundu.";
						}
					}
				}
				sqlTransaction3.Commit();
			}
			catch (Exception ex3)
			{
				Log_Error(ex3.ToString());
				_ = "Hata: " + ex3.ToString();
				text2 = ex3.ToString();
				Console.WriteLine("HATA GERI ALINIYOR : " + ex3.ToString());
				sqlTransaction3.Rollback();
			}
			finally
			{
				sqlConnection3.Close();
			}
			if (text2 == "")
			{
				evrak.SetEvraknoSira(num14);
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Aktarildi;
				offlineevrak.AktarilmaTarihi = DateTime.Now;
				offlineevrak.HataString = "";
			}
			else
			{
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
				offlineevrak.HataString = text2;
			}
			break;
		}
		case enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama:
		{
			string text = "";
			int evraknoSira = 0;
			SqlConnection sqlConnection2 = new SqlConnection();
			if (baglantibilgileri.SqlUserName == "")
			{
				sqlConnection2.ConnectionString = "Data Source=" + baglantibilgileri.SqlServer + "; Initial Catalog=" + MikroFirmaDBName + "; Trusted_Connection=true;";
			}
			else
			{
				sqlConnection2.ConnectionString = "Data Source=" + baglantibilgileri.SqlServer + "; Initial Catalog=" + MikroFirmaDBName + "; User Id=" + baglantibilgileri.SqlUserName + ";Password=" + baglantibilgileri.SqlPassword + ";";
			}
			sqlConnection2.Open();
			SqlTransaction sqlTransaction2 = sqlConnection2.BeginTransaction();
			try
			{
				EvrakData.BarkodKontrolluFaturaOnaylamaKaydet(sqlConnection2, sqlTransaction2, MikroFirmaDBName, evrak);
				sqlTransaction2.Commit();
			}
			catch (Exception ex2)
			{
				Log_Error(ex2.ToString());
				_ = "Hata: " + ex2.ToString();
				text = ex2.ToString();
				Console.WriteLine("HATA GERI ALINIYOR : " + ex2.ToString());
				sqlTransaction2.Rollback();
			}
			finally
			{
				sqlConnection2.Close();
			}
			if (text == "")
			{
				evrak.SetEvraknoSira(evraknoSira);
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Aktarildi;
				offlineevrak.AktarilmaTarihi = DateTime.Now;
				offlineevrak.HataString = "";
			}
			else
			{
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Hatali;
				offlineevrak.HataString = text;
			}
			break;
		}
		default:
		{
			if (!flag)
			{
				break;
			}
			int num13 = -1;
			if (offlineevrak.YeniKayit)
			{
				SqlConnection sqlConnection = new SqlConnection();
				if (baglantibilgileri.SqlUserName == "")
				{
					sqlConnection.ConnectionString = "Data Source=" + baglantibilgileri.SqlServer + "; Initial Catalog=" + MikroFirmaDBName + "; Trusted_Connection=true;";
				}
				else
				{
					sqlConnection.ConnectionString = "Data Source=" + baglantibilgileri.SqlServer + "; Initial Catalog=" + MikroFirmaDBName + "; User Id=" + baglantibilgileri.SqlUserName + ";Password=" + baglantibilgileri.SqlPassword + ";";
				}
				sqlConnection.Open();
				SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
				try
				{
					num13 = EvrakData.EvrakKaydet(sqlConnection, sqlTransaction, MikroFirmaDBName, evrak, getBoolean);
					sqlTransaction.Commit();
				}
				catch (Exception ex)
				{
					Log_Error(ex.ToString());
					_ = "Hata: " + ex.ToString();
					num13 = -1;
					Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
					sqlTransaction.Rollback();
				}
				finally
				{
					sqlConnection.Close();
				}
			}
			if (num13 == -1 || num13 == -2)
			{
				if (num13 == -1)
				{
					offlineevrak.HataString = "SQL Bağlantı Hatası";
				}
				if (num13 == -2)
				{
					offlineevrak.HataString = "Aynı seri ve sıra nolu başka bir evrak bulundu.";
				}
			}
			else
			{
				evrak.SetEvraknoSira(num13);
				offlineevrak.AktarimDurumu = enum_EvrakAktarimDurumu.Aktarildi;
				offlineevrak.AktarilmaTarihi = DateTime.Now;
				offlineevrak.HataString = "";
			}
			break;
		}
		}
		offlineevrak.Evrak = Evrak.WriteToByteArray(evrak, 999);
		return offlineevrak;
	}

	public void DataBaseKurulumu(SqlBaglantiBilgileri baglantibilgileri, string MikroFirmaDBName, string MikroAnaDBName)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantibilgileri, MikroFirmaDBName);
		SqlDB sqlDB2 = new SqlDB();
		sqlDB2.ConnectionOpen(baglantibilgileri, MikroAnaDBName);
		TabloOlusturSenkronizasyon(sqlDB);
		TabloOlusturZiyaretHareketleri(sqlDB);
		TabloOlusturTemsilciGunlukHareketler(sqlDB);
		List<Tablo> mikroV14DefaultTablolar = TabloHelper.GetMikroV14DefaultTablolar();
		if (MikroAnaDBName.Contains("MikroDB_V15"))
		{
			mikroV14DefaultTablolar.Add(TabloHelper.Get_MikroV15_Tablo_KUR_ISIMLERI());
		}
		Tablo mikroV14_Tablo__FORA_PARAMETRELER = TabloHelper.Get_MikroV14_Tablo__FORA_PARAMETRELER();
		mikroV14DefaultTablolar.Add(mikroV14_Tablo__FORA_PARAMETRELER);
		foreach (Tablo item in mikroV14DefaultTablolar)
		{
			if (item.TabloAdi == "YEREL_BANKA_KODLARI" && MikroFirmaDBName.StartsWith("MikroDB_V12"))
			{
				item.TabloAdi = "BANKA_TCMB_KODLARI";
			}
			if (item.TabloAdi == "DOVIZ_KURLARI" || item.TabloAdi == "YEREL_BANKA_KODLARI" || item.TabloAdi == "BANKA_TCMB_KODLARI" || item.TabloAdi == "KUR_ISIMLERI")
			{
				TriggerOlustur(item, sqlDB2, MikroFirmaDBName);
			}
			else
			{
				TriggerOlustur(item, sqlDB, MikroFirmaDBName);
			}
		}
		sqlDB2.ConnectionClose();
		sqlDB.ConnectionClose();
	}

	private void TriggerOlustur(Tablo tablo, SqlDB Db, string FirmaDBName)
	{
		string commandText = "IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = N'" + tablo.TabloAdi + "_FORA_SENKRONIZASYON' AND type = 'TR') BEGIN EXEC('  CREATE TRIGGER dbo." + tablo.TabloAdi + "_FORA_SENKRONIZASYON  ON " + tablo.TabloAdi + "  AFTER INSERT, UPDATE, DELETE  AS   BEGIN  SET NOCOUNT ON;  DECLARE @TabloID as int; SET @TabloID = " + tablo.TabloID + "; DECLARE @Action as tinyint; SET @Action = (CASE WHEN EXISTS(SELECT * FROM INSERTED WITH (NOLOCK)) AND EXISTS(SELECT * FROM DELETED WITH (NOLOCK)) THEN 1 WHEN EXISTS(SELECT * FROM INSERTED WITH (NOLOCK)) THEN 2 WHEN EXISTS(SELECT * FROM DELETED WITH (NOLOCK)) THEN 0 ELSE NULL END) IF @Action IS NULL return; IF EXISTS (SELECT * FROM inserted WITH (NOLOCK)) INSERT INTO " + FirmaDBName + ".dbo._FORA_SENKRONIZASYON SELECT @TabloID, " + tablo.GetRECnoFieldName() + ",GETDATE(),@Action FROM inserted WITH (NOLOCK) WHERE " + tablo.GetRECnoFieldName() + " not in (SELECT KayitRECno FROM " + FirmaDBName + ".dbo._FORA_SENKRONIZASYON WHERE Tarih>DATEADD(SECOND,-1,GETDATE()) AND TabloID=@TabloID AND KayitRECno=" + tablo.GetRECnoFieldName() + ") ELSE INSERT INTO " + FirmaDBName + ".dbo._FORA_SENKRONIZASYON SELECT @TabloID, " + tablo.GetRECnoFieldName() + ",GETDATE(),@Action FROM deleted WITH (NOLOCK) WHERE " + tablo.GetRECnoFieldName() + " not in (SELECT KayitRECno FROM " + FirmaDBName + ".dbo._FORA_SENKRONIZASYON WHERE Tarih>DATEADD(SECOND,-1,GETDATE()) AND TabloID=@TabloID AND KayitRECno=" + tablo.GetRECnoFieldName() + ") END  ')  SELECT 1 END ELSE SELECT 0";
		using SqlCommand sqlCommand = Db.Connection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.ExecuteScalar();
	}

	private void TabloOlusturZiyaretHareketleri(SqlDB Db)
	{
		try
		{
			string commandText = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[_ZIYARET_HAREKETLERI]')) BEGIN CREATE TABLE [dbo].[_ZIYARET_HAREKETLERI]([zyrt_RecNo] [int] IDENTITY(1,1) NOT NULL,[zyrt_Create_Date] [datetime] NOT NULL,[zyrt_Lastup_Date] [datetime] NOT NULL,[zyrt_iptal] [bit] NOT NULL,[zyrt_Tarihi] [datetime] NOT NULL,[zyrt_Temsilci_Kodu] [nvarchar](25) NOT NULL,[zyrt_Bolge_Kodu] [nvarchar](25) NOT NULL,[zyrt_Cari_Kodu] [nvarchar](25) NOT NULL,[zyrt_Cari_Adres_No] [int] NOT NULL,[zyrt_Cari_Adres_Enlem] [float] NOT NULL,[zyrt_Cari_Adres_Boylam] [float] NOT NULL,[zyrt_Baslama_Saati] [datetime] NOT NULL,[zyrt_Baslama_Enlem] [float] NOT NULL,[zyrt_Baslama_Boylam] [float] NOT NULL,[zyrt_Bitis_Saati] [datetime] NOT NULL,[zyrt_Bitis_Enlem] [float] NOT NULL,[zyrt_Bitis_Boylam] [float] NOT NULL,[zyrt_Tamamlandi] [bit] NOT NULL,[zyrt_Fotograf_Id] [int] NOT NULL,[zyrt_Proje_Kodu] [nvarchar](25) NOT NULL,[zyrt_Sor_Mer_Kodu] [nvarchar](25) NOT NULL,[zyrt_Bakim_Evrak_Seri] [nvarchar](6) NOT NULL,[zyrt_Bakim_Evrak_Sira] [int] NOT NULL,[zyrt_Satis_Yapildi] [bit] NOT NULL,[zyrt_Siparis_Alindi] [bit] NOT NULL,[zyrt_Urun_Teslim_Edildi] [bit] NOT NULL,[zyrt_Tahsilat_Yapildi] [bit] NOT NULL,[zyrt_Katalog_Birakildi] [bit] NOT NULL,[zyrt_Fiyat_Listesi_Birakildi] [bit] NOT NULL,[zyrt_Numune_Urun_Birakildi] [bit] NOT NULL,[zyrt_Konsinye_Urun_Birakildi] [bit] NOT NULL,[zyrt_Promosyon_Birakildi] [bit] NOT NULL,[zyrt_Firma_Durumu] [tinyint] NOT NULL,[zyrt_Rakip_Firma_Var] [bit] NOT NULL,[zyrt_Rakip_Firma_Durumu] [tinyint] NOT NULL,[zyrt_Urun_Yerlesim_Durumu] [tinyint] NOT NULL,[zyrt_Rakip_Urun_Yerlesim_Durumu] [tinyint] NOT NULL,[zyrt_Temsilci_Aciklama] [nvarchar](127) NOT NULL,[zyrt_Cari_Firma_Memnuniyeti] [tinyint] NOT NULL,[zyrt_Cari_Urun_Memnuniyeti] [tinyint] NOT NULL,[zyrt_Cari_Fiyat_Memnuniyeti] [tinyint] NOT NULL,[zyrt_Cari_Temsilci_Memnuniyeti] [tinyint] NOT NULL,[zyrt_Cari_Aciklama] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_01_Var_Yok] [bit] NOT NULL,[zyrt_Temsilci_Ozel_01_Evet_Hayir] [bit] NOT NULL,[zyrt_Temsilci_Ozel_01_Derece] [tinyint] NOT NULL,[zyrt_Temsilci_Ozel_01_TamSayi] [int] NOT NULL,[zyrt_Temsilci_Ozel_01_OndalikliSayi] [float] NOT NULL,[zyrt_Temsilci_Ozel_01_Metin] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_01_Fotograf_Id] [int] NOT NULL,[zyrt_Temsilci_Ozel_02_Var_Yok] [bit] NOT NULL,[zyrt_Temsilci_Ozel_02_Evet_Hayir] [bit] NOT NULL,[zyrt_Temsilci_Ozel_02_Derece] [tinyint] NOT NULL,[zyrt_Temsilci_Ozel_02_TamSayi] [int] NOT NULL,[zyrt_Temsilci_Ozel_02_OndalikliSayi] [float] NOT NULL,[zyrt_Temsilci_Ozel_02_Metin] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_02_Fotograf_Id] [int] NOT NULL,[zyrt_Temsilci_Ozel_03_Var_Yok] [bit] NOT NULL,[zyrt_Temsilci_Ozel_03_Evet_Hayir] [bit] NOT NULL,[zyrt_Temsilci_Ozel_03_Derece] [tinyint] NOT NULL,[zyrt_Temsilci_Ozel_03_TamSayi] [int] NOT NULL,[zyrt_Temsilci_Ozel_03_OndalikliSayi] [float] NOT NULL,[zyrt_Temsilci_Ozel_03_Metin] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_03_Fotograf_Id] [int] NOT NULL,[zyrt_Temsilci_Ozel_04_Var_Yok] [bit] NOT NULL,[zyrt_Temsilci_Ozel_04_Evet_Hayir] [bit] NOT NULL,[zyrt_Temsilci_Ozel_04_Derece] [tinyint] NOT NULL,[zyrt_Temsilci_Ozel_04_TamSayi] [int] NOT NULL,[zyrt_Temsilci_Ozel_04_OndalikliSayi] [float] NOT NULL,[zyrt_Temsilci_Ozel_04_Metin] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_04_Fotograf_Id] [int] NOT NULL,[zyrt_Temsilci_Ozel_05_Var_Yok] [bit] NOT NULL,[zyrt_Temsilci_Ozel_05_Evet_Hayir] [bit] NOT NULL,[zyrt_Temsilci_Ozel_05_Derece] [tinyint] NOT NULL,[zyrt_Temsilci_Ozel_05_TamSayi] [int] NOT NULL,[zyrt_Temsilci_Ozel_05_OndalikliSayi] [float] NOT NULL,[zyrt_Temsilci_Ozel_05_Metin] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_05_Fotograf_Id] [int] NOT NULL,[zyrt_Cari_Ozel_01_Var_Yok] [bit] NOT NULL,[zyrt_Cari_Ozel_01_Evet_Hayir] [bit] NOT NULL,[zyrt_Cari_Ozel_01_Derece] [tinyint] NOT NULL,[zyrt_Cari_Ozel_01_TamSayi] [int] NOT NULL,[zyrt_Cari_Ozel_01_OndalikliSayi] [float] NOT NULL,[zyrt_Cari_Ozel_01_Metin] [nvarchar](127) NOT NULL,[zyrt_Cari_Ozel_01_Fotograf_Id] [int] NOT NULL,[zyrt_Cari_Ozel_02_Var_Yok] [bit] NOT NULL,[zyrt_Cari_Ozel_02_Evet_Hayir] [bit] NOT NULL,[zyrt_Cari_Ozel_02_Derece] [tinyint] NOT NULL,[zyrt_Cari_Ozel_02_TamSayi] [int] NOT NULL,[zyrt_Cari_Ozel_02_OndalikliSayi] [float] NOT NULL,[zyrt_Cari_Ozel_02_Metin] [nvarchar](127) NOT NULL,[zyrt_Cari_Ozel_02_Fotograf_Id] [int] NOT NULL, CONSTRAINT [PK__ZIYARET_HAREKETLERI] PRIMARY KEY CLUSTERED  ( [zyrt_RecNo] ASC )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] ) ON [PRIMARY] CREATE NONCLUSTERED INDEX [01] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [02] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Temsilci_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [03] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Cari_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [04] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Cari_Kodu] ASC,[zyrt_Cari_Adres_No] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [05] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Temsilci_Kodu] ASC,[zyrt_Cari_Kodu] ASC,[zyrt_Cari_Adres_No] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [06] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Proje_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [07] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Sor_Mer_Kodu] ASC,[zyrt_Proje_Kodu] ASC,[zyrt_Tarihi] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [08] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Sor_Mer_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [09] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Bolge_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] SELECT 1 END ELSE SELECT 0";
			using (SqlCommand sqlCommand = Db.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.ExecuteScalar();
			}
			string commandText2 = "IF NOT EXISTS(SELECT *  FROM sys.columns WHERE Name = N'zyrt_Temsilci_Ozel_06_Var_Yok' AND Object_ID = Object_ID(N'[dbo].[_ZIYARET_HAREKETLERI]'))BEGIN ALTER TABLE [dbo].[_ZIYARET_HAREKETLERI] ADD [zyrt_Temsilci_Ozel_06_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_06_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_06_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_06_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_07_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_07_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_07_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_07_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_08_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_08_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_08_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_08_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_09_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_09_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_09_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_09_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_10_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_10_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_10_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_10_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_11_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_11_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_11_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_11_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_12_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_12_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_12_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_12_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_13_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_13_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_13_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_13_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_14_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_14_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_14_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_14_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_15_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_15_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_15_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_15_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_16_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_16_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_16_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_16_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_17_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_17_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_17_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_17_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_18_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_18_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_18_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_18_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_19_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_19_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_19_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_19_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_20_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_20_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_20_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_20_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_21_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_21_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_21_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_21_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_22_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_22_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_22_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_22_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_23_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_23_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_23_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_23_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_24_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_24_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_24_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_24_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_25_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_25_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_25_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_25_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_26_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_26_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_26_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_26_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_27_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_27_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_27_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_27_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_28_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_28_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_28_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_28_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_29_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_29_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_29_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_29_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_30_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_30_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_30_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_30_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_31_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_31_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_31_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_31_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_32_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_32_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_32_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_32_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_33_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_33_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_33_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_33_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_34_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_34_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_34_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_34_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_35_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_35_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_35_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_35_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_36_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_36_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_36_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_36_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_37_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_37_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_37_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_37_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_38_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_38_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_38_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_38_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_39_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_39_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_39_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_39_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_40_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_40_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_40_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_40_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_41_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_41_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_41_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_41_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_42_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_42_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_42_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_42_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_43_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_43_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_43_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_43_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_44_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_44_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_44_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_44_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_45_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_45_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_45_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_45_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_46_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_46_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_46_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_46_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_47_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_47_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_47_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_47_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_48_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_48_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_48_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_48_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_49_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_49_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_49_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_49_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_50_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_50_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_50_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_50_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_51_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_51_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_51_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_51_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_52_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_52_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_52_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_52_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_53_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_53_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_53_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_53_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_54_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_54_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_54_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_54_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_55_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_55_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_55_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_55_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_56_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_56_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_56_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_56_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_57_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_57_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_57_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_57_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_58_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_58_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_58_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_58_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_59_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_59_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_59_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_59_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_60_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_60_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_60_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_60_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_61_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_61_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_61_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_61_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_62_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_62_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_62_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_62_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_63_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_63_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_63_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_63_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_64_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_64_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_64_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_64_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_65_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_65_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_65_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_65_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_66_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_66_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_66_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_66_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_67_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_67_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_67_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_67_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_68_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_68_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_68_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_68_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_69_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_69_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_69_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_69_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_70_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_70_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_70_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_70_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_71_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_71_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_71_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_71_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_72_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_72_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_72_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_72_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_73_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_73_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_73_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_73_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_74_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_74_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_74_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_74_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_75_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_75_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_75_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_75_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_76_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_76_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_76_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_76_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_77_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_77_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_77_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_77_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_78_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_78_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_78_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_78_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_79_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_79_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_79_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_79_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_80_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_80_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_80_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_80_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_81_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_81_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_81_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_81_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_82_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_82_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_82_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_82_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_83_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_83_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_83_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_83_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_84_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_84_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_84_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_84_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_85_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_85_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_85_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_85_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_86_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_86_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_86_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_86_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_87_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_87_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_87_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_87_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_88_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_88_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_88_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_88_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_89_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_89_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_89_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_89_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_90_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_90_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_90_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_90_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_91_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_91_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_91_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_91_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_92_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_92_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_92_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_92_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_93_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_93_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_93_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_93_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_94_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_94_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_94_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_94_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_95_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_95_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_95_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_95_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_96_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_96_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_96_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_96_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_97_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_97_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_97_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_97_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_98_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_98_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_98_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_98_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_99_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_99_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_99_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_99_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_100_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_100_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_100_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_100_Metin] [nvarchar](127) NULL END";
			using SqlCommand sqlCommand2 = Db.Connection.CreateCommand();
			sqlCommand2.CommandText = commandText2;
			sqlCommand2.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			throw ex;
		}
	}

	private void TabloOlusturTemsilciGunlukHareketler(SqlDB Db)
	{
		string commandText = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[_TEMSILCI_GUNLUK_HAREKETLER]')) BEGIN CREATE TABLE [dbo].[_TEMSILCI_GUNLUK_HAREKETLER]([RecNO] [int] IDENTITY(1,1) NOT NULL,[Temsilci_Kodu] [nvarchar](25) NOT NULL,[Tarih] [datetime] NOT NULL,[Baslama_Yapildi] [bit] NOT NULL,[Baslama_Saati] [datetime] NOT NULL,[Baslama_Kayit_Saati] [datetime] NOT NULL,[Baslama_Enlem] [float] NOT NULL,[Baslama_Boylam] [float] NOT NULL,[Baslama_Arac_Km] [int] NOT NULL,[Baslama_Mesaj] [nvarchar](127) NOT NULL,[Ogle_Arasi_Baslama_Saati] [datetime] NOT NULL,[Ogle_Arasi_Bitis_Saati] [datetime] NOT NULL,[Bitis_Yapildi] [bit] NOT NULL,[Bitis_Saati] [datetime] NOT NULL,[Bitis_Kayit_Saati] [datetime] NOT NULL,[Bitis_Enlem] [float] NOT NULL,[Bitis_Boylam] [float] NOT NULL,[Bitis_Arac_Km] [int] NOT NULL,[Bitis_Mesaj] [nvarchar](127) NOT NULL,[Create_Date] [datetime] NOT NULL,[Lastup_Date] [datetime] NOT NULL,CONSTRAINT [PK__TEMSILCI_GUNLUK_HAREKETLER] PRIMARY KEY CLUSTERED ([RecNO] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY] CREATE NONCLUSTERED INDEX [01] ON [dbo].[_TEMSILCI_GUNLUK_HAREKETLER] ([Temsilci_Kodu] ASC,[Tarih] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [02] ON [dbo].[_TEMSILCI_GUNLUK_HAREKETLER] ([Tarih] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [03] ON [dbo].[_TEMSILCI_GUNLUK_HAREKETLER] ([Temsilci_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] SELECT 1 END ELSE SELECT 0";
		using SqlCommand sqlCommand = Db.Connection.CreateCommand();
		sqlCommand.CommandText = commandText;
		object obj = sqlCommand.ExecuteScalar();
		if (obj != null)
		{
			int.Parse(obj.ToString());
		}
	}

	private void TabloOlusturSenkronizasyon(SqlDB Db)
	{
		string commandText = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[_FORA_SENKRONIZASYON]')) BEGIN CREATE TABLE [dbo].[_FORA_SENKRONIZASYON]([TriggerRECno] [int] IDENTITY(1,1) NOT NULL,[TabloID] [int] NOT NULL,[KayitRECno] [int] NOT NULL,[Tarih] [datetime] NOT NULL,[Islem] [tinyint] NOT NULL, CONSTRAINT [PK_Table_1] PRIMARY KEY CLUSTERED  ([TriggerRECno] ASC ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] ) ON [PRIMARY] CREATE NONCLUSTERED INDEX [01] ON [dbo].[_FORA_SENKRONIZASYON] ([TriggerRECno] ASC,[TabloID] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [02] ON [dbo].[_FORA_SENKRONIZASYON] ([TriggerRECno] ASC,[TabloID] ASC,[Islem] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] SELECT 1 END ELSE SELECT 0";
		using SqlCommand sqlCommand = Db.Connection.CreateCommand();
		sqlCommand.CommandText = commandText;
		object obj = sqlCommand.ExecuteScalar();
		if (obj != null)
		{
			int.Parse(obj.ToString());
		}
	}

	private static void SqlReaderStreamYaz(DataTable TabloYapisi, BinaryWriter writer, SqlDataReader r)
	{
		long position = writer.BaseStream.Position;
		int value = 1;
		writer.Write(value);
		int num = 0;
		while (r.Read())
		{
			for (int i = 0; i < TabloYapisi.Columns.Count; i++)
			{
				if (TabloYapisi.Columns[i].DataType == typeof(string))
				{
					writer.Write(r.GetSafeString(i));
				}
				else if (TabloYapisi.Columns[i].DataType == typeof(double))
				{
					writer.Write(r.GetSafeDouble(i).ToString(CultureInfo.InvariantCulture));
				}
				else if (TabloYapisi.Columns[i].DataType == typeof(DateTime))
				{
					writer.Write(r.GetSafeDateTime(i).Ticks);
				}
				else if (TabloYapisi.Columns[i].DataType == typeof(byte))
				{
					if (!r.IsDBNull(i))
					{
						writer.Write(r.GetByte(i));
						continue;
					}
					byte value2 = 0;
					writer.Write(value2);
				}
				else if (TabloYapisi.Columns[i].DataType == typeof(short))
				{
					if (!r.IsDBNull(i))
					{
						writer.Write(r.GetInt16(i));
						continue;
					}
					short value3 = 0;
					writer.Write(value3);
				}
				else if (TabloYapisi.Columns[i].DataType == typeof(int))
				{
					writer.Write(r.GetSafeInt32(i));
				}
				else if (TabloYapisi.Columns[i].DataType == typeof(long))
				{
					writer.Write(r.GetSafeInt64(i));
				}
				else if (TabloYapisi.Columns[i].DataType == typeof(bool))
				{
					writer.Write(r.GetSafeBoolean(i));
				}
				else
				{
					writer.Write(r[i].ToString());
				}
			}
			num++;
		}
		writer.BaseStream.Seek(position, SeekOrigin.Begin);
		writer.Write(num);
		writer.BaseStream.Seek(0L, SeekOrigin.End);
	}

	private static byte[] ReadToEnd(Stream stream)
	{
		long position = 0L;
		if (stream.CanSeek)
		{
			position = stream.Position;
			stream.Position = 0L;
		}
		try
		{
			byte[] array = new byte[4096];
			int num = 0;
			int num2;
			while ((num2 = stream.Read(array, num, array.Length - num)) > 0)
			{
				num += num2;
				if (num == array.Length)
				{
					int num3 = stream.ReadByte();
					if (num3 != -1)
					{
						byte[] array2 = new byte[array.Length * 2];
						Buffer.BlockCopy(array, 0, array2, 0, array.Length);
						Buffer.SetByte(array2, num, (byte)num3);
						array = array2;
						num++;
					}
				}
			}
			byte[] array3 = array;
			if (array.Length != num)
			{
				array3 = new byte[num];
				Buffer.BlockCopy(array, 0, array3, 0, num);
			}
			return array3;
		}
		finally
		{
			if (stream.CanSeek)
			{
				stream.Position = position;
			}
		}
	}

	private static void Log_Error(string message)
	{
		try
		{
			EventLog eventLog = new EventLog("ForaMikroService");
			eventLog.Source = "ForaMikroService";
			eventLog.WriteEntry(message, EventLogEntryType.Error);
		}
		catch
		{
		}
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			if (!Directory.Exists(text + "data"))
			{
				Directory.CreateDirectory(text + "data");
			}
			if (!Directory.Exists(text + "data\\logs"))
			{
				Directory.CreateDirectory(text + "data\\logs");
			}
			if (!File.Exists(text + "data\\logs\\errorlog.log"))
			{
				FileStream fileStream = File.Create(text + "data\\logs\\errorlog.log");
				fileStream.Close();
				fileStream.Dispose();
			}
			StreamWriter streamWriter = File.AppendText(text + "data\\logs\\errorlog.log");
			streamWriter.WriteLine(message);
			streamWriter.Close();
			streamWriter.Dispose();
		}
		catch
		{
		}
	}

	private static void Log_Evrak(string kullanici_adi, OfflineEvrakV2 Content)
	{
		try
		{
			string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
			if (!Directory.Exists(text + "data"))
			{
				Directory.CreateDirectory(text + "data");
			}
			if (!Directory.Exists(text + "data\\logs"))
			{
				Directory.CreateDirectory(text + "data\\logs");
			}
			if (!Directory.Exists(text + "data\\logs\\evrak"))
			{
				Directory.CreateDirectory(text + "data\\logs\\evrak");
			}
			FileStream fileStream = File.Create(text + "data\\logs\\evrak\\" + kullanici_adi + "_" + DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + "_" + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + "." + DateTime.Now.Millisecond + ".fme");
			OfflineEvrakV2.WriteToStream(Content, fileStream, 1);
			fileStream.Close();
		}
		catch
		{
		}
	}

	Message ITeknikTesisAdmin.Login(string firmaid, string UserName, string Password)
	{
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail,cari_wwwadresi FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
				userTeknikTesisBakimYonetimiAdmin.rol = sqlDataReader.GetSafeString(5);
				if (userTeknikTesisBakimYonetimiAdmin.rol == "")
				{
					userTeknikTesisBakimYonetimiAdmin.rol = "ekipsefi";
				}
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		string text2 = JsonConvert.SerializeObject(userTeknikTesisBakimYonetimiAdmin, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.GetBitirilecekIsler(string firmaid, string UserName, string Password)
	{
		Console.WriteLine("GetBitirilecekIsler Cagrildi");
		List<BAKIM_KABUL_HAREKETLERI> list = new List<BAKIM_KABUL_HAREKETLERI>();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		string text2 = "";
		string[] array = userTeknikTesisBakimYonetimiAdmin.kurumlar.Split(',');
		bool flag = true;
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			if (!flag)
			{
				text2 += ",";
				flag = false;
			}
			text2 = text2 + "'" + text3 + "'";
		}
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			string commandText2 = "SELECT b.bkmkb_RECno,b.bkmkb_tarihi,b.bkmkb_evrakno_seri,b.bkmkb_evrakno_sira,cihaz.chz_aciklama1 + ' ' + cihaz.chz_aciklama2 + ' ' + cihaz.chz_aciklama3 AS ariza_yeri,b.bkmkb_tuketici_kodu,s.tuk_ismi,b.bkmkb_bildirilen_arizalar,b.bkmkb_teslim_alinma_tarihi,b.bkmkb_ariza_kodu1,c.chs_sorun,b.bkmkb_inceleyecek_ekip_kodu,e.ekp_adi,b.bkmkb_stok_hizmet_kodu,b.bkmkb_fis_stok_kodu FROM BAKIM_KABUL_HAREKETLERI AS b INNER JOIN SON_KULLANICILAR AS s ON b.bkmkb_tuketici_kodu=s.tuk_kodu INNER JOIN CIHAZ_SORUNLARI AS c ON b.bkmkb_ariza_kodu1=c.chs_kodu INNER JOIN EKIP_TANIMLARI AS e ON b.bkmkb_inceleyecek_ekip_kodu=e.ekp_kodu INNER JOIN STOK_SERINO_TANIMLARI AS cihaz ON b.bkmkb_cihaz_serino=cihaz.chz_serino WHERE b.bkmkb_inceleyecek_ekip_kodu<>'' AND bkmkb_planlandi_fl=0 AND b.bkmkb_tuketici_kodu in (SELECT tuk_kodu FROM SON_KULLANICILAR WHERE tuk_cari_kodu in (" + text2 + "))";
			sqlCommand2.CommandText = commandText2;
			SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
			while (sqlDataReader2.Read())
			{
				BAKIM_KABUL_HAREKETLERI bAKIM_KABUL_HAREKETLERI = new BAKIM_KABUL_HAREKETLERI();
				bAKIM_KABUL_HAREKETLERI.bkmkb_RECno = sqlDataReader2.GetSafeInt32(0);
				bAKIM_KABUL_HAREKETLERI.bkmkb_tarihi = sqlDataReader2.GetSafeDateTime(1);
				bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_seri = sqlDataReader2.GetSafeString(2);
				bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_sira = sqlDataReader2.GetSafeInt32(3);
				bAKIM_KABUL_HAREKETLERI.bkmkb_cihaz_serino = sqlDataReader2.GetSafeString(4);
				bAKIM_KABUL_HAREKETLERI.bkmkb_tuketici_kodu = sqlDataReader2.GetSafeString(5);
				bAKIM_KABUL_HAREKETLERI.TuketiciAdi = sqlDataReader2.GetSafeString(6);
				bAKIM_KABUL_HAREKETLERI.bkmkb_bildirilen_arizalar = sqlDataReader2.GetSafeString(7);
				bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_alinma_tarihi = sqlDataReader2.GetSafeDateTime(8);
				bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu1 = sqlDataReader2.GetSafeString(9);
				bAKIM_KABUL_HAREKETLERI.ArizaAdi1 = sqlDataReader2.GetSafeString(10);
				bAKIM_KABUL_HAREKETLERI.bkmkb_inceleyecek_ekip_kodu = sqlDataReader2.GetSafeString(11);
				bAKIM_KABUL_HAREKETLERI.EkipAdi = sqlDataReader2.GetSafeString(12);
				bAKIM_KABUL_HAREKETLERI.bkmkb_stok_hizmet_kodu = sqlDataReader2.GetSafeString(13);
				bAKIM_KABUL_HAREKETLERI.bkmkb_fis_stok_kodu = sqlDataReader2.GetSafeString(14);
				list.Add(bAKIM_KABUL_HAREKETLERI);
			}
			sqlDataReader2.Close();
		}
		sqlDB.ConnectionClose();
		string text4 = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text4, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.GetStoklarSarf(string firmaid, string UserName, string Password)
	{
		Console.WriteLine("GetStoklarSarf Cagrildi");
		DataTable dataTable = new DataTable("STOKLAR");
		dataTable.Columns.Add("sto_kod", typeof(string));
		dataTable.Columns.Add("sto_isim", typeof(string));
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			string commandText2 = "SELECT sto_kod,sto_isim,sto_birim1_ad FROM STOKLAR WITH(NOLOCK) WHERE sto_anagrup_kod='SARF'";
			sqlCommand2.CommandText = commandText2;
			SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
			while (sqlDataReader2.Read())
			{
				dataTable.Rows.Add(sqlDataReader2.GetSafeString(0), sqlDataReader2.GetSafeString(1), sqlDataReader2.GetSafeString(2));
			}
			sqlDataReader2.Close();
		}
		sqlDB.ConnectionClose();
		string text2 = JsonConvert.SerializeObject(dataTable, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.IsBitir(string firmaid, string UserName, string Password, Stream Content)
	{
		Console.WriteLine("IsBitir Cagrildi");
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		BAKIM_KABUL_HAREKETLERI bAKIM_KABUL_HAREKETLERI = JsonConvert.DeserializeObject<BAKIM_KABUL_HAREKETLERI>(new StreamReader(Content).ReadToEnd());
		int num = 1;
		string cari_kod = "";
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			string commandText2 = "SELECT cari_VarsayilanCikisDepo,cari_kod FROM CARI_HESAPLAR WHERE cari_kod = (SELECT TOP 1 tuk_cari_kodu FROM SON_KULLANICILAR WHERE tuk_kodu=@tuk_kodu)";
			sqlCommand2.CommandText = commandText2;
			sqlCommand2.Parameters.AddWithValue("tuk_kodu", bAKIM_KABUL_HAREKETLERI.bkmkb_tuketici_kodu);
			SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
			while (sqlDataReader2.Read())
			{
				num = sqlDataReader2.GetSafeInt32(0);
				cari_kod = sqlDataReader2.GetSafeString(1);
			}
			sqlDataReader2.Close();
		}
		SqlTransaction sqlTransaction = sqlDB.Connection.BeginTransaction();
		try
		{
			List<BAKIM_HAREKETLERI> list = new List<BAKIM_HAREKETLERI>();
			BAKIM_HAREKETLERI bAKIM_HAREKETLERI = new BAKIM_HAREKETLERI();
			bAKIM_HAREKETLERI.bkm_RECno = 0;
			bAKIM_HAREKETLERI.bkm_RECid_DBCno = 0;
			bAKIM_HAREKETLERI.bkm_RECid_RECno = 0;
			bAKIM_HAREKETLERI.bkm_Spec_Rec_no = 0;
			bAKIM_HAREKETLERI.bkm_iptal = false;
			bAKIM_HAREKETLERI.bkm_fileid = 97;
			bAKIM_HAREKETLERI.bkm_hidden = false;
			bAKIM_HAREKETLERI.bkm_kilitli = false;
			bAKIM_HAREKETLERI.bkm_degisti = false;
			bAKIM_HAREKETLERI.bkm_checksum = 0;
			bAKIM_HAREKETLERI.bkm_create_user = 1;
			bAKIM_HAREKETLERI.bkm_create_date = DateTime.Now;
			bAKIM_HAREKETLERI.bkm_lastup_user = 1;
			bAKIM_HAREKETLERI.bkm_lastup_date = DateTime.Now;
			bAKIM_HAREKETLERI.bkm_special1 = "FORA";
			bAKIM_HAREKETLERI.bkm_special2 = "";
			bAKIM_HAREKETLERI.bkm_special3 = "";
			bAKIM_HAREKETLERI.bkm_firmano = 0;
			bAKIM_HAREKETLERI.bkm_subeno = 0;
			bAKIM_HAREKETLERI.bkm_tarihi = DateTime.Now;
			bAKIM_HAREKETLERI.bkm_evrakno_seri = bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_seri;
			bAKIM_HAREKETLERI.bkm_evrakno_sira = bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_sira;
			bAKIM_HAREKETLERI.bkm_satirno = 0;
			bAKIM_HAREKETLERI.bkm_belgeno = "";
			bAKIM_HAREKETLERI.bkm_belge_tarihi = DateTime.Now;
			bAKIM_HAREKETLERI.bkm_tuketici_kodu = bAKIM_KABUL_HAREKETLERI.bkmkb_tuketici_kodu;
			bAKIM_HAREKETLERI.bkm_cihaz_serino = bAKIM_KABUL_HAREKETLERI.bkmkb_cihaz_serino;
			bAKIM_HAREKETLERI.bkm_fis_stok_kodu = bAKIM_KABUL_HAREKETLERI.bkmkb_fis_stok_kodu;
			bAKIM_HAREKETLERI.bkm_teslim_alinma_tarihi = DateTime.Now;
			bAKIM_HAREKETLERI.bkm_teslim_edilme_tarihi = DateTime.Now;
			bAKIM_HAREKETLERI.bkm_teslim_edilme_sekli = 1;
			bAKIM_HAREKETLERI.bkm_ariza_kodu1 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu1;
			bAKIM_HAREKETLERI.bkm_ariza_kodu2 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu2;
			bAKIM_HAREKETLERI.bkm_ariza_kodu3 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu3;
			bAKIM_HAREKETLERI.bkm_ariza_kodu4 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu4;
			bAKIM_HAREKETLERI.bkm_ariza_kodu5 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu5;
			bAKIM_HAREKETLERI.bkm_ariza_kodu6 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu6;
			bAKIM_HAREKETLERI.bkm_ariza_kodu7 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu7;
			bAKIM_HAREKETLERI.bkm_ariza_kodu8 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu8;
			bAKIM_HAREKETLERI.bkm_ariza_kodu9 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu9;
			bAKIM_HAREKETLERI.bkm_ariza_kodu10 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu10;
			bAKIM_HAREKETLERI.bkm_ekip_kodu = bAKIM_KABUL_HAREKETLERI.bkmkb_inceleyecek_ekip_kodu;
			bAKIM_HAREKETLERI.bkm_depono = num;
			bAKIM_HAREKETLERI.bkm_aciklama = bAKIM_KABUL_HAREKETLERI.bkmkb_aciklama;
			bAKIM_HAREKETLERI.bkm_hareket_tipi = 1;
			bAKIM_HAREKETLERI.bkm_stok_hizmet_kodu = bAKIM_KABUL_HAREKETLERI.bkmkb_stok_hizmet_kodu;
			bAKIM_HAREKETLERI.bkm_miktari = 1.0;
			bAKIM_HAREKETLERI.bkm_birim_fiyati = 0.0;
			bAKIM_HAREKETLERI.bkm_tutari = 0.0;
			bAKIM_HAREKETLERI.bkm_iskonto1 = 0.0;
			bAKIM_HAREKETLERI.bkm_iskonto2 = 0.0;
			bAKIM_HAREKETLERI.bkm_iskonto3 = 0.0;
			bAKIM_HAREKETLERI.bkm_iskonto4 = 0.0;
			bAKIM_HAREKETLERI.bkm_iskonto5 = 0.0;
			bAKIM_HAREKETLERI.bkm_iskonto6 = 0.0;
			bAKIM_HAREKETLERI.bkm_masraf1 = 0.0;
			bAKIM_HAREKETLERI.bkm_masraf2 = 0.0;
			bAKIM_HAREKETLERI.bkm_masraf3 = 0.0;
			bAKIM_HAREKETLERI.bkm_masraf4 = 0.0;
			bAKIM_HAREKETLERI.bkm_vergi_pntr = 0;
			bAKIM_HAREKETLERI.bkm_vergi = 4.0;
			bAKIM_HAREKETLERI.bkm_masraf_vergi_pnt = 0;
			bAKIM_HAREKETLERI.bkm_masraf_vergi = 0.0;
			bAKIM_HAREKETLERI.bkm_isk_mas1 = 0;
			bAKIM_HAREKETLERI.bkm_isk_mas2 = 0;
			bAKIM_HAREKETLERI.bkm_isk_mas3 = 0;
			bAKIM_HAREKETLERI.bkm_isk_mas4 = 0;
			bAKIM_HAREKETLERI.bkm_isk_mas5 = 0;
			bAKIM_HAREKETLERI.bkm_isk_mas6 = 0;
			bAKIM_HAREKETLERI.bkm_isk_mas7 = 0;
			bAKIM_HAREKETLERI.bkm_isk_mas8 = 0;
			bAKIM_HAREKETLERI.bkm_isk_mas9 = 0;
			bAKIM_HAREKETLERI.bkm_isk_mas10 = 0;
			bAKIM_HAREKETLERI.bkm_sat_isk_mas1 = false;
			bAKIM_HAREKETLERI.bkm_sat_isk_mas2 = false;
			bAKIM_HAREKETLERI.bkm_sat_isk_mas3 = false;
			bAKIM_HAREKETLERI.bkm_sat_isk_mas4 = false;
			bAKIM_HAREKETLERI.bkm_sat_isk_mas5 = false;
			bAKIM_HAREKETLERI.bkm_sat_isk_mas6 = false;
			bAKIM_HAREKETLERI.bkm_sat_isk_mas7 = false;
			bAKIM_HAREKETLERI.bkm_sat_isk_mas8 = false;
			bAKIM_HAREKETLERI.bkm_sat_isk_mas9 = false;
			bAKIM_HAREKETLERI.bkm_sat_isk_mas10 = false;
			bAKIM_HAREKETLERI.bkm_doviz_cins = 0;
			bAKIM_HAREKETLERI.bkm_doviz_kur = 1.0;
			bAKIM_HAREKETLERI.bkm_alt_doviz_kur = 0.0;
			bAKIM_HAREKETLERI.bkm_vergisiz_fl = false;
			bAKIM_HAREKETLERI.bkm_satir_aciklama = "";
			bAKIM_HAREKETLERI.bkm_faturalandi_fl = false;
			bAKIM_HAREKETLERI.bkm_ziyaret_kodu = "";
			bAKIM_HAREKETLERI.bkm_ziy_ac_tar = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_ziy_cik_zmn = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_ziy_bas_zmn = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_ziy_son_zmn = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_ziy_don_zmn = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_kabul_RECid_DBCno = 0;
			bAKIM_HAREKETLERI.bkm_kabul_RECid_RECno = bAKIM_KABUL_HAREKETLERI.bkmkb_RECno;
			bAKIM_HAREKETLERI.bkm_isemri_RECid_DBCno = 0;
			bAKIM_HAREKETLERI.bkm_isemri_RECid_RECno = 0;
			bAKIM_HAREKETLERI.bkm_cihazdurumbastarihi1 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumbittarihi1 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumkodu1 = "";
			bAKIM_HAREKETLERI.bkm_cihazserviselemanikodu1 = "";
			bAKIM_HAREKETLERI.bkm_cihazdurumbastarihi2 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumbittarihi2 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumkodu2 = "";
			bAKIM_HAREKETLERI.bkm_cihazserviselemanikodu2 = "";
			bAKIM_HAREKETLERI.bkm_cihazdurumbastarihi3 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumbittarihi3 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumkodu3 = "";
			bAKIM_HAREKETLERI.bkm_cihazserviselemanikodu3 = "";
			bAKIM_HAREKETLERI.bkm_cihazdurumbastarihi4 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumbittarihi4 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumkodu4 = "";
			bAKIM_HAREKETLERI.bkm_cihazserviselemanikodu4 = "";
			bAKIM_HAREKETLERI.bkm_cihazdurumbastarihi5 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumbittarihi5 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumkodu5 = "";
			bAKIM_HAREKETLERI.bkm_cihazserviselemanikodu5 = "";
			bAKIM_HAREKETLERI.bkm_cihazdurumbastarihi6 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumbittarihi6 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumkodu6 = "";
			bAKIM_HAREKETLERI.bkm_cihazserviselemanikodu6 = "";
			bAKIM_HAREKETLERI.bkm_cihazdurumbastarihi7 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumbittarihi7 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumkodu7 = "";
			bAKIM_HAREKETLERI.bkm_cihazserviselemanikodu7 = "";
			bAKIM_HAREKETLERI.bkm_cihazdurumbastarihi8 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumbittarihi8 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumkodu8 = "";
			bAKIM_HAREKETLERI.bkm_cihazserviselemanikodu8 = "";
			bAKIM_HAREKETLERI.bkm_cihazdurumbastarihi9 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumbittarihi9 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumkodu9 = "";
			bAKIM_HAREKETLERI.bkm_cihazserviselemanikodu9 = "";
			bAKIM_HAREKETLERI.bkm_cihazdurumbastarihi10 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumbittarihi10 = new DateTime(1900, 1, 1);
			bAKIM_HAREKETLERI.bkm_cihazdurumkodu10 = "";
			bAKIM_HAREKETLERI.bkm_cihazserviselemanikodu10 = "";
			bAKIM_HAREKETLERI.bkm_fiyat_liste_no = 0;
			bAKIM_HAREKETLERI.bkm_parti_kodu = "";
			bAKIM_HAREKETLERI.bkm_lot_no = 0;
			bAKIM_HAREKETLERI.bkm_servis_turu = 0;
			bAKIM_HAREKETLERI.bkm_prj_kodu = "";
			bAKIM_HAREKETLERI.bkm_srm_kodu = "";
			bAKIM_HAREKETLERI.bkm_adres_no = 0;
			list.Add(bAKIM_HAREKETLERI);
			int num2 = 1;
			foreach (StokBase item in bAKIM_KABUL_HAREKETLERI.KullanilanStoklar)
			{
				BAKIM_HAREKETLERI bAKIM_HAREKETLERI2 = new BAKIM_HAREKETLERI();
				bAKIM_HAREKETLERI2.bkm_RECno = 0;
				bAKIM_HAREKETLERI2.bkm_RECid_DBCno = 0;
				bAKIM_HAREKETLERI2.bkm_RECid_RECno = 0;
				bAKIM_HAREKETLERI2.bkm_Spec_Rec_no = 0;
				bAKIM_HAREKETLERI2.bkm_iptal = false;
				bAKIM_HAREKETLERI2.bkm_fileid = 97;
				bAKIM_HAREKETLERI2.bkm_hidden = false;
				bAKIM_HAREKETLERI2.bkm_kilitli = false;
				bAKIM_HAREKETLERI2.bkm_degisti = false;
				bAKIM_HAREKETLERI2.bkm_checksum = 0;
				bAKIM_HAREKETLERI2.bkm_create_user = 1;
				bAKIM_HAREKETLERI2.bkm_create_date = DateTime.Now;
				bAKIM_HAREKETLERI2.bkm_lastup_user = 1;
				bAKIM_HAREKETLERI2.bkm_lastup_date = DateTime.Now;
				bAKIM_HAREKETLERI2.bkm_special1 = "FORA";
				bAKIM_HAREKETLERI2.bkm_special2 = "";
				bAKIM_HAREKETLERI2.bkm_special3 = "";
				bAKIM_HAREKETLERI2.bkm_firmano = 0;
				bAKIM_HAREKETLERI2.bkm_subeno = 0;
				bAKIM_HAREKETLERI2.bkm_tarihi = DateTime.Now;
				bAKIM_HAREKETLERI2.bkm_evrakno_seri = bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_seri;
				bAKIM_HAREKETLERI2.bkm_evrakno_sira = bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_sira;
				bAKIM_HAREKETLERI2.bkm_satirno = num2;
				bAKIM_HAREKETLERI2.bkm_belgeno = "";
				bAKIM_HAREKETLERI2.bkm_belge_tarihi = DateTime.Now;
				bAKIM_HAREKETLERI2.bkm_tuketici_kodu = bAKIM_KABUL_HAREKETLERI.bkmkb_tuketici_kodu;
				bAKIM_HAREKETLERI2.bkm_cihaz_serino = bAKIM_KABUL_HAREKETLERI.bkmkb_cihaz_serino;
				bAKIM_HAREKETLERI2.bkm_fis_stok_kodu = bAKIM_KABUL_HAREKETLERI.bkmkb_fis_stok_kodu;
				bAKIM_HAREKETLERI2.bkm_teslim_alinma_tarihi = DateTime.Now;
				bAKIM_HAREKETLERI2.bkm_teslim_edilme_tarihi = DateTime.Now;
				bAKIM_HAREKETLERI2.bkm_teslim_edilme_sekli = 1;
				bAKIM_HAREKETLERI2.bkm_ariza_kodu1 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu1;
				bAKIM_HAREKETLERI2.bkm_ariza_kodu2 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu2;
				bAKIM_HAREKETLERI2.bkm_ariza_kodu3 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu3;
				bAKIM_HAREKETLERI2.bkm_ariza_kodu4 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu4;
				bAKIM_HAREKETLERI2.bkm_ariza_kodu5 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu5;
				bAKIM_HAREKETLERI2.bkm_ariza_kodu6 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu6;
				bAKIM_HAREKETLERI2.bkm_ariza_kodu7 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu7;
				bAKIM_HAREKETLERI2.bkm_ariza_kodu8 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu8;
				bAKIM_HAREKETLERI2.bkm_ariza_kodu9 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu9;
				bAKIM_HAREKETLERI2.bkm_ariza_kodu10 = bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu10;
				bAKIM_HAREKETLERI2.bkm_ekip_kodu = bAKIM_KABUL_HAREKETLERI.bkmkb_inceleyecek_ekip_kodu;
				bAKIM_HAREKETLERI2.bkm_depono = num;
				bAKIM_HAREKETLERI2.bkm_aciklama = bAKIM_KABUL_HAREKETLERI.bkmkb_aciklama;
				bAKIM_HAREKETLERI2.bkm_hareket_tipi = 0;
				bAKIM_HAREKETLERI2.bkm_stok_hizmet_kodu = item.sto_kod;
				bAKIM_HAREKETLERI2.bkm_miktari = item.miktar;
				bAKIM_HAREKETLERI2.bkm_birim_fiyati = 0.0;
				bAKIM_HAREKETLERI2.bkm_tutari = 0.0;
				bAKIM_HAREKETLERI2.bkm_iskonto1 = 0.0;
				bAKIM_HAREKETLERI2.bkm_iskonto2 = 0.0;
				bAKIM_HAREKETLERI2.bkm_iskonto3 = 0.0;
				bAKIM_HAREKETLERI2.bkm_iskonto4 = 0.0;
				bAKIM_HAREKETLERI2.bkm_iskonto5 = 0.0;
				bAKIM_HAREKETLERI2.bkm_iskonto6 = 0.0;
				bAKIM_HAREKETLERI2.bkm_masraf1 = 0.0;
				bAKIM_HAREKETLERI2.bkm_masraf2 = 0.0;
				bAKIM_HAREKETLERI2.bkm_masraf3 = 0.0;
				bAKIM_HAREKETLERI2.bkm_masraf4 = 0.0;
				bAKIM_HAREKETLERI2.bkm_vergi_pntr = 0;
				bAKIM_HAREKETLERI2.bkm_vergi = 4.0;
				bAKIM_HAREKETLERI2.bkm_masraf_vergi_pnt = 0;
				bAKIM_HAREKETLERI2.bkm_masraf_vergi = 0.0;
				bAKIM_HAREKETLERI2.bkm_isk_mas1 = 0;
				bAKIM_HAREKETLERI2.bkm_isk_mas2 = 0;
				bAKIM_HAREKETLERI2.bkm_isk_mas3 = 0;
				bAKIM_HAREKETLERI2.bkm_isk_mas4 = 0;
				bAKIM_HAREKETLERI2.bkm_isk_mas5 = 0;
				bAKIM_HAREKETLERI2.bkm_isk_mas6 = 0;
				bAKIM_HAREKETLERI2.bkm_isk_mas7 = 0;
				bAKIM_HAREKETLERI2.bkm_isk_mas8 = 0;
				bAKIM_HAREKETLERI2.bkm_isk_mas9 = 0;
				bAKIM_HAREKETLERI2.bkm_isk_mas10 = 0;
				bAKIM_HAREKETLERI2.bkm_sat_isk_mas1 = false;
				bAKIM_HAREKETLERI2.bkm_sat_isk_mas2 = false;
				bAKIM_HAREKETLERI2.bkm_sat_isk_mas3 = false;
				bAKIM_HAREKETLERI2.bkm_sat_isk_mas4 = false;
				bAKIM_HAREKETLERI2.bkm_sat_isk_mas5 = false;
				bAKIM_HAREKETLERI2.bkm_sat_isk_mas6 = false;
				bAKIM_HAREKETLERI2.bkm_sat_isk_mas7 = false;
				bAKIM_HAREKETLERI2.bkm_sat_isk_mas8 = false;
				bAKIM_HAREKETLERI2.bkm_sat_isk_mas9 = false;
				bAKIM_HAREKETLERI2.bkm_sat_isk_mas10 = false;
				bAKIM_HAREKETLERI2.bkm_doviz_cins = 0;
				bAKIM_HAREKETLERI2.bkm_doviz_kur = 1.0;
				bAKIM_HAREKETLERI2.bkm_alt_doviz_kur = 0.0;
				bAKIM_HAREKETLERI2.bkm_vergisiz_fl = false;
				bAKIM_HAREKETLERI2.bkm_satir_aciklama = "";
				bAKIM_HAREKETLERI2.bkm_faturalandi_fl = false;
				bAKIM_HAREKETLERI2.bkm_ziyaret_kodu = "";
				bAKIM_HAREKETLERI2.bkm_ziy_ac_tar = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_ziy_cik_zmn = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_ziy_bas_zmn = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_ziy_son_zmn = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_ziy_don_zmn = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_kabul_RECid_DBCno = 0;
				bAKIM_HAREKETLERI2.bkm_kabul_RECid_RECno = 0;
				bAKIM_HAREKETLERI2.bkm_isemri_RECid_DBCno = 0;
				bAKIM_HAREKETLERI2.bkm_isemri_RECid_RECno = 0;
				bAKIM_HAREKETLERI2.bkm_cihazdurumbastarihi1 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumbittarihi1 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumkodu1 = "";
				bAKIM_HAREKETLERI2.bkm_cihazserviselemanikodu1 = "";
				bAKIM_HAREKETLERI2.bkm_cihazdurumbastarihi2 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumbittarihi2 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumkodu2 = "";
				bAKIM_HAREKETLERI2.bkm_cihazserviselemanikodu2 = "";
				bAKIM_HAREKETLERI2.bkm_cihazdurumbastarihi3 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumbittarihi3 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumkodu3 = "";
				bAKIM_HAREKETLERI2.bkm_cihazserviselemanikodu3 = "";
				bAKIM_HAREKETLERI2.bkm_cihazdurumbastarihi4 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumbittarihi4 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumkodu4 = "";
				bAKIM_HAREKETLERI2.bkm_cihazserviselemanikodu4 = "";
				bAKIM_HAREKETLERI2.bkm_cihazdurumbastarihi5 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumbittarihi5 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumkodu5 = "";
				bAKIM_HAREKETLERI2.bkm_cihazserviselemanikodu5 = "";
				bAKIM_HAREKETLERI2.bkm_cihazdurumbastarihi6 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumbittarihi6 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumkodu6 = "";
				bAKIM_HAREKETLERI2.bkm_cihazserviselemanikodu6 = "";
				bAKIM_HAREKETLERI2.bkm_cihazdurumbastarihi7 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumbittarihi7 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumkodu7 = "";
				bAKIM_HAREKETLERI2.bkm_cihazserviselemanikodu7 = "";
				bAKIM_HAREKETLERI2.bkm_cihazdurumbastarihi8 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumbittarihi8 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumkodu8 = "";
				bAKIM_HAREKETLERI2.bkm_cihazserviselemanikodu8 = "";
				bAKIM_HAREKETLERI2.bkm_cihazdurumbastarihi9 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumbittarihi9 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumkodu9 = "";
				bAKIM_HAREKETLERI2.bkm_cihazserviselemanikodu9 = "";
				bAKIM_HAREKETLERI2.bkm_cihazdurumbastarihi10 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumbittarihi10 = new DateTime(1900, 1, 1);
				bAKIM_HAREKETLERI2.bkm_cihazdurumkodu10 = "";
				bAKIM_HAREKETLERI2.bkm_cihazserviselemanikodu10 = "";
				bAKIM_HAREKETLERI2.bkm_fiyat_liste_no = 0;
				bAKIM_HAREKETLERI2.bkm_parti_kodu = "";
				bAKIM_HAREKETLERI2.bkm_lot_no = 0;
				bAKIM_HAREKETLERI2.bkm_servis_turu = 0;
				bAKIM_HAREKETLERI2.bkm_prj_kodu = "";
				bAKIM_HAREKETLERI2.bkm_srm_kodu = "";
				bAKIM_HAREKETLERI2.bkm_adres_no = 0;
				list.Add(bAKIM_HAREKETLERI2);
				num2++;
			}
			string cmdText = "BEGIN INSERT INTO BAKIM_HAREKETLERI(bkm_RECid_DBCno,bkm_RECid_RECno,bkm_Spec_Rec_no,bkm_iptal,bkm_fileid,bkm_hidden,bkm_kilitli,bkm_degisti,bkm_checksum,bkm_create_user,bkm_create_date,bkm_lastup_user,bkm_lastup_date,bkm_special1,bkm_special2,bkm_special3,bkm_firmano,bkm_subeno,bkm_tarihi,bkm_evrakno_seri,bkm_evrakno_sira,bkm_satirno,bkm_belgeno,bkm_belge_tarihi,bkm_tuketici_kodu,bkm_cihaz_serino,bkm_fis_stok_kodu,bkm_teslim_alinma_tarihi,bkm_teslim_edilme_tarihi,bkm_teslim_edilme_sekli,bkm_ariza_kodu1,bkm_ariza_kodu2,bkm_ariza_kodu3,bkm_ariza_kodu4,bkm_ariza_kodu5,bkm_ariza_kodu6,bkm_ariza_kodu7,bkm_ariza_kodu8,bkm_ariza_kodu9,bkm_ariza_kodu10,bkm_ekip_kodu,bkm_depono,bkm_aciklama,bkm_hareket_tipi,bkm_stok_hizmet_kodu,bkm_miktari,bkm_birim_fiyati,bkm_tutari,bkm_iskonto1,bkm_iskonto2,bkm_iskonto3,bkm_iskonto4,bkm_iskonto5,bkm_iskonto6,bkm_masraf1,bkm_masraf2,bkm_masraf3,bkm_masraf4,bkm_vergi_pntr,bkm_vergi,bkm_masraf_vergi_pnt,bkm_masraf_vergi,bkm_isk_mas1,bkm_isk_mas2,bkm_isk_mas3,bkm_isk_mas4,bkm_isk_mas5,bkm_isk_mas6,bkm_isk_mas7,bkm_isk_mas8,bkm_isk_mas9,bkm_isk_mas10,bkm_sat_isk_mas1,bkm_sat_isk_mas2,bkm_sat_isk_mas3,bkm_sat_isk_mas4,bkm_sat_isk_mas5,bkm_sat_isk_mas6,bkm_sat_isk_mas7,bkm_sat_isk_mas8,bkm_sat_isk_mas9,bkm_sat_isk_mas10,bkm_doviz_cins,bkm_doviz_kur,bkm_alt_doviz_kur,bkm_vergisiz_fl,bkm_satir_aciklama,bkm_faturalandi_fl,bkm_ziyaret_kodu,bkm_ziy_ac_tar,bkm_ziy_cik_zmn,bkm_ziy_bas_zmn,bkm_ziy_son_zmn,bkm_ziy_don_zmn,bkm_kabul_RECid_DBCno,bkm_kabul_RECid_RECno,bkm_isemri_RECid_DBCno,bkm_isemri_RECid_RECno,bkm_cihazdurumbastarihi1,bkm_cihazdurumbittarihi1,bkm_cihazdurumkodu1,bkm_cihazserviselemanikodu1,bkm_cihazdurumbastarihi2,bkm_cihazdurumbittarihi2,bkm_cihazdurumkodu2,bkm_cihazserviselemanikodu2,bkm_cihazdurumbastarihi3,bkm_cihazdurumbittarihi3,bkm_cihazdurumkodu3,bkm_cihazserviselemanikodu3,bkm_cihazdurumbastarihi4,bkm_cihazdurumbittarihi4,bkm_cihazdurumkodu4,bkm_cihazserviselemanikodu4,bkm_cihazdurumbastarihi5,bkm_cihazdurumbittarihi5,bkm_cihazdurumkodu5,bkm_cihazserviselemanikodu5,bkm_cihazdurumbastarihi6,bkm_cihazdurumbittarihi6,bkm_cihazdurumkodu6,bkm_cihazserviselemanikodu6,bkm_cihazdurumbastarihi7,bkm_cihazdurumbittarihi7,bkm_cihazdurumkodu7,bkm_cihazserviselemanikodu7,bkm_cihazdurumbastarihi8,bkm_cihazdurumbittarihi8,bkm_cihazdurumkodu8,bkm_cihazserviselemanikodu8,bkm_cihazdurumbastarihi9,bkm_cihazdurumbittarihi9,bkm_cihazdurumkodu9,bkm_cihazserviselemanikodu9,bkm_cihazdurumbastarihi10,bkm_cihazdurumbittarihi10,bkm_cihazdurumkodu10,bkm_cihazserviselemanikodu10,bkm_fiyat_liste_no,bkm_parti_kodu,bkm_lot_no,bkm_servis_turu,bkm_prj_kodu,bkm_srm_kodu) VALUES(@bkm_RECid_DBCno,@bkm_RECid_RECno,@bkm_Spec_Rec_no,@bkm_iptal,@bkm_fileid,@bkm_hidden,@bkm_kilitli,@bkm_degisti,@bkm_checksum,@bkm_create_user,getdate(),@bkm_lastup_user,getdate(),@bkm_special1,@bkm_special2,@bkm_special3,@bkm_firmano,@bkm_subeno,CONVERT(DATETIME,CONVERT(varchar(10), @bkm_tarihi, 103),103),@bkm_evrakno_seri,@bkm_evrakno_sira,@bkm_satirno,@bkm_belgeno,CONVERT(DATETIME,CONVERT(varchar(10), @bkm_belge_tarihi, 103),103),@bkm_tuketici_kodu,@bkm_cihaz_serino,@bkm_fis_stok_kodu,@bkm_teslim_alinma_tarihi,@bkm_teslim_edilme_tarihi,@bkm_teslim_edilme_sekli,@bkm_ariza_kodu1,@bkm_ariza_kodu2,@bkm_ariza_kodu3,@bkm_ariza_kodu4,@bkm_ariza_kodu5,@bkm_ariza_kodu6,@bkm_ariza_kodu7,@bkm_ariza_kodu8,@bkm_ariza_kodu9,@bkm_ariza_kodu10,@bkm_ekip_kodu,@bkm_depono,@bkm_aciklama,@bkm_hareket_tipi,@bkm_stok_hizmet_kodu,@bkm_miktari,@bkm_birim_fiyati,@bkm_tutari,@bkm_iskonto1,@bkm_iskonto2,@bkm_iskonto3,@bkm_iskonto4,@bkm_iskonto5,@bkm_iskonto6,@bkm_masraf1,@bkm_masraf2,@bkm_masraf3,@bkm_masraf4,@bkm_vergi_pntr,@bkm_vergi,@bkm_masraf_vergi_pnt,@bkm_masraf_vergi,@bkm_isk_mas1,@bkm_isk_mas2,@bkm_isk_mas3,@bkm_isk_mas4,@bkm_isk_mas5,@bkm_isk_mas6,@bkm_isk_mas7,@bkm_isk_mas8,@bkm_isk_mas9,@bkm_isk_mas10,@bkm_sat_isk_mas1,@bkm_sat_isk_mas2,@bkm_sat_isk_mas3,@bkm_sat_isk_mas4,@bkm_sat_isk_mas5,@bkm_sat_isk_mas6,@bkm_sat_isk_mas7,@bkm_sat_isk_mas8,@bkm_sat_isk_mas9,@bkm_sat_isk_mas10,@bkm_doviz_cins,@bkm_doviz_kur,@bkm_alt_doviz_kur,@bkm_vergisiz_fl,@bkm_satir_aciklama,@bkm_faturalandi_fl,@bkm_ziyaret_kodu,@bkm_ziy_ac_tar,@bkm_ziy_cik_zmn,@bkm_ziy_bas_zmn,@bkm_ziy_son_zmn,@bkm_ziy_don_zmn,@bkm_kabul_RECid_DBCno,@bkm_kabul_RECid_RECno,@bkm_isemri_RECid_DBCno,@bkm_isemri_RECid_RECno,@bkm_cihazdurumbastarihi1,@bkm_cihazdurumbittarihi1,@bkm_cihazdurumkodu1,@bkm_cihazserviselemanikodu1,@bkm_cihazdurumbastarihi2,@bkm_cihazdurumbittarihi2,@bkm_cihazdurumkodu2,@bkm_cihazserviselemanikodu2,@bkm_cihazdurumbastarihi3,@bkm_cihazdurumbittarihi3,@bkm_cihazdurumkodu3,@bkm_cihazserviselemanikodu3,@bkm_cihazdurumbastarihi4,@bkm_cihazdurumbittarihi4,@bkm_cihazdurumkodu4,@bkm_cihazserviselemanikodu4,@bkm_cihazdurumbastarihi5,@bkm_cihazdurumbittarihi5,@bkm_cihazdurumkodu5,@bkm_cihazserviselemanikodu5,@bkm_cihazdurumbastarihi6,@bkm_cihazdurumbittarihi6,@bkm_cihazdurumkodu6,@bkm_cihazserviselemanikodu6,@bkm_cihazdurumbastarihi7,@bkm_cihazdurumbittarihi7,@bkm_cihazdurumkodu7,@bkm_cihazserviselemanikodu7,@bkm_cihazdurumbastarihi8,@bkm_cihazdurumbittarihi8,@bkm_cihazdurumkodu8,@bkm_cihazserviselemanikodu8,@bkm_cihazdurumbastarihi9,@bkm_cihazdurumbittarihi9,@bkm_cihazdurumkodu9,@bkm_cihazserviselemanikodu9,@bkm_cihazdurumbastarihi10,@bkm_cihazdurumbittarihi10,@bkm_cihazdurumkodu10,@bkm_cihazserviselemanikodu10,@bkm_fiyat_liste_no,@bkm_parti_kodu,@bkm_lot_no,@bkm_servis_turu,@bkm_prj_kodu,@bkm_srm_kodu) UPDATE BAKIM_HAREKETLERI SET bkm_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE bkm_RECno=(SELECT SCOPE_IDENTITY()) END";
			foreach (BAKIM_HAREKETLERI item2 in list)
			{
				SqlCommand sqlCommand3 = new SqlCommand(cmdText, sqlDB.Connection, sqlTransaction);
				sqlCommand3.Parameters.AddWithValue("@bkm_RECid_DBCno", item2.bkm_RECid_DBCno);
				sqlCommand3.Parameters.AddWithValue("@bkm_RECid_RECno", item2.bkm_RECid_RECno);
				sqlCommand3.Parameters.AddWithValue("@bkm_Spec_Rec_no", item2.bkm_Spec_Rec_no);
				sqlCommand3.Parameters.AddWithValue("@bkm_iptal", item2.bkm_iptal);
				sqlCommand3.Parameters.AddWithValue("@bkm_fileid", item2.bkm_fileid);
				sqlCommand3.Parameters.AddWithValue("@bkm_hidden", item2.bkm_hidden);
				sqlCommand3.Parameters.AddWithValue("@bkm_kilitli", item2.bkm_kilitli);
				sqlCommand3.Parameters.AddWithValue("@bkm_degisti", item2.bkm_degisti);
				sqlCommand3.Parameters.AddWithValue("@bkm_checksum", item2.bkm_checksum);
				sqlCommand3.Parameters.AddWithValue("@bkm_create_user", item2.bkm_create_user);
				sqlCommand3.Parameters.AddWithValue("@bkm_lastup_user", item2.bkm_lastup_user);
				sqlCommand3.Parameters.AddWithValue("@bkm_special1", item2.bkm_special1);
				sqlCommand3.Parameters.AddWithValue("@bkm_special2", item2.bkm_special2);
				sqlCommand3.Parameters.AddWithValue("@bkm_special3", item2.bkm_special3);
				sqlCommand3.Parameters.AddWithValue("@bkm_firmano", item2.bkm_firmano);
				sqlCommand3.Parameters.AddWithValue("@bkm_subeno", item2.bkm_subeno);
				sqlCommand3.Parameters.AddWithValue("@bkm_tarihi", item2.bkm_tarihi);
				sqlCommand3.Parameters.AddWithValue("@bkm_evrakno_seri", item2.bkm_evrakno_seri);
				sqlCommand3.Parameters.AddWithValue("@bkm_evrakno_sira", item2.bkm_evrakno_sira);
				sqlCommand3.Parameters.AddWithValue("@bkm_satirno", item2.bkm_satirno);
				sqlCommand3.Parameters.AddWithValue("@bkm_belgeno", item2.bkm_belgeno);
				sqlCommand3.Parameters.AddWithValue("@bkm_belge_tarihi", item2.bkm_belge_tarihi);
				sqlCommand3.Parameters.AddWithValue("@bkm_tuketici_kodu", item2.bkm_tuketici_kodu);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihaz_serino", item2.bkm_cihaz_serino);
				sqlCommand3.Parameters.AddWithValue("@bkm_fis_stok_kodu", item2.bkm_fis_stok_kodu);
				sqlCommand3.Parameters.AddWithValue("@bkm_teslim_alinma_tarihi", item2.bkm_teslim_alinma_tarihi);
				sqlCommand3.Parameters.AddWithValue("@bkm_teslim_edilme_tarihi", item2.bkm_teslim_edilme_tarihi);
				sqlCommand3.Parameters.AddWithValue("@bkm_teslim_edilme_sekli", item2.bkm_teslim_edilme_sekli);
				sqlCommand3.Parameters.AddWithValue("@bkm_ariza_kodu1", item2.bkm_ariza_kodu1);
				sqlCommand3.Parameters.AddWithValue("@bkm_ariza_kodu2", item2.bkm_ariza_kodu2);
				sqlCommand3.Parameters.AddWithValue("@bkm_ariza_kodu3", item2.bkm_ariza_kodu3);
				sqlCommand3.Parameters.AddWithValue("@bkm_ariza_kodu4", item2.bkm_ariza_kodu4);
				sqlCommand3.Parameters.AddWithValue("@bkm_ariza_kodu5", item2.bkm_ariza_kodu5);
				sqlCommand3.Parameters.AddWithValue("@bkm_ariza_kodu6", item2.bkm_ariza_kodu6);
				sqlCommand3.Parameters.AddWithValue("@bkm_ariza_kodu7", item2.bkm_ariza_kodu7);
				sqlCommand3.Parameters.AddWithValue("@bkm_ariza_kodu8", item2.bkm_ariza_kodu8);
				sqlCommand3.Parameters.AddWithValue("@bkm_ariza_kodu9", item2.bkm_ariza_kodu9);
				sqlCommand3.Parameters.AddWithValue("@bkm_ariza_kodu10", item2.bkm_ariza_kodu10);
				sqlCommand3.Parameters.AddWithValue("@bkm_ekip_kodu", item2.bkm_ekip_kodu);
				sqlCommand3.Parameters.AddWithValue("@bkm_depono", item2.bkm_depono);
				sqlCommand3.Parameters.AddWithValue("@bkm_aciklama", item2.bkm_aciklama);
				sqlCommand3.Parameters.AddWithValue("@bkm_hareket_tipi", item2.bkm_hareket_tipi);
				sqlCommand3.Parameters.AddWithValue("@bkm_stok_hizmet_kodu", item2.bkm_stok_hizmet_kodu);
				sqlCommand3.Parameters.AddWithValue("@bkm_miktari", item2.bkm_miktari);
				sqlCommand3.Parameters.AddWithValue("@bkm_birim_fiyati", item2.bkm_birim_fiyati);
				sqlCommand3.Parameters.AddWithValue("@bkm_tutari", item2.bkm_tutari);
				sqlCommand3.Parameters.AddWithValue("@bkm_iskonto1", item2.bkm_iskonto1);
				sqlCommand3.Parameters.AddWithValue("@bkm_iskonto2", item2.bkm_iskonto2);
				sqlCommand3.Parameters.AddWithValue("@bkm_iskonto3", item2.bkm_iskonto3);
				sqlCommand3.Parameters.AddWithValue("@bkm_iskonto4", item2.bkm_iskonto4);
				sqlCommand3.Parameters.AddWithValue("@bkm_iskonto5", item2.bkm_iskonto5);
				sqlCommand3.Parameters.AddWithValue("@bkm_iskonto6", item2.bkm_iskonto6);
				sqlCommand3.Parameters.AddWithValue("@bkm_masraf1", item2.bkm_masraf1);
				sqlCommand3.Parameters.AddWithValue("@bkm_masraf2", item2.bkm_masraf2);
				sqlCommand3.Parameters.AddWithValue("@bkm_masraf3", item2.bkm_masraf3);
				sqlCommand3.Parameters.AddWithValue("@bkm_masraf4", item2.bkm_masraf4);
				sqlCommand3.Parameters.AddWithValue("@bkm_vergi_pntr", item2.bkm_vergi_pntr);
				sqlCommand3.Parameters.AddWithValue("@bkm_vergi", item2.bkm_vergi);
				sqlCommand3.Parameters.AddWithValue("@bkm_masraf_vergi_pnt", item2.bkm_masraf_vergi_pnt);
				sqlCommand3.Parameters.AddWithValue("@bkm_masraf_vergi", item2.bkm_masraf_vergi);
				sqlCommand3.Parameters.AddWithValue("@bkm_isk_mas1", item2.bkm_isk_mas1);
				sqlCommand3.Parameters.AddWithValue("@bkm_isk_mas2", item2.bkm_isk_mas2);
				sqlCommand3.Parameters.AddWithValue("@bkm_isk_mas3", item2.bkm_isk_mas3);
				sqlCommand3.Parameters.AddWithValue("@bkm_isk_mas4", item2.bkm_isk_mas4);
				sqlCommand3.Parameters.AddWithValue("@bkm_isk_mas5", item2.bkm_isk_mas5);
				sqlCommand3.Parameters.AddWithValue("@bkm_isk_mas6", item2.bkm_isk_mas6);
				sqlCommand3.Parameters.AddWithValue("@bkm_isk_mas7", item2.bkm_isk_mas7);
				sqlCommand3.Parameters.AddWithValue("@bkm_isk_mas8", item2.bkm_isk_mas8);
				sqlCommand3.Parameters.AddWithValue("@bkm_isk_mas9", item2.bkm_isk_mas9);
				sqlCommand3.Parameters.AddWithValue("@bkm_isk_mas10", item2.bkm_isk_mas10);
				sqlCommand3.Parameters.AddWithValue("@bkm_sat_isk_mas1", item2.bkm_sat_isk_mas1);
				sqlCommand3.Parameters.AddWithValue("@bkm_sat_isk_mas2", item2.bkm_sat_isk_mas2);
				sqlCommand3.Parameters.AddWithValue("@bkm_sat_isk_mas3", item2.bkm_sat_isk_mas3);
				sqlCommand3.Parameters.AddWithValue("@bkm_sat_isk_mas4", item2.bkm_sat_isk_mas4);
				sqlCommand3.Parameters.AddWithValue("@bkm_sat_isk_mas5", item2.bkm_sat_isk_mas5);
				sqlCommand3.Parameters.AddWithValue("@bkm_sat_isk_mas6", item2.bkm_sat_isk_mas6);
				sqlCommand3.Parameters.AddWithValue("@bkm_sat_isk_mas7", item2.bkm_sat_isk_mas7);
				sqlCommand3.Parameters.AddWithValue("@bkm_sat_isk_mas8", item2.bkm_sat_isk_mas8);
				sqlCommand3.Parameters.AddWithValue("@bkm_sat_isk_mas9", item2.bkm_sat_isk_mas9);
				sqlCommand3.Parameters.AddWithValue("@bkm_sat_isk_mas10", item2.bkm_sat_isk_mas10);
				sqlCommand3.Parameters.AddWithValue("@bkm_doviz_cins", item2.bkm_doviz_cins);
				sqlCommand3.Parameters.AddWithValue("@bkm_doviz_kur", item2.bkm_doviz_kur);
				sqlCommand3.Parameters.AddWithValue("@bkm_alt_doviz_kur", item2.bkm_alt_doviz_kur);
				sqlCommand3.Parameters.AddWithValue("@bkm_vergisiz_fl", item2.bkm_vergisiz_fl);
				sqlCommand3.Parameters.AddWithValue("@bkm_satir_aciklama", item2.bkm_satir_aciklama);
				sqlCommand3.Parameters.AddWithValue("@bkm_faturalandi_fl", item2.bkm_faturalandi_fl);
				sqlCommand3.Parameters.AddWithValue("@bkm_ziyaret_kodu", item2.bkm_ziyaret_kodu);
				sqlCommand3.Parameters.AddWithValue("@bkm_ziy_ac_tar", item2.bkm_ziy_ac_tar);
				sqlCommand3.Parameters.AddWithValue("@bkm_ziy_cik_zmn", item2.bkm_ziy_cik_zmn);
				sqlCommand3.Parameters.AddWithValue("@bkm_ziy_bas_zmn", item2.bkm_ziy_bas_zmn);
				sqlCommand3.Parameters.AddWithValue("@bkm_ziy_son_zmn", item2.bkm_ziy_son_zmn);
				sqlCommand3.Parameters.AddWithValue("@bkm_ziy_don_zmn", item2.bkm_ziy_don_zmn);
				sqlCommand3.Parameters.AddWithValue("@bkm_kabul_RECid_DBCno", item2.bkm_kabul_RECid_DBCno);
				sqlCommand3.Parameters.AddWithValue("@bkm_kabul_RECid_RECno", item2.bkm_kabul_RECid_RECno);
				sqlCommand3.Parameters.AddWithValue("@bkm_isemri_RECid_DBCno", item2.bkm_isemri_RECid_DBCno);
				sqlCommand3.Parameters.AddWithValue("@bkm_isemri_RECid_RECno", item2.bkm_isemri_RECid_RECno);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbastarihi1", item2.bkm_cihazdurumbastarihi1);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbittarihi1", item2.bkm_cihazdurumbittarihi1);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumkodu1", item2.bkm_cihazdurumkodu1);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazserviselemanikodu1", item2.bkm_cihazserviselemanikodu1);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbastarihi2", item2.bkm_cihazdurumbastarihi2);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbittarihi2", item2.bkm_cihazdurumbittarihi2);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumkodu2", item2.bkm_cihazdurumkodu2);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazserviselemanikodu2", item2.bkm_cihazserviselemanikodu2);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbastarihi3", item2.bkm_cihazdurumbastarihi3);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbittarihi3", item2.bkm_cihazdurumbittarihi3);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumkodu3", item2.bkm_cihazdurumkodu3);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazserviselemanikodu3", item2.bkm_cihazserviselemanikodu3);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbastarihi4", item2.bkm_cihazdurumbastarihi4);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbittarihi4", item2.bkm_cihazdurumbittarihi4);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumkodu4", item2.bkm_cihazdurumkodu4);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazserviselemanikodu4", item2.bkm_cihazserviselemanikodu4);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbastarihi5", item2.bkm_cihazdurumbastarihi5);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbittarihi5", item2.bkm_cihazdurumbittarihi5);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumkodu5", item2.bkm_cihazdurumkodu5);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazserviselemanikodu5", item2.bkm_cihazserviselemanikodu5);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbastarihi6", item2.bkm_cihazdurumbastarihi6);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbittarihi6", item2.bkm_cihazdurumbittarihi6);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumkodu6", item2.bkm_cihazdurumkodu6);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazserviselemanikodu6", item2.bkm_cihazserviselemanikodu6);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbastarihi7", item2.bkm_cihazdurumbastarihi7);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbittarihi7", item2.bkm_cihazdurumbittarihi7);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumkodu7", item2.bkm_cihazdurumkodu7);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazserviselemanikodu7", item2.bkm_cihazserviselemanikodu7);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbastarihi8", item2.bkm_cihazdurumbastarihi8);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbittarihi8", item2.bkm_cihazdurumbittarihi8);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumkodu8", item2.bkm_cihazdurumkodu8);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazserviselemanikodu8", item2.bkm_cihazserviselemanikodu8);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbastarihi9", item2.bkm_cihazdurumbastarihi9);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbittarihi9", item2.bkm_cihazdurumbittarihi9);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumkodu9", item2.bkm_cihazdurumkodu9);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazserviselemanikodu9", item2.bkm_cihazserviselemanikodu9);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbastarihi10", item2.bkm_cihazdurumbastarihi10);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumbittarihi10", item2.bkm_cihazdurumbittarihi10);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazdurumkodu10", item2.bkm_cihazdurumkodu10);
				sqlCommand3.Parameters.AddWithValue("@bkm_cihazserviselemanikodu10", item2.bkm_cihazserviselemanikodu10);
				sqlCommand3.Parameters.AddWithValue("@bkm_fiyat_liste_no", item2.bkm_fiyat_liste_no);
				sqlCommand3.Parameters.AddWithValue("@bkm_parti_kodu", item2.bkm_parti_kodu);
				sqlCommand3.Parameters.AddWithValue("@bkm_lot_no", item2.bkm_lot_no);
				sqlCommand3.Parameters.AddWithValue("@bkm_servis_turu", item2.bkm_servis_turu);
				sqlCommand3.Parameters.AddWithValue("@bkm_prj_kodu", item2.bkm_prj_kodu);
				sqlCommand3.Parameters.AddWithValue("@bkm_srm_kodu", item2.bkm_srm_kodu);
				foreach (SqlParameter parameter in sqlCommand3.Parameters)
				{
					if (parameter.Value == null)
					{
						parameter.IsNullable = true;
						parameter.Value = DBNull.Value;
					}
				}
				sqlCommand3.ExecuteNonQuery();
			}
			if (bAKIM_KABUL_HAREKETLERI.Aciklama1 != "" || bAKIM_KABUL_HAREKETLERI.Aciklama2 != "" || bAKIM_KABUL_HAREKETLERI.Aciklama3 != "")
			{
				SqlCommand sqlCommand4 = new SqlCommand("BEGIN INSERT INTO EVRAK_ACIKLAMALARI(egk_RECid_DBCno,egk_RECid_RECno,egk_SpecRECno,egk_iptal,egk_fileid,egk_hidden,egk_kilitli,egk_degisti,egk_checksum,egk_create_user,egk_create_date,egk_lastup_user,egk_lastup_date,egk_special1,egk_special2,egk_special3,egk_dosyano,egk_hareket_tip,egk_evr_tip,egk_evr_seri,egk_evr_sira,egk_evr_ustkod,egk_evr_doksayisi,egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10,egk_sipgenkarorani,egk_kargokodu,egk_kargono,egk_tesaltarihi,egk_tesalkisi) VALUES(@egk_RECid_DBCno,@egk_RECid_RECno,@egk_SpecRECno,@egk_iptal,@egk_fileid,@egk_hidden,@egk_kilitli,@egk_degisti,@egk_checksum,@egk_create_user,getdate(),@egk_lastup_user,getdate(),@egk_special1,@egk_special2,@egk_special3,@egk_dosyano,@egk_hareket_tip,@egk_evr_tip,@egk_evr_seri,@egk_evr_sira,@egk_evr_ustkod,@egk_evr_doksayisi,@egk_evracik1,@egk_evracik2,@egk_evracik3,@egk_evracik4,@egk_evracik5,@egk_evracik6,@egk_evracik7,@egk_evracik8,@egk_evracik9,@egk_evracik10,@egk_sipgenkarorani,@egk_kargokodu,@egk_kargono,@egk_tesaltarihi,@egk_tesalkisi) UPDATE EVRAK_ACIKLAMALARI SET egk_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE egk_RECno=(SELECT SCOPE_IDENTITY()) END", sqlDB.Connection, sqlTransaction);
				sqlCommand4.Parameters.AddWithValue("@egk_RECid_DBCno", 0);
				sqlCommand4.Parameters.AddWithValue("@egk_RECid_RECno", 0);
				sqlCommand4.Parameters.AddWithValue("@egk_SpecRECno", 0);
				sqlCommand4.Parameters.AddWithValue("@egk_iptal", false);
				sqlCommand4.Parameters.AddWithValue("@egk_fileid", 66);
				sqlCommand4.Parameters.AddWithValue("@egk_hidden", false);
				sqlCommand4.Parameters.AddWithValue("@egk_kilitli", false);
				sqlCommand4.Parameters.AddWithValue("@egk_degisti", false);
				sqlCommand4.Parameters.AddWithValue("@egk_checksum", 0);
				sqlCommand4.Parameters.AddWithValue("@egk_create_user", 1);
				sqlCommand4.Parameters.AddWithValue("@egk_lastup_user", 1);
				sqlCommand4.Parameters.AddWithValue("@egk_special1", "FORA");
				sqlCommand4.Parameters.AddWithValue("@egk_special2", "");
				sqlCommand4.Parameters.AddWithValue("@egk_special3", "");
				sqlCommand4.Parameters.AddWithValue("@egk_dosyano", 97);
				sqlCommand4.Parameters.AddWithValue("@egk_hareket_tip", 0);
				sqlCommand4.Parameters.AddWithValue("@egk_evr_tip", 0);
				sqlCommand4.Parameters.AddWithValue("@egk_evr_seri", bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_seri);
				sqlCommand4.Parameters.AddWithValue("@egk_evr_sira", bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_sira);
				sqlCommand4.Parameters.AddWithValue("@egk_evr_ustkod", "");
				sqlCommand4.Parameters.AddWithValue("@egk_evr_doksayisi", 0);
				if (bAKIM_KABUL_HAREKETLERI.Aciklama1 != null)
				{
					if (bAKIM_KABUL_HAREKETLERI.Aciklama1.Length > 126)
					{
						bAKIM_KABUL_HAREKETLERI.Aciklama1 = bAKIM_KABUL_HAREKETLERI.Aciklama1.Substring(0, 126);
					}
				}
				else
				{
					bAKIM_KABUL_HAREKETLERI.Aciklama1 = "";
				}
				sqlCommand4.Parameters.AddWithValue("@egk_evracik1", bAKIM_KABUL_HAREKETLERI.Aciklama1);
				if (bAKIM_KABUL_HAREKETLERI.Aciklama2 != null)
				{
					if (bAKIM_KABUL_HAREKETLERI.Aciklama2.Length > 126)
					{
						bAKIM_KABUL_HAREKETLERI.Aciklama2 = bAKIM_KABUL_HAREKETLERI.Aciklama2.Substring(0, 126);
					}
				}
				else
				{
					bAKIM_KABUL_HAREKETLERI.Aciklama2 = "";
				}
				sqlCommand4.Parameters.AddWithValue("@egk_evracik2", bAKIM_KABUL_HAREKETLERI.Aciklama2);
				if (bAKIM_KABUL_HAREKETLERI.Aciklama3 != null)
				{
					if (bAKIM_KABUL_HAREKETLERI.Aciklama3.Length > 126)
					{
						bAKIM_KABUL_HAREKETLERI.Aciklama3 = bAKIM_KABUL_HAREKETLERI.Aciklama3.Substring(0, 126);
					}
				}
				else
				{
					bAKIM_KABUL_HAREKETLERI.Aciklama3 = "";
				}
				sqlCommand4.Parameters.AddWithValue("@egk_evracik3", bAKIM_KABUL_HAREKETLERI.Aciklama3);
				sqlCommand4.Parameters.AddWithValue("@egk_evracik4", "");
				sqlCommand4.Parameters.AddWithValue("@egk_evracik5", "");
				sqlCommand4.Parameters.AddWithValue("@egk_evracik6", "");
				sqlCommand4.Parameters.AddWithValue("@egk_evracik7", "");
				sqlCommand4.Parameters.AddWithValue("@egk_evracik8", "");
				sqlCommand4.Parameters.AddWithValue("@egk_evracik9", "");
				sqlCommand4.Parameters.AddWithValue("@egk_evracik10", "");
				sqlCommand4.Parameters.AddWithValue("@egk_sipgenkarorani", 0);
				sqlCommand4.Parameters.AddWithValue("@egk_kargokodu", "");
				sqlCommand4.Parameters.AddWithValue("@egk_kargono", "");
				sqlCommand4.Parameters.AddWithValue("@egk_tesaltarihi", DateTime.Parse("1900-01-01 00:00:00.000"));
				sqlCommand4.Parameters.AddWithValue("@egk_tesalkisi", "");
				foreach (SqlParameter parameter2 in sqlCommand4.Parameters)
				{
					if (parameter2.Value == null)
					{
						parameter2.IsNullable = true;
						parameter2.Value = DBNull.Value;
					}
				}
				sqlCommand4.ExecuteNonQuery();
			}
			SqlCommand sqlCommand5 = new SqlCommand("UPDATE BAKIM_KABUL_HAREKETLERI SET bkmkb_planlandi_fl=1 WHERE bkmkb_RECno=@bkmkb_RECno");
			sqlCommand5.Parameters.AddWithValue("@bkmkb_RECno", bAKIM_KABUL_HAREKETLERI.bkmkb_RECno);
			sqlCommand5.Connection = sqlDB.Connection;
			sqlCommand5.Transaction = sqlTransaction;
			sqlCommand5.ExecuteNonQuery();
			if (bAKIM_KABUL_HAREKETLERI.KullanilanStoklar.Count != 0)
			{
				SqlDB sqlDB2 = new SqlDB();
				sqlDB2.ConnectionOpen(baglantiBilgileri, dBName);
				Evrak evrak = new Evrak(enum_GenelEvrakTipleri.SatisIrsaliyesi, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
				evrak.SetAlternatifDovizCinsi(0);
				evrak.alternatifdovizkuru = new Kur();
				evrak.SetBelgeNo("");
				evrak.cari = CariData.GetCariByCariKod(sqlDB2.Connection, cari_kod, AdreslerTemsilciyeGore: false, "");
				evrak.SetDegistirSpecialAlan1("FORA");
				evrak.SetDovizCinsi(0);
				evrak.SetEvrakNoSeri(bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_seri);
				evrak.SetEvraknoSira(bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_sira);
				evrak.SetKaynakDepo(new Depo(num));
				evrak.kur = new Kur();
				evrak.SetMikroUserNo(1);
				List<VergiTanimi> list2 = new List<VergiTanimi>();
				VergiTanimi vergiTanimi = new VergiTanimi();
				vergiTanimi.KisaAdi = "Tanımsız";
				vergiTanimi.UzunAdi = "TANIMSIZ";
				vergiTanimi.Yuzde = 0.0;
				VergiTanimi vergiTanimi2 = new VergiTanimi();
				vergiTanimi2.KisaAdi = "Yok";
				vergiTanimi2.UzunAdi = "YOK";
				vergiTanimi2.Yuzde = 0.0;
				VergiTanimi vergiTanimi3 = new VergiTanimi();
				vergiTanimi3.KisaAdi = "%1";
				vergiTanimi3.UzunAdi = "K.D.V. (%) 1";
				vergiTanimi3.Yuzde = 1.0;
				VergiTanimi vergiTanimi4 = new VergiTanimi();
				vergiTanimi4.KisaAdi = "%8";
				vergiTanimi4.UzunAdi = "K.D.V. (%) 8";
				vergiTanimi4.Yuzde = 8.0;
				VergiTanimi vergiTanimi5 = new VergiTanimi();
				vergiTanimi5.KisaAdi = "%18";
				vergiTanimi5.UzunAdi = "K.D.V. (%) 18";
				vergiTanimi5.Yuzde = 18.0;
				VergiTanimi vergiTanimi6 = new VergiTanimi();
				vergiTanimi6.KisaAdi = "%26";
				vergiTanimi6.UzunAdi = "K.D.V. (%) 26";
				vergiTanimi6.Yuzde = 26.0;
				VergiTanimi vergiTanimi7 = new VergiTanimi();
				vergiTanimi7.KisaAdi = "Özel Mahtah";
				vergiTanimi7.UzunAdi = "ÖZEL MATRAH";
				vergiTanimi7.Yuzde = 0.0;
				VergiTanimi vergiTanimi8 = new VergiTanimi();
				vergiTanimi8.KisaAdi = "";
				vergiTanimi8.UzunAdi = "";
				vergiTanimi8.Yuzde = 0.0;
				VergiTanimi vergiTanimi9 = new VergiTanimi();
				vergiTanimi9.KisaAdi = "";
				vergiTanimi9.UzunAdi = "";
				vergiTanimi9.Yuzde = 0.0;
				VergiTanimi vergiTanimi10 = new VergiTanimi();
				vergiTanimi10.KisaAdi = "";
				vergiTanimi10.UzunAdi = "";
				vergiTanimi10.Yuzde = 0.0;
				VergiTanimi vergiTanimi11 = new VergiTanimi();
				vergiTanimi11.KisaAdi = "";
				vergiTanimi11.UzunAdi = "";
				vergiTanimi11.Yuzde = 0.0;
				list2.Add(vergiTanimi);
				list2.Add(vergiTanimi2);
				list2.Add(vergiTanimi3);
				list2.Add(vergiTanimi4);
				list2.Add(vergiTanimi5);
				list2.Add(vergiTanimi6);
				list2.Add(vergiTanimi7);
				list2.Add(vergiTanimi8);
				list2.Add(vergiTanimi9);
				list2.Add(vergiTanimi10);
				list2.Add(vergiTanimi11);
				evrak.Parametreler.vergitanimlari = list2;
				foreach (StokBase item3 in bAKIM_KABUL_HAREKETLERI.KullanilanStoklar)
				{
					SqlDB sqlDB3 = new SqlDB();
					sqlDB3.ConnectionOpen(baglantiBilgileri, dBName);
					Stok stok = StokData.GetStok(sqlDB3.Connection, item3.sto_kod, evrak.GetToptanPerakende());
					sqlDB3.ConnectionClose();
					stok.ekleme_bilgileri.Miktar = item3.miktar;
					stok.ekleme_bilgileri.BirimFiyat = new FiyatTanimlamasi();
					evrak.AddUrun(stok);
				}
				if (EvrakData.EvrakKaydet(sqlDB.Connection, sqlTransaction, dBName, evrak, EArsivAktif: false) < 0)
				{
					sqlTransaction.Rollback();
					sqlDB.ConnectionClose();
					return WebOperationContext.Current.CreateTextResponse("Hata : İrsaliye oluşturulamadı.", "application/json;charset=utf-8", Encoding.UTF8);
				}
				sqlDB2.ConnectionClose();
			}
			sqlTransaction.Commit();
		}
		catch (Exception ex3)
		{
			sqlTransaction.Rollback();
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Evrak eklenemedi. " + ex3.ToString(), "application/json;charset=utf-8", Encoding.UTF8);
		}
		sqlDB.ConnectionClose();
		return WebOperationContext.Current.CreateTextResponse("Evrak kayıt edildi.", "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.GetEkipAtanacaklar(string firmaid, string UserName, string Password)
	{
		Console.WriteLine("GetEkipAtanacaklar Cagrildi");
		List<BAKIM_KABUL_HAREKETLERI> list = new List<BAKIM_KABUL_HAREKETLERI>();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		List<GenelList> list2 = new List<GenelList>();
		SqlCommand sqlCommand2 = new SqlCommand();
		string text2 = "'EKİP.ORTAK','" + userTeknikTesisBakimYonetimiAdmin.cari_kod + "'";
		string commandText2 = (sqlCommand2.CommandText = "SELECT ekp_kodu,ekp_adi FROM EKIP_TANIMLARI WITH(NOLOCK) WHERE ekp_cari_kodu in (" + text2 + ")");
		try
		{
			using SqlCommand sqlCommand3 = sqlDB.Connection.CreateCommand();
			sqlCommand3.CommandText = commandText2;
			SqlDataReader sqlDataReader2 = sqlCommand3.ExecuteReader();
			while (sqlDataReader2.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader2.GetSafeString(0);
				genelList.Text = sqlDataReader2.GetSafeString(1);
				list2.Add(genelList);
			}
			sqlDataReader2.Close();
		}
		catch
		{
		}
		string text4 = "";
		string[] array = userTeknikTesisBakimYonetimiAdmin.kurumlar.Split(',');
		bool flag = true;
		string[] array2 = array;
		foreach (string text5 in array2)
		{
			if (!flag)
			{
				text4 += ",";
				flag = false;
			}
			text4 = text4 + "'" + text5 + "'";
		}
		commandText2 = (sqlCommand2.CommandText = "SELECT b.bkmkb_RECno,b.bkmkb_tarihi,b.bkmkb_evrakno_seri,b.bkmkb_evrakno_sira,cihaz.chz_aciklama1 + ' ' + cihaz.chz_aciklama2 + ' ' + cihaz.chz_aciklama3 AS ariza_yeri,b.bkmkb_tuketici_kodu,s.tuk_ismi,b.bkmkb_bildirilen_arizalar,b.bkmkb_teslim_alinma_tarihi,b.bkmkb_ariza_kodu1,c.chs_sorun FROM BAKIM_KABUL_HAREKETLERI AS b INNER JOIN SON_KULLANICILAR AS s ON b.bkmkb_tuketici_kodu=s.tuk_kodu INNER JOIN CIHAZ_SORUNLARI AS c ON b.bkmkb_ariza_kodu1=c.chs_kodu INNER JOIN STOK_SERINO_TANIMLARI AS cihaz ON b.bkmkb_cihaz_serino=cihaz.chz_serino WHERE b.bkmkb_inceleyecek_ekip_kodu='' AND b.bkmkb_tuketici_kodu in (SELECT tuk_kodu FROM SON_KULLANICILAR WHERE tuk_cari_kodu in (" + text4 + "))");
		try
		{
			using SqlCommand sqlCommand4 = sqlDB.Connection.CreateCommand();
			sqlCommand4.CommandText = commandText2;
			SqlDataReader sqlDataReader3 = sqlCommand4.ExecuteReader();
			while (sqlDataReader3.Read())
			{
				BAKIM_KABUL_HAREKETLERI bAKIM_KABUL_HAREKETLERI = new BAKIM_KABUL_HAREKETLERI();
				bAKIM_KABUL_HAREKETLERI.bkmkb_RECno = sqlDataReader3.GetSafeInt32(0);
				bAKIM_KABUL_HAREKETLERI.bkmkb_tarihi = sqlDataReader3.GetSafeDateTime(1);
				bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_seri = sqlDataReader3.GetSafeString(2);
				bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_sira = sqlDataReader3.GetSafeInt32(3);
				bAKIM_KABUL_HAREKETLERI.bkmkb_cihaz_serino = sqlDataReader3.GetSafeString(4);
				bAKIM_KABUL_HAREKETLERI.bkmkb_tuketici_kodu = sqlDataReader3.GetSafeString(5);
				bAKIM_KABUL_HAREKETLERI.TuketiciAdi = sqlDataReader3.GetSafeString(6);
				bAKIM_KABUL_HAREKETLERI.bkmkb_bildirilen_arizalar = sqlDataReader3.GetSafeString(7);
				bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_alinma_tarihi = sqlDataReader3.GetSafeDateTime(8);
				bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu1 = sqlDataReader3.GetSafeString(9);
				bAKIM_KABUL_HAREKETLERI.ArizaAdi1 = sqlDataReader3.GetSafeString(10);
				bAKIM_KABUL_HAREKETLERI.Ekipler = list2;
				list.Add(bAKIM_KABUL_HAREKETLERI);
			}
			sqlDataReader3.Close();
		}
		catch
		{
		}
		sqlDB.ConnectionClose();
		string text7 = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text7, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.BakimKabulEkipAta(string firmaid, string UserName, string Password, string bkmkb_RECno, string bkmkb_inceleyecek_ekip_kodu)
	{
		bool flag = false;
		Console.WriteLine("BakimKabulEkipAta Cagrildi");
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		try
		{
			flag = EvrakData.BakimKabulEkipAta(sqlDB.Connection, int.Parse(bkmkb_RECno), bkmkb_inceleyecek_ekip_kodu);
		}
		catch
		{
		}
		sqlDB.ConnectionClose();
		string text2 = JsonConvert.SerializeObject(flag, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.GetBakimKabul(string firmaid, string UserName, string Password, string bkmkb_RECno)
	{
		BAKIM_KABUL_HAREKETLERI bAKIM_KABUL_HAREKETLERI = new BAKIM_KABUL_HAREKETLERI();
		Console.WriteLine("GetBakimKabul Cagrildi");
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		string commandText2 = "SELECT b.bkmkb_RECno,b.bkmkb_tarihi,b.bkmkb_evrakno_seri,b.bkmkb_evrakno_sira,cihaz.chz_aciklama1 + ' ' + cihaz.chz_aciklama2 + ' ' + cihaz.chz_aciklama3 AS ariza_yeri,b.bkmkb_tuketici_kodu,s.tuk_ismi,b.bkmkb_bildirilen_arizalar,b.bkmkb_teslim_alinma_tarihi,b.bkmkb_ariza_kodu1,c.chs_sorun,b.bkmkb_inceleyecek_ekip_kodu,e.ekp_adi FROM BAKIM_KABUL_HAREKETLERI AS b INNER JOIN SON_KULLANICILAR AS s ON b.bkmkb_tuketici_kodu=s.tuk_kodu INNER JOIN CIHAZ_SORUNLARI AS c ON b.bkmkb_ariza_kodu1=c.chs_kodu INNER JOIN EKIP_TANIMLARI AS e ON b.bkmkb_inceleyecek_ekip_kodu=e.ekp_kodu INNER JOIN STOK_SERINO_TANIMLARI AS cihaz ON b.bkmkb_cihaz_serino=cihaz.chz_serino WHERE b.bkmkb_RECno=@bkmkb_RECno";
		new SqlCommand().CommandText = commandText2;
		try
		{
			using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand2.CommandText = commandText2;
				sqlCommand2.Parameters.AddWithValue("@bkmkb_RECno", int.Parse(bkmkb_RECno));
				SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
				if (sqlDataReader2.HasRows)
				{
					sqlDataReader2.Read();
					bAKIM_KABUL_HAREKETLERI.bkmkb_RECno = sqlDataReader2.GetSafeInt32(0);
					bAKIM_KABUL_HAREKETLERI.bkmkb_tarihi = sqlDataReader2.GetSafeDateTime(1);
					bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_seri = sqlDataReader2.GetSafeString(2);
					bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_sira = sqlDataReader2.GetSafeInt32(3);
					bAKIM_KABUL_HAREKETLERI.bkmkb_cihaz_serino = sqlDataReader2.GetSafeString(4);
					bAKIM_KABUL_HAREKETLERI.bkmkb_tuketici_kodu = sqlDataReader2.GetSafeString(5);
					bAKIM_KABUL_HAREKETLERI.TuketiciAdi = sqlDataReader2.GetSafeString(6);
					bAKIM_KABUL_HAREKETLERI.bkmkb_bildirilen_arizalar = sqlDataReader2.GetSafeString(7);
					bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_alinma_tarihi = sqlDataReader2.GetSafeDateTime(8);
					bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu1 = sqlDataReader2.GetSafeString(9);
					bAKIM_KABUL_HAREKETLERI.ArizaAdi1 = sqlDataReader2.GetSafeString(10);
					bAKIM_KABUL_HAREKETLERI.bkmkb_inceleyecek_ekip_kodu = sqlDataReader2.GetSafeString(11);
					bAKIM_KABUL_HAREKETLERI.EkipAdi = sqlDataReader2.GetSafeString(12);
				}
				sqlDataReader2.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : " + ex3.ToString(), "application/json;charset=utf-8", Encoding.UTF8);
		}
		string text2 = JsonConvert.SerializeObject(bAKIM_KABUL_HAREKETLERI, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.SearchSarfStok(string firmaid, string UserName, string Password, Stream Content)
	{
		Console.WriteLine("SearchSarfStok Cagrildi");
		List<StokBase> list = new List<StokBase>();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		string text2 = JsonConvert.DeserializeObject<string>(new StreamReader(Content).ReadToEnd());
		text2 = "%" + text2.Replace(' ', '%') + "%";
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			string commandText2 = "SELECT TOP 10 sto_RECno,sto_RECid_RECno,sto_kod,sto_isim,sto_kisa_ismi,sto_yabanci_isim,sto_birim1_ad FROM STOKLAR WITH(NOLOCK) WHERE sto_anagrup_kod='SARF' AND (sto_kod like '" + text2 + "' Collate Turkish_CI_AI OR sto_isim like '" + text2 + "'  Collate Turkish_CI_AI)";
			sqlCommand2.CommandText = commandText2;
			SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
			while (sqlDataReader2.Read())
			{
				StokBase stokBase = new StokBase();
				stokBase.sto_RECno = sqlDataReader2.GetSafeInt32(0);
				stokBase.sto_RECid_RECno = sqlDataReader2.GetSafeInt32(1);
				stokBase.sto_kod = sqlDataReader2.GetSafeString(2);
				stokBase.sto_isim = sqlDataReader2.GetSafeString(3);
				stokBase.sto_kisa_ismi = sqlDataReader2.GetSafeString(4);
				stokBase.sto_yabanci_isim = sqlDataReader2.GetSafeString(5);
				stokBase.sto_birim1_ad = sqlDataReader2.GetSafeString(6);
				list.Add(stokBase);
			}
			sqlDataReader2.Close();
		}
		sqlDB.ConnectionClose();
		string text3 = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text3, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.GetStokBase(string firmaid, string UserName, string Password, Stream Content)
	{
		Console.WriteLine("GetStokBase Cagrildi");
		StokBase stokBase = new StokBase();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		string value = JsonConvert.DeserializeObject<string>(new StreamReader(Content).ReadToEnd());
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			string commandText2 = "SELECT TOP 1 sto_RECno,sto_RECid_RECno,sto_kod,sto_isim,sto_kisa_ismi,sto_yabanci_isim FROM STOKLAR WITH(NOLOCK) WHERE sto_kod=@sto_kod";
			sqlCommand2.CommandText = commandText2;
			sqlCommand2.Parameters.AddWithValue("@sto_kod", value);
			SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
			while (sqlDataReader2.Read())
			{
				stokBase.sto_RECno = sqlDataReader2.GetSafeInt32(0);
				stokBase.sto_RECid_RECno = sqlDataReader2.GetSafeInt32(1);
				stokBase.sto_kod = sqlDataReader2.GetSafeString(2);
				stokBase.sto_isim = sqlDataReader2.GetSafeString(3);
				stokBase.sto_kisa_ismi = sqlDataReader2.GetSafeString(4);
				stokBase.sto_yabanci_isim = sqlDataReader2.GetSafeString(5);
			}
			sqlDataReader2.Close();
		}
		sqlDB.ConnectionClose();
		string text2 = JsonConvert.SerializeObject(stokBase, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.GetKurumGenelListV2(string firmaid, string UserName, string Password)
	{
		Console.WriteLine("GetEkipAtanacaklar Cagrildi");
		List<GenelList> list = new List<GenelList>();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		string commandText2 = (new SqlCommand().CommandText = "SELECT cari_kod,cari_unvan1 FROM CARI_HESAPLAR WITH(NOLOCK) WHERE cari_kod like 'KURUM%'");
		try
		{
			using SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand();
			sqlCommand2.CommandText = commandText2;
			SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
			while (sqlDataReader2.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader2.GetSafeString(0);
				genelList.Text = sqlDataReader2.GetSafeString(1);
				list.Add(genelList);
			}
			sqlDataReader2.Close();
		}
		catch
		{
		}
		sqlDB.ConnectionClose();
		string text3 = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text3, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.RaporBekleyenIsler(string firmaid, string UserName, string Password, Stream Content)
	{
		Console.WriteLine("RaporBekleyenIsler Cagrildi");
		DataTable dataTable = new DataTable();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		TeknikTesisRaporSecenekleri teknikTesisRaporSecenekleri = JsonConvert.DeserializeObject<TeknikTesisRaporSecenekleri>(new StreamReader(Content).ReadToEnd());
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			string text2 = "";
			bool flag = true;
			string[] secili_kurumlar = teknikTesisRaporSecenekleri.secili_kurumlar;
			foreach (string text3 in secili_kurumlar)
			{
				if (!flag)
				{
					text2 += ",";
				}
				text2 = text2 + "'" + text3 + "'";
				flag = false;
			}
			string commandText2 = "SELECT b.bkmkb_tarihi AS tarih, b.bkmkb_create_date AS talep_zamani, b.bkmkb_lastup_date AS ekip_atanma_zamani, b.bkmkb_evrakno_seri + '-' + CONVERT(nvarchar, b.bkmkb_evrakno_sira) AS seri_sira, cihaz.chz_aciklama1 + ' ' + cihaz.chz_aciklama2 + ' ' + cihaz.chz_aciklama3 AS ariza_yeri, s.tuk_ismi AS talep_eden_bolum, b.bkmkb_bildirilen_arizalar AS talep_edenin_notu, c.chs_sorun AS bildirilen_ariza, e.ekp_adi AS inceleyecek_ekip FROM BAKIM_KABUL_HAREKETLERI AS b WITH(NOLOCK) INNER JOIN SON_KULLANICILAR AS s WITH(NOLOCK) ON b.bkmkb_tuketici_kodu= s.tuk_kodu INNER JOIN CIHAZ_SORUNLARI AS c WITH(NOLOCK) ON b.bkmkb_ariza_kodu1= c.chs_kodu INNER JOIN EKIP_TANIMLARI AS e WITH(NOLOCK) ON b.bkmkb_inceleyecek_ekip_kodu= e.ekp_kodu INNER JOIN STOK_SERINO_TANIMLARI AS cihaz WITH(NOLOCK) ON b.bkmkb_cihaz_serino= cihaz.chz_serino WHERE b.bkmkb_planlandi_fl= 0 AND b.bkmkb_tarihi>=@baslangic_tarihi AND b.bkmkb_tarihi<=@bitis_tarihi AND b.bkmkb_tuketici_kodu not like '%.BAKIM' AND b.bkmkb_tuketici_kodu in (SELECT tuk_kodu FROM SON_KULLANICILAR WITH(NOLOCK) WHERE tuk_cari_kodu in (" + text2 + "))";
			sqlCommand2.CommandText = commandText2;
			DateTime dateTime = new DateTime(int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(6, 4)), int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(3, 2)), int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(0, 2)));
			DateTime dateTime2 = new DateTime(int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(6, 4)), int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(3, 2)), int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(0, 2)));
			sqlCommand2.Parameters.AddWithValue("@baslangic_tarihi", dateTime);
			sqlCommand2.Parameters.AddWithValue("@bitis_tarihi", dateTime2);
			new SqlDataAdapter(sqlCommand2).Fill(dataTable);
		}
		sqlDB.ConnectionClose();
		string text4 = JsonConvert.SerializeObject(dataTable, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text4, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.RaporBekleyenBakimlar(string firmaid, string UserName, string Password, Stream Content)
	{
		Console.WriteLine("RaporBekleyenIsler Cagrildi");
		DataTable dataTable = new DataTable();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		TeknikTesisRaporSecenekleri teknikTesisRaporSecenekleri = JsonConvert.DeserializeObject<TeknikTesisRaporSecenekleri>(new StreamReader(Content).ReadToEnd());
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			string text2 = "";
			bool flag = true;
			string[] secili_kurumlar = teknikTesisRaporSecenekleri.secili_kurumlar;
			foreach (string text3 in secili_kurumlar)
			{
				if (!flag)
				{
					text2 += ",";
				}
				text2 = text2 + "'" + text3 + "'";
				flag = false;
			}
			string commandText2 = "SELECT b.bkmkb_tarihi AS tarih, b.bkmkb_create_date AS talep_zamani, b.bkmkb_lastup_date AS ekip_atanma_zamani, b.bkmkb_evrakno_seri + '-' + CONVERT(nvarchar, b.bkmkb_evrakno_sira) AS seri_sira, cihaz.chz_aciklama1 + ' ' + cihaz.chz_aciklama2 + ' ' + cihaz.chz_aciklama3 AS ariza_yeri, s.tuk_ismi AS talep_eden_bolum, b.bkmkb_bildirilen_arizalar AS talep_edenin_notu, c.chs_sorun AS bildirilen_ariza, e.ekp_adi AS inceleyecek_ekip FROM BAKIM_KABUL_HAREKETLERI AS b WITH(NOLOCK) INNER JOIN SON_KULLANICILAR AS s WITH(NOLOCK) ON b.bkmkb_tuketici_kodu= s.tuk_kodu INNER JOIN CIHAZ_SORUNLARI AS c WITH(NOLOCK) ON b.bkmkb_ariza_kodu1= c.chs_kodu INNER JOIN EKIP_TANIMLARI AS e WITH(NOLOCK) ON b.bkmkb_inceleyecek_ekip_kodu= e.ekp_kodu INNER JOIN STOK_SERINO_TANIMLARI AS cihaz WITH(NOLOCK) ON b.bkmkb_cihaz_serino= cihaz.chz_serino WHERE b.bkmkb_planlandi_fl= 0 AND b.bkmkb_tarihi>=@baslangic_tarihi AND b.bkmkb_tarihi<=@bitis_tarihi AND b.bkmkb_tuketici_kodu like '%.BAKIM' AND b.bkmkb_tuketici_kodu in (SELECT tuk_kodu FROM SON_KULLANICILAR WITH(NOLOCK) WHERE tuk_cari_kodu in (" + text2 + "))";
			sqlCommand2.CommandText = commandText2;
			DateTime dateTime = new DateTime(int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(6, 4)), int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(3, 2)), int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(0, 2)));
			DateTime dateTime2 = new DateTime(int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(6, 4)), int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(3, 2)), int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(0, 2)));
			sqlCommand2.Parameters.AddWithValue("@baslangic_tarihi", dateTime);
			sqlCommand2.Parameters.AddWithValue("@bitis_tarihi", dateTime2);
			new SqlDataAdapter(sqlCommand2).Fill(dataTable);
		}
		sqlDB.ConnectionClose();
		string text4 = JsonConvert.SerializeObject(dataTable, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text4, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.RaporTamamlananIsler(string firmaid, string UserName, string Password, Stream Content)
	{
		Console.WriteLine("RaporTamamlananIsler Cagrildi");
		DataTable dataTable = new DataTable();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		TeknikTesisRaporSecenekleri teknikTesisRaporSecenekleri = JsonConvert.DeserializeObject<TeknikTesisRaporSecenekleri>(new StreamReader(Content).ReadToEnd());
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			string text2 = "";
			bool flag = true;
			string[] secili_kurumlar = teknikTesisRaporSecenekleri.secili_kurumlar;
			foreach (string text3 in secili_kurumlar)
			{
				if (!flag)
				{
					text2 += ",";
				}
				text2 = text2 + "'" + text3 + "'";
				flag = false;
			}
			string commandText2 = "SELECT b.bkm_tarihi AS tarih, bk.bkmkb_create_date AS talep_zamani, bk.bkmkb_lastup_date AS ekip_atanma_zamani, b.bkm_create_date AS tamamlanma_zamani, CONVERT(varchar(10),DATEDIFF(hh,bkmkb_create_date,bkm_create_date),108) + ' saat ' AS is_sonlandirma_suresi, b.bkm_evrakno_seri + '-' + CONVERT(nvarchar, b.bkm_evrakno_sira) AS seri_sira, s.tuk_ismi AS talep_eden_bolum, cihaz.chz_aciklama1 + ' ' + cihaz.chz_aciklama2 + ' ' + cihaz.chz_aciklama3 AS ariza_yeri, c.chs_sorun AS bildirilen_ariza, e.ekp_adi AS inceleyen_ekip, bk.bkmkb_bildirilen_arizalar AS talep_edenin_notu, (SELECT TOP 1 (egk_evracik1 + ' ' + egk_evracik2 + ' ' + egk_evracik3) FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=97 AND egk_evr_seri=b.bkm_evrakno_seri AND egk_evr_sira=b.bkm_evrakno_sira) AS yapilan_is_aciklamalari FROM BAKIM_HAREKETLERI AS b WITH(NOLOCK) INNER JOIN SON_KULLANICILAR AS s WITH(NOLOCK) ON b.bkm_tuketici_kodu=s.tuk_kodu INNER JOIN CIHAZ_SORUNLARI AS c WITH(NOLOCK) ON b.bkm_ariza_kodu1=c.chs_kodu INNER JOIN EKIP_TANIMLARI AS e WITH(NOLOCK) ON b.bkm_ekip_kodu=e.ekp_kodu INNER JOIN BAKIM_KABUL_HAREKETLERI AS bk WITH(NOLOCK) ON b.bkm_kabul_RECid_RECno=bk.bkmkb_RECid_RECno INNER JOIN STOK_SERINO_TANIMLARI AS cihaz WITH(NOLOCK) ON b.bkm_cihaz_serino=cihaz.chz_serino WHERE bkm_hareket_tipi=1 AND b.bkm_tarihi>=@baslangic_tarihi AND b.bkm_tarihi<=@bitis_tarihi AND b.bkm_tuketici_kodu not like '%.BAKIM' AND b.bkm_tuketici_kodu in (SELECT tuk_kodu FROM SON_KULLANICILAR WITH(NOLOCK) WHERE tuk_cari_kodu in (" + text2 + "))";
			try
			{
				sqlCommand2.CommandText = commandText2;
				DateTime dateTime = new DateTime(int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(6, 4)), int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(3, 2)), int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(0, 2)));
				DateTime dateTime2 = new DateTime(int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(6, 4)), int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(3, 2)), int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(0, 2)));
				sqlCommand2.Parameters.AddWithValue("@baslangic_tarihi", dateTime);
				sqlCommand2.Parameters.AddWithValue("@bitis_tarihi", dateTime2);
				new SqlDataAdapter(sqlCommand2).Fill(dataTable);
			}
			catch
			{
			}
		}
		sqlDB.ConnectionClose();
		string text4 = JsonConvert.SerializeObject(dataTable, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text4, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.RaporTamamlananBakimlar(string firmaid, string UserName, string Password, Stream Content)
	{
		Console.WriteLine("RaporTamamlananIsler Cagrildi");
		DataTable dataTable = new DataTable();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		TeknikTesisRaporSecenekleri teknikTesisRaporSecenekleri = JsonConvert.DeserializeObject<TeknikTesisRaporSecenekleri>(new StreamReader(Content).ReadToEnd());
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			string text2 = "";
			bool flag = true;
			string[] secili_kurumlar = teknikTesisRaporSecenekleri.secili_kurumlar;
			foreach (string text3 in secili_kurumlar)
			{
				if (!flag)
				{
					text2 += ",";
				}
				text2 = text2 + "'" + text3 + "'";
				flag = false;
			}
			string commandText2 = "SELECT b.bkm_tarihi AS tarih, bk.bkmkb_create_date AS talep_zamani, bk.bkmkb_lastup_date AS ekip_atanma_zamani, b.bkm_create_date AS tamamlanma_zamani, CONVERT(varchar(10),DATEDIFF(hh,bkmkb_create_date,bkm_create_date),108) + ' saat ' AS is_sonlandirma_suresi, b.bkm_evrakno_seri + '-' + CONVERT(nvarchar, b.bkm_evrakno_sira) AS seri_sira, s.tuk_ismi AS talep_eden_bolum, cihaz.chz_aciklama1 + ' ' + cihaz.chz_aciklama2 + ' ' + cihaz.chz_aciklama3 AS ariza_yeri, c.chs_sorun AS bildirilen_ariza, e.ekp_adi AS inceleyen_ekip, bk.bkmkb_bildirilen_arizalar AS talep_edenin_notu, (SELECT TOP 1 (egk_evracik1 + ' ' + egk_evracik2 + ' ' + egk_evracik3) FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=97 AND egk_evr_seri=b.bkm_evrakno_seri AND egk_evr_sira=b.bkm_evrakno_sira) AS yapilan_is_aciklamalari FROM BAKIM_HAREKETLERI AS b WITH(NOLOCK) INNER JOIN SON_KULLANICILAR AS s WITH(NOLOCK) ON b.bkm_tuketici_kodu=s.tuk_kodu INNER JOIN CIHAZ_SORUNLARI AS c WITH(NOLOCK) ON b.bkm_ariza_kodu1=c.chs_kodu INNER JOIN EKIP_TANIMLARI AS e WITH(NOLOCK) ON b.bkm_ekip_kodu=e.ekp_kodu INNER JOIN BAKIM_KABUL_HAREKETLERI AS bk WITH(NOLOCK) ON b.bkm_kabul_RECid_RECno=bk.bkmkb_RECid_RECno INNER JOIN STOK_SERINO_TANIMLARI AS cihaz WITH(NOLOCK) ON b.bkm_cihaz_serino=cihaz.chz_serino WHERE bkm_hareket_tipi=1 AND b.bkm_tarihi>=@baslangic_tarihi AND b.bkm_tarihi<=@bitis_tarihi AND b.bkm_tuketici_kodu like '%.BAKIM' AND b.bkm_tuketici_kodu in (SELECT tuk_kodu FROM SON_KULLANICILAR WITH(NOLOCK) WHERE tuk_cari_kodu in (" + text2 + "))";
			try
			{
				sqlCommand2.CommandText = commandText2;
				DateTime dateTime = new DateTime(int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(6, 4)), int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(3, 2)), int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(0, 2)));
				DateTime dateTime2 = new DateTime(int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(6, 4)), int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(3, 2)), int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(0, 2)));
				sqlCommand2.Parameters.AddWithValue("@baslangic_tarihi", dateTime);
				sqlCommand2.Parameters.AddWithValue("@bitis_tarihi", dateTime2);
				new SqlDataAdapter(sqlCommand2).Fill(dataTable);
			}
			catch
			{
			}
		}
		sqlDB.ConnectionClose();
		string text4 = JsonConvert.SerializeObject(dataTable, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text4, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesisAdmin.RaporKullanilanStoklar(string firmaid, string UserName, string Password, Stream Content)
	{
		Console.WriteLine("RaporKullanilanStoklar Cagrildi");
		DataTable dataTable = new DataTable();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimiAdmin userTeknikTesisBakimYonetimiAdmin = new UserTeknikTesisBakimYonetimiAdmin();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT cari_RECno,cari_kod,cari_unvan1,cari_CepTel,cari_EMail FROM CARI_HESAPLAR WHERE cari_CepTel=@sifre AND cari_kod=@kullaniciadi";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("kullaniciadi", UserName);
			sqlCommand.Parameters.AddWithValue("sifre", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimiAdmin.cari_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimiAdmin.cari_kod = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimiAdmin.cari_unvan1 = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimiAdmin.sifre = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimiAdmin.kurumlar = sqlDataReader.GetString(4);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimiAdmin.cari_kod != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		TeknikTesisRaporSecenekleri teknikTesisRaporSecenekleri = JsonConvert.DeserializeObject<TeknikTesisRaporSecenekleri>(new StreamReader(Content).ReadToEnd());
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			string text2 = "";
			bool flag = true;
			string[] secili_kurumlar = teknikTesisRaporSecenekleri.secili_kurumlar;
			foreach (string text3 in secili_kurumlar)
			{
				if (!flag)
				{
					text2 += ",";
				}
				text2 = text2 + "'" + text3 + "'";
				flag = false;
			}
			string commandText2 = "SELECT sh.sth_tarih AS tarih, sh.sth_evrakno_seri + '-' + CONVERT(nvarchar, sh.sth_evrakno_sira) AS seri_sira, sh.sth_stok_kod AS stok_kodu, s.sto_isim AS stok_ismi, sh.sth_miktar AS miktar, s.sto_birim1_ad AS birim, c.cari_unvan1 AS kurum_adi, (SELECT TOP 1 sk.tuk_ismi FROM BAKIM_HAREKETLERI AS bh WITH(NOLOCK)     INNER JOIN SON_KULLANICILAR AS sk WITH(NOLOCK) ON bh.bkm_tuketici_kodu=sk.tuk_kodu WHERE bh.bkm_evrakno_seri=sth_evrakno_seri AND bh.bkm_evrakno_sira=sh.sth_evrakno_sira AND bh.bkm_hareket_tipi=1) AS talep_eden_bolum, (SELECT (cihaz.chz_aciklama1 + ' ' + cihaz.chz_aciklama2 + ' ' + cihaz.chz_aciklama3) FROM STOK_SERINO_TANIMLARI AS cihaz WITH(NOLOCK) WHERE cihaz.chz_serino in ( (SELECT TOP 1 bh.bkm_cihaz_serino FROM BAKIM_HAREKETLERI AS bh WITH(NOLOCK) WHERE bh.bkm_evrakno_seri=sth_evrakno_seri AND bh.bkm_evrakno_sira=sh.sth_evrakno_sira AND bh.bkm_hareket_tipi=1))) AS ariza_yeri, (SELECT TOP 1 cs.chs_sorun FROM BAKIM_HAREKETLERI AS bh WITH(NOLOCK)    INNER JOIN CIHAZ_SORUNLARI AS cs WITH(NOLOCK) ON bh.bkm_ariza_kodu1=cs.chs_kodu WHERE bh.bkm_evrakno_seri=sth_evrakno_seri AND bh.bkm_evrakno_sira=sh.sth_evrakno_sira AND bh.bkm_hareket_tipi=1) AS bildirilen_ariza, (SELECT TOP 1 et.ekp_adi FROM BAKIM_HAREKETLERI AS bh WITH(NOLOCK)    INNER JOIN EKIP_TANIMLARI AS et WITH(NOLOCK) ON bh.bkm_ekip_kodu=et.ekp_kodu WHERE bh.bkm_evrakno_seri=sth_evrakno_seri AND bh.bkm_evrakno_sira=sh.sth_evrakno_sira AND bh.bkm_hareket_tipi=1) AS inceleyen_ekip, (SELECT TOP 1 (egk_evracik1 + ' ' + egk_evracik2 + ' ' + egk_evracik3) FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=97 AND egk_evr_seri=sth_evrakno_seri AND egk_evr_sira=sth_evrakno_sira) AS yapilan_is_aciklamalari FROM STOK_HAREKETLERI AS sh WITH(NOLOCK) INNER JOIN STOKLAR AS s WITH(NOLOCK) ON sh.sth_stok_kod=s.sto_kod INNER JOIN CARI_HESAPLAR AS c WITH(NOLOCK) ON sh.sth_cari_kodu=c.cari_kod WHERE sh.sth_tip=1 AND sh.sth_tarih>=@baslangic_tarihi AND sh.sth_tarih<=@bitis_tarihi AND sth_cari_kodu in (" + text2 + ")";
			try
			{
				sqlCommand2.CommandText = commandText2;
				DateTime dateTime = new DateTime(int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(6, 4)), int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(3, 2)), int.Parse(teknikTesisRaporSecenekleri.baslangic_tarihi.Substring(0, 2)));
				DateTime dateTime2 = new DateTime(int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(6, 4)), int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(3, 2)), int.Parse(teknikTesisRaporSecenekleri.bitis_tarihi.Substring(0, 2)));
				sqlCommand2.Parameters.AddWithValue("@baslangic_tarihi", dateTime);
				sqlCommand2.Parameters.AddWithValue("@bitis_tarihi", dateTime2);
				new SqlDataAdapter(sqlCommand2).Fill(dataTable);
			}
			catch
			{
			}
		}
		sqlDB.ConnectionClose();
		string text4 = JsonConvert.SerializeObject(dataTable, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text4, "application/json;charset=utf-8", Encoding.UTF8);
	}

	Message ITeknikTesis.Login(string firmaid, string UserName, string Password)
	{
		UserTeknikTesisBakimYonetimi userTeknikTesisBakimYonetimi = new UserTeknikTesisBakimYonetimi();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT tuk_RECno,tuk_kodu,tuk_ismi,tuk_Tel2_ModemNo,tuk_email2,tuk_yetkili2,tuk_ceptel2,tuk_cari_kodu,tuk_bolge_kodu FROM SON_KULLANICILAR WHERE tuk_email2=@tuk_email2 AND tuk_yetkili2=@tuk_yetkili2";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("tuk_email2", UserName);
			sqlCommand.Parameters.AddWithValue("tuk_yetkili2", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimi.tuk_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimi.tuk_kodu = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimi.tuk_ismi = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimi.bakim_kabul_evrak_seri = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimi.kullanici_adi = sqlDataReader.GetString(4);
				userTeknikTesisBakimYonetimi.sifre = sqlDataReader.GetString(5);
				userTeknikTesisBakimYonetimi.bakim_kabul_hizmet_kodu = sqlDataReader.GetString(6);
				userTeknikTesisBakimYonetimi.tuk_cari_kodu = sqlDataReader.GetString(7);
				userTeknikTesisBakimYonetimi.tuk_bolge_kodu = sqlDataReader.GetString(8);
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		string text2 = JsonConvert.SerializeObject(userTeknikTesisBakimYonetimi, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message GetCihazGruplari(string firmaid, string UserName, string Password)
	{
		Console.WriteLine("GetCihazGruplari Cagrildi");
		List<CihazGruplari> list = new List<CihazGruplari>();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimi userTeknikTesisBakimYonetimi = new UserTeknikTesisBakimYonetimi();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT tuk_RECno,tuk_kodu,tuk_ismi,tuk_Tel2_ModemNo,tuk_email2,tuk_yetkili2,tuk_ceptel2,tuk_cari_kodu,tuk_bolge_kodu FROM SON_KULLANICILAR WHERE tuk_email2=@tuk_email2 AND tuk_yetkili2=@tuk_yetkili2";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("tuk_email2", UserName);
			sqlCommand.Parameters.AddWithValue("tuk_yetkili2", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimi.tuk_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimi.tuk_kodu = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimi.tuk_ismi = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimi.bakim_kabul_evrak_seri = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimi.kullanici_adi = sqlDataReader.GetString(4);
				userTeknikTesisBakimYonetimi.sifre = sqlDataReader.GetString(5);
				userTeknikTesisBakimYonetimi.bakim_kabul_hizmet_kodu = sqlDataReader.GetString(6);
				userTeknikTesisBakimYonetimi.tuk_cari_kodu = sqlDataReader.GetString(7);
				userTeknikTesisBakimYonetimi.tuk_bolge_kodu = sqlDataReader.GetString(8);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimi.kullanici_adi != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		list = CihazGruplariData.GetCihazGruplari(sqlDB.Connection, userTeknikTesisBakimYonetimi.tuk_kodu);
		sqlDB.ConnectionClose();
		string text2 = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message GetCihazGenelList(string firmaid, string UserName, string Password, string cg_kodu)
	{
		Console.WriteLine("GetCihazGenelList Cagrildi");
		List<GenelList> list = new List<GenelList>();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimi userTeknikTesisBakimYonetimi = new UserTeknikTesisBakimYonetimi();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT tuk_RECno,tuk_kodu,tuk_ismi,tuk_Tel2_ModemNo,tuk_email2,tuk_yetkili2,tuk_ceptel2,tuk_cari_kodu,tuk_bolge_kodu FROM SON_KULLANICILAR WHERE tuk_email2=@tuk_email2 AND tuk_yetkili2=@tuk_yetkili2";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("tuk_email2", UserName);
			sqlCommand.Parameters.AddWithValue("tuk_yetkili2", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimi.tuk_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimi.tuk_kodu = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimi.tuk_ismi = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimi.bakim_kabul_evrak_seri = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimi.kullanici_adi = sqlDataReader.GetString(4);
				userTeknikTesisBakimYonetimi.sifre = sqlDataReader.GetString(5);
				userTeknikTesisBakimYonetimi.bakim_kabul_hizmet_kodu = sqlDataReader.GetString(6);
				userTeknikTesisBakimYonetimi.tuk_cari_kodu = sqlDataReader.GetString(7);
				userTeknikTesisBakimYonetimi.tuk_bolge_kodu = sqlDataReader.GetString(8);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimi.kullanici_adi != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		list = CihazData.GetCihazGenelList(sqlDB.Connection, userTeknikTesisBakimYonetimi.tuk_kodu, cg_kodu);
		sqlDB.ConnectionClose();
		string text2 = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message GetCihazSorunuGrubuList(string firmaid, string UserName, string Password, string chz_serino)
	{
		Console.WriteLine("GetCihazSorunuGrubuList Cagrildi");
		List<GenelList> list = new List<GenelList>();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimi userTeknikTesisBakimYonetimi = new UserTeknikTesisBakimYonetimi();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT tuk_RECno,tuk_kodu,tuk_ismi,tuk_Tel2_ModemNo,tuk_email2,tuk_yetkili2,tuk_ceptel2,tuk_cari_kodu,tuk_bolge_kodu FROM SON_KULLANICILAR WHERE tuk_email2=@tuk_email2 AND tuk_yetkili2=@tuk_yetkili2";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("tuk_email2", UserName);
			sqlCommand.Parameters.AddWithValue("tuk_yetkili2", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimi.tuk_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimi.tuk_kodu = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimi.tuk_ismi = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimi.bakim_kabul_evrak_seri = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimi.kullanici_adi = sqlDataReader.GetString(4);
				userTeknikTesisBakimYonetimi.sifre = sqlDataReader.GetString(5);
				userTeknikTesisBakimYonetimi.bakim_kabul_hizmet_kodu = sqlDataReader.GetString(6);
				userTeknikTesisBakimYonetimi.tuk_cari_kodu = sqlDataReader.GetString(7);
				userTeknikTesisBakimYonetimi.tuk_bolge_kodu = sqlDataReader.GetString(8);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimi.kullanici_adi != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		string cihazStokKod = CihazData.GetCihazStokKod(sqlDB.Connection, chz_serino);
		Stok stok = StokData.GetStok(sqlDB.Connection, cihazStokKod, enum_toptan_perakende.Toptan);
		list = CihazSorunlariData.GetCihazSorunlariGrubuGenelList(sqlDB.Connection, stok.sto_anagrup_kod);
		sqlDB.ConnectionClose();
		string text2 = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message GetCihazSorunuList(string firmaid, string UserName, string Password, string chz_serino, string agr_kodu)
	{
		Console.WriteLine("GetCihazSorunuList Cagrildi");
		List<GenelList> list = new List<GenelList>();
		string dBName = "";
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimi userTeknikTesisBakimYonetimi = new UserTeknikTesisBakimYonetimi();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT tuk_RECno,tuk_kodu,tuk_ismi,tuk_Tel2_ModemNo,tuk_email2,tuk_yetkili2,tuk_ceptel2,tuk_cari_kodu,tuk_bolge_kodu FROM SON_KULLANICILAR WHERE tuk_email2=@tuk_email2 AND tuk_yetkili2=@tuk_yetkili2";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("tuk_email2", UserName);
			sqlCommand.Parameters.AddWithValue("tuk_yetkili2", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimi.tuk_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimi.tuk_kodu = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimi.tuk_ismi = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimi.bakim_kabul_evrak_seri = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimi.kullanici_adi = sqlDataReader.GetString(4);
				userTeknikTesisBakimYonetimi.sifre = sqlDataReader.GetString(5);
				userTeknikTesisBakimYonetimi.bakim_kabul_hizmet_kodu = sqlDataReader.GetString(6);
				userTeknikTesisBakimYonetimi.tuk_cari_kodu = sqlDataReader.GetString(7);
				userTeknikTesisBakimYonetimi.tuk_bolge_kodu = sqlDataReader.GetString(8);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimi.kullanici_adi != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		string cihazStokKod = CihazData.GetCihazStokKod(sqlDB.Connection, chz_serino);
		Stok stok = StokData.GetStok(sqlDB.Connection, cihazStokKod, enum_toptan_perakende.Toptan);
		list = CihazSorunlariData.GetCihazSorunlariGenelList(sqlDB.Connection, agr_kodu, stok.sto_anagrup_kod);
		sqlDB.ConnectionClose();
		string text2 = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text2, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message SaveBakimTalebi(string firmaid, string UserName, string Password, Stream Content)
	{
		Console.WriteLine("SaveBakimTalebi Cagrildi");
		string text = "";
		string dBName = "";
		string text2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text2 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text2 + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
		UserTeknikTesisBakimYonetimi userTeknikTesisBakimYonetimi = new UserTeknikTesisBakimYonetimi();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			string commandText = "SELECT tuk_RECno,tuk_kodu,tuk_ismi,tuk_Tel2_ModemNo,tuk_email2,tuk_yetkili2,tuk_ceptel2,tuk_cari_kodu,tuk_bolge_kodu FROM SON_KULLANICILAR WHERE tuk_email2=@tuk_email2 AND tuk_yetkili2=@tuk_yetkili2";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("tuk_email2", UserName);
			sqlCommand.Parameters.AddWithValue("tuk_yetkili2", Password);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				userTeknikTesisBakimYonetimi.tuk_RECno = sqlDataReader.GetInt32(0);
				userTeknikTesisBakimYonetimi.tuk_kodu = sqlDataReader.GetString(1);
				userTeknikTesisBakimYonetimi.tuk_ismi = sqlDataReader.GetString(2);
				userTeknikTesisBakimYonetimi.bakim_kabul_evrak_seri = sqlDataReader.GetString(3);
				userTeknikTesisBakimYonetimi.kullanici_adi = sqlDataReader.GetString(4);
				userTeknikTesisBakimYonetimi.sifre = sqlDataReader.GetString(5);
				userTeknikTesisBakimYonetimi.bakim_kabul_hizmet_kodu = sqlDataReader.GetString(6);
				userTeknikTesisBakimYonetimi.tuk_cari_kodu = sqlDataReader.GetString(7);
				userTeknikTesisBakimYonetimi.tuk_bolge_kodu = sqlDataReader.GetString(8);
			}
			sqlDataReader.Close();
		}
		if (userTeknikTesisBakimYonetimi.kullanici_adi != UserName)
		{
			sqlDB.ConnectionClose();
			return WebOperationContext.Current.CreateTextResponse("Hata : Kullanıcı adı veya şifre hatalı", "application/json;charset=utf-8", Encoding.UTF8);
		}
		List<string> list = JsonConvert.DeserializeObject<List<string>>(new StreamReader(Content).ReadToEnd());
		string text3 = list[0];
		string bkmkb_ariza_kodu = list[1];
		string bkmkb_bildirilen_arizalar = list[2];
		string cihazStokKod = CihazData.GetCihazStokKod(sqlDB.Connection, text3);
		BAKIM_KABUL_HAREKETLERI bAKIM_KABUL_HAREKETLERI = new BAKIM_KABUL_HAREKETLERI();
		bAKIM_KABUL_HAREKETLERI.bkmkb_RECno = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_RECid_DBCno = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_RECid_RECno = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_Spec_Rec_no = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_iptal = false;
		bAKIM_KABUL_HAREKETLERI.bkmkb_fileid = 147;
		bAKIM_KABUL_HAREKETLERI.bkmkb_hidden = false;
		bAKIM_KABUL_HAREKETLERI.bkmkb_kilitli = false;
		bAKIM_KABUL_HAREKETLERI.bkmkb_degisti = false;
		bAKIM_KABUL_HAREKETLERI.bkmkb_checksum = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_create_user = 2;
		bAKIM_KABUL_HAREKETLERI.bkmkb_create_date = DateTime.Now;
		bAKIM_KABUL_HAREKETLERI.bkmkb_lastup_user = 2;
		bAKIM_KABUL_HAREKETLERI.bkmkb_lastup_date = DateTime.Now;
		bAKIM_KABUL_HAREKETLERI.bkmkb_special1 = "FORA";
		bAKIM_KABUL_HAREKETLERI.bkmkb_special2 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_special3 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_firmano = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_subeno = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_seri = userTeknikTesisBakimYonetimi.bakim_kabul_evrak_seri;
		bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_sira = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_satirno = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_belgeno = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_belge_tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		bAKIM_KABUL_HAREKETLERI.bkmkb_cihaz_serino = text3;
		bAKIM_KABUL_HAREKETLERI.bkmkb_fis_stok_kodu = cihazStokKod;
		bAKIM_KABUL_HAREKETLERI.bkmkb_tuketici_kodu = userTeknikTesisBakimYonetimi.tuk_kodu;
		bAKIM_KABUL_HAREKETLERI.bkmkb_talep_gelis_sekli = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_gelis_kargo_kodu = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_gelis_kargo_belgeno = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_gelis_irsaliyeno = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_servis_turu = 1;
		bAKIM_KABUL_HAREKETLERI.bkmkb_servis_yeri = 1;
		bAKIM_KABUL_HAREKETLERI.bkmkb_aksesuarlar = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_bildirilen_arizalar = bkmkb_bildirilen_arizalar;
		bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_alinma_tarihi = DateTime.Now;
		bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_edilme_tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_edilme_sekli = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu1 = bkmkb_ariza_kodu;
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu2 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu3 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu4 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu5 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu6 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu7 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu8 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu9 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu10 = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_bilgilendirme_sekli = 0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_inceleyecek_ekip_kodu = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_depono = 1;
		bAKIM_KABUL_HAREKETLERI.bkmkb_aciklama = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_hareket_tipi = 1;
		bAKIM_KABUL_HAREKETLERI.bkmkb_stok_hizmet_kodu = userTeknikTesisBakimYonetimi.bakim_kabul_hizmet_kodu;
		bAKIM_KABUL_HAREKETLERI.bkmkb_operasyon_suresi = 600;
		bAKIM_KABUL_HAREKETLERI.bkmkb_miktari = 1.0;
		bAKIM_KABUL_HAREKETLERI.bkmkb_satir_aciklama = "";
		bAKIM_KABUL_HAREKETLERI.bkmkb_planlandi_fl = false;
		bAKIM_KABUL_HAREKETLERI.bkmkb_adres_no = 0;
		Evrak evrak = new Evrak(enum_GenelEvrakTipleri.BakimTalep, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		evrak.SetEvrakNoSeri(userTeknikTesisBakimYonetimi.bakim_kabul_evrak_seri);
		evrak.AddBakimKabulHareketi(bAKIM_KABUL_HAREKETLERI);
		SqlTransaction sqlTransaction = sqlDB.Connection.BeginTransaction();
		try
		{
			int num = EvrakData.EvrakKaydet(sqlDB.Connection, sqlTransaction, dBName, evrak, EArsivAktif: false);
			text = ((num <= 0) ? "Sistemden kaynaklanan bir sebepten dolayı talebiniz kayıt edilemedi. Lütfen daha sonra tekrar deneyiniz." : ("Bakım talebiniz " + evrak.EvrakNoSeri + "-" + num + " takip numarası ile alınmıştır."));
			sqlTransaction.Commit();
		}
		catch
		{
			sqlTransaction.Rollback();
			text = "Sistemden kaynaklanan bir sebepten dolayı talebiniz kayıt edilemedi. Lütfen daha sonra tekrar deneyiniz.";
		}
		sqlDB.ConnectionClose();
		string text4 = JsonConvert.SerializeObject(text, Newtonsoft.Json.Formatting.None);
		return WebOperationContext.Current.CreateTextResponse(text4, "application/json;charset=utf-8", Encoding.UTF8);
	}

	public Message GetCihazDurumu(string firmaid, string cari_vergino)
	{
		Console.WriteLine("GetCihazDurumu Cagrildi");
		string text = "";
		string dBName = "";
		string text2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		SqlBaglantiBilgileri baglantiBilgileri;
		try
		{
			baglantiBilgileri = new SqlBaglantiBilgileri(text2 + "data\\sqlbaglantibilgileri.xml");
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			_ = "Hata: " + ex.ToString();
			throw ex;
		}
		try
		{
			using FileStream stream = File.OpenRead(text2 + "data\\servisbilgileri.xml");
			XElement xElement = (from x in XDocument.Load(stream).Descendants("Firma")
				where (string)x.Attribute("id") == firmaid
				select x).FirstOrDefault();
			if (xElement != null)
			{
				dBName = (string)xElement.Attribute("MikroDbName");
			}
		}
		catch (Exception ex2)
		{
			Log_Error(ex2.ToString());
			_ = "Hata: " + ex2.ToString();
			throw ex2;
		}
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantiBilgileri, dBName);
			SqlCommand sqlCommand = new SqlCommand("SELECT bkh.bkmkb_tarihi,bkh.bkmkb_cihaz_serino,s.sto_isim,bkh.bkmkb_bildirilen_arizalar,bkh.bkmkb_satir_aciklama, CASE  WHEN ((SELECT COUNT(*) FROM BAKIM_HAREKETLERI WHERE bkm_kabul_RECid_RECno=bkh.bkmkb_RECid_RECno)<1) AND bkh.bkmkb_special1<>'OB' THEN 'İşlem devam ediyor' WHEN ((SELECT COUNT(*) FROM BAKIM_HAREKETLERI WHERE bkm_kabul_RECid_RECno=bkh.bkmkb_RECid_RECno)<1) AND bkh.bkmkb_special1='OB' THEN 'Onay bekliyor' WHEN bkh.bkmkb_teslim_edilme_sekli=1 AND CAST(DATEPART(HOUR, bkh.bkmkb_teslim_edilme_tarihi) as varchar(2))='0' AND CAST(DATEPART(MINUTE, bkh.bkmkb_teslim_edilme_tarihi) as varchar(2))='0' THEN 'Kargoya verilecek' WHEN bkh.bkmkb_teslim_edilme_sekli=1 AND (CAST(DATEPART(HOUR, bkh.bkmkb_teslim_edilme_tarihi) as varchar(2))<>'0' OR CAST(DATEPART(MINUTE, bkh.bkmkb_teslim_edilme_tarihi) as varchar(2))<>'0') THEN 'Kargoya verildi' WHEN bkh.bkmkb_teslim_edilme_sekli=0 AND CAST(DATEPART(HOUR, bkh.bkmkb_teslim_edilme_tarihi) as varchar(2))='0' AND CAST(DATEPART(MINUTE, bkh.bkmkb_teslim_edilme_tarihi) as varchar(2))='0' THEN 'Almanızı bekliyor' WHEN bkh.bkmkb_teslim_edilme_sekli=0 AND (CAST(DATEPART(HOUR, bkh.bkmkb_teslim_edilme_tarihi) as varchar(2))<>'0' OR CAST(DATEPART(MINUTE, bkh.bkmkb_teslim_edilme_tarihi) as varchar(2))<>'0') THEN 'Elden teslim edildi' ELSE '-' END AS Durumu FROM BAKIM_KABUL_HAREKETLERI AS bkh WITH(NOLOCK) INNER JOIN STOKLAR AS s WITH(NOLOCK) ON s.sto_kod=bkh.bkmkb_fis_stok_kodu WHERE bkmkb_tuketici_kodu in ( SELECT tuk_kodu FROM SON_KULLANICILAR WHERE tuk_cari_kodu in ( SELECT cari_kod FROM CARI_HESAPLAR WHERE cari_vdaire_no=@cari_vdaire_no OR cari_CepTel=@cari_vdaire_no)) AND bkh.bkmkb_tarihi>DATEADD(DAY,-120,GETDATE()) ORDER BY bkh.bkmkb_tarihi DESC", sqlDB.Connection);
			sqlCommand.CommandType = CommandType.Text;
			sqlCommand.Parameters.AddWithValue("@cari_vdaire_no", cari_vergino);
			SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
			sqlDataAdapter.SelectCommand = sqlCommand;
			DataTable dataTable = new DataTable();
			sqlDataAdapter.Fill(dataTable);
			dataTable.TableName = "SONUC";
			sqlDB.ConnectionClose();
			text = JsonConvert.SerializeObject(dataTable, Newtonsoft.Json.Formatting.None);
		}
		catch (Exception ex3)
		{
			text = "Hata : " + ex3.ToString();
		}
		return WebOperationContext.Current.CreateTextResponse(text, "application/json;charset=utf-8", Encoding.UTF8);
	}

	[DllImport("kernel32.dll")]
	private static extern long GetVolumeInformation(string PathName, StringBuilder VolumeNameBuffer, uint VolumeNameSize, ref uint VolumeSerialNumber, ref uint MaximumComponentLength, ref uint FileSystemFlags, StringBuilder FileSystemNameBuffer, uint FileSystemNameSize);

	public Message GetLisans(string modul)
	{
		Lisans lisans = new Lisans();
		try
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string text = "yok";
			using (StreamReader textReader = File.OpenText(directoryName + "\\data\\lisans.xml"))
			{
				text = XDocument.Load(textReader).Root.Element("firma").Value;
			}
			string text2 = "";
			string text3 = "C";
			try
			{
				if (text3 == string.Empty)
				{
					DriveInfo[] drives = DriveInfo.GetDrives();
					foreach (DriveInfo driveInfo in drives)
					{
						if (driveInfo.IsReady)
						{
							text3 = driveInfo.RootDirectory.ToString();
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log_Error(ex.ToString());
			}
			if (text3.EndsWith(":\\"))
			{
				text3 = text3.Substring(0, text3.Length - 2);
			}
			string text4 = "";
			string text5 = "";
			string text6 = "";
			try
			{
				text4 = getVolumeSerial(text3);
			}
			catch (Exception ex2)
			{
				Log_Error(ex2.ToString());
			}
			try
			{
				text5 = getCPUID();
			}
			catch (Exception ex3)
			{
				Log_Error(ex3.ToString());
			}
			try
			{
				text6 = GetMotherBoardID();
			}
			catch (Exception ex4)
			{
				Log_Error(ex4.ToString());
			}
			text2 = text;
			if (text5 != null && text5.Length >= 13)
			{
				text2 += text5.Substring(13);
				text2 += text5.Substring(1, 4);
			}
			if (text4 != null)
			{
				text2 += text4;
			}
			if (text5 != null && text5.Length >= 8)
			{
				text2 += text5.Substring(4, 4);
			}
			if (text6 != null)
			{
				text2 += text6;
			}
			text2 = text2.Replace(".", "");
			text2 = text2.Replace(",", "");
			text2 = text2.Replace("\\", "");
			text2 = text2.Replace("/", "");
			if (File.Exists(directoryName + "\\licance.lic"))
			{
				StreamReader streamReader = new StreamReader(directoryName + "\\licance.lic");
				string[] array = GenelUtilityWin.DecryptText(text2, streamReader.ReadLine(), useHashing: true).Split('|');
				try
				{
					lisans.LisansSonlanmaTarihi = DateTime.ParseExact(array[0], "yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
				}
				catch
				{
					lisans.LisansSonlanmaTarihi = DateTime.Parse(array[0]);
				}
				lisans.LisansliModuller = array[1];
				lisans.LisansSahibi = array[2];
				streamReader.Close();
			}
			if (lisans.LisansSonlanmaTarihi < DateTime.Now)
			{
				lisans.LisansSonlanmaTarihi = new DateTime(1980, 1, 1);
				lisans.LisansliModuller = "";
				lisans.LisansSahibi = "";
				try
				{
					File.Delete(directoryName + "\\licance.lic");
				}
				catch (Exception ex5)
				{
					Log_Error(ex5.ToString());
				}
			}
			bool flag = false;
			if (lisans.LisansSonlanmaTarihi < DateTime.Now.AddDays(2.0))
			{
				flag = true;
			}
			if (!lisans.LisansliModuller.Contains(modul))
			{
				flag = true;
			}
			if (flag)
			{
				Lisans lisansFromServer = GetLisansFromServer(text, text2);
				if (lisansFromServer.LisansSonlanmaTarihi > DateTime.Now)
				{
					try
					{
						File.Delete(directoryName + "\\licance.lic");
						StreamWriter streamWriter = new StreamWriter(directoryName + "\\licance.lic");
						string toEncrypt = lisansFromServer.LisansSonlanmaTarihi.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ") + "|" + lisansFromServer.LisansliModuller + "|" + lisansFromServer.LisansSahibi;
						streamWriter.WriteLine(GenelUtilityWin.EncryptText(text2, toEncrypt, useHashing: true));
						streamWriter.Close();
						lisans = lisansFromServer;
					}
					catch (Exception ex6)
					{
						Log_Error(ex6.ToString());
					}
				}
			}
			return WebOperationContext.Current.CreateTextResponse(lisans.LisansliModuller + "|" + lisans.LisansSahibi, "application/json;charset=utf-8", Encoding.UTF8);
		}
		catch (Exception ex7)
		{
			Log_Error(ex7.ToString());
			return WebOperationContext.Current.CreateTextResponse("Hata : " + ex7.ToString(), "application/json;charset=utf-8", Encoding.UTF8);
		}
	}

	private static Lisans GetLisansFromServer(string firma_id, string makine_id)
	{
		Lisans lisans = new Lisans();
		string text = "";
		try
		{
			Encoding uTF = Encoding.UTF8;
			text = "http://lm.forayazilim.com/GetLisansV2/" + firma_id + "/" + makine_id;
			HttpWebRequest obj = WebRequest.Create(text) as HttpWebRequest;
			obj.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
			obj.AutomaticDecompression = DecompressionMethods.GZip;
			obj.Credentials = CredentialCache.DefaultCredentials;
			obj.Timeout = 20000;
			obj.ReadWriteTimeout = 20000;
			obj.Method = "GET";
			if (obj.GetResponse() is HttpWebResponse { StatusCode: HttpStatusCode.OK } httpWebResponse)
			{
				StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), uTF);
				string[] array = streamReader.ReadToEnd().Split('|');
				streamReader.Close();
				httpWebResponse.Close();
				lisans = new Lisans();
				lisans.LisansSonlanmaTarihi = DateTime.ParseExact(array[0], "yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
				lisans.LisansliModuller = array[1];
				lisans.LisansSahibi = array[2];
			}
		}
		catch (Exception ex)
		{
			Log_Error("Makine ID : " + makine_id + " url : " + text + " " + ex.ToString());
		}
		return lisans;
	}

	private string getVolumeSerial(string drive)
	{
		try
		{
			ManagementObject managementObject = new ManagementObject("win32_logicaldisk.deviceid=\"" + drive + ":\"");
			managementObject.Get();
			string result = managementObject["VolumeSerialNumber"].ToString();
			managementObject.Dispose();
			return result;
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			return "";
		}
	}

	private string getCPUID()
	{
		try
		{
			string text = "";
			foreach (ManagementObject instance in new ManagementClass("win32_processor").GetInstances())
			{
				if (text == "")
				{
					text = instance.Properties["processorID"].Value.ToString();
					break;
				}
			}
			return text;
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			return "";
		}
	}

	public static string GetMotherBoardID()
	{
		try
		{
			ManagementScope managementScope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
			managementScope.Connect();
			string result = "";
			using (ManagementObject managementObject = new ManagementObject(managementScope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions()))
			{
				result = managementObject.Properties["SerialNumber"].Value.ToString();
			}
			return result;
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
			return "";
		}
	}
}
