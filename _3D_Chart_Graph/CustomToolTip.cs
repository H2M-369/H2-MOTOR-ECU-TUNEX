using System.Drawing;
using System.Windows.Forms;

namespace _3D_Chart_Graph;

public class CustomToolTip : Form
{
	private Label label;

	public CustomToolTip()
	{
		FormBorderStyle = FormBorderStyle.None;
		StartPosition = FormStartPosition.Manual;
		BackColor = Color.Orange;
		Opacity = 0.85;
		ShowInTaskbar = false;
		label = new Label
		{
			ForeColor = Color.White,
			BackColor = Color.Transparent,
			Font = new Font("Arial", 10f, FontStyle.Bold),
			AutoSize = true
		};
		Controls.Add(label);
	}

	public void Show(string text, Form parent, int x, int y)
	{
		label.Text = text;
		Size = label.Size;
		Location = parent.PointToScreen(new Point(x, y));
		Show();
		BringToFront();
	}

	public new void Hide()
	{
		base.Hide();
	}
}
