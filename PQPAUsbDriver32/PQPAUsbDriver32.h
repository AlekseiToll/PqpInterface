// PQPAUsbDriver32.h

#pragma once

namespace PQPAUsbDriver32 
{
	public ref class UsbComm
	{
		// TODO: Add your methods for this class here.
		private:
			// ����� ����������, � ������� �������� �������.
			int ActiveDev;

		public:
			// ����� ��������� ���������� �� ���������� USB.
			int OpenConnection();
			// ����� ��������� ���������� �� ���������� USB.
			int CloseConnection();
			// ����� ��������� ����� �� ���������� USB.
			int Exchange(unsigned char * pTx, unsigned long SendBytes,
                         unsigned char * pRx, unsigned long &ReadBytes,
                         unsigned int WaitBytes);
			int WriteByte(unsigned char * pTx, unsigned long SendBytes);
			int ReadByte(unsigned char * pRx, unsigned long ReadBytes, int timeout);
	};
}
