using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// Air Car controller class.
	/// Manages inputs from InputManager and driving of the hover car
	/// You can customize the properties for different types of vehicles.
	/// </summary>
	public class AirCarController : BaseController	
	{
		[Information("Engine Power force.\n", InformationAttribute.InformationType.Info, false)]
		/// the engine's power
		public int EnginePower = 100;

		/// Lateral force applied when steering
		public float LateralSteeringForce = 1f; 
		/// the maximum speed
		public int MaxSpeed = 100;

		[Header("Hover management")]
		[Information("Hover height.\n", InformationAttribute.InformationType.Info, false)]
		/// the distance from the ground at which the vehicle hovers
		public float HoverHeight = 1f;

		[Information("Gravity force applied to the vehicle when ground is too far.\n", InformationAttribute.InformationType.Info, false)]
		/// the force that pushes the vehicle towards the ground
		public float HoverGravityForce = 1f;

		[Information("Hover force applied.\n", InformationAttribute.InformationType.Info, false)]
		/// the force that pushes the vehicle in the air
		public float HoverForce = 1f;

		public float OrientationGroundSpeed = 10f;

		protected RaycastHit _groundRaycastHit;

			
		/// <summary>
		/// Fixed Update : physics controls
		/// </summary>
		public virtual void FixedUpdate()
		{
			// Input management
			if (CurrentGasPedalAmount > 0)
			{
				Accelerate();
			}
				
			Rotation();

			// Physics
			Hover();

			OrientationToGround();
		}

		/// <summary>
		/// Manages the acceleration of the vehicle
		/// </summary>
		protected virtual void Accelerate() 
		{
			if (Speed < MaxSpeed)
			{
				_rigidbody.AddForce(CurrentGasPedalAmount * transform.forward * EnginePower * Time.fixedDeltaTime);
			}
		}

		/// <summary>
		/// Rotation of the vehicle using steering input
		/// </summary>
		protected virtual void Rotation()
		{
			if (CurrentSteeringAmount != 0)
			{
				transform.Rotate(CurrentSteeringAmount * Vector3.up * SteeringSpeed * Time.fixedDeltaTime);

				Vector3 horizontalVector = transform.right;

				// When rotating, we also apply an opposite tangent force to counter slipping 
				_rigidbody.AddForce(horizontalVector * CurrentSteeringAmount * Time.fixedDeltaTime * LateralSteeringForce * Speed);
			}
		}


		/// <summary>
		/// Management of the hover and gravity of the vehicle
		/// </summary>
		protected virtual void Hover()
		{
			// we enforce isgrounded to false before calculations
			IsGrounded = false;

			// Raycast origin is positionned on the center front of the car
			Vector3 rayOrigin = transform.position + (transform.forward * _collider.bounds.size.z / 2);

			// Raycast to the ground layer
			if (Physics.Raycast(
				rayOrigin, 
				-Vector3.up, 
				out _groundRaycastHit, 
				Mathf.Infinity,
				1 << LayerMask.NameToLayer("Ground")))
			{
				// Raycast hit the ground

				// If distance between vehicle and ground is higher than target height, we apply a force from up to
				// bottom (gravity) to push the vehicle down.
				if (_groundRaycastHit.distance > HoverHeight)
				{
					// Vehicle is too high, We apply gravity force
					_rigidbody.AddForce(-Vector3.up * HoverGravityForce * Time.fixedDeltaTime, ForceMode.Acceleration);
				} 
				else
				{
					// if the vehicle is low enough, it is considered grounded
					IsGrounded = true;
				
					// we determine the distance between current vehicle height and wanted height
					float distanceVehicleToHoverPosition = HoverHeight - _groundRaycastHit.distance;

					float force = distanceVehicleToHoverPosition * HoverForce;

					// we add the hoverforce to the rigidbody
					_rigidbody.AddForce(Vector3.up * force * Time.fixedDeltaTime, ForceMode.Acceleration);
				}
			}
		}

		/// <summary>
		/// Manages orientation of the vehicle depending ground surface normale 
		/// </summary>
		protected virtual void OrientationToGround()
		{
			var rotationTarget = Quaternion.FromToRotation(transform.up, _groundRaycastHit.normal) * transform.rotation;

			transform.rotation = Quaternion.Slerp(transform.rotation, rotationTarget, Time.fixedDeltaTime * OrientationGroundSpeed);
		}
	
		/// <summary>
		/// Draws controller gizmos
		/// </summary>
		public virtual void OnDrawGizmos()
		{
			var collider = GetComponent<BoxCollider>();

			var hoverposition = transform.position + (transform.forward * collider.size.z / 2);

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(hoverposition, 0.1f);

			if (IsGrounded) 
			{
				Gizmos.color = Color.green;
			} 
			else 
			{
				Gizmos.color = Color.red;
			}

			Gizmos.DrawLine(hoverposition, _groundRaycastHit.point);
		}
	}
}

