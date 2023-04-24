#ifndef REGISTRATION_H
#define REGISTRATION_H

#include <windows.h>
#include "EmServiceClasses.h"
#include "EmUsb.h"

#define READ_TIMEOUT			10
#define READ_TIMEOUT_AVG		60

#define AVG_PACKET_LENGTH		16384
#define PQP_SEGMENT_COUNT		256
#define PQP_SEGMENT_LENGTH		2054
#define PQP_ARCHIVE_LENGTH		524288		// PQP_SEGMENT_COUNT * PQP_SEGMENT_LENGTH

struct TRegistrationData // 1024 words // VERSION 1
{
	DWORD dwIndex;
	DWORD dwSignature;
	TDateTime xRegistrationStartDateTime;
	TDateTime xRegistrationStopDateTime;
	char sObjectName[16];

	WORD wConnection;
	WORD wVoltageRangeName;
	WORD wCurrentRangeName;
	WORD wFrequencyRangeName;
	WORD boolVoltageTransformerEnable;
	WORD wVoltageTransformerType;
	DWORD dwDeclaredVoltage;
	WORD wCurrentSensorType;
	WORD wCurrentTransformer;
	WORD wCurrentTransformerPrimary;
	WORD boolMainsSynchronization;
	WORD boolDateTimeAutomaticCorrection;
	WORD wSupplySystem;
	WORD wSetsType;
	WORD wRegistrationArchiveIntervals;
	WORD wRegistrationMode;

	WORD boolRegistrationDone;
	WORD wInterruptionsCounter;

	WORD wRegistrationArchiveCounter;
	WORD wRegistrationStopSource;
	WORD wRegistrationVersion;
	DWORD dwFirmwareVersion;

	LONG lSetsFrequencyDeviationSynchronizedDown95;
	LONG lSetsFrequencyDeviationSynchronizedDown100;
	LONG lSetsFrequencyDeviationSynchronizedUp95;
	LONG lSetsFrequencyDeviationSynchronizedUp100;
	LONG lSetsFrequencyDeviationIsolatedDown95;
	LONG lSetsFrequencyDeviationIsolatedDown100;
	LONG lSetsFrequencyDeviationIsolatedUp95;
	LONG lSetsFrequencyDeviationIsolatedUp100;
	LONG lSetsVoltageDeviationPositive95;
	LONG lSetsVoltageDeviationPositive100;
	LONG lSetsVoltageDeviationNegative95;
	LONG lSetsVoltageDeviationNegative100;
	LONG lSetsFlickerShortTerm95;
	LONG lSetsFlickerShortTerm100;
	LONG lSetsFlickerLongTerm95;
	LONG lSetsFlickerLongTerm100;
	LONG alSetsKHarm95[39];
	LONG alSetsKHarm100[39];
	LONG lSetsKHarmTotal95;
	LONG lSetsKHarmTotal100;
	LONG lSetsK2U95;
	LONG lSetsK2U100;
	LONG lSetsK0U95;
	LONG lSetsK0U100;
	LONG alSetsReserved[102];

	DWORD dwRegistrationStartDateTimeSeconds;
	DWORD dwRegistrationStopDateTimeSeconds;
	DWORD adwRegistrationArchiveIndices[64];

	DWORD dwAverageArchiveIndex3SecHead;
	DWORD dwAverageArchive3SecCounter;
	DWORD dwAverageArchiveIndex10MinHead;
	DWORD dwAverageArchive10MinCounter;
	DWORD dwAverageArchiveIndex2HourHead;
	DWORD dwAverageArchive2HourCounter;

	double dCoordinateLatitude;
	double dCoordinateLongitude;
	WORD boolCoordinateValid;
	WORD wTimeCorrectionsCounter;
	DWORD dwGpsSecondsFullPrecision;
	DWORD dwGpsSecondsTotal;

	WORD wMemoryDistribution_3Sec;
	WORD wMemoryDistribution_10Min;
	WORD wMemoryDistribution_2Hour;
	
	WORD wFlaggedData;

	WORD wReserved630[394];
}; 

//=====================================================================
//=====================================================================

// info about which archives were selected (within one registration)
class CSelectedArchivesData
{
public:
	DWORD regId;
	std::vector<DWORD> vecPqpIndexes;	// indexes of PQP archives
	std::vector<EAvgType> avgTypes;		// selected AVG types
	bool readDns;						// if we must read DNS
};

class RegistrationManager;

class CRegistration
{
private:
	CUsb* usb_;							// port to connect to the device
	DWORD regId_;						// registration id
	DWORD deviceSerialNumber_;
	std::string objectName_;
	TRegistrationData regData_;
	std::string dataPath_;				// path to store data that have been read

	bool ReadPqpArchive(DWORD archiveIndex, RegistrationManager& regManager);
	bool ReadAvgArchive(EAvgType type, RegistrationManager& regManager);
	bool ReadDnsArchive(TDateTime& dtStart, RegistrationManager& regManager);

public:
	CRegistration(DWORD regId, std::string dataPath, CUsb* usb);

	//bool ReadRegistrationByIndex( 
	//			std::vector<DWORD> pqpIndexes,	//DWORD* pPqpIndex, short pqpCount, 
	//			std::vector<EAvgType> avgTypes,	//EAvgType* pAvgType, short avgCount, 
	//						bool readDns);
	bool ReadRegistrationByIndex(RegistrationManager& regManager, int curRegIndex);
	//bool ReadAllRegistration();
};

//===================================================================
//===================================================================
struct TPqpArchive
{
	DWORD dwArchiveIndex;
	DWORD dwSignature;
	DWORD dwRegistrationIndex;
	TDateTime xArchiveStartDateTime;
	TDateTime xArchiveStopDateTimePlanned;
	WORD wArchiveNumber;
	WORD wArchiveVersion;
	DWORD dwArchiveStartDateTimeSeconds;
	DWORD dwArchiveStopDateTimeSecondsPlanned;
	DWORD dwArchiveStopDateTimeSecondsDefacto;
	TDateTime xArchiveStopDateTimeDefacto;
	WORD awReserved56[8];

	//------f-----------------------------------------
	WORD wFrequencyDeviationCounterTotal;
	WORD wFrequencyDeviationCounterLocked;
	WORD wFrequencyDeviationCounterLockedT1;
	WORD wFrequencyDeviationCounterLockedT2;
	WORD wFrequencyDeviationCounterNonlocked;
	WORD boolFrequencyDeviationStatistics;
	LONG lFrequencyDeviationDown95;
	LONG lFrequencyDeviationDown100;
	LONG lFrequencyDeviationUp95;
	LONG lFrequencyDeviationUp100;
	DWORD adwFrequencyDeviationStartDateTimeSeconds[60480];
	LONG alFrequencyDeviation[60480];

	//------cnt-----------------------------------------
	WORD w10MinuteCounterTotal;
	WORD w10MinuteCounterNonflagged;
	WORD w10MinuteCounterFlagged;
	WORD bool10MinuteStatistics;
	DWORD adw10MinuteStartDateTimeSeconds[1008];
	BYTE ab10MinuteFlaggedOrNonflagged[1008];

	//------u-----------------------------------------
	WORD wUAABDeviationPositiveCounterNonflaggedT1;
	WORD wUAABDeviationPositiveCounterNonflaggedT2;
	LONG lUAABDeviationPositive95;
	LONG lUAABDeviationPositive100;

	WORD wUBBCDeviationPositiveCounterNonflaggedT1;
	WORD wUBBCDeviationPositiveCounterNonflaggedT2;
	LONG lUBBCDeviationPositive95;
	LONG lUBBCDeviationPositive100;

	WORD wUCCADeviationPositiveCounterNonflaggedT1;
	WORD wUCCADeviationPositiveCounterNonflaggedT2;
	LONG lUCCADeviationPositive95;
	LONG lUCCADeviationPositive100;

	WORD wUAABDeviationNegativeCounterNonflaggedT1;
	WORD wUAABDeviationNegativeCounterNonflaggedT2;
	LONG lUAABDeviationNegative95;
	LONG lUAABDeviationNegative100;

	WORD wUBBCDeviationNegativeCounterNonflaggedT1;
	WORD wUBBCDeviationNegativeCounterNonflaggedT2;
	LONG lUBBCDeviationNegative95;
	LONG lUBBCDeviationNegative100;

	WORD wUCCADeviationNegativeCounterNonflaggedT1;
	WORD wUCCADeviationNegativeCounterNonflaggedT2;
	LONG lUCCADeviationNegative95;
	LONG lUCCADeviationNegative100;

	LONG alUAABDeviationPositive[1008];
	LONG alUBBCDeviationPositive[1008];
	LONG alUCCADeviationPositive[1008];
	LONG alUAABDeviationNegative[1008];
	LONG alUBBCDeviationNegative [1008];
	LONG alUCCADeviationNegative [1008];

	//-----u harm-----------------------------------------
	WORD awUAABKHarmCounterNonflaggedT1[40];
	WORD awUAABKHarmCounterNonflaggedT2[40];
	LONG alUAABKHarm95[40];
	LONG alUAABKHarm100[40];
	WORD awUBBCKHarmCounterNonflaggedT1[40];
	WORD awUBBCKHarmCounterNonflaggedT2[40];
	LONG alUBBCKHarm95[40];
	LONG alUBBCKHarm100[40];
	WORD awUCCAKHarmCounterNonflaggedT1[40];
	WORD awUCCAKHarmCounterNonflaggedT2[40];
	LONG alUCCAKHarm95[40];
	LONG alUCCAKHarm100[40];

	//-----k2-----------------------------------------
	WORD wK2UCounterNonflaggedT1;
	WORD wK2UcounterNonflaggedT2;
	LONG lK2U95;
	LONG lK2U100;
	WORD wK0UCounterNonflaggedT1;
	WORD wK0UcounterNonflaggedT2;
	LONG lK0U95;
	LONG lK0U100;

	//-------flicker-----------------------------------
	WORD wUAABFlickerPstCounterNonflaggedT1;
	WORD wUAABFlickerPstCounterNonflaggedT2;
	WORD wUAABFlickerPltCounterNonflaggedT1;
	WORD wUAABFlickerPltCounterNonflaggedT2;
	SHORT iUAABFlickerPst95;
	SHORT iUAABFlickerPst100;
	SHORT iUAABFlickerPlt95;
	SHORT iUAABFlickerPlt100;
	WORD wUBBCFlickerPstCounterNonflaggedT1;
	WORD wUBBCFlickerPstCounterNonflaggedT2;
	WORD wUBBCFlickerPltCounterNonflaggedT1;
	WORD wUBBCFlickerPltCounterNonflaggedT2;
	SHORT iUBBCFlickerPst95;
	SHORT iUBBCFlickerPst100;
	SHORT iUBBCFlickerPlt95;
	SHORT iUBBCFlickerPlt100;
	WORD wUCCAFlickerPstCounterNonflaggedT1;
	WORD wUCCAFlickerPstCounterNonflaggedT2;
	WORD wUCCAFlickerPltCounterNonflaggedT1;
	WORD wUCCAFlickerPltCounterNonflaggedT2;
	SHORT iUCCAFlickerPst95;
	SHORT iUCCAFlickerPst100;
	SHORT iUCCAFlickerPlt95;
	SHORT iUCCAFlickerPlt100;
	SHORT aiUAABFlickerPst[1008];
	SHORT aiUBBCFlickerPst[1008];
	SHORT aiUCCAFlickerPst[1008];
	SHORT aiUAABFlickerPlt[84];
	SHORT aiUBBCFlickerPlt[84];
	SHORT aiUCCAFlickerPlt[84];
	DWORD adwFlickerPltDateTimeSeconds[84];
	BYTE abFlickerPltValid[84];
	WORD wFlickerPltCounterTotal;
	WORD wFlickerPltCounterNonflagged;
	WORD boolFlickerPltStatistics;
	WORD wReservedX;	

	//-----dns-----------------------------------------
	WORD awSwells_110_112_Counter[8];
	WORD awSwells_112_115_Counter[8];
	WORD awSwells_115_120_Counter[8];
	WORD awSwells_120_150_Counter[8];
	WORD awDips_90_85_Counter[8];
	WORD awDips_85_70_Counter[8];
	WORD awDips_70_40_Counter[8];
	WORD awDips_40_10_Counter[8];
	WORD awDips_10_5_Counter[8];
	WORD awInterruptions_5_0_Counter[8];

	//--------angle-----------------------------------------
	LONG alUAABIsg100[41];
	LONG alUBBCIsg100[41];
	LONG alUCCAIsg100[41];

	//---------dns----------------------------------------	
	WORD wSwells_110_112_Long_Counter;
	WORD wSwells_112_115_Long_Counter;
	WORD wSwells_115_120_Long_Counter;
	WORD wSwells_120_150_Long_Counter;
	WORD wDips_90_85_Long_Counter;
	WORD wDips_85_70_Long_Counter;
	WORD wDips_70_40_Long_Counter;
	WORD wDips_40_10_Long_Counter;
	WORD wDips_10_5_Long_Counter;
	WORD wInterruptions_5_0_Long_Counter;

	//------dns-----------------------------------------
	WORD awSwell33073Counters_110_120[6];
	WORD awSwell33073Counters_120_140[6];
	WORD awSwell33073Counters_140_160[6];
	WORD awSwell33073Counters_160_180[6];
	WORD awDip33073Counters_90_85[6];
	WORD awDip33073Counters_85_70[6];
	WORD awDip33073Counters_70_40[6];
	WORD awDip33073Counters_40_10[6];
	WORD awDip33073Counters_10_0[6];
	WORD awInterruption33073Counters_5_0[7];
	WORD awReserved261297;
	DWORD dwInterruptionMaxDuration;
	
	//------reserved-------------------------------------
	WORD awReserved261300[844];
};

//===================================================================
//===================================================================

struct TAverageArchiveDescription
{
	DWORD dwAverageArchiveIndex;	//4bytes
	TDateTime xStartDateTime;		//28bytes
	TDateTime xStopDateTime;		//28bytes
};		//60bytes

#define MAX_NUMBER_OF_DESCRIPTIONS_IN_PACKET  (128)

struct TAverageArchiveIndexByDateTime
{
	DWORD dwIndex;					//4bytes
	TDateTime xDateTime;			//28bytes
};		//32bytes


//=====================================================================================

#define AVERAGE_SIGNATURE_3SEC			(0xA5AAAAA5)
#define AVERAGE_SIGNATURE_10MIN			(0xA5AAA5A5)
#define AVERAGE_SIGNATURE_2HOUR			(0xA5AA5AA5)
//--------------------------average-archive-format------------
#define AA_AverageArchiveIndex		0
#define AA_Signature		2
#define AA_RegistrationIndex		4

#define AA_StartDateTime		6
#define AA_StopDateTime			20

#define AA_ActiveConnection		34
#define AA_MainsSynchronization	35
#define AA_VoltageRange			36
#define AA_CurrentSensorType	37
#define AA_CurrentRange			38
#define AA_FrequencyRange		39

#define AA_VoltageTransformerEnable	40
#define AA_VoltageTransformerType	41
#define	AA_DeclaredVoltage			42

#define AA_CurrentTransformerUsage		44
#define AA_CurrentTransformerPrimary	45
#define AA_Flagged						46
#define AA_DateTimeAutomaticCorrection	47
#define AA_ObjectName					48

#define AA_StartDateTimeSeconds			98
#define AA_StopDateTimeSeconds			100

#define AA_ArchiveDurationMilliseconds	102
#define	AA_NOSamplesInArchive			104
#define	AA_NOSamples1stWindow			106
#define AA_NOWindowsLockedA				121		
#define AA_NOWindowsLockedB				122	
#define AA_NOWindowsLockedC				123	
#define AA_NOWindowsLockedAB			124	
#define AA_NOWindowsLockedBC			125	
#define AA_NOWindowsLockedCA			126	
#define AA_NOWindowsNonlocked			127	
//--------------------------------average-archive-format------------
#define	AA_MainsFrequencyA				128
#define	AA_MainsFrequencyB				130
#define	AA_MainsFrequencyC				132
#define	AA_MainsFrequencyAB				134
#define	AA_MainsFrequencyBC				136
#define	AA_MainsFrequencyCA				138

#define	AA_UA_RMS			 	140
#define	AA_UB_RMS			 	142
#define	AA_UC_RMS			 	144
#define	AA_UAB_RMS			 	146
#define	AA_UBC_RMS			 	148
#define	AA_UCA_RMS			 	150
#define	AA_IA_RMS			 	152
#define	AA_IB_RMS			 	154
#define	AA_IC_RMS			 	156
#define	AA_IN_RMS			 	158

#define	AA_UA_0			 	160
#define	AA_UB_0			 	162
#define	AA_UC_0			 	164
#define	AA_UAB_0		 	166
#define	AA_UBC_0		 	168
#define	AA_UCA_0		 	170
#define	AA_IA_0			 	172
#define	AA_IB_0			 	174
#define	AA_IC_0			 	176
#define	AA_IN_0			 	178
		
#define	AA_UA_R			 	180
#define	AA_UB_R			 	182
#define	AA_UC_R			 	184
#define	AA_UAB_R		 	186
#define	AA_UBC_R		 	188
#define	AA_UCA_R		 	190
#define	AA_IA_R			 	192
#define	AA_IB_R			 	194
#define	AA_IC_R			 	196
#define	AA_IN_R			 	198

#define	AA_UA_1			 	200
#define	AA_UB_1			 	202
#define	AA_UC_1			 	204
#define	AA_UAB_1		 	206
#define	AA_UBC_1		 	208
#define	AA_UCA_1		 	210
#define	AA_IA_1			 	212
#define	AA_IB_1			 	214
#define	AA_IC_1			 	216
#define	AA_IN_1			 	218

#define	AA_PA			 	220
#define	AA_PB			 	222
#define	AA_PC			 	224
#define	AA_PABC			 	226
#define	AA_P1			 	228
#define	AA_P2			 	230
#define	AA_P12			 	232

#define	AA_SA			 	234
#define	AA_SB			 	236
#define	AA_SC			 	238
#define	AA_SABC			 	240
#define	AA_S1			 	242
#define	AA_S2			 	244
#define	AA_S12			 	246

#define	AA_QA			 	248
#define	AA_QB			 	250
#define	AA_QC			 	252
#define	AA_QABC			 	254
#define	AA_Q1			 	256
#define	AA_Q2			 	258
#define	AA_Q12			 	260

#define	AA_KpA				262
#define	AA_KpB				264
#define	AA_KpC				266
#define	AA_KpABC			268
#define	AA_Kp12				270

#define	AA_U11				272
#define	AA_U21				274
#define	AA_U01				276
#define	AA_K21				278
#define	AA_K01				280
#define	AA_I11				282
#define	AA_I21				284
#define	AA_I01				286
#define	AA_P11				288
#define	AA_P21				290
#define	AA_P01				292
#define	AA_AngleP11			294
#define	AA_AngleP21			296
#define	AA_AngleP01			298

#define	AA_rdU11		  300
#define	AA_rdUA1		  302
#define	AA_rdUB1		  304
#define	AA_rdUC1		  306
#define	AA_rdUAB1		  308
#define	AA_rdUBC1		  310
#define	AA_rdUCA1		  312
#define	AA_dUAover		  314
#define	AA_dUBover		  316
#define	AA_dUCover		  318
#define	AA_dUABover		  320
#define	AA_dUBCover		  322
#define	AA_dUCAover		  324
#define	AA_dUAunder		  326
#define	AA_dUBunder		  328
#define	AA_dUCunder		  330
#define	AA_dUABunder	  332
#define	AA_dUBCunder	  334
#define	AA_dUCAunder	  336
#define	AA_rdUAover		  338
#define	AA_rdUBover		  340
#define	AA_rdUCover		  342
#define	AA_rdUABover	  344
#define	AA_rdUBCover	  346
#define	AA_rdUCAover	  348
#define	AA_rdUAunder	  350
#define	AA_rdUBunder	  352
#define	AA_rdUCunder	  354
#define	AA_rdUABunder	  356
#define	AA_rdUBCunder	  358
#define	AA_rdUCAunder	  360
	
#define	AA_angleUAUB	  362
#define	AA_angleUBUC	  364
#define	AA_angleUCUA	  366
#define	AA_angleUAIA	  368
#define	AA_angleUBIB	  370
#define	AA_angleUCIC	  372
#define	AA_angle3w2iUabc  374
#define	AA_angle3w2iUabIa	  376
#define	AA_angle3w2iUcbIc	  378
		
#define		AA_CmplxProductUAIA		380
#define		AA_CmplxProductUBIB		384
#define		AA_CmplxProductUCIC		388
#define		AA_CmplxProductUAIB		392
#define		AA_CmplxProductUAIC		396
#define		AA_CmplxProductUBIA		400
#define		AA_CmplxProductUBIC		404
#define		AA_CmplxProductUCIA		408
#define		AA_CmplxProductUCIB		412
#define		AA_CmplxProductUAUB		416
#define		AA_CmplxProductUBUC		420
#define		AA_CmplxProductUCUA		424
#define		AA_CmplxProductIAIB		428
#define		AA_CmplxProductIBIC		432
#define		AA_CmplxProductICIA		436
//---------------------------------average-archive-format------
#define		AA_HSG_UA	440
#define		AA_HSG_UB	642
#define		AA_HSG_UC	844
#define		AA_HSG_UAB	1046
#define		AA_HSG_UBC	1248
#define		AA_HSG_UCA	1450
#define		AA_HSG_IA	1652
#define		AA_HSG_IB	1854
#define		AA_HSG_IC	2056	
#define		AA_HSG_IN	2258
//----------------------------------average-archive-format------
#define		AA_IHG_UA	   2460
#define		AA_IHG_UB	   2562
#define		AA_IHG_UC	   2664
#define		AA_IHG_UAB	   2766
#define		AA_IHG_UBC	   2868
#define		AA_IHG_UCA	   2970
#define		AA_IHG_IA	   3072
#define		AA_IHG_IB	   3174
#define		AA_IHG_IC	   3276
#define		AA_IHG_IN	   3378
//-----------------------------------average-archive-format------
#define		AA_HP_PA		  3480
#define		AA_HP_QA		  3580
#define		AA_HP_angleA	  3680	
#define		AA_HP_PB		  3780
#define		AA_HP_QB		  3880
#define		AA_HP_angleB	  3980	
#define		AA_HP_PC		  4080
#define		AA_HP_QC		  4180		
#define		AA_HP_angleC	  4280	
#define		AA_HP_P1		  4380
#define		AA_HP_Q1		  4480
#define		AA_HP_angle1	  4580	
#define		AA_HP_P2		  4680
#define		AA_HP_Q2		  4780
#define		AA_HP_angle2	  4880	
#define		AA_HP_PSUM		  4980
#define		AA_HP_QSUM		  5080
#define		AA_HP_angleSUM	  5180	
//----------------------------------average-archive-format-----
#define		AA_CmplxProductUAUA			5280
#define		AA_CmplxProductUBUB			5282
#define		AA_CmplxProductUCUC			5284
#define		AA_CmplxProductUABUAB		5286
#define		AA_CmplxProductUBCUBC		5288
#define		AA_CmplxProductUCAUCA		5290
#define		AA_CmplxProductIAIA			5292
#define		AA_CmplxProductIBIB			5294
#define		AA_CmplxProductICIC			5296
#define		AA_CmplxProductININ			5298

#define		AA_angleUABUBC				5300
#define		AA_angleUBCUCA				5302	
#define		AA_angleUCAUAB				5304	
#define		AA_angleUBCIA				5306
#define		AA_angleUBCIB				5308
#define		AA_angleUBCIC				5310
#define		AA_angleUBCIN				5312
#define		AA_angle3w3iUABIA			5314
#define		AA_angle3w3iUBCIB			5316
#define		AA_angle3w3iUCAIC			5318

#define		AA_Q11	5320										
#define		AA_Q21	5322										
#define		AA_Q01	5324	

#define DSI_SIGNATURE			(0xAA555A5A)


struct TDsiArchiveEntry		// 128 bytes
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
};

#endif