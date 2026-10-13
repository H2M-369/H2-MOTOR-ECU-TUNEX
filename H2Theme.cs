using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace _3D_Chart_Graph
{
	// Tema hitam-merah "H2 MOTOR ECU TUNEX": warna, menu rapi, window tidak menutup taskbar, kotak input lebih lebar.
	public partial class Form1
	{
		private static readonly Color H2Black = Color.FromArgb(10, 10, 12);
		private static readonly Color H2Panel = Color.FromArgb(22, 22, 26);
		private static readonly Color H2Button = Color.FromArgb(30, 30, 36);
		private static readonly Color H2Red = Color.FromArgb(214, 28, 46);
		private static readonly Color H2RedBright = Color.FromArgb(255, 70, 80);
		private static readonly Color H2DarkRed = Color.FromArgb(110, 12, 22);
		private static readonly Color H2Text = Color.FromArgb(240, 240, 240);

		private bool h2ThemeApplied;

		private sealed class H2ColorTable : ProfessionalColorTable
		{
			public H2ColorTable()
			{
				UseSystemColors = false;
			}
			public override Color MenuStripGradientBegin { get { return Color.FromArgb(8, 8, 10); } }
			public override Color MenuStripGradientEnd { get { return Color.FromArgb(26, 8, 12); } }
			public override Color MenuItemSelected { get { return H2DarkRed; } }
			public override Color MenuItemSelectedGradientBegin { get { return Color.FromArgb(150, 18, 32); } }
			public override Color MenuItemSelectedGradientEnd { get { return H2DarkRed; } }
			public override Color MenuItemPressedGradientBegin { get { return Color.FromArgb(70, 8, 16); } }
			public override Color MenuItemPressedGradientMiddle { get { return Color.FromArgb(90, 10, 20); } }
			public override Color MenuItemPressedGradientEnd { get { return Color.FromArgb(70, 8, 16); } }
			public override Color MenuItemBorder { get { return H2Red; } }
			public override Color MenuBorder { get { return H2Red; } }
			public override Color ToolStripDropDownBackground { get { return Color.FromArgb(16, 16, 20); } }
			public override Color ImageMarginGradientBegin { get { return Color.FromArgb(16, 16, 20); } }
			public override Color ImageMarginGradientMiddle { get { return Color.FromArgb(16, 16, 20); } }
			public override Color ImageMarginGradientEnd { get { return Color.FromArgb(16, 16, 20); } }
			public override Color SeparatorDark { get { return Color.FromArgb(120, 20, 30); } }
			public override Color SeparatorLight { get { return Color.FromArgb(40, 12, 16); } }
			public override Color CheckBackground { get { return H2DarkRed; } }
			public override Color CheckSelectedBackground { get { return H2Red; } }
			public override Color CheckPressedBackground { get { return H2Red; } }
		}

		private sealed class H2Renderer : ToolStripProfessionalRenderer
		{
			public H2Renderer() : base(new H2ColorTable())
			{
				RoundedEdges = false;
			}
			protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
			{
				e.TextColor = e.Item.Enabled ? H2Text : Color.FromArgb(120, 120, 126);
				base.OnRenderItemText(e);
			}
			protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
			{
				e.ArrowColor = H2RedBright;
				base.OnRenderArrow(e);
			}
			protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
			{
				if (e.ToolStrip is MenuStrip)
				{
					using (Pen p = new Pen(H2Red, 2f))
					{
						e.Graphics.DrawLine(p, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width, e.ToolStrip.Height - 1);
					}
					return;
				}
				using (Pen p2 = new Pen(H2Red))
				{
					e.Graphics.DrawRectangle(p2, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
				}
			}
		}

		private void ApplyH2Theme()
		{
			if (h2ThemeApplied)
			{
				return;
			}
			h2ThemeApplied = true;
			try
			{
				RemoveReadEcuLeftovers();
				RearrangeMenus();
				StyleMenus();
				ThemeControl(this);
				WidenValueInput();
				SetWindowFitsWorkingArea();
				SetH2Icon();
			}
			catch (Exception)
			{
			}
			// Appearance/Load bisa mengubah ukuran dan WindowState setelah ini; rapikan lagi saat window tampil.
			Shown += delegate
			{
				SetWindowFitsWorkingArea();
			};
			SizeChanged += delegate
			{
				SetWindowFitsWorkingArea();
			};
			Microsoft.Win32.SystemEvents.DisplaySettingsChanged += delegate
			{
				SetWindowFitsWorkingArea();
			};
		}

		// Hapus sisa menu Read ECU bila masih ada.
		private void RemoveReadEcuLeftovers()
		{
			for (int i = menuStrip7.Items.Count - 1; i >= 0; i--)
			{
				ToolStripItem item = menuStrip7.Items[i];
				if (item.Text != null && item.Text.IndexOf("Read ECU", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					menuStrip7.Items.RemoveAt(i);
				}
			}
		}

		private static string MenuKey(ToolStripItem item)
		{
			string t = item.Text ?? "";
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (char c in t)
			{
				if (char.IsLetterOrDigit(c))
				{
					sb.Append(char.ToLowerInvariant(c));
				}
			}
			return sb.ToString();
		}

		// Susun urutan menu per kelompok dengan pemisah: File | Edit | Compare/Bin | Diagnosa | Tuning | Lainnya.
		private void RearrangeMenus()
		{
			string[][] groups = new string[][]
			{
				new string[] { "file" },
				new string[] { "save", "undo", "redo" },
				new string[] { "compare", "tambahbin" },
				new string[] { "diagnostic", "resetcount", "tool" },
				new string[] { "breakpoint", "fuel", "ignition", "limiter" },
				new string[] { "settinganlain" }
			};
			List<ToolStripItem> all = new List<ToolStripItem>();
			foreach (ToolStripItem it in menuStrip7.Items)
			{
				if (!(it is ToolStripSeparator))
				{
					all.Add(it);
				}
			}
			List<ToolStripItem> ordered = new List<ToolStripItem>();
			List<int> breaks = new List<int>();
			foreach (string[] g in groups)
			{
				int before = ordered.Count;
				foreach (string key in g)
				{
					foreach (ToolStripItem it in all)
					{
						if (!ordered.Contains(it) && MenuKey(it).Contains(key))
						{
							ordered.Add(it);
						}
					}
				}
				if (ordered.Count > before)
				{
					breaks.Add(ordered.Count);
				}
			}
			foreach (ToolStripItem it in all)
			{
				if (!ordered.Contains(it))
				{
					ordered.Add(it);
				}
			}
			menuStrip7.SuspendLayout();
			menuStrip7.Items.Clear();
			int pos = 0;
			for (int i = 0; i < ordered.Count; i++)
			{
				menuStrip7.Items.Add(ordered[i]);
				pos++;
				if (breaks.Contains(pos) && pos < ordered.Count)
				{
					ToolStripSeparator sep = new ToolStripSeparator();
					sep.Margin = new Padding(4, 2, 4, 2);
					menuStrip7.Items.Add(sep);
				}
			}
			menuStrip7.ResumeLayout(true);
		}

		private void StyleMenus()
		{
			menuStrip7.Renderer = new H2Renderer();
			menuStrip7.BackColor = H2Black;
			menuStrip7.ForeColor = H2Text;
			menuStrip7.Padding = new Padding(3, 2, 3, 2);
			menuStrip7.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
			menuStrip7.AutoSize = true;
			menuStrip7.GripStyle = ToolStripGripStyle.Hidden;
			Font f = new Font("Segoe UI Semibold", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
			foreach (ToolStripItem it in menuStrip7.Items)
			{
				StyleMenuItem(it, f, true);
			}
		}

		private void StyleMenuItem(ToolStripItem it, Font f, bool top)
		{
			if (it is ToolStripSeparator)
			{
				return;
			}
			it.ForeColor = H2Text;
			it.BackColor = H2Black;
			it.Font = f;
			if (top)
			{
				it.Padding = new Padding(5, 3, 5, 3);
			}
			else
			{
				it.BackColor = Color.FromArgb(16, 16, 20);
				it.Padding = new Padding(2, 3, 2, 3);
			}
			ToolStripMenuItem mi = it as ToolStripMenuItem;
			if (mi != null)
			{
				mi.DropDown.BackColor = Color.FromArgb(16, 16, 20);
				mi.DropDown.ForeColor = H2Text;
				if (mi.DropDown is ToolStripDropDownMenu)
				{
					((ToolStripDropDownMenu)mi.DropDown).ShowImageMargin = true;
				}
				foreach (ToolStripItem child in mi.DropDownItems)
				{
					StyleMenuItem(child, new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point), false);
				}
			}
		}

		private void ThemeControl(Control c)
		{
			try
			{
				if (c is Form)
				{
					c.BackColor = H2Black;
					c.ForeColor = H2Text;
				}
				else if (c is Button)
				{
					Button b = (Button)c;
					b.FlatStyle = FlatStyle.Flat;
					b.FlatAppearance.BorderColor = H2Red;
					b.FlatAppearance.BorderSize = 1;
					b.FlatAppearance.MouseOverBackColor = H2DarkRed;
					b.FlatAppearance.MouseDownBackColor = Color.FromArgb(160, 16, 30);
					b.BackColor = H2Button;
					b.ForeColor = H2Text;
				}
				else if (c is TextBox)
				{
					TextBox t = (TextBox)c;
					t.BackColor = H2Panel;
					t.ForeColor = H2Text;
					t.BorderStyle = BorderStyle.FixedSingle;
				}
				else if (c is ComboBox)
				{
					ComboBox cb = (ComboBox)c;
					cb.FlatStyle = FlatStyle.Flat;
					cb.BackColor = H2Panel;
					cb.ForeColor = H2Text;
				}
				else if (c is GroupBox)
				{
					c.ForeColor = H2RedBright;
				}
				else if (c is CheckBox)
				{
					c.ForeColor = H2Text;
				}
				else if (c is Label)
				{
					Label l = (Label)c;
					if (l.Width >= 120 && l.Height >= 40)
					{
						l.ForeColor = H2RedBright;
					}
					else
					{
						l.ForeColor = H2Text;
					}
				}
				else if (c is DataGridView)
				{
					DataGridView g = (DataGridView)c;
					g.BackgroundColor = H2Black;
					g.GridColor = Color.FromArgb(70, 22, 28);
					g.EnableHeadersVisualStyles = false;
					g.ColumnHeadersDefaultCellStyle.BackColor = H2DarkRed;
					g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
					g.RowHeadersDefaultCellStyle.BackColor = H2DarkRed;
					g.RowHeadersDefaultCellStyle.ForeColor = Color.White;
				}
				else if (c is Panel && c.BackgroundImage == null)
				{
					c.BackColor = Color.FromArgb(14, 14, 18);
				}
			}
			catch (Exception)
			{
			}
			foreach (Control child in c.Controls)
			{
				ThemeControl(child);
			}
		}

		// Kotak input "Value" (txtInput) dilebarkan; baris Function/Value/Execute disusun ulang tanpa menabrak kotak lain.
		private void WidenValueInput()
		{
			try
			{
				if (groupBox7 == null || comboBox2 == null || txtInput == null || button2 == null) return;
				// Jaga baris Function / Value / Execute tetap sejajar dan tidak menimpa kolom nama BIN.
				groupBox7.SetBounds(166, 114, 299, 37);
				label19.SetBounds(6, 9, 56, 22);
				comboBox2.SetBounds(64, 9, 78, 23);
				label20.SetBounds(145, 9, 38, 22);
				txtInput.SetBounds(184, 8, 36, 24);
				button2.SetBounds(224, 8, 64, 24);
				txtInput.Multiline = false;
				txtInput.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
				txtInput.TextAlign = HorizontalAlignment.Center;
				button2.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
			}
			catch (Exception) { }
		}

		// Window mengikuti area kerja layar (di luar taskbar), baik saat normal maupun maximize.
		private bool h2Clamping;

		private void SetWindowFitsWorkingArea()
		{
			if (h2Clamping || IsDisposed)
			{
				return;
			}
			h2Clamping = true;
			try
			{
				Rectangle wa = Screen.FromControl(this).WorkingArea;
				MaximizedBounds = wa;
				MinimumSize = new Size(Math.Min(1120, wa.Width), Math.Min(640, wa.Height));
				if (WindowState == FormWindowState.Normal)
				{
					int w = Math.Min(Width, wa.Width);
					int h = Math.Min(Height, wa.Height);
					int x = Math.Max(wa.Left, Math.Min(Left, wa.Right - w));
					int y = Math.Max(wa.Top, Math.Min(Top, wa.Bottom - h));
					if (w != Width || h != Height || x != Left || y != Top)
					{
						SetBounds(x, y, w, h);
					}
				}
				FormBorderStyle = FormBorderStyle.Sizable;
				MaximizeBox = true;
				MinimizeBox = true;
			}
			catch (Exception)
			{
			}
			finally
			{
				h2Clamping = false;
			}
		}

		// Ikon window (menggantikan logo Honda) dibuat dari gambar splash_tunex bila tersedia.
		private void SetH2Icon()
		{
			try
			{
				Image img = null;
				System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
				foreach (string rn in asm.GetManifestResourceNames())
				{
					if (rn.EndsWith("splash_tunex.jpg", StringComparison.OrdinalIgnoreCase) || rn.EndsWith("logo_h2m.png", StringComparison.OrdinalIgnoreCase))
					{
						using (System.IO.Stream st = asm.GetManifestResourceStream(rn))
						{
							if (st != null)
							{
								using (Image tmp = Image.FromStream(st))
								{
									img = new Bitmap(tmp);
								}
							}
						}
						break;
					}
				}
				if (img == null)
				{
					return;
				}
				using (Bitmap bmp = new Bitmap(32, 32))
				{
					using (Graphics g = Graphics.FromImage(bmp))
					{
						g.InterpolationMode = InterpolationMode.HighQualityBicubic;
						g.Clear(H2Black);
						g.DrawImage(img, new Rectangle(0, 0, 32, 32));
					}
					Icon = Icon.FromHandle(bmp.GetHicon());
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
