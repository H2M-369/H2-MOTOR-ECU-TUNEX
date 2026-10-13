using System;
using System.Runtime.InteropServices;

namespace SPECIALIST_PH_TUNER;

internal class Class15
{
	public enum Enum5
	{
		const_0,
		const_1,
		const_2,
		const_3,
		const_4,
		const_5,
		const_6,
		const_7,
		const_8,
		const_9,
		const_10,
		const_11,
		const_12,
		const_13,
		const_14,
		const_15,
		const_16,
		const_17
	}

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_Open(uint uint_0, ref IntPtr intptr_0);

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_Close(IntPtr intptr_0);

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_Read(IntPtr intptr_0, byte[] byte_0, uint uint_0, ref uint uint_1);

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_Write(IntPtr intptr_0, byte[] byte_0, uint uint_0, ref uint uint_1);

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_GetQueueStatus(IntPtr intptr_0, ref uint uint_0);

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_SetBaudRate(IntPtr intptr_0, uint uint_0);

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_SetDataCharacteristics(IntPtr intptr_0, byte byte_0, byte byte_1, byte byte_2);

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_Purge(IntPtr intptr_0, uint uint_0);

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_SetTimeouts(IntPtr intptr_0, uint uint_0, uint uint_1);

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_SetLatencyTimer(IntPtr intptr_0, byte byte_0);

	[DllImport("ftd2xx.dll")]
	public static extern Enum5 FT_SetBitMode(IntPtr intptr_0, byte byte_0, byte byte_1);
}
