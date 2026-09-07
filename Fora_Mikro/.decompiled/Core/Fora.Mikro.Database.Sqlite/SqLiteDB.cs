using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Database.Sqlite;

public static class SqLiteDB
{
	public static int ExecuteNonQuery(string DBFile, string QueryString)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		try
		{
			SqliteConnection val = new SqliteConnection("Data Source=" + DBFile);
			((DbConnection)val).Open();
			SqliteCommand obj = val.CreateCommand();
			((DbCommand)obj).CommandText = QueryString;
			int result = ((DbCommand)obj).ExecuteNonQuery();
			((DbConnection)val).Close();
			return result;
		}
		catch
		{
			return 0;
		}
	}

	public static int ExecuteNonQuery(string DBFile, SqliteCommand Cmd)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		SqliteConnection val = new SqliteConnection("Data Source=" + DBFile);
		try
		{
			((DbConnection)val).Open();
			Cmd.Connection = val;
			int result = ((DbCommand)Cmd).ExecuteNonQuery();
			((DbConnection)val).Close();
			return result;
		}
		catch
		{
			return 0;
		}
		finally
		{
			((Component)Cmd).Dispose();
			((DbConnection)val).Close();
		}
	}

	public static object ExecuteScaller(string DBFile, string QueryString)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		SqliteConnection val = new SqliteConnection("Data Source=" + DBFile);
		((DbConnection)val).Open();
		SqliteCommand obj = val.CreateCommand();
		((DbCommand)obj).CommandText = QueryString;
		object result = ((DbCommand)obj).ExecuteScalar();
		((DbConnection)val).Close();
		return result;
	}

	public static bool ExecuteNonQueryWithTransaction(object OpenedConnection, object CmdList, ref string HataMesaji)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		SqliteTransaction val = ((SqliteConnection)OpenedConnection).BeginTransaction();
		try
		{
			foreach (SqliteCommand item in (List<SqliteCommand>)CmdList)
			{
				item.Connection = (SqliteConnection)OpenedConnection;
				((DbCommand)item).ExecuteNonQuery();
			}
			((DbTransaction)val).Commit();
			return true;
		}
		catch (Exception ex)
		{
			HataMesaji = ex.ToString();
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			((DbTransaction)val).Rollback();
			return false;
		}
		finally
		{
			foreach (SqliteCommand item2 in (List<SqliteCommand>)CmdList)
			{
				((Component)item2).Dispose();
			}
		}
	}
}
