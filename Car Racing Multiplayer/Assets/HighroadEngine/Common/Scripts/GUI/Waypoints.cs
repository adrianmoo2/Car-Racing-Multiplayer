using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine {

	/// <summary>
	/// Manages waypoints of the AI.
	/// The AI will follow each point of the items collection in order and loops.
	/// </summary>
	public class Waypoints : MonoBehaviour 
	{
		/// the points that make up the path the object will follow
		[Information("Add points to the <b>Path</b> (set the size of the path first), then position the points using either the inspector or by moving the handles directly in scene view. The order of the points will be the order the object follows.\n", InformationAttribute.InformationType.Info, false)]
		[Header("Path")]
		/// the list of items
		public List<Vector3> items;

		/// <summary>
		/// On DrawGizmos, we draw lines to show the path the object will follow
		/// </summary>
		protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR
			if (items == null)
			{
				return;
			}

			if (items.Count == 0)
			{
				return;
			}

			Gizmos.color = Color.yellow;
				
			// for each point in the path
			for (int i = 0; i < items.Count; i++)
			{
				// we draw a green point 
				Gizmos.DrawSphere(items[i], 0.5f);

				// we draw a line towards the next point in the path
				if ((i + 1) < items.Count)
				{
					MMDebug.GizmosDrawArrow(items[i], items[i + 1] - items[i], Color.yellow);
				}
				// we draw a line from the first to the last point if we're looping
				if ( (i == items.Count - 1))
				{
					MMDebug.GizmosDrawArrow(items[0], items[i] - items[0], Color.yellow);
				}
			}
			#endif
		}
	}
}