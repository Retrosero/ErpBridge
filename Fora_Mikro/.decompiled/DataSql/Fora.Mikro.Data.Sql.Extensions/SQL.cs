using System;
using System.Data.SqlClient;

namespace Fora.Mikro.Data.Sql.Extensions;

public static class SQL
{
	public static string GetSafeString(this SqlDataReader reader, int colIndex)
	{
		if (!reader.IsDBNull(colIndex))
		{
			return reader.GetString(colIndex);
		}
		return "";
	}

	public static double GetSafeDouble(this SqlDataReader reader, int colIndex)
	{
		if (!reader.IsDBNull(colIndex))
		{
			return reader.GetDouble(colIndex);
		}
		return 0.0;
	}

	public static bool GetSafeBoolean(this SqlDataReader reader, int colIndex)
	{
		if (!reader.IsDBNull(colIndex))
		{
			return reader.GetBoolean(colIndex);
		}
		return false;
	}

	public static decimal GetSafeDecimal(this SqlDataReader reader, int colIndex)
	{
		if (!reader.IsDBNull(colIndex))
		{
			return reader.GetDecimal(colIndex);
		}
		return 0m;
	}

	public static float GetSafeFloat(this SqlDataReader reader, int colIndex)
	{
		if (!reader.IsDBNull(colIndex))
		{
			return reader.GetFloat(colIndex);
		}
		return 0f;
	}

	public static int GetSafeByte(this SqlDataReader reader, int colIndex)
	{
		if (!reader.IsDBNull(colIndex))
		{
			return reader.GetByte(colIndex);
		}
		return 0;
	}

	public static int GetSafeInt32(this SqlDataReader reader, int colIndex)
	{
		if (!reader.IsDBNull(colIndex))
		{
			return reader.GetInt32(colIndex);
		}
		return 0;
	}

	public static long GetSafeInt64(this SqlDataReader reader, int colIndex)
	{
		if (!reader.IsDBNull(colIndex))
		{
			return reader.GetInt64(colIndex);
		}
		return 0L;
	}

	public static DateTime GetSafeDateTime(this SqlDataReader reader, int colIndex)
	{
		if (!reader.IsDBNull(colIndex))
		{
			return reader.GetDateTime(colIndex);
		}
		return DateTime.MinValue;
	}

	public static int GetSafeInt16(this SqlDataReader reader, int colIndex)
	{
		if (!reader.IsDBNull(colIndex))
		{
			return reader.GetInt16(colIndex);
		}
		return 0;
	}
}
