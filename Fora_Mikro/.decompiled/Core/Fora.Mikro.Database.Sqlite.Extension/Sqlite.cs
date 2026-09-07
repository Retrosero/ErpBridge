using System;
using System.Data.Common;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Database.Sqlite.Extension;

public static class Sqlite
{
	public static string GetSafeString(this SqliteDataReader reader, int colIndex)
	{
		if (!((DbDataReader)reader).IsDBNull(colIndex))
		{
			return ((DbDataReader)reader).GetString(colIndex);
		}
		return "";
	}

	public static double GetSafeDouble(this SqliteDataReader reader, int colIndex)
	{
		if (!((DbDataReader)reader).IsDBNull(colIndex))
		{
			return ((DbDataReader)reader).GetDouble(colIndex);
		}
		return 0.0;
	}

	public static bool GetSafeBoolean(this SqliteDataReader reader, int colIndex)
	{
		if (!((DbDataReader)reader).IsDBNull(colIndex))
		{
			return ((DbDataReader)reader).GetBoolean(colIndex);
		}
		return false;
	}

	public static decimal GetSafeDecimal(this SqliteDataReader reader, int colIndex)
	{
		if (!((DbDataReader)reader).IsDBNull(colIndex))
		{
			return ((DbDataReader)reader).GetDecimal(colIndex);
		}
		return 0m;
	}

	public static float GetSafeFloat(this SqliteDataReader reader, int colIndex)
	{
		if (!((DbDataReader)reader).IsDBNull(colIndex))
		{
			return ((DbDataReader)reader).GetFloat(colIndex);
		}
		return 0f;
	}

	public static int GetSafeByte(this SqliteDataReader reader, int colIndex)
	{
		if (!((DbDataReader)reader).IsDBNull(colIndex))
		{
			return ((DbDataReader)reader).GetByte(colIndex);
		}
		return 0;
	}

	public static int GetSafeInt32(this SqliteDataReader reader, int colIndex)
	{
		if (!((DbDataReader)reader).IsDBNull(colIndex))
		{
			return ((DbDataReader)reader).GetInt32(colIndex);
		}
		return 0;
	}

	public static DateTime GetSafeDateTime(this SqliteDataReader reader, int colIndex)
	{
		if (!((DbDataReader)reader).IsDBNull(colIndex))
		{
			return ((DbDataReader)reader).GetDateTime(colIndex);
		}
		return DateTime.MinValue;
	}

	public static int GetSafeInt16(this SqliteDataReader reader, int colIndex)
	{
		if (!((DbDataReader)reader).IsDBNull(colIndex))
		{
			return ((DbDataReader)reader).GetInt16(colIndex);
		}
		return 0;
	}
}
