using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// Graphics and particles behaviour
	/// </summary>
	[RequireComponent(typeof(AirCarController))]
	public class AirCarBehaviour : MonoBehaviour 
	{
		// Reference to vehicle mesh
		public Transform Mesh;

		[Header("Animation Settings")]
		[Range(0f, 90f)]
		public float MaximumBodyRollAngle = 30f;
		public float BodyRollSpeed = 1f;

		[Header("Particle Behaviour Settings")]
		public float WindEmissionsFullRate = 200f;
		public float ReactorFullScale = 0.2f;
		public float TailMinSize = 0.01f;
		public float TailMaxSize = 0.3f;
		public Color TailNormalColor = Color.white;
		public Color TailBoostColor = Color.blue;
		public float ParticlesActivationMinimumSpeed = 5f;
		[Information("Speed is divided by this value when used to lerp emission rate or scale size.\n", InformationAttribute.InformationType.Info, false)]
		public float SpeedFactor = 10f;


		[Header("Particle Systems")]
		public ParticleSystem WindLeft;
		public ParticleSystem WindRight;
		public SpriteRenderer ReactorLeft;
		public SpriteRenderer ReactorRight;
		public SpriteRenderer Reactor2Left;
		public SpriteRenderer Reactor2Right;
		public ParticleSystem RearParticlesLeft;
		public ParticleSystem RearParticlesRight;
		public ParticleSystem Tail;

		protected AirCarController _controller;
		protected ParticleSystem.EmissionModule _windLeftEmissions;
		protected ParticleSystem.EmissionModule _windRightEmissions;
		protected ParticleSystem.EmissionModule _rearLeftEmissions;
		protected ParticleSystem.EmissionModule _rearRightEmissions;
		protected ParticleSystem.EmissionModule _tailEmissions;

		/// <summary>
		/// Initializes references
		/// </summary>
		public virtual void Start() 
		{
			_controller = GetComponent<AirCarController>();
			_windLeftEmissions = WindLeft.emission;
			_windRightEmissions = WindRight.emission;
			_rearLeftEmissions = RearParticlesLeft.emission;
			_rearRightEmissions = RearParticlesRight.emission;
			_tailEmissions = Tail.emission;
		}
		
		/// <summary>
		/// Particles and animations updates
		/// </summary>
		public virtual void FixedUpdate()
		{
			VehicleRoll();

			WindUpdate();

			ReactorUpdate();
		}

		/// <summary>
		/// Manages rolling of the vehicle depending on input controls
		/// </summary>
		protected virtual void VehicleRoll()
		{
			// we linearise steering amount from -1 -> 1 to a value from 0 to MaximumBodyRollAngle * 2
			float targetz = Mathf.Lerp(MaximumBodyRollAngle * 2, 0f, (_controller.CurrentSteeringAmount / 2 + 0.5f));

			// we linearise current z mesh rotation angle to 0 -> MaximumBodyRollAngle * 2 absolute value
			float zAngle = Mesh.localEulerAngles.z;
			float currentz = (zAngle > 180 ? zAngle - 360 : zAngle) + (MaximumBodyRollAngle);

			// we evaluate lerp rotation from current roation to target input rotation
			float lerpz = Mathf.Lerp(currentz, targetz, Time.fixedDeltaTime * BodyRollSpeed);

			// we linearise back to -30 -> 30 angle
			lerpz -= MaximumBodyRollAngle;

			Mesh.localEulerAngles = new Vector3(Mesh.localEulerAngles.x, Mesh.localEulerAngles.y, lerpz);
		}

		/// <summary>
		/// Updates wind particles emission rate depending on vehicle speed
		/// </summary>
		protected virtual void WindUpdate()
		{
			float newValue = 0f;

			if (_controller.Speed >= ParticlesActivationMinimumSpeed)
			{
				newValue = Mathf.Lerp(0f, WindEmissionsFullRate, _controller.Speed / SpeedFactor);
			}

			_windLeftEmissions.rateOverTime = newValue;
			_windRightEmissions.rateOverTime = newValue;
		}

		/// <summary>
		/// Updates reactor display
		/// </summary>
		protected virtual void ReactorUpdate()
		{
			ParticleSystem.MainModule mainModule = Tail.main;

			float scaleValue = Mathf.Lerp(0f, ReactorFullScale, _controller.Speed / SpeedFactor);
			Vector3 newScale = Vector3.one * scaleValue;

			ReactorLeft.transform.localScale = newScale;
			Reactor2Left.transform.localScale = newScale;

			ReactorRight.transform.localScale = newScale;
			Reactor2Right.transform.localScale = newScale;

			if (_controller.Speed <= ParticlesActivationMinimumSpeed) 
			{
				mainModule.startSize = 0f;
				_rearLeftEmissions.enabled = false;
				_rearRightEmissions.enabled = false;
			}
			else 
			{
				_rearLeftEmissions.enabled = true;
				_rearRightEmissions.enabled = true;

				mainModule.startSize = Mathf.Lerp(TailMinSize, TailMaxSize, _controller.Speed / SpeedFactor);
			}

			// If vehicle is have a boost, we change tail color
			if (_controller.IsOnSpeedBoost)
			{
				mainModule.startColor = TailBoostColor;
			}
			else 
			{
				mainModule.startColor = TailNormalColor;
			}
		}

	}
}