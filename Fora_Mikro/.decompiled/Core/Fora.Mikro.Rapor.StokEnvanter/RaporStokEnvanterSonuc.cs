using System.Collections.Generic;
using System.IO;
using Fora.Mikro.Enumler;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokEnvanter;

public class RaporStokEnvanterSonuc
{
	public enum_stok_envanter_gruplandirma_secenekleri gruplandirma_secenegi { get; set; }

	public enum_siralama_secenekleri siralama_secenegi { get; set; }

	public enum_stok_envanter_tutar_secenekleri tutar1_secenek { get; set; }

	public enum_stok_envanter_tutar_secenekleri tutar2_secenek { get; set; }

	public enum_stok_envanter_tutar_secenekleri tutar3_secenek { get; set; }

	public enum_stok_envanter_miktar_secenekleri miktar1_secenek { get; set; }

	public enum_stok_envanter_miktar_secenekleri miktar2_secenek { get; set; }

	public enum_stok_envanter_miktar_secenekleri miktar3_secenek { get; set; }

	public List<RaporStokEnvanterListItem> list_items { get; set; }

	public string baslik_tutar1 => tutar1_secenek switch
	{
		enum_stok_envanter_tutar_secenekleri.Gosterme => "", 
		enum_stok_envanter_tutar_secenekleri.Tutar => AppResource.genel_tutar, 
		_ => EnumUtility.EnumToLocalizedString(tutar1_secenek), 
	};

	public string baslik_tutar2 => tutar2_secenek switch
	{
		enum_stok_envanter_tutar_secenekleri.Gosterme => "", 
		enum_stok_envanter_tutar_secenekleri.Tutar => AppResource.genel_tutar, 
		_ => EnumUtility.EnumToLocalizedString(tutar1_secenek), 
	};

	public string baslik_tutar3 => tutar3_secenek switch
	{
		enum_stok_envanter_tutar_secenekleri.Gosterme => "", 
		enum_stok_envanter_tutar_secenekleri.Tutar => AppResource.genel_tutar, 
		_ => EnumUtility.EnumToLocalizedString(tutar1_secenek), 
	};

	public string baslik_miktar1 => miktar1_secenek switch
	{
		enum_stok_envanter_miktar_secenekleri.Gosterme => "", 
		enum_stok_envanter_miktar_secenekleri.MiktarBirim1 => AppResource.genel_miktar + " 1", 
		enum_stok_envanter_miktar_secenekleri.MiktarBirim2 => AppResource.genel_miktar + " 2", 
		enum_stok_envanter_miktar_secenekleri.MiktarBirim3 => AppResource.genel_miktar + " 3", 
		_ => EnumUtility.EnumToLocalizedString(miktar1_secenek), 
	};

	public string baslik_miktar2 => miktar2_secenek switch
	{
		enum_stok_envanter_miktar_secenekleri.Gosterme => "", 
		enum_stok_envanter_miktar_secenekleri.MiktarBirim1 => AppResource.genel_miktar + " 1", 
		enum_stok_envanter_miktar_secenekleri.MiktarBirim2 => AppResource.genel_miktar + " 2", 
		enum_stok_envanter_miktar_secenekleri.MiktarBirim3 => AppResource.genel_miktar + " 3", 
		_ => EnumUtility.EnumToLocalizedString(miktar1_secenek), 
	};

	public string baslik_miktar3 => miktar3_secenek switch
	{
		enum_stok_envanter_miktar_secenekleri.Gosterme => "", 
		enum_stok_envanter_miktar_secenekleri.MiktarBirim1 => AppResource.genel_miktar + " 1", 
		enum_stok_envanter_miktar_secenekleri.MiktarBirim2 => AppResource.genel_miktar + " 2", 
		enum_stok_envanter_miktar_secenekleri.MiktarBirim3 => AppResource.genel_miktar + " 3", 
		_ => EnumUtility.EnumToLocalizedString(miktar1_secenek), 
	};

	public int satir_sayisi => list_items.Count;

	public double Toplam_Miktar1
	{
		get
		{
			double num = 0.0;
			foreach (RaporStokEnvanterListItem list_item in list_items)
			{
				num += list_item.miktar1;
			}
			return num;
		}
	}

	public double Toplam_Miktar2
	{
		get
		{
			double num = 0.0;
			foreach (RaporStokEnvanterListItem list_item in list_items)
			{
				num += list_item.miktar2;
			}
			return num;
		}
	}

	public double Toplam_Miktar3
	{
		get
		{
			double num = 0.0;
			foreach (RaporStokEnvanterListItem list_item in list_items)
			{
				num += list_item.miktar3;
			}
			return num;
		}
	}

	public double Toplam_Tutar1
	{
		get
		{
			double num = 0.0;
			foreach (RaporStokEnvanterListItem list_item in list_items)
			{
				num += list_item.tutar1;
			}
			return num;
		}
	}

	public double Toplam_Tutar2
	{
		get
		{
			double num = 0.0;
			foreach (RaporStokEnvanterListItem list_item in list_items)
			{
				num += list_item.tutar2;
			}
			return num;
		}
	}

	public double Toplam_Tutar3
	{
		get
		{
			double num = 0.0;
			foreach (RaporStokEnvanterListItem list_item in list_items)
			{
				num += list_item.tutar3;
			}
			return num;
		}
	}

	public string Miktar1_Birimleri
	{
		get
		{
			string text = "";
			List<string> list = new List<string>();
			foreach (RaporStokEnvanterListItem list_item in list_items)
			{
				string text2 = "";
				text2 = ((miktar1_secenek == enum_stok_envanter_miktar_secenekleri.MiktarBirim2) ? list_item.birim2_adi : ((miktar1_secenek != enum_stok_envanter_miktar_secenekleri.MiktarBirim3) ? list_item.birim_adi : list_item.birim3_adi));
				if (!(text2 != ""))
				{
					continue;
				}
				bool flag = false;
				foreach (string item in list)
				{
					if (item == text2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(text2);
				}
			}
			string text3 = "";
			foreach (string item2 in list)
			{
				text = text + text3 + item2;
				text3 = ",";
			}
			return text;
		}
	}

	public string Miktar2_Birimleri
	{
		get
		{
			string text = "";
			List<string> list = new List<string>();
			foreach (RaporStokEnvanterListItem list_item in list_items)
			{
				string text2 = "";
				text2 = ((miktar2_secenek == enum_stok_envanter_miktar_secenekleri.MiktarBirim2) ? list_item.birim2_adi : ((miktar2_secenek != enum_stok_envanter_miktar_secenekleri.MiktarBirim3) ? list_item.birim_adi : list_item.birim3_adi));
				if (!(text2 != ""))
				{
					continue;
				}
				bool flag = false;
				foreach (string item in list)
				{
					if (item == text2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(text2);
				}
			}
			string text3 = "";
			foreach (string item2 in list)
			{
				text = text + text3 + item2;
				text3 = ",";
			}
			return text;
		}
	}

	public string Miktar3_Birimleri
	{
		get
		{
			string text = "";
			List<string> list = new List<string>();
			foreach (RaporStokEnvanterListItem list_item in list_items)
			{
				string text2 = "";
				text2 = ((miktar3_secenek == enum_stok_envanter_miktar_secenekleri.MiktarBirim2) ? list_item.birim2_adi : ((miktar3_secenek != enum_stok_envanter_miktar_secenekleri.MiktarBirim3) ? list_item.birim_adi : list_item.birim3_adi));
				if (!(text2 != ""))
				{
					continue;
				}
				bool flag = false;
				foreach (string item in list)
				{
					if (item == text2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(text2);
				}
			}
			string text3 = "";
			foreach (string item2 in list)
			{
				text = text + text3 + item2;
				text3 = ",";
			}
			return text;
		}
	}

	public RaporStokEnvanterSonuc()
	{
		list_items = new List<RaporStokEnvanterListItem>();
	}

	public static byte[] WriteToByteArray(RaporStokEnvanterSonuc toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokEnvanterSonuc toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokEnvanterSonuc toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write((int)toWrite.gruplandirma_secenegi);
		writer.Write((int)toWrite.siralama_secenegi);
		writer.Write((int)toWrite.tutar1_secenek);
		writer.Write((int)toWrite.tutar2_secenek);
		writer.Write((int)toWrite.tutar3_secenek);
		writer.Write((int)toWrite.miktar1_secenek);
		writer.Write((int)toWrite.miktar2_secenek);
		writer.Write((int)toWrite.miktar3_secenek);
		writer.Write(toWrite.list_items.Count);
		foreach (RaporStokEnvanterListItem list_item in toWrite.list_items)
		{
			RaporStokEnvanterListItem.WriteToBinaryWriter(list_item, writer, 1);
		}
		_ = 2;
	}

	public static RaporStokEnvanterSonuc ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokEnvanterSonuc result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokEnvanterSonuc ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporStokEnvanterSonuc ReadFromBinaryReader(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		RaporStokEnvanterSonuc raporStokEnvanterSonuc = new RaporStokEnvanterSonuc();
		raporStokEnvanterSonuc.gruplandirma_secenegi = (enum_stok_envanter_gruplandirma_secenekleri)reader.ReadInt32();
		raporStokEnvanterSonuc.siralama_secenegi = (enum_siralama_secenekleri)reader.ReadInt32();
		raporStokEnvanterSonuc.tutar1_secenek = (enum_stok_envanter_tutar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSonuc.tutar2_secenek = (enum_stok_envanter_tutar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSonuc.tutar3_secenek = (enum_stok_envanter_tutar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSonuc.miktar1_secenek = (enum_stok_envanter_miktar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSonuc.miktar2_secenek = (enum_stok_envanter_miktar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSonuc.miktar3_secenek = (enum_stok_envanter_miktar_secenekleri)reader.ReadInt32();
		int num2 = reader.ReadInt32();
		raporStokEnvanterSonuc.list_items = new List<RaporStokEnvanterListItem>();
		for (int i = 0; i < num2; i++)
		{
			raporStokEnvanterSonuc.list_items.Add(RaporStokEnvanterListItem.ReadFromStream(reader));
		}
		_ = 2;
		return raporStokEnvanterSonuc;
	}
}
