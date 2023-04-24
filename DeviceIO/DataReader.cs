using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using EmServiceLib;
using Microsoft.Win32;

namespace DeviceIO
{
	public abstract class DataReaderThread
	{
		#region Fields

		protected IntPtr hMainWnd_;

		//protected object sender_;
		//protected IntPtr sender_;
		protected EmSettings settings_;
		protected BackgroundWorker bw_ = null;
		protected DoWorkEventArgs e_;

		// количество страниц, которые надо читать (инфа для ProgressBar)
		//protected double cnt_pages_to_read_ = 0.0;		// 100%
		//protected double reader_cur_percent_ = 0;		// current percent
		//protected int reader_prev_percent_ = 0;
		//protected double reader_percent_for_one_step_ = 0;

		#endregion

		#region Constructor

		public DataReaderThread(IntPtr hMainWnd)
		{
			hMainWnd_ = hMainWnd;
		}

		#endregion

		#region Main methods

		/// <summary>
		/// Main saving function - start reading process
		/// </summary>
		public abstract void Run(ref DoWorkEventArgs e);

		protected abstract ExchangeResult ReadDataFromDevice();

		//public abstract void SetCancelReading();

		#endregion

		#region Protected Methods

		// проверяем соответствует ли время на компе или приборе, если разница больше минуты,
		// выдаем предупреждение
		//protected void DoTimeSynchronizationSLIP(ref byte[] bufTime, DateTime curCompDateTime)
		//{
		//    try
		//    {
		//        // сначала проверяем включена ли на компе синхронизация с инетом
		//        settings_.LoadSettings();

		//        if (!settings_.DontWarnAutoSynchroTimeDisabled)
		//        {
		//            const string keyName = @"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\services\W32Time\Parameters";
		//            object obj = Registry.GetValue(keyName, "Type", null);
		//            if (obj.ToString().Equals("NoSync"))
		//            {
		//                frmSynchroTimeWarning frmTime = new frmSynchroTimeWarning();
		//                frmTime.ShowDialog();
		//                if (settings_.DontWarnAutoSynchroTimeDisabled != !frmTime.ShowThisMessage)
		//                {
		//                    settings_.DontWarnAutoSynchroTimeDisabled = !frmTime.ShowThisMessage;
		//                    settings_.SaveSettings();
		//                }
		//            }
		//        }
		//        // then compare times
		//        DateTime curDeviceDateTime = Conversions.bytes_2_DateTimeSLIP2(ref bufTime, 0);
		//        TimeSpan diff = curCompDateTime - curDeviceDateTime;
		//        if (diff > new TimeSpan(0, 5, 0) || diff < new TimeSpan(0, -5, 0))
		//        {
		//            MessageBoxes.NotCorrectTime(sender_ as Form, this);
		//        }
		//    }
		//    catch (Exception timeEx)
		//    {
		//        EmService.DumpException(timeEx, "ReaderBase: Time synchronization error:");
		//    }
		//}

		//protected bool DeviceLicenceCheck(long serial)
		//{
		//    try
		//    {
		//        if (!settings_.Licences.IsLicenced(serial))
		//        {
		//            EmService.WriteToLogFailed("not licenced");
		//            return false;
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "Error in DeviceLicenceCheck(): ");
		//        return false;
		//    }
		//    return true;
		//}

		//protected bool ReaderReportProgress(double step)
		//{
		//	try
		//	{
		//		//reader_cur_percent_ += reader_percent_for_one_step_ * step;
		//		//int diff = (int)reader_cur_percent_ - reader_prev_percent_;
		//		//if (diff > 0 && bw_ != null)
		//		//    bw_.ReportProgress(diff);

		//		//reader_prev_percent_ = (int)reader_cur_percent_;
		//	}
		//	catch (Exception ex)
		//	{
		//		EmService.DumpException(ex, "Error in ReaderReportProgress(): ");
		//		throw;
		//	}
		//	return true;
		//}

		#endregion
	}

	public class EtDataReader : DataReaderThread
	{
		#region Events

		public delegate void SetCntArchivesHandler(int totalArchives, int curArchive);
		public event SetCntArchivesHandler OnSetCntArchives;

		public delegate void StartProgressBarHandler(double reader_percent_for_one_step);
		public event StartProgressBarHandler OnStartProgressBar;

		#endregion

		#region Fields

		//private DeviceCommonInfo devInfo_;
		private EtDevice device_;

		#endregion

		#region Properties

		public DeviceCommonInfo DeviceInfo
		{
			get { return device_.DeviceInfo; }
		}

		#endregion

		#region Constructor

		public EtDataReader(EmSettings settings, BackgroundWorker bw, IntPtr hMainWnd)
			: base(hMainWnd)
		{
			this.settings_ = settings;
			this.bw_ = bw;
		}

		#endregion

		/// <summary>
		/// Main saving function - start reading process
		/// </summary>
		public override void Run(ref DoWorkEventArgs e)
		{
			try
			{
				e_ = e;
				e.Result = ReadDataFromDevice();
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in EtDataReader::Run(): " + emx.Message);
				e.Result = ExchangeResult.EXCEPTION;
			}
			catch (EmDeviceEmptyException)
			{
				if (!e_.Cancel && !bw_.CancellationPending)
				{
					e.Result = ExchangeResult.DEVICE_EMPTY_EXCEPTION;
					e.Cancel = true;
				}
			}
			catch (EmDisconnectException)
			{
				if (!e_.Cancel && !bw_.CancellationPending)
				{
					e.Result = ExchangeResult.DISCONNECT;
					e.Cancel = true;
				}
				//Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 3, 0, 0);
			}
			catch (EmDeviceOldVersionException)
			{
				if (!e_.Cancel && !bw_.CancellationPending)
				{
					e.Result = ExchangeResult.DEVICE_OLD_VERSION_EXCEPTION;
					e.Cancel = true;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtDataReader::Run():");
				e.Result = ExchangeResult.EXCEPTION;
				throw;
			}
			finally
			{
				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
				}
			}
		}

		protected override ExchangeResult ReadDataFromDevice()
		{
			try
			{
				settings_.LoadSettings();

				object[] port_params = null;
				EmPortType curInterface;

				curInterface = settings_.IOInterface;

				#region Not used so far

				//if (curInterface == EmPortType.USB)
				//{

				//}
				//else if (curInterface == EmPortType.Ethernet)
				//{
				//    port_params = new object[2];
				//    port_params[0] = settings_.CurrentIPAddress;
				//    port_params[1] = settings_.CurrentPort;
				//}
				//else if (curInterface == EmPortType.WI_FI)
				//{
				//    try
				//    {
				//        if (!Wlan.IsWifiConnected(false, settings_.CurWifiProfileName))
				//        {
				//            WlanClient.WlanInterface wlanIface = Wlan.ConnectWifiEtpqpA(settings_.CurWifiProfileName,
				//                                                                        settings_.WifiPassword);

				//            if (!Wlan.IsWifiConnected(true, wlanIface, settings_.CurWifiProfileName))
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

				//    port_params = new object[2];
				//    port_params[0] = EmService.GetCurrentDhcpIpAddress();
				//    port_params[1] = settings_.CurrentPort;
				//}
				//else
				//{
				//    throw new EmInvalidInterfaceException();
				//}

				//#region Write debug info

				//string debugInfo = DateTime.Now.ToString() + "   create device: ETPQP-A " +
				//                curInterface.ToString() + "  ";
				//debugInfo += "{not auto mode} ";
				//if (curInterface == EmPortType.WI_FI)
				//{
				//    debugInfo += string.Format(" Wi-fi: {0}, {1}, {2}", settings_.CurWifiProfileName,
				//        settings_.WifiPassword, EmService.GetCurrentDhcpIpAddress());
				//}
				//EmService.WriteToLogGeneral(debugInfo);

				//#endregion

				//if (bw_.CancellationPending)
				//{
				//    e_.Cancel = true;
				//    return false;
				//}

				#endregion

				long serial = -1;
				Thread.Sleep(500);
				device_ = new EtDevice(curInterface, port_params, settings_.CurWifiProfileName, settings_.WifiPassword,
				                       hMainWnd_, ref bw_, ref e_);
				serial = device_.OpenDevice();
				if (serial == -1)
				{
					throw new EmDisconnectException();
				}
				bw_.ReportProgress(1);
				device_.SerialNumber = serial;

				#region Time Synchro & firmware

				// проверяем соответствует ли время на компе или приборе, если разница больше минуты,
				// выдаем предупреждение
				//try
				//{
				//    byte[] bufTime = null;
				//    device_.ReadTime(ref bufTime);
				//    DateTime res = Conversions.bytes_2_DateTimeSLIP2(ref bufTime, 0);
				//    DoTimeSynchronizationSLIP(ref bufTime, DateTime.Now);
				//}
				//catch (Exception timeEx)
				//{
				//    EmService.DumpException(timeEx, "Time synchronization error:");
				//}

				// проверяем на сайте есть ли новая прошивка для прибора
				//try
				//{
				//    if (settings_.CheckFirmwareEtPQP_A)
				//    {
				//        FTPClient.FTPMarsClient ftp = new FTPClient.FTPMarsClient();
				//        DateTime dtFtp = ftp.FtpGetFirmwareVersion();
				//        EmService.WriteToLogDebug("Date FTP: " + dtFtp.ToString());
				//        EmService.WriteToLogDebug("Date Dev: " + device_.DeviceInfo.DevVersionDate.ToString());
				//        if (dtFtp > device_.DeviceInfo.DevVersionDate)
				//        {
				//            frmNewFirmware frm = new frmNewFirmware();
				//            frm.ShowDialog();

				//            //settings_.LoadSettings();
				//            if (settings_.CheckFirmwareEtPQP_A != frm.ShowThisMessage)
				//            {
				//                settings_.CheckFirmwareEtPQP_A = frm.ShowThisMessage;
				//                //settings_.SaveSettings();
				//            }
				//        }
				//    }
				//}
				//catch (Exception ftpEx)
				//{
				//    EmService.DumpException(ftpEx, "Error in FTP code:");
				//}

				#endregion

				// считываем данные об арихвах
				ExchangeResult errCode = device_.ReadDeviceInfo();

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return ExchangeResult.CANCELLED;
				}

				// if reading device contents was not successfull
				if (errCode != ExchangeResult.OK)
				{
					throw new EmException("EDR::Unable to read device contents");
				}

				if (!device_.IsSomeArchiveExist())
				{
					throw new EmDeviceEmptyException();
				}

				//devInfo_ = device_.DeviceInfo;

				return ExchangeResult.OK;
			}
			catch (EmDeviceEmptyException)
			{
				throw;
			}
			catch (ThreadAbortException)
			{
				EmService.WriteToLogFailed("ThreadAbortException in EDR::ReadDataFromDevice()");
				Thread.ResetAbort();
				return ExchangeResult.EXCEPTION;
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("EmException in EDR::ReadDataFromDevice()" + emx.Message);
				return ExchangeResult.EXCEPTION;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EDR::ReadDataFromDevice():");
				//frmSentLogs frmLogs = new frmSentLogs();
				//frmLogs.ShowDialog();
				//throw;
				return ExchangeResult.EXCEPTION;
			}
			finally
			{
				//settings_.SaveSettings();

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
				}

				if (device_ != null) DisconnectDevice();
			}
		}

		//public override void SetCancelReading()
		//{
		//    device_.BCancelReading = true;
		//}

		public bool DisconnectDevice()
		{
			try
			{
				EmService.WriteToLogGeneral("DisconnectDevice entry");

				if (device_ == null) return false;

				return device_.Close();
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error DisconnectDevice(): " + e.Message);
				return false;
			}
		}

		//private void saver_OnStepReading()
		//{
		//	try
		//	{
		//		// set ProgressBar position
		//		ReaderReportProgress(1.0);
		//	}
		//	catch (Exception ex)
		//	{
		//		EmService.WriteToLogFailed("Error in saver_OnStepReading(): " + ex.Message);
		//	}
		//}
	}
}
