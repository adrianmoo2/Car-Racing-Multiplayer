using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// This class acts as an interface to allow the demo levels to work whether the environment (colliders, rigidbodies) are set as 2D or 3D.
	/// If you already know for sure that you're going for a 2D or 3D game, I suggest you replace the use of this class with the appropriate classes.
	/// </summary>
	public class RigidbodyInterface : MonoBehaviour 
	{	
		/// <summary>
		/// Returns the rigidbody's position
		/// </summary>
		/// <value>The position.</value>
		public Vector3 position
	    {
	        get
	        {
	            if (_rigidbody2D != null)
	            {
	                return _rigidbody2D.position;
	            }
	            if (_rigidbody != null)
	            {
	                return _rigidbody.position;
	            }
	            return Vector3.zero;
	        }
	        set { }
	    }

		public Rigidbody2D InternalRigidBody2D 
		{
			get {
				return _rigidbody2D;
			}
		}

		public Rigidbody InternalRigidBody 
		{
			get {
				return _rigidbody;
			}
		} 

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Awake () 
		{
			// we check for rigidbodies, and depending on their presence determine if the interface will work with 2D or 3D rigidbodies and colliders.
			_rigidbody2D=GetComponent<Rigidbody2D>();
			_rigidbody=GetComponent<Rigidbody>();
			
			if (_rigidbody2D != null) 
			{
				_mode="2D";
			}
			if (_rigidbody != null) 
			{
				_mode="3D";
			}
			if (_rigidbody==null && _rigidbody2D==null)
			{
				Debug.LogWarning("A RigidBodyInterface has been added to "+gameObject+" but there's no Rigidbody or Rigidbody2D on it.", gameObject);
			}
		}

		/// <summary>
		/// Gets or sets the velocity of the rigidbody associated to the interface.
		/// </summary>
		/// <value>The velocity.</value>
		public Vector3 Velocity 
		{
			get 
			{ 
				if (_mode == "2D") 
				{
					return(_rigidbody2D.velocity);
				}
				else 
				{
					if (_mode == "3D") 
					{
						return(_rigidbody.velocity);
					}
					else
					{
						return new Vector3(0,0,0);
					}
				}
			}
			set 
			{
				if (_mode == "2D") {
					_rigidbody2D.velocity = value;
				}
				if (_mode == "3D") {
					_rigidbody.velocity = value;
				}
			}
		}

		protected string _mode;
		protected Rigidbody2D _rigidbody2D;
		protected Rigidbody _rigidbody;
		
		/// <summary>
		/// Adds the specified force to the rigidbody associated to the interface..
		/// </summary>
		/// <param name="force">Force.</param>
		public virtual void AddForce(Vector3 force)
		{
			if (_mode == "2D") 
			{
				_rigidbody2D.AddForce(force,ForceMode2D.Impulse);
			}
			if (_mode == "3D")
			{
				_rigidbody.AddForce(force);
			}
		}

	    /// <summary>
	    /// Move the rigidbody to the position vector specified
	    /// </summary>
	    /// <param name="newPosition"></param>
	    public virtual void MovePosition(Vector3 newPosition)
	    {
	        if (_mode == "2D")
	        {
	            _rigidbody2D.MovePosition(newPosition);
	        }
	        if (_mode == "3D")
	        {
	            _rigidbody.MovePosition(newPosition);
	        }
	    }



		public virtual void ResetAngularVelocity()
		{
			if (_mode == "2D")
			{
				_rigidbody2D.angularVelocity = 0;
			}
			if (_mode == "3D")
			{
				_rigidbody.angularVelocity = Vector3.zero;
			}	
		}

		public virtual void ResetRotation()
		{
			if (_mode == "2D")
			{
				_rigidbody2D.rotation = 0;
			}
			if (_mode == "3D")
			{
				_rigidbody.rotation = Quaternion.identity;
			}	
		}
			
		
		/// <summary>
		/// Determines whether the rigidbody associated to the interface is kinematic
		/// </summary>
		/// <returns><c>true</c> if this instance is kinematic the specified status; otherwise, <c>false</c>.</returns>
		/// <param name="status">If set to <c>true</c> status.</param>
		public virtual void IsKinematic(bool status)
		{
			if (_mode == "2D") 
			{
				GetComponent<Rigidbody2D>().isKinematic=status;
			}
			if (_mode == "3D")
			{			
				GetComponent<Rigidbody>().isKinematic=status;
			}
		}
		
		/// <summary>
		/// Enables the box collider associated to the interface.
		/// </summary>
		/// <param name="status">If set to <c>true</c> status.</param>
		public virtual void EnableBoxCollider(bool status)
		{
			if (_mode == "2D") 
			{
				GetComponent<Collider2D>().enabled=status;
			}
			if (_mode == "3D")
			{			
				GetComponent<Collider>().enabled=status;
			}
		}

		/// <summary>
		/// Use this to check if you're dealing with a 3D object
		/// </summary>
		/// <value><c>true</c> if this instance is3 d; otherwise, <c>false</c>.</value>
		public bool Is3D 
		{ 
			get
	        {
				if (_mode=="3D") 
				{ 
					return true; 
				} 
				else 
				{ 
					return false; 
				}
			}
		}

		/// <summary>
		/// Use this to check if you're dealing with a 2D object
		/// </summary>
		/// <value>The position.</value>
		public bool Is2D 
		{ 
			get
	        {
				if (_mode=="2D") 
				{ 
					return true; 
				} 
				else
				{ 
					return false; 
				}
			}
		}
	}
}
