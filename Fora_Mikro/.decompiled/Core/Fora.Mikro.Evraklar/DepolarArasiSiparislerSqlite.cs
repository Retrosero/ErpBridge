using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Siparis;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Evraklar;

public static class DepolarArasiSiparislerSqlite
{
	public static List<SiparisEvrakListItem> GetDepolarArasiSiparisEvrakList(SqliteConnection OpenedConnection, string SiparisSeri, int kaynak_depo_no, int hedef_depo_no, enum_sip_orderby Siralama, siparis_teslim_durumu teslim_durumu)
	{
		List<SiparisEvrakListItem> list = new List<SiparisEvrakListItem>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string text = "SELECT ssip_evrakno_seri,ssip_evrakno_sira,ssip_tarih,MIN(ssip_teslim_tarih) AS ssip_teslim_tarih,SUM(ssip_miktar) AS ssip_miktar,SUM(ssip_teslim_miktar) AS ssip_teslim_miktar FROM DEPOLAR_ARASI_SIPARISLER ";
				string text2 = "WHERE";
				if (SiparisSeri != "")
				{
					text = text + text2 + " ssip_evrakno_seri=@ssip_evrakno_seri ";
					val.Parameters.AddWithValue("@ssip_evrakno_seri", (object)SiparisSeri);
					text2 = "AND";
				}
				if (hedef_depo_no != 0)
				{
					text = text + text2 + " ssip_girdepo=@ssip_girdepo ";
					val.Parameters.AddWithValue("@ssip_girdepo", (object)hedef_depo_no);
					text2 = "AND";
				}
				if (kaynak_depo_no != 0)
				{
					text = text + text2 + " ssip_cikdepo=@ssip_cikdepo ";
					val.Parameters.AddWithValue("@ssip_cikdepo", (object)kaynak_depo_no);
				}
				switch (teslim_durumu)
				{
				case siparis_teslim_durumu.Bekleyenler:
					text = text + text2 + " (ssip_miktar>ssip_teslim_miktar AND ssip_kapat_fl=0)";
					text2 = "AND";
					break;
				case siparis_teslim_durumu.TeslimEdilenler:
					text = text + text2 + " ssip_miktar=ssip_teslim_miktar";
					text2 = "AND";
					break;
				case siparis_teslim_durumu.Vazgecilenler:
					text = text + text2 + " ssip_kapat_fl=1";
					text2 = "AND";
					break;
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
				text += " LIMIT 100";
				((DbCommand)val).CommandText = text;
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					SiparisEvrakListItem item = new SiparisEvrakListItem
					{
						sip_evrakno_seri = val2.GetSafeString(0),
						sip_evrakno_sira = val2.GetSafeInt32(1),
						sip_tarih = val2.GetSafeDateTime(2),
						sip_teslim_tarih = val2.GetSafeDateTime(3),
						sip_miktar = val2.GetSafeDouble(4),
						sip_teslim_miktar = val2.GetSafeDouble(5),
						sip_tip = enum_sip_tip.Talep,
						sip_cins = enum_sip_cins.DepolarArasiSiparis,
						sip_musteri_kod = ""
					};
					list.Add(item);
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
}
