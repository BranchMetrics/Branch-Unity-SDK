using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.InteropServices;

public class Branch : MonoBehaviour
{

	public static string sdkVersion = "2.0.0";

	public delegate void BranchCallbackWithParams(Dictionary<string, object> parameters, string error);
	public delegate void BranchCallbackWithUrl(string url, string error);
	public delegate void BranchCallbackWithStatus(bool changed, string error);
	public delegate void BranchCallbackWithList(List<object> list, string error);
	public delegate void BranchCallbackWithBranchUniversalObject(BranchUniversalObject universalObject, BranchLinkProperties linkProperties, string error);
	public delegate void BranchCallbackWithData(string data, string error);

	#region Public methods

	#region InitSession methods


	/**
	 * Initialize session and receive information about how it opened.
	 */
	public static void initSession(BranchCallbackWithParams callback)
	{
		if (_sessionCounter == 0)
		{
			++_sessionCounter;
			_isFirstSessionInited = true;
			autoInitCallbackWithParams = callback;

			var callbackId = _getNextCallbackId();
			_branchCallbacks[callbackId] = callback;
			_initSessionWithCallback(callbackId);
		}
	}

	/**
     * Initialize session and receive information about how it opened.
     */
	public static void initSession(BranchCallbackWithBranchUniversalObject callback)
	{
		if (_sessionCounter == 0)
		{
			++_sessionCounter;
			_isFirstSessionInited = true;
			autoInitCallbackWithBUO = callback;

			var callbackId = _getNextCallbackId();
			_branchCallbacks[callbackId] = callback;
			_initSessionWithUniversalObjectCallback(callbackId);
		}
	}

	/**
     * Close session, necessary for some platforms to track when to cut off a Branch session.
     */
	private static void closeSession()
	{
#if UNITY_ANDROID || UNITY_EDITOR
		if (_sessionCounter > 0) {
			_sessionCounter--;
		}
#endif
	}

	#endregion

	#region Session Item methods

	/**
     * Get the BranchUniversalObject from the initial install.
     */
	public static BranchUniversalObject getFirstReferringBranchUniversalObject()
	{
		string firstReferringParamsString = "";

#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
		IntPtr ptrResult = _getFirstReferringBranchUniversalObject();
		firstReferringParamsString = Marshal.PtrToStringAnsi(ptrResult);
#else
		firstReferringParamsString = _getFirstReferringBranchUniversalObject();
#endif

		BranchUniversalObject resultObject = new BranchUniversalObject(firstReferringParamsString);
		return resultObject;
	}

	/**
     * Get the BranchLinkProperties from the initial install.
     */
	public static BranchLinkProperties getFirstReferringBranchLinkProperties()
	{
		string firstReferringParamsString = "";

#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
		IntPtr ptrResult = _getFirstReferringBranchLinkProperties();
		firstReferringParamsString = Marshal.PtrToStringAnsi(ptrResult);
#else
		firstReferringParamsString = _getFirstReferringBranchLinkProperties();
#endif

		BranchLinkProperties resultObject = new BranchLinkProperties(firstReferringParamsString);
		return resultObject;
	}

	/**
     * Get the BranchUniversalObject from the last open.
     */
	public static BranchUniversalObject getLatestReferringBranchUniversalObject()
	{
		string latestReferringParamsString = "";

#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
		IntPtr ptrResult = _getLatestReferringBranchUniversalObject();
		latestReferringParamsString = Marshal.PtrToStringAnsi(ptrResult);
#else
		latestReferringParamsString = _getLatestReferringBranchUniversalObject();
#endif

		BranchUniversalObject resultObject = new BranchUniversalObject(latestReferringParamsString);
		return resultObject;
	}

	/**
     * Get the BranchLinkProperties from the initial install.
     */
	public static BranchLinkProperties getLatestReferringBranchLinkProperties()
	{
		string latestReferringParamsString = "";

#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
		IntPtr ptrResult = _getLatestReferringBranchLinkProperties();
		latestReferringParamsString = Marshal.PtrToStringAnsi(ptrResult);
#else
		latestReferringParamsString = _getLatestReferringBranchLinkProperties();
#endif

		BranchLinkProperties resultObject = new BranchLinkProperties(latestReferringParamsString);
		return resultObject;
	}

	/**
     * Reset the current session
     */
	public static void resetUserSession()
	{
		_resetUserSession();
	}

	/**
     * Specifiy an identity for the current session
     */
	public static void setIdentity(string userId)
	{
		_setIdentity(userId);
	}

	/**
     * Specifiy a Facebook ID for the current session.
     */
	public static void setFBAppID(string facebookAppID)
	{
		//_setFBAppID(facebookAppID);
	}

	/**
     * Specifiy an identity for the current session and receive information about the set.
     */
	public static void setIdentity(string userId, BranchCallbackWithParams callback)
	{
		var callbackId = _getNextCallbackId();

		_branchCallbacks[callbackId] = callback;

		_setIdentityWithCallback(userId, callbackId);
	}

	/**
     * Clear current session
     */
	public static void logout()
	{
		_logout();
	}

	#endregion

	#region Configuration methods

	/**
     * Enable native SDK logging.
     */
	public static void enableLogging()
	{
		_enableLogging();
	}

	/**
     * How many seconds between retries
     */
	public static void setRetryInterval(int retryInterval)
	{
		_setRetryInterval(retryInterval);
	}

	/**
     * How many retries before giving up
     */
	public static void setMaxRetries(int maxRetries)
	{
		_setMaxRetries(maxRetries);
	}

	/**
     * How long before deeming a request as timed out
     */
	public static void setNetworkTimeout(int timeout)
	{
		_setNetworkTimeout(timeout);
	}

	public static void registerView(BranchUniversalObject universalObject)
	{
		_registerView(universalObject.ToJsonString());
	}

	public static void listOnSpotlight(BranchUniversalObject universalObject)
	{
		_listOnSpotlight(universalObject.ToJsonString());
	}
	public static void setRequestMetadata(string key, string val)
	{

		if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
		{
			_setRequestMetadata(key, val);
		}
	}

	public static void addFacebookPartnerParameter(string name, string val)
	{

		if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(val))
		{
			_addFacebookPartnerParameter(name, val);
		}
	}

	public static void clearPartnerParameters()
	{
		_clearPartnerParameters();
	}

	public static void setTrackingDisabled(bool value)
	{
		_setTrackingDisabled(value);
	}

	public static void setDMAParamsForEEA(bool eeaRegion, bool adPersonalizationConsent, bool adUserDataUsageConsent)
	{
		_setDMAParamsForEEA(eeaRegion, adPersonalizationConsent, adUserDataUsageConsent);
	}

	#endregion

	#region Send Event methods

	/**
	 * Send event
	 **/
	public static void sendEvent(BranchEvent branchEvent)
	{
		_sendEvent(branchEvent.ToJsonString());

	}

	#endregion

	#region Share Link methods

	public static void shareLink(BranchUniversalObject universalObject, BranchLinkProperties linkProperties, string message, BranchCallbackWithParams callback)
	{
		var callbackId = _getNextCallbackId();

		_branchCallbacks[callbackId] = callback;

		_shareLinkWithLinkProperties(universalObject.ToJsonString(), linkProperties.ToJsonString(), message, callbackId);
	}

	#endregion

	#region Short URL Generation methods

	/**
     * Get a short url given a BranchUniversalObject, BranchLinkProperties
     */
	public static void getShortURL(BranchUniversalObject universalObject, BranchLinkProperties linkProperties, BranchCallbackWithUrl callback)
	{

		var callbackId = _getNextCallbackId();

		_branchCallbacks[callbackId] = callback;

		_getShortURLWithBranchUniversalObjectAndCallback(universalObject.ToJsonString(), linkProperties.ToJsonString(), callbackId);
	}

	#endregion

	#region QR Code methods

	/**
     * Generate a QR Code
     */
	public static void generateQRCode(BranchUniversalObject universalObject, BranchLinkProperties linkProperties, BranchQRCode branchQRCode, BranchCallbackWithData callback)
	{
		var callbackId = _getNextCallbackId();
		_branchCallbacks[callbackId] = callback;
		_generateBranchQRCode(universalObject.ToJsonString(), linkProperties.ToJsonString(), branchQRCode.ToJsonString(), callbackId);
	}

	#endregion

	#region Validator methods

	public static void validate()
	{
		_validate();
	}

	#endregion

	#endregion

	#region Singleton

	public void Awake()
	{

		// make sure there's only a single Branch instance
		var olderBranches = FindObjectsOfType<Branch>();
		if (olderBranches != null && olderBranches.Length > 1)
		{
			Destroy(gameObject);
			return;
		}

		// setup Branch singleton
		name = "Branch";
		DontDestroyOnLoad(gameObject);

		if (BranchData.Instance.testMode)
		{
			_setBranchKey(BranchData.Instance.testBranchKey, sdkVersion);
		}
		else
		{
			_setBranchKey(BranchData.Instance.liveBranchKey, sdkVersion);
		}
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (!_isFirstSessionInited)
		{
			return;
		}

		if (!pauseStatus)
		{
			if (autoInitCallbackWithParams != null)
			{
				initSession(autoInitCallbackWithParams);
			}
			else if (autoInitCallbackWithBUO != null)
			{
				initSession(autoInitCallbackWithBUO);
			}
		}
		else
		{
			closeSession();
		}
	}

	#endregion

	#region Private methods

	#region Platform Loading Methods

#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
    
    [DllImport ("__Internal")]
    private static extern void _setBranchKey(string branchKey, string sdkVersion);

	[DllImport ("__Internal")]
	private static extern void _initSessionWithCallback(string callbackId);

	[DllImport ("__Internal")]
	private static extern void _initSessionWithUniversalObjectCallback(string callbackId);
        
	[DllImport ("__Internal")]
	private static extern IntPtr _getFirstReferringBranchUniversalObject();

	[DllImport ("__Internal")]
	private static extern IntPtr _getFirstReferringBranchLinkProperties();

	[DllImport ("__Internal")]
	private static extern IntPtr _getLatestReferringBranchUniversalObject();
    
	[DllImport ("__Internal")]
	private static extern IntPtr _getLatestReferringBranchLinkProperties();

    [DllImport ("__Internal")]
    private static extern void _resetUserSession();
    
    [DllImport ("__Internal")]
    private static extern void _setIdentity(string userId);
    
    [DllImport ("__Internal")]
    private static extern void _setIdentityWithCallback(string userId, string callbackId);
    
    [DllImport ("__Internal")]
    private static extern void _logout();

    [DllImport ("__Internal")]
    private static extern void _enableLogging();

    [DllImport ("__Internal")]
    private static extern void _setRetryInterval(int retryInterval);
    
    [DllImport ("__Internal")]
    private static extern void _setMaxRetries(int maxRetries);
    
    [DllImport ("__Internal")]
    private static extern void _setNetworkTimeout(int timeout);
    
	[DllImport ("__Internal")]
	private static extern void _registerView(string universalObject);

	[DllImport ("__Internal")]
	private static extern void _listOnSpotlight(string universalObject);

	[DllImport ("__Internal")]
	private static extern void _setRequestMetadata(string key, string val);

	[DllImport ("__Internal")]
	private static extern void _addFacebookPartnerParameter(string name, string val);

    [DllImport ("__Internal")]
	private static extern void _clearPartnerParameters();

	[DllImport ("__Internal")]
	private static extern void _setTrackingDisabled(bool value);

	[DllImport ("__Internal")]
	private static extern void _setDMAParamsForEEA(bool eeaRegion, bool adPersonalizationConsent, bool adUserDataUsageConsent);

	[DllImport ("__Internal")]
	private static extern void _sendEvent(string eventName);

	[DllImport ("__Internal")]
	private static extern void _getShortURLWithBranchUniversalObjectAndCallback(string universalObject, string linkProperties, string callbackId);

	[DllImport ("__Internal")]
	private static extern void _shareLinkWithLinkProperties(string universalObject, string linkProperties, string message, string callbackId);

	[DllImport ("__Internal")]
	private static extern void _generateBranchQRCode(string universalObject, string linkProperties, string branchQRCode, string callbackId);

	[DllImport ("__Internal")]
	private static extern void _validate();
	    
#elif UNITY_ANDROID && !UNITY_EDITOR

    private static void _setBranchKey(string branchKey, string sdkVersion) {
        BranchAndroidWrapper.setBranchKey(branchKey, sdkVersion);
    }

	private static void _initSessionWithCallback(string callbackId) {
		BranchAndroidWrapper.initSessionWithCallback(callbackId);
	}

	private static void _initSessionWithUniversalObjectCallback(string callbackId) {
		BranchAndroidWrapper.initSessionWithUniversalObjectCallback(callbackId);
	}

	private static string _getFirstReferringBranchUniversalObject() {
		return BranchAndroidWrapper.getFirstReferringBranchUniversalObject();
	}

	private static string _getFirstReferringBranchLinkProperties() {
		return BranchAndroidWrapper.getFirstReferringBranchLinkProperties();
	}

	private static string _getLatestReferringBranchUniversalObject() {
		return BranchAndroidWrapper.getLatestReferringBranchUniversalObject();
	}

	private static string _getLatestReferringBranchLinkProperties() {
		return BranchAndroidWrapper.getLatestReferringBranchLinkProperties();
	}

    private static void _resetUserSession() {
        BranchAndroidWrapper.resetUserSession();
    }
    
    private static void _setIdentity(string userId) {
        BranchAndroidWrapper.setIdentity(userId);
    }

	private static void _setFBAppID(string facebookAppID){
        BranchAndroidWrapper.setFBAppID(facebookAppID);
	}
    
    private static void _setIdentityWithCallback(string userId, string callbackId) {
        BranchAndroidWrapper.setIdentityWithCallback(userId, callbackId);
    }
    
    private static void _logout() {
        BranchAndroidWrapper.logout();
    }

    private static void _enableLogging() {
        BranchAndroidWrapper.enableLogging();
    }
    
    private static void _setRetryInterval(int retryInterval) {
        BranchAndroidWrapper.setRetryInterval(retryInterval);
    }
    
    private static void _setMaxRetries(int maxRetries) {
        BranchAndroidWrapper.setMaxRetries(maxRetries);
    }
    
    private static void _setNetworkTimeout(int timeout) {
        BranchAndroidWrapper.setNetworkTimeout(timeout);
    }
    
	private static void _registerView(string universalObject) {
		BranchAndroidWrapper.registerView(universalObject);
	}

	private static void _listOnSpotlight(string universalObject) {
		BranchAndroidWrapper.listOnSpotlight(universalObject);
	}

	private static void _setRequestMetadata(string key, string val) {
		BranchAndroidWrapper.setRequestMetadata(key, val);
	}

    private static void _addFacebookPartnerParameter(string name, string val) {
		BranchAndroidWrapper.addFacebookPartnerParameter(name, val);
	}

    private static void _clearPartnerParameters() {
        BranchAndroidWrapper.clearPartnerParameters();
    }

	private static void _setTrackingDisabled(bool value) {
	    BranchAndroidWrapper.setTrackingDisabled(value);
    }
    
	private static void _setDMAParamsForEEA(bool eeaRegion, bool adPersonalizationConsent, bool adUserDataUsageConsent) {
		BranchAndroidWrapper.setDMAParamsForEEA(eeaRegion, adPersonalizationConsent, adUserDataUsageConsent);
	}

	private static void _sendEvent(string eventName) {
		BranchAndroidWrapper.sendEvent(eventName);
	}

	private static void _shareLinkWithLinkProperties(string universalObject, string linkProperties, string message, string callbackId) {
		BranchAndroidWrapper.shareLinkWithLinkProperties(universalObject, linkProperties, message, callbackId);
	}
	    
	private static void _getShortURLWithBranchUniversalObjectAndCallback(string universalObject, string linkProperties, string callbackId) {
		BranchAndroidWrapper.getShortURLWithBranchUniversalObjectAndCallback(universalObject, linkProperties, callbackId);
	}

	private static void _generateBranchQRCode(string universalObject, string linkProperties, string branchQRCode, string callbackId) {
		BranchAndroidWrapper.generateBranchQRCode(universalObject, linkProperties, branchQRCode, callbackId);
	}

	private static void _validate() {
		BranchAndroidWrapper.validate();
	}

#else

	private static void _setFBAppID(string facebookAppID) { }

	private static void _setBranchKey(string branchKey, string sdkVersion) { }

	private static void _initSessionWithCallback(string callbackId)
	{
		callNotImplementedCallbackForParamCallback(callbackId);
	}

	private static void _initSessionWithUniversalObjectCallback(string callbackId)
	{
		callNotImplementedCallbackForBUOCallback(callbackId);
	}

	private static string _getFirstReferringBranchUniversalObject()
	{
		return "{}";
	}

	private static string _getFirstReferringBranchLinkProperties()
	{
		return "{}";
	}

	private static string _getLatestReferringBranchUniversalObject()
	{
		return "{}";
	}

	private static string _getLatestReferringBranchLinkProperties()
	{
		return "{}";
	}

	private static void _resetUserSession() { }

	private static void _setIdentity(string userId) { }

	private static void _setIdentityWithCallback(string userId, string callbackId)
	{
		callNotImplementedCallbackForParamCallback(callbackId);
	}

	private static void _logout() { }

	private static void _enableLogging() { }

	private static void _setRetryInterval(int retryInterval) { }

	private static void _setMaxRetries(int maxRetries) { }

	private static void _setNetworkTimeout(int timeout) { }

	private static void _registerView(string universalObject) { }

	private static void _listOnSpotlight(string universalObject) { }

	private static void _setRequestMetadata(string key, string val) { }

	private static void _addFacebookPartnerParameter(string name, string val) { }

	private static void _clearPartnerParameters() { }

	private static void _setTrackingDisabled(bool value) { }

	private static void _setDMAParamsForEEA(bool eeaRegion, bool adPersonalizationConsent, bool adUserDataUsageConsent) { }

	private static void _sendEvent(string eventName) { }

	private static void _shareLinkWithLinkProperties(string universalObject, string linkProperties, string message, string callbackId)
	{
		callNotImplementedCallbackForUrlCallback(callbackId);
	}

	private static void _getShortURLWithBranchUniversalObjectAndCallback(string universalObject, string linkProperties, string callbackId)
	{
		callNotImplementedCallbackForUrlCallback(callbackId);
	}

	private static void _generateBranchQRCode(string universalObject, string linkProperties, string branchQRCode, string callbackId)
	{
		callNotImplementedCallbackForUrlCallback(callbackId);
	}

	private static void _validate() { }

	private static void callNotImplementedCallbackForParamCallback(string callbackId)
	{
		var callback = _branchCallbacks[callbackId] as BranchCallbackWithParams;
		callback(null, "Not implemented on this platform");
	}

	private static void callNotImplementedCallbackForUrlCallback(string callbackId)
	{
		var callback = _branchCallbacks[callbackId] as BranchCallbackWithUrl;
		callback(null, "Not implemented on this platform");
	}

	private static void callNotImplementedCallbackForListCallback(string callbackId)
	{
		var callback = _branchCallbacks[callbackId] as BranchCallbackWithList;
		callback(null, "Not implemented on this platform");
	}

	private static void callNotImplementedCallbackForStatusCallback(string callbackId)
	{
		var callback = _branchCallbacks[callbackId] as BranchCallbackWithStatus;
		callback(false, "Not implemented on this platform");
	}

	private static void callNotImplementedCallbackForBUOCallback(string callbackId)
	{
		var callback = _branchCallbacks[callbackId] as BranchCallbackWithBranchUniversalObject;
		callback(null, null, "Not implemented on this platform");
	}

#endif

	#endregion

	#region Callback management

	public void _asyncCallbackWithParams(string callbackDictString)
	{
		var callbackDict = BranchThirdParty_MiniJSON.Json.Deserialize(callbackDictString) as Dictionary<string, object>;
		var callbackId = callbackDict["callbackId"] as string;
		Dictionary<string, object> parameters = callbackDict.ContainsKey("params") ? callbackDict["params"] as Dictionary<string, object> : null;
		string error = callbackDict.ContainsKey("error") ? callbackDict["error"] as string : null;

		var callback = _branchCallbacks[callbackId] as BranchCallbackWithParams;
		if (callback != null)
		{
			callback(parameters, error);
		}
	}

	public void _asyncCallbackWithStatus(string callbackDictString)
	{
		var callbackDict = BranchThirdParty_MiniJSON.Json.Deserialize(callbackDictString) as Dictionary<string, object>;
		var callbackId = callbackDict["callbackId"] as string;
		bool status = callbackDict.ContainsKey("status") ? (callbackDict["status"] as bool?).Value : false;
		string error = callbackDict.ContainsKey("error") ? callbackDict["error"] as string : null;

		var callback = _branchCallbacks[callbackId] as BranchCallbackWithStatus;
		if (callback != null)
		{
			callback(status, error);
		}
	}

	public void _asyncCallbackWithList(string callbackDictString)
	{
		var callbackDict = BranchThirdParty_MiniJSON.Json.Deserialize(callbackDictString) as Dictionary<string, object>;
		var callbackId = callbackDict["callbackId"] as string;
		List<object> list = callbackDict.ContainsKey("list") ? callbackDict["list"] as List<object> : null;
		string error = callbackDict.ContainsKey("error") ? callbackDict["error"] as string : null;

		var callback = _branchCallbacks[callbackId] as BranchCallbackWithList;
		if (callback != null)
		{
			callback(list, error);
		}
	}

	public void _asyncCallbackWithUrl(string callbackDictString)
	{
		var callbackDict = BranchThirdParty_MiniJSON.Json.Deserialize(callbackDictString) as Dictionary<string, object>;
		var callbackId = callbackDict["callbackId"] as string;
		string url = callbackDict.ContainsKey("url") ? callbackDict["url"] as string : null;
		string error = callbackDict.ContainsKey("error") ? callbackDict["error"] as string : null;

		var callback = _branchCallbacks[callbackId] as BranchCallbackWithUrl;
		if (callback != null)
		{
			callback(url, error);
		}
	}

	public void _asyncCallbackWithData(string callbackDictString)
	{
		var callbackDict = BranchThirdParty_MiniJSON.Json.Deserialize(callbackDictString) as Dictionary<string, object>;
		var callbackId = callbackDict["callbackId"] as string;
		string data = callbackDict.ContainsKey("data") ? callbackDict["data"] as string : null;
		string error = callbackDict.ContainsKey("error") ? callbackDict["error"] as string : null;

		var callback = _branchCallbacks[callbackId] as BranchCallbackWithData;
		if (callback != null)
		{
			callback(data, error);
		}
	}

	public void _asyncCallbackWithBranchUniversalObject(string callbackDictString)
	{

		Debug.Log("callbackDictString: \n\n" + callbackDictString + "\n\n");

		var callbackDict = BranchThirdParty_MiniJSON.Json.Deserialize(callbackDictString) as Dictionary<string, object>;
		var callbackId = callbackDict["callbackId"] as string;
		var paramsDict = callbackDict.ContainsKey("params") ? callbackDict["params"] as Dictionary<string, object> : null;
		var universalObject = paramsDict != null && paramsDict.ContainsKey("universalObject") ? paramsDict["universalObject"] as Dictionary<string, object> : null;
		var linkProperties = paramsDict != null && paramsDict.ContainsKey("linkProperties") ? paramsDict["linkProperties"] as Dictionary<string, object> : null;
		string error = callbackDict.ContainsKey("error") ? callbackDict["error"] as string : null;

		var callback = _branchCallbacks[callbackId] as BranchCallbackWithBranchUniversalObject;
		if (callback != null)
		{
			callback(new BranchUniversalObject(universalObject), new BranchLinkProperties(linkProperties), error);
		}
	}

	public void _DebugLog(string val)
	{
		Debug.Log(val);
	}

	private static string _getNextCallbackId()
	{
		return "BranchCallbackId" + (++_nextCallbackId);
	}

	#endregion

	#endregion

	private static int _nextCallbackId = 0;
	private static Dictionary<string, object> _branchCallbacks = new Dictionary<string, object>();

	private static int _sessionCounter = 0;
	private static bool _isFirstSessionInited = false;
	private static BranchCallbackWithParams autoInitCallbackWithParams = null;
	private static BranchCallbackWithBranchUniversalObject autoInitCallbackWithBUO = null;
}
