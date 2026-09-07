using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ServiceTools;

public class ServiceTool
{
	[StructLayout(LayoutKind.Sequential)]
	private class SERVICE_STATUS
	{
		public int dwServiceType;

		public ServiceState dwCurrentState;

		public int dwControlsAccepted;

		public int dwWin32ExitCode;

		public int dwServiceSpecificExitCode;

		public int dwCheckPoint;

		public int dwWaitHint;
	}

	private const int STANDARD_RIGHTS_REQUIRED = 983040;

	private const int SERVICE_WIN32_OWN_PROCESS = 16;

	[DllImport("advapi32.dll", EntryPoint = "OpenSCManagerA")]
	private static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, ServiceManagerRights dwDesiredAccess);

	[DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "OpenServiceA")]
	private static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, ServiceRights dwDesiredAccess);

	[DllImport("advapi32.dll", EntryPoint = "CreateServiceA")]
	private static extern IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName, ServiceRights dwDesiredAccess, int dwServiceType, ServiceBootFlag dwStartType, ServiceError dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lp, string lpPassword);

	[DllImport("advapi32.dll")]
	private static extern int CloseServiceHandle(IntPtr hSCObject);

	[DllImport("advapi32.dll")]
	private static extern int QueryServiceStatus(IntPtr hService, SERVICE_STATUS lpServiceStatus);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern int DeleteService(IntPtr hService);

	[DllImport("advapi32.dll")]
	private static extern int ControlService(IntPtr hService, ServiceControl dwControl, SERVICE_STATUS lpServiceStatus);

	[DllImport("advapi32.dll", EntryPoint = "StartServiceA")]
	private static extern int StartService(IntPtr hService, int dwNumServiceArgs, int lpServiceArgVectors);

	public static void Uninstall(string ServiceName)
	{
		IntPtr intPtr = OpenSCManager(ServiceManagerRights.Connect);
		try
		{
			IntPtr intPtr2 = OpenService(intPtr, ServiceName, ServiceRights.StandardRightsRequired | ServiceRights.QueryStatus | ServiceRights.Stop);
			if (intPtr2 == IntPtr.Zero)
			{
				throw new ApplicationException("Servis yüklü değil.");
			}
			try
			{
				StopService(intPtr2);
				if (DeleteService(intPtr2) == 0)
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					throw new ApplicationException("Servis silinemedi " + lastWin32Error);
				}
			}
			finally
			{
				CloseServiceHandle(intPtr2);
			}
		}
		finally
		{
			CloseServiceHandle(intPtr);
		}
	}

	public static bool ServiceIsInstalled(string ServiceName)
	{
		IntPtr intPtr = OpenSCManager(ServiceManagerRights.Connect);
		try
		{
			IntPtr intPtr2 = OpenService(intPtr, ServiceName, ServiceRights.QueryStatus);
			if (intPtr2 == IntPtr.Zero)
			{
				return false;
			}
			CloseServiceHandle(intPtr2);
			return true;
		}
		finally
		{
			CloseServiceHandle(intPtr);
		}
	}

	public static void InstallAndStart(string ServiceName, string DisplayName, string FileName)
	{
		IntPtr intPtr = OpenSCManager(ServiceManagerRights.Connect | ServiceManagerRights.CreateService);
		try
		{
			IntPtr intPtr2 = OpenService(intPtr, ServiceName, ServiceRights.QueryStatus | ServiceRights.Start);
			if (intPtr2 == IntPtr.Zero)
			{
				intPtr2 = CreateService(intPtr, ServiceName, DisplayName, ServiceRights.QueryStatus | ServiceRights.Start, 16, ServiceBootFlag.AutoStart, ServiceError.Normal, FileName, null, IntPtr.Zero, null, null, null);
			}
			if (intPtr2 == IntPtr.Zero)
			{
				throw new ApplicationException("Servis yüklenemedi.");
			}
			try
			{
				StartService(intPtr2);
			}
			finally
			{
				CloseServiceHandle(intPtr2);
			}
		}
		finally
		{
			CloseServiceHandle(intPtr);
		}
	}

	public static void InstallAndStart(string ServiceName, string DisplayName, string FileName, string UserName, string Password)
	{
		IntPtr intPtr = OpenSCManager(ServiceManagerRights.Connect | ServiceManagerRights.CreateService);
		try
		{
			IntPtr intPtr2 = OpenService(intPtr, ServiceName, ServiceRights.QueryStatus | ServiceRights.Start);
			if (intPtr2 == IntPtr.Zero)
			{
				intPtr2 = CreateService(intPtr, ServiceName, DisplayName, ServiceRights.QueryStatus | ServiceRights.Start, 16, ServiceBootFlag.AutoStart, ServiceError.Normal, FileName, null, IntPtr.Zero, null, UserName, Password);
			}
			if (intPtr2 == IntPtr.Zero)
			{
				throw new ApplicationException("Servis yüklenemedi.");
			}
			try
			{
				StartService(intPtr2);
			}
			finally
			{
				CloseServiceHandle(intPtr2);
			}
		}
		finally
		{
			CloseServiceHandle(intPtr);
		}
	}

	public static void StartService(string Name)
	{
		IntPtr intPtr = OpenSCManager(ServiceManagerRights.Connect);
		try
		{
			IntPtr intPtr2 = OpenService(intPtr, Name, ServiceRights.QueryStatus | ServiceRights.Start);
			if (intPtr2 == IntPtr.Zero)
			{
				throw new ApplicationException("Servis açılamadı.");
			}
			try
			{
				StartService(intPtr2);
			}
			finally
			{
				CloseServiceHandle(intPtr2);
			}
		}
		finally
		{
			CloseServiceHandle(intPtr);
		}
	}

	public static void StopService(string Name)
	{
		IntPtr intPtr = OpenSCManager(ServiceManagerRights.Connect);
		try
		{
			IntPtr intPtr2 = OpenService(intPtr, Name, ServiceRights.QueryStatus | ServiceRights.Stop);
			if (intPtr2 == IntPtr.Zero)
			{
				throw new ApplicationException("Servis kapatılamadı.");
			}
			try
			{
				StopService(intPtr2);
			}
			finally
			{
				CloseServiceHandle(intPtr2);
			}
		}
		finally
		{
			CloseServiceHandle(intPtr);
		}
	}

	private static void StartService(IntPtr hService)
	{
		new SERVICE_STATUS();
		StartService(hService, 0, 0);
		WaitForServiceStatus(hService, ServiceState.Starting, ServiceState.Run);
	}

	private static void StopService(IntPtr hService)
	{
		SERVICE_STATUS lpServiceStatus = new SERVICE_STATUS();
		ControlService(hService, ServiceControl.Stop, lpServiceStatus);
		WaitForServiceStatus(hService, ServiceState.Stopping, ServiceState.Stop);
	}

	public static ServiceState GetServiceStatus(string ServiceName)
	{
		IntPtr intPtr = OpenSCManager(ServiceManagerRights.Connect);
		try
		{
			IntPtr intPtr2 = OpenService(intPtr, ServiceName, ServiceRights.QueryStatus);
			if (intPtr2 == IntPtr.Zero)
			{
				return ServiceState.NotFound;
			}
			try
			{
				return GetServiceStatus(intPtr2);
			}
			finally
			{
				CloseServiceHandle(intPtr);
			}
		}
		finally
		{
			CloseServiceHandle(intPtr);
		}
	}

	private static ServiceState GetServiceStatus(IntPtr hService)
	{
		SERVICE_STATUS sERVICE_STATUS = new SERVICE_STATUS();
		if (QueryServiceStatus(hService, sERVICE_STATUS) == 0)
		{
			throw new ApplicationException("Servis durumu bulunamadı.");
		}
		return sERVICE_STATUS.dwCurrentState;
	}

	private static bool WaitForServiceStatus(IntPtr hService, ServiceState WaitStatus, ServiceState DesiredStatus)
	{
		SERVICE_STATUS sERVICE_STATUS = new SERVICE_STATUS();
		QueryServiceStatus(hService, sERVICE_STATUS);
		if (sERVICE_STATUS.dwCurrentState == DesiredStatus)
		{
			return true;
		}
		int tickCount = Environment.TickCount;
		int dwCheckPoint = sERVICE_STATUS.dwCheckPoint;
		while (sERVICE_STATUS.dwCurrentState == WaitStatus)
		{
			int num = sERVICE_STATUS.dwWaitHint / 10;
			if (num < 1000)
			{
				num = 1000;
			}
			else if (num > 10000)
			{
				num = 10000;
			}
			Thread.Sleep(num);
			if (QueryServiceStatus(hService, sERVICE_STATUS) == 0)
			{
				break;
			}
			if (sERVICE_STATUS.dwCheckPoint > dwCheckPoint)
			{
				tickCount = Environment.TickCount;
				dwCheckPoint = sERVICE_STATUS.dwCheckPoint;
			}
			else if (Environment.TickCount - tickCount > sERVICE_STATUS.dwWaitHint)
			{
				break;
			}
		}
		return sERVICE_STATUS.dwCurrentState == DesiredStatus;
	}

	private static IntPtr OpenSCManager(ServiceManagerRights Rights)
	{
		IntPtr intPtr = OpenSCManager(null, null, Rights);
		if (intPtr == IntPtr.Zero)
		{
			throw new ApplicationException("Servis yöneticisi açılamadı.");
		}
		return intPtr;
	}
}
