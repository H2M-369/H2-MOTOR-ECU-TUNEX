using System;
using System.Runtime.InteropServices;

namespace _3D_Chart_Graph;

internal class FTDI
{
	public enum FT_STATUS
	{
		FT_OK,
		FT_INVALID_HANDLE,
		FT_DEVICE_NOT_FOUND,
		FT_DEVICE_NOT_OPENED,
		FT_IO_ERROR,
		FT_INSUFFICIENT_RESOURCES,
		FT_INVALID_PARAMETER,
		FT_INVALID_BAUD_RATE,
		FT_DEVICE_NOT_OPENED_FOR_ERASE,
		FT_DEVICE_NOT_OPENED_FOR_WRITE,
		FT_FAILED_TO_WRITE_DEVICE,
		FT_EEPROM_READ_FAILED,
		FT_EEPROM_WRITE_FAILED,
		FT_EEPROM_ERASE_FAILED,
		FT_EEPROM_NOT_PRESENT,
		FT_EEPROM_NOT_PROGRAMMED,
		FT_INVALID_ARGS,
		FT_OTHER_ERROR
	}

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_Open(uint uint_0, ref IntPtr intptr_0);

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_Close(IntPtr intptr_0);

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_Read(IntPtr intptr_0, byte[] byte_0, uint uint_0, ref uint uint_1);

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_Write(IntPtr intptr_0, byte[] byte_0, uint uint_0, ref uint uint_1);

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_GetQueueStatus(IntPtr intptr_0, ref uint uint_0);

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_SetBaudRate(IntPtr intptr_0, uint uint_0);

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_SetDataCharacteristics(IntPtr intptr_0, byte byte_0, byte byte_1, byte byte_2);

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_Purge(IntPtr intptr_0, uint uint_0);

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_SetTimeouts(IntPtr intptr_0, uint uint_0, uint uint_1);

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_SetLatencyTimer(IntPtr intptr_0, byte byte_0);

	[DllImport("ftd2xx.dll")]
	public static extern FT_STATUS FT_SetBitMode(IntPtr intptr_0, byte byte_0, byte byte_1);
}
