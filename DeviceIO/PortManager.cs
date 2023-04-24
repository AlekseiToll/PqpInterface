using System;
using System.Collections.Generic;
using System.Threading;
using DeviceIOEmPort;
using EmServiceLib;

namespace DeviceIO
{
	public class PortManager
	{
		private EmPort portA_;

		private IntPtr hMainWnd_;
		private EmPortType portType_;

		private string wifiProfileName_;
		private string wifiPassword_;

		private object[] portParams_;

		public PortManager(IntPtr hMainWnd, EmPortType portType,
							ref object[] portParams)
		{
			hMainWnd_ = hMainWnd;
			portType_ = portType;
			portParams_ = portParams;
		}

		public PortManager(IntPtr hMainWnd, EmPortType portType,
								string wifiProfileName, string wifiPassword,
								ref object[] portParams)
		{
			hMainWnd_ = hMainWnd;
			portType_ = portType;
			portParams_ = portParams;
			wifiProfileName_ = wifiProfileName;
			wifiPassword_ = wifiPassword;
		}

		public ExchangeResult WriteData(EmCommands command, ref byte[] buffer)
		{
			return ((EtPqpAUSB)portA_).WriteData((ushort)command, ref buffer);
		}

		public ExchangeResult ReadData(EmCommands command, ref byte[] buffer, List<UInt32> listParams)
		{
			return ((EtPqpAUSB)portA_).ReadData((ushort)command, ref buffer, listParams);
		}

		//public static bool ConnectToWifi()
		//{
		//    try
		//    {
		//        if (!Wlan.IsWifiConnected(false, wifiProfileName_))
		//        {
		//            WlanClient.WlanInterface wlanIface = Wlan.ConnectWifiEtpqpA(wifiProfileName_,
		//                                                                        wifiPassword_);

		//            if (!Wlan.IsWifiConnected(true, wlanIface, wifiProfileName_))
		//            {
		//                EmService.WriteToLogFailed("Wi-fi not connected!");
		//                return false;
		//            }
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "Exception in ConnectToWifi() WI-FI:");
		//        return false;
		//    }
		//}

		public bool CreatePort()
		{
			try
			{
				//ushort devAddress = 0xFFFF;
				//List<string> cparams = null;

				switch (portType_)
				{
					case EmPortType.USB:
						portA_ = new EtPqpAUSB(0xFFFF, hMainWnd_);
						break;

					case EmPortType.WI_FI:
						string address;
						uint port;
						try
						{
							address = (string)portParams_[0];
							port = (uint)(int)portParams_[1];
						}
						catch
						{
							return false;
						}
						if (address == string.Empty || port == 0) return false;
						System.Net.IPAddress ipAddress = System.Net.IPAddress.Parse(address);
						byte[] arrayAddr = ipAddress.GetAddressBytes();

						List<uint> iparams = new List<uint>(6);
						for (int i = 0; i < 4; ++i)
							iparams.Add((uint)arrayAddr[i]);
						// add 1 to the last digit (specially for wi-fi)
						iparams[3] += 1;

						iparams.Add(port);
						//port_ = new EmPortWrapperManaged((int)devType_, (int)portType_, iparams, cparams,
						//								 (int)hMainWnd_);
						break;
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in PortManager::CreatePort():");
				throw;
			}
		}

		public bool OpenPort()
		{
			bool res = portA_.Open();
			//if (res) Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 2, 0, 0);
			//else Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 3, 0, 0);
			return res;
		}

		/// <summary>
		/// For EtPQP-A and USB only
		/// </summary>
		public bool OpenFast(bool bNeedClose)
		{
			try
			{
				if (bNeedClose)
				{
					ClosePort(false);
					Thread.Sleep(2000);
				}

				if (!CreatePort()) return false;

				if (!portA_.Open()) return false;

				//Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 2, 0, 0);
				return true;
			}
			catch (EmDisconnectException)
			{
				ClosePort(true);
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in OpenFast EtPQP-A device:");
				throw;
			}
		}

		public bool ClosePort(bool bNeedDispose)
		{
			//Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 3, 0, 0);
			if (portA_ == null) return false;

			try
			{
				if (!portA_.Close()) return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ClosePort() 1:");
				//throw;
			}

			if (!bNeedDispose) return true;

			try
			{
				//if (portA_ != null) portA_.Dispose();
				portA_ = null;
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ClosePort() 2:");
				portA_ = null;
				throw;
			}
		}
	}
}
