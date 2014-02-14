using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlServer : MonoBehaviour {
	float forceMultiplyer = 10000;
	Dictionary<NetworkViewID,int> KBinfos = new Dictionary<NetworkViewID,int>();

	// UPDATE ALL THE FIZIKS!
	void FixedUpdate () {
		foreach(KeyValuePair<NetworkViewID,int> entry in KBinfos)
		{
			// probably not best to call Find every fiz update - will optimize later
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
				playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront,playerGameObject.transform.position+playerGameObject.transform.localRotation*Vector3.forward);
				playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront,playerGameObject.transform.position+playerGameObject.transform.localRotation*Vector3.forward);
				playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack,playerGameObject.transform.position+playerGameObject.transform.localRotation*Vector3.back);
				playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack,playerGameObject.transform.position+playerGameObject.transform.localRotation*Vector3.back);
			}
		}
	}
	
	// remove players from update table
	void OnPlayerDisconnected(NetworkPlayer player) {
		NetworkViewID keyToRemove = NetworkViewID.unassigned;
		foreach(KeyValuePair<NetworkViewID,int> entry in KBinfos)
		{
			if (entry.Key.owner==player) keyToRemove=entry.Key;
		}
		if (KBinfos.ContainsKey(keyToRemove)) KBinfos.Remove(keyToRemove);
	}



	// update what they are currenly doing - this also adds new updates
	[RPC]
	public void KartMovement(NetworkViewID viewId, int currentKBStatus) {
		KBinfos[viewId] = currentKBStatus;
	}

	// honks
	[RPC]
	void IHonked(NetworkViewID viewId) {
		NetworkView.Find(viewId).gameObject.audio.Play();
	}
}
