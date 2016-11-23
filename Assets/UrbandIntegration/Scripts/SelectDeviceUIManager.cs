using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectDeviceUIManager : MonoBehaviour {
	// Public Vars
	public GameObject loading;
	public GameObject deviceList;
	public Image urbandImage;
	public string scene;
	// Private Vars
	private bool UIIsHidden = false;
	private ConnectToUrbandSharedInstance connectToDevice;
	// Use this for initialization
	void Start () {
		GameObject connectGObj = GameObject.Find ("ConnectToUrbandSharedInstance");
		connectToDevice = connectGObj.GetComponent<ConnectToUrbandSharedInstance> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (connectToDevice.urbanConnected) {
			if (!UIIsHidden) {
				UIIsHidden = true;
				loading.SetActive (false);
				deviceList.SetActive (false);
				Color c = urbandImage.color;
				c.a = 1;
				urbandImage.color = c;
				//BluetoothLEHardwareInterface.DisconnectPeripheral (connectToDevice._connectedID, (action) => {});
				SceneManager.LoadSceneAsync (scene);
			}
		}
	}
}
