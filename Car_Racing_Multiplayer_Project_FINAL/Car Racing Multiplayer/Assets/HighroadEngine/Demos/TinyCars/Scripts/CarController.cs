using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// Car controller class.
	/// Manages inputs from InputManager and driving evaluation
	/// You can customize the car properties for different types of cars
	/// </summary>
	public class CarController : BaseController
	{
		[Header("Engine")]
		[Range(1,30)]
		/// the car's maximum speed
		public float MaximumVelocity = 20f; 
		[Range(20,100)]
		/// the engine's force
		public float Acceleration = 100f; 
		[Range(1,10)]
		/// the intensity of the brakes
		public float BrakeForce = 10f; 
		[Range(1,10)]
		/// the penalty applied when going offroad
		public float OffroadFactor = 1f; // penalty when going offroad

		[Header("Parameters")]
		// the maximal distance to the ground for IsGrounded evaluation
		public float CarGroundDistance = 0.1f; 

		/// Gears enum. Car can be forward driving or backward driving (reverse)
		public enum Gears {forward, reverse}

		/// Current car velocity
		protected Vector3 _currentVelocity;

		/// The current gear value
		public Gears CurrentGear {get; protected set;}

		// Const used by the car engine
		// Feel free to edit these values. Just be sure to test thoroughly the new car driving behaviour
		protected const float _smallValue = 0.01f; // The minimal speed value at which car is changing from braking to going reverse
		protected const float _minimalGasPedalValue = 20f; // Minimal velocity value. Going less will means car can't move offroad and others glitches
		protected float _distanceToTheGroundRaycastLength = 50f; // raycast length used for Ground evaluation

		protected Vector3 _gravity = new Vector3(0,-30,0); // Gravity is this world is set to -30 on the y axis
		protected Vector3 _centerOfMass = new Vector3(0, -1f, 0.5f); // Point of gravity of the car is set below. This helps the Unity Physics with car stability

        public GameObject bulletPrefab;
        Vector3 bulletOffset = new Vector3(5.0f, 0.0f, .0f);
        protected VehicleAI _vehicleAI;

        /// <summary>
        /// Gets or sets the distance to the ground.
        /// </summary>
        /// <value>The distance to the ground.</value>
        protected virtual float DistanceToTheGround {get; set;}

		/// <summary>
		/// Gets or sets the ground game object.
		/// </summary>
		/// <value>The ground game object.</value>
		protected virtual GameObject GroundGameObject {get; set;}

		/// <summary>
		/// Gets a value indicating whether this car is off road.
		/// </summary>
		/// <value><c>true</c> if this instance is off road; otherwise, <c>false</c>.</value>
		public virtual bool IsOffRoad 
		{
			get 
			{
				return (GroundGameObject != null && GroundGameObject.tag == "OffRoad");
			}
		}

		/// <summary>
		/// Gets the normalized speed.
		/// </summary>
		/// <value>The normalized speed.</value>
		public virtual float NormalizedSpeed {
			get 
			{
				return Mathf.InverseLerp(0f, MaximumVelocity, Mathf.Abs(Speed));
			}
		}

		/// <summary>
		/// Physics initialization
		/// </summary>
		protected override void Awake() 
		{
			base.Awake();

            //Setting up references
            _vehicleAI = GetComponent<VehicleAI>();

            // we set the physics gravity adapted to this game
            Physics.gravity = _gravity;

			// we change the center of mass below the vehicle to help with unity physics stability
			_rigidbody.centerOfMass += _centerOfMass;
		}

		/// <summary>
		/// Update
		/// </summary>
		protected virtual void Update() 
		{
			_currentVelocity = _rigidbody.velocity;

			UpdateGroundSituation();

            if (Input.GetKeyDown(KeyCode.Z) && !(_vehicleAI.Active))
            {
                Fire();
            }

			// Enable this line if you want to see debug info
			//ShowDebugInformation();
		}

		/// <summary>
		/// Shows the debug information. Optionnal.
		/// Can be used during tests to show vehicle values
		/// </summary>
		protected virtual void ShowDebugInformation() 
		{
			MMDebug.DebugOnScreen("Speed", (int)Speed);
			MMDebug.DebugOnScreen("IsGrounded", IsGrounded);
			MMDebug.DebugOnScreen("IsOffRoad", IsOffRoad);
			MMDebug.DebugOnScreen("Score", Score);
		}

		/// <summary>
		/// Updates the ground situation for this car.
		/// </summary>
		protected virtual void UpdateGroundSituation() 
		{
			// we cast a ray below the car to check if we're above ground and to determine the distance
			RaycastHit raycast3D = MMDebug.Raycast3D(
				_collider.bounds.center, 
				Vector3.down, 
				_distanceToTheGroundRaycastLength, 
				1<<LayerMask.NameToLayer("Ground"), 
				Color.green, 
				true);

			if (raycast3D.transform != null) 
			{
				// We have a ground object
				DistanceToTheGround = raycast3D.distance;

				if (raycast3D.transform.gameObject != null) 
				{
					// We store the ground object. (Will be used for offroad check)
					GroundGameObject = raycast3D.transform.gameObject;
				}
			}

			// Distance to the ground should be at a minimum
			if ((DistanceToTheGround - _collider.bounds.extents.y) < CarGroundDistance) 
			{
				IsGrounded = true;
			} 
			else 
			{
				IsGrounded = false;
			}
		}

		/// <summary>
		/// Fixed update.
		/// We apply physics and input evaluation.
		/// </summary>
		protected virtual void FixedUpdate() 
		{
			// if the player is accelerating
			if (CurrentGasPedalAmount > 0)
			{
				CurrentGear = Gears.forward;
				Accelerate();
			}

			// if the player is steering
			if (CurrentSteeringAmount != 0)
			{
				Steer();
			}	  

			// if the player is braking
			if (CurrentGasPedalAmount < 0)
			{
				// if it's fast enough, the car starts braking
				if ((Speed > _smallValue) && (CurrentGear == Gears.forward))
				{
					Brake();
				} else
				{
					// Otherwise, car is slow enough to go reverse
					CurrentGear = Gears.reverse;
					// Car is going reverse at half acceleration value
					CurrentGasPedalAmount /= 2;
					Accelerate();
				}
			}
		}

		/// <summary>
		/// Manages the acceleration of the car (forward or reverse)
		/// </summary>
		protected virtual void Accelerate() 
		{
			if (_rigidbody.velocity.magnitude > MaximumVelocity) 
			{
				// if the car goes fast enough, we don't do anything and exit
				return;
			}

			// If we are offroad, we penalize acceleration and max velocity
			if ( GroundGameObject != null && GroundGameObject.tag == "OffRoad") 
			{
				if (_rigidbody.velocity.magnitude > ( MaximumVelocity / OffroadFactor))
				{
					// if the car is going fast enough in an offroad ground, we don't do anything
					return;
				}

				// acceleration is divided by the offroad penalty factor
				CurrentGasPedalAmount /= OffroadFactor;
			}

			// the force of acceleration has a minimal value to avoid having the car blocked by drag / friction and material physics interaction
			float force = Mathf.Max(_minimalGasPedalValue, Mathf.Abs(CurrentGasPedalAmount * Acceleration));

			// if the acceleration is negative, we change the force value sign
			if (CurrentGasPedalAmount < 0) 
			{
				force = -force;
			}

			// we apply the new velocity
			_rigidbody.velocity = _rigidbody.velocity + (transform.forward * force * Time.fixedDeltaTime);

		}

		/// <summary>
		/// Brake force
		/// </summary>
		protected virtual void Brake() 
		{
			// only if the car is moving
			if ((Speed > 0) && (CurrentGear == Gears.forward)) 
			{
				// we apply the new velocity
				_rigidbody.velocity = _rigidbody.velocity - (transform.forward * BrakeForce * Time.fixedDeltaTime);	        
			}
	    }

		/// <summary>
		/// Steer evaluation.
		/// We get steering value and apply rotation accordingly
		/// </summary>
		protected virtual void Steer() 
		{
			float steeringAmount = CurrentSteeringAmount * Time.fixedDeltaTime * SteeringSpeed;

			// Going backward, we invert steering
			if (CurrentGear == Gears.reverse) 
			{
				steeringAmount = -steeringAmount;
			}

			transform.Rotate(steeringAmount * NormalizedSpeed * Vector3.up);
		}

        public void Fire()
        {
            // Create the Bullet from the Bullet Prefab
            Vector3 tempPosition = transform.position;
            tempPosition += bulletOffset;

            var bullet = (GameObject)Instantiate(
                bulletPrefab,
                tempPosition,
                transform.rotation);

            // Add velocity to the bullet
            bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 30f;

            // Destroy the bullet after 2 seconds
            Destroy(bullet, 1.0f);
        }
	}
}