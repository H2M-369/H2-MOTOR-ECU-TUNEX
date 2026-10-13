using System.Collections;

namespace System.Windows.Forms;

public class AGaugeLabelCollection : CollectionBase
{
	private AGauge Owner;

	public AGaugeLabel this[int index] => (AGaugeLabel)List[index];

	public AGaugeLabelCollection(AGauge sender)
	{
		Owner = sender;
	}

	public bool Contains(AGaugeLabel itemType)
	{
		return List.Contains(itemType);
	}

	public int Add(AGaugeLabel itemType)
	{
		itemType.SetOwner(Owner);
		if (string.IsNullOrEmpty(itemType.Name))
		{
			itemType.Name = GetUniqueName();
		}
		return List.Add(itemType);
	}

	public void Remove(AGaugeLabel itemType)
	{
		List.Remove(itemType);
	}

	public void Insert(int index, AGaugeLabel itemType)
	{
		itemType.SetOwner(Owner);
		if (string.IsNullOrEmpty(itemType.Name))
		{
			itemType.Name = GetUniqueName();
		}
		List.Insert(index, itemType);
	}

	public int IndexOf(AGaugeLabel itemType)
	{
		return List.IndexOf(itemType);
	}

	public AGaugeLabel FindByName(string name)
	{
		foreach (AGaugeLabel item in List)
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
		if (string.IsNullOrEmpty(((AGaugeLabel)value).Name))
		{
			((AGaugeLabel)value).Name = GetUniqueName();
		}
		base.OnInsert(index, value);
		((AGaugeLabel)value).SetOwner(Owner);
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
			for (int i = 0; i < Count; i++)
			{
				if (!(this[i].Name == "GaugeLabel" + num))
				{
					return "GaugeLabel" + num;
				}
			}
			num++;
		}
		return "GaugeLabel" + num;
	}
}
