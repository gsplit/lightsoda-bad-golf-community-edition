using UnityEngine;
using System.Collections;

public class netTest : MonoBehaviour {
	bool canHostServer = true;
	bool amIAServer = false;
	public GameObject buggyPrefab;
	public GameObject myCamera;
	ArrayList playerGameObjects = new ArrayList();

	// Use this for initialization
	void Start () {
		// change to custom master server
		MasterServer.ipAddress = "127.0.0.1";
		MasterServer.port = 23466;
		// get them servers
		MasterServer.ClearHostList();
		MasterServer.RequestHostList("HL3");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {
		// only show if not already in a game
		if (canHostServer) {
			if (GUILayout.Button ("Host a server"))
			{
				//amIAServer = true;
				// Use NAT punchthrough if no public IP present
				Network.InitializeServer(8, 11177, !Network.HavePublicAddress());
				MasterServer.RegisterHost("HL3", SystemInfo.deviceName, "Does it work yet?");

				// create server owners buggy
				GameObject clone = Instantiate(buggyPrefab, new Vector3(0,5,0), Quaternion.identity) as GameObject;
				NetworkViewID viewID = Network.AllocateViewID();
				clone.networkView.viewID = viewID;
				clone.rigidbody.useGravity = true;
				movement nms = clone.AddComponent("movement") as movement;
				nms.cameraGameObject = myCamera;

				// add it to the list
				playerGameObjects.Add(clone);
			}
		}
		if (GUILayout.Button ("Refresh server list"))
		{
			// get them servers
			MasterServer.ClearHostList();
			MasterServer.RequestHostList("HL3");
		}

		HostData[] data = MasterServer.PollHostList();
		// Go through all the hosts in the host list
		foreach (HostData element in data)
		{
			GUILayout.BeginHorizontal();	
			string name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
			GUILayout.Label(name);	
			GUILayout.Space(5);
			string hostInfo = "[";
			foreach (var host in element.ip)
				hostInfo = hostInfo + host + ":" + element.port + " ";
			hostInfo = hostInfo + "]";
			GUILayout.Label(hostInfo);	
			GUILayout.Space(5);
			GUILayout.Label(element.comment);
			GUILayout.Space(5);
			GUILayout.FlexibleSpace();
			if (!amIAServer) {
				if (GUILayout.Button("Connect"))
				{
					// Connect to HostData struct, internally the correct method is used (GUID when using NAT).
					Network.Connect(element);
					//canHostServer = false;
				}
			}
			GUILayout.EndHorizontal();
		}

		if (GUILayout.Button ("Spawn something"))
		{
			NetworkViewID viewID = Network.AllocateViewID();
			networkView.RPC("NewGuyJoined", RPCMode.AllBuffered, viewID, new Vector3(0,5,0));
		}
	}

	// fired by the server
	void OnPlayerConnected(NetworkPlayer player) {
		networkView.RPC("PrintText", player, "Welcome to the test server");
		foreach (GameObject playerGameObject in playerGameObjects)
		{
			networkView.RPC("NewGuyJoined", player, playerGameObject.networkView.viewID, playerGameObject.transform.position);
		}
		// this must be done on the server otherwise collisions wont work
		NetworkViewID viewID = Network.AllocateViewID();
		networkView.RPC("NewGuyJoined", RPCMode.AllBuffered, viewID, new Vector3(0,5,0));
		// tell the player it's theirs
		networkView.RPC("ThisOnesYours", player, viewID);
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}

	void OnConnectedToServer() {
	}

	// things that can be run over the network
	[RPC]
	void PrintText (string text) {
		Debug.Log(text);
	}
	// spawns a car for each player
	[RPC]
	void NewGuyJoined (NetworkViewID viewID, Vector3 spawnLocation) {
		GameObject clone = Instantiate(buggyPrefab, spawnLocation, Quaternion.identity) as GameObject;
		clone.networkView.viewID = viewID;
		clone.rigidbody.useGravity = true;
		if (Network.isServer) {
			clone.AddComponent("netMovement");
		}
	}
	// tells the player that this ones theirs
	[RPC]
	void ThisOnesYours (NetworkViewID viewID) {
		netMovement nms = NetworkView.Find(viewID).gameObject.AddComponent("netMovement") as netMovement;
		nms.cameraGameObject = myCamera;
		nms.isMine = true;
	}
}
