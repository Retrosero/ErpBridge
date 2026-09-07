namespace Fora.Mikro.Utility;

public class MailBilgileri
{
	private string _FromAddress;

	private string _FromPassWord;

	private string _SmtpAddress;

	private int _SmtpPort;

	private string _ToAddress;

	public string FromAddress
	{
		get
		{
			return _FromAddress;
		}
		set
		{
			_FromAddress = value;
		}
	}

	public string FromPassWord
	{
		get
		{
			return _FromPassWord;
		}
		set
		{
			_FromPassWord = value;
		}
	}

	public string SmtpAddress
	{
		get
		{
			return _SmtpAddress;
		}
		set
		{
			_SmtpAddress = value;
		}
	}

	public int SmtpPort
	{
		get
		{
			return _SmtpPort;
		}
		set
		{
			_SmtpPort = value;
		}
	}

	public string ToAddress
	{
		get
		{
			return _ToAddress;
		}
		set
		{
			_ToAddress = value;
		}
	}

	public MailBilgileri()
	{
		_FromAddress = "";
		_FromPassWord = "";
		_SmtpAddress = "";
		_SmtpPort = 587;
		_ToAddress = "";
	}

	public MailBilgileri(string FromAddress, string FromPassWord, string SmtpAddress, int SmtpPort, string ToAddress)
	{
		_FromAddress = FromAddress;
		_FromPassWord = FromPassWord;
		_SmtpAddress = SmtpAddress;
		_SmtpPort = SmtpPort;
		_ToAddress = ToAddress;
	}
}
