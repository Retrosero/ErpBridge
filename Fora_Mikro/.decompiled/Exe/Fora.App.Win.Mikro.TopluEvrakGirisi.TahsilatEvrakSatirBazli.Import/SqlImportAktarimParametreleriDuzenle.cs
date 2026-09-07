using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using Fora.App.Win.Mikro.GenelFormlar;
using Fora.App.Win.Mikro.TopluEvrakGirisi.TahsilatEvrakSatirBazli.Kriter;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.TahsilatEvrakSatirBazli.Import;

public class SqlImportAktarimParametreleriDuzenle : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _aktarimparametreleri;

	private bool DegisiklikVar;

	private string AktifKullanici;

	private IContainer components;

	private ListBoxControl lb_kullanicilar;

	private TextEdit te_kullanici_adi;

	private LabelControl labelControl1;

	private XtraTabControl tc_ayarlar;

	private XtraTabPage xtraTabPage1;

	private XtraTabPage xtraTabPage3;

	private SpinEdit evrak_tarihi_baslangic;

	private Label label29;

	private SpinEdit evrak_sira_baslangic;

	private Label label6;

	private XtraTabPage xtraTabPage5;

	private XtraTabPage xtraTabPage8;

	private XtraTabPage xtraTabPage9;

	private XtraTabPage xtraTabPage10;

	private TextEdit cari_kod_sabit_deger;

	private Label label91;

	private SpinEdit cari_kod_baslangic;

	private Label label92;

	private CheckEdit cari_kod_sabit_kullan;

	private Label label93;

	private Label label94;

	private TextEdit plasiyer_kodu_sabit_deger;

	private Label label123;

	private SpinEdit plasiyer_kodu_baslangic;

	private Label label124;

	private CheckEdit plasiyer_kodu_sabit_kullan;

	private Label label125;

	private Label label126;

	private TextEdit sor_mer_kodu_sabit_deger;

	private Label label118;

	private SpinEdit sor_mer_kodu_baslangic;

	private Label label119;

	private CheckEdit sor_mer_kodu_sabit_kullan;

	private Label label120;

	private Label label121;

	private TextEdit proje_kodu_sabit_deger;

	private Label label113;

	private SpinEdit proje_kodu_baslangic;

	private Label label114;

	private CheckEdit proje_kodu_sabit_kullan;

	private Label label115;

	private Label label116;

	private CheckEdit plasiyer_kodu_cariden_kullan;

	private TextEdit aciklama10_sabit_deger;

	private Label label165;

	private SpinEdit aciklama10_baslangic;

	private Label label166;

	private CheckEdit aciklama10_sabit_kullan;

	private Label label167;

	private Label label168;

	private TextEdit aciklama9_sabit_deger;

	private Label label160;

	private SpinEdit aciklama9_baslangic;

	private Label label161;

	private CheckEdit aciklama9_sabit_kullan;

	private Label label162;

	private Label label163;

	private TextEdit aciklama8_sabit_deger;

	private Label label155;

	private SpinEdit aciklama8_baslangic;

	private Label label156;

	private CheckEdit aciklama8_sabit_kullan;

	private Label label157;

	private Label label158;

	private TextEdit aciklama7_sabit_deger;

	private Label label150;

	private SpinEdit aciklama7_baslangic;

	private Label label151;

	private CheckEdit aciklama7_sabit_kullan;

	private Label label152;

	private Label label153;

	private TextEdit aciklama6_sabit_deger;

	private Label label145;

	private SpinEdit aciklama6_baslangic;

	private Label label146;

	private CheckEdit aciklama6_sabit_kullan;

	private Label label147;

	private Label label148;

	private TextEdit aciklama5_sabit_deger;

	private Label label140;

	private SpinEdit aciklama5_baslangic;

	private Label label141;

	private CheckEdit aciklama5_sabit_kullan;

	private Label label142;

	private Label label143;

	private TextEdit aciklama4_sabit_deger;

	private Label label135;

	private SpinEdit aciklama4_baslangic;

	private Label label136;

	private CheckEdit aciklama4_sabit_kullan;

	private Label label137;

	private Label label138;

	private TextEdit aciklama3_sabit_deger;

	private Label label130;

	private SpinEdit aciklama3_baslangic;

	private Label label131;

	private CheckEdit aciklama3_sabit_kullan;

	private Label label132;

	private Label label133;

	private TextEdit aciklama2_sabit_deger;

	private Label label36;

	private SpinEdit aciklama2_baslangic;

	private Label label37;

	private CheckEdit aciklama2_sabit_kullan;

	private Label label38;

	private Label label39;

	private TextEdit aciklama1_sabit_deger;

	private Label label27;

	private SpinEdit aciklama1_baslangic;

	private Label label28;

	private CheckEdit aciklama1_sabit_kullan;

	private Label label31;

	private Label label32;

	private TextEdit ozel_alan_3_sabit_deger;

	private Label label180;

	private SpinEdit ozel_alan_3_baslangic;

	private Label label181;

	private CheckEdit ozel_alan_3_sabit_kullan;

	private Label label182;

	private Label label183;

	private TextEdit ozel_alan_2_sabit_deger;

	private Label label175;

	private SpinEdit ozel_alan_2_baslangic;

	private Label label176;

	private CheckEdit ozel_alan_2_sabit_kullan;

	private Label label177;

	private Label label178;

	private TextEdit ozel_alan_1_sabit_deger;

	private Label label170;

	private SpinEdit ozel_alan_1_baslangic;

	private Label label171;

	private CheckEdit ozel_alan_1_sabit_kullan;

	private Label label172;

	private Label label173;

	private Label label207;

	private Label label205;

	private TextEdit evrak_seri_sabit_deger;

	private Label label197;

	private SpinEdit evrak_seri_baslangic;

	private Label label198;

	private CheckEdit evrak_seri_sabit_kullan;

	private Label label199;

	private Label label45;

	private Label label41;

	private Label label1;

	private TextEdit cari_kodu_on_ek_satis;

	private CheckEdit cari_kodu_on_ek_kullan;

	private Label label212;

	private LabelControl labelControl3;

	private LabelControl labelControl2;

	private TextEdit cari_arama_secenekleri;

	private Label label214;

	private Label label213;

	private TextEdit aciklama10_on_ek_deger;

	private CheckEdit aciklama10_on_ek_kullan;

	private TextEdit aciklama9_on_ek_deger;

	private CheckEdit aciklama9_on_ek_kullan;

	private TextEdit aciklama8_on_ek_deger;

	private CheckEdit aciklama8_on_ek_kullan;

	private TextEdit aciklama7_on_ek_deger;

	private CheckEdit aciklama7_on_ek_kullan;

	private TextEdit aciklama6_on_ek_deger;

	private CheckEdit aciklama6_on_ek_kullan;

	private TextEdit aciklama5_on_ek_deger;

	private CheckEdit aciklama5_on_ek_kullan;

	private TextEdit aciklama4_on_ek_deger;

	private CheckEdit aciklama4_on_ek_kullan;

	private TextEdit aciklama3_on_ek_deger;

	private CheckEdit aciklama3_on_ek_kullan;

	private TextEdit aciklama2_on_ek_deger;

	private CheckEdit aciklama2_on_ek_kullan;

	private TextEdit aciklama1_on_ek_deger;

	private CheckEdit aciklama1_on_ek_kullan;

	private XtraTabControl xtraTabControl2;

	private XtraTabPage xtraTabPage16;

	private LabelControl labelControl4;

	private Label label291;

	private Label label289;

	private SpinEdit cari_tc_kimlik_no_baslangic;

	private Label label220;

	private SpinEdit cari_vergi_no_baslangic;

	private Label label219;

	private XtraTabPage xtraTabPage20;

	private XtraTabControl xtraTabControl4;

	private XtraTabPage xtraTabPage21;

	private Label label303;

	private TextEdit satir_aciklama_on_ek_deger;

	private CheckEdit satir_aciklama_on_ek_kullan;

	private TextEdit satir_aciklama_sabit_deger;

	private Label label19;

	private SpinEdit satir_aciklama_baslangic;

	private Label label127;

	private CheckEdit satir_aciklama_sabit_kullan;

	private Label label128;

	private Label label129;

	private Label label25;

	private Label label83;

	private TextEdit satir_cinsi_veri_cek;

	private Label label84;

	private TextEdit satir_cinsi_veri_nakit;

	private Label label86;

	private SpinEdit satir_cinsi_baslangic;

	private Label label87;

	private CheckEdit satir_cinsi_sabit_kullan;

	private Label label88;

	private System.Windows.Forms.ComboBox satir_cinsi_sabit_deger;

	private Label label81;

	private LabelControl labelControl12;

	private Label label422;

	private SpinEdit cari_kod2_baslangic;

	private Label label209;

	private SpinEdit belge_no_baslangic;

	private Label label7;

	private Label label62;

	private SpinEdit belge_tarihi_baslangic;

	private Label label11;

	private Label label474;

	private Label label476;

	private Label label95;

	private SpinEdit kur_baslangic;

	private Label label12;

	private Label label473;

	private XtraTabPage xtraTabPage4;

	private Label label505;

	private SpinEdit kriter_metin1_baslangic;

	private Label label506;

	private Label label524;

	private SpinEdit kriter_metin5_baslangic;

	private Label label523;

	private SpinEdit kriter_metin4_baslangic;

	private Label label515;

	private SpinEdit kriter_metin3_baslangic;

	private Label label508;

	private SpinEdit kriter_metin2_baslangic;

	private Label label507;

	private TextEdit kriter_bool5_evet_icin_deger;

	private TextEdit kriter_bool4_evet_icin_deger;

	private TextEdit kriter_bool3_evet_icin_deger;

	private TextEdit kriter_bool2_evet_icin_deger;

	private SpinEdit kriter_bool5_baslangic;

	private Label label539;

	private SpinEdit kriter_bool4_baslangic;

	private Label label540;

	private SpinEdit kriter_bool3_baslangic;

	private Label label541;

	private SpinEdit kriter_bool2_baslangic;

	private Label label542;

	private Label label544;

	private SpinEdit kriter_bool1_baslangic;

	private Label label545;

	private Label label533;

	private TextEdit kriter_bool1_evet_icin_deger;

	private Label label525;

	private SpinEdit kriter_double5_baslangic;

	private Label label526;

	private SpinEdit kriter_double4_baslangic;

	private Label label527;

	private SpinEdit kriter_double3_baslangic;

	private Label label528;

	private SpinEdit kriter_double2_baslangic;

	private Label label529;

	private Label label531;

	private SpinEdit kriter_double1_baslangic;

	private Label label532;

	private XtraTabPage xtraTabPage6;

	private Label label264;

	private SpinEdit sube_no;

	private SpinEdit firma_no;

	private Label label211;

	private Label label210;

	private TextEdit SqlServer;

	private TextEdit SqlServerPort;

	private CheckedListBoxControl KriterListesi;

	private Label label536;

	private Label label537;

	private Label label546;

	private Label label548;

	private SpinEdit cari_banka_hesap_no_baslangic;

	private Label label233;

	private CheckEdit cari_unvan_turkce_karakterleri_kaldir;

	private SpinEdit cari_unvan2_baslangic;

	private Label label223;

	private Label label217;

	private SpinEdit cari_unvan_baslangic;

	private Label label218;

	private Label label215;

	private SpinEdit cari_eposta_baslangic;

	private Label label232;

	private XtraTabPage xtraTabPage35;

	private XtraTabControl xtraTabControl8;

	private XtraTabPage xtraTabPage36;

	private XtraTabControl xtraTabControl9;

	private XtraTabPage xtraTabPage27;

	private Label label549;

	private SpinEdit cari_vergi_dairesi_baslangic;

	private Label label221;

	private XtraTabPage xtraTabPage31;

	private XtraTabPage xtraTabPage39;

	private Label label292;

	private CheckEdit cari_il_bilgisi_plaka_kodu;

	private SpinEdit cari_telefon_baslangic;

	private Label label231;

	private SpinEdit cari_posta_kodu_baslangic;

	private Label label230;

	private SpinEdit cari_ulke_baslangic;

	private Label label229;

	private Label label228;

	private SpinEdit cari_il_baslangic;

	private Label label226;

	private SpinEdit cari_ilce_baslangic;

	private Label label225;

	private SpinEdit cari_mahalle_baslangic;

	private Label label224;

	private SpinEdit cari_adres_baslangic;

	private Label label222;

	private XtraTabPage xtraTabPage17;

	private XtraTabPage xtraTabPage40;

	private CheckEdit kayit_id_otomatik_ver;

	private Label label554;

	private SpinEdit kayit_id_baslangic;

	private Label label555;

	private Label label550;

	private CheckEdit evrak_sira_otomatik_ver;

	private Label label551;

	private System.Windows.Forms.ComboBox otomatik_hesap_acma_secenek_cari;

	private Label label557;

	private System.Windows.Forms.ComboBox otomatik_hesap_acma_secenek_proje;

	private Label label558;

	private System.Windows.Forms.ComboBox otomatik_hesap_acma_secenek_sorumluluk;

	private TextEdit pro_adi_sabit_deger;

	private Label label560;

	private SpinEdit pro_adi_baslangic;

	private Label label561;

	private CheckEdit pro_adi_sabit_kullan;

	private Label label562;

	private Label label563;

	private TextEdit pro_muh_kod_artikeli_sabit_deger;

	private Label label600;

	private SpinEdit pro_muh_kod_artikeli_baslangic;

	private Label label601;

	private CheckEdit pro_muh_kod_artikeli_sabit_kullan;

	private Label label602;

	private Label label603;

	private TextEdit pro_aciklama_sabit_deger;

	private Label label595;

	private SpinEdit pro_aciklama_baslangic;

	private Label label596;

	private CheckEdit pro_aciklama_sabit_kullan;

	private Label label597;

	private Label label598;

	private TextEdit pro_ana_projekodu_sabit_deger;

	private Label label590;

	private SpinEdit pro_ana_projekodu_baslangic;

	private Label label591;

	private CheckEdit pro_ana_projekodu_sabit_kullan;

	private Label label592;

	private Label label593;

	private TextEdit pro_bolgekodu_sabit_deger;

	private Label label585;

	private SpinEdit pro_bolgekodu_baslangic;

	private Label label586;

	private CheckEdit pro_bolgekodu_sabit_kullan;

	private Label label587;

	private Label label588;

	private TextEdit pro_sektorkodu_sabit_deger;

	private Label label580;

	private SpinEdit pro_sektorkodu_baslangic;

	private Label label581;

	private CheckEdit pro_sektorkodu_sabit_kullan;

	private Label label582;

	private Label label583;

	private TextEdit pro_grupkodu_sabit_deger;

	private Label label575;

	private SpinEdit pro_grupkodu_baslangic;

	private Label label576;

	private CheckEdit pro_grupkodu_sabit_kullan;

	private Label label577;

	private Label label578;

	private TextEdit pro_sormerkodu_sabit_deger;

	private Label label570;

	private SpinEdit pro_sormerkodu_baslangic;

	private Label label571;

	private CheckEdit pro_sormerkodu_sabit_kullan;

	private Label label572;

	private Label label573;

	private TextEdit pro_musterikodu_sabit_deger;

	private Label label565;

	private SpinEdit pro_musterikodu_baslangic;

	private Label label566;

	private CheckEdit pro_musterikodu_sabit_kullan;

	private Label label567;

	private Label label568;

	private TextEdit som_MuhArtikeli_sabit_deger;

	private Label label605;

	private SpinEdit som_MuhArtikeli_baslangic;

	private Label label606;

	private CheckEdit som_MuhArtikeli_sabit_kullan;

	private Label label607;

	private Label label608;

	private TextEdit som_isim_sabit_deger;

	private Label label610;

	private SpinEdit som_isim_baslangic;

	private Label label611;

	private CheckEdit som_isim_sabit_kullan;

	private Label label612;

	private Label label613;

	private XtraTabPage xtraTabPage41;

	private Label label639;

	private SpinEdit cari_muh_kod2_satis_baslangic;

	private Label label640;

	private CheckEdit cari_muh_kod2_satis_sabit_kullan;

	private Label label641;

	private Label label642;

	private Label label628;

	private SpinEdit cari_muh_kod1_satis_baslangic;

	private Label label629;

	private CheckEdit cari_muh_kod1_satis_sabit_kullan;

	private Label label630;

	private Label label631;

	private Label label614;

	private SpinEdit cari_muh_kod_satis_baslangic;

	private Label label615;

	private CheckEdit cari_muh_kod_satis_sabit_kullan;

	private Label label616;

	private TextEdit cari_muhartikeli;

	private TextEdit cari_muh_kod2_satis;

	private System.Windows.Forms.ComboBox cari_doviz_cinsi2;

	private TextEdit cari_muh_kod1_satis;

	private System.Windows.Forms.ComboBox cari_doviz_cinsi1;

	private TextEdit cari_muh_kod_satis;

	private Label label411;

	private System.Windows.Forms.ComboBox cari_doviz_cinsi;

	private Label label643;

	private Label label645;

	private SpinEdit cari_muhartikeli_baslangic;

	private Label label646;

	private CheckEdit cari_muhartikeli_sabit_kullan;

	private Label label647;

	private Label label694;

	private CheckEdit cari_Portal_Enabled;

	private TextEdit cari_satis_isk_kod_sabit_deger;

	private Label label670;

	private SpinEdit cari_satis_isk_kod_baslangic;

	private Label label671;

	private CheckEdit cari_satis_isk_kod_sabit_kullan;

	private Label label672;

	private Label label673;

	private TextEdit cari_sicil_no_sabit_deger;

	private Label label665;

	private SpinEdit cari_sicil_no_baslangic;

	private Label label666;

	private CheckEdit cari_sicil_no_sabit_kullan;

	private Label label667;

	private Label label668;

	private TextEdit cari_CepTel_sabit_deger;

	private Label label660;

	private SpinEdit cari_CepTel_baslangic;

	private Label label661;

	private CheckEdit cari_CepTel_sabit_kullan;

	private Label label662;

	private Label label663;

	private TextEdit cari_wwwadresi_sabit_deger;

	private Label label655;

	private SpinEdit cari_wwwadresi_baslangic;

	private Label label656;

	private CheckEdit cari_wwwadresi_sabit_kullan;

	private Label label657;

	private Label label658;

	private TextEdit cari_Ana_cari_kodu_sabit_deger;

	private Label label650;

	private SpinEdit cari_Ana_cari_kodu_baslangic;

	private Label label651;

	private CheckEdit cari_Ana_cari_kodu_sabit_kullan;

	private Label label652;

	private Label label653;

	private TextEdit cari_temsilci_kodu_sabit_deger;

	private Label label414;

	private SpinEdit cari_temsilci_kodu_baslangic;

	private Label label415;

	private CheckEdit cari_temsilci_kodu_sabit_kullan;

	private Label label416;

	private Label label648;

	private TextEdit cari_VarsayilanCikisDepo_sabit_deger;

	private Label label696;

	private SpinEdit cari_VarsayilanCikisDepo_baslangic;

	private Label label697;

	private CheckEdit cari_VarsayilanCikisDepo_sabit_kullan;

	private Label label698;

	private Label label699;

	private TextEdit cari_VarsayilanGirisDepo_sabit_deger;

	private Label label701;

	private SpinEdit cari_VarsayilanGirisDepo_baslangic;

	private Label label702;

	private CheckEdit cari_VarsayilanGirisDepo_sabit_kullan;

	private Label label703;

	private Label label704;

	private TextEdit cari_Portal_PW_sabit_deger;

	private Label label685;

	private SpinEdit cari_Portal_PW_baslangic;

	private Label label686;

	private CheckEdit cari_Portal_PW_sabit_kullan;

	private Label label687;

	private Label label688;

	private TextEdit cari_bolge_kodu_sabit_deger;

	private Label label690;

	private SpinEdit cari_bolge_kodu_baslangic;

	private Label label691;

	private CheckEdit cari_bolge_kodu_sabit_kullan;

	private Label label692;

	private Label label693;

	private TextEdit cari_sektor_kodu_sabit_deger;

	private Label label675;

	private SpinEdit cari_sektor_kodu_baslangic;

	private Label label676;

	private CheckEdit cari_sektor_kodu_sabit_kullan;

	private Label label677;

	private Label label678;

	private TextEdit cari_grup_kodu_sabit_deger;

	private Label label680;

	private SpinEdit cari_grup_kodu_baslangic;

	private Label label681;

	private CheckEdit cari_grup_kodu_sabit_kullan;

	private Label label682;

	private Label label683;

	private Label label3;

	private Label label538;

	private Label label13;

	private Label label17;

	private TextEdit Query;

	private Label label16;

	private TextEdit DBName;

	private Label label15;

	private TextEdit SqlPassword;

	private Label label10;

	private TextEdit SqlUserName;

	private Label label9;

	private Label label2;

	private LabelControl labelControl14;

	private Label label18;

	private TextEdit satir_cinsi_veri_kredi_karti;

	private Label label14;

	private TextEdit satir_cinsi_veri_senet;

	private XtraTabPage xtraTabPage2;

	private TextEdit satir_sorumlulukmerkezi_sabit_deger;

	private Label label24;

	private SpinEdit satir_sorumlulukmerkezi_baslangic;

	private Label label26;

	private CheckEdit satir_sorumlulukmerkezi_sabit_kullan;

	private Label label30;

	private Label label33;

	private TextEdit satir_vadesi_sabit_deger;

	private Label label20;

	private SpinEdit satir_vadesi_baslangic;

	private Label label21;

	private CheckEdit satir_vadesi_sabit_kullan;

	private Label label22;

	private Label label23;

	private TextEdit satir_tutar_sabit_deger;

	private CheckEdit satir_tutar_sabit_kullan;

	private Label label5;

	private System.Windows.Forms.ComboBox satir_tutar_islem_2_islem_tipi;

	private SpinEdit satir_tutar_islem_2_baslangic;

	private System.Windows.Forms.ComboBox satir_tutar_islem_1_islem_tipi;

	private SpinEdit satir_tutar_islem_1_baslangic;

	private Label label275;

	private Label label47;

	private SpinEdit satir_tutar_baslangic;

	private Label label184;

	private Label label35;

	private TextEdit satir_cinsi_veri_giden_havale;

	private Label label34;

	private TextEdit satir_cinsi_veri_gelen_havale;

	private XtraTabPage xtraTabPage7;

	private TextEdit satir_hesap_kodu_nakit_son_ek;

	private Label label42;

	private TextEdit satir_hesap_kodu_nakit_on_ek;

	private Label label40;

	private TextEdit satir_hesap_kodu_nakit_sabit_deger;

	private CheckEdit satir_hesap_kodu_nakit_sabit_kullan;

	private Label label89;

	private Label label82;

	private Label label4;

	private SpinEdit satir_hesap_kodu_nakit_baslangic;

	private Label label8;

	private TextEdit satir_hesap_kodu_giden_havale_son_ek;

	private Label label70;

	private TextEdit satir_hesap_kodu_giden_havale_on_ek;

	private Label label71;

	private TextEdit satir_hesap_kodu_giden_havale_sabit_deger;

	private CheckEdit satir_hesap_kodu_giden_havale_sabit_kullan;

	private Label label72;

	private Label label73;

	private Label label74;

	private SpinEdit satir_hesap_kodu_giden_havale_baslangic;

	private Label label75;

	private TextEdit satir_hesap_kodu_gelen_havale_son_ek;

	private Label label64;

	private TextEdit satir_hesap_kodu_gelen_havale_on_ek;

	private Label label65;

	private TextEdit satir_hesap_kodu_gelen_havale_sabit_deger;

	private CheckEdit satir_hesap_kodu_gelen_havale_sabit_kullan;

	private Label label66;

	private Label label67;

	private Label label68;

	private SpinEdit satir_hesap_kodu_gelen_havale_baslangic;

	private Label label69;

	private TextEdit satir_hesap_kodu_kredi_karti_son_ek;

	private Label label57;

	private TextEdit satir_hesap_kodu_kredi_karti_on_ek;

	private Label label58;

	private TextEdit satir_hesap_kodu_kredi_karti_sabit_deger;

	private CheckEdit satir_hesap_kodu_kredi_karti_sabit_kullan;

	private Label label59;

	private Label label60;

	private Label label61;

	private SpinEdit satir_hesap_kodu_kredi_karti_baslangic;

	private Label label63;

	private TextEdit satir_hesap_kodu_senet_son_ek;

	private Label label51;

	private TextEdit satir_hesap_kodu_senet_on_ek;

	private Label label52;

	private TextEdit satir_hesap_kodu_senet_sabit_deger;

	private CheckEdit satir_hesap_kodu_senet_sabit_kullan;

	private Label label53;

	private Label label54;

	private Label label55;

	private SpinEdit satir_hesap_kodu_senet_baslangic;

	private Label label56;

	private TextEdit satir_hesap_kodu_cek_son_ek;

	private Label label43;

	private TextEdit satir_hesap_kodu_cek_on_ek;

	private Label label44;

	private TextEdit satir_hesap_kodu_cek_sabit_deger;

	private CheckEdit satir_hesap_kodu_cek_sabit_kullan;

	private Label label46;

	private Label label48;

	private Label label49;

	private SpinEdit satir_hesap_kodu_cek_baslangic;

	private Label label50;

	private SpinEdit dbc_no;

	private Label label76;

	private Label label718;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem degisiklikleriKaydetToolStripMenuItem;

	private ToolStripMenuItem sablonSilToolStripMenuItem;

	private ToolStripMenuItem sablonEkleToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem dosyayaYazToolStripMenuItem;

	private ToolStripMenuItem dosyadanOkuToolStripMenuItem;

	public SqlImportAktarimParametreleriDuzenle(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		DataSourceAyarla();
		KullanicilariListele();
		AktarimKriterSablonlariOlustur();
	}

	private void DataSourceAyarla()
	{
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("ID", typeof(int));
		dataTable.Columns.Add("Isim", typeof(string));
		dataTable.Rows.Add(0, "Nakit");
		dataTable.Rows.Add(1, "Müşteri seneti");
		dataTable.Rows.Add(2, "Müşteri çeki");
		dataTable.Rows.Add(3, "Müşteri kredi kartı");
		dataTable.Rows.Add(4, "Gelen havale");
		dataTable.Rows.Add(5, "Giden havale");
		satir_cinsi_sabit_deger.DataSource = dataTable;
		satir_cinsi_sabit_deger.ValueMember = "ID";
		satir_cinsi_sabit_deger.DisplayMember = "Isim";
		DataTable dataTable2 = new DataTable();
		dataTable2.Columns.Add("ID", typeof(int));
		dataTable2.Columns.Add("Isim", typeof(string));
		dataTable2.Rows.Add(0, "İşlem yok");
		dataTable2.Rows.Add(1, "Topla");
		dataTable2.Rows.Add(2, "Çıkart");
		dataTable2.Rows.Add(3, "Böl");
		dataTable2.Rows.Add(4, "Çarp");
		satir_tutar_islem_1_islem_tipi.DataSource = dataTable2;
		satir_tutar_islem_1_islem_tipi.ValueMember = "ID";
		satir_tutar_islem_1_islem_tipi.DisplayMember = "Isim";
		DataTable dataTable3 = new DataTable();
		dataTable3.Columns.Add("ID", typeof(int));
		dataTable3.Columns.Add("Isim", typeof(string));
		dataTable3.Rows.Add(0, "İşlem yok");
		dataTable3.Rows.Add(1, "Topla");
		dataTable3.Rows.Add(2, "Çıkart");
		dataTable3.Rows.Add(3, "Böl");
		dataTable3.Rows.Add(4, "Çarp");
		satir_tutar_islem_2_islem_tipi.DataSource = dataTable3;
		satir_tutar_islem_2_islem_tipi.ValueMember = "ID";
		satir_tutar_islem_2_islem_tipi.DisplayMember = "Isim";
		DataTable dataTable4 = new DataTable();
		dataTable4.Columns.Add("ID", typeof(int));
		dataTable4.Columns.Add("Isim", typeof(string));
		dataTable4.Rows.Add(0, "Türk Lirası");
		dataTable4.Rows.Add(1, "Amerikan Doları");
		dataTable4.Rows.Add(2, "Euro");
		dataTable4.Rows.Add(3, "Kanada Doları");
		dataTable4.Rows.Add(4, "Danimarka Kronu");
		dataTable4.Rows.Add(5, "İsveç Kronu");
		dataTable4.Rows.Add(6, "İsviçre Frangı");
		dataTable4.Rows.Add(7, "Norveç Kronu");
		dataTable4.Rows.Add(8, "Japon Yeni");
		dataTable4.Rows.Add(9, "Suudi Arab. Riyali");
		dataTable4.Rows.Add(10, "Kuveyt Dinarı");
		dataTable4.Rows.Add(11, "Avustralya Doları");
		dataTable4.Rows.Add(12, "İngiliz Paundu");
		dataTable4.Rows.Add(13, "İran Riyali");
		dataTable4.Rows.Add(14, "Suriye Lirası");
		dataTable4.Rows.Add(15, "Ürdün Dinarı");
		dataTable4.Rows.Add(16, "Bulgar Levası");
		dataTable4.Rows.Add(17, "Yeni Rumen Leyi");
		dataTable4.Rows.Add(18, "Yeni İsrail Şekeli");
		dataTable4.Rows.Add(19, "Manat");
		dataTable4.Rows.Add(255, "Yok (Tanımsız)");
		cari_doviz_cinsi.DataSource = dataTable4;
		cari_doviz_cinsi.ValueMember = "ID";
		cari_doviz_cinsi.DisplayMember = "Isim";
		DataTable dataTable5 = new DataTable();
		dataTable5.Columns.Add("ID", typeof(int));
		dataTable5.Columns.Add("Isim", typeof(string));
		dataTable5.Rows.Add(0, "Türk Lirası");
		dataTable5.Rows.Add(1, "Amerikan Doları");
		dataTable5.Rows.Add(2, "Euro");
		dataTable5.Rows.Add(3, "Kanada Doları");
		dataTable5.Rows.Add(4, "Danimarka Kronu");
		dataTable5.Rows.Add(5, "İsveç Kronu");
		dataTable5.Rows.Add(6, "İsviçre Frangı");
		dataTable5.Rows.Add(7, "Norveç Kronu");
		dataTable5.Rows.Add(8, "Japon Yeni");
		dataTable5.Rows.Add(9, "Suudi Arab. Riyali");
		dataTable5.Rows.Add(10, "Kuveyt Dinarı");
		dataTable5.Rows.Add(11, "Avustralya Doları");
		dataTable5.Rows.Add(12, "İngiliz Paundu");
		dataTable5.Rows.Add(13, "İran Riyali");
		dataTable5.Rows.Add(14, "Suriye Lirası");
		dataTable5.Rows.Add(15, "Ürdün Dinarı");
		dataTable5.Rows.Add(16, "Bulgar Levası");
		dataTable5.Rows.Add(17, "Yeni Rumen Leyi");
		dataTable5.Rows.Add(18, "Yeni İsrail Şekeli");
		dataTable5.Rows.Add(19, "Manat");
		dataTable5.Rows.Add(255, "Yok (Tanımsız)");
		cari_doviz_cinsi1.DataSource = dataTable5;
		cari_doviz_cinsi1.ValueMember = "ID";
		cari_doviz_cinsi1.DisplayMember = "Isim";
		DataTable dataTable6 = new DataTable();
		dataTable6.Columns.Add("ID", typeof(int));
		dataTable6.Columns.Add("Isim", typeof(string));
		dataTable6.Rows.Add(0, "Türk Lirası");
		dataTable6.Rows.Add(1, "Amerikan Doları");
		dataTable6.Rows.Add(2, "Euro");
		dataTable6.Rows.Add(3, "Kanada Doları");
		dataTable6.Rows.Add(4, "Danimarka Kronu");
		dataTable6.Rows.Add(5, "İsveç Kronu");
		dataTable6.Rows.Add(6, "İsviçre Frangı");
		dataTable6.Rows.Add(7, "Norveç Kronu");
		dataTable6.Rows.Add(8, "Japon Yeni");
		dataTable6.Rows.Add(9, "Suudi Arab. Riyali");
		dataTable6.Rows.Add(10, "Kuveyt Dinarı");
		dataTable6.Rows.Add(11, "Avustralya Doları");
		dataTable6.Rows.Add(12, "İngiliz Paundu");
		dataTable6.Rows.Add(13, "İran Riyali");
		dataTable6.Rows.Add(14, "Suriye Lirası");
		dataTable6.Rows.Add(15, "Ürdün Dinarı");
		dataTable6.Rows.Add(16, "Bulgar Levası");
		dataTable6.Rows.Add(17, "Yeni Rumen Leyi");
		dataTable6.Rows.Add(18, "Yeni İsrail Şekeli");
		dataTable6.Rows.Add(19, "Manat");
		dataTable6.Rows.Add(255, "Yok (Tanımsız)");
		cari_doviz_cinsi2.DataSource = dataTable6;
		cari_doviz_cinsi2.ValueMember = "ID";
		cari_doviz_cinsi2.DisplayMember = "Isim";
		DataTable dataTable7 = new DataTable();
		dataTable7.Columns.Add("ID", typeof(int));
		dataTable7.Columns.Add("Isim", typeof(string));
		dataTable7.Rows.Add(0, "Açma");
		dataTable7.Rows.Add(1, "Otomatik aç");
		dataTable7.Rows.Add(2, "Sor");
		otomatik_hesap_acma_secenek_cari.DataSource = dataTable7;
		otomatik_hesap_acma_secenek_cari.ValueMember = "ID";
		otomatik_hesap_acma_secenek_cari.DisplayMember = "Isim";
		DataTable dataTable8 = new DataTable();
		dataTable8.Columns.Add("ID", typeof(int));
		dataTable8.Columns.Add("Isim", typeof(string));
		dataTable8.Rows.Add(0, "Açma");
		dataTable8.Rows.Add(1, "Otomatik aç");
		dataTable8.Rows.Add(2, "Sor");
		otomatik_hesap_acma_secenek_proje.DataSource = dataTable8;
		otomatik_hesap_acma_secenek_proje.ValueMember = "ID";
		otomatik_hesap_acma_secenek_proje.DisplayMember = "Isim";
		DataTable dataTable9 = new DataTable();
		dataTable9.Columns.Add("ID", typeof(int));
		dataTable9.Columns.Add("Isim", typeof(string));
		dataTable9.Rows.Add(0, "Açma");
		dataTable9.Rows.Add(1, "Otomatik aç");
		dataTable9.Rows.Add(2, "Sor");
		otomatik_hesap_acma_secenek_sorumluluk.DataSource = dataTable9;
		otomatik_hesap_acma_secenek_sorumluluk.ValueMember = "ID";
		otomatik_hesap_acma_secenek_sorumluluk.DisplayMember = "Isim";
	}

	private void ParametreTextEdit_KeyDown(object sender, KeyEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void ParametreCheckEditMouseClick(object sender, MouseEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void GenelParametreler_Load(object sender, EventArgs e)
	{
	}

	private void KullanicilariListele()
	{
		lb_kullanicilar.Items.Clear();
		foreach (string item in SqlImportAktarimParametreleri.GetParametreAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			lb_kullanicilar.Items.Add(item);
		}
	}

	private void EkranBilgiGuncelle()
	{
		te_kullanici_adi.Text = AktifKullanici;
		KriterListesi.Text = _aktarimparametreleri._GetParametre("KriterListesi")._GetString;
		SqlServer.Text = _aktarimparametreleri._GetParametre("SqlServer")._GetString;
		SqlServerPort.Text = _aktarimparametreleri._GetParametre("SqlServerPort")._GetString;
		SqlUserName.Text = _aktarimparametreleri._GetParametre("SqlUserName")._GetString;
		SqlPassword.Text = _aktarimparametreleri._GetParametre("SqlPassword")._GetString;
		DBName.Text = _aktarimparametreleri._GetParametre("DBName")._GetString;
		Query.Text = _aktarimparametreleri._GetParametre("Query")._GetString;
		firma_no.Value = _aktarimparametreleri._GetParametre("firma_no")._GetInt;
		sube_no.Value = _aktarimparametreleri._GetParametre("sube_no")._GetInt;
		evrak_tarihi_baslangic.Value = _aktarimparametreleri._GetParametre("evrak_tarihi_baslangic")._GetInt;
		belge_no_baslangic.Value = _aktarimparametreleri._GetParametre("belge_no_baslangic")._GetInt;
		belge_tarihi_baslangic.Value = _aktarimparametreleri._GetParametre("belge_tarihi_baslangic")._GetInt;
		kur_baslangic.Value = _aktarimparametreleri._GetParametre("kur_baslangic")._GetInt;
		satir_cinsi_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_cinsi_sabit_kullan")._GetBoolean;
		satir_cinsi_sabit_deger.SelectedValue = _aktarimparametreleri._GetParametre("satir_cinsi_sabit_deger")._GetInt;
		satir_cinsi_baslangic.Value = _aktarimparametreleri._GetParametre("satir_cinsi_baslangic")._GetInt;
		satir_cinsi_veri_nakit.Text = _aktarimparametreleri._GetParametre("satir_cinsi_veri_nakit")._GetString;
		satir_cinsi_veri_cek.Text = _aktarimparametreleri._GetParametre("satir_cinsi_veri_musteri_ceki")._GetString;
		satir_cinsi_veri_senet.Text = _aktarimparametreleri._GetParametre("satir_cinsi_veri_musteri_seneti")._GetString;
		satir_cinsi_veri_kredi_karti.Text = _aktarimparametreleri._GetParametre("satir_cinsi_veri_musteri_kredi_karti")._GetString;
		satir_cinsi_veri_gelen_havale.Text = _aktarimparametreleri._GetParametre("satir_cinsi_veri_gelen_havale")._GetString;
		satir_cinsi_veri_giden_havale.Text = _aktarimparametreleri._GetParametre("satir_cinsi_veri_giden_havale")._GetString;
		satir_hesap_kodu_nakit_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_hesap_kodu_nakit_sabit_kullan")._GetBoolean;
		satir_hesap_kodu_nakit_sabit_deger.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_nakit_sabit_deger")._GetString;
		satir_hesap_kodu_nakit_baslangic.Value = _aktarimparametreleri._GetParametre("satir_hesap_kodu_nakit_baslangic")._GetInt;
		satir_hesap_kodu_nakit_on_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_nakit_on_ek")._GetString;
		satir_hesap_kodu_nakit_son_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_nakit_son_ek")._GetString;
		satir_hesap_kodu_cek_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_hesap_kodu_cek_sabit_kullan")._GetBoolean;
		satir_hesap_kodu_cek_sabit_deger.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_cek_sabit_deger")._GetString;
		satir_hesap_kodu_cek_baslangic.Value = _aktarimparametreleri._GetParametre("satir_hesap_kodu_cek_baslangic")._GetInt;
		satir_hesap_kodu_cek_on_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_cek_on_ek")._GetString;
		satir_hesap_kodu_cek_son_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_cek_son_ek")._GetString;
		satir_hesap_kodu_senet_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_hesap_kodu_senet_sabit_kullan")._GetBoolean;
		satir_hesap_kodu_senet_sabit_deger.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_senet_sabit_deger")._GetString;
		satir_hesap_kodu_senet_baslangic.Value = _aktarimparametreleri._GetParametre("satir_hesap_kodu_senet_baslangic")._GetInt;
		satir_hesap_kodu_senet_on_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_senet_on_ek")._GetString;
		satir_hesap_kodu_senet_son_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_senet_son_ek")._GetString;
		satir_hesap_kodu_kredi_karti_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_hesap_kodu_kredi_karti_sabit_kullan")._GetBoolean;
		satir_hesap_kodu_kredi_karti_sabit_deger.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_kredi_karti_sabit_deger")._GetString;
		satir_hesap_kodu_kredi_karti_baslangic.Value = _aktarimparametreleri._GetParametre("satir_hesap_kodu_kredi_karti_baslangic")._GetInt;
		satir_hesap_kodu_kredi_karti_on_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_kredi_karti_on_ek")._GetString;
		satir_hesap_kodu_kredi_karti_son_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_kredi_karti_son_ek")._GetString;
		satir_hesap_kodu_gelen_havale_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_hesap_kodu_gelen_havale_sabit_kullan")._GetBoolean;
		satir_hesap_kodu_gelen_havale_sabit_deger.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_gelen_havale_sabit_deger")._GetString;
		satir_hesap_kodu_gelen_havale_baslangic.Value = _aktarimparametreleri._GetParametre("satir_hesap_kodu_gelen_havale_baslangic")._GetInt;
		satir_hesap_kodu_gelen_havale_on_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_gelen_havale_on_ek")._GetString;
		satir_hesap_kodu_gelen_havale_son_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_gelen_havale_son_ek")._GetString;
		satir_hesap_kodu_giden_havale_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_hesap_kodu_giden_havale_sabit_kullan")._GetBoolean;
		satir_hesap_kodu_giden_havale_sabit_deger.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_giden_havale_sabit_deger")._GetString;
		satir_hesap_kodu_giden_havale_baslangic.Value = _aktarimparametreleri._GetParametre("satir_hesap_kodu_giden_havale_baslangic")._GetInt;
		satir_hesap_kodu_giden_havale_on_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_giden_havale_on_ek")._GetString;
		satir_hesap_kodu_giden_havale_son_ek.Text = _aktarimparametreleri._GetParametre("satir_hesap_kodu_giden_havale_son_ek")._GetString;
		cari_kod_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_kod_sabit_kullan")._GetBoolean;
		cari_kod_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_kod_sabit_deger")._GetString;
		cari_kod_baslangic.Value = _aktarimparametreleri._GetParametre("cari_kod_baslangic")._GetInt;
		cari_arama_secenekleri.Text = _aktarimparametreleri._GetParametre("cari_arama_secenekleri")._GetString;
		cari_unvan_turkce_karakterleri_kaldir.Checked = _aktarimparametreleri._GetParametre("cari_unvan_turkce_karakterleri_kaldir")._GetBoolean;
		cari_unvan_baslangic.Value = _aktarimparametreleri._GetParametre("cari_unvan_baslangic")._GetInt;
		cari_unvan2_baslangic.Value = _aktarimparametreleri._GetParametre("cari_unvan2_baslangic")._GetInt;
		cari_vergi_no_baslangic.Value = _aktarimparametreleri._GetParametre("cari_vergi_no_baslangic")._GetInt;
		cari_tc_kimlik_no_baslangic.Value = _aktarimparametreleri._GetParametre("cari_tc_kimlik_no_baslangic")._GetInt;
		cari_vergi_dairesi_baslangic.Value = _aktarimparametreleri._GetParametre("cari_vergi_dairesi_baslangic")._GetInt;
		cari_banka_hesap_no_baslangic.Value = _aktarimparametreleri._GetParametre("cari_banka_hesap_no_baslangic")._GetInt;
		cari_adres_baslangic.Value = _aktarimparametreleri._GetParametre("cari_adres_baslangic")._GetInt;
		cari_mahalle_baslangic.Value = _aktarimparametreleri._GetParametre("cari_mahalle_baslangic")._GetInt;
		cari_ilce_baslangic.Value = _aktarimparametreleri._GetParametre("cari_ilce_baslangic")._GetInt;
		cari_il_baslangic.Value = _aktarimparametreleri._GetParametre("cari_il_baslangic")._GetInt;
		cari_ulke_baslangic.Value = _aktarimparametreleri._GetParametre("cari_ulke_baslangic")._GetInt;
		cari_posta_kodu_baslangic.Value = _aktarimparametreleri._GetParametre("cari_posta_kodu_baslangic")._GetInt;
		cari_telefon_baslangic.Value = _aktarimparametreleri._GetParametre("cari_telefon_baslangic")._GetInt;
		cari_eposta_baslangic.Value = _aktarimparametreleri._GetParametre("cari_eposta_baslangic")._GetInt;
		proje_kodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("proje_kodu_sabit_kullan")._GetBoolean;
		proje_kodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("proje_kodu_sabit_deger")._GetString;
		proje_kodu_baslangic.Value = _aktarimparametreleri._GetParametre("proje_kodu_baslangic")._GetInt;
		sor_mer_kodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("sor_mer_kodu_sabit_kullan")._GetBoolean;
		sor_mer_kodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("sor_mer_kodu_sabit_deger")._GetString;
		sor_mer_kodu_baslangic.Value = _aktarimparametreleri._GetParametre("sor_mer_kodu_baslangic")._GetInt;
		plasiyer_kodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("plasiyer_kodu_sabit_kullan")._GetBoolean;
		plasiyer_kodu_cariden_kullan.Checked = _aktarimparametreleri._GetParametre("plasiyer_kodu_cariden_kullan")._GetBoolean;
		plasiyer_kodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("plasiyer_kodu_sabit_deger")._GetString;
		plasiyer_kodu_baslangic.Value = _aktarimparametreleri._GetParametre("plasiyer_kodu_baslangic")._GetInt;
		satir_aciklama_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_aciklama_sabit_kullan")._GetBoolean;
		satir_aciklama_sabit_deger.Text = _aktarimparametreleri._GetParametre("satir_aciklama_sabit_deger")._GetString;
		satir_aciklama_baslangic.Value = _aktarimparametreleri._GetParametre("satir_aciklama_baslangic")._GetInt;
		aciklama1_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama1_sabit_kullan")._GetBoolean;
		aciklama1_sabit_deger.Text = _aktarimparametreleri._GetParametre("aciklama1_sabit_deger")._GetString;
		aciklama1_baslangic.Value = _aktarimparametreleri._GetParametre("aciklama1_baslangic")._GetInt;
		aciklama2_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama2_sabit_kullan")._GetBoolean;
		aciklama2_sabit_deger.Text = _aktarimparametreleri._GetParametre("aciklama2_sabit_deger")._GetString;
		aciklama2_baslangic.Value = _aktarimparametreleri._GetParametre("aciklama2_baslangic")._GetInt;
		aciklama3_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama3_sabit_kullan")._GetBoolean;
		aciklama3_sabit_deger.Text = _aktarimparametreleri._GetParametre("aciklama3_sabit_deger")._GetString;
		aciklama3_baslangic.Value = _aktarimparametreleri._GetParametre("aciklama3_baslangic")._GetInt;
		aciklama4_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama4_sabit_kullan")._GetBoolean;
		aciklama4_sabit_deger.Text = _aktarimparametreleri._GetParametre("aciklama4_sabit_deger")._GetString;
		aciklama4_baslangic.Value = _aktarimparametreleri._GetParametre("aciklama4_baslangic")._GetInt;
		aciklama5_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama5_sabit_kullan")._GetBoolean;
		aciklama5_sabit_deger.Text = _aktarimparametreleri._GetParametre("aciklama5_sabit_deger")._GetString;
		aciklama5_baslangic.Value = _aktarimparametreleri._GetParametre("aciklama5_baslangic")._GetInt;
		aciklama6_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama6_sabit_kullan")._GetBoolean;
		aciklama6_sabit_deger.Text = _aktarimparametreleri._GetParametre("aciklama6_sabit_deger")._GetString;
		aciklama6_baslangic.Value = _aktarimparametreleri._GetParametre("aciklama6_baslangic")._GetInt;
		aciklama7_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama7_sabit_kullan")._GetBoolean;
		aciklama7_sabit_deger.Text = _aktarimparametreleri._GetParametre("aciklama7_sabit_deger")._GetString;
		aciklama7_baslangic.Value = _aktarimparametreleri._GetParametre("aciklama7_baslangic")._GetInt;
		aciklama8_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama8_sabit_kullan")._GetBoolean;
		aciklama8_sabit_deger.Text = _aktarimparametreleri._GetParametre("aciklama8_sabit_deger")._GetString;
		aciklama8_baslangic.Value = _aktarimparametreleri._GetParametre("aciklama8_baslangic")._GetInt;
		aciklama9_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama9_sabit_kullan")._GetBoolean;
		aciklama9_sabit_deger.Text = _aktarimparametreleri._GetParametre("aciklama9_sabit_deger")._GetString;
		aciklama9_baslangic.Value = _aktarimparametreleri._GetParametre("aciklama9_baslangic")._GetInt;
		aciklama10_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama10_sabit_kullan")._GetBoolean;
		aciklama10_sabit_deger.Text = _aktarimparametreleri._GetParametre("aciklama10_sabit_deger")._GetString;
		aciklama10_baslangic.Value = _aktarimparametreleri._GetParametre("aciklama10_baslangic")._GetInt;
		ozel_alan_1_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("ozel_alan_1_sabit_kullan")._GetBoolean;
		ozel_alan_1_sabit_deger.Text = _aktarimparametreleri._GetParametre("ozel_alan_1_sabit_deger")._GetString;
		ozel_alan_1_baslangic.Value = _aktarimparametreleri._GetParametre("ozel_alan_1_baslangic")._GetInt;
		ozel_alan_2_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("ozel_alan_2_sabit_kullan")._GetBoolean;
		ozel_alan_2_sabit_deger.Text = _aktarimparametreleri._GetParametre("ozel_alan_2_sabit_deger")._GetString;
		ozel_alan_2_baslangic.Value = _aktarimparametreleri._GetParametre("ozel_alan_2_baslangic")._GetInt;
		ozel_alan_3_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("ozel_alan_3_sabit_kullan")._GetBoolean;
		ozel_alan_3_sabit_deger.Text = _aktarimparametreleri._GetParametre("ozel_alan_3_sabit_deger")._GetString;
		ozel_alan_3_baslangic.Value = _aktarimparametreleri._GetParametre("ozel_alan_3_baslangic")._GetInt;
		evrak_seri_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("evrak_seri_sabit_kullan")._GetBoolean;
		evrak_seri_sabit_deger.Text = _aktarimparametreleri._GetParametre("evrak_seri_sabit_deger")._GetString;
		evrak_seri_baslangic.Value = _aktarimparametreleri._GetParametre("evrak_seri_baslangic")._GetInt;
		evrak_sira_baslangic.Value = _aktarimparametreleri._GetParametre("evrak_sira_baslangic")._GetInt;
		cari_kodu_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("cari_kodu_on_ek_kullan")._GetBoolean;
		cari_kodu_on_ek_satis.Text = _aktarimparametreleri._GetParametre("cari_kodu_on_ek_satis")._GetString;
		satir_aciklama_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("satir_aciklama_on_ek_kullan")._GetBoolean;
		satir_aciklama_on_ek_deger.Text = _aktarimparametreleri._GetParametre("satir_aciklama_on_ek_deger")._GetString;
		aciklama1_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama1_on_ek_kullan")._GetBoolean;
		aciklama1_on_ek_deger.Text = _aktarimparametreleri._GetParametre("aciklama1_on_ek_deger")._GetString;
		aciklama2_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama2_on_ek_kullan")._GetBoolean;
		aciklama2_on_ek_deger.Text = _aktarimparametreleri._GetParametre("aciklama2_on_ek_deger")._GetString;
		aciklama3_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama3_on_ek_kullan")._GetBoolean;
		aciklama3_on_ek_deger.Text = _aktarimparametreleri._GetParametre("aciklama3_on_ek_deger")._GetString;
		aciklama4_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama4_on_ek_kullan")._GetBoolean;
		aciklama4_on_ek_deger.Text = _aktarimparametreleri._GetParametre("aciklama4_on_ek_deger")._GetString;
		aciklama5_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama5_on_ek_kullan")._GetBoolean;
		aciklama5_on_ek_deger.Text = _aktarimparametreleri._GetParametre("aciklama5_on_ek_deger")._GetString;
		aciklama6_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama6_on_ek_kullan")._GetBoolean;
		aciklama6_on_ek_deger.Text = _aktarimparametreleri._GetParametre("aciklama6_on_ek_deger")._GetString;
		aciklama7_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama7_on_ek_kullan")._GetBoolean;
		aciklama7_on_ek_deger.Text = _aktarimparametreleri._GetParametre("aciklama7_on_ek_deger")._GetString;
		aciklama8_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama8_on_ek_kullan")._GetBoolean;
		aciklama8_on_ek_deger.Text = _aktarimparametreleri._GetParametre("aciklama8_on_ek_deger")._GetString;
		aciklama9_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama9_on_ek_kullan")._GetBoolean;
		aciklama9_on_ek_deger.Text = _aktarimparametreleri._GetParametre("aciklama9_on_ek_deger")._GetString;
		aciklama10_on_ek_kullan.Checked = _aktarimparametreleri._GetParametre("aciklama10_on_ek_kullan")._GetBoolean;
		aciklama10_on_ek_deger.Text = _aktarimparametreleri._GetParametre("aciklama10_on_ek_deger")._GetString;
		cari_il_bilgisi_plaka_kodu.Checked = _aktarimparametreleri._GetParametre("cari_il_bilgisi_plaka_kodu")._GetBoolean;
		cari_doviz_cinsi.SelectedValue = _aktarimparametreleri._GetParametre("cari_doviz_cinsi")._GetInt;
		cari_doviz_cinsi1.SelectedValue = _aktarimparametreleri._GetParametre("cari_doviz_cinsi1")._GetInt;
		cari_doviz_cinsi2.SelectedValue = _aktarimparametreleri._GetParametre("cari_doviz_cinsi2")._GetInt;
		cari_muh_kod_satis.Text = _aktarimparametreleri._GetParametre("cari_muh_kod_satis")._GetString;
		cari_muh_kod1_satis.Text = _aktarimparametreleri._GetParametre("cari_muh_kod1_satis")._GetString;
		cari_muh_kod2_satis.Text = _aktarimparametreleri._GetParametre("cari_muh_kod2_satis")._GetString;
		cari_muhartikeli.Text = _aktarimparametreleri._GetParametre("cari_muhartikeli")._GetString;
		cari_kod2_baslangic.Value = _aktarimparametreleri._GetParametre("cari_kod2_baslangic")._GetInt;
		kriter_metin1_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_metin1_baslangic")._GetInt;
		kriter_metin2_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_metin2_baslangic")._GetInt;
		kriter_metin3_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_metin3_baslangic")._GetInt;
		kriter_metin4_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_metin4_baslangic")._GetInt;
		kriter_metin5_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_metin5_baslangic")._GetInt;
		kriter_double1_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_double1_baslangic")._GetInt;
		kriter_double2_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_double2_baslangic")._GetInt;
		kriter_double3_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_double3_baslangic")._GetInt;
		kriter_double4_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_double4_baslangic")._GetInt;
		kriter_double5_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_double5_baslangic")._GetInt;
		kriter_bool1_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_bool1_baslangic")._GetInt;
		kriter_bool1_evet_icin_deger.Text = _aktarimparametreleri._GetParametre("kriter_bool1_evet_icin_deger")._GetString;
		kriter_bool2_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_bool2_baslangic")._GetInt;
		kriter_bool2_evet_icin_deger.Text = _aktarimparametreleri._GetParametre("kriter_bool2_evet_icin_deger")._GetString;
		kriter_bool3_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_bool3_baslangic")._GetInt;
		kriter_bool3_evet_icin_deger.Text = _aktarimparametreleri._GetParametre("kriter_bool3_evet_icin_deger")._GetString;
		kriter_bool4_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_bool4_baslangic")._GetInt;
		kriter_bool4_evet_icin_deger.Text = _aktarimparametreleri._GetParametre("kriter_bool4_evet_icin_deger")._GetString;
		kriter_bool5_baslangic.Value = _aktarimparametreleri._GetParametre("kriter_bool5_baslangic")._GetInt;
		kriter_bool5_evet_icin_deger.Text = _aktarimparametreleri._GetParametre("kriter_bool5_evet_icin_deger")._GetString;
		kayit_id_otomatik_ver.Checked = _aktarimparametreleri._GetParametre("kayit_id_otomatik_ver")._GetBoolean;
		kayit_id_baslangic.Value = _aktarimparametreleri._GetParametre("kayit_id_baslangic")._GetInt;
		_ = _aktarimparametreleri._GetParametre("otomatik_hesap_acma_secenek_cari")._GetInt;
		otomatik_hesap_acma_secenek_cari.SelectedValue = _aktarimparametreleri._GetParametre("otomatik_hesap_acma_secenek_cari")._GetInt;
		otomatik_hesap_acma_secenek_proje.SelectedValue = _aktarimparametreleri._GetParametre("otomatik_hesap_acma_secenek_proje")._GetInt;
		otomatik_hesap_acma_secenek_sorumluluk.SelectedValue = _aktarimparametreleri._GetParametre("otomatik_hesap_acma_secenek_sorumluluk")._GetInt;
		evrak_sira_otomatik_ver.Checked = _aktarimparametreleri._GetParametre("evrak_sira_otomatik_ver")._GetBoolean;
		pro_adi_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("pro_adi_sabit_kullan")._GetBoolean;
		pro_adi_sabit_deger.Text = _aktarimparametreleri._GetParametre("pro_adi_sabit_deger")._GetString;
		pro_adi_baslangic.Value = _aktarimparametreleri._GetParametre("pro_adi_baslangic")._GetInt;
		pro_musterikodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("pro_musterikodu_sabit_kullan")._GetBoolean;
		pro_musterikodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("pro_musterikodu_sabit_deger")._GetString;
		pro_musterikodu_baslangic.Value = _aktarimparametreleri._GetParametre("pro_musterikodu_baslangic")._GetInt;
		pro_sormerkodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("pro_sormerkodu_sabit_kullan")._GetBoolean;
		pro_sormerkodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("pro_sormerkodu_sabit_deger")._GetString;
		pro_sormerkodu_baslangic.Value = _aktarimparametreleri._GetParametre("pro_sormerkodu_baslangic")._GetInt;
		pro_grupkodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("pro_grupkodu_sabit_kullan")._GetBoolean;
		pro_grupkodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("pro_grupkodu_sabit_deger")._GetString;
		pro_grupkodu_baslangic.Value = _aktarimparametreleri._GetParametre("pro_grupkodu_baslangic")._GetInt;
		pro_sektorkodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("pro_sektorkodu_sabit_kullan")._GetBoolean;
		pro_sektorkodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("pro_sektorkodu_sabit_deger")._GetString;
		pro_sektorkodu_baslangic.Value = _aktarimparametreleri._GetParametre("pro_sektorkodu_baslangic")._GetInt;
		pro_bolgekodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("pro_bolgekodu_sabit_kullan")._GetBoolean;
		pro_bolgekodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("pro_bolgekodu_sabit_deger")._GetString;
		pro_bolgekodu_baslangic.Value = _aktarimparametreleri._GetParametre("pro_bolgekodu_baslangic")._GetInt;
		pro_ana_projekodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("pro_ana_projekodu_sabit_kullan")._GetBoolean;
		pro_ana_projekodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("pro_ana_projekodu_sabit_deger")._GetString;
		pro_ana_projekodu_baslangic.Value = _aktarimparametreleri._GetParametre("pro_ana_projekodu_baslangic")._GetInt;
		pro_aciklama_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("pro_aciklama_sabit_kullan")._GetBoolean;
		pro_aciklama_sabit_deger.Text = _aktarimparametreleri._GetParametre("pro_aciklama_sabit_deger")._GetString;
		pro_aciklama_baslangic.Value = _aktarimparametreleri._GetParametre("pro_aciklama_baslangic")._GetInt;
		pro_muh_kod_artikeli_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("pro_muh_kod_artikeli_sabit_kullan")._GetBoolean;
		pro_muh_kod_artikeli_sabit_deger.Text = _aktarimparametreleri._GetParametre("pro_muh_kod_artikeli_sabit_deger")._GetString;
		pro_muh_kod_artikeli_baslangic.Value = _aktarimparametreleri._GetParametre("pro_muh_kod_artikeli_baslangic")._GetInt;
		som_isim_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("som_isim_sabit_kullan")._GetBoolean;
		som_isim_sabit_deger.Text = _aktarimparametreleri._GetParametre("som_isim_sabit_deger")._GetString;
		som_isim_baslangic.Value = _aktarimparametreleri._GetParametre("som_isim_baslangic")._GetInt;
		som_MuhArtikeli_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("som_MuhArtikeli_sabit_kullan")._GetBoolean;
		som_MuhArtikeli_sabit_deger.Text = _aktarimparametreleri._GetParametre("som_MuhArtikeli_sabit_deger")._GetString;
		som_MuhArtikeli_baslangic.Value = _aktarimparametreleri._GetParametre("som_MuhArtikeli_baslangic")._GetInt;
		cari_muhartikeli_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_muhartikeli_sabit_kullan")._GetBoolean;
		cari_muhartikeli_baslangic.Value = _aktarimparametreleri._GetParametre("cari_muhartikeli_baslangic")._GetInt;
		cari_muh_kod_satis_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_muh_kod_satis_sabit_kullan")._GetBoolean;
		cari_muh_kod_satis_baslangic.Value = _aktarimparametreleri._GetParametre("cari_muh_kod_satis_baslangic")._GetInt;
		cari_muh_kod1_satis_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_muh_kod1_satis_sabit_kullan")._GetBoolean;
		cari_muh_kod1_satis_baslangic.Value = _aktarimparametreleri._GetParametre("cari_muh_kod1_satis_baslangic")._GetInt;
		cari_muh_kod2_satis_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_muh_kod2_satis_sabit_kullan")._GetBoolean;
		cari_muh_kod2_satis_baslangic.Value = _aktarimparametreleri._GetParametre("cari_muh_kod2_satis_baslangic")._GetInt;
		cari_Ana_cari_kodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_Ana_cari_kodu_sabit_kullan")._GetBoolean;
		cari_Ana_cari_kodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_Ana_cari_kodu_sabit_deger")._GetString;
		cari_Ana_cari_kodu_baslangic.Value = _aktarimparametreleri._GetParametre("cari_Ana_cari_kodu_baslangic")._GetInt;
		cari_temsilci_kodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_temsilci_kodu_sabit_kullan")._GetBoolean;
		cari_temsilci_kodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_temsilci_kodu_sabit_deger")._GetString;
		cari_temsilci_kodu_baslangic.Value = _aktarimparametreleri._GetParametre("cari_temsilci_kodu_baslangic")._GetInt;
		cari_grup_kodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_grup_kodu_sabit_kullan")._GetBoolean;
		cari_grup_kodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_grup_kodu_sabit_deger")._GetString;
		cari_grup_kodu_baslangic.Value = _aktarimparametreleri._GetParametre("cari_grup_kodu_baslangic")._GetInt;
		cari_sektor_kodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_sektor_kodu_sabit_kullan")._GetBoolean;
		cari_sektor_kodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_sektor_kodu_sabit_deger")._GetString;
		cari_sektor_kodu_baslangic.Value = _aktarimparametreleri._GetParametre("cari_sektor_kodu_baslangic")._GetInt;
		cari_bolge_kodu_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_bolge_kodu_sabit_kullan")._GetBoolean;
		cari_bolge_kodu_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_bolge_kodu_sabit_deger")._GetString;
		cari_bolge_kodu_baslangic.Value = _aktarimparametreleri._GetParametre("cari_bolge_kodu_baslangic")._GetInt;
		cari_wwwadresi_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_wwwadresi_sabit_kullan")._GetBoolean;
		cari_wwwadresi_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_wwwadresi_sabit_deger")._GetString;
		cari_wwwadresi_baslangic.Value = _aktarimparametreleri._GetParametre("cari_wwwadresi_baslangic")._GetInt;
		cari_CepTel_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_CepTel_sabit_kullan")._GetBoolean;
		cari_CepTel_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_CepTel_sabit_deger")._GetString;
		cari_CepTel_baslangic.Value = _aktarimparametreleri._GetParametre("cari_CepTel_baslangic")._GetInt;
		cari_satis_isk_kod_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_satis_isk_kod_sabit_kullan")._GetBoolean;
		cari_satis_isk_kod_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_satis_isk_kod_sabit_deger")._GetString;
		cari_satis_isk_kod_baslangic.Value = _aktarimparametreleri._GetParametre("cari_satis_isk_kod_baslangic")._GetInt;
		cari_sicil_no_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_sicil_no_sabit_kullan")._GetBoolean;
		cari_sicil_no_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_sicil_no_sabit_deger")._GetString;
		cari_sicil_no_baslangic.Value = _aktarimparametreleri._GetParametre("cari_sicil_no_baslangic")._GetInt;
		cari_VarsayilanGirisDepo_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_VarsayilanGirisDepo_sabit_kullan")._GetBoolean;
		cari_VarsayilanGirisDepo_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_VarsayilanGirisDepo_sabit_deger")._GetString;
		cari_VarsayilanGirisDepo_baslangic.Value = _aktarimparametreleri._GetParametre("cari_VarsayilanGirisDepo_baslangic")._GetInt;
		cari_VarsayilanCikisDepo_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_VarsayilanCikisDepo_sabit_kullan")._GetBoolean;
		cari_VarsayilanCikisDepo_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_VarsayilanCikisDepo_sabit_deger")._GetString;
		cari_VarsayilanCikisDepo_baslangic.Value = _aktarimparametreleri._GetParametre("cari_VarsayilanCikisDepo_baslangic")._GetInt;
		cari_Portal_PW_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("cari_Portal_PW_sabit_kullan")._GetBoolean;
		cari_Portal_PW_sabit_deger.Text = _aktarimparametreleri._GetParametre("cari_Portal_PW_sabit_deger")._GetString;
		cari_Portal_PW_baslangic.Value = _aktarimparametreleri._GetParametre("cari_Portal_PW_baslangic")._GetInt;
		cari_Portal_Enabled.Checked = _aktarimparametreleri._GetParametre("cari_Portal_Enabled")._GetBoolean;
		som_isim_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("som_isim_sabit_kullan")._GetBoolean;
		som_isim_sabit_deger.Text = _aktarimparametreleri._GetParametre("som_isim_sabit_deger")._GetString;
		som_isim_baslangic.Value = _aktarimparametreleri._GetParametre("som_isim_baslangic")._GetInt;
		som_MuhArtikeli_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("som_MuhArtikeli_sabit_kullan")._GetBoolean;
		som_MuhArtikeli_sabit_deger.Text = _aktarimparametreleri._GetParametre("som_MuhArtikeli_sabit_deger")._GetString;
		som_MuhArtikeli_baslangic.Value = _aktarimparametreleri._GetParametre("som_MuhArtikeli_baslangic")._GetInt;
		satir_tutar_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_tutar_sabit_kullan")._GetBoolean;
		satir_tutar_sabit_deger.Text = _aktarimparametreleri._GetParametre("satir_tutar_sabit_deger")._GetString;
		satir_tutar_baslangic.Value = _aktarimparametreleri._GetParametre("satir_tutar_baslangic")._GetInt;
		satir_tutar_islem_1_islem_tipi.SelectedValue = _aktarimparametreleri._GetParametre("satir_tutar_islem_1_islem_tipi")._GetInt;
		satir_tutar_islem_1_baslangic.Value = _aktarimparametreleri._GetParametre("satir_tutar_islem_1_baslangic")._GetInt;
		satir_tutar_islem_2_islem_tipi.SelectedValue = _aktarimparametreleri._GetParametre("satir_tutar_islem_2_islem_tipi")._GetInt;
		satir_tutar_islem_2_baslangic.Value = _aktarimparametreleri._GetParametre("satir_tutar_islem_2_baslangic")._GetInt;
		satir_vadesi_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_vadesi_sabit_kullan")._GetBoolean;
		satir_vadesi_sabit_deger.Text = _aktarimparametreleri._GetParametre("satir_vadesi_sabit_deger")._GetString;
		satir_vadesi_baslangic.Value = _aktarimparametreleri._GetParametre("satir_vadesi_baslangic")._GetInt;
		satir_sorumlulukmerkezi_sabit_kullan.Checked = _aktarimparametreleri._GetParametre("satir_sorumlulukmerkezi_sabit_kullan")._GetBoolean;
		satir_sorumlulukmerkezi_sabit_deger.Text = _aktarimparametreleri._GetParametre("satir_sorumlulukmerkezi_sabit_deger")._GetString;
		satir_sorumlulukmerkezi_baslangic.Value = _aktarimparametreleri._GetParametre("satir_sorumlulukmerkezi_baslangic")._GetInt;
		dbc_no.Value = _aktarimparametreleri._GetParametre("dbc_no")._GetInt;
		string[] array = _aktarimparametreleri._GetParametre("KriterListesi")._GetString.Split(',');
		int num = 0;
		foreach (object item in KriterListesi.Items)
		{
			string text = item.ToString();
			bool flag = false;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (text == text2)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				KriterListesi.Items[num].CheckState = CheckState.Checked;
			}
			else
			{
				KriterListesi.Items[num].CheckState = CheckState.Unchecked;
			}
			num++;
		}
	}

	private void KullaniciParametreKaydet()
	{
		te_kullanici_adi.Text = AktifKullanici;
		_aktarimparametreleri._GetParametre("SablonAdi")._SetString = AktifKullanici;
		_aktarimparametreleri._GetParametre("KriterListesi")._SetString = KriterListesi.Text;
		_aktarimparametreleri._GetParametre("SqlServer")._SetString = SqlServer.Text;
		_aktarimparametreleri._GetParametre("SqlServerPort")._SetString = SqlServerPort.Text;
		_aktarimparametreleri._GetParametre("SqlUserName")._SetString = SqlUserName.Text;
		_aktarimparametreleri._GetParametre("SqlPassword")._SetString = SqlPassword.Text;
		_aktarimparametreleri._GetParametre("DBName")._SetString = DBName.Text;
		_aktarimparametreleri._GetParametre("Query")._SetString = Query.Text;
		_aktarimparametreleri._GetParametre("firma_no")._SetInt = (int)firma_no.Value;
		_aktarimparametreleri._GetParametre("sube_no")._SetInt = (int)sube_no.Value;
		_aktarimparametreleri._GetParametre("evrak_tarihi_baslangic")._SetInt = (int)evrak_tarihi_baslangic.Value;
		_aktarimparametreleri._GetParametre("belge_no_baslangic")._SetInt = (int)belge_no_baslangic.Value;
		_aktarimparametreleri._GetParametre("belge_tarihi_baslangic")._SetInt = (int)belge_tarihi_baslangic.Value;
		_aktarimparametreleri._GetParametre("kur_baslangic")._SetInt = (int)kur_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_cinsi_sabit_kullan")._SetBoolean = satir_cinsi_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_cinsi_sabit_deger")._SetInt = (int)satir_cinsi_sabit_deger.SelectedValue;
		_aktarimparametreleri._GetParametre("satir_cinsi_baslangic")._SetInt = (int)satir_cinsi_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_cinsi_veri_nakit")._SetString = satir_cinsi_veri_nakit.Text;
		_aktarimparametreleri._GetParametre("satir_cinsi_veri_musteri_ceki")._SetString = satir_cinsi_veri_cek.Text;
		_aktarimparametreleri._GetParametre("satir_cinsi_veri_musteri_seneti")._SetString = satir_cinsi_veri_senet.Text;
		_aktarimparametreleri._GetParametre("satir_cinsi_veri_musteri_kredi_karti")._SetString = satir_cinsi_veri_kredi_karti.Text;
		_aktarimparametreleri._GetParametre("satir_cinsi_veri_gelen_havale")._SetString = satir_cinsi_veri_gelen_havale.Text;
		_aktarimparametreleri._GetParametre("satir_cinsi_veri_giden_havale")._SetString = satir_cinsi_veri_giden_havale.Text;
		_aktarimparametreleri._GetParametre("cari_kod_sabit_kullan")._SetBoolean = cari_kod_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_kod_sabit_deger")._SetString = cari_kod_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_kod_baslangic")._SetInt = (int)cari_kod_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_arama_secenekleri")._SetString = cari_arama_secenekleri.Text;
		_aktarimparametreleri._GetParametre("cari_unvan_turkce_karakterleri_kaldir")._SetBoolean = cari_unvan_turkce_karakterleri_kaldir.Checked;
		_aktarimparametreleri._GetParametre("cari_unvan_baslangic")._SetInt = (int)cari_unvan_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_unvan2_baslangic")._SetInt = (int)cari_unvan2_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_vergi_no_baslangic")._SetInt = (int)cari_vergi_no_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_tc_kimlik_no_baslangic")._SetInt = (int)cari_tc_kimlik_no_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_vergi_dairesi_baslangic")._SetInt = (int)cari_vergi_dairesi_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_banka_hesap_no_baslangic")._SetInt = (int)cari_banka_hesap_no_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_adres_baslangic")._SetInt = (int)cari_adres_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_mahalle_baslangic")._SetInt = (int)cari_mahalle_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_ilce_baslangic")._SetInt = (int)cari_ilce_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_il_baslangic")._SetInt = (int)cari_il_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_ulke_baslangic")._SetInt = (int)cari_ulke_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_posta_kodu_baslangic")._SetInt = (int)cari_posta_kodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_telefon_baslangic")._SetInt = (int)cari_telefon_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_eposta_baslangic")._SetInt = (int)cari_eposta_baslangic.Value;
		_aktarimparametreleri._GetParametre("proje_kodu_sabit_kullan")._SetBoolean = proje_kodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("proje_kodu_sabit_deger")._SetString = proje_kodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("proje_kodu_baslangic")._SetInt = (int)proje_kodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("sor_mer_kodu_sabit_kullan")._SetBoolean = sor_mer_kodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("sor_mer_kodu_sabit_deger")._SetString = sor_mer_kodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("sor_mer_kodu_baslangic")._SetInt = (int)sor_mer_kodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("plasiyer_kodu_sabit_kullan")._SetBoolean = plasiyer_kodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("plasiyer_kodu_cariden_kullan")._SetBoolean = plasiyer_kodu_cariden_kullan.Checked;
		_aktarimparametreleri._GetParametre("plasiyer_kodu_sabit_deger")._SetString = plasiyer_kodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("plasiyer_kodu_baslangic")._SetInt = (int)plasiyer_kodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_aciklama_sabit_kullan")._SetBoolean = satir_aciklama_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_aciklama_sabit_deger")._SetString = satir_aciklama_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("satir_aciklama_baslangic")._SetInt = (int)satir_aciklama_baslangic.Value;
		_aktarimparametreleri._GetParametre("aciklama1_sabit_kullan")._SetBoolean = aciklama1_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama1_sabit_deger")._SetString = aciklama1_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama1_baslangic")._SetInt = (int)aciklama1_baslangic.Value;
		_aktarimparametreleri._GetParametre("aciklama2_sabit_kullan")._SetBoolean = aciklama2_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama2_sabit_deger")._SetString = aciklama2_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama2_baslangic")._SetInt = (int)aciklama2_baslangic.Value;
		_aktarimparametreleri._GetParametre("aciklama3_sabit_kullan")._SetBoolean = aciklama3_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama3_sabit_deger")._SetString = aciklama3_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama3_baslangic")._SetInt = (int)aciklama3_baslangic.Value;
		_aktarimparametreleri._GetParametre("aciklama4_sabit_kullan")._SetBoolean = aciklama4_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama4_sabit_deger")._SetString = aciklama4_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama4_baslangic")._SetInt = (int)aciklama4_baslangic.Value;
		_aktarimparametreleri._GetParametre("aciklama5_sabit_kullan")._SetBoolean = aciklama5_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama5_sabit_deger")._SetString = aciklama5_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama5_baslangic")._SetInt = (int)aciklama5_baslangic.Value;
		_aktarimparametreleri._GetParametre("aciklama6_sabit_kullan")._SetBoolean = aciklama6_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama6_sabit_deger")._SetString = aciklama6_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama6_baslangic")._SetInt = (int)aciklama6_baslangic.Value;
		_aktarimparametreleri._GetParametre("aciklama7_sabit_kullan")._SetBoolean = aciklama7_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama7_sabit_deger")._SetString = aciklama7_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama7_baslangic")._SetInt = (int)aciklama7_baslangic.Value;
		_aktarimparametreleri._GetParametre("aciklama8_sabit_kullan")._SetBoolean = aciklama8_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama8_sabit_deger")._SetString = aciklama8_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama8_baslangic")._SetInt = (int)aciklama8_baslangic.Value;
		_aktarimparametreleri._GetParametre("aciklama9_sabit_kullan")._SetBoolean = aciklama9_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama9_sabit_deger")._SetString = aciklama9_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama9_baslangic")._SetInt = (int)aciklama9_baslangic.Value;
		_aktarimparametreleri._GetParametre("aciklama10_sabit_kullan")._SetBoolean = aciklama10_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama10_sabit_deger")._SetString = aciklama10_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama10_baslangic")._SetInt = (int)aciklama10_baslangic.Value;
		_aktarimparametreleri._GetParametre("ozel_alan_1_sabit_kullan")._SetBoolean = ozel_alan_1_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("ozel_alan_1_sabit_deger")._SetString = ozel_alan_1_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("ozel_alan_1_baslangic")._SetInt = (int)ozel_alan_1_baslangic.Value;
		_aktarimparametreleri._GetParametre("ozel_alan_2_sabit_kullan")._SetBoolean = ozel_alan_2_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("ozel_alan_2_sabit_deger")._SetString = ozel_alan_2_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("ozel_alan_2_baslangic")._SetInt = (int)ozel_alan_2_baslangic.Value;
		_aktarimparametreleri._GetParametre("ozel_alan_3_sabit_kullan")._SetBoolean = ozel_alan_3_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("ozel_alan_3_sabit_deger")._SetString = ozel_alan_3_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("ozel_alan_3_baslangic")._SetInt = (int)ozel_alan_3_baslangic.Value;
		_aktarimparametreleri._GetParametre("evrak_seri_sabit_kullan")._SetBoolean = evrak_seri_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("evrak_seri_sabit_deger")._SetString = evrak_seri_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("evrak_seri_baslangic")._SetInt = (int)evrak_seri_baslangic.Value;
		_aktarimparametreleri._GetParametre("evrak_sira_baslangic")._SetInt = (int)evrak_sira_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_kodu_on_ek_kullan")._SetBoolean = cari_kodu_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_kodu_on_ek_satis")._SetString = cari_kodu_on_ek_satis.Text;
		_aktarimparametreleri._GetParametre("satir_aciklama_on_ek_kullan")._SetBoolean = satir_aciklama_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_aciklama_on_ek_deger")._SetString = satir_aciklama_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama1_on_ek_kullan")._SetBoolean = aciklama1_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama1_on_ek_deger")._SetString = aciklama1_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama2_on_ek_kullan")._SetBoolean = aciklama2_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama2_on_ek_deger")._SetString = aciklama2_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama3_on_ek_kullan")._SetBoolean = aciklama3_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama3_on_ek_deger")._SetString = aciklama3_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama4_on_ek_kullan")._SetBoolean = aciklama4_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama4_on_ek_deger")._SetString = aciklama4_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama5_on_ek_kullan")._SetBoolean = aciklama5_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama5_on_ek_deger")._SetString = aciklama5_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama6_on_ek_kullan")._SetBoolean = aciklama6_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama6_on_ek_deger")._SetString = aciklama6_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama7_on_ek_kullan")._SetBoolean = aciklama7_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama7_on_ek_deger")._SetString = aciklama7_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama8_on_ek_kullan")._SetBoolean = aciklama8_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama8_on_ek_deger")._SetString = aciklama8_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama9_on_ek_kullan")._SetBoolean = aciklama9_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama9_on_ek_deger")._SetString = aciklama9_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("aciklama10_on_ek_kullan")._SetBoolean = aciklama10_on_ek_kullan.Checked;
		_aktarimparametreleri._GetParametre("aciklama10_on_ek_deger")._SetString = aciklama10_on_ek_deger.Text;
		_aktarimparametreleri._GetParametre("cari_il_bilgisi_plaka_kodu")._SetBoolean = cari_il_bilgisi_plaka_kodu.Checked;
		_aktarimparametreleri._GetParametre("cari_doviz_cinsi")._SetInt = (int)cari_doviz_cinsi.SelectedValue;
		_aktarimparametreleri._GetParametre("cari_doviz_cinsi1")._SetInt = (int)cari_doviz_cinsi1.SelectedValue;
		_aktarimparametreleri._GetParametre("cari_doviz_cinsi2")._SetInt = (int)cari_doviz_cinsi2.SelectedValue;
		_aktarimparametreleri._GetParametre("cari_muh_kod_satis")._SetString = cari_muh_kod_satis.Text;
		_aktarimparametreleri._GetParametre("cari_muh_kod1_satis")._SetString = cari_muh_kod1_satis.Text;
		_aktarimparametreleri._GetParametre("cari_muh_kod2_satis")._SetString = cari_muh_kod2_satis.Text;
		_aktarimparametreleri._GetParametre("cari_muhartikeli")._SetString = cari_muhartikeli.Text;
		_aktarimparametreleri._GetParametre("cari_kod2_baslangic")._SetInt = (int)cari_kod2_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_metin1_baslangic")._SetInt = (int)kriter_metin1_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_metin2_baslangic")._SetInt = (int)kriter_metin2_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_metin3_baslangic")._SetInt = (int)kriter_metin3_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_metin4_baslangic")._SetInt = (int)kriter_metin4_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_metin5_baslangic")._SetInt = (int)kriter_metin5_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_double1_baslangic")._SetInt = (int)kriter_double1_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_double2_baslangic")._SetInt = (int)kriter_double2_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_double3_baslangic")._SetInt = (int)kriter_double3_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_double4_baslangic")._SetInt = (int)kriter_double4_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_double5_baslangic")._SetInt = (int)kriter_double5_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_bool1_baslangic")._SetInt = (int)kriter_bool1_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_bool1_evet_icin_deger")._SetString = kriter_bool1_evet_icin_deger.Text;
		_aktarimparametreleri._GetParametre("kriter_bool2_baslangic")._SetInt = (int)kriter_bool2_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_bool2_evet_icin_deger")._SetString = kriter_bool2_evet_icin_deger.Text;
		_aktarimparametreleri._GetParametre("kriter_bool3_baslangic")._SetInt = (int)kriter_bool3_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_bool3_evet_icin_deger")._SetString = kriter_bool3_evet_icin_deger.Text;
		_aktarimparametreleri._GetParametre("kriter_bool4_baslangic")._SetInt = (int)kriter_bool4_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_bool4_evet_icin_deger")._SetString = kriter_bool4_evet_icin_deger.Text;
		_aktarimparametreleri._GetParametre("kriter_bool5_baslangic")._SetInt = (int)kriter_bool5_baslangic.Value;
		_aktarimparametreleri._GetParametre("kriter_bool5_evet_icin_deger")._SetString = kriter_bool5_evet_icin_deger.Text;
		_aktarimparametreleri._GetParametre("kayit_id_otomatik_ver")._SetBoolean = kayit_id_otomatik_ver.Checked;
		_aktarimparametreleri._GetParametre("kayit_id_baslangic")._SetInt = (int)kayit_id_baslangic.Value;
		_aktarimparametreleri._GetParametre("otomatik_hesap_acma_secenek_cari")._SetInt = (int)otomatik_hesap_acma_secenek_cari.SelectedValue;
		_aktarimparametreleri._GetParametre("otomatik_hesap_acma_secenek_proje")._SetInt = (int)otomatik_hesap_acma_secenek_proje.SelectedValue;
		_aktarimparametreleri._GetParametre("otomatik_hesap_acma_secenek_sorumluluk")._SetInt = (int)otomatik_hesap_acma_secenek_sorumluluk.SelectedValue;
		_aktarimparametreleri._GetParametre("evrak_sira_otomatik_ver")._SetBoolean = evrak_sira_otomatik_ver.Checked;
		_aktarimparametreleri._GetParametre("pro_adi_sabit_kullan")._SetBoolean = pro_adi_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("pro_adi_sabit_deger")._SetString = pro_adi_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("pro_adi_baslangic")._SetInt = (int)pro_adi_baslangic.Value;
		_aktarimparametreleri._GetParametre("pro_musterikodu_sabit_kullan")._SetBoolean = pro_musterikodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("pro_musterikodu_sabit_deger")._SetString = pro_musterikodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("pro_musterikodu_baslangic")._SetInt = (int)pro_musterikodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("pro_sormerkodu_sabit_kullan")._SetBoolean = pro_sormerkodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("pro_sormerkodu_sabit_deger")._SetString = pro_sormerkodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("pro_sormerkodu_baslangic")._SetInt = (int)pro_sormerkodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("pro_grupkodu_sabit_kullan")._SetBoolean = pro_grupkodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("pro_grupkodu_sabit_deger")._SetString = pro_grupkodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("pro_grupkodu_baslangic")._SetInt = (int)pro_grupkodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("pro_sektorkodu_sabit_kullan")._SetBoolean = pro_sektorkodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("pro_sektorkodu_sabit_deger")._SetString = pro_sektorkodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("pro_sektorkodu_baslangic")._SetInt = (int)pro_sektorkodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("pro_bolgekodu_sabit_kullan")._SetBoolean = pro_bolgekodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("pro_bolgekodu_sabit_deger")._SetString = pro_bolgekodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("pro_bolgekodu_baslangic")._SetInt = (int)pro_bolgekodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("pro_ana_projekodu_sabit_kullan")._SetBoolean = pro_ana_projekodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("pro_ana_projekodu_sabit_deger")._SetString = pro_ana_projekodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("pro_ana_projekodu_baslangic")._SetInt = (int)pro_ana_projekodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("pro_aciklama_sabit_kullan")._SetBoolean = pro_aciklama_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("pro_aciklama_sabit_deger")._SetString = pro_aciklama_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("pro_aciklama_baslangic")._SetInt = (int)pro_aciklama_baslangic.Value;
		_aktarimparametreleri._GetParametre("pro_muh_kod_artikeli_sabit_kullan")._SetBoolean = pro_muh_kod_artikeli_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("pro_muh_kod_artikeli_sabit_deger")._SetString = pro_muh_kod_artikeli_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("pro_muh_kod_artikeli_baslangic")._SetInt = (int)pro_muh_kod_artikeli_baslangic.Value;
		_aktarimparametreleri._GetParametre("som_isim_sabit_kullan")._SetBoolean = som_isim_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("som_isim_sabit_deger")._SetString = som_isim_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("som_isim_baslangic")._SetInt = (int)som_isim_baslangic.Value;
		_aktarimparametreleri._GetParametre("som_MuhArtikeli_sabit_kullan")._SetBoolean = som_MuhArtikeli_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("som_MuhArtikeli_sabit_deger")._SetString = som_MuhArtikeli_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("som_MuhArtikeli_baslangic")._SetInt = (int)som_MuhArtikeli_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_muhartikeli_sabit_kullan")._SetBoolean = cari_muhartikeli_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_muhartikeli_baslangic")._SetInt = (int)cari_muhartikeli_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_muh_kod_satis_sabit_kullan")._SetBoolean = cari_muh_kod_satis_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_muh_kod_satis_baslangic")._SetInt = (int)cari_muh_kod_satis_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_muh_kod1_satis_sabit_kullan")._SetBoolean = cari_muh_kod1_satis_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_muh_kod1_satis_baslangic")._SetInt = (int)cari_muh_kod1_satis_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_muh_kod2_satis_sabit_kullan")._SetBoolean = cari_muh_kod2_satis_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_muh_kod2_satis_baslangic")._SetInt = (int)cari_muh_kod2_satis_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_Ana_cari_kodu_sabit_kullan")._SetBoolean = cari_Ana_cari_kodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_Ana_cari_kodu_sabit_deger")._SetString = cari_Ana_cari_kodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_Ana_cari_kodu_baslangic")._SetInt = (int)cari_Ana_cari_kodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_temsilci_kodu_sabit_kullan")._SetBoolean = cari_temsilci_kodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_temsilci_kodu_sabit_deger")._SetString = cari_temsilci_kodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_temsilci_kodu_baslangic")._SetInt = (int)cari_temsilci_kodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_grup_kodu_sabit_kullan")._SetBoolean = cari_grup_kodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_grup_kodu_sabit_deger")._SetString = cari_grup_kodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_grup_kodu_baslangic")._SetInt = (int)cari_grup_kodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_sektor_kodu_sabit_kullan")._SetBoolean = cari_sektor_kodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_sektor_kodu_sabit_deger")._SetString = cari_sektor_kodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_sektor_kodu_baslangic")._SetInt = (int)cari_sektor_kodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_bolge_kodu_sabit_kullan")._SetBoolean = cari_bolge_kodu_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_bolge_kodu_sabit_deger")._SetString = cari_bolge_kodu_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_bolge_kodu_baslangic")._SetInt = (int)cari_bolge_kodu_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_wwwadresi_sabit_kullan")._SetBoolean = cari_wwwadresi_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_wwwadresi_sabit_deger")._SetString = cari_wwwadresi_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_wwwadresi_baslangic")._SetInt = (int)cari_wwwadresi_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_CepTel_sabit_kullan")._SetBoolean = cari_CepTel_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_CepTel_sabit_deger")._SetString = cari_CepTel_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_CepTel_baslangic")._SetInt = (int)cari_CepTel_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_satis_isk_kod_sabit_kullan")._SetBoolean = cari_satis_isk_kod_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_satis_isk_kod_sabit_deger")._SetString = cari_satis_isk_kod_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_satis_isk_kod_baslangic")._SetInt = (int)cari_satis_isk_kod_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_sicil_no_sabit_kullan")._SetBoolean = cari_sicil_no_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_sicil_no_sabit_deger")._SetString = cari_sicil_no_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_sicil_no_baslangic")._SetInt = (int)cari_sicil_no_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_VarsayilanGirisDepo_sabit_kullan")._SetBoolean = cari_VarsayilanGirisDepo_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_VarsayilanGirisDepo_sabit_deger")._SetString = cari_VarsayilanGirisDepo_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_VarsayilanGirisDepo_baslangic")._SetInt = (int)cari_VarsayilanGirisDepo_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_VarsayilanCikisDepo_sabit_kullan")._SetBoolean = cari_VarsayilanCikisDepo_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_VarsayilanCikisDepo_sabit_deger")._SetString = cari_VarsayilanCikisDepo_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_VarsayilanCikisDepo_baslangic")._SetInt = (int)cari_VarsayilanCikisDepo_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_Portal_PW_sabit_kullan")._SetBoolean = cari_Portal_PW_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("cari_Portal_PW_sabit_deger")._SetString = cari_Portal_PW_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("cari_Portal_PW_baslangic")._SetInt = (int)cari_Portal_PW_baslangic.Value;
		_aktarimparametreleri._GetParametre("cari_Portal_Enabled")._SetBoolean = cari_Portal_Enabled.Checked;
		_aktarimparametreleri._GetParametre("satir_tutar_sabit_kullan")._SetBoolean = satir_tutar_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_tutar_sabit_deger")._SetString = satir_tutar_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("satir_tutar_baslangic")._SetInt = (int)satir_tutar_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_tutar_islem_1_islem_tipi")._SetInt = (int)satir_tutar_islem_1_islem_tipi.SelectedValue;
		_aktarimparametreleri._GetParametre("satir_tutar_islem_1_baslangic")._SetInt = (int)satir_tutar_islem_1_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_tutar_islem_2_islem_tipi")._SetInt = (int)satir_tutar_islem_2_islem_tipi.SelectedValue;
		_aktarimparametreleri._GetParametre("satir_tutar_islem_2_baslangic")._SetInt = (int)satir_tutar_islem_2_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_vadesi_sabit_kullan")._SetBoolean = satir_vadesi_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_vadesi_sabit_deger")._SetString = satir_vadesi_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("satir_vadesi_baslangic")._SetInt = (int)satir_vadesi_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_sorumlulukmerkezi_sabit_kullan")._SetBoolean = satir_sorumlulukmerkezi_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_sorumlulukmerkezi_sabit_deger")._SetString = satir_sorumlulukmerkezi_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("satir_sorumlulukmerkezi_baslangic")._SetInt = (int)satir_sorumlulukmerkezi_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_nakit_sabit_kullan")._SetBoolean = satir_hesap_kodu_nakit_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_nakit_sabit_deger")._SetString = satir_hesap_kodu_nakit_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_nakit_baslangic")._SetInt = (int)satir_hesap_kodu_nakit_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_nakit_on_ek")._SetString = satir_hesap_kodu_nakit_on_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_nakit_son_ek")._SetString = satir_hesap_kodu_nakit_son_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_cek_sabit_kullan")._SetBoolean = satir_hesap_kodu_cek_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_cek_sabit_deger")._SetString = satir_hesap_kodu_cek_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_cek_baslangic")._SetInt = (int)satir_hesap_kodu_cek_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_cek_on_ek")._SetString = satir_hesap_kodu_cek_on_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_cek_son_ek")._SetString = satir_hesap_kodu_cek_son_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_senet_sabit_kullan")._SetBoolean = satir_hesap_kodu_senet_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_senet_sabit_deger")._SetString = satir_hesap_kodu_senet_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_senet_baslangic")._SetInt = (int)satir_hesap_kodu_senet_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_senet_on_ek")._SetString = satir_hesap_kodu_senet_on_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_senet_son_ek")._SetString = satir_hesap_kodu_senet_son_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_kredi_karti_sabit_kullan")._SetBoolean = satir_hesap_kodu_kredi_karti_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_kredi_karti_sabit_deger")._SetString = satir_hesap_kodu_kredi_karti_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_kredi_karti_baslangic")._SetInt = (int)satir_hesap_kodu_kredi_karti_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_kredi_karti_on_ek")._SetString = satir_hesap_kodu_kredi_karti_on_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_kredi_karti_son_ek")._SetString = satir_hesap_kodu_kredi_karti_son_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_gelen_havale_sabit_kullan")._SetBoolean = satir_hesap_kodu_gelen_havale_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_gelen_havale_sabit_deger")._SetString = satir_hesap_kodu_gelen_havale_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_gelen_havale_baslangic")._SetInt = (int)satir_hesap_kodu_gelen_havale_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_gelen_havale_on_ek")._SetString = satir_hesap_kodu_gelen_havale_on_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_gelen_havale_son_ek")._SetString = satir_hesap_kodu_gelen_havale_son_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_giden_havale_sabit_kullan")._SetBoolean = satir_hesap_kodu_giden_havale_sabit_kullan.Checked;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_giden_havale_sabit_deger")._SetString = satir_hesap_kodu_giden_havale_sabit_deger.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_giden_havale_baslangic")._SetInt = (int)satir_hesap_kodu_giden_havale_baslangic.Value;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_giden_havale_on_ek")._SetString = satir_hesap_kodu_giden_havale_on_ek.Text;
		_aktarimparametreleri._GetParametre("satir_hesap_kodu_giden_havale_son_ek")._SetString = satir_hesap_kodu_giden_havale_son_ek.Text;
		_aktarimparametreleri._GetParametre("dbc_no")._SetInt = (int)dbc_no.Value;
		string text = "";
		bool flag = true;
		foreach (object item in (IEnumerable)KriterListesi.CheckedItems)
		{
			if (!flag)
			{
				text += ",";
			}
			string text2 = item.ToString();
			text += text2;
			flag = false;
		}
		_aktarimparametreleri._GetParametre("KriterListesi")._SetString = text;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _aktarimparametreleri);
		_aktarimparametreleri = ParametrelerDefault.TahsilatAktarimSqlSablon(AktifKullanici);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _aktarimparametreleri, "TahsilatAktarim", "", "SqlAktarimSablon", AktifKullanici);
		EkranBilgiGuncelle();
		DegisiklikVar = false;
	}

	private void AktarimKriterSablonlariOlustur()
	{
		KriterListesi.Items.Clear();
		foreach (string item in TahsilatEvrakKriterParametreleri.GetKriterAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			KriterListesi.Items.Add(item);
		}
	}

	private void lb_kullanicilar_SelectedValueChanged(object sender, EventArgs e)
	{
		if (lb_kullanicilar.SelectedValue == null)
		{
			return;
		}
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
			tc_ayarlar.Enabled = true;
			AktifKullanici = lb_kullanicilar.SelectedValue.ToString();
			_aktarimparametreleri = ParametrelerDefault.TahsilatAktarimSqlSablon(AktifKullanici);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _aktarimparametreleri, "TahsilatAktarim", "", "SqlAktarimSablon", AktifKullanici);
			EkranBilgiGuncelle();
			DegisiklikVar = false;
		}
		else
		{
			lb_kullanicilar.SelectedItem = AktifKullanici;
		}
	}

	private void degisiklikleriKaydetToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (lb_kullanicilar.SelectedValue != null)
		{
			KullaniciParametreKaydet();
		}
	}

	private void sablonSilToolStripMenuItem_Click(object sender, EventArgs e)
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
			DialogResult dialogResult = MessageBox.Show(text + " parametrelerini silmek istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult != DialogResult.Yes)
			{
				_ = 7;
				return;
			}
			SqlImportAktarimParametreleri.ParametreSil(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text);
			KullanicilariListele();
		}
	}

	private void sablonEkleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				break;
			case DialogResult.Cancel:
				return;
			}
		}
		TextSor textSor = new TextSor("Şablon adı", "DEVAM");
		if (textSor.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		foreach (string item in lb_kullanicilar.Items)
		{
			if (item == textSor.te_cevap.Text)
			{
				MessageBox.Show("Bu şablon adı daha önce kullanılmış. İşlem tamamlanamadı!");
				return;
			}
		}
		Parametreler parametreler = ParametrelerDefault.TahsilatAktarimSqlSablon(textSor.te_cevap.Text);
		parametreler._GetParametre("SablonAdi")._SetString = textSor.te_cevap.Text;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler);
		KullanicilariListele();
		lb_kullanicilar.SelectedItem = textSor.te_cevap.Text;
	}

	private void dosyayaYazToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				break;
			case DialogResult.Cancel:
				return;
			}
		}
		try
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Fora parametreler|*.fpt|Tüm dosyalar|*.*";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.CreateNew);
				_aktarimparametreleri.KullanimAlani = "TahsilatSqlImportParametreleri";
				Parametreler.WriteToStream(_aktarimparametreleri, fileStream, 2);
				fileStream.Close();
				fileStream.Dispose();
			}
		}
		catch
		{
			MessageBox.Show("Dosya oluşturulamadı!");
		}
	}

	private void dosyadanOkuToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Fora parametreler|*.fpt|Tüm dosyalar|*.*";
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		TextSor textSor = new TextSor("Şablon adı", "DEVAM");
		if (textSor.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		foreach (string item in lb_kullanicilar.Items)
		{
			if (item == textSor.te_cevap.Text)
			{
				MessageBox.Show("Bu şablon adı daha önce kullanılmış. İşlem tamamlanamadı!");
				return;
			}
		}
		FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
		Parametreler parametreler = Parametreler.ReadFromStream(fileStream);
		fileStream.Close();
		fileStream.Dispose();
		if (parametreler.KullanimAlani != "TahsilatSqlImportParametreleri")
		{
			MessageBox.Show("Dosya uygun değil. İşlem tamamlanamadı!");
			return;
		}
		parametreler._GetParametre("SablonAdi")._SetString = textSor.te_cevap.Text;
		foreach (Parametre item2 in parametreler.ParametreListesi)
		{
			item2.EskiID = 0;
			item2.IDGuid = null;
			item2.ParametreAltGrubu = textSor.te_cevap.Text;
		}
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler);
		KullanicilariListele();
		lb_kullanicilar.SelectedItem = textSor.te_cevap.Text;
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
		this.te_kullanici_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.tc_ayarlar = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
		this.labelControl14 = new DevExpress.XtraEditors.LabelControl();
		this.label17 = new System.Windows.Forms.Label();
		this.Query = new DevExpress.XtraEditors.TextEdit();
		this.label16 = new System.Windows.Forms.Label();
		this.DBName = new DevExpress.XtraEditors.TextEdit();
		this.label15 = new System.Windows.Forms.Label();
		this.SqlPassword = new DevExpress.XtraEditors.TextEdit();
		this.label10 = new System.Windows.Forms.Label();
		this.SqlUserName = new DevExpress.XtraEditors.TextEdit();
		this.label9 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.KriterListesi = new DevExpress.XtraEditors.CheckedListBoxControl();
		this.label536 = new System.Windows.Forms.Label();
		this.SqlServer = new DevExpress.XtraEditors.TextEdit();
		this.SqlServerPort = new DevExpress.XtraEditors.TextEdit();
		this.xtraTabPage6 = new DevExpress.XtraTab.XtraTabPage();
		this.dbc_no = new DevExpress.XtraEditors.SpinEdit();
		this.label76 = new System.Windows.Forms.Label();
		this.kayit_id_otomatik_ver = new DevExpress.XtraEditors.CheckEdit();
		this.label554 = new System.Windows.Forms.Label();
		this.kayit_id_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label555 = new System.Windows.Forms.Label();
		this.label550 = new System.Windows.Forms.Label();
		this.label264 = new System.Windows.Forms.Label();
		this.sube_no = new DevExpress.XtraEditors.SpinEdit();
		this.firma_no = new DevExpress.XtraEditors.SpinEdit();
		this.label211 = new System.Windows.Forms.Label();
		this.label210 = new System.Windows.Forms.Label();
		this.xtraTabPage5 = new DevExpress.XtraTab.XtraTabPage();
		this.xtraTabControl2 = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage16 = new DevExpress.XtraTab.XtraTabPage();
		this.label13 = new System.Windows.Forms.Label();
		this.label548 = new System.Windows.Forms.Label();
		this.cari_banka_hesap_no_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label233 = new System.Windows.Forms.Label();
		this.cari_unvan_turkce_karakterleri_kaldir = new DevExpress.XtraEditors.CheckEdit();
		this.cari_unvan2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label223 = new System.Windows.Forms.Label();
		this.label217 = new System.Windows.Forms.Label();
		this.cari_unvan_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label218 = new System.Windows.Forms.Label();
		this.label215 = new System.Windows.Forms.Label();
		this.cari_eposta_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label232 = new System.Windows.Forms.Label();
		this.label537 = new System.Windows.Forms.Label();
		this.label546 = new System.Windows.Forms.Label();
		this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
		this.label422 = new System.Windows.Forms.Label();
		this.cari_kod2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.label291 = new System.Windows.Forms.Label();
		this.label289 = new System.Windows.Forms.Label();
		this.cari_tc_kimlik_no_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label220 = new System.Windows.Forms.Label();
		this.cari_vergi_no_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label219 = new System.Windows.Forms.Label();
		this.label94 = new System.Windows.Forms.Label();
		this.label93 = new System.Windows.Forms.Label();
		this.cari_kod_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label92 = new System.Windows.Forms.Label();
		this.cari_kod_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label91 = new System.Windows.Forms.Label();
		this.cari_kod_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label212 = new System.Windows.Forms.Label();
		this.cari_kodu_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.cari_kodu_on_ek_satis = new DevExpress.XtraEditors.TextEdit();
		this.label213 = new System.Windows.Forms.Label();
		this.label214 = new System.Windows.Forms.Label();
		this.cari_arama_secenekleri = new DevExpress.XtraEditors.TextEdit();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.xtraTabPage3 = new DevExpress.XtraTab.XtraTabPage();
		this.evrak_sira_otomatik_ver = new DevExpress.XtraEditors.CheckEdit();
		this.label207 = new System.Windows.Forms.Label();
		this.label205 = new System.Windows.Forms.Label();
		this.evrak_seri_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label197 = new System.Windows.Forms.Label();
		this.evrak_seri_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label198 = new System.Windows.Forms.Label();
		this.evrak_seri_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label199 = new System.Windows.Forms.Label();
		this.label45 = new System.Windows.Forms.Label();
		this.label41 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.evrak_tarihi_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label29 = new System.Windows.Forms.Label();
		this.evrak_sira_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label6 = new System.Windows.Forms.Label();
		this.xtraTabPage8 = new DevExpress.XtraTab.XtraTabPage();
		this.label209 = new System.Windows.Forms.Label();
		this.belge_no_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label7 = new System.Windows.Forms.Label();
		this.label62 = new System.Windows.Forms.Label();
		this.belge_tarihi_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label11 = new System.Windows.Forms.Label();
		this.label474 = new System.Windows.Forms.Label();
		this.label476 = new System.Windows.Forms.Label();
		this.label95 = new System.Windows.Forms.Label();
		this.kur_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label12 = new System.Windows.Forms.Label();
		this.label473 = new System.Windows.Forms.Label();
		this.plasiyer_kodu_cariden_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.plasiyer_kodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label123 = new System.Windows.Forms.Label();
		this.plasiyer_kodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label124 = new System.Windows.Forms.Label();
		this.plasiyer_kodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label125 = new System.Windows.Forms.Label();
		this.label126 = new System.Windows.Forms.Label();
		this.sor_mer_kodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label118 = new System.Windows.Forms.Label();
		this.sor_mer_kodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label119 = new System.Windows.Forms.Label();
		this.sor_mer_kodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label120 = new System.Windows.Forms.Label();
		this.label121 = new System.Windows.Forms.Label();
		this.proje_kodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label113 = new System.Windows.Forms.Label();
		this.proje_kodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label114 = new System.Windows.Forms.Label();
		this.proje_kodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label115 = new System.Windows.Forms.Label();
		this.label116 = new System.Windows.Forms.Label();
		this.xtraTabPage9 = new DevExpress.XtraTab.XtraTabPage();
		this.aciklama10_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.aciklama10_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.aciklama9_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.aciklama9_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.aciklama8_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.aciklama8_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.aciklama7_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.aciklama7_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.aciklama6_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.aciklama6_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.aciklama5_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.aciklama5_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.aciklama4_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.aciklama4_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.aciklama3_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.aciklama3_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.aciklama2_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.aciklama2_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.aciklama1_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.aciklama1_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.aciklama10_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label165 = new System.Windows.Forms.Label();
		this.aciklama10_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label166 = new System.Windows.Forms.Label();
		this.aciklama10_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label167 = new System.Windows.Forms.Label();
		this.label168 = new System.Windows.Forms.Label();
		this.aciklama9_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label160 = new System.Windows.Forms.Label();
		this.aciklama9_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label161 = new System.Windows.Forms.Label();
		this.aciklama9_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label162 = new System.Windows.Forms.Label();
		this.label163 = new System.Windows.Forms.Label();
		this.aciklama8_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label155 = new System.Windows.Forms.Label();
		this.aciklama8_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label156 = new System.Windows.Forms.Label();
		this.aciklama8_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label157 = new System.Windows.Forms.Label();
		this.label158 = new System.Windows.Forms.Label();
		this.aciklama7_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label150 = new System.Windows.Forms.Label();
		this.aciklama7_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label151 = new System.Windows.Forms.Label();
		this.aciklama7_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label152 = new System.Windows.Forms.Label();
		this.label153 = new System.Windows.Forms.Label();
		this.aciklama6_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label145 = new System.Windows.Forms.Label();
		this.aciklama6_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label146 = new System.Windows.Forms.Label();
		this.aciklama6_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label147 = new System.Windows.Forms.Label();
		this.label148 = new System.Windows.Forms.Label();
		this.aciklama5_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label140 = new System.Windows.Forms.Label();
		this.aciklama5_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label141 = new System.Windows.Forms.Label();
		this.aciklama5_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label142 = new System.Windows.Forms.Label();
		this.label143 = new System.Windows.Forms.Label();
		this.aciklama4_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label135 = new System.Windows.Forms.Label();
		this.aciklama4_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label136 = new System.Windows.Forms.Label();
		this.aciklama4_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label137 = new System.Windows.Forms.Label();
		this.label138 = new System.Windows.Forms.Label();
		this.aciklama3_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label130 = new System.Windows.Forms.Label();
		this.aciklama3_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label131 = new System.Windows.Forms.Label();
		this.aciklama3_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label132 = new System.Windows.Forms.Label();
		this.label133 = new System.Windows.Forms.Label();
		this.aciklama2_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label36 = new System.Windows.Forms.Label();
		this.aciklama2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label37 = new System.Windows.Forms.Label();
		this.aciklama2_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label38 = new System.Windows.Forms.Label();
		this.label39 = new System.Windows.Forms.Label();
		this.aciklama1_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label27 = new System.Windows.Forms.Label();
		this.aciklama1_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label28 = new System.Windows.Forms.Label();
		this.aciklama1_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label31 = new System.Windows.Forms.Label();
		this.label32 = new System.Windows.Forms.Label();
		this.xtraTabPage10 = new DevExpress.XtraTab.XtraTabPage();
		this.ozel_alan_3_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label180 = new System.Windows.Forms.Label();
		this.ozel_alan_3_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label181 = new System.Windows.Forms.Label();
		this.ozel_alan_3_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label182 = new System.Windows.Forms.Label();
		this.label183 = new System.Windows.Forms.Label();
		this.ozel_alan_2_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label175 = new System.Windows.Forms.Label();
		this.ozel_alan_2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label176 = new System.Windows.Forms.Label();
		this.ozel_alan_2_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label177 = new System.Windows.Forms.Label();
		this.label178 = new System.Windows.Forms.Label();
		this.ozel_alan_1_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label170 = new System.Windows.Forms.Label();
		this.ozel_alan_1_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label171 = new System.Windows.Forms.Label();
		this.ozel_alan_1_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label172 = new System.Windows.Forms.Label();
		this.label173 = new System.Windows.Forms.Label();
		this.xtraTabPage35 = new DevExpress.XtraTab.XtraTabPage();
		this.xtraTabControl8 = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage36 = new DevExpress.XtraTab.XtraTabPage();
		this.xtraTabControl9 = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage27 = new DevExpress.XtraTab.XtraTabPage();
		this.label3 = new System.Windows.Forms.Label();
		this.label694 = new System.Windows.Forms.Label();
		this.cari_Portal_Enabled = new DevExpress.XtraEditors.CheckEdit();
		this.cari_satis_isk_kod_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label670 = new System.Windows.Forms.Label();
		this.cari_satis_isk_kod_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label671 = new System.Windows.Forms.Label();
		this.cari_satis_isk_kod_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label672 = new System.Windows.Forms.Label();
		this.label673 = new System.Windows.Forms.Label();
		this.cari_sicil_no_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label665 = new System.Windows.Forms.Label();
		this.cari_sicil_no_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label666 = new System.Windows.Forms.Label();
		this.cari_sicil_no_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label667 = new System.Windows.Forms.Label();
		this.label668 = new System.Windows.Forms.Label();
		this.cari_CepTel_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label660 = new System.Windows.Forms.Label();
		this.cari_CepTel_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label661 = new System.Windows.Forms.Label();
		this.cari_CepTel_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label662 = new System.Windows.Forms.Label();
		this.label663 = new System.Windows.Forms.Label();
		this.cari_wwwadresi_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label655 = new System.Windows.Forms.Label();
		this.cari_wwwadresi_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label656 = new System.Windows.Forms.Label();
		this.cari_wwwadresi_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label657 = new System.Windows.Forms.Label();
		this.label658 = new System.Windows.Forms.Label();
		this.cari_Ana_cari_kodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label650 = new System.Windows.Forms.Label();
		this.cari_Ana_cari_kodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label651 = new System.Windows.Forms.Label();
		this.cari_Ana_cari_kodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label652 = new System.Windows.Forms.Label();
		this.label653 = new System.Windows.Forms.Label();
		this.cari_temsilci_kodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label414 = new System.Windows.Forms.Label();
		this.cari_temsilci_kodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label415 = new System.Windows.Forms.Label();
		this.cari_temsilci_kodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label416 = new System.Windows.Forms.Label();
		this.label648 = new System.Windows.Forms.Label();
		this.label551 = new System.Windows.Forms.Label();
		this.otomatik_hesap_acma_secenek_cari = new System.Windows.Forms.ComboBox();
		this.label549 = new System.Windows.Forms.Label();
		this.cari_vergi_dairesi_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label221 = new System.Windows.Forms.Label();
		this.xtraTabPage41 = new DevExpress.XtraTab.XtraTabPage();
		this.label643 = new System.Windows.Forms.Label();
		this.label645 = new System.Windows.Forms.Label();
		this.cari_muhartikeli_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label646 = new System.Windows.Forms.Label();
		this.cari_muhartikeli_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label647 = new System.Windows.Forms.Label();
		this.label639 = new System.Windows.Forms.Label();
		this.cari_muh_kod2_satis_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label640 = new System.Windows.Forms.Label();
		this.cari_muh_kod2_satis_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label641 = new System.Windows.Forms.Label();
		this.label642 = new System.Windows.Forms.Label();
		this.label628 = new System.Windows.Forms.Label();
		this.cari_muh_kod1_satis_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label629 = new System.Windows.Forms.Label();
		this.cari_muh_kod1_satis_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label630 = new System.Windows.Forms.Label();
		this.label631 = new System.Windows.Forms.Label();
		this.label614 = new System.Windows.Forms.Label();
		this.cari_muh_kod_satis_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label615 = new System.Windows.Forms.Label();
		this.cari_muh_kod_satis_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label616 = new System.Windows.Forms.Label();
		this.cari_muhartikeli = new DevExpress.XtraEditors.TextEdit();
		this.cari_muh_kod2_satis = new DevExpress.XtraEditors.TextEdit();
		this.cari_doviz_cinsi2 = new System.Windows.Forms.ComboBox();
		this.cari_muh_kod1_satis = new DevExpress.XtraEditors.TextEdit();
		this.cari_doviz_cinsi1 = new System.Windows.Forms.ComboBox();
		this.cari_muh_kod_satis = new DevExpress.XtraEditors.TextEdit();
		this.label411 = new System.Windows.Forms.Label();
		this.cari_doviz_cinsi = new System.Windows.Forms.ComboBox();
		this.xtraTabPage39 = new DevExpress.XtraTab.XtraTabPage();
		this.label292 = new System.Windows.Forms.Label();
		this.cari_il_bilgisi_plaka_kodu = new DevExpress.XtraEditors.CheckEdit();
		this.cari_telefon_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label231 = new System.Windows.Forms.Label();
		this.cari_posta_kodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label230 = new System.Windows.Forms.Label();
		this.cari_ulke_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label229 = new System.Windows.Forms.Label();
		this.label228 = new System.Windows.Forms.Label();
		this.cari_il_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label226 = new System.Windows.Forms.Label();
		this.cari_ilce_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label225 = new System.Windows.Forms.Label();
		this.cari_mahalle_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label224 = new System.Windows.Forms.Label();
		this.cari_adres_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label222 = new System.Windows.Forms.Label();
		this.xtraTabPage31 = new DevExpress.XtraTab.XtraTabPage();
		this.cari_VarsayilanCikisDepo_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label696 = new System.Windows.Forms.Label();
		this.cari_VarsayilanCikisDepo_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label697 = new System.Windows.Forms.Label();
		this.cari_VarsayilanCikisDepo_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label698 = new System.Windows.Forms.Label();
		this.label699 = new System.Windows.Forms.Label();
		this.cari_VarsayilanGirisDepo_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label701 = new System.Windows.Forms.Label();
		this.cari_VarsayilanGirisDepo_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label702 = new System.Windows.Forms.Label();
		this.cari_VarsayilanGirisDepo_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label703 = new System.Windows.Forms.Label();
		this.label704 = new System.Windows.Forms.Label();
		this.cari_Portal_PW_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label685 = new System.Windows.Forms.Label();
		this.cari_Portal_PW_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label686 = new System.Windows.Forms.Label();
		this.cari_Portal_PW_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label687 = new System.Windows.Forms.Label();
		this.label688 = new System.Windows.Forms.Label();
		this.cari_bolge_kodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label690 = new System.Windows.Forms.Label();
		this.cari_bolge_kodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label691 = new System.Windows.Forms.Label();
		this.cari_bolge_kodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label692 = new System.Windows.Forms.Label();
		this.label693 = new System.Windows.Forms.Label();
		this.cari_sektor_kodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label675 = new System.Windows.Forms.Label();
		this.cari_sektor_kodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label676 = new System.Windows.Forms.Label();
		this.cari_sektor_kodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label677 = new System.Windows.Forms.Label();
		this.label678 = new System.Windows.Forms.Label();
		this.cari_grup_kodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label680 = new System.Windows.Forms.Label();
		this.cari_grup_kodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label681 = new System.Windows.Forms.Label();
		this.cari_grup_kodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label682 = new System.Windows.Forms.Label();
		this.label683 = new System.Windows.Forms.Label();
		this.xtraTabPage17 = new DevExpress.XtraTab.XtraTabPage();
		this.pro_muh_kod_artikeli_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label600 = new System.Windows.Forms.Label();
		this.pro_muh_kod_artikeli_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label601 = new System.Windows.Forms.Label();
		this.pro_muh_kod_artikeli_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label602 = new System.Windows.Forms.Label();
		this.label603 = new System.Windows.Forms.Label();
		this.pro_aciklama_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label595 = new System.Windows.Forms.Label();
		this.pro_aciklama_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label596 = new System.Windows.Forms.Label();
		this.pro_aciklama_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label597 = new System.Windows.Forms.Label();
		this.label598 = new System.Windows.Forms.Label();
		this.pro_ana_projekodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label590 = new System.Windows.Forms.Label();
		this.pro_ana_projekodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label591 = new System.Windows.Forms.Label();
		this.pro_ana_projekodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label592 = new System.Windows.Forms.Label();
		this.label593 = new System.Windows.Forms.Label();
		this.pro_bolgekodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label585 = new System.Windows.Forms.Label();
		this.pro_bolgekodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label586 = new System.Windows.Forms.Label();
		this.pro_bolgekodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label587 = new System.Windows.Forms.Label();
		this.label588 = new System.Windows.Forms.Label();
		this.pro_sektorkodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label580 = new System.Windows.Forms.Label();
		this.pro_sektorkodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label581 = new System.Windows.Forms.Label();
		this.pro_sektorkodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label582 = new System.Windows.Forms.Label();
		this.label583 = new System.Windows.Forms.Label();
		this.pro_grupkodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label575 = new System.Windows.Forms.Label();
		this.pro_grupkodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label576 = new System.Windows.Forms.Label();
		this.pro_grupkodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label577 = new System.Windows.Forms.Label();
		this.label578 = new System.Windows.Forms.Label();
		this.pro_sormerkodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label570 = new System.Windows.Forms.Label();
		this.pro_sormerkodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label571 = new System.Windows.Forms.Label();
		this.pro_sormerkodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label572 = new System.Windows.Forms.Label();
		this.label573 = new System.Windows.Forms.Label();
		this.pro_musterikodu_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label565 = new System.Windows.Forms.Label();
		this.pro_musterikodu_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label566 = new System.Windows.Forms.Label();
		this.pro_musterikodu_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label567 = new System.Windows.Forms.Label();
		this.label568 = new System.Windows.Forms.Label();
		this.pro_adi_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label560 = new System.Windows.Forms.Label();
		this.pro_adi_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label561 = new System.Windows.Forms.Label();
		this.pro_adi_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label562 = new System.Windows.Forms.Label();
		this.label563 = new System.Windows.Forms.Label();
		this.label557 = new System.Windows.Forms.Label();
		this.otomatik_hesap_acma_secenek_proje = new System.Windows.Forms.ComboBox();
		this.xtraTabPage40 = new DevExpress.XtraTab.XtraTabPage();
		this.som_MuhArtikeli_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label605 = new System.Windows.Forms.Label();
		this.som_MuhArtikeli_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label606 = new System.Windows.Forms.Label();
		this.som_MuhArtikeli_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label607 = new System.Windows.Forms.Label();
		this.label608 = new System.Windows.Forms.Label();
		this.som_isim_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label610 = new System.Windows.Forms.Label();
		this.som_isim_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label611 = new System.Windows.Forms.Label();
		this.som_isim_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label612 = new System.Windows.Forms.Label();
		this.label613 = new System.Windows.Forms.Label();
		this.label558 = new System.Windows.Forms.Label();
		this.otomatik_hesap_acma_secenek_sorumluluk = new System.Windows.Forms.ComboBox();
		this.xtraTabPage20 = new DevExpress.XtraTab.XtraTabPage();
		this.xtraTabControl4 = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage21 = new DevExpress.XtraTab.XtraTabPage();
		this.label35 = new System.Windows.Forms.Label();
		this.satir_cinsi_veri_giden_havale = new DevExpress.XtraEditors.TextEdit();
		this.label34 = new System.Windows.Forms.Label();
		this.satir_cinsi_veri_gelen_havale = new DevExpress.XtraEditors.TextEdit();
		this.satir_sorumlulukmerkezi_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label24 = new System.Windows.Forms.Label();
		this.satir_sorumlulukmerkezi_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label26 = new System.Windows.Forms.Label();
		this.satir_sorumlulukmerkezi_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label30 = new System.Windows.Forms.Label();
		this.label33 = new System.Windows.Forms.Label();
		this.satir_vadesi_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label20 = new System.Windows.Forms.Label();
		this.satir_vadesi_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label21 = new System.Windows.Forms.Label();
		this.satir_vadesi_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label22 = new System.Windows.Forms.Label();
		this.label23 = new System.Windows.Forms.Label();
		this.satir_tutar_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.satir_tutar_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label5 = new System.Windows.Forms.Label();
		this.satir_tutar_islem_2_islem_tipi = new System.Windows.Forms.ComboBox();
		this.satir_tutar_islem_2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.satir_tutar_islem_1_islem_tipi = new System.Windows.Forms.ComboBox();
		this.satir_tutar_islem_1_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label275 = new System.Windows.Forms.Label();
		this.label47 = new System.Windows.Forms.Label();
		this.satir_tutar_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label184 = new System.Windows.Forms.Label();
		this.label18 = new System.Windows.Forms.Label();
		this.satir_cinsi_veri_kredi_karti = new DevExpress.XtraEditors.TextEdit();
		this.label14 = new System.Windows.Forms.Label();
		this.satir_cinsi_veri_senet = new DevExpress.XtraEditors.TextEdit();
		this.label303 = new System.Windows.Forms.Label();
		this.satir_aciklama_on_ek_deger = new DevExpress.XtraEditors.TextEdit();
		this.satir_aciklama_on_ek_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.satir_aciklama_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.label19 = new System.Windows.Forms.Label();
		this.satir_aciklama_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label127 = new System.Windows.Forms.Label();
		this.satir_aciklama_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label128 = new System.Windows.Forms.Label();
		this.label129 = new System.Windows.Forms.Label();
		this.label25 = new System.Windows.Forms.Label();
		this.label83 = new System.Windows.Forms.Label();
		this.satir_cinsi_veri_cek = new DevExpress.XtraEditors.TextEdit();
		this.label84 = new System.Windows.Forms.Label();
		this.satir_cinsi_veri_nakit = new DevExpress.XtraEditors.TextEdit();
		this.label86 = new System.Windows.Forms.Label();
		this.satir_cinsi_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label87 = new System.Windows.Forms.Label();
		this.satir_cinsi_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label88 = new System.Windows.Forms.Label();
		this.satir_cinsi_sabit_deger = new System.Windows.Forms.ComboBox();
		this.label81 = new System.Windows.Forms.Label();
		this.xtraTabPage7 = new DevExpress.XtraTab.XtraTabPage();
		this.satir_hesap_kodu_giden_havale_son_ek = new DevExpress.XtraEditors.TextEdit();
		this.label70 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_giden_havale_on_ek = new DevExpress.XtraEditors.TextEdit();
		this.label71 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_giden_havale_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.satir_hesap_kodu_giden_havale_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label72 = new System.Windows.Forms.Label();
		this.label73 = new System.Windows.Forms.Label();
		this.label74 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_giden_havale_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label75 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_gelen_havale_son_ek = new DevExpress.XtraEditors.TextEdit();
		this.label64 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_gelen_havale_on_ek = new DevExpress.XtraEditors.TextEdit();
		this.label65 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_gelen_havale_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.satir_hesap_kodu_gelen_havale_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label66 = new System.Windows.Forms.Label();
		this.label67 = new System.Windows.Forms.Label();
		this.label68 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_gelen_havale_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label69 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_kredi_karti_son_ek = new DevExpress.XtraEditors.TextEdit();
		this.label57 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_kredi_karti_on_ek = new DevExpress.XtraEditors.TextEdit();
		this.label58 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_kredi_karti_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.satir_hesap_kodu_kredi_karti_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label59 = new System.Windows.Forms.Label();
		this.label60 = new System.Windows.Forms.Label();
		this.label61 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_kredi_karti_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label63 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_senet_son_ek = new DevExpress.XtraEditors.TextEdit();
		this.label51 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_senet_on_ek = new DevExpress.XtraEditors.TextEdit();
		this.label52 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_senet_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.satir_hesap_kodu_senet_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label53 = new System.Windows.Forms.Label();
		this.label54 = new System.Windows.Forms.Label();
		this.label55 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_senet_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label56 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_cek_son_ek = new DevExpress.XtraEditors.TextEdit();
		this.label43 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_cek_on_ek = new DevExpress.XtraEditors.TextEdit();
		this.label44 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_cek_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.satir_hesap_kodu_cek_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label46 = new System.Windows.Forms.Label();
		this.label48 = new System.Windows.Forms.Label();
		this.label49 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_cek_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label50 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_nakit_son_ek = new DevExpress.XtraEditors.TextEdit();
		this.label42 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_nakit_on_ek = new DevExpress.XtraEditors.TextEdit();
		this.label40 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_nakit_sabit_deger = new DevExpress.XtraEditors.TextEdit();
		this.satir_hesap_kodu_nakit_sabit_kullan = new DevExpress.XtraEditors.CheckEdit();
		this.label89 = new System.Windows.Forms.Label();
		this.label82 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.satir_hesap_kodu_nakit_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label8 = new System.Windows.Forms.Label();
		this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
		this.xtraTabPage4 = new DevExpress.XtraTab.XtraTabPage();
		this.kriter_bool5_evet_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.kriter_bool4_evet_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.kriter_bool3_evet_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.kriter_bool2_evet_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label538 = new System.Windows.Forms.Label();
		this.kriter_bool5_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label539 = new System.Windows.Forms.Label();
		this.kriter_bool4_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label540 = new System.Windows.Forms.Label();
		this.kriter_bool3_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label541 = new System.Windows.Forms.Label();
		this.kriter_bool2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label542 = new System.Windows.Forms.Label();
		this.label544 = new System.Windows.Forms.Label();
		this.kriter_bool1_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label545 = new System.Windows.Forms.Label();
		this.label533 = new System.Windows.Forms.Label();
		this.kriter_bool1_evet_icin_deger = new DevExpress.XtraEditors.TextEdit();
		this.label525 = new System.Windows.Forms.Label();
		this.kriter_double5_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label526 = new System.Windows.Forms.Label();
		this.kriter_double4_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label527 = new System.Windows.Forms.Label();
		this.kriter_double3_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label528 = new System.Windows.Forms.Label();
		this.kriter_double2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label529 = new System.Windows.Forms.Label();
		this.label531 = new System.Windows.Forms.Label();
		this.kriter_double1_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label532 = new System.Windows.Forms.Label();
		this.label524 = new System.Windows.Forms.Label();
		this.kriter_metin5_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label523 = new System.Windows.Forms.Label();
		this.kriter_metin4_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label515 = new System.Windows.Forms.Label();
		this.kriter_metin3_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label508 = new System.Windows.Forms.Label();
		this.kriter_metin2_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label507 = new System.Windows.Forms.Label();
		this.label505 = new System.Windows.Forms.Label();
		this.kriter_metin1_baslangic = new DevExpress.XtraEditors.SpinEdit();
		this.label506 = new System.Windows.Forms.Label();
		this.label718 = new System.Windows.Forms.Label();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.degisiklikleriKaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sablonSilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sablonEkleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.dosyayaYazToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.dosyadanOkuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.tc_ayarlar).BeginInit();
		this.tc_ayarlar.SuspendLayout();
		this.xtraTabPage1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.Query.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DBName.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SqlPassword.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SqlUserName.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.KriterListesi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SqlServer.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SqlServerPort.Properties).BeginInit();
		this.xtraTabPage6.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dbc_no.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kayit_id_otomatik_ver.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kayit_id_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.sube_no.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.firma_no.Properties).BeginInit();
		this.xtraTabPage5.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl2).BeginInit();
		this.xtraTabControl2.SuspendLayout();
		this.xtraTabPage16.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.cari_banka_hesap_no_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_unvan_turkce_karakterleri_kaldir.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_unvan2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_unvan_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_eposta_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kod2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_tc_kimlik_no_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_vergi_no_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kod_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kod_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kod_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kodu_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kodu_on_ek_satis.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_arama_secenekleri.Properties).BeginInit();
		this.xtraTabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.evrak_sira_otomatik_ver.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.evrak_seri_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.evrak_seri_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.evrak_seri_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.evrak_tarihi_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.evrak_sira_baslangic.Properties).BeginInit();
		this.xtraTabPage8.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.belge_no_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.belge_tarihi_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kur_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.plasiyer_kodu_cariden_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.plasiyer_kodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.plasiyer_kodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.plasiyer_kodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.sor_mer_kodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.sor_mer_kodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.sor_mer_kodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.proje_kodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.proje_kodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.proje_kodu_sabit_kullan.Properties).BeginInit();
		this.xtraTabPage9.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.aciklama10_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama10_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama9_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama9_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama8_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama8_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama7_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama7_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama6_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama6_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama5_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama5_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama4_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama4_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama3_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama3_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama2_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama2_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama1_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama1_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama10_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama10_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama10_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama9_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama9_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama9_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama8_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama8_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama8_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama7_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama7_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama7_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama6_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama6_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama6_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama5_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama5_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama5_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama4_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama4_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama4_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama3_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama3_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama3_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama2_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama2_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama1_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama1_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama1_sabit_kullan.Properties).BeginInit();
		this.xtraTabPage10.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_3_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_3_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_3_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_2_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_2_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_1_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_1_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_1_sabit_kullan.Properties).BeginInit();
		this.xtraTabPage35.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl8).BeginInit();
		this.xtraTabControl8.SuspendLayout();
		this.xtraTabPage36.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl9).BeginInit();
		this.xtraTabControl9.SuspendLayout();
		this.xtraTabPage27.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.cari_Portal_Enabled.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_satis_isk_kod_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_satis_isk_kod_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_satis_isk_kod_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sicil_no_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sicil_no_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sicil_no_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_CepTel_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_CepTel_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_CepTel_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_wwwadresi_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_wwwadresi_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_wwwadresi_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Ana_cari_kodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Ana_cari_kodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Ana_cari_kodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_temsilci_kodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_temsilci_kodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_temsilci_kodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_vergi_dairesi_baslangic.Properties).BeginInit();
		this.xtraTabPage41.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.cari_muhartikeli_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muhartikeli_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod2_satis_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod2_satis_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod1_satis_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod1_satis_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod_satis_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod_satis_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muhartikeli.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod2_satis.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod1_satis.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod_satis.Properties).BeginInit();
		this.xtraTabPage39.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.cari_il_bilgisi_plaka_kodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_telefon_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_posta_kodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_ulke_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_il_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_ilce_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_mahalle_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_adres_baslangic.Properties).BeginInit();
		this.xtraTabPage31.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanCikisDepo_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanCikisDepo_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanCikisDepo_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanGirisDepo_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanGirisDepo_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanGirisDepo_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Portal_PW_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Portal_PW_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Portal_PW_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_bolge_kodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_bolge_kodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_bolge_kodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sektor_kodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sektor_kodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sektor_kodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_grup_kodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_grup_kodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cari_grup_kodu_sabit_kullan.Properties).BeginInit();
		this.xtraTabPage17.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pro_muh_kod_artikeli_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_muh_kod_artikeli_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_muh_kod_artikeli_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_aciklama_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_aciklama_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_aciklama_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_ana_projekodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_ana_projekodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_ana_projekodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_bolgekodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_bolgekodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_bolgekodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sektorkodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sektorkodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sektorkodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_grupkodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_grupkodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_grupkodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sormerkodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sormerkodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sormerkodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_musterikodu_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_musterikodu_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_musterikodu_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_adi_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_adi_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pro_adi_sabit_kullan.Properties).BeginInit();
		this.xtraTabPage40.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.som_MuhArtikeli_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.som_MuhArtikeli_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.som_MuhArtikeli_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.som_isim_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.som_isim_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.som_isim_sabit_kullan.Properties).BeginInit();
		this.xtraTabPage20.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl4).BeginInit();
		this.xtraTabControl4.SuspendLayout();
		this.xtraTabPage21.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_giden_havale.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_gelen_havale.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_sorumlulukmerkezi_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_sorumlulukmerkezi_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_sorumlulukmerkezi_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_vadesi_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_vadesi_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_vadesi_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_tutar_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_tutar_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_tutar_islem_2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_tutar_islem_1_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_tutar_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_kredi_karti.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_senet.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_aciklama_on_ek_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_aciklama_on_ek_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_aciklama_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_aciklama_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_aciklama_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_cek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_nakit.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_sabit_kullan.Properties).BeginInit();
		this.xtraTabPage7.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_giden_havale_son_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_giden_havale_on_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_giden_havale_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_giden_havale_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_giden_havale_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_gelen_havale_son_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_gelen_havale_on_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_gelen_havale_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_gelen_havale_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_gelen_havale_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_kredi_karti_son_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_kredi_karti_on_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_kredi_karti_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_kredi_karti_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_kredi_karti_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_senet_son_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_senet_on_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_senet_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_senet_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_senet_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_cek_son_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_cek_on_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_cek_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_cek_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_cek_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_nakit_son_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_nakit_on_ek.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_nakit_sabit_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_nakit_sabit_kullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_nakit_baslangic.Properties).BeginInit();
		this.xtraTabPage4.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool5_evet_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool4_evet_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool3_evet_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool2_evet_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool5_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool4_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool3_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool1_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool1_evet_icin_deger.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_double5_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_double4_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_double3_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_double2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_double1_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_metin5_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_metin4_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_metin3_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_metin2_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_metin1_baslangic.Properties).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.lb_kullanicilar.Location = new System.Drawing.Point(11, 57);
		this.lb_kullanicilar.Name = "lb_kullanicilar";
		this.lb_kullanicilar.Size = new System.Drawing.Size(153, 597);
		this.lb_kullanicilar.TabIndex = 94;
		this.lb_kullanicilar.SelectedValueChanged += new System.EventHandler(lb_kullanicilar_SelectedValueChanged);
		this.te_kullanici_adi.Enabled = false;
		this.te_kullanici_adi.Location = new System.Drawing.Point(140, 17);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.te_kullanici_adi.Properties.Appearance.Options.UseForeColor = true;
		this.te_kullanici_adi.Size = new System.Drawing.Size(214, 20);
		this.te_kullanici_adi.TabIndex = 100;
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(17, 20);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(117, 13);
		this.labelControl1.TabIndex = 99;
		this.labelControl1.Text = "Genel parametre adı :";
		this.tc_ayarlar.Enabled = false;
		this.tc_ayarlar.Location = new System.Drawing.Point(172, 34);
		this.tc_ayarlar.Name = "tc_ayarlar";
		this.tc_ayarlar.SelectedTabPage = this.xtraTabPage1;
		this.tc_ayarlar.Size = new System.Drawing.Size(793, 625);
		this.tc_ayarlar.TabIndex = 101;
		this.tc_ayarlar.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[10] { this.xtraTabPage1, this.xtraTabPage6, this.xtraTabPage5, this.xtraTabPage3, this.xtraTabPage8, this.xtraTabPage9, this.xtraTabPage10, this.xtraTabPage35, this.xtraTabPage20, this.xtraTabPage4 });
		this.xtraTabPage1.Controls.Add(this.labelControl14);
		this.xtraTabPage1.Controls.Add(this.label17);
		this.xtraTabPage1.Controls.Add(this.Query);
		this.xtraTabPage1.Controls.Add(this.label16);
		this.xtraTabPage1.Controls.Add(this.DBName);
		this.xtraTabPage1.Controls.Add(this.label15);
		this.xtraTabPage1.Controls.Add(this.SqlPassword);
		this.xtraTabPage1.Controls.Add(this.label10);
		this.xtraTabPage1.Controls.Add(this.SqlUserName);
		this.xtraTabPage1.Controls.Add(this.label9);
		this.xtraTabPage1.Controls.Add(this.label2);
		this.xtraTabPage1.Controls.Add(this.KriterListesi);
		this.xtraTabPage1.Controls.Add(this.label536);
		this.xtraTabPage1.Controls.Add(this.SqlServer);
		this.xtraTabPage1.Controls.Add(this.SqlServerPort);
		this.xtraTabPage1.Controls.Add(this.te_kullanici_adi);
		this.xtraTabPage1.Controls.Add(this.labelControl1);
		this.xtraTabPage1.Name = "xtraTabPage1";
		this.xtraTabPage1.Size = new System.Drawing.Size(787, 597);
		this.xtraTabPage1.Text = "Ayarlar";
		this.labelControl14.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl14.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl14.Location = new System.Drawing.Point(360, 124);
		this.labelControl14.Name = "labelControl14";
		this.labelControl14.Size = new System.Drawing.Size(313, 20);
		this.labelControl14.TabIndex = 422;
		this.labelControl14.Text = "Boş bırakılması durumunda Windows Authentication ile bağlanılır";
		this.label17.Location = new System.Drawing.Point(7, 202);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(127, 19);
		this.label17.TabIndex = 267;
		this.label17.Text = "Sql sorgusu :";
		this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.Query.Location = new System.Drawing.Point(140, 202);
		this.Query.Name = "Query";
		this.Query.Size = new System.Drawing.Size(642, 20);
		this.Query.TabIndex = 266;
		this.Query.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.Query.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label16.Location = new System.Drawing.Point(7, 176);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(127, 19);
		this.label16.TabIndex = 265;
		this.label16.Text = "Veritabanı adı :";
		this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DBName.Location = new System.Drawing.Point(140, 176);
		this.DBName.Name = "DBName";
		this.DBName.Size = new System.Drawing.Size(214, 20);
		this.DBName.TabIndex = 264;
		this.DBName.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DBName.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label15.Location = new System.Drawing.Point(7, 150);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(127, 19);
		this.label15.TabIndex = 263;
		this.label15.Text = "Şifre :";
		this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.SqlPassword.Location = new System.Drawing.Point(140, 150);
		this.SqlPassword.Name = "SqlPassword";
		this.SqlPassword.Size = new System.Drawing.Size(214, 20);
		this.SqlPassword.TabIndex = 262;
		this.SqlPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.SqlPassword.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label10.Location = new System.Drawing.Point(7, 124);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(127, 19);
		this.label10.TabIndex = 261;
		this.label10.Text = "Kullanıcı adı :";
		this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.SqlUserName.Location = new System.Drawing.Point(140, 124);
		this.SqlUserName.Name = "SqlUserName";
		this.SqlUserName.Size = new System.Drawing.Size(214, 20);
		this.SqlUserName.TabIndex = 260;
		this.SqlUserName.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.SqlUserName.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label9.Location = new System.Drawing.Point(7, 98);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(127, 19);
		this.label9.TabIndex = 259;
		this.label9.Text = "Sql Server port :";
		this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label2.Location = new System.Drawing.Point(7, 73);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(127, 19);
		this.label2.TabIndex = 258;
		this.label2.Text = "Sql Server :";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.KriterListesi.CheckOnClick = true;
		this.KriterListesi.Location = new System.Drawing.Point(140, 257);
		this.KriterListesi.Name = "KriterListesi";
		this.KriterListesi.Size = new System.Drawing.Size(214, 154);
		this.KriterListesi.TabIndex = 105;
		this.KriterListesi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.KriterListesi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label536.AutoSize = true;
		this.label536.Location = new System.Drawing.Point(18, 260);
		this.label536.Name = "label536";
		this.label536.Size = new System.Drawing.Size(116, 13);
		this.label536.TabIndex = 106;
		this.label536.Text = "Uygulanacak Kriterler :";
		this.SqlServer.Location = new System.Drawing.Point(140, 72);
		this.SqlServer.Name = "SqlServer";
		this.SqlServer.Size = new System.Drawing.Size(214, 20);
		this.SqlServer.TabIndex = 101;
		this.SqlServer.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.SqlServer.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.SqlServerPort.Location = new System.Drawing.Point(140, 98);
		this.SqlServerPort.Name = "SqlServerPort";
		this.SqlServerPort.Size = new System.Drawing.Size(214, 20);
		this.SqlServerPort.TabIndex = 102;
		this.SqlServerPort.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.SqlServerPort.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.xtraTabPage6.Controls.Add(this.dbc_no);
		this.xtraTabPage6.Controls.Add(this.label76);
		this.xtraTabPage6.Controls.Add(this.kayit_id_otomatik_ver);
		this.xtraTabPage6.Controls.Add(this.label554);
		this.xtraTabPage6.Controls.Add(this.kayit_id_baslangic);
		this.xtraTabPage6.Controls.Add(this.label555);
		this.xtraTabPage6.Controls.Add(this.label550);
		this.xtraTabPage6.Controls.Add(this.label264);
		this.xtraTabPage6.Controls.Add(this.sube_no);
		this.xtraTabPage6.Controls.Add(this.firma_no);
		this.xtraTabPage6.Controls.Add(this.label211);
		this.xtraTabPage6.Controls.Add(this.label210);
		this.xtraTabPage6.Name = "xtraTabPage6";
		this.xtraTabPage6.Size = new System.Drawing.Size(787, 597);
		this.xtraTabPage6.Text = "Genel";
		this.dbc_no.EditValue = new decimal(new int[4]);
		this.dbc_no.Location = new System.Drawing.Point(91, 456);
		this.dbc_no.Name = "dbc_no";
		this.dbc_no.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.dbc_no.Properties.IsFloatValue = false;
		this.dbc_no.Properties.Mask.EditMask = "N00";
		this.dbc_no.Size = new System.Drawing.Size(96, 20);
		this.dbc_no.TabIndex = 414;
		this.dbc_no.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.dbc_no.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label76.Location = new System.Drawing.Point(21, 455);
		this.label76.Name = "label76";
		this.label76.Size = new System.Drawing.Size(64, 19);
		this.label76.TabIndex = 413;
		this.label76.Text = "DBC no :";
		this.label76.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.kayit_id_otomatik_ver.Location = new System.Drawing.Point(108, 286);
		this.kayit_id_otomatik_ver.Name = "kayit_id_otomatik_ver";
		this.kayit_id_otomatik_ver.Properties.Caption = "KayıtID otomatik ver";
		this.kayit_id_otomatik_ver.Size = new System.Drawing.Size(189, 19);
		this.kayit_id_otomatik_ver.TabIndex = 410;
		this.kayit_id_otomatik_ver.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kayit_id_otomatik_ver.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label554.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label554.Location = new System.Drawing.Point(141, 308);
		this.label554.Name = "label554";
		this.label554.Size = new System.Drawing.Size(96, 19);
		this.label554.TabIndex = 408;
		this.label554.Text = "Sıra";
		this.label554.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.kayit_id_baslangic.EditValue = new decimal(new int[4]);
		this.kayit_id_baslangic.Location = new System.Drawing.Point(141, 329);
		this.kayit_id_baslangic.Name = "kayit_id_baslangic";
		this.kayit_id_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kayit_id_baslangic.Properties.IsFloatValue = false;
		this.kayit_id_baslangic.Properties.Mask.EditMask = "N00";
		this.kayit_id_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kayit_id_baslangic.TabIndex = 406;
		this.kayit_id_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kayit_id_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label555.Location = new System.Drawing.Point(15, 329);
		this.label555.Name = "label555";
		this.label555.Size = new System.Drawing.Size(120, 19);
		this.label555.TabIndex = 405;
		this.label555.Text = "KayıtID :";
		this.label555.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label550.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label550.Location = new System.Drawing.Point(17, 256);
		this.label550.Name = "label550";
		this.label550.Size = new System.Drawing.Size(234, 19);
		this.label550.TabIndex = 404;
		this.label550.Text = "KayıtID";
		this.label550.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label264.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label264.Location = new System.Drawing.Point(17, 375);
		this.label264.Name = "label264";
		this.label264.Size = new System.Drawing.Size(234, 19);
		this.label264.TabIndex = 390;
		this.label264.Text = "Firma / Şube no";
		this.label264.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.sube_no.EditValue = new decimal(new int[4]);
		this.sube_no.Location = new System.Drawing.Point(91, 430);
		this.sube_no.Name = "sube_no";
		this.sube_no.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.sube_no.Properties.IsFloatValue = false;
		this.sube_no.Properties.Mask.EditMask = "N00";
		this.sube_no.Size = new System.Drawing.Size(96, 20);
		this.sube_no.TabIndex = 389;
		this.sube_no.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.sube_no.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.firma_no.EditValue = new decimal(new int[4]);
		this.firma_no.Location = new System.Drawing.Point(91, 404);
		this.firma_no.Name = "firma_no";
		this.firma_no.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.firma_no.Properties.IsFloatValue = false;
		this.firma_no.Properties.Mask.EditMask = "N00";
		this.firma_no.Size = new System.Drawing.Size(96, 20);
		this.firma_no.TabIndex = 388;
		this.firma_no.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.firma_no.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label211.Location = new System.Drawing.Point(21, 433);
		this.label211.Name = "label211";
		this.label211.Size = new System.Drawing.Size(64, 19);
		this.label211.TabIndex = 387;
		this.label211.Text = "Şube no :";
		this.label211.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label210.Location = new System.Drawing.Point(21, 404);
		this.label210.Name = "label210";
		this.label210.Size = new System.Drawing.Size(64, 19);
		this.label210.TabIndex = 386;
		this.label210.Text = "Firma no :";
		this.label210.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.xtraTabPage5.Controls.Add(this.xtraTabControl2);
		this.xtraTabPage5.Name = "xtraTabPage5";
		this.xtraTabPage5.Size = new System.Drawing.Size(787, 597);
		this.xtraTabPage5.Text = "Cari";
		this.xtraTabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.xtraTabControl2.Location = new System.Drawing.Point(0, 0);
		this.xtraTabControl2.Name = "xtraTabControl2";
		this.xtraTabControl2.SelectedTabPage = this.xtraTabPage16;
		this.xtraTabControl2.Size = new System.Drawing.Size(787, 597);
		this.xtraTabControl2.TabIndex = 396;
		this.xtraTabControl2.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[1] { this.xtraTabPage16 });
		this.xtraTabPage16.Controls.Add(this.label13);
		this.xtraTabPage16.Controls.Add(this.label548);
		this.xtraTabPage16.Controls.Add(this.cari_banka_hesap_no_baslangic);
		this.xtraTabPage16.Controls.Add(this.label233);
		this.xtraTabPage16.Controls.Add(this.cari_unvan_turkce_karakterleri_kaldir);
		this.xtraTabPage16.Controls.Add(this.cari_unvan2_baslangic);
		this.xtraTabPage16.Controls.Add(this.label223);
		this.xtraTabPage16.Controls.Add(this.label217);
		this.xtraTabPage16.Controls.Add(this.cari_unvan_baslangic);
		this.xtraTabPage16.Controls.Add(this.label218);
		this.xtraTabPage16.Controls.Add(this.label215);
		this.xtraTabPage16.Controls.Add(this.cari_eposta_baslangic);
		this.xtraTabPage16.Controls.Add(this.label232);
		this.xtraTabPage16.Controls.Add(this.label537);
		this.xtraTabPage16.Controls.Add(this.label546);
		this.xtraTabPage16.Controls.Add(this.labelControl12);
		this.xtraTabPage16.Controls.Add(this.label422);
		this.xtraTabPage16.Controls.Add(this.cari_kod2_baslangic);
		this.xtraTabPage16.Controls.Add(this.labelControl4);
		this.xtraTabPage16.Controls.Add(this.label291);
		this.xtraTabPage16.Controls.Add(this.label289);
		this.xtraTabPage16.Controls.Add(this.cari_tc_kimlik_no_baslangic);
		this.xtraTabPage16.Controls.Add(this.label220);
		this.xtraTabPage16.Controls.Add(this.cari_vergi_no_baslangic);
		this.xtraTabPage16.Controls.Add(this.label219);
		this.xtraTabPage16.Controls.Add(this.label94);
		this.xtraTabPage16.Controls.Add(this.label93);
		this.xtraTabPage16.Controls.Add(this.cari_kod_sabit_kullan);
		this.xtraTabPage16.Controls.Add(this.label92);
		this.xtraTabPage16.Controls.Add(this.cari_kod_baslangic);
		this.xtraTabPage16.Controls.Add(this.label91);
		this.xtraTabPage16.Controls.Add(this.cari_kod_sabit_deger);
		this.xtraTabPage16.Controls.Add(this.label212);
		this.xtraTabPage16.Controls.Add(this.cari_kodu_on_ek_kullan);
		this.xtraTabPage16.Controls.Add(this.cari_kodu_on_ek_satis);
		this.xtraTabPage16.Controls.Add(this.label213);
		this.xtraTabPage16.Controls.Add(this.label214);
		this.xtraTabPage16.Controls.Add(this.cari_arama_secenekleri);
		this.xtraTabPage16.Controls.Add(this.labelControl2);
		this.xtraTabPage16.Controls.Add(this.labelControl3);
		this.xtraTabPage16.Name = "xtraTabPage16";
		this.xtraTabPage16.Size = new System.Drawing.Size(781, 569);
		this.xtraTabPage16.Text = "Cari hesap bilgileri";
		this.label13.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label13.Location = new System.Drawing.Point(490, 354);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(96, 19);
		this.label13.TabIndex = 459;
		this.label13.Text = "Sıra";
		this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label548.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label548.Location = new System.Drawing.Point(379, 335);
		this.label548.Name = "label548";
		this.label548.Size = new System.Drawing.Size(234, 19);
		this.label548.TabIndex = 458;
		this.label548.Text = "Banka hesap no";
		this.label548.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_banka_hesap_no_baslangic.EditValue = new decimal(new int[4]);
		this.cari_banka_hesap_no_baslangic.Location = new System.Drawing.Point(490, 376);
		this.cari_banka_hesap_no_baslangic.Name = "cari_banka_hesap_no_baslangic";
		this.cari_banka_hesap_no_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_banka_hesap_no_baslangic.Properties.IsFloatValue = false;
		this.cari_banka_hesap_no_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_banka_hesap_no_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_banka_hesap_no_baslangic.TabIndex = 456;
		this.cari_banka_hesap_no_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_banka_hesap_no_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label233.Location = new System.Drawing.Point(364, 376);
		this.label233.Name = "label233";
		this.label233.Size = new System.Drawing.Size(120, 19);
		this.label233.TabIndex = 455;
		this.label233.Text = "Banka hesap no :";
		this.label233.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_unvan_turkce_karakterleri_kaldir.Location = new System.Drawing.Point(488, 296);
		this.cari_unvan_turkce_karakterleri_kaldir.Name = "cari_unvan_turkce_karakterleri_kaldir";
		this.cari_unvan_turkce_karakterleri_kaldir.Properties.Caption = "Cari ünvanındaki Türkçe karakterleri kaldır";
		this.cari_unvan_turkce_karakterleri_kaldir.Size = new System.Drawing.Size(233, 19);
		this.cari_unvan_turkce_karakterleri_kaldir.TabIndex = 454;
		this.cari_unvan_turkce_karakterleri_kaldir.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_unvan_turkce_karakterleri_kaldir.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_unvan2_baslangic.EditValue = new decimal(new int[4]);
		this.cari_unvan2_baslangic.Location = new System.Drawing.Point(490, 269);
		this.cari_unvan2_baslangic.Name = "cari_unvan2_baslangic";
		this.cari_unvan2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_unvan2_baslangic.Properties.IsFloatValue = false;
		this.cari_unvan2_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_unvan2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_unvan2_baslangic.TabIndex = 452;
		this.cari_unvan2_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_unvan2_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label223.Location = new System.Drawing.Point(364, 269);
		this.label223.Name = "label223";
		this.label223.Size = new System.Drawing.Size(120, 19);
		this.label223.TabIndex = 451;
		this.label223.Text = "Cari ünvan 2 :";
		this.label223.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label217.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label217.Location = new System.Drawing.Point(490, 222);
		this.label217.Name = "label217";
		this.label217.Size = new System.Drawing.Size(96, 19);
		this.label217.TabIndex = 449;
		this.label217.Text = "Sıra";
		this.label217.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_unvan_baslangic.EditValue = new decimal(new int[4]);
		this.cari_unvan_baslangic.Location = new System.Drawing.Point(490, 243);
		this.cari_unvan_baslangic.Name = "cari_unvan_baslangic";
		this.cari_unvan_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_unvan_baslangic.Properties.IsFloatValue = false;
		this.cari_unvan_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_unvan_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_unvan_baslangic.TabIndex = 447;
		this.cari_unvan_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_unvan_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label218.Location = new System.Drawing.Point(364, 243);
		this.label218.Name = "label218";
		this.label218.Size = new System.Drawing.Size(120, 19);
		this.label218.TabIndex = 446;
		this.label218.Text = "Cari ünvan :";
		this.label218.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label215.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label215.Location = new System.Drawing.Point(379, 203);
		this.label215.Name = "label215";
		this.label215.Size = new System.Drawing.Size(129, 19);
		this.label215.TabIndex = 445;
		this.label215.Text = "Cari ünvan";
		this.label215.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_eposta_baslangic.EditValue = new decimal(new int[4]);
		this.cari_eposta_baslangic.Location = new System.Drawing.Point(139, 376);
		this.cari_eposta_baslangic.Name = "cari_eposta_baslangic";
		this.cari_eposta_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_eposta_baslangic.Properties.IsFloatValue = false;
		this.cari_eposta_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_eposta_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_eposta_baslangic.TabIndex = 442;
		this.cari_eposta_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_eposta_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label232.Location = new System.Drawing.Point(64, 376);
		this.label232.Name = "label232";
		this.label232.Size = new System.Drawing.Size(69, 19);
		this.label232.TabIndex = 441;
		this.label232.Text = "E-posta :";
		this.label232.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label537.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label537.Location = new System.Drawing.Point(13, 335);
		this.label537.Name = "label537";
		this.label537.Size = new System.Drawing.Size(234, 19);
		this.label537.TabIndex = 430;
		this.label537.Text = "E-Posta";
		this.label537.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label546.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label546.Location = new System.Drawing.Point(139, 354);
		this.label546.Name = "label546";
		this.label546.Size = new System.Drawing.Size(96, 19);
		this.label546.TabIndex = 428;
		this.label546.Text = "Sıra";
		this.label546.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelControl12.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl12.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl12.Location = new System.Drawing.Point(532, 82);
		this.labelControl12.Name = "labelControl12";
		this.labelControl12.Size = new System.Drawing.Size(152, 20);
		this.labelControl12.TabIndex = 421;
		this.labelControl12.Text = "Cari kod bulunamazsa bakılır.";
		this.label422.Location = new System.Drawing.Point(304, 82);
		this.label422.Name = "label422";
		this.label422.Size = new System.Drawing.Size(120, 19);
		this.label422.TabIndex = 418;
		this.label422.Text = "2. Cari kodu değeri :";
		this.label422.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_kod2_baslangic.EditValue = new decimal(new int[4]);
		this.cari_kod2_baslangic.Location = new System.Drawing.Point(430, 82);
		this.cari_kod2_baslangic.Name = "cari_kod2_baslangic";
		this.cari_kod2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_kod2_baslangic.Properties.IsFloatValue = false;
		this.cari_kod2_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_kod2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_kod2_baslangic.TabIndex = 419;
		this.cari_kod2_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_kod2_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(139, 298);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(152, 20);
		this.labelControl4.TabIndex = 417;
		this.labelControl4.Text = "Vergi no bulunamazsa bakılır.";
		this.label291.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label291.Location = new System.Drawing.Point(13, 205);
		this.label291.Name = "label291";
		this.label291.Size = new System.Drawing.Size(234, 19);
		this.label291.TabIndex = 416;
		this.label291.Text = "Vergi/Tc kimlik no";
		this.label291.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label289.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label289.Location = new System.Drawing.Point(139, 224);
		this.label289.Name = "label289";
		this.label289.Size = new System.Drawing.Size(96, 19);
		this.label289.TabIndex = 414;
		this.label289.Text = "Sıra";
		this.label289.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_tc_kimlik_no_baslangic.EditValue = new decimal(new int[4]);
		this.cari_tc_kimlik_no_baslangic.Location = new System.Drawing.Point(139, 272);
		this.cari_tc_kimlik_no_baslangic.Name = "cari_tc_kimlik_no_baslangic";
		this.cari_tc_kimlik_no_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_tc_kimlik_no_baslangic.Properties.IsFloatValue = false;
		this.cari_tc_kimlik_no_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_tc_kimlik_no_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_tc_kimlik_no_baslangic.TabIndex = 412;
		this.cari_tc_kimlik_no_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_tc_kimlik_no_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label220.Location = new System.Drawing.Point(13, 272);
		this.label220.Name = "label220";
		this.label220.Size = new System.Drawing.Size(120, 19);
		this.label220.TabIndex = 411;
		this.label220.Text = "Tc kimlik no :";
		this.label220.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_vergi_no_baslangic.EditValue = new decimal(new int[4]);
		this.cari_vergi_no_baslangic.Location = new System.Drawing.Point(139, 246);
		this.cari_vergi_no_baslangic.Name = "cari_vergi_no_baslangic";
		this.cari_vergi_no_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_vergi_no_baslangic.Properties.IsFloatValue = false;
		this.cari_vergi_no_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_vergi_no_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_vergi_no_baslangic.TabIndex = 409;
		this.cari_vergi_no_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_vergi_no_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label219.Location = new System.Drawing.Point(13, 246);
		this.label219.Name = "label219";
		this.label219.Size = new System.Drawing.Size(120, 19);
		this.label219.TabIndex = 408;
		this.label219.Text = "Vergi no :";
		this.label219.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label94.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label94.Location = new System.Drawing.Point(13, 6);
		this.label94.Name = "label94";
		this.label94.Size = new System.Drawing.Size(234, 19);
		this.label94.TabIndex = 284;
		this.label94.Text = "Cari kodu";
		this.label94.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label93.Location = new System.Drawing.Point(6, 59);
		this.label93.Name = "label93";
		this.label93.Size = new System.Drawing.Size(127, 19);
		this.label93.TabIndex = 286;
		this.label93.Text = "Cari kodu sabit değer :";
		this.label93.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_kod_sabit_kullan.Location = new System.Drawing.Point(137, 31);
		this.cari_kod_sabit_kullan.Name = "cari_kod_sabit_kullan";
		this.cari_kod_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_kod_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.cari_kod_sabit_kullan.TabIndex = 287;
		this.cari_kod_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_kod_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label92.Location = new System.Drawing.Point(304, 56);
		this.label92.Name = "label92";
		this.label92.Size = new System.Drawing.Size(120, 19);
		this.label92.TabIndex = 288;
		this.label92.Text = "Cari kodu değeri :";
		this.label92.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_kod_baslangic.EditValue = new decimal(new int[4]);
		this.cari_kod_baslangic.Location = new System.Drawing.Point(430, 56);
		this.cari_kod_baslangic.Name = "cari_kod_baslangic";
		this.cari_kod_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_kod_baslangic.Properties.IsFloatValue = false;
		this.cari_kod_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_kod_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_kod_baslangic.TabIndex = 289;
		this.cari_kod_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_kod_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label91.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label91.Location = new System.Drawing.Point(430, 35);
		this.label91.Name = "label91";
		this.label91.Size = new System.Drawing.Size(96, 19);
		this.label91.TabIndex = 291;
		this.label91.Text = "Sıra";
		this.label91.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_kod_sabit_deger.Location = new System.Drawing.Point(139, 59);
		this.cari_kod_sabit_deger.Name = "cari_kod_sabit_deger";
		this.cari_kod_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_kod_sabit_deger.TabIndex = 293;
		this.cari_kod_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_kod_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label212.Location = new System.Drawing.Point(6, 127);
		this.label212.Name = "label212";
		this.label212.Size = new System.Drawing.Size(127, 19);
		this.label212.TabIndex = 294;
		this.label212.Text = "Cari kodu ön ek :";
		this.label212.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_kodu_on_ek_kullan.Location = new System.Drawing.Point(137, 99);
		this.cari_kodu_on_ek_kullan.Name = "cari_kodu_on_ek_kullan";
		this.cari_kodu_on_ek_kullan.Properties.Caption = "Cari kod ön ek kullan";
		this.cari_kodu_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.cari_kodu_on_ek_kullan.TabIndex = 295;
		this.cari_kodu_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_kodu_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_kodu_on_ek_satis.Location = new System.Drawing.Point(139, 127);
		this.cari_kodu_on_ek_satis.Name = "cari_kodu_on_ek_satis";
		this.cari_kodu_on_ek_satis.Size = new System.Drawing.Size(156, 20);
		this.cari_kodu_on_ek_satis.TabIndex = 296;
		this.cari_kodu_on_ek_satis.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_kodu_on_ek_satis.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label213.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label213.Location = new System.Drawing.Point(13, 460);
		this.label213.Name = "label213";
		this.label213.Size = new System.Drawing.Size(234, 19);
		this.label213.TabIndex = 297;
		this.label213.Text = "Cari arama seçenekleri";
		this.label213.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label214.Location = new System.Drawing.Point(6, 488);
		this.label214.Name = "label214";
		this.label214.Size = new System.Drawing.Size(127, 19);
		this.label214.TabIndex = 298;
		this.label214.Text = "Cari arama seçenekleri :";
		this.label214.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_arama_secenekleri.Location = new System.Drawing.Point(139, 488);
		this.cari_arama_secenekleri.Name = "cari_arama_secenekleri";
		this.cari_arama_secenekleri.Size = new System.Drawing.Size(156, 20);
		this.cari_arama_secenekleri.TabIndex = 299;
		this.cari_arama_secenekleri.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_arama_secenekleri.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(301, 488);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(406, 20);
		this.labelControl2.TabIndex = 342;
		this.labelControl2.Text = "Mikro'da cari taraması yapılırken bakılacak alanlar. Birden fazla alanı virgülle ayırınız.";
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(139, 514);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(369, 20);
		this.labelControl3.TabIndex = 343;
		this.labelControl3.Text = "1: Cari kodu 2: Veri/Tc no 3: EMail 4: Banka hesap no";
		this.xtraTabPage3.Controls.Add(this.evrak_sira_otomatik_ver);
		this.xtraTabPage3.Controls.Add(this.label207);
		this.xtraTabPage3.Controls.Add(this.label205);
		this.xtraTabPage3.Controls.Add(this.evrak_seri_sabit_deger);
		this.xtraTabPage3.Controls.Add(this.label197);
		this.xtraTabPage3.Controls.Add(this.evrak_seri_baslangic);
		this.xtraTabPage3.Controls.Add(this.label198);
		this.xtraTabPage3.Controls.Add(this.evrak_seri_sabit_kullan);
		this.xtraTabPage3.Controls.Add(this.label199);
		this.xtraTabPage3.Controls.Add(this.label45);
		this.xtraTabPage3.Controls.Add(this.label41);
		this.xtraTabPage3.Controls.Add(this.label1);
		this.xtraTabPage3.Controls.Add(this.evrak_tarihi_baslangic);
		this.xtraTabPage3.Controls.Add(this.label29);
		this.xtraTabPage3.Controls.Add(this.evrak_sira_baslangic);
		this.xtraTabPage3.Controls.Add(this.label6);
		this.xtraTabPage3.Name = "xtraTabPage3";
		this.xtraTabPage3.Size = new System.Drawing.Size(787, 597);
		this.xtraTabPage3.Text = "Seri-Sıra-Tarih";
		this.evrak_sira_otomatik_ver.Location = new System.Drawing.Point(144, 259);
		this.evrak_sira_otomatik_ver.Name = "evrak_sira_otomatik_ver";
		this.evrak_sira_otomatik_ver.Properties.Caption = "Evrak sıra otomatik ver";
		this.evrak_sira_otomatik_ver.Size = new System.Drawing.Size(189, 19);
		this.evrak_sira_otomatik_ver.TabIndex = 411;
		this.evrak_sira_otomatik_ver.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.evrak_sira_otomatik_ver.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label207.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label207.Location = new System.Drawing.Point(143, 281);
		this.label207.Name = "label207";
		this.label207.Size = new System.Drawing.Size(96, 19);
		this.label207.TabIndex = 315;
		this.label207.Text = "Sıra";
		this.label207.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label205.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label205.Location = new System.Drawing.Point(143, 63);
		this.label205.Name = "label205";
		this.label205.Size = new System.Drawing.Size(96, 19);
		this.label205.TabIndex = 313;
		this.label205.Text = "Sıra";
		this.label205.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.evrak_seri_sabit_deger.Location = new System.Drawing.Point(245, 175);
		this.evrak_seri_sabit_deger.Name = "evrak_seri_sabit_deger";
		this.evrak_seri_sabit_deger.Size = new System.Drawing.Size(54, 20);
		this.evrak_seri_sabit_deger.TabIndex = 301;
		this.evrak_seri_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.evrak_seri_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label197.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label197.Location = new System.Drawing.Point(508, 154);
		this.label197.Name = "label197";
		this.label197.Size = new System.Drawing.Size(96, 19);
		this.label197.TabIndex = 299;
		this.label197.Text = "Sıra";
		this.label197.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.evrak_seri_baslangic.EditValue = new decimal(new int[4]);
		this.evrak_seri_baslangic.Location = new System.Drawing.Point(508, 175);
		this.evrak_seri_baslangic.Name = "evrak_seri_baslangic";
		this.evrak_seri_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.evrak_seri_baslangic.Properties.IsFloatValue = false;
		this.evrak_seri_baslangic.Properties.Mask.EditMask = "N00";
		this.evrak_seri_baslangic.Size = new System.Drawing.Size(96, 20);
		this.evrak_seri_baslangic.TabIndex = 297;
		this.evrak_seri_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.evrak_seri_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label198.Location = new System.Drawing.Point(382, 175);
		this.label198.Name = "label198";
		this.label198.Size = new System.Drawing.Size(120, 19);
		this.label198.TabIndex = 296;
		this.label198.Text = "Evrak seri değeri :";
		this.label198.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.evrak_seri_sabit_kullan.Location = new System.Drawing.Point(141, 147);
		this.evrak_seri_sabit_kullan.Name = "evrak_seri_sabit_kullan";
		this.evrak_seri_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.evrak_seri_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.evrak_seri_sabit_kullan.TabIndex = 295;
		this.evrak_seri_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.evrak_seri_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label199.Location = new System.Drawing.Point(40, 176);
		this.label199.Name = "label199";
		this.label199.Size = new System.Drawing.Size(199, 19);
		this.label199.TabIndex = 294;
		this.label199.Text = "Evrak seri sabit değer :";
		this.label199.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label45.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label45.Location = new System.Drawing.Point(37, 235);
		this.label45.Name = "label45";
		this.label45.Size = new System.Drawing.Size(234, 19);
		this.label45.TabIndex = 263;
		this.label45.Text = "Evrak sıra";
		this.label45.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label41.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label41.Location = new System.Drawing.Point(37, 122);
		this.label41.Name = "label41";
		this.label41.Size = new System.Drawing.Size(234, 19);
		this.label41.TabIndex = 262;
		this.label41.Text = "Evrak seri";
		this.label41.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label1.Location = new System.Drawing.Point(37, 31);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(234, 19);
		this.label1.TabIndex = 260;
		this.label1.Text = "Evrak tarihi";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.evrak_tarihi_baslangic.EditValue = new decimal(new int[4]);
		this.evrak_tarihi_baslangic.Location = new System.Drawing.Point(143, 85);
		this.evrak_tarihi_baslangic.Name = "evrak_tarihi_baslangic";
		this.evrak_tarihi_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.evrak_tarihi_baslangic.Properties.IsFloatValue = false;
		this.evrak_tarihi_baslangic.Properties.Mask.EditMask = "N00";
		this.evrak_tarihi_baslangic.Size = new System.Drawing.Size(96, 20);
		this.evrak_tarihi_baslangic.TabIndex = 223;
		this.evrak_tarihi_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.evrak_tarihi_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label29.Location = new System.Drawing.Point(40, 85);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(97, 19);
		this.label29.TabIndex = 220;
		this.label29.Text = "Evrak tarihi :";
		this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.evrak_sira_baslangic.EditValue = new decimal(new int[4]);
		this.evrak_sira_baslangic.Location = new System.Drawing.Point(143, 303);
		this.evrak_sira_baslangic.Name = "evrak_sira_baslangic";
		this.evrak_sira_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.evrak_sira_baslangic.Properties.IsFloatValue = false;
		this.evrak_sira_baslangic.Properties.Mask.EditMask = "N00";
		this.evrak_sira_baslangic.Size = new System.Drawing.Size(96, 20);
		this.evrak_sira_baslangic.TabIndex = 218;
		this.evrak_sira_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.evrak_sira_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label6.Location = new System.Drawing.Point(17, 303);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(120, 19);
		this.label6.TabIndex = 217;
		this.label6.Text = "Evrak sıra :";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.xtraTabPage8.AutoScroll = true;
		this.xtraTabPage8.AutoScrollMargin = new System.Drawing.Size(0, 100);
		this.xtraTabPage8.Controls.Add(this.label209);
		this.xtraTabPage8.Controls.Add(this.belge_no_baslangic);
		this.xtraTabPage8.Controls.Add(this.label7);
		this.xtraTabPage8.Controls.Add(this.label62);
		this.xtraTabPage8.Controls.Add(this.belge_tarihi_baslangic);
		this.xtraTabPage8.Controls.Add(this.label11);
		this.xtraTabPage8.Controls.Add(this.label474);
		this.xtraTabPage8.Controls.Add(this.label476);
		this.xtraTabPage8.Controls.Add(this.label95);
		this.xtraTabPage8.Controls.Add(this.kur_baslangic);
		this.xtraTabPage8.Controls.Add(this.label12);
		this.xtraTabPage8.Controls.Add(this.label473);
		this.xtraTabPage8.Controls.Add(this.plasiyer_kodu_cariden_kullan);
		this.xtraTabPage8.Controls.Add(this.plasiyer_kodu_sabit_deger);
		this.xtraTabPage8.Controls.Add(this.label123);
		this.xtraTabPage8.Controls.Add(this.plasiyer_kodu_baslangic);
		this.xtraTabPage8.Controls.Add(this.label124);
		this.xtraTabPage8.Controls.Add(this.plasiyer_kodu_sabit_kullan);
		this.xtraTabPage8.Controls.Add(this.label125);
		this.xtraTabPage8.Controls.Add(this.label126);
		this.xtraTabPage8.Controls.Add(this.sor_mer_kodu_sabit_deger);
		this.xtraTabPage8.Controls.Add(this.label118);
		this.xtraTabPage8.Controls.Add(this.sor_mer_kodu_baslangic);
		this.xtraTabPage8.Controls.Add(this.label119);
		this.xtraTabPage8.Controls.Add(this.sor_mer_kodu_sabit_kullan);
		this.xtraTabPage8.Controls.Add(this.label120);
		this.xtraTabPage8.Controls.Add(this.label121);
		this.xtraTabPage8.Controls.Add(this.proje_kodu_sabit_deger);
		this.xtraTabPage8.Controls.Add(this.label113);
		this.xtraTabPage8.Controls.Add(this.proje_kodu_baslangic);
		this.xtraTabPage8.Controls.Add(this.label114);
		this.xtraTabPage8.Controls.Add(this.proje_kodu_sabit_kullan);
		this.xtraTabPage8.Controls.Add(this.label115);
		this.xtraTabPage8.Controls.Add(this.label116);
		this.xtraTabPage8.Name = "xtraTabPage8";
		this.xtraTabPage8.Size = new System.Drawing.Size(787, 597);
		this.xtraTabPage8.Text = "Detaylar";
		this.label209.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label209.Location = new System.Drawing.Point(150, 113);
		this.label209.Name = "label209";
		this.label209.Size = new System.Drawing.Size(96, 19);
		this.label209.TabIndex = 396;
		this.label209.Text = "Sıra";
		this.label209.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.belge_no_baslangic.EditValue = new decimal(new int[4]);
		this.belge_no_baslangic.Location = new System.Drawing.Point(150, 135);
		this.belge_no_baslangic.Name = "belge_no_baslangic";
		this.belge_no_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.belge_no_baslangic.Properties.IsFloatValue = false;
		this.belge_no_baslangic.Properties.Mask.EditMask = "N00";
		this.belge_no_baslangic.Size = new System.Drawing.Size(96, 20);
		this.belge_no_baslangic.TabIndex = 394;
		this.belge_no_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.belge_no_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label7.Location = new System.Drawing.Point(24, 135);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(120, 19);
		this.label7.TabIndex = 393;
		this.label7.Text = "Belge no :";
		this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label62.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label62.Location = new System.Drawing.Point(24, 95);
		this.label62.Name = "label62";
		this.label62.Size = new System.Drawing.Size(234, 19);
		this.label62.TabIndex = 392;
		this.label62.Text = "Belge no";
		this.label62.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.belge_tarihi_baslangic.EditValue = new decimal(new int[4]);
		this.belge_tarihi_baslangic.Location = new System.Drawing.Point(153, 56);
		this.belge_tarihi_baslangic.Name = "belge_tarihi_baslangic";
		this.belge_tarihi_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.belge_tarihi_baslangic.Properties.IsFloatValue = false;
		this.belge_tarihi_baslangic.Properties.Mask.EditMask = "N00";
		this.belge_tarihi_baslangic.Size = new System.Drawing.Size(96, 20);
		this.belge_tarihi_baslangic.TabIndex = 386;
		this.belge_tarihi_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.belge_tarihi_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label11.Location = new System.Drawing.Point(50, 56);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(97, 19);
		this.label11.TabIndex = 383;
		this.label11.Text = "Belge tarihi :";
		this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label474.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label474.Location = new System.Drawing.Point(27, 15);
		this.label474.Name = "label474";
		this.label474.Size = new System.Drawing.Size(234, 19);
		this.label474.TabIndex = 382;
		this.label474.Text = "Belge tarihi";
		this.label474.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label476.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label476.Location = new System.Drawing.Point(153, 34);
		this.label476.Name = "label476";
		this.label476.Size = new System.Drawing.Size(96, 19);
		this.label476.TabIndex = 380;
		this.label476.Text = "Sıra";
		this.label476.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label95.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label95.Location = new System.Drawing.Point(153, 475);
		this.label95.Name = "label95";
		this.label95.Size = new System.Drawing.Size(96, 19);
		this.label95.TabIndex = 368;
		this.label95.Text = "Sıra";
		this.label95.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.kur_baslangic.EditValue = new decimal(new int[4]);
		this.kur_baslangic.Location = new System.Drawing.Point(153, 497);
		this.kur_baslangic.Name = "kur_baslangic";
		this.kur_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kur_baslangic.Properties.IsFloatValue = false;
		this.kur_baslangic.Properties.Mask.EditMask = "N00";
		this.kur_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kur_baslangic.TabIndex = 366;
		this.kur_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kur_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label12.Location = new System.Drawing.Point(27, 497);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(120, 19);
		this.label12.TabIndex = 365;
		this.label12.Text = "Kur :";
		this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label473.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label473.Location = new System.Drawing.Point(27, 463);
		this.label473.Name = "label473";
		this.label473.Size = new System.Drawing.Size(234, 19);
		this.label473.TabIndex = 364;
		this.label473.Text = "Döviz cinsi";
		this.label473.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.plasiyer_kodu_cariden_kullan.Location = new System.Drawing.Point(297, 396);
		this.plasiyer_kodu_cariden_kullan.Name = "plasiyer_kodu_cariden_kullan";
		this.plasiyer_kodu_cariden_kullan.Properties.Caption = "Cariden kullan";
		this.plasiyer_kodu_cariden_kullan.Size = new System.Drawing.Size(118, 19);
		this.plasiyer_kodu_cariden_kullan.TabIndex = 351;
		this.plasiyer_kodu_cariden_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.plasiyer_kodu_cariden_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.plasiyer_kodu_sabit_deger.Location = new System.Drawing.Point(175, 423);
		this.plasiyer_kodu_sabit_deger.Name = "plasiyer_kodu_sabit_deger";
		this.plasiyer_kodu_sabit_deger.Size = new System.Drawing.Size(76, 20);
		this.plasiyer_kodu_sabit_deger.TabIndex = 350;
		this.plasiyer_kodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.plasiyer_kodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label123.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label123.Location = new System.Drawing.Point(444, 402);
		this.label123.Name = "label123";
		this.label123.Size = new System.Drawing.Size(96, 19);
		this.label123.TabIndex = 348;
		this.label123.Text = "Sıra";
		this.label123.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.plasiyer_kodu_baslangic.EditValue = new decimal(new int[4]);
		this.plasiyer_kodu_baslangic.Location = new System.Drawing.Point(444, 423);
		this.plasiyer_kodu_baslangic.Name = "plasiyer_kodu_baslangic";
		this.plasiyer_kodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.plasiyer_kodu_baslangic.Properties.IsFloatValue = false;
		this.plasiyer_kodu_baslangic.Properties.Mask.EditMask = "N00";
		this.plasiyer_kodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.plasiyer_kodu_baslangic.TabIndex = 346;
		this.plasiyer_kodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.plasiyer_kodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label124.Location = new System.Drawing.Point(318, 423);
		this.label124.Name = "label124";
		this.label124.Size = new System.Drawing.Size(120, 19);
		this.label124.TabIndex = 345;
		this.label124.Text = "Plasiyer kodu değeri :";
		this.label124.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.plasiyer_kodu_sabit_kullan.Location = new System.Drawing.Point(173, 396);
		this.plasiyer_kodu_sabit_kullan.Name = "plasiyer_kodu_sabit_kullan";
		this.plasiyer_kodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.plasiyer_kodu_sabit_kullan.Size = new System.Drawing.Size(118, 19);
		this.plasiyer_kodu_sabit_kullan.TabIndex = 344;
		this.plasiyer_kodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.plasiyer_kodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label125.Location = new System.Drawing.Point(15, 423);
		this.label125.Name = "label125";
		this.label125.Size = new System.Drawing.Size(154, 19);
		this.label125.TabIndex = 343;
		this.label125.Text = "Plasiyer kodu sabit değer :";
		this.label125.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label126.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label126.Location = new System.Drawing.Point(27, 373);
		this.label126.Name = "label126";
		this.label126.Size = new System.Drawing.Size(234, 19);
		this.label126.TabIndex = 342;
		this.label126.Text = "Plasiyer kodu";
		this.label126.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.sor_mer_kodu_sabit_deger.Location = new System.Drawing.Point(175, 339);
		this.sor_mer_kodu_sabit_deger.Name = "sor_mer_kodu_sabit_deger";
		this.sor_mer_kodu_sabit_deger.Size = new System.Drawing.Size(76, 20);
		this.sor_mer_kodu_sabit_deger.TabIndex = 341;
		this.sor_mer_kodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.sor_mer_kodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label118.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label118.Location = new System.Drawing.Point(444, 318);
		this.label118.Name = "label118";
		this.label118.Size = new System.Drawing.Size(96, 19);
		this.label118.TabIndex = 339;
		this.label118.Text = "Sıra";
		this.label118.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.sor_mer_kodu_baslangic.EditValue = new decimal(new int[4]);
		this.sor_mer_kodu_baslangic.Location = new System.Drawing.Point(444, 339);
		this.sor_mer_kodu_baslangic.Name = "sor_mer_kodu_baslangic";
		this.sor_mer_kodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.sor_mer_kodu_baslangic.Properties.IsFloatValue = false;
		this.sor_mer_kodu_baslangic.Properties.Mask.EditMask = "N00";
		this.sor_mer_kodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.sor_mer_kodu_baslangic.TabIndex = 337;
		this.sor_mer_kodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.sor_mer_kodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label119.Location = new System.Drawing.Point(318, 339);
		this.label119.Name = "label119";
		this.label119.Size = new System.Drawing.Size(120, 19);
		this.label119.TabIndex = 336;
		this.label119.Text = "Sor. mer. kodu değeri :";
		this.label119.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.sor_mer_kodu_sabit_kullan.Location = new System.Drawing.Point(173, 312);
		this.sor_mer_kodu_sabit_kullan.Name = "sor_mer_kodu_sabit_kullan";
		this.sor_mer_kodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.sor_mer_kodu_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.sor_mer_kodu_sabit_kullan.TabIndex = 335;
		this.sor_mer_kodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.sor_mer_kodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label120.Location = new System.Drawing.Point(15, 339);
		this.label120.Name = "label120";
		this.label120.Size = new System.Drawing.Size(154, 19);
		this.label120.TabIndex = 334;
		this.label120.Text = "Sor. merk. kodu sabit değer :";
		this.label120.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label121.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label121.Location = new System.Drawing.Point(27, 289);
		this.label121.Name = "label121";
		this.label121.Size = new System.Drawing.Size(234, 19);
		this.label121.TabIndex = 333;
		this.label121.Text = "Sorumluluk merkezi kodu";
		this.label121.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.proje_kodu_sabit_deger.Location = new System.Drawing.Point(175, 254);
		this.proje_kodu_sabit_deger.Name = "proje_kodu_sabit_deger";
		this.proje_kodu_sabit_deger.Size = new System.Drawing.Size(76, 20);
		this.proje_kodu_sabit_deger.TabIndex = 332;
		this.proje_kodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.proje_kodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label113.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label113.Location = new System.Drawing.Point(444, 233);
		this.label113.Name = "label113";
		this.label113.Size = new System.Drawing.Size(96, 19);
		this.label113.TabIndex = 330;
		this.label113.Text = "Sıra";
		this.label113.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.proje_kodu_baslangic.EditValue = new decimal(new int[4]);
		this.proje_kodu_baslangic.Location = new System.Drawing.Point(444, 254);
		this.proje_kodu_baslangic.Name = "proje_kodu_baslangic";
		this.proje_kodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.proje_kodu_baslangic.Properties.IsFloatValue = false;
		this.proje_kodu_baslangic.Properties.Mask.EditMask = "N00";
		this.proje_kodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.proje_kodu_baslangic.TabIndex = 328;
		this.proje_kodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.proje_kodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label114.Location = new System.Drawing.Point(318, 254);
		this.label114.Name = "label114";
		this.label114.Size = new System.Drawing.Size(120, 19);
		this.label114.TabIndex = 327;
		this.label114.Text = "Proje kodu değeri :";
		this.label114.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.proje_kodu_sabit_kullan.Location = new System.Drawing.Point(173, 227);
		this.proje_kodu_sabit_kullan.Name = "proje_kodu_sabit_kullan";
		this.proje_kodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.proje_kodu_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.proje_kodu_sabit_kullan.TabIndex = 326;
		this.proje_kodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.proje_kodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label115.Location = new System.Drawing.Point(15, 254);
		this.label115.Name = "label115";
		this.label115.Size = new System.Drawing.Size(154, 19);
		this.label115.TabIndex = 325;
		this.label115.Text = "Proje kodu sabit değer :";
		this.label115.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label116.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label116.Location = new System.Drawing.Point(27, 204);
		this.label116.Name = "label116";
		this.label116.Size = new System.Drawing.Size(234, 19);
		this.label116.TabIndex = 324;
		this.label116.Text = "Proje kodu";
		this.label116.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.xtraTabPage9.Controls.Add(this.aciklama10_on_ek_deger);
		this.xtraTabPage9.Controls.Add(this.aciklama10_on_ek_kullan);
		this.xtraTabPage9.Controls.Add(this.aciklama9_on_ek_deger);
		this.xtraTabPage9.Controls.Add(this.aciklama9_on_ek_kullan);
		this.xtraTabPage9.Controls.Add(this.aciklama8_on_ek_deger);
		this.xtraTabPage9.Controls.Add(this.aciklama8_on_ek_kullan);
		this.xtraTabPage9.Controls.Add(this.aciklama7_on_ek_deger);
		this.xtraTabPage9.Controls.Add(this.aciklama7_on_ek_kullan);
		this.xtraTabPage9.Controls.Add(this.aciklama6_on_ek_deger);
		this.xtraTabPage9.Controls.Add(this.aciklama6_on_ek_kullan);
		this.xtraTabPage9.Controls.Add(this.aciklama5_on_ek_deger);
		this.xtraTabPage9.Controls.Add(this.aciklama5_on_ek_kullan);
		this.xtraTabPage9.Controls.Add(this.aciklama4_on_ek_deger);
		this.xtraTabPage9.Controls.Add(this.aciklama4_on_ek_kullan);
		this.xtraTabPage9.Controls.Add(this.aciklama3_on_ek_deger);
		this.xtraTabPage9.Controls.Add(this.aciklama3_on_ek_kullan);
		this.xtraTabPage9.Controls.Add(this.aciklama2_on_ek_deger);
		this.xtraTabPage9.Controls.Add(this.aciklama2_on_ek_kullan);
		this.xtraTabPage9.Controls.Add(this.aciklama1_on_ek_deger);
		this.xtraTabPage9.Controls.Add(this.aciklama1_on_ek_kullan);
		this.xtraTabPage9.Controls.Add(this.aciklama10_sabit_deger);
		this.xtraTabPage9.Controls.Add(this.label165);
		this.xtraTabPage9.Controls.Add(this.aciklama10_baslangic);
		this.xtraTabPage9.Controls.Add(this.label166);
		this.xtraTabPage9.Controls.Add(this.aciklama10_sabit_kullan);
		this.xtraTabPage9.Controls.Add(this.label167);
		this.xtraTabPage9.Controls.Add(this.label168);
		this.xtraTabPage9.Controls.Add(this.aciklama9_sabit_deger);
		this.xtraTabPage9.Controls.Add(this.label160);
		this.xtraTabPage9.Controls.Add(this.aciklama9_baslangic);
		this.xtraTabPage9.Controls.Add(this.label161);
		this.xtraTabPage9.Controls.Add(this.aciklama9_sabit_kullan);
		this.xtraTabPage9.Controls.Add(this.label162);
		this.xtraTabPage9.Controls.Add(this.label163);
		this.xtraTabPage9.Controls.Add(this.aciklama8_sabit_deger);
		this.xtraTabPage9.Controls.Add(this.label155);
		this.xtraTabPage9.Controls.Add(this.aciklama8_baslangic);
		this.xtraTabPage9.Controls.Add(this.label156);
		this.xtraTabPage9.Controls.Add(this.aciklama8_sabit_kullan);
		this.xtraTabPage9.Controls.Add(this.label157);
		this.xtraTabPage9.Controls.Add(this.label158);
		this.xtraTabPage9.Controls.Add(this.aciklama7_sabit_deger);
		this.xtraTabPage9.Controls.Add(this.label150);
		this.xtraTabPage9.Controls.Add(this.aciklama7_baslangic);
		this.xtraTabPage9.Controls.Add(this.label151);
		this.xtraTabPage9.Controls.Add(this.aciklama7_sabit_kullan);
		this.xtraTabPage9.Controls.Add(this.label152);
		this.xtraTabPage9.Controls.Add(this.label153);
		this.xtraTabPage9.Controls.Add(this.aciklama6_sabit_deger);
		this.xtraTabPage9.Controls.Add(this.label145);
		this.xtraTabPage9.Controls.Add(this.aciklama6_baslangic);
		this.xtraTabPage9.Controls.Add(this.label146);
		this.xtraTabPage9.Controls.Add(this.aciklama6_sabit_kullan);
		this.xtraTabPage9.Controls.Add(this.label147);
		this.xtraTabPage9.Controls.Add(this.label148);
		this.xtraTabPage9.Controls.Add(this.aciklama5_sabit_deger);
		this.xtraTabPage9.Controls.Add(this.label140);
		this.xtraTabPage9.Controls.Add(this.aciklama5_baslangic);
		this.xtraTabPage9.Controls.Add(this.label141);
		this.xtraTabPage9.Controls.Add(this.aciklama5_sabit_kullan);
		this.xtraTabPage9.Controls.Add(this.label142);
		this.xtraTabPage9.Controls.Add(this.label143);
		this.xtraTabPage9.Controls.Add(this.aciklama4_sabit_deger);
		this.xtraTabPage9.Controls.Add(this.label135);
		this.xtraTabPage9.Controls.Add(this.aciklama4_baslangic);
		this.xtraTabPage9.Controls.Add(this.label136);
		this.xtraTabPage9.Controls.Add(this.aciklama4_sabit_kullan);
		this.xtraTabPage9.Controls.Add(this.label137);
		this.xtraTabPage9.Controls.Add(this.label138);
		this.xtraTabPage9.Controls.Add(this.aciklama3_sabit_deger);
		this.xtraTabPage9.Controls.Add(this.label130);
		this.xtraTabPage9.Controls.Add(this.aciklama3_baslangic);
		this.xtraTabPage9.Controls.Add(this.label131);
		this.xtraTabPage9.Controls.Add(this.aciklama3_sabit_kullan);
		this.xtraTabPage9.Controls.Add(this.label132);
		this.xtraTabPage9.Controls.Add(this.label133);
		this.xtraTabPage9.Controls.Add(this.aciklama2_sabit_deger);
		this.xtraTabPage9.Controls.Add(this.label36);
		this.xtraTabPage9.Controls.Add(this.aciklama2_baslangic);
		this.xtraTabPage9.Controls.Add(this.label37);
		this.xtraTabPage9.Controls.Add(this.aciklama2_sabit_kullan);
		this.xtraTabPage9.Controls.Add(this.label38);
		this.xtraTabPage9.Controls.Add(this.label39);
		this.xtraTabPage9.Controls.Add(this.aciklama1_sabit_deger);
		this.xtraTabPage9.Controls.Add(this.label27);
		this.xtraTabPage9.Controls.Add(this.aciklama1_baslangic);
		this.xtraTabPage9.Controls.Add(this.label28);
		this.xtraTabPage9.Controls.Add(this.aciklama1_sabit_kullan);
		this.xtraTabPage9.Controls.Add(this.label31);
		this.xtraTabPage9.Controls.Add(this.label32);
		this.xtraTabPage9.Name = "xtraTabPage9";
		this.xtraTabPage9.Size = new System.Drawing.Size(787, 597);
		this.xtraTabPage9.Text = "Açıklamalar";
		this.aciklama10_on_ek_deger.Location = new System.Drawing.Point(310, 502);
		this.aciklama10_on_ek_deger.Name = "aciklama10_on_ek_deger";
		this.aciklama10_on_ek_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama10_on_ek_deger.TabIndex = 462;
		this.aciklama10_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama10_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama10_on_ek_kullan.Location = new System.Drawing.Point(308, 475);
		this.aciklama10_on_ek_kullan.Name = "aciklama10_on_ek_kullan";
		this.aciklama10_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.aciklama10_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.aciklama10_on_ek_kullan.TabIndex = 461;
		this.aciklama10_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama10_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama9_on_ek_deger.Location = new System.Drawing.Point(310, 451);
		this.aciklama9_on_ek_deger.Name = "aciklama9_on_ek_deger";
		this.aciklama9_on_ek_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama9_on_ek_deger.TabIndex = 460;
		this.aciklama9_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama9_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama9_on_ek_kullan.Location = new System.Drawing.Point(308, 424);
		this.aciklama9_on_ek_kullan.Name = "aciklama9_on_ek_kullan";
		this.aciklama9_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.aciklama9_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.aciklama9_on_ek_kullan.TabIndex = 459;
		this.aciklama9_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama9_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama8_on_ek_deger.Location = new System.Drawing.Point(310, 400);
		this.aciklama8_on_ek_deger.Name = "aciklama8_on_ek_deger";
		this.aciklama8_on_ek_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama8_on_ek_deger.TabIndex = 458;
		this.aciklama8_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama8_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama8_on_ek_kullan.Location = new System.Drawing.Point(308, 373);
		this.aciklama8_on_ek_kullan.Name = "aciklama8_on_ek_kullan";
		this.aciklama8_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.aciklama8_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.aciklama8_on_ek_kullan.TabIndex = 457;
		this.aciklama8_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama8_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama7_on_ek_deger.Location = new System.Drawing.Point(310, 349);
		this.aciklama7_on_ek_deger.Name = "aciklama7_on_ek_deger";
		this.aciklama7_on_ek_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama7_on_ek_deger.TabIndex = 456;
		this.aciklama7_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama7_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama7_on_ek_kullan.Location = new System.Drawing.Point(308, 322);
		this.aciklama7_on_ek_kullan.Name = "aciklama7_on_ek_kullan";
		this.aciklama7_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.aciklama7_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.aciklama7_on_ek_kullan.TabIndex = 455;
		this.aciklama7_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama7_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama6_on_ek_deger.Location = new System.Drawing.Point(310, 298);
		this.aciklama6_on_ek_deger.Name = "aciklama6_on_ek_deger";
		this.aciklama6_on_ek_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama6_on_ek_deger.TabIndex = 454;
		this.aciklama6_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama6_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama6_on_ek_kullan.Location = new System.Drawing.Point(308, 271);
		this.aciklama6_on_ek_kullan.Name = "aciklama6_on_ek_kullan";
		this.aciklama6_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.aciklama6_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.aciklama6_on_ek_kullan.TabIndex = 453;
		this.aciklama6_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama6_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama5_on_ek_deger.Location = new System.Drawing.Point(310, 247);
		this.aciklama5_on_ek_deger.Name = "aciklama5_on_ek_deger";
		this.aciklama5_on_ek_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama5_on_ek_deger.TabIndex = 452;
		this.aciklama5_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama5_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama5_on_ek_kullan.Location = new System.Drawing.Point(308, 220);
		this.aciklama5_on_ek_kullan.Name = "aciklama5_on_ek_kullan";
		this.aciklama5_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.aciklama5_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.aciklama5_on_ek_kullan.TabIndex = 451;
		this.aciklama5_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama5_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama4_on_ek_deger.Location = new System.Drawing.Point(310, 196);
		this.aciklama4_on_ek_deger.Name = "aciklama4_on_ek_deger";
		this.aciklama4_on_ek_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama4_on_ek_deger.TabIndex = 450;
		this.aciklama4_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama4_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama4_on_ek_kullan.Location = new System.Drawing.Point(308, 169);
		this.aciklama4_on_ek_kullan.Name = "aciklama4_on_ek_kullan";
		this.aciklama4_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.aciklama4_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.aciklama4_on_ek_kullan.TabIndex = 449;
		this.aciklama4_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama4_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama3_on_ek_deger.Location = new System.Drawing.Point(310, 145);
		this.aciklama3_on_ek_deger.Name = "aciklama3_on_ek_deger";
		this.aciklama3_on_ek_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama3_on_ek_deger.TabIndex = 448;
		this.aciklama3_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama3_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama3_on_ek_kullan.Location = new System.Drawing.Point(308, 118);
		this.aciklama3_on_ek_kullan.Name = "aciklama3_on_ek_kullan";
		this.aciklama3_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.aciklama3_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.aciklama3_on_ek_kullan.TabIndex = 447;
		this.aciklama3_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama3_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama2_on_ek_deger.Location = new System.Drawing.Point(310, 94);
		this.aciklama2_on_ek_deger.Name = "aciklama2_on_ek_deger";
		this.aciklama2_on_ek_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama2_on_ek_deger.TabIndex = 446;
		this.aciklama2_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama2_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama2_on_ek_kullan.Location = new System.Drawing.Point(308, 67);
		this.aciklama2_on_ek_kullan.Name = "aciklama2_on_ek_kullan";
		this.aciklama2_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.aciklama2_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.aciklama2_on_ek_kullan.TabIndex = 445;
		this.aciklama2_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama2_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama1_on_ek_deger.Location = new System.Drawing.Point(310, 43);
		this.aciklama1_on_ek_deger.Name = "aciklama1_on_ek_deger";
		this.aciklama1_on_ek_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama1_on_ek_deger.TabIndex = 444;
		this.aciklama1_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama1_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama1_on_ek_kullan.Location = new System.Drawing.Point(308, 16);
		this.aciklama1_on_ek_kullan.Name = "aciklama1_on_ek_kullan";
		this.aciklama1_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.aciklama1_on_ek_kullan.Size = new System.Drawing.Size(158, 19);
		this.aciklama1_on_ek_kullan.TabIndex = 443;
		this.aciklama1_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama1_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.aciklama10_sabit_deger.Location = new System.Drawing.Point(167, 504);
		this.aciklama10_sabit_deger.Name = "aciklama10_sabit_deger";
		this.aciklama10_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama10_sabit_deger.TabIndex = 440;
		this.aciklama10_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama10_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label165.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label165.Location = new System.Drawing.Point(610, 483);
		this.label165.Name = "label165";
		this.label165.Size = new System.Drawing.Size(96, 19);
		this.label165.TabIndex = 438;
		this.label165.Text = "Sıra";
		this.label165.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.aciklama10_baslangic.EditValue = new decimal(new int[4]);
		this.aciklama10_baslangic.Location = new System.Drawing.Point(610, 504);
		this.aciklama10_baslangic.Name = "aciklama10_baslangic";
		this.aciklama10_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.aciklama10_baslangic.Properties.IsFloatValue = false;
		this.aciklama10_baslangic.Properties.Mask.EditMask = "N00";
		this.aciklama10_baslangic.Size = new System.Drawing.Size(96, 20);
		this.aciklama10_baslangic.TabIndex = 436;
		this.aciklama10_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama10_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label166.Location = new System.Drawing.Point(484, 504);
		this.label166.Name = "label166";
		this.label166.Size = new System.Drawing.Size(120, 19);
		this.label166.TabIndex = 435;
		this.label166.Text = "Açıklama 10 değeri :";
		this.label166.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.aciklama10_sabit_kullan.Location = new System.Drawing.Point(165, 477);
		this.aciklama10_sabit_kullan.Name = "aciklama10_sabit_kullan";
		this.aciklama10_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.aciklama10_sabit_kullan.Size = new System.Drawing.Size(139, 19);
		this.aciklama10_sabit_kullan.TabIndex = 434;
		this.aciklama10_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama10_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label167.Location = new System.Drawing.Point(7, 504);
		this.label167.Name = "label167";
		this.label167.Size = new System.Drawing.Size(154, 19);
		this.label167.TabIndex = 433;
		this.label167.Text = "Açıklama 10 sabit değer :";
		this.label167.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label168.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label168.Location = new System.Drawing.Point(17, 476);
		this.label168.Name = "label168";
		this.label168.Size = new System.Drawing.Size(144, 19);
		this.label168.TabIndex = 432;
		this.label168.Text = "Açıklama 10";
		this.label168.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.aciklama9_sabit_deger.Location = new System.Drawing.Point(167, 453);
		this.aciklama9_sabit_deger.Name = "aciklama9_sabit_deger";
		this.aciklama9_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama9_sabit_deger.TabIndex = 431;
		this.aciklama9_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama9_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label160.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label160.Location = new System.Drawing.Point(610, 432);
		this.label160.Name = "label160";
		this.label160.Size = new System.Drawing.Size(96, 19);
		this.label160.TabIndex = 429;
		this.label160.Text = "Sıra";
		this.label160.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.aciklama9_baslangic.EditValue = new decimal(new int[4]);
		this.aciklama9_baslangic.Location = new System.Drawing.Point(610, 453);
		this.aciklama9_baslangic.Name = "aciklama9_baslangic";
		this.aciklama9_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.aciklama9_baslangic.Properties.IsFloatValue = false;
		this.aciklama9_baslangic.Properties.Mask.EditMask = "N00";
		this.aciklama9_baslangic.Size = new System.Drawing.Size(96, 20);
		this.aciklama9_baslangic.TabIndex = 427;
		this.aciklama9_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama9_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label161.Location = new System.Drawing.Point(484, 453);
		this.label161.Name = "label161";
		this.label161.Size = new System.Drawing.Size(120, 19);
		this.label161.TabIndex = 426;
		this.label161.Text = "Açıklama 9 değeri :";
		this.label161.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.aciklama9_sabit_kullan.Location = new System.Drawing.Point(165, 426);
		this.aciklama9_sabit_kullan.Name = "aciklama9_sabit_kullan";
		this.aciklama9_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.aciklama9_sabit_kullan.Size = new System.Drawing.Size(139, 19);
		this.aciklama9_sabit_kullan.TabIndex = 425;
		this.aciklama9_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama9_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label162.Location = new System.Drawing.Point(7, 453);
		this.label162.Name = "label162";
		this.label162.Size = new System.Drawing.Size(154, 19);
		this.label162.TabIndex = 424;
		this.label162.Text = "Açıklama 9 sabit değer :";
		this.label162.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label163.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label163.Location = new System.Drawing.Point(17, 425);
		this.label163.Name = "label163";
		this.label163.Size = new System.Drawing.Size(144, 19);
		this.label163.TabIndex = 423;
		this.label163.Text = "Açıklama 9";
		this.label163.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.aciklama8_sabit_deger.Location = new System.Drawing.Point(167, 402);
		this.aciklama8_sabit_deger.Name = "aciklama8_sabit_deger";
		this.aciklama8_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama8_sabit_deger.TabIndex = 422;
		this.aciklama8_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama8_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label155.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label155.Location = new System.Drawing.Point(610, 381);
		this.label155.Name = "label155";
		this.label155.Size = new System.Drawing.Size(96, 19);
		this.label155.TabIndex = 420;
		this.label155.Text = "Sıra";
		this.label155.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.aciklama8_baslangic.EditValue = new decimal(new int[4]);
		this.aciklama8_baslangic.Location = new System.Drawing.Point(610, 402);
		this.aciklama8_baslangic.Name = "aciklama8_baslangic";
		this.aciklama8_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.aciklama8_baslangic.Properties.IsFloatValue = false;
		this.aciklama8_baslangic.Properties.Mask.EditMask = "N00";
		this.aciklama8_baslangic.Size = new System.Drawing.Size(96, 20);
		this.aciklama8_baslangic.TabIndex = 418;
		this.aciklama8_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama8_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label156.Location = new System.Drawing.Point(484, 402);
		this.label156.Name = "label156";
		this.label156.Size = new System.Drawing.Size(120, 19);
		this.label156.TabIndex = 417;
		this.label156.Text = "Açıklama 8 değeri :";
		this.label156.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.aciklama8_sabit_kullan.Location = new System.Drawing.Point(165, 375);
		this.aciklama8_sabit_kullan.Name = "aciklama8_sabit_kullan";
		this.aciklama8_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.aciklama8_sabit_kullan.Size = new System.Drawing.Size(139, 19);
		this.aciklama8_sabit_kullan.TabIndex = 416;
		this.aciklama8_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama8_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label157.Location = new System.Drawing.Point(7, 402);
		this.label157.Name = "label157";
		this.label157.Size = new System.Drawing.Size(154, 19);
		this.label157.TabIndex = 415;
		this.label157.Text = "Açıklama 8 sabit değer :";
		this.label157.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label158.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label158.Location = new System.Drawing.Point(17, 374);
		this.label158.Name = "label158";
		this.label158.Size = new System.Drawing.Size(144, 19);
		this.label158.TabIndex = 414;
		this.label158.Text = "Açıklama 8";
		this.label158.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.aciklama7_sabit_deger.Location = new System.Drawing.Point(167, 351);
		this.aciklama7_sabit_deger.Name = "aciklama7_sabit_deger";
		this.aciklama7_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama7_sabit_deger.TabIndex = 413;
		this.aciklama7_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama7_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label150.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label150.Location = new System.Drawing.Point(610, 330);
		this.label150.Name = "label150";
		this.label150.Size = new System.Drawing.Size(96, 19);
		this.label150.TabIndex = 411;
		this.label150.Text = "Sıra";
		this.label150.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.aciklama7_baslangic.EditValue = new decimal(new int[4]);
		this.aciklama7_baslangic.Location = new System.Drawing.Point(610, 351);
		this.aciklama7_baslangic.Name = "aciklama7_baslangic";
		this.aciklama7_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.aciklama7_baslangic.Properties.IsFloatValue = false;
		this.aciklama7_baslangic.Properties.Mask.EditMask = "N00";
		this.aciklama7_baslangic.Size = new System.Drawing.Size(96, 20);
		this.aciklama7_baslangic.TabIndex = 409;
		this.aciklama7_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama7_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label151.Location = new System.Drawing.Point(484, 351);
		this.label151.Name = "label151";
		this.label151.Size = new System.Drawing.Size(120, 19);
		this.label151.TabIndex = 408;
		this.label151.Text = "Açıklama 7 değeri :";
		this.label151.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.aciklama7_sabit_kullan.Location = new System.Drawing.Point(165, 324);
		this.aciklama7_sabit_kullan.Name = "aciklama7_sabit_kullan";
		this.aciklama7_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.aciklama7_sabit_kullan.Size = new System.Drawing.Size(139, 19);
		this.aciklama7_sabit_kullan.TabIndex = 407;
		this.aciklama7_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama7_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label152.Location = new System.Drawing.Point(7, 351);
		this.label152.Name = "label152";
		this.label152.Size = new System.Drawing.Size(154, 19);
		this.label152.TabIndex = 406;
		this.label152.Text = "Açıklama 7 sabit değer :";
		this.label152.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label153.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label153.Location = new System.Drawing.Point(17, 323);
		this.label153.Name = "label153";
		this.label153.Size = new System.Drawing.Size(144, 19);
		this.label153.TabIndex = 405;
		this.label153.Text = "Açıklama 7";
		this.label153.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.aciklama6_sabit_deger.Location = new System.Drawing.Point(167, 300);
		this.aciklama6_sabit_deger.Name = "aciklama6_sabit_deger";
		this.aciklama6_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama6_sabit_deger.TabIndex = 404;
		this.aciklama6_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama6_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label145.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label145.Location = new System.Drawing.Point(610, 279);
		this.label145.Name = "label145";
		this.label145.Size = new System.Drawing.Size(96, 19);
		this.label145.TabIndex = 402;
		this.label145.Text = "Sıra";
		this.label145.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.aciklama6_baslangic.EditValue = new decimal(new int[4]);
		this.aciklama6_baslangic.Location = new System.Drawing.Point(610, 300);
		this.aciklama6_baslangic.Name = "aciklama6_baslangic";
		this.aciklama6_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.aciklama6_baslangic.Properties.IsFloatValue = false;
		this.aciklama6_baslangic.Properties.Mask.EditMask = "N00";
		this.aciklama6_baslangic.Size = new System.Drawing.Size(96, 20);
		this.aciklama6_baslangic.TabIndex = 400;
		this.aciklama6_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama6_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label146.Location = new System.Drawing.Point(484, 300);
		this.label146.Name = "label146";
		this.label146.Size = new System.Drawing.Size(120, 19);
		this.label146.TabIndex = 399;
		this.label146.Text = "Açıklama 6 değeri :";
		this.label146.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.aciklama6_sabit_kullan.Location = new System.Drawing.Point(165, 273);
		this.aciklama6_sabit_kullan.Name = "aciklama6_sabit_kullan";
		this.aciklama6_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.aciklama6_sabit_kullan.Size = new System.Drawing.Size(139, 19);
		this.aciklama6_sabit_kullan.TabIndex = 398;
		this.aciklama6_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama6_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label147.Location = new System.Drawing.Point(7, 300);
		this.label147.Name = "label147";
		this.label147.Size = new System.Drawing.Size(154, 19);
		this.label147.TabIndex = 397;
		this.label147.Text = "Açıklama 6 sabit değer :";
		this.label147.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label148.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label148.Location = new System.Drawing.Point(17, 272);
		this.label148.Name = "label148";
		this.label148.Size = new System.Drawing.Size(144, 19);
		this.label148.TabIndex = 396;
		this.label148.Text = "Açıklama 6";
		this.label148.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.aciklama5_sabit_deger.Location = new System.Drawing.Point(167, 249);
		this.aciklama5_sabit_deger.Name = "aciklama5_sabit_deger";
		this.aciklama5_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama5_sabit_deger.TabIndex = 395;
		this.aciklama5_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama5_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label140.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label140.Location = new System.Drawing.Point(610, 228);
		this.label140.Name = "label140";
		this.label140.Size = new System.Drawing.Size(96, 19);
		this.label140.TabIndex = 393;
		this.label140.Text = "Sıra";
		this.label140.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.aciklama5_baslangic.EditValue = new decimal(new int[4]);
		this.aciklama5_baslangic.Location = new System.Drawing.Point(610, 249);
		this.aciklama5_baslangic.Name = "aciklama5_baslangic";
		this.aciklama5_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.aciklama5_baslangic.Properties.IsFloatValue = false;
		this.aciklama5_baslangic.Properties.Mask.EditMask = "N00";
		this.aciklama5_baslangic.Size = new System.Drawing.Size(96, 20);
		this.aciklama5_baslangic.TabIndex = 391;
		this.aciklama5_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama5_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label141.Location = new System.Drawing.Point(484, 249);
		this.label141.Name = "label141";
		this.label141.Size = new System.Drawing.Size(120, 19);
		this.label141.TabIndex = 390;
		this.label141.Text = "Açıklama 5 değeri :";
		this.label141.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.aciklama5_sabit_kullan.Location = new System.Drawing.Point(165, 222);
		this.aciklama5_sabit_kullan.Name = "aciklama5_sabit_kullan";
		this.aciklama5_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.aciklama5_sabit_kullan.Size = new System.Drawing.Size(139, 19);
		this.aciklama5_sabit_kullan.TabIndex = 389;
		this.aciklama5_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama5_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label142.Location = new System.Drawing.Point(7, 249);
		this.label142.Name = "label142";
		this.label142.Size = new System.Drawing.Size(154, 19);
		this.label142.TabIndex = 388;
		this.label142.Text = "Açıklama 5 sabit değer :";
		this.label142.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label143.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label143.Location = new System.Drawing.Point(17, 221);
		this.label143.Name = "label143";
		this.label143.Size = new System.Drawing.Size(144, 19);
		this.label143.TabIndex = 387;
		this.label143.Text = "Açıklama 5";
		this.label143.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.aciklama4_sabit_deger.Location = new System.Drawing.Point(167, 198);
		this.aciklama4_sabit_deger.Name = "aciklama4_sabit_deger";
		this.aciklama4_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama4_sabit_deger.TabIndex = 386;
		this.aciklama4_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama4_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label135.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label135.Location = new System.Drawing.Point(610, 177);
		this.label135.Name = "label135";
		this.label135.Size = new System.Drawing.Size(96, 19);
		this.label135.TabIndex = 384;
		this.label135.Text = "Sıra";
		this.label135.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.aciklama4_baslangic.EditValue = new decimal(new int[4]);
		this.aciklama4_baslangic.Location = new System.Drawing.Point(610, 198);
		this.aciklama4_baslangic.Name = "aciklama4_baslangic";
		this.aciklama4_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.aciklama4_baslangic.Properties.IsFloatValue = false;
		this.aciklama4_baslangic.Properties.Mask.EditMask = "N00";
		this.aciklama4_baslangic.Size = new System.Drawing.Size(96, 20);
		this.aciklama4_baslangic.TabIndex = 382;
		this.aciklama4_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama4_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label136.Location = new System.Drawing.Point(484, 198);
		this.label136.Name = "label136";
		this.label136.Size = new System.Drawing.Size(120, 19);
		this.label136.TabIndex = 381;
		this.label136.Text = "Açıklama 4 değeri :";
		this.label136.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.aciklama4_sabit_kullan.Location = new System.Drawing.Point(165, 171);
		this.aciklama4_sabit_kullan.Name = "aciklama4_sabit_kullan";
		this.aciklama4_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.aciklama4_sabit_kullan.Size = new System.Drawing.Size(139, 19);
		this.aciklama4_sabit_kullan.TabIndex = 380;
		this.aciklama4_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama4_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label137.Location = new System.Drawing.Point(7, 198);
		this.label137.Name = "label137";
		this.label137.Size = new System.Drawing.Size(154, 19);
		this.label137.TabIndex = 379;
		this.label137.Text = "Açıklama 4 sabit değer :";
		this.label137.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label138.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label138.Location = new System.Drawing.Point(17, 170);
		this.label138.Name = "label138";
		this.label138.Size = new System.Drawing.Size(144, 19);
		this.label138.TabIndex = 378;
		this.label138.Text = "Açıklama 4";
		this.label138.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.aciklama3_sabit_deger.Location = new System.Drawing.Point(167, 147);
		this.aciklama3_sabit_deger.Name = "aciklama3_sabit_deger";
		this.aciklama3_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama3_sabit_deger.TabIndex = 377;
		this.aciklama3_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama3_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label130.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label130.Location = new System.Drawing.Point(610, 126);
		this.label130.Name = "label130";
		this.label130.Size = new System.Drawing.Size(96, 19);
		this.label130.TabIndex = 375;
		this.label130.Text = "Sıra";
		this.label130.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.aciklama3_baslangic.EditValue = new decimal(new int[4]);
		this.aciklama3_baslangic.Location = new System.Drawing.Point(610, 147);
		this.aciklama3_baslangic.Name = "aciklama3_baslangic";
		this.aciklama3_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.aciklama3_baslangic.Properties.IsFloatValue = false;
		this.aciklama3_baslangic.Properties.Mask.EditMask = "N00";
		this.aciklama3_baslangic.Size = new System.Drawing.Size(96, 20);
		this.aciklama3_baslangic.TabIndex = 373;
		this.aciklama3_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama3_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label131.Location = new System.Drawing.Point(484, 147);
		this.label131.Name = "label131";
		this.label131.Size = new System.Drawing.Size(120, 19);
		this.label131.TabIndex = 372;
		this.label131.Text = "Açıklama 3 değeri :";
		this.label131.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.aciklama3_sabit_kullan.Location = new System.Drawing.Point(165, 120);
		this.aciklama3_sabit_kullan.Name = "aciklama3_sabit_kullan";
		this.aciklama3_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.aciklama3_sabit_kullan.Size = new System.Drawing.Size(139, 19);
		this.aciklama3_sabit_kullan.TabIndex = 371;
		this.aciklama3_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama3_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label132.Location = new System.Drawing.Point(7, 147);
		this.label132.Name = "label132";
		this.label132.Size = new System.Drawing.Size(154, 19);
		this.label132.TabIndex = 370;
		this.label132.Text = "Açıklama 3 sabit değer :";
		this.label132.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label133.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label133.Location = new System.Drawing.Point(17, 119);
		this.label133.Name = "label133";
		this.label133.Size = new System.Drawing.Size(144, 19);
		this.label133.TabIndex = 369;
		this.label133.Text = "Açıklama 3";
		this.label133.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.aciklama2_sabit_deger.Location = new System.Drawing.Point(167, 96);
		this.aciklama2_sabit_deger.Name = "aciklama2_sabit_deger";
		this.aciklama2_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama2_sabit_deger.TabIndex = 368;
		this.aciklama2_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama2_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label36.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label36.Location = new System.Drawing.Point(610, 75);
		this.label36.Name = "label36";
		this.label36.Size = new System.Drawing.Size(96, 19);
		this.label36.TabIndex = 366;
		this.label36.Text = "Sıra";
		this.label36.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.aciklama2_baslangic.EditValue = new decimal(new int[4]);
		this.aciklama2_baslangic.Location = new System.Drawing.Point(610, 96);
		this.aciklama2_baslangic.Name = "aciklama2_baslangic";
		this.aciklama2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.aciklama2_baslangic.Properties.IsFloatValue = false;
		this.aciklama2_baslangic.Properties.Mask.EditMask = "N00";
		this.aciklama2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.aciklama2_baslangic.TabIndex = 364;
		this.aciklama2_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama2_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label37.Location = new System.Drawing.Point(484, 96);
		this.label37.Name = "label37";
		this.label37.Size = new System.Drawing.Size(120, 19);
		this.label37.TabIndex = 363;
		this.label37.Text = "Açıklama 2 değeri :";
		this.label37.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.aciklama2_sabit_kullan.Location = new System.Drawing.Point(165, 69);
		this.aciklama2_sabit_kullan.Name = "aciklama2_sabit_kullan";
		this.aciklama2_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.aciklama2_sabit_kullan.Size = new System.Drawing.Size(139, 19);
		this.aciklama2_sabit_kullan.TabIndex = 362;
		this.aciklama2_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama2_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label38.Location = new System.Drawing.Point(7, 96);
		this.label38.Name = "label38";
		this.label38.Size = new System.Drawing.Size(154, 19);
		this.label38.TabIndex = 361;
		this.label38.Text = "Açıklama 2 sabit değer :";
		this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label39.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label39.Location = new System.Drawing.Point(17, 68);
		this.label39.Name = "label39";
		this.label39.Size = new System.Drawing.Size(144, 19);
		this.label39.TabIndex = 360;
		this.label39.Text = "Açıklama 2";
		this.label39.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.aciklama1_sabit_deger.Location = new System.Drawing.Point(167, 45);
		this.aciklama1_sabit_deger.Name = "aciklama1_sabit_deger";
		this.aciklama1_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.aciklama1_sabit_deger.TabIndex = 359;
		this.aciklama1_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama1_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label27.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label27.Location = new System.Drawing.Point(610, 24);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(96, 19);
		this.label27.TabIndex = 357;
		this.label27.Text = "Sıra";
		this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.aciklama1_baslangic.EditValue = new decimal(new int[4]);
		this.aciklama1_baslangic.Location = new System.Drawing.Point(610, 45);
		this.aciklama1_baslangic.Name = "aciklama1_baslangic";
		this.aciklama1_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.aciklama1_baslangic.Properties.IsFloatValue = false;
		this.aciklama1_baslangic.Properties.Mask.EditMask = "N00";
		this.aciklama1_baslangic.Size = new System.Drawing.Size(96, 20);
		this.aciklama1_baslangic.TabIndex = 355;
		this.aciklama1_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama1_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label28.Location = new System.Drawing.Point(484, 45);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(120, 19);
		this.label28.TabIndex = 354;
		this.label28.Text = "Açıklama 1 değeri :";
		this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.aciklama1_sabit_kullan.Location = new System.Drawing.Point(165, 18);
		this.aciklama1_sabit_kullan.Name = "aciklama1_sabit_kullan";
		this.aciklama1_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.aciklama1_sabit_kullan.Size = new System.Drawing.Size(139, 19);
		this.aciklama1_sabit_kullan.TabIndex = 353;
		this.aciklama1_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.aciklama1_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label31.Location = new System.Drawing.Point(7, 45);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(154, 19);
		this.label31.TabIndex = 352;
		this.label31.Text = "Açıklama 1 sabit değer :";
		this.label31.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label32.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label32.Location = new System.Drawing.Point(17, 17);
		this.label32.Name = "label32";
		this.label32.Size = new System.Drawing.Size(142, 19);
		this.label32.TabIndex = 351;
		this.label32.Text = "Açıklama 1";
		this.label32.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.xtraTabPage10.Controls.Add(this.ozel_alan_3_sabit_deger);
		this.xtraTabPage10.Controls.Add(this.label180);
		this.xtraTabPage10.Controls.Add(this.ozel_alan_3_baslangic);
		this.xtraTabPage10.Controls.Add(this.label181);
		this.xtraTabPage10.Controls.Add(this.ozel_alan_3_sabit_kullan);
		this.xtraTabPage10.Controls.Add(this.label182);
		this.xtraTabPage10.Controls.Add(this.label183);
		this.xtraTabPage10.Controls.Add(this.ozel_alan_2_sabit_deger);
		this.xtraTabPage10.Controls.Add(this.label175);
		this.xtraTabPage10.Controls.Add(this.ozel_alan_2_baslangic);
		this.xtraTabPage10.Controls.Add(this.label176);
		this.xtraTabPage10.Controls.Add(this.ozel_alan_2_sabit_kullan);
		this.xtraTabPage10.Controls.Add(this.label177);
		this.xtraTabPage10.Controls.Add(this.label178);
		this.xtraTabPage10.Controls.Add(this.ozel_alan_1_sabit_deger);
		this.xtraTabPage10.Controls.Add(this.label170);
		this.xtraTabPage10.Controls.Add(this.ozel_alan_1_baslangic);
		this.xtraTabPage10.Controls.Add(this.label171);
		this.xtraTabPage10.Controls.Add(this.ozel_alan_1_sabit_kullan);
		this.xtraTabPage10.Controls.Add(this.label172);
		this.xtraTabPage10.Controls.Add(this.label173);
		this.xtraTabPage10.Name = "xtraTabPage10";
		this.xtraTabPage10.Size = new System.Drawing.Size(787, 597);
		this.xtraTabPage10.Text = "Özel alanlar";
		this.ozel_alan_3_sabit_deger.Location = new System.Drawing.Point(171, 275);
		this.ozel_alan_3_sabit_deger.Name = "ozel_alan_3_sabit_deger";
		this.ozel_alan_3_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.ozel_alan_3_sabit_deger.TabIndex = 377;
		this.ozel_alan_3_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.ozel_alan_3_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label180.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label180.Location = new System.Drawing.Point(440, 254);
		this.label180.Name = "label180";
		this.label180.Size = new System.Drawing.Size(96, 19);
		this.label180.TabIndex = 375;
		this.label180.Text = "Sıra";
		this.label180.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.ozel_alan_3_baslangic.EditValue = new decimal(new int[4]);
		this.ozel_alan_3_baslangic.Location = new System.Drawing.Point(440, 275);
		this.ozel_alan_3_baslangic.Name = "ozel_alan_3_baslangic";
		this.ozel_alan_3_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.ozel_alan_3_baslangic.Properties.IsFloatValue = false;
		this.ozel_alan_3_baslangic.Properties.Mask.EditMask = "N00";
		this.ozel_alan_3_baslangic.Size = new System.Drawing.Size(96, 20);
		this.ozel_alan_3_baslangic.TabIndex = 373;
		this.ozel_alan_3_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.ozel_alan_3_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label181.Location = new System.Drawing.Point(314, 275);
		this.label181.Name = "label181";
		this.label181.Size = new System.Drawing.Size(120, 19);
		this.label181.TabIndex = 372;
		this.label181.Text = "Özel alan 3 değeri :";
		this.label181.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.ozel_alan_3_sabit_kullan.Location = new System.Drawing.Point(169, 248);
		this.ozel_alan_3_sabit_kullan.Name = "ozel_alan_3_sabit_kullan";
		this.ozel_alan_3_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.ozel_alan_3_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.ozel_alan_3_sabit_kullan.TabIndex = 371;
		this.ozel_alan_3_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.ozel_alan_3_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label182.Location = new System.Drawing.Point(11, 275);
		this.label182.Name = "label182";
		this.label182.Size = new System.Drawing.Size(154, 19);
		this.label182.TabIndex = 370;
		this.label182.Text = "Özel alan 3 sabit değer :";
		this.label182.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label183.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label183.Location = new System.Drawing.Point(26, 227);
		this.label183.Name = "label183";
		this.label183.Size = new System.Drawing.Size(234, 19);
		this.label183.TabIndex = 369;
		this.label183.Text = "Özel alan 3";
		this.label183.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.ozel_alan_2_sabit_deger.Location = new System.Drawing.Point(171, 182);
		this.ozel_alan_2_sabit_deger.Name = "ozel_alan_2_sabit_deger";
		this.ozel_alan_2_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.ozel_alan_2_sabit_deger.TabIndex = 368;
		this.ozel_alan_2_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.ozel_alan_2_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label175.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label175.Location = new System.Drawing.Point(440, 161);
		this.label175.Name = "label175";
		this.label175.Size = new System.Drawing.Size(96, 19);
		this.label175.TabIndex = 366;
		this.label175.Text = "Sıra";
		this.label175.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.ozel_alan_2_baslangic.EditValue = new decimal(new int[4]);
		this.ozel_alan_2_baslangic.Location = new System.Drawing.Point(440, 182);
		this.ozel_alan_2_baslangic.Name = "ozel_alan_2_baslangic";
		this.ozel_alan_2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.ozel_alan_2_baslangic.Properties.IsFloatValue = false;
		this.ozel_alan_2_baslangic.Properties.Mask.EditMask = "N00";
		this.ozel_alan_2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.ozel_alan_2_baslangic.TabIndex = 364;
		this.ozel_alan_2_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.ozel_alan_2_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label176.Location = new System.Drawing.Point(314, 182);
		this.label176.Name = "label176";
		this.label176.Size = new System.Drawing.Size(120, 19);
		this.label176.TabIndex = 363;
		this.label176.Text = "Özel alan 2 değeri :";
		this.label176.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.ozel_alan_2_sabit_kullan.Location = new System.Drawing.Point(169, 155);
		this.ozel_alan_2_sabit_kullan.Name = "ozel_alan_2_sabit_kullan";
		this.ozel_alan_2_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.ozel_alan_2_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.ozel_alan_2_sabit_kullan.TabIndex = 362;
		this.ozel_alan_2_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.ozel_alan_2_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label177.Location = new System.Drawing.Point(11, 182);
		this.label177.Name = "label177";
		this.label177.Size = new System.Drawing.Size(154, 19);
		this.label177.TabIndex = 361;
		this.label177.Text = "Özel alan 2 sabit değer :";
		this.label177.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label178.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label178.Location = new System.Drawing.Point(26, 134);
		this.label178.Name = "label178";
		this.label178.Size = new System.Drawing.Size(234, 19);
		this.label178.TabIndex = 360;
		this.label178.Text = "Özel alan 2";
		this.label178.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.ozel_alan_1_sabit_deger.Location = new System.Drawing.Point(171, 97);
		this.ozel_alan_1_sabit_deger.Name = "ozel_alan_1_sabit_deger";
		this.ozel_alan_1_sabit_deger.Size = new System.Drawing.Size(137, 20);
		this.ozel_alan_1_sabit_deger.TabIndex = 359;
		this.ozel_alan_1_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.ozel_alan_1_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label170.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label170.Location = new System.Drawing.Point(440, 76);
		this.label170.Name = "label170";
		this.label170.Size = new System.Drawing.Size(96, 19);
		this.label170.TabIndex = 357;
		this.label170.Text = "Sıra";
		this.label170.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.ozel_alan_1_baslangic.EditValue = new decimal(new int[4]);
		this.ozel_alan_1_baslangic.Location = new System.Drawing.Point(440, 97);
		this.ozel_alan_1_baslangic.Name = "ozel_alan_1_baslangic";
		this.ozel_alan_1_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.ozel_alan_1_baslangic.Properties.IsFloatValue = false;
		this.ozel_alan_1_baslangic.Properties.Mask.EditMask = "N00";
		this.ozel_alan_1_baslangic.Size = new System.Drawing.Size(96, 20);
		this.ozel_alan_1_baslangic.TabIndex = 355;
		this.ozel_alan_1_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.ozel_alan_1_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label171.Location = new System.Drawing.Point(314, 97);
		this.label171.Name = "label171";
		this.label171.Size = new System.Drawing.Size(120, 19);
		this.label171.TabIndex = 354;
		this.label171.Text = "Özel alan 1 değeri :";
		this.label171.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.ozel_alan_1_sabit_kullan.Location = new System.Drawing.Point(169, 70);
		this.ozel_alan_1_sabit_kullan.Name = "ozel_alan_1_sabit_kullan";
		this.ozel_alan_1_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.ozel_alan_1_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.ozel_alan_1_sabit_kullan.TabIndex = 353;
		this.ozel_alan_1_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.ozel_alan_1_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label172.Location = new System.Drawing.Point(11, 97);
		this.label172.Name = "label172";
		this.label172.Size = new System.Drawing.Size(154, 19);
		this.label172.TabIndex = 352;
		this.label172.Text = "Özel alan 1 sabit değer :";
		this.label172.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label173.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label173.Location = new System.Drawing.Point(26, 49);
		this.label173.Name = "label173";
		this.label173.Size = new System.Drawing.Size(234, 19);
		this.label173.TabIndex = 351;
		this.label173.Text = "Özel alan 1";
		this.label173.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.xtraTabPage35.Controls.Add(this.xtraTabControl8);
		this.xtraTabPage35.Name = "xtraTabPage35";
		this.xtraTabPage35.Size = new System.Drawing.Size(787, 597);
		this.xtraTabPage35.Text = "Yeni hesap bilgileri";
		this.xtraTabControl8.Dock = System.Windows.Forms.DockStyle.Fill;
		this.xtraTabControl8.Location = new System.Drawing.Point(0, 0);
		this.xtraTabControl8.Name = "xtraTabControl8";
		this.xtraTabControl8.SelectedTabPage = this.xtraTabPage36;
		this.xtraTabControl8.Size = new System.Drawing.Size(787, 597);
		this.xtraTabControl8.TabIndex = 0;
		this.xtraTabControl8.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[3] { this.xtraTabPage36, this.xtraTabPage17, this.xtraTabPage40 });
		this.xtraTabPage36.Controls.Add(this.xtraTabControl9);
		this.xtraTabPage36.Name = "xtraTabPage36";
		this.xtraTabPage36.Size = new System.Drawing.Size(781, 569);
		this.xtraTabPage36.Text = "Yeni cari hesap bilgileri";
		this.xtraTabControl9.Dock = System.Windows.Forms.DockStyle.Fill;
		this.xtraTabControl9.Location = new System.Drawing.Point(0, 0);
		this.xtraTabControl9.Name = "xtraTabControl9";
		this.xtraTabControl9.SelectedTabPage = this.xtraTabPage27;
		this.xtraTabControl9.Size = new System.Drawing.Size(781, 569);
		this.xtraTabControl9.TabIndex = 0;
		this.xtraTabControl9.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[4] { this.xtraTabPage27, this.xtraTabPage41, this.xtraTabPage39, this.xtraTabPage31 });
		this.xtraTabPage27.Controls.Add(this.label3);
		this.xtraTabPage27.Controls.Add(this.label694);
		this.xtraTabPage27.Controls.Add(this.cari_Portal_Enabled);
		this.xtraTabPage27.Controls.Add(this.cari_satis_isk_kod_sabit_deger);
		this.xtraTabPage27.Controls.Add(this.label670);
		this.xtraTabPage27.Controls.Add(this.cari_satis_isk_kod_baslangic);
		this.xtraTabPage27.Controls.Add(this.label671);
		this.xtraTabPage27.Controls.Add(this.cari_satis_isk_kod_sabit_kullan);
		this.xtraTabPage27.Controls.Add(this.label672);
		this.xtraTabPage27.Controls.Add(this.label673);
		this.xtraTabPage27.Controls.Add(this.cari_sicil_no_sabit_deger);
		this.xtraTabPage27.Controls.Add(this.label665);
		this.xtraTabPage27.Controls.Add(this.cari_sicil_no_baslangic);
		this.xtraTabPage27.Controls.Add(this.label666);
		this.xtraTabPage27.Controls.Add(this.cari_sicil_no_sabit_kullan);
		this.xtraTabPage27.Controls.Add(this.label667);
		this.xtraTabPage27.Controls.Add(this.label668);
		this.xtraTabPage27.Controls.Add(this.cari_CepTel_sabit_deger);
		this.xtraTabPage27.Controls.Add(this.label660);
		this.xtraTabPage27.Controls.Add(this.cari_CepTel_baslangic);
		this.xtraTabPage27.Controls.Add(this.label661);
		this.xtraTabPage27.Controls.Add(this.cari_CepTel_sabit_kullan);
		this.xtraTabPage27.Controls.Add(this.label662);
		this.xtraTabPage27.Controls.Add(this.label663);
		this.xtraTabPage27.Controls.Add(this.cari_wwwadresi_sabit_deger);
		this.xtraTabPage27.Controls.Add(this.label655);
		this.xtraTabPage27.Controls.Add(this.cari_wwwadresi_baslangic);
		this.xtraTabPage27.Controls.Add(this.label656);
		this.xtraTabPage27.Controls.Add(this.cari_wwwadresi_sabit_kullan);
		this.xtraTabPage27.Controls.Add(this.label657);
		this.xtraTabPage27.Controls.Add(this.label658);
		this.xtraTabPage27.Controls.Add(this.cari_Ana_cari_kodu_sabit_deger);
		this.xtraTabPage27.Controls.Add(this.label650);
		this.xtraTabPage27.Controls.Add(this.cari_Ana_cari_kodu_baslangic);
		this.xtraTabPage27.Controls.Add(this.label651);
		this.xtraTabPage27.Controls.Add(this.cari_Ana_cari_kodu_sabit_kullan);
		this.xtraTabPage27.Controls.Add(this.label652);
		this.xtraTabPage27.Controls.Add(this.label653);
		this.xtraTabPage27.Controls.Add(this.cari_temsilci_kodu_sabit_deger);
		this.xtraTabPage27.Controls.Add(this.label414);
		this.xtraTabPage27.Controls.Add(this.cari_temsilci_kodu_baslangic);
		this.xtraTabPage27.Controls.Add(this.label415);
		this.xtraTabPage27.Controls.Add(this.cari_temsilci_kodu_sabit_kullan);
		this.xtraTabPage27.Controls.Add(this.label416);
		this.xtraTabPage27.Controls.Add(this.label648);
		this.xtraTabPage27.Controls.Add(this.label551);
		this.xtraTabPage27.Controls.Add(this.otomatik_hesap_acma_secenek_cari);
		this.xtraTabPage27.Controls.Add(this.label549);
		this.xtraTabPage27.Controls.Add(this.cari_vergi_dairesi_baslangic);
		this.xtraTabPage27.Controls.Add(this.label221);
		this.xtraTabPage27.Name = "xtraTabPage27";
		this.xtraTabPage27.Size = new System.Drawing.Size(775, 541);
		this.xtraTabPage27.Text = "Genel bilgiler";
		this.label3.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label3.Location = new System.Drawing.Point(166, 70);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(96, 19);
		this.label3.TabIndex = 560;
		this.label3.Text = "Sıra";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label694.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label694.Location = new System.Drawing.Point(485, 60);
		this.label694.Name = "label694";
		this.label694.Size = new System.Drawing.Size(90, 19);
		this.label694.TabIndex = 559;
		this.label694.Text = "Portal aktif";
		this.label694.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_Portal_Enabled.Location = new System.Drawing.Point(581, 60);
		this.cari_Portal_Enabled.Name = "cari_Portal_Enabled";
		this.cari_Portal_Enabled.Properties.Caption = "Portal aktif";
		this.cari_Portal_Enabled.Size = new System.Drawing.Size(113, 19);
		this.cari_Portal_Enabled.TabIndex = 558;
		this.cari_Portal_Enabled.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_Portal_Enabled.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_satis_isk_kod_sabit_deger.Location = new System.Drawing.Point(583, 382);
		this.cari_satis_isk_kod_sabit_deger.Name = "cari_satis_isk_kod_sabit_deger";
		this.cari_satis_isk_kod_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_satis_isk_kod_sabit_deger.TabIndex = 557;
		this.cari_satis_isk_kod_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_satis_isk_kod_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label670.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label670.Location = new System.Drawing.Point(583, 406);
		this.label670.Name = "label670";
		this.label670.Size = new System.Drawing.Size(96, 19);
		this.label670.TabIndex = 555;
		this.label670.Text = "Sıra";
		this.label670.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_satis_isk_kod_baslangic.EditValue = new decimal(new int[4]);
		this.cari_satis_isk_kod_baslangic.Location = new System.Drawing.Point(583, 427);
		this.cari_satis_isk_kod_baslangic.Name = "cari_satis_isk_kod_baslangic";
		this.cari_satis_isk_kod_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_satis_isk_kod_baslangic.Properties.IsFloatValue = false;
		this.cari_satis_isk_kod_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_satis_isk_kod_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_satis_isk_kod_baslangic.TabIndex = 553;
		this.cari_satis_isk_kod_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_satis_isk_kod_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label671.Location = new System.Drawing.Point(437, 427);
		this.label671.Name = "label671";
		this.label671.Size = new System.Drawing.Size(140, 19);
		this.label671.TabIndex = 552;
		this.label671.Text = "İskonto kodu :";
		this.label671.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_satis_isk_kod_sabit_kullan.Location = new System.Drawing.Point(581, 357);
		this.cari_satis_isk_kod_sabit_kullan.Name = "cari_satis_isk_kod_sabit_kullan";
		this.cari_satis_isk_kod_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_satis_isk_kod_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_satis_isk_kod_sabit_kullan.TabIndex = 551;
		this.cari_satis_isk_kod_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_satis_isk_kod_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label672.Location = new System.Drawing.Point(434, 382);
		this.label672.Name = "label672";
		this.label672.Size = new System.Drawing.Size(143, 19);
		this.label672.TabIndex = 550;
		this.label672.Text = "İskonto kodu sabit değer :";
		this.label672.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label673.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label673.Location = new System.Drawing.Point(442, 355);
		this.label673.Name = "label673";
		this.label673.Size = new System.Drawing.Size(135, 19);
		this.label673.TabIndex = 549;
		this.label673.Text = "İskonto kodu";
		this.label673.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_sicil_no_sabit_deger.Location = new System.Drawing.Point(184, 382);
		this.cari_sicil_no_sabit_deger.Name = "cari_sicil_no_sabit_deger";
		this.cari_sicil_no_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_sicil_no_sabit_deger.TabIndex = 548;
		this.cari_sicil_no_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_sicil_no_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label665.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label665.Location = new System.Drawing.Point(184, 406);
		this.label665.Name = "label665";
		this.label665.Size = new System.Drawing.Size(96, 19);
		this.label665.TabIndex = 546;
		this.label665.Text = "Sıra";
		this.label665.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_sicil_no_baslangic.EditValue = new decimal(new int[4]);
		this.cari_sicil_no_baslangic.Location = new System.Drawing.Point(184, 427);
		this.cari_sicil_no_baslangic.Name = "cari_sicil_no_baslangic";
		this.cari_sicil_no_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_sicil_no_baslangic.Properties.IsFloatValue = false;
		this.cari_sicil_no_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_sicil_no_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_sicil_no_baslangic.TabIndex = 544;
		this.cari_sicil_no_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_sicil_no_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label666.Location = new System.Drawing.Point(38, 427);
		this.label666.Name = "label666";
		this.label666.Size = new System.Drawing.Size(140, 19);
		this.label666.TabIndex = 543;
		this.label666.Text = "Ticaret oda sicil no:";
		this.label666.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_sicil_no_sabit_kullan.Location = new System.Drawing.Point(185, 357);
		this.cari_sicil_no_sabit_kullan.Name = "cari_sicil_no_sabit_kullan";
		this.cari_sicil_no_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_sicil_no_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_sicil_no_sabit_kullan.TabIndex = 542;
		this.cari_sicil_no_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_sicil_no_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label667.Location = new System.Drawing.Point(3, 382);
		this.label667.Name = "label667";
		this.label667.Size = new System.Drawing.Size(175, 19);
		this.label667.TabIndex = 541;
		this.label667.Text = "Ticaret oda sicil no sabit değer :";
		this.label667.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label668.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label668.Location = new System.Drawing.Point(15, 355);
		this.label668.Name = "label668";
		this.label668.Size = new System.Drawing.Size(163, 19);
		this.label668.TabIndex = 540;
		this.label668.Text = "Ticaret oda sicil no";
		this.label668.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_CepTel_sabit_deger.Location = new System.Drawing.Point(583, 271);
		this.cari_CepTel_sabit_deger.Name = "cari_CepTel_sabit_deger";
		this.cari_CepTel_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_CepTel_sabit_deger.TabIndex = 539;
		this.cari_CepTel_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_CepTel_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label660.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label660.Location = new System.Drawing.Point(583, 295);
		this.label660.Name = "label660";
		this.label660.Size = new System.Drawing.Size(96, 19);
		this.label660.TabIndex = 537;
		this.label660.Text = "Sıra";
		this.label660.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_CepTel_baslangic.EditValue = new decimal(new int[4]);
		this.cari_CepTel_baslangic.Location = new System.Drawing.Point(583, 316);
		this.cari_CepTel_baslangic.Name = "cari_CepTel_baslangic";
		this.cari_CepTel_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_CepTel_baslangic.Properties.IsFloatValue = false;
		this.cari_CepTel_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_CepTel_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_CepTel_baslangic.TabIndex = 535;
		this.cari_CepTel_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_CepTel_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label661.Location = new System.Drawing.Point(437, 316);
		this.label661.Name = "label661";
		this.label661.Size = new System.Drawing.Size(140, 19);
		this.label661.TabIndex = 534;
		this.label661.Text = "Yetkili cep tel :";
		this.label661.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_CepTel_sabit_kullan.Location = new System.Drawing.Point(581, 246);
		this.cari_CepTel_sabit_kullan.Name = "cari_CepTel_sabit_kullan";
		this.cari_CepTel_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_CepTel_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_CepTel_sabit_kullan.TabIndex = 533;
		this.cari_CepTel_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_CepTel_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label662.Location = new System.Drawing.Point(434, 271);
		this.label662.Name = "label662";
		this.label662.Size = new System.Drawing.Size(143, 19);
		this.label662.TabIndex = 532;
		this.label662.Text = "Yetkili cep tel sabit değer :";
		this.label662.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label663.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label663.Location = new System.Drawing.Point(442, 244);
		this.label663.Name = "label663";
		this.label663.Size = new System.Drawing.Size(135, 19);
		this.label663.TabIndex = 531;
		this.label663.Text = "Yetkili cep tel";
		this.label663.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_wwwadresi_sabit_deger.Location = new System.Drawing.Point(184, 271);
		this.cari_wwwadresi_sabit_deger.Name = "cari_wwwadresi_sabit_deger";
		this.cari_wwwadresi_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_wwwadresi_sabit_deger.TabIndex = 530;
		this.cari_wwwadresi_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_wwwadresi_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label655.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label655.Location = new System.Drawing.Point(184, 295);
		this.label655.Name = "label655";
		this.label655.Size = new System.Drawing.Size(96, 19);
		this.label655.TabIndex = 528;
		this.label655.Text = "Sıra";
		this.label655.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_wwwadresi_baslangic.EditValue = new decimal(new int[4]);
		this.cari_wwwadresi_baslangic.Location = new System.Drawing.Point(184, 316);
		this.cari_wwwadresi_baslangic.Name = "cari_wwwadresi_baslangic";
		this.cari_wwwadresi_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_wwwadresi_baslangic.Properties.IsFloatValue = false;
		this.cari_wwwadresi_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_wwwadresi_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_wwwadresi_baslangic.TabIndex = 526;
		this.cari_wwwadresi_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_wwwadresi_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label656.Location = new System.Drawing.Point(38, 316);
		this.label656.Name = "label656";
		this.label656.Size = new System.Drawing.Size(140, 19);
		this.label656.TabIndex = 525;
		this.label656.Text = "Web adresi :";
		this.label656.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_wwwadresi_sabit_kullan.Location = new System.Drawing.Point(185, 246);
		this.cari_wwwadresi_sabit_kullan.Name = "cari_wwwadresi_sabit_kullan";
		this.cari_wwwadresi_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_wwwadresi_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_wwwadresi_sabit_kullan.TabIndex = 524;
		this.cari_wwwadresi_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_wwwadresi_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label657.Location = new System.Drawing.Point(35, 271);
		this.label657.Name = "label657";
		this.label657.Size = new System.Drawing.Size(143, 19);
		this.label657.TabIndex = 523;
		this.label657.Text = "Web adresi sabit değer :";
		this.label657.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label658.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label658.Location = new System.Drawing.Point(15, 244);
		this.label658.Name = "label658";
		this.label658.Size = new System.Drawing.Size(163, 19);
		this.label658.TabIndex = 522;
		this.label658.Text = "Web adresi";
		this.label658.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_Ana_cari_kodu_sabit_deger.Location = new System.Drawing.Point(583, 160);
		this.cari_Ana_cari_kodu_sabit_deger.Name = "cari_Ana_cari_kodu_sabit_deger";
		this.cari_Ana_cari_kodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_Ana_cari_kodu_sabit_deger.TabIndex = 521;
		this.cari_Ana_cari_kodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_Ana_cari_kodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label650.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label650.Location = new System.Drawing.Point(583, 184);
		this.label650.Name = "label650";
		this.label650.Size = new System.Drawing.Size(96, 19);
		this.label650.TabIndex = 519;
		this.label650.Text = "Sıra";
		this.label650.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_Ana_cari_kodu_baslangic.EditValue = new decimal(new int[4]);
		this.cari_Ana_cari_kodu_baslangic.Location = new System.Drawing.Point(583, 205);
		this.cari_Ana_cari_kodu_baslangic.Name = "cari_Ana_cari_kodu_baslangic";
		this.cari_Ana_cari_kodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_Ana_cari_kodu_baslangic.Properties.IsFloatValue = false;
		this.cari_Ana_cari_kodu_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_Ana_cari_kodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_Ana_cari_kodu_baslangic.TabIndex = 517;
		this.cari_Ana_cari_kodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_Ana_cari_kodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label651.Location = new System.Drawing.Point(437, 205);
		this.label651.Name = "label651";
		this.label651.Size = new System.Drawing.Size(140, 19);
		this.label651.TabIndex = 516;
		this.label651.Text = "Ana cari kodu :";
		this.label651.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_Ana_cari_kodu_sabit_kullan.Location = new System.Drawing.Point(581, 135);
		this.cari_Ana_cari_kodu_sabit_kullan.Name = "cari_Ana_cari_kodu_sabit_kullan";
		this.cari_Ana_cari_kodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_Ana_cari_kodu_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_Ana_cari_kodu_sabit_kullan.TabIndex = 515;
		this.cari_Ana_cari_kodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_Ana_cari_kodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label652.Location = new System.Drawing.Point(434, 160);
		this.label652.Name = "label652";
		this.label652.Size = new System.Drawing.Size(143, 19);
		this.label652.TabIndex = 514;
		this.label652.Text = "Ana cari kodu sabit değer :";
		this.label652.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label653.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label653.Location = new System.Drawing.Point(442, 133);
		this.label653.Name = "label653";
		this.label653.Size = new System.Drawing.Size(135, 19);
		this.label653.TabIndex = 513;
		this.label653.Text = "Ana cari kodu";
		this.label653.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_temsilci_kodu_sabit_deger.Location = new System.Drawing.Point(184, 160);
		this.cari_temsilci_kodu_sabit_deger.Name = "cari_temsilci_kodu_sabit_deger";
		this.cari_temsilci_kodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_temsilci_kodu_sabit_deger.TabIndex = 512;
		this.cari_temsilci_kodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_temsilci_kodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label414.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label414.Location = new System.Drawing.Point(184, 184);
		this.label414.Name = "label414";
		this.label414.Size = new System.Drawing.Size(96, 19);
		this.label414.TabIndex = 510;
		this.label414.Text = "Sıra";
		this.label414.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_temsilci_kodu_baslangic.EditValue = new decimal(new int[4]);
		this.cari_temsilci_kodu_baslangic.Location = new System.Drawing.Point(184, 205);
		this.cari_temsilci_kodu_baslangic.Name = "cari_temsilci_kodu_baslangic";
		this.cari_temsilci_kodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_temsilci_kodu_baslangic.Properties.IsFloatValue = false;
		this.cari_temsilci_kodu_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_temsilci_kodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_temsilci_kodu_baslangic.TabIndex = 508;
		this.cari_temsilci_kodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_temsilci_kodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label415.Location = new System.Drawing.Point(38, 205);
		this.label415.Name = "label415";
		this.label415.Size = new System.Drawing.Size(140, 19);
		this.label415.TabIndex = 507;
		this.label415.Text = "Temsilci kodu :";
		this.label415.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_temsilci_kodu_sabit_kullan.Location = new System.Drawing.Point(185, 135);
		this.cari_temsilci_kodu_sabit_kullan.Name = "cari_temsilci_kodu_sabit_kullan";
		this.cari_temsilci_kodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_temsilci_kodu_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_temsilci_kodu_sabit_kullan.TabIndex = 506;
		this.cari_temsilci_kodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_temsilci_kodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label416.Location = new System.Drawing.Point(35, 160);
		this.label416.Name = "label416";
		this.label416.Size = new System.Drawing.Size(143, 19);
		this.label416.TabIndex = 505;
		this.label416.Text = "Temsilci kodu sabit değer :";
		this.label416.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label648.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label648.Location = new System.Drawing.Point(15, 133);
		this.label648.Name = "label648";
		this.label648.Size = new System.Drawing.Size(163, 19);
		this.label648.TabIndex = 504;
		this.label648.Text = "Temsilci kodu";
		this.label648.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label551.Location = new System.Drawing.Point(15, 26);
		this.label551.Name = "label551";
		this.label551.Size = new System.Drawing.Size(219, 19);
		this.label551.TabIndex = 488;
		this.label551.Text = "Yeni cari hesap açma seçeneği :";
		this.label551.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.otomatik_hesap_acma_secenek_cari.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.otomatik_hesap_acma_secenek_cari.FormattingEnabled = true;
		this.otomatik_hesap_acma_secenek_cari.Location = new System.Drawing.Point(240, 23);
		this.otomatik_hesap_acma_secenek_cari.Name = "otomatik_hesap_acma_secenek_cari";
		this.otomatik_hesap_acma_secenek_cari.Size = new System.Drawing.Size(156, 21);
		this.otomatik_hesap_acma_secenek_cari.TabIndex = 487;
		this.otomatik_hesap_acma_secenek_cari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.otomatik_hesap_acma_secenek_cari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label549.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label549.Location = new System.Drawing.Point(12, 60);
		this.label549.Name = "label549";
		this.label549.Size = new System.Drawing.Size(166, 19);
		this.label549.TabIndex = 486;
		this.label549.Text = "Genel bilgiler";
		this.label549.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_vergi_dairesi_baslangic.EditValue = new decimal(new int[4]);
		this.cari_vergi_dairesi_baslangic.Location = new System.Drawing.Point(166, 92);
		this.cari_vergi_dairesi_baslangic.Name = "cari_vergi_dairesi_baslangic";
		this.cari_vergi_dairesi_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_vergi_dairesi_baslangic.Properties.IsFloatValue = false;
		this.cari_vergi_dairesi_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_vergi_dairesi_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_vergi_dairesi_baslangic.TabIndex = 484;
		this.cari_vergi_dairesi_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_vergi_dairesi_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label221.Location = new System.Drawing.Point(40, 92);
		this.label221.Name = "label221";
		this.label221.Size = new System.Drawing.Size(120, 19);
		this.label221.TabIndex = 483;
		this.label221.Text = "Vergi dairesi :";
		this.label221.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.xtraTabPage41.AutoScroll = true;
		this.xtraTabPage41.AutoScrollMargin = new System.Drawing.Size(0, 60);
		this.xtraTabPage41.Controls.Add(this.label643);
		this.xtraTabPage41.Controls.Add(this.label645);
		this.xtraTabPage41.Controls.Add(this.cari_muhartikeli_baslangic);
		this.xtraTabPage41.Controls.Add(this.label646);
		this.xtraTabPage41.Controls.Add(this.cari_muhartikeli_sabit_kullan);
		this.xtraTabPage41.Controls.Add(this.label647);
		this.xtraTabPage41.Controls.Add(this.label639);
		this.xtraTabPage41.Controls.Add(this.cari_muh_kod2_satis_baslangic);
		this.xtraTabPage41.Controls.Add(this.label640);
		this.xtraTabPage41.Controls.Add(this.cari_muh_kod2_satis_sabit_kullan);
		this.xtraTabPage41.Controls.Add(this.label641);
		this.xtraTabPage41.Controls.Add(this.label642);
		this.xtraTabPage41.Controls.Add(this.label628);
		this.xtraTabPage41.Controls.Add(this.cari_muh_kod1_satis_baslangic);
		this.xtraTabPage41.Controls.Add(this.label629);
		this.xtraTabPage41.Controls.Add(this.cari_muh_kod1_satis_sabit_kullan);
		this.xtraTabPage41.Controls.Add(this.label630);
		this.xtraTabPage41.Controls.Add(this.label631);
		this.xtraTabPage41.Controls.Add(this.label614);
		this.xtraTabPage41.Controls.Add(this.cari_muh_kod_satis_baslangic);
		this.xtraTabPage41.Controls.Add(this.label615);
		this.xtraTabPage41.Controls.Add(this.cari_muh_kod_satis_sabit_kullan);
		this.xtraTabPage41.Controls.Add(this.label616);
		this.xtraTabPage41.Controls.Add(this.cari_muhartikeli);
		this.xtraTabPage41.Controls.Add(this.cari_muh_kod2_satis);
		this.xtraTabPage41.Controls.Add(this.cari_doviz_cinsi2);
		this.xtraTabPage41.Controls.Add(this.cari_muh_kod1_satis);
		this.xtraTabPage41.Controls.Add(this.cari_doviz_cinsi1);
		this.xtraTabPage41.Controls.Add(this.cari_muh_kod_satis);
		this.xtraTabPage41.Controls.Add(this.label411);
		this.xtraTabPage41.Controls.Add(this.cari_doviz_cinsi);
		this.xtraTabPage41.Name = "xtraTabPage41";
		this.xtraTabPage41.Size = new System.Drawing.Size(775, 541);
		this.xtraTabPage41.Text = "Döviz cinsi / Muhasebe kodları";
		this.label643.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label643.Location = new System.Drawing.Point(14, 21);
		this.label643.Name = "label643";
		this.label643.Size = new System.Drawing.Size(267, 19);
		this.label643.TabIndex = 570;
		this.label643.Text = "Muhasebe kod artikeli";
		this.label643.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label645.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label645.Location = new System.Drawing.Point(201, 94);
		this.label645.Name = "label645";
		this.label645.Size = new System.Drawing.Size(96, 19);
		this.label645.TabIndex = 568;
		this.label645.Text = "Sıra";
		this.label645.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_muhartikeli_baslangic.EditValue = new decimal(new int[4]);
		this.cari_muhartikeli_baslangic.Location = new System.Drawing.Point(201, 115);
		this.cari_muhartikeli_baslangic.Name = "cari_muhartikeli_baslangic";
		this.cari_muhartikeli_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_muhartikeli_baslangic.Properties.IsFloatValue = false;
		this.cari_muhartikeli_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_muhartikeli_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_muhartikeli_baslangic.TabIndex = 566;
		this.cari_muhartikeli_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muhartikeli_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label646.Location = new System.Drawing.Point(55, 115);
		this.label646.Name = "label646";
		this.label646.Size = new System.Drawing.Size(140, 19);
		this.label646.TabIndex = 565;
		this.label646.Text = "Muhasebe kod artikeli :";
		this.label646.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_muhartikeli_sabit_kullan.Location = new System.Drawing.Point(199, 43);
		this.cari_muhartikeli_sabit_kullan.Name = "cari_muhartikeli_sabit_kullan";
		this.cari_muhartikeli_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_muhartikeli_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.cari_muhartikeli_sabit_kullan.TabIndex = 564;
		this.cari_muhartikeli_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muhartikeli_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label647.Location = new System.Drawing.Point(12, 70);
		this.label647.Name = "label647";
		this.label647.Size = new System.Drawing.Size(183, 19);
		this.label647.TabIndex = 563;
		this.label647.Text = "Muhasebe kod artikeli sabit değer :";
		this.label647.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label639.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label639.Location = new System.Drawing.Point(192, 544);
		this.label639.Name = "label639";
		this.label639.Size = new System.Drawing.Size(96, 19);
		this.label639.TabIndex = 551;
		this.label639.Text = "Sıra";
		this.label639.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_muh_kod2_satis_baslangic.EditValue = new decimal(new int[4]);
		this.cari_muh_kod2_satis_baslangic.Location = new System.Drawing.Point(192, 565);
		this.cari_muh_kod2_satis_baslangic.Name = "cari_muh_kod2_satis_baslangic";
		this.cari_muh_kod2_satis_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_muh_kod2_satis_baslangic.Properties.IsFloatValue = false;
		this.cari_muh_kod2_satis_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_muh_kod2_satis_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_muh_kod2_satis_baslangic.TabIndex = 549;
		this.cari_muh_kod2_satis_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muh_kod2_satis_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label640.Location = new System.Drawing.Point(46, 565);
		this.label640.Name = "label640";
		this.label640.Size = new System.Drawing.Size(140, 19);
		this.label640.TabIndex = 548;
		this.label640.Text = "Muhasebe kodu :";
		this.label640.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_muh_kod2_satis_sabit_kullan.Location = new System.Drawing.Point(190, 516);
		this.cari_muh_kod2_satis_sabit_kullan.Name = "cari_muh_kod2_satis_sabit_kullan";
		this.cari_muh_kod2_satis_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_muh_kod2_satis_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.cari_muh_kod2_satis_sabit_kullan.TabIndex = 547;
		this.cari_muh_kod2_satis_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muh_kod2_satis_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label641.Location = new System.Drawing.Point(30, 520);
		this.label641.Name = "label641";
		this.label641.Size = new System.Drawing.Size(156, 19);
		this.label641.TabIndex = 546;
		this.label641.Text = "Muhasebe kodu sabit değer :";
		this.label641.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label642.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label642.Location = new System.Drawing.Point(14, 470);
		this.label642.Name = "label642";
		this.label642.Size = new System.Drawing.Size(97, 19);
		this.label642.TabIndex = 545;
		this.label642.Text = "Döviz cinsi 3";
		this.label642.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label628.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label628.Location = new System.Drawing.Point(192, 408);
		this.label628.Name = "label628";
		this.label628.Size = new System.Drawing.Size(96, 19);
		this.label628.TabIndex = 531;
		this.label628.Text = "Sıra";
		this.label628.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_muh_kod1_satis_baslangic.EditValue = new decimal(new int[4]);
		this.cari_muh_kod1_satis_baslangic.Location = new System.Drawing.Point(192, 429);
		this.cari_muh_kod1_satis_baslangic.Name = "cari_muh_kod1_satis_baslangic";
		this.cari_muh_kod1_satis_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_muh_kod1_satis_baslangic.Properties.IsFloatValue = false;
		this.cari_muh_kod1_satis_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_muh_kod1_satis_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_muh_kod1_satis_baslangic.TabIndex = 529;
		this.cari_muh_kod1_satis_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muh_kod1_satis_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label629.Location = new System.Drawing.Point(46, 429);
		this.label629.Name = "label629";
		this.label629.Size = new System.Drawing.Size(140, 19);
		this.label629.TabIndex = 528;
		this.label629.Text = "Muhasebe kodu :";
		this.label629.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_muh_kod1_satis_sabit_kullan.Location = new System.Drawing.Point(190, 357);
		this.cari_muh_kod1_satis_sabit_kullan.Name = "cari_muh_kod1_satis_sabit_kullan";
		this.cari_muh_kod1_satis_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_muh_kod1_satis_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.cari_muh_kod1_satis_sabit_kullan.TabIndex = 527;
		this.cari_muh_kod1_satis_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muh_kod1_satis_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label630.Location = new System.Drawing.Point(30, 384);
		this.label630.Name = "label630";
		this.label630.Size = new System.Drawing.Size(156, 19);
		this.label630.TabIndex = 526;
		this.label630.Text = "Muhasebe kodu sabit değer :";
		this.label630.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label631.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label631.Location = new System.Drawing.Point(14, 311);
		this.label631.Name = "label631";
		this.label631.Size = new System.Drawing.Size(97, 19);
		this.label631.TabIndex = 523;
		this.label631.Text = "Döviz cinsi 2";
		this.label631.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label614.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label614.Location = new System.Drawing.Point(192, 255);
		this.label614.Name = "label614";
		this.label614.Size = new System.Drawing.Size(96, 19);
		this.label614.TabIndex = 509;
		this.label614.Text = "Sıra";
		this.label614.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_muh_kod_satis_baslangic.EditValue = new decimal(new int[4]);
		this.cari_muh_kod_satis_baslangic.Location = new System.Drawing.Point(192, 276);
		this.cari_muh_kod_satis_baslangic.Name = "cari_muh_kod_satis_baslangic";
		this.cari_muh_kod_satis_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_muh_kod_satis_baslangic.Properties.IsFloatValue = false;
		this.cari_muh_kod_satis_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_muh_kod_satis_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_muh_kod_satis_baslangic.TabIndex = 507;
		this.cari_muh_kod_satis_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muh_kod_satis_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label615.Location = new System.Drawing.Point(46, 276);
		this.label615.Name = "label615";
		this.label615.Size = new System.Drawing.Size(140, 19);
		this.label615.TabIndex = 506;
		this.label615.Text = "Muhasebe kodu :";
		this.label615.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_muh_kod_satis_sabit_kullan.Location = new System.Drawing.Point(190, 204);
		this.cari_muh_kod_satis_sabit_kullan.Name = "cari_muh_kod_satis_sabit_kullan";
		this.cari_muh_kod_satis_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_muh_kod_satis_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.cari_muh_kod_satis_sabit_kullan.TabIndex = 505;
		this.cari_muh_kod_satis_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muh_kod_satis_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label616.Location = new System.Drawing.Point(30, 231);
		this.label616.Name = "label616";
		this.label616.Size = new System.Drawing.Size(156, 19);
		this.label616.TabIndex = 504;
		this.label616.Text = "Muhasebe kodu sabit değer :";
		this.label616.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_muhartikeli.Location = new System.Drawing.Point(201, 69);
		this.cari_muhartikeli.Name = "cari_muhartikeli";
		this.cari_muhartikeli.Size = new System.Drawing.Size(156, 20);
		this.cari_muhartikeli.TabIndex = 492;
		this.cari_muhartikeli.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muhartikeli.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_muh_kod2_satis.Location = new System.Drawing.Point(192, 520);
		this.cari_muh_kod2_satis.Name = "cari_muh_kod2_satis";
		this.cari_muh_kod2_satis.Size = new System.Drawing.Size(156, 20);
		this.cari_muh_kod2_satis.TabIndex = 491;
		this.cari_doviz_cinsi2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_doviz_cinsi2.FormattingEnabled = true;
		this.cari_doviz_cinsi2.Location = new System.Drawing.Point(117, 470);
		this.cari_doviz_cinsi2.Name = "cari_doviz_cinsi2";
		this.cari_doviz_cinsi2.Size = new System.Drawing.Size(156, 21);
		this.cari_doviz_cinsi2.TabIndex = 490;
		this.cari_doviz_cinsi2.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_doviz_cinsi2.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_muh_kod1_satis.Location = new System.Drawing.Point(192, 383);
		this.cari_muh_kod1_satis.Name = "cari_muh_kod1_satis";
		this.cari_muh_kod1_satis.Size = new System.Drawing.Size(156, 20);
		this.cari_muh_kod1_satis.TabIndex = 489;
		this.cari_muh_kod1_satis.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muh_kod1_satis.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_doviz_cinsi1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_doviz_cinsi1.FormattingEnabled = true;
		this.cari_doviz_cinsi1.Location = new System.Drawing.Point(117, 311);
		this.cari_doviz_cinsi1.Name = "cari_doviz_cinsi1";
		this.cari_doviz_cinsi1.Size = new System.Drawing.Size(156, 21);
		this.cari_doviz_cinsi1.TabIndex = 488;
		this.cari_doviz_cinsi1.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_doviz_cinsi1.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_muh_kod_satis.Location = new System.Drawing.Point(192, 231);
		this.cari_muh_kod_satis.Name = "cari_muh_kod_satis";
		this.cari_muh_kod_satis.Size = new System.Drawing.Size(156, 20);
		this.cari_muh_kod_satis.TabIndex = 486;
		this.cari_muh_kod_satis.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_muh_kod_satis.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label411.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label411.Location = new System.Drawing.Point(14, 158);
		this.label411.Name = "label411";
		this.label411.Size = new System.Drawing.Size(97, 19);
		this.label411.TabIndex = 484;
		this.label411.Text = "Döviz cinsi 1";
		this.label411.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_doviz_cinsi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cari_doviz_cinsi.FormattingEnabled = true;
		this.cari_doviz_cinsi.Location = new System.Drawing.Point(117, 158);
		this.cari_doviz_cinsi.Name = "cari_doviz_cinsi";
		this.cari_doviz_cinsi.Size = new System.Drawing.Size(156, 21);
		this.cari_doviz_cinsi.TabIndex = 483;
		this.cari_doviz_cinsi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_doviz_cinsi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.xtraTabPage39.Controls.Add(this.label292);
		this.xtraTabPage39.Controls.Add(this.cari_il_bilgisi_plaka_kodu);
		this.xtraTabPage39.Controls.Add(this.cari_telefon_baslangic);
		this.xtraTabPage39.Controls.Add(this.label231);
		this.xtraTabPage39.Controls.Add(this.cari_posta_kodu_baslangic);
		this.xtraTabPage39.Controls.Add(this.label230);
		this.xtraTabPage39.Controls.Add(this.cari_ulke_baslangic);
		this.xtraTabPage39.Controls.Add(this.label229);
		this.xtraTabPage39.Controls.Add(this.label228);
		this.xtraTabPage39.Controls.Add(this.cari_il_baslangic);
		this.xtraTabPage39.Controls.Add(this.label226);
		this.xtraTabPage39.Controls.Add(this.cari_ilce_baslangic);
		this.xtraTabPage39.Controls.Add(this.label225);
		this.xtraTabPage39.Controls.Add(this.cari_mahalle_baslangic);
		this.xtraTabPage39.Controls.Add(this.label224);
		this.xtraTabPage39.Controls.Add(this.cari_adres_baslangic);
		this.xtraTabPage39.Controls.Add(this.label222);
		this.xtraTabPage39.Name = "xtraTabPage39";
		this.xtraTabPage39.Size = new System.Drawing.Size(775, 541);
		this.xtraTabPage39.Text = "Adres bilgisi";
		this.label292.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label292.Location = new System.Drawing.Point(93, 24);
		this.label292.Name = "label292";
		this.label292.Size = new System.Drawing.Size(156, 19);
		this.label292.TabIndex = 471;
		this.label292.Text = "Adres bilgileri";
		this.label292.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_il_bilgisi_plaka_kodu.Location = new System.Drawing.Point(91, 247);
		this.cari_il_bilgisi_plaka_kodu.Name = "cari_il_bilgisi_plaka_kodu";
		this.cari_il_bilgisi_plaka_kodu.Properties.Caption = "İl bilgisi plaka kodu şeklinde, otomatik isim bul";
		this.cari_il_bilgisi_plaka_kodu.Size = new System.Drawing.Size(253, 19);
		this.cari_il_bilgisi_plaka_kodu.TabIndex = 470;
		this.cari_il_bilgisi_plaka_kodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_il_bilgisi_plaka_kodu.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.cari_telefon_baslangic.EditValue = new decimal(new int[4]);
		this.cari_telefon_baslangic.Location = new System.Drawing.Point(93, 195);
		this.cari_telefon_baslangic.Name = "cari_telefon_baslangic";
		this.cari_telefon_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_telefon_baslangic.Properties.IsFloatValue = false;
		this.cari_telefon_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_telefon_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_telefon_baslangic.TabIndex = 468;
		this.cari_telefon_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_telefon_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label231.Location = new System.Drawing.Point(18, 195);
		this.label231.Name = "label231";
		this.label231.Size = new System.Drawing.Size(69, 19);
		this.label231.TabIndex = 467;
		this.label231.Text = "Telefon :";
		this.label231.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_posta_kodu_baslangic.EditValue = new decimal(new int[4]);
		this.cari_posta_kodu_baslangic.Location = new System.Drawing.Point(93, 169);
		this.cari_posta_kodu_baslangic.Name = "cari_posta_kodu_baslangic";
		this.cari_posta_kodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_posta_kodu_baslangic.Properties.IsFloatValue = false;
		this.cari_posta_kodu_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_posta_kodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_posta_kodu_baslangic.TabIndex = 465;
		this.cari_posta_kodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_posta_kodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label230.Location = new System.Drawing.Point(18, 169);
		this.label230.Name = "label230";
		this.label230.Size = new System.Drawing.Size(69, 19);
		this.label230.TabIndex = 464;
		this.label230.Text = "Posta kodu :";
		this.label230.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_ulke_baslangic.EditValue = new decimal(new int[4]);
		this.cari_ulke_baslangic.Location = new System.Drawing.Point(93, 143);
		this.cari_ulke_baslangic.Name = "cari_ulke_baslangic";
		this.cari_ulke_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_ulke_baslangic.Properties.IsFloatValue = false;
		this.cari_ulke_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_ulke_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_ulke_baslangic.TabIndex = 462;
		this.cari_ulke_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_ulke_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label229.Location = new System.Drawing.Point(18, 143);
		this.label229.Name = "label229";
		this.label229.Size = new System.Drawing.Size(69, 19);
		this.label229.TabIndex = 461;
		this.label229.Text = "Ülke :";
		this.label229.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label228.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label228.Location = new System.Drawing.Point(90, 43);
		this.label228.Name = "label228";
		this.label228.Size = new System.Drawing.Size(96, 19);
		this.label228.TabIndex = 459;
		this.label228.Text = "Sıra";
		this.label228.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_il_baslangic.EditValue = new decimal(new int[4]);
		this.cari_il_baslangic.Location = new System.Drawing.Point(93, 221);
		this.cari_il_baslangic.Name = "cari_il_baslangic";
		this.cari_il_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_il_baslangic.Properties.IsFloatValue = false;
		this.cari_il_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_il_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_il_baslangic.TabIndex = 457;
		this.cari_il_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_il_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label226.Location = new System.Drawing.Point(18, 221);
		this.label226.Name = "label226";
		this.label226.Size = new System.Drawing.Size(69, 19);
		this.label226.TabIndex = 456;
		this.label226.Text = "İl :";
		this.label226.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_ilce_baslangic.EditValue = new decimal(new int[4]);
		this.cari_ilce_baslangic.Location = new System.Drawing.Point(93, 117);
		this.cari_ilce_baslangic.Name = "cari_ilce_baslangic";
		this.cari_ilce_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_ilce_baslangic.Properties.IsFloatValue = false;
		this.cari_ilce_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_ilce_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_ilce_baslangic.TabIndex = 454;
		this.cari_ilce_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_ilce_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label225.Location = new System.Drawing.Point(18, 117);
		this.label225.Name = "label225";
		this.label225.Size = new System.Drawing.Size(69, 19);
		this.label225.TabIndex = 453;
		this.label225.Text = "İlçe :";
		this.label225.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_mahalle_baslangic.EditValue = new decimal(new int[4]);
		this.cari_mahalle_baslangic.Location = new System.Drawing.Point(93, 91);
		this.cari_mahalle_baslangic.Name = "cari_mahalle_baslangic";
		this.cari_mahalle_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_mahalle_baslangic.Properties.IsFloatValue = false;
		this.cari_mahalle_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_mahalle_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_mahalle_baslangic.TabIndex = 451;
		this.cari_mahalle_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_mahalle_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label224.Location = new System.Drawing.Point(18, 91);
		this.label224.Name = "label224";
		this.label224.Size = new System.Drawing.Size(69, 19);
		this.label224.TabIndex = 450;
		this.label224.Text = "Mahalle :";
		this.label224.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_adres_baslangic.EditValue = new decimal(new int[4]);
		this.cari_adres_baslangic.Location = new System.Drawing.Point(93, 65);
		this.cari_adres_baslangic.Name = "cari_adres_baslangic";
		this.cari_adres_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_adres_baslangic.Properties.IsFloatValue = false;
		this.cari_adres_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_adres_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_adres_baslangic.TabIndex = 448;
		this.cari_adres_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_adres_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label222.Location = new System.Drawing.Point(18, 65);
		this.label222.Name = "label222";
		this.label222.Size = new System.Drawing.Size(69, 19);
		this.label222.TabIndex = 447;
		this.label222.Text = "Adres :";
		this.label222.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.xtraTabPage31.Controls.Add(this.cari_VarsayilanCikisDepo_sabit_deger);
		this.xtraTabPage31.Controls.Add(this.label696);
		this.xtraTabPage31.Controls.Add(this.cari_VarsayilanCikisDepo_baslangic);
		this.xtraTabPage31.Controls.Add(this.label697);
		this.xtraTabPage31.Controls.Add(this.cari_VarsayilanCikisDepo_sabit_kullan);
		this.xtraTabPage31.Controls.Add(this.label698);
		this.xtraTabPage31.Controls.Add(this.label699);
		this.xtraTabPage31.Controls.Add(this.cari_VarsayilanGirisDepo_sabit_deger);
		this.xtraTabPage31.Controls.Add(this.label701);
		this.xtraTabPage31.Controls.Add(this.cari_VarsayilanGirisDepo_baslangic);
		this.xtraTabPage31.Controls.Add(this.label702);
		this.xtraTabPage31.Controls.Add(this.cari_VarsayilanGirisDepo_sabit_kullan);
		this.xtraTabPage31.Controls.Add(this.label703);
		this.xtraTabPage31.Controls.Add(this.label704);
		this.xtraTabPage31.Controls.Add(this.cari_Portal_PW_sabit_deger);
		this.xtraTabPage31.Controls.Add(this.label685);
		this.xtraTabPage31.Controls.Add(this.cari_Portal_PW_baslangic);
		this.xtraTabPage31.Controls.Add(this.label686);
		this.xtraTabPage31.Controls.Add(this.cari_Portal_PW_sabit_kullan);
		this.xtraTabPage31.Controls.Add(this.label687);
		this.xtraTabPage31.Controls.Add(this.label688);
		this.xtraTabPage31.Controls.Add(this.cari_bolge_kodu_sabit_deger);
		this.xtraTabPage31.Controls.Add(this.label690);
		this.xtraTabPage31.Controls.Add(this.cari_bolge_kodu_baslangic);
		this.xtraTabPage31.Controls.Add(this.label691);
		this.xtraTabPage31.Controls.Add(this.cari_bolge_kodu_sabit_kullan);
		this.xtraTabPage31.Controls.Add(this.label692);
		this.xtraTabPage31.Controls.Add(this.label693);
		this.xtraTabPage31.Controls.Add(this.cari_sektor_kodu_sabit_deger);
		this.xtraTabPage31.Controls.Add(this.label675);
		this.xtraTabPage31.Controls.Add(this.cari_sektor_kodu_baslangic);
		this.xtraTabPage31.Controls.Add(this.label676);
		this.xtraTabPage31.Controls.Add(this.cari_sektor_kodu_sabit_kullan);
		this.xtraTabPage31.Controls.Add(this.label677);
		this.xtraTabPage31.Controls.Add(this.label678);
		this.xtraTabPage31.Controls.Add(this.cari_grup_kodu_sabit_deger);
		this.xtraTabPage31.Controls.Add(this.label680);
		this.xtraTabPage31.Controls.Add(this.cari_grup_kodu_baslangic);
		this.xtraTabPage31.Controls.Add(this.label681);
		this.xtraTabPage31.Controls.Add(this.cari_grup_kodu_sabit_kullan);
		this.xtraTabPage31.Controls.Add(this.label682);
		this.xtraTabPage31.Controls.Add(this.label683);
		this.xtraTabPage31.Name = "xtraTabPage31";
		this.xtraTabPage31.Size = new System.Drawing.Size(775, 541);
		this.xtraTabPage31.Text = "Diğer";
		this.cari_VarsayilanCikisDepo_sabit_deger.Location = new System.Drawing.Point(587, 277);
		this.cari_VarsayilanCikisDepo_sabit_deger.Name = "cari_VarsayilanCikisDepo_sabit_deger";
		this.cari_VarsayilanCikisDepo_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_VarsayilanCikisDepo_sabit_deger.TabIndex = 584;
		this.cari_VarsayilanCikisDepo_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_VarsayilanCikisDepo_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label696.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label696.Location = new System.Drawing.Point(587, 301);
		this.label696.Name = "label696";
		this.label696.Size = new System.Drawing.Size(96, 19);
		this.label696.TabIndex = 582;
		this.label696.Text = "Sıra";
		this.label696.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_VarsayilanCikisDepo_baslangic.EditValue = new decimal(new int[4]);
		this.cari_VarsayilanCikisDepo_baslangic.Location = new System.Drawing.Point(587, 322);
		this.cari_VarsayilanCikisDepo_baslangic.Name = "cari_VarsayilanCikisDepo_baslangic";
		this.cari_VarsayilanCikisDepo_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_VarsayilanCikisDepo_baslangic.Properties.IsFloatValue = false;
		this.cari_VarsayilanCikisDepo_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_VarsayilanCikisDepo_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_VarsayilanCikisDepo_baslangic.TabIndex = 580;
		this.cari_VarsayilanCikisDepo_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_VarsayilanCikisDepo_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label697.Location = new System.Drawing.Point(441, 322);
		this.label697.Name = "label697";
		this.label697.Size = new System.Drawing.Size(140, 19);
		this.label697.TabIndex = 579;
		this.label697.Text = "Varsayılan çıkış depo no :";
		this.label697.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_VarsayilanCikisDepo_sabit_kullan.Location = new System.Drawing.Point(585, 252);
		this.cari_VarsayilanCikisDepo_sabit_kullan.Name = "cari_VarsayilanCikisDepo_sabit_kullan";
		this.cari_VarsayilanCikisDepo_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_VarsayilanCikisDepo_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_VarsayilanCikisDepo_sabit_kullan.TabIndex = 578;
		this.cari_VarsayilanCikisDepo_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_VarsayilanCikisDepo_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label698.Location = new System.Drawing.Point(382, 277);
		this.label698.Name = "label698";
		this.label698.Size = new System.Drawing.Size(199, 19);
		this.label698.TabIndex = 577;
		this.label698.Text = "Varsayılan çıkış depo no sabit değer :";
		this.label698.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label699.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label699.Location = new System.Drawing.Point(410, 250);
		this.label699.Name = "label699";
		this.label699.Size = new System.Drawing.Size(171, 19);
		this.label699.TabIndex = 576;
		this.label699.Text = "Varsayılan çıkış depo no";
		this.label699.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_VarsayilanGirisDepo_sabit_deger.Location = new System.Drawing.Point(220, 277);
		this.cari_VarsayilanGirisDepo_sabit_deger.Name = "cari_VarsayilanGirisDepo_sabit_deger";
		this.cari_VarsayilanGirisDepo_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_VarsayilanGirisDepo_sabit_deger.TabIndex = 575;
		this.cari_VarsayilanGirisDepo_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_VarsayilanGirisDepo_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label701.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label701.Location = new System.Drawing.Point(220, 301);
		this.label701.Name = "label701";
		this.label701.Size = new System.Drawing.Size(96, 19);
		this.label701.TabIndex = 573;
		this.label701.Text = "Sıra";
		this.label701.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_VarsayilanGirisDepo_baslangic.EditValue = new decimal(new int[4]);
		this.cari_VarsayilanGirisDepo_baslangic.Location = new System.Drawing.Point(220, 322);
		this.cari_VarsayilanGirisDepo_baslangic.Name = "cari_VarsayilanGirisDepo_baslangic";
		this.cari_VarsayilanGirisDepo_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_VarsayilanGirisDepo_baslangic.Properties.IsFloatValue = false;
		this.cari_VarsayilanGirisDepo_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_VarsayilanGirisDepo_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_VarsayilanGirisDepo_baslangic.TabIndex = 571;
		this.cari_VarsayilanGirisDepo_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_VarsayilanGirisDepo_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label702.Location = new System.Drawing.Point(74, 322);
		this.label702.Name = "label702";
		this.label702.Size = new System.Drawing.Size(140, 19);
		this.label702.TabIndex = 570;
		this.label702.Text = "Varsayılan giriş depo no :";
		this.label702.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_VarsayilanGirisDepo_sabit_kullan.Location = new System.Drawing.Point(221, 252);
		this.cari_VarsayilanGirisDepo_sabit_kullan.Name = "cari_VarsayilanGirisDepo_sabit_kullan";
		this.cari_VarsayilanGirisDepo_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_VarsayilanGirisDepo_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_VarsayilanGirisDepo_sabit_kullan.TabIndex = 569;
		this.cari_VarsayilanGirisDepo_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_VarsayilanGirisDepo_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label703.Location = new System.Drawing.Point(25, 277);
		this.label703.Name = "label703";
		this.label703.Size = new System.Drawing.Size(189, 19);
		this.label703.TabIndex = 568;
		this.label703.Text = "Varsayılan giriş depo no sabit değer :";
		this.label703.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label704.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label704.Location = new System.Drawing.Point(22, 250);
		this.label704.Name = "label704";
		this.label704.Size = new System.Drawing.Size(192, 19);
		this.label704.TabIndex = 567;
		this.label704.Text = "Varsayılan giriş depo no";
		this.label704.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_Portal_PW_sabit_deger.Location = new System.Drawing.Point(587, 161);
		this.cari_Portal_PW_sabit_deger.Name = "cari_Portal_PW_sabit_deger";
		this.cari_Portal_PW_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_Portal_PW_sabit_deger.TabIndex = 566;
		this.cari_Portal_PW_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_Portal_PW_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label685.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label685.Location = new System.Drawing.Point(587, 185);
		this.label685.Name = "label685";
		this.label685.Size = new System.Drawing.Size(96, 19);
		this.label685.TabIndex = 564;
		this.label685.Text = "Sıra";
		this.label685.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_Portal_PW_baslangic.EditValue = new decimal(new int[4]);
		this.cari_Portal_PW_baslangic.Location = new System.Drawing.Point(587, 206);
		this.cari_Portal_PW_baslangic.Name = "cari_Portal_PW_baslangic";
		this.cari_Portal_PW_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_Portal_PW_baslangic.Properties.IsFloatValue = false;
		this.cari_Portal_PW_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_Portal_PW_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_Portal_PW_baslangic.TabIndex = 562;
		this.cari_Portal_PW_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_Portal_PW_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label686.Location = new System.Drawing.Point(441, 206);
		this.label686.Name = "label686";
		this.label686.Size = new System.Drawing.Size(140, 19);
		this.label686.TabIndex = 561;
		this.label686.Text = "Portal şifresi :";
		this.label686.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_Portal_PW_sabit_kullan.Location = new System.Drawing.Point(585, 136);
		this.cari_Portal_PW_sabit_kullan.Name = "cari_Portal_PW_sabit_kullan";
		this.cari_Portal_PW_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_Portal_PW_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_Portal_PW_sabit_kullan.TabIndex = 560;
		this.cari_Portal_PW_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_Portal_PW_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label687.Location = new System.Drawing.Point(438, 161);
		this.label687.Name = "label687";
		this.label687.Size = new System.Drawing.Size(143, 19);
		this.label687.TabIndex = 559;
		this.label687.Text = "Portal şifresi sabit değer :";
		this.label687.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label688.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label688.Location = new System.Drawing.Point(446, 134);
		this.label688.Name = "label688";
		this.label688.Size = new System.Drawing.Size(135, 19);
		this.label688.TabIndex = 558;
		this.label688.Text = "Portal şifresi";
		this.label688.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_bolge_kodu_sabit_deger.Location = new System.Drawing.Point(220, 160);
		this.cari_bolge_kodu_sabit_deger.Name = "cari_bolge_kodu_sabit_deger";
		this.cari_bolge_kodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_bolge_kodu_sabit_deger.TabIndex = 557;
		this.cari_bolge_kodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_bolge_kodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label690.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label690.Location = new System.Drawing.Point(220, 184);
		this.label690.Name = "label690";
		this.label690.Size = new System.Drawing.Size(96, 19);
		this.label690.TabIndex = 555;
		this.label690.Text = "Sıra";
		this.label690.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_bolge_kodu_baslangic.EditValue = new decimal(new int[4]);
		this.cari_bolge_kodu_baslangic.Location = new System.Drawing.Point(220, 205);
		this.cari_bolge_kodu_baslangic.Name = "cari_bolge_kodu_baslangic";
		this.cari_bolge_kodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_bolge_kodu_baslangic.Properties.IsFloatValue = false;
		this.cari_bolge_kodu_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_bolge_kodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_bolge_kodu_baslangic.TabIndex = 553;
		this.cari_bolge_kodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_bolge_kodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label691.Location = new System.Drawing.Point(74, 205);
		this.label691.Name = "label691";
		this.label691.Size = new System.Drawing.Size(140, 19);
		this.label691.TabIndex = 552;
		this.label691.Text = "Bölge kodu :";
		this.label691.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_bolge_kodu_sabit_kullan.Location = new System.Drawing.Point(221, 135);
		this.cari_bolge_kodu_sabit_kullan.Name = "cari_bolge_kodu_sabit_kullan";
		this.cari_bolge_kodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_bolge_kodu_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_bolge_kodu_sabit_kullan.TabIndex = 551;
		this.cari_bolge_kodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_bolge_kodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label692.Location = new System.Drawing.Point(71, 160);
		this.label692.Name = "label692";
		this.label692.Size = new System.Drawing.Size(143, 19);
		this.label692.TabIndex = 550;
		this.label692.Text = "Bölge kodu sabit değer :";
		this.label692.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label693.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label693.Location = new System.Drawing.Point(51, 133);
		this.label693.Name = "label693";
		this.label693.Size = new System.Drawing.Size(163, 19);
		this.label693.TabIndex = 549;
		this.label693.Text = "Bölge kodu";
		this.label693.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_sektor_kodu_sabit_deger.Location = new System.Drawing.Point(587, 46);
		this.cari_sektor_kodu_sabit_deger.Name = "cari_sektor_kodu_sabit_deger";
		this.cari_sektor_kodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_sektor_kodu_sabit_deger.TabIndex = 539;
		this.cari_sektor_kodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_sektor_kodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label675.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label675.Location = new System.Drawing.Point(587, 70);
		this.label675.Name = "label675";
		this.label675.Size = new System.Drawing.Size(96, 19);
		this.label675.TabIndex = 537;
		this.label675.Text = "Sıra";
		this.label675.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_sektor_kodu_baslangic.EditValue = new decimal(new int[4]);
		this.cari_sektor_kodu_baslangic.Location = new System.Drawing.Point(587, 91);
		this.cari_sektor_kodu_baslangic.Name = "cari_sektor_kodu_baslangic";
		this.cari_sektor_kodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_sektor_kodu_baslangic.Properties.IsFloatValue = false;
		this.cari_sektor_kodu_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_sektor_kodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_sektor_kodu_baslangic.TabIndex = 535;
		this.cari_sektor_kodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_sektor_kodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label676.Location = new System.Drawing.Point(441, 91);
		this.label676.Name = "label676";
		this.label676.Size = new System.Drawing.Size(140, 19);
		this.label676.TabIndex = 534;
		this.label676.Text = "Sektör kodu :";
		this.label676.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_sektor_kodu_sabit_kullan.Location = new System.Drawing.Point(585, 21);
		this.cari_sektor_kodu_sabit_kullan.Name = "cari_sektor_kodu_sabit_kullan";
		this.cari_sektor_kodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_sektor_kodu_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_sektor_kodu_sabit_kullan.TabIndex = 533;
		this.cari_sektor_kodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_sektor_kodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label677.Location = new System.Drawing.Point(438, 46);
		this.label677.Name = "label677";
		this.label677.Size = new System.Drawing.Size(143, 19);
		this.label677.TabIndex = 532;
		this.label677.Text = "Sektör kodu sabit değer :";
		this.label677.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label678.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label678.Location = new System.Drawing.Point(446, 19);
		this.label678.Name = "label678";
		this.label678.Size = new System.Drawing.Size(135, 19);
		this.label678.TabIndex = 531;
		this.label678.Text = "Sektör kodu";
		this.label678.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cari_grup_kodu_sabit_deger.Location = new System.Drawing.Point(220, 45);
		this.cari_grup_kodu_sabit_deger.Name = "cari_grup_kodu_sabit_deger";
		this.cari_grup_kodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.cari_grup_kodu_sabit_deger.TabIndex = 530;
		this.cari_grup_kodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_grup_kodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label680.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label680.Location = new System.Drawing.Point(220, 69);
		this.label680.Name = "label680";
		this.label680.Size = new System.Drawing.Size(96, 19);
		this.label680.TabIndex = 528;
		this.label680.Text = "Sıra";
		this.label680.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.cari_grup_kodu_baslangic.EditValue = new decimal(new int[4]);
		this.cari_grup_kodu_baslangic.Location = new System.Drawing.Point(220, 90);
		this.cari_grup_kodu_baslangic.Name = "cari_grup_kodu_baslangic";
		this.cari_grup_kodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.cari_grup_kodu_baslangic.Properties.IsFloatValue = false;
		this.cari_grup_kodu_baslangic.Properties.Mask.EditMask = "N00";
		this.cari_grup_kodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.cari_grup_kodu_baslangic.TabIndex = 526;
		this.cari_grup_kodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_grup_kodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label681.Location = new System.Drawing.Point(74, 90);
		this.label681.Name = "label681";
		this.label681.Size = new System.Drawing.Size(140, 19);
		this.label681.TabIndex = 525;
		this.label681.Text = "Grup kodu :";
		this.label681.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.cari_grup_kodu_sabit_kullan.Location = new System.Drawing.Point(221, 20);
		this.cari_grup_kodu_sabit_kullan.Name = "cari_grup_kodu_sabit_kullan";
		this.cari_grup_kodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.cari_grup_kodu_sabit_kullan.Size = new System.Drawing.Size(113, 19);
		this.cari_grup_kodu_sabit_kullan.TabIndex = 524;
		this.cari_grup_kodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.cari_grup_kodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label682.Location = new System.Drawing.Point(71, 45);
		this.label682.Name = "label682";
		this.label682.Size = new System.Drawing.Size(143, 19);
		this.label682.TabIndex = 523;
		this.label682.Text = "Grup kodu sabit değer :";
		this.label682.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label683.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label683.Location = new System.Drawing.Point(51, 18);
		this.label683.Name = "label683";
		this.label683.Size = new System.Drawing.Size(163, 19);
		this.label683.TabIndex = 522;
		this.label683.Text = "Grup kodu";
		this.label683.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.xtraTabPage17.AutoScroll = true;
		this.xtraTabPage17.AutoScrollMargin = new System.Drawing.Size(0, 40);
		this.xtraTabPage17.Controls.Add(this.pro_muh_kod_artikeli_sabit_deger);
		this.xtraTabPage17.Controls.Add(this.label600);
		this.xtraTabPage17.Controls.Add(this.pro_muh_kod_artikeli_baslangic);
		this.xtraTabPage17.Controls.Add(this.label601);
		this.xtraTabPage17.Controls.Add(this.pro_muh_kod_artikeli_sabit_kullan);
		this.xtraTabPage17.Controls.Add(this.label602);
		this.xtraTabPage17.Controls.Add(this.label603);
		this.xtraTabPage17.Controls.Add(this.pro_aciklama_sabit_deger);
		this.xtraTabPage17.Controls.Add(this.label595);
		this.xtraTabPage17.Controls.Add(this.pro_aciklama_baslangic);
		this.xtraTabPage17.Controls.Add(this.label596);
		this.xtraTabPage17.Controls.Add(this.pro_aciklama_sabit_kullan);
		this.xtraTabPage17.Controls.Add(this.label597);
		this.xtraTabPage17.Controls.Add(this.label598);
		this.xtraTabPage17.Controls.Add(this.pro_ana_projekodu_sabit_deger);
		this.xtraTabPage17.Controls.Add(this.label590);
		this.xtraTabPage17.Controls.Add(this.pro_ana_projekodu_baslangic);
		this.xtraTabPage17.Controls.Add(this.label591);
		this.xtraTabPage17.Controls.Add(this.pro_ana_projekodu_sabit_kullan);
		this.xtraTabPage17.Controls.Add(this.label592);
		this.xtraTabPage17.Controls.Add(this.label593);
		this.xtraTabPage17.Controls.Add(this.pro_bolgekodu_sabit_deger);
		this.xtraTabPage17.Controls.Add(this.label585);
		this.xtraTabPage17.Controls.Add(this.pro_bolgekodu_baslangic);
		this.xtraTabPage17.Controls.Add(this.label586);
		this.xtraTabPage17.Controls.Add(this.pro_bolgekodu_sabit_kullan);
		this.xtraTabPage17.Controls.Add(this.label587);
		this.xtraTabPage17.Controls.Add(this.label588);
		this.xtraTabPage17.Controls.Add(this.pro_sektorkodu_sabit_deger);
		this.xtraTabPage17.Controls.Add(this.label580);
		this.xtraTabPage17.Controls.Add(this.pro_sektorkodu_baslangic);
		this.xtraTabPage17.Controls.Add(this.label581);
		this.xtraTabPage17.Controls.Add(this.pro_sektorkodu_sabit_kullan);
		this.xtraTabPage17.Controls.Add(this.label582);
		this.xtraTabPage17.Controls.Add(this.label583);
		this.xtraTabPage17.Controls.Add(this.pro_grupkodu_sabit_deger);
		this.xtraTabPage17.Controls.Add(this.label575);
		this.xtraTabPage17.Controls.Add(this.pro_grupkodu_baslangic);
		this.xtraTabPage17.Controls.Add(this.label576);
		this.xtraTabPage17.Controls.Add(this.pro_grupkodu_sabit_kullan);
		this.xtraTabPage17.Controls.Add(this.label577);
		this.xtraTabPage17.Controls.Add(this.label578);
		this.xtraTabPage17.Controls.Add(this.pro_sormerkodu_sabit_deger);
		this.xtraTabPage17.Controls.Add(this.label570);
		this.xtraTabPage17.Controls.Add(this.pro_sormerkodu_baslangic);
		this.xtraTabPage17.Controls.Add(this.label571);
		this.xtraTabPage17.Controls.Add(this.pro_sormerkodu_sabit_kullan);
		this.xtraTabPage17.Controls.Add(this.label572);
		this.xtraTabPage17.Controls.Add(this.label573);
		this.xtraTabPage17.Controls.Add(this.pro_musterikodu_sabit_deger);
		this.xtraTabPage17.Controls.Add(this.label565);
		this.xtraTabPage17.Controls.Add(this.pro_musterikodu_baslangic);
		this.xtraTabPage17.Controls.Add(this.label566);
		this.xtraTabPage17.Controls.Add(this.pro_musterikodu_sabit_kullan);
		this.xtraTabPage17.Controls.Add(this.label567);
		this.xtraTabPage17.Controls.Add(this.label568);
		this.xtraTabPage17.Controls.Add(this.pro_adi_sabit_deger);
		this.xtraTabPage17.Controls.Add(this.label560);
		this.xtraTabPage17.Controls.Add(this.pro_adi_baslangic);
		this.xtraTabPage17.Controls.Add(this.label561);
		this.xtraTabPage17.Controls.Add(this.pro_adi_sabit_kullan);
		this.xtraTabPage17.Controls.Add(this.label562);
		this.xtraTabPage17.Controls.Add(this.label563);
		this.xtraTabPage17.Controls.Add(this.label557);
		this.xtraTabPage17.Controls.Add(this.otomatik_hesap_acma_secenek_proje);
		this.xtraTabPage17.Name = "xtraTabPage17";
		this.xtraTabPage17.Size = new System.Drawing.Size(781, 569);
		this.xtraTabPage17.Text = "Yeni proje bilgileri";
		this.pro_muh_kod_artikeli_sabit_deger.Location = new System.Drawing.Point(191, 699);
		this.pro_muh_kod_artikeli_sabit_deger.Name = "pro_muh_kod_artikeli_sabit_deger";
		this.pro_muh_kod_artikeli_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.pro_muh_kod_artikeli_sabit_deger.TabIndex = 575;
		this.pro_muh_kod_artikeli_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_muh_kod_artikeli_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label600.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label600.Location = new System.Drawing.Point(191, 723);
		this.label600.Name = "label600";
		this.label600.Size = new System.Drawing.Size(96, 19);
		this.label600.TabIndex = 573;
		this.label600.Text = "Sıra";
		this.label600.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pro_muh_kod_artikeli_baslangic.EditValue = new decimal(new int[4]);
		this.pro_muh_kod_artikeli_baslangic.Location = new System.Drawing.Point(191, 744);
		this.pro_muh_kod_artikeli_baslangic.Name = "pro_muh_kod_artikeli_baslangic";
		this.pro_muh_kod_artikeli_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.pro_muh_kod_artikeli_baslangic.Properties.IsFloatValue = false;
		this.pro_muh_kod_artikeli_baslangic.Properties.Mask.EditMask = "N00";
		this.pro_muh_kod_artikeli_baslangic.Size = new System.Drawing.Size(96, 20);
		this.pro_muh_kod_artikeli_baslangic.TabIndex = 571;
		this.pro_muh_kod_artikeli_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_muh_kod_artikeli_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label601.Location = new System.Drawing.Point(45, 744);
		this.label601.Name = "label601";
		this.label601.Size = new System.Drawing.Size(140, 19);
		this.label601.TabIndex = 570;
		this.label601.Text = "Muh. kod artikeli :";
		this.label601.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.pro_muh_kod_artikeli_sabit_kullan.Location = new System.Drawing.Point(189, 672);
		this.pro_muh_kod_artikeli_sabit_kullan.Name = "pro_muh_kod_artikeli_sabit_kullan";
		this.pro_muh_kod_artikeli_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.pro_muh_kod_artikeli_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.pro_muh_kod_artikeli_sabit_kullan.TabIndex = 569;
		this.pro_muh_kod_artikeli_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_muh_kod_artikeli_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label602.Location = new System.Drawing.Point(10, 699);
		this.label602.Name = "label602";
		this.label602.Size = new System.Drawing.Size(175, 19);
		this.label602.TabIndex = 568;
		this.label602.Text = "Muh. kod artikeli sabit değer :";
		this.label602.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label603.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label603.Location = new System.Drawing.Point(67, 650);
		this.label603.Name = "label603";
		this.label603.Size = new System.Drawing.Size(280, 19);
		this.label603.TabIndex = 567;
		this.label603.Text = "Muh. kod artikeli";
		this.label603.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pro_aciklama_sabit_deger.Location = new System.Drawing.Point(582, 555);
		this.pro_aciklama_sabit_deger.Name = "pro_aciklama_sabit_deger";
		this.pro_aciklama_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.pro_aciklama_sabit_deger.TabIndex = 566;
		this.pro_aciklama_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_aciklama_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label595.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label595.Location = new System.Drawing.Point(582, 579);
		this.label595.Name = "label595";
		this.label595.Size = new System.Drawing.Size(96, 19);
		this.label595.TabIndex = 564;
		this.label595.Text = "Sıra";
		this.label595.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pro_aciklama_baslangic.EditValue = new decimal(new int[4]);
		this.pro_aciklama_baslangic.Location = new System.Drawing.Point(582, 600);
		this.pro_aciklama_baslangic.Name = "pro_aciklama_baslangic";
		this.pro_aciklama_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.pro_aciklama_baslangic.Properties.IsFloatValue = false;
		this.pro_aciklama_baslangic.Properties.Mask.EditMask = "N00";
		this.pro_aciklama_baslangic.Size = new System.Drawing.Size(96, 20);
		this.pro_aciklama_baslangic.TabIndex = 562;
		this.pro_aciklama_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_aciklama_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label596.Location = new System.Drawing.Point(436, 600);
		this.label596.Name = "label596";
		this.label596.Size = new System.Drawing.Size(140, 19);
		this.label596.TabIndex = 561;
		this.label596.Text = "Açıklama :";
		this.label596.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.pro_aciklama_sabit_kullan.Location = new System.Drawing.Point(580, 528);
		this.pro_aciklama_sabit_kullan.Name = "pro_aciklama_sabit_kullan";
		this.pro_aciklama_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.pro_aciklama_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.pro_aciklama_sabit_kullan.TabIndex = 560;
		this.pro_aciklama_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_aciklama_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label597.Location = new System.Drawing.Point(401, 555);
		this.label597.Name = "label597";
		this.label597.Size = new System.Drawing.Size(175, 19);
		this.label597.TabIndex = 559;
		this.label597.Text = "Açıklama sabit değer :";
		this.label597.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label598.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label598.Location = new System.Drawing.Point(458, 506);
		this.label598.Name = "label598";
		this.label598.Size = new System.Drawing.Size(280, 19);
		this.label598.TabIndex = 558;
		this.label598.Text = "Açıklama";
		this.label598.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pro_ana_projekodu_sabit_deger.Location = new System.Drawing.Point(191, 555);
		this.pro_ana_projekodu_sabit_deger.Name = "pro_ana_projekodu_sabit_deger";
		this.pro_ana_projekodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.pro_ana_projekodu_sabit_deger.TabIndex = 557;
		this.pro_ana_projekodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_ana_projekodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label590.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label590.Location = new System.Drawing.Point(191, 579);
		this.label590.Name = "label590";
		this.label590.Size = new System.Drawing.Size(96, 19);
		this.label590.TabIndex = 555;
		this.label590.Text = "Sıra";
		this.label590.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pro_ana_projekodu_baslangic.EditValue = new decimal(new int[4]);
		this.pro_ana_projekodu_baslangic.Location = new System.Drawing.Point(191, 600);
		this.pro_ana_projekodu_baslangic.Name = "pro_ana_projekodu_baslangic";
		this.pro_ana_projekodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.pro_ana_projekodu_baslangic.Properties.IsFloatValue = false;
		this.pro_ana_projekodu_baslangic.Properties.Mask.EditMask = "N00";
		this.pro_ana_projekodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.pro_ana_projekodu_baslangic.TabIndex = 553;
		this.pro_ana_projekodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_ana_projekodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label591.Location = new System.Drawing.Point(45, 600);
		this.label591.Name = "label591";
		this.label591.Size = new System.Drawing.Size(140, 19);
		this.label591.TabIndex = 552;
		this.label591.Text = "Ana proje kodu :";
		this.label591.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.pro_ana_projekodu_sabit_kullan.Location = new System.Drawing.Point(189, 528);
		this.pro_ana_projekodu_sabit_kullan.Name = "pro_ana_projekodu_sabit_kullan";
		this.pro_ana_projekodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.pro_ana_projekodu_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.pro_ana_projekodu_sabit_kullan.TabIndex = 551;
		this.pro_ana_projekodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_ana_projekodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label592.Location = new System.Drawing.Point(10, 555);
		this.label592.Name = "label592";
		this.label592.Size = new System.Drawing.Size(175, 19);
		this.label592.TabIndex = 550;
		this.label592.Text = "Ana proje kodu sabit değer :";
		this.label592.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label593.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label593.Location = new System.Drawing.Point(67, 506);
		this.label593.Name = "label593";
		this.label593.Size = new System.Drawing.Size(280, 19);
		this.label593.TabIndex = 549;
		this.label593.Text = "Ana proje kodu";
		this.label593.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pro_bolgekodu_sabit_deger.Location = new System.Drawing.Point(582, 412);
		this.pro_bolgekodu_sabit_deger.Name = "pro_bolgekodu_sabit_deger";
		this.pro_bolgekodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.pro_bolgekodu_sabit_deger.TabIndex = 548;
		this.pro_bolgekodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_bolgekodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label585.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label585.Location = new System.Drawing.Point(582, 436);
		this.label585.Name = "label585";
		this.label585.Size = new System.Drawing.Size(96, 19);
		this.label585.TabIndex = 546;
		this.label585.Text = "Sıra";
		this.label585.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pro_bolgekodu_baslangic.EditValue = new decimal(new int[4]);
		this.pro_bolgekodu_baslangic.Location = new System.Drawing.Point(582, 457);
		this.pro_bolgekodu_baslangic.Name = "pro_bolgekodu_baslangic";
		this.pro_bolgekodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.pro_bolgekodu_baslangic.Properties.IsFloatValue = false;
		this.pro_bolgekodu_baslangic.Properties.Mask.EditMask = "N00";
		this.pro_bolgekodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.pro_bolgekodu_baslangic.TabIndex = 544;
		this.pro_bolgekodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_bolgekodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label586.Location = new System.Drawing.Point(436, 457);
		this.label586.Name = "label586";
		this.label586.Size = new System.Drawing.Size(140, 19);
		this.label586.TabIndex = 543;
		this.label586.Text = "Bölge kodu :";
		this.label586.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.pro_bolgekodu_sabit_kullan.Location = new System.Drawing.Point(580, 385);
		this.pro_bolgekodu_sabit_kullan.Name = "pro_bolgekodu_sabit_kullan";
		this.pro_bolgekodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.pro_bolgekodu_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.pro_bolgekodu_sabit_kullan.TabIndex = 542;
		this.pro_bolgekodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_bolgekodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label587.Location = new System.Drawing.Point(401, 412);
		this.label587.Name = "label587";
		this.label587.Size = new System.Drawing.Size(175, 19);
		this.label587.TabIndex = 541;
		this.label587.Text = "Bölge kodu sabit değer :";
		this.label587.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label588.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label588.Location = new System.Drawing.Point(458, 363);
		this.label588.Name = "label588";
		this.label588.Size = new System.Drawing.Size(280, 19);
		this.label588.TabIndex = 540;
		this.label588.Text = "Bölge kodu";
		this.label588.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pro_sektorkodu_sabit_deger.Location = new System.Drawing.Point(191, 412);
		this.pro_sektorkodu_sabit_deger.Name = "pro_sektorkodu_sabit_deger";
		this.pro_sektorkodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.pro_sektorkodu_sabit_deger.TabIndex = 539;
		this.pro_sektorkodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_sektorkodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label580.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label580.Location = new System.Drawing.Point(191, 436);
		this.label580.Name = "label580";
		this.label580.Size = new System.Drawing.Size(96, 19);
		this.label580.TabIndex = 537;
		this.label580.Text = "Sıra";
		this.label580.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pro_sektorkodu_baslangic.EditValue = new decimal(new int[4]);
		this.pro_sektorkodu_baslangic.Location = new System.Drawing.Point(191, 457);
		this.pro_sektorkodu_baslangic.Name = "pro_sektorkodu_baslangic";
		this.pro_sektorkodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.pro_sektorkodu_baslangic.Properties.IsFloatValue = false;
		this.pro_sektorkodu_baslangic.Properties.Mask.EditMask = "N00";
		this.pro_sektorkodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.pro_sektorkodu_baslangic.TabIndex = 535;
		this.pro_sektorkodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_sektorkodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label581.Location = new System.Drawing.Point(45, 457);
		this.label581.Name = "label581";
		this.label581.Size = new System.Drawing.Size(140, 19);
		this.label581.TabIndex = 534;
		this.label581.Text = "Sektör kodu :";
		this.label581.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.pro_sektorkodu_sabit_kullan.Location = new System.Drawing.Point(189, 385);
		this.pro_sektorkodu_sabit_kullan.Name = "pro_sektorkodu_sabit_kullan";
		this.pro_sektorkodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.pro_sektorkodu_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.pro_sektorkodu_sabit_kullan.TabIndex = 533;
		this.pro_sektorkodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_sektorkodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label582.Location = new System.Drawing.Point(10, 412);
		this.label582.Name = "label582";
		this.label582.Size = new System.Drawing.Size(175, 19);
		this.label582.TabIndex = 532;
		this.label582.Text = "Sektör kodu sabit değer :";
		this.label582.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label583.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label583.Location = new System.Drawing.Point(67, 363);
		this.label583.Name = "label583";
		this.label583.Size = new System.Drawing.Size(280, 19);
		this.label583.TabIndex = 531;
		this.label583.Text = "Sektör kodu";
		this.label583.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pro_grupkodu_sabit_deger.Location = new System.Drawing.Point(582, 260);
		this.pro_grupkodu_sabit_deger.Name = "pro_grupkodu_sabit_deger";
		this.pro_grupkodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.pro_grupkodu_sabit_deger.TabIndex = 530;
		this.pro_grupkodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_grupkodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label575.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label575.Location = new System.Drawing.Point(582, 284);
		this.label575.Name = "label575";
		this.label575.Size = new System.Drawing.Size(96, 19);
		this.label575.TabIndex = 528;
		this.label575.Text = "Sıra";
		this.label575.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pro_grupkodu_baslangic.EditValue = new decimal(new int[4]);
		this.pro_grupkodu_baslangic.Location = new System.Drawing.Point(582, 305);
		this.pro_grupkodu_baslangic.Name = "pro_grupkodu_baslangic";
		this.pro_grupkodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.pro_grupkodu_baslangic.Properties.IsFloatValue = false;
		this.pro_grupkodu_baslangic.Properties.Mask.EditMask = "N00";
		this.pro_grupkodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.pro_grupkodu_baslangic.TabIndex = 526;
		this.pro_grupkodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_grupkodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label576.Location = new System.Drawing.Point(436, 305);
		this.label576.Name = "label576";
		this.label576.Size = new System.Drawing.Size(140, 19);
		this.label576.TabIndex = 525;
		this.label576.Text = "Grup kodu :";
		this.label576.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.pro_grupkodu_sabit_kullan.Location = new System.Drawing.Point(580, 233);
		this.pro_grupkodu_sabit_kullan.Name = "pro_grupkodu_sabit_kullan";
		this.pro_grupkodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.pro_grupkodu_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.pro_grupkodu_sabit_kullan.TabIndex = 524;
		this.pro_grupkodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_grupkodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label577.Location = new System.Drawing.Point(401, 260);
		this.label577.Name = "label577";
		this.label577.Size = new System.Drawing.Size(175, 19);
		this.label577.TabIndex = 523;
		this.label577.Text = "Grup kodu sabit değer :";
		this.label577.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label578.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label578.Location = new System.Drawing.Point(458, 211);
		this.label578.Name = "label578";
		this.label578.Size = new System.Drawing.Size(280, 19);
		this.label578.TabIndex = 522;
		this.label578.Text = "Grup kodu";
		this.label578.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pro_sormerkodu_sabit_deger.Location = new System.Drawing.Point(191, 260);
		this.pro_sormerkodu_sabit_deger.Name = "pro_sormerkodu_sabit_deger";
		this.pro_sormerkodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.pro_sormerkodu_sabit_deger.TabIndex = 521;
		this.pro_sormerkodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_sormerkodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label570.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label570.Location = new System.Drawing.Point(191, 284);
		this.label570.Name = "label570";
		this.label570.Size = new System.Drawing.Size(96, 19);
		this.label570.TabIndex = 519;
		this.label570.Text = "Sıra";
		this.label570.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pro_sormerkodu_baslangic.EditValue = new decimal(new int[4]);
		this.pro_sormerkodu_baslangic.Location = new System.Drawing.Point(191, 305);
		this.pro_sormerkodu_baslangic.Name = "pro_sormerkodu_baslangic";
		this.pro_sormerkodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.pro_sormerkodu_baslangic.Properties.IsFloatValue = false;
		this.pro_sormerkodu_baslangic.Properties.Mask.EditMask = "N00";
		this.pro_sormerkodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.pro_sormerkodu_baslangic.TabIndex = 517;
		this.pro_sormerkodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_sormerkodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label571.Location = new System.Drawing.Point(45, 305);
		this.label571.Name = "label571";
		this.label571.Size = new System.Drawing.Size(140, 19);
		this.label571.TabIndex = 516;
		this.label571.Text = "Proje sor. mer. kodu :";
		this.label571.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.pro_sormerkodu_sabit_kullan.Location = new System.Drawing.Point(189, 233);
		this.pro_sormerkodu_sabit_kullan.Name = "pro_sormerkodu_sabit_kullan";
		this.pro_sormerkodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.pro_sormerkodu_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.pro_sormerkodu_sabit_kullan.TabIndex = 515;
		this.pro_sormerkodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_sormerkodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label572.Location = new System.Drawing.Point(10, 260);
		this.label572.Name = "label572";
		this.label572.Size = new System.Drawing.Size(175, 19);
		this.label572.TabIndex = 514;
		this.label572.Text = "Proje sor. mer. kodu sabit değer :";
		this.label572.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label573.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label573.Location = new System.Drawing.Point(67, 211);
		this.label573.Name = "label573";
		this.label573.Size = new System.Drawing.Size(280, 19);
		this.label573.TabIndex = 513;
		this.label573.Text = "Proje sor. mer. kodu";
		this.label573.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pro_musterikodu_sabit_deger.Location = new System.Drawing.Point(582, 118);
		this.pro_musterikodu_sabit_deger.Name = "pro_musterikodu_sabit_deger";
		this.pro_musterikodu_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.pro_musterikodu_sabit_deger.TabIndex = 512;
		this.pro_musterikodu_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_musterikodu_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label565.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label565.Location = new System.Drawing.Point(582, 142);
		this.label565.Name = "label565";
		this.label565.Size = new System.Drawing.Size(96, 19);
		this.label565.TabIndex = 510;
		this.label565.Text = "Sıra";
		this.label565.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pro_musterikodu_baslangic.EditValue = new decimal(new int[4]);
		this.pro_musterikodu_baslangic.Location = new System.Drawing.Point(582, 163);
		this.pro_musterikodu_baslangic.Name = "pro_musterikodu_baslangic";
		this.pro_musterikodu_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.pro_musterikodu_baslangic.Properties.IsFloatValue = false;
		this.pro_musterikodu_baslangic.Properties.Mask.EditMask = "N00";
		this.pro_musterikodu_baslangic.Size = new System.Drawing.Size(96, 20);
		this.pro_musterikodu_baslangic.TabIndex = 508;
		this.pro_musterikodu_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_musterikodu_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label566.Location = new System.Drawing.Point(436, 163);
		this.label566.Name = "label566";
		this.label566.Size = new System.Drawing.Size(140, 19);
		this.label566.TabIndex = 507;
		this.label566.Text = "Proje cari kodu :";
		this.label566.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.pro_musterikodu_sabit_kullan.Location = new System.Drawing.Point(580, 91);
		this.pro_musterikodu_sabit_kullan.Name = "pro_musterikodu_sabit_kullan";
		this.pro_musterikodu_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.pro_musterikodu_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.pro_musterikodu_sabit_kullan.TabIndex = 506;
		this.pro_musterikodu_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_musterikodu_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label567.Location = new System.Drawing.Point(433, 118);
		this.label567.Name = "label567";
		this.label567.Size = new System.Drawing.Size(143, 19);
		this.label567.TabIndex = 505;
		this.label567.Text = "Proje cari kodu sabit değer :";
		this.label567.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label568.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label568.Location = new System.Drawing.Point(458, 69);
		this.label568.Name = "label568";
		this.label568.Size = new System.Drawing.Size(280, 19);
		this.label568.TabIndex = 504;
		this.label568.Text = "Proje cari kodu";
		this.label568.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pro_adi_sabit_deger.Location = new System.Drawing.Point(191, 118);
		this.pro_adi_sabit_deger.Name = "pro_adi_sabit_deger";
		this.pro_adi_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.pro_adi_sabit_deger.TabIndex = 503;
		this.pro_adi_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_adi_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label560.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label560.Location = new System.Drawing.Point(191, 142);
		this.label560.Name = "label560";
		this.label560.Size = new System.Drawing.Size(96, 19);
		this.label560.TabIndex = 501;
		this.label560.Text = "Sıra";
		this.label560.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pro_adi_baslangic.EditValue = new decimal(new int[4]);
		this.pro_adi_baslangic.Location = new System.Drawing.Point(191, 163);
		this.pro_adi_baslangic.Name = "pro_adi_baslangic";
		this.pro_adi_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.pro_adi_baslangic.Properties.IsFloatValue = false;
		this.pro_adi_baslangic.Properties.Mask.EditMask = "N00";
		this.pro_adi_baslangic.Size = new System.Drawing.Size(96, 20);
		this.pro_adi_baslangic.TabIndex = 499;
		this.pro_adi_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_adi_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label561.Location = new System.Drawing.Point(45, 163);
		this.label561.Name = "label561";
		this.label561.Size = new System.Drawing.Size(140, 19);
		this.label561.TabIndex = 498;
		this.label561.Text = "Proje adı :";
		this.label561.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.pro_adi_sabit_kullan.Location = new System.Drawing.Point(189, 91);
		this.pro_adi_sabit_kullan.Name = "pro_adi_sabit_kullan";
		this.pro_adi_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.pro_adi_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.pro_adi_sabit_kullan.TabIndex = 497;
		this.pro_adi_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.pro_adi_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label562.Location = new System.Drawing.Point(42, 118);
		this.label562.Name = "label562";
		this.label562.Size = new System.Drawing.Size(143, 19);
		this.label562.TabIndex = 496;
		this.label562.Text = "Proje adı sabit değer :";
		this.label562.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label563.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label563.Location = new System.Drawing.Point(67, 69);
		this.label563.Name = "label563";
		this.label563.Size = new System.Drawing.Size(280, 19);
		this.label563.TabIndex = 495;
		this.label563.Text = "Proje adı";
		this.label563.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label557.Location = new System.Drawing.Point(103, 19);
		this.label557.Name = "label557";
		this.label557.Size = new System.Drawing.Size(219, 19);
		this.label557.TabIndex = 494;
		this.label557.Text = "Yeni proje hesap açma seçeneği :";
		this.label557.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.otomatik_hesap_acma_secenek_proje.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.otomatik_hesap_acma_secenek_proje.FormattingEnabled = true;
		this.otomatik_hesap_acma_secenek_proje.Location = new System.Drawing.Point(328, 16);
		this.otomatik_hesap_acma_secenek_proje.Name = "otomatik_hesap_acma_secenek_proje";
		this.otomatik_hesap_acma_secenek_proje.Size = new System.Drawing.Size(156, 21);
		this.otomatik_hesap_acma_secenek_proje.TabIndex = 493;
		this.otomatik_hesap_acma_secenek_proje.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.otomatik_hesap_acma_secenek_proje.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.xtraTabPage40.Controls.Add(this.som_MuhArtikeli_sabit_deger);
		this.xtraTabPage40.Controls.Add(this.label605);
		this.xtraTabPage40.Controls.Add(this.som_MuhArtikeli_baslangic);
		this.xtraTabPage40.Controls.Add(this.label606);
		this.xtraTabPage40.Controls.Add(this.som_MuhArtikeli_sabit_kullan);
		this.xtraTabPage40.Controls.Add(this.label607);
		this.xtraTabPage40.Controls.Add(this.label608);
		this.xtraTabPage40.Controls.Add(this.som_isim_sabit_deger);
		this.xtraTabPage40.Controls.Add(this.label610);
		this.xtraTabPage40.Controls.Add(this.som_isim_baslangic);
		this.xtraTabPage40.Controls.Add(this.label611);
		this.xtraTabPage40.Controls.Add(this.som_isim_sabit_kullan);
		this.xtraTabPage40.Controls.Add(this.label612);
		this.xtraTabPage40.Controls.Add(this.label613);
		this.xtraTabPage40.Controls.Add(this.label558);
		this.xtraTabPage40.Controls.Add(this.otomatik_hesap_acma_secenek_sorumluluk);
		this.xtraTabPage40.Name = "xtraTabPage40";
		this.xtraTabPage40.Size = new System.Drawing.Size(781, 569);
		this.xtraTabPage40.Text = "Yeni sorumluluk merkezi bilgileri";
		this.som_MuhArtikeli_sabit_deger.Location = new System.Drawing.Point(560, 118);
		this.som_MuhArtikeli_sabit_deger.Name = "som_MuhArtikeli_sabit_deger";
		this.som_MuhArtikeli_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.som_MuhArtikeli_sabit_deger.TabIndex = 530;
		this.som_MuhArtikeli_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.som_MuhArtikeli_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label605.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label605.Location = new System.Drawing.Point(560, 142);
		this.label605.Name = "label605";
		this.label605.Size = new System.Drawing.Size(96, 19);
		this.label605.TabIndex = 528;
		this.label605.Text = "Sıra";
		this.label605.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.som_MuhArtikeli_baslangic.EditValue = new decimal(new int[4]);
		this.som_MuhArtikeli_baslangic.Location = new System.Drawing.Point(560, 163);
		this.som_MuhArtikeli_baslangic.Name = "som_MuhArtikeli_baslangic";
		this.som_MuhArtikeli_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.som_MuhArtikeli_baslangic.Properties.IsFloatValue = false;
		this.som_MuhArtikeli_baslangic.Properties.Mask.EditMask = "N00";
		this.som_MuhArtikeli_baslangic.Size = new System.Drawing.Size(96, 20);
		this.som_MuhArtikeli_baslangic.TabIndex = 526;
		this.som_MuhArtikeli_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.som_MuhArtikeli_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label606.Location = new System.Drawing.Point(414, 163);
		this.label606.Name = "label606";
		this.label606.Size = new System.Drawing.Size(140, 19);
		this.label606.TabIndex = 525;
		this.label606.Text = "Muh. hesap kod artikeli :";
		this.label606.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.som_MuhArtikeli_sabit_kullan.Location = new System.Drawing.Point(558, 91);
		this.som_MuhArtikeli_sabit_kullan.Name = "som_MuhArtikeli_sabit_kullan";
		this.som_MuhArtikeli_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.som_MuhArtikeli_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.som_MuhArtikeli_sabit_kullan.TabIndex = 524;
		this.som_MuhArtikeli_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.som_MuhArtikeli_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label607.Location = new System.Drawing.Point(355, 118);
		this.label607.Name = "label607";
		this.label607.Size = new System.Drawing.Size(199, 19);
		this.label607.TabIndex = 523;
		this.label607.Text = "Muh. hesap kod artikeli sabit değer :";
		this.label607.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label608.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label608.Location = new System.Drawing.Point(436, 69);
		this.label608.Name = "label608";
		this.label608.Size = new System.Drawing.Size(280, 19);
		this.label608.TabIndex = 522;
		this.label608.Text = "Muh. hesap kod artikeli";
		this.label608.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.som_isim_sabit_deger.Location = new System.Drawing.Point(169, 118);
		this.som_isim_sabit_deger.Name = "som_isim_sabit_deger";
		this.som_isim_sabit_deger.Size = new System.Drawing.Size(156, 20);
		this.som_isim_sabit_deger.TabIndex = 521;
		this.som_isim_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.som_isim_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label610.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label610.Location = new System.Drawing.Point(169, 142);
		this.label610.Name = "label610";
		this.label610.Size = new System.Drawing.Size(96, 19);
		this.label610.TabIndex = 519;
		this.label610.Text = "Sıra";
		this.label610.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.som_isim_baslangic.EditValue = new decimal(new int[4]);
		this.som_isim_baslangic.Location = new System.Drawing.Point(169, 163);
		this.som_isim_baslangic.Name = "som_isim_baslangic";
		this.som_isim_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.som_isim_baslangic.Properties.IsFloatValue = false;
		this.som_isim_baslangic.Properties.Mask.EditMask = "N00";
		this.som_isim_baslangic.Size = new System.Drawing.Size(96, 20);
		this.som_isim_baslangic.TabIndex = 517;
		this.som_isim_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.som_isim_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label611.Location = new System.Drawing.Point(23, 163);
		this.label611.Name = "label611";
		this.label611.Size = new System.Drawing.Size(140, 19);
		this.label611.TabIndex = 516;
		this.label611.Text = "Sor. mer. adı :";
		this.label611.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.som_isim_sabit_kullan.Location = new System.Drawing.Point(167, 91);
		this.som_isim_sabit_kullan.Name = "som_isim_sabit_kullan";
		this.som_isim_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.som_isim_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.som_isim_sabit_kullan.TabIndex = 515;
		this.som_isim_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.som_isim_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label612.Location = new System.Drawing.Point(20, 118);
		this.label612.Name = "label612";
		this.label612.Size = new System.Drawing.Size(143, 19);
		this.label612.TabIndex = 514;
		this.label612.Text = "Sor. mer. adı sabit değer :";
		this.label612.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label613.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label613.Location = new System.Drawing.Point(45, 69);
		this.label613.Name = "label613";
		this.label613.Size = new System.Drawing.Size(280, 19);
		this.label613.TabIndex = 513;
		this.label613.Text = "Sorumluluk merkezi adı";
		this.label613.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label558.Location = new System.Drawing.Point(50, 18);
		this.label558.Name = "label558";
		this.label558.Size = new System.Drawing.Size(243, 19);
		this.label558.TabIndex = 496;
		this.label558.Text = "Yeni sorumluluk merkezi hesap açma seçeneği :";
		this.label558.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.otomatik_hesap_acma_secenek_sorumluluk.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.otomatik_hesap_acma_secenek_sorumluluk.FormattingEnabled = true;
		this.otomatik_hesap_acma_secenek_sorumluluk.Location = new System.Drawing.Point(299, 15);
		this.otomatik_hesap_acma_secenek_sorumluluk.Name = "otomatik_hesap_acma_secenek_sorumluluk";
		this.otomatik_hesap_acma_secenek_sorumluluk.Size = new System.Drawing.Size(156, 21);
		this.otomatik_hesap_acma_secenek_sorumluluk.TabIndex = 495;
		this.otomatik_hesap_acma_secenek_sorumluluk.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.otomatik_hesap_acma_secenek_sorumluluk.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.xtraTabPage20.Controls.Add(this.xtraTabControl4);
		this.xtraTabPage20.Name = "xtraTabPage20";
		this.xtraTabPage20.Size = new System.Drawing.Size(787, 597);
		this.xtraTabPage20.Text = "Satır bilgileri";
		this.xtraTabControl4.Dock = System.Windows.Forms.DockStyle.Top;
		this.xtraTabControl4.Location = new System.Drawing.Point(0, 0);
		this.xtraTabControl4.Name = "xtraTabControl4";
		this.xtraTabControl4.SelectedTabPage = this.xtraTabPage21;
		this.xtraTabControl4.Size = new System.Drawing.Size(787, 568);
		this.xtraTabControl4.TabIndex = 0;
		this.xtraTabControl4.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[3] { this.xtraTabPage21, this.xtraTabPage7, this.xtraTabPage2 });
		this.xtraTabPage21.AutoScroll = true;
		this.xtraTabPage21.AutoScrollMargin = new System.Drawing.Size(0, 50);
		this.xtraTabPage21.Controls.Add(this.label35);
		this.xtraTabPage21.Controls.Add(this.satir_cinsi_veri_giden_havale);
		this.xtraTabPage21.Controls.Add(this.label34);
		this.xtraTabPage21.Controls.Add(this.satir_cinsi_veri_gelen_havale);
		this.xtraTabPage21.Controls.Add(this.satir_sorumlulukmerkezi_sabit_deger);
		this.xtraTabPage21.Controls.Add(this.label24);
		this.xtraTabPage21.Controls.Add(this.satir_sorumlulukmerkezi_baslangic);
		this.xtraTabPage21.Controls.Add(this.label26);
		this.xtraTabPage21.Controls.Add(this.satir_sorumlulukmerkezi_sabit_kullan);
		this.xtraTabPage21.Controls.Add(this.label30);
		this.xtraTabPage21.Controls.Add(this.label33);
		this.xtraTabPage21.Controls.Add(this.satir_vadesi_sabit_deger);
		this.xtraTabPage21.Controls.Add(this.label20);
		this.xtraTabPage21.Controls.Add(this.satir_vadesi_baslangic);
		this.xtraTabPage21.Controls.Add(this.label21);
		this.xtraTabPage21.Controls.Add(this.satir_vadesi_sabit_kullan);
		this.xtraTabPage21.Controls.Add(this.label22);
		this.xtraTabPage21.Controls.Add(this.label23);
		this.xtraTabPage21.Controls.Add(this.satir_tutar_sabit_deger);
		this.xtraTabPage21.Controls.Add(this.satir_tutar_sabit_kullan);
		this.xtraTabPage21.Controls.Add(this.label5);
		this.xtraTabPage21.Controls.Add(this.satir_tutar_islem_2_islem_tipi);
		this.xtraTabPage21.Controls.Add(this.satir_tutar_islem_2_baslangic);
		this.xtraTabPage21.Controls.Add(this.satir_tutar_islem_1_islem_tipi);
		this.xtraTabPage21.Controls.Add(this.satir_tutar_islem_1_baslangic);
		this.xtraTabPage21.Controls.Add(this.label275);
		this.xtraTabPage21.Controls.Add(this.label47);
		this.xtraTabPage21.Controls.Add(this.satir_tutar_baslangic);
		this.xtraTabPage21.Controls.Add(this.label184);
		this.xtraTabPage21.Controls.Add(this.label18);
		this.xtraTabPage21.Controls.Add(this.satir_cinsi_veri_kredi_karti);
		this.xtraTabPage21.Controls.Add(this.label14);
		this.xtraTabPage21.Controls.Add(this.satir_cinsi_veri_senet);
		this.xtraTabPage21.Controls.Add(this.label303);
		this.xtraTabPage21.Controls.Add(this.satir_aciklama_on_ek_deger);
		this.xtraTabPage21.Controls.Add(this.satir_aciklama_on_ek_kullan);
		this.xtraTabPage21.Controls.Add(this.satir_aciklama_sabit_deger);
		this.xtraTabPage21.Controls.Add(this.label19);
		this.xtraTabPage21.Controls.Add(this.satir_aciklama_baslangic);
		this.xtraTabPage21.Controls.Add(this.label127);
		this.xtraTabPage21.Controls.Add(this.satir_aciklama_sabit_kullan);
		this.xtraTabPage21.Controls.Add(this.label128);
		this.xtraTabPage21.Controls.Add(this.label129);
		this.xtraTabPage21.Controls.Add(this.label25);
		this.xtraTabPage21.Controls.Add(this.label83);
		this.xtraTabPage21.Controls.Add(this.satir_cinsi_veri_cek);
		this.xtraTabPage21.Controls.Add(this.label84);
		this.xtraTabPage21.Controls.Add(this.satir_cinsi_veri_nakit);
		this.xtraTabPage21.Controls.Add(this.label86);
		this.xtraTabPage21.Controls.Add(this.satir_cinsi_baslangic);
		this.xtraTabPage21.Controls.Add(this.label87);
		this.xtraTabPage21.Controls.Add(this.satir_cinsi_sabit_kullan);
		this.xtraTabPage21.Controls.Add(this.label88);
		this.xtraTabPage21.Controls.Add(this.satir_cinsi_sabit_deger);
		this.xtraTabPage21.Controls.Add(this.label81);
		this.xtraTabPage21.Name = "xtraTabPage21";
		this.xtraTabPage21.Size = new System.Drawing.Size(781, 540);
		this.xtraTabPage21.Text = "Hesap bilgileri";
		this.label35.Location = new System.Drawing.Point(371, 195);
		this.label35.Name = "label35";
		this.label35.Size = new System.Drawing.Size(120, 19);
		this.label35.TabIndex = 509;
		this.label35.Text = "Giden havale :";
		this.label35.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_cinsi_veri_giden_havale.Location = new System.Drawing.Point(497, 195);
		this.satir_cinsi_veri_giden_havale.Name = "satir_cinsi_veri_giden_havale";
		this.satir_cinsi_veri_giden_havale.Size = new System.Drawing.Size(156, 20);
		this.satir_cinsi_veri_giden_havale.TabIndex = 508;
		this.satir_cinsi_veri_giden_havale.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_cinsi_veri_giden_havale.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label34.Location = new System.Drawing.Point(371, 169);
		this.label34.Name = "label34";
		this.label34.Size = new System.Drawing.Size(120, 19);
		this.label34.TabIndex = 507;
		this.label34.Text = "Gelen havale :";
		this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_cinsi_veri_gelen_havale.Location = new System.Drawing.Point(497, 169);
		this.satir_cinsi_veri_gelen_havale.Name = "satir_cinsi_veri_gelen_havale";
		this.satir_cinsi_veri_gelen_havale.Size = new System.Drawing.Size(156, 20);
		this.satir_cinsi_veri_gelen_havale.TabIndex = 506;
		this.satir_cinsi_veri_gelen_havale.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_cinsi_veri_gelen_havale.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_sorumlulukmerkezi_sabit_deger.Location = new System.Drawing.Point(659, 252);
		this.satir_sorumlulukmerkezi_sabit_deger.Name = "satir_sorumlulukmerkezi_sabit_deger";
		this.satir_sorumlulukmerkezi_sabit_deger.Size = new System.Drawing.Size(84, 20);
		this.satir_sorumlulukmerkezi_sabit_deger.TabIndex = 505;
		this.satir_sorumlulukmerkezi_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_sorumlulukmerkezi_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label24.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label24.Location = new System.Drawing.Point(526, 281);
		this.label24.Name = "label24";
		this.label24.Size = new System.Drawing.Size(96, 19);
		this.label24.TabIndex = 504;
		this.label24.Text = "Sıra";
		this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.satir_sorumlulukmerkezi_baslangic.EditValue = new decimal(new int[4]);
		this.satir_sorumlulukmerkezi_baslangic.Location = new System.Drawing.Point(526, 302);
		this.satir_sorumlulukmerkezi_baslangic.Name = "satir_sorumlulukmerkezi_baslangic";
		this.satir_sorumlulukmerkezi_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_sorumlulukmerkezi_baslangic.Properties.IsFloatValue = false;
		this.satir_sorumlulukmerkezi_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_sorumlulukmerkezi_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_sorumlulukmerkezi_baslangic.TabIndex = 503;
		this.satir_sorumlulukmerkezi_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_sorumlulukmerkezi_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label26.Location = new System.Drawing.Point(371, 302);
		this.label26.Name = "label26";
		this.label26.Size = new System.Drawing.Size(149, 19);
		this.label26.TabIndex = 502;
		this.label26.Text = "Sorumluluk merkezi değeri :";
		this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_sorumlulukmerkezi_sabit_kullan.Location = new System.Drawing.Point(374, 253);
		this.satir_sorumlulukmerkezi_sabit_kullan.Name = "satir_sorumlulukmerkezi_sabit_kullan";
		this.satir_sorumlulukmerkezi_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.satir_sorumlulukmerkezi_sabit_kullan.Size = new System.Drawing.Size(111, 19);
		this.satir_sorumlulukmerkezi_sabit_kullan.TabIndex = 501;
		this.satir_sorumlulukmerkezi_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_sorumlulukmerkezi_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label30.Location = new System.Drawing.Point(491, 252);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(162, 19);
		this.label30.TabIndex = 500;
		this.label30.Text = "Sorumluluk merkezi sabit değer :";
		this.label30.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label33.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label33.Location = new System.Drawing.Point(371, 228);
		this.label33.Name = "label33";
		this.label33.Size = new System.Drawing.Size(164, 19);
		this.label33.TabIndex = 499;
		this.label33.Text = "Sorumluluk merkezi";
		this.label33.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.satir_vadesi_sabit_deger.Location = new System.Drawing.Point(599, 406);
		this.satir_vadesi_sabit_deger.Name = "satir_vadesi_sabit_deger";
		this.satir_vadesi_sabit_deger.Size = new System.Drawing.Size(86, 20);
		this.satir_vadesi_sabit_deger.TabIndex = 498;
		this.satir_vadesi_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_vadesi_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label20.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label20.Location = new System.Drawing.Point(500, 434);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(96, 19);
		this.label20.TabIndex = 497;
		this.label20.Text = "Sıra";
		this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.satir_vadesi_baslangic.EditValue = new decimal(new int[4]);
		this.satir_vadesi_baslangic.Location = new System.Drawing.Point(500, 455);
		this.satir_vadesi_baslangic.Name = "satir_vadesi_baslangic";
		this.satir_vadesi_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_vadesi_baslangic.Properties.IsFloatValue = false;
		this.satir_vadesi_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_vadesi_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_vadesi_baslangic.TabIndex = 496;
		this.satir_vadesi_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_vadesi_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label21.Location = new System.Drawing.Point(374, 455);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(120, 19);
		this.label21.TabIndex = 495;
		this.label21.Text = "Vade değeri :";
		this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_vadesi_sabit_kullan.Location = new System.Drawing.Point(374, 407);
		this.satir_vadesi_sabit_kullan.Name = "satir_vadesi_sabit_kullan";
		this.satir_vadesi_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.satir_vadesi_sabit_kullan.Size = new System.Drawing.Size(111, 19);
		this.satir_vadesi_sabit_kullan.TabIndex = 494;
		this.satir_vadesi_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_vadesi_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label22.Location = new System.Drawing.Point(492, 406);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(101, 19);
		this.label22.TabIndex = 493;
		this.label22.Text = "Vade sabit değer :";
		this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label23.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label23.Location = new System.Drawing.Point(371, 382);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(51, 19);
		this.label23.TabIndex = 492;
		this.label23.Text = "Vade";
		this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.satir_tutar_sabit_deger.Location = new System.Drawing.Point(246, 250);
		this.satir_tutar_sabit_deger.Name = "satir_tutar_sabit_deger";
		this.satir_tutar_sabit_deger.Size = new System.Drawing.Size(86, 20);
		this.satir_tutar_sabit_deger.TabIndex = 491;
		this.satir_tutar_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_tutar_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_tutar_sabit_kullan.Location = new System.Drawing.Point(18, 250);
		this.satir_tutar_sabit_kullan.Name = "satir_tutar_sabit_kullan";
		this.satir_tutar_sabit_kullan.Properties.Caption = "Sabit değer kullan";
		this.satir_tutar_sabit_kullan.Size = new System.Drawing.Size(114, 19);
		this.satir_tutar_sabit_kullan.TabIndex = 490;
		this.satir_tutar_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_tutar_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label5.Location = new System.Drawing.Point(138, 250);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(102, 19);
		this.label5.TabIndex = 489;
		this.label5.Text = "Tutar sabit değer :";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_tutar_islem_2_islem_tipi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.satir_tutar_islem_2_islem_tipi.FormattingEnabled = true;
		this.satir_tutar_islem_2_islem_tipi.Location = new System.Drawing.Point(14, 349);
		this.satir_tutar_islem_2_islem_tipi.Name = "satir_tutar_islem_2_islem_tipi";
		this.satir_tutar_islem_2_islem_tipi.Size = new System.Drawing.Size(127, 21);
		this.satir_tutar_islem_2_islem_tipi.TabIndex = 488;
		this.satir_tutar_islem_2_islem_tipi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_tutar_islem_2_islem_tipi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_tutar_islem_2_baslangic.EditValue = new decimal(new int[4]);
		this.satir_tutar_islem_2_baslangic.Location = new System.Drawing.Point(147, 349);
		this.satir_tutar_islem_2_baslangic.Name = "satir_tutar_islem_2_baslangic";
		this.satir_tutar_islem_2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_tutar_islem_2_baslangic.Properties.IsFloatValue = false;
		this.satir_tutar_islem_2_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_tutar_islem_2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_tutar_islem_2_baslangic.TabIndex = 487;
		this.satir_tutar_islem_2_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_tutar_islem_2_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_tutar_islem_1_islem_tipi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.satir_tutar_islem_1_islem_tipi.FormattingEnabled = true;
		this.satir_tutar_islem_1_islem_tipi.Location = new System.Drawing.Point(14, 323);
		this.satir_tutar_islem_1_islem_tipi.Name = "satir_tutar_islem_1_islem_tipi";
		this.satir_tutar_islem_1_islem_tipi.Size = new System.Drawing.Size(127, 21);
		this.satir_tutar_islem_1_islem_tipi.TabIndex = 486;
		this.satir_tutar_islem_1_islem_tipi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_tutar_islem_1_islem_tipi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_tutar_islem_1_baslangic.EditValue = new decimal(new int[4]);
		this.satir_tutar_islem_1_baslangic.Location = new System.Drawing.Point(147, 323);
		this.satir_tutar_islem_1_baslangic.Name = "satir_tutar_islem_1_baslangic";
		this.satir_tutar_islem_1_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_tutar_islem_1_baslangic.Properties.IsFloatValue = false;
		this.satir_tutar_islem_1_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_tutar_islem_1_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_tutar_islem_1_baslangic.TabIndex = 485;
		this.satir_tutar_islem_1_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_tutar_islem_1_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label275.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label275.Location = new System.Drawing.Point(15, 228);
		this.label275.Name = "label275";
		this.label275.Size = new System.Drawing.Size(126, 19);
		this.label275.TabIndex = 484;
		this.label275.Text = "Tutar";
		this.label275.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label47.Location = new System.Drawing.Point(18, 297);
		this.label47.Name = "label47";
		this.label47.Size = new System.Drawing.Size(123, 19);
		this.label47.TabIndex = 481;
		this.label47.Text = "Tutar :";
		this.label47.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_tutar_baslangic.EditValue = new decimal(new int[4]);
		this.satir_tutar_baslangic.Location = new System.Drawing.Point(147, 297);
		this.satir_tutar_baslangic.Name = "satir_tutar_baslangic";
		this.satir_tutar_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_tutar_baslangic.Properties.IsFloatValue = false;
		this.satir_tutar_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_tutar_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_tutar_baslangic.TabIndex = 482;
		this.satir_tutar_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_tutar_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label184.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label184.Location = new System.Drawing.Point(147, 275);
		this.label184.Name = "label184";
		this.label184.Size = new System.Drawing.Size(96, 19);
		this.label184.TabIndex = 483;
		this.label184.Text = "Sıra";
		this.label184.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label18.Location = new System.Drawing.Point(371, 143);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(120, 19);
		this.label18.TabIndex = 458;
		this.label18.Text = "Müşteri kredi kartı :";
		this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_cinsi_veri_kredi_karti.Location = new System.Drawing.Point(497, 143);
		this.satir_cinsi_veri_kredi_karti.Name = "satir_cinsi_veri_kredi_karti";
		this.satir_cinsi_veri_kredi_karti.Size = new System.Drawing.Size(156, 20);
		this.satir_cinsi_veri_kredi_karti.TabIndex = 457;
		this.satir_cinsi_veri_kredi_karti.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_cinsi_veri_kredi_karti.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label14.Location = new System.Drawing.Point(371, 117);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(120, 19);
		this.label14.TabIndex = 456;
		this.label14.Text = "Müşteri seneti :";
		this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_cinsi_veri_senet.Location = new System.Drawing.Point(497, 117);
		this.satir_cinsi_veri_senet.Name = "satir_cinsi_veri_senet";
		this.satir_cinsi_veri_senet.Size = new System.Drawing.Size(156, 20);
		this.satir_cinsi_veri_senet.TabIndex = 455;
		this.satir_cinsi_veri_senet.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_cinsi_veri_senet.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label303.Location = new System.Drawing.Point(82, 481);
		this.label303.Name = "label303";
		this.label303.Size = new System.Drawing.Size(53, 19);
		this.label303.TabIndex = 454;
		this.label303.Text = "Ön ek :";
		this.label303.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_aciklama_on_ek_deger.Location = new System.Drawing.Point(141, 481);
		this.satir_aciklama_on_ek_deger.Name = "satir_aciklama_on_ek_deger";
		this.satir_aciklama_on_ek_deger.Size = new System.Drawing.Size(96, 20);
		this.satir_aciklama_on_ek_deger.TabIndex = 453;
		this.satir_aciklama_on_ek_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_aciklama_on_ek_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_aciklama_on_ek_kullan.Location = new System.Drawing.Point(243, 481);
		this.satir_aciklama_on_ek_kullan.Name = "satir_aciklama_on_ek_kullan";
		this.satir_aciklama_on_ek_kullan.Properties.Caption = "Ön ek kullan";
		this.satir_aciklama_on_ek_kullan.Size = new System.Drawing.Size(91, 19);
		this.satir_aciklama_on_ek_kullan.TabIndex = 452;
		this.satir_aciklama_on_ek_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_aciklama_on_ek_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_aciklama_sabit_deger.Location = new System.Drawing.Point(282, 407);
		this.satir_aciklama_sabit_deger.Name = "satir_aciklama_sabit_deger";
		this.satir_aciklama_sabit_deger.Size = new System.Drawing.Size(67, 20);
		this.satir_aciklama_sabit_deger.TabIndex = 451;
		this.satir_aciklama_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_aciklama_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label19.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label19.Location = new System.Drawing.Point(141, 434);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(96, 19);
		this.label19.TabIndex = 449;
		this.label19.Text = "Sıra";
		this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.satir_aciklama_baslangic.EditValue = new decimal(new int[4]);
		this.satir_aciklama_baslangic.Location = new System.Drawing.Point(141, 455);
		this.satir_aciklama_baslangic.Name = "satir_aciklama_baslangic";
		this.satir_aciklama_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_aciklama_baslangic.Properties.IsFloatValue = false;
		this.satir_aciklama_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_aciklama_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_aciklama_baslangic.TabIndex = 447;
		this.satir_aciklama_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_aciklama_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label127.Location = new System.Drawing.Point(15, 455);
		this.label127.Name = "label127";
		this.label127.Size = new System.Drawing.Size(120, 19);
		this.label127.TabIndex = 446;
		this.label127.Text = "Satır açıklama değeri :";
		this.label127.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_aciklama_sabit_kullan.Location = new System.Drawing.Point(18, 407);
		this.satir_aciklama_sabit_kullan.Name = "satir_aciklama_sabit_kullan";
		this.satir_aciklama_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.satir_aciklama_sabit_kullan.Size = new System.Drawing.Size(111, 19);
		this.satir_aciklama_sabit_kullan.TabIndex = 445;
		this.satir_aciklama_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_aciklama_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label128.Location = new System.Drawing.Point(135, 406);
		this.label128.Name = "label128";
		this.label128.Size = new System.Drawing.Size(141, 19);
		this.label128.TabIndex = 444;
		this.label128.Text = "Satır açıklama sabit değer :";
		this.label128.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label129.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label129.Location = new System.Drawing.Point(15, 382);
		this.label129.Name = "label129";
		this.label129.Size = new System.Drawing.Size(142, 19);
		this.label129.TabIndex = 443;
		this.label129.Text = "Satır açıklama";
		this.label129.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label25.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label25.Location = new System.Drawing.Point(497, 43);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(156, 19);
		this.label25.TabIndex = 313;
		this.label25.Text = "Veri içeriği";
		this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label83.Location = new System.Drawing.Point(371, 91);
		this.label83.Name = "label83";
		this.label83.Size = new System.Drawing.Size(120, 19);
		this.label83.TabIndex = 312;
		this.label83.Text = "Müşteri çeki :";
		this.label83.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_cinsi_veri_cek.Location = new System.Drawing.Point(497, 91);
		this.satir_cinsi_veri_cek.Name = "satir_cinsi_veri_cek";
		this.satir_cinsi_veri_cek.Size = new System.Drawing.Size(156, 20);
		this.satir_cinsi_veri_cek.TabIndex = 311;
		this.satir_cinsi_veri_cek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_cinsi_veri_cek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label84.Location = new System.Drawing.Point(371, 65);
		this.label84.Name = "label84";
		this.label84.Size = new System.Drawing.Size(120, 19);
		this.label84.TabIndex = 310;
		this.label84.Text = "Nakit :";
		this.label84.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_cinsi_veri_nakit.Location = new System.Drawing.Point(497, 65);
		this.satir_cinsi_veri_nakit.Name = "satir_cinsi_veri_nakit";
		this.satir_cinsi_veri_nakit.Size = new System.Drawing.Size(156, 20);
		this.satir_cinsi_veri_nakit.TabIndex = 309;
		this.satir_cinsi_veri_nakit.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_cinsi_veri_nakit.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label86.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label86.Location = new System.Drawing.Point(178, 98);
		this.label86.Name = "label86";
		this.label86.Size = new System.Drawing.Size(96, 19);
		this.label86.TabIndex = 307;
		this.label86.Text = "Sıra";
		this.label86.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.satir_cinsi_baslangic.EditValue = new decimal(new int[4]);
		this.satir_cinsi_baslangic.Location = new System.Drawing.Point(178, 119);
		this.satir_cinsi_baslangic.Name = "satir_cinsi_baslangic";
		this.satir_cinsi_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_cinsi_baslangic.Properties.IsFloatValue = false;
		this.satir_cinsi_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_cinsi_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_cinsi_baslangic.TabIndex = 305;
		this.satir_cinsi_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_cinsi_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label87.Location = new System.Drawing.Point(52, 119);
		this.label87.Name = "label87";
		this.label87.Size = new System.Drawing.Size(120, 19);
		this.label87.TabIndex = 304;
		this.label87.Text = "Satır cinsi değeri :";
		this.label87.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_cinsi_sabit_kullan.Location = new System.Drawing.Point(176, 43);
		this.satir_cinsi_sabit_kullan.Name = "satir_cinsi_sabit_kullan";
		this.satir_cinsi_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.satir_cinsi_sabit_kullan.Size = new System.Drawing.Size(158, 19);
		this.satir_cinsi_sabit_kullan.TabIndex = 303;
		this.satir_cinsi_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_cinsi_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label88.Location = new System.Drawing.Point(55, 71);
		this.label88.Name = "label88";
		this.label88.Size = new System.Drawing.Size(117, 19);
		this.label88.TabIndex = 302;
		this.label88.Text = "Satır cinsi sabit değer :";
		this.label88.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_cinsi_sabit_deger.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.satir_cinsi_sabit_deger.FormattingEnabled = true;
		this.satir_cinsi_sabit_deger.Location = new System.Drawing.Point(178, 68);
		this.satir_cinsi_sabit_deger.Name = "satir_cinsi_sabit_deger";
		this.satir_cinsi_sabit_deger.Size = new System.Drawing.Size(156, 21);
		this.satir_cinsi_sabit_deger.TabIndex = 301;
		this.satir_cinsi_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_cinsi_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label81.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label81.Location = new System.Drawing.Point(52, 18);
		this.label81.Name = "label81";
		this.label81.Size = new System.Drawing.Size(234, 19);
		this.label81.TabIndex = 299;
		this.label81.Text = "Satır cinsi";
		this.label81.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_giden_havale_son_ek);
		this.xtraTabPage7.Controls.Add(this.label70);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_giden_havale_on_ek);
		this.xtraTabPage7.Controls.Add(this.label71);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_giden_havale_sabit_deger);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_giden_havale_sabit_kullan);
		this.xtraTabPage7.Controls.Add(this.label72);
		this.xtraTabPage7.Controls.Add(this.label73);
		this.xtraTabPage7.Controls.Add(this.label74);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_giden_havale_baslangic);
		this.xtraTabPage7.Controls.Add(this.label75);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_gelen_havale_son_ek);
		this.xtraTabPage7.Controls.Add(this.label64);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_gelen_havale_on_ek);
		this.xtraTabPage7.Controls.Add(this.label65);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_gelen_havale_sabit_deger);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_gelen_havale_sabit_kullan);
		this.xtraTabPage7.Controls.Add(this.label66);
		this.xtraTabPage7.Controls.Add(this.label67);
		this.xtraTabPage7.Controls.Add(this.label68);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_gelen_havale_baslangic);
		this.xtraTabPage7.Controls.Add(this.label69);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_kredi_karti_son_ek);
		this.xtraTabPage7.Controls.Add(this.label57);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_kredi_karti_on_ek);
		this.xtraTabPage7.Controls.Add(this.label58);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_kredi_karti_sabit_deger);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_kredi_karti_sabit_kullan);
		this.xtraTabPage7.Controls.Add(this.label59);
		this.xtraTabPage7.Controls.Add(this.label60);
		this.xtraTabPage7.Controls.Add(this.label61);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_kredi_karti_baslangic);
		this.xtraTabPage7.Controls.Add(this.label63);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_senet_son_ek);
		this.xtraTabPage7.Controls.Add(this.label51);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_senet_on_ek);
		this.xtraTabPage7.Controls.Add(this.label52);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_senet_sabit_deger);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_senet_sabit_kullan);
		this.xtraTabPage7.Controls.Add(this.label53);
		this.xtraTabPage7.Controls.Add(this.label54);
		this.xtraTabPage7.Controls.Add(this.label55);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_senet_baslangic);
		this.xtraTabPage7.Controls.Add(this.label56);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_cek_son_ek);
		this.xtraTabPage7.Controls.Add(this.label43);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_cek_on_ek);
		this.xtraTabPage7.Controls.Add(this.label44);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_cek_sabit_deger);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_cek_sabit_kullan);
		this.xtraTabPage7.Controls.Add(this.label46);
		this.xtraTabPage7.Controls.Add(this.label48);
		this.xtraTabPage7.Controls.Add(this.label49);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_cek_baslangic);
		this.xtraTabPage7.Controls.Add(this.label50);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_nakit_son_ek);
		this.xtraTabPage7.Controls.Add(this.label42);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_nakit_on_ek);
		this.xtraTabPage7.Controls.Add(this.label40);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_nakit_sabit_deger);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_nakit_sabit_kullan);
		this.xtraTabPage7.Controls.Add(this.label89);
		this.xtraTabPage7.Controls.Add(this.label82);
		this.xtraTabPage7.Controls.Add(this.label4);
		this.xtraTabPage7.Controls.Add(this.satir_hesap_kodu_nakit_baslangic);
		this.xtraTabPage7.Controls.Add(this.label8);
		this.xtraTabPage7.Name = "xtraTabPage7";
		this.xtraTabPage7.Size = new System.Drawing.Size(781, 540);
		this.xtraTabPage7.Text = "Hesap kodu";
		this.satir_hesap_kodu_giden_havale_son_ek.Location = new System.Drawing.Point(582, 444);
		this.satir_hesap_kodu_giden_havale_son_ek.Name = "satir_hesap_kodu_giden_havale_son_ek";
		this.satir_hesap_kodu_giden_havale_son_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_giden_havale_son_ek.TabIndex = 382;
		this.satir_hesap_kodu_giden_havale_son_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_giden_havale_son_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label70.Location = new System.Drawing.Point(527, 444);
		this.label70.Name = "label70";
		this.label70.Size = new System.Drawing.Size(49, 19);
		this.label70.TabIndex = 381;
		this.label70.Text = "Son ek :";
		this.label70.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_giden_havale_on_ek.Location = new System.Drawing.Point(460, 443);
		this.satir_hesap_kodu_giden_havale_on_ek.Name = "satir_hesap_kodu_giden_havale_on_ek";
		this.satir_hesap_kodu_giden_havale_on_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_giden_havale_on_ek.TabIndex = 380;
		this.satir_hesap_kodu_giden_havale_on_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_giden_havale_on_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label71.Location = new System.Drawing.Point(408, 443);
		this.label71.Name = "label71";
		this.label71.Size = new System.Drawing.Size(46, 19);
		this.label71.TabIndex = 379;
		this.label71.Text = "Ön ek :";
		this.label71.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_giden_havale_sabit_deger.Location = new System.Drawing.Point(154, 443);
		this.satir_hesap_kodu_giden_havale_sabit_deger.Name = "satir_hesap_kodu_giden_havale_sabit_deger";
		this.satir_hesap_kodu_giden_havale_sabit_deger.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_giden_havale_sabit_deger.TabIndex = 378;
		this.satir_hesap_kodu_giden_havale_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_giden_havale_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_hesap_kodu_giden_havale_sabit_kullan.Location = new System.Drawing.Point(24, 421);
		this.satir_hesap_kodu_giden_havale_sabit_kullan.Name = "satir_hesap_kodu_giden_havale_sabit_kullan";
		this.satir_hesap_kodu_giden_havale_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.satir_hesap_kodu_giden_havale_sabit_kullan.Size = new System.Drawing.Size(114, 19);
		this.satir_hesap_kodu_giden_havale_sabit_kullan.TabIndex = 377;
		this.satir_hesap_kodu_giden_havale_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_giden_havale_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label72.Location = new System.Drawing.Point(21, 443);
		this.label72.Name = "label72";
		this.label72.Size = new System.Drawing.Size(127, 19);
		this.label72.TabIndex = 376;
		this.label72.Text = "Hesap kodu sabit değer :";
		this.label72.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label73.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label73.Location = new System.Drawing.Point(21, 399);
		this.label73.Name = "label73";
		this.label73.Size = new System.Drawing.Size(182, 19);
		this.label73.TabIndex = 375;
		this.label73.Text = "Giden havale banka kodu";
		this.label73.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label74.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label74.Location = new System.Drawing.Point(309, 422);
		this.label74.Name = "label74";
		this.label74.Size = new System.Drawing.Size(96, 19);
		this.label74.TabIndex = 374;
		this.label74.Text = "Sıra";
		this.label74.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.satir_hesap_kodu_giden_havale_baslangic.EditValue = new decimal(new int[4]);
		this.satir_hesap_kodu_giden_havale_baslangic.Location = new System.Drawing.Point(309, 443);
		this.satir_hesap_kodu_giden_havale_baslangic.Name = "satir_hesap_kodu_giden_havale_baslangic";
		this.satir_hesap_kodu_giden_havale_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_hesap_kodu_giden_havale_baslangic.Properties.IsFloatValue = false;
		this.satir_hesap_kodu_giden_havale_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_hesap_kodu_giden_havale_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_hesap_kodu_giden_havale_baslangic.TabIndex = 373;
		this.satir_hesap_kodu_giden_havale_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_giden_havale_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label75.Location = new System.Drawing.Point(226, 443);
		this.label75.Name = "label75";
		this.label75.Size = new System.Drawing.Size(77, 19);
		this.label75.TabIndex = 372;
		this.label75.Text = "Hesap kodu :";
		this.label75.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_gelen_havale_son_ek.Location = new System.Drawing.Point(582, 367);
		this.satir_hesap_kodu_gelen_havale_son_ek.Name = "satir_hesap_kodu_gelen_havale_son_ek";
		this.satir_hesap_kodu_gelen_havale_son_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_gelen_havale_son_ek.TabIndex = 371;
		this.satir_hesap_kodu_gelen_havale_son_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_gelen_havale_son_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label64.Location = new System.Drawing.Point(527, 367);
		this.label64.Name = "label64";
		this.label64.Size = new System.Drawing.Size(49, 19);
		this.label64.TabIndex = 370;
		this.label64.Text = "Son ek :";
		this.label64.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_gelen_havale_on_ek.Location = new System.Drawing.Point(460, 366);
		this.satir_hesap_kodu_gelen_havale_on_ek.Name = "satir_hesap_kodu_gelen_havale_on_ek";
		this.satir_hesap_kodu_gelen_havale_on_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_gelen_havale_on_ek.TabIndex = 369;
		this.satir_hesap_kodu_gelen_havale_on_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_gelen_havale_on_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label65.Location = new System.Drawing.Point(408, 366);
		this.label65.Name = "label65";
		this.label65.Size = new System.Drawing.Size(46, 19);
		this.label65.TabIndex = 368;
		this.label65.Text = "Ön ek :";
		this.label65.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_gelen_havale_sabit_deger.Location = new System.Drawing.Point(154, 366);
		this.satir_hesap_kodu_gelen_havale_sabit_deger.Name = "satir_hesap_kodu_gelen_havale_sabit_deger";
		this.satir_hesap_kodu_gelen_havale_sabit_deger.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_gelen_havale_sabit_deger.TabIndex = 367;
		this.satir_hesap_kodu_gelen_havale_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_gelen_havale_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_hesap_kodu_gelen_havale_sabit_kullan.Location = new System.Drawing.Point(24, 344);
		this.satir_hesap_kodu_gelen_havale_sabit_kullan.Name = "satir_hesap_kodu_gelen_havale_sabit_kullan";
		this.satir_hesap_kodu_gelen_havale_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.satir_hesap_kodu_gelen_havale_sabit_kullan.Size = new System.Drawing.Size(114, 19);
		this.satir_hesap_kodu_gelen_havale_sabit_kullan.TabIndex = 366;
		this.satir_hesap_kodu_gelen_havale_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_gelen_havale_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label66.Location = new System.Drawing.Point(21, 366);
		this.label66.Name = "label66";
		this.label66.Size = new System.Drawing.Size(127, 19);
		this.label66.TabIndex = 365;
		this.label66.Text = "Hesap kodu sabit değer :";
		this.label66.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label67.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label67.Location = new System.Drawing.Point(21, 322);
		this.label67.Name = "label67";
		this.label67.Size = new System.Drawing.Size(182, 19);
		this.label67.TabIndex = 364;
		this.label67.Text = "Gelen havale banka kodu";
		this.label67.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label68.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label68.Location = new System.Drawing.Point(309, 345);
		this.label68.Name = "label68";
		this.label68.Size = new System.Drawing.Size(96, 19);
		this.label68.TabIndex = 363;
		this.label68.Text = "Sıra";
		this.label68.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.satir_hesap_kodu_gelen_havale_baslangic.EditValue = new decimal(new int[4]);
		this.satir_hesap_kodu_gelen_havale_baslangic.Location = new System.Drawing.Point(309, 366);
		this.satir_hesap_kodu_gelen_havale_baslangic.Name = "satir_hesap_kodu_gelen_havale_baslangic";
		this.satir_hesap_kodu_gelen_havale_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_hesap_kodu_gelen_havale_baslangic.Properties.IsFloatValue = false;
		this.satir_hesap_kodu_gelen_havale_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_hesap_kodu_gelen_havale_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_hesap_kodu_gelen_havale_baslangic.TabIndex = 362;
		this.satir_hesap_kodu_gelen_havale_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_gelen_havale_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label69.Location = new System.Drawing.Point(226, 366);
		this.label69.Name = "label69";
		this.label69.Size = new System.Drawing.Size(77, 19);
		this.label69.TabIndex = 361;
		this.label69.Text = "Hesap kodu :";
		this.label69.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_kredi_karti_son_ek.Location = new System.Drawing.Point(582, 291);
		this.satir_hesap_kodu_kredi_karti_son_ek.Name = "satir_hesap_kodu_kredi_karti_son_ek";
		this.satir_hesap_kodu_kredi_karti_son_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_kredi_karti_son_ek.TabIndex = 360;
		this.satir_hesap_kodu_kredi_karti_son_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_kredi_karti_son_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label57.Location = new System.Drawing.Point(527, 291);
		this.label57.Name = "label57";
		this.label57.Size = new System.Drawing.Size(49, 19);
		this.label57.TabIndex = 359;
		this.label57.Text = "Son ek :";
		this.label57.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_kredi_karti_on_ek.Location = new System.Drawing.Point(460, 290);
		this.satir_hesap_kodu_kredi_karti_on_ek.Name = "satir_hesap_kodu_kredi_karti_on_ek";
		this.satir_hesap_kodu_kredi_karti_on_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_kredi_karti_on_ek.TabIndex = 358;
		this.satir_hesap_kodu_kredi_karti_on_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_kredi_karti_on_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label58.Location = new System.Drawing.Point(408, 290);
		this.label58.Name = "label58";
		this.label58.Size = new System.Drawing.Size(46, 19);
		this.label58.TabIndex = 357;
		this.label58.Text = "Ön ek :";
		this.label58.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_kredi_karti_sabit_deger.Location = new System.Drawing.Point(154, 290);
		this.satir_hesap_kodu_kredi_karti_sabit_deger.Name = "satir_hesap_kodu_kredi_karti_sabit_deger";
		this.satir_hesap_kodu_kredi_karti_sabit_deger.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_kredi_karti_sabit_deger.TabIndex = 356;
		this.satir_hesap_kodu_kredi_karti_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_kredi_karti_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_hesap_kodu_kredi_karti_sabit_kullan.Location = new System.Drawing.Point(24, 268);
		this.satir_hesap_kodu_kredi_karti_sabit_kullan.Name = "satir_hesap_kodu_kredi_karti_sabit_kullan";
		this.satir_hesap_kodu_kredi_karti_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.satir_hesap_kodu_kredi_karti_sabit_kullan.Size = new System.Drawing.Size(114, 19);
		this.satir_hesap_kodu_kredi_karti_sabit_kullan.TabIndex = 355;
		this.satir_hesap_kodu_kredi_karti_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_kredi_karti_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label59.Location = new System.Drawing.Point(21, 290);
		this.label59.Name = "label59";
		this.label59.Size = new System.Drawing.Size(127, 19);
		this.label59.TabIndex = 354;
		this.label59.Text = "Hesap kodu sabit değer :";
		this.label59.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label60.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label60.Location = new System.Drawing.Point(21, 246);
		this.label60.Name = "label60";
		this.label60.Size = new System.Drawing.Size(173, 19);
		this.label60.TabIndex = 353;
		this.label60.Text = "Kredi kartı banka kodu";
		this.label60.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label61.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label61.Location = new System.Drawing.Point(309, 269);
		this.label61.Name = "label61";
		this.label61.Size = new System.Drawing.Size(96, 19);
		this.label61.TabIndex = 352;
		this.label61.Text = "Sıra";
		this.label61.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.satir_hesap_kodu_kredi_karti_baslangic.EditValue = new decimal(new int[4]);
		this.satir_hesap_kodu_kredi_karti_baslangic.Location = new System.Drawing.Point(309, 290);
		this.satir_hesap_kodu_kredi_karti_baslangic.Name = "satir_hesap_kodu_kredi_karti_baslangic";
		this.satir_hesap_kodu_kredi_karti_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_hesap_kodu_kredi_karti_baslangic.Properties.IsFloatValue = false;
		this.satir_hesap_kodu_kredi_karti_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_hesap_kodu_kredi_karti_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_hesap_kodu_kredi_karti_baslangic.TabIndex = 351;
		this.satir_hesap_kodu_kredi_karti_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_kredi_karti_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label63.Location = new System.Drawing.Point(226, 290);
		this.label63.Name = "label63";
		this.label63.Size = new System.Drawing.Size(77, 19);
		this.label63.TabIndex = 350;
		this.label63.Text = "Hesap kodu :";
		this.label63.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_senet_son_ek.Location = new System.Drawing.Point(582, 211);
		this.satir_hesap_kodu_senet_son_ek.Name = "satir_hesap_kodu_senet_son_ek";
		this.satir_hesap_kodu_senet_son_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_senet_son_ek.TabIndex = 349;
		this.satir_hesap_kodu_senet_son_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_senet_son_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label51.Location = new System.Drawing.Point(527, 211);
		this.label51.Name = "label51";
		this.label51.Size = new System.Drawing.Size(49, 19);
		this.label51.TabIndex = 348;
		this.label51.Text = "Son ek :";
		this.label51.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_senet_on_ek.Location = new System.Drawing.Point(460, 210);
		this.satir_hesap_kodu_senet_on_ek.Name = "satir_hesap_kodu_senet_on_ek";
		this.satir_hesap_kodu_senet_on_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_senet_on_ek.TabIndex = 347;
		this.satir_hesap_kodu_senet_on_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_senet_on_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label52.Location = new System.Drawing.Point(408, 210);
		this.label52.Name = "label52";
		this.label52.Size = new System.Drawing.Size(46, 19);
		this.label52.TabIndex = 346;
		this.label52.Text = "Ön ek :";
		this.label52.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_senet_sabit_deger.Location = new System.Drawing.Point(152, 211);
		this.satir_hesap_kodu_senet_sabit_deger.Name = "satir_hesap_kodu_senet_sabit_deger";
		this.satir_hesap_kodu_senet_sabit_deger.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_senet_sabit_deger.TabIndex = 345;
		this.satir_hesap_kodu_senet_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_senet_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_hesap_kodu_senet_sabit_kullan.Location = new System.Drawing.Point(22, 189);
		this.satir_hesap_kodu_senet_sabit_kullan.Name = "satir_hesap_kodu_senet_sabit_kullan";
		this.satir_hesap_kodu_senet_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.satir_hesap_kodu_senet_sabit_kullan.Size = new System.Drawing.Size(114, 19);
		this.satir_hesap_kodu_senet_sabit_kullan.TabIndex = 344;
		this.satir_hesap_kodu_senet_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_senet_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label53.Location = new System.Drawing.Point(19, 211);
		this.label53.Name = "label53";
		this.label53.Size = new System.Drawing.Size(127, 19);
		this.label53.TabIndex = 343;
		this.label53.Text = "Hesap kodu sabit değer :";
		this.label53.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label54.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label54.Location = new System.Drawing.Point(19, 167);
		this.label54.Name = "label54";
		this.label54.Size = new System.Drawing.Size(129, 19);
		this.label54.TabIndex = 342;
		this.label54.Text = "Senet kasa kodu";
		this.label54.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label55.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label55.Location = new System.Drawing.Point(309, 189);
		this.label55.Name = "label55";
		this.label55.Size = new System.Drawing.Size(96, 19);
		this.label55.TabIndex = 341;
		this.label55.Text = "Sıra";
		this.label55.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.satir_hesap_kodu_senet_baslangic.EditValue = new decimal(new int[4]);
		this.satir_hesap_kodu_senet_baslangic.Location = new System.Drawing.Point(309, 210);
		this.satir_hesap_kodu_senet_baslangic.Name = "satir_hesap_kodu_senet_baslangic";
		this.satir_hesap_kodu_senet_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_hesap_kodu_senet_baslangic.Properties.IsFloatValue = false;
		this.satir_hesap_kodu_senet_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_hesap_kodu_senet_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_hesap_kodu_senet_baslangic.TabIndex = 340;
		this.satir_hesap_kodu_senet_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_senet_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label56.Location = new System.Drawing.Point(226, 210);
		this.label56.Name = "label56";
		this.label56.Size = new System.Drawing.Size(77, 19);
		this.label56.TabIndex = 339;
		this.label56.Text = "Hesap kodu :";
		this.label56.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_cek_son_ek.Location = new System.Drawing.Point(582, 130);
		this.satir_hesap_kodu_cek_son_ek.Name = "satir_hesap_kodu_cek_son_ek";
		this.satir_hesap_kodu_cek_son_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_cek_son_ek.TabIndex = 338;
		this.satir_hesap_kodu_cek_son_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_cek_son_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label43.Location = new System.Drawing.Point(527, 130);
		this.label43.Name = "label43";
		this.label43.Size = new System.Drawing.Size(49, 19);
		this.label43.TabIndex = 337;
		this.label43.Text = "Son ek :";
		this.label43.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_cek_on_ek.Location = new System.Drawing.Point(460, 129);
		this.satir_hesap_kodu_cek_on_ek.Name = "satir_hesap_kodu_cek_on_ek";
		this.satir_hesap_kodu_cek_on_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_cek_on_ek.TabIndex = 336;
		this.satir_hesap_kodu_cek_on_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_cek_on_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label44.Location = new System.Drawing.Point(408, 129);
		this.label44.Name = "label44";
		this.label44.Size = new System.Drawing.Size(46, 19);
		this.label44.TabIndex = 335;
		this.label44.Text = "Ön ek :";
		this.label44.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_cek_sabit_deger.Location = new System.Drawing.Point(154, 129);
		this.satir_hesap_kodu_cek_sabit_deger.Name = "satir_hesap_kodu_cek_sabit_deger";
		this.satir_hesap_kodu_cek_sabit_deger.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_cek_sabit_deger.TabIndex = 334;
		this.satir_hesap_kodu_cek_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_cek_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_hesap_kodu_cek_sabit_kullan.Location = new System.Drawing.Point(24, 107);
		this.satir_hesap_kodu_cek_sabit_kullan.Name = "satir_hesap_kodu_cek_sabit_kullan";
		this.satir_hesap_kodu_cek_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.satir_hesap_kodu_cek_sabit_kullan.Size = new System.Drawing.Size(114, 19);
		this.satir_hesap_kodu_cek_sabit_kullan.TabIndex = 333;
		this.satir_hesap_kodu_cek_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_cek_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label46.Location = new System.Drawing.Point(21, 129);
		this.label46.Name = "label46";
		this.label46.Size = new System.Drawing.Size(127, 19);
		this.label46.TabIndex = 332;
		this.label46.Text = "Hesap kodu sabit değer :";
		this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label48.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label48.Location = new System.Drawing.Point(21, 85);
		this.label48.Name = "label48";
		this.label48.Size = new System.Drawing.Size(127, 19);
		this.label48.TabIndex = 331;
		this.label48.Text = "Çek kasa kodu";
		this.label48.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label49.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label49.Location = new System.Drawing.Point(309, 108);
		this.label49.Name = "label49";
		this.label49.Size = new System.Drawing.Size(96, 19);
		this.label49.TabIndex = 330;
		this.label49.Text = "Sıra";
		this.label49.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.satir_hesap_kodu_cek_baslangic.EditValue = new decimal(new int[4]);
		this.satir_hesap_kodu_cek_baslangic.Location = new System.Drawing.Point(309, 129);
		this.satir_hesap_kodu_cek_baslangic.Name = "satir_hesap_kodu_cek_baslangic";
		this.satir_hesap_kodu_cek_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_hesap_kodu_cek_baslangic.Properties.IsFloatValue = false;
		this.satir_hesap_kodu_cek_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_hesap_kodu_cek_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_hesap_kodu_cek_baslangic.TabIndex = 329;
		this.satir_hesap_kodu_cek_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_cek_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label50.Location = new System.Drawing.Point(226, 129);
		this.label50.Name = "label50";
		this.label50.Size = new System.Drawing.Size(77, 19);
		this.label50.TabIndex = 328;
		this.label50.Text = "Hesap kodu :";
		this.label50.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_nakit_son_ek.Location = new System.Drawing.Point(582, 54);
		this.satir_hesap_kodu_nakit_son_ek.Name = "satir_hesap_kodu_nakit_son_ek";
		this.satir_hesap_kodu_nakit_son_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_nakit_son_ek.TabIndex = 327;
		this.satir_hesap_kodu_nakit_son_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_nakit_son_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label42.Location = new System.Drawing.Point(527, 54);
		this.label42.Name = "label42";
		this.label42.Size = new System.Drawing.Size(49, 19);
		this.label42.TabIndex = 326;
		this.label42.Text = "Son ek :";
		this.label42.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_nakit_on_ek.Location = new System.Drawing.Point(460, 53);
		this.satir_hesap_kodu_nakit_on_ek.Name = "satir_hesap_kodu_nakit_on_ek";
		this.satir_hesap_kodu_nakit_on_ek.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_nakit_on_ek.TabIndex = 325;
		this.satir_hesap_kodu_nakit_on_ek.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_nakit_on_ek.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label40.Location = new System.Drawing.Point(408, 53);
		this.label40.Name = "label40";
		this.label40.Size = new System.Drawing.Size(46, 19);
		this.label40.TabIndex = 324;
		this.label40.Text = "Ön ek :";
		this.label40.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.satir_hesap_kodu_nakit_sabit_deger.Location = new System.Drawing.Point(154, 53);
		this.satir_hesap_kodu_nakit_sabit_deger.Name = "satir_hesap_kodu_nakit_sabit_deger";
		this.satir_hesap_kodu_nakit_sabit_deger.Size = new System.Drawing.Size(61, 20);
		this.satir_hesap_kodu_nakit_sabit_deger.TabIndex = 323;
		this.satir_hesap_kodu_nakit_sabit_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_nakit_sabit_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.satir_hesap_kodu_nakit_sabit_kullan.Location = new System.Drawing.Point(24, 31);
		this.satir_hesap_kodu_nakit_sabit_kullan.Name = "satir_hesap_kodu_nakit_sabit_kullan";
		this.satir_hesap_kodu_nakit_sabit_kullan.Properties.Caption = "Sabit değeri kullan";
		this.satir_hesap_kodu_nakit_sabit_kullan.Size = new System.Drawing.Size(114, 19);
		this.satir_hesap_kodu_nakit_sabit_kullan.TabIndex = 322;
		this.satir_hesap_kodu_nakit_sabit_kullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_nakit_sabit_kullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label89.Location = new System.Drawing.Point(21, 53);
		this.label89.Name = "label89";
		this.label89.Size = new System.Drawing.Size(127, 19);
		this.label89.TabIndex = 321;
		this.label89.Text = "Hesap kodu sabit değer :";
		this.label89.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label82.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label82.Location = new System.Drawing.Point(21, 9);
		this.label82.Name = "label82";
		this.label82.Size = new System.Drawing.Size(127, 19);
		this.label82.TabIndex = 320;
		this.label82.Text = "Nakit kasa kodu";
		this.label82.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label4.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label4.Location = new System.Drawing.Point(309, 32);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(96, 19);
		this.label4.TabIndex = 319;
		this.label4.Text = "Sıra";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.satir_hesap_kodu_nakit_baslangic.EditValue = new decimal(new int[4]);
		this.satir_hesap_kodu_nakit_baslangic.Location = new System.Drawing.Point(309, 53);
		this.satir_hesap_kodu_nakit_baslangic.Name = "satir_hesap_kodu_nakit_baslangic";
		this.satir_hesap_kodu_nakit_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.satir_hesap_kodu_nakit_baslangic.Properties.IsFloatValue = false;
		this.satir_hesap_kodu_nakit_baslangic.Properties.Mask.EditMask = "N00";
		this.satir_hesap_kodu_nakit_baslangic.Size = new System.Drawing.Size(96, 20);
		this.satir_hesap_kodu_nakit_baslangic.TabIndex = 318;
		this.satir_hesap_kodu_nakit_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.satir_hesap_kodu_nakit_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label8.Location = new System.Drawing.Point(226, 53);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(77, 19);
		this.label8.TabIndex = 317;
		this.label8.Text = "Hesap kodu :";
		this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.xtraTabPage2.Name = "xtraTabPage2";
		this.xtraTabPage2.Size = new System.Drawing.Size(781, 540);
		this.xtraTabPage2.Text = "Çek / Senet bilgileri";
		this.xtraTabPage4.Controls.Add(this.kriter_bool5_evet_icin_deger);
		this.xtraTabPage4.Controls.Add(this.kriter_bool4_evet_icin_deger);
		this.xtraTabPage4.Controls.Add(this.kriter_bool3_evet_icin_deger);
		this.xtraTabPage4.Controls.Add(this.kriter_bool2_evet_icin_deger);
		this.xtraTabPage4.Controls.Add(this.label538);
		this.xtraTabPage4.Controls.Add(this.kriter_bool5_baslangic);
		this.xtraTabPage4.Controls.Add(this.label539);
		this.xtraTabPage4.Controls.Add(this.kriter_bool4_baslangic);
		this.xtraTabPage4.Controls.Add(this.label540);
		this.xtraTabPage4.Controls.Add(this.kriter_bool3_baslangic);
		this.xtraTabPage4.Controls.Add(this.label541);
		this.xtraTabPage4.Controls.Add(this.kriter_bool2_baslangic);
		this.xtraTabPage4.Controls.Add(this.label542);
		this.xtraTabPage4.Controls.Add(this.label544);
		this.xtraTabPage4.Controls.Add(this.kriter_bool1_baslangic);
		this.xtraTabPage4.Controls.Add(this.label545);
		this.xtraTabPage4.Controls.Add(this.label533);
		this.xtraTabPage4.Controls.Add(this.kriter_bool1_evet_icin_deger);
		this.xtraTabPage4.Controls.Add(this.label525);
		this.xtraTabPage4.Controls.Add(this.kriter_double5_baslangic);
		this.xtraTabPage4.Controls.Add(this.label526);
		this.xtraTabPage4.Controls.Add(this.kriter_double4_baslangic);
		this.xtraTabPage4.Controls.Add(this.label527);
		this.xtraTabPage4.Controls.Add(this.kriter_double3_baslangic);
		this.xtraTabPage4.Controls.Add(this.label528);
		this.xtraTabPage4.Controls.Add(this.kriter_double2_baslangic);
		this.xtraTabPage4.Controls.Add(this.label529);
		this.xtraTabPage4.Controls.Add(this.label531);
		this.xtraTabPage4.Controls.Add(this.kriter_double1_baslangic);
		this.xtraTabPage4.Controls.Add(this.label532);
		this.xtraTabPage4.Controls.Add(this.label524);
		this.xtraTabPage4.Controls.Add(this.kriter_metin5_baslangic);
		this.xtraTabPage4.Controls.Add(this.label523);
		this.xtraTabPage4.Controls.Add(this.kriter_metin4_baslangic);
		this.xtraTabPage4.Controls.Add(this.label515);
		this.xtraTabPage4.Controls.Add(this.kriter_metin3_baslangic);
		this.xtraTabPage4.Controls.Add(this.label508);
		this.xtraTabPage4.Controls.Add(this.kriter_metin2_baslangic);
		this.xtraTabPage4.Controls.Add(this.label507);
		this.xtraTabPage4.Controls.Add(this.label505);
		this.xtraTabPage4.Controls.Add(this.kriter_metin1_baslangic);
		this.xtraTabPage4.Controls.Add(this.label506);
		this.xtraTabPage4.Name = "xtraTabPage4";
		this.xtraTabPage4.Size = new System.Drawing.Size(787, 597);
		this.xtraTabPage4.Text = "Kriterler için veriler";
		this.kriter_bool5_evet_icin_deger.Location = new System.Drawing.Point(203, 370);
		this.kriter_bool5_evet_icin_deger.Name = "kriter_bool5_evet_icin_deger";
		this.kriter_bool5_evet_icin_deger.Size = new System.Drawing.Size(156, 20);
		this.kriter_bool5_evet_icin_deger.TabIndex = 510;
		this.kriter_bool5_evet_icin_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_bool5_evet_icin_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.kriter_bool4_evet_icin_deger.Location = new System.Drawing.Point(203, 344);
		this.kriter_bool4_evet_icin_deger.Name = "kriter_bool4_evet_icin_deger";
		this.kriter_bool4_evet_icin_deger.Size = new System.Drawing.Size(156, 20);
		this.kriter_bool4_evet_icin_deger.TabIndex = 509;
		this.kriter_bool4_evet_icin_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_bool4_evet_icin_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.kriter_bool3_evet_icin_deger.Location = new System.Drawing.Point(203, 318);
		this.kriter_bool3_evet_icin_deger.Name = "kriter_bool3_evet_icin_deger";
		this.kriter_bool3_evet_icin_deger.Size = new System.Drawing.Size(156, 20);
		this.kriter_bool3_evet_icin_deger.TabIndex = 508;
		this.kriter_bool3_evet_icin_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_bool3_evet_icin_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.kriter_bool2_evet_icin_deger.Location = new System.Drawing.Point(203, 292);
		this.kriter_bool2_evet_icin_deger.Name = "kriter_bool2_evet_icin_deger";
		this.kriter_bool2_evet_icin_deger.Size = new System.Drawing.Size(156, 20);
		this.kriter_bool2_evet_icin_deger.TabIndex = 507;
		this.kriter_bool2_evet_icin_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_bool2_evet_icin_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label538.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label538.Location = new System.Drawing.Point(23, 226);
		this.label538.Name = "label538";
		this.label538.Size = new System.Drawing.Size(234, 19);
		this.label538.TabIndex = 506;
		this.label538.Text = "Evet/Hayır alanlar";
		this.label538.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.kriter_bool5_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_bool5_baslangic.Location = new System.Drawing.Point(101, 370);
		this.kriter_bool5_baslangic.Name = "kriter_bool5_baslangic";
		this.kriter_bool5_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_bool5_baslangic.Properties.IsFloatValue = false;
		this.kriter_bool5_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_bool5_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_bool5_baslangic.TabIndex = 504;
		this.kriter_bool5_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_bool5_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label539.Location = new System.Drawing.Point(16, 370);
		this.label539.Name = "label539";
		this.label539.Size = new System.Drawing.Size(79, 19);
		this.label539.TabIndex = 503;
		this.label539.Text = "Evet/Hayır 5 :";
		this.label539.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.kriter_bool4_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_bool4_baslangic.Location = new System.Drawing.Point(101, 344);
		this.kriter_bool4_baslangic.Name = "kriter_bool4_baslangic";
		this.kriter_bool4_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_bool4_baslangic.Properties.IsFloatValue = false;
		this.kriter_bool4_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_bool4_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_bool4_baslangic.TabIndex = 501;
		this.kriter_bool4_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_bool4_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label540.Location = new System.Drawing.Point(16, 344);
		this.label540.Name = "label540";
		this.label540.Size = new System.Drawing.Size(79, 19);
		this.label540.TabIndex = 500;
		this.label540.Text = "Evet/Hayır 4 :";
		this.label540.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.kriter_bool3_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_bool3_baslangic.Location = new System.Drawing.Point(101, 318);
		this.kriter_bool3_baslangic.Name = "kriter_bool3_baslangic";
		this.kriter_bool3_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_bool3_baslangic.Properties.IsFloatValue = false;
		this.kriter_bool3_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_bool3_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_bool3_baslangic.TabIndex = 498;
		this.kriter_bool3_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_bool3_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label541.Location = new System.Drawing.Point(16, 318);
		this.label541.Name = "label541";
		this.label541.Size = new System.Drawing.Size(79, 19);
		this.label541.TabIndex = 497;
		this.label541.Text = "Evet/Hayır 3 :";
		this.label541.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.kriter_bool2_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_bool2_baslangic.Location = new System.Drawing.Point(101, 292);
		this.kriter_bool2_baslangic.Name = "kriter_bool2_baslangic";
		this.kriter_bool2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_bool2_baslangic.Properties.IsFloatValue = false;
		this.kriter_bool2_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_bool2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_bool2_baslangic.TabIndex = 495;
		this.kriter_bool2_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_bool2_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label542.Location = new System.Drawing.Point(16, 292);
		this.label542.Name = "label542";
		this.label542.Size = new System.Drawing.Size(79, 19);
		this.label542.TabIndex = 494;
		this.label542.Text = "Evet/Hayır 2 :";
		this.label542.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label544.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label544.Location = new System.Drawing.Point(101, 245);
		this.label544.Name = "label544";
		this.label544.Size = new System.Drawing.Size(96, 19);
		this.label544.TabIndex = 492;
		this.label544.Text = "Sıra";
		this.label544.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.kriter_bool1_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_bool1_baslangic.Location = new System.Drawing.Point(101, 266);
		this.kriter_bool1_baslangic.Name = "kriter_bool1_baslangic";
		this.kriter_bool1_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_bool1_baslangic.Properties.IsFloatValue = false;
		this.kriter_bool1_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_bool1_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_bool1_baslangic.TabIndex = 490;
		this.kriter_bool1_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_bool1_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label545.Location = new System.Drawing.Point(16, 266);
		this.label545.Name = "label545";
		this.label545.Size = new System.Drawing.Size(79, 19);
		this.label545.TabIndex = 489;
		this.label545.Text = "Evet/Hayır 1 :";
		this.label545.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label533.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label533.Location = new System.Drawing.Point(203, 244);
		this.label533.Name = "label533";
		this.label533.Size = new System.Drawing.Size(156, 19);
		this.label533.TabIndex = 488;
		this.label533.Text = "Evet için veri içeriği";
		this.label533.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.kriter_bool1_evet_icin_deger.Location = new System.Drawing.Point(203, 266);
		this.kriter_bool1_evet_icin_deger.Name = "kriter_bool1_evet_icin_deger";
		this.kriter_bool1_evet_icin_deger.Size = new System.Drawing.Size(156, 20);
		this.kriter_bool1_evet_icin_deger.TabIndex = 486;
		this.kriter_bool1_evet_icin_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_bool1_evet_icin_deger.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label525.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label525.Location = new System.Drawing.Point(385, 24);
		this.label525.Name = "label525";
		this.label525.Size = new System.Drawing.Size(234, 19);
		this.label525.TabIndex = 344;
		this.label525.Text = "Sayı alanlar";
		this.label525.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.kriter_double5_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_double5_baslangic.Location = new System.Drawing.Point(463, 168);
		this.kriter_double5_baslangic.Name = "kriter_double5_baslangic";
		this.kriter_double5_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_double5_baslangic.Properties.IsFloatValue = false;
		this.kriter_double5_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_double5_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_double5_baslangic.TabIndex = 342;
		this.kriter_double5_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_double5_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label526.Location = new System.Drawing.Point(388, 168);
		this.label526.Name = "label526";
		this.label526.Size = new System.Drawing.Size(69, 19);
		this.label526.TabIndex = 341;
		this.label526.Text = "Sayı 5 :";
		this.label526.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.kriter_double4_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_double4_baslangic.Location = new System.Drawing.Point(463, 142);
		this.kriter_double4_baslangic.Name = "kriter_double4_baslangic";
		this.kriter_double4_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_double4_baslangic.Properties.IsFloatValue = false;
		this.kriter_double4_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_double4_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_double4_baslangic.TabIndex = 339;
		this.kriter_double4_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_double4_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label527.Location = new System.Drawing.Point(388, 142);
		this.label527.Name = "label527";
		this.label527.Size = new System.Drawing.Size(69, 19);
		this.label527.TabIndex = 338;
		this.label527.Text = "Sayı 4 :";
		this.label527.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.kriter_double3_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_double3_baslangic.Location = new System.Drawing.Point(463, 116);
		this.kriter_double3_baslangic.Name = "kriter_double3_baslangic";
		this.kriter_double3_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_double3_baslangic.Properties.IsFloatValue = false;
		this.kriter_double3_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_double3_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_double3_baslangic.TabIndex = 336;
		this.kriter_double3_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_double3_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label528.Location = new System.Drawing.Point(388, 116);
		this.label528.Name = "label528";
		this.label528.Size = new System.Drawing.Size(69, 19);
		this.label528.TabIndex = 335;
		this.label528.Text = "Sayı 3 :";
		this.label528.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.kriter_double2_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_double2_baslangic.Location = new System.Drawing.Point(463, 90);
		this.kriter_double2_baslangic.Name = "kriter_double2_baslangic";
		this.kriter_double2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_double2_baslangic.Properties.IsFloatValue = false;
		this.kriter_double2_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_double2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_double2_baslangic.TabIndex = 333;
		this.kriter_double2_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_double2_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label529.Location = new System.Drawing.Point(388, 90);
		this.label529.Name = "label529";
		this.label529.Size = new System.Drawing.Size(69, 19);
		this.label529.TabIndex = 332;
		this.label529.Text = "Sayı 2 :";
		this.label529.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label531.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label531.Location = new System.Drawing.Point(463, 43);
		this.label531.Name = "label531";
		this.label531.Size = new System.Drawing.Size(96, 19);
		this.label531.TabIndex = 330;
		this.label531.Text = "Sıra";
		this.label531.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.kriter_double1_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_double1_baslangic.Location = new System.Drawing.Point(463, 64);
		this.kriter_double1_baslangic.Name = "kriter_double1_baslangic";
		this.kriter_double1_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_double1_baslangic.Properties.IsFloatValue = false;
		this.kriter_double1_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_double1_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_double1_baslangic.TabIndex = 328;
		this.kriter_double1_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_double1_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label532.Location = new System.Drawing.Point(388, 64);
		this.label532.Name = "label532";
		this.label532.Size = new System.Drawing.Size(69, 19);
		this.label532.TabIndex = 327;
		this.label532.Text = "Sayı 1 :";
		this.label532.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label524.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label524.Location = new System.Drawing.Point(23, 24);
		this.label524.Name = "label524";
		this.label524.Size = new System.Drawing.Size(234, 19);
		this.label524.TabIndex = 326;
		this.label524.Text = "Metin alanlar";
		this.label524.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.kriter_metin5_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_metin5_baslangic.Location = new System.Drawing.Point(101, 168);
		this.kriter_metin5_baslangic.Name = "kriter_metin5_baslangic";
		this.kriter_metin5_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_metin5_baslangic.Properties.IsFloatValue = false;
		this.kriter_metin5_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_metin5_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_metin5_baslangic.TabIndex = 324;
		this.kriter_metin5_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_metin5_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label523.Location = new System.Drawing.Point(26, 168);
		this.label523.Name = "label523";
		this.label523.Size = new System.Drawing.Size(69, 19);
		this.label523.TabIndex = 323;
		this.label523.Text = "Metin 5 :";
		this.label523.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.kriter_metin4_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_metin4_baslangic.Location = new System.Drawing.Point(101, 142);
		this.kriter_metin4_baslangic.Name = "kriter_metin4_baslangic";
		this.kriter_metin4_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_metin4_baslangic.Properties.IsFloatValue = false;
		this.kriter_metin4_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_metin4_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_metin4_baslangic.TabIndex = 321;
		this.kriter_metin4_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_metin4_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label515.Location = new System.Drawing.Point(26, 142);
		this.label515.Name = "label515";
		this.label515.Size = new System.Drawing.Size(69, 19);
		this.label515.TabIndex = 320;
		this.label515.Text = "Metin 4 :";
		this.label515.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.kriter_metin3_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_metin3_baslangic.Location = new System.Drawing.Point(101, 116);
		this.kriter_metin3_baslangic.Name = "kriter_metin3_baslangic";
		this.kriter_metin3_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_metin3_baslangic.Properties.IsFloatValue = false;
		this.kriter_metin3_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_metin3_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_metin3_baslangic.TabIndex = 318;
		this.kriter_metin3_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_metin3_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label508.Location = new System.Drawing.Point(26, 116);
		this.label508.Name = "label508";
		this.label508.Size = new System.Drawing.Size(69, 19);
		this.label508.TabIndex = 317;
		this.label508.Text = "Metin 3 :";
		this.label508.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.kriter_metin2_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_metin2_baslangic.Location = new System.Drawing.Point(101, 90);
		this.kriter_metin2_baslangic.Name = "kriter_metin2_baslangic";
		this.kriter_metin2_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_metin2_baslangic.Properties.IsFloatValue = false;
		this.kriter_metin2_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_metin2_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_metin2_baslangic.TabIndex = 315;
		this.kriter_metin2_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_metin2_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label507.Location = new System.Drawing.Point(26, 90);
		this.label507.Name = "label507";
		this.label507.Size = new System.Drawing.Size(69, 19);
		this.label507.TabIndex = 314;
		this.label507.Text = "Metin 2 :";
		this.label507.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label505.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label505.Location = new System.Drawing.Point(101, 43);
		this.label505.Name = "label505";
		this.label505.Size = new System.Drawing.Size(96, 19);
		this.label505.TabIndex = 312;
		this.label505.Text = "Sıra";
		this.label505.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.kriter_metin1_baslangic.EditValue = new decimal(new int[4]);
		this.kriter_metin1_baslangic.Location = new System.Drawing.Point(101, 64);
		this.kriter_metin1_baslangic.Name = "kriter_metin1_baslangic";
		this.kriter_metin1_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.kriter_metin1_baslangic.Properties.IsFloatValue = false;
		this.kriter_metin1_baslangic.Properties.Mask.EditMask = "N00";
		this.kriter_metin1_baslangic.Size = new System.Drawing.Size(96, 20);
		this.kriter_metin1_baslangic.TabIndex = 310;
		this.kriter_metin1_baslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.kriter_metin1_baslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label506.Location = new System.Drawing.Point(26, 64);
		this.label506.Name = "label506";
		this.label506.Size = new System.Drawing.Size(69, 19);
		this.label506.TabIndex = 309;
		this.label506.Text = "Metin 1 :";
		this.label506.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label718.AutoSize = true;
		this.label718.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.label718.Location = new System.Drawing.Point(12, 40);
		this.label718.Name = "label718";
		this.label718.Size = new System.Drawing.Size(64, 14);
		this.label718.TabIndex = 104;
		this.label718.Text = "Şablonlar";
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.dosyaToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(967, 24);
		this.menuStrip1.TabIndex = 105;
		this.menuStrip1.Text = "menuStrip1";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.degisiklikleriKaydetToolStripMenuItem, this.sablonEkleToolStripMenuItem, this.sablonSilToolStripMenuItem, this.toolStripSeparator1, this.dosyayaYazToolStripMenuItem, this.dosyadanOkuToolStripMenuItem });
		this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
		this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
		this.dosyaToolStripMenuItem.Text = "Dosya";
		this.degisiklikleriKaydetToolStripMenuItem.Name = "degisiklikleriKaydetToolStripMenuItem";
		this.degisiklikleriKaydetToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.degisiklikleriKaydetToolStripMenuItem.Text = "Kaydet";
		this.degisiklikleriKaydetToolStripMenuItem.Click += new System.EventHandler(degisiklikleriKaydetToolStripMenuItem_Click);
		this.sablonSilToolStripMenuItem.Name = "sablonSilToolStripMenuItem";
		this.sablonSilToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.sablonSilToolStripMenuItem.Text = "Sil";
		this.sablonSilToolStripMenuItem.Click += new System.EventHandler(sablonSilToolStripMenuItem_Click);
		this.sablonEkleToolStripMenuItem.Name = "sablonEkleToolStripMenuItem";
		this.sablonEkleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.sablonEkleToolStripMenuItem.Text = "Ekle";
		this.sablonEkleToolStripMenuItem.Click += new System.EventHandler(sablonEkleToolStripMenuItem_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
		this.dosyayaYazToolStripMenuItem.Name = "dosyayaYazToolStripMenuItem";
		this.dosyayaYazToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.dosyayaYazToolStripMenuItem.Text = "Dosyaya yaz";
		this.dosyayaYazToolStripMenuItem.Click += new System.EventHandler(dosyayaYazToolStripMenuItem_Click);
		this.dosyadanOkuToolStripMenuItem.Name = "dosyadanOkuToolStripMenuItem";
		this.dosyadanOkuToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.dosyadanOkuToolStripMenuItem.Text = "Dosyadan oku";
		this.dosyadanOkuToolStripMenuItem.Click += new System.EventHandler(dosyadanOkuToolStripMenuItem_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(967, 662);
		base.Controls.Add(this.label718);
		base.Controls.Add(this.tc_ayarlar);
		base.Controls.Add(this.lb_kullanicilar);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "SqlImportAktarimParametreleriDuzenle";
		this.Text = "Tahsilat aktarım şablonları düzenleme (SQL)";
		base.Load += new System.EventHandler(GenelParametreler_Load);
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.tc_ayarlar).EndInit();
		this.tc_ayarlar.ResumeLayout(false);
		this.xtraTabPage1.ResumeLayout(false);
		this.xtraTabPage1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.Query.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DBName.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SqlPassword.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SqlUserName.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.KriterListesi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SqlServer.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SqlServerPort.Properties).EndInit();
		this.xtraTabPage6.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dbc_no.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kayit_id_otomatik_ver.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kayit_id_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.sube_no.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.firma_no.Properties).EndInit();
		this.xtraTabPage5.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl2).EndInit();
		this.xtraTabControl2.ResumeLayout(false);
		this.xtraTabPage16.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.cari_banka_hesap_no_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_unvan_turkce_karakterleri_kaldir.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_unvan2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_unvan_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_eposta_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kod2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_tc_kimlik_no_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_vergi_no_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kod_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kod_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kod_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kodu_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_kodu_on_ek_satis.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_arama_secenekleri.Properties).EndInit();
		this.xtraTabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.evrak_sira_otomatik_ver.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.evrak_seri_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.evrak_seri_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.evrak_seri_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.evrak_tarihi_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.evrak_sira_baslangic.Properties).EndInit();
		this.xtraTabPage8.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.belge_no_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.belge_tarihi_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kur_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.plasiyer_kodu_cariden_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.plasiyer_kodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.plasiyer_kodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.plasiyer_kodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.sor_mer_kodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.sor_mer_kodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.sor_mer_kodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.proje_kodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.proje_kodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.proje_kodu_sabit_kullan.Properties).EndInit();
		this.xtraTabPage9.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.aciklama10_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama10_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama9_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama9_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama8_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama8_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama7_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama7_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama6_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama6_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama5_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama5_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama4_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama4_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama3_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama3_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama2_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama2_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama1_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama1_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama10_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama10_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama10_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama9_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama9_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama9_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama8_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama8_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama8_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama7_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama7_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama7_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama6_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama6_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama6_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama5_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama5_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama5_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama4_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama4_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama4_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama3_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama3_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama3_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama2_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama2_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama1_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama1_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.aciklama1_sabit_kullan.Properties).EndInit();
		this.xtraTabPage10.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_3_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_3_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_3_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_2_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_2_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_1_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_1_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ozel_alan_1_sabit_kullan.Properties).EndInit();
		this.xtraTabPage35.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl8).EndInit();
		this.xtraTabControl8.ResumeLayout(false);
		this.xtraTabPage36.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl9).EndInit();
		this.xtraTabControl9.ResumeLayout(false);
		this.xtraTabPage27.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.cari_Portal_Enabled.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_satis_isk_kod_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_satis_isk_kod_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_satis_isk_kod_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sicil_no_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sicil_no_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sicil_no_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_CepTel_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_CepTel_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_CepTel_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_wwwadresi_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_wwwadresi_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_wwwadresi_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Ana_cari_kodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Ana_cari_kodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Ana_cari_kodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_temsilci_kodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_temsilci_kodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_temsilci_kodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_vergi_dairesi_baslangic.Properties).EndInit();
		this.xtraTabPage41.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.cari_muhartikeli_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muhartikeli_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod2_satis_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod2_satis_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod1_satis_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod1_satis_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod_satis_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod_satis_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muhartikeli.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod2_satis.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod1_satis.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_muh_kod_satis.Properties).EndInit();
		this.xtraTabPage39.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.cari_il_bilgisi_plaka_kodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_telefon_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_posta_kodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_ulke_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_il_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_ilce_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_mahalle_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_adres_baslangic.Properties).EndInit();
		this.xtraTabPage31.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanCikisDepo_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanCikisDepo_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanCikisDepo_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanGirisDepo_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanGirisDepo_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_VarsayilanGirisDepo_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Portal_PW_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Portal_PW_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_Portal_PW_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_bolge_kodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_bolge_kodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_bolge_kodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sektor_kodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sektor_kodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_sektor_kodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_grup_kodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_grup_kodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cari_grup_kodu_sabit_kullan.Properties).EndInit();
		this.xtraTabPage17.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.pro_muh_kod_artikeli_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_muh_kod_artikeli_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_muh_kod_artikeli_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_aciklama_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_aciklama_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_aciklama_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_ana_projekodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_ana_projekodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_ana_projekodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_bolgekodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_bolgekodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_bolgekodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sektorkodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sektorkodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sektorkodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_grupkodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_grupkodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_grupkodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sormerkodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sormerkodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_sormerkodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_musterikodu_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_musterikodu_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_musterikodu_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_adi_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_adi_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pro_adi_sabit_kullan.Properties).EndInit();
		this.xtraTabPage40.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.som_MuhArtikeli_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.som_MuhArtikeli_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.som_MuhArtikeli_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.som_isim_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.som_isim_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.som_isim_sabit_kullan.Properties).EndInit();
		this.xtraTabPage20.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl4).EndInit();
		this.xtraTabControl4.ResumeLayout(false);
		this.xtraTabPage21.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_giden_havale.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_gelen_havale.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_sorumlulukmerkezi_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_sorumlulukmerkezi_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_sorumlulukmerkezi_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_vadesi_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_vadesi_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_vadesi_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_tutar_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_tutar_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_tutar_islem_2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_tutar_islem_1_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_tutar_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_kredi_karti.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_senet.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_aciklama_on_ek_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_aciklama_on_ek_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_aciklama_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_aciklama_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_aciklama_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_cek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_veri_nakit.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_cinsi_sabit_kullan.Properties).EndInit();
		this.xtraTabPage7.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_giden_havale_son_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_giden_havale_on_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_giden_havale_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_giden_havale_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_giden_havale_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_gelen_havale_son_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_gelen_havale_on_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_gelen_havale_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_gelen_havale_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_gelen_havale_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_kredi_karti_son_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_kredi_karti_on_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_kredi_karti_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_kredi_karti_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_kredi_karti_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_senet_son_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_senet_on_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_senet_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_senet_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_senet_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_cek_son_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_cek_on_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_cek_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_cek_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_cek_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_nakit_son_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_nakit_on_ek.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_nakit_sabit_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_nakit_sabit_kullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.satir_hesap_kodu_nakit_baslangic.Properties).EndInit();
		this.xtraTabPage4.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.kriter_bool5_evet_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool4_evet_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool3_evet_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool2_evet_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool5_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool4_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool3_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool1_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_bool1_evet_icin_deger.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_double5_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_double4_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_double3_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_double2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_double1_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_metin5_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_metin4_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_metin3_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_metin2_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kriter_metin1_baslangic.Properties).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
