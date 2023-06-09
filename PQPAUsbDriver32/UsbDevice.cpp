
#include "targetver.h"
#define WIN32_LEAN_AND_MEAN

#include <stdafx.h>
#include <windows.h>
#include <stdio.h>

#pragma comment(lib, "Setupapi.lib")
#include <Setupapi.h> // !!! Setupapi.lib must be linked !!!
#include "cyioctl.h"
#include <winioctl.h>
#include "UsbDevice.h"

HANDLE hUsbDevice;
DWORD dwUsbTxBytesCounter = 0;
DWORD dwUsbRxBytesCounter = 0;
DWORD dwReturnBytes = 0;
BYTE UsbRxBuffer[10 + 32768 + 2];

FILE * pFile; //debug

// Функция реализует подключение к USB-порту.
BOOL UsbConnect()
{
	HDEVINFO hwDeviceInfo = SetupDiGetClassDevs((LPGUID) &CYUSBDRV_GUID, NULL, 
											    NULL, DIGCF_PRESENT | DIGCF_INTERFACEDEVICE); 

	SP_DEVINFO_DATA devInfoData; 
	SP_DEVICE_INTERFACE_DATA  devInterfaceData; 
	PSP_INTERFACE_DEVICE_DETAIL_DATA functionClassDeviceData; 
	int deviceNumber = 0;
	DWORD requiredLength = 0; 
	BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)];
	SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;  

	if (hwDeviceInfo != INVALID_HANDLE_VALUE)
	{ 
		devInterfaceData.cbSize = sizeof(devInterfaceData); 

		if (SetupDiEnumDeviceInterfaces(hwDeviceInfo, 0, (LPGUID) &CYUSBDRV_GUID, deviceNumber, &devInterfaceData))
		{ 
			SetupDiGetInterfaceDeviceDetail ( hwDeviceInfo, &devInterfaceData, NULL, 0, &requiredLength, NULL); 
			ULONG predictedLength = requiredLength; 

			functionClassDeviceData = (PSP_INTERFACE_DEVICE_DETAIL_DATA) malloc(predictedLength);
			functionClassDeviceData->cbSize = sizeof(SP_INTERFACE_DEVICE_DETAIL_DATA); 
			devInfoData.cbSize = sizeof(devInfoData); 

			if (SetupDiGetInterfaceDeviceDetail(hwDeviceInfo, &devInterfaceData, functionClassDeviceData, predictedLength, &requiredLength, &devInfoData))
			{ 
				hUsbDevice = CreateFile(functionClassDeviceData->DevicePath, GENERIC_WRITE | GENERIC_READ,
										FILE_SHARE_WRITE | FILE_SHARE_READ,	NULL, 
										OPEN_EXISTING, FILE_FLAG_OVERLAPPED, NULL); 

				free(functionClassDeviceData); 
				SetupDiDestroyDeviceInfoList(hwDeviceInfo); 
				
				// Сбрасываем счетчик принятых байт.
				dwUsbRxBytesCounter = 0;

				if(pCtrlBuf != NULL) delete[] pCtrlBuf;
				return TRUE;
			}
		}
	}
	if(pCtrlBuf != NULL) delete[] pCtrlBuf;
	return FALSE;
}

// Функция выполняет отключение USB-порта.
void UsbDisconnect()
{
	if(hUsbDevice != NULL) CloseHandle(hUsbDevice);
	hUsbDevice = NULL;
}

//void WriteToLog(int x)//debug
//{
//	pFile = fopen ("logcpp.txt","a");//debug
//	if (pFile!=NULL) 
//{
//		if(x == 0) fputs ("WAIT_OBJECT_0\n", pFile);
//		else if(x == 1) fputs ("WAIT_TIMEOUT\n", pFile);
//		else if(x == 2) fputs ("switch default\n", pFile);
//		else if(x == 3) fputs ("WAIT_TIMEOUT != 0\n", pFile);
//		else if(x == 4) fputs ("Write data OK\n", pFile);
//		else if(x == 5) fputs ("Write data failed\n", pFile);
//		else fputs ("after DeviceIoControl\n", pFile);
//	}
//	if (pFile!=NULL) fclose (pFile);//debug
//}

// Функция считывает данные из USB-порта.
int ReadData(unsigned char * pRx, int timeout)
{
	// Проверяем, выполняли ли чтение из порта.
	// Если счетчик принятых байт нулевой, то читаем сразу 
	// весь буфер принятых от прибора байт данных.
	if (dwUsbRxBytesCounter == 0) 
	{
		memset(UsbRxBuffer, 0, 10 + 32768 + 2);

		OVERLAPPED RxOverlapped;
		BOOL bBreak;
		DWORD dwLastError;
		BOOL bGetOverolappedResultResult;
		int cntTimeout = 0;  // счетчик неудачных чтений

		BOOL bDeviceIoControlResult;

		BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; 
		SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;  
		memset(pCtrlBuf, 0, sizeof(SINGLE_TRANSFER));  
		pTransfer->ucEndpointAddress = USB_EPIN_ADDRESS; 

		memset(&RxOverlapped, 0, sizeof(RxOverlapped));
		RxOverlapped.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

	    // Инициализируем кол-во всех принятых байт.
		dwReturnBytes = 0;

		// Считываем буфер порта и сохраняем в массиве-приемнике.
		bDeviceIoControlResult = DeviceIoControl(hUsbDevice, IOCTL_ADAPT_SEND_NON_EP0_DIRECT,
				                                 pTransfer, sizeof(SINGLE_TRANSFER),   
											     UsbRxBuffer, 10 + 32768 + 2, 
												 &dwReturnBytes, &RxOverlapped);

		if(dwReturnBytes > (10 + 32768 + 2))
		{
			dwReturnBytes = 10 + 32768 + 2;
		}

		// Обработка в зависимости от результата чтения.
		if (bDeviceIoControlResult == TRUE)
		{
			cntTimeout++;

			// Копируем первый принятый байт и увеличиваем счетчик принятых байт.
			*pRx = UsbRxBuffer[dwUsbRxBytesCounter];
		    dwUsbRxBytesCounter++;
			// Если скопированы все принятые байты, то сбрасываем счетчик
			// (на всякий случай).
		    if (dwUsbRxBytesCounter >= dwReturnBytes)
               dwUsbRxBytesCounter = 0;
		}
		else
		{
			bBreak = FALSE;
			if ((dwLastError=GetLastError()) != ERROR_IO_PENDING)
			{
			}
			while(1)
			{
				//if (terminateRxThread_) break;

				switch(WaitForSingleObject(RxOverlapped.hEvent, 100))
				{
					case WAIT_OBJECT_0:
						//WriteToLog(0);
						cntTimeout = 0;
						bGetOverolappedResultResult = GetOverlappedResult(hUsbDevice, &RxOverlapped, &dwReturnBytes, TRUE);
						/*for(dw = 0; dw < dwReturnBytes; dw++)
						{
							RxFunction(UsbRxBuffer[dw]);
						}*/
						*pRx = UsbRxBuffer[dwUsbRxBytesCounter];
						dwUsbRxBytesCounter++;
						// Если скопированы все принятые байты, то сбрасываем счетчик
						// (на всякий случай).
						if (dwUsbRxBytesCounter >= dwReturnBytes)
						   dwUsbRxBytesCounter = 0;

						bBreak = TRUE;
						break;

					case WAIT_TIMEOUT:
						//WriteToLog(1);
						bGetOverolappedResultResult = GetOverlappedResult(hUsbDevice, &RxOverlapped, &dwReturnBytes, FALSE);
						/*for(dw = 0; dw < dwReturnBytes; dw++)
						{
							RxFunction(UsbRxBuffer[dw]);
						}*/
						if(dwReturnBytes == 0) { cntTimeout++; Sleep(100); }
						else { bBreak = TRUE; /*WriteToLog(3);*/ }

						if(cntTimeout > timeout)
						{
							cntTimeout = 0;
							delete [] pCtrlBuf;							
							return -3;
						}

						*pRx = UsbRxBuffer[dwUsbRxBytesCounter];
						dwUsbRxBytesCounter++;
						// Если скопированы все принятые байты, то сбрасываем счетчик
						// (на всякий случай).
						if (dwUsbRxBytesCounter >= dwReturnBytes)
						   dwUsbRxBytesCounter = 0;

						break;
					default:
						//WriteToLog(2);
						//dw = 0;
						break;
				}
				if (bBreak == TRUE)
				{
					break;
				}
				//if (connectStatus_ == CONNECTSTATUS_FORCEDISCONNECT)
				//{
				//	InterlockedExchange((LONG*)(&connectStatus_), CONNECTSTATUS_IDLE);
				//	EndpointAddress = USB_EPIN_ADDRESS;
				//	bDeviceIoControlResult = DeviceIoControl(hUsbDevice_,
				//					IOCTL_ADAPT_ABORT_PIPE,
				//					&EndpointAddress,sizeof(BYTE),NULL,0,
				//					&dwReturnBytes,NULL); 
				//	delete [] pCtrlBuf;
				//	pCtrlBuf = 0;
				//	CloseHandle(hUsbDevice_);						
				//	//OnDisconnect();
				//	return;
				//}
			}
		}
		CloseHandle(RxOverlapped.hEvent);

	    // Отсоединяемся от USB-канала.
		DWORD dwBytes = 0;
		UCHAR Address = USB_EPIN_ADDRESS;
		DeviceIoControl(hUsbDevice, IOCTL_ADAPT_ABORT_PIPE,	&Address, sizeof(UCHAR),
		   			    NULL, 0, &dwBytes, NULL);

		// Освобождаем память.
		delete [] pCtrlBuf;	

		// Если обмена не было, то выходим с ошибкой.
		//if (!bDeviceIoControlResult)
		//{
            ////delete [] pCtrlBuf;							
		//	return -3;
		//}
	}
	else // Если счетчик ненулевой, то имитируем чтение - просто выдаем 
	{    // ранее принятые байты.
		*pRx = UsbRxBuffer[dwUsbRxBytesCounter];
		dwUsbRxBytesCounter++;
		//Sleep(10);
	    // Если скопированы все принятые байты, то сбрасываем счетчик
		// (на всякий случай).
		if (dwUsbRxBytesCounter >= dwReturnBytes)
            dwUsbRxBytesCounter = 0;
	}						
	// Возвращаем код успешного чтения.
	return 0;
}

//// Функция считывает данные из USB-порта.
//int ReadData(unsigned char * pRx)
//{
//	// Проверяем, выполняли ли чтение из порта.
//	// Если счетчик принятых байт нулевой, то читаем сразу 
//	// весь буфер принятых от прибора байт данных.
//	if (dwUsbRxBytesCounter == 0) 
//	{
//		BOOL bDeviceIoControlResult;
//
//		BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; 
//		SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;  
//
//		memset(pCtrlBuf, 0, sizeof(SINGLE_TRANSFER));  
//		pTransfer->ucEndpointAddress = USB_EPIN_ADDRESS;  
//
//	    // Инициализируем кол-во всех принятых байт.
//		dwReturnBytes = 0;
//
//		WriteToLog(0);//debug
//
//		// Считываем буфер порта и сохраняем в массиве-приемнике.
//		bDeviceIoControlResult = DeviceIoControl(hUsbDevice, IOCTL_ADAPT_SEND_NON_EP0_DIRECT,
//				                                 pTransfer, sizeof(SINGLE_TRANSFER),   
//											     UsbRxBuffer, 10 + 32768 + 2, 
//												 &dwReturnBytes, NULL);
//		WriteToLog(1);//debug
//
//		if(dwReturnBytes > (10 + 32768 + 2))
//		{
//			dwReturnBytes = 10 + 32768 + 2;
//		}
//
//		// Обработка в зависимости от результата чтения.
//		if (bDeviceIoControlResult)
//		{   // Копируем первый принятый байт и увеличиваем счетчик принятых байт.
//			*pRx = UsbRxBuffer[dwUsbRxBytesCounter];
//		    dwUsbRxBytesCounter++;
//			// Если скопированы все принятые байты, то сбрасываем счетчик
//			// (на всякий случай).
//		    if (dwUsbRxBytesCounter >= dwReturnBytes)
//               dwUsbRxBytesCounter = 0;
//		}
//	    // Отсоединяемся от USB-канала.
//		DWORD dwBytes = 0;
//		UCHAR Address = USB_EPIN_ADDRESS;
//		DeviceIoControl(hUsbDevice, IOCTL_ADAPT_ABORT_PIPE,	&Address, sizeof(UCHAR),
//		   			    NULL, 0, &dwBytes, NULL);
//
//		// Освобождаем память.
//		delete [] pCtrlBuf;	
//
//		// Если обмена не было, то выходим с ошибкой.
//		if (!bDeviceIoControlResult)
//		{
//            //delete [] pCtrlBuf;							
//			return -3;
//		}
//	}
//	else // Если счетчик ненулевой, то имитируем чтение - просто выдаем 
//	{    // ранее принятые байты.
//		*pRx = UsbRxBuffer[dwUsbRxBytesCounter];
//		dwUsbRxBytesCounter++;
//		//Sleep(10);
//	    // Если скопированы все принятые байты, то сбрасываем счетчик
//		// (на всякий случай).
//		if (dwUsbRxBytesCounter >= dwReturnBytes)
//            dwUsbRxBytesCounter = 0;
//	}						
//	// Возвращаем код успешного чтения.
//	return 0;
//}

// Функция выполняет отправку данных в USB-порт.
BOOL SendData(BYTE *pbData, DWORD dwBytes, DWORD dwTimeout)
{
	BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; 
	SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;  

	BOOL bDeviceIoControlResult;
	DWORD dwReturnBytes;
	BYTE EndpointAddress;

	memset(pCtrlBuf, 0, sizeof(SINGLE_TRANSFER));
	pTransfer->ucEndpointAddress = USB_EPOUT_ADDRESS;  

	// Отправляем в USB-порт байты.
	bDeviceIoControlResult = DeviceIoControl(hUsbDevice, IOCTL_ADAPT_SEND_NON_EP0_DIRECT,
						                     pTransfer, sizeof(SINGLE_TRANSFER),   
											 pbData, dwBytes, &dwReturnBytes, NULL);
	
	// Если отправка выполнена с ошибкой, то отключаемся от USB-канала.
	if (!bDeviceIoControlResult)
	{   
		EndpointAddress = USB_EPOUT_ADDRESS;
		bDeviceIoControlResult = DeviceIoControl(hUsbDevice, IOCTL_ADAPT_ABORT_PIPE,
									             &EndpointAddress, sizeof(BYTE), NULL, 0,
												 &dwReturnBytes, NULL);
	}
	// Освобождаем память.
	delete [] pCtrlBuf;
	// Возвращаем флаг результата посылки.
	return bDeviceIoControlResult;
}