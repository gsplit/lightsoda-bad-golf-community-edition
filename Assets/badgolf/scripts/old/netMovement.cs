using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class netMovement : MonoBehaviour {
	float forceMultiplyer = 10000;
	public GameObject cameraGameObject;
	Vector3 cameraPos = new Vector3(0,2,-4);
	Dictionary<NetworkViewID,int> KBinfos = new Dictionary<NetworkViewID,int>();
	float timer = 0;
	public bool isMine = false;
	
	// Update is called once per frame
	void Update () {
		// only do fiziks if it's the server
		if (Network.isServer) {
			foreach(KeyValuePair<NetworkViewID,int> entry in KBinfos)
			{
				GameObject playerGameObject = NetworkView.Find(entry.Key).gameObject;
				Vector3 forceFromFront = new Vector3();	// force from front tires
				Vector3 forceFromBack = new Vector3();	// force from back tires
				if ((entry.Value & 8)==8) {
					// make sure it's facing the direction of the vehicle
					forceFromFront += playerGameObject.transform.localRotation * Vector3.forward;
					forceFromBack += playerGameObject.transform.localRotation * Vector3.forward;
				}
				if ((entry.Value & 4)==4) {
					// make sure it's facing the direction of the vehicle
					forceFromFront += playerGameObject.transform.localRotation * Vector3.back;
					forceFromBack += playerGameObject.transform.localRotation * Vector3.back;
				}
				if ((entry.Value & 2)==2) {
					// rotate the front forces if they are turning
					forceFromFront = Quaternion.AngleAxis(-60,Vector3.up) * forceFromFront;
				}
				if ((entry.Value & 1)==1) {
					// rotate the front forces if they are turning
					forceFromFront = Quaternion.AngleAxis(60,Vector3.up) * forceFromFront;
				}
				if (forceFromFront.sqrMagnitude!=0) {
					// one at each tyre
					playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront,playerGameObject.transform.position+transform.localRotation*Vector3.forward);
					playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront,playerGameObject.transform.position+transform.localRotation*Vector3.forward);
					playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack,playerGameObject.transform.position+transform.localRotation*Vector3.back);
					playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack,playerGameObject.transform.position+transform.localRotation*Vector3.back);
				}
			}
		} else if (isMine) {
			timer += Time.deltaTime;
			if (timer > 0.015) {
				timer = 0;
				int toSend = 0;
				if (Input.GetKey(KeyCode.W)) {
					toSend += 1;
				}
				toSend = toSend << 1;
				if (Input.GetKey(KeyCode.S)) {
					toSend += 1;
				}
				toSend = toSend << 1;
				if (Input.GetKey(KeyCode.A)) {
					toSend += 1;
				}
				toSend = toSend << 1;
				if (Input.GetKey(KeyCode.D)) {
					toSend += 1;
				}
				networkView.RPC("KartMovement", RPCMode.Server, networkView.viewID, toSend);
			}
		}
	}
	
	// put camera movement in here for reasons
	void FixedUpdate() {
		if (Network.isClient && isMine) {
			// camera movement
			Vector3 newPos = transform.position + transform.localRotation * cameraPos;
			float lerper = (cameraGameObject.transform.position - newPos).sqrMagnitude / 400;
			cameraGameObject.transform.position = Vector3.Lerp(cameraGameObject.transform.position, newPos, lerper);
			cameraGameObject.transform.rotation = Quaternion.Lerp(cameraGameObject.transform.rotation, Quaternion.LookRotation(transform.position-cameraGameObject.transform.position), 0.75f);
		}
	}

	// update what they are currenly doing
	[RPC]
	void KartMovement(NetworkViewID viewId, int currentKBStatus) {
		KBinfos[viewId] = currentKBStatus;
	}
}
