using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using EmServiceLib;

namespace FileAnalyzerLib
{
	/// <summary>
	/// Class contains info about archives stored on the computer
	/// </summary>
	public class StoredArchivesInfo
	{
		private EmSettings settings_;
		private List<RegistrationInfo> listRegistrations_ = new List<RegistrationInfo>();
		private BackgroundWorker bw_ = null;
		protected DoWorkEventArgs e_;

		private List<string> listPqpFiles_ = new List<string>();
		private List<string> listAvgFiles_ = new List<string>();
		private List<string> listDnsFiles_ = new List<string>();

		public StoredArchivesInfo(ref EmSettings settings)
		{
			settings_ = settings;
		}

		public long[] GetDeviceSerialNumbers()
		{
			List<long> listDevSerialNumbers = new List<long>();
			foreach (var reg in listRegistrations_)
			{
				if (!listDevSerialNumbers.Contains(reg.SerialNumber))
					listDevSerialNumbers.Add(reg.SerialNumber);
			}
			return listDevSerialNumbers.ToArray();
		}

		public RegistrationInfo[] GetRegistrations(long serialNumber)
		{
			List<RegistrationInfo> listRegistrations = new List<RegistrationInfo>();
			foreach (var reg in listRegistrations_)
			{
				if (reg.SerialNumber == serialNumber)
					listRegistrations.Add(reg);
			}
			return listRegistrations.ToArray();
		}

		public RegistrationInfo GetRegistration(long serialNumber, long regId)
		{
			foreach (var reg in listRegistrations_)
			{
				if (reg.SerialNumber == serialNumber && reg.RegId == regId)
					return reg;
			}
			return null;
		}

		public RegistrationInfo[] GetRegistrationsForDate(DateTime date)
		{
			List<RegistrationInfo> listRegistrations = new List<RegistrationInfo>();
			foreach (var reg in listRegistrations_)
			{
				if (reg.DtStart.Date == date.Date ||
					reg.DtEnd.Date == date.Date ||
					(reg.DtStart < date.Date && reg.DtEnd > date))
					listRegistrations.Add(reg);
			}
			return listRegistrations.ToArray();
		}

		public bool IsPqpArchiveExists(long serialNumber, uint regId, uint pqpId)
		{
			RegistrationInfo reg = GetRegistration(serialNumber, regId);
			if (reg == null) return false;

			for (int iPqpStored = 0; iPqpStored < reg.PqpArchives.Count; iPqpStored++)
			{
				if (reg.PqpArchives[iPqpStored].Id == pqpId)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsAvgArchiveExists(long serialNumber, uint regId, AvgTypes type)
		{
			RegistrationInfo reg = GetRegistration(serialNumber, regId);
			if (reg == null) return false;

			switch (type)
			{
				case AvgTypes.ThreeSec: return reg.AvgArchive3Sec != null;
				case AvgTypes.TenMin: return reg.AvgArchive10Min != null;
				case AvgTypes.TwoHours: return reg.AvgArchive2Hour != null;
			}
			return false;
		}

		public bool IsDnsArchiveExists(long serialNumber, uint regId)
		{
			RegistrationInfo reg = GetRegistration(serialNumber, regId);
			if (reg == null) return false;

			return reg.DnsArchive != null;
		}

		public void SetBackgroundWorker(ref BackgroundWorker bw)
		{
			bw_ = bw;
		}

		public bool LoadArchivesInfo(ref DoWorkEventArgs e)
		{
			try
			{
				e_ = e;
				listPqpFiles_.Clear();
				listAvgFiles_.Clear();
				listDnsFiles_.Clear();
				listRegistrations_.Clear();

				settings_.LoadSettings();
				// if there is no user directories then we will search on all disks
				if (settings_.PathToStoredArchives.Length == 0)
				{
					List<string> listDirs = new List<string>();
					DriveInfo[] allDrives = DriveInfo.GetDrives();
					for (int iDisk = 0; iDisk < allDrives.Length; ++iDisk)
					{
						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							e_.Result = false;
							return false;
						}

						if (allDrives[iDisk].IsReady)
						{
							EmService.SearchAdditionalDirs(allDrives[iDisk].RootDirectory.Name,
									   new string[] { "*.pqp", "*.avg", "*.dns" },
									   ref listDirs, ref e,
									   ref bw_,
									   settings_.PathToStoredArchives);
						}
						else
						{
							EmService.WriteToLogFailed("Disk is not ready: " + allDrives[iDisk].Name);
						}
						bw_.ReportProgress((iDisk + 1) * 100 / allDrives.Length);
					}

					settings_.PathToStoredArchives = listDirs.ToArray();
					settings_.SaveSettings();
				}

				// unused directories which we can remove from the settings
				List<string> listUnusedDirectories = new List<string>();

				for (int iPath = 0; iPath < settings_.PathToStoredArchives.Length; ++iPath)
				{
					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						e_.Result = false;
						return false;
					}

					string path = settings_.PathToStoredArchives[iPath];
					if (Directory.Exists(path))
					{
						// search files
						SearchIn(path);

						// if there is no archive files
						if (listPqpFiles_.Count == 0 && listAvgFiles_.Count == 0 && listDnsFiles_.Count == 0)
						{
							listUnusedDirectories.Add(path);
							continue;
						}

						// if files exist, analyze them
						if (listPqpFiles_.Count > 0) LoadInfoAboutPqpArchives();
						if (listAvgFiles_.Count > 0) LoadInfoAboutAvgArchives();
						if (listDnsFiles_.Count > 0) LoadInfoAboutDnsArchives();
					}
					else listUnusedDirectories.Add(path);

					bw_.ReportProgress(
						(iPath + 1) * 100 / settings_.PathToStoredArchives.Length);
				}

				// remove directories in which there is no archive file
				if (listUnusedDirectories.Count > 0)
				{
					List<string> listNewPaths = new List<string>();
					foreach (var path in settings_.PathToStoredArchives)
					{
						if (!listUnusedDirectories.Contains(path))
						{
							listNewPaths.Add(path);
						}
						else EmService.WriteToLogGeneral("Unused dir was removed: " + path);
					}
					settings_.PathToStoredArchives = listNewPaths.ToArray();
					settings_.SaveSettings();
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in LoadArchivesInfo:");
				return false;
			}
		}

		private void LoadInfoAboutAvgArchives()
		{
			try
			{
				for (int iFile = 0; iFile < listAvgFiles_.Count; ++iFile)
				{
					try
					{
						RegistrationInfo regInfo;
						AvgArchiveInfo avgArchiveInfo;
						if (!FileAnalyzer.GetInfoAboutAvgArchive(listAvgFiles_[iFile], out regInfo,
																out avgArchiveInfo))
							continue;

						int regIndex = FindRegistrationInList(regInfo.SerialNumber, regInfo.RegId);
						if (regIndex == -1)
						{
							listRegistrations_.Add(regInfo);
						}
						else
						{
							listRegistrations_[regIndex].SetAvgInfo(avgArchiveInfo);
						}
					}
					catch
					{
						EmService.WriteToLogFailed("LoadInfoAboutAvgArchives: invalid file " +
							listAvgFiles_[iFile]);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in LoadInfoAboutAvgArchives:");
				throw;
			}
		}

		private void LoadInfoAboutPqpArchives()
		{
			try
			{
				for (int iFile = 0; iFile < listPqpFiles_.Count; ++iFile)
				{
					try
					{
						RegistrationInfo regInfo;
						PqpArchiveInfo pqpArchiveInfo;
						if (!FileAnalyzer.GetInfoAboutPqpArchive(listPqpFiles_[iFile], out regInfo,
																out pqpArchiveInfo))
							continue;

						int regIndex = FindRegistrationInList(regInfo.SerialNumber, regInfo.RegId);
						if (regIndex == -1)
						{
							listRegistrations_.Add(regInfo);
						}
						else
						{
							if (!listRegistrations_[regIndex].IfPqpArchiveExists(pqpArchiveInfo.Id))
								listRegistrations_[regIndex].PqpArchives.Add(pqpArchiveInfo);
						}
					}
					catch
					{
						EmService.WriteToLogFailed("LoadInfoAboutPqpArchives: invalid file " +
							listPqpFiles_[iFile]);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in LoadInfoAboutPqpArchives:");
				throw;
			}
		}

		private void LoadInfoAboutDnsArchives()
		{
			try
			{
				for (int iFile = 0; iFile < listDnsFiles_.Count; ++iFile)
				{
					try
					{
						RegistrationInfo regInfo;
						DnsArchiveInfo dnsArchiveInfo;
						if (!FileAnalyzer.GetInfoAboutDnsArchive(listDnsFiles_[iFile], out regInfo,
																out dnsArchiveInfo))
							continue;

						int regIndex = FindRegistrationInList(regInfo.SerialNumber, regInfo.RegId);
						if (regIndex == -1)
						{
							listRegistrations_.Add(regInfo);
						}
						else
						{
							listRegistrations_[regIndex].DnsArchive = dnsArchiveInfo;
						}
					}
					catch
					{
						EmService.WriteToLogFailed("LoadInfoAboutDnsArchives: invalid file " +
							listDnsFiles_[iFile]);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in LoadInfoAboutDnsArchives:");
				throw;
			}
		}

		private int FindRegistrationInList(long serialNumber, long regId)
		{
			try
			{
				for (int iItem = 0; iItem < listRegistrations_.Count; ++iItem)
				{
					if (listRegistrations_[iItem].SerialNumber == serialNumber &&
						listRegistrations_[iItem].RegId == regId)
						return iItem;
				}
				return -1;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FindRegistrationInList:");
				throw;
			}
		}

		#region Service methods

		// the method was found here https://msdn.microsoft.com/en-us/library/bb513869.aspx
		private void SearchIn(string path)
		{
			// Data structure to hold names of subfolders to be examined for files
			Stack<string> dirs = new Stack<string>(20);

			if (!Directory.Exists(path))
			{
				EmService.WriteToLogFailed("SearchIn: Directory doesn't exist: " + path);
				return;
			}
			dirs.Push(path);

			while (dirs.Count > 0)
			{
				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					e_.Result = false;
					return;
				}

				string currentDir = dirs.Pop();
				string[] subDirs;
				try
				{
					subDirs = Directory.GetDirectories(currentDir);
				}
				catch (UnauthorizedAccessException)
				{
					continue;
				}
				catch (DirectoryNotFoundException)
				{
					continue;
				}

				try
				{
					//??????????????? здесь и в подобных местах найдется также файл *.pqpxxx, это не хорошо
					listPqpFiles_.AddRange(Directory.GetFiles(currentDir, "*.pqp"));
					listAvgFiles_.AddRange(Directory.GetFiles(currentDir, "*.avg"));
					listDnsFiles_.AddRange(Directory.GetFiles(currentDir, "*.dns"));
				}
				catch (UnauthorizedAccessException)
				{
					continue;
				}
				catch (DirectoryNotFoundException)
				{
					continue;
				}

				// Push the subdirectories onto the stack for traversal.
				// This could also be done before handing the files.
				foreach (string str in subDirs)
					dirs.Push(str);
			}
		}

		#region old search method
		//private static void SearchIn_old(string path, string[] extensions,
		//	ref List<string> files)
		//{
		//	List<string> dirs = new List<string>();

		//	try
		//	{
		//		foreach (var extension in extensions)
		//		{
		//			files.AddRange(Directory.GetFiles(path, extension, SearchOption.TopDirectoryOnly));
		//			dirs.AddRange(Directory.GetDirectories(path));
		//		}
		//	}
		//	catch (UnauthorizedAccessException e)
		//	{
		//		//EmService.WriteToLogFailed(e.Message);
		//	}

		//	foreach (string dir in dirs)
		//	{
		//		DirectoryInfo dirInfo = new DirectoryInfo(dir);
		//		if ((dirInfo.Attributes & FileAttributes.System) == 0 &&
		//			(dirInfo.Attributes & FileAttributes.Hidden) == 0 &&
		//			(dirInfo.Attributes & FileAttributes.Archive) == 0)
		//			SearchIn_old(dir, extensions, ref files);
		//	}
		//}
		#endregion

		#endregion

		#region Operation with temporary files

		public static void DeleteTemporaryArchiveFiles(string path)
		{
			try
			{
				string[] extensions = new string[] { "*.tmppqp", "*.tmpavg", "*.tmpdns" };
				List<string> files = new List<string>();
				List<string> dirs = new List<string>();

				// search files with these extensions and nested dirs
				try
				{
					foreach (var extension in extensions)
					{
						files.AddRange(Directory.GetFiles(path, extension, SearchOption.TopDirectoryOnly));
					}
					dirs.AddRange(Directory.GetDirectories(path));
				}
				catch (UnauthorizedAccessException e)
				{
					EmService.WriteToLogFailed("DeleteTemporaryArchiveFiles  " + e.Message);
				}

				// delete all files with these extensions
				foreach (var file in files)
				{
					File.Delete(file);
				}

				// recursive call
				foreach (string dir in dirs)
				{
					DirectoryInfo dirInfo = new DirectoryInfo(dir);
					if ((dirInfo.Attributes & FileAttributes.System) == 0 &&
						(dirInfo.Attributes & FileAttributes.Hidden) == 0 &&
						(dirInfo.Attributes & FileAttributes.Archive) == 0)
						DeleteTemporaryArchiveFiles(dir);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeleteTemporaryArchiveFiles(): ");
				throw;
			}
		}

		public static bool RenameTemporaryArchiveFiles(string path)
		{
			try
			{
				if (string.IsNullOrEmpty(path))
				{
					EmService.WriteToLogFailed("RenameTemporaryArchiveFiles IsNullOrEmpty");
					return false;
				}
				
				EmService.WriteToLogDebug("RenameTemporaryArchiveFiles enter: " + path);
				string[] extensions = new string[] { "*.tmppqp", "*.tmpavg", "*.tmpdns" };
				List<string> files = new List<string>();
				List<string> dirs = new List<string>();

				// search files with these extensions and nested dirs
				try
				{
					foreach (var extension in extensions)
					{
						files.AddRange(Directory.GetFiles(path, extension, SearchOption.TopDirectoryOnly));
					}
					dirs.AddRange(Directory.GetDirectories(path));
				}
				catch (UnauthorizedAccessException e)
				{
					EmService.WriteToLogFailed("RenameTemporaryArchiveFiles  " + e.Message);
				}

				// rename all files with these extensions
				foreach (var file in files)
				{
					if (file.Contains(".tmppqp")) File.Move(file, file.Replace(".tmppqp", ".pqp"));
					if (file.Contains(".tmpavg")) File.Move(file, file.Replace(".tmpavg", ".avg"));
					if (file.Contains(".tmpdns")) File.Move(file, file.Replace(".tmpdns", ".dns"));
				}

				// recursive call
				foreach (string dir in dirs)
				{
					DirectoryInfo dirInfo = new DirectoryInfo(dir);
					if ((dirInfo.Attributes & FileAttributes.System) == 0 &&
						(dirInfo.Attributes & FileAttributes.Hidden) == 0 &&
						(dirInfo.Attributes & FileAttributes.Archive) == 0)
						RenameTemporaryArchiveFiles(dir);
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in RenameTemporaryArchiveFiles(): ");
				return false;
			}
		}

		#endregion
	}
}
