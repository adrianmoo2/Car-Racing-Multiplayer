using UnityEngine;
using System.Collections;
using UnityEditor;
using ChartboostSDK;

namespace AdsManagerAPI {
	[CustomEditor(typeof(ChartboostManager))]
	public class ChartboostSettingsButton : Editor {

	    public override void OnInspectorGUI()
	    {
	        EditorGUILayout.Space();
	        if (GUILayout.Button("Set Ids"))
	        {
				CBSettings.Edit ();
	        }
	    }
	}
}
