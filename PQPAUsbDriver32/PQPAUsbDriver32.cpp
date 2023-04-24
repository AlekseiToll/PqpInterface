// This is the main DLL file.

#include "stdafx.h"

#include "PQPAUsbDriver32.h"
#include "UsbDevice.h"

namespace PQPAUsbDriver32
{
	// ����� ��������� �������� ���������� � ������������ ��������.
	int UsbComm::OpenConnection()
	{ 
		if (UsbConnect())
			return 0;
		else
			return -1;
	}

	// ����� ��������� �������� ������������ USB.
	int UsbComm::CloseConnection()
	{   // �������� ����� ��� �������� ����������.
		UsbDisconnect();
		// ���������� ��� ��������� �������� ���������� �� USB.
		return 0;
	}

	// ����� ��������� ����� �� USB: �������� ��������� ���-�� ������
	// �� ���������� ������� � ����� � ��������� ����� ���������� ���-�� ������.
	int UsbComm::Exchange(unsigned char * pTx, unsigned long SendBytes,
		                  unsigned char * pRx, unsigned long &ReadBytes,
                          unsigned int WaitBytes)
	{   
		// ���������� ��� ��������� ������ � ��������.
		return 0;
	}
	// ����� ��������� �������� ����� � ����.
	int UsbComm::WriteByte(unsigned char * pTx, unsigned long SendBytes)
	{   // �������� ����� ������ ����� � ����.
		if (SendData(pTx, SendBytes, 500))
			return 0;
		else
			return -2;
	}
	// ����� ��������� ������ ����� �� �����.
	int UsbComm::ReadByte(unsigned char * pRx, unsigned long ReadBytes, int timeout)
	{   // �������� ����� ��� ������ �����.
		return ReadData(pRx, timeout);
	}

}