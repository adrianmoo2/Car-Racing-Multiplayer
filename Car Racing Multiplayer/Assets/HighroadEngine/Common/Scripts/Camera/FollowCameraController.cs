using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine 
{
	/// <summary>
	/// Camera controller following a target or a group of targets
	/// with en orthographic view.
	/// </summary>
    public class FollowCameraController : CameraController 
	{
		[Header("Targets")]
		[Information("When human players are active, do you want the camera to also follow bots?",InformationAttribute.InformationType.Info,false)]
		/// If set to true, camera will follow humans and bot players on the track.
		public bool FollowBotPlayers;

		[Header("Camera Controls")]
		/// the time (in seconds) before the camera starts to focus on the targets. Allows a smooth follow effect 
        public float DampTime = 0.2f; 
		[Information("For a single fast velocity object, a low damp time value will make the screen moving too fast. Use this different value to have a smooth movement.", InformationAttribute.InformationType.Info, false)]
		/// the time (in seconds) it takes the camera to focus when the target is a single vehicle
		public float SingleDampTime = 1f;
		/// the space added around the rectangle made by two most distant cars
        public float ScreenEdgeSize = 4f;
		/// the maximal zoom value
        public float CameraMinimalSize = 6.5f;
    
		[Header("Camera Single Human Player Controls")]
		/// Minimal zoom size
		public float CameraMaximalSingleSize = 6.5f;
		/// Acceleration offset.The bigger the value, the more ahead the camera will be when car is racing
		public float OffsetSingleCar = 1f;
		/// Multiplied by vehicle speed to change zoom of the camera
		public float ZoomSingleCar = 2f;

		protected float _zoomDampSpeed;
		protected Vector3 _moveVelocityReference;
		protected Vector3 _cameraTargetPosition;
		protected GameObject _singleTarget;

		/// <summary>
		/// Determines if this camera can be used with multiple targets or a single one
		/// </summary>
		/// <value><c>true</c> if this instance is a single player camera; otherwise, <c>false</c>.</value>
		public override bool IsSinglePlayerCamera 
		{
			get { return false; }
		}

		/// <summary>
		/// Return Damp Time depending on a single target or more
		/// </summary>
		/// <returns>The damp time value.</returns>
		protected virtual float CorrectDampTime()
		{
			if (_singleTarget != null)
			{
				return SingleDampTime;
			}

			return DampTime;
		}

		/// <summary>
		/// Camera position update
		/// </summary>
		protected virtual void LateUpdate() 
		{
			// We need a list of targets to follow
			if (HumanPlayers == null && BotPlayers == null)
			{
				return;
			}

			// change camera position
            EvaluatePosition();

            // Change camera orthographic size
            EvaluateSize();
        }

		/// <summary>
		/// Moves the camera
		/// </summary>
		protected virtual void EvaluatePosition()  
		{
            // Calculate the average position of the vehicles.
            EvaluateAveragePosition();

            // Change camera position with a smooth damp
			transform.position = Vector3.SmoothDamp(transform.position, _cameraTargetPosition, ref _moveVelocityReference, CorrectDampTime());
        }

		/// <summary>
		/// Finds the average position.
		/// </summary>
		protected virtual void EvaluateAveragePosition()
        {
			Vector3 averagePosition = new Vector3();
			_singleTarget = null;
			int targetsCounter = 0;

            // Iterate human players
            for (int i = 0; i < HumanPlayers.Length; i++)
            {
				if (HumanPlayers[i] != null)
				{
					averagePosition += HumanPlayers[i].position;
					targetsCounter++;
				}
            }

			// In case camera follow bots too or no human players active
			if (FollowBotPlayers || HumanPlayers.Length == 0)
			{
				for (int i = 0; i < BotPlayers.Length; i++)
				{
					if (BotPlayers[i] != null)
					{
						averagePosition += BotPlayers[i].position;
						targetsCounter++;
					}
				}
			}
					
	        if (targetsCounter > 0)
			{
				// Average position is divided by number of targets
				averagePosition /= targetsCounter;
			}

			// When we only have one target, we center the view on that target
			if (targetsCounter == 1) 
			{
				if (HumanPlayers.Length == 1)
				{
					_singleTarget = HumanPlayers[0].gameObject;
				}
				else
				{
					_singleTarget = BotPlayers[0].gameObject;
				}

				// Position have an offset depending on offset value and speed of the vehicle
				BaseController controller = _singleTarget.GetComponent<BaseController>();
				Vector3 vehicle = _singleTarget.transform.forward * controller.Speed * OffsetSingleCar;
				averagePosition += vehicle;
			}

			// new camera desired position is set
            _cameraTargetPosition = averagePosition;
        }

		/// <summary>
		/// Zoom the camera
		/// </summary>
        protected virtual void EvaluateSize() 
		{
			float newSize;

			// In single target mode, size depends on vehicle speed
			if (_singleTarget != null)
			{
				// When only one target, the zoom is linear to the speed of the car
				BaseController controller = _singleTarget.GetComponent<BaseController>();
				newSize = Mathf.Max(CameraMinimalSize, controller.Speed * ZoomSingleCar);
				newSize = Mathf.Min(CameraMaximalSingleSize, newSize );
			}
			else
			{
				newSize = EvaluateNewSize();
			}

			// Sets camera size
			_camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, newSize, ref _zoomDampSpeed, CorrectDampTime());
        }

		/// <summary>
		/// Finds the required size of the zoom.
		/// </summary>
		/// <returns>The required size.</returns>
		protected virtual float EvaluateNewSize() 
		{
			Vector3 localPosition = transform.InverseTransformPoint(_cameraTargetPosition);

			// This float will store best size found
			float newSize = 0f;

			// For each human player, we find minimal viable size
			for (int i = 0; i < HumanPlayers.Length; i++)
			{
				if (HumanPlayers[i] != null)
				{
					Vector3 vehicleLocalPosition = transform.InverseTransformPoint(HumanPlayers[i].position);
					Vector3 cameraToVehicleVector = vehicleLocalPosition - localPosition;

					// new size is the largest value between distance to y or x
					newSize = Mathf.Max(newSize, Mathf.Abs(cameraToVehicleVector.y));
					newSize = Mathf.Max(newSize, Mathf.Abs(cameraToVehicleVector.x) / _camera.aspect);
				}
			}

			if (FollowBotPlayers || HumanPlayers.Length == 0)
			{
				for (int i = 0; i < BotPlayers.Length; i++)
				{
					if (BotPlayers[i] != null)
					{
						Vector3 vehicleLocalPosition = transform.InverseTransformPoint(BotPlayers[i].position);
						Vector3 cameraToVehicleVector = vehicleLocalPosition - localPosition;

						// new size is the largest value between distance to y or x
						newSize = Mathf.Max(newSize, Mathf.Abs(cameraToVehicleVector.y));
						newSize = Mathf.Max(newSize, Mathf.Abs(cameraToVehicleVector.x) / _camera.aspect);
					}
				}
			}

			// We add the size buffer to the new size
			newSize += ScreenEdgeSize;

			// New site can't go below the camera minimal size value
			newSize = Mathf.Max(newSize, CameraMinimalSize);

			return newSize;
		}
    }
}