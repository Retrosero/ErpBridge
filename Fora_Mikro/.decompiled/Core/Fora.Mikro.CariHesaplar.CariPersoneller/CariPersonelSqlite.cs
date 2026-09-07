using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Enumler;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.CariHesaplar.CariPersoneller;

public static class CariPersonelSqlite
{
	public static List<GenelList> GetCariPersonelGenelList(SqliteConnection connection, string GorebilecegiTemsilciler)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		string text = "SELECT cari_per_kod,cari_per_adi FROM CARI_PERSONEL_TANIMLARI";
		if (GorebilecegiTemsilciler != "")
		{
			text = text + " WHERE cari_per_kod in ('" + GorebilecegiTemsilciler.Replace(",", "','") + "')";
		}
		text += " ORDER BY cari_per_adi";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = text;
			val.Connection = connection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = val2.GetSafeString(0);
				genelList.Text = val2.GetSafeString(1);
				list.Add(genelList);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch
		{
		}
		return list;
	}

	public static CariPersonel GetCariPersonel(SqliteConnection OpenedConnection, string cari_per_kod)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		CariPersonel cariPersonel = new CariPersonel();
		string commandText = (((DbCommand)new SqliteCommand()).CommandText = "SELECT cari_per_kod,cari_per_adi,cari_per_soyadi,cari_per_tip,cari_per_doviz_cinsi,cari_per_kasiyerkodu,cari_per_kasiyersifresi,cari_per_kasiyerAmiri,cari_per_userno,cari_per_depono ,cari_per_cepno,cari_per_mail FROM CARI_PERSONEL_TANIMLARI WHERE cari_per_kod=@cari_per_kod");
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@cari_per_kod", (object)cari_per_kod);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					cariPersonel.cari_per_kod = val2.GetSafeString(0);
					cariPersonel.cari_per_adi = val2.GetSafeString(1);
					cariPersonel.cari_per_soyadi = val2.GetSafeString(2);
					cariPersonel.cari_per_tip = (enum_Cari_Personel_Tip)val2.GetSafeByte(3);
					cariPersonel.cari_per_doviz_cinsi = val2.GetSafeByte(4);
					cariPersonel.cari_per_kasiyerkodu = val2.GetSafeString(5);
					cariPersonel.cari_per_kasiyersifresi = val2.GetSafeString(6);
					cariPersonel.cari_per_kasiyerAmiri = val2.GetSafeString(7);
					cariPersonel.cari_per_userno = val2.GetSafeInt32(8);
					cariPersonel.cari_per_depono = val2.GetSafeInt32(9);
					cariPersonel.cari_per_cepno = val2.GetSafeString(10);
					cariPersonel.cari_per_mail = val2.GetSafeString(11);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return cariPersonel;
	}

	public static List<CariPersonel> GetCariPersonelByForaCariBolgeKodu(SqliteConnection OpenedConnection, string cari_bolge_kodu, string GorebilecegiTemsilciler)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<CariPersonel> list = new List<CariPersonel>();
		SqliteCommand val = new SqliteCommand();
		string text = "SELECT cari_per_kod,cari_per_adi,cari_per_soyadi,cari_per_tip,cari_per_doviz_cinsi,cari_per_kasiyerkodu,cari_per_kasiyersifresi,cari_per_kasiyerAmiri,cari_per_userno,cari_per_depono ,cari_per_cepno,cari_per_mail FROM CARI_PERSONEL_TANIMLARI WITH WHERE cari_per_kod in (SELECT ParametreDegeri FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreAdi='CariPersonelKodu' AND ParametreUser in (SELECT ParametreUser FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreAdi='CariBolgeKodu' AND ParametreDegeri=@bolge_kodu))";
		if (GorebilecegiTemsilciler != "")
		{
			text = text + " AND cari_per_kod in ('" + GorebilecegiTemsilciler.Replace(",", "','") + "')";
		}
		((DbCommand)val).CommandText = text;
		try
		{
			SqliteCommand val2 = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val2).CommandText = text;
				val2.Parameters.AddWithValue("@bolge_kodu", (object)cari_bolge_kodu);
				SqliteDataReader val3 = val2.ExecuteReader();
				while (((DbDataReader)val3).Read())
				{
					CariPersonel cariPersonel = new CariPersonel();
					cariPersonel.cari_per_kod = val3.GetSafeString(0);
					cariPersonel.cari_per_adi = val3.GetSafeString(1);
					cariPersonel.cari_per_soyadi = val3.GetSafeString(2);
					cariPersonel.cari_per_tip = (enum_Cari_Personel_Tip)val3.GetSafeByte(3);
					cariPersonel.cari_per_doviz_cinsi = val3.GetSafeByte(4);
					cariPersonel.cari_per_kasiyerkodu = val3.GetSafeString(5);
					cariPersonel.cari_per_kasiyersifresi = val3.GetSafeString(6);
					cariPersonel.cari_per_kasiyerAmiri = val3.GetSafeString(7);
					cariPersonel.cari_per_userno = val3.GetSafeInt32(8);
					cariPersonel.cari_per_depono = val3.GetSafeInt32(9);
					cariPersonel.cari_per_cepno = val3.GetSafeString(10);
					cariPersonel.cari_per_mail = val3.GetSafeString(11);
					list.Add(cariPersonel);
				}
				((DbDataReader)val3).Close();
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}
}
