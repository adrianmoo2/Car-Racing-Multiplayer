using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MoreMountains.HighroadEngine
{	
	/// <summary>
	/// Simple start screen class.
	/// </summary>
	public class SplashScreen : MonoBehaviour 
	{
		/// the level to load after the splash screen
		public string FirstLevel;
		/// the delay (in seconds) after which it should go to the next level
		public float AutoSkipDelay=2f;

	    protected float _delayAfterClick=1f;

	    /// <summary>
	    /// Initialization
	    /// </summary>
	    protected virtual IEnumerator Start()
		{	
			FaderManager.Instance.Fader.alpha=1f;
			FaderManager.Instance.FaderOn(false,1f);

			while (Application.isShowingSplashScreen)
			{
				yield return null;
			}
				
			if (AutoSkipDelay>1f)
			{
				_delayAfterClick=AutoSkipDelay;
				StartCoroutine(LoadFirstLevel());
			}
		}

		/// <summary>
		/// Loads the level specified in the inspector
		/// </summary>
		/// <returns>The first level.</returns>
	    protected virtual IEnumerator LoadFirstLevel()
		{
			yield return new WaitForSeconds(_delayAfterClick);
			FaderManager.Instance.FaderOn(true,1f);
			yield return new WaitForSeconds(1f);
			LoadingSceneManager.LoadScene(FirstLevel);
		}		
	}
}
