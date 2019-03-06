using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// If the game is online, this class overrides methods from Race Manager to manage specific 
	/// online parts.
	/// In case of local mode, this class is automatically disabled by UNET framework.
	/// </summary>
	public class NetworkRaceManager : NetworkBehaviour
	{
		[Header("Race Manager")]
		public RaceManager RaceManager;

		[Header("Playing Options")]
		[Information("By Default, we set the game with no collisions in network. This is because Unity Physics Engine is non-deterministic. This will cause interference between server and each client and provoques a bad experience for the players.\n", InformationAttribute.InformationType.Info, false)]
		/// Determines if collisions are active in network play
		public bool NoCollisions = true; 

		/// <summary>
		/// Initialization
		/// </summary>
		public override void OnStartServer()
		{
			base.OnStartServer();

			// Code from NetworkRaceManager is executed instead of RaceManager one
			RaceManager.OnDisableControlForPlayers = DisableControlForPlayers;
			RaceManager.OnEnableControlForPlayers = EnableControlForPlayers;
			RaceManager.OnUpdateCountdownText = UpdateCountdownText;
			RaceManager.OnShowEndGameScreen = ShowEndGameScreen;
			RaceManager.OnUpdatePlayersList = UpdatePlayersList;

			// We register end game backbutton callback
			RaceManager.BackToMenuButton.onClick.RemoveAllListeners();
			RaceManager.BackToMenuButton.onClick.AddListener(ReturnToMenu);

			RaceManager.StartGameCountdown.text ="WAITING FOR PLAYERS";
		}

		/// <summary>
		/// Describes what happens when the client starts
		/// </summary>
		public override void OnStartClient()
		{
			// We override racemanager value with the networkracemanager value
			RaceManager.NoCollisions = NoCollisions;
			RaceManager.UpdateNoPlayersCollisions();

			// We register backbutton callback
			RaceManager.BackButton.onClick.RemoveAllListeners();
			RaceManager.BackButton.onClick.AddListener(ReturnToMenu);

			// All players are instantiated, we can start the game from the server
			if (isServer)
			{
				StartCoroutine(ManagerStart());
			}
			else 
			{
				// We hide the back to menu button from end game panel
				RaceManager.BackToMenuButton.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Start Manager
		/// </summary>
		/// <returns>The start.</returns>
		public virtual IEnumerator ManagerStart()
		{
			// Manager will not start until each player has changed scene and is ready to play
			while (!OnlineLobbyManager.Instance.PlayersReadyToPlay)
			{
				yield return null;
			}
				
			// we disable players controls at start to let the race countdown run
			RaceManager.OnDisableControlForPlayers();

			// the Start Game Countdown must be 2 seconds at least
			// otherwise, the network doesn't have time to properly synchronize
			if (RaceManager.StartGameCountDownTime < 2)
			{
				Debug.LogWarning("StartGameCountDownTime was changed to 2 seconds (from "
				+ RaceManager.StartGameCountdown
				+ ". In network, StartGameCountDownTime must be at least 2 seconds.");
				RaceManager.StartGameCountDownTime = 2;
			}

			// we start the race countdown
			StartCoroutine(RaceManager.StartGameCountdownCoroutine());
		}

		/// <summary>
		/// return to lobby
		/// </summary>
		public virtual void ReturnToMenu() 
		{
			if (isServer)
			{
				OnlineLobbyManager.Instance.ReturnToLobby();
			}
			else
			{
				OnlineLobbyManager.Instance.SendReturnToLobby();
			}
		}

		/// <summary>
		/// Updates the countdown text. Call from server to clients
		/// </summary>
		/// <param name="text">Text of the countdown</param>
		[Server]
		protected virtual void UpdateCountdownText(string text)
		{
			RpcUpdateStartGameCountdown(text); 
		}

		/// <summary>
		/// Update of the countdown text in clients
		/// </summary>
		/// <param name="text">Text of the countdown</param>
		[ClientRpc]
		public virtual void RpcUpdateStartGameCountdown(string text)
		{
			// If text is empty, we hide the canvas GUI
			if (text == "")
			{
				RaceManager.StartGameCountdown.gameObject.SetActive(false);
			}
			else 
			{
				RaceManager.StartGameCountdown.text = text;
			}
		}

		/// <summary>
		/// Shows the end game screen. Call from server
		/// </summary>
		/// <param name="text">Text of the end of the game</param>
		[Server]
		protected virtual void ShowEndGameScreen(string text)
		{
			RpcShowEndGameScreen(text);
		}

		/// <summary>
		/// Shows the end game screen in client.
		/// </summary>
		/// <param name="text">Text of the end of the game</param>
		[ClientRpc]
		public virtual void RpcShowEndGameScreen(string text)
		{
			RaceManager.EndGameRanking.text = text;
			RaceManager.EndGamePanel.gameObject.SetActive(true);
		}

		/// <summary>
		/// Enables the control for players.
		/// </summary>
		[Server]
		public virtual void EnableControlForPlayers()
		{
			foreach(var c in NetworkServer.connections)
			{
				NetworkVehicleController car = c.playerControllers[0].gameObject.GetComponent<NetworkVehicleController>();
				if (car != null)
				{
					car.RpcEnableControl();
				}
			}
		}

		/// <summary>
		/// Disables the control for players.
		/// </summary>
		[Server]
		public virtual void DisableControlForPlayers()
		{
			foreach(var c in NetworkServer.connections)
			{
				NetworkVehicleController car = c.playerControllers[0].gameObject.GetComponent<NetworkVehicleController>();
				if (car != null)
				{
					car.RpcDisableControl();
				}
			}
		}

		/// <summary>
		/// Returns the players list actually playing game
		/// </summary>
		/// <returns>The players list.</returns>
		[Server]
		protected virtual List<BaseController> UpdatePlayersList()
		{
			List<BaseController> cars = new List<BaseController>();
			foreach(var c in NetworkServer.connections)
			{
				if (c != null)
				{
					var controller = c.playerControllers[0];
					if (controller != null && controller.gameObject != null)
					{
						BaseController car = controller.gameObject.GetComponent<BaseController>();
						if (car != null)
						{
							cars.Add(car);
						}
					}
				}
			}

			return cars;
		}
	}
}