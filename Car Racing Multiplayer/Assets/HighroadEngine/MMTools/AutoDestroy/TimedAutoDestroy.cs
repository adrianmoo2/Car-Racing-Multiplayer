using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Add this component to an object and it'll be auto destroyed X seconds after its Start()
	/// </summary>
	public class TimedAutoDestroy : MonoBehaviour
	{
	    /// The time (in seconds) before we destroy the object
	    public float TimeBeforeDestruction=2;

		/// <summary>
		/// On Start(), we schedule the object's destruction
		/// </summary>
		protected virtual void Start ()
	    {
	        StartCoroutine(Destruction());
		}
		
		/// <summary>
		/// Destroys the object after TimeBeforeDestruction seconds
		/// </summary>
	    protected virtual IEnumerator Destruction()
	    {
	        yield return new WaitForSeconds(TimeBeforeDestruction);
	        Destroy(gameObject);
	    }
	}
}
