using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace _3D_Chart_Graph
{
	// Bandingkan full dua bin (sebelum vs sesudah remap): semua tabel tipe ECU aktif + seluruh byte.
	public partial class Form1
	{
		private class CmpTable
		{
			public string Name;
			public int Addr;
			public int Cols;
			public int Rows;
			public bool Wide;
			public int Bytes { get { return Cols * Rows * (Wide ? 2 : 1); } }
		}

		private List<CmpTable> CurrentTableList()
		{
			List<CmpTable> list = new List<CmpTable>();
			Action<string, int, int, int, bool> add = delegate(string n, int a, int c, int r, bool w)
			{
				if (a > 0 && c > 0 && r > 0)
				{
					list.Add(new CmpTable { Name = n, Addr = a, Cols = c, Rows = r, Wide = w });
				}
			};
			add("Sumbu TPS", tpsh, columnCount, 1, true);
			add("Sumbu RPM", rpmh, rowCount, 1, true);
			add("Fuel 1", feul1, columnCount, rowCount, true);
			add("Fuel 2", feul2, columnCount, rowCount, true);
			add("Fuel 3", feul3, columnCount, rowCount, true);
			add("Ignition 1", ig1, columnCount, rowCount, false);
			add("Ignition 2", ig2, columnCount, rowCount, false);
			add("Ignition 3", ig3, columnCount, rowCount, false);
			add("Ignition 4", ig4, columnCount, rowCount, false);
			int[] lim = new int[] { limit1, limit2, limit3, limit4, limit5, limit6, limit7 };
			for (int i = 0; i < lim.Length; i++)
			{
				add("Limiter " + (i + 1), lim[i], 2, 1, true);
			}
			add("O2", o2, 1, 1, false);
			add("O2 Close", o2c, 1, 1, false);
			add("IMMO", IMMO, 1, 1, false);
			int[] modes = new int[] { MODE0, MODE1, MODE2, MODE3, MODE4 };
			for (int i = 0; i < modes.Length; i++)
			{
				add("MODE" + i, modes[i], 1, 1, false);
			}
			return list;
		}

		private bool CurrentBigEndian
		{
			get { return feul1 == 239620 || feul1 == 233756 || customBigEndian; }
		}

		private void ShowFullCompare()
		{
			byte[] before = (binBytesORI != null) ? (byte[])binBytesORI.Clone() : null;
			byte[] after = null;
			if (binBytes != null)
			{
				try
				{
					RunWithRealValues(() => SaveEditedValues2());
				}
				catch (Exception)
				{
				}
				after = (byte[])binBytes.Clone();
			}
			using (FullCompareForm f = new FullCompareForm(this, before, after))
			{
				f.ShowDialog(this);
			}
		}

		private class FullCompareForm : Form
		{
			private readonly Form1 owner;
			private byte[] before;
			private byte[] after;
			private readonly Label lblBefore = new Label();
			private readonly Label lblAfter = new Label();
			private readonly Label lblSummary = new Label();
			private readonly DataGridView gridSummary = new DataGridView();
			private readonly DataGridView gridDetail = new DataGridView();
			private readonly DataGridView gridRaw = new DataGridView();
			private readonly TabControl tabs = new TabControl();
			private List<CmpTable> tables;
			private readonly bool bigEndian;

			public FullCompareForm(Form1 owner, byte[] before, byte[] after)
			{
				this.owner = owner;
				this.before = before;
				this.after = after;
				tables = owner.CurrentTableList();
				bigEndian = owner.CurrentBigEndian;
				Text = "Bandingkan Full - Sebelum vs Sesudah Remap";
				StartPosition = FormStartPosition.CenterParent;
				Size = new Size(1100, 700);
				MinimumSize = new Size(800, 500);
				BackColor = Color.FromArgb(24, 24, 24);
				ForeColor = Color.White;
				Font = new Font("Segoe UI", 9f);

				Panel top = new Panel { Dock = DockStyle.Top, Height = 96 };
				Button bBefore = MakeButton("Pilih SEBELUM...", 8, 8);
				Button bAfter = MakeButton("Pilih SESUDAH...", 8, 40);
				lblBefore.SetBounds(160, 12, 900, 22);
				lblAfter.SetBounds(160, 44, 900, 22);
				lblBefore.Anchor = lblAfter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
				Button bSwap = MakeButton("Tukar", 8, 70);
				bSwap.Height = 24;
				Button bExport = MakeButton("Export CSV...", 160, 70);
				bExport.Height = 24;
				lblSummary.SetBounds(312, 72, 760, 22);
				lblSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
				lblSummary.ForeColor = Color.Yellow;
				top.Controls.AddRange(new Control[] { bBefore, bAfter, lblBefore, lblAfter, bSwap, bExport, lblSummary });
				bBefore.Click += delegate { byte[] b = PickBin("Pilih bin SEBELUM remap (file ori)"); if (b != null) { this.before = b; Recalc(); } };
				bAfter.Click += delegate { byte[] b = PickBin("Pilih bin SESUDAH remap"); if (b != null) { this.after = b; Recalc(); } };
				bSwap.Click += delegate { byte[] t = this.before; this.before = this.after; this.after = t; Recalc(); };
				bExport.Click += delegate { ExportCsv(); };

				StyleGrid(gridSummary);
				StyleGrid(gridDetail);
				StyleGrid(gridRaw);
				gridSummary.Columns.Add("t", "Tabel");
				gridSummary.Columns.Add("a", "Alamat");
				gridSummary.Columns.Add("u", "Ukuran");
				gridSummary.Columns.Add("c", "Sel berubah");
				gridSummary.Columns.Add("mn", "\u0394 min");
				gridSummary.Columns.Add("mx", "\u0394 max");
				gridSummary.Columns.Add("av", "\u0394 rata-rata %");
				gridSummary.SelectionChanged += delegate { ShowDetail(); };
				gridRaw.Columns.Add("a", "Alamat");
				gridRaw.Columns.Add("n", "Panjang");
				gridRaw.Columns.Add("b", "Sebelum (hex)");
				gridRaw.Columns.Add("s", "Sesudah (hex)");
				gridRaw.Columns.Add("t", "Area");
				gridRaw.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
				gridRaw.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

				TabPage p1 = new TabPage("Ringkasan semua tabel");
				TabPage p2 = new TabPage("Detail tabel (sebelum \u2192 sesudah)");
				TabPage p3 = new TabPage("Semua byte berubah (full)");
				p1.Controls.Add(gridSummary);
				p2.Controls.Add(gridDetail);
				p3.Controls.Add(gridRaw);
				tabs.TabPages.AddRange(new TabPage[] { p1, p2, p3 });
				tabs.Dock = DockStyle.Fill;
				Controls.Add(tabs);
				Controls.Add(top);
				Load += delegate { Recalc(); };
			}

			private Button MakeButton(string text, int x, int y)
			{
				Button b = new Button { Text = text, Left = x, Top = y, Width = 144, Height = 28, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
				return b;
			}

			private static void StyleGrid(DataGridView g)
			{
				g.Dock = DockStyle.Fill;
				g.ReadOnly = true;
				g.AllowUserToAddRows = false;
				g.AllowUserToDeleteRows = false;
				g.RowHeadersWidth = 70;
				g.BackgroundColor = Color.Black;
				g.DefaultCellStyle.BackColor = Color.Black;
				g.DefaultCellStyle.ForeColor = Color.White;
				g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 90, 160);
				g.EnableHeadersVisualStyles = false;
				g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
				g.ColumnHeadersDefaultCellStyle.ForeColor = Color.Cyan;
				g.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
				g.RowHeadersDefaultCellStyle.ForeColor = Color.Cyan;
				g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
				typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, true, null);
			}

			private byte[] PickBin(string title)
			{
				using (OpenFileDialog dlg = new OpenFileDialog())
				{
					dlg.Title = title;
					dlg.Filter = "Bin ECU (*.bin;*.acg)|*.bin;*.acg|Semua file (*.*)|*.*";
					if (dlg.ShowDialog(this) != DialogResult.OK)
					{
						return null;
					}
					try
					{
						byte[] b = File.ReadAllBytes(dlg.FileName);
						if (dlg.FileName.EndsWith(".acg", StringComparison.OrdinalIgnoreCase))
						{
							b = owner.DecryptBytes2(b);
						}
						return b;
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, "Gagal membaca file: " + ex.Message, "Bandingkan", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return null;
					}
				}
			}

			private int Val(byte[] b, CmpTable t, int i)
			{
				if (!t.Wide)
				{
					int p = t.Addr + i;
					return (p < b.Length) ? b[p] : 0;
				}
				int q = t.Addr + i * 2;
				if (q + 1 >= b.Length)
				{
					return 0;
				}
				return bigEndian ? ((b[q] << 8) | b[q + 1]) : (b[q] | (b[q + 1] << 8));
			}

			private bool Ready
			{
				get { return before != null && after != null && before.Length == after.Length; }
			}

			private void Recalc()
			{
				lblBefore.Text = (before == null) ? "SEBELUM: (belum ada, klik Pilih)" : "SEBELUM: " + before.Length.ToString("N0") + " byte";
				lblAfter.Text = (after == null) ? "SESUDAH: (belum ada, klik Pilih)" : "SESUDAH: " + after.Length.ToString("N0") + " byte";
				gridSummary.Rows.Clear();
				gridRaw.Rows.Clear();
				gridDetail.Columns.Clear();
				if (before == null || after == null)
				{
					lblSummary.Text = "Pilih bin sebelum dan sesudah remap.";
					return;
				}
				if (before.Length != after.Length)
				{
					lblSummary.Text = "Ukuran file berbeda, tidak bisa dibandingkan (tipe ECU harus sama).";
					return;
				}
				int changedBytes = 0;
				for (int i = 0; i < before.Length; i++)
				{
					if (before[i] != after[i])
					{
						changedBytes++;
					}
				}
				foreach (CmpTable t in tables)
				{
					if (t.Addr + t.Bytes > before.Length)
					{
						continue;
					}
					int n = t.Cols * t.Rows;
					int changed = 0;
					int mn = int.MaxValue;
					int mx = int.MinValue;
					double pctSum = 0.0;
					for (int i = 0; i < n; i++)
					{
						int a = Val(before, t, i);
						int b = Val(after, t, i);
						if (a != b)
						{
							changed++;
							int d = b - a;
							mn = Math.Min(mn, d);
							mx = Math.Max(mx, d);
							pctSum += (a != 0) ? (100.0 * d / a) : 0.0;
						}
					}
					int row = gridSummary.Rows.Add(t.Name, "0x" + t.Addr.ToString("X"), t.Cols + "x" + t.Rows, changed + " / " + n, changed > 0 ? mn.ToString("+0;-0;0") : "-", changed > 0 ? mx.ToString("+0;-0;0") : "-", changed > 0 ? (pctSum / changed).ToString("+0.00;-0.00;0.00") + " %" : "-");
					gridSummary.Rows[row].Tag = t;
					if (changed > 0)
					{
						gridSummary.Rows[row].DefaultCellStyle.ForeColor = Color.Orange;
					}
				}
				int ranges = 0;
				int start = -1;
				for (int i = 0; i <= before.Length; i++)
				{
					bool diff = i < before.Length && before[i] != after[i];
					if (diff && start < 0)
					{
						start = i;
					}
					else if (!diff && start >= 0)
					{
						int gapEnd = i;
						while (gapEnd < before.Length && gapEnd < i + 3 && before[gapEnd] == after[gapEnd])
						{
							gapEnd++;
						}
						if (gapEnd < before.Length && gapEnd < i + 3)
						{
							continue;
						}
						int len = i - start;
						if (ranges < 20000)
						{
							gridRaw.Rows.Add("0x" + start.ToString("X6"), len, Hex(before, start, len), Hex(after, start, len), AreaOf(start));
						}
						ranges++;
						start = -1;
					}
				}
				lblSummary.Text = "Byte berubah: " + changedBytes.ToString("N0") + " dari " + before.Length.ToString("N0") + " (" + (100.0 * changedBytes / before.Length).ToString("F2") + "%), " + ranges.ToString("N0") + " blok" + (changedBytes == 0 ? "  -  FILE IDENTIK" : "");
				ShowDetail();
			}

			private static string Hex(byte[] b, int start, int len)
			{
				int n = Math.Min(len, 32);
				string s = BitConverter.ToString(b, start, n).Replace("-", " ");
				return (len > n) ? s + " ..." : s;
			}

			private string AreaOf(int addr)
			{
				foreach (CmpTable t in tables)
				{
					if (addr >= t.Addr && addr < t.Addr + t.Bytes)
					{
						return t.Name;
					}
				}
				return "-";
			}

			private void ShowDetail()
			{
				gridDetail.Columns.Clear();
				if (!Ready || gridSummary.SelectedRows.Count == 0)
				{
					return;
				}
				CmpTable t = gridSummary.SelectedRows[0].Tag as CmpTable;
				if (t == null)
				{
					return;
				}
				CmpTable tps = tables.Find(x => x.Name == "Sumbu TPS");
				CmpTable rpm = tables.Find(x => x.Name == "Sumbu RPM");
				for (int c = 0; c < t.Cols; c++)
				{
					string h = (tps != null && t.Rows > 1 && t.Cols == tps.Cols) ? Val(after, tps, c).ToString() : (c + 1).ToString();
					gridDetail.Columns.Add("c" + c, h);
					gridDetail.Columns[c].SortMode = DataGridViewColumnSortMode.NotSortable;
				}
				float k = (owner.kon > 0.01f) ? owner.kon : 1f;
				for (int r = 0; r < t.Rows; r++)
				{
					object[] cells = new object[t.Cols];
					for (int c = 0; c < t.Cols; c++)
					{
						int i = r * t.Cols + c;
						int a = Val(before, t, i);
						int b = Val(after, t, i);
						cells[c] = (a == b) ? a.ToString() : (a + " \u2192 " + b);
					}
					int row = gridDetail.Rows.Add(cells);
					gridDetail.Rows[row].HeaderCell.Value = (rpm != null && t.Rows > 1 && t.Rows == rpm.Cols) ? Math.Round(Val(after, rpm, r) / k).ToString(CultureInfo.InvariantCulture) : (r + 1).ToString();
					for (int c = 0; c < t.Cols; c++)
					{
						int i = r * t.Cols + c;
						int a = Val(before, t, i);
						int b = Val(after, t, i);
						if (b > a)
						{
							gridDetail[c, row].Style.BackColor = Color.FromArgb(0, 90, 0);
						}
						else if (b < a)
						{
							gridDetail[c, row].Style.BackColor = Color.FromArgb(120, 0, 0);
						}
					}
				}
				tabs.SelectedIndex = (tabs.SelectedIndex == 0) ? 0 : tabs.SelectedIndex;
			}

			private void ExportCsv()
			{
				if (!Ready)
				{
					return;
				}
				using (SaveFileDialog dlg = new SaveFileDialog())
				{
					dlg.Filter = "CSV (*.csv)|*.csv";
					dlg.FileName = "compare_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".csv";
					if (dlg.ShowDialog(this) != DialogResult.OK)
					{
						return;
					}
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("RINGKASAN;" + lblSummary.Text);
					sb.AppendLine();
					sb.AppendLine("Tabel;Alamat;Ukuran;Sel berubah;Delta min;Delta max;Delta rata %");
					foreach (DataGridViewRow r in gridSummary.Rows)
					{
						List<string> v = new List<string>();
						foreach (DataGridViewCell c in r.Cells)
						{
							v.Add(Convert.ToString(c.Value));
						}
						sb.AppendLine(string.Join(";", v.ToArray()));
					}
					sb.AppendLine();
					sb.AppendLine("Tabel;Baris;Kolom;Sebelum;Sesudah;Selisih");
					foreach (CmpTable t in tables)
					{
						if (t.Addr + t.Bytes > before.Length)
						{
							continue;
						}
						for (int i = 0; i < t.Cols * t.Rows; i++)
						{
							int a = Val(before, t, i);
							int b = Val(after, t, i);
							if (a != b)
							{
								sb.AppendLine(t.Name + ";" + (i / t.Cols + 1) + ";" + (i % t.Cols + 1) + ";" + a + ";" + b + ";" + (b - a));
							}
						}
					}
					sb.AppendLine();
					sb.AppendLine("Alamat;Panjang;Sebelum;Sesudah;Area");
					foreach (DataGridViewRow r in gridRaw.Rows)
					{
						sb.AppendLine(r.Cells[0].Value + ";" + r.Cells[1].Value + ";" + r.Cells[2].Value + ";" + r.Cells[3].Value + ";" + r.Cells[4].Value);
					}
					try
					{
						File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
						MessageBox.Show(this, "Laporan disimpan: " + dlg.FileName, "Export", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, "Gagal menyimpan: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
				}
			}
		}
	}
}
