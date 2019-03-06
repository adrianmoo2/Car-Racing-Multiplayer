using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// Manages the driving with a simple AI : the AI follows each AIWaypoint in their order.
	/// 
	/// This engine is generic and can be used on different types of vehicles as long as they implement
	/// BaseController with Steer and GasPedal.
	/// </summary>
	[RequireComponent(typeof(BaseController))]
	public class VehicleAI : MonoBehaviour 
	{
		/// If Active, AI controls the vehicle
		public bool Active;

		[Header("AI configuration")]
		[Information("Distance to consider waypoint reached", InformationAttribute.InformationType.Info, false)]
		[Range(5,30)]
		/// when this distance is reached, AI goes to next waypoint
		public int MinimalDistanceToNextWaypoint = 10; 

		[Information("Throttle when waypoint is ahead", InformationAttribute.InformationType.Info, false)]
		[Range(0f, 1f)]
		/// the maximum Gas Pedal Amount 
		public float FullThrottle = 1f; 

		[Information("Throttle when vehicle must turn to reach waypoint.", InformationAttribute.InformationType.Info, false)]
		[Range(0f, 1f)]
		/// the minimum Gas Pedal Amount
		public float SmallThrottle = 0.3f; 

		[Information("To help the AI, vehicles can have a better steering speed than usual.", InformationAttribute.InformationType.Info, false)]
		public bool OverrideSteringSpeed = false;
		public int SteeringSpeed = 300;


		// Const used by the AI engine
		// Feel free to edit these values. Just be sure to test thoroughly the new AI vehicle driving behaviour
		protected const float _largeAngleDistance = 90f; // When angle between front of the vehicle and target waypoint are distant 
		protected const float _smallAngleDistance = 5f;  // When angle between front of the vehicle and target waypoint are near
		protected const float _minimalSpeedForBrakes = 0.5f; // When vehicle is at least at this speed, AI can use brakes

		protected List<Vector3> _AIWaypoints;
		protected BaseController _controller;
		protected int _currentWaypoint;
		protected float _direction = 0f;
		protected float _acceleration = 0f;
		protected Vector3 _targetWaypoint;
		protected RaceManager _raceManager;

		/// <summary>
		/// Initialization
		/// </summary>
		public virtual void Start() 
		{
			_controller = GetComponent<BaseController>();
			_raceManager = FindObjectOfType<RaceManager>();

			// we get the list of AI waypoint
			if (_raceManager.AIWaypoints != null)
			{
				_AIWaypoints = _raceManager.AIWaypoints.GetComponent<Waypoints>().items;
				// the AI will look for the first waypoint in the list
				_currentWaypoint = 0;
				_targetWaypoint = _AIWaypoints[_currentWaypoint];
			}
		}
	
		/// <summary>
		/// At LateUpdate, we apply ou AI logic
		/// </summary>
		public virtual void LateUpdate()
		{
			// if the AI can't control this vehicle, we do nothing and exit
			if (!_controller.IsPlaying || !Active)
			{
				return;
			}

			// we override the AI's steering speed if needed
			if (OverrideSteringSpeed)
			{
				_controller.SteeringSpeed = SteeringSpeed;
			}

			// --------------------------------------------------
			// 1. we determine if the current waypoint is still correct
				
			var distanceToWaypoint = PlaneDistanceToWaypoints();

			// if we are close enough to the current waypoint, we switch to the next one
			if (distanceToWaypoint < MinimalDistanceToNextWaypoint)
			{
				_currentWaypoint++;
				// after one lap, we go back to checkpoint 1
				if (_currentWaypoint == _AIWaypoints.Count)
				{
					_currentWaypoint = 0;
				}
				// we set the new target waypoint
				_targetWaypoint = _AIWaypoints[_currentWaypoint];
			}

			// --------------------------------------
			// 2. Determine direction towards the waypoint
				
			// we compute the target vector between the vehicle and the next waypoint on a plane (without Y axis)
			Vector3 targetVector = _targetWaypoint - transform.position;
			targetVector.y = 0;

			Vector3 transformForwardPlane = transform.forward;
			transformForwardPlane.y = 0;

			// then we measure the angle from vehicle forward to target Vector
			float targetAngleAbsolute = Vector3.Angle(transformForwardPlane, targetVector);

			// we also compute the cross product in order to find out if the angle is positive 
			Vector3 cross = Vector3.Cross(transformForwardPlane, targetVector);

			// this value indicates if the vehicle has to move towards the left or right
			int newDirection = cross.y >= 0 ? 1 : -1;

			// --------------------------------------------------
			// 3. Applies controls to move vehicle towards the waypoint

			// now, we apply _direction & _acceleration values 

			// if the vehicle is looking towards the opposite direction ?
			if (targetAngleAbsolute > _largeAngleDistance)
			{
				// we steer to the proper direction
				_direction = newDirection;

				// if we have enough speed, we brake to rotate faster
				if (_controller.Speed > _minimalSpeedForBrakes)
				{
					_acceleration = -FullThrottle;
				} else
				{
					// otherwise we accelerate slowly
					_acceleration = SmallThrottle;
				}

				// else if the vehicle is not pointing towards the waypoint but also not too far ? 
			} 
			else if (targetAngleAbsolute > _smallAngleDistance)
			{
				// we steer to the proper direction
				_direction = newDirection;
				// we acceleration slowly
				_acceleration = SmallThrottle;

			} 
			else
			{
				// if the vehicle is facing the waypoint, we switch to full speed
				_direction = 0f;
				_acceleration = FullThrottle;
			}

			// we update controller inputs
			_controller.VerticalPosition(_acceleration);
			_controller.HorizontalPosition(_direction);
		}

		/// <summary>
		/// Returns the Plane distance between the next waypoint and the vehicle
		/// </summary>
		/// <returns>The distance to the next waypoint.</returns>
		protected virtual float PlaneDistanceToWaypoints()
		{
			Vector2 target = new Vector2(_targetWaypoint.x, _targetWaypoint.z);
			Vector2 position = new Vector2(transform.position.x, transform.position.z);

			return Vector2.Distance(target, position);
		}	

		/// <summary>
		/// On DrawGizmos, we draw a line between the vehicle and its target
		/// </summary>
		public virtual void OnDrawGizmos() 
		{
			#if UNITY_EDITOR

			// we draw a line between the vehicle & target waypoint
			if (_AIWaypoints != null && (_AIWaypoints.Count >= (_currentWaypoint + 1)))
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(transform.position, _AIWaypoints[_currentWaypoint]);
			}

			#endif
		}

	}

}
