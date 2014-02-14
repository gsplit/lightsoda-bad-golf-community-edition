using UnityEngine;
using System.Collections;

public class controlClient : MonoBehaviour {
	float timer = 0;
	public NetworkViewID myViewID;
	public controlServer ms;
	bool chatVisible = false;
	string chatBuffer = "Click here";

	void Update () {
		// send packets about keyboard every 0.015s
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
			if (Network.isServer) {
				ms.KartMovement(myViewID, toSend);
			} else {
				networkView.RPC("KartMovement", RPCMode.Server, myViewID, toSend);
			}
			if (Input.GetKeyDown(KeyCode.Q)) {
				networkView.RPC("IHonked", RPCMode.All, myViewID);
			}
			if (Input.GetKeyDown(KeyCode.T)) {
				chatVisible = true;
			}
			if (Input.GetKeyDown(KeyCode.Escape)) {
				chatVisible = false;
			}
			if (Input.GetKeyDown(KeyCode.Return)) {
				chatVisible = false;
				networkView.RPC("PrintText", RPCMode.AllBuffered, chatBuffer);
				chatBuffer = "Click here";
			}
		}
	}
	// chat box
	void OnGUI() {
		if (chatVisible) {
			GUILayout.BeginHorizontal();
			chatBuffer = GUILayout.TextField(chatBuffer);
			GUILayout.EndHorizontal();
		}
	}

	
	// honks
	[RPC]
	void IHonked(NetworkViewID viewId) {
		NetworkView.Find(viewId).gameObject.audio.Play();
	}

	// blank for server use only
	[RPC]
	void KartMovement(NetworkViewID viewId, int currentKBStatus) {}
}
