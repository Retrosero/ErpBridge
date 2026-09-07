using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Fora.App.Win.Mikro.SR_EDI;

[DebuggerStepThrough]
[GeneratedCode("System.ServiceModel", "4.0.0.0")]
public class EDIServiceSoapClient : ClientBase<EDIServiceSoap>, EDIServiceSoap
{
	public EDIServiceSoapClient()
	{
	}

	public EDIServiceSoapClient(string endpointConfigurationName)
		: base(endpointConfigurationName)
	{
	}

	public EDIServiceSoapClient(string endpointConfigurationName, string remoteAddress)
		: base(endpointConfigurationName, remoteAddress)
	{
	}

	public EDIServiceSoapClient(string endpointConfigurationName, EndpointAddress remoteAddress)
		: base(endpointConfigurationName, remoteAddress)
	{
	}

	public EDIServiceSoapClient(Binding binding, EndpointAddress remoteAddress)
		: base(binding, remoteAddress)
	{
	}

	public RetRes Send(string Name, string Password, string PartnerIln, string DocumentType, string DocumentVersion, string DocumentStandard, string DocumentTest, string ControlNumber, string DocumentContent, int Timeout)
	{
		return base.Channel.Send(Name, Password, PartnerIln, DocumentType, DocumentVersion, DocumentStandard, DocumentTest, ControlNumber, DocumentContent, Timeout);
	}

	public RetRes ListPB(string Name, string Password, string PartnerIln, string DocumentType, string DocumentVersion, string DocumentStandard, string DocumentTest, string DateFrom, string DateTo, string ItemFrom, string ItemTo, int Timeout)
	{
		return base.Channel.ListPB(Name, Password, PartnerIln, DocumentType, DocumentVersion, DocumentStandard, DocumentTest, DateFrom, DateTo, ItemFrom, ItemTo, Timeout);
	}

	public RetRes ListPBEx(string Name, string Password, string PartnerIln, string DocumentType, string DocumentVersion, string DocumentStandard, string DocumentTest, string DateFrom, string DateTo, string ItemFrom, string ItemTo, string OrderBy, int Timeout)
	{
		return base.Channel.ListPBEx(Name, Password, PartnerIln, DocumentType, DocumentVersion, DocumentStandard, DocumentTest, DateFrom, DateTo, ItemFrom, ItemTo, OrderBy, Timeout);
	}

	public RetRes ListMB(string Name, string Password, string PartnerIln, string DocumentType, string DocumentVersion, string DocumentStandard, string DocumentTest, string DocumentStatus, int Timeout)
	{
		return base.Channel.ListMB(Name, Password, PartnerIln, DocumentType, DocumentVersion, DocumentStandard, DocumentTest, DocumentStatus, Timeout);
	}

	public RetRes ListMBEx(string Name, string Password, string PartnerIln, string DocumentType, string DocumentVersion, string DocumentStandard, string DocumentTest, string DateFrom, string DateTo, string ItemFrom, string ItemTo, string DocumentStatus, int Timeout)
	{
		return base.Channel.ListMBEx(Name, Password, PartnerIln, DocumentType, DocumentVersion, DocumentStandard, DocumentTest, DateFrom, DateTo, ItemFrom, ItemTo, DocumentStatus, Timeout);
	}

	public RetRes Receive(string Name, string Password, string PartnerIln, string DocumentType, string TrackingId, string DocumentStandard, string ChangeDocumentStatus, int Timeout)
	{
		return base.Channel.Receive(Name, Password, PartnerIln, DocumentType, TrackingId, DocumentStandard, ChangeDocumentStatus, Timeout);
	}

	public RetRes Relationships(string Name, string Password, int Timeout)
	{
		return base.Channel.Relationships(Name, Password, Timeout);
	}

	public RetRes ChangeDocumentStatus(string Name, string Password, string TrackingId, string Status)
	{
		return base.Channel.ChangeDocumentStatus(Name, Password, TrackingId, Status);
	}
}
