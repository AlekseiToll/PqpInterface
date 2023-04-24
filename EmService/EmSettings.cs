using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace EmServiceLib
{
	public class EmSettings
	{
		#region Fields

		/// <summary>Synchronization state</summary>
		[NonSerialized]
		private bool settingsChanged_ = false;

		/// <summary>Settings file name</summary>
		[NonSerialized]
		private string settingsFileName_ = EmService.AppDirectory + "ReaderInterface.config";

		/// <summary>Current language</summary>
		[XmlIgnore]
		public string CurrentLanguage;

		/// <summary>Application interface language</summary>
		[NonSerialized]
		private string language_ = String.Empty;

		/// <summary>Float signs in all tables</summary>
		[NonSerialized]
		private int floatSigns_ = 0;

		/// <summary>Float signs in all tables</summary>
		[NonSerialized]
		private bool bShowAvgTooltip_ = false;

		/// <summary>IO Interface: 0 - COM; 1 - USB; 2 - Modem</summary>
		[NonSerialized]
		private EmPortType IOInterface_ = EmPortType.USB;

		/// <summary>1 or 0,001 (for A or kA)</summary>
		[NonSerialized]
		private float currentRatio_;

		/// <summary>1 or 0,001 (for V or kV)</summary>
		[NonSerialized]
		private float voltageRatio_;

		/// <summary>1 or 0,001 (for W or kW)</summary>
		[NonSerialized]
		private float powerRatio_;

		/// <summary>IP address for wi-fi</summary>
		[NonSerialized]
		private string curWifiProfileName_ = string.Empty;

		/// <summary>Password for wi-fi</summary>
		[NonSerialized]
		private string wifiPassword_ = string.Empty;

		/// <summary>Only for ethernet and GPRS</summary>
		[NonSerialized]
		private int curPort_ = 2200;

		/// <summary>Only for ethernet and GPRS</summary>
		[NonSerialized]
		private string curIPAddress_;

		/// <summary>If need to check new firmware for EtPQP-A</summary>
		[NonSerialized]
		private bool checkFirmwareEtPQP_A_;

		/// <summary>If need to check new software version</summary>
		[NonSerialized]
		private bool checkNewSoftwareVersion_;

		/// <summary>If need to warn that auto time synchronization is disabled</summary>
		[NonSerialized]
		private bool dontWarnAutoSynchroTimeDisabled_;

		/// <summary>Paths to stored archives</summary>
		[NonSerialized]
		private string[] pathToStoredArchives_;

		/// <summary>Last path to store loaded archives</summary>
		[NonSerialized]
		private string lastPathToStoreArchives_;

		#endregion

		#region Properties

		/// <summary>If need to warn that auto time synchronization is disabled</summary>
		public bool DontWarnAutoSynchroTimeDisabled
		{
			get { return dontWarnAutoSynchroTimeDisabled_; }
			set { dontWarnAutoSynchroTimeDisabled_ = value; }
		}

		/// <summary>If need to check new firmware for EtPQP-A</summary>
		public bool CheckFirmwareEtPQP_A
		{
			get { return checkFirmwareEtPQP_A_; }
			set { checkFirmwareEtPQP_A_ = value; }
		}

		/// <summary>If need to check new software version</summary>
		public bool CheckNewSoftwareVersion
		{
			get { return checkNewSoftwareVersion_; }
			set { checkNewSoftwareVersion_ = value; }
		}

		/// <summary>Gets settings state</summary>
		public bool SettingsChanged
		{
			get { return settingsChanged_; }
		}

		/// <summary>Gets settings file name</summary>
		public string SettingsFileName
		{
			get { return settingsFileName_; }
		}

		/// <summary>Gets or sets application interface language</summary>
		public string Language
		{
			get { return language_; }
			set
			{
				if (value != language_)
				{
					language_ = value;
					settingsChanged_ = true;
				}
			}
		}

		/// <summary>Float signs in all tables</summary>
		public int FloatSigns
		{
			get { return floatSigns_; }
			set
			{
				if (floatSigns_ != value)
				{
					floatSigns_ = value;
					settingsChanged_ = true;
				}
			}
		}

		/// <summary>Show Tooltips for AVG params</summary>
		public bool ShowAvgTooltip
		{
			get { return bShowAvgTooltip_; }
			set
			{
				if (bShowAvgTooltip_ != value)
				{
					bShowAvgTooltip_ = value;
					settingsChanged_ = true;
				}
			}
		}

		/// <summary>Gets string float format</summary>
		public string FloatFormat
		{
			get
			{
				StringBuilder sb = new StringBuilder("0");
				if (floatSigns_ > 0)
				{
					sb.Append(".");
					for (int i = 0; i < floatSigns_; i++)
					{
						sb.Append("0");
					}
				}
				return sb.ToString();
			}
		}

		/// <summary>Gets or sets I/O Interface</summary>
		public EmPortType IOInterface
		{
			get { return IOInterface_; }
			set { IOInterface_ = value; }
		}

		/// <summary>Current IP address (only for Ethernet and GPRS)</summary>		
		public string CurrentIPAddress
		{
			get { return curIPAddress_; }
			set
			{
				if (value != curIPAddress_)
				{
					curIPAddress_ = value;
					settingsChanged_ = true;
				}
			}
		}

		/// <summary>Current port (only for Ethernet and GPRS)</summary>		
		public int CurrentPort
		{
			get { return curPort_; }
			set
			{
				if (value != curPort_)
				{
					curPort_ = value;
					settingsChanged_ = true;
				}
			}
		}

		/// <summary>Gets connection interface parameters array</summary>
		public object[] IOParameters
		{
			get
			{
				switch (IOInterface)
				{
					case EmPortType.USB:
						return null;
					case EmPortType.Internet:
						return new object[] { curIPAddress_, curPort_ };
					case EmPortType.WI_FI:
						return new object[] { curWifiProfileName_, wifiPassword_ };
					default:
						return null;
				}
			}
		}

		/// <summary>1 or 0,001 (for A or kA)</summary>
		public float CurrentRatio
		{
			get { return currentRatio_; }
			set { currentRatio_ = value; }
		}

		/// <summary>1 or 0,001 (for V or kV)</summary>
		public float VoltageRatio
		{
			get { return voltageRatio_; }
			set { voltageRatio_ = value; }
		}

		/// <summary>1 or 0,001 (for W or kW)</summary>
		public float PowerRatio
		{
			get { return powerRatio_; }
			set { powerRatio_ = value; }
		}

		/// <summary>Password for wi-fi</summary>
		public string WifiPassword
		{
			get { return wifiPassword_; }
			set { wifiPassword_ = value; }
		}

		public string CurWifiProfileName
		{
			get { return curWifiProfileName_; }
			set { curWifiProfileName_ = value; }
		}

		/// <summary>Path to stored archives</summary>
		public string[] PathToStoredArchives
		{
			get { return pathToStoredArchives_; }
			set { pathToStoredArchives_ = value; }
		}

		/// <summary>Last path to store loaded archives</summary>
		public string LastPathToStoreArchives
		{
			get { return lastPathToStoreArchives_; }
			set { lastPathToStoreArchives_ = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor with defaut settings
		/// </summary>
		public EmSettings()
		{
			// defaults:
			this.language_ = "English";
			this.CurrentLanguage = "en";
			this.floatSigns_ = 2;
			this.bShowAvgTooltip_ = false;
			this.IOInterface_ = EmPortType.USB;
			this.settingsChanged_ = false;
			this.currentRatio_ = 0.0F;
			this.voltageRatio_ = 0.0F;
			this.powerRatio_ = 0.0F;
			this.checkFirmwareEtPQP_A_ = true;
			this.checkNewSoftwareVersion_ = true;
			this.dontWarnAutoSynchroTimeDisabled_ = false;

			this.pathToStoredArchives_ = new string[0];
			this.lastPathToStoreArchives_ = string.Empty;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Serializes the class to the config file if any of the settings have changed.
		/// </summary>
		public void SaveSettings()
		{
			StreamWriter myWriter = null;
			XmlSerializer mySerializer = null;
			try
			{
				// Create an XmlSerializer for the 
				// ApplicationSettings type.
				mySerializer = new XmlSerializer(typeof(EmSettings));
				myWriter = new StreamWriter(settingsFileName_, false);
				// Serialize this instance of the ApplicationSettings 
				// class to the config file.
				mySerializer.Serialize(myWriter, this);
				this.settingsChanged_ = false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in SaveSettings(): ");
				throw;
			}
			finally
			{
				if (myWriter != null) myWriter.Close();
			}
		}

		/// <summary>
		/// Deserializes the class from the config file.
		/// </summary>
		public void LoadSettings()
		{
			XmlSerializer mySerializer = null;
			FileStream myFileStream = null;
			try
			{
				// Create an XmlSerializer for the ApplicationSettings type.
				mySerializer = new XmlSerializer(typeof(EmSettings));
				FileInfo fi = new FileInfo(settingsFileName_);
				// If the config file exists, open it.
				if (fi.Exists)
				{
					myFileStream = fi.OpenRead();
					// Create a new instance of the ApplicationSettings by
					// deserializing the config file.
					EmSettings myAppSettings = (EmSettings)mySerializer.Deserialize(myFileStream);
					// Assign the property values to this instance of 
					// the ApplicationSettings class.
					this.language_ = myAppSettings.language_;
					this.IOInterface_ = myAppSettings.IOInterface_;
					this.settingsChanged_ = false;
					this.floatSigns_ = myAppSettings.floatSigns_;
					this.bShowAvgTooltip_ = myAppSettings.bShowAvgTooltip_;
					this.currentRatio_ = myAppSettings.currentRatio_;
					this.voltageRatio_ = myAppSettings.voltageRatio_;
					this.powerRatio_ = myAppSettings.powerRatio_;
					this.curIPAddress_ = myAppSettings.curIPAddress_;
					//this.curWifiIPaddress_ = myAppSettings.curWifiIPaddress_;
					this.wifiPassword_ = myAppSettings.wifiPassword_;
					this.curWifiProfileName_ = myAppSettings.curWifiProfileName_;
					this.curPort_ = myAppSettings.curPort_;
					this.checkFirmwareEtPQP_A_ = myAppSettings.checkFirmwareEtPQP_A_;
					this.checkNewSoftwareVersion_ = myAppSettings.checkNewSoftwareVersion_;
					this.dontWarnAutoSynchroTimeDisabled_ = myAppSettings.dontWarnAutoSynchroTimeDisabled_;
					this.pathToStoredArchives_ = myAppSettings.pathToStoredArchives_;
					this.lastPathToStoreArchives_ = myAppSettings.lastPathToStoreArchives_;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in LoadSettings(): ");
				throw;
			}
			finally
			{
				if (myFileStream != null) myFileStream.Close();
			}
		}

		/// <summary>
		/// Clone method
		/// </summary>
		/// <returns>Copy of this object</returns>
		public EmSettings Clone()
		{
			EmSettings obj = new EmSettings();
			obj.language_ = this.language_;
			obj.settingsChanged_ = this.settingsChanged_;
			obj.CurrentLanguage = this.CurrentLanguage;
			obj.floatSigns_ = this.floatSigns_;
			obj.bShowAvgTooltip_ = this.bShowAvgTooltip_;
			obj.IOInterface_ = this.IOInterface_;
			obj.currentRatio_ = this.currentRatio_;
			obj.voltageRatio_ = this.voltageRatio_;
			obj.powerRatio_ = this.powerRatio_;
			obj.curIPAddress_ = this.curIPAddress_;
			obj.wifiPassword_ = this.wifiPassword_;
			obj.curWifiProfileName_ = this.curWifiProfileName_;
			obj.curPort_ = this.curPort_;
			obj.checkFirmwareEtPQP_A_ = this.checkFirmwareEtPQP_A_;
			obj.checkNewSoftwareVersion_ = this.checkNewSoftwareVersion_;
			obj.dontWarnAutoSynchroTimeDisabled_ = this.dontWarnAutoSynchroTimeDisabled_;

			obj.pathToStoredArchives_ = new string[this.pathToStoredArchives_.Length];
			Array.Copy(this.pathToStoredArchives_, obj.pathToStoredArchives_,
				obj.pathToStoredArchives_.Length);

			obj.lastPathToStoreArchives_ = this.lastPathToStoreArchives_;

			return obj;
		}

		#endregion

		//#region auto settings

		//[NonSerialized]
		//public AutoSettingsData AutoSettings = new AutoSettingsData();

		//public class AutoSettingsData
		//{
		//    public EmPortType AutoIOInterface;

		//    //ethernet or GPRS
		//    public int AutoPort;
		//    public string AutoIPAddress;

		//    //RS485
		//    public ushort AutoDeviceAddress;
		//}

		//#endregion
	}	
}
