#ifndef REGMANAGER_H
#define REGMANAGER_H

#include "ReadRegistration.h"

class RegistrationManager
{
	HANDLE hPipe_;
	//std::string pipeName_;

public:
	std::vector<CSelectedArchivesData> VecRegistrations;

	int CountArchives;
	int CurrentArchiveNumber;

	RegistrationManager(std::string pipeName);

	void WriteToPipe(BYTE* buffer);
	void WriteToPipeAboutError(EUsbResult res);
	void WriteToPipeCurrenPercent(int percent);

	~RegistrationManager()
	{
		if(hPipe_ != INVALID_HANDLE_VALUE)
			CloseHandle(hPipe_);
	}
};

#endif