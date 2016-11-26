using UnityEngine;
using UnityEngine.UI;

public class SampleShowGestures : MonoBehaviour {
	//public Vars
	public Text listenGestureTxt;
	// Public Vars
	public GameObject loading;
	public Image urbandImage;
	// Private Vars
	private ConnectToUrbandSharedInstance connectToDevice;
	private bool UIUrbanConnected = false;
	// Use this for initialization
	void Start () {
		//Connect to device instance
		GameObject conectGO = GameObject.Find ("ConnectToUrbandSharedInstance");
		connectToDevice = conectGO.GetComponent<ConnectToUrbandSharedInstance> ();
		connectToDevice.InitBluetoothLE (false, () => {
		});
	}
	
	// Update is called once per frame
	void Update () {
		if (connectToDevice.urbanConnected) {
			if (!UIUrbanConnected) {
				UIUrbanConnected = true;
				loading.SetActive (false);
				Color c = urbandImage.color;
				c.a = 1;
				urbandImage.color = c;
			}
			if (!connectToDevice.listenUrbandMeasure) {
				if (connectToDevice.detectDoubleSpin) {
					listenGestureTxt.text = "Listen Gesture: Doble Spin";
					connectToDevice.detectDoubleSpin = false;
				}
				if (connectToDevice.detectDoubleTab) {
					listenGestureTxt.text = "Listen Gesture: Doble Tap";
					connectToDevice.detectDoubleTab = false;
				}
			} else {
				listenGestureTxt.text = "Listen Mode meassurement: " + connectToDevice.urbandMeasure;
			}
		}
	}
}
