using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.HighroadEngine 
{

	/// <summary>
	/// This singleton class manages the list of players between races and lobby operations
	/// </summary>
	public class LocalLobbyManager : PersistentSingleton<LocalLobbyManager>, IGenericLobbyManager 
	{
		/// the maximum number of players in the game.
		/// if you want to change this value, you need to take care of the lobby GUI. 
		/// this is why it's not a public property in the inspector.
		[HideInInspector]
		public int MAX_PLAYERS = 4;

		[Header("Lobby parameters")]
		/// the name of the lobby scene.
		public string LobbyScene = "LocalLobby";

		[Header("Vehicles configuration")]
		/// the list of vehicle Prefabs the player can choose from.
		public GameObject[] AvailableVehiclesPrefabs;

		[Header("Tracks configuration")]
		/// the list of Track Scenes names. Used to load scene & show scene name in UI
		public string[] AvailableTracksSceneName;
		/// the list of tracks sprites. Used to show an image of the chosen track in UI
		public Sprite[] AvailableTracksSprite; 

		protected Dictionary<int, ILobbyPlayerInfo> _players = new Dictionary<int, ILobbyPlayerInfo>(); // Key is player position number.
		protected Dictionary<int, int> _registeredLobbyPlayers = new Dictionary<int, int>(); // registered lobby players not ready. 

		/// <summary>
		/// Stores currently selected track index.
		/// </summary>
		/// <value>The currently selected track.</value>
		public virtual int TrackSelected {get; set;}

		#region interface IGenericLobbyManager

		/// <summary>
		/// Returns the maximum number of players.
		/// </summary>
		/// <value>The maximum number of players.</value>
		public virtual int MaxPlayers 
		{ 
			get { return MAX_PLAYERS; }
		}

		/// <summary>
		/// Changes the current scene to the lobby scene.
		/// </summary>
		public void ReturnToLobby()
		{
			SceneManager.LoadScene(LobbyScene);
		}

		/// <summary>
		/// Changes the current scene to the start screen.
		/// </summary>
		public virtual void ReturnToStartScreen()
		{
			LoadingSceneManager.LoadScene("StartScreen");
			// We destroy this persistent object
			Destroy(gameObject);
		}

		#endregion

		/// <summary>
		/// By default, we select the first track
		/// </summary>
		public virtual void Start() 
		{
			TrackSelected = 0;
		}

		/// <summary>
		/// Returns the active players list
		/// key is index, value is Player data.
		/// </summary>
		public virtual Dictionary<int, ILobbyPlayerInfo> Players() 
		{
			return _players;
		}
			
		/// <summary>
		/// Adds the player object to the active players list
		/// </summary>
		/// <param name="p">LocalLobbyPlayer player</param>
		public virtual void AddPlayer(LocalLobbyPlayer p) 
		{
			_players[p.Position] = p;
		}

		/// <summary>
		/// Removes the player from active players list
		/// </summary>
		/// <param name="position">Position</param>
		public virtual void RemovePlayer(int position) 
		{ 
			_players.Remove(position);
		}
			
		/// <summary>
		/// Gets the player by his position
		/// </summary>
		/// <returns>The player</returns>
		/// <param name="position">Position index</param>
		public virtual ILobbyPlayerInfo GetPlayer(int position)
		{
			return _players[position];
		}

		/// <summary>
		/// Returns true if a player is already registered for this position.
		/// Used when going back to the lobby scene after a game scene.
		/// </summary>
		/// <returns><c>true</c>, if player exists, <c>false</c> otherwise.</returns>
		/// <param name="position">Position index.</param>
		public virtual bool ContainsPlayer(int position) 
		{
			return _players.ContainsKey(position);
		}

		/// <summary>
		/// Finds the next free vehicle index for a player.
		/// </summary>
		/// <returns>The free vehicle index.</returns>
		public virtual int FindFreeVehicle()
		{
			int index = 0;

			while (true)
			{		
				if (!_registeredLobbyPlayers.ContainsValue(index))
				{
					break;
				}

				index++;
			}

			return Mathf.Min(index, AvailableVehiclesPrefabs.Length - 1);
		}

		/// <summary>
		/// Checks if the players are ready
		/// </summary>
		/// <returns><c>true</c> when at least one player is present and all players are ready.</returns>
		public virtual bool IsReadyToPlay() 
		{
			if (_players.Count == 0)
			{
				return false;
			}

			return (_players.Count == _registeredLobbyPlayers.Count);
		}

		/// <summary>
		/// Returns the number of players in lobby that are not ready yet
		/// </summary>
		/// <returns>The players count.</returns>
		public virtual int PlayersNotReadyCount()
		{
			return _registeredLobbyPlayers.Count;
		}
			
		/// <summary>
		/// Registers the lobby player.
		/// </summary>
		/// <param name="position">Position.</param>
		/// /// <param name="vehicleIndex">Vehicle Index.</param>
		public virtual void registerLobbyPlayer(int position, int vehicleIndex)
		{
			_registeredLobbyPlayers[position] = vehicleIndex;
		}

		/// <summary>
		/// Unregisters the lobby player.
		/// </summary>
		/// <param name="position">Position.</param>
		public virtual void unregisterLobbyPlayer(int position)
		{
			_registeredLobbyPlayers.Remove(position);
		}
	}
}