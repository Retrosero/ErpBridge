namespace Fora.Mikro.Stoklar.SonKullanicilar;

public class SonKullanici
{
	public int tuk_RECno { get; set; }

	public string tuk_kodu { get; set; }

	public string tuk_ismi { get; set; }

	public string tuk_Adr1_Cadde { get; set; }

	public string tuk_Adr1_Sokak { get; set; }

	public string tuk_Adr1_Postakodu { get; set; }

	public string tuk_Adr1_Ilce { get; set; }

	public string tuk_Adr1_Il { get; set; }

	public string tuk_Adr1_Ulke { get; set; }

	public string tuk_Adr2_Cadde { get; set; }

	public string tuk_Adr2_Sokak { get; set; }

	public string tuk_Adr2_Postakodu { get; set; }

	public string tuk_Adr2_Ilce { get; set; }

	public string tuk_Adr2_Il { get; set; }

	public string tuk_Adr2_Ulke { get; set; }

	public string tuk_Tel1_Ulkekod { get; set; }

	public string tuk_Tel1_Bolgekod { get; set; }

	public string tuk_Tel1_TelNo1 { get; set; }

	public string tuk_Tel1_TelNo2 { get; set; }

	public string tuk_Tel1_FaxNo { get; set; }

	public string tuk_Tel1_ModemNo { get; set; }

	public string tuk_Tel2_Ulkekod { get; set; }

	public string tuk_Tel2_Bolgekod { get; set; }

	public string tuk_Tel2_TelNo1 { get; set; }

	public string tuk_Tel2_TelNo2 { get; set; }

	public string tuk_Tel2_FaxNo { get; set; }

	public string tuk_Tel2_ModemNo { get; set; }

	public string tuk_yetkili1 { get; set; }

	public string tuk_yetkili2 { get; set; }

	public string tuk_ceptel1 { get; set; }

	public string tuk_ceptel2 { get; set; }

	public string tuk_email1 { get; set; }

	public string tuk_email2 { get; set; }

	public string tuk_GrpKodu { get; set; }

	public string tuk_MuhKodu { get; set; }

	public bool tuk_kilitli_flg { get; set; }

	public string tuk_cari_kodu { get; set; }

	public string tuk_sektor_kodu { get; set; }

	public string tuk_bolge_kodu { get; set; }

	public SonKullanici()
	{
		tuk_RECno = 0;
		tuk_kodu = "";
		tuk_ismi = "";
		tuk_Adr1_Cadde = "";
		tuk_Adr1_Sokak = "";
		tuk_Adr1_Postakodu = "";
		tuk_Adr1_Ilce = "";
		tuk_Adr1_Il = "";
		tuk_Adr1_Ulke = "";
		tuk_Adr2_Cadde = "";
		tuk_Adr2_Sokak = "";
		tuk_Adr2_Postakodu = "";
		tuk_Adr2_Ilce = "";
		tuk_Adr2_Il = "";
		tuk_Adr2_Ulke = "";
		tuk_Tel1_Ulkekod = "";
		tuk_Tel1_Bolgekod = "";
		tuk_Tel1_TelNo1 = "";
		tuk_Tel1_TelNo2 = "";
		tuk_Tel1_FaxNo = "";
		tuk_Tel1_ModemNo = "";
		tuk_Tel2_Ulkekod = "";
		tuk_Tel2_Bolgekod = "";
		tuk_Tel2_TelNo1 = "";
		tuk_Tel2_TelNo2 = "";
		tuk_Tel2_FaxNo = "";
		tuk_Tel2_ModemNo = "";
		tuk_yetkili1 = "";
		tuk_yetkili2 = "";
		tuk_ceptel1 = "";
		tuk_ceptel2 = "";
		tuk_email1 = "";
		tuk_email2 = "";
		tuk_GrpKodu = "";
		tuk_MuhKodu = "";
		tuk_kilitli_flg = false;
		tuk_cari_kodu = "";
		tuk_sektor_kodu = "";
		tuk_bolge_kodu = "";
	}
}
