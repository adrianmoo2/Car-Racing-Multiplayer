using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace AdsManagerAPI {
	public class CustomAdsManager : MonoBehaviour {

		private const string SCHEME_URL = "http://www.appicstudio.com/ad/scheme.php?";
		private const string BANNER_URL = "http://www.appicstudio.com/ad/banner.php?";
		private const string INTERSTITIAL_URL = "http://www.appicstudio.com/ad/interstitial.php?";
		private const string SERVER_CODE = "b2k";
		private const string ANDROID_PLATFORM_NAME = "android";
		private const string IOS_PLATFORM_NAME = "ios";
		private const int SCHEME_DATA_COUNT = 4;
		private const int BANNER_DATA_COUNT = 2;
		private const int INTERSTITIAL_DATA_COUNT = 2;

		private string Platform {
			get {
				if (Application.platform == RuntimePlatform.Android) {
					return ANDROID_PLATFORM_NAME;
				}
				else {
					return IOS_PLATFORM_NAME;
				}
			}
		}

		private string BundleIdentifier {
			get {
				return Application.identifier;
			}
		}

		private string Orientation {
			get {
				if (Screen.orientation == ScreenOrientation.Portrait ||
				    Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
					return "portrait";
				}
				else {
					return "landscape";
				}
			}
		}

		private string BannerTargetLink {
			get;
			set;
		}

		private string InterstitialTargetLink {
			get;
			set;
		}

		private bool isSchemeFetched = false;
		internal bool IsSchemeFetched {
			get {
				return isSchemeFetched;
			}
		}
		private bool schemeError = false;
		internal bool SchemeError {
			get {
				return schemeError;
			}
		}

		private bool isBannerReady = false;
		private bool bannerError = false;
		private bool BannerError {
			get {
				return bannerError;
			}
		}

		[SerializeField] private GameObject bannerAd;
		[SerializeField] private GameObject interstitialHolder;
		[SerializeField] private GameObject interstitialAdPortrait;
		[SerializeField] private GameObject interstitialAdLandscape;
		private GameObject InterstitialAd {
			get {
				if (Orientation == "portrait") {
					return interstitialAdPortrait;
				}
				else {
					return interstitialAdLandscape;
				}
			}
		}
		private bool isInterstitialLoaded = false;
		private bool IsInterstitialLoaded {
			get {
				return isInterstitialLoaded;
			}
		}
		private bool interstitialError = false;
		private bool InterstitialError {
			get {
				return interstitialError;
			}
		}

		private Animator anim;

		void Start() {
			anim = interstitialHolder.GetComponent<Animator> ();
		}

		internal void RequestScheme() {
			StartCoroutine (RequestSchemeCo ());
		}

		internal void RequestBanner() {
			StartCoroutine (RequestBannerCo ());
		}

		internal void ShowBanner() {
			AdjustBannerPos ();

			if (isBannerReady) {
				Debug.Log ("Showing custom banner.");
				bannerAd.SetActive (true);
			}
			else {
				Debug.Log ("Custom banner is not yet ready. It will be shown once it is ready.");
				StartCoroutine (BannerDelayedCo ());
			}
		}

		internal void HideBanner() {
			Debug.Log ("Hiding custom banner.");
			bannerAd.SetActive (false);
		}

		internal void RequestInterstitial() {
			StartCoroutine (RequestInterstitialCo ());
		}

		internal void ShowInterstitial() {
			if (IsInterstitialLoaded) {
				Debug.Log ("Showing custom interstitial.");
				InterstitialAd.SetActive (true);
				PlayInterstitialAnim ();
			}
			else {
				Debug.Log ("Custom interstitial is not yet ready. It will be shown once it is ready.");
				StartCoroutine (InterstitialDelayedCo ());
			}
		}

		internal void Banner_OnClick() {
			Application.OpenURL (BannerTargetLink);
		}

		private void PlayInterstitialAnim() {
			// Pause the game
			Time.timeScale = 0;

			EnableInterstitialHolder(true);
			SetRaycastTarget (true);
			// Play interstitial animation
			anim.Play ("InterstitialIn");
		}

		internal void Interstitial_OnClick() {
			Application.OpenURL (InterstitialTargetLink);
			CloseInterstitial ();
		}

		internal void InterstitialTickBtn_OnClick() {
			Application.OpenURL (InterstitialTargetLink);
			CloseInterstitial ();
		}

		internal void InterstitialCrossBtn_OnClick() {
			CloseInterstitial ();
		}

		private void CloseInterstitial() {
			// Resume game
			Time.timeScale = 1;

			InterstitialAd.SetActive (false);
			anim.Play ("InterstitialOut");
		}

		private void AdjustBannerPos() {
			RectTransform bannerRect = bannerAd.GetComponent<RectTransform> ();

			if (AdsManager.Instance.admobManager.bannerPosition == GoogleMobileAds.Api.AdPosition.Top) {
				bannerRect.anchorMin = new Vector2 (0.5f, 1);
				bannerRect.anchorMax = new Vector2 (0.5f, 1);
				bannerRect.anchoredPosition = new Vector2 (bannerRect.anchoredPosition.x,
					-Mathf.Abs (bannerRect.anchoredPosition.y));
			}
			else {
				bannerAd.GetComponent<RectTransform> ().anchorMin = new Vector2 (0.5f, 0);
				bannerAd.GetComponent<RectTransform> ().anchorMax = new Vector2 (0.5f, 0);
				bannerRect.anchoredPosition = new Vector2 (bannerRect.anchoredPosition.x,
					Mathf.Abs (bannerRect.anchoredPosition.y));
			}
		}

		private IEnumerator RequestSchemeCo() {
			// Scheme is being fetched
			isSchemeFetched = false;
			schemeError = false;

			string url = SCHEME_URL + "code=" + SERVER_CODE + "&platform=" + Platform + "&bundleid=" + BundleIdentifier;

			WWW www = new WWW(url);
			yield return www;

			// Check for errors
			if (www.error == null) {
			//	Debug.Log("WWW Ok!: " + www.text);
				Debug.Log("Ads initialization successful.");
				string[] data = www.text.Split ("\n".ToCharArray(), SCHEME_DATA_COUNT);

				string startupAd = data [0];
				if (startupAd == "c") {
					AdsManager.Instance.StartupAd = AdsManager.AdProvider.CUSTOM;
					AdsManager.Instance.customAdsManager.RequestInterstitial ();
				}
				else if (startupAd == "a") {
					AdsManager.Instance.StartupAd = AdsManager.AdProvider.ADMOB;
					AdsManager.Instance.admobManager.RequestInterstitial ();
				}
				else if (startupAd == "b") {
					AdsManager.Instance.StartupAd = AdsManager.AdProvider.CHARTBOOST;
					AdsManager.Instance.chartboostManager.RequestInterstitial ();
				}
				else {
					AdsManager.Instance.StartupAd = AdsManager.AdProvider.NONE;
				}

				AdsManager.Instance.ShowStartupAd ();

				string interval = data [1];
				AdsManager.Instance.AdInterval = int.Parse (interval);

				string adsToShow = data [2];
				AdsManager.Instance.InitAdList ();
				if (adsToShow == "nnn") {
					// Add nothing in the list
				}
				else if (adsToShow == "yyy") {
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.CUSTOM);
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.ADMOB);
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.CHARTBOOST);

					AdsManager.Instance.customAdsManager.RequestInterstitial ();
					AdsManager.Instance.admobManager.RequestInterstitial ();
					AdsManager.Instance.chartboostManager.RequestInterstitial ();
				}
				else if (adsToShow == "nyy") {
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.ADMOB);
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.CHARTBOOST);
					AdsManager.Instance.apFor2 = AdsManager.AdProvider.ADMOB;
					AdsManager.Instance.ap1 = AdsManager.AdProvider.ADMOB;
					AdsManager.Instance.ap2 = AdsManager.AdProvider.CHARTBOOST;

					AdsManager.Instance.admobManager.RequestInterstitial ();
					AdsManager.Instance.chartboostManager.RequestInterstitial ();
				}
				else if (adsToShow == "yny") {
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.CUSTOM);
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.CHARTBOOST);
					AdsManager.Instance.apFor2 = AdsManager.AdProvider.CUSTOM;
					AdsManager.Instance.ap1 = AdsManager.AdProvider.CUSTOM;
					AdsManager.Instance.ap2 = AdsManager.AdProvider.CHARTBOOST;

					AdsManager.Instance.customAdsManager.RequestInterstitial ();
					AdsManager.Instance.chartboostManager.RequestInterstitial ();
				}
				else if (adsToShow == "yyn") {
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.CUSTOM);
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.ADMOB);
					AdsManager.Instance.apFor2 = AdsManager.AdProvider.CUSTOM;
					AdsManager.Instance.ap1 = AdsManager.AdProvider.CUSTOM;
					AdsManager.Instance.ap2 = AdsManager.AdProvider.ADMOB;

					AdsManager.Instance.customAdsManager.RequestInterstitial ();
					AdsManager.Instance.admobManager.RequestInterstitial ();
				}
				else if (adsToShow == "ynn") {
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.CUSTOM);

					AdsManager.Instance.customAdsManager.RequestInterstitial ();
				}
				else if (adsToShow == "nyn") {
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.ADMOB);

					AdsManager.Instance.admobManager.RequestInterstitial ();
				}
				else if (adsToShow == "nny") {
					AdsManager.Instance.adList.Add (AdsManager.AdProvider.CHARTBOOST);

					AdsManager.Instance.chartboostManager.RequestInterstitial ();
				}

				string bannersToShow = data [3];
				AdsManager.Instance.InitBannersList ();
				if (bannersToShow == "nn") {
					// Add nothing in list
				}
				else if (bannersToShow == "ny") {
					AdsManager.Instance.bannersList.Add (AdsManager.AdProvider.ADMOB);
					AdsManager.Instance.admobManager.RequestBanner ();
				}
				else if (bannersToShow == "yn") {
					AdsManager.Instance.bannersList.Add (AdsManager.AdProvider.CUSTOM);
					AdsManager.Instance.customAdsManager.RequestBanner ();
				}
				else if (bannersToShow == "yy") {
					AdsManager.Instance.bannersList.Add (AdsManager.AdProvider.CUSTOM);
					AdsManager.Instance.bannersList.Add (AdsManager.AdProvider.ADMOB);
					AdsManager.Instance.customAdsManager.RequestBanner ();
					AdsManager.Instance.admobManager.RequestBanner ();
				}

				// Scheme has been fetched
				isSchemeFetched = true;
			}
			else {
				Debug.Log("Ads initialization failed: " + www.error);
				schemeError = true;
			}
		}

		private IEnumerator RequestBannerCo() {
			// Data is being fetched, banner is not yet loaded
			isBannerReady = false;
			bannerError = false;

			string url = BANNER_URL + "code=" + SERVER_CODE + "&platform=" + Platform + "&bundleid=" + BundleIdentifier;

			WWW www = new WWW(url);
			yield return www;

			// Check for errors
			if (www.error == null) {
			//	Debug.Log("WWW Ok!: " + www.text);
				string[] data = www.text.Split ("\n".ToCharArray(), BANNER_DATA_COUNT);
				string adImageLink = data [0];
				BannerTargetLink = data [1];

				yield return StartCoroutine (DownloadAndSetImage (bannerAd, adImageLink));

				// Banner has now been loaded
				isBannerReady = true;
			//	Debug.Log ("Banner is ready.");
			}
			else {
				Debug.Log("Banner request failed: " + www.error);
				bannerError = true;
			}
		}

		private IEnumerator BannerDelayedCo() {
			while (!isBannerReady && !BannerError) {
				yield return null;
			}

			if (!BannerError && AdsManager.Instance.bannerStatus) {
				Debug.Log ("Showing custom banner.");
				bannerAd.SetActive (true);
			}
		}

		private IEnumerator RequestInterstitialCo() {
			// Data is being fetched, interstitial is not yet loaded
			isInterstitialLoaded = false;
			interstitialError = false;

			string url = INTERSTITIAL_URL + "code=" + SERVER_CODE + "&platform=" + Platform + "&bundleid=" + BundleIdentifier +
			             "&orientation=" + Orientation;
			
			WWW www = new WWW(url);
			yield return www;

			// Check for errors
			if (www.error == null) {
			//	Debug.Log("WWW Ok!: " + www.text);
				string[] data = www.text.Split ("\n".ToCharArray(), INTERSTITIAL_DATA_COUNT);
				string adImageLink = data [0];
				InterstitialTargetLink = data [1];

				yield return StartCoroutine (DownloadAndSetImage (InterstitialAd, adImageLink));

				// Interstitial has now been loaded
				isInterstitialLoaded = true;
			//	Debug.Log ("Interstitial has been loaded.");
			}
			else {
			//	Debug.Log("WWW Error: " + www.error);
				interstitialError = true;
			}
		}

		private IEnumerator InterstitialDelayedCo() {
			while (!IsInterstitialLoaded && !InterstitialError) {
				yield return null;
			}

			if (!InterstitialError) {
				Debug.Log ("Showing custom interstitial.");
				InterstitialAd.SetActive (true);
				PlayInterstitialAnim ();
			}
		}

		private IEnumerator DownloadAndSetImage(GameObject adUnit, string url) {
			WWW www = new WWW(url);
			yield return www;

			if (www.error == null) {
			//	Debug.Log ("Image has been fetched successfully.");
				Texture2D tex = www.textureNonReadable;
				Sprite spr = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0.5f, 0.5f));

				adUnit.GetComponent<Image> ().sprite = spr;
			}
			else {
			//	Debug.Log ("There was an error while fetching the image: " + www.error);
			}
		}

		internal void EnableInterstitialHolder(bool value) {
			interstitialHolder.SetActive (value);
		}

		internal void SetRaycastTarget(bool value) {
			interstitialHolder.GetComponent<Image> ().raycastTarget = value;
		}

		private IEnumerator PostCo() {
			string url = "http://appicstudio.com/ad/ad.php";
			WWWForm form = new WWWForm();
			form.AddField("var1", "value1");
			form.AddField("var2", "value2");
			WWW www = new WWW(url, form);

			yield return www;

			// check for errors
			if (www.error == null) {
			//	Debug.Log("WWW Ok!: " + www.text);
			}
			else {
			//	Debug.Log("WWW Error: " + www.error);
			}    
		}
	}
}















