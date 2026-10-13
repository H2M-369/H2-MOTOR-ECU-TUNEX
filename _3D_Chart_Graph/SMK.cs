using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SPECIALIST_PH_TUNER;
using ZXing;
using ZXing.Common;

namespace _3D_Chart_Graph;

public class SMK : Form
{
	private string receivedtype;

	private static IntPtr ftHandle = IntPtr.Zero;

	private byte[] binBytes;

	private static string ecmIdStr;

	private Thread eepromThread;

	private byte nilai11;

	private byte nilai12;

	private byte nilai112;

	private byte nilai122;

	private static bool endThread = false;

	private byte value1;

	private byte value2;

	private byte value3;

	private byte value4;

	private List<byte> collectedData2 = new List<byte>();

	private IContainer components = null;

	private Label label1;

	private Button button2;

	private ProgressBar progressBar1;

	private ComboBox comboBox1;

	private TextBox textBox1;

	private Button button1;

	private PictureBox pictureBoxBarcode;

	private TextBox textBox2;

	private TextBox textBox3;

	private TextBox textBox4;

	private Label label2;

	private Label label3;

	private Label PERSEN;

	private Label lblStatus;

	private Button button3;

	public SMK()
	{
		InitializeComponent();
		comboBox1.Items.Add("TYPE ECM");
		comboBox1.Items.Add("ADV 150 - 30400-K0WA-N01");
		comboBox1.Items.Add("ADV 160 - 30400-K0WL-NXX");
		comboBox1.Items.Add("BEAT 110 - 30400-K1AL-NB1");
		comboBox1.Items.Add("PCX 150 - 30400-K97G-NXX");
		comboBox1.Items.Add("PCX 160 - 30400-K1ZG-NXX");
		comboBox1.Items.Add("PCX 160 - 30400-K1ZN-NXX");
		comboBox1.Items.Add("STYLO 160 - 30400-K3VA-N01");
		comboBox1.Items.Add("SCOOPY 110 - 30400-K2F-NXX");
		comboBox1.Items.Add("VARIO 125 - 30400-K2VG-N42");
		comboBox1.Items.Add("VARIO 150 - 30400-K59J-A71");
		comboBox1.Items.Add("VARIO 160 - 30400-K2SA-N02");
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
		{ "PCX 160 - 30400-K1ZV-NA1", "PCX 160 - 30400-K1ZG-NXX" },
		{ "PCX 160 - 30400-K1ZV-NC1", "PCX 160 - 30400-K1ZG-NXX" },
		{ "PCX 160 - 30400-K1ZV-NF1", "PCX 160 - 30400-K1ZG-NXX" },
		{ "SCOOPY 110 - 30400-K2F-ND1", "SCOOPY 110 - 30400-K2F-NXX" },
		{ "VARIO 125 - 30400-K2VM-NA1", "VARIO 125 - 30400-K2VG-N42" },
		{ "VARIO 125 - 30400-K2VM-V51", "VARIO 125 - 30400-K2VG-N42" },
		{ "ADV 160 - 30400-K0WW-NC2", "ADV 160 - 30400-K0WL-NXX" },
		{ "ADV 160 - 30400-K0WW-ND2", "ADV 160 - 30400-K0WL-NXX" },
		{ "ADV 160 - 30400-K0WW-NE2", "ADV 160 - 30400-K0WL-NXX" },
		{ "KEYLESS 110 - 30400-K0J-NA1", "SCOOPY 110 - 30400-K2F-NXX" },
		{ "KEYLESS 110 - 30400-K0J-NB1", "SCOOPY 110 - 30400-K2F-NXX" },
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
		if (Class15.FT_Open(0u, ref ftHandle) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_Purge(ftHandle, 3u) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_SetBitMode(ftHandle, 0, 0) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_SetDataCharacteristics(ftHandle, 8, 0, 0) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_SetBaudRate(ftHandle, 10400u) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_SetTimeouts(ftHandle, 50u, 0u) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_SetLatencyTimer(ftHandle, 8) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_SetBitMode(ftHandle, 1, 1) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_Write(ftHandle, array, (uint)array.Length, ref uint_) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		Thread.Sleep(70);
		if (Class15.FT_Write(ftHandle, array2, (uint)array2.Length, ref uint_) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_SetBitMode(ftHandle, 0, 0) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_SetBaudRate(ftHandle, 10400u) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
			return false;
		}
		if (Class15.FT_Purge(ftHandle, 3u) > Class15.Enum5.const_0)
		{
			Class15.FT_Close(ftHandle);
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
		if (Class15.FT_Write(ftHandle, command, (uint)commandSize, ref uint_) > Class15.Enum5.const_0)
		{
			return false;
		}
		if (Class15.FT_SetLatencyTimer(ftHandle, 8) > Class15.Enum5.const_0)
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
			if (Class15.FT_GetQueueStatus(ftHandle, ref uint_2) == Class15.Enum5.const_0 && uint_2 != 0 && Class15.FT_Read(ftHandle, array, uint_2, ref uint_3) == Class15.Enum5.const_0 && uint_3 != 0)
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
		Class15.FT_Purge(ftHandle, 3u);
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
		lblStatus.Text = " Proses READ ID KEIHIN ,Silahkan tunggu sebentar . ";
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
			if (replySize < 13)
			{
				MessageBox.Show("Security Access Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
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
				PERSEN.Text = $"{num2} %";
				TimeSpan elapsed = stopwatch.Elapsed;
				label2.Text = $"{elapsed.Seconds} Detik";
			}));
		}
		stopwatch.Stop();
		Class15.FT_Close(ftHandle);
		Invoke((Action)(() =>
		{
			lblStatus.Text = " Proses READ ID KEIHIN Selesai ";
			label3.Text = $"Proses READ ID Key : Keihin {stopwatch.Elapsed.Seconds} Detik.";
		}));
		binBytes = list.ToArray();
		SaveFileOnUIThread1(list);
	}

	private void eeprom()
	{
		lblStatus.Text = " Proses READ ID Shindengen ,Silahkan tunggu sebentar... ";
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
			if (replySize < 15)
			{
				MessageBox.Show("Security Access Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
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
				PERSEN.Text = $"{num2} %";
				TimeSpan elapsed = stopwatch.Elapsed;
				label2.Text = $"{elapsed.Seconds} Detik";
			}));
		}
		stopwatch.Stop();
		Class15.FT_Close(ftHandle);
		Invoke((Action)(() =>
		{
			lblStatus.Text = " Proses READ ID SHINDENGEN Selesai ";
			label3.Text = $"Proses READ ID Key : Shindengen {stopwatch.Elapsed.Seconds} Detik.";
		}));
		binBytes = list.ToArray();
		SaveFileOnUIThread1(list);
	}

	private void SaveFileOnUIThread(List<byte> collectedData)
	{
		Invoke((Action)(() =>
		{
			string text = "C:\\Users\\S'COM\\Desktop\\File EEPROM READ ID";
			string path = "File EEPROM READ ID .bin";
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

	private void SaveFileOnUIThread1(List<byte> collectedData)
	{
		Invoke((Action)(() =>
		{
			string text = "C:\\File EEPROM READ ID";
			string arg = comboBox1.SelectedItem?.ToString() ?? "Default";
			string path = $"{arg}{DateTime.Now: dd-MM-yyyy}.bin";
			string text2 = Path.Combine(text, path);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			try
			{
				File.WriteAllBytes(text2, collectedData.ToArray());
				Console.WriteLine("Data berhasil disimpan ke: " + text2);
				progressBar1.Value = 0;
				baca();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Gagal menyimpan file: " + ex.Message);
			}
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
			else if (EffectiveType() == "VARIO 150 - 30400-K59J-A71")
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
			else if (EffectiveType() == "ADV 150 - 30400-K0WA-N01")
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
			else if (EffectiveType() == "PCX 150 - 30400-K97G-NXX")
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
			else if (EffectiveType() == "PCX 150 - 30400-K97G-N31")
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
			else if (EffectiveType() == "VARIO 125 - 30400-K2VG-N42")
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
			else if (IsK1ZVFamily())
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
			else if (EffectiveType() == "PCX 160 - 30400-K1ZG-NXX")
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
			else if (EffectiveType() == "PCX 160 - 30400-K1ZG-N21")
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
			else if (EffectiveType() == "PCX 160 - 30400-K1ZN-NXX")
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
			else if (EffectiveType() == "BEAT 110 - 30400-K1AL-NB1")
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
			else if (EffectiveType() == "STYLO 160 - 30400-K3VA-N01")
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
			else if (EffectiveType() == "VARIO 160 - 30400-K2SA-N02")
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
			else if (EffectiveType() == "ADV 160 - 30400-K0WL-NXX")
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
			else if (EffectiveType() == "SCOOPY 110 - 30400-K2F-NXX")
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

	private void ProcessPcxValueskh(byte[] firstValue, byte[] secondValue)
	{
		try
		{
			string text = BitConverter.ToString(firstValue).Replace("-", "");
			string text2 = BitConverter.ToString(secondValue).Replace("-", "");
			string text3 = text + text2;
			if (text3.Length != 8)
			{
				return;
			}
			if (text3 == "00000000")
			{
				ecmIdStr = "000000000";
				return;
			}
			string hex = text3.Substring(0, 4);
			string hex2 = SwapBytes(hex);
			string hex3 = RightShift(hex2, 2);
			string text4 = SubtractHex(hex3, "2000");
			string hex4 = text3.Substring(4, 4);
			string hex5 = XorHex(hex4, "80");
			string text5 = SwapBytes(hex5);
			string value = text4 + text5;
			ecmIdStr = Convert.ToInt64(value, 16).ToString().PadLeft(9, '0');
			textBox3.Text = ecmIdStr;
			textBox2.Text = ecmIdStr;
			string text6 = ecmIdStr;
			if (!string.IsNullOrEmpty(text6))
			{
				BarcodeWriter barcodeWriter = new BarcodeWriter
				{
					Format = BarcodeFormat.CODE_128,
					Options = new EncodingOptions
					{
						Height = 150,
						Width = 400
					}
				};
				Bitmap image = barcodeWriter.Write(text6);
				pictureBoxBarcode.Image = image;
			}
		}
		catch (Exception)
		{
		}
	}

	private string SwapBytes(string hex)
	{
		if (hex.Length != 4)
		{
			throw new System.FormatException("DATA ERROR");
		}
		return hex.Substring(2, 2) + hex.Substring(0, 2);
	}

	private string RightShift(string hex, int shift)
	{
		int num = Convert.ToInt32(hex, 16);
		return (num >> shift).ToString("X4");
	}

	private string SubtractHex(string hex1, string hex2)
	{
		int num = Convert.ToInt32(hex1, 16);
		int num2 = Convert.ToInt32(hex2, 16);
		return (num - num2).ToString("X4");
	}

	private string XorHex(string hex1, string hex2)
	{
		int num = Convert.ToInt32(hex1, 16);
		int num2 = Convert.ToInt32(hex2, 16);
		return (num ^ num2).ToString("X4");
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
				textBox3.Text = text5;
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
		string text = textBox3.Text.Trim();
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
			textBox2.Text = num9.ToString("D9");
			string text2 = num9.ToString();
			if (!string.IsNullOrEmpty(text2))
			{
				BarcodeWriter barcodeWriter = new BarcodeWriter
				{
					Format = BarcodeFormat.CODE_128,
					Options = new EncodingOptions
					{
						Height = 150,
						Width = 400
					}
				};
				Bitmap image = barcodeWriter.Write(text2);
				pictureBoxBarcode.Image = image;
			}
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
			textBox2.Text = "";
			for (int i = 0; i < array.Length; i += 4)
			{
				byte b = ConvertInputToByte(array[i]);
				byte b2 = ConvertInputToByte(array[i + 1]);
				byte b3 = ConvertInputToByte(array[i + 2]);
				byte b4 = ConvertInputToByte(array[i + 3]);
				uint num = (uint)((b4 << 8) | b3);
				uint num2 = (uint)((b2 << 24) | (b << 16)) | num;
				string text4 = num2.ToString().PadLeft(9, '0');
				textBox2.Text = text4 ?? "";
				value1 = (byte)((num2 >> 16) & 0xFF);
				value2 = (byte)((num2 >> 24) & 0xFF);
				value3 = (byte)(num2 & 0xFF);
				value4 = (byte)((num2 >> 8) & 0xFF);
				string text5 = $"{value3:X2} {value4:X2} {value1:X2} {value2:X2}";
				if (!string.IsNullOrEmpty(text4))
				{
					BarcodeWriter barcodeWriter = new BarcodeWriter
					{
						Format = BarcodeFormat.CODE_128,
						Options = new EncodingOptions
						{
							Height = 150,
							Width = 400
						}
					};
					Bitmap image = barcodeWriter.Write(text4);
					pictureBoxBarcode.Image = image;
				}
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

	private async void button1_Click(object sender, EventArgs e)
	{
		try
		{
			if (comboBox1.SelectedItem == null)
			{
				MessageBox.Show("Tidak Dapat Dijalankan. Silakan Pilih Type ECM terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else if (EffectiveType() == "VARIO 150 - 30400-K59J-A71")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
			else if (EffectiveType() == "ADV 150 - 30400-K0WA-N01")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
			else if (EffectiveType() == "PCX 150 - 30400-K97G-NXX")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
			else if (EffectiveType() == "VARIO 125 - 30400-K2VG-N42")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
			else if (EffectiveType() == "PCX 160 - 30400-K1ZG-NXX")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
			else if (EffectiveType() == "PCX 160 - 30400-K1ZN-NXX")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
			else if (EffectiveType() == "BEAT 110 - 30400-K1AL-NB1")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
			else if (EffectiveType() == "STYLO 160 - 30400-K3VA-N01")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
			else if (EffectiveType() == "VARIO 160 - 30400-K2SA-N02")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
			else if (EffectiveType() == "ADV 160 - 30400-K0WL-NXX")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
			else if (EffectiveType() == "READ EEPROM SH")
			{
				DialogResult result = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nKoneksi Ke EEPROM", "READ ECM ID", MessageBoxButtons.OK);
				Thread.Sleep(50);
				MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nProses READ ECM ID", "READ ECM ID", MessageBoxButtons.OK);
				if (result == DialogResult.OK)
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
			else if (EffectiveType() == "SCOOPY 110 - 30400-K2F-NXX")
			{
				lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
				await Task.Delay(2000);
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
		catch (Exception)
		{
		}
	}

	private async void button2_Click(object sender, EventArgs e)
	{
		lblStatus.Text = " OFF-ON Kontak Untuk Konek ke EEprom";
		await Task.Delay(3000);
		if (eepromThread == null || !eepromThread.IsAlive)
		{
			eepromThread = new Thread(eepromkeihin3)
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

	private void eepromkeihin3()
	{
		Invoke((Action)(() =>
		{
			lblStatus.Text = " PROSES RESET EEPROM KEIHIN ,Silahkan tunggu sebentar ";
		}));
		collectedData2.Clear();
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
			if (replySize < 13)
			{
				MessageBox.Show("Security Access Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
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
			Console.WriteLine(string.Join(" ", array9.Select((byte b3) => b3.ToString("X2"))));
			if (replySize > 12)
			{
				for (int num = 10; num < Math.Min(replySize, 12u); num++)
				{
					Console.Write(reply[num].ToString("X2") + " ");
					if (num == 10 || num == 11)
					{
						collectedData2.Add(reply[num]);
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
			}));
		}
		if (collectedData2.Count < 512)
		{
			Invoke((Action)(() =>
			{
				lblStatus.Text = " Reset EEPROM KEIHIN GAGAL: data EEPROM tidak lengkap (" + collectedData2.Count + "/512 byte). OFF-ON kontak lalu ulangi. ";
			}));
			return;
		}
		binBytes = collectedData2.ToArray();
		collectedData2[70] = 0;
		collectedData2[71] = 0;
		collectedData2[72] = 0;
		collectedData2[73] = 0;
		int count = collectedData2.Count;
		int y;
		for (y = 0; y < 256; y++)
		{
			int num2 = y * 2 % count;
			byte b = collectedData2[num2];
			byte b2 = collectedData2[(num2 + 1) % count];
			byte[] array10 = new byte[8] { 130, 130, 20, 8, 0, 0, 0, 0 };
			array10[4] = (byte)y;
			array10[5] = b;
			array10[6] = b2;
			array10[7] = (byte)(256 - (288 + (byte)y + b++) % 256);
			byte[] array11 = array10;
			SendCommand6(array11, array11.Length, ref reply, ref replySize);
			string text7 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine(string.Join(" ", array11.Select((byte b3) => b3.ToString("X2"))));
			Invoke((Action)(() =>
			{
				progressBar1.Value = y + 1;
			}));
			Console.WriteLine("Response cmd: " + text7);
		}
		Invoke((Action)(() =>
		{
			textBox2.Text = "000000000";
			lblStatus.Text = " Reset EEPROM KEIHIN SELESAI !!! ";
		}));
	}

	private async void button3_Click(object sender, EventArgs e)
	{
		lblStatus.Text = " OFF-ON KONTAK UNTUK KONEK KE EEPROM ... ";
		await Task.Delay(3000);
		if (eepromThread == null || !eepromThread.IsAlive)
		{
			eepromThread = new Thread(eeprom3)
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

	private void eeprom3()
	{
		lblStatus.Text = " PROSES RESET ID SHINDENGEN ,Silahkan tunggu sebentar ";
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
		byte[] reply = new byte[256];
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
			if (replySize < 15)
			{
				MessageBox.Show("Security Access Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
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
				Console.WriteLine("scurytyacces failed");
			}
			Invoke((Action)(() =>
			{
				progressBar1.Value = i + 1;
			}));
		}
		binBytes = list.ToArray();
		for (int num2 = 32; num2 <= 175; num2++)
		{
			binBytes[num2] = 0;
		}
		int num3 = binBytes.Length;
		int y;
		for (y = 0; y < 256; y++)
		{
			int num4 = y * 2 % num3;
			byte b = binBytes[num4];
			byte b2 = binBytes[(num4 + 1) % num3];
			byte[] array7 = new byte[9] { 145, 145, 9, 65, 0, 0, 0, 0, 0 };
			array7[4] = (byte)y;
			array7[6] = b;
			array7[7] = b2;
			array7[8] = (byte)(256 - (74 + y + b + b2));
			byte[] array8 = array7;
			SendCommand6(array8, array8.Length, ref reply, ref replySize);
			string text5 = BitConverter.ToString(reply, 0, (int)replySize);
			Console.WriteLine(string.Join(" ", array8.Select((byte b3) => b3.ToString("X2"))));
			Invoke((Action)(() =>
			{
				progressBar1.Value = y + 1;
			}));
			Console.WriteLine("Response cmd: " + text5);
		}
		Invoke((Action)(() =>
		{
			textBox2.Text = "000000000";
			lblStatus.Text = " PROSES RESET ID SHINDENGEN SELESAI !!! ";
		}));
	}

	private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (EffectiveType() == "VARIO 150 - 30400-K59J-A71")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
		else if (EffectiveType() == "TYPE ECM")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih type ECM terlebih dahulu";
			textBox2.Text = "000000000";
			textBox1.Text = "";
		}
		else if (EffectiveType() == "ADV 150 - 30400-K0WA-N01")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
		else if (EffectiveType() == "PCX 150 - 30400-K97G-NXX")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
		else if (EffectiveType() == "VARIO 125 - 30400-K2VG-N42")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
		else if (EffectiveType() == "PCX 160 - 30400-K1ZG-NXX")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
		else if (EffectiveType() == "PCX 160 - 30400-K1ZN-NXX")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
		else if (EffectiveType() == "BEAT 110 - 30400-K1AL-NB1")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
		else if (EffectiveType() == "STYLO 160 - 30400-K3VA-N01")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
		else if (EffectiveType() == "VARIO 160 - 30400-K2SA-N02")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
		else if (EffectiveType() == "ADV 160 - 30400-K0WL-NXX")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
		else if (EffectiveType() == "SCOOPY 110 - 30400-K2F-NXX")
		{
			lblStatus.Text = "\ud83d\udd0d Silahkan pilih Menu Read atau Reset";
		}
	}

	private void SMK_Load(object sender, EventArgs e)
	{
		textBox2.Text = "\ud83d\udd11 ---------";
		lblStatus.Text = "\ud83d\udd0d Silahkan pilih type ECM terlebih dahulu";
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
		this.label1 = new System.Windows.Forms.Label();
		this.button2 = new System.Windows.Forms.Button();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.button1 = new System.Windows.Forms.Button();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.textBox3 = new System.Windows.Forms.TextBox();
		this.textBox4 = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.PERSEN = new System.Windows.Forms.Label();
		this.lblStatus = new System.Windows.Forms.Label();
		this.button3 = new System.Windows.Forms.Button();
		this.pictureBoxBarcode = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.pictureBoxBarcode).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.Black;
		this.label1.Location = new System.Drawing.Point(178, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(195, 32);
		this.label1.TabIndex = 11;
		this.label1.Text = "\ud83d\udd11 ID Smartkey";
		this.button2.Location = new System.Drawing.Point(12, 89);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(131, 27);
		this.button2.TabIndex = 10;
		this.button2.Text = "RESET ID KEIHIN";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(this.button2_Click);
		this.progressBar1.Location = new System.Drawing.Point(12, 205);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(522, 10);
		this.progressBar1.TabIndex = 9;
		this.comboBox1.BackColor = System.Drawing.Color.Black;
		this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.comboBox1.ForeColor = System.Drawing.Color.White;
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Location = new System.Drawing.Point(12, 221);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(522, 25);
		this.comboBox1.TabIndex = 8;
		this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
		this.textBox1.BackColor = System.Drawing.Color.Black;
		this.textBox1.Font = new System.Drawing.Font("Segoe UI", 35.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.textBox1.ForeColor = System.Drawing.Color.White;
		this.textBox1.Location = new System.Drawing.Point(240, 418);
		this.textBox1.Multiline = true;
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(361, 10);
		this.textBox1.TabIndex = 7;
		this.textBox1.Text = "\ud83d\udd11 ---------";
		this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.button1.Location = new System.Drawing.Point(459, 87);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 27);
		this.button1.TabIndex = 6;
		this.button1.Text = "READ ID";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(this.button1_Click);
		this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 48f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.textBox2.Location = new System.Drawing.Point(12, 122);
		this.textBox2.Multiline = true;
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(522, 77);
		this.textBox2.TabIndex = 28;
		this.textBox2.Text = "\ud83d\udd11 ---------";
		this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.textBox3.Location = new System.Drawing.Point(143, 418);
		this.textBox3.Name = "textBox3";
		this.textBox3.Size = new System.Drawing.Size(100, 20);
		this.textBox3.TabIndex = 29;
		this.textBox4.Location = new System.Drawing.Point(134, 392);
		this.textBox4.Name = "textBox4";
		this.textBox4.Size = new System.Drawing.Size(100, 20);
		this.textBox4.TabIndex = 30;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(566, 378);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(35, 13);
		this.label2.TabIndex = 31;
		this.label2.Text = "label2";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(455, 392);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(35, 13);
		this.label3.TabIndex = 32;
		this.label3.Text = "label3";
		this.PERSEN.AutoSize = true;
		this.PERSEN.Location = new System.Drawing.Point(414, 399);
		this.PERSEN.Name = "PERSEN";
		this.PERSEN.Size = new System.Drawing.Size(35, 13);
		this.PERSEN.TabIndex = 33;
		this.PERSEN.Text = "label4";
		this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblStatus.Location = new System.Drawing.Point(15, 44);
		this.lblStatus.Name = "lblStatus";
		this.lblStatus.Size = new System.Drawing.Size(519, 36);
		this.lblStatus.TabIndex = 34;
		this.lblStatus.Text = "-";
		this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.button3.Location = new System.Drawing.Point(149, 89);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(176, 27);
		this.button3.TabIndex = 35;
		this.button3.Text = "RESET ID SHINDENGEN";
		this.button3.UseVisualStyleBackColor = true;
		this.button3.Click += new System.EventHandler(this.button3_Click);
		this.pictureBoxBarcode.BackColor = System.Drawing.Color.Transparent;
		this.pictureBoxBarcode.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pictureBoxBarcode.Location = new System.Drawing.Point(643, 358);
		this.pictureBoxBarcode.Name = "pictureBoxBarcode";
		this.pictureBoxBarcode.Size = new System.Drawing.Size(10, 80);
		this.pictureBoxBarcode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pictureBoxBarcode.TabIndex = 27;
		this.pictureBoxBarcode.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(548, 260);
		base.Controls.Add(this.button3);
		base.Controls.Add(this.lblStatus);
		base.Controls.Add(this.PERSEN);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.textBox4);
		base.Controls.Add(this.textBox3);
		base.Controls.Add(this.textBox2);
		base.Controls.Add(this.pictureBoxBarcode);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.progressBar1);
		base.Controls.Add(this.comboBox1);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.button1);
		base.Name = "SMK";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "SMK";
		base.Load += new System.EventHandler(this.SMK_Load);
		((System.ComponentModel.ISupportInitialize)this.pictureBoxBarcode).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
