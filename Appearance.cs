using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace _3D_Chart_Graph
{
	// Pengaturan tampilan (background, logo pojok kanan bawah, teks berjalan), ukuran window,
	// dan Reset Count Keihin (memakai metode Reset ID Key: tulis ulang EEPROM ke 0).
	public partial class Form1
	{
		private Image defaultBackground;
		private ImageLayout defaultBackgroundLayout;
		private Image defaultLogo;
		private string defaultRunningText;
		private string runningText;
		private string backgroundFile;
		private string logoFile;
		private bool customBigEndian;
		private bool keihinResetRunning;
		private MarqueeLabel marquee;
		private readonly List<Button> quickButtons = new List<Button>();
		private readonly ToolTip quickTips = new ToolTip();

		private void AddQuickButton(string text, string tip, Action action)
		{
			Button b = new Button();
			b.Text = text;
			b.Font = BtnWrite.Font;
			b.FlatStyle = BtnWrite.FlatStyle;
			b.BackColor = BtnWrite.BackColor;
			b.ForeColor = BtnWrite.ForeColor;
			b.UseVisualStyleBackColor = BtnWrite.UseVisualStyleBackColor;
			b.Height = 32;
			b.Width = Math.Max(60, TextRenderer.MeasureText(text, b.Font).Width + 18);
			b.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			b.TabStop = false;
			b.Click += delegate { action(); };
			quickTips.SetToolTip(b, tip);
			Controls.Add(b);
			quickButtons.Add(b);
		}

		private static Image LoadEmbeddedImage(string name)
		{
			try
			{
				using (Stream s = typeof(Form1).Assembly.GetManifestResourceStream(name))
				{
					if (s == null)
					{
						return null;
					}
					using (Image img = Image.FromStream(s))
					{
						return new Bitmap(img);
					}
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		// Posisi kontrol bagian atas supaya tetap rapi saat window di-maximize.
		private void TidyMainLayout()
		{
			label1.Visible = false;
			TbFileName.Visible = false;
			comboBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			TxtPartCode.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			comboBox1.Width = 299;
			TxtPartCode.Width = 299;
			groupBox7.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			groupBox7.SetBounds(166, 114, 299, groupBox7.Height);
			button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			AddQuickButton("Compare", "Bandingkan full: sebelum vs sesudah remap", ShowFullCompare);
			AddQuickButton("Undo", "Batalkan perubahan terakhir (Ctrl+Z)", Undo);
			AddQuickButton("Redo", "Ulangi perubahan yang dibatalkan", Redo);
			AddQuickButton("Open", "Buka file bin", () => button2_Click(this, EventArgs.Empty));
			AddQuickButton("Save", "Simpan bin", () => button9_Click(this, EventArgs.Empty));
			LayoutToolbarRow();
			Resize += delegate { LayoutToolbarRow(); };
		}

		private void LayoutToolbarRow()
		{
			Control[] row = new Control[] { BtnWrite, button1, show_all, button8, button12, CbFastWrite, label15, TbStartOffset, label16, TbChecksumOffset };
			const int gap = 6;
			int total = gap * (row.Length - 1);
			foreach (Control c in row)
			{
				total += c.Width;
			}
			int x = Math.Max(groupBox7.Right + 10, ClientSize.Width - 8 - total);
			const int rowTop = 119;
			const int rowHeight = 32;
			button3.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			button3.SetBounds(8, panel4.Top, Math.Max(100, panel4.Left - 14), panel4.Height);
			int qx = groupBox7.Right + 10;
			foreach (Button b in quickButtons)
			{
				bool fits = qx + b.Width <= x - 8;
				b.Visible = fits;
				if (fits)
				{
					b.Location = new Point(qx, rowTop + (rowHeight - b.Height) / 2);
					qx += b.Width + gap;
				}
			}
			foreach (Control c in row)
			{
				c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				c.Location = new Point(x, rowTop + (rowHeight - c.Height) / 2);
				x += c.Width + gap;
			}
		}

		private static string AppearanceDir
		{
			get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EcuTuneX"); }
		}

		private static string AppearanceFile
		{
			get { return Path.Combine(AppearanceDir, "tampilan.txt"); }
		}

		private void InitAppearance()
		{
			// Window selalu penuh layar: tanpa tombol restore/maximize, minimize tetap ada.
			FormBorderStyle = FormBorderStyle.Fixed3D;
			MaximizeBox = false;
			MinimizeBox = true;
			MinimumSize = new Size(1120, 640);
			Resize += delegate
			{
				if (WindowState == FormWindowState.Normal && IsHandleCreated)
				{
					BeginInvoke((Action)(() => WindowState = FormWindowState.Maximized));
				}
			};
			TidyMainLayout();
			WindowState = FormWindowState.Maximized;
			LayoutToolbarRow();
			defaultBackground = LoadEmbeddedImage("EcuTuneX.bg_h2m.jpg") ?? panelDATA.BackgroundImage;
			defaultBackgroundLayout = ImageLayout.Stretch;
			defaultLogo = LoadEmbeddedImage("EcuTuneX.logo_h2m.png") ?? pictureBox1.Image;
			panelDATA.BackgroundImage = defaultBackground;
			panelDATA.BackgroundImageLayout = defaultBackgroundLayout;
			pictureBox1.Image = defaultLogo;
			pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			pictureBox1.Width = 176;
			pictureBox1.Height = 40;
			pictureBox1.Left = Math.Max(0, ClientSize.Width - pictureBox1.Width - 12);
			pictureBox1.Top = Math.Max(0, ClientSize.Height - pictureBox1.Height - 8);
			defaultRunningText = (label3.Text.Trim().Length > 0 && label3.Text != "label3") ? label3.Text : "Ecu TuneX";
			TxtPb.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			marquee = new MarqueeLabel();
			marquee.Font = label14.Font;
			marquee.ForeColor = label14.ForeColor;
			marquee.BackColor = Color.Black;
			marquee.Text = defaultRunningText;
			marquee.SetBounds(TxtPb.Right + 6, 1, Math.Max(80, panel4.ClientSize.Width - TxtPb.Right - 8), panel4.ClientSize.Height - 2);
			marquee.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			panel4.Controls.Add(marquee);
			marquee.BringToFront();
			label3.Visible = false;
			label14.Visible = false;
			if (labelMovementTimer != null)
			{
				labelMovementTimer.Stop();
			}
			BuildAppearanceMenu();
			BuildPinoutMenu();
			LoadAppearance();
		}

		private void BuildAppearanceMenu()
		{
			ToolStripMenuItem root = new ToolStripMenuItem("Tampilan");
			root.Font = fileToolStripMenuItem.Font;
			ToolStripMenuItem bg = new ToolStripMenuItem("Ganti background...");
			bg.Click += delegate { ChooseBackground(); };
			ToolStripMenuItem logo = new ToolStripMenuItem("Ganti logo (pojok kanan bawah)...");
			logo.Click += delegate { ChooseLogo(); };
			ToolStripMenuItem text = new ToolStripMenuItem("Ubah teks berjalan...");
			text.Click += delegate { EditRunningText(); };
			ToolStripMenuItem reset = new ToolStripMenuItem("Kembalikan tampilan default");
			reset.Click += delegate { ResetAppearance(); };
			root.DropDownItems.AddRange(new ToolStripItem[] { bg, logo, text, new ToolStripSeparator(), reset });
			menuStrip7.Items.Add(root);
		}

		private void BuildPinoutMenu()
		{
			foreach (ToolStripItem item in menuStrip7.Items)
			{
				if (string.Equals(item.Text, "Pinout ECU", StringComparison.OrdinalIgnoreCase)) return;
			}
			ToolStripMenuItem pinout = new ToolStripMenuItem("\ud83d\udd0c Pinout ECU");
			pinout.ToolTipText = "Referensi gambar pinout ECM Honda";
			pinout.Click += delegate { ShowPinoutEcm(); };
			menuStrip7.Items.Add(pinout);
		}

		private void ShowPinoutEcm()
		{
			using (PinoutEcmForm form = new PinoutEcmForm())
			{
				form.StartPosition = FormStartPosition.CenterParent;
				form.ShowDialog(this);
			}
		}

		private static string PickImage(string title)
		{
			using (OpenFileDialog dlg = new OpenFileDialog())
			{
				dlg.Title = title;
				dlg.Filter = "Gambar (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Semua file (*.*)|*.*";
				return (dlg.ShowDialog() == DialogResult.OK) ? dlg.FileName : null;
			}
		}

		// Gambar dibaca ke memori supaya file tidak terkunci.
		private static Image LoadImageFile(string path)
		{
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return null;
			}
			try
			{
				using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
				using (Image img = Image.FromStream(ms))
				{
					return new Bitmap(img);
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static string CopyToAppearanceDir(string source, string baseName)
		{
			Directory.CreateDirectory(AppearanceDir);
			foreach (string old in Directory.GetFiles(AppearanceDir, baseName + ".*"))
			{
				try
				{
					File.Delete(old);
				}
				catch (Exception)
				{
				}
			}
			string target = Path.Combine(AppearanceDir, baseName + Path.GetExtension(source).ToLowerInvariant());
			File.Copy(source, target, true);
			return target;
		}

		private void ChooseBackground()
		{
			string file = PickImage("Pilih gambar background");
			if (file == null)
			{
				return;
			}
			Image img = LoadImageFile(file);
			if (img == null)
			{
				MessageBox.Show("File gambar tidak bisa dibuka.", "Ganti Background", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			try
			{
				backgroundFile = CopyToAppearanceDir(file, "background");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Gagal menyimpan background: " + ex.Message, "Ganti Background", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			ApplyBackground(img);
			SaveAppearance();
		}

		private void ApplyBackground(Image img)
		{
			panelDATA.BackgroundImage = img;
			panelDATA.BackgroundImageLayout = ImageLayout.Stretch;
			BackgroundImage = img;
			BackgroundImageLayout = ImageLayout.Stretch;
		}

		private void ChooseLogo()
		{
			string file = PickImage("Pilih gambar logo (pojok kanan bawah)");
			if (file == null)
			{
				return;
			}
			Image img = LoadImageFile(file);
			if (img == null)
			{
				MessageBox.Show("File gambar tidak bisa dibuka.", "Ganti Logo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			try
			{
				logoFile = CopyToAppearanceDir(file, "logo");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Gagal menyimpan logo: " + ex.Message, "Ganti Logo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			pictureBox1.Image = img;
			pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			pictureBox1.Width = 176;
			pictureBox1.Height = 40;
			pictureBox1.Left = Math.Max(0, ClientSize.Width - pictureBox1.Width - 12);
			pictureBox1.Top = Math.Max(0, ClientSize.Height - pictureBox1.Height - 8);
			SaveAppearance();
		}

		private void EditRunningText()
		{
			string current = string.IsNullOrEmpty(runningText) ? marquee.Text : runningText;
			string text = PromptText("Teks Berjalan", "Teks berjalan (kosongkan untuk teks default):", current);
			if (text == null)
			{
				return;
			}
			runningText = text.Replace("\r", " ").Replace("\n", " ");
			marquee.Text = (runningText.Length > 0) ? runningText : defaultRunningText;
			SaveAppearance();
		}

		private void ResetAppearance()
		{
			if (MessageBox.Show("Kembalikan background, logo dan teks berjalan ke default?", "Tampilan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}
			panelDATA.BackgroundImage = defaultBackground;
			panelDATA.BackgroundImageLayout = defaultBackgroundLayout;
			BackgroundImage = null;
			pictureBox1.Image = defaultLogo;
			pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			pictureBox1.Width = 176;
			pictureBox1.Height = 40;
			pictureBox1.Left = Math.Max(0, ClientSize.Width - pictureBox1.Width - 12);
			pictureBox1.Top = Math.Max(0, ClientSize.Height - pictureBox1.Height - 8);
			marquee.Text = defaultRunningText;
			runningText = "";
			backgroundFile = "";
			logoFile = "";
			try
			{
				if (File.Exists(AppearanceFile))
				{
					File.Delete(AppearanceFile);
				}
			}
			catch (Exception)
			{
			}
		}

		private void SaveAppearance()
		{
			try
			{
				Directory.CreateDirectory(AppearanceDir);
				File.WriteAllLines(AppearanceFile, new string[]
				{
					"background=" + (backgroundFile ?? ""),
					"logo=" + (logoFile ?? ""),
					"text=" + (runningText ?? "")
				}, Encoding.UTF8);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Gagal menyimpan pengaturan tampilan: " + ex.Message, "Tampilan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void LoadAppearance()
		{
			try
			{
				if (!File.Exists(AppearanceFile))
				{
					return;
				}
				Dictionary<string, string> values = new Dictionary<string, string>();
				foreach (string line in File.ReadAllLines(AppearanceFile, Encoding.UTF8))
				{
					int eq = line.IndexOf('=');
					if (eq > 0)
					{
						values[line.Substring(0, eq)] = line.Substring(eq + 1);
					}
				}
				string v;
				if (values.TryGetValue("background", out v))
				{
					Image img = LoadImageFile(v);
					if (img != null)
					{
						backgroundFile = v;
						ApplyBackground(img);
					}
				}
				if (values.TryGetValue("logo", out v))
				{
					Image img = LoadImageFile(v);
					if (img != null)
					{
						logoFile = v;
						pictureBox1.Image = img;
						pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			pictureBox1.Width = 176;
			pictureBox1.Height = 40;
			pictureBox1.Left = Math.Max(0, ClientSize.Width - pictureBox1.Width - 12);
			pictureBox1.Top = Math.Max(0, ClientSize.Height - pictureBox1.Height - 8);
					}
				}
				if (values.TryGetValue("text", out v) && v.Length > 0)
				{
					runningText = v;
					marquee.Text = v;
				}
			}
			catch (Exception)
			{
			}
		}

		// ---------- Reset Count Keihin ----------
		private void KeihinResetCount_Click(object sender, EventArgs e)
		{
			if (keihinResetRunning)
			{
				MessageBox.Show("Reset Count Keihin sedang berjalan.", "Reset Count Keihin", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			if (MessageBox.Show("Reset Count Keihin memakai metode RESET ID KEY:\r\nseluruh EEPROM (256 word) ditulis ulang ke 0 (ID 000000000).\r\n\r\nKUNCI KONTAK OFF - ON lalu klik OK untuk mulai.", "Reset Count Keihin", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
			{
				return;
			}
			keihinResetRunning = true;
			string oldTitle = Text;
			Thread thread = new Thread(() =>
			{
				string result;
				try
				{
					result = KeihinResetEeprom(delegate(int percent)
					{
						BeginInvoke((Action)(() => { Text = oldTitle + "  |  Reset Count Keihin " + percent + "%"; }));
					});
				}
				catch (Exception ex)
				{
					result = "Reset Count Keihin gagal: " + ex.Message;
				}
				BeginInvoke((Action)(() =>
				{
					keihinResetRunning = false;
					Text = oldTitle;
					MessageBox.Show(this, result, "Reset Count Keihin", MessageBoxButtons.OK, result.StartsWith("RESET") ? MessageBoxIcon.Asterisk : MessageBoxIcon.Hand);
				}));
			})
			{
				IsBackground = true
			};
			thread.Start();
		}

		// Sama dengan writeeeprom1 di form Reset ID Key (nilai ID = 000000000).
		private string KeihinResetEeprom(Action<int> progress)
		{
			byte[] wake = new byte[4] { 254, 4, 114, 140 };
			byte[] init = new byte[5] { 114, 5, 0, 240, 153 };
			byte[] key1 = new byte[13] { 145, 145, 13, 223, 158, 141, 154, 134, 144, 138, 140, 155, 136 };
			byte[] key2 = new byte[13] { 145, 145, 13, 223, 146, 158, 134, 150, 139, 141, 134, 192, 106 };
			byte[] reply = new byte[256];
			uint replySize = 0u;
			if (!InitFtdiIso())
			{
				return "Interface tidak terhubung. Cek kabel / driver lalu coba lagi.";
			}
			try
			{
				foreach (byte[] cmd in new byte[][] { wake, init, key1, key2 })
				{
					SendCommand(cmd, cmd.Length, ref reply, ref replySize);
					Thread.Sleep(150);
				}
				int failed = 0;
				for (int i = 0; i <= 255; i++)
				{
					byte[] cmd = new byte[9] { 145, 145, 9, 65, (byte)i, 0, 0, 0, 0 };
					cmd[8] = (byte)(256 - (74 + i));
					if (!SendCommand(cmd, cmd.Length, ref reply, ref replySize))
					{
						failed++;
					}
					Thread.Sleep(50);
					if (i % 8 == 7 || i == 255)
					{
						progress((i + 1) * 100 / 256);
					}
				}
				if (failed > 0)
				{
					return "Reset Count Keihin selesai dengan " + failed + " dari 256 word gagal ditulis.\r\nOFF-ON kontak lalu ulangi.";
				}
				return "RESET COUNT KEIHIN SUKSES\r\nEEPROM / ID KEY sudah di reset (000000000).\r\nOFF-ON kontak lalu READ ulang untuk memastikan.";
			}
			finally
			{
				try
				{
					FTDI.FT_Close(ftHandle);
				}
				catch (Exception)
				{
				}
			}
		}
	}

	// Teks berjalan yang digambar sendiri (halus, tidak tertutup label lain).
	internal class MarqueeLabel : Control
	{
		private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
		private float offset;

		public MarqueeLabel()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
			timer.Interval = 30;
			timer.Tick += delegate
			{
				offset -= 2f;
				if (offset < -TextRenderer.MeasureText(Text ?? "", Font).Width)
				{
					offset = Width;
				}
				Invalidate();
			};
			timer.Start();
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			offset = Width;
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.Clear(BackColor);
			Size size = TextRenderer.MeasureText(Text ?? "", Font);
			TextRenderer.DrawText(e.Graphics, Text ?? "", Font, new Point((int)offset, (Height - size.Height) / 2), ForeColor, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
			ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				timer.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
