using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.HighroadEngine
{	
	/// <summary>
	/// This persistent singleton handles the inputs and sends commands to the active players
	/// </summary>
	public class InputManager : MonoBehaviour
	{
		[Header("Mobile Touch Controls")]
		// reference to mobile touch controls UI. If device is not a mobile device, we hide this canvas from screen.
		public RectTransform MobileTouchControls;

		/// <summary>
		/// when true, we don't handle keyboards events
		/// </summary>
		public bool MobileDevice { get; protected set;}

		// singleton pattern
		static public InputManager Instance { get { return _instance; } }
		static protected InputManager _instance;

		protected Dictionary<int, IActorInput> _players;

		/// <summary>
		/// initialization of the active players list and type of input
		/// </summary>
		public virtual void Awake() 
		{
			_instance = this;
			_players = new Dictionary<int, IActorInput>();

			if (MobileTouchControls != null)
			{
				MobileTouchControls.gameObject.SetActive(false);
			}

			MobileDevice = false;

			#if UNITY_ANDROID || UNITY_IPHONE
			// On android/ios we activate mobileTouchControls
			if (MobileTouchControls != null)
			{
		        MobileTouchControls.gameObject.SetActive(true);
			}

			MobileDevice = true;
			#endif
		}

		/// <summary>
		/// Links a player to a controller id
		/// </summary>
		/// <param name="controllerId">Controller identifier.</param>
		/// <param name="p">Player</param>
		public virtual void SetPlayer(int controllerId, IActorInput p) 
		{
			_players[controllerId] = p;
		}

		/// <summary>
		/// Unlinks a player to a controller id
		/// </summary>
		/// <param name="controllerId">Controller identifier.</param>
		public virtual void DisablePlayer(int controllerId) 
		{
			_players.Remove(controllerId);
		}

		/// <summary>
		/// Every frame, we get the various inputs and process them
		/// </summary>
		public virtual void Update() 
		{
			// We handle keyboard inputs when device is not mobile
			if (_players.Count > 0 && !MobileDevice) 
			{
				HandleKeyboard();
			}
		}

		/// <summary>
		/// Called at each Update(), it checks for various key presses
		/// </summary>
		protected virtual void HandleKeyboard() 
		{
			foreach (KeyValuePair<int, IActorInput> player in _players) 
			{
				HandleKeyboardForPlayer(player.Key+1, player.Value);
			}
		}

		/// <summary>
		/// Handles the input for a specific player.
		/// </summary>
		/// <param name="number">controller Id</param>
		/// <param name="p">P.layer</param>
		protected virtual void HandleKeyboardForPlayer(int number, IActorInput p) 
		{
			// We define the input axis / buttons names for this player
			string playerMainAction = "Player" + number + "MainAction";
			string playerAltAction = "Player" + number + "AltAction";
			string playerHorizontal = "Player" + number + "Horizontal";
			string playerVertical = "Player" + number + "Vertical";

			// Movement management
			HorizontalPosition(p, Input.GetAxis(playerHorizontal));
			VerticalPosition(p, Input.GetAxis(playerVertical));

			if (Input.GetButton(playerMainAction)) { MainActionButtonPressed(p); }
			if (Input.GetButtonDown(playerMainAction)) { MainActionButtonDown(p); }
			if (Input.GetButtonUp(playerMainAction)) { MainActionButtonReleased(p); }

			if (Input.GetButton(playerAltAction)) { AltActionButtonPressed(p); }
			if (Input.GetButtonDown(playerAltAction)) { AltActionButtonDown(p); }
			if (Input.GetButtonUp(playerAltAction)) { AltActionButtonReleased(p); }
		}


		/// <summary>
		/// Triggered when the main action button is being pressed for player 1
		/// </summary>
		public virtual void MainActionButtonDown() 
		{
			if (_players.ContainsKey(0)) 
			{
				MainActionButtonDown(_players[0]);
			}
		}

		/// <summary>
		/// Triggered when the main action button is being pressed for a player
		/// </summary>
		///<param name="p">Player</param>
		public virtual void MainActionButtonDown(IActorInput p) 
		{
			p.MainActionDown();
		}

		/// <summary>
		/// Triggered when the main action button is released for player 1
		/// </summary>
		public virtual void MainActionButtonReleased() 
		{
			if (_players.ContainsKey(0)) 
			{
				MainActionButtonReleased(_players[0]);
			}
		}

		/// <summary>
		/// Triggered when the main action button is released for a player
		/// </summary>
		/// <param name="p">Player</param>
		public virtual void MainActionButtonReleased(IActorInput p) 
		{
			p.MainActionReleased();
		}

		/// <summary>
		/// Triggered when the main action button is pressed for player 1
		/// </summary>
		public virtual void MainActionButtonPressed() 
		{
			if (_players.ContainsKey(0)) 
			{
				MainActionButtonPressed(_players[0]);
			}
		}
			
		/// <summary>
		/// Triggered when the main action button is pressed for a player
		/// </summary>
		/// <param name="p">Player</param>
		public virtual void MainActionButtonPressed(IActorInput p) 
		{
			p.MainActionPressed();
		}

		/// <summary>
		/// Triggered when the alt action button is being pressed for player 1
		/// </summary>
		public virtual void AltActionButtonDown() 
		{
			if (_players.ContainsKey(0)) 
			{
				AltActionButtonDown(_players[0]);
			}
		}

		/// <summary>
		/// Triggered when the alt action button is being pressed for a player
		/// </summary>
		/// <param name="p">Player</param>
		public virtual void AltActionButtonDown(IActorInput p) 
		{
			p.AltActionDown();
		}

		/// <summary>
		/// Triggered when the alt action button is released for player 1
		/// </summary>
		public virtual void AltActionButtonReleased() {
			if (_players.ContainsKey(0)) 
			{
				AltActionButtonReleased(_players[0]);
			}
		}

		/// <summary>
		/// Triggered when the alt action button is released for a player
		/// </summary>
		/// <param name="p">Player</param>
		public virtual void AltActionButtonReleased(IActorInput p) 
		{
			p.AltActionReleased();
		}

		/// <summary>
		/// Triggered when the alt action button is being pressed for player 1
		/// </summary>
		/// <param name="p">Player</param>
		public virtual void AltActionButtonPressed() 
		{
			if (_players.ContainsKey(0)) 
			{
				AltActionButtonPressed(_players[0]);
			}
		}

		/// <summary>
		/// Triggered when the alt action button is being pressed for a player
		/// </summary>
		/// <param name="p">Player</param>
		public virtual void AltActionButtonPressed(IActorInput p) 
		{
			p.AltActionPressed();
		}

		/// <summary>
		/// Triggered when Left Button is pressed for player 1
		/// </summary>
		public virtual void LeftButtonPressed() 
		{
			if (_players.ContainsKey(0)) 
			{
				LeftButtonPressed(_players[0]);
			}
		}
			
		/// <summary>
		/// Triggered when Left Button is pressed for a player
		/// </summary>
		/// <param name="p">Player.</param>
		public virtual void LeftButtonPressed(IActorInput p) 
		{
			p.LeftPressed();
		}

		/// <summary>
		/// Triggered when Left Button is released for player 1
		/// </summary>
		public virtual void LeftButtonReleased() 
		{
			if (_players.ContainsKey(0)) 
			{
				LeftButtonReleased(_players[0]);
			}
		}

		/// <summary>
		/// Triggered when Left Button is released for a player
		/// </summary>
		/// <param name="p">Player.</param>
		public virtual void LeftButtonReleased(IActorInput p) 
		{
			p.LeftReleased();
		}

		/// <summary>
		/// Triggered when Right Button is pressed for player 1
		/// </summary>
		public virtual void RightButtonPressed() 
		{
			if (_players.ContainsKey(0)) 
			{
				RightButtonPressed(_players[0]);
			}
		}

		/// <summary>
		/// Triggered when Right Button is pressed for a player
		/// </summary>
		/// <param name="p">Player.</param>
		public virtual void RightButtonPressed(IActorInput p) 
		{
			p.RightPressed();
		}

		/// <summary>
		/// Triggered when Right Button is released for player 1
		/// </summary>
		public virtual void RightButtonReleased() 
		{
			if (_players.ContainsKey(0)) 
			{
				RightButtonReleased(_players[0]);
			}
		}

		/// <summary>
		/// Triggered when Right Button is released for a player
		/// </summary>
		/// <param name="p">Player.</param>
		public virtual void RightButtonReleased(IActorInput p) 
		{
			p.RightReleased();
		}

		/// <summary>
		/// Triggered when Up Button is pressed for player 1
		/// </summary>
		public virtual void UpButtonPressed() 
		{
			if (_players.ContainsKey(0)) 
			{
				UpButtonPressed(_players[0]);
			}
		}

		/// <summary>
		/// Triggered when Up Button is pressed for a player
		/// </summary>
		/// <param name="p">Player.</param>
		public virtual void UpButtonPressed(IActorInput p) 
		{
			p.UpPressed();
		}

		/// <summary>
		/// Triggered when Down Button is pressed for player 1
		/// </summary>
		public virtual void DownButtonPressed()
		{
			if (_players.ContainsKey(0)) 
			{
				DownButtonPressed(_players[0]);
			}
		}

		/// <summary>
		/// Triggered when Down Button is pressed for a player
		/// </summary>
		/// <param name="p">Player.</param>
		public virtual void DownButtonPressed(IActorInput p) 
		{
			p.DownPressed();
		}

		/// <summary>
		/// Update position from mobile joystick for player 1
		/// </summary>
		/// <param name="value">Value.</param>
		public virtual void MobileJoystickPosition(Vector2 value) 
		{
			if (_players.ContainsKey(0)) 
			{
				_players[0].MobileJoystickPosition(value);
			}
		}

		/// <summary>
		/// Updates horizontal position for player 1
		/// </summary>
		/// <param name="value">Value.</param>
		public virtual void HorizontalPosition(float value) 
		{
			if (_players.ContainsKey(0)) 
			{
				HorizontalPosition(_players[0], value);
			}
		}

		/// <summary>
		///Updates horizontal position for a player
		/// </summary>
		/// <param name="p">Player.</param>
		/// <param name="value">Value.</param>
		public virtual void HorizontalPosition(IActorInput p, float value) 
		{
			p.HorizontalPosition(value);
		}

		/// <summary>
		/// Updates vertical position for player 1
		/// </summary>
		/// <param name="value">Value.</param>
		public virtual void VerticalPosition(float value) 
		{
			if (_players.ContainsKey(0)) 
			{
				VerticalPosition(_players[0], value);
			}
		}

		/// <summary>
		/// Updates vertical position for a player
		/// </summary>
		/// <param name="p">Player</param>
		/// <param name="value">Value.</param>
		public virtual void VerticalPosition(IActorInput p, float value) 
		{
			p.VerticalPosition(value);
		}
	}
}