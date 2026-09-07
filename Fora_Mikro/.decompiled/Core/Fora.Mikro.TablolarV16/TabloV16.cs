using System.Collections.Generic;

namespace Fora.Mikro.TablolarV16;

public class TabloV16
{
	public int TabloID { get; set; }

	public string TabloAdi { get; set; }

	public List<FieldV16> Fieldlar { get; set; }

	public List<string> Indexler { get; set; }

	public int DegisenKayitSayisi { get; set; }

	public int OfflineKayitSayisi { get; set; }

	public int OfflineLastUpdateTriggerRecNo { get; set; }

	public int OfflineLastDeleteTriggerRecNo { get; set; }

	public bool UpdateEdildi { get; set; }

	public TabloV16()
	{
		Fieldlar = new List<FieldV16>();
		Indexler = new List<string>();
		TabloID = 0;
		DegisenKayitSayisi = 0;
		OfflineLastUpdateTriggerRecNo = 0;
		OfflineLastDeleteTriggerRecNo = 0;
		UpdateEdildi = false;
	}

	public string GetCekilecekFieldlar(string prefix)
	{
		bool flag = true;
		string text = "";
		foreach (FieldV16 item in Fieldlar)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				text += ",";
			}
			text = text + prefix + item.Adi;
		}
		if (prefix == "@")
		{
			text = text.ToLower().Replace("ı", "i");
		}
		return text;
	}
}
