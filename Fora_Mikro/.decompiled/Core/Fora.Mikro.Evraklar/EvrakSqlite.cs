using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Depolar;
using Fora.Mikro.DepolarArasiSiparisler;
using Fora.Mikro.Enumler;
using Fora.Mikro.Firmalar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Projeler;
using Fora.Mikro.Siparis;
using Fora.Mikro.SorumlulukMerkezleri;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Subeler;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Evraklar;

public static class EvrakSqlite
{
	public static Evrak GetEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_GenelEvrakTipleri EvrakTipi, int AlternatifDovizCinsi, enum_sip_cins sip_cins, enum_sth_cins sth_cins)
	{
		return EvrakTipi switch
		{
			enum_GenelEvrakTipleri.AlinanSiparis => GetSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sip_tip.Talep, sip_cins), 
			enum_GenelEvrakTipleri.VerilenSiparis => GetSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sip_tip.Temin, sip_cins), 
			enum_GenelEvrakTipleri.AlisFaturasi => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.AlisFaturasi, AlternatifDovizCinsi), 
			enum_GenelEvrakTipleri.AlisIrsaliyesi => GetIrsaliyeEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sth_evraktip.GirisIrsaliyesi, AlternatifDovizCinsi, sth_cins), 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi => GetIrsaliyeEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sth_evraktip.DepolarArasiNakliyeFisi, AlternatifDovizCinsi, sth_cins), 
			enum_GenelEvrakTipleri.DepolarArasiSevk => GetIrsaliyeEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sth_evraktip.DepoTransferFisi, AlternatifDovizCinsi, sth_cins), 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => GetDepolarArasiSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira), 
			enum_GenelEvrakTipleri.GelenHavale => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.GelenHavale, AlternatifDovizCinsi), 
			enum_GenelEvrakTipleri.GidenHavale => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.GonderilenHavale, AlternatifDovizCinsi), 
			enum_GenelEvrakTipleri.ProformaSiparis => GetProformaSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sip_tip.Talep, enum_sip_cins.ProformaSiparis), 
			enum_GenelEvrakTipleri.SatisFaturasi => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.SatisFaturasi, AlternatifDovizCinsi), 
			enum_GenelEvrakTipleri.SatisIrsaliyesi => GetIrsaliyeEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sth_evraktip.CikisIrsaliyesi, AlternatifDovizCinsi, sth_cins), 
			enum_GenelEvrakTipleri.Tahsilat => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.TahsilatMakbuzu, AlternatifDovizCinsi), 
			enum_GenelEvrakTipleri.Tediye => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.TediyeMakbuzu, AlternatifDovizCinsi), 
			_ => null, 
		};
	}

	private static Evrak GetGenelEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_cha_evrak_tip cha_evrak_tip, int AlternatifDovizCinsi)
	{
		if (GenelUtility.GetMikroVersiyon(((DbConnection)OpenedConnection).DataSource) > 15)
		{
			return V16_GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, cha_evrak_tip, AlternatifDovizCinsi);
		}
		return V15_GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, cha_evrak_tip, AlternatifDovizCinsi);
	}

	private static Evrak V15_GetGenelEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_cha_evrak_tip cha_evrak_tip, int AlternatifDovizCinsi)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Expected O, but got Unknown
		//IL_0a20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a26: Expected O, but got Unknown
		//IL_0f6c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f72: Expected O, but got Unknown
		bool flag = false;
		Evrak evrak = new Evrak();
		evrak.SetSiparisKarsilamaMi(YeniDeger: false);
		evrak.SetYeniKayit(YeniDeger: false);
		evrak.evraktipi = enum_GenelEvrakTipleri.Tanimsiz;
		List<CARI_HESAP_HAREKETLERI> list = new List<CARI_HESAP_HAREKETLERI>();
		try
		{
			SqliteCommand val = new SqliteCommand("SELECT cha_RECno,cha_evrakno_seri,cha_belge_no,cha_kod,cha_meblag,cha_d_kur,cha_miktari,cha_aratoplam,cha_firmano,cha_subeno,cha_tip,cha_cinsi,cha_normal_Iade,cha_evrak_tip,cha_satir_no,cha_evrakno_sira,cha_cari_cins,cha_vade,cha_lastup_date,cha_tarihi,cha_belge_tarih,cha_ft_iskonto1,cha_ft_iskonto2,cha_ft_iskonto3,cha_ft_iskonto4,cha_ft_iskonto5,cha_ft_iskonto6,cha_ciro_cari_kodu,cha_grupno,cha_d_cins,cha_ticaret_turu,cha_aciklama,cha_projekodu,cha_satici_kodu,cha_srmrkkodu,cha_trefno,cha_ft_masraf1,cha_ft_masraf2,cha_ft_masraf3,cha_ft_masraf4,cha_otvtutari,cha_vergi1,cha_vergi2,cha_vergi3,cha_vergi4,cha_vergi5,cha_vergi6,cha_vergi7,cha_vergi8,cha_vergi9,cha_vergi10,cha_kasa_hizmet,cha_kasa_hizkod,cha_altd_kur,cha_tpoz,cha_vergipntr,cha_EXIMkodu FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip=@cha_evrak_tip AND cha_evrakno_seri=@cha_evrakno_seri AND cha_evrakno_sira=@cha_evrakno_sira");
			val.Parameters.AddWithValue("@cha_evrak_tip", (object)(int)cha_evrak_tip);
			val.Parameters.AddWithValue("@cha_evrakno_seri", (object)EvrakSeri);
			val.Parameters.AddWithValue("@cha_evrakno_sira", (object)EvrakSira);
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				while (((DbDataReader)val2).Read())
				{
					CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI = new CARI_HESAP_HAREKETLERI();
					cARI_HESAP_HAREKETLERI.cha_RECno = val2.GetSafeInt32(0);
					cARI_HESAP_HAREKETLERI.cha_evrakno_seri = val2.GetSafeString(1);
					cARI_HESAP_HAREKETLERI.cha_belge_no = val2.GetSafeString(2);
					cARI_HESAP_HAREKETLERI.cha_kod = val2.GetSafeString(3);
					cARI_HESAP_HAREKETLERI.cha_meblag = val2.GetSafeDouble(4);
					cARI_HESAP_HAREKETLERI.cha_d_kur = val2.GetSafeDouble(5);
					cARI_HESAP_HAREKETLERI.cha_miktari = val2.GetSafeDouble(6);
					cARI_HESAP_HAREKETLERI.cha_aratoplam = val2.GetSafeDouble(7);
					cARI_HESAP_HAREKETLERI.cha_firmano = val2.GetSafeInt32(8);
					cARI_HESAP_HAREKETLERI.cha_subeno = val2.GetSafeInt32(9);
					cARI_HESAP_HAREKETLERI.cha_tip = (enum_cha_tip)val2.GetSafeByte(10);
					cARI_HESAP_HAREKETLERI.cha_cinsi = (enum_cha_cinsi)val2.GetSafeByte(11);
					cARI_HESAP_HAREKETLERI.cha_normal_Iade = (enum_cha_normal_Iade)val2.GetSafeByte(12);
					cARI_HESAP_HAREKETLERI.cha_evrak_tip = (enum_cha_evrak_tip)val2.GetSafeByte(13);
					cARI_HESAP_HAREKETLERI.cha_satir_no = val2.GetSafeInt32(14);
					cARI_HESAP_HAREKETLERI.cha_evrakno_sira = val2.GetSafeInt32(15);
					cARI_HESAP_HAREKETLERI.cha_cari_cins = (enum_cha_cari_cins)val2.GetSafeByte(16);
					cARI_HESAP_HAREKETLERI.cha_vade = val2.GetSafeInt32(17);
					cARI_HESAP_HAREKETLERI.cha_lastup_date = val2.GetSafeDateTime(18);
					cARI_HESAP_HAREKETLERI.cha_tarihi = val2.GetSafeDateTime(19);
					cARI_HESAP_HAREKETLERI.cha_belge_tarih = val2.GetSafeDateTime(20);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto1 = val2.GetSafeDouble(21);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto2 = val2.GetSafeDouble(22);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto3 = val2.GetSafeDouble(23);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto4 = val2.GetSafeDouble(24);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto5 = val2.GetSafeDouble(25);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto6 = val2.GetSafeDouble(26);
					cARI_HESAP_HAREKETLERI.cha_ciro_cari_kodu = val2.GetSafeString(27);
					cARI_HESAP_HAREKETLERI.cha_grupno = val2.GetSafeByte(28);
					cARI_HESAP_HAREKETLERI.cha_d_cins = val2.GetSafeByte(29);
					cARI_HESAP_HAREKETLERI.cha_ticaret_turu = (enum_cha_ticaret_turu)val2.GetSafeByte(30);
					cARI_HESAP_HAREKETLERI.cha_aciklama = val2.GetSafeString(31);
					cARI_HESAP_HAREKETLERI.cha_projekodu = val2.GetSafeString(32);
					cARI_HESAP_HAREKETLERI.cha_satici_kodu = val2.GetSafeString(33);
					cARI_HESAP_HAREKETLERI.cha_srmrkkodu = val2.GetSafeString(34);
					cARI_HESAP_HAREKETLERI.cha_trefno = val2.GetSafeString(35);
					cARI_HESAP_HAREKETLERI.cha_ft_masraf1 = val2.GetSafeDouble(36);
					cARI_HESAP_HAREKETLERI.cha_ft_masraf2 = val2.GetSafeDouble(37);
					cARI_HESAP_HAREKETLERI.cha_ft_masraf3 = val2.GetSafeDouble(38);
					cARI_HESAP_HAREKETLERI.cha_ft_masraf4 = val2.GetSafeDouble(39);
					cARI_HESAP_HAREKETLERI.cha_otvtutari = val2.GetSafeDouble(40);
					cARI_HESAP_HAREKETLERI.cha_vergi1 = val2.GetSafeDouble(41);
					cARI_HESAP_HAREKETLERI.cha_vergi2 = val2.GetSafeDouble(42);
					cARI_HESAP_HAREKETLERI.cha_vergi3 = val2.GetSafeDouble(43);
					cARI_HESAP_HAREKETLERI.cha_vergi4 = val2.GetSafeDouble(44);
					cARI_HESAP_HAREKETLERI.cha_vergi5 = val2.GetSafeDouble(45);
					cARI_HESAP_HAREKETLERI.cha_vergi6 = val2.GetSafeDouble(46);
					cARI_HESAP_HAREKETLERI.cha_vergi7 = val2.GetSafeDouble(47);
					cARI_HESAP_HAREKETLERI.cha_vergi8 = val2.GetSafeDouble(48);
					cARI_HESAP_HAREKETLERI.cha_vergi9 = val2.GetSafeDouble(49);
					cARI_HESAP_HAREKETLERI.cha_vergi10 = val2.GetSafeDouble(50);
					cARI_HESAP_HAREKETLERI.cha_kasa_hizmet = (enum_cha_kasa_hizmet)val2.GetSafeByte(51);
					cARI_HESAP_HAREKETLERI.cha_kasa_hizkod = val2.GetSafeString(52);
					cARI_HESAP_HAREKETLERI.cha_altd_kur = val2.GetSafeDouble(53);
					cARI_HESAP_HAREKETLERI.cha_tpoz = (enum_cha_tpoz)val2.GetSafeByte(54);
					cARI_HESAP_HAREKETLERI.cha_vergipntr = val2.GetSafeByte(55);
					cARI_HESAP_HAREKETLERI.cha_EXIMkodu = val2.GetSafeString(56);
					switch (cARI_HESAP_HAREKETLERI.cha_cinsi)
					{
					case enum_cha_cinsi.HizmetFaturasi:
						evrak.AddHizmetHareketi(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.MusteriCeki:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.MusteriHavaleSozu:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.MusteriKrediKarti:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.MusteriOdemeSozu:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.MusteriSenedi:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.Nakit:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.ToptanFatura:
						evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
						list.Add(cARI_HESAP_HAREKETLERI);
						evrak.ticaretturu = enum_cha_ticaret_turu.ToptanYurtIciTicaret;
						flag = true;
						break;
					case enum_cha_cinsi.PerakendeFaturasi:
						evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
						list.Add(cARI_HESAP_HAREKETLERI);
						evrak.ticaretturu = enum_cha_ticaret_turu.PerakendeYurtIciTicaret;
						flag = true;
						break;
					case enum_cha_cinsi.GumrukBeyannamesi:
						evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
						list.Add(cARI_HESAP_HAREKETLERI);
						flag = true;
						break;
					}
				}
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
			foreach (CARI_HESAP_HAREKETLERI item in evrak.GetTahsilatHareketleri())
			{
				if (item.cha_cinsi != enum_cha_cinsi.MusteriCeki)
				{
					continue;
				}
				SqliteCommand val3 = new SqliteCommand("SELECT sck_banka_adres1,sck_bankano,sck_borclu,sck_hesapno_sehir,sck_no,sck_sube_adres2,Sck_TCMB_Banka_kodu,Sck_TCMB_il_kodu,Sck_TCMB_Sube_kodu,sck_vdaire_no FROM ODEME_EMIRLERI WHERE sck_tip=0 AND sck_ilk_evrak_seri=@sck_ilk_evrak_seri AND sck_ilk_evrak_sira_no=@sck_ilk_evrak_sira_no AND sck_ilk_evrak_satir_no=@sck_ilk_evrak_satir_no");
				val3.Parameters.AddWithValue("@sck_ilk_evrak_seri", (object)EvrakSeri);
				val3.Parameters.AddWithValue("@sck_ilk_evrak_sira_no", (object)EvrakSira);
				val3.Parameters.AddWithValue("@sck_ilk_evrak_satir_no", (object)item.cha_satir_no);
				val3.Connection = OpenedConnection;
				SqliteDataReader val4 = val3.ExecuteReader();
				if (((DbDataReader)val4).HasRows)
				{
					while (((DbDataReader)val4).Read())
					{
						item.sck_banka_adres1 = val4.GetSafeString(0);
						item.sck_bankano = val4.GetSafeString(1);
						item.sck_borclu = val4.GetSafeString(2);
						item.sck_hesapno_sehir = val4.GetSafeString(3);
						item.sck_no = val4.GetSafeString(4);
						item.sck_sube_adres2 = val4.GetSafeString(5);
						item.Sck_TCMB_Banka_kodu = val4.GetSafeString(6);
						item.Sck_TCMB_il_kodu = val4.GetSafeString(7);
						item.Sck_TCMB_Sube_kodu = val4.GetSafeString(8);
						item.sck_vdaire_no = val4.GetSafeString(9);
					}
				}
				((DbDataReader)val4).Close();
				((DbDataReader)val4).Dispose();
				val4 = null;
				((Component)val3).Dispose();
				val3 = null;
			}
			CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI2 = null;
			if (evrak.GetTahsilatHareketleri().Count > 0)
			{
				cARI_HESAP_HAREKETLERI2 = evrak.GetTahsilatHareketleri()[0];
			}
			if (evrak.GetHizmetHareketleri().Count > 0)
			{
				cARI_HESAP_HAREKETLERI2 = evrak.GetHizmetHareketleri()[0];
			}
			if (flag)
			{
				cARI_HESAP_HAREKETLERI2 = evrak.GetStokCariHesapHareketi();
			}
			if (cARI_HESAP_HAREKETLERI2 != null)
			{
				if (cARI_HESAP_HAREKETLERI2.cha_tpoz == enum_cha_tpoz.Acik)
				{
					evrak.kapamasekli = enum_KapamaSekli.AcikHesap;
				}
				else
				{
					switch (cARI_HESAP_HAREKETLERI2.cha_cari_cins)
					{
					case enum_cha_cari_cins.Bankamiz:
						evrak.kapamasekli = enum_KapamaSekli.BankadanKapanacak;
						break;
					case enum_cha_cari_cins.CariPersonelimiz:
						evrak.kapamasekli = enum_KapamaSekli.CariPersoneldenKapanacak;
						break;
					case enum_cha_cari_cins.Kasamiz:
						evrak.kapamasekli = enum_KapamaSekli.BankadanKapanacak;
						break;
					}
				}
				evrak.normaliade = cARI_HESAP_HAREKETLERI2.cha_normal_Iade;
				bool flag2 = false;
				switch (cARI_HESAP_HAREKETLERI2.cha_cinsi)
				{
				case enum_cha_cinsi.HizmetFaturasi:
					flag2 = true;
					break;
				case enum_cha_cinsi.MusteriCeki:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.MusteriHavaleSozu:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.MusteriKrediKarti:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.MusteriOdemeSozu:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.MusteriSenedi:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.Nakit:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.ToptanFatura:
					flag2 = true;
					break;
				case enum_cha_cinsi.GumrukBeyannamesi:
					flag2 = true;
					break;
				}
				if (flag2)
				{
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Borc)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.SatisFaturasi;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.AlisFaturasi;
					}
				}
				evrak.SetAciklama(cARI_HESAP_HAREKETLERI2.cha_aciklama);
				evrak.SetAlternatifDovizCinsi(AlternatifDovizCinsi);
				evrak.SetKapamaHesapKodu(cARI_HESAP_HAREKETLERI2.cha_kasa_hizkod);
				evrak.SetDovizCinsi(cARI_HESAP_HAREKETLERI2.cha_d_cins);
				evrak.kur = new Kur();
				evrak.kur.dov_fiyat = cARI_HESAP_HAREKETLERI2.cha_d_kur;
				evrak.kur.dov_no = cARI_HESAP_HAREKETLERI2.cha_d_cins;
				evrak.kur.dov_tarih = cARI_HESAP_HAREKETLERI2.cha_tarihi;
				evrak.SetProje(ProjeSqlite.GetProje(OpenedConnection, cARI_HESAP_HAREKETLERI2.cha_projekodu));
				evrak.SetSorumlulukMerkezi(SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(OpenedConnection, cARI_HESAP_HAREKETLERI2.cha_srmrkkodu));
				evrak.SetTemsilciKodu(cARI_HESAP_HAREKETLERI2.cha_satici_kodu);
				evrak.alternatifdovizkuru = new Kur();
				evrak.alternatifdovizkuru.dov_fiyat = cARI_HESAP_HAREKETLERI2.cha_altd_kur;
				evrak.alternatifdovizkuru.dov_no = AlternatifDovizCinsi;
				evrak.SetBelgeNo(cARI_HESAP_HAREKETLERI2.cha_belge_no);
				evrak.SetBelgeTarihi(cARI_HESAP_HAREKETLERI2.cha_belge_tarih);
				evrak.cari = CariSqlite.GetCariByCariKod(OpenedConnection, cARI_HESAP_HAREKETLERI2.cha_kod, AdreslerTemsilciyeGore: false, "");
				evrak.SetEvrakKilitli(cARI_HESAP_HAREKETLERI2.cha_kilitli);
				evrak.SetEvrakNoSeri(cARI_HESAP_HAREKETLERI2.cha_evrakno_seri);
				evrak.SetEvraknoSira(cARI_HESAP_HAREKETLERI2.cha_evrakno_sira);
				evrak.SetEvrakTarihi(cARI_HESAP_HAREKETLERI2.cha_tarihi);
				evrak.SetFirma(FirmaSqlite.GetFirma(OpenedConnection, cARI_HESAP_HAREKETLERI2.cha_firmano));
				evrak.SetSube(SubeSqlite.GetSube(OpenedConnection, cARI_HESAP_HAREKETLERI2.cha_subeno));
				evrak.SetMikroUserNo(cARI_HESAP_HAREKETLERI2.cha_create_user);
				evrak.SetOdemePlani(cARI_HESAP_HAREKETLERI2.cha_vade);
			}
			if (flag)
			{
				foreach (CARI_HESAP_HAREKETLERI item2 in list)
				{
					val = new SqliteCommand("SELECT STOK_HAREKETLERI.sth_RECno,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,STOK_HAREKETLERI.sth_sip_recid_dbcno,STOK_HAREKETLERI.sth_sip_recid_recno,STOK_HAREKETLERI.sth_fat_recid_dbcno,STOK_HAREKETLERI.sth_fat_recid_recno,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6,sth_aciklama,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM STOK_HAREKETLERI INNER JOIN STOKLAR ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod WHERE sth_fat_recid_recno=@sth_fat_recid_recno");
					val.Parameters.AddWithValue("@sth_fat_recid_recno", (object)item2.cha_RECno);
					val.Connection = OpenedConnection;
					val2 = val.ExecuteReader();
					if (((DbDataReader)val2).HasRows)
					{
						while (((DbDataReader)val2).Read())
						{
							STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
							sTOK_HAREKETLERI.sth_RECno = val2.GetSafeInt32(0);
							sTOK_HAREKETLERI.sth_cari_kodu = val2.GetSafeString(1);
							sTOK_HAREKETLERI.sth_stok_kod = val2.GetSafeString(2);
							sTOK_HAREKETLERI.sth_evrakno_seri = val2.GetSafeString(3);
							sTOK_HAREKETLERI.sth_evrakno_sira = val2.GetSafeInt32(4);
							sTOK_HAREKETLERI.sth_plasiyer_kodu = val2.GetSafeString(5);
							sTOK_HAREKETLERI.sth_miktar = val2.GetSafeDouble(6);
							sTOK_HAREKETLERI.sth_miktar2 = val2.GetSafeDouble(7);
							sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)val2.GetSafeByte(8);
							sTOK_HAREKETLERI.sth_giris_depo_no = val2.GetSafeInt32(9);
							sTOK_HAREKETLERI.sth_cikis_depo_no = val2.GetSafeInt32(10);
							sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)val2.GetSafeByte(11);
							sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)val2.GetSafeByte(12);
							sTOK_HAREKETLERI.sth_satirno = val2.GetSafeInt32(13);
							sTOK_HAREKETLERI.sth_sip_recid_dbcno = val2.GetSafeInt16(14);
							sTOK_HAREKETLERI.sth_sip_recid_recno = val2.GetSafeInt32(15);
							sTOK_HAREKETLERI.sth_fat_recid_dbcno = val2.GetSafeInt16(16);
							sTOK_HAREKETLERI.sth_fat_recid_recno = val2.GetSafeInt32(17);
							sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)val2.GetSafeByte(18);
							sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)val2.GetSafeByte(19);
							sTOK_HAREKETLERI.sth_lastup_date = val2.GetSafeDateTime(20);
							sTOK_HAREKETLERI.sth_tarih = val2.GetSafeDateTime(21);
							sTOK_HAREKETLERI.sth_belge_tarih = val2.GetSafeDateTime(22);
							sTOK_HAREKETLERI.sth_tutar = val2.GetSafeDouble(23);
							sTOK_HAREKETLERI.sth_vergi = val2.GetSafeDouble(24);
							sTOK_HAREKETLERI.sth_har_doviz_kuru = val2.GetSafeDouble(25);
							sTOK_HAREKETLERI.sth_iskonto1 = val2.GetSafeDouble(26);
							sTOK_HAREKETLERI.sth_iskonto2 = val2.GetSafeDouble(27);
							sTOK_HAREKETLERI.sth_iskonto3 = val2.GetSafeDouble(28);
							sTOK_HAREKETLERI.sth_iskonto4 = val2.GetSafeDouble(29);
							sTOK_HAREKETLERI.sth_iskonto5 = val2.GetSafeDouble(30);
							sTOK_HAREKETLERI.sth_iskonto6 = val2.GetSafeDouble(31);
							sTOK_HAREKETLERI.sth_masraf1 = val2.GetSafeDouble(32);
							sTOK_HAREKETLERI.sth_masraf2 = val2.GetSafeDouble(33);
							sTOK_HAREKETLERI.sth_masraf3 = val2.GetSafeDouble(34);
							sTOK_HAREKETLERI.sth_masraf4 = val2.GetSafeDouble(35);
							sTOK_HAREKETLERI.sth_masraf_vergi = val2.GetSafeDouble(36);
							sTOK_HAREKETLERI.sth_vergi_pntr = val2.GetSafeByte(37);
							sTOK_HAREKETLERI.sth_har_doviz_cinsi = val2.GetSafeByte(38);
							sTOK_HAREKETLERI.sth_alt_doviz_kuru = val2.GetSafeDouble(39);
							sTOK_HAREKETLERI.sth_stok_doviz_cinsi = val2.GetSafeByte(40);
							sTOK_HAREKETLERI.sth_stok_doviz_kuru = val2.GetSafeDouble(41);
							sTOK_HAREKETLERI.sth_birim_pntr = val2.GetSafeByte(42);
							sTOK_HAREKETLERI.sth_fiyat_liste_no = val2.GetSafeInt32(43);
							sTOK_HAREKETLERI.sth_adres_no = val2.GetSafeInt32(44);
							sTOK_HAREKETLERI.sto_isim = val2.GetSafeString(45);
							sTOK_HAREKETLERI.sto_birim1_ad = val2.GetSafeString(46);
							sTOK_HAREKETLERI.sto_birim2_ad = val2.GetSafeString(47);
							sTOK_HAREKETLERI.sto_birim3_ad = val2.GetSafeString(48);
							sTOK_HAREKETLERI.sto_birim4_ad = val2.GetSafeString(49);
							sTOK_HAREKETLERI.sto_birim1_katsayi = val2.GetSafeDouble(50);
							sTOK_HAREKETLERI.sto_birim2_katsayi = val2.GetSafeDouble(51);
							sTOK_HAREKETLERI.sto_birim3_katsayi = val2.GetSafeDouble(52);
							sTOK_HAREKETLERI.sto_birim4_katsayi = val2.GetSafeDouble(53);
							sTOK_HAREKETLERI.sth_isk_mas1 = val2.GetSafeByte(54);
							sTOK_HAREKETLERI.sth_isk_mas2 = val2.GetSafeByte(55);
							sTOK_HAREKETLERI.sth_isk_mas3 = val2.GetSafeByte(56);
							sTOK_HAREKETLERI.sth_isk_mas4 = val2.GetSafeByte(57);
							sTOK_HAREKETLERI.sth_isk_mas5 = val2.GetSafeByte(58);
							sTOK_HAREKETLERI.sth_isk_mas6 = val2.GetSafeByte(59);
							sTOK_HAREKETLERI.sth_aciklama = val2.GetSafeString(60);
							bool safeBoolean = val2.GetSafeBoolean(61);
							bool safeBoolean2 = val2.GetSafeBoolean(62);
							if (safeBoolean || safeBoolean2)
							{
								sTOK_HAREKETLERI.renk_beden_hareketleri = StokSqlite.V15_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.StokHareket, sTOK_HAREKETLERI.sth_RECid_DBCno, sTOK_HAREKETLERI.sth_RECid_RECno);
							}
							evrak.AddStokHareketi(sTOK_HAREKETLERI);
						}
					}
					((DbDataReader)val2).Close();
					((DbDataReader)val2).Dispose();
					val2 = null;
					((Component)val).Dispose();
					val = null;
				}
			}
			foreach (STOK_HAREKETLERI item3 in evrak.GetStokHareketleri())
			{
				if (item3.sth_sip_recid_recno != 0)
				{
					evrak.SetSiparisKarsilamaMi(YeniDeger: true);
					evrak.AddSiparisHareketi(V15_GetSiparisKalem(OpenedConnection, item3.sth_sip_recid_recno), SiparisKarsilamaYap: false);
				}
			}
			if (evrak.GetStokHareketleri().Count > 0)
			{
				STOK_HAREKETLERI sTOK_HAREKETLERI2 = evrak.GetStokHareketleri()[0];
				evrak.SetFiyatListesi(FiyatListesiSqlite.GetFiyatListesi(OpenedConnection, sTOK_HAREKETLERI2.sth_fiyat_liste_no));
				evrak.SetHedefDepo(DepoSqlite.GetDepo(OpenedConnection, sTOK_HAREKETLERI2.sth_giris_depo_no));
				evrak.SetKaynakDepo(DepoSqlite.GetDepo(OpenedConnection, sTOK_HAREKETLERI2.sth_cikis_depo_no));
				evrak.SetSevkAdresNo(sTOK_HAREKETLERI2.sth_adres_no);
			}
			val = new SqliteCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano=51 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
			if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
			{
				val.Parameters.AddWithValue("@egk_hareket_tip", (object)evrak.GetStokCariHesapHareketi().cha_tip);
				val.Parameters.AddWithValue("@egk_evr_tip", (object)evrak.GetStokCariHesapHareketi().cha_evrak_tip);
				val.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.GetStokCariHesapHareketi().cha_evrakno_seri);
				val.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.GetStokCariHesapHareketi().cha_evrakno_sira);
			}
			else
			{
				val.Parameters.AddWithValue("@egk_hareket_tip", (object)evrak.GetTahsilatHareketleri()[0].cha_tip);
				val.Parameters.AddWithValue("@egk_evr_tip", (object)evrak.GetTahsilatHareketleri()[0].cha_evrak_tip);
				val.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.GetTahsilatHareketleri()[0].cha_evrakno_seri);
				val.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.GetTahsilatHareketleri()[0].cha_evrakno_sira);
			}
			val.Connection = OpenedConnection;
			val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				while (((DbDataReader)val2).Read())
				{
					evrak.SetAciklama1(val2.GetSafeString(0));
					evrak.SetAciklama2(val2.GetSafeString(1));
					evrak.SetAciklama3(val2.GetSafeString(2));
					evrak.SetAciklama4(val2.GetSafeString(3));
					evrak.SetAciklama5(val2.GetSafeString(4));
					evrak.SetAciklama6(val2.GetSafeString(5));
					evrak.SetAciklama7(val2.GetSafeString(6));
					evrak.SetAciklama8(val2.GetSafeString(7));
					evrak.SetAciklama9(val2.GetSafeString(8));
					evrak.SetAciklama10(val2.GetSafeString(9));
				}
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return evrak;
	}

	private static Evrak V16_GetGenelEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_cha_evrak_tip cha_evrak_tip, int AlternatifDovizCinsi)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Expected O, but got Unknown
		//IL_0a20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a26: Expected O, but got Unknown
		//IL_0f4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f55: Expected O, but got Unknown
		bool flag = false;
		Evrak evrak = new Evrak();
		evrak.SetSiparisKarsilamaMi(YeniDeger: false);
		evrak.SetYeniKayit(YeniDeger: false);
		evrak.evraktipi = enum_GenelEvrakTipleri.Tanimsiz;
		List<CARI_HESAP_HAREKETLERI> list = new List<CARI_HESAP_HAREKETLERI>();
		try
		{
			SqliteCommand val = new SqliteCommand("SELECT cha_Guid,cha_evrakno_seri,cha_belge_no,cha_kod,cha_meblag,cha_d_kur,cha_miktari,cha_aratoplam,cha_firmano,cha_subeno,cha_tip,cha_cinsi,cha_normal_Iade,cha_evrak_tip,cha_satir_no,cha_evrakno_sira,cha_cari_cins,cha_vade,cha_lastup_date,cha_tarihi,cha_belge_tarih,cha_ft_iskonto1,cha_ft_iskonto2,cha_ft_iskonto3,cha_ft_iskonto4,cha_ft_iskonto5,cha_ft_iskonto6,cha_ciro_cari_kodu,cha_grupno,cha_d_cins,cha_ticaret_turu,cha_aciklama,cha_projekodu,cha_satici_kodu,cha_srmrkkodu,cha_trefno,cha_ft_masraf1,cha_ft_masraf2,cha_ft_masraf3,cha_ft_masraf4,cha_otvtutari,cha_vergi1,cha_vergi2,cha_vergi3,cha_vergi4,cha_vergi5,cha_vergi6,cha_vergi7,cha_vergi8,cha_vergi9,cha_vergi10,cha_kasa_hizmet,cha_kasa_hizkod,cha_altd_kur,cha_tpoz,cha_vergipntr,cha_EXIMkodu FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip=@cha_evrak_tip AND cha_evrakno_seri=@cha_evrakno_seri AND cha_evrakno_sira=@cha_evrakno_sira");
			val.Parameters.AddWithValue("@cha_evrak_tip", (object)(int)cha_evrak_tip);
			val.Parameters.AddWithValue("@cha_evrakno_seri", (object)EvrakSeri);
			val.Parameters.AddWithValue("@cha_evrakno_sira", (object)EvrakSira);
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				while (((DbDataReader)val2).Read())
				{
					CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI = new CARI_HESAP_HAREKETLERI();
					cARI_HESAP_HAREKETLERI.cha_Guid = ((DbDataReader)val2).GetGuid(0);
					cARI_HESAP_HAREKETLERI.cha_evrakno_seri = val2.GetSafeString(1);
					cARI_HESAP_HAREKETLERI.cha_belge_no = val2.GetSafeString(2);
					cARI_HESAP_HAREKETLERI.cha_kod = val2.GetSafeString(3);
					cARI_HESAP_HAREKETLERI.cha_meblag = val2.GetSafeDouble(4);
					cARI_HESAP_HAREKETLERI.cha_d_kur = val2.GetSafeDouble(5);
					cARI_HESAP_HAREKETLERI.cha_miktari = val2.GetSafeDouble(6);
					cARI_HESAP_HAREKETLERI.cha_aratoplam = val2.GetSafeDouble(7);
					cARI_HESAP_HAREKETLERI.cha_firmano = val2.GetSafeInt32(8);
					cARI_HESAP_HAREKETLERI.cha_subeno = val2.GetSafeInt32(9);
					cARI_HESAP_HAREKETLERI.cha_tip = (enum_cha_tip)val2.GetSafeByte(10);
					cARI_HESAP_HAREKETLERI.cha_cinsi = (enum_cha_cinsi)val2.GetSafeByte(11);
					cARI_HESAP_HAREKETLERI.cha_normal_Iade = (enum_cha_normal_Iade)val2.GetSafeByte(12);
					cARI_HESAP_HAREKETLERI.cha_evrak_tip = (enum_cha_evrak_tip)val2.GetSafeByte(13);
					cARI_HESAP_HAREKETLERI.cha_satir_no = val2.GetSafeInt32(14);
					cARI_HESAP_HAREKETLERI.cha_evrakno_sira = val2.GetSafeInt32(15);
					cARI_HESAP_HAREKETLERI.cha_cari_cins = (enum_cha_cari_cins)val2.GetSafeByte(16);
					cARI_HESAP_HAREKETLERI.cha_vade = val2.GetSafeInt32(17);
					cARI_HESAP_HAREKETLERI.cha_lastup_date = val2.GetSafeDateTime(18);
					cARI_HESAP_HAREKETLERI.cha_tarihi = val2.GetSafeDateTime(19);
					cARI_HESAP_HAREKETLERI.cha_belge_tarih = val2.GetSafeDateTime(20);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto1 = val2.GetSafeDouble(21);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto2 = val2.GetSafeDouble(22);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto3 = val2.GetSafeDouble(23);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto4 = val2.GetSafeDouble(24);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto5 = val2.GetSafeDouble(25);
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto6 = val2.GetSafeDouble(26);
					cARI_HESAP_HAREKETLERI.cha_ciro_cari_kodu = val2.GetSafeString(27);
					cARI_HESAP_HAREKETLERI.cha_grupno = val2.GetSafeByte(28);
					cARI_HESAP_HAREKETLERI.cha_d_cins = val2.GetSafeByte(29);
					cARI_HESAP_HAREKETLERI.cha_ticaret_turu = (enum_cha_ticaret_turu)val2.GetSafeByte(30);
					cARI_HESAP_HAREKETLERI.cha_aciklama = val2.GetSafeString(31);
					cARI_HESAP_HAREKETLERI.cha_projekodu = val2.GetSafeString(32);
					cARI_HESAP_HAREKETLERI.cha_satici_kodu = val2.GetSafeString(33);
					cARI_HESAP_HAREKETLERI.cha_srmrkkodu = val2.GetSafeString(34);
					cARI_HESAP_HAREKETLERI.cha_trefno = val2.GetSafeString(35);
					cARI_HESAP_HAREKETLERI.cha_ft_masraf1 = val2.GetSafeDouble(36);
					cARI_HESAP_HAREKETLERI.cha_ft_masraf2 = val2.GetSafeDouble(37);
					cARI_HESAP_HAREKETLERI.cha_ft_masraf3 = val2.GetSafeDouble(38);
					cARI_HESAP_HAREKETLERI.cha_ft_masraf4 = val2.GetSafeDouble(39);
					cARI_HESAP_HAREKETLERI.cha_otvtutari = val2.GetSafeDouble(40);
					cARI_HESAP_HAREKETLERI.cha_vergi1 = val2.GetSafeDouble(41);
					cARI_HESAP_HAREKETLERI.cha_vergi2 = val2.GetSafeDouble(42);
					cARI_HESAP_HAREKETLERI.cha_vergi3 = val2.GetSafeDouble(43);
					cARI_HESAP_HAREKETLERI.cha_vergi4 = val2.GetSafeDouble(44);
					cARI_HESAP_HAREKETLERI.cha_vergi5 = val2.GetSafeDouble(45);
					cARI_HESAP_HAREKETLERI.cha_vergi6 = val2.GetSafeDouble(46);
					cARI_HESAP_HAREKETLERI.cha_vergi7 = val2.GetSafeDouble(47);
					cARI_HESAP_HAREKETLERI.cha_vergi8 = val2.GetSafeDouble(48);
					cARI_HESAP_HAREKETLERI.cha_vergi9 = val2.GetSafeDouble(49);
					cARI_HESAP_HAREKETLERI.cha_vergi10 = val2.GetSafeDouble(50);
					cARI_HESAP_HAREKETLERI.cha_kasa_hizmet = (enum_cha_kasa_hizmet)val2.GetSafeByte(51);
					cARI_HESAP_HAREKETLERI.cha_kasa_hizkod = val2.GetSafeString(52);
					cARI_HESAP_HAREKETLERI.cha_altd_kur = val2.GetSafeDouble(53);
					cARI_HESAP_HAREKETLERI.cha_tpoz = (enum_cha_tpoz)val2.GetSafeByte(54);
					cARI_HESAP_HAREKETLERI.cha_vergipntr = val2.GetSafeByte(55);
					cARI_HESAP_HAREKETLERI.cha_EXIMkodu = val2.GetSafeString(56);
					switch (cARI_HESAP_HAREKETLERI.cha_cinsi)
					{
					case enum_cha_cinsi.HizmetFaturasi:
						evrak.AddHizmetHareketi(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.MusteriCeki:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.MusteriHavaleSozu:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.MusteriKrediKarti:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.MusteriOdemeSozu:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.MusteriSenedi:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.Nakit:
						evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
						break;
					case enum_cha_cinsi.ToptanFatura:
						evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
						list.Add(cARI_HESAP_HAREKETLERI);
						evrak.ticaretturu = enum_cha_ticaret_turu.ToptanYurtIciTicaret;
						flag = true;
						break;
					case enum_cha_cinsi.PerakendeFaturasi:
						evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
						list.Add(cARI_HESAP_HAREKETLERI);
						evrak.ticaretturu = enum_cha_ticaret_turu.PerakendeYurtIciTicaret;
						flag = true;
						break;
					case enum_cha_cinsi.GumrukBeyannamesi:
						evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
						list.Add(cARI_HESAP_HAREKETLERI);
						flag = true;
						break;
					}
				}
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
			foreach (CARI_HESAP_HAREKETLERI item in evrak.GetTahsilatHareketleri())
			{
				if (item.cha_cinsi != enum_cha_cinsi.MusteriCeki)
				{
					continue;
				}
				SqliteCommand val3 = new SqliteCommand("SELECT sck_banka_adres1,sck_bankano,sck_borclu,sck_hesapno_sehir,sck_no,sck_sube_adres2,Sck_TCMB_Banka_kodu,Sck_TCMB_il_kodu,Sck_TCMB_Sube_kodu,sck_vdaire_no FROM ODEME_EMIRLERI WHERE sck_tip=0 AND sck_ilk_evrak_seri=@sck_ilk_evrak_seri AND sck_ilk_evrak_sira_no=@sck_ilk_evrak_sira_no AND sck_ilk_evrak_satir_no=@sck_ilk_evrak_satir_no");
				val3.Parameters.AddWithValue("@sck_ilk_evrak_seri", (object)EvrakSeri);
				val3.Parameters.AddWithValue("@sck_ilk_evrak_sira_no", (object)EvrakSira);
				val3.Parameters.AddWithValue("@sck_ilk_evrak_satir_no", (object)item.cha_satir_no);
				val3.Connection = OpenedConnection;
				SqliteDataReader val4 = val3.ExecuteReader();
				if (((DbDataReader)val4).HasRows)
				{
					while (((DbDataReader)val4).Read())
					{
						item.sck_banka_adres1 = val4.GetSafeString(0);
						item.sck_bankano = val4.GetSafeString(1);
						item.sck_borclu = val4.GetSafeString(2);
						item.sck_hesapno_sehir = val4.GetSafeString(3);
						item.sck_no = val4.GetSafeString(4);
						item.sck_sube_adres2 = val4.GetSafeString(5);
						item.Sck_TCMB_Banka_kodu = val4.GetSafeString(6);
						item.Sck_TCMB_il_kodu = val4.GetSafeString(7);
						item.Sck_TCMB_Sube_kodu = val4.GetSafeString(8);
						item.sck_vdaire_no = val4.GetSafeString(9);
					}
				}
				((DbDataReader)val4).Close();
				((DbDataReader)val4).Dispose();
				val4 = null;
				((Component)val3).Dispose();
				val3 = null;
			}
			CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI2 = null;
			if (evrak.GetTahsilatHareketleri().Count > 0)
			{
				cARI_HESAP_HAREKETLERI2 = evrak.GetTahsilatHareketleri()[0];
			}
			if (evrak.GetHizmetHareketleri().Count > 0)
			{
				cARI_HESAP_HAREKETLERI2 = evrak.GetHizmetHareketleri()[0];
			}
			if (flag)
			{
				cARI_HESAP_HAREKETLERI2 = evrak.GetStokCariHesapHareketi();
			}
			if (cARI_HESAP_HAREKETLERI2 != null)
			{
				if (cARI_HESAP_HAREKETLERI2.cha_tpoz == enum_cha_tpoz.Acik)
				{
					evrak.kapamasekli = enum_KapamaSekli.AcikHesap;
				}
				else
				{
					switch (cARI_HESAP_HAREKETLERI2.cha_cari_cins)
					{
					case enum_cha_cari_cins.Bankamiz:
						evrak.kapamasekli = enum_KapamaSekli.BankadanKapanacak;
						break;
					case enum_cha_cari_cins.CariPersonelimiz:
						evrak.kapamasekli = enum_KapamaSekli.CariPersoneldenKapanacak;
						break;
					case enum_cha_cari_cins.Kasamiz:
						evrak.kapamasekli = enum_KapamaSekli.BankadanKapanacak;
						break;
					}
				}
				evrak.normaliade = cARI_HESAP_HAREKETLERI2.cha_normal_Iade;
				bool flag2 = false;
				switch (cARI_HESAP_HAREKETLERI2.cha_cinsi)
				{
				case enum_cha_cinsi.HizmetFaturasi:
					flag2 = true;
					break;
				case enum_cha_cinsi.MusteriCeki:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.MusteriHavaleSozu:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.MusteriKrediKarti:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.MusteriOdemeSozu:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.MusteriSenedi:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.Nakit:
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
					}
					break;
				case enum_cha_cinsi.ToptanFatura:
					flag2 = true;
					break;
				case enum_cha_cinsi.GumrukBeyannamesi:
					flag2 = true;
					break;
				}
				if (flag2)
				{
					if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Borc)
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.SatisFaturasi;
					}
					else
					{
						evrak.evraktipi = enum_GenelEvrakTipleri.AlisFaturasi;
					}
				}
				evrak.SetAciklama(cARI_HESAP_HAREKETLERI2.cha_aciklama);
				evrak.SetAlternatifDovizCinsi(AlternatifDovizCinsi);
				evrak.SetKapamaHesapKodu(cARI_HESAP_HAREKETLERI2.cha_kasa_hizkod);
				evrak.SetDovizCinsi(cARI_HESAP_HAREKETLERI2.cha_d_cins);
				evrak.kur = new Kur();
				evrak.kur.dov_fiyat = cARI_HESAP_HAREKETLERI2.cha_d_kur;
				evrak.kur.dov_no = cARI_HESAP_HAREKETLERI2.cha_d_cins;
				evrak.kur.dov_tarih = cARI_HESAP_HAREKETLERI2.cha_tarihi;
				evrak.SetProje(ProjeSqlite.GetProje(OpenedConnection, cARI_HESAP_HAREKETLERI2.cha_projekodu));
				evrak.SetSorumlulukMerkezi(SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(OpenedConnection, cARI_HESAP_HAREKETLERI2.cha_srmrkkodu));
				evrak.SetTemsilciKodu(cARI_HESAP_HAREKETLERI2.cha_satici_kodu);
				evrak.alternatifdovizkuru = new Kur();
				evrak.alternatifdovizkuru.dov_fiyat = cARI_HESAP_HAREKETLERI2.cha_altd_kur;
				evrak.alternatifdovizkuru.dov_no = AlternatifDovizCinsi;
				evrak.SetBelgeNo(cARI_HESAP_HAREKETLERI2.cha_belge_no);
				evrak.SetBelgeTarihi(cARI_HESAP_HAREKETLERI2.cha_belge_tarih);
				evrak.cari = CariSqlite.GetCariByCariKod(OpenedConnection, cARI_HESAP_HAREKETLERI2.cha_kod, AdreslerTemsilciyeGore: false, "");
				evrak.SetEvrakKilitli(cARI_HESAP_HAREKETLERI2.cha_kilitli);
				evrak.SetEvrakNoSeri(cARI_HESAP_HAREKETLERI2.cha_evrakno_seri);
				evrak.SetEvraknoSira(cARI_HESAP_HAREKETLERI2.cha_evrakno_sira);
				evrak.SetEvrakTarihi(cARI_HESAP_HAREKETLERI2.cha_tarihi);
				evrak.SetFirma(FirmaSqlite.GetFirma(OpenedConnection, cARI_HESAP_HAREKETLERI2.cha_firmano));
				evrak.SetSube(SubeSqlite.GetSube(OpenedConnection, cARI_HESAP_HAREKETLERI2.cha_subeno));
				evrak.SetMikroUserNo(cARI_HESAP_HAREKETLERI2.cha_create_user);
				evrak.SetOdemePlani(cARI_HESAP_HAREKETLERI2.cha_vade);
			}
			if (flag)
			{
				foreach (CARI_HESAP_HAREKETLERI item2 in list)
				{
					val = new SqliteCommand("SELECT STOK_HAREKETLERI.sth_Guid,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,0,STOK_HAREKETLERI.sth_sip_uid,0,STOK_HAREKETLERI.sth_fat_uid,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6,sth_aciklama,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM STOK_HAREKETLERI INNER JOIN STOKLAR ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod WHERE sth_fat_uid=@sth_fat_uid");
					val.Parameters.AddWithValue("@sth_fat_uid", (object)item2.cha_Guid);
					val.Connection = OpenedConnection;
					val2 = val.ExecuteReader();
					if (((DbDataReader)val2).HasRows)
					{
						while (((DbDataReader)val2).Read())
						{
							STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
							sTOK_HAREKETLERI.sth_Guid = ((DbDataReader)val2).GetGuid(0);
							sTOK_HAREKETLERI.sth_cari_kodu = val2.GetSafeString(1);
							sTOK_HAREKETLERI.sth_stok_kod = val2.GetSafeString(2);
							sTOK_HAREKETLERI.sth_evrakno_seri = val2.GetSafeString(3);
							sTOK_HAREKETLERI.sth_evrakno_sira = val2.GetSafeInt32(4);
							sTOK_HAREKETLERI.sth_plasiyer_kodu = val2.GetSafeString(5);
							sTOK_HAREKETLERI.sth_miktar = val2.GetSafeDouble(6);
							sTOK_HAREKETLERI.sth_miktar2 = val2.GetSafeDouble(7);
							sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)val2.GetSafeByte(8);
							sTOK_HAREKETLERI.sth_giris_depo_no = val2.GetSafeInt32(9);
							sTOK_HAREKETLERI.sth_cikis_depo_no = val2.GetSafeInt32(10);
							sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)val2.GetSafeByte(11);
							sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)val2.GetSafeByte(12);
							sTOK_HAREKETLERI.sth_satirno = val2.GetSafeInt32(13);
							sTOK_HAREKETLERI.sth_sip_uid = ((DbDataReader)val2).GetGuid(15);
							sTOK_HAREKETLERI.sth_fat_uid = ((DbDataReader)val2).GetGuid(17);
							sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)val2.GetSafeByte(18);
							sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)val2.GetSafeByte(19);
							sTOK_HAREKETLERI.sth_lastup_date = val2.GetSafeDateTime(20);
							sTOK_HAREKETLERI.sth_tarih = val2.GetSafeDateTime(21);
							sTOK_HAREKETLERI.sth_belge_tarih = val2.GetSafeDateTime(22);
							sTOK_HAREKETLERI.sth_tutar = val2.GetSafeDouble(23);
							sTOK_HAREKETLERI.sth_vergi = val2.GetSafeDouble(24);
							sTOK_HAREKETLERI.sth_har_doviz_kuru = val2.GetSafeDouble(25);
							sTOK_HAREKETLERI.sth_iskonto1 = val2.GetSafeDouble(26);
							sTOK_HAREKETLERI.sth_iskonto2 = val2.GetSafeDouble(27);
							sTOK_HAREKETLERI.sth_iskonto3 = val2.GetSafeDouble(28);
							sTOK_HAREKETLERI.sth_iskonto4 = val2.GetSafeDouble(29);
							sTOK_HAREKETLERI.sth_iskonto5 = val2.GetSafeDouble(30);
							sTOK_HAREKETLERI.sth_iskonto6 = val2.GetSafeDouble(31);
							sTOK_HAREKETLERI.sth_masraf1 = val2.GetSafeDouble(32);
							sTOK_HAREKETLERI.sth_masraf2 = val2.GetSafeDouble(33);
							sTOK_HAREKETLERI.sth_masraf3 = val2.GetSafeDouble(34);
							sTOK_HAREKETLERI.sth_masraf4 = val2.GetSafeDouble(35);
							sTOK_HAREKETLERI.sth_masraf_vergi = val2.GetSafeDouble(36);
							sTOK_HAREKETLERI.sth_vergi_pntr = val2.GetSafeByte(37);
							sTOK_HAREKETLERI.sth_har_doviz_cinsi = val2.GetSafeByte(38);
							sTOK_HAREKETLERI.sth_alt_doviz_kuru = val2.GetSafeDouble(39);
							sTOK_HAREKETLERI.sth_stok_doviz_cinsi = val2.GetSafeByte(40);
							sTOK_HAREKETLERI.sth_stok_doviz_kuru = val2.GetSafeDouble(41);
							sTOK_HAREKETLERI.sth_birim_pntr = val2.GetSafeByte(42);
							sTOK_HAREKETLERI.sth_fiyat_liste_no = val2.GetSafeInt32(43);
							sTOK_HAREKETLERI.sth_adres_no = val2.GetSafeInt32(44);
							sTOK_HAREKETLERI.sto_isim = val2.GetSafeString(45);
							sTOK_HAREKETLERI.sto_birim1_ad = val2.GetSafeString(46);
							sTOK_HAREKETLERI.sto_birim2_ad = val2.GetSafeString(47);
							sTOK_HAREKETLERI.sto_birim3_ad = val2.GetSafeString(48);
							sTOK_HAREKETLERI.sto_birim4_ad = val2.GetSafeString(49);
							sTOK_HAREKETLERI.sto_birim1_katsayi = val2.GetSafeDouble(50);
							sTOK_HAREKETLERI.sto_birim2_katsayi = val2.GetSafeDouble(51);
							sTOK_HAREKETLERI.sto_birim3_katsayi = val2.GetSafeDouble(52);
							sTOK_HAREKETLERI.sto_birim4_katsayi = val2.GetSafeDouble(53);
							sTOK_HAREKETLERI.sth_isk_mas1 = val2.GetSafeByte(54);
							sTOK_HAREKETLERI.sth_isk_mas2 = val2.GetSafeByte(55);
							sTOK_HAREKETLERI.sth_isk_mas3 = val2.GetSafeByte(56);
							sTOK_HAREKETLERI.sth_isk_mas4 = val2.GetSafeByte(57);
							sTOK_HAREKETLERI.sth_isk_mas5 = val2.GetSafeByte(58);
							sTOK_HAREKETLERI.sth_isk_mas6 = val2.GetSafeByte(59);
							sTOK_HAREKETLERI.sth_aciklama = val2.GetSafeString(60);
							bool safeBoolean = val2.GetSafeBoolean(61);
							bool safeBoolean2 = val2.GetSafeBoolean(62);
							if (safeBoolean || safeBoolean2)
							{
								sTOK_HAREKETLERI.renk_beden_hareketleri = StokSqlite.V16_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.StokHareket, sTOK_HAREKETLERI.sth_Guid);
							}
							evrak.AddStokHareketi(sTOK_HAREKETLERI);
						}
					}
					((DbDataReader)val2).Close();
					((DbDataReader)val2).Dispose();
					val2 = null;
					((Component)val).Dispose();
					val = null;
				}
			}
			foreach (STOK_HAREKETLERI item3 in evrak.GetStokHareketleri())
			{
				if (item3.sth_sip_uid != Guid.Empty)
				{
					evrak.SetSiparisKarsilamaMi(YeniDeger: true);
					evrak.AddSiparisHareketi(V16_GetSiparisKalem(OpenedConnection, item3.sth_sip_uid), SiparisKarsilamaYap: false);
				}
			}
			if (evrak.GetStokHareketleri().Count > 0)
			{
				STOK_HAREKETLERI sTOK_HAREKETLERI2 = evrak.GetStokHareketleri()[0];
				evrak.SetFiyatListesi(FiyatListesiSqlite.GetFiyatListesi(OpenedConnection, sTOK_HAREKETLERI2.sth_fiyat_liste_no));
				evrak.SetHedefDepo(DepoSqlite.GetDepo(OpenedConnection, sTOK_HAREKETLERI2.sth_giris_depo_no));
				evrak.SetKaynakDepo(DepoSqlite.GetDepo(OpenedConnection, sTOK_HAREKETLERI2.sth_cikis_depo_no));
				evrak.SetSevkAdresNo(sTOK_HAREKETLERI2.sth_adres_no);
			}
			val = new SqliteCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano=51 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
			if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
			{
				val.Parameters.AddWithValue("@egk_hareket_tip", (object)evrak.GetStokCariHesapHareketi().cha_tip);
				val.Parameters.AddWithValue("@egk_evr_tip", (object)evrak.GetStokCariHesapHareketi().cha_evrak_tip);
				val.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.GetStokCariHesapHareketi().cha_evrakno_seri);
				val.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.GetStokCariHesapHareketi().cha_evrakno_sira);
			}
			else
			{
				val.Parameters.AddWithValue("@egk_hareket_tip", (object)evrak.GetTahsilatHareketleri()[0].cha_tip);
				val.Parameters.AddWithValue("@egk_evr_tip", (object)evrak.GetTahsilatHareketleri()[0].cha_evrak_tip);
				val.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.GetTahsilatHareketleri()[0].cha_evrakno_seri);
				val.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.GetTahsilatHareketleri()[0].cha_evrakno_sira);
			}
			val.Connection = OpenedConnection;
			val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				while (((DbDataReader)val2).Read())
				{
					evrak.SetAciklama1(val2.GetSafeString(0));
					evrak.SetAciklama2(val2.GetSafeString(1));
					evrak.SetAciklama3(val2.GetSafeString(2));
					evrak.SetAciklama4(val2.GetSafeString(3));
					evrak.SetAciklama5(val2.GetSafeString(4));
					evrak.SetAciklama6(val2.GetSafeString(5));
					evrak.SetAciklama7(val2.GetSafeString(6));
					evrak.SetAciklama8(val2.GetSafeString(7));
					evrak.SetAciklama9(val2.GetSafeString(8));
					evrak.SetAciklama10(val2.GetSafeString(9));
				}
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return evrak;
	}

	private static Evrak GetSiparisEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		if (GenelUtility.GetMikroVersiyon(((DbConnection)OpenedConnection).DataSource) > 15)
		{
			return V16_GetSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira, sip_tip, sip_cins);
		}
		return V15_GetSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira, sip_tip, sip_cins);
	}

	private static Evrak V15_GetSiparisEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		//IL_098e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0995: Expected O, but got Unknown
		Evrak evrak = ((sip_tip != enum_sip_tip.Talep) ? new Evrak(enum_GenelEvrakTipleri.VerilenSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret) : new Evrak(enum_GenelEvrakTipleri.AlinanSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret));
		string commandText = "SELECT SIPARISLER.sip_RECno,SIPARISLER.sip_RECid_DBCno,SIPARISLER.sip_RECid_RECno,SIPARISLER.sip_SpecRECno,SIPARISLER.sip_iptal,SIPARISLER.sip_fileid,SIPARISLER.sip_hidden,SIPARISLER.sip_kilitli,SIPARISLER.sip_degisti,SIPARISLER.sip_checksum,SIPARISLER.sip_create_user,SIPARISLER.sip_create_date,SIPARISLER.sip_lastup_user,SIPARISLER.sip_special1,SIPARISLER.sip_special2,SIPARISLER.sip_special3,SIPARISLER.sip_firmano,SIPARISLER.sip_subeno,SIPARISLER.sip_tarih,SIPARISLER.sip_teslim_tarih,SIPARISLER.sip_tip,SIPARISLER.sip_cins,SIPARISLER.sip_evrakno_seri,SIPARISLER.sip_evrakno_sira,SIPARISLER.sip_satirno,SIPARISLER.sip_belgeno,SIPARISLER.sip_belge_tarih,SIPARISLER.sip_satici_kod,SIPARISLER.sip_musteri_kod,SIPARISLER.sip_stok_kod,SIPARISLER.sip_b_fiyat,SIPARISLER.sip_miktar,SIPARISLER.sip_birim_pntr,SIPARISLER.sip_teslim_miktar,SIPARISLER.sip_tutar,SIPARISLER.sip_iskonto_1,SIPARISLER.sip_iskonto_2,SIPARISLER.sip_iskonto_3,SIPARISLER.sip_iskonto_4,SIPARISLER.sip_iskonto_5,SIPARISLER.sip_iskonto_6,SIPARISLER.sip_masraf_1,SIPARISLER.sip_masraf_2,SIPARISLER.sip_masraf_3,SIPARISLER.sip_masraf_4,SIPARISLER.sip_vergi_pntr,SIPARISLER.sip_vergi,SIPARISLER.sip_masvergi_pntr,SIPARISLER.sip_masvergi,SIPARISLER.sip_opno,SIPARISLER.sip_aciklama,SIPARISLER.sip_aciklama2,SIPARISLER.sip_depono,SIPARISLER.sip_OnaylayanKulNo,SIPARISLER.sip_vergisiz_fl,SIPARISLER.sip_kapat_fl,SIPARISLER.sip_promosyon_fl,SIPARISLER.sip_cari_sormerk,SIPARISLER.sip_stok_sormerk,SIPARISLER.sip_cari_grupno,SIPARISLER.sip_doviz_cinsi,SIPARISLER.sip_doviz_kuru,SIPARISLER.sip_alt_doviz_kuru,SIPARISLER.sip_adresno,SIPARISLER.sip_teslimturu,SIPARISLER.sip_cagrilabilir_fl,SIPARISLER.sip_prosiprecDbId,SIPARISLER.sip_prosiprecrecI,SIPARISLER.sip_iskonto1,SIPARISLER.sip_iskonto2,SIPARISLER.sip_iskonto3,SIPARISLER.sip_iskonto4,SIPARISLER.sip_iskonto5,SIPARISLER.sip_iskonto6,SIPARISLER.sip_masraf1,SIPARISLER.sip_masraf2,SIPARISLER.sip_masraf3,SIPARISLER.sip_masraf4,SIPARISLER.sip_isk1,SIPARISLER.sip_isk2,SIPARISLER.sip_isk3,SIPARISLER.sip_isk4,SIPARISLER.sip_isk5,SIPARISLER.sip_isk6,SIPARISLER.sip_mas1,SIPARISLER.sip_mas2,SIPARISLER.sip_mas3,SIPARISLER.sip_mas4,SIPARISLER.sip_Exp_Imp_Kodu,SIPARISLER.sip_kar_orani,SIPARISLER.sip_durumu,SIPARISLER.sip_stalRecId_DBCno,SIPARISLER.sip_stalRecId_RECno,SIPARISLER.sip_planlananmiktar,SIPARISLER.sip_teklifRecId_DBCno,SIPARISLER.sip_teklifRecId_RECno,SIPARISLER.sip_parti_kodu,SIPARISLER.sip_lot_no,SIPARISLER.sip_projekodu,SIPARISLER.sip_fiyat_liste_no,SIPARISLER.sip_Otv_Pntr,SIPARISLER.sip_Otv_Vergi,SIPARISLER.sip_otvtutari,SIPARISLER.sip_OtvVergisiz_Fl,SIPARISLER.sip_paket_kod,SIPARISLER.sip_RezRecId_DBCno,SIPARISLER.sip_RezRecId_RECno,SIPARISLER.sip_harekettipi,SIPARISLER.sip_yetkili_recid_dbcno,SIPARISLER.sip_yetkili_recid_recno,SIPARISLER.sip_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM SIPARISLER INNER JOIN STOKLAR ON STOKLAR.sto_kod=SIPARISLER.sip_stok_kod where sip_evrakno_seri=@EvrakSeri AND sip_evrakno_sira=@EvrakSira AND sip_cins=@sip_cins AND sip_tip=@sip_tip ORDER BY sip_satirno";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@EvrakSeri", (object)EvrakSeri);
				val.Parameters.AddWithValue("@EvrakSira", (object)EvrakSira);
				val.Parameters.AddWithValue("@sip_cins", (object)(int)sip_cins);
				val.Parameters.AddWithValue("@sip_tip", (object)(int)sip_tip);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					SIPARISLER sIPARISLER = new SIPARISLER();
					sIPARISLER.sip_RECno = val2.GetSafeInt32(0);
					sIPARISLER.sip_RECid_DBCno = val2.GetSafeInt16(1);
					sIPARISLER.sip_RECid_RECno = val2.GetSafeInt32(2);
					sIPARISLER.sip_SpecRECno = val2.GetSafeInt32(3);
					sIPARISLER.sip_iptal = val2.GetSafeBoolean(4);
					sIPARISLER.sip_fileid = val2.GetSafeInt16(5);
					sIPARISLER.sip_hidden = val2.GetSafeBoolean(6);
					sIPARISLER.sip_kilitli = val2.GetSafeBoolean(7);
					sIPARISLER.sip_degisti = val2.GetSafeBoolean(8);
					sIPARISLER.sip_checksum = val2.GetSafeInt32(9);
					sIPARISLER.sip_create_user = val2.GetSafeInt16(10);
					sIPARISLER.sip_create_date = val2.GetSafeDateTime(11);
					sIPARISLER.sip_lastup_user = val2.GetSafeInt16(12);
					sIPARISLER.sip_special1 = val2.GetSafeString(13);
					sIPARISLER.sip_special2 = val2.GetSafeString(14);
					sIPARISLER.sip_special3 = val2.GetSafeString(15);
					sIPARISLER.sip_firmano = val2.GetSafeInt32(16);
					sIPARISLER.sip_subeno = val2.GetSafeInt32(17);
					sIPARISLER.sip_tarih = val2.GetSafeDateTime(18);
					sIPARISLER.sip_teslim_tarih = val2.GetSafeDateTime(19);
					sIPARISLER.sip_tip = (enum_sip_tip)val2.GetSafeByte(20);
					sIPARISLER.sip_cins = (enum_sip_cins)val2.GetSafeByte(21);
					sIPARISLER.sip_evrakno_seri = val2.GetSafeString(22);
					sIPARISLER.sip_evrakno_sira = val2.GetSafeInt32(23);
					sIPARISLER.sip_satirno = val2.GetSafeInt32(24);
					sIPARISLER.sip_belgeno = val2.GetSafeString(25);
					sIPARISLER.sip_belge_tarih = val2.GetSafeDateTime(26);
					sIPARISLER.sip_satici_kod = val2.GetSafeString(27);
					sIPARISLER.sip_musteri_kod = val2.GetSafeString(28);
					sIPARISLER.sip_stok_kod = val2.GetSafeString(29);
					sIPARISLER.sip_b_fiyat = val2.GetSafeDouble(30);
					sIPARISLER.sip_miktar = val2.GetSafeDouble(31);
					sIPARISLER.sip_birim_pntr = val2.GetSafeByte(32);
					sIPARISLER.sip_teslim_miktar = val2.GetSafeDouble(33);
					sIPARISLER.sip_tutar = val2.GetSafeDouble(34);
					sIPARISLER.sip_iskonto_1 = val2.GetSafeDouble(35);
					sIPARISLER.sip_iskonto_2 = val2.GetSafeDouble(36);
					sIPARISLER.sip_iskonto_3 = val2.GetSafeDouble(37);
					sIPARISLER.sip_iskonto_4 = val2.GetSafeDouble(38);
					sIPARISLER.sip_iskonto_5 = val2.GetSafeDouble(39);
					sIPARISLER.sip_iskonto_6 = val2.GetSafeDouble(40);
					sIPARISLER.sip_masraf_1 = val2.GetSafeDouble(41);
					sIPARISLER.sip_masraf_2 = val2.GetSafeDouble(42);
					sIPARISLER.sip_masraf_3 = val2.GetSafeDouble(43);
					sIPARISLER.sip_masraf_4 = val2.GetSafeDouble(44);
					sIPARISLER.sip_vergi_pntr = val2.GetSafeByte(45);
					sIPARISLER.sip_vergi = val2.GetSafeDouble(46);
					sIPARISLER.sip_masvergi_pntr = val2.GetSafeByte(47);
					sIPARISLER.sip_masvergi = val2.GetSafeDouble(48);
					sIPARISLER.sip_opno = val2.GetSafeInt32(49);
					sIPARISLER.sip_aciklama = val2.GetSafeString(50);
					sIPARISLER.sip_aciklama2 = val2.GetSafeString(51);
					sIPARISLER.sip_depono = val2.GetSafeInt32(52);
					sIPARISLER.sip_OnaylayanKulNo = val2.GetSafeInt16(53);
					sIPARISLER.sip_vergisiz_fl = val2.GetSafeBoolean(54);
					sIPARISLER.sip_kapat_fl = val2.GetSafeBoolean(55);
					sIPARISLER.sip_promosyon_fl = val2.GetSafeBoolean(56);
					sIPARISLER.sip_cari_sormerk = val2.GetSafeString(57);
					sIPARISLER.sip_stok_sormerk = val2.GetSafeString(58);
					sIPARISLER.sip_cari_grupno = val2.GetSafeByte(59);
					sIPARISLER.sip_doviz_cinsi = val2.GetSafeByte(60);
					sIPARISLER.sip_doviz_kuru = val2.GetSafeDouble(61);
					sIPARISLER.sip_alt_doviz_kuru = val2.GetSafeDouble(62);
					sIPARISLER.sip_adresno = val2.GetSafeInt32(63);
					sIPARISLER.sip_teslimturu = val2.GetSafeString(64);
					sIPARISLER.sip_cagrilabilir_fl = val2.GetSafeBoolean(65);
					sIPARISLER.sip_prosiprecDbId = val2.GetSafeInt16(66);
					sIPARISLER.sip_prosiprecrecI = val2.GetSafeInt32(67);
					sIPARISLER.sip_iskonto1 = val2.GetSafeByte(68);
					sIPARISLER.sip_iskonto2 = val2.GetSafeByte(69);
					sIPARISLER.sip_iskonto3 = val2.GetSafeByte(70);
					sIPARISLER.sip_iskonto4 = val2.GetSafeByte(71);
					sIPARISLER.sip_iskonto5 = val2.GetSafeByte(72);
					sIPARISLER.sip_iskonto6 = val2.GetSafeByte(73);
					sIPARISLER.sip_masraf1 = val2.GetSafeByte(74);
					sIPARISLER.sip_masraf2 = val2.GetSafeByte(75);
					sIPARISLER.sip_masraf3 = val2.GetSafeByte(76);
					sIPARISLER.sip_masraf4 = val2.GetSafeByte(77);
					sIPARISLER.sip_isk1 = val2.GetSafeBoolean(78);
					sIPARISLER.sip_isk2 = val2.GetSafeBoolean(79);
					sIPARISLER.sip_isk3 = val2.GetSafeBoolean(80);
					sIPARISLER.sip_isk4 = val2.GetSafeBoolean(81);
					sIPARISLER.sip_isk5 = val2.GetSafeBoolean(82);
					sIPARISLER.sip_isk6 = val2.GetSafeBoolean(83);
					sIPARISLER.sip_mas1 = val2.GetSafeBoolean(84);
					sIPARISLER.sip_mas2 = val2.GetSafeBoolean(85);
					sIPARISLER.sip_mas3 = val2.GetSafeBoolean(86);
					sIPARISLER.sip_mas4 = val2.GetSafeBoolean(87);
					sIPARISLER.sip_Exp_Imp_Kodu = val2.GetSafeString(88);
					sIPARISLER.sip_kar_orani = val2.GetSafeDouble(89);
					sIPARISLER.sip_durumu = (enum_sip_durumu)val2.GetSafeByte(90);
					sIPARISLER.sip_stalRecId_DBCno = val2.GetSafeInt16(91);
					sIPARISLER.sip_stalRecId_RECno = val2.GetSafeInt32(92);
					sIPARISLER.sip_planlananmiktar = val2.GetSafeDouble(93);
					sIPARISLER.sip_teklifRecId_DBCno = val2.GetSafeInt16(94);
					sIPARISLER.sip_teklifRecId_RECno = val2.GetSafeInt32(95);
					sIPARISLER.sip_parti_kodu = val2.GetSafeString(96);
					sIPARISLER.sip_lot_no = val2.GetSafeInt32(97);
					sIPARISLER.sip_projekodu = val2.GetSafeString(98);
					sIPARISLER.sip_fiyat_liste_no = val2.GetSafeInt32(99);
					sIPARISLER.sip_Otv_Pntr = val2.GetSafeByte(100);
					sIPARISLER.sip_Otv_Vergi = val2.GetSafeDouble(101);
					sIPARISLER.sip_otvtutari = val2.GetSafeDouble(102);
					sIPARISLER.sip_OtvVergisiz_Fl = val2.GetSafeByte(103);
					sIPARISLER.sip_paket_kod = val2.GetSafeString(104);
					sIPARISLER.sip_RezRecId_DBCno = val2.GetSafeInt16(105);
					sIPARISLER.sip_RezRecId_RECno = val2.GetSafeInt32(106);
					sIPARISLER.sip_harekettipi = (enum_sip_harekettipi)val2.GetSafeByte(107);
					sIPARISLER.sip_yetkili_recid_dbcno = val2.GetSafeInt16(108);
					sIPARISLER.sip_yetkili_recid_recno = val2.GetSafeInt32(109);
					sIPARISLER.sip_kapatmanedenkod = val2.GetSafeString(110);
					sIPARISLER.sto_isim = val2.GetSafeString(111);
					sIPARISLER.sto_birim1_ad = val2.GetSafeString(112);
					sIPARISLER.sto_birim2_ad = val2.GetSafeString(113);
					sIPARISLER.sto_birim3_ad = val2.GetSafeString(114);
					sIPARISLER.sto_birim4_ad = val2.GetSafeString(115);
					sIPARISLER.sto_birim1_katsayi = val2.GetSafeDouble(116);
					sIPARISLER.sto_birim2_katsayi = val2.GetSafeDouble(117);
					sIPARISLER.sto_birim3_katsayi = val2.GetSafeDouble(118);
					sIPARISLER.sto_birim4_katsayi = val2.GetSafeDouble(119);
					bool safeBoolean = val2.GetSafeBoolean(120);
					bool safeBoolean2 = val2.GetSafeBoolean(121);
					if (safeBoolean || safeBoolean2)
					{
						sIPARISLER.renk_beden_hareketleri = StokSqlite.V15_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.Siparis, sIPARISLER.sip_RECid_DBCno, sIPARISLER.sip_RECid_RECno);
					}
					evrak.AddSiparisHareketi(sIPARISLER, SiparisKarsilamaYap: false);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			if (evrak.GetSiparisler().Count > 0)
			{
				SIPARISLER sIPARISLER2 = evrak.GetSiparisler()[0];
				evrak.alternatifdovizkuru = new Kur();
				evrak.alternatifdovizkuru.dov_fiyat = sIPARISLER2.sip_alt_doviz_kuru;
				evrak.SetBelgeNo(sIPARISLER2.sip_belgeno);
				evrak.SetBelgeTarihi(sIPARISLER2.sip_belge_tarih);
				evrak.cari = CariSqlite.GetCariByCariKod(OpenedConnection, sIPARISLER2.sip_musteri_kod, AdreslerTemsilciyeGore: false, "");
				evrak.SetKaynakDepo(DepoSqlite.GetDepo(OpenedConnection, sIPARISLER2.sip_depono));
				evrak.SetDovizCinsi(sIPARISLER2.sip_doviz_cinsi);
				evrak.SetFiyatListesi(FiyatListesiSqlite.GetFiyatListesi(OpenedConnection, sIPARISLER2.sip_fiyat_liste_no));
				evrak.kur = new Kur();
				evrak.kur.dov_fiyat = sIPARISLER2.sip_doviz_kuru;
				evrak.SetProje(ProjeSqlite.GetProje(OpenedConnection, sIPARISLER2.sip_projekodu));
				evrak.SetSorumlulukMerkezi(SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(OpenedConnection, sIPARISLER2.sip_cari_sormerk));
				evrak.SetTemsilciKodu(sIPARISLER2.sip_satici_kod);
				evrak.SetEvrakKilitli(sIPARISLER2.sip_kilitli);
				evrak.SetEvrakNoSeri(EvrakSeri);
				evrak.SetEvraknoSira(EvrakSira);
				evrak.SetEvrakTarihi(sIPARISLER2.sip_tarih);
				evrak.SetFirma(FirmaSqlite.GetFirma(OpenedConnection, sIPARISLER2.sip_firmano));
				evrak.SetSube(SubeSqlite.GetSube(OpenedConnection, sIPARISLER2.sip_subeno));
				evrak.SetMikroUserNo(sIPARISLER2.sip_create_user);
				evrak.SetOdemePlani(sIPARISLER2.sip_opno);
				evrak.SetSevkAdresNo(sIPARISLER2.sip_adresno);
				evrak.SetYeniKayit(YeniDeger: false);
				if (evrak.GetSiparisler()[0].sip_kilitli)
				{
					evrak.SetEvrakKilitli(YeniDeger: true);
				}
				else
				{
					evrak.SetEvrakKilitli(YeniDeger: false);
				}
				commandText = "SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano=@egk_dosyano AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira";
				SqliteCommand val3 = new SqliteCommand(commandText);
				val3.Connection = OpenedConnection;
				val3.Parameters.AddWithValue("@egk_dosyano", (object)21);
				val3.Parameters.AddWithValue("@egk_hareket_tip", (object)0);
				val3.Parameters.AddWithValue("@egk_evr_tip", (object)0);
				val3.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.GetSiparisler()[0].sip_evrakno_seri);
				val3.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.GetSiparisler()[0].sip_evrakno_sira);
				SqliteDataReader val4 = val3.ExecuteReader();
				if (((DbDataReader)val4).HasRows)
				{
					while (((DbDataReader)val4).Read())
					{
						evrak.SetAciklama1(val4.GetSafeString(0));
						evrak.SetAciklama2(val4.GetSafeString(1));
						evrak.SetAciklama3(val4.GetSafeString(2));
						evrak.SetAciklama4(val4.GetSafeString(3));
						evrak.SetAciklama5(val4.GetSafeString(4));
						evrak.SetAciklama6(val4.GetSafeString(5));
						evrak.SetAciklama7(val4.GetSafeString(6));
						evrak.SetAciklama8(val4.GetSafeString(7));
						evrak.SetAciklama9(val4.GetSafeString(8));
						evrak.SetAciklama10(val4.GetSafeString(9));
					}
				}
				((DbDataReader)val4).Close();
				((DbDataReader)val4).Dispose();
				val4 = null;
				((Component)val3).Dispose();
				val3 = null;
			}
		}
		catch
		{
		}
		return evrak;
	}

	private static Evrak V16_GetSiparisEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		//IL_08d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08da: Expected O, but got Unknown
		Evrak evrak = ((sip_tip != enum_sip_tip.Talep) ? new Evrak(enum_GenelEvrakTipleri.VerilenSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret) : new Evrak(enum_GenelEvrakTipleri.AlinanSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret));
		string commandText = "SELECT SIPARISLER.sip_Guid,SIPARISLER.sip_SpecRECno,SIPARISLER.sip_iptal,SIPARISLER.sip_fileid,SIPARISLER.sip_hidden,SIPARISLER.sip_kilitli,SIPARISLER.sip_degisti,SIPARISLER.sip_checksum,SIPARISLER.sip_create_user,SIPARISLER.sip_create_date,SIPARISLER.sip_lastup_user,SIPARISLER.sip_special1,SIPARISLER.sip_special2,SIPARISLER.sip_special3,SIPARISLER.sip_firmano,SIPARISLER.sip_subeno,SIPARISLER.sip_tarih,SIPARISLER.sip_teslim_tarih,SIPARISLER.sip_tip,SIPARISLER.sip_cins,SIPARISLER.sip_evrakno_seri,SIPARISLER.sip_evrakno_sira,SIPARISLER.sip_satirno,SIPARISLER.sip_belgeno,SIPARISLER.sip_belge_tarih,SIPARISLER.sip_satici_kod,SIPARISLER.sip_musteri_kod,SIPARISLER.sip_stok_kod,SIPARISLER.sip_b_fiyat,SIPARISLER.sip_miktar,SIPARISLER.sip_birim_pntr,SIPARISLER.sip_teslim_miktar,SIPARISLER.sip_tutar,SIPARISLER.sip_iskonto_1,SIPARISLER.sip_iskonto_2,SIPARISLER.sip_iskonto_3,SIPARISLER.sip_iskonto_4,SIPARISLER.sip_iskonto_5,SIPARISLER.sip_iskonto_6,SIPARISLER.sip_masraf_1,SIPARISLER.sip_masraf_2,SIPARISLER.sip_masraf_3,SIPARISLER.sip_masraf_4,SIPARISLER.sip_vergi_pntr,SIPARISLER.sip_vergi,SIPARISLER.sip_masvergi_pntr,SIPARISLER.sip_masvergi,SIPARISLER.sip_opno,SIPARISLER.sip_aciklama,SIPARISLER.sip_aciklama2,SIPARISLER.sip_depono,SIPARISLER.sip_OnaylayanKulNo,SIPARISLER.sip_vergisiz_fl,SIPARISLER.sip_kapat_fl,SIPARISLER.sip_promosyon_fl,SIPARISLER.sip_cari_sormerk,SIPARISLER.sip_stok_sormerk,SIPARISLER.sip_cari_grupno,SIPARISLER.sip_doviz_cinsi,SIPARISLER.sip_doviz_kuru,SIPARISLER.sip_alt_doviz_kuru,SIPARISLER.sip_adresno,SIPARISLER.sip_teslimturu,SIPARISLER.sip_cagrilabilir_fl,SIPARISLER.sip_iskonto1,SIPARISLER.sip_iskonto2,SIPARISLER.sip_iskonto3,SIPARISLER.sip_iskonto4,SIPARISLER.sip_iskonto5,SIPARISLER.sip_iskonto6,SIPARISLER.sip_masraf1,SIPARISLER.sip_masraf2,SIPARISLER.sip_masraf3,SIPARISLER.sip_masraf4,SIPARISLER.sip_isk1,SIPARISLER.sip_isk2,SIPARISLER.sip_isk3,SIPARISLER.sip_isk4,SIPARISLER.sip_isk5,SIPARISLER.sip_isk6,SIPARISLER.sip_mas1,SIPARISLER.sip_mas2,SIPARISLER.sip_mas3,SIPARISLER.sip_mas4,SIPARISLER.sip_Exp_Imp_Kodu,SIPARISLER.sip_kar_orani,SIPARISLER.sip_durumu,SIPARISLER.sip_planlananmiktar,SIPARISLER.sip_parti_kodu,SIPARISLER.sip_lot_no,SIPARISLER.sip_projekodu,SIPARISLER.sip_fiyat_liste_no,SIPARISLER.sip_Otv_Pntr,SIPARISLER.sip_Otv_Vergi,SIPARISLER.sip_otvtutari,SIPARISLER.sip_OtvVergisiz_Fl,SIPARISLER.sip_paket_kod,SIPARISLER.sip_harekettipi,SIPARISLER.sip_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM SIPARISLER INNER JOIN STOKLAR ON STOKLAR.sto_kod=SIPARISLER.sip_stok_kod where sip_evrakno_seri=@EvrakSeri AND sip_evrakno_sira=@EvrakSira AND sip_cins=@sip_cins AND sip_tip=@sip_tip ORDER BY sip_satirno";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@EvrakSeri", (object)EvrakSeri);
				val.Parameters.AddWithValue("@EvrakSira", (object)EvrakSira);
				val.Parameters.AddWithValue("@sip_cins", (object)(int)sip_cins);
				val.Parameters.AddWithValue("@sip_tip", (object)(int)sip_tip);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					SIPARISLER sIPARISLER = new SIPARISLER();
					sIPARISLER.sip_Guid = ((DbDataReader)val2).GetGuid(0);
					sIPARISLER.sip_SpecRECno = val2.GetSafeInt32(1);
					sIPARISLER.sip_iptal = val2.GetSafeBoolean(2);
					sIPARISLER.sip_fileid = val2.GetSafeInt16(3);
					sIPARISLER.sip_hidden = val2.GetSafeBoolean(4);
					sIPARISLER.sip_kilitli = val2.GetSafeBoolean(5);
					sIPARISLER.sip_degisti = val2.GetSafeBoolean(6);
					sIPARISLER.sip_checksum = val2.GetSafeInt32(7);
					sIPARISLER.sip_create_user = val2.GetSafeInt16(8);
					sIPARISLER.sip_create_date = val2.GetSafeDateTime(9);
					sIPARISLER.sip_lastup_user = val2.GetSafeInt16(10);
					sIPARISLER.sip_special1 = val2.GetSafeString(11);
					sIPARISLER.sip_special2 = val2.GetSafeString(12);
					sIPARISLER.sip_special3 = val2.GetSafeString(13);
					sIPARISLER.sip_firmano = val2.GetSafeInt32(14);
					sIPARISLER.sip_subeno = val2.GetSafeInt32(15);
					sIPARISLER.sip_tarih = val2.GetSafeDateTime(16);
					sIPARISLER.sip_teslim_tarih = val2.GetSafeDateTime(17);
					sIPARISLER.sip_tip = (enum_sip_tip)val2.GetSafeByte(18);
					sIPARISLER.sip_cins = (enum_sip_cins)val2.GetSafeByte(19);
					sIPARISLER.sip_evrakno_seri = val2.GetSafeString(20);
					sIPARISLER.sip_evrakno_sira = val2.GetSafeInt32(21);
					sIPARISLER.sip_satirno = val2.GetSafeInt32(22);
					sIPARISLER.sip_belgeno = val2.GetSafeString(23);
					sIPARISLER.sip_belge_tarih = val2.GetSafeDateTime(24);
					sIPARISLER.sip_satici_kod = val2.GetSafeString(25);
					sIPARISLER.sip_musteri_kod = val2.GetSafeString(26);
					sIPARISLER.sip_stok_kod = val2.GetSafeString(27);
					sIPARISLER.sip_b_fiyat = val2.GetSafeDouble(28);
					sIPARISLER.sip_miktar = val2.GetSafeDouble(29);
					sIPARISLER.sip_birim_pntr = val2.GetSafeByte(30);
					sIPARISLER.sip_teslim_miktar = val2.GetSafeDouble(31);
					sIPARISLER.sip_tutar = val2.GetSafeDouble(32);
					sIPARISLER.sip_iskonto_1 = val2.GetSafeDouble(33);
					sIPARISLER.sip_iskonto_2 = val2.GetSafeDouble(34);
					sIPARISLER.sip_iskonto_3 = val2.GetSafeDouble(35);
					sIPARISLER.sip_iskonto_4 = val2.GetSafeDouble(36);
					sIPARISLER.sip_iskonto_5 = val2.GetSafeDouble(37);
					sIPARISLER.sip_iskonto_6 = val2.GetSafeDouble(38);
					sIPARISLER.sip_masraf_1 = val2.GetSafeDouble(39);
					sIPARISLER.sip_masraf_2 = val2.GetSafeDouble(40);
					sIPARISLER.sip_masraf_3 = val2.GetSafeDouble(41);
					sIPARISLER.sip_masraf_4 = val2.GetSafeDouble(42);
					sIPARISLER.sip_vergi_pntr = val2.GetSafeByte(43);
					sIPARISLER.sip_vergi = val2.GetSafeDouble(44);
					sIPARISLER.sip_masvergi_pntr = val2.GetSafeByte(45);
					sIPARISLER.sip_masvergi = val2.GetSafeDouble(46);
					sIPARISLER.sip_opno = val2.GetSafeInt32(47);
					sIPARISLER.sip_aciklama = val2.GetSafeString(48);
					sIPARISLER.sip_aciklama2 = val2.GetSafeString(49);
					sIPARISLER.sip_depono = val2.GetSafeInt32(50);
					sIPARISLER.sip_OnaylayanKulNo = val2.GetSafeInt16(51);
					sIPARISLER.sip_vergisiz_fl = val2.GetSafeBoolean(52);
					sIPARISLER.sip_kapat_fl = val2.GetSafeBoolean(53);
					sIPARISLER.sip_promosyon_fl = val2.GetSafeBoolean(54);
					sIPARISLER.sip_cari_sormerk = val2.GetSafeString(55);
					sIPARISLER.sip_stok_sormerk = val2.GetSafeString(56);
					sIPARISLER.sip_cari_grupno = val2.GetSafeByte(57);
					sIPARISLER.sip_doviz_cinsi = val2.GetSafeByte(58);
					sIPARISLER.sip_doviz_kuru = val2.GetSafeDouble(59);
					sIPARISLER.sip_alt_doviz_kuru = val2.GetSafeDouble(60);
					sIPARISLER.sip_adresno = val2.GetSafeInt32(61);
					sIPARISLER.sip_teslimturu = val2.GetSafeString(62);
					sIPARISLER.sip_cagrilabilir_fl = val2.GetSafeBoolean(63);
					sIPARISLER.sip_iskonto1 = val2.GetSafeByte(64);
					sIPARISLER.sip_iskonto2 = val2.GetSafeByte(65);
					sIPARISLER.sip_iskonto3 = val2.GetSafeByte(66);
					sIPARISLER.sip_iskonto4 = val2.GetSafeByte(67);
					sIPARISLER.sip_iskonto5 = val2.GetSafeByte(68);
					sIPARISLER.sip_iskonto6 = val2.GetSafeByte(69);
					sIPARISLER.sip_masraf1 = val2.GetSafeByte(70);
					sIPARISLER.sip_masraf2 = val2.GetSafeByte(71);
					sIPARISLER.sip_masraf3 = val2.GetSafeByte(72);
					sIPARISLER.sip_masraf4 = val2.GetSafeByte(73);
					sIPARISLER.sip_isk1 = val2.GetSafeBoolean(74);
					sIPARISLER.sip_isk2 = val2.GetSafeBoolean(75);
					sIPARISLER.sip_isk3 = val2.GetSafeBoolean(76);
					sIPARISLER.sip_isk4 = val2.GetSafeBoolean(77);
					sIPARISLER.sip_isk5 = val2.GetSafeBoolean(78);
					sIPARISLER.sip_isk6 = val2.GetSafeBoolean(79);
					sIPARISLER.sip_mas1 = val2.GetSafeBoolean(80);
					sIPARISLER.sip_mas2 = val2.GetSafeBoolean(81);
					sIPARISLER.sip_mas3 = val2.GetSafeBoolean(82);
					sIPARISLER.sip_mas4 = val2.GetSafeBoolean(83);
					sIPARISLER.sip_Exp_Imp_Kodu = val2.GetSafeString(84);
					sIPARISLER.sip_kar_orani = val2.GetSafeDouble(85);
					sIPARISLER.sip_durumu = (enum_sip_durumu)val2.GetSafeByte(86);
					sIPARISLER.sip_planlananmiktar = val2.GetSafeDouble(87);
					sIPARISLER.sip_parti_kodu = val2.GetSafeString(88);
					sIPARISLER.sip_lot_no = val2.GetSafeInt32(89);
					sIPARISLER.sip_projekodu = val2.GetSafeString(90);
					sIPARISLER.sip_fiyat_liste_no = val2.GetSafeInt32(91);
					sIPARISLER.sip_Otv_Pntr = val2.GetSafeByte(92);
					sIPARISLER.sip_Otv_Vergi = val2.GetSafeDouble(93);
					sIPARISLER.sip_otvtutari = val2.GetSafeDouble(94);
					sIPARISLER.sip_OtvVergisiz_Fl = val2.GetSafeByte(95);
					sIPARISLER.sip_paket_kod = val2.GetSafeString(96);
					sIPARISLER.sip_harekettipi = (enum_sip_harekettipi)val2.GetSafeByte(97);
					sIPARISLER.sip_kapatmanedenkod = val2.GetSafeString(98);
					sIPARISLER.sto_isim = val2.GetSafeString(99);
					sIPARISLER.sto_birim1_ad = val2.GetSafeString(100);
					sIPARISLER.sto_birim2_ad = val2.GetSafeString(101);
					sIPARISLER.sto_birim3_ad = val2.GetSafeString(102);
					sIPARISLER.sto_birim4_ad = val2.GetSafeString(103);
					sIPARISLER.sto_birim1_katsayi = val2.GetSafeDouble(104);
					sIPARISLER.sto_birim2_katsayi = val2.GetSafeDouble(105);
					sIPARISLER.sto_birim3_katsayi = val2.GetSafeDouble(106);
					sIPARISLER.sto_birim4_katsayi = val2.GetSafeDouble(107);
					bool safeBoolean = val2.GetSafeBoolean(108);
					bool safeBoolean2 = val2.GetSafeBoolean(109);
					if (safeBoolean || safeBoolean2)
					{
						sIPARISLER.renk_beden_hareketleri = StokSqlite.V16_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.Siparis, sIPARISLER.sip_Guid);
					}
					evrak.AddSiparisHareketi(sIPARISLER, SiparisKarsilamaYap: false);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			if (evrak.GetSiparisler().Count > 0)
			{
				SIPARISLER sIPARISLER2 = evrak.GetSiparisler()[0];
				evrak.alternatifdovizkuru = new Kur();
				evrak.alternatifdovizkuru.dov_fiyat = sIPARISLER2.sip_alt_doviz_kuru;
				evrak.SetBelgeNo(sIPARISLER2.sip_belgeno);
				evrak.SetBelgeTarihi(sIPARISLER2.sip_belge_tarih);
				evrak.cari = CariSqlite.GetCariByCariKod(OpenedConnection, sIPARISLER2.sip_musteri_kod, AdreslerTemsilciyeGore: false, "");
				evrak.SetKaynakDepo(DepoSqlite.GetDepo(OpenedConnection, sIPARISLER2.sip_depono));
				evrak.SetDovizCinsi(sIPARISLER2.sip_doviz_cinsi);
				evrak.SetFiyatListesi(FiyatListesiSqlite.GetFiyatListesi(OpenedConnection, sIPARISLER2.sip_fiyat_liste_no));
				evrak.kur = new Kur();
				evrak.kur.dov_fiyat = sIPARISLER2.sip_doviz_kuru;
				evrak.SetProje(ProjeSqlite.GetProje(OpenedConnection, sIPARISLER2.sip_projekodu));
				evrak.SetSorumlulukMerkezi(SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(OpenedConnection, sIPARISLER2.sip_cari_sormerk));
				evrak.SetTemsilciKodu(sIPARISLER2.sip_satici_kod);
				evrak.SetEvrakKilitli(sIPARISLER2.sip_kilitli);
				evrak.SetEvrakNoSeri(EvrakSeri);
				evrak.SetEvraknoSira(EvrakSira);
				evrak.SetEvrakTarihi(sIPARISLER2.sip_tarih);
				evrak.SetFirma(FirmaSqlite.GetFirma(OpenedConnection, sIPARISLER2.sip_firmano));
				evrak.SetSube(SubeSqlite.GetSube(OpenedConnection, sIPARISLER2.sip_subeno));
				evrak.SetMikroUserNo(sIPARISLER2.sip_create_user);
				evrak.SetOdemePlani(sIPARISLER2.sip_opno);
				evrak.SetSevkAdresNo(sIPARISLER2.sip_adresno);
				evrak.SetYeniKayit(YeniDeger: false);
				if (evrak.GetSiparisler()[0].sip_kilitli)
				{
					evrak.SetEvrakKilitli(YeniDeger: true);
				}
				else
				{
					evrak.SetEvrakKilitli(YeniDeger: false);
				}
				commandText = "SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano=@egk_dosyano AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira";
				SqliteCommand val3 = new SqliteCommand(commandText);
				val3.Connection = OpenedConnection;
				val3.Parameters.AddWithValue("@egk_dosyano", (object)21);
				val3.Parameters.AddWithValue("@egk_hareket_tip", (object)0);
				val3.Parameters.AddWithValue("@egk_evr_tip", (object)0);
				val3.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.GetSiparisler()[0].sip_evrakno_seri);
				val3.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.GetSiparisler()[0].sip_evrakno_sira);
				SqliteDataReader val4 = val3.ExecuteReader();
				if (((DbDataReader)val4).HasRows)
				{
					while (((DbDataReader)val4).Read())
					{
						evrak.SetAciklama1(val4.GetSafeString(0));
						evrak.SetAciklama2(val4.GetSafeString(1));
						evrak.SetAciklama3(val4.GetSafeString(2));
						evrak.SetAciklama4(val4.GetSafeString(3));
						evrak.SetAciklama5(val4.GetSafeString(4));
						evrak.SetAciklama6(val4.GetSafeString(5));
						evrak.SetAciklama7(val4.GetSafeString(6));
						evrak.SetAciklama8(val4.GetSafeString(7));
						evrak.SetAciklama9(val4.GetSafeString(8));
						evrak.SetAciklama10(val4.GetSafeString(9));
					}
				}
				((DbDataReader)val4).Close();
				((DbDataReader)val4).Dispose();
				val4 = null;
				((Component)val3).Dispose();
				val3 = null;
			}
		}
		catch
		{
		}
		return evrak;
	}

	private static Evrak GetProformaSiparisEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		if (GenelUtility.GetMikroVersiyon(((DbConnection)OpenedConnection).DataSource) > 15)
		{
			return V16_GetProformaSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira, sip_tip, sip_cins);
		}
		return V15_GetProformaSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira, sip_tip, sip_cins);
	}

	private static Evrak V15_GetProformaSiparisEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		//IL_0985: Unknown result type (might be due to invalid IL or missing references)
		//IL_098c: Expected O, but got Unknown
		Evrak evrak = new Evrak(enum_GenelEvrakTipleri.ProformaSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		string commandText = "SELECT PROFORMA_SIPARISLER.pro_RECno,PROFORMA_SIPARISLER.pro_RECid_DBCno,PROFORMA_SIPARISLER.pro_RECid_RECno,PROFORMA_SIPARISLER.pro_SpecRecNo,PROFORMA_SIPARISLER.pro_iptal,PROFORMA_SIPARISLER.pro_fileid,PROFORMA_SIPARISLER.pro_hidden,PROFORMA_SIPARISLER.pro_kilitli,PROFORMA_SIPARISLER.pro_degisti,PROFORMA_SIPARISLER.pro_checksum,PROFORMA_SIPARISLER.pro_create_user,PROFORMA_SIPARISLER.pro_create_date,PROFORMA_SIPARISLER.pro_lastup_user,PROFORMA_SIPARISLER.pro_special1,PROFORMA_SIPARISLER.pro_special2,PROFORMA_SIPARISLER.pro_special3,PROFORMA_SIPARISLER.pro_firmano,PROFORMA_SIPARISLER.pro_subeno,PROFORMA_SIPARISLER.pro_tarihi,PROFORMA_SIPARISLER.pro_testarihi,PROFORMA_SIPARISLER.pro_tipi,PROFORMA_SIPARISLER.pro_cinsi,PROFORMA_SIPARISLER.pro_evrakno_seri,PROFORMA_SIPARISLER.pro_evrakno_sira,PROFORMA_SIPARISLER.pro_satirno,PROFORMA_SIPARISLER.pro_belge_no,PROFORMA_SIPARISLER.pro_belge_tarihi,PROFORMA_SIPARISLER.pro_saticikodu,PROFORMA_SIPARISLER.pro_mustkodu,PROFORMA_SIPARISLER.pro_stokkodu,PROFORMA_SIPARISLER.pro_bfiyati,PROFORMA_SIPARISLER.pro_miktar,PROFORMA_SIPARISLER.pro_birim_pntr,PROFORMA_SIPARISLER.pro_tesmiktari,PROFORMA_SIPARISLER.pro_tutari,PROFORMA_SIPARISLER.pro_iskonto1,PROFORMA_SIPARISLER.pro_iskonto2,PROFORMA_SIPARISLER.pro_iskonto3,PROFORMA_SIPARISLER.pro_iskonto4,PROFORMA_SIPARISLER.pro_iskonto5,PROFORMA_SIPARISLER.pro_iskonto6,PROFORMA_SIPARISLER.pro_masraf1,PROFORMA_SIPARISLER.pro_masraf2,PROFORMA_SIPARISLER.pro_masraf3,PROFORMA_SIPARISLER.pro_masraf4,PROFORMA_SIPARISLER.pro_vergipntr,PROFORMA_SIPARISLER.pro_vergi,PROFORMA_SIPARISLER.pro_masrafvergipntr,PROFORMA_SIPARISLER.pro_masrafvergi,PROFORMA_SIPARISLER.pro_opno,PROFORMA_SIPARISLER.pro_aciklama,PROFORMA_SIPARISLER.pro_aciklama2,PROFORMA_SIPARISLER.pro_depono,PROFORMA_SIPARISLER.pro_onaylayanKul_no,PROFORMA_SIPARISLER.pro_vergisiz,PROFORMA_SIPARISLER.pro_kapat,PROFORMA_SIPARISLER.pro_promosyon_fl,PROFORMA_SIPARISLER.pro_cari_sormerk,PROFORMA_SIPARISLER.pro_stok_sormerk,PROFORMA_SIPARISLER.pro_cari_grupno,PROFORMA_SIPARISLER.pro_dovizcinsi,PROFORMA_SIPARISLER.pro_dovizkuru,PROFORMA_SIPARISLER.pro_altdovizkuru,PROFORMA_SIPARISLER.pro_adresno,PROFORMA_SIPARISLER.pro_teslimturu,PROFORMA_SIPARISLER.pro_cagrilabilir_fl,PROFORMA_SIPARISLER.pro_sipDbID,PROFORMA_SIPARISLER.pro_sipRecID,PROFORMA_SIPARISLER.pro_isk_mas_1,PROFORMA_SIPARISLER.pro_isk_mas_2,PROFORMA_SIPARISLER.pro_isk_mas_3,PROFORMA_SIPARISLER.pro_isk_mas_4,PROFORMA_SIPARISLER.pro_isk_mas_5,PROFORMA_SIPARISLER.pro_isk_mas_6,PROFORMA_SIPARISLER.pro_isk_mas_7,PROFORMA_SIPARISLER.pro_isk_mas_8,PROFORMA_SIPARISLER.pro_isk_mas_9,PROFORMA_SIPARISLER.pro_isk_mas_10,PROFORMA_SIPARISLER.pro_sat_isk_mas1,PROFORMA_SIPARISLER.pro_sat_isk_mas2,PROFORMA_SIPARISLER.pro_sat_isk_mas3,PROFORMA_SIPARISLER.pro_sat_isk_mas4,PROFORMA_SIPARISLER.pro_sat_isk_mas5,PROFORMA_SIPARISLER.pro_sat_isk_mas6,PROFORMA_SIPARISLER.pro_sat_isk_mas7,PROFORMA_SIPARISLER.pro_sat_isk_mas8,PROFORMA_SIPARISLER.pro_sat_isk_mas9,PROFORMA_SIPARISLER.pro_sat_isk_mas10,PROFORMA_SIPARISLER.pro_Exp_Imp_Kodu,PROFORMA_SIPARISLER.pro_karoani,PROFORMA_SIPARISLER.pro_durumu,PROFORMA_SIPARISLER.pro_stalRecId_DBCno,PROFORMA_SIPARISLER.pro_stalRecId_RECno,PROFORMA_SIPARISLER.pro_planlananmiktar,PROFORMA_SIPARISLER.pro_teklifRecId_DBCno,PROFORMA_SIPARISLER.pro_teklifRecId_RECno,PROFORMA_SIPARISLER.pro_parti_kodu,PROFORMA_SIPARISLER.pro_lot_no,PROFORMA_SIPARISLER.pro_projekodu,PROFORMA_SIPARISLER.pro_fiyat_liste_no,PROFORMA_SIPARISLER.pro_Otv_Pntr,PROFORMA_SIPARISLER.pro_Otv_Vergi,PROFORMA_SIPARISLER.pro_otvtutari,PROFORMA_SIPARISLER.pro_OtvVergisiz_Fl,PROFORMA_SIPARISLER.pro_paket_kod,PROFORMA_SIPARISLER.pro_RezRecId_DBCno,PROFORMA_SIPARISLER.pro_RezRecId_RECno,PROFORMA_SIPARISLER.pro_harekettipi,PROFORMA_SIPARISLER.pro_yetkili_recid_dbcno,PROFORMA_SIPARISLER.pro_yetkili_recid_recno,PROFORMA_SIPARISLER.pro_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM PROFORMA_SIPARISLER INNER JOIN STOKLAR ON STOKLAR.sto_kod=PROFORMA_SIPARISLER.pro_stokkodu where pro_evrakno_seri=@EvrakSeri AND pro_evrakno_sira=@EvrakSira AND pro_cinsi=@sip_cins AND pro_tipi=@sip_tip ORDER BY pro_satirno";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@EvrakSeri", (object)EvrakSeri);
				val.Parameters.AddWithValue("@EvrakSira", (object)EvrakSira);
				val.Parameters.AddWithValue("@sip_cins", (object)(int)sip_cins);
				val.Parameters.AddWithValue("@sip_tip", (object)(int)sip_tip);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					SIPARISLER sIPARISLER = new SIPARISLER();
					sIPARISLER.sip_RECno = val2.GetSafeInt32(0);
					sIPARISLER.sip_RECid_DBCno = val2.GetSafeInt16(1);
					sIPARISLER.sip_RECid_RECno = val2.GetSafeInt32(2);
					sIPARISLER.sip_SpecRECno = val2.GetSafeInt32(3);
					sIPARISLER.sip_iptal = val2.GetSafeBoolean(4);
					sIPARISLER.sip_fileid = val2.GetSafeInt16(5);
					sIPARISLER.sip_hidden = val2.GetSafeBoolean(6);
					sIPARISLER.sip_kilitli = val2.GetSafeBoolean(7);
					sIPARISLER.sip_degisti = val2.GetSafeBoolean(8);
					sIPARISLER.sip_checksum = val2.GetSafeInt32(9);
					sIPARISLER.sip_create_user = val2.GetSafeInt16(10);
					sIPARISLER.sip_create_date = val2.GetSafeDateTime(11);
					sIPARISLER.sip_lastup_user = val2.GetSafeInt16(12);
					sIPARISLER.sip_special1 = val2.GetSafeString(13);
					sIPARISLER.sip_special2 = val2.GetSafeString(14);
					sIPARISLER.sip_special3 = val2.GetSafeString(15);
					sIPARISLER.sip_firmano = val2.GetSafeInt32(16);
					sIPARISLER.sip_subeno = val2.GetSafeInt32(17);
					sIPARISLER.sip_tarih = val2.GetSafeDateTime(18);
					sIPARISLER.sip_teslim_tarih = val2.GetSafeDateTime(19);
					sIPARISLER.sip_tip = (enum_sip_tip)val2.GetSafeByte(20);
					sIPARISLER.sip_cins = (enum_sip_cins)val2.GetSafeByte(21);
					sIPARISLER.sip_evrakno_seri = val2.GetSafeString(22);
					sIPARISLER.sip_evrakno_sira = val2.GetSafeInt32(23);
					sIPARISLER.sip_satirno = val2.GetSafeInt32(24);
					sIPARISLER.sip_belgeno = val2.GetSafeString(25);
					sIPARISLER.sip_belge_tarih = val2.GetSafeDateTime(26);
					sIPARISLER.sip_satici_kod = val2.GetSafeString(27);
					sIPARISLER.sip_musteri_kod = val2.GetSafeString(28);
					sIPARISLER.sip_stok_kod = val2.GetSafeString(29);
					sIPARISLER.sip_b_fiyat = val2.GetSafeDouble(30);
					sIPARISLER.sip_miktar = val2.GetSafeDouble(31);
					sIPARISLER.sip_birim_pntr = val2.GetSafeByte(32);
					sIPARISLER.sip_teslim_miktar = val2.GetSafeDouble(33);
					sIPARISLER.sip_tutar = val2.GetSafeDouble(34);
					sIPARISLER.sip_iskonto_1 = val2.GetSafeDouble(35);
					sIPARISLER.sip_iskonto_2 = val2.GetSafeDouble(36);
					sIPARISLER.sip_iskonto_3 = val2.GetSafeDouble(37);
					sIPARISLER.sip_iskonto_4 = val2.GetSafeDouble(38);
					sIPARISLER.sip_iskonto_5 = val2.GetSafeDouble(39);
					sIPARISLER.sip_iskonto_6 = val2.GetSafeDouble(40);
					sIPARISLER.sip_masraf_1 = val2.GetSafeDouble(41);
					sIPARISLER.sip_masraf_2 = val2.GetSafeDouble(42);
					sIPARISLER.sip_masraf_3 = val2.GetSafeDouble(43);
					sIPARISLER.sip_masraf_4 = val2.GetSafeDouble(44);
					sIPARISLER.sip_vergi_pntr = val2.GetSafeByte(45);
					sIPARISLER.sip_vergi = val2.GetSafeDouble(46);
					sIPARISLER.sip_masvergi_pntr = val2.GetSafeByte(47);
					sIPARISLER.sip_masvergi = val2.GetSafeDouble(48);
					sIPARISLER.sip_opno = val2.GetSafeInt32(49);
					sIPARISLER.sip_aciklama = val2.GetSafeString(50);
					sIPARISLER.sip_aciklama2 = val2.GetSafeString(51);
					sIPARISLER.sip_depono = val2.GetSafeInt32(52);
					sIPARISLER.sip_OnaylayanKulNo = val2.GetSafeInt16(53);
					sIPARISLER.sip_vergisiz_fl = val2.GetSafeBoolean(54);
					sIPARISLER.sip_kapat_fl = val2.GetSafeBoolean(55);
					sIPARISLER.sip_promosyon_fl = val2.GetSafeBoolean(56);
					sIPARISLER.sip_cari_sormerk = val2.GetSafeString(57);
					sIPARISLER.sip_stok_sormerk = val2.GetSafeString(58);
					sIPARISLER.sip_cari_grupno = val2.GetSafeByte(59);
					sIPARISLER.sip_doviz_cinsi = val2.GetSafeByte(60);
					sIPARISLER.sip_doviz_kuru = val2.GetSafeDouble(61);
					sIPARISLER.sip_alt_doviz_kuru = val2.GetSafeDouble(62);
					sIPARISLER.sip_adresno = val2.GetSafeInt32(63);
					sIPARISLER.sip_teslimturu = val2.GetSafeString(64);
					sIPARISLER.sip_cagrilabilir_fl = val2.GetSafeBoolean(65);
					sIPARISLER.sip_prosiprecDbId = val2.GetSafeInt16(66);
					sIPARISLER.sip_prosiprecrecI = val2.GetSafeInt32(67);
					sIPARISLER.sip_iskonto1 = val2.GetSafeByte(68);
					sIPARISLER.sip_iskonto2 = val2.GetSafeByte(69);
					sIPARISLER.sip_iskonto3 = val2.GetSafeByte(70);
					sIPARISLER.sip_iskonto4 = val2.GetSafeByte(71);
					sIPARISLER.sip_iskonto5 = val2.GetSafeByte(72);
					sIPARISLER.sip_iskonto6 = val2.GetSafeByte(73);
					sIPARISLER.sip_masraf1 = val2.GetSafeByte(74);
					sIPARISLER.sip_masraf2 = val2.GetSafeByte(75);
					sIPARISLER.sip_masraf3 = val2.GetSafeByte(76);
					sIPARISLER.sip_masraf4 = val2.GetSafeByte(77);
					sIPARISLER.sip_isk1 = val2.GetSafeBoolean(78);
					sIPARISLER.sip_isk2 = val2.GetSafeBoolean(79);
					sIPARISLER.sip_isk3 = val2.GetSafeBoolean(80);
					sIPARISLER.sip_isk4 = val2.GetSafeBoolean(81);
					sIPARISLER.sip_isk5 = val2.GetSafeBoolean(82);
					sIPARISLER.sip_isk6 = val2.GetSafeBoolean(83);
					sIPARISLER.sip_mas1 = val2.GetSafeBoolean(84);
					sIPARISLER.sip_mas2 = val2.GetSafeBoolean(85);
					sIPARISLER.sip_mas3 = val2.GetSafeBoolean(86);
					sIPARISLER.sip_mas4 = val2.GetSafeBoolean(87);
					sIPARISLER.sip_Exp_Imp_Kodu = val2.GetSafeString(88);
					sIPARISLER.sip_kar_orani = val2.GetSafeDouble(89);
					sIPARISLER.sip_durumu = (enum_sip_durumu)val2.GetSafeByte(90);
					sIPARISLER.sip_stalRecId_DBCno = val2.GetSafeInt16(91);
					sIPARISLER.sip_stalRecId_RECno = val2.GetSafeInt32(92);
					sIPARISLER.sip_planlananmiktar = val2.GetSafeDouble(93);
					sIPARISLER.sip_teklifRecId_DBCno = val2.GetSafeInt16(94);
					sIPARISLER.sip_teklifRecId_RECno = val2.GetSafeInt32(95);
					sIPARISLER.sip_parti_kodu = val2.GetSafeString(96);
					sIPARISLER.sip_lot_no = val2.GetSafeInt32(97);
					sIPARISLER.sip_projekodu = val2.GetSafeString(98);
					sIPARISLER.sip_fiyat_liste_no = val2.GetSafeInt32(99);
					sIPARISLER.sip_Otv_Pntr = val2.GetSafeByte(100);
					sIPARISLER.sip_Otv_Vergi = val2.GetSafeDouble(101);
					sIPARISLER.sip_otvtutari = val2.GetSafeDouble(102);
					sIPARISLER.sip_OtvVergisiz_Fl = val2.GetSafeByte(103);
					sIPARISLER.sip_paket_kod = val2.GetSafeString(104);
					sIPARISLER.sip_RezRecId_DBCno = val2.GetSafeInt16(105);
					sIPARISLER.sip_RezRecId_RECno = val2.GetSafeInt32(106);
					sIPARISLER.sip_harekettipi = (enum_sip_harekettipi)val2.GetSafeByte(107);
					sIPARISLER.sip_yetkili_recid_dbcno = val2.GetSafeInt16(108);
					sIPARISLER.sip_yetkili_recid_recno = val2.GetSafeInt32(109);
					sIPARISLER.sip_kapatmanedenkod = val2.GetSafeString(110);
					sIPARISLER.sto_isim = val2.GetSafeString(111);
					sIPARISLER.sto_birim1_ad = val2.GetSafeString(112);
					sIPARISLER.sto_birim2_ad = val2.GetSafeString(113);
					sIPARISLER.sto_birim3_ad = val2.GetSafeString(114);
					sIPARISLER.sto_birim4_ad = val2.GetSafeString(115);
					sIPARISLER.sto_birim1_katsayi = val2.GetSafeDouble(116);
					sIPARISLER.sto_birim2_katsayi = val2.GetSafeDouble(117);
					sIPARISLER.sto_birim3_katsayi = val2.GetSafeDouble(118);
					sIPARISLER.sto_birim4_katsayi = val2.GetSafeDouble(119);
					bool safeBoolean = val2.GetSafeBoolean(120);
					bool safeBoolean2 = val2.GetSafeBoolean(121);
					if (safeBoolean || safeBoolean2)
					{
						sIPARISLER.renk_beden_hareketleri = StokSqlite.V15_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.ProformaSiparis, sIPARISLER.sip_RECid_DBCno, sIPARISLER.sip_RECid_RECno);
					}
					evrak.AddSiparisHareketi(sIPARISLER, SiparisKarsilamaYap: false);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			if (evrak.GetSiparisler().Count > 0)
			{
				SIPARISLER sIPARISLER2 = evrak.GetSiparisler()[0];
				evrak.alternatifdovizkuru = new Kur();
				evrak.alternatifdovizkuru.dov_fiyat = sIPARISLER2.sip_alt_doviz_kuru;
				evrak.SetBelgeNo(sIPARISLER2.sip_belgeno);
				evrak.SetBelgeTarihi(sIPARISLER2.sip_belge_tarih);
				evrak.cari = CariSqlite.GetCariByCariKod(OpenedConnection, sIPARISLER2.sip_musteri_kod, AdreslerTemsilciyeGore: false, "");
				evrak.SetKaynakDepo(DepoSqlite.GetDepo(OpenedConnection, sIPARISLER2.sip_depono));
				evrak.SetDovizCinsi(sIPARISLER2.sip_doviz_cinsi);
				evrak.SetFiyatListesi(FiyatListesiSqlite.GetFiyatListesi(OpenedConnection, sIPARISLER2.sip_fiyat_liste_no));
				evrak.kur = new Kur();
				evrak.kur.dov_fiyat = sIPARISLER2.sip_doviz_kuru;
				evrak.SetProje(ProjeSqlite.GetProje(OpenedConnection, sIPARISLER2.sip_projekodu));
				evrak.SetSorumlulukMerkezi(SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(OpenedConnection, sIPARISLER2.sip_cari_sormerk));
				evrak.SetTemsilciKodu(sIPARISLER2.sip_satici_kod);
				evrak.SetEvrakKilitli(sIPARISLER2.sip_kilitli);
				evrak.SetEvrakNoSeri(EvrakSeri);
				evrak.SetEvraknoSira(EvrakSira);
				evrak.SetEvrakTarihi(sIPARISLER2.sip_tarih);
				evrak.evraktipi = enum_GenelEvrakTipleri.ProformaSiparis;
				evrak.SetFirma(FirmaSqlite.GetFirma(OpenedConnection, sIPARISLER2.sip_firmano));
				evrak.SetSube(SubeSqlite.GetSube(OpenedConnection, sIPARISLER2.sip_subeno));
				evrak.SetMikroUserNo(sIPARISLER2.sip_create_user);
				evrak.SetOdemePlani(sIPARISLER2.sip_opno);
				evrak.SetSevkAdresNo(sIPARISLER2.sip_adresno);
				evrak.SetYeniKayit(YeniDeger: false);
				if (evrak.GetSiparisler()[0].sip_kilitli)
				{
					evrak.SetEvrakKilitli(YeniDeger: true);
				}
				else
				{
					evrak.SetEvrakKilitli(YeniDeger: false);
				}
				commandText = "SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano=@egk_dosyano AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira";
				SqliteCommand val3 = new SqliteCommand(commandText);
				val3.Connection = OpenedConnection;
				val3.Parameters.AddWithValue("@egk_dosyano", (object)22);
				val3.Parameters.AddWithValue("@egk_hareket_tip", (object)0);
				val3.Parameters.AddWithValue("@egk_evr_tip", (object)2);
				val3.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.GetSiparisler()[0].sip_evrakno_seri);
				val3.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.GetSiparisler()[0].sip_evrakno_sira);
				SqliteDataReader val4 = val3.ExecuteReader();
				if (((DbDataReader)val4).HasRows)
				{
					while (((DbDataReader)val4).Read())
					{
						evrak.SetAciklama1(val4.GetSafeString(0));
						evrak.SetAciklama2(val4.GetSafeString(1));
						evrak.SetAciklama3(val4.GetSafeString(2));
						evrak.SetAciklama4(val4.GetSafeString(3));
						evrak.SetAciklama5(val4.GetSafeString(4));
						evrak.SetAciklama6(val4.GetSafeString(5));
						evrak.SetAciklama7(val4.GetSafeString(6));
						evrak.SetAciklama8(val4.GetSafeString(7));
						evrak.SetAciklama9(val4.GetSafeString(8));
						evrak.SetAciklama10(val4.GetSafeString(9));
					}
				}
				((DbDataReader)val4).Close();
				((DbDataReader)val4).Dispose();
				val4 = null;
				((Component)val3).Dispose();
				val3 = null;
			}
		}
		catch
		{
		}
		return evrak;
	}

	private static Evrak V16_GetProformaSiparisEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		//IL_0917: Unknown result type (might be due to invalid IL or missing references)
		//IL_091e: Expected O, but got Unknown
		Evrak evrak = new Evrak(enum_GenelEvrakTipleri.ProformaSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		string commandText = "SELECT PROFORMA_SIPARISLER.pro_Guid,0,0,PROFORMA_SIPARISLER.pro_SpecRecNo,PROFORMA_SIPARISLER.pro_iptal,PROFORMA_SIPARISLER.pro_fileid,PROFORMA_SIPARISLER.pro_hidden,PROFORMA_SIPARISLER.pro_kilitli,PROFORMA_SIPARISLER.pro_degisti,PROFORMA_SIPARISLER.pro_checksum,PROFORMA_SIPARISLER.pro_create_user,PROFORMA_SIPARISLER.pro_create_date,PROFORMA_SIPARISLER.pro_lastup_user,PROFORMA_SIPARISLER.pro_special1,PROFORMA_SIPARISLER.pro_special2,PROFORMA_SIPARISLER.pro_special3,PROFORMA_SIPARISLER.pro_firmano,PROFORMA_SIPARISLER.pro_subeno,PROFORMA_SIPARISLER.pro_tarihi,PROFORMA_SIPARISLER.pro_testarihi,PROFORMA_SIPARISLER.pro_tipi,PROFORMA_SIPARISLER.pro_cinsi,PROFORMA_SIPARISLER.pro_evrakno_seri,PROFORMA_SIPARISLER.pro_evrakno_sira,PROFORMA_SIPARISLER.pro_satirno,PROFORMA_SIPARISLER.pro_belge_no,PROFORMA_SIPARISLER.pro_belge_tarihi,PROFORMA_SIPARISLER.pro_saticikodu,PROFORMA_SIPARISLER.pro_mustkodu,PROFORMA_SIPARISLER.pro_stokkodu,PROFORMA_SIPARISLER.pro_bfiyati,PROFORMA_SIPARISLER.pro_miktar,PROFORMA_SIPARISLER.pro_birim_pntr,PROFORMA_SIPARISLER.pro_tesmiktari,PROFORMA_SIPARISLER.pro_tutari,PROFORMA_SIPARISLER.pro_iskonto1,PROFORMA_SIPARISLER.pro_iskonto2,PROFORMA_SIPARISLER.pro_iskonto3,PROFORMA_SIPARISLER.pro_iskonto4,PROFORMA_SIPARISLER.pro_iskonto5,PROFORMA_SIPARISLER.pro_iskonto6,PROFORMA_SIPARISLER.pro_masraf1,PROFORMA_SIPARISLER.pro_masraf2,PROFORMA_SIPARISLER.pro_masraf3,PROFORMA_SIPARISLER.pro_masraf4,PROFORMA_SIPARISLER.pro_vergipntr,PROFORMA_SIPARISLER.pro_vergi,PROFORMA_SIPARISLER.pro_masrafvergipntr,PROFORMA_SIPARISLER.pro_masrafvergi,PROFORMA_SIPARISLER.pro_opno,PROFORMA_SIPARISLER.pro_aciklama,PROFORMA_SIPARISLER.pro_aciklama2,PROFORMA_SIPARISLER.pro_depono,PROFORMA_SIPARISLER.pro_onaylayanKul_no,PROFORMA_SIPARISLER.pro_vergisiz,PROFORMA_SIPARISLER.pro_kapat,PROFORMA_SIPARISLER.pro_promosyon_fl,PROFORMA_SIPARISLER.pro_cari_sormerk,PROFORMA_SIPARISLER.pro_stok_sormerk,PROFORMA_SIPARISLER.pro_cari_grupno,PROFORMA_SIPARISLER.pro_dovizcinsi,PROFORMA_SIPARISLER.pro_dovizkuru,PROFORMA_SIPARISLER.pro_altdovizkuru,PROFORMA_SIPARISLER.pro_adresno,PROFORMA_SIPARISLER.pro_teslimturu,PROFORMA_SIPARISLER.pro_cagrilabilir_fl,0,PROFORMA_SIPARISLER.pro_sip_uid,PROFORMA_SIPARISLER.pro_isk_mas_1,PROFORMA_SIPARISLER.pro_isk_mas_2,PROFORMA_SIPARISLER.pro_isk_mas_3,PROFORMA_SIPARISLER.pro_isk_mas_4,PROFORMA_SIPARISLER.pro_isk_mas_5,PROFORMA_SIPARISLER.pro_isk_mas_6,PROFORMA_SIPARISLER.pro_isk_mas_7,PROFORMA_SIPARISLER.pro_isk_mas_8,PROFORMA_SIPARISLER.pro_isk_mas_9,PROFORMA_SIPARISLER.pro_isk_mas_10,PROFORMA_SIPARISLER.pro_sat_isk_mas1,PROFORMA_SIPARISLER.pro_sat_isk_mas2,PROFORMA_SIPARISLER.pro_sat_isk_mas3,PROFORMA_SIPARISLER.pro_sat_isk_mas4,PROFORMA_SIPARISLER.pro_sat_isk_mas5,PROFORMA_SIPARISLER.pro_sat_isk_mas6,PROFORMA_SIPARISLER.pro_sat_isk_mas7,PROFORMA_SIPARISLER.pro_sat_isk_mas8,PROFORMA_SIPARISLER.pro_sat_isk_mas9,PROFORMA_SIPARISLER.pro_sat_isk_mas10,PROFORMA_SIPARISLER.pro_Exp_Imp_Kodu,PROFORMA_SIPARISLER.pro_karoani,PROFORMA_SIPARISLER.pro_durumu,0,PROFORMA_SIPARISLER.pro_stal_uid,PROFORMA_SIPARISLER.pro_planlananmiktar,0,PROFORMA_SIPARISLER.pro_teklif_uid,PROFORMA_SIPARISLER.pro_parti_kodu,PROFORMA_SIPARISLER.pro_lot_no,PROFORMA_SIPARISLER.pro_projekodu,PROFORMA_SIPARISLER.pro_fiyat_liste_no,PROFORMA_SIPARISLER.pro_Otv_Pntr,PROFORMA_SIPARISLER.pro_Otv_Vergi,PROFORMA_SIPARISLER.pro_otvtutari,PROFORMA_SIPARISLER.pro_OtvVergisiz_Fl,PROFORMA_SIPARISLER.pro_paket_kod,0,PROFORMA_SIPARISLER.pro_Rez_uid,PROFORMA_SIPARISLER.pro_harekettipi,0,PROFORMA_SIPARISLER.pro_yetkili_uid,PROFORMA_SIPARISLER.pro_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM PROFORMA_SIPARISLER INNER JOIN STOKLAR ON STOKLAR.sto_kod=PROFORMA_SIPARISLER.pro_stokkodu where pro_evrakno_seri=@EvrakSeri AND pro_evrakno_sira=@EvrakSira AND pro_cinsi=@sip_cins AND pro_tipi=@sip_tip ORDER BY pro_satirno";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@EvrakSeri", (object)EvrakSeri);
				val.Parameters.AddWithValue("@EvrakSira", (object)EvrakSira);
				val.Parameters.AddWithValue("@sip_cins", (object)(int)sip_cins);
				val.Parameters.AddWithValue("@sip_tip", (object)(int)sip_tip);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					SIPARISLER sIPARISLER = new SIPARISLER();
					sIPARISLER.sip_Guid = ((DbDataReader)val2).GetGuid(0);
					sIPARISLER.sip_SpecRECno = val2.GetSafeInt32(3);
					sIPARISLER.sip_iptal = val2.GetSafeBoolean(4);
					sIPARISLER.sip_fileid = val2.GetSafeInt16(5);
					sIPARISLER.sip_hidden = val2.GetSafeBoolean(6);
					sIPARISLER.sip_kilitli = val2.GetSafeBoolean(7);
					sIPARISLER.sip_degisti = val2.GetSafeBoolean(8);
					sIPARISLER.sip_checksum = val2.GetSafeInt32(9);
					sIPARISLER.sip_create_user = val2.GetSafeInt16(10);
					sIPARISLER.sip_create_date = val2.GetSafeDateTime(11);
					sIPARISLER.sip_lastup_user = val2.GetSafeInt16(12);
					sIPARISLER.sip_special1 = val2.GetSafeString(13);
					sIPARISLER.sip_special2 = val2.GetSafeString(14);
					sIPARISLER.sip_special3 = val2.GetSafeString(15);
					sIPARISLER.sip_firmano = val2.GetSafeInt32(16);
					sIPARISLER.sip_subeno = val2.GetSafeInt32(17);
					sIPARISLER.sip_tarih = val2.GetSafeDateTime(18);
					sIPARISLER.sip_teslim_tarih = val2.GetSafeDateTime(19);
					sIPARISLER.sip_tip = (enum_sip_tip)val2.GetSafeByte(20);
					sIPARISLER.sip_cins = (enum_sip_cins)val2.GetSafeByte(21);
					sIPARISLER.sip_evrakno_seri = val2.GetSafeString(22);
					sIPARISLER.sip_evrakno_sira = val2.GetSafeInt32(23);
					sIPARISLER.sip_satirno = val2.GetSafeInt32(24);
					sIPARISLER.sip_belgeno = val2.GetSafeString(25);
					sIPARISLER.sip_belge_tarih = val2.GetSafeDateTime(26);
					sIPARISLER.sip_satici_kod = val2.GetSafeString(27);
					sIPARISLER.sip_musteri_kod = val2.GetSafeString(28);
					sIPARISLER.sip_stok_kod = val2.GetSafeString(29);
					sIPARISLER.sip_b_fiyat = val2.GetSafeDouble(30);
					sIPARISLER.sip_miktar = val2.GetSafeDouble(31);
					sIPARISLER.sip_birim_pntr = val2.GetSafeByte(32);
					sIPARISLER.sip_teslim_miktar = val2.GetSafeDouble(33);
					sIPARISLER.sip_tutar = val2.GetSafeDouble(34);
					sIPARISLER.sip_iskonto_1 = val2.GetSafeDouble(35);
					sIPARISLER.sip_iskonto_2 = val2.GetSafeDouble(36);
					sIPARISLER.sip_iskonto_3 = val2.GetSafeDouble(37);
					sIPARISLER.sip_iskonto_4 = val2.GetSafeDouble(38);
					sIPARISLER.sip_iskonto_5 = val2.GetSafeDouble(39);
					sIPARISLER.sip_iskonto_6 = val2.GetSafeDouble(40);
					sIPARISLER.sip_masraf_1 = val2.GetSafeDouble(41);
					sIPARISLER.sip_masraf_2 = val2.GetSafeDouble(42);
					sIPARISLER.sip_masraf_3 = val2.GetSafeDouble(43);
					sIPARISLER.sip_masraf_4 = val2.GetSafeDouble(44);
					sIPARISLER.sip_vergi_pntr = val2.GetSafeByte(45);
					sIPARISLER.sip_vergi = val2.GetSafeDouble(46);
					sIPARISLER.sip_masvergi_pntr = val2.GetSafeByte(47);
					sIPARISLER.sip_masvergi = val2.GetSafeDouble(48);
					sIPARISLER.sip_opno = val2.GetSafeInt32(49);
					sIPARISLER.sip_aciklama = val2.GetSafeString(50);
					sIPARISLER.sip_aciklama2 = val2.GetSafeString(51);
					sIPARISLER.sip_depono = val2.GetSafeInt32(52);
					sIPARISLER.sip_OnaylayanKulNo = val2.GetSafeInt16(53);
					sIPARISLER.sip_vergisiz_fl = val2.GetSafeBoolean(54);
					sIPARISLER.sip_kapat_fl = val2.GetSafeBoolean(55);
					sIPARISLER.sip_promosyon_fl = val2.GetSafeBoolean(56);
					sIPARISLER.sip_cari_sormerk = val2.GetSafeString(57);
					sIPARISLER.sip_stok_sormerk = val2.GetSafeString(58);
					sIPARISLER.sip_cari_grupno = val2.GetSafeByte(59);
					sIPARISLER.sip_doviz_cinsi = val2.GetSafeByte(60);
					sIPARISLER.sip_doviz_kuru = val2.GetSafeDouble(61);
					sIPARISLER.sip_alt_doviz_kuru = val2.GetSafeDouble(62);
					sIPARISLER.sip_adresno = val2.GetSafeInt32(63);
					sIPARISLER.sip_teslimturu = val2.GetSafeString(64);
					sIPARISLER.sip_cagrilabilir_fl = val2.GetSafeBoolean(65);
					sIPARISLER.sip_prosip_uid = ((DbDataReader)val2).GetGuid(67);
					sIPARISLER.sip_iskonto1 = val2.GetSafeByte(68);
					sIPARISLER.sip_iskonto2 = val2.GetSafeByte(69);
					sIPARISLER.sip_iskonto3 = val2.GetSafeByte(70);
					sIPARISLER.sip_iskonto4 = val2.GetSafeByte(71);
					sIPARISLER.sip_iskonto5 = val2.GetSafeByte(72);
					sIPARISLER.sip_iskonto6 = val2.GetSafeByte(73);
					sIPARISLER.sip_masraf1 = val2.GetSafeByte(74);
					sIPARISLER.sip_masraf2 = val2.GetSafeByte(75);
					sIPARISLER.sip_masraf3 = val2.GetSafeByte(76);
					sIPARISLER.sip_masraf4 = val2.GetSafeByte(77);
					sIPARISLER.sip_isk1 = val2.GetSafeBoolean(78);
					sIPARISLER.sip_isk2 = val2.GetSafeBoolean(79);
					sIPARISLER.sip_isk3 = val2.GetSafeBoolean(80);
					sIPARISLER.sip_isk4 = val2.GetSafeBoolean(81);
					sIPARISLER.sip_isk5 = val2.GetSafeBoolean(82);
					sIPARISLER.sip_isk6 = val2.GetSafeBoolean(83);
					sIPARISLER.sip_mas1 = val2.GetSafeBoolean(84);
					sIPARISLER.sip_mas2 = val2.GetSafeBoolean(85);
					sIPARISLER.sip_mas3 = val2.GetSafeBoolean(86);
					sIPARISLER.sip_mas4 = val2.GetSafeBoolean(87);
					sIPARISLER.sip_Exp_Imp_Kodu = val2.GetSafeString(88);
					sIPARISLER.sip_kar_orani = val2.GetSafeDouble(89);
					sIPARISLER.sip_durumu = (enum_sip_durumu)val2.GetSafeByte(90);
					sIPARISLER.sip_stal_uid = ((DbDataReader)val2).GetGuid(92);
					sIPARISLER.sip_planlananmiktar = val2.GetSafeDouble(93);
					sIPARISLER.sip_teklif_uid = ((DbDataReader)val2).GetGuid(95);
					sIPARISLER.sip_parti_kodu = val2.GetSafeString(96);
					sIPARISLER.sip_lot_no = val2.GetSafeInt32(97);
					sIPARISLER.sip_projekodu = val2.GetSafeString(98);
					sIPARISLER.sip_fiyat_liste_no = val2.GetSafeInt32(99);
					sIPARISLER.sip_Otv_Pntr = val2.GetSafeByte(100);
					sIPARISLER.sip_Otv_Vergi = val2.GetSafeDouble(101);
					sIPARISLER.sip_otvtutari = val2.GetSafeDouble(102);
					sIPARISLER.sip_OtvVergisiz_Fl = val2.GetSafeByte(103);
					sIPARISLER.sip_paket_kod = val2.GetSafeString(104);
					sIPARISLER.sip_Rez_uid = ((DbDataReader)val2).GetGuid(106);
					sIPARISLER.sip_harekettipi = (enum_sip_harekettipi)val2.GetSafeByte(107);
					sIPARISLER.sip_yetkili_uid = ((DbDataReader)val2).GetGuid(109);
					sIPARISLER.sip_kapatmanedenkod = val2.GetSafeString(110);
					sIPARISLER.sto_isim = val2.GetSafeString(111);
					sIPARISLER.sto_birim1_ad = val2.GetSafeString(112);
					sIPARISLER.sto_birim2_ad = val2.GetSafeString(113);
					sIPARISLER.sto_birim3_ad = val2.GetSafeString(114);
					sIPARISLER.sto_birim4_ad = val2.GetSafeString(115);
					sIPARISLER.sto_birim1_katsayi = val2.GetSafeDouble(116);
					sIPARISLER.sto_birim2_katsayi = val2.GetSafeDouble(117);
					sIPARISLER.sto_birim3_katsayi = val2.GetSafeDouble(118);
					sIPARISLER.sto_birim4_katsayi = val2.GetSafeDouble(119);
					bool safeBoolean = val2.GetSafeBoolean(120);
					bool safeBoolean2 = val2.GetSafeBoolean(121);
					if (safeBoolean || safeBoolean2)
					{
						sIPARISLER.renk_beden_hareketleri = StokSqlite.V16_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.ProformaSiparis, sIPARISLER.sip_Guid);
					}
					evrak.AddSiparisHareketi(sIPARISLER, SiparisKarsilamaYap: false);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			if (evrak.GetSiparisler().Count > 0)
			{
				SIPARISLER sIPARISLER2 = evrak.GetSiparisler()[0];
				evrak.alternatifdovizkuru = new Kur();
				evrak.alternatifdovizkuru.dov_fiyat = sIPARISLER2.sip_alt_doviz_kuru;
				evrak.SetBelgeNo(sIPARISLER2.sip_belgeno);
				evrak.SetBelgeTarihi(sIPARISLER2.sip_belge_tarih);
				evrak.cari = CariSqlite.GetCariByCariKod(OpenedConnection, sIPARISLER2.sip_musteri_kod, AdreslerTemsilciyeGore: false, "");
				evrak.SetKaynakDepo(DepoSqlite.GetDepo(OpenedConnection, sIPARISLER2.sip_depono));
				evrak.SetDovizCinsi(sIPARISLER2.sip_doviz_cinsi);
				evrak.SetFiyatListesi(FiyatListesiSqlite.GetFiyatListesi(OpenedConnection, sIPARISLER2.sip_fiyat_liste_no));
				evrak.kur = new Kur();
				evrak.kur.dov_fiyat = sIPARISLER2.sip_doviz_kuru;
				evrak.SetProje(ProjeSqlite.GetProje(OpenedConnection, sIPARISLER2.sip_projekodu));
				evrak.SetSorumlulukMerkezi(SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(OpenedConnection, sIPARISLER2.sip_cari_sormerk));
				evrak.SetTemsilciKodu(sIPARISLER2.sip_satici_kod);
				evrak.SetEvrakKilitli(sIPARISLER2.sip_kilitli);
				evrak.SetEvrakNoSeri(EvrakSeri);
				evrak.SetEvraknoSira(EvrakSira);
				evrak.SetEvrakTarihi(sIPARISLER2.sip_tarih);
				evrak.evraktipi = enum_GenelEvrakTipleri.ProformaSiparis;
				evrak.SetFirma(FirmaSqlite.GetFirma(OpenedConnection, sIPARISLER2.sip_firmano));
				evrak.SetSube(SubeSqlite.GetSube(OpenedConnection, sIPARISLER2.sip_subeno));
				evrak.SetMikroUserNo(sIPARISLER2.sip_create_user);
				evrak.SetOdemePlani(sIPARISLER2.sip_opno);
				evrak.SetSevkAdresNo(sIPARISLER2.sip_adresno);
				evrak.SetYeniKayit(YeniDeger: false);
				if (evrak.GetSiparisler()[0].sip_kilitli)
				{
					evrak.SetEvrakKilitli(YeniDeger: true);
				}
				else
				{
					evrak.SetEvrakKilitli(YeniDeger: false);
				}
				commandText = "SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano=@egk_dosyano AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira";
				SqliteCommand val3 = new SqliteCommand(commandText);
				val3.Connection = OpenedConnection;
				val3.Parameters.AddWithValue("@egk_dosyano", (object)22);
				val3.Parameters.AddWithValue("@egk_hareket_tip", (object)0);
				val3.Parameters.AddWithValue("@egk_evr_tip", (object)2);
				val3.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.GetSiparisler()[0].sip_evrakno_seri);
				val3.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.GetSiparisler()[0].sip_evrakno_sira);
				SqliteDataReader val4 = val3.ExecuteReader();
				if (((DbDataReader)val4).HasRows)
				{
					while (((DbDataReader)val4).Read())
					{
						evrak.SetAciklama1(val4.GetSafeString(0));
						evrak.SetAciklama2(val4.GetSafeString(1));
						evrak.SetAciklama3(val4.GetSafeString(2));
						evrak.SetAciklama4(val4.GetSafeString(3));
						evrak.SetAciklama5(val4.GetSafeString(4));
						evrak.SetAciklama6(val4.GetSafeString(5));
						evrak.SetAciklama7(val4.GetSafeString(6));
						evrak.SetAciklama8(val4.GetSafeString(7));
						evrak.SetAciklama9(val4.GetSafeString(8));
						evrak.SetAciklama10(val4.GetSafeString(9));
					}
				}
				((DbDataReader)val4).Close();
				((DbDataReader)val4).Dispose();
				val4 = null;
				((Component)val3).Dispose();
				val3 = null;
			}
		}
		catch
		{
		}
		return evrak;
	}

	private static Evrak GetIrsaliyeEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_sth_evraktip sth_evraktip, int AlternatifDovizCinsi, enum_sth_cins sth_cins)
	{
		if (GenelUtility.GetMikroVersiyon(((DbConnection)OpenedConnection).DataSource) > 15)
		{
			return V16_GetIrsaliyeEvrak(OpenedConnection, EvrakSeri, EvrakSira, sth_evraktip, AlternatifDovizCinsi, sth_cins);
		}
		return V15_GetIrsaliyeEvrak(OpenedConnection, EvrakSeri, EvrakSira, sth_evraktip, AlternatifDovizCinsi, sth_cins);
	}

	private static Evrak V15_GetIrsaliyeEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_sth_evraktip sth_evraktip, int AlternatifDovizCinsi, enum_sth_cins sth_cins)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0697: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Expected O, but got Unknown
		Evrak evrak = new Evrak();
		evrak.SetSiparisKarsilamaMi(YeniDeger: false);
		evrak.SetYeniKayit(YeniDeger: false);
		evrak.evraktipi = enum_GenelEvrakTipleri.Tanimsiz;
		SqliteCommand val = new SqliteCommand("SELECT STOK_HAREKETLERI.sth_RECno,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,STOK_HAREKETLERI.sth_sip_recid_dbcno,STOK_HAREKETLERI.sth_sip_recid_recno,STOK_HAREKETLERI.sth_fat_recid_dbcno,STOK_HAREKETLERI.sth_fat_recid_recno,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOK_HAREKETLERI.sth_cari_srm_merkezi,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6,sth_aciklama,sth_exim_kodu,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM STOK_HAREKETLERI WITH(NOLOCK) INNER JOIN STOKLAR WITH(NOLOCK) ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod WHERE sth_evraktip=@sth_evraktip AND sth_evrakno_seri=@sth_evrakno_seri AND sth_evrakno_sira=@sth_evrakno_sira AND sth_cins=@sth_cins", OpenedConnection);
		val.Parameters.AddWithValue("@sth_evraktip", (object)(int)sth_evraktip);
		val.Parameters.AddWithValue("@sth_evrakno_seri", (object)EvrakSeri);
		val.Parameters.AddWithValue("@sth_evrakno_sira", (object)EvrakSira);
		val.Parameters.AddWithValue("@sth_cins", (object)(int)sth_cins);
		SqliteDataReader val2 = val.ExecuteReader();
		if (((DbDataReader)val2).HasRows)
		{
			while (((DbDataReader)val2).Read())
			{
				STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
				sTOK_HAREKETLERI.sth_RECno = val2.GetSafeInt32(0);
				sTOK_HAREKETLERI.sth_cari_kodu = val2.GetSafeString(1);
				sTOK_HAREKETLERI.sth_stok_kod = val2.GetSafeString(2);
				sTOK_HAREKETLERI.sth_evrakno_seri = val2.GetSafeString(3);
				sTOK_HAREKETLERI.sth_evrakno_sira = val2.GetSafeInt32(4);
				sTOK_HAREKETLERI.sth_plasiyer_kodu = val2.GetSafeString(5);
				sTOK_HAREKETLERI.sth_miktar = val2.GetSafeDouble(6);
				sTOK_HAREKETLERI.sth_miktar2 = val2.GetSafeDouble(7);
				sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)val2.GetSafeByte(8);
				sTOK_HAREKETLERI.sth_giris_depo_no = val2.GetSafeInt32(9);
				sTOK_HAREKETLERI.sth_cikis_depo_no = val2.GetSafeInt32(10);
				sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)val2.GetSafeByte(11);
				sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)val2.GetSafeByte(12);
				sTOK_HAREKETLERI.sth_satirno = val2.GetSafeInt32(13);
				sTOK_HAREKETLERI.sth_sip_recid_dbcno = val2.GetSafeInt16(14);
				sTOK_HAREKETLERI.sth_sip_recid_recno = val2.GetSafeInt32(15);
				sTOK_HAREKETLERI.sth_fat_recid_dbcno = val2.GetSafeInt16(16);
				sTOK_HAREKETLERI.sth_fat_recid_recno = val2.GetSafeInt32(17);
				sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)val2.GetSafeByte(18);
				sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)val2.GetSafeByte(19);
				sTOK_HAREKETLERI.sth_lastup_date = val2.GetSafeDateTime(20);
				sTOK_HAREKETLERI.sth_tarih = val2.GetSafeDateTime(21);
				sTOK_HAREKETLERI.sth_belge_tarih = val2.GetSafeDateTime(22);
				sTOK_HAREKETLERI.sth_tutar = val2.GetSafeDouble(23);
				sTOK_HAREKETLERI.sth_vergi = val2.GetSafeDouble(24);
				sTOK_HAREKETLERI.sth_har_doviz_kuru = val2.GetSafeDouble(25);
				sTOK_HAREKETLERI.sth_iskonto1 = val2.GetSafeDouble(26);
				sTOK_HAREKETLERI.sth_iskonto2 = val2.GetSafeDouble(27);
				sTOK_HAREKETLERI.sth_iskonto3 = val2.GetSafeDouble(28);
				sTOK_HAREKETLERI.sth_iskonto4 = val2.GetSafeDouble(28);
				sTOK_HAREKETLERI.sth_iskonto5 = val2.GetSafeDouble(30);
				sTOK_HAREKETLERI.sth_iskonto6 = val2.GetSafeDouble(31);
				sTOK_HAREKETLERI.sth_masraf1 = val2.GetSafeDouble(32);
				sTOK_HAREKETLERI.sth_masraf2 = val2.GetSafeDouble(33);
				sTOK_HAREKETLERI.sth_masraf3 = val2.GetSafeDouble(34);
				sTOK_HAREKETLERI.sth_masraf4 = val2.GetSafeDouble(35);
				sTOK_HAREKETLERI.sth_masraf_vergi = val2.GetSafeDouble(36);
				sTOK_HAREKETLERI.sth_vergi_pntr = val2.GetSafeByte(37);
				sTOK_HAREKETLERI.sth_har_doviz_cinsi = val2.GetSafeByte(38);
				sTOK_HAREKETLERI.sth_alt_doviz_kuru = val2.GetSafeDouble(39);
				sTOK_HAREKETLERI.sth_stok_doviz_cinsi = val2.GetSafeByte(40);
				sTOK_HAREKETLERI.sth_stok_doviz_kuru = val2.GetSafeDouble(41);
				sTOK_HAREKETLERI.sth_birim_pntr = val2.GetSafeByte(42);
				sTOK_HAREKETLERI.sth_fiyat_liste_no = val2.GetSafeInt32(43);
				sTOK_HAREKETLERI.sth_adres_no = val2.GetSafeInt32(44);
				sTOK_HAREKETLERI.sto_isim = val2.GetSafeString(45);
				sTOK_HAREKETLERI.sto_birim1_ad = val2.GetSafeString(46);
				sTOK_HAREKETLERI.sto_birim2_ad = val2.GetSafeString(47);
				sTOK_HAREKETLERI.sto_birim3_ad = val2.GetSafeString(48);
				sTOK_HAREKETLERI.sto_birim4_ad = val2.GetSafeString(49);
				sTOK_HAREKETLERI.sto_birim1_katsayi = val2.GetSafeDouble(50);
				sTOK_HAREKETLERI.sto_birim2_katsayi = val2.GetSafeDouble(51);
				sTOK_HAREKETLERI.sto_birim3_katsayi = val2.GetSafeDouble(52);
				sTOK_HAREKETLERI.sto_birim4_katsayi = val2.GetSafeDouble(53);
				sTOK_HAREKETLERI.sth_cari_srm_merkezi = val2.GetSafeString(54);
				sTOK_HAREKETLERI.sth_isk_mas1 = val2.GetSafeByte(55);
				sTOK_HAREKETLERI.sth_isk_mas2 = val2.GetSafeByte(56);
				sTOK_HAREKETLERI.sth_isk_mas3 = val2.GetSafeByte(57);
				sTOK_HAREKETLERI.sth_isk_mas4 = val2.GetSafeByte(58);
				sTOK_HAREKETLERI.sth_isk_mas5 = val2.GetSafeByte(59);
				sTOK_HAREKETLERI.sth_isk_mas6 = val2.GetSafeByte(60);
				sTOK_HAREKETLERI.sth_aciklama = val2.GetSafeString(61);
				sTOK_HAREKETLERI.sth_exim_kodu = val2.GetSafeString(62);
				bool safeBoolean = val2.GetSafeBoolean(63);
				bool safeBoolean2 = val2.GetSafeBoolean(64);
				if (safeBoolean || safeBoolean2)
				{
					sTOK_HAREKETLERI.renk_beden_hareketleri = StokSqlite.V15_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.StokHareket, sTOK_HAREKETLERI.sth_RECid_DBCno, sTOK_HAREKETLERI.sth_RECid_RECno);
				}
				evrak.AddStokHareketi(sTOK_HAREKETLERI);
			}
		}
		((DbDataReader)val2).Close();
		((DbDataReader)val2).Dispose();
		val2 = null;
		((Component)val).Dispose();
		val = null;
		switch (sth_evraktip)
		{
		case enum_sth_evraktip.CikisIrsaliyesi:
			evrak.evraktipi = enum_GenelEvrakTipleri.SatisIrsaliyesi;
			break;
		case enum_sth_evraktip.GirisIrsaliyesi:
			evrak.evraktipi = enum_GenelEvrakTipleri.AlisIrsaliyesi;
			break;
		case enum_sth_evraktip.DepolarArasiNakliyeFisi:
			evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi;
			break;
		case enum_sth_evraktip.DepoTransferFisi:
			evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiSevk;
			break;
		}
		if (evrak.GetStokHareketleri().Count > 0)
		{
			STOK_HAREKETLERI sTOK_HAREKETLERI2 = evrak.GetStokHareketleri()[0];
			evrak.SetAciklama(sTOK_HAREKETLERI2.sth_aciklama);
			evrak.SetAlternatifDovizCinsi(AlternatifDovizCinsi);
			evrak.SetKapamaHesapKodu("");
			evrak.SetDovizCinsi(sTOK_HAREKETLERI2.sth_har_doviz_cinsi);
			evrak.kur = new Kur();
			evrak.kur.dov_fiyat = sTOK_HAREKETLERI2.sth_har_doviz_kuru;
			evrak.kur.dov_no = sTOK_HAREKETLERI2.sth_har_doviz_cinsi;
			evrak.kur.dov_tarih = sTOK_HAREKETLERI2.sth_tarih;
			evrak.SetProje(ProjeSqlite.GetProje(OpenedConnection, sTOK_HAREKETLERI2.sth_proje_kodu));
			evrak.SetSorumlulukMerkezi(SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(OpenedConnection, sTOK_HAREKETLERI2.sth_cari_srm_merkezi));
			evrak.SetTemsilciKodu(sTOK_HAREKETLERI2.sth_plasiyer_kodu);
			evrak.alternatifdovizkuru = new Kur();
			evrak.alternatifdovizkuru.dov_fiyat = sTOK_HAREKETLERI2.sth_alt_doviz_kuru;
			evrak.alternatifdovizkuru.dov_no = AlternatifDovizCinsi;
			evrak.SetBelgeNo(sTOK_HAREKETLERI2.sth_belge_no);
			evrak.SetBelgeTarihi(sTOK_HAREKETLERI2.sth_belge_tarih);
			evrak.cari = CariSqlite.GetCariByCariKod(OpenedConnection, sTOK_HAREKETLERI2.sth_cari_kodu, AdreslerTemsilciyeGore: false, "");
			evrak.SetEvrakKilitli(sTOK_HAREKETLERI2.sth_kilitli);
			evrak.SetEvrakNoSeri(sTOK_HAREKETLERI2.sth_evrakno_seri);
			evrak.SetEvraknoSira(sTOK_HAREKETLERI2.sth_evrakno_sira);
			evrak.SetEvrakTarihi(sTOK_HAREKETLERI2.sth_tarih);
			evrak.SetFirma(FirmaSqlite.GetFirma(OpenedConnection, sTOK_HAREKETLERI2.sth_firmano));
			evrak.SetSube(SubeSqlite.GetSube(OpenedConnection, sTOK_HAREKETLERI2.sth_subeno));
			evrak.SetMikroUserNo(sTOK_HAREKETLERI2.sth_create_user);
			evrak.SetOdemePlani(sTOK_HAREKETLERI2.sth_odeme_op);
			evrak.SetFiyatListesi(FiyatListesiSqlite.GetFiyatListesi(OpenedConnection, sTOK_HAREKETLERI2.sth_fiyat_liste_no));
			evrak.SetHedefDepo(DepoSqlite.GetDepo(OpenedConnection, sTOK_HAREKETLERI2.sth_giris_depo_no));
			evrak.SetKaynakDepo(DepoSqlite.GetDepo(OpenedConnection, sTOK_HAREKETLERI2.sth_cikis_depo_no));
			evrak.SetNakliyeDepo(DepoSqlite.GetDepo(OpenedConnection, sTOK_HAREKETLERI2.sth_nakliyedeposu));
			evrak.SetSevkAdresNo(sTOK_HAREKETLERI2.sth_adres_no);
			evrak.SetEximKodu(sTOK_HAREKETLERI2.sth_exim_kodu);
		}
		val = new SqliteCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano=16 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
		switch (evrak.evraktipi)
		{
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			val.Parameters.AddWithValue("@egk_hareket_tip", (object)1);
			val.Parameters.AddWithValue("@egk_evr_tip", (object)1);
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			val.Parameters.AddWithValue("@egk_hareket_tip", (object)0);
			val.Parameters.AddWithValue("@egk_evr_tip", (object)13);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
			val.Parameters.AddWithValue("@egk_hareket_tip", (object)2);
			val.Parameters.AddWithValue("@egk_evr_tip", (object)17);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSevk:
			val.Parameters.AddWithValue("@egk_hareket_tip", (object)2);
			val.Parameters.AddWithValue("@egk_evr_tip", (object)2);
			break;
		}
		val.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.EvrakNoSeri);
		val.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.EvrakNoSira);
		val.Connection = OpenedConnection;
		val2 = val.ExecuteReader();
		if (((DbDataReader)val2).HasRows)
		{
			while (((DbDataReader)val2).Read())
			{
				evrak.SetAciklama1(val2.GetSafeString(0));
				evrak.SetAciklama2(val2.GetSafeString(1));
				evrak.SetAciklama3(val2.GetSafeString(2));
				evrak.SetAciklama4(val2.GetSafeString(3));
				evrak.SetAciklama5(val2.GetSafeString(4));
				evrak.SetAciklama6(val2.GetSafeString(5));
				evrak.SetAciklama7(val2.GetSafeString(6));
				evrak.SetAciklama8(val2.GetSafeString(7));
				evrak.SetAciklama9(val2.GetSafeString(8));
				evrak.SetAciklama10(val2.GetSafeString(9));
			}
		}
		((DbDataReader)val2).Close();
		((DbDataReader)val2).Dispose();
		val2 = null;
		((Component)val).Dispose();
		val = null;
		return evrak;
	}

	private static Evrak V16_GetIrsaliyeEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_sth_evraktip sth_evraktip, int AlternatifDovizCinsi, enum_sth_cins sth_cins)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Expected O, but got Unknown
		Evrak evrak = new Evrak();
		evrak.SetSiparisKarsilamaMi(YeniDeger: false);
		evrak.SetYeniKayit(YeniDeger: false);
		evrak.evraktipi = enum_GenelEvrakTipleri.Tanimsiz;
		SqliteCommand val = new SqliteCommand("SELECT STOK_HAREKETLERI.sth_Guid,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,0,STOK_HAREKETLERI.sth_sip_uid,0,STOK_HAREKETLERI.sth_fat_uid,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOK_HAREKETLERI.sth_cari_srm_merkezi,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6,sth_aciklama,sth_exim_kodu,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM STOK_HAREKETLERI INNER JOIN STOKLAR ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod WHERE sth_evraktip=@sth_evraktip AND sth_evrakno_seri=@sth_evrakno_seri AND sth_evrakno_sira=@sth_evrakno_sira AND sth_cins=@sth_cins", OpenedConnection);
		val.Parameters.AddWithValue("@sth_evraktip", (object)(int)sth_evraktip);
		val.Parameters.AddWithValue("@sth_evrakno_seri", (object)EvrakSeri);
		val.Parameters.AddWithValue("@sth_evrakno_sira", (object)EvrakSira);
		val.Parameters.AddWithValue("@sth_cins", (object)(int)sth_cins);
		SqliteDataReader val2 = val.ExecuteReader();
		if (((DbDataReader)val2).HasRows)
		{
			while (((DbDataReader)val2).Read())
			{
				STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
				sTOK_HAREKETLERI.sth_Guid = ((DbDataReader)val2).GetGuid(0);
				sTOK_HAREKETLERI.sth_cari_kodu = val2.GetSafeString(1);
				sTOK_HAREKETLERI.sth_stok_kod = val2.GetSafeString(2);
				sTOK_HAREKETLERI.sth_evrakno_seri = val2.GetSafeString(3);
				sTOK_HAREKETLERI.sth_evrakno_sira = val2.GetSafeInt32(4);
				sTOK_HAREKETLERI.sth_plasiyer_kodu = val2.GetSafeString(5);
				sTOK_HAREKETLERI.sth_miktar = val2.GetSafeDouble(6);
				sTOK_HAREKETLERI.sth_miktar2 = val2.GetSafeDouble(7);
				sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)val2.GetSafeByte(8);
				sTOK_HAREKETLERI.sth_giris_depo_no = val2.GetSafeInt32(9);
				sTOK_HAREKETLERI.sth_cikis_depo_no = val2.GetSafeInt32(10);
				sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)val2.GetSafeByte(11);
				sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)val2.GetSafeByte(12);
				sTOK_HAREKETLERI.sth_satirno = val2.GetSafeInt32(13);
				sTOK_HAREKETLERI.sth_sip_uid = ((DbDataReader)val2).GetGuid(15);
				sTOK_HAREKETLERI.sth_fat_uid = ((DbDataReader)val2).GetGuid(17);
				sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)val2.GetSafeByte(18);
				sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)val2.GetSafeByte(19);
				sTOK_HAREKETLERI.sth_lastup_date = val2.GetSafeDateTime(20);
				sTOK_HAREKETLERI.sth_tarih = val2.GetSafeDateTime(21);
				sTOK_HAREKETLERI.sth_belge_tarih = val2.GetSafeDateTime(22);
				sTOK_HAREKETLERI.sth_tutar = val2.GetSafeDouble(23);
				sTOK_HAREKETLERI.sth_vergi = val2.GetSafeDouble(24);
				sTOK_HAREKETLERI.sth_har_doviz_kuru = val2.GetSafeDouble(25);
				sTOK_HAREKETLERI.sth_iskonto1 = val2.GetSafeDouble(26);
				sTOK_HAREKETLERI.sth_iskonto2 = val2.GetSafeDouble(27);
				sTOK_HAREKETLERI.sth_iskonto3 = val2.GetSafeDouble(28);
				sTOK_HAREKETLERI.sth_iskonto4 = val2.GetSafeDouble(28);
				sTOK_HAREKETLERI.sth_iskonto5 = val2.GetSafeDouble(30);
				sTOK_HAREKETLERI.sth_iskonto6 = val2.GetSafeDouble(31);
				sTOK_HAREKETLERI.sth_masraf1 = val2.GetSafeDouble(32);
				sTOK_HAREKETLERI.sth_masraf2 = val2.GetSafeDouble(33);
				sTOK_HAREKETLERI.sth_masraf3 = val2.GetSafeDouble(34);
				sTOK_HAREKETLERI.sth_masraf4 = val2.GetSafeDouble(35);
				sTOK_HAREKETLERI.sth_masraf_vergi = val2.GetSafeDouble(36);
				sTOK_HAREKETLERI.sth_vergi_pntr = val2.GetSafeByte(37);
				sTOK_HAREKETLERI.sth_har_doviz_cinsi = val2.GetSafeByte(38);
				sTOK_HAREKETLERI.sth_alt_doviz_kuru = val2.GetSafeDouble(39);
				sTOK_HAREKETLERI.sth_stok_doviz_cinsi = val2.GetSafeByte(40);
				sTOK_HAREKETLERI.sth_stok_doviz_kuru = val2.GetSafeDouble(41);
				sTOK_HAREKETLERI.sth_birim_pntr = val2.GetSafeByte(42);
				sTOK_HAREKETLERI.sth_fiyat_liste_no = val2.GetSafeInt32(43);
				sTOK_HAREKETLERI.sth_adres_no = val2.GetSafeInt32(44);
				sTOK_HAREKETLERI.sto_isim = val2.GetSafeString(45);
				sTOK_HAREKETLERI.sto_birim1_ad = val2.GetSafeString(46);
				sTOK_HAREKETLERI.sto_birim2_ad = val2.GetSafeString(47);
				sTOK_HAREKETLERI.sto_birim3_ad = val2.GetSafeString(48);
				sTOK_HAREKETLERI.sto_birim4_ad = val2.GetSafeString(49);
				sTOK_HAREKETLERI.sto_birim1_katsayi = val2.GetSafeDouble(50);
				sTOK_HAREKETLERI.sto_birim2_katsayi = val2.GetSafeDouble(51);
				sTOK_HAREKETLERI.sto_birim3_katsayi = val2.GetSafeDouble(52);
				sTOK_HAREKETLERI.sto_birim4_katsayi = val2.GetSafeDouble(53);
				sTOK_HAREKETLERI.sth_cari_srm_merkezi = val2.GetSafeString(54);
				sTOK_HAREKETLERI.sth_isk_mas1 = val2.GetSafeByte(55);
				sTOK_HAREKETLERI.sth_isk_mas2 = val2.GetSafeByte(56);
				sTOK_HAREKETLERI.sth_isk_mas3 = val2.GetSafeByte(57);
				sTOK_HAREKETLERI.sth_isk_mas4 = val2.GetSafeByte(58);
				sTOK_HAREKETLERI.sth_isk_mas5 = val2.GetSafeByte(59);
				sTOK_HAREKETLERI.sth_isk_mas6 = val2.GetSafeByte(60);
				sTOK_HAREKETLERI.sth_aciklama = val2.GetSafeString(61);
				sTOK_HAREKETLERI.sth_exim_kodu = val2.GetSafeString(62);
				bool safeBoolean = val2.GetSafeBoolean(63);
				bool safeBoolean2 = val2.GetSafeBoolean(64);
				if (safeBoolean || safeBoolean2)
				{
					sTOK_HAREKETLERI.renk_beden_hareketleri = StokSqlite.V16_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.StokHareket, sTOK_HAREKETLERI.sth_Guid);
				}
				evrak.AddStokHareketi(sTOK_HAREKETLERI);
			}
		}
		((DbDataReader)val2).Close();
		((DbDataReader)val2).Dispose();
		val2 = null;
		((Component)val).Dispose();
		val = null;
		switch (sth_evraktip)
		{
		case enum_sth_evraktip.CikisIrsaliyesi:
			evrak.evraktipi = enum_GenelEvrakTipleri.SatisIrsaliyesi;
			break;
		case enum_sth_evraktip.GirisIrsaliyesi:
			evrak.evraktipi = enum_GenelEvrakTipleri.AlisIrsaliyesi;
			break;
		case enum_sth_evraktip.DepolarArasiNakliyeFisi:
			evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi;
			break;
		case enum_sth_evraktip.DepoTransferFisi:
			evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiSevk;
			break;
		}
		if (evrak.GetStokHareketleri().Count > 0)
		{
			STOK_HAREKETLERI sTOK_HAREKETLERI2 = evrak.GetStokHareketleri()[0];
			evrak.SetAciklama(sTOK_HAREKETLERI2.sth_aciklama);
			evrak.SetAlternatifDovizCinsi(AlternatifDovizCinsi);
			evrak.SetKapamaHesapKodu("");
			evrak.SetDovizCinsi(sTOK_HAREKETLERI2.sth_har_doviz_cinsi);
			evrak.kur = new Kur();
			evrak.kur.dov_fiyat = sTOK_HAREKETLERI2.sth_har_doviz_kuru;
			evrak.kur.dov_no = sTOK_HAREKETLERI2.sth_har_doviz_cinsi;
			evrak.kur.dov_tarih = sTOK_HAREKETLERI2.sth_tarih;
			evrak.SetProje(ProjeSqlite.GetProje(OpenedConnection, sTOK_HAREKETLERI2.sth_proje_kodu));
			evrak.SetSorumlulukMerkezi(SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(OpenedConnection, sTOK_HAREKETLERI2.sth_cari_srm_merkezi));
			evrak.SetTemsilciKodu(sTOK_HAREKETLERI2.sth_plasiyer_kodu);
			evrak.alternatifdovizkuru = new Kur();
			evrak.alternatifdovizkuru.dov_fiyat = sTOK_HAREKETLERI2.sth_alt_doviz_kuru;
			evrak.alternatifdovizkuru.dov_no = AlternatifDovizCinsi;
			evrak.SetBelgeNo(sTOK_HAREKETLERI2.sth_belge_no);
			evrak.SetBelgeTarihi(sTOK_HAREKETLERI2.sth_belge_tarih);
			evrak.cari = CariSqlite.GetCariByCariKod(OpenedConnection, sTOK_HAREKETLERI2.sth_cari_kodu, AdreslerTemsilciyeGore: false, "");
			evrak.SetEvrakKilitli(sTOK_HAREKETLERI2.sth_kilitli);
			evrak.SetEvrakNoSeri(sTOK_HAREKETLERI2.sth_evrakno_seri);
			evrak.SetEvraknoSira(sTOK_HAREKETLERI2.sth_evrakno_sira);
			evrak.SetEvrakTarihi(sTOK_HAREKETLERI2.sth_tarih);
			evrak.SetFirma(FirmaSqlite.GetFirma(OpenedConnection, sTOK_HAREKETLERI2.sth_firmano));
			evrak.SetSube(SubeSqlite.GetSube(OpenedConnection, sTOK_HAREKETLERI2.sth_subeno));
			evrak.SetMikroUserNo(sTOK_HAREKETLERI2.sth_create_user);
			evrak.SetOdemePlani(sTOK_HAREKETLERI2.sth_odeme_op);
			evrak.SetFiyatListesi(FiyatListesiSqlite.GetFiyatListesi(OpenedConnection, sTOK_HAREKETLERI2.sth_fiyat_liste_no));
			evrak.SetHedefDepo(DepoSqlite.GetDepo(OpenedConnection, sTOK_HAREKETLERI2.sth_giris_depo_no));
			evrak.SetKaynakDepo(DepoSqlite.GetDepo(OpenedConnection, sTOK_HAREKETLERI2.sth_cikis_depo_no));
			evrak.SetNakliyeDepo(DepoSqlite.GetDepo(OpenedConnection, sTOK_HAREKETLERI2.sth_nakliyedeposu));
			evrak.SetSevkAdresNo(sTOK_HAREKETLERI2.sth_adres_no);
			evrak.SetEximKodu(sTOK_HAREKETLERI2.sth_exim_kodu);
		}
		val = new SqliteCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano=16 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
		switch (evrak.evraktipi)
		{
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			val.Parameters.AddWithValue("@egk_hareket_tip", (object)1);
			val.Parameters.AddWithValue("@egk_evr_tip", (object)1);
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			val.Parameters.AddWithValue("@egk_hareket_tip", (object)0);
			val.Parameters.AddWithValue("@egk_evr_tip", (object)13);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
			val.Parameters.AddWithValue("@egk_hareket_tip", (object)2);
			val.Parameters.AddWithValue("@egk_evr_tip", (object)17);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSevk:
			val.Parameters.AddWithValue("@egk_hareket_tip", (object)2);
			val.Parameters.AddWithValue("@egk_evr_tip", (object)2);
			break;
		}
		val.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.EvrakNoSeri);
		val.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.EvrakNoSira);
		val.Connection = OpenedConnection;
		val2 = val.ExecuteReader();
		if (((DbDataReader)val2).HasRows)
		{
			while (((DbDataReader)val2).Read())
			{
				evrak.SetAciklama1(val2.GetSafeString(0));
				evrak.SetAciklama2(val2.GetSafeString(1));
				evrak.SetAciklama3(val2.GetSafeString(2));
				evrak.SetAciklama4(val2.GetSafeString(3));
				evrak.SetAciklama5(val2.GetSafeString(4));
				evrak.SetAciklama6(val2.GetSafeString(5));
				evrak.SetAciklama7(val2.GetSafeString(6));
				evrak.SetAciklama8(val2.GetSafeString(7));
				evrak.SetAciklama9(val2.GetSafeString(8));
				evrak.SetAciklama10(val2.GetSafeString(9));
			}
		}
		((DbDataReader)val2).Close();
		((DbDataReader)val2).Dispose();
		val2 = null;
		((Component)val).Dispose();
		val = null;
		return evrak;
	}

	private static Evrak GetDepolarArasiSiparisEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira)
	{
		if (GenelUtility.GetMikroVersiyon(((DbConnection)OpenedConnection).DataSource) > 15)
		{
			return V16_GetDepolarArasiSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira);
		}
		return V15_GetDepolarArasiSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira);
	}

	private static Evrak V15_GetDepolarArasiSiparisEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira)
	{
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Expected O, but got Unknown
		Evrak evrak = new Evrak(enum_GenelEvrakTipleri.DepolarArasiSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		string commandText = "SELECT DEPOLAR_ARASI_SIPARISLER.ssip_RECno,DEPOLAR_ARASI_SIPARISLER.ssip_RECid_DBCno,DEPOLAR_ARASI_SIPARISLER.ssip_RECid_RECno,DEPOLAR_ARASI_SIPARISLER.ssip_SpecRECno,DEPOLAR_ARASI_SIPARISLER.ssip_iptal,DEPOLAR_ARASI_SIPARISLER.ssip_fileid,DEPOLAR_ARASI_SIPARISLER.ssip_hidden,DEPOLAR_ARASI_SIPARISLER.ssip_kilitli,DEPOLAR_ARASI_SIPARISLER.ssip_degisti,DEPOLAR_ARASI_SIPARISLER.ssip_checksum,DEPOLAR_ARASI_SIPARISLER.ssip_create_user,DEPOLAR_ARASI_SIPARISLER.ssip_create_date,DEPOLAR_ARASI_SIPARISLER.ssip_lastup_user,DEPOLAR_ARASI_SIPARISLER.ssip_special1,DEPOLAR_ARASI_SIPARISLER.ssip_special2,DEPOLAR_ARASI_SIPARISLER.ssip_special3,DEPOLAR_ARASI_SIPARISLER.ssip_firmano,DEPOLAR_ARASI_SIPARISLER.ssip_subeno,DEPOLAR_ARASI_SIPARISLER.ssip_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_teslim_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_evrakno_seri,DEPOLAR_ARASI_SIPARISLER.ssip_evrakno_sira,DEPOLAR_ARASI_SIPARISLER.ssip_satirno,DEPOLAR_ARASI_SIPARISLER.ssip_belgeno,DEPOLAR_ARASI_SIPARISLER.ssip_belge_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_stok_kod,DEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat,DEPOLAR_ARASI_SIPARISLER.ssip_miktar,DEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr,DEPOLAR_ARASI_SIPARISLER.ssip_teslim_miktar,DEPOLAR_ARASI_SIPARISLER.ssip_tutar,DEPOLAR_ARASI_SIPARISLER.ssip_aciklama,DEPOLAR_ARASI_SIPARISLER.ssip_girdepo,DEPOLAR_ARASI_SIPARISLER.ssip_cikdepo,DEPOLAR_ARASI_SIPARISLER.ssip_kapat_fl,DEPOLAR_ARASI_SIPARISLER.ssip_stalRecId_DBCno,DEPOLAR_ARASI_SIPARISLER.ssip_stalRecId_RECno,DEPOLAR_ARASI_SIPARISLER.ssip_projekodu,DEPOLAR_ARASI_SIPARISLER.ssip_fiyat_liste_no,DEPOLAR_ARASI_SIPARISLER.ssip_paket_kod,DEPOLAR_ARASI_SIPARISLER.ssip_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM DEPOLAR_ARASI_SIPARISLER INNER JOIN STOKLAR ON STOKLAR.sto_kod=DEPOLAR_ARASI_SIPARISLER.ssip_stok_kod where ssip_evrakno_seri=@EvrakSeri AND ssip_evrakno_sira=@EvrakSira ORDER BY ssip_satirno";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@EvrakSeri", (object)EvrakSeri);
				val.Parameters.AddWithValue("@EvrakSira", (object)EvrakSira);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER = new DEPOLAR_ARASI_SIPARISLER();
					dEPOLAR_ARASI_SIPARISLER.ssip_RECno = val2.GetSafeInt32(0);
					dEPOLAR_ARASI_SIPARISLER.ssip_RECid_DBCno = val2.GetSafeInt16(1);
					dEPOLAR_ARASI_SIPARISLER.ssip_RECid_RECno = val2.GetSafeInt32(2);
					dEPOLAR_ARASI_SIPARISLER.ssip_SpecRECno = val2.GetSafeInt32(3);
					dEPOLAR_ARASI_SIPARISLER.ssip_iptal = val2.GetSafeBoolean(4);
					dEPOLAR_ARASI_SIPARISLER.ssip_fileid = val2.GetSafeInt16(5);
					dEPOLAR_ARASI_SIPARISLER.ssip_hidden = val2.GetSafeBoolean(6);
					dEPOLAR_ARASI_SIPARISLER.ssip_kilitli = val2.GetSafeBoolean(7);
					dEPOLAR_ARASI_SIPARISLER.ssip_degisti = val2.GetSafeBoolean(8);
					dEPOLAR_ARASI_SIPARISLER.ssip_checksum = val2.GetSafeInt32(9);
					dEPOLAR_ARASI_SIPARISLER.ssip_create_user = val2.GetSafeInt16(10);
					dEPOLAR_ARASI_SIPARISLER.ssip_create_date = val2.GetSafeDateTime(11);
					dEPOLAR_ARASI_SIPARISLER.ssip_lastup_user = val2.GetSafeInt16(12);
					dEPOLAR_ARASI_SIPARISLER.ssip_special1 = val2.GetSafeString(13);
					dEPOLAR_ARASI_SIPARISLER.ssip_special2 = val2.GetSafeString(14);
					dEPOLAR_ARASI_SIPARISLER.ssip_special3 = val2.GetSafeString(15);
					dEPOLAR_ARASI_SIPARISLER.ssip_firmano = val2.GetSafeInt32(16);
					dEPOLAR_ARASI_SIPARISLER.ssip_subeno = val2.GetSafeInt32(17);
					dEPOLAR_ARASI_SIPARISLER.ssip_tarih = val2.GetSafeDateTime(18);
					dEPOLAR_ARASI_SIPARISLER.ssip_teslim_tarih = val2.GetSafeDateTime(19);
					dEPOLAR_ARASI_SIPARISLER.ssip_evrakno_seri = val2.GetSafeString(20);
					dEPOLAR_ARASI_SIPARISLER.ssip_evrakno_sira = val2.GetSafeInt32(21);
					dEPOLAR_ARASI_SIPARISLER.ssip_satirno = val2.GetSafeInt32(22);
					dEPOLAR_ARASI_SIPARISLER.ssip_belgeno = val2.GetSafeString(23);
					dEPOLAR_ARASI_SIPARISLER.ssip_belge_tarih = val2.GetSafeDateTime(24);
					dEPOLAR_ARASI_SIPARISLER.ssip_stok_kod = val2.GetSafeString(25);
					dEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat = ((DbDataReader)val2).GetDouble(26);
					dEPOLAR_ARASI_SIPARISLER.ssip_miktar = ((DbDataReader)val2).GetDouble(27);
					dEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr = val2.GetSafeByte(28);
					dEPOLAR_ARASI_SIPARISLER.ssip_teslim_miktar = ((DbDataReader)val2).GetDouble(29);
					dEPOLAR_ARASI_SIPARISLER.ssip_tutar = ((DbDataReader)val2).GetDouble(30);
					dEPOLAR_ARASI_SIPARISLER.ssip_aciklama = val2.GetSafeString(31);
					dEPOLAR_ARASI_SIPARISLER.ssip_girdepo = val2.GetSafeInt32(32);
					dEPOLAR_ARASI_SIPARISLER.ssip_cikdepo = val2.GetSafeInt32(33);
					dEPOLAR_ARASI_SIPARISLER.ssip_kapat_fl = val2.GetSafeBoolean(34);
					dEPOLAR_ARASI_SIPARISLER.ssip_stalRecId_DBCno = val2.GetSafeInt16(35);
					dEPOLAR_ARASI_SIPARISLER.ssip_stalRecId_RECno = val2.GetSafeInt32(36);
					dEPOLAR_ARASI_SIPARISLER.ssip_projekodu = val2.GetSafeString(37);
					dEPOLAR_ARASI_SIPARISLER.ssip_fiyat_liste_no = val2.GetSafeInt32(38);
					dEPOLAR_ARASI_SIPARISLER.ssip_paket_kod = val2.GetSafeString(39);
					dEPOLAR_ARASI_SIPARISLER.ssip_kapatmanedenkod = val2.GetSafeString(40);
					dEPOLAR_ARASI_SIPARISLER.sto_isim = val2.GetSafeString(41);
					dEPOLAR_ARASI_SIPARISLER.sto_birim1_ad = val2.GetSafeString(42);
					dEPOLAR_ARASI_SIPARISLER.sto_birim2_ad = val2.GetSafeString(43);
					dEPOLAR_ARASI_SIPARISLER.sto_birim3_ad = val2.GetSafeString(44);
					dEPOLAR_ARASI_SIPARISLER.sto_birim4_ad = val2.GetSafeString(45);
					dEPOLAR_ARASI_SIPARISLER.sto_birim1_katsayi = ((DbDataReader)val2).GetDouble(46);
					dEPOLAR_ARASI_SIPARISLER.sto_birim2_katsayi = ((DbDataReader)val2).GetDouble(47);
					dEPOLAR_ARASI_SIPARISLER.sto_birim3_katsayi = ((DbDataReader)val2).GetDouble(48);
					dEPOLAR_ARASI_SIPARISLER.sto_birim4_katsayi = ((DbDataReader)val2).GetDouble(49);
					bool safeBoolean = val2.GetSafeBoolean(50);
					bool safeBoolean2 = val2.GetSafeBoolean(51);
					if (safeBoolean || safeBoolean2)
					{
						dEPOLAR_ARASI_SIPARISLER.renk_beden_hareketleri = StokSqlite.V15_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.SubeSiparis, dEPOLAR_ARASI_SIPARISLER.ssip_RECid_DBCno, dEPOLAR_ARASI_SIPARISLER.ssip_RECid_RECno);
					}
					evrak.AddDepolarArasiSiparisHareketi(dEPOLAR_ARASI_SIPARISLER, SiparisKarsilamaYap: false);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			if (evrak.GetDepolarArasiSiparisHareketleri().Count > 0)
			{
				DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER2 = evrak.GetDepolarArasiSiparisHareketleri()[0];
				evrak.alternatifdovizkuru = new Kur();
				evrak.alternatifdovizkuru.dov_fiyat = 1.0;
				evrak.SetBelgeNo(dEPOLAR_ARASI_SIPARISLER2.ssip_belgeno);
				evrak.SetBelgeTarihi(dEPOLAR_ARASI_SIPARISLER2.ssip_belge_tarih);
				evrak.SetHedefDepo(DepoSqlite.GetDepo(OpenedConnection, dEPOLAR_ARASI_SIPARISLER2.ssip_girdepo));
				evrak.SetKaynakDepo(DepoSqlite.GetDepo(OpenedConnection, dEPOLAR_ARASI_SIPARISLER2.ssip_cikdepo));
				evrak.SetEvrakKilitli(dEPOLAR_ARASI_SIPARISLER2.ssip_kilitli);
				evrak.SetEvrakNoSeri(EvrakSeri);
				evrak.SetEvraknoSira(EvrakSira);
				evrak.SetEvrakTarihi(dEPOLAR_ARASI_SIPARISLER2.ssip_tarih);
				evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiSiparis;
				evrak.SetFirma(FirmaSqlite.GetFirma(OpenedConnection, dEPOLAR_ARASI_SIPARISLER2.ssip_firmano));
				evrak.SetSube(SubeSqlite.GetSube(OpenedConnection, dEPOLAR_ARASI_SIPARISLER2.ssip_subeno));
				evrak.SetMikroUserNo(dEPOLAR_ARASI_SIPARISLER2.ssip_create_user);
				evrak.SetYeniKayit(YeniDeger: false);
				if (evrak.GetDepolarArasiSiparisHareketleri()[0].ssip_kilitli)
				{
					evrak.SetEvrakKilitli(YeniDeger: true);
				}
				else
				{
					evrak.SetEvrakKilitli(YeniDeger: false);
				}
				commandText = "SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano=86 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira";
				SqliteCommand val3 = new SqliteCommand(commandText, OpenedConnection);
				val3.Parameters.AddWithValue("@egk_hareket_tip", (object)0);
				val3.Parameters.AddWithValue("@egk_evr_tip", (object)0);
				val3.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.EvrakNoSeri);
				val3.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.EvrakNoSira);
				SqliteDataReader val4 = val3.ExecuteReader();
				if (((DbDataReader)val4).HasRows)
				{
					while (((DbDataReader)val4).Read())
					{
						evrak.SetAciklama1(val4.GetSafeString(0));
						evrak.SetAciklama2(val4.GetSafeString(1));
						evrak.SetAciklama3(val4.GetSafeString(2));
						evrak.SetAciklama4(val4.GetSafeString(3));
						evrak.SetAciklama5(val4.GetSafeString(4));
						evrak.SetAciklama6(val4.GetSafeString(5));
						evrak.SetAciklama7(val4.GetSafeString(6));
						evrak.SetAciklama8(val4.GetSafeString(7));
						evrak.SetAciklama9(val4.GetSafeString(8));
						evrak.SetAciklama10(val4.GetSafeString(9));
					}
				}
				((DbDataReader)val4).Close();
				((DbDataReader)val4).Dispose();
				val4 = null;
				((Component)val3).Dispose();
				val3 = null;
			}
		}
		catch
		{
		}
		return evrak;
	}

	private static Evrak V16_GetDepolarArasiSiparisEvrak(SqliteConnection OpenedConnection, string EvrakSeri, int EvrakSira)
	{
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Expected O, but got Unknown
		Evrak evrak = new Evrak(enum_GenelEvrakTipleri.DepolarArasiSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		string commandText = "SELECT DEPOLAR_ARASI_SIPARISLER.ssip_Guid,0,0,DEPOLAR_ARASI_SIPARISLER.ssip_SpecRECno,DEPOLAR_ARASI_SIPARISLER.ssip_iptal,DEPOLAR_ARASI_SIPARISLER.ssip_fileid,DEPOLAR_ARASI_SIPARISLER.ssip_hidden,DEPOLAR_ARASI_SIPARISLER.ssip_kilitli,DEPOLAR_ARASI_SIPARISLER.ssip_degisti,DEPOLAR_ARASI_SIPARISLER.ssip_checksum,DEPOLAR_ARASI_SIPARISLER.ssip_create_user,DEPOLAR_ARASI_SIPARISLER.ssip_create_date,DEPOLAR_ARASI_SIPARISLER.ssip_lastup_user,DEPOLAR_ARASI_SIPARISLER.ssip_special1,DEPOLAR_ARASI_SIPARISLER.ssip_special2,DEPOLAR_ARASI_SIPARISLER.ssip_special3,DEPOLAR_ARASI_SIPARISLER.ssip_firmano,DEPOLAR_ARASI_SIPARISLER.ssip_subeno,DEPOLAR_ARASI_SIPARISLER.ssip_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_teslim_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_evrakno_seri,DEPOLAR_ARASI_SIPARISLER.ssip_evrakno_sira,DEPOLAR_ARASI_SIPARISLER.ssip_satirno,DEPOLAR_ARASI_SIPARISLER.ssip_belgeno,DEPOLAR_ARASI_SIPARISLER.ssip_belge_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_stok_kod,DEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat,DEPOLAR_ARASI_SIPARISLER.ssip_miktar,DEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr,DEPOLAR_ARASI_SIPARISLER.ssip_teslim_miktar,DEPOLAR_ARASI_SIPARISLER.ssip_tutar,DEPOLAR_ARASI_SIPARISLER.ssip_aciklama,DEPOLAR_ARASI_SIPARISLER.ssip_girdepo,DEPOLAR_ARASI_SIPARISLER.ssip_cikdepo,DEPOLAR_ARASI_SIPARISLER.ssip_kapat_fl,0,DEPOLAR_ARASI_SIPARISLER.ssip_stal_uid,DEPOLAR_ARASI_SIPARISLER.ssip_projekodu,DEPOLAR_ARASI_SIPARISLER.ssip_fiyat_liste_no,DEPOLAR_ARASI_SIPARISLER.ssip_paket_kod,DEPOLAR_ARASI_SIPARISLER.ssip_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM DEPOLAR_ARASI_SIPARISLER INNER JOIN STOKLAR ON STOKLAR.sto_kod=DEPOLAR_ARASI_SIPARISLER.ssip_stok_kod where ssip_evrakno_seri=@EvrakSeri AND ssip_evrakno_sira=@EvrakSira ORDER BY ssip_satirno";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@EvrakSeri", (object)EvrakSeri);
				val.Parameters.AddWithValue("@EvrakSira", (object)EvrakSira);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER = new DEPOLAR_ARASI_SIPARISLER();
					dEPOLAR_ARASI_SIPARISLER.ssip_Guid = ((DbDataReader)val2).GetGuid(0);
					dEPOLAR_ARASI_SIPARISLER.ssip_SpecRECno = val2.GetSafeInt32(3);
					dEPOLAR_ARASI_SIPARISLER.ssip_iptal = val2.GetSafeBoolean(4);
					dEPOLAR_ARASI_SIPARISLER.ssip_fileid = val2.GetSafeInt16(5);
					dEPOLAR_ARASI_SIPARISLER.ssip_hidden = val2.GetSafeBoolean(6);
					dEPOLAR_ARASI_SIPARISLER.ssip_kilitli = val2.GetSafeBoolean(7);
					dEPOLAR_ARASI_SIPARISLER.ssip_degisti = val2.GetSafeBoolean(8);
					dEPOLAR_ARASI_SIPARISLER.ssip_checksum = val2.GetSafeInt32(9);
					dEPOLAR_ARASI_SIPARISLER.ssip_create_user = val2.GetSafeInt16(10);
					dEPOLAR_ARASI_SIPARISLER.ssip_create_date = val2.GetSafeDateTime(11);
					dEPOLAR_ARASI_SIPARISLER.ssip_lastup_user = val2.GetSafeInt16(12);
					dEPOLAR_ARASI_SIPARISLER.ssip_special1 = val2.GetSafeString(13);
					dEPOLAR_ARASI_SIPARISLER.ssip_special2 = val2.GetSafeString(14);
					dEPOLAR_ARASI_SIPARISLER.ssip_special3 = val2.GetSafeString(15);
					dEPOLAR_ARASI_SIPARISLER.ssip_firmano = val2.GetSafeInt32(16);
					dEPOLAR_ARASI_SIPARISLER.ssip_subeno = val2.GetSafeInt32(17);
					dEPOLAR_ARASI_SIPARISLER.ssip_tarih = val2.GetSafeDateTime(18);
					dEPOLAR_ARASI_SIPARISLER.ssip_teslim_tarih = val2.GetSafeDateTime(19);
					dEPOLAR_ARASI_SIPARISLER.ssip_evrakno_seri = val2.GetSafeString(20);
					dEPOLAR_ARASI_SIPARISLER.ssip_evrakno_sira = val2.GetSafeInt32(21);
					dEPOLAR_ARASI_SIPARISLER.ssip_satirno = val2.GetSafeInt32(22);
					dEPOLAR_ARASI_SIPARISLER.ssip_belgeno = val2.GetSafeString(23);
					dEPOLAR_ARASI_SIPARISLER.ssip_belge_tarih = val2.GetSafeDateTime(24);
					dEPOLAR_ARASI_SIPARISLER.ssip_stok_kod = val2.GetSafeString(25);
					dEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat = ((DbDataReader)val2).GetDouble(26);
					dEPOLAR_ARASI_SIPARISLER.ssip_miktar = ((DbDataReader)val2).GetDouble(27);
					dEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr = val2.GetSafeByte(28);
					dEPOLAR_ARASI_SIPARISLER.ssip_teslim_miktar = ((DbDataReader)val2).GetDouble(29);
					dEPOLAR_ARASI_SIPARISLER.ssip_tutar = ((DbDataReader)val2).GetDouble(30);
					dEPOLAR_ARASI_SIPARISLER.ssip_aciklama = val2.GetSafeString(31);
					dEPOLAR_ARASI_SIPARISLER.ssip_girdepo = val2.GetSafeInt32(32);
					dEPOLAR_ARASI_SIPARISLER.ssip_cikdepo = val2.GetSafeInt32(33);
					dEPOLAR_ARASI_SIPARISLER.ssip_kapat_fl = val2.GetSafeBoolean(34);
					dEPOLAR_ARASI_SIPARISLER.ssip_stal_uid = ((DbDataReader)val2).GetGuid(36);
					dEPOLAR_ARASI_SIPARISLER.ssip_projekodu = val2.GetSafeString(37);
					dEPOLAR_ARASI_SIPARISLER.ssip_fiyat_liste_no = val2.GetSafeInt32(38);
					dEPOLAR_ARASI_SIPARISLER.ssip_paket_kod = val2.GetSafeString(39);
					dEPOLAR_ARASI_SIPARISLER.ssip_kapatmanedenkod = val2.GetSafeString(40);
					dEPOLAR_ARASI_SIPARISLER.sto_isim = val2.GetSafeString(41);
					dEPOLAR_ARASI_SIPARISLER.sto_birim1_ad = val2.GetSafeString(42);
					dEPOLAR_ARASI_SIPARISLER.sto_birim2_ad = val2.GetSafeString(43);
					dEPOLAR_ARASI_SIPARISLER.sto_birim3_ad = val2.GetSafeString(44);
					dEPOLAR_ARASI_SIPARISLER.sto_birim4_ad = val2.GetSafeString(45);
					dEPOLAR_ARASI_SIPARISLER.sto_birim1_katsayi = ((DbDataReader)val2).GetDouble(46);
					dEPOLAR_ARASI_SIPARISLER.sto_birim2_katsayi = ((DbDataReader)val2).GetDouble(47);
					dEPOLAR_ARASI_SIPARISLER.sto_birim3_katsayi = ((DbDataReader)val2).GetDouble(48);
					dEPOLAR_ARASI_SIPARISLER.sto_birim4_katsayi = ((DbDataReader)val2).GetDouble(49);
					bool safeBoolean = val2.GetSafeBoolean(50);
					bool safeBoolean2 = val2.GetSafeBoolean(51);
					if (safeBoolean || safeBoolean2)
					{
						dEPOLAR_ARASI_SIPARISLER.renk_beden_hareketleri = StokSqlite.V16_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.SubeSiparis, dEPOLAR_ARASI_SIPARISLER.ssip_Guid);
					}
					evrak.AddDepolarArasiSiparisHareketi(dEPOLAR_ARASI_SIPARISLER, SiparisKarsilamaYap: false);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			if (evrak.GetDepolarArasiSiparisHareketleri().Count > 0)
			{
				DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER2 = evrak.GetDepolarArasiSiparisHareketleri()[0];
				evrak.alternatifdovizkuru = new Kur();
				evrak.alternatifdovizkuru.dov_fiyat = 1.0;
				evrak.SetBelgeNo(dEPOLAR_ARASI_SIPARISLER2.ssip_belgeno);
				evrak.SetBelgeTarihi(dEPOLAR_ARASI_SIPARISLER2.ssip_belge_tarih);
				evrak.SetHedefDepo(DepoSqlite.GetDepo(OpenedConnection, dEPOLAR_ARASI_SIPARISLER2.ssip_girdepo));
				evrak.SetKaynakDepo(DepoSqlite.GetDepo(OpenedConnection, dEPOLAR_ARASI_SIPARISLER2.ssip_cikdepo));
				evrak.SetEvrakKilitli(dEPOLAR_ARASI_SIPARISLER2.ssip_kilitli);
				evrak.SetEvrakNoSeri(EvrakSeri);
				evrak.SetEvraknoSira(EvrakSira);
				evrak.SetEvrakTarihi(dEPOLAR_ARASI_SIPARISLER2.ssip_tarih);
				evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiSiparis;
				evrak.SetFirma(FirmaSqlite.GetFirma(OpenedConnection, dEPOLAR_ARASI_SIPARISLER2.ssip_firmano));
				evrak.SetSube(SubeSqlite.GetSube(OpenedConnection, dEPOLAR_ARASI_SIPARISLER2.ssip_subeno));
				evrak.SetMikroUserNo(dEPOLAR_ARASI_SIPARISLER2.ssip_create_user);
				evrak.SetYeniKayit(YeniDeger: false);
				if (evrak.GetDepolarArasiSiparisHareketleri()[0].ssip_kilitli)
				{
					evrak.SetEvrakKilitli(YeniDeger: true);
				}
				else
				{
					evrak.SetEvrakKilitli(YeniDeger: false);
				}
				commandText = "SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano=86 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira";
				SqliteCommand val3 = new SqliteCommand(commandText, OpenedConnection);
				val3.Parameters.AddWithValue("@egk_hareket_tip", (object)0);
				val3.Parameters.AddWithValue("@egk_evr_tip", (object)0);
				val3.Parameters.AddWithValue("@egk_evr_seri", (object)evrak.EvrakNoSeri);
				val3.Parameters.AddWithValue("@egk_evr_sira", (object)evrak.EvrakNoSira);
				SqliteDataReader val4 = val3.ExecuteReader();
				if (((DbDataReader)val4).HasRows)
				{
					while (((DbDataReader)val4).Read())
					{
						evrak.SetAciklama1(val4.GetSafeString(0));
						evrak.SetAciklama2(val4.GetSafeString(1));
						evrak.SetAciklama3(val4.GetSafeString(2));
						evrak.SetAciklama4(val4.GetSafeString(3));
						evrak.SetAciklama5(val4.GetSafeString(4));
						evrak.SetAciklama6(val4.GetSafeString(5));
						evrak.SetAciklama7(val4.GetSafeString(6));
						evrak.SetAciklama8(val4.GetSafeString(7));
						evrak.SetAciklama9(val4.GetSafeString(8));
						evrak.SetAciklama10(val4.GetSafeString(9));
					}
				}
				((DbDataReader)val4).Close();
				((DbDataReader)val4).Dispose();
				val4 = null;
				((Component)val3).Dispose();
				val3 = null;
			}
		}
		catch
		{
		}
		return evrak;
	}

	private static SIPARISLER V15_GetSiparisKalem(SqliteConnection OpenedConnection, int sip_RECno)
	{
		SIPARISLER sIPARISLER = new SIPARISLER();
		string commandText = "SELECT SIPARISLER.sip_RECno,SIPARISLER.sip_RECid_DBCno,SIPARISLER.sip_RECid_RECno,SIPARISLER.sip_SpecRECno,SIPARISLER.sip_iptal,SIPARISLER.sip_fileid,SIPARISLER.sip_hidden,SIPARISLER.sip_kilitli,SIPARISLER.sip_degisti,SIPARISLER.sip_checksum,SIPARISLER.sip_create_user,SIPARISLER.sip_create_date,SIPARISLER.sip_lastup_user,SIPARISLER.sip_special1,SIPARISLER.sip_special2,SIPARISLER.sip_special3,SIPARISLER.sip_firmano,SIPARISLER.sip_subeno,SIPARISLER.sip_tarih,SIPARISLER.sip_teslim_tarih,SIPARISLER.sip_tip,SIPARISLER.sip_cins,SIPARISLER.sip_evrakno_seri,SIPARISLER.sip_evrakno_sira,SIPARISLER.sip_satirno,SIPARISLER.sip_belgeno,SIPARISLER.sip_belge_tarih,SIPARISLER.sip_satici_kod,SIPARISLER.sip_musteri_kod,SIPARISLER.sip_stok_kod,SIPARISLER.sip_b_fiyat,SIPARISLER.sip_miktar,SIPARISLER.sip_birim_pntr,SIPARISLER.sip_teslim_miktar,SIPARISLER.sip_tutar,SIPARISLER.sip_iskonto_1,SIPARISLER.sip_iskonto_2,SIPARISLER.sip_iskonto_3,SIPARISLER.sip_iskonto_4,SIPARISLER.sip_iskonto_5,SIPARISLER.sip_iskonto_6,SIPARISLER.sip_masraf_1,SIPARISLER.sip_masraf_2,SIPARISLER.sip_masraf_3,SIPARISLER.sip_masraf_4,SIPARISLER.sip_vergi_pntr,SIPARISLER.sip_vergi,SIPARISLER.sip_masvergi_pntr,SIPARISLER.sip_masvergi,SIPARISLER.sip_opno,SIPARISLER.sip_aciklama,SIPARISLER.sip_aciklama2,SIPARISLER.sip_depono,SIPARISLER.sip_OnaylayanKulNo,SIPARISLER.sip_vergisiz_fl,SIPARISLER.sip_kapat_fl,SIPARISLER.sip_promosyon_fl,SIPARISLER.sip_cari_sormerk,SIPARISLER.sip_stok_sormerk,SIPARISLER.sip_cari_grupno,SIPARISLER.sip_doviz_cinsi,SIPARISLER.sip_doviz_kuru,SIPARISLER.sip_alt_doviz_kuru,SIPARISLER.sip_adresno,SIPARISLER.sip_teslimturu,SIPARISLER.sip_cagrilabilir_fl,SIPARISLER.sip_prosiprecDbId,SIPARISLER.sip_prosiprecrecI,SIPARISLER.sip_iskonto1,SIPARISLER.sip_iskonto2,SIPARISLER.sip_iskonto3,SIPARISLER.sip_iskonto4,SIPARISLER.sip_iskonto5,SIPARISLER.sip_iskonto6,SIPARISLER.sip_masraf1,SIPARISLER.sip_masraf2,SIPARISLER.sip_masraf3,SIPARISLER.sip_masraf4,SIPARISLER.sip_isk1,SIPARISLER.sip_isk2,SIPARISLER.sip_isk3,SIPARISLER.sip_isk4,SIPARISLER.sip_isk5,SIPARISLER.sip_isk6,SIPARISLER.sip_mas1,SIPARISLER.sip_mas2,SIPARISLER.sip_mas3,SIPARISLER.sip_mas4,SIPARISLER.sip_Exp_Imp_Kodu,SIPARISLER.sip_kar_orani,SIPARISLER.sip_durumu,SIPARISLER.sip_stalRecId_DBCno,SIPARISLER.sip_stalRecId_RECno,SIPARISLER.sip_planlananmiktar,SIPARISLER.sip_teklifRecId_DBCno,SIPARISLER.sip_teklifRecId_RECno,SIPARISLER.sip_parti_kodu,SIPARISLER.sip_lot_no,SIPARISLER.sip_projekodu,SIPARISLER.sip_fiyat_liste_no,SIPARISLER.sip_Otv_Pntr,SIPARISLER.sip_Otv_Vergi,SIPARISLER.sip_otvtutari,SIPARISLER.sip_OtvVergisiz_Fl,SIPARISLER.sip_paket_kod,SIPARISLER.sip_RezRecId_DBCno,SIPARISLER.sip_RezRecId_RECno,SIPARISLER.sip_harekettipi,SIPARISLER.sip_yetkili_recid_dbcno,SIPARISLER.sip_yetkili_recid_recno,SIPARISLER.sip_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM SIPARISLER INNER JOIN STOKLAR ON STOKLAR.sto_kod=SIPARISLER.sip_stok_kod WHERE sip_RECno=@sip_RECno";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@sip_RECno", (object)sip_RECno);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					sIPARISLER.sip_RECno = val2.GetSafeInt32(0);
					sIPARISLER.sip_RECid_DBCno = val2.GetSafeInt16(1);
					sIPARISLER.sip_RECid_RECno = val2.GetSafeInt32(2);
					sIPARISLER.sip_SpecRECno = val2.GetSafeInt32(3);
					sIPARISLER.sip_iptal = val2.GetSafeBoolean(4);
					sIPARISLER.sip_fileid = val2.GetSafeInt16(5);
					sIPARISLER.sip_hidden = val2.GetSafeBoolean(6);
					sIPARISLER.sip_kilitli = val2.GetSafeBoolean(7);
					sIPARISLER.sip_degisti = val2.GetSafeBoolean(8);
					sIPARISLER.sip_checksum = val2.GetSafeInt32(9);
					sIPARISLER.sip_create_user = val2.GetSafeInt16(10);
					sIPARISLER.sip_create_date = val2.GetSafeDateTime(11);
					sIPARISLER.sip_lastup_user = val2.GetSafeInt16(12);
					sIPARISLER.sip_special1 = val2.GetSafeString(13);
					sIPARISLER.sip_special2 = val2.GetSafeString(14);
					sIPARISLER.sip_special3 = val2.GetSafeString(15);
					sIPARISLER.sip_firmano = val2.GetSafeInt32(16);
					sIPARISLER.sip_subeno = val2.GetSafeInt32(17);
					sIPARISLER.sip_tarih = val2.GetSafeDateTime(18);
					sIPARISLER.sip_teslim_tarih = val2.GetSafeDateTime(19);
					sIPARISLER.sip_tip = (enum_sip_tip)val2.GetSafeByte(20);
					sIPARISLER.sip_cins = (enum_sip_cins)val2.GetSafeByte(21);
					sIPARISLER.sip_evrakno_seri = val2.GetSafeString(22);
					sIPARISLER.sip_evrakno_sira = val2.GetSafeInt32(23);
					sIPARISLER.sip_satirno = val2.GetSafeInt32(24);
					sIPARISLER.sip_belgeno = val2.GetSafeString(25);
					sIPARISLER.sip_belge_tarih = val2.GetSafeDateTime(26);
					sIPARISLER.sip_satici_kod = val2.GetSafeString(27);
					sIPARISLER.sip_musteri_kod = val2.GetSafeString(28);
					sIPARISLER.sip_stok_kod = val2.GetSafeString(29);
					sIPARISLER.sip_b_fiyat = val2.GetSafeDouble(30);
					sIPARISLER.sip_miktar = val2.GetSafeDouble(31);
					sIPARISLER.sip_birim_pntr = val2.GetSafeByte(32);
					sIPARISLER.sip_teslim_miktar = val2.GetSafeDouble(33);
					sIPARISLER.sip_tutar = val2.GetSafeDouble(34);
					sIPARISLER.sip_iskonto_1 = val2.GetSafeDouble(35);
					sIPARISLER.sip_iskonto_2 = val2.GetSafeDouble(36);
					sIPARISLER.sip_iskonto_3 = val2.GetSafeDouble(37);
					sIPARISLER.sip_iskonto_4 = val2.GetSafeDouble(38);
					sIPARISLER.sip_iskonto_5 = val2.GetSafeDouble(39);
					sIPARISLER.sip_iskonto_6 = val2.GetSafeDouble(40);
					sIPARISLER.sip_masraf_1 = val2.GetSafeDouble(41);
					sIPARISLER.sip_masraf_2 = val2.GetSafeDouble(42);
					sIPARISLER.sip_masraf_3 = val2.GetSafeDouble(43);
					sIPARISLER.sip_masraf_4 = val2.GetSafeDouble(44);
					sIPARISLER.sip_vergi_pntr = val2.GetSafeByte(45);
					sIPARISLER.sip_vergi = val2.GetSafeDouble(46);
					sIPARISLER.sip_masvergi_pntr = val2.GetSafeByte(47);
					sIPARISLER.sip_masvergi = val2.GetSafeDouble(48);
					sIPARISLER.sip_opno = val2.GetSafeInt32(49);
					sIPARISLER.sip_aciklama = val2.GetSafeString(50);
					sIPARISLER.sip_aciklama2 = val2.GetSafeString(51);
					sIPARISLER.sip_depono = val2.GetSafeInt32(52);
					sIPARISLER.sip_OnaylayanKulNo = val2.GetSafeInt16(53);
					sIPARISLER.sip_vergisiz_fl = val2.GetSafeBoolean(54);
					sIPARISLER.sip_kapat_fl = val2.GetSafeBoolean(55);
					sIPARISLER.sip_promosyon_fl = val2.GetSafeBoolean(56);
					sIPARISLER.sip_cari_sormerk = val2.GetSafeString(57);
					sIPARISLER.sip_stok_sormerk = val2.GetSafeString(58);
					sIPARISLER.sip_cari_grupno = val2.GetSafeByte(59);
					sIPARISLER.sip_doviz_cinsi = val2.GetSafeByte(60);
					sIPARISLER.sip_doviz_kuru = val2.GetSafeDouble(61);
					sIPARISLER.sip_alt_doviz_kuru = val2.GetSafeDouble(62);
					sIPARISLER.sip_adresno = val2.GetSafeInt32(63);
					sIPARISLER.sip_teslimturu = val2.GetSafeString(64);
					sIPARISLER.sip_cagrilabilir_fl = val2.GetSafeBoolean(65);
					sIPARISLER.sip_prosiprecDbId = val2.GetSafeInt16(66);
					sIPARISLER.sip_prosiprecrecI = val2.GetSafeInt32(67);
					sIPARISLER.sip_iskonto1 = val2.GetSafeByte(68);
					sIPARISLER.sip_iskonto2 = val2.GetSafeByte(69);
					sIPARISLER.sip_iskonto3 = val2.GetSafeByte(70);
					sIPARISLER.sip_iskonto4 = val2.GetSafeByte(71);
					sIPARISLER.sip_iskonto5 = val2.GetSafeByte(72);
					sIPARISLER.sip_iskonto6 = val2.GetSafeByte(73);
					sIPARISLER.sip_masraf1 = val2.GetSafeByte(74);
					sIPARISLER.sip_masraf2 = val2.GetSafeByte(75);
					sIPARISLER.sip_masraf3 = val2.GetSafeByte(76);
					sIPARISLER.sip_masraf4 = val2.GetSafeByte(77);
					sIPARISLER.sip_isk1 = val2.GetSafeBoolean(78);
					sIPARISLER.sip_isk2 = val2.GetSafeBoolean(79);
					sIPARISLER.sip_isk3 = val2.GetSafeBoolean(80);
					sIPARISLER.sip_isk4 = val2.GetSafeBoolean(81);
					sIPARISLER.sip_isk5 = val2.GetSafeBoolean(82);
					sIPARISLER.sip_isk6 = val2.GetSafeBoolean(83);
					sIPARISLER.sip_mas1 = val2.GetSafeBoolean(84);
					sIPARISLER.sip_mas2 = val2.GetSafeBoolean(85);
					sIPARISLER.sip_mas3 = val2.GetSafeBoolean(86);
					sIPARISLER.sip_mas4 = val2.GetSafeBoolean(87);
					sIPARISLER.sip_Exp_Imp_Kodu = val2.GetSafeString(88);
					sIPARISLER.sip_kar_orani = val2.GetSafeDouble(89);
					sIPARISLER.sip_durumu = (enum_sip_durumu)val2.GetSafeByte(90);
					sIPARISLER.sip_stalRecId_DBCno = val2.GetSafeInt16(91);
					sIPARISLER.sip_stalRecId_RECno = val2.GetSafeInt32(92);
					sIPARISLER.sip_planlananmiktar = val2.GetSafeDouble(93);
					sIPARISLER.sip_teklifRecId_DBCno = val2.GetSafeInt16(94);
					sIPARISLER.sip_teklifRecId_RECno = val2.GetSafeInt32(95);
					sIPARISLER.sip_parti_kodu = val2.GetSafeString(96);
					sIPARISLER.sip_lot_no = val2.GetSafeInt32(97);
					sIPARISLER.sip_projekodu = val2.GetSafeString(98);
					sIPARISLER.sip_fiyat_liste_no = val2.GetSafeInt32(99);
					sIPARISLER.sip_Otv_Pntr = val2.GetSafeByte(100);
					sIPARISLER.sip_Otv_Vergi = val2.GetSafeDouble(101);
					sIPARISLER.sip_otvtutari = val2.GetSafeDouble(102);
					sIPARISLER.sip_OtvVergisiz_Fl = val2.GetSafeByte(103);
					sIPARISLER.sip_paket_kod = val2.GetSafeString(104);
					sIPARISLER.sip_RezRecId_DBCno = val2.GetSafeInt16(105);
					sIPARISLER.sip_RezRecId_RECno = val2.GetSafeInt32(106);
					sIPARISLER.sip_harekettipi = (enum_sip_harekettipi)val2.GetSafeByte(107);
					sIPARISLER.sip_yetkili_recid_dbcno = val2.GetSafeInt16(108);
					sIPARISLER.sip_yetkili_recid_recno = val2.GetSafeInt32(109);
					sIPARISLER.sip_kapatmanedenkod = val2.GetSafeString(110);
					sIPARISLER.sto_isim = val2.GetSafeString(111);
					sIPARISLER.sto_birim1_ad = val2.GetSafeString(112);
					sIPARISLER.sto_birim2_ad = val2.GetSafeString(113);
					sIPARISLER.sto_birim3_ad = val2.GetSafeString(114);
					sIPARISLER.sto_birim4_ad = val2.GetSafeString(115);
					sIPARISLER.sto_birim1_katsayi = val2.GetSafeDouble(116);
					sIPARISLER.sto_birim2_katsayi = val2.GetSafeDouble(117);
					sIPARISLER.sto_birim3_katsayi = val2.GetSafeDouble(118);
					sIPARISLER.sto_birim4_katsayi = val2.GetSafeDouble(119);
					bool safeBoolean = val2.GetSafeBoolean(120);
					bool safeBoolean2 = val2.GetSafeBoolean(121);
					if (safeBoolean || safeBoolean2)
					{
						sIPARISLER.renk_beden_hareketleri = StokSqlite.V15_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.Siparis, sIPARISLER.sip_RECid_DBCno, sIPARISLER.sip_RECid_RECno);
					}
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return sIPARISLER;
	}

	private static SIPARISLER V16_GetSiparisKalem(SqliteConnection OpenedConnection, Guid sip_Guid)
	{
		SIPARISLER sIPARISLER = new SIPARISLER();
		string commandText = "SELECT SIPARISLER.sip_Guid,0,0,SIPARISLER.sip_SpecRECno,SIPARISLER.sip_iptal,SIPARISLER.sip_fileid,SIPARISLER.sip_hidden,SIPARISLER.sip_kilitli,SIPARISLER.sip_degisti,SIPARISLER.sip_checksum,SIPARISLER.sip_create_user,SIPARISLER.sip_create_date,SIPARISLER.sip_lastup_user,SIPARISLER.sip_special1,SIPARISLER.sip_special2,SIPARISLER.sip_special3,SIPARISLER.sip_firmano,SIPARISLER.sip_subeno,SIPARISLER.sip_tarih,SIPARISLER.sip_teslim_tarih,SIPARISLER.sip_tip,SIPARISLER.sip_cins,SIPARISLER.sip_evrakno_seri,SIPARISLER.sip_evrakno_sira,SIPARISLER.sip_satirno,SIPARISLER.sip_belgeno,SIPARISLER.sip_belge_tarih,SIPARISLER.sip_satici_kod,SIPARISLER.sip_musteri_kod,SIPARISLER.sip_stok_kod,SIPARISLER.sip_b_fiyat,SIPARISLER.sip_miktar,SIPARISLER.sip_birim_pntr,SIPARISLER.sip_teslim_miktar,SIPARISLER.sip_tutar,SIPARISLER.sip_iskonto_1,SIPARISLER.sip_iskonto_2,SIPARISLER.sip_iskonto_3,SIPARISLER.sip_iskonto_4,SIPARISLER.sip_iskonto_5,SIPARISLER.sip_iskonto_6,SIPARISLER.sip_masraf_1,SIPARISLER.sip_masraf_2,SIPARISLER.sip_masraf_3,SIPARISLER.sip_masraf_4,SIPARISLER.sip_vergi_pntr,SIPARISLER.sip_vergi,SIPARISLER.sip_masvergi_pntr,SIPARISLER.sip_masvergi,SIPARISLER.sip_opno,SIPARISLER.sip_aciklama,SIPARISLER.sip_aciklama2,SIPARISLER.sip_depono,SIPARISLER.sip_OnaylayanKulNo,SIPARISLER.sip_vergisiz_fl,SIPARISLER.sip_kapat_fl,SIPARISLER.sip_promosyon_fl,SIPARISLER.sip_cari_sormerk,SIPARISLER.sip_stok_sormerk,SIPARISLER.sip_cari_grupno,SIPARISLER.sip_doviz_cinsi,SIPARISLER.sip_doviz_kuru,SIPARISLER.sip_alt_doviz_kuru,SIPARISLER.sip_adresno,SIPARISLER.sip_teslimturu,SIPARISLER.sip_cagrilabilir_fl,0,SIPARISLER.sip_prosip_uid,SIPARISLER.sip_iskonto1,SIPARISLER.sip_iskonto2,SIPARISLER.sip_iskonto3,SIPARISLER.sip_iskonto4,SIPARISLER.sip_iskonto5,SIPARISLER.sip_iskonto6,SIPARISLER.sip_masraf1,SIPARISLER.sip_masraf2,SIPARISLER.sip_masraf3,SIPARISLER.sip_masraf4,SIPARISLER.sip_isk1,SIPARISLER.sip_isk2,SIPARISLER.sip_isk3,SIPARISLER.sip_isk4,SIPARISLER.sip_isk5,SIPARISLER.sip_isk6,SIPARISLER.sip_mas1,SIPARISLER.sip_mas2,SIPARISLER.sip_mas3,SIPARISLER.sip_mas4,SIPARISLER.sip_Exp_Imp_Kodu,SIPARISLER.sip_kar_orani,SIPARISLER.sip_durumu,0,SIPARISLER.sip_stal_uid,SIPARISLER.sip_planlananmiktar,0,SIPARISLER.sip_teklif_uid,SIPARISLER.sip_parti_kodu,SIPARISLER.sip_lot_no,SIPARISLER.sip_projekodu,SIPARISLER.sip_fiyat_liste_no,SIPARISLER.sip_Otv_Pntr,SIPARISLER.sip_Otv_Vergi,SIPARISLER.sip_otvtutari,SIPARISLER.sip_OtvVergisiz_Fl,SIPARISLER.sip_paket_kod,0,SIPARISLER.sip_Rez_uid,SIPARISLER.sip_harekettipi,0,SIPARISLER.sip_yetkili_uid,SIPARISLER.sip_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOKLAR.sto_bedenli_takip,STOKLAR.sto_renkDetayli FROM SIPARISLER INNER JOIN STOKLAR ON STOKLAR.sto_kod=SIPARISLER.sip_stok_kod WHERE sip_Guid=@sip_Guid";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@sip_Guid", (object)sip_Guid);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					sIPARISLER.sip_Guid = ((DbDataReader)val2).GetGuid(0);
					sIPARISLER.sip_SpecRECno = val2.GetSafeInt32(3);
					sIPARISLER.sip_iptal = val2.GetSafeBoolean(4);
					sIPARISLER.sip_fileid = val2.GetSafeInt16(5);
					sIPARISLER.sip_hidden = val2.GetSafeBoolean(6);
					sIPARISLER.sip_kilitli = val2.GetSafeBoolean(7);
					sIPARISLER.sip_degisti = val2.GetSafeBoolean(8);
					sIPARISLER.sip_checksum = val2.GetSafeInt32(9);
					sIPARISLER.sip_create_user = val2.GetSafeInt16(10);
					sIPARISLER.sip_create_date = val2.GetSafeDateTime(11);
					sIPARISLER.sip_lastup_user = val2.GetSafeInt16(12);
					sIPARISLER.sip_special1 = val2.GetSafeString(13);
					sIPARISLER.sip_special2 = val2.GetSafeString(14);
					sIPARISLER.sip_special3 = val2.GetSafeString(15);
					sIPARISLER.sip_firmano = val2.GetSafeInt32(16);
					sIPARISLER.sip_subeno = val2.GetSafeInt32(17);
					sIPARISLER.sip_tarih = val2.GetSafeDateTime(18);
					sIPARISLER.sip_teslim_tarih = val2.GetSafeDateTime(19);
					sIPARISLER.sip_tip = (enum_sip_tip)val2.GetSafeByte(20);
					sIPARISLER.sip_cins = (enum_sip_cins)val2.GetSafeByte(21);
					sIPARISLER.sip_evrakno_seri = val2.GetSafeString(22);
					sIPARISLER.sip_evrakno_sira = val2.GetSafeInt32(23);
					sIPARISLER.sip_satirno = val2.GetSafeInt32(24);
					sIPARISLER.sip_belgeno = val2.GetSafeString(25);
					sIPARISLER.sip_belge_tarih = val2.GetSafeDateTime(26);
					sIPARISLER.sip_satici_kod = val2.GetSafeString(27);
					sIPARISLER.sip_musteri_kod = val2.GetSafeString(28);
					sIPARISLER.sip_stok_kod = val2.GetSafeString(29);
					sIPARISLER.sip_b_fiyat = val2.GetSafeDouble(30);
					sIPARISLER.sip_miktar = val2.GetSafeDouble(31);
					sIPARISLER.sip_birim_pntr = val2.GetSafeByte(32);
					sIPARISLER.sip_teslim_miktar = val2.GetSafeDouble(33);
					sIPARISLER.sip_tutar = val2.GetSafeDouble(34);
					sIPARISLER.sip_iskonto_1 = val2.GetSafeDouble(35);
					sIPARISLER.sip_iskonto_2 = val2.GetSafeDouble(36);
					sIPARISLER.sip_iskonto_3 = val2.GetSafeDouble(37);
					sIPARISLER.sip_iskonto_4 = val2.GetSafeDouble(38);
					sIPARISLER.sip_iskonto_5 = val2.GetSafeDouble(39);
					sIPARISLER.sip_iskonto_6 = val2.GetSafeDouble(40);
					sIPARISLER.sip_masraf_1 = val2.GetSafeDouble(41);
					sIPARISLER.sip_masraf_2 = val2.GetSafeDouble(42);
					sIPARISLER.sip_masraf_3 = val2.GetSafeDouble(43);
					sIPARISLER.sip_masraf_4 = val2.GetSafeDouble(44);
					sIPARISLER.sip_vergi_pntr = val2.GetSafeByte(45);
					sIPARISLER.sip_vergi = val2.GetSafeDouble(46);
					sIPARISLER.sip_masvergi_pntr = val2.GetSafeByte(47);
					sIPARISLER.sip_masvergi = val2.GetSafeDouble(48);
					sIPARISLER.sip_opno = val2.GetSafeInt32(49);
					sIPARISLER.sip_aciklama = val2.GetSafeString(50);
					sIPARISLER.sip_aciklama2 = val2.GetSafeString(51);
					sIPARISLER.sip_depono = val2.GetSafeInt32(52);
					sIPARISLER.sip_OnaylayanKulNo = val2.GetSafeInt16(53);
					sIPARISLER.sip_vergisiz_fl = val2.GetSafeBoolean(54);
					sIPARISLER.sip_kapat_fl = val2.GetSafeBoolean(55);
					sIPARISLER.sip_promosyon_fl = val2.GetSafeBoolean(56);
					sIPARISLER.sip_cari_sormerk = val2.GetSafeString(57);
					sIPARISLER.sip_stok_sormerk = val2.GetSafeString(58);
					sIPARISLER.sip_cari_grupno = val2.GetSafeByte(59);
					sIPARISLER.sip_doviz_cinsi = val2.GetSafeByte(60);
					sIPARISLER.sip_doviz_kuru = val2.GetSafeDouble(61);
					sIPARISLER.sip_alt_doviz_kuru = val2.GetSafeDouble(62);
					sIPARISLER.sip_adresno = val2.GetSafeInt32(63);
					sIPARISLER.sip_teslimturu = val2.GetSafeString(64);
					sIPARISLER.sip_cagrilabilir_fl = val2.GetSafeBoolean(65);
					sIPARISLER.sip_prosip_uid = ((DbDataReader)val2).GetGuid(67);
					sIPARISLER.sip_iskonto1 = val2.GetSafeByte(68);
					sIPARISLER.sip_iskonto2 = val2.GetSafeByte(69);
					sIPARISLER.sip_iskonto3 = val2.GetSafeByte(70);
					sIPARISLER.sip_iskonto4 = val2.GetSafeByte(71);
					sIPARISLER.sip_iskonto5 = val2.GetSafeByte(72);
					sIPARISLER.sip_iskonto6 = val2.GetSafeByte(73);
					sIPARISLER.sip_masraf1 = val2.GetSafeByte(74);
					sIPARISLER.sip_masraf2 = val2.GetSafeByte(75);
					sIPARISLER.sip_masraf3 = val2.GetSafeByte(76);
					sIPARISLER.sip_masraf4 = val2.GetSafeByte(77);
					sIPARISLER.sip_isk1 = val2.GetSafeBoolean(78);
					sIPARISLER.sip_isk2 = val2.GetSafeBoolean(79);
					sIPARISLER.sip_isk3 = val2.GetSafeBoolean(80);
					sIPARISLER.sip_isk4 = val2.GetSafeBoolean(81);
					sIPARISLER.sip_isk5 = val2.GetSafeBoolean(82);
					sIPARISLER.sip_isk6 = val2.GetSafeBoolean(83);
					sIPARISLER.sip_mas1 = val2.GetSafeBoolean(84);
					sIPARISLER.sip_mas2 = val2.GetSafeBoolean(85);
					sIPARISLER.sip_mas3 = val2.GetSafeBoolean(86);
					sIPARISLER.sip_mas4 = val2.GetSafeBoolean(87);
					sIPARISLER.sip_Exp_Imp_Kodu = val2.GetSafeString(88);
					sIPARISLER.sip_kar_orani = val2.GetSafeDouble(89);
					sIPARISLER.sip_durumu = (enum_sip_durumu)val2.GetSafeByte(90);
					sIPARISLER.sip_stal_uid = ((DbDataReader)val2).GetGuid(92);
					sIPARISLER.sip_planlananmiktar = val2.GetSafeDouble(93);
					sIPARISLER.sip_teklif_uid = ((DbDataReader)val2).GetGuid(95);
					sIPARISLER.sip_parti_kodu = val2.GetSafeString(96);
					sIPARISLER.sip_lot_no = val2.GetSafeInt32(97);
					sIPARISLER.sip_projekodu = val2.GetSafeString(98);
					sIPARISLER.sip_fiyat_liste_no = val2.GetSafeInt32(99);
					sIPARISLER.sip_Otv_Pntr = val2.GetSafeByte(100);
					sIPARISLER.sip_Otv_Vergi = val2.GetSafeDouble(101);
					sIPARISLER.sip_otvtutari = val2.GetSafeDouble(102);
					sIPARISLER.sip_OtvVergisiz_Fl = val2.GetSafeByte(103);
					sIPARISLER.sip_paket_kod = val2.GetSafeString(104);
					sIPARISLER.sip_Rez_uid = ((DbDataReader)val2).GetGuid(106);
					sIPARISLER.sip_harekettipi = (enum_sip_harekettipi)val2.GetSafeByte(107);
					sIPARISLER.sip_yetkili_uid = ((DbDataReader)val2).GetGuid(109);
					sIPARISLER.sip_kapatmanedenkod = val2.GetSafeString(110);
					sIPARISLER.sto_isim = val2.GetSafeString(111);
					sIPARISLER.sto_birim1_ad = val2.GetSafeString(112);
					sIPARISLER.sto_birim2_ad = val2.GetSafeString(113);
					sIPARISLER.sto_birim3_ad = val2.GetSafeString(114);
					sIPARISLER.sto_birim4_ad = val2.GetSafeString(115);
					sIPARISLER.sto_birim1_katsayi = val2.GetSafeDouble(116);
					sIPARISLER.sto_birim2_katsayi = val2.GetSafeDouble(117);
					sIPARISLER.sto_birim3_katsayi = val2.GetSafeDouble(118);
					sIPARISLER.sto_birim4_katsayi = val2.GetSafeDouble(119);
					bool safeBoolean = val2.GetSafeBoolean(120);
					bool safeBoolean2 = val2.GetSafeBoolean(121);
					if (safeBoolean || safeBoolean2)
					{
						sIPARISLER.renk_beden_hareketleri = StokSqlite.V16_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.Siparis, sIPARISLER.sip_Guid);
					}
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return sIPARISLER;
	}

	public static List<GenelEvrakListItem> GetGenelEvrakList(SqliteConnection OpenedConnection, enum_cha_evrak_tip cha_evrak_tip, string ekstra_where)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(((DbConnection)OpenedConnection).DataSource);
		List<GenelEvrakListItem> list = new List<GenelEvrakListItem>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string text = "";
				text = ((mikroVersiyon <= 15) ? "cha_RECid_RECno" : "cha_Guid");
				string text2 = "SELECT CARI_HESAP_HAREKETLERI." + text + ",CARI_HESAP_HAREKETLERI.cha_tarihi,CARI_HESAP_HAREKETLERI.cha_evrakno_seri,CARI_HESAP_HAREKETLERI.cha_evrakno_sira,CARI_HESAP_HAREKETLERI.cha_kod,CARI_HESAPLAR.cari_unvan1,CARI_HESAPLAR.cari_unvan2 FROM CARI_HESAP_HAREKETLERI INNER JOIN CARI_HESAPLAR ON CARI_HESAPLAR.cari_kod=CARI_HESAP_HAREKETLERI.cha_kod WHERE cha_evrak_tip=@cha_evrak_tip ";
				val.Parameters.AddWithValue("@cha_evrak_tip", (object)(int)cha_evrak_tip);
				text2 += ekstra_where;
				text2 += " ORDER BY cha_tarihi DESC,cha_evrakno_seri,cha_evrakno_sira DESC";
				((DbCommand)val).CommandText = text2;
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					GenelEvrakListItem genelEvrakListItem = new GenelEvrakListItem();
					if (mikroVersiyon > 15)
					{
						genelEvrakListItem.cha_RECid_RECno = 0;
						genelEvrakListItem.cha_Guid = ((DbDataReader)val2).GetGuid(0);
					}
					else
					{
						genelEvrakListItem.cha_RECid_RECno = val2.GetSafeInt32(0);
						genelEvrakListItem.cha_Guid = Guid.Empty;
					}
					genelEvrakListItem.cha_tarihi = val2.GetSafeDateTime(1);
					genelEvrakListItem.cha_evrakno_seri = val2.GetSafeString(2);
					genelEvrakListItem.cha_evrakno_sira = val2.GetSafeInt32(3);
					genelEvrakListItem.cha_kod = val2.GetSafeString(4);
					genelEvrakListItem.cari_unvan1 = val2.GetSafeString(5);
					genelEvrakListItem.cari_unvan2 = val2.GetSafeString(6);
					list.Add(genelEvrakListItem);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}
}
