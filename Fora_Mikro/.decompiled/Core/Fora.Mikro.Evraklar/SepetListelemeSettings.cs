namespace Fora.Mikro.Evraklar;

public class SepetListelemeSettings
{
	public bool YeniKayit { get; set; }

	public enum_GenelEvrakTipleri EvrakTipi { get; set; }

	public bool KdvDahilGoster { get; set; }

	public bool SepetKodGoster { get; set; }

	public bool SepetIsimGoster { get; set; }

	public bool SepetFotoGoster { get; set; }

	public bool SepetBrutFiyatGoster { get; set; }

	public bool SepetIskontoBilgisiGoster { get; set; }

	public bool SepetNetFiyatGoster { get; set; }

	public bool SepetMiktarGoster { get; set; }

	public bool SepetToplamTutarGoster { get; set; }

	public bool SepetTeslimMiktarGoster { get; set; }

	public bool SepetKalanMiktarGoster { get; set; }

	public bool SepetCekDetayGoster { get; set; }

	public bool SepetKdvBilgisiGoster { get; set; }
}
