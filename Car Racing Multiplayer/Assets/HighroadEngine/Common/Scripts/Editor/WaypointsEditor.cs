#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// This class adds order number and handles for each Waypoint on the scene view, for easier setup.
	/// </summary>
	[CustomEditor(typeof(Waypoints))]
	[InitializeOnLoad]
	public class WaypointsEditor : Editor 
	{		
		/// <summary>
		/// Draws repositionable handles at every point in the path, for easier setup.
		/// </summary>
		public void OnSceneGUI()
		{
			Handles.color = Color.green;
			Waypoints t = (target as Waypoints);

			for (int i = 0; i < t.items.Count; i++)
			{
				EditorGUI.BeginChangeCheck();

				Vector3 oldPoint = t.items[i];
				GUIStyle style = new GUIStyle();

				// we draw the path item number
				style.normal.textColor = Color.yellow;	 
				Handles.Label(t.items[i] + (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "AI-" + i, style);

				// we draw a movable handle
				Vector3 newPoint = Handles.PositionHandle(oldPoint, Quaternion.identity);

				// records changes
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(target, "Waypoint Move Handle");
					t.items[i] = newPoint;
				}
			}	        
		}
	}
}
#endif