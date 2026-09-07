using System.CodeDom.Compiler;
using System.ServiceModel;

namespace Fora.App.Win.Mikro.SR_EDI;

[GeneratedCode("System.ServiceModel", "4.0.0.0")]
[ServiceContract(Namespace = "http://www.comarch.com/", ConfigurationName = "SR_EDI.EDIServiceSoap")]
public interface EDIServiceSoap
{
	[OperationContract(Action = "http://www.comarch.com/Send", ReplyAction = "*")]
	[XmlSerializerFormat(SupportFaults = true)]
	RetRes Send(string Name, string Password, string PartnerIln, string DocumentType, string DocumentVersion, string DocumentStandard, string DocumentTest, string ControlNumber, string DocumentContent, int Timeout);

	[OperationContract(Action = "http://www.comarch.com/ListPB", ReplyAction = "*")]
	[XmlSerializerFormat(SupportFaults = true)]
	RetRes ListPB(string Name, string Password, string PartnerIln, string DocumentType, string DocumentVersion, string DocumentStandard, string DocumentTest, string DateFrom, string DateTo, string ItemFrom, string ItemTo, int Timeout);

	[OperationContract(Action = "http://www.comarch.com/ListPBEx", ReplyAction = "*")]
	[XmlSerializerFormat(SupportFaults = true)]
	RetRes ListPBEx(string Name, string Password, string PartnerIln, string DocumentType, string DocumentVersion, string DocumentStandard, string DocumentTest, string DateFrom, string DateTo, string ItemFrom, string ItemTo, string OrderBy, int Timeout);

	[OperationContract(Action = "http://www.comarch.com/ListMB", ReplyAction = "*")]
	[XmlSerializerFormat(SupportFaults = true)]
	RetRes ListMB(string Name, string Password, string PartnerIln, string DocumentType, string DocumentVersion, string DocumentStandard, string DocumentTest, string DocumentStatus, int Timeout);

	[OperationContract(Action = "http://www.comarch.com/ListMBEx", ReplyAction = "*")]
	[XmlSerializerFormat(SupportFaults = true)]
	RetRes ListMBEx(string Name, string Password, string PartnerIln, string DocumentType, string DocumentVersion, string DocumentStandard, string DocumentTest, string DateFrom, string DateTo, string ItemFrom, string ItemTo, string DocumentStatus, int Timeout);

	[OperationContract(Action = "http://www.comarch.com/Receive", ReplyAction = "*")]
	[XmlSerializerFormat(SupportFaults = true)]
	RetRes Receive(string Name, string Password, string PartnerIln, string DocumentType, string TrackingId, string DocumentStandard, string ChangeDocumentStatus, int Timeout);

	[OperationContract(Action = "http://www.comarch.com/Relationships", ReplyAction = "*")]
	[XmlSerializerFormat(SupportFaults = true)]
	RetRes Relationships(string Name, string Password, int Timeout);

	[OperationContract(Action = "http://www.comarch.com/ChangeDocumentStatus", ReplyAction = "*")]
	[XmlSerializerFormat(SupportFaults = true)]
	RetRes ChangeDocumentStatus(string Name, string Password, string TrackingId, string Status);
}
