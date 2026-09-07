using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Enumler;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Kasalar;

public static class KasaSqlite
{
	public static Kasa GetKasa(SqliteConnection OpenedConnection, string kas_kod)
	{
		string commandText = "SELECT kas_tip,kas_firma_no,kas_kod,kas_isim,kas_muh_kod,kas_doviz_cinsi FROM KASALAR WHERE kas_kod=@kas_kod";
		Kasa kasa = new Kasa();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@kas_kod", (object)kas_kod);
				SqliteDataReader val2 = val.ExecuteReader();
				if (((DbDataReader)val2).HasRows)
				{
					((DbDataReader)val2).Read();
					kasa.kas_tip = (enum_kas_tip)int.Parse(((DbDataReader)val2)[0].ToString());
					kasa.kas_firma_no = int.Parse(((DbDataReader)val2)[1].ToString());
					kasa.kas_kod = ((DbDataReader)val2)[2].ToString();
					kasa.kas_isim = ((DbDataReader)val2)[3].ToString();
					kasa.kas_muh_kod = ((DbDataReader)val2)[4].ToString();
					kasa.kas_doviz_cinsi = int.Parse(((DbDataReader)val2)[5].ToString());
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
		return kasa;
	}

	public static List<KasaListItem> GetKasaGenelList(SqliteConnection OpenedConnection, enum_kas_tip kasatipi, int FirmaNo, string listelenecek_kasa_kodlari)
	{
		string text = "";
		if (listelenecek_kasa_kodlari != "")
		{
			text = " AND kas_kod in ('" + listelenecek_kasa_kodlari.Replace(",", "','") + "') ";
		}
		string text2 = "";
		if (kasatipi != enum_kas_tip.Hepsi)
		{
			text2 = " kas_tip=@kas_tip AND";
		}
		string commandText = "SELECT kas_tip,kas_firma_no,kas_kod,kas_isim,kas_muh_kod,kas_doviz_cinsi FROM KASALAR WHERE" + text2 + " (kas_firma_no=@kas_firma_no OR @kas_firma_no=-1) " + text + " ORDER BY kas_isim";
		List<KasaListItem> list = new List<KasaListItem>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				if (kasatipi != enum_kas_tip.Hepsi)
				{
					val.Parameters.AddWithValue("@kas_tip", (object)(int)kasatipi);
				}
				val.Parameters.AddWithValue("@kas_firma_no", (object)FirmaNo);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					KasaListItem kasaListItem = new KasaListItem();
					kasaListItem.kas_tip = (enum_kas_tip)int.Parse(((DbDataReader)val2)[0].ToString());
					kasaListItem.kas_firma_no = int.Parse(((DbDataReader)val2)[1].ToString());
					kasaListItem.kas_kod = ((DbDataReader)val2)[2].ToString();
					kasaListItem.kas_isim = ((DbDataReader)val2)[3].ToString();
					kasaListItem.kas_muh_kod = ((DbDataReader)val2)[4].ToString();
					kasaListItem.kas_doviz_cinsi = int.Parse(((DbDataReader)val2)[5].ToString());
					list.Add(kasaListItem);
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
		return list;
	}

	public static double GetKasaBakiye(SqliteConnection connection, string cari_kodu)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		double result = 0.0;
		string text = "SELECT  IFNULL( SUM( CASE WHEN cha_tip = 0 AND((cha_cari_cins = @cari_cins) AND(cha_kod = @carikod) AND (cha_kod <> '')) THEN(cha_meblag * cha_d_kur) WHEN cha_tip = 1 AND((cha_cari_cins = @cari_cins) AND(cha_kod = @carikod) AND(cha_kod <> '')) THEN(-1 * (cha_meblag * cha_d_kur)) WHEN cha_tip = 0 AND((cha_kasa_hizmet = @cari_cins) AND(cha_kasa_hizkod = @carikod) AND(cha_kasa_hizkod <> '')) THEN(-1 * ((cha_meblag * cha_d_kur) / cha_karsid_kur)) WHEN cha_tip = 1 AND((cha_kasa_hizmet = @cari_cins) AND(cha_kasa_hizkod = @carikod) AND(cha_kasa_hizkod <> '')) THEN((cha_meblag * cha_d_kur) / cha_karsid_kur) ELSE 0 END ), 0) FROM CARI_HESAP_HAREKETLERI WHERE (cha_cari_cins = @cari_cins AND cha_kod = @carikod AND cha_kod <> '') OR (cha_kasa_hizmet = @cari_cins AND cha_kasa_hizkod = @carikod AND cha_kasa_hizkod <> '')";
		try
		{
			SqliteCommand val = new SqliteCommand(text, connection)
			{
				CommandText = text
			};
			val.Parameters.AddWithValue("@carikod", (object)cari_kodu);
			val.Parameters.AddWithValue("@cari_cins", (object)4);
			result = double.Parse(((DbCommand)val).ExecuteScalar().ToString());
			((Component)val).Dispose();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result;
	}
}
