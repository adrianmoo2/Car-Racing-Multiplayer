using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.HighroadEngine {

	/// <summary>
	/// Generic lobby manager interface (local and online). 
	/// Used by the race manager to get info about the players and send commands to the current Lobby Manager
	/// </summary>
	public interface IGenericLobbyManager {

		/// <summary>
		/// Returns the maximum number of players
		/// </summary>
		/// <value>The max players.</value>
		int MaxPlayers { get; }

		/// <summary>
		/// Changes the current scene to the lobby scene. Will be different between local and online Lobby Manager
		/// </summary>
		void ReturnToLobby();

		/// <summary>
		/// Changes the current scene to the start screen.
		/// </summary>
		void ReturnToStartScreen();
	}
}
