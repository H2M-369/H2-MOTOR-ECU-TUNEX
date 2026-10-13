using System;
using System.Windows.Forms;

namespace _3D_Chart_Graph;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		using (SplashScreen splashScreen = new SplashScreen())
		{
			splashScreen.ShowDialog();
		}
		Application.Run(new Form1());
	}
}
