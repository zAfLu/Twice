﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

// http://www.codeproject.com/Articles/73000/Getting-Operating-System-Version-Info-Even-for-Win https://en.wikipedia.org/wiki/List_of_Microsoft_Windows_versions

// Thanks to Member 7861383, Scott Vickery for the Windows 8.1 update and workaround. I have moved it
// to the beginning of the Name property, though...

// Thakts to Brisingr Aerowing for help with the Windows 10 adapatation

namespace Twice.Utilities.Os
{
	/// <summary>
	///     Provides detailed information about the host operating system.
	/// </summary>
	[SuppressMessage( "ReSharper", "MemberCanBePrivate.Global" )]
	[SuppressMessage( "ReSharper", "FieldCanBeMadeReadOnly.Global" )]
	[SuppressMessage( "ReSharper", "MemberCanBePrivate.Local" )]
	[ExcludeFromCodeCoverage]
	public static class OsVersionInfo
	{
		private static IsWow64ProcessDelegate GetIsWow64ProcessDelegate()
		{
			IntPtr handle = LoadLibrary( "kernel32" );

			if( handle != IntPtr.Zero )
			{
				IntPtr fnPtr = GetProcAddress( handle, "IsWow64Process" );

				if( fnPtr != IntPtr.Zero )
				{
					return (IsWow64ProcessDelegate)Marshal.GetDelegateForFunctionPointer( fnPtr, typeof(IsWow64ProcessDelegate) );
				}
			}

			return null;
		}

		[DllImport( "kernel32.dll" )]
		private static extern void GetNativeSystemInfo( [MarshalAs( UnmanagedType.Struct )] ref SYSTEM_INFO lpSystemInfo );

		[DllImport( "kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi )]
		private static extern IntPtr GetProcAddress( IntPtr hwnd, string procedureName );

		[DllImport( "Kernel32.dll" )]
		private static extern bool GetProductInfo( int osMajorVersion, int osMinorVersion, int spMajorVersion,
			int spMinorVersion, out int edition );

		[DllImport( "user32" )]
		private static extern int GetSystemMetrics( int nIndex );

		[DllImport( "kernel32.dll" )]
		private static extern bool GetVersionEx( ref OSVERSIONINFOEX osVersionInfo );

		private static bool Is32BitProcessOn64BitProcessor()
		{
			IsWow64ProcessDelegate fnDelegate = GetIsWow64ProcessDelegate();

			if( fnDelegate == null )
			{
				return false;
			}

			bool isWow64;
			bool retVal = fnDelegate.Invoke( Process.GetCurrentProcess().Handle, out isWow64 );

			if( retVal == false )
			{
				return false;
			}

			return isWow64;
		}

		private static bool IsWindows10()
		{
			string productName = RegistryRead( @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName",
				"" );
			if( productName.StartsWith( "Windows 10", StringComparison.OrdinalIgnoreCase ) )
			{
				return true;
			}

			return false;
		}

		[DllImport( "kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi )]
		private static extern IntPtr LoadLibrary( string libraryName );

		private static string RegistryRead( string registryPath, string field, string defaultValue )
		{
			string rtn = "";
			string backSlash = "";
			string newRegistryPath = "";

			try
			{
				RegistryKey ourKey = null;
				string[] splitResult = registryPath.Split( '\\' );

				if( splitResult.Length > 0 )
				{
					splitResult[0] = splitResult[0].ToUpper(); // Make the first entry uppercase...

					if( splitResult[0] == "HKEY_CLASSES_ROOT" )
						ourKey = Registry.ClassesRoot;
					else if( splitResult[0] == "HKEY_CURRENT_USER" )
						ourKey = Registry.CurrentUser;
					else if( splitResult[0] == "HKEY_LOCAL_MACHINE" )
						ourKey = Registry.LocalMachine;
					else if( splitResult[0] == "HKEY_USERS" )
						ourKey = Registry.Users;
					else if( splitResult[0] == "HKEY_CURRENT_CONFIG" )
						ourKey = Registry.CurrentConfig;

					if( ourKey != null )
					{
						for( int i = 1; i < splitResult.Length; i++ )
						{
							newRegistryPath += backSlash + splitResult[i];
							backSlash = "\\";
						}

						if( newRegistryPath != "" )
						{
							//rtn = (string)Registry.GetValue(RegistryPath, "CurrentVersion", DefaultValue);

							ourKey = ourKey.OpenSubKey( newRegistryPath );
							if( ourKey != null )
							{
								rtn = (string)ourKey.GetValue( field, defaultValue );
								ourKey.Close();
							}
						}
					}
				}
			}
			catch
			{
				// ignored
			}

			return rtn;
		}

		public enum ProcessorArchitecture
		{
			Unknown = 0,
			Bit32 = 1,
			Bit64 = 2,
			Itanium64 = 3
		}

		public enum SoftwareArchitecture
		{
			Unknown = 0,
			Bit32 = 1,
			Bit64 = 2
		}

		private const int PRODUCT_BUSINESS = 0x00000006;
		private const int PRODUCT_BUSINESS_N = 0x00000010;
		private const int PRODUCT_CLUSTER_SERVER = 0x00000012;
		private const int PRODUCT_CLUSTER_SERVER_V = 0x00000040;
		private const int PRODUCT_DATACENTER_SERVER = 0x00000008;
		private const int PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C;
		private const int PRODUCT_DATACENTER_SERVER_CORE_V = 0x00000027;
		private const int PRODUCT_DATACENTER_SERVER_V = 0x00000025;
		private const int PRODUCT_EMBEDDED = 0x00000041;
		private const int PRODUCT_ENTERPRISE = 0x00000004;
		private const int PRODUCT_ENTERPRISE_E = 0x00000046;
		private const int PRODUCT_ENTERPRISE_N = 0x0000001B;
		private const int PRODUCT_ENTERPRISE_SERVER = 0x0000000A;
		private const int PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E;
		private const int PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029;
		private const int PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F;
		private const int PRODUCT_ENTERPRISE_SERVER_V = 0x00000026;
		private const int PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL = 0x0000003C;
		private const int PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC = 0x0000003E;

		//private const int ???? = 0x0000003A;
		private const int PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT = 0x0000003B;

		private const int PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC = 0x0000003D;
		private const int PRODUCT_HOME_BASIC = 0x00000002;
		private const int PRODUCT_HOME_BASIC_E = 0x00000043;
		private const int PRODUCT_HOME_BASIC_N = 0x00000005;
		private const int PRODUCT_HOME_PREMIUM = 0x00000003;
		private const int PRODUCT_HOME_PREMIUM_E = 0x00000044;
		private const int PRODUCT_HOME_PREMIUM_N = 0x0000001A;
		private const int PRODUCT_HOME_PREMIUM_SERVER = 0x00000022;
		private const int PRODUCT_HYPERV = 0x0000002A;
		private const int PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E;
		private const int PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020;
		private const int PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F;
		private const int PRODUCT_PROFESSIONAL = 0x00000030;
		private const int PRODUCT_PROFESSIONAL_E = 0x00000045;
		private const int PRODUCT_PROFESSIONAL_N = 0x00000031;
		private const int PRODUCT_SB_SOLUTION_SERVER = 0x00000032;
		private const int PRODUCT_SB_SOLUTION_SERVER_EM = 0x00000036;
		private const int PRODUCT_SERVER_FOR_SB_SOLUTIONS = 0x00000033;
		private const int PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM = 0x00000037;
		private const int PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018;
		private const int PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023;
		private const int PRODUCT_SERVER_FOUNDATION = 0x00000021;
		private const int PRODUCT_SMALLBUSINESS_SERVER = 0x00000009;
		private const int PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 0x00000019;
		private const int PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE = 0x0000003F;
		private const int PRODUCT_SOLUTION_EMBEDDEDSERVER = 0x00000038;
		private const int PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE = 0x00000039;
		private const int PRODUCT_STANDARD_SERVER = 0x00000007;
		private const int PRODUCT_STANDARD_SERVER_CORE = 0x0000000D;
		private const int PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028;
		private const int PRODUCT_STANDARD_SERVER_SOLUTIONS = 0x00000034;
		private const int PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE = 0x00000035;
		private const int PRODUCT_STANDARD_SERVER_V = 0x00000024;
		private const int PRODUCT_STARTER = 0x0000000B;
		private const int PRODUCT_STARTER_E = 0x00000042;
		private const int PRODUCT_STARTER_N = 0x0000002F;
		private const int PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017;
		private const int PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE = 0x0000002E;
		private const int PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014;
		private const int PRODUCT_STORAGE_EXPRESS_SERVER_CORE = 0x0000002B;
		private const int PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015;
		private const int PRODUCT_STORAGE_STANDARD_SERVER_CORE = 0x0000002C;
		private const int PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016;
		private const int PRODUCT_STORAGE_WORKGROUP_SERVER_CORE = 0x0000002D;
		private const int PRODUCT_ULTIMATE = 0x00000001;
		private const int PRODUCT_ULTIMATE_E = 0x00000047;
		private const int PRODUCT_ULTIMATE_N = 0x0000001C;
		private const int PRODUCT_UNDEFINED = 0x00000000;
		private const int PRODUCT_WEB_SERVER = 0x00000011;
		private const int PRODUCT_WEB_SERVER_CORE = 0x0000001D;
		private const int VER_NT_SERVER = 3;
		private const int VER_NT_WORKSTATION = 1;
		private const int VER_SUITE_BLADE = 1024;
		private const int VER_SUITE_DATACENTER = 128;
		private const int VER_SUITE_ENTERPRISE = 2;
		private const int VER_SUITE_PERSONAL = 512;

		private static string s_Edition;
		private static string s_Name;

		/// <summary>
		///     Gets the edition of the operating system running on this computer.
		/// </summary>
		public static string Edition
		{
			get
			{
				if( s_Edition != null )
					return s_Edition; //***** RETURN *****//

				string edition = string.Empty;

				OperatingSystem osVersion = Environment.OSVersion;
				OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX
				{
					dwOSVersionInfoSize = Marshal.SizeOf( typeof(OSVERSIONINFOEX) )
				};

				if( GetVersionEx( ref osVersionInfo ) )
				{
					int majorVersion = osVersion.Version.Major;
					int minorVersion = osVersion.Version.Minor;
					byte productType = osVersionInfo.wProductType;
					short suiteMask = osVersionInfo.wSuiteMask;

					if( majorVersion == 4 )
					{
						if( productType == VER_NT_WORKSTATION )
						{
							// Windows NT 4.0 Workstation
							edition = "Workstation";
						}
						else if( productType == VER_NT_SERVER )
						{
							edition = ( suiteMask & VER_SUITE_ENTERPRISE ) != 0
								? "Enterprise Server"
								: "Standard Server";
						}
					}
					else if( majorVersion == 5 )
					{
						if( productType == VER_NT_WORKSTATION )
						{
							if( ( suiteMask & VER_SUITE_PERSONAL ) != 0 )
							{
								edition = "Home";
							}
							else
							{
								edition = GetSystemMetrics( 86 ) == 0
									? "Professional"
									: "Tablet Edition";
							}
						}
						else if( productType == VER_NT_SERVER )
						{
							if( minorVersion == 0 )
							{
								if( ( suiteMask & VER_SUITE_DATACENTER ) != 0 )
								{
									// Windows 2000 Datacenter Server
									edition = "Datacenter Server";
								}
								else if( ( suiteMask & VER_SUITE_ENTERPRISE ) != 0 )
								{
									// Windows 2000 Advanced Server
									edition = "Advanced Server";
								}
								else
								{
									// Windows 2000 Server
									edition = "Server";
								}
							}
							else
							{
								if( ( suiteMask & VER_SUITE_DATACENTER ) != 0 )
								{
									// Windows Server 2003 Datacenter Edition
									edition = "Datacenter";
								}
								else if( ( suiteMask & VER_SUITE_ENTERPRISE ) != 0 )
								{
									// Windows Server 2003 Enterprise Edition
									edition = "Enterprise";
								}
								else if( ( suiteMask & VER_SUITE_BLADE ) != 0 )
								{
									// Windows Server 2003 Web Edition
									edition = "Web Edition";
								}
								else
								{
									// Windows Server 2003 Standard Edition
									edition = "Standard";
								}
							}
						}
					}
					else if( majorVersion == 6 )
					{
						int ed;
						if( GetProductInfo( majorVersion, minorVersion,
							osVersionInfo.wServicePackMajor, osVersionInfo.wServicePackMinor,
							out ed ) )
						{
							switch( ed )
							{
							case PRODUCT_BUSINESS:
								edition = "Business";
								break;

							case PRODUCT_BUSINESS_N:
								edition = "Business N";
								break;

							case PRODUCT_CLUSTER_SERVER:
								edition = "HPC Edition";
								break;

							case PRODUCT_CLUSTER_SERVER_V:
								edition = "HPC Edition without Hyper-V";
								break;

							case PRODUCT_DATACENTER_SERVER:
								edition = "Datacenter Server";
								break;

							case PRODUCT_DATACENTER_SERVER_CORE:
								edition = "Datacenter Server (core installation)";
								break;

							case PRODUCT_DATACENTER_SERVER_V:
								edition = "Datacenter Server without Hyper-V";
								break;

							case PRODUCT_DATACENTER_SERVER_CORE_V:
								edition = "Datacenter Server without Hyper-V (core installation)";
								break;

							case PRODUCT_EMBEDDED:
								edition = "Embedded";
								break;

							case PRODUCT_ENTERPRISE:
								edition = "Enterprise";
								break;

							case PRODUCT_ENTERPRISE_N:
								edition = "Enterprise N";
								break;

							case PRODUCT_ENTERPRISE_E:
								edition = "Enterprise E";
								break;

							case PRODUCT_ENTERPRISE_SERVER:
								edition = "Enterprise Server";
								break;

							case PRODUCT_ENTERPRISE_SERVER_CORE:
								edition = "Enterprise Server (core installation)";
								break;

							case PRODUCT_ENTERPRISE_SERVER_CORE_V:
								edition = "Enterprise Server without Hyper-V (core installation)";
								break;

							case PRODUCT_ENTERPRISE_SERVER_IA64:
								edition = "Enterprise Server for Itanium-based Systems";
								break;

							case PRODUCT_ENTERPRISE_SERVER_V:
								edition = "Enterprise Server without Hyper-V";
								break;

							case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT:
								edition = "Essential Business Server MGMT";
								break;

							case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL:
								edition = "Essential Business Server ADDL";
								break;

							case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC:
								edition = "Essential Business Server MGMTSVC";
								break;

							case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC:
								edition = "Essential Business Server ADDLSVC";
								break;

							case PRODUCT_HOME_BASIC:
								edition = "Home Basic";
								break;

							case PRODUCT_HOME_BASIC_N:
								edition = "Home Basic N";
								break;

							case PRODUCT_HOME_BASIC_E:
								edition = "Home Basic E";
								break;

							case PRODUCT_HOME_PREMIUM:
								edition = "Home Premium";
								break;

							case PRODUCT_HOME_PREMIUM_N:
								edition = "Home Premium N";
								break;

							case PRODUCT_HOME_PREMIUM_E:
								edition = "Home Premium E";
								break;

							case PRODUCT_HOME_PREMIUM_SERVER:
								edition = "Home Premium Server";
								break;

							case PRODUCT_HYPERV:
								edition = "Microsoft Hyper-V Server";
								break;

							case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
								edition = "Windows Essential Business Management Server";
								break;

							case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
								edition = "Windows Essential Business Messaging Server";
								break;

							case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
								edition = "Windows Essential Business Security Server";
								break;

							case PRODUCT_PROFESSIONAL:
								edition = "Professional";
								break;

							case PRODUCT_PROFESSIONAL_N:
								edition = "Professional N";
								break;

							case PRODUCT_PROFESSIONAL_E:
								edition = "Professional E";
								break;

							case PRODUCT_SB_SOLUTION_SERVER:
								edition = "SB Solution Server";
								break;

							case PRODUCT_SB_SOLUTION_SERVER_EM:
								edition = "SB Solution Server EM";
								break;

							case PRODUCT_SERVER_FOR_SB_SOLUTIONS:
								edition = "Server for SB Solutions";
								break;

							case PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM:
								edition = "Server for SB Solutions EM";
								break;

							case PRODUCT_SERVER_FOR_SMALLBUSINESS:
								edition = "Windows Essential Server Solutions";
								break;

							case PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
								edition = "Windows Essential Server Solutions without Hyper-V";
								break;

							case PRODUCT_SERVER_FOUNDATION:
								edition = "Server Foundation";
								break;

							case PRODUCT_SMALLBUSINESS_SERVER:
								edition = "Windows Small Business Server";
								break;

							case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM:
								edition = "Windows Small Business Server Premium";
								break;

							case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE:
								edition = "Windows Small Business Server Premium (core installation)";
								break;

							case PRODUCT_SOLUTION_EMBEDDEDSERVER:
								edition = "Solution Embedded Server";
								break;

							case PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE:
								edition = "Solution Embedded Server (core installation)";
								break;

							case PRODUCT_STANDARD_SERVER:
								edition = "Standard Server";
								break;

							case PRODUCT_STANDARD_SERVER_CORE:
								edition = "Standard Server (core installation)";
								break;

							case PRODUCT_STANDARD_SERVER_SOLUTIONS:
								edition = "Standard Server Solutions";
								break;

							case PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE:
								edition = "Standard Server Solutions (core installation)";
								break;

							case PRODUCT_STANDARD_SERVER_CORE_V:
								edition = "Standard Server without Hyper-V (core installation)";
								break;

							case PRODUCT_STANDARD_SERVER_V:
								edition = "Standard Server without Hyper-V";
								break;

							case PRODUCT_STARTER:
								edition = "Starter";
								break;

							case PRODUCT_STARTER_N:
								edition = "Starter N";
								break;

							case PRODUCT_STARTER_E:
								edition = "Starter E";
								break;

							case PRODUCT_STORAGE_ENTERPRISE_SERVER:
								edition = "Enterprise Storage Server";
								break;

							case PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE:
								edition = "Enterprise Storage Server (core installation)";
								break;

							case PRODUCT_STORAGE_EXPRESS_SERVER:
								edition = "Express Storage Server";
								break;

							case PRODUCT_STORAGE_EXPRESS_SERVER_CORE:
								edition = "Express Storage Server (core installation)";
								break;

							case PRODUCT_STORAGE_STANDARD_SERVER:
								edition = "Standard Storage Server";
								break;

							case PRODUCT_STORAGE_STANDARD_SERVER_CORE:
								edition = "Standard Storage Server (core installation)";
								break;

							case PRODUCT_STORAGE_WORKGROUP_SERVER:
								edition = "Workgroup Storage Server";
								break;

							case PRODUCT_STORAGE_WORKGROUP_SERVER_CORE:
								edition = "Workgroup Storage Server (core installation)";
								break;

							case PRODUCT_UNDEFINED:
								edition = "Unknown product";
								break;

							case PRODUCT_ULTIMATE:
								edition = "Ultimate";
								break;

							case PRODUCT_ULTIMATE_N:
								edition = "Ultimate N";
								break;

							case PRODUCT_ULTIMATE_E:
								edition = "Ultimate E";
								break;

							case PRODUCT_WEB_SERVER:
								edition = "Web Server";
								break;

							case PRODUCT_WEB_SERVER_CORE:
								edition = "Web Server (core installation)";
								break;
							}
						}
					}
				}

				s_Edition = edition;
				return edition;
			}
		}

		/// <summary>
		///     Gets the name of the operating system running on this computer.
		/// </summary>
		public static string Name
		{
			get
			{
				if( s_Name != null )
					return s_Name; //***** RETURN *****//

				string name = "unknown";

				OperatingSystem osVersion = Environment.OSVersion;
				OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX
				{
					dwOSVersionInfoSize = Marshal.SizeOf( typeof(OSVERSIONINFOEX) )
				};

				if( GetVersionEx( ref osVersionInfo ) )
				{
					int majorVersion = osVersion.Version.Major;
					int minorVersion = osVersion.Version.Minor;

					if( majorVersion == 6 && minorVersion == 2 )
					{
						//The registry read workaround is by Scott Vickery. Thanks a lot for the help!

						//http://msdn.microsoft.com/en-us/library/windows/desktop/ms724832(v=vs.85).aspx

						// For applications that have been manifested for Windows 8.1 & Windows 10.
						// Applications not manifested for 8.1 or 10 will return the Windows 8 OS
						// version value (6.2). By reading the registry, we'll get the exact version
						// - meaning we can even compare against Win 8 and Win 8.1.
						string exactVersion = RegistryRead( @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
							"CurrentVersion", "" );
						if( !string.IsNullOrEmpty( exactVersion ) )
						{
							string[] splitResult = exactVersion.Split( '.' );
							majorVersion = Convert.ToInt32( splitResult[0] );
							minorVersion = Convert.ToInt32( splitResult[1] );
						}
						if( IsWindows10() )
						{
							majorVersion = 10;
							minorVersion = 0;
						}
					}

					switch( osVersion.Platform )
					{
					case PlatformID.Win32S:
						name = "Windows 3.1";
						break;

					case PlatformID.WinCE:
						name = "Windows CE";
						break;

					case PlatformID.Win32Windows:
					{
						if( majorVersion == 4 )
						{
							string csdVersion = osVersionInfo.szCSDVersion;
							switch( minorVersion )
							{
							case 0:
								if( csdVersion == "B" || csdVersion == "C" )
									name = "Windows 95 OSR2";
								else
									name = "Windows 95";
								break;

							case 10:
								name = csdVersion == "A"
									? "Windows 98 Second Edition"
									: "Windows 98";
								break;

							case 90:
								name = "Windows Me";
								break;
							}
						}

						break;
					}
					case PlatformID.Win32NT:
					{
						byte productType = osVersionInfo.wProductType;

						switch( majorVersion )
						{
						case 3:
							name = "Windows NT 3.51";
							break;

						case 4:
							switch( productType )
							{
							case 1:
								name = "Windows NT 4.0";
								break;

							case 3:
								name = "Windows NT 4.0 Server";
								break;
							}

							break;

						case 5:
							switch( minorVersion )
							{
							case 0:
								name = "Windows 2000";
								break;

							case 1:
								name = "Windows XP";
								break;

							case 2:
								name = "Windows Server 2003";
								break;
							}

							break;

						case 6:
							switch( minorVersion )
							{
							case 0:
								switch( productType )
								{
								case 1:
									name = "Windows Vista";
									break;

								case 3:
									name = "Windows Server 2008";
									break;
								}

								break;

							case 1:
								switch( productType )
								{
								case 1:
									name = "Windows 7";
									break;

								case 3:
									name = "Windows Server 2008 R2";
									break;
								}

								break;

							case 2:
								switch( productType )
								{
								case 1:
									name = "Windows 8";
									break;

								case 3:
									name = "Windows Server 2012";
									break;
								}

								break;

							case 3:
								switch( productType )
								{
								case 1:
									name = "Windows 8.1";
									break;

								case 3:
									name = "Windows Server 2012 R2";
									break;
								}

								break;
							}

							break;

						case 10:
							switch( minorVersion )
							{
							case 0:
								switch( productType )
								{
								case 1:
									name = "Windows 10";
									break;

								case 3:
									name = "Windows Server 2016";
									break;
								}

								break;
							}

							break;
						}

						break;
					}
					}
				}

				s_Name = name;
				return name;
			}
		}

		public static SoftwareArchitecture OsBits
		{
			get
			{
				SoftwareArchitecture osbits;

				switch( IntPtr.Size * 8 )
				{
				case 64:
					osbits = SoftwareArchitecture.Bit64;
					break;

				case 32:
					osbits = Is32BitProcessOn64BitProcessor()
						? SoftwareArchitecture.Bit64
						: SoftwareArchitecture.Bit32;
					break;

				default:
					osbits = SoftwareArchitecture.Unknown;
					break;
				}

				return osbits;
			}
		}

		/// <summary>
		///     Determines if the current processor is 32 or 64-bit.
		/// </summary>
		public static ProcessorArchitecture ProcessorBits
		{
			get
			{
				ProcessorArchitecture pbits = ProcessorArchitecture.Unknown;

				try
				{
					SYSTEM_INFO lSystemInfo = new SYSTEM_INFO();
					GetNativeSystemInfo( ref lSystemInfo );

					switch( lSystemInfo.uProcessorInfo.wProcessorArchitecture )
					{
					case 9: // PROCESSOR_ARCHITECTURE_AMD64
						pbits = ProcessorArchitecture.Bit64;
						break;

					case 6: // PROCESSOR_ARCHITECTURE_IA64
						pbits = ProcessorArchitecture.Itanium64;
						break;

					case 0: // PROCESSOR_ARCHITECTURE_INTEL
						pbits = ProcessorArchitecture.Bit32;
						break;

					default: // PROCESSOR_ARCHITECTURE_UNKNOWN
						pbits = ProcessorArchitecture.Unknown;
						break;
					}
				}
				catch
				{
					// Ignore
				}

				return pbits;
			}
		}

		/// <summary>
		///     Determines if the current application is 32 or 64-bit.
		/// </summary>
		public static SoftwareArchitecture ProgramBits
		{
			get
			{
				SoftwareArchitecture pbits;

				switch( IntPtr.Size * 8 )
				{
				case 64:
					pbits = SoftwareArchitecture.Bit64;
					break;

				case 32:
					pbits = SoftwareArchitecture.Bit32;
					break;

				default:
					pbits = SoftwareArchitecture.Unknown;
					break;
				}

				return pbits;
			}
		}

		/// <summary>
		///     Gets the service pack information of the operating system running on this computer.
		/// </summary>
		public static string ServicePack
		{
			get
			{
				string servicePack = string.Empty;
				OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX
				{
					dwOSVersionInfoSize = Marshal.SizeOf( typeof(OSVERSIONINFOEX) )
				};

				if( GetVersionEx( ref osVersionInfo ) )
				{
					servicePack = osVersionInfo.szCSDVersion;
				}

				return servicePack;
			}
		}

		public static Version Version => new Version( MajorVersion, MinorVersion, BuildVersion, RevisionVersion );

		/// <summary>
		///     Gets the major version number of the operating system running on this computer.
		/// </summary>
		private static int MajorVersion
		{
			get
			{
				if( IsWindows10() )
				{
					return 10;
				}

				string exactVersion = RegistryRead( @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
					"CurrentVersion", "" );
				if( !string.IsNullOrEmpty( exactVersion ) )
				{
					string[] splitVersion = exactVersion.Split( '.' );
					return int.Parse( splitVersion[0] );
				}

				return Environment.OSVersion.Version.Major;
			}
		}

		/// <summary>
		///     Gets the minor version number of the operating system running on this computer.
		/// </summary>
		private static int MinorVersion
		{
			get
			{
				if( IsWindows10() )
				{
					return 0;
				}

				string exactVersion = RegistryRead( @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
					"CurrentVersion", "" );
				if( !string.IsNullOrEmpty( exactVersion ) )
				{
					string[] splitVersion = exactVersion.Split( '.' );
					return int.Parse( splitVersion[1] );
				}

				return Environment.OSVersion.Version.Minor;
			}
		}

		/// <summary>
		///     Gets the revision version number of the operating system running on this computer.
		/// </summary>
		private static int RevisionVersion
		{
			get
			{
				if( IsWindows10() )
				{
					return 0;
				}

				return Environment.OSVersion.Version.Revision;
			}
		}

		/// <summary>
		///     Gets the build version number of the operating system running on this computer.
		/// </summary>
		private static int BuildVersion
			=>
				int.Parse( RegistryRead( @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber",
					"0" ) );

		private delegate bool IsWow64ProcessDelegate( [In] IntPtr handle, [Out] out bool isWow64Process );

		[StructLayout( LayoutKind.Explicit )]
		public struct _PROCESSOR_INFO_UNION
		{
			[FieldOffset( 0 )]
			internal uint dwOemId;

			[FieldOffset( 0 )]
			internal ushort wProcessorArchitecture;

			[FieldOffset( 2 )]
			internal ushort wReserved;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct SYSTEM_INFO
		{
			internal _PROCESSOR_INFO_UNION uProcessorInfo;
			public uint dwPageSize;
			public IntPtr lpMinimumApplicationAddress;
			public IntPtr lpMaximumApplicationAddress;
			public IntPtr dwActiveProcessorMask;
			public uint dwNumberOfProcessors;
			public uint dwProcessorType;
			public uint dwAllocationGranularity;
			public ushort dwProcessorLevel;
			public ushort dwProcessorRevision;
		}

		[StructLayout( LayoutKind.Sequential )]
		private struct OSVERSIONINFOEX
		{
			public int dwOSVersionInfoSize;
			public readonly int dwMajorVersion;
			public readonly int dwMinorVersion;
			public readonly int dwBuildNumber;
			public readonly int dwPlatformId;

			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 128 )]
			public readonly string szCSDVersion;

			public readonly short wServicePackMajor;
			public readonly short wServicePackMinor;
			public readonly short wSuiteMask;
			public readonly byte wProductType;
			public readonly byte wReserved;
		}
	}
}