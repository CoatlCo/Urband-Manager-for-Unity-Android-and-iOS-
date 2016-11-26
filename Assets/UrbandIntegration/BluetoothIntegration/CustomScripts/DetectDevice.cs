using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DetectDevice : MonoBehaviour
{
	// Private Vars
	private bool _scanning = false;
	private AddButtons addButtons;
	private ConnectToUrbandSharedInstance connectToDevice;

	public void Initialize ()
	{
		GameObject listView = GameObject.Find ("List");
		addButtons = listView.GetComponent<AddButtons> ();
		connectToDevice.InitBluetoothLE (true, () => {
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
			//if(address == "B0:B4:48:DD:69:ED")
			if(name.Contains("Urband"))
				AddPeripheral (name, address);
		}, (address, name, rssi, advertisingInfo) => {
			
		});
	}

	void AddPeripheral (string name, string address)
	{
		// Stop device Scan
		addButtons.addButton(name, address);
	}

	// Use this for initialization
	void Start ()
	{
		GameObject connectGObj = GameObject.Find ("ConnectToUrbandSharedInstance");
		connectToDevice = connectGObj.GetComponent<ConnectToUrbandSharedInstance> ();
		Initialize ();
	}
}
