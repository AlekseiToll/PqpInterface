#ifndef EMUSB_H
#define EMUSB_H

#include <sstream>
#include "EmServiceClasses.h"

//=============================================================================
#define USB_EPOUT_ADDRESS 0x02
#define USB_EPIN_ADDRESS 0x86

#define SLIP_END             0xC0    /* indicates end of packet */
#define SLIP_ESC             0xDB    /* indicates byte stuffing */
#define SLIP_ESC_END         0xDC    /* ESC ESC_END means END data byte */
#define SLIP_ESC_ESC         0xDD    /* ESC ESC_ESC means ESC data byte */

#define 		CRC16_SEED		0xFFFF
const WORD CRC16Table[] =
{
0X0000, 0XC0C1, 0XC181, 0X0140, 0XC301, 0X03C0, 0X0280, 0XC241,
0XC601, 0X06C0, 0X0780, 0XC741, 0X0500, 0XC5C1, 0XC481, 0X0440,
0XCC01, 0X0CC0, 0X0D80, 0XCD41, 0X0F00, 0XCFC1, 0XCE81, 0X0E40,
0X0A00, 0XCAC1, 0XCB81, 0X0B40, 0XC901, 0X09C0, 0X0880, 0XC841,
0XD801, 0X18C0, 0X1980, 0XD941, 0X1B00, 0XDBC1, 0XDA81, 0X1A40,
0X1E00, 0XDEC1, 0XDF81, 0X1F40, 0XDD01, 0X1DC0, 0X1C80, 0XDC41,
0X1400, 0XD4C1, 0XD581, 0X1540, 0XD701, 0X17C0, 0X1680, 0XD641,
0XD201, 0X12C0, 0X1380, 0XD341, 0X1100, 0XD1C1, 0XD081, 0X1040,
0XF001, 0X30C0, 0X3180, 0XF141, 0X3300, 0XF3C1, 0XF281, 0X3240,
0X3600, 0XF6C1, 0XF781, 0X3740, 0XF501, 0X35C0, 0X3480, 0XF441,
0X3C00, 0XFCC1, 0XFD81, 0X3D40, 0XFF01, 0X3FC0, 0X3E80, 0XFE41,
0XFA01, 0X3AC0, 0X3B80, 0XFB41, 0X3900, 0XF9C1, 0XF881, 0X3840,
0X2800, 0XE8C1, 0XE981, 0X2940, 0XEB01, 0X2BC0, 0X2A80, 0XEA41,
0XEE01, 0X2EC0, 0X2F80, 0XEF41, 0X2D00, 0XEDC1, 0XEC81, 0X2C40,
0XE401, 0X24C0, 0X2580, 0XE541, 0X2700, 0XE7C1, 0XE681, 0X2640,
0X2200, 0XE2C1, 0XE381, 0X2340, 0XE101, 0X21C0, 0X2080, 0XE041,
0XA001, 0X60C0, 0X6180, 0XA141, 0X6300, 0XA3C1, 0XA281, 0X6240,
0X6600, 0XA6C1, 0XA781, 0X6740, 0XA501, 0X65C0, 0X6480, 0XA441,
0X6C00, 0XACC1, 0XAD81, 0X6D40, 0XAF01, 0X6FC0, 0X6E80, 0XAE41,
0XAA01, 0X6AC0, 0X6B80, 0XAB41, 0X6900, 0XA9C1, 0XA881, 0X6840,
0X7800, 0XB8C1, 0XB981, 0X7940, 0XBB01, 0X7BC0, 0X7A80, 0XBA41,
0XBE01, 0X7EC0, 0X7F80, 0XBF41, 0X7D00, 0XBDC1, 0XBC81, 0X7C40,
0XB401, 0X74C0, 0X7580, 0XB541, 0X7700, 0XB7C1, 0XB681, 0X7640,
0X7200, 0XB2C1, 0XB381, 0X7340, 0XB101, 0X71C0, 0X7080, 0XB041,
0X5000, 0X90C1, 0X9181, 0X5140, 0X9301, 0X53C0, 0X5280, 0X9241,
0X9601, 0X56C0, 0X5780, 0X9741, 0X5500, 0X95C1, 0X9481, 0X5440,
0X9C01, 0X5CC0, 0X5D80, 0X9D41, 0X5F00, 0X9FC1, 0X9E81, 0X5E40,
0X5A00, 0X9AC1, 0X9B81, 0X5B40, 0X9901, 0X59C0, 0X5880, 0X9841,
0X8801, 0X48C0, 0X4980, 0X8941, 0X4B00, 0X8BC1, 0X8A81, 0X4A40,
0X4E00, 0X8EC1, 0X8F81, 0X4F40, 0X8D01, 0X4DC0, 0X4C80, 0X8C41,
0X4400, 0X84C1, 0X8581, 0X4540, 0X8701, 0X47C0, 0X4680, 0X8641,
0X8201, 0X42C0, 0X4380, 0X8341, 0X4100, 0X81C1, 0X8081, 0X4040
};
//=============================================================================

#define		COMMAND_OK							0x1000
#define		COMMAND_UNKNOWN_COMMAND				0x1001	
#define		COMMAND_CRC_ERROR					0x1002	
#define		COMMAND_BAD_DATA					0x1003	
#define		COMMAND_BAD_PASSWORD				0x1004	
#define		COMMAND_ACCESS_ERROR				0x1005	
#define		COMMAND_CHECK_FAILED				0x1006	
#define		COMMAND_NO_DATA						0x1007
#define		COMMAND_DATA_OVERFLOW				0x1008

#define		COMMAND_ReadTime					0x0001
#define		COMMAND_WriteTime					0x0002
//#define		COMMAND_ReadEEPROM					0x2000
#define		COMMAND_ReadSystemData				0x000B
#define		COMMAND_WriteSystemData				0x000C
#define		COMMAND_CommitSystemData			0x0040
#define		COMMAND_ReadEventLogger				0x0012
#define		COMMAND_ReadMeasurements3Sec		0xA000
#define		COMMAND_ReadMeasurements10Min		0xA001
#define		COMMAND_ReadMeasurements2Hour		0xA002

#define		COMMAND_ReadDSIStatus				0x4006
#define		COMMAND_ReadDSIArchives				0x4007
//#define		COMMAND_ReadSDRAM					0x2020
#define		COMMAND_ReadRegistrationSets		0x4008

#define		COMMAND_ReadRegistrationIndices		0x4009
#define		COMMAND_ReadRegistrationByIndex		0x400A

//#define		COMMAND_ReadNandTraslationTable		0x400C
#define		COMMAND_RestartWORK					0x0016
#define		COMMAND_RestartInterface			0x0003

#define		COMMAND_ReadRegistrationArchiveByIndex		0x400D
#define		COMMAND_ReadDSIArchivesByRegistration		0x400E
#define		COMMAND_Trace						0x400F
#define 	COMMAND_ReadAverageArchive3SecByIndex	0x4010
#define 	COMMAND_ReadAverageArchive10MinByIndex	0x4011
#define 	COMMAND_ReadAverageArchive2HourByIndex	0x4012
#define 	COMMAND_ReadAverageArchive3SecIndices	0x4013
#define 	COMMAND_ReadAverageArchive10MinIndices	0x4014
#define 	COMMAND_ReadAverageArchive2HourIndices	0x4015

#define 	COMMAND_ReadAverageArchive3SecIndexByDateTime	0x4016
#define 	COMMAND_ReadAverageArchive10MinIndexByDateTime	0x4017
#define 	COMMAND_ReadAverageArchive2HourIndexByDateTime	0x4018

#define 	COMMAND_ReadAverageArchive3SecMinMaxIndices		0x4019
#define 	COMMAND_ReadAverageArchive10MinMinMaxIndices	0x401A
#define 	COMMAND_ReadAverageArchive2HourMinMaxIndices	0x401B

#define COMMAND_UsbModule_Setup			0x0070	
#define COMMAND_UsbModule_GetStatus			0x0071
//=============================================================================

#define		TYPE_PC								0x00L
#define		TYPE_ENERGOMONITOR32				0x01L	//???????????? why
//#define		TYPE_ENERGOTESTER					0x02L
//#define		TYPE_ENERGOFORMA30					0x03L
//#define		TYPE_ENERGOMONITOR30				0x04L
#define		TYPE_ENERGOTESTERA					0x05L

#define		TYPE_BROADCAST						0x0FL
//=============================================================================

enum TStuff
{
	STUFF_IDLE,
	STUFF_BYTE1, 
	STUFF_NEWBYTE,
	STUFF_END
}; 

enum EProtocol
{
	PROTOCOL_LISTENING,
	PROTOCOL_ADDRESS0_EXPECTED,
	PROTOCOL_ADDRESS1_EXPECTED,
	PROTOCOL_ADDRESS2_EXPECTED,
	PROTOCOL_ADDRESS3_EXPECTED,
	PROTOCOL_COMMAND0_EXPECTED,
	PROTOCOL_COMMAND1_EXPECTED,
	PROTOCOL_LENGTH0_EXPECTED,
	PROTOCOL_LENGTH1_EXPECTED,
	PROTOCOL_DATA_EXPECTED,
	PROTOCOL_CRC0_EXPECTED,
	PROTOCOL_CRC1_EXPECTED,
	PROTOCOL_PACKET_BEING_PROCESSED
};

enum EUsbResult
{
	USBRESULT_TIMEOUT = 0,
	USBRESULT_COMMERROR,
	USBRESULT_CRCERROR,
	USBRESULT_OK,
	USBRESULT_CANCELLED,
};

struct TUsbRequest
{
	DWORD dwAddress;
	WORD wCommand;
		DWORD wpad0;
	WORD wLength;
		DWORD wpad1;
	BYTE bData[0x8000];
		DWORD wpad2;
	WORD wCRC;
};

struct TUsbReply
{
	DWORD dwAddress;
	WORD wCommand;
		DWORD wpad0;
	WORD wLength;
		DWORD wpad1;
	WORD wCounter;
		DWORD wpad2;
	BYTE bData[0x8000];
		DWORD wpad3;
	WORD wCRC;
};

class CUsb;

// timer function
VOID CALLBACK HeartbeatTimerAPCProcGlobal(LPVOID lpArg,             // Data value
									DWORD dwTimerLowValue,      // Timer low value
									DWORD dwTimerHighValue);

class CHeartbeatTimerThread
{ 
	HANDLE hHeartbeatTimer_;
	CUsb* owner_;
	bool bStopTimer_;

public: 
	CHeartbeatTimerThread(CUsb* port): owner_(port), bStopTimer_(false)
	{
	}

	void StopTimer() { bStopTimer_ = true; }

	DWORD DwCommunicationTimeout;
	DWORD DwHeartbeatCounter;

	void HeartbeatTimerAPCProc();

	static void ThreadEntryStatic(CHeartbeatTimerThread* pObj)
	{
		pObj->ThreadEntry();
	}

	void ThreadEntry()
	{
		if (hHeartbeatTimer_ = CreateWaitableTimer(NULL,         // Default security attributes
					FALSE,                  // Create auto-reset timer
					"HeartbeatTimer"))      // Name of waitable timer
		{
			BOOL            bSuccess;
			__int64         qwDueTime;
			LARGE_INTEGER   liDueTime;
			try 
			{
				EmService::WriteToLogGeneral("CHeartbeatTimerThread::ThreadEntry(): timer was started");

				// Create an integer that will be used to signal the timer 
				// 5 seconds from now.
				qwDueTime = -1 * 10000000;

				// Copy the relative time into a LARGE_INTEGER.
				liDueTime.LowPart  = (DWORD) ( qwDueTime & 0xFFFFFFFF );
				liDueTime.HighPart = (LONG)  ( qwDueTime >> 32 );

				bSuccess = SetWaitableTimer(hHeartbeatTimer_, &liDueTime, 1000, 
					HeartbeatTimerAPCProcGlobal, this, FALSE);

				if (bSuccess) 
				{
					while(1)
					{
					   SleepEx(INFINITE, TRUE); 
					   if(bStopTimer_) 
						   break;
					}
				} 
				else 
				{
					DWORD errCode = GetLastError();
					std::stringstream ss;
					ss << "SetWaitableTimer failed with Error " << errCode;
					EmService::WriteToLogFailed(ss.str());
				}
			}
			catch(...) 
			{
				EmService::WriteToLogFailed("Error in CHeartbeatTimerThread::ThreadEntry");
				throw;
			}

			CancelWaitableTimer(hHeartbeatTimer_);
			EmService::WriteToLogGeneralWithDate("CHeartbeatTimerThread::ThreadEntry(): timer was cancelled");
			//CloseHandle(hHeartbeatTimer_);
		} 
		else 
		{
			std::stringstream ss;
			ss << "CreateWaitableTimer failed with Error " << GetLastError();
			EmService::WriteToLogFailed(ss.str());
		}
	}
};

class CUsb
{
protected:
	HANDLE hUsbDevice_;
	TStuff stuffState_;
	EProtocol protocolState_;

	CHeartbeatTimerThread* pTimerThread_;
	HANDLE hTimerThread_;
	HANDLE hEventCommunicationTimeout_;
	HANDLE hEventPacketReceived_;
	HANDLE hEventDisconnected_;
	HANDLE* hEventsArray_;

	TUsbReply xActualReply_;

	HANDLE rxThread_;

	BOOL bUsbReceiverEnable_;
	BOOL bUsbReceiverEnabled_;

	void UsbSendRequest(TUsbRequest *Request);
	void crc16(BYTE byte, WORD *crc);

	void RxThread();
	void UsbRxByte(BYTE bByte);
	void UsbSendOverlapped(BYTE *pbData,DWORD dwBytes,DWORD dwTimeout);
	void UsbSendRequestCore(BYTE bByte, BYTE *pBuffer, DWORD *pdwPtr,WORD *pwCrc);

	static void RxThreadStart(LPVOID param)
	{
		((CUsb*)param)->RxThread();
	}

	void SetTimeoutCounter(WORD timeOut)
	{
		DWORD dwtemp = pTimerThread_->DwHeartbeatCounter + timeOut;
		if (dwtemp == 0xFFFFFFFF) dwtemp = 0;
		pTimerThread_->DwCommunicationTimeout = dwtemp;
		ResetEvent(hEventCommunicationTimeout_);
		ResetEvent(hEventPacketReceived_);
	}

	void ResetTimeoutCounter()
	{
		pTimerThread_->DwCommunicationTimeout = 0xFFFFFFFF;
		ResetEvent(hEventCommunicationTimeout_);
	}

public:
	CUsb();
	~CUsb();

	BOOL UsbConnect();
	void UsbDisconnect();
	
	EUsbResult UsbCommunication(		
					BOOL boolEmptyRequest,
					DWORD dwTimeoutSecs,
					WORD *wReplyAddress,
					WORD *wReplyCommand,
					WORD *wReplyLength,
					void* pvReplyData,
					WORD wMaxReplyLength,
					WORD wCommand = 0,
					WORD wLength = 0,
					void* pvData = 0);

	void SetTimeoutEvent()
	{
		SetEvent(hEventCommunicationTimeout_);
	}
};

#endif
