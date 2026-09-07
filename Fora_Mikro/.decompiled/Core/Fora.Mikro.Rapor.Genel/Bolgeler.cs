using System;
using System.Collections.Generic;

namespace Fora.Mikro.Rapor.Genel;

public class Bolgeler
{
	public class Bolge
	{
		public class Temsilci
		{
			public class GunlukHareketler
			{
				public class Ziyaret
				{
					public bool ziyaret_edildi { get; set; }

					public bool rotada_var { get; set; }

					public string cari_kodu { get; set; }

					public string cari_ismi { get; set; }

					public string adres { get; set; }

					public TimeSpan baslama_zamani { get; set; }

					public TimeSpan bitis_zamani { get; set; }

					public string ziyaret_edildi_uzun
					{
						get
						{
							if (ziyaret_edildi)
							{
								return "Evet";
							}
							return "Hayır";
						}
					}

					public string ziyaret_edildi_kisa
					{
						get
						{
							if (ziyaret_edildi)
							{
								return "E";
							}
							return "H";
						}
					}

					public string rotada_var_uzun
					{
						get
						{
							if (rotada_var)
							{
								return "Evet";
							}
							return "Hayır";
						}
					}

					public string rotada_var_kisa
					{
						get
						{
							if (rotada_var)
							{
								return "E";
							}
							return "H";
						}
					}
				}

				public class Siparis
				{
					public string cari_kodu { get; set; }

					public string cari_ismi { get; set; }

					public double tutar { get; set; }

					public int tipi { get; set; }

					public string evrak_seri { get; set; }

					public int evrak_sira { get; set; }
				}

				public class Fatura
				{
					public string cari_kodu { get; set; }

					public string cari_ismi { get; set; }

					public string aciklama { get; set; }

					public double tutar { get; set; }

					public int tipi { get; set; }

					public string evrak_seri { get; set; }

					public int evrak_sira { get; set; }
				}

				public class Tahsilat
				{
					public string cari_kodu { get; set; }

					public string cari_ismi { get; set; }

					public string aciklama { get; set; }

					public double tutar { get; set; }

					public tahsilat_tipleri tipi { get; set; }

					public string evrak_seri { get; set; }

					public int evrak_sira { get; set; }

					public string tipi_uzun => tipi switch
					{
						tahsilat_tipleri.Cek => "Çek", 
						tahsilat_tipleri.KrediKarti => "Kredi Kartı", 
						tahsilat_tipleri.Nakit => "Nakit", 
						tahsilat_tipleri.Senet => "Senet", 
						_ => "Tanımsız", 
					};

					public string tipi_kisa => tipi switch
					{
						tahsilat_tipleri.Cek => "Ç", 
						tahsilat_tipleri.KrediKarti => "KK", 
						tahsilat_tipleri.Nakit => "N", 
						tahsilat_tipleri.Senet => "S", 
						_ => "T", 
					};
				}

				public class Masraf
				{
					public string masraf_kodu { get; set; }

					public string masraf_ismi { get; set; }

					public string aciklama { get; set; }

					public double tutar { get; set; }

					public string evrak_seri { get; set; }

					public int evrak_sira { get; set; }
				}

				public enum tahsilat_tipleri
				{
					Nakit = 0,
					Cek = 1,
					KrediKarti = 2,
					Senet = 3,
					Tanimsiz = 99
				}

				public DateTime tarih { get; set; }

				public TimeSpan baslama_saati { get; set; }

				public int baslama_arac_km { get; set; }

				public string baslama_mesaj { get; set; }

				public TimeSpan bitis_saati { get; set; }

				public int bitis_arac_km { get; set; }

				public string bitis_mesaj { get; set; }

				public List<Ziyaret> ziyaret_listesi { get; set; }

				public List<Siparis> siparisler { get; set; }

				public List<Fatura> faturalar { get; set; }

				public List<Tahsilat> tahsilatlar { get; set; }

				public List<Masraf> masraflar { get; set; }

				public List<Ziyaret> yapilan_ziyaretler
				{
					get
					{
						List<Ziyaret> list = new List<Ziyaret>();
						foreach (Ziyaret item in ziyaret_listesi)
						{
							if (item.ziyaret_edildi)
							{
								list.Add(item);
							}
						}
						return list;
					}
				}

				public List<Ziyaret> yapilan_hedef_ziyaretler
				{
					get
					{
						List<Ziyaret> list = new List<Ziyaret>();
						foreach (Ziyaret item in ziyaret_listesi)
						{
							if (item.ziyaret_edildi && item.rotada_var)
							{
								list.Add(item);
							}
						}
						return list;
					}
				}

				public List<Ziyaret> hedef_ziyaretler
				{
					get
					{
						List<Ziyaret> list = new List<Ziyaret>();
						foreach (Ziyaret item in ziyaret_listesi)
						{
							if (item.rotada_var)
							{
								list.Add(item);
							}
						}
						return list;
					}
				}

				public List<Ziyaret> yapilmayan_ziyaretler
				{
					get
					{
						List<Ziyaret> list = new List<Ziyaret>();
						foreach (Ziyaret item in ziyaret_listesi)
						{
							if (!item.ziyaret_edildi && item.rotada_var)
							{
								list.Add(item);
							}
						}
						return list;
					}
				}

				public List<Ziyaret> rota_disi_ziyaretler
				{
					get
					{
						List<Ziyaret> list = new List<Ziyaret>();
						foreach (Ziyaret item in ziyaret_listesi)
						{
							if (item.ziyaret_edildi && !item.rotada_var)
							{
								list.Add(item);
							}
						}
						return list;
					}
				}

				public int yapilan_km => bitis_arac_km - baslama_arac_km;

				public TimeSpan mesai_suresi => bitis_saati.Subtract(baslama_saati);

				public TimeSpan toplam_ziyaret_suresi
				{
					get
					{
						TimeSpan result = default(TimeSpan);
						foreach (Ziyaret item in yapilan_ziyaretler)
						{
							result = result.Add(item.bitis_zamani.Subtract(item.baslama_zamani));
						}
						return result;
					}
				}

				public TimeSpan ziyaret_basi_ortalama_ziyaret_suresi => TimeSpan.FromSeconds((int)(toplam_ziyaret_suresi.TotalSeconds / (double)yapilan_ziyaret_sayisi));

				public TimeSpan toplam_ulasim_suresi => mesai_suresi.Subtract(toplam_ziyaret_suresi);

				public int hedef_ziyaret_sayisi => hedef_ziyaretler.Count;

				public int yapilan_ziyaret_sayisi => yapilan_ziyaretler.Count;

				public int yapilmayan_ziyaret_sayisi => yapilmayan_ziyaretler.Count;

				public int rota_disi_ziyaret_sayisi => rota_disi_ziyaretler.Count;

				public int siparis_adedi
				{
					get
					{
						int num = 0;
						List<int> list = new List<int>();
						foreach (Siparis item in siparisler)
						{
							bool flag = false;
							foreach (int item2 in list)
							{
								if (item.evrak_sira == item2)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								num++;
								list.Add(item.evrak_sira);
							}
						}
						return num;
					}
				}

				public int fatura_adedi
				{
					get
					{
						int num = 0;
						List<int> list = new List<int>();
						foreach (Fatura item in faturalar)
						{
							bool flag = false;
							foreach (int item2 in list)
							{
								if (item.evrak_sira == item2)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								num++;
								list.Add(item.evrak_sira);
							}
						}
						return num;
					}
				}

				public int tahsilat_adedi
				{
					get
					{
						int num = 0;
						List<int> list = new List<int>();
						foreach (Tahsilat item in tahsilatlar)
						{
							bool flag = false;
							foreach (int item2 in list)
							{
								if (item.evrak_sira == item2)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								num++;
								list.Add(item.evrak_sira);
							}
						}
						return num;
					}
				}

				public int masraf_adedi
				{
					get
					{
						int num = 0;
						List<int> list = new List<int>();
						foreach (Masraf item in masraflar)
						{
							bool flag = false;
							foreach (int item2 in list)
							{
								if (item.evrak_sira == item2)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								num++;
								list.Add(item.evrak_sira);
							}
						}
						return num;
					}
				}

				public int cek_adedi
				{
					get
					{
						int num = 0;
						List<int> list = new List<int>();
						foreach (Tahsilat item in tahsilatlar)
						{
							if (item.tipi == tahsilat_tipleri.Cek)
							{
								num++;
							}
							list.Add(item.evrak_sira);
						}
						return num;
					}
				}

				public int kk_adedi
				{
					get
					{
						int num = 0;
						List<int> list = new List<int>();
						foreach (Tahsilat item in tahsilatlar)
						{
							if (item.tipi == tahsilat_tipleri.KrediKarti)
							{
								num++;
							}
							list.Add(item.evrak_sira);
						}
						return num;
					}
				}

				public double siparis_tutari
				{
					get
					{
						double num = 0.0;
						foreach (Siparis item in siparisler)
						{
							num += item.tutar;
						}
						return num;
					}
				}

				public double ziyaret_basi_ortalama_siparis_tutari => siparis_tutari / (double)yapilan_ziyaret_sayisi;

				public double fatura_tutari
				{
					get
					{
						double num = 0.0;
						foreach (Fatura item in faturalar)
						{
							num += item.tutar;
						}
						return num;
					}
				}

				public double ziyaret_basi_ortalama_fatura_tutari => fatura_tutari / (double)yapilan_ziyaret_sayisi;

				public double nakit_tahsilat_tutari
				{
					get
					{
						double num = 0.0;
						foreach (Tahsilat item in tahsilatlar)
						{
							if (item.tipi == tahsilat_tipleri.Nakit)
							{
								num += item.tutar;
							}
						}
						return num;
					}
				}

				public double ziyaret_basi_ortalama_nakit_tahsilat_tutari => nakit_tahsilat_tutari / (double)yapilan_ziyaret_sayisi;

				public double cek_tutari
				{
					get
					{
						double num = 0.0;
						foreach (Tahsilat item in tahsilatlar)
						{
							if (item.tipi == tahsilat_tipleri.Cek)
							{
								num += item.tutar;
							}
						}
						return num;
					}
				}

				public double ziyaret_basi_ortalama_cek_tutari => cek_tutari / (double)yapilan_ziyaret_sayisi;

				public double kk_tutari
				{
					get
					{
						double num = 0.0;
						foreach (Tahsilat item in tahsilatlar)
						{
							if (item.tipi == tahsilat_tipleri.KrediKarti)
							{
								num += item.tutar;
							}
						}
						return num;
					}
				}

				public double ziyaret_basi_ortalama_kk_tutari => kk_tutari / (double)yapilan_ziyaret_sayisi;

				public double masraf_tutari
				{
					get
					{
						double num = 0.0;
						foreach (Masraf item in masraflar)
						{
							num += item.tutar;
						}
						return num;
					}
				}

				public double kalan_nakit_tutar => nakit_tahsilat_tutari - masraf_tutari;

				public double toplam_tahsilat_tutari
				{
					get
					{
						double num = 0.0;
						foreach (Tahsilat item in tahsilatlar)
						{
							num += item.tutar;
						}
						return num;
					}
				}

				public GunlukHareketler()
				{
					ziyaret_listesi = new List<Ziyaret>();
					siparisler = new List<Siparis>();
					faturalar = new List<Fatura>();
					tahsilatlar = new List<Tahsilat>();
					masraflar = new List<Masraf>();
				}
			}

			public List<GunlukHareketler> gunler { get; set; }

			public string bolge_kodu { get; set; }

			public string bolge_adi { get; set; }

			public string temsilci_kodu { get; set; }

			public string temsilci_adi { get; set; }

			public string evrak_seri_alinan_siparis { get; set; }

			public string evrak_seri_satis_faturasi { get; set; }

			public string evrak_seri_tahsilat { get; set; }

			public string evrak_seri_masraf { get; set; }

			public int gun_sayisi => gunler.Count;

			public DateTime baslangic_tarihi
			{
				get
				{
					DateTime dateTime = DateTime.Now.AddYears(10);
					foreach (GunlukHareketler item in gunler)
					{
						if (dateTime > item.tarih)
						{
							dateTime = item.tarih;
						}
					}
					return dateTime;
				}
			}

			public DateTime bitis_tarihi
			{
				get
				{
					DateTime dateTime = DateTime.Now.AddYears(-10);
					foreach (GunlukHareketler item in gunler)
					{
						if (dateTime < item.tarih)
						{
							dateTime = item.tarih;
						}
					}
					return dateTime;
				}
			}

			public int arac_baslangic_km
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						if (num > item.baslama_arac_km)
						{
							num = item.baslama_arac_km;
						}
					}
					if (num == int.MinValue)
					{
						num = 0;
					}
					return num;
				}
			}

			public int arac_bitis_km
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						if (num < item.bitis_arac_km)
						{
							num = item.bitis_arac_km;
						}
					}
					if (num == int.MinValue)
					{
						num = 0;
					}
					return num;
				}
			}

			public TimeSpan toplam_mesai_suresi
			{
				get
				{
					TimeSpan result = default(TimeSpan);
					foreach (GunlukHareketler item in gunler)
					{
						result = result.Add(item.mesai_suresi);
					}
					return result;
				}
			}

			public TimeSpan toplam_ziyaret_suresi
			{
				get
				{
					TimeSpan result = default(TimeSpan);
					foreach (GunlukHareketler item in gunler)
					{
						result = result.Add(item.toplam_ziyaret_suresi);
					}
					return result;
				}
			}

			public TimeSpan toplam_ulasim_suresi
			{
				get
				{
					TimeSpan result = default(TimeSpan);
					foreach (GunlukHareketler item in gunler)
					{
						result = result.Add(item.toplam_ulasim_suresi);
					}
					return result;
				}
			}

			public int toplam_yapilan_km
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.yapilan_km;
					}
					return num;
				}
			}

			public int toplam_hedef_ziyaret
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.hedef_ziyaret_sayisi;
					}
					return num;
				}
			}

			public int toplam_yapilan_ziyaret
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.yapilan_ziyaret_sayisi;
					}
					return num;
				}
			}

			public int toplam_yapilmayan_ziyaret
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.yapilmayan_ziyaret_sayisi;
					}
					return num;
				}
			}

			public int toplam_rota_disi_ziyaret
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.rota_disi_ziyaret_sayisi;
					}
					return num;
				}
			}

			public int toplam_siparis_adedi
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.siparis_adedi;
					}
					return num;
				}
			}

			public double toplam_siparis_tutari
			{
				get
				{
					double num = 0.0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.siparis_tutari;
					}
					return num;
				}
			}

			public int toplam_fatura_adedi
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.fatura_adedi;
					}
					return num;
				}
			}

			public double toplam_fatura_tutari
			{
				get
				{
					double num = 0.0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.fatura_tutari;
					}
					return num;
				}
			}

			public int toplam_tahsilat_evrak_adedi
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.tahsilat_adedi;
					}
					return num;
				}
			}

			public int toplam_masraf_adedi
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.masraf_adedi;
					}
					return num;
				}
			}

			public double toplam_nakit_tahsilat_tutari
			{
				get
				{
					double num = 0.0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.nakit_tahsilat_tutari;
					}
					return num;
				}
			}

			public double toplam_masraf_tutari
			{
				get
				{
					double num = 0.0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.masraf_tutari;
					}
					return num;
				}
			}

			public double toplam_kalan_nakit_tutar
			{
				get
				{
					double num = 0.0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.kalan_nakit_tutar;
					}
					return num;
				}
			}

			public int toplam_cek_adedi
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.cek_adedi;
					}
					return num;
				}
			}

			public double toplam_cek_tutari
			{
				get
				{
					double num = 0.0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.cek_tutari;
					}
					return num;
				}
			}

			public int toplam_kk_adedi
			{
				get
				{
					int num = 0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.kk_adedi;
					}
					return num;
				}
			}

			public double toplam_kk_tutari
			{
				get
				{
					double num = 0.0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.kk_tutari;
					}
					return num;
				}
			}

			public double toplam_tahsilat_tutari
			{
				get
				{
					double num = 0.0;
					foreach (GunlukHareketler item in gunler)
					{
						num += item.toplam_tahsilat_tutari;
					}
					return num;
				}
			}

			public TimeSpan ziyaret_basi_ortalama_ziyaret_suresi => TimeSpan.FromSeconds((int)(toplam_ziyaret_suresi.TotalSeconds / (double)toplam_yapilan_ziyaret));

			public TimeSpan ziyaret_basi_ortalama_ulasim_suresi => TimeSpan.FromSeconds((int)(toplam_ulasim_suresi.TotalSeconds / (double)toplam_yapilan_ziyaret));

			public double ziyaret_basi_ortalama_yapilan_km => (double)toplam_yapilan_km / (double)toplam_yapilan_ziyaret;

			public double ziyaret_basi_ortalama_siparis_tutari => toplam_siparis_tutari / (double)toplam_yapilan_ziyaret;

			public double ziyaret_basi_ortalama_fatura_tutari => toplam_fatura_tutari / (double)toplam_yapilan_ziyaret;

			public double ziyaret_basi_ortalama_nakit_tahsilat_tutari => toplam_nakit_tahsilat_tutari / (double)toplam_yapilan_ziyaret;

			public double ziyaret_basi_ortalama_cek_tutari => toplam_cek_tutari / (double)toplam_yapilan_ziyaret;

			public double ziyaret_basi_ortalama_kk_tutari => toplam_kk_tutari / (double)toplam_yapilan_ziyaret;

			public double ziyaret_basi_ortalama_tahsilat_tutari => toplam_tahsilat_tutari / (double)toplam_yapilan_ziyaret;

			public TimeSpan gunluk_ortalama_mesai_suresi => TimeSpan.FromSeconds((int)(toplam_mesai_suresi.TotalSeconds / (double)gun_sayisi));

			public TimeSpan gunluk_ortalama_ziyaret_suresi => TimeSpan.FromSeconds((int)(toplam_ziyaret_suresi.TotalSeconds / (double)gun_sayisi));

			public TimeSpan gunluk_ortalama_ulasim_suresi => TimeSpan.FromSeconds((int)(toplam_ulasim_suresi.TotalSeconds / (double)gun_sayisi));

			public double gunluk_ortalama_yapilan_km => (double)toplam_yapilan_km / (double)gun_sayisi;

			public double gunluk_ortalama_hedef_ziyaret => (double)toplam_hedef_ziyaret / (double)gun_sayisi;

			public double gunluk_ortalama_yapilan_ziyaret => (double)toplam_yapilan_ziyaret / (double)gun_sayisi;

			public double gunluk_ortalama_yapilmayan_ziyaret => (double)toplam_yapilmayan_ziyaret / (double)gun_sayisi;

			public double gunluk_ortalama_rota_disi_ziyaret => (double)toplam_rota_disi_ziyaret / (double)gun_sayisi;

			public double gunluk_ortalama_siparis_adedi => (double)toplam_siparis_adedi / (double)gun_sayisi;

			public double gunluk_ortalama_siparis_tutari => toplam_siparis_tutari / (double)gun_sayisi;

			public double gunluk_ortalama_fatura_adedi => (double)toplam_fatura_adedi / (double)gun_sayisi;

			public double gunluk_ortalama_fatura_tutari => toplam_fatura_tutari / (double)gun_sayisi;

			public double gunluk_ortalama_tahsilat_evrak_adedi => (double)toplam_tahsilat_evrak_adedi / (double)gun_sayisi;

			public double gunluk_ortalama_masraf_adedi => (double)toplam_masraf_adedi / (double)gun_sayisi;

			public double gunluk_ortalama_nakit_tahsilat_tutari => toplam_nakit_tahsilat_tutari / (double)gun_sayisi;

			public double gunluk_ortalama_masraf_tutari => toplam_masraf_tutari / (double)gun_sayisi;

			public double gunluk_ortalama_kalan_nakit_tutar => toplam_kalan_nakit_tutar / (double)gun_sayisi;

			public double gunluk_ortalama_cek_adedi => (double)toplam_cek_adedi / (double)gun_sayisi;

			public double gunluk_ortalama_cek_tutari => toplam_cek_tutari / (double)gun_sayisi;

			public double gunluk_ortalama_kk_adedi => (double)toplam_kk_adedi / (double)gun_sayisi;

			public double gunluk_ortalama_kk_tutari => toplam_kk_tutari / (double)gun_sayisi;

			public double gunluk_ortalama_tahsilat_tutari => toplam_tahsilat_tutari / (double)gun_sayisi;

			public Temsilci()
			{
				gunler = new List<GunlukHareketler>();
			}
		}

		public List<Temsilci> temsilciler { get; set; }

		public string bolge_kodu { get; set; }

		public string bolge_adi { get; set; }

		public int temsilci_sayisi => temsilciler.Count;

		public int toplam_gun_sayisi
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.gun_sayisi;
				}
				return num;
			}
		}

		public DateTime baslangic_tarihi
		{
			get
			{
				DateTime dateTime = DateTime.Now.AddYears(10);
				foreach (Temsilci item in temsilciler)
				{
					if (dateTime > item.baslangic_tarihi)
					{
						dateTime = item.baslangic_tarihi;
					}
				}
				return dateTime;
			}
		}

		public DateTime bitis_tarihi
		{
			get
			{
				DateTime dateTime = DateTime.Now.AddYears(-10);
				foreach (Temsilci item in temsilciler)
				{
					if (dateTime < item.bitis_tarihi)
					{
						dateTime = item.bitis_tarihi;
					}
				}
				return dateTime;
			}
		}

		public TimeSpan toplam_mesai_suresi
		{
			get
			{
				TimeSpan result = default(TimeSpan);
				foreach (Temsilci item in temsilciler)
				{
					result = result.Add(item.toplam_mesai_suresi);
				}
				return result;
			}
		}

		public TimeSpan toplam_ziyaret_suresi
		{
			get
			{
				TimeSpan result = default(TimeSpan);
				foreach (Temsilci item in temsilciler)
				{
					result = result.Add(item.toplam_ziyaret_suresi);
				}
				return result;
			}
		}

		public TimeSpan toplam_ulasim_suresi
		{
			get
			{
				TimeSpan result = default(TimeSpan);
				foreach (Temsilci item in temsilciler)
				{
					result = result.Add(item.toplam_ulasim_suresi);
				}
				return result;
			}
		}

		public int toplam_yapilan_km
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_yapilan_km;
				}
				return num;
			}
		}

		public int toplam_hedef_ziyaret
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_hedef_ziyaret;
				}
				return num;
			}
		}

		public int toplam_yapilan_ziyaret
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_yapilan_ziyaret;
				}
				return num;
			}
		}

		public int toplam_yapilmayan_ziyaret
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_yapilmayan_ziyaret;
				}
				return num;
			}
		}

		public int toplam_rota_disi_ziyaret
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_rota_disi_ziyaret;
				}
				return num;
			}
		}

		public int toplam_siparis_adedi
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_siparis_adedi;
				}
				return num;
			}
		}

		public double toplam_siparis_tutari
		{
			get
			{
				double num = 0.0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_siparis_tutari;
				}
				return num;
			}
		}

		public int toplam_fatura_adedi
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_fatura_adedi;
				}
				return num;
			}
		}

		public double toplam_fatura_tutari
		{
			get
			{
				double num = 0.0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_fatura_tutari;
				}
				return num;
			}
		}

		public int toplam_tahsilat_evrak_adedi
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_tahsilat_evrak_adedi;
				}
				return num;
			}
		}

		public int toplam_masraf_adedi
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_masraf_adedi;
				}
				return num;
			}
		}

		public double toplam_nakit_tahsilat_tutari
		{
			get
			{
				double num = 0.0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_nakit_tahsilat_tutari;
				}
				return num;
			}
		}

		public double toplam_masraf_tutari
		{
			get
			{
				double num = 0.0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_masraf_tutari;
				}
				return num;
			}
		}

		public double toplam_kalan_nakit_tutar
		{
			get
			{
				double num = 0.0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_kalan_nakit_tutar;
				}
				return num;
			}
		}

		public int toplam_cek_adedi
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_cek_adedi;
				}
				return num;
			}
		}

		public double toplam_cek_tutari
		{
			get
			{
				double num = 0.0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_cek_tutari;
				}
				return num;
			}
		}

		public int toplam_kk_adedi
		{
			get
			{
				int num = 0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_kk_adedi;
				}
				return num;
			}
		}

		public double toplam_kk_tutari
		{
			get
			{
				double num = 0.0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_kk_tutari;
				}
				return num;
			}
		}

		public double toplam_tahsilat_tutari
		{
			get
			{
				double num = 0.0;
				foreach (Temsilci item in temsilciler)
				{
					num += item.toplam_tahsilat_tutari;
				}
				return num;
			}
		}

		public TimeSpan ziyaret_basi_ortalama_ziyaret_suresi => TimeSpan.FromSeconds((int)(toplam_ziyaret_suresi.TotalSeconds / (double)toplam_yapilan_ziyaret));

		public TimeSpan ziyaret_basi_ortalama_ulasim_suresi => TimeSpan.FromSeconds((int)(toplam_ulasim_suresi.TotalSeconds / (double)toplam_yapilan_ziyaret));

		public double ziyaret_basi_ortalama_yapilan_km => (double)toplam_yapilan_km / (double)toplam_yapilan_ziyaret;

		public double ziyaret_basi_ortalama_siparis_tutari => toplam_siparis_tutari / (double)toplam_yapilan_ziyaret;

		public double ziyaret_basi_ortalama_fatura_tutari => toplam_fatura_tutari / (double)toplam_yapilan_ziyaret;

		public double ziyaret_basi_ortalama_nakit_tahsilat_tutari => toplam_nakit_tahsilat_tutari / (double)toplam_yapilan_ziyaret;

		public double ziyaret_basi_ortalama_cek_tutari => toplam_cek_tutari / (double)toplam_yapilan_ziyaret;

		public double ziyaret_basi_ortalama_kk_tutari => toplam_kk_tutari / (double)toplam_yapilan_ziyaret;

		public double ziyaret_basi_ortalama_tahsilat_tutari => toplam_tahsilat_tutari / (double)toplam_yapilan_ziyaret;

		public TimeSpan temsilci_basi_mesai_suresi => TimeSpan.FromSeconds((int)(toplam_mesai_suresi.TotalSeconds / (double)temsilci_sayisi));

		public TimeSpan temsilci_basi_ulasim_suresi => TimeSpan.FromSeconds((int)(toplam_ulasim_suresi.TotalSeconds / (double)temsilci_sayisi));

		public double temsilci_basi_yapilan_km => (double)toplam_yapilan_km / (double)temsilci_sayisi;

		public double temsilci_basi_hedef_ziyaret => (double)toplam_hedef_ziyaret / (double)temsilci_sayisi;

		public double temsilci_basi_yapilan_ziyaret => (double)toplam_yapilan_ziyaret / (double)temsilci_sayisi;

		public double temsilci_basi_yapilmayan_ziyaret => (double)toplam_yapilmayan_ziyaret / (double)temsilci_sayisi;

		public double temsilci_basi_rota_disi_ziyaret => (double)toplam_rota_disi_ziyaret / (double)temsilci_sayisi;

		public double temsilci_basi_siparis_adedi => (double)toplam_siparis_adedi / (double)temsilci_sayisi;

		public double temsilci_basi_siparis_tutari => toplam_siparis_tutari / (double)temsilci_sayisi;

		public double temsilci_basi_fatura_adedi => (double)toplam_fatura_adedi / (double)temsilci_sayisi;

		public double temsilci_basi_fatura_tutari => toplam_fatura_tutari / (double)temsilci_sayisi;

		public double temsilci_basi_tahsilat_evrak_adedi => (double)toplam_tahsilat_evrak_adedi / (double)temsilci_sayisi;

		public double temsilci_basi_masraf_adedi => (double)toplam_masraf_adedi / (double)temsilci_sayisi;

		public double temsilci_basi_nakit_tahsilat_tutari => toplam_nakit_tahsilat_tutari / (double)temsilci_sayisi;

		public double temsilci_basi_masraf_tutari => toplam_masraf_tutari / (double)temsilci_sayisi;

		public double temsilci_basi_kalan_nakit_tutar => toplam_kalan_nakit_tutar / (double)temsilci_sayisi;

		public double temsilci_basi_cek_adedi => (double)toplam_cek_adedi / (double)temsilci_sayisi;

		public double temsilci_basi_cek_tutari => toplam_cek_tutari / (double)temsilci_sayisi;

		public double temsilci_basi_kk_adedi => (double)toplam_kk_adedi / (double)temsilci_sayisi;

		public double temsilci_basi_kk_tutari => toplam_kk_tutari / (double)temsilci_sayisi;

		public double temsilci_basi_tahsilat_tutari => toplam_tahsilat_tutari / (double)temsilci_sayisi;

		public TimeSpan gunluk_ortalama_ziyaret_suresi => TimeSpan.FromSeconds((int)(toplam_ziyaret_suresi.TotalSeconds / (double)toplam_gun_sayisi));

		public TimeSpan gunluk_ortalama_ulasim_suresi => TimeSpan.FromSeconds((int)(toplam_ulasim_suresi.TotalSeconds / (double)toplam_gun_sayisi));

		public double gunluk_ortalama_gunluk_yapilan_km => (double)toplam_yapilan_km / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_gunluk_hedef_ziyaret => (double)toplam_hedef_ziyaret / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_yapilan_ziyaret => (double)toplam_yapilan_ziyaret / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_yapilmayan_ziyaret => (double)toplam_yapilmayan_ziyaret / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_rota_disi_ziyaret => (double)toplam_rota_disi_ziyaret / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_siparis_adedi => (double)toplam_siparis_adedi / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_siparis_tutari => toplam_siparis_tutari / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_fatura_adedi => (double)toplam_fatura_adedi / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_fatura_tutari => toplam_fatura_tutari / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_tahsilat_evrak_adedi => (double)toplam_tahsilat_evrak_adedi / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_masraf_adedi => (double)toplam_masraf_adedi / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_nakit_tahsilat_tutari => toplam_nakit_tahsilat_tutari / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_masraf_tutari => toplam_masraf_tutari / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_kalan_nakit_tutar => toplam_kalan_nakit_tutar / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_cek_adedi => (double)toplam_cek_adedi / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_cek_tutari => toplam_cek_tutari / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_kk_adedi => (double)toplam_kk_adedi / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_kk_tutari => toplam_kk_tutari / (double)toplam_gun_sayisi;

		public double gunluk_ortalama_tahsilat_tutari => toplam_tahsilat_tutari / (double)toplam_gun_sayisi;

		public Bolge()
		{
			temsilciler = new List<Temsilci>();
		}
	}

	public List<Bolge> bolgeler { get; set; }

	public int bolge_sayisi => bolgeler.Count;

	public int temsilci_sayisi
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.temsilci_sayisi;
			}
			return num;
		}
	}

	public int toplam_gun_sayisi
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_gun_sayisi;
			}
			return num;
		}
	}

	public DateTime baslangic_tarihi
	{
		get
		{
			DateTime dateTime = DateTime.Now.AddYears(10);
			foreach (Bolge item in bolgeler)
			{
				if (dateTime > item.baslangic_tarihi)
				{
					dateTime = item.baslangic_tarihi;
				}
			}
			return dateTime;
		}
	}

	public DateTime bitis_tarihi
	{
		get
		{
			DateTime dateTime = DateTime.Now.AddYears(-10);
			foreach (Bolge item in bolgeler)
			{
				if (dateTime < item.bitis_tarihi)
				{
					dateTime = item.bitis_tarihi;
				}
			}
			return dateTime;
		}
	}

	public TimeSpan toplam_mesai_suresi
	{
		get
		{
			TimeSpan result = default(TimeSpan);
			foreach (Bolge item in bolgeler)
			{
				result = result.Add(item.toplam_mesai_suresi);
			}
			return result;
		}
	}

	public TimeSpan toplam_ziyaret_suresi
	{
		get
		{
			TimeSpan result = default(TimeSpan);
			foreach (Bolge item in bolgeler)
			{
				result = result.Add(item.toplam_ziyaret_suresi);
			}
			return result;
		}
	}

	public TimeSpan toplam_ulasim_suresi
	{
		get
		{
			TimeSpan result = default(TimeSpan);
			foreach (Bolge item in bolgeler)
			{
				result = result.Add(item.toplam_ulasim_suresi);
			}
			return result;
		}
	}

	public int toplam_yapilan_km
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_yapilan_km;
			}
			return num;
		}
	}

	public int toplam_hedef_ziyaret
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_hedef_ziyaret;
			}
			return num;
		}
	}

	public int toplam_yapilan_ziyaret
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_yapilan_ziyaret;
			}
			return num;
		}
	}

	public int toplam_yapilmayan_ziyaret
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_yapilmayan_ziyaret;
			}
			return num;
		}
	}

	public int toplam_rota_disi_ziyaret
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_rota_disi_ziyaret;
			}
			return num;
		}
	}

	public int toplam_siparis_adedi
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_siparis_adedi;
			}
			return num;
		}
	}

	public double toplam_siparis_tutari
	{
		get
		{
			double num = 0.0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_siparis_tutari;
			}
			return num;
		}
	}

	public int toplam_fatura_adedi
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_fatura_adedi;
			}
			return num;
		}
	}

	public double toplam_fatura_tutari
	{
		get
		{
			double num = 0.0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_fatura_tutari;
			}
			return num;
		}
	}

	public int toplam_tahsilat_evrak_adedi
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_tahsilat_evrak_adedi;
			}
			return num;
		}
	}

	public int toplam_masraf_adedi
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_masraf_adedi;
			}
			return num;
		}
	}

	public double toplam_nakit_tahsilat_tutari
	{
		get
		{
			double num = 0.0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_nakit_tahsilat_tutari;
			}
			return num;
		}
	}

	public double toplam_masraf_tutari
	{
		get
		{
			double num = 0.0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_masraf_tutari;
			}
			return num;
		}
	}

	public double toplam_kalan_nakit_tutar
	{
		get
		{
			double num = 0.0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_kalan_nakit_tutar;
			}
			return num;
		}
	}

	public int toplam_cek_adedi
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_cek_adedi;
			}
			return num;
		}
	}

	public double toplam_cek_tutari
	{
		get
		{
			double num = 0.0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_cek_tutari;
			}
			return num;
		}
	}

	public int toplam_kk_adedi
	{
		get
		{
			int num = 0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_kk_adedi;
			}
			return num;
		}
	}

	public double toplam_kk_tutari
	{
		get
		{
			double num = 0.0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_kk_tutari;
			}
			return num;
		}
	}

	public double toplam_tahsilat_tutari
	{
		get
		{
			double num = 0.0;
			foreach (Bolge item in bolgeler)
			{
				num += item.toplam_tahsilat_tutari;
			}
			return num;
		}
	}

	public TimeSpan ziyaret_basi_ortalama_ziyaret_suresi => TimeSpan.FromSeconds((int)(toplam_ziyaret_suresi.TotalSeconds / (double)toplam_yapilan_ziyaret));

	public TimeSpan ziyaret_basi_ortalama_ulasim_suresi => TimeSpan.FromSeconds((int)(toplam_ulasim_suresi.TotalSeconds / (double)toplam_yapilan_ziyaret));

	public double ziyaret_basi_ortalama_yapilan_km => (double)toplam_yapilan_km / (double)toplam_yapilan_ziyaret;

	public double ziyaret_basi_ortalama_siparis_tutari => toplam_siparis_tutari / (double)toplam_yapilan_ziyaret;

	public double ziyaret_basi_ortalama_fatura_tutari => toplam_fatura_tutari / (double)toplam_yapilan_ziyaret;

	public double ziyaret_basi_ortalama_nakit_tahsilat_tutari => toplam_nakit_tahsilat_tutari / (double)toplam_yapilan_ziyaret;

	public double ziyaret_basi_ortalama_cek_tutari => toplam_cek_tutari / (double)toplam_yapilan_ziyaret;

	public double ziyaret_basi_ortalama_kk_tutari => toplam_kk_tutari / (double)toplam_yapilan_ziyaret;

	public double ziyaret_basi_ortalama_tahsilat_tutari => toplam_tahsilat_tutari / (double)toplam_yapilan_ziyaret;

	public TimeSpan temsilci_basi_mesai_suresi => TimeSpan.FromSeconds((int)(toplam_mesai_suresi.TotalSeconds / (double)temsilci_sayisi));

	public TimeSpan temsilci_basi_ulasim_suresi => TimeSpan.FromSeconds((int)(toplam_ulasim_suresi.TotalSeconds / (double)temsilci_sayisi));

	public double temsilci_basi_yapilan_km => (double)toplam_yapilan_km / (double)temsilci_sayisi;

	public double temsilci_basi_hedef_ziyaret => (double)toplam_hedef_ziyaret / (double)temsilci_sayisi;

	public double temsilci_basi_yapilan_ziyaret => (double)toplam_yapilan_ziyaret / (double)temsilci_sayisi;

	public double temsilci_basi_yapilmayan_ziyaret => (double)toplam_yapilmayan_ziyaret / (double)temsilci_sayisi;

	public double temsilci_basi_rota_disi_ziyaret => (double)toplam_rota_disi_ziyaret / (double)temsilci_sayisi;

	public double temsilci_basi_siparis_adedi => (double)toplam_siparis_adedi / (double)temsilci_sayisi;

	public double temsilci_basi_siparis_tutari => toplam_siparis_tutari / (double)temsilci_sayisi;

	public double temsilci_basi_fatura_adedi => (double)toplam_fatura_adedi / (double)temsilci_sayisi;

	public double temsilci_basi_fatura_tutari => toplam_fatura_tutari / (double)temsilci_sayisi;

	public double temsilci_basi_tahsilat_evrak_adedi => (double)toplam_tahsilat_evrak_adedi / (double)temsilci_sayisi;

	public double temsilci_basi_masraf_adedi => (double)toplam_masraf_adedi / (double)temsilci_sayisi;

	public double temsilci_basi_nakit_tahsilat_tutari => toplam_nakit_tahsilat_tutari / (double)temsilci_sayisi;

	public double temsilci_basi_masraf_tutari => toplam_masraf_tutari / (double)temsilci_sayisi;

	public double temsilci_basi_kalan_nakit_tutar => toplam_kalan_nakit_tutar / (double)temsilci_sayisi;

	public double temsilci_basi_cek_adedi => (double)toplam_cek_adedi / (double)temsilci_sayisi;

	public double temsilci_basi_cek_tutari => toplam_cek_tutari / (double)temsilci_sayisi;

	public double temsilci_basi_kk_adedi => (double)toplam_kk_adedi / (double)temsilci_sayisi;

	public double temsilci_basi_kk_tutari => toplam_kk_tutari / (double)temsilci_sayisi;

	public double temsilci_basi_tahsilat_tutari => toplam_tahsilat_tutari / (double)temsilci_sayisi;

	public TimeSpan bolge_basi_mesai_suresi => TimeSpan.FromSeconds((int)(toplam_mesai_suresi.TotalSeconds / (double)bolge_sayisi));

	public TimeSpan bolge_basi_ulasim_suresi => TimeSpan.FromSeconds((int)(toplam_ulasim_suresi.TotalSeconds / (double)bolge_sayisi));

	public double bolge_basi_yapilan_km => (double)toplam_yapilan_km / (double)bolge_sayisi;

	public double bolge_basi_hedef_ziyaret => (double)toplam_hedef_ziyaret / (double)bolge_sayisi;

	public double bolge_basi_yapilan_ziyaret => (double)toplam_yapilan_ziyaret / (double)bolge_sayisi;

	public double bolge_basi_yapilmayan_ziyaret => (double)toplam_yapilmayan_ziyaret / (double)bolge_sayisi;

	public double bolge_basi_rota_disi_ziyaret => (double)toplam_rota_disi_ziyaret / (double)bolge_sayisi;

	public double bolge_basi_siparis_adedi => (double)toplam_siparis_adedi / (double)bolge_sayisi;

	public double bolge_basi_siparis_tutari => toplam_siparis_tutari / (double)bolge_sayisi;

	public double bolge_basi_fatura_adedi => (double)toplam_fatura_adedi / (double)bolge_sayisi;

	public double bolge_basi_fatura_tutari => toplam_fatura_tutari / (double)bolge_sayisi;

	public double bolge_basi_tahsilat_evrak_adedi => (double)toplam_tahsilat_evrak_adedi / (double)bolge_sayisi;

	public double bolge_basi_masraf_adedi => (double)toplam_masraf_adedi / (double)bolge_sayisi;

	public double bolge_basi_nakit_tahsilat_tutari => toplam_nakit_tahsilat_tutari / (double)bolge_sayisi;

	public double bolge_basi_masraf_tutari => toplam_masraf_tutari / (double)bolge_sayisi;

	public double bolge_basi_kalan_nakit_tutar => toplam_kalan_nakit_tutar / (double)bolge_sayisi;

	public double bolge_basi_cek_adedi => (double)toplam_cek_adedi / (double)bolge_sayisi;

	public double bolge_basi_cek_tutari => toplam_cek_tutari / (double)bolge_sayisi;

	public double bolge_basi_kk_adedi => (double)toplam_kk_adedi / (double)bolge_sayisi;

	public double bolge_basi_kk_tutari => toplam_kk_tutari / (double)bolge_sayisi;

	public double bolge_basi_tahsilat_tutari => toplam_tahsilat_tutari / (double)bolge_sayisi;

	public TimeSpan gunluk_ortalama_ziyaret_suresi => TimeSpan.FromSeconds((int)(toplam_ziyaret_suresi.TotalSeconds / (double)toplam_gun_sayisi));

	public TimeSpan gunluk_ortalama_ulasim_suresi => TimeSpan.FromSeconds((int)(toplam_ulasim_suresi.TotalSeconds / (double)toplam_gun_sayisi));

	public double gunluk_ortalama_gunluk_yapilan_km => (double)toplam_yapilan_km / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_gunluk_hedef_ziyaret => (double)toplam_hedef_ziyaret / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_yapilan_ziyaret => (double)toplam_yapilan_ziyaret / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_yapilmayan_ziyaret => (double)toplam_yapilmayan_ziyaret / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_rota_disi_ziyaret => (double)toplam_rota_disi_ziyaret / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_siparis_adedi => (double)toplam_siparis_adedi / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_siparis_tutari => toplam_siparis_tutari / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_fatura_adedi => (double)toplam_fatura_adedi / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_fatura_tutari => toplam_fatura_tutari / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_tahsilat_evrak_adedi => (double)toplam_tahsilat_evrak_adedi / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_masraf_adedi => (double)toplam_masraf_adedi / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_nakit_tahsilat_tutari => toplam_nakit_tahsilat_tutari / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_masraf_tutari => toplam_masraf_tutari / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_kalan_nakit_tutar => toplam_kalan_nakit_tutar / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_cek_adedi => (double)toplam_cek_adedi / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_cek_tutari => toplam_cek_tutari / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_kk_adedi => (double)toplam_kk_adedi / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_kk_tutari => toplam_kk_tutari / (double)toplam_gun_sayisi;

	public double gunluk_ortalama_tahsilat_tutari => toplam_tahsilat_tutari / (double)toplam_gun_sayisi;

	public Bolgeler()
	{
		bolgeler = new List<Bolge>();
	}
}
