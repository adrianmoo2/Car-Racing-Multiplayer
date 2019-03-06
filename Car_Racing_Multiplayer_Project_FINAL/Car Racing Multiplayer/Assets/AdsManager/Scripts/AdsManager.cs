using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace AdsManagerAPI {
	public class AdsManager : MonoBehaviour {

		private AdsManager() {

		}

		private static AdsManager instance = null;
		public static AdsManager Instance {
			get {
				return instance;
			}
		}

		// enums
		internal enum AdProvider {
			CUSTOM,
			ADMOB,
			CHARTBOOST,
			NONE
		}

		internal bool bannerStatus = true;

		private AdProvider currentBanner = AdProvider.CUSTOM;
		internal List<AdProvider> bannersList;
		internal void InitBannersList() {
			if (bannersList == null) {
				bannersList = new List<AdProvider> ();
			}
			else {
				bannersList.Clear ();
			}
		}

		internal AdProvider StartupAd {
			get;
			set;
		}

		internal int AdInterval {
			get;
			set;
		}

		internal List<AdProvider> adList;
		internal void InitAdList() {
			if (adList == null) {
				adList = new List<AdProvider> ();
			}
			else {
				adList.Clear ();
			}
		}
			
		private AdProvider apFor3 = AdProvider.CUSTOM;

		internal AdProvider apFor2;
		internal AdProvider ap1;
		internal AdProvider ap2;

		[SerializeField] private GameObject chartboostPrefab;

		[SerializeField] internal CustomAdsManager customAdsManager;
		[SerializeField] internal AdmobManager admobManager;
		[SerializeField] internal ChartboostManager chartboostManager;

		private bool inited = false;

		private int count = 0;

		void Awake() {
			if (instance == null) {
				instance = this;
				DontDestroyOnLoad (this);
			}
			else if (instance != this) {
				Destroy(gameObject);
			}
		}

		// Use this for initialization
		void Start () {
			Init ();
		}

		private void Init() {
			if (!inited) {
				Instantiate (chartboostPrefab);

				if (!customAdsManager) {
					customAdsManager = GetComponent<CustomAdsManager> ();
				}
				if (!admobManager) {
					admobManager = GetComponent<AdmobManager> ();
				}
				if (!chartboostManager) {
					chartboostManager = GetComponent<ChartboostManager> ();
				}

				customAdsManager.RequestScheme ();

				inited = true;
			}
		}

		internal void ShowStartupAd() {
			if (StartupAd == AdProvider.CUSTOM) {
				Debug.Log ("Showing startup interstitial.");
				customAdsManager.ShowInterstitial ();
			}
			else if (StartupAd == AdProvider.ADMOB) {
				Debug.Log ("Showing startup interstitial.");
				admobManager.ShowInterstitial ();
				admobManager.RequestInterstitial ();
			}
			else if (StartupAd == AdProvider.CHARTBOOST) {
				Debug.Log ("Showing startup interstitial.");
				chartboostManager.ShowInterstitial ();
				chartboostManager.RequestInterstitial ();
			}
		}

		public void ShowBanner() {
			bannerStatus = true;

			StartCoroutine (ShowBannerCo ());
		}

		private IEnumerator ShowBannerCo() {
			while (!customAdsManager.IsSchemeFetched && !customAdsManager.SchemeError) {
				yield return null;
			}

			if (bannersList != null && bannerStatus) {
				if (bannersList.Count == 1) {
					// Show appropriate banner (1)
					if (bannersList [0] == AdProvider.CUSTOM) {
						customAdsManager.ShowBanner ();
					}
					else if (bannersList [0] == AdProvider.ADMOB) {
						admobManager.ShowBanner ();
					}
				}
				else if (bannersList.Count == 2) {
					// Show appropriate banners consecutively (2)
					if (currentBanner == AdProvider.CUSTOM) {
						customAdsManager.ShowBanner ();
					}
					else if (currentBanner == AdProvider.ADMOB) {
						admobManager.ShowBanner ();
					}
				}
			}
		}

		public void HideBanner() {
			bannerStatus = false;

			StartCoroutine (HideBannerCo ());
		}

		private IEnumerator HideBannerCo() {
			while (!customAdsManager.IsSchemeFetched && !customAdsManager.SchemeError) {
				yield return null;
			}

			if (bannersList != null) {
				if (bannersList.Count == 1) {
					if (bannersList [0] == AdProvider.CUSTOM) {
						customAdsManager.HideBanner ();
					}
					else if (bannersList [0] == AdProvider.ADMOB) {
						Debug.Log ("Hiding admob banner.");
						admobManager.HideBanner ();
					}
				}
				else if (bannersList.Count == 2) {
					if (currentBanner == AdProvider.CUSTOM) {
						customAdsManager.HideBanner ();
						currentBanner = AdProvider.ADMOB;
					}
					else if (currentBanner == AdProvider.ADMOB) {
						Debug.Log ("Hiding admob banner.");
						admobManager.HideBanner ();
						currentBanner = AdProvider.CUSTOM;
					}
				}
			}
		}

		public void ShowInterstitial() {
			StartCoroutine (ShowInterstitialCo ());
		}

		private IEnumerator ShowInterstitialCo() {
			while (!customAdsManager.IsSchemeFetched && !customAdsManager.SchemeError) {
				yield return null;
			}

			// Show the appropriate ad
			if (adList != null) {
				if (count == 0) {
					if (adList.Count == 1) {
						// Show same ad everytime
						Show1Ad ();
					}
					else if (adList.Count == 2) {
						// Show 2 ads consecutively
						Show2Ads ();
					}
					else if (adList.Count == 3) {
						// Show 3 ads consecutively
						Show3Ads ();
					}

					count = AdInterval;
				}
			// Introduce an interval for next ad to be shown
			else {
					count--;
				}
			}
		}

		private void Show1Ad() {
			if (adList [0] == AdProvider.CUSTOM) {
				customAdsManager.ShowInterstitial ();
			}
			else if (adList [0] == AdProvider.ADMOB) {
				admobManager.ShowInterstitial ();
				admobManager.RequestInterstitial ();
			}
			else if (adList [0] == AdProvider.CHARTBOOST) {
				chartboostManager.ShowInterstitial ();
				chartboostManager.RequestInterstitial ();
			}
		}

		private void Show2Ads() {
			if (ap1 == AdProvider.ADMOB && ap2 == AdProvider.CHARTBOOST) {
				if (apFor2 == AdProvider.ADMOB) {
					admobManager.ShowInterstitial ();
					admobManager.RequestInterstitial ();
					apFor2 = AdProvider.CHARTBOOST;
				}
				else if (apFor2 == AdProvider.CHARTBOOST) {
					chartboostManager.ShowInterstitial ();
					chartboostManager.RequestInterstitial ();
					apFor2 = AdProvider.ADMOB;
				}
			}
			else if (ap1 == AdProvider.CUSTOM && ap2 == AdProvider.CHARTBOOST) {
				if (apFor2 == AdProvider.CUSTOM) {
					customAdsManager.ShowInterstitial ();
					apFor2 = AdProvider.CHARTBOOST;
				}
				else if (apFor2 == AdProvider.CHARTBOOST) {
					chartboostManager.ShowInterstitial ();
					chartboostManager.RequestInterstitial ();
					apFor2 = AdProvider.CUSTOM;
				}
			}
			else if (ap1 == AdProvider.CUSTOM && ap2 == AdProvider.ADMOB) {
				if (apFor2 == AdProvider.CUSTOM) {
					customAdsManager.ShowInterstitial ();
					apFor2 = AdProvider.ADMOB;
				}
				else if (apFor2 == AdProvider.ADMOB) {
					admobManager.ShowInterstitial ();
					admobManager.RequestInterstitial ();
					apFor2 = AdProvider.CUSTOM;
				}
			}
		}

		private void Show3Ads() {
			if (apFor3 == AdProvider.CUSTOM) {
				customAdsManager.ShowInterstitial ();
				apFor3 = AdProvider.ADMOB;
			}
			else if (apFor3 == AdProvider.ADMOB) {
				admobManager.ShowInterstitial ();
				admobManager.RequestInterstitial ();
				apFor3 = AdProvider.CHARTBOOST;
			}
			else if (apFor3 == AdProvider.CHARTBOOST) {
				chartboostManager.ShowInterstitial ();
				chartboostManager.RequestInterstitial ();
				apFor3 = AdProvider.CUSTOM;
			}
		}
	}
}






















