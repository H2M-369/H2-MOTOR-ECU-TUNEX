#define DEBUG
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace _3D_Chart_Graph;

public class SplashScreen : Form
{
	private Timer timer;

	private const string KeyFilePath = "license.lic";

	private readonly byte[] Key = Encoding.UTF8.GetBytes("ABCDEF0123456789");

	private readonly byte[] IV = Encoding.UTF8.GetBytes("0123456789ABCDEF");

	private IContainer components = null;

	private SaveFileDialog saveFileDialog1;

	public SplashScreen()
	{
		InitializeComponent();
		try
		{
			using (Stream stream = typeof(SplashScreen).Assembly.GetManifestResourceStream("EcuTuneX.splash_tunex.jpg"))
			{
				if (stream != null)
				{
					using (Image image = Image.FromStream(stream)) BackgroundImage = new Bitmap(image);
					BackgroundImageLayout = ImageLayout.Stretch;
				}
			}
		}
		catch (Exception) { }
		ControlBox = false;
		timer = new Timer();
		timer.Interval = 7000;
		timer.Tick += Timer_Tick;
		timer.Start();
	}

	private void Timer_Tick(object sender, EventArgs e)
	{
		timer.Stop();
		Close();
	}

	private void SplashScreen_Load(object sender, EventArgs e)
	{
		string text = GetProcessorId().Trim();
		Debug.WriteLine("Processor Serial Number: " + text);
		string text2 = GetDiskSerialNumber().Trim();
		Debug.WriteLine("Disk Serial Number: " + text2);
		string text3 = text + "_" + text2;
		Debug.WriteLine("Combined Serial Number: " + text3);
		string text4 = ProcessDiskSerial(text3);
		Debug.WriteLine("Processed Combined Serial Number: " + text4);
		string[] array = DecryptKeyFromFile();
		string text5 = array[0];
		string text6 = array[1];
		string text7 = array[2];
		Debug.WriteLine("Stored Key: " + text5);
		Debug.WriteLine("User Name: " + text6);
		Debug.WriteLine("Phone Number: " + text7);
		if (string.IsNullOrEmpty(text6) || string.IsNullOrEmpty(text7) || text4 != text5)
		{
			string why = H2License.Diagnose(array, text4);
			if (why.Length == 0) why = "Nama / No. HP pada lisensi kosong.";
			MessageBox.Show(why + "\n\nSimpan HWID komputer ini pada dialog berikut bila perlu membuat lisensi baru.", "Lisensi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			ShowSaveFileDialog1(text3);
			Application.Exit();
		}
	}

	private void ShowSaveFileDialog1(string combinedSerialNumber)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "Text files (*.dat)|*.dat|All files (*.*)|*.*",
			Title = "Save HWID",
			FileName = "HWID.dat"
		};
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			try
			{
				File.WriteAllText(saveFileDialog.FileName, combinedSerialNumber);
				Debug.WriteLine("Saved serial number to " + saveFileDialog.FileName);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error saving file: " + ex.Message);
			}
		}
	}

	private static string GetDiskSerialNumber()
	{
		try
		{
			string text = string.Empty;
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia");
			foreach (ManagementObject item in managementObjectSearcher.Get())
			{
				text = item["SerialNumber"]?.ToString().Trim();
				if (!string.IsNullOrEmpty(text))
				{
					break;
				}
			}
			return text;
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Error retrieving disk serial number: " + ex.Message);
			return "Error";
		}
	}

	private static string GetProcessorId()
	{
		string result = string.Empty;
		ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
		using (ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectSearcher.Get().GetEnumerator())
		{
			if (managementObjectEnumerator.MoveNext())
			{
				ManagementObject managementObject = (ManagementObject)managementObjectEnumerator.Current;
				result = managementObject["ProcessorId"]?.ToString();
			}
		}
		return result;
	}

	private string ProcessDiskSerial(string serialNumber)
	{
		return H2License.HwidHash(serialNumber);
	}

	private string DecryptString(string encryptedText)
	{
		using Aes aes = Aes.Create();
		aes.Key = Key;
		aes.IV = IV;
		ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
		try
		{
			byte[] buffer = Convert.FromBase64String(encryptedText);
			using MemoryStream stream = new MemoryStream(buffer);
			using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
			using StreamReader streamReader = new StreamReader(stream2);
			return streamReader.ReadToEnd();
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Error decrypting string: " + ex.Message);
			return "Error";
		}
	}

	private string[] DecryptKeyFromFile()
	{
		return H2License.Load("license.lic");
	}

	private string HashSerialNumber(string serialNumber)
	{
		using SHA256 sHA = SHA256.Create();
		byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(serialNumber));
		return BitConverter.ToString(array).Replace("-", "").ToUpper();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3D_Chart_Graph.SplashScreen));
		this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		base.ClientSize = new System.Drawing.Size(292, 140);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "SplashScreen";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		base.Load += new System.EventHandler(this.SplashScreen_Load);
		base.ResumeLayout(false);
	}
}
