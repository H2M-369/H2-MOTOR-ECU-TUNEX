using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace _3D_Chart_Graph;

public class DYNO : Form
{
	private List<Point> rpmPoints = new List<Point>();

	private List<Point> hpPoints = new List<Point>();

	private List<Point> afrPoints = new List<Point>();

	private List<Point> tqPoints = new List<Point>();

	private const int xScale = 10;

	private float yScale;

	private bool isAutoScroll = true;

	private bool startDrawing = false;

	private HScrollBar hScrollBar;

	private float maxHP = float.MinValue;

	private float maxTQ = float.MinValue;

	private int rpmAtMaxHP = 0;

	private IContainer components = null;

	private Button button1;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label label4;

	private Panel panel1;

	private Panel panel2;

	private Button button2;

	public int RPMD { get; set; }

	public float HP { get; set; }

	public float AFR { get; set; }

	public float TQ { get; set; }

	public DYNO()
	{
		InitializeComponent();
		SetDoubleBuffered(panel1);
		hScrollBar = new HScrollBar();
		hScrollBar.Dock = DockStyle.Bottom;
		hScrollBar.Scroll += HScrollBar_Scroll;
		Controls.Add(hScrollBar);
		panel1.Paint += panel1_Paint;
		panel1.MouseClick += Panel1_MouseClick;
		button1.Text = "Run";
		label1.Text = "0.00";
		label2.Text = "0";
		label4.Text = "0.00";
		label3.Text = "0.00";
	}

	private void Panel1_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			startDrawing = true;
			button1.Text = "Stop";
		}
		else if (e.Button == MouseButtons.Left)
		{
			startDrawing = false;
			button1.Text = "Run";
		}
		panel1.Invalidate();
	}

	private void SetDoubleBuffered(Control control)
	{
		PropertyInfo property = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
		if (property != null)
		{
			property.SetValue(control, true, null);
		}
	}

	private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		isAutoScroll = false;
		panel1.Invalidate();
	}

	private void panel1_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.Clear(panel1.BackColor);
		Graphics graphics = e.Graphics;
		Pen pen = new Pen(Color.Blue, 2f);
		Pen pen2 = new Pen(Color.Red, 2f);
		Pen pen3 = new Pen(Color.GreenYellow, 2f);
		Pen pen4 = new Pen(Color.White, 2f);
		int num = panel1.Height - 10;
		float num2 = 45f;
		yScale = (float)(panel1.Height - 20) / num2;
		if (startDrawing)
		{
			int num3 = ((rpmPoints.Count > 0) ? (rpmPoints[rpmPoints.Count - 1].X + 10) : 0);
			rpmPoints.Add(new Point(num3, num - (int)(NormalizeRPMD(RPMD) * yScale)));
			hpPoints.Add(new Point(num3, num - (int)(NormalizeHP(HP) * yScale)));
			afrPoints.Add(new Point(num3, num - (int)(NormalizeAFR(AFR) * yScale)));
			tqPoints.Add(new Point(num3, num - (int)(NormalizeTQ(TQ) * yScale)));
		}
		DrawPoints(graphics, rpmPoints, pen);
		DrawPoints(graphics, hpPoints, pen2);
		DrawPoints(graphics, afrPoints, pen3);
		DrawPoints(graphics, tqPoints, pen4);
		DrawScaleLabels(graphics, num2, num);
		if (isAutoScroll)
		{
			try
			{
				hScrollBar.Value = hScrollBar.Maximum;
			}
			catch (ArgumentOutOfRangeException)
			{
				hScrollBar.Value = hScrollBar.Maximum - hScrollBar.LargeChange + 1;
			}
		}
	}

	private void DrawPoints(Graphics g, List<Point> points, Pen pen)
	{
		if (points.Count <= 1)
		{
			return;
		}
		if (points[points.Count - 1].X >= panel1.Width)
		{
			float num = (float)panel1.Width / (float)points.Count;
			for (int i = 0; i < points.Count - 1; i++)
			{
				int num2 = (int)((float)i * num);
				int num3 = (int)((float)(i + 1) * num);
				if (num3 >= 0 && num2 < panel1.Width)
				{
					g.DrawLine(pen, new Point(num2, points[i].Y), new Point(num3, points[i + 1].Y));
				}
			}
		}
		else
		{
			for (int j = 0; j < points.Count - 1; j++)
			{
				g.DrawLine(pen, points[j], points[j + 1]);
			}
		}
	}

	private void DrawScaleLabels(Graphics g, float maxDataValue, int yOffset)
	{
		Font font = new Font("Arial", 8f);
		Brush lightGreen = Brushes.LightGreen;
		Pen pen = new Pen(Color.Gray, 1f);
		int num = 15;
		for (int i = 0; (float)i <= maxDataValue; i += 5)
		{
			int num2 = yOffset - (int)((float)i * yScale);
			g.DrawString(i.ToString(), font, lightGreen, new PointF(num, num2 - 10));
			g.DrawLine(pen, new Point(10, num2), new Point(panel1.Width, num2));
		}
	}

	private float NormalizeRPMD(int rpm)
	{
		return (float)rpm / 20000f * 45f;
	}

	private float NormalizeHP(float hp)
	{
		return Math.Min(hp, 45f);
	}

	private float NormalizeAFR(float afr)
	{
		return Math.Min(afr, 45f);
	}

	private float NormalizeTQ(float tq)
	{
		return Math.Min(tq, 45f);
	}

	public void Updatedyno(int rpm, float hp, float afr, float tq)
	{
		RPMD = rpm;
		HP = hp;
		AFR = afr;
		TQ = tq;
		if (startDrawing)
		{
			if (HP > maxHP)
			{
				maxHP = HP;
				rpmAtMaxHP = RPMD;
				label1.Text = $"Max HP: {maxHP:F2}";
				label2.Text = $"RPM at Max HP: {rpmAtMaxHP}";
			}
			if (TQ > maxTQ)
			{
				maxTQ = TQ;
				label4.Text = $"Max TQ: {maxTQ:F2}";
			}
			label3.Text = $"AFR: {afr:F2}";
			Console.WriteLine($"RPM: {RPMD}, HP: {HP}, AFR: {AFR}, TQ: {TQ}");
			panel1.Invalidate();
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		startDrawing = !startDrawing;
		button1.Text = (startDrawing ? "Stop" : "Run");
		if (startDrawing)
		{
			panel1.Invalidate();
		}
	}

	private void button2_Click(object sender, EventArgs e)
	{
		rpmPoints.Clear();
		hpPoints.Clear();
		afrPoints.Clear();
		tqPoints.Clear();
		maxHP = float.MinValue;
		maxTQ = float.MinValue;
		rpmAtMaxHP = 0;
		label1.Text = "0.00";
		label2.Text = "0";
		label4.Text = "0.00";
		label3.Text = "0.00";
		isAutoScroll = true;
		panel1.Invalidate();
	}

	private void DYNO_Load(object sender, EventArgs e)
	{
	}

	private void label3_Click(object sender, EventArgs e)
	{
	}

	private void label2_Click(object sender, EventArgs e)
	{
	}

	private void label4_Click(object sender, EventArgs e)
	{
	}

	private void label1_Click(object sender, EventArgs e)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3D_Chart_Graph.DYNO));
		this.button1 = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.panel1 = new System.Windows.Forms.Panel();
		this.panel2 = new System.Windows.Forms.Panel();
		this.button2 = new System.Windows.Forms.Button();
		this.panel2.SuspendLayout();
		base.SuspendLayout();
		this.button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.button1.ForeColor = System.Drawing.Color.Black;
		this.button1.Location = new System.Drawing.Point(1028, 3);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 38);
		this.button1.TabIndex = 0;
		this.button1.Text = "Run";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(this.button1_Click);
		this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label1.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.Black;
		this.label1.Location = new System.Drawing.Point(687, 6);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(236, 38);
		this.label1.TabIndex = 1;
		this.label1.Text = "Max HP";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label1.Click += new System.EventHandler(this.label1_Click);
		this.label2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label2.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.ForeColor = System.Drawing.Color.Black;
		this.label2.Location = new System.Drawing.Point(176, 6);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(239, 38);
		this.label2.TabIndex = 2;
		this.label2.Text = "Max Rpm";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label2.Click += new System.EventHandler(this.label2_Click);
		this.label3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label3.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label3.ForeColor = System.Drawing.Color.Black;
		this.label3.Location = new System.Drawing.Point(8, 6);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(162, 38);
		this.label3.TabIndex = 3;
		this.label3.Text = "Afr NR";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label3.Click += new System.EventHandler(this.label3_Click);
		this.label4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label4.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label4.ForeColor = System.Drawing.Color.Black;
		this.label4.Location = new System.Drawing.Point(421, 5);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(260, 38);
		this.label4.TabIndex = 4;
		this.label4.Text = "Max Torque";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label4.Click += new System.EventHandler(this.label4_Click);
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.BackColor = System.Drawing.Color.Black;
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel1.Location = new System.Drawing.Point(2, 56);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(1117, 542);
		this.panel1.TabIndex = 5;
		this.panel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel2.BackColor = System.Drawing.Color.Black;
		this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel2.Controls.Add(this.button2);
		this.panel2.Controls.Add(this.button1);
		this.panel2.Controls.Add(this.label4);
		this.panel2.Controls.Add(this.label2);
		this.panel2.Controls.Add(this.label1);
		this.panel2.Controls.Add(this.label3);
		this.panel2.Location = new System.Drawing.Point(2, 2);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(1117, 48);
		this.panel2.TabIndex = 6;
		this.button2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.button2.ForeColor = System.Drawing.Color.Black;
		this.button2.Location = new System.Drawing.Point(929, 3);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 38);
		this.button2.TabIndex = 5;
		this.button2.Text = "Refresh";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(this.button2_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(1121, 601);
		base.Controls.Add(this.panel2);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "DYNO";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Run Graph";
		base.Load += new System.EventHandler(this.DYNO_Load);
		this.panel2.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
