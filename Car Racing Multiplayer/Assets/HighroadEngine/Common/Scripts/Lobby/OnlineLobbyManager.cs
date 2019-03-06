using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking.Types;

namespace MoreMountains.HighroadEngine 
{
	/// <summary>
	/// Online lobby manager. Manages UNET network integration with server creation / joining and lobby players.
	/// It's also in charge of communication to the network race manager (players data & scene selection)
	/// </summary>
	public class OnlineLobbyManager : NetworkLobbyManager, IGenericLobbyManager 
	{
		/// reference to static instance.
		static public OnlineLobbyManager Instance;

		[Header("GUI")]
		/// reference to OnlineLobbyGUI child GameObject
		public OnlineLobbyUI _onlineLobbyUI;

		[Header("Vehicles configuration")]
		/// the list of vehicles prefabs the player can choose from.
		public GameObject[] AvailableVehiclesPrefab; 

		[Header("Tracks configuration")]
		/// the list of Track Scenes names. Used to load scene & show scene name in UI
		public string[] AvailableTracksSceneName; 
		/// the list of tracks sprites. Used to show image of chosen track in UI
		public Sprite[] AvailableTracksSprite; 

		[Header("Matchmaking")]
		[Information("Set a unique id (number). This will be used to separate online rooms between each type of game.\n", InformationAttribute.InformationType.Info, false)]
		/// the unique online game identifier.
		public int GameId = 0;

		protected ulong _matchId; // The match identifier in matchmaking
		protected ulong _nodeId;
		protected string _matchName;
		protected bool _matchServer = false;
		protected bool _disconnectServer = false;
		protected int _playersReadyToPlayCount;
		protected int _currentStartPosition = 0;
		protected bool _destroyInstance = false;


		/// <summary>
		/// Gets or sets a value indicating whether players are ready to play.
		/// We use this value to delay the start race countdown
		/// </summary>
		/// <value><c>true</c> if players ready to play; otherwise, <c>false</c>.</value>
		public virtual bool PlayersReadyToPlay {get; protected set;}

		/// <summary>
		/// Initializes the manager
		/// </summary>
		public virtual void Start() 
		{
			Instance = this;

			_onlineLobbyUI = GetComponentInChildren<OnlineLobbyUI>();

			// Init UI
			_onlineLobbyUI.ShowLobby();
			OnReturnToMain();

			// Register call on scene loaded to destroy this object
			SceneManager.sceneLoaded += SceneManager_sceneLoaded; 
		}
			
		/// <summary>
		/// We use this callback to instantiate player vehicle for the game scene.
		/// This instantiated object is then positionned on the track scene
		/// </summary>
		/// <param name="conn">Conn.</param>
		/// <param name="playerControllerId">Player controller identifier.</param>
		public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
		{
			GameObject vehicle = null;

			// we look for a reference to generic interface of the racing manager class
			RaceManager manager = FindObjectOfType<RaceManager>();

			// we get the associated lobby player object 
			GameObject newPlayer = conn.playerControllers[playerControllerId].gameObject;
			OnlineLobbyPlayer lobbyplayer = newPlayer.GetComponent<OnlineLobbyPlayer>();

			// we determine the start position
			Vector3 startPosition = manager.StartPositions[_currentStartPosition];
			Quaternion startRotation = Quaternion.Euler(new Vector3(0, manager.StartAngleDegree, 0));

			// we increment start position for next player
			_currentStartPosition++;

			// we instantiate the player vehicle
			vehicle = (GameObject)Object.Instantiate(
				AvailableVehiclesPrefab[lobbyplayer.VehicleSelectedIndex], 
				startPosition, 
				startRotation
			);

			return vehicle;
		}

		/// <summary>
		/// This event allow us to attribute lobby player values to the game player object.
		/// We also count the number of players ready at this stage.
		/// </summary>
		/// <param name="lobbyPlayer">Lobby player.</param>
		/// <param name="gamePlayer">Game player.</param>
		public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
		{
			OnlineLobbyPlayer onlinelobbyplayer = lobbyPlayer.GetComponent<OnlineLobbyPlayer>();
			INetworkVehicle vehicleController = gamePlayer.GetComponent<INetworkVehicle>();

			vehicleController.SetPlayerName(onlinelobbyplayer.PlayerName.text);
			gamePlayer.name = onlinelobbyplayer.PlayerName.text;

			_playersReadyToPlayCount++;

			// When all players are at this stage, game can start
			if (_playersReadyToPlayCount == numPlayers)
			{
				PlayersReadyToPlay = true;
			}

			return true;
		}

		/// <summary>
		/// We use this event to show or hide lobby (as lobby is a persistent object)
		/// </summary>
		/// <param name="conn">Conn.</param>
		public override void OnLobbyClientSceneChanged(NetworkConnection conn)
		{
			string newscene = SceneManager.GetSceneAt(0).name;

			if (newscene == lobbyScene)
			{
				// we go back to lobby scene
				_onlineLobbyUI.ShowLobby();
			}
			else if (System.Array.IndexOf(AvailableTracksSceneName, newscene) > -1)
			{
				// if the currently loaded scene is an available track scene
				_onlineLobbyUI.HideLobby();
			}
			else 
			{
				// we are going back to the start screen or another unknown scene, we kill the LobbyManager
				Destroy(gameObject);
			}
		}


		/// <summary>
		/// Updates the wait for players text in the lobby screen.
		/// </summary>
		public virtual void Update()
		{
			if (SceneManager.GetSceneAt(0).name == lobbyScene)
			{
				int playersTotal = 0;
				int playersReady = 0;

				foreach (var lobbyplayer in lobbySlots)
				{
					if (lobbyplayer != null)
					{
						playersTotal++;

						if (lobbyplayer.readyToBegin)
						{
							playersReady++;
						}
					}
				}

				if (playersTotal > 0)
				{
					_onlineLobbyUI.UpdateWaitPlayersText(playersReady, playersTotal);
				}
			}
		}

		/// <summary>
		/// we use this event to activate or hide start game button
		/// </summary>
		public override void OnLobbyServerPlayersReady() 
		{
			var ready = true;

			// ready value is the sum of lobby players ready state
			foreach (var lobbyplayer in lobbySlots)
			{
				if (lobbyplayer != null)
				{
					ready &= lobbyplayer.readyToBegin;
				}
			}

			if (ready)
			{
				_onlineLobbyUI.ShowStartGame();
			}
			else
			{
				_onlineLobbyUI.HideStartGame();
			}
		}
			
		/// <summary>
		/// Describes what happens when the server player clicks on the start game button
		/// </summary>
		public virtual void OnStartGame()
		{
			// we get the current track choice
			var serverPlayer = lobbySlots[0];
			var onlinelobbyplayer = serverPlayer.GetComponent<OnlineLobbyPlayer>();

			PlayersReadyToPlay = false;
			_playersReadyToPlayCount = 0;
			_currentStartPosition = 0;

			string trackSceneName = AvailableTracksSceneName[onlinelobbyplayer.TrackSelected];

			// we send a message to each client telling them that the game will start soon
			foreach(var player in lobbySlots)
			{
				if (player != null)
				{
					var lobbyplayer = player.GetComponent<OnlineLobbyPlayer>();
					if (lobbyplayer != null)
					{
						lobbyplayer.RpcOnStartGame(trackSceneName);
					}
				}
			}
			ServerChangeScene(trackSceneName);
		}

		#region main actions

		/// <summary>
		/// Describes what happens when the player clicks on the Match Making button
		/// </summary>
		public virtual void OnMatchmaking()
		{
			// Update UI
			_onlineLobbyUI.ShowMatchmaking();
		
			InitMatchmaking();
		}

		/// <summary>
		/// Describes what happens when the player clicks on the Direct Connection button
		/// </summary>
		public virtual void OnDirectConnection()
		{
			// Update UI
			_onlineLobbyUI.ShowDirectConnection();
		}

		/// <summary>
		/// Describes what happens when the player clicks on the main button
		/// </summary>
		public virtual void OnReturnToMain()
		{
			// If matchmaking was active, we stop it
			if (matchMaker != null)
			{
				StopMatchMaker();
			}

			_onlineLobbyUI.ShowLobby();
			_onlineLobbyUI.ShowMainMenu();
		}

		#endregion

		#region matchmaking

		/// <summary>
		/// Initializes the matchmaking connection.
		/// </summary>
		public virtual void InitMatchmaking()
		{
			StartMatchMaker();
		}

		/// <summary>
		/// Describes what happens when the player creates a new matchmaking game
		/// </summary>
		public virtual void OnClickCreateMatchmakingGame()
		{
			if (matchMaker == null)
			{
				Debug.LogError("MatchMaker object should be initialized");
			}

			// Match name is randomly generated
			_matchName = "#" + Random.Range(1, 1000);

			matchMaker.CreateMatch(
				_matchName, 
				(uint) this.MaxPlayers,
				true,
				"",
				"",
				"",
				0,
				GameId,
				this.OnMatchCreate);
		}

		/// <summary>
		/// Describes what happens when the server list gets refreshed
		/// </summary>
		public virtual void OnClickRefreshServerList()
		{
			// we remove the currently shown matches
			_onlineLobbyUI.RemoveMatchesFromMatchmakingList();

			// we display a popup to inform the user
			_onlineLobbyUI.ShowPopup("Refresh online matches...");

			if (matchMaker == null)
			{
				Debug.LogError("MatchMaker object should be initialized");
			}

			// we list the 20 first matches
			matchMaker.ListMatches(0, 20, "", true, 0, GameId, OnMatchList);
		}

		/// <summary>
		/// Callback when a list of matches has been received from matchmaking server
		/// </summary>
		/// <param name="success">If set to <c>true</c> success.</param>
		/// <param name="extendedInfo">Extended info.</param>
		/// <param name="matchList">Match list.</param>
		public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
		{
			base.OnMatchList(success, extendedInfo, matchList);

			// Hiding popup
			_onlineLobbyUI.HidePopup();

			// For each match, we show its button
			if (success)
			{
				if (matchList.Count > 0)
				{
					_onlineLobbyUI.ShowMatchesFromMatchmaking(matchList);
				}
			}
		}

		/// <summary>
		/// Callback when a match has been created.
		/// </summary>
		/// <param name="success">If set to <c>true</c> success.</param>
		/// <param name="extendedInfo">Extended info.</param>
		/// <param name="matchInfo">Match info.</param>
		public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
		{
			base.OnMatchCreate(success, extendedInfo, matchInfo);
			_matchId = (System.UInt64) matchInfo.networkId;
			_onlineLobbyUI.TitleLabel.text = "GAME " + _matchName;
		}

		/// <summary>
		/// Callback when a match has been joined.
		/// </summary>
		/// <param name="success">If set to <c>true</c> success.</param>
		/// <param name="extendedInfo">Extended info.</param>
		/// <param name="matchInfo">Match info.</param>
		public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
		{
			base.OnMatchJoined(success, extendedInfo, matchInfo);
			_matchId = (System.UInt64) matchInfo.networkId;
			_nodeId = (System.UInt64) matchInfo.nodeId;
			_matchServer = false;
		}

		#endregion

		#region direct connection

		/// <summary>
		/// Start host action
		/// </summary>
		public virtual void OnClickStartHost()
		{
			StartHost();
		}

		/// <summary>
		/// When the user clicks Join Server with ip adress
		/// </summary>
		/// <param name="ipadress">Ipadress.</param>
		public virtual void OnClickJoinServer(string ipadress)
		{
			networkAddress = ipadress;
			StartClient();
		}

		#endregion

		#region connected 

		/// <summary>
		/// Back button from connected canvas
		/// </summary>
		public virtual void OnConnectedReturnToMain()
		{
			// if matchmaking, we destroy current match
			if (matchMaker != null)
			{
				matchMaker.DropConnection((NetworkID)_matchId, (NodeID) _nodeId, 0, OnDropConnection);

				if (_matchServer) 
				{
					matchMaker.DestroyMatch((NetworkID) _matchId, 0, OnDestroyMatch);
				}

				_disconnectServer = true;
			}
				
			StopHost();
		}

		/// <summary>
		/// When online match has been destroyed, we clean matchmaking and host process
		/// </summary>
		/// <param name="success">If set to <c>true</c> success.</param>
		/// <param name="extendedInfo">Extended info.</param>
		public override void OnDestroyMatch(bool success, string extendedInfo)
		{
			if (_disconnectServer)
			{
				StopMatchMaker();
				StopHost();
			}
		}

		/// <summary>
		/// Describes what happens when the client tries to connect to the server
		/// </summary>
		/// <param name="lobbyClient">Lobby client.</param>
		public override void OnLobbyStartClient(NetworkClient lobbyClient)
		{
			_onlineLobbyUI.ShowPopup("Connecting...");
		}

		/// <summary>
		/// Describes what happens when the client is connected to the server
		/// </summary>
		/// <param name="conn">Conn.</param>
		public override void OnLobbyClientConnect(NetworkConnection conn)
		{
			base.OnLobbyClientConnect(conn);

			// Updates UI
			_onlineLobbyUI.ShowConnected();
			_onlineLobbyUI.HidePopup();
		}

		/// <summary>
		/// Raised when client exists scene.
		/// If client is still in lobby, we go back to main menu. Else, we don't do anything.
		/// </summary>
		public override void OnLobbyClientExit()
		{
			base.OnLobbyClientExit();

			if (SceneManager.GetSceneAt(0).name == lobbyScene)
			{
				if (_onlineLobbyUI.ConnectedCanvas.gameObject.activeSelf)
				{
					OnReturnToMain();
				}
			}
		}

		/// <summary>
		/// Raised when client disconnects server
		/// </summary>
		/// <param name="conn">Conn.</param>
		public override void OnLobbyClientDisconnect(NetworkConnection conn)
		{
			base.OnLobbyClientDisconnect(conn);

			// We were online, we go back to main menu
			if (_onlineLobbyUI.ConnectedCanvas.gameObject.activeSelf)
			{
				// We return to main menu
				_onlineLobbyUI.ShowPopup("Connection to server lost", "CONTINUE", _onlineLobbyUI.HidePopup);
				OnReturnToMain();
			}
			else
			{
				// We weren't online, we just popup error info
				_onlineLobbyUI.ShowPopup("Could not connect to server", "CONTINUE", _onlineLobbyUI.HidePopup);
			}
		}

		/// <summary>
		/// Raised when client has network error
		/// </summary>
		/// <param name="conn">Conn.</param>
		/// <param name="errorCode">Error code.</param>
		public override void OnClientError(NetworkConnection conn, int errorCode)
		{
			_onlineLobbyUI.ShowPopup("Could not join server", "CONTINUE", _onlineLobbyUI.HidePopup);
		}
			
		#endregion

		#region GenericLobbyManager implementation

		/// <summary>
		/// Returns Players
		/// </summary>
		public virtual Dictionary<int, ILobbyPlayerInfo> Players()
		{
			Dictionary<int, ILobbyPlayerInfo> players = new Dictionary<int, ILobbyPlayerInfo>();

			foreach(var lobbySlot in lobbySlots) {
				if( lobbySlot != null) {
					OnlineLobbyPlayer lobbyPlayer = (OnlineLobbyPlayer) lobbySlot;
					players.Add(lobbyPlayer.Position, lobbyPlayer);
				}
			}

			return players;
		}

		/// <summary>
		/// Gets the player.
		/// </summary>
		/// <returns>The player.</returns>
		/// <param name="position">Position.</param>
		public virtual ILobbyPlayerInfo GetPlayer (int position)
		{
			foreach(var lobbySlot in lobbySlots) {
				OnlineLobbyPlayer lobbyPlayer = (OnlineLobbyPlayer) lobbySlot;
				if (lobbyPlayer.Position == position) {
					return lobbyPlayer;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the maximum number of players allowed
		/// </summary>
		/// <value>The max players.</value>
		public virtual int MaxPlayers 
		{
			get 
			{
				return maxPlayers;
			}
		}

		/// <summary>
		/// Changes the current scene to lobby scene as server
		/// </summary>
		public virtual void ReturnToLobby()
		{
			ServerReturnToLobby();
		}

		/// <summary>
		/// Changes the current scene to the start screen.
		/// </summary>
		public virtual void ReturnToStartScreen()
		{
			_destroyInstance = true;
			LoadingSceneManager.LoadScene("StartScreen");
		}

		#endregion

		/// <summary>
		/// We use this event to destroy this object when the scene has changed and the instance must be destroyed
		/// </summary>
		/// <param name="scene">Scene.</param>
		/// <param name="mode">Mode.</param>
		protected virtual void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (_destroyInstance)
			{
				SceneManager.sceneLoaded -= SceneManager_sceneLoaded; 
				if (gameObject != null) 
				{
					Destroy(gameObject);
				}
			}
		}
	}
}
