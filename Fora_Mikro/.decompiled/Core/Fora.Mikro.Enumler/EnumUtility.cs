using System;

namespace Fora.Mikro.Enumler;

public class EnumUtility
{
	public static string EnumToLocalizedString(Enum en)
	{
		string text = null;
		Type type = en.GetType();
		string name = Enum.GetName(type, en);
		if (name != null)
		{
			text = AppResource.ResourceManager.GetString(type.Name + "_" + name);
			if (text == null)
			{
				text = name;
			}
			return text;
		}
		return null;
	}
}
