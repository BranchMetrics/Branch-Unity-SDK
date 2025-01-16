#import "AppDelegateListener.h"

#pragma mark - Private notification class interface

typedef void (^callbackWithShareCompletion) (NSString *activityType, BOOL completed, NSError *error);

@interface BranchUnityWrapper : NSObject<AppDelegateListener>
+ (BranchUnityWrapper *)sharedInstance;
- (BOOL)continueUserActivity:(NSUserActivity *)userActivity;
@end

#pragma mark - Unity plugin methods

extern "C" {
    #pragma mark - Key methods

    void _setBranchKey(char *branchKey, char* branchSDKVersion);

    #pragma mark - InitSession methods

    void _initSessionWithCallback(char *callbackId);
    void _initSessionWithUniversalObjectCallback(char *callbackId);

    #pragma mark - Session Item methods

    const char *_getFirstReferringBranchUniversalObject();
    const char *_getFirstReferringBranchLinkProperties();
    const char *_getLatestReferringBranchUniversalObject();
    const char *_getLatestReferringBranchLinkProperties();
    void _resetUserSession();
    void _setIdentity(char *userId);
    void _setIdentityWithCallback(char *userId, char *callbackId);
    void _logout();

    # pragma mark - Configuration methods

    void _enableLogging();
    void _setRetryInterval(int retryInterval);
    void _setMaxRetries(int maxRetries);
    void _setNetworkTimeout(int timeout);
    void _registerView(char *universalObjectJson);
    void _listOnSpotlight(char *universalObjectJson);
    void _setRequestMetadata(char *key, char *value);
    void _addFacebookPartnerParameter(char *name, char *value);
    void _clearPartnerParameters();
    void _setTrackingDisabled(BOOL value);
    void _setDMAParamsForEEA(BOOL eeaRegion, BOOL adPersonalizationConsent, BOOL adUserDataUsageConsent);

    #pragma mark - Send event methods
    
    void _sendEvent(char *eventJson);
    
    #pragma mark - Short URL Generation methods

    void _getShortURLWithBranchUniversalObjectAndCallback(char *universalObjectJson, char *linkPropertiesJson, char *callbackId);

    #pragma mark - Share Link methods
    
    void _shareLinkWithLinkProperties(char *universalObjectJson, char *linkPropertiesJson, char *message, char *callbackId);

    #pragma mark - QR Code methods

    void _generateBranchQRCode(char *universalObjectJson, char *linkPropertiesJson, char *qrCodeSettingsJson, char *callbackId);

    void _validate();
}

