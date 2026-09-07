using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Fora.App.Win.Mikro.Service;

[ServiceContract]
public interface IMikroEntegrasyonServisi
{
	[OperationContract]
	[WebGet]
	FiyatDegisikleriResult FiyatDegisiklikleri(string MikroVeritabaniAdi, int fiyat_liste_no, DateTime LastUpdate);
}
