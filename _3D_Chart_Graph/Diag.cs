using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3D_Chart_Graph;

public class Diag : Form
{
	private string teks = "⟶ (\ud83d\udcbb Scan - \ud83d\udd0d Diag  ) ⟵  \ud83d\udcbb Scanner Pc  -  \ud83d\udd0d Diagnosa Kerusakan (DTC) ";

	private static IntPtr ftHandle = IntPtr.Zero;

	private bool tek10 = true;

	private bool tek11 = true;

	private bool tek13 = true;

	private bool tek16 = true;

	private bool tek17 = true;

	private CancellationTokenSource _cancellationTokenSource;

	private DataGridView dataGridView;

	private DataGridView dataGridView2;

	private string combinedResult = "";

	private Dictionary<string, string> errorMessages = new Dictionary<string, string>();

	private string Hasil = "-";

	private string Hasil2 = "-";

	private string Hasil3 = "-";

	private string Hasil4 = "-";

	private string hasilGabungan = "-";

	private int dtcCounter = 1;

	private bool dtcTitleAdded = false;

	private float o2 = 0f;

	private float oxi = 0f;

	private float o2v = 0f;

	private float stf = 0f;

	private float afr = 0f;

	private string fiz = "-";

	private string bas = "-";

	private string sss = "-";

	private string ssw = "-";

	private string ggs = "-";

	private string pom = "-";

	private string fan = "-";

	private string ggv = "-";

	private IContainer components = null;

	private Panel panel1;

	private Button button1;

	private Button button2;

	private Panel panel3;

	private Panel panel2;

	private Panel panel4;

	private ListBox listBox1;

	private Button button3;

	private Button button4;

	private Label label1;

	private System.Windows.Forms.Timer timer1;

	public Diag()
	{
		InitializeComponent();
		MinimizeBox = false;
		ControlBox = true;
		listBox1.Visible = false;
		listBox1.DrawMode = DrawMode.OwnerDrawFixed;
		listBox1.DrawItem += listBox1_DrawItem1;
		timer1.Interval = 500;
		timer1.Enabled = true;
	}

	private void Diag_Load(object sender, EventArgs e)
	{
		InitFtdiIso();
		InitializeDataGridView();
		InitializeDataGridView2();
		FormClosing += SecondForm_FormClosing;
	}

	private void InitializeDataGridView()
	{
		dataGridView = new DataGridView
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
			ReadOnly = true,
			Enabled = false,
			AllowUserToAddRows = false,
			AllowUserToDeleteRows = false,
			AllowUserToResizeColumns = false,
			AllowUserToResizeRows = false,
			SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			MultiSelect = false,
			RowHeadersVisible = true
		};
		dataGridView.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		dataGridView.ClearSelection();
		dataGridView.SelectionChanged += (object s, EventArgs e) =>
		{
			dataGridView.ClearSelection();
		};
		dataGridView.Columns[0].HeaderText = "Parameter";
		dataGridView.Columns[1].HeaderText = "Value";
		dataGridView.Rows.Add("Engine RPM");
		dataGridView.Rows.Add("Throttle Position Volt");
		dataGridView.Rows.Add("Throttle Position ");
		dataGridView.Rows.Add("Engine Temp Volt");
		dataGridView.Rows.Add("Engine Temp ");
		dataGridView.Rows.Add("Intake Air Temperartur Volt");
		dataGridView.Rows.Add("Intake Air Temperartur ");
		dataGridView.Rows.Add("Manifold Absolute Pressure Volt");
		dataGridView.Rows.Add("Manifold Absolute Pressure");
		dataGridView.Rows.Add("Voltage Battery");
		dataGridView.Rows.Add("Injection Duration");
		dataGridView.Rows.Add("MODE");
		dataGridView.Rows.Add("Ignition Timming");
		dataGridView.Rows.Add("Vehicle Speed Sensor");
		dataGridView.Rows.Add("TableType");
		dataGridView.Rows.Add("Handle grip position 1 Volt");
		dataGridView.Rows.Add("Handle grip position 2 Volt");
		dataGridView.Rows.Add("Handle grip position 1 °");
		dataGridView.Rows.Add("Handle grip position 2 °");
		dataGridView.Rows.Add("Throttle Position 1 Volt");
		dataGridView.Rows.Add("Throttle Position 2 Volt");
		dataGridView.Rows.Add("Throttle Position 1 °");
		dataGridView.Rows.Add("Throttle Position 2 °");
		dataGridView.Rows.Add("COR");
		dataGridView.Rows.Add("Intake Air Temperartur Volt");
		dataGridView.Rows.Add("Intake Air Temperartur ");
		dataGridView.Rows.Add("Manifold Absolute Pressure Volt");
		dataGridView.Rows.Add("Manifold Absolute Pressure");
		for (int num = 0; num < dataGridView.Rows.Count; num++)
		{
			dataGridView.Rows[num].HeaderCell.Value = (num + 1).ToString();
		}
		dataGridView.CellFormatting += (object s, DataGridViewCellFormattingEventArgs e) =>
		{
			e.CellStyle.BackColor = dataGridView.DefaultCellStyle.BackColor;
			e.CellStyle.SelectionBackColor = dataGridView.DefaultCellStyle.BackColor;
			e.CellStyle.SelectionForeColor = dataGridView.DefaultCellStyle.ForeColor;
		};
		panel2.Controls.Add(dataGridView);
	}

	private void dtc()
	{
		byte[] array = new byte[5] { 114, 5, 115, 2, 20 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		string text = "-";
		SendCommand(array, array.Length, ref reply, ref replySize);
		text = BitConverter.ToString(reply).Replace("-", "");
		byte b = reply[5];
		byte b2 = reply[6];
		byte b3 = reply[7];
		byte b4 = reply[8];
		byte b5 = reply[9];
		byte b6 = reply[10];
		Hasil = $"{b}-{b2}, {b3}-{b4}, {b5}-{b6}";
	}

	private void dtc2()
	{
		byte[] array = new byte[5] { 114, 5, 115, 1, 21 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		string text = "-";
		SendCommand(array, array.Length, ref reply, ref replySize);
		text = BitConverter.ToString(reply).Replace("-", "");
		byte b = reply[5];
		byte b2 = reply[6];
		byte b3 = reply[7];
		byte b4 = reply[8];
		byte b5 = reply[9];
		byte b6 = reply[10];
		Hasil2 = $"{b}-{b2}, {b3}-{b4}, {b5}-{b6}";
	}

	private void dtc3()
	{
		byte[] array = new byte[5] { 114, 5, 116, 2, 19 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		string text = "-";
		SendCommand(array, array.Length, ref reply, ref replySize);
		text = BitConverter.ToString(reply).Replace("-", "");
		byte b = reply[5];
		byte b2 = reply[6];
		byte b3 = reply[7];
		byte b4 = reply[8];
		byte b5 = reply[9];
		byte b6 = reply[10];
		Hasil3 = $"{b}-{b2}, {b3}-{b4}, {b5}-{b6}";
	}

	private void dtc4()
	{
		byte[] array = new byte[5] { 114, 5, 116, 1, 20 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		string text = "-";
		SendCommand(array, array.Length, ref reply, ref replySize);
		text = BitConverter.ToString(reply).Replace("-", "");
		byte b = reply[5];
		byte b2 = reply[6];
		byte b3 = reply[7];
		byte b4 = reply[8];
		byte b5 = reply[9];
		byte b6 = reply[10];
		Hasil4 = $"{b}-{b2}, {b3}-{b4}, {b5}-{b6}";
	}

	private void Gabung(ref string gabung, string hasil1, string hasil2, string hasil3, string hasil4)
	{
		gabung = hasil1 + ", " + hasil2 + ", " + hasil3 + ", " + hasil4;
	}

	private void ContohPemanggilanGabung()
	{
		Gabung(ref hasilGabungan, Hasil, Hasil2, Hasil3, Hasil4);
		Console.WriteLine(hasilGabungan);
		DisplayResponse(hasilGabungan);
		dtcCounter = 1;
		dtcTitleAdded = false;
	}

	private void DisplayResponse(string hasilGabungan)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "1-1", "MAP sensor circuit low voltage" },
			{ "1-2", "MAP sensor circuit high voltage" },
			{ "2-1", "MAP sensor performance problem" },
			{ "7-1", "ECT sensor circuit low voltage" },
			{ "7-2", "ECT sensor circuit high voltage" },
			{ "8-1", "TP sensor circuit low voltage" },
			{ "8-2", "TP sensor circuit high voltage" },
			{ "9-1", "IAT sensor circuit low voltage" },
			{ "9-2", "IAT sensor circuit high voltage" },
			{ "11-1", "VS sensor no signal" },
			{ "12-1", "No.1 primary injector circuit malfunction" },
			{ "13-1", "No.2 primary injector circuit malfunction" },
			{ "14-1", "No.3 primary injector circuit malfunction" },
			{ "15-1", "No.4 primary injector circuit malfunction" },
			{ "16-1", "No.1 secondary injector circuit malfunction" },
			{ "17-1", "No.2 secondary injector circuit malfunction" },
			{ "18-1", "CMP sensor no signal" },
			{ "19-1", "CKP sensor no signal" },
			{ "21-1", "0₂ sensor malfunction" },
			{ "23-1", "0₂ sensor heater malfunction" },
			{ "25-2", "Knock sensor circuit malfunction" },
			{ "25-3", "Knock sensor circuit malfunction" },
			{ "29-1", "IACV circuit malfunction" },
			{ "33-2", "ECM EEPROM malfunction" },
			{ "34-1", "ECV POT low voltage malfunction" },
			{ "34-2", "ECV POT high voltage malfunction" },
			{ "35-1", "EGCA malfunction" },
			{ "48-1", "No.3 secondary injector circuit malfunction" },
			{ "49-1", "No.4 secondary injector circuit malfunction" },
			{ "51-1", "HESD linear solenoid malfunction" },
			{ "54-1", "Bank angle sensor circuit low voltage" },
			{ "54-2", "Bank angle sensor circuit high voltage" },
			{ "56-1", "Knock sensor IC malfunction" },
			{ "57-1", "Engine coolant over temperature" },
			{ "57-2", "Engine coolant over temperature" },
			{ "86-1", "Serial communication malfunction" },
			{ "91-1", "Coil Ignition Primary Malfungtion" },
			{ "91-2", "Coil Ignition Primary Malfungtion" }
		};
		string[] array = hasilGabungan.Split(new string[1] { ", " }, StringSplitOptions.None);
		string[] array2 = array;
		foreach (string key in array2)
		{
			if (dictionary.TryGetValue(key, out var value))
			{
				ShowMessageInListBox(value);
			}
		}
	}

	private void ShowMessageInListBox(string message)
	{
		string item = $"{dtcCounter++}. {message}";
		listBox1.Items.Add(item);
		listBox1.DrawMode = DrawMode.OwnerDrawFixed;
	}

	private void listBox1_DrawItem1(object sender, DrawItemEventArgs e)
	{
		if (e.Index >= 0)
		{
			string s = listBox1.Items[e.Index].ToString();
			float num = 10f;
			float num2 = (float)e.Bounds.Top + num;
			e.Graphics.DrawString(s, listBox1.Font, Brushes.Black, e.Bounds.Left, num2);
			e.DrawFocusRectangle();
		}
	}

	private void ClearAllMessages()
	{
		listBox1.Items.Clear();
		dtcTitleAdded = false;
		listBox1.Refresh();
	}

	private void rst()
	{
		byte[] array = new byte[5] { 114, 5, 96, 1, 40 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		string text = "-";
		SendCommand(array, array.Length, ref reply, ref replySize);
		text = BitConverter.ToString(reply).Replace("-", "");
	}

	private void rst1()
	{
		byte[] array = new byte[5] { 114, 5, 96, 3, 38 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		string text = "-";
		SendCommand(array, array.Length, ref reply, ref replySize);
		text = BitConverter.ToString(reply).Replace("-", "");
	}

	private void rst2()
	{
		byte[] array = new byte[5] { 114, 5, 0, 240, 153 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		string text = "-";
		SendCommand(array, array.Length, ref reply, ref replySize);
		text = BitConverter.ToString(reply).Replace("-", "");
	}

	private void rst3()
	{
		byte[] array = new byte[5] { 114, 5, 96, 1, 40 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		string text = "-";
		SendCommand(array, array.Length, ref reply, ref replySize);
		text = BitConverter.ToString(reply).Replace("-", "");
	}

	private void rst4()
	{
		byte[] array = new byte[5] { 114, 5, 116, 3, 18 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		string text = "-";
		SendCommand(array, array.Length, ref reply, ref replySize);
		text = BitConverter.ToString(reply).Replace("-", "");
	}

	private void InitializeDataGridView2()
	{
		dataGridView2 = new DataGridView
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
			ReadOnly = true,
			Enabled = false,
			AllowUserToAddRows = false,
			AllowUserToDeleteRows = false,
			AllowUserToResizeColumns = false,
			AllowUserToResizeRows = false,
			SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			MultiSelect = false,
			RowHeadersVisible = true
		};
		dataGridView2.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		dataGridView2.ClearSelection();
		dataGridView2.SelectionChanged += (object s, EventArgs e) =>
		{
			dataGridView2.ClearSelection();
		};
		dataGridView2.Columns[0].HeaderText = "Parameter";
		dataGridView2.Columns[1].HeaderText = "Value";
		dataGridView2.Rows.Add("Oxygen Sensor");
		dataGridView2.Rows.Add("Oxygen Sensor Volt");
		dataGridView2.Rows.Add("Short term Fuel Trim");
		dataGridView2.Rows.Add("Air Fuel Ratio");
		dataGridView2.Rows.Add("Fast Idle Step");
		dataGridView2.Rows.Add("Bank Angle Sensor");
		dataGridView2.Rows.Add("Switch Starter");
		dataGridView2.Rows.Add("Side Stand Switch");
		dataGridView2.Rows.Add("Gear Status");
		dataGridView2.Rows.Add("Fuel Pump");
		dataGridView2.Rows.Add("Cooling Fan");
		dataGridView2.Rows.Add("Gear Position");
		for (int num = 0; num < dataGridView2.Rows.Count; num++)
		{
			dataGridView2.Rows[num].HeaderCell.Value = (num + 1).ToString();
		}
		dataGridView2.CellFormatting += (object s, DataGridViewCellFormattingEventArgs e) =>
		{
			e.CellStyle.BackColor = dataGridView2.DefaultCellStyle.BackColor;
			e.CellStyle.SelectionBackColor = dataGridView2.DefaultCellStyle.BackColor;
			e.CellStyle.SelectionForeColor = dataGridView2.DefaultCellStyle.ForeColor;
		};
		panel3.Controls.Add(dataGridView2);
	}

	private void LoopDat17(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			if (tek11)
			{
				dat11();
				dat20();
			}
			if (tek10)
			{
				dat10();
				dat20();
			}
			if (tek13)
			{
				dat13();
				dat20();
			}
			if (tek16)
			{
				dat16();
				dat20();
			}
			if (tek17)
			{
				dat17();
				dat20();
			}
			Task.Delay(10, token);
		}
	}

	private void dat10()
	{
		byte[] array = new byte[5] { 114, 5, 113, 16, 8 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		string text = BitConverter.ToString(reply, 0, (int)replySize);
		Console.WriteLine("Response dat10: " + text);
		if (reply[1] == 5)
		{
			tek10 = false;
			return;
		}
		try
		{
			if (reply.Length >= 1)
			{
				int rpm = (reply[4] << 8) + reply[5];
				float tps = (float)(int)reply[6] * 5f / 256f;
				float tps2 = (float)(int)reply[7] / 2f;
				float ect = (float)(int)reply[8] * 5f / 256f;
				float ect2 = reply[9] - 40;
				float iat = (float)(int)reply[10] * 5f / 256f;
				float iat2 = reply[11] - 40;
				float map = (float)(int)reply[12] * 5f / 256f;
				int num = reply[13];
				float bat = (float)(int)reply[16] / 10f;
				int vss = reply[17];
				float inj = (float)((reply[18] << 8) + reply[19]) / 200f;
				float adv = (float)(int)reply[20] / 2f - 64f;
				UpdateGridView(rpm, tps, tps2, ect, ect2, iat, iat2, map, num, bat, inj, 0, adv, vss, 16);
			}
			else
			{
				Console.WriteLine("Response array is too small to be parsed.");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
	}

	private void datD1()
	{
		byte[] array = new byte[5] { 114, 5, 113, 209, 71 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		try
		{
			Console.WriteLine("array9 (Hex): " + BitConverter.ToString(reply));
			bas = (sss = (ssw = (ggs = (pom = (fan = (ggv = "OFF"))))));
			ggs = "Netral";
			switch (reply[4])
			{
			case 1:
				bas = "ON";
				ggs = "Netral";
				ssw = "OFF";
				sss = "ON";
				break;
			case 2:
				ssw = "ON";
				break;
			case 0:
				ggs = "OnGear";
				ssw = "OFF";
				sss = "OFF";
				break;
			default:
				ssw = "OFF";
				break;
			}
			switch (reply[8])
			{
			case 0:
				pom = "OFF";
				break;
			case 1:
			case 5:
				pom = "ON";
				break;
			case 4:
				pom = "OFF";
				break;
			}
			fan = ((reply[9] == 1) ? "ON" : "OFF");
			switch (reply[10])
			{
			case 128:
				ggv = "6";
				break;
			case 16:
				ggv = "5";
				break;
			case 8:
				ggv = "4";
				break;
			case 4:
				ggv = "3";
				break;
			case 2:
				ggv = "2";
				break;
			case 1:
				ggv = "1";
				break;
			case 32:
				ggv = "N";
				break;
			case 0:
				ggv = "-";
				break;
			default:
				ggv = "-";
				break;
			}
			UpdateGridView2(oxi, o2v, stf, afr, fiz, bas, sss, ssw, ggs, pom, fan, ggv);
			Console.WriteLine("Data updated in GridView.");
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
	}

	private void dat20()
	{
		byte[] array = new byte[5] { 114, 5, 113, 32, 248 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		try
		{
			o2 = (float)(BitConverter.ToUInt16(reply, 4) & 0xFFFF) / 13.142708f / 1000f;
			stf = (float)(reply[5] & 0xFF) / 128.25652f;
			oxi = o2;
			o2v = (float)(reply[4] & 0xFF) / 50.9937f;
			afr = (float)(-5.351 * (double)o2v + 17.700000762939453);
			UpdateGridView2(oxi, o2v, stf, afr, fiz, bas, sss, ssw, ggs, pom, fan, ggv);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
		datD1();
	}

	private void UpdateGridView2(float oxi, float o2v, float stf, float afr, string fiz, string bas, string sss, string ssw, string ggs, string pom, string fan, string ggv)
	{
		if (dataGridView2.InvokeRequired)
		{
			dataGridView2.Invoke((Action)(() =>
			{
				UpdateRows2(oxi, o2v, stf, afr, fiz, bas, sss, ssw, ggs, pom, fan, ggv);
			}));
		}
		else
		{
			UpdateRows2(oxi, o2v, stf, afr, fiz, bas, sss, ssw, ggs, pom, fan, ggv);
		}
	}

	private void UpdateRows2(float oxi, float o2v, float stf, float afr, string fiz, string bas, string sss, string ssw, string ggs, string pom, string fan, string ggv)
	{
		Console.WriteLine("Updating GridView with bas: " + bas + ", sss: " + sss + ", ssw: " + ssw + ", ggs: " + ggs + ", pom: " + pom + ", fan: " + fan + ", ggv: " + ggv);
		dataGridView2.Rows[0].Cells[1].Value = oxi.ToString("F4");
		dataGridView2.Rows[1].Cells[1].Value = o2v.ToString("F4") + " Volt";
		dataGridView2.Rows[2].Cells[1].Value = stf.ToString("F4");
		dataGridView2.Rows[3].Cells[1].Value = afr.ToString("F2") + " : 1";
		dataGridView2.Rows[4].Cells[1].Value = fiz;
		dataGridView2.Rows[5].Cells[1].Value = bas;
		dataGridView2.Rows[6].Cells[1].Value = sss;
		dataGridView2.Rows[7].Cells[1].Value = ssw;
		dataGridView2.Rows[8].Cells[1].Value = ggs;
		dataGridView2.Rows[9].Cells[1].Value = pom;
		dataGridView2.Rows[10].Cells[1].Value = fan;
		dataGridView2.Rows[11].Cells[1].Value = ggv;
		dataGridView2.InvalidateCell(dataGridView2.Rows[5].Cells[1]);
		dataGridView2.InvalidateCell(dataGridView2.Rows[6].Cells[1]);
		dataGridView2.InvalidateCell(dataGridView2.Rows[7].Cells[1]);
		dataGridView2.InvalidateCell(dataGridView2.Rows[8].Cells[1]);
		dataGridView2.InvalidateCell(dataGridView2.Rows[9].Cells[1]);
		dataGridView2.InvalidateCell(dataGridView2.Rows[10].Cells[1]);
		dataGridView2.InvalidateCell(dataGridView2.Rows[11].Cells[1]);
	}

	private void dat11()
	{
		byte[] array = new byte[5] { 114, 5, 113, 17, 7 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		string text = BitConverter.ToString(reply, 0, (int)replySize);
		Console.WriteLine("Response 11: " + text);
		if (reply[1] == 5)
		{
			tek11 = false;
			return;
		}
		try
		{
			if (reply.Length >= 1)
			{
				int rpm = (reply[4] << 8) + reply[5];
				float tps = (float)(int)reply[6] * 5f / 256f;
				float tps2 = (float)(int)reply[7] / 2f;
				float ect = (float)(int)reply[8] * 5f / 256f;
				float ect2 = reply[9] - 40;
				float iat = (float)(int)reply[10] * 5f / 256f;
				float iat2 = reply[11] - 40;
				float map = (float)(int)reply[12] * 5f / 256f;
				int num = reply[13];
				float bat = (float)(int)reply[16] / 10f;
				int vss = reply[17];
				float inj = (float)((reply[18] << 8) + reply[19]) / 200f;
				float adv = (float)(int)reply[20] / 2f - 64f;
				float num2 = (float)(int)reply[22] / 10000f;
				UpdateGridView(rpm, tps, tps2, ect, ect2, iat, iat2, map, num, bat, inj, 0, adv, vss, 17);
			}
			else
			{
				Console.WriteLine("Response array is too small to be parsed.");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
	}

	private void dat13()
	{
		byte[] array = new byte[5] { 114, 5, 113, 19, 5 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		string text = BitConverter.ToString(reply, 0, (int)replySize);
		Console.WriteLine("Response 13: " + text);
		if (reply[1] == 5)
		{
			tek13 = false;
			return;
		}
		try
		{
			if (reply.Length >= 1)
			{
				int rpm = (reply[4] << 8) + reply[5];
				float tps = (float)(int)reply[6] * 5f / 256f;
				float tps2 = (float)(int)reply[7] / 2f;
				float ect = (float)(int)reply[8] * 5f / 256f;
				float ect2 = reply[9] - 40;
				float iat = (float)(int)reply[10] * 5f / 256f;
				float iat2 = reply[11] - 40;
				float map = (float)(int)reply[12] * 5f / 256f;
				int num = reply[13];
				float bat = (float)(int)reply[14] / 10f;
				float inj = (float)((reply[15] << 8) + reply[16]) / 200f;
				float adv = (float)(int)reply[17] / 2f - 64f;
				UpdateGridView(rpm, tps, tps2, ect, ect2, iat, iat2, map, num, bat, inj, 0, adv, 0, 19);
			}
			else
			{
				Console.WriteLine("Response array is too small to be parsed.");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
	}

	private void dat16()
	{
		byte[] array = new byte[5] { 114, 5, 113, 22, 2 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		string text = BitConverter.ToString(reply, 0, (int)replySize);
		Console.WriteLine("Response 16: " + text);
		if (reply[1] == 5)
		{
			tek16 = false;
			return;
		}
		try
		{
			if (reply.Length >= 2)
			{
				int rpm = (reply[4] << 8) + reply[5];
				float num = (float)(int)reply[6] * 5f / 256f;
				float num2 = (float)(int)reply[7] * 5f / 256f;
				float num3 = (float)(int)reply[8] / 2f;
				float num4 = (float)(int)reply[9] / 2f;
				float num5 = (float)(int)reply[10] * 5f / 256f;
				float num6 = (float)(int)reply[11] * 5f / 256f;
				float num7 = (float)(int)reply[12] / 2f;
				float num8 = (float)(int)reply[13] / 2f;
				int num9 = reply[14];
				float num10 = (float)(int)reply[15] * 5f / 256f;
				float num11 = (float)(int)reply[16] - 40f;
				float num12 = (float)(int)reply[17] * 5f / 256f;
				float num13 = (float)(int)reply[18] - 40f;
				float num14 = (float)(int)reply[19] * 5f / 256f;
				int num15 = reply[20];
				float num16 = (float)(int)reply[21] / 10f;
				int num17 = reply[22];
				float num18 = (float)(int)reply[23] / 200f;
				float num19 = (float)(int)reply[24] / 2f - 64f;
				UpdateGridView(rpm, (int)Math.Round(num), (int)Math.Round(num2), (int)Math.Round(num3), (int)Math.Round(num4), (int)Math.Round(num5), (int)Math.Round(num6), (int)Math.Round(num7), (int)Math.Round(num8), num9, (int)Math.Round(num10), (int)Math.Round(num11), (int)Math.Round(num12), (int)Math.Round(num13), (int)Math.Round(num14), num15, (int)Math.Round(num16), num17, (int)Math.Round(num18), (int)Math.Round(num19));
			}
			else
			{
				Console.WriteLine("Response array is too small to be parsed.");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
	}

	private void dat17()
	{
		byte[] array = new byte[5] { 114, 5, 113, 23, 1 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		string text = BitConverter.ToString(reply, 0, (int)replySize);
		Console.WriteLine("Response 17: " + text);
		if (reply[1] == 5)
		{
			tek17 = false;
			return;
		}
		try
		{
			int rpm = (reply[4] << 8) + reply[5];
			float tps = (float)(int)reply[6] * 5f / 256f;
			float tps2 = (float)(int)reply[7] / 2f;
			float ect = ((reply[12] != byte.MaxValue) ? ((float)(int)reply[12] * 5f / 256f) : ((float)(int)reply[8] * 5f / 256f));
			float ect2 = ((reply[13] != byte.MaxValue) ? (reply[13] - 40) : (reply[9] - 40));
			float bat = (float)(int)reply[14] / 10f;
			float inj = (float)((reply[15] << 8) + reply[16]) / 200f;
			int num = reply[18];
			int num2 = 0;
			num2 = ((num == 128 || num == 103) ? 1 : (num switch
			{
				102 => 2, 
				60 => 3, 
				89 => 4, 
				121 => 0, 
				_ => 0, 
			}));
			if (num < 128)
			{
				fiz = "ON";
			}
			else
			{
				fiz = "OFF";
			}
			float adv = (float)(int)reply[17] / 2f - 64f;
			int vss = reply[19];
			int tableType = reply[2];
			UpdateGridView(rpm, tps, tps2, ect, ect2, 0f, 0f, 0f, 0f, bat, inj, num2, adv, vss, tableType);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
	}

	private void UpdateGridView(int rpm, float tps1, float tps2, float ect1, float ect2, float iat1, float iat2, float map1, float map2, float bat, float inj, int MODE, float adv, int vss, int tableType, float ap1_0 = 0f, float ap2_0 = 0f, float ap1_1 = 0f, float ap2_1 = 0f, float tp1_0 = 0f, float tp2_0 = 0f, float tp1_1 = 0f, float tp2_1 = 0f, int cor = 0, float iat11 = 0f, float iat21 = 0f, float map11 = 0f, int map21 = 0)
	{
		if (dataGridView.InvokeRequired)
		{
			dataGridView.Invoke((Action)(() =>
			{
				UpdateRows(rpm, tps1, tps2, ect1, ect2, iat1, iat2, map1, map2, bat, inj, MODE, adv, vss, tableType, ap1_0, ap2_0, ap1_1, ap2_1, tp1_0, tp2_0, tp1_1, tp2_1, cor, iat11, iat21, map11, map21);
			}));
		}
		else
		{
			UpdateRows(rpm, tps1, tps2, ect1, ect2, iat1, iat2, map1, map2, bat, inj, MODE, adv, vss, tableType, ap1_0, ap2_0, ap1_1, ap2_1, tp1_0, tp2_0, tp1_1, tp2_1, cor, iat11, iat21, map11, map21);
		}
	}

	private void UpdateRows(int rpm, float tps1, float tps2, float ect1, float ect2, float iat1, float iat2, float map1, float map2, float bat, float inj, int MODE, float adv, int vss, int tableType, float ap1_0, float ap2_0, float ap1_1, float ap2_1, float tp1_0, float tp2_0, float tp1_1, float tp2_1, int cor, float iat11, float iat21, float map11, int map21)
	{
		dataGridView.Rows[0].Cells[1].Value = rpm + " RPM";
		dataGridView.Rows[1].Cells[1].Value = tps1 + " Volt";
		dataGridView.Rows[2].Cells[1].Value = tps2 + " °";
		dataGridView.Rows[3].Cells[1].Value = ect1 + " Volt";
		dataGridView.Rows[4].Cells[1].Value = ect2 + " °C";
		dataGridView.Rows[5].Cells[1].Value = iat1 + " Volt";
		dataGridView.Rows[6].Cells[1].Value = iat2 + " °C";
		dataGridView.Rows[7].Cells[1].Value = map1 + " Volt";
		dataGridView.Rows[8].Cells[1].Value = map2.ToString();
		dataGridView.Rows[9].Cells[1].Value = bat + " Volt";
		dataGridView.Rows[10].Cells[1].Value = inj + " ms";
		dataGridView.Rows[11].Cells[1].Value = MODE.ToString();
		dataGridView.Rows[12].Cells[1].Value = adv + " °";
		dataGridView.Rows[13].Cells[1].Value = vss + " km/h";
		dataGridView.Rows[14].Cells[1].Value = tableType.ToString();
		dataGridView.Rows[15].Cells[1].Value = ap1_0 + " Volt";
		dataGridView.Rows[16].Cells[1].Value = ap2_0 + " Volt";
		dataGridView.Rows[17].Cells[1].Value = ap1_1 + " °";
		dataGridView.Rows[18].Cells[1].Value = ap2_1 + " °";
		dataGridView.Rows[19].Cells[1].Value = tp1_0 + " Volt";
		dataGridView.Rows[20].Cells[1].Value = tp2_0 + " Volt";
		dataGridView.Rows[21].Cells[1].Value = tp1_1 + " °";
		dataGridView.Rows[22].Cells[1].Value = tp2_1 + " °";
		dataGridView.Rows[23].Cells[1].Value = cor.ToString();
		dataGridView.Rows[24].Cells[1].Value = iat11 + " Volt";
		dataGridView.Rows[25].Cells[1].Value = iat21 + " °C";
		dataGridView.Rows[26].Cells[1].Value = map11 + " Volt";
		dataGridView.Rows[27].Cells[1].Value = map21.ToString();
	}

	private void start()
	{
		_cancellationTokenSource = new CancellationTokenSource();
		Thread thread = new Thread(() =>
		{
			LoopDat17(_cancellationTokenSource.Token);
		})
		{
			IsBackground = true
		};
		thread.Start();
	}

	private async void button2_Click(object sender, EventArgs e)
	{
		start();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		_cancellationTokenSource?.Cancel();
		CloseFtdiConnection();
		Close();
	}

	private void CloseFtdiConnection()
	{
		if (ftHandle != IntPtr.Zero)
		{
			FTDI.FT_Close(ftHandle);
			ftHandle = IntPtr.Zero;
		}
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

	private bool SendCommand(byte[] command, int commandSize, ref byte[] reply, ref uint replySize, int sleepMs = 0)
	{
		byte[] array = new byte[256];
		byte[] array2 = new byte[256];
		uint uint_ = 0u;
		uint uint_2 = 0u;
		uint uint_3 = 0u;
		uint num = 0u;
		uint num2 = 0u;
		long num3 = 50 + 2 * commandSize;
		bool result;
		if (FTDI.FT_Write(ftHandle, command, (uint)commandSize, ref uint_) > FTDI.FT_STATUS.FT_OK)
		{
			result = false;
		}
		else if (FTDI.FT_SetLatencyTimer(ftHandle, 8) <= FTDI.FT_STATUS.FT_OK)
		{
			if (sleepMs > 0)
			{
				Thread.Sleep(sleepMs);
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (true)
			{
				if (FTDI.FT_GetQueueStatus(ftHandle, ref uint_2) == FTDI.FT_STATUS.FT_OK && uint_2 != 0 && (ulong)uint_2 < (ulong)array.Length && FTDI.FT_Read(ftHandle, array, uint_2, ref uint_3) == FTDI.FT_STATUS.FT_OK && uint_3 != 0)
				{
					Array.Copy(array, 0L, array2, num, uint_3);
					num += uint_3;
					if ((ulong)num >= (ulong)(commandSize + 2))
					{
						if (num2 == 0)
						{
							num2 = array2[commandSize + 1];
						}
						if (num - commandSize == num2)
						{
							replySize = num2;
							Array.Copy(array2, num - num2, reply, 0L, num2);
							if (ValidateChecksum(reply, (int)num2))
							{
								break;
							}
						}
					}
				}
				if (stopwatch.ElapsedMilliseconds < num3)
				{
					continue;
				}
				stopwatch.Stop();
				FTDI.FT_Purge(ftHandle, 3u);
				return false;
			}
			stopwatch.Stop();
			FTDI.FT_Purge(ftHandle, 3u);
			result = true;
		}
		else
		{
			result = false;
		}
		return result;
	}

	private bool ValidateChecksum(byte[] data, int dataSize)
	{
		return Checksum8BitHonda(data, dataSize - 1) == data[dataSize - 1];
	}

	private byte Checksum8BitHonda(byte[] data, int dataSize, int startPos = 0)
	{
		int num = startPos + dataSize;
		int num2 = 0;
		while (startPos < num)
		{
			num2 += data[startPos];
			startPos++;
		}
		return (byte)(((num2 ^ 0xFF) + 1) & 0xFF);
	}

	private void button3_Click(object sender, EventArgs e)
	{
		_cancellationTokenSource?.Cancel();
		listBox1.Visible = true;
		ClearAllMessages();
		dtc();
		dtc2();
		dtc3();
		dtc4();
		ContohPemanggilanGabung();
	}

	private void button4_Click(object sender, EventArgs e)
	{
		_cancellationTokenSource?.Cancel();
		rst();
		rst1();
		rst2();
		rst3();
		rst4();
		Thread.Sleep(1000);
		MessageBox.Show("Reset ECM OK", "Reset ECM", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		ClearAllMessages();
		dtc();
		dtc2();
		dtc3();
		dtc4();
		ContohPemanggilanGabung();
	}

	private void SecondForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		_cancellationTokenSource?.Cancel();
	}

	private void Diag_FormClosing(object sender, FormClosingEventArgs e)
	{
		CloseFtdiConnection();
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		teks = teks.Substring(1) + teks[0];
		Text = teks;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3D_Chart_Graph.Diag));
		this.panel1 = new System.Windows.Forms.Panel();
		this.button1 = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.button3 = new System.Windows.Forms.Button();
		this.button4 = new System.Windows.Forms.Button();
		this.panel4 = new System.Windows.Forms.Panel();
		this.listBox1 = new System.Windows.Forms.ListBox();
		this.panel3 = new System.Windows.Forms.Panel();
		this.panel2 = new System.Windows.Forms.Panel();
		this.label1 = new System.Windows.Forms.Label();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.panel1.SuspendLayout();
		this.panel4.SuspendLayout();
		base.SuspendLayout();
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel1.Controls.Add(this.label1);
		this.panel1.Controls.Add(this.button1);
		this.panel1.Controls.Add(this.button2);
		this.panel1.Controls.Add(this.button3);
		this.panel1.Controls.Add(this.button4);
		this.panel1.Controls.Add(this.panel4);
		this.panel1.Controls.Add(this.panel3);
		this.panel1.Controls.Add(this.panel2);
		this.panel1.Location = new System.Drawing.Point(6, 6);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(1337, 670);
		this.panel1.TabIndex = 0;
		this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.button1.Font = new System.Drawing.Font("Franklin Gothic Demi Cond", 20.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.button1.Location = new System.Drawing.Point(1184, 607);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(146, 56);
		this.button1.TabIndex = 1;
		this.button1.Text = "❎Exit";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(this.button1_Click);
		this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.button2.Font = new System.Drawing.Font("Franklin Gothic Demi Cond", 20.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.button2.Location = new System.Drawing.Point(1027, 607);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(151, 56);
		this.button2.TabIndex = 2;
		this.button2.Text = "⚡Start";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(this.button2_Click);
		this.button3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.button3.Font = new System.Drawing.Font("Franklin Gothic Demi Cond", 20.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.button3.Location = new System.Drawing.Point(850, 607);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(171, 56);
		this.button3.TabIndex = 3;
		this.button3.Text = "\ud83d\udcc4Read DTC";
		this.button3.UseVisualStyleBackColor = true;
		this.button3.Click += new System.EventHandler(this.button3_Click);
		this.button4.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.button4.Font = new System.Drawing.Font("Franklin Gothic Demi Cond", 20.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.button4.Location = new System.Drawing.Point(673, 607);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(171, 56);
		this.button4.TabIndex = 4;
		this.button4.Text = "\ud83d\udd04Reset ECM";
		this.button4.UseVisualStyleBackColor = true;
		this.button4.Click += new System.EventHandler(this.button4_Click);
		this.panel4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel4.BackColor = System.Drawing.Color.Gray;
		this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel4.Controls.Add(this.listBox1);
		this.panel4.Font = new System.Drawing.Font("Franklin Gothic Demi Cond", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.panel4.Location = new System.Drawing.Point(673, 343);
		this.panel4.Name = "panel4";
		this.panel4.Size = new System.Drawing.Size(657, 258);
		this.panel4.TabIndex = 2;
		this.listBox1.BackColor = System.Drawing.Color.Gray;
		this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.listBox1.FormattingEnabled = true;
		this.listBox1.ItemHeight = 21;
		this.listBox1.Location = new System.Drawing.Point(-2, -2);
		this.listBox1.Name = "listBox1";
		this.listBox1.Size = new System.Drawing.Size(645, 273);
		this.listBox1.TabIndex = 0;
		this.panel3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel3.BackColor = System.Drawing.Color.Gray;
		this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel3.Font = new System.Drawing.Font("Franklin Gothic Demi Cond", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.panel3.Location = new System.Drawing.Point(673, 3);
		this.panel3.Name = "panel3";
		this.panel3.Size = new System.Drawing.Size(657, 290);
		this.panel3.TabIndex = 1;
		this.panel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.panel2.BackColor = System.Drawing.Color.Gray;
		this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel2.Font = new System.Drawing.Font("Franklin Gothic Demi Cond", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.panel2.Location = new System.Drawing.Point(3, 3);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(664, 660);
		this.panel2.TabIndex = 0;
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Franklin Gothic Demi", 27.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.White;
		this.label1.Location = new System.Drawing.Point(767, 296);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(497, 43);
		this.label1.TabIndex = 5;
		this.label1.Text = "\ud83d\udd0d Diagnosa Kerusakan (DTC)";
		this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(1349, 683);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "Diag";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Diag";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Diag_FormClosing);
		base.Load += new System.EventHandler(this.Diag_Load);
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		this.panel4.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
