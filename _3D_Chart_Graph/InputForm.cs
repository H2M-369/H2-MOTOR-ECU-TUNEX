using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace _3D_Chart_Graph;

public class InputForm : Form
{
	private IContainer components = null;

	private TextBox txtInput;

	private Button btnOK;

	private Label label1;

	public double InputValue { get; private set; }

	public bool IsConfirmed { get; private set; }

	public InputForm(string prompt, string initialValue)
	{
		InitializeComponent();
		txtInput.Text = initialValue;
		IsConfirmed = false;
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void btnOK_Click_1(object sender, EventArgs e)
	{
		if (double.TryParse(txtInput.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
		{
			InputValue = result;
			IsConfirmed = true;
			Close();
			return;
		}
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		if (double.TryParse(txtInput.Text.Replace(',', '.'), NumberStyles.Any, invariantCulture, out result))
		{
			InputValue = result;
			IsConfirmed = true;
			Close();
		}
		else
		{
			MessageBox.Show("Input tidak valid. Masukkan angka yang benar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void InputForm_Load(object sender, EventArgs e)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3D_Chart_Graph.InputForm));
		this.txtInput = new System.Windows.Forms.TextBox();
		this.btnOK = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.txtInput.Font = new System.Drawing.Font("Franklin Gothic Demi Cond", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtInput.Location = new System.Drawing.Point(78, 12);
		this.txtInput.Multiline = true;
		this.txtInput.Name = "txtInput";
		this.txtInput.Size = new System.Drawing.Size(66, 30);
		this.txtInput.TabIndex = 0;
		this.txtInput.Text = "0";
		this.txtInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.btnOK.Font = new System.Drawing.Font("Franklin Gothic Demi Cond", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.btnOK.Location = new System.Drawing.Point(150, 12);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(78, 30);
		this.btnOK.TabIndex = 1;
		this.btnOK.Text = "Execute";
		this.btnOK.UseVisualStyleBackColor = true;
		this.btnOK.Click += new System.EventHandler(this.btnOK_Click_1);
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Franklin Gothic Demi Cond", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.White;
		this.label1.Location = new System.Drawing.Point(12, 15);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(60, 24);
		this.label1.TabIndex = 2;
		this.label1.Text = "Value :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(240, 54);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.txtInput);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "InputForm";
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Input Value";
		base.Load += new System.EventHandler(this.InputForm_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
