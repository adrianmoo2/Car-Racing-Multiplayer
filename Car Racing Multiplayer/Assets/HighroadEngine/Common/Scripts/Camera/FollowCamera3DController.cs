using UnityEngine;

namespace MoreMountains.HighroadEngine 
{
	/// <summary>
	/// 3D Camera controller following a single target in perspective
	/// </summary>
    public class FollowCamera3DController : CameraController 
	{
		[Header("Camera Controls")]
		/// distance to the target
		public float Distance = 8.0f;
		/// height between target and camera
		public float Height = 4.0f;
		/// damping translation of the camera
		public float DampingPosition = 1.0f;
		/// damping steering lateral translation
		public float DampingSteering = 0.5f;
		/// steering translation impact to camera
		public float SteeringOffset = 1.0f;
		/// camera LookAt target offset
		public float TargetLookUpOffset = 2f;
		/// this type of camera can only follow one target
		public override bool IsSinglePlayerCamera 
		{
			get { return true; }
		}

		protected Transform _target;
		protected BaseController _baseController;
		protected Vector3 currentLateralOffset = Vector3.zero;
		protected Vector3 _moveVelocityReference;
		protected Vector3 _targetPosition;
		protected Vector3 _targetLateralTranslation;

		/// <summary>
		/// Lateral speed is zero at start.
		/// </summary>
		public virtual void Start()
		{
			currentLateralOffset = Vector3.zero;
		}

		/// <summary>
		/// Updates the camera position
		/// </summary>
		protected virtual void LateUpdate() 
		{
			// we identify which target we want to follow
			if (!_target)
			{
				if (HumanPlayers.Length > 0)
				{
					_target = HumanPlayers[0];
				}
				else if (BotPlayers.Length > 0)
				{
					_target = BotPlayers[0];
				}

				if (_target != null)
				{
					_baseController = _target.GetComponent<BaseController>();
				}
			}

			// if we didn't find any target, we do nothing and exit
			if (_target == null)
			{
				return;
			}

			// the target position is computed depending on the vehicle's position and camera parameters
			_targetPosition = _target.transform.position 
				- (_target.transform.forward * Distance) 
				+ (_target.transform.up * Height);

			// we change camera position with a smooth damp
			_camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, _targetPosition, ref _moveVelocityReference, DampingPosition);

			// we compute the new lateral translation value to smooth the vehicle's rotation
			_targetLateralTranslation = _target.transform.right * (_baseController.CurrentSteeringAmount * SteeringOffset);

			// we save the current value in the camera GameObject in anticipation of the next Update
			currentLateralOffset = Vector3.Lerp(currentLateralOffset, _targetLateralTranslation, Time.deltaTime * DampingSteering);

			// we make the camera look at the vehicle, modified by the lateral and up offsets
			_camera.transform.LookAt(currentLateralOffset + _target.transform.position + (_target.up * TargetLookUpOffset));
			return;
		}
    }
}