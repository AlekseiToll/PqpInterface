#define _CRT_SECURE_NO_WARNINGS

#include "stdafx.h"
#include <windows.h>
#include <stdio.h>
#include <iostream>
#include <fstream>

#include "ReadRegistration.h"
#include "RegistrationManager.h"
#include "EmUsb.h"

CRegistration::CRegistration(DWORD regId, std::string dataPath, CUsb* usb): 
		regId_(regId), dataPath_(dataPath), usb_(usb)
{
}

//DWORD WINAPI CRegistration::ReadAllRegistration()
//{
//	WORD wReplyCommand;
//	WORD wReplyLength;
//	WORD wReplyAddress;
//	//char szResultFileName[MAX_PATH];
//	//char szResultFilePath[MAX_PATH];
//	//char szResultFileDirectory[MAX_PATH];
//	DWORD dwCurrentRegistrationIndex;
//	DWORD dwMinIndex,dwMaxIndex;
//	DWORD dwMinRealIndex,dwMaxRealIndex;
//	LONG lNumberOfIndices;
//	TDateTime xMinIndexDateTime,xMaxIndexDateTime;
//
//	//wRegistrationArchiveIndexCounter = 0;
//	//wRegistrationCounter = 0;
//
//	// read current registration index
//	/*{
//		WORD wRequest = SD_RegistrationIndex;
//		UsbResult = UsbCommunication(		
//			FALSE,
//			COMMAND_ReadSystemData,
//			2,
//			&wRequest,
//			5,
//			&wReplyAddress,
//			&wReplyCommand,
//			&wReplyLength,
//			(void*)&dwCurrentRegistrationIndex,
//			sizeof(dwCurrentRegistrationIndex));
//	}*/
//
//	//---------------------------------------------------------------------------------------
//	// COMMAND_ReadRegistrationIndices = Чтение списка абсолютных индексов доступных Регистраций 
//	EUsbResult UsbResult = UsbCommunication(		
//		FALSE,
//		COMMAND_ReadRegistrationIndices,
//		0,
//		NULL,
//		5,
//		&wReplyAddress,
//		&wReplyCommand,
//		&wReplyLength,
//		(void*)adwRegistrationIndexList,
//		sizeof(adwRegistrationIndexList));
//
//	wRegistrationIndexCounter = wReplyLength/4;
//
//	if (wRegistrationIndexCounter == 0)
//	{
//		//???????????????? нет регистраций: msg to main program
//		return 0;
//	}
//
//	//============================================================================
//	/*fprintf(fResult,"Список индексов Регистраций:\n");
//	for(wRegistrationIndex=0;wRegistrationIndex<wRegistrationIndexCounter;wRegistrationIndex++)
//	{
//		fprintf(fResult,"   %09d\n",adwRegistrationIndexList[wRegistrationIndex]);
//	}*/
//	//============================================================================
//	for(WORD wRegistrationIndex = 0; wRegistrationIndex < wRegistrationIndexCounter; wRegistrationIndex++)
//	{
//		//fprintf(fResult,"   Регистрация %09d\n",adwRegistrationIndexList[wRegistrationIndex]);
//
//		// Чтение описания Регистрации по ее абсолютному индексу
//		UsbResult = UsbCommunication(		
//			FALSE,
//			COMMAND_ReadRegistrationByIndex,
//			4,
//			&(adwRegistrationIndexList[wRegistrationIndex]),
//			5,
//			&wReplyAddress,
//			&wReplyCommand,
//			&wReplyLength,
//			(void*)&xRegistration,
//			sizeof(xRegistration));
//			
//		if (wReplyLength == 2048)
//		{
//			WORD wNumberOfDsiArchives = 0;
//
//			UsbResult = UsbCommunication(		
//				FALSE,
//				COMMAND_ReadDSIArchivesByRegistration,
//				4,
//				&(adwRegistrationIndexList[wRegistrationIndex]),
//				5,
//				&wReplyAddress,
//				&wReplyCommand,
//				&wReplyLength,
//				(void*)&axDsiArchiveEntries[0],
//				8192);
//
//			if (wReplyLength != 0)
//			{
//				wNumberOfDsiArchives += (wReplyLength/sizeof(TDsiArchiveEntry));
//				while(1)
//				{
//					UsbResult = UsbCommunication(		
//						TRUE,
//						COMMAND_ReadDSIArchives,
//						4,
//						&(adwRegistrationIndexList[wRegistrationIndex]),
//						5,
//						&wReplyAddress,
//						&wReplyCommand,
//						&wReplyLength,
//						(void*)&axDsiArchiveEntries[wNumberOfDsiArchives],
//						8192);
//
//					if ((wReplyLength == 0))
//					{
//						break;
//					}
//					else if ((UsbResult == USBRESULT_OK))
//					{
//						wNumberOfDsiArchives += (wReplyLength/sizeof(TDsiArchiveEntry));
//					}
//				}
//			}
//
//			WORD wHarmIndex;
//			//fprintf(fResult,"Описание Регистрации:\n");
//			//fprintf(fResult,"   dwIndex                      %09d\n",xRegistration.dwIndex);
//			//fprintf(fResult,"   dwSignature                  0x%08X\n",xRegistration.dwSignature);
//			//fprintf(fResult,"   wRegistrationVersion         %d\n",xRegistration.wRegistrationVersion);
//			/*fprintf(fResult,"   xRegistrationStartDateTime   UTC:%.2d/%.2d/%.4d %.2d:%.2d:%.2d.%.4d RTC:%.2d/%.2d/%.4d %.2d:%.2d:%.2d.%.4d tZ:%d\n",
//					xRegistration.xRegistrationStartDateTime.wUtcDate,
//					xRegistration.xRegistrationStartDateTime.wUtcMonth,
//					xRegistration.xRegistrationStartDateTime.wUtcYear,
//					xRegistration.xRegistrationStartDateTime.wUtcHours,
//					xRegistration.xRegistrationStartDateTime.wUtcMinutes,
//					xRegistration.xRegistrationStartDateTime.wUtcSeconds,
//					xRegistration.xRegistrationStartDateTime.wMilliseconds,
//					xRegistration.xRegistrationStartDateTime.wLocalDate,
//					xRegistration.xRegistrationStartDateTime.wLocalMonth,
//					xRegistration.xRegistrationStartDateTime.wLocalYear,
//					xRegistration.xRegistrationStartDateTime.wLocalHours,
//					xRegistration.xRegistrationStartDateTime.wLocalMinutes,
//					xRegistration.xRegistrationStartDateTime.wLocalSeconds,
//					xRegistration.xRegistrationStartDateTime.wMilliseconds,
//					xRegistration.xRegistrationStartDateTime.wTimeZone);
//			fprintf(fResult,"   xRegistrationStopDateTime    UTC:%.2d/%.2d/%.4d %.2d:%.2d:%.2d.%.4d RTC:%.2d/%.2d/%.4d %.2d:%.2d:%.2d.%.4d tZ:%d\n",
//					xRegistration.xRegistrationStopDateTime.wUtcDate,*/			
//
//			/*fprintf(fResult,"   sObjectName                  %16s\n",xRegistration.sObjectName);
//			fprintf(fResult,"   wConnection                  %d\n",xRegistration.wConnection);
//			fprintf(fResult,"   wVoltageRangeName            %d\n",xRegistration.wVoltageRangeName);
//			fprintf(fResult,"   wCurrentRangeName            %d\n",xRegistration.wCurrentRangeName);
//			fprintf(fResult,"   wFrequencyRangeName          %d\n",xRegistration.wFrequencyRangeName);
//			fprintf(fResult,"   boolVoltageTransformerEnable %d (0=TRUE,1=FALSE)\n",xRegistration.boolVoltageTransformerEnable);
//			fprintf(fResult,"   wVoltageTransformerType      %d\n",xRegistration.wVoltageTransformerType);
//			fprintf(fResult,"   dwDeclaredVoltage            %d\n",xRegistration.dwDeclaredVoltage);
//			fprintf(fResult,"   wCurrentSensorType           %d\n",xRegistration.wCurrentSensorType);
//			fprintf(fResult,"   wCurrentTransformer          %d\n",xRegistration.wCurrentTransformer);
//			fprintf(fResult,"   wCurrentTransformerPrimary   %d\n",xRegistration.wCurrentTransformerPrimary);
//			fprintf(fResult,"   boolMainsSynchronization     %d (0=TRUE,1=FALSE)\n",xRegistration.boolMainsSynchronization);
//			fprintf(fResult,"   boolDateTimeAutomaticCorre...%d (0=TRUE,1=FALSE)\n",xRegistration.boolDateTimeAutomaticCorrection);
//			fprintf(fResult,"   wSupplySystem                %d\n",xRegistration.wSupplySystem);
//			fprintf(fResult,"   wSetsType                    %d\n",xRegistration.wSetsType);
//			fprintf(fResult,"   wRegistrationArchiveInterv...%d\n",xRegistration.wRegistrationArchiveIntervals);
//			fprintf(fResult,"   wRegistrationMode            %d (0=MANUAL,1=SCHEDULED)\n",xRegistration.wRegistrationMode);
//			fprintf(fResult,"   boolRegistrationDone         %d (0=TRUE,1=FALSE)\n",xRegistration.boolRegistrationDone);
//			fprintf(fResult,"   wInterruptionsCounter        %d\n",xRegistration.wInterruptionsCounter);
//			fprintf(fResult,"   dwFirmwareVersion            %lu\n",xRegistration.dwFirmwareVersion);
//			fprintf(fResult,"   dwRegistrationStartDateTimeSeconds       %lu\n",xRegistration.dwRegistrationStartDateTimeSeconds);
//			fprintf(fResult,"   dwRegistrationStopDateTimeSeconds        %lu\n",xRegistration.dwRegistrationStopDateTimeSeconds);
//			fprintf(fResult,"   wRegistrationArchiveCounter  %lu\n",xRegistration.wRegistrationArchiveCounter);
//			fprintf(fResult,"   wRegistrationStopSource      %d (0=manual,1=automatic,2=when powered on)\n",xRegistration.wRegistrationStopSource);
//			fprintf(fResult,"   boolCoordinateValid			 %d\n",xRegistration.boolCoordinateValid);
//			fprintf(fResult,"   dCoordinateLatitude			 %f\n",xRegistration.dCoordinateLatitude);
//			fprintf(fResult,"   dCoordinateLongitude	     %f\n",xRegistration.dCoordinateLongitude);
//			fprintf(fResult,"   wTimeCorrectionsCounter      %d\n",xRegistration.wTimeCorrectionsCounter);
//			fprintf(fResult,"   dwGpsSecondsFullPrecision	 %lu\n",xRegistration.dwGpsSecondsFullPrecision);
//			fprintf(fResult,"   dwGpsSecondsTotal			 %lu\n",xRegistration.dwGpsSecondsTotal);
//			fprintf(fResult,"   wMemoryDistribution_3Sec        %d\n",xRegistration.wMemoryDistribution_3Sec);
//			fprintf(fResult,"   wMemoryDistribution_10Min        %d\n",xRegistration.wMemoryDistribution_10Min);
//			fprintf(fResult,"   wMemoryDistribution_2Hour        %d\n",xRegistration.wMemoryDistribution_2Hour);
//			fprintf(fResult,"   wFlaggedData                 %d\n",xRegistration.wFlaggedData);*/
//
//			/*fprintf(fResult,"\n");
//			fprintf(fResult,"   lSetsFrequencyDeviationSynchronizedDown95       %+09.3f\n",(float)(xRegistration.lSetsFrequencyDeviationSynchronizedDown95)/(float)65536);
//			fprintf(fResult,"   lSetsFrequencyDeviationSynchronizedDown100      %+09.3f\n",(float)(xRegistration.lSetsFrequencyDeviationSynchronizedDown100)/(float)65536);
//			fprintf(fResult,"   lSetsFrequencyDeviationSynchronizedUp95         %+09.3f\n",(float)(xRegistration.lSetsFrequencyDeviationSynchronizedUp95)/(float)65536);
//			fprintf(fResult,"   lSetsFrequencyDeviationSynchronizedUp100        %+09.3f\n",(float)(xRegistration.lSetsFrequencyDeviationSynchronizedUp100)/(float)65536);
//			fprintf(fResult,"   lSetsFrequencyDeviationIsolatedDown95           %+09.3f\n",(float)(xRegistration.lSetsFrequencyDeviationIsolatedDown95)/(float)65536);
//			fprintf(fResult,"   lSetsFrequencyDeviationIsolatedDown100          %+09.3f\n",(float)(xRegistration.lSetsFrequencyDeviationIsolatedDown100)/(float)65536);
//			fprintf(fResult,"   lSetsFrequencyDeviationIsolatedUp95             %+09.3f\n",(float)(xRegistration.lSetsFrequencyDeviationIsolatedUp95)/(float)65536);
//			fprintf(fResult,"   lSetsFrequencyDeviationIsolatedUp100            %+09.3f\n",(float)(xRegistration.lSetsFrequencyDeviationIsolatedUp100)/(float)65536);
//			fprintf(fResult,"\n");
//
//			lrWriteSets(fResult,xRegistration.lSetsVoltageDeviationPositive95,"lSetsVoltageDeviationPositive95");
//			lrWriteSets(fResult,xRegistration.lSetsVoltageDeviationPositive100,"lSetsVoltageDeviationPositive100");
//			lrWriteSets(fResult,xRegistration.lSetsVoltageDeviationNegative95,"lSetsVoltageDeviationNegative95");
//			lrWriteSets(fResult,xRegistration.lSetsVoltageDeviationNegative100,"lSetsVoltageDeviationNegative100");
//
//			lrWriteSets(fResult,xRegistration.lSetsFlickerShortTerm95,"lSetsFlickerShortTerm95");
//			lrWriteSets(fResult,xRegistration.lSetsFlickerShortTerm100,"lSetsFlickerShortTerm100");
//			lrWriteSets(fResult,xRegistration.lSetsFlickerLongTerm95,"lSetsFlickerLongTerm95");
//			lrWriteSets(fResult,xRegistration.lSetsFlickerLongTerm100,"lSetsFlickerLongTerm100");*/
//
//			/*for(wHarmIndex=2;wHarmIndex<=40;wHarmIndex++)
//			{
//				char sz[128];
//				sprintf(sz,"alSetsKHarm95 [#%d]",wHarmIndex);
//				lrWriteSets(fResult,xRegistration.alSetsKHarm95[wHarmIndex-2],sz);
//			}
//			for(wHarmIndex=2;wHarmIndex<=40;wHarmIndex++)
//			{
//				char sz[128];
//				sprintf(sz,"alSetsKHarm100 [#%d]",wHarmIndex);
//				lrWriteSets(fResult,xRegistration.alSetsKHarm100[wHarmIndex-2],sz);
//			}
//
//			lrWriteSets(fResult,xRegistration.lSetsKHarmTotal95,"lSetsKHarmTotal95");
//			lrWriteSets(fResult,xRegistration.lSetsKHarmTotal100,"lSetsKHarmTotal100");
//			lrWriteSets(fResult,xRegistration.lSetsK2U95,"lSetsK2U95");
//			lrWriteSets(fResult,xRegistration.lSetsK2U100,"lSetsK2U100");
//			lrWriteSets(fResult,xRegistration.lSetsK0U95,"lSetsK0U95");
//			lrWriteSets(fResult,xRegistration.lSetsK0U100,"lSetsK0U100");*/
//				
//			WORD wArchive;
//			adwRegistrationList[wRegistrationCounter] = xRegistration.dwIndex;
//			adwRegistrationAverageArchiveIndex3SecHead[wRegistrationCounter] = xRegistration.dwAverageArchiveIndex3SecHead;
//			adwRegistrationAverageArchive3SecCounter[wRegistrationCounter] = xRegistration.dwAverageArchive3SecCounter;
//			adwRegistrationAverageArchiveIndex10MinHead[wRegistrationCounter] = xRegistration.dwAverageArchiveIndex10MinHead;
//			adwRegistrationAverageArchive10MinCounter[wRegistrationCounter] = xRegistration.dwAverageArchive10MinCounter;
//			adwRegistrationAverageArchiveIndex2HourHead[wRegistrationCounter] = xRegistration.dwAverageArchiveIndex2HourHead;
//			adwRegistrationAverageArchive2HourCounter[wRegistrationCounter] = xRegistration.dwAverageArchive2HourCounter;
//
//			memcpy(
//				&adwRegistrationStartDateTimeList[wRegistrationCounter],
//				&xRegistration.xRegistrationStartDateTime,
//				sizeof(TDateTime));
//			memcpy(
//				&adwRegistrationStopDateTimeList[wRegistrationCounter],
//				&xRegistration.xRegistrationStopDateTime,
//				sizeof(TDateTime));
//			adwRegistrationStartDateTimeSeconds[wRegistrationCounter] = xRegistration.dwRegistrationStartDateTimeSeconds;
//			adwRegistrationStopDateTimeSeconds[wRegistrationCounter] = xRegistration.dwRegistrationStopDateTimeSeconds;
//
//			wRegistrationCounter++;
//
//			//for(wArchive=0;wArchive<64;wArchive++)
//			for(wArchive = 0; wArchive < 32; wArchive++) //!!! debug(Borya) ????????????????????
//			{
//				if ((xRegistration.adwRegistrationArchiveIndices[wArchive]==0xFFFFFFFF))
//				{
//					//fprintf(fResult,"    %02d: ---------\n",wArchive);
//				}
//				else if ((xRegistration.adwRegistrationArchiveIndices[wArchive]==0x00000000)) // DEBUG(Borya)
//				{
//					//fprintf(fResult,"    %02d: ---------\n",wArchive);
//				}
//				else
//				{
//					//fprintf(fResult,"%02d: %09ld\n",wArchive,xRegistration.adwRegistrationArchiveIndices[wArchive]);
//
//					adwRegistrationArchiveIndexList[wRegistrationArchiveIndexCounter] =
//						xRegistration.adwRegistrationArchiveIndices[wArchive];
//					adwRegistrationArchiveParentIndexList[wRegistrationArchiveIndexCounter] = xRegistration.dwIndex;
//					{
//						memcpy(
//							&adwRegistrationArchiveParentStartDateTimeList[wRegistrationArchiveIndexCounter],
//							&xRegistration.xRegistrationStartDateTime,
//							sizeof(TDateTime)
//							);
//						memcpy(
//							&adwRegistrationArchiveParentStopDateTimeList[wRegistrationArchiveIndexCounter],
//							&xRegistration.xRegistrationStopDateTime,
//							sizeof(TDateTime)
//							);
//					}
//					wRegistrationArchiveIndexCounter++;
//				}
//			}
//			
//			WORD wIndex;
//			BYTE bType;
//			BYTE bChannel;
//			//fprintf(fResult,"   wNumberOfDsiArchives       %d\n",wNumberOfDsiArchives);
//			//fprintf(fResult,"[в архивах случайных событий время локальное]\n");
//
//			for(bType=0;bType<=2;bType++)
//			{
//				switch(bType)
//				{
//					case DSI_TYPE_DIP:	
//						//fprintf(fResult,"DSI_TYPE_DIP\n");
//						break;
//					case DSI_TYPE_SWELL:	
//						//fprintf(fResult,"DSI_TYPE_SWELL\n");
//						break;
//					case DSI_TYPE_INTERRUPTION:	
//						//fprintf(fResult,"DSI_TYPE_INTERRUPTION\n");
//						break;
//				}
//
//				for(bChannel=0;bChannel<=7;bChannel++)
//				{
//					switch(bChannel)
//					{
//						case DSI_CHANNEL_UABCN:
//							//fprintf(fResult,"   DSI_CHANNEL_UABCN\n");
//							break;
//						case DSI_CHANNEL_UABC:	
//							//fprintf(fResult,"   DSI_CHANNEL_UABC\n");
//							break;
//						case DSI_CHANNEL_UA:	
//							//fprintf(fResult,"   DSI_CHANNEL_UA\n");
//							break;
//						case DSI_CHANNEL_UB:
//							//fprintf(fResult,"   DSI_CHANNEL_UB\n");
//							break;
//						case DSI_CHANNEL_UC:	
//							//fprintf(fResult,"   DSI_CHANNEL_UC\n");
//							break;
//						case DSI_CHANNEL_UAB:	
//							//fprintf(fResult,"   DSI_CHANNEL_UAB\n");
//							break;
//						case DSI_CHANNEL_UBC:	
//							//fprintf(fResult,"   DSI_CHANNEL_UBC\n");
//							break;
//						case DSI_CHANNEL_UCA:
//							//fprintf(fResult,"   DSI_CHANNEL_UCA\n");
//							break;
//					}
//
//					for(wIndex=0;wIndex<wNumberOfDsiArchives;wIndex++)
//					{
//						if ( ((BYTE)((axDsiArchiveEntries[wIndex].wType)&0xFF)==bType) && ((BYTE)(((axDsiArchiveEntries[wIndex].wType)>>8)&0xFF)==bChannel) )
//						{
//							/*fprintf(fResult,"  Index=%6d(%d)   [%.2d/%.2d/%.4d %.2d:%.2d:%.2d.%.3d]>[%.2d/%.2d/%.4d %.2d:%.2d:%.2d.%.3d]=[%0.4ddays %.2d:%.2d:%.2d.%.3d]\n",
//								axDsiArchiveEntries[wIndex].dwDsiIndex,
//								axDsiArchiveEntries[wIndex].wEventFinished,
//
//								axDsiArchiveEntries[wIndex].xStart.wLocalDate,
//								axDsiArchiveEntries[wIndex].xStart.wLocalMonth,
//								axDsiArchiveEntries[wIndex].xStart.wLocalYear,
//								axDsiArchiveEntries[wIndex].xStart.wLocalHours,
//								axDsiArchiveEntries[wIndex].xStart.wLocalMinutes,
//								axDsiArchiveEntries[wIndex].xStart.wLocalSeconds,
//								axDsiArchiveEntries[wIndex].xStart.wMilliseconds,
//
//								axDsiArchiveEntries[wIndex].xEnd.wLocalDate,
//								axDsiArchiveEntries[wIndex].xEnd.wLocalMonth,
//								axDsiArchiveEntries[wIndex].xEnd.wLocalYear,
//								axDsiArchiveEntries[wIndex].xEnd.wLocalHours,
//								axDsiArchiveEntries[wIndex].xEnd.wLocalMinutes,
//								axDsiArchiveEntries[wIndex].xEnd.wLocalSeconds,
//								axDsiArchiveEntries[wIndex].xEnd.wMilliseconds,
//
//								axDsiArchiveEntries[wIndex].wDurationDays,
//								axDsiArchiveEntries[wIndex].wDurationHours,
//								axDsiArchiveEntries[wIndex].wDurationMinutes,
//								axDsiArchiveEntries[wIndex].wDurationSeconds,
//								axDsiArchiveEntries[wIndex].wDurationMilliseconds
//
//								);
//
//							fprintf(fResult,"   [%09d > %09d sec]   U=%.3fV (dU=%.3f%%) (Ud=%.3fV)\n",
//								axDsiArchiveEntries[wIndex].dwStartSeconds,
//								axDsiArchiveEntries[wIndex].dwEndSeconds,
//								((float)(axDsiArchiveEntries[wIndex].dwVoltageMicrovolts))/(float)1000000.0,
//								((float)(axDsiArchiveEntries[wIndex].dwVoltageRelative))/(float)1342177.28,
//								((float)(axDsiArchiveEntries[wIndex].dwDeclaredVoltageMicrovolts))/(float)1000000.0
//								);*/
//
//						}
//					}
//				}
//			}
//
//			/*fprintf(fResult,"   dwAverageArchiveIndex3SecHead %09d\n",xRegistration.dwAverageArchiveIndex3SecHead);
//			fprintf(fResult,"   dwAverageArchive3SecCounter %09d\n",xRegistration.dwAverageArchive3SecCounter);
//			fprintf(fResult,"   dwAverageArchiveIndex10MinHead %09d\n",xRegistration.dwAverageArchiveIndex10MinHead);
//			fprintf(fResult,"   dwAverageArchive10MinCounter %09d\n",xRegistration.dwAverageArchive10MinCounter);
//			fprintf(fResult,"   dwAverageArchiveIndex2HourHead %09d\n",xRegistration.dwAverageArchiveIndex2HourHead);
//			fprintf(fResult,"   dwAverageArchive2HourCounter %09d\n",xRegistration.dwAverageArchive2HourCounter);*/
//		}
//	}
//}
	
//===========================================================
// pqpCount - кол-во индексов архивов ПКЭ
// pAvgType - типы архивов усред., которые надо читать
// avgCount - кол-во этих типов
// readDns - надо ли читать архив провалов
//===========================================================
//bool CRegistration::ReadRegistrationByIndex( 
//	std::vector<DWORD> pqpIndexes,
//	std::vector<EAvgType> avgTypes, 
//	bool readDns)

// curRegIndex - index of registration in the loop of outer method
bool CRegistration::ReadRegistrationByIndex(RegistrationManager& regManager, int curRegIndex)
{
	try
	{
		WORD wReplyCommand;
		WORD wReplyLength;
		WORD wReplyAddress;
		DWORD dwMinIndex, dwMaxIndex;
		DWORD dwMinRealIndex, dwMaxRealIndex;
		LONG lNumberOfIndices;
		TDateTime xMinIndexDateTime, xMaxIndexDateTime;

		// reading the registration by its absolute index
		EUsbResult UsbResult = usb_->UsbCommunication(		
			FALSE,
			READ_TIMEOUT,
			&wReplyAddress,
			&wReplyCommand,
			&wReplyLength,
			(void*)&regData_,
			sizeof(TRegistrationData),
			COMMAND_ReadRegistrationByIndex,
			4,
			&regId_);

		if (UsbResult != USBRESULT_OK)
		{
			EmService::WriteToLogFailed("ReadRegistrationByIndex: UsbResult != USBRESULT_OK");
			regManager.WriteToPipeAboutError(UsbResult);
			return false;
		}
		if (wReplyLength != 2048)
		{
			EmService::WriteToLogFailed("ReadRegistrationByIndex: wReplyLength != 2048");
			regManager.WriteToPipeAboutError(UsbResult);
			return false;
		}
		EmService::WriteToLogGeneral("ReadRegistrationByIndex: OK");

		deviceSerialNumber_ = wReplyAddress;
		std::stringstream ss_temp;
		ss_temp << "\\ETPQP_A_" << deviceSerialNumber_;
		dataPath_ += ss_temp.str();
		// check if folder for this device exists
		if(!EmService::DirExists(dataPath_.c_str()))
		{
			BOOL res = CreateDirectory(dataPath_.c_str(), NULL);
			if(!res)
			{
				EmService::WriteToLogFailed("Unable to create the directory for the device");
				EmService::WriteToLogFailed(dataPath_);
				return false;
			}
		}
		ss_temp.str("");
		ss_temp << "\\Reg" << regId_;
		dataPath_ += ss_temp.str();
		// check if folder for this registration exists
		if(!EmService::DirExists(dataPath_.c_str()))
		{
			BOOL res = CreateDirectory(dataPath_.c_str(), NULL);
			if(!res)
			{
				EmService::WriteToLogFailed("Unable to create the directory for the device");
				EmService::WriteToLogFailed(dataPath_);
				return false;
			}
		}
		EmService::WriteToLogGeneral("Create dir for device and registration: OK");

		// object name
		objectName_ = std::string(regData_.sObjectName);

		EmService::WriteToLogGeneral("Reading archives start");
		for(WORD wPqpIndex = 0; 
			wPqpIndex < regManager.VecRegistrations[curRegIndex].vecPqpIndexes.size(); 
			++wPqpIndex)
		{
			regManager.CurrentArchiveNumber++;
			ReadPqpArchive(regManager.VecRegistrations[curRegIndex].vecPqpIndexes[wPqpIndex], regManager);
		}

		for(WORD wAvgType = 0; wAvgType < regManager.VecRegistrations[curRegIndex].avgTypes.size(); ++wAvgType)
		{
			regManager.CurrentArchiveNumber++;
			ReadAvgArchive(regManager.VecRegistrations[curRegIndex].avgTypes[wAvgType], regManager);
		}

		if(regManager.VecRegistrations[curRegIndex].readDns)		// if we must read DNS archive
		{
			regManager.CurrentArchiveNumber++;
			ReadDnsArchive(regData_.xRegistrationStartDateTime, regManager);
		}
		EmService::WriteToLogGeneral("Reading archives end");

		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in ReadRegistrationByIndex()");
		throw;
	}
}

//===========================================================
//===========================================================
bool CRegistration::ReadPqpArchive(DWORD archiveIndex, RegistrationManager& regManager)
{
	try
	{
		EmService::WriteToLogGeneral("ReadPqpArchive: start");
		WORD awArchiveSegmentData[PQP_SEGMENT_LENGTH / 2];
		DWORD adwRequest[2];
		WORD wReplyCommand;
		WORD wReplyLength;
		WORD wReplyAddress;
		DWORD adwPqpArchive[512 * 1024 / 4];
		
		adwRequest[0] = archiveIndex;
		for(WORD wArchiveSegment = 0; wArchiveSegment < PQP_SEGMENT_COUNT; wArchiveSegment++)
		{
			*(WORD*)&(adwRequest[1]) = wArchiveSegment;
			EUsbResult UsbResult = usb_->UsbCommunication(		
				FALSE,
				READ_TIMEOUT,
				&wReplyAddress,
				&wReplyCommand,
				&wReplyLength,
				(void*)awArchiveSegmentData,
				PQP_SEGMENT_LENGTH,
				COMMAND_ReadRegistrationArchiveByIndex,
				6,	// from Borya
				(void*)adwRequest);

			if (wReplyLength != PQP_SEGMENT_LENGTH)
			{
				EmService::WriteToLogFailed("ERROR: COMMAND_ReadRegistrationArchiveByIndex returned 0 bytes");
				regManager.WriteToPipeAboutError(UsbResult);
				return false;
			}
			else
			{
				memcpy(&(adwPqpArchive[((DWORD)wArchiveSegment) * 512]),
					&(awArchiveSegmentData[3]),
					2048);
			}

			// send report about progress of reading to pipe
			if((wArchiveSegment % 10) == 0)
				regManager.WriteToPipeCurrenPercent(wArchiveSegment * 100 / PQP_SEGMENT_COUNT);
		}
		EmService::WriteToLogGeneral("ReadPqpArchive: Reading archive: OK");
		regManager.WriteToPipeCurrenPercent(100);

		std::string fileName = EmService::RemoveAllExceptLettersAndDigits(objectName_);
		if(fileName.length() == 0) fileName = "PQP";

		TPqpArchive *pxPqpArchive = (TPqpArchive*)adwPqpArchive;
		std::stringstream ss_temp;
		TDateTime dtStart = pxPqpArchive->xArchiveStartDateTime;
		ss_temp << fileName << '_' << dtStart.wUtcYear << '_' <<
			EmService::MakeCorrectNumberLength(dtStart.wUtcMonth, 2) << '_' <<
			EmService::MakeCorrectNumberLength(dtStart.wUtcDate, 2) << "__" <<
			EmService::MakeCorrectNumberLength(dtStart.wUtcHours, 2) << '_' <<
			EmService::MakeCorrectNumberLength(dtStart.wUtcMinutes, 2) << '_' <<
			EmService::MakeCorrectNumberLength(dtStart.wUtcSeconds, 2) << ".tmppqp";
		EmService::WriteToLogGeneral(ss_temp.str());

		// check if folder for PQP archives exists
		if(!EmService::DirExists((dataPath_+ "\\PQP").c_str()))
		{
			BOOL res = CreateDirectory((dataPath_+ "\\PQP").c_str(), NULL);
			if(!res)
			{
				EmService::WriteToLogFailed("Unable to create the directory for the device");
				EmService::WriteToLogFailed(dataPath_+ "\\PQP");
				return false;
			}
		}
		// write header to file
		std::string filename = dataPath_ + "\\PQP\\" + ss_temp.str();

		// if file already exists then we won't replace it
		if(EmService::FileExists(filename.c_str()))
		{
			EmService::WriteToLogGeneral("PQP file already exists!");
			if(remove(filename.c_str()) == -1)
			{
				EmService::WriteToLogFailed("PQP file already exists and can't be deleted!");
				return false;
			}
		}

		std::ofstream ostr(filename, std::ios::binary | std::ios::out | std::ios::trunc);
		if(!ostr)
		{
			EmService::WriteToLogFailed("ReadPqpArchive: unable to open ofstream");
			return false;
		}

		int wholeHeaderLength = //EmService::DigitCount(wholeHeaderLength)
			3 /*file version: 3 digits*/ + 1 /*separator*/
			+ EmService::DigitCount(deviceSerialNumber_) + 1 /*separator*/
			+ EmService::DigitCount(regId_) + 1
			+ EmService::DigitCount(archiveIndex) + 1
			+ EmService::DigitCount(sizeof(TRegistrationData)) + 1
			+ sizeof(TRegistrationData) + 1;

		std::string fileVersion = "001";	// this valiable is needed because fucking Avast will delete 
		// the EXE file if we write directly ostr << "001";

		// write header
		ostr << wholeHeaderLength << '|';
		// file version 
		ostr << fileVersion << '|';
		// serial number	
		ostr << deviceSerialNumber_ << '|';
		// registration id
		ostr << regId_ << '|';

		// all registration data
		ostr << sizeof(TRegistrationData) << '|';
		ostr.write((const char*)&regData_, sizeof(TRegistrationData));
		ostr << '|';

		// archive id
		ostr << archiveIndex << '|';

		// archive itself
		ostr.write((const char*)adwPqpArchive, PQP_ARCHIVE_LENGTH);

		ostr.close();
		EmService::WriteToLogGeneral("ReadPqpArchive: end");
		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in ReadPqpArchive()");
		throw;
	}
}

//===========================================================
//===========================================================
bool CRegistration::ReadAvgArchive(EAvgType type, RegistrationManager& regManager)
{
	try
	{
		EmService::WriteToLogGeneral("ReadAvgArchive: start");

		if ((type == THREE_SEC && regData_.dwAverageArchive3SecCounter == 0) ||
			(type == TEN_MIN && regData_.dwAverageArchive10MinCounter == 0) ||
			(type == TWO_HOUR && regData_.dwAverageArchive2HourCounter == 0))
		{
			EmService::WriteToLogGeneral("ReadAvgArchive: no records! exit");
			return false;
		}
		
		DWORD startIndex, endIndex;
		WORD wReplyCommand;
		WORD wReplyLength;
		WORD wReplyAddress;
		TAverageArchiveDescription axAverageArchiveDescriptions[2];

		WORD command = COMMAND_ReadAverageArchive3SecMinMaxIndices;
		if(type == TEN_MIN) command = COMMAND_ReadAverageArchive10MinMinMaxIndices;
		else if(type == TWO_HOUR) command = COMMAND_ReadAverageArchive2HourMinMaxIndices;

		EUsbResult UsbResult = usb_->UsbCommunication(		
			FALSE,
			READ_TIMEOUT_AVG,
			&wReplyAddress,
			&wReplyCommand,
			&wReplyLength,
			(void*)&axAverageArchiveDescriptions,
			sizeof(axAverageArchiveDescriptions),
			command,
			4,
			&regId_);

		if (UsbResult != USBRESULT_OK)
		{
			EmService::WriteToLogFailed("reply COMMAND_ReadAverageArchiveXXXMinMaxIndices ERROR");
		}
		else if (wReplyLength != 0)
		{
			EmService::WriteToLogGeneral("ReadAvgArchive: read 1: OK");
			startIndex = axAverageArchiveDescriptions[0].dwAverageArchiveIndex;
			endIndex = axAverageArchiveDescriptions[1].dwAverageArchiveIndex;
		}
		else
		{
			EmService::WriteToLogFailed("reply COMMAND_ReadAverageArchiveXXXMinMaxIndices EMPTY");
			startIndex = endIndex = 0xFFFFFFFF;
			regManager.WriteToPipeAboutError(UsbResult);
			return false;
		}

		//////////////////////////////////////////
		// prepare for writing to file
		//////////////////////////////////////////
		std::string fileName = EmService::RemoveAllExceptLettersAndDigits(objectName_);
		if(fileName.length() == 0) fileName = "AVG";

		std::stringstream ss_temp;
		TDateTime dtStart = axAverageArchiveDescriptions[0].xStartDateTime;
		ss_temp << fileName << '_' << EmService::AvgTypeToString(type) << '_' <<
			dtStart.wUtcYear << '_' <<
			EmService::MakeCorrectNumberLength(dtStart.wUtcMonth, 2) << '_' <<
			EmService::MakeCorrectNumberLength(dtStart.wUtcDate, 2) << "__" <<
			EmService::MakeCorrectNumberLength(dtStart.wUtcHours, 2) << '_' <<
			EmService::MakeCorrectNumberLength(dtStart.wUtcMinutes, 2) << '_' <<
			EmService::MakeCorrectNumberLength(dtStart.wUtcSeconds, 2) << ".tmpavg";

		EmService::WriteToLogGeneral(ss_temp.str());

		// check if folder for PQP archives exists
		if(!EmService::DirExists((dataPath_+ "\\AVG").c_str()))
		{
			BOOL res = CreateDirectory((dataPath_+ "\\AVG").c_str(), NULL);
			if(!res)
			{
				EmService::WriteToLogFailed("Unable to create the directory for the device");
				EmService::WriteToLogFailed(dataPath_+ "\\AVG");
				return false;
			}
		}

		// write header to file
		std::string filename = dataPath_ + "\\AVG\\" + ss_temp.str();

		// if file already exists then we won't replace it
		if(EmService::FileExists(filename.c_str()))
		{
			EmService::WriteToLogGeneral("AVG file already exists!");
			if(remove(filename.c_str()) == -1)
			{
				EmService::WriteToLogFailed("AVG file already exists and can't be deleted!");
				return false;
			}
		}

		std::ofstream ostr(filename, std::ios::binary | std::ios::out | std::ios::trunc);
		if(!ostr)
		{
			EmService::WriteToLogFailed("ReadAvgArchive: unable to open ofstream");
			return false;
		}

		int wholeHeaderLength = //EmService::DigitCount(wholeHeaderLength)
			3 /*file version: 3 digits*/ + 1 /*separator*/
			+ EmService::DigitCount(deviceSerialNumber_) + 1 /*separator*/
			+ EmService::DigitCount(regId_) + 1
			+ EmService::DigitCount((int)type) + 1
			+ EmService::DigitCount(sizeof(TRegistrationData)) + 1
			+ sizeof(TRegistrationData) + 1;

		std::string fileVersion = "001";
		// write header
		ostr << wholeHeaderLength << '|';
		// file version 
		ostr << fileVersion << '|';
		// serial number
		ostr << deviceSerialNumber_ << '|';
		// registration id
		ostr << regId_ << '|';

		// all registration data
		ostr << sizeof(TRegistrationData) << '|';
		ostr.write((const char*)&regData_, sizeof(TRegistrationData));
		ostr << '|';

		// avg type
		ostr << (int)type << '|';

		// read and write archive
		command = COMMAND_ReadAverageArchive3SecByIndex;
		if(type == TEN_MIN) command = COMMAND_ReadAverageArchive10MinByIndex;
		else if(type == TWO_HOUR) command = COMMAND_ReadAverageArchive2HourByIndex;

		DWORD adwAverageArchive[16 / 4 * 1024];
		WORD errorCounter = 0;
		EmService::WriteToLogGeneral("ReadAvgArchive: start reading archive");
		for(DWORD avgIndex = startIndex; avgIndex <= endIndex; avgIndex++)
		{
			UsbResult = usb_->UsbCommunication(		
				FALSE,
				READ_TIMEOUT,
				&wReplyAddress,
				&wReplyCommand,
				&wReplyLength,
				(void*)&adwAverageArchive,
				sizeof(adwAverageArchive),
				command,
				4,
				&avgIndex);

			if ((UsbResult == USBRESULT_OK) && (wReplyLength == AVG_PACKET_LENGTH))
			{
				errorCounter = 0;
				if (wReplyLength == AVG_PACKET_LENGTH)
				{
					//write data to file
					ostr.write((const char*)adwAverageArchive, sizeof(adwAverageArchive));
				}
				else    // no data
				{
					EmService::WriteToLogFailed("reply COMMAND_ReadAverageArchive3SecByIndex EMPTY");
				}
			}
			else
			{
				EmService::WriteToLogFailed("reply COMMAND_ReadAverageArchive3SecByIndex ERROR");
				if(++errorCounter < 10)
				{
					avgIndex--;
				}
			}

			// send report about progress of reading to pipe
			if((avgIndex % 10) == 0)
				regManager.WriteToPipeCurrenPercent((avgIndex - startIndex) * 100 / (endIndex - startIndex));
		}
		// send report about finish
		regManager.WriteToPipeCurrenPercent(100);
	
		ostr.close();

		EmService::WriteToLogGeneral("ReadAvgArchive: end");
		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in ReadAvgArchive()");
		throw;
	}
}

//===========================================================
//===========================================================
bool CRegistration::ReadDnsArchive(TDateTime& dtStart, RegistrationManager& regManager)
{
	try
	{
		EmService::WriteToLogGeneral("ReadDnsArchive: start");
		WORD wReplyCommand;
		WORD wReplyLength;
		WORD wReplyAddress;
		TDsiArchiveEntry* pxDsiArchiveEntries = new TDsiArchiveEntry[64];
		//WORD wNumberOfDsiArchives = 0;

		EUsbResult UsbResult = usb_->UsbCommunication(		
			FALSE,
			READ_TIMEOUT,
			&wReplyAddress,
			&wReplyCommand,
			&wReplyLength,
			(void*)pxDsiArchiveEntries,
			8192,
			COMMAND_ReadDSIArchivesByRegistration,
			4,
			&regId_);

		if (UsbResult == USBRESULT_TIMEOUT)
		{
			EmService::WriteToLogFailed("ReadDnsArchive: USBRESULT_TIMEOUT");
			regManager.WriteToPipeAboutError(UsbResult);
			return false;
		}

		std::ofstream ostr;
		// begin writing to the file
		if (wReplyLength != 0 && UsbResult == USBRESULT_OK)
		{
			//////////////////////////////////////////
			// prepare for writing to file
			//////////////////////////////////////////
			std::string fileName = EmService::RemoveAllExceptLettersAndDigits(objectName_);
			if(fileName.length() == 0) fileName = "DNS";

			std::stringstream ss_temp;
			ss_temp << fileName << '_' << dtStart.wUtcYear << '_' <<
				EmService::MakeCorrectNumberLength(dtStart.wUtcMonth, 2) << '_' <<
				EmService::MakeCorrectNumberLength(dtStart.wUtcDate, 2) << "__" <<
				EmService::MakeCorrectNumberLength(dtStart.wUtcHours, 2) << '_' <<
				EmService::MakeCorrectNumberLength(dtStart.wUtcMinutes, 2) << '_' <<
				EmService::MakeCorrectNumberLength(dtStart.wUtcSeconds, 2) << ".tmpdns";

			// check if folder for PQP archives exists
			if(!EmService::DirExists((dataPath_+ "\\DNS").c_str()))
			{
				BOOL res = CreateDirectory((dataPath_+ "\\DNS").c_str(), NULL);
				if(!res)
				{
					EmService::WriteToLogFailed("Unable to create the directory for the device");
					EmService::WriteToLogFailed(dataPath_+ "\\DNS");
					return false;
				}
			}

			std::string filename = dataPath_ + "\\DNS\\" + ss_temp.str();

			// if file already exists then we won't replace it
			if(EmService::FileExists(filename.c_str()))
			{
				EmService::WriteToLogGeneral("DNS file already exists!");
			if(remove(filename.c_str()) == -1)
			{
				EmService::WriteToLogFailed("DNS file already exists and can't be deleted!");
				return false;
			}
			}

			ostr.open(filename, std::ios::binary | std::ios::out | std::ios::trunc);
			if(!ostr)
			{
				EmService::WriteToLogFailed("ReadDnsArchive: unable to open ofstream");
				return false;
			}

			int wholeHeaderLength = //EmService::DigitCount(wholeHeaderLength)
					3 /*file version: 3 digits*/ + 1 /*separator*/
					+ EmService::DigitCount(deviceSerialNumber_) + 1 /*separator*/
					+ EmService::DigitCount(regId_) + 1
					+ EmService::DigitCount(sizeof(TRegistrationData)) + 1
					+ sizeof(TRegistrationData) + 1;

			std::string fileVersion = "001";
			// write header to the file
			ostr << wholeHeaderLength << '|';
			// file version 
			ostr << fileVersion << '|';
			// serial number
			ostr << deviceSerialNumber_ << '|';
			// registration id
			ostr << regId_ << '|';

			// all registration data
			ostr << sizeof(TRegistrationData) << '|';
			ostr.write((const char*)&regData_, sizeof(TRegistrationData));
			ostr << '|';

			ostr.write((const char*)pxDsiArchiveEntries, wReplyLength);
		}

		if (wReplyLength != 0)
		{
			//wNumberOfDsiArchives += (wReplyLength/sizeof(TDsiArchiveEntry));
			int iRecord = 0;
			while(1)
			{
				UsbResult = usb_->UsbCommunication(		
					TRUE,
					READ_TIMEOUT,
					&wReplyAddress,
					&wReplyCommand,
					&wReplyLength,
					(void*)pxDsiArchiveEntries,
					8192);

				if ((wReplyLength == 0))
				{
					break;
				}
				else if (UsbResult == USBRESULT_OK)
				{
					//wNumberOfDsiArchives += (wReplyLength/sizeof(TDsiArchiveEntry));
					ostr.write((const char*)pxDsiArchiveEntries, wReplyLength);
				}
				else
				{
					EmService::WriteToLogFailed("Reading DNS archive: UsbResult != USBRESULT_OK");
					//regManager.WriteToPipeAboutError(UsbResult);
				}

				// send report about progress of reading to pipe
				if((iRecord % 10) == 0)
					regManager.WriteToPipeCurrenPercent(50);	// dummy
				iRecord++;
			}
			// send report about finish of reading to pipe
			regManager.WriteToPipeCurrenPercent(100);
		}

		ostr.close();
		delete[] pxDsiArchiveEntries;

		EmService::WriteToLogGeneral("ReadDnsArchive: end");
		return true;
	}
	catch (...)
	{
		EmService::WriteToLogFailed("Error in ReadDnsArchive()");
		throw;
	}
}

