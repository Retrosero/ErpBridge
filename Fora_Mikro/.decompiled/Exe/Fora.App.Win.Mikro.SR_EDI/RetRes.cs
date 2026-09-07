using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Fora.App.Win.Mikro.SR_EDI;

[Serializable]
[GeneratedCode("System.Xml", "4.7.2102.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "http://www.comarch.com/")]
public class RetRes : INotifyPropertyChanged
{
	private string resField;

	private string cntField;

	[XmlElement(Order = 0)]
	public string Res
	{
		get
		{
			return resField;
		}
		set
		{
			resField = value;
			RaisePropertyChanged("Res");
		}
	}

	[XmlElement(Order = 1)]
	public string Cnt
	{
		get
		{
			return cntField;
		}
		set
		{
			cntField = value;
			RaisePropertyChanged("Cnt");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
