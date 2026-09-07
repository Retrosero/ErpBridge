using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.StokSerino;

public static class StokSerinoSqlite
{
	public static STOK_SERINO_TANIMLARI GetStokSerino(SqliteConnection OpenedConnection, string seri_no)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "SELECT chz_serino,chz_stok_kodu,chz_grup_kodu,chz_Tuktckodu,chz_aciklama1,chz_aciklama2,chz_aciklama3,chz_al_evr_seri,chz_al_cari_kodu,chz_st_evr_seri,chz_st_cari_kodu,chz_parca_garantisi,chz_parca_serino,chz_brut_fiati,chz_al_fiati_ana,chz_al_fiati_alt,chz_al_fiati_orj,chz_st_fiati_ana,chz_st_fiati_alt,chz_st_fiati_orj,chz_al_evr_sira,chz_st_evr_sira,chz_GrnBasTarihi,chz_GrnBitTarihi,chz_al_tarih,chz_st_tarih,chz_parca_garanti_baslangic,chz_parca_garanti_bitis FROM STOK_SERINO_TANIMLARI WHERE chz_serino=@chz_serino";
		STOK_SERINO_TANIMLARI sTOK_SERINO_TANIMLARI = new STOK_SERINO_TANIMLARI();
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = OpenedConnection
			};
			val.Parameters.AddWithValue("@chz_serino", (object)seri_no);
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				sTOK_SERINO_TANIMLARI.chz_serino = val2.GetSafeString(0);
				sTOK_SERINO_TANIMLARI.chz_stok_kodu = val2.GetSafeString(1);
				sTOK_SERINO_TANIMLARI.chz_grup_kodu = val2.GetSafeString(2);
				sTOK_SERINO_TANIMLARI.chz_Tuktckodu = val2.GetSafeString(3);
				sTOK_SERINO_TANIMLARI.chz_aciklama1 = val2.GetSafeString(4);
				sTOK_SERINO_TANIMLARI.chz_aciklama2 = val2.GetSafeString(5);
				sTOK_SERINO_TANIMLARI.chz_aciklama3 = val2.GetSafeString(6);
				sTOK_SERINO_TANIMLARI.chz_al_evr_seri = val2.GetSafeString(7);
				sTOK_SERINO_TANIMLARI.chz_al_cari_kodu = val2.GetSafeString(8);
				sTOK_SERINO_TANIMLARI.chz_st_evr_seri = val2.GetSafeString(9);
				sTOK_SERINO_TANIMLARI.chz_st_cari_kodu = val2.GetSafeString(10);
				sTOK_SERINO_TANIMLARI.chz_parca_garantisi = val2.GetSafeBoolean(11);
				sTOK_SERINO_TANIMLARI.chz_parca_serino = val2.GetSafeString(12);
				sTOK_SERINO_TANIMLARI.chz_brut_fiati = val2.GetSafeDouble(13);
				sTOK_SERINO_TANIMLARI.chz_al_fiati_ana = val2.GetSafeDouble(14);
				sTOK_SERINO_TANIMLARI.chz_al_fiati_alt = val2.GetSafeDouble(15);
				sTOK_SERINO_TANIMLARI.chz_al_fiati_orj = val2.GetSafeDouble(16);
				sTOK_SERINO_TANIMLARI.chz_st_fiati_ana = val2.GetSafeDouble(17);
				sTOK_SERINO_TANIMLARI.chz_st_fiati_alt = val2.GetSafeDouble(18);
				sTOK_SERINO_TANIMLARI.chz_st_fiati_orj = val2.GetSafeDouble(19);
				sTOK_SERINO_TANIMLARI.chz_al_evr_sira = val2.GetSafeInt32(20);
				sTOK_SERINO_TANIMLARI.chz_st_evr_sira = val2.GetSafeInt32(21);
				sTOK_SERINO_TANIMLARI.chz_GrnBasTarihi = val2.GetSafeDateTime(22);
				sTOK_SERINO_TANIMLARI.chz_GrnBitTarihi = val2.GetSafeDateTime(23);
				sTOK_SERINO_TANIMLARI.chz_al_tarih = val2.GetSafeDateTime(24);
				sTOK_SERINO_TANIMLARI.chz_st_tarih = val2.GetSafeDateTime(25);
				sTOK_SERINO_TANIMLARI.chz_parca_garanti_baslangic = val2.GetSafeDateTime(26);
				sTOK_SERINO_TANIMLARI.chz_parca_garanti_bitis = val2.GetSafeDateTime(27);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
		}
		catch
		{
		}
		return sTOK_SERINO_TANIMLARI;
	}
}
