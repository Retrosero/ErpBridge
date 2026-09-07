using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Utility;

namespace Fora.App.Win.Mikro.Service;

internal class MikroEntegrasyonServisi : IMikroEntegrasyonServisi
{
	public FiyatDegisikleriResult FiyatDegisiklikleri(string MikroVeritabaniAdi, int fiyat_liste_no, DateTime LastUpdate)
	{
		FiyatDegisikleriResult fiyatDegisikleriResult = new FiyatDegisikleriResult();
		fiyatDegisikleriResult.hatamesaji = "";
		fiyatDegisikleriResult.sonuc = "OK";
		fiyatDegisikleriResult.liste = new List<StokItemBase>();
		SqlBaglantiBilgileri sqlBaglantiBilgileri = new SqlBaglantiBilgileri();
		try
		{
			string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
			try
			{
				sqlBaglantiBilgileri = new SqlBaglantiBilgileri(text + "data\\sqlbaglantibilgileri.xml");
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		catch (Exception ex2)
		{
			fiyatDegisikleriResult.sonuc = "HATA";
			fiyatDegisikleriResult.hatamesaji = ex2.ToString();
			return fiyatDegisikleriResult;
		}
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(sqlBaglantiBilgileri, MikroVeritabaniAdi);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT sto_kod,sto_isim,sto_birim1_ad,sto_birim2_ad,sto_birim3_ad,sto_birim4_ad,sto_perakende_vergi,sto_toptan_vergi,sfiyat_fiyati FROM STOK_SATIS_FIYAT_LISTELERI INNER JOIN STOKLAR ON sfiyat_stokkod=sto_kod WHERE sfiyat_lastup_date>=@sfiyat_lastup_date AND sfiyat_listesirano=@sfiyat_listesirano";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@sfiyat_lastup_date", LastUpdate);
				sqlCommand.Parameters.AddWithValue("sfiyat_listesirano", fiyat_liste_no);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					StokItemBase stokItemBase = new StokItemBase();
					stokItemBase.sto_kod = sqlDataReader.GetSafeString(0);
					stokItemBase.sto_isim = sqlDataReader.GetSafeString(1);
					stokItemBase.sto_birim1_ad = sqlDataReader.GetSafeString(2);
					stokItemBase.sto_birim2_ad = sqlDataReader.GetSafeString(3);
					stokItemBase.sto_birim3_ad = sqlDataReader.GetSafeString(4);
					stokItemBase.sto_birim4_ad = sqlDataReader.GetSafeString(5);
					stokItemBase.sto_perakende_vergi = sqlDataReader.GetSafeByte(6);
					stokItemBase.sto_toptan_vergi = sqlDataReader.GetSafeByte(7);
					stokItemBase.birim_fiyat = sqlDataReader.GetSafeDouble(8);
					fiyatDegisikleriResult.liste.Add(stokItemBase);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch (Exception ex3)
		{
			fiyatDegisikleriResult.sonuc = "HATA";
			fiyatDegisikleriResult.hatamesaji = ex3.ToString();
		}
		return fiyatDegisikleriResult;
	}
}
