using UnityEngine;
using System.Collections;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// A simple interface used by the network manager to set properties to vehicle objects when starting the game.
	/// For now, only the player name is generic.
	/// </summary>
	public interface INetworkVehicle 
	{
		/// <summary>
		/// Sets the name of the player.
		/// </summary>
		/// <param name="value">Value.</param>
		void SetPlayerName(string value);
	}
}