using System;
using System.Collections.Generic;
using System.Text;

namespace _3D_Chart_Graph
{
	// Hasil analisis tata letak tabel sebuah bin terhadap tipe template.
	public class LayoutResult
	{
		public int Delta;
		public double Similarity;
		public int Score;
		public Dictionary<string, int> Addr = new Dictionary<string, int>();
		public List<string> Verified = new List<string>();
		public List<string> Unverified = new List<string>();
		public bool BigEndian;
	}

	// Format angka di dalam bin: urutan byte dan skala RPM/limiter (Shindengen memakai skala ~3.64).
	public class BinFormat
	{
		public bool BigEndian;
		public float Kon = 1f;
		public float Lm = 1f;
		public static readonly BinFormat Default = new BinFormat();
	}

	// Fungsi murni (tanpa UI) untuk mencari dan memvalidasi alamat tabel di dalam bin.
	// Cara baca sama dengan aplikasi: 16-bit little/big-endian sesuai template, tabel ignition 8-bit.
	public static class EcuLayoutTools
	{
		public static readonly string[] FuelFields = { "feul1", "feul2", "feul3" };
		public static readonly string[] IgnFields = { "ig1", "ig2", "ig3", "ig4" };
		public static readonly string[] LimitFields = { "limit1", "limit2", "limit3", "limit4", "limit5", "limit6", "limit7" };
		public static readonly string[] FlagFields = { "o2", "o2c", "IMMO", "MODE0", "MODE1", "MODE2", "MODE3", "MODE4" };
		public static readonly string[] GroupFields = { "iaph", "iap" };

		public static readonly string[] AllAddrFields = { "tpsh", "rpmh", "iaph", "iap", "feul1", "feul2", "feul3", "ig1", "ig2", "ig3", "ig4", "limit1", "limit2", "limit3", "limit4", "limit5", "limit6", "limit7", "o2", "o2c", "IMMO", "MODE0", "MODE1", "MODE2", "MODE3", "MODE4" };

		[ThreadStatic]
		private static BinFormat fmt;

		private static BinFormat F
		{
			get { return fmt ?? BinFormat.Default; }
		}

		private static int U16(byte[] b, int p)
		{
			return F.BigEndian ? ((b[p] << 8) | b[p + 1]) : (b[p] | (b[p + 1] << 8));
		}

		private static float Scale(float k)
		{
			return (k > 0.01f) ? k : 1f;
		}

		private static bool Fits(byte[] b, long addr, long len)
		{
			return addr >= 0 && addr + len <= b.Length;
		}

		public static double Similarity(byte[] a, byte[] b)
		{
			if (a == null || b == null || a.Length != b.Length || a.Length == 0)
			{
				return 0.0;
			}
			int same = 0;
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] == b[i])
				{
					same++;
				}
			}
			return (double)same / (double)a.Length;
		}

		private static bool Ascending(byte[] b, int start, int count)
		{
			if (!Fits(b, start, 2L * count) || count < 2)
			{
				return false;
			}
			int prev = U16(b, start);
			int first = prev;
			for (int i = 1; i < count; i++)
			{
				int v = U16(b, start + 2 * i);
				if (v < prev)
				{
					return false;
				}
				prev = v;
			}
			return prev > first;
		}

		public static bool AxesOk(byte[] b, int tps, int rpm, int cols, int rows)
		{
			if (!Ascending(b, tps, cols) || !Ascending(b, rpm, rows))
			{
				return false;
			}
			int rpmLast = U16(b, rpm + 2 * (rows - 1));
			int rpmFirst = U16(b, rpm);
			int tpsFirst = U16(b, tps);
			float k = Scale(F.Kon);
			return rpmLast / k >= 4000f && rpmLast / k <= 20000f && rpmFirst / k <= 3000f && tpsFirst <= 200;
		}

		public static bool Smooth16(byte[] b, int addr, int cols, int rows)
		{
			if (addr <= 0 || !Fits(b, addr, 2L * cols * rows))
			{
				return false;
			}
			int bad = 0;
			int total = 0;
			HashSet<int> distinct = new HashSet<int>();
			for (int r = 0; r < rows; r++)
			{
				for (int c = 0; c < cols; c++)
				{
					int v = U16(b, addr + 2 * (r * cols + c));
					distinct.Add(v);
					if (c > 0)
					{
						int prev = U16(b, addr + 2 * (r * cols + c - 1));
						total++;
						if (Math.Abs(v - prev) > Math.Max(400, (int)(0.25 * Math.Max(v, prev))))
						{
							bad++;
						}
					}
				}
			}
			return distinct.Count > 10 && (double)bad / (double)total < 0.08;
		}

		public static bool Smooth8(byte[] b, int addr, int cols, int rows)
		{
			if (addr <= 0 || !Fits(b, addr, (long)cols * rows))
			{
				return false;
			}
			int bad = 0;
			int total = 0;
			HashSet<int> distinct = new HashSet<int>();
			for (int r = 1; r < rows; r++)
			{
				for (int c = 0; c < cols; c++)
				{
					int v = b[addr + r * cols + c];
					distinct.Add(v);
					if (c > 0)
					{
						total++;
						if (Math.Abs(v - b[addr + r * cols + c - 1]) > 25)
						{
							bad++;
						}
					}
				}
			}
			return distinct.Count > 8 && (double)bad / (double)total < 0.08;
		}

		public static bool LimiterOk(byte[] b, int addr)
		{
			if (addr <= 0 || !Fits(b, addr, 4))
			{
				return false;
			}
			for (int i = 0; i < 2; i++)
			{
				int v = U16(b, addr + 2 * i);
				float k = Scale(F.Lm);
				float rpm = v / k;
				if (rpm < 3000f || rpm > 20000f || (Math.Abs(k - 1f) < 0.001f && v % 50 != 0))
				{
					return false;
				}
			}
			return true;
		}

		// Cari kandidat selisih alamat (delta) antara bin referensi (template) dan bin baru.
		public static List<int> FindDeltas(byte[] refB, byte[] newB, int tpsh, int rpmh, int cols, int rows)
		{
			List<int> deltas = new List<int>();
			deltas.Add(0);
			int len = 2 * cols;
			if (rpmh == tpsh + 2 * cols)
			{
				len = 2 * (cols + rows);
			}
			if (Fits(refB, tpsh, len))
			{
				for (int p = 0; p + len <= newB.Length; p++)
				{
					if (newB[p] != refB[tpsh])
					{
						continue;
					}
					bool match = true;
					for (int i = 1; i < len; i++)
					{
						if (newB[p + i] != refB[tpsh + i])
						{
							match = false;
							break;
						}
					}
					if (match && !deltas.Contains(p - tpsh))
					{
						deltas.Add(p - tpsh);
					}
				}
			}
			if (deltas.Count == 1)
			{
				int found = 0;
				for (int p = 0; p + 2 * (cols + rows) <= newB.Length && found < 200; p += 2)
				{
					if (AxesOk(newB, p, p + 2 * cols, cols, rows))
					{
						int d = p - tpsh;
						if (!deltas.Contains(d))
						{
							deltas.Add(d);
							found++;
						}
					}
				}
			}
			return deltas;
		}

		public static LayoutResult Evaluate(byte[] newB, Dictionary<string, int> tpl, int cols, int rows, int delta, double similarity, BinFormat format)
		{
			BinFormat old = fmt;
			fmt = format;
			try
			{
				LayoutResult r = Evaluate(newB, tpl, cols, rows, delta, similarity);
				r.BigEndian = F.BigEndian;
				return r;
			}
			finally
			{
				fmt = old;
			}
		}

		// Tentukan urutan byte template: coba little-endian, kalau sumbunya tidak valid coba big-endian.
		public static BinFormat DetectFormat(byte[] refB, Dictionary<string, int> tpl, int cols, int rows, float kon, float lm)
		{
			BinFormat le = new BinFormat { BigEndian = false, Kon = kon, Lm = lm };
			BinFormat be = new BinFormat { BigEndian = true, Kon = kon, Lm = lm };
			BinFormat old = fmt;
			try
			{
				fmt = le;
				if (AxesOk(refB, tpl["tpsh"], tpl["rpmh"], cols, rows))
				{
					return le;
				}
				fmt = be;
				if (AxesOk(refB, tpl["tpsh"], tpl["rpmh"], cols, rows))
				{
					return be;
				}
				return le;
			}
			finally
			{
				fmt = old;
			}
		}

		public static LayoutResult Evaluate(byte[] newB, Dictionary<string, int> tpl, int cols, int rows, int delta, double similarity)
		{
			LayoutResult res = new LayoutResult();
			res.BigEndian = F.BigEndian;
			res.Delta = delta;
			res.Similarity = similarity;
			int tps = tpl["tpsh"] + delta;
			int rpm = tpl["rpmh"] + delta;
			if (!AxesOk(newB, tps, rpm, cols, rows))
			{
				res.Score = -1;
				return res;
			}
			res.Addr["tpsh"] = tps;
			res.Addr["rpmh"] = rpm;
			res.Verified.Add("tpsh");
			res.Verified.Add("rpmh");
			res.Score = 5;
			bool fuelOk = true;
			foreach (string f in FuelFields)
			{
				int a;
				if (!tpl.TryGetValue(f, out a) || a == 0)
				{
					res.Addr[f] = 0;
					continue;
				}
				if (Smooth16(newB, a + delta, cols, rows))
				{
					res.Addr[f] = a + delta;
					res.Verified.Add(f);
					res.Score += 3;
				}
				else
				{
					res.Addr[f] = 0;
					res.Unverified.Add(f);
					if (f != "feul3")
					{
						fuelOk = false;
					}
				}
			}
			foreach (string f in IgnFields)
			{
				int a;
				if (!tpl.TryGetValue(f, out a) || a == 0)
				{
					res.Addr[f] = 0;
					continue;
				}
				if (Smooth8(newB, a + delta, cols, rows))
				{
					res.Addr[f] = a + delta;
					res.Verified.Add(f);
					res.Score += 2;
				}
				else
				{
					res.Addr[f] = 0;
					res.Unverified.Add(f);
				}
			}
			foreach (string f in LimitFields)
			{
				int a;
				if (!tpl.TryGetValue(f, out a) || a == 0)
				{
					res.Addr[f] = 0;
					continue;
				}
				if (LimiterOk(newB, a + delta))
				{
					res.Addr[f] = a + delta;
					res.Verified.Add(f);
					res.Score += 1;
				}
				else
				{
					res.Addr[f] = 0;
					res.Unverified.Add(f);
				}
			}
			foreach (string f in GroupFields)
			{
				int a;
				if (!tpl.TryGetValue(f, out a) || a == 0)
				{
					res.Addr[f] = 0;
					continue;
				}
				if (fuelOk)
				{
					res.Addr[f] = a + delta;
					res.Verified.Add(f);
				}
				else
				{
					res.Addr[f] = 0;
					res.Unverified.Add(f);
				}
			}
			bool sameFirmware = delta == 0 && similarity >= 0.99;
			foreach (string f in FlagFields)
			{
				int a;
				if (!tpl.TryGetValue(f, out a) || a == 0)
				{
					res.Addr[f] = 0;
					continue;
				}
				if (sameFirmware)
				{
					res.Addr[f] = a;
					res.Verified.Add(f);
				}
				else
				{
					res.Addr[f] = 0;
					res.Unverified.Add(f);
				}
			}
			return res;
		}

		// Pilih tata letak terbaik. Kembalikan null kalau sumbu atau tabel fuel utama tidak valid.
		public static LayoutResult Detect(byte[] refB, byte[] newB, Dictionary<string, int> tpl, int cols, int rows)
		{
			return Detect(refB, newB, tpl, cols, rows, 1f, 1f);
		}

		public static LayoutResult Detect(byte[] refB, byte[] newB, Dictionary<string, int> tpl, int cols, int rows, float kon, float lm)
		{
			BinFormat old = fmt;
			fmt = DetectFormat(refB, tpl, cols, rows, kon, lm);
			try
			{
				return DetectCore(refB, newB, tpl, cols, rows);
			}
			finally
			{
				fmt = old;
			}
		}

		private static LayoutResult DetectCore(byte[] refB, byte[] newB, Dictionary<string, int> tpl, int cols, int rows)
		{
			double sim = Similarity(refB, newB);
			LayoutResult best = null;
			foreach (int d in FindDeltas(refB, newB, tpl["tpsh"], tpl["rpmh"], cols, rows))
			{
				LayoutResult r = Evaluate(newB, tpl, cols, rows, d, sim);
				if (r.Score < 0 || !r.Verified.Contains("feul1"))
				{
					continue;
				}
				if (best == null || r.Score > best.Score)
				{
					best = r;
				}
			}
			return best;
		}

		public static string Describe(LayoutResult r)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Selisih alamat dari template: " + r.Delta + " byte");
			sb.AppendLine("Kemiripan isi dengan bin template: " + (r.Similarity * 100.0).ToString("F0") + "%");
			if (r.BigEndian)
			{
				sb.AppendLine("Format angka: big-endian");
			}
			sb.AppendLine("Tabel valid: " + string.Join(", ", r.Verified.ToArray()));
			if (r.Unverified.Count > 0)
			{
				sb.AppendLine("Tidak terverifikasi (menunya disembunyikan): " + string.Join(", ", r.Unverified.ToArray()));
			}
			return sb.ToString();
		}
	}
}
