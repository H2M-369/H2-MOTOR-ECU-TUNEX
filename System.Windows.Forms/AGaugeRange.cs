using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms;

public class AGaugeRange
{
	private AGauge Owner;

	private Color _Color;

	private float _StartValue;

	private float _EndValue;

	private int _InnerRadius = 1;

	private int _OuterRadius = 2;

	[Browsable(true)]
	[Category("Design")]
	[DisplayName("(Name)")]
	[Description("Instance Name.")]
	public string Name { get; set; }

	[Browsable(false)]
	public bool InRange { get; set; }

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The color of the range.")]
	public Color Color
	{
		get
		{
			return _Color;
		}
		set
		{
			_Color = value;
			NotifyOwner();
		}
	}

	[Browsable(true)]
	[Category("Limits")]
	[Description("The start value of the range, must be less than RangeEndValue.")]
	public float StartValue
	{
		get
		{
			return _StartValue;
		}
		set
		{
			if (value < _EndValue)
			{
				_StartValue = value;
				NotifyOwner();
			}
		}
	}

	[Browsable(true)]
	[Category("Limits")]
	[Description("The end value of the range. Must be greater than RangeStartValue.")]
	public float EndValue
	{
		get
		{
			return _EndValue;
		}
		set
		{
			if (value > _StartValue)
			{
				_EndValue = value;
				NotifyOwner();
			}
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The inner radius of the range.")]
	public int InnerRadius
	{
		get
		{
			return _InnerRadius;
		}
		set
		{
			if (value > 0)
			{
				_InnerRadius = value;
				NotifyOwner();
			}
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The outer radius of the range.")]
	public int OuterRadius
	{
		get
		{
			return _OuterRadius;
		}
		set
		{
			if (value > 0)
			{
				_OuterRadius = value;
				NotifyOwner();
			}
		}
	}

	public AGaugeRange()
	{
	}

	public AGaugeRange(Color color, float startValue, float endValue, int innerRadius, int outerRadius)
	{
		Color = color;
		_StartValue = startValue;
		_EndValue = endValue;
		InnerRadius = innerRadius;
		OuterRadius = outerRadius;
	}

	[Browsable(false)]
	public void SetOwner(AGauge value)
	{
		Owner = value;
	}

	private void NotifyOwner()
	{
		if (Owner != null)
		{
			Owner.RepaintControl();
		}
	}
}
