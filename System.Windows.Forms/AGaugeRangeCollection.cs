using System.Collections;

namespace System.Windows.Forms;

public class AGaugeRangeCollection : CollectionBase
{
	private AGauge Owner;

	public AGaugeRange this[int index] => (AGaugeRange)List[index];

	public AGaugeRangeCollection(AGauge sender)
	{
		Owner = sender;
	}

	public bool Contains(AGaugeRange itemType)
	{
		return List.Contains(itemType);
	}

	public int Add(AGaugeRange itemType)
	{
		itemType.SetOwner(Owner);
		if (string.IsNullOrEmpty(itemType.Name))
		{
			itemType.Name = GetUniqueName();
		}
		return List.Add(itemType);
	}

	public void Remove(AGaugeRange itemType)
	{
		List.Remove(itemType);
	}

	public void Insert(int index, AGaugeRange itemType)
	{
		itemType.SetOwner(Owner);
		if (string.IsNullOrEmpty(itemType.Name))
		{
			itemType.Name = GetUniqueName();
		}
		List.Insert(index, itemType);
	}

	public int IndexOf(AGaugeRange itemType)
	{
		return List.IndexOf(itemType);
	}

	public AGaugeRange FindByName(string name)
	{
		foreach (AGaugeRange item in List)
		{
			if (item.Name == name)
			{
				return item;
			}
		}
		return null;
	}

	protected override void OnInsert(int index, object value)
	{
		if (string.IsNullOrEmpty(((AGaugeRange)value).Name))
		{
			((AGaugeRange)value).Name = GetUniqueName();
		}
		base.OnInsert(index, value);
		((AGaugeRange)value).SetOwner(Owner);
	}

	protected override void OnRemove(int index, object value)
	{
		if (Owner != null)
		{
			Owner.RepaintControl();
		}
	}

	protected override void OnClear()
	{
		if (Owner != null)
		{
			Owner.RepaintControl();
		}
	}

	private string GetUniqueName()
	{
		int num = 1;
		while (Count != 0)
		{
			bool flag = true;
			for (int i = 0; i < Count; i++)
			{
				if (this[i].Name == "GaugeRange" + num)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				break;
			}
			num++;
		}
		return "GaugeRange" + num;
	}
}
