using UnityEngine;
using System.Collections;

namespace MoreMountains.HighroadEngine
{	
	/// <summary>
	/// Simple class to allow the player to select a scene on the start screen
	/// </summary>
	public class StartScreen : MonoBehaviour 
	{
		[Header("Racing Game")]
		/// the name of the basic racing game
		public string LocalGameSceneName;
		/// the name of the basic racing game / online version
		public string OnlineGameSceneName;

		[Header("Aphex")]
		/// the name of the aphex scene
		public string LocalGameAphexSceneName;
		/// the name of the aphex scene / online version
		public string OnlineGameAphexSceneName;

		public virtual void OnLocalGameClick()
		{
			RemoveBackgroundGame();
			LoadingSceneManager.LoadScene(LocalGameSceneName);
		}

		public virtual void OnOnlineGameClick()
		{
			RemoveBackgroundGame();
			LoadingSceneManager.LoadScene(OnlineGameSceneName);
		}

		public virtual void OnLocalGameAphexClick()
		{
			RemoveBackgroundGame();
			LoadingSceneManager.LoadScene(LocalGameAphexSceneName);
		}

		public virtual void OnOnlineGameAphexClick()
		{
			RemoveBackgroundGame();
			LoadingSceneManager.LoadScene(OnlineGameAphexSceneName);
		}

		protected virtual void RemoveBackgroundGame()
		{
			// We need to remove LocalLobby since it's a persistent object
			Destroy(LocalLobbyManager.Instance.gameObject);
		}
	}
}
