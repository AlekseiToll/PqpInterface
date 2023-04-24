#define _CRT_SECURE_NO_WARNINGS

#include "stdafx.h"
#include <windows.h>
#include <stdio.h>

//#include "MDI_DeviceDiagnostics.h"
#include "ReadDSI.h"
#include "EmUsb.h"

HANDLE hThread_DeviceDiagnostics_ReadDSI;

void lrWriteEventData(BYTE *pbStatus0,TDsiArchiveEntry *pxEntry0,FILE *fResult);
void lrWriteArchiveEventStatistics(DWORD *pdwMinID,DWORD *pdwMaxID,WORD *pwCounter,FILE *fResult);
void lrWriteEventFullData(TDsiArchiveEntry *pxEntry,WORD wCounter,FILE *fResult);

//==================================================
//==================================================
DWORD WINAPI ThreadFunc_DeviceDiagnostics_ReadDSI(LPVOID lpv)
{
	TUsbResult UsbResult;
	WORD wRequest;
	WORD wReplyCommand;
	WORD wReplyLength;
	WORD wReplyAddress;
	TDSIStatus xDSIStatus;
	FILE *fResult;
	char szResultFileName[MAX_PATH];
	char szResultFilePath[MAX_PATH];
	TDsiArchiveEntry axDsiArchives[64];

	if (UsbConnect()==FALSE)
	{
		MessageBox(NULL,"UsbConnect() FAILED","FATAL ERROR",MB_OK);
		UsbDisconnect();
		return 0;
	}

	UsbResult = UsbCommunication(		
		FALSE,
		COMMAND_ReadDSIStatus,
		0,
		NULL,
		5,
		&wReplyAddress,
		&wReplyCommand,
		&wReplyLength,
		(void*)&xDSIStatus,
		sizeof(TDSIStatus));

	if ((UsbResult==USBRESULT_OK)&&(wReplyLength==sizeof(TDSIStatus)))
	{
		CommunicationResultFileName(wReplyAddress,"ReadDSI.txt",szResultFileName,TRUE);
		GetCurrentDirectory(MAX_PATH,szResultFilePath);
		strcat(szResultFilePath,"\\");
		strcat(szResultFilePath,szResultFileName);
		fResult = fopen(szResultFileName,"w");

		fprintf(fResult,"“≈ ”Ÿ»≈ —Œ¡€“»ﬂ\n");
		fprintf(fResult," œ–Œ¬¿À€ Õ¿œ–ﬂ∆≈Õ»ﬂ\n");
		lrWriteEventData(&xDSIStatus.aboolActualDipStatus[0],&xDSIStatus.axActualDip[0],fResult);
		fprintf(fResult," œ≈–≈Õ¿œ–ﬂ∆≈Õ»ﬂ\n");
		lrWriteEventData(&xDSIStatus.aboolActualSwellStatus[0],&xDSIStatus.axActualSwell[0],fResult);
		fprintf(fResult," œ–≈–€¬¿Õ»ﬂ Õ¿œ–ﬂ∆≈Õ»ﬂ\n");
		lrWriteEventData(&xDSIStatus.aboolActualInterruptionStatus[0],&xDSIStatus.axActualInterruption[0],fResult);
		fprintf(fResult,"\n");

		fprintf(fResult,"¿–’»¬ —Œ¡€“»…\n");
		fprintf(fResult," œ–Œ¬¿À€ Õ¿œ–ﬂ∆≈Õ»ﬂ\n");
		lrWriteArchiveEventStatistics(&xDSIStatus.adwMinDipID[0],&xDSIStatus.adwMaxDipID[0],&xDSIStatus.wDipCounter[0],fResult);
		fprintf(fResult," œ≈–≈Õ¿œ–ﬂ∆≈Õ»ﬂ\n");
		lrWriteArchiveEventStatistics(&xDSIStatus.adwMinSwellID[0],&xDSIStatus.adwMaxSwellID[0],&xDSIStatus.wSwellCounter[0],fResult);
		fprintf(fResult," œ–≈–€¬¿Õ»ﬂ Õ¿œ–ﬂ∆≈Õ»ﬂ\n");
		lrWriteArchiveEventStatistics(&xDSIStatus.adwMinInterruptionID[0],&xDSIStatus.adwMaxInterruptionID[0],&xDSIStatus.wInterruptionCounter[0],fResult);
		fprintf(fResult,"\n");

		fclose(fResult);
	}
	else
	{
		UsbDisconnect();
		return 0;
	}

	fResult = fopen(szResultFileName,"a");
	fprintf(fResult,"«¿œ»—» ¿–’»¬¿\n");

	BYTE bChannel;
	BYTE bType;
	WORD wEntry;
	WORD wCounter;

	for (bType=DSI_TYPE_DIP;bType<=DSI_TYPE_INTERRUPTION;bType++)
	{
		for (bChannel=DSI_CHANNEL_UABCN;bChannel<=DSI_CHANNEL_UCA;bChannel++)
		{
			fprintf(fResult,"\n");
			switch(bType)
			{
				case DSI_TYPE_DIP:
					fprintf(fResult,"œ–Œ¬¿À€ Õ¿œ–ﬂ∆≈Õ»ﬂ - ");
					break;
				case DSI_TYPE_SWELL:
					fprintf(fResult,"œ≈–≈Õ¿œ–ﬂ∆≈Õ»ﬂ - ");
					break;
				case DSI_TYPE_INTERRUPTION:
					fprintf(fResult,"œ–≈–€¬¿Õ»ﬂ Õ¿œ–ﬂ∆≈Õ»ﬂ - ");
					break;
			}
			switch(bChannel)
			{
				case DSI_CHANNEL_UABCN:
					fprintf(fResult,"ABCN");
					break;
				case DSI_CHANNEL_UABC:
					fprintf(fResult,"ABC");
					break;
				case DSI_CHANNEL_UA:
					fprintf(fResult,"A");
					break;
				case DSI_CHANNEL_UB:
					fprintf(fResult,"B");
					break;
				case DSI_CHANNEL_UC:
					fprintf(fResult,"C");
					break;
				case DSI_CHANNEL_UAB:
					fprintf(fResult,"AB");
					break;
				case DSI_CHANNEL_UBC:
					fprintf(fResult,"BC");
					break;
				case DSI_CHANNEL_UCA:
					fprintf(fResult,"CA");
					break;
			}
			fprintf(fResult,"\n");
			wCounter = 0;
			wRequest = (WORD)bType + ((WORD)bChannel<<8);

			UsbResult = UsbCommunication(		
				FALSE,
				COMMAND_ReadDSIArchives,
				2,
				&wRequest,
				5,
				&wReplyAddress,
				&wReplyCommand,
				&wReplyLength,
				(void*)&axDsiArchives,
				sizeof(axDsiArchives));

			if (wReplyLength!=0)
			{
				for(wEntry=0;wEntry<((wReplyLength/128));wEntry++)
				{
					lrWriteEventFullData(&axDsiArchives[wEntry],wCounter,fResult);
					wCounter++;
				}

				while(1)
				{
					UsbResult = UsbCommunication(		
						TRUE,
						COMMAND_ReadDSIArchives,
						2,
						&wRequest,
						5,
						&wReplyAddress,
						&wReplyCommand,
						&wReplyLength,
						(void*)&axDsiArchives,
						sizeof(axDsiArchives));

					for(wEntry=0;wEntry<((wReplyLength/128));wEntry++)
					{
						lrWriteEventFullData(&axDsiArchives[wEntry],wCounter,fResult);
						wCounter++;
					}

					if (wReplyLength==0)
					{
						break;
					}
				}
			}
		}
	}
	
	fclose(fResult);
	UsbDisconnect();
	return 0;
}
//==========================================================
//==========================================================
void lrWriteEventData(BYTE *pbStatus0,TDsiArchiveEntry *pxEntry0,FILE *fResult)
{
	BYTE bChannel;
	for(bChannel=0;bChannel<DSI_CHANNEL_LAST;bChannel++)
	{
		if (pbStatus0[bChannel]==PQPA_TRUE)
		{
			switch(bChannel)
			{
				case DSI_CHANNEL_UABCN:
					fprintf(fResult,"   ABCN: ");
					break;
				case DSI_CHANNEL_UABC:	
					fprintf(fResult,"    ABC: ");
					break;
				case DSI_CHANNEL_UA:
					fprintf(fResult,"      A: ");
					break;
				case DSI_CHANNEL_UB:	
					fprintf(fResult,"      B: ");
					break;
				case DSI_CHANNEL_UC:	
					fprintf(fResult,"      C: ");
					break;
				case DSI_CHANNEL_UAB:	
					fprintf(fResult,"     AB: ");
					break;
				case DSI_CHANNEL_UBC:	
					fprintf(fResult,"     BC: ");
					break;
				case DSI_CHANNEL_UCA:
					fprintf(fResult,"     CA: ");
					break;
			}
			fprintf(fResult," Ì‡˜‡ÎÓ %.2d/%.2d/%.4d %.2d:%.2d:%.2d.%.3d Ì‡ÔˇÊÂÌËÂ %.3fV (%.3f%%)\n",
				pxEntry0[bChannel].xStart.wLocalDate,
				pxEntry0[bChannel].xStart.wLocalMonth,
				pxEntry0[bChannel].xStart.wLocalYear,
				pxEntry0[bChannel].xStart.wLocalHours,
				pxEntry0[bChannel].xStart.wLocalMinutes,
				pxEntry0[bChannel].xStart.wLocalSeconds,
				pxEntry0[bChannel].xStart.wMilliseconds,
				((float)(pxEntry0[bChannel].dwVoltageMicrovolts))/(float)1000000.0,
				((float)(pxEntry0[bChannel].dwVoltageRelative))/(float)1342177.28			
				);

		}
	}
}
//============================================================
//============================================================
void lrWriteArchiveEventStatistics(DWORD *pdwMinID,DWORD *pdwMaxID,WORD *pwCounter,FILE *fResult)
{
	BYTE bChannel;
	for(bChannel=0;bChannel<DSI_CHANNEL_LAST;bChannel++)
	{
		switch(bChannel)
		{
			case DSI_CHANNEL_UABCN:
				fprintf(fResult,"   ABCN: ");
				break;
			case DSI_CHANNEL_UABC:	
				fprintf(fResult,"    ABC: ");
				break;
			case DSI_CHANNEL_UA:
				fprintf(fResult,"      A: ");
				break;
			case DSI_CHANNEL_UB:	
				fprintf(fResult,"      B: ");
				break;
			case DSI_CHANNEL_UC:	
				fprintf(fResult,"      C: ");
				break;
			case DSI_CHANNEL_UAB:	
				fprintf(fResult,"     AB: ");
				break;
			case DSI_CHANNEL_UBC:	
				fprintf(fResult,"     BC: ");
				break;
			case DSI_CHANNEL_UCA:
				fprintf(fResult,"     CA: ");
				break;
		}
		fprintf(fResult," %d ÒÓ·˚ÚËÈ (0x%.8x > 0x%.8x)\n",
			pwCounter[bChannel],
			pdwMinID[bChannel],
			pdwMaxID[bChannel]
			);
	}
}
//====================================================================
//====================================================================
void lrWriteEventFullData(TDsiArchiveEntry *pxEntry,WORD wCounter,FILE *fResult)
{
	fprintf(fResult,"  %0.4d (0x%.8X) (RegIndex=%0.9d) [%.2d/%.2d/%.4d %.2d:%.2d:%.2d.%.3d]>[%.2d/%.2d/%.4d %.2d:%.2d:%.2d.%.3d]=[%0.4dÒÛÚ %.2d:%.2d:%.2d.%.3d] U=%9.3fV (dU=%7.3f%%) (Ud=%9.3fV) (Smpl=%0.9d>%0.9d) (Seconds=%0.9d>%0.9d)\n",
		wCounter,
		pxEntry->dwDsiIndex,

		pxEntry->dwRegistrationIndex,

		pxEntry->xStart.wLocalDate,
		pxEntry->xStart.wLocalMonth,
		pxEntry->xStart.wLocalYear,
		pxEntry->xStart.wLocalHours,
		pxEntry->xStart.wLocalMinutes,
		pxEntry->xStart.wLocalSeconds,
		pxEntry->xStart.wMilliseconds,

		pxEntry->xEnd.wLocalDate,
		pxEntry->xEnd.wLocalMonth,
		pxEntry->xEnd.wLocalYear,
		pxEntry->xEnd.wLocalHours,
		pxEntry->xEnd.wLocalMinutes,
		pxEntry->xEnd.wLocalSeconds,
		pxEntry->xEnd.wMilliseconds,

		pxEntry->wDurationDays,
		pxEntry->wDurationHours,
		pxEntry->wDurationMinutes,
		pxEntry->wDurationSeconds,
		pxEntry->wDurationMilliseconds,

		((float)(pxEntry->dwVoltageMicrovolts))/(float)1000000.0,
		((float)(pxEntry->dwVoltageRelative))/(float)1342177.28,
		((float)(pxEntry->dwDeclaredVoltageMicrovolts))/(float)1000000.0,

		pxEntry->dwStartSample,
		pxEntry->dwEndSample,

		pxEntry->dwStartSeconds,
		pxEntry->dwEndSeconds
		);
}


