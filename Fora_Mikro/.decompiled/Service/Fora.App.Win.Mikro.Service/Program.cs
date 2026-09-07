using System.ServiceProcess;

namespace Fora.App.Win.Mikro.Service;

internal static class Program
{
	private static void Main()
	{
		ServiceBase.Run(new ServiceBase[1]
		{
			new ForaMikroService()
		});
	}
}
