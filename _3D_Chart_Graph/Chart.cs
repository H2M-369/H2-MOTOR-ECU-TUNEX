using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace _3D_Chart_Graph;

public class Chart : Form
{
	private List<Point> rpmPoints = new List<Point>();

	private List<Point> tpsPoints = new List<Point>();

	private List<Point> adPoints = new List<Point>();

	private List<Point> afrPoints = new List<Point>();

	private const int xScale = 10;

	private float yScale;

	private bool isAutoScroll = true;

	private HScrollBar hScrollBar;

	private IContainer components = null;

	private Panel panel1;

	private Label label1;

	private Label label4;

	private Label label3;

	private Label label2;

	public int RPMD { get; set; }

	public float TPSD { get; set; }

	public float AD { get; set; }

	public float AFR { get; set; }

	public Chart()
	{
		InitializeComponent();
		SetDoubleBuffered(panel1);
		hScrollBar = new HScrollBar();
		hScrollBar.Dock = DockStyle.Bottom;
		hScrollBar.Scroll += HScrollBar_Scroll;
		Controls.Add(hScrollBar);
		panel1.Paint += panel1_Paint;
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
		Pen pen2 = new Pen(Color.Orange, 2f);
		Pen pen3 = new Pen(Color.Red, 2f);
		Pen pen4 = new Pen(Color.GreenYellow, 2f);
		int num = panel1.Height - 10;
		float num2 = 100f;
		yScale = (float)(panel1.Height - 20) / num2;
		int value = hScrollBar.Value;
		int num3 = ((rpmPoints.Count > 0) ? (rpmPoints[rpmPoints.Count - 1].X + 10) : 0);
		rpmPoints.Add(new Point(num3, num - (int)(NormalizeRPMD(RPMD) * yScale)));
		tpsPoints.Add(new Point(num3, num - (int)(NormalizeTPSD(TPSD) * yScale)));
		adPoints.Add(new Point(num3, num - (int)(NormalizeAD(AD) * yScale)));
		afrPoints.Add(new Point(num3, num - (int)(NormalizeAFR(AFR) * yScale)));
		if (rpmPoints.Count > 0 && rpmPoints[rpmPoints.Count - 1].X + 10 > panel1.Width)
		{
			for (int i = 0; i < rpmPoints.Count; i++)
			{
				rpmPoints[i] = new Point(rpmPoints[i].X - 10, rpmPoints[i].Y);
				tpsPoints[i] = new Point(tpsPoints[i].X - 10, tpsPoints[i].Y);
				adPoints[i] = new Point(adPoints[i].X - 10, adPoints[i].Y);
				afrPoints[i] = new Point(afrPoints[i].X - 10, afrPoints[i].Y);
			}
		}
		if (rpmPoints.Count > 0 && rpmPoints[0].X < 0)
		{
			rpmPoints.RemoveAt(0);
			tpsPoints.RemoveAt(0);
			adPoints.RemoveAt(0);
			afrPoints.RemoveAt(0);
		}
		for (int j = 0; j < rpmPoints.Count - 1; j++)
		{
			int num4 = rpmPoints[j].X - value;
			int num5 = rpmPoints[j + 1].X - value;
			if (num5 >= 0 && num4 < panel1.Width)
			{
				graphics.DrawLine(pen, new Point(num4, rpmPoints[j].Y), new Point(num5, rpmPoints[j + 1].Y));
			}
		}
		for (int k = 0; k < tpsPoints.Count - 1; k++)
		{
			int num6 = tpsPoints[k].X - value;
			int num7 = tpsPoints[k + 1].X - value;
			if (num7 >= 0 && num6 < panel1.Width)
			{
				graphics.DrawLine(pen2, new Point(num6, tpsPoints[k].Y), new Point(num7, tpsPoints[k + 1].Y));
			}
		}
		for (int l = 0; l < adPoints.Count - 1; l++)
		{
			int num8 = adPoints[l].X - value;
			int num9 = adPoints[l + 1].X - value;
			if (num9 >= 0 && num8 < panel1.Width)
			{
				graphics.DrawLine(pen3, new Point(num8, adPoints[l].Y), new Point(num9, adPoints[l + 1].Y));
			}
		}
		for (int m = 0; m < afrPoints.Count - 1; m++)
		{
			int num10 = afrPoints[m].X - value;
			int num11 = afrPoints[m + 1].X - value;
			if (num11 >= 0 && num10 < panel1.Width)
			{
				graphics.DrawLine(pen4, new Point(num10, afrPoints[m].Y), new Point(num11, afrPoints[m + 1].Y));
			}
		}
		Font font = new Font("Arial", 8f);
		Brush lightGreen = Brushes.LightGreen;
		Pen pen5 = new Pen(Color.Gray, 1f);
		int num12 = 15;
		for (int n = 0; n <= 100; n += 20)
		{
			int num13 = num - (int)((float)n * yScale);
			graphics.DrawString(n.ToString(), font, lightGreen, new PointF(num12, num13 - 10));
			graphics.DrawLine(pen5, new Point(10, num13), new Point(panel1.Width, num13));
		}
		if (isAutoScroll)
		{
			hScrollBar.Value = hScrollBar.Maximum;
		}
	}

	private float NormalizeRPMD(int rpm)
	{
		return (float)rpm / 20000f * 100f;
	}

	private float NormalizeTPSD(float tps)
	{
		return tps;
	}

	private float NormalizeAD(float ad)
	{
		return ad;
	}

	private float NormalizeAFR(float afr)
	{
		return afr;
	}

	public void UpdateChart(int rpm, float tps, float ad, float afr)
	{
		RPMD = rpm;
		TPSD = tps;
		AD = ad;
		AFR = afr;
		panel1.Invalidate();
	}

	private void Chart_Load(object sender, EventArgs e)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3D_Chart_Graph.Chart));
		this.panel1 = new System.Windows.Forms.Panel();
		this.label4 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.BackColor = System.Drawing.Color.Black;
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel1.Controls.Add(this.label4);
		this.panel1.Controls.Add(this.label3);
		this.panel1.Controls.Add(this.label2);
		this.panel1.Controls.Add(this.label1);
		this.panel1.Location = new System.Drawing.Point(3, 2);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(861, 218);
		this.panel1.TabIndex = 0;
		this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
		this.label4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label4.ForeColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.label4.Location = new System.Drawing.Point(793, 62);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(58, 19);
		this.label4.TabIndex = 3;
		this.label4.Text = "AFR";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label3.ForeColor = System.Drawing.Color.Red;
		this.label3.Location = new System.Drawing.Point(793, 43);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(58, 19);
		this.label3.TabIndex = 2;
		this.label3.Text = "IG";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.label2.Location = new System.Drawing.Point(793, 24);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(58, 19);
		this.label2.TabIndex = 1;
		this.label2.Text = "TPS";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.Blue;
		this.label1.Location = new System.Drawing.Point(793, 5);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(58, 19);
		this.label1.TabIndex = 0;
		this.label1.Text = "RPM";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(868, 232);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "Chart";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Chart";
		base.Load += new System.EventHandler(this.Chart_Load);
		this.panel1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
