using System;
using System.Resources;
using System.Windows.Forms;
using EmServiceLib;

namespace MainInterface
{
	public class MessageBoxes
	{
		public static void InvalidData(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("MainInterface.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_invalid_data");
				string cap = rm.GetString("caption_error");
				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in InvalidData:");
				throw;
			}
		}

		public static void MsgErrorGetVolValues(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("MainInterface.emstrings",
				                                         sender.GetType().Assembly);
				string msg = rm.GetString("msg_error_get_volvalues");
				string cap = rm.GetString("caption_unfortunately");
				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in MsgErrorGetVolValues:");
				throw;
			}
		}

		public static void DeviceHasNoData(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("MainInterface.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_device_empty");
				string cap = rm.GetString("caption_information");
				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceHasNoData:");
				throw;
			}
		}

		public static void DeviceOldVersion(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("MainInterface.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_device_version_is_old");
				string cap = rm.GetString("caption_unfortunately");
				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceOldVersion:");
				throw;
			}
		}

		public static void DeviceConnectionError(object sender,
					EmPortType portInterface, object[] portSettings)
		{
			try
			{
				ResourceManager rm = new ResourceManager("MainInterface.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_device_connect_error");
					//???????????????? в свитче сделать сообщения для интернета и вайфая информативными
				string cap = rm.GetString("caption_device_connect_error");

				switch (portInterface)
				{
					case EmPortType.USB:
						msg = string.Format(msg, portInterface, string.Empty, string.Empty);
						msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
						break;
					case EmPortType.Internet:
						msg = string.Format(msg, portInterface, string.Empty, string.Empty);
						msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
						break;
					case EmPortType.WI_FI:
						msg = string.Format(msg, portInterface, string.Empty, string.Empty);
						msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
						break;
					default:
						throw (new EmException("Error 0x02432"));
				}

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceConnectionError:");
				throw;
			}
		}

		public static void ReadDevInfoError(object sender)
		{
			try
			{
				ResourceManager rm = new ResourceManager("MainInterface.emstrings", sender.GetType().Assembly);
				string msg = rm.GetString("msg_read_device_info_error");
				string cap = rm.GetString("caption_unfortunately");
				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadDevInfoError:");
				throw;
			}
		}
	}
}
