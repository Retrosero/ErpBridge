using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Bankalar;

public static class BankaSqlite
{
	public static Banka GetBanka(SqliteConnection OpenedConnection, string ban_kod)
	{
		string sqlString = "SELECT ban_kod,ban_ismi,ban_sube,ban_firma_no,ban_doviz_cinsi FROM BANKALAR WHERE ban_kod=@ban_kod";
		return GetBanka(OpenedConnection, ban_kod, sqlString);
	}

	public static Banka GetBanka(SqliteConnection OpenedConnection, string ban_kod, string SqlString)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Banka banka = new Banka();
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = SqlString,
				Connection = OpenedConnection
			};
			val.Parameters.AddWithValue("@ban_kod", (object)ban_kod);
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				banka.ban_kod = val2.GetSafeString(0);
				banka.ban_ismi = val2.GetSafeString(1);
				banka.ban_sube = val2.GetSafeString(2);
				banka.ban_firma_no = val2.GetSafeInt32(3);
				banka.ban_doviz_cinsi = val2.GetSafeByte(4);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
		}
		catch
		{
		}
		return banka;
	}

	public static List<BankaListItem> GetBankaGenelList(SqliteConnection OpenedConnection, int FirmaNo, int dovizcinsi, string listelenecek_banka_kodlari)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		string text = "";
		if (listelenecek_banka_kodlari != "")
		{
			text = " AND ban_kod in ('" + listelenecek_banka_kodlari.Replace(",", "','") + "') ";
		}
		string commandText = "SELECT ban_kod,ban_ismi,ban_sube,ban_firma_no,ban_doviz_cinsi FROM BANKALAR WHERE (ban_firma_no=@ban_firma_no OR @ban_firma_no=-1) AND (ban_doviz_cinsi=@ban_doviz_cinsi OR @ban_doviz_cinsi=-1) " + text + " ORDER BY ban_ismi COLLATE COLLATION_CASE_INSENSITIVE";
		List<BankaListItem> list = new List<BankaListItem>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@ban_firma_no", (object)FirmaNo);
			val.Parameters.AddWithValue("@ban_doviz_cinsi", (object)dovizcinsi);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				BankaListItem bankaListItem = new BankaListItem();
				bankaListItem.ban_kod = val2.GetSafeString(0);
				bankaListItem.ban_ismi = val2.GetSafeString(1);
				bankaListItem.ban_sube = val2.GetSafeString(2);
				bankaListItem.ban_firma_no = val2.GetSafeInt32(3);
				bankaListItem.ban_doviz_cinsi = val2.GetSafeByte(4);
				list.Add(bankaListItem);
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

	public static double GetBankaBakiye(SqliteConnection connection, string cari_kodu)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		double result = 0.0;
		string commandText = "SELECT  IFNULL( SUM( CASE WHEN cha_tip = 0 AND((cha_cari_cins = @cari_cins) AND((@carikod = '') OR(cha_kod = @carikod)) AND(cha_kod <> '')) THEN(cha_meblag * cha_d_kur) WHEN cha_tip = 1 AND((cha_cari_cins = @cari_cins) AND((@carikod = '') OR(cha_kod = @carikod)) AND(cha_kod <> '')) THEN(-1 * (cha_meblag * cha_d_kur)) WHEN cha_tip = 0 AND((cha_kasa_hizmet = @cari_cins) AND((@carikod = '') OR(cha_kasa_hizkod = @carikod)) AND(cha_kasa_hizkod <> '')) THEN(-1 * ((cha_meblag * cha_d_kur) / cha_karsid_kur)) WHEN cha_tip = 1 AND((cha_kasa_hizmet = @cari_cins) AND((@carikod = '') OR(cha_kasa_hizkod = @carikod)) AND(cha_kasa_hizkod <> '')) THEN((cha_meblag * cha_d_kur) / cha_karsid_kur) ELSE 0 END ), 0) FROM CARI_HESAP_HAREKETLERI WHERE ((cha_cari_cins = @cari_cins) AND((@carikod = '') OR(cha_kod = @carikod)) AND(cha_kod <> '')) OR ((cha_kasa_hizmet = @cari_cins) AND((@carikod = '') OR(cha_kasa_hizkod = @carikod)) AND(cha_kasa_hizkod <> ''))";
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = connection
			};
			val.Parameters.AddWithValue("@carikod", (object)cari_kodu);
			val.Parameters.AddWithValue("@cari_cins", (object)2);
			result = double.Parse(((DbCommand)val).ExecuteScalar().ToString());
			((Component)val).Dispose();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result;
	}

	public static List<GenelList> GetYerelBankaGenelList(SqliteConnection OpenedConnection)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		List<GenelList> list = new List<GenelList>();
		string commandText = "SELECT bankkod_kod,bankkod_bankadi FROM YEREL_BANKA_KODLARI GROUP BY bankkod_kod,bankkod_bankadi ORDER BY bankkod_bankadi";
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = ((DbDataReader)val2).GetString(0);
				genelList.Text = ((DbDataReader)val2).GetString(1);
				list.Add(genelList);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return list;
	}

	public static List<GenelList> GetYerelBankaSehirGenelList(SqliteConnection OpenedConnection, string bankkod_kod)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		List<GenelList> list = new List<GenelList>();
		string commandText = "SELECT bankkod_ilkodu FROM YEREL_BANKA_KODLARI WHERE bankkod_kod=@bankkod_kod GROUP BY bankkod_ilkodu ORDER BY bankkod_ilkodu";
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@bankkod_kod", (object)bankkod_kod);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = ((DbDataReader)val2).GetString(0);
				genelList.Text = GetBankaSehirIsmi(genelList.Kod);
				list.Add(genelList);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return list;
	}

	public static List<GenelList> GetYerelBankaSubeGenelList(SqliteConnection OpenedConnection, string bankkod_kod, string bankkod_ilkodu)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		List<GenelList> list = new List<GenelList>();
		string commandText = "SELECT bankkod_subekodu,bankkod_subeadi FROM YEREL_BANKA_KODLARI WHERE bankkod_kod=@bankkod_kod AND bankkod_ilkodu=@bankkod_ilkodu ORDER BY bankkod_subeadi";
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@bankkod_kod", (object)bankkod_kod);
			val.Parameters.AddWithValue("@bankkod_ilkodu", (object)bankkod_ilkodu);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = ((DbDataReader)val2).GetString(0);
				genelList.Text = ((DbDataReader)val2).GetString(1);
				list.Add(genelList);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return list;
	}

	public static string GetBankaSehirIsmi(string bankkod_ilkodu)
	{
		return bankkod_ilkodu switch
		{
			"001" => "ADANA", 
			"002" => "ADIYAMAN", 
			"003" => "AFYON", 
			"004" => "AĞRI", 
			"005" => "AMASYA", 
			"006" => "ANKARA", 
			"007" => "ANTALYA", 
			"008" => "ARTVİN", 
			"009" => "AYDIN", 
			"010" => "BALIKESİR", 
			"011" => "BİLECİK", 
			"012" => "BİNGÖL", 
			"013" => "BİTLİS", 
			"014" => "BOLU", 
			"015" => "BURDUR", 
			"016" => "BURSA", 
			"017" => "ÇANAKKALE", 
			"018" => "ÇANKIRI", 
			"019" => "ÇORUM", 
			"020" => "DENİZLİ", 
			"021" => "DİYARBAKIR", 
			"022" => "EDİRNE", 
			"023" => "ELAZIĞ", 
			"024" => "ERZİNCAN", 
			"025" => "ERZURUM", 
			"026" => "ESKİŞEHİR", 
			"027" => "GAZİANTEP", 
			"028" => "GİRESUN", 
			"029" => "GÜMÜŞHANE", 
			"030" => "HAKKARİ", 
			"031" => "HATAY", 
			"032" => "ISPARTA", 
			"033" => "MERSİN", 
			"034" => "İSTANBUL", 
			"035" => "İZMİR", 
			"036" => "KARS", 
			"037" => "KASTAMONU", 
			"038" => "KAYSERİ", 
			"039" => "KIRKLARELİ", 
			"040" => "KIRŞEHİR", 
			"041" => "KOCAELİ", 
			"042" => "KONYA", 
			"043" => "KÜTAHYA", 
			"044" => "MALATYA", 
			"045" => "MANİSA", 
			"046" => "K.MARAŞ", 
			"047" => "MARDİN", 
			"048" => "MUĞLA", 
			"049" => "MUŞ", 
			"050" => "NEVŞEHİR", 
			"051" => "NİĞDE", 
			"052" => "ORDU", 
			"053" => "RİZE", 
			"054" => "SAKARYA", 
			"055" => "SAMSUN", 
			"056" => "SİİRT", 
			"057" => "SİNOP", 
			"058" => "SİVAS", 
			"059" => "TEKİRDAĞ", 
			"060" => "TOKAT", 
			"061" => "TRABZON", 
			"062" => "TUNCELİ", 
			"063" => "Ş.URFA", 
			"064" => "UŞAK", 
			"065" => "VAN", 
			"066" => "YOZGAT", 
			"067" => "ZONGULDAK", 
			"068" => "AKSARAY", 
			"069" => "BAYBURT", 
			"070" => "KARAMAN", 
			"071" => "KIRIKKALE", 
			"072" => "BATMAN", 
			"073" => "ŞIRNAK", 
			"074" => "BARTIN", 
			"075" => "ARDAHAN", 
			"076" => "IĞDIR", 
			"077" => "YALOVA", 
			"078" => "KARABÜK", 
			"079" => "KİLİS", 
			"080" => "OSMANİYE", 
			"081" => "DÜZCE", 
			"090" => "KIBRIS", 
			"999" => "TANIMSIZ", 
			_ => "TANIMSIZ", 
		};
	}
}
