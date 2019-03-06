using UnityEngine;
using System.Collections;
using System.Linq;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// Base class controller.
	/// Must be used by vehicles specific controllers.
	/// Manages Score and input management.
	/// </summary>
	[RequireComponent(typeof(Rigidbody))]
	public class BaseController : MonoBehaviour, IActorInput
	{
		[Header("Bonus")]
		/// the force applied when the vehicle is in a boost zone
		public float BoostForce = 1f; 

		[Header("Engine")]
		[Range(1,300)]
		/// the speed at which the car steers 
		public float SteeringSpeed = 100f; 

		protected Rigidbody _rigidbody;
		protected Collider _collider;
		protected RaceManager _raceManager;
		protected Transform[] _checkpoints;
		protected int _currentWaypoint = 0;

		/// <summary>
		/// Returns the current lap.
		/// </summary>
		public int CurrentLap {get; protected set;}

		/// <summary>
		/// Gets or sets the current steering amount.
		/// </summary>
		/// <value>The current steering amount.</value>
		public float CurrentSteeringAmount {get; set;}

		/// <summary>
		/// Gets or sets the current gas pedal amount.
		/// </summary>
		/// <value>The current gas pedal amount.</value>
		public float CurrentGasPedalAmount {get; set;}

		/// <summary>
		/// Gets or sets a value indicating whether this user is playing.
		/// </summary>
		/// <value><c>true</c> if this instance is playing; otherwise, <c>false</c>.</value>
		public virtual bool IsPlaying {get; protected set;}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is grounded.
		/// </summary>
		/// <value><c>true</c> if this instance is grounded; otherwise, <c>false</c>.</value>
		public virtual bool IsGrounded {get; protected set;}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is on speed boost.
		/// </summary>
		/// <value><c>true</c> if this instance is on speed boost; otherwise, <c>false</c>.</value>
		public virtual bool IsOnSpeedBoost {get; protected set;}

		/// <summary>
		/// Gets the vehicle speed.
		/// </summary>
		/// <value>The speed.</value>
		public virtual float Speed 
		{ 
			get 
			{ 
				return _rigidbody.velocity.magnitude;
			} 
		}

		/// <summary>
		/// Gets the player score.
		/// </summary>
		/// <value>The score.</value>
		public virtual int Score 
		{
			get 
			{
				return (CurrentLap * _checkpoints.Length) + _currentWaypoint;
			}
		}

		/// <summary>
		/// Gets the distance to the next waypoint.
		/// </summary>
		/// <value>The distance to next waypoint.</value>
		public virtual float DistanceToNextWaypoint 
		{
			get 
			{
				if (_checkpoints.Length == 0)
				{
					return 0;
				}

				Vector3 checkpoint = _checkpoints[(_currentWaypoint + 1) % _checkpoints.Length].position;
				return Vector3.Distance(transform.position, checkpoint);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this vehicle has finished the race.
		/// </summary>
		/// <value><c>true</c> if this vehicle has finished; otherwise, <c>false</c>.</value>
		public virtual bool HasFinished 
		{
			get 
			{
				return (Score >= (_raceManager.Laps * _checkpoints.Length));
			}
		}

		/// <summary>
		/// Initializes various references
		/// </summary>
		protected virtual void Awake()
		{
			// Init managers
			_collider = GetComponent<Collider>();
			_raceManager = FindObjectOfType<RaceManager>();
			_rigidbody = GetComponent<Rigidbody>();

			IsOnSpeedBoost = false;
		}

		/// <summary>
		/// Initializes checkpoints
		/// </summary>
		protected virtual void Start() 
		{
			// We get checkpoints as an array of transform
			if (_raceManager != null) 
			{
				_checkpoints = _raceManager.Checkpoints.Select(x => x.transform).ToArray();
			}
		}

		#region IActorInput implementation

		// Manages User interactions from keyboard, joystick, touch joypad

		public virtual void MainActionPressed() 
		{
			CurrentGasPedalAmount = 1;
		}

		public virtual void MainActionDown() 
		{
			CurrentGasPedalAmount = 1;
		}

		public virtual void MainActionReleased() 
		{
			CurrentGasPedalAmount = 0;
		}

		public void AltActionPressed()
		{
			CurrentGasPedalAmount = -1;
		}

		public void AltActionDown()
		{
			CurrentGasPedalAmount = -1;
		}

		public void AltActionReleased()
		{
			CurrentGasPedalAmount = 0;
		}

		public virtual void LeftPressed() 
		{ 
			CurrentSteeringAmount = -1;
		}

		public virtual void RightPressed() 
		{ 
			CurrentSteeringAmount = 1;
		}

		public virtual void UpPressed() 
		{ 
			CurrentGasPedalAmount = 1;
		}

		public virtual void DownPressed() 
		{ 
			CurrentGasPedalAmount = -1;
		}

		public virtual void MobileJoystickPosition(Vector2 value)
		{
			CurrentSteeringAmount = value.x;
		}

		public virtual void HorizontalPosition(float value) 
		{
			CurrentSteeringAmount = value;
		}

		public virtual void VerticalPosition(float value) 
		{
			CurrentGasPedalAmount = value;
		}

		public void LeftReleased()
		{ 
			CurrentSteeringAmount = 0;
		}

		public void RightReleased()
		{ 
			CurrentSteeringAmount = 0;
		}

		public void UpReleased()
		{
			CurrentGasPedalAmount = 0;
		}

		public void DownReleased()
		{ 
			CurrentGasPedalAmount = 0;
		}

		#endregion

		/// <summary>
		/// Describes what happens when the object starts colliding with something
		/// Used for checkpoint interaction
		/// </summary>
		/// <param name="other">Other.</param>
		public virtual void OnTriggerEnter(Collider other) 
		{
			// Vehicle just crossed a checkpoint
			if (other.tag == "Checkpoint") 
			{
				int newLap = CurrentLap;
				int newWaypoint = _currentWaypoint;

				// If this checkpoint was the next checkpoint for this vehicle
				if (_checkpoints[_currentWaypoint] == other.transform) 
				{
					newWaypoint++;
				}

				// If this was the last checkpoint for the lap
				if (newWaypoint == _checkpoints.Length) 
				{
					newLap++;
					newWaypoint = 0;
				}

				_currentWaypoint = newWaypoint;
				CurrentLap = newLap;
			}
		}

		/// <summary>
		/// Describes what happens when something is colliding with our object
		/// Used to apply a boost force to the vehicle while staying in a boost zone.
		/// </summary>
		/// <param name="other">Other.</param>
		public virtual void OnTriggerStay(Collider other) 
		{
			if (other.tag == "SpeedBoost")
			{
				// While in speedboost, we accelerate vehicle
				_rigidbody.AddForce(transform.forward * BoostForce, ForceMode.Impulse);
				IsOnSpeedBoost = true;
			} 
		}

		/// <summary>
		/// Describes what happens when the collision ends
		/// Removes "boost" state when the vehicle exits a boost zone
		/// </summary>
		/// <param name="other">Other.</param>
		public virtual void OnTriggerExit(Collider other)
		{
			if (other.tag == "SpeedBoost")
			{
				IsOnSpeedBoost = false;
			}
		}

		/// <summary>
		/// Enables the controls.
		/// </summary>
		/// <param name="controllerId">Controller identifier.</param>
		public virtual void EnableControls(int controllerId) 
		{
			IsPlaying = true;
			CurrentSteeringAmount = 0;
			CurrentGasPedalAmount = 0;

			// If player is not a bot
			if (controllerId != -1) 
			{
				InputManager.Instance.SetPlayer(controllerId, this);
			}
		}

		/// <summary>
		/// Disables the controls.
		/// </summary>
		/// <param name="controllerId">Controller identifier.</param>
		public virtual void DisableControls(int controllerId) 
		{
			IsPlaying = false;
			CurrentSteeringAmount = 0;
			CurrentGasPedalAmount = 0;

			// If player is not a bot
			if (controllerId != -1) 
			{
				InputManager.Instance.DisablePlayer(controllerId);
			}
		}
	}
}
