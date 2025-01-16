package io.branch.unity;

import android.app.Activity;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Iterator;
import java.util.Locale;
import java.util.Base64;
import java.io.IOException;

import io.branch.indexing.BranchUniversalObject;
import io.branch.referral.Branch;
import io.branch.referral.Branch.BranchReferralInitListener;
import io.branch.referral.BranchError;
import io.branch.referral.BranchUtil;
import io.branch.referral.Defines;
import io.branch.referral.SharingHelper;
import io.branch.referral.util.BRANCH_STANDARD_EVENT;
import io.branch.referral.util.BranchEvent;
import io.branch.referral.util.ContentMetadata;
import io.branch.referral.util.CurrencyType;
import io.branch.referral.util.LinkProperties;
import io.branch.referral.util.ShareSheetStyle;
import io.branch.referral.QRCode.BranchQRCode;
import io.branch.referral.validators.IntegrationValidator;

/**
 * Created by grahammueller on 3/25/15.
 */
public class BranchUnityWrapper {
    public static void setBranchKey(String branchKey, String sdkVersion) {
        _branchKey = branchKey;
        Branch.registerPlugin("Unity", sdkVersion);
    }

    // workaround to save payload, before C# code runs
    private static BranchReferralInitListenerUnityCallback defaultListener;

    private static final String TAG = "BranchSDK.Unity";

    /**
     * Init session from BranchUnityActivity.onStart()
     * Caches the last response.
     */
    public static void initSession() {
        //Log.i(TAG, "BranchUnityWrapper.initSession()");

        defaultListener = new BranchReferralInitListenerUnityCallback();
        Activity unityActivity = UnityPlayer.currentActivity;

        // TODO: replace with sessionbuilder version
        Branch.getAutoInstance(UnityPlayer.currentActivity.getApplicationContext(), _branchKey);
        Branch.sessionBuilder(unityActivity).withData(unityActivity.getIntent().getData()).withCallback(defaultListener).init();
    }

    /**
     * Branch SDK is already initialized, this just attaches the callback id and format
     * @param callbackId
     */
    public static void initSession(String callbackId) {
        //Log.i(TAG, "BranchUnityWrapper.initSession(String callbackId)");
        
        defaultListener.setCallbackIDAndClearCachedParams(callbackId, false);
        Branch.notifyNativeToInit();
    }

    /**
     * Branch SDK is already initialized, this just attaches the callback id and format
     * @param callbackId
     */
    public static void initSessionWithUniversalObjectCallback(String callbackId) {
        //Log.i(TAG, "BranchUnityWrapper.initSessionWithUniversalObjectCallback(String callbackId)");

        defaultListener.setCallbackIDAndClearCachedParams(callbackId, true);
        Branch.notifyNativeToInit();
    }

    /**
     * Called by BranchUnityActivity onNewIntent. Forces a new session.
     */
    public static void initSessionWithIntent() {
        //Log.i(TAG, "BranchUnityWrapper.initSessionWithIntent()");

        Activity unityActivity = UnityPlayer.currentActivity;
        unityActivity.getIntent().putExtra("branch_force_new_session", true);
        initSession();
    }

    /**
     * Session Item methods
     */
    public static String getFirstReferringBranchUniversalObject() {
        BranchUniversalObject branchUniversalObject = null;
        Branch branchInstance = Branch.getInstance();
        if (branchInstance != null && branchInstance.getFirstReferringParams() != null) {
            JSONObject firstParam = branchInstance.getFirstReferringParams();
            try {
                if (firstParam.has("+clicked_branch_link") && firstParam.getBoolean("+clicked_branch_link")) {
                    branchUniversalObject = BranchUniversalObject.createInstance(firstParam);
                }
            } catch (Exception ignore) {
            }
        }

        return _jsonObjectFromBranchUniversalObject(branchUniversalObject).toString();
    }

    public static String getFirstReferringBranchLinkProperties() {
        LinkProperties linkProperties = null;
        Branch branchInstance = Branch.getInstance();
        if (branchInstance != null && branchInstance.getFirstReferringParams() != null) {
            JSONObject firstParam = branchInstance.getFirstReferringParams();

            try {
                if (firstParam.has("+clicked_branch_link") && firstParam.getBoolean("+clicked_branch_link")) {
                    linkProperties = new LinkProperties();
                    if (firstParam.has("~channel")) {
                        linkProperties.setChannel(firstParam.getString("~channel"));
                    }
                    if (firstParam.has("~feature")) {
                        linkProperties.setFeature(firstParam.getString("~feature"));
                    }
                    if (firstParam.has("~alias")) {
                        linkProperties.setAlias(firstParam.getString("~alias"));
                    }
                    if (firstParam.has("~stage")) {
                        linkProperties.setStage(firstParam.getString("~stage"));
                    }
                    if (firstParam.has("~duration")) {
                        linkProperties.setDuration(firstParam.getInt("~duration"));
                    }
                    if (firstParam.has("$match_duration")) {
                        linkProperties.setDuration(firstParam.getInt("$match_duration"));
                    }
                    if (firstParam.has("~tags")) {
                        JSONArray tagsArray = firstParam.getJSONArray("~tags");
                        for (int i = 0; i < tagsArray.length(); i++) {
                            linkProperties.addTag(tagsArray.getString(i));
                        }
                    }

                    Iterator<String> keys = firstParam.keys();
                    while (keys.hasNext()) {
                        String key = keys.next();
                        if (key.startsWith("$")) {
                            linkProperties.addControlParameter(key, firstParam.getString(key));
                        }
                    }
                }
            } catch (Exception ignore) {
            }
        }
        return _jsonObjectFromLinkProperties(linkProperties).toString();
    }

    public static String getLatestReferringBranchUniversalObject() {
        BranchUniversalObject branchUniversalObject = null;
        Branch branchInstance = Branch.getInstance();

        if (branchInstance != null && branchInstance.getLatestReferringParams() != null) {
            JSONObject latestParam = branchInstance.getLatestReferringParams();
            try {
                if (latestParam.has("+clicked_branch_link") && latestParam.getBoolean("+clicked_branch_link")) {
                    branchUniversalObject = BranchUniversalObject.createInstance(latestParam);
                }

            } catch (Exception ignore) {
            }
        }

        return _jsonObjectFromBranchUniversalObject(branchUniversalObject).toString();
    }

    public static String getLatestReferringBranchLinkProperties() {
        LinkProperties linkProperties = null;
        Branch branchInstance = Branch.getInstance();
        if (branchInstance != null && branchInstance.getLatestReferringParams() != null) {
            JSONObject latestParam = branchInstance.getLatestReferringParams();
            try {
                if (latestParam.has("+clicked_branch_link") && latestParam.getBoolean("+clicked_branch_link")) {
                    linkProperties = new LinkProperties();
                    if (latestParam.has("~channel")) {
                        linkProperties.setChannel(latestParam.getString("~channel"));
                    }
                    if (latestParam.has("~feature")) {
                        linkProperties.setFeature(latestParam.getString("~feature"));
                    }
                    if (latestParam.has("~alias")) {
                        linkProperties.setAlias(latestParam.getString("~alias"));
                    }
                    if (latestParam.has("~stage")) {
                        linkProperties.setStage(latestParam.getString("~stage"));
                    }
                    if (latestParam.has("~duration")) {
                        linkProperties.setDuration(latestParam.getInt("~duration"));
                    }
                    if (latestParam.has("$match_duration")) {
                        linkProperties.setDuration(latestParam.getInt("$match_duration"));
                    }
                    if (latestParam.has("~tags")) {
                        JSONArray tagsArray = latestParam.getJSONArray("~tags");
                        for (int i = 0; i < tagsArray.length(); i++) {
                            linkProperties.addTag(tagsArray.getString(i));
                        }
                    }

                    Iterator<String> keys = latestParam.keys();
                    while (keys.hasNext()) {
                        String key = keys.next();
                        if (key.startsWith("$")) {
                            linkProperties.addControlParameter(key, latestParam.getString(key));
                        }
                    }
                }
            } catch (Exception ignore) {
            }
        }
        return _jsonObjectFromLinkProperties(linkProperties).toString();
    }

    public static void resetUserSession() {
        Branch.getInstance().resetUserSession();
    }

    public static void setIdentity(String userId) {
        Branch.getInstance().setIdentity(userId);
    }

    public static void setIdentity(String userId, String callbackId) {
        Branch.getInstance().setIdentity(userId, new BranchReferralInitListenerUnityCallback(callbackId));
    }

    public static void logout() {
        Branch.getInstance().logout();
    }
    
    public static void enableLogging() {
        Branch.enableLogging();
    }

    public static void setRetryInterval(int retryInterval) {
        Branch.getInstance().setRetryInterval(retryInterval);
    }

    public static void setMaxRetries(int maxRetries) {
        Branch.getInstance().setRetryCount(maxRetries);
    }

    public static void setNetworkTimeout(int timeout) {
        Branch.getInstance().setNetworkTimeout(timeout);
    }

    public static void setDMAParamsForEEA(boolean eeaRegion, boolean adPersonalizationConsent, boolean adUserDataUsageConsent) {
        Branch.getInstance().setDMAParamsForEEA(eeaRegion, adPersonalizationConsent, adUserDataUsageConsent);
    }

    public static void registerView(String universalObjectDict) {
        try {
            BranchUniversalObject universalObject = _branchUniversalObjectFromJSONObject(new JSONObject(universalObjectDict));
            Branch.getInstance().registerView(universalObject, null);
        }
        catch (JSONException json) {
            json.printStackTrace();
        }
    }

    public static void listOnSpotlight(String universalObjectDict) {
        try {
            BranchUniversalObject universalObject = _branchUniversalObjectFromJSONObject(new JSONObject(universalObjectDict));
            //universalObject.listOnGoogleSearch(UnityPlayer.currentActivity.getApplicationContext());
        }
        catch (JSONException jsone) {
            jsone.printStackTrace();
        }
    }

    public static void setRequestMetadata(String key, String value) {
        Branch.getInstance().setRequestMetadata(key, value);
    }

    public static void addFacebookPartnerParameter(String name, String value) {
        Branch.getInstance().addFacebookPartnerParameterWithName(name, value);
    }

    public static void clearPartnerParameters() {
        Branch.getInstance().clearPartnerParameters();
    }

    public static void setTrackingDisabled(boolean value) {
        Branch.getInstance().disableTracking(value);
    }

    /**
     * Send Event methods
     */

    public static void sendEvent(String eventData) {

        if (eventData.isEmpty()) {
            return;
        }

        try {
            BranchEvent event = null;
            JSONObject params = new JSONObject(eventData);

            if (params.has("event_name")) {

                // try to create standart event
                for (BRANCH_STANDARD_EVENT type : BRANCH_STANDARD_EVENT.values()) {
                    if (type.getName().equals(params.getString("event_name"))) {
                        event = new BranchEvent(type);
                        break;
                    }
                }

                // if standart event can't be created, let's create custom event
                if (event == null) {
                    event = new BranchEvent(params.getString("event_name"));
                }
            }
            else {
                return;
            }

            if (params.has("transaction_id")) {
                event.setTransactionID(params.getString("transaction_id"));
            }

            if (params.has("customer_event_alias")) {
                event.setCustomerEventAlias(params.getString("customer_event_alias"));
            }

            if (params.has("affiliation")) {
                event.setAffiliation(params.getString("affiliation"));
            }

            if (params.has("coupon")) {
                event.setCoupon(params.getString("coupon"));
            }

            if (params.has("currency")) {
                event.setCurrency(CurrencyType.getValue(params.getString("currency")));
            }

            if (params.has("tax")) {
                event.setTax(params.getDouble("tax"));
            }

            if (params.has("revenue")) {
                event.setRevenue(params.getDouble("revenue"));
            }

            if (params.has("description")) {
                event.setDescription(params.getString("description"));
            }

            if (params.has("shipping")) {
                event.setRevenue(params.getDouble("shipping"));
            }

            if (params.has("search_query")) {
                event.setSearchQuery(params.getString("search_query"));
            }

            if (params.has("custom_data")) {
                JSONObject dict = params.getJSONObject("custom_data");
                Iterator<String> keys = dict.keys();
                while (keys.hasNext()) {
                    String key = keys.next();
                    event.addCustomDataProperty(key, dict.getString(key));
                }
            }

            if (params.has("content_items")) {
                JSONArray array = params.getJSONArray("content_items");
                for (int i = 0; i < array.length(); i++) {
                    event.addContentItems( _branchUniversalObjectFromJSONObject(new JSONObject(array.get(i).toString())) );
                }
            }

            // log event
            event.logEvent(UnityPlayer.currentActivity.getApplicationContext());
        }
        catch (JSONException jsone) {
            jsone.printStackTrace();
        }
    }

    /**
     * Short URL Generation methods
     */

    public static void getShortURLWithBranchUniversalObject(String universalObjectDict, String linkPropertiesDict, String callbackId) {

        try {
            BranchUniversalObject universalObject = _branchUniversalObjectFromJSONObject(new JSONObject(universalObjectDict));
            LinkProperties linkProperties = _linkPropertiesFromJSONObject(new JSONObject(linkPropertiesDict));

            universalObject.generateShortUrl(UnityPlayer.currentActivity.getApplicationContext(), linkProperties, new BranchReferralInitListenerUnityCallback(callbackId));
        }
        catch (JSONException jsone) {
            jsone.printStackTrace();
        }
    }

    /**
     * QR Code Generation methods
     */
     public static void generateBranchQRCode(String universalObjectDict, String linkPropertiesDict, String qrCodeDict, String callbackId) throws IOException {
         try {
             BranchUniversalObject universalObject = _branchUniversalObjectFromJSONObject(new JSONObject(universalObjectDict));
             LinkProperties linkProperties = _linkPropertiesFromJSONObject(new JSONObject(linkPropertiesDict));
             BranchQRCode qrCode = _qrCodeFromJSONObject(new JSONObject(qrCodeDict));
             qrCode.getQRCodeAsData(UnityPlayer.currentActivity.getApplicationContext(), universalObject, linkProperties, new BranchReferralInitListenerUnityCallback(callbackId));
         }
         catch (JSONException jsone) {
             jsone.printStackTrace();
         }
     }

     /**
     * Validator methods
     */
    public static void validate() {
        IntegrationValidator.validate(UnityPlayer.currentActivity.getApplicationContext());
    }

    /**
     * Share methods
     */

    public static void shareLinkWithLinkProperties(String universalObjectDict, String linkPropertiesDict, String message, String callbackId) {
        try {
            BranchUniversalObject universalObject = _branchUniversalObjectFromJSONObject(new JSONObject(universalObjectDict));
            LinkProperties linkProperties = _linkPropertiesFromJSONObject(new JSONObject(linkPropertiesDict));

            ShareSheetStyle style = new ShareSheetStyle(UnityPlayer.currentActivity.getApplicationContext(), "", message);
            style.addPreferredSharingOption(SharingHelper.SHARE_WITH.FACEBOOK);
            style.addPreferredSharingOption(SharingHelper.SHARE_WITH.TWITTER);
            style.addPreferredSharingOption(SharingHelper.SHARE_WITH.MESSAGE);
            style.addPreferredSharingOption(SharingHelper.SHARE_WITH.EMAIL);
            style.addPreferredSharingOption(SharingHelper.SHARE_WITH.FLICKR);
            style.addPreferredSharingOption(SharingHelper.SHARE_WITH.GOOGLE_DOC);
            style.addPreferredSharingOption(SharingHelper.SHARE_WITH.WHATS_APP);

            universalObject.showShareSheet(UnityPlayer.currentActivity, linkProperties, style, new BranchReferralInitListenerUnityCallback(callbackId));
        }
        catch (JSONException jsone) {
            jsone.printStackTrace();
        }
    }

    /**
     * Util methods
     */

    private static Date _dateFromString(String dateString) {
        Date date = null;

        if (dateString.length() > 0) {
            SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd'T'hh:mm:ss.SSS'Z'", Locale.US);

            try {
                date = format.parse(dateString);
            } catch (ParseException pe) {
                pe.printStackTrace();
            }
        }

        return date;
    }

    private static JSONObject _jsonObjectFromBranchUniversalObject(BranchUniversalObject obj) {
        JSONObject jsonObject = new JSONObject();

        if (obj != null) {
            try {
                jsonObject.put(Defines.Jsonkey.CanonicalIdentifier.getKey(), obj.getCanonicalIdentifier());
                jsonObject.put(Defines.Jsonkey.CanonicalUrl.getKey(), obj.getCanonicalUrl());
                jsonObject.put(Defines.Jsonkey.ContentTitle.getKey(), obj.getTitle());
                jsonObject.put(Defines.Jsonkey.ContentDesc.getKey(), obj.getDescription());
                jsonObject.put(Defines.Jsonkey.ContentImgUrl.getKey(), obj.getImageUrl());
                jsonObject.put(Defines.Jsonkey.PublicallyIndexable.getKey(), obj.isPublicallyIndexable() ? "0" : "1");
                jsonObject.put(Defines.Jsonkey.LocallyIndexable.getKey(), obj.isLocallyIndexable() ? "0" : "1");
                jsonObject.put(Defines.Jsonkey.ContentKeyWords.getKey(), new JSONArray(obj.getKeywords()));
                jsonObject.put(Defines.Jsonkey.ContentExpiryTime.getKey(), Long.toString(obj.getExpirationTime()));
                jsonObject.put("metadata", obj.getContentMetadata().convertToJson());

            } catch (JSONException jsone) {
                jsone.printStackTrace();
            }
        }

        return jsonObject;
    }

    private static JSONObject _jsonObjectFromLinkProperties(LinkProperties link) {
        JSONObject jsonObject = new JSONObject();

        if (link != null) {
            try {
                jsonObject.put("~tags", new JSONArray(link.getTags()));
                jsonObject.put("~feature", link.getFeature());
                jsonObject.put("~alias", link.getAlias());
                jsonObject.put("~stage", link.getStage());
                jsonObject.put("~duration", link.getMatchDuration());
                jsonObject.put("~channel", link.getChannel());
                jsonObject.put("control_params", new JSONObject(link.getControlParams()));

            } catch (JSONException jsone) {
                jsone.printStackTrace();
            }
        }

        return jsonObject;
    }

    private static BranchUniversalObject _branchUniversalObjectFromJSONObject(JSONObject params) {
        BranchUniversalObject branchUniversalObject = new BranchUniversalObject();

        try {
            if (params.has(Defines.Jsonkey.ContentTitle.getKey())) {
                branchUniversalObject.setTitle(params.getString(Defines.Jsonkey.ContentTitle.getKey()));
            }
            if (params.has(Defines.Jsonkey.CanonicalIdentifier.getKey())) {
                branchUniversalObject.setCanonicalIdentifier(params.getString(Defines.Jsonkey.CanonicalIdentifier.getKey()));
            }
            if (params.has(Defines.Jsonkey.CanonicalUrl.getKey())) {
                branchUniversalObject.setCanonicalUrl(params.getString(Defines.Jsonkey.CanonicalUrl.getKey()));
            }
            if (params.has(Defines.Jsonkey.ContentKeyWords.getKey())) {
                JSONArray keywordJsonArray = params.getJSONArray(Defines.Jsonkey.ContentKeyWords.getKey());
                for (int i = 0; i < keywordJsonArray.length(); i++) {
                    branchUniversalObject.addKeyWord(keywordJsonArray.get(i).toString());
                }
            }
            if (params.has(Defines.Jsonkey.ContentDesc.getKey())) {
                branchUniversalObject.setContentDescription(params.getString(Defines.Jsonkey.ContentDesc.getKey()));
            }
            if (params.has(Defines.Jsonkey.ContentImgUrl.getKey())) {
                branchUniversalObject.setContentImageUrl(params.getString(Defines.Jsonkey.ContentImgUrl.getKey()));
            }
            if (params.has(Defines.Jsonkey.PublicallyIndexable.getKey())) {
                branchUniversalObject.setContentIndexingMode(
                        params.getString(Defines.Jsonkey.PublicallyIndexable.getKey()).equals("0") ?
                                BranchUniversalObject.CONTENT_INDEX_MODE.PUBLIC : BranchUniversalObject.CONTENT_INDEX_MODE.PRIVATE);
            }
            if (params.has(Defines.Jsonkey.LocallyIndexable.getKey())) {
                branchUniversalObject.setLocalIndexMode(
                        params.getString(Defines.Jsonkey.LocallyIndexable.getKey()).equals("0") ?
                                BranchUniversalObject.CONTENT_INDEX_MODE.PUBLIC : BranchUniversalObject.CONTENT_INDEX_MODE.PRIVATE);
            }
            if (params.has(Defines.Jsonkey.ContentExpiryTime.getKey())) {
                if (!params.getString(Defines.Jsonkey.ContentExpiryTime.getKey()).isEmpty()) {
                    branchUniversalObject.setContentExpiration(new Date(Long.decode(params.getString(Defines.Jsonkey.ContentExpiryTime.getKey()))));
                }
            }
            if (params.has("metadata")) {
                JSONObject dict = new JSONObject(params.getString("metadata"));

                ContentMetadata cmd = ContentMetadata.createFromJson(new BranchUtil.JsonReader(dict));
                branchUniversalObject.setContentMetadata(cmd);
            }
        } catch (Exception ignore) {
        }

        return branchUniversalObject;
    }

    private static LinkProperties _linkPropertiesFromJSONObject(JSONObject params) {
        LinkProperties linkProperties = new LinkProperties();

        try {
            if (params.has("~channel")) {
                linkProperties.setChannel(params.getString("~channel"));
            }
            if (params.has("~feature")) {
                linkProperties.setFeature(params.getString("~feature"));
            }
            if (params.has("~alias")) {
                linkProperties.setAlias(params.getString("~alias"));
            }
            if (params.has("~stage")) {
                linkProperties.setStage(params.getString("~stage"));
            }
            if (params.has("~duration")) {
                linkProperties.setDuration(Long.valueOf(params.getString("~duration")).intValue());
            }
            if (params.has("~tags")) {
                JSONArray tagsArray = params.getJSONArray("~tags");
                for (int i = 0; i < tagsArray.length(); i++) {
                    linkProperties.addTag(tagsArray.getString(i));
                }
            }
            if (params.has("control_params")) {
                JSONObject dict = params.getJSONObject("control_params");
                Iterator<String> keys = dict.keys();
                while (keys.hasNext()) {
                    String key = keys.next();
                    linkProperties.addControlParameter(key, dict.getString(key));
                }
            }
        } catch (Exception ignore) {
        }

        return linkProperties;
    }

    private static BranchQRCode _qrCodeFromJSONObject(JSONObject params) {
        BranchQRCode branchQRCode = new BranchQRCode();

        try {
            if (params.has("code_color")) {
                branchQRCode.setCodeColor(params.getString("code_color"));
            }
            if (params.has("background_color")) {
                branchQRCode.setBackgroundColor(params.getString("background_color"));
            }
            if (params.has("margin")) {
                branchQRCode.setMargin(Long.valueOf(params.getString("margin")).intValue());
            }
            if (params.has("width")) {
                branchQRCode.setWidth(Long.valueOf(params.getString("width")).intValue());
            }
            if (params.has("image_format")) {
                branchQRCode.setImageFormat(params.getString("image_format").equals("0") ? io.branch.referral.QRCode.BranchQRCode.BranchImageFormat.JPEG : io.branch.referral.QRCode.BranchQRCode.BranchImageFormat.PNG);
            }
            if (params.has("center_logo_url")) {
                branchQRCode.setCenterLogo(params.getString("center_logo_url"));
            }
        } catch(Exception e) {
          Log.e("Error Creating QR Code from JSON", e.toString());
        }

        return branchQRCode;
    }

    private static String _branchKey;

    /**
     * Callback for Unity.
     */
    private static class BranchReferralInitListenerUnityCallback implements Branch.BranchReferralInitListener, Branch.BranchReferralStateChangedListener, Branch.BranchListResponseListener, Branch.BranchLinkCreateListener, Branch.BranchLinkShareListener, BranchQRCode.BranchQRCodeDataHandler {

        // used for the default listener before C# is running
        public BranchReferralInitListenerUnityCallback() {

        }

        public BranchReferralInitListenerUnityCallback(String callbackId) {
            _callbackId = callbackId;
        }

        @Override
        public void onInitFinished(JSONObject params, BranchError branchError) {
            //Log.i(TAG, "BranchReferralInitListenerUnityCallback.onInitFinished(JSONObject params, BranchError branchError)");
            _waitForFirstPayload = false;

            // if we don't have a callback id yet, save params until we do
            if (_callbackId.isEmpty() && branchError == null) {
                _lastJSON = params;
            } else {
                handleCallback(params, branchError);
            }
        }

        // set a callbackId and respond with the last data we have
        public void setCallbackIDAndClearCachedParams(String callbackId, Boolean isBUO) {
            //Log.i(TAG, "BranchReferralInitListenerUnityCallback.setCallbackIDAndClearCachedParams(String callbackId, Boolean isBUO)");
            _callbackId = callbackId;
            _isBUO = isBUO;

            // First request could be in flight, wait for it.
            if (!_waitForFirstPayload) {
                handleCallback(_lastJSON, null);
                _lastJSON = null;
            }
        }

        private void handleCallback(JSONObject params, BranchError branchError) {
            //Log.i(TAG, "BranchReferralInitListenerUnityCallback.handleCallback(JSONObject params, BranchError branchError)");
            if (_isBUO) {
                // recreate the convienience API from the Branch SDK
                BranchUniversalObject branchUniversalObject = BranchUniversalObject.getReferredBranchUniversalObject();
                LinkProperties linkProperties = LinkProperties.getReferredLinkProperties();
                try {
                    JSONObject buoParams = new JSONObject();
                    buoParams.put("universalObject", _jsonObjectFromBranchUniversalObject(branchUniversalObject));
                    buoParams.put("linkProperties", _jsonObjectFromLinkProperties(linkProperties));

                    _sendMessageWithWithBranchError("_asyncCallbackWithBranchUniversalObject", branchError, "params", buoParams);
                }
                catch (JSONException jsone) {
                    jsone.printStackTrace();
                }

            } else {
                _sendMessageWithWithBranchError("_asyncCallbackWithParams", branchError, "params", params);
            }
        }

        @Override
        public void onStateChanged(boolean changed, BranchError branchError) {
            _sendMessageWithWithBranchError("_asyncCallbackWithStatus", branchError, "status", changed);
        }

        @Override
        public void onReceivingResponse(JSONArray list, BranchError branchError) {
            _sendMessageWithWithBranchError("_asyncCallbackWithList", branchError, "list", list);
        }

        @Override
        public void onLinkCreate(String url, BranchError branchError) {
            _sendMessageWithWithBranchError("_asyncCallbackWithUrl", branchError, "url", url);
        }

        @Override
        public void onShareLinkDialogLaunched() {

        }

        @Override
        public void onShareLinkDialogDismissed() {
            try {
                if (!_linkShared) {
                    JSONObject params = new JSONObject();
                    params.put("sharedLink", "");
                    params.put("sharedChannel", "");

                    _sendMessageWithWithBranchError("_asyncCallbackWithParams", null, "params", params);
                }
            }
            catch (JSONException jsone) {
                jsone.printStackTrace();
            }
        }

        @Override
        public void onLinkShareResponse(String sharedLink, String sharedChannel, BranchError branchError) {
            try {
                JSONObject params = new JSONObject();
                params.put("sharedLink", sharedLink);
                params.put("sharedChannel", sharedChannel);

                _sendMessageWithWithBranchError("_asyncCallbackWithParams", branchError, "params", params);
                _linkShared = true;
            }
            catch (JSONException jsone) {
                jsone.printStackTrace();
            }
        }

        @Override
        public void onChannelSelected(java.lang.String selectedChannel) {
            _sendMessageWithWithBranchError("_asyncCallbackWithParams", null, "selectedChannel", selectedChannel);
            _linkShared = true;
        }

        @Override
        public void onSuccess(byte[] qrCodeData){
            _sendMessageWithWithBranchError("_asyncCallbackWithData", null, "data", Base64.getEncoder().encodeToString(qrCodeData));
        }

        @Override
        public void onFailure(Exception e) {
            Log.e("Branch QR Code Generation Error", e.toString());
        }

        private void _sendMessageWithWithBranchError(String asyncCallbackMethod, BranchError branchError, String extraKey, Object extraValue) {
            try {
                JSONObject responseObject = new JSONObject();
                responseObject.put("callbackId", _callbackId);
                responseObject.put(extraKey, extraValue);
                responseObject.put("error", branchError == null ? null : branchError.getMessage());

                String respString = responseObject.toString();
                UnityPlayer.UnitySendMessage("Branch", asyncCallbackMethod, respString);
            }
            catch (JSONException jsone) {
                // nothing to do here
                jsone.printStackTrace();
            }
        }

        private String _callbackId = "";
        private Boolean _linkShared = false;
        private Boolean _isBUO = false;

        // TODO: Check with Benas if this needs to be synchronized.
        private Boolean _waitForFirstPayload = true;
        private JSONObject _lastJSON = null;
    }
}
