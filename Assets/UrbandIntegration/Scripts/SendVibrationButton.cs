using UnityEngine;
using UnityEngine.UI;

public class SendVibrationButton : MonoBehaviour {
	// Private Vars
	private ConnectToUrbandSharedInstance connectToDevice;

	// Use this for initialization
	void Start () {
		//Connect to device instance
		GameObject conectGO = GameObject.Find ("ConnectToUrbandSharedInstance");
		connectToDevice = conectGO.GetComponent<ConnectToUrbandSharedInstance> ();

		Button btn = GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
	}
	
	void TaskOnClick(){
		// Stop device Scan
		connectToDevice.MakeUrbandRumble (
			0,
			50,
			100,
			100,
			0, 
			10,
			10,
			0,
			0,
			"FF",
			"FF",
			"00"
		);
	}
}
