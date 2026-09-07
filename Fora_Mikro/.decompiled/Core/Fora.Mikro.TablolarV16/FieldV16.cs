using System.Data;
using Fora.Mikro.Enumler;

namespace Fora.Mikro.TablolarV16;

public class FieldV16
{
	public string Adi { get; set; }

	public SqlDbType SqlType { get; set; }

	public enum_SqliteDataType SqliteType { get; set; }

	public FieldV16(string FieldAdi, SqlDbType FieldSqlDbType, enum_SqliteDataType FieldSqliteDataType)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Adi = FieldAdi;
		SqlType = FieldSqlDbType;
		SqliteType = FieldSqliteDataType;
	}
}
