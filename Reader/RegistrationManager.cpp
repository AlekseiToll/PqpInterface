#define _CRT_SECURE_NO_WARNINGS

#include "stdafx.h"
#include "RegistrationManager.h"

RegistrationManager::RegistrationManager(std::string pipeName) : CountArchives(0), CurrentArchiveNumber(0)
{
	//LPTSTR lpszPipename = TEXT("\\\\.\\pipe\\readeretpqpa");
	std::string fullPipeName = "\\\\.\\pipe\\" + pipeName;
	
	while (1) 
	{ 
		hPipe_ = CreateFile( 
			fullPipeName.c_str(),   // pipe name 
			GENERIC_READ |  // read and write access
			GENERIC_WRITE, 
			0,              // no sharing 
			NULL,           // default security attributes
			OPEN_EXISTING,  // opens existing pipe 
			0,              // default attributes 
			NULL);          // no template file 
 
		// Break if the pipe handle is valid
		if (hPipe_ != INVALID_HANDLE_VALUE)
		{
			EmService::WriteToLogGeneral("hPipe valid");
			break;
		}
		else EmService::WriteToLogFailed("hPipe NOT valid");
 
		// Exit if an error other than ERROR_PIPE_BUSY occurs
		if (GetLastError() != ERROR_PIPE_BUSY) 
		{
			//EmService::WriteToLogFailed(TEXT("1 Could not open pipe. GLE=%d\n"), GetLastError());??????????????????
			hPipe_ = INVALID_HANDLE_VALUE;
			return;
		}
 
		// All pipe instances are busy, so wait for 20 seconds
		if (!WaitNamedPipe(fullPipeName.c_str(), 20000)) 
		{ 
			EmService::WriteToLogFailed("2 Could not open pipe: 20 second wait timed out.");
			hPipe_ = INVALID_HANDLE_VALUE;
			return;
		} 
	}
}

void RegistrationManager::WriteToPipe(BYTE* buffer)
{
	DWORD  cbToWrite = 10, cbWritten;

	if(hPipe_ == INVALID_HANDLE_VALUE)
		return;

	try
	{
		BOOL fSuccess = WriteFile( 
					hPipe_,                  // pipe handle 
					buffer,					// message 
					cbToWrite,              // message length 
					&cbWritten,             // bytes written 
					NULL);                  // not overlapped 

		if (!fSuccess) 
		{
			//EmService::WriteToLogFailed(TEXT("WriteToPipe failed. GLE=%d\n"), GetLastError());???????????????????????
		}
	}
	catch(...)
	{
		//EmService::WriteToLogFailed(TEXT("WriteToPipe exception. GLE=%d\n"), GetLastError());????????????????
	}
}

void RegistrationManager::WriteToPipeAboutError(EUsbResult res)
{
	try
	{
		BYTE buffer[10];
		buffer[BUF_TO_CSHARP_VALID_DATA] = 5;			// sign that data is valid

		switch(res)
		{
			case USBRESULT_TIMEOUT: buffer[BUF_TO_CSHARP_IF_ERROR] = TO_CSHARP_TIMEOUT; break;
			case USBRESULT_CRCERROR: buffer[BUF_TO_CSHARP_IF_ERROR] = TO_CSHARP_READ_ERROR; break;
			case USBRESULT_CANCELLED: buffer[BUF_TO_CSHARP_IF_ERROR] = TO_CSHARP_DISCONNECT; break;
			default: buffer[1] = TO_CSHARP_READ_ERROR;
		}
		buffer[BUF_TO_CSHARP_CNT_ARCHIVES] = 0;				// count of archives to read
		buffer[BUF_TO_CSHARP_CUR_ARCHIVE] = 0;				// current archive number
		buffer[BUF_TO_CSHARP_PERCENT] = 0;				// percent of reading current archive
		WriteToPipe(buffer);
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in WriteToPipeAboutError()");
		throw;
	}
}

void RegistrationManager::WriteToPipeCurrenPercent(int percent)
{
	try
	{
		BYTE buffer[10];
		buffer[BUF_TO_CSHARP_VALID_DATA] = 5;			// sign that data is valid
		buffer[BUF_TO_CSHARP_IF_ERROR] = TO_CSHARP_NO_READ_ERROR;			// no reading error
		buffer[BUF_TO_CSHARP_CNT_ARCHIVES] = CountArchives;				// count of archives to read
		buffer[BUF_TO_CSHARP_CUR_ARCHIVE] = CurrentArchiveNumber;		// current archive number
		buffer[BUF_TO_CSHARP_PERCENT] = percent;			// percent of reading current archive
		WriteToPipe(buffer);
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in WriteToPipeCurrenPercent()");
		throw;
	}
}