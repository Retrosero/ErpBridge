using System.ComponentModel;
using System.Data.Common;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Database.Sqlite;

public class SqliteDatabaseHelper
{
	private SqliteConnection _connection_readonly;

	private SqliteConnection _connection_writable;

	public string DBNameWithPath { get; set; }

	public SqliteDatabaseHelper()
	{
	}

	public SqliteDatabaseHelper(string DbName)
	{
		DBNameWithPath = DbName;
	}

	public SqliteConnection GetConnectionReadOnly()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		if (_connection_readonly == null)
		{
			_connection_readonly = SqLiteYardimci.GetConnection(DBNameWithPath, ReadOnly: true);
		}
		if ((int)((DbConnection)_connection_readonly).State != 1)
		{
			((DbConnection)_connection_readonly).Open();
		}
		return _connection_readonly;
	}

	public SqliteConnection GetConnectionWriteable()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		if (_connection_writable == null)
		{
			_connection_writable = SqLiteYardimci.GetConnection(DBNameWithPath, ReadOnly: false);
		}
		if ((int)((DbConnection)_connection_writable).State != 1)
		{
			((DbConnection)_connection_writable).Open();
		}
		return _connection_writable;
	}

	public void CloseConnection()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			try
			{
				if ((int)((DbConnection)_connection_readonly).State != 0)
				{
					((DbConnection)_connection_readonly).Close();
				}
			}
			catch
			{
			}
			((Component)_connection_readonly).Dispose();
			_connection_readonly = null;
		}
		catch
		{
		}
		try
		{
			try
			{
				if ((int)((DbConnection)_connection_writable).State != 0)
				{
					((DbConnection)_connection_writable).Close();
				}
			}
			catch
			{
			}
			((Component)_connection_writable).Dispose();
			_connection_writable = null;
		}
		catch
		{
		}
	}
}
