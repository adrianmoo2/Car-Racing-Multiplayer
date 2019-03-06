#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace MoreMountains.HighroadEngine
{
	/// <summary>
	/// This class adds order number and handles for each checkpoint on the scene view, for easier setup.
	/// It also shows start positions.
	/// </summary>
	[CustomEditor(typeof(RaceManager))]
	[InitializeOnLoad]
	public class RaceManagerEditor : Editor 
	{		
		/// <summary>
		/// Draws repositionable handles at every point in the path, for easier setup
		/// </summary>
		public void OnSceneGUI()
		{
			Handles.color = Color.green;
			RaceManager t = (target as RaceManager);

			if (t.Checkpoints != null)
			{
				for (int i = 0; i < t.Checkpoints.Length; i++)
				{
					if (t.Checkpoints[i] != null)
					{
						// draws the path item number
						GUIStyle style = new GUIStyle();
						style.normal.textColor = Color.red;	 
						Handles.Label(t.Checkpoints[i].transform.position + (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "CP-" + i, style);
					}
				} 
			}

			for (int i = 0; i < t.StartPositions.Length; i++)
			{
				Vector3 oldPoint = t.StartPositions[i];

				Handles.color = Color.magenta;

				// we draw the start angle
				Handles.ConeCap(0, oldPoint, Quaternion.AngleAxis(t.StartAngleDegree, Vector3.up), 2f);

				Handles.color = Color.green;

				EditorGUI.BeginChangeCheck();

				// we draw the path item number
				GUIStyle style = new GUIStyle();
				style.normal.textColor = Color.gray;	 
				Handles.Label(t.StartPositions[i] + (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "start-" + (i + 1), style);

				// we draw a movable handle
				Vector3 newPoint = Handles.PositionHandle(oldPoint, Quaternion.identity);

				// records changes
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(target, "Start Position Move Handle");
					t.StartPositions[i] = newPoint;
				}
			}    
		}
	}
}
#endif