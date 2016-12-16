using UnityEngine;
using System;

public class ConnectToUrbandSharedInstance : MonoBehaviour {
	// Private Vars
	private bool _connecting = false;
	private string _connectedID = "";
	private bool deviceIsSelected = false;

	#if UNITY_IPHONE
	private string SecureService = "FC00";
	private string SecureServiceConnection = "FC02";

	private string UrbandS = "FA00";
	private string UrbandSGesture = "FA01";

	private string Haptics = "FB00";
	private string HapticsControl = "FB01";
	private string HapticsConfig = "FB02";

	private int serviceLimit = 23;
	#endif

	#if UNITY_ANDROID
	private string SecureService = "0000fc00-0000-1000-8000-00805f9b34fb";
	private string SecureServiceConnection = "0000fc02-0000-1000-8000-00805f9b34fb";

	private string UrbandS = "0000fa00-0000-1000-8000-00805f9b34fb";
	private string UrbandSGesture = "0000fa01-0000-1000-8000-00805f9b34fb";

	private string Haptics = "0000fb00-0000-1000-8000-00805f9b34fb";
	private string HapticsControl = "0000fb01-0000-1000-8000-00805f9b34fb";
	private string HapticsConfig = "0000fb02-0000-1000-8000-00805f9b34fb";

	private int serviceLimit = 26;
	#endif

	private bool sendConnection = true;
	private bool isFirst = true;
	private bool isFirstSecure = true;

	private int count = 0;

	private BluetoothDeviceScript bluetoothDevScript;

	// Public Vars
	public bool urbanDetected = false;
	public bool urbanConnected = false;
	public static ConnectToUrbandSharedInstance Instance;
	public bool makeUrbandRumble = false;
	public bool detectDoubleSpin = false;
	public bool detectDoubleTab = true;
	public bool listenUrbandMeasure = false;
	public int urbandMeasure;

	void Awake()
	{
		if (Instance == null) {
			DontDestroyOnLoad (gameObject);
			Instance = this;
		} else if (Instance != this) {
			Destroy (gameObject);
		}
	}

	// It initializes all the operation of bluetooth LE
	// To not initialize the bluetooth engine more than once,
	// Use the isFirst variable in true only the first time the Bluetooth engine is initialized
	// In the other scenes call the initializer with the variable isFirst in false
	public void InitBluetoothLE(System.Action action){
		BluetoothLEHardwareInterface.Initialize (true, false, () => {
			if(deviceIsSelected)
				InitConnection();
			else
				action();
		}, (error) => {
		});
	}

	// Save the mac address of the bluetooth device
	// Connects to the bluetooth device
	public void OnConnect (string addressText)
	{
		_connectedID = addressText;
		deviceIsSelected = true;
		InitConnection();
	}

	// Try to start and establish the connection with the device
	public void InitConnection(){
		beginConnection((connectionOk) => {
			if(connectionOk)
				firstConnection();
			else
				InitConnection();
		});
	}

	// Ends the connection with the bluetooth device
	public void OnDisConnect (System.Action action)
	{
		BluetoothLEHardwareInterface.DisconnectPeripheral (_connectedID, (resp) =>{
			action();
		});
	}

	// Write Characteristic
	void SendByte(
		byte[] value,
		string _serviceUUID,
		string _writeCharacteristicUUID,
		System.Action<string> action,
		System.Action<string> errorAction = null
		)
	{
		BluetoothLEHardwareInterface.WriteCharacteristic (
			_connectedID, 
			_serviceUUID, 
			_writeCharacteristicUUID, 
			value, value.Length, true, (characteristicUUID) => {
				BluetoothLEHardwareInterface.Log ("Write Succeeded");
				action(characteristicUUID);
			},
			(error) => {
				errorAction(error);
			}
		);
	}

	// Init connection
	void beginConnection(System.Action<bool> action) {
		if (!_connecting) {
			BluetoothLEHardwareInterface.ConnectToPeripheral (_connectedID, 
				(address) => {
					// On Connection Action
				},
				(address, serviceUUID) => {
					// Service detection
				},
				(address, serviceUUID, characteristicUUID) => {
					// Characteristis detection
					urbanDetected = true;
					if (count < serviceLimit)
						count++;
					else {
						action(true);
					}

				}, (error) => {
					//On connection error
					action(false);
				},
				(address) => {
				// this will get called when the device disconnects
				// be aware that this will also get called when the disconnect
				// is called above. both methods get call for the same action
				// this is for backwards compatibility
				//Connected = false;
			});

			_connecting = true;
		} else {
			action(true);
		}
	}

	// Suscribe and send fisrt detection msj
	void firstConnection(){
		BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress (
			_connectedID, 
			UrbandS, 
			UrbandSGesture, 
			(deviceAddress, notification) => {
				
			}, 
			(deviceAddress2, characteristic, data) => {
				if(isFirst) {
					if(data[0] == 17) {
						isFirst = false;
						// Send first detection msj
						byte[] value = new byte[] { (byte)0x00 };
						SendByte(
							value,
							UrbandS,
							UrbandSGesture, 
							(action) => {
								// Continue whit secure auth service
								connectSegureService();
							});
					}	
				}
				// Afther urband is secure connected, Listen and notify urband gestures
				if(urbanConnected)
				{
					if(!listenUrbandMeasure){
						//Listen mode deactivated

						// 20 = 0x14 double spin
						if(data[0] == 20)
						{
							// Active listen mode
							listenUrbandMeasure = true;
						}
						// 21 = 0x15 double spin
						if(data[0] == 21)
						{
							// Active listen mode
							detectDoubleTab = true;
						}
						// 22 = 0x16 double spin
						if(data[0] == 22)
						{
							detectDoubleSpin = true;
							listenUrbandMeasure = false;
						}
					}
					else
					{
						//Listen mode actived
						urbandMeasure = data[0];
						if(data[0] == 22)
						{
							detectDoubleSpin = true;
							listenUrbandMeasure = false;
						}
					}
				}
			}
		);
	}

	// Suscribe and send secure auth service token
	void connectSegureService(){
		BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress (
			_connectedID, 
			SecureService, 
			SecureServiceConnection, 
			(deviceAddress, notification) => {
				if (isFirstSecure) {
					isFirstSecure = false;
					// Send secure auth service token
					byte[] value = new byte[] {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};
					SendByte (
						value,
						SecureService, 
						SecureServiceConnection,
						(action2) => {
							sendConnection = false;
							// Notify that Urband is connected
							urbanConnected = true;
							// Se envia una peticion para que la urband vibre y confirmar la conneccion
							MakeUrbandRumble(
								0,
								50,
								100,
								100,
								0, 
								10,
								10,
								0,
								5,
								"FF",
								"FF",
								"FF",
								2
							);
							// Suscribe to gesture service again
							firstConnection();
						},
						(error) => {
							isFirstSecure = true;
							connectSegureService();
						}
					);
				}
			}, 
			(deviceAddress2, characteristic, data) => {
				
			}
		);
	}

	// It allows to configure the necessary parameters to control the way the urband vibrates
	// And also the parameters to control the color, brightness and duration of the notification LED
	// The parameters are:
	// vibrationIntensityInitialTime, vibrationIntensityEndTime
		// LEDbrightnessLevelInitialTime, LEDbrightnessLevelEndTime
			// Are of integer type and the allowed values go from 0 to 100
	// LEDbrightnessVibrationDelayTime, LEDbrightnessVibrationTrancisionTime, LEDbrightnessVibrationDurationTime
		// LEDbrightnessVibrationDownTime, LEDbrightnessVibrationOffTime
			// Are integer type and the allowed values range from 0 to 22 and are increments of 10 milliseconds
	// redColor, greenColor, blueColor
		// They are of type string and represent a color in RGB format
	// LEDbrightnessVibrationRepetitions
		// It is integer type and allowed values range from 0 to 50
	public void MakeUrbandRumble(
		int vibrationIntensityInitialTime = 0,
		int vibrationIntensityEndTime = 100,
		int LEDbrightnessLevelInitialTime = 0,
		int LEDbrightnessLevelEndTime = 100,
		int LEDbrightnessVibrationDelayTime = 0,
		int LEDbrightnessVibrationTrancisionTime = 5,
		int LEDbrightnessVibrationDurationTime = 10,
		int LEDbrightnessVibrationDownTime = 0,
		int LEDbrightnessVibrationOffTime = 0,
		string redColor = "FF",
		string greenColor = "FF",
		string blueColor = "FF",
		int LEDbrightnessVibrationRepetitions = 1
	){
		byte redByte = Convert.ToByte (redColor.Substring (0, 2), 16);
		byte greenByte = Convert.ToByte (greenColor.Substring (0, 2), 16);
		byte blueByte = Convert.ToByte (blueColor.Substring (0, 2), 16);
		// Send rumble action data to Urband
		byte[] value = new byte[] {
			IntToHex(vibrationIntensityInitialTime),
			IntToHex(vibrationIntensityEndTime),
			IntToHex(LEDbrightnessLevelInitialTime),
			IntToHex(LEDbrightnessLevelEndTime),
			IntToHex(LEDbrightnessVibrationDelayTime * 10),
			IntToHex(LEDbrightnessVibrationTrancisionTime * 10),
			IntToHex(LEDbrightnessVibrationDurationTime * 10),
			IntToHex(LEDbrightnessVibrationDownTime * 10),
			IntToHex(LEDbrightnessVibrationOffTime * 10),
			redByte,
			greenByte,
			blueByte
		};
		SendByte(value, Haptics, HapticsConfig, (action) => {
			byte[] value2 = new byte[] { IntToHex(LEDbrightnessVibrationRepetitions) };
			SendByte(value2, Haptics, HapticsControl, (action2) => {});	
		});
	}

	// Converts an integer to its hexadecimal equivalent
	byte IntToHex(int intValue){
		string hexValue = intValue.ToString("X");
		int restult = int.Parse (hexValue);
		byte byteResult = Convert.ToByte (restult);
		return byteResult;
	}
}
