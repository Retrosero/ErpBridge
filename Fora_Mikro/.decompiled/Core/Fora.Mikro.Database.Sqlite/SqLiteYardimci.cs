using System.Runtime.InteropServices;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Database.Sqlite;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SqLiteYardimci
{
	public static SqliteConnection GetConnection(string DataSource, bool ReadOnly)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (ReadOnly)
		{
			return new SqliteConnection("Data Source=" + DataSource + ";Read Only = True");
		}
		return new SqliteConnection("Data Source=" + DataSource);
	}
}
