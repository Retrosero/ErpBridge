using System;
using Fora.Mikro.CariHesapHareket;

namespace Fora.Mikro.Evraklar;

public interface GenelEvrakSecimiListener
{
	void OnGenelEvrakSelected(string tag, enum_cha_evrak_tip cha_evrak_tip, int cha_RECid_RECno, Guid cha_Guid);
}
