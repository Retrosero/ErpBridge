using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Fora.App.Win.Mikro.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Fora.App.Win.Mikro.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap cari_rota_disi_ziyaret => (Bitmap)ResourceManager.GetObject("cari_rota_disi_ziyaret", resourceCulture);

	internal static Bitmap cari_ziyaret_edilmemis => (Bitmap)ResourceManager.GetObject("cari_ziyaret_edilmemis", resourceCulture);

	internal static Bitmap cari_ziyaret_edilmis => (Bitmap)ResourceManager.GetObject("cari_ziyaret_edilmis", resourceCulture);

	internal static Bitmap derya_dagitim_logo => (Bitmap)ResourceManager.GetObject("derya_dagitim_logo", resourceCulture);

	internal static Bitmap kurulum_1 => (Bitmap)ResourceManager.GetObject("kurulum_1", resourceCulture);

	internal static Bitmap kurulum_2 => (Bitmap)ResourceManager.GetObject("kurulum_2", resourceCulture);

	internal static Bitmap kurulum_3 => (Bitmap)ResourceManager.GetObject("kurulum_3", resourceCulture);

	internal static Bitmap kurulum_4 => (Bitmap)ResourceManager.GetObject("kurulum_4", resourceCulture);

	internal static Bitmap kurulum_5 => (Bitmap)ResourceManager.GetObject("kurulum_5", resourceCulture);

	internal static Bitmap kurulum_6 => (Bitmap)ResourceManager.GetObject("kurulum_6", resourceCulture);

	internal static Bitmap kurulum_7 => (Bitmap)ResourceManager.GetObject("kurulum_7", resourceCulture);

	internal static Bitmap ziyaret_baslangic => (Bitmap)ResourceManager.GetObject("ziyaret_baslangic", resourceCulture);

	internal static Bitmap ziyaret_bitis => (Bitmap)ResourceManager.GetObject("ziyaret_bitis", resourceCulture);

	internal Resources()
	{
	}
}
