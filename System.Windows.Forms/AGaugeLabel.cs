using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms;

public class AGaugeLabel
{
	private AGauge Owner;

	private Color _Color = Color.FromKnownColor(KnownColor.WindowText);

	private string _Text;

	private Point _Position;

	private Font _Font = DefaultFont;

	private static Font DefaultFont = Control.DefaultFont;

	[Browsable(true)]
	[Category("Design")]
	[DisplayName("(Name)")]
	[Description("Instance Name.")]
	public string Name { get; set; }

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The color of the caption text.")]
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
	[Category("Appearance")]
	[Description("The text of the caption.")]
	public string Text
	{
		get
		{
			return _Text;
		}
		set
		{
			_Text = value;
			NotifyOwner();
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The position of the caption.")]
	public Point Position
	{
		get
		{
			return _Position;
		}
		set
		{
			_Position = value;
			NotifyOwner();
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("Font of Text.")]
	public Font Font
	{
		get
		{
			return _Font;
		}
		set
		{
			_Font = value;
			NotifyOwner();
		}
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

	public void ResetFont()
	{
		_Font = DefaultFont;
	}

	private bool ShouldSerializeFont()
	{
		return _Font != DefaultFont;
	}
}
