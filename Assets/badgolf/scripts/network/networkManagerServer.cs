using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManagerServer : MonoBehaviour {
	ArrayList playerGameObjects = new ArrayList();
	Dictionary<float,string> screenMessages = new Dictionary<float,string>();
	public GameObject myCart;

	// Use this for initialization
	void Start () {
		// Use NAT punchthrough if no public IP present
		Network.InitializeServer(8, 11177, !Network.HavePublicAddress());
		MasterServer.RegisterHost("HL3", SystemInfo.deviceName, "Test server");
		
		// create server owners buggy
		myCart = Instantiate(Resources.Load("buggy1"), new Vector3(0,5,0), Quaternion.identity) as GameObject;
		// networkview that shit
		NetworkViewID viewID = Network.AllocateViewID();
		myCart.networkView.viewID = viewID;
		// controls for the server to move their own buggy
		//myCart.AddComponent("movement");
		
		// add it to the list
		playerGameObjects.Add(myCart);

		// ANY SERVER SIDE SCRIPTS GO HERE
		//********************************************
		// receives player input and handles fiziks
		controlServer ms = gameObject.AddComponent("controlServer") as controlServer;
		// controls for the server to move their own buggy
		controlClient mc = gameObject.AddComponent("controlClient") as controlClient;
		mc.myViewID = viewID;
		mc.ms = ms;
		//********************************************
	}
	
	// Update is called once per frame
	void Update () {
		/*/ debug spawn thing
		if (GUILayout.Button ("Spawn something"))
		{
			NetworkViewID viewID = Network.AllocateViewID();
			networkView.RPC("NewGuyJoined", RPCMode.AllBuffered, viewID, new Vector3(0,5,0));
		}
		//*/
	}

	// fired when a player joins (if you couldn't tell)
	void OnPlayerConnected(NetworkPlayer player) {
		networkView.RPC("PrintText", player, "Welcome to the test server");
		PrintText("Someone joined");
		// send all current players to new guy
		foreach (GameObject playerGameObject in playerGameObjects)
		{
			networkView.RPC("SpawnPrefab", player, playerGameObject.networkView.viewID, playerGameObject.transform.position, new Vector3(0,0,0), "buggy1");
		}
	}
	void OnPlayerDisconnected(NetworkPlayer player) {
		// remove all their stuff
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}
	
	void OnGUI() {
		float keyToRemove = 0;
		// show any debug messages
		foreach (KeyValuePair<float,string> msgs in screenMessages) {
			if (msgs.Key < Time.time) {
				keyToRemove = msgs.Key;	// don't worry about there being more than 1 - it'll update next frame
			} else {
				GUILayout.BeginHorizontal();
				GUILayout.Label(msgs.Value);
				GUILayout.EndHorizontal();
			}
		}
		if (screenMessages.ContainsKey(keyToRemove)) screenMessages.Remove(keyToRemove);
	}




	// things that can be run over the network
	// debug text
	[RPC]
	void PrintText(string text) {
		Debug.Log(text);
		screenMessages.Add(Time.time+5,text);
	}
	
	// spawns a prefab
	[RPC]
	void SpawnPrefab(NetworkViewID IGNORED, Vector3 spawnLocation, Vector3 velocity, string prefabName) {
		Object prefab = Resources.Load(prefabName);
		// instantiate the prefab
		GameObject clone = Instantiate(prefab, spawnLocation, Quaternion.identity) as GameObject;
		// create and set viewID
		NetworkViewID viewID = Network.AllocateViewID();
		clone.networkView.viewID = viewID;
		// set velocity if we can
		if (clone.transform) clone.rigidbody.velocity = velocity;
		// tell everyone else about it
		networkView.RPC("SpawnPrefab", RPCMode.OthersBuffered, viewID, spawnLocation, velocity, prefabName);
	}

	[RPC]
	void GiveMeACart(NetworkMessageInfo info) {
		// create new buggy for the new guy - his must be done on the server otherwise collisions wont work!
		Vector3 spawnLocation = new Vector3(0,5,0);
		Vector3 velocity = new Vector3(0,0,0);
		// instantiate the prefab
		GameObject clone = Instantiate(Resources.Load("buggy1"), spawnLocation, Quaternion.identity) as GameObject;
		// create and set viewID
		NetworkViewID viewID = Network.AllocateViewID();
		clone.networkView.viewID = viewID;
		// tell everyone else about it
		networkView.RPC("SpawnPrefab", RPCMode.OthersBuffered, viewID, spawnLocation, velocity, "buggy1");
		// tell the player it's theirs
		networkView.RPC("ThisOnesYours", info.sender, viewID);
	}

	
	// blank for client use only
	[RPC]
	void ThisOnesYours(NetworkViewID viewID) {}
}
