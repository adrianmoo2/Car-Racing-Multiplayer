using System.Collections;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{
    [RequireComponent(typeof(CarController))]
    public class ObjectCollision : MonoBehaviour
    {
        protected PlayerHealth _playerHealth;

        protected void Awake()
        { 
            _playerHealth = GetComponent<PlayerHealth>();
        }
        void OnCollisionEnter(Collision other)
        {
            Debug.Log("Collided with a Tree");
            if (other.gameObject.layer == LayerMask.NameToLayer("Actors"))
            {
                Debug.Log("Collided with a Tree");
                //_playerHealth.TakeDamage(5);
                Destroy(gameObject, .1f);
            }
        }
    }
}
