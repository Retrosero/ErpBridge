using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.Aktarimlar.BankaAktarimi;

public class Aktarim_Banka_Aktarim_Parametre_Duzenleme : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _kullaniciparametreleri = new Parametreler();

	private bool DegisiklikVar;

	private string AktifKullanici;

	private IContainer components;

	private ListBoxControl lb_kullanicilar;

	private SimpleButton sb_kullanici_ekle;

	private SimpleButton sb_kullanici_sil;

	private SimpleButton sb_ayarlari_kaydet;

	private XtraTabControl tc_parametreler;

	private XtraTabPage xtraTabPage3;

	private TextEdit te_kullanici_adi;

	private LabelControl labelControl1;

	private TextEdit te_eklenecek_parametre_adi;

	private LabelControl labelControl17;

	private CheckEdit ce_ayrackarakterikullan;

	private TextEdit te_ayrackarakteri;

	private Label label18;

	private CheckEdit ce_bilgilerinbaslangicsatirikullan;

	private TextEdit te_bilgilerinbaslangicsatiri;

	private Label label17;

	private CheckEdit ce_alinacaksatirlarinbaslangickarakterikullan;

	private TextEdit te_alinacaksatirlarinbaslangickarakteri;

	private Label label16;

	private TextEdit te_tarihgun_uzunluk;

	private TextEdit te_tarihgun;

	private Label label34;

	private TextEdit te_tarihay_uzunluk;

	private TextEdit te_tarihay;

	private Label label33;

	private TextEdit te_tutar_uzunluk;

	private TextEdit te_aciklama_uzunluk;

	private TextEdit te_tarihyil_uzunluk;

	private TextEdit te_tutar;

	private Label label8;

	private TextEdit te_aciklama;

	private Label label30;

	private TextEdit te_tarihyil;

	private Label label29;

	private Label label39;

	private Label label40;

	private TextEdit te_mahalle_uzunluk;

	private TextEdit te_mahalle;

	private Label label14;

	private TextEdit te_unvan2_uzunluk;

	private TextEdit te_unvan2;

	private Label label15;

	private Label label32;

	private TextEdit te_bankahesapno_uzunluk;

	private TextEdit te_tckimlikvergino_uzunluk;

	private TextEdit te_eposta_uzunluk;

	private TextEdit te_telefon_uzunluk;

	private TextEdit te_postakodu_uzunluk;

	private TextEdit te_ulke_uzunluk;

	private TextEdit te_il_uzunluk;

	private TextEdit te_ilce_uzunluk;

	private TextEdit te_adres_uzunluk;

	private TextEdit te_unvan_uzunluk;

	private Label label20;

	private TextEdit te_bankahesapno;

	private Label label28;

	private TextEdit te_tckimlikvergino;

	private Label label27;

	private TextEdit te_eposta;

	private Label label26;

	private TextEdit te_telefon;

	private Label label25;

	private TextEdit te_postakodu;

	private Label label24;

	private TextEdit te_ulke;

	private Label label23;

	private TextEdit te_il;

	private Label label35;

	private TextEdit te_ilce;

	private Label label36;

	private TextEdit te_adres;

	private Label label37;

	private TextEdit te_unvan;

	private Label label38;

	private Label label44;

	private Label label43;

	private TextEdit te_bakiye_uzunluk;

	private TextEdit te_bakiye;

	private Label label42;

	private Label label41;

	private XtraTabPage xtraTabPage1;

	private XtraTabControl xtraTabControl1;

	private XtraTabPage xtraTabPage4;

	private XtraTabPage xtraTabPage5;

	private CheckEdit mikro_disi_ek_bilgileri_kullan;

	private Label label5;

	private XtraTabPage xtraTabPage6;

	private TextEdit metin17_varsayilan_deger;

	private CheckEdit metin17_zorunlu;

	private SpinEdit metin17_uzunluk;

	private SpinEdit metin17_baslangic;

	private TextEdit metin17_gorunen_adi;

	private Label label50;

	private TextEdit metin16_varsayilan_deger;

	private CheckEdit metin16_zorunlu;

	private SpinEdit metin16_uzunluk;

	private SpinEdit metin16_baslangic;

	private TextEdit metin16_gorunen_adi;

	private Label label49;

	private TextEdit metin15_varsayilan_deger;

	private CheckEdit metin15_zorunlu;

	private SpinEdit metin15_uzunluk;

	private SpinEdit metin15_baslangic;

	private TextEdit metin15_gorunen_adi;

	private Label label48;

	private TextEdit metin14_varsayilan_deger;

	private CheckEdit metin14_zorunlu;

	private SpinEdit metin14_uzunluk;

	private SpinEdit metin14_baslangic;

	private TextEdit metin14_gorunen_adi;

	private Label label47;

	private TextEdit metin13_varsayilan_deger;

	private CheckEdit metin13_zorunlu;

	private SpinEdit metin13_uzunluk;

	private SpinEdit metin13_baslangic;

	private TextEdit metin13_gorunen_adi;

	private Label label46;

	private TextEdit metin12_varsayilan_deger;

	private CheckEdit metin12_zorunlu;

	private SpinEdit metin12_uzunluk;

	private SpinEdit metin12_baslangic;

	private TextEdit metin12_gorunen_adi;

	private Label label45;

	private TextEdit metin11_varsayilan_deger;

	private CheckEdit metin11_zorunlu;

	private SpinEdit metin11_uzunluk;

	private SpinEdit metin11_baslangic;

	private TextEdit metin11_gorunen_adi;

	private Label label31;

	private TextEdit metin10_varsayilan_deger;

	private CheckEdit metin10_zorunlu;

	private SpinEdit metin10_uzunluk;

	private SpinEdit metin10_baslangic;

	private TextEdit metin10_gorunen_adi;

	private Label label22;

	private TextEdit metin9_varsayilan_deger;

	private CheckEdit metin9_zorunlu;

	private SpinEdit metin9_uzunluk;

	private SpinEdit metin9_baslangic;

	private TextEdit metin9_gorunen_adi;

	private Label label21;

	private TextEdit metin8_varsayilan_deger;

	private CheckEdit metin8_zorunlu;

	private SpinEdit metin8_uzunluk;

	private SpinEdit metin8_baslangic;

	private TextEdit metin8_gorunen_adi;

	private Label label19;

	private TextEdit metin7_varsayilan_deger;

	private CheckEdit metin7_zorunlu;

	private SpinEdit metin7_uzunluk;

	private SpinEdit metin7_baslangic;

	private TextEdit metin7_gorunen_adi;

	private Label label13;

	private TextEdit metin6_varsayilan_deger;

	private CheckEdit metin6_zorunlu;

	private SpinEdit metin6_uzunluk;

	private SpinEdit metin6_baslangic;

	private TextEdit metin6_gorunen_adi;

	private Label label12;

	private TextEdit metin5_varsayilan_deger;

	private CheckEdit metin5_zorunlu;

	private SpinEdit metin5_uzunluk;

	private SpinEdit metin5_baslangic;

	private TextEdit metin5_gorunen_adi;

	private Label label11;

	private TextEdit metin4_varsayilan_deger;

	private CheckEdit metin4_zorunlu;

	private SpinEdit metin4_uzunluk;

	private SpinEdit metin4_baslangic;

	private TextEdit metin4_gorunen_adi;

	private Label label10;

	private TextEdit metin3_varsayilan_deger;

	private CheckEdit metin3_zorunlu;

	private SpinEdit metin3_uzunluk;

	private SpinEdit metin3_baslangic;

	private TextEdit metin3_gorunen_adi;

	private Label label9;

	private TextEdit metin2_varsayilan_deger;

	private CheckEdit metin2_zorunlu;

	private SpinEdit metin2_uzunluk;

	private SpinEdit metin2_baslangic;

	private TextEdit metin2_gorunen_adi;

	private Label label7;

	private TextEdit metin1_varsayilan_deger;

	private Label label6;

	private CheckEdit metin1_zorunlu;

	private SpinEdit metin1_uzunluk;

	private SpinEdit metin1_baslangic;

	private TextEdit metin1_gorunen_adi;

	private Label label4;

	private Label label1;

	private Label label2;

	private Label label3;

	private TextEdit metin20_varsayilan_deger;

	private CheckEdit metin20_zorunlu;

	private SpinEdit metin20_uzunluk;

	private SpinEdit metin20_baslangic;

	private TextEdit metin20_gorunen_adi;

	private Label label53;

	private TextEdit metin19_varsayilan_deger;

	private CheckEdit metin19_zorunlu;

	private SpinEdit metin19_uzunluk;

	private SpinEdit metin19_baslangic;

	private TextEdit metin19_gorunen_adi;

	private Label label52;

	private TextEdit metin18_varsayilan_deger;

	private CheckEdit metin18_zorunlu;

	private SpinEdit metin18_uzunluk;

	private SpinEdit metin18_baslangic;

	private TextEdit metin18_gorunen_adi;

	private Label label51;

	private TextEdit metin50_varsayilan_deger;

	private CheckEdit metin50_zorunlu;

	private SpinEdit metin50_uzunluk;

	private SpinEdit metin50_baslangic;

	private TextEdit metin50_gorunen_adi;

	private Label label83;

	private TextEdit metin49_varsayilan_deger;

	private CheckEdit metin49_zorunlu;

	private SpinEdit metin49_uzunluk;

	private SpinEdit metin49_baslangic;

	private TextEdit metin49_gorunen_adi;

	private Label label82;

	private TextEdit metin48_varsayilan_deger;

	private CheckEdit metin48_zorunlu;

	private SpinEdit metin48_uzunluk;

	private SpinEdit metin48_baslangic;

	private TextEdit metin48_gorunen_adi;

	private Label label81;

	private TextEdit metin47_varsayilan_deger;

	private CheckEdit metin47_zorunlu;

	private SpinEdit metin47_uzunluk;

	private SpinEdit metin47_baslangic;

	private TextEdit metin47_gorunen_adi;

	private Label label80;

	private TextEdit metin46_varsayilan_deger;

	private CheckEdit metin46_zorunlu;

	private SpinEdit metin46_uzunluk;

	private SpinEdit metin46_baslangic;

	private TextEdit metin46_gorunen_adi;

	private Label label79;

	private TextEdit metin45_varsayilan_deger;

	private CheckEdit metin45_zorunlu;

	private SpinEdit metin45_uzunluk;

	private SpinEdit metin45_baslangic;

	private TextEdit metin45_gorunen_adi;

	private Label label78;

	private TextEdit metin44_varsayilan_deger;

	private CheckEdit metin44_zorunlu;

	private SpinEdit metin44_uzunluk;

	private SpinEdit metin44_baslangic;

	private TextEdit metin44_gorunen_adi;

	private Label label77;

	private TextEdit metin43_varsayilan_deger;

	private CheckEdit metin43_zorunlu;

	private SpinEdit metin43_uzunluk;

	private SpinEdit metin43_baslangic;

	private TextEdit metin43_gorunen_adi;

	private Label label76;

	private TextEdit metin42_varsayilan_deger;

	private CheckEdit metin42_zorunlu;

	private SpinEdit metin42_uzunluk;

	private SpinEdit metin42_baslangic;

	private TextEdit metin42_gorunen_adi;

	private Label label75;

	private TextEdit metin41_varsayilan_deger;

	private CheckEdit metin41_zorunlu;

	private SpinEdit metin41_uzunluk;

	private SpinEdit metin41_baslangic;

	private TextEdit metin41_gorunen_adi;

	private Label label74;

	private TextEdit metin40_varsayilan_deger;

	private CheckEdit metin40_zorunlu;

	private SpinEdit metin40_uzunluk;

	private SpinEdit metin40_baslangic;

	private TextEdit metin40_gorunen_adi;

	private Label label73;

	private TextEdit metin39_varsayilan_deger;

	private CheckEdit metin39_zorunlu;

	private SpinEdit metin39_uzunluk;

	private SpinEdit metin39_baslangic;

	private TextEdit metin39_gorunen_adi;

	private Label label72;

	private TextEdit metin38_varsayilan_deger;

	private CheckEdit metin38_zorunlu;

	private SpinEdit metin38_uzunluk;

	private SpinEdit metin38_baslangic;

	private TextEdit metin38_gorunen_adi;

	private Label label71;

	private TextEdit metin37_varsayilan_deger;

	private CheckEdit metin37_zorunlu;

	private SpinEdit metin37_uzunluk;

	private SpinEdit metin37_baslangic;

	private TextEdit metin37_gorunen_adi;

	private Label label70;

	private TextEdit metin36_varsayilan_deger;

	private CheckEdit metin36_zorunlu;

	private SpinEdit metin36_uzunluk;

	private SpinEdit metin36_baslangic;

	private TextEdit metin36_gorunen_adi;

	private Label label69;

	private TextEdit metin35_varsayilan_deger;

	private CheckEdit metin35_zorunlu;

	private SpinEdit metin35_uzunluk;

	private SpinEdit metin35_baslangic;

	private TextEdit metin35_gorunen_adi;

	private Label label68;

	private TextEdit metin34_varsayilan_deger;

	private CheckEdit metin34_zorunlu;

	private SpinEdit metin34_uzunluk;

	private SpinEdit metin34_baslangic;

	private TextEdit metin34_gorunen_adi;

	private Label label67;

	private TextEdit metin33_varsayilan_deger;

	private CheckEdit metin33_zorunlu;

	private SpinEdit metin33_uzunluk;

	private SpinEdit metin33_baslangic;

	private TextEdit metin33_gorunen_adi;

	private Label label66;

	private TextEdit metin32_varsayilan_deger;

	private CheckEdit metin32_zorunlu;

	private SpinEdit metin32_uzunluk;

	private SpinEdit metin32_baslangic;

	private TextEdit metin32_gorunen_adi;

	private Label label65;

	private TextEdit metin31_varsayilan_deger;

	private CheckEdit metin31_zorunlu;

	private SpinEdit metin31_uzunluk;

	private SpinEdit metin31_baslangic;

	private TextEdit metin31_gorunen_adi;

	private Label label64;

	private TextEdit metin30_varsayilan_deger;

	private CheckEdit metin30_zorunlu;

	private SpinEdit metin30_uzunluk;

	private SpinEdit metin30_baslangic;

	private TextEdit metin30_gorunen_adi;

	private Label label63;

	private TextEdit metin29_varsayilan_deger;

	private CheckEdit metin29_zorunlu;

	private SpinEdit metin29_uzunluk;

	private SpinEdit metin29_baslangic;

	private TextEdit metin29_gorunen_adi;

	private Label label62;

	private TextEdit metin28_varsayilan_deger;

	private CheckEdit metin28_zorunlu;

	private SpinEdit metin28_uzunluk;

	private SpinEdit metin28_baslangic;

	private TextEdit metin28_gorunen_adi;

	private Label label61;

	private TextEdit metin27_varsayilan_deger;

	private CheckEdit metin27_zorunlu;

	private SpinEdit metin27_uzunluk;

	private SpinEdit metin27_baslangic;

	private TextEdit metin27_gorunen_adi;

	private Label label60;

	private TextEdit metin26_varsayilan_deger;

	private CheckEdit metin26_zorunlu;

	private SpinEdit metin26_uzunluk;

	private SpinEdit metin26_baslangic;

	private TextEdit metin26_gorunen_adi;

	private Label label59;

	private TextEdit metin25_varsayilan_deger;

	private CheckEdit metin25_zorunlu;

	private SpinEdit metin25_uzunluk;

	private SpinEdit metin25_baslangic;

	private TextEdit metin25_gorunen_adi;

	private Label label58;

	private TextEdit metin24_varsayilan_deger;

	private CheckEdit metin24_zorunlu;

	private SpinEdit metin24_uzunluk;

	private SpinEdit metin24_baslangic;

	private TextEdit metin24_gorunen_adi;

	private Label label57;

	private TextEdit metin23_varsayilan_deger;

	private CheckEdit metin23_zorunlu;

	private SpinEdit metin23_uzunluk;

	private SpinEdit metin23_baslangic;

	private TextEdit metin23_gorunen_adi;

	private Label label56;

	private TextEdit metin22_varsayilan_deger;

	private CheckEdit metin22_zorunlu;

	private SpinEdit metin22_uzunluk;

	private SpinEdit metin22_baslangic;

	private TextEdit metin22_gorunen_adi;

	private Label label55;

	private TextEdit metin21_varsayilan_deger;

	private CheckEdit metin21_zorunlu;

	private SpinEdit metin21_uzunluk;

	private SpinEdit metin21_baslangic;

	private TextEdit metin21_gorunen_adi;

	private Label label54;

	private Label label165;

	private Label label166;

	private TextEdit dropbox10_varsayilan_deger;

	private Label label167;

	private TextEdit dropbox10_secenekler_veri;

	private Label label168;

	private TextEdit dropbox10_secenekler_yazi;

	private Label label169;

	private CheckEdit dropbox10_zorunlu;

	private SpinEdit dropbox10_uzunluk;

	private SpinEdit dropbox10_baslangic;

	private TextEdit dropbox10_gorunen_adi;

	private Label label170;

	private Label label171;

	private Label label172;

	private Label label173;

	private Label label156;

	private Label label157;

	private TextEdit dropbox9_varsayilan_deger;

	private Label label158;

	private TextEdit dropbox9_secenekler_veri;

	private Label label159;

	private TextEdit dropbox9_secenekler_yazi;

	private Label label160;

	private CheckEdit dropbox9_zorunlu;

	private SpinEdit dropbox9_uzunluk;

	private SpinEdit dropbox9_baslangic;

	private TextEdit dropbox9_gorunen_adi;

	private Label label161;

	private Label label162;

	private Label label163;

	private Label label164;

	private Label label147;

	private Label label148;

	private TextEdit dropbox8_varsayilan_deger;

	private Label label149;

	private TextEdit dropbox8_secenekler_veri;

	private Label label150;

	private TextEdit dropbox8_secenekler_yazi;

	private Label label151;

	private CheckEdit dropbox8_zorunlu;

	private SpinEdit dropbox8_uzunluk;

	private SpinEdit dropbox8_baslangic;

	private TextEdit dropbox8_gorunen_adi;

	private Label label152;

	private Label label153;

	private Label label154;

	private Label label155;

	private Label label138;

	private Label label139;

	private TextEdit dropbox7_varsayilan_deger;

	private Label label140;

	private TextEdit dropbox7_secenekler_veri;

	private Label label141;

	private TextEdit dropbox7_secenekler_yazi;

	private Label label142;

	private CheckEdit dropbox7_zorunlu;

	private SpinEdit dropbox7_uzunluk;

	private SpinEdit dropbox7_baslangic;

	private TextEdit dropbox7_gorunen_adi;

	private Label label143;

	private Label label144;

	private Label label145;

	private Label label146;

	private Label label129;

	private Label label130;

	private TextEdit dropbox6_varsayilan_deger;

	private Label label131;

	private TextEdit dropbox6_secenekler_veri;

	private Label label132;

	private TextEdit dropbox6_secenekler_yazi;

	private Label label133;

	private CheckEdit dropbox6_zorunlu;

	private SpinEdit dropbox6_uzunluk;

	private SpinEdit dropbox6_baslangic;

	private TextEdit dropbox6_gorunen_adi;

	private Label label134;

	private Label label135;

	private Label label136;

	private Label label137;

	private Label label120;

	private Label label121;

	private TextEdit dropbox5_varsayilan_deger;

	private Label label122;

	private TextEdit dropbox5_secenekler_veri;

	private Label label123;

	private TextEdit dropbox5_secenekler_yazi;

	private Label label124;

	private CheckEdit dropbox5_zorunlu;

	private SpinEdit dropbox5_uzunluk;

	private SpinEdit dropbox5_baslangic;

	private TextEdit dropbox5_gorunen_adi;

	private Label label125;

	private Label label126;

	private Label label127;

	private Label label128;

	private Label label111;

	private Label label112;

	private TextEdit dropbox4_varsayilan_deger;

	private Label label113;

	private TextEdit dropbox4_secenekler_veri;

	private Label label114;

	private TextEdit dropbox4_secenekler_yazi;

	private Label label115;

	private CheckEdit dropbox4_zorunlu;

	private SpinEdit dropbox4_uzunluk;

	private SpinEdit dropbox4_baslangic;

	private TextEdit dropbox4_gorunen_adi;

	private Label label116;

	private Label label117;

	private Label label118;

	private Label label119;

	private Label label102;

	private Label label103;

	private TextEdit dropbox3_varsayilan_deger;

	private Label label104;

	private TextEdit dropbox3_secenekler_veri;

	private Label label105;

	private TextEdit dropbox3_secenekler_yazi;

	private Label label106;

	private CheckEdit dropbox3_zorunlu;

	private SpinEdit dropbox3_uzunluk;

	private SpinEdit dropbox3_baslangic;

	private TextEdit dropbox3_gorunen_adi;

	private Label label107;

	private Label label108;

	private Label label109;

	private Label label110;

	private Label label93;

	private Label label94;

	private TextEdit dropbox2_varsayilan_deger;

	private Label label95;

	private TextEdit dropbox2_secenekler_veri;

	private Label label96;

	private TextEdit dropbox2_secenekler_yazi;

	private Label label97;

	private CheckEdit dropbox2_zorunlu;

	private SpinEdit dropbox2_uzunluk;

	private SpinEdit dropbox2_baslangic;

	private TextEdit dropbox2_gorunen_adi;

	private Label label98;

	private Label label99;

	private Label label100;

	private Label label101;

	private Label label92;

	private Label label91;

	private TextEdit dropbox1_varsayilan_deger;

	private Label label90;

	private TextEdit dropbox1_secenekler_veri;

	private Label label89;

	private TextEdit dropbox1_secenekler_yazi;

	private Label label84;

	private CheckEdit dropbox1_zorunlu;

	private SpinEdit dropbox1_uzunluk;

	private SpinEdit dropbox1_baslangic;

	private TextEdit dropbox1_gorunen_adi;

	private Label label85;

	private Label label86;

	private Label label87;

	private Label label88;

	private XtraTabPage xtraTabPage2;

	private TextEdit checkbox10_isaretsiz_icin_deger;

	private Label label228;

	private TextEdit checkbox10_isaretli_icin_deger;

	private Label label229;

	private CheckEdit checkbox10_varsayilan_deger;

	private SpinEdit checkbox10_uzunluk;

	private SpinEdit checkbox10_baslangic;

	private TextEdit checkbox10_gorunen_adi;

	private Label label230;

	private Label label231;

	private Label label232;

	private Label label233;

	private TextEdit checkbox9_isaretsiz_icin_deger;

	private Label label222;

	private TextEdit checkbox9_isaretli_icin_deger;

	private Label label223;

	private CheckEdit checkbox9_varsayilan_deger;

	private SpinEdit checkbox9_uzunluk;

	private SpinEdit checkbox9_baslangic;

	private TextEdit checkbox9_gorunen_adi;

	private Label label224;

	private Label label225;

	private Label label226;

	private Label label227;

	private TextEdit checkbox8_isaretsiz_icin_deger;

	private Label label216;

	private TextEdit checkbox8_isaretli_icin_deger;

	private Label label217;

	private CheckEdit checkbox8_varsayilan_deger;

	private SpinEdit checkbox8_uzunluk;

	private SpinEdit checkbox8_baslangic;

	private TextEdit checkbox8_gorunen_adi;

	private Label label218;

	private Label label219;

	private Label label220;

	private Label label221;

	private TextEdit checkbox7_isaretsiz_icin_deger;

	private Label label210;

	private TextEdit checkbox7_isaretli_icin_deger;

	private Label label211;

	private CheckEdit checkbox7_varsayilan_deger;

	private SpinEdit checkbox7_uzunluk;

	private SpinEdit checkbox7_baslangic;

	private TextEdit checkbox7_gorunen_adi;

	private Label label212;

	private Label label213;

	private Label label214;

	private Label label215;

	private TextEdit checkbox6_isaretsiz_icin_deger;

	private Label label204;

	private TextEdit checkbox6_isaretli_icin_deger;

	private Label label205;

	private CheckEdit checkbox6_varsayilan_deger;

	private SpinEdit checkbox6_uzunluk;

	private SpinEdit checkbox6_baslangic;

	private TextEdit checkbox6_gorunen_adi;

	private Label label206;

	private Label label207;

	private Label label208;

	private Label label209;

	private TextEdit checkbox5_isaretsiz_icin_deger;

	private Label label198;

	private TextEdit checkbox5_isaretli_icin_deger;

	private Label label199;

	private CheckEdit checkbox5_varsayilan_deger;

	private SpinEdit checkbox5_uzunluk;

	private SpinEdit checkbox5_baslangic;

	private TextEdit checkbox5_gorunen_adi;

	private Label label200;

	private Label label201;

	private Label label202;

	private Label label203;

	private TextEdit checkbox4_isaretsiz_icin_deger;

	private Label label192;

	private TextEdit checkbox4_isaretli_icin_deger;

	private Label label193;

	private CheckEdit checkbox4_varsayilan_deger;

	private SpinEdit checkbox4_uzunluk;

	private SpinEdit checkbox4_baslangic;

	private TextEdit checkbox4_gorunen_adi;

	private Label label194;

	private Label label195;

	private Label label196;

	private Label label197;

	private TextEdit checkbox3_isaretsiz_icin_deger;

	private Label label186;

	private TextEdit checkbox3_isaretli_icin_deger;

	private Label label187;

	private CheckEdit checkbox3_varsayilan_deger;

	private SpinEdit checkbox3_uzunluk;

	private SpinEdit checkbox3_baslangic;

	private TextEdit checkbox3_gorunen_adi;

	private Label label188;

	private Label label189;

	private Label label190;

	private Label label191;

	private TextEdit checkbox2_isaretsiz_icin_deger;

	private Label label180;

	private TextEdit checkbox2_isaretli_icin_deger;

	private Label label181;

	private CheckEdit checkbox2_varsayilan_deger;

	private SpinEdit checkbox2_uzunluk;

	private SpinEdit checkbox2_baslangic;

	private TextEdit checkbox2_gorunen_adi;

	private Label label182;

	private Label label183;

	private Label label184;

	private Label label185;

	private TextEdit checkbox1_isaretsiz_icin_deger;

	private Label label179;

	private TextEdit checkbox1_isaretli_icin_deger;

	private Label label178;

	private CheckEdit checkbox1_varsayilan_deger;

	private SpinEdit checkbox1_uzunluk;

	private SpinEdit checkbox1_baslangic;

	private TextEdit checkbox1_gorunen_adi;

	private Label label174;

	private Label label175;

	private Label label176;

	private Label label177;

	public Aktarim_Banka_Aktarim_Parametre_Duzenleme(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		KullanicilariListele();
	}

	private void KullanicilariListele()
	{
		lb_kullanicilar.Items.Clear();
		foreach (string item in AktarimBankaAktarimParametre.GetParametreAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			lb_kullanicilar.Items.Add(item);
		}
		if (lb_kullanicilar.ItemCount == 0)
		{
			sb_kullanici_sil.Enabled = false;
		}
	}

	private void lb_kullanicilar_SelectedValueChanged(object sender, EventArgs e)
	{
		if (lb_kullanicilar.SelectedValue == null)
		{
			return;
		}
		sb_kullanici_sil.Enabled = true;
		bool flag = true;
		if (DegisiklikVar && AktifKullanici != lb_kullanicilar.SelectedValue.ToString())
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				flag = true;
				break;
			case DialogResult.No:
				flag = true;
				break;
			case DialogResult.Cancel:
				flag = false;
				break;
			}
		}
		if (AktifKullanici == lb_kullanicilar.SelectedValue.ToString())
		{
			flag = false;
		}
		if (flag)
		{
			tc_parametreler.Enabled = true;
			sb_ayarlari_kaydet.Enabled = true;
			AktifKullanici = lb_kullanicilar.SelectedValue.ToString();
			_kullaniciparametreleri = ParametrelerDefault.BankaAktarim(AktifKullanici);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri, "BankaAktarim", "", "AktarimSablon", AktifKullanici);
			EkranBilgiGuncelle();
			DegisiklikVar = false;
		}
		else
		{
			lb_kullanicilar.SelectedItem = AktifKullanici;
		}
	}

	private void EkranBilgiGuncelle()
	{
		te_kullanici_adi.Text = AktifKullanici;
		te_alinacaksatirlarinbaslangickarakteri.Text = _kullaniciparametreleri._GetParametre("alinacak_satirlarin_baslangic_karakteri")._GetString;
		ce_alinacaksatirlarinbaslangickarakterikullan.Checked = _kullaniciparametreleri._GetParametre("alinacak_satirlarin_baslangic_karakteri_kullan")._GetBoolean;
		te_bilgilerinbaslangicsatiri.Text = _kullaniciparametreleri._GetParametre("bilgilerin_baslangic_satiri")._GetString;
		ce_bilgilerinbaslangicsatirikullan.Checked = _kullaniciparametreleri._GetParametre("bilgilerin_baslangic_satiri_kullan")._GetBoolean;
		te_ayrackarakteri.Text = _kullaniciparametreleri._GetParametre("ayrac_karakteri")._GetString;
		ce_ayrackarakterikullan.Checked = _kullaniciparametreleri._GetParametre("ayrac_karakteri_kullan")._GetBoolean;
		te_unvan.Text = _kullaniciparametreleri._GetParametre("unvan_baslangic")._GetString;
		te_unvan_uzunluk.Text = _kullaniciparametreleri._GetParametre("unvan_uzunluk")._GetString;
		te_unvan2.Text = _kullaniciparametreleri._GetParametre("unvan2_baslangic")._GetString;
		te_unvan2_uzunluk.Text = _kullaniciparametreleri._GetParametre("unvan2_uzunluk")._GetString;
		te_adres.Text = _kullaniciparametreleri._GetParametre("adres_baslangic")._GetString;
		te_adres_uzunluk.Text = _kullaniciparametreleri._GetParametre("adres_uzunluk")._GetString;
		te_mahalle.Text = _kullaniciparametreleri._GetParametre("mahalle_baslangic")._GetString;
		te_mahalle_uzunluk.Text = _kullaniciparametreleri._GetParametre("mahalle_uzunluk")._GetString;
		te_ilce.Text = _kullaniciparametreleri._GetParametre("ilce_baslangic")._GetString;
		te_ilce_uzunluk.Text = _kullaniciparametreleri._GetParametre("ilce_uzunluk")._GetString;
		te_il.Text = _kullaniciparametreleri._GetParametre("il_baslangic")._GetString;
		te_il_uzunluk.Text = _kullaniciparametreleri._GetParametre("il_uzunluk")._GetString;
		te_ulke.Text = _kullaniciparametreleri._GetParametre("ulke_baslangic")._GetString;
		te_ulke_uzunluk.Text = _kullaniciparametreleri._GetParametre("ulke_uzunluk")._GetString;
		te_postakodu.Text = _kullaniciparametreleri._GetParametre("postakodu_baslangic")._GetString;
		te_postakodu_uzunluk.Text = _kullaniciparametreleri._GetParametre("postakodu_uzunluk")._GetString;
		te_telefon.Text = _kullaniciparametreleri._GetParametre("telefon_baslangic")._GetString;
		te_telefon_uzunluk.Text = _kullaniciparametreleri._GetParametre("telefon_uzunluk")._GetString;
		te_eposta.Text = _kullaniciparametreleri._GetParametre("eposta_baslangic")._GetString;
		te_eposta_uzunluk.Text = _kullaniciparametreleri._GetParametre("eposta_uzunluk")._GetString;
		te_tckimlikvergino.Text = _kullaniciparametreleri._GetParametre("tckimlik_vergino_baslangic")._GetString;
		te_tckimlikvergino_uzunluk.Text = _kullaniciparametreleri._GetParametre("tckimlik_vergino_uzunluk")._GetString;
		te_bankahesapno.Text = _kullaniciparametreleri._GetParametre("banka_hesap_no_baslangic")._GetString;
		te_bankahesapno_uzunluk.Text = _kullaniciparametreleri._GetParametre("banka_hesap_no_uzunluk")._GetString;
		te_tarihyil.Text = _kullaniciparametreleri._GetParametre("tarih_yil_baslangic")._GetString;
		te_tarihyil_uzunluk.Text = _kullaniciparametreleri._GetParametre("tarih_yil_uzunluk")._GetString;
		te_tarihay.Text = _kullaniciparametreleri._GetParametre("tarih_ay_baslangic")._GetString;
		te_tarihay_uzunluk.Text = _kullaniciparametreleri._GetParametre("tarih_ay_uzunluk")._GetString;
		te_tarihgun.Text = _kullaniciparametreleri._GetParametre("tarih_gun_baslangic")._GetString;
		te_tarihgun_uzunluk.Text = _kullaniciparametreleri._GetParametre("tarih_gun_uzunluk")._GetString;
		te_aciklama.Text = _kullaniciparametreleri._GetParametre("aciklama_baslangic")._GetString;
		te_aciklama_uzunluk.Text = _kullaniciparametreleri._GetParametre("aciklama_uzunluk")._GetString;
		te_tutar.Text = _kullaniciparametreleri._GetParametre("tutar_baslangic")._GetString;
		te_tutar_uzunluk.Text = _kullaniciparametreleri._GetParametre("tutar_uzunluk")._GetString;
		te_bakiye.Text = _kullaniciparametreleri._GetParametre("bakiye_baslangic")._GetString;
		te_bakiye_uzunluk.Text = _kullaniciparametreleri._GetParametre("bakiye_uzunluk")._GetString;
		mikro_disi_ek_bilgileri_kullan.Checked = _kullaniciparametreleri._GetParametre("mikro_disi_ek_bilgileri_kullan")._GetBoolean;
		metin1_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin1_gorunen_adi")._GetString;
		metin1_baslangic.Value = _kullaniciparametreleri._GetParametre("metin1_baslangic")._GetInt;
		metin1_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin1_uzunluk")._GetInt;
		metin1_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin1_varsayilan_deger")._GetString;
		metin1_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin1_zorunlu")._GetBoolean;
		metin2_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin2_gorunen_adi")._GetString;
		metin2_baslangic.Value = _kullaniciparametreleri._GetParametre("metin2_baslangic")._GetInt;
		metin2_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin2_uzunluk")._GetInt;
		metin2_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin2_varsayilan_deger")._GetString;
		metin2_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin2_zorunlu")._GetBoolean;
		metin3_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin3_gorunen_adi")._GetString;
		metin3_baslangic.Value = _kullaniciparametreleri._GetParametre("metin3_baslangic")._GetInt;
		metin3_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin3_uzunluk")._GetInt;
		metin3_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin3_varsayilan_deger")._GetString;
		metin3_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin3_zorunlu")._GetBoolean;
		metin4_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin4_gorunen_adi")._GetString;
		metin4_baslangic.Value = _kullaniciparametreleri._GetParametre("metin4_baslangic")._GetInt;
		metin4_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin4_uzunluk")._GetInt;
		metin4_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin4_varsayilan_deger")._GetString;
		metin4_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin4_zorunlu")._GetBoolean;
		metin5_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin5_gorunen_adi")._GetString;
		metin5_baslangic.Value = _kullaniciparametreleri._GetParametre("metin5_baslangic")._GetInt;
		metin5_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin5_uzunluk")._GetInt;
		metin5_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin5_varsayilan_deger")._GetString;
		metin5_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin5_zorunlu")._GetBoolean;
		metin6_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin6_gorunen_adi")._GetString;
		metin6_baslangic.Value = _kullaniciparametreleri._GetParametre("metin6_baslangic")._GetInt;
		metin6_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin6_uzunluk")._GetInt;
		metin6_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin6_varsayilan_deger")._GetString;
		metin6_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin6_zorunlu")._GetBoolean;
		metin7_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin7_gorunen_adi")._GetString;
		metin7_baslangic.Value = _kullaniciparametreleri._GetParametre("metin7_baslangic")._GetInt;
		metin7_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin7_uzunluk")._GetInt;
		metin7_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin7_varsayilan_deger")._GetString;
		metin7_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin7_zorunlu")._GetBoolean;
		metin8_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin8_gorunen_adi")._GetString;
		metin8_baslangic.Value = _kullaniciparametreleri._GetParametre("metin8_baslangic")._GetInt;
		metin8_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin8_uzunluk")._GetInt;
		metin8_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin8_varsayilan_deger")._GetString;
		metin8_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin8_zorunlu")._GetBoolean;
		metin9_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin9_gorunen_adi")._GetString;
		metin9_baslangic.Value = _kullaniciparametreleri._GetParametre("metin9_baslangic")._GetInt;
		metin9_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin9_uzunluk")._GetInt;
		metin9_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin9_varsayilan_deger")._GetString;
		metin9_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin9_zorunlu")._GetBoolean;
		metin10_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin10_gorunen_adi")._GetString;
		metin10_baslangic.Value = _kullaniciparametreleri._GetParametre("metin10_baslangic")._GetInt;
		metin10_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin10_uzunluk")._GetInt;
		metin10_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin10_varsayilan_deger")._GetString;
		metin10_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin10_zorunlu")._GetBoolean;
		metin11_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin11_gorunen_adi")._GetString;
		metin11_baslangic.Value = _kullaniciparametreleri._GetParametre("metin11_baslangic")._GetInt;
		metin11_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin11_uzunluk")._GetInt;
		metin11_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin11_varsayilan_deger")._GetString;
		metin11_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin11_zorunlu")._GetBoolean;
		metin12_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin12_gorunen_adi")._GetString;
		metin12_baslangic.Value = _kullaniciparametreleri._GetParametre("metin12_baslangic")._GetInt;
		metin12_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin12_uzunluk")._GetInt;
		metin12_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin12_varsayilan_deger")._GetString;
		metin12_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin12_zorunlu")._GetBoolean;
		metin13_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin13_gorunen_adi")._GetString;
		metin13_baslangic.Value = _kullaniciparametreleri._GetParametre("metin13_baslangic")._GetInt;
		metin13_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin13_uzunluk")._GetInt;
		metin13_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin13_varsayilan_deger")._GetString;
		metin13_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin13_zorunlu")._GetBoolean;
		metin14_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin14_gorunen_adi")._GetString;
		metin14_baslangic.Value = _kullaniciparametreleri._GetParametre("metin14_baslangic")._GetInt;
		metin14_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin14_uzunluk")._GetInt;
		metin14_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin14_varsayilan_deger")._GetString;
		metin14_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin14_zorunlu")._GetBoolean;
		metin15_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin15_gorunen_adi")._GetString;
		metin15_baslangic.Value = _kullaniciparametreleri._GetParametre("metin15_baslangic")._GetInt;
		metin15_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin15_uzunluk")._GetInt;
		metin15_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin15_varsayilan_deger")._GetString;
		metin15_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin15_zorunlu")._GetBoolean;
		metin16_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin16_gorunen_adi")._GetString;
		metin16_baslangic.Value = _kullaniciparametreleri._GetParametre("metin16_baslangic")._GetInt;
		metin16_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin16_uzunluk")._GetInt;
		metin16_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin16_varsayilan_deger")._GetString;
		metin16_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin16_zorunlu")._GetBoolean;
		metin17_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin17_gorunen_adi")._GetString;
		metin17_baslangic.Value = _kullaniciparametreleri._GetParametre("metin17_baslangic")._GetInt;
		metin17_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin17_uzunluk")._GetInt;
		metin17_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin17_varsayilan_deger")._GetString;
		metin17_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin17_zorunlu")._GetBoolean;
		metin18_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin18_gorunen_adi")._GetString;
		metin18_baslangic.Value = _kullaniciparametreleri._GetParametre("metin18_baslangic")._GetInt;
		metin18_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin18_uzunluk")._GetInt;
		metin18_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin18_varsayilan_deger")._GetString;
		metin18_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin18_zorunlu")._GetBoolean;
		metin19_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin19_gorunen_adi")._GetString;
		metin19_baslangic.Value = _kullaniciparametreleri._GetParametre("metin19_baslangic")._GetInt;
		metin19_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin19_uzunluk")._GetInt;
		metin19_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin19_varsayilan_deger")._GetString;
		metin19_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin19_zorunlu")._GetBoolean;
		metin20_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin20_gorunen_adi")._GetString;
		metin20_baslangic.Value = _kullaniciparametreleri._GetParametre("metin20_baslangic")._GetInt;
		metin20_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin20_uzunluk")._GetInt;
		metin20_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin20_varsayilan_deger")._GetString;
		metin20_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin20_zorunlu")._GetBoolean;
		metin21_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin21_gorunen_adi")._GetString;
		metin21_baslangic.Value = _kullaniciparametreleri._GetParametre("metin21_baslangic")._GetInt;
		metin21_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin21_uzunluk")._GetInt;
		metin21_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin21_varsayilan_deger")._GetString;
		metin21_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin21_zorunlu")._GetBoolean;
		metin22_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin22_gorunen_adi")._GetString;
		metin22_baslangic.Value = _kullaniciparametreleri._GetParametre("metin22_baslangic")._GetInt;
		metin22_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin22_uzunluk")._GetInt;
		metin22_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin22_varsayilan_deger")._GetString;
		metin22_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin22_zorunlu")._GetBoolean;
		metin23_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin23_gorunen_adi")._GetString;
		metin23_baslangic.Value = _kullaniciparametreleri._GetParametre("metin23_baslangic")._GetInt;
		metin23_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin23_uzunluk")._GetInt;
		metin23_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin23_varsayilan_deger")._GetString;
		metin23_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin23_zorunlu")._GetBoolean;
		metin24_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin24_gorunen_adi")._GetString;
		metin24_baslangic.Value = _kullaniciparametreleri._GetParametre("metin24_baslangic")._GetInt;
		metin24_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin24_uzunluk")._GetInt;
		metin24_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin24_varsayilan_deger")._GetString;
		metin24_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin24_zorunlu")._GetBoolean;
		metin25_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin25_gorunen_adi")._GetString;
		metin25_baslangic.Value = _kullaniciparametreleri._GetParametre("metin25_baslangic")._GetInt;
		metin25_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin25_uzunluk")._GetInt;
		metin25_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin25_varsayilan_deger")._GetString;
		metin25_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin25_zorunlu")._GetBoolean;
		metin26_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin26_gorunen_adi")._GetString;
		metin26_baslangic.Value = _kullaniciparametreleri._GetParametre("metin26_baslangic")._GetInt;
		metin26_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin26_uzunluk")._GetInt;
		metin26_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin26_varsayilan_deger")._GetString;
		metin26_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin26_zorunlu")._GetBoolean;
		metin27_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin27_gorunen_adi")._GetString;
		metin27_baslangic.Value = _kullaniciparametreleri._GetParametre("metin27_baslangic")._GetInt;
		metin27_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin27_uzunluk")._GetInt;
		metin27_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin27_varsayilan_deger")._GetString;
		metin27_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin27_zorunlu")._GetBoolean;
		metin28_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin28_gorunen_adi")._GetString;
		metin28_baslangic.Value = _kullaniciparametreleri._GetParametre("metin28_baslangic")._GetInt;
		metin28_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin28_uzunluk")._GetInt;
		metin28_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin28_varsayilan_deger")._GetString;
		metin28_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin28_zorunlu")._GetBoolean;
		metin29_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin29_gorunen_adi")._GetString;
		metin29_baslangic.Value = _kullaniciparametreleri._GetParametre("metin29_baslangic")._GetInt;
		metin29_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin29_uzunluk")._GetInt;
		metin29_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin29_varsayilan_deger")._GetString;
		metin29_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin29_zorunlu")._GetBoolean;
		metin30_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin30_gorunen_adi")._GetString;
		metin30_baslangic.Value = _kullaniciparametreleri._GetParametre("metin30_baslangic")._GetInt;
		metin30_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin30_uzunluk")._GetInt;
		metin30_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin30_varsayilan_deger")._GetString;
		metin30_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin30_zorunlu")._GetBoolean;
		metin31_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin31_gorunen_adi")._GetString;
		metin31_baslangic.Value = _kullaniciparametreleri._GetParametre("metin31_baslangic")._GetInt;
		metin31_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin31_uzunluk")._GetInt;
		metin31_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin31_varsayilan_deger")._GetString;
		metin31_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin31_zorunlu")._GetBoolean;
		metin32_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin32_gorunen_adi")._GetString;
		metin32_baslangic.Value = _kullaniciparametreleri._GetParametre("metin32_baslangic")._GetInt;
		metin32_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin32_uzunluk")._GetInt;
		metin32_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin32_varsayilan_deger")._GetString;
		metin32_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin32_zorunlu")._GetBoolean;
		metin33_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin33_gorunen_adi")._GetString;
		metin33_baslangic.Value = _kullaniciparametreleri._GetParametre("metin33_baslangic")._GetInt;
		metin33_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin33_uzunluk")._GetInt;
		metin33_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin33_varsayilan_deger")._GetString;
		metin33_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin33_zorunlu")._GetBoolean;
		metin34_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin34_gorunen_adi")._GetString;
		metin34_baslangic.Value = _kullaniciparametreleri._GetParametre("metin34_baslangic")._GetInt;
		metin34_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin34_uzunluk")._GetInt;
		metin34_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin34_varsayilan_deger")._GetString;
		metin34_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin34_zorunlu")._GetBoolean;
		metin35_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin35_gorunen_adi")._GetString;
		metin35_baslangic.Value = _kullaniciparametreleri._GetParametre("metin35_baslangic")._GetInt;
		metin35_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin35_uzunluk")._GetInt;
		metin35_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin35_varsayilan_deger")._GetString;
		metin35_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin35_zorunlu")._GetBoolean;
		metin36_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin36_gorunen_adi")._GetString;
		metin36_baslangic.Value = _kullaniciparametreleri._GetParametre("metin36_baslangic")._GetInt;
		metin36_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin36_uzunluk")._GetInt;
		metin36_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin36_varsayilan_deger")._GetString;
		metin36_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin36_zorunlu")._GetBoolean;
		metin37_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin37_gorunen_adi")._GetString;
		metin37_baslangic.Value = _kullaniciparametreleri._GetParametre("metin37_baslangic")._GetInt;
		metin37_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin37_uzunluk")._GetInt;
		metin37_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin37_varsayilan_deger")._GetString;
		metin37_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin37_zorunlu")._GetBoolean;
		metin38_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin38_gorunen_adi")._GetString;
		metin38_baslangic.Value = _kullaniciparametreleri._GetParametre("metin38_baslangic")._GetInt;
		metin38_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin38_uzunluk")._GetInt;
		metin38_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin38_varsayilan_deger")._GetString;
		metin38_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin38_zorunlu")._GetBoolean;
		metin39_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin39_gorunen_adi")._GetString;
		metin39_baslangic.Value = _kullaniciparametreleri._GetParametre("metin39_baslangic")._GetInt;
		metin39_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin39_uzunluk")._GetInt;
		metin39_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin39_varsayilan_deger")._GetString;
		metin39_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin39_zorunlu")._GetBoolean;
		metin40_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin40_gorunen_adi")._GetString;
		metin40_baslangic.Value = _kullaniciparametreleri._GetParametre("metin40_baslangic")._GetInt;
		metin40_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin40_uzunluk")._GetInt;
		metin40_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin40_varsayilan_deger")._GetString;
		metin40_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin40_zorunlu")._GetBoolean;
		metin41_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin41_gorunen_adi")._GetString;
		metin41_baslangic.Value = _kullaniciparametreleri._GetParametre("metin41_baslangic")._GetInt;
		metin41_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin41_uzunluk")._GetInt;
		metin41_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin41_varsayilan_deger")._GetString;
		metin41_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin41_zorunlu")._GetBoolean;
		metin42_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin42_gorunen_adi")._GetString;
		metin42_baslangic.Value = _kullaniciparametreleri._GetParametre("metin42_baslangic")._GetInt;
		metin42_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin42_uzunluk")._GetInt;
		metin42_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin42_varsayilan_deger")._GetString;
		metin42_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin42_zorunlu")._GetBoolean;
		metin43_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin43_gorunen_adi")._GetString;
		metin43_baslangic.Value = _kullaniciparametreleri._GetParametre("metin43_baslangic")._GetInt;
		metin43_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin43_uzunluk")._GetInt;
		metin43_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin43_varsayilan_deger")._GetString;
		metin43_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin43_zorunlu")._GetBoolean;
		metin44_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin44_gorunen_adi")._GetString;
		metin44_baslangic.Value = _kullaniciparametreleri._GetParametre("metin44_baslangic")._GetInt;
		metin44_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin44_uzunluk")._GetInt;
		metin44_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin44_varsayilan_deger")._GetString;
		metin44_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin44_zorunlu")._GetBoolean;
		metin45_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin45_gorunen_adi")._GetString;
		metin45_baslangic.Value = _kullaniciparametreleri._GetParametre("metin45_baslangic")._GetInt;
		metin45_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin45_uzunluk")._GetInt;
		metin45_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin45_varsayilan_deger")._GetString;
		metin45_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin45_zorunlu")._GetBoolean;
		metin46_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin46_gorunen_adi")._GetString;
		metin46_baslangic.Value = _kullaniciparametreleri._GetParametre("metin46_baslangic")._GetInt;
		metin46_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin46_uzunluk")._GetInt;
		metin46_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin46_varsayilan_deger")._GetString;
		metin46_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin46_zorunlu")._GetBoolean;
		metin47_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin47_gorunen_adi")._GetString;
		metin47_baslangic.Value = _kullaniciparametreleri._GetParametre("metin47_baslangic")._GetInt;
		metin47_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin47_uzunluk")._GetInt;
		metin47_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin47_varsayilan_deger")._GetString;
		metin47_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin47_zorunlu")._GetBoolean;
		metin48_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin48_gorunen_adi")._GetString;
		metin48_baslangic.Value = _kullaniciparametreleri._GetParametre("metin48_baslangic")._GetInt;
		metin48_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin48_uzunluk")._GetInt;
		metin48_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin48_varsayilan_deger")._GetString;
		metin48_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin48_zorunlu")._GetBoolean;
		metin49_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin49_gorunen_adi")._GetString;
		metin49_baslangic.Value = _kullaniciparametreleri._GetParametre("metin49_baslangic")._GetInt;
		metin49_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin49_uzunluk")._GetInt;
		metin49_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin49_varsayilan_deger")._GetString;
		metin49_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin49_zorunlu")._GetBoolean;
		metin50_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("metin50_gorunen_adi")._GetString;
		metin50_baslangic.Value = _kullaniciparametreleri._GetParametre("metin50_baslangic")._GetInt;
		metin50_uzunluk.Value = _kullaniciparametreleri._GetParametre("metin50_uzunluk")._GetInt;
		metin50_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("metin50_varsayilan_deger")._GetString;
		metin50_zorunlu.Checked = _kullaniciparametreleri._GetParametre("metin50_zorunlu")._GetBoolean;
		dropbox1_zorunlu.Checked = _kullaniciparametreleri._GetParametre("dropbox1_zorunlu")._GetBoolean;
		dropbox1_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("dropbox1_gorunen_adi")._GetString;
		dropbox1_baslangic.Value = _kullaniciparametreleri._GetParametre("dropbox1_baslangic")._GetInt;
		dropbox1_uzunluk.Value = _kullaniciparametreleri._GetParametre("dropbox1_uzunluk")._GetInt;
		dropbox1_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("dropbox1_varsayilan_deger")._GetString;
		dropbox1_secenekler_yazi.Text = _kullaniciparametreleri._GetParametre("dropbox1_secenekler_yazi")._GetString;
		dropbox1_secenekler_veri.Text = _kullaniciparametreleri._GetParametre("dropbox1_secenekler_veri")._GetString;
		dropbox2_zorunlu.Checked = _kullaniciparametreleri._GetParametre("dropbox2_zorunlu")._GetBoolean;
		dropbox2_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("dropbox2_gorunen_adi")._GetString;
		dropbox2_baslangic.Value = _kullaniciparametreleri._GetParametre("dropbox2_baslangic")._GetInt;
		dropbox2_uzunluk.Value = _kullaniciparametreleri._GetParametre("dropbox2_uzunluk")._GetInt;
		dropbox2_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("dropbox2_varsayilan_deger")._GetString;
		dropbox2_secenekler_yazi.Text = _kullaniciparametreleri._GetParametre("dropbox2_secenekler_yazi")._GetString;
		dropbox2_secenekler_veri.Text = _kullaniciparametreleri._GetParametre("dropbox2_secenekler_veri")._GetString;
		dropbox3_zorunlu.Checked = _kullaniciparametreleri._GetParametre("dropbox3_zorunlu")._GetBoolean;
		dropbox3_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("dropbox3_gorunen_adi")._GetString;
		dropbox3_baslangic.Value = _kullaniciparametreleri._GetParametre("dropbox3_baslangic")._GetInt;
		dropbox3_uzunluk.Value = _kullaniciparametreleri._GetParametre("dropbox3_uzunluk")._GetInt;
		dropbox3_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("dropbox3_varsayilan_deger")._GetString;
		dropbox3_secenekler_yazi.Text = _kullaniciparametreleri._GetParametre("dropbox3_secenekler_yazi")._GetString;
		dropbox3_secenekler_veri.Text = _kullaniciparametreleri._GetParametre("dropbox3_secenekler_veri")._GetString;
		dropbox4_zorunlu.Checked = _kullaniciparametreleri._GetParametre("dropbox4_zorunlu")._GetBoolean;
		dropbox4_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("dropbox4_gorunen_adi")._GetString;
		dropbox4_baslangic.Value = _kullaniciparametreleri._GetParametre("dropbox4_baslangic")._GetInt;
		dropbox4_uzunluk.Value = _kullaniciparametreleri._GetParametre("dropbox4_uzunluk")._GetInt;
		dropbox4_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("dropbox4_varsayilan_deger")._GetString;
		dropbox4_secenekler_yazi.Text = _kullaniciparametreleri._GetParametre("dropbox4_secenekler_yazi")._GetString;
		dropbox4_secenekler_veri.Text = _kullaniciparametreleri._GetParametre("dropbox4_secenekler_veri")._GetString;
		dropbox5_zorunlu.Checked = _kullaniciparametreleri._GetParametre("dropbox5_zorunlu")._GetBoolean;
		dropbox5_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("dropbox5_gorunen_adi")._GetString;
		dropbox5_baslangic.Value = _kullaniciparametreleri._GetParametre("dropbox5_baslangic")._GetInt;
		dropbox5_uzunluk.Value = _kullaniciparametreleri._GetParametre("dropbox5_uzunluk")._GetInt;
		dropbox5_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("dropbox5_varsayilan_deger")._GetString;
		dropbox5_secenekler_yazi.Text = _kullaniciparametreleri._GetParametre("dropbox5_secenekler_yazi")._GetString;
		dropbox5_secenekler_veri.Text = _kullaniciparametreleri._GetParametre("dropbox5_secenekler_veri")._GetString;
		dropbox6_zorunlu.Checked = _kullaniciparametreleri._GetParametre("dropbox6_zorunlu")._GetBoolean;
		dropbox6_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("dropbox6_gorunen_adi")._GetString;
		dropbox6_baslangic.Value = _kullaniciparametreleri._GetParametre("dropbox6_baslangic")._GetInt;
		dropbox6_uzunluk.Value = _kullaniciparametreleri._GetParametre("dropbox6_uzunluk")._GetInt;
		dropbox6_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("dropbox6_varsayilan_deger")._GetString;
		dropbox6_secenekler_yazi.Text = _kullaniciparametreleri._GetParametre("dropbox6_secenekler_yazi")._GetString;
		dropbox6_secenekler_veri.Text = _kullaniciparametreleri._GetParametre("dropbox6_secenekler_veri")._GetString;
		dropbox7_zorunlu.Checked = _kullaniciparametreleri._GetParametre("dropbox7_zorunlu")._GetBoolean;
		dropbox7_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("dropbox7_gorunen_adi")._GetString;
		dropbox7_baslangic.Value = _kullaniciparametreleri._GetParametre("dropbox7_baslangic")._GetInt;
		dropbox7_uzunluk.Value = _kullaniciparametreleri._GetParametre("dropbox7_uzunluk")._GetInt;
		dropbox7_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("dropbox7_varsayilan_deger")._GetString;
		dropbox7_secenekler_yazi.Text = _kullaniciparametreleri._GetParametre("dropbox7_secenekler_yazi")._GetString;
		dropbox7_secenekler_veri.Text = _kullaniciparametreleri._GetParametre("dropbox7_secenekler_veri")._GetString;
		dropbox8_zorunlu.Checked = _kullaniciparametreleri._GetParametre("dropbox8_zorunlu")._GetBoolean;
		dropbox8_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("dropbox8_gorunen_adi")._GetString;
		dropbox8_baslangic.Value = _kullaniciparametreleri._GetParametre("dropbox8_baslangic")._GetInt;
		dropbox8_uzunluk.Value = _kullaniciparametreleri._GetParametre("dropbox8_uzunluk")._GetInt;
		dropbox8_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("dropbox8_varsayilan_deger")._GetString;
		dropbox8_secenekler_yazi.Text = _kullaniciparametreleri._GetParametre("dropbox8_secenekler_yazi")._GetString;
		dropbox8_secenekler_veri.Text = _kullaniciparametreleri._GetParametre("dropbox8_secenekler_veri")._GetString;
		dropbox9_zorunlu.Checked = _kullaniciparametreleri._GetParametre("dropbox9_zorunlu")._GetBoolean;
		dropbox9_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("dropbox9_gorunen_adi")._GetString;
		dropbox9_baslangic.Value = _kullaniciparametreleri._GetParametre("dropbox9_baslangic")._GetInt;
		dropbox9_uzunluk.Value = _kullaniciparametreleri._GetParametre("dropbox9_uzunluk")._GetInt;
		dropbox9_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("dropbox9_varsayilan_deger")._GetString;
		dropbox9_secenekler_yazi.Text = _kullaniciparametreleri._GetParametre("dropbox9_secenekler_yazi")._GetString;
		dropbox9_secenekler_veri.Text = _kullaniciparametreleri._GetParametre("dropbox9_secenekler_veri")._GetString;
		dropbox10_zorunlu.Checked = _kullaniciparametreleri._GetParametre("dropbox10_zorunlu")._GetBoolean;
		dropbox10_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("dropbox10_gorunen_adi")._GetString;
		dropbox10_baslangic.Value = _kullaniciparametreleri._GetParametre("dropbox10_baslangic")._GetInt;
		dropbox10_uzunluk.Value = _kullaniciparametreleri._GetParametre("dropbox10_uzunluk")._GetInt;
		dropbox10_varsayilan_deger.Text = _kullaniciparametreleri._GetParametre("dropbox10_varsayilan_deger")._GetString;
		dropbox10_secenekler_yazi.Text = _kullaniciparametreleri._GetParametre("dropbox10_secenekler_yazi")._GetString;
		dropbox10_secenekler_veri.Text = _kullaniciparametreleri._GetParametre("dropbox10_secenekler_veri")._GetString;
		checkbox1_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("checkbox1_gorunen_adi")._GetString;
		checkbox1_baslangic.Value = _kullaniciparametreleri._GetParametre("checkbox1_baslangic")._GetInt;
		checkbox1_uzunluk.Value = _kullaniciparametreleri._GetParametre("checkbox1_uzunluk")._GetInt;
		checkbox1_varsayilan_deger.Checked = _kullaniciparametreleri._GetParametre("checkbox1_varsayilan_deger")._GetBoolean;
		checkbox1_isaretli_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox1_isaretli_icin_deger")._GetString;
		checkbox1_isaretsiz_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox1_isaretsiz_icin_deger")._GetString;
		checkbox2_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("checkbox2_gorunen_adi")._GetString;
		checkbox2_baslangic.Value = _kullaniciparametreleri._GetParametre("checkbox2_baslangic")._GetInt;
		checkbox2_uzunluk.Value = _kullaniciparametreleri._GetParametre("checkbox2_uzunluk")._GetInt;
		checkbox2_varsayilan_deger.Checked = _kullaniciparametreleri._GetParametre("checkbox2_varsayilan_deger")._GetBoolean;
		checkbox2_isaretli_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox2_isaretli_icin_deger")._GetString;
		checkbox2_isaretsiz_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox2_isaretsiz_icin_deger")._GetString;
		checkbox3_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("checkbox3_gorunen_adi")._GetString;
		checkbox3_baslangic.Value = _kullaniciparametreleri._GetParametre("checkbox3_baslangic")._GetInt;
		checkbox3_uzunluk.Value = _kullaniciparametreleri._GetParametre("checkbox3_uzunluk")._GetInt;
		checkbox3_varsayilan_deger.Checked = _kullaniciparametreleri._GetParametre("checkbox3_varsayilan_deger")._GetBoolean;
		checkbox3_isaretli_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox3_isaretli_icin_deger")._GetString;
		checkbox3_isaretsiz_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox3_isaretsiz_icin_deger")._GetString;
		checkbox4_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("checkbox4_gorunen_adi")._GetString;
		checkbox4_baslangic.Value = _kullaniciparametreleri._GetParametre("checkbox4_baslangic")._GetInt;
		checkbox4_uzunluk.Value = _kullaniciparametreleri._GetParametre("checkbox4_uzunluk")._GetInt;
		checkbox4_varsayilan_deger.Checked = _kullaniciparametreleri._GetParametre("checkbox4_varsayilan_deger")._GetBoolean;
		checkbox4_isaretli_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox4_isaretli_icin_deger")._GetString;
		checkbox4_isaretsiz_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox4_isaretsiz_icin_deger")._GetString;
		checkbox5_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("checkbox5_gorunen_adi")._GetString;
		checkbox5_baslangic.Value = _kullaniciparametreleri._GetParametre("checkbox5_baslangic")._GetInt;
		checkbox5_uzunluk.Value = _kullaniciparametreleri._GetParametre("checkbox5_uzunluk")._GetInt;
		checkbox5_varsayilan_deger.Checked = _kullaniciparametreleri._GetParametre("checkbox5_varsayilan_deger")._GetBoolean;
		checkbox5_isaretli_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox5_isaretli_icin_deger")._GetString;
		checkbox5_isaretsiz_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox5_isaretsiz_icin_deger")._GetString;
		checkbox6_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("checkbox6_gorunen_adi")._GetString;
		checkbox6_baslangic.Value = _kullaniciparametreleri._GetParametre("checkbox6_baslangic")._GetInt;
		checkbox6_uzunluk.Value = _kullaniciparametreleri._GetParametre("checkbox6_uzunluk")._GetInt;
		checkbox6_varsayilan_deger.Checked = _kullaniciparametreleri._GetParametre("checkbox6_varsayilan_deger")._GetBoolean;
		checkbox6_isaretli_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox6_isaretli_icin_deger")._GetString;
		checkbox6_isaretsiz_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox6_isaretsiz_icin_deger")._GetString;
		checkbox7_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("checkbox7_gorunen_adi")._GetString;
		checkbox7_baslangic.Value = _kullaniciparametreleri._GetParametre("checkbox7_baslangic")._GetInt;
		checkbox7_uzunluk.Value = _kullaniciparametreleri._GetParametre("checkbox7_uzunluk")._GetInt;
		checkbox7_varsayilan_deger.Checked = _kullaniciparametreleri._GetParametre("checkbox7_varsayilan_deger")._GetBoolean;
		checkbox7_isaretli_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox7_isaretli_icin_deger")._GetString;
		checkbox7_isaretsiz_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox7_isaretsiz_icin_deger")._GetString;
		checkbox8_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("checkbox8_gorunen_adi")._GetString;
		checkbox8_baslangic.Value = _kullaniciparametreleri._GetParametre("checkbox8_baslangic")._GetInt;
		checkbox8_uzunluk.Value = _kullaniciparametreleri._GetParametre("checkbox8_uzunluk")._GetInt;
		checkbox8_varsayilan_deger.Checked = _kullaniciparametreleri._GetParametre("checkbox8_varsayilan_deger")._GetBoolean;
		checkbox8_isaretli_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox8_isaretli_icin_deger")._GetString;
		checkbox8_isaretsiz_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox8_isaretsiz_icin_deger")._GetString;
		checkbox9_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("checkbox9_gorunen_adi")._GetString;
		checkbox9_baslangic.Value = _kullaniciparametreleri._GetParametre("checkbox9_baslangic")._GetInt;
		checkbox9_uzunluk.Value = _kullaniciparametreleri._GetParametre("checkbox9_uzunluk")._GetInt;
		checkbox9_varsayilan_deger.Checked = _kullaniciparametreleri._GetParametre("checkbox9_varsayilan_deger")._GetBoolean;
		checkbox9_isaretli_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox9_isaretli_icin_deger")._GetString;
		checkbox9_isaretsiz_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox9_isaretsiz_icin_deger")._GetString;
		checkbox10_gorunen_adi.Text = _kullaniciparametreleri._GetParametre("checkbox10_gorunen_adi")._GetString;
		checkbox10_baslangic.Value = _kullaniciparametreleri._GetParametre("checkbox10_baslangic")._GetInt;
		checkbox10_uzunluk.Value = _kullaniciparametreleri._GetParametre("checkbox10_uzunluk")._GetInt;
		checkbox10_varsayilan_deger.Checked = _kullaniciparametreleri._GetParametre("checkbox10_varsayilan_deger")._GetBoolean;
		checkbox10_isaretli_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox10_isaretli_icin_deger")._GetString;
		checkbox10_isaretsiz_icin_deger.Text = _kullaniciparametreleri._GetParametre("checkbox10_isaretsiz_icin_deger")._GetString;
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
		KullaniciParametreKaydet();
	}

	private void KullaniciParametreKaydet()
	{
		te_kullanici_adi.Text = AktifKullanici;
		_kullaniciparametreleri._GetParametre("SablonAdi")._SetString = AktifKullanici;
		_kullaniciparametreleri._GetParametre("alinacak_satirlarin_baslangic_karakteri")._SetString = te_alinacaksatirlarinbaslangickarakteri.Text;
		_kullaniciparametreleri._GetParametre("alinacak_satirlarin_baslangic_karakteri_kullan")._SetBoolean = ce_alinacaksatirlarinbaslangickarakterikullan.Checked;
		_kullaniciparametreleri._GetParametre("bilgilerin_baslangic_satiri")._SetString = te_bilgilerinbaslangicsatiri.Text;
		_kullaniciparametreleri._GetParametre("bilgilerin_baslangic_satiri_kullan")._SetBoolean = ce_bilgilerinbaslangicsatirikullan.Checked;
		_kullaniciparametreleri._GetParametre("ayrac_karakteri")._SetString = te_ayrackarakteri.Text;
		_kullaniciparametreleri._GetParametre("ayrac_karakteri_kullan")._SetBoolean = ce_ayrackarakterikullan.Checked;
		_kullaniciparametreleri._GetParametre("unvan_baslangic")._SetString = te_unvan.Text;
		_kullaniciparametreleri._GetParametre("unvan_uzunluk")._SetString = te_unvan_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("unvan2_baslangic")._SetString = te_unvan2.Text;
		_kullaniciparametreleri._GetParametre("unvan2_uzunluk")._SetString = te_unvan2_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("adres_baslangic")._SetString = te_adres.Text;
		_kullaniciparametreleri._GetParametre("adres_uzunluk")._SetString = te_adres_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("mahalle_baslangic")._SetString = te_mahalle.Text;
		_kullaniciparametreleri._GetParametre("mahalle_uzunluk")._SetString = te_mahalle_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("ilce_baslangic")._SetString = te_ilce.Text;
		_kullaniciparametreleri._GetParametre("ilce_uzunluk")._SetString = te_ilce_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("il_baslangic")._SetString = te_il.Text;
		_kullaniciparametreleri._GetParametre("il_uzunluk")._SetString = te_il_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("ulke_baslangic")._SetString = te_ulke.Text;
		_kullaniciparametreleri._GetParametre("ulke_uzunluk")._SetString = te_ulke_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("postakodu_baslangic")._SetString = te_postakodu.Text;
		_kullaniciparametreleri._GetParametre("postakodu_uzunluk")._SetString = te_postakodu_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("telefon_baslangic")._SetString = te_telefon.Text;
		_kullaniciparametreleri._GetParametre("telefon_uzunluk")._SetString = te_telefon_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("eposta_baslangic")._SetString = te_eposta.Text;
		_kullaniciparametreleri._GetParametre("eposta_uzunluk")._SetString = te_eposta_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("tckimlik_vergino_baslangic")._SetString = te_tckimlikvergino.Text;
		_kullaniciparametreleri._GetParametre("tckimlik_vergino_uzunluk")._SetString = te_tckimlikvergino_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("banka_hesap_no_baslangic")._SetString = te_bankahesapno.Text;
		_kullaniciparametreleri._GetParametre("banka_hesap_no_uzunluk")._SetString = te_bankahesapno_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("tarih_yil_baslangic")._SetString = te_tarihyil.Text;
		_kullaniciparametreleri._GetParametre("tarih_yil_uzunluk")._SetString = te_tarihyil_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("tarih_ay_baslangic")._SetString = te_tarihay.Text;
		_kullaniciparametreleri._GetParametre("tarih_ay_uzunluk")._SetString = te_tarihay_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("tarih_gun_baslangic")._SetString = te_tarihgun.Text;
		_kullaniciparametreleri._GetParametre("tarih_gun_uzunluk")._SetString = te_tarihgun_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("aciklama_baslangic")._SetString = te_aciklama.Text;
		_kullaniciparametreleri._GetParametre("aciklama_uzunluk")._SetString = te_aciklama_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("tutar_baslangic")._SetString = te_tutar.Text;
		_kullaniciparametreleri._GetParametre("tutar_uzunluk")._SetString = te_tutar_uzunluk.Text;
		_kullaniciparametreleri._GetParametre("mikro_disi_ek_bilgileri_kullan")._SetBoolean = mikro_disi_ek_bilgileri_kullan.Checked;
		_kullaniciparametreleri._GetParametre("metin1_gorunen_adi")._SetString = metin1_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin1_baslangic")._SetInt = (int)metin1_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin1_uzunluk")._SetInt = (int)metin1_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin1_varsayilan_deger")._SetString = metin1_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin1_zorunlu")._SetBoolean = metin1_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin2_gorunen_adi")._SetString = metin2_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin2_baslangic")._SetInt = (int)metin2_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin2_uzunluk")._SetInt = (int)metin2_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin2_varsayilan_deger")._SetString = metin2_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin2_zorunlu")._SetBoolean = metin2_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin3_gorunen_adi")._SetString = metin3_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin3_baslangic")._SetInt = (int)metin3_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin3_uzunluk")._SetInt = (int)metin3_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin3_varsayilan_deger")._SetString = metin3_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin3_zorunlu")._SetBoolean = metin3_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin4_gorunen_adi")._SetString = metin4_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin4_baslangic")._SetInt = (int)metin4_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin4_uzunluk")._SetInt = (int)metin4_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin4_varsayilan_deger")._SetString = metin4_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin4_zorunlu")._SetBoolean = metin4_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin5_gorunen_adi")._SetString = metin5_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin5_baslangic")._SetInt = (int)metin5_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin5_uzunluk")._SetInt = (int)metin5_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin5_varsayilan_deger")._SetString = metin5_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin5_zorunlu")._SetBoolean = metin5_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin6_gorunen_adi")._SetString = metin6_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin6_baslangic")._SetInt = (int)metin6_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin6_uzunluk")._SetInt = (int)metin6_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin6_varsayilan_deger")._SetString = metin6_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin6_zorunlu")._SetBoolean = metin6_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin7_gorunen_adi")._SetString = metin7_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin7_baslangic")._SetInt = (int)metin7_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin7_uzunluk")._SetInt = (int)metin7_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin7_varsayilan_deger")._SetString = metin7_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin7_zorunlu")._SetBoolean = metin7_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin8_gorunen_adi")._SetString = metin8_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin8_baslangic")._SetInt = (int)metin8_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin8_uzunluk")._SetInt = (int)metin8_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin8_varsayilan_deger")._SetString = metin8_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin8_zorunlu")._SetBoolean = metin8_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin9_gorunen_adi")._SetString = metin9_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin9_baslangic")._SetInt = (int)metin9_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin9_uzunluk")._SetInt = (int)metin9_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin9_varsayilan_deger")._SetString = metin9_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin9_zorunlu")._SetBoolean = metin9_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin10_gorunen_adi")._SetString = metin10_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin10_baslangic")._SetInt = (int)metin10_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin10_uzunluk")._SetInt = (int)metin10_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin10_varsayilan_deger")._SetString = metin10_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin10_zorunlu")._SetBoolean = metin10_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin11_gorunen_adi")._SetString = metin11_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin11_baslangic")._SetInt = (int)metin11_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin11_uzunluk")._SetInt = (int)metin11_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin11_varsayilan_deger")._SetString = metin11_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin11_zorunlu")._SetBoolean = metin11_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin12_gorunen_adi")._SetString = metin12_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin12_baslangic")._SetInt = (int)metin12_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin12_uzunluk")._SetInt = (int)metin12_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin12_varsayilan_deger")._SetString = metin12_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin12_zorunlu")._SetBoolean = metin12_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin13_gorunen_adi")._SetString = metin13_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin13_baslangic")._SetInt = (int)metin13_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin13_uzunluk")._SetInt = (int)metin13_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin13_varsayilan_deger")._SetString = metin13_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin13_zorunlu")._SetBoolean = metin13_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin14_gorunen_adi")._SetString = metin14_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin14_baslangic")._SetInt = (int)metin14_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin14_uzunluk")._SetInt = (int)metin14_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin14_varsayilan_deger")._SetString = metin14_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin14_zorunlu")._SetBoolean = metin14_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin15_gorunen_adi")._SetString = metin15_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin15_baslangic")._SetInt = (int)metin15_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin15_uzunluk")._SetInt = (int)metin15_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin15_varsayilan_deger")._SetString = metin15_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin15_zorunlu")._SetBoolean = metin15_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin16_gorunen_adi")._SetString = metin16_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin16_baslangic")._SetInt = (int)metin16_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin16_uzunluk")._SetInt = (int)metin16_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin16_varsayilan_deger")._SetString = metin16_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin16_zorunlu")._SetBoolean = metin16_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin17_gorunen_adi")._SetString = metin17_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin17_baslangic")._SetInt = (int)metin17_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin17_uzunluk")._SetInt = (int)metin17_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin17_varsayilan_deger")._SetString = metin17_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin17_zorunlu")._SetBoolean = metin17_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin18_gorunen_adi")._SetString = metin18_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin18_baslangic")._SetInt = (int)metin18_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin18_uzunluk")._SetInt = (int)metin18_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin18_varsayilan_deger")._SetString = metin18_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin18_zorunlu")._SetBoolean = metin18_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin19_gorunen_adi")._SetString = metin19_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin19_baslangic")._SetInt = (int)metin19_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin19_uzunluk")._SetInt = (int)metin19_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin19_varsayilan_deger")._SetString = metin19_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin19_zorunlu")._SetBoolean = metin19_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin20_gorunen_adi")._SetString = metin20_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin20_baslangic")._SetInt = (int)metin20_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin20_uzunluk")._SetInt = (int)metin20_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin20_varsayilan_deger")._SetString = metin20_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin20_zorunlu")._SetBoolean = metin20_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin21_gorunen_adi")._SetString = metin21_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin21_baslangic")._SetInt = (int)metin21_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin21_uzunluk")._SetInt = (int)metin21_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin21_varsayilan_deger")._SetString = metin21_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin21_zorunlu")._SetBoolean = metin21_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin22_gorunen_adi")._SetString = metin22_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin22_baslangic")._SetInt = (int)metin22_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin22_uzunluk")._SetInt = (int)metin22_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin22_varsayilan_deger")._SetString = metin22_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin22_zorunlu")._SetBoolean = metin22_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin23_gorunen_adi")._SetString = metin23_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin23_baslangic")._SetInt = (int)metin23_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin23_uzunluk")._SetInt = (int)metin23_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin23_varsayilan_deger")._SetString = metin23_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin23_zorunlu")._SetBoolean = metin23_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin24_gorunen_adi")._SetString = metin24_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin24_baslangic")._SetInt = (int)metin24_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin24_uzunluk")._SetInt = (int)metin24_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin24_varsayilan_deger")._SetString = metin24_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin24_zorunlu")._SetBoolean = metin24_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin25_gorunen_adi")._SetString = metin25_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin25_baslangic")._SetInt = (int)metin25_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin25_uzunluk")._SetInt = (int)metin25_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin25_varsayilan_deger")._SetString = metin25_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin25_zorunlu")._SetBoolean = metin25_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin26_gorunen_adi")._SetString = metin26_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin26_baslangic")._SetInt = (int)metin26_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin26_uzunluk")._SetInt = (int)metin26_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin26_varsayilan_deger")._SetString = metin26_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin26_zorunlu")._SetBoolean = metin26_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin27_gorunen_adi")._SetString = metin27_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin27_baslangic")._SetInt = (int)metin27_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin27_uzunluk")._SetInt = (int)metin27_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin27_varsayilan_deger")._SetString = metin27_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin27_zorunlu")._SetBoolean = metin27_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin28_gorunen_adi")._SetString = metin28_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin28_baslangic")._SetInt = (int)metin28_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin28_uzunluk")._SetInt = (int)metin28_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin28_varsayilan_deger")._SetString = metin28_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin28_zorunlu")._SetBoolean = metin28_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin29_gorunen_adi")._SetString = metin29_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin29_baslangic")._SetInt = (int)metin29_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin29_uzunluk")._SetInt = (int)metin29_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin29_varsayilan_deger")._SetString = metin29_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin29_zorunlu")._SetBoolean = metin29_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin30_gorunen_adi")._SetString = metin30_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin30_baslangic")._SetInt = (int)metin30_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin30_uzunluk")._SetInt = (int)metin30_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin30_varsayilan_deger")._SetString = metin30_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin30_zorunlu")._SetBoolean = metin30_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin31_gorunen_adi")._SetString = metin31_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin31_baslangic")._SetInt = (int)metin31_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin31_uzunluk")._SetInt = (int)metin31_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin31_varsayilan_deger")._SetString = metin31_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin31_zorunlu")._SetBoolean = metin31_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin32_gorunen_adi")._SetString = metin32_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin32_baslangic")._SetInt = (int)metin32_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin32_uzunluk")._SetInt = (int)metin32_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin32_varsayilan_deger")._SetString = metin32_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin32_zorunlu")._SetBoolean = metin32_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin33_gorunen_adi")._SetString = metin33_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin33_baslangic")._SetInt = (int)metin33_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin33_uzunluk")._SetInt = (int)metin33_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin33_varsayilan_deger")._SetString = metin33_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin33_zorunlu")._SetBoolean = metin33_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin34_gorunen_adi")._SetString = metin34_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin34_baslangic")._SetInt = (int)metin34_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin34_uzunluk")._SetInt = (int)metin34_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin34_varsayilan_deger")._SetString = metin34_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin34_zorunlu")._SetBoolean = metin34_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin35_gorunen_adi")._SetString = metin35_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin35_baslangic")._SetInt = (int)metin35_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin35_uzunluk")._SetInt = (int)metin35_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin35_varsayilan_deger")._SetString = metin35_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin35_zorunlu")._SetBoolean = metin35_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin36_gorunen_adi")._SetString = metin36_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin36_baslangic")._SetInt = (int)metin36_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin36_uzunluk")._SetInt = (int)metin36_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin36_varsayilan_deger")._SetString = metin36_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin36_zorunlu")._SetBoolean = metin36_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin37_gorunen_adi")._SetString = metin37_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin37_baslangic")._SetInt = (int)metin37_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin37_uzunluk")._SetInt = (int)metin37_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin37_varsayilan_deger")._SetString = metin37_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin37_zorunlu")._SetBoolean = metin37_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin38_gorunen_adi")._SetString = metin38_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin38_baslangic")._SetInt = (int)metin38_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin38_uzunluk")._SetInt = (int)metin38_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin38_varsayilan_deger")._SetString = metin38_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin38_zorunlu")._SetBoolean = metin38_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin39_gorunen_adi")._SetString = metin39_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin39_baslangic")._SetInt = (int)metin39_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin39_uzunluk")._SetInt = (int)metin39_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin39_varsayilan_deger")._SetString = metin39_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin39_zorunlu")._SetBoolean = metin39_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin40_gorunen_adi")._SetString = metin40_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin40_baslangic")._SetInt = (int)metin40_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin40_uzunluk")._SetInt = (int)metin40_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin40_varsayilan_deger")._SetString = metin40_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin40_zorunlu")._SetBoolean = metin40_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin41_gorunen_adi")._SetString = metin41_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin41_baslangic")._SetInt = (int)metin41_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin41_uzunluk")._SetInt = (int)metin41_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin41_varsayilan_deger")._SetString = metin41_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin41_zorunlu")._SetBoolean = metin41_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin42_gorunen_adi")._SetString = metin42_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin42_baslangic")._SetInt = (int)metin42_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin42_uzunluk")._SetInt = (int)metin42_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin42_varsayilan_deger")._SetString = metin42_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin42_zorunlu")._SetBoolean = metin42_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin43_gorunen_adi")._SetString = metin43_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin43_baslangic")._SetInt = (int)metin43_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin43_uzunluk")._SetInt = (int)metin43_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin43_varsayilan_deger")._SetString = metin43_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin43_zorunlu")._SetBoolean = metin43_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin44_gorunen_adi")._SetString = metin44_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin44_baslangic")._SetInt = (int)metin44_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin44_uzunluk")._SetInt = (int)metin44_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin44_varsayilan_deger")._SetString = metin44_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin44_zorunlu")._SetBoolean = metin44_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin45_gorunen_adi")._SetString = metin45_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin45_baslangic")._SetInt = (int)metin45_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin45_uzunluk")._SetInt = (int)metin45_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin45_varsayilan_deger")._SetString = metin45_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin45_zorunlu")._SetBoolean = metin45_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin46_gorunen_adi")._SetString = metin46_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin46_baslangic")._SetInt = (int)metin46_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin46_uzunluk")._SetInt = (int)metin46_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin46_varsayilan_deger")._SetString = metin46_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin46_zorunlu")._SetBoolean = metin46_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin47_gorunen_adi")._SetString = metin47_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin47_baslangic")._SetInt = (int)metin47_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin47_uzunluk")._SetInt = (int)metin47_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin47_varsayilan_deger")._SetString = metin47_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin47_zorunlu")._SetBoolean = metin47_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin48_gorunen_adi")._SetString = metin48_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin48_baslangic")._SetInt = (int)metin48_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin48_uzunluk")._SetInt = (int)metin48_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin48_varsayilan_deger")._SetString = metin48_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin48_zorunlu")._SetBoolean = metin48_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin49_gorunen_adi")._SetString = metin49_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin49_baslangic")._SetInt = (int)metin49_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin49_uzunluk")._SetInt = (int)metin49_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin49_varsayilan_deger")._SetString = metin49_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin49_zorunlu")._SetBoolean = metin49_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("metin50_gorunen_adi")._SetString = metin50_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("metin50_baslangic")._SetInt = (int)metin50_baslangic.Value;
		_kullaniciparametreleri._GetParametre("metin50_uzunluk")._SetInt = (int)metin50_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("metin50_varsayilan_deger")._SetString = metin50_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("metin50_zorunlu")._SetBoolean = metin50_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox1_zorunlu")._SetBoolean = dropbox1_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox1_gorunen_adi")._SetString = dropbox1_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("dropbox1_baslangic")._SetInt = (int)dropbox1_baslangic.Value;
		_kullaniciparametreleri._GetParametre("dropbox1_uzunluk")._SetInt = (int)dropbox1_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("dropbox1_varsayilan_deger")._SetString = dropbox1_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("dropbox1_secenekler_yazi")._SetString = dropbox1_secenekler_yazi.Text;
		_kullaniciparametreleri._GetParametre("dropbox1_secenekler_veri")._SetString = dropbox1_secenekler_veri.Text;
		_kullaniciparametreleri._GetParametre("dropbox2_zorunlu")._SetBoolean = dropbox2_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox2_gorunen_adi")._SetString = dropbox2_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("dropbox2_baslangic")._SetInt = (int)dropbox2_baslangic.Value;
		_kullaniciparametreleri._GetParametre("dropbox2_uzunluk")._SetInt = (int)dropbox2_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("dropbox2_varsayilan_deger")._SetString = dropbox2_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("dropbox2_secenekler_yazi")._SetString = dropbox2_secenekler_yazi.Text;
		_kullaniciparametreleri._GetParametre("dropbox2_secenekler_veri")._SetString = dropbox2_secenekler_veri.Text;
		_kullaniciparametreleri._GetParametre("dropbox3_zorunlu")._SetBoolean = dropbox3_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox3_gorunen_adi")._SetString = dropbox3_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("dropbox3_baslangic")._SetInt = (int)dropbox3_baslangic.Value;
		_kullaniciparametreleri._GetParametre("dropbox3_uzunluk")._SetInt = (int)dropbox3_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("dropbox3_varsayilan_deger")._SetString = dropbox3_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("dropbox3_secenekler_yazi")._SetString = dropbox3_secenekler_yazi.Text;
		_kullaniciparametreleri._GetParametre("dropbox3_secenekler_veri")._SetString = dropbox3_secenekler_veri.Text;
		_kullaniciparametreleri._GetParametre("dropbox4_zorunlu")._SetBoolean = dropbox4_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox4_gorunen_adi")._SetString = dropbox4_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("dropbox4_baslangic")._SetInt = (int)dropbox4_baslangic.Value;
		_kullaniciparametreleri._GetParametre("dropbox4_uzunluk")._SetInt = (int)dropbox4_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("dropbox4_varsayilan_deger")._SetString = dropbox4_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("dropbox4_secenekler_yazi")._SetString = dropbox4_secenekler_yazi.Text;
		_kullaniciparametreleri._GetParametre("dropbox4_secenekler_veri")._SetString = dropbox4_secenekler_veri.Text;
		_kullaniciparametreleri._GetParametre("dropbox5_zorunlu")._SetBoolean = dropbox5_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox5_gorunen_adi")._SetString = dropbox5_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("dropbox5_baslangic")._SetInt = (int)dropbox5_baslangic.Value;
		_kullaniciparametreleri._GetParametre("dropbox5_uzunluk")._SetInt = (int)dropbox5_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("dropbox5_varsayilan_deger")._SetString = dropbox5_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("dropbox5_secenekler_yazi")._SetString = dropbox5_secenekler_yazi.Text;
		_kullaniciparametreleri._GetParametre("dropbox5_secenekler_veri")._SetString = dropbox5_secenekler_veri.Text;
		_kullaniciparametreleri._GetParametre("dropbox6_zorunlu")._SetBoolean = dropbox6_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox6_gorunen_adi")._SetString = dropbox6_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("dropbox6_baslangic")._SetInt = (int)dropbox6_baslangic.Value;
		_kullaniciparametreleri._GetParametre("dropbox6_uzunluk")._SetInt = (int)dropbox6_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("dropbox6_varsayilan_deger")._SetString = dropbox6_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("dropbox6_secenekler_yazi")._SetString = dropbox6_secenekler_yazi.Text;
		_kullaniciparametreleri._GetParametre("dropbox6_secenekler_veri")._SetString = dropbox6_secenekler_veri.Text;
		_kullaniciparametreleri._GetParametre("dropbox7_zorunlu")._SetBoolean = dropbox7_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox7_gorunen_adi")._SetString = dropbox7_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("dropbox7_baslangic")._SetInt = (int)dropbox7_baslangic.Value;
		_kullaniciparametreleri._GetParametre("dropbox7_uzunluk")._SetInt = (int)dropbox7_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("dropbox7_varsayilan_deger")._SetString = dropbox7_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("dropbox7_secenekler_yazi")._SetString = dropbox7_secenekler_yazi.Text;
		_kullaniciparametreleri._GetParametre("dropbox7_secenekler_veri")._SetString = dropbox7_secenekler_veri.Text;
		_kullaniciparametreleri._GetParametre("dropbox8_zorunlu")._SetBoolean = dropbox8_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox8_gorunen_adi")._SetString = dropbox8_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("dropbox8_baslangic")._SetInt = (int)dropbox8_baslangic.Value;
		_kullaniciparametreleri._GetParametre("dropbox8_uzunluk")._SetInt = (int)dropbox8_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("dropbox8_varsayilan_deger")._SetString = dropbox8_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("dropbox8_secenekler_yazi")._SetString = dropbox8_secenekler_yazi.Text;
		_kullaniciparametreleri._GetParametre("dropbox8_secenekler_veri")._SetString = dropbox8_secenekler_veri.Text;
		_kullaniciparametreleri._GetParametre("dropbox9_zorunlu")._SetBoolean = dropbox9_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox9_gorunen_adi")._SetString = dropbox9_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("dropbox9_baslangic")._SetInt = (int)dropbox9_baslangic.Value;
		_kullaniciparametreleri._GetParametre("dropbox9_uzunluk")._SetInt = (int)dropbox9_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("dropbox9_varsayilan_deger")._SetString = dropbox9_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("dropbox9_secenekler_yazi")._SetString = dropbox9_secenekler_yazi.Text;
		_kullaniciparametreleri._GetParametre("dropbox9_secenekler_veri")._SetString = dropbox9_secenekler_veri.Text;
		_kullaniciparametreleri._GetParametre("dropbox10_zorunlu")._SetBoolean = dropbox10_zorunlu.Checked;
		_kullaniciparametreleri._GetParametre("dropbox10_gorunen_adi")._SetString = dropbox10_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("dropbox10_baslangic")._SetInt = (int)dropbox10_baslangic.Value;
		_kullaniciparametreleri._GetParametre("dropbox10_uzunluk")._SetInt = (int)dropbox10_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("dropbox10_varsayilan_deger")._SetString = dropbox10_varsayilan_deger.Text;
		_kullaniciparametreleri._GetParametre("dropbox10_secenekler_yazi")._SetString = dropbox10_secenekler_yazi.Text;
		_kullaniciparametreleri._GetParametre("dropbox10_secenekler_veri")._SetString = dropbox10_secenekler_veri.Text;
		_kullaniciparametreleri._GetParametre("checkbox1_gorunen_adi")._SetString = checkbox1_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("checkbox1_baslangic")._SetInt = (int)checkbox1_baslangic.Value;
		_kullaniciparametreleri._GetParametre("checkbox1_uzunluk")._SetInt = (int)checkbox1_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("checkbox1_varsayilan_deger")._SetBoolean = checkbox1_varsayilan_deger.Checked;
		_kullaniciparametreleri._GetParametre("checkbox1_isaretli_icin_deger")._SetString = checkbox1_isaretli_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox1_isaretsiz_icin_deger")._SetString = checkbox1_isaretsiz_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox2_gorunen_adi")._SetString = checkbox2_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("checkbox2_baslangic")._SetInt = (int)checkbox2_baslangic.Value;
		_kullaniciparametreleri._GetParametre("checkbox2_uzunluk")._SetInt = (int)checkbox2_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("checkbox2_varsayilan_deger")._SetBoolean = checkbox2_varsayilan_deger.Checked;
		_kullaniciparametreleri._GetParametre("checkbox2_isaretli_icin_deger")._SetString = checkbox2_isaretli_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox2_isaretsiz_icin_deger")._SetString = checkbox2_isaretsiz_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox3_gorunen_adi")._SetString = checkbox3_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("checkbox3_baslangic")._SetInt = (int)checkbox3_baslangic.Value;
		_kullaniciparametreleri._GetParametre("checkbox3_uzunluk")._SetInt = (int)checkbox3_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("checkbox3_varsayilan_deger")._SetBoolean = checkbox3_varsayilan_deger.Checked;
		_kullaniciparametreleri._GetParametre("checkbox3_isaretli_icin_deger")._SetString = checkbox3_isaretli_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox3_isaretsiz_icin_deger")._SetString = checkbox3_isaretsiz_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox4_gorunen_adi")._SetString = checkbox4_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("checkbox4_baslangic")._SetInt = (int)checkbox4_baslangic.Value;
		_kullaniciparametreleri._GetParametre("checkbox4_uzunluk")._SetInt = (int)checkbox4_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("checkbox4_varsayilan_deger")._SetBoolean = checkbox4_varsayilan_deger.Checked;
		_kullaniciparametreleri._GetParametre("checkbox4_isaretli_icin_deger")._SetString = checkbox4_isaretli_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox4_isaretsiz_icin_deger")._SetString = checkbox4_isaretsiz_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox5_gorunen_adi")._SetString = checkbox5_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("checkbox5_baslangic")._SetInt = (int)checkbox5_baslangic.Value;
		_kullaniciparametreleri._GetParametre("checkbox5_uzunluk")._SetInt = (int)checkbox5_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("checkbox5_varsayilan_deger")._SetBoolean = checkbox5_varsayilan_deger.Checked;
		_kullaniciparametreleri._GetParametre("checkbox5_isaretli_icin_deger")._SetString = checkbox5_isaretli_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox5_isaretsiz_icin_deger")._SetString = checkbox5_isaretsiz_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox6_gorunen_adi")._SetString = checkbox6_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("checkbox6_baslangic")._SetInt = (int)checkbox6_baslangic.Value;
		_kullaniciparametreleri._GetParametre("checkbox6_uzunluk")._SetInt = (int)checkbox6_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("checkbox6_varsayilan_deger")._SetBoolean = checkbox6_varsayilan_deger.Checked;
		_kullaniciparametreleri._GetParametre("checkbox6_isaretli_icin_deger")._SetString = checkbox6_isaretli_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox6_isaretsiz_icin_deger")._SetString = checkbox6_isaretsiz_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox7_gorunen_adi")._SetString = checkbox7_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("checkbox7_baslangic")._SetInt = (int)checkbox7_baslangic.Value;
		_kullaniciparametreleri._GetParametre("checkbox7_uzunluk")._SetInt = (int)checkbox7_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("checkbox7_varsayilan_deger")._SetBoolean = checkbox7_varsayilan_deger.Checked;
		_kullaniciparametreleri._GetParametre("checkbox7_isaretli_icin_deger")._SetString = checkbox7_isaretli_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox7_isaretsiz_icin_deger")._SetString = checkbox7_isaretsiz_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox8_gorunen_adi")._SetString = checkbox8_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("checkbox8_baslangic")._SetInt = (int)checkbox8_baslangic.Value;
		_kullaniciparametreleri._GetParametre("checkbox8_uzunluk")._SetInt = (int)checkbox8_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("checkbox8_varsayilan_deger")._SetBoolean = checkbox8_varsayilan_deger.Checked;
		_kullaniciparametreleri._GetParametre("checkbox8_isaretli_icin_deger")._SetString = checkbox8_isaretli_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox8_isaretsiz_icin_deger")._SetString = checkbox8_isaretsiz_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox9_gorunen_adi")._SetString = checkbox9_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("checkbox9_baslangic")._SetInt = (int)checkbox9_baslangic.Value;
		_kullaniciparametreleri._GetParametre("checkbox9_uzunluk")._SetInt = (int)checkbox9_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("checkbox9_varsayilan_deger")._SetBoolean = checkbox9_varsayilan_deger.Checked;
		_kullaniciparametreleri._GetParametre("checkbox9_isaretli_icin_deger")._SetString = checkbox9_isaretli_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox9_isaretsiz_icin_deger")._SetString = checkbox9_isaretsiz_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox10_gorunen_adi")._SetString = checkbox10_gorunen_adi.Text;
		_kullaniciparametreleri._GetParametre("checkbox10_baslangic")._SetInt = (int)checkbox10_baslangic.Value;
		_kullaniciparametreleri._GetParametre("checkbox10_uzunluk")._SetInt = (int)checkbox10_uzunluk.Value;
		_kullaniciparametreleri._GetParametre("checkbox10_varsayilan_deger")._SetBoolean = checkbox10_varsayilan_deger.Checked;
		_kullaniciparametreleri._GetParametre("checkbox10_isaretli_icin_deger")._SetString = checkbox10_isaretli_icin_deger.Text;
		_kullaniciparametreleri._GetParametre("checkbox10_isaretsiz_icin_deger")._SetString = checkbox10_isaretsiz_icin_deger.Text;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri);
		_kullaniciparametreleri = ParametrelerDefault.BankaAktarim(AktifKullanici);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri, "BankaAktarim", "", "AktarimSablon", AktifKullanici);
		EkranBilgiGuncelle();
		DegisiklikVar = false;
	}

	private void ParametreTextEdit_KeyDown(object sender, KeyEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void ParametreCheckEditMouseClick(object sender, MouseEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void sb_kullanici_ekle_Click(object sender, EventArgs e)
	{
		bool flag = true;
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				flag = true;
				break;
			case DialogResult.No:
				flag = true;
				break;
			case DialogResult.Cancel:
				flag = false;
				break;
			}
		}
		if (te_eklenecek_parametre_adi.Text == "")
		{
			MessageBox.Show("Eklenecek kaydın adını girmelisiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			flag = false;
		}
		if (flag)
		{
			Parametreler parametreler = ParametrelerDefault.BankaAktarim(te_eklenecek_parametre_adi.Text);
			parametreler._GetParametre("SablonAdi")._SetString = te_eklenecek_parametre_adi.Text;
			ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler);
			KullanicilariListele();
			lb_kullanicilar.SelectedItem = te_eklenecek_parametre_adi.Text;
		}
	}

	private void sb_kullanici_sil_Click(object sender, EventArgs e)
	{
		if (lb_kullanicilar.SelectedValue == null)
		{
			return;
		}
		bool flag = true;
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				flag = true;
				break;
			case DialogResult.No:
				flag = true;
				break;
			case DialogResult.Cancel:
				flag = false;
				break;
			}
		}
		string text = lb_kullanicilar.SelectedValue.ToString();
		if (flag)
		{
			DialogResult dialogResult = MessageBox.Show(text + " kullanıcısını silmek istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult != DialogResult.Yes)
			{
				_ = 7;
				return;
			}
			AktarimBankaAktarimParametre.ParametreSil(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text);
			KullanicilariListele();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.lb_kullanicilar = new DevExpress.XtraEditors.ListBoxControl();
		this.sb_kullanici_ekle = new DevExpress.XtraEditors.SimpleButton();
		this.sb_kullanici_sil = new DevExpress.XtraEditors.SimpleButton();
		this.sb_ayarlari_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.tc_parametreler = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage3 = new DevExpress.XtraTab.XtraTabPage();
		this.label44 = new System.Windows.Forms.Label();
		this.label43 = new System.Windows.Forms.Label();
		this.te_bakiye_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_bakiye = new DevExpress.XtraEditors.TextEdit();
		this.label42 = new System.Windows.Forms.Label();
		this.label41 = new System.Windows.Forms.Label();
		this.label39 = new System.Windows.Forms.Label();
		this.label40 = new System.Windows.Forms.Label();
		this.te_mahalle_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_mahalle = new DevExpress.XtraEditors.TextEdit();
		this.label14 = new System.Windows.Forms.Label();
		this.te_unvan2_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_unvan2 = new DevExpress.XtraEditors.TextEdit();
		this.label15 = new System.Windows.Forms.Label();
		this.label32 = new System.Windows.Forms.Label();
		this.te_bankahesapno_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_tckimlikvergino_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_eposta_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_telefon_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_postakodu_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_ulke_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_il_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_ilce_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_adres_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_unvan_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.label20 = new System.Windows.Forms.Label();
		this.te_bankahesapno = new DevExpress.XtraEditors.TextEdit();
		this.label28 = new System.Windows.Forms.Label();
		this.te_tckimlikvergino = new DevExpress.XtraEditors.TextEdit();
		this.label27 = new System.Windows.Forms.Label();
		this.te_eposta = new DevExpress.XtraEditors.TextEdit();
		this.label26 = new System.Windows.Forms.Label();
		this.te_telefon = new DevExpress.XtraEditors.TextEdit();
		this.label25 = new System.Windows.Forms.Label();
		this.te_postakodu = new DevExpress.XtraEditors.TextEdit();
		this.label24 = new System.Windows.Forms.Label();
		this.te_ulke = new DevExpress.XtraEditors.TextEdit();
		this.label23 = new System.Windows.Forms.Label();
		this.te_il = new DevExpress.XtraEditors.TextEdit();
		this.label35 = new System.Windows.Forms.Label();
		this.te_ilce = new DevExpress.XtraEditors.TextEdit();
		this.label36 = new System.Windows.Forms.Label();
		this.te_adres = new DevExpress.XtraEditors.TextEdit();
		this.label37 = new System.Windows.Forms.Label();
		this.te_unvan = new DevExpress.XtraEditors.TextEdit();
		this.label38 = new System.Windows.Forms.Label();
		this.te_tarihgun_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_tarihgun = new DevExpress.XtraEditors.TextEdit();
		this.label34 = new System.Windows.Forms.Label();
		this.te_tarihay_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_tarihay = new DevExpress.XtraEditors.TextEdit();
		this.label33 = new System.Windows.Forms.Label();
		this.te_tutar_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_aciklama_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_tarihyil_uzunluk = new DevExpress.XtraEditors.TextEdit();
		this.te_tutar = new DevExpress.XtraEditors.TextEdit();
		this.label8 = new System.Windows.Forms.Label();
		this.te_aciklama = new DevExpress.XtraEditors.TextEdit();
		this.label30 = new System.Windows.Forms.Label();
		this.te_tarihyil = new DevExpress.XtraEditors.TextEdit();
		this.label29 = new System.Windows.Forms.Label();
		this.ce_ayrackarakterikullan = new DevExpress.XtraEditors.CheckEdit();
		this.te_ayrackarakteri = new DevExpress.XtraEditors.TextEdit();
		this.label18 = new System.Windows.Forms.Label();
		this.ce_bilgilerinbaslangicsatirikullan = new DevExpress.XtraEditors.CheckEdit();
		this.te_bilgilerinbaslangicsatiri = new DevExpress.XtraEditors.TextEdit();
		this.label17 = new System.Windows.Forms.Label();
		this.ce_alinacaksatirlarinbaslangickarakterikullan = new DevExpress.XtraEditors.CheckEdit();
		this.te_alinacaksatirlarinbaslangickarakteri = new DevExpress.XtraEditors.TextEdit();
		this.label16 = new System.Windows.Forms.Label();
		this.te_kullanici_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
		this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage5 = new DevExpress.XtraTab.XtraTabPage();
		this.label5 = new System.Windows.Forms.Label();
		this.mikro_disi_ek_bilgileri_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.xtraTabPage6 = new DevExpress.XtraTab.XtraTabPage();
		this.metin50_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin50_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin50_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin50_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin50_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label83 = new System.Windows.Forms.Label();
		this.metin49_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin49_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin49_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin49_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin49_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label82 = new System.Windows.Forms.Label();
		this.metin48_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin48_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin48_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin48_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin48_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label81 = new System.Windows.Forms.Label();
		this.metin47_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin47_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin47_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin47_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin47_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label80 = new System.Windows.Forms.Label();
		this.metin46_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin46_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin46_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin46_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin46_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label79 = new System.Windows.Forms.Label();
		this.metin45_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin45_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin45_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin45_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin45_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label78 = new System.Windows.Forms.Label();
		this.metin44_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin44_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin44_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin44_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin44_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label77 = new System.Windows.Forms.Label();
		this.metin43_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin43_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin43_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin43_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin43_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label76 = new System.Windows.Forms.Label();
		this.metin42_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin42_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin42_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin42_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin42_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label75 = new System.Windows.Forms.Label();
		this.metin41_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin41_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin41_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin41_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin41_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label74 = new System.Windows.Forms.Label();
		this.metin40_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin40_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin40_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin40_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin40_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label73 = new System.Windows.Forms.Label();
		this.metin39_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin39_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin39_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin39_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin39_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label72 = new System.Windows.Forms.Label();
		this.metin38_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin38_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin38_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin38_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin38_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label71 = new System.Windows.Forms.Label();
		this.metin37_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin37_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin37_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin37_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin37_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label70 = new System.Windows.Forms.Label();
		this.metin36_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin36_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin36_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin36_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin36_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label69 = new System.Windows.Forms.Label();
		this.metin35_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin35_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin35_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin35_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin35_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label68 = new System.Windows.Forms.Label();
		this.metin34_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin34_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin34_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin34_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin34_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label67 = new System.Windows.Forms.Label();
		this.metin33_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin33_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin33_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin33_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin33_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label66 = new System.Windows.Forms.Label();
		this.metin32_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin32_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin32_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin32_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin32_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label65 = new System.Windows.Forms.Label();
		this.metin31_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin31_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin31_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin31_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin31_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label64 = new System.Windows.Forms.Label();
		this.metin30_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin30_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin30_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin30_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin30_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label63 = new System.Windows.Forms.Label();
		this.metin29_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin29_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin29_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin29_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin29_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label62 = new System.Windows.Forms.Label();
		this.metin28_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin28_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin28_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin28_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin28_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label61 = new System.Windows.Forms.Label();
		this.metin27_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin27_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin27_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin27_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin27_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label60 = new System.Windows.Forms.Label();
		this.metin26_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin26_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin26_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin26_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin26_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label59 = new System.Windows.Forms.Label();
		this.metin25_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin25_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin25_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin25_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin25_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label58 = new System.Windows.Forms.Label();
		this.metin24_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin24_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin24_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin24_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin24_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label57 = new System.Windows.Forms.Label();
		this.metin23_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin23_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin23_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin23_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin23_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label56 = new System.Windows.Forms.Label();
		this.metin22_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin22_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin22_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin22_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin22_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label55 = new System.Windows.Forms.Label();
		this.metin21_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin21_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin21_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin21_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin21_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label54 = new System.Windows.Forms.Label();
		this.metin20_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin20_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin20_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin20_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin20_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label53 = new System.Windows.Forms.Label();
		this.metin19_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin19_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin19_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin19_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin19_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label52 = new System.Windows.Forms.Label();
		this.metin18_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin18_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin18_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin18_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin18_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label51 = new System.Windows.Forms.Label();
		this.metin17_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin17_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin17_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin17_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin17_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label50 = new System.Windows.Forms.Label();
		this.metin16_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin16_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin16_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin16_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin16_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label49 = new System.Windows.Forms.Label();
		this.metin15_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin15_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin15_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin15_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin15_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label48 = new System.Windows.Forms.Label();
		this.metin14_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin14_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin14_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin14_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin14_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label47 = new System.Windows.Forms.Label();
		this.metin13_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin13_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin13_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin13_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin13_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label46 = new System.Windows.Forms.Label();
		this.metin12_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin12_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin12_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin12_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin12_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label45 = new System.Windows.Forms.Label();
		this.metin11_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin11_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin11_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin11_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin11_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label31 = new System.Windows.Forms.Label();
		this.metin10_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin10_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin10_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin10_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin10_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label22 = new System.Windows.Forms.Label();
		this.metin9_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin9_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin9_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin9_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin9_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label21 = new System.Windows.Forms.Label();
		this.metin8_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin8_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin8_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin8_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin8_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label19 = new System.Windows.Forms.Label();
		this.metin7_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin7_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin7_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin7_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin7_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label13 = new System.Windows.Forms.Label();
		this.metin6_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin6_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin6_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin6_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin6_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label12 = new System.Windows.Forms.Label();
		this.metin5_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin5_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin5_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin5_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin5_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label11 = new System.Windows.Forms.Label();
		this.metin4_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin4_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin4_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin4_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin4_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label10 = new System.Windows.Forms.Label();
		this.metin3_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin3_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin3_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin3_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin3_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label9 = new System.Windows.Forms.Label();
		this.metin2_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.metin2_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin2_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin2_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label7 = new System.Windows.Forms.Label();
		this.metin1_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label6 = new System.Windows.Forms.Label();
		this.metin1_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.metin1_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.metin1_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.metin1_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label4 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.xtraTabPage4 = new DevExpress.XtraTab.XtraTabPage();
		this.label165 = new System.Windows.Forms.Label();
		this.label166 = new System.Windows.Forms.Label();
		this.dropbox10_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label167 = new System.Windows.Forms.Label();
		this.dropbox10_secenekler_veri = new DevExpress.XtraEditors.TextEdit();
		this.label168 = new System.Windows.Forms.Label();
		this.dropbox10_secenekler_yazi = new DevExpress.XtraEditors.TextEdit();
		this.label169 = new System.Windows.Forms.Label();
		this.dropbox10_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.dropbox10_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox10_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox10_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label170 = new System.Windows.Forms.Label();
		this.label171 = new System.Windows.Forms.Label();
		this.label172 = new System.Windows.Forms.Label();
		this.label173 = new System.Windows.Forms.Label();
		this.label156 = new System.Windows.Forms.Label();
		this.label157 = new System.Windows.Forms.Label();
		this.dropbox9_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label158 = new System.Windows.Forms.Label();
		this.dropbox9_secenekler_veri = new DevExpress.XtraEditors.TextEdit();
		this.label159 = new System.Windows.Forms.Label();
		this.dropbox9_secenekler_yazi = new DevExpress.XtraEditors.TextEdit();
		this.label160 = new System.Windows.Forms.Label();
		this.dropbox9_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.dropbox9_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox9_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox9_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label161 = new System.Windows.Forms.Label();
		this.label162 = new System.Windows.Forms.Label();
		this.label163 = new System.Windows.Forms.Label();
		this.label164 = new System.Windows.Forms.Label();
		this.label147 = new System.Windows.Forms.Label();
		this.label148 = new System.Windows.Forms.Label();
		this.dropbox8_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label149 = new System.Windows.Forms.Label();
		this.dropbox8_secenekler_veri = new DevExpress.XtraEditors.TextEdit();
		this.label150 = new System.Windows.Forms.Label();
		this.dropbox8_secenekler_yazi = new DevExpress.XtraEditors.TextEdit();
		this.label151 = new System.Windows.Forms.Label();
		this.dropbox8_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.dropbox8_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox8_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox8_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label152 = new System.Windows.Forms.Label();
		this.label153 = new System.Windows.Forms.Label();
		this.label154 = new System.Windows.Forms.Label();
		this.label155 = new System.Windows.Forms.Label();
		this.label138 = new System.Windows.Forms.Label();
		this.label139 = new System.Windows.Forms.Label();
		this.dropbox7_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label140 = new System.Windows.Forms.Label();
		this.dropbox7_secenekler_veri = new DevExpress.XtraEditors.TextEdit();
		this.label141 = new System.Windows.Forms.Label();
		this.dropbox7_secenekler_yazi = new DevExpress.XtraEditors.TextEdit();
		this.label142 = new System.Windows.Forms.Label();
		this.dropbox7_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.dropbox7_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox7_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox7_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label143 = new System.Windows.Forms.Label();
		this.label144 = new System.Windows.Forms.Label();
		this.label145 = new System.Windows.Forms.Label();
		this.label146 = new System.Windows.Forms.Label();
		this.label129 = new System.Windows.Forms.Label();
		this.label130 = new System.Windows.Forms.Label();
		this.dropbox6_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label131 = new System.Windows.Forms.Label();
		this.dropbox6_secenekler_veri = new DevExpress.XtraEditors.TextEdit();
		this.label132 = new System.Windows.Forms.Label();
		this.dropbox6_secenekler_yazi = new DevExpress.XtraEditors.TextEdit();
		this.label133 = new System.Windows.Forms.Label();
		this.dropbox6_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.dropbox6_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox6_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox6_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label134 = new System.Windows.Forms.Label();
		this.label135 = new System.Windows.Forms.Label();
		this.label136 = new System.Windows.Forms.Label();
		this.label137 = new System.Windows.Forms.Label();
		this.label120 = new System.Windows.Forms.Label();
		this.label121 = new System.Windows.Forms.Label();
		this.dropbox5_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label122 = new System.Windows.Forms.Label();
		this.dropbox5_secenekler_veri = new DevExpress.XtraEditors.TextEdit();
		this.label123 = new System.Windows.Forms.Label();
		this.dropbox5_secenekler_yazi = new DevExpress.XtraEditors.TextEdit();
		this.label124 = new System.Windows.Forms.Label();
		this.dropbox5_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.dropbox5_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox5_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox5_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label125 = new System.Windows.Forms.Label();
		this.label126 = new System.Windows.Forms.Label();
		this.label127 = new System.Windows.Forms.Label();
		this.label128 = new System.Windows.Forms.Label();
		this.label111 = new System.Windows.Forms.Label();
		this.label112 = new System.Windows.Forms.Label();
		this.dropbox4_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label113 = new System.Windows.Forms.Label();
		this.dropbox4_secenekler_veri = new DevExpress.XtraEditors.TextEdit();
		this.label114 = new System.Windows.Forms.Label();
		this.dropbox4_secenekler_yazi = new DevExpress.XtraEditors.TextEdit();
		this.label115 = new System.Windows.Forms.Label();
		this.dropbox4_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.dropbox4_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox4_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox4_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label116 = new System.Windows.Forms.Label();
		this.label117 = new System.Windows.Forms.Label();
		this.label118 = new System.Windows.Forms.Label();
		this.label119 = new System.Windows.Forms.Label();
		this.label102 = new System.Windows.Forms.Label();
		this.label103 = new System.Windows.Forms.Label();
		this.dropbox3_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label104 = new System.Windows.Forms.Label();
		this.dropbox3_secenekler_veri = new DevExpress.XtraEditors.TextEdit();
		this.label105 = new System.Windows.Forms.Label();
		this.dropbox3_secenekler_yazi = new DevExpress.XtraEditors.TextEdit();
		this.label106 = new System.Windows.Forms.Label();
		this.dropbox3_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.dropbox3_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox3_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox3_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label107 = new System.Windows.Forms.Label();
		this.label108 = new System.Windows.Forms.Label();
		this.label109 = new System.Windows.Forms.Label();
		this.label110 = new System.Windows.Forms.Label();
		this.label93 = new System.Windows.Forms.Label();
		this.label94 = new System.Windows.Forms.Label();
		this.dropbox2_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label95 = new System.Windows.Forms.Label();
		this.dropbox2_secenekler_veri = new DevExpress.XtraEditors.TextEdit();
		this.label96 = new System.Windows.Forms.Label();
		this.dropbox2_secenekler_yazi = new DevExpress.XtraEditors.TextEdit();
		this.label97 = new System.Windows.Forms.Label();
		this.dropbox2_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.dropbox2_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox2_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label98 = new System.Windows.Forms.Label();
		this.label99 = new System.Windows.Forms.Label();
		this.label100 = new System.Windows.Forms.Label();
		this.label101 = new System.Windows.Forms.Label();
		this.label92 = new System.Windows.Forms.Label();
		this.label91 = new System.Windows.Forms.Label();
		this.dropbox1_varsayilan_deger = new DevExpress.XtraEditors.TextEdit();
		this.label90 = new System.Windows.Forms.Label();
		this.dropbox1_secenekler_veri = new DevExpress.XtraEditors.TextEdit();
		this.label89 = new System.Windows.Forms.Label();
		this.dropbox1_secenekler_yazi = new DevExpress.XtraEditors.TextEdit();
		this.label84 = new System.Windows.Forms.Label();
		this.dropbox1_zorunlu = new DevExpress.XtraEditors.CheckEdit();
		this.dropbox1_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox1_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.dropbox1_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label85 = new System.Windows.Forms.Label();
		this.label86 = new System.Windows.Forms.Label();
		this.label87 = new System.Windows.Forms.Label();
		this.label88 = new System.Windows.Forms.Label();
		this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
		this.checkbox10_isaretsiz_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label228 = new System.Windows.Forms.Label();
		this.checkbox10_isaretli_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label229 = new System.Windows.Forms.Label();
		this.checkbox10_varsayilan_deger = new DevExpress.XtraEditors.CheckEdit();
		this.checkbox10_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox10_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox10_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label230 = new System.Windows.Forms.Label();
		this.label231 = new System.Windows.Forms.Label();
		this.label232 = new System.Windows.Forms.Label();
		this.label233 = new System.Windows.Forms.Label();
		this.checkbox9_isaretsiz_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label222 = new System.Windows.Forms.Label();
		this.checkbox9_isaretli_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label223 = new System.Windows.Forms.Label();
		this.checkbox9_varsayilan_deger = new DevExpress.XtraEditors.CheckEdit();
		this.checkbox9_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox9_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox9_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label224 = new System.Windows.Forms.Label();
		this.label225 = new System.Windows.Forms.Label();
		this.label226 = new System.Windows.Forms.Label();
		this.label227 = new System.Windows.Forms.Label();
		this.checkbox8_isaretsiz_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label216 = new System.Windows.Forms.Label();
		this.checkbox8_isaretli_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label217 = new System.Windows.Forms.Label();
		this.checkbox8_varsayilan_deger = new DevExpress.XtraEditors.CheckEdit();
		this.checkbox8_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox8_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox8_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label218 = new System.Windows.Forms.Label();
		this.label219 = new System.Windows.Forms.Label();
		this.label220 = new System.Windows.Forms.Label();
		this.label221 = new System.Windows.Forms.Label();
		this.checkbox7_isaretsiz_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label210 = new System.Windows.Forms.Label();
		this.checkbox7_isaretli_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label211 = new System.Windows.Forms.Label();
		this.checkbox7_varsayilan_deger = new DevExpress.XtraEditors.CheckEdit();
		this.checkbox7_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox7_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox7_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label212 = new System.Windows.Forms.Label();
		this.label213 = new System.Windows.Forms.Label();
		this.label214 = new System.Windows.Forms.Label();
		this.label215 = new System.Windows.Forms.Label();
		this.checkbox6_isaretsiz_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label204 = new System.Windows.Forms.Label();
		this.checkbox6_isaretli_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label205 = new System.Windows.Forms.Label();
		this.checkbox6_varsayilan_deger = new DevExpress.XtraEditors.CheckEdit();
		this.checkbox6_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox6_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox6_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label206 = new System.Windows.Forms.Label();
		this.label207 = new System.Windows.Forms.Label();
		this.label208 = new System.Windows.Forms.Label();
		this.label209 = new System.Windows.Forms.Label();
		this.checkbox5_isaretsiz_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label198 = new System.Windows.Forms.Label();
		this.checkbox5_isaretli_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label199 = new System.Windows.Forms.Label();
		this.checkbox5_varsayilan_deger = new DevExpress.XtraEditors.CheckEdit();
		this.checkbox5_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox5_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox5_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label200 = new System.Windows.Forms.Label();
		this.label201 = new System.Windows.Forms.Label();
		this.label202 = new System.Windows.Forms.Label();
		this.label203 = new System.Windows.Forms.Label();
		this.checkbox4_isaretsiz_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label192 = new System.Windows.Forms.Label();
		this.checkbox4_isaretli_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label193 = new System.Windows.Forms.Label();
		this.checkbox4_varsayilan_deger = new DevExpress.XtraEditors.CheckEdit();
		this.checkbox4_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox4_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox4_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label194 = new System.Windows.Forms.Label();
		this.label195 = new System.Windows.Forms.Label();
		this.label196 = new System.Windows.Forms.Label();
		this.label197 = new System.Windows.Forms.Label();
		this.checkbox3_isaretsiz_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label186 = new System.Windows.Forms.Label();
		this.checkbox3_isaretli_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label187 = new System.Windows.Forms.Label();
		this.checkbox3_varsayilan_deger = new DevExpress.XtraEditors.CheckEdit();
		this.checkbox3_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox3_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox3_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label188 = new System.Windows.Forms.Label();
		this.label189 = new System.Windows.Forms.Label();
		this.label190 = new System.Windows.Forms.Label();
		this.label191 = new System.Windows.Forms.Label();
		this.checkbox2_isaretsiz_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label180 = new System.Windows.Forms.Label();
		this.checkbox2_isaretli_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label181 = new System.Windows.Forms.Label();
		this.checkbox2_varsayilan_deger = new DevExpress.XtraEditors.CheckEdit();
		this.checkbox2_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox2_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label182 = new System.Windows.Forms.Label();
		this.label183 = new System.Windows.Forms.Label();
		this.label184 = new System.Windows.Forms.Label();
		this.label185 = new System.Windows.Forms.Label();
		this.checkbox1_isaretsiz_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label179 = new System.Windows.Forms.Label();
		this.checkbox1_isaretli_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label178 = new System.Windows.Forms.Label();
		this.checkbox1_varsayilan_deger = new DevExpress.XtraEditors.CheckEdit();
		this.checkbox1_uzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox1_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.checkbox1_gorunen_adi = new DevExpress.XtraEditors.TextEdit();
		this.label174 = new System.Windows.Forms.Label();
		this.label175 = new System.Windows.Forms.Label();
		this.label176 = new System.Windows.Forms.Label();
		this.label177 = new System.Windows.Forms.Label();
		this.te_eklenecek_parametre_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl17 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).BeginInit();
		this.tc_parametreler.SuspendLayout();
		this.xtraTabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.te_bakiye_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_bakiye.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_mahalle_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_mahalle.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_unvan2_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_unvan2.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_bankahesapno_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tckimlikvergino_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_eposta_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_telefon_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_postakodu_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_ulke_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_il_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_ilce_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_adres_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_unvan_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_bankahesapno.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tckimlikvergino.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_eposta.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_telefon.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_postakodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_ulke.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_il.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_ilce.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_adres.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_unvan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihgun_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihgun.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihay_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihay.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tutar_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_aciklama_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihyil_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tutar.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_aciklama.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihyil.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ce_ayrackarakterikullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_ayrackarakteri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ce_bilgilerinbaslangicsatirikullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_bilgilerinbaslangicsatiri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ce_alinacaksatirlarinbaslangickarakterikullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_alinacaksatirlarinbaslangickarakteri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		this.xtraTabPage1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl1).BeginInit();
		this.xtraTabControl1.SuspendLayout();
		this.xtraTabPage5.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.mikro_disi_ek_bilgileri_kullan.Properties).BeginInit();
		this.xtraTabPage6.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.metin50_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin50_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin50_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin50_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin50_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin49_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin49_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin49_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin49_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin49_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin48_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin48_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin48_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin48_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin48_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin47_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin47_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin47_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin47_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin47_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin46_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin46_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin46_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin46_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin46_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin45_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin45_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin45_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin45_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin45_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin44_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin44_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin44_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin44_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin44_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin43_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin43_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin43_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin43_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin43_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin42_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin42_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin42_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin42_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin42_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin41_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin41_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin41_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin41_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin41_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin40_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin40_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin40_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin40_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin40_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin39_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin39_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin39_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin39_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin39_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin38_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin38_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin38_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin38_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin38_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin37_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin37_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin37_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin37_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin37_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin36_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin36_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin36_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin36_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin36_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin35_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin35_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin35_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin35_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin35_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin34_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin34_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin34_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin34_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin34_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin33_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin33_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin33_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin33_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin33_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin32_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin32_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin32_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin32_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin32_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin31_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin31_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin31_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin31_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin31_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin30_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin30_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin30_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin30_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin30_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin29_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin29_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin29_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin29_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin29_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin28_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin28_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin28_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin28_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin28_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin27_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin27_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin27_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin27_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin27_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin26_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin26_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin26_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin26_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin26_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin25_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin25_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin25_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin25_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin25_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin24_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin24_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin24_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin24_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin24_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin23_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin23_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin23_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin23_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin23_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin22_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin22_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin22_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin22_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin22_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin21_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin21_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin21_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin21_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin21_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin20_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin20_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin20_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin20_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin20_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin19_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin19_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin19_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin19_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin19_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin18_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin18_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin18_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin18_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin18_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin17_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin17_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin17_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin17_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin17_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin16_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin16_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin16_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin16_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin16_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin15_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin15_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin15_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin15_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin15_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin14_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin14_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin14_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin14_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin14_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin13_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin13_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin13_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin13_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin13_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin12_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin12_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin12_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin12_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin12_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin11_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin11_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin11_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin11_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin11_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin10_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin10_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin10_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin10_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin10_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin9_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin9_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin9_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin9_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin9_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin8_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin8_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin8_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin8_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin8_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin7_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin7_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin7_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin7_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin7_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin6_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin6_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin6_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin6_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin6_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin5_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin5_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin5_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin5_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin5_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin4_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin4_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin4_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin4_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin4_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin3_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin3_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin3_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin3_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin3_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin2_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin2_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin2_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin2_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin1_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin1_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin1_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin1_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.metin1_gorunen_adi.Properties).BeginInit();
		this.xtraTabPage4.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_secenekler_veri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_secenekler_yazi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_secenekler_veri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_secenekler_yazi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_secenekler_veri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_secenekler_yazi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_secenekler_veri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_secenekler_yazi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_secenekler_veri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_secenekler_yazi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_secenekler_veri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_secenekler_yazi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_secenekler_veri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_secenekler_yazi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_secenekler_veri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_secenekler_yazi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_secenekler_veri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_secenekler_yazi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_secenekler_veri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_secenekler_yazi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_zorunlu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_gorunen_adi.Properties).BeginInit();
		this.xtraTabPage2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_isaretsiz_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_isaretli_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_isaretsiz_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_isaretli_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_isaretsiz_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_isaretli_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_isaretsiz_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_isaretli_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_isaretsiz_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_isaretli_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_isaretsiz_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_isaretli_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_isaretsiz_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_isaretli_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_isaretsiz_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_isaretli_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_isaretsiz_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_isaretli_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_isaretsiz_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_isaretli_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_varsayilan_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_uzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_gorunen_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_eklenecek_parametre_adi.Properties).BeginInit();
		base.SuspendLayout();
		this.lb_kullanicilar.Location = new System.Drawing.Point(12, 35);
		this.lb_kullanicilar.Name = "lb_kullanicilar";
		this.lb_kullanicilar.Size = new System.Drawing.Size(153, 221);
		this.lb_kullanicilar.TabIndex = 5;
		this.lb_kullanicilar.SelectedValueChanged += new System.EventHandler(lb_kullanicilar_SelectedValueChanged);
		this.sb_kullanici_ekle.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_kullanici_ekle.Appearance.Options.UseFont = true;
		this.sb_kullanici_ekle.Location = new System.Drawing.Point(13, 378);
		this.sb_kullanici_ekle.Name = "sb_kullanici_ekle";
		this.sb_kullanici_ekle.Size = new System.Drawing.Size(153, 23);
		this.sb_kullanici_ekle.TabIndex = 6;
		this.sb_kullanici_ekle.Text = "Genel Parametre Ekle";
		this.sb_kullanici_ekle.Click += new System.EventHandler(sb_kullanici_ekle_Click);
		this.sb_kullanici_sil.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_kullanici_sil.Appearance.Options.UseFont = true;
		this.sb_kullanici_sil.Enabled = false;
		this.sb_kullanici_sil.Location = new System.Drawing.Point(13, 262);
		this.sb_kullanici_sil.Name = "sb_kullanici_sil";
		this.sb_kullanici_sil.Size = new System.Drawing.Size(153, 23);
		this.sb_kullanici_sil.TabIndex = 7;
		this.sb_kullanici_sil.Text = "Genel Parametre Sil";
		this.sb_kullanici_sil.Click += new System.EventHandler(sb_kullanici_sil_Click);
		this.sb_ayarlari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlari_kaydet.Appearance.Options.UseFont = true;
		this.sb_ayarlari_kaydet.Enabled = false;
		this.sb_ayarlari_kaydet.Location = new System.Drawing.Point(785, 533);
		this.sb_ayarlari_kaydet.Name = "sb_ayarlari_kaydet";
		this.sb_ayarlari_kaydet.Size = new System.Drawing.Size(132, 23);
		this.sb_ayarlari_kaydet.TabIndex = 8;
		this.sb_ayarlari_kaydet.Text = "Ayarları kaydet";
		this.sb_ayarlari_kaydet.Click += new System.EventHandler(sb_ayarlari_kaydet_Click);
		this.tc_parametreler.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tc_parametreler.Location = new System.Drawing.Point(171, 35);
		this.tc_parametreler.Name = "tc_parametreler";
		this.tc_parametreler.SelectedTabPage = this.xtraTabPage3;
		this.tc_parametreler.Size = new System.Drawing.Size(746, 480);
		this.tc_parametreler.TabIndex = 0;
		this.tc_parametreler.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[2] { this.xtraTabPage3, this.xtraTabPage1 });
		this.xtraTabPage3.Controls.Add(this.label44);
		this.xtraTabPage3.Controls.Add(this.label43);
		this.xtraTabPage3.Controls.Add(this.te_bakiye_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_bakiye);
		this.xtraTabPage3.Controls.Add(this.label42);
		this.xtraTabPage3.Controls.Add(this.label41);
		this.xtraTabPage3.Controls.Add(this.label39);
		this.xtraTabPage3.Controls.Add(this.label40);
		this.xtraTabPage3.Controls.Add(this.te_mahalle_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_mahalle);
		this.xtraTabPage3.Controls.Add(this.label14);
		this.xtraTabPage3.Controls.Add(this.te_unvan2_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_unvan2);
		this.xtraTabPage3.Controls.Add(this.label15);
		this.xtraTabPage3.Controls.Add(this.label32);
		this.xtraTabPage3.Controls.Add(this.te_bankahesapno_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_tckimlikvergino_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_eposta_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_telefon_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_postakodu_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_ulke_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_il_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_ilce_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_adres_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_unvan_uzunluk);
		this.xtraTabPage3.Controls.Add(this.label20);
		this.xtraTabPage3.Controls.Add(this.te_bankahesapno);
		this.xtraTabPage3.Controls.Add(this.label28);
		this.xtraTabPage3.Controls.Add(this.te_tckimlikvergino);
		this.xtraTabPage3.Controls.Add(this.label27);
		this.xtraTabPage3.Controls.Add(this.te_eposta);
		this.xtraTabPage3.Controls.Add(this.label26);
		this.xtraTabPage3.Controls.Add(this.te_telefon);
		this.xtraTabPage3.Controls.Add(this.label25);
		this.xtraTabPage3.Controls.Add(this.te_postakodu);
		this.xtraTabPage3.Controls.Add(this.label24);
		this.xtraTabPage3.Controls.Add(this.te_ulke);
		this.xtraTabPage3.Controls.Add(this.label23);
		this.xtraTabPage3.Controls.Add(this.te_il);
		this.xtraTabPage3.Controls.Add(this.label35);
		this.xtraTabPage3.Controls.Add(this.te_ilce);
		this.xtraTabPage3.Controls.Add(this.label36);
		this.xtraTabPage3.Controls.Add(this.te_adres);
		this.xtraTabPage3.Controls.Add(this.label37);
		this.xtraTabPage3.Controls.Add(this.te_unvan);
		this.xtraTabPage3.Controls.Add(this.label38);
		this.xtraTabPage3.Controls.Add(this.te_tarihgun_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_tarihgun);
		this.xtraTabPage3.Controls.Add(this.label34);
		this.xtraTabPage3.Controls.Add(this.te_tarihay_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_tarihay);
		this.xtraTabPage3.Controls.Add(this.label33);
		this.xtraTabPage3.Controls.Add(this.te_tutar_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_aciklama_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_tarihyil_uzunluk);
		this.xtraTabPage3.Controls.Add(this.te_tutar);
		this.xtraTabPage3.Controls.Add(this.label8);
		this.xtraTabPage3.Controls.Add(this.te_aciklama);
		this.xtraTabPage3.Controls.Add(this.label30);
		this.xtraTabPage3.Controls.Add(this.te_tarihyil);
		this.xtraTabPage3.Controls.Add(this.label29);
		this.xtraTabPage3.Controls.Add(this.ce_ayrackarakterikullan);
		this.xtraTabPage3.Controls.Add(this.te_ayrackarakteri);
		this.xtraTabPage3.Controls.Add(this.label18);
		this.xtraTabPage3.Controls.Add(this.ce_bilgilerinbaslangicsatirikullan);
		this.xtraTabPage3.Controls.Add(this.te_bilgilerinbaslangicsatiri);
		this.xtraTabPage3.Controls.Add(this.label17);
		this.xtraTabPage3.Controls.Add(this.ce_alinacaksatirlarinbaslangickarakterikullan);
		this.xtraTabPage3.Controls.Add(this.te_alinacaksatirlarinbaslangickarakteri);
		this.xtraTabPage3.Controls.Add(this.label16);
		this.xtraTabPage3.Controls.Add(this.te_kullanici_adi);
		this.xtraTabPage3.Controls.Add(this.labelControl1);
		this.xtraTabPage3.Name = "xtraTabPage3";
		this.xtraTabPage3.Size = new System.Drawing.Size(740, 452);
		this.xtraTabPage3.Text = "Genel";
		this.label44.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label44.Location = new System.Drawing.Point(562, 30);
		this.label44.Name = "label44";
		this.label44.Size = new System.Drawing.Size(146, 19);
		this.label44.TabIndex = 150;
		this.label44.Text = "Zorunlu olmayan alanlar";
		this.label44.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label43.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label43.Location = new System.Drawing.Point(152, 190);
		this.label43.Name = "label43";
		this.label43.Size = new System.Drawing.Size(114, 19);
		this.label43.TabIndex = 149;
		this.label43.Text = "Zorunlu alanlar";
		this.label43.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.te_bakiye_uzunluk.Location = new System.Drawing.Point(663, 75);
		this.te_bakiye_uzunluk.Name = "te_bakiye_uzunluk";
		this.te_bakiye_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_bakiye_uzunluk.TabIndex = 147;
		this.te_bakiye.Location = new System.Drawing.Point(561, 75);
		this.te_bakiye.Name = "te_bakiye";
		this.te_bakiye.Size = new System.Drawing.Size(96, 20);
		this.te_bakiye.TabIndex = 146;
		this.label42.Location = new System.Drawing.Point(435, 75);
		this.label42.Name = "label42";
		this.label42.Size = new System.Drawing.Size(120, 19);
		this.label42.TabIndex = 148;
		this.label42.Text = "Bakiye :";
		this.label42.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label41.Location = new System.Drawing.Point(291, 286);
		this.label41.Name = "label41";
		this.label41.Size = new System.Drawing.Size(129, 63);
		this.label41.TabIndex = 145;
		this.label41.Text = "Ayraç kullanıldığı zaman sadece yıl bölümü dikkate alınır. GG.AA.YYYY formatında olmalıdır.";
		this.label39.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label39.Location = new System.Drawing.Point(228, 209);
		this.label39.Name = "label39";
		this.label39.Size = new System.Drawing.Size(54, 19);
		this.label39.TabIndex = 144;
		this.label39.Text = "Uzunluk";
		this.label39.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label40.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label40.Location = new System.Drawing.Point(126, 209);
		this.label40.Name = "label40";
		this.label40.Size = new System.Drawing.Size(96, 19);
		this.label40.TabIndex = 143;
		this.label40.Text = "Başlangıç/Sıra";
		this.label40.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.te_mahalle_uzunluk.Location = new System.Drawing.Point(663, 179);
		this.te_mahalle_uzunluk.Name = "te_mahalle_uzunluk";
		this.te_mahalle_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_mahalle_uzunluk.TabIndex = 112;
		this.te_mahalle.Location = new System.Drawing.Point(561, 179);
		this.te_mahalle.Name = "te_mahalle";
		this.te_mahalle.Size = new System.Drawing.Size(96, 20);
		this.te_mahalle.TabIndex = 111;
		this.label14.Location = new System.Drawing.Point(435, 179);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(120, 19);
		this.label14.TabIndex = 142;
		this.label14.Text = "Mahalle :";
		this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_unvan2_uzunluk.Location = new System.Drawing.Point(663, 127);
		this.te_unvan2_uzunluk.Name = "te_unvan2_uzunluk";
		this.te_unvan2_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_unvan2_uzunluk.TabIndex = 108;
		this.te_unvan2.Location = new System.Drawing.Point(561, 127);
		this.te_unvan2.Name = "te_unvan2";
		this.te_unvan2.Size = new System.Drawing.Size(96, 20);
		this.te_unvan2.TabIndex = 107;
		this.label15.Location = new System.Drawing.Point(435, 127);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(120, 19);
		this.label15.TabIndex = 141;
		this.label15.Text = "Ünvan2 (Soyadı) :";
		this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label32.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label32.Location = new System.Drawing.Point(663, 52);
		this.label32.Name = "label32";
		this.label32.Size = new System.Drawing.Size(54, 19);
		this.label32.TabIndex = 140;
		this.label32.Text = "Uzunluk";
		this.label32.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.te_bankahesapno_uzunluk.Location = new System.Drawing.Point(663, 387);
		this.te_bankahesapno_uzunluk.Name = "te_bankahesapno_uzunluk";
		this.te_bankahesapno_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_bankahesapno_uzunluk.TabIndex = 128;
		this.te_tckimlikvergino_uzunluk.Location = new System.Drawing.Point(663, 361);
		this.te_tckimlikvergino_uzunluk.Name = "te_tckimlikvergino_uzunluk";
		this.te_tckimlikvergino_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_tckimlikvergino_uzunluk.TabIndex = 126;
		this.te_eposta_uzunluk.Location = new System.Drawing.Point(663, 335);
		this.te_eposta_uzunluk.Name = "te_eposta_uzunluk";
		this.te_eposta_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_eposta_uzunluk.TabIndex = 124;
		this.te_telefon_uzunluk.Location = new System.Drawing.Point(663, 309);
		this.te_telefon_uzunluk.Name = "te_telefon_uzunluk";
		this.te_telefon_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_telefon_uzunluk.TabIndex = 122;
		this.te_postakodu_uzunluk.Location = new System.Drawing.Point(663, 283);
		this.te_postakodu_uzunluk.Name = "te_postakodu_uzunluk";
		this.te_postakodu_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_postakodu_uzunluk.TabIndex = 120;
		this.te_ulke_uzunluk.Location = new System.Drawing.Point(663, 257);
		this.te_ulke_uzunluk.Name = "te_ulke_uzunluk";
		this.te_ulke_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_ulke_uzunluk.TabIndex = 118;
		this.te_il_uzunluk.Location = new System.Drawing.Point(663, 231);
		this.te_il_uzunluk.Name = "te_il_uzunluk";
		this.te_il_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_il_uzunluk.TabIndex = 116;
		this.te_ilce_uzunluk.Location = new System.Drawing.Point(663, 205);
		this.te_ilce_uzunluk.Name = "te_ilce_uzunluk";
		this.te_ilce_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_ilce_uzunluk.TabIndex = 114;
		this.te_adres_uzunluk.Location = new System.Drawing.Point(663, 153);
		this.te_adres_uzunluk.Name = "te_adres_uzunluk";
		this.te_adres_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_adres_uzunluk.TabIndex = 110;
		this.te_unvan_uzunluk.Location = new System.Drawing.Point(663, 101);
		this.te_unvan_uzunluk.Name = "te_unvan_uzunluk";
		this.te_unvan_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_unvan_uzunluk.TabIndex = 106;
		this.label20.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label20.Location = new System.Drawing.Point(561, 52);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(96, 19);
		this.label20.TabIndex = 139;
		this.label20.Text = "Başlangıç/Sıra";
		this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.te_bankahesapno.Location = new System.Drawing.Point(561, 387);
		this.te_bankahesapno.Name = "te_bankahesapno";
		this.te_bankahesapno.Size = new System.Drawing.Size(96, 20);
		this.te_bankahesapno.TabIndex = 127;
		this.label28.Location = new System.Drawing.Point(435, 387);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(120, 19);
		this.label28.TabIndex = 138;
		this.label28.Text = "Banka hesap no :";
		this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_tckimlikvergino.Location = new System.Drawing.Point(561, 361);
		this.te_tckimlikvergino.Name = "te_tckimlikvergino";
		this.te_tckimlikvergino.Size = new System.Drawing.Size(96, 20);
		this.te_tckimlikvergino.TabIndex = 125;
		this.label27.Location = new System.Drawing.Point(435, 361);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(120, 19);
		this.label27.TabIndex = 137;
		this.label27.Text = "Tc kimlik/Vergi no :";
		this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_eposta.Location = new System.Drawing.Point(561, 335);
		this.te_eposta.Name = "te_eposta";
		this.te_eposta.Size = new System.Drawing.Size(96, 20);
		this.te_eposta.TabIndex = 123;
		this.label26.Location = new System.Drawing.Point(435, 335);
		this.label26.Name = "label26";
		this.label26.Size = new System.Drawing.Size(120, 19);
		this.label26.TabIndex = 136;
		this.label26.Text = "E-posta :";
		this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_telefon.Location = new System.Drawing.Point(561, 309);
		this.te_telefon.Name = "te_telefon";
		this.te_telefon.Size = new System.Drawing.Size(96, 20);
		this.te_telefon.TabIndex = 121;
		this.label25.Location = new System.Drawing.Point(435, 309);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(120, 19);
		this.label25.TabIndex = 135;
		this.label25.Text = "Telefon :";
		this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_postakodu.Location = new System.Drawing.Point(561, 283);
		this.te_postakodu.Name = "te_postakodu";
		this.te_postakodu.Size = new System.Drawing.Size(96, 20);
		this.te_postakodu.TabIndex = 119;
		this.label24.Location = new System.Drawing.Point(435, 283);
		this.label24.Name = "label24";
		this.label24.Size = new System.Drawing.Size(120, 19);
		this.label24.TabIndex = 134;
		this.label24.Text = "Posta kodu :";
		this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_ulke.Location = new System.Drawing.Point(561, 257);
		this.te_ulke.Name = "te_ulke";
		this.te_ulke.Size = new System.Drawing.Size(96, 20);
		this.te_ulke.TabIndex = 117;
		this.label23.Location = new System.Drawing.Point(435, 257);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(120, 19);
		this.label23.TabIndex = 133;
		this.label23.Text = "Ülke :";
		this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_il.Location = new System.Drawing.Point(561, 231);
		this.te_il.Name = "te_il";
		this.te_il.Size = new System.Drawing.Size(96, 20);
		this.te_il.TabIndex = 115;
		this.label35.Location = new System.Drawing.Point(435, 231);
		this.label35.Name = "label35";
		this.label35.Size = new System.Drawing.Size(120, 19);
		this.label35.TabIndex = 132;
		this.label35.Text = "İl :";
		this.label35.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_ilce.Location = new System.Drawing.Point(561, 205);
		this.te_ilce.Name = "te_ilce";
		this.te_ilce.Size = new System.Drawing.Size(96, 20);
		this.te_ilce.TabIndex = 113;
		this.label36.Location = new System.Drawing.Point(435, 205);
		this.label36.Name = "label36";
		this.label36.Size = new System.Drawing.Size(120, 19);
		this.label36.TabIndex = 131;
		this.label36.Text = "İlçe :";
		this.label36.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_adres.Location = new System.Drawing.Point(561, 153);
		this.te_adres.Name = "te_adres";
		this.te_adres.Size = new System.Drawing.Size(96, 20);
		this.te_adres.TabIndex = 109;
		this.label37.Location = new System.Drawing.Point(435, 153);
		this.label37.Name = "label37";
		this.label37.Size = new System.Drawing.Size(120, 19);
		this.label37.TabIndex = 130;
		this.label37.Text = "Adres :";
		this.label37.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_unvan.Location = new System.Drawing.Point(561, 101);
		this.te_unvan.Name = "te_unvan";
		this.te_unvan.Size = new System.Drawing.Size(96, 20);
		this.te_unvan.TabIndex = 105;
		this.label38.Location = new System.Drawing.Point(435, 101);
		this.label38.Name = "label38";
		this.label38.Size = new System.Drawing.Size(120, 19);
		this.label38.TabIndex = 129;
		this.label38.Text = "Ünvan (Adı) :";
		this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_tarihgun_uzunluk.Location = new System.Drawing.Point(231, 335);
		this.te_tarihgun_uzunluk.Name = "te_tarihgun_uzunluk";
		this.te_tarihgun_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_tarihgun_uzunluk.TabIndex = 95;
		this.te_tarihgun.Location = new System.Drawing.Point(129, 335);
		this.te_tarihgun.Name = "te_tarihgun";
		this.te_tarihgun.Size = new System.Drawing.Size(96, 20);
		this.te_tarihgun.TabIndex = 94;
		this.label34.Location = new System.Drawing.Point(40, 335);
		this.label34.Name = "label34";
		this.label34.Size = new System.Drawing.Size(83, 19);
		this.label34.TabIndex = 104;
		this.label34.Text = "Tarih gün :";
		this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_tarihay_uzunluk.Location = new System.Drawing.Point(231, 309);
		this.te_tarihay_uzunluk.Name = "te_tarihay_uzunluk";
		this.te_tarihay_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_tarihay_uzunluk.TabIndex = 93;
		this.te_tarihay.Location = new System.Drawing.Point(129, 309);
		this.te_tarihay.Name = "te_tarihay";
		this.te_tarihay.Size = new System.Drawing.Size(96, 20);
		this.te_tarihay.TabIndex = 92;
		this.label33.Location = new System.Drawing.Point(40, 309);
		this.label33.Name = "label33";
		this.label33.Size = new System.Drawing.Size(83, 19);
		this.label33.TabIndex = 103;
		this.label33.Text = "Tarih ay :";
		this.label33.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_tutar_uzunluk.Location = new System.Drawing.Point(231, 257);
		this.te_tutar_uzunluk.Name = "te_tutar_uzunluk";
		this.te_tutar_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_tutar_uzunluk.TabIndex = 99;
		this.te_aciklama_uzunluk.Location = new System.Drawing.Point(231, 231);
		this.te_aciklama_uzunluk.Name = "te_aciklama_uzunluk";
		this.te_aciklama_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_aciklama_uzunluk.TabIndex = 97;
		this.te_tarihyil_uzunluk.Location = new System.Drawing.Point(231, 283);
		this.te_tarihyil_uzunluk.Name = "te_tarihyil_uzunluk";
		this.te_tarihyil_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.te_tarihyil_uzunluk.TabIndex = 91;
		this.te_tutar.Location = new System.Drawing.Point(129, 257);
		this.te_tutar.Name = "te_tutar";
		this.te_tutar.Size = new System.Drawing.Size(96, 20);
		this.te_tutar.TabIndex = 98;
		this.label8.Location = new System.Drawing.Point(40, 257);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(83, 19);
		this.label8.TabIndex = 102;
		this.label8.Text = "Tutar :";
		this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_aciklama.Location = new System.Drawing.Point(129, 231);
		this.te_aciklama.Name = "te_aciklama";
		this.te_aciklama.Size = new System.Drawing.Size(96, 20);
		this.te_aciklama.TabIndex = 96;
		this.label30.Location = new System.Drawing.Point(40, 231);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(83, 19);
		this.label30.TabIndex = 101;
		this.label30.Text = "Açıklama :";
		this.label30.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_tarihyil.Location = new System.Drawing.Point(129, 283);
		this.te_tarihyil.Name = "te_tarihyil";
		this.te_tarihyil.Size = new System.Drawing.Size(96, 20);
		this.te_tarihyil.TabIndex = 90;
		this.label29.Location = new System.Drawing.Point(40, 283);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(83, 19);
		this.label29.TabIndex = 100;
		this.label29.Text = "Tarih yıl :";
		this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.ce_ayrackarakterikullan.Location = new System.Drawing.Point(268, 132);
		this.ce_ayrackarakterikullan.Name = "ce_ayrackarakterikullan";
		this.ce_ayrackarakterikullan.Properties.Caption = "kullan";
		this.ce_ayrackarakterikullan.Size = new System.Drawing.Size(58, 19);
		this.ce_ayrackarakterikullan.TabIndex = 48;
		this.te_ayrackarakteri.Location = new System.Drawing.Point(230, 131);
		this.te_ayrackarakteri.Name = "te_ayrackarakteri";
		this.te_ayrackarakteri.Size = new System.Drawing.Size(32, 20);
		this.te_ayrackarakteri.TabIndex = 47;
		this.label18.Location = new System.Drawing.Point(33, 131);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(192, 19);
		this.label18.TabIndex = 51;
		this.label18.Text = "Ayraç karakteri :";
		this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.ce_bilgilerinbaslangicsatirikullan.Location = new System.Drawing.Point(268, 106);
		this.ce_bilgilerinbaslangicsatirikullan.Name = "ce_bilgilerinbaslangicsatirikullan";
		this.ce_bilgilerinbaslangicsatirikullan.Properties.Caption = "kullan";
		this.ce_bilgilerinbaslangicsatirikullan.Size = new System.Drawing.Size(58, 19);
		this.ce_bilgilerinbaslangicsatirikullan.TabIndex = 46;
		this.te_bilgilerinbaslangicsatiri.Location = new System.Drawing.Point(230, 105);
		this.te_bilgilerinbaslangicsatiri.Name = "te_bilgilerinbaslangicsatiri";
		this.te_bilgilerinbaslangicsatiri.Size = new System.Drawing.Size(32, 20);
		this.te_bilgilerinbaslangicsatiri.TabIndex = 45;
		this.label17.Location = new System.Drawing.Point(33, 105);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(192, 19);
		this.label17.TabIndex = 50;
		this.label17.Text = "Bilgilerin başlangıç satırı :";
		this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.ce_alinacaksatirlarinbaslangickarakterikullan.Location = new System.Drawing.Point(268, 82);
		this.ce_alinacaksatirlarinbaslangickarakterikullan.Name = "ce_alinacaksatirlarinbaslangickarakterikullan";
		this.ce_alinacaksatirlarinbaslangickarakterikullan.Properties.Caption = "kullan";
		this.ce_alinacaksatirlarinbaslangickarakterikullan.Size = new System.Drawing.Size(58, 19);
		this.ce_alinacaksatirlarinbaslangickarakterikullan.TabIndex = 44;
		this.te_alinacaksatirlarinbaslangickarakteri.Location = new System.Drawing.Point(230, 81);
		this.te_alinacaksatirlarinbaslangickarakteri.Name = "te_alinacaksatirlarinbaslangickarakteri";
		this.te_alinacaksatirlarinbaslangickarakteri.Size = new System.Drawing.Size(32, 20);
		this.te_alinacaksatirlarinbaslangickarakteri.TabIndex = 43;
		this.label16.Location = new System.Drawing.Point(33, 81);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(192, 19);
		this.label16.TabIndex = 49;
		this.label16.Text = "Alınacak satırların başlangıç karakteri :";
		this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_kullanici_adi.Enabled = false;
		this.te_kullanici_adi.Location = new System.Drawing.Point(228, 49);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.te_kullanici_adi.Properties.Appearance.Options.UseForeColor = true;
		this.te_kullanici_adi.Size = new System.Drawing.Size(106, 20);
		this.te_kullanici_adi.TabIndex = 2;
		this.te_kullanici_adi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(105, 52);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(117, 13);
		this.labelControl1.TabIndex = 0;
		this.labelControl1.Text = "Genel parametre adı :";
		this.xtraTabPage1.AutoScroll = true;
		this.xtraTabPage1.Controls.Add(this.xtraTabControl1);
		this.xtraTabPage1.Name = "xtraTabPage1";
		this.xtraTabPage1.Size = new System.Drawing.Size(740, 452);
		this.xtraTabPage1.Text = "Mikro dışı ek bilgiler";
		this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.xtraTabControl1.Location = new System.Drawing.Point(0, 0);
		this.xtraTabControl1.Name = "xtraTabControl1";
		this.xtraTabControl1.SelectedTabPage = this.xtraTabPage5;
		this.xtraTabControl1.Size = new System.Drawing.Size(740, 452);
		this.xtraTabControl1.TabIndex = 0;
		this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[4] { this.xtraTabPage5, this.xtraTabPage6, this.xtraTabPage4, this.xtraTabPage2 });
		this.xtraTabPage5.Controls.Add(this.label5);
		this.xtraTabPage5.Controls.Add(this.mikro_disi_ek_bilgileri_kullan);
		this.xtraTabPage5.Name = "xtraTabPage5";
		this.xtraTabPage5.Size = new System.Drawing.Size(734, 424);
		this.xtraTabPage5.Text = "Genel";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(22, 50);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(565, 13);
		this.label5.TabIndex = 160;
		this.label5.Text = "Mikro veritabanında oluşturulan _FORA_BANKA_AKTARIM_EK_BILGILER tablosuna ekstra bilgilerin yazılmasını sağlar.";
		this.mikro_disi_ek_bilgileri_kullan.Location = new System.Drawing.Point(25, 18);
		this.mikro_disi_ek_bilgileri_kullan.Name = "mikro_disi_ek_bilgileri_kullan";
		this.mikro_disi_ek_bilgileri_kullan.Properties.Caption = "Mikro dışı ek bilgileri Sql'e aktar";
		this.mikro_disi_ek_bilgileri_kullan.Size = new System.Drawing.Size(204, 19);
		this.mikro_disi_ek_bilgileri_kullan.TabIndex = 159;
		this.xtraTabPage6.AutoScroll = true;
		this.xtraTabPage6.AutoScrollMargin = new System.Drawing.Size(0, 100);
		this.xtraTabPage6.Controls.Add(this.metin50_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin50_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin50_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin50_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin50_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label83);
		this.xtraTabPage6.Controls.Add(this.metin49_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin49_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin49_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin49_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin49_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label82);
		this.xtraTabPage6.Controls.Add(this.metin48_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin48_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin48_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin48_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin48_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label81);
		this.xtraTabPage6.Controls.Add(this.metin47_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin47_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin47_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin47_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin47_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label80);
		this.xtraTabPage6.Controls.Add(this.metin46_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin46_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin46_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin46_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin46_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label79);
		this.xtraTabPage6.Controls.Add(this.metin45_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin45_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin45_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin45_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin45_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label78);
		this.xtraTabPage6.Controls.Add(this.metin44_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin44_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin44_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin44_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin44_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label77);
		this.xtraTabPage6.Controls.Add(this.metin43_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin43_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin43_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin43_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin43_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label76);
		this.xtraTabPage6.Controls.Add(this.metin42_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin42_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin42_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin42_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin42_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label75);
		this.xtraTabPage6.Controls.Add(this.metin41_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin41_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin41_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin41_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin41_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label74);
		this.xtraTabPage6.Controls.Add(this.metin40_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin40_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin40_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin40_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin40_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label73);
		this.xtraTabPage6.Controls.Add(this.metin39_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin39_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin39_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin39_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin39_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label72);
		this.xtraTabPage6.Controls.Add(this.metin38_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin38_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin38_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin38_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin38_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label71);
		this.xtraTabPage6.Controls.Add(this.metin37_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin37_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin37_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin37_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin37_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label70);
		this.xtraTabPage6.Controls.Add(this.metin36_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin36_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin36_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin36_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin36_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label69);
		this.xtraTabPage6.Controls.Add(this.metin35_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin35_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin35_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin35_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin35_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label68);
		this.xtraTabPage6.Controls.Add(this.metin34_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin34_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin34_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin34_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin34_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label67);
		this.xtraTabPage6.Controls.Add(this.metin33_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin33_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin33_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin33_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin33_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label66);
		this.xtraTabPage6.Controls.Add(this.metin32_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin32_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin32_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin32_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin32_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label65);
		this.xtraTabPage6.Controls.Add(this.metin31_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin31_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin31_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin31_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin31_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label64);
		this.xtraTabPage6.Controls.Add(this.metin30_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin30_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin30_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin30_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin30_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label63);
		this.xtraTabPage6.Controls.Add(this.metin29_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin29_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin29_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin29_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin29_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label62);
		this.xtraTabPage6.Controls.Add(this.metin28_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin28_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin28_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin28_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin28_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label61);
		this.xtraTabPage6.Controls.Add(this.metin27_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin27_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin27_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin27_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin27_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label60);
		this.xtraTabPage6.Controls.Add(this.metin26_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin26_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin26_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin26_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin26_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label59);
		this.xtraTabPage6.Controls.Add(this.metin25_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin25_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin25_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin25_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin25_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label58);
		this.xtraTabPage6.Controls.Add(this.metin24_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin24_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin24_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin24_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin24_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label57);
		this.xtraTabPage6.Controls.Add(this.metin23_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin23_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin23_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin23_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin23_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label56);
		this.xtraTabPage6.Controls.Add(this.metin22_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin22_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin22_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin22_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin22_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label55);
		this.xtraTabPage6.Controls.Add(this.metin21_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin21_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin21_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin21_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin21_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label54);
		this.xtraTabPage6.Controls.Add(this.metin20_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin20_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin20_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin20_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin20_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label53);
		this.xtraTabPage6.Controls.Add(this.metin19_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin19_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin19_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin19_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin19_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label52);
		this.xtraTabPage6.Controls.Add(this.metin18_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin18_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin18_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin18_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin18_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label51);
		this.xtraTabPage6.Controls.Add(this.metin17_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin17_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin17_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin17_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin17_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label50);
		this.xtraTabPage6.Controls.Add(this.metin16_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin16_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin16_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin16_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin16_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label49);
		this.xtraTabPage6.Controls.Add(this.metin15_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin15_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin15_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin15_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin15_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label48);
		this.xtraTabPage6.Controls.Add(this.metin14_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin14_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin14_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin14_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin14_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label47);
		this.xtraTabPage6.Controls.Add(this.metin13_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin13_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin13_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin13_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin13_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label46);
		this.xtraTabPage6.Controls.Add(this.metin12_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin12_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin12_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin12_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin12_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label45);
		this.xtraTabPage6.Controls.Add(this.metin11_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin11_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin11_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin11_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin11_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label31);
		this.xtraTabPage6.Controls.Add(this.metin10_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin10_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin10_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin10_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin10_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label22);
		this.xtraTabPage6.Controls.Add(this.metin9_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin9_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin9_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin9_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin9_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label21);
		this.xtraTabPage6.Controls.Add(this.metin8_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin8_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin8_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin8_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin8_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label19);
		this.xtraTabPage6.Controls.Add(this.metin7_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin7_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin7_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin7_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin7_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label13);
		this.xtraTabPage6.Controls.Add(this.metin6_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin6_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin6_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin6_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin6_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label12);
		this.xtraTabPage6.Controls.Add(this.metin5_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin5_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin5_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin5_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin5_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label11);
		this.xtraTabPage6.Controls.Add(this.metin4_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin4_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin4_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin4_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin4_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label10);
		this.xtraTabPage6.Controls.Add(this.metin3_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin3_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin3_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin3_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin3_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label9);
		this.xtraTabPage6.Controls.Add(this.metin2_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.metin2_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin2_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin2_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin2_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label7);
		this.xtraTabPage6.Controls.Add(this.metin1_varsayilan_deger);
		this.xtraTabPage6.Controls.Add(this.label6);
		this.xtraTabPage6.Controls.Add(this.metin1_zorunlu);
		this.xtraTabPage6.Controls.Add(this.metin1_uzunluk);
		this.xtraTabPage6.Controls.Add(this.metin1_baslangic);
		this.xtraTabPage6.Controls.Add(this.metin1_gorunen_adi);
		this.xtraTabPage6.Controls.Add(this.label4);
		this.xtraTabPage6.Controls.Add(this.label1);
		this.xtraTabPage6.Controls.Add(this.label2);
		this.xtraTabPage6.Controls.Add(this.label3);
		this.xtraTabPage6.Name = "xtraTabPage6";
		this.xtraTabPage6.Size = new System.Drawing.Size(734, 424);
		this.xtraTabPage6.Text = "Metin alanlar";
		this.metin50_varsayilan_deger.Location = new System.Drawing.Point(481, 1314);
		this.metin50_varsayilan_deger.Name = "metin50_varsayilan_deger";
		this.metin50_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin50_varsayilan_deger.TabIndex = 498;
		this.metin50_zorunlu.Location = new System.Drawing.Point(636, 1314);
		this.metin50_zorunlu.Name = "metin50_zorunlu";
		this.metin50_zorunlu.Properties.Caption = "Zorunlu";
		this.metin50_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin50_zorunlu.TabIndex = 497;
		this.metin50_zorunlu.Visible = false;
		this.metin50_uzunluk.EditValue = new decimal(new int[4]);
		this.metin50_uzunluk.Location = new System.Drawing.Point(421, 1314);
		this.metin50_uzunluk.Name = "metin50_uzunluk";
		this.metin50_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin50_uzunluk.Properties.IsFloatValue = false;
		this.metin50_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin50_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin50_uzunluk.TabIndex = 496;
		this.metin50_baslangic.EditValue = new decimal(new int[4]);
		this.metin50_baslangic.Location = new System.Drawing.Point(316, 1314);
		this.metin50_baslangic.Name = "metin50_baslangic";
		this.metin50_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin50_baslangic.Properties.IsFloatValue = false;
		this.metin50_baslangic.Properties.Mask.EditMask = "N00";
		this.metin50_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin50_baslangic.TabIndex = 495;
		this.metin50_gorunen_adi.Location = new System.Drawing.Point(76, 1314);
		this.metin50_gorunen_adi.Name = "metin50_gorunen_adi";
		this.metin50_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin50_gorunen_adi.TabIndex = 494;
		this.label83.Location = new System.Drawing.Point(9, 1314);
		this.label83.Name = "label83";
		this.label83.Size = new System.Drawing.Size(61, 19);
		this.label83.TabIndex = 493;
		this.label83.Text = "Metin 50 :";
		this.label83.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin49_varsayilan_deger.Location = new System.Drawing.Point(481, 1288);
		this.metin49_varsayilan_deger.Name = "metin49_varsayilan_deger";
		this.metin49_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin49_varsayilan_deger.TabIndex = 492;
		this.metin49_zorunlu.Location = new System.Drawing.Point(636, 1288);
		this.metin49_zorunlu.Name = "metin49_zorunlu";
		this.metin49_zorunlu.Properties.Caption = "Zorunlu";
		this.metin49_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin49_zorunlu.TabIndex = 491;
		this.metin49_zorunlu.Visible = false;
		this.metin49_uzunluk.EditValue = new decimal(new int[4]);
		this.metin49_uzunluk.Location = new System.Drawing.Point(421, 1288);
		this.metin49_uzunluk.Name = "metin49_uzunluk";
		this.metin49_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin49_uzunluk.Properties.IsFloatValue = false;
		this.metin49_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin49_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin49_uzunluk.TabIndex = 490;
		this.metin49_baslangic.EditValue = new decimal(new int[4]);
		this.metin49_baslangic.Location = new System.Drawing.Point(316, 1288);
		this.metin49_baslangic.Name = "metin49_baslangic";
		this.metin49_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin49_baslangic.Properties.IsFloatValue = false;
		this.metin49_baslangic.Properties.Mask.EditMask = "N00";
		this.metin49_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin49_baslangic.TabIndex = 489;
		this.metin49_gorunen_adi.Location = new System.Drawing.Point(76, 1288);
		this.metin49_gorunen_adi.Name = "metin49_gorunen_adi";
		this.metin49_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin49_gorunen_adi.TabIndex = 488;
		this.label82.Location = new System.Drawing.Point(9, 1288);
		this.label82.Name = "label82";
		this.label82.Size = new System.Drawing.Size(61, 19);
		this.label82.TabIndex = 487;
		this.label82.Text = "Metin 49 :";
		this.label82.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin48_varsayilan_deger.Location = new System.Drawing.Point(481, 1262);
		this.metin48_varsayilan_deger.Name = "metin48_varsayilan_deger";
		this.metin48_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin48_varsayilan_deger.TabIndex = 486;
		this.metin48_zorunlu.Location = new System.Drawing.Point(636, 1262);
		this.metin48_zorunlu.Name = "metin48_zorunlu";
		this.metin48_zorunlu.Properties.Caption = "Zorunlu";
		this.metin48_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin48_zorunlu.TabIndex = 485;
		this.metin48_zorunlu.Visible = false;
		this.metin48_uzunluk.EditValue = new decimal(new int[4]);
		this.metin48_uzunluk.Location = new System.Drawing.Point(421, 1262);
		this.metin48_uzunluk.Name = "metin48_uzunluk";
		this.metin48_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin48_uzunluk.Properties.IsFloatValue = false;
		this.metin48_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin48_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin48_uzunluk.TabIndex = 484;
		this.metin48_baslangic.EditValue = new decimal(new int[4]);
		this.metin48_baslangic.Location = new System.Drawing.Point(316, 1262);
		this.metin48_baslangic.Name = "metin48_baslangic";
		this.metin48_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin48_baslangic.Properties.IsFloatValue = false;
		this.metin48_baslangic.Properties.Mask.EditMask = "N00";
		this.metin48_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin48_baslangic.TabIndex = 483;
		this.metin48_gorunen_adi.Location = new System.Drawing.Point(76, 1262);
		this.metin48_gorunen_adi.Name = "metin48_gorunen_adi";
		this.metin48_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin48_gorunen_adi.TabIndex = 482;
		this.label81.Location = new System.Drawing.Point(9, 1262);
		this.label81.Name = "label81";
		this.label81.Size = new System.Drawing.Size(61, 19);
		this.label81.TabIndex = 481;
		this.label81.Text = "Metin 48 :";
		this.label81.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin47_varsayilan_deger.Location = new System.Drawing.Point(481, 1236);
		this.metin47_varsayilan_deger.Name = "metin47_varsayilan_deger";
		this.metin47_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin47_varsayilan_deger.TabIndex = 480;
		this.metin47_zorunlu.Location = new System.Drawing.Point(636, 1236);
		this.metin47_zorunlu.Name = "metin47_zorunlu";
		this.metin47_zorunlu.Properties.Caption = "Zorunlu";
		this.metin47_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin47_zorunlu.TabIndex = 479;
		this.metin47_zorunlu.Visible = false;
		this.metin47_uzunluk.EditValue = new decimal(new int[4]);
		this.metin47_uzunluk.Location = new System.Drawing.Point(421, 1236);
		this.metin47_uzunluk.Name = "metin47_uzunluk";
		this.metin47_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin47_uzunluk.Properties.IsFloatValue = false;
		this.metin47_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin47_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin47_uzunluk.TabIndex = 478;
		this.metin47_baslangic.EditValue = new decimal(new int[4]);
		this.metin47_baslangic.Location = new System.Drawing.Point(316, 1236);
		this.metin47_baslangic.Name = "metin47_baslangic";
		this.metin47_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin47_baslangic.Properties.IsFloatValue = false;
		this.metin47_baslangic.Properties.Mask.EditMask = "N00";
		this.metin47_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin47_baslangic.TabIndex = 477;
		this.metin47_gorunen_adi.Location = new System.Drawing.Point(76, 1236);
		this.metin47_gorunen_adi.Name = "metin47_gorunen_adi";
		this.metin47_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin47_gorunen_adi.TabIndex = 476;
		this.label80.Location = new System.Drawing.Point(9, 1236);
		this.label80.Name = "label80";
		this.label80.Size = new System.Drawing.Size(61, 19);
		this.label80.TabIndex = 475;
		this.label80.Text = "Metin 47 :";
		this.label80.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin46_varsayilan_deger.Location = new System.Drawing.Point(481, 1210);
		this.metin46_varsayilan_deger.Name = "metin46_varsayilan_deger";
		this.metin46_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin46_varsayilan_deger.TabIndex = 474;
		this.metin46_zorunlu.Location = new System.Drawing.Point(636, 1210);
		this.metin46_zorunlu.Name = "metin46_zorunlu";
		this.metin46_zorunlu.Properties.Caption = "Zorunlu";
		this.metin46_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin46_zorunlu.TabIndex = 473;
		this.metin46_zorunlu.Visible = false;
		this.metin46_uzunluk.EditValue = new decimal(new int[4]);
		this.metin46_uzunluk.Location = new System.Drawing.Point(421, 1210);
		this.metin46_uzunluk.Name = "metin46_uzunluk";
		this.metin46_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin46_uzunluk.Properties.IsFloatValue = false;
		this.metin46_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin46_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin46_uzunluk.TabIndex = 472;
		this.metin46_baslangic.EditValue = new decimal(new int[4]);
		this.metin46_baslangic.Location = new System.Drawing.Point(316, 1210);
		this.metin46_baslangic.Name = "metin46_baslangic";
		this.metin46_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin46_baslangic.Properties.IsFloatValue = false;
		this.metin46_baslangic.Properties.Mask.EditMask = "N00";
		this.metin46_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin46_baslangic.TabIndex = 471;
		this.metin46_gorunen_adi.Location = new System.Drawing.Point(76, 1210);
		this.metin46_gorunen_adi.Name = "metin46_gorunen_adi";
		this.metin46_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin46_gorunen_adi.TabIndex = 470;
		this.label79.Location = new System.Drawing.Point(9, 1210);
		this.label79.Name = "label79";
		this.label79.Size = new System.Drawing.Size(61, 19);
		this.label79.TabIndex = 469;
		this.label79.Text = "Metin 46 :";
		this.label79.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin45_varsayilan_deger.Location = new System.Drawing.Point(481, 1184);
		this.metin45_varsayilan_deger.Name = "metin45_varsayilan_deger";
		this.metin45_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin45_varsayilan_deger.TabIndex = 468;
		this.metin45_zorunlu.Location = new System.Drawing.Point(636, 1184);
		this.metin45_zorunlu.Name = "metin45_zorunlu";
		this.metin45_zorunlu.Properties.Caption = "Zorunlu";
		this.metin45_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin45_zorunlu.TabIndex = 467;
		this.metin45_zorunlu.Visible = false;
		this.metin45_uzunluk.EditValue = new decimal(new int[4]);
		this.metin45_uzunluk.Location = new System.Drawing.Point(421, 1184);
		this.metin45_uzunluk.Name = "metin45_uzunluk";
		this.metin45_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin45_uzunluk.Properties.IsFloatValue = false;
		this.metin45_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin45_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin45_uzunluk.TabIndex = 466;
		this.metin45_baslangic.EditValue = new decimal(new int[4]);
		this.metin45_baslangic.Location = new System.Drawing.Point(316, 1184);
		this.metin45_baslangic.Name = "metin45_baslangic";
		this.metin45_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin45_baslangic.Properties.IsFloatValue = false;
		this.metin45_baslangic.Properties.Mask.EditMask = "N00";
		this.metin45_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin45_baslangic.TabIndex = 465;
		this.metin45_gorunen_adi.Location = new System.Drawing.Point(76, 1184);
		this.metin45_gorunen_adi.Name = "metin45_gorunen_adi";
		this.metin45_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin45_gorunen_adi.TabIndex = 464;
		this.label78.Location = new System.Drawing.Point(9, 1184);
		this.label78.Name = "label78";
		this.label78.Size = new System.Drawing.Size(61, 19);
		this.label78.TabIndex = 463;
		this.label78.Text = "Metin 45 :";
		this.label78.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin44_varsayilan_deger.Location = new System.Drawing.Point(481, 1158);
		this.metin44_varsayilan_deger.Name = "metin44_varsayilan_deger";
		this.metin44_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin44_varsayilan_deger.TabIndex = 462;
		this.metin44_zorunlu.Location = new System.Drawing.Point(636, 1158);
		this.metin44_zorunlu.Name = "metin44_zorunlu";
		this.metin44_zorunlu.Properties.Caption = "Zorunlu";
		this.metin44_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin44_zorunlu.TabIndex = 461;
		this.metin44_zorunlu.Visible = false;
		this.metin44_uzunluk.EditValue = new decimal(new int[4]);
		this.metin44_uzunluk.Location = new System.Drawing.Point(421, 1158);
		this.metin44_uzunluk.Name = "metin44_uzunluk";
		this.metin44_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin44_uzunluk.Properties.IsFloatValue = false;
		this.metin44_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin44_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin44_uzunluk.TabIndex = 460;
		this.metin44_baslangic.EditValue = new decimal(new int[4]);
		this.metin44_baslangic.Location = new System.Drawing.Point(316, 1158);
		this.metin44_baslangic.Name = "metin44_baslangic";
		this.metin44_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin44_baslangic.Properties.IsFloatValue = false;
		this.metin44_baslangic.Properties.Mask.EditMask = "N00";
		this.metin44_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin44_baslangic.TabIndex = 459;
		this.metin44_gorunen_adi.Location = new System.Drawing.Point(76, 1158);
		this.metin44_gorunen_adi.Name = "metin44_gorunen_adi";
		this.metin44_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin44_gorunen_adi.TabIndex = 458;
		this.label77.Location = new System.Drawing.Point(9, 1158);
		this.label77.Name = "label77";
		this.label77.Size = new System.Drawing.Size(61, 19);
		this.label77.TabIndex = 457;
		this.label77.Text = "Metin 44 :";
		this.label77.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin43_varsayilan_deger.Location = new System.Drawing.Point(481, 1132);
		this.metin43_varsayilan_deger.Name = "metin43_varsayilan_deger";
		this.metin43_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin43_varsayilan_deger.TabIndex = 456;
		this.metin43_zorunlu.Location = new System.Drawing.Point(636, 1132);
		this.metin43_zorunlu.Name = "metin43_zorunlu";
		this.metin43_zorunlu.Properties.Caption = "Zorunlu";
		this.metin43_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin43_zorunlu.TabIndex = 455;
		this.metin43_zorunlu.Visible = false;
		this.metin43_uzunluk.EditValue = new decimal(new int[4]);
		this.metin43_uzunluk.Location = new System.Drawing.Point(421, 1132);
		this.metin43_uzunluk.Name = "metin43_uzunluk";
		this.metin43_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin43_uzunluk.Properties.IsFloatValue = false;
		this.metin43_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin43_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin43_uzunluk.TabIndex = 454;
		this.metin43_baslangic.EditValue = new decimal(new int[4]);
		this.metin43_baslangic.Location = new System.Drawing.Point(316, 1132);
		this.metin43_baslangic.Name = "metin43_baslangic";
		this.metin43_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin43_baslangic.Properties.IsFloatValue = false;
		this.metin43_baslangic.Properties.Mask.EditMask = "N00";
		this.metin43_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin43_baslangic.TabIndex = 453;
		this.metin43_gorunen_adi.Location = new System.Drawing.Point(76, 1132);
		this.metin43_gorunen_adi.Name = "metin43_gorunen_adi";
		this.metin43_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin43_gorunen_adi.TabIndex = 452;
		this.label76.Location = new System.Drawing.Point(9, 1132);
		this.label76.Name = "label76";
		this.label76.Size = new System.Drawing.Size(61, 19);
		this.label76.TabIndex = 451;
		this.label76.Text = "Metin 43 :";
		this.label76.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin42_varsayilan_deger.Location = new System.Drawing.Point(481, 1106);
		this.metin42_varsayilan_deger.Name = "metin42_varsayilan_deger";
		this.metin42_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin42_varsayilan_deger.TabIndex = 450;
		this.metin42_zorunlu.Location = new System.Drawing.Point(636, 1106);
		this.metin42_zorunlu.Name = "metin42_zorunlu";
		this.metin42_zorunlu.Properties.Caption = "Zorunlu";
		this.metin42_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin42_zorunlu.TabIndex = 449;
		this.metin42_zorunlu.Visible = false;
		this.metin42_uzunluk.EditValue = new decimal(new int[4]);
		this.metin42_uzunluk.Location = new System.Drawing.Point(421, 1106);
		this.metin42_uzunluk.Name = "metin42_uzunluk";
		this.metin42_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin42_uzunluk.Properties.IsFloatValue = false;
		this.metin42_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin42_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin42_uzunluk.TabIndex = 448;
		this.metin42_baslangic.EditValue = new decimal(new int[4]);
		this.metin42_baslangic.Location = new System.Drawing.Point(316, 1106);
		this.metin42_baslangic.Name = "metin42_baslangic";
		this.metin42_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin42_baslangic.Properties.IsFloatValue = false;
		this.metin42_baslangic.Properties.Mask.EditMask = "N00";
		this.metin42_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin42_baslangic.TabIndex = 447;
		this.metin42_gorunen_adi.Location = new System.Drawing.Point(76, 1106);
		this.metin42_gorunen_adi.Name = "metin42_gorunen_adi";
		this.metin42_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin42_gorunen_adi.TabIndex = 446;
		this.label75.Location = new System.Drawing.Point(9, 1106);
		this.label75.Name = "label75";
		this.label75.Size = new System.Drawing.Size(61, 19);
		this.label75.TabIndex = 445;
		this.label75.Text = "Metin 42 :";
		this.label75.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin41_varsayilan_deger.Location = new System.Drawing.Point(481, 1080);
		this.metin41_varsayilan_deger.Name = "metin41_varsayilan_deger";
		this.metin41_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin41_varsayilan_deger.TabIndex = 444;
		this.metin41_zorunlu.Location = new System.Drawing.Point(636, 1080);
		this.metin41_zorunlu.Name = "metin41_zorunlu";
		this.metin41_zorunlu.Properties.Caption = "Zorunlu";
		this.metin41_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin41_zorunlu.TabIndex = 443;
		this.metin41_zorunlu.Visible = false;
		this.metin41_uzunluk.EditValue = new decimal(new int[4]);
		this.metin41_uzunluk.Location = new System.Drawing.Point(421, 1080);
		this.metin41_uzunluk.Name = "metin41_uzunluk";
		this.metin41_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin41_uzunluk.Properties.IsFloatValue = false;
		this.metin41_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin41_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin41_uzunluk.TabIndex = 442;
		this.metin41_baslangic.EditValue = new decimal(new int[4]);
		this.metin41_baslangic.Location = new System.Drawing.Point(316, 1080);
		this.metin41_baslangic.Name = "metin41_baslangic";
		this.metin41_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin41_baslangic.Properties.IsFloatValue = false;
		this.metin41_baslangic.Properties.Mask.EditMask = "N00";
		this.metin41_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin41_baslangic.TabIndex = 441;
		this.metin41_gorunen_adi.Location = new System.Drawing.Point(76, 1080);
		this.metin41_gorunen_adi.Name = "metin41_gorunen_adi";
		this.metin41_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin41_gorunen_adi.TabIndex = 440;
		this.label74.Location = new System.Drawing.Point(9, 1080);
		this.label74.Name = "label74";
		this.label74.Size = new System.Drawing.Size(61, 19);
		this.label74.TabIndex = 439;
		this.label74.Text = "Metin 41 :";
		this.label74.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin40_varsayilan_deger.Location = new System.Drawing.Point(481, 1054);
		this.metin40_varsayilan_deger.Name = "metin40_varsayilan_deger";
		this.metin40_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin40_varsayilan_deger.TabIndex = 438;
		this.metin40_zorunlu.Location = new System.Drawing.Point(636, 1054);
		this.metin40_zorunlu.Name = "metin40_zorunlu";
		this.metin40_zorunlu.Properties.Caption = "Zorunlu";
		this.metin40_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin40_zorunlu.TabIndex = 437;
		this.metin40_zorunlu.Visible = false;
		this.metin40_uzunluk.EditValue = new decimal(new int[4]);
		this.metin40_uzunluk.Location = new System.Drawing.Point(421, 1054);
		this.metin40_uzunluk.Name = "metin40_uzunluk";
		this.metin40_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin40_uzunluk.Properties.IsFloatValue = false;
		this.metin40_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin40_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin40_uzunluk.TabIndex = 436;
		this.metin40_baslangic.EditValue = new decimal(new int[4]);
		this.metin40_baslangic.Location = new System.Drawing.Point(316, 1054);
		this.metin40_baslangic.Name = "metin40_baslangic";
		this.metin40_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin40_baslangic.Properties.IsFloatValue = false;
		this.metin40_baslangic.Properties.Mask.EditMask = "N00";
		this.metin40_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin40_baslangic.TabIndex = 435;
		this.metin40_gorunen_adi.Location = new System.Drawing.Point(76, 1054);
		this.metin40_gorunen_adi.Name = "metin40_gorunen_adi";
		this.metin40_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin40_gorunen_adi.TabIndex = 434;
		this.label73.Location = new System.Drawing.Point(9, 1054);
		this.label73.Name = "label73";
		this.label73.Size = new System.Drawing.Size(61, 19);
		this.label73.TabIndex = 433;
		this.label73.Text = "Metin 40 :";
		this.label73.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin39_varsayilan_deger.Location = new System.Drawing.Point(481, 1028);
		this.metin39_varsayilan_deger.Name = "metin39_varsayilan_deger";
		this.metin39_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin39_varsayilan_deger.TabIndex = 432;
		this.metin39_zorunlu.Location = new System.Drawing.Point(636, 1028);
		this.metin39_zorunlu.Name = "metin39_zorunlu";
		this.metin39_zorunlu.Properties.Caption = "Zorunlu";
		this.metin39_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin39_zorunlu.TabIndex = 431;
		this.metin39_zorunlu.Visible = false;
		this.metin39_uzunluk.EditValue = new decimal(new int[4]);
		this.metin39_uzunluk.Location = new System.Drawing.Point(421, 1028);
		this.metin39_uzunluk.Name = "metin39_uzunluk";
		this.metin39_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin39_uzunluk.Properties.IsFloatValue = false;
		this.metin39_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin39_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin39_uzunluk.TabIndex = 430;
		this.metin39_baslangic.EditValue = new decimal(new int[4]);
		this.metin39_baslangic.Location = new System.Drawing.Point(316, 1028);
		this.metin39_baslangic.Name = "metin39_baslangic";
		this.metin39_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin39_baslangic.Properties.IsFloatValue = false;
		this.metin39_baslangic.Properties.Mask.EditMask = "N00";
		this.metin39_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin39_baslangic.TabIndex = 429;
		this.metin39_gorunen_adi.Location = new System.Drawing.Point(76, 1028);
		this.metin39_gorunen_adi.Name = "metin39_gorunen_adi";
		this.metin39_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin39_gorunen_adi.TabIndex = 428;
		this.label72.Location = new System.Drawing.Point(9, 1028);
		this.label72.Name = "label72";
		this.label72.Size = new System.Drawing.Size(61, 19);
		this.label72.TabIndex = 427;
		this.label72.Text = "Metin 39 :";
		this.label72.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin38_varsayilan_deger.Location = new System.Drawing.Point(481, 1002);
		this.metin38_varsayilan_deger.Name = "metin38_varsayilan_deger";
		this.metin38_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin38_varsayilan_deger.TabIndex = 426;
		this.metin38_zorunlu.Location = new System.Drawing.Point(636, 1002);
		this.metin38_zorunlu.Name = "metin38_zorunlu";
		this.metin38_zorunlu.Properties.Caption = "Zorunlu";
		this.metin38_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin38_zorunlu.TabIndex = 425;
		this.metin38_zorunlu.Visible = false;
		this.metin38_uzunluk.EditValue = new decimal(new int[4]);
		this.metin38_uzunluk.Location = new System.Drawing.Point(421, 1002);
		this.metin38_uzunluk.Name = "metin38_uzunluk";
		this.metin38_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin38_uzunluk.Properties.IsFloatValue = false;
		this.metin38_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin38_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin38_uzunluk.TabIndex = 424;
		this.metin38_baslangic.EditValue = new decimal(new int[4]);
		this.metin38_baslangic.Location = new System.Drawing.Point(316, 1002);
		this.metin38_baslangic.Name = "metin38_baslangic";
		this.metin38_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin38_baslangic.Properties.IsFloatValue = false;
		this.metin38_baslangic.Properties.Mask.EditMask = "N00";
		this.metin38_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin38_baslangic.TabIndex = 423;
		this.metin38_gorunen_adi.Location = new System.Drawing.Point(76, 1002);
		this.metin38_gorunen_adi.Name = "metin38_gorunen_adi";
		this.metin38_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin38_gorunen_adi.TabIndex = 422;
		this.label71.Location = new System.Drawing.Point(9, 1002);
		this.label71.Name = "label71";
		this.label71.Size = new System.Drawing.Size(61, 19);
		this.label71.TabIndex = 421;
		this.label71.Text = "Metin 38 :";
		this.label71.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin37_varsayilan_deger.Location = new System.Drawing.Point(481, 976);
		this.metin37_varsayilan_deger.Name = "metin37_varsayilan_deger";
		this.metin37_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin37_varsayilan_deger.TabIndex = 420;
		this.metin37_zorunlu.Location = new System.Drawing.Point(636, 976);
		this.metin37_zorunlu.Name = "metin37_zorunlu";
		this.metin37_zorunlu.Properties.Caption = "Zorunlu";
		this.metin37_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin37_zorunlu.TabIndex = 419;
		this.metin37_zorunlu.Visible = false;
		this.metin37_uzunluk.EditValue = new decimal(new int[4]);
		this.metin37_uzunluk.Location = new System.Drawing.Point(421, 976);
		this.metin37_uzunluk.Name = "metin37_uzunluk";
		this.metin37_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin37_uzunluk.Properties.IsFloatValue = false;
		this.metin37_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin37_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin37_uzunluk.TabIndex = 418;
		this.metin37_baslangic.EditValue = new decimal(new int[4]);
		this.metin37_baslangic.Location = new System.Drawing.Point(316, 976);
		this.metin37_baslangic.Name = "metin37_baslangic";
		this.metin37_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin37_baslangic.Properties.IsFloatValue = false;
		this.metin37_baslangic.Properties.Mask.EditMask = "N00";
		this.metin37_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin37_baslangic.TabIndex = 417;
		this.metin37_gorunen_adi.Location = new System.Drawing.Point(76, 976);
		this.metin37_gorunen_adi.Name = "metin37_gorunen_adi";
		this.metin37_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin37_gorunen_adi.TabIndex = 416;
		this.label70.Location = new System.Drawing.Point(9, 976);
		this.label70.Name = "label70";
		this.label70.Size = new System.Drawing.Size(61, 19);
		this.label70.TabIndex = 415;
		this.label70.Text = "Metin 37 :";
		this.label70.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin36_varsayilan_deger.Location = new System.Drawing.Point(481, 950);
		this.metin36_varsayilan_deger.Name = "metin36_varsayilan_deger";
		this.metin36_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin36_varsayilan_deger.TabIndex = 414;
		this.metin36_zorunlu.Location = new System.Drawing.Point(636, 950);
		this.metin36_zorunlu.Name = "metin36_zorunlu";
		this.metin36_zorunlu.Properties.Caption = "Zorunlu";
		this.metin36_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin36_zorunlu.TabIndex = 413;
		this.metin36_zorunlu.Visible = false;
		this.metin36_uzunluk.EditValue = new decimal(new int[4]);
		this.metin36_uzunluk.Location = new System.Drawing.Point(421, 950);
		this.metin36_uzunluk.Name = "metin36_uzunluk";
		this.metin36_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin36_uzunluk.Properties.IsFloatValue = false;
		this.metin36_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin36_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin36_uzunluk.TabIndex = 412;
		this.metin36_baslangic.EditValue = new decimal(new int[4]);
		this.metin36_baslangic.Location = new System.Drawing.Point(316, 950);
		this.metin36_baslangic.Name = "metin36_baslangic";
		this.metin36_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin36_baslangic.Properties.IsFloatValue = false;
		this.metin36_baslangic.Properties.Mask.EditMask = "N00";
		this.metin36_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin36_baslangic.TabIndex = 411;
		this.metin36_gorunen_adi.Location = new System.Drawing.Point(76, 950);
		this.metin36_gorunen_adi.Name = "metin36_gorunen_adi";
		this.metin36_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin36_gorunen_adi.TabIndex = 410;
		this.label69.Location = new System.Drawing.Point(9, 950);
		this.label69.Name = "label69";
		this.label69.Size = new System.Drawing.Size(61, 19);
		this.label69.TabIndex = 409;
		this.label69.Text = "Metin 36 :";
		this.label69.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin35_varsayilan_deger.Location = new System.Drawing.Point(481, 924);
		this.metin35_varsayilan_deger.Name = "metin35_varsayilan_deger";
		this.metin35_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin35_varsayilan_deger.TabIndex = 408;
		this.metin35_zorunlu.Location = new System.Drawing.Point(636, 924);
		this.metin35_zorunlu.Name = "metin35_zorunlu";
		this.metin35_zorunlu.Properties.Caption = "Zorunlu";
		this.metin35_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin35_zorunlu.TabIndex = 407;
		this.metin35_zorunlu.Visible = false;
		this.metin35_uzunluk.EditValue = new decimal(new int[4]);
		this.metin35_uzunluk.Location = new System.Drawing.Point(421, 924);
		this.metin35_uzunluk.Name = "metin35_uzunluk";
		this.metin35_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin35_uzunluk.Properties.IsFloatValue = false;
		this.metin35_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin35_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin35_uzunluk.TabIndex = 406;
		this.metin35_baslangic.EditValue = new decimal(new int[4]);
		this.metin35_baslangic.Location = new System.Drawing.Point(316, 924);
		this.metin35_baslangic.Name = "metin35_baslangic";
		this.metin35_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin35_baslangic.Properties.IsFloatValue = false;
		this.metin35_baslangic.Properties.Mask.EditMask = "N00";
		this.metin35_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin35_baslangic.TabIndex = 405;
		this.metin35_gorunen_adi.Location = new System.Drawing.Point(76, 924);
		this.metin35_gorunen_adi.Name = "metin35_gorunen_adi";
		this.metin35_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin35_gorunen_adi.TabIndex = 404;
		this.label68.Location = new System.Drawing.Point(9, 924);
		this.label68.Name = "label68";
		this.label68.Size = new System.Drawing.Size(61, 19);
		this.label68.TabIndex = 403;
		this.label68.Text = "Metin 35 :";
		this.label68.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin34_varsayilan_deger.Location = new System.Drawing.Point(481, 898);
		this.metin34_varsayilan_deger.Name = "metin34_varsayilan_deger";
		this.metin34_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin34_varsayilan_deger.TabIndex = 402;
		this.metin34_zorunlu.Location = new System.Drawing.Point(636, 898);
		this.metin34_zorunlu.Name = "metin34_zorunlu";
		this.metin34_zorunlu.Properties.Caption = "Zorunlu";
		this.metin34_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin34_zorunlu.TabIndex = 401;
		this.metin34_zorunlu.Visible = false;
		this.metin34_uzunluk.EditValue = new decimal(new int[4]);
		this.metin34_uzunluk.Location = new System.Drawing.Point(421, 898);
		this.metin34_uzunluk.Name = "metin34_uzunluk";
		this.metin34_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin34_uzunluk.Properties.IsFloatValue = false;
		this.metin34_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin34_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin34_uzunluk.TabIndex = 400;
		this.metin34_baslangic.EditValue = new decimal(new int[4]);
		this.metin34_baslangic.Location = new System.Drawing.Point(316, 898);
		this.metin34_baslangic.Name = "metin34_baslangic";
		this.metin34_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin34_baslangic.Properties.IsFloatValue = false;
		this.metin34_baslangic.Properties.Mask.EditMask = "N00";
		this.metin34_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin34_baslangic.TabIndex = 399;
		this.metin34_gorunen_adi.Location = new System.Drawing.Point(76, 898);
		this.metin34_gorunen_adi.Name = "metin34_gorunen_adi";
		this.metin34_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin34_gorunen_adi.TabIndex = 398;
		this.label67.Location = new System.Drawing.Point(9, 898);
		this.label67.Name = "label67";
		this.label67.Size = new System.Drawing.Size(61, 19);
		this.label67.TabIndex = 397;
		this.label67.Text = "Metin 34 :";
		this.label67.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin33_varsayilan_deger.Location = new System.Drawing.Point(481, 872);
		this.metin33_varsayilan_deger.Name = "metin33_varsayilan_deger";
		this.metin33_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin33_varsayilan_deger.TabIndex = 396;
		this.metin33_zorunlu.Location = new System.Drawing.Point(636, 872);
		this.metin33_zorunlu.Name = "metin33_zorunlu";
		this.metin33_zorunlu.Properties.Caption = "Zorunlu";
		this.metin33_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin33_zorunlu.TabIndex = 395;
		this.metin33_zorunlu.Visible = false;
		this.metin33_uzunluk.EditValue = new decimal(new int[4]);
		this.metin33_uzunluk.Location = new System.Drawing.Point(421, 872);
		this.metin33_uzunluk.Name = "metin33_uzunluk";
		this.metin33_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin33_uzunluk.Properties.IsFloatValue = false;
		this.metin33_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin33_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin33_uzunluk.TabIndex = 394;
		this.metin33_baslangic.EditValue = new decimal(new int[4]);
		this.metin33_baslangic.Location = new System.Drawing.Point(316, 872);
		this.metin33_baslangic.Name = "metin33_baslangic";
		this.metin33_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin33_baslangic.Properties.IsFloatValue = false;
		this.metin33_baslangic.Properties.Mask.EditMask = "N00";
		this.metin33_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin33_baslangic.TabIndex = 393;
		this.metin33_gorunen_adi.Location = new System.Drawing.Point(76, 872);
		this.metin33_gorunen_adi.Name = "metin33_gorunen_adi";
		this.metin33_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin33_gorunen_adi.TabIndex = 392;
		this.label66.Location = new System.Drawing.Point(9, 872);
		this.label66.Name = "label66";
		this.label66.Size = new System.Drawing.Size(61, 19);
		this.label66.TabIndex = 391;
		this.label66.Text = "Metin 33 :";
		this.label66.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin32_varsayilan_deger.Location = new System.Drawing.Point(481, 846);
		this.metin32_varsayilan_deger.Name = "metin32_varsayilan_deger";
		this.metin32_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin32_varsayilan_deger.TabIndex = 390;
		this.metin32_zorunlu.Location = new System.Drawing.Point(636, 846);
		this.metin32_zorunlu.Name = "metin32_zorunlu";
		this.metin32_zorunlu.Properties.Caption = "Zorunlu";
		this.metin32_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin32_zorunlu.TabIndex = 389;
		this.metin32_zorunlu.Visible = false;
		this.metin32_uzunluk.EditValue = new decimal(new int[4]);
		this.metin32_uzunluk.Location = new System.Drawing.Point(421, 846);
		this.metin32_uzunluk.Name = "metin32_uzunluk";
		this.metin32_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin32_uzunluk.Properties.IsFloatValue = false;
		this.metin32_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin32_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin32_uzunluk.TabIndex = 388;
		this.metin32_baslangic.EditValue = new decimal(new int[4]);
		this.metin32_baslangic.Location = new System.Drawing.Point(316, 846);
		this.metin32_baslangic.Name = "metin32_baslangic";
		this.metin32_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin32_baslangic.Properties.IsFloatValue = false;
		this.metin32_baslangic.Properties.Mask.EditMask = "N00";
		this.metin32_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin32_baslangic.TabIndex = 387;
		this.metin32_gorunen_adi.Location = new System.Drawing.Point(76, 846);
		this.metin32_gorunen_adi.Name = "metin32_gorunen_adi";
		this.metin32_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin32_gorunen_adi.TabIndex = 386;
		this.label65.Location = new System.Drawing.Point(9, 846);
		this.label65.Name = "label65";
		this.label65.Size = new System.Drawing.Size(61, 19);
		this.label65.TabIndex = 385;
		this.label65.Text = "Metin 32 :";
		this.label65.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin31_varsayilan_deger.Location = new System.Drawing.Point(481, 820);
		this.metin31_varsayilan_deger.Name = "metin31_varsayilan_deger";
		this.metin31_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin31_varsayilan_deger.TabIndex = 384;
		this.metin31_zorunlu.Location = new System.Drawing.Point(636, 820);
		this.metin31_zorunlu.Name = "metin31_zorunlu";
		this.metin31_zorunlu.Properties.Caption = "Zorunlu";
		this.metin31_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin31_zorunlu.TabIndex = 383;
		this.metin31_zorunlu.Visible = false;
		this.metin31_uzunluk.EditValue = new decimal(new int[4]);
		this.metin31_uzunluk.Location = new System.Drawing.Point(421, 820);
		this.metin31_uzunluk.Name = "metin31_uzunluk";
		this.metin31_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin31_uzunluk.Properties.IsFloatValue = false;
		this.metin31_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin31_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin31_uzunluk.TabIndex = 382;
		this.metin31_baslangic.EditValue = new decimal(new int[4]);
		this.metin31_baslangic.Location = new System.Drawing.Point(316, 820);
		this.metin31_baslangic.Name = "metin31_baslangic";
		this.metin31_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin31_baslangic.Properties.IsFloatValue = false;
		this.metin31_baslangic.Properties.Mask.EditMask = "N00";
		this.metin31_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin31_baslangic.TabIndex = 381;
		this.metin31_gorunen_adi.Location = new System.Drawing.Point(76, 820);
		this.metin31_gorunen_adi.Name = "metin31_gorunen_adi";
		this.metin31_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin31_gorunen_adi.TabIndex = 380;
		this.label64.Location = new System.Drawing.Point(9, 820);
		this.label64.Name = "label64";
		this.label64.Size = new System.Drawing.Size(61, 19);
		this.label64.TabIndex = 379;
		this.label64.Text = "Metin 31 :";
		this.label64.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin30_varsayilan_deger.Location = new System.Drawing.Point(481, 794);
		this.metin30_varsayilan_deger.Name = "metin30_varsayilan_deger";
		this.metin30_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin30_varsayilan_deger.TabIndex = 378;
		this.metin30_zorunlu.Location = new System.Drawing.Point(636, 794);
		this.metin30_zorunlu.Name = "metin30_zorunlu";
		this.metin30_zorunlu.Properties.Caption = "Zorunlu";
		this.metin30_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin30_zorunlu.TabIndex = 377;
		this.metin30_zorunlu.Visible = false;
		this.metin30_uzunluk.EditValue = new decimal(new int[4]);
		this.metin30_uzunluk.Location = new System.Drawing.Point(421, 794);
		this.metin30_uzunluk.Name = "metin30_uzunluk";
		this.metin30_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin30_uzunluk.Properties.IsFloatValue = false;
		this.metin30_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin30_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin30_uzunluk.TabIndex = 376;
		this.metin30_baslangic.EditValue = new decimal(new int[4]);
		this.metin30_baslangic.Location = new System.Drawing.Point(316, 794);
		this.metin30_baslangic.Name = "metin30_baslangic";
		this.metin30_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin30_baslangic.Properties.IsFloatValue = false;
		this.metin30_baslangic.Properties.Mask.EditMask = "N00";
		this.metin30_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin30_baslangic.TabIndex = 375;
		this.metin30_gorunen_adi.Location = new System.Drawing.Point(76, 794);
		this.metin30_gorunen_adi.Name = "metin30_gorunen_adi";
		this.metin30_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin30_gorunen_adi.TabIndex = 374;
		this.label63.Location = new System.Drawing.Point(9, 794);
		this.label63.Name = "label63";
		this.label63.Size = new System.Drawing.Size(61, 19);
		this.label63.TabIndex = 373;
		this.label63.Text = "Metin 30 :";
		this.label63.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin29_varsayilan_deger.Location = new System.Drawing.Point(481, 768);
		this.metin29_varsayilan_deger.Name = "metin29_varsayilan_deger";
		this.metin29_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin29_varsayilan_deger.TabIndex = 372;
		this.metin29_zorunlu.Location = new System.Drawing.Point(636, 768);
		this.metin29_zorunlu.Name = "metin29_zorunlu";
		this.metin29_zorunlu.Properties.Caption = "Zorunlu";
		this.metin29_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin29_zorunlu.TabIndex = 371;
		this.metin29_zorunlu.Visible = false;
		this.metin29_uzunluk.EditValue = new decimal(new int[4]);
		this.metin29_uzunluk.Location = new System.Drawing.Point(421, 768);
		this.metin29_uzunluk.Name = "metin29_uzunluk";
		this.metin29_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin29_uzunluk.Properties.IsFloatValue = false;
		this.metin29_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin29_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin29_uzunluk.TabIndex = 370;
		this.metin29_baslangic.EditValue = new decimal(new int[4]);
		this.metin29_baslangic.Location = new System.Drawing.Point(316, 768);
		this.metin29_baslangic.Name = "metin29_baslangic";
		this.metin29_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin29_baslangic.Properties.IsFloatValue = false;
		this.metin29_baslangic.Properties.Mask.EditMask = "N00";
		this.metin29_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin29_baslangic.TabIndex = 369;
		this.metin29_gorunen_adi.Location = new System.Drawing.Point(76, 768);
		this.metin29_gorunen_adi.Name = "metin29_gorunen_adi";
		this.metin29_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin29_gorunen_adi.TabIndex = 368;
		this.label62.Location = new System.Drawing.Point(9, 768);
		this.label62.Name = "label62";
		this.label62.Size = new System.Drawing.Size(61, 19);
		this.label62.TabIndex = 367;
		this.label62.Text = "Metin 29 :";
		this.label62.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin28_varsayilan_deger.Location = new System.Drawing.Point(481, 742);
		this.metin28_varsayilan_deger.Name = "metin28_varsayilan_deger";
		this.metin28_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin28_varsayilan_deger.TabIndex = 366;
		this.metin28_zorunlu.Location = new System.Drawing.Point(636, 742);
		this.metin28_zorunlu.Name = "metin28_zorunlu";
		this.metin28_zorunlu.Properties.Caption = "Zorunlu";
		this.metin28_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin28_zorunlu.TabIndex = 365;
		this.metin28_zorunlu.Visible = false;
		this.metin28_uzunluk.EditValue = new decimal(new int[4]);
		this.metin28_uzunluk.Location = new System.Drawing.Point(421, 742);
		this.metin28_uzunluk.Name = "metin28_uzunluk";
		this.metin28_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin28_uzunluk.Properties.IsFloatValue = false;
		this.metin28_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin28_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin28_uzunluk.TabIndex = 364;
		this.metin28_baslangic.EditValue = new decimal(new int[4]);
		this.metin28_baslangic.Location = new System.Drawing.Point(316, 742);
		this.metin28_baslangic.Name = "metin28_baslangic";
		this.metin28_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin28_baslangic.Properties.IsFloatValue = false;
		this.metin28_baslangic.Properties.Mask.EditMask = "N00";
		this.metin28_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin28_baslangic.TabIndex = 363;
		this.metin28_gorunen_adi.Location = new System.Drawing.Point(76, 742);
		this.metin28_gorunen_adi.Name = "metin28_gorunen_adi";
		this.metin28_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin28_gorunen_adi.TabIndex = 362;
		this.label61.Location = new System.Drawing.Point(9, 742);
		this.label61.Name = "label61";
		this.label61.Size = new System.Drawing.Size(61, 19);
		this.label61.TabIndex = 361;
		this.label61.Text = "Metin 28 :";
		this.label61.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin27_varsayilan_deger.Location = new System.Drawing.Point(481, 716);
		this.metin27_varsayilan_deger.Name = "metin27_varsayilan_deger";
		this.metin27_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin27_varsayilan_deger.TabIndex = 360;
		this.metin27_zorunlu.Location = new System.Drawing.Point(636, 716);
		this.metin27_zorunlu.Name = "metin27_zorunlu";
		this.metin27_zorunlu.Properties.Caption = "Zorunlu";
		this.metin27_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin27_zorunlu.TabIndex = 359;
		this.metin27_zorunlu.Visible = false;
		this.metin27_uzunluk.EditValue = new decimal(new int[4]);
		this.metin27_uzunluk.Location = new System.Drawing.Point(421, 716);
		this.metin27_uzunluk.Name = "metin27_uzunluk";
		this.metin27_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin27_uzunluk.Properties.IsFloatValue = false;
		this.metin27_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin27_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin27_uzunluk.TabIndex = 358;
		this.metin27_baslangic.EditValue = new decimal(new int[4]);
		this.metin27_baslangic.Location = new System.Drawing.Point(316, 716);
		this.metin27_baslangic.Name = "metin27_baslangic";
		this.metin27_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin27_baslangic.Properties.IsFloatValue = false;
		this.metin27_baslangic.Properties.Mask.EditMask = "N00";
		this.metin27_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin27_baslangic.TabIndex = 357;
		this.metin27_gorunen_adi.Location = new System.Drawing.Point(76, 716);
		this.metin27_gorunen_adi.Name = "metin27_gorunen_adi";
		this.metin27_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin27_gorunen_adi.TabIndex = 356;
		this.label60.Location = new System.Drawing.Point(9, 716);
		this.label60.Name = "label60";
		this.label60.Size = new System.Drawing.Size(61, 19);
		this.label60.TabIndex = 355;
		this.label60.Text = "Metin 27 :";
		this.label60.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin26_varsayilan_deger.Location = new System.Drawing.Point(481, 690);
		this.metin26_varsayilan_deger.Name = "metin26_varsayilan_deger";
		this.metin26_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin26_varsayilan_deger.TabIndex = 354;
		this.metin26_zorunlu.Location = new System.Drawing.Point(636, 690);
		this.metin26_zorunlu.Name = "metin26_zorunlu";
		this.metin26_zorunlu.Properties.Caption = "Zorunlu";
		this.metin26_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin26_zorunlu.TabIndex = 353;
		this.metin26_zorunlu.Visible = false;
		this.metin26_uzunluk.EditValue = new decimal(new int[4]);
		this.metin26_uzunluk.Location = new System.Drawing.Point(421, 690);
		this.metin26_uzunluk.Name = "metin26_uzunluk";
		this.metin26_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin26_uzunluk.Properties.IsFloatValue = false;
		this.metin26_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin26_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin26_uzunluk.TabIndex = 352;
		this.metin26_baslangic.EditValue = new decimal(new int[4]);
		this.metin26_baslangic.Location = new System.Drawing.Point(316, 690);
		this.metin26_baslangic.Name = "metin26_baslangic";
		this.metin26_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin26_baslangic.Properties.IsFloatValue = false;
		this.metin26_baslangic.Properties.Mask.EditMask = "N00";
		this.metin26_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin26_baslangic.TabIndex = 351;
		this.metin26_gorunen_adi.Location = new System.Drawing.Point(76, 690);
		this.metin26_gorunen_adi.Name = "metin26_gorunen_adi";
		this.metin26_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin26_gorunen_adi.TabIndex = 350;
		this.label59.Location = new System.Drawing.Point(9, 690);
		this.label59.Name = "label59";
		this.label59.Size = new System.Drawing.Size(61, 19);
		this.label59.TabIndex = 349;
		this.label59.Text = "Metin 26 :";
		this.label59.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin25_varsayilan_deger.Location = new System.Drawing.Point(481, 664);
		this.metin25_varsayilan_deger.Name = "metin25_varsayilan_deger";
		this.metin25_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin25_varsayilan_deger.TabIndex = 348;
		this.metin25_zorunlu.Location = new System.Drawing.Point(636, 664);
		this.metin25_zorunlu.Name = "metin25_zorunlu";
		this.metin25_zorunlu.Properties.Caption = "Zorunlu";
		this.metin25_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin25_zorunlu.TabIndex = 347;
		this.metin25_zorunlu.Visible = false;
		this.metin25_uzunluk.EditValue = new decimal(new int[4]);
		this.metin25_uzunluk.Location = new System.Drawing.Point(421, 664);
		this.metin25_uzunluk.Name = "metin25_uzunluk";
		this.metin25_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin25_uzunluk.Properties.IsFloatValue = false;
		this.metin25_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin25_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin25_uzunluk.TabIndex = 346;
		this.metin25_baslangic.EditValue = new decimal(new int[4]);
		this.metin25_baslangic.Location = new System.Drawing.Point(316, 664);
		this.metin25_baslangic.Name = "metin25_baslangic";
		this.metin25_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin25_baslangic.Properties.IsFloatValue = false;
		this.metin25_baslangic.Properties.Mask.EditMask = "N00";
		this.metin25_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin25_baslangic.TabIndex = 345;
		this.metin25_gorunen_adi.Location = new System.Drawing.Point(76, 664);
		this.metin25_gorunen_adi.Name = "metin25_gorunen_adi";
		this.metin25_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin25_gorunen_adi.TabIndex = 344;
		this.label58.Location = new System.Drawing.Point(9, 664);
		this.label58.Name = "label58";
		this.label58.Size = new System.Drawing.Size(61, 19);
		this.label58.TabIndex = 343;
		this.label58.Text = "Metin 25 :";
		this.label58.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin24_varsayilan_deger.Location = new System.Drawing.Point(481, 638);
		this.metin24_varsayilan_deger.Name = "metin24_varsayilan_deger";
		this.metin24_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin24_varsayilan_deger.TabIndex = 342;
		this.metin24_zorunlu.Location = new System.Drawing.Point(636, 638);
		this.metin24_zorunlu.Name = "metin24_zorunlu";
		this.metin24_zorunlu.Properties.Caption = "Zorunlu";
		this.metin24_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin24_zorunlu.TabIndex = 341;
		this.metin24_zorunlu.Visible = false;
		this.metin24_uzunluk.EditValue = new decimal(new int[4]);
		this.metin24_uzunluk.Location = new System.Drawing.Point(421, 638);
		this.metin24_uzunluk.Name = "metin24_uzunluk";
		this.metin24_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin24_uzunluk.Properties.IsFloatValue = false;
		this.metin24_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin24_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin24_uzunluk.TabIndex = 340;
		this.metin24_baslangic.EditValue = new decimal(new int[4]);
		this.metin24_baslangic.Location = new System.Drawing.Point(316, 638);
		this.metin24_baslangic.Name = "metin24_baslangic";
		this.metin24_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin24_baslangic.Properties.IsFloatValue = false;
		this.metin24_baslangic.Properties.Mask.EditMask = "N00";
		this.metin24_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin24_baslangic.TabIndex = 339;
		this.metin24_gorunen_adi.Location = new System.Drawing.Point(76, 638);
		this.metin24_gorunen_adi.Name = "metin24_gorunen_adi";
		this.metin24_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin24_gorunen_adi.TabIndex = 338;
		this.label57.Location = new System.Drawing.Point(9, 638);
		this.label57.Name = "label57";
		this.label57.Size = new System.Drawing.Size(61, 19);
		this.label57.TabIndex = 337;
		this.label57.Text = "Metin 24 :";
		this.label57.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin23_varsayilan_deger.Location = new System.Drawing.Point(481, 612);
		this.metin23_varsayilan_deger.Name = "metin23_varsayilan_deger";
		this.metin23_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin23_varsayilan_deger.TabIndex = 336;
		this.metin23_zorunlu.Location = new System.Drawing.Point(636, 612);
		this.metin23_zorunlu.Name = "metin23_zorunlu";
		this.metin23_zorunlu.Properties.Caption = "Zorunlu";
		this.metin23_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin23_zorunlu.TabIndex = 335;
		this.metin23_zorunlu.Visible = false;
		this.metin23_uzunluk.EditValue = new decimal(new int[4]);
		this.metin23_uzunluk.Location = new System.Drawing.Point(421, 612);
		this.metin23_uzunluk.Name = "metin23_uzunluk";
		this.metin23_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin23_uzunluk.Properties.IsFloatValue = false;
		this.metin23_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin23_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin23_uzunluk.TabIndex = 334;
		this.metin23_baslangic.EditValue = new decimal(new int[4]);
		this.metin23_baslangic.Location = new System.Drawing.Point(316, 612);
		this.metin23_baslangic.Name = "metin23_baslangic";
		this.metin23_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin23_baslangic.Properties.IsFloatValue = false;
		this.metin23_baslangic.Properties.Mask.EditMask = "N00";
		this.metin23_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin23_baslangic.TabIndex = 333;
		this.metin23_gorunen_adi.Location = new System.Drawing.Point(76, 612);
		this.metin23_gorunen_adi.Name = "metin23_gorunen_adi";
		this.metin23_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin23_gorunen_adi.TabIndex = 332;
		this.label56.Location = new System.Drawing.Point(9, 612);
		this.label56.Name = "label56";
		this.label56.Size = new System.Drawing.Size(61, 19);
		this.label56.TabIndex = 331;
		this.label56.Text = "Metin 23 :";
		this.label56.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin22_varsayilan_deger.Location = new System.Drawing.Point(481, 586);
		this.metin22_varsayilan_deger.Name = "metin22_varsayilan_deger";
		this.metin22_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin22_varsayilan_deger.TabIndex = 330;
		this.metin22_zorunlu.Location = new System.Drawing.Point(636, 586);
		this.metin22_zorunlu.Name = "metin22_zorunlu";
		this.metin22_zorunlu.Properties.Caption = "Zorunlu";
		this.metin22_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin22_zorunlu.TabIndex = 329;
		this.metin22_zorunlu.Visible = false;
		this.metin22_uzunluk.EditValue = new decimal(new int[4]);
		this.metin22_uzunluk.Location = new System.Drawing.Point(421, 586);
		this.metin22_uzunluk.Name = "metin22_uzunluk";
		this.metin22_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin22_uzunluk.Properties.IsFloatValue = false;
		this.metin22_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin22_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin22_uzunluk.TabIndex = 328;
		this.metin22_baslangic.EditValue = new decimal(new int[4]);
		this.metin22_baslangic.Location = new System.Drawing.Point(316, 586);
		this.metin22_baslangic.Name = "metin22_baslangic";
		this.metin22_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin22_baslangic.Properties.IsFloatValue = false;
		this.metin22_baslangic.Properties.Mask.EditMask = "N00";
		this.metin22_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin22_baslangic.TabIndex = 327;
		this.metin22_gorunen_adi.Location = new System.Drawing.Point(76, 586);
		this.metin22_gorunen_adi.Name = "metin22_gorunen_adi";
		this.metin22_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin22_gorunen_adi.TabIndex = 326;
		this.label55.Location = new System.Drawing.Point(9, 586);
		this.label55.Name = "label55";
		this.label55.Size = new System.Drawing.Size(61, 19);
		this.label55.TabIndex = 325;
		this.label55.Text = "Metin 22 :";
		this.label55.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin21_varsayilan_deger.Location = new System.Drawing.Point(481, 560);
		this.metin21_varsayilan_deger.Name = "metin21_varsayilan_deger";
		this.metin21_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin21_varsayilan_deger.TabIndex = 324;
		this.metin21_zorunlu.Location = new System.Drawing.Point(636, 560);
		this.metin21_zorunlu.Name = "metin21_zorunlu";
		this.metin21_zorunlu.Properties.Caption = "Zorunlu";
		this.metin21_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin21_zorunlu.TabIndex = 323;
		this.metin21_zorunlu.Visible = false;
		this.metin21_uzunluk.EditValue = new decimal(new int[4]);
		this.metin21_uzunluk.Location = new System.Drawing.Point(421, 560);
		this.metin21_uzunluk.Name = "metin21_uzunluk";
		this.metin21_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin21_uzunluk.Properties.IsFloatValue = false;
		this.metin21_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin21_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin21_uzunluk.TabIndex = 322;
		this.metin21_baslangic.EditValue = new decimal(new int[4]);
		this.metin21_baslangic.Location = new System.Drawing.Point(316, 560);
		this.metin21_baslangic.Name = "metin21_baslangic";
		this.metin21_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin21_baslangic.Properties.IsFloatValue = false;
		this.metin21_baslangic.Properties.Mask.EditMask = "N00";
		this.metin21_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin21_baslangic.TabIndex = 321;
		this.metin21_gorunen_adi.Location = new System.Drawing.Point(76, 560);
		this.metin21_gorunen_adi.Name = "metin21_gorunen_adi";
		this.metin21_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin21_gorunen_adi.TabIndex = 320;
		this.label54.Location = new System.Drawing.Point(9, 560);
		this.label54.Name = "label54";
		this.label54.Size = new System.Drawing.Size(61, 19);
		this.label54.TabIndex = 319;
		this.label54.Text = "Metin 21 :";
		this.label54.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin20_varsayilan_deger.Location = new System.Drawing.Point(481, 534);
		this.metin20_varsayilan_deger.Name = "metin20_varsayilan_deger";
		this.metin20_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin20_varsayilan_deger.TabIndex = 318;
		this.metin20_zorunlu.Location = new System.Drawing.Point(636, 534);
		this.metin20_zorunlu.Name = "metin20_zorunlu";
		this.metin20_zorunlu.Properties.Caption = "Zorunlu";
		this.metin20_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin20_zorunlu.TabIndex = 317;
		this.metin20_zorunlu.Visible = false;
		this.metin20_uzunluk.EditValue = new decimal(new int[4]);
		this.metin20_uzunluk.Location = new System.Drawing.Point(421, 534);
		this.metin20_uzunluk.Name = "metin20_uzunluk";
		this.metin20_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin20_uzunluk.Properties.IsFloatValue = false;
		this.metin20_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin20_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin20_uzunluk.TabIndex = 316;
		this.metin20_baslangic.EditValue = new decimal(new int[4]);
		this.metin20_baslangic.Location = new System.Drawing.Point(316, 534);
		this.metin20_baslangic.Name = "metin20_baslangic";
		this.metin20_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin20_baslangic.Properties.IsFloatValue = false;
		this.metin20_baslangic.Properties.Mask.EditMask = "N00";
		this.metin20_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin20_baslangic.TabIndex = 315;
		this.metin20_gorunen_adi.Location = new System.Drawing.Point(76, 534);
		this.metin20_gorunen_adi.Name = "metin20_gorunen_adi";
		this.metin20_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin20_gorunen_adi.TabIndex = 314;
		this.label53.Location = new System.Drawing.Point(9, 534);
		this.label53.Name = "label53";
		this.label53.Size = new System.Drawing.Size(61, 19);
		this.label53.TabIndex = 313;
		this.label53.Text = "Metin 20 :";
		this.label53.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin19_varsayilan_deger.Location = new System.Drawing.Point(481, 508);
		this.metin19_varsayilan_deger.Name = "metin19_varsayilan_deger";
		this.metin19_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin19_varsayilan_deger.TabIndex = 312;
		this.metin19_zorunlu.Location = new System.Drawing.Point(636, 508);
		this.metin19_zorunlu.Name = "metin19_zorunlu";
		this.metin19_zorunlu.Properties.Caption = "Zorunlu";
		this.metin19_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin19_zorunlu.TabIndex = 311;
		this.metin19_zorunlu.Visible = false;
		this.metin19_uzunluk.EditValue = new decimal(new int[4]);
		this.metin19_uzunluk.Location = new System.Drawing.Point(421, 508);
		this.metin19_uzunluk.Name = "metin19_uzunluk";
		this.metin19_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin19_uzunluk.Properties.IsFloatValue = false;
		this.metin19_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin19_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin19_uzunluk.TabIndex = 310;
		this.metin19_baslangic.EditValue = new decimal(new int[4]);
		this.metin19_baslangic.Location = new System.Drawing.Point(316, 508);
		this.metin19_baslangic.Name = "metin19_baslangic";
		this.metin19_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin19_baslangic.Properties.IsFloatValue = false;
		this.metin19_baslangic.Properties.Mask.EditMask = "N00";
		this.metin19_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin19_baslangic.TabIndex = 309;
		this.metin19_gorunen_adi.Location = new System.Drawing.Point(76, 508);
		this.metin19_gorunen_adi.Name = "metin19_gorunen_adi";
		this.metin19_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin19_gorunen_adi.TabIndex = 308;
		this.label52.Location = new System.Drawing.Point(9, 508);
		this.label52.Name = "label52";
		this.label52.Size = new System.Drawing.Size(61, 19);
		this.label52.TabIndex = 307;
		this.label52.Text = "Metin 19 :";
		this.label52.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin18_varsayilan_deger.Location = new System.Drawing.Point(481, 482);
		this.metin18_varsayilan_deger.Name = "metin18_varsayilan_deger";
		this.metin18_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin18_varsayilan_deger.TabIndex = 306;
		this.metin18_zorunlu.Location = new System.Drawing.Point(636, 482);
		this.metin18_zorunlu.Name = "metin18_zorunlu";
		this.metin18_zorunlu.Properties.Caption = "Zorunlu";
		this.metin18_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin18_zorunlu.TabIndex = 305;
		this.metin18_zorunlu.Visible = false;
		this.metin18_uzunluk.EditValue = new decimal(new int[4]);
		this.metin18_uzunluk.Location = new System.Drawing.Point(421, 482);
		this.metin18_uzunluk.Name = "metin18_uzunluk";
		this.metin18_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin18_uzunluk.Properties.IsFloatValue = false;
		this.metin18_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin18_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin18_uzunluk.TabIndex = 304;
		this.metin18_baslangic.EditValue = new decimal(new int[4]);
		this.metin18_baslangic.Location = new System.Drawing.Point(316, 482);
		this.metin18_baslangic.Name = "metin18_baslangic";
		this.metin18_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin18_baslangic.Properties.IsFloatValue = false;
		this.metin18_baslangic.Properties.Mask.EditMask = "N00";
		this.metin18_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin18_baslangic.TabIndex = 303;
		this.metin18_gorunen_adi.Location = new System.Drawing.Point(76, 482);
		this.metin18_gorunen_adi.Name = "metin18_gorunen_adi";
		this.metin18_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin18_gorunen_adi.TabIndex = 302;
		this.label51.Location = new System.Drawing.Point(9, 482);
		this.label51.Name = "label51";
		this.label51.Size = new System.Drawing.Size(61, 19);
		this.label51.TabIndex = 301;
		this.label51.Text = "Metin 18 :";
		this.label51.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin17_varsayilan_deger.Location = new System.Drawing.Point(481, 456);
		this.metin17_varsayilan_deger.Name = "metin17_varsayilan_deger";
		this.metin17_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin17_varsayilan_deger.TabIndex = 300;
		this.metin17_zorunlu.Location = new System.Drawing.Point(636, 456);
		this.metin17_zorunlu.Name = "metin17_zorunlu";
		this.metin17_zorunlu.Properties.Caption = "Zorunlu";
		this.metin17_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin17_zorunlu.TabIndex = 299;
		this.metin17_zorunlu.Visible = false;
		this.metin17_uzunluk.EditValue = new decimal(new int[4]);
		this.metin17_uzunluk.Location = new System.Drawing.Point(421, 456);
		this.metin17_uzunluk.Name = "metin17_uzunluk";
		this.metin17_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin17_uzunluk.Properties.IsFloatValue = false;
		this.metin17_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin17_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin17_uzunluk.TabIndex = 298;
		this.metin17_baslangic.EditValue = new decimal(new int[4]);
		this.metin17_baslangic.Location = new System.Drawing.Point(316, 456);
		this.metin17_baslangic.Name = "metin17_baslangic";
		this.metin17_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin17_baslangic.Properties.IsFloatValue = false;
		this.metin17_baslangic.Properties.Mask.EditMask = "N00";
		this.metin17_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin17_baslangic.TabIndex = 297;
		this.metin17_gorunen_adi.Location = new System.Drawing.Point(76, 456);
		this.metin17_gorunen_adi.Name = "metin17_gorunen_adi";
		this.metin17_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin17_gorunen_adi.TabIndex = 296;
		this.label50.Location = new System.Drawing.Point(9, 456);
		this.label50.Name = "label50";
		this.label50.Size = new System.Drawing.Size(61, 19);
		this.label50.TabIndex = 295;
		this.label50.Text = "Metin 17 :";
		this.label50.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin16_varsayilan_deger.Location = new System.Drawing.Point(481, 430);
		this.metin16_varsayilan_deger.Name = "metin16_varsayilan_deger";
		this.metin16_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin16_varsayilan_deger.TabIndex = 294;
		this.metin16_zorunlu.Location = new System.Drawing.Point(636, 430);
		this.metin16_zorunlu.Name = "metin16_zorunlu";
		this.metin16_zorunlu.Properties.Caption = "Zorunlu";
		this.metin16_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin16_zorunlu.TabIndex = 293;
		this.metin16_zorunlu.Visible = false;
		this.metin16_uzunluk.EditValue = new decimal(new int[4]);
		this.metin16_uzunluk.Location = new System.Drawing.Point(421, 430);
		this.metin16_uzunluk.Name = "metin16_uzunluk";
		this.metin16_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin16_uzunluk.Properties.IsFloatValue = false;
		this.metin16_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin16_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin16_uzunluk.TabIndex = 292;
		this.metin16_baslangic.EditValue = new decimal(new int[4]);
		this.metin16_baslangic.Location = new System.Drawing.Point(316, 430);
		this.metin16_baslangic.Name = "metin16_baslangic";
		this.metin16_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin16_baslangic.Properties.IsFloatValue = false;
		this.metin16_baslangic.Properties.Mask.EditMask = "N00";
		this.metin16_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin16_baslangic.TabIndex = 291;
		this.metin16_gorunen_adi.Location = new System.Drawing.Point(76, 430);
		this.metin16_gorunen_adi.Name = "metin16_gorunen_adi";
		this.metin16_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin16_gorunen_adi.TabIndex = 290;
		this.label49.Location = new System.Drawing.Point(9, 430);
		this.label49.Name = "label49";
		this.label49.Size = new System.Drawing.Size(61, 19);
		this.label49.TabIndex = 289;
		this.label49.Text = "Metin 16 :";
		this.label49.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin15_varsayilan_deger.Location = new System.Drawing.Point(481, 404);
		this.metin15_varsayilan_deger.Name = "metin15_varsayilan_deger";
		this.metin15_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin15_varsayilan_deger.TabIndex = 288;
		this.metin15_zorunlu.Location = new System.Drawing.Point(636, 404);
		this.metin15_zorunlu.Name = "metin15_zorunlu";
		this.metin15_zorunlu.Properties.Caption = "Zorunlu";
		this.metin15_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin15_zorunlu.TabIndex = 287;
		this.metin15_zorunlu.Visible = false;
		this.metin15_uzunluk.EditValue = new decimal(new int[4]);
		this.metin15_uzunluk.Location = new System.Drawing.Point(421, 404);
		this.metin15_uzunluk.Name = "metin15_uzunluk";
		this.metin15_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin15_uzunluk.Properties.IsFloatValue = false;
		this.metin15_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin15_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin15_uzunluk.TabIndex = 286;
		this.metin15_baslangic.EditValue = new decimal(new int[4]);
		this.metin15_baslangic.Location = new System.Drawing.Point(316, 404);
		this.metin15_baslangic.Name = "metin15_baslangic";
		this.metin15_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin15_baslangic.Properties.IsFloatValue = false;
		this.metin15_baslangic.Properties.Mask.EditMask = "N00";
		this.metin15_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin15_baslangic.TabIndex = 285;
		this.metin15_gorunen_adi.Location = new System.Drawing.Point(76, 404);
		this.metin15_gorunen_adi.Name = "metin15_gorunen_adi";
		this.metin15_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin15_gorunen_adi.TabIndex = 284;
		this.label48.Location = new System.Drawing.Point(9, 404);
		this.label48.Name = "label48";
		this.label48.Size = new System.Drawing.Size(61, 19);
		this.label48.TabIndex = 283;
		this.label48.Text = "Metin 15 :";
		this.label48.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin14_varsayilan_deger.Location = new System.Drawing.Point(481, 378);
		this.metin14_varsayilan_deger.Name = "metin14_varsayilan_deger";
		this.metin14_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin14_varsayilan_deger.TabIndex = 282;
		this.metin14_zorunlu.Location = new System.Drawing.Point(636, 378);
		this.metin14_zorunlu.Name = "metin14_zorunlu";
		this.metin14_zorunlu.Properties.Caption = "Zorunlu";
		this.metin14_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin14_zorunlu.TabIndex = 281;
		this.metin14_zorunlu.Visible = false;
		this.metin14_uzunluk.EditValue = new decimal(new int[4]);
		this.metin14_uzunluk.Location = new System.Drawing.Point(421, 378);
		this.metin14_uzunluk.Name = "metin14_uzunluk";
		this.metin14_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin14_uzunluk.Properties.IsFloatValue = false;
		this.metin14_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin14_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin14_uzunluk.TabIndex = 280;
		this.metin14_baslangic.EditValue = new decimal(new int[4]);
		this.metin14_baslangic.Location = new System.Drawing.Point(316, 378);
		this.metin14_baslangic.Name = "metin14_baslangic";
		this.metin14_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin14_baslangic.Properties.IsFloatValue = false;
		this.metin14_baslangic.Properties.Mask.EditMask = "N00";
		this.metin14_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin14_baslangic.TabIndex = 279;
		this.metin14_gorunen_adi.Location = new System.Drawing.Point(76, 378);
		this.metin14_gorunen_adi.Name = "metin14_gorunen_adi";
		this.metin14_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin14_gorunen_adi.TabIndex = 278;
		this.label47.Location = new System.Drawing.Point(9, 378);
		this.label47.Name = "label47";
		this.label47.Size = new System.Drawing.Size(61, 19);
		this.label47.TabIndex = 277;
		this.label47.Text = "Metin 14 :";
		this.label47.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin13_varsayilan_deger.Location = new System.Drawing.Point(481, 352);
		this.metin13_varsayilan_deger.Name = "metin13_varsayilan_deger";
		this.metin13_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin13_varsayilan_deger.TabIndex = 276;
		this.metin13_zorunlu.Location = new System.Drawing.Point(636, 352);
		this.metin13_zorunlu.Name = "metin13_zorunlu";
		this.metin13_zorunlu.Properties.Caption = "Zorunlu";
		this.metin13_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin13_zorunlu.TabIndex = 275;
		this.metin13_zorunlu.Visible = false;
		this.metin13_uzunluk.EditValue = new decimal(new int[4]);
		this.metin13_uzunluk.Location = new System.Drawing.Point(421, 352);
		this.metin13_uzunluk.Name = "metin13_uzunluk";
		this.metin13_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin13_uzunluk.Properties.IsFloatValue = false;
		this.metin13_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin13_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin13_uzunluk.TabIndex = 274;
		this.metin13_baslangic.EditValue = new decimal(new int[4]);
		this.metin13_baslangic.Location = new System.Drawing.Point(316, 352);
		this.metin13_baslangic.Name = "metin13_baslangic";
		this.metin13_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin13_baslangic.Properties.IsFloatValue = false;
		this.metin13_baslangic.Properties.Mask.EditMask = "N00";
		this.metin13_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin13_baslangic.TabIndex = 273;
		this.metin13_gorunen_adi.Location = new System.Drawing.Point(76, 352);
		this.metin13_gorunen_adi.Name = "metin13_gorunen_adi";
		this.metin13_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin13_gorunen_adi.TabIndex = 272;
		this.label46.Location = new System.Drawing.Point(9, 352);
		this.label46.Name = "label46";
		this.label46.Size = new System.Drawing.Size(61, 19);
		this.label46.TabIndex = 271;
		this.label46.Text = "Metin 13 :";
		this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin12_varsayilan_deger.Location = new System.Drawing.Point(481, 326);
		this.metin12_varsayilan_deger.Name = "metin12_varsayilan_deger";
		this.metin12_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin12_varsayilan_deger.TabIndex = 270;
		this.metin12_zorunlu.Location = new System.Drawing.Point(636, 326);
		this.metin12_zorunlu.Name = "metin12_zorunlu";
		this.metin12_zorunlu.Properties.Caption = "Zorunlu";
		this.metin12_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin12_zorunlu.TabIndex = 269;
		this.metin12_zorunlu.Visible = false;
		this.metin12_uzunluk.EditValue = new decimal(new int[4]);
		this.metin12_uzunluk.Location = new System.Drawing.Point(421, 326);
		this.metin12_uzunluk.Name = "metin12_uzunluk";
		this.metin12_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin12_uzunluk.Properties.IsFloatValue = false;
		this.metin12_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin12_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin12_uzunluk.TabIndex = 268;
		this.metin12_baslangic.EditValue = new decimal(new int[4]);
		this.metin12_baslangic.Location = new System.Drawing.Point(316, 326);
		this.metin12_baslangic.Name = "metin12_baslangic";
		this.metin12_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin12_baslangic.Properties.IsFloatValue = false;
		this.metin12_baslangic.Properties.Mask.EditMask = "N00";
		this.metin12_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin12_baslangic.TabIndex = 267;
		this.metin12_gorunen_adi.Location = new System.Drawing.Point(76, 326);
		this.metin12_gorunen_adi.Name = "metin12_gorunen_adi";
		this.metin12_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin12_gorunen_adi.TabIndex = 266;
		this.label45.Location = new System.Drawing.Point(9, 326);
		this.label45.Name = "label45";
		this.label45.Size = new System.Drawing.Size(61, 19);
		this.label45.TabIndex = 265;
		this.label45.Text = "Metin 12 :";
		this.label45.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin11_varsayilan_deger.Location = new System.Drawing.Point(481, 300);
		this.metin11_varsayilan_deger.Name = "metin11_varsayilan_deger";
		this.metin11_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin11_varsayilan_deger.TabIndex = 264;
		this.metin11_zorunlu.Location = new System.Drawing.Point(636, 300);
		this.metin11_zorunlu.Name = "metin11_zorunlu";
		this.metin11_zorunlu.Properties.Caption = "Zorunlu";
		this.metin11_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin11_zorunlu.TabIndex = 263;
		this.metin11_zorunlu.Visible = false;
		this.metin11_uzunluk.EditValue = new decimal(new int[4]);
		this.metin11_uzunluk.Location = new System.Drawing.Point(421, 300);
		this.metin11_uzunluk.Name = "metin11_uzunluk";
		this.metin11_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin11_uzunluk.Properties.IsFloatValue = false;
		this.metin11_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin11_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin11_uzunluk.TabIndex = 262;
		this.metin11_baslangic.EditValue = new decimal(new int[4]);
		this.metin11_baslangic.Location = new System.Drawing.Point(316, 300);
		this.metin11_baslangic.Name = "metin11_baslangic";
		this.metin11_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin11_baslangic.Properties.IsFloatValue = false;
		this.metin11_baslangic.Properties.Mask.EditMask = "N00";
		this.metin11_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin11_baslangic.TabIndex = 261;
		this.metin11_gorunen_adi.Location = new System.Drawing.Point(76, 300);
		this.metin11_gorunen_adi.Name = "metin11_gorunen_adi";
		this.metin11_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin11_gorunen_adi.TabIndex = 260;
		this.label31.Location = new System.Drawing.Point(9, 300);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(61, 19);
		this.label31.TabIndex = 259;
		this.label31.Text = "Metin 11 :";
		this.label31.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin10_varsayilan_deger.Location = new System.Drawing.Point(481, 273);
		this.metin10_varsayilan_deger.Name = "metin10_varsayilan_deger";
		this.metin10_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin10_varsayilan_deger.TabIndex = 258;
		this.metin10_zorunlu.Location = new System.Drawing.Point(636, 273);
		this.metin10_zorunlu.Name = "metin10_zorunlu";
		this.metin10_zorunlu.Properties.Caption = "Zorunlu";
		this.metin10_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin10_zorunlu.TabIndex = 257;
		this.metin10_zorunlu.Visible = false;
		this.metin10_uzunluk.EditValue = new decimal(new int[4]);
		this.metin10_uzunluk.Location = new System.Drawing.Point(421, 273);
		this.metin10_uzunluk.Name = "metin10_uzunluk";
		this.metin10_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin10_uzunluk.Properties.IsFloatValue = false;
		this.metin10_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin10_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin10_uzunluk.TabIndex = 256;
		this.metin10_baslangic.EditValue = new decimal(new int[4]);
		this.metin10_baslangic.Location = new System.Drawing.Point(316, 273);
		this.metin10_baslangic.Name = "metin10_baslangic";
		this.metin10_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin10_baslangic.Properties.IsFloatValue = false;
		this.metin10_baslangic.Properties.Mask.EditMask = "N00";
		this.metin10_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin10_baslangic.TabIndex = 255;
		this.metin10_gorunen_adi.Location = new System.Drawing.Point(76, 273);
		this.metin10_gorunen_adi.Name = "metin10_gorunen_adi";
		this.metin10_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin10_gorunen_adi.TabIndex = 254;
		this.label22.Location = new System.Drawing.Point(9, 273);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(61, 19);
		this.label22.TabIndex = 253;
		this.label22.Text = "Metin 10 :";
		this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin9_varsayilan_deger.Location = new System.Drawing.Point(481, 247);
		this.metin9_varsayilan_deger.Name = "metin9_varsayilan_deger";
		this.metin9_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin9_varsayilan_deger.TabIndex = 252;
		this.metin9_zorunlu.Location = new System.Drawing.Point(636, 247);
		this.metin9_zorunlu.Name = "metin9_zorunlu";
		this.metin9_zorunlu.Properties.Caption = "Zorunlu";
		this.metin9_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin9_zorunlu.TabIndex = 251;
		this.metin9_zorunlu.Visible = false;
		this.metin9_uzunluk.EditValue = new decimal(new int[4]);
		this.metin9_uzunluk.Location = new System.Drawing.Point(421, 247);
		this.metin9_uzunluk.Name = "metin9_uzunluk";
		this.metin9_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin9_uzunluk.Properties.IsFloatValue = false;
		this.metin9_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin9_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin9_uzunluk.TabIndex = 250;
		this.metin9_baslangic.EditValue = new decimal(new int[4]);
		this.metin9_baslangic.Location = new System.Drawing.Point(316, 247);
		this.metin9_baslangic.Name = "metin9_baslangic";
		this.metin9_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin9_baslangic.Properties.IsFloatValue = false;
		this.metin9_baslangic.Properties.Mask.EditMask = "N00";
		this.metin9_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin9_baslangic.TabIndex = 249;
		this.metin9_gorunen_adi.Location = new System.Drawing.Point(76, 247);
		this.metin9_gorunen_adi.Name = "metin9_gorunen_adi";
		this.metin9_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin9_gorunen_adi.TabIndex = 248;
		this.label21.Location = new System.Drawing.Point(9, 247);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(61, 19);
		this.label21.TabIndex = 247;
		this.label21.Text = "Metin 9 :";
		this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin8_varsayilan_deger.Location = new System.Drawing.Point(481, 221);
		this.metin8_varsayilan_deger.Name = "metin8_varsayilan_deger";
		this.metin8_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin8_varsayilan_deger.TabIndex = 246;
		this.metin8_zorunlu.Location = new System.Drawing.Point(636, 221);
		this.metin8_zorunlu.Name = "metin8_zorunlu";
		this.metin8_zorunlu.Properties.Caption = "Zorunlu";
		this.metin8_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin8_zorunlu.TabIndex = 245;
		this.metin8_zorunlu.Visible = false;
		this.metin8_uzunluk.EditValue = new decimal(new int[4]);
		this.metin8_uzunluk.Location = new System.Drawing.Point(421, 221);
		this.metin8_uzunluk.Name = "metin8_uzunluk";
		this.metin8_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin8_uzunluk.Properties.IsFloatValue = false;
		this.metin8_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin8_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin8_uzunluk.TabIndex = 244;
		this.metin8_baslangic.EditValue = new decimal(new int[4]);
		this.metin8_baslangic.Location = new System.Drawing.Point(316, 221);
		this.metin8_baslangic.Name = "metin8_baslangic";
		this.metin8_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin8_baslangic.Properties.IsFloatValue = false;
		this.metin8_baslangic.Properties.Mask.EditMask = "N00";
		this.metin8_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin8_baslangic.TabIndex = 243;
		this.metin8_gorunen_adi.Location = new System.Drawing.Point(76, 221);
		this.metin8_gorunen_adi.Name = "metin8_gorunen_adi";
		this.metin8_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin8_gorunen_adi.TabIndex = 242;
		this.label19.Location = new System.Drawing.Point(9, 221);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(61, 19);
		this.label19.TabIndex = 241;
		this.label19.Text = "Metin 8 :";
		this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin7_varsayilan_deger.Location = new System.Drawing.Point(481, 195);
		this.metin7_varsayilan_deger.Name = "metin7_varsayilan_deger";
		this.metin7_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin7_varsayilan_deger.TabIndex = 240;
		this.metin7_zorunlu.Location = new System.Drawing.Point(636, 195);
		this.metin7_zorunlu.Name = "metin7_zorunlu";
		this.metin7_zorunlu.Properties.Caption = "Zorunlu";
		this.metin7_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin7_zorunlu.TabIndex = 239;
		this.metin7_zorunlu.Visible = false;
		this.metin7_uzunluk.EditValue = new decimal(new int[4]);
		this.metin7_uzunluk.Location = new System.Drawing.Point(421, 195);
		this.metin7_uzunluk.Name = "metin7_uzunluk";
		this.metin7_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin7_uzunluk.Properties.IsFloatValue = false;
		this.metin7_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin7_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin7_uzunluk.TabIndex = 238;
		this.metin7_baslangic.EditValue = new decimal(new int[4]);
		this.metin7_baslangic.Location = new System.Drawing.Point(316, 195);
		this.metin7_baslangic.Name = "metin7_baslangic";
		this.metin7_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin7_baslangic.Properties.IsFloatValue = false;
		this.metin7_baslangic.Properties.Mask.EditMask = "N00";
		this.metin7_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin7_baslangic.TabIndex = 237;
		this.metin7_gorunen_adi.Location = new System.Drawing.Point(76, 195);
		this.metin7_gorunen_adi.Name = "metin7_gorunen_adi";
		this.metin7_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin7_gorunen_adi.TabIndex = 236;
		this.label13.Location = new System.Drawing.Point(9, 195);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(61, 19);
		this.label13.TabIndex = 235;
		this.label13.Text = "Metin 7 :";
		this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin6_varsayilan_deger.Location = new System.Drawing.Point(481, 169);
		this.metin6_varsayilan_deger.Name = "metin6_varsayilan_deger";
		this.metin6_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin6_varsayilan_deger.TabIndex = 234;
		this.metin6_zorunlu.Location = new System.Drawing.Point(636, 169);
		this.metin6_zorunlu.Name = "metin6_zorunlu";
		this.metin6_zorunlu.Properties.Caption = "Zorunlu";
		this.metin6_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin6_zorunlu.TabIndex = 233;
		this.metin6_zorunlu.Visible = false;
		this.metin6_uzunluk.EditValue = new decimal(new int[4]);
		this.metin6_uzunluk.Location = new System.Drawing.Point(421, 169);
		this.metin6_uzunluk.Name = "metin6_uzunluk";
		this.metin6_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin6_uzunluk.Properties.IsFloatValue = false;
		this.metin6_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin6_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin6_uzunluk.TabIndex = 232;
		this.metin6_baslangic.EditValue = new decimal(new int[4]);
		this.metin6_baslangic.Location = new System.Drawing.Point(316, 169);
		this.metin6_baslangic.Name = "metin6_baslangic";
		this.metin6_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin6_baslangic.Properties.IsFloatValue = false;
		this.metin6_baslangic.Properties.Mask.EditMask = "N00";
		this.metin6_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin6_baslangic.TabIndex = 231;
		this.metin6_gorunen_adi.Location = new System.Drawing.Point(76, 169);
		this.metin6_gorunen_adi.Name = "metin6_gorunen_adi";
		this.metin6_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin6_gorunen_adi.TabIndex = 230;
		this.label12.Location = new System.Drawing.Point(9, 169);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(61, 19);
		this.label12.TabIndex = 229;
		this.label12.Text = "Metin 6 :";
		this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin5_varsayilan_deger.Location = new System.Drawing.Point(481, 143);
		this.metin5_varsayilan_deger.Name = "metin5_varsayilan_deger";
		this.metin5_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin5_varsayilan_deger.TabIndex = 228;
		this.metin5_zorunlu.Location = new System.Drawing.Point(636, 143);
		this.metin5_zorunlu.Name = "metin5_zorunlu";
		this.metin5_zorunlu.Properties.Caption = "Zorunlu";
		this.metin5_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin5_zorunlu.TabIndex = 227;
		this.metin5_zorunlu.Visible = false;
		this.metin5_uzunluk.EditValue = new decimal(new int[4]);
		this.metin5_uzunluk.Location = new System.Drawing.Point(421, 143);
		this.metin5_uzunluk.Name = "metin5_uzunluk";
		this.metin5_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin5_uzunluk.Properties.IsFloatValue = false;
		this.metin5_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin5_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin5_uzunluk.TabIndex = 226;
		this.metin5_baslangic.EditValue = new decimal(new int[4]);
		this.metin5_baslangic.Location = new System.Drawing.Point(316, 143);
		this.metin5_baslangic.Name = "metin5_baslangic";
		this.metin5_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin5_baslangic.Properties.IsFloatValue = false;
		this.metin5_baslangic.Properties.Mask.EditMask = "N00";
		this.metin5_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin5_baslangic.TabIndex = 225;
		this.metin5_gorunen_adi.Location = new System.Drawing.Point(76, 143);
		this.metin5_gorunen_adi.Name = "metin5_gorunen_adi";
		this.metin5_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin5_gorunen_adi.TabIndex = 224;
		this.label11.Location = new System.Drawing.Point(9, 143);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(61, 19);
		this.label11.TabIndex = 223;
		this.label11.Text = "Metin 5 :";
		this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin4_varsayilan_deger.Location = new System.Drawing.Point(481, 117);
		this.metin4_varsayilan_deger.Name = "metin4_varsayilan_deger";
		this.metin4_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin4_varsayilan_deger.TabIndex = 222;
		this.metin4_zorunlu.Location = new System.Drawing.Point(636, 117);
		this.metin4_zorunlu.Name = "metin4_zorunlu";
		this.metin4_zorunlu.Properties.Caption = "Zorunlu";
		this.metin4_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin4_zorunlu.TabIndex = 221;
		this.metin4_zorunlu.Visible = false;
		this.metin4_uzunluk.EditValue = new decimal(new int[4]);
		this.metin4_uzunluk.Location = new System.Drawing.Point(421, 117);
		this.metin4_uzunluk.Name = "metin4_uzunluk";
		this.metin4_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin4_uzunluk.Properties.IsFloatValue = false;
		this.metin4_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin4_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin4_uzunluk.TabIndex = 220;
		this.metin4_baslangic.EditValue = new decimal(new int[4]);
		this.metin4_baslangic.Location = new System.Drawing.Point(316, 117);
		this.metin4_baslangic.Name = "metin4_baslangic";
		this.metin4_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin4_baslangic.Properties.IsFloatValue = false;
		this.metin4_baslangic.Properties.Mask.EditMask = "N00";
		this.metin4_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin4_baslangic.TabIndex = 219;
		this.metin4_gorunen_adi.Location = new System.Drawing.Point(76, 117);
		this.metin4_gorunen_adi.Name = "metin4_gorunen_adi";
		this.metin4_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin4_gorunen_adi.TabIndex = 218;
		this.label10.Location = new System.Drawing.Point(9, 117);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(61, 19);
		this.label10.TabIndex = 217;
		this.label10.Text = "Metin 4 :";
		this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin3_varsayilan_deger.Location = new System.Drawing.Point(481, 91);
		this.metin3_varsayilan_deger.Name = "metin3_varsayilan_deger";
		this.metin3_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin3_varsayilan_deger.TabIndex = 216;
		this.metin3_zorunlu.Location = new System.Drawing.Point(636, 91);
		this.metin3_zorunlu.Name = "metin3_zorunlu";
		this.metin3_zorunlu.Properties.Caption = "Zorunlu";
		this.metin3_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin3_zorunlu.TabIndex = 215;
		this.metin3_zorunlu.Visible = false;
		this.metin3_uzunluk.EditValue = new decimal(new int[4]);
		this.metin3_uzunluk.Location = new System.Drawing.Point(421, 91);
		this.metin3_uzunluk.Name = "metin3_uzunluk";
		this.metin3_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin3_uzunluk.Properties.IsFloatValue = false;
		this.metin3_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin3_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin3_uzunluk.TabIndex = 214;
		this.metin3_baslangic.EditValue = new decimal(new int[4]);
		this.metin3_baslangic.Location = new System.Drawing.Point(316, 91);
		this.metin3_baslangic.Name = "metin3_baslangic";
		this.metin3_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin3_baslangic.Properties.IsFloatValue = false;
		this.metin3_baslangic.Properties.Mask.EditMask = "N00";
		this.metin3_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin3_baslangic.TabIndex = 213;
		this.metin3_gorunen_adi.Location = new System.Drawing.Point(76, 91);
		this.metin3_gorunen_adi.Name = "metin3_gorunen_adi";
		this.metin3_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin3_gorunen_adi.TabIndex = 212;
		this.label9.Location = new System.Drawing.Point(9, 91);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(61, 19);
		this.label9.TabIndex = 211;
		this.label9.Text = "Metin 3 :";
		this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin2_varsayilan_deger.Location = new System.Drawing.Point(481, 65);
		this.metin2_varsayilan_deger.Name = "metin2_varsayilan_deger";
		this.metin2_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin2_varsayilan_deger.TabIndex = 210;
		this.metin2_zorunlu.Location = new System.Drawing.Point(636, 65);
		this.metin2_zorunlu.Name = "metin2_zorunlu";
		this.metin2_zorunlu.Properties.Caption = "Zorunlu";
		this.metin2_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin2_zorunlu.TabIndex = 209;
		this.metin2_zorunlu.Visible = false;
		this.metin2_uzunluk.EditValue = new decimal(new int[4]);
		this.metin2_uzunluk.Location = new System.Drawing.Point(421, 65);
		this.metin2_uzunluk.Name = "metin2_uzunluk";
		this.metin2_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin2_uzunluk.Properties.IsFloatValue = false;
		this.metin2_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin2_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin2_uzunluk.TabIndex = 208;
		this.metin2_baslangic.EditValue = new decimal(new int[4]);
		this.metin2_baslangic.Location = new System.Drawing.Point(316, 65);
		this.metin2_baslangic.Name = "metin2_baslangic";
		this.metin2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin2_baslangic.Properties.IsFloatValue = false;
		this.metin2_baslangic.Properties.Mask.EditMask = "N00";
		this.metin2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin2_baslangic.TabIndex = 207;
		this.metin2_gorunen_adi.Location = new System.Drawing.Point(76, 65);
		this.metin2_gorunen_adi.Name = "metin2_gorunen_adi";
		this.metin2_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin2_gorunen_adi.TabIndex = 206;
		this.label7.Location = new System.Drawing.Point(9, 65);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(61, 19);
		this.label7.TabIndex = 205;
		this.label7.Text = "Metin 2 :";
		this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.metin1_varsayilan_deger.Location = new System.Drawing.Point(481, 39);
		this.metin1_varsayilan_deger.Name = "metin1_varsayilan_deger";
		this.metin1_varsayilan_deger.Size = new System.Drawing.Size(149, 20);
		this.metin1_varsayilan_deger.TabIndex = 204;
		this.label6.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label6.Location = new System.Drawing.Point(481, 16);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(149, 19);
		this.label6.TabIndex = 203;
		this.label6.Text = "Varsayılan değer";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.metin1_zorunlu.Location = new System.Drawing.Point(636, 39);
		this.metin1_zorunlu.Name = "metin1_zorunlu";
		this.metin1_zorunlu.Properties.Caption = "Zorunlu";
		this.metin1_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.metin1_zorunlu.TabIndex = 202;
		this.metin1_zorunlu.Visible = false;
		this.metin1_uzunluk.EditValue = new decimal(new int[4]);
		this.metin1_uzunluk.Location = new System.Drawing.Point(421, 39);
		this.metin1_uzunluk.Name = "metin1_uzunluk";
		this.metin1_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin1_uzunluk.Properties.IsFloatValue = false;
		this.metin1_uzunluk.Properties.Mask.EditMask = "N00";
		this.metin1_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.metin1_uzunluk.TabIndex = 201;
		this.metin1_baslangic.EditValue = new decimal(new int[4]);
		this.metin1_baslangic.Location = new System.Drawing.Point(316, 39);
		this.metin1_baslangic.Name = "metin1_baslangic";
		this.metin1_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.metin1_baslangic.Properties.IsFloatValue = false;
		this.metin1_baslangic.Properties.Mask.EditMask = "N00";
		this.metin1_baslangic.Size = new System.Drawing.Size(96, 20);
		this.metin1_baslangic.TabIndex = 200;
		this.metin1_gorunen_adi.Location = new System.Drawing.Point(76, 39);
		this.metin1_gorunen_adi.Name = "metin1_gorunen_adi";
		this.metin1_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.metin1_gorunen_adi.TabIndex = 199;
		this.label4.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label4.Location = new System.Drawing.Point(76, 16);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(234, 19);
		this.label4.TabIndex = 198;
		this.label4.Text = "Başlık";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label1.Location = new System.Drawing.Point(9, 39);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(61, 19);
		this.label1.TabIndex = 197;
		this.label1.Text = "Metin 1 :";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label2.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label2.Location = new System.Drawing.Point(418, 16);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(54, 19);
		this.label2.TabIndex = 196;
		this.label2.Text = "Uzunluk";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label3.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label3.Location = new System.Drawing.Point(316, 16);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(96, 19);
		this.label3.TabIndex = 195;
		this.label3.Text = "Başlangıç/Sıra";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.xtraTabPage4.AutoScroll = true;
		this.xtraTabPage4.AutoScrollMargin = new System.Drawing.Size(0, 300);
		this.xtraTabPage4.Controls.Add(this.label165);
		this.xtraTabPage4.Controls.Add(this.label166);
		this.xtraTabPage4.Controls.Add(this.dropbox10_varsayilan_deger);
		this.xtraTabPage4.Controls.Add(this.label167);
		this.xtraTabPage4.Controls.Add(this.dropbox10_secenekler_veri);
		this.xtraTabPage4.Controls.Add(this.label168);
		this.xtraTabPage4.Controls.Add(this.dropbox10_secenekler_yazi);
		this.xtraTabPage4.Controls.Add(this.label169);
		this.xtraTabPage4.Controls.Add(this.dropbox10_zorunlu);
		this.xtraTabPage4.Controls.Add(this.dropbox10_uzunluk);
		this.xtraTabPage4.Controls.Add(this.dropbox10_baslangic);
		this.xtraTabPage4.Controls.Add(this.dropbox10_gorunen_adi);
		this.xtraTabPage4.Controls.Add(this.label170);
		this.xtraTabPage4.Controls.Add(this.label171);
		this.xtraTabPage4.Controls.Add(this.label172);
		this.xtraTabPage4.Controls.Add(this.label173);
		this.xtraTabPage4.Controls.Add(this.label156);
		this.xtraTabPage4.Controls.Add(this.label157);
		this.xtraTabPage4.Controls.Add(this.dropbox9_varsayilan_deger);
		this.xtraTabPage4.Controls.Add(this.label158);
		this.xtraTabPage4.Controls.Add(this.dropbox9_secenekler_veri);
		this.xtraTabPage4.Controls.Add(this.label159);
		this.xtraTabPage4.Controls.Add(this.dropbox9_secenekler_yazi);
		this.xtraTabPage4.Controls.Add(this.label160);
		this.xtraTabPage4.Controls.Add(this.dropbox9_zorunlu);
		this.xtraTabPage4.Controls.Add(this.dropbox9_uzunluk);
		this.xtraTabPage4.Controls.Add(this.dropbox9_baslangic);
		this.xtraTabPage4.Controls.Add(this.dropbox9_gorunen_adi);
		this.xtraTabPage4.Controls.Add(this.label161);
		this.xtraTabPage4.Controls.Add(this.label162);
		this.xtraTabPage4.Controls.Add(this.label163);
		this.xtraTabPage4.Controls.Add(this.label164);
		this.xtraTabPage4.Controls.Add(this.label147);
		this.xtraTabPage4.Controls.Add(this.label148);
		this.xtraTabPage4.Controls.Add(this.dropbox8_varsayilan_deger);
		this.xtraTabPage4.Controls.Add(this.label149);
		this.xtraTabPage4.Controls.Add(this.dropbox8_secenekler_veri);
		this.xtraTabPage4.Controls.Add(this.label150);
		this.xtraTabPage4.Controls.Add(this.dropbox8_secenekler_yazi);
		this.xtraTabPage4.Controls.Add(this.label151);
		this.xtraTabPage4.Controls.Add(this.dropbox8_zorunlu);
		this.xtraTabPage4.Controls.Add(this.dropbox8_uzunluk);
		this.xtraTabPage4.Controls.Add(this.dropbox8_baslangic);
		this.xtraTabPage4.Controls.Add(this.dropbox8_gorunen_adi);
		this.xtraTabPage4.Controls.Add(this.label152);
		this.xtraTabPage4.Controls.Add(this.label153);
		this.xtraTabPage4.Controls.Add(this.label154);
		this.xtraTabPage4.Controls.Add(this.label155);
		this.xtraTabPage4.Controls.Add(this.label138);
		this.xtraTabPage4.Controls.Add(this.label139);
		this.xtraTabPage4.Controls.Add(this.dropbox7_varsayilan_deger);
		this.xtraTabPage4.Controls.Add(this.label140);
		this.xtraTabPage4.Controls.Add(this.dropbox7_secenekler_veri);
		this.xtraTabPage4.Controls.Add(this.label141);
		this.xtraTabPage4.Controls.Add(this.dropbox7_secenekler_yazi);
		this.xtraTabPage4.Controls.Add(this.label142);
		this.xtraTabPage4.Controls.Add(this.dropbox7_zorunlu);
		this.xtraTabPage4.Controls.Add(this.dropbox7_uzunluk);
		this.xtraTabPage4.Controls.Add(this.dropbox7_baslangic);
		this.xtraTabPage4.Controls.Add(this.dropbox7_gorunen_adi);
		this.xtraTabPage4.Controls.Add(this.label143);
		this.xtraTabPage4.Controls.Add(this.label144);
		this.xtraTabPage4.Controls.Add(this.label145);
		this.xtraTabPage4.Controls.Add(this.label146);
		this.xtraTabPage4.Controls.Add(this.label129);
		this.xtraTabPage4.Controls.Add(this.label130);
		this.xtraTabPage4.Controls.Add(this.dropbox6_varsayilan_deger);
		this.xtraTabPage4.Controls.Add(this.label131);
		this.xtraTabPage4.Controls.Add(this.dropbox6_secenekler_veri);
		this.xtraTabPage4.Controls.Add(this.label132);
		this.xtraTabPage4.Controls.Add(this.dropbox6_secenekler_yazi);
		this.xtraTabPage4.Controls.Add(this.label133);
		this.xtraTabPage4.Controls.Add(this.dropbox6_zorunlu);
		this.xtraTabPage4.Controls.Add(this.dropbox6_uzunluk);
		this.xtraTabPage4.Controls.Add(this.dropbox6_baslangic);
		this.xtraTabPage4.Controls.Add(this.dropbox6_gorunen_adi);
		this.xtraTabPage4.Controls.Add(this.label134);
		this.xtraTabPage4.Controls.Add(this.label135);
		this.xtraTabPage4.Controls.Add(this.label136);
		this.xtraTabPage4.Controls.Add(this.label137);
		this.xtraTabPage4.Controls.Add(this.label120);
		this.xtraTabPage4.Controls.Add(this.label121);
		this.xtraTabPage4.Controls.Add(this.dropbox5_varsayilan_deger);
		this.xtraTabPage4.Controls.Add(this.label122);
		this.xtraTabPage4.Controls.Add(this.dropbox5_secenekler_veri);
		this.xtraTabPage4.Controls.Add(this.label123);
		this.xtraTabPage4.Controls.Add(this.dropbox5_secenekler_yazi);
		this.xtraTabPage4.Controls.Add(this.label124);
		this.xtraTabPage4.Controls.Add(this.dropbox5_zorunlu);
		this.xtraTabPage4.Controls.Add(this.dropbox5_uzunluk);
		this.xtraTabPage4.Controls.Add(this.dropbox5_baslangic);
		this.xtraTabPage4.Controls.Add(this.dropbox5_gorunen_adi);
		this.xtraTabPage4.Controls.Add(this.label125);
		this.xtraTabPage4.Controls.Add(this.label126);
		this.xtraTabPage4.Controls.Add(this.label127);
		this.xtraTabPage4.Controls.Add(this.label128);
		this.xtraTabPage4.Controls.Add(this.label111);
		this.xtraTabPage4.Controls.Add(this.label112);
		this.xtraTabPage4.Controls.Add(this.dropbox4_varsayilan_deger);
		this.xtraTabPage4.Controls.Add(this.label113);
		this.xtraTabPage4.Controls.Add(this.dropbox4_secenekler_veri);
		this.xtraTabPage4.Controls.Add(this.label114);
		this.xtraTabPage4.Controls.Add(this.dropbox4_secenekler_yazi);
		this.xtraTabPage4.Controls.Add(this.label115);
		this.xtraTabPage4.Controls.Add(this.dropbox4_zorunlu);
		this.xtraTabPage4.Controls.Add(this.dropbox4_uzunluk);
		this.xtraTabPage4.Controls.Add(this.dropbox4_baslangic);
		this.xtraTabPage4.Controls.Add(this.dropbox4_gorunen_adi);
		this.xtraTabPage4.Controls.Add(this.label116);
		this.xtraTabPage4.Controls.Add(this.label117);
		this.xtraTabPage4.Controls.Add(this.label118);
		this.xtraTabPage4.Controls.Add(this.label119);
		this.xtraTabPage4.Controls.Add(this.label102);
		this.xtraTabPage4.Controls.Add(this.label103);
		this.xtraTabPage4.Controls.Add(this.dropbox3_varsayilan_deger);
		this.xtraTabPage4.Controls.Add(this.label104);
		this.xtraTabPage4.Controls.Add(this.dropbox3_secenekler_veri);
		this.xtraTabPage4.Controls.Add(this.label105);
		this.xtraTabPage4.Controls.Add(this.dropbox3_secenekler_yazi);
		this.xtraTabPage4.Controls.Add(this.label106);
		this.xtraTabPage4.Controls.Add(this.dropbox3_zorunlu);
		this.xtraTabPage4.Controls.Add(this.dropbox3_uzunluk);
		this.xtraTabPage4.Controls.Add(this.dropbox3_baslangic);
		this.xtraTabPage4.Controls.Add(this.dropbox3_gorunen_adi);
		this.xtraTabPage4.Controls.Add(this.label107);
		this.xtraTabPage4.Controls.Add(this.label108);
		this.xtraTabPage4.Controls.Add(this.label109);
		this.xtraTabPage4.Controls.Add(this.label110);
		this.xtraTabPage4.Controls.Add(this.label93);
		this.xtraTabPage4.Controls.Add(this.label94);
		this.xtraTabPage4.Controls.Add(this.dropbox2_varsayilan_deger);
		this.xtraTabPage4.Controls.Add(this.label95);
		this.xtraTabPage4.Controls.Add(this.dropbox2_secenekler_veri);
		this.xtraTabPage4.Controls.Add(this.label96);
		this.xtraTabPage4.Controls.Add(this.dropbox2_secenekler_yazi);
		this.xtraTabPage4.Controls.Add(this.label97);
		this.xtraTabPage4.Controls.Add(this.dropbox2_zorunlu);
		this.xtraTabPage4.Controls.Add(this.dropbox2_uzunluk);
		this.xtraTabPage4.Controls.Add(this.dropbox2_baslangic);
		this.xtraTabPage4.Controls.Add(this.dropbox2_gorunen_adi);
		this.xtraTabPage4.Controls.Add(this.label98);
		this.xtraTabPage4.Controls.Add(this.label99);
		this.xtraTabPage4.Controls.Add(this.label100);
		this.xtraTabPage4.Controls.Add(this.label101);
		this.xtraTabPage4.Controls.Add(this.label92);
		this.xtraTabPage4.Controls.Add(this.label91);
		this.xtraTabPage4.Controls.Add(this.dropbox1_varsayilan_deger);
		this.xtraTabPage4.Controls.Add(this.label90);
		this.xtraTabPage4.Controls.Add(this.dropbox1_secenekler_veri);
		this.xtraTabPage4.Controls.Add(this.label89);
		this.xtraTabPage4.Controls.Add(this.dropbox1_secenekler_yazi);
		this.xtraTabPage4.Controls.Add(this.label84);
		this.xtraTabPage4.Controls.Add(this.dropbox1_zorunlu);
		this.xtraTabPage4.Controls.Add(this.dropbox1_uzunluk);
		this.xtraTabPage4.Controls.Add(this.dropbox1_baslangic);
		this.xtraTabPage4.Controls.Add(this.dropbox1_gorunen_adi);
		this.xtraTabPage4.Controls.Add(this.label85);
		this.xtraTabPage4.Controls.Add(this.label86);
		this.xtraTabPage4.Controls.Add(this.label87);
		this.xtraTabPage4.Controls.Add(this.label88);
		this.xtraTabPage4.Name = "xtraTabPage4";
		this.xtraTabPage4.Size = new System.Drawing.Size(734, 424);
		this.xtraTabPage4.Text = "Açılan kutu alanları";
		this.label165.Location = new System.Drawing.Point(371, 1642);
		this.label165.Name = "label165";
		this.label165.Size = new System.Drawing.Size(319, 19);
		this.label165.TabIndex = 364;
		this.label165.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label165.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label166.Location = new System.Drawing.Point(29, 1642);
		this.label166.Name = "label166";
		this.label166.Size = new System.Drawing.Size(336, 19);
		this.label166.TabIndex = 363;
		this.label166.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label166.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dropbox10_varsayilan_deger.Location = new System.Drawing.Point(434, 1563);
		this.dropbox10_varsayilan_deger.Name = "dropbox10_varsayilan_deger";
		this.dropbox10_varsayilan_deger.Size = new System.Drawing.Size(256, 20);
		this.dropbox10_varsayilan_deger.TabIndex = 362;
		this.label167.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label167.Location = new System.Drawing.Point(434, 1540);
		this.label167.Name = "label167";
		this.label167.Size = new System.Drawing.Size(256, 19);
		this.label167.TabIndex = 361;
		this.label167.Text = "Varsayılan (Veri)";
		this.label167.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox10_secenekler_veri.Location = new System.Drawing.Point(374, 1619);
		this.dropbox10_secenekler_veri.Name = "dropbox10_secenekler_veri";
		this.dropbox10_secenekler_veri.Size = new System.Drawing.Size(316, 20);
		this.dropbox10_secenekler_veri.TabIndex = 360;
		this.label168.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label168.Location = new System.Drawing.Point(374, 1596);
		this.label168.Name = "label168";
		this.label168.Size = new System.Drawing.Size(316, 19);
		this.label168.TabIndex = 359;
		this.label168.Text = "Seçenekler (Veri)";
		this.label168.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox10_secenekler_yazi.Location = new System.Drawing.Point(29, 1619);
		this.dropbox10_secenekler_yazi.Name = "dropbox10_secenekler_yazi";
		this.dropbox10_secenekler_yazi.Size = new System.Drawing.Size(336, 20);
		this.dropbox10_secenekler_yazi.TabIndex = 358;
		this.label169.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label169.Location = new System.Drawing.Point(29, 1596);
		this.label169.Name = "label169";
		this.label169.Size = new System.Drawing.Size(336, 19);
		this.label169.TabIndex = 357;
		this.label169.Text = "Seçenekler (Yazı)";
		this.label169.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox10_zorunlu.Location = new System.Drawing.Point(140, 1514);
		this.dropbox10_zorunlu.Name = "dropbox10_zorunlu";
		this.dropbox10_zorunlu.Properties.Caption = "Zorunlu";
		this.dropbox10_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.dropbox10_zorunlu.TabIndex = 356;
		this.dropbox10_zorunlu.Visible = false;
		this.dropbox10_uzunluk.EditValue = new decimal(new int[4]);
		this.dropbox10_uzunluk.Location = new System.Drawing.Point(374, 1563);
		this.dropbox10_uzunluk.Name = "dropbox10_uzunluk";
		this.dropbox10_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox10_uzunluk.Properties.IsFloatValue = false;
		this.dropbox10_uzunluk.Properties.Mask.EditMask = "N00";
		this.dropbox10_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.dropbox10_uzunluk.TabIndex = 355;
		this.dropbox10_baslangic.EditValue = new decimal(new int[4]);
		this.dropbox10_baslangic.Location = new System.Drawing.Point(269, 1563);
		this.dropbox10_baslangic.Name = "dropbox10_baslangic";
		this.dropbox10_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox10_baslangic.Properties.IsFloatValue = false;
		this.dropbox10_baslangic.Properties.Mask.EditMask = "N00";
		this.dropbox10_baslangic.Size = new System.Drawing.Size(96, 20);
		this.dropbox10_baslangic.TabIndex = 354;
		this.dropbox10_gorunen_adi.Location = new System.Drawing.Point(29, 1563);
		this.dropbox10_gorunen_adi.Name = "dropbox10_gorunen_adi";
		this.dropbox10_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.dropbox10_gorunen_adi.TabIndex = 353;
		this.label170.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label170.Location = new System.Drawing.Point(29, 1540);
		this.label170.Name = "label170";
		this.label170.Size = new System.Drawing.Size(234, 19);
		this.label170.TabIndex = 352;
		this.label170.Text = "Başlık";
		this.label170.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label171.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label171.Location = new System.Drawing.Point(26, 1514);
		this.label171.Name = "label171";
		this.label171.Size = new System.Drawing.Size(108, 19);
		this.label171.TabIndex = 351;
		this.label171.Text = "Açılan kutu 10 :";
		this.label171.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label172.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label172.Location = new System.Drawing.Point(371, 1540);
		this.label172.Name = "label172";
		this.label172.Size = new System.Drawing.Size(54, 19);
		this.label172.TabIndex = 350;
		this.label172.Text = "Uzunluk";
		this.label172.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label173.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label173.Location = new System.Drawing.Point(269, 1540);
		this.label173.Name = "label173";
		this.label173.Size = new System.Drawing.Size(96, 19);
		this.label173.TabIndex = 349;
		this.label173.Text = "Başlangıç/Sıra";
		this.label173.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label156.Location = new System.Drawing.Point(368, 1477);
		this.label156.Name = "label156";
		this.label156.Size = new System.Drawing.Size(319, 19);
		this.label156.TabIndex = 348;
		this.label156.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label156.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label157.Location = new System.Drawing.Point(26, 1477);
		this.label157.Name = "label157";
		this.label157.Size = new System.Drawing.Size(336, 19);
		this.label157.TabIndex = 347;
		this.label157.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label157.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dropbox9_varsayilan_deger.Location = new System.Drawing.Point(431, 1398);
		this.dropbox9_varsayilan_deger.Name = "dropbox9_varsayilan_deger";
		this.dropbox9_varsayilan_deger.Size = new System.Drawing.Size(256, 20);
		this.dropbox9_varsayilan_deger.TabIndex = 346;
		this.label158.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label158.Location = new System.Drawing.Point(431, 1375);
		this.label158.Name = "label158";
		this.label158.Size = new System.Drawing.Size(256, 19);
		this.label158.TabIndex = 345;
		this.label158.Text = "Varsayılan (Veri)";
		this.label158.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox9_secenekler_veri.Location = new System.Drawing.Point(371, 1454);
		this.dropbox9_secenekler_veri.Name = "dropbox9_secenekler_veri";
		this.dropbox9_secenekler_veri.Size = new System.Drawing.Size(316, 20);
		this.dropbox9_secenekler_veri.TabIndex = 344;
		this.label159.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label159.Location = new System.Drawing.Point(371, 1431);
		this.label159.Name = "label159";
		this.label159.Size = new System.Drawing.Size(316, 19);
		this.label159.TabIndex = 343;
		this.label159.Text = "Seçenekler (Veri)";
		this.label159.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox9_secenekler_yazi.Location = new System.Drawing.Point(26, 1454);
		this.dropbox9_secenekler_yazi.Name = "dropbox9_secenekler_yazi";
		this.dropbox9_secenekler_yazi.Size = new System.Drawing.Size(336, 20);
		this.dropbox9_secenekler_yazi.TabIndex = 342;
		this.label160.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label160.Location = new System.Drawing.Point(26, 1431);
		this.label160.Name = "label160";
		this.label160.Size = new System.Drawing.Size(336, 19);
		this.label160.TabIndex = 341;
		this.label160.Text = "Seçenekler (Yazı)";
		this.label160.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox9_zorunlu.Location = new System.Drawing.Point(137, 1349);
		this.dropbox9_zorunlu.Name = "dropbox9_zorunlu";
		this.dropbox9_zorunlu.Properties.Caption = "Zorunlu";
		this.dropbox9_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.dropbox9_zorunlu.TabIndex = 340;
		this.dropbox9_zorunlu.Visible = false;
		this.dropbox9_uzunluk.EditValue = new decimal(new int[4]);
		this.dropbox9_uzunluk.Location = new System.Drawing.Point(371, 1398);
		this.dropbox9_uzunluk.Name = "dropbox9_uzunluk";
		this.dropbox9_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox9_uzunluk.Properties.IsFloatValue = false;
		this.dropbox9_uzunluk.Properties.Mask.EditMask = "N00";
		this.dropbox9_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.dropbox9_uzunluk.TabIndex = 339;
		this.dropbox9_baslangic.EditValue = new decimal(new int[4]);
		this.dropbox9_baslangic.Location = new System.Drawing.Point(266, 1398);
		this.dropbox9_baslangic.Name = "dropbox9_baslangic";
		this.dropbox9_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox9_baslangic.Properties.IsFloatValue = false;
		this.dropbox9_baslangic.Properties.Mask.EditMask = "N00";
		this.dropbox9_baslangic.Size = new System.Drawing.Size(96, 20);
		this.dropbox9_baslangic.TabIndex = 338;
		this.dropbox9_gorunen_adi.Location = new System.Drawing.Point(26, 1398);
		this.dropbox9_gorunen_adi.Name = "dropbox9_gorunen_adi";
		this.dropbox9_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.dropbox9_gorunen_adi.TabIndex = 337;
		this.label161.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label161.Location = new System.Drawing.Point(26, 1375);
		this.label161.Name = "label161";
		this.label161.Size = new System.Drawing.Size(234, 19);
		this.label161.TabIndex = 336;
		this.label161.Text = "Başlık";
		this.label161.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label162.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label162.Location = new System.Drawing.Point(23, 1349);
		this.label162.Name = "label162";
		this.label162.Size = new System.Drawing.Size(108, 19);
		this.label162.TabIndex = 335;
		this.label162.Text = "Açılan kutu 9 :";
		this.label162.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label163.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label163.Location = new System.Drawing.Point(368, 1375);
		this.label163.Name = "label163";
		this.label163.Size = new System.Drawing.Size(54, 19);
		this.label163.TabIndex = 334;
		this.label163.Text = "Uzunluk";
		this.label163.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label164.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label164.Location = new System.Drawing.Point(266, 1375);
		this.label164.Name = "label164";
		this.label164.Size = new System.Drawing.Size(96, 19);
		this.label164.TabIndex = 333;
		this.label164.Text = "Başlangıç/Sıra";
		this.label164.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label147.Location = new System.Drawing.Point(365, 1314);
		this.label147.Name = "label147";
		this.label147.Size = new System.Drawing.Size(319, 19);
		this.label147.TabIndex = 332;
		this.label147.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label147.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label148.Location = new System.Drawing.Point(23, 1314);
		this.label148.Name = "label148";
		this.label148.Size = new System.Drawing.Size(336, 19);
		this.label148.TabIndex = 331;
		this.label148.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label148.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dropbox8_varsayilan_deger.Location = new System.Drawing.Point(428, 1235);
		this.dropbox8_varsayilan_deger.Name = "dropbox8_varsayilan_deger";
		this.dropbox8_varsayilan_deger.Size = new System.Drawing.Size(256, 20);
		this.dropbox8_varsayilan_deger.TabIndex = 330;
		this.label149.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label149.Location = new System.Drawing.Point(428, 1212);
		this.label149.Name = "label149";
		this.label149.Size = new System.Drawing.Size(256, 19);
		this.label149.TabIndex = 329;
		this.label149.Text = "Varsayılan (Veri)";
		this.label149.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox8_secenekler_veri.Location = new System.Drawing.Point(368, 1291);
		this.dropbox8_secenekler_veri.Name = "dropbox8_secenekler_veri";
		this.dropbox8_secenekler_veri.Size = new System.Drawing.Size(316, 20);
		this.dropbox8_secenekler_veri.TabIndex = 328;
		this.label150.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label150.Location = new System.Drawing.Point(368, 1268);
		this.label150.Name = "label150";
		this.label150.Size = new System.Drawing.Size(316, 19);
		this.label150.TabIndex = 327;
		this.label150.Text = "Seçenekler (Veri)";
		this.label150.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox8_secenekler_yazi.Location = new System.Drawing.Point(23, 1291);
		this.dropbox8_secenekler_yazi.Name = "dropbox8_secenekler_yazi";
		this.dropbox8_secenekler_yazi.Size = new System.Drawing.Size(336, 20);
		this.dropbox8_secenekler_yazi.TabIndex = 326;
		this.label151.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label151.Location = new System.Drawing.Point(23, 1268);
		this.label151.Name = "label151";
		this.label151.Size = new System.Drawing.Size(336, 19);
		this.label151.TabIndex = 325;
		this.label151.Text = "Seçenekler (Yazı)";
		this.label151.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox8_zorunlu.Location = new System.Drawing.Point(134, 1186);
		this.dropbox8_zorunlu.Name = "dropbox8_zorunlu";
		this.dropbox8_zorunlu.Properties.Caption = "Zorunlu";
		this.dropbox8_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.dropbox8_zorunlu.TabIndex = 324;
		this.dropbox8_zorunlu.Visible = false;
		this.dropbox8_uzunluk.EditValue = new decimal(new int[4]);
		this.dropbox8_uzunluk.Location = new System.Drawing.Point(368, 1235);
		this.dropbox8_uzunluk.Name = "dropbox8_uzunluk";
		this.dropbox8_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox8_uzunluk.Properties.IsFloatValue = false;
		this.dropbox8_uzunluk.Properties.Mask.EditMask = "N00";
		this.dropbox8_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.dropbox8_uzunluk.TabIndex = 323;
		this.dropbox8_baslangic.EditValue = new decimal(new int[4]);
		this.dropbox8_baslangic.Location = new System.Drawing.Point(263, 1235);
		this.dropbox8_baslangic.Name = "dropbox8_baslangic";
		this.dropbox8_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox8_baslangic.Properties.IsFloatValue = false;
		this.dropbox8_baslangic.Properties.Mask.EditMask = "N00";
		this.dropbox8_baslangic.Size = new System.Drawing.Size(96, 20);
		this.dropbox8_baslangic.TabIndex = 322;
		this.dropbox8_gorunen_adi.Location = new System.Drawing.Point(23, 1235);
		this.dropbox8_gorunen_adi.Name = "dropbox8_gorunen_adi";
		this.dropbox8_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.dropbox8_gorunen_adi.TabIndex = 321;
		this.label152.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label152.Location = new System.Drawing.Point(23, 1212);
		this.label152.Name = "label152";
		this.label152.Size = new System.Drawing.Size(234, 19);
		this.label152.TabIndex = 320;
		this.label152.Text = "Başlık";
		this.label152.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label153.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label153.Location = new System.Drawing.Point(20, 1186);
		this.label153.Name = "label153";
		this.label153.Size = new System.Drawing.Size(108, 19);
		this.label153.TabIndex = 319;
		this.label153.Text = "Açılan kutu 8 :";
		this.label153.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label154.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label154.Location = new System.Drawing.Point(365, 1212);
		this.label154.Name = "label154";
		this.label154.Size = new System.Drawing.Size(54, 19);
		this.label154.TabIndex = 318;
		this.label154.Text = "Uzunluk";
		this.label154.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label155.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label155.Location = new System.Drawing.Point(263, 1212);
		this.label155.Name = "label155";
		this.label155.Size = new System.Drawing.Size(96, 19);
		this.label155.TabIndex = 317;
		this.label155.Text = "Başlangıç/Sıra";
		this.label155.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label138.Location = new System.Drawing.Point(362, 1145);
		this.label138.Name = "label138";
		this.label138.Size = new System.Drawing.Size(319, 19);
		this.label138.TabIndex = 316;
		this.label138.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label138.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label139.Location = new System.Drawing.Point(20, 1145);
		this.label139.Name = "label139";
		this.label139.Size = new System.Drawing.Size(336, 19);
		this.label139.TabIndex = 315;
		this.label139.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label139.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dropbox7_varsayilan_deger.Location = new System.Drawing.Point(425, 1066);
		this.dropbox7_varsayilan_deger.Name = "dropbox7_varsayilan_deger";
		this.dropbox7_varsayilan_deger.Size = new System.Drawing.Size(256, 20);
		this.dropbox7_varsayilan_deger.TabIndex = 314;
		this.label140.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label140.Location = new System.Drawing.Point(425, 1043);
		this.label140.Name = "label140";
		this.label140.Size = new System.Drawing.Size(256, 19);
		this.label140.TabIndex = 313;
		this.label140.Text = "Varsayılan (Veri)";
		this.label140.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox7_secenekler_veri.Location = new System.Drawing.Point(365, 1122);
		this.dropbox7_secenekler_veri.Name = "dropbox7_secenekler_veri";
		this.dropbox7_secenekler_veri.Size = new System.Drawing.Size(316, 20);
		this.dropbox7_secenekler_veri.TabIndex = 312;
		this.label141.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label141.Location = new System.Drawing.Point(365, 1099);
		this.label141.Name = "label141";
		this.label141.Size = new System.Drawing.Size(316, 19);
		this.label141.TabIndex = 311;
		this.label141.Text = "Seçenekler (Veri)";
		this.label141.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox7_secenekler_yazi.Location = new System.Drawing.Point(20, 1122);
		this.dropbox7_secenekler_yazi.Name = "dropbox7_secenekler_yazi";
		this.dropbox7_secenekler_yazi.Size = new System.Drawing.Size(336, 20);
		this.dropbox7_secenekler_yazi.TabIndex = 310;
		this.label142.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label142.Location = new System.Drawing.Point(20, 1099);
		this.label142.Name = "label142";
		this.label142.Size = new System.Drawing.Size(336, 19);
		this.label142.TabIndex = 309;
		this.label142.Text = "Seçenekler (Yazı)";
		this.label142.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox7_zorunlu.Location = new System.Drawing.Point(131, 1017);
		this.dropbox7_zorunlu.Name = "dropbox7_zorunlu";
		this.dropbox7_zorunlu.Properties.Caption = "Zorunlu";
		this.dropbox7_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.dropbox7_zorunlu.TabIndex = 308;
		this.dropbox7_zorunlu.Visible = false;
		this.dropbox7_uzunluk.EditValue = new decimal(new int[4]);
		this.dropbox7_uzunluk.Location = new System.Drawing.Point(365, 1066);
		this.dropbox7_uzunluk.Name = "dropbox7_uzunluk";
		this.dropbox7_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox7_uzunluk.Properties.IsFloatValue = false;
		this.dropbox7_uzunluk.Properties.Mask.EditMask = "N00";
		this.dropbox7_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.dropbox7_uzunluk.TabIndex = 307;
		this.dropbox7_baslangic.EditValue = new decimal(new int[4]);
		this.dropbox7_baslangic.Location = new System.Drawing.Point(260, 1066);
		this.dropbox7_baslangic.Name = "dropbox7_baslangic";
		this.dropbox7_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox7_baslangic.Properties.IsFloatValue = false;
		this.dropbox7_baslangic.Properties.Mask.EditMask = "N00";
		this.dropbox7_baslangic.Size = new System.Drawing.Size(96, 20);
		this.dropbox7_baslangic.TabIndex = 306;
		this.dropbox7_gorunen_adi.Location = new System.Drawing.Point(20, 1066);
		this.dropbox7_gorunen_adi.Name = "dropbox7_gorunen_adi";
		this.dropbox7_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.dropbox7_gorunen_adi.TabIndex = 305;
		this.label143.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label143.Location = new System.Drawing.Point(20, 1043);
		this.label143.Name = "label143";
		this.label143.Size = new System.Drawing.Size(234, 19);
		this.label143.TabIndex = 304;
		this.label143.Text = "Başlık";
		this.label143.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label144.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label144.Location = new System.Drawing.Point(17, 1017);
		this.label144.Name = "label144";
		this.label144.Size = new System.Drawing.Size(108, 19);
		this.label144.TabIndex = 303;
		this.label144.Text = "Açılan kutu 7 :";
		this.label144.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label145.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label145.Location = new System.Drawing.Point(362, 1043);
		this.label145.Name = "label145";
		this.label145.Size = new System.Drawing.Size(54, 19);
		this.label145.TabIndex = 302;
		this.label145.Text = "Uzunluk";
		this.label145.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label146.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label146.Location = new System.Drawing.Point(260, 1043);
		this.label146.Name = "label146";
		this.label146.Size = new System.Drawing.Size(96, 19);
		this.label146.TabIndex = 301;
		this.label146.Text = "Başlangıç/Sıra";
		this.label146.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label129.Location = new System.Drawing.Point(362, 981);
		this.label129.Name = "label129";
		this.label129.Size = new System.Drawing.Size(319, 19);
		this.label129.TabIndex = 300;
		this.label129.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label129.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label130.Location = new System.Drawing.Point(20, 981);
		this.label130.Name = "label130";
		this.label130.Size = new System.Drawing.Size(336, 19);
		this.label130.TabIndex = 299;
		this.label130.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label130.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dropbox6_varsayilan_deger.Location = new System.Drawing.Point(425, 902);
		this.dropbox6_varsayilan_deger.Name = "dropbox6_varsayilan_deger";
		this.dropbox6_varsayilan_deger.Size = new System.Drawing.Size(256, 20);
		this.dropbox6_varsayilan_deger.TabIndex = 298;
		this.label131.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label131.Location = new System.Drawing.Point(425, 879);
		this.label131.Name = "label131";
		this.label131.Size = new System.Drawing.Size(256, 19);
		this.label131.TabIndex = 297;
		this.label131.Text = "Varsayılan (Veri)";
		this.label131.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox6_secenekler_veri.Location = new System.Drawing.Point(365, 958);
		this.dropbox6_secenekler_veri.Name = "dropbox6_secenekler_veri";
		this.dropbox6_secenekler_veri.Size = new System.Drawing.Size(316, 20);
		this.dropbox6_secenekler_veri.TabIndex = 296;
		this.label132.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label132.Location = new System.Drawing.Point(365, 935);
		this.label132.Name = "label132";
		this.label132.Size = new System.Drawing.Size(316, 19);
		this.label132.TabIndex = 295;
		this.label132.Text = "Seçenekler (Veri)";
		this.label132.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox6_secenekler_yazi.Location = new System.Drawing.Point(20, 958);
		this.dropbox6_secenekler_yazi.Name = "dropbox6_secenekler_yazi";
		this.dropbox6_secenekler_yazi.Size = new System.Drawing.Size(336, 20);
		this.dropbox6_secenekler_yazi.TabIndex = 294;
		this.label133.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label133.Location = new System.Drawing.Point(20, 935);
		this.label133.Name = "label133";
		this.label133.Size = new System.Drawing.Size(336, 19);
		this.label133.TabIndex = 293;
		this.label133.Text = "Seçenekler (Yazı)";
		this.label133.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox6_zorunlu.Location = new System.Drawing.Point(131, 853);
		this.dropbox6_zorunlu.Name = "dropbox6_zorunlu";
		this.dropbox6_zorunlu.Properties.Caption = "Zorunlu";
		this.dropbox6_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.dropbox6_zorunlu.TabIndex = 292;
		this.dropbox6_zorunlu.Visible = false;
		this.dropbox6_uzunluk.EditValue = new decimal(new int[4]);
		this.dropbox6_uzunluk.Location = new System.Drawing.Point(365, 902);
		this.dropbox6_uzunluk.Name = "dropbox6_uzunluk";
		this.dropbox6_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox6_uzunluk.Properties.IsFloatValue = false;
		this.dropbox6_uzunluk.Properties.Mask.EditMask = "N00";
		this.dropbox6_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.dropbox6_uzunluk.TabIndex = 291;
		this.dropbox6_baslangic.EditValue = new decimal(new int[4]);
		this.dropbox6_baslangic.Location = new System.Drawing.Point(260, 902);
		this.dropbox6_baslangic.Name = "dropbox6_baslangic";
		this.dropbox6_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox6_baslangic.Properties.IsFloatValue = false;
		this.dropbox6_baslangic.Properties.Mask.EditMask = "N00";
		this.dropbox6_baslangic.Size = new System.Drawing.Size(96, 20);
		this.dropbox6_baslangic.TabIndex = 290;
		this.dropbox6_gorunen_adi.Location = new System.Drawing.Point(20, 902);
		this.dropbox6_gorunen_adi.Name = "dropbox6_gorunen_adi";
		this.dropbox6_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.dropbox6_gorunen_adi.TabIndex = 289;
		this.label134.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label134.Location = new System.Drawing.Point(20, 879);
		this.label134.Name = "label134";
		this.label134.Size = new System.Drawing.Size(234, 19);
		this.label134.TabIndex = 288;
		this.label134.Text = "Başlık";
		this.label134.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label135.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label135.Location = new System.Drawing.Point(17, 853);
		this.label135.Name = "label135";
		this.label135.Size = new System.Drawing.Size(108, 19);
		this.label135.TabIndex = 287;
		this.label135.Text = "Açılan kutu 6 :";
		this.label135.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label136.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label136.Location = new System.Drawing.Point(362, 879);
		this.label136.Name = "label136";
		this.label136.Size = new System.Drawing.Size(54, 19);
		this.label136.TabIndex = 286;
		this.label136.Text = "Uzunluk";
		this.label136.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label137.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label137.Location = new System.Drawing.Point(260, 879);
		this.label137.Name = "label137";
		this.label137.Size = new System.Drawing.Size(96, 19);
		this.label137.TabIndex = 285;
		this.label137.Text = "Başlangıç/Sıra";
		this.label137.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label120.Location = new System.Drawing.Point(359, 802);
		this.label120.Name = "label120";
		this.label120.Size = new System.Drawing.Size(319, 19);
		this.label120.TabIndex = 284;
		this.label120.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label120.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label121.Location = new System.Drawing.Point(17, 802);
		this.label121.Name = "label121";
		this.label121.Size = new System.Drawing.Size(336, 19);
		this.label121.TabIndex = 283;
		this.label121.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label121.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dropbox5_varsayilan_deger.Location = new System.Drawing.Point(422, 723);
		this.dropbox5_varsayilan_deger.Name = "dropbox5_varsayilan_deger";
		this.dropbox5_varsayilan_deger.Size = new System.Drawing.Size(256, 20);
		this.dropbox5_varsayilan_deger.TabIndex = 282;
		this.label122.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label122.Location = new System.Drawing.Point(422, 700);
		this.label122.Name = "label122";
		this.label122.Size = new System.Drawing.Size(256, 19);
		this.label122.TabIndex = 281;
		this.label122.Text = "Varsayılan (Veri)";
		this.label122.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox5_secenekler_veri.Location = new System.Drawing.Point(362, 779);
		this.dropbox5_secenekler_veri.Name = "dropbox5_secenekler_veri";
		this.dropbox5_secenekler_veri.Size = new System.Drawing.Size(316, 20);
		this.dropbox5_secenekler_veri.TabIndex = 280;
		this.label123.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label123.Location = new System.Drawing.Point(362, 756);
		this.label123.Name = "label123";
		this.label123.Size = new System.Drawing.Size(316, 19);
		this.label123.TabIndex = 279;
		this.label123.Text = "Seçenekler (Veri)";
		this.label123.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox5_secenekler_yazi.Location = new System.Drawing.Point(17, 779);
		this.dropbox5_secenekler_yazi.Name = "dropbox5_secenekler_yazi";
		this.dropbox5_secenekler_yazi.Size = new System.Drawing.Size(336, 20);
		this.dropbox5_secenekler_yazi.TabIndex = 278;
		this.label124.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label124.Location = new System.Drawing.Point(17, 756);
		this.label124.Name = "label124";
		this.label124.Size = new System.Drawing.Size(336, 19);
		this.label124.TabIndex = 277;
		this.label124.Text = "Seçenekler (Yazı)";
		this.label124.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox5_zorunlu.Location = new System.Drawing.Point(128, 674);
		this.dropbox5_zorunlu.Name = "dropbox5_zorunlu";
		this.dropbox5_zorunlu.Properties.Caption = "Zorunlu";
		this.dropbox5_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.dropbox5_zorunlu.TabIndex = 276;
		this.dropbox5_zorunlu.Visible = false;
		this.dropbox5_uzunluk.EditValue = new decimal(new int[4]);
		this.dropbox5_uzunluk.Location = new System.Drawing.Point(362, 723);
		this.dropbox5_uzunluk.Name = "dropbox5_uzunluk";
		this.dropbox5_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox5_uzunluk.Properties.IsFloatValue = false;
		this.dropbox5_uzunluk.Properties.Mask.EditMask = "N00";
		this.dropbox5_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.dropbox5_uzunluk.TabIndex = 275;
		this.dropbox5_baslangic.EditValue = new decimal(new int[4]);
		this.dropbox5_baslangic.Location = new System.Drawing.Point(257, 723);
		this.dropbox5_baslangic.Name = "dropbox5_baslangic";
		this.dropbox5_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox5_baslangic.Properties.IsFloatValue = false;
		this.dropbox5_baslangic.Properties.Mask.EditMask = "N00";
		this.dropbox5_baslangic.Size = new System.Drawing.Size(96, 20);
		this.dropbox5_baslangic.TabIndex = 274;
		this.dropbox5_gorunen_adi.Location = new System.Drawing.Point(17, 723);
		this.dropbox5_gorunen_adi.Name = "dropbox5_gorunen_adi";
		this.dropbox5_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.dropbox5_gorunen_adi.TabIndex = 273;
		this.label125.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label125.Location = new System.Drawing.Point(17, 700);
		this.label125.Name = "label125";
		this.label125.Size = new System.Drawing.Size(234, 19);
		this.label125.TabIndex = 272;
		this.label125.Text = "Başlık";
		this.label125.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label126.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label126.Location = new System.Drawing.Point(14, 674);
		this.label126.Name = "label126";
		this.label126.Size = new System.Drawing.Size(108, 19);
		this.label126.TabIndex = 271;
		this.label126.Text = "Açılan kutu 5 :";
		this.label126.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label127.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label127.Location = new System.Drawing.Point(359, 700);
		this.label127.Name = "label127";
		this.label127.Size = new System.Drawing.Size(54, 19);
		this.label127.TabIndex = 270;
		this.label127.Text = "Uzunluk";
		this.label127.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label128.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label128.Location = new System.Drawing.Point(257, 700);
		this.label128.Name = "label128";
		this.label128.Size = new System.Drawing.Size(96, 19);
		this.label128.TabIndex = 269;
		this.label128.Text = "Başlangıç/Sıra";
		this.label128.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label111.Location = new System.Drawing.Point(359, 638);
		this.label111.Name = "label111";
		this.label111.Size = new System.Drawing.Size(319, 19);
		this.label111.TabIndex = 268;
		this.label111.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label111.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label112.Location = new System.Drawing.Point(17, 638);
		this.label112.Name = "label112";
		this.label112.Size = new System.Drawing.Size(336, 19);
		this.label112.TabIndex = 267;
		this.label112.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label112.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dropbox4_varsayilan_deger.Location = new System.Drawing.Point(422, 559);
		this.dropbox4_varsayilan_deger.Name = "dropbox4_varsayilan_deger";
		this.dropbox4_varsayilan_deger.Size = new System.Drawing.Size(256, 20);
		this.dropbox4_varsayilan_deger.TabIndex = 266;
		this.label113.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label113.Location = new System.Drawing.Point(422, 536);
		this.label113.Name = "label113";
		this.label113.Size = new System.Drawing.Size(256, 19);
		this.label113.TabIndex = 265;
		this.label113.Text = "Varsayılan (Veri)";
		this.label113.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox4_secenekler_veri.Location = new System.Drawing.Point(362, 615);
		this.dropbox4_secenekler_veri.Name = "dropbox4_secenekler_veri";
		this.dropbox4_secenekler_veri.Size = new System.Drawing.Size(316, 20);
		this.dropbox4_secenekler_veri.TabIndex = 264;
		this.label114.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label114.Location = new System.Drawing.Point(362, 592);
		this.label114.Name = "label114";
		this.label114.Size = new System.Drawing.Size(316, 19);
		this.label114.TabIndex = 263;
		this.label114.Text = "Seçenekler (Veri)";
		this.label114.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox4_secenekler_yazi.Location = new System.Drawing.Point(17, 615);
		this.dropbox4_secenekler_yazi.Name = "dropbox4_secenekler_yazi";
		this.dropbox4_secenekler_yazi.Size = new System.Drawing.Size(336, 20);
		this.dropbox4_secenekler_yazi.TabIndex = 262;
		this.label115.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label115.Location = new System.Drawing.Point(17, 592);
		this.label115.Name = "label115";
		this.label115.Size = new System.Drawing.Size(336, 19);
		this.label115.TabIndex = 261;
		this.label115.Text = "Seçenekler (Yazı)";
		this.label115.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox4_zorunlu.Location = new System.Drawing.Point(128, 510);
		this.dropbox4_zorunlu.Name = "dropbox4_zorunlu";
		this.dropbox4_zorunlu.Properties.Caption = "Zorunlu";
		this.dropbox4_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.dropbox4_zorunlu.TabIndex = 260;
		this.dropbox4_zorunlu.Visible = false;
		this.dropbox4_uzunluk.EditValue = new decimal(new int[4]);
		this.dropbox4_uzunluk.Location = new System.Drawing.Point(362, 559);
		this.dropbox4_uzunluk.Name = "dropbox4_uzunluk";
		this.dropbox4_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox4_uzunluk.Properties.IsFloatValue = false;
		this.dropbox4_uzunluk.Properties.Mask.EditMask = "N00";
		this.dropbox4_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.dropbox4_uzunluk.TabIndex = 259;
		this.dropbox4_baslangic.EditValue = new decimal(new int[4]);
		this.dropbox4_baslangic.Location = new System.Drawing.Point(257, 559);
		this.dropbox4_baslangic.Name = "dropbox4_baslangic";
		this.dropbox4_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox4_baslangic.Properties.IsFloatValue = false;
		this.dropbox4_baslangic.Properties.Mask.EditMask = "N00";
		this.dropbox4_baslangic.Size = new System.Drawing.Size(96, 20);
		this.dropbox4_baslangic.TabIndex = 258;
		this.dropbox4_gorunen_adi.Location = new System.Drawing.Point(17, 559);
		this.dropbox4_gorunen_adi.Name = "dropbox4_gorunen_adi";
		this.dropbox4_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.dropbox4_gorunen_adi.TabIndex = 257;
		this.label116.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label116.Location = new System.Drawing.Point(17, 536);
		this.label116.Name = "label116";
		this.label116.Size = new System.Drawing.Size(234, 19);
		this.label116.TabIndex = 256;
		this.label116.Text = "Başlık";
		this.label116.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label117.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label117.Location = new System.Drawing.Point(14, 510);
		this.label117.Name = "label117";
		this.label117.Size = new System.Drawing.Size(108, 19);
		this.label117.TabIndex = 255;
		this.label117.Text = "Açılan kutu 4 :";
		this.label117.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label118.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label118.Location = new System.Drawing.Point(359, 536);
		this.label118.Name = "label118";
		this.label118.Size = new System.Drawing.Size(54, 19);
		this.label118.TabIndex = 254;
		this.label118.Text = "Uzunluk";
		this.label118.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label119.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label119.Location = new System.Drawing.Point(257, 536);
		this.label119.Name = "label119";
		this.label119.Size = new System.Drawing.Size(96, 19);
		this.label119.TabIndex = 253;
		this.label119.Text = "Başlangıç/Sıra";
		this.label119.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label102.Location = new System.Drawing.Point(359, 470);
		this.label102.Name = "label102";
		this.label102.Size = new System.Drawing.Size(319, 19);
		this.label102.TabIndex = 252;
		this.label102.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label102.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label103.Location = new System.Drawing.Point(17, 470);
		this.label103.Name = "label103";
		this.label103.Size = new System.Drawing.Size(336, 19);
		this.label103.TabIndex = 251;
		this.label103.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label103.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dropbox3_varsayilan_deger.Location = new System.Drawing.Point(422, 391);
		this.dropbox3_varsayilan_deger.Name = "dropbox3_varsayilan_deger";
		this.dropbox3_varsayilan_deger.Size = new System.Drawing.Size(256, 20);
		this.dropbox3_varsayilan_deger.TabIndex = 250;
		this.label104.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label104.Location = new System.Drawing.Point(422, 368);
		this.label104.Name = "label104";
		this.label104.Size = new System.Drawing.Size(256, 19);
		this.label104.TabIndex = 249;
		this.label104.Text = "Varsayılan (Veri)";
		this.label104.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox3_secenekler_veri.Location = new System.Drawing.Point(362, 447);
		this.dropbox3_secenekler_veri.Name = "dropbox3_secenekler_veri";
		this.dropbox3_secenekler_veri.Size = new System.Drawing.Size(316, 20);
		this.dropbox3_secenekler_veri.TabIndex = 248;
		this.label105.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label105.Location = new System.Drawing.Point(362, 424);
		this.label105.Name = "label105";
		this.label105.Size = new System.Drawing.Size(316, 19);
		this.label105.TabIndex = 247;
		this.label105.Text = "Seçenekler (Veri)";
		this.label105.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox3_secenekler_yazi.Location = new System.Drawing.Point(17, 447);
		this.dropbox3_secenekler_yazi.Name = "dropbox3_secenekler_yazi";
		this.dropbox3_secenekler_yazi.Size = new System.Drawing.Size(336, 20);
		this.dropbox3_secenekler_yazi.TabIndex = 246;
		this.label106.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label106.Location = new System.Drawing.Point(17, 424);
		this.label106.Name = "label106";
		this.label106.Size = new System.Drawing.Size(336, 19);
		this.label106.TabIndex = 245;
		this.label106.Text = "Seçenekler (Yazı)";
		this.label106.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox3_zorunlu.Location = new System.Drawing.Point(128, 342);
		this.dropbox3_zorunlu.Name = "dropbox3_zorunlu";
		this.dropbox3_zorunlu.Properties.Caption = "Zorunlu";
		this.dropbox3_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.dropbox3_zorunlu.TabIndex = 244;
		this.dropbox3_zorunlu.Visible = false;
		this.dropbox3_uzunluk.EditValue = new decimal(new int[4]);
		this.dropbox3_uzunluk.Location = new System.Drawing.Point(362, 391);
		this.dropbox3_uzunluk.Name = "dropbox3_uzunluk";
		this.dropbox3_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox3_uzunluk.Properties.IsFloatValue = false;
		this.dropbox3_uzunluk.Properties.Mask.EditMask = "N00";
		this.dropbox3_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.dropbox3_uzunluk.TabIndex = 243;
		this.dropbox3_baslangic.EditValue = new decimal(new int[4]);
		this.dropbox3_baslangic.Location = new System.Drawing.Point(257, 391);
		this.dropbox3_baslangic.Name = "dropbox3_baslangic";
		this.dropbox3_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox3_baslangic.Properties.IsFloatValue = false;
		this.dropbox3_baslangic.Properties.Mask.EditMask = "N00";
		this.dropbox3_baslangic.Size = new System.Drawing.Size(96, 20);
		this.dropbox3_baslangic.TabIndex = 242;
		this.dropbox3_gorunen_adi.Location = new System.Drawing.Point(17, 391);
		this.dropbox3_gorunen_adi.Name = "dropbox3_gorunen_adi";
		this.dropbox3_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.dropbox3_gorunen_adi.TabIndex = 241;
		this.label107.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label107.Location = new System.Drawing.Point(17, 368);
		this.label107.Name = "label107";
		this.label107.Size = new System.Drawing.Size(234, 19);
		this.label107.TabIndex = 240;
		this.label107.Text = "Başlık";
		this.label107.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label108.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label108.Location = new System.Drawing.Point(14, 342);
		this.label108.Name = "label108";
		this.label108.Size = new System.Drawing.Size(108, 19);
		this.label108.TabIndex = 239;
		this.label108.Text = "Açılan kutu 3 :";
		this.label108.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label109.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label109.Location = new System.Drawing.Point(359, 368);
		this.label109.Name = "label109";
		this.label109.Size = new System.Drawing.Size(54, 19);
		this.label109.TabIndex = 238;
		this.label109.Text = "Uzunluk";
		this.label109.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label110.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label110.Location = new System.Drawing.Point(257, 368);
		this.label110.Name = "label110";
		this.label110.Size = new System.Drawing.Size(96, 19);
		this.label110.TabIndex = 237;
		this.label110.Text = "Başlangıç/Sıra";
		this.label110.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label93.Location = new System.Drawing.Point(359, 313);
		this.label93.Name = "label93";
		this.label93.Size = new System.Drawing.Size(319, 19);
		this.label93.TabIndex = 236;
		this.label93.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label93.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label94.Location = new System.Drawing.Point(17, 313);
		this.label94.Name = "label94";
		this.label94.Size = new System.Drawing.Size(336, 19);
		this.label94.TabIndex = 235;
		this.label94.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label94.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dropbox2_varsayilan_deger.Location = new System.Drawing.Point(422, 234);
		this.dropbox2_varsayilan_deger.Name = "dropbox2_varsayilan_deger";
		this.dropbox2_varsayilan_deger.Size = new System.Drawing.Size(256, 20);
		this.dropbox2_varsayilan_deger.TabIndex = 234;
		this.label95.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label95.Location = new System.Drawing.Point(422, 211);
		this.label95.Name = "label95";
		this.label95.Size = new System.Drawing.Size(256, 19);
		this.label95.TabIndex = 233;
		this.label95.Text = "Varsayılan (Veri)";
		this.label95.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox2_secenekler_veri.Location = new System.Drawing.Point(362, 290);
		this.dropbox2_secenekler_veri.Name = "dropbox2_secenekler_veri";
		this.dropbox2_secenekler_veri.Size = new System.Drawing.Size(316, 20);
		this.dropbox2_secenekler_veri.TabIndex = 232;
		this.label96.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label96.Location = new System.Drawing.Point(362, 267);
		this.label96.Name = "label96";
		this.label96.Size = new System.Drawing.Size(316, 19);
		this.label96.TabIndex = 231;
		this.label96.Text = "Seçenekler (Veri)";
		this.label96.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox2_secenekler_yazi.Location = new System.Drawing.Point(17, 290);
		this.dropbox2_secenekler_yazi.Name = "dropbox2_secenekler_yazi";
		this.dropbox2_secenekler_yazi.Size = new System.Drawing.Size(336, 20);
		this.dropbox2_secenekler_yazi.TabIndex = 230;
		this.label97.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label97.Location = new System.Drawing.Point(17, 267);
		this.label97.Name = "label97";
		this.label97.Size = new System.Drawing.Size(336, 19);
		this.label97.TabIndex = 229;
		this.label97.Text = "Seçenekler (Yazı)";
		this.label97.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox2_zorunlu.Location = new System.Drawing.Point(128, 185);
		this.dropbox2_zorunlu.Name = "dropbox2_zorunlu";
		this.dropbox2_zorunlu.Properties.Caption = "Zorunlu";
		this.dropbox2_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.dropbox2_zorunlu.TabIndex = 228;
		this.dropbox2_zorunlu.Visible = false;
		this.dropbox2_uzunluk.EditValue = new decimal(new int[4]);
		this.dropbox2_uzunluk.Location = new System.Drawing.Point(362, 234);
		this.dropbox2_uzunluk.Name = "dropbox2_uzunluk";
		this.dropbox2_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox2_uzunluk.Properties.IsFloatValue = false;
		this.dropbox2_uzunluk.Properties.Mask.EditMask = "N00";
		this.dropbox2_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.dropbox2_uzunluk.TabIndex = 227;
		this.dropbox2_baslangic.EditValue = new decimal(new int[4]);
		this.dropbox2_baslangic.Location = new System.Drawing.Point(257, 234);
		this.dropbox2_baslangic.Name = "dropbox2_baslangic";
		this.dropbox2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox2_baslangic.Properties.IsFloatValue = false;
		this.dropbox2_baslangic.Properties.Mask.EditMask = "N00";
		this.dropbox2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.dropbox2_baslangic.TabIndex = 226;
		this.dropbox2_gorunen_adi.Location = new System.Drawing.Point(17, 234);
		this.dropbox2_gorunen_adi.Name = "dropbox2_gorunen_adi";
		this.dropbox2_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.dropbox2_gorunen_adi.TabIndex = 225;
		this.label98.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label98.Location = new System.Drawing.Point(17, 211);
		this.label98.Name = "label98";
		this.label98.Size = new System.Drawing.Size(234, 19);
		this.label98.TabIndex = 224;
		this.label98.Text = "Başlık";
		this.label98.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label99.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label99.Location = new System.Drawing.Point(14, 185);
		this.label99.Name = "label99";
		this.label99.Size = new System.Drawing.Size(108, 19);
		this.label99.TabIndex = 223;
		this.label99.Text = "Açılan kutu 2 :";
		this.label99.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label100.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label100.Location = new System.Drawing.Point(359, 211);
		this.label100.Name = "label100";
		this.label100.Size = new System.Drawing.Size(54, 19);
		this.label100.TabIndex = 222;
		this.label100.Text = "Uzunluk";
		this.label100.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label101.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label101.Location = new System.Drawing.Point(257, 211);
		this.label101.Name = "label101";
		this.label101.Size = new System.Drawing.Size(96, 19);
		this.label101.TabIndex = 221;
		this.label101.Text = "Başlangıç/Sıra";
		this.label101.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label92.Location = new System.Drawing.Point(359, 140);
		this.label92.Name = "label92";
		this.label92.Size = new System.Drawing.Size(319, 19);
		this.label92.TabIndex = 220;
		this.label92.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label92.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label91.Location = new System.Drawing.Point(17, 140);
		this.label91.Name = "label91";
		this.label91.Size = new System.Drawing.Size(336, 19);
		this.label91.TabIndex = 219;
		this.label91.Text = "Birden fazla seçeneği virgül ile ayırınız.";
		this.label91.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dropbox1_varsayilan_deger.Location = new System.Drawing.Point(422, 61);
		this.dropbox1_varsayilan_deger.Name = "dropbox1_varsayilan_deger";
		this.dropbox1_varsayilan_deger.Size = new System.Drawing.Size(256, 20);
		this.dropbox1_varsayilan_deger.TabIndex = 218;
		this.label90.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label90.Location = new System.Drawing.Point(422, 38);
		this.label90.Name = "label90";
		this.label90.Size = new System.Drawing.Size(256, 19);
		this.label90.TabIndex = 217;
		this.label90.Text = "Varsayılan (Veri)";
		this.label90.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox1_secenekler_veri.Location = new System.Drawing.Point(362, 117);
		this.dropbox1_secenekler_veri.Name = "dropbox1_secenekler_veri";
		this.dropbox1_secenekler_veri.Size = new System.Drawing.Size(316, 20);
		this.dropbox1_secenekler_veri.TabIndex = 216;
		this.label89.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label89.Location = new System.Drawing.Point(362, 94);
		this.label89.Name = "label89";
		this.label89.Size = new System.Drawing.Size(316, 19);
		this.label89.TabIndex = 215;
		this.label89.Text = "Seçenekler (Veri)";
		this.label89.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox1_secenekler_yazi.Location = new System.Drawing.Point(17, 117);
		this.dropbox1_secenekler_yazi.Name = "dropbox1_secenekler_yazi";
		this.dropbox1_secenekler_yazi.Size = new System.Drawing.Size(336, 20);
		this.dropbox1_secenekler_yazi.TabIndex = 214;
		this.label84.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label84.Location = new System.Drawing.Point(17, 94);
		this.label84.Name = "label84";
		this.label84.Size = new System.Drawing.Size(336, 19);
		this.label84.TabIndex = 213;
		this.label84.Text = "Seçenekler (Yazı)";
		this.label84.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.dropbox1_zorunlu.Location = new System.Drawing.Point(128, 12);
		this.dropbox1_zorunlu.Name = "dropbox1_zorunlu";
		this.dropbox1_zorunlu.Properties.Caption = "Zorunlu";
		this.dropbox1_zorunlu.Size = new System.Drawing.Size(61, 19);
		this.dropbox1_zorunlu.TabIndex = 212;
		this.dropbox1_zorunlu.Visible = false;
		this.dropbox1_uzunluk.EditValue = new decimal(new int[4]);
		this.dropbox1_uzunluk.Location = new System.Drawing.Point(362, 61);
		this.dropbox1_uzunluk.Name = "dropbox1_uzunluk";
		this.dropbox1_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox1_uzunluk.Properties.IsFloatValue = false;
		this.dropbox1_uzunluk.Properties.Mask.EditMask = "N00";
		this.dropbox1_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.dropbox1_uzunluk.TabIndex = 211;
		this.dropbox1_baslangic.EditValue = new decimal(new int[4]);
		this.dropbox1_baslangic.Location = new System.Drawing.Point(257, 61);
		this.dropbox1_baslangic.Name = "dropbox1_baslangic";
		this.dropbox1_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dropbox1_baslangic.Properties.IsFloatValue = false;
		this.dropbox1_baslangic.Properties.Mask.EditMask = "N00";
		this.dropbox1_baslangic.Size = new System.Drawing.Size(96, 20);
		this.dropbox1_baslangic.TabIndex = 210;
		this.dropbox1_gorunen_adi.Location = new System.Drawing.Point(17, 61);
		this.dropbox1_gorunen_adi.Name = "dropbox1_gorunen_adi";
		this.dropbox1_gorunen_adi.Size = new System.Drawing.Size(234, 20);
		this.dropbox1_gorunen_adi.TabIndex = 209;
		this.label85.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label85.Location = new System.Drawing.Point(17, 38);
		this.label85.Name = "label85";
		this.label85.Size = new System.Drawing.Size(234, 19);
		this.label85.TabIndex = 208;
		this.label85.Text = "Başlık";
		this.label85.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label86.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label86.Location = new System.Drawing.Point(14, 12);
		this.label86.Name = "label86";
		this.label86.Size = new System.Drawing.Size(108, 19);
		this.label86.TabIndex = 207;
		this.label86.Text = "Açılan kutu 1 :";
		this.label86.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label87.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label87.Location = new System.Drawing.Point(359, 38);
		this.label87.Name = "label87";
		this.label87.Size = new System.Drawing.Size(54, 19);
		this.label87.TabIndex = 206;
		this.label87.Text = "Uzunluk";
		this.label87.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label88.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label88.Location = new System.Drawing.Point(257, 38);
		this.label88.Name = "label88";
		this.label88.Size = new System.Drawing.Size(96, 19);
		this.label88.TabIndex = 205;
		this.label88.Text = "Başlangıç/Sıra";
		this.label88.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.xtraTabPage2.AutoScroll = true;
		this.xtraTabPage2.AutoScrollMargin = new System.Drawing.Size(0, 300);
		this.xtraTabPage2.Controls.Add(this.checkbox10_isaretsiz_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label228);
		this.xtraTabPage2.Controls.Add(this.checkbox10_isaretli_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label229);
		this.xtraTabPage2.Controls.Add(this.checkbox10_varsayilan_deger);
		this.xtraTabPage2.Controls.Add(this.checkbox10_uzunluk);
		this.xtraTabPage2.Controls.Add(this.checkbox10_baslangic);
		this.xtraTabPage2.Controls.Add(this.checkbox10_gorunen_adi);
		this.xtraTabPage2.Controls.Add(this.label230);
		this.xtraTabPage2.Controls.Add(this.label231);
		this.xtraTabPage2.Controls.Add(this.label232);
		this.xtraTabPage2.Controls.Add(this.label233);
		this.xtraTabPage2.Controls.Add(this.checkbox9_isaretsiz_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label222);
		this.xtraTabPage2.Controls.Add(this.checkbox9_isaretli_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label223);
		this.xtraTabPage2.Controls.Add(this.checkbox9_varsayilan_deger);
		this.xtraTabPage2.Controls.Add(this.checkbox9_uzunluk);
		this.xtraTabPage2.Controls.Add(this.checkbox9_baslangic);
		this.xtraTabPage2.Controls.Add(this.checkbox9_gorunen_adi);
		this.xtraTabPage2.Controls.Add(this.label224);
		this.xtraTabPage2.Controls.Add(this.label225);
		this.xtraTabPage2.Controls.Add(this.label226);
		this.xtraTabPage2.Controls.Add(this.label227);
		this.xtraTabPage2.Controls.Add(this.checkbox8_isaretsiz_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label216);
		this.xtraTabPage2.Controls.Add(this.checkbox8_isaretli_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label217);
		this.xtraTabPage2.Controls.Add(this.checkbox8_varsayilan_deger);
		this.xtraTabPage2.Controls.Add(this.checkbox8_uzunluk);
		this.xtraTabPage2.Controls.Add(this.checkbox8_baslangic);
		this.xtraTabPage2.Controls.Add(this.checkbox8_gorunen_adi);
		this.xtraTabPage2.Controls.Add(this.label218);
		this.xtraTabPage2.Controls.Add(this.label219);
		this.xtraTabPage2.Controls.Add(this.label220);
		this.xtraTabPage2.Controls.Add(this.label221);
		this.xtraTabPage2.Controls.Add(this.checkbox7_isaretsiz_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label210);
		this.xtraTabPage2.Controls.Add(this.checkbox7_isaretli_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label211);
		this.xtraTabPage2.Controls.Add(this.checkbox7_varsayilan_deger);
		this.xtraTabPage2.Controls.Add(this.checkbox7_uzunluk);
		this.xtraTabPage2.Controls.Add(this.checkbox7_baslangic);
		this.xtraTabPage2.Controls.Add(this.checkbox7_gorunen_adi);
		this.xtraTabPage2.Controls.Add(this.label212);
		this.xtraTabPage2.Controls.Add(this.label213);
		this.xtraTabPage2.Controls.Add(this.label214);
		this.xtraTabPage2.Controls.Add(this.label215);
		this.xtraTabPage2.Controls.Add(this.checkbox6_isaretsiz_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label204);
		this.xtraTabPage2.Controls.Add(this.checkbox6_isaretli_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label205);
		this.xtraTabPage2.Controls.Add(this.checkbox6_varsayilan_deger);
		this.xtraTabPage2.Controls.Add(this.checkbox6_uzunluk);
		this.xtraTabPage2.Controls.Add(this.checkbox6_baslangic);
		this.xtraTabPage2.Controls.Add(this.checkbox6_gorunen_adi);
		this.xtraTabPage2.Controls.Add(this.label206);
		this.xtraTabPage2.Controls.Add(this.label207);
		this.xtraTabPage2.Controls.Add(this.label208);
		this.xtraTabPage2.Controls.Add(this.label209);
		this.xtraTabPage2.Controls.Add(this.checkbox5_isaretsiz_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label198);
		this.xtraTabPage2.Controls.Add(this.checkbox5_isaretli_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label199);
		this.xtraTabPage2.Controls.Add(this.checkbox5_varsayilan_deger);
		this.xtraTabPage2.Controls.Add(this.checkbox5_uzunluk);
		this.xtraTabPage2.Controls.Add(this.checkbox5_baslangic);
		this.xtraTabPage2.Controls.Add(this.checkbox5_gorunen_adi);
		this.xtraTabPage2.Controls.Add(this.label200);
		this.xtraTabPage2.Controls.Add(this.label201);
		this.xtraTabPage2.Controls.Add(this.label202);
		this.xtraTabPage2.Controls.Add(this.label203);
		this.xtraTabPage2.Controls.Add(this.checkbox4_isaretsiz_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label192);
		this.xtraTabPage2.Controls.Add(this.checkbox4_isaretli_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label193);
		this.xtraTabPage2.Controls.Add(this.checkbox4_varsayilan_deger);
		this.xtraTabPage2.Controls.Add(this.checkbox4_uzunluk);
		this.xtraTabPage2.Controls.Add(this.checkbox4_baslangic);
		this.xtraTabPage2.Controls.Add(this.checkbox4_gorunen_adi);
		this.xtraTabPage2.Controls.Add(this.label194);
		this.xtraTabPage2.Controls.Add(this.label195);
		this.xtraTabPage2.Controls.Add(this.label196);
		this.xtraTabPage2.Controls.Add(this.label197);
		this.xtraTabPage2.Controls.Add(this.checkbox3_isaretsiz_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label186);
		this.xtraTabPage2.Controls.Add(this.checkbox3_isaretli_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label187);
		this.xtraTabPage2.Controls.Add(this.checkbox3_varsayilan_deger);
		this.xtraTabPage2.Controls.Add(this.checkbox3_uzunluk);
		this.xtraTabPage2.Controls.Add(this.checkbox3_baslangic);
		this.xtraTabPage2.Controls.Add(this.checkbox3_gorunen_adi);
		this.xtraTabPage2.Controls.Add(this.label188);
		this.xtraTabPage2.Controls.Add(this.label189);
		this.xtraTabPage2.Controls.Add(this.label190);
		this.xtraTabPage2.Controls.Add(this.label191);
		this.xtraTabPage2.Controls.Add(this.checkbox2_isaretsiz_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label180);
		this.xtraTabPage2.Controls.Add(this.checkbox2_isaretli_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label181);
		this.xtraTabPage2.Controls.Add(this.checkbox2_varsayilan_deger);
		this.xtraTabPage2.Controls.Add(this.checkbox2_uzunluk);
		this.xtraTabPage2.Controls.Add(this.checkbox2_baslangic);
		this.xtraTabPage2.Controls.Add(this.checkbox2_gorunen_adi);
		this.xtraTabPage2.Controls.Add(this.label182);
		this.xtraTabPage2.Controls.Add(this.label183);
		this.xtraTabPage2.Controls.Add(this.label184);
		this.xtraTabPage2.Controls.Add(this.label185);
		this.xtraTabPage2.Controls.Add(this.checkbox1_isaretsiz_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label179);
		this.xtraTabPage2.Controls.Add(this.checkbox1_isaretli_icin_deger);
		this.xtraTabPage2.Controls.Add(this.label178);
		this.xtraTabPage2.Controls.Add(this.checkbox1_varsayilan_deger);
		this.xtraTabPage2.Controls.Add(this.checkbox1_uzunluk);
		this.xtraTabPage2.Controls.Add(this.checkbox1_baslangic);
		this.xtraTabPage2.Controls.Add(this.checkbox1_gorunen_adi);
		this.xtraTabPage2.Controls.Add(this.label174);
		this.xtraTabPage2.Controls.Add(this.label175);
		this.xtraTabPage2.Controls.Add(this.label176);
		this.xtraTabPage2.Controls.Add(this.label177);
		this.xtraTabPage2.Name = "xtraTabPage2";
		this.xtraTabPage2.Size = new System.Drawing.Size(734, 424);
		this.xtraTabPage2.Text = "Onay kutusu alanları";
		this.checkbox10_isaretsiz_icin_deger.Location = new System.Drawing.Point(587, 830);
		this.checkbox10_isaretsiz_icin_deger.Name = "checkbox10_isaretsiz_icin_deger";
		this.checkbox10_isaretsiz_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox10_isaretsiz_icin_deger.TabIndex = 332;
		this.label228.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label228.Location = new System.Drawing.Point(587, 807);
		this.label228.Name = "label228";
		this.label228.Size = new System.Drawing.Size(124, 19);
		this.label228.TabIndex = 331;
		this.label228.Text = "İşaretsiz için değer";
		this.label228.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox10_isaretli_icin_deger.Location = new System.Drawing.Point(457, 830);
		this.checkbox10_isaretli_icin_deger.Name = "checkbox10_isaretli_icin_deger";
		this.checkbox10_isaretli_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox10_isaretli_icin_deger.TabIndex = 330;
		this.label229.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label229.Location = new System.Drawing.Point(457, 807);
		this.label229.Name = "label229";
		this.label229.Size = new System.Drawing.Size(124, 19);
		this.label229.TabIndex = 329;
		this.label229.Text = "İşaretli için değer";
		this.label229.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox10_varsayilan_deger.Location = new System.Drawing.Point(346, 830);
		this.checkbox10_varsayilan_deger.Name = "checkbox10_varsayilan_deger";
		this.checkbox10_varsayilan_deger.Properties.Caption = "Varsayılan değer";
		this.checkbox10_varsayilan_deger.Size = new System.Drawing.Size(105, 19);
		this.checkbox10_varsayilan_deger.TabIndex = 328;
		this.checkbox10_uzunluk.EditValue = new decimal(new int[4]);
		this.checkbox10_uzunluk.Location = new System.Drawing.Point(286, 830);
		this.checkbox10_uzunluk.Name = "checkbox10_uzunluk";
		this.checkbox10_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox10_uzunluk.Properties.IsFloatValue = false;
		this.checkbox10_uzunluk.Properties.Mask.EditMask = "N00";
		this.checkbox10_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.checkbox10_uzunluk.TabIndex = 327;
		this.checkbox10_baslangic.EditValue = new decimal(new int[4]);
		this.checkbox10_baslangic.Location = new System.Drawing.Point(181, 830);
		this.checkbox10_baslangic.Name = "checkbox10_baslangic";
		this.checkbox10_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox10_baslangic.Properties.IsFloatValue = false;
		this.checkbox10_baslangic.Properties.Mask.EditMask = "N00";
		this.checkbox10_baslangic.Size = new System.Drawing.Size(96, 20);
		this.checkbox10_baslangic.TabIndex = 326;
		this.checkbox10_gorunen_adi.Location = new System.Drawing.Point(12, 830);
		this.checkbox10_gorunen_adi.Name = "checkbox10_gorunen_adi";
		this.checkbox10_gorunen_adi.Size = new System.Drawing.Size(163, 20);
		this.checkbox10_gorunen_adi.TabIndex = 325;
		this.label230.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label230.Location = new System.Drawing.Point(12, 807);
		this.label230.Name = "label230";
		this.label230.Size = new System.Drawing.Size(163, 19);
		this.label230.TabIndex = 324;
		this.label230.Text = "Başlık";
		this.label230.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label231.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label231.Location = new System.Drawing.Point(9, 781);
		this.label231.Name = "label231";
		this.label231.Size = new System.Drawing.Size(135, 19);
		this.label231.TabIndex = 323;
		this.label231.Text = "Onay kutusu 10 :";
		this.label231.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label232.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label232.Location = new System.Drawing.Point(283, 807);
		this.label232.Name = "label232";
		this.label232.Size = new System.Drawing.Size(54, 19);
		this.label232.TabIndex = 322;
		this.label232.Text = "Uzunluk";
		this.label232.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label233.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label233.Location = new System.Drawing.Point(181, 807);
		this.label233.Name = "label233";
		this.label233.Size = new System.Drawing.Size(96, 19);
		this.label233.TabIndex = 321;
		this.label233.Text = "Başlangıç/Sıra";
		this.label233.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox9_isaretsiz_icin_deger.Location = new System.Drawing.Point(587, 750);
		this.checkbox9_isaretsiz_icin_deger.Name = "checkbox9_isaretsiz_icin_deger";
		this.checkbox9_isaretsiz_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox9_isaretsiz_icin_deger.TabIndex = 320;
		this.label222.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label222.Location = new System.Drawing.Point(587, 727);
		this.label222.Name = "label222";
		this.label222.Size = new System.Drawing.Size(124, 19);
		this.label222.TabIndex = 319;
		this.label222.Text = "İşaretsiz için değer";
		this.label222.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox9_isaretli_icin_deger.Location = new System.Drawing.Point(457, 750);
		this.checkbox9_isaretli_icin_deger.Name = "checkbox9_isaretli_icin_deger";
		this.checkbox9_isaretli_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox9_isaretli_icin_deger.TabIndex = 318;
		this.label223.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label223.Location = new System.Drawing.Point(457, 727);
		this.label223.Name = "label223";
		this.label223.Size = new System.Drawing.Size(124, 19);
		this.label223.TabIndex = 317;
		this.label223.Text = "İşaretli için değer";
		this.label223.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox9_varsayilan_deger.Location = new System.Drawing.Point(346, 750);
		this.checkbox9_varsayilan_deger.Name = "checkbox9_varsayilan_deger";
		this.checkbox9_varsayilan_deger.Properties.Caption = "Varsayılan değer";
		this.checkbox9_varsayilan_deger.Size = new System.Drawing.Size(105, 19);
		this.checkbox9_varsayilan_deger.TabIndex = 316;
		this.checkbox9_uzunluk.EditValue = new decimal(new int[4]);
		this.checkbox9_uzunluk.Location = new System.Drawing.Point(286, 750);
		this.checkbox9_uzunluk.Name = "checkbox9_uzunluk";
		this.checkbox9_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox9_uzunluk.Properties.IsFloatValue = false;
		this.checkbox9_uzunluk.Properties.Mask.EditMask = "N00";
		this.checkbox9_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.checkbox9_uzunluk.TabIndex = 315;
		this.checkbox9_baslangic.EditValue = new decimal(new int[4]);
		this.checkbox9_baslangic.Location = new System.Drawing.Point(181, 750);
		this.checkbox9_baslangic.Name = "checkbox9_baslangic";
		this.checkbox9_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox9_baslangic.Properties.IsFloatValue = false;
		this.checkbox9_baslangic.Properties.Mask.EditMask = "N00";
		this.checkbox9_baslangic.Size = new System.Drawing.Size(96, 20);
		this.checkbox9_baslangic.TabIndex = 314;
		this.checkbox9_gorunen_adi.Location = new System.Drawing.Point(12, 750);
		this.checkbox9_gorunen_adi.Name = "checkbox9_gorunen_adi";
		this.checkbox9_gorunen_adi.Size = new System.Drawing.Size(163, 20);
		this.checkbox9_gorunen_adi.TabIndex = 313;
		this.label224.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label224.Location = new System.Drawing.Point(12, 727);
		this.label224.Name = "label224";
		this.label224.Size = new System.Drawing.Size(163, 19);
		this.label224.TabIndex = 312;
		this.label224.Text = "Başlık";
		this.label224.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label225.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label225.Location = new System.Drawing.Point(9, 701);
		this.label225.Name = "label225";
		this.label225.Size = new System.Drawing.Size(135, 19);
		this.label225.TabIndex = 311;
		this.label225.Text = "Onay kutusu 9 :";
		this.label225.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label226.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label226.Location = new System.Drawing.Point(283, 727);
		this.label226.Name = "label226";
		this.label226.Size = new System.Drawing.Size(54, 19);
		this.label226.TabIndex = 310;
		this.label226.Text = "Uzunluk";
		this.label226.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label227.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label227.Location = new System.Drawing.Point(181, 727);
		this.label227.Name = "label227";
		this.label227.Size = new System.Drawing.Size(96, 19);
		this.label227.TabIndex = 309;
		this.label227.Text = "Başlangıç/Sıra";
		this.label227.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox8_isaretsiz_icin_deger.Location = new System.Drawing.Point(587, 657);
		this.checkbox8_isaretsiz_icin_deger.Name = "checkbox8_isaretsiz_icin_deger";
		this.checkbox8_isaretsiz_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox8_isaretsiz_icin_deger.TabIndex = 308;
		this.label216.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label216.Location = new System.Drawing.Point(587, 634);
		this.label216.Name = "label216";
		this.label216.Size = new System.Drawing.Size(124, 19);
		this.label216.TabIndex = 307;
		this.label216.Text = "İşaretsiz için değer";
		this.label216.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox8_isaretli_icin_deger.Location = new System.Drawing.Point(457, 657);
		this.checkbox8_isaretli_icin_deger.Name = "checkbox8_isaretli_icin_deger";
		this.checkbox8_isaretli_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox8_isaretli_icin_deger.TabIndex = 306;
		this.label217.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label217.Location = new System.Drawing.Point(457, 634);
		this.label217.Name = "label217";
		this.label217.Size = new System.Drawing.Size(124, 19);
		this.label217.TabIndex = 305;
		this.label217.Text = "İşaretli için değer";
		this.label217.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox8_varsayilan_deger.Location = new System.Drawing.Point(346, 657);
		this.checkbox8_varsayilan_deger.Name = "checkbox8_varsayilan_deger";
		this.checkbox8_varsayilan_deger.Properties.Caption = "Varsayılan değer";
		this.checkbox8_varsayilan_deger.Size = new System.Drawing.Size(105, 19);
		this.checkbox8_varsayilan_deger.TabIndex = 304;
		this.checkbox8_uzunluk.EditValue = new decimal(new int[4]);
		this.checkbox8_uzunluk.Location = new System.Drawing.Point(286, 657);
		this.checkbox8_uzunluk.Name = "checkbox8_uzunluk";
		this.checkbox8_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox8_uzunluk.Properties.IsFloatValue = false;
		this.checkbox8_uzunluk.Properties.Mask.EditMask = "N00";
		this.checkbox8_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.checkbox8_uzunluk.TabIndex = 303;
		this.checkbox8_baslangic.EditValue = new decimal(new int[4]);
		this.checkbox8_baslangic.Location = new System.Drawing.Point(181, 657);
		this.checkbox8_baslangic.Name = "checkbox8_baslangic";
		this.checkbox8_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox8_baslangic.Properties.IsFloatValue = false;
		this.checkbox8_baslangic.Properties.Mask.EditMask = "N00";
		this.checkbox8_baslangic.Size = new System.Drawing.Size(96, 20);
		this.checkbox8_baslangic.TabIndex = 302;
		this.checkbox8_gorunen_adi.Location = new System.Drawing.Point(12, 657);
		this.checkbox8_gorunen_adi.Name = "checkbox8_gorunen_adi";
		this.checkbox8_gorunen_adi.Size = new System.Drawing.Size(163, 20);
		this.checkbox8_gorunen_adi.TabIndex = 301;
		this.label218.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label218.Location = new System.Drawing.Point(12, 634);
		this.label218.Name = "label218";
		this.label218.Size = new System.Drawing.Size(163, 19);
		this.label218.TabIndex = 300;
		this.label218.Text = "Başlık";
		this.label218.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label219.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label219.Location = new System.Drawing.Point(9, 608);
		this.label219.Name = "label219";
		this.label219.Size = new System.Drawing.Size(135, 19);
		this.label219.TabIndex = 299;
		this.label219.Text = "Onay kutusu 8 :";
		this.label219.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label220.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label220.Location = new System.Drawing.Point(283, 634);
		this.label220.Name = "label220";
		this.label220.Size = new System.Drawing.Size(54, 19);
		this.label220.TabIndex = 298;
		this.label220.Text = "Uzunluk";
		this.label220.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label221.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label221.Location = new System.Drawing.Point(181, 634);
		this.label221.Name = "label221";
		this.label221.Size = new System.Drawing.Size(96, 19);
		this.label221.TabIndex = 297;
		this.label221.Text = "Başlangıç/Sıra";
		this.label221.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox7_isaretsiz_icin_deger.Location = new System.Drawing.Point(587, 575);
		this.checkbox7_isaretsiz_icin_deger.Name = "checkbox7_isaretsiz_icin_deger";
		this.checkbox7_isaretsiz_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox7_isaretsiz_icin_deger.TabIndex = 296;
		this.label210.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label210.Location = new System.Drawing.Point(587, 552);
		this.label210.Name = "label210";
		this.label210.Size = new System.Drawing.Size(124, 19);
		this.label210.TabIndex = 295;
		this.label210.Text = "İşaretsiz için değer";
		this.label210.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox7_isaretli_icin_deger.Location = new System.Drawing.Point(457, 575);
		this.checkbox7_isaretli_icin_deger.Name = "checkbox7_isaretli_icin_deger";
		this.checkbox7_isaretli_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox7_isaretli_icin_deger.TabIndex = 294;
		this.label211.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label211.Location = new System.Drawing.Point(457, 552);
		this.label211.Name = "label211";
		this.label211.Size = new System.Drawing.Size(124, 19);
		this.label211.TabIndex = 293;
		this.label211.Text = "İşaretli için değer";
		this.label211.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox7_varsayilan_deger.Location = new System.Drawing.Point(346, 575);
		this.checkbox7_varsayilan_deger.Name = "checkbox7_varsayilan_deger";
		this.checkbox7_varsayilan_deger.Properties.Caption = "Varsayılan değer";
		this.checkbox7_varsayilan_deger.Size = new System.Drawing.Size(105, 19);
		this.checkbox7_varsayilan_deger.TabIndex = 292;
		this.checkbox7_uzunluk.EditValue = new decimal(new int[4]);
		this.checkbox7_uzunluk.Location = new System.Drawing.Point(286, 575);
		this.checkbox7_uzunluk.Name = "checkbox7_uzunluk";
		this.checkbox7_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox7_uzunluk.Properties.IsFloatValue = false;
		this.checkbox7_uzunluk.Properties.Mask.EditMask = "N00";
		this.checkbox7_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.checkbox7_uzunluk.TabIndex = 291;
		this.checkbox7_baslangic.EditValue = new decimal(new int[4]);
		this.checkbox7_baslangic.Location = new System.Drawing.Point(181, 575);
		this.checkbox7_baslangic.Name = "checkbox7_baslangic";
		this.checkbox7_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox7_baslangic.Properties.IsFloatValue = false;
		this.checkbox7_baslangic.Properties.Mask.EditMask = "N00";
		this.checkbox7_baslangic.Size = new System.Drawing.Size(96, 20);
		this.checkbox7_baslangic.TabIndex = 290;
		this.checkbox7_gorunen_adi.Location = new System.Drawing.Point(12, 575);
		this.checkbox7_gorunen_adi.Name = "checkbox7_gorunen_adi";
		this.checkbox7_gorunen_adi.Size = new System.Drawing.Size(163, 20);
		this.checkbox7_gorunen_adi.TabIndex = 289;
		this.label212.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label212.Location = new System.Drawing.Point(12, 552);
		this.label212.Name = "label212";
		this.label212.Size = new System.Drawing.Size(163, 19);
		this.label212.TabIndex = 288;
		this.label212.Text = "Başlık";
		this.label212.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label213.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label213.Location = new System.Drawing.Point(9, 526);
		this.label213.Name = "label213";
		this.label213.Size = new System.Drawing.Size(135, 19);
		this.label213.TabIndex = 287;
		this.label213.Text = "Onay kutusu 7 :";
		this.label213.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label214.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label214.Location = new System.Drawing.Point(283, 552);
		this.label214.Name = "label214";
		this.label214.Size = new System.Drawing.Size(54, 19);
		this.label214.TabIndex = 286;
		this.label214.Text = "Uzunluk";
		this.label214.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label215.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label215.Location = new System.Drawing.Point(181, 552);
		this.label215.Name = "label215";
		this.label215.Size = new System.Drawing.Size(96, 19);
		this.label215.TabIndex = 285;
		this.label215.Text = "Başlangıç/Sıra";
		this.label215.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox6_isaretsiz_icin_deger.Location = new System.Drawing.Point(587, 488);
		this.checkbox6_isaretsiz_icin_deger.Name = "checkbox6_isaretsiz_icin_deger";
		this.checkbox6_isaretsiz_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox6_isaretsiz_icin_deger.TabIndex = 284;
		this.label204.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label204.Location = new System.Drawing.Point(587, 465);
		this.label204.Name = "label204";
		this.label204.Size = new System.Drawing.Size(124, 19);
		this.label204.TabIndex = 283;
		this.label204.Text = "İşaretsiz için değer";
		this.label204.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox6_isaretli_icin_deger.Location = new System.Drawing.Point(457, 488);
		this.checkbox6_isaretli_icin_deger.Name = "checkbox6_isaretli_icin_deger";
		this.checkbox6_isaretli_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox6_isaretli_icin_deger.TabIndex = 282;
		this.label205.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label205.Location = new System.Drawing.Point(457, 465);
		this.label205.Name = "label205";
		this.label205.Size = new System.Drawing.Size(124, 19);
		this.label205.TabIndex = 281;
		this.label205.Text = "İşaretli için değer";
		this.label205.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox6_varsayilan_deger.Location = new System.Drawing.Point(346, 488);
		this.checkbox6_varsayilan_deger.Name = "checkbox6_varsayilan_deger";
		this.checkbox6_varsayilan_deger.Properties.Caption = "Varsayılan değer";
		this.checkbox6_varsayilan_deger.Size = new System.Drawing.Size(105, 19);
		this.checkbox6_varsayilan_deger.TabIndex = 280;
		this.checkbox6_uzunluk.EditValue = new decimal(new int[4]);
		this.checkbox6_uzunluk.Location = new System.Drawing.Point(286, 488);
		this.checkbox6_uzunluk.Name = "checkbox6_uzunluk";
		this.checkbox6_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox6_uzunluk.Properties.IsFloatValue = false;
		this.checkbox6_uzunluk.Properties.Mask.EditMask = "N00";
		this.checkbox6_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.checkbox6_uzunluk.TabIndex = 279;
		this.checkbox6_baslangic.EditValue = new decimal(new int[4]);
		this.checkbox6_baslangic.Location = new System.Drawing.Point(181, 488);
		this.checkbox6_baslangic.Name = "checkbox6_baslangic";
		this.checkbox6_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox6_baslangic.Properties.IsFloatValue = false;
		this.checkbox6_baslangic.Properties.Mask.EditMask = "N00";
		this.checkbox6_baslangic.Size = new System.Drawing.Size(96, 20);
		this.checkbox6_baslangic.TabIndex = 278;
		this.checkbox6_gorunen_adi.Location = new System.Drawing.Point(12, 488);
		this.checkbox6_gorunen_adi.Name = "checkbox6_gorunen_adi";
		this.checkbox6_gorunen_adi.Size = new System.Drawing.Size(163, 20);
		this.checkbox6_gorunen_adi.TabIndex = 277;
		this.label206.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label206.Location = new System.Drawing.Point(12, 465);
		this.label206.Name = "label206";
		this.label206.Size = new System.Drawing.Size(163, 19);
		this.label206.TabIndex = 276;
		this.label206.Text = "Başlık";
		this.label206.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label207.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label207.Location = new System.Drawing.Point(9, 439);
		this.label207.Name = "label207";
		this.label207.Size = new System.Drawing.Size(135, 19);
		this.label207.TabIndex = 275;
		this.label207.Text = "Onay kutusu 6 :";
		this.label207.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label208.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label208.Location = new System.Drawing.Point(283, 465);
		this.label208.Name = "label208";
		this.label208.Size = new System.Drawing.Size(54, 19);
		this.label208.TabIndex = 274;
		this.label208.Text = "Uzunluk";
		this.label208.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label209.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label209.Location = new System.Drawing.Point(181, 465);
		this.label209.Name = "label209";
		this.label209.Size = new System.Drawing.Size(96, 19);
		this.label209.TabIndex = 273;
		this.label209.Text = "Başlangıç/Sıra";
		this.label209.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox5_isaretsiz_icin_deger.Location = new System.Drawing.Point(587, 401);
		this.checkbox5_isaretsiz_icin_deger.Name = "checkbox5_isaretsiz_icin_deger";
		this.checkbox5_isaretsiz_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox5_isaretsiz_icin_deger.TabIndex = 272;
		this.label198.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label198.Location = new System.Drawing.Point(587, 378);
		this.label198.Name = "label198";
		this.label198.Size = new System.Drawing.Size(124, 19);
		this.label198.TabIndex = 271;
		this.label198.Text = "İşaretsiz için değer";
		this.label198.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox5_isaretli_icin_deger.Location = new System.Drawing.Point(457, 401);
		this.checkbox5_isaretli_icin_deger.Name = "checkbox5_isaretli_icin_deger";
		this.checkbox5_isaretli_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox5_isaretli_icin_deger.TabIndex = 270;
		this.label199.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label199.Location = new System.Drawing.Point(457, 378);
		this.label199.Name = "label199";
		this.label199.Size = new System.Drawing.Size(124, 19);
		this.label199.TabIndex = 269;
		this.label199.Text = "İşaretli için değer";
		this.label199.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox5_varsayilan_deger.Location = new System.Drawing.Point(346, 401);
		this.checkbox5_varsayilan_deger.Name = "checkbox5_varsayilan_deger";
		this.checkbox5_varsayilan_deger.Properties.Caption = "Varsayılan değer";
		this.checkbox5_varsayilan_deger.Size = new System.Drawing.Size(105, 19);
		this.checkbox5_varsayilan_deger.TabIndex = 268;
		this.checkbox5_uzunluk.EditValue = new decimal(new int[4]);
		this.checkbox5_uzunluk.Location = new System.Drawing.Point(286, 401);
		this.checkbox5_uzunluk.Name = "checkbox5_uzunluk";
		this.checkbox5_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox5_uzunluk.Properties.IsFloatValue = false;
		this.checkbox5_uzunluk.Properties.Mask.EditMask = "N00";
		this.checkbox5_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.checkbox5_uzunluk.TabIndex = 267;
		this.checkbox5_baslangic.EditValue = new decimal(new int[4]);
		this.checkbox5_baslangic.Location = new System.Drawing.Point(181, 401);
		this.checkbox5_baslangic.Name = "checkbox5_baslangic";
		this.checkbox5_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox5_baslangic.Properties.IsFloatValue = false;
		this.checkbox5_baslangic.Properties.Mask.EditMask = "N00";
		this.checkbox5_baslangic.Size = new System.Drawing.Size(96, 20);
		this.checkbox5_baslangic.TabIndex = 266;
		this.checkbox5_gorunen_adi.Location = new System.Drawing.Point(12, 401);
		this.checkbox5_gorunen_adi.Name = "checkbox5_gorunen_adi";
		this.checkbox5_gorunen_adi.Size = new System.Drawing.Size(163, 20);
		this.checkbox5_gorunen_adi.TabIndex = 265;
		this.label200.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label200.Location = new System.Drawing.Point(12, 378);
		this.label200.Name = "label200";
		this.label200.Size = new System.Drawing.Size(163, 19);
		this.label200.TabIndex = 264;
		this.label200.Text = "Başlık";
		this.label200.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label201.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label201.Location = new System.Drawing.Point(9, 352);
		this.label201.Name = "label201";
		this.label201.Size = new System.Drawing.Size(135, 19);
		this.label201.TabIndex = 263;
		this.label201.Text = "Onay kutusu 5 :";
		this.label201.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label202.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label202.Location = new System.Drawing.Point(283, 378);
		this.label202.Name = "label202";
		this.label202.Size = new System.Drawing.Size(54, 19);
		this.label202.TabIndex = 262;
		this.label202.Text = "Uzunluk";
		this.label202.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label203.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label203.Location = new System.Drawing.Point(181, 378);
		this.label203.Name = "label203";
		this.label203.Size = new System.Drawing.Size(96, 19);
		this.label203.TabIndex = 261;
		this.label203.Text = "Başlangıç/Sıra";
		this.label203.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox4_isaretsiz_icin_deger.Location = new System.Drawing.Point(587, 315);
		this.checkbox4_isaretsiz_icin_deger.Name = "checkbox4_isaretsiz_icin_deger";
		this.checkbox4_isaretsiz_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox4_isaretsiz_icin_deger.TabIndex = 260;
		this.label192.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label192.Location = new System.Drawing.Point(587, 292);
		this.label192.Name = "label192";
		this.label192.Size = new System.Drawing.Size(124, 19);
		this.label192.TabIndex = 259;
		this.label192.Text = "İşaretsiz için değer";
		this.label192.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox4_isaretli_icin_deger.Location = new System.Drawing.Point(457, 315);
		this.checkbox4_isaretli_icin_deger.Name = "checkbox4_isaretli_icin_deger";
		this.checkbox4_isaretli_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox4_isaretli_icin_deger.TabIndex = 258;
		this.label193.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label193.Location = new System.Drawing.Point(457, 292);
		this.label193.Name = "label193";
		this.label193.Size = new System.Drawing.Size(124, 19);
		this.label193.TabIndex = 257;
		this.label193.Text = "İşaretli için değer";
		this.label193.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox4_varsayilan_deger.Location = new System.Drawing.Point(346, 315);
		this.checkbox4_varsayilan_deger.Name = "checkbox4_varsayilan_deger";
		this.checkbox4_varsayilan_deger.Properties.Caption = "Varsayılan değer";
		this.checkbox4_varsayilan_deger.Size = new System.Drawing.Size(105, 19);
		this.checkbox4_varsayilan_deger.TabIndex = 256;
		this.checkbox4_uzunluk.EditValue = new decimal(new int[4]);
		this.checkbox4_uzunluk.Location = new System.Drawing.Point(286, 315);
		this.checkbox4_uzunluk.Name = "checkbox4_uzunluk";
		this.checkbox4_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox4_uzunluk.Properties.IsFloatValue = false;
		this.checkbox4_uzunluk.Properties.Mask.EditMask = "N00";
		this.checkbox4_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.checkbox4_uzunluk.TabIndex = 255;
		this.checkbox4_baslangic.EditValue = new decimal(new int[4]);
		this.checkbox4_baslangic.Location = new System.Drawing.Point(181, 315);
		this.checkbox4_baslangic.Name = "checkbox4_baslangic";
		this.checkbox4_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox4_baslangic.Properties.IsFloatValue = false;
		this.checkbox4_baslangic.Properties.Mask.EditMask = "N00";
		this.checkbox4_baslangic.Size = new System.Drawing.Size(96, 20);
		this.checkbox4_baslangic.TabIndex = 254;
		this.checkbox4_gorunen_adi.Location = new System.Drawing.Point(12, 315);
		this.checkbox4_gorunen_adi.Name = "checkbox4_gorunen_adi";
		this.checkbox4_gorunen_adi.Size = new System.Drawing.Size(163, 20);
		this.checkbox4_gorunen_adi.TabIndex = 253;
		this.label194.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label194.Location = new System.Drawing.Point(12, 292);
		this.label194.Name = "label194";
		this.label194.Size = new System.Drawing.Size(163, 19);
		this.label194.TabIndex = 252;
		this.label194.Text = "Başlık";
		this.label194.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label195.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label195.Location = new System.Drawing.Point(9, 266);
		this.label195.Name = "label195";
		this.label195.Size = new System.Drawing.Size(135, 19);
		this.label195.TabIndex = 251;
		this.label195.Text = "Onay kutusu 4 :";
		this.label195.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label196.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label196.Location = new System.Drawing.Point(283, 292);
		this.label196.Name = "label196";
		this.label196.Size = new System.Drawing.Size(54, 19);
		this.label196.TabIndex = 250;
		this.label196.Text = "Uzunluk";
		this.label196.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label197.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label197.Location = new System.Drawing.Point(181, 292);
		this.label197.Name = "label197";
		this.label197.Size = new System.Drawing.Size(96, 19);
		this.label197.TabIndex = 249;
		this.label197.Text = "Başlangıç/Sıra";
		this.label197.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox3_isaretsiz_icin_deger.Location = new System.Drawing.Point(587, 228);
		this.checkbox3_isaretsiz_icin_deger.Name = "checkbox3_isaretsiz_icin_deger";
		this.checkbox3_isaretsiz_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox3_isaretsiz_icin_deger.TabIndex = 248;
		this.label186.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label186.Location = new System.Drawing.Point(587, 205);
		this.label186.Name = "label186";
		this.label186.Size = new System.Drawing.Size(124, 19);
		this.label186.TabIndex = 247;
		this.label186.Text = "İşaretsiz için değer";
		this.label186.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox3_isaretli_icin_deger.Location = new System.Drawing.Point(457, 228);
		this.checkbox3_isaretli_icin_deger.Name = "checkbox3_isaretli_icin_deger";
		this.checkbox3_isaretli_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox3_isaretli_icin_deger.TabIndex = 246;
		this.label187.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label187.Location = new System.Drawing.Point(457, 205);
		this.label187.Name = "label187";
		this.label187.Size = new System.Drawing.Size(124, 19);
		this.label187.TabIndex = 245;
		this.label187.Text = "İşaretli için değer";
		this.label187.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox3_varsayilan_deger.Location = new System.Drawing.Point(346, 228);
		this.checkbox3_varsayilan_deger.Name = "checkbox3_varsayilan_deger";
		this.checkbox3_varsayilan_deger.Properties.Caption = "Varsayılan değer";
		this.checkbox3_varsayilan_deger.Size = new System.Drawing.Size(105, 19);
		this.checkbox3_varsayilan_deger.TabIndex = 244;
		this.checkbox3_uzunluk.EditValue = new decimal(new int[4]);
		this.checkbox3_uzunluk.Location = new System.Drawing.Point(286, 228);
		this.checkbox3_uzunluk.Name = "checkbox3_uzunluk";
		this.checkbox3_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox3_uzunluk.Properties.IsFloatValue = false;
		this.checkbox3_uzunluk.Properties.Mask.EditMask = "N00";
		this.checkbox3_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.checkbox3_uzunluk.TabIndex = 243;
		this.checkbox3_baslangic.EditValue = new decimal(new int[4]);
		this.checkbox3_baslangic.Location = new System.Drawing.Point(181, 228);
		this.checkbox3_baslangic.Name = "checkbox3_baslangic";
		this.checkbox3_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox3_baslangic.Properties.IsFloatValue = false;
		this.checkbox3_baslangic.Properties.Mask.EditMask = "N00";
		this.checkbox3_baslangic.Size = new System.Drawing.Size(96, 20);
		this.checkbox3_baslangic.TabIndex = 242;
		this.checkbox3_gorunen_adi.Location = new System.Drawing.Point(12, 228);
		this.checkbox3_gorunen_adi.Name = "checkbox3_gorunen_adi";
		this.checkbox3_gorunen_adi.Size = new System.Drawing.Size(163, 20);
		this.checkbox3_gorunen_adi.TabIndex = 241;
		this.label188.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label188.Location = new System.Drawing.Point(12, 205);
		this.label188.Name = "label188";
		this.label188.Size = new System.Drawing.Size(163, 19);
		this.label188.TabIndex = 240;
		this.label188.Text = "Başlık";
		this.label188.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label189.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label189.Location = new System.Drawing.Point(9, 179);
		this.label189.Name = "label189";
		this.label189.Size = new System.Drawing.Size(135, 19);
		this.label189.TabIndex = 239;
		this.label189.Text = "Onay kutusu 3 :";
		this.label189.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label190.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label190.Location = new System.Drawing.Point(283, 205);
		this.label190.Name = "label190";
		this.label190.Size = new System.Drawing.Size(54, 19);
		this.label190.TabIndex = 238;
		this.label190.Text = "Uzunluk";
		this.label190.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label191.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label191.Location = new System.Drawing.Point(181, 205);
		this.label191.Name = "label191";
		this.label191.Size = new System.Drawing.Size(96, 19);
		this.label191.TabIndex = 237;
		this.label191.Text = "Başlangıç/Sıra";
		this.label191.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox2_isaretsiz_icin_deger.Location = new System.Drawing.Point(587, 145);
		this.checkbox2_isaretsiz_icin_deger.Name = "checkbox2_isaretsiz_icin_deger";
		this.checkbox2_isaretsiz_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox2_isaretsiz_icin_deger.TabIndex = 236;
		this.label180.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label180.Location = new System.Drawing.Point(587, 122);
		this.label180.Name = "label180";
		this.label180.Size = new System.Drawing.Size(124, 19);
		this.label180.TabIndex = 235;
		this.label180.Text = "İşaretsiz için değer";
		this.label180.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox2_isaretli_icin_deger.Location = new System.Drawing.Point(457, 145);
		this.checkbox2_isaretli_icin_deger.Name = "checkbox2_isaretli_icin_deger";
		this.checkbox2_isaretli_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox2_isaretli_icin_deger.TabIndex = 234;
		this.label181.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label181.Location = new System.Drawing.Point(457, 122);
		this.label181.Name = "label181";
		this.label181.Size = new System.Drawing.Size(124, 19);
		this.label181.TabIndex = 233;
		this.label181.Text = "İşaretli için değer";
		this.label181.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox2_varsayilan_deger.Location = new System.Drawing.Point(346, 145);
		this.checkbox2_varsayilan_deger.Name = "checkbox2_varsayilan_deger";
		this.checkbox2_varsayilan_deger.Properties.Caption = "Varsayılan değer";
		this.checkbox2_varsayilan_deger.Size = new System.Drawing.Size(105, 19);
		this.checkbox2_varsayilan_deger.TabIndex = 232;
		this.checkbox2_uzunluk.EditValue = new decimal(new int[4]);
		this.checkbox2_uzunluk.Location = new System.Drawing.Point(286, 145);
		this.checkbox2_uzunluk.Name = "checkbox2_uzunluk";
		this.checkbox2_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox2_uzunluk.Properties.IsFloatValue = false;
		this.checkbox2_uzunluk.Properties.Mask.EditMask = "N00";
		this.checkbox2_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.checkbox2_uzunluk.TabIndex = 231;
		this.checkbox2_baslangic.EditValue = new decimal(new int[4]);
		this.checkbox2_baslangic.Location = new System.Drawing.Point(181, 145);
		this.checkbox2_baslangic.Name = "checkbox2_baslangic";
		this.checkbox2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox2_baslangic.Properties.IsFloatValue = false;
		this.checkbox2_baslangic.Properties.Mask.EditMask = "N00";
		this.checkbox2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.checkbox2_baslangic.TabIndex = 230;
		this.checkbox2_gorunen_adi.Location = new System.Drawing.Point(12, 145);
		this.checkbox2_gorunen_adi.Name = "checkbox2_gorunen_adi";
		this.checkbox2_gorunen_adi.Size = new System.Drawing.Size(163, 20);
		this.checkbox2_gorunen_adi.TabIndex = 229;
		this.label182.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label182.Location = new System.Drawing.Point(12, 122);
		this.label182.Name = "label182";
		this.label182.Size = new System.Drawing.Size(163, 19);
		this.label182.TabIndex = 228;
		this.label182.Text = "Başlık";
		this.label182.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label183.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label183.Location = new System.Drawing.Point(9, 96);
		this.label183.Name = "label183";
		this.label183.Size = new System.Drawing.Size(135, 19);
		this.label183.TabIndex = 227;
		this.label183.Text = "Onay kutusu 2 :";
		this.label183.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label184.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label184.Location = new System.Drawing.Point(283, 122);
		this.label184.Name = "label184";
		this.label184.Size = new System.Drawing.Size(54, 19);
		this.label184.TabIndex = 226;
		this.label184.Text = "Uzunluk";
		this.label184.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label185.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label185.Location = new System.Drawing.Point(181, 122);
		this.label185.Name = "label185";
		this.label185.Size = new System.Drawing.Size(96, 19);
		this.label185.TabIndex = 225;
		this.label185.Text = "Başlangıç/Sıra";
		this.label185.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox1_isaretsiz_icin_deger.Location = new System.Drawing.Point(587, 63);
		this.checkbox1_isaretsiz_icin_deger.Name = "checkbox1_isaretsiz_icin_deger";
		this.checkbox1_isaretsiz_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox1_isaretsiz_icin_deger.TabIndex = 224;
		this.label179.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label179.Location = new System.Drawing.Point(587, 40);
		this.label179.Name = "label179";
		this.label179.Size = new System.Drawing.Size(124, 19);
		this.label179.TabIndex = 223;
		this.label179.Text = "İşaretsiz için değer";
		this.label179.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox1_isaretli_icin_deger.Location = new System.Drawing.Point(457, 63);
		this.checkbox1_isaretli_icin_deger.Name = "checkbox1_isaretli_icin_deger";
		this.checkbox1_isaretli_icin_deger.Size = new System.Drawing.Size(124, 20);
		this.checkbox1_isaretli_icin_deger.TabIndex = 222;
		this.label178.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label178.Location = new System.Drawing.Point(457, 40);
		this.label178.Name = "label178";
		this.label178.Size = new System.Drawing.Size(124, 19);
		this.label178.TabIndex = 221;
		this.label178.Text = "İşaretli için değer";
		this.label178.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkbox1_varsayilan_deger.Location = new System.Drawing.Point(346, 63);
		this.checkbox1_varsayilan_deger.Name = "checkbox1_varsayilan_deger";
		this.checkbox1_varsayilan_deger.Properties.Caption = "Varsayılan değer";
		this.checkbox1_varsayilan_deger.Size = new System.Drawing.Size(105, 19);
		this.checkbox1_varsayilan_deger.TabIndex = 220;
		this.checkbox1_uzunluk.EditValue = new decimal(new int[4]);
		this.checkbox1_uzunluk.Location = new System.Drawing.Point(286, 63);
		this.checkbox1_uzunluk.Name = "checkbox1_uzunluk";
		this.checkbox1_uzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox1_uzunluk.Properties.IsFloatValue = false;
		this.checkbox1_uzunluk.Properties.Mask.EditMask = "N00";
		this.checkbox1_uzunluk.Size = new System.Drawing.Size(54, 20);
		this.checkbox1_uzunluk.TabIndex = 219;
		this.checkbox1_baslangic.EditValue = new decimal(new int[4]);
		this.checkbox1_baslangic.Location = new System.Drawing.Point(181, 63);
		this.checkbox1_baslangic.Name = "checkbox1_baslangic";
		this.checkbox1_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.checkbox1_baslangic.Properties.IsFloatValue = false;
		this.checkbox1_baslangic.Properties.Mask.EditMask = "N00";
		this.checkbox1_baslangic.Size = new System.Drawing.Size(96, 20);
		this.checkbox1_baslangic.TabIndex = 218;
		this.checkbox1_gorunen_adi.Location = new System.Drawing.Point(12, 63);
		this.checkbox1_gorunen_adi.Name = "checkbox1_gorunen_adi";
		this.checkbox1_gorunen_adi.Size = new System.Drawing.Size(163, 20);
		this.checkbox1_gorunen_adi.TabIndex = 217;
		this.label174.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label174.Location = new System.Drawing.Point(12, 40);
		this.label174.Name = "label174";
		this.label174.Size = new System.Drawing.Size(163, 19);
		this.label174.TabIndex = 216;
		this.label174.Text = "Başlık";
		this.label174.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label175.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label175.Location = new System.Drawing.Point(9, 14);
		this.label175.Name = "label175";
		this.label175.Size = new System.Drawing.Size(135, 19);
		this.label175.TabIndex = 215;
		this.label175.Text = "Onay kutusu 1 :";
		this.label175.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label176.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label176.Location = new System.Drawing.Point(283, 40);
		this.label176.Name = "label176";
		this.label176.Size = new System.Drawing.Size(54, 19);
		this.label176.TabIndex = 214;
		this.label176.Text = "Uzunluk";
		this.label176.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label177.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label177.Location = new System.Drawing.Point(181, 40);
		this.label177.Name = "label177";
		this.label177.Size = new System.Drawing.Size(96, 19);
		this.label177.TabIndex = 213;
		this.label177.Text = "Başlangıç/Sıra";
		this.label177.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.te_eklenecek_parametre_adi.Location = new System.Drawing.Point(13, 352);
		this.te_eklenecek_parametre_adi.Name = "te_eklenecek_parametre_adi";
		this.te_eklenecek_parametre_adi.Size = new System.Drawing.Size(153, 20);
		this.te_eklenecek_parametre_adi.TabIndex = 89;
		this.labelControl17.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl17.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl17.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl17.Location = new System.Drawing.Point(13, 327);
		this.labelControl17.Name = "labelControl17";
		this.labelControl17.Size = new System.Drawing.Size(153, 19);
		this.labelControl17.TabIndex = 89;
		this.labelControl17.Text = "Eklenecek parametre adı";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(934, 562);
		base.Controls.Add(this.labelControl17);
		base.Controls.Add(this.te_eklenecek_parametre_adi);
		base.Controls.Add(this.tc_parametreler);
		base.Controls.Add(this.sb_ayarlari_kaydet);
		base.Controls.Add(this.sb_kullanici_sil);
		base.Controls.Add(this.sb_kullanici_ekle);
		base.Controls.Add(this.lb_kullanicilar);
		base.Name = "Aktarim_Banka_Aktarim_Parametre_Duzenleme";
		this.Text = "Banka Aktarımı Genel Parametre Düzenleme";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).EndInit();
		this.tc_parametreler.ResumeLayout(false);
		this.xtraTabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.te_bakiye_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_bakiye.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_mahalle_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_mahalle.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_unvan2_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_unvan2.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_bankahesapno_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tckimlikvergino_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_eposta_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_telefon_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_postakodu_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_ulke_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_il_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_ilce_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_adres_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_unvan_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_bankahesapno.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tckimlikvergino.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_eposta.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_telefon.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_postakodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_ulke.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_il.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_ilce.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_adres.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_unvan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihgun_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihgun.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihay_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihay.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tutar_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_aciklama_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihyil_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tutar.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_aciklama.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tarihyil.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ce_ayrackarakterikullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_ayrackarakteri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ce_bilgilerinbaslangicsatirikullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_bilgilerinbaslangicsatiri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ce_alinacaksatirlarinbaslangickarakterikullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_alinacaksatirlarinbaslangickarakteri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		this.xtraTabPage1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl1).EndInit();
		this.xtraTabControl1.ResumeLayout(false);
		this.xtraTabPage5.ResumeLayout(false);
		this.xtraTabPage5.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.mikro_disi_ek_bilgileri_kullan.Properties).EndInit();
		this.xtraTabPage6.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.metin50_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin50_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin50_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin50_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin50_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin49_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin49_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin49_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin49_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin49_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin48_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin48_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin48_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin48_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin48_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin47_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin47_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin47_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin47_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin47_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin46_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin46_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin46_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin46_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin46_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin45_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin45_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin45_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin45_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin45_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin44_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin44_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin44_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin44_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin44_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin43_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin43_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin43_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin43_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin43_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin42_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin42_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin42_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin42_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin42_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin41_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin41_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin41_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin41_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin41_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin40_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin40_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin40_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin40_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin40_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin39_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin39_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin39_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin39_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin39_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin38_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin38_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin38_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin38_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin38_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin37_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin37_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin37_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin37_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin37_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin36_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin36_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin36_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin36_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin36_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin35_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin35_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin35_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin35_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin35_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin34_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin34_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin34_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin34_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin34_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin33_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin33_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin33_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin33_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin33_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin32_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin32_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin32_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin32_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin32_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin31_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin31_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin31_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin31_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin31_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin30_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin30_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin30_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin30_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin30_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin29_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin29_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin29_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin29_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin29_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin28_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin28_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin28_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin28_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin28_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin27_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin27_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin27_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin27_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin27_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin26_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin26_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin26_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin26_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin26_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin25_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin25_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin25_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin25_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin25_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin24_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin24_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin24_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin24_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin24_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin23_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin23_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin23_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin23_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin23_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin22_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin22_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin22_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin22_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin22_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin21_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin21_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin21_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin21_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin21_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin20_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin20_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin20_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin20_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin20_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin19_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin19_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin19_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin19_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin19_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin18_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin18_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin18_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin18_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin18_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin17_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin17_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin17_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin17_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin17_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin16_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin16_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin16_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin16_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin16_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin15_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin15_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin15_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin15_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin15_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin14_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin14_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin14_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin14_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin14_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin13_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin13_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin13_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin13_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin13_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin12_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin12_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin12_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin12_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin12_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin11_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin11_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin11_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin11_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin11_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin10_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin10_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin10_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin10_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin10_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin9_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin9_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin9_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin9_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin9_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin8_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin8_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin8_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin8_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin8_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin7_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin7_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin7_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin7_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin7_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin6_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin6_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin6_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin6_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin6_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin5_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin5_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin5_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin5_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin5_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin4_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin4_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin4_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin4_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin4_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin3_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin3_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin3_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin3_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin3_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin2_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin2_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin2_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin2_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin1_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin1_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin1_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin1_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.metin1_gorunen_adi.Properties).EndInit();
		this.xtraTabPage4.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dropbox10_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_secenekler_veri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_secenekler_yazi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox10_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_secenekler_veri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_secenekler_yazi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox9_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_secenekler_veri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_secenekler_yazi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox8_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_secenekler_veri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_secenekler_yazi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox7_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_secenekler_veri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_secenekler_yazi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox6_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_secenekler_veri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_secenekler_yazi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox5_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_secenekler_veri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_secenekler_yazi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox4_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_secenekler_veri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_secenekler_yazi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox3_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_secenekler_veri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_secenekler_yazi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox2_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_secenekler_veri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_secenekler_yazi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_zorunlu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dropbox1_gorunen_adi.Properties).EndInit();
		this.xtraTabPage2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.checkbox10_isaretsiz_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_isaretli_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox10_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_isaretsiz_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_isaretli_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox9_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_isaretsiz_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_isaretli_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox8_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_isaretsiz_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_isaretli_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox7_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_isaretsiz_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_isaretli_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox6_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_isaretsiz_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_isaretli_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox5_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_isaretsiz_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_isaretli_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox4_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_isaretsiz_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_isaretli_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox3_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_isaretsiz_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_isaretli_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox2_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_isaretsiz_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_isaretli_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_varsayilan_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_uzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkbox1_gorunen_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_eklenecek_parametre_adi.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
