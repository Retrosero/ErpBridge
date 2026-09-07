using System.Collections.Generic;
using System.Data;
using DevExpress.XtraEditors.Repository;
using Fora.Mikro.Enumler;

namespace Fora.App.Win.Mikro.Classes;

public class DevExpressRepositoryItemGridLookupEdit
{
	public List<DevExpressRepositoryItemGridLookupEditExtended> Items;

	public DevExpressRepositoryItemGridLookupEdit()
	{
		Items = new List<DevExpressRepositoryItemGridLookupEditExtended>();
	}

	public void Init(List<enum_DevExpressRepositoryItemGridLookUpEdit> itemlist, DataSet _lookuptablolar, string tag)
	{
		foreach (enum_DevExpressRepositoryItemGridLookUpEdit item in itemlist)
		{
			DevExpressRepositoryItemGridLookupEditExtended devExpressRepositoryItemGridLookupEditExtended = new DevExpressRepositoryItemGridLookupEditExtended(tag, item);
			devExpressRepositoryItemGridLookupEditExtended.Init(_lookuptablolar);
			Items.Add(devExpressRepositoryItemGridLookupEditExtended);
		}
	}

	public RepositoryItemGridLookUpEdit FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit itemenum)
	{
		foreach (DevExpressRepositoryItemGridLookupEditExtended item in Items)
		{
			if (item._RepositoryItem.AccessibleName == itemenum.ToString())
			{
				return item._RepositoryItem;
			}
		}
		return null;
	}
}
