using System.Collections.Generic;

namespace Fora.Mikro.Interface;

public interface IPrintToDotMatrix
{
	bool InitPrinter(string aygitismi, int YaziciSatirlarArasiBeklemeSuresi);

	bool Print(List<string> yazilacaktext);

	string GetErrorMessage();

	void Dispose();
}
