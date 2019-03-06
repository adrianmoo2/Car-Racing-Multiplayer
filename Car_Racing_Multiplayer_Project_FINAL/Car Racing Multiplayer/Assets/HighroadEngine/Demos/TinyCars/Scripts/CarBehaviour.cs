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
		public float SmokesMultiplier = 1f;

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
        public AudioClip ExplosionSound;
        public AudioClip ZeroHealthExplosionSound;
        public AudioClip PitStopSound;

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
        protected PlayerHealth _playerHealth;
        protected PlayerFuel _playerFuel;
        private bool ActorHandled = false;
        private bool PitStopHandler = false;
        private float pitStopAnimLength = 3.5f; //Used to prevent pit stop animation from playing indefinitely. Make slightly longer than animation length.

        enum Collided
        { Tree, Rock, Building, Actor, Bullet, PitStop };
        Collided value = Collided.Tree;

        //---Explosions----
        public GameObject Explosion;
        public GameObject ZeroHealthExplosion;
        //---------------

        private Animation myAnim;

		/// <summary>
		/// Initializes components
		/// </summary>
		protected virtual void Start() 
		{
            _playerHealth = GetComponent<PlayerHealth>();
            _playerFuel = GetComponent<PlayerFuel>();
			_carController = GetComponent<CarController>();

            myAnim = GetComponent<Animation>();

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

			float startSizeMultiplier = (Mathf.Abs(_carController.CurrentGasPedalAmount) * SmokesMultiplier)/8;

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
		/// Used to play crash sound and cause player damage when colliding 
		/// </summary>
		/// <param name="other">Other.</param>
		protected virtual void OnCollisionEnter(Collision other)
		{
			if (CrashSound != null)
			{
				if (other.gameObject.layer != LayerMask.NameToLayer("Ground"))
				{       
                    //----- Player health / collision code -----
                    if (other.gameObject.layer == LayerMask.NameToLayer("Actors") && !ActorHandled)
                    {
                        value = Collided.Actor; 
                        if (_soundManager != null)
                        {
                            _soundManager.PlaySound(CrashSound, transform.position, true);
                        }
                        //Debug.Log("Collided with an actor");
                        StartCoroutine(ActorCollided(0.5f));    //Have to use a coroutine to prevent the player from taking excess damage
                    }
                    else if (other.gameObject.layer == LayerMask.NameToLayer("Collideables") && !ActorHandled)
                    {
                        //Debug.Log("Collided with a collideable");
                        if (other.gameObject.tag != "PitStop")
                        {
                            if (_soundManager != null)
                            {
                                _soundManager.PlaySound(ExplosionSound, transform.position, true);
                            }
                            Destroy(other.gameObject, .1f);
                            GameObject expl = Instantiate(Explosion, other.transform.position, other.transform.rotation);
                            Destroy(expl, 2);
                        }

                        switch (other.gameObject.tag)
                        {
                            case "Tree":
                                value = Collided.Tree;
                                //Debug.Log("Collided with a tree");
                                break;
                            case "Rock":
                                value = Collided.Rock;
                                //Debug.Log("Collided with a rock");
                                break;
                            case "Building":
                                value = Collided.Building;
                                //Debug.Log("Collided with a building");
                                break;
                            case "Bullet":
                                value = Collided.Bullet;
                                break;
                            case "PitStop":
                                value = Collided.PitStop;
                                break;
                            default:
                                break;
                        }
                        StartCoroutine(ActorCollided(0.5f));
                    }
				}
			}
        }

        //Enumerator used to make player take differing amounts of damage only once
        public IEnumerator ActorCollided(float delay)
        {
            int temp = 0;

            ActorHandled = true;
            yield return new WaitForSeconds(delay);

            switch (value)
            {
                case Collided.Tree:
                    //Debug.Log("Collided with Tree. Took 5 damage");
                    temp = _playerHealth.TakeDamage(5);
                    break;
                case Collided.Rock:
                    //Debug.Log("Collided with Rock. Took 10 damage");
                    temp = _playerHealth.TakeDamage(10);
                    break;
                case Collided.Building:
                    //Debug.Log("Collided with Building. Took 15 damage");
                    temp = _playerHealth.TakeDamage(15);
                    break;
                case Collided.Actor:
                    //Debug.Log("Collided with Actor. Took 10 damage");
                    temp = _playerHealth.TakeDamage(5);
                    break;
                case Collided.Bullet:
                    temp = _playerHealth.TakeDamage(5);
                    break;
                case Collided.PitStop:
                    //Debug.Log("Collided with the PitStop. Restored health and fuel.");

                    if (!(PitStopHandler))
                    {
                        PitStopHandler = true;
                        myAnim.Play("PitStopAnimation");
                        StartCoroutine(PitStopCoroutine(pitStopAnimLength));
                    }

                    _playerHealth.restoreHealth();
                    temp = _playerHealth.TakeDamage(0);
                    _playerFuel.refillFuel();
                    break;
                default:
                    break;
            }

            if (temp == 0)
            {
                //Debug.Log("Reached zero health");
                if (_soundManager != null)
                {
                    _soundManager.PlaySound(ExplosionSound, transform.position, true);
                }
                GameObject expl = Instantiate(ZeroHealthExplosion, transform.position, transform.rotation);
                Destroy(expl, 2);
            }

            ActorHandled = false;
        }

        //Pit Stop coroutine necessary to stop animation from playing indefinitely (plays due to collision w/ pit stop)
        public IEnumerator PitStopCoroutine(float pitStopAnimLength)
        {
            if (_soundManager != null)
            {
                _soundManager.PlaySound(PitStopSound, transform.position, true);
            }
            yield return new WaitForSeconds(pitStopAnimLength);

            PitStopHandler = false;
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
