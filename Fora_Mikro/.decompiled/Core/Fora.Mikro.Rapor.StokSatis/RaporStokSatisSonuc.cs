using System.Collections.Generic;
using System.IO;
using Fora.Mikro.Enumler;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokSatis;

public class RaporStokSatisSonuc
{
	private string BaslikTutarBrutTutar;

	private string BaslikTutarIskontoTutari;

	private string BaslikTutarNetTutar;

	private string BaslikTutarNetTutarKdvDahil;

	private string BaslikTutarNetKarTutari;

	private string BaslikTutarNetKarYuzdesi;

	private string BaslikTutarNetMaliyet;

	private string BaslikMiktarBrutSatisMiktari;

	private string BaslikMiktarIadeMiktari;

	private string BaslikMiktarNetSatisMiktari;

	private string BaslikMiktarNetSatisMiktariBirim2;

	private string BaslikMiktarNetSatisMiktariBirim3;

	private string BaslikMiktarCariAdresSayisi;

	public string baslik_tutar1 => tutar1_secenek switch
	{
		enum_tutar_secenekleri.BrutTutar => BaslikTutarBrutTutar, 
		enum_tutar_secenekleri.Gosterme => "", 
		enum_tutar_secenekleri.IskontoTutari => BaslikTutarIskontoTutari, 
		enum_tutar_secenekleri.NetTutar => BaslikTutarNetTutar, 
		enum_tutar_secenekleri.NetTutarKdvDahil => BaslikTutarNetTutarKdvDahil, 
		enum_tutar_secenekleri.NetMaliyet => BaslikTutarNetMaliyet, 
		enum_tutar_secenekleri.NetKarTutari => BaslikTutarNetKarTutari, 
		enum_tutar_secenekleri.NetKarYuzdesi => BaslikTutarNetKarYuzdesi, 
		_ => EnumUtility.EnumToLocalizedString(tutar1_secenek), 
	};

	public string baslik_tutar2 => tutar2_secenek switch
	{
		enum_tutar_secenekleri.BrutTutar => BaslikTutarBrutTutar, 
		enum_tutar_secenekleri.Gosterme => "", 
		enum_tutar_secenekleri.IskontoTutari => BaslikTutarIskontoTutari, 
		enum_tutar_secenekleri.NetTutar => BaslikTutarNetTutar, 
		enum_tutar_secenekleri.NetTutarKdvDahil => BaslikTutarNetTutarKdvDahil, 
		enum_tutar_secenekleri.NetMaliyet => BaslikTutarNetMaliyet, 
		enum_tutar_secenekleri.NetKarTutari => BaslikTutarNetKarTutari, 
		enum_tutar_secenekleri.NetKarYuzdesi => BaslikTutarNetKarYuzdesi, 
		_ => EnumUtility.EnumToLocalizedString(tutar2_secenek), 
	};

	public string baslik_tutar3 => tutar3_secenek switch
	{
		enum_tutar_secenekleri.BrutTutar => BaslikTutarBrutTutar, 
		enum_tutar_secenekleri.Gosterme => "", 
		enum_tutar_secenekleri.IskontoTutari => BaslikTutarIskontoTutari, 
		enum_tutar_secenekleri.NetTutar => BaslikTutarNetTutar, 
		enum_tutar_secenekleri.NetTutarKdvDahil => BaslikTutarNetTutarKdvDahil, 
		enum_tutar_secenekleri.NetMaliyet => BaslikTutarNetMaliyet, 
		enum_tutar_secenekleri.NetKarTutari => BaslikTutarNetKarTutari, 
		enum_tutar_secenekleri.NetKarYuzdesi => BaslikTutarNetKarYuzdesi, 
		_ => EnumUtility.EnumToLocalizedString(tutar3_secenek), 
	};

	public string baslik_miktar1 => miktar1_secenek switch
	{
		enum_miktar_secenekleri.BrutSatisMiktari => BaslikMiktarBrutSatisMiktari, 
		enum_miktar_secenekleri.CariAdresSayisi => BaslikMiktarCariAdresSayisi, 
		enum_miktar_secenekleri.Gosterme => "", 
		enum_miktar_secenekleri.IadeMiktari => BaslikMiktarIadeMiktari, 
		enum_miktar_secenekleri.NetSatisMiktari => BaslikMiktarNetSatisMiktari, 
		enum_miktar_secenekleri.NetSatisMiktariBirim2 => BaslikMiktarNetSatisMiktariBirim2, 
		enum_miktar_secenekleri.NetSatisMiktariBirim3 => BaslikMiktarNetSatisMiktariBirim3, 
		_ => EnumUtility.EnumToLocalizedString(miktar1_secenek), 
	};

	public string baslik_miktar2 => miktar2_secenek switch
	{
		enum_miktar_secenekleri.BrutSatisMiktari => BaslikMiktarBrutSatisMiktari, 
		enum_miktar_secenekleri.CariAdresSayisi => BaslikMiktarCariAdresSayisi, 
		enum_miktar_secenekleri.Gosterme => "", 
		enum_miktar_secenekleri.IadeMiktari => BaslikMiktarIadeMiktari, 
		enum_miktar_secenekleri.NetSatisMiktari => BaslikMiktarNetSatisMiktari, 
		enum_miktar_secenekleri.NetSatisMiktariBirim2 => BaslikMiktarNetSatisMiktariBirim2, 
		enum_miktar_secenekleri.NetSatisMiktariBirim3 => BaslikMiktarNetSatisMiktariBirim3, 
		_ => EnumUtility.EnumToLocalizedString(miktar2_secenek), 
	};

	public string baslik_miktar3 => miktar3_secenek switch
	{
		enum_miktar_secenekleri.BrutSatisMiktari => BaslikMiktarBrutSatisMiktari, 
		enum_miktar_secenekleri.CariAdresSayisi => BaslikMiktarCariAdresSayisi, 
		enum_miktar_secenekleri.Gosterme => "", 
		enum_miktar_secenekleri.IadeMiktari => BaslikMiktarIadeMiktari, 
		enum_miktar_secenekleri.NetSatisMiktari => BaslikMiktarNetSatisMiktari, 
		enum_miktar_secenekleri.NetSatisMiktariBirim2 => BaslikMiktarNetSatisMiktariBirim2, 
		enum_miktar_secenekleri.NetSatisMiktariBirim3 => BaslikMiktarNetSatisMiktariBirim3, 
		_ => EnumUtility.EnumToLocalizedString(miktar3_secenek), 
	};

	public int satir_sayisi => list_items.Count;

	public enum_gruplandirma_secenekleri gruplandirma_secenegi { get; set; }

	public enum_siralama_secenekleri siralama_secenegi { get; set; }

	public enum_tutar_secenekleri tutar1_secenek { get; set; }

	public enum_tutar_secenekleri tutar2_secenek { get; set; }

	public enum_tutar_secenekleri tutar3_secenek { get; set; }

	public enum_miktar_secenekleri miktar1_secenek { get; set; }

	public enum_miktar_secenekleri miktar2_secenek { get; set; }

	public enum_miktar_secenekleri miktar3_secenek { get; set; }

	public List<RaporStokSatisListItem> list_items { get; set; }

	public double Toplam_Miktar1
	{
		get
		{
			double num = 0.0;
			if (miktar1_secenek != enum_miktar_secenekleri.CariAdresSayisi)
			{
				foreach (RaporStokSatisListItem list_item in list_items)
				{
					num += list_item.miktar1;
				}
			}
			else
			{
				List<string> list = new List<string>();
				foreach (RaporStokSatisListItem list_item2 in list_items)
				{
					foreach (string item in list_item2.cari_adresleri)
					{
						if (!list.Contains(item))
						{
							list.Add(item);
							num += 1.0;
						}
					}
				}
			}
			return num;
		}
	}

	public double Toplam_Miktar2
	{
		get
		{
			double num = 0.0;
			if (miktar2_secenek != enum_miktar_secenekleri.CariAdresSayisi)
			{
				foreach (RaporStokSatisListItem list_item in list_items)
				{
					num += list_item.miktar2;
				}
			}
			else
			{
				List<string> list = new List<string>();
				foreach (RaporStokSatisListItem list_item2 in list_items)
				{
					foreach (string item in list_item2.cari_adresleri)
					{
						if (!list.Contains(item))
						{
							list.Add(item);
							num += 1.0;
						}
					}
				}
			}
			return num;
		}
	}

	public double Toplam_Miktar3
	{
		get
		{
			double num = 0.0;
			if (miktar3_secenek != enum_miktar_secenekleri.CariAdresSayisi)
			{
				foreach (RaporStokSatisListItem list_item in list_items)
				{
					num += list_item.miktar3;
				}
			}
			else
			{
				List<string> list = new List<string>();
				foreach (RaporStokSatisListItem list_item2 in list_items)
				{
					foreach (string item in list_item2.cari_adresleri)
					{
						if (!list.Contains(item))
						{
							list.Add(item);
							num += 1.0;
						}
					}
				}
			}
			return num;
		}
	}

	public double Toplam_Tutar1
	{
		get
		{
			if (tutar1_secenek != enum_tutar_secenekleri.NetKarYuzdesi)
			{
				double num = 0.0;
				{
					foreach (RaporStokSatisListItem list_item in list_items)
					{
						num += list_item.tutar1;
					}
					return num;
				}
			}
			double num2 = 0.0;
			double num3 = 0.0;
			foreach (RaporStokSatisListItem list_item2 in list_items)
			{
				num2 += list_item2.net_tutar;
				num3 += list_item2.net_maliyet;
			}
			return num2 * 100.0 / num3 - 100.0;
		}
	}

	public double Toplam_Tutar2
	{
		get
		{
			if (tutar2_secenek != enum_tutar_secenekleri.NetKarYuzdesi)
			{
				double num = 0.0;
				{
					foreach (RaporStokSatisListItem list_item in list_items)
					{
						num += list_item.tutar2;
					}
					return num;
				}
			}
			double num2 = 0.0;
			double num3 = 0.0;
			foreach (RaporStokSatisListItem list_item2 in list_items)
			{
				num2 += list_item2.net_tutar;
				num3 += list_item2.net_maliyet;
			}
			return num2 * 100.0 / num3 - 100.0;
		}
	}

	public double Toplam_Tutar3
	{
		get
		{
			if (tutar3_secenek != enum_tutar_secenekleri.NetKarYuzdesi)
			{
				double num = 0.0;
				{
					foreach (RaporStokSatisListItem list_item in list_items)
					{
						num += list_item.tutar3;
					}
					return num;
				}
			}
			double num2 = 0.0;
			double num3 = 0.0;
			foreach (RaporStokSatisListItem list_item2 in list_items)
			{
				num2 += list_item2.net_tutar;
				num3 += list_item2.net_maliyet;
			}
			return num2 * 100.0 / num3 - 100.0;
		}
	}

	public string Miktar1_Birimleri
	{
		get
		{
			if (miktar1_secenek == enum_miktar_secenekleri.CariAdresSayisi)
			{
				return "Nokta";
			}
			string text = "";
			List<string> list = new List<string>();
			foreach (RaporStokSatisListItem list_item in list_items)
			{
				string text2 = "";
				text2 = ((miktar1_secenek == enum_miktar_secenekleri.NetSatisMiktariBirim2) ? list_item.birim2_adi : ((miktar1_secenek != enum_miktar_secenekleri.NetSatisMiktariBirim3) ? list_item.birim_adi : list_item.birim3_adi));
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
			if (miktar2_secenek == enum_miktar_secenekleri.CariAdresSayisi)
			{
				return "Nokta";
			}
			string text = "";
			List<string> list = new List<string>();
			foreach (RaporStokSatisListItem list_item in list_items)
			{
				string text2 = "";
				text2 = ((miktar2_secenek == enum_miktar_secenekleri.NetSatisMiktariBirim2) ? list_item.birim2_adi : ((miktar2_secenek != enum_miktar_secenekleri.NetSatisMiktariBirim3) ? list_item.birim_adi : list_item.birim3_adi));
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
			if (miktar3_secenek == enum_miktar_secenekleri.CariAdresSayisi)
			{
				return "Nokta";
			}
			string text = "";
			List<string> list = new List<string>();
			foreach (RaporStokSatisListItem list_item in list_items)
			{
				string text2 = "";
				text2 = ((miktar3_secenek == enum_miktar_secenekleri.NetSatisMiktariBirim2) ? list_item.birim2_adi : ((miktar3_secenek != enum_miktar_secenekleri.NetSatisMiktariBirim3) ? list_item.birim_adi : list_item.birim3_adi));
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

	public RaporStokSatisSonuc()
	{
		BaslikTutarBrutTutar = "Brüt tutar";
		BaslikTutarIskontoTutari = "İskonto tutarı";
		BaslikTutarNetTutar = "Net tutar";
		BaslikTutarNetTutarKdvDahil = "Net tutar (Kdv dahil)";
		BaslikTutarNetKarTutari = "Net kar tutarı";
		BaslikTutarNetKarYuzdesi = "Net kar yüzdesi";
		BaslikTutarNetMaliyet = "Net maliyet";
		BaslikMiktarBrutSatisMiktari = "Brüt satış miktarı";
		BaslikMiktarIadeMiktari = "İade miktarı";
		BaslikMiktarNetSatisMiktari = "Net satış miktarı";
		BaslikMiktarNetSatisMiktariBirim2 = "Net satış miktarı (2. birim)";
		BaslikMiktarNetSatisMiktariBirim3 = "Net satış miktarı (3. birim)";
		BaslikMiktarCariAdresSayisi = "Cari adres sayısı";
		list_items = new List<RaporStokSatisListItem>();
	}

	public void SetBaslikTutarBrutTutar(string value)
	{
		BaslikTutarBrutTutar = value;
	}

	public void SetBaslikTutarIskontoTutari(string value)
	{
		BaslikTutarIskontoTutari = value;
	}

	public void SetBaslikTutarNetTutar(string value)
	{
		BaslikTutarNetTutar = value;
	}

	public void SetBaslikTutarNetTutarKdvDahil(string value)
	{
		BaslikTutarNetTutarKdvDahil = value;
	}

	public void SetBaslikTutarNetKarTutari(string value)
	{
		BaslikTutarNetKarTutari = value;
	}

	public void SetBaslikTutarNetKarYuzdesi(string value)
	{
		BaslikTutarNetKarYuzdesi = value;
	}

	public void SetBaslikTutarNetMaliyet(string value)
	{
		BaslikTutarNetMaliyet = value;
	}

	public void SetBaslikMiktarBrutSatisMiktari(string value)
	{
		BaslikMiktarBrutSatisMiktari = value;
	}

	public void SetBaslikMiktarIadeMiktari(string value)
	{
		BaslikMiktarIadeMiktari = value;
	}

	public void SetBaslikMiktarNetSatisMiktari(string value)
	{
		BaslikMiktarNetSatisMiktari = value;
	}

	public void SetBaslikMiktarNetSatisMiktariBirim2(string value)
	{
		BaslikMiktarNetSatisMiktariBirim2 = value;
	}

	public void SetBaslikMiktarNetSatisMiktariBirim3(string value)
	{
		BaslikMiktarNetSatisMiktariBirim3 = value;
	}

	public void SetBaslikMiktarCariAdresSayisi(string value)
	{
		BaslikMiktarCariAdresSayisi = value;
	}

	public static byte[] WriteToByteArray(RaporStokSatisSonuc toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokSatisSonuc toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokSatisSonuc toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 2;
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
		foreach (RaporStokSatisListItem list_item in toWrite.list_items)
		{
			RaporStokSatisListItem.WriteToBinaryWriter(list_item, writer, 2);
		}
		writer.Write(toWrite.BaslikTutarBrutTutar);
		writer.Write(toWrite.BaslikTutarIskontoTutari);
		writer.Write(toWrite.BaslikTutarNetTutar);
		writer.Write(toWrite.BaslikTutarNetTutarKdvDahil);
		writer.Write(toWrite.BaslikTutarNetKarTutari);
		writer.Write(toWrite.BaslikTutarNetKarYuzdesi);
		writer.Write(toWrite.BaslikTutarNetMaliyet);
		writer.Write(toWrite.BaslikMiktarBrutSatisMiktari);
		writer.Write(toWrite.BaslikMiktarIadeMiktari);
		writer.Write(toWrite.BaslikMiktarNetSatisMiktari);
		writer.Write(toWrite.BaslikMiktarNetSatisMiktariBirim2);
		writer.Write(toWrite.BaslikMiktarCariAdresSayisi);
		if (versiyon >= 2)
		{
			writer.Write(toWrite.BaslikMiktarNetSatisMiktariBirim3);
		}
	}

	public static RaporStokSatisSonuc ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokSatisSonuc result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokSatisSonuc ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporStokSatisSonuc ReadFromBinaryReader(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		RaporStokSatisSonuc raporStokSatisSonuc = new RaporStokSatisSonuc();
		raporStokSatisSonuc.gruplandirma_secenegi = (enum_gruplandirma_secenekleri)reader.ReadInt32();
		raporStokSatisSonuc.siralama_secenegi = (enum_siralama_secenekleri)reader.ReadInt32();
		raporStokSatisSonuc.tutar1_secenek = (enum_tutar_secenekleri)reader.ReadInt32();
		raporStokSatisSonuc.tutar2_secenek = (enum_tutar_secenekleri)reader.ReadInt32();
		raporStokSatisSonuc.tutar3_secenek = (enum_tutar_secenekleri)reader.ReadInt32();
		raporStokSatisSonuc.miktar1_secenek = (enum_miktar_secenekleri)reader.ReadInt32();
		raporStokSatisSonuc.miktar2_secenek = (enum_miktar_secenekleri)reader.ReadInt32();
		raporStokSatisSonuc.miktar3_secenek = (enum_miktar_secenekleri)reader.ReadInt32();
		int num2 = reader.ReadInt32();
		raporStokSatisSonuc.list_items = new List<RaporStokSatisListItem>();
		for (int i = 0; i < num2; i++)
		{
			raporStokSatisSonuc.list_items.Add(RaporStokSatisListItem.ReadFromStream(reader));
		}
		raporStokSatisSonuc.SetBaslikTutarBrutTutar(reader.ReadString());
		raporStokSatisSonuc.SetBaslikTutarIskontoTutari(reader.ReadString());
		raporStokSatisSonuc.SetBaslikTutarNetTutar(reader.ReadString());
		raporStokSatisSonuc.SetBaslikTutarNetTutarKdvDahil(reader.ReadString());
		raporStokSatisSonuc.SetBaslikTutarNetKarTutari(reader.ReadString());
		raporStokSatisSonuc.SetBaslikTutarNetKarYuzdesi(reader.ReadString());
		raporStokSatisSonuc.SetBaslikTutarNetMaliyet(reader.ReadString());
		raporStokSatisSonuc.SetBaslikMiktarBrutSatisMiktari(reader.ReadString());
		raporStokSatisSonuc.SetBaslikMiktarIadeMiktari(reader.ReadString());
		raporStokSatisSonuc.SetBaslikMiktarNetSatisMiktari(reader.ReadString());
		raporStokSatisSonuc.SetBaslikMiktarNetSatisMiktariBirim2(reader.ReadString());
		raporStokSatisSonuc.SetBaslikMiktarCariAdresSayisi(reader.ReadString());
		if (num >= 2)
		{
			raporStokSatisSonuc.SetBaslikMiktarNetSatisMiktariBirim3(reader.ReadString());
		}
		return raporStokSatisSonuc;
	}
}
