using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.ServiceProcess;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Fora.App.Win.Mikro.Service;

public class ForaMikroService : ServiceBase
{
	private IContainer components;

	public ForaMikroService()
	{
		InitializeComponent();
	}

	protected override void OnStart(string[] args)
	{
		new Thread(ServisBaslat).Start();
	}

	protected override void OnStop()
	{
	}

	private static void ServisBaslat()
	{
		string text = "1400";
		string text2 = "1409";
		string text3 = "1414";
		WebServiceHost webServiceHost = new WebServiceHost(typeof(MikroService));
		ServiceHost serviceHost = new ServiceHost(typeof(MikroEntegrasyonServisi), new Uri("http://localhost:" + text3));
		try
		{
			using StreamReader textReader = File.OpenText(string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\", "data\\servisbilgileri.xml"));
			text = XDocument.Load(textReader).Root.Element("Port").Value;
		}
		catch (Exception ex)
		{
			Log_Error(ex.ToString());
		}
		try
		{
			WebHttpBinding webHttpBinding = new WebHttpBinding();
			webHttpBinding.CloseTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding.OpenTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding.ReceiveTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding.SendTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding.MaxReceivedMessageSize = 2147483647L;
			webHttpBinding.MaxBufferSize = int.MaxValue;
			XmlDictionaryReaderQuotas xmlDictionaryReaderQuotas = new XmlDictionaryReaderQuotas();
			xmlDictionaryReaderQuotas.MaxArrayLength = int.MaxValue;
			xmlDictionaryReaderQuotas.MaxStringContentLength = int.MaxValue;
			xmlDictionaryReaderQuotas.MaxBytesPerRead = int.MaxValue;
			xmlDictionaryReaderQuotas.MaxDepth = int.MaxValue;
			xmlDictionaryReaderQuotas.MaxNameTableCharCount = int.MaxValue;
			webHttpBinding.ReaderQuotas = xmlDictionaryReaderQuotas;
			WebHttpBinding webHttpBinding2 = new WebHttpBinding();
			webHttpBinding2.CloseTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding2.OpenTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding2.ReceiveTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding2.SendTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding2.MaxReceivedMessageSize = 2147483647L;
			webHttpBinding2.MaxBufferSize = int.MaxValue;
			XmlDictionaryReaderQuotas xmlDictionaryReaderQuotas2 = new XmlDictionaryReaderQuotas();
			xmlDictionaryReaderQuotas2.MaxArrayLength = int.MaxValue;
			xmlDictionaryReaderQuotas2.MaxStringContentLength = int.MaxValue;
			xmlDictionaryReaderQuotas2.MaxBytesPerRead = int.MaxValue;
			xmlDictionaryReaderQuotas2.MaxDepth = int.MaxValue;
			xmlDictionaryReaderQuotas2.MaxNameTableCharCount = int.MaxValue;
			webHttpBinding2.ReaderQuotas = xmlDictionaryReaderQuotas2;
			WebHttpBinding webHttpBinding3 = new WebHttpBinding();
			webHttpBinding3.CloseTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding3.OpenTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding3.ReceiveTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding3.SendTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding3.MaxReceivedMessageSize = 2147483647L;
			webHttpBinding3.MaxBufferSize = int.MaxValue;
			XmlDictionaryReaderQuotas xmlDictionaryReaderQuotas3 = new XmlDictionaryReaderQuotas();
			xmlDictionaryReaderQuotas3.MaxArrayLength = int.MaxValue;
			xmlDictionaryReaderQuotas3.MaxStringContentLength = int.MaxValue;
			xmlDictionaryReaderQuotas3.MaxBytesPerRead = int.MaxValue;
			xmlDictionaryReaderQuotas3.MaxDepth = int.MaxValue;
			xmlDictionaryReaderQuotas3.MaxNameTableCharCount = int.MaxValue;
			webHttpBinding3.ReaderQuotas = xmlDictionaryReaderQuotas3;
			WebHttpBinding webHttpBinding4 = new WebHttpBinding();
			webHttpBinding4.CloseTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding4.OpenTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding4.ReceiveTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding4.SendTimeout = new TimeSpan(0, 5, 0);
			webHttpBinding4.MaxReceivedMessageSize = 2147483647L;
			webHttpBinding4.MaxBufferSize = int.MaxValue;
			XmlDictionaryReaderQuotas xmlDictionaryReaderQuotas4 = new XmlDictionaryReaderQuotas();
			xmlDictionaryReaderQuotas4.MaxArrayLength = int.MaxValue;
			xmlDictionaryReaderQuotas4.MaxStringContentLength = int.MaxValue;
			xmlDictionaryReaderQuotas4.MaxBytesPerRead = int.MaxValue;
			xmlDictionaryReaderQuotas4.MaxDepth = int.MaxValue;
			xmlDictionaryReaderQuotas4.MaxNameTableCharCount = int.MaxValue;
			webHttpBinding4.ReaderQuotas = xmlDictionaryReaderQuotas4;
			BasicHttpBinding basicHttpBinding = new BasicHttpBinding();
			basicHttpBinding.CloseTimeout = new TimeSpan(0, 5, 0);
			basicHttpBinding.OpenTimeout = new TimeSpan(0, 5, 0);
			basicHttpBinding.ReceiveTimeout = new TimeSpan(0, 5, 0);
			basicHttpBinding.SendTimeout = new TimeSpan(0, 5, 0);
			basicHttpBinding.MaxReceivedMessageSize = 2147483647L;
			basicHttpBinding.MaxBufferSize = int.MaxValue;
			XmlDictionaryReaderQuotas xmlDictionaryReaderQuotas5 = new XmlDictionaryReaderQuotas();
			xmlDictionaryReaderQuotas5.MaxArrayLength = int.MaxValue;
			xmlDictionaryReaderQuotas5.MaxStringContentLength = int.MaxValue;
			xmlDictionaryReaderQuotas5.MaxBytesPerRead = int.MaxValue;
			xmlDictionaryReaderQuotas5.MaxDepth = int.MaxValue;
			xmlDictionaryReaderQuotas5.MaxNameTableCharCount = int.MaxValue;
			basicHttpBinding.ReaderQuotas = xmlDictionaryReaderQuotas5;
			webServiceHost.AddServiceEndpoint(typeof(IMikroService), webHttpBinding, "http://localhost:" + text + "/");
			webServiceHost.AddServiceEndpoint(typeof(ITeknikTesisAdmin), webHttpBinding2, "http://localhost:" + text + "/TeknikAdmin/");
			webServiceHost.AddServiceEndpoint(typeof(ITeknikTesis), webHttpBinding3, "http://localhost:" + text + "/Teknik/");
			webServiceHost.AddServiceEndpoint(typeof(ILisansYoneticisiService), webHttpBinding4, "http://localhost:" + text2 + "/");
			serviceHost.AddServiceEndpoint(typeof(IMikroEntegrasyonServisi), basicHttpBinding, "Soap");
			ServiceMetadataBehavior serviceMetadataBehavior = new ServiceMetadataBehavior();
			serviceMetadataBehavior.HttpGetEnabled = true;
			webServiceHost.Description.Behaviors.Add(serviceMetadataBehavior);
			ServiceMetadataBehavior serviceMetadataBehavior2 = new ServiceMetadataBehavior();
			serviceMetadataBehavior2.HttpGetEnabled = true;
			serviceHost.Description.Behaviors.Add(serviceMetadataBehavior2);
			ServiceThrottlingBehavior serviceThrottlingBehavior = new ServiceThrottlingBehavior();
			serviceThrottlingBehavior.MaxConcurrentCalls = 100;
			serviceThrottlingBehavior.MaxConcurrentInstances = 100;
			serviceThrottlingBehavior.MaxConcurrentSessions = 100;
			webServiceHost.Description.Behaviors.Add(serviceThrottlingBehavior);
			ServiceThrottlingBehavior serviceThrottlingBehavior2 = new ServiceThrottlingBehavior();
			serviceThrottlingBehavior2.MaxConcurrentCalls = 100;
			serviceThrottlingBehavior2.MaxConcurrentInstances = 100;
			serviceThrottlingBehavior2.MaxConcurrentSessions = 100;
			serviceHost.Description.Behaviors.Add(serviceThrottlingBehavior2);
			webServiceHost.Open();
			serviceHost.Open();
		}
		catch (CommunicationException ex2)
		{
			Log_Error(ex2.ToString());
			webServiceHost.Abort();
		}
		catch (Exception ex3)
		{
			Log_Error(ex3.ToString());
			webServiceHost.Abort();
		}
	}

	private static void Log_Error(string message)
	{
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		try
		{
			if (!Directory.Exists(text + "data"))
			{
				Directory.CreateDirectory(text + "data");
			}
			if (!Directory.Exists(text + "data\\logs"))
			{
				Directory.CreateDirectory(text + "data\\logs");
			}
			if (!File.Exists(text + "data\\logs\\errorlog.log"))
			{
				FileStream fileStream = File.Create(text + "data\\logs\\errorlog.log");
				fileStream.Close();
				fileStream.Dispose();
			}
			StreamWriter streamWriter = File.AppendText(text + "data\\logs\\errorlog.log");
			streamWriter.WriteLine(message);
			streamWriter.Close();
			streamWriter.Dispose();
		}
		catch
		{
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
		base.ServiceName = "ForaLM";
	}
}
