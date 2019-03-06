using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// Network vehicle controller. In charge of the network manager for players in game
	/// </summary>
	[RequireComponent(typeof(BaseController))]
	[RequireComponent(typeof(NetworkTransform))]
	public class NetworkVehicleController : NetworkBehaviour, INetworkVehicle
	{
		[SyncVar]
		// The name of the player.
		public string PlayerName;

		[SyncVar]
		protected float SteerValue;
		[SyncVar]
		protected float GasPedalValue;
		protected BaseController _controller;
		protected NetworkRaceManager _networkRaceManager;
		protected bool _initialized = false; // while false, we are waiting for the scene and other objects to be properly initialized

		/// <summary>
		/// We synchronize steer and gas pedal values to others clients in the network
		/// </summary>
		public virtual void Update()
		{
			if (!_initialized)
			{
				if (_controller == null)
				{
					_controller = GetComponent<BaseController>();
				}

				if (isLocalPlayer)
				{
					_networkRaceManager = FindObjectOfType<NetworkRaceManager>();

					if (_networkRaceManager != null)
					{
						foreach (var c in _networkRaceManager.RaceManager.CameraControllers)
						{
							c.HumanPlayers = new [] { this.transform };
						}
						_initialized = true;
					}
				}
			}

		
			if (isLocalPlayer)
			{
				// As local player we send our gas pedal and steering values to the server
				CmdUpdateSteerAndGasPedalValues(_controller.CurrentSteeringAmount, _controller.CurrentGasPedalAmount);

				if (_initialized && _controller.IsPlaying)
				{
					// We show an update of score of current player
					_networkRaceManager.RaceManager.ScoreText1.text = "";
					_networkRaceManager.RaceManager.ScoreText2.text = PlayerName;
					_networkRaceManager.RaceManager.ScoreText3.text = string.Format("Lap {0}/{1}",
						_controller.CurrentLap >= _networkRaceManager.RaceManager.Laps ? 
						_networkRaceManager.RaceManager.Laps : _controller.CurrentLap + 1,
						_networkRaceManager.RaceManager.Laps
					);
				}
			}
			else
			{
				// As a remote controlled player, we update the vehicle controller with steer and gas pedal values
				_controller.CurrentSteeringAmount = SteerValue;
				_controller.CurrentGasPedalAmount = GasPedalValue;
			}
		}

		/// <summary>
		/// Sends the update steer and gas pedal values to the server
		/// </summary>
		/// <param name="steeringSpeed">Steering speed.</param>
		/// <param name="gasPedal">Gas pedal.</param>
		[Command]
		public virtual void CmdUpdateSteerAndGasPedalValues(float steeringSpeed, float gasPedal)
		{
			SteerValue = steeringSpeed;
			GasPedalValue = gasPedal;
		}

		/// <summary>
		/// Manages the start local player event.
		/// </summary>
		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();

			_controller = GetComponent<BaseController>();

			// We wait for server instructions
			DisableControls();
		}


		/// <summary>
		/// Enables controls instruction sent from server
		/// </summary>
		[ClientRpc]
		public virtual void RpcEnableControl()
		{
			EnableControls();
		}

		/// <summary>
		/// Disables controls instruction sent from server
		/// </summary>
		[ClientRpc]
		public virtual void RpcDisableControl()
		{
			DisableControls();
		}



		/// <summary>
		/// Enables the controls when vehicle is controlled by a local player
		/// </summary>
		public virtual void EnableControls()
		{
			if (isLocalPlayer)
			{
				_controller.EnableControls(0);
			}
		}

		/// <summary>
		/// Disables the controls when vehicle is is controlled by a local player
		/// </summary>
		public virtual void DisableControls()
		{
			if (isLocalPlayer)
			{
				_controller.DisableControls(0);
			}
		}

		#region INetworkPlayer implementation

		[Server]
		public virtual void SetPlayerName(string value)
		{
			PlayerName = value;
		}

		#endregion
	}
}