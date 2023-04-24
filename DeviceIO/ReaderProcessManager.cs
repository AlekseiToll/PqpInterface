using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using EmServiceLib;

namespace DeviceIO
{
	public enum IndexInBufferFromCpp
	{
		/// <summary>sign that data is valid</summary>
		VALID_DATA = 0,
		/// <summary>1 = no error, 2 = some read error, 3 = disconnect</summary>
		IF_ERROR = 1,
		/// <summary>count of archives to read</summary>
		CNT_ARCHIVES = 2,
		/// <summary>current archive number</summary>
		CUR_ARCHIVE = 3,
		/// <summary>percent of reading current archive</summary>
		PERCENT = 4
	}

	/// <summary>
	/// This enum must be synchronized with the same enum in C++ modul
	/// </summary>
	public enum ErrorFromCpp
	{
		NO_READ_ERROR = 1,
		READ_ERROR = 2,
		DISCONNECT = 3,
		TIMEOUT = 4
	}

	/// <summary>
	/// Class manages the reader program written in C++
	/// </summary>
	public class ReaderProcessManager : DataReaderThread
	{
		private System.Diagnostics.Process processReader_;
		private ReceiverFromCpp receiver_;
		private string pipeName_;

		#region Constructor

		public ReaderProcessManager(EmSettings settings, BackgroundWorker bw, IntPtr hMainWnd, string pipeName)
			: base(hMainWnd)
		{
			settings_ = settings;
			bw_ = bw;
			pipeName_ = pipeName;
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
				e_.Result = ReadDataFromDevice();
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in RPM::Run(): " + emx.Message);
				e_.Result = ExchangeResult.EXCEPTION;
			}
			catch (EmDisconnectException)
			{
				if (!e_.Cancel && !bw_.CancellationPending)
				{
					e_.Result = ExchangeResult.DISCONNECT;
					e_.Cancel = true;
				}
				//Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 3, 0, 0);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in RPM::Run():");
				e_.Result = ExchangeResult.EXCEPTION;
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

		public void KillReader()
		{
			try
			{
				if (processReader_ != null && !processReader_.HasExited)
				{
					processReader_.Kill();
					EmService.WriteToLogGeneral("KillReader: process killed");
				}
				else EmService.WriteToLogGeneral("KillReader else");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in RPM::KillReader():");
				//throw;
			}
		}

		protected override ExchangeResult ReadDataFromDevice()
		{
			try
			{
				// start reader
				processReader_ = new System.Diagnostics.Process();
				processReader_.StartInfo.FileName = "ReaderProc.exe";
				//processReader_.StartInfo.Arguments = String.Format("\"{0}\"", out_fn);
				processReader_.StartInfo.WorkingDirectory = EmService.AppDirectory;
				processReader_.StartInfo.UseShellExecute = false;
				processReader_.StartInfo.CreateNoWindow = true;
				processReader_.StartInfo.RedirectStandardOutput = false;
				processReader_.Start();

				receiver_ = new ReceiverFromCpp(ref bw_, ref e_, ref processReader_);

				if (!receiver_.Connect(pipeName_))
				{
					EmService.WriteToLogFailed("Unable to connect to pipe!");
					return ExchangeResult.DISCONNECT;
				}

				return receiver_.BeginReceive();
			}
			catch (EmDeviceEmptyException)
			{
				throw;
			}
			catch (ThreadAbortException)
			{
				EmService.WriteToLogFailed("ThreadAbortException in RPM::ReadDataFromDevice()");
				Thread.ResetAbort();
				return ExchangeResult.EXCEPTION;
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("EmException in RPM::ReadDataFromDevice()" + emx.Message);
				return ExchangeResult.EXCEPTION;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in RPM::ReadDataFromDevice():");
				//frmSentLogs frmLogs = new frmSentLogs();
				//frmLogs.ShowDialog();
				//throw;
				return ExchangeResult.EXCEPTION;
			}
			finally
			{
				Thread.Sleep(3000);
				try
				{
					EmService.WriteToLogGeneral("KillReader: process killed in finally");
					if(processReader_  != null && !processReader_.HasExited)
						processReader_.Kill();
				}
				catch (Exception exx)
				{
					EmService.DumpException(exx, "Error in RPM::ReadDataFromDevice() finally:");
				}

				if(receiver_ != null) receiver_.Dispose();
			}
		}
	}

	public class ReceiverFromCpp : IDisposable
	{
		private System.Diagnostics.Process processReader_;

		private AutoResetEvent eventSuccess_ = new AutoResetEvent(false);
		private AutoResetEvent eventError_ = new AutoResetEvent(false);
		private AutoResetEvent eventTimeout_ = new AutoResetEvent(false);
		private WaitHandle[] waitHandles_ = new WaitHandle[3];

		private NamedPipeServerStream pipe_;
		public const int BufferLength = 10;
		private byte[] pipeBuffer_ = new byte[BufferLength];

		private BackgroundWorker bw_ = null;
		private DoWorkEventArgs e_;
		// timer alarms if there was no respond from reader for too long time
		private System.Timers.Timer timer_ = new System.Timers.Timer();

		public ReceiverFromCpp(ref BackgroundWorker bw, ref DoWorkEventArgs e, 
			ref System.Diagnostics.Process processReader)
		{
			bw_ = bw;
			e_ = e;
			processReader_ = processReader;

			timer_.Elapsed += new ElapsedEventHandler(TimerFunc);
			timer_.AutoReset = false;		// if true timer alarms many times, if false - only one time

			waitHandles_[0] = eventSuccess_;
			waitHandles_[1] = eventTimeout_;
			waitHandles_[2] = eventError_;
		}

		public bool Connect(string pipeName)
		{
			try
			{
				timer_.Interval = 20000;		// 20 sec

				pipe_ = new NamedPipeServerStream(pipeName, PipeDirection.InOut,
								NamedPipeServerStream.MaxAllowedServerInstances,
								PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				for (int iAttempt = 0; iAttempt < 2; ++iAttempt)
				{
					timer_.Start();
					pipe_.BeginWaitForConnection(AsyncConnect, pipe_);

					switch (WaitHandle.WaitAny(waitHandles_))
					{
						case 0:
							return true; // ok
						case 1: EmService.WriteToLogFailed("Error while connecting to pipe 1");
							Thread.Sleep(5000); continue; //return false; // timeout
						case 2: EmService.WriteToLogFailed("Error while connecting to pipe 2");
							Thread.Sleep(5000); continue; //return false; // error
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReceiverFromCpp::Connect():");
				return false;
			}
		}

		private void AsyncConnect(IAsyncResult iar)
		{
			try
			{
				pipe_.EndWaitForConnection(iar);
				if (pipe_.IsConnected)
				{
					eventSuccess_.Set();
					EmService.WriteToLogGeneral("Pipe is connected");
				}
				else
				{
					eventError_.Set();
					EmService.WriteToLogGeneral("Pipe is NOT connected");
				}
				timer_.Stop();
			}
			catch (ObjectDisposedException)
			{
				EmService.WriteToLogFailed("ObjectDisposedException in ReceiverFromCpp::AsyncConnect");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReceiverFromCpp::AsyncConnect():");
			}
		}

		private void AsyncRead(IAsyncResult iar)
		{
			try
			{
				int cntReadBytes = pipe_.EndRead(iar);

				if (cntReadBytes < 10 || pipeBuffer_ == null)
				{
					eventError_.Set();
					return;
				}
				// "5" is an agreed byte which means that data in the buffer is valid
				if (pipeBuffer_.Length < BufferLength || pipeBuffer_[(int) IndexInBufferFromCpp.VALID_DATA] != 5)
				{
					eventError_.Set();
					return;
				}

				eventSuccess_.Set();
				// if we've received valid data, reset timer
				timer_.Stop();
				timer_.Start();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReceiverFromCpp::AsyncRead():");
			}
		}

		public ExchangeResult BeginReceive()
		{
			try
			{
				timer_.Interval = 30000;		// 30 sec
				timer_.Start();

				int cntError = 0;

				while (true)
				{
					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return ExchangeResult.CANCELLED;
					}
					if (e_.Cancel) break;

					if (processReader_.HasExited)
					{
						EmService.WriteToLogFailed("ProcessReader HasExited in ReceiverFromCpp");
						return ExchangeResult.CANCELLED;
					}

					if (cntError > 5)
					{
						EmService.WriteToLogFailed("Too many errors in ReceiverFromCpp");
						return ExchangeResult.READ_ERROR;
					}

					try
					{
						//pipeBuffer_[0] - sign that data is valid
						//pipeBuffer_[1] - 1 = no reading error, 2 = some error
						//pipeBuffer_[2] - count of archives to read
						//pipeBuffer_[3] - current archive number
						//pipeBuffer_[4] - percent of reading current archive

						//pipe.BeginRead(pipeBuffer, 0, pipeBuffer.Length,
						//     new AsyncCallback(
						//        result => System.Diagnostics.Debug.WriteLine(
						//            "Pipe: " + (DateTime.Now - dt).TotalMilliseconds + " мс")), null);
						for (int iItem = 0; iItem < BufferLength; ++iItem)
							pipeBuffer_[iItem] = 0;
						pipe_.BeginRead(pipeBuffer_, 0, BufferLength, AsyncRead, null);

						switch (WaitHandle.WaitAny(waitHandles_))
						{
							case 0:		// normal read event
								cntError = 0;
								if (pipeBuffer_[(int)IndexInBufferFromCpp.IF_ERROR] == (int)ErrorFromCpp.NO_READ_ERROR)
								{
									if (pipeBuffer_[(int)IndexInBufferFromCpp.PERCENT] == 100 &&
										pipeBuffer_[(int)IndexInBufferFromCpp.CNT_ARCHIVES] ==
										pipeBuffer_[(int)IndexInBufferFromCpp.CUR_ARCHIVE])
									{
										return ExchangeResult.OK; // if 100% then finish
									}
									else
									{
										// set progressbar
										int numberToSend = 0;
										numberToSend |= pipeBuffer_[(int)IndexInBufferFromCpp.CNT_ARCHIVES];
										numberToSend |= (pipeBuffer_[(int)IndexInBufferFromCpp.CUR_ARCHIVE] << 8);
										numberToSend |= (pipeBuffer_[(int)IndexInBufferFromCpp.PERCENT] << 16);
										bw_.ReportProgress(numberToSend);
									}
								}
								else if (pipeBuffer_[(int)IndexInBufferFromCpp.IF_ERROR] == (int)ErrorFromCpp.READ_ERROR)
								{
									EmService.WriteToLogFailed("Some error from Reader");
									return ExchangeResult.READ_ERROR;
								}
								else if (pipeBuffer_[(int)IndexInBufferFromCpp.IF_ERROR] == (int)ErrorFromCpp.DISCONNECT)
								{
									EmService.WriteToLogFailed("Disconnect from Reader");
									return ExchangeResult.DISCONNECT;
								}
								else if (pipeBuffer_[(int)IndexInBufferFromCpp.IF_ERROR] == (int)ErrorFromCpp.TIMEOUT)
								{
									EmService.WriteToLogFailed("Timeout from Reader");
									return ExchangeResult.TIMEOUT;
								}

								break;

							case 1:
								EmService.WriteToLogFailed("Timeout 1 in ReceiverFromCpp");
								if (cntError <= 1)
								{
									cntError++;
									timer_.Start();  // one additional attempt
									continue;
								}
								return ExchangeResult.TIMEOUT;	// timeout event

							case 2: // some error
								EmService.WriteToLogFailed("Error 2 in ReceiverFromCpp");
								cntError++;
								Thread.Sleep(100);
								continue;
						}
					}
					catch (InvalidOperationException)
					{
						EmService.WriteToLogFailed("InvalidOperationException in ReceiverFromCpp");
					}
				}
				return (ExchangeResult) e_.Result;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReceiverFromCpp::BeginReceive():");
				return ExchangeResult.EXCEPTION;
			}
			finally
			{
				if(pipe_.IsConnected) pipe_.Disconnect();
				pipe_.Close();
				EmService.WriteToLogGeneral("Pipe was disconnected and closed");
			}
		}

		public void Dispose()
		{
			if (pipe_.IsConnected) pipe_.Disconnect();
			pipe_.Close();
			pipe_.Dispose();

			timer_.Stop();
			timer_.Dispose();
			EmService.WriteToLogGeneral("ReceiverFromCpp::Dispose");
		}

		private void TimerFunc(object obj, EventArgs e)
		{
			EmService.WriteToLogFailed("TimerFunc was called");
			eventTimeout_.Set();

			//if (!e_.Cancel && !bw_.CancellationPending)
			//{
			//	EmService.WriteToLogFailed("Timeout in RPM::TimerFunc()");
			//	e_.Result = ExchangeResult.TIMEOUT;
			//	e_.Cancel = true;
			//}
		}

		//private Task<int> ReadFromPipe()
		//{
		//	var result = new TaskFactory<int>().StartNew(() =>
		//	{
		//		//pipe.BeginRead(pipeBuffer, 0, pipeBuffer.Length,
		//		//     new AsyncCallback(
		//		//        result => System.Diagnostics.Debug.WriteLine(
		//		//            "Pipe: " + (DateTime.Now - dt).TotalMilliseconds + " мс")), null);
		//		int cntReadBytes = pipe_.Read(pipeBuffer_, 0, BufferLength);
		//		return cntReadBytes;
		//	});
		//	return result;
		//}
	}
}
