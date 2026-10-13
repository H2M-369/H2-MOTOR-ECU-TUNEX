using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace _3D_Chart_Graph
{
	// Fitur "Tambah Bin": menambah tipe ECU baru tanpa compile ulang.
	// Alamat tabel dicari otomatis dengan membandingkan bin baru ke semua tipe yang sudah ada,
	// hanya tabel yang lolos validasi yang dipakai, sisanya menunya disembunyikan.
	public partial class Form1
	{
		private class CustomProfile
		{
			public string Name;
			public string BinFile;
			public int Cols;
			public int Rows;
			public float Kon = 1f;
			public float Lm = 1f;
			public int O2Mask;
			public int O2cMask;
			public int ImmoMask;
			public string Template = "";
			public Dictionary<string, int> Addr = new Dictionary<string, int>();
			public string ItemText = "";
		}

		private readonly Dictionary<string, CustomProfile> customProfiles = new Dictionary<string, CustomProfile>();

		private static string CustomDir
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "custom_ecu"); }
		}

		private static string CustomIndexFile
		{
			get { return Path.Combine(CustomDir, "profiles.txt"); }
		}

		private byte[] ReadBinSource(string name)
		{
			if (name != null && name.StartsWith("@custom:"))
			{
				string path = name.Substring("@custom:".Length);
				return File.Exists(path) ? File.ReadAllBytes(path) : null;
			}
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			using (Stream stream = executingAssembly.GetManifestResourceStream(name))
			{
				if (stream == null)
				{
					return null;
				}
				using (MemoryStream memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					return memoryStream.ToArray();
				}
			}
		}

		// ---------- penyimpanan ----------
		private static string Serialize(CustomProfile p)
		{
			StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair<string, int> kv in p.Addr)
			{
				if (kv.Value != 0)
				{
					if (sb.Length > 0)
					{
						sb.Append(';');
					}
					sb.Append(kv.Key).Append('=').Append(kv.Value.ToString(CultureInfo.InvariantCulture));
				}
			}
			return string.Join("|", new string[]
			{
				p.Name, p.BinFile, p.Cols.ToString(CultureInfo.InvariantCulture), p.Rows.ToString(CultureInfo.InvariantCulture),
				p.Kon.ToString(CultureInfo.InvariantCulture), p.Lm.ToString(CultureInfo.InvariantCulture),
				p.O2Mask.ToString(CultureInfo.InvariantCulture), p.O2cMask.ToString(CultureInfo.InvariantCulture), p.ImmoMask.ToString(CultureInfo.InvariantCulture),
				p.Template.Replace("|", "/"), sb.ToString()
			});
		}

		private static CustomProfile Deserialize(string line)
		{
			string[] parts = line.Split('|');
			if (parts.Length < 11)
			{
				return null;
			}
			CustomProfile p = new CustomProfile();
			p.Name = parts[0];
			p.BinFile = parts[1];
			int.TryParse(parts[2], out p.Cols);
			int.TryParse(parts[3], out p.Rows);
			float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out p.Kon);
			float.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out p.Lm);
			int.TryParse(parts[6], out p.O2Mask);
			int.TryParse(parts[7], out p.O2cMask);
			int.TryParse(parts[8], out p.ImmoMask);
			p.Template = parts[9];
			foreach (string pair in parts[10].Split(';'))
			{
				string[] kv = pair.Split('=');
				int v;
				if (kv.Length == 2 && int.TryParse(kv[1], out v))
				{
					p.Addr[kv[0]] = v;
				}
			}
			return (p.Cols > 0 && p.Rows > 0 && p.Name.Length > 0) ? p : null;
		}

		private void SaveCustomIndex()
		{
			Directory.CreateDirectory(CustomDir);
			List<string> lines = new List<string>();
			foreach (CustomProfile p in customProfiles.Values)
			{
				lines.Add(Serialize(p));
			}
			File.WriteAllLines(CustomIndexFile, lines.ToArray(), Encoding.UTF8);
		}

		private void LoadCustomProfiles()
		{
			BuildCustomMenu();
			customProfiles.Clear();
			if (!File.Exists(CustomIndexFile))
			{
				return;
			}
			foreach (string line in File.ReadAllLines(CustomIndexFile, Encoding.UTF8))
			{
				if (line.Trim().Length == 0)
				{
					continue;
				}
				CustomProfile p = Deserialize(line);
				if (p == null || !File.Exists(Path.Combine(CustomDir, p.BinFile)))
				{
					continue;
				}
				AddCustomItem(p);
			}
		}

		private void AddCustomItem(CustomProfile p)
		{
			p.ItemText = (comboBox1.Items.Count + 1).ToString(CultureInfo.InvariantCulture) + ". " + p.Name;
			customProfiles[p.ItemText] = p;
			comboBox1.Items.Add(p.ItemText);
		}

		// ---------- menu ----------
		private void BuildCustomMenu()
		{
			ToolStripMenuItem root = new ToolStripMenuItem("\ud83d\udce5 Tambah Bin");
			root.Font = fileToolStripMenuItem.Font;
			ToolStripMenuItem add = new ToolStripMenuItem("Tambah bin tipe baru...");
			add.Click += AddBin_Click;
			ToolStripMenuItem del = new ToolStripMenuItem("Hapus bin tambahan...");
			del.Click += RemoveBin_Click;
			root.DropDownItems.AddRange(new ToolStripItem[] { add, del });
			int index = menuStrip7.Items.IndexOf(fileToolStripMenuItem) + 5;
			if (index > menuStrip7.Items.Count)
			{
				index = menuStrip7.Items.Count;
			}
			menuStrip7.Items.Insert(index, root);
		}

		// ---------- menerapkan profil ke aplikasi ----------
		private string ApplyCustomProfile(string itemText)
		{
			CustomProfile p = customProfiles[itemText];
			Func<string, int> g = delegate(string key)
			{
				int v;
				return p.Addr.TryGetValue(key, out v) ? v : 0;
			};
			columnCount = p.Cols;
			rowCount = p.Rows;
			customBigEndian = g("BE") == 1;
			kon = p.Kon;
			lm = p.Lm;
			o2mask = (byte)p.O2Mask;
			o2cmask = (byte)p.O2cMask;
			immomask = (byte)p.ImmoMask;
			tpsh = g("tpsh");
			rpmh = g("rpmh");
			iaph = g("iaph");
			iap = g("iap");
			feul1 = g("feul1");
			feul2 = g("feul2");
			feul3 = g("feul3");
			ig1 = g("ig1");
			ig2 = g("ig2");
			ig3 = g("ig3");
			ig4 = g("ig4");
			limit1 = g("limit1");
			limit2 = g("limit2");
			limit3 = g("limit3");
			limit4 = g("limit4");
			limit5 = g("limit5");
			limit6 = g("limit6");
			limit7 = g("limit7");
			o2 = g("o2");
			o2c = g("o2c");
			IMMO = g("IMMO");
			MODE0 = g("MODE0");
			MODE1 = g("MODE1");
			MODE2 = g("MODE2");
			MODE3 = g("MODE3");
			MODE4 = g("MODE4");
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			breakpointIAPToolStripMenuItem.Visible = iaph != 0;
			rpmVsIAPToolStripMenuItem.Visible = iap != 0;
			rpmVsTps1ToolStripMenuItem.Visible = feul1 != 0;
			rpmVsTps2ToolStripMenuItem.Visible = feul2 != 0;
			rpmVsToolStripMenuItem.Visible = feul3 != 0;
			toolStripMenuItem6.Visible = ig1 != 0;
			toolStripMenuItem7.Visible = ig2 != 0;
			toolStripMenuItem8.Visible = ig3 != 0;
			ignition4ToolStripMenuItem.Visible = ig4 != 0;
			ignition5ToolStripMenuItem.Visible = false;
			toolStripMenuItem10.Visible = limit1 != 0;
			toolStripMenuItem11.Visible = limit2 != 0;
			toolStripMenuItem12.Visible = limit3 != 0;
			toolStripMenuItem13.Visible = limit4 != 0;
			toolStripMenuItem14.Visible = limit5 != 0;
			limiter6ToolStripMenuItem.Visible = limit6 != 0;
			limiter7ToolStripMenuItem.Visible = limit7 != 0;
			toolStripMenuItem16.Visible = IMMO != 0;
			toolStripMenuItem17.Visible = o2 != 0;
			toolStripMenuItem18.Visible = o2c != 0;
			toolStripMenuItem19.Visible = MODE0 != 0;
			toolStripMenuItem20.Visible = MODE1 != 0;
			toolStripMenuItem21.Visible = MODE2 != 0;
			toolStripMenuItem22.Visible = MODE3 != 0;
			offMode4ToolStripMenuItem.Visible = MODE4 != 0;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			return "@custom:" + Path.Combine(CustomDir, p.BinFile);
		}

		// ---------- mencari template terbaik ----------
		private bool FindBestTemplate(byte[] data, out string templateName, out int cols, out int rows, out Dictionary<string, int> tpl, out float kon, out float lm, out int o2m, out int o2cm, out int immom, out LayoutResult best)
		{
			templateName = "";
			cols = 0;
			rows = 0;
			tpl = null;
			kon = 1f;
			lm = 1f;
			o2m = 0;
			o2cm = 0;
			immom = 0;
			best = null;
			HashSet<string> seen = new HashSet<string>();
			List<BuiltinEcu> templates = new List<BuiltinEcu>(BuiltinEcuTable.All);
			foreach (CustomProfile cp in customProfiles.Values)
			{
				StringBuilder sb = new StringBuilder();
				foreach (KeyValuePair<string, int> kv in cp.Addr)
				{
					if (kv.Value != 0)
					{
						sb.Append(kv.Key).Append('=').Append(kv.Value).Append(';');
					}
				}
				templates.Add(new BuiltinEcu { Name = cp.Name, Resource = "@custom:" + Path.Combine(CustomDir, cp.BinFile), Cols = cp.Cols, Rows = cp.Rows, Kon = cp.Kon, Lm = cp.Lm, O2Mask = cp.O2Mask, O2cMask = cp.O2cMask, ImmoMask = cp.ImmoMask, Addr = sb.ToString().TrimEnd(';') });
			}
			foreach (BuiltinEcu t in templates)
			{
				if (t.Addr.Length == 0 || !seen.Add(t.Resource))
				{
					continue;
				}
				byte[] refBytes;
				try
				{
					refBytes = ReadBinSource(t.Resource);
				}
				catch (Exception)
				{
					continue;
				}
				if (refBytes == null || refBytes.Length != data.Length)
				{
					continue;
				}
				Dictionary<string, int> dict = t.ToDict();
				if (dict["tpsh"] == 0 || dict["rpmh"] == 0)
				{
					continue;
				}
				LayoutResult r = EcuLayoutTools.Detect(refBytes, data, dict, t.Cols, t.Rows, t.Kon, t.Lm);
				if (r == null)
				{
					continue;
				}
				if (best == null || r.Score > best.Score || (r.Score == best.Score && r.Similarity > best.Similarity))
				{
					best = r;
					templateName = t.Name;
					cols = t.Cols;
					rows = t.Rows;
					tpl = dict;
					kon = t.Kon;
					lm = t.Lm;
					o2m = t.O2Mask;
					o2cm = t.O2cMask;
					immom = t.ImmoMask;
				}
			}
			return best != null;
		}

		private static string SuggestName(string path)
		{
			string file = Path.GetFileNameWithoutExtension(path) ?? "";
			Match m = Regex.Match(file, "[0-9]{5}-[0-9A-Za-z]{2,5}(-[0-9A-Za-z]{3})?");
			string s = m.Success ? m.Value.ToUpperInvariant() : Regex.Replace(file, "[^0-9A-Za-z_ .-]", "").Trim().ToUpperInvariant();
			if (s.Length == 0)
			{
				s = "30400-";
			}
			return Regex.Replace(s, "(?<=-)KO", "K0");
		}

		private static string PromptText(string title, string label, string defaultValue)
		{
			using (Form form = new Form())
			{
				form.Text = title;
				form.StartPosition = FormStartPosition.CenterParent;
				form.FormBorderStyle = FormBorderStyle.FixedDialog;
				form.MaximizeBox = false;
				form.MinimizeBox = false;
				form.ClientSize = new Size(420, 110);
				Label lbl = new Label { Text = label, Left = 12, Top = 12, Width = 396, Height = 32 };
				TextBox box = new TextBox { Left = 12, Top = 48, Width = 396, Text = defaultValue };
				Button ok = new Button { Text = "OK", Left = 232, Top = 76, Width = 84, DialogResult = DialogResult.OK };
				Button cancel = new Button { Text = "Batal", Left = 324, Top = 76, Width = 84, DialogResult = DialogResult.Cancel };
				form.Controls.AddRange(new Control[] { lbl, box, ok, cancel });
				form.AcceptButton = ok;
				form.CancelButton = cancel;
				return (form.ShowDialog() == DialogResult.OK) ? box.Text.Trim() : null;
			}
		}

		private void AddBin_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dlg = new OpenFileDialog())
			{
				dlg.Filter = "Bin ECU (*.bin)|*.bin|Semua file (*.*)|*.*";
				dlg.Title = "Pilih file bin tipe ECU baru";
				if (dlg.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				byte[] data;
				try
				{
					data = File.ReadAllBytes(dlg.FileName);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Gagal membaca file: " + ex.Message, "Tambah Bin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				AddBinFromData(data, SuggestName(dlg.FileName));
			}
		}

		// Daftarkan file BIN sebagai tipe ECU baru di list.
		private bool AddBinFromData(byte[] data, string suggestedName)
		{
			string templateName;
			int cols;
			int rows;
			Dictionary<string, int> tpl;
			float kon;
			float lm;
			int o2m;
			int o2cm;
			int immom;
			LayoutResult result;
			Cursor = Cursors.WaitCursor;
			bool ok;
			try
			{
				ok = FindBestTemplate(data, out templateName, out cols, out rows, out tpl, out kon, out lm, out o2m, out o2cm, out immom, out result);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
			if (!ok && !ManualLayout(data, out templateName, out cols, out rows, out tpl, out kon, out lm, out o2m, out o2cm, out immom, out result))
			{
			return false;
			}
			string name = PromptText("Tambah Bin", "Nama tipe ECU (kode part), contoh 30400-K2VM-NC1:", suggestedName);
			if (string.IsNullOrEmpty(name))
			{
			return false;
			}
			name = name.ToUpperInvariant();
			name = Regex.Replace(name, "\\s+", " ").Trim();
			if (name.Length > 40 || !Regex.IsMatch(name, "^[0-9A-Z][0-9A-Z _.()-]*$"))
			{
				MessageBox.Show("Nama hanya boleh huruf, angka, spasi, titik, strip, garis bawah (maks. 40 karakter). Contoh: 30400-K2VM-NC1", "Tambah Bin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return false;
			}
			foreach (object item in comboBox1.Items)
			{
				string text = item.ToString();
				if (text.EndsWith(". " + name) || text.EndsWith(" " + name))
				{
					MessageBox.Show("Tipe " + name + " sudah ada di daftar.", "Tambah Bin", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return false;
				}
			}
			string summary = "Template terdekat: " + templateName + "\r\n\r\n" + EcuLayoutTools.Describe(result) + "\r\nTambahkan sebagai tipe " + name + "?";
			if (MessageBox.Show(summary, "Konfirmasi Tambah Bin", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
			return false;
			}
			try
			{
				Directory.CreateDirectory(Path.Combine(CustomDir, "bins"));
				string binFile = Path.Combine("bins", Regex.Replace(name, "[^0-9A-Z_.-]", "_") + ".bin");
				File.WriteAllBytes(Path.Combine(CustomDir, binFile), data);
				CustomProfile p = new CustomProfile();
				p.Name = name;
				p.BinFile = binFile;
				p.Cols = cols;
				p.Rows = rows;
				p.Kon = kon;
				p.Lm = lm;
				p.O2Mask = o2m;
				p.O2cMask = o2cm;
				p.ImmoMask = immom;
				p.Template = templateName;
				foreach (KeyValuePair<string, int> kv in result.Addr)
				{
					p.Addr[kv.Key] = kv.Value;
				}
				if (result.BigEndian)
				{
					p.Addr["BE"] = 1;
				}
				AddCustomItem(p);
				SaveCustomIndex();
				comboBox1.SelectedItem = p.ItemText;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Gagal menyimpan profil: " + ex.Message, "Tambah Bin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			return true;
		}

		// Bin yang tata letaknya tidak dikenali otomatis: isi alamat tabel manual (dari XDF / A2L).
		private bool ManualLayout(byte[] data, out string templateName, out int cols, out int rows, out Dictionary<string, int> tpl, out float kon, out float lm, out int o2m, out int o2cm, out int immom, out LayoutResult result)
		{
			templateName = "Manual";
			cols = 25;
			rows = 29;
			tpl = null;
			kon = 1f;
			lm = 1f;
			o2m = 0;
			o2cm = 1;
			immom = 64;
			result = null;
			BuiltinEcu near = null;
			foreach (BuiltinEcu t in BuiltinEcuTable.All)
			{
				byte[] refBytes = null;
				try
				{
					refBytes = ReadBinSource(t.Resource);
				}
				catch (Exception)
				{
				}
				if (refBytes != null && refBytes.Length == data.Length && t.Addr.Length > 0)
				{
					near = t;
					break;
				}
			}
			string def = "cols=25;rows=29;kon=1;lm=1;be=0;tpsh=;rpmh=;feul1=;feul2=;ig1=;limit1=";
			if (near != null)
			{
				def = "cols=" + near.Cols + ";rows=" + near.Rows + ";kon=" + near.Kon.ToString(CultureInfo.InvariantCulture) + ";lm=" + near.Lm.ToString(CultureInfo.InvariantCulture) + ";be=0;" + near.Addr;
				o2m = near.O2Mask;
				o2cm = near.O2cMask;
				immom = near.ImmoMask;
			}
			string text = PromptText("Tambah Bin - alamat manual", "Tata letak tidak dikenali otomatis (" + data.Length.ToString("N0") + " byte). Isi alamat tabel (desimal atau 0x hex), pisahkan dengan ';'. be=1 untuk big-endian:", def);
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			Dictionary<string, int> addr = new Dictionary<string, int>();
			bool be = false;
			foreach (string pair in text.Split(';'))
			{
				string[] kv = pair.Split('=');
				if (kv.Length != 2)
				{
					continue;
				}
				string key = kv[0].Trim();
				string val = kv[1].Trim();
				if (val.Length == 0)
				{
					continue;
				}
				if (key == "kon" || key == "lm")
				{
					float f;
					if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out f) && f > 0f)
					{
						if (key == "kon")
						{
							kon = f;
						}
						else
						{
							lm = f;
						}
					}
					continue;
				}
				int v;
				bool parsed = val.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? int.TryParse(val.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v) : int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);
				if (!parsed)
				{
					MessageBox.Show("Nilai '" + pair.Trim() + "' tidak valid.", "Tambah Bin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}
				if (key == "cols")
				{
					cols = v;
				}
				else if (key == "rows")
				{
					rows = v;
				}
				else if (key == "be")
				{
					be = v == 1;
				}
				else if (Array.IndexOf(EcuLayoutTools.AllAddrFields, key) >= 0)
				{
					if (v < 0 || v >= data.Length)
					{
						MessageBox.Show("Alamat " + key + " di luar ukuran file.", "Tambah Bin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return false;
					}
					addr[key] = v;
				}
			}
			int tps;
			int rpm;
			int fuel;
			if (cols < 2 || rows < 2 || cols > 64 || rows > 64 || !addr.TryGetValue("tpsh", out tps) || !addr.TryGetValue("rpmh", out rpm) || !addr.TryGetValue("feul1", out fuel) || tps <= 0 || rpm <= 0 || fuel <= 0)
			{
				MessageBox.Show("Minimal isi cols, rows, tpsh, rpmh dan feul1.", "Tambah Bin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			tpl = new Dictionary<string, int>();
			foreach (string key in EcuLayoutTools.AllAddrFields)
			{
				int v;
				tpl[key] = addr.TryGetValue(key, out v) ? v : 0;
			}
			BinFormat format = new BinFormat { BigEndian = be, Kon = kon, Lm = lm };
			LayoutResult check = EcuLayoutTools.Evaluate(data, tpl, cols, rows, 0, 0.0, format);
			result = new LayoutResult();
			result.BigEndian = be;
			foreach (KeyValuePair<string, int> kv in tpl)
			{
				result.Addr[kv.Key] = kv.Value;
				if (kv.Value != 0)
				{
					if (check.Score >= 0 && check.Verified.Contains(kv.Key))
					{
						result.Verified.Add(kv.Key);
					}
					else
					{
						result.Unverified.Add(kv.Key);
					}
				}
			}
			if (check.Score < 0 && MessageBox.Show("Sumbu TPS/RPM di alamat tersebut tidak terlihat valid. Tetap tambahkan?", "Tambah Bin", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
			{
				return false;
			}
			result.Score = Math.Max(0, check.Score);
			return true;
		}

		private void RemoveBin_Click(object sender, EventArgs e)
		{
			if (customProfiles.Count == 0)
			{
				MessageBox.Show("Belum ada bin tambahan.", "Hapus Bin", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			using (Form form = new Form())
			{
				form.Text = "Hapus Bin Tambahan";
				form.StartPosition = FormStartPosition.CenterParent;
				form.FormBorderStyle = FormBorderStyle.FixedDialog;
				form.MaximizeBox = false;
				form.MinimizeBox = false;
				form.ClientSize = new Size(380, 280);
				ListBox list = new ListBox { Left = 12, Top = 12, Width = 356, Height = 220 };
				foreach (CustomProfile p in customProfiles.Values)
				{
					list.Items.Add(p.Name);
				}
				Button del = new Button { Text = "Hapus", Left = 196, Top = 242, Width = 84, DialogResult = DialogResult.OK };
				Button cancel = new Button { Text = "Tutup", Left = 284, Top = 242, Width = 84, DialogResult = DialogResult.Cancel };
				form.Controls.AddRange(new Control[] { list, del, cancel });
				form.AcceptButton = del;
				form.CancelButton = cancel;
				if (form.ShowDialog() != DialogResult.OK || list.SelectedItem == null)
				{
					return;
				}
				string name = list.SelectedItem.ToString();
				if (MessageBox.Show("Hapus tipe " + name + " dari daftar? File bin salinannya juga dihapus.", "Hapus Bin", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
				{
					return;
				}
				string key = null;
				foreach (KeyValuePair<string, CustomProfile> kv in customProfiles)
				{
					if (kv.Value.Name == name)
					{
						key = kv.Key;
					}
				}
				if (key == null)
				{
					return;
				}
				try
				{
					string file = Path.Combine(CustomDir, customProfiles[key].BinFile);
					if (File.Exists(file))
					{
						File.Delete(file);
					}
				}
				catch (Exception)
				{
				}
				if (comboBox1.SelectedItem != null && comboBox1.SelectedItem.ToString() == key)
				{
					comboBox1.SelectedIndex = 0;
				}
				customProfiles.Remove(key);
				comboBox1.Items.Remove(key);
				SaveCustomIndex();
			}
		}
	}
}
