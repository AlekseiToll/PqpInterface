#define _CRT_SECURE_NO_WARNINGS

#include "stdafx.h"
#include "Device.h"
#include "EmUsb.h"
#include "RegistrationManager.h"

CDevice::CDevice(EReadMode mode, std::string dataPath): mode_(mode), dataPath_(dataPath)
{
}

bool CDevice::Connect()
{
	if (usb_.UsbConnect() == FALSE)
	{
		EmService::WriteToLogFailed("CDevice::Connect: UsbConnect() == FALSE");
		// msg to main program ???????????????????
		usb_.UsbDisconnect();
		return false;
	}
}

void CDevice::Disconnect()
{
	usb_.UsbDisconnect();
}

bool CDevice::StartReading()
{
	return true; //dummy
}

bool CDevice::StartReading(RegistrationManager& regManager)
{
	try
	{
		for(int iReg = 0; iReg < regManager.VecRegistrations.size(); ++iReg)
		{
			EmService::WriteToLogGeneral("Reading registration start " + EmService::NumberToString(iReg));
			CRegistration curRegistration(regManager.VecRegistrations[iReg].regId, dataPath_, &usb_);
			/*curRegistration.ReadRegistrationByIndex(
				regManager.VecRegistrations[iReg].vecPqpIndexes,
				regManager.VecRegistrations[iReg].avgTypes,
				regManager.VecRegistrations[iReg].readDns);*/
			curRegistration.ReadRegistrationByIndex(regManager, iReg);
			EmService::WriteToLogGeneral("Reading registration end " + EmService::NumberToString(iReg));
		}
		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in StartReading()");
		throw;
	}
}

