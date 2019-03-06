using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// This class manages the player state in the lobby.
	/// </summary>
	public class OnlineLobbyPlayer : NetworkLobbyPlayer, ILobbyPlayerInfo
	{
		#region ILobbyPlayerInfo implementation

		public int Position { get; set; }

		public string Name { get; set; }

		public string VehicleName { get; set; }

		public int VehicleSelectedIndex { get; set; }

		public bool IsBot { get; set; }

		#endregion

		[Header("GUI Elements")]
		public Text PlayerName;
		public Button LeftButton;
		public Button RightButton;
		public Button ReadyButton;
		public Text VehicleText;
		public Image VehicleImage;
		public Sprite ReadyButtonSprite;
		public Sprite CancelReadyButtonSprite;

		[SyncVar(hook = "OnVehicleChanged")]
		protected int _currentVehicleSelected; // index of the current selected vehicle
		[SyncVar(hook = "OnCurrentTrackChanged")]
		protected int _currentTrackSelected; // index of the current track selected.
		[SyncVar(hook = "OnPlayerNameChanged")]
		protected string _playerName; // The name of the player.
		protected Vector3 elementPosition = Vector3.zero;
		protected RectTransform _rectTransform;


		/// <summary>
		/// Gets the currently selected track.
		/// </summary>
		/// <value>The track selected.</value>
		public virtual int TrackSelected 
		{
			get { return _currentTrackSelected; }
		}

		/// <summary>
		/// We use this event to initialize object properties and gui.
		/// This is called when a client connects for the first time and when a client goes back to the lobby after a race.
		/// </summary>
		public override void OnClientEnterLobby()
		{
			base.OnClientEnterLobby();

			// In case the lobby is not shown, when going back from the game screen, we show the lobby gui
			if (!OnlineLobbyManager.Instance._onlineLobbyUI.Lobby.gameObject.activeSelf)
			{
				// We change to connected mode
				OnlineLobbyManager.Instance._onlineLobbyUI.ShowLobby();
				OnlineLobbyManager.Instance._onlineLobbyUI.ShowConnected();
				OnlineLobbyManager.Instance._onlineLobbyUI.HidePopup();
			}

			// we look for the associated player gui zone
			RectTransform slotZone = OnlineLobbyManager.Instance._onlineLobbyUI.PlayersSelection[slot];

			if (slotZone != null)
			{
				_rectTransform = GetComponent<RectTransform>();

				_rectTransform.SetParent(slotZone.transform, false);
				_rectTransform.anchoredPosition = Vector2.zero;

				if (elementPosition == Vector3.zero)
				{
					// we store the initial and correct position for future updates
					elementPosition = _rectTransform.position;
				}

				LeftButton.gameObject.SetActive(false);
				RightButton.gameObject.SetActive(false);
				ReadyButton.interactable = false;

				// the last level played is selected by default
				if (slot == 0)
				{
					OnCurrentTrackChanged(_currentTrackSelected);
				}

				InitUI();

				ShowSelectedVehicle();
			} 
			else
			{
				Debug.LogWarning("Zone UI for player is missing. Ensure Lobby is visible & in connected mode");
			}

			// we ask the LobbyManager to refresh ready state
			OnlineLobbyManager.Instance.OnLobbyServerPlayersReady();
		}

		/// <summary>
		/// when a player first joins the server, we use this call to reset values
		/// </summary>
		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();

			// we first initialize the player's name and attributes if it's empty
			if (string.IsNullOrEmpty(Name))
			{
				Name = "Player #" + (slot + 1);
				CmdUpdatePlayerName(Name);


				Position = 0;
				_currentVehicleSelected = 0;
			}

			InitUI();
		}

		/// <summary>
		/// Sends a new player's name to the server
		/// </summary>
		/// <param name="name">Name.</param>
		[Command]
		protected virtual void CmdUpdatePlayerName(string name)
		{
			_playerName = name;
		}

		/// <summary>
		/// Updates the player's name in the textbox
		/// </summary>
		/// <param name="name">Name.</param>
		protected virtual void OnPlayerNameChanged(string name)
		{
			PlayerName.text = name;
		}

		/// <summary>
		/// We need to anchor the GameObject's position otherwise when going back from game to lobby,
		/// the object would be moved to an incorrect position.
		/// </summary>
		protected virtual void Update()
		{
			if (transform.position != elementPosition)
			{
				transform.position = elementPosition;
			}
		}

		/// <summary>
		/// Registers the available vehicles
		/// </summary>
		public override void OnStartClient()
		{
			base.OnStartClient();

			foreach (var prefab in OnlineLobbyManager.Instance.AvailableVehiclesPrefab)
			{
				ClientScene.RegisterPrefab(prefab);
			}
		}
			
		/// <summary>
		/// Initializes the UI
		/// </summary>
		protected virtual void InitUI()
		{
			if (isLocalPlayer)
			{
				LeftButton.gameObject.SetActive(true);
				RightButton.gameObject.SetActive(true);
				LeftButton.onClick.RemoveAllListeners();
				LeftButton.onClick.AddListener(OnLeftButton);
				RightButton.onClick.RemoveAllListeners();
				RightButton.onClick.AddListener(OnRightButton);
				ReadyButton.onClick.RemoveAllListeners();
				ReadyButton.onClick.AddListener(OnReady);
				ReadyButton.interactable = true;
				OnClientReady(false);

				if (slot == 0)
				{
					OnlineLobbyManager.Instance._onlineLobbyUI.ShowTrackSelection(this);
				}
			}
			else 
			{
				LeftButton.gameObject.SetActive(false);
				RightButton.gameObject.SetActive(false);
				ReadyButton.interactable = false;
				UpdateReadyButtonText(false);
			}
		}

		/// <summary>
		/// Describes what happens when pressing the left button
		/// </summary>
		public virtual void OnLeftButton()
		{
			OnVehicleChange(-1);
		}

		/// <summary>
		/// Describes what happens when pressing the right button
		/// </summary>
		public virtual void OnRightButton()
		{
			OnVehicleChange(1);
		}

		/// <summary>
		/// Changes the current player vehicle
		/// </summary>
		/// <param name="direction">Direction.</param>
		public virtual void OnVehicleChange(int direction)
		{
			int newVehicleSelected = _currentVehicleSelected + direction;

			if (newVehicleSelected < 0)
			{
				newVehicleSelected = OnlineLobbyManager.Instance.AvailableVehiclesPrefab.Length - 1;
			} 
			else if (newVehicleSelected > (OnlineLobbyManager.Instance.AvailableVehiclesPrefab.Length - 1))
			{
				newVehicleSelected = 0;
			} 

			CmdVehicleSelected(newVehicleSelected);
		}

		/// <summary>
		/// Command Action when vehicle is chosen
		/// </summary>
		/// <param name="newVehicleSelected">New vehicle selected.</param>
		[Command]
		public virtual void CmdVehicleSelected(int newVehicleSelected) 
		{
			_currentVehicleSelected = newVehicleSelected;
		}

		/// <summary>
		/// Describes what happens when the vehicle changes
		/// </summary>
		/// <param name="value">Value.</param>
		public virtual void OnVehicleChanged(int value)
		{
			_currentVehicleSelected = value;
			VehicleSelectedIndex = value;

			ShowSelectedVehicle();
		}

		/// <summary>
		/// Shows the selected vehicle.
		/// </summary>
		protected virtual void ShowSelectedVehicle()
		{
			VehicleInformation info = OnlineLobbyManager.Instance.AvailableVehiclesPrefab[_currentVehicleSelected].GetComponent<VehicleInformation>();

			VehicleText.text = info.LobbyName;
			VehicleImage.sprite = info.lobbySprite;

			// Server stores vehicle name of object instantiation
			if (isServer)
			{
				VehicleName = VehicleText.text;
			}
		}

		/// <summary>
		/// Describes what happens when the track changes
		/// </summary>
		/// <param name="direction">Direction.</param>
		public virtual void OnTrackChange(int direction)
		{
			int newvalue = _currentTrackSelected + direction;

			if (newvalue < 0)
			{
				newvalue = OnlineLobbyManager.Instance.AvailableTracksSceneName.Length - 1;
			}
			else if (newvalue >= OnlineLobbyManager.Instance.AvailableTracksSceneName.Length) 
			{
				newvalue = 0;
			}

			_currentTrackSelected = newvalue;
		}

		/// <summary>
		/// Describes what happens when the current track gets changed
		/// </summary>
		/// <param name="value">Value.</param>
		public virtual void OnCurrentTrackChanged(int value)
		{
			_currentTrackSelected = value;
			OnlineLobbyManager.Instance._onlineLobbyUI.UpdateTrackInfo(_currentTrackSelected);
		}

		/// <summary>
		/// Describes what happens when the player chooses the "READY" action in GUI
		/// </summary>
		public virtual void OnReady()
		{
			if (readyToBegin)
			{
				SendNotReadyToBeginMessage();
			} 
			else
			{
				SendReadyToBeginMessage();
			}
		}

		/// <summary>
		/// When player is ready, READY button is disabled.
		/// </summary>
		/// <param name="readyState">If set to <c>true</c> ready state.</param>
		public override void OnClientReady(bool readyState)
		{
			base.OnClientReady(readyState);

			if (readyState)
			{
				UpdateReadyButtonText(true);
				ReadyButton.interactable = false;
				LeftButton.gameObject.SetActive(false);
				RightButton.gameObject.SetActive(false);
			} 
			else
			{
				UpdateReadyButtonText(false);
				ReadyButton.interactable = true;
				LeftButton.gameObject.SetActive(true);
				RightButton.gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// Updates the ready button text.
		/// </summary>
		/// <param name="ready">If set to <c>true</c> ready.</param>
		protected virtual void UpdateReadyButtonText(bool ready)
		{
			Text buttonText = ReadyButton.GetComponentInChildren<Text>();

			if (ready)
			{
				buttonText.text = "- READY -";
				ReadyButton.image.sprite = CancelReadyButtonSprite;
			}
			else
			{
				buttonText.text = "READY ?";
				ReadyButton.image.sprite = ReadyButtonSprite;
			}
		}

		/// <summary>
		/// Rpc Call used to show popup loading scren before race scene is loaded.
		/// </summary>
		/// <param name="scenename">Scenename.</param>
		[ClientRpc]
		public virtual void RpcOnStartGame(string scenename)
		{
			OnlineLobbyManager.Instance._onlineLobbyUI.ShowPopup("Loading " + scenename + "...");
		}
	}
}
