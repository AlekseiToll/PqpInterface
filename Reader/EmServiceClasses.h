#ifndef EMSERVICECLASSES_H
#define EMSERVICECLASSES_H

#include <Windows.h>
#include <vector>
#include <sstream>
#include <list>
#include <sys/timeb.h>
#include <time.h>
#include <fstream>
#include <iostream>
#include <algorithm>
#include <iterator>
#include <string>
#include <io.h>

enum EIndexInBufferToCSharp
{
	/// <summary>sign that data is valid</summary>
	BUF_TO_CSHARP_VALID_DATA = 0,
	/// <summary>1 = no error, 2 = some read error, 3 = disconnect</summary>
	BUF_TO_CSHARP_IF_ERROR = 1,
	/// <summary>count of archives to read</summary>
	BUF_TO_CSHARP_CNT_ARCHIVES = 2,
	/// <summary>current archive number</summary>
	BUF_TO_CSHARP_CUR_ARCHIVE = 3,
	/// <summary>percent of reading current archive</summary>
	BUF_TO_CSHARP_PERCENT = 4
};

/// <summary>This enum must be synchronized with the same enum in C++ modul</summary>
enum ErrorFromCpp
{
	TO_CSHARP_NO_READ_ERROR = 1,
	TO_CSHARP_READ_ERROR = 2,
	TO_CSHARP_DISCONNECT = 3,
	TO_CSHARP_TIMEOUT = 4
};

enum EAvgType
{
	THREE_SEC = 0,
	TEN_MIN = 1,
	TWO_HOUR = 2,
	NONE = 3
};

enum EReadMode
{
	ALL,
	SELECTED
};

struct TDateTime
{
	WORD wUtcDate;
	WORD wUtcMonth;
	WORD wUtcYear;
	WORD wUtcHours;
	WORD wUtcMinutes;
	WORD wUtcSeconds;
		WORD wLocalDate;
		WORD wLocalMonth;
		WORD wLocalYear;
		WORD wLocalHours;
		WORD wLocalMinutes;
		WORD wLocalSeconds;
	WORD wMilliseconds;
	WORD wTimeZone;
};

class DateTime
{
	WORD year_;
	WORD month_;
	WORD day_;
	WORD hour_;
	WORD minutes_;
	WORD seconds_;
	WORD milliseconds_;

	friend bool operator ==(const DateTime& d1, const DateTime& d2);

public:
	DateTime(WORD y, BYTE mo, BYTE d, BYTE h, BYTE min, BYTE sec, WORD millisec) :
	  year_(y), month_(mo), day_(d), hour_(h), minutes_(min), seconds_(sec), milliseconds_(millisec)
	  {}

	DateTime(WORD y, BYTE mo, BYTE d, BYTE h, BYTE min, BYTE sec) :
	  year_(y), month_(mo), day_(d), hour_(h), minutes_(min), seconds_(sec), milliseconds_(0)
	  {}

	DateTime(BYTE h, BYTE min, BYTE sec) :
	  hour_(h), minutes_(min), seconds_(sec), milliseconds_(0)
	{
		time_t curTimer;
		time(&curTimer);
		tm* curTime = localtime(&curTimer);
		year_ = curTime->tm_year + 1900;
		month_ = curTime->tm_mon;
		day_ = curTime->tm_mday;
	}

	static DateTime Now()
	{
		time_t curTimer;
		time(&curTimer);
		tm* curTime = localtime(&curTimer);
		return DateTime(curTime->tm_year + 1900, curTime->tm_mon + 1, curTime->tm_mday, curTime->tm_hour,
			curTime->tm_min, curTime->tm_sec);
	}

	std::string ToString()
	{
		std::stringstream ss;
		ss << day_ << "." << month_ << "." << year_ << " " << hour_ << ":" << minutes_ << ":" << seconds_;
		return ss.str();
	}
};

bool operator ==(const DateTime& d1, const DateTime& d2);

//template<class T> class EmList
//{
//	std::list<T> list_;
//
//public:
//	bool Contains(const T& item)
//	{
//		std::list<T>::iterator res = std::find(list_.begin(), list_.end(), item);
//		if(res == list_.end()) return false;
//		return true;
//	}
//
//	void push_back(const T& value)
//	{
//		list_.push_back(value);
//	}
//};

class EmService 
{
public:
	//static CRITICAL_SECTION scLogFailed;
	//static CRITICAL_SECTION scLogGeneral;
	static std::string appDirectory;
	static std::string logFailedName;
	static std::string logGeneralName;
	static bool appDirWasSet;

	static void Init()
	{
		if(!EmService::appDirWasSet)
		{
			TCHAR szPath[MAX_PATH];
			if(!GetModuleFileName(NULL, szPath, MAX_PATH))
			{
				EmService::WriteToLogFailed("GetModuleFileName failed " + EmService::NumberToString(GetLastError())); 
			}
			std::string appDir = std::string(szPath);

			static const std::basic_string<char>::size_type npos = -1;
			std::basic_string<char>::size_type indexSlash = appDir.find_last_of("\\", appDir.length());
			if(indexSlash == npos)
				EmService::WriteToLogFailed("indexSlash == npos\n");
			else appDir = appDir.substr(0, indexSlash + 1);
			EmService::appDirectory = appDir;
			EmService::logFailedName = EmService::appDirectory + EmService::logFailedName;
			EmService::logGeneralName = EmService::appDirectory + EmService::logGeneralName;
			EmService::WriteToLogGeneral("appDir = " + appDir);
			EmService::appDirWasSet = true;
		}
	}

	static bool IsLetter(char c) 
	{ 
		return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
			(c >= 'а' && c <= '€') || (c >= 'ј' && c <= 'я');
	}

	static std::string RemoveAllExceptLettersAndDigits(std::string src)
	{
		std::string res;
		for(int iChar = 0; iChar < src.length(); ++iChar)
		{
			if(IsLetter(src[iChar]) || isdigit(src[iChar]))
				res += src[iChar];
		}
		return res;
	}

	static bool FileExists(const char *fname)
	{
		return access(fname, 0) != -1;
	}

	static std::string MakeCorrectNumberLength(int number, int len)
	{
		std::stringstream ss;
		ss << number;
		std::string strnum = ss.str();
		while(strnum.length() < len) strnum = "0" + strnum;
		return strnum;
	}

	static void WriteToLogFailed(const std::string& s)
	{
		try
		{
			//EnterCriticalSection(&scLogFailed);
			std::ofstream ofs(logFailedName.c_str(), std::ios_base::out | std::ios_base::app);
			ofs << s << '\n';
			ofs.close();
			//LeaveCriticalSection(&scLogFailed);
		}
		catch(...) {}
	}

	static void WriteToLogFailedWithDate(const std::string& s)
	{
		try
		{
			std::stringstream ss;
			ss << s;
			ss << ",  " << EmService::GetCurrentDateTime();
			EmService::WriteToLogFailed(ss.str());
		}
		catch(...) {}
	}

	static void WriteToLogGeneral(const std::string& s)
	{
		try
		{
			//EnterCriticalSection(&scLogGeneral);
			std::ofstream ofs(logGeneralName.c_str(), std::ios_base::out | std::ios_base::app);
			ofs << s << '\n';
			ofs.close();
			//LeaveCriticalSection(&scLogGeneral);
		}
		catch(...) {}
	}

	static void WriteToLogGeneralWithDate(const std::string& s)
	{
		try
		{
			std::stringstream ss;
			ss << s;
			ss << ",  " << EmService::GetCurrentDateTime();
			EmService::WriteToLogGeneral(ss.str());
		}
		catch(...) {}
	}

	static void WriteToLogGeneral2(const std::string& s)
	{
		try
		{
			//EnterCriticalSection(&scLogGeneral);
			std::ofstream ofs(logGeneralName.c_str(), std::ios_base::out | std::ios_base::app);
			ofs << s << "  ";
			ofs.close();
			//LeaveCriticalSection(&scLogGeneral);
		}
		catch(...) {}
	}

	static std::string GetCommandText(WORD command)
	{
		switch (command)
		{
			case 0x1000: return "COMMAND_OK";
			case 0x1003: return "COMMAND_BAD_DATA";
			case 0x1004: return "COMMAND_BAD_PASSWORD";
			case 0x1005: return "COMMAND_ACCESS_ERROR";
			case 0x1006: return "COMMAND_CHECK_FAILED";
			case 0x1007: return "COMMAND_NO_DATA";
			case 0x1001: return "COMMAND_UNKNOWN_COMMAND";
			case 0x1002: return "COMMAND_CRC_ERROR";
			case 0x0000: return "COMMAND_ECHO";
			case 0x0001: return "COMMAND_ReadTime";
			case 0x0002: return "COMMAND_ReadCalibration";
			case 0x0003: return "COMMAND_WriteCalibration";
			case 0x0009: return "COMMAND_ReadQualityDates";
			case 0x000A: return "COMMAND_ReadQualityEntry";
			case 0x0007: return "COMMAND_ReadSets";
			case 0x0008: return "COMMAND_WriteSets";
			case 0x000B: return "COMMAND_ReadSystemData";
			case 0x000C: return "COMMAND_WriteSystemData";
			case 0x000D: return "COMMAND_Read3secValues";
			case 0x000E: return "COMMAND_Read1minValues";
			case 0x000F: return "COMMAND_Read30minValues";
			case 0x0012: return "COMMAND_ReadEventLogger";
			case 0x0019: return "COMMAND_ReadDipSwellArchive";
			case 0x0010: return "COMMAND_ReadDipSwellStatus";
			case 0x001A: return "COMMAND_ReadDipSwellIndexByStartTimestamp";
			case 0x001B: return "COMMAND_ReadDipSwellIndexByEndTimestamp";
			case 0x001C: return "COMMAND_ReadEarliestAndLatestDipSwellTimestamp";
			case 0x0013: return "COMMAND_Read3secArchiveByTimestamp";
			case 0x0014: return "COMMAND_Read1minArchiveByTimestamp";
			case 0x0015: return "COMMAND_Read30minArchiveByTimestamp";
			case 0x001E: return "COMMAND_ReadEarliestAndLatestAverageTimestamp";
			case 0x001F: return "COMMAND_ReadQualityDatesByObject";
			case 0x0006: return "COMMAND_AverageArchiveQuery";
			case 0x0026: return "COMMAND_Read3secArchiveByTimestampObjectDemand";
			case 0x0027: return "COMMAND_Read1minArchiveByTimestampObjectDemand";
			case 0x0028: return "COMMAND_Read30minArchiveByTimestampObjectDemand";
			case 0x0024: return "COMMAND_ReadObjectsEntrys";
			case 0x0025: return "COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand";
			case 0x0021: return "COMMAND_ReadDipSwellArchiveByObject";
			case 0x0022: return "COMMAND_ReadDipSwellIndexByStartTimestampByObject";
			case 0x0023: return "COMMAND_ReadDipSwellIndexByEndTimestampByObject";
			//case 0x0020: return "COMMAND_ReadQualityEntryObjectDemand";
			case 0x0032: return "COMMAND_ReadQualityContents";
			case 0x0033: return "COMMAND_ReadQualityEntryByTimestampByObject";
			case 0x4009: return "COMMAND_ReadRegistrationIndices";
			case 0x400A: return "COMMAND_ReadRegistrationByIndex";
			case 0x400D: return "COMMAND_ReadRegistrationArchiveByIndex";
			case 0x4010: return "COMMAND_ReadAverageArchive3SecByIndex";
			case 0x4011: return "COMMAND_ReadAverageArchive10MinByIndex";
			case 0x4012: return "COMMAND_ReadAverageArchive2HourByIndex";
			case 0x4016: return "COMMAND_ReadAverageArchive3SecIndexByDateTime";
			case 0x4017: return "COMMAND_ReadAverageArchive10MinIndexByDateTime";
			case 0x4018: return "COMMAND_ReadAverageArchive2HourIndexByDateTime";
			case 0x4019: return "COMMAND_ReadAverageArchive3SecMinMaxIndices";
			case 0x401A: return "COMMAND_ReadAverageArchive10MinMinMaxIndices";
			case 0x401B: return "COMMAND_ReadAverageArchive2HourMinMaxIndices";
			default: return EmService::NumberToString(command);
		}
	}

	static std::string GetCurrentDateTime()
	{
		/*char tmpbuf[128];
		std::string res;

		// Set time zone from TZ environment variable. If TZ is not set,
		// the operating system is queried to obtain the default value 
		// for the variable. 
		_tzset();

		// Display operating system-style date and time. 
		_strtime_s( tmpbuf, 128 );
		res = std::string(tmpbuf);
		_strdate_s( tmpbuf, 128 );
		res += std::string("  ");
		res += std::string(tmpbuf);
		return res;*/

		return DateTime::Now().ToString();
	}

	static std::string AvgTypeToString(EAvgType type)
	{
		switch(type)
		{ 
		case THREE_SEC: return "3sec";
		case TEN_MIN: return "10min";
		case TWO_HOUR: return "2hour";
		default: return "UnknownAVG";
		}
	}

	static std::string NumberToString(long value)
	{
		/*char buffer[1024];
		_itoa_s(value, buffer, 10); 
		std::string res = std::string(buffer);
		return res;*/
		std::stringstream ss;
		ss << value;
		return ss.str();
	}

	static int DigitCount(long number) 
	{
		int result = 1;
  
		while ((number /= 10) != 0) 
			result++;
 
		return result;
	}

	/*static int DigitCount(DWORD number) 
	{
		int result = 1;
  
		while ((number /= 10) != 0) 
			result++;
 
		return result;
	}*/

	static void IntToBytes(int val, char* buf, int shift)
	{
		buf[3 + shift] = (char)((unsigned int)val / 0x1000000 % 0x100);
		buf[2 + shift] = (char)((unsigned int)val / 0x10000 % 0x100);
		buf[1 + shift] = (char)((unsigned int)val / 0x100 % 0x100);
		buf[0 + shift] = (char)((unsigned int)val % 0x100);
	}

	static int BytesToInt(char* buf, int shift)
	{
		/*int i = array[shift + 3] * 0x1000000 + array[shift + 2] * 0x10000 +
			array[shift + 1] * 0x100 + array[shift];
		return i;*/ // этот вариант полностью идентичен второму

		//bool sign = (Array[shift + 3] / 128 == 1);
		unsigned int lo, hi;
		lo = (unsigned int)(buf[shift + 1] * 0x100 + buf[shift]);
		hi = (unsigned int)(buf[shift + 3] * 0x100 + buf[shift + 2]);
		return (int)(hi * 0x10000 + lo);
		//return sign ? -res : res;
	}

	template<class T> static std::string VectorToString(std::vector<T>& vec)
	{
		std::stringstream ss;
		std::copy(vec.begin(), vec.end(), std::ostream_iterator<T, char>(ss, " "));
		return ss.str();
	}

	static long StringToNumberL(const std::string& str)
	{
		return atol(str.c_str());
	}

	static long StringToNumberI(const std::string& str)
	{
		return atoi(str.c_str());
	}

	static std::string StdStringToLower(const std::string& str)
	{
		std::string res;
		std::string::const_iterator it = str.begin();
		for(;  it != str.end(); ++it) 
		{
			res += tolower(*it);
		}
		return res;
	}

	static std::string RemoveSymbolFromString(const std::string& str, int symbolCode)
	{
		return RemoveSymbolFromString(str, (char)symbolCode);
	}

	static std::string RemoveSymbolFromString(const std::string& str, char c)
	{
		std::string res = str;
		std::string::iterator b = res.begin(), e = res.end(), r;
		r = remove(b, e, c);
		if(r != e) res.erase(r, res.end());
		return res;
	}

	// the function was found here http://rsdn.ru/forum/cpp.applied/2364177.hot
	static bool DirExists(const char *fname)
	{
		if( fname == NULL || strlen(fname) == 0 )
		{
			return false;
		}
		DWORD dwAttrs = ::GetFileAttributes(fname);
		if( dwAttrs == DWORD(-1) )
		{
			DWORD dLastError = GetLastError();
			if(    ERROR_TOO_MANY_NAMES == dLastError 
				|| ERROR_SHARING_VIOLATION == dLastError 
				|| ERROR_TOO_MANY_SESS == dLastError 
				|| ERROR_SHARING_BUFFER_EXCEEDED == dLastError )
			{
				return true;
			}else
			{
				return false;
			}
		}
		return true;
	}
};

class EmException
{
public:
	std::string Message;
	EmException(std::string mess): Message(mess) {}
};

#endif