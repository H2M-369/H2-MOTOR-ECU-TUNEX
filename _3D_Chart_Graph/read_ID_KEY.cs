using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace _3D_Chart_Graph;

public class read_ID_KEY : Form
{
	private string teks = "⟶ ( \ud83d\udd11 Smartkey ) ⟵  \ud83d\udcbb Reset ID Smartkey  -  Read ID Smartkey  ";

	private byte[] binBytes;

	private static IntPtr ftHandle = IntPtr.Zero;

	private Thread eepromThread;

	private static bool endThread = false;

	private byte value1;

	private byte value2;

	private byte value3;

	private byte value4;

	private IContainer components = null;

	private Button button1;

	private TextBox textBox1;

	private ComboBox comboBox1;

	private ProgressBar progressBar1;

	private Button button2;

	private System.Windows.Forms.Timer timer1;

	private Label label1;

	private TextBox textBox2;

	public read_ID_KEY()
	{
		InitializeComponent();
		timer1.Interval = 500;
		timer1.Enabled = true;
		comboBox1.Items.Add("TYPE ECM");
		comboBox1.Items.Add("ADV 150 - 30400-K0WA");
		comboBox1.Items.Add("ADV 160 - 30400-K0WL");
		comboBox1.Items.Add("BEAT 110 - 30400-K1AL");
		comboBox1.Items.Add("PCX 150 - 30400-K97G");
		comboBox1.Items.Add("PCX 160 - 30400-K1ZG");
		comboBox1.Items.Add("PCX 160 - 30400-K1ZN");
		comboBox1.Items.Add("PCX 160 - 30400-K1ZV");
		comboBox1.Items.Add("STYLO 160 - 30400-K3VA");
		comboBox1.Items.Add("SCOOPY 110 - 30400-K2F");
		comboBox1.Items.Add("VARIO 125 - 30400-K2VG");
		comboBox1.Items.Add("VARIO 150 - 30400-K59J");
		comboBox1.Items.Add("VARIO 160 - 30400-K2SA");
		comboBox1.Items.Add("PCX 160 - 30400-K1ZV-NA1");
		comboBox1.Items.Add("PCX 160 - 30400-K1ZV-NC1");
		comboBox1.Items.Add("PCX 160 - 30400-K1ZV-NF1");
		comboBox1.Items.Add("SCOOPY 110 - 30400-K2F-ND1");
		comboBox1.Items.Add("VARIO 125 - 30400-K2VM-NA1");
		comboBox1.Items.Add("VARIO 125 - 30400-K2VM-V51");
		comboBox1.Items.Add("ADV 160 - 30400-K0WW-NC2");
		comboBox1.Items.Add("ADV 160 - 30400-K0WW-ND2");
		comboBox1.Items.Add("ADV 160 - 30400-K0WW-NE2");
		comboBox1.Items.Add("KEYLESS 110 - 30400-K0J-NA1");
		comboBox1.Items.Add("KEYLESS 110 - 30400-K0J-NB1");
		if (comboBox1.Items.Count > 0)
		{
			comboBox1.SelectedIndex = 0;
		}
		AutoSelectEcmFromBin();
	}

	// Peta tipe ECM baru (motor keyless) ke tipe lama yang metode baca ID-nya sama.
	// Ubah satu baris di sini kalau ternyata offset EEPROM suatu tipe berbeda.
	private static readonly Dictionary<string, string> EcmAlias = new Dictionary<string, string>
	{
		{ "PCX 160 - 30400-K1ZV-NA1", "PCX 160 - 30400-K1ZV" },
		{ "PCX 160 - 30400-K1ZV-NC1", "PCX 160 - 30400-K1ZV" },
		{ "PCX 160 - 30400-K1ZV-NF1", "PCX 160 - 30400-K1ZV" },
		{ "SCOOPY 110 - 30400-K2F-ND1", "SCOOPY 110 - 30400-K2F" },
		{ "VARIO 125 - 30400-K2VM-NA1", "VARIO 125 - 30400-K2VG" },
		{ "VARIO 125 - 30400-K2VM-V51", "VARIO 125 - 30400-K2VG" },
		{ "ADV 160 - 30400-K0WW-NC2", "ADV 160 - 30400-K0WL" },
		{ "ADV 160 - 30400-K0WW-ND2", "ADV 160 - 30400-K0WL" },
		{ "ADV 160 - 30400-K0WW-NE2", "ADV 160 - 30400-K0WL" },
		{ "KEYLESS 110 - 30400-K0J-NA1", "SCOOPY 110 - 30400-K2F" },
		{ "KEYLESS 110 - 30400-K0J-NB1", "SCOOPY 110 - 30400-K2F" },
	};

	// Tipe yang dipetakan berdasarkan kemiripan keluarga ECU dan belum diverifikasi dengan dump EEPROM aslinya.
	private static readonly string[] UnverifiedEcm = new string[] { "K2VM", "K0WW", "K0J" };

	private string EffectiveType()
	{
		string selected = (comboBox1.SelectedItem != null) ? comboBox1.SelectedItem.ToString() : "";
		string baseType;
		return EcmAlias.TryGetValue(selected, out baseType) ? baseType : selected;
	}

	private bool IsK1ZVFamily()
	{
		string selected = (comboBox1.SelectedItem != null) ? comboBox1.SelectedItem.ToString() : "";
		return selected.Contains("-K1ZV");
	}

	private bool IsUnverifiedEcm()
	{
		string selected = (comboBox1.SelectedItem != null) ? comboBox1.SelectedItem.ToString() : "";
		foreach (string code in UnverifiedEcm)
		{
			if (selected.Contains("-" + code))
			{
				return true;
			}
		}
		return false;
	}

	private bool ConfirmUnverifiedEcm(string action)
	{
		if (!IsUnverifiedEcm())
		{
			return true;
		}
		string message = "Tipe ECM ini (" + comboBox1.SelectedItem + ") dipetakan berdasarkan kemiripan keluarga ECU dan BELUM diverifikasi dengan dump EEPROM aslinya.\r\n\r\nLakukan READ dulu dan cocokkan hasilnya dengan ID yang sudah Anda ketahui sebelum " + action + ".\r\n\r\nTetap lanjutkan " + action + "?";
		return MessageBox.Show(message, "Peringatan: Tipe Belum Terverifikasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
	}

	private void AutoSelectEcmFromBin()
	{
		// Pilih TYPE ECM otomatis dari bin yang dibuka di Form utama
		string part = _3D_Chart_Graph.Form1.BinPartCode ?? "";
		System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(part, "K[0-9A-Z]{1,4}(-[0-9A-Z]{3})?");
		if (!m.Success)
		{
			return;
		}
		string full = m.Value;
		string baseCode = full.Split('-')[0];
		int pick = -1;
		for (int i = 1; i < comboBox1.Items.Count; i++)
		{
			string item = comboBox1.Items[i].ToString();
			if (item.Contains("-" + full))
			{
				pick = i;
				break;
			}
		}
		if (pick < 0)
		{
			for (int i = 1; i < comboBox1.Items.Count; i++)
			{
				string item = comboBox1.Items[i].ToString();
				if (item.EndsWith("-" + baseCode) || item.Contains("-" + baseCode + "-"))
				{
					pick = i;
					break;
				}
			}
		}
		if (pick > 0)
		{
			comboBox1.SelectedIndex = pick;
		}
		Text = "Type ECU: " + _3D_Chart_Graph.Form1.BinEcuType + "  |  " + part;
	}

	private bool InitFtdiIso()
	{
		byte[] array = new byte[1];
		byte[] array2 = new byte[1] { 1 };
		uint uint_ = 0u;
		if (FTDI.FT_Open(0u, ref ftHandle) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_Purge(ftHandle, 3u) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetBitMode(ftHandle, 0, 0) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetDataCharacteristics(ftHandle, 8, 0, 0) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetBaudRate(ftHandle, 10400u) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetTimeouts(ftHandle, 50u, 0u) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetLatencyTimer(ftHandle, 8) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetBitMode(ftHandle, 1, 1) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_Write(ftHandle, array, (uint)array.Length, ref uint_) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		Thread.Sleep(70);
		if (FTDI.FT_Write(ftHandle, array2, (uint)array2.Length, ref uint_) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetBitMode(ftHandle, 0, 0) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetBaudRate(ftHandle, 10400u) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_Purge(ftHandle, 3u) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		Thread.Sleep(130);
		return true;
	}

	private bool SendCommand6(byte[] command, int commandSize, ref byte[] reply, ref uint replySize, int sleepMs = 0)
	{
		byte[] array = new byte[256];
		List<byte> list = new List<byte>();
		uint uint_ = 0u;
		uint uint_2 = 0u;
		uint uint_3 = 0u;
		long num = 50 + 2 * commandSize;
		if (FTDI.FT_Write(ftHandle, command, (uint)commandSize, ref uint_) > FTDI.FT_STATUS.FT_OK)
		{
			return false;
		}
		if (FTDI.FT_SetLatencyTimer(ftHandle, 8) > FTDI.FT_STATUS.FT_OK)
		{
			return false;
		}
		if (sleepMs > 0)
		{
			Thread.Sleep(sleepMs);
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		do
		{
			if (FTDI.FT_GetQueueStatus(ftHandle, ref uint_2) == FTDI.FT_STATUS.FT_OK && uint_2 != 0 && FTDI.FT_Read(ftHandle, array, uint_2, ref uint_3) == FTDI.FT_STATUS.FT_OK && uint_3 != 0)
			{
				list.AddRange(array.Take((int)uint_3));
				if (uint_2 == 0)
				{
					break;
				}
			}
		}
		while (stopwatch.ElapsedMilliseconds < num);
		stopwatch.Stop();
		FTDI.FT_Purge(ftHandle, 3u);
		if (list.Count > 0)
		{
			replySize = (uint)list.Count;
			reply = list.ToArray();
			return true;
		}
		return false;
	}

	private void eeprom1()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		byte[] array = new byte[4] { 254, 4, 114, 140 };
		byte[] array2 = new byte[5] { 114, 5, 0, 240, 153 };
		byte[] array3 = new byte[11]
		{
			39, 11, 224, 72, 101, 108, 108, 111, 72, 111,
			67
		};
		byte[] array4 = new byte[11]
		{
			39, 11, 224, 119, 65, 114, 101, 89, 111, 117,
			34
		};
		byte[] reply = new byte[256];
		byte[] array5 = new byte[6] { 130, 130, 16, 6, 0, 230 };
		byte[] array6 = new byte[6] { 130, 130, 16, 6, 1, 229 };
		byte[] array7 = new byte[6] { 130, 130, 16, 6, 1, 229 };
		uint replySize = 0u;
		List<byte> list = new List<byte>();
		if (!InitFtdiIso())
		{
			return;
		}
		Invoke((Action)(() =>
		{
			progressBar1.Maximum = 256;
			progressBar1.Value = 0;
		}));
		if (SendCommand6(array, array.Length, ref reply, ref replySize))
		{
			string text = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response: " + text);
		}
		Thread.Sleep(150);
		if (SendCommand6(array2, array2.Length, ref reply, ref replySize))
		{
			string text2 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response: " + text2);
		}
		Thread.Sleep(150);
		if (SendCommand6(array3, array3.Length, ref reply, ref replySize))
		{
			string text3 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response: " + text3);
		}
		Thread.Sleep(150);
		if (SendCommand6(array4, array4.Length, ref reply, ref replySize))
		{
			string text4 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response2: " + text4);
		}
		if (SendCommand6(array5, array5.Length, ref reply, ref replySize))
		{
			string text5 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response7: " + text5);
		}
		if (SendCommand6(array6, array6.Length, ref reply, ref replySize))
		{
			string text6 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response8: " + text6);
		}
		Console.WriteLine("Response Data:");
		int i;
		for (i = 0; i <= 255; i++)
		{
			byte[] array8 = new byte[6] { 130, 130, 16, 6, 0, 0 };
			array8[4] = (byte)i;
			array8[5] = (byte)(256 - (282 + i) % 256);
			byte[] array9 = array8;
			SendCommand6(array9, array9.Length, ref reply, ref replySize);
			if (replySize > 12)
			{
				for (int num = 10; num < Math.Min(replySize, 12u); num++)
				{
					Console.Write(reply[num].ToString("X2") + " ");
					if (num == 10 || num == 11)
					{
						list.Add(reply[num]);
					}
				}
				Console.WriteLine();
			}
			else
			{
				Console.WriteLine("Data kosong.");
			}
			Invoke((Action)(() =>
			{
				progressBar1.Value = i + 1;
				int num2 = (i + 1) * 100 / 256;
				TimeSpan elapsed = stopwatch.Elapsed;
			}));
		}
		stopwatch.Stop();
		Invoke((Action)(() =>
		{
		}));
		binBytes = list.ToArray();
		SaveFileOnUIThread(list);
	}

	private void eeprom()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		byte[] array = new byte[4] { 254, 4, 114, 140 };
		byte[] array2 = new byte[5] { 114, 5, 0, 240, 153 };
		byte[] array3 = new byte[13]
		{
			145, 145, 13, 223, 158, 141, 154, 134, 144, 138,
			140, 155, 136
		};
		byte[] array4 = new byte[13]
		{
			145, 145, 13, 223, 146, 158, 134, 150, 139, 141,
			134, 192, 106
		};
		byte[] reply = new byte[20];
		uint replySize = 0u;
		List<byte> list = new List<byte>();
		if (!InitFtdiIso())
		{
			return;
		}
		Invoke((Action)(() =>
		{
			progressBar1.Maximum = 256;
			progressBar1.Value = 0;
		}));
		if (SendCommand6(array, array.Length, ref reply, ref replySize))
		{
			string text = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response: " + text);
		}
		Thread.Sleep(150);
		if (SendCommand6(array2, array2.Length, ref reply, ref replySize))
		{
			string text2 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response: " + text2);
		}
		Thread.Sleep(150);
		if (SendCommand6(array3, array3.Length, ref reply, ref replySize))
		{
			string text3 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response: " + text3);
		}
		Thread.Sleep(150);
		if (SendCommand6(array4, array4.Length, ref reply, ref replySize))
		{
			string text4 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response2: " + text4);
		}
		Console.WriteLine("Response Data:");
		int i;
		for (i = 0; i <= 255; i++)
		{
			byte[] array5 = new byte[7] { 145, 145, 7, 64, 0, 0, 0 };
			array5[4] = (byte)i;
			array5[6] = (byte)(256 - (71 + i));
			byte[] array6 = array5;
			SendCommand6(array6, array6.Length, ref reply, ref replySize);
			Thread.Sleep(100);
			if (replySize > 12)
			{
				for (int num = 11; num < Math.Min(replySize, 13u); num++)
				{
					Console.Write(reply[num].ToString("X2") + " ");
					if (num == 11 || num == 12)
					{
						list.Add(reply[num]);
					}
				}
				Console.WriteLine();
			}
			else
			{
				Console.WriteLine("Data tidak cukup untuk menampilkan byte 11 dan 12.");
			}
			Invoke((Action)(() =>
			{
				progressBar1.Value = i + 1;
				int num2 = (i + 1) * 100 / 256;
				TimeSpan elapsed = stopwatch.Elapsed;
			}));
		}
		stopwatch.Stop();
		Invoke((Action)(() =>
		{
		}));
		binBytes = list.ToArray();
		SaveFileOnUIThread(list);
	}

	private void SaveFileOnUIThread(List<byte> collectedData)
	{
		Invoke((Action)(() =>
		{
			string text = "C:\\File EEPROM READ ID";
			string path = "File EEPROM READ ID " + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bin";
			string text2 = Path.Combine(text, path);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			File.WriteAllBytes(text2, collectedData.ToArray());
			Console.WriteLine("Data berhasil disimpan ke: " + text2);
			progressBar1.Value = 0;
			baca();
		}));
	}

	private void baca()
	{
		try
		{
			if (binBytes == null)
			{
				MessageBox.Show("Tidak ada data yang dikumpulkan. Silakan jalankan fungsi EEPROM terlebih dahulu.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else if (EffectiveType() == "VARIO 150 - 30400-K59J")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[144],
						binBytes[145]
					}, new byte[2]
					{
						binBytes[160],
						binBytes[161]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "ADV 150 - 30400-K0WA")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[64],
						binBytes[65]
					}, new byte[2]
					{
						binBytes[80],
						binBytes[81]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "PCX 150 - 30400-K97G")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[64],
						binBytes[65]
					}, new byte[2]
					{
						binBytes[80],
						binBytes[81]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZV")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[64],
						binBytes[65]
					}, new byte[2]
					{
						binBytes[80],
						binBytes[81]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "VARIO 125 - 30400-K2VG")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[60],
						binBytes[61]
					}, new byte[2]
					{
						binBytes[62],
						binBytes[63]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZG")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[64],
						binBytes[65]
					}, new byte[2]
					{
						binBytes[76],
						binBytes[77]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZG")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[64],
						binBytes[65]
					}, new byte[2]
					{
						binBytes[76],
						binBytes[77]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZN")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[64],
						binBytes[65]
					}, new byte[2]
					{
						binBytes[76],
						binBytes[77]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZN-N51")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[64],
						binBytes[65]
					}, new byte[2]
					{
						binBytes[76],
						binBytes[77]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "BEAT 110 - 30400-K1AL")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[60],
						binBytes[61]
					}, new byte[2]
					{
						binBytes[62],
						binBytes[63]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "STYLO 160 - 30400-K3VA")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[60],
						binBytes[61]
					}, new byte[2]
					{
						binBytes[62],
						binBytes[63]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "VARIO 160 - 30400-K2SA")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[60],
						binBytes[61]
					}, new byte[2]
					{
						binBytes[62],
						binBytes[63]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "ADV 160 - 30400-K0WL")
			{
				if (binBytes.Length >= 88)
				{
					ProcessPcxValues(new byte[2]
					{
						binBytes[64],
						binBytes[65]
					}, new byte[2]
					{
						binBytes[76],
						binBytes[77]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "ADV 160 - 30400-K0WL-NB1")
			{
				if (binBytes.Length >= 88)
				{
					byte[] firstValue = new byte[2]
					{
						binBytes[64],
						binBytes[65]
					};
					byte[] secondValue = new byte[2]
					{
						binBytes[76],
						binBytes[77]
					};
					ProcessPcxValues(firstValue, secondValue);
					ProcessPcxValues(firstValue, secondValue);
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "SCOOPY 110 - 30400-K2F")
			{
				if (binBytes.Length >= 88)
				{
					ProcessKEIHIN(new byte[2]
					{
						binBytes[70],
						binBytes[71]
					}, new byte[2]
					{
						binBytes[72],
						binBytes[73]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "SCOOPY 110 - 30400-K2F-N91")
			{
				if (binBytes.Length >= 88)
				{
					ProcessKEIHIN(new byte[2]
					{
						binBytes[70],
						binBytes[71]
					}, new byte[2]
					{
						binBytes[72],
						binBytes[73]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (EffectiveType() == "SCOOPY 110 - 30400-K2F-ND1")
			{
				if (binBytes.Length >= 88)
				{
					ProcessKEIHIN(new byte[2]
					{
						binBytes[70],
						binBytes[71]
					}, new byte[2]
					{
						binBytes[72],
						binBytes[73]
					});
				}
				else
				{
					MessageBox.Show("Gagal Read Data\r\nRestart your ECM\r\nUlangi Dari Awal", "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Terjadi kesalahan saat memproses data: " + ex.Message, "READ ECM ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void ProcessKEIHIN(byte[] firstValue, byte[] secondValue)
	{
		string text = BitConverter.ToString(firstValue).Replace("-", " ");
		string text2 = BitConverter.ToString(secondValue).Replace("-", " ");
		try
		{
			string text3 = text2 + " " + text;
			string[] array = text3.Split(new char[4] { ' ', ',', '-', ':' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length % 4 != 0)
			{
				MessageBox.Show("Jumlah byte harus kelipatan 4.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			for (int i = 0; i < array.Length; i += 4)
			{
				byte b = ConvertInputToByte(array[i]);
				byte b2 = ConvertInputToByte(array[i + 1]);
				byte b3 = ConvertInputToByte(array[i + 2]);
				byte b4 = ConvertInputToByte(array[i + 3]);
				uint num = (uint)((b4 << 8) | b3);
				uint num2 = (uint)((b << 16) | (b2 << 24)) | num;
				string text4 = num2.ToString().PadLeft(9, '0');
				value1 = (byte)((num2 >> 16) & 0xFF);
				value2 = (byte)((num2 >> 24) & 0xFF);
				value3 = (byte)(num2 & 0xFF);
				value4 = (byte)((num2 >> 8) & 0xFF);
				string text5 = $"{value3:X2} {value4:X2} {value1:X2} {value2:X2}";
				textBox2.Text = text5;
				textBox3_TextChanged();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void textBox3_TextChanged()
	{
		string text = textBox2.Text.Trim();
		string[] array = text.Split(' ');
		if (array.Length < 4)
		{
			MessageBox.Show("Input harus terdiri dari 4 byte hexadecimal, dipisahkan oleh spasi.");
			return;
		}
		try
		{
			int num = Convert.ToInt32(array[0], 16);
			int num2 = Convert.ToInt32(array[1], 16);
			int num3 = Convert.ToInt32(array[2], 16);
			int num4 = Convert.ToInt32(array[3], 16);
			long num5 = (num2 << 8) | num;
			long num6 = (num4 << 8) | num3;
			long num7 = (num5 * 65536 >> 2) - 536870912;
			long num8 = num6 ^ 0x8000;
			long num9 = num7 + num8;
			if (num9 < 0)
			{
				num9 = 0L;
			}
			textBox1.Text = "\ud83d\udd11 " + num9.ToString("D9");
		}
		catch (Exception ex)
		{
			MessageBox.Show("Error in conversion: " + ex.Message);
		}
	}

	private void ProcessPcxValues(byte[] firstValue, byte[] secondValue)
	{
		string text = BitConverter.ToString(firstValue).Replace("-", " ");
		string text2 = BitConverter.ToString(secondValue).Replace("-", " ");
		try
		{
			string text3 = text2 + " " + text;
			string[] array = text3.Split(new char[4] { ' ', ',', '-', ':' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length % 4 != 0)
			{
				MessageBox.Show("Jumlah byte harus kelipatan 4.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			textBox1.Text = "";
			for (int i = 0; i < array.Length; i += 4)
			{
				byte b = ConvertInputToByte(array[i]);
				byte b2 = ConvertInputToByte(array[i + 1]);
				byte b3 = ConvertInputToByte(array[i + 2]);
				byte b4 = ConvertInputToByte(array[i + 3]);
				uint num = (uint)((b4 << 8) | b3);
				uint num2 = (uint)((b2 << 24) | (b << 16)) | num;
				string text4 = num2.ToString().PadLeft(9, '0');
				textBox1.Text = "\ud83d\udd11 {123456789}";
				value1 = (byte)((num2 >> 16) & 0xFF);
				value2 = (byte)((num2 >> 24) & 0xFF);
				value3 = (byte)(num2 & 0xFF);
				value4 = (byte)((num2 >> 8) & 0xFF);
				string text5 = $"{value3:X2} {value4:X2} {value1:X2} {value2:X2}";
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private byte ConvertInputToByte(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			throw new ArgumentException("Input tidak boleh kosong.");
		}
		return Convert.ToByte(input, 16);
	}

	private void button1_Click(object sender, EventArgs e)
	{
		try
		{
			if (comboBox1.SelectedItem == null)
			{
				MessageBox.Show("Tidak Dapat Dijalankan. Silakan Pilih Type ECM terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else if (EffectiveType() == "VARIO 150 - 30400-K59J")
			{
				DialogResult dialogResult = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "ADV 150 - 30400-K0WA")
			{
				DialogResult dialogResult2 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult2 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "PCX 150 - 30400-K97G")
			{
				DialogResult dialogResult3 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult3 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "VARIO 125 - 30400-K2VG")
			{
				DialogResult dialogResult4 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult4 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZG")
			{
				DialogResult dialogResult5 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult5 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZN")
			{
				DialogResult dialogResult6 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult6 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZV")
			{
				DialogResult dialogResult7 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult7 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "BEAT 110 - 30400-K1AL")
			{
				DialogResult dialogResult8 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult8 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "STYLO 160 - 30400-K3VA")
			{
				DialogResult dialogResult9 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult9 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "VARIO 160 - 30400-K2SA")
			{
				DialogResult dialogResult10 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult10 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "ADV 160 - 30400-K0WL")
			{
				DialogResult dialogResult11 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult11 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "READ EEPROM SH")
			{
				DialogResult dialogResult12 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				Thread.Sleep(50);
				DialogResult dialogResult13 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nProses READ ECM ID", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult12 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else
			{
				if (!(EffectiveType() == "SCOOPY 110 - 30400-K2F"))
				{
					return;
				}
				DialogResult dialogResult14 = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				if (dialogResult14 == DialogResult.OK)
				{
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(eeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private void button2_Click(object sender, EventArgs e)
	{
		if (!ConfirmUnverifiedEcm("RESET ID"))
		{
			return;
		}
		try
		{
			if (comboBox1.SelectedItem == null)
			{
				MessageBox.Show("Tidak Dapat Dijalankan. Silakan Pilih Type ECM terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else if (EffectiveType() == "VARIO 150 - 30400-K59J")
			{
				DialogResult dialogResult = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "ADV 150 - 30400-K0WA")
			{
				DialogResult dialogResult2 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult2 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "PCX 150 - 30400-K97G")
			{
				DialogResult dialogResult3 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult3 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "VARIO 125 - 30400-K2VG")
			{
				DialogResult dialogResult4 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult4 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZG")
			{
				DialogResult dialogResult5 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult5 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZN")
			{
				DialogResult dialogResult6 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult6 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "PCX 160 - 30400-K1ZV")
			{
				DialogResult dialogResult7 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult7 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "BEAT 110 - 30400-K1AL")
			{
				DialogResult dialogResult8 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult8 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "STYLO 160 - 30400-K3VA")
			{
				DialogResult dialogResult9 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult9 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "VARIO 160 - 30400-K2SA")
			{
				DialogResult dialogResult10 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult10 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else if (EffectiveType() == "ADV 160 - 30400-K0WL")
			{
				DialogResult dialogResult11 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult11 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
			else
			{
				if (!(EffectiveType() == "SCOOPY 110 - 30400-K2F"))
				{
					return;
				}
				DialogResult dialogResult12 = MessageBox.Show("Klik 'OK' Untuk Reset ID Kembali Baru", "RENEW ID", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				if (dialogResult12 == DialogResult.OK)
				{
					EDIT_NUMBER();
					if (eepromThread == null || !eepromThread.IsAlive)
					{
						eepromThread = new Thread(writeeeprom1)
						{
							IsBackground = true
						};
						eepromThread.Start();
					}
					else
					{
						MessageBox.Show("Process is already running.");
					}
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private void EDIT_NUMBER()
	{
		string text = "000000000";
		uint num = Convert.ToUInt32(text.Trim(), 10);
		value1 = (byte)((num >> 16) & 0xFF);
		value2 = (byte)((num >> 24) & 0xFF);
		value3 = (byte)(num & 0xFF);
		value4 = (byte)((num >> 8) & 0xFF);
		string text2 = $"{value1:X2} {value2:X2} {value3:X2} {value4:X2}";
		textBox2.Text = text2;
	}

	private void writeeeprom1()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		byte[] array = new byte[4] { 254, 4, 114, 140 };
		byte[] array2 = new byte[5] { 114, 5, 0, 240, 153 };
		byte[] array3 = new byte[13]
		{
			145, 145, 13, 223, 158, 141, 154, 134, 144, 138,
			140, 155, 136
		};
		byte[] array4 = new byte[13]
		{
			145, 145, 13, 223, 146, 158, 134, 150, 139, 141,
			134, 192, 106
		};
		byte[] reply = new byte[20];
		uint replySize = 0u;
		if (!InitFtdiIso())
		{
			return;
		}
		Invoke((Action)(() =>
		{
			progressBar1.Maximum = 256;
			progressBar1.Value = 0;
		}));
		if (SendCommand6(array, array.Length, ref reply, ref replySize))
		{
			string text = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response: " + text);
		}
		Thread.Sleep(150);
		if (SendCommand6(array2, array2.Length, ref reply, ref replySize))
		{
			string text2 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response: " + text2);
		}
		Thread.Sleep(150);
		if (SendCommand6(array3, array3.Length, ref reply, ref replySize))
		{
			string text3 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response: " + text3);
		}
		Thread.Sleep(150);
		if (SendCommand6(array4, array4.Length, ref reply, ref replySize))
		{
			string text4 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine("Response2: " + text4);
		}
		int i;
		for (i = 0; i <= 255; i++)
		{
			byte[] array5 = new byte[9] { 145, 145, 9, 65, 0, 0, 0, 0, 0 };
			array5[4] = (byte)i;
			array5[6] = ((i % 2 != 0) ? value1 : value3);
			array5[7] = ((i % 2 != 0) ? value2 : value4);
			array5[8] = (byte)(256 - (74 + i + ((i % 2 != 0) ? value1 : value3) + ((i % 2 != 0) ? value2 : value4)));
			SendCommand6(array5, array5.Length, ref reply, ref replySize);
			Thread.Sleep(50);
			Invoke((Action)(() =>
			{
				progressBar1.Value = i + 1;
				int num = (i + 1) * 100 / 256;
				TimeSpan elapsed = stopwatch.Elapsed;
			}));
		}
		Invoke((Action)(() =>
		{
			MessageBox.Show("RENEW ID KEY SUKSES\r\nID KEY SUDAH DI RESET (000000000)\r\nSILAHKAN READ ULANG ID KEY UNTUK MEMASTIKAN", "RENEW ID");
		}));
	}

	private void read_ID_KEY_Load(object sender, EventArgs e)
	{
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		teks = teks.Substring(1) + teks[0];
		Text = teks;
	}

	private void read_ID_KEY_FormClosing(object sender, FormClosingEventArgs e)
	{
		endThread = true;
		FTDI.FT_Close(ftHandle);
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3D_Chart_Graph.read_ID_KEY));
		this.button1 = new System.Windows.Forms.Button();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.button2 = new System.Windows.Forms.Button();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.label1 = new System.Windows.Forms.Label();
		this.textBox2 = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.button1.Location = new System.Drawing.Point(299, 56);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 27);
		this.button1.TabIndex = 0;
		this.button1.Text = "READ ID";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(this.button1_Click);
		this.textBox1.BackColor = System.Drawing.Color.Black;
		this.textBox1.Font = new System.Drawing.Font("Segoe UI", 35.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.textBox1.ForeColor = System.Drawing.Color.White;
		this.textBox1.Location = new System.Drawing.Point(13, 89);
		this.textBox1.Multiline = true;
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(361, 72);
		this.textBox1.TabIndex = 1;
		this.textBox1.Text = "\ud83d\udd11 ---------";
		this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.comboBox1.BackColor = System.Drawing.Color.Black;
		this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.comboBox1.ForeColor = System.Drawing.Color.White;
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Location = new System.Drawing.Point(93, 58);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(200, 25);
		this.comboBox1.TabIndex = 2;
		this.progressBar1.Location = new System.Drawing.Point(13, 167);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(361, 10);
		this.progressBar1.TabIndex = 3;
		this.button2.Location = new System.Drawing.Point(12, 56);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 27);
		this.button2.TabIndex = 4;
		this.button2.Text = "RESET ID";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(this.button2_Click);
		this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.White;
		this.label1.Location = new System.Drawing.Point(98, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(195, 32);
		this.label1.TabIndex = 5;
		this.label1.Text = "\ud83d\udd11 ID Smartkey";
		this.textBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox2.BackColor = System.Drawing.Color.Black;
		this.textBox2.Font = new System.Drawing.Font("Segoe UI", 36f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.textBox2.ForeColor = System.Drawing.Color.White;
		this.textBox2.Location = new System.Drawing.Point(12, 356);
		this.textBox2.Multiline = true;
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(361, 72);
		this.textBox2.TabIndex = 6;
		this.textBox2.Text = "123456789";
		this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(559, 337);
		base.Controls.Add(this.textBox2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.progressBar1);
		base.Controls.Add(this.comboBox1);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.button1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "read_ID_KEY";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "READ ID KEY";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.read_ID_KEY_FormClosing);
		base.Load += new System.EventHandler(this.read_ID_KEY_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
