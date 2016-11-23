using UnityEngine;
using UnityEngine.UI;

public class DeviceButton : MonoBehaviour {
	// Public Vars
	public string name;
	public string address;
	// Private Vars
	private ConnectToUrbandSharedInstance connectToDevice;
	// Use this for initialization
	public DeviceButton(string _name, string _address){
		this.name = _name;
		this.address = _address;
	}

	void Start () {
		//Connect to device instance
		GameObject conectGO = GameObject.Find ("ConnectToUrbandSharedInstance");
		connectToDevice = conectGO.GetComponent<ConnectToUrbandSharedInstance> ();

		GameObject childText = transform.Find("Text").gameObject;
		Text txt = childText.GetComponent<Text> ();
		txt.text = name + " - " + address;
		name = txt.text;
		Button btn = GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
	}

	void TaskOnClick(){
		// Stop device Scan
		BluetoothLEHardwareInterface.StopScan ();
		connectToDevice.OnConnect (address);
	}
}
