using System.IO;

namespace Fora.Mikro.Rapor.StokEnvanter;

public class RaporStokEnvanterSatir
{
	public int stok_sira_no { get; set; }

	public string stok_kod { get; set; }

	public int depo_sira_no { get; set; }

	public string depo_kod { get; set; }

	public double tutar { get; set; }

	public double miktar { get; set; }

	public double miktar_birim2 { get; set; }

	public double miktar_birim3 { get; set; }

	public double birim2_katsayi { get; set; }

	public double birim3_katsayi { get; set; }

	public RaporStokEnvanterSatir()
	{
	}

	public RaporStokEnvanterSatir(int _stok_sira_no, double _miktar, double _tutar, int _depo_sira_no, double _birim2_katsayi, double _birim3_katsayi)
	{
		stok_sira_no = _stok_sira_no;
		miktar = _miktar;
		tutar = _tutar;
		depo_sira_no = _depo_sira_no;
		birim2_katsayi = _birim2_katsayi;
		birim3_katsayi = _birim3_katsayi;
	}

	public RaporStokEnvanterSatir(string _stok_kod, double _miktar, double _tutar, string _depo_kod, double _birim2_katsayi, double _birim3_katsayi)
	{
		stok_kod = _stok_kod;
		miktar = _miktar;
		tutar = _tutar;
		depo_kod = _depo_kod;
		birim2_katsayi = _birim2_katsayi;
		birim3_katsayi = _birim3_katsayi;
	}

	public static byte[] WriteToByteArray(RaporStokEnvanterSatir toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokEnvanterSatir toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokEnvanterSatir toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.stok_sira_no);
		writer.Write(toWrite.depo_sira_no);
		writer.Write(toWrite.miktar);
		writer.Write(toWrite.tutar);
		writer.Write(toWrite.miktar_birim2);
		writer.Write(toWrite.miktar_birim3);
		_ = 2;
	}

	public static RaporStokEnvanterSatir ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokEnvanterSatir result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokEnvanterSatir ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporStokEnvanterSatir ReadFromBinaryReader(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		RaporStokEnvanterSatir result = new RaporStokEnvanterSatir
		{
			stok_sira_no = reader.ReadInt32(),
			depo_sira_no = reader.ReadInt32(),
			miktar = reader.ReadDouble(),
			tutar = reader.ReadDouble(),
			miktar_birim2 = reader.ReadDouble(),
			miktar_birim3 = reader.ReadDouble()
		};
		_ = 2;
		return result;
	}
}
