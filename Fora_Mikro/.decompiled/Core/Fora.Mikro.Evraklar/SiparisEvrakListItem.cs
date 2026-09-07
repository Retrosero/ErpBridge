using System;
using System.IO;
using Fora.Mikro.Siparis;

namespace Fora.Mikro.Evraklar;

public class SiparisEvrakListItem
{
	public bool Selected { get; set; }

	public string sip_evrakno_seri { get; set; }

	public int sip_evrakno_sira { get; set; }

	public DateTime sip_tarih { get; set; }

	public DateTime sip_teslim_tarih { get; set; }

	public string sip_musteri_kod { get; set; }

	public double sip_miktar { get; set; }

	public double sip_teslim_miktar { get; set; }

	public enum_sip_tip sip_tip { get; set; }

	public enum_sip_cins sip_cins { get; set; }

	public static byte[] getBytesWithManualWrite(SiparisEvrakListItem toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(SiparisEvrakListItem toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToStream(toWrite, writer);
	}

	public static void WriteToStream(SiparisEvrakListItem toWrite, BinaryWriter writer)
	{
		try
		{
			int value = 1;
			writer.Write(value);
			writer.Write(toWrite.sip_evrakno_seri);
			writer.Write(toWrite.sip_evrakno_sira);
			writer.Write(toWrite.sip_tarih.Ticks);
			writer.Write(toWrite.sip_teslim_tarih.Ticks);
			writer.Write(toWrite.sip_musteri_kod);
			writer.Write(toWrite.sip_miktar);
			writer.Write(toWrite.sip_teslim_miktar);
			writer.Write((int)toWrite.sip_tip);
			writer.Write((int)toWrite.sip_cins);
		}
		catch
		{
		}
	}

	public static SiparisEvrakListItem ReadFromStream(Stream from)
	{
		return ReadFromStream(new BinaryReader(from));
	}

	public static SiparisEvrakListItem ReadFromStream(BinaryReader reader)
	{
		SiparisEvrakListItem siparisEvrakListItem = new SiparisEvrakListItem();
		reader.ReadInt32();
		siparisEvrakListItem.sip_evrakno_seri = reader.ReadString();
		siparisEvrakListItem.sip_evrakno_sira = reader.ReadInt32();
		siparisEvrakListItem.sip_tarih = new DateTime(reader.ReadInt64());
		siparisEvrakListItem.sip_teslim_tarih = new DateTime(reader.ReadInt64());
		siparisEvrakListItem.sip_musteri_kod = reader.ReadString();
		siparisEvrakListItem.sip_miktar = reader.ReadDouble();
		siparisEvrakListItem.sip_teslim_miktar = reader.ReadDouble();
		siparisEvrakListItem.sip_tip = (enum_sip_tip)reader.ReadInt32();
		siparisEvrakListItem.sip_cins = (enum_sip_cins)reader.ReadInt32();
		return siparisEvrakListItem;
	}
}
