using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace Fora.App.Win.Mikro.Service;

[ServiceContract]
public interface ITeknikTesisAdmin
{
	[OperationContract]
	[WebGet(UriTemplate = "Login/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message Login(string firmaid, string UserName, string Password);

	[OperationContract]
	[WebGet(UriTemplate = "GetBitirilecekIsler/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetBitirilecekIsler(string firmaid, string UserName, string Password);

	[OperationContract]
	[WebGet(UriTemplate = "GetStoklarSarf/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetStoklarSarf(string firmaid, string UserName, string Password);

	[OperationContract]
	[WebGet(UriTemplate = "GetEkipAtanacaklar/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetEkipAtanacaklar(string firmaid, string UserName, string Password);

	[OperationContract]
	[WebGet(UriTemplate = "BakimKabulEkipAta/{firmaid}/{UserName}/{Password}/{bkmkb_RECno}/{bkmkb_inceleyecek_ekip_kodu}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message BakimKabulEkipAta(string firmaid, string UserName, string Password, string bkmkb_RECno, string bkmkb_inceleyecek_ekip_kodu);

	[OperationContract]
	[WebGet(UriTemplate = "GetBakimKabul/{firmaid}/{UserName}/{Password}/{bkmkb_RECno}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetBakimKabul(string firmaid, string UserName, string Password, string bkmkb_RECno);

	[OperationContract]
	[WebInvoke(UriTemplate = "IsBitir/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message IsBitir(string firmaid, string UserName, string Password, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "SearchSarfStok/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message SearchSarfStok(string firmaid, string UserName, string Password, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "GetStokBase/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetStokBase(string firmaid, string UserName, string Password, Stream Content);

	[OperationContract]
	[WebGet(UriTemplate = "GetKurumGenelListV2/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetKurumGenelListV2(string firmaid, string UserName, string Password);

	[OperationContract]
	[WebInvoke(UriTemplate = "RaporBekleyenIsler/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message RaporBekleyenIsler(string firmaid, string UserName, string Password, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "RaporBekleyenBakimlar/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message RaporBekleyenBakimlar(string firmaid, string UserName, string Password, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "RaporTamamlananIsler/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message RaporTamamlananIsler(string firmaid, string UserName, string Password, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "RaporTamamlananBakimlar/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message RaporTamamlananBakimlar(string firmaid, string UserName, string Password, Stream Content);

	[OperationContract]
	[WebInvoke(UriTemplate = "RaporKullanilanStoklar/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message RaporKullanilanStoklar(string firmaid, string UserName, string Password, Stream Content);
}
