using System.ComponentModel;
using System.Data.Common;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Database.Sqlite;

public class SqliteHelper
{
	[SqliteFunction(/*Could not decode attribute arguments.*/)]
	public class TOUPPER : SqliteFunction
	{
		public override object Invoke(object[] args)
		{
			return args[0].ToString().ToUpper();
		}
	}

	[SqliteFunction(/*Could not decode attribute arguments.*/)]
	private class CollationCaseInsensitive : SqliteFunction
	{
		public override int Compare(string param1, string param2)
		{
			return string.Compare(param1, param2, ignoreCase: true);
		}
	}

	private SqliteConnection connection;

	public string _DbNameWithPath;

	private bool _ReadOnly;

	public SqliteHelper()
	{
	}

	public SqliteHelper(string DbNameWithPath, bool ReadOnly)
	{
		_DbNameWithPath = DbNameWithPath;
		_ReadOnly = ReadOnly;
	}

	public void Dispose()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		try
		{
			if (connection != null)
			{
				if ((int)((DbConnection)connection).State == 1)
				{
					((DbConnection)connection).Close();
				}
				((Component)connection).Dispose();
				connection = null;
			}
		}
		catch
		{
		}
	}

	public SqliteConnection GetConnection()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		if (connection == null)
		{
			Open(_DbNameWithPath, _ReadOnly);
		}
		if ((int)((DbConnection)connection).State != 1)
		{
			Open(_DbNameWithPath, _ReadOnly);
		}
		return connection;
	}

	public void Close()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		try
		{
			if (connection != null && (int)((DbConnection)connection).State == 1)
			{
				((DbConnection)connection).Close();
			}
		}
		catch
		{
		}
	}

	public void Open(string DbNameWithPath, bool ReadOnly)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		_DbNameWithPath = DbNameWithPath;
		_ReadOnly = ReadOnly;
		SqliteFunction.RegisterFunction(typeof(CollationCaseInsensitive));
		if (ReadOnly)
		{
			connection = new SqliteConnection("Data Source=" + DbNameWithPath + ";Read Only = True");
		}
		else
		{
			connection = new SqliteConnection("Data Source=" + DbNameWithPath);
		}
		((DbConnection)connection).Open();
	}
}
