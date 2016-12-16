using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DetectDevice : MonoBehaviour
{
	// Private Vars
	private bool _scanning = false;
	private ConnectToUrbandSharedInstance connectToDevice;
	private AddButtons addButtons;

	public void Initialize ()
	{
		connectToDevice.InitBluetoothLE (() => {
			OnScan ();	
		});
	}

	public void OnBack ()
	{
		if (_scanning)
			OnScan (); // this will stop scanning

		BluetoothLEHardwareInterface.DeInitialize (() => {
		});
	}

	public void OnScan ()
	{
		// the first callback will only get called the first time this device is seen
		// this is because it gets added to a list in the BluetoothDeviceScript
		// after that only the second callback will get called and only if there is
		// advertising data available
		BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, (address, name) => {
			// Detect only urband devices
			if(name.Contains("Urband"))
				AddPeripheral (name, address);
		}, (address, name, rssi, advertisingInfo) => {
			
		});
	}

	void AddPeripheral (string name, string address)
	{
		// Stop device Scan
		//BluetoothLEHardwareInterface.StopScan ();
		//connectToDevice.OnConnect (address);
		addButtons.addButton(name, address);
	}

	// Use this for initialization
	void Start ()
	{
		GameObject connectGObj = GameObject.Find ("ConnectToUrbandSharedInstance");
		connectToDevice = connectGObj.GetComponent<ConnectToUrbandSharedInstance> ();
		addButtons = GameObject.Find ("List").GetComponent<AddButtons> ();
		Initialize ();
	}
}
