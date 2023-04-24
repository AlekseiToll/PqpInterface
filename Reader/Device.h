#ifndef DEVICE_H
#define DEVICE_H

#include "ReadRegistration.h"

class RegistrationManager;

class CDevice
{
	CUsb usb_;
	EReadMode mode_;
	std::string dataPath_;		// path to store data that have been read

public:
	CDevice(EReadMode mode, std::string dataPath);

	bool Connect();

	void Disconnect();

	bool StartReading();
	bool StartReading(RegistrationManager& regManager);
};

#endif