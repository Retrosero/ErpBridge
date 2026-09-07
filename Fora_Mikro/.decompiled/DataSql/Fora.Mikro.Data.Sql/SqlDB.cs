using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

public class SqlDB : IDisposable
{
	public static readonly SqlDB Instance = new SqlDB();

	public SqlConnection Connection = new SqlConnection();

	public SqlTransaction Transaction;

	public string HataMesaji = string.Empty;

	public void Dispose()
	{
		if (Connection != null)
		{
			Connection.Dispose();
			Connection = null;
		}
		if (Transaction != null)
		{
			Transaction.Dispose();
			Transaction = null;
		}
	}

	public void ConnectionOpen(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		if (BaglantiBilgileri.SqlUserName == string.Empty)
		{
			Connection.ConnectionString = $"Data Source={BaglantiBilgileri.SqlServer}; Initial Catalog={DBName}; Trusted_Connection=true;MultipleActiveResultSets=True;";
		}
		else
		{
			Connection.ConnectionString = $"Data Source={BaglantiBilgileri.SqlServer}; Initial Catalog={DBName}; User Id={BaglantiBilgileri.SqlUserName};Password={BaglantiBilgileri.SqlPassword};MultipleActiveResultSets=True;";
		}
		Connection.Open();
	}

	public bool ConnectionClose()
	{
		try
		{
			Connection.Close();
			return true;
		}
		catch (SqlException ex)
		{
			HataMesaji = ex.Message;
			return false;
		}
	}

	public bool TransactionBegin()
	{
		try
		{
			Transaction = Connection.BeginTransaction();
			return true;
		}
		catch (SqlException ex)
		{
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			Transaction.Rollback();
			HataMesaji = ex.Message;
			return false;
		}
	}

	public bool TransactionCommit()
	{
		try
		{
			Transaction.Commit();
			return true;
		}
		catch (SqlException ex)
		{
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			Transaction.Rollback();
			HataMesaji = ex.Message;
			return false;
		}
	}

	public object ExecuteScaller(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string QueryString)
	{
		SqlConnection sqlConnection = new SqlConnection();
		if (BaglantiBilgileri.SqlUserName == string.Empty)
		{
			sqlConnection.ConnectionString = $"Data Source={BaglantiBilgileri.SqlServer}; Initial Catalog={DBName}; Trusted_Connection=true;";
		}
		else
		{
			sqlConnection.ConnectionString = $"Data Source={BaglantiBilgileri.SqlServer}; Initial Catalog={DBName}; User Id={BaglantiBilgileri.SqlUserName};Password={BaglantiBilgileri.SqlPassword};";
		}
		sqlConnection.Open();
		SqlCommand sqlCommand = sqlConnection.CreateCommand();
		sqlCommand.CommandText = QueryString;
		object result = sqlCommand.ExecuteScalar();
		sqlConnection.Close();
		return result;
	}

	public SqlDataReader ExecuteReader(string QueryString)
	{
		return new SqlCommand(QueryString, Connection).ExecuteReader();
	}

	public SqlDataReader ExecuteReader(SqlCommand command)
	{
		command.Connection = Connection;
		return command.ExecuteReader();
	}

	public bool ExecuteNonQueryWithTransaction(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, List<SqlCommand> CmdList)
	{
		SqlConnection sqlConnection = new SqlConnection();
		if (BaglantiBilgileri.SqlUserName == string.Empty)
		{
			sqlConnection.ConnectionString = $"Data Source={BaglantiBilgileri.SqlServer}; Initial Catalog={DBName}; Trusted_Connection=true;";
		}
		else
		{
			sqlConnection.ConnectionString = $"Data Source={BaglantiBilgileri.SqlServer}; Initial Catalog={DBName}; User Id={BaglantiBilgileri.SqlUserName};Password={BaglantiBilgileri.SqlPassword};";
		}
		sqlConnection.Open();
		SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
		try
		{
			foreach (SqlCommand Cmd in CmdList)
			{
				Cmd.Connection = sqlConnection;
				Cmd.Transaction = sqlTransaction;
				Cmd.ExecuteNonQuery();
			}
			sqlTransaction.Commit();
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			sqlTransaction.Rollback();
			return false;
		}
		finally
		{
			foreach (SqlCommand Cmd2 in CmdList)
			{
				Cmd2.Dispose();
			}
			sqlConnection.Close();
		}
	}

	public bool ExecuteNonQueryWithTransaction(SqlConnection OpenedConnection, List<SqlCommand> CmdList)
	{
		SqlTransaction sqlTransaction = OpenedConnection.BeginTransaction();
		try
		{
			foreach (SqlCommand Cmd in CmdList)
			{
				Cmd.Connection = OpenedConnection;
				Cmd.ExecuteNonQuery();
			}
			sqlTransaction.Commit();
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			sqlTransaction.Rollback();
			return false;
		}
		finally
		{
			foreach (SqlCommand Cmd2 in CmdList)
			{
				Cmd2.Dispose();
			}
		}
	}
}
