using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class BranchAndroidWrapper {
#if UNITY_ANDROID
    
    public static void setBranchKey(String branchKey, String sdkVersion) {
		_runBlockOnThread(() => {
        	_getBranchClass().CallStatic("setBranchKey", branchKey, sdkVersion);
		});
    }

	#region InitSession methods

	public static void initSessionWithCallback(string callbackId) {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("initSession", callbackId);
		});
	}

	public static void initSessionWithUniversalObjectCallback(string callbackId) {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("initSessionWithUniversalObjectCallback", callbackId);
		});
	}
    
	#endregion
    
	#region Session Item methods
    
	public static string getFirstReferringBranchUniversalObject() {
		return _getBranchClass().CallStatic<string>("getFirstReferringBranchUniversalObject");
	}
	
	public static string getFirstReferringBranchLinkProperties() {
		return _getBranchClass().CallStatic<string>("getFirstReferringBranchLinkProperties");
	}
    
	public static string getLatestReferringBranchUniversalObject() {
		return _getBranchClass().CallStatic<string>("getLatestReferringBranchUniversalObject");
	}
	
	public static string getLatestReferringBranchLinkProperties() {
		return _getBranchClass().CallStatic<string>("getLatestReferringBranchLinkProperties");
	}

    public static void resetUserSession() {
		_runBlockOnThread(() => {
        	_getBranchClass().CallStatic("resetUserSession");
		});
    }
    
    public static void setIdentity(string userId) {
		_runBlockOnThread(() => {
        	_getBranchClass().CallStatic("setIdentity", userId);
		});
    }

    //INTENG-20830
    public static void setFBAppID(string facebookAppID)
    {
	    _runBlockOnThread(() => {
		    _getBranchClass().CallStatic("setFBAppID", facebookAppID);
	    });
    }
    
    public static void setIdentityWithCallback(string userId, string callbackId) {
		_runBlockOnThread(() => {
        	_getBranchClass().CallStatic("setIdentity", userId, callbackId);
		});
    }
    
    public static void logout() {
		_runBlockOnThread(() => {
        	_getBranchClass().CallStatic("logout");
		});
    }
    
	#endregion
    
	#region Configuration methods

	public static void enableLogging() {
		_runBlockOnThread(() => {
        	_getBranchClass().CallStatic("enableLogging");
		});
    }
    
    public static void setRetryInterval(int retryInterval) {
		_runBlockOnThread(() => {
        	_getBranchClass().CallStatic("setRetryInterval", retryInterval);
		});
    }
    
    public static void setMaxRetries(int maxRetries) {
		_runBlockOnThread(() => {
        	_getBranchClass().CallStatic("setMaxRetries", maxRetries);
		});
    }
    
    public static void setNetworkTimeout(int timeout) {
		_runBlockOnThread(() => {
        	_getBranchClass().CallStatic("setNetworkTimeout", timeout);
		});
    }
    
	public static void registerView(string universalObject) {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("registerView", universalObject);
		});
	}

	public static void listOnSpotlight(string universalObject) {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("listOnSpotlight", universalObject);
		});
	}

	public static void setRequestMetadata(string key, string val) {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("setRequestMetadata", key, val);
		});
	}

	public static void addFacebookPartnerParameter(string name, string val) {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("addFacebookPartnerParameter", name, val);
		});
	}

	 public static void clearPartnerParameters() {
	 	_runBlockOnThread(() => {
	 		_getBranchClass().CallStatic("clearPartnerParameters");
	 	});
	 }

	public static void setTrackingDisabled(bool value) {
	    _runBlockOnThread(() => {
			_getBranchClass().CallStatic("setTrackingDisabled", value);
        });
	}

	public static void setDMAParamsForEEA(bool eeaRegion, bool adPersonalizationConsent, bool adUserDataUsageConsent) {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("setDMAParamsForEEA", eeaRegion, adPersonalizationConsent, adUserDataUsageConsent);
		});
	}

	#endregion
    
	#region Event methods
    
	public static void sendEvent(string eventName) {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("sendEvent", eventName);
		});
	}

	#endregion
    
	#region Share Link methods

	public static void shareLinkWithLinkProperties(string universalObject, string linkProperties, string message, string callbackId) {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("shareLinkWithLinkProperties", universalObject, linkProperties, message, callbackId);
		});
	}

	#endregion
    
	#region Short URL Generation methods
    
	public static void getShortURLWithBranchUniversalObjectAndCallback(string universalObject, string linkProperties, string callbackId) {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("getShortURLWithBranchUniversalObject", universalObject, linkProperties, callbackId);
		});
	}

	#endregion

	#region QR Code Generation methods

	public static void generateBranchQRCode(string universalObject, string linkProperties, string branchQRCode, string callbackId)
	{
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("generateBranchQRCode", universalObject, linkProperties, branchQRCode, callbackId);
		});
	}

	#endregion

	#region Validator methods

	public static void validate() {
		_runBlockOnThread(() => {
			_getBranchClass().CallStatic("validate");
		});
	}

	#endregion

	#region Utility methods

	private static AndroidJavaClass _getBranchClass() {
		if (_branchClass == null) {
			_branchClass = new AndroidJavaClass("io/branch/unity/BranchUnityWrapper");
		}

		return _branchClass;
	}
	
	private static void _runBlockOnThread(Action runBlock) {
		var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		activity.Call("runOnUiThread", new AndroidJavaRunnable(runBlock));
	}
    
	#endregion
    
    private static AndroidJavaClass _branchClass;
	private static AndroidJavaClass _validatorClass;
    
#endif
}
