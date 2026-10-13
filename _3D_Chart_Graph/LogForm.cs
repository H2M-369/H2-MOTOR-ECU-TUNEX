using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace _3D_Chart_Graph;

public class LogForm : Form
{
	private IContainer components = null;

	private ListBox lstLog;

	public LogForm()
	{
		InitializeComponent();
		StartPosition = FormStartPosition.CenterScreen;
		MaximizeBox = false;
		MinimizeBox = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		ControlBox = false;
	}

	private void LogForm_Load(object sender, EventArgs e)
	{
	}

	public void AddLog(string message)
	{
		if (lstLog.InvokeRequired)
		{
			lstLog.Invoke(new Action<string>(AddLog), message);
		}
		else
		{
			lstLog.Items.Add(message);
			lstLog.TopIndex = lstLog.Items.Count - 1;
		}
	}

	public void ClearLog()
	{
		if (lstLog.InvokeRequired)
		{
			lstLog.Invoke(new Action(ClearLog));
		}
		else
		{
			lstLog.Items.Clear();
		}
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
		this.lstLog = new System.Windows.Forms.ListBox();
		base.SuspendLayout();
		this.lstLog.BackColor = System.Drawing.SystemColors.InfoText;
		this.lstLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.lstLog.ForeColor = System.Drawing.Color.Yellow;
		this.lstLog.FormattingEnabled = true;
		this.lstLog.Location = new System.Drawing.Point(1, 2);
		this.lstLog.Name = "lstLog";
		this.lstLog.Size = new System.Drawing.Size(917, 316);
		this.lstLog.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(917, 317);
		base.Controls.Add(this.lstLog);
		base.Name = "LogForm";
		this.Text = "LogData";
		base.Load += new System.EventHandler(this.LogForm_Load);
		base.ResumeLayout(false);
	}
}
