
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
	BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; // ????????????? эта память не освобождается
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

				return TRUE;
			}
		}
	}
	return FALSE;
}

// Функция выполняет отключение USB-порта.
void UsbDisconnect()
{
	CloseHandle(hUsbDevice);
}

// Функция считывает данные из USB-порта.
int ReadData(unsigned char * pRx)
{
	// Проверяем, выполняли ли чтение из порта.
	// Если счетчик принятых байт нулевой, то читаем сразу 
	// весь буфер принятых от прибора байт данных.
	if (dwUsbRxBytesCounter == 0) 
	{
		BOOL bDeviceIoControlResult;

		BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; 
		SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;  

		memset(pCtrlBuf, 0, sizeof(SINGLE_TRANSFER));  
		pTransfer->ucEndpointAddress = USB_EPIN_ADDRESS;  

	    // Инициализируем кол-во всех принятых байт.
		dwReturnBytes = 0;
		// Считываем буфер порта и сохраняем в массиве-приемнике.
		bDeviceIoControlResult = DeviceIoControl(hUsbDevice, IOCTL_ADAPT_SEND_NON_EP0_DIRECT,
				                                 pTransfer, sizeof(SINGLE_TRANSFER),   
											     UsbRxBuffer, 10 + 32768 + 2, 
												 &dwReturnBytes, NULL);  // dwReturnBytes = 1759977384, 587213073, 141354896
		if(dwReturnBytes > 65536) dwReturnBytes = 0;//??????????????????????????my code

		// Обработка в зависимости от результата чтения.
		if (bDeviceIoControlResult)
		{   // Копируем первый принятый байт и увеличиваем счетчик принятых байт.
			*pRx = UsbRxBuffer[dwUsbRxBytesCounter];
		    dwUsbRxBytesCounter++;
			// Если скопированы все принятые байты, то сбрасываем счетчик
			// (на всякий случай).
		    if (dwUsbRxBytesCounter >= dwReturnBytes)
               dwUsbRxBytesCounter = 0;
		}
	    // Отсоединяемся от USB-канала.
		DWORD dwBytes = 0;
		UCHAR Address = USB_EPIN_ADDRESS;
		DeviceIoControl(hUsbDevice, IOCTL_ADAPT_ABORT_PIPE,	&Address, sizeof(UCHAR),
		   			    NULL, 0, &dwBytes, NULL);

		// Освобождаем память.
		delete [] pCtrlBuf;	

		// Если обмена не было, то выходим с ошибкой.
		if (!bDeviceIoControlResult)
		{
            //delete [] pCtrlBuf;							
			return -3;
		}
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
//	BOOL bDeviceIoControlResult;
//
//	BYTE *pCtrlBuf = new BYTE[sizeof(SINGLE_TRANSFER)]; 
//	SINGLE_TRANSFER *pTransfer = (SINGLE_TRANSFER*)pCtrlBuf;  
//
//	memset(pCtrlBuf, 0, sizeof(SINGLE_TRANSFER));  
//	pTransfer->ucEndpointAddress = USB_EPIN_ADDRESS;  
//
//	// Проверяем, выполняли ли чтение из порта.
//	// Если счетчик принятых байт нулевой, то читаем сразу 
//	// весь буфер принятых от прибора байт данных.
//	if (dwUsbRxBytesCounter == 0)
//	{   // Инициализируем кол-во всех принятых байт.
//		dwReturnBytes = 0;
//		// Считываем буфер порта и сохраняем в массиве-приемнике.
//		bDeviceIoControlResult = DeviceIoControl(hUsbDevice, IOCTL_ADAPT_SEND_NON_EP0_DIRECT,
//				                                 pTransfer, sizeof(SINGLE_TRANSFER),   
//											     UsbRxBuffer, 10 + 32768 + 2, 
//												 &dwReturnBytes, NULL);  // dwReturnBytes = 1759977384, 587213073, 141354896
//		if(dwReturnBytes > 65536) dwReturnBytes = 0;//??????????????????????????my code
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
//		// Если обмена не было, то выходим с ошибкой.
//		if (!bDeviceIoControlResult)
//		{	// Освобождаем память.
//            delete [] pCtrlBuf;							
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
//	// Освобождаем память.
//    delete [] pCtrlBuf;							
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