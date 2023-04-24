#include <windows.h>
#include "SysData.h"

extern HANDLE hThread_ReadDSI;
extern DWORD WINAPI ThreadFunc_ReadDSI(LPVOID lpv);

enum
{
	DSI_CHANNEL_UABCN=0,	
	DSI_CHANNEL_UABC,	
	DSI_CHANNEL_UA,	
	DSI_CHANNEL_UB,	
	DSI_CHANNEL_UC,	
	DSI_CHANNEL_UAB,	
	DSI_CHANNEL_UBC,	
	DSI_CHANNEL_UCA,
	DSI_CHANNEL_LAST
};

enum
{
	DSI_TYPE_DIP=0,	
	DSI_TYPE_SWELL,	
	DSI_TYPE_INTERRUPTION	
};


#define DSI_SIGNATURE			(0xAA555A5A)
typedef struct
{
	DWORD dwDsiIndex;
	DWORD dwSignature;
	WORD wType;
	WORD wReserved3;
	DWORD dwVoltageMicrovolts;
	DWORD dwVoltageRelative;
	DWORD dwDeclaredVoltageMicrovolts;
	TDateTime xStart;
	TDateTime xEnd;
	DWORD dwDurationTotalSeconds;
	WORD wDurationMilliseconds;
	WORD wDurationDays;
	WORD wDurationHours;
	WORD wDurationMinutes;
	WORD wDurationSeconds;
	WORD wEventFinished;
	DWORD dwStartSample;
	DWORD dwEndSample;
	DWORD dwRegistrationIndex;
	DWORD dwStartSeconds;
	DWORD dwEndSeconds;
	WORD wReserved58;
	WORD wReserved59;
	WORD wReserved60;
	WORD wReserved61;
	WORD wReserved62;
	WORD wCRC;
} TDsiArchiveEntry;

typedef struct
{
	BYTE aboolActualDipStatus[DSI_CHANNEL_LAST];
	BYTE aboolActualSwellStatus[DSI_CHANNEL_LAST];
	BYTE aboolActualInterruptionStatus[DSI_CHANNEL_LAST];

	TDsiArchiveEntry axActualDip[DSI_CHANNEL_LAST];
	TDsiArchiveEntry axActualSwell[DSI_CHANNEL_LAST];
	TDsiArchiveEntry axActualInterruption[DSI_CHANNEL_LAST];

	DWORD adwMinDipID[DSI_CHANNEL_LAST];
	DWORD adwMaxDipID[DSI_CHANNEL_LAST];
	DWORD adwMinSwellID[DSI_CHANNEL_LAST];
	DWORD adwMaxSwellID[DSI_CHANNEL_LAST];
	DWORD adwMinInterruptionID[DSI_CHANNEL_LAST];
	DWORD adwMaxInterruptionID[DSI_CHANNEL_LAST];
	WORD wDipCounter[DSI_CHANNEL_LAST];
	WORD wSwellCounter[DSI_CHANNEL_LAST];
	WORD wInterruptionCounter[DSI_CHANNEL_LAST];

} TDSIStatus;
