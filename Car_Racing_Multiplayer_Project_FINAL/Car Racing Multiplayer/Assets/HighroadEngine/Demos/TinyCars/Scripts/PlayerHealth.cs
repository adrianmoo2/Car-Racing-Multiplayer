using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using MoreMountains.Tools;


namespace MoreMountains.HighroadEngine
{

    [RequireComponent(typeof(BaseController))]
    public class PlayerHealth : BaseController
    {
        public int startingHealth = 100;
        public int currentHealth;
        public Slider HealthSlider;
        //public float healthFloat; Implement later if really desired

        Image myHealth;                             //Image component
        public Sprite myHealth_Normal;              //Sprite for normal health level
        public Sprite myHealth_Low;                 //Sprite for low health
        public Sprite myHealth_Critical;            //Sprite for critical fuel

        protected CarController _carController;
        protected VehicleAI _vehicleAI;
        protected PlayerFuel _playerFuel;

        public GameObject Explosion;
        public AudioClip ExplosionSound;

        // Use this for initialization
        protected override void Awake()
        {
            //Calling BaseController's Awake function
            base.Awake();

            //Setting up references
            _carController = GetComponent<CarController>();
            _vehicleAI = GetComponent<VehicleAI>();
            _playerFuel = GetComponent<PlayerFuel>();

            //Acquiring Slider and Health image GameObjects if the player is not a bot
            if (!(_vehicleAI.Active))
            {
                HealthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
                myHealth = GameObject.Find("Health").GetComponent<Image>();
            }

            //Set the initial health of the player
            currentHealth = startingHealth;

        }

        // Update is called once per frame
        public int TakeDamage(int damage)
        {
            currentHealth = (currentHealth -= damage) < 0 ? 0 : currentHealth -= damage;

            if (!(_vehicleAI.Active))
            {
                HealthSlider.value = currentHealth;
            }

            if (currentHealth > 35)
            {
                if (!(_vehicleAI.Active))
                {
                    myHealth.sprite = myHealth_Normal;
                }
                _playerFuel.playerFuelVelocity = 20f;
                
            }
            else if (currentHealth > 15 && currentHealth <= 35)
            {
                if (!(_vehicleAI.Active))
                {
                    myHealth.sprite = myHealth_Low;
                }
                _playerFuel.playerFuelVelocity = 15f;
            }
            else if (currentHealth <= 15)
            {
                if (!(_vehicleAI.Active))
                {
                    myHealth.sprite = myHealth_Critical;
                }
                _playerFuel.playerFuelVelocity = 5f;
            }
            return currentHealth;
        }

        public void restoreHealth()
        {
            currentHealth = 100;
        }
    }
}
