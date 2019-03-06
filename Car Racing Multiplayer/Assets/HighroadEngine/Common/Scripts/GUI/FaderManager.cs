using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{	
	/// <summary>
	/// Handles all GUI effects and changes
	/// </summary>
	public class FaderManager : MonoBehaviour 
	{
		/// the screen used for all fades
		public CanvasGroup Fader;	
			
		/// singleton pattern
		static public FaderManager Instance { get { return _instance; } }
		static protected FaderManager _instance;
		public void Awake()
		{
			_instance = this;
		}
						
		/// <summary>
		/// Fades the fader in or out depending on the state
		/// </summary>
		/// <param name="state">If set to <c>true</c> fades the fader in, otherwise out if <c>false</c>.</param>
		public virtual void FaderOn(bool state,float duration)
		{
			if (Fader==null)
			{
				return;
			}
			Fader.gameObject.SetActive(true);
			if (state)
			{
				StartCoroutine(MMFade.FadeCanvasGroup(Fader,duration, 1f));
			}
			else
			{
				StartCoroutine(MMFade.FadeCanvasGroup(Fader,duration, 0f));
			}
		}		

		/// <summary>
		/// Fades the fader to the alpha set as parameter
		/// </summary>
		/// <param name="newColor">The color to fade to.</param>
		/// <param name="duration">Duration.</param>
		public virtual void FaderTo(float newOpacity,float duration)
		{
			if (Fader==null)
			{
				return;
			}
			Fader.gameObject.SetActive(true);
			StartCoroutine(MMFade.FadeCanvasGroup(Fader,duration, newOpacity));
		}	
	}
}