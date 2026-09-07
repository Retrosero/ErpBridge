using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace Fora.App.Win.Mikro.Service;

[ServiceContract]
public interface ITeknikTesis
{
	[OperationContract]
	[WebGet(UriTemplate = "Login/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message Login(string firmaid, string UserName, string Password);

	[OperationContract]
	[WebGet(UriTemplate = "GetCihazGruplari/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetCihazGruplari(string firmaid, string UserName, string Password);

	[OperationContract]
	[WebGet(UriTemplate = "GetCihazGenelList/{firmaid}/{UserName}/{Password}/{cg_kodu}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetCihazGenelList(string firmaid, string UserName, string Password, string cg_kodu);

	[OperationContract]
	[WebGet(UriTemplate = "GetCihazSorunuGrubuList/{firmaid}/{UserName}/{Password}/{chz_serino}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetCihazSorunuGrubuList(string firmaid, string UserName, string Password, string chz_serino);

	[OperationContract]
	[WebGet(UriTemplate = "GetCihazSorunuList/{firmaid}/{UserName}/{Password}/{chz_serino}/{agr_kodu}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetCihazSorunuList(string firmaid, string UserName, string Password, string chz_serino, string agr_kodu);

	[OperationContract]
	[WebInvoke(UriTemplate = "SaveBakimTalebi/{firmaid}/{UserName}/{Password}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message SaveBakimTalebi(string firmaid, string UserName, string Password, Stream Content);

	[OperationContract]
	[WebGet(UriTemplate = "GetCihazDurumu/{firmaid}/{cari_vergino}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetCihazDurumu(string firmaid, string cari_vergino);
}
