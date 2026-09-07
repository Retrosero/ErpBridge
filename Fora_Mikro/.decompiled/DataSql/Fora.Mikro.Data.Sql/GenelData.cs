using System.Data;
using System.Runtime.InteropServices;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GenelData
{
	public static DataSet GetLookupTablolar(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		DataSet dataSet = new DataSet();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		dataSet.Tables.Add(CariData.GetCarilerDataTable(sqlDB.Connection));
		dataSet.Tables.Add(StokData.GetStoklarDataTable(sqlDB.Connection));
		dataSet.Tables.Add(HizmetData.GetHizmetlerDataTable(sqlDB.Connection));
		dataSet.Tables.Add(KasaData.GetKasalarDataTable(sqlDB.Connection));
		dataSet.Tables.Add(BankaData.GetBankalarDataTable(sqlDB.Connection));
		dataSet.Tables.Add(CariPersonelData.GetCariPersonellerDataTable(sqlDB.Connection));
		dataSet.Tables.Add(OdemePlaniData.GetOdemePlanlariDataTable(sqlDB.Connection));
		dataSet.Tables.Add(DepoData.GetDepolarDataTable(sqlDB.Connection));
		dataSet.Tables.Add(ProjeData.GetProjelerDataTable(sqlDB.Connection));
		dataSet.Tables.Add(SorumlulukMerkeziData.GetSorumlulukMerkezleriDataTable(sqlDB.Connection));
		dataSet.Tables.Add(FiyatListesiData.GetFiyatListeleriDataTable(sqlDB.Connection));
		dataSet.Tables.Add(DemirbasData.GetDemirbasDataTable(sqlDB.Connection));
		dataSet.Tables.Add(PersonelData.GetPersonelDataTable(sqlDB.Connection));
		dataSet.Tables.Add(MasrafData.GetMasrafDataTable(sqlDB.Connection));
		sqlDB.ConnectionClose();
		return dataSet;
	}
}
