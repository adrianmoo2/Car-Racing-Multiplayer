using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{

    [RequireComponent(typeof(BaseController))]
    public class PlayerFuel : BaseController 
    {
        public int startingFuel = 100;                            // The amount of fuel the player starts the game with.
        public int currentFuel;                                   // The current fuel the player has.
        public Slider FuelSlider;                                 // Reference to the UI's fuel bar.
        public float fuelFloat;                                   // Float that determines the rate of fuel consumption.

        Image myFuel;                                             // Image Component
        public Sprite myFuel_Normal;                              //Sprite for normal fuel level
        public Sprite myFuel_Low;                                 // Sprite for low fuel
        public Sprite myFuel_Critical;                            // Sprite for critical fuel

        protected CarController _carController;
        protected VehicleAI _vehicleAI;
        protected PlayerHealth _playerHealth;

        public float playerFuelVelocity;                          //Float to communicate maximum car velocity with PlayerHealth script

        private bool fuelHandled = false;

        protected override void Awake()
        {
            //Calling BaseController's Awake function
            base.Awake();

            // Setting up the references.
            _carController = GetComponent<CarController>();
            _vehicleAI = GetComponent <VehicleAI>();
            _playerHealth = GetComponent<PlayerHealth>();

            //Acquiring Slider and Fuel image GameObjects if the player is not a bot
            if (!(_vehicleAI.Active))
            {
                FuelSlider = GameObject.Find("FuelSlider").GetComponent<Slider>();
                myFuel = GameObject.Find("Fuel").GetComponent<Image>();
            }
            //myFuel = GetComponent<Image>();

            fuelFloat = 0.3f;

            // Set the initial fuel of the player.
            currentFuel = startingFuel;

            //Initializing the PlayerFuel's communicated velocity value
            playerFuelVelocity = 20f;
        }

        public void Update()
        {

            // Reduce the current fuel by 1 if accelerating. Use Coroutines to delay the update. 
            // Increase float value to decrease fuel consumption
            if (!fuelHandled)
            {
                StartCoroutine(LoseFuel(fuelFloat));
                //StartCoroutine(LoseFuel(0.5f));
            }

            LoseSpeed();
        }

        public IEnumerator LoseFuel(float delay)
        {
            fuelHandled = true;
            yield return new WaitForSeconds(delay);
            /*Debug.Log("CurrentSteeringAmount: " + _carController.CurrentSteeringAmount);*/
            //Debug.Log("CurrentGasPedalAmount: " + _carController.CurrentGasPedalAmount);
            //Debug.Log("IsPlaying: " + _carController.IsPlaying);

            //Decreases fuel if the player is accelerating past a certain threshold
            if (_carController.CurrentGasPedalAmount > 0.3 && currentFuel > 1)
            {
                currentFuel -= 1;
            }

            // Set the fuel bar's value to the current fuel if the player is not a bot.
            //Debug.Log("active status: " + _vehicleAI.Active);
            if (!(_vehicleAI.Active))
            {
                FuelSlider.value = currentFuel;
            }

            fuelHandled = false;
        }

        public void LoseSpeed()
        {
            if (currentFuel > 35)
            {
                myFuel.sprite = myFuel_Normal;
                fuelFloat = 0.3f;
                _carController.MaximumVelocity = (playerFuelVelocity < 20f) ? playerFuelVelocity : 20f;
            }
            else if (currentFuel > 15 && currentFuel <= 35)
            {
                //Debug.Log("Losing Minor Speed");
                myFuel.sprite = myFuel_Low;
                fuelFloat = 0.6f;
                _carController.MaximumVelocity = (playerFuelVelocity < 15f) ? playerFuelVelocity : 15f;
            }
            else if (currentFuel <= 15)
            {
                //Debug.Log("Losing Major Speed");
                myFuel.sprite = myFuel_Critical;
                fuelFloat = 0.9f;
                _carController.MaximumVelocity = (playerFuelVelocity < 5f) ? playerFuelVelocity : 5f;
            }
        }

        public void refillFuel()
        {
            currentFuel = 100;
            myFuel.sprite = myFuel_Normal;
            _carController.MaximumVelocity = 20f;
        }
    }
}