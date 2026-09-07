namespace Fora.Mikro.CariHesaplar;

public class CariDetayOlusturTask
{
	public delegate void CariDetayBilgileriBittiHandler(object sender, CariDetayBilgileri caridetaybilgileri);

	private CariDetayBilgileri _caridetaybilgileri;

	public event CariDetayBilgileriBittiHandler OnCariDetayBilgileriBitti;

	public void Baslat(CariDetayBilgileri caridetaybilgileri)
	{
		_caridetaybilgileri = caridetaybilgileri;
		Main();
	}

	public void Main()
	{
		try
		{
			_caridetaybilgileri = CariSqlite.GetCariDetayBilgileri(AppBase.GetDbConnectionSync(), _caridetaybilgileri, AppBase._doviz_cinsi_tanimlari);
		}
		catch
		{
		}
		this.OnCariDetayBilgileriBitti(this, _caridetaybilgileri);
	}
}
