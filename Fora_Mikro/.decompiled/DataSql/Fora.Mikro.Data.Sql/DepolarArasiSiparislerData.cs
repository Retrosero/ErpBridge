using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Siparis;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DepolarArasiSiparislerData
{
	public static double GetDepolarArasiSiparisKalanMiktar(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int ssip_RECid_RECno)
	{
		double result = 0.0;
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		try
		{
			using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
			sqlCommand.CommandText = "SELECT (ssip_miktar-ssip_teslim_miktar) AS kalanmiktar FROM DEPOLAR_ARASI_SIPARISLER WHERE ssip_RECid_RECno=@ssip_RECid_RECno";
			sqlCommand.Parameters.AddWithValue("@ssip_RECid_RECno", ssip_RECid_RECno);
			object obj = sqlCommand.ExecuteScalar();
			if (obj != null)
			{
				result = (double)obj;
			}
		}
		catch
		{
		}
		sqlDB.ConnectionClose();
		return result;
	}

	public static double GetDepolarArasiSiparisKalanMiktar(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid ssip_uid)
	{
		double result = 0.0;
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		try
		{
			using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
			sqlCommand.CommandText = "SELECT (ssip_miktar-ssip_teslim_miktar) AS kalanmiktar FROM DEPOLAR_ARASI_SIPARISLER WHERE ssip_Guid=@ssip_Guid";
			sqlCommand.Parameters.AddWithValue("@ssip_Guid", ssip_uid);
			object obj = sqlCommand.ExecuteScalar();
			if (obj != null)
			{
				result = (double)obj;
			}
		}
		catch
		{
		}
		sqlDB.ConnectionClose();
		return result;
	}

	public static List<SiparisEvrakListItem> GetDepolarArasiSiparisEvrakList(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string SiparisSeri, int kaynak_depo_no, int hedef_depo_no, enum_sip_orderby Siralama)
	{
		List<SiparisEvrakListItem> list = new List<SiparisEvrakListItem>();
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			string text = "SELECT TOP 100 ";
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				text += " ssip_evrakno_seri,ssip_evrakno_sira,ssip_tarih,MIN(ssip_teslim_tarih) AS ssip_teslim_tarih,SUM(ssip_miktar) AS ssip_miktar,SUM(ssip_teslim_miktar) AS ssip_teslim_miktar FROM DEPOLAR_ARASI_SIPARISLER ";
				string text2 = "WHERE";
				if (SiparisSeri != "")
				{
					text = text + text2 + " ssip_evrakno_seri=@ssip_evrakno_seri ";
					sqlCommand.Parameters.AddWithValue("@ssip_evrakno_seri", SiparisSeri);
					text2 = "AND";
				}
				if (hedef_depo_no != 0)
				{
					text = text + text2 + " ssip_girdepo=@ssip_girdepo ";
					sqlCommand.Parameters.AddWithValue("@ssip_girdepo", hedef_depo_no);
					text2 = "AND";
				}
				if (kaynak_depo_no != 0)
				{
					text = text + text2 + " ssip_cikdepo=@ssip_cikdepo ";
					sqlCommand.Parameters.AddWithValue("@ssip_cikdepo", kaynak_depo_no);
				}
				text += "GROUP BY ssip_evrakno_seri,ssip_evrakno_sira,ssip_tarih";
				switch (Siralama)
				{
				case enum_sip_orderby.SeriSira:
					text += " ORDER BY ssip_evrakno_seri,ssip_evrakno_sira";
					break;
				case enum_sip_orderby.SiparisTarihi:
					text += " ORDER BY ssip_tarih DESC,ssip_evrakno_seri,ssip_evrakno_sira DESC";
					break;
				case enum_sip_orderby.TeslimTarihi:
					text += " ORDER BY ssip_teslim_tarih DESC,ssip_evrakno_seri,ssip_evrakno_sira DESC";
					break;
				}
				sqlCommand.CommandText = text;
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					SiparisEvrakListItem siparisEvrakListItem = new SiparisEvrakListItem();
					siparisEvrakListItem.sip_evrakno_seri = sqlDataReader.GetSafeString(0);
					siparisEvrakListItem.sip_evrakno_sira = sqlDataReader.GetSafeInt32(1);
					siparisEvrakListItem.sip_tarih = sqlDataReader.GetSafeDateTime(2);
					siparisEvrakListItem.sip_teslim_tarih = sqlDataReader.GetSafeDateTime(3);
					siparisEvrakListItem.sip_miktar = sqlDataReader.GetSafeDouble(4);
					siparisEvrakListItem.sip_teslim_miktar = sqlDataReader.GetSafeDouble(5);
					siparisEvrakListItem.sip_tip = enum_sip_tip.Talep;
					siparisEvrakListItem.sip_cins = enum_sip_cins.DepolarArasiSiparis;
					siparisEvrakListItem.sip_musteri_kod = "";
					list.Add(siparisEvrakListItem);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return list;
	}
}
