using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace System.Windows.Forms;

[ToolboxBitmap(typeof(AGauge), "AGauge.AGauge.bmp")]
[DefaultEvent("ValueInRangeChanged")]
[Description("Displays a value on an analog gauge. Raises an event if the value enters one of the definable ranges.")]
public class AGauge : Control
{
	private float fontBoundY1;

	private float fontBoundY2;

	private Bitmap gaugeBitmap;

	private bool drawGaugeBackground = true;

	private float m_value;

	private Point m_Center = new Point(100, 100);

	private float m_MinValue = -100f;

	private float m_MaxValue = 400f;

	private Color m_BaseArcColor = Color.Gray;

	private int m_BaseArcRadius = 80;

	private int m_BaseArcStart = 135;

	private int m_BaseArcSweep = 270;

	private int m_BaseArcWidth = 2;

	private Color m_ScaleLinesInterColor = Color.Black;

	private int m_ScaleLinesInterInnerRadius = 73;

	private int m_ScaleLinesInterOuterRadius = 80;

	private int m_ScaleLinesInterWidth = 1;

	private int m_ScaleLinesMinorTicks = 9;

	private Color m_ScaleLinesMinorColor = Color.Gray;

	private int m_ScaleLinesMinorInnerRadius = 75;

	private int m_ScaleLinesMinorOuterRadius = 80;

	private int m_ScaleLinesMinorWidth = 1;

	private float m_ScaleLinesMajorStepValue = 50f;

	private Color m_ScaleLinesMajorColor = Color.Black;

	private int m_ScaleLinesMajorInnerRadius = 70;

	private int m_ScaleLinesMajorOuterRadius = 80;

	private int m_ScaleLinesMajorWidth = 2;

	private int m_ScaleNumbersRadius = 95;

	private Color m_ScaleNumbersColor = Color.Black;

	private string m_ScaleNumbersFormat;

	private int m_ScaleNumbersStartScaleLine;

	private int m_ScaleNumbersStepScaleLines = 1;

	private int m_ScaleNumbersRotation;

	private NeedleType m_NeedleType;

	private int m_NeedleRadius = 80;

	private AGaugeNeedleColor m_NeedleColor1 = AGaugeNeedleColor.Gray;

	private Color m_NeedleColor2 = Color.DimGray;

	private int m_NeedleWidth = 2;

	private AGaugeRangeCollection _GaugeRanges;

	private AGaugeLabelCollection _GaugeLabels;

	private IContainer components = null;

	public new bool AllowDrop
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public new bool AutoSize
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public new bool ForeColor
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public new bool ImeMode
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public override Color BackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			base.BackColor = value;
			drawGaugeBackground = true;
			Refresh();
		}
	}

	public override Font Font
	{
		get
		{
			return base.Font;
		}
		set
		{
			base.Font = value;
			drawGaugeBackground = true;
			Refresh();
		}
	}

	public override ImageLayout BackgroundImageLayout
	{
		get
		{
			return base.BackgroundImageLayout;
		}
		set
		{
			base.BackgroundImageLayout = value;
			drawGaugeBackground = true;
			Refresh();
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("Gauge value.")]
	public float Value
	{
		get
		{
			return m_value;
		}
		set
		{
			value = Math.Min(Math.Max(value, m_MinValue), m_MaxValue);
			if (m_value == value)
			{
				return;
			}
			m_value = value;
			OnValueChanged();
			if (DesignMode)
			{
				drawGaugeBackground = true;
			}
			foreach (AGaugeRange gaugeRange in _GaugeRanges)
			{
				if (m_value >= gaugeRange.StartValue && m_value <= gaugeRange.EndValue)
				{
					if (!gaugeRange.InRange)
					{
						gaugeRange.InRange = true;
						OnValueInRangeChanged(gaugeRange, m_value);
					}
				}
				else if (gaugeRange.InRange)
				{
					gaugeRange.InRange = false;
					OnValueInRangeChanged(gaugeRange, m_value);
				}
			}
			Refresh();
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("Gauge Ranges.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public AGaugeRangeCollection GaugeRanges => _GaugeRanges;

	[Browsable(true)]
	[Category("AGauge")]
	[Description("Gauge Labels.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public AGaugeLabelCollection GaugeLabels => _GaugeLabels;

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The center of the gauge (in the control's client area).")]
	public Point Center
	{
		get
		{
			return m_Center;
		}
		set
		{
			if (m_Center != value)
			{
				m_Center = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The color of the base arc.")]
	public Color BaseArcColor
	{
		get
		{
			return m_BaseArcColor;
		}
		set
		{
			if (m_BaseArcColor != value)
			{
				m_BaseArcColor = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The radius of the base arc.")]
	public int BaseArcRadius
	{
		get
		{
			return m_BaseArcRadius;
		}
		set
		{
			if (m_BaseArcRadius != value)
			{
				m_BaseArcRadius = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The start angle of the base arc.")]
	public int BaseArcStart
	{
		get
		{
			return m_BaseArcStart;
		}
		set
		{
			if (m_BaseArcStart != value)
			{
				m_BaseArcStart = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The sweep angle of the base arc.")]
	public int BaseArcSweep
	{
		get
		{
			return m_BaseArcSweep;
		}
		set
		{
			if (m_BaseArcSweep != value)
			{
				m_BaseArcSweep = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The width of the base arc.")]
	public int BaseArcWidth
	{
		get
		{
			return m_BaseArcWidth;
		}
		set
		{
			if (m_BaseArcWidth != value)
			{
				m_BaseArcWidth = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The minimum value to show on the scale.")]
	public float MinValue
	{
		get
		{
			return m_MinValue;
		}
		set
		{
			if (m_MinValue != value && value < m_MaxValue)
			{
				m_MinValue = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The maximum value to show on the scale.")]
	public float MaxValue
	{
		get
		{
			return m_MaxValue;
		}
		set
		{
			if (m_MaxValue != value && value > m_MinValue)
			{
				m_MaxValue = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The color of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
	public Color ScaleLinesInterColor
	{
		get
		{
			return m_ScaleLinesInterColor;
		}
		set
		{
			if (m_ScaleLinesInterColor != value)
			{
				m_ScaleLinesInterColor = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The inner radius of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
	public int ScaleLinesInterInnerRadius
	{
		get
		{
			return m_ScaleLinesInterInnerRadius;
		}
		set
		{
			if (m_ScaleLinesInterInnerRadius != value)
			{
				m_ScaleLinesInterInnerRadius = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The outer radius of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
	public int ScaleLinesInterOuterRadius
	{
		get
		{
			return m_ScaleLinesInterOuterRadius;
		}
		set
		{
			if (m_ScaleLinesInterOuterRadius != value)
			{
				m_ScaleLinesInterOuterRadius = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The width of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
	public int ScaleLinesInterWidth
	{
		get
		{
			return m_ScaleLinesInterWidth;
		}
		set
		{
			if (m_ScaleLinesInterWidth != value)
			{
				m_ScaleLinesInterWidth = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The number of minor scale lines.")]
	public int ScaleLinesMinorTicks
	{
		get
		{
			return m_ScaleLinesMinorTicks;
		}
		set
		{
			if (m_ScaleLinesMinorTicks != value)
			{
				m_ScaleLinesMinorTicks = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The color of the minor scale lines.")]
	public Color ScaleLinesMinorColor
	{
		get
		{
			return m_ScaleLinesMinorColor;
		}
		set
		{
			if (m_ScaleLinesMinorColor != value)
			{
				m_ScaleLinesMinorColor = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The inner radius of the minor scale lines.")]
	public int ScaleLinesMinorInnerRadius
	{
		get
		{
			return m_ScaleLinesMinorInnerRadius;
		}
		set
		{
			if (m_ScaleLinesMinorInnerRadius != value)
			{
				m_ScaleLinesMinorInnerRadius = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The outer radius of the minor scale lines.")]
	public int ScaleLinesMinorOuterRadius
	{
		get
		{
			return m_ScaleLinesMinorOuterRadius;
		}
		set
		{
			if (m_ScaleLinesMinorOuterRadius != value)
			{
				m_ScaleLinesMinorOuterRadius = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The width of the minor scale lines.")]
	public int ScaleLinesMinorWidth
	{
		get
		{
			return m_ScaleLinesMinorWidth;
		}
		set
		{
			if (m_ScaleLinesMinorWidth != value)
			{
				m_ScaleLinesMinorWidth = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The step value of the major scale lines.")]
	public float ScaleLinesMajorStepValue
	{
		get
		{
			return m_ScaleLinesMajorStepValue;
		}
		set
		{
			if (m_ScaleLinesMajorStepValue != value && value > 0f)
			{
				m_ScaleLinesMajorStepValue = Math.Max(Math.Min(value, m_MaxValue), m_MinValue);
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The color of the major scale lines.")]
	public Color ScaleLinesMajorColor
	{
		get
		{
			return m_ScaleLinesMajorColor;
		}
		set
		{
			if (m_ScaleLinesMajorColor != value)
			{
				m_ScaleLinesMajorColor = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The inner radius of the major scale lines.")]
	public int ScaleLinesMajorInnerRadius
	{
		get
		{
			return m_ScaleLinesMajorInnerRadius;
		}
		set
		{
			if (m_ScaleLinesMajorInnerRadius != value)
			{
				m_ScaleLinesMajorInnerRadius = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The outer radius of the major scale lines.")]
	public int ScaleLinesMajorOuterRadius
	{
		get
		{
			return m_ScaleLinesMajorOuterRadius;
		}
		set
		{
			if (m_ScaleLinesMajorOuterRadius != value)
			{
				m_ScaleLinesMajorOuterRadius = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The width of the major scale lines.")]
	public int ScaleLinesMajorWidth
	{
		get
		{
			return m_ScaleLinesMajorWidth;
		}
		set
		{
			if (m_ScaleLinesMajorWidth != value)
			{
				m_ScaleLinesMajorWidth = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The radius of the scale numbers.")]
	public int ScaleNumbersRadius
	{
		get
		{
			return m_ScaleNumbersRadius;
		}
		set
		{
			if (m_ScaleNumbersRadius != value)
			{
				m_ScaleNumbersRadius = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The color of the scale numbers.")]
	public Color ScaleNumbersColor
	{
		get
		{
			return m_ScaleNumbersColor;
		}
		set
		{
			if (m_ScaleNumbersColor != value)
			{
				m_ScaleNumbersColor = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The format of the scale numbers.")]
	public string ScaleNumbersFormat
	{
		get
		{
			return m_ScaleNumbersFormat;
		}
		set
		{
			if (m_ScaleNumbersFormat != value)
			{
				m_ScaleNumbersFormat = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The number of the scale line to start writing numbers next to.")]
	public int ScaleNumbersStartScaleLine
	{
		get
		{
			return m_ScaleNumbersStartScaleLine;
		}
		set
		{
			if (m_ScaleNumbersStartScaleLine != value)
			{
				m_ScaleNumbersStartScaleLine = Math.Max(value, 1);
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The number of scale line steps for writing numbers.")]
	public int ScaleNumbersStepScaleLines
	{
		get
		{
			return m_ScaleNumbersStepScaleLines;
		}
		set
		{
			if (m_ScaleNumbersStepScaleLines != value)
			{
				m_ScaleNumbersStepScaleLines = Math.Max(value, 1);
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The angle relative to the tangent of the base arc at a scale line that is used to rotate numbers. set to 0 for no rotation or e.g. set to 90.")]
	public int ScaleNumbersRotation
	{
		get
		{
			return m_ScaleNumbersRotation;
		}
		set
		{
			if (m_ScaleNumbersRotation != value)
			{
				m_ScaleNumbersRotation = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The type of the needle, currently only type 0 and 1 are supported. Type 0 looks nicers but if you experience performance problems you might consider using type 1.")]
	public NeedleType NeedleType
	{
		get
		{
			return m_NeedleType;
		}
		set
		{
			if (m_NeedleType != value)
			{
				m_NeedleType = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The radius of the needle.")]
	public int NeedleRadius
	{
		get
		{
			return m_NeedleRadius;
		}
		set
		{
			if (m_NeedleRadius != value)
			{
				m_NeedleRadius = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The first color of the needle.")]
	public AGaugeNeedleColor NeedleColor1
	{
		get
		{
			return m_NeedleColor1;
		}
		set
		{
			if (m_NeedleColor1 != value)
			{
				m_NeedleColor1 = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The second color of the needle.")]
	public Color NeedleColor2
	{
		get
		{
			return m_NeedleColor2;
		}
		set
		{
			if (m_NeedleColor2 != value)
			{
				m_NeedleColor2 = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Browsable(true)]
	[Category("AGauge")]
	[Description("The width of the needle.")]
	public int NeedleWidth
	{
		get
		{
			return m_NeedleWidth;
		}
		set
		{
			if (m_NeedleWidth != value)
			{
				m_NeedleWidth = value;
				drawGaugeBackground = true;
				Refresh();
			}
		}
	}

	[Description("This event is raised when gauge value changed.")]
	public event EventHandler ValueChanged;

	[Description("This event is raised if the value is entering or leaving defined range.")]
	public event EventHandler<ValueInRangeChangedEventArgs> ValueInRangeChanged;

	private void OnValueChanged()
	{
		ValueChanged?.Invoke(this, null);
	}

	private void OnValueInRangeChanged(AGaugeRange range, float value)
	{
		ValueInRangeChanged?.Invoke(this, new ValueInRangeChangedEventArgs(range, value, range.InRange));
	}

	public AGauge()
	{
		InitializeComponent();
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		_GaugeRanges = new AGaugeRangeCollection(this);
		_GaugeLabels = new AGaugeLabelCollection(this);
		Size = new Size(205, 180);
	}

	private void FindFontBounds()
	{
		SolidBrush solidBrush = new SolidBrush(Color.White);
		SolidBrush brush = new SolidBrush(Color.Black);
		Bitmap image = new Bitmap(5, 5);
		Graphics graphics = Graphics.FromImage(image);
		SizeF sizeF = graphics.MeasureString("0123456789", Font, -1, StringFormat.GenericTypographic);
		image = new Bitmap((int)sizeF.Width, (int)sizeF.Height);
		graphics = Graphics.FromImage(image);
		graphics.FillRectangle(solidBrush, 0f, 0f, sizeF.Width, sizeF.Height);
		graphics.DrawString("0123456789", Font, brush, 0f, 0f, StringFormat.GenericTypographic);
		fontBoundY1 = 0f;
		fontBoundY2 = 0f;
		int i = 0;
		bool flag = false;
		for (; i < image.Height; i++)
		{
			if (flag)
			{
				break;
			}
			for (int j = 0; j < image.Width; j++)
			{
				if (flag)
				{
					break;
				}
				if (image.GetPixel(j, i) != solidBrush.Color)
				{
					fontBoundY1 = i;
					flag = true;
				}
			}
		}
		i = image.Height - 1;
		flag = false;
		while (0 < i && !flag)
		{
			for (int j = 0; j < image.Width; j++)
			{
				if (flag)
				{
					break;
				}
				if (image.GetPixel(j, i) != solidBrush.Color)
				{
					fontBoundY2 = i;
					flag = true;
				}
			}
			i--;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public void RepaintControl()
	{
		drawGaugeBackground = true;
		Refresh();
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (Width < 10 || Height < 10)
		{
			return;
		}
		if (drawGaugeBackground)
		{
			drawGaugeBackground = false;
			FindFontBounds();
			gaugeBitmap = new Bitmap(Width, Height, e.Graphics);
			Graphics graphics = Graphics.FromImage(gaugeBitmap);
			graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
			if (BackgroundImage != null)
			{
				switch (BackgroundImageLayout)
				{
				case ImageLayout.Center:
					graphics.DrawImageUnscaled(BackgroundImage, Width / 2 - BackgroundImage.Width / 2, Height / 2 - BackgroundImage.Height / 2);
					break;
				case ImageLayout.None:
					graphics.DrawImageUnscaled(BackgroundImage, 0, 0);
					break;
				case ImageLayout.Stretch:
					graphics.DrawImage(BackgroundImage, 0, 0, Width, Height);
					break;
				case ImageLayout.Tile:
				{
					int i = 0;
					int num = 0;
					for (; i < Width; i += BackgroundImage.Width)
					{
						for (num = 0; num < Height; num += BackgroundImage.Height)
						{
							graphics.DrawImageUnscaled(BackgroundImage, i, num);
						}
					}
					break;
				}
				case ImageLayout.Zoom:
					if ((float)(BackgroundImage.Width / Width) < (float)(BackgroundImage.Height / Height))
					{
						graphics.DrawImage(BackgroundImage, 0, 0, Height, Height);
					}
					else
					{
						graphics.DrawImage(BackgroundImage, 0, 0, Width, Width);
					}
					break;
				}
			}
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			GraphicsPath graphicsPath = new GraphicsPath();
			foreach (AGaugeRange gaugeRange in _GaugeRanges)
			{
				if (gaugeRange.EndValue > gaugeRange.StartValue)
				{
					float startAngle = (float)m_BaseArcStart + (gaugeRange.StartValue - m_MinValue) * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue);
					float sweepAngle = (gaugeRange.EndValue - gaugeRange.StartValue) * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue);
					graphicsPath.Reset();
					graphicsPath.AddPie(new Rectangle(m_Center.X - gaugeRange.OuterRadius, m_Center.Y - gaugeRange.OuterRadius, 2 * gaugeRange.OuterRadius, 2 * gaugeRange.OuterRadius), startAngle, sweepAngle);
					graphicsPath.Reverse();
					graphicsPath.AddPie(new Rectangle(m_Center.X - gaugeRange.InnerRadius, m_Center.Y - gaugeRange.InnerRadius, 2 * gaugeRange.InnerRadius, 2 * gaugeRange.InnerRadius), startAngle, sweepAngle);
					graphicsPath.Reverse();
					graphics.SetClip(graphicsPath);
					graphics.FillPie(new SolidBrush(gaugeRange.Color), new Rectangle(m_Center.X - gaugeRange.OuterRadius, m_Center.Y - gaugeRange.OuterRadius, 2 * gaugeRange.OuterRadius, 2 * gaugeRange.OuterRadius), startAngle, sweepAngle);
				}
			}
			graphics.SetClip(ClientRectangle);
			if (m_BaseArcRadius > 0)
			{
				graphics.DrawArc(new Pen(m_BaseArcColor, m_BaseArcWidth), new Rectangle(m_Center.X - m_BaseArcRadius, m_Center.Y - m_BaseArcRadius, 2 * m_BaseArcRadius, 2 * m_BaseArcRadius), m_BaseArcStart, m_BaseArcSweep);
			}
			string text = "";
			float num2 = 0f;
			int num3 = 0;
			while (num2 <= m_MaxValue - m_MinValue)
			{
				text = (m_MinValue + num2).ToString(m_ScaleNumbersFormat);
				graphics.ResetTransform();
				SizeF sizeF = graphics.MeasureString(text, Font, -1, StringFormat.GenericTypographic);
				graphicsPath.Reset();
				graphicsPath.AddEllipse(new Rectangle(m_Center.X - m_ScaleLinesMajorOuterRadius, m_Center.Y - m_ScaleLinesMajorOuterRadius, 2 * m_ScaleLinesMajorOuterRadius, 2 * m_ScaleLinesMajorOuterRadius));
				graphicsPath.Reverse();
				graphicsPath.AddEllipse(new Rectangle(m_Center.X - m_ScaleLinesMajorInnerRadius, m_Center.Y - m_ScaleLinesMajorInnerRadius, 2 * m_ScaleLinesMajorInnerRadius, 2 * m_ScaleLinesMajorInnerRadius));
				graphicsPath.Reverse();
				graphics.SetClip(graphicsPath);
				graphics.DrawLine(new Pen(m_ScaleLinesMajorColor, m_ScaleLinesMajorWidth), Center.X, Center.Y, (float)((double)Center.X + (double)(2 * m_ScaleLinesMajorOuterRadius) * Math.Cos((double)((float)m_BaseArcStart + num2 * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue)) * Math.PI / 180.0)), (float)((double)Center.Y + (double)(2 * m_ScaleLinesMajorOuterRadius) * Math.Sin((double)((float)m_BaseArcStart + num2 * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue)) * Math.PI / 180.0)));
				graphicsPath.Reset();
				graphicsPath.AddEllipse(new Rectangle(m_Center.X - m_ScaleLinesMinorOuterRadius, m_Center.Y - m_ScaleLinesMinorOuterRadius, 2 * m_ScaleLinesMinorOuterRadius, 2 * m_ScaleLinesMinorOuterRadius));
				graphicsPath.Reverse();
				graphicsPath.AddEllipse(new Rectangle(m_Center.X - m_ScaleLinesMinorInnerRadius, m_Center.Y - m_ScaleLinesMinorInnerRadius, 2 * m_ScaleLinesMinorInnerRadius, 2 * m_ScaleLinesMinorInnerRadius));
				graphicsPath.Reverse();
				graphics.SetClip(graphicsPath);
				if (num2 < m_MaxValue - m_MinValue)
				{
					for (int j = 1; j <= m_ScaleLinesMinorTicks; j++)
					{
						if (m_ScaleLinesMinorTicks % 2 == 1 && m_ScaleLinesMinorTicks / 2 + 1 == j)
						{
							graphicsPath.Reset();
							graphicsPath.AddEllipse(new Rectangle(m_Center.X - m_ScaleLinesInterOuterRadius, m_Center.Y - m_ScaleLinesInterOuterRadius, 2 * m_ScaleLinesInterOuterRadius, 2 * m_ScaleLinesInterOuterRadius));
							graphicsPath.Reverse();
							graphicsPath.AddEllipse(new Rectangle(m_Center.X - m_ScaleLinesInterInnerRadius, m_Center.Y - m_ScaleLinesInterInnerRadius, 2 * m_ScaleLinesInterInnerRadius, 2 * m_ScaleLinesInterInnerRadius));
							graphicsPath.Reverse();
							graphics.SetClip(graphicsPath);
							graphics.DrawLine(new Pen(m_ScaleLinesInterColor, m_ScaleLinesInterWidth), Center.X, Center.Y, (float)((double)Center.X + (double)(2 * m_ScaleLinesInterOuterRadius) * Math.Cos((double)((float)m_BaseArcStart + num2 * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue) + (float)(j * m_BaseArcSweep) / ((m_MaxValue - m_MinValue) / m_ScaleLinesMajorStepValue * (float)(m_ScaleLinesMinorTicks + 1))) * Math.PI / 180.0)), (float)((double)Center.Y + (double)(2 * m_ScaleLinesInterOuterRadius) * Math.Sin((double)((float)m_BaseArcStart + num2 * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue) + (float)(j * m_BaseArcSweep) / ((m_MaxValue - m_MinValue) / m_ScaleLinesMajorStepValue * (float)(m_ScaleLinesMinorTicks + 1))) * Math.PI / 180.0)));
							graphicsPath.Reset();
							graphicsPath.AddEllipse(new Rectangle(m_Center.X - m_ScaleLinesMinorOuterRadius, m_Center.Y - m_ScaleLinesMinorOuterRadius, 2 * m_ScaleLinesMinorOuterRadius, 2 * m_ScaleLinesMinorOuterRadius));
							graphicsPath.Reverse();
							graphicsPath.AddEllipse(new Rectangle(m_Center.X - m_ScaleLinesMinorInnerRadius, m_Center.Y - m_ScaleLinesMinorInnerRadius, 2 * m_ScaleLinesMinorInnerRadius, 2 * m_ScaleLinesMinorInnerRadius));
							graphicsPath.Reverse();
							graphics.SetClip(graphicsPath);
						}
						else
						{
							graphics.DrawLine(new Pen(m_ScaleLinesMinorColor, m_ScaleLinesMinorWidth), Center.X, Center.Y, (float)((double)Center.X + (double)(2 * m_ScaleLinesMinorOuterRadius) * Math.Cos((double)((float)m_BaseArcStart + num2 * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue) + (float)(j * m_BaseArcSweep) / ((m_MaxValue - m_MinValue) / m_ScaleLinesMajorStepValue * (float)(m_ScaleLinesMinorTicks + 1))) * Math.PI / 180.0)), (float)((double)Center.Y + (double)(2 * m_ScaleLinesMinorOuterRadius) * Math.Sin((double)((float)m_BaseArcStart + num2 * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue) + (float)(j * m_BaseArcSweep) / ((m_MaxValue - m_MinValue) / m_ScaleLinesMajorStepValue * (float)(m_ScaleLinesMinorTicks + 1))) * Math.PI / 180.0)));
						}
					}
				}
				graphics.SetClip(ClientRectangle);
				if (m_ScaleNumbersRotation != 0)
				{
					graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
					graphics.RotateTransform(90f + (float)m_BaseArcStart + num2 * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue));
				}
				graphics.TranslateTransform((float)((double)Center.X + (double)m_ScaleNumbersRadius * Math.Cos((double)((float)m_BaseArcStart + num2 * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue)) * Math.PI / 180.0)), (float)((double)Center.Y + (double)m_ScaleNumbersRadius * Math.Sin((double)((float)m_BaseArcStart + num2 * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue)) * Math.PI / 180.0)), MatrixOrder.Append);
				if (num3 >= ScaleNumbersStartScaleLine - 1)
				{
					graphics.DrawString(text, Font, new SolidBrush(m_ScaleNumbersColor), (0f - sizeF.Width) / 2f, 0f - fontBoundY1 - (fontBoundY2 - fontBoundY1 + 1f) / 2f, StringFormat.GenericTypographic);
				}
				num2 += m_ScaleLinesMajorStepValue;
				num3++;
			}
			graphics.ResetTransform();
			graphics.SetClip(ClientRectangle);
			if (m_ScaleNumbersRotation != 0)
			{
				graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
			}
			foreach (AGaugeLabel gaugeLabel in _GaugeLabels)
			{
				if (!string.IsNullOrEmpty(gaugeLabel.Text))
				{
					graphics.DrawString(gaugeLabel.Text, gaugeLabel.Font, new SolidBrush(gaugeLabel.Color), gaugeLabel.Position.X, gaugeLabel.Position.Y, StringFormat.GenericTypographic);
				}
			}
		}
		e.Graphics.DrawImageUnscaled(gaugeBitmap, 0, 0);
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		float num4 = (int)((float)m_BaseArcStart + (m_value - m_MinValue) * (float)m_BaseArcSweep / (m_MaxValue - m_MinValue)) % 360;
		double num5 = (double)num4 * Math.PI / 180.0;
		switch (m_NeedleType)
		{
		case NeedleType.Advance:
		{
			PointF[] array = new PointF[3];
			Brush brush = Brushes.White;
			Brush brush2 = Brushes.White;
			Brush brush3 = Brushes.White;
			Brush brush4 = Brushes.White;
			Brush white = Brushes.White;
			int num6 = (int)((num4 + 225f) % 180f * 100f / 180f);
			int num7 = (int)((num4 + 135f) % 180f * 100f / 180f);
			e.Graphics.FillEllipse(new SolidBrush(m_NeedleColor2), Center.X - m_NeedleWidth * 3, Center.Y - m_NeedleWidth * 3, m_NeedleWidth * 6, m_NeedleWidth * 6);
			switch (m_NeedleColor1)
			{
			case AGaugeNeedleColor.Gray:
				brush = new SolidBrush(Color.FromArgb(80 + num6, 80 + num6, 80 + num6));
				brush2 = new SolidBrush(Color.FromArgb(180 - num6, 180 - num6, 180 - num6));
				brush3 = new SolidBrush(Color.FromArgb(80 + num7, 80 + num7, 80 + num7));
				brush4 = new SolidBrush(Color.FromArgb(180 - num7, 180 - num7, 180 - num7));
				e.Graphics.DrawEllipse(Pens.Gray, Center.X - m_NeedleWidth * 3, Center.Y - m_NeedleWidth * 3, m_NeedleWidth * 6, m_NeedleWidth * 6);
				break;
			case AGaugeNeedleColor.Red:
				brush = new SolidBrush(Color.FromArgb(145 + num6, num6, num6));
				brush2 = new SolidBrush(Color.FromArgb(245 - num6, 100 - num6, 100 - num6));
				brush3 = new SolidBrush(Color.FromArgb(145 + num7, num7, num7));
				brush4 = new SolidBrush(Color.FromArgb(245 - num7, 100 - num7, 100 - num7));
				e.Graphics.DrawEllipse(Pens.Red, Center.X - m_NeedleWidth * 3, Center.Y - m_NeedleWidth * 3, m_NeedleWidth * 6, m_NeedleWidth * 6);
				break;
			case AGaugeNeedleColor.Green:
				brush = new SolidBrush(Color.FromArgb(num6, 145 + num6, num6));
				brush2 = new SolidBrush(Color.FromArgb(100 - num6, 245 - num6, 100 - num6));
				brush3 = new SolidBrush(Color.FromArgb(num7, 145 + num7, num7));
				brush4 = new SolidBrush(Color.FromArgb(100 - num7, 245 - num7, 100 - num7));
				e.Graphics.DrawEllipse(Pens.Green, Center.X - m_NeedleWidth * 3, Center.Y - m_NeedleWidth * 3, m_NeedleWidth * 6, m_NeedleWidth * 6);
				break;
			case AGaugeNeedleColor.Blue:
				brush = new SolidBrush(Color.FromArgb(num6, num6, 145 + num6));
				brush2 = new SolidBrush(Color.FromArgb(100 - num6, 100 - num6, 245 - num6));
				brush3 = new SolidBrush(Color.FromArgb(num7, num7, 145 + num7));
				brush4 = new SolidBrush(Color.FromArgb(100 - num7, 100 - num7, 245 - num7));
				e.Graphics.DrawEllipse(Pens.Blue, Center.X - m_NeedleWidth * 3, Center.Y - m_NeedleWidth * 3, m_NeedleWidth * 6, m_NeedleWidth * 6);
				break;
			case AGaugeNeedleColor.Magenta:
				brush = new SolidBrush(Color.FromArgb(num6, 145 + num6, 145 + num6));
				brush2 = new SolidBrush(Color.FromArgb(100 - num6, 245 - num6, 245 - num6));
				brush3 = new SolidBrush(Color.FromArgb(num7, 145 + num7, 145 + num7));
				brush4 = new SolidBrush(Color.FromArgb(100 - num7, 245 - num7, 245 - num7));
				e.Graphics.DrawEllipse(Pens.Magenta, Center.X - m_NeedleWidth * 3, Center.Y - m_NeedleWidth * 3, m_NeedleWidth * 6, m_NeedleWidth * 6);
				break;
			case AGaugeNeedleColor.Violet:
				brush = new SolidBrush(Color.FromArgb(145 + num6, num6, 145 + num6));
				brush2 = new SolidBrush(Color.FromArgb(245 - num6, 100 - num6, 245 - num6));
				brush3 = new SolidBrush(Color.FromArgb(145 + num7, num7, 145 + num7));
				brush4 = new SolidBrush(Color.FromArgb(245 - num7, 100 - num7, 245 - num7));
				e.Graphics.DrawEllipse(Pens.Violet, Center.X - m_NeedleWidth * 3, Center.Y - m_NeedleWidth * 3, m_NeedleWidth * 6, m_NeedleWidth * 6);
				break;
			case AGaugeNeedleColor.Yellow:
				brush = new SolidBrush(Color.FromArgb(145 + num6, 145 + num6, num6));
				brush2 = new SolidBrush(Color.FromArgb(245 - num6, 245 - num6, 100 - num6));
				brush3 = new SolidBrush(Color.FromArgb(145 + num7, 145 + num7, num7));
				brush4 = new SolidBrush(Color.FromArgb(245 - num7, 245 - num7, 100 - num7));
				e.Graphics.DrawEllipse(Pens.Violet, Center.X - m_NeedleWidth * 3, Center.Y - m_NeedleWidth * 3, m_NeedleWidth * 6, m_NeedleWidth * 6);
				break;
			}
			if (Math.Floor((float)((double)((num4 + 225f) % 360f) / 180.0)) == 0.0)
			{
				white = brush;
				brush = brush2;
				brush2 = white;
			}
			if (Math.Floor((float)((double)((num4 + 135f) % 360f) / 180.0)) == 0.0)
			{
				brush4 = brush3;
			}
			array[0].X = (float)((double)Center.X + (double)m_NeedleRadius * Math.Cos(num5));
			array[0].Y = (float)((double)Center.Y + (double)m_NeedleRadius * Math.Sin(num5));
			array[1].X = (float)((double)Center.X - (double)(m_NeedleRadius / 20) * Math.Cos(num5));
			array[1].Y = (float)((double)Center.Y - (double)(m_NeedleRadius / 20) * Math.Sin(num5));
			array[2].X = (float)((double)Center.X - (double)(m_NeedleRadius / 5) * Math.Cos(num5) + (double)(m_NeedleWidth * 2) * Math.Cos(num5 + Math.PI / 2.0));
			array[2].Y = (float)((double)Center.Y - (double)(m_NeedleRadius / 5) * Math.Sin(num5) + (double)(m_NeedleWidth * 2) * Math.Sin(num5 + Math.PI / 2.0));
			e.Graphics.FillPolygon(brush, array);
			array[2].X = (float)((double)Center.X - (double)(m_NeedleRadius / 5) * Math.Cos(num5) + (double)(m_NeedleWidth * 2) * Math.Cos(num5 - Math.PI / 2.0));
			array[2].Y = (float)((double)Center.Y - (double)(m_NeedleRadius / 5) * Math.Sin(num5) + (double)(m_NeedleWidth * 2) * Math.Sin(num5 - Math.PI / 2.0));
			e.Graphics.FillPolygon(brush2, array);
			array[0].X = (float)((double)Center.X - (double)(m_NeedleRadius / 20 - 1) * Math.Cos(num5));
			array[0].Y = (float)((double)Center.Y - (double)(m_NeedleRadius / 20 - 1) * Math.Sin(num5));
			array[1].X = (float)((double)Center.X - (double)(m_NeedleRadius / 5) * Math.Cos(num5) + (double)(m_NeedleWidth * 2) * Math.Cos(num5 + Math.PI / 2.0));
			array[1].Y = (float)((double)Center.Y - (double)(m_NeedleRadius / 5) * Math.Sin(num5) + (double)(m_NeedleWidth * 2) * Math.Sin(num5 + Math.PI / 2.0));
			array[2].X = (float)((double)Center.X - (double)(m_NeedleRadius / 5) * Math.Cos(num5) + (double)(m_NeedleWidth * 2) * Math.Cos(num5 - Math.PI / 2.0));
			array[2].Y = (float)((double)Center.Y - (double)(m_NeedleRadius / 5) * Math.Sin(num5) + (double)(m_NeedleWidth * 2) * Math.Sin(num5 - Math.PI / 2.0));
			e.Graphics.FillPolygon(brush4, array);
			array[0].X = (float)((double)Center.X - (double)(m_NeedleRadius / 20) * Math.Cos(num5));
			array[0].Y = (float)((double)Center.Y - (double)(m_NeedleRadius / 20) * Math.Sin(num5));
			array[1].X = (float)((double)Center.X + (double)m_NeedleRadius * Math.Cos(num5));
			array[1].Y = (float)((double)Center.Y + (double)m_NeedleRadius * Math.Sin(num5));
			e.Graphics.DrawLine(new Pen(m_NeedleColor2), Center.X, Center.Y, array[0].X, array[0].Y);
			e.Graphics.DrawLine(new Pen(m_NeedleColor2), Center.X, Center.Y, array[1].X, array[1].Y);
			break;
		}
		case NeedleType.Simple:
		{
			Point point = new Point((int)((double)Center.X - (double)(m_NeedleRadius / 8) * Math.Cos(num5)), (int)((double)Center.Y - (double)(m_NeedleRadius / 8) * Math.Sin(num5)));
			Point point2 = new Point((int)((double)Center.X + (double)m_NeedleRadius * Math.Cos(num5)), (int)((double)Center.Y + (double)m_NeedleRadius * Math.Sin(num5)));
			e.Graphics.FillEllipse(new SolidBrush(m_NeedleColor2), Center.X - m_NeedleWidth * 3, Center.Y - m_NeedleWidth * 3, m_NeedleWidth * 6, m_NeedleWidth * 6);
			switch (m_NeedleColor1)
			{
			case AGaugeNeedleColor.Gray:
				e.Graphics.DrawLine(new Pen(Color.DarkGray, m_NeedleWidth), Center.X, Center.Y, point2.X, point2.Y);
				e.Graphics.DrawLine(new Pen(Color.DarkGray, m_NeedleWidth), Center.X, Center.Y, point.X, point.Y);
				break;
			case AGaugeNeedleColor.Red:
				e.Graphics.DrawLine(new Pen(Color.Red, m_NeedleWidth), Center.X, Center.Y, point2.X, point2.Y);
				e.Graphics.DrawLine(new Pen(Color.Red, m_NeedleWidth), Center.X, Center.Y, point.X, point.Y);
				break;
			case AGaugeNeedleColor.Green:
				e.Graphics.DrawLine(new Pen(Color.Green, m_NeedleWidth), Center.X, Center.Y, point2.X, point2.Y);
				e.Graphics.DrawLine(new Pen(Color.Green, m_NeedleWidth), Center.X, Center.Y, point.X, point.Y);
				break;
			case AGaugeNeedleColor.Blue:
				e.Graphics.DrawLine(new Pen(Color.Blue, m_NeedleWidth), Center.X, Center.Y, point2.X, point2.Y);
				e.Graphics.DrawLine(new Pen(Color.Blue, m_NeedleWidth), Center.X, Center.Y, point.X, point.Y);
				break;
			case AGaugeNeedleColor.Magenta:
				e.Graphics.DrawLine(new Pen(Color.Magenta, m_NeedleWidth), Center.X, Center.Y, point2.X, point2.Y);
				e.Graphics.DrawLine(new Pen(Color.Magenta, m_NeedleWidth), Center.X, Center.Y, point.X, point.Y);
				break;
			case AGaugeNeedleColor.Violet:
				e.Graphics.DrawLine(new Pen(Color.Violet, m_NeedleWidth), Center.X, Center.Y, point2.X, point2.Y);
				e.Graphics.DrawLine(new Pen(Color.Violet, m_NeedleWidth), Center.X, Center.Y, point.X, point.Y);
				break;
			case AGaugeNeedleColor.Yellow:
				e.Graphics.DrawLine(new Pen(Color.Yellow, m_NeedleWidth), Center.X, Center.Y, point2.X, point2.Y);
				e.Graphics.DrawLine(new Pen(Color.Yellow, m_NeedleWidth), Center.X, Center.Y, point.X, point.Y);
				break;
			}
			break;
		}
		}
	}

	protected override void OnResize(EventArgs e)
	{
		drawGaugeBackground = true;
		Refresh();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			gaugeBitmap.Dispose();
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
	}
}
