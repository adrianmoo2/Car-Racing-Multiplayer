using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

namespace MoreMountains.HighroadEngine 
{
	/// <summary>
	/// Online lobby match entry UI element. Used to join a specific match in matchmaking
	/// </summary>
	public class OnlineLobbyMatchEntry : MonoBehaviour 
	{
		// The match join button.
		public Button MatchJoinButton;

		protected string _matchName;

		/// <summary>
		/// Initializes the button with match description and button onclick
		/// </summary>
		/// <param name="match">Match description used to populate button value.</param>
		/// <param name="manager">Manager referenced on the onclick event</param>
		public virtual void Init(MatchInfoSnapshot match, OnlineLobbyManager manager)
		{
			_matchName = match.name;

			// Match name is combined with the current number of players & max size
			string info = _matchName + "  (" + match.currentSize + "/" + match.maxSize + ")";
			MatchJoinButton.GetComponentInChildren<Text>().text = info;

			MatchJoinButton.onClick.RemoveAllListeners();
			MatchJoinButton.onClick.AddListener(() => OnClick(match.networkId, manager));
		}

		/// <summary>
		/// Describes what happens when the button is clicked
		/// </summary>
		/// <param name="networkId">Network identifier.</param>
		/// <param name="manager">Manager to call back</param>
		public virtual void OnClick(NetworkID networkId, OnlineLobbyManager manager)
		{
			manager._onlineLobbyUI.TitleLabel.text = "GAME " + _matchName;
			manager.matchMaker.JoinMatch(networkId, "", "", "", 0, manager.GameId, manager.OnMatchJoined);
		}
	}
}
