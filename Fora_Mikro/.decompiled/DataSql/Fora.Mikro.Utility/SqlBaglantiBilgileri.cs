using System.IO;
using System.Xml.Linq;

namespace Fora.Mikro.Utility;

public class SqlBaglantiBilgileri
{
	private string _SqlServer;

	private string _SqlUserName;

	private string _SqlPassword;

	public string SqlServer
	{
		get
		{
			return _SqlServer;
		}
		set
		{
			_SqlServer = value;
		}
	}

	public string SqlUserName
	{
		get
		{
			return _SqlUserName;
		}
		set
		{
			_SqlUserName = value;
		}
	}

	public string SqlPassword
	{
		get
		{
			return _SqlPassword;
		}
		set
		{
			_SqlPassword = value;
		}
	}

	public SqlBaglantiBilgileri()
	{
		_SqlServer = "";
		_SqlUserName = "";
		_SqlPassword = "";
	}

	public SqlBaglantiBilgileri(string SqlServer, string SqlUserName, string SqlPassword)
	{
		_SqlServer = SqlServer;
		_SqlUserName = SqlUserName;
		_SqlPassword = SqlPassword;
	}

	public SqlBaglantiBilgileri(string XmlFileName)
	{
		try
		{
			using StreamReader textReader = File.OpenText(XmlFileName);
			XDocument xDocument = XDocument.Load((TextReader)textReader);
			_SqlServer = xDocument.Root.Element("SqlServer").Value;
			_SqlUserName = xDocument.Root.Element("SqlUserName").Value;
			_SqlPassword = xDocument.Root.Element("SqlPassword").Value;
		}
		catch
		{
			_SqlServer = "";
			_SqlUserName = "";
			_SqlPassword = "";
		}
	}

	public bool Save(string FileName)
	{
		try
		{
			XDocument xDocument = new XDocument();
			xDocument.Add(new XComment("BaglantiAyarlari"));
			XElement xElement = new XElement("Config");
			xElement.Add(new XElement("SqlServer", _SqlServer));
			xElement.Add(new XElement("SqlUserName", _SqlUserName));
			xElement.Add(new XElement("SqlPassword", _SqlPassword));
			xDocument.Add(xElement);
			xDocument.Save(FileName);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
