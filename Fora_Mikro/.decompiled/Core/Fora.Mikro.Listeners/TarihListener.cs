using System;

namespace Fora.Mikro.Listeners;

public interface TarihListener
{
	void OnTarihChanged(DateTime tarih, string tag);
}
