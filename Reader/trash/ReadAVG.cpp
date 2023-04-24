
#define _CRT_SECURE_NO_WARNINGS

#include <windows.h>
#include <stdio.h>
#include <math.h>

#include "SysData.h"
//#include "MDI_DeviceDiagnostics.h"
#include "ReadAVG.h"
#include "EmUsb.h"

#define M_PI (3.14159265358979)

HANDLE hThread_ReadMeasurements3Sec;
HANDLE hThread_ReadMeasurements10Min;
HANDLE hThread_ReadMeasurements2Hour;

WORD awMeasurements3Sec[8192];

void WriteDateTimeToFile(FILE *file,char *szLegend,TDateTime *pxDateTime);
void WriteDwordToFile(FILE *file,char *szLegend,BYTE *pb);
void WriteWordToFile(FILE *file,char *szLegend,BYTE *pb);
void WriteFrequencyToFile(FILE *file,char *szLegend,DWORD *pdw);
void WriteVoltageCurrentToFile(FILE *file,char *szLegend,LONG *plRMS,LONG *pl0,LONG *plR,LONG *pl1,float fMultiplier);
void WritePowerToFile(FILE *file,char *szLegend,LONG *pl,float fVoltageMultiplier,float fCurrentMultiplier);
void WritePowerFactorToFile(FILE *file,char *szLegend,LONG *plKp,LONG *plQ,WORD wNONonLockedWindows);
void WriteAngleToFile(FILE *file,char *szLegend,LONG *pl);
void WriteAngleFromCmplxProductToFile(FILE *file,char *szLegend,WORD *pwBuffer,WORD wIndex);
double GetAngleDegrees(double dRe,double dIm);

double dUAUA,dUBUB,dUCUC; 								
double dReUAUB,dReUBUC,dReUCUA,dImUAUB,dImUBUC,dImUCUA;
double dAngleUABUBC;
double dAngleUBCUCA;
double dAngleUCAUAB;

double DspFloat(DWORD dw)
{
	DWORD dwMant;
	BYTE bExp;
	dwMant = dw & 0xFFFFFF00;
	bExp = (BYTE)(dw & 0x000000FF);
	return (*(LONG*)&dwMant)*pow(2.0,(double)(*(CHAR*)&bExp));
}

void WriteMeasurementsToFile(FILE *fResult,WORD *awMeasurements3Sec);

//==================================================================================
//==================================================================================
DWORD WINAPI ThreadFunc_ReadMeasurements3Sec(LPVOID lpv)
{
	TUsbResult UsbResult;
	WORD wReplyCommand;
	WORD wReplyLength;
	WORD wReplyAddress;
	FILE *fResult;
	char szResultFileName[MAX_PATH];
	char szResultFilePath[MAX_PATH];

	if (UsbConnect() == FALSE)
	{
		MessageBox(NULL, "UsbConnect() FAILED", "FATAL ERROR", MB_OK);
		UsbDisconnect();
		DeviceDiagnostics_StopActivity();
		return 0;
	}

	UsbResult = UsbCommunication(		
		FALSE,
		COMMAND_ReadMeasurements3Sec,
		0,
		NULL,
		5,
		&wReplyAddress,
		&wReplyCommand,
		&wReplyLength,
		(void*)&(awMeasurements3Sec[0]),
		16384);

	if (wReplyLength != 0)
	{
		CommunicationResultFileName(wReplyAddress, "ReadMeasurements3Sec.txt", szResultFileName, TRUE);
		GetCurrentDirectory(MAX_PATH,szResultFilePath);
		strcat(szResultFilePath,"\\");
		strcat(szResultFilePath,szResultFileName);

		fResult = fopen(szResultFileName,"w");
		WriteMeasurementsToFile(fResult,awMeasurements3Sec);
		fclose(fResult);
	}
	else
	{
		MessageBox(NULL, "no data available", "", MB_OK);
	}

	UsbDisconnect();
	DeviceDiagnostics_StopActivity();
	return 0;
}
//===========================================================================
//===========================================================================
DWORD WINAPI ThreadFunc_ReadMeasurements10Min(LPVOID lpv)
{
	TUsbResult UsbResult;
	WORD wReplyCommand;
	WORD wReplyLength;
	WORD wReplyAddress;
	FILE *fResult;
	char szResultFileName[MAX_PATH];
	char szResultFilePath[MAX_PATH];

	if (UsbConnect() == FALSE)
	{
		MessageBox(NULL, "UsbConnect() FAILED", "FATAL ERROR", MB_OK);
		UsbDisconnect();
		DeviceDiagnostics_StopActivity();
		return 0;
	}

	UsbResult = UsbCommunication(		
		FALSE,
		COMMAND_ReadMeasurements10Min,
		0,
		NULL,
		5,
		&wReplyAddress,
		&wReplyCommand,
		&wReplyLength,
		(void*)&(awMeasurements3Sec[0]),
		16384);

	if (wReplyLength != 0)
	{
		CommunicationResultFileName(wReplyAddress, "ReadMeasurements10Min.txt", szResultFileName, TRUE);
		GetCurrentDirectory(MAX_PATH, szResultFilePath);
		strcat(szResultFilePath, "\\");
		strcat(szResultFilePath, szResultFileName);

		fResult = fopen(szResultFileName, "w");
		WriteMeasurementsToFile(fResult, awMeasurements3Sec);
		fclose(fResult);
	}
	else
	{
		MessageBox(NULL, "no data available", "", MB_OK);
	}

	UsbDisconnect();
	DeviceDiagnostics_StopActivity();
	return 0;
}
//=========================================================================
//=========================================================================
DWORD WINAPI ThreadFunc_DeviceDiagnostics_ReadMeasurements2Hour(LPVOID lpv)
{
	TUsbResult UsbResult;
	WORD wReplyCommand;
	WORD wReplyLength;
	WORD wReplyAddress;
	FILE *fResult;
	char szResultFileName[MAX_PATH];
	char szResultFilePath[MAX_PATH];

	if (UsbConnect() == FALSE)
	{
		MessageBox(NULL, "UsbConnect() FAILED", "FATAL ERROR", MB_OK);
		UsbDisconnect();
		DeviceDiagnostics_StopActivity();
		return 0;
	}

	UsbResult = UsbCommunication(		
		FALSE,
		COMMAND_ReadMeasurements2Hour,
		0,
		NULL,
		5,
		&wReplyAddress,
		&wReplyCommand,
		&wReplyLength,
		(void*)&(awMeasurements3Sec[0]),
		16384);

	if (wReplyLength != 0)
	{
		CommunicationResultFileName(wReplyAddress, "ReadMeasurements2Hour.txt", szResultFileName, TRUE);
		GetCurrentDirectory(MAX_PATH, szResultFilePath);
		strcat(szResultFilePath, "\\");
		strcat(szResultFilePath, szResultFileName);

		fResult = fopen(szResultFileName, "w");
		WriteMeasurementsToFile(fResult, awMeasurements3Sec);
		fclose(fResult);
	}
	else
	{
		MessageBox(NULL, "no data available", "", MB_OK);
	}

	UsbDisconnect();
	DeviceDiagnostics_StopActivity();
	return 0;

}
//===================================================================
//===================================================================
void WriteDateTimeToFile(FILE *file,char *szLegend,TDateTime *pxDateTime)
{
	fprintf(file,"%s: [UTC:] %02d/%02d/%04d %02d:%02d:%02d.%03d [RTC:] %02d/%02d/%04d %02d:%02d:%02d.%03d \n",
		szLegend,

		pxDateTime->wUtcDate,
		pxDateTime->wUtcMonth,
		pxDateTime->wUtcYear,
		pxDateTime->wUtcHours,
		pxDateTime->wUtcMinutes,
		pxDateTime->wUtcSeconds,
		pxDateTime->wMilliseconds,

		pxDateTime->wLocalDate,
		pxDateTime->wLocalMonth,
		pxDateTime->wLocalYear,
		pxDateTime->wLocalHours,
		pxDateTime->wLocalMinutes,
		pxDateTime->wLocalSeconds,
		pxDateTime->wMilliseconds
		);
}
//==============================================================
//==============================================================
void WriteDwordToFile(FILE *file,char *szLegend,BYTE *pb)
{
	DWORD dw;
	BYTE *pbDst;
	pbDst = (BYTE*)&dw;

	pbDst[0] = pb[2];
	pbDst[1] = pb[3];
	pbDst[2] = pb[0];
	pbDst[3] = pb[1];
	
	fprintf(file,"%s: %d\n",szLegend,dw);
}
//==============================================================
//==============================================================
void WriteWordToFile(FILE *file,char *szLegend,BYTE *pb)
{
	WORD w;
	BYTE *pbDst;
	pbDst = (BYTE*)&w;

	pbDst[0] = pb[0];
	pbDst[1] = pb[1];
	
	fprintf(file,"%s: %d\n",szLegend,w);

}
//===============================================================
//===============================================================
void WriteFrequencyToFile(FILE *file,char *szLegend,DWORD *pdw)
{
	DWORD dw = *pdw;
	
	if (dw==0)
	{
		fprintf(file,"%s: -----\n",szLegend);
	}
	else
	{
		fprintf(file,"%s: %.3fHz\n",szLegend,(float)dw/(float)1000.0);
	}

}
//===============================================================
//===============================================================
void WriteVoltageCurrentToFile(FILE *file,char *szLegend,LONG *plRMS,LONG *pl0,LONG *plR,LONG *pl1,float fMultiplier)
{
	LONG lRMS,l0,lR,l1;
	lRMS = *plRMS;
	l0 = *pl0;
	lR = *plR;
	l1 = *pl1;

	fprintf(file,"%s: %+7.3f / %+7.3f / %+7.3f / %+7.3f \n",szLegend,
		(float)lRMS/(float)1000000.0*fMultiplier,
		(float)l0/(float)1000000.0*fMultiplier,
		(float)lR/(float)1000000.0*fMultiplier,
		(float)l1/(float)1000000.0*fMultiplier);

}
//================================================================
//================================================================
void WritePowerToFile(FILE *file,char *szLegend,LONG *pl,float fVoltageMultiplier,float fCurrentMultiplier)
{
	LONG l = *pl;
	fprintf(file,"%s: %+7.5f \n",szLegend,(float)l/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier);

}
//================================================================
//================================================================
void WritePowerFactorToFile(FILE *file,char *szLegend,LONG *plKp,LONG *plQ,WORD wNONonLockedWindows)
{
	LONG lKp = *plKp;
	LONG lQ = *plQ;

	if (wNONonLockedWindows==0)
	{
		if (lKp>=0)
		{
			if (lQ>=0)
			{
				fprintf(file,"%s: %+7.5fL \n",szLegend,(float)lKp*(float)(4.65661287307739e-010));
			}
			else
			{
				fprintf(file,"%s: %+7.5fC \n",szLegend,(float)lKp*(float)(4.65661287307739e-010));
			}
		}
		else
		{
			if (lQ>=0)
			{
				fprintf(file,"%s: %+7.5fC \n",szLegend,(float)lKp*(float)(4.65661287307739e-010));
			}
			else
			{
				fprintf(file,"%s: %+7.5fL \n",szLegend,(float)lKp*(float)(4.65661287307739e-010));
			}
		}	
	}
	else
	{
		fprintf(file,"%s: %+7.5f? \n",szLegend,(float)lKp*(float)(4.65661287307739e-010));
	}

}
//====================================================================
//====================================================================
void WriteAngleToFile(FILE *file,char *szLegend,LONG *pl)
{
	if ((*pl)==-2147483648)
	{
		fprintf(file,"%s: -----\n",szLegend);
	}
	else
	{
		fprintf(file,"%s: %+8.3f^\n",szLegend,(float)(*pl)/(float)1000.0);
	}
}
//====================================================================
//====================================================================
double GetAngleDegrees(double dRe,double dIm)
{
	double dA;
	if (abs(dRe)>=abs(dIm))
	{
		dA = atan(abs(dIm)/abs(dRe))/M_PI*180.0;		
		if(dRe>=0)
		{
			if (dIm>=0) //Q1
			{
				dA=dA;
			}
			else //Q4
			{
				dA=-dA;
			}
		}
		else
		{
			if (dIm>=0) //Q2
			{
				dA=180-dA;
			}
			else //Q3
			{
				dA=180+dA;
			}
		}
	}
	else
	{
		dA = atan(abs(dRe)/abs(dIm))/M_PI*180.0;		
		if(dRe>=0)
		{
			if (dIm>=0) //Q1
			{
				dA=90-dA;
			}
			else //Q4
			{
				dA=-90+dA;
			}
		}
		else
		{
			if (dIm>=0) //Q2
			{
				dA=90+dA;
			}
			else //Q3
			{
				dA=-90-dA;
			}
		}

	}

	while(1)
	{
		if (dA>180)
		{
			dA-=180;
		}
		else
		{
			break;
		}
	}
	while(1)
	{
		if (dA<-180)
		{
			dA+=180;
		}
		else
		{
			break;
		}
	}
	return dA;
}

void WriteAngleFromCmplxProductToFile(FILE *file,char *szLegend,WORD *pwBuffer,WORD wIndex)
{
	double dRe,dIm;
	dRe = DspFloat(*(DWORD*)&pwBuffer[wIndex]);
	dIm = DspFloat(*(DWORD*)&pwBuffer[wIndex+2]);
	fprintf(file," угол %s = %+8.3fdeg\n",szLegend,GetAngleDegrees(dRe,dIm));
}
//====================================================================
//====================================================================
void WriteMeasurementsToFile(FILE *fResult,WORD *awMeasurements3Sec)
{

	float fVoltageMultiplier=0;
	float fCurrentMultiplier=0;

		{
			TDateTime *pxStartDateTime;
			TDateTime *pxStopDateTime;

			pxStartDateTime = (TDateTime*)&(awMeasurements3Sec[ AA_StartDateTime ]);
			pxStopDateTime = (TDateTime*)&(awMeasurements3Sec[ AA_StopDateTime ]);
			WriteDateTimeToFile(fResult,"Время начала интервала   ",pxStartDateTime);
			WriteDateTimeToFile(fResult,"Время окончания интервала",pxStopDateTime);
			fprintf(fResult,"Длительность интервала: %.3f сек\n",
				((float)(*(DWORD*)&(awMeasurements3Sec[ AA_ArchiveDurationMilliseconds ])))/(float)1000.0);
			switch(pxStartDateTime->wTimeZone)
			{
				case 0:
					fprintf(fResult,"Временная зона: TZ_USZ1 (Калиниградское) (MSK–1) (UTC+3)\n");
					break;
				case 1:
					fprintf(fResult,"Временная зона: TZ_MSK (Московское) (MSK) (UTC+4)\n");
					break;
				case 2:
					fprintf(fResult,"Временная зона: TZ_YEKT (Екатеринбургское) (MSK+2) (UTC+6)\n");
					break;
				case 3:
					fprintf(fResult,"Временная зона: TZ_OMST (Омское) (MSK+3) (UTC+7)\n");
					break;
				case 4:
					fprintf(fResult,"Временная зона: TZ_KRAT (Красноярское) (MSK+4) (UTC+8)\n");
					break;
				case 5:
					fprintf(fResult,"Временная зона: TZ_IRKT (Иркутское) (MSK+5) (UTC+9)\n");
					break;
				case 6:
					fprintf(fResult,"Временная зона: TZ_YAKT (Якутское) (MSK+6) (UTC+10)\n");
					break;
				case 7:
					fprintf(fResult,"Временная зона: TZ_VLAT (Владивостокское) (MSK+7) (UTC+11)\n");
					break;
				case 8:
					fprintf(fResult,"Временная зона: TZ_MAGT (Магаданское) (MSK+8) (UTC+12)\n");
					break;
			}

		}

		{
			fprintf(fResult,"\n");
			WriteWordToFile(fResult,"Количество окон с потерей синхронизации      ",(BYTE*)&(awMeasurements3Sec[ AA_NOWindowsNonlocked ]));
			WriteWordToFile(fResult,"Количество окон с синхронизацией от фазы A   ",(BYTE*)&(awMeasurements3Sec[ AA_NOWindowsLockedA ]));
			WriteWordToFile(fResult,"Количество окон с синхронизацией от фазы B   ",(BYTE*)&(awMeasurements3Sec[ AA_NOWindowsLockedB ]));
			WriteWordToFile(fResult,"Количество окон с синхронизацией от фазы C   ",(BYTE*)&(awMeasurements3Sec[ AA_NOWindowsLockedC ]));
			WriteWordToFile(fResult,"Количество окон с синхронизацией от линии AB ",(BYTE*)&(awMeasurements3Sec[ AA_NOWindowsLockedAB ]));
			WriteWordToFile(fResult,"Количество окон с синхронизацией от линии BC ",(BYTE*)&(awMeasurements3Sec[ AA_NOWindowsLockedBC ]));
			WriteWordToFile(fResult,"Количество окон с синхронизацией от линии CA ",(BYTE*)&(awMeasurements3Sec[ AA_NOWindowsLockedCA ]));

			fprintf(fResult,"\n");
			fprintf(fResult,"Количество сэмплов в окнах (всего %d):\n",*(DWORD*)&(awMeasurements3Sec[ AA_NOSamplesInArchive ]));
			for(int iW = 0;iW<15;iW++)
			{
				WORD wNOSamples;
				wNOSamples = *(WORD*)&(awMeasurements3Sec[ AA_NOSamples1stWindow+iW ]);
				if ((wNOSamples&(1<<15))==0)
				{
					fprintf(fResult," #%.2d: %d\n",iW+1,wNOSamples);
				}
				else
				{
					wNOSamples = wNOSamples&0x7FFF;
					fprintf(fResult," #%.2d: %d (потеря синхронизации)\n",iW+1,wNOSamples);
				}
			}
			fprintf(fResult,"Количество 15-оконных интервалов в 10-минутном усреднении: %d\n",
				awMeasurements3Sec[ AA_15WinIn10MinCounter ]);
			fprintf(fResult,"Индекс 10-минутной ветки: %d\n",awMeasurements3Sec[ AA_10MinBranch ]);

		}

		{
			fprintf(fResult,"\n");
			fprintf(fResult,"Текущие параметры прибора:\n");

			switch(awMeasurements3Sec[AA_ActiveConnection])
			{
				case 0:
					fprintf(fResult," Схема - 1ф1пр\n");
					break;
				case 1:
					fprintf(fResult," Схема - 3ф4пр\n");
					break;
				case 2:
					fprintf(fResult," Схема - 3ф3пр3т\n");
					break;
				case 3:
					fprintf(fResult," Схема - 3ф3пр2т(схема Арона)\n");
					break;
			}

			if (awMeasurements3Sec[AA_MainsSynchronization]==MyTRUE)
			{
				fprintf(fResult," Синхронизация с сетью - разрешена\n");
			}
			else
			{
				fprintf(fResult," Синхронизация с сетью - запрещена\n");
			}

			fprintf(fResult," Пределы: %dV / %dA ",awMeasurements3Sec[AA_VoltageRange],
				awMeasurements3Sec[AA_CurrentRange]);
			fCurrentMultiplier = (float)awMeasurements3Sec[AA_CurrentRange];
			switch(awMeasurements3Sec[AA_CurrentSensorType])
			{
				case 0:
					fprintf(fResult,"(клещи) ");
					break;
				case 1:
					fprintf(fResult,"(гибкие клещи) ");
					break;
				case 2:
					fprintf(fResult,"(трансформаторы тока) ");
					break;
			}
			fprintf(fResult,"/ %dHz\n",awMeasurements3Sec[AA_FrequencyRange]);



			if (awMeasurements3Sec[AA_VoltageTransformerEnable]==0)
			{
				fVoltageMultiplier = afVoltageTransformationRatios[awMeasurements3Sec[AA_VoltageTransformerType]];
				fprintf(fResult," Трансформатор напряжения фазный %f:1\n",fVoltageMultiplier);
			}
			else
			{
				fVoltageMultiplier = 1.0;
				fprintf(fResult," Трансформатор напряжения не используется\n");
			}


			switch(awMeasurements3Sec[AA_CurrentTransformerUsage])
			{
				case 0:
					fprintf(fResult," Трансформатор тока не используется\n");
					break;
				case 1:
					fprintf(fResult," Трансформатор тока %d:1A\n",awMeasurements3Sec[AA_CurrentTransformerPrimary]);
					fCurrentMultiplier = fCurrentMultiplier*(float)(awMeasurements3Sec[AA_CurrentTransformerPrimary]);
					break;
				case 2:
					fprintf(fResult," Трансформатор тока %d:5A\n",awMeasurements3Sec[AA_CurrentTransformerPrimary]);
					fCurrentMultiplier = fCurrentMultiplier*(float)(awMeasurements3Sec[AA_CurrentTransformerPrimary])/5.0f;
					break;
			}

			fprintf(fResult," Согласованное напряжение: %d\n",*(LONG*)&(awMeasurements3Sec[ AA_DeclaredVoltage ]));

			if (awMeasurements3Sec[AA_DateTimeAutomaticCorrection]==MyTRUE)
			{
				fprintf(fResult," Автоматическая коррекция даты/времени по GPS - разрешена\n");
			}
			else
			{
				fprintf(fResult," Автоматическая коррекция даты/времени по GPS - запрещена\n");
			}

			fprintf(fResult," Имя объекта: %16s\n",(CHAR*)&awMeasurements3Sec[AA_ObjectName]);
		}

		{
			fprintf(fResult," AA_10MinBranchMaxPImport: %d\n",*(LONG*)&(awMeasurements3Sec[ AA_10MinBranchMaxPImport ]));
			fprintf(fResult," AA_10MinBranchMaxQImport: %d\n",*(LONG*)&(awMeasurements3Sec[ AA_10MinBranchMaxQImport ]));
			fprintf(fResult," AA_10MinBranchMaxPExport: %d\n",*(LONG*)&(awMeasurements3Sec[ AA_10MinBranchMaxPExport ]));
			fprintf(fResult," AA_10MinBranchMaxQExport: %d\n",*(LONG*)&(awMeasurements3Sec[ AA_10MinBranchMaxQExport ]));

			TDateTime *pxDateTime;
			pxDateTime = (TDateTime*)&(awMeasurements3Sec[ AA_10MinBranchDateTime ]);
			WriteDateTimeToFile(fResult,"AA_10MinBranchDateTime   ",pxDateTime);

		}

		{
			fprintf(fResult,"\n");
			fprintf(fResult,"Частота:\n");
			WriteFrequencyToFile(fResult,"  A",(DWORD*)&(awMeasurements3Sec[ AA_MainsFrequencyA ]));
			WriteFrequencyToFile(fResult,"  B",(DWORD*)&(awMeasurements3Sec[ AA_MainsFrequencyB ]));
			WriteFrequencyToFile(fResult,"  C",(DWORD*)&(awMeasurements3Sec[ AA_MainsFrequencyC ]));
			WriteFrequencyToFile(fResult," AB",(DWORD*)&(awMeasurements3Sec[ AA_MainsFrequencyAB ]));
			WriteFrequencyToFile(fResult," BC",(DWORD*)&(awMeasurements3Sec[ AA_MainsFrequencyBC ]));
			WriteFrequencyToFile(fResult," CA",(DWORD*)&(awMeasurements3Sec[ AA_MainsFrequencyCA ]));

		}

		{
			fprintf(fResult,"\n");
			fprintf(fResult,"Напряжение (rms/0/rect/1):\n");

			WriteVoltageCurrentToFile(fResult,"  A",
				(LONG*)&(awMeasurements3Sec[ AA_UA_RMS ]),
				(LONG*)&(awMeasurements3Sec[ AA_UA_0 ]),
				(LONG*)&(awMeasurements3Sec[ AA_UA_R ]),
				(LONG*)&(awMeasurements3Sec[ AA_UA_1 ]),
				fVoltageMultiplier);

			WriteVoltageCurrentToFile(fResult,"  B",
				(LONG*)&(awMeasurements3Sec[ AA_UB_RMS ]),
				(LONG*)&(awMeasurements3Sec[ AA_UB_0 ]),
				(LONG*)&(awMeasurements3Sec[ AA_UB_R ]),
				(LONG*)&(awMeasurements3Sec[ AA_UB_1 ]),
				fVoltageMultiplier);

			WriteVoltageCurrentToFile(fResult,"  C",
				(LONG*)&(awMeasurements3Sec[ AA_UC_RMS ]),
				(LONG*)&(awMeasurements3Sec[ AA_UC_0 ]),
				(LONG*)&(awMeasurements3Sec[ AA_UC_R ]),
				(LONG*)&(awMeasurements3Sec[ AA_UC_1 ]),
				fVoltageMultiplier);

			WriteVoltageCurrentToFile(fResult," AB",
				(LONG*)&(awMeasurements3Sec[ AA_UAB_RMS ]),
				(LONG*)&(awMeasurements3Sec[ AA_UAB_0 ]),
				(LONG*)&(awMeasurements3Sec[ AA_UAB_R ]),
				(LONG*)&(awMeasurements3Sec[ AA_UAB_1 ]),
				fVoltageMultiplier);

			WriteVoltageCurrentToFile(fResult," BC",
				(LONG*)&(awMeasurements3Sec[ AA_UBC_RMS ]),
				(LONG*)&(awMeasurements3Sec[ AA_UBC_0 ]),
				(LONG*)&(awMeasurements3Sec[ AA_UBC_R ]),
				(LONG*)&(awMeasurements3Sec[ AA_UBC_1 ]),
				fVoltageMultiplier);

			WriteVoltageCurrentToFile(fResult," CA",
				(LONG*)&(awMeasurements3Sec[ AA_UCA_RMS ]),
				(LONG*)&(awMeasurements3Sec[ AA_UCA_0 ]),
				(LONG*)&(awMeasurements3Sec[ AA_UCA_R ]),
				(LONG*)&(awMeasurements3Sec[ AA_UCA_1 ]),
				fVoltageMultiplier);
		}

		{
			fprintf(fResult,"\n");
			fprintf(fResult,"Ток (rms/0/rect/1):\n");

			WriteVoltageCurrentToFile(fResult,"  A",
				(LONG*)&(awMeasurements3Sec[ AA_IA_RMS ]),
				(LONG*)&(awMeasurements3Sec[ AA_IA_0 ]),
				(LONG*)&(awMeasurements3Sec[ AA_IA_R ]),
				(LONG*)&(awMeasurements3Sec[ AA_IA_1 ]),
				fCurrentMultiplier);

			WriteVoltageCurrentToFile(fResult,"  B",
				(LONG*)&(awMeasurements3Sec[ AA_IB_RMS ]),
				(LONG*)&(awMeasurements3Sec[ AA_IB_0 ]),
				(LONG*)&(awMeasurements3Sec[ AA_IB_R ]),
				(LONG*)&(awMeasurements3Sec[ AA_IB_1 ]),
				fCurrentMultiplier);

			WriteVoltageCurrentToFile(fResult,"  C",
				(LONG*)&(awMeasurements3Sec[ AA_IC_RMS ]),
				(LONG*)&(awMeasurements3Sec[ AA_IC_0 ]),
				(LONG*)&(awMeasurements3Sec[ AA_IC_R ]),
				(LONG*)&(awMeasurements3Sec[ AA_IC_1 ]),
				fCurrentMultiplier);

			WriteVoltageCurrentToFile(fResult,"  N",
				(LONG*)&(awMeasurements3Sec[ AA_IN_RMS ]),
				(LONG*)&(awMeasurements3Sec[ AA_IN_0 ]),
				(LONG*)&(awMeasurements3Sec[ AA_IN_R ]),
				(LONG*)&(awMeasurements3Sec[ AA_IN_1 ]),
				fCurrentMultiplier);
		}

		{
			fprintf(fResult,"\n");
			fprintf(fResult,"Мощность (активная):\n");
			WritePowerToFile(fResult,"  A",(LONG*)&(awMeasurements3Sec[ AA_PA ]),fVoltageMultiplier,fCurrentMultiplier);
			WritePowerToFile(fResult,"  B",(LONG*)&(awMeasurements3Sec[ AA_PB ]),fVoltageMultiplier,fCurrentMultiplier);
			WritePowerToFile(fResult,"  C",(LONG*)&(awMeasurements3Sec[ AA_PC ]),fVoltageMultiplier,fCurrentMultiplier);
			WritePowerToFile(fResult,"SUM",(LONG*)&(awMeasurements3Sec[ AA_PABC ]),fVoltageMultiplier,fCurrentMultiplier);

			fprintf(fResult,"\n");
			fprintf(fResult,"Мощность (реактивная):\n");
			WritePowerToFile(fResult,"  A",(LONG*)&(awMeasurements3Sec[ AA_QA ]),fVoltageMultiplier,fCurrentMultiplier);
			WritePowerToFile(fResult,"  B",(LONG*)&(awMeasurements3Sec[ AA_QB ]),fVoltageMultiplier,fCurrentMultiplier);
			WritePowerToFile(fResult,"  C",(LONG*)&(awMeasurements3Sec[ AA_QC ]),fVoltageMultiplier,fCurrentMultiplier);
			WritePowerToFile(fResult,"SUM",(LONG*)&(awMeasurements3Sec[ AA_QABC ]),fVoltageMultiplier,fCurrentMultiplier);

			fprintf(fResult,"\n");
			fprintf(fResult,"Мощность (полная):\n");
			WritePowerToFile(fResult,"  A",(LONG*)&(awMeasurements3Sec[ AA_SA ]),fVoltageMultiplier,fCurrentMultiplier);
			WritePowerToFile(fResult,"  B",(LONG*)&(awMeasurements3Sec[ AA_SB ]),fVoltageMultiplier,fCurrentMultiplier);
			WritePowerToFile(fResult,"  C",(LONG*)&(awMeasurements3Sec[ AA_SC ]),fVoltageMultiplier,fCurrentMultiplier);
			WritePowerToFile(fResult,"SUM",(LONG*)&(awMeasurements3Sec[ AA_SABC ]),fVoltageMultiplier,fCurrentMultiplier);

			fprintf(fResult,"\n");
			fprintf(fResult,"Коэффициент мощности:\n");
			WritePowerFactorToFile(fResult,"  A",(LONG*)&(awMeasurements3Sec[ AA_KpA ]),(LONG*)&(awMeasurements3Sec[ AA_QA ]),*(WORD*)&(awMeasurements3Sec[ AA_NOWindowsNonlocked ]));
			WritePowerFactorToFile(fResult,"  B",(LONG*)&(awMeasurements3Sec[ AA_KpB ]),(LONG*)&(awMeasurements3Sec[ AA_QB ]),*(WORD*)&(awMeasurements3Sec[ AA_NOWindowsNonlocked ]));
			WritePowerFactorToFile(fResult,"  C",(LONG*)&(awMeasurements3Sec[ AA_KpC ]),(LONG*)&(awMeasurements3Sec[ AA_QC ]),*(WORD*)&(awMeasurements3Sec[ AA_NOWindowsNonlocked ]));
			WritePowerFactorToFile(fResult,"SUM",(LONG*)&(awMeasurements3Sec[ AA_KpABC ]),(LONG*)&(awMeasurements3Sec[ AA_QABC ]),*(WORD*)&(awMeasurements3Sec[ AA_NOWindowsNonlocked ]));
		}

		{
			fprintf(fResult,"\n");
			fprintf(fResult,"Напряжения и коэффициенты последовательностей:\n");

			fprintf(fResult,"  U1: %7.3fВ (%+8.3f%%)\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_U11]))/1000000.0, (float)(*(LONG*)&(awMeasurements3Sec[AA_rdU11]))/134217728.0*100.0);
			fprintf(fResult,"  U2: %7.3fВ (%8.3f%%)\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_U21]))/1000000.0, (float)(*(LONG*)&(awMeasurements3Sec[AA_K21]))/134217728.0*100.0);
			fprintf(fResult,"  U0: %7.3fВ (%8.3f%%)\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_U01]))/1000000.0, (float)(*(LONG*)&(awMeasurements3Sec[AA_K01]))/134217728.0*100.0);
		}

		{
			fprintf(fResult,"\n");
			fprintf(fResult,"Токи последовательностей:\n");
			fprintf(fResult,"  I11: %7.3fA\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_I11]))/1000000.0*fCurrentMultiplier);
			fprintf(fResult,"  I21: %7.3fA\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_I21]))/1000000.0*fCurrentMultiplier);
			fprintf(fResult,"  I01: %7.3fA\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_I01]))/1000000.0*fCurrentMultiplier);
		}

		{
			fprintf(fResult,"\n");
			fprintf(fResult,"Мощности последовательностей:\n");
			fprintf(fResult,"  P11: %7.3fВт\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_P11]))/1000000.0*fCurrentMultiplier*fVoltageMultiplier);
			fprintf(fResult,"  P21: %7.3fВт\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_P21]))/1000000.0*fCurrentMultiplier*fVoltageMultiplier);
			fprintf(fResult,"  P01: %7.3fВт\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_P01]))/1000000.0*fCurrentMultiplier*fVoltageMultiplier);
			fprintf(fResult,"  Q11: %7.3fВт\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_Q11]))/1000000.0*fCurrentMultiplier*fVoltageMultiplier);
			fprintf(fResult,"  Q21: %7.3fВт\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_Q21]))/1000000.0*fCurrentMultiplier*fVoltageMultiplier);
			fprintf(fResult,"  Q01: %7.3fВт\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_Q01]))/1000000.0*fCurrentMultiplier*fVoltageMultiplier);

			fprintf(fResult,"  <P11: %+8.3f^\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_AngleP11]))/(float)1000.0);
			fprintf(fResult,"  <P21: %+8.3f^\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_AngleP21]))/(float)1000.0);
			fprintf(fResult,"  <P01: %+8.3f^\n",(float)(*(LONG*)&(awMeasurements3Sec[AA_AngleP01]))/(float)1000.0);
		}


		{
			//LONG lAngleValue;
			fprintf(fResult,"\n");
			fprintf(fResult,"Углы:\n");

			WriteAngleToFile(fResult,"  < UAUB  ",(LONG*)&(awMeasurements3Sec[AA_angleUAUB]));
			WriteAngleToFile(fResult,"  < UBUC  ",(LONG*)&(awMeasurements3Sec[AA_angleUBUC]));
			WriteAngleToFile(fResult,"  < UCUA  ",(LONG*)&(awMeasurements3Sec[AA_angleUCUA]));
			WriteAngleToFile(fResult,"  < UAIA  ",(LONG*)&(awMeasurements3Sec[AA_angleUAIA]));
			WriteAngleToFile(fResult,"  < UBIB  ",(LONG*)&(awMeasurements3Sec[AA_angleUBIB]));
			WriteAngleToFile(fResult,"  < UCIC  ",(LONG*)&(awMeasurements3Sec[AA_angleUCIC]));
			fprintf(fResult,"\n");
			WriteAngleToFile(fResult,"  < 3w2iUabc",(LONG*)&(awMeasurements3Sec[AA_angle3w2iUABUCB]));
			WriteAngleToFile(fResult,"  < 3w2iUabIa",(LONG*)&(awMeasurements3Sec[AA_angle3w2iUABIA]));
			WriteAngleToFile(fResult,"  < 3w2iUcbCIc",(LONG*)&(awMeasurements3Sec[AA_angle3w2iUCBIC]));
			fprintf(fResult,"\n");
			WriteAngleToFile(fResult,"  < UBCIA",(LONG*)&(awMeasurements3Sec[AA_angleUBCIA]));
			WriteAngleToFile(fResult,"  < UBCIB",(LONG*)&(awMeasurements3Sec[AA_angleUBCIB]));
			WriteAngleToFile(fResult,"  < UBCIC",(LONG*)&(awMeasurements3Sec[AA_angleUBCIC]));
			WriteAngleToFile(fResult,"  < UBCIN",(LONG*)&(awMeasurements3Sec[AA_angleUBCIN]));
			WriteAngleToFile(fResult,"  < 3w3iUABIA",(LONG*)&(awMeasurements3Sec[AA_angle3w3iUABIA]));
			WriteAngleToFile(fResult,"  < 3w3iUBCIB",(LONG*)&(awMeasurements3Sec[AA_angle3w3iUBCIB]));
			WriteAngleToFile(fResult,"  < 3w3iUCAIC",(LONG*)&(awMeasurements3Sec[AA_angle3w3iUCAIC]));
			fprintf(fResult,"\n");
			WriteAngleToFile(fResult,"  < UABUBC",(LONG*)&(awMeasurements3Sec[AA_angleUABUBC]));
			WriteAngleToFile(fResult,"  < UBCUCA",(LONG*)&(awMeasurements3Sec[AA_angleUBCUCA]));
			WriteAngleToFile(fResult,"  < UCAUAB",(LONG*)&(awMeasurements3Sec[AA_angleUCAUAB]));
		}

		{
			WORD w=0;
			fprintf(fResult,"\n");
			fprintf(fResult,"Гармонические подгруппы:\n");
			fprintf(fResult,"[абсолютные значения]\n");

			
			{
				w=0;
				fprintf(fResult,"  [T:]  %10.6fВ %10.6fВ %10.6fВ    %10.6fВ %10.6fВ %10.6fВ    %10.6fА %10.6fА %10.6fА %10.6fА\n",
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UA+2*w]))/(float)1000000.0*fVoltageMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UB+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UC+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UAB+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UBC+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UCA+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IA+2*w]))/(float)1000000.0*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IB+2*w]))/(float)1000000.0*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IC+2*w]))/(float)1000000.0*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IN+2*w]))/(float)1000000.0*fCurrentMultiplier
					); 
			}

			for (w=1;w<51;w++)
			{
				fprintf(fResult," [%2d:] %10.6fВ %10.6fВ %10.6fВ    %10.6fВ %10.6fВ %10.6fВ    %10.6fА %10.6fА %10.6fА %10.6fА\n",
					w,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UA+2*w]))/(float)1000000.0*fVoltageMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UB+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UC+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UAB+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UBC+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UCA+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IA+2*w]))/(float)1000000.0*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IB+2*w]))/(float)1000000.0*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IC+2*w]))/(float)1000000.0*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IN+2*w]))/(float)1000000.0*fCurrentMultiplier
					); 
			}

			fprintf(fResult,"[коэффициенты]\n");

			w=1;
			fprintf(fResult," [ T:] %10.6f%% %10.6f%% %10.6f%%    %10.6f%% %10.6f%% %10.6f%%    %10.6f%% %10.6f%% %10.6f%% %10.6f%%\n",
				(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UA+102+2*w-2]))/(float)1342177.28,
				(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UB+102+2*w-2]))/(float)1342177.28,					
				(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UC+102+2*w-2]))/(float)1342177.28,					
				(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UAB+102+2*w-2]))/(float)1342177.28,					
				(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UBC+102+2*w-2]))/(float)1342177.28,					
				(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UCA+102+2*w-2]))/(float)1342177.28,					
				(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IA+102+2*w-2]))/(float)1342177.28,
				(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IB+102+2*w-2]))/(float)1342177.28,
				(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IC+102+2*w-2]))/(float)1342177.28,
				(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IN+102+2*w-2]))/(float)1342177.28
				); 

			for (w=2;w<51;w++)
			{
				fprintf(fResult," [%2d:] %10.6f%% %10.6f%% %10.6f%%    %10.6f%% %10.6f%% %10.6f%%    %10.6f%% %10.6f%% %10.6f%% %10.6f%%\n",
					w,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UA+102+2*w-2]))/(float)1342177.28,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UB+102+2*w-2]))/(float)1342177.28,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UC+102+2*w-2]))/(float)1342177.28,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UAB+102+2*w-2]))/(float)1342177.28,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UBC+102+2*w-2]))/(float)1342177.28,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_UCA+102+2*w-2]))/(float)1342177.28,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IA+102+2*w-2]))/(float)1342177.28,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IB+102+2*w-2]))/(float)1342177.28,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IC+102+2*w-2]))/(float)1342177.28,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HSG_IN+102+2*w-2]))/(float)1342177.28
					); 
			}

		}

		{
			WORD w;
			fprintf(fResult,"\n");
			fprintf(fResult,"Субгармонические и интергармонические группы:\n");
			fprintf(fResult,"[абсолютные значения]\n");

			w=0;
			fprintf(fResult," [SUB] %10.6fВ %10.6fВ %10.6fВ    %10.6fВ %10.6fВ %10.6fВ    %10.6fА %10.6fА %10.6fА %10.6fА\n",
				(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UA+2*w]))/(float)1000000.0*fVoltageMultiplier,
				(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UB+2*w]))/(float)1000000.0*fVoltageMultiplier,					
				(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UC+2*w]))/(float)1000000.0*fVoltageMultiplier,					
				(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UAB+2*w]))/(float)1000000.0*fVoltageMultiplier,					
				(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UBC+2*w]))/(float)1000000.0*fVoltageMultiplier,					
				(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UCA+2*w]))/(float)1000000.0*fVoltageMultiplier,					
				(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_IA+2*w]))/(float)1000000.0*fCurrentMultiplier,
				(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_IB+2*w]))/(float)1000000.0*fCurrentMultiplier,
				(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_IC+2*w]))/(float)1000000.0*fCurrentMultiplier,
				(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_IN+2*w]))/(float)1000000.0*fCurrentMultiplier
				); 

			for (w=1;w<51;w++)
			{
				fprintf(fResult," [%2d:] %10.6fВ %10.6fВ %10.6fВ    %10.6fВ %10.6fВ %10.6fВ    %10.6fА %10.6fА %10.6fА %10.6fА\n",
					w,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UA+2*w]))/(float)1000000.0*fVoltageMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UB+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UC+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UAB+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UBC+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_UCA+2*w]))/(float)1000000.0*fVoltageMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_IA+2*w]))/(float)1000000.0*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_IB+2*w]))/(float)1000000.0*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_IC+2*w]))/(float)1000000.0*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_IHG_IN+2*w]))/(float)1000000.0*fCurrentMultiplier
					); 
			}
		}

		{
			fprintf(fResult,"\n");
			fprintf(fResult,"Мощности гармоник:\n");
			WORD w;

			for(w=1;w<=50;w++)
			{
				fprintf(fResult," [%2d:] %+10.3f %+10.3f      %+10.3f %+10.3f      %+10.3f %+10.3f      %+10.3f      %+10.3f      %+10.3f %+10.3f\n",
					w,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HP_PA+2*w-2]))/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HP_QA+2*w-2]))/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HP_PB+2*w-2]))/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HP_QB+2*w-2]))/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HP_PC+2*w-2]))/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HP_QC+2*w-2]))/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier,					
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HP_P1+2*w-2]))/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HP_P2+2*w-2]))/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HP_PSUM+2*w-2]))/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier,
					(float)(*(LONG*)&(awMeasurements3Sec[AA_HP_QSUM+2*w-2]))/(float)1000000.0*fVoltageMultiplier*fCurrentMultiplier
					); 

			}
		}

		{
			fprintf(fResult,"\n");
			fprintf(fResult,"Комплексные произведения:\n");
			fprintf(fResult,"\n");

			fprintf(fResult," AA_CmplxProductUAIA = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAIA ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAIA+2 ])));
			fprintf(fResult," AA_CmplxProductUBIB = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBIB ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBIB+2 ])));
			fprintf(fResult," AA_CmplxProductUCIC = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCIC ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCIC+2 ])));
			fprintf(fResult," AA_CmplxProductUAIB = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAIB ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAIB+2 ])));
			fprintf(fResult," AA_CmplxProductUAIC = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAIC ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAIC+2 ])));
			fprintf(fResult," AA_CmplxProductUBIA = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBIA ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBIA+2 ])));
			fprintf(fResult," AA_CmplxProductUBIC = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBIC ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBIC+2 ])));
			fprintf(fResult," AA_CmplxProductUCIA = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCIA ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCIA+2 ])));
			fprintf(fResult," AA_CmplxProductUCIB = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCIB ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCIB+2 ])));
			fprintf(fResult," AA_CmplxProductUAUB = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAUB ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAUB+2 ])));
			fprintf(fResult," AA_CmplxProductUBUC = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBUC ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBUC+2 ])));
			fprintf(fResult," AA_CmplxProductUCUA = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCUA ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCUA+2 ])));
			fprintf(fResult," AA_CmplxProductIAIB = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductIAIB ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductIAIB+2 ])));
			fprintf(fResult," AA_CmplxProductIBIC = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductIBIC ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductIBIC+2 ])));
			fprintf(fResult," AA_CmplxProductICIA = %f + %fi\n",
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductICIA ])),
				DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductICIA+2 ])));

			fprintf(fResult,"\n");
			fprintf(fResult," AA_CmplxProductUAUA = %f\n",DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAUA ])));
			fprintf(fResult," AA_CmplxProductUBUB = %f\n",DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBUB ])));
			fprintf(fResult," AA_CmplxProductUCUC = %f\n",DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCUC ])));
			fprintf(fResult," AA_CmplxProductUABUAB = %f\n",DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUABUAB ])));
			fprintf(fResult," AA_CmplxProductUBCUBC = %f\n",DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBCUBC ])));
			fprintf(fResult," AA_CmplxProductUCAUCA = %f\n",DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCAUCA ])));
			fprintf(fResult," AA_CmplxProductIAIA = %f\n",DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductIAIA ])));
			fprintf(fResult," AA_CmplxProductIBIB = %f\n",DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductIBIB ])));
			fprintf(fResult," AA_CmplxProductICIC = %f\n",DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductICIC ])));
			fprintf(fResult," AA_CmplxProductININ = %f\n",DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductININ ])));

			fprintf(fResult,"\n");
			fprintf(fResult,"Углы из комплексных произведений:\n");
			fprintf(fResult,"\n");

			WriteAngleFromCmplxProductToFile(fResult,"UAIA",awMeasurements3Sec,AA_CmplxProductUAIA);
			WriteAngleFromCmplxProductToFile(fResult,"UBIB",awMeasurements3Sec,AA_CmplxProductUBIB);
			WriteAngleFromCmplxProductToFile(fResult,"UCIC",awMeasurements3Sec,AA_CmplxProductUCIC);
			WriteAngleFromCmplxProductToFile(fResult,"UAIB",awMeasurements3Sec,AA_CmplxProductUAIB);
			WriteAngleFromCmplxProductToFile(fResult,"UAIC",awMeasurements3Sec,AA_CmplxProductUAIC);
			WriteAngleFromCmplxProductToFile(fResult,"UBIA",awMeasurements3Sec,AA_CmplxProductUBIA);
			WriteAngleFromCmplxProductToFile(fResult,"UBIC",awMeasurements3Sec,AA_CmplxProductUBIC);
			WriteAngleFromCmplxProductToFile(fResult,"UCIA",awMeasurements3Sec,AA_CmplxProductUCIA);
			WriteAngleFromCmplxProductToFile(fResult,"UCIB",awMeasurements3Sec,AA_CmplxProductUCIB);
			WriteAngleFromCmplxProductToFile(fResult,"UAUB",awMeasurements3Sec,AA_CmplxProductUAUB);
			WriteAngleFromCmplxProductToFile(fResult,"UBUC",awMeasurements3Sec,AA_CmplxProductUBUC);
			WriteAngleFromCmplxProductToFile(fResult,"UCUA",awMeasurements3Sec,AA_CmplxProductUCUA);
			WriteAngleFromCmplxProductToFile(fResult,"IAIB",awMeasurements3Sec,AA_CmplxProductIAIB);
			WriteAngleFromCmplxProductToFile(fResult,"IBIC",awMeasurements3Sec,AA_CmplxProductIBIC);
			WriteAngleFromCmplxProductToFile(fResult,"ICIA",awMeasurements3Sec,AA_CmplxProductICIA);

			{
				dUAUA = DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAUA ]));
				dUBUB = DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBUB ]));
				dUCUC = DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCUC ]));

				dReUAUB = DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAUB ]));
				dReUBUC = DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBUC ]));
				dReUCUA = DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCUA ]));
				dImUAUB = DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUAUB+2 ]));
				dImUBUC = DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUBUC+2 ]));
				dImUCUA = DspFloat(*(DWORD*)&(awMeasurements3Sec[ AA_CmplxProductUCUA+2 ]));

				dAngleUABUBC = GetAngleDegrees(
					dReUAUB-dUBUB-dReUCUA+dReUBUC,
					dImUAUB+dImUCUA+dImUBUC);
				dAngleUBCUCA = GetAngleDegrees(
					dReUBUC-dUCUC-dReUAUB+dReUCUA,
					dImUBUC+dImUAUB+dImUCUA);
				dAngleUCAUAB = GetAngleDegrees(
					dReUCUA-dUAUA-dReUBUC+dReUAUB,
					dImUCUA+dImUBUC+dImUCUA);

				fprintf(fResult,"\n");
				fprintf(fResult," угол AB BC = %f\n",dAngleUABUBC);
				fprintf(fResult," угол BC CA = %f\n",dAngleUBCUCA);
				fprintf(fResult," угол CA AB = %f\n",dAngleUCAUAB);
			}
		}

		//=============================
		{
			fprintf(fResult,"\n\n\n\n");
			fprintf(fResult,"memory dump:\n");
			WORD wLine;
			WORD wCoulmn;

			for(wLine=0;wLine<1024;wLine++)
			{
				fprintf(fResult,"%04X: ",wLine*16);
				for(wCoulmn=0;wCoulmn<16;wCoulmn++)
				{
					fprintf(fResult,"0x%04X ",awMeasurements3Sec[16*wLine+wCoulmn]);
				}
				fprintf(fResult,"\n");
			}
			fprintf(fResult,"\n\n\n\n");
		}
}
//====================================================================
//====================================================================





