using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace _3D_Chart_Graph;

public class _2d : Form
{
	private List<DataPoint> dataPoints;

	private bool isDragging;

	private DataPoint draggedPoint;

	private Point offset;

	private int margin = 15;

	private int pointSize = 6;

	private CustomToolTip toolTip;

	private IContainer components = null;

	private Panel panel1;

	public event Action<int, double> DataPointValueChanged;

	public _2d()
	{
		InitializeComponent();
		dataPoints = new List<DataPoint>();
		toolTip = new CustomToolTip();
		panel1.Paint += Panel1_Paint;
		panel1.MouseDown += Panel1_MouseDown;
		panel1.MouseMove += Panel1_MouseMove;
		panel1.MouseUp += Panel1_MouseUp;
		panel1.Resize += Panel1_Resize;
	}

	public void ShowChart(List<double> values)
	{
		dataPoints.Clear();
		for (int i = 0; i < values.Count; i++)
		{
			int num = margin + i * (panel1.Width - 2 * margin) / (values.Count - 1);
			int num2 = margin + (int)((double)(panel1.Height - 2 * margin) - values[i] * (double)(panel1.Height - 2 * margin) / 100.0);
			dataPoints.Add(new DataPoint
			{
				X = i,
				Y = values[i],
				Bounds = new Rectangle(num - pointSize / 2, num2 - pointSize / 2, pointSize, pointSize)
			});
		}
		panel1.Invalidate();
	}

	private void Panel1_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		using (Pen pen = new Pen(Color.Lime, 0.1f)
		{
			DashStyle = DashStyle.Dash
		})
		{
			for (int i = 0; i <= 10; i++)
			{
				int num = margin + i * (panel1.Height - 2 * margin) / 10;
				graphics.DrawLine(pen, margin, num, panel1.Width - margin, num);
			}
			for (int j = 0; j < dataPoints.Count; j++)
			{
				int num2 = margin + j * (panel1.Width - 2 * margin) / (dataPoints.Count - 1);
				graphics.DrawLine(pen, num2, margin, num2, panel1.Height - margin);
			}
		}
		if (dataPoints.Count > 1)
		{
			using Pen pen2 = new Pen(Color.Orange, 2.5f);
			for (int k = 1; k < dataPoints.Count; k++)
			{
				if (dataPoints[k - 1].Bounds != Rectangle.Empty && dataPoints[k].Bounds != Rectangle.Empty && k > 1)
				{
					graphics.DrawLine(pen2, dataPoints[k].Bounds.X + pointSize / 2, dataPoints[k].Bounds.Y + pointSize / 2, dataPoints[k - 1].Bounds.X + pointSize / 2, dataPoints[k - 1].Bounds.Y + pointSize / 2);
				}
			}
		}
		for (int l = 1; l < dataPoints.Count; l++)
		{
			DataPoint dataPoint = dataPoints[l];
			if (dataPoint.Bounds != Rectangle.Empty)
			{
				graphics.FillEllipse(Brushes.Cyan, dataPoint.Bounds);
			}
		}
	}

	private void Panel1_MouseDown(object sender, MouseEventArgs e)
	{
		foreach (DataPoint dataPoint in dataPoints)
		{
			if (dataPoint.Bounds.Contains(e.Location))
			{
				isDragging = true;
				draggedPoint = dataPoint;
				offset = new Point(e.X - dataPoint.Bounds.X, e.Y - dataPoint.Bounds.Y);
				toolTip.Show($"Value: {draggedPoint.Y:F2}", this, e.Location.X + 10, e.Location.Y + 10);
				break;
			}
		}
	}

	private void Panel1_MouseMove(object sender, MouseEventArgs e)
	{
		if (isDragging && draggedPoint != null)
		{
			int num = e.Y - offset.Y;
			draggedPoint.Y = (panel1.Height - margin - num) * 100 / (panel1.Height - 2 * margin);
			draggedPoint.Bounds = new Rectangle(draggedPoint.Bounds.X, num, pointSize, pointSize);
			toolTip.Show($"Value: {draggedPoint.Y:F2}", this, e.Location.X + 10, e.Location.Y + 10);
			panel1.Invalidate();
			int arg = dataPoints.IndexOf(draggedPoint);
			DataPointValueChanged?.Invoke(arg, draggedPoint.Y);
		}
	}

	private void Panel1_MouseUp(object sender, MouseEventArgs e)
	{
		isDragging = false;
		if (draggedPoint != null)
		{
			DataPointValueChanged?.Invoke((int)draggedPoint.X, draggedPoint.Y);
		}
		draggedPoint = null;
		toolTip.Hide();
	}

	private void Panel1_Resize(object sender, EventArgs e)
	{
		for (int i = 0; i < dataPoints.Count; i++)
		{
			int num = margin + i * (panel1.Width - 2 * margin) / (dataPoints.Count - 1);
			int num2 = margin + (int)((double)(panel1.Height - 2 * margin) - dataPoints[i].Y * (double)(panel1.Height - 2 * margin) / 100.0);
			dataPoints[i].Bounds = new Rectangle(num - pointSize / 2, num2 - pointSize / 2, pointSize, pointSize);
		}
		panel1.Invalidate();
	}

	private void _2d_Load(object sender, EventArgs e)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3D_Chart_Graph._2d));
		this.panel1 = new System.Windows.Forms.Panel();
		base.SuspendLayout();
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.BackColor = System.Drawing.Color.Black;
		this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel1.Location = new System.Drawing.Point(3, 3);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(794, 248);
		this.panel1.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(800, 253);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "_2d";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "2D";
		base.Load += new System.EventHandler(this._2d_Load);
		base.ResumeLayout(false);
	}
}
