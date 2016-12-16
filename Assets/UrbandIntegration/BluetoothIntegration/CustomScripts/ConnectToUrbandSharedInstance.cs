﻿using UnityEngine;
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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void InitBluetoothLE(bool isFirst, System.Action action){
		BluetoothLEHardwareInterface.Initialize (true, false, isFirst, () => {
			if(deviceIsSelected)
				InitConnection();
			else
				action();
		}, (error) => {
		});
	}

	public void OnConnect (string addressText)
	{
		_connectedID = addressText;
		deviceIsSelected = true;
		InitConnection();
	}

	public void InitConnection(){
		beginConnection((connectionOk) => {
			if(connectionOk)
				firstConnection();
			else
				InitConnection();
		});
	}

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
					// on Connection Action
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
							MakeUrbandRumble();
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
		string blueColor = "FF"
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
			byte[] value2 = new byte[] { (byte)0x01 };
			SendByte(value2, Haptics, HapticsControl, (action2) => {});	
		});
	}

	/*public void MakeUrbandRumble(
		int vibrationIntensityInitialTime = 0,
		int vibrationIntensityEndTime = 100,
		int LEDbrightnessLevelInitialTime = 0,
		int LEDbrightnessLevelEndTime = 100,
		int LEDbrightnessVibrationDelayTime = 0,
		int LEDbrightnessVibrationTrancisionTime = 50,
		int LEDbrightnessVibrationDurationTime = 50,
		int LEDbrightnessVibrationDownTime = 0,
		string redColor = "FF",
		string greenColor = "FF",
		string blueColor = "FF"
	){
		byte redByte = Convert.ToByte (redColor);
		byte greenByte = Convert.ToByte (greenColor);
		byte blueByte = Convert.ToByte (blueColor);
		// Send rumble action data to Urband
		byte[] value = new byte[] {
			IntToHex(vibrationIntensityInitialTime),
			IntToHex(vibrationIntensityEndTime),
			IntToHex(LEDbrightnessLevelInitialTime),
			IntToHex(LEDbrightnessLevelEndTime),
			IntToHex(LEDbrightnessVibrationDelayTime),
			IntToHex(LEDbrightnessVibrationTrancisionTime),
			IntToHex(LEDbrightnessVibrationDurationTime),
			IntToHex(LEDbrightnessVibrationDownTime),
			redByte,
			greenByte,
			blueByte
		};
		SendByte(value, Haptics, HapticsConfig, (action) => {
			byte[] value2 = new byte[] { (byte)0x01 };
			SendByte(value2, Haptics, HapticsControl, (action2) => {});	
		});
	}*/

	byte IntToHex(int intValue){
		// Convert integer 182 as a hex in a string variable
		string hexValue = intValue.ToString("X");
		int restult = int.Parse (hexValue);
		byte byteResult = Convert.ToByte (restult);
		return byteResult;
	}
}
