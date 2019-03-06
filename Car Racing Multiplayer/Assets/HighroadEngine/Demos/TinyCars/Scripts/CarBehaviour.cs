using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine 
{
	/// <summary>
	/// Car behaviour class. Manages car behaviour in the race.
	/// This class is in charge of the particles emitted by the car, the smoke, sounds and moving parts.
	/// It depends on a CarController class to get data like speed or steering info
	/// </summary>
	[RequireComponent(typeof(CarController))]
	public class CarBehaviour : MonoBehaviour 
	{
		[Header("Animation Settings")]
		[Range(0f, 180f)]
		public float MaximumWheelSteeringAngle = 30f;
		[Range(0f, 90f)]
		public float MaximumCarBodyRollAngle = 12f;

		[Header("Particles Settings")]
		[Range(0f, 1f)]
		public float StartsLeavingSkidmarksAt = 0.8f;
		public bool EmitRocksOnlyOffRoad = false;
		[Range(0f, 1f)]
		public float StartsEmittingRocksAt = 0.6f;
		[Range(0f, 5f)]
		public float SmokesMultiplier = 2f;

		[Header("Particle Systems")]
		public ParticleSystem SmokeLeft;
		public ParticleSystem SmokeRight;
		public ParticleSystem RocksLeft;
		public ParticleSystem RocksRight;

		[Header("Car parts")]
		public GameObject FrontRightWheel;
		public GameObject FrontLeftWheel;
		public GameObject BackRightWheel;
		public GameObject BackLeftWheel;
		public GameObject CarBody;

		[Header("Sounds")]
		public AudioClip EngineSound;
		public AudioClip CrashSound;

		protected float _smokeAngle;
		protected float _smokeStartSize;

		protected ParticleSystem.ShapeModule _leftSmokeShape;
		protected ParticleSystem.ShapeModule _rightSmokeShape;

		protected GameObject _skidmarksLeft;
		protected GameObject _skidmarksRight;

		protected ParticleSystem.EmissionModule _smokeLeftEmission;
		protected ParticleSystem.EmissionModule _smokeRightEmission;
		protected ParticleSystem.EmissionModule _rocksLeftEmission;
		protected ParticleSystem.EmissionModule _rocksRightEmission;

		protected SoundManager _soundManager;
		protected AudioSource _engineSound;
		protected float _engineSoundPitch;

		protected CarController _carController;

		/// <summary>
		/// Initializes components
		/// </summary>
		protected virtual void Start() 
		{
			_carController = GetComponent<CarController>();
			_soundManager = FindObjectOfType<SoundManager>();

			_leftSmokeShape = SmokeLeft.shape;
			_rightSmokeShape = SmokeRight.shape;
			_smokeAngle = SmokeLeft.shape.angle;

			_smokeLeftEmission = SmokeLeft.emission;
			_smokeRightEmission = SmokeRight.emission;
			_rocksLeftEmission = RocksLeft.emission;
			_rocksRightEmission = RocksRight.emission;

			_smokeLeftEmission.enabled = true;
			_smokeRightEmission.enabled = true;
			_rocksLeftEmission.enabled = false;
			_rocksRightEmission.enabled = false;

			_skidmarksLeft = Instantiate(
				Resources.Load<GameObject>("Particles/Skidmarks"),
				SmokeLeft.transform.position,SmokeLeft.transform.rotation) as GameObject;
			
			_skidmarksRight = Instantiate(
				Resources.Load<GameObject>("Particles/Skidmarks"),
				SmokeRight.transform.position,SmokeLeft.transform.rotation) as GameObject;
			
			_skidmarksLeft.transform.parent = SmokeLeft.transform;
			_skidmarksRight.transform.parent = SmokeRight.transform;		

			if (EngineSound != null)
			{
				_soundManager = FindObjectOfType<SoundManager>();
				if (_soundManager != null)
				{
					_engineSound = _soundManager.PlayLoop(EngineSound, transform.position);

					if (_engineSound != null)
					{
						_engineSoundPitch = _engineSound.pitch;
						_engineSound.volume = 0;
					}
				}
			}
		}

		/// <summary>
		/// On Update, we make our car's body roll, turn its wheels and handle effects
		/// </summary>
		protected virtual void Update()
		{
			CarBodyRoll();

			TurnWheels();

			ManageSounds();

			if (_carController.IsGrounded)
			{
				SmokeControl();
				SkidMarks();
				EmitRocks();
			}
		}

		/// <summary>
		/// Manages the sounds.
		/// </summary>
		protected virtual void ManageSounds()
		{
			if (_engineSound == null)
			{
				return;
			}

			_engineSound.volume = 0.1f 
				+ Mathf.Abs(_carController.NormalizedSpeed) 
				- Mathf.Abs(_carController.CurrentSteeringAmount / 2);
			
			_engineSound.pitch = _engineSoundPitch
				* (Mathf.Abs(_carController.NormalizedSpeed * 3) + Mathf.Abs(_carController.CurrentSteeringAmount * 2));
		}

		/// <summary>
		/// Manages the car's body roll
		/// </summary>
		protected virtual void CarBodyRoll()
		{
			CarBody.transform.localEulerAngles = _carController.CurrentSteeringAmount 
				* MaximumCarBodyRollAngle 
				* Mathf.Abs(_carController.NormalizedSpeed) 
				* Vector3.forward;
		}

		/// <summary>
		/// Controls the smoke effets
		/// </summary>
		protected virtual void SmokeControl()
		{
			_leftSmokeShape.angle = Mathf.Abs(_carController.NormalizedSpeed) * _smokeAngle;
			_rightSmokeShape.angle = Mathf.Abs(_carController.NormalizedSpeed) * _smokeAngle;

			float startSizeMultiplier = Mathf.Abs(_carController.CurrentGasPedalAmount) * SmokesMultiplier;

			ParticleSystem.MainModule leftMain = SmokeLeft.main;
			ParticleSystem.MainModule rightMain = SmokeRight.main;

			leftMain.startSizeMultiplier = startSizeMultiplier;
			rightMain.startSizeMultiplier = startSizeMultiplier;
		}

		/// <summary>
		/// Turns the wheels.
		/// </summary>
		protected virtual void TurnWheels()
		{
			FrontLeftWheel.transform.localEulerAngles = _carController.CurrentSteeringAmount 
				* MaximumWheelSteeringAngle
				* Vector3.up;
			
			FrontRightWheel.transform.localEulerAngles = _carController.CurrentSteeringAmount
				* MaximumWheelSteeringAngle 
				* Vector3.up;
		}

		/// <summary>
		/// Handles skid marks when the vehicle is steering enough 
		/// </summary>
		protected virtual void SkidMarks()
		{	
			Vector3 leftPosition = SmokeLeft.transform.position;
			leftPosition.y -= 0.15f; 
			Vector3 rightPosition = SmokeRight.transform.position;
			rightPosition.y -= 0.15f; 

			
			// if we need to put skidmarks, we position our skidmarks emitters at ground level
			if (Mathf.Abs(_carController.CurrentSteeringAmount) > StartsLeavingSkidmarksAt)
			{
				_skidmarksLeft.transform.position = leftPosition;
				_skidmarksRight.transform.position = rightPosition;
			}
			else
			{
				// otherwise we hide them underground. Ugly, but that's one limitation of the TrailRenderer component.
				_skidmarksLeft.transform.position = leftPosition + Vector3.down * 3;
				_skidmarksRight.transform.position = rightPosition + Vector3.down * 3;
			}
		}

		/// <summary>
		/// Rocks emission
		/// </summary>
		protected virtual void EmitRocks()
		{
			_rocksLeftEmission.enabled=false;
			_rocksRightEmission.enabled=false;

			// Only when car is grounded
			if (!_carController.IsGrounded) 
			{
				return;
			}

			// if rocks are limited to offroads, we check this condition
			if (EmitRocksOnlyOffRoad && !_carController.IsOffRoad) 
			{
				return;
			}
			
			// if we need to start throwing rocks, we turn the emitter on, otherwise off
			if (Mathf.Abs(_carController.CurrentSteeringAmount) > StartsEmittingRocksAt)
			{
				_rocksLeftEmission.enabled = true;
				_rocksRightEmission.enabled = true;
			}
		}

		/// <summary>
		/// Used to play crash sound when colliding 
		/// </summary>
		/// <param name="other">Other.</param>
		protected virtual void OnCollisionEnter(Collision other)
		{
			if (CrashSound != null)
			{
				if (other.gameObject.layer != LayerMask.NameToLayer("Ground"))
				{
					if (_soundManager != null)
					{
						_soundManager.PlaySound(CrashSound, transform.position, true);
					}
				}
			}
	    }

		/// <summary>
		/// We remove the engine sound at the end of the game.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (_engineSound != null)
			{
				_engineSound.Stop();
				Destroy(_engineSound);
			}
		}
	}
}
