// ReaderProc.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"
#include <fstream>
#include <iostream>
#include <string>

#include "RegistrationManager.h"
#include "EmServiceClasses.h"
#include "Device.h"

int _tmain(int argc, _TCHAR* argv[])
{
	/*if (File.Exists(EmService.logGeneralName)) ????????? to C# prog!
		File.Delete(EmService.logGeneralName);
	if (File.Exists(EmService.logFailedName))
		File.Delete(EmService.logFailedName);
	if (File.Exists(EmService.logDebugName))
		File.Delete(EmService.logDebugName);*/

	EmService::Init();

	// open settings file and read settings
	std::ifstream istr("ForReaderProc.cfg");
	if(!istr)
	{
		// send a message to main program (by means of tmp file?) ???????????????
		EmService::WriteToLogFailed("Unable to open ForReaderProc.cfg!");
		return -1;
	}
	std::string mode;
	std::string dataPath;
	std::string pipeName;
	std::getline(istr, mode, '\n');			// mode ALL or SELECTED
	std::getline(istr, dataPath, '\n');		// path to store data that have been read
	std::getline(istr, pipeName, '\n');		// pipe name (to send data to the main process)

	CDevice* pDevice = 0;

	if(mode.find("ALL") != std::string::npos)
	{
		// ALL mode
		/*EmService::WriteToLogGeneral("ALL mode");
		pDevice = new CDevice(ALL, dataPath);
		if(!pDevice->Connect())
		{
			EmService::WriteToLogFailed("Unable to connect to the device!");
			delete pDevice;
			return -1;
		}
		pDevice->StartReading();*/
	}
	else if(mode.find("SELECTED") != std::string::npos)
	{
		// SELECTED mode
		EmService::WriteToLogGeneral("SELECTED mode");
		//std::vector<CSelectedArchivesData> vecRegistrations;
		RegistrationManager regManager(pipeName);
		pDevice = new CDevice(SELECTED, dataPath);
		if(!pDevice->Connect())
		{
			EmService::WriteToLogFailed("Unable to connect to the device!");
			delete pDevice;
			return -1;
		}
		EmService::WriteToLogGeneral("Device connected successfully");

		std::string tmpStr;
		while(!istr.eof())
		{
			CSelectedArchivesData curData;
			// which archives we must read:
			std::getline(istr, tmpStr, '\n');
			curData.regId = EmService::StringToNumberL(tmpStr); // проверка на валидность в конце цикла
			std::getline(istr, tmpStr, '\n');		// PQP indexes in format "index1|index2|..."
			std::string tmp_str2;
			if(tmpStr.find("EMPTY") == std::string::npos)
			{
				// parse string with PQP indexes
				std::istringstream stream(tmpStr);
				while(!stream.eof())
				{
					std::getline(stream, tmp_str2, '|');
					if(EmService::StringToNumberL(tmp_str2) > 0)
					{
						curData.vecPqpIndexes.push_back(EmService::StringToNumberL(tmp_str2));
						regManager.CountArchives++;
					}
				}
			}

			std::getline(istr, tmpStr, '\n');		// AVG types in format "type1|type2|..."
			std::vector<short> avgTypes;		// 1 = 3 sec, 2 = 10 min, 3 = 2 hour
					// we mustn't use 0 because atoi() returns 0 when where's an error
			if(tmpStr.find("EMPTY") == std::string::npos)
			{
				EmService::WriteToLogGeneral("");
				// parse string with PQP indexes
				std::istringstream stream(tmpStr);
				while(!stream.eof())
				{
					std::getline(stream, tmp_str2, '|');
					short shtmp = EmService::StringToNumberI(tmp_str2);
					if(shtmp > 0)
					{
						switch(shtmp)
						{
							case 1: curData.avgTypes.push_back(THREE_SEC); break;
							case 2: curData.avgTypes.push_back(TEN_MIN); break;
							case 3: curData.avgTypes.push_back(TWO_HOUR); break;
						}
						regManager.CountArchives++;
					}
				}
			}

			std::getline(istr, tmpStr, '\n');		// Read DNS (1) or not (0)
			curData.readDns = tmpStr.find("1") != std::string::npos;
			if(curData.readDns) regManager.CountArchives++;

			if(curData.regId > 0 && 
				(curData.vecPqpIndexes.size() > 0 ||
				curData.avgTypes.size() > 0 ||
				curData.readDns))
			{
				EmService::WriteToLogGeneral("Registration data:");
				EmService::WriteToLogGeneral(EmService::NumberToString(curData.regId));
				EmService::WriteToLogGeneral(EmService::VectorToString<DWORD>(curData.vecPqpIndexes));
				EmService::WriteToLogGeneral(EmService::VectorToString<EAvgType>(curData.avgTypes));
				EmService::WriteToLogGeneral(curData.readDns ? "true" : "false");
				EmService::WriteToLogGeneral("End of Registration data:");

				regManager.VecRegistrations.push_back(curData);
			}
		}

		// start reading
		pDevice->StartReading(regManager);

		pDevice->Disconnect();
	}
	else
	{
		EmService::WriteToLogFailed("main(): unknown mode!");
		return -1;
	}

	istr.close();
	if(pDevice != 0) delete pDevice;

	return 0;
}

