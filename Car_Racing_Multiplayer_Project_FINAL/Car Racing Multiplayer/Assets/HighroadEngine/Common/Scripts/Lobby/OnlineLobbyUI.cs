using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking.Match;
using UnityEngine.Events;
using System.Net;

namespace MoreMountains.HighroadEngine
{	
	/// <summary>
	/// References to the various UI elements in the online lobby scene
	/// </summary>
	public class OnlineLobbyUI : MonoBehaviour
	{
		[Header("Background")]
		public Image BackgroundImage;

		[Header("Main Panel")]
		public RectTransform MainCanvas;
		public RectTransform MatchmakingCanvas;
		public Button MatchmakingButton;
		public RectTransform DirectConnectionCanvas;
		public Button DirectConnectionButton;
		public Button BackButton;

		[Header("Matchmaking Panel")]
		public GameObject OnlineLobbyMatchEntryPrefab;
		public RectTransform OnlineMatchesCanvas;
		public Button RefreshOnlineMatchesButton;
		public Button CreateMatchButton;
		public Button MMReturnToMainButton;

		[Header("Direct connection Panel")]
		public InputField ServerAdressInput;
		public Button JoinServerButton;
		public Button StartServerButton;
		public Button DirectReturnToMainButton;

		[Header("Connected Panel")]
		public RectTransform ConnectedCanvas;
		public Button OnlineReturnToMainButton;
		public RectTransform Lobby;
		public Text TitleLabel;
		public Button LeftTrackButton;
		public Button RightTrackButton;
		public Image TrackImage;
		public Text TrackNameText;
		public Button StartGameButton;
		public RectTransform[] PlayersSelection;
		public Text WaitPlayersText;


		[Header("Popup Panel")]
		public RectTransform PopupCanvas;
		public Text PopupText;
		public Button PopupActionButton;
		public Text PopupActionButtonText;

		[Header("In Game")]
		public RectTransform InGameCanvas;
		public Button ReturnToLobbyButton;

		#region main

		/// <summary>
		/// On Start, we hide the popup
		/// </summary>
		public virtual void Start()
		{
			PopupCanvas.gameObject.SetActive(false);
			//ServerAdressInput.text = Network.player.ipAddress;
		}

		/// <summary>
		/// Shows the main menu.
		/// </summary>
		public virtual void ShowMainMenu()
		{
			MatchmakingCanvas.gameObject.SetActive(false);
			DirectConnectionCanvas.gameObject.SetActive(false);
			ConnectedCanvas.gameObject.SetActive(false);
			InGameCanvas.gameObject.SetActive(false);

			MainCanvas.gameObject.SetActive(true);

			MatchmakingButton.onClick.RemoveAllListeners();
			MatchmakingButton.onClick.AddListener(OnlineLobbyManager.Instance.OnMatchmaking);

			DirectConnectionButton.onClick.RemoveAllListeners();
			DirectConnectionButton.onClick.AddListener(OnlineLobbyManager.Instance.OnDirectConnection);

			BackButton.onClick.RemoveAllListeners();
			BackButton.onClick.AddListener(OnlineLobbyManager.Instance.ReturnToStartScreen);
		}

		/// <summary>
		/// Shows the lobby UI
		/// </summary>
		public virtual void ShowLobby()
		{
			Lobby.gameObject.SetActive(true);
		}

		/// <summary>
		/// Hides the lobby UI
		/// </summary>
		public virtual void HideLobby()
		{
			Lobby.gameObject.SetActive(false);
		}
			
		#endregion

		#region matchmaking

		/// <summary>
		/// Shows the matchmaking canvas
		/// </summary>
		public virtual void ShowMatchmaking()
		{
			MainCanvas.gameObject.SetActive(false);
			MatchmakingCanvas.gameObject.SetActive(true);
			DirectConnectionCanvas.gameObject.SetActive(false);
			ConnectedCanvas.gameObject.SetActive(false);

			RefreshOnlineMatchesButton.onClick.RemoveAllListeners();
			RefreshOnlineMatchesButton.onClick.AddListener(OnlineLobbyManager.Instance.OnClickRefreshServerList);

			CreateMatchButton.onClick.RemoveAllListeners();
			CreateMatchButton.onClick.AddListener(OnlineLobbyManager.Instance.OnClickCreateMatchmakingGame);

			MMReturnToMainButton.onClick.RemoveAllListeners();
			MMReturnToMainButton.onClick.AddListener(OnlineLobbyManager.Instance.OnReturnToMain);
		}

		/// <summary>
		/// Removes the matches from matchmaking list.
		/// </summary>
		public virtual void RemoveMatchesFromMatchmakingList()
		{
			// We remove match list
			foreach (Transform match in OnlineMatchesCanvas.transform)
			{
				// We keep refresh button
				if (match.gameObject != RefreshOnlineMatchesButton.gameObject)
				{
					Destroy(match.gameObject);
				}
			}
		}

		/// <summary>
		/// Shows the matches from matchmaking server call
		/// </summary>
		/// <param name="matches">Matches.</param>
		public virtual void ShowMatchesFromMatchmaking(List<MatchInfoSnapshot> matches)
		{
			for(int i = 0; i < matches.Count; i++)
			{
				GameObject entry = Instantiate(OnlineLobbyMatchEntryPrefab);

				entry.GetComponent<OnlineLobbyMatchEntry>().Init(matches[i], OnlineLobbyManager.Instance);
				entry.transform.SetParent(OnlineMatchesCanvas.transform, false);
			}
		}

		#endregion

		#region direct connection

		/// <summary>
		/// Shows the direct connection canvas
		/// </summary>
		public virtual void ShowDirectConnection()
		{
			MainCanvas.gameObject.SetActive(false);
			MatchmakingCanvas.gameObject.SetActive(false);
			DirectConnectionCanvas.gameObject.SetActive(true);
			ConnectedCanvas.gameObject.SetActive(false);

			JoinServerButton.onClick.RemoveAllListeners();
			JoinServerButton.onClick.AddListener(
				delegate {OnlineLobbyManager.Instance.OnClickJoinServer(ServerAdressInput.text);}
			);

			StartServerButton.onClick.RemoveAllListeners();
			StartServerButton.onClick.AddListener(OnlineLobbyManager.Instance.OnClickStartHost);

			DirectReturnToMainButton.onClick.RemoveAllListeners();
			DirectReturnToMainButton.onClick.AddListener(OnlineLobbyManager.Instance.OnReturnToMain);
		}

		#endregion

		#region connected 

		/// <summary>
		/// Shows the online canvas
		/// </summary>
		public virtual void ShowConnected()
		{
			ConnectedCanvas.gameObject.SetActive(true);
			MainCanvas.gameObject.SetActive(false);
			MatchmakingCanvas.gameObject.SetActive(false);
			DirectConnectionCanvas.gameObject.SetActive(false);

			LeftTrackButton.gameObject.SetActive(false);
			RightTrackButton.gameObject.SetActive(false);

			StartGameButton.gameObject.SetActive(false);
			WaitPlayersText.text = "";

			OnlineReturnToMainButton.onClick.RemoveAllListeners();
			OnlineReturnToMainButton.onClick.AddListener(OnlineLobbyManager.Instance.OnConnectedReturnToMain);
		}

		/// <summary>
		/// Shows the track selection canvas
		/// </summary>
		/// <param name="player">Reference to the Player 1 (owner of track selection)</param>
		public virtual void ShowTrackSelection(OnlineLobbyPlayer player)
		{
			LeftTrackButton.gameObject.SetActive(true);
			RightTrackButton.gameObject.SetActive(true);	

			LeftTrackButton.onClick.RemoveAllListeners();
			LeftTrackButton.onClick.AddListener(
				delegate {player.OnTrackChange(-1);
				});

			RightTrackButton.onClick.RemoveAllListeners();
			RightTrackButton.onClick.AddListener(
				delegate {player.OnTrackChange(1);
				});
		}

		/// <summary>
		/// Updates the track info.
		/// </summary>
		/// <param name="currentTrack">Current track.</param>
		public virtual void UpdateTrackInfo(int currentTrack)
		{
			TrackImage.sprite = OnlineLobbyManager.Instance.AvailableTracksSprite[currentTrack];
			TrackNameText.text = OnlineLobbyManager.Instance.AvailableTracksSceneName[currentTrack];	
		}

		/// <summary>
		/// Shows the start game button
		/// </summary>
		public virtual void ShowStartGame()
		{
			StartGameButton.gameObject.SetActive(true);
			StartGameButton.onClick.RemoveAllListeners();
			StartGameButton.onClick.AddListener(OnlineLobbyManager.Instance.OnStartGame);
		}

		/// <summary>
		/// Hides the start game button
		/// </summary>
		public virtual void HideStartGame()
		{
			StartGameButton.gameObject.SetActive(false);
		}

		/// <summary>
		/// Updates the wait players text.
		/// </summary>
		/// <param name="playersReady">Players ready.</param>
		/// <param name="playersTotal">Players total.</param>
		public virtual void UpdateWaitPlayersText(int playersReady, int playersTotal)
		{
			if (StartGameButton.gameObject.activeInHierarchy == false)
			{
				string newText = "";

				if (playersTotal > 0)
				{
					newText = playersReady.ToString();
					newText += " / " + playersTotal;
					newText += " Players Ready";
				}

				WaitPlayersText.text = newText;
			}
		}

		#endregion

		#region popup

		/// <summary>
		/// Shows the popup.
		/// </summary>
		/// <param name="text">Text inside the popup</param>
		public virtual void ShowPopup(string text)
		{
			ShowPopup(text, "", null);
		}

		/// <summary>
		/// Shows the popup.
		/// </summary>
		/// <param name="text">Text inside the button. If text is null, popup is hidden</param>
		/// <param name="buttonText">Button text to close the popup</param>
		/// <param name="onclickAction">Onclick action.</param>
		public virtual void ShowPopup(string text, string buttonText, UnityAction onclickAction)
		{
			PopupCanvas.gameObject.SetActive(true);
			PopupText.text = text;

			if (!string.IsNullOrEmpty(buttonText))
			{
				// we activate the button
				PopupActionButton.gameObject.SetActive(true);
				PopupActionButtonText.text = buttonText;

				PopupActionButton.onClick.RemoveAllListeners();
				PopupActionButton.onClick.AddListener(onclickAction);
			}
			else 
			{
				PopupActionButton.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Hides the popup.
		/// </summary>
		public virtual void HidePopup()
		{
			PopupCanvas.gameObject.SetActive(false);
		}

		#endregion
	}
}
