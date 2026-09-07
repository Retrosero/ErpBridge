using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Fora.Mikro;

public class CustomObservableCollection<T> : ObservableCollection<T>
{
	public CustomObservableCollection()
	{
	}

	public CustomObservableCollection(IEnumerable<T> items)
		: this()
	{
		foreach (T item in items)
		{
			((Collection<T>)(object)this).Add(item);
		}
	}

	public void ReportItemChange(T item)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)2, (object)item, (object)item, ((Collection<T>)(object)this).IndexOf(item));
		((ObservableCollection<T>)this).OnCollectionChanged(e);
	}
}
