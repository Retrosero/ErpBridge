using System.Collections.Generic;
using System.IO;
using Fora.Mikro.Enumler;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokSiparis;

public class RaporStokSiparisSonuc
{
	private string BaslikTutarSiparisBrutTutari;

	private string BaslikTutarSiparisIskontoTutari;

	private string BaslikTutarSiparisNetTutar;

	private string BaslikTutarSiparisNetTutarKdvDahil;

	private string BaslikTutarTeslimEdilenBrutTutari;

	private string BaslikTutarTeslimEdilenIskontoTutari;

	private string BaslikTutarTeslimEdilenNetTutar;

	private string BaslikTutarTeslimEdilenNetTutarKdvDahil;

	private string BaslikTutarBekleyenBrutTutari;

	private string BaslikTutarBekleyenIskontoTutari;

	private string BaslikTutarBekleyenNetTutar;

	private string BaslikTutarBekleyenNetTutarKdvDahil;

	private string BaslikTutarVazgecilenBrutTutari;

	private string BaslikTutarVazgecilenIskontoTutari;

	private string BaslikTutarVazgecilenNetTutar;

	private string BaslikTutarVazgecilenNetTutarKdvDahil;

	private string BaslikMiktarSiparisMiktari;

	private string BaslikMiktarTeslimEdilenMiktar;

	private string BaslikMiktarBekleyenMiktar;

	private string BaslikMiktarVazgecilenMiktar;

	public string baslik_tutar1 => tutar1_secenek switch
	{
		enum_siparis_tutar_secenekleri.BekleyenBrutTutari => BaslikTutarBekleyenBrutTutari, 
		enum_siparis_tutar_secenekleri.BekleyenIskontoTutari => BaslikTutarBekleyenIskontoTutari, 
		enum_siparis_tutar_secenekleri.BekleyenNetTutar => BaslikTutarBekleyenNetTutar, 
		enum_siparis_tutar_secenekleri.BekleyenNetTutarKdvDahil => BaslikTutarBekleyenNetTutarKdvDahil, 
		enum_siparis_tutar_secenekleri.Gosterme => "", 
		enum_siparis_tutar_secenekleri.SiparisBrutTutari => BaslikTutarSiparisBrutTutari, 
		enum_siparis_tutar_secenekleri.SiparisIskontoTutari => BaslikTutarSiparisIskontoTutari, 
		enum_siparis_tutar_secenekleri.SiparisNetTutar => BaslikTutarSiparisNetTutar, 
		enum_siparis_tutar_secenekleri.SiparisNetTutarKdvDahil => BaslikTutarSiparisNetTutarKdvDahil, 
		enum_siparis_tutar_secenekleri.TeslimEdilenBrutTutari => BaslikTutarTeslimEdilenBrutTutari, 
		enum_siparis_tutar_secenekleri.TeslimEdilenIskontoTutari => BaslikTutarTeslimEdilenIskontoTutari, 
		enum_siparis_tutar_secenekleri.TeslimEdilenNetTutar => BaslikTutarTeslimEdilenNetTutar, 
		enum_siparis_tutar_secenekleri.TeslimEdilenNetTutarKdvDahil => BaslikTutarTeslimEdilenNetTutarKdvDahil, 
		enum_siparis_tutar_secenekleri.VazgecilenBrutTutari => BaslikTutarVazgecilenBrutTutari, 
		enum_siparis_tutar_secenekleri.VazgecilenIskontoTutari => BaslikTutarVazgecilenIskontoTutari, 
		enum_siparis_tutar_secenekleri.VazgecilenNetTutar => BaslikTutarVazgecilenNetTutar, 
		enum_siparis_tutar_secenekleri.VazgecilenNetTutarKdvDahil => BaslikTutarVazgecilenNetTutarKdvDahil, 
		_ => EnumUtility.EnumToLocalizedString(tutar1_secenek), 
	};

	public string baslik_tutar2 => tutar2_secenek switch
	{
		enum_siparis_tutar_secenekleri.BekleyenBrutTutari => BaslikTutarBekleyenBrutTutari, 
		enum_siparis_tutar_secenekleri.BekleyenIskontoTutari => BaslikTutarBekleyenIskontoTutari, 
		enum_siparis_tutar_secenekleri.BekleyenNetTutar => BaslikTutarBekleyenNetTutar, 
		enum_siparis_tutar_secenekleri.BekleyenNetTutarKdvDahil => BaslikTutarBekleyenNetTutarKdvDahil, 
		enum_siparis_tutar_secenekleri.Gosterme => "", 
		enum_siparis_tutar_secenekleri.SiparisBrutTutari => BaslikTutarSiparisBrutTutari, 
		enum_siparis_tutar_secenekleri.SiparisIskontoTutari => BaslikTutarSiparisIskontoTutari, 
		enum_siparis_tutar_secenekleri.SiparisNetTutar => BaslikTutarSiparisNetTutar, 
		enum_siparis_tutar_secenekleri.SiparisNetTutarKdvDahil => BaslikTutarSiparisNetTutarKdvDahil, 
		enum_siparis_tutar_secenekleri.TeslimEdilenBrutTutari => BaslikTutarTeslimEdilenBrutTutari, 
		enum_siparis_tutar_secenekleri.TeslimEdilenIskontoTutari => BaslikTutarTeslimEdilenIskontoTutari, 
		enum_siparis_tutar_secenekleri.TeslimEdilenNetTutar => BaslikTutarTeslimEdilenNetTutar, 
		enum_siparis_tutar_secenekleri.TeslimEdilenNetTutarKdvDahil => BaslikTutarTeslimEdilenNetTutarKdvDahil, 
		enum_siparis_tutar_secenekleri.VazgecilenBrutTutari => BaslikTutarVazgecilenBrutTutari, 
		enum_siparis_tutar_secenekleri.VazgecilenIskontoTutari => BaslikTutarVazgecilenIskontoTutari, 
		enum_siparis_tutar_secenekleri.VazgecilenNetTutar => BaslikTutarVazgecilenNetTutar, 
		enum_siparis_tutar_secenekleri.VazgecilenNetTutarKdvDahil => BaslikTutarVazgecilenNetTutarKdvDahil, 
		_ => EnumUtility.EnumToLocalizedString(tutar2_secenek), 
	};

	public string baslik_tutar3 => tutar3_secenek switch
	{
		enum_siparis_tutar_secenekleri.BekleyenBrutTutari => BaslikTutarBekleyenBrutTutari, 
		enum_siparis_tutar_secenekleri.BekleyenIskontoTutari => BaslikTutarBekleyenIskontoTutari, 
		enum_siparis_tutar_secenekleri.BekleyenNetTutar => BaslikTutarBekleyenNetTutar, 
		enum_siparis_tutar_secenekleri.BekleyenNetTutarKdvDahil => BaslikTutarBekleyenNetTutarKdvDahil, 
		enum_siparis_tutar_secenekleri.Gosterme => "", 
		enum_siparis_tutar_secenekleri.SiparisBrutTutari => BaslikTutarSiparisBrutTutari, 
		enum_siparis_tutar_secenekleri.SiparisIskontoTutari => BaslikTutarSiparisIskontoTutari, 
		enum_siparis_tutar_secenekleri.SiparisNetTutar => BaslikTutarSiparisNetTutar, 
		enum_siparis_tutar_secenekleri.SiparisNetTutarKdvDahil => BaslikTutarSiparisNetTutarKdvDahil, 
		enum_siparis_tutar_secenekleri.TeslimEdilenBrutTutari => BaslikTutarTeslimEdilenBrutTutari, 
		enum_siparis_tutar_secenekleri.TeslimEdilenIskontoTutari => BaslikTutarTeslimEdilenIskontoTutari, 
		enum_siparis_tutar_secenekleri.TeslimEdilenNetTutar => BaslikTutarTeslimEdilenNetTutar, 
		enum_siparis_tutar_secenekleri.TeslimEdilenNetTutarKdvDahil => BaslikTutarTeslimEdilenNetTutarKdvDahil, 
		enum_siparis_tutar_secenekleri.VazgecilenBrutTutari => BaslikTutarVazgecilenBrutTutari, 
		enum_siparis_tutar_secenekleri.VazgecilenIskontoTutari => BaslikTutarVazgecilenIskontoTutari, 
		enum_siparis_tutar_secenekleri.VazgecilenNetTutar => BaslikTutarVazgecilenNetTutar, 
		enum_siparis_tutar_secenekleri.VazgecilenNetTutarKdvDahil => BaslikTutarVazgecilenNetTutarKdvDahil, 
		_ => EnumUtility.EnumToLocalizedString(tutar3_secenek), 
	};

	public string baslik_miktar1 => miktar1_secenek switch
	{
		enum_siparis_miktar_secenekleri.BekleyenMiktar => BaslikMiktarBekleyenMiktar, 
		enum_siparis_miktar_secenekleri.Gosterme => "", 
		enum_siparis_miktar_secenekleri.SiparisMiktari => BaslikMiktarSiparisMiktari, 
		enum_siparis_miktar_secenekleri.TeslimEdilenMiktar => BaslikMiktarTeslimEdilenMiktar, 
		enum_siparis_miktar_secenekleri.VazgecilenMiktar => BaslikMiktarVazgecilenMiktar, 
		_ => EnumUtility.EnumToLocalizedString(miktar1_secenek), 
	};

	public string baslik_miktar2 => miktar2_secenek switch
	{
		enum_siparis_miktar_secenekleri.BekleyenMiktar => BaslikMiktarBekleyenMiktar, 
		enum_siparis_miktar_secenekleri.Gosterme => "", 
		enum_siparis_miktar_secenekleri.SiparisMiktari => BaslikMiktarSiparisMiktari, 
		enum_siparis_miktar_secenekleri.TeslimEdilenMiktar => BaslikMiktarTeslimEdilenMiktar, 
		enum_siparis_miktar_secenekleri.VazgecilenMiktar => BaslikMiktarVazgecilenMiktar, 
		_ => EnumUtility.EnumToLocalizedString(miktar2_secenek), 
	};

	public string baslik_miktar3 => miktar3_secenek switch
	{
		enum_siparis_miktar_secenekleri.BekleyenMiktar => BaslikMiktarBekleyenMiktar, 
		enum_siparis_miktar_secenekleri.Gosterme => "", 
		enum_siparis_miktar_secenekleri.SiparisMiktari => BaslikMiktarSiparisMiktari, 
		enum_siparis_miktar_secenekleri.TeslimEdilenMiktar => BaslikMiktarTeslimEdilenMiktar, 
		enum_siparis_miktar_secenekleri.VazgecilenMiktar => BaslikMiktarVazgecilenMiktar, 
		_ => EnumUtility.EnumToLocalizedString(miktar3_secenek), 
	};

	public enum_gruplandirma_secenekleri gruplandirma_secenegi { get; set; }

	public enum_siralama_secenekleri siralama_secenegi { get; set; }

	public enum_siparis_tutar_secenekleri tutar1_secenek { get; set; }

	public enum_siparis_tutar_secenekleri tutar2_secenek { get; set; }

	public enum_siparis_tutar_secenekleri tutar3_secenek { get; set; }

	public enum_siparis_miktar_secenekleri miktar1_secenek { get; set; }

	public enum_siparis_miktar_secenekleri miktar2_secenek { get; set; }

	public enum_siparis_miktar_secenekleri miktar3_secenek { get; set; }

	public List<RaporStokSiparisListItem> list_items { get; set; }

	public double Toplam_Miktar1
	{
		get
		{
			double num = 0.0;
			foreach (RaporStokSiparisListItem list_item in list_items)
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
			foreach (RaporStokSiparisListItem list_item in list_items)
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
			foreach (RaporStokSiparisListItem list_item in list_items)
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
			foreach (RaporStokSiparisListItem list_item in list_items)
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
			foreach (RaporStokSiparisListItem list_item in list_items)
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
			foreach (RaporStokSiparisListItem list_item in list_items)
			{
				num += list_item.tutar3;
			}
			return num;
		}
	}

	public RaporStokSiparisSonuc()
	{
		BaslikTutarSiparisBrutTutari = "Sipariş brüt tutarı";
		BaslikTutarSiparisIskontoTutari = "Sipariş iskonto tutarı";
		BaslikTutarSiparisNetTutar = "Sipariş net tutar";
		BaslikTutarSiparisNetTutarKdvDahil = "Sipariş net tutar (Kdv dahil)";
		BaslikTutarTeslimEdilenBrutTutari = "Teslim edilen brüt tutarı";
		BaslikTutarTeslimEdilenIskontoTutari = "Teslim edilen iskonto tutarı";
		BaslikTutarTeslimEdilenNetTutar = "Teslim edilen net tutar";
		BaslikTutarTeslimEdilenNetTutarKdvDahil = "Teslim edilen net tutar (Kdv dahil)";
		BaslikTutarBekleyenBrutTutari = "Bekleyen brüt tutarı";
		BaslikTutarBekleyenIskontoTutari = "Bekleyen iskonto tutarı";
		BaslikTutarBekleyenNetTutar = "Bekleyen net tutar";
		BaslikTutarBekleyenNetTutarKdvDahil = "Bekleyen net tutar (Kdv dahil)";
		BaslikTutarVazgecilenBrutTutari = "Vazgeçilen brüt tutarı";
		BaslikTutarVazgecilenIskontoTutari = "Vazgeçilen iskonto tutarı";
		BaslikTutarVazgecilenNetTutar = "Vazgeçilen net tutar";
		BaslikTutarVazgecilenNetTutarKdvDahil = "Vazgeçilen net tutar (Kdv dahil)";
		BaslikMiktarSiparisMiktari = "Sipariş miktarı";
		BaslikMiktarTeslimEdilenMiktar = "Teslim edilen miktar";
		BaslikMiktarBekleyenMiktar = "Bekleyen miktar";
		BaslikMiktarVazgecilenMiktar = "Vazgeçilen miktar";
		list_items = new List<RaporStokSiparisListItem>();
	}

	public static byte[] WriteToByteArray(RaporStokSiparisSonuc toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokSiparisSonuc toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokSiparisSonuc toWrite, BinaryWriter writer, int versiyon)
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
		foreach (RaporStokSiparisListItem list_item in toWrite.list_items)
		{
			RaporStokSiparisListItem.WriteToStream(list_item, writer);
		}
		writer.Write(toWrite.BaslikTutarSiparisBrutTutari);
		writer.Write(toWrite.BaslikTutarSiparisIskontoTutari);
		writer.Write(toWrite.BaslikTutarSiparisNetTutar);
		writer.Write(toWrite.BaslikTutarSiparisNetTutarKdvDahil);
		writer.Write(toWrite.BaslikTutarTeslimEdilenBrutTutari);
		writer.Write(toWrite.BaslikTutarTeslimEdilenIskontoTutari);
		writer.Write(toWrite.BaslikTutarTeslimEdilenNetTutar);
		writer.Write(toWrite.BaslikTutarTeslimEdilenNetTutarKdvDahil);
		writer.Write(toWrite.BaslikTutarBekleyenBrutTutari);
		writer.Write(toWrite.BaslikTutarBekleyenIskontoTutari);
		writer.Write(toWrite.BaslikTutarBekleyenNetTutar);
		writer.Write(toWrite.BaslikTutarBekleyenNetTutarKdvDahil);
		writer.Write(toWrite.BaslikTutarVazgecilenBrutTutari);
		writer.Write(toWrite.BaslikTutarVazgecilenIskontoTutari);
		writer.Write(toWrite.BaslikTutarVazgecilenNetTutar);
		writer.Write(toWrite.BaslikTutarVazgecilenNetTutarKdvDahil);
		writer.Write(toWrite.BaslikMiktarSiparisMiktari);
		writer.Write(toWrite.BaslikMiktarTeslimEdilenMiktar);
		writer.Write(toWrite.BaslikMiktarBekleyenMiktar);
		writer.Write(toWrite.BaslikMiktarVazgecilenMiktar);
	}

	public static RaporStokSiparisSonuc ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokSiparisSonuc result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokSiparisSonuc ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporStokSiparisSonuc ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		RaporStokSiparisSonuc raporStokSiparisSonuc = new RaporStokSiparisSonuc();
		raporStokSiparisSonuc.gruplandirma_secenegi = (enum_gruplandirma_secenekleri)reader.ReadInt32();
		raporStokSiparisSonuc.siralama_secenegi = (enum_siralama_secenekleri)reader.ReadInt32();
		raporStokSiparisSonuc.tutar1_secenek = (enum_siparis_tutar_secenekleri)reader.ReadInt32();
		raporStokSiparisSonuc.tutar2_secenek = (enum_siparis_tutar_secenekleri)reader.ReadInt32();
		raporStokSiparisSonuc.tutar3_secenek = (enum_siparis_tutar_secenekleri)reader.ReadInt32();
		raporStokSiparisSonuc.miktar1_secenek = (enum_siparis_miktar_secenekleri)reader.ReadInt32();
		raporStokSiparisSonuc.miktar2_secenek = (enum_siparis_miktar_secenekleri)reader.ReadInt32();
		raporStokSiparisSonuc.miktar3_secenek = (enum_siparis_miktar_secenekleri)reader.ReadInt32();
		int num = reader.ReadInt32();
		raporStokSiparisSonuc.list_items = new List<RaporStokSiparisListItem>();
		for (int i = 0; i < num; i++)
		{
			raporStokSiparisSonuc.list_items.Add(RaporStokSiparisListItem.ReadFromStream(reader));
		}
		raporStokSiparisSonuc.BaslikTutarSiparisBrutTutari = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarSiparisIskontoTutari = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarSiparisNetTutar = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarSiparisNetTutarKdvDahil = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarTeslimEdilenBrutTutari = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarTeslimEdilenIskontoTutari = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarTeslimEdilenNetTutar = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarTeslimEdilenNetTutarKdvDahil = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarBekleyenBrutTutari = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarBekleyenIskontoTutari = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarBekleyenNetTutar = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarBekleyenNetTutarKdvDahil = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarVazgecilenBrutTutari = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarVazgecilenIskontoTutari = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarVazgecilenNetTutar = reader.ReadString();
		raporStokSiparisSonuc.BaslikTutarVazgecilenNetTutarKdvDahil = reader.ReadString();
		raporStokSiparisSonuc.BaslikMiktarSiparisMiktari = reader.ReadString();
		raporStokSiparisSonuc.BaslikMiktarTeslimEdilenMiktar = reader.ReadString();
		raporStokSiparisSonuc.BaslikMiktarBekleyenMiktar = reader.ReadString();
		raporStokSiparisSonuc.BaslikMiktarVazgecilenMiktar = reader.ReadString();
		return raporStokSiparisSonuc;
	}
}
