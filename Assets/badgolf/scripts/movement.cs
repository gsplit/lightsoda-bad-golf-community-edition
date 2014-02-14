﻿using UnityEngine;
using System.Collections;

public class movement : MonoBehaviour {
	float forceMultiplyer = 10000;

	// Update is called once per frame
	void FixedUpdate () {
		Vector3 forceFromFront = new Vector3();	// force from front tires
		Vector3 forceFromBack = new Vector3();	// force from back tires
		if (Input.GetKey(KeyCode.W)) {
			// make sure it's facing the direction of the vehicle
			forceFromFront += transform.localRotation * Vector3.forward;
			forceFromBack += transform.localRotation * Vector3.forward;
		}
		if (Input.GetKey(KeyCode.S)) {
			// make sure it's facing the direction of the vehicle
			forceFromFront += transform.localRotation * Vector3.back;
			forceFromBack += transform.localRotation * Vector3.back;
		}
		if (Input.GetKey(KeyCode.A)) {
			// rotate the front forces if they are turning
			forceFromFront = Quaternion.AngleAxis(-60,Vector3.up) * forceFromFront;
		}
		if (Input.GetKey(KeyCode.D)) {
			// rotate the front forces if they are turning
			forceFromFront = Quaternion.AngleAxis(60,Vector3.up) * forceFromFront;
		}
		if (forceFromFront.sqrMagnitude!=0) {
			// one at each tyre
			rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront,transform.position+transform.localRotation*Vector3.forward);
			rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront,transform.position+transform.localRotation*Vector3.forward);
			rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack,transform.position+transform.localRotation*Vector3.back);
			rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack,transform.position+transform.localRotation*Vector3.back);
		}
	}

	void OnGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Fps: " + Mathf.Floor(100/Time.deltaTime)/100);
		GUILayout.EndHorizontal();
	}
}
