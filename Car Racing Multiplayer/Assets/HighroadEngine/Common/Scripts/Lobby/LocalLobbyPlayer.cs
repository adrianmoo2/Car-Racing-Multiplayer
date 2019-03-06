using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace MoreMountains.HighroadEngine 
{
	/// <summary>
	/// Local lobby player.
	/// </summary>
	public class LocalLobbyPlayer : ILobbyPlayerInfo 
	{
		#region ILobbyPlayerInfo implementation

		public int Position { get; set; }

		public string Name { get; set; }

		public string VehicleName { get; set; }

		public int VehicleSelectedIndex { get; set; }

		public bool IsBot { get; set; }

		#endregion
	}
}