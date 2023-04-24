#include <windows.h>
#include "SysData.h"

extern HANDLE hThread_ReadMeasurements3Sec;
extern DWORD WINAPI ThreadFunc_ReadMeasurements3Sec(LPVOID lpv);
extern HANDLE hThread_ReadMeasurements10Min;
extern DWORD WINAPI ThreadFunc_ReadMeasurements10Min(LPVOID lpv);
extern HANDLE hThread_ReadMeasurements2Hour;
extern DWORD WINAPI ThreadFunc_ReadMeasurements2Hour(LPVOID lpv);

extern WORD awMeasurements3Sec[8192];

#define MyTRUE	(0)
#define MyFALSE	(1)

//====================================================================================
//---------------------------------------------------average-archive-format-----------

#define AVERAGE_SIGNATURE_3SEC			(0xA5AAAAA5)
#define AVERAGE_SIGNATURE_10MIN			(0xA5AAA5A5)
#define AVERAGE_SIGNATURE_2HOUR			(0xA5AA5AA5)

//---------------------------------------------------average-archive-format-----------

#define AA_AverageArchiveIndex		0
#define AA_Signature				2
#define AA_Registration				4

#define AA_StartDateTime			6
#define AA_StopDateTime				20

#define AA_ActiveConnection			34
#define AA_MainsSynchronization		35
#define AA_VoltageRange				36
#define AA_CurrentSensorType		37
#define AA_CurrentRange				38
#define AA_FrequencyRange			39

#define AA_VoltageTransformerEnable		40
#define AA_VoltageTransformerType		41
#define	AA_DeclaredVoltage				42

#define AA_CurrentTransformerUsage		44
#define AA_CurrentTransformerPrimary	45
//#define AA_VoltageNominal				46
#define AA_DateTimeAutomaticCorrection	47
#define AA_ObjectName					48

#define AA_NOSamples1stWindowFractional	56

#define AA_10MinBranchMaxPImport		(74)
#define AA_10MinBranchMaxQImport		(76)
#define AA_10MinBranchMaxPExport		(78)
#define AA_10MinBranchMaxQExport		(80)

#define AA_10MinBranchDateTime			82

#define AA_10MinBranch					96
#define AA_15WinIn10MinCounter			97
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

//---------------------------------------------------average-archive-format-----------------
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

#define	AA_U11			272
#define	AA_U21			274
#define	AA_U01			276
#define	AA_K21			278
#define	AA_K01			280
#define	AA_I11			282
#define	AA_I21			284
#define	AA_I01			286
#define	AA_P11			288
#define	AA_P21			290
#define	AA_P01			292
#define	AA_AngleP11		294
#define	AA_AngleP21		296
#define	AA_AngleP01		298

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
#define	AA_angle3w2iUABUCB  374
#define	AA_angle3w2iUABIA	  376
#define	AA_angle3w2iUCBIC	  378


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
//---------------------------------------------------average-archive-format------------

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
//---------------------------------------------------average-archive-format------------

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

//---------------------------------------------------average-archive-format-------------

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
//---------------------------------------------------average-archive-format--------------

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

//---------------------------------------------------average-archive-format---------------
//========================================================================================
									
const float afVoltageTransformationRatios[23] = 
	{
		1,
		30,
		31.5,
		33,
		60,
		66,
		100,
		105,
		110,
		138,
		150,
		157.5,
		180,
		200,
		240,
		270,
		350,
		1100,
		1500,
		2200,
		3300,
		5000,
		7500
	};













