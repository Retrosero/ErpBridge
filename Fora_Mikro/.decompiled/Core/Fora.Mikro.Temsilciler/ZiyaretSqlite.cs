using System;
using System.ComponentModel;
using System.Data.Common;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Temsilciler;

public static class ZiyaretSqlite
{
	public static bool ZiyaretBitir(SqliteConnection connection, Ziyaret ziyaret)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		try
		{
			SqliteCommand val = new SqliteCommand("INSERT INTO [OFFLINE_KAYITLAR] (AktarimDurumu,YeniKayit,AktarilmaTarihi,Tipi,Evrak,HataString) VALUES (@AktarimDurumu,@YeniKayit,@AktarilmaTarihi,@Tipi,@Evrak,@HataString)", connection);
			val.Parameters.AddWithValue("@AktarimDurumu", (object)2);
			val.Parameters.AddWithValue("@YeniKayit", (object)true);
			val.Parameters.AddWithValue("@AktarilmaTarihi", (object)DateTime.Now.AddYears(-10));
			val.Parameters.AddWithValue("@Tipi", (object)4);
			byte[] array = Ziyaret.WriteToByteArray(ziyaret, 2);
			val.Parameters.AddWithValue("@Evrak", (object)array);
			val.Parameters.AddWithValue("@HataString", (object)"");
			((DbCommand)val).ExecuteScalar();
			((Component)val).Dispose();
		}
		catch
		{
			result = false;
		}
		return result;
	}
}
