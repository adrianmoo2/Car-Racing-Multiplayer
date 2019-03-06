 using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace MoreMountains.HighroadEngine 
{
	/// <summary>
	/// An interface listing available input actions
	/// </summary>
	public interface IActorInput 
	{
		/// <summary>
		/// Button Main Action is being pressed
		/// </summary>
		void MainActionPressed();

		/// <summary>
		/// Button Main Action is pressed down
		/// </summary>
		void MainActionDown();

		/// <summary>
		/// Button Main Action is released
		/// </summary>
		void MainActionReleased();

		/// <summary>
		/// Button Alt Action is being pressed
		/// </summary>
		void AltActionPressed();

		/// <summary>
		/// Button Alt Action is pressed down
		/// </summary>
		void AltActionDown();

		/// <summary>
		/// Button Alt Action is released
		/// </summary>
		void AltActionReleased();

		/// <summary>
		/// Button Left is being pressed
		/// </summary>
		void LeftPressed();

		/// <summary>
		/// Button Left is released
		/// </summary>
		void LeftReleased();

		/// <summary>
		/// Button Right is being pressed
		/// </summary>
		void RightPressed();

		/// <summary>
		/// Button Right is released
		/// </summary>
		void RightReleased();

		/// <summary>
		/// Button Up is being pressed
		/// </summary>
		void UpPressed();

		/// <summary>
		/// Button Up is released
		/// </summary>
		void UpReleased();

		/// <summary>
		/// Button Down is being pressed
		/// </summary>
		void DownPressed();

		/// <summary>
		/// Button Down is released
		/// </summary>
		void DownReleased();

		/// <summary>
		/// Update of the mobile joystick position
		/// </summary>
		/// <param name="value">Value.</param>
		void MobileJoystickPosition(Vector2 value);

		/// <summary>
		/// Update of the horizontal value (from -1 to 1)
		/// </summary>
		/// <param name="value">Value.</param>
		void HorizontalPosition(float value);

		/// <summary>
		/// Update of the vertical value (from -1 to 1)
		/// </summary>
		/// <param name="value">Value.</param>
		void VerticalPosition(float value);
	}
}
