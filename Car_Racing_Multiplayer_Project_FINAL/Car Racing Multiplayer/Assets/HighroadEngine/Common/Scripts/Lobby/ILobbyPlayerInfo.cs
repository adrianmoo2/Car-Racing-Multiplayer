using UnityEngine;
using System.Collections;

namespace MoreMountains.HighroadEngine 
{
	/// <summary>
	/// Stores data about a player in the lobby screen
	/// </summary>
	public interface ILobbyPlayerInfo 
	{
		/// <summary>
		/// Position id of the player (starting at 0)
		/// </summary>
		int Position {get; set;}

		/// <summary>
		/// Name of the player. Generated from player position.
		/// </summary>
		string Name {get; set;}

		/// <summary>
		/// Player gameobject name used by player. Shown in UI and used to instantiate prefab frome Resources
		/// </summary>
		string VehicleName {get; set;}

		/// <summary>
		/// Index of the chosen vehicle in UI.
		/// </summary>
		int VehicleSelectedIndex {get; set;}

		/// <summary>
		/// Boolean indicating if the vehicle is AI controlled
		/// </summary>
		bool IsBot {get; set;}
	}
}
