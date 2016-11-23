using UnityEngine;
using System.Collections;

public class RotateLoading : MonoBehaviour {

	// Use this for initialization
	private RectTransform rectTransform;
	void Start () {
		rectTransform = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		rectTransform.Rotate( Vector3.forward * (Time.deltaTime * 200) );
	}
}
