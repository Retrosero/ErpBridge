using System.Collections.Generic;

namespace Fora.Mikro.Dovizler;

public class DovizCinsiTanimlari
{
	public List<DovizCinsiTanimi> _doviz_cinsi_tanimlari;

	public DovizCinsiTanimlari()
	{
		_doviz_cinsi_tanimlari = new List<DovizCinsiTanimi>();
		for (int i = 0; i < 256; i++)
		{
			_doviz_cinsi_tanimlari.Add(new DovizCinsiTanimi());
		}
		_doviz_cinsi_tanimlari[0].SetValue(0, "", "TL", "Türk Lirası", "Türk Lirası", "Kuruş", 2, "Kr");
		_doviz_cinsi_tanimlari[1].SetValue(1, "", "USD", "Amerikan Doları", "US Dollar", "Sent", 2, "Ct");
		_doviz_cinsi_tanimlari[2].SetValue(2, "", "EUR", "Euro", "Euro", "Sent", 2, "Ct");
		_doviz_cinsi_tanimlari[3].SetValue(3, "", "CAD", "Kanada Doları", "Canadian Dollar", "Sent", 2, "Ct");
		_doviz_cinsi_tanimlari[4].SetValue(4, "", "DKK", "Danimarka Kronu", "Danish Krone", "Öre", 2, "");
		_doviz_cinsi_tanimlari[5].SetValue(5, "", "SEK", "İsveç Kronu", "Swedish Krona", "Öre", 2, "");
		_doviz_cinsi_tanimlari[6].SetValue(6, "", "CHF", "İsviçre Frangı", "Swiss Franc", "Sent", 2, "");
		_doviz_cinsi_tanimlari[7].SetValue(7, "", "NOK", "Norveç Kronu", "Norwegian Krone", "Öre", 2, "");
		_doviz_cinsi_tanimlari[8].SetValue(8, "", "JPY", "Japon Yeni", "Yen", "Sent", 2, "");
		_doviz_cinsi_tanimlari[9].SetValue(9, "", "SAR", "Suudi Arab. Riyali", "Saudi Riyal", "Halalat", 2, "");
		_doviz_cinsi_tanimlari[10].SetValue(10, "", "KWD", "Kuveyt Dinarı", "Kuwaiti Dinar", "Filin", 2, "");
		_doviz_cinsi_tanimlari[11].SetValue(11, "", "AUD", "Avustralya Doları", "Australian Dollar", "Sent", 2, "");
		_doviz_cinsi_tanimlari[12].SetValue(12, "", "GBP", "İngiliz Paundu", "Pound Sterling", "Peni", 2, "");
		_doviz_cinsi_tanimlari[13].SetValue(13, "", "IRR", "İran Riyali", "Iranian Rial", "", 2, "");
		_doviz_cinsi_tanimlari[14].SetValue(14, "", "SYP", "Suriye Lirası", "Syrian Pound", "", 2, "");
		_doviz_cinsi_tanimlari[15].SetValue(15, "", "JOD", "Ürdün Dinarı", "Jordanian Dinar", "", 2, "");
		_doviz_cinsi_tanimlari[16].SetValue(16, "", "BGN", "Bulgar Levası", "Bulgarian Lev", "", 2, "");
		_doviz_cinsi_tanimlari[17].SetValue(17, "", "RON", "Yeni Rumen Leyi", "Yeni Rumen Leyi", "", 2, "");
		_doviz_cinsi_tanimlari[18].SetValue(18, "", "ILS", "Yeni İsrail Şekeli", "New Israeli Sheqel", "", 2, "");
		_doviz_cinsi_tanimlari[19].SetValue(19, "", "AZN", "Manat", "Azerbaijanian Manat", "Qəpik", 2, "Qəp");
	}

	public DovizCinsiTanimi GetDovizCinsiTanimi(int doviz_kod)
	{
		if (doviz_kod == 255)
		{
			return _doviz_cinsi_tanimlari[0];
		}
		return _doviz_cinsi_tanimlari[doviz_kod];
	}
}
