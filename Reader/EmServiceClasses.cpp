#include "stdafx.h"
#include "EmServiceClasses.h"

std::string EmService::logFailedName = "LogFailedCpp.txt";
std::string EmService::logGeneralName = "LogGeneralCpp.txt";
std::string EmService::appDirectory = "";
bool EmService::appDirWasSet = false;

//CRITICAL_SECTION EmService::scLogFailed;
//CRITICAL_SECTION EmService::scLogGeneral;

bool operator ==(const DateTime& d1, const DateTime& d2)
{
	return (d1.year_ == d2.year_ && d1.month_ == d2.month_ && d1.day_ == d2.day_ &&
		d1.hour_ == d2.hour_ && d1.minutes_ == d2.minutes_ && d1.seconds_ == d2.seconds_ &&
		d1.milliseconds_ == d2.milliseconds_);
}
