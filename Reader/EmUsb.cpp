#define _CRT_SECURE_NO_WARNINGS

#include "stdafx.h"
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>

#include <Setupapi.h> // !!! Setupapi.lib must be linked !!!
#include "cyioctl.h"
#include <winioctl.h>

#include "EmUsb.h"

// timer function
VOID CALLBACK HeartbeatTimerAPCProcGlobal(LPVOID lpArg,             // Data value
									DWORD dwTimerLowValue,      // Timer low value
									DWORD dwTimerHighValue)		// Timer high value

{
	CHeartbeatTimerThread *pTimerThread = (CHeartbeatTimerThread*)lpArg;
	pTimerThread->HeartbeatTimerAPCProc();
}

void CHeartbeatTimerThread::HeartbeatTimerAPCProc()
{
	InterlockedIncrement((LONG*) &DwHeartbeatCounter);
	if (DwCommunicationTimeout != 0xFFFFFFFF)
	{
		if (DwHeartbeatCounter == DwCommunicationTimeout)
		{
			DwCommunicationTimeout = 0xFFFFFFFF;
			std::stringstream ss;
			ss << "HeartbeatTimerCallBack() dwHeartbeatCounter_ = " << DwHeartbeatCounter;
			ss << ",  " << EmService::GetCurrentDateTime();
			EmService::WriteToLogFailed(ss.str());
			//owner_->UsbDisconnect();
			owner_->SetTimeoutEvent();
		}
	}
}

//{AE18A550-7F6A-11d4-97DD-00010229B95B}
static GUID CYUSBDRV_GUID = {0xAE18A550,0x7F6A,0x11d4,0x97,0xDD,0x00,0x01,0x02,0x29,0xB9,0x5B}; 
//9f543223-cede-4fa3-b376-a25ce9a30e74
//static GUID CYUSBDRV_GUID = {0x9f543223,0xcede,0x4fa3,0xB3,0x76,0xa2,0x5c,0xe9,0xa3,0x0e,0x74}; 

CUsb::CUsb() : pTimerThread_(0), hUsbDevice_(INVALID_HANDLE_VALUE)
{
	EmService::WriteToLogGeneral("CUsb constructor");

	hEventCommunicationTimeout_ = CreateEvent(NULL, true, false, NULL);
	hEventPacketReceived_ = CreateEvent(NULL, true, false, NULL);
	hEventDisconnected_ = CreateEvent(NULL, true, false, NULL);
	hEventsArray_ = new HANDLE[3]; 
	hEventsArray_[0] = hEventCommunicationTimeout_; 
	hEventsArray_[1] = hEventPacketReceived_;
	hEventsArray_[2] = hEventDisconnected_ ;

	//xActualReply_ = new StructReply();
}

CUsb::~CUsb() 
{
	EmService::WriteToLogGeneral("CUsb destructor");

	CloseHandle(hEventCommunicationTimeout_);
	CloseHandle(hEventPacketReceived_);
	CloseHandle(hEventDisconnected_);
	delete[] hEventsArray_;

	//delete xActualReply_;
}

//====================================================================
//====================================================================
BOOL CUsb::UsbConnect(void)
{
	try
	{
		HDEVINFO hwDeviceInfo = SetupDiGetClassDevs ( (LPGUID) &CYUSBDRV_GUID, 
													  NULL, 
													  NULL, 
													  DIGCF_PRESENT|DIGCF_INTERFACEDEVICE); 

		SP_DEVINFO_DATA devInfoData; 
		SP_DEVICE_INTERFACE_DATA  devInterfaceData; 
		PSP_INTERFACE_DEVICE_DETAIL_DATA functionClassDeviceData; 
		int deviceNumber = 0;
		DWORD requiredLength = 0; 
		BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; 
		SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;  

		bUsbReceiverEnable_ = FALSE;
		bUsbReceiverEnabled_ = FALSE;

		if (hwDeviceInfo != INVALID_HANDLE_VALUE)
		{ 
			devInterfaceData.cbSize = sizeof(devInterfaceData); 

			if (SetupDiEnumDeviceInterfaces ( hwDeviceInfo, 0, (LPGUID) &CYUSBDRV_GUID, deviceNumber, 
												&devInterfaceData))
			{ 

				SetupDiGetInterfaceDeviceDetail ( hwDeviceInfo, &devInterfaceData, NULL, 0, &requiredLength, NULL); 

				ULONG predictedLength = requiredLength; 

				functionClassDeviceData = (PSP_INTERFACE_DEVICE_DETAIL_DATA) malloc (predictedLength); 
				functionClassDeviceData->cbSize = sizeof (SP_INTERFACE_DEVICE_DETAIL_DATA); 

				devInfoData.cbSize = sizeof(devInfoData); 

				if (SetupDiGetInterfaceDeviceDetail (hwDeviceInfo, &devInterfaceData, functionClassDeviceData,
															predictedLength, &requiredLength, &devInfoData))
				{ 
					hUsbDevice_ = CreateFile(
									functionClassDeviceData->DevicePath, 
									GENERIC_WRITE | GENERIC_READ, 
									FILE_SHARE_WRITE | FILE_SHARE_READ, 
									NULL, 
									OPEN_EXISTING, 
									FILE_FLAG_OVERLAPPED, 
									NULL); 

					free(functionClassDeviceData); 
					SetupDiDestroyDeviceInfoList(hwDeviceInfo); 

					bUsbReceiverEnable_ = TRUE;
					//hThread_UsbReceiver = CreateThread( NULL,  0,  ThreadFunc_UsbReceiver,  NULL,  0,  NULL);
					rxThread_ = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)(RxThreadStart), this, 0, NULL);

					// create heartbeat timer
					pTimerThread_ = new CHeartbeatTimerThread(this);
					hTimerThread_ = CreateThread(NULL, 0, 
						(LPTHREAD_START_ROUTINE)(CHeartbeatTimerThread::ThreadEntryStatic), pTimerThread_, 0, NULL);
					return TRUE;
				}
			}
		}

		delete [] pCtrlBuf;
		return FALSE;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in UsbConnect()");
		throw;
	}
}
//====================================================================
//====================================================================
void CUsb::UsbDisconnect(void)
{
	try
	{
		bUsbReceiverEnable_ = FALSE;

		while(1)
		{
			Sleep(100);
			if (bUsbReceiverEnabled_ == FALSE)
			{
				break;
			}
		}

		if(pTimerThread_ != 0)
		{
			pTimerThread_->StopTimer();
			Sleep(1000);
			delete pTimerThread_;
			pTimerThread_ = 0;
		}

		if(hUsbDevice_ != INVALID_HANDLE_VALUE)
			CloseHandle(hUsbDevice_);
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in UsbDisonnect()");
		throw;
	}
}
//====================================================================
//====================================================================
//DWORD WINAPI ThreadFunc_UsbReceiver(LPVOID lpv)
void CUsb::RxThread()
{
	try
	{
		BOOL bDeviceIoControlResult;
		BYTE UsbRxBuffer[4096];
		OVERLAPPED RxOverlapped;
		DWORD dwReturnBytes;
		BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; 
		SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;  
		DWORD dw;
		DWORD dwLastError;
		BOOL bGetOverolappedResultResult;
		BOOL bBreak;

		bUsbReceiverEnabled_ = TRUE;
		while(1)
		{
			if (bUsbReceiverEnable_ == FALSE)
			{
				break;
			}

			memset(pCtrlBuf,0,sizeof(SINGLE_TRANSFER));  
			pTransfer->ucEndpointAddress = USB_EPIN_ADDRESS;  
			memset(&RxOverlapped, 0, sizeof(RxOverlapped));
			RxOverlapped.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
			bDeviceIoControlResult = DeviceIoControl(
				hUsbDevice_,  
				IOCTL_ADAPT_SEND_NON_EP0_DIRECT,
				pTransfer, sizeof(SINGLE_TRANSFER),   
				UsbRxBuffer, 512,   
				&dwReturnBytes, &RxOverlapped);  

			if (bDeviceIoControlResult == TRUE)
			{
				for(dw = 0; dw < dwReturnBytes; dw++)
				{
					UsbRxByte(UsbRxBuffer[dw]);
				}
			}
			else
			{
				bBreak = FALSE;
				if ((dwLastError=GetLastError()) != ERROR_IO_PENDING)
				{
				}
				while(1)
				{
					switch(WaitForSingleObject(RxOverlapped.hEvent, 100))
					{
						case WAIT_OBJECT_0:
							bGetOverolappedResultResult = GetOverlappedResult(hUsbDevice_, &RxOverlapped, 
																	&dwReturnBytes, TRUE);
							for(dw = 0; dw < dwReturnBytes; dw++)
							{
								UsbRxByte(UsbRxBuffer[dw]);
							}
							bBreak = TRUE;
							break;
						case WAIT_TIMEOUT:
							bGetOverolappedResultResult = GetOverlappedResult(hUsbDevice_, &RxOverlapped, 
																	&dwReturnBytes, FALSE);
							for(dw = 0; dw<dwReturnBytes; dw++)
							{
								UsbRxByte(UsbRxBuffer[dw]);
							}
							break;
					}
					if (bBreak==TRUE)
					{
						break;
					}
					if (bUsbReceiverEnable_ == FALSE)
					{
						break;
					}
				}
			}
			CloseHandle(RxOverlapped.hEvent);
		}

		{
			DWORD dwBytes = 0;
			UCHAR Address = USB_EPIN_ADDRESS;
			DeviceIoControl(
				hUsbDevice_,
				IOCTL_ADAPT_ABORT_PIPE,
				&Address, sizeof (UCHAR),
				NULL, 0,
			&dwBytes, NULL);
		}

		bUsbReceiverEnabled_ = FALSE;
		delete [] pCtrlBuf;
		//return 0;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in RxThread()");
		throw;
	}
}
//====================================================================
//====================================================================
void CUsb::UsbSendOverlapped(BYTE *pbData, DWORD dwBytes, DWORD dwTimeout)
{
	try
	{
		BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; 
		SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;  

		BOOL bDeviceIoControlResult;
		DWORD dwReturnBytes;  

		OVERLAPPED TxOverlapped;
		BYTE EndpointAddress;

		memset(pCtrlBuf, 0, sizeof(SINGLE_TRANSFER));  
		pTransfer->ucEndpointAddress = USB_EPOUT_ADDRESS;  

		memset(&TxOverlapped, 0, sizeof(TxOverlapped));
		TxOverlapped.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);


		bDeviceIoControlResult = DeviceIoControl (hUsbDevice_,  
					IOCTL_ADAPT_SEND_NON_EP0_DIRECT,
					pTransfer, sizeof(SINGLE_TRANSFER),   
					pbData, dwBytes,   
					&dwReturnBytes, &TxOverlapped);  

		if (bDeviceIoControlResult == TRUE)
		{
			//dwUsbTxBytesCounter+=dwReturnBytes;
		}
		else
		{
			WaitForSingleObject(TxOverlapped.hEvent, dwTimeout);
			GetOverlappedResult(hUsbDevice_, &TxOverlapped, &dwReturnBytes, TRUE);

			//dwUsbTxBytesCounter+=dwReturnBytes;

			EndpointAddress = USB_EPOUT_ADDRESS;
			bDeviceIoControlResult = DeviceIoControl(hUsbDevice_,
							IOCTL_ADAPT_ABORT_PIPE,
							&EndpointAddress, sizeof(BYTE), NULL, 0,
							&dwReturnBytes, NULL); 
		}

		delete [] pCtrlBuf;							
		CloseHandle(TxOverlapped.hEvent);
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in UsbSendOverlapped()");
		throw;
	}
}
//====================================================================
//====================================================================
void CUsb::UsbRxByte(BYTE bNewByte)
{
	try
	{
		if (protocolState_ == PROTOCOL_PACKET_BEING_PROCESSED)	
		{
			Sleep(100);
			while(1)
			{
				if (protocolState_ != PROTOCOL_PACKET_BEING_PROCESSED)	break;
			}
		}
		//__________________________________________________________________
		if (bNewByte == SLIP_END)
		{	
			stuffState_ = STUFF_END;
		}	
		//__________________________________________________________________
		switch( stuffState_ )
		{
			//-------------------------------------------------------------	
			//case STUFF_END :
				//protocolState_ = PROTOCOL_LISTENING;
			case STUFF_IDLE :
		
				switch ( bNewByte )
				{
					//-------------------------------------------------------------	
					case SLIP_ESC:
						stuffState_ = STUFF_BYTE1;		
						break;
					//-------------------------------------------------------------	
					default:
						stuffState_ = STUFF_NEWBYTE;
						break;
				}	
				break;
			//-------------------------------------------------------------	
			case STUFF_BYTE1 :
				stuffState_ = STUFF_IDLE;
				switch ( bNewByte )
				{
					//-------------------------------------------------------------	
					case SLIP_ESC_END:
						bNewByte = SLIP_END;
						stuffState_ = STUFF_NEWBYTE;
						break;
					//-------------------------------------------------------------	
					case SLIP_ESC_ESC:
						bNewByte = SLIP_ESC;
						stuffState_ =STUFF_NEWBYTE;
						break;
						//-------------------------------------------------------------	
					default:
						protocolState_ = PROTOCOL_LISTENING;
						break;
					}	
					break;
			//-------------------------------------------------------------	
			case STUFF_END :
				stuffState_ = STUFF_NEWBYTE;
				protocolState_ = PROTOCOL_LISTENING;
				break;
			//-------------------------------------------------------------	
			default :
				stuffState_ = STUFF_IDLE;
				protocolState_ = PROTOCOL_LISTENING;
				break;
			//-------------------------------------------------------------	
			}
		//__________________________________________________________________
		if ( stuffState_ == STUFF_NEWBYTE )
		{
				stuffState_ = STUFF_IDLE;
				switch( protocolState_ )
				{
					//-------------------------------------------------------------	
					case PROTOCOL_LISTENING:					
						if (bNewByte == SLIP_END)
							protocolState_ = PROTOCOL_ADDRESS0_EXPECTED;
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_ADDRESS0_EXPECTED:
						/*
						if (bNewByte == SLIP_END)
						{	
							protocolState_ = PROTOCOL_ADDRESS0_EXPECTED;
							break;
						}
						*/
						xActualReply_.dwAddress = 0x000000FF & (DWORD)bNewByte;
						protocolState_ = PROTOCOL_ADDRESS1_EXPECTED;
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_ADDRESS1_EXPECTED:
						xActualReply_.dwAddress |= (DWORD)(bNewByte << 8);
						protocolState_ = PROTOCOL_ADDRESS2_EXPECTED;
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_ADDRESS2_EXPECTED:
						xActualReply_.dwAddress |= (DWORD)(bNewByte << 16);
						protocolState_ = PROTOCOL_ADDRESS3_EXPECTED;
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_ADDRESS3_EXPECTED:
						xActualReply_.dwAddress |= (DWORD)(bNewByte << 24);
						if( (0xFF000000 & xActualReply_.dwAddress) == ((TYPE_ENERGOMONITOR32 << 28)|(TYPE_PC << 24)) )
						{
							protocolState_ = PROTOCOL_COMMAND0_EXPECTED;
						}
						else
						{
							protocolState_ = PROTOCOL_COMMAND0_EXPECTED;
						}
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_COMMAND0_EXPECTED:
						xActualReply_.wCommand = (WORD)bNewByte;
						protocolState_ = PROTOCOL_COMMAND1_EXPECTED;
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_COMMAND1_EXPECTED:
						xActualReply_.wCommand |= ((WORD)bNewByte << 8);
						protocolState_ = PROTOCOL_LENGTH0_EXPECTED;
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_LENGTH0_EXPECTED:
						xActualReply_.wLength = 0x00FF & (WORD)bNewByte;
						protocolState_ = PROTOCOL_LENGTH1_EXPECTED;
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_LENGTH1_EXPECTED:
						xActualReply_.wLength |= ((WORD)bNewByte << 8);
						if ( xActualReply_.wLength == 0 )
						{
							protocolState_ = PROTOCOL_CRC0_EXPECTED;
						}
						else
						{
							protocolState_ = PROTOCOL_DATA_EXPECTED;
							xActualReply_.wCounter = 0;
						}
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_DATA_EXPECTED:
						xActualReply_.bData[ xActualReply_.wCounter ] = bNewByte;
						xActualReply_.wCounter++;
						if ( xActualReply_.wCounter == xActualReply_.wLength )
						{
							protocolState_ = PROTOCOL_CRC0_EXPECTED;
						}
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_CRC0_EXPECTED:
						xActualReply_.wCRC = 0x00FF & (WORD)bNewByte;
						protocolState_ = PROTOCOL_CRC1_EXPECTED;
						break;
					//-------------------------------------------------------------	
					case PROTOCOL_CRC1_EXPECTED:
						xActualReply_.wCRC |= (WORD)bNewByte << 8;
						WORD wTemp;
						WORD i;
						wTemp = CRC16_SEED;
						crc16( (BYTE)(((xActualReply_.dwAddress) >> (8*0)) & 0xFF)  , &wTemp );
						crc16( (BYTE)(((xActualReply_.dwAddress) >> (8*1)) & 0xFF)  , &wTemp );
						crc16( (BYTE)(((xActualReply_.dwAddress) >> (8*2)) & 0xFF)  , &wTemp );
						crc16( (BYTE)(((xActualReply_.dwAddress) >> (8*3)) & 0xFF)  , &wTemp );
						crc16( (BYTE)(((xActualReply_.wCommand) >> (8*0)) & 0xFF)  , &wTemp );
						crc16( (BYTE)(((xActualReply_.wCommand) >> (8*1)) & 0xFF)  , &wTemp );
						crc16( (BYTE)(((xActualReply_.wLength) >> (8*0)) & 0xFF)  , &wTemp );
						crc16( (BYTE)(((xActualReply_.wLength) >> (8*1)) & 0xFF)  , &wTemp );

						for(i = 0; i < (xActualReply_.wLength); i++)
							crc16( xActualReply_.bData[i] , &wTemp );
						crc16( (BYTE)(((xActualReply_.wCRC) >> (8*0)) & 0xFF)  , &wTemp );
						crc16( (BYTE)(((xActualReply_.wCRC) >> (8*1)) & 0xFF)  , &wTemp );
						if ( wTemp == 0 )
						{
							protocolState_ = PROTOCOL_PACKET_BEING_PROCESSED;
							SetEvent(hEventPacketReceived_);
						}
						else
						{
							protocolState_ = PROTOCOL_LISTENING;
						}
						break;
				}
		}
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in RxThread()");
		throw;
	}
}
//================================================================
//================================================================
EUsbResult CUsb::UsbCommunication(		
	BOOL boolEmptyRequest,
	DWORD dwTimeoutSecs,
	WORD *wReplyAddress,
	WORD *wReplyCommand,
	WORD *wReplyLength,
	void* pvReplyData,
	WORD wMaxReplyLength,
	WORD wCommand,
	WORD wLength,
	void* pvData)
{
	try
	{
		std::stringstream ss;
		EUsbResult res;

		for(short cntErrors = 0; cntErrors < 3; ++cntErrors)
		{
			res = USBRESULT_COMMERROR;
			SetTimeoutCounter(dwTimeoutSecs);

			if (boolEmptyRequest == FALSE)
			{
				TUsbRequest SRequest;
				SRequest.dwAddress = (DWORD)(0xFFFF) | ((TYPE_ENERGOTESTERA&0x000F)<<(24)) | ((TYPE_PC&0x000F)<<(28));
				SRequest.wCommand = wCommand;
				SRequest.wLength = wLength;
				{
					int ByteCount;
					BYTE *pbData;
					pbData = (BYTE*)pvData;
					for(ByteCount = 0; ByteCount<wLength; ByteCount++)
					{
						SRequest.bData[ByteCount] = *pbData++;			
					}
				}

				ResetEvent( hEventCommunicationTimeout_ );
				ResetEvent( hEventPacketReceived_ );
				UsbSendRequest(&SRequest);

				if(wCommand != COMMAND_ReadRegistrationArchiveByIndex &&
								wCommand != COMMAND_ReadAverageArchive3SecByIndex &&
								wCommand != COMMAND_ReadAverageArchive10MinByIndex)
				{
					ss.str("");
					ss << "UsbCommunication(): send command " << EmService::GetCommandText(wCommand);
					ss << "  " << EmService::GetCurrentDateTime();
					EmService::WriteToLogGeneral(ss.str());
				}
			}

			*wReplyAddress = 0xFFFF;

			switch (MsgWaitForMultipleObjects(3 , (LPHANDLE)hEventsArray_, FALSE, INFINITE, 0))
			{
				case (WAIT_OBJECT_0 + 0):
					ResetEvent( hEventCommunicationTimeout_ );
					ResetTimeoutCounter();
					EmService::WriteToLogFailedWithDate("UsbCommunication(): Timeout");
					res = USBRESULT_TIMEOUT;
					break;
		
				case (WAIT_OBJECT_0 + 1):
					ResetEvent( hEventPacketReceived_ );
					ResetTimeoutCounter();
					protocolState_ = PROTOCOL_LISTENING;
					*wReplyAddress = (xActualReply_.dwAddress) & 0xFFFF;

					switch(xActualReply_.wCommand)
					{
						case COMMAND_CRC_ERROR:
							//return USBRESULT_CRCERROR;
							res = USBRESULT_CRCERROR;
							break;

						case COMMAND_UNKNOWN_COMMAND:
						case COMMAND_BAD_DATA:
							//return USBRESULT_COMMERROR;
							res = USBRESULT_COMMERROR;
							break;

						default:
							if (wReplyCommand!=NULL)
							{
								*wReplyCommand = xActualReply_.wCommand;
							}
							if (wReplyLength!=NULL)
							{
								*wReplyLength = xActualReply_.wLength;
							}
							if (pvReplyData != NULL)
							{
								int ByteCount;
								BYTE *pbData;
								pbData = (BYTE*)pvReplyData;
								if (wMaxReplyLength >= xActualReply_.wLength)
								{
									for(ByteCount = 0; ByteCount<xActualReply_.wLength; ByteCount++)
									{
										*pbData++ = xActualReply_.bData[ByteCount];			
									}
								}
								else
								{
									for(ByteCount = 0 ; ByteCount < wMaxReplyLength; ByteCount++)
									{
										*pbData++ = xActualReply_.bData[ByteCount];			
									}
								}
							}

							if(wCommand != COMMAND_ReadRegistrationArchiveByIndex &&
								wCommand != COMMAND_ReadAverageArchive3SecByIndex &&
								wCommand != COMMAND_ReadAverageArchive10MinByIndex)
							{
								ss.str("");
								ss << "UsbCommunication(): packet received " << EmService::GetCommandText(wCommand);
								ss << "  " << EmService::GetCurrentDateTime();
								EmService::WriteToLogGeneral(ss.str());
							}

							return USBRESULT_OK;
					}
					break;
		
				case (WAIT_OBJECT_0 + 2):
					ResetEvent(hEventDisconnected_);
					ResetTimeoutCounter();
					return USBRESULT_CANCELLED;

				default:
					EmService::WriteToLogFailed("UsbCommunication(): unknown event");
			}

			EmService::WriteToLogGeneralWithDate(
				"UsbCommunication(): repeated request will be done");
		}

		return res;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in UsbCommunication()");
		throw;
	}
}
//================================================================
//================================================================
void CUsb::UsbSendRequestCore(BYTE bByte, BYTE *pBuffer, DWORD *pdwPtr,WORD *pwCrc)
{
	try
	{
		switch(bByte)
		{
			case SLIP_END:
				pBuffer[(*pdwPtr)++] = (SLIP_ESC);
				pBuffer[(*pdwPtr)++] = (SLIP_ESC_END);
				break;
			case SLIP_ESC:
				pBuffer[(*pdwPtr)++] = (SLIP_ESC);
				pBuffer[(*pdwPtr)++] = (SLIP_ESC_ESC);
				break;
			default:
				pBuffer[(*pdwPtr)++] = (bByte);
				break;
		}
		if (pwCrc != NULL)
		{
			crc16(bByte, pwCrc);
		}
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in UsbSendRequestCore()");
		throw;
	}
}

void CUsb::UsbSendRequest(TUsbRequest *Request)
{
	try
	{
		BYTE abSendBuffer[8192];
		WORD wCrc;
		DWORD dwPtr;
		WORD w;

		wCrc = CRC16_SEED;
		dwPtr = 0;

		// start
		abSendBuffer[dwPtr++] = SLIP_END;
		abSendBuffer[dwPtr++] = SLIP_END;
		abSendBuffer[dwPtr++] = SLIP_END;
		abSendBuffer[dwPtr++] = SLIP_END;
		abSendBuffer[dwPtr++] = SLIP_END;
		abSendBuffer[dwPtr++] = SLIP_END;

		// address
		UsbSendRequestCore( (BYTE)(((Request->dwAddress) >> (8*0)) & 0xFF), abSendBuffer, &dwPtr, &wCrc);
		UsbSendRequestCore( (BYTE)(((Request->dwAddress) >> (8*1)) & 0xFF), abSendBuffer, &dwPtr, &wCrc);
		UsbSendRequestCore( (BYTE)(((Request->dwAddress) >> (8*2)) & 0xFF), abSendBuffer, &dwPtr, &wCrc);
		UsbSendRequestCore( (BYTE)(((Request->dwAddress) >> (8*3)) & 0xFF), abSendBuffer, &dwPtr, &wCrc);

		// command
		UsbSendRequestCore( (BYTE)(((Request->wCommand) >> (8*0)) & 0xFF), abSendBuffer, &dwPtr, &wCrc);
		UsbSendRequestCore( (BYTE)(((Request->wCommand) >> (8*1)) & 0xFF), abSendBuffer, &dwPtr, &wCrc);

		// length
		UsbSendRequestCore( (BYTE)(((Request->wLength) >> (8*0)) & 0xFF), abSendBuffer, &dwPtr, &wCrc);
		UsbSendRequestCore( (BYTE)(((Request->wLength) >> (8*1)) & 0xFF), abSendBuffer, &dwPtr, &wCrc);

		// data
		for(w = 0; w < (Request->wLength); w++)
		{
			UsbSendRequestCore( Request->bData[w], abSendBuffer, &dwPtr, &wCrc);
		}

		// crc
		Request->wCRC = wCrc;
		UsbSendRequestCore( (BYTE)(((Request->wCRC) >> (8 * 0)) & 0xFF), abSendBuffer ,&dwPtr, NULL);
		UsbSendRequestCore( (BYTE)(((Request->wCRC) >> (8 * 1)) & 0xFF), abSendBuffer ,&dwPtr, NULL);

		// send
		UsbSendOverlapped(abSendBuffer, dwPtr, 100);
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in UsbSendRequest()");
		throw;
	}
}

//================================================================
//================================================================
void CUsb::crc16(BYTE byte, WORD *crc)
{
	try
	{
		WORD temp = ((byte & 0x00FF) ^ *crc) & 0xFF;
 		*crc >>= 8;
		*crc ^= CRC16Table[temp];
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in crc16()");
		throw;
	}
}



