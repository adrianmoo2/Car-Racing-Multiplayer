using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{	
	/// <summary>
	/// Add this class to a GameObject to have it play a background music when instanciated.
	/// Careful : only one background music will be played at a time.
	/// </summary>
	public class BackgroundMusic : MonoBehaviour
	{
		/// the sound clip to play
		public AudioClip SoundClip;
	    
		protected AudioSource _source;
		protected SoundManager _soundManager;

	    /// <summary>
	    /// Gets the AudioSource associated to that GameObject, and asks the GameManager to play it.
	    /// </summary>
	    public virtual void Start() 
		{
			_source = gameObject.AddComponent<AudioSource>() as AudioSource;	
			_source.playOnAwake = false;
			_source.spatialBlend = 0;
			_source.rolloffMode = AudioRolloffMode.Logarithmic;
			_source.loop = true;
		
			_source.clip=SoundClip;

			_soundManager = FindObjectOfType<SoundManager>();

			if (_soundManager == null)
			{
				Debug.LogWarning("BackgroundMusic need a SoundManager gameObject in the scene to play music. Please add one.");
				return;
			}
			
			_soundManager.PlayBackgroundMusic(_source);
		}
	}
}