using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;

namespace AdsManagerAPI {
	public class AdmobManager : MonoBehaviour {

		[Header("Android Ids")]
		[SerializeField] private string androidBannerID;
		[SerializeField] private string androidInterstitialID;

		[Header("iOS Ids")]
		[SerializeField] private string iOSBannerID;
		[SerializeField] private string iOSInterstitialID;

		[Header("Banner position (Top or Bottom only)")]
		[SerializeField] internal AdPosition bannerPosition;

		// Banner
		BannerView bannerView;

		// Interstitial
		InterstitialAd interstitial;

		internal void RequestBanner() {
			#if UNITY_ANDROID
			string adUnitId = androidBannerID;
			#elif UNITY_IPHONE
			string adUnitId = iOSBannerID;
			#else
			string adUnitId = "unexpected_platform";
			#endif

			// Create a 320x50 banner at given position
			bannerView = new BannerView(adUnitId, AdSize.Banner, bannerPosition);
			// Create an empty ad request.
			AdRequest request = new AdRequest.Builder().Build();
			// Load the banner with the request.
			bannerView.LoadAd(request);
			HideBanner ();
		}

		internal void ShowBanner() {
			Debug.Log ("Showing admob banner.");
			bannerView.Show ();
		}

		internal void HideBanner() {
			bannerView.Hide ();
		}

		internal void DestroyBanner() {
			if (bannerView != null) {
				bannerView.Destroy ();
			}
		}

		internal void RequestInterstitial() {
			#if UNITY_ANDROID
			string adUnitId = androidInterstitialID;
			#elif UNITY_IPHONE
			string adUnitId = iOSInterstitialID;
			#else
			string adUnitId = "unexpected_platform";
			#endif

			// Initialize an InterstitialAd.
			interstitial = new InterstitialAd(adUnitId);
			// Create an empty ad request.
			AdRequest request = new AdRequest.Builder().Build();
			// Load the interstitial with the request.
			interstitial.LoadAd(request);
		}

		internal bool IsInterstitialLoaded {
			get {
				return interstitial.IsLoaded ();
			}
		}

		internal void ShowInterstitial() {
			if (IsInterstitialLoaded) {
				Debug.Log ("Showing admob interstitial.");
				interstitial.Show ();
			}
			else {
				Debug.Log ("Admob interstitial is not yet ready. It will be shown once it is ready.");
				StartCoroutine (DelayedInterstitialCo ());
			}
		}

		internal void DestroyInterstitial() {
			if (interstitial != null) {
				interstitial.Destroy ();
			}
		}

		private IEnumerator DelayedInterstitialCo() {
			while (!IsInterstitialLoaded) {
				yield return null;
			}

			Debug.Log ("Showing admob interstitial.");
			interstitial.Show ();
		}
	}
}

























