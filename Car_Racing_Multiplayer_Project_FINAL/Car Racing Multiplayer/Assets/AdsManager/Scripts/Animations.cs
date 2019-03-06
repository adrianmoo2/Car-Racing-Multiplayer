using UnityEngine;
using System.Collections;

namespace AdsManagerAPI {
	public class Animations : MonoBehaviour {

		public void InterstitialOut_Ended() {
			CustomAdsManager cam = transform.root.GetComponent<CustomAdsManager> ();

			if (cam) {
				cam.SetRaycastTarget (false);
				cam.EnableInterstitialHolder (false);
			}
		}

	}
}




























