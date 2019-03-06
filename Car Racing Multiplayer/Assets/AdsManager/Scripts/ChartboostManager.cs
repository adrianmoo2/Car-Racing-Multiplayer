using UnityEngine;
using System.Collections;
using ChartboostSDK;

namespace AdsManagerAPI {
	public class ChartboostManager : MonoBehaviour {

		internal void RequestInterstitial() {
		//	Debug.Log ("Chartboost requested.");
			Chartboost.cacheInterstitial (CBLocation.Default);
		}

		internal void ShowInterstitial() {
			Debug.Log ("Showing chartboost interstitial.");
			Chartboost.showInterstitial (CBLocation.Default);
		}
	}
}





















