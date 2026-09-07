using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Fora.Mikro.Utility;

public class Hdd
{
	[DllImport("kernel32.dll")]
	private static extern long GetVolumeInformation(string PathName, StringBuilder VolumeNameBuffer, uint VolumeNameSize, ref uint VolumeSerialNumber, ref uint MaximumComponentLength, ref uint FileSystemFlags, StringBuilder FileSystemNameBuffer, uint FileSystemNameSize);

	public static string GetHddSerialNo(string strSurucuHarf)
	{
		uint VolumeSerialNumber = 0u;
		uint MaximumComponentLength = 0u;
		StringBuilder stringBuilder = new StringBuilder(256);
		uint FileSystemFlags = 0u;
		StringBuilder stringBuilder2 = new StringBuilder(256);
		strSurucuHarf += ":\\";
		GetVolumeInformation(strSurucuHarf, stringBuilder, (uint)stringBuilder.Capacity, ref VolumeSerialNumber, ref MaximumComponentLength, ref FileSystemFlags, stringBuilder2, (uint)stringBuilder2.Capacity);
		return Convert.ToString(VolumeSerialNumber);
	}
}
