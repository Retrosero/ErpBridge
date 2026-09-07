using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace Fora.App.Win.Mikro.Service;

[ServiceContract]
public interface ILisansYoneticisiService
{
	[OperationContract]
	[WebGet(UriTemplate = "IsAlive", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message IsAlive();

	[OperationContract]
	[WebGet(UriTemplate = "GetErrorLog")]
	Stream GetErrorLog();

	[OperationContract]
	[WebGet(UriTemplate = "GetLisans/{modul}", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
	Message GetLisans(string modul);
}
