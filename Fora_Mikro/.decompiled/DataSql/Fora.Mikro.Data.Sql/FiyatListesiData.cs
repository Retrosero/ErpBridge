using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FiyatListesiData
{
	public static FiyatListesi GetFiyatListesi(SqlConnection connection, int sfl_sirano)
	{
		string cmdText = "SELECT sfl_sirano,sfl_aciklama,sfl_kdvdahil FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI WITH(NOLOCK) WHERE sfl_sirano=@sfl_sirano";
		FiyatListesi fiyatListesi = new FiyatListesi();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
			sqlCommand.Parameters.AddWithValue("@sfl_sirano", sfl_sirano);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				fiyatListesi.sfl_sirano = int.Parse(sqlDataReader[0].ToString());
				fiyatListesi.sfl_aciklama = sqlDataReader[1].ToString();
				fiyatListesi.sfl_kdvdahil = sqlDataReader[2].ToString() == "1";
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		catch
		{
		}
		return fiyatListesi;
	}

	public static List<GenelList> GetFiyatListesiGenelList(SqlConnection connection)
	{
		string cmdText = "SELECT sfl_sirano,sfl_aciklama FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI WITH(NOLOCK) ORDER BY sfl_sirano";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader[0].ToString();
				genelList.Text = sqlDataReader[1].ToString();
				list.Add(genelList);
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		catch
		{
		}
		return list;
	}

	public static DataTable GetFiyatListeleriDataTable(SqlConnection OpenedConnection)
	{
		int num = 15;
		if (OpenedConnection.Database.StartsWith("MikroDB_V12"))
		{
			num = 12;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V14"))
		{
			num = 14;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V15"))
		{
			num = 15;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V16"))
		{
			num = 16;
		}
		string text = "sfl_RECno AS 'RECno',";
		if (num > 15)
		{
			text = "sfl_Guid AS 'Guid',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "sfl_sirano AS 'SIRA NO',sfl_aciklama AS 'AÇIKLAMA' FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI WITH(NOLOCK) ORDER BY sfl_sirano", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "STOK_SATIS_FIYAT_LISTE_TANIMLARI";
		return dataTable;
	}
}
