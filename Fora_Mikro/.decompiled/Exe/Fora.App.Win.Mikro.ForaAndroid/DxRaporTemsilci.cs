using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Fora.Mikro.Rapor.Genel;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class DxRaporTemsilci : XtraReport
{
	private IContainer components;

	private DetailBand Genel_Ozet;

	private TopMarginBand TopMargin;

	private BottomMarginBand BottomMargin;

	private BindingSource bindingSource1;

	private XRPageInfo xrPageInfo1;

	private FormattingRule formattingRule1;

	private XRControlStyle xrControlStyle1;

	private XRControlStyle xrControlStyle2;

	private DetailReportBand Bolgeler;

	private DetailBand Bolge_Genel_Ozet;

	private DetailReportBand Temsilciler;

	private DetailBand Temsilci_Ozet;

	private DetailReportBand Temsilci_Gunluk_Hareket;

	private DetailBand Gun_Ozeti;

	private XRLabel xrLabel37;

	private XRLabel xrLabel39;

	private XRLabel xrLabel36;

	private XRTable xrTable3;

	private XRTableRow xrTableRow3;

	private XRTableCell xrTableCell7;

	private XRTableCell xrTableCell8;

	private XRTableCell xrTableCell9;

	private XRTableRow xrTableRow10;

	private XRTableCell xrTableCell28;

	private XRTableCell xrTableCell29;

	private XRTableCell xrTableCell30;

	private XRLabel xrLabel1;

	private XRLabel xrLabel38;

	private XRLabel xrLabel7;

	private DetailReportBand Ziyaret_Listesi;

	private DetailBand Ziyaret_Listesi_Liste;

	private DetailReportBand Hedef_Ziyaretler;

	private DetailBand Hedef_Ziyaretler_Liste;

	private XRTable xrTable19;

	private XRTableRow xrTableRow27;

	private XRTableCell xrTableCell109;

	private XRTableCell xrTableCell110;

	private XRTableCell xrTableCell111;

	private GroupHeaderBand Hedef_Ziyaretler_Baslik;

	private XRLabel xrLabel45;

	private XRTable xrTable18;

	private XRTableRow xrTableRow26;

	private XRTableCell xrTableCell103;

	private XRTableCell xrTableCell107;

	private XRTableCell xrTableCell108;

	private DetailReportBand Yapilmayan_Ziyaretler;

	private DetailBand Yapilmayan_Ziyaret_Listesi;

	private XRTable xrTable5;

	private XRTableRow xrTableRow13;

	private XRTableCell xrTableCell41;

	private XRTableCell xrTableCell42;

	private XRTableCell xrTableCell43;

	private GroupHeaderBand Yapilmayan_Ziyaretler_Baslik;

	private XRTable xrTable4;

	private XRTableRow xrTableRow12;

	private XRTableCell xrTableCell35;

	private XRTableCell xrTableCell39;

	private XRTableCell xrTableCell40;

	private XRLabel xrLabel4;

	private DetailReportBand Yapilan_Ziyaretler;

	private DetailBand Yapilan_Ziyaret_Listesi;

	private XRTable xrTable7;

	private XRTableRow xrTableRow15;

	private XRTableCell xrTableCell49;

	private XRTableCell xrTableCell112;

	private XRTableCell xrTableCell113;

	private XRTableCell xrTableCell114;

	private XRTableCell xrTableCell115;

	private GroupHeaderBand Yapilan_Ziyaretler_Baslik;

	private XRTable xrTable6;

	private XRTableRow xrTableRow14;

	private XRTableCell xrTableCell44;

	private XRTableCell xrTableCell45;

	private XRTableCell xrTableCell46;

	private XRTableCell xrTableCell47;

	private XRTableCell xrTableCell48;

	private XRLabel xrLabel5;

	private DetailReportBand Rota_Disi_Ziyaretler;

	private DetailBand Rota_Disi_Ziyaret_Listesi;

	private XRTable xrTable2;

	private XRTableRow xrTableRow2;

	private XRTableCell xrTableCell6;

	private XRTableCell xrTableCell34;

	private XRTableCell xrTableCell36;

	private XRTableCell xrTableCell37;

	private XRTableCell xrTableCell38;

	private GroupHeaderBand Rota_Disi_Ziyaretler_Baslik;

	private XRLabel xrLabel3;

	private XRTable xrTable1;

	private XRTableRow xrTableRow1;

	private XRTableCell xrTableCell1;

	private XRTableCell xrTableCell2;

	private XRTableCell xrTableCell3;

	private XRTableCell xrTableCell4;

	private XRTableCell xrTableCell5;

	private DetailReportBand Siparisler;

	private DetailBand Siparis_Listesi;

	private XRTable xrTable9;

	private XRTableRow xrTableRow17;

	private XRTableCell xrTableCell55;

	private XRTableCell xrTableCell56;

	private XRTableCell xrTableCell57;

	private XRTableCell xrTableCell58;

	private XRTableCell xrTableCell59;

	private GroupHeaderBand Siparisler_Baslik;

	private XRTable xrTable8;

	private XRTableRow xrTableRow16;

	private XRTableCell xrTableCell50;

	private XRTableCell xrTableCell51;

	private XRTableCell xrTableCell52;

	private XRTableCell xrTableCell53;

	private XRTableCell xrTableCell54;

	private XRLabel xrLabel6;

	private DetailReportBand Faturalar;

	private DetailBand Fatura_Listesi;

	private DetailReportBand Tahsilatlar;

	private DetailBand Tahsilat_Listesi;

	private DetailReportBand Masraflar;

	private DetailBand Masraf_Listesi;

	private XRTable xrTable11;

	private XRTableRow xrTableRow19;

	private XRTableCell xrTableCell65;

	private XRTableCell xrTableCell66;

	private XRTableCell xrTableCell67;

	private XRTableCell xrTableCell68;

	private XRTableCell xrTableCell69;

	private GroupHeaderBand Faturalar_Baslik;

	private XRLabel xrLabel8;

	private XRTable xrTable10;

	private XRTableRow xrTableRow18;

	private XRTableCell xrTableCell60;

	private XRTableCell xrTableCell61;

	private XRTableCell xrTableCell62;

	private XRTableCell xrTableCell63;

	private XRTableCell xrTableCell64;

	private XRTable xrTable13;

	private XRTableRow xrTableRow21;

	private XRTableCell xrTableCell77;

	private XRTableCell xrTableCell78;

	private XRTableCell xrTableCell79;

	private XRTableCell xrTableCell116;

	private XRTableCell xrTableCell117;

	private XRTableCell xrTableCell118;

	private XRTableCell xrTableCell119;

	private GroupHeaderBand Tahsilatlar_baslik;

	private XRTable xrTable12;

	private XRTableRow xrTableRow20;

	private XRTableCell xrTableCell70;

	private XRTableCell xrTableCell71;

	private XRTableCell xrTableCell72;

	private XRTableCell xrTableCell73;

	private XRTableCell xrTableCell74;

	private XRTableCell xrTableCell75;

	private XRTableCell xrTableCell76;

	private XRLabel xrLabel33;

	private XRTable xrTable15;

	private XRTableRow xrTableRow23;

	private XRTableCell xrTableCell86;

	private XRTableCell xrTableCell87;

	private XRTableCell xrTableCell88;

	private XRTableCell xrTableCell89;

	private XRTableCell xrTableCell90;

	private XRTableCell xrTableCell91;

	private GroupHeaderBand Masraflar_Baslik;

	private XRLabel xrLabel34;

	private XRTable xrTable14;

	private XRTableRow xrTableRow22;

	private XRTableCell xrTableCell80;

	private XRTableCell xrTableCell81;

	private XRTableCell xrTableCell82;

	private XRTableCell xrTableCell83;

	private XRTableCell xrTableCell84;

	private XRTableCell xrTableCell85;

	private XRTable xrTable22;

	private XRTableRow xrTableRow24;

	private XRTableCell xrTableCell33;

	private XRTableRow xrTableRow40;

	private XRTableCell xrTableCell124;

	private XRTableCell xrTableCell136;

	private XRTableCell xrTableCell141;

	private XRTableRow xrTableRow43;

	private XRTableCell xrTableCell142;

	private XRTableCell xrTableCell143;

	private XRTableCell xrTableCell144;

	private XRTableRow xrTableRow44;

	private XRTableCell xrTableCell146;

	private XRTableCell xrTableCell149;

	private XRTableCell xrTableCell152;

	private XRTable xrTable23;

	private XRTableRow xrTableRow45;

	private XRTableCell xrTableCell138;

	private XRTableRow xrTableRow48;

	private XRTableCell xrTableCell145;

	private XRTableCell xrTableCell147;

	private XRTableRow xrTableRow49;

	private XRTableCell xrTableCell148;

	private XRTableCell xrTableCell150;

	private XRTableRow xrTableRow50;

	private XRTableCell xrTableCell151;

	private XRTableCell xrTableCell153;

	private XRTable xrTable21;

	private XRTableRow xrTableRow39;

	private XRTableCell xrTableCell26;

	private XRTableRow xrTableRow6;

	private XRTableCell xrTableCell16;

	private XRTableCell xrTableCell92;

	private XRTableCell xrTableCell93;

	private XRTableRow xrTableRow41;

	private XRTableCell xrTableCell122;

	private XRTableCell xrTableCell125;

	private XRTableCell xrTableCell18;

	private XRTableRow xrTableRow42;

	private XRTableCell xrTableCell135;

	private XRTableCell xrTableCell137;

	private XRTableCell xrTableCell31;

	private XRTableRow xrTableRow46;

	private XRTableCell xrTableCell139;

	private XRTableCell xrTableCell140;

	private XRTableCell xrTableCell32;

	private XRTable xrTable20;

	private XRTableRow xrTableRow7;

	private XRTableCell xrTableCell19;

	private XRTableRow xrTableRow8;

	private XRTableCell xrTableCell22;

	private XRTableCell xrTableCell24;

	private XRTableRow xrTableRow9;

	private XRTableCell xrTableCell25;

	private XRTableCell xrTableCell27;

	private XRTableRow xrTableRow34;

	private XRTableCell xrTableCell121;

	private XRTableCell xrTableCell123;

	private XRTable xrTable17;

	private XRTableRow xrTableRow33;

	private XRTableCell xrTableCell120;

	private XRTableRow xrTableRow32;

	private XRTableCell xrTableCell97;

	private XRTableCell xrTableCell100;

	private XRTableCell xrTableCell104;

	private XRTableRow xrTableRow35;

	private XRTableCell xrTableCell126;

	private XRTableCell xrTableCell127;

	private XRTableCell xrTableCell128;

	private XRTableRow xrTableRow36;

	private XRTableCell xrTableCell129;

	private XRTableCell xrTableCell130;

	private XRTableCell xrTableCell131;

	private XRTableRow xrTableRow37;

	private XRTableCell xrTableCell132;

	private XRTableCell xrTableCell133;

	private XRTableCell xrTableCell134;

	private XRTable xrTable16;

	private XRTableRow xrTableRow31;

	private XRTableCell xrTableCell94;

	private XRTableRow xrTableRow25;

	private XRTableCell xrTableCell95;

	private XRTableCell xrTableCell96;

	private XRTableRow xrTableRow28;

	private XRTableCell xrTableCell98;

	private XRTableCell xrTableCell99;

	private XRTableRow xrTableRow29;

	private XRTableCell xrTableCell101;

	private XRTableCell xrTableCell102;

	private XRTableRow xrTableRow30;

	private XRTableCell xrTableCell105;

	private XRTableCell xrTableCell106;

	private XRLabel xrLabel35;

	private XRTableRow xrTableRow38;

	private XRTableCell xrTableCell23;

	private XRTableRow xrTableRow11;

	private XRTableCell xrTableCell17;

	private XRTableCell xrTableCell20;

	private XRTableCell xrTableCell21;

	private XRTable xrTable25;

	private XRTableRow xrTableRow5;

	private XRTableCell xrTableCell13;

	private XRTableCell xrTableCell14;

	private XRTableCell xrTableCell15;

	private GroupHeaderBand GroupHeader1;

	private XRTable xrTable24;

	private XRTableRow xrTableRow4;

	private XRTableCell xrTableCell155;

	private XRTableCell xrTableCell154;

	private XRTableCell xrTableCell10;

	private XRTableCell xrTableCell11;

	private XRTableCell xrTableCell12;

	private XRLabel xrLabel9;

	private XRTableCell xrTableCell156;

	private XRTableCell xrTableCell159;

	private XRTableCell xrTableCell158;

	private XRTableCell xrTableCell157;

	private XRLabel xrLabel2;

	private XRLabel xrLabel12;

	private XRLabel xrLabel11;

	private XRLabel xrLabel10;

	private XRLabel xrLabel13;

	private XRLabel xrLabel14;

	private XRLabel xrLabel15;

	private XRTable xrTable26;

	private XRTableRow xrTableRow51;

	private XRTableCell xrTableCell161;

	private XRTableCell xrTableCell162;

	private XRTableCell xrTableCell163;

	private XRTableCell xrTableCell160;

	private XRTableRow xrTableRow52;

	private XRTableCell xrTableCell164;

	private XRTableCell xrTableCell165;

	private XRTableCell xrTableCell166;

	private XRTableCell xrTableCell210;

	private XRTableRow xrTableRow53;

	private XRTableCell xrTableCell167;

	private XRTableCell xrTableCell169;

	private XRTableCell xrTableCell211;

	private XRTableRow xrTableRow54;

	private XRTableCell xrTableCell170;

	private XRTableCell xrTableCell171;

	private XRTableCell xrTableCell172;

	private XRTableCell xrTableCell212;

	private XRTableRow xrTableRow55;

	private XRTableCell xrTableCell173;

	private XRTableCell xrTableCell174;

	private XRTableCell xrTableCell175;

	private XRTableCell xrTableCell213;

	private XRTableRow xrTableRow56;

	private XRTableCell xrTableCell176;

	private XRTableCell xrTableCell177;

	private XRTableCell xrTableCell178;

	private XRTableCell xrTableCell214;

	private XRTableRow xrTableRow57;

	private XRTableCell xrTableCell179;

	private XRTableCell xrTableCell180;

	private XRTableCell xrTableCell181;

	private XRTableCell xrTableCell215;

	private XRTableRow xrTableRow66;

	private XRTableCell xrTableCell204;

	private XRTableCell xrTableCell205;

	private XRTableCell xrTableCell206;

	private XRTableCell xrTableCell216;

	private XRTableRow xrTableRow67;

	private XRTableCell xrTableCell207;

	private XRTableCell xrTableCell208;

	private XRTableCell xrTableCell209;

	private XRTableCell xrTableCell217;

	private XRTable xrTable27;

	private XRTableRow xrTableRow47;

	private XRTableCell xrTableCell182;

	private XRTableCell xrTableCell183;

	private XRTableCell xrTableCell184;

	private XRTableCell xrTableCell185;

	private XRTableRow xrTableRow68;

	private XRTableCell xrTableCell232;

	private XRTableCell xrTableCell233;

	private XRTableCell xrTableCell246;

	private XRTableCell xrTableCell234;

	private XRTableCell xrTableCell235;

	private XRTableRow xrTableRow58;

	private XRTableCell xrTableCell186;

	private XRTableCell xrTableCell187;

	private XRTableCell xrTableCell245;

	private XRTableCell xrTableCell188;

	private XRTableCell xrTableCell189;

	private XRTableRow xrTableRow59;

	private XRTableCell xrTableCell190;

	private XRTableCell xrTableCell191;

	private XRTableCell xrTableCell247;

	private XRTableCell xrTableCell192;

	private XRTableCell xrTableCell193;

	private XRTableRow xrTableRow60;

	private XRTableCell xrTableCell194;

	private XRTableCell xrTableCell195;

	private XRTableCell xrTableCell248;

	private XRTableCell xrTableCell196;

	private XRTableCell xrTableCell197;

	private XRTableRow xrTableRow61;

	private XRTableCell xrTableCell198;

	private XRTableCell xrTableCell199;

	private XRTableCell xrTableCell249;

	private XRTableCell xrTableCell200;

	private XRTableCell xrTableCell201;

	private XRTableRow xrTableRow62;

	private XRTableCell xrTableCell202;

	private XRTableCell xrTableCell203;

	private XRTableCell xrTableCell250;

	private XRTableCell xrTableCell218;

	private XRTableCell xrTableCell219;

	private XRTableRow xrTableRow63;

	private XRTableCell xrTableCell220;

	private XRTableCell xrTableCell221;

	private XRTableCell xrTableCell251;

	private XRTableCell xrTableCell222;

	private XRTableCell xrTableCell223;

	private XRTableRow xrTableRow64;

	private XRTableCell xrTableCell224;

	private XRTableCell xrTableCell225;

	private XRTableCell xrTableCell252;

	private XRTableCell xrTableCell226;

	private XRTableCell xrTableCell227;

	private XRTableRow xrTableRow65;

	private XRTableCell xrTableCell228;

	private XRTableCell xrTableCell229;

	private XRTableCell xrTableCell253;

	private XRTableCell xrTableCell230;

	private XRTableCell xrTableCell231;

	private XRLabel xrLabel16;

	private XRLabel xrLabel17;

	private XRLabel xrLabel21;

	private XRLabel xrLabel22;

	private XRLabel xrLabel19;

	private XRLabel xrLabel20;

	private XRLabel xrLabel18;

	private XRLabel xrLabel23;

	private XRLabel xrLabel24;

	private XRLabel xrLabel25;

	private XRLabel xrLabel26;

	private XRTableCell xrTableCell313;

	private XRTableCell xrTableCell315;

	private XRTableCell xrTableCell316;

	private XRTableCell xrTableCell312;

	private XRTableCell xrTableCell317;

	private XRTableCell xrTableCell314;

	private XRTableCell xrTableCell318;

	private XRTableCell xrTableCell319;

	private XRTableCell xrTableCell320;

	private XRTableCell xrTableCell168;

	private XRTable xrTable28;

	private XRTableRow xrTableRow69;

	private XRTableCell xrTableCell236;

	private XRTableCell xrTableCell237;

	private XRTableCell xrTableCell238;

	private XRTableCell xrTableCell239;

	private XRTableRow xrTableRow70;

	private XRTableCell xrTableCell240;

	private XRTableCell xrTableCell241;

	private XRTableCell xrTableCell242;

	private XRTableCell xrTableCell243;

	private XRTableRow xrTableRow71;

	private XRTableCell xrTableCell244;

	private XRTableCell xrTableCell254;

	private XRTableCell xrTableCell255;

	private XRTableCell xrTableCell256;

	private XRTableRow xrTableRow72;

	private XRTableCell xrTableCell257;

	private XRTableCell xrTableCell258;

	private XRTableCell xrTableCell259;

	private XRTableCell xrTableCell260;

	private XRTableRow xrTableRow73;

	private XRTableCell xrTableCell261;

	private XRTableCell xrTableCell262;

	private XRTableCell xrTableCell263;

	private XRTableCell xrTableCell264;

	private XRTableRow xrTableRow74;

	private XRTableCell xrTableCell265;

	private XRTableCell xrTableCell266;

	private XRTableCell xrTableCell267;

	private XRTableCell xrTableCell268;

	private XRTableRow xrTableRow75;

	private XRTableCell xrTableCell269;

	private XRTableCell xrTableCell270;

	private XRTableCell xrTableCell271;

	private XRTableCell xrTableCell272;

	private XRTableRow xrTableRow76;

	private XRTableCell xrTableCell273;

	private XRTableCell xrTableCell274;

	private XRTableCell xrTableCell275;

	private XRTableCell xrTableCell276;

	private XRTableRow xrTableRow77;

	private XRTableCell xrTableCell277;

	private XRTableCell xrTableCell278;

	private XRTableCell xrTableCell279;

	private XRTableCell xrTableCell280;

	private XRTable xrTable29;

	private XRTableRow xrTableRow78;

	private XRTableCell xrTableCell281;

	private XRTableCell xrTableCell282;

	private XRTableCell xrTableCell283;

	private XRTableCell xrTableCell284;

	private XRTableRow xrTableRow79;

	private XRTableCell xrTableCell285;

	private XRTableCell xrTableCell286;

	private XRTableCell xrTableCell287;

	private XRTableCell xrTableCell288;

	private XRTableCell xrTableCell289;

	private XRTableCell xrTableCell290;

	private XRTableRow xrTableRow80;

	private XRTableCell xrTableCell291;

	private XRTableCell xrTableCell292;

	private XRTableCell xrTableCell293;

	private XRTableCell xrTableCell294;

	private XRTableCell xrTableCell295;

	private XRTableCell xrTableCell296;

	private XRTableRow xrTableRow81;

	private XRTableCell xrTableCell297;

	private XRTableCell xrTableCell298;

	private XRTableCell xrTableCell299;

	private XRTableCell xrTableCell300;

	private XRTableCell xrTableCell301;

	private XRTableCell xrTableCell302;

	private XRTableRow xrTableRow82;

	private XRTableCell xrTableCell303;

	private XRTableCell xrTableCell304;

	private XRTableCell xrTableCell305;

	private XRTableCell xrTableCell306;

	private XRTableCell xrTableCell307;

	private XRTableCell xrTableCell308;

	private XRTableRow xrTableRow83;

	private XRTableCell xrTableCell309;

	private XRTableCell xrTableCell310;

	private XRTableCell xrTableCell311;

	private XRTableCell xrTableCell321;

	private XRTableCell xrTableCell322;

	private XRTableCell xrTableCell323;

	private XRTableRow xrTableRow84;

	private XRTableCell xrTableCell324;

	private XRTableCell xrTableCell325;

	private XRTableCell xrTableCell326;

	private XRTableCell xrTableCell327;

	private XRTableCell xrTableCell328;

	private XRTableCell xrTableCell329;

	private XRTableRow xrTableRow85;

	private XRTableCell xrTableCell330;

	private XRTableCell xrTableCell331;

	private XRTableCell xrTableCell332;

	private XRTableCell xrTableCell333;

	private XRTableCell xrTableCell334;

	private XRTableCell xrTableCell335;

	private XRTableRow xrTableRow86;

	private XRTableCell xrTableCell336;

	private XRTableCell xrTableCell337;

	private XRTableCell xrTableCell338;

	private XRTableCell xrTableCell339;

	private XRTableCell xrTableCell340;

	private XRTableCell xrTableCell341;

	private XRTableRow xrTableRow87;

	private XRTableCell xrTableCell342;

	private XRTableCell xrTableCell343;

	private XRTableCell xrTableCell344;

	private XRTableCell xrTableCell345;

	private XRTableCell xrTableCell346;

	private XRTableCell xrTableCell347;

	private XRTableCell xrTableCell365;

	private XRTableCell xrTableCell366;

	private XRTableCell xrTableCell368;

	private XRTableCell xrTableCell369;

	private XRTableCell xrTableCell370;

	private XRTableCell xrTableCell372;

	private XRTableCell xrTableCell373;

	private XRTableCell xrTableCell374;

	private XRTableCell xrTableCell375;

	private XRTableCell xrTableCell348;

	private XRTableCell xrTableCell363;

	private XRTableCell xrTableCell349;

	private XRTableCell xrTableCell356;

	private XRTableCell xrTableCell350;

	private XRTableCell xrTableCell357;

	private XRTableCell xrTableCell351;

	private XRTableCell xrTableCell364;

	private XRTableCell xrTableCell355;

	private XRTableCell xrTableCell367;

	private XRTableCell xrTableCell358;

	private XRTableCell xrTableCell371;

	private XRTableCell xrTableCell362;

	private XRTableCell xrTableCell359;

	private XRTableCell xrTableCell352;

	private XRTableCell xrTableCell360;

	private XRTableCell xrTableCell353;

	private XRTableCell xrTableCell361;

	private XRTableCell xrTableCell354;

	private XRLabel xrLabel29;

	private XRLabel xrLabel30;

	private XRLabel xrLabel27;

	private XRLabel xrLabel28;

	private XRTable xrTable30;

	private XRTableRow xrTableRow88;

	private XRTableCell xrTableCell376;

	private XRTableCell xrTableCell377;

	private XRTableCell xrTableCell378;

	private XRTableCell xrTableCell379;

	private XRTableCell xrTableCell380;

	private XRTableRow xrTableRow89;

	private XRTableCell xrTableCell381;

	private XRTableCell xrTableCell382;

	private XRTableCell xrTableCell383;

	private XRTableCell xrTableCell384;

	private XRTableCell xrTableCell385;

	private XRTableCell xrTableCell386;

	private XRTableCell xrTableCell387;

	private XRTableCell xrTableCell388;

	private XRTableRow xrTableRow90;

	private XRTableCell xrTableCell389;

	private XRTableCell xrTableCell390;

	private XRTableCell xrTableCell391;

	private XRTableCell xrTableCell392;

	private XRTableCell xrTableCell393;

	private XRTableCell xrTableCell394;

	private XRTableCell xrTableCell395;

	private XRTableCell xrTableCell396;

	private XRTableRow xrTableRow91;

	private XRTableCell xrTableCell397;

	private XRTableCell xrTableCell398;

	private XRTableCell xrTableCell399;

	private XRTableCell xrTableCell400;

	private XRTableCell xrTableCell401;

	private XRTableCell xrTableCell402;

	private XRTableCell xrTableCell403;

	private XRTableCell xrTableCell404;

	private XRTableRow xrTableRow92;

	private XRTableCell xrTableCell405;

	private XRTableCell xrTableCell406;

	private XRTableCell xrTableCell407;

	private XRTableCell xrTableCell408;

	private XRTableCell xrTableCell409;

	private XRTableCell xrTableCell410;

	private XRTableCell xrTableCell411;

	private XRTableCell xrTableCell412;

	private XRTableRow xrTableRow93;

	private XRTableCell xrTableCell413;

	private XRTableCell xrTableCell414;

	private XRTableCell xrTableCell415;

	private XRTableCell xrTableCell416;

	private XRTableCell xrTableCell417;

	private XRTableCell xrTableCell418;

	private XRTableCell xrTableCell419;

	private XRTableCell xrTableCell420;

	private XRTableRow xrTableRow94;

	private XRTableCell xrTableCell421;

	private XRTableCell xrTableCell422;

	private XRTableCell xrTableCell423;

	private XRTableCell xrTableCell424;

	private XRTableCell xrTableCell425;

	private XRTableCell xrTableCell426;

	private XRTableCell xrTableCell427;

	private XRTableCell xrTableCell428;

	private XRTableRow xrTableRow95;

	private XRTableCell xrTableCell429;

	private XRTableCell xrTableCell430;

	private XRTableCell xrTableCell431;

	private XRTableCell xrTableCell432;

	private XRTableCell xrTableCell433;

	private XRTableCell xrTableCell434;

	private XRTableCell xrTableCell435;

	private XRTableCell xrTableCell436;

	private XRTableRow xrTableRow96;

	private XRTableCell xrTableCell437;

	private XRTableCell xrTableCell438;

	private XRTableCell xrTableCell439;

	private XRTableCell xrTableCell440;

	private XRTableCell xrTableCell441;

	private XRTableCell xrTableCell442;

	private XRTableCell xrTableCell443;

	private XRTableCell xrTableCell444;

	private XRTableRow xrTableRow97;

	private XRTableCell xrTableCell445;

	private XRTableCell xrTableCell446;

	private XRTableCell xrTableCell447;

	private XRTableCell xrTableCell448;

	private XRTableCell xrTableCell449;

	private XRTableCell xrTableCell450;

	private XRTableCell xrTableCell451;

	private XRTableCell xrTableCell452;

	private XRTable xrTable31;

	private XRTableRow xrTableRow98;

	private XRTableCell xrTableCell453;

	private XRTableCell xrTableCell454;

	private XRTableCell xrTableCell455;

	private XRTableCell xrTableCell456;

	private XRTableCell xrTableCell457;

	private XRTableRow xrTableRow99;

	private XRTableCell xrTableCell458;

	private XRTableCell xrTableCell459;

	private XRTableCell xrTableCell460;

	private XRTableCell xrTableCell461;

	private XRTableCell xrTableCell462;

	private XRTableRow xrTableRow100;

	private XRTableCell xrTableCell463;

	private XRTableCell xrTableCell464;

	private XRTableCell xrTableCell465;

	private XRTableCell xrTableCell466;

	private XRTableCell xrTableCell467;

	private XRTableRow xrTableRow101;

	private XRTableCell xrTableCell468;

	private XRTableCell xrTableCell469;

	private XRTableCell xrTableCell470;

	private XRTableCell xrTableCell471;

	private XRTableCell xrTableCell472;

	private XRTableRow xrTableRow102;

	private XRTableCell xrTableCell473;

	private XRTableCell xrTableCell474;

	private XRTableCell xrTableCell475;

	private XRTableCell xrTableCell476;

	private XRTableCell xrTableCell477;

	private XRTableRow xrTableRow103;

	private XRTableCell xrTableCell478;

	private XRTableCell xrTableCell479;

	private XRTableCell xrTableCell480;

	private XRTableCell xrTableCell481;

	private XRTableCell xrTableCell482;

	private XRTableRow xrTableRow104;

	private XRTableCell xrTableCell483;

	private XRTableCell xrTableCell484;

	private XRTableCell xrTableCell485;

	private XRTableCell xrTableCell486;

	private XRTableCell xrTableCell487;

	private XRTableRow xrTableRow105;

	private XRTableCell xrTableCell488;

	private XRTableCell xrTableCell489;

	private XRTableCell xrTableCell490;

	private XRTableCell xrTableCell491;

	private XRTableCell xrTableCell492;

	private XRTableRow xrTableRow106;

	private XRTableCell xrTableCell493;

	private XRTableCell xrTableCell494;

	private XRTableCell xrTableCell495;

	private XRTableCell xrTableCell496;

	private XRTableCell xrTableCell497;

	public DxRaporTemsilci()
	{
		InitializeComponent();
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
		components = new Container();
		Genel_Ozet = new DetailBand();
		xrLabel29 = new XRLabel();
		xrLabel30 = new XRLabel();
		xrLabel27 = new XRLabel();
		xrLabel28 = new XRLabel();
		xrTable30 = new XRTable();
		xrTableRow88 = new XRTableRow();
		xrTableCell376 = new XRTableCell();
		xrTableCell377 = new XRTableCell();
		xrTableCell378 = new XRTableCell();
		xrTableCell379 = new XRTableCell();
		xrTableCell380 = new XRTableCell();
		xrTableRow89 = new XRTableRow();
		xrTableCell381 = new XRTableCell();
		xrTableCell382 = new XRTableCell();
		xrTableCell383 = new XRTableCell();
		xrTableCell384 = new XRTableCell();
		xrTableCell385 = new XRTableCell();
		xrTableCell386 = new XRTableCell();
		xrTableCell387 = new XRTableCell();
		xrTableCell388 = new XRTableCell();
		xrTableRow90 = new XRTableRow();
		xrTableCell389 = new XRTableCell();
		xrTableCell390 = new XRTableCell();
		xrTableCell391 = new XRTableCell();
		xrTableCell392 = new XRTableCell();
		xrTableCell393 = new XRTableCell();
		xrTableCell394 = new XRTableCell();
		xrTableCell395 = new XRTableCell();
		xrTableCell396 = new XRTableCell();
		xrTableRow91 = new XRTableRow();
		xrTableCell397 = new XRTableCell();
		xrTableCell398 = new XRTableCell();
		xrTableCell399 = new XRTableCell();
		xrTableCell400 = new XRTableCell();
		xrTableCell401 = new XRTableCell();
		xrTableCell402 = new XRTableCell();
		xrTableCell403 = new XRTableCell();
		xrTableCell404 = new XRTableCell();
		xrTableRow92 = new XRTableRow();
		xrTableCell405 = new XRTableCell();
		xrTableCell406 = new XRTableCell();
		xrTableCell407 = new XRTableCell();
		xrTableCell408 = new XRTableCell();
		xrTableCell409 = new XRTableCell();
		xrTableCell410 = new XRTableCell();
		xrTableCell411 = new XRTableCell();
		xrTableCell412 = new XRTableCell();
		xrTableRow93 = new XRTableRow();
		xrTableCell413 = new XRTableCell();
		xrTableCell414 = new XRTableCell();
		xrTableCell415 = new XRTableCell();
		xrTableCell416 = new XRTableCell();
		xrTableCell417 = new XRTableCell();
		xrTableCell418 = new XRTableCell();
		xrTableCell419 = new XRTableCell();
		xrTableCell420 = new XRTableCell();
		xrTableRow94 = new XRTableRow();
		xrTableCell421 = new XRTableCell();
		xrTableCell422 = new XRTableCell();
		xrTableCell423 = new XRTableCell();
		xrTableCell424 = new XRTableCell();
		xrTableCell425 = new XRTableCell();
		xrTableCell426 = new XRTableCell();
		xrTableCell427 = new XRTableCell();
		xrTableCell428 = new XRTableCell();
		xrTableRow95 = new XRTableRow();
		xrTableCell429 = new XRTableCell();
		xrTableCell430 = new XRTableCell();
		xrTableCell431 = new XRTableCell();
		xrTableCell432 = new XRTableCell();
		xrTableCell433 = new XRTableCell();
		xrTableCell434 = new XRTableCell();
		xrTableCell435 = new XRTableCell();
		xrTableCell436 = new XRTableCell();
		xrTableRow96 = new XRTableRow();
		xrTableCell437 = new XRTableCell();
		xrTableCell438 = new XRTableCell();
		xrTableCell439 = new XRTableCell();
		xrTableCell440 = new XRTableCell();
		xrTableCell441 = new XRTableCell();
		xrTableCell442 = new XRTableCell();
		xrTableCell443 = new XRTableCell();
		xrTableCell444 = new XRTableCell();
		xrTableRow97 = new XRTableRow();
		xrTableCell445 = new XRTableCell();
		xrTableCell446 = new XRTableCell();
		xrTableCell447 = new XRTableCell();
		xrTableCell448 = new XRTableCell();
		xrTableCell449 = new XRTableCell();
		xrTableCell450 = new XRTableCell();
		xrTableCell451 = new XRTableCell();
		xrTableCell452 = new XRTableCell();
		xrTable31 = new XRTable();
		xrTableRow98 = new XRTableRow();
		xrTableCell453 = new XRTableCell();
		xrTableCell454 = new XRTableCell();
		xrTableCell455 = new XRTableCell();
		xrTableCell456 = new XRTableCell();
		xrTableCell457 = new XRTableCell();
		xrTableRow99 = new XRTableRow();
		xrTableCell458 = new XRTableCell();
		xrTableCell459 = new XRTableCell();
		xrTableCell460 = new XRTableCell();
		xrTableCell461 = new XRTableCell();
		xrTableCell462 = new XRTableCell();
		xrTableRow100 = new XRTableRow();
		xrTableCell463 = new XRTableCell();
		xrTableCell464 = new XRTableCell();
		xrTableCell465 = new XRTableCell();
		xrTableCell466 = new XRTableCell();
		xrTableCell467 = new XRTableCell();
		xrTableRow101 = new XRTableRow();
		xrTableCell468 = new XRTableCell();
		xrTableCell469 = new XRTableCell();
		xrTableCell470 = new XRTableCell();
		xrTableCell471 = new XRTableCell();
		xrTableCell472 = new XRTableCell();
		xrTableRow102 = new XRTableRow();
		xrTableCell473 = new XRTableCell();
		xrTableCell474 = new XRTableCell();
		xrTableCell475 = new XRTableCell();
		xrTableCell476 = new XRTableCell();
		xrTableCell477 = new XRTableCell();
		xrTableRow103 = new XRTableRow();
		xrTableCell478 = new XRTableCell();
		xrTableCell479 = new XRTableCell();
		xrTableCell480 = new XRTableCell();
		xrTableCell481 = new XRTableCell();
		xrTableCell482 = new XRTableCell();
		xrTableRow104 = new XRTableRow();
		xrTableCell483 = new XRTableCell();
		xrTableCell484 = new XRTableCell();
		xrTableCell485 = new XRTableCell();
		xrTableCell486 = new XRTableCell();
		xrTableCell487 = new XRTableCell();
		xrTableRow105 = new XRTableRow();
		xrTableCell488 = new XRTableCell();
		xrTableCell489 = new XRTableCell();
		xrTableCell490 = new XRTableCell();
		xrTableCell491 = new XRTableCell();
		xrTableCell492 = new XRTableCell();
		xrTableRow106 = new XRTableRow();
		xrTableCell493 = new XRTableCell();
		xrTableCell494 = new XRTableCell();
		xrTableCell495 = new XRTableCell();
		xrTableCell496 = new XRTableCell();
		xrTableCell497 = new XRTableCell();
		xrLabel12 = new XRLabel();
		TopMargin = new TopMarginBand();
		BottomMargin = new BottomMarginBand();
		xrPageInfo1 = new XRPageInfo();
		formattingRule1 = new FormattingRule();
		xrControlStyle1 = new XRControlStyle();
		xrControlStyle2 = new XRControlStyle();
		Bolgeler = new DetailReportBand();
		Bolge_Genel_Ozet = new DetailBand();
		xrTable28 = new XRTable();
		xrTableRow69 = new XRTableRow();
		xrTableCell236 = new XRTableCell();
		xrTableCell237 = new XRTableCell();
		xrTableCell365 = new XRTableCell();
		xrTableCell238 = new XRTableCell();
		xrTableCell239 = new XRTableCell();
		xrTableRow70 = new XRTableRow();
		xrTableCell240 = new XRTableCell();
		xrTableCell241 = new XRTableCell();
		xrTableCell366 = new XRTableCell();
		xrTableCell242 = new XRTableCell();
		xrTableCell243 = new XRTableCell();
		xrTableRow71 = new XRTableRow();
		xrTableCell244 = new XRTableCell();
		xrTableCell254 = new XRTableCell();
		xrTableCell368 = new XRTableCell();
		xrTableCell255 = new XRTableCell();
		xrTableCell256 = new XRTableCell();
		xrTableRow72 = new XRTableRow();
		xrTableCell257 = new XRTableCell();
		xrTableCell258 = new XRTableCell();
		xrTableCell369 = new XRTableCell();
		xrTableCell259 = new XRTableCell();
		xrTableCell260 = new XRTableCell();
		xrTableRow73 = new XRTableRow();
		xrTableCell261 = new XRTableCell();
		xrTableCell262 = new XRTableCell();
		xrTableCell370 = new XRTableCell();
		xrTableCell263 = new XRTableCell();
		xrTableCell264 = new XRTableCell();
		xrTableRow74 = new XRTableRow();
		xrTableCell265 = new XRTableCell();
		xrTableCell266 = new XRTableCell();
		xrTableCell372 = new XRTableCell();
		xrTableCell267 = new XRTableCell();
		xrTableCell268 = new XRTableCell();
		xrTableRow75 = new XRTableRow();
		xrTableCell269 = new XRTableCell();
		xrTableCell270 = new XRTableCell();
		xrTableCell373 = new XRTableCell();
		xrTableCell271 = new XRTableCell();
		xrTableCell272 = new XRTableCell();
		xrTableRow76 = new XRTableRow();
		xrTableCell273 = new XRTableCell();
		xrTableCell274 = new XRTableCell();
		xrTableCell374 = new XRTableCell();
		xrTableCell275 = new XRTableCell();
		xrTableCell276 = new XRTableCell();
		xrTableRow77 = new XRTableRow();
		xrTableCell277 = new XRTableCell();
		xrTableCell278 = new XRTableCell();
		xrTableCell375 = new XRTableCell();
		xrTableCell279 = new XRTableCell();
		xrTableCell280 = new XRTableCell();
		xrTable29 = new XRTable();
		xrTableRow78 = new XRTableRow();
		xrTableCell281 = new XRTableCell();
		xrTableCell282 = new XRTableCell();
		xrTableCell348 = new XRTableCell();
		xrTableCell283 = new XRTableCell();
		xrTableCell284 = new XRTableCell();
		xrTableRow79 = new XRTableRow();
		xrTableCell285 = new XRTableCell();
		xrTableCell286 = new XRTableCell();
		xrTableCell363 = new XRTableCell();
		xrTableCell349 = new XRTableCell();
		xrTableCell287 = new XRTableCell();
		xrTableCell288 = new XRTableCell();
		xrTableCell289 = new XRTableCell();
		xrTableCell290 = new XRTableCell();
		xrTableRow80 = new XRTableRow();
		xrTableCell291 = new XRTableCell();
		xrTableCell292 = new XRTableCell();
		xrTableCell356 = new XRTableCell();
		xrTableCell350 = new XRTableCell();
		xrTableCell293 = new XRTableCell();
		xrTableCell294 = new XRTableCell();
		xrTableCell295 = new XRTableCell();
		xrTableCell296 = new XRTableCell();
		xrTableRow81 = new XRTableRow();
		xrTableCell297 = new XRTableCell();
		xrTableCell298 = new XRTableCell();
		xrTableCell357 = new XRTableCell();
		xrTableCell351 = new XRTableCell();
		xrTableCell299 = new XRTableCell();
		xrTableCell300 = new XRTableCell();
		xrTableCell301 = new XRTableCell();
		xrTableCell302 = new XRTableCell();
		xrTableRow82 = new XRTableRow();
		xrTableCell303 = new XRTableCell();
		xrTableCell304 = new XRTableCell();
		xrTableCell364 = new XRTableCell();
		xrTableCell355 = new XRTableCell();
		xrTableCell305 = new XRTableCell();
		xrTableCell306 = new XRTableCell();
		xrTableCell307 = new XRTableCell();
		xrTableCell308 = new XRTableCell();
		xrTableRow83 = new XRTableRow();
		xrTableCell309 = new XRTableCell();
		xrTableCell310 = new XRTableCell();
		xrTableCell367 = new XRTableCell();
		xrTableCell358 = new XRTableCell();
		xrTableCell311 = new XRTableCell();
		xrTableCell321 = new XRTableCell();
		xrTableCell322 = new XRTableCell();
		xrTableCell323 = new XRTableCell();
		xrTableRow84 = new XRTableRow();
		xrTableCell324 = new XRTableCell();
		xrTableCell325 = new XRTableCell();
		xrTableCell371 = new XRTableCell();
		xrTableCell362 = new XRTableCell();
		xrTableCell326 = new XRTableCell();
		xrTableCell327 = new XRTableCell();
		xrTableCell328 = new XRTableCell();
		xrTableCell329 = new XRTableCell();
		xrTableRow85 = new XRTableRow();
		xrTableCell330 = new XRTableCell();
		xrTableCell331 = new XRTableCell();
		xrTableCell359 = new XRTableCell();
		xrTableCell352 = new XRTableCell();
		xrTableCell332 = new XRTableCell();
		xrTableCell333 = new XRTableCell();
		xrTableCell334 = new XRTableCell();
		xrTableCell335 = new XRTableCell();
		xrTableRow86 = new XRTableRow();
		xrTableCell336 = new XRTableCell();
		xrTableCell337 = new XRTableCell();
		xrTableCell360 = new XRTableCell();
		xrTableCell353 = new XRTableCell();
		xrTableCell338 = new XRTableCell();
		xrTableCell339 = new XRTableCell();
		xrTableCell340 = new XRTableCell();
		xrTableCell341 = new XRTableCell();
		xrTableRow87 = new XRTableRow();
		xrTableCell342 = new XRTableCell();
		xrTableCell343 = new XRTableCell();
		xrTableCell361 = new XRTableCell();
		xrTableCell354 = new XRTableCell();
		xrTableCell344 = new XRTableCell();
		xrTableCell345 = new XRTableCell();
		xrTableCell346 = new XRTableCell();
		xrTableCell347 = new XRTableCell();
		xrLabel18 = new XRLabel();
		xrLabel23 = new XRLabel();
		xrLabel24 = new XRLabel();
		xrLabel25 = new XRLabel();
		xrLabel26 = new XRLabel();
		xrLabel11 = new XRLabel();
		Temsilciler = new DetailReportBand();
		Temsilci_Ozet = new DetailBand();
		xrLabel16 = new XRLabel();
		xrLabel17 = new XRLabel();
		xrLabel21 = new XRLabel();
		xrLabel22 = new XRLabel();
		xrLabel19 = new XRLabel();
		xrLabel20 = new XRLabel();
		xrTable27 = new XRTable();
		xrTableRow47 = new XRTableRow();
		xrTableCell182 = new XRTableCell();
		xrTableCell183 = new XRTableCell();
		xrTableCell184 = new XRTableCell();
		xrTableCell185 = new XRTableCell();
		xrTableRow68 = new XRTableRow();
		xrTableCell232 = new XRTableCell();
		xrTableCell233 = new XRTableCell();
		xrTableCell246 = new XRTableCell();
		xrTableCell234 = new XRTableCell();
		xrTableCell313 = new XRTableCell();
		xrTableCell235 = new XRTableCell();
		xrTableRow58 = new XRTableRow();
		xrTableCell186 = new XRTableCell();
		xrTableCell187 = new XRTableCell();
		xrTableCell245 = new XRTableCell();
		xrTableCell188 = new XRTableCell();
		xrTableCell315 = new XRTableCell();
		xrTableCell189 = new XRTableCell();
		xrTableRow59 = new XRTableRow();
		xrTableCell190 = new XRTableCell();
		xrTableCell191 = new XRTableCell();
		xrTableCell247 = new XRTableCell();
		xrTableCell192 = new XRTableCell();
		xrTableCell316 = new XRTableCell();
		xrTableCell193 = new XRTableCell();
		xrTableRow60 = new XRTableRow();
		xrTableCell194 = new XRTableCell();
		xrTableCell195 = new XRTableCell();
		xrTableCell248 = new XRTableCell();
		xrTableCell196 = new XRTableCell();
		xrTableCell312 = new XRTableCell();
		xrTableCell197 = new XRTableCell();
		xrTableRow61 = new XRTableRow();
		xrTableCell198 = new XRTableCell();
		xrTableCell199 = new XRTableCell();
		xrTableCell249 = new XRTableCell();
		xrTableCell200 = new XRTableCell();
		xrTableCell317 = new XRTableCell();
		xrTableCell201 = new XRTableCell();
		xrTableRow62 = new XRTableRow();
		xrTableCell202 = new XRTableCell();
		xrTableCell203 = new XRTableCell();
		xrTableCell250 = new XRTableCell();
		xrTableCell218 = new XRTableCell();
		xrTableCell314 = new XRTableCell();
		xrTableCell219 = new XRTableCell();
		xrTableRow63 = new XRTableRow();
		xrTableCell220 = new XRTableCell();
		xrTableCell221 = new XRTableCell();
		xrTableCell251 = new XRTableCell();
		xrTableCell222 = new XRTableCell();
		xrTableCell318 = new XRTableCell();
		xrTableCell223 = new XRTableCell();
		xrTableRow64 = new XRTableRow();
		xrTableCell224 = new XRTableCell();
		xrTableCell225 = new XRTableCell();
		xrTableCell252 = new XRTableCell();
		xrTableCell226 = new XRTableCell();
		xrTableCell319 = new XRTableCell();
		xrTableCell227 = new XRTableCell();
		xrTableRow65 = new XRTableRow();
		xrTableCell228 = new XRTableCell();
		xrTableCell229 = new XRTableCell();
		xrTableCell253 = new XRTableCell();
		xrTableCell230 = new XRTableCell();
		xrTableCell320 = new XRTableCell();
		xrTableCell231 = new XRTableCell();
		xrTable26 = new XRTable();
		xrTableRow51 = new XRTableRow();
		xrTableCell161 = new XRTableCell();
		xrTableCell162 = new XRTableCell();
		xrTableCell163 = new XRTableCell();
		xrTableCell160 = new XRTableCell();
		xrTableRow52 = new XRTableRow();
		xrTableCell164 = new XRTableCell();
		xrTableCell165 = new XRTableCell();
		xrTableCell166 = new XRTableCell();
		xrTableCell210 = new XRTableCell();
		xrTableRow53 = new XRTableRow();
		xrTableCell167 = new XRTableCell();
		xrTableCell168 = new XRTableCell();
		xrTableCell169 = new XRTableCell();
		xrTableCell211 = new XRTableCell();
		xrTableRow54 = new XRTableRow();
		xrTableCell170 = new XRTableCell();
		xrTableCell171 = new XRTableCell();
		xrTableCell172 = new XRTableCell();
		xrTableCell212 = new XRTableCell();
		xrTableRow55 = new XRTableRow();
		xrTableCell173 = new XRTableCell();
		xrTableCell174 = new XRTableCell();
		xrTableCell175 = new XRTableCell();
		xrTableCell213 = new XRTableCell();
		xrTableRow56 = new XRTableRow();
		xrTableCell176 = new XRTableCell();
		xrTableCell177 = new XRTableCell();
		xrTableCell178 = new XRTableCell();
		xrTableCell214 = new XRTableCell();
		xrTableRow57 = new XRTableRow();
		xrTableCell179 = new XRTableCell();
		xrTableCell180 = new XRTableCell();
		xrTableCell181 = new XRTableCell();
		xrTableCell215 = new XRTableCell();
		xrTableRow66 = new XRTableRow();
		xrTableCell204 = new XRTableCell();
		xrTableCell205 = new XRTableCell();
		xrTableCell206 = new XRTableCell();
		xrTableCell216 = new XRTableCell();
		xrTableRow67 = new XRTableRow();
		xrTableCell207 = new XRTableCell();
		xrTableCell208 = new XRTableCell();
		xrTableCell209 = new XRTableCell();
		xrTableCell217 = new XRTableCell();
		xrLabel13 = new XRLabel();
		xrLabel14 = new XRLabel();
		xrLabel15 = new XRLabel();
		xrLabel10 = new XRLabel();
		Temsilci_Gunluk_Hareket = new DetailReportBand();
		Gun_Ozeti = new DetailBand();
		xrLabel2 = new XRLabel();
		xrTable22 = new XRTable();
		xrTableRow24 = new XRTableRow();
		xrTableCell33 = new XRTableCell();
		xrTableRow40 = new XRTableRow();
		xrTableCell124 = new XRTableCell();
		xrTableCell136 = new XRTableCell();
		xrTableCell141 = new XRTableCell();
		xrTableRow43 = new XRTableRow();
		xrTableCell142 = new XRTableCell();
		xrTableCell143 = new XRTableCell();
		xrTableCell144 = new XRTableCell();
		xrTableRow44 = new XRTableRow();
		xrTableCell146 = new XRTableCell();
		xrTableCell149 = new XRTableCell();
		xrTableCell152 = new XRTableCell();
		xrTable23 = new XRTable();
		xrTableRow45 = new XRTableRow();
		xrTableCell138 = new XRTableCell();
		xrTableRow48 = new XRTableRow();
		xrTableCell145 = new XRTableCell();
		xrTableCell147 = new XRTableCell();
		xrTableRow49 = new XRTableRow();
		xrTableCell148 = new XRTableCell();
		xrTableCell150 = new XRTableCell();
		xrTableRow50 = new XRTableRow();
		xrTableCell151 = new XRTableCell();
		xrTableCell153 = new XRTableCell();
		xrTable21 = new XRTable();
		xrTableRow39 = new XRTableRow();
		xrTableCell26 = new XRTableCell();
		xrTableRow6 = new XRTableRow();
		xrTableCell16 = new XRTableCell();
		xrTableCell92 = new XRTableCell();
		xrTableCell93 = new XRTableCell();
		xrTableRow41 = new XRTableRow();
		xrTableCell122 = new XRTableCell();
		xrTableCell125 = new XRTableCell();
		xrTableCell18 = new XRTableCell();
		xrTableRow42 = new XRTableRow();
		xrTableCell135 = new XRTableCell();
		xrTableCell137 = new XRTableCell();
		xrTableCell31 = new XRTableCell();
		xrTableRow46 = new XRTableRow();
		xrTableCell139 = new XRTableCell();
		xrTableCell140 = new XRTableCell();
		xrTableCell32 = new XRTableCell();
		xrTable20 = new XRTable();
		xrTableRow7 = new XRTableRow();
		xrTableCell19 = new XRTableCell();
		xrTableRow8 = new XRTableRow();
		xrTableCell22 = new XRTableCell();
		xrTableCell24 = new XRTableCell();
		xrTableRow9 = new XRTableRow();
		xrTableCell25 = new XRTableCell();
		xrTableCell27 = new XRTableCell();
		xrTableRow34 = new XRTableRow();
		xrTableCell121 = new XRTableCell();
		xrTableCell123 = new XRTableCell();
		xrTable17 = new XRTable();
		xrTableRow33 = new XRTableRow();
		xrTableCell120 = new XRTableCell();
		xrTableRow32 = new XRTableRow();
		xrTableCell97 = new XRTableCell();
		xrTableCell100 = new XRTableCell();
		xrTableCell104 = new XRTableCell();
		xrTableRow35 = new XRTableRow();
		xrTableCell126 = new XRTableCell();
		xrTableCell127 = new XRTableCell();
		xrTableCell128 = new XRTableCell();
		xrTableRow36 = new XRTableRow();
		xrTableCell129 = new XRTableCell();
		xrTableCell130 = new XRTableCell();
		xrTableCell131 = new XRTableCell();
		xrTableRow37 = new XRTableRow();
		xrTableCell132 = new XRTableCell();
		xrTableCell133 = new XRTableCell();
		xrTableCell134 = new XRTableCell();
		xrTable16 = new XRTable();
		xrTableRow31 = new XRTableRow();
		xrTableCell94 = new XRTableCell();
		xrTableRow25 = new XRTableRow();
		xrTableCell95 = new XRTableCell();
		xrTableCell96 = new XRTableCell();
		xrTableRow28 = new XRTableRow();
		xrTableCell98 = new XRTableCell();
		xrTableCell99 = new XRTableCell();
		xrTableRow29 = new XRTableRow();
		xrTableCell101 = new XRTableCell();
		xrTableCell102 = new XRTableCell();
		xrTableRow30 = new XRTableRow();
		xrTableCell105 = new XRTableCell();
		xrTableCell106 = new XRTableCell();
		xrLabel35 = new XRLabel();
		xrLabel37 = new XRLabel();
		xrLabel39 = new XRLabel();
		xrLabel36 = new XRLabel();
		xrTable3 = new XRTable();
		xrTableRow38 = new XRTableRow();
		xrTableCell23 = new XRTableCell();
		xrTableRow3 = new XRTableRow();
		xrTableCell7 = new XRTableCell();
		xrTableCell8 = new XRTableCell();
		xrTableCell9 = new XRTableCell();
		xrTableRow10 = new XRTableRow();
		xrTableCell28 = new XRTableCell();
		xrTableCell29 = new XRTableCell();
		xrTableCell30 = new XRTableCell();
		xrTableRow11 = new XRTableRow();
		xrTableCell17 = new XRTableCell();
		xrTableCell20 = new XRTableCell();
		xrTableCell21 = new XRTableCell();
		xrLabel1 = new XRLabel();
		xrLabel38 = new XRLabel();
		xrLabel7 = new XRLabel();
		Ziyaret_Listesi = new DetailReportBand();
		Ziyaret_Listesi_Liste = new DetailBand();
		xrTable25 = new XRTable();
		xrTableRow5 = new XRTableRow();
		xrTableCell159 = new XRTableCell();
		xrTableCell158 = new XRTableCell();
		xrTableCell157 = new XRTableCell();
		xrTableCell13 = new XRTableCell();
		xrTableCell14 = new XRTableCell();
		xrTableCell15 = new XRTableCell();
		GroupHeader1 = new GroupHeaderBand();
		xrTable24 = new XRTable();
		xrTableRow4 = new XRTableRow();
		xrTableCell156 = new XRTableCell();
		xrTableCell155 = new XRTableCell();
		xrTableCell154 = new XRTableCell();
		xrTableCell10 = new XRTableCell();
		xrTableCell11 = new XRTableCell();
		xrTableCell12 = new XRTableCell();
		xrLabel9 = new XRLabel();
		bindingSource1 = new BindingSource(components);
		Hedef_Ziyaretler = new DetailReportBand();
		Hedef_Ziyaretler_Liste = new DetailBand();
		xrTable19 = new XRTable();
		xrTableRow27 = new XRTableRow();
		xrTableCell109 = new XRTableCell();
		xrTableCell110 = new XRTableCell();
		xrTableCell111 = new XRTableCell();
		Hedef_Ziyaretler_Baslik = new GroupHeaderBand();
		xrLabel45 = new XRLabel();
		xrTable18 = new XRTable();
		xrTableRow26 = new XRTableRow();
		xrTableCell103 = new XRTableCell();
		xrTableCell107 = new XRTableCell();
		xrTableCell108 = new XRTableCell();
		Yapilmayan_Ziyaretler = new DetailReportBand();
		Yapilmayan_Ziyaret_Listesi = new DetailBand();
		xrTable5 = new XRTable();
		xrTableRow13 = new XRTableRow();
		xrTableCell41 = new XRTableCell();
		xrTableCell42 = new XRTableCell();
		xrTableCell43 = new XRTableCell();
		Yapilmayan_Ziyaretler_Baslik = new GroupHeaderBand();
		xrTable4 = new XRTable();
		xrTableRow12 = new XRTableRow();
		xrTableCell35 = new XRTableCell();
		xrTableCell39 = new XRTableCell();
		xrTableCell40 = new XRTableCell();
		xrLabel4 = new XRLabel();
		Yapilan_Ziyaretler = new DetailReportBand();
		Yapilan_Ziyaret_Listesi = new DetailBand();
		xrTable7 = new XRTable();
		xrTableRow15 = new XRTableRow();
		xrTableCell49 = new XRTableCell();
		xrTableCell112 = new XRTableCell();
		xrTableCell113 = new XRTableCell();
		xrTableCell114 = new XRTableCell();
		xrTableCell115 = new XRTableCell();
		Yapilan_Ziyaretler_Baslik = new GroupHeaderBand();
		xrTable6 = new XRTable();
		xrTableRow14 = new XRTableRow();
		xrTableCell44 = new XRTableCell();
		xrTableCell45 = new XRTableCell();
		xrTableCell46 = new XRTableCell();
		xrTableCell47 = new XRTableCell();
		xrTableCell48 = new XRTableCell();
		xrLabel5 = new XRLabel();
		Rota_Disi_Ziyaretler = new DetailReportBand();
		Rota_Disi_Ziyaret_Listesi = new DetailBand();
		xrTable2 = new XRTable();
		xrTableRow2 = new XRTableRow();
		xrTableCell6 = new XRTableCell();
		xrTableCell34 = new XRTableCell();
		xrTableCell36 = new XRTableCell();
		xrTableCell37 = new XRTableCell();
		xrTableCell38 = new XRTableCell();
		Rota_Disi_Ziyaretler_Baslik = new GroupHeaderBand();
		xrLabel3 = new XRLabel();
		xrTable1 = new XRTable();
		xrTableRow1 = new XRTableRow();
		xrTableCell1 = new XRTableCell();
		xrTableCell2 = new XRTableCell();
		xrTableCell3 = new XRTableCell();
		xrTableCell4 = new XRTableCell();
		xrTableCell5 = new XRTableCell();
		Siparisler = new DetailReportBand();
		Siparis_Listesi = new DetailBand();
		xrTable9 = new XRTable();
		xrTableRow17 = new XRTableRow();
		xrTableCell55 = new XRTableCell();
		xrTableCell56 = new XRTableCell();
		xrTableCell57 = new XRTableCell();
		xrTableCell58 = new XRTableCell();
		xrTableCell59 = new XRTableCell();
		Siparisler_Baslik = new GroupHeaderBand();
		xrTable8 = new XRTable();
		xrTableRow16 = new XRTableRow();
		xrTableCell50 = new XRTableCell();
		xrTableCell51 = new XRTableCell();
		xrTableCell52 = new XRTableCell();
		xrTableCell53 = new XRTableCell();
		xrTableCell54 = new XRTableCell();
		xrLabel6 = new XRLabel();
		Faturalar = new DetailReportBand();
		Fatura_Listesi = new DetailBand();
		xrTable11 = new XRTable();
		xrTableRow19 = new XRTableRow();
		xrTableCell65 = new XRTableCell();
		xrTableCell66 = new XRTableCell();
		xrTableCell67 = new XRTableCell();
		xrTableCell68 = new XRTableCell();
		xrTableCell69 = new XRTableCell();
		Faturalar_Baslik = new GroupHeaderBand();
		xrLabel8 = new XRLabel();
		xrTable10 = new XRTable();
		xrTableRow18 = new XRTableRow();
		xrTableCell60 = new XRTableCell();
		xrTableCell61 = new XRTableCell();
		xrTableCell62 = new XRTableCell();
		xrTableCell63 = new XRTableCell();
		xrTableCell64 = new XRTableCell();
		Tahsilatlar = new DetailReportBand();
		Tahsilat_Listesi = new DetailBand();
		xrTable13 = new XRTable();
		xrTableRow21 = new XRTableRow();
		xrTableCell77 = new XRTableCell();
		xrTableCell78 = new XRTableCell();
		xrTableCell79 = new XRTableCell();
		xrTableCell116 = new XRTableCell();
		xrTableCell117 = new XRTableCell();
		xrTableCell118 = new XRTableCell();
		xrTableCell119 = new XRTableCell();
		Tahsilatlar_baslik = new GroupHeaderBand();
		xrTable12 = new XRTable();
		xrTableRow20 = new XRTableRow();
		xrTableCell70 = new XRTableCell();
		xrTableCell71 = new XRTableCell();
		xrTableCell72 = new XRTableCell();
		xrTableCell73 = new XRTableCell();
		xrTableCell74 = new XRTableCell();
		xrTableCell75 = new XRTableCell();
		xrTableCell76 = new XRTableCell();
		xrLabel33 = new XRLabel();
		Masraflar = new DetailReportBand();
		Masraf_Listesi = new DetailBand();
		xrTable15 = new XRTable();
		xrTableRow23 = new XRTableRow();
		xrTableCell86 = new XRTableCell();
		xrTableCell87 = new XRTableCell();
		xrTableCell88 = new XRTableCell();
		xrTableCell89 = new XRTableCell();
		xrTableCell90 = new XRTableCell();
		xrTableCell91 = new XRTableCell();
		Masraflar_Baslik = new GroupHeaderBand();
		xrLabel34 = new XRLabel();
		xrTable14 = new XRTable();
		xrTableRow22 = new XRTableRow();
		xrTableCell80 = new XRTableCell();
		xrTableCell81 = new XRTableCell();
		xrTableCell82 = new XRTableCell();
		xrTableCell83 = new XRTableCell();
		xrTableCell84 = new XRTableCell();
		xrTableCell85 = new XRTableCell();
		((ISupportInitialize)xrTable30).BeginInit();
		((ISupportInitialize)xrTable31).BeginInit();
		((ISupportInitialize)xrTable28).BeginInit();
		((ISupportInitialize)xrTable29).BeginInit();
		((ISupportInitialize)xrTable27).BeginInit();
		((ISupportInitialize)xrTable26).BeginInit();
		((ISupportInitialize)xrTable22).BeginInit();
		((ISupportInitialize)xrTable23).BeginInit();
		((ISupportInitialize)xrTable21).BeginInit();
		((ISupportInitialize)xrTable20).BeginInit();
		((ISupportInitialize)xrTable17).BeginInit();
		((ISupportInitialize)xrTable16).BeginInit();
		((ISupportInitialize)xrTable3).BeginInit();
		((ISupportInitialize)xrTable25).BeginInit();
		((ISupportInitialize)xrTable24).BeginInit();
		((ISupportInitialize)bindingSource1).BeginInit();
		((ISupportInitialize)xrTable19).BeginInit();
		((ISupportInitialize)xrTable18).BeginInit();
		((ISupportInitialize)xrTable5).BeginInit();
		((ISupportInitialize)xrTable4).BeginInit();
		((ISupportInitialize)xrTable7).BeginInit();
		((ISupportInitialize)xrTable6).BeginInit();
		((ISupportInitialize)xrTable2).BeginInit();
		((ISupportInitialize)xrTable1).BeginInit();
		((ISupportInitialize)xrTable9).BeginInit();
		((ISupportInitialize)xrTable8).BeginInit();
		((ISupportInitialize)xrTable11).BeginInit();
		((ISupportInitialize)xrTable10).BeginInit();
		((ISupportInitialize)xrTable13).BeginInit();
		((ISupportInitialize)xrTable12).BeginInit();
		((ISupportInitialize)xrTable15).BeginInit();
		((ISupportInitialize)xrTable14).BeginInit();
		((ISupportInitialize)this).BeginInit();
		Genel_Ozet.Controls.AddRange(new XRControl[7] { xrLabel29, xrLabel30, xrLabel27, xrLabel28, xrTable30, xrTable31, xrLabel12 });
		Genel_Ozet.Expanded = false;
		Genel_Ozet.HeightF = 517.421f;
		Genel_Ozet.Name = "Genel_Ozet";
		Genel_Ozet.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		Genel_Ozet.PageBreak = PageBreak.AfterBand;
		Genel_Ozet.TextAlignment = TextAlignment.TopLeft;
		xrLabel29.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolge_sayisi", "{0:n0}")
		});
		xrLabel29.Font = new Font("Arial", 9.75f);
		xrLabel29.LocationFloat = new PointFloat(154.0126f, 39.37494f);
		xrLabel29.Name = "xrLabel29";
		xrLabel29.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel29.SizeF = new SizeF(240.9986f, 18.00002f);
		xrLabel29.StylePriority.UseFont = false;
		xrLabel29.StylePriority.UseTextAlignment = false;
		xrLabel29.Text = "xrLabel4";
		xrLabel29.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel30.Font = new Font("Arial", 9.75f, FontStyle.Bold);
		xrLabel30.LocationFloat = new PointFloat(9.999943f, 39.375f);
		xrLabel30.Name = "xrLabel30";
		xrLabel30.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel30.SizeF = new SizeF(144.0127f, 18f);
		xrLabel30.StylePriority.UseFont = false;
		xrLabel30.StylePriority.UseTextAlignment = false;
		xrLabel30.Text = "Bölge sayısı :";
		xrLabel30.TextAlignment = TextAlignment.MiddleRight;
		xrLabel27.Font = new Font("Arial", 9.75f, FontStyle.Bold);
		xrLabel27.LocationFloat = new PointFloat(10.00001f, 57.37502f);
		xrLabel27.Name = "xrLabel27";
		xrLabel27.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel27.SizeF = new SizeF(144.0127f, 18f);
		xrLabel27.StylePriority.UseFont = false;
		xrLabel27.StylePriority.UseTextAlignment = false;
		xrLabel27.Text = "Temsilci sayısı :";
		xrLabel27.TextAlignment = TextAlignment.MiddleRight;
		xrLabel28.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_sayisi", "{0:n0}")
		});
		xrLabel28.Font = new Font("Arial", 9.75f);
		xrLabel28.LocationFloat = new PointFloat(154.0126f, 57.37495f);
		xrLabel28.Name = "xrLabel28";
		xrLabel28.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel28.SizeF = new SizeF(240.9986f, 18.00002f);
		xrLabel28.StylePriority.UseFont = false;
		xrLabel28.StylePriority.UseTextAlignment = false;
		xrLabel28.Text = "xrLabel4";
		xrLabel28.TextAlignment = TextAlignment.MiddleLeft;
		xrTable30.Borders = BorderSide.All;
		xrTable30.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable30.LocationFloat = new PointFloat(9.999911f, 296.4214f);
		xrTable30.Name = "xrTable30";
		xrTable30.Rows.AddRange(new XRTableRow[10] { xrTableRow88, xrTableRow89, xrTableRow90, xrTableRow91, xrTableRow92, xrTableRow93, xrTableRow94, xrTableRow95, xrTableRow96, xrTableRow97 });
		xrTable30.SizeF = new SizeF(774.9374f, 206.0183f);
		xrTable30.StylePriority.UseBorders = false;
		xrTable30.StylePriority.UseFont = false;
		xrTable30.StylePriority.UseTextAlignment = false;
		xrTable30.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow88.Cells.AddRange(new XRTableCell[5] { xrTableCell376, xrTableCell377, xrTableCell378, xrTableCell379, xrTableCell380 });
		xrTableRow88.Name = "xrTableRow88";
		xrTableRow88.Weight = 1.0;
		xrTableCell376.Borders = BorderSide.Right;
		xrTableCell376.Name = "xrTableCell376";
		xrTableCell376.StylePriority.UseBorders = false;
		xrTableCell376.Weight = 0.3539978303423901;
		xrTableCell377.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell377.Name = "xrTableCell377";
		xrTableCell377.StylePriority.UseFont = false;
		xrTableCell377.StylePriority.UseTextAlignment = false;
		xrTableCell377.Text = "Ziyaret başı";
		xrTableCell377.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell377.Weight = 0.3116740938483716;
		xrTableCell378.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell378.Name = "xrTableCell378";
		xrTableCell378.StylePriority.UseFont = false;
		xrTableCell378.StylePriority.UseTextAlignment = false;
		xrTableCell378.Text = "Temsilci başı";
		xrTableCell378.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell378.Weight = 0.5100121289111983;
		xrTableCell379.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell379.Name = "xrTableCell379";
		xrTableCell379.StylePriority.UseFont = false;
		xrTableCell379.StylePriority.UseTextAlignment = false;
		xrTableCell379.Text = "Günlük ortalama";
		xrTableCell379.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell379.Weight = 0.510011999208395;
		xrTableCell380.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell380.Name = "xrTableCell380";
		xrTableCell380.StylePriority.UseFont = false;
		xrTableCell380.StylePriority.UseTextAlignment = false;
		xrTableCell380.Text = "Toplam";
		xrTableCell380.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell380.Weight = 0.5100122663447048;
		xrTableRow89.Cells.AddRange(new XRTableCell[8] { xrTableCell381, xrTableCell382, xrTableCell383, xrTableCell384, xrTableCell385, xrTableCell386, xrTableCell387, xrTableCell388 });
		xrTableRow89.Name = "xrTableRow89";
		xrTableRow89.Weight = 1.0;
		xrTableCell381.Borders = BorderSide.Right | BorderSide.Bottom;
		xrTableCell381.Name = "xrTableCell381";
		xrTableCell381.StylePriority.UseBorders = false;
		xrTableCell381.Weight = 0.35399754869263084;
		xrTableCell382.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell382.Name = "xrTableCell382";
		xrTableCell382.StylePriority.UseFont = false;
		xrTableCell382.StylePriority.UseTextAlignment = false;
		xrTableCell382.Text = "Tutar";
		xrTableCell382.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell382.Weight = 0.31167412514895654;
		xrTableCell383.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell383.Name = "xrTableCell383";
		xrTableCell383.StylePriority.UseFont = false;
		xrTableCell383.StylePriority.UseTextAlignment = false;
		xrTableCell383.Text = "Adet";
		xrTableCell383.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell383.Weight = 0.19833807135463175;
		xrTableCell384.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell384.Name = "xrTableCell384";
		xrTableCell384.StylePriority.UseFont = false;
		xrTableCell384.StylePriority.UseTextAlignment = false;
		xrTableCell384.Text = "Tutar";
		xrTableCell384.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell384.Weight = 0.3116741101721726;
		xrTableCell385.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell385.Name = "xrTableCell385";
		xrTableCell385.StylePriority.UseFont = false;
		xrTableCell385.StylePriority.UseTextAlignment = false;
		xrTableCell385.Text = "Adet";
		xrTableCell385.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell385.Weight = 0.19833806396999998;
		xrTableCell386.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell386.Name = "xrTableCell386";
		xrTableCell386.StylePriority.UseFont = false;
		xrTableCell386.StylePriority.UseTextAlignment = false;
		xrTableCell386.Text = "Tutar";
		xrTableCell386.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell386.Weight = 0.3116741098610186;
		xrTableCell387.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell387.Name = "xrTableCell387";
		xrTableCell387.StylePriority.UseFont = false;
		xrTableCell387.StylePriority.UseTextAlignment = false;
		xrTableCell387.Text = "Adet";
		xrTableCell387.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell387.Weight = 0.19833807328858916;
		xrTableCell388.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell388.Name = "xrTableCell388";
		xrTableCell388.StylePriority.UseFont = false;
		xrTableCell388.StylePriority.UseTextAlignment = false;
		xrTableCell388.Text = "Tutar";
		xrTableCell388.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell388.Weight = 0.31167421616706065;
		xrTableRow90.Cells.AddRange(new XRTableCell[8] { xrTableCell389, xrTableCell390, xrTableCell391, xrTableCell392, xrTableCell393, xrTableCell394, xrTableCell395, xrTableCell396 });
		xrTableRow90.Name = "xrTableRow90";
		xrTableRow90.Weight = 1.0;
		xrTableCell389.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell389.Name = "xrTableCell389";
		xrTableCell389.StylePriority.UseFont = false;
		xrTableCell389.StylePriority.UseTextAlignment = false;
		xrTableCell389.Text = "Sipariş";
		xrTableCell389.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell389.Weight = 0.35399776724447063;
		xrTableCell390.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "ziyaret_basi_ortalama_siparis_tutari", "{0:c}")
		});
		xrTableCell390.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell390.Name = "xrTableCell390";
		xrTableCell390.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell390.StylePriority.UseFont = false;
		xrTableCell390.StylePriority.UsePadding = false;
		xrTableCell390.StylePriority.UseTextAlignment = false;
		xrTableCell390.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell390.Weight = 0.31167411069954143;
		xrTableCell391.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_siparis_adedi", "{0:n}")
		});
		xrTableCell391.Name = "xrTableCell391";
		xrTableCell391.StylePriority.UseTextAlignment = false;
		xrTableCell391.Text = "xrTableCell356";
		xrTableCell391.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell391.Weight = 0.1983380714848679;
		xrTableCell392.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_siparis_tutari", "{0:c}")
		});
		xrTableCell392.Name = "xrTableCell392";
		xrTableCell392.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell392.StylePriority.UsePadding = false;
		xrTableCell392.Text = "xrTableCell350";
		xrTableCell392.Weight = 0.31167411030240877;
		xrTableCell393.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_siparis_adedi", "{0:n}")
		});
		xrTableCell393.Name = "xrTableCell393";
		xrTableCell393.StylePriority.UseTextAlignment = false;
		xrTableCell393.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell393.Weight = 0.1983380642304724;
		xrTableCell394.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_siparis_tutari", "{0:c}")
		});
		xrTableCell394.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell394.Name = "xrTableCell394";
		xrTableCell394.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell394.StylePriority.UseFont = false;
		xrTableCell394.StylePriority.UsePadding = false;
		xrTableCell394.StylePriority.UseTextAlignment = false;
		xrTableCell394.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell394.Weight = 0.3116741022520808;
		xrTableCell395.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_siparis_adedi", "{0:n0}")
		});
		xrTableCell395.Name = "xrTableCell395";
		xrTableCell395.StylePriority.UseTextAlignment = false;
		xrTableCell395.Text = "xrTableCell315";
		xrTableCell395.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell395.Weight = 0.19833806660193634;
		xrTableCell396.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_siparis_tutari", "{0:c}")
		});
		xrTableCell396.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell396.Name = "xrTableCell396";
		xrTableCell396.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell396.StylePriority.UseFont = false;
		xrTableCell396.StylePriority.UsePadding = false;
		xrTableCell396.StylePriority.UseTextAlignment = false;
		xrTableCell396.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell396.Weight = 0.31167402583928155;
		xrTableRow91.Cells.AddRange(new XRTableCell[8] { xrTableCell397, xrTableCell398, xrTableCell399, xrTableCell400, xrTableCell401, xrTableCell402, xrTableCell403, xrTableCell404 });
		xrTableRow91.Name = "xrTableRow91";
		xrTableRow91.Weight = 1.0;
		xrTableCell397.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell397.Name = "xrTableCell397";
		xrTableCell397.StylePriority.UseFont = false;
		xrTableCell397.StylePriority.UseTextAlignment = false;
		xrTableCell397.Text = "Fatura";
		xrTableCell397.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell397.Weight = 0.3539977672444706;
		xrTableCell398.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "ziyaret_basi_ortalama_fatura_tutari", "{0:c}")
		});
		xrTableCell398.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell398.Name = "xrTableCell398";
		xrTableCell398.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell398.StylePriority.UseFont = false;
		xrTableCell398.StylePriority.UsePadding = false;
		xrTableCell398.StylePriority.UseTextAlignment = false;
		xrTableCell398.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell398.Weight = 0.3116741216126203;
		xrTableCell399.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_fatura_adedi", "{0:n}")
		});
		xrTableCell399.Name = "xrTableCell399";
		xrTableCell399.StylePriority.UseTextAlignment = false;
		xrTableCell399.Text = "xrTableCell357";
		xrTableCell399.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell399.Weight = 0.1983380714848679;
		xrTableCell400.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_fatura_tutari", "{0:c}")
		});
		xrTableCell400.Name = "xrTableCell400";
		xrTableCell400.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell400.StylePriority.UsePadding = false;
		xrTableCell400.Text = "xrTableCell351";
		xrTableCell400.Weight = 0.31167411030240877;
		xrTableCell401.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_fatura_adedi", "{0:n}")
		});
		xrTableCell401.Name = "xrTableCell401";
		xrTableCell401.StylePriority.UseTextAlignment = false;
		xrTableCell401.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell401.Weight = 0.19833806423047243;
		xrTableCell402.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_fatura_tutari", "{0:c}")
		});
		xrTableCell402.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell402.Name = "xrTableCell402";
		xrTableCell402.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell402.StylePriority.UseFont = false;
		xrTableCell402.StylePriority.UsePadding = false;
		xrTableCell402.StylePriority.UseTextAlignment = false;
		xrTableCell402.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell402.Weight = 0.3116741022520808;
		xrTableCell403.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_fatura_adedi", "{0:n0}")
		});
		xrTableCell403.Name = "xrTableCell403";
		xrTableCell403.StylePriority.UseTextAlignment = false;
		xrTableCell403.Text = "xrTableCell316";
		xrTableCell403.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell403.Weight = 0.1983380666019363;
		xrTableCell404.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_fatura_tutari", "{0:c}")
		});
		xrTableCell404.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell404.Name = "xrTableCell404";
		xrTableCell404.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell404.StylePriority.UseFont = false;
		xrTableCell404.StylePriority.UsePadding = false;
		xrTableCell404.StylePriority.UseTextAlignment = false;
		xrTableCell404.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell404.Weight = 0.3116740149262028;
		xrTableRow92.Cells.AddRange(new XRTableCell[8] { xrTableCell405, xrTableCell406, xrTableCell407, xrTableCell408, xrTableCell409, xrTableCell410, xrTableCell411, xrTableCell412 });
		xrTableRow92.Name = "xrTableRow92";
		xrTableRow92.Weight = 1.0;
		xrTableCell405.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell405.Name = "xrTableCell405";
		xrTableCell405.StylePriority.UseFont = false;
		xrTableCell405.StylePriority.UseTextAlignment = false;
		xrTableCell405.Text = "Nakit tahsilat";
		xrTableCell405.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell405.Weight = 0.3539977130971329;
		xrTableCell406.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "ziyaret_basi_ortalama_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell406.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell406.Name = "xrTableCell406";
		xrTableCell406.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell406.StylePriority.UseFont = false;
		xrTableCell406.StylePriority.UsePadding = false;
		xrTableCell406.StylePriority.UseTextAlignment = false;
		xrTableCell406.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell406.Weight = 0.3116741006225573;
		xrTableCell407.Name = "xrTableCell407";
		xrTableCell407.StylePriority.UseTextAlignment = false;
		xrTableCell407.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell407.Weight = 0.19833807148486793;
		xrTableCell408.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell408.Name = "xrTableCell408";
		xrTableCell408.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell408.StylePriority.UsePadding = false;
		xrTableCell408.Text = "xrTableCell355";
		xrTableCell408.Weight = 0.31167411030240877;
		xrTableCell409.Name = "xrTableCell409";
		xrTableCell409.StylePriority.UseTextAlignment = false;
		xrTableCell409.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell409.Weight = 0.19833806423047234;
		xrTableCell410.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell410.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell410.Name = "xrTableCell410";
		xrTableCell410.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell410.StylePriority.UseFont = false;
		xrTableCell410.StylePriority.UsePadding = false;
		xrTableCell410.StylePriority.UseTextAlignment = false;
		xrTableCell410.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell410.Weight = 0.31167410225208103;
		xrTableCell411.Name = "xrTableCell411";
		xrTableCell411.StylePriority.UseTextAlignment = false;
		xrTableCell411.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell411.Weight = 0.19833806660193187;
		xrTableCell412.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell412.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell412.Name = "xrTableCell412";
		xrTableCell412.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell412.StylePriority.UseFont = false;
		xrTableCell412.StylePriority.UsePadding = false;
		xrTableCell412.StylePriority.UseTextAlignment = false;
		xrTableCell412.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell412.Weight = 0.3116740900636078;
		xrTableRow93.Cells.AddRange(new XRTableCell[8] { xrTableCell413, xrTableCell414, xrTableCell415, xrTableCell416, xrTableCell417, xrTableCell418, xrTableCell419, xrTableCell420 });
		xrTableRow93.Name = "xrTableRow93";
		xrTableRow93.Weight = 1.0;
		xrTableCell413.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell413.Name = "xrTableCell413";
		xrTableCell413.StylePriority.UseFont = false;
		xrTableCell413.StylePriority.UseTextAlignment = false;
		xrTableCell413.Text = "Çek tahsilat";
		xrTableCell413.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell413.Weight = 0.3539977672444706;
		xrTableCell414.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "ziyaret_basi_ortalama_cek_tutari", "{0:c}")
		});
		xrTableCell414.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell414.Name = "xrTableCell414";
		xrTableCell414.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell414.StylePriority.UseFont = false;
		xrTableCell414.StylePriority.UsePadding = false;
		xrTableCell414.StylePriority.UseTextAlignment = false;
		xrTableCell414.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell414.Weight = 0.31167410062255707;
		xrTableCell415.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_cek_adedi", "{0:n}")
		});
		xrTableCell415.Name = "xrTableCell415";
		xrTableCell415.StylePriority.UseTextAlignment = false;
		xrTableCell415.Text = "xrTableCell367";
		xrTableCell415.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell415.Weight = 0.19833807148486793;
		xrTableCell416.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_cek_tutari", "{0:c}")
		});
		xrTableCell416.Name = "xrTableCell416";
		xrTableCell416.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell416.StylePriority.UsePadding = false;
		xrTableCell416.Text = "xrTableCell358";
		xrTableCell416.Weight = 0.31167411030240877;
		xrTableCell417.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_cek_adedi", "{0:n}")
		});
		xrTableCell417.Name = "xrTableCell417";
		xrTableCell417.StylePriority.UseTextAlignment = false;
		xrTableCell417.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell417.Weight = 0.19833806423047246;
		xrTableCell418.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_cek_tutari", "{0:c}")
		});
		xrTableCell418.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell418.Name = "xrTableCell418";
		xrTableCell418.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell418.StylePriority.UseFont = false;
		xrTableCell418.StylePriority.UsePadding = false;
		xrTableCell418.StylePriority.UseTextAlignment = false;
		xrTableCell418.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell418.Weight = 0.3116741022520809;
		xrTableCell419.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_cek_adedi", "{0:n0}")
		});
		xrTableCell419.Name = "xrTableCell419";
		xrTableCell419.StylePriority.UseTextAlignment = false;
		xrTableCell419.Text = "xrTableCell317";
		xrTableCell419.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell419.Weight = 0.1983380666019363;
		xrTableCell420.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_cek_tutari", "{0:c}")
		});
		xrTableCell420.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell420.Name = "xrTableCell420";
		xrTableCell420.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell420.StylePriority.UseFont = false;
		xrTableCell420.StylePriority.UsePadding = false;
		xrTableCell420.StylePriority.UseTextAlignment = false;
		xrTableCell420.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell420.Weight = 0.3116740359162658;
		xrTableRow94.Cells.AddRange(new XRTableCell[8] { xrTableCell421, xrTableCell422, xrTableCell423, xrTableCell424, xrTableCell425, xrTableCell426, xrTableCell427, xrTableCell428 });
		xrTableRow94.Name = "xrTableRow94";
		xrTableRow94.Weight = 1.0;
		xrTableCell421.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell421.Name = "xrTableCell421";
		xrTableCell421.StylePriority.UseFont = false;
		xrTableCell421.StylePriority.UseTextAlignment = false;
		xrTableCell421.Text = "Kredi kartı tahsilat";
		xrTableCell421.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell421.Weight = 0.35399748559471145;
		xrTableCell422.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "ziyaret_basi_ortalama_kk_tutari", "{0:c}")
		});
		xrTableCell422.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell422.Name = "xrTableCell422";
		xrTableCell422.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell422.StylePriority.UseFont = false;
		xrTableCell422.StylePriority.UsePadding = false;
		xrTableCell422.StylePriority.UseTextAlignment = false;
		xrTableCell422.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell422.Weight = 0.311674122866762;
		xrTableCell423.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_kk_adedi", "{0:n}")
		});
		xrTableCell423.Name = "xrTableCell423";
		xrTableCell423.StylePriority.UseTextAlignment = false;
		xrTableCell423.Text = "xrTableCell371";
		xrTableCell423.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell423.Weight = 0.1983380714848679;
		xrTableCell424.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_kk_tutari", "{0:c}")
		});
		xrTableCell424.Name = "xrTableCell424";
		xrTableCell424.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell424.StylePriority.UsePadding = false;
		xrTableCell424.Text = "xrTableCell362";
		xrTableCell424.Weight = 0.31167411030240877;
		xrTableCell425.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_kk_adedi", "{0:n}")
		});
		xrTableCell425.Name = "xrTableCell425";
		xrTableCell425.StylePriority.UseTextAlignment = false;
		xrTableCell425.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell425.Weight = 0.19833806423047234;
		xrTableCell426.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_kk_tutari", "{0:c}")
		});
		xrTableCell426.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell426.Name = "xrTableCell426";
		xrTableCell426.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell426.StylePriority.UseFont = false;
		xrTableCell426.StylePriority.UsePadding = false;
		xrTableCell426.StylePriority.UseTextAlignment = false;
		xrTableCell426.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell426.Weight = 0.311674102252081;
		xrTableCell427.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_kk_adedi", "{0:n0}")
		});
		xrTableCell427.Name = "xrTableCell427";
		xrTableCell427.StylePriority.UseTextAlignment = false;
		xrTableCell427.Text = "xrTableCell314";
		xrTableCell427.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell427.Weight = 0.19833806660191036;
		xrTableCell428.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_kk_tutari", "{0:c}")
		});
		xrTableCell428.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell428.Name = "xrTableCell428";
		xrTableCell428.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell428.StylePriority.UseFont = false;
		xrTableCell428.StylePriority.UsePadding = false;
		xrTableCell428.StylePriority.UseTextAlignment = false;
		xrTableCell428.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell428.Weight = 0.31167429532184626;
		xrTableRow95.Cells.AddRange(new XRTableCell[8] { xrTableCell429, xrTableCell430, xrTableCell431, xrTableCell432, xrTableCell433, xrTableCell434, xrTableCell435, xrTableCell436 });
		xrTableRow95.Name = "xrTableRow95";
		xrTableRow95.Weight = 1.0;
		xrTableCell429.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell429.Name = "xrTableCell429";
		xrTableCell429.StylePriority.UseFont = false;
		xrTableCell429.StylePriority.UseTextAlignment = false;
		xrTableCell429.Text = "Toplam tahsilat";
		xrTableCell429.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell429.Weight = 0.3539977672444706;
		xrTableCell430.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "ziyaret_basi_ortalama_tahsilat_tutari", "{0:c}")
		});
		xrTableCell430.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell430.Name = "xrTableCell430";
		xrTableCell430.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell430.StylePriority.UseFont = false;
		xrTableCell430.StylePriority.UsePadding = false;
		xrTableCell430.StylePriority.UseTextAlignment = false;
		xrTableCell430.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell430.Weight = 0.3116741216126203;
		xrTableCell431.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_tahsilat_evrak_adedi", "{0:n}")
		});
		xrTableCell431.Name = "xrTableCell431";
		xrTableCell431.StylePriority.UseTextAlignment = false;
		xrTableCell431.Text = "xrTableCell359";
		xrTableCell431.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell431.Weight = 0.1983380714848679;
		xrTableCell432.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_tahsilat_tutari", "{0:c}")
		});
		xrTableCell432.Name = "xrTableCell432";
		xrTableCell432.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell432.StylePriority.UsePadding = false;
		xrTableCell432.Text = "xrTableCell352";
		xrTableCell432.Weight = 0.31167411030240877;
		xrTableCell433.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_tahsilat_evrak_adedi", "{0:n}")
		});
		xrTableCell433.Name = "xrTableCell433";
		xrTableCell433.StylePriority.UseTextAlignment = false;
		xrTableCell433.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell433.Weight = 0.19833806423047243;
		xrTableCell434.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_tahsilat_tutari", "{0:c}")
		});
		xrTableCell434.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell434.Name = "xrTableCell434";
		xrTableCell434.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell434.StylePriority.UseFont = false;
		xrTableCell434.StylePriority.UsePadding = false;
		xrTableCell434.StylePriority.UseTextAlignment = false;
		xrTableCell434.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell434.Weight = 0.3116741022520808;
		xrTableCell435.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_tahsilat_evrak_adedi", "{0:n0}")
		});
		xrTableCell435.Name = "xrTableCell435";
		xrTableCell435.StylePriority.UseTextAlignment = false;
		xrTableCell435.Text = "xrTableCell318";
		xrTableCell435.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell435.Weight = 0.1983380666019363;
		xrTableCell436.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_tahsilat_tutari", "{0:c}")
		});
		xrTableCell436.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell436.Name = "xrTableCell436";
		xrTableCell436.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell436.StylePriority.UseFont = false;
		xrTableCell436.StylePriority.UsePadding = false;
		xrTableCell436.StylePriority.UseTextAlignment = false;
		xrTableCell436.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell436.Weight = 0.3116740149262028;
		xrTableRow96.Cells.AddRange(new XRTableCell[8] { xrTableCell437, xrTableCell438, xrTableCell439, xrTableCell440, xrTableCell441, xrTableCell442, xrTableCell443, xrTableCell444 });
		xrTableRow96.Name = "xrTableRow96";
		xrTableRow96.Weight = 1.0;
		xrTableCell437.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell437.Name = "xrTableCell437";
		xrTableCell437.StylePriority.UseFont = false;
		xrTableCell437.StylePriority.UseTextAlignment = false;
		xrTableCell437.Text = "Masraf";
		xrTableCell437.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell437.Weight = 0.35399776724447063;
		xrTableCell438.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell438.Name = "xrTableCell438";
		xrTableCell438.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell438.StylePriority.UseFont = false;
		xrTableCell438.StylePriority.UsePadding = false;
		xrTableCell438.StylePriority.UseTextAlignment = false;
		xrTableCell438.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell438.Weight = 0.3116741216126202;
		xrTableCell439.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_masraf_adedi", "{0:n}")
		});
		xrTableCell439.Name = "xrTableCell439";
		xrTableCell439.StylePriority.UseTextAlignment = false;
		xrTableCell439.Text = "xrTableCell360";
		xrTableCell439.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell439.Weight = 0.1983380714848679;
		xrTableCell440.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_masraf_tutari", "{0:c}")
		});
		xrTableCell440.Name = "xrTableCell440";
		xrTableCell440.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell440.StylePriority.UsePadding = false;
		xrTableCell440.Text = "xrTableCell353";
		xrTableCell440.Weight = 0.31167411030240877;
		xrTableCell441.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_masraf_adedi", "{0:n}")
		});
		xrTableCell441.Name = "xrTableCell441";
		xrTableCell441.StylePriority.UseTextAlignment = false;
		xrTableCell441.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell441.Weight = 0.19833806423047237;
		xrTableCell442.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_masraf_tutari", "{0:c}")
		});
		xrTableCell442.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell442.Name = "xrTableCell442";
		xrTableCell442.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell442.StylePriority.UseFont = false;
		xrTableCell442.StylePriority.UsePadding = false;
		xrTableCell442.StylePriority.UseTextAlignment = false;
		xrTableCell442.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell442.Weight = 0.31167410225208086;
		xrTableCell443.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_masraf_adedi", "{0:n0}")
		});
		xrTableCell443.Name = "xrTableCell443";
		xrTableCell443.StylePriority.UseTextAlignment = false;
		xrTableCell443.Text = "xrTableCell319";
		xrTableCell443.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell443.Weight = 0.1983380666019363;
		xrTableCell444.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_masraf_tutari", "{0:c}")
		});
		xrTableCell444.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell444.Name = "xrTableCell444";
		xrTableCell444.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell444.StylePriority.UseFont = false;
		xrTableCell444.StylePriority.UsePadding = false;
		xrTableCell444.StylePriority.UseTextAlignment = false;
		xrTableCell444.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell444.Weight = 0.3116740149262028;
		xrTableRow97.Cells.AddRange(new XRTableCell[8] { xrTableCell445, xrTableCell446, xrTableCell447, xrTableCell448, xrTableCell449, xrTableCell450, xrTableCell451, xrTableCell452 });
		xrTableRow97.Name = "xrTableRow97";
		xrTableRow97.Weight = 1.0;
		xrTableCell445.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell445.Name = "xrTableCell445";
		xrTableCell445.StylePriority.UseFont = false;
		xrTableCell445.StylePriority.UseTextAlignment = false;
		xrTableCell445.Text = "Kalan nakit tutar";
		xrTableCell445.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell445.Weight = 0.3539977672444706;
		xrTableCell446.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell446.Name = "xrTableCell446";
		xrTableCell446.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell446.StylePriority.UseFont = false;
		xrTableCell446.StylePriority.UsePadding = false;
		xrTableCell446.StylePriority.UseTextAlignment = false;
		xrTableCell446.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell446.Weight = 0.3116741216126203;
		xrTableCell447.Name = "xrTableCell447";
		xrTableCell447.StylePriority.UseTextAlignment = false;
		xrTableCell447.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell447.Weight = 0.1983380714848679;
		xrTableCell448.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_kalan_nakit_tutar", "{0:c}")
		});
		xrTableCell448.Name = "xrTableCell448";
		xrTableCell448.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell448.StylePriority.UsePadding = false;
		xrTableCell448.Text = "xrTableCell354";
		xrTableCell448.Weight = 0.31167411030240877;
		xrTableCell449.Name = "xrTableCell449";
		xrTableCell449.StylePriority.UseTextAlignment = false;
		xrTableCell449.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell449.Weight = 0.19833806423047243;
		xrTableCell450.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_kalan_nakit_tutar", "{0:c}")
		});
		xrTableCell450.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell450.Name = "xrTableCell450";
		xrTableCell450.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell450.StylePriority.UseFont = false;
		xrTableCell450.StylePriority.UsePadding = false;
		xrTableCell450.StylePriority.UseTextAlignment = false;
		xrTableCell450.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell450.Weight = 0.3116741022520808;
		xrTableCell451.Name = "xrTableCell451";
		xrTableCell451.StylePriority.UseTextAlignment = false;
		xrTableCell451.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell451.Weight = 0.1983380666019363;
		xrTableCell452.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_kalan_nakit_tutar", "{0:c}")
		});
		xrTableCell452.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell452.Name = "xrTableCell452";
		xrTableCell452.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell452.StylePriority.UseFont = false;
		xrTableCell452.StylePriority.UsePadding = false;
		xrTableCell452.StylePriority.UseTextAlignment = false;
		xrTableCell452.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell452.Weight = 0.3116740149262028;
		xrTable31.Borders = BorderSide.All;
		xrTable31.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable31.LocationFloat = new PointFloat(9.999911f, 90.74996f);
		xrTable31.Name = "xrTable31";
		xrTable31.Rows.AddRange(new XRTableRow[9] { xrTableRow98, xrTableRow99, xrTableRow100, xrTableRow101, xrTableRow102, xrTableRow103, xrTableRow104, xrTableRow105, xrTableRow106 });
		xrTable31.SizeF = new SizeF(584.0128f, 185.4165f);
		xrTable31.StylePriority.UseBorders = false;
		xrTable31.StylePriority.UseFont = false;
		xrTable31.StylePriority.UseTextAlignment = false;
		xrTable31.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow98.Cells.AddRange(new XRTableCell[5] { xrTableCell453, xrTableCell454, xrTableCell455, xrTableCell456, xrTableCell457 });
		xrTableRow98.Name = "xrTableRow98";
		xrTableRow98.Weight = 1.0;
		xrTableCell453.Borders = BorderSide.Right | BorderSide.Bottom;
		xrTableCell453.Name = "xrTableCell453";
		xrTableCell453.StylePriority.UseBorders = false;
		xrTableCell453.Weight = 0.5094463594590848;
		xrTableCell454.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell454.Name = "xrTableCell454";
		xrTableCell454.StylePriority.UseFont = false;
		xrTableCell454.StylePriority.UseTextAlignment = false;
		xrTableCell454.Text = "Ziyaret başı";
		xrTableCell454.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell454.Weight = 0.3891260570315845;
		xrTableCell455.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell455.Name = "xrTableCell455";
		xrTableCell455.StylePriority.UseFont = false;
		xrTableCell455.StylePriority.UseTextAlignment = false;
		xrTableCell455.Text = "Temsilci başı";
		xrTableCell455.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell455.Weight = 0.38912603384429956;
		xrTableCell456.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell456.Name = "xrTableCell456";
		xrTableCell456.StylePriority.UseFont = false;
		xrTableCell456.StylePriority.UseTextAlignment = false;
		xrTableCell456.Text = "Günlük ortalama";
		xrTableCell456.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell456.Weight = 0.38912606083335527;
		xrTableCell457.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell457.Name = "xrTableCell457";
		xrTableCell457.StylePriority.UseFont = false;
		xrTableCell457.StylePriority.UseTextAlignment = false;
		xrTableCell457.Text = "Toplam";
		xrTableCell457.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell457.Weight = 0.38912628505579716;
		xrTableRow99.Cells.AddRange(new XRTableCell[5] { xrTableCell458, xrTableCell459, xrTableCell460, xrTableCell461, xrTableCell462 });
		xrTableRow99.Name = "xrTableRow99";
		xrTableRow99.Weight = 1.0;
		xrTableCell458.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell458.Name = "xrTableCell458";
		xrTableCell458.StylePriority.UseFont = false;
		xrTableCell458.StylePriority.UseTextAlignment = false;
		xrTableCell458.Text = "Yapılan km";
		xrTableCell458.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell458.Weight = 0.5094466755617562;
		xrTableCell459.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "ziyaret_basi_ortalama_yapilan_km", "{0:n0}")
		});
		xrTableCell459.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell459.Name = "xrTableCell459";
		xrTableCell459.StylePriority.UseFont = false;
		xrTableCell459.StylePriority.UseTextAlignment = false;
		xrTableCell459.Text = "xrTableCell29";
		xrTableCell459.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell459.Weight = 0.38912605763637903;
		xrTableCell460.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_yapilan_km", "{0:n0}")
		});
		xrTableCell460.Name = "xrTableCell460";
		xrTableCell460.StylePriority.UseTextAlignment = false;
		xrTableCell460.Text = "xrTableCell366";
		xrTableCell460.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell460.Weight = 0.38912602828127374;
		xrTableCell461.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_gunluk_yapilan_km", "{0:n0}")
		});
		xrTableCell461.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell461.Name = "xrTableCell461";
		xrTableCell461.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell461.StylePriority.UseFont = false;
		xrTableCell461.StylePriority.UsePadding = false;
		xrTableCell461.StylePriority.UseTextAlignment = false;
		xrTableCell461.Text = "xrTableCell30";
		xrTableCell461.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell461.Weight = 0.3891260552703295;
		xrTableCell462.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_yapilan_km", "{0:n0}")
		});
		xrTableCell462.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell462.Name = "xrTableCell462";
		xrTableCell462.StylePriority.UseFont = false;
		xrTableCell462.StylePriority.UseTextAlignment = false;
		xrTableCell462.Text = "xrTableCell210";
		xrTableCell462.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell462.Weight = 0.3891259794743829;
		xrTableRow100.Cells.AddRange(new XRTableCell[5] { xrTableCell463, xrTableCell464, xrTableCell465, xrTableCell466, xrTableCell467 });
		xrTableRow100.Name = "xrTableRow100";
		xrTableRow100.Weight = 1.0;
		xrTableCell463.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell463.Name = "xrTableCell463";
		xrTableCell463.StylePriority.UseFont = false;
		xrTableCell463.StylePriority.UseTextAlignment = false;
		xrTableCell463.Text = "Mesai süresi";
		xrTableCell463.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell463.Weight = 0.5094466755617562;
		xrTableCell464.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell464.Name = "xrTableCell464";
		xrTableCell464.StylePriority.UseFont = false;
		xrTableCell464.StylePriority.UseTextAlignment = false;
		xrTableCell464.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell464.Weight = 0.38912605763637903;
		xrTableCell465.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_mesai_suresi")
		});
		xrTableCell465.Name = "xrTableCell465";
		xrTableCell465.StylePriority.UseTextAlignment = false;
		xrTableCell465.Text = "xrTableCell368";
		xrTableCell465.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell465.Weight = 0.38912602828127374;
		xrTableCell466.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell466.Name = "xrTableCell466";
		xrTableCell466.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell466.StylePriority.UseFont = false;
		xrTableCell466.StylePriority.UsePadding = false;
		xrTableCell466.StylePriority.UseTextAlignment = false;
		xrTableCell466.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell466.Weight = 0.3891260552703295;
		xrTableCell467.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_mesai_suresi")
		});
		xrTableCell467.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell467.Name = "xrTableCell467";
		xrTableCell467.StylePriority.UseFont = false;
		xrTableCell467.StylePriority.UseTextAlignment = false;
		xrTableCell467.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell467.Weight = 0.3891259794743829;
		xrTableRow101.Cells.AddRange(new XRTableCell[5] { xrTableCell468, xrTableCell469, xrTableCell470, xrTableCell471, xrTableCell472 });
		xrTableRow101.Name = "xrTableRow101";
		xrTableRow101.Weight = 1.0;
		xrTableCell468.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell468.Name = "xrTableCell468";
		xrTableCell468.StylePriority.UseFont = false;
		xrTableCell468.StylePriority.UseTextAlignment = false;
		xrTableCell468.Text = "Ziyaret süresi";
		xrTableCell468.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell468.Weight = 0.5094466755617562;
		xrTableCell469.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "ziyaret_basi_ortalama_ziyaret_suresi")
		});
		xrTableCell469.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell469.Name = "xrTableCell469";
		xrTableCell469.StylePriority.UseFont = false;
		xrTableCell469.StylePriority.UseTextAlignment = false;
		xrTableCell469.Text = "xrTableCell171";
		xrTableCell469.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell469.Weight = 0.38912605763637903;
		xrTableCell470.Name = "xrTableCell470";
		xrTableCell470.StylePriority.UseTextAlignment = false;
		xrTableCell470.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell470.Weight = 0.38912602828127374;
		xrTableCell471.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_ziyaret_suresi")
		});
		xrTableCell471.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell471.Name = "xrTableCell471";
		xrTableCell471.StylePriority.UseFont = false;
		xrTableCell471.StylePriority.UseTextAlignment = false;
		xrTableCell471.Text = "xrTableCell172";
		xrTableCell471.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell471.Weight = 0.3891260552703295;
		xrTableCell472.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_ziyaret_suresi")
		});
		xrTableCell472.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell472.Name = "xrTableCell472";
		xrTableCell472.StylePriority.UseFont = false;
		xrTableCell472.StylePriority.UseTextAlignment = false;
		xrTableCell472.Text = "xrTableCell212";
		xrTableCell472.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell472.Weight = 0.3891259794743829;
		xrTableRow102.Cells.AddRange(new XRTableCell[5] { xrTableCell473, xrTableCell474, xrTableCell475, xrTableCell476, xrTableCell477 });
		xrTableRow102.Name = "xrTableRow102";
		xrTableRow102.Weight = 1.0;
		xrTableCell473.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell473.Name = "xrTableCell473";
		xrTableCell473.StylePriority.UseFont = false;
		xrTableCell473.StylePriority.UseTextAlignment = false;
		xrTableCell473.Text = "Ulaşım süresi";
		xrTableCell473.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell473.Weight = 0.5094466755617562;
		xrTableCell474.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "ziyaret_basi_ortalama_ulasim_suresi")
		});
		xrTableCell474.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell474.Name = "xrTableCell474";
		xrTableCell474.StylePriority.UseFont = false;
		xrTableCell474.StylePriority.UseTextAlignment = false;
		xrTableCell474.Text = "xrTableCell174";
		xrTableCell474.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell474.Weight = 0.38912605763637903;
		xrTableCell475.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_ulasim_suresi")
		});
		xrTableCell475.Name = "xrTableCell475";
		xrTableCell475.StylePriority.UseTextAlignment = false;
		xrTableCell475.Text = "xrTableCell370";
		xrTableCell475.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell475.Weight = 0.38912602828127374;
		xrTableCell476.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_ulasim_suresi")
		});
		xrTableCell476.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell476.Name = "xrTableCell476";
		xrTableCell476.StylePriority.UseFont = false;
		xrTableCell476.StylePriority.UseTextAlignment = false;
		xrTableCell476.Text = "xrTableCell175";
		xrTableCell476.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell476.Weight = 0.3891260552703295;
		xrTableCell477.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_ulasim_suresi")
		});
		xrTableCell477.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell477.Name = "xrTableCell477";
		xrTableCell477.StylePriority.UseFont = false;
		xrTableCell477.StylePriority.UseTextAlignment = false;
		xrTableCell477.Text = "xrTableCell213";
		xrTableCell477.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell477.Weight = 0.3891259794743829;
		xrTableRow103.Cells.AddRange(new XRTableCell[5] { xrTableCell478, xrTableCell479, xrTableCell480, xrTableCell481, xrTableCell482 });
		xrTableRow103.Name = "xrTableRow103";
		xrTableRow103.Weight = 1.0;
		xrTableCell478.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell478.Name = "xrTableCell478";
		xrTableCell478.StylePriority.UseFont = false;
		xrTableCell478.StylePriority.UseTextAlignment = false;
		xrTableCell478.Text = "Hedef ziyaret";
		xrTableCell478.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell478.Weight = 0.5094466755617562;
		xrTableCell479.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell479.Name = "xrTableCell479";
		xrTableCell479.StylePriority.UseFont = false;
		xrTableCell479.StylePriority.UseTextAlignment = false;
		xrTableCell479.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell479.Weight = 0.38912605763637903;
		xrTableCell480.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_hedef_ziyaret")
		});
		xrTableCell480.Name = "xrTableCell480";
		xrTableCell480.StylePriority.UseTextAlignment = false;
		xrTableCell480.Text = "xrTableCell372";
		xrTableCell480.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell480.Weight = 0.38912602828127374;
		xrTableCell481.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_gunluk_hedef_ziyaret", "{0:n}")
		});
		xrTableCell481.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell481.Name = "xrTableCell481";
		xrTableCell481.StylePriority.UseFont = false;
		xrTableCell481.StylePriority.UseTextAlignment = false;
		xrTableCell481.Text = "xrTableCell178";
		xrTableCell481.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell481.Weight = 0.3891260552703295;
		xrTableCell482.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_hedef_ziyaret")
		});
		xrTableCell482.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell482.Name = "xrTableCell482";
		xrTableCell482.StylePriority.UseFont = false;
		xrTableCell482.StylePriority.UseTextAlignment = false;
		xrTableCell482.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell482.Weight = 0.3891259794743829;
		xrTableRow104.Cells.AddRange(new XRTableCell[5] { xrTableCell483, xrTableCell484, xrTableCell485, xrTableCell486, xrTableCell487 });
		xrTableRow104.Name = "xrTableRow104";
		xrTableRow104.Weight = 1.0;
		xrTableCell483.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell483.Name = "xrTableCell483";
		xrTableCell483.StylePriority.UseFont = false;
		xrTableCell483.StylePriority.UseTextAlignment = false;
		xrTableCell483.Text = "Yapılan ziyaret";
		xrTableCell483.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell483.Weight = 0.5094466755617562;
		xrTableCell484.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell484.Name = "xrTableCell484";
		xrTableCell484.StylePriority.UseFont = false;
		xrTableCell484.StylePriority.UseTextAlignment = false;
		xrTableCell484.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell484.Weight = 0.38912605763637903;
		xrTableCell485.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_yapilan_ziyaret")
		});
		xrTableCell485.Name = "xrTableCell485";
		xrTableCell485.StylePriority.UseTextAlignment = false;
		xrTableCell485.Text = "xrTableCell373";
		xrTableCell485.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell485.Weight = 0.38912602828127374;
		xrTableCell486.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_yapilan_ziyaret", "{0:n}")
		});
		xrTableCell486.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell486.Name = "xrTableCell486";
		xrTableCell486.StylePriority.UseFont = false;
		xrTableCell486.StylePriority.UseTextAlignment = false;
		xrTableCell486.Text = "xrTableCell181";
		xrTableCell486.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell486.Weight = 0.3891260552703295;
		xrTableCell487.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_yapilan_ziyaret")
		});
		xrTableCell487.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell487.Name = "xrTableCell487";
		xrTableCell487.StylePriority.UseFont = false;
		xrTableCell487.StylePriority.UseTextAlignment = false;
		xrTableCell487.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell487.Weight = 0.3891259794743829;
		xrTableRow105.Cells.AddRange(new XRTableCell[5] { xrTableCell488, xrTableCell489, xrTableCell490, xrTableCell491, xrTableCell492 });
		xrTableRow105.Name = "xrTableRow105";
		xrTableRow105.Weight = 1.0;
		xrTableCell488.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell488.Name = "xrTableCell488";
		xrTableCell488.StylePriority.UseFont = false;
		xrTableCell488.StylePriority.UseTextAlignment = false;
		xrTableCell488.Text = "Yapılmayan ziyaret";
		xrTableCell488.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell488.Weight = 0.5094466755617562;
		xrTableCell489.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell489.Name = "xrTableCell489";
		xrTableCell489.StylePriority.UseFont = false;
		xrTableCell489.StylePriority.UseTextAlignment = false;
		xrTableCell489.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell489.Weight = 0.38912605763637903;
		xrTableCell490.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_yapilmayan_ziyaret")
		});
		xrTableCell490.Name = "xrTableCell490";
		xrTableCell490.StylePriority.UseTextAlignment = false;
		xrTableCell490.Text = "xrTableCell374";
		xrTableCell490.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell490.Weight = 0.38912602828127374;
		xrTableCell491.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_yapilmayan_ziyaret", "{0:n}")
		});
		xrTableCell491.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell491.Name = "xrTableCell491";
		xrTableCell491.StylePriority.UseFont = false;
		xrTableCell491.StylePriority.UseTextAlignment = false;
		xrTableCell491.Text = "xrTableCell206";
		xrTableCell491.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell491.Weight = 0.3891260552703295;
		xrTableCell492.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_yapilmayan_ziyaret")
		});
		xrTableCell492.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell492.Name = "xrTableCell492";
		xrTableCell492.StylePriority.UseFont = false;
		xrTableCell492.StylePriority.UseTextAlignment = false;
		xrTableCell492.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell492.Weight = 0.3891259794743829;
		xrTableRow106.Cells.AddRange(new XRTableCell[5] { xrTableCell493, xrTableCell494, xrTableCell495, xrTableCell496, xrTableCell497 });
		xrTableRow106.Name = "xrTableRow106";
		xrTableRow106.Weight = 1.0;
		xrTableCell493.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell493.Name = "xrTableCell493";
		xrTableCell493.StylePriority.UseFont = false;
		xrTableCell493.StylePriority.UseTextAlignment = false;
		xrTableCell493.Text = "Rota dışı ziyaret";
		xrTableCell493.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell493.Weight = 0.5094466755617562;
		xrTableCell494.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell494.Name = "xrTableCell494";
		xrTableCell494.StylePriority.UseFont = false;
		xrTableCell494.StylePriority.UseTextAlignment = false;
		xrTableCell494.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell494.Weight = 0.38912605763637903;
		xrTableCell495.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "temsilci_basi_rota_disi_ziyaret")
		});
		xrTableCell495.Name = "xrTableCell495";
		xrTableCell495.StylePriority.UseTextAlignment = false;
		xrTableCell495.Text = "xrTableCell375";
		xrTableCell495.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell495.Weight = 0.38912602828127374;
		xrTableCell496.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gunluk_ortalama_rota_disi_ziyaret", "{0:n}")
		});
		xrTableCell496.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell496.Name = "xrTableCell496";
		xrTableCell496.StylePriority.UseFont = false;
		xrTableCell496.StylePriority.UseTextAlignment = false;
		xrTableCell496.Text = "xrTableCell209";
		xrTableCell496.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell496.Weight = 0.3891260552703295;
		xrTableCell497.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_rota_disi_ziyaret")
		});
		xrTableCell497.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell497.Name = "xrTableCell497";
		xrTableCell497.StylePriority.UseFont = false;
		xrTableCell497.StylePriority.UseTextAlignment = false;
		xrTableCell497.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell497.Weight = 0.3891259794743829;
		xrLabel12.Font = new Font("Arial", 12f, FontStyle.Bold | FontStyle.Underline);
		xrLabel12.LocationFloat = new PointFloat(0f, 0f);
		xrLabel12.Name = "xrLabel12";
		xrLabel12.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel12.SizeF = new SizeF(789f, 23f);
		xrLabel12.StylePriority.UseFont = false;
		xrLabel12.StylePriority.UseTextAlignment = false;
		xrLabel12.Text = "RAPOR ÖZETİ";
		xrLabel12.TextAlignment = TextAlignment.MiddleCenter;
		TopMargin.HeightF = 22.08333f;
		TopMargin.Name = "TopMargin";
		TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		TopMargin.TextAlignment = TextAlignment.TopLeft;
		BottomMargin.Controls.AddRange(new XRControl[1] { xrPageInfo1 });
		BottomMargin.HeightF = 55.29162f;
		BottomMargin.Name = "BottomMargin";
		BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		BottomMargin.TextAlignment = TextAlignment.TopLeft;
		xrPageInfo1.Font = new Font("Arial", 9.75f);
		xrPageInfo1.LocationFloat = new PointFloat(678.9999f, 10f);
		xrPageInfo1.Name = "xrPageInfo1";
		xrPageInfo1.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrPageInfo1.SizeF = new SizeF(100f, 23f);
		xrPageInfo1.StylePriority.UseFont = false;
		xrPageInfo1.StylePriority.UseTextAlignment = false;
		xrPageInfo1.TextAlignment = TextAlignment.MiddleRight;
		formattingRule1.Formatting.Font = new Font("Arial", 14.25f, FontStyle.Bold, GraphicsUnit.Point, 162);
		formattingRule1.Name = "formattingRule1";
		xrControlStyle1.BackColor = Color.White;
		xrControlStyle1.Name = "xrControlStyle1";
		xrControlStyle1.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		xrControlStyle2.BackColor = Color.FromArgb(240, 240, 240);
		xrControlStyle2.Name = "xrControlStyle2";
		xrControlStyle2.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		Bolgeler.Bands.AddRange(new Band[2] { Bolge_Genel_Ozet, Temsilciler });
		Bolgeler.DataMember = "bolgeler";
		Bolgeler.DataSource = bindingSource1;
		Bolgeler.Level = 0;
		Bolgeler.Name = "Bolgeler";
		Bolge_Genel_Ozet.Controls.AddRange(new XRControl[8] { xrTable28, xrTable29, xrLabel18, xrLabel23, xrLabel24, xrLabel25, xrLabel26, xrLabel11 });
		Bolge_Genel_Ozet.Expanded = false;
		Bolge_Genel_Ozet.HeightF = 533.5647f;
		Bolge_Genel_Ozet.Name = "Bolge_Genel_Ozet";
		Bolge_Genel_Ozet.PageBreak = PageBreak.AfterBand;
		xrTable28.Borders = BorderSide.All;
		xrTable28.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable28.LocationFloat = new PointFloat(9.999943f, 106.25f);
		xrTable28.Name = "xrTable28";
		xrTable28.Rows.AddRange(new XRTableRow[9] { xrTableRow69, xrTableRow70, xrTableRow71, xrTableRow72, xrTableRow73, xrTableRow74, xrTableRow75, xrTableRow76, xrTableRow77 });
		xrTable28.SizeF = new SizeF(584.0128f, 185.4165f);
		xrTable28.StylePriority.UseBorders = false;
		xrTable28.StylePriority.UseFont = false;
		xrTable28.StylePriority.UseTextAlignment = false;
		xrTable28.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow69.Cells.AddRange(new XRTableCell[5] { xrTableCell236, xrTableCell237, xrTableCell365, xrTableCell238, xrTableCell239 });
		xrTableRow69.Name = "xrTableRow69";
		xrTableRow69.Weight = 1.0;
		xrTableCell236.Borders = BorderSide.Right | BorderSide.Bottom;
		xrTableCell236.Name = "xrTableCell236";
		xrTableCell236.StylePriority.UseBorders = false;
		xrTableCell236.Weight = 0.5094463594590848;
		xrTableCell237.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell237.Name = "xrTableCell237";
		xrTableCell237.StylePriority.UseFont = false;
		xrTableCell237.StylePriority.UseTextAlignment = false;
		xrTableCell237.Text = "Ziyaret başı";
		xrTableCell237.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell237.Weight = 0.3891260570315845;
		xrTableCell365.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell365.Name = "xrTableCell365";
		xrTableCell365.StylePriority.UseFont = false;
		xrTableCell365.StylePriority.UseTextAlignment = false;
		xrTableCell365.Text = "Temsilci başı";
		xrTableCell365.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell365.Weight = 0.38912603384429956;
		xrTableCell238.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell238.Name = "xrTableCell238";
		xrTableCell238.StylePriority.UseFont = false;
		xrTableCell238.StylePriority.UseTextAlignment = false;
		xrTableCell238.Text = "Günlük ortalama";
		xrTableCell238.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell238.Weight = 0.38912606083335527;
		xrTableCell239.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell239.Name = "xrTableCell239";
		xrTableCell239.StylePriority.UseFont = false;
		xrTableCell239.StylePriority.UseTextAlignment = false;
		xrTableCell239.Text = "Toplam";
		xrTableCell239.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell239.Weight = 0.38912628505579716;
		xrTableRow70.Cells.AddRange(new XRTableCell[5] { xrTableCell240, xrTableCell241, xrTableCell366, xrTableCell242, xrTableCell243 });
		xrTableRow70.Name = "xrTableRow70";
		xrTableRow70.Weight = 1.0;
		xrTableCell240.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell240.Name = "xrTableCell240";
		xrTableCell240.StylePriority.UseFont = false;
		xrTableCell240.StylePriority.UseTextAlignment = false;
		xrTableCell240.Text = "Yapılan km";
		xrTableCell240.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell240.Weight = 0.5094466755617562;
		xrTableCell241.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.ziyaret_basi_ortalama_yapilan_km", "{0:n0}")
		});
		xrTableCell241.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell241.Name = "xrTableCell241";
		xrTableCell241.StylePriority.UseFont = false;
		xrTableCell241.StylePriority.UseTextAlignment = false;
		xrTableCell241.Text = "xrTableCell29";
		xrTableCell241.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell241.Weight = 0.38912605763637903;
		xrTableCell366.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_yapilan_km", "{0:n0}")
		});
		xrTableCell366.Name = "xrTableCell366";
		xrTableCell366.StylePriority.UseTextAlignment = false;
		xrTableCell366.Text = "xrTableCell366";
		xrTableCell366.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell366.Weight = 0.38912602828127374;
		xrTableCell242.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_gunluk_yapilan_km", "{0:n0}")
		});
		xrTableCell242.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell242.Name = "xrTableCell242";
		xrTableCell242.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell242.StylePriority.UseFont = false;
		xrTableCell242.StylePriority.UsePadding = false;
		xrTableCell242.StylePriority.UseTextAlignment = false;
		xrTableCell242.Text = "xrTableCell30";
		xrTableCell242.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell242.Weight = 0.3891260552703295;
		xrTableCell243.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_yapilan_km", "{0:n0}")
		});
		xrTableCell243.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell243.Name = "xrTableCell243";
		xrTableCell243.StylePriority.UseFont = false;
		xrTableCell243.StylePriority.UseTextAlignment = false;
		xrTableCell243.Text = "xrTableCell210";
		xrTableCell243.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell243.Weight = 0.3891259794743829;
		xrTableRow71.Cells.AddRange(new XRTableCell[5] { xrTableCell244, xrTableCell254, xrTableCell368, xrTableCell255, xrTableCell256 });
		xrTableRow71.Name = "xrTableRow71";
		xrTableRow71.Weight = 1.0;
		xrTableCell244.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell244.Name = "xrTableCell244";
		xrTableCell244.StylePriority.UseFont = false;
		xrTableCell244.StylePriority.UseTextAlignment = false;
		xrTableCell244.Text = "Mesai süresi";
		xrTableCell244.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell244.Weight = 0.5094466755617562;
		xrTableCell254.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell254.Name = "xrTableCell254";
		xrTableCell254.StylePriority.UseFont = false;
		xrTableCell254.StylePriority.UseTextAlignment = false;
		xrTableCell254.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell254.Weight = 0.38912605763637903;
		xrTableCell368.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_mesai_suresi")
		});
		xrTableCell368.Name = "xrTableCell368";
		xrTableCell368.StylePriority.UseTextAlignment = false;
		xrTableCell368.Text = "xrTableCell368";
		xrTableCell368.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell368.Weight = 0.38912602828127374;
		xrTableCell255.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_mesai_suresi")
		});
		xrTableCell255.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell255.Name = "xrTableCell255";
		xrTableCell255.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell255.StylePriority.UseFont = false;
		xrTableCell255.StylePriority.UsePadding = false;
		xrTableCell255.StylePriority.UseTextAlignment = false;
		xrTableCell255.Text = "xrTableCell21";
		xrTableCell255.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell255.Weight = 0.3891260552703295;
		xrTableCell256.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_mesai_suresi")
		});
		xrTableCell256.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell256.Name = "xrTableCell256";
		xrTableCell256.StylePriority.UseFont = false;
		xrTableCell256.StylePriority.UseTextAlignment = false;
		xrTableCell256.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell256.Weight = 0.3891259794743829;
		xrTableRow72.Cells.AddRange(new XRTableCell[5] { xrTableCell257, xrTableCell258, xrTableCell369, xrTableCell259, xrTableCell260 });
		xrTableRow72.Name = "xrTableRow72";
		xrTableRow72.Weight = 1.0;
		xrTableCell257.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell257.Name = "xrTableCell257";
		xrTableCell257.StylePriority.UseFont = false;
		xrTableCell257.StylePriority.UseTextAlignment = false;
		xrTableCell257.Text = "Ziyaret süresi";
		xrTableCell257.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell257.Weight = 0.5094466755617562;
		xrTableCell258.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.ziyaret_basi_ortalama_ziyaret_suresi")
		});
		xrTableCell258.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell258.Name = "xrTableCell258";
		xrTableCell258.StylePriority.UseFont = false;
		xrTableCell258.StylePriority.UseTextAlignment = false;
		xrTableCell258.Text = "xrTableCell171";
		xrTableCell258.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell258.Weight = 0.38912605763637903;
		xrTableCell369.Name = "xrTableCell369";
		xrTableCell369.StylePriority.UseTextAlignment = false;
		xrTableCell369.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell369.Weight = 0.38912602828127374;
		xrTableCell259.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_ziyaret_suresi")
		});
		xrTableCell259.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell259.Name = "xrTableCell259";
		xrTableCell259.StylePriority.UseFont = false;
		xrTableCell259.StylePriority.UseTextAlignment = false;
		xrTableCell259.Text = "xrTableCell172";
		xrTableCell259.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell259.Weight = 0.3891260552703295;
		xrTableCell260.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_ziyaret_suresi")
		});
		xrTableCell260.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell260.Name = "xrTableCell260";
		xrTableCell260.StylePriority.UseFont = false;
		xrTableCell260.StylePriority.UseTextAlignment = false;
		xrTableCell260.Text = "xrTableCell212";
		xrTableCell260.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell260.Weight = 0.3891259794743829;
		xrTableRow73.Cells.AddRange(new XRTableCell[5] { xrTableCell261, xrTableCell262, xrTableCell370, xrTableCell263, xrTableCell264 });
		xrTableRow73.Name = "xrTableRow73";
		xrTableRow73.Weight = 1.0;
		xrTableCell261.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell261.Name = "xrTableCell261";
		xrTableCell261.StylePriority.UseFont = false;
		xrTableCell261.StylePriority.UseTextAlignment = false;
		xrTableCell261.Text = "Ulaşım süresi";
		xrTableCell261.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell261.Weight = 0.5094466755617562;
		xrTableCell262.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.ziyaret_basi_ortalama_ulasim_suresi")
		});
		xrTableCell262.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell262.Name = "xrTableCell262";
		xrTableCell262.StylePriority.UseFont = false;
		xrTableCell262.StylePriority.UseTextAlignment = false;
		xrTableCell262.Text = "xrTableCell174";
		xrTableCell262.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell262.Weight = 0.38912605763637903;
		xrTableCell370.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_ulasim_suresi")
		});
		xrTableCell370.Name = "xrTableCell370";
		xrTableCell370.StylePriority.UseTextAlignment = false;
		xrTableCell370.Text = "xrTableCell370";
		xrTableCell370.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell370.Weight = 0.38912602828127374;
		xrTableCell263.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_ulasim_suresi")
		});
		xrTableCell263.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell263.Name = "xrTableCell263";
		xrTableCell263.StylePriority.UseFont = false;
		xrTableCell263.StylePriority.UseTextAlignment = false;
		xrTableCell263.Text = "xrTableCell175";
		xrTableCell263.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell263.Weight = 0.3891260552703295;
		xrTableCell264.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_ulasim_suresi")
		});
		xrTableCell264.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell264.Name = "xrTableCell264";
		xrTableCell264.StylePriority.UseFont = false;
		xrTableCell264.StylePriority.UseTextAlignment = false;
		xrTableCell264.Text = "xrTableCell213";
		xrTableCell264.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell264.Weight = 0.3891259794743829;
		xrTableRow74.Cells.AddRange(new XRTableCell[5] { xrTableCell265, xrTableCell266, xrTableCell372, xrTableCell267, xrTableCell268 });
		xrTableRow74.Name = "xrTableRow74";
		xrTableRow74.Weight = 1.0;
		xrTableCell265.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell265.Name = "xrTableCell265";
		xrTableCell265.StylePriority.UseFont = false;
		xrTableCell265.StylePriority.UseTextAlignment = false;
		xrTableCell265.Text = "Hedef ziyaret";
		xrTableCell265.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell265.Weight = 0.5094466755617562;
		xrTableCell266.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell266.Name = "xrTableCell266";
		xrTableCell266.StylePriority.UseFont = false;
		xrTableCell266.StylePriority.UseTextAlignment = false;
		xrTableCell266.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell266.Weight = 0.38912605763637903;
		xrTableCell372.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_hedef_ziyaret")
		});
		xrTableCell372.Name = "xrTableCell372";
		xrTableCell372.StylePriority.UseTextAlignment = false;
		xrTableCell372.Text = "xrTableCell372";
		xrTableCell372.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell372.Weight = 0.38912602828127374;
		xrTableCell267.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_gunluk_hedef_ziyaret", "{0:n}")
		});
		xrTableCell267.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell267.Name = "xrTableCell267";
		xrTableCell267.StylePriority.UseFont = false;
		xrTableCell267.StylePriority.UseTextAlignment = false;
		xrTableCell267.Text = "xrTableCell178";
		xrTableCell267.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell267.Weight = 0.3891260552703295;
		xrTableCell268.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_hedef_ziyaret")
		});
		xrTableCell268.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell268.Name = "xrTableCell268";
		xrTableCell268.StylePriority.UseFont = false;
		xrTableCell268.StylePriority.UseTextAlignment = false;
		xrTableCell268.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell268.Weight = 0.3891259794743829;
		xrTableRow75.Cells.AddRange(new XRTableCell[5] { xrTableCell269, xrTableCell270, xrTableCell373, xrTableCell271, xrTableCell272 });
		xrTableRow75.Name = "xrTableRow75";
		xrTableRow75.Weight = 1.0;
		xrTableCell269.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell269.Name = "xrTableCell269";
		xrTableCell269.StylePriority.UseFont = false;
		xrTableCell269.StylePriority.UseTextAlignment = false;
		xrTableCell269.Text = "Yapılan ziyaret";
		xrTableCell269.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell269.Weight = 0.5094466755617562;
		xrTableCell270.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell270.Name = "xrTableCell270";
		xrTableCell270.StylePriority.UseFont = false;
		xrTableCell270.StylePriority.UseTextAlignment = false;
		xrTableCell270.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell270.Weight = 0.38912605763637903;
		xrTableCell373.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_yapilan_ziyaret")
		});
		xrTableCell373.Name = "xrTableCell373";
		xrTableCell373.StylePriority.UseTextAlignment = false;
		xrTableCell373.Text = "xrTableCell373";
		xrTableCell373.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell373.Weight = 0.38912602828127374;
		xrTableCell271.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_yapilan_ziyaret", "{0:n}")
		});
		xrTableCell271.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell271.Name = "xrTableCell271";
		xrTableCell271.StylePriority.UseFont = false;
		xrTableCell271.StylePriority.UseTextAlignment = false;
		xrTableCell271.Text = "xrTableCell181";
		xrTableCell271.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell271.Weight = 0.3891260552703295;
		xrTableCell272.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_yapilan_ziyaret")
		});
		xrTableCell272.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell272.Name = "xrTableCell272";
		xrTableCell272.StylePriority.UseFont = false;
		xrTableCell272.StylePriority.UseTextAlignment = false;
		xrTableCell272.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell272.Weight = 0.3891259794743829;
		xrTableRow76.Cells.AddRange(new XRTableCell[5] { xrTableCell273, xrTableCell274, xrTableCell374, xrTableCell275, xrTableCell276 });
		xrTableRow76.Name = "xrTableRow76";
		xrTableRow76.Weight = 1.0;
		xrTableCell273.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell273.Name = "xrTableCell273";
		xrTableCell273.StylePriority.UseFont = false;
		xrTableCell273.StylePriority.UseTextAlignment = false;
		xrTableCell273.Text = "Yapılmayan ziyaret";
		xrTableCell273.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell273.Weight = 0.5094466755617562;
		xrTableCell274.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell274.Name = "xrTableCell274";
		xrTableCell274.StylePriority.UseFont = false;
		xrTableCell274.StylePriority.UseTextAlignment = false;
		xrTableCell274.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell274.Weight = 0.38912605763637903;
		xrTableCell374.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_yapilmayan_ziyaret")
		});
		xrTableCell374.Name = "xrTableCell374";
		xrTableCell374.StylePriority.UseTextAlignment = false;
		xrTableCell374.Text = "xrTableCell374";
		xrTableCell374.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell374.Weight = 0.38912602828127374;
		xrTableCell275.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_yapilmayan_ziyaret", "{0:n}")
		});
		xrTableCell275.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell275.Name = "xrTableCell275";
		xrTableCell275.StylePriority.UseFont = false;
		xrTableCell275.StylePriority.UseTextAlignment = false;
		xrTableCell275.Text = "xrTableCell206";
		xrTableCell275.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell275.Weight = 0.3891260552703295;
		xrTableCell276.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_yapilmayan_ziyaret")
		});
		xrTableCell276.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell276.Name = "xrTableCell276";
		xrTableCell276.StylePriority.UseFont = false;
		xrTableCell276.StylePriority.UseTextAlignment = false;
		xrTableCell276.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell276.Weight = 0.3891259794743829;
		xrTableRow77.Cells.AddRange(new XRTableCell[5] { xrTableCell277, xrTableCell278, xrTableCell375, xrTableCell279, xrTableCell280 });
		xrTableRow77.Name = "xrTableRow77";
		xrTableRow77.Weight = 1.0;
		xrTableCell277.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell277.Name = "xrTableCell277";
		xrTableCell277.StylePriority.UseFont = false;
		xrTableCell277.StylePriority.UseTextAlignment = false;
		xrTableCell277.Text = "Rota dışı ziyaret";
		xrTableCell277.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell277.Weight = 0.5094466755617562;
		xrTableCell278.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell278.Name = "xrTableCell278";
		xrTableCell278.StylePriority.UseFont = false;
		xrTableCell278.StylePriority.UseTextAlignment = false;
		xrTableCell278.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell278.Weight = 0.38912605763637903;
		xrTableCell375.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_rota_disi_ziyaret")
		});
		xrTableCell375.Name = "xrTableCell375";
		xrTableCell375.StylePriority.UseTextAlignment = false;
		xrTableCell375.Text = "xrTableCell375";
		xrTableCell375.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell375.Weight = 0.38912602828127374;
		xrTableCell279.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_rota_disi_ziyaret", "{0:n}")
		});
		xrTableCell279.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell279.Name = "xrTableCell279";
		xrTableCell279.StylePriority.UseFont = false;
		xrTableCell279.StylePriority.UseTextAlignment = false;
		xrTableCell279.Text = "xrTableCell209";
		xrTableCell279.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell279.Weight = 0.3891260552703295;
		xrTableCell280.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_rota_disi_ziyaret")
		});
		xrTableCell280.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell280.Name = "xrTableCell280";
		xrTableCell280.StylePriority.UseFont = false;
		xrTableCell280.StylePriority.UseTextAlignment = false;
		xrTableCell280.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell280.Weight = 0.3891259794743829;
		xrTable29.Borders = BorderSide.All;
		xrTable29.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable29.LocationFloat = new PointFloat(9.999943f, 311.9214f);
		xrTable29.Name = "xrTable29";
		xrTable29.Rows.AddRange(new XRTableRow[10] { xrTableRow78, xrTableRow79, xrTableRow80, xrTableRow81, xrTableRow82, xrTableRow83, xrTableRow84, xrTableRow85, xrTableRow86, xrTableRow87 });
		xrTable29.SizeF = new SizeF(774.9374f, 206.0183f);
		xrTable29.StylePriority.UseBorders = false;
		xrTable29.StylePriority.UseFont = false;
		xrTable29.StylePriority.UseTextAlignment = false;
		xrTable29.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow78.Cells.AddRange(new XRTableCell[5] { xrTableCell281, xrTableCell282, xrTableCell348, xrTableCell283, xrTableCell284 });
		xrTableRow78.Name = "xrTableRow78";
		xrTableRow78.Weight = 1.0;
		xrTableCell281.Borders = BorderSide.Right;
		xrTableCell281.Name = "xrTableCell281";
		xrTableCell281.StylePriority.UseBorders = false;
		xrTableCell281.Weight = 0.3539978303423901;
		xrTableCell282.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell282.Name = "xrTableCell282";
		xrTableCell282.StylePriority.UseFont = false;
		xrTableCell282.StylePriority.UseTextAlignment = false;
		xrTableCell282.Text = "Ziyaret başı";
		xrTableCell282.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell282.Weight = 0.3116740938483716;
		xrTableCell348.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell348.Name = "xrTableCell348";
		xrTableCell348.StylePriority.UseFont = false;
		xrTableCell348.StylePriority.UseTextAlignment = false;
		xrTableCell348.Text = "Temsilci başı";
		xrTableCell348.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell348.Weight = 0.5100121289111983;
		xrTableCell283.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell283.Name = "xrTableCell283";
		xrTableCell283.StylePriority.UseFont = false;
		xrTableCell283.StylePriority.UseTextAlignment = false;
		xrTableCell283.Text = "Günlük ortalama";
		xrTableCell283.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell283.Weight = 0.510011999208395;
		xrTableCell284.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell284.Name = "xrTableCell284";
		xrTableCell284.StylePriority.UseFont = false;
		xrTableCell284.StylePriority.UseTextAlignment = false;
		xrTableCell284.Text = "Toplam";
		xrTableCell284.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell284.Weight = 0.5100122663447048;
		xrTableRow79.Cells.AddRange(new XRTableCell[8] { xrTableCell285, xrTableCell286, xrTableCell363, xrTableCell349, xrTableCell287, xrTableCell288, xrTableCell289, xrTableCell290 });
		xrTableRow79.Name = "xrTableRow79";
		xrTableRow79.Weight = 1.0;
		xrTableCell285.Borders = BorderSide.Right | BorderSide.Bottom;
		xrTableCell285.Name = "xrTableCell285";
		xrTableCell285.StylePriority.UseBorders = false;
		xrTableCell285.Weight = 0.35399754869263084;
		xrTableCell286.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell286.Name = "xrTableCell286";
		xrTableCell286.StylePriority.UseFont = false;
		xrTableCell286.StylePriority.UseTextAlignment = false;
		xrTableCell286.Text = "Tutar";
		xrTableCell286.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell286.Weight = 0.31167412514895654;
		xrTableCell363.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell363.Name = "xrTableCell363";
		xrTableCell363.StylePriority.UseFont = false;
		xrTableCell363.StylePriority.UseTextAlignment = false;
		xrTableCell363.Text = "Adet";
		xrTableCell363.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell363.Weight = 0.19833807135463175;
		xrTableCell349.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell349.Name = "xrTableCell349";
		xrTableCell349.StylePriority.UseFont = false;
		xrTableCell349.StylePriority.UseTextAlignment = false;
		xrTableCell349.Text = "Tutar";
		xrTableCell349.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell349.Weight = 0.3116741101721726;
		xrTableCell287.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell287.Name = "xrTableCell287";
		xrTableCell287.StylePriority.UseFont = false;
		xrTableCell287.StylePriority.UseTextAlignment = false;
		xrTableCell287.Text = "Adet";
		xrTableCell287.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell287.Weight = 0.19833806396999998;
		xrTableCell288.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell288.Name = "xrTableCell288";
		xrTableCell288.StylePriority.UseFont = false;
		xrTableCell288.StylePriority.UseTextAlignment = false;
		xrTableCell288.Text = "Tutar";
		xrTableCell288.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell288.Weight = 0.3116741098610186;
		xrTableCell289.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell289.Name = "xrTableCell289";
		xrTableCell289.StylePriority.UseFont = false;
		xrTableCell289.StylePriority.UseTextAlignment = false;
		xrTableCell289.Text = "Adet";
		xrTableCell289.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell289.Weight = 0.19833807328858916;
		xrTableCell290.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell290.Name = "xrTableCell290";
		xrTableCell290.StylePriority.UseFont = false;
		xrTableCell290.StylePriority.UseTextAlignment = false;
		xrTableCell290.Text = "Tutar";
		xrTableCell290.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell290.Weight = 0.31167421616706065;
		xrTableRow80.Cells.AddRange(new XRTableCell[8] { xrTableCell291, xrTableCell292, xrTableCell356, xrTableCell350, xrTableCell293, xrTableCell294, xrTableCell295, xrTableCell296 });
		xrTableRow80.Name = "xrTableRow80";
		xrTableRow80.Weight = 1.0;
		xrTableCell291.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell291.Name = "xrTableCell291";
		xrTableCell291.StylePriority.UseFont = false;
		xrTableCell291.StylePriority.UseTextAlignment = false;
		xrTableCell291.Text = "Sipariş";
		xrTableCell291.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell291.Weight = 0.35399776724447063;
		xrTableCell292.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.ziyaret_basi_ortalama_siparis_tutari", "{0:c}")
		});
		xrTableCell292.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell292.Name = "xrTableCell292";
		xrTableCell292.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell292.StylePriority.UseFont = false;
		xrTableCell292.StylePriority.UsePadding = false;
		xrTableCell292.StylePriority.UseTextAlignment = false;
		xrTableCell292.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell292.Weight = 0.31167411069954143;
		xrTableCell356.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_siparis_adedi", "{0:n}")
		});
		xrTableCell356.Name = "xrTableCell356";
		xrTableCell356.StylePriority.UseTextAlignment = false;
		xrTableCell356.Text = "xrTableCell356";
		xrTableCell356.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell356.Weight = 0.1983380714848679;
		xrTableCell350.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_siparis_tutari", "{0:c}")
		});
		xrTableCell350.Name = "xrTableCell350";
		xrTableCell350.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell350.StylePriority.UsePadding = false;
		xrTableCell350.Text = "xrTableCell350";
		xrTableCell350.Weight = 0.31167411030240877;
		xrTableCell293.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_siparis_adedi", "{0:n}")
		});
		xrTableCell293.Name = "xrTableCell293";
		xrTableCell293.StylePriority.UseTextAlignment = false;
		xrTableCell293.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell293.Weight = 0.1983380642304724;
		xrTableCell294.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_siparis_tutari", "{0:c}")
		});
		xrTableCell294.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell294.Name = "xrTableCell294";
		xrTableCell294.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell294.StylePriority.UseFont = false;
		xrTableCell294.StylePriority.UsePadding = false;
		xrTableCell294.StylePriority.UseTextAlignment = false;
		xrTableCell294.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell294.Weight = 0.3116741022520808;
		xrTableCell295.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_siparis_adedi", "{0:n0}")
		});
		xrTableCell295.Name = "xrTableCell295";
		xrTableCell295.StylePriority.UseTextAlignment = false;
		xrTableCell295.Text = "xrTableCell315";
		xrTableCell295.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell295.Weight = 0.19833806660193634;
		xrTableCell296.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_siparis_tutari", "{0:c}")
		});
		xrTableCell296.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell296.Name = "xrTableCell296";
		xrTableCell296.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell296.StylePriority.UseFont = false;
		xrTableCell296.StylePriority.UsePadding = false;
		xrTableCell296.StylePriority.UseTextAlignment = false;
		xrTableCell296.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell296.Weight = 0.31167402583928155;
		xrTableRow81.Cells.AddRange(new XRTableCell[8] { xrTableCell297, xrTableCell298, xrTableCell357, xrTableCell351, xrTableCell299, xrTableCell300, xrTableCell301, xrTableCell302 });
		xrTableRow81.Name = "xrTableRow81";
		xrTableRow81.Weight = 1.0;
		xrTableCell297.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell297.Name = "xrTableCell297";
		xrTableCell297.StylePriority.UseFont = false;
		xrTableCell297.StylePriority.UseTextAlignment = false;
		xrTableCell297.Text = "Fatura";
		xrTableCell297.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell297.Weight = 0.3539977672444706;
		xrTableCell298.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.ziyaret_basi_ortalama_fatura_tutari", "{0:c}")
		});
		xrTableCell298.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell298.Name = "xrTableCell298";
		xrTableCell298.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell298.StylePriority.UseFont = false;
		xrTableCell298.StylePriority.UsePadding = false;
		xrTableCell298.StylePriority.UseTextAlignment = false;
		xrTableCell298.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell298.Weight = 0.3116741216126203;
		xrTableCell357.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_fatura_adedi", "{0:n}")
		});
		xrTableCell357.Name = "xrTableCell357";
		xrTableCell357.StylePriority.UseTextAlignment = false;
		xrTableCell357.Text = "xrTableCell357";
		xrTableCell357.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell357.Weight = 0.1983380714848679;
		xrTableCell351.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_fatura_tutari", "{0:c}")
		});
		xrTableCell351.Name = "xrTableCell351";
		xrTableCell351.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell351.StylePriority.UsePadding = false;
		xrTableCell351.Text = "xrTableCell351";
		xrTableCell351.Weight = 0.31167411030240877;
		xrTableCell299.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_fatura_adedi", "{0:n}")
		});
		xrTableCell299.Name = "xrTableCell299";
		xrTableCell299.StylePriority.UseTextAlignment = false;
		xrTableCell299.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell299.Weight = 0.19833806423047243;
		xrTableCell300.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_fatura_tutari", "{0:c}")
		});
		xrTableCell300.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell300.Name = "xrTableCell300";
		xrTableCell300.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell300.StylePriority.UseFont = false;
		xrTableCell300.StylePriority.UsePadding = false;
		xrTableCell300.StylePriority.UseTextAlignment = false;
		xrTableCell300.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell300.Weight = 0.3116741022520808;
		xrTableCell301.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_fatura_adedi", "{0:n0}")
		});
		xrTableCell301.Name = "xrTableCell301";
		xrTableCell301.StylePriority.UseTextAlignment = false;
		xrTableCell301.Text = "xrTableCell316";
		xrTableCell301.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell301.Weight = 0.1983380666019363;
		xrTableCell302.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_fatura_tutari", "{0:c}")
		});
		xrTableCell302.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell302.Name = "xrTableCell302";
		xrTableCell302.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell302.StylePriority.UseFont = false;
		xrTableCell302.StylePriority.UsePadding = false;
		xrTableCell302.StylePriority.UseTextAlignment = false;
		xrTableCell302.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell302.Weight = 0.3116740149262028;
		xrTableRow82.Cells.AddRange(new XRTableCell[8] { xrTableCell303, xrTableCell304, xrTableCell364, xrTableCell355, xrTableCell305, xrTableCell306, xrTableCell307, xrTableCell308 });
		xrTableRow82.Name = "xrTableRow82";
		xrTableRow82.Weight = 1.0;
		xrTableCell303.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell303.Name = "xrTableCell303";
		xrTableCell303.StylePriority.UseFont = false;
		xrTableCell303.StylePriority.UseTextAlignment = false;
		xrTableCell303.Text = "Nakit tahsilat";
		xrTableCell303.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell303.Weight = 0.3539977130971329;
		xrTableCell304.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.ziyaret_basi_ortalama_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell304.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell304.Name = "xrTableCell304";
		xrTableCell304.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell304.StylePriority.UseFont = false;
		xrTableCell304.StylePriority.UsePadding = false;
		xrTableCell304.StylePriority.UseTextAlignment = false;
		xrTableCell304.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell304.Weight = 0.3116741006225573;
		xrTableCell364.Name = "xrTableCell364";
		xrTableCell364.StylePriority.UseTextAlignment = false;
		xrTableCell364.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell364.Weight = 0.19833807148486793;
		xrTableCell355.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell355.Name = "xrTableCell355";
		xrTableCell355.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell355.StylePriority.UsePadding = false;
		xrTableCell355.Text = "xrTableCell355";
		xrTableCell355.Weight = 0.31167411030240877;
		xrTableCell305.Name = "xrTableCell305";
		xrTableCell305.StylePriority.UseTextAlignment = false;
		xrTableCell305.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell305.Weight = 0.19833806423047234;
		xrTableCell306.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell306.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell306.Name = "xrTableCell306";
		xrTableCell306.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell306.StylePriority.UseFont = false;
		xrTableCell306.StylePriority.UsePadding = false;
		xrTableCell306.StylePriority.UseTextAlignment = false;
		xrTableCell306.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell306.Weight = 0.31167410225208103;
		xrTableCell307.Name = "xrTableCell307";
		xrTableCell307.StylePriority.UseTextAlignment = false;
		xrTableCell307.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell307.Weight = 0.19833806660193187;
		xrTableCell308.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell308.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell308.Name = "xrTableCell308";
		xrTableCell308.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell308.StylePriority.UseFont = false;
		xrTableCell308.StylePriority.UsePadding = false;
		xrTableCell308.StylePriority.UseTextAlignment = false;
		xrTableCell308.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell308.Weight = 0.3116740900636078;
		xrTableRow83.Cells.AddRange(new XRTableCell[8] { xrTableCell309, xrTableCell310, xrTableCell367, xrTableCell358, xrTableCell311, xrTableCell321, xrTableCell322, xrTableCell323 });
		xrTableRow83.Name = "xrTableRow83";
		xrTableRow83.Weight = 1.0;
		xrTableCell309.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell309.Name = "xrTableCell309";
		xrTableCell309.StylePriority.UseFont = false;
		xrTableCell309.StylePriority.UseTextAlignment = false;
		xrTableCell309.Text = "Çek tahsilat";
		xrTableCell309.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell309.Weight = 0.3539977672444706;
		xrTableCell310.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.ziyaret_basi_ortalama_cek_tutari", "{0:c}")
		});
		xrTableCell310.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell310.Name = "xrTableCell310";
		xrTableCell310.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell310.StylePriority.UseFont = false;
		xrTableCell310.StylePriority.UsePadding = false;
		xrTableCell310.StylePriority.UseTextAlignment = false;
		xrTableCell310.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell310.Weight = 0.31167410062255707;
		xrTableCell367.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_cek_adedi", "{0:n}")
		});
		xrTableCell367.Name = "xrTableCell367";
		xrTableCell367.StylePriority.UseTextAlignment = false;
		xrTableCell367.Text = "xrTableCell367";
		xrTableCell367.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell367.Weight = 0.19833807148486793;
		xrTableCell358.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_cek_tutari", "{0:c}")
		});
		xrTableCell358.Name = "xrTableCell358";
		xrTableCell358.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell358.StylePriority.UsePadding = false;
		xrTableCell358.Text = "xrTableCell358";
		xrTableCell358.Weight = 0.31167411030240877;
		xrTableCell311.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_cek_adedi", "{0:n}")
		});
		xrTableCell311.Name = "xrTableCell311";
		xrTableCell311.StylePriority.UseTextAlignment = false;
		xrTableCell311.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell311.Weight = 0.19833806423047246;
		xrTableCell321.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_cek_tutari", "{0:c}")
		});
		xrTableCell321.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell321.Name = "xrTableCell321";
		xrTableCell321.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell321.StylePriority.UseFont = false;
		xrTableCell321.StylePriority.UsePadding = false;
		xrTableCell321.StylePriority.UseTextAlignment = false;
		xrTableCell321.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell321.Weight = 0.3116741022520809;
		xrTableCell322.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_cek_adedi", "{0:n0}")
		});
		xrTableCell322.Name = "xrTableCell322";
		xrTableCell322.StylePriority.UseTextAlignment = false;
		xrTableCell322.Text = "xrTableCell317";
		xrTableCell322.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell322.Weight = 0.1983380666019363;
		xrTableCell323.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_cek_tutari", "{0:c}")
		});
		xrTableCell323.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell323.Name = "xrTableCell323";
		xrTableCell323.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell323.StylePriority.UseFont = false;
		xrTableCell323.StylePriority.UsePadding = false;
		xrTableCell323.StylePriority.UseTextAlignment = false;
		xrTableCell323.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell323.Weight = 0.3116740359162658;
		xrTableRow84.Cells.AddRange(new XRTableCell[8] { xrTableCell324, xrTableCell325, xrTableCell371, xrTableCell362, xrTableCell326, xrTableCell327, xrTableCell328, xrTableCell329 });
		xrTableRow84.Name = "xrTableRow84";
		xrTableRow84.Weight = 1.0;
		xrTableCell324.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell324.Name = "xrTableCell324";
		xrTableCell324.StylePriority.UseFont = false;
		xrTableCell324.StylePriority.UseTextAlignment = false;
		xrTableCell324.Text = "Kredi kartı tahsilat";
		xrTableCell324.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell324.Weight = 0.35399748559471145;
		xrTableCell325.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.ziyaret_basi_ortalama_kk_tutari", "{0:c}")
		});
		xrTableCell325.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell325.Name = "xrTableCell325";
		xrTableCell325.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell325.StylePriority.UseFont = false;
		xrTableCell325.StylePriority.UsePadding = false;
		xrTableCell325.StylePriority.UseTextAlignment = false;
		xrTableCell325.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell325.Weight = 0.311674122866762;
		xrTableCell371.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_kk_adedi", "{0:n}")
		});
		xrTableCell371.Name = "xrTableCell371";
		xrTableCell371.StylePriority.UseTextAlignment = false;
		xrTableCell371.Text = "xrTableCell371";
		xrTableCell371.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell371.Weight = 0.1983380714848679;
		xrTableCell362.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_kk_tutari", "{0:c}")
		});
		xrTableCell362.Name = "xrTableCell362";
		xrTableCell362.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell362.StylePriority.UsePadding = false;
		xrTableCell362.Text = "xrTableCell362";
		xrTableCell362.Weight = 0.31167411030240877;
		xrTableCell326.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_kk_adedi", "{0:n}")
		});
		xrTableCell326.Name = "xrTableCell326";
		xrTableCell326.StylePriority.UseTextAlignment = false;
		xrTableCell326.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell326.Weight = 0.19833806423047234;
		xrTableCell327.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_kk_tutari", "{0:c}")
		});
		xrTableCell327.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell327.Name = "xrTableCell327";
		xrTableCell327.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell327.StylePriority.UseFont = false;
		xrTableCell327.StylePriority.UsePadding = false;
		xrTableCell327.StylePriority.UseTextAlignment = false;
		xrTableCell327.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell327.Weight = 0.311674102252081;
		xrTableCell328.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_kk_adedi", "{0:n0}")
		});
		xrTableCell328.Name = "xrTableCell328";
		xrTableCell328.StylePriority.UseTextAlignment = false;
		xrTableCell328.Text = "xrTableCell314";
		xrTableCell328.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell328.Weight = 0.19833806660191036;
		xrTableCell329.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_kk_tutari", "{0:c}")
		});
		xrTableCell329.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell329.Name = "xrTableCell329";
		xrTableCell329.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell329.StylePriority.UseFont = false;
		xrTableCell329.StylePriority.UsePadding = false;
		xrTableCell329.StylePriority.UseTextAlignment = false;
		xrTableCell329.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell329.Weight = 0.31167429532184626;
		xrTableRow85.Cells.AddRange(new XRTableCell[8] { xrTableCell330, xrTableCell331, xrTableCell359, xrTableCell352, xrTableCell332, xrTableCell333, xrTableCell334, xrTableCell335 });
		xrTableRow85.Name = "xrTableRow85";
		xrTableRow85.Weight = 1.0;
		xrTableCell330.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell330.Name = "xrTableCell330";
		xrTableCell330.StylePriority.UseFont = false;
		xrTableCell330.StylePriority.UseTextAlignment = false;
		xrTableCell330.Text = "Toplam tahsilat";
		xrTableCell330.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell330.Weight = 0.3539977672444706;
		xrTableCell331.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.ziyaret_basi_ortalama_tahsilat_tutari", "{0:c}")
		});
		xrTableCell331.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell331.Name = "xrTableCell331";
		xrTableCell331.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell331.StylePriority.UseFont = false;
		xrTableCell331.StylePriority.UsePadding = false;
		xrTableCell331.StylePriority.UseTextAlignment = false;
		xrTableCell331.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell331.Weight = 0.3116741216126203;
		xrTableCell359.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_tahsilat_evrak_adedi", "{0:n}")
		});
		xrTableCell359.Name = "xrTableCell359";
		xrTableCell359.StylePriority.UseTextAlignment = false;
		xrTableCell359.Text = "xrTableCell359";
		xrTableCell359.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell359.Weight = 0.1983380714848679;
		xrTableCell352.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_tahsilat_tutari", "{0:c}")
		});
		xrTableCell352.Name = "xrTableCell352";
		xrTableCell352.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell352.StylePriority.UsePadding = false;
		xrTableCell352.Text = "xrTableCell352";
		xrTableCell352.Weight = 0.31167411030240877;
		xrTableCell332.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_tahsilat_evrak_adedi", "{0:n}")
		});
		xrTableCell332.Name = "xrTableCell332";
		xrTableCell332.StylePriority.UseTextAlignment = false;
		xrTableCell332.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell332.Weight = 0.19833806423047243;
		xrTableCell333.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_tahsilat_tutari", "{0:c}")
		});
		xrTableCell333.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell333.Name = "xrTableCell333";
		xrTableCell333.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell333.StylePriority.UseFont = false;
		xrTableCell333.StylePriority.UsePadding = false;
		xrTableCell333.StylePriority.UseTextAlignment = false;
		xrTableCell333.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell333.Weight = 0.3116741022520808;
		xrTableCell334.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_tahsilat_evrak_adedi", "{0:n0}")
		});
		xrTableCell334.Name = "xrTableCell334";
		xrTableCell334.StylePriority.UseTextAlignment = false;
		xrTableCell334.Text = "xrTableCell318";
		xrTableCell334.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell334.Weight = 0.1983380666019363;
		xrTableCell335.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_tahsilat_tutari", "{0:c}")
		});
		xrTableCell335.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell335.Name = "xrTableCell335";
		xrTableCell335.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell335.StylePriority.UseFont = false;
		xrTableCell335.StylePriority.UsePadding = false;
		xrTableCell335.StylePriority.UseTextAlignment = false;
		xrTableCell335.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell335.Weight = 0.3116740149262028;
		xrTableRow86.Cells.AddRange(new XRTableCell[8] { xrTableCell336, xrTableCell337, xrTableCell360, xrTableCell353, xrTableCell338, xrTableCell339, xrTableCell340, xrTableCell341 });
		xrTableRow86.Name = "xrTableRow86";
		xrTableRow86.Weight = 1.0;
		xrTableCell336.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell336.Name = "xrTableCell336";
		xrTableCell336.StylePriority.UseFont = false;
		xrTableCell336.StylePriority.UseTextAlignment = false;
		xrTableCell336.Text = "Masraf";
		xrTableCell336.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell336.Weight = 0.35399776724447063;
		xrTableCell337.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell337.Name = "xrTableCell337";
		xrTableCell337.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell337.StylePriority.UseFont = false;
		xrTableCell337.StylePriority.UsePadding = false;
		xrTableCell337.StylePriority.UseTextAlignment = false;
		xrTableCell337.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell337.Weight = 0.3116741216126202;
		xrTableCell360.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_masraf_adedi", "{0:n}")
		});
		xrTableCell360.Name = "xrTableCell360";
		xrTableCell360.StylePriority.UseTextAlignment = false;
		xrTableCell360.Text = "xrTableCell360";
		xrTableCell360.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell360.Weight = 0.1983380714848679;
		xrTableCell353.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_masraf_tutari", "{0:c}")
		});
		xrTableCell353.Name = "xrTableCell353";
		xrTableCell353.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell353.StylePriority.UsePadding = false;
		xrTableCell353.Text = "xrTableCell353";
		xrTableCell353.Weight = 0.31167411030240877;
		xrTableCell338.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_masraf_adedi", "{0:n}")
		});
		xrTableCell338.Name = "xrTableCell338";
		xrTableCell338.StylePriority.UseTextAlignment = false;
		xrTableCell338.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell338.Weight = 0.19833806423047237;
		xrTableCell339.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_masraf_tutari", "{0:c}")
		});
		xrTableCell339.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell339.Name = "xrTableCell339";
		xrTableCell339.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell339.StylePriority.UseFont = false;
		xrTableCell339.StylePriority.UsePadding = false;
		xrTableCell339.StylePriority.UseTextAlignment = false;
		xrTableCell339.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell339.Weight = 0.31167410225208086;
		xrTableCell340.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_masraf_adedi", "{0:n0}")
		});
		xrTableCell340.Name = "xrTableCell340";
		xrTableCell340.StylePriority.UseTextAlignment = false;
		xrTableCell340.Text = "xrTableCell319";
		xrTableCell340.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell340.Weight = 0.1983380666019363;
		xrTableCell341.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_masraf_tutari", "{0:c}")
		});
		xrTableCell341.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell341.Name = "xrTableCell341";
		xrTableCell341.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell341.StylePriority.UseFont = false;
		xrTableCell341.StylePriority.UsePadding = false;
		xrTableCell341.StylePriority.UseTextAlignment = false;
		xrTableCell341.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell341.Weight = 0.3116740149262028;
		xrTableRow87.Cells.AddRange(new XRTableCell[8] { xrTableCell342, xrTableCell343, xrTableCell361, xrTableCell354, xrTableCell344, xrTableCell345, xrTableCell346, xrTableCell347 });
		xrTableRow87.Name = "xrTableRow87";
		xrTableRow87.Weight = 1.0;
		xrTableCell342.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell342.Name = "xrTableCell342";
		xrTableCell342.StylePriority.UseFont = false;
		xrTableCell342.StylePriority.UseTextAlignment = false;
		xrTableCell342.Text = "Kalan nakit tutar";
		xrTableCell342.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell342.Weight = 0.3539977672444706;
		xrTableCell343.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell343.Name = "xrTableCell343";
		xrTableCell343.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell343.StylePriority.UseFont = false;
		xrTableCell343.StylePriority.UsePadding = false;
		xrTableCell343.StylePriority.UseTextAlignment = false;
		xrTableCell343.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell343.Weight = 0.3116741216126203;
		xrTableCell361.Name = "xrTableCell361";
		xrTableCell361.StylePriority.UseTextAlignment = false;
		xrTableCell361.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell361.Weight = 0.1983380714848679;
		xrTableCell354.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_basi_kalan_nakit_tutar", "{0:c}")
		});
		xrTableCell354.Name = "xrTableCell354";
		xrTableCell354.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell354.StylePriority.UsePadding = false;
		xrTableCell354.Text = "xrTableCell354";
		xrTableCell354.Weight = 0.31167411030240877;
		xrTableCell344.Name = "xrTableCell344";
		xrTableCell344.StylePriority.UseTextAlignment = false;
		xrTableCell344.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell344.Weight = 0.19833806423047243;
		xrTableCell345.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.gunluk_ortalama_kalan_nakit_tutar", "{0:c}")
		});
		xrTableCell345.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell345.Name = "xrTableCell345";
		xrTableCell345.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell345.StylePriority.UseFont = false;
		xrTableCell345.StylePriority.UsePadding = false;
		xrTableCell345.StylePriority.UseTextAlignment = false;
		xrTableCell345.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell345.Weight = 0.3116741022520808;
		xrTableCell346.Name = "xrTableCell346";
		xrTableCell346.StylePriority.UseTextAlignment = false;
		xrTableCell346.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell346.Weight = 0.1983380666019363;
		xrTableCell347.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.toplam_kalan_nakit_tutar", "{0:c}")
		});
		xrTableCell347.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell347.Name = "xrTableCell347";
		xrTableCell347.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell347.StylePriority.UseFont = false;
		xrTableCell347.StylePriority.UsePadding = false;
		xrTableCell347.StylePriority.UseTextAlignment = false;
		xrTableCell347.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell347.Weight = 0.3116740149262028;
		xrLabel18.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.bolge_kodu")
		});
		xrLabel18.Font = new Font("Arial", 12f, FontStyle.Bold);
		xrLabel18.LocationFloat = new PointFloat(9.999943f, 34.45835f);
		xrLabel18.Name = "xrLabel18";
		xrLabel18.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel18.SizeF = new SizeF(168.8776f, 23f);
		xrLabel18.StylePriority.UseFont = false;
		xrLabel18.StylePriority.UseTextAlignment = false;
		xrLabel18.Text = "xrLabel4";
		xrLabel18.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel23.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.bolge_adi")
		});
		xrLabel23.Font = new Font("Arial", 12f, FontStyle.Bold);
		xrLabel23.LocationFloat = new PointFloat(252.414f, 34.45835f);
		xrLabel23.Name = "xrLabel23";
		xrLabel23.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel23.SizeF = new SizeF(288.6692f, 23f);
		xrLabel23.StylePriority.UseFont = false;
		xrLabel23.StylePriority.UseTextAlignment = false;
		xrLabel23.Text = "xrLabel4";
		xrLabel23.TextAlignment = TextAlignment.MiddleCenter;
		xrLabel24.Font = new Font("Arial", 12f, FontStyle.Bold);
		xrLabel24.LocationFloat = new PointFloat(557.4434f, 34.45835f);
		xrLabel24.Name = "xrLabel24";
		xrLabel24.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel24.SizeF = new SizeF(221.5566f, 23f);
		xrLabel24.StylePriority.UseFont = false;
		xrLabel24.StylePriority.UseTextAlignment = false;
		xrLabel24.Text = "[baslangic_tarihi!dd.MM.yyyy] - [bitis_tarihi!dd.MM.yyyy]";
		xrLabel24.TextAlignment = TextAlignment.MiddleRight;
		xrLabel25.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilci_sayisi", "{0:n0}")
		});
		xrLabel25.Font = new Font("Arial", 9.75f);
		xrLabel25.LocationFloat = new PointFloat(154.0126f, 72.87496f);
		xrLabel25.Name = "xrLabel25";
		xrLabel25.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel25.SizeF = new SizeF(240.9986f, 18.00002f);
		xrLabel25.StylePriority.UseFont = false;
		xrLabel25.StylePriority.UseTextAlignment = false;
		xrLabel25.Text = "xrLabel4";
		xrLabel25.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel26.Font = new Font("Arial", 9.75f, FontStyle.Bold);
		xrLabel26.LocationFloat = new PointFloat(10.00001f, 72.87502f);
		xrLabel26.Name = "xrLabel26";
		xrLabel26.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel26.SizeF = new SizeF(144.0127f, 18f);
		xrLabel26.StylePriority.UseFont = false;
		xrLabel26.StylePriority.UseTextAlignment = false;
		xrLabel26.Text = "Temsilci sayısı :";
		xrLabel26.TextAlignment = TextAlignment.MiddleRight;
		xrLabel11.Font = new Font("Arial", 12f, FontStyle.Bold | FontStyle.Underline);
		xrLabel11.LocationFloat = new PointFloat(0f, 0f);
		xrLabel11.Name = "xrLabel11";
		xrLabel11.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel11.SizeF = new SizeF(789f, 23f);
		xrLabel11.StylePriority.UseFont = false;
		xrLabel11.StylePriority.UseTextAlignment = false;
		xrLabel11.Text = "BÖLGE ÖZETİ";
		xrLabel11.TextAlignment = TextAlignment.MiddleCenter;
		Temsilciler.Bands.AddRange(new Band[2] { Temsilci_Ozet, Temsilci_Gunluk_Hareket });
		Temsilciler.DataMember = "bolgeler.temsilciler";
		Temsilciler.DataSource = bindingSource1;
		Temsilciler.Level = 0;
		Temsilciler.Name = "Temsilciler";
		Temsilci_Ozet.Controls.AddRange(new XRControl[12]
		{
			xrLabel16, xrLabel17, xrLabel21, xrLabel22, xrLabel19, xrLabel20, xrTable27, xrTable26, xrLabel13, xrLabel14,
			xrLabel15, xrLabel10
		});
		Temsilci_Ozet.Expanded = false;
		Temsilci_Ozet.HeightF = 567.7082f;
		Temsilci_Ozet.Name = "Temsilci_Ozet";
		Temsilci_Ozet.PageBreak = PageBreak.AfterBand;
		xrLabel16.Font = new Font("Arial", 9.75f, FontStyle.Bold);
		xrLabel16.LocationFloat = new PointFloat(9.999943f, 108.875f);
		xrLabel16.Name = "xrLabel16";
		xrLabel16.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel16.SizeF = new SizeF(144.0127f, 18f);
		xrLabel16.StylePriority.UseFont = false;
		xrLabel16.StylePriority.UseTextAlignment = false;
		xrLabel16.Text = "Gün sayısı :";
		xrLabel16.TextAlignment = TextAlignment.MiddleRight;
		xrLabel17.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gun_sayisi", "{0:n0}")
		});
		xrLabel17.Font = new Font("Arial", 9.75f);
		xrLabel17.LocationFloat = new PointFloat(154.0128f, 108.875f);
		xrLabel17.Name = "xrLabel17";
		xrLabel17.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel17.SizeF = new SizeF(240.9986f, 18.00002f);
		xrLabel17.StylePriority.UseFont = false;
		xrLabel17.StylePriority.UseTextAlignment = false;
		xrLabel17.Text = "xrLabel4";
		xrLabel17.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel21.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.arac_bitis_km", "{0:n0}")
		});
		xrLabel21.Font = new Font("Arial", 9.75f);
		xrLabel21.LocationFloat = new PointFloat(154.0126f, 90.87499f);
		xrLabel21.Name = "xrLabel21";
		xrLabel21.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel21.SizeF = new SizeF(240.9986f, 18.00002f);
		xrLabel21.StylePriority.UseFont = false;
		xrLabel21.StylePriority.UseTextAlignment = false;
		xrLabel21.Text = "xrLabel4";
		xrLabel21.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel22.Font = new Font("Arial", 9.75f, FontStyle.Bold);
		xrLabel22.LocationFloat = new PointFloat(9.999943f, 90.87499f);
		xrLabel22.Name = "xrLabel22";
		xrLabel22.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel22.SizeF = new SizeF(144.0127f, 18f);
		xrLabel22.StylePriority.UseFont = false;
		xrLabel22.StylePriority.UseTextAlignment = false;
		xrLabel22.Text = "Bitiş araç km :";
		xrLabel22.TextAlignment = TextAlignment.MiddleRight;
		xrLabel19.Font = new Font("Arial", 9.75f, FontStyle.Bold);
		xrLabel19.LocationFloat = new PointFloat(10.00007f, 72.87499f);
		xrLabel19.Name = "xrLabel19";
		xrLabel19.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel19.SizeF = new SizeF(144.0127f, 18f);
		xrLabel19.StylePriority.UseFont = false;
		xrLabel19.StylePriority.UseTextAlignment = false;
		xrLabel19.Text = "Başlangıç araç km :";
		xrLabel19.TextAlignment = TextAlignment.MiddleRight;
		xrLabel20.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.arac_baslangic_km", "{0:n0}")
		});
		xrLabel20.Font = new Font("Arial", 9.75f);
		xrLabel20.LocationFloat = new PointFloat(154.0127f, 72.87496f);
		xrLabel20.Name = "xrLabel20";
		xrLabel20.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel20.SizeF = new SizeF(240.9986f, 18.00002f);
		xrLabel20.StylePriority.UseFont = false;
		xrLabel20.StylePriority.UseTextAlignment = false;
		xrLabel20.Text = "xrLabel4";
		xrLabel20.TextAlignment = TextAlignment.MiddleLeft;
		xrTable27.Borders = BorderSide.All;
		xrTable27.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable27.LocationFloat = new PointFloat(10.00007f, 346.2964f);
		xrTable27.Name = "xrTable27";
		xrTable27.Rows.AddRange(new XRTableRow[10] { xrTableRow47, xrTableRow68, xrTableRow58, xrTableRow59, xrTableRow60, xrTableRow61, xrTableRow62, xrTableRow63, xrTableRow64, xrTableRow65 });
		xrTable27.SizeF = new SizeF(614.0127f, 206.0183f);
		xrTable27.StylePriority.UseBorders = false;
		xrTable27.StylePriority.UseFont = false;
		xrTable27.StylePriority.UseTextAlignment = false;
		xrTable27.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow47.Cells.AddRange(new XRTableCell[4] { xrTableCell182, xrTableCell183, xrTableCell184, xrTableCell185 });
		xrTableRow47.Name = "xrTableRow47";
		xrTableRow47.Weight = 1.0;
		xrTableCell182.Borders = BorderSide.Right;
		xrTableCell182.Name = "xrTableCell182";
		xrTableCell182.StylePriority.UseBorders = false;
		xrTableCell182.Weight = 0.5110436273739111;
		xrTableCell183.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell183.Name = "xrTableCell183";
		xrTableCell183.StylePriority.UseFont = false;
		xrTableCell183.StylePriority.UseTextAlignment = false;
		xrTableCell183.Text = "Ziyaret başı";
		xrTableCell183.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell183.Weight = 0.3903462370563616;
		xrTableCell184.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell184.Name = "xrTableCell184";
		xrTableCell184.StylePriority.UseFont = false;
		xrTableCell184.StylePriority.UseTextAlignment = false;
		xrTableCell184.Text = "Günlük ortalama";
		xrTableCell184.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell184.Weight = 0.6387477790206574;
		xrTableCell185.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell185.Name = "xrTableCell185";
		xrTableCell185.StylePriority.UseFont = false;
		xrTableCell185.StylePriority.UseTextAlignment = false;
		xrTableCell185.Text = "Toplam";
		xrTableCell185.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell185.Weight = 0.6387482216275058;
		xrTableRow68.Cells.AddRange(new XRTableCell[6] { xrTableCell232, xrTableCell233, xrTableCell246, xrTableCell234, xrTableCell313, xrTableCell235 });
		xrTableRow68.Name = "xrTableRow68";
		xrTableRow68.Weight = 1.0;
		xrTableCell232.Borders = BorderSide.Right | BorderSide.Bottom;
		xrTableCell232.Name = "xrTableCell232";
		xrTableCell232.StylePriority.UseBorders = false;
		xrTableCell232.Weight = 0.5110433024898842;
		xrTableCell233.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell233.Name = "xrTableCell233";
		xrTableCell233.StylePriority.UseFont = false;
		xrTableCell233.StylePriority.UseTextAlignment = false;
		xrTableCell233.Text = "Tutar";
		xrTableCell233.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell233.Weight = 0.39034631159121425;
		xrTableCell246.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell246.Name = "xrTableCell246";
		xrTableCell246.StylePriority.UseFont = false;
		xrTableCell246.StylePriority.UseTextAlignment = false;
		xrTableCell246.Text = "Adet";
		xrTableCell246.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell246.Weight = 0.2484020234040516;
		xrTableCell234.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell234.Name = "xrTableCell234";
		xrTableCell234.StylePriority.UseFont = false;
		xrTableCell234.StylePriority.UseTextAlignment = false;
		xrTableCell234.Text = "Tutar";
		xrTableCell234.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell234.Weight = 0.3903460801319377;
		xrTableCell313.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell313.Name = "xrTableCell313";
		xrTableCell313.StylePriority.UseFont = false;
		xrTableCell313.StylePriority.UseTextAlignment = false;
		xrTableCell313.Text = "Adet";
		xrTableCell313.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell313.Weight = 0.2484020366833373;
		xrTableCell235.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell235.Name = "xrTableCell235";
		xrTableCell235.StylePriority.UseFont = false;
		xrTableCell235.StylePriority.UseTextAlignment = false;
		xrTableCell235.Text = "Tutar";
		xrTableCell235.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell235.Weight = 0.3903461107780112;
		xrTableRow58.Cells.AddRange(new XRTableCell[6] { xrTableCell186, xrTableCell187, xrTableCell245, xrTableCell188, xrTableCell315, xrTableCell189 });
		xrTableRow58.Name = "xrTableRow58";
		xrTableRow58.Weight = 1.0;
		xrTableCell186.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell186.Name = "xrTableCell186";
		xrTableCell186.StylePriority.UseFont = false;
		xrTableCell186.StylePriority.UseTextAlignment = false;
		xrTableCell186.Text = "Sipariş";
		xrTableCell186.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell186.Weight = 0.5110435642759916;
		xrTableCell187.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.ziyaret_basi_ortalama_siparis_tutari", "{0:c}")
		});
		xrTableCell187.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell187.Name = "xrTableCell187";
		xrTableCell187.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell187.StylePriority.UseFont = false;
		xrTableCell187.StylePriority.UsePadding = false;
		xrTableCell187.StylePriority.UseTextAlignment = false;
		xrTableCell187.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell187.Weight = 0.3903460377361928;
		xrTableCell245.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_siparis_adedi", "{0:n}")
		});
		xrTableCell245.Name = "xrTableCell245";
		xrTableCell245.StylePriority.UseTextAlignment = false;
		xrTableCell245.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell245.Weight = 0.24840202392499636;
		xrTableCell188.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_siparis_tutari", "{0:c}")
		});
		xrTableCell188.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell188.Name = "xrTableCell188";
		xrTableCell188.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell188.StylePriority.UseFont = false;
		xrTableCell188.StylePriority.UsePadding = false;
		xrTableCell188.StylePriority.UseTextAlignment = false;
		xrTableCell188.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell188.Weight = 0.3903460292887322;
		xrTableCell315.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_siparis_adedi", "{0:n0}")
		});
		xrTableCell315.Name = "xrTableCell315";
		xrTableCell315.StylePriority.UseTextAlignment = false;
		xrTableCell315.Text = "xrTableCell315";
		xrTableCell315.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell315.Weight = 0.24840204080525133;
		xrTableCell189.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_siparis_tutari", "{0:c}")
		});
		xrTableCell189.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell189.Name = "xrTableCell189";
		xrTableCell189.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell189.StylePriority.UseFont = false;
		xrTableCell189.StylePriority.UsePadding = false;
		xrTableCell189.StylePriority.UseTextAlignment = false;
		xrTableCell189.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell189.Weight = 0.3903461690472716;
		xrTableRow59.Cells.AddRange(new XRTableCell[6] { xrTableCell190, xrTableCell191, xrTableCell247, xrTableCell192, xrTableCell316, xrTableCell193 });
		xrTableRow59.Name = "xrTableRow59";
		xrTableRow59.Weight = 1.0;
		xrTableCell190.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell190.Name = "xrTableCell190";
		xrTableCell190.StylePriority.UseFont = false;
		xrTableCell190.StylePriority.UseTextAlignment = false;
		xrTableCell190.Text = "Fatura";
		xrTableCell190.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell190.Weight = 0.5110435642759916;
		xrTableCell191.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.ziyaret_basi_ortalama_fatura_tutari", "{0:c}")
		});
		xrTableCell191.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell191.Name = "xrTableCell191";
		xrTableCell191.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell191.StylePriority.UseFont = false;
		xrTableCell191.StylePriority.UsePadding = false;
		xrTableCell191.StylePriority.UseTextAlignment = false;
		xrTableCell191.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell191.Weight = 0.3903460918835394;
		xrTableCell247.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_fatura_adedi", "{0:n}")
		});
		xrTableCell247.Name = "xrTableCell247";
		xrTableCell247.StylePriority.UseTextAlignment = false;
		xrTableCell247.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell247.Weight = 0.24840202392499636;
		xrTableCell192.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_fatura_tutari", "{0:c}")
		});
		xrTableCell192.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell192.Name = "xrTableCell192";
		xrTableCell192.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell192.StylePriority.UseFont = false;
		xrTableCell192.StylePriority.UsePadding = false;
		xrTableCell192.StylePriority.UseTextAlignment = false;
		xrTableCell192.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell192.Weight = 0.3903460292887322;
		xrTableCell316.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_fatura_adedi", "{0:n0}")
		});
		xrTableCell316.Name = "xrTableCell316";
		xrTableCell316.StylePriority.UseTextAlignment = false;
		xrTableCell316.Text = "xrTableCell316";
		xrTableCell316.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell316.Weight = 0.24840204080525133;
		xrTableCell193.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_fatura_tutari", "{0:c}")
		});
		xrTableCell193.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell193.Name = "xrTableCell193";
		xrTableCell193.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell193.StylePriority.UseFont = false;
		xrTableCell193.StylePriority.UsePadding = false;
		xrTableCell193.StylePriority.UseTextAlignment = false;
		xrTableCell193.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell193.Weight = 0.3903461148999251;
		xrTableRow60.Cells.AddRange(new XRTableCell[6] { xrTableCell194, xrTableCell195, xrTableCell248, xrTableCell196, xrTableCell312, xrTableCell197 });
		xrTableRow60.Name = "xrTableRow60";
		xrTableRow60.Weight = 1.0;
		xrTableCell194.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell194.Name = "xrTableCell194";
		xrTableCell194.StylePriority.UseFont = false;
		xrTableCell194.StylePriority.UseTextAlignment = false;
		xrTableCell194.Text = "Nakit tahsilat";
		xrTableCell194.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell194.Weight = 0.5110435101286539;
		xrTableCell195.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.ziyaret_basi_ortalama_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell195.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell195.Name = "xrTableCell195";
		xrTableCell195.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell195.StylePriority.UseFont = false;
		xrTableCell195.StylePriority.UsePadding = false;
		xrTableCell195.StylePriority.UseTextAlignment = false;
		xrTableCell195.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell195.Weight = 0.39034641676761833;
		xrTableCell248.Name = "xrTableCell248";
		xrTableCell248.StylePriority.UseTextAlignment = false;
		xrTableCell248.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell248.Weight = 0.24840202392499627;
		xrTableCell196.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell196.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell196.Name = "xrTableCell196";
		xrTableCell196.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell196.StylePriority.UseFont = false;
		xrTableCell196.StylePriority.UsePadding = false;
		xrTableCell196.StylePriority.UseTextAlignment = false;
		xrTableCell196.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell196.Weight = 0.39034602928873235;
		xrTableCell312.Name = "xrTableCell312";
		xrTableCell312.StylePriority.UseTextAlignment = false;
		xrTableCell312.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell312.Weight = 0.24840204080524692;
		xrTableCell197.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell197.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell197.Name = "xrTableCell197";
		xrTableCell197.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell197.StylePriority.UseFont = false;
		xrTableCell197.StylePriority.UsePadding = false;
		xrTableCell197.StylePriority.UseTextAlignment = false;
		xrTableCell197.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell197.Weight = 0.3903458441631882;
		xrTableRow61.Cells.AddRange(new XRTableCell[6] { xrTableCell198, xrTableCell199, xrTableCell249, xrTableCell200, xrTableCell317, xrTableCell201 });
		xrTableRow61.Name = "xrTableRow61";
		xrTableRow61.Weight = 1.0;
		xrTableCell198.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell198.Name = "xrTableCell198";
		xrTableCell198.StylePriority.UseFont = false;
		xrTableCell198.StylePriority.UseTextAlignment = false;
		xrTableCell198.Text = "Çek tahsilat";
		xrTableCell198.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell198.Weight = 0.5110435642759916;
		xrTableCell199.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.ziyaret_basi_ortalama_cek_tutari", "{0:c}")
		});
		xrTableCell199.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell199.Name = "xrTableCell199";
		xrTableCell199.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell199.StylePriority.UseFont = false;
		xrTableCell199.StylePriority.UsePadding = false;
		xrTableCell199.StylePriority.UseTextAlignment = false;
		xrTableCell199.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell199.Weight = 0.39034641676761817;
		xrTableCell249.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_cek_adedi", "{0:n}")
		});
		xrTableCell249.Name = "xrTableCell249";
		xrTableCell249.StylePriority.UseTextAlignment = false;
		xrTableCell249.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell249.Weight = 0.24840202392499633;
		xrTableCell200.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_cek_tutari", "{0:c}")
		});
		xrTableCell200.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell200.Name = "xrTableCell200";
		xrTableCell200.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell200.StylePriority.UseFont = false;
		xrTableCell200.StylePriority.UsePadding = false;
		xrTableCell200.StylePriority.UseTextAlignment = false;
		xrTableCell200.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell200.Weight = 0.3903460292887323;
		xrTableCell317.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_cek_adedi", "{0:n0}")
		});
		xrTableCell317.Name = "xrTableCell317";
		xrTableCell317.StylePriority.UseTextAlignment = false;
		xrTableCell317.Text = "xrTableCell317";
		xrTableCell317.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell317.Weight = 0.24840204080525133;
		xrTableCell201.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_cek_tutari", "{0:c}")
		});
		xrTableCell201.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell201.Name = "xrTableCell201";
		xrTableCell201.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell201.StylePriority.UseFont = false;
		xrTableCell201.StylePriority.UsePadding = false;
		xrTableCell201.StylePriority.UseTextAlignment = false;
		xrTableCell201.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell201.Weight = 0.3903457900158462;
		xrTableRow62.Cells.AddRange(new XRTableCell[6] { xrTableCell202, xrTableCell203, xrTableCell250, xrTableCell218, xrTableCell314, xrTableCell219 });
		xrTableRow62.Name = "xrTableRow62";
		xrTableRow62.Weight = 1.0;
		xrTableCell202.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell202.Name = "xrTableCell202";
		xrTableCell202.StylePriority.UseFont = false;
		xrTableCell202.StylePriority.UseTextAlignment = false;
		xrTableCell202.Text = "Kredi kartı tahsilat";
		xrTableCell202.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell202.Weight = 0.5110432393919647;
		xrTableCell203.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.ziyaret_basi_ortalama_kk_tutari", "{0:c}")
		});
		xrTableCell203.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell203.Name = "xrTableCell203";
		xrTableCell203.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell203.StylePriority.UseFont = false;
		xrTableCell203.StylePriority.UsePadding = false;
		xrTableCell203.StylePriority.UseTextAlignment = false;
		xrTableCell203.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell203.Weight = 0.3903467416516972;
		xrTableCell250.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_kk_adedi", "{0:n}")
		});
		xrTableCell250.Name = "xrTableCell250";
		xrTableCell250.StylePriority.UseTextAlignment = false;
		xrTableCell250.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell250.Weight = 0.24840202392499633;
		xrTableCell218.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_kk_tutari", "{0:c}")
		});
		xrTableCell218.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell218.Name = "xrTableCell218";
		xrTableCell218.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell218.StylePriority.UseFont = false;
		xrTableCell218.StylePriority.UsePadding = false;
		xrTableCell218.StylePriority.UseTextAlignment = false;
		xrTableCell218.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell218.Weight = 0.39034602928873224;
		xrTableCell314.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_kk_adedi", "{0:n0}")
		});
		xrTableCell314.Name = "xrTableCell314";
		xrTableCell314.StylePriority.UseTextAlignment = false;
		xrTableCell314.Text = "xrTableCell314";
		xrTableCell314.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell314.Weight = 0.24840204080522538;
		xrTableCell219.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_kk_tutari", "{0:c}")
		});
		xrTableCell219.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell219.Name = "xrTableCell219";
		xrTableCell219.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell219.StylePriority.UseFont = false;
		xrTableCell219.StylePriority.UsePadding = false;
		xrTableCell219.StylePriority.UseTextAlignment = false;
		xrTableCell219.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell219.Weight = 0.39034579001582026;
		xrTableRow63.Cells.AddRange(new XRTableCell[6] { xrTableCell220, xrTableCell221, xrTableCell251, xrTableCell222, xrTableCell318, xrTableCell223 });
		xrTableRow63.Name = "xrTableRow63";
		xrTableRow63.Weight = 1.0;
		xrTableCell220.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell220.Name = "xrTableCell220";
		xrTableCell220.StylePriority.UseFont = false;
		xrTableCell220.StylePriority.UseTextAlignment = false;
		xrTableCell220.Text = "Toplam tahsilat";
		xrTableCell220.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell220.Weight = 0.5110435642759916;
		xrTableCell221.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.ziyaret_basi_ortalama_tahsilat_tutari", "{0:c}")
		});
		xrTableCell221.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell221.Name = "xrTableCell221";
		xrTableCell221.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell221.StylePriority.UseFont = false;
		xrTableCell221.StylePriority.UsePadding = false;
		xrTableCell221.StylePriority.UseTextAlignment = false;
		xrTableCell221.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell221.Weight = 0.3903460918835394;
		xrTableCell251.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_tahsilat_evrak_adedi", "{0:n}")
		});
		xrTableCell251.Name = "xrTableCell251";
		xrTableCell251.StylePriority.UseTextAlignment = false;
		xrTableCell251.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell251.Weight = 0.24840202392499636;
		xrTableCell222.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_tahsilat_tutari", "{0:c}")
		});
		xrTableCell222.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell222.Name = "xrTableCell222";
		xrTableCell222.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell222.StylePriority.UseFont = false;
		xrTableCell222.StylePriority.UsePadding = false;
		xrTableCell222.StylePriority.UseTextAlignment = false;
		xrTableCell222.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell222.Weight = 0.3903460292887322;
		xrTableCell318.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_tahsilat_evrak_adedi", "{0:n0}")
		});
		xrTableCell318.Name = "xrTableCell318";
		xrTableCell318.StylePriority.UseTextAlignment = false;
		xrTableCell318.Text = "xrTableCell318";
		xrTableCell318.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell318.Weight = 0.24840204080525133;
		xrTableCell223.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_tahsilat_tutari", "{0:c}")
		});
		xrTableCell223.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell223.Name = "xrTableCell223";
		xrTableCell223.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell223.StylePriority.UseFont = false;
		xrTableCell223.StylePriority.UsePadding = false;
		xrTableCell223.StylePriority.UseTextAlignment = false;
		xrTableCell223.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell223.Weight = 0.3903461148999251;
		xrTableRow64.Cells.AddRange(new XRTableCell[6] { xrTableCell224, xrTableCell225, xrTableCell252, xrTableCell226, xrTableCell319, xrTableCell227 });
		xrTableRow64.Name = "xrTableRow64";
		xrTableRow64.Weight = 1.0;
		xrTableCell224.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell224.Name = "xrTableCell224";
		xrTableCell224.StylePriority.UseFont = false;
		xrTableCell224.StylePriority.UseTextAlignment = false;
		xrTableCell224.Text = "Masraf";
		xrTableCell224.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell224.Weight = 0.5110435642759916;
		xrTableCell225.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell225.Name = "xrTableCell225";
		xrTableCell225.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell225.StylePriority.UseFont = false;
		xrTableCell225.StylePriority.UsePadding = false;
		xrTableCell225.StylePriority.UseTextAlignment = false;
		xrTableCell225.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell225.Weight = 0.3903460918835393;
		xrTableCell252.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_masraf_adedi", "{0:n}")
		});
		xrTableCell252.Name = "xrTableCell252";
		xrTableCell252.StylePriority.UseTextAlignment = false;
		xrTableCell252.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell252.Weight = 0.24840202392499636;
		xrTableCell226.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_masraf_tutari", "{0:c}")
		});
		xrTableCell226.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell226.Name = "xrTableCell226";
		xrTableCell226.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell226.StylePriority.UseFont = false;
		xrTableCell226.StylePriority.UsePadding = false;
		xrTableCell226.StylePriority.UseTextAlignment = false;
		xrTableCell226.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell226.Weight = 0.3903460292887322;
		xrTableCell319.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_masraf_adedi", "{0:n0}")
		});
		xrTableCell319.Name = "xrTableCell319";
		xrTableCell319.StylePriority.UseTextAlignment = false;
		xrTableCell319.Text = "xrTableCell319";
		xrTableCell319.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell319.Weight = 0.24840204080525133;
		xrTableCell227.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_masraf_tutari", "{0:c}")
		});
		xrTableCell227.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell227.Name = "xrTableCell227";
		xrTableCell227.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell227.StylePriority.UseFont = false;
		xrTableCell227.StylePriority.UsePadding = false;
		xrTableCell227.StylePriority.UseTextAlignment = false;
		xrTableCell227.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell227.Weight = 0.3903461148999251;
		xrTableRow65.Cells.AddRange(new XRTableCell[6] { xrTableCell228, xrTableCell229, xrTableCell253, xrTableCell230, xrTableCell320, xrTableCell231 });
		xrTableRow65.Name = "xrTableRow65";
		xrTableRow65.Weight = 1.0;
		xrTableCell228.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell228.Name = "xrTableCell228";
		xrTableCell228.StylePriority.UseFont = false;
		xrTableCell228.StylePriority.UseTextAlignment = false;
		xrTableCell228.Text = "Kalan nakit tutar";
		xrTableCell228.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell228.Weight = 0.5110435642759916;
		xrTableCell229.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell229.Name = "xrTableCell229";
		xrTableCell229.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell229.StylePriority.UseFont = false;
		xrTableCell229.StylePriority.UsePadding = false;
		xrTableCell229.StylePriority.UseTextAlignment = false;
		xrTableCell229.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell229.Weight = 0.3903460918835394;
		xrTableCell253.Name = "xrTableCell253";
		xrTableCell253.StylePriority.UseTextAlignment = false;
		xrTableCell253.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell253.Weight = 0.24840202392499636;
		xrTableCell230.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_kalan_nakit_tutar", "{0:c}")
		});
		xrTableCell230.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell230.Name = "xrTableCell230";
		xrTableCell230.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell230.StylePriority.UseFont = false;
		xrTableCell230.StylePriority.UsePadding = false;
		xrTableCell230.StylePriority.UseTextAlignment = false;
		xrTableCell230.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell230.Weight = 0.3903460292887322;
		xrTableCell320.Name = "xrTableCell320";
		xrTableCell320.StylePriority.UseTextAlignment = false;
		xrTableCell320.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell320.Weight = 0.24840204080525133;
		xrTableCell231.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_kalan_nakit_tutar", "{0:c}")
		});
		xrTableCell231.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell231.Name = "xrTableCell231";
		xrTableCell231.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell231.StylePriority.UseFont = false;
		xrTableCell231.StylePriority.UsePadding = false;
		xrTableCell231.StylePriority.UseTextAlignment = false;
		xrTableCell231.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell231.Weight = 0.3903461148999251;
		xrTable26.Borders = BorderSide.All;
		xrTable26.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable26.LocationFloat = new PointFloat(10.00007f, 140.625f);
		xrTable26.Name = "xrTable26";
		xrTable26.Rows.AddRange(new XRTableRow[9] { xrTableRow51, xrTableRow52, xrTableRow53, xrTableRow54, xrTableRow55, xrTableRow56, xrTableRow57, xrTableRow66, xrTableRow67 });
		xrTable26.SizeF = new SizeF(474.0128f, 185.4165f);
		xrTable26.StylePriority.UseBorders = false;
		xrTable26.StylePriority.UseFont = false;
		xrTable26.StylePriority.UseTextAlignment = false;
		xrTable26.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow51.Cells.AddRange(new XRTableCell[4] { xrTableCell161, xrTableCell162, xrTableCell163, xrTableCell160 });
		xrTableRow51.Name = "xrTableRow51";
		xrTableRow51.Weight = 1.0;
		xrTableCell161.Borders = BorderSide.Right | BorderSide.Bottom;
		xrTableCell161.Name = "xrTableCell161";
		xrTableCell161.StylePriority.UseBorders = false;
		xrTableCell161.Weight = 0.6784201418764519;
		xrTableCell162.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell162.Name = "xrTableCell162";
		xrTableCell162.StylePriority.UseFont = false;
		xrTableCell162.StylePriority.UseTextAlignment = false;
		xrTableCell162.Text = "Ziyaret başı";
		xrTableCell162.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell162.Weight = 0.518192067219189;
		xrTableCell163.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell163.Name = "xrTableCell163";
		xrTableCell163.StylePriority.UseFont = false;
		xrTableCell163.StylePriority.UseTextAlignment = false;
		xrTableCell163.Text = "Günlük ortalama";
		xrTableCell163.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell163.Weight = 0.5181920571215578;
		xrTableCell160.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell160.Name = "xrTableCell160";
		xrTableCell160.StylePriority.UseFont = false;
		xrTableCell160.StylePriority.UseTextAlignment = false;
		xrTableCell160.Text = "Toplam";
		xrTableCell160.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell160.Weight = 0.5181924841667924;
		xrTableRow52.Cells.AddRange(new XRTableCell[4] { xrTableCell164, xrTableCell165, xrTableCell166, xrTableCell210 });
		xrTableRow52.Name = "xrTableRow52";
		xrTableRow52.Weight = 1.0;
		xrTableCell164.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell164.Name = "xrTableCell164";
		xrTableCell164.StylePriority.UseFont = false;
		xrTableCell164.StylePriority.UseTextAlignment = false;
		xrTableCell164.Text = "Yapılan km";
		xrTableCell164.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell164.Weight = 0.6784205119572351;
		xrTableCell165.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.ziyaret_basi_ortalama_yapilan_km", "{0:n0}")
		});
		xrTableCell165.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell165.Name = "xrTableCell165";
		xrTableCell165.StylePriority.UseFont = false;
		xrTableCell165.StylePriority.UseTextAlignment = false;
		xrTableCell165.Text = "xrTableCell29";
		xrTableCell165.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell165.Weight = 0.5181920678239835;
		xrTableCell166.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_yapilan_km", "{0:n0}")
		});
		xrTableCell166.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell166.Name = "xrTableCell166";
		xrTableCell166.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell166.StylePriority.UseFont = false;
		xrTableCell166.StylePriority.UsePadding = false;
		xrTableCell166.StylePriority.UseTextAlignment = false;
		xrTableCell166.Text = "xrTableCell30";
		xrTableCell166.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell166.Weight = 0.5181920459955063;
		xrTableCell210.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_yapilan_km", "{0:n0}")
		});
		xrTableCell210.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell210.Name = "xrTableCell210";
		xrTableCell210.StylePriority.UseFont = false;
		xrTableCell210.StylePriority.UseTextAlignment = false;
		xrTableCell210.Text = "xrTableCell210";
		xrTableCell210.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell210.Weight = 0.5181921246072665;
		xrTableRow53.Cells.AddRange(new XRTableCell[4] { xrTableCell167, xrTableCell168, xrTableCell169, xrTableCell211 });
		xrTableRow53.Name = "xrTableRow53";
		xrTableRow53.Weight = 1.0;
		xrTableCell167.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell167.Name = "xrTableCell167";
		xrTableCell167.StylePriority.UseFont = false;
		xrTableCell167.StylePriority.UseTextAlignment = false;
		xrTableCell167.Text = "Mesai süresi";
		xrTableCell167.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell167.Weight = 0.6784205119572351;
		xrTableCell168.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell168.Name = "xrTableCell168";
		xrTableCell168.StylePriority.UseFont = false;
		xrTableCell168.StylePriority.UseTextAlignment = false;
		xrTableCell168.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell168.Weight = 0.5181920678239835;
		xrTableCell169.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_mesai_suresi")
		});
		xrTableCell169.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell169.Name = "xrTableCell169";
		xrTableCell169.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell169.StylePriority.UseFont = false;
		xrTableCell169.StylePriority.UsePadding = false;
		xrTableCell169.StylePriority.UseTextAlignment = false;
		xrTableCell169.Text = "xrTableCell21";
		xrTableCell169.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell169.Weight = 0.5181920459955063;
		xrTableCell211.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_mesai_suresi")
		});
		xrTableCell211.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell211.Name = "xrTableCell211";
		xrTableCell211.StylePriority.UseFont = false;
		xrTableCell211.StylePriority.UseTextAlignment = false;
		xrTableCell211.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell211.Weight = 0.5181921246072665;
		xrTableRow54.Cells.AddRange(new XRTableCell[4] { xrTableCell170, xrTableCell171, xrTableCell172, xrTableCell212 });
		xrTableRow54.Name = "xrTableRow54";
		xrTableRow54.Weight = 1.0;
		xrTableCell170.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell170.Name = "xrTableCell170";
		xrTableCell170.StylePriority.UseFont = false;
		xrTableCell170.StylePriority.UseTextAlignment = false;
		xrTableCell170.Text = "Ziyaret süresi";
		xrTableCell170.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell170.Weight = 0.6784205119572351;
		xrTableCell171.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.ziyaret_basi_ortalama_ziyaret_suresi")
		});
		xrTableCell171.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell171.Name = "xrTableCell171";
		xrTableCell171.StylePriority.UseFont = false;
		xrTableCell171.StylePriority.UseTextAlignment = false;
		xrTableCell171.Text = "xrTableCell171";
		xrTableCell171.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell171.Weight = 0.5181920678239835;
		xrTableCell172.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_ziyaret_suresi")
		});
		xrTableCell172.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell172.Name = "xrTableCell172";
		xrTableCell172.StylePriority.UseFont = false;
		xrTableCell172.StylePriority.UseTextAlignment = false;
		xrTableCell172.Text = "xrTableCell172";
		xrTableCell172.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell172.Weight = 0.5181920459955063;
		xrTableCell212.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_ziyaret_suresi")
		});
		xrTableCell212.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell212.Name = "xrTableCell212";
		xrTableCell212.StylePriority.UseFont = false;
		xrTableCell212.StylePriority.UseTextAlignment = false;
		xrTableCell212.Text = "xrTableCell212";
		xrTableCell212.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell212.Weight = 0.5181921246072665;
		xrTableRow55.Cells.AddRange(new XRTableCell[4] { xrTableCell173, xrTableCell174, xrTableCell175, xrTableCell213 });
		xrTableRow55.Name = "xrTableRow55";
		xrTableRow55.Weight = 1.0;
		xrTableCell173.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell173.Name = "xrTableCell173";
		xrTableCell173.StylePriority.UseFont = false;
		xrTableCell173.StylePriority.UseTextAlignment = false;
		xrTableCell173.Text = "Ulaşım süresi";
		xrTableCell173.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell173.Weight = 0.6784205119572351;
		xrTableCell174.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.ziyaret_basi_ortalama_ulasim_suresi")
		});
		xrTableCell174.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell174.Name = "xrTableCell174";
		xrTableCell174.StylePriority.UseFont = false;
		xrTableCell174.StylePriority.UseTextAlignment = false;
		xrTableCell174.Text = "xrTableCell174";
		xrTableCell174.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell174.Weight = 0.5181920678239835;
		xrTableCell175.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_ulasim_suresi")
		});
		xrTableCell175.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell175.Name = "xrTableCell175";
		xrTableCell175.StylePriority.UseFont = false;
		xrTableCell175.StylePriority.UseTextAlignment = false;
		xrTableCell175.Text = "xrTableCell175";
		xrTableCell175.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell175.Weight = 0.5181920459955063;
		xrTableCell213.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_ulasim_suresi")
		});
		xrTableCell213.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell213.Name = "xrTableCell213";
		xrTableCell213.StylePriority.UseFont = false;
		xrTableCell213.StylePriority.UseTextAlignment = false;
		xrTableCell213.Text = "xrTableCell213";
		xrTableCell213.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell213.Weight = 0.5181921246072665;
		xrTableRow56.Cells.AddRange(new XRTableCell[4] { xrTableCell176, xrTableCell177, xrTableCell178, xrTableCell214 });
		xrTableRow56.Name = "xrTableRow56";
		xrTableRow56.Weight = 1.0;
		xrTableCell176.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell176.Name = "xrTableCell176";
		xrTableCell176.StylePriority.UseFont = false;
		xrTableCell176.StylePriority.UseTextAlignment = false;
		xrTableCell176.Text = "Hedef ziyaret";
		xrTableCell176.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell176.Weight = 0.6784205119572351;
		xrTableCell177.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell177.Name = "xrTableCell177";
		xrTableCell177.StylePriority.UseFont = false;
		xrTableCell177.StylePriority.UseTextAlignment = false;
		xrTableCell177.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell177.Weight = 0.5181920678239835;
		xrTableCell178.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_hedef_ziyaret", "{0:n}")
		});
		xrTableCell178.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell178.Name = "xrTableCell178";
		xrTableCell178.StylePriority.UseFont = false;
		xrTableCell178.StylePriority.UseTextAlignment = false;
		xrTableCell178.Text = "xrTableCell178";
		xrTableCell178.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell178.Weight = 0.5181920459955063;
		xrTableCell214.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_hedef_ziyaret")
		});
		xrTableCell214.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell214.Name = "xrTableCell214";
		xrTableCell214.StylePriority.UseFont = false;
		xrTableCell214.StylePriority.UseTextAlignment = false;
		xrTableCell214.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell214.Weight = 0.5181921246072665;
		xrTableRow57.Cells.AddRange(new XRTableCell[4] { xrTableCell179, xrTableCell180, xrTableCell181, xrTableCell215 });
		xrTableRow57.Name = "xrTableRow57";
		xrTableRow57.Weight = 1.0;
		xrTableCell179.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell179.Name = "xrTableCell179";
		xrTableCell179.StylePriority.UseFont = false;
		xrTableCell179.StylePriority.UseTextAlignment = false;
		xrTableCell179.Text = "Yapılan ziyaret";
		xrTableCell179.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell179.Weight = 0.6784205119572351;
		xrTableCell180.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell180.Name = "xrTableCell180";
		xrTableCell180.StylePriority.UseFont = false;
		xrTableCell180.StylePriority.UseTextAlignment = false;
		xrTableCell180.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell180.Weight = 0.5181920678239835;
		xrTableCell181.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_yapilan_ziyaret", "{0:n}")
		});
		xrTableCell181.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell181.Name = "xrTableCell181";
		xrTableCell181.StylePriority.UseFont = false;
		xrTableCell181.StylePriority.UseTextAlignment = false;
		xrTableCell181.Text = "xrTableCell181";
		xrTableCell181.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell181.Weight = 0.5181920459955063;
		xrTableCell215.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_yapilan_ziyaret")
		});
		xrTableCell215.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell215.Name = "xrTableCell215";
		xrTableCell215.StylePriority.UseFont = false;
		xrTableCell215.StylePriority.UseTextAlignment = false;
		xrTableCell215.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell215.Weight = 0.5181921246072665;
		xrTableRow66.Cells.AddRange(new XRTableCell[4] { xrTableCell204, xrTableCell205, xrTableCell206, xrTableCell216 });
		xrTableRow66.Name = "xrTableRow66";
		xrTableRow66.Weight = 1.0;
		xrTableCell204.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell204.Name = "xrTableCell204";
		xrTableCell204.StylePriority.UseFont = false;
		xrTableCell204.StylePriority.UseTextAlignment = false;
		xrTableCell204.Text = "Yapılmayan ziyaret";
		xrTableCell204.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell204.Weight = 0.6784205119572351;
		xrTableCell205.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell205.Name = "xrTableCell205";
		xrTableCell205.StylePriority.UseFont = false;
		xrTableCell205.StylePriority.UseTextAlignment = false;
		xrTableCell205.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell205.Weight = 0.5181920678239835;
		xrTableCell206.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_yapilmayan_ziyaret", "{0:n}")
		});
		xrTableCell206.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell206.Name = "xrTableCell206";
		xrTableCell206.StylePriority.UseFont = false;
		xrTableCell206.StylePriority.UseTextAlignment = false;
		xrTableCell206.Text = "xrTableCell206";
		xrTableCell206.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell206.Weight = 0.5181920459955063;
		xrTableCell216.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_yapilmayan_ziyaret")
		});
		xrTableCell216.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell216.Name = "xrTableCell216";
		xrTableCell216.StylePriority.UseFont = false;
		xrTableCell216.StylePriority.UseTextAlignment = false;
		xrTableCell216.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell216.Weight = 0.5181921246072665;
		xrTableRow67.Cells.AddRange(new XRTableCell[4] { xrTableCell207, xrTableCell208, xrTableCell209, xrTableCell217 });
		xrTableRow67.Name = "xrTableRow67";
		xrTableRow67.Weight = 1.0;
		xrTableCell207.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell207.Name = "xrTableCell207";
		xrTableCell207.StylePriority.UseFont = false;
		xrTableCell207.StylePriority.UseTextAlignment = false;
		xrTableCell207.Text = "Rota dışı ziyaret";
		xrTableCell207.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell207.Weight = 0.6784205119572351;
		xrTableCell208.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell208.Name = "xrTableCell208";
		xrTableCell208.StylePriority.UseFont = false;
		xrTableCell208.StylePriority.UseTextAlignment = false;
		xrTableCell208.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell208.Weight = 0.5181920678239835;
		xrTableCell209.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunluk_ortalama_rota_disi_ziyaret", "{0:n}")
		});
		xrTableCell209.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell209.Name = "xrTableCell209";
		xrTableCell209.StylePriority.UseFont = false;
		xrTableCell209.StylePriority.UseTextAlignment = false;
		xrTableCell209.Text = "xrTableCell209";
		xrTableCell209.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell209.Weight = 0.5181920459955063;
		xrTableCell217.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.toplam_rota_disi_ziyaret")
		});
		xrTableCell217.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		xrTableCell217.Name = "xrTableCell217";
		xrTableCell217.StylePriority.UseFont = false;
		xrTableCell217.StylePriority.UseTextAlignment = false;
		xrTableCell217.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell217.Weight = 0.5181921246072665;
		xrLabel13.Font = new Font("Arial", 12f, FontStyle.Bold);
		xrLabel13.LocationFloat = new PointFloat(557.4435f, 34.45832f);
		xrLabel13.Name = "xrLabel13";
		xrLabel13.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel13.SizeF = new SizeF(221.5566f, 23f);
		xrLabel13.StylePriority.UseFont = false;
		xrLabel13.StylePriority.UseTextAlignment = false;
		xrLabel13.Text = "[baslangic_tarihi!dd.MM.yyyy] - [bitis_tarihi!dd.MM.yyyy]";
		xrLabel13.TextAlignment = TextAlignment.MiddleRight;
		xrLabel14.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.temsilci_adi")
		});
		xrLabel14.Font = new Font("Arial", 12f, FontStyle.Bold);
		xrLabel14.LocationFloat = new PointFloat(252.4141f, 34.45832f);
		xrLabel14.Name = "xrLabel14";
		xrLabel14.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel14.SizeF = new SizeF(288.6692f, 23f);
		xrLabel14.StylePriority.UseFont = false;
		xrLabel14.StylePriority.UseTextAlignment = false;
		xrLabel14.Text = "xrLabel4";
		xrLabel14.TextAlignment = TextAlignment.MiddleCenter;
		xrLabel15.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.bolge_adi")
		});
		xrLabel15.Font = new Font("Arial", 12f, FontStyle.Bold);
		xrLabel15.LocationFloat = new PointFloat(10.00001f, 34.45832f);
		xrLabel15.Name = "xrLabel15";
		xrLabel15.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel15.SizeF = new SizeF(168.8776f, 23f);
		xrLabel15.StylePriority.UseFont = false;
		xrLabel15.StylePriority.UseTextAlignment = false;
		xrLabel15.Text = "xrLabel4";
		xrLabel15.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel10.Font = new Font("Arial", 12f, FontStyle.Bold | FontStyle.Underline);
		xrLabel10.LocationFloat = new PointFloat(0f, 0f);
		xrLabel10.Name = "xrLabel10";
		xrLabel10.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel10.SizeF = new SizeF(789f, 23f);
		xrLabel10.StylePriority.UseFont = false;
		xrLabel10.StylePriority.UseTextAlignment = false;
		xrLabel10.Text = "TEMSİLCİ ÖZETİ";
		xrLabel10.TextAlignment = TextAlignment.MiddleCenter;
		Temsilci_Gunluk_Hareket.Bands.AddRange(new Band[10] { Gun_Ozeti, Ziyaret_Listesi, Hedef_Ziyaretler, Yapilmayan_Ziyaretler, Yapilan_Ziyaretler, Rota_Disi_Ziyaretler, Siparisler, Faturalar, Tahsilatlar, Masraflar });
		Temsilci_Gunluk_Hareket.DataMember = "bolgeler.temsilciler.gunler";
		Temsilci_Gunluk_Hareket.DataSource = bindingSource1;
		Temsilci_Gunluk_Hareket.Level = 0;
		Temsilci_Gunluk_Hareket.Name = "Temsilci_Gunluk_Hareket";
		Temsilci_Gunluk_Hareket.PageBreak = PageBreak.AfterBand;
		Gun_Ozeti.Controls.AddRange(new XRControl[15]
		{
			xrLabel2, xrTable22, xrTable23, xrTable21, xrTable20, xrTable17, xrTable16, xrLabel35, xrLabel37, xrLabel39,
			xrLabel36, xrTable3, xrLabel1, xrLabel38, xrLabel7
		});
		Gun_Ozeti.Expanded = false;
		Gun_Ozeti.HeightF = 314.3517f;
		Gun_Ozeti.Name = "Gun_Ozeti";
		Gun_Ozeti.PageBreak = PageBreak.BeforeBand;
		xrLabel2.Font = new Font("Arial", 12f, FontStyle.Bold | FontStyle.Underline);
		xrLabel2.LocationFloat = new PointFloat(0f, 0f);
		xrLabel2.Name = "xrLabel2";
		xrLabel2.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel2.SizeF = new SizeF(789f, 23f);
		xrLabel2.StylePriority.UseFont = false;
		xrLabel2.StylePriority.UseTextAlignment = false;
		xrLabel2.Text = "GÜNLÜK TEMSİLCİ RAPORU";
		xrLabel2.TextAlignment = TextAlignment.MiddleCenter;
		xrTable22.Borders = BorderSide.All;
		xrTable22.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable22.LocationFloat = new PointFloat(283.8188f, 179.9121f);
		xrTable22.Name = "xrTable22";
		xrTable22.Rows.AddRange(new XRTableRow[4] { xrTableRow24, xrTableRow40, xrTableRow43, xrTableRow44 });
		xrTable22.SizeF = new SizeF(262.6276f, 82.40733f);
		xrTable22.StylePriority.UseBorders = false;
		xrTable22.StylePriority.UseFont = false;
		xrTable22.StylePriority.UseTextAlignment = false;
		xrTable22.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow24.Cells.AddRange(new XRTableCell[1] { xrTableCell33 });
		xrTableRow24.Name = "xrTableRow24";
		xrTableRow24.Weight = 1.0;
		xrTableCell33.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell33.Name = "xrTableCell33";
		xrTableCell33.StylePriority.UseFont = false;
		xrTableCell33.StylePriority.UseTextAlignment = false;
		xrTableCell33.Text = "Sipariş & Fatura";
		xrTableCell33.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell33.Weight = 2.225490291819853;
		xrTableRow40.Cells.AddRange(new XRTableCell[3] { xrTableCell124, xrTableCell136, xrTableCell141 });
		xrTableRow40.Name = "xrTableRow40";
		xrTableRow40.Weight = 1.0;
		xrTableCell124.Borders = BorderSide.All;
		xrTableCell124.Name = "xrTableCell124";
		xrTableCell124.StylePriority.UseBorders = false;
		xrTableCell124.Weight = 0.9999997127757353;
		xrTableCell136.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell136.Name = "xrTableCell136";
		xrTableCell136.StylePriority.UseFont = false;
		xrTableCell136.StylePriority.UseTextAlignment = false;
		xrTableCell136.Text = "Adet";
		xrTableCell136.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell136.Weight = 0.40196116727941184;
		xrTableCell141.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell141.Name = "xrTableCell141";
		xrTableCell141.StylePriority.UseFont = false;
		xrTableCell141.StylePriority.UseTextAlignment = false;
		xrTableCell141.Text = "Tutar";
		xrTableCell141.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell141.Weight = 0.823529411764706;
		xrTableRow43.Cells.AddRange(new XRTableCell[3] { xrTableCell142, xrTableCell143, xrTableCell144 });
		xrTableRow43.Name = "xrTableRow43";
		xrTableRow43.Weight = 1.0;
		xrTableCell142.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell142.Name = "xrTableCell142";
		xrTableCell142.StylePriority.UseFont = false;
		xrTableCell142.StylePriority.UseTextAlignment = false;
		xrTableCell142.Text = "Sipariş";
		xrTableCell142.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell142.Weight = 1.0;
		xrTableCell143.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.siparis_adedi")
		});
		xrTableCell143.Name = "xrTableCell143";
		xrTableCell143.StylePriority.UseTextAlignment = false;
		xrTableCell143.Text = "xrTableCell11";
		xrTableCell143.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell143.Weight = 0.4019605928308823;
		xrTableCell144.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.siparis_tutari", "{0:c}")
		});
		xrTableCell144.Name = "xrTableCell144";
		xrTableCell144.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell144.StylePriority.UsePadding = false;
		xrTableCell144.Text = "xrTableCell12";
		xrTableCell144.Weight = 0.8235296989889708;
		xrTableRow44.Cells.AddRange(new XRTableCell[3] { xrTableCell146, xrTableCell149, xrTableCell152 });
		xrTableRow44.Name = "xrTableRow44";
		xrTableRow44.Weight = 1.0;
		xrTableCell146.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell146.Name = "xrTableCell146";
		xrTableCell146.StylePriority.UseFont = false;
		xrTableCell146.StylePriority.UseTextAlignment = false;
		xrTableCell146.Text = "Fatura";
		xrTableCell146.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell146.Weight = 1.0;
		xrTableCell149.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.fatura_adedi")
		});
		xrTableCell149.Name = "xrTableCell149";
		xrTableCell149.StylePriority.UseTextAlignment = false;
		xrTableCell149.Text = "xrTableCell14";
		xrTableCell149.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell149.Weight = 0.4019605928308823;
		xrTableCell152.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.fatura_tutari", "{0:c}")
		});
		xrTableCell152.Name = "xrTableCell152";
		xrTableCell152.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell152.StylePriority.UsePadding = false;
		xrTableCell152.Text = "xrTableCell15";
		xrTableCell152.Weight = 0.8235296989889708;
		xrTable23.Borders = BorderSide.All;
		xrTable23.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable23.LocationFloat = new PointFloat(10.00001f, 62.08801f);
		xrTable23.Name = "xrTable23";
		xrTable23.Rows.AddRange(new XRTableRow[4] { xrTableRow45, xrTableRow48, xrTableRow49, xrTableRow50 });
		xrTable23.SizeF = new SizeF(164.5834f, 103.0092f);
		xrTable23.StylePriority.UseBorders = false;
		xrTable23.StylePriority.UseFont = false;
		xrTable23.StylePriority.UseTextAlignment = false;
		xrTable23.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow45.Cells.AddRange(new XRTableCell[1] { xrTableCell138 });
		xrTableRow45.Name = "xrTableRow45";
		xrTableRow45.Weight = 1.0;
		xrTableCell138.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell138.Name = "xrTableCell138";
		xrTableCell138.StylePriority.UseFont = false;
		xrTableCell138.StylePriority.UseTextAlignment = false;
		xrTableCell138.Text = "Süreler";
		xrTableCell138.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell138.Weight = 2.225490291819853;
		xrTableRow48.Cells.AddRange(new XRTableCell[2] { xrTableCell145, xrTableCell147 });
		xrTableRow48.Name = "xrTableRow48";
		xrTableRow48.Weight = 1.0;
		xrTableCell145.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell145.Name = "xrTableCell145";
		xrTableCell145.StylePriority.UseFont = false;
		xrTableCell145.StylePriority.UseTextAlignment = false;
		xrTableCell145.Text = "Mesai süresi";
		xrTableCell145.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell145.Weight = 1.241433455500236;
		xrTableCell147.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.mesai_suresi", "{0:c}")
		});
		xrTableCell147.Name = "xrTableCell147";
		xrTableCell147.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell147.StylePriority.UsePadding = false;
		xrTableCell147.Text = "xrTableCell15";
		xrTableCell147.Weight = 0.9840568363196172;
		xrTableRow49.Cells.AddRange(new XRTableCell[2] { xrTableCell148, xrTableCell150 });
		xrTableRow49.Name = "xrTableRow49";
		xrTableRow49.Weight = 1.0;
		xrTableCell148.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell148.Name = "xrTableCell148";
		xrTableCell148.StylePriority.UseFont = false;
		xrTableCell148.StylePriority.UseTextAlignment = false;
		xrTableCell148.Text = "Ziyaret süresi";
		xrTableCell148.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell148.Weight = 1.2414332658307208;
		xrTableCell150.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.toplam_ziyaret_suresi", "{0:c}")
		});
		xrTableCell150.Name = "xrTableCell150";
		xrTableCell150.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell150.StylePriority.UsePadding = false;
		xrTableCell150.Text = "xrTableCell30";
		xrTableCell150.Weight = 0.9840570259891325;
		xrTableRow50.Cells.AddRange(new XRTableCell[2] { xrTableCell151, xrTableCell153 });
		xrTableRow50.Name = "xrTableRow50";
		xrTableRow50.Weight = 1.0;
		xrTableCell151.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell151.Name = "xrTableCell151";
		xrTableCell151.StylePriority.UseFont = false;
		xrTableCell151.StylePriority.UseTextAlignment = false;
		xrTableCell151.Text = "Ulaşım süresi";
		xrTableCell151.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell151.Weight = 1.2414332658307208;
		xrTableCell153.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.toplam_ulasim_suresi")
		});
		xrTableCell153.Name = "xrTableCell153";
		xrTableCell153.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell153.StylePriority.UsePadding = false;
		xrTableCell153.Text = "xrTableCell21";
		xrTableCell153.Weight = 0.9840570259891325;
		xrTable21.Borders = BorderSide.All;
		xrTable21.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable21.LocationFloat = new PointFloat(186.5368f, 62.08801f);
		xrTable21.Name = "xrTable21";
		xrTable21.Rows.AddRange(new XRTableRow[5] { xrTableRow39, xrTableRow6, xrTableRow41, xrTableRow42, xrTableRow46 });
		xrTable21.SizeF = new SizeF(216.7941f, 103.0092f);
		xrTable21.StylePriority.UseBorders = false;
		xrTable21.StylePriority.UseFont = false;
		xrTable21.StylePriority.UseTextAlignment = false;
		xrTable21.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow39.Cells.AddRange(new XRTableCell[1] { xrTableCell26 });
		xrTableRow39.Name = "xrTableRow39";
		xrTableRow39.Weight = 1.0;
		xrTableCell26.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell26.Name = "xrTableCell26";
		xrTableCell26.StylePriority.UseFont = false;
		xrTableCell26.StylePriority.UseTextAlignment = false;
		xrTableCell26.Text = "Çıkış & Dönüş";
		xrTableCell26.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell26.Weight = 1.814851493558398;
		xrTableRow6.Cells.AddRange(new XRTableCell[3] { xrTableCell16, xrTableCell92, xrTableCell93 });
		xrTableRow6.Name = "xrTableRow6";
		xrTableRow6.Weight = 1.0;
		xrTableCell16.Name = "xrTableCell16";
		xrTableCell16.Weight = 0.7041555058386134;
		xrTableCell92.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell92.Name = "xrTableCell92";
		xrTableCell92.StylePriority.UseFont = false;
		xrTableCell92.StylePriority.UseTextAlignment = false;
		xrTableCell92.Text = "Saati";
		xrTableCell92.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell92.Weight = 0.5813300378310021;
		xrTableCell93.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell93.Name = "xrTableCell93";
		xrTableCell93.StylePriority.UseFont = false;
		xrTableCell93.StylePriority.UseTextAlignment = false;
		xrTableCell93.Text = "Araç km";
		xrTableCell93.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell93.Weight = 0.5293659498887824;
		xrTableRow41.Cells.AddRange(new XRTableCell[3] { xrTableCell122, xrTableCell125, xrTableCell18 });
		xrTableRow41.Name = "xrTableRow41";
		xrTableRow41.Weight = 1.0;
		xrTableCell122.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell122.Name = "xrTableCell122";
		xrTableCell122.StylePriority.UseFont = false;
		xrTableCell122.StylePriority.UseTextAlignment = false;
		xrTableCell122.Text = "Çıkış";
		xrTableCell122.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell122.Weight = 0.704155475086658;
		xrTableCell125.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.baslama_saati", "{0:c}")
		});
		xrTableCell125.Name = "xrTableCell125";
		xrTableCell125.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell125.StylePriority.UsePadding = false;
		xrTableCell125.Text = "xrTableCell24";
		xrTableCell125.Weight = 0.5813300685829575;
		xrTableCell18.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.baslama_arac_km", "{0:n0}")
		});
		xrTableCell18.Name = "xrTableCell18";
		xrTableCell18.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell18.StylePriority.UsePadding = false;
		xrTableCell18.Text = "xrTableCell18";
		xrTableCell18.Weight = 0.5293659498887824;
		xrTableRow42.Cells.AddRange(new XRTableCell[3] { xrTableCell135, xrTableCell137, xrTableCell31 });
		xrTableRow42.Name = "xrTableRow42";
		xrTableRow42.Weight = 1.0;
		xrTableCell135.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell135.Name = "xrTableCell135";
		xrTableCell135.StylePriority.UseFont = false;
		xrTableCell135.StylePriority.UseTextAlignment = false;
		xrTableCell135.Text = "Dönüş";
		xrTableCell135.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell135.Weight = 0.7041554184584679;
		xrTableCell137.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.bitis_saati", "{0:c}")
		});
		xrTableCell137.Name = "xrTableCell137";
		xrTableCell137.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell137.StylePriority.UsePadding = false;
		xrTableCell137.Text = "xrTableCell18";
		xrTableCell137.Weight = 0.581330127649008;
		xrTableCell31.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.bitis_arac_km", "{0:n0}")
		});
		xrTableCell31.Name = "xrTableCell31";
		xrTableCell31.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell31.StylePriority.UsePadding = false;
		xrTableCell31.Text = "xrTableCell31";
		xrTableCell31.Weight = 0.5293659474509222;
		xrTableRow46.Cells.AddRange(new XRTableCell[3] { xrTableCell139, xrTableCell140, xrTableCell32 });
		xrTableRow46.Name = "xrTableRow46";
		xrTableRow46.Weight = 1.0;
		xrTableCell139.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell139.Name = "xrTableCell139";
		xrTableCell139.StylePriority.UseFont = false;
		xrTableCell139.StylePriority.UseTextAlignment = false;
		xrTableCell139.Text = "Yapılan km";
		xrTableCell139.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell139.Weight = 0.7041554184584679;
		xrTableCell140.Name = "xrTableCell140";
		xrTableCell140.Weight = 0.581330127649008;
		xrTableCell32.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilan_km", "{0:n0}")
		});
		xrTableCell32.Name = "xrTableCell32";
		xrTableCell32.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell32.StylePriority.UsePadding = false;
		xrTableCell32.Text = "xrTableCell32";
		xrTableCell32.Weight = 0.5293659474509222;
		xrTable20.Borders = BorderSide.All;
		xrTable20.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable20.LocationFloat = new PointFloat(557.4436f, 179.9121f);
		xrTable20.Name = "xrTable20";
		xrTable20.Rows.AddRange(new XRTableRow[4] { xrTableRow7, xrTableRow8, xrTableRow9, xrTableRow34 });
		xrTable20.SizeF = new SizeF(220.8272f, 82.40735f);
		xrTable20.StylePriority.UseBorders = false;
		xrTable20.StylePriority.UseFont = false;
		xrTable20.StylePriority.UseTextAlignment = false;
		xrTable20.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow7.Cells.AddRange(new XRTableCell[1] { xrTableCell19 });
		xrTableRow7.Name = "xrTableRow7";
		xrTableRow7.Weight = 1.0;
		xrTableCell19.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell19.Name = "xrTableCell19";
		xrTableCell19.StylePriority.UseFont = false;
		xrTableCell19.StylePriority.UseTextAlignment = false;
		xrTableCell19.Text = "Nakit teslimi";
		xrTableCell19.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell19.Weight = 2.121005527543579;
		xrTableRow8.Cells.AddRange(new XRTableCell[2] { xrTableCell22, xrTableCell24 });
		xrTableRow8.Name = "xrTableRow8";
		xrTableRow8.Weight = 1.0;
		xrTableCell22.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell22.Name = "xrTableCell22";
		xrTableCell22.StylePriority.UseFont = false;
		xrTableCell22.StylePriority.UseTextAlignment = false;
		xrTableCell22.Text = "Nakit tahsilat";
		xrTableCell22.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell22.Weight = 1.1397152931319194;
		xrTableCell24.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell24.Name = "xrTableCell24";
		xrTableCell24.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell24.StylePriority.UsePadding = false;
		xrTableCell24.Text = "xrTableCell15";
		xrTableCell24.Weight = 0.9812902344116597;
		xrTableRow9.Cells.AddRange(new XRTableCell[2] { xrTableCell25, xrTableCell27 });
		xrTableRow9.Name = "xrTableRow9";
		xrTableRow9.Weight = 1.0;
		xrTableCell25.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell25.Name = "xrTableCell25";
		xrTableCell25.StylePriority.UseFont = false;
		xrTableCell25.StylePriority.UseTextAlignment = false;
		xrTableCell25.Text = "Yapılan masraf";
		xrTableCell25.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell25.Weight = 1.139715238939048;
		xrTableCell27.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.masraf_tutari", "{0:c}")
		});
		xrTableCell27.Name = "xrTableCell27";
		xrTableCell27.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell27.StylePriority.UsePadding = false;
		xrTableCell27.Text = "xrTableCell30";
		xrTableCell27.Weight = 0.9812902886045312;
		xrTableRow34.Cells.AddRange(new XRTableCell[2] { xrTableCell121, xrTableCell123 });
		xrTableRow34.Name = "xrTableRow34";
		xrTableRow34.Weight = 1.0;
		xrTableCell121.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell121.Name = "xrTableCell121";
		xrTableCell121.StylePriority.UseFont = false;
		xrTableCell121.StylePriority.UseTextAlignment = false;
		xrTableCell121.Text = "Kalan nakit";
		xrTableCell121.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell121.Weight = 1.139715238939048;
		xrTableCell123.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.kalan_nakit_tutar", "{0:c}")
		});
		xrTableCell123.Name = "xrTableCell123";
		xrTableCell123.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell123.StylePriority.UsePadding = false;
		xrTableCell123.Text = "xrTableCell33";
		xrTableCell123.Weight = 0.9812902886045312;
		xrTable17.Borders = BorderSide.All;
		xrTable17.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable17.LocationFloat = new PointFloat(557.4435f, 62.08801f);
		xrTable17.Name = "xrTable17";
		xrTable17.Rows.AddRange(new XRTableRow[5] { xrTableRow33, xrTableRow32, xrTableRow35, xrTableRow36, xrTableRow37 });
		xrTable17.SizeF = new SizeF(220.8273f, 103.0092f);
		xrTable17.StylePriority.UseBorders = false;
		xrTable17.StylePriority.UseFont = false;
		xrTable17.StylePriority.UseTextAlignment = false;
		xrTable17.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow33.Cells.AddRange(new XRTableCell[1] { xrTableCell120 });
		xrTableRow33.Name = "xrTableRow33";
		xrTableRow33.Weight = 1.0;
		xrTableCell120.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell120.Name = "xrTableCell120";
		xrTableCell120.StylePriority.UseFont = false;
		xrTableCell120.StylePriority.UseTextAlignment = false;
		xrTableCell120.Text = "Tahsilat detayı";
		xrTableCell120.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell120.Weight = 2.225490291819853;
		xrTableRow32.Cells.AddRange(new XRTableCell[3] { xrTableCell97, xrTableCell100, xrTableCell104 });
		xrTableRow32.Name = "xrTableRow32";
		xrTableRow32.Weight = 1.0;
		xrTableCell97.Borders = BorderSide.All;
		xrTableCell97.Name = "xrTableCell97";
		xrTableCell97.StylePriority.UseBorders = false;
		xrTableCell97.Weight = 0.6955634055911216;
		xrTableCell100.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell100.Name = "xrTableCell100";
		xrTableCell100.StylePriority.UseFont = false;
		xrTableCell100.StylePriority.UseTextAlignment = false;
		xrTableCell100.Text = "Adet";
		xrTableCell100.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell100.Weight = 0.5002961849136681;
		xrTableCell104.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell104.Name = "xrTableCell104";
		xrTableCell104.StylePriority.UseFont = false;
		xrTableCell104.StylePriority.UseTextAlignment = false;
		xrTableCell104.Text = "Tutar";
		xrTableCell104.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell104.Weight = 1.0296307013150634;
		xrTableRow35.Cells.AddRange(new XRTableCell[3] { xrTableCell126, xrTableCell127, xrTableCell128 });
		xrTableRow35.Name = "xrTableRow35";
		xrTableRow35.Weight = 1.0;
		xrTableCell126.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell126.Name = "xrTableCell126";
		xrTableCell126.StylePriority.UseFont = false;
		xrTableCell126.StylePriority.UseTextAlignment = false;
		xrTableCell126.Text = "Çek";
		xrTableCell126.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell126.Weight = 0.6955631161495585;
		xrTableCell127.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.cek_adedi")
		});
		xrTableCell127.Name = "xrTableCell127";
		xrTableCell127.StylePriority.UseTextAlignment = false;
		xrTableCell127.Text = "xrTableCell23";
		xrTableCell127.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell127.Weight = 0.5002968022411829;
		xrTableCell128.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.cek_tutari", "{0:c}")
		});
		xrTableCell128.Name = "xrTableCell128";
		xrTableCell128.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell128.StylePriority.UsePadding = false;
		xrTableCell128.Text = "xrTableCell24";
		xrTableCell128.Weight = 1.0296303734291117;
		xrTableRow36.Cells.AddRange(new XRTableCell[3] { xrTableCell129, xrTableCell130, xrTableCell131 });
		xrTableRow36.Name = "xrTableRow36";
		xrTableRow36.Weight = 1.0;
		xrTableCell129.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell129.Name = "xrTableCell129";
		xrTableCell129.StylePriority.UseFont = false;
		xrTableCell129.StylePriority.UseTextAlignment = false;
		xrTableCell129.Text = "Kredi kartı";
		xrTableCell129.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell129.Weight = 0.6955617321515715;
		xrTableCell130.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.kk_adedi")
		});
		xrTableCell130.Name = "xrTableCell130";
		xrTableCell130.StylePriority.UseTextAlignment = false;
		xrTableCell130.Text = "xrTableCell17";
		xrTableCell130.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell130.Weight = 0.5002974173513993;
		xrTableCell131.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.kk_tutari", "{0:c}")
		});
		xrTableCell131.Name = "xrTableCell131";
		xrTableCell131.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell131.StylePriority.UsePadding = false;
		xrTableCell131.Text = "xrTableCell18";
		xrTableCell131.Weight = 1.0296311423168822;
		xrTableRow37.Cells.AddRange(new XRTableCell[3] { xrTableCell132, xrTableCell133, xrTableCell134 });
		xrTableRow37.Name = "xrTableRow37";
		xrTableRow37.Weight = 1.0;
		xrTableCell132.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell132.Name = "xrTableCell132";
		xrTableCell132.StylePriority.UseFont = false;
		xrTableCell132.StylePriority.UseTextAlignment = false;
		xrTableCell132.Text = "Nakit";
		xrTableCell132.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell132.Weight = 0.695562347261788;
		xrTableCell133.Name = "xrTableCell133";
		xrTableCell133.StylePriority.UseTextAlignment = false;
		xrTableCell133.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell133.Weight = 0.5002974173513992;
		xrTableCell134.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.nakit_tahsilat_tutari", "{0:c}")
		});
		xrTableCell134.Name = "xrTableCell134";
		xrTableCell134.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell134.StylePriority.UsePadding = false;
		xrTableCell134.Text = "xrTableCell21";
		xrTableCell134.Weight = 1.029630527206666;
		xrTable16.Borders = BorderSide.All;
		xrTable16.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable16.LocationFloat = new PointFloat(414.1547f, 62.08804f);
		xrTable16.Name = "xrTable16";
		xrTable16.Rows.AddRange(new XRTableRow[5] { xrTableRow31, xrTableRow25, xrTableRow28, xrTableRow29, xrTableRow30 });
		xrTable16.SizeF = new SizeF(132.2918f, 103.0092f);
		xrTable16.StylePriority.UseBorders = false;
		xrTable16.StylePriority.UseFont = false;
		xrTable16.StylePriority.UseTextAlignment = false;
		xrTable16.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow31.Cells.AddRange(new XRTableCell[1] { xrTableCell94 });
		xrTableRow31.Name = "xrTableRow31";
		xrTableRow31.Weight = 1.0;
		xrTableCell94.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell94.Name = "xrTableCell94";
		xrTableCell94.StylePriority.UseFont = false;
		xrTableCell94.StylePriority.UseTextAlignment = false;
		xrTableCell94.Text = "Ziyaretler";
		xrTableCell94.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell94.Weight = 1.7775933955576524;
		xrTableRow25.Cells.AddRange(new XRTableCell[2] { xrTableCell95, xrTableCell96 });
		xrTableRow25.Name = "xrTableRow25";
		xrTableRow25.Weight = 1.0;
		xrTableCell95.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell95.Name = "xrTableCell95";
		xrTableCell95.StylePriority.UseFont = false;
		xrTableCell95.StylePriority.UseTextAlignment = false;
		xrTableCell95.Text = "Hedef";
		xrTableCell95.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell95.Weight = 1.1801759585650011;
		xrTableCell96.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.hedef_ziyaret_sayisi")
		});
		xrTableCell96.Name = "xrTableCell96";
		xrTableCell96.StylePriority.UseTextAlignment = false;
		xrTableCell96.Text = "xrTableCell11";
		xrTableCell96.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell96.Weight = 0.5974174369926514;
		xrTableRow28.Cells.AddRange(new XRTableCell[2] { xrTableCell98, xrTableCell99 });
		xrTableRow28.Name = "xrTableRow28";
		xrTableRow28.Weight = 1.0;
		xrTableCell98.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell98.Name = "xrTableCell98";
		xrTableCell98.StylePriority.UseFont = false;
		xrTableCell98.StylePriority.UseTextAlignment = false;
		xrTableCell98.Text = "Yapılan";
		xrTableCell98.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell98.Weight = 1.180177188751319;
		xrTableCell99.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilan_ziyaret_sayisi")
		});
		xrTableCell99.Name = "xrTableCell99";
		xrTableCell99.StylePriority.UseTextAlignment = false;
		xrTableCell99.Text = "xrTableCell14";
		xrTableCell99.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell99.Weight = 0.5974162068063334;
		xrTableRow29.Cells.AddRange(new XRTableCell[2] { xrTableCell101, xrTableCell102 });
		xrTableRow29.Name = "xrTableRow29";
		xrTableRow29.Weight = 1.0;
		xrTableCell101.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell101.Name = "xrTableCell101";
		xrTableCell101.StylePriority.UseFont = false;
		xrTableCell101.StylePriority.UseTextAlignment = false;
		xrTableCell101.Text = "Yapılmayan";
		xrTableCell101.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell101.Weight = 1.1801759585650011;
		xrTableCell102.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilmayan_ziyaret_sayisi")
		});
		xrTableCell102.Name = "xrTableCell102";
		xrTableCell102.StylePriority.UseTextAlignment = false;
		xrTableCell102.Text = "xrTableCell23";
		xrTableCell102.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell102.Weight = 0.5974174369926514;
		xrTableRow30.Cells.AddRange(new XRTableCell[2] { xrTableCell105, xrTableCell106 });
		xrTableRow30.Name = "xrTableRow30";
		xrTableRow30.Weight = 1.0;
		xrTableCell105.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell105.Name = "xrTableCell105";
		xrTableCell105.StylePriority.UseFont = false;
		xrTableCell105.StylePriority.UseTextAlignment = false;
		xrTableCell105.Text = "Rota dışı";
		xrTableCell105.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell105.Weight = 1.1801780088755311;
		xrTableCell106.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.rota_disi_ziyaret_sayisi")
		});
		xrTableCell106.Name = "xrTableCell106";
		xrTableCell106.StylePriority.UseTextAlignment = false;
		xrTableCell106.Text = "xrTableCell17";
		xrTableCell106.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell106.Weight = 0.5974153866821214;
		xrLabel35.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.bolge_adi")
		});
		xrLabel35.Font = new Font("Arial", 12f, FontStyle.Bold);
		xrLabel35.LocationFloat = new PointFloat(10.00001f, 31.25f);
		xrLabel35.Name = "xrLabel35";
		xrLabel35.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel35.SizeF = new SizeF(168.8776f, 23f);
		xrLabel35.StylePriority.UseFont = false;
		xrLabel35.StylePriority.UseTextAlignment = false;
		xrLabel35.Text = "xrLabel4";
		xrLabel35.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel37.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.baslama_mesaj")
		});
		xrLabel37.Font = new Font("Arial", 9.75f);
		xrLabel37.LocationFloat = new PointFloat(134.9374f, 271.6944f);
		xrLabel37.Name = "xrLabel37";
		xrLabel37.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel37.SizeF = new SizeF(643.3333f, 18.00002f);
		xrLabel37.StylePriority.UseFont = false;
		xrLabel37.StylePriority.UseTextAlignment = false;
		xrLabel37.Text = "xrLabel4";
		xrLabel37.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel39.Font = new Font("Arial", 9.75f, FontStyle.Bold);
		xrLabel39.LocationFloat = new PointFloat(14.10405f, 292.2962f);
		xrLabel39.Name = "xrLabel39";
		xrLabel39.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel39.SizeF = new SizeF(120.8333f, 18f);
		xrLabel39.StylePriority.UseFont = false;
		xrLabel39.StylePriority.UseTextAlignment = false;
		xrLabel39.Text = "Bitiş mesajı :";
		xrLabel39.TextAlignment = TextAlignment.MiddleRight;
		xrLabel36.Font = new Font("Arial", 9.75f, FontStyle.Bold);
		xrLabel36.LocationFloat = new PointFloat(14.10408f, 271.6944f);
		xrLabel36.Name = "xrLabel36";
		xrLabel36.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel36.SizeF = new SizeF(120.8333f, 18f);
		xrLabel36.StylePriority.UseFont = false;
		xrLabel36.StylePriority.UseTextAlignment = false;
		xrLabel36.Text = "Başlama mesajı :";
		xrLabel36.TextAlignment = TextAlignment.MiddleRight;
		xrTable3.Borders = BorderSide.All;
		xrTable3.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable3.LocationFloat = new PointFloat(10.00007f, 179.9121f);
		xrTable3.Name = "xrTable3";
		xrTable3.Rows.AddRange(new XRTableRow[4] { xrTableRow38, xrTableRow3, xrTableRow10, xrTableRow11 });
		xrTable3.SizeF = new SizeF(265.0528f, 82.40733f);
		xrTable3.StylePriority.UseBorders = false;
		xrTable3.StylePriority.UseFont = false;
		xrTable3.StylePriority.UseTextAlignment = false;
		xrTable3.TextAlignment = TextAlignment.MiddleRight;
		xrTableRow38.Cells.AddRange(new XRTableCell[1] { xrTableCell23 });
		xrTableRow38.Name = "xrTableRow38";
		xrTableRow38.Weight = 1.0;
		xrTableCell23.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell23.Name = "xrTableCell23";
		xrTableCell23.StylePriority.UseFont = false;
		xrTableCell23.StylePriority.UseTextAlignment = false;
		xrTableCell23.Text = "Tahsilat & Masraf";
		xrTableCell23.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell23.Weight = 2.225490291819853;
		xrTableRow3.Cells.AddRange(new XRTableCell[3] { xrTableCell7, xrTableCell8, xrTableCell9 });
		xrTableRow3.Name = "xrTableRow3";
		xrTableRow3.Weight = 1.0;
		xrTableCell7.Borders = BorderSide.All;
		xrTableCell7.Name = "xrTableCell7";
		xrTableCell7.StylePriority.UseBorders = false;
		xrTableCell7.Weight = 0.9999997127757353;
		xrTableCell8.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell8.Name = "xrTableCell8";
		xrTableCell8.StylePriority.UseFont = false;
		xrTableCell8.StylePriority.UseTextAlignment = false;
		xrTableCell8.Text = "Adet";
		xrTableCell8.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell8.Weight = 0.40196116727941184;
		xrTableCell9.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell9.Name = "xrTableCell9";
		xrTableCell9.StylePriority.UseFont = false;
		xrTableCell9.StylePriority.UseTextAlignment = false;
		xrTableCell9.Text = "Tutar";
		xrTableCell9.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell9.Weight = 0.823529411764706;
		xrTableRow10.Cells.AddRange(new XRTableCell[3] { xrTableCell28, xrTableCell29, xrTableCell30 });
		xrTableRow10.Name = "xrTableRow10";
		xrTableRow10.Weight = 1.0;
		xrTableCell28.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell28.Name = "xrTableCell28";
		xrTableCell28.StylePriority.UseFont = false;
		xrTableCell28.StylePriority.UseTextAlignment = false;
		xrTableCell28.Text = "Masraf";
		xrTableCell28.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell28.Weight = 1.0;
		xrTableCell29.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.masraf_adedi")
		});
		xrTableCell29.Name = "xrTableCell29";
		xrTableCell29.StylePriority.UseTextAlignment = false;
		xrTableCell29.Text = "xrTableCell29";
		xrTableCell29.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell29.Weight = 0.4019605928308823;
		xrTableCell30.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.masraf_tutari", "{0:c}")
		});
		xrTableCell30.Name = "xrTableCell30";
		xrTableCell30.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell30.StylePriority.UsePadding = false;
		xrTableCell30.Text = "xrTableCell30";
		xrTableCell30.Weight = 0.8235296989889708;
		xrTableRow11.Cells.AddRange(new XRTableCell[3] { xrTableCell17, xrTableCell20, xrTableCell21 });
		xrTableRow11.Name = "xrTableRow11";
		xrTableRow11.Weight = 1.0;
		xrTableCell17.Font = new Font("Arial", 9f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell17.Name = "xrTableCell17";
		xrTableCell17.StylePriority.UseFont = false;
		xrTableCell17.StylePriority.UseTextAlignment = false;
		xrTableCell17.Text = "Tahsilat";
		xrTableCell17.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell17.Weight = 1.0;
		xrTableCell20.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.tahsilat_adedi")
		});
		xrTableCell20.Name = "xrTableCell20";
		xrTableCell20.StylePriority.UseTextAlignment = false;
		xrTableCell20.Text = "xrTableCell20";
		xrTableCell20.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell20.Weight = 0.4019605928308823;
		xrTableCell21.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.toplam_tahsilat_tutari", "{0:c}")
		});
		xrTableCell21.Name = "xrTableCell21";
		xrTableCell21.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell21.StylePriority.UsePadding = false;
		xrTableCell21.Text = "xrTableCell21";
		xrTableCell21.Weight = 0.8235296989889708;
		xrLabel1.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.temsilci_adi")
		});
		xrLabel1.Font = new Font("Arial", 12f, FontStyle.Bold);
		xrLabel1.LocationFloat = new PointFloat(252.4141f, 31.25f);
		xrLabel1.Name = "xrLabel1";
		xrLabel1.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel1.SizeF = new SizeF(288.6692f, 23f);
		xrLabel1.StylePriority.UseFont = false;
		xrLabel1.StylePriority.UseTextAlignment = false;
		xrLabel1.Text = "xrLabel4";
		xrLabel1.TextAlignment = TextAlignment.MiddleCenter;
		xrLabel38.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.bitis_mesaj")
		});
		xrLabel38.Font = new Font("Arial", 9.75f);
		xrLabel38.LocationFloat = new PointFloat(134.9374f, 292.2962f);
		xrLabel38.Name = "xrLabel38";
		xrLabel38.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel38.SizeF = new SizeF(643.3334f, 18.00006f);
		xrLabel38.StylePriority.UseFont = false;
		xrLabel38.StylePriority.UseTextAlignment = false;
		xrLabel38.Text = "xrLabel4";
		xrLabel38.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel7.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.tarih", "{0:dd.MM.yyyy}")
		});
		xrLabel7.Font = new Font("Arial", 12f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrLabel7.LocationFloat = new PointFloat(613.6875f, 31.25f);
		xrLabel7.Name = "xrLabel7";
		xrLabel7.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel7.SizeF = new SizeF(164.5834f, 23f);
		xrLabel7.StylePriority.UseFont = false;
		xrLabel7.StylePriority.UseTextAlignment = false;
		xrLabel7.Text = "xrLabel4";
		xrLabel7.TextAlignment = TextAlignment.MiddleRight;
		Ziyaret_Listesi.Bands.AddRange(new Band[2] { Ziyaret_Listesi_Liste, GroupHeader1 });
		Ziyaret_Listesi.DataMember = "bolgeler.temsilciler.gunler.ziyaret_listesi";
		Ziyaret_Listesi.DataSource = bindingSource1;
		Ziyaret_Listesi.Level = 0;
		Ziyaret_Listesi.Name = "Ziyaret_Listesi";
		Ziyaret_Listesi.ReportPrintOptions.PrintOnEmptyDataSource = false;
		Ziyaret_Listesi_Liste.Controls.AddRange(new XRControl[1] { xrTable25 });
		Ziyaret_Listesi_Liste.HeightF = 15f;
		Ziyaret_Listesi_Liste.Name = "Ziyaret_Listesi_Liste";
		xrTable25.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable25.EvenStyleName = "xrControlStyle1";
		xrTable25.LocationFloat = new PointFloat(0f, 0f);
		xrTable25.Name = "xrTable25";
		xrTable25.OddStyleName = "xrControlStyle2";
		xrTable25.Rows.AddRange(new XRTableRow[1] { xrTableRow5 });
		xrTable25.SizeF = new SizeF(788.9999f, 15f);
		xrTable25.StylePriority.UseBorders = false;
		xrTableRow5.Cells.AddRange(new XRTableCell[6] { xrTableCell159, xrTableCell158, xrTableCell157, xrTableCell13, xrTableCell14, xrTableCell15 });
		xrTableRow5.Name = "xrTableRow5";
		xrTableRow5.Weight = 1.0;
		xrTableCell159.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.ziyaret_listesi.ziyaret_edildi_uzun")
		});
		xrTableCell159.Font = new Font("Arial Narrow", 8.25f);
		xrTableCell159.Name = "xrTableCell159";
		xrTableCell159.StylePriority.UseFont = false;
		xrTableCell159.StylePriority.UseTextAlignment = false;
		xrTableCell159.Text = "xrTableCell159";
		xrTableCell159.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell159.Weight = 1.0253787381663044;
		xrTableCell158.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.ziyaret_listesi.rotada_var_uzun")
		});
		xrTableCell158.Font = new Font("Arial Narrow", 8.25f);
		xrTableCell158.Name = "xrTableCell158";
		xrTableCell158.StylePriority.UseFont = false;
		xrTableCell158.StylePriority.UseTextAlignment = false;
		xrTableCell158.Text = "xrTableCell158";
		xrTableCell158.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell158.Weight = 0.751628998548874;
		xrTableCell157.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.ziyaret_listesi.cari_kodu")
		});
		xrTableCell157.Font = new Font("Arial Narrow", 8.25f);
		xrTableCell157.Name = "xrTableCell157";
		xrTableCell157.StylePriority.UseFont = false;
		xrTableCell157.StylePriority.UseTextAlignment = false;
		xrTableCell157.Text = "xrTableCell157";
		xrTableCell157.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell157.Weight = 0.9808128432515466;
		xrTableCell13.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.ziyaret_listesi.cari_ismi")
		});
		xrTableCell13.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell13.Name = "xrTableCell13";
		xrTableCell13.StylePriority.UseFont = false;
		xrTableCell13.StylePriority.UseTextAlignment = false;
		xrTableCell13.Text = "xrTableCell4";
		xrTableCell13.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell13.Weight = 3.896820400128422;
		xrTableCell13.WordWrap = false;
		xrTableCell14.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.ziyaret_listesi.baslama_zamani")
		});
		xrTableCell14.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell14.Name = "xrTableCell14";
		xrTableCell14.StylePriority.UseFont = false;
		xrTableCell14.StylePriority.UseTextAlignment = false;
		xrTableCell14.Text = "xrTableCell5";
		xrTableCell14.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell14.Weight = 0.666609119160821;
		xrTableCell14.WordWrap = false;
		xrTableCell15.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.ziyaret_listesi.bitis_zamani")
		});
		xrTableCell15.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell15.Name = "xrTableCell15";
		xrTableCell15.StylePriority.UseFont = false;
		xrTableCell15.StylePriority.UseTextAlignment = false;
		xrTableCell15.Text = "xrTableCell38";
		xrTableCell15.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell15.Weight = 0.5687499710684145;
		xrTableCell15.WordWrap = false;
		GroupHeader1.Controls.AddRange(new XRControl[2] { xrTable24, xrLabel9 });
		GroupHeader1.HeightF = 55.91667f;
		GroupHeader1.KeepTogether = true;
		GroupHeader1.Name = "GroupHeader1";
		xrTable24.LocationFloat = new PointFloat(6.103516E-05f, 30.91667f);
		xrTable24.Name = "xrTable24";
		xrTable24.Rows.AddRange(new XRTableRow[1] { xrTableRow4 });
		xrTable24.SizeF = new SizeF(788.9999f, 25f);
		xrTableRow4.Cells.AddRange(new XRTableCell[6] { xrTableCell156, xrTableCell155, xrTableCell154, xrTableCell10, xrTableCell11, xrTableCell12 });
		xrTableRow4.Name = "xrTableRow4";
		xrTableRow4.Weight = 1.0;
		xrTableCell156.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell156.Borders = BorderSide.All;
		xrTableCell156.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell156.Name = "xrTableCell156";
		xrTableCell156.StylePriority.UseBackColor = false;
		xrTableCell156.StylePriority.UseBorders = false;
		xrTableCell156.StylePriority.UseFont = false;
		xrTableCell156.StylePriority.UseTextAlignment = false;
		xrTableCell156.Text = "Ziyaret edildi";
		xrTableCell156.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell156.Weight = 0.4597202136079748;
		xrTableCell155.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell155.Borders = BorderSide.All;
		xrTableCell155.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell155.Name = "xrTableCell155";
		xrTableCell155.StylePriority.UseBackColor = false;
		xrTableCell155.StylePriority.UseBorders = false;
		xrTableCell155.StylePriority.UseFont = false;
		xrTableCell155.StylePriority.UseTextAlignment = false;
		xrTableCell155.Text = "Rota var";
		xrTableCell155.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell155.Weight = 0.33698715370921817;
		xrTableCell154.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell154.Borders = BorderSide.All;
		xrTableCell154.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell154.Name = "xrTableCell154";
		xrTableCell154.StylePriority.UseBackColor = false;
		xrTableCell154.StylePriority.UseBorders = false;
		xrTableCell154.StylePriority.UseFont = false;
		xrTableCell154.StylePriority.UseTextAlignment = false;
		xrTableCell154.Text = "Cari kodu";
		xrTableCell154.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell154.Weight = 0.43973954376295854;
		xrTableCell10.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell10.Borders = BorderSide.All;
		xrTableCell10.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell10.ForeColor = Color.Black;
		xrTableCell10.Name = "xrTableCell10";
		xrTableCell10.StylePriority.UseBackColor = false;
		xrTableCell10.StylePriority.UseBorders = false;
		xrTableCell10.StylePriority.UseFont = false;
		xrTableCell10.StylePriority.UseForeColor = false;
		xrTableCell10.StylePriority.UseTextAlignment = false;
		xrTableCell10.Text = "Cari ünvan";
		xrTableCell10.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell10.Weight = 1.7471095546154984;
		xrTableCell11.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell11.Borders = BorderSide.All;
		xrTableCell11.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell11.ForeColor = Color.Black;
		xrTableCell11.FormattingRules.Add(formattingRule1);
		xrTableCell11.Name = "xrTableCell11";
		xrTableCell11.StylePriority.UseBackColor = false;
		xrTableCell11.StylePriority.UseBorders = false;
		xrTableCell11.StylePriority.UseFont = false;
		xrTableCell11.StylePriority.UseForeColor = false;
		xrTableCell11.StylePriority.UseTextAlignment = false;
		xrTableCell11.Text = "Başlama";
		xrTableCell11.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell11.Weight = 0.2988689296902387;
		xrTableCell12.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell12.Borders = BorderSide.All;
		xrTableCell12.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell12.ForeColor = Color.Black;
		xrTableCell12.Name = "xrTableCell12";
		xrTableCell12.StylePriority.UseBackColor = false;
		xrTableCell12.StylePriority.UseBorders = false;
		xrTableCell12.StylePriority.UseFont = false;
		xrTableCell12.StylePriority.UseForeColor = false;
		xrTableCell12.StylePriority.UseTextAlignment = false;
		xrTableCell12.Text = "Bitiş";
		xrTableCell12.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell12.Weight = 0.2549949537352043;
		xrLabel9.BackColor = Color.Silver;
		xrLabel9.Borders = BorderSide.Left | BorderSide.Top | BorderSide.Right;
		xrLabel9.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrLabel9.ForeColor = Color.Black;
		xrLabel9.LocationFloat = new PointFloat(6.103516E-05f, 10f);
		xrLabel9.Name = "xrLabel9";
		xrLabel9.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel9.SizeF = new SizeF(788.9999f, 20.91667f);
		xrLabel9.StylePriority.UseBackColor = false;
		xrLabel9.StylePriority.UseBorders = false;
		xrLabel9.StylePriority.UseFont = false;
		xrLabel9.StylePriority.UseForeColor = false;
		xrLabel9.StylePriority.UseTextAlignment = false;
		xrLabel9.Text = "ZİYARET LİSTESİ";
		xrLabel9.TextAlignment = TextAlignment.MiddleCenter;
		bindingSource1.DataSource = typeof(Bolgeler);
		Hedef_Ziyaretler.Bands.AddRange(new Band[2] { Hedef_Ziyaretler_Liste, Hedef_Ziyaretler_Baslik });
		Hedef_Ziyaretler.DataMember = "bolgeler.temsilciler.gunler.hedef_ziyaretler";
		Hedef_Ziyaretler.DataSource = bindingSource1;
		Hedef_Ziyaretler.Expanded = false;
		Hedef_Ziyaretler.Level = 1;
		Hedef_Ziyaretler.Name = "Hedef_Ziyaretler";
		Hedef_Ziyaretler.ReportPrintOptions.PrintOnEmptyDataSource = false;
		Hedef_Ziyaretler_Liste.Controls.AddRange(new XRControl[1] { xrTable19 });
		Hedef_Ziyaretler_Liste.HeightF = 15f;
		Hedef_Ziyaretler_Liste.Name = "Hedef_Ziyaretler_Liste";
		xrTable19.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable19.EvenStyleName = "xrControlStyle1";
		xrTable19.LocationFloat = new PointFloat(0f, 0f);
		xrTable19.Name = "xrTable19";
		xrTable19.OddStyleName = "xrControlStyle2";
		xrTable19.Rows.AddRange(new XRTableRow[1] { xrTableRow27 });
		xrTable19.SizeF = new SizeF(788.9999f, 15f);
		xrTable19.StylePriority.UseBorders = false;
		xrTableRow27.Cells.AddRange(new XRTableCell[3] { xrTableCell109, xrTableCell110, xrTableCell111 });
		xrTableRow27.Name = "xrTableRow27";
		xrTableRow27.Weight = 1.0;
		xrTableCell109.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.hedef_ziyaretler.cari_kodu")
		});
		xrTableCell109.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell109.Name = "xrTableCell109";
		xrTableCell109.StylePriority.UseFont = false;
		xrTableCell109.StylePriority.UseTextAlignment = false;
		xrTableCell109.Text = "xrTableCell4";
		xrTableCell109.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell109.Weight = 0.8750001418040636;
		xrTableCell109.WordWrap = false;
		xrTableCell110.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.hedef_ziyaretler.cari_ismi")
		});
		xrTableCell110.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell110.Name = "xrTableCell110";
		xrTableCell110.StylePriority.UseFont = false;
		xrTableCell110.StylePriority.UseTextAlignment = false;
		xrTableCell110.Text = "xrTableCell5";
		xrTableCell110.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell110.Weight = 2.834608436309265;
		xrTableCell110.WordWrap = false;
		xrTableCell111.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.hedef_ziyaretler.adres")
		});
		xrTableCell111.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell111.Name = "xrTableCell111";
		xrTableCell111.StylePriority.UseFont = false;
		xrTableCell111.StylePriority.UseTextAlignment = false;
		xrTableCell111.Text = "xrTableCell38";
		xrTableCell111.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell111.Weight = 4.180391492211053;
		xrTableCell111.WordWrap = false;
		Hedef_Ziyaretler_Baslik.Controls.AddRange(new XRControl[2] { xrLabel45, xrTable18 });
		Hedef_Ziyaretler_Baslik.HeightF = 55.91667f;
		Hedef_Ziyaretler_Baslik.KeepTogether = true;
		Hedef_Ziyaretler_Baslik.Name = "Hedef_Ziyaretler_Baslik";
		xrLabel45.BackColor = Color.Silver;
		xrLabel45.Borders = BorderSide.Left | BorderSide.Top | BorderSide.Right;
		xrLabel45.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrLabel45.ForeColor = Color.Black;
		xrLabel45.LocationFloat = new PointFloat(6.103516E-05f, 10f);
		xrLabel45.Name = "xrLabel45";
		xrLabel45.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel45.SizeF = new SizeF(788.9999f, 20.91667f);
		xrLabel45.StylePriority.UseBackColor = false;
		xrLabel45.StylePriority.UseBorders = false;
		xrLabel45.StylePriority.UseFont = false;
		xrLabel45.StylePriority.UseForeColor = false;
		xrLabel45.StylePriority.UseTextAlignment = false;
		xrLabel45.Text = "HEDEF ZİYARETLER";
		xrLabel45.TextAlignment = TextAlignment.MiddleCenter;
		xrTable18.LocationFloat = new PointFloat(6.103516E-05f, 30.91667f);
		xrTable18.Name = "xrTable18";
		xrTable18.Rows.AddRange(new XRTableRow[1] { xrTableRow26 });
		xrTable18.SizeF = new SizeF(788.9999f, 25f);
		xrTableRow26.Cells.AddRange(new XRTableCell[3] { xrTableCell103, xrTableCell107, xrTableCell108 });
		xrTableRow26.Name = "xrTableRow26";
		xrTableRow26.Weight = 1.0;
		xrTableCell103.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell103.Borders = BorderSide.All;
		xrTableCell103.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell103.ForeColor = Color.Black;
		xrTableCell103.Name = "xrTableCell103";
		xrTableCell103.StylePriority.UseBackColor = false;
		xrTableCell103.StylePriority.UseBorders = false;
		xrTableCell103.StylePriority.UseFont = false;
		xrTableCell103.StylePriority.UseForeColor = false;
		xrTableCell103.StylePriority.UseTextAlignment = false;
		xrTableCell103.Text = "Cari kodu";
		xrTableCell103.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell103.Weight = 0.3922992281211122;
		xrTableCell107.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell107.Borders = BorderSide.All;
		xrTableCell107.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell107.ForeColor = Color.Black;
		xrTableCell107.FormattingRules.Add(formattingRule1);
		xrTableCell107.Name = "xrTableCell107";
		xrTableCell107.StylePriority.UseBackColor = false;
		xrTableCell107.StylePriority.UseBorders = false;
		xrTableCell107.StylePriority.UseFont = false;
		xrTableCell107.StylePriority.UseForeColor = false;
		xrTableCell107.StylePriority.UseTextAlignment = false;
		xrTableCell107.Text = "Cari ünvan";
		xrTableCell107.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell107.Weight = 1.2708751079311624;
		xrTableCell108.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell108.Borders = BorderSide.All;
		xrTableCell108.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell108.ForeColor = Color.Black;
		xrTableCell108.Name = "xrTableCell108";
		xrTableCell108.StylePriority.UseBackColor = false;
		xrTableCell108.StylePriority.UseBorders = false;
		xrTableCell108.StylePriority.UseFont = false;
		xrTableCell108.StylePriority.UseForeColor = false;
		xrTableCell108.StylePriority.UseTextAlignment = false;
		xrTableCell108.Text = "Adres";
		xrTableCell108.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell108.Weight = 1.8742460130688183;
		Yapilmayan_Ziyaretler.Bands.AddRange(new Band[2] { Yapilmayan_Ziyaret_Listesi, Yapilmayan_Ziyaretler_Baslik });
		Yapilmayan_Ziyaretler.DataMember = "bolgeler.temsilciler.gunler.yapilmayan_ziyaretler";
		Yapilmayan_Ziyaretler.DataSource = bindingSource1;
		Yapilmayan_Ziyaretler.Expanded = false;
		Yapilmayan_Ziyaretler.Level = 2;
		Yapilmayan_Ziyaretler.Name = "Yapilmayan_Ziyaretler";
		Yapilmayan_Ziyaretler.ReportPrintOptions.PrintOnEmptyDataSource = false;
		Yapilmayan_Ziyaret_Listesi.Controls.AddRange(new XRControl[1] { xrTable5 });
		Yapilmayan_Ziyaret_Listesi.HeightF = 15f;
		Yapilmayan_Ziyaret_Listesi.Name = "Yapilmayan_Ziyaret_Listesi";
		xrTable5.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable5.EvenStyleName = "xrControlStyle1";
		xrTable5.LocationFloat = new PointFloat(0f, 0f);
		xrTable5.Name = "xrTable5";
		xrTable5.OddStyleName = "xrControlStyle2";
		xrTable5.Rows.AddRange(new XRTableRow[1] { xrTableRow13 });
		xrTable5.SizeF = new SizeF(788.9999f, 15f);
		xrTable5.StylePriority.UseBorders = false;
		xrTableRow13.Cells.AddRange(new XRTableCell[3] { xrTableCell41, xrTableCell42, xrTableCell43 });
		xrTableRow13.Name = "xrTableRow13";
		xrTableRow13.Weight = 1.0;
		xrTableCell41.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilmayan_ziyaretler.cari_kodu")
		});
		xrTableCell41.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell41.Name = "xrTableCell41";
		xrTableCell41.StylePriority.UseFont = false;
		xrTableCell41.StylePriority.UseTextAlignment = false;
		xrTableCell41.Text = "xrTableCell4";
		xrTableCell41.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell41.Weight = 0.8750001418040636;
		xrTableCell41.WordWrap = false;
		xrTableCell42.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilmayan_ziyaretler.cari_ismi")
		});
		xrTableCell42.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell42.Name = "xrTableCell42";
		xrTableCell42.StylePriority.UseFont = false;
		xrTableCell42.StylePriority.UseTextAlignment = false;
		xrTableCell42.Text = "xrTableCell5";
		xrTableCell42.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell42.Weight = 2.834608436309265;
		xrTableCell42.WordWrap = false;
		xrTableCell43.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilmayan_ziyaretler.adres")
		});
		xrTableCell43.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell43.Name = "xrTableCell43";
		xrTableCell43.StylePriority.UseFont = false;
		xrTableCell43.StylePriority.UseTextAlignment = false;
		xrTableCell43.Text = "xrTableCell38";
		xrTableCell43.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell43.Weight = 4.180391492211053;
		xrTableCell43.WordWrap = false;
		Yapilmayan_Ziyaretler_Baslik.Controls.AddRange(new XRControl[2] { xrTable4, xrLabel4 });
		Yapilmayan_Ziyaretler_Baslik.HeightF = 55.91667f;
		Yapilmayan_Ziyaretler_Baslik.KeepTogether = true;
		Yapilmayan_Ziyaretler_Baslik.Name = "Yapilmayan_Ziyaretler_Baslik";
		xrTable4.LocationFloat = new PointFloat(6.103516E-05f, 30.91667f);
		xrTable4.Name = "xrTable4";
		xrTable4.Rows.AddRange(new XRTableRow[1] { xrTableRow12 });
		xrTable4.SizeF = new SizeF(788.9999f, 25f);
		xrTableRow12.Cells.AddRange(new XRTableCell[3] { xrTableCell35, xrTableCell39, xrTableCell40 });
		xrTableRow12.Name = "xrTableRow12";
		xrTableRow12.Weight = 1.0;
		xrTableCell35.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell35.Borders = BorderSide.All;
		xrTableCell35.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell35.ForeColor = Color.Black;
		xrTableCell35.Name = "xrTableCell35";
		xrTableCell35.StylePriority.UseBackColor = false;
		xrTableCell35.StylePriority.UseBorders = false;
		xrTableCell35.StylePriority.UseFont = false;
		xrTableCell35.StylePriority.UseForeColor = false;
		xrTableCell35.StylePriority.UseTextAlignment = false;
		xrTableCell35.Text = "Cari kodu";
		xrTableCell35.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell35.Weight = 0.3922992281211122;
		xrTableCell39.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell39.Borders = BorderSide.All;
		xrTableCell39.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell39.ForeColor = Color.Black;
		xrTableCell39.FormattingRules.Add(formattingRule1);
		xrTableCell39.Name = "xrTableCell39";
		xrTableCell39.StylePriority.UseBackColor = false;
		xrTableCell39.StylePriority.UseBorders = false;
		xrTableCell39.StylePriority.UseFont = false;
		xrTableCell39.StylePriority.UseForeColor = false;
		xrTableCell39.StylePriority.UseTextAlignment = false;
		xrTableCell39.Text = "Cari ünvan";
		xrTableCell39.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell39.Weight = 1.2708751079311624;
		xrTableCell40.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell40.Borders = BorderSide.All;
		xrTableCell40.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell40.ForeColor = Color.Black;
		xrTableCell40.Name = "xrTableCell40";
		xrTableCell40.StylePriority.UseBackColor = false;
		xrTableCell40.StylePriority.UseBorders = false;
		xrTableCell40.StylePriority.UseFont = false;
		xrTableCell40.StylePriority.UseForeColor = false;
		xrTableCell40.StylePriority.UseTextAlignment = false;
		xrTableCell40.Text = "Adres";
		xrTableCell40.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell40.Weight = 1.8742460130688183;
		xrLabel4.BackColor = Color.Silver;
		xrLabel4.Borders = BorderSide.Left | BorderSide.Top | BorderSide.Right;
		xrLabel4.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrLabel4.ForeColor = Color.Black;
		xrLabel4.LocationFloat = new PointFloat(6.103516E-05f, 10f);
		xrLabel4.Name = "xrLabel4";
		xrLabel4.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel4.SizeF = new SizeF(788.9999f, 20.91667f);
		xrLabel4.StylePriority.UseBackColor = false;
		xrLabel4.StylePriority.UseBorders = false;
		xrLabel4.StylePriority.UseFont = false;
		xrLabel4.StylePriority.UseForeColor = false;
		xrLabel4.StylePriority.UseTextAlignment = false;
		xrLabel4.Text = "YAPILMAYAN ZİYARETLER";
		xrLabel4.TextAlignment = TextAlignment.MiddleCenter;
		Yapilan_Ziyaretler.Bands.AddRange(new Band[2] { Yapilan_Ziyaret_Listesi, Yapilan_Ziyaretler_Baslik });
		Yapilan_Ziyaretler.DataMember = "bolgeler.temsilciler.gunler.yapilan_ziyaretler";
		Yapilan_Ziyaretler.DataSource = bindingSource1;
		Yapilan_Ziyaretler.Expanded = false;
		Yapilan_Ziyaretler.Level = 3;
		Yapilan_Ziyaretler.Name = "Yapilan_Ziyaretler";
		Yapilan_Ziyaretler.ReportPrintOptions.PrintOnEmptyDataSource = false;
		Yapilan_Ziyaret_Listesi.Controls.AddRange(new XRControl[1] { xrTable7 });
		Yapilan_Ziyaret_Listesi.HeightF = 15f;
		Yapilan_Ziyaret_Listesi.Name = "Yapilan_Ziyaret_Listesi";
		xrTable7.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable7.EvenStyleName = "xrControlStyle1";
		xrTable7.LocationFloat = new PointFloat(0f, 0f);
		xrTable7.Name = "xrTable7";
		xrTable7.OddStyleName = "xrControlStyle2";
		xrTable7.Rows.AddRange(new XRTableRow[1] { xrTableRow15 });
		xrTable7.SizeF = new SizeF(788.9999f, 15f);
		xrTable7.StylePriority.UseBorders = false;
		xrTableRow15.Cells.AddRange(new XRTableCell[5] { xrTableCell49, xrTableCell112, xrTableCell113, xrTableCell114, xrTableCell115 });
		xrTableRow15.Name = "xrTableRow15";
		xrTableRow15.Weight = 1.0;
		xrTableCell49.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilan_ziyaretler.cari_kodu")
		});
		xrTableCell49.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell49.Name = "xrTableCell49";
		xrTableCell49.StylePriority.UseFont = false;
		xrTableCell49.StylePriority.UseTextAlignment = false;
		xrTableCell49.Text = "xrTableCell4";
		xrTableCell49.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell49.Weight = 0.8750001418040636;
		xrTableCell49.WordWrap = false;
		xrTableCell112.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilan_ziyaretler.cari_ismi")
		});
		xrTableCell112.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell112.Name = "xrTableCell112";
		xrTableCell112.StylePriority.UseFont = false;
		xrTableCell112.StylePriority.UseTextAlignment = false;
		xrTableCell112.Text = "xrTableCell5";
		xrTableCell112.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell112.Weight = 2.355441830029813;
		xrTableCell112.WordWrap = false;
		xrTableCell113.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilan_ziyaretler.adres")
		});
		xrTableCell113.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell113.Name = "xrTableCell113";
		xrTableCell113.StylePriority.UseFont = false;
		xrTableCell113.StylePriority.UseTextAlignment = false;
		xrTableCell113.Text = "xrTableCell38";
		xrTableCell113.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell113.Weight = 3.424200861502803;
		xrTableCell113.WordWrap = false;
		xrTableCell114.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilan_ziyaretler.baslama_zamani")
		});
		xrTableCell114.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell114.Name = "xrTableCell114";
		xrTableCell114.StylePriority.UseFont = false;
		xrTableCell114.StylePriority.UseTextAlignment = false;
		xrTableCell114.Text = "xrTableCell37";
		xrTableCell114.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell114.Weight = 0.6666070872262644;
		xrTableCell114.WordWrap = false;
		xrTableCell115.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.yapilan_ziyaretler.bitis_zamani")
		});
		xrTableCell115.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell115.Name = "xrTableCell115";
		xrTableCell115.StylePriority.UseFont = false;
		xrTableCell115.StylePriority.UseTextAlignment = false;
		xrTableCell115.Text = "xrTableCell6";
		xrTableCell115.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell115.Weight = 0.568750149761437;
		xrTableCell115.WordWrap = false;
		Yapilan_Ziyaretler_Baslik.Controls.AddRange(new XRControl[2] { xrTable6, xrLabel5 });
		Yapilan_Ziyaretler_Baslik.HeightF = 55.91667f;
		Yapilan_Ziyaretler_Baslik.KeepTogether = true;
		Yapilan_Ziyaretler_Baslik.Name = "Yapilan_Ziyaretler_Baslik";
		xrTable6.LocationFloat = new PointFloat(6.103516E-05f, 30.91667f);
		xrTable6.Name = "xrTable6";
		xrTable6.Rows.AddRange(new XRTableRow[1] { xrTableRow14 });
		xrTable6.SizeF = new SizeF(788.9999f, 25f);
		xrTableRow14.Cells.AddRange(new XRTableCell[5] { xrTableCell44, xrTableCell45, xrTableCell46, xrTableCell47, xrTableCell48 });
		xrTableRow14.Name = "xrTableRow14";
		xrTableRow14.Weight = 1.0;
		xrTableCell44.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell44.Borders = BorderSide.All;
		xrTableCell44.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell44.ForeColor = Color.Black;
		xrTableCell44.Name = "xrTableCell44";
		xrTableCell44.StylePriority.UseBackColor = false;
		xrTableCell44.StylePriority.UseBorders = false;
		xrTableCell44.StylePriority.UseFont = false;
		xrTableCell44.StylePriority.UseForeColor = false;
		xrTableCell44.StylePriority.UseTextAlignment = false;
		xrTableCell44.Text = "Cari kodu";
		xrTableCell44.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell44.Weight = 0.3922992281211122;
		xrTableCell45.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell45.Borders = BorderSide.All;
		xrTableCell45.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell45.ForeColor = Color.Black;
		xrTableCell45.FormattingRules.Add(formattingRule1);
		xrTableCell45.Name = "xrTableCell45";
		xrTableCell45.StylePriority.UseBackColor = false;
		xrTableCell45.StylePriority.UseBorders = false;
		xrTableCell45.StylePriority.UseFont = false;
		xrTableCell45.StylePriority.UseForeColor = false;
		xrTableCell45.StylePriority.UseTextAlignment = false;
		xrTableCell45.Text = "Cari ünvan";
		xrTableCell45.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell45.Weight = 1.0560444590481932;
		xrTableCell46.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell46.Borders = BorderSide.All;
		xrTableCell46.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell46.ForeColor = Color.Black;
		xrTableCell46.Name = "xrTableCell46";
		xrTableCell46.StylePriority.UseBackColor = false;
		xrTableCell46.StylePriority.UseBorders = false;
		xrTableCell46.StylePriority.UseFont = false;
		xrTableCell46.StylePriority.UseForeColor = false;
		xrTableCell46.StylePriority.UseTextAlignment = false;
		xrTableCell46.Text = "Adres";
		xrTableCell46.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell46.Weight = 1.5352134196097904;
		xrTableCell47.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell47.Borders = BorderSide.All;
		xrTableCell47.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell47.ForeColor = Color.Black;
		xrTableCell47.Name = "xrTableCell47";
		xrTableCell47.StylePriority.UseBackColor = false;
		xrTableCell47.StylePriority.UseBorders = false;
		xrTableCell47.StylePriority.UseFont = false;
		xrTableCell47.StylePriority.UseForeColor = false;
		xrTableCell47.StylePriority.UseTextAlignment = false;
		xrTableCell47.Text = "Başlama";
		xrTableCell47.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell47.Weight = 0.29886841921300034;
		xrTableCell48.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell48.Borders = BorderSide.All;
		xrTableCell48.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell48.ForeColor = Color.Black;
		xrTableCell48.Name = "xrTableCell48";
		xrTableCell48.StylePriority.UseBackColor = false;
		xrTableCell48.StylePriority.UseBorders = false;
		xrTableCell48.StylePriority.UseFont = false;
		xrTableCell48.StylePriority.UseForeColor = false;
		xrTableCell48.StylePriority.UseTextAlignment = false;
		xrTableCell48.Text = "Bitiş";
		xrTableCell48.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell48.Weight = 0.2549948231289972;
		xrLabel5.BackColor = Color.Silver;
		xrLabel5.Borders = BorderSide.Left | BorderSide.Top | BorderSide.Right;
		xrLabel5.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrLabel5.ForeColor = Color.Black;
		xrLabel5.LocationFloat = new PointFloat(6.103516E-05f, 10f);
		xrLabel5.Name = "xrLabel5";
		xrLabel5.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel5.SizeF = new SizeF(788.9999f, 20.91667f);
		xrLabel5.StylePriority.UseBackColor = false;
		xrLabel5.StylePriority.UseBorders = false;
		xrLabel5.StylePriority.UseFont = false;
		xrLabel5.StylePriority.UseForeColor = false;
		xrLabel5.StylePriority.UseTextAlignment = false;
		xrLabel5.Text = "YAPILAN ZİYARETLER";
		xrLabel5.TextAlignment = TextAlignment.MiddleCenter;
		Rota_Disi_Ziyaretler.Bands.AddRange(new Band[2] { Rota_Disi_Ziyaret_Listesi, Rota_Disi_Ziyaretler_Baslik });
		Rota_Disi_Ziyaretler.DataMember = "bolgeler.temsilciler.gunler.rota_disi_ziyaretler";
		Rota_Disi_Ziyaretler.DataSource = bindingSource1;
		Rota_Disi_Ziyaretler.Expanded = false;
		Rota_Disi_Ziyaretler.Level = 4;
		Rota_Disi_Ziyaretler.Name = "Rota_Disi_Ziyaretler";
		Rota_Disi_Ziyaretler.ReportPrintOptions.PrintOnEmptyDataSource = false;
		Rota_Disi_Ziyaret_Listesi.Controls.AddRange(new XRControl[1] { xrTable2 });
		Rota_Disi_Ziyaret_Listesi.HeightF = 15f;
		Rota_Disi_Ziyaret_Listesi.Name = "Rota_Disi_Ziyaret_Listesi";
		xrTable2.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable2.EvenStyleName = "xrControlStyle1";
		xrTable2.LocationFloat = new PointFloat(0f, 0f);
		xrTable2.Name = "xrTable2";
		xrTable2.OddStyleName = "xrControlStyle2";
		xrTable2.Rows.AddRange(new XRTableRow[1] { xrTableRow2 });
		xrTable2.SizeF = new SizeF(788.9999f, 15f);
		xrTable2.StylePriority.UseBorders = false;
		xrTableRow2.Cells.AddRange(new XRTableCell[5] { xrTableCell6, xrTableCell34, xrTableCell36, xrTableCell37, xrTableCell38 });
		xrTableRow2.Name = "xrTableRow2";
		xrTableRow2.Weight = 1.0;
		xrTableCell6.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.rota_disi_ziyaretler.cari_kodu")
		});
		xrTableCell6.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell6.Name = "xrTableCell6";
		xrTableCell6.StylePriority.UseFont = false;
		xrTableCell6.StylePriority.UseTextAlignment = false;
		xrTableCell6.Text = "xrTableCell4";
		xrTableCell6.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell6.Weight = 0.8750001418040636;
		xrTableCell6.WordWrap = false;
		xrTableCell34.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.rota_disi_ziyaretler.cari_ismi")
		});
		xrTableCell34.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell34.Name = "xrTableCell34";
		xrTableCell34.StylePriority.UseFont = false;
		xrTableCell34.StylePriority.UseTextAlignment = false;
		xrTableCell34.Text = "xrTableCell5";
		xrTableCell34.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell34.Weight = 2.355441830029813;
		xrTableCell34.WordWrap = false;
		xrTableCell36.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.rota_disi_ziyaretler.adres")
		});
		xrTableCell36.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell36.Name = "xrTableCell36";
		xrTableCell36.StylePriority.UseFont = false;
		xrTableCell36.StylePriority.UseTextAlignment = false;
		xrTableCell36.Text = "xrTableCell38";
		xrTableCell36.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell36.Weight = 3.424200861502803;
		xrTableCell36.WordWrap = false;
		xrTableCell37.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.rota_disi_ziyaretler.baslama_zamani")
		});
		xrTableCell37.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell37.Name = "xrTableCell37";
		xrTableCell37.StylePriority.UseFont = false;
		xrTableCell37.StylePriority.UseTextAlignment = false;
		xrTableCell37.Text = "xrTableCell37";
		xrTableCell37.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell37.Weight = 0.6666070872262644;
		xrTableCell37.WordWrap = false;
		xrTableCell38.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.rota_disi_ziyaretler.bitis_zamani")
		});
		xrTableCell38.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell38.Name = "xrTableCell38";
		xrTableCell38.StylePriority.UseFont = false;
		xrTableCell38.StylePriority.UseTextAlignment = false;
		xrTableCell38.Text = "xrTableCell6";
		xrTableCell38.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell38.Weight = 0.568750149761437;
		xrTableCell38.WordWrap = false;
		Rota_Disi_Ziyaretler_Baslik.Controls.AddRange(new XRControl[2] { xrLabel3, xrTable1 });
		Rota_Disi_Ziyaretler_Baslik.Expanded = false;
		Rota_Disi_Ziyaretler_Baslik.HeightF = 55.91667f;
		Rota_Disi_Ziyaretler_Baslik.KeepTogether = true;
		Rota_Disi_Ziyaretler_Baslik.Name = "Rota_Disi_Ziyaretler_Baslik";
		xrLabel3.BackColor = Color.Silver;
		xrLabel3.Borders = BorderSide.Left | BorderSide.Top | BorderSide.Right;
		xrLabel3.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrLabel3.ForeColor = Color.Black;
		xrLabel3.LocationFloat = new PointFloat(6.103516E-05f, 10f);
		xrLabel3.Name = "xrLabel3";
		xrLabel3.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel3.SizeF = new SizeF(788.9999f, 20.91667f);
		xrLabel3.StylePriority.UseBackColor = false;
		xrLabel3.StylePriority.UseBorders = false;
		xrLabel3.StylePriority.UseFont = false;
		xrLabel3.StylePriority.UseForeColor = false;
		xrLabel3.StylePriority.UseTextAlignment = false;
		xrLabel3.Text = "ROTA DIŞI ZİYARETLER";
		xrLabel3.TextAlignment = TextAlignment.MiddleCenter;
		xrTable1.LocationFloat = new PointFloat(6.103516E-05f, 30.91667f);
		xrTable1.Name = "xrTable1";
		xrTable1.Rows.AddRange(new XRTableRow[1] { xrTableRow1 });
		xrTable1.SizeF = new SizeF(788.9999f, 25f);
		xrTableRow1.Cells.AddRange(new XRTableCell[5] { xrTableCell1, xrTableCell2, xrTableCell3, xrTableCell4, xrTableCell5 });
		xrTableRow1.Name = "xrTableRow1";
		xrTableRow1.Weight = 1.0;
		xrTableCell1.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell1.Borders = BorderSide.All;
		xrTableCell1.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell1.ForeColor = Color.Black;
		xrTableCell1.Name = "xrTableCell1";
		xrTableCell1.StylePriority.UseBackColor = false;
		xrTableCell1.StylePriority.UseBorders = false;
		xrTableCell1.StylePriority.UseFont = false;
		xrTableCell1.StylePriority.UseForeColor = false;
		xrTableCell1.StylePriority.UseTextAlignment = false;
		xrTableCell1.Text = "Cari kodu";
		xrTableCell1.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell1.Weight = 0.3922992281211122;
		xrTableCell2.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell2.Borders = BorderSide.All;
		xrTableCell2.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell2.ForeColor = Color.Black;
		xrTableCell2.FormattingRules.Add(formattingRule1);
		xrTableCell2.Name = "xrTableCell2";
		xrTableCell2.StylePriority.UseBackColor = false;
		xrTableCell2.StylePriority.UseBorders = false;
		xrTableCell2.StylePriority.UseFont = false;
		xrTableCell2.StylePriority.UseForeColor = false;
		xrTableCell2.StylePriority.UseTextAlignment = false;
		xrTableCell2.Text = "Cari ünvan";
		xrTableCell2.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell2.Weight = 1.0560444590481932;
		xrTableCell3.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell3.Borders = BorderSide.All;
		xrTableCell3.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell3.ForeColor = Color.Black;
		xrTableCell3.Name = "xrTableCell3";
		xrTableCell3.StylePriority.UseBackColor = false;
		xrTableCell3.StylePriority.UseBorders = false;
		xrTableCell3.StylePriority.UseFont = false;
		xrTableCell3.StylePriority.UseForeColor = false;
		xrTableCell3.StylePriority.UseTextAlignment = false;
		xrTableCell3.Text = "Adres";
		xrTableCell3.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell3.Weight = 1.5352134196097904;
		xrTableCell4.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell4.Borders = BorderSide.All;
		xrTableCell4.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell4.ForeColor = Color.Black;
		xrTableCell4.Name = "xrTableCell4";
		xrTableCell4.StylePriority.UseBackColor = false;
		xrTableCell4.StylePriority.UseBorders = false;
		xrTableCell4.StylePriority.UseFont = false;
		xrTableCell4.StylePriority.UseForeColor = false;
		xrTableCell4.StylePriority.UseTextAlignment = false;
		xrTableCell4.Text = "Başlama";
		xrTableCell4.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell4.Weight = 0.29886841921300034;
		xrTableCell5.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell5.Borders = BorderSide.All;
		xrTableCell5.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell5.ForeColor = Color.Black;
		xrTableCell5.Name = "xrTableCell5";
		xrTableCell5.StylePriority.UseBackColor = false;
		xrTableCell5.StylePriority.UseBorders = false;
		xrTableCell5.StylePriority.UseFont = false;
		xrTableCell5.StylePriority.UseForeColor = false;
		xrTableCell5.StylePriority.UseTextAlignment = false;
		xrTableCell5.Text = "Bitiş";
		xrTableCell5.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell5.Weight = 0.2549948231289972;
		Siparisler.Bands.AddRange(new Band[2] { Siparis_Listesi, Siparisler_Baslik });
		Siparisler.DataMember = "bolgeler.temsilciler.gunler.siparisler";
		Siparisler.DataSource = bindingSource1;
		Siparisler.Expanded = false;
		Siparisler.Level = 5;
		Siparisler.Name = "Siparisler";
		Siparisler.ReportPrintOptions.PrintOnEmptyDataSource = false;
		Siparis_Listesi.Controls.AddRange(new XRControl[1] { xrTable9 });
		Siparis_Listesi.Expanded = false;
		Siparis_Listesi.HeightF = 15f;
		Siparis_Listesi.Name = "Siparis_Listesi";
		xrTable9.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable9.EvenStyleName = "xrControlStyle1";
		xrTable9.LocationFloat = new PointFloat(0f, 0f);
		xrTable9.Name = "xrTable9";
		xrTable9.OddStyleName = "xrControlStyle2";
		xrTable9.Rows.AddRange(new XRTableRow[1] { xrTableRow17 });
		xrTable9.SizeF = new SizeF(788.9999f, 15f);
		xrTable9.StylePriority.UseBorders = false;
		xrTableRow17.Cells.AddRange(new XRTableCell[5] { xrTableCell55, xrTableCell56, xrTableCell57, xrTableCell58, xrTableCell59 });
		xrTableRow17.Name = "xrTableRow17";
		xrTableRow17.Weight = 1.0;
		xrTableCell55.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.siparisler.cari_kodu")
		});
		xrTableCell55.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell55.Name = "xrTableCell55";
		xrTableCell55.StylePriority.UseFont = false;
		xrTableCell55.StylePriority.UseTextAlignment = false;
		xrTableCell55.Text = "xrTableCell4";
		xrTableCell55.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell55.Weight = 0.8750001418040636;
		xrTableCell55.WordWrap = false;
		xrTableCell56.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.siparisler.cari_ismi")
		});
		xrTableCell56.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell56.Name = "xrTableCell56";
		xrTableCell56.StylePriority.UseFont = false;
		xrTableCell56.StylePriority.UseTextAlignment = false;
		xrTableCell56.Text = "xrTableCell5";
		xrTableCell56.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell56.Weight = 5.2616922333420035;
		xrTableCell56.WordWrap = false;
		xrTableCell57.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.siparisler.evrak_seri")
		});
		xrTableCell57.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell57.Name = "xrTableCell57";
		xrTableCell57.StylePriority.UseFont = false;
		xrTableCell57.StylePriority.UseTextAlignment = false;
		xrTableCell57.Text = "xrTableCell38";
		xrTableCell57.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell57.Weight = 0.5179498218925283;
		xrTableCell57.WordWrap = false;
		xrTableCell58.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.siparisler.evrak_sira")
		});
		xrTableCell58.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell58.Name = "xrTableCell58";
		xrTableCell58.StylePriority.UseFont = false;
		xrTableCell58.StylePriority.UseTextAlignment = false;
		xrTableCell58.Text = "xrTableCell68";
		xrTableCell58.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell58.Weight = 0.4895233317147476;
		xrTableCell59.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.siparisler.tutar", "{0:c}")
		});
		xrTableCell59.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell59.Name = "xrTableCell59";
		xrTableCell59.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell59.StylePriority.UseFont = false;
		xrTableCell59.StylePriority.UsePadding = false;
		xrTableCell59.StylePriority.UseTextAlignment = false;
		xrTableCell59.Text = "xrTableCell69";
		xrTableCell59.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell59.Weight = 0.7458345415710396;
		Siparisler_Baslik.Controls.AddRange(new XRControl[2] { xrTable8, xrLabel6 });
		Siparisler_Baslik.Expanded = false;
		Siparisler_Baslik.HeightF = 55.91666f;
		Siparisler_Baslik.KeepTogether = true;
		Siparisler_Baslik.Name = "Siparisler_Baslik";
		xrTable8.LocationFloat = new PointFloat(3.051758E-05f, 30.91666f);
		xrTable8.Name = "xrTable8";
		xrTable8.Rows.AddRange(new XRTableRow[1] { xrTableRow16 });
		xrTable8.SizeF = new SizeF(788.9999f, 25f);
		xrTableRow16.Cells.AddRange(new XRTableCell[5] { xrTableCell50, xrTableCell51, xrTableCell52, xrTableCell53, xrTableCell54 });
		xrTableRow16.Name = "xrTableRow16";
		xrTableRow16.Weight = 1.0;
		xrTableCell50.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell50.Borders = BorderSide.All;
		xrTableCell50.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell50.ForeColor = Color.Black;
		xrTableCell50.Name = "xrTableCell50";
		xrTableCell50.StylePriority.UseBackColor = false;
		xrTableCell50.StylePriority.UseBorders = false;
		xrTableCell50.StylePriority.UseFont = false;
		xrTableCell50.StylePriority.UseForeColor = false;
		xrTableCell50.StylePriority.UseTextAlignment = false;
		xrTableCell50.Text = "Cari kodu";
		xrTableCell50.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell50.Weight = 0.3922992281211122;
		xrTableCell51.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell51.Borders = BorderSide.All;
		xrTableCell51.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell51.ForeColor = Color.Black;
		xrTableCell51.FormattingRules.Add(formattingRule1);
		xrTableCell51.Name = "xrTableCell51";
		xrTableCell51.StylePriority.UseBackColor = false;
		xrTableCell51.StylePriority.UseBorders = false;
		xrTableCell51.StylePriority.UseFont = false;
		xrTableCell51.StylePriority.UseForeColor = false;
		xrTableCell51.StylePriority.UseTextAlignment = false;
		xrTableCell51.Text = "Cari ünvan";
		xrTableCell51.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell51.Weight = 2.359039049812795;
		xrTableCell52.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell52.Borders = BorderSide.All;
		xrTableCell52.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell52.ForeColor = Color.Black;
		xrTableCell52.Name = "xrTableCell52";
		xrTableCell52.StylePriority.UseBackColor = false;
		xrTableCell52.StylePriority.UseBorders = false;
		xrTableCell52.StylePriority.UseFont = false;
		xrTableCell52.StylePriority.UseForeColor = false;
		xrTableCell52.StylePriority.UseTextAlignment = false;
		xrTableCell52.Text = "Seri";
		xrTableCell52.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell52.Weight = 0.2322190942155582;
		xrTableCell53.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell53.Borders = BorderSide.All;
		xrTableCell53.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell53.ForeColor = Color.Black;
		xrTableCell53.Name = "xrTableCell53";
		xrTableCell53.StylePriority.UseBackColor = false;
		xrTableCell53.StylePriority.UseBorders = false;
		xrTableCell53.StylePriority.UseFont = false;
		xrTableCell53.StylePriority.UseForeColor = false;
		xrTableCell53.StylePriority.UseTextAlignment = false;
		xrTableCell53.Text = "Sıra";
		xrTableCell53.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell53.Weight = 0.2194740149618578;
		xrTableCell54.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell54.Borders = BorderSide.All;
		xrTableCell54.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell54.ForeColor = Color.Black;
		xrTableCell54.Name = "xrTableCell54";
		xrTableCell54.StylePriority.UseBackColor = false;
		xrTableCell54.StylePriority.UseBorders = false;
		xrTableCell54.StylePriority.UseFont = false;
		xrTableCell54.StylePriority.UseForeColor = false;
		xrTableCell54.StylePriority.UseTextAlignment = false;
		xrTableCell54.Text = "Tutar";
		xrTableCell54.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell54.Weight = 0.33438896200977;
		xrLabel6.BackColor = Color.Silver;
		xrLabel6.Borders = BorderSide.Left | BorderSide.Top | BorderSide.Right;
		xrLabel6.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrLabel6.ForeColor = Color.Black;
		xrLabel6.LocationFloat = new PointFloat(0f, 10f);
		xrLabel6.Name = "xrLabel6";
		xrLabel6.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel6.SizeF = new SizeF(789f, 20.91666f);
		xrLabel6.StylePriority.UseBackColor = false;
		xrLabel6.StylePriority.UseBorders = false;
		xrLabel6.StylePriority.UseFont = false;
		xrLabel6.StylePriority.UseForeColor = false;
		xrLabel6.StylePriority.UseTextAlignment = false;
		xrLabel6.Text = "SİPARİŞLER";
		xrLabel6.TextAlignment = TextAlignment.MiddleCenter;
		Faturalar.Bands.AddRange(new Band[2] { Fatura_Listesi, Faturalar_Baslik });
		Faturalar.DataMember = "bolgeler.temsilciler.gunler.faturalar";
		Faturalar.DataSource = bindingSource1;
		Faturalar.Expanded = false;
		Faturalar.Level = 6;
		Faturalar.Name = "Faturalar";
		Faturalar.ReportPrintOptions.PrintOnEmptyDataSource = false;
		Fatura_Listesi.Controls.AddRange(new XRControl[1] { xrTable11 });
		Fatura_Listesi.HeightF = 15f;
		Fatura_Listesi.Name = "Fatura_Listesi";
		xrTable11.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable11.EvenStyleName = "xrControlStyle1";
		xrTable11.LocationFloat = new PointFloat(0f, 0f);
		xrTable11.Name = "xrTable11";
		xrTable11.OddStyleName = "xrControlStyle2";
		xrTable11.Rows.AddRange(new XRTableRow[1] { xrTableRow19 });
		xrTable11.SizeF = new SizeF(788.9999f, 15f);
		xrTable11.StylePriority.UseBorders = false;
		xrTableRow19.Cells.AddRange(new XRTableCell[5] { xrTableCell65, xrTableCell66, xrTableCell67, xrTableCell68, xrTableCell69 });
		xrTableRow19.Name = "xrTableRow19";
		xrTableRow19.Weight = 1.0;
		xrTableCell65.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.faturalar.cari_kodu")
		});
		xrTableCell65.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell65.Name = "xrTableCell65";
		xrTableCell65.StylePriority.UseFont = false;
		xrTableCell65.StylePriority.UseTextAlignment = false;
		xrTableCell65.Text = "xrTableCell4";
		xrTableCell65.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell65.Weight = 0.8750001418040636;
		xrTableCell65.WordWrap = false;
		xrTableCell66.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.faturalar.cari_ismi")
		});
		xrTableCell66.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell66.Name = "xrTableCell66";
		xrTableCell66.StylePriority.UseFont = false;
		xrTableCell66.StylePriority.UseTextAlignment = false;
		xrTableCell66.Text = "xrTableCell5";
		xrTableCell66.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell66.Weight = 5.2616922333420035;
		xrTableCell66.WordWrap = false;
		xrTableCell67.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.faturalar.evrak_seri")
		});
		xrTableCell67.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell67.Name = "xrTableCell67";
		xrTableCell67.StylePriority.UseFont = false;
		xrTableCell67.StylePriority.UseTextAlignment = false;
		xrTableCell67.Text = "xrTableCell38";
		xrTableCell67.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell67.Weight = 0.5179492115409132;
		xrTableCell67.WordWrap = false;
		xrTableCell68.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.faturalar.evrak_sira")
		});
		xrTableCell68.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell68.Name = "xrTableCell68";
		xrTableCell68.StylePriority.UseFont = false;
		xrTableCell68.StylePriority.UseTextAlignment = false;
		xrTableCell68.Text = "xrTableCell68";
		xrTableCell68.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell68.Weight = 0.4895239420663626;
		xrTableCell69.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.faturalar.tutar", "{0:c}")
		});
		xrTableCell69.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell69.Name = "xrTableCell69";
		xrTableCell69.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell69.StylePriority.UseFont = false;
		xrTableCell69.StylePriority.UsePadding = false;
		xrTableCell69.StylePriority.UseTextAlignment = false;
		xrTableCell69.Text = "xrTableCell69";
		xrTableCell69.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell69.Weight = 0.7458345415710396;
		Faturalar_Baslik.Controls.AddRange(new XRControl[2] { xrLabel8, xrTable10 });
		Faturalar_Baslik.Expanded = false;
		Faturalar_Baslik.HeightF = 55.91666f;
		Faturalar_Baslik.KeepTogether = true;
		Faturalar_Baslik.Name = "Faturalar_Baslik";
		xrLabel8.BackColor = Color.Silver;
		xrLabel8.Borders = BorderSide.Left | BorderSide.Top | BorderSide.Right;
		xrLabel8.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrLabel8.ForeColor = Color.Black;
		xrLabel8.LocationFloat = new PointFloat(0f, 10f);
		xrLabel8.Name = "xrLabel8";
		xrLabel8.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel8.SizeF = new SizeF(789f, 20.91666f);
		xrLabel8.StylePriority.UseBackColor = false;
		xrLabel8.StylePriority.UseBorders = false;
		xrLabel8.StylePriority.UseFont = false;
		xrLabel8.StylePriority.UseForeColor = false;
		xrLabel8.StylePriority.UseTextAlignment = false;
		xrLabel8.Text = "FATURALAR";
		xrLabel8.TextAlignment = TextAlignment.MiddleCenter;
		xrTable10.LocationFloat = new PointFloat(0f, 30.91666f);
		xrTable10.Name = "xrTable10";
		xrTable10.Rows.AddRange(new XRTableRow[1] { xrTableRow18 });
		xrTable10.SizeF = new SizeF(788.9999f, 25f);
		xrTableRow18.Cells.AddRange(new XRTableCell[5] { xrTableCell60, xrTableCell61, xrTableCell62, xrTableCell63, xrTableCell64 });
		xrTableRow18.Name = "xrTableRow18";
		xrTableRow18.Weight = 1.0;
		xrTableCell60.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell60.Borders = BorderSide.All;
		xrTableCell60.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell60.ForeColor = Color.Black;
		xrTableCell60.Name = "xrTableCell60";
		xrTableCell60.StylePriority.UseBackColor = false;
		xrTableCell60.StylePriority.UseBorders = false;
		xrTableCell60.StylePriority.UseFont = false;
		xrTableCell60.StylePriority.UseForeColor = false;
		xrTableCell60.StylePriority.UseTextAlignment = false;
		xrTableCell60.Text = "Cari kodu";
		xrTableCell60.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell60.Weight = 0.3922992281211122;
		xrTableCell61.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell61.Borders = BorderSide.All;
		xrTableCell61.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell61.ForeColor = Color.Black;
		xrTableCell61.FormattingRules.Add(formattingRule1);
		xrTableCell61.Name = "xrTableCell61";
		xrTableCell61.StylePriority.UseBackColor = false;
		xrTableCell61.StylePriority.UseBorders = false;
		xrTableCell61.StylePriority.UseFont = false;
		xrTableCell61.StylePriority.UseForeColor = false;
		xrTableCell61.StylePriority.UseTextAlignment = false;
		xrTableCell61.Text = "Cari ünvan";
		xrTableCell61.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell61.Weight = 2.3590393234592297;
		xrTableCell62.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell62.Borders = BorderSide.All;
		xrTableCell62.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell62.ForeColor = Color.Black;
		xrTableCell62.Name = "xrTableCell62";
		xrTableCell62.StylePriority.UseBackColor = false;
		xrTableCell62.StylePriority.UseBorders = false;
		xrTableCell62.StylePriority.UseFont = false;
		xrTableCell62.StylePriority.UseForeColor = false;
		xrTableCell62.StylePriority.UseTextAlignment = false;
		xrTableCell62.Text = "Seri";
		xrTableCell62.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell62.Weight = 0.23221882056912346;
		xrTableCell63.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell63.Borders = BorderSide.All;
		xrTableCell63.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell63.ForeColor = Color.Black;
		xrTableCell63.Name = "xrTableCell63";
		xrTableCell63.StylePriority.UseBackColor = false;
		xrTableCell63.StylePriority.UseBorders = false;
		xrTableCell63.StylePriority.UseFont = false;
		xrTableCell63.StylePriority.UseForeColor = false;
		xrTableCell63.StylePriority.UseTextAlignment = false;
		xrTableCell63.Text = "Sıra";
		xrTableCell63.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell63.Weight = 0.2194740149618578;
		xrTableCell64.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell64.Borders = BorderSide.All;
		xrTableCell64.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell64.ForeColor = Color.Black;
		xrTableCell64.Name = "xrTableCell64";
		xrTableCell64.StylePriority.UseBackColor = false;
		xrTableCell64.StylePriority.UseBorders = false;
		xrTableCell64.StylePriority.UseFont = false;
		xrTableCell64.StylePriority.UseForeColor = false;
		xrTableCell64.StylePriority.UseTextAlignment = false;
		xrTableCell64.Text = "Tutar";
		xrTableCell64.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell64.Weight = 0.33438896200977;
		Tahsilatlar.Bands.AddRange(new Band[2] { Tahsilat_Listesi, Tahsilatlar_baslik });
		Tahsilatlar.DataMember = "bolgeler.temsilciler.gunler.tahsilatlar";
		Tahsilatlar.DataSource = bindingSource1;
		Tahsilatlar.Expanded = false;
		Tahsilatlar.Level = 7;
		Tahsilatlar.Name = "Tahsilatlar";
		Tahsilatlar.ReportPrintOptions.PrintOnEmptyDataSource = false;
		Tahsilat_Listesi.Controls.AddRange(new XRControl[1] { xrTable13 });
		Tahsilat_Listesi.HeightF = 15f;
		Tahsilat_Listesi.Name = "Tahsilat_Listesi";
		xrTable13.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable13.EvenStyleName = "xrControlStyle1";
		xrTable13.LocationFloat = new PointFloat(0f, 0f);
		xrTable13.Name = "xrTable13";
		xrTable13.OddStyleName = "xrControlStyle2";
		xrTable13.Rows.AddRange(new XRTableRow[1] { xrTableRow21 });
		xrTable13.SizeF = new SizeF(788.9999f, 15f);
		xrTable13.StylePriority.UseBorders = false;
		xrTableRow21.Cells.AddRange(new XRTableCell[7] { xrTableCell77, xrTableCell78, xrTableCell79, xrTableCell116, xrTableCell117, xrTableCell118, xrTableCell119 });
		xrTableRow21.Name = "xrTableRow21";
		xrTableRow21.Weight = 1.0;
		xrTableCell77.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.tahsilatlar.cari_kodu")
		});
		xrTableCell77.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell77.Name = "xrTableCell77";
		xrTableCell77.StylePriority.UseFont = false;
		xrTableCell77.StylePriority.UseTextAlignment = false;
		xrTableCell77.Text = "xrTableCell4";
		xrTableCell77.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell77.Weight = 0.8750001418040636;
		xrTableCell77.WordWrap = false;
		xrTableCell78.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.tahsilatlar.cari_ismi")
		});
		xrTableCell78.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell78.Name = "xrTableCell78";
		xrTableCell78.StylePriority.UseFont = false;
		xrTableCell78.StylePriority.UseTextAlignment = false;
		xrTableCell78.Text = "xrTableCell5";
		xrTableCell78.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell78.Weight = 2.6158587226133525;
		xrTableCell78.WordWrap = false;
		xrTableCell79.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.tahsilatlar.aciklama")
		});
		xrTableCell79.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell79.Name = "xrTableCell79";
		xrTableCell79.StylePriority.UseFont = false;
		xrTableCell79.StylePriority.UseTextAlignment = false;
		xrTableCell79.Text = "xrTableCell38";
		xrTableCell79.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell79.Weight = 2.27747641716959;
		xrTableCell79.WordWrap = false;
		xrTableCell116.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.tahsilatlar.tipi_kisa")
		});
		xrTableCell116.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell116.Name = "xrTableCell116";
		xrTableCell116.StylePriority.UseFont = false;
		xrTableCell116.StylePriority.UseTextAlignment = false;
		xrTableCell116.Text = "xrTableCell68";
		xrTableCell116.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell116.Weight = 0.36835760738414236;
		xrTableCell117.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.tahsilatlar.evrak_seri")
		});
		xrTableCell117.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell117.Name = "xrTableCell117";
		xrTableCell117.StylePriority.UseFont = false;
		xrTableCell117.StylePriority.UseTextAlignment = false;
		xrTableCell117.Text = "xrTableCell93";
		xrTableCell117.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell117.Weight = 0.5179492569472359;
		xrTableCell118.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.tahsilatlar.evrak_sira")
		});
		xrTableCell118.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell118.Name = "xrTableCell118";
		xrTableCell118.StylePriority.UseFont = false;
		xrTableCell118.StylePriority.UseTextAlignment = false;
		xrTableCell118.Text = "xrTableCell92";
		xrTableCell118.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell118.Weight = 0.48952460353818883;
		xrTableCell119.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.tahsilatlar.tutar", "{0:c}")
		});
		xrTableCell119.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell119.Name = "xrTableCell119";
		xrTableCell119.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell119.StylePriority.UseFont = false;
		xrTableCell119.StylePriority.UsePadding = false;
		xrTableCell119.StylePriority.UseTextAlignment = false;
		xrTableCell119.Text = "xrTableCell69";
		xrTableCell119.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell119.Weight = 0.7458333208678092;
		Tahsilatlar_baslik.Controls.AddRange(new XRControl[2] { xrTable12, xrLabel33 });
		Tahsilatlar_baslik.HeightF = 55.91666f;
		Tahsilatlar_baslik.KeepTogether = true;
		Tahsilatlar_baslik.Name = "Tahsilatlar_baslik";
		xrTable12.LocationFloat = new PointFloat(0f, 30.91666f);
		xrTable12.Name = "xrTable12";
		xrTable12.Rows.AddRange(new XRTableRow[1] { xrTableRow20 });
		xrTable12.SizeF = new SizeF(788.9999f, 25f);
		xrTableRow20.Cells.AddRange(new XRTableCell[7] { xrTableCell70, xrTableCell71, xrTableCell72, xrTableCell73, xrTableCell74, xrTableCell75, xrTableCell76 });
		xrTableRow20.Name = "xrTableRow20";
		xrTableRow20.Weight = 1.0;
		xrTableCell70.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell70.Borders = BorderSide.All;
		xrTableCell70.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell70.ForeColor = Color.Black;
		xrTableCell70.Name = "xrTableCell70";
		xrTableCell70.StylePriority.UseBackColor = false;
		xrTableCell70.StylePriority.UseBorders = false;
		xrTableCell70.StylePriority.UseFont = false;
		xrTableCell70.StylePriority.UseForeColor = false;
		xrTableCell70.StylePriority.UseTextAlignment = false;
		xrTableCell70.Text = "Cari kodu";
		xrTableCell70.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell70.Weight = 0.3922992281211122;
		xrTableCell71.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell71.Borders = BorderSide.All;
		xrTableCell71.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell71.ForeColor = Color.Black;
		xrTableCell71.FormattingRules.Add(formattingRule1);
		xrTableCell71.Name = "xrTableCell71";
		xrTableCell71.StylePriority.UseBackColor = false;
		xrTableCell71.StylePriority.UseBorders = false;
		xrTableCell71.StylePriority.UseFont = false;
		xrTableCell71.StylePriority.UseForeColor = false;
		xrTableCell71.StylePriority.UseTextAlignment = false;
		xrTableCell71.Text = "Cari ünvan";
		xrTableCell71.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell71.Weight = 1.1728003624868826;
		xrTableCell72.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell72.Borders = BorderSide.All;
		xrTableCell72.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell72.ForeColor = Color.Black;
		xrTableCell72.Name = "xrTableCell72";
		xrTableCell72.StylePriority.UseBackColor = false;
		xrTableCell72.StylePriority.UseBorders = false;
		xrTableCell72.StylePriority.UseFont = false;
		xrTableCell72.StylePriority.UseForeColor = false;
		xrTableCell72.StylePriority.UseTextAlignment = false;
		xrTableCell72.Text = "Açıklama";
		xrTableCell72.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell72.Weight = 1.021088678618558;
		xrTableCell73.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell73.Borders = BorderSide.All;
		xrTableCell73.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell73.ForeColor = Color.Black;
		xrTableCell73.Name = "xrTableCell73";
		xrTableCell73.StylePriority.UseBackColor = false;
		xrTableCell73.StylePriority.UseBorders = false;
		xrTableCell73.StylePriority.UseFont = false;
		xrTableCell73.StylePriority.UseForeColor = false;
		xrTableCell73.StylePriority.UseTextAlignment = false;
		xrTableCell73.Text = "Tipi";
		xrTableCell73.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell73.Weight = 0.16515027199874666;
		xrTableCell74.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell74.Borders = BorderSide.All;
		xrTableCell74.Font = new Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell74.Name = "xrTableCell74";
		xrTableCell74.StylePriority.UseBackColor = false;
		xrTableCell74.StylePriority.UseBorders = false;
		xrTableCell74.StylePriority.UseFont = false;
		xrTableCell74.StylePriority.UseTextAlignment = false;
		xrTableCell74.Text = "Seri";
		xrTableCell74.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell74.Weight = 0.2322196475488696;
		xrTableCell75.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell75.Borders = BorderSide.All;
		xrTableCell75.Font = new Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell75.Name = "xrTableCell75";
		xrTableCell75.StylePriority.UseBackColor = false;
		xrTableCell75.StylePriority.UseBorders = false;
		xrTableCell75.StylePriority.UseFont = false;
		xrTableCell75.StylePriority.UseTextAlignment = false;
		xrTableCell75.Text = "Sıra";
		xrTableCell75.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell75.Weight = 0.21947350446355318;
		xrTableCell76.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell76.Borders = BorderSide.All;
		xrTableCell76.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell76.ForeColor = Color.Black;
		xrTableCell76.Name = "xrTableCell76";
		xrTableCell76.StylePriority.UseBackColor = false;
		xrTableCell76.StylePriority.UseBorders = false;
		xrTableCell76.StylePriority.UseFont = false;
		xrTableCell76.StylePriority.UseForeColor = false;
		xrTableCell76.StylePriority.UseTextAlignment = false;
		xrTableCell76.Text = "Tutar";
		xrTableCell76.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell76.Weight = 0.3343886558833712;
		xrLabel33.BackColor = Color.Silver;
		xrLabel33.Borders = BorderSide.Left | BorderSide.Top | BorderSide.Right;
		xrLabel33.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrLabel33.ForeColor = Color.Black;
		xrLabel33.LocationFloat = new PointFloat(0f, 10f);
		xrLabel33.Name = "xrLabel33";
		xrLabel33.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel33.SizeF = new SizeF(789f, 20.91666f);
		xrLabel33.StylePriority.UseBackColor = false;
		xrLabel33.StylePriority.UseBorders = false;
		xrLabel33.StylePriority.UseFont = false;
		xrLabel33.StylePriority.UseForeColor = false;
		xrLabel33.StylePriority.UseTextAlignment = false;
		xrLabel33.Text = "TAHSİLATLAR";
		xrLabel33.TextAlignment = TextAlignment.MiddleCenter;
		Masraflar.Bands.AddRange(new Band[2] { Masraf_Listesi, Masraflar_Baslik });
		Masraflar.DataMember = "bolgeler.temsilciler.gunler.masraflar";
		Masraflar.DataSource = bindingSource1;
		Masraflar.Expanded = false;
		Masraflar.Level = 8;
		Masraflar.Name = "Masraflar";
		Masraflar.ReportPrintOptions.PrintOnEmptyDataSource = false;
		Masraf_Listesi.Controls.AddRange(new XRControl[1] { xrTable15 });
		Masraf_Listesi.HeightF = 15f;
		Masraf_Listesi.Name = "Masraf_Listesi";
		xrTable15.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable15.EvenStyleName = "xrControlStyle1";
		xrTable15.LocationFloat = new PointFloat(0f, 0f);
		xrTable15.Name = "xrTable15";
		xrTable15.OddStyleName = "xrControlStyle2";
		xrTable15.Rows.AddRange(new XRTableRow[1] { xrTableRow23 });
		xrTable15.SizeF = new SizeF(788.9999f, 15f);
		xrTable15.StylePriority.UseBorders = false;
		xrTableRow23.Cells.AddRange(new XRTableCell[6] { xrTableCell86, xrTableCell87, xrTableCell88, xrTableCell89, xrTableCell90, xrTableCell91 });
		xrTableRow23.Name = "xrTableRow23";
		xrTableRow23.Weight = 1.0;
		xrTableCell86.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.masraflar.masraf_kodu")
		});
		xrTableCell86.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell86.Name = "xrTableCell86";
		xrTableCell86.StylePriority.UseFont = false;
		xrTableCell86.StylePriority.UseTextAlignment = false;
		xrTableCell86.Text = "xrTableCell4";
		xrTableCell86.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell86.Weight = 0.8750001418040636;
		xrTableCell86.WordWrap = false;
		xrTableCell87.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.masraflar.masraf_ismi")
		});
		xrTableCell87.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell87.Name = "xrTableCell87";
		xrTableCell87.StylePriority.UseFont = false;
		xrTableCell87.StylePriority.UseTextAlignment = false;
		xrTableCell87.Text = "xrTableCell5";
		xrTableCell87.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell87.Weight = 2.6158587226133525;
		xrTableCell87.WordWrap = false;
		xrTableCell88.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.masraflar.aciklama")
		});
		xrTableCell88.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell88.Name = "xrTableCell88";
		xrTableCell88.StylePriority.UseFont = false;
		xrTableCell88.StylePriority.UseTextAlignment = false;
		xrTableCell88.Text = "xrTableCell38";
		xrTableCell88.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell88.Weight = 2.645833382541759;
		xrTableCell88.WordWrap = false;
		xrTableCell89.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.masraflar.evrak_seri")
		});
		xrTableCell89.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell89.Name = "xrTableCell89";
		xrTableCell89.StylePriority.UseFont = false;
		xrTableCell89.StylePriority.UseTextAlignment = false;
		xrTableCell89.Text = "xrTableCell93";
		xrTableCell89.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell89.Weight = 0.5179498989592096;
		xrTableCell90.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.masraflar.evrak_sira")
		});
		xrTableCell90.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell90.Name = "xrTableCell90";
		xrTableCell90.StylePriority.UseFont = false;
		xrTableCell90.StylePriority.UseTextAlignment = false;
		xrTableCell90.Text = "xrTableCell92";
		xrTableCell90.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell90.Weight = 0.48952460353818883;
		xrTableCell91.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "bolgeler.temsilciler.gunler.masraflar.tutar", "{0:c}")
		});
		xrTableCell91.Font = new Font("Arial Narrow", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTableCell91.Name = "xrTableCell91";
		xrTableCell91.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell91.StylePriority.UseFont = false;
		xrTableCell91.StylePriority.UsePadding = false;
		xrTableCell91.StylePriority.UseTextAlignment = false;
		xrTableCell91.Text = "xrTableCell69";
		xrTableCell91.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell91.Weight = 0.7458333208678092;
		Masraflar_Baslik.Controls.AddRange(new XRControl[2] { xrLabel34, xrTable14 });
		Masraflar_Baslik.Expanded = false;
		Masraflar_Baslik.HeightF = 55.91666f;
		Masraflar_Baslik.KeepTogether = true;
		Masraflar_Baslik.Name = "Masraflar_Baslik";
		xrLabel34.BackColor = Color.Silver;
		xrLabel34.Borders = BorderSide.Left | BorderSide.Top | BorderSide.Right;
		xrLabel34.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrLabel34.ForeColor = Color.Black;
		xrLabel34.LocationFloat = new PointFloat(0f, 10f);
		xrLabel34.Name = "xrLabel34";
		xrLabel34.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel34.SizeF = new SizeF(789f, 20.91666f);
		xrLabel34.StylePriority.UseBackColor = false;
		xrLabel34.StylePriority.UseBorders = false;
		xrLabel34.StylePriority.UseFont = false;
		xrLabel34.StylePriority.UseForeColor = false;
		xrLabel34.StylePriority.UseTextAlignment = false;
		xrLabel34.Text = "MASRAFLAR";
		xrLabel34.TextAlignment = TextAlignment.MiddleCenter;
		xrTable14.LocationFloat = new PointFloat(0f, 30.91666f);
		xrTable14.Name = "xrTable14";
		xrTable14.Rows.AddRange(new XRTableRow[1] { xrTableRow22 });
		xrTable14.SizeF = new SizeF(788.9999f, 25f);
		xrTableRow22.Cells.AddRange(new XRTableCell[6] { xrTableCell80, xrTableCell81, xrTableCell82, xrTableCell83, xrTableCell84, xrTableCell85 });
		xrTableRow22.Name = "xrTableRow22";
		xrTableRow22.Weight = 1.0;
		xrTableCell80.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell80.Borders = BorderSide.All;
		xrTableCell80.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell80.ForeColor = Color.Black;
		xrTableCell80.Name = "xrTableCell80";
		xrTableCell80.StylePriority.UseBackColor = false;
		xrTableCell80.StylePriority.UseBorders = false;
		xrTableCell80.StylePriority.UseFont = false;
		xrTableCell80.StylePriority.UseForeColor = false;
		xrTableCell80.StylePriority.UseTextAlignment = false;
		xrTableCell80.Text = "Masraf kodu";
		xrTableCell80.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell80.Weight = 0.3922992281211122;
		xrTableCell81.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell81.Borders = BorderSide.All;
		xrTableCell81.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell81.ForeColor = Color.Black;
		xrTableCell81.FormattingRules.Add(formattingRule1);
		xrTableCell81.Name = "xrTableCell81";
		xrTableCell81.StylePriority.UseBackColor = false;
		xrTableCell81.StylePriority.UseBorders = false;
		xrTableCell81.StylePriority.UseFont = false;
		xrTableCell81.StylePriority.UseForeColor = false;
		xrTableCell81.StylePriority.UseTextAlignment = false;
		xrTableCell81.Text = "Masraf adı";
		xrTableCell81.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell81.Weight = 1.1728003624868826;
		xrTableCell82.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell82.Borders = BorderSide.All;
		xrTableCell82.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell82.ForeColor = Color.Black;
		xrTableCell82.Name = "xrTableCell82";
		xrTableCell82.StylePriority.UseBackColor = false;
		xrTableCell82.StylePriority.UseBorders = false;
		xrTableCell82.StylePriority.UseFont = false;
		xrTableCell82.StylePriority.UseForeColor = false;
		xrTableCell82.StylePriority.UseTextAlignment = false;
		xrTableCell82.Text = "Açıklama";
		xrTableCell82.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell82.Weight = 1.1862386804225507;
		xrTableCell83.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell83.Borders = BorderSide.All;
		xrTableCell83.Font = new Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell83.Name = "xrTableCell83";
		xrTableCell83.StylePriority.UseBackColor = false;
		xrTableCell83.StylePriority.UseBorders = false;
		xrTableCell83.StylePriority.UseFont = false;
		xrTableCell83.StylePriority.UseTextAlignment = false;
		xrTableCell83.Text = "Seri";
		xrTableCell83.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell83.Weight = 0.2322199177436236;
		xrTableCell84.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell84.Borders = BorderSide.All;
		xrTableCell84.Font = new Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell84.Name = "xrTableCell84";
		xrTableCell84.StylePriority.UseBackColor = false;
		xrTableCell84.StylePriority.UseBorders = false;
		xrTableCell84.StylePriority.UseFont = false;
		xrTableCell84.StylePriority.UseTextAlignment = false;
		xrTableCell84.Text = "Sıra";
		xrTableCell84.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell84.Weight = 0.21947350446355318;
		xrTableCell85.BackColor = Color.FromArgb(224, 224, 224);
		xrTableCell85.Borders = BorderSide.All;
		xrTableCell85.Font = new Font("Arial", 10f, FontStyle.Bold);
		xrTableCell85.ForeColor = Color.Black;
		xrTableCell85.Name = "xrTableCell85";
		xrTableCell85.StylePriority.UseBackColor = false;
		xrTableCell85.StylePriority.UseBorders = false;
		xrTableCell85.StylePriority.UseFont = false;
		xrTableCell85.StylePriority.UseForeColor = false;
		xrTableCell85.StylePriority.UseTextAlignment = false;
		xrTableCell85.Text = "Tutar";
		xrTableCell85.TextAlignment = TextAlignment.MiddleCenter;
		xrTableCell85.Weight = 0.3343886558833712;
		base.Bands.AddRange(new Band[4] { Genel_Ozet, TopMargin, BottomMargin, Bolgeler });
		base.DataSource = bindingSource1;
		base.FormattingRuleSheet.AddRange(new FormattingRule[1] { formattingRule1 });
		base.Margins = new Margins(21, 17, 22, 55);
		base.PageHeight = 1169;
		base.PageWidth = 827;
		base.PaperKind = PaperKind.A4;
		base.StyleSheet.AddRange(new XRControlStyle[2] { xrControlStyle1, xrControlStyle2 });
		base.Version = "13.1";
		((ISupportInitialize)xrTable30).EndInit();
		((ISupportInitialize)xrTable31).EndInit();
		((ISupportInitialize)xrTable28).EndInit();
		((ISupportInitialize)xrTable29).EndInit();
		((ISupportInitialize)xrTable27).EndInit();
		((ISupportInitialize)xrTable26).EndInit();
		((ISupportInitialize)xrTable22).EndInit();
		((ISupportInitialize)xrTable23).EndInit();
		((ISupportInitialize)xrTable21).EndInit();
		((ISupportInitialize)xrTable20).EndInit();
		((ISupportInitialize)xrTable17).EndInit();
		((ISupportInitialize)xrTable16).EndInit();
		((ISupportInitialize)xrTable3).EndInit();
		((ISupportInitialize)xrTable25).EndInit();
		((ISupportInitialize)xrTable24).EndInit();
		((ISupportInitialize)bindingSource1).EndInit();
		((ISupportInitialize)xrTable19).EndInit();
		((ISupportInitialize)xrTable18).EndInit();
		((ISupportInitialize)xrTable5).EndInit();
		((ISupportInitialize)xrTable4).EndInit();
		((ISupportInitialize)xrTable7).EndInit();
		((ISupportInitialize)xrTable6).EndInit();
		((ISupportInitialize)xrTable2).EndInit();
		((ISupportInitialize)xrTable1).EndInit();
		((ISupportInitialize)xrTable9).EndInit();
		((ISupportInitialize)xrTable8).EndInit();
		((ISupportInitialize)xrTable11).EndInit();
		((ISupportInitialize)xrTable10).EndInit();
		((ISupportInitialize)xrTable13).EndInit();
		((ISupportInitialize)xrTable12).EndInit();
		((ISupportInitialize)xrTable15).EndInit();
		((ISupportInitialize)xrTable14).EndInit();
		((ISupportInitialize)this).EndInit();
	}
}
