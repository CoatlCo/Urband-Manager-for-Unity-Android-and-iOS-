using UnityEngine;
using System.Collections;

public class AddButtons : MonoBehaviour {
	public GameObject buttonPrefabs;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	public void addButton(string name, string address){
		GameObject btn = Instantiate(buttonPrefabs) as GameObject;
		DeviceButton handleBtn = btn.GetComponent<DeviceButton> ();
		handleBtn.name = name;
		handleBtn.address = address;
		btn.transform.parent = transform;
	}
}
