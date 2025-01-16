#include "BranchiOSWrapper.h"
#include "Branch.h"
#include "BranchEvent.h"
#include "BranchConstants.h"
#include "BranchUniversalObject.h"
#include "BranchQRCode.h"
#import "UnityAppController.h"
#import "Branch+Validator.h"

@interface BranchUnityWrapper()
@property (nonatomic, strong, readwrite) NSString *branchKey;
@property (nonatomic, strong, readwrite) NSDictionary *launchOptions;
@end

#pragma mark - Private notification class implementation

@implementation BranchUnityWrapper

+ (BranchUnityWrapper *)sharedInstance {
    static BranchUnityWrapper *wrapper;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        wrapper = [BranchUnityWrapper new];
    });
    return wrapper;
}

- (instancetype)init {
    if (self = [super init]) {
        UnityRegisterAppDelegateListener(self);
    }
    return self;
}

- (void)dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

- (void)didFinishLaunching:(NSNotification *)notification {
    self.launchOptions = notification.userInfo;
}

- (void)onOpenURL:(NSNotification *)notification {
    NSURL *openURL = notification.userInfo[@"url"];
    [[Branch getInstance] handleDeepLink:openURL];
}

- (BOOL)continueUserActivity:(NSUserActivity *)userActivity {
    return [[Branch getInstance] continueUserActivity:userActivity];
}

@end


#pragma mark - Converter methods

static NSString *CreateNSString(const char *string) {
    if (string == NULL) {
        return nil;
    }

    return [NSString stringWithUTF8String:string];
}

static NSURL *CreateNSUrl(const char *string) {
    return [NSURL URLWithString:CreateNSString(string)];
}

static NSDate *CreateNSDate(char *strDate) {
    NSString *str = CreateNSString(strDate);
    NSDateFormatter *formatter= [[NSDateFormatter alloc] init];
    formatter.dateFormat = @"yyyy-MM-ddTHH:mm:ssZ";

    return [formatter dateFromString:str];
}

static NSDate *CreateNSDate(NSString *strDate) {
    NSDateFormatter *formatter= [[NSDateFormatter alloc] init];
    formatter.dateFormat = @"yyyy-MM-ddTHH:mm:ssZ";
    
    return [formatter dateFromString:strDate];
}

static NSString *CreateNSStringFromNSDate(NSDate *date) {
    NSDateFormatter *formatter= [[NSDateFormatter alloc] init];
    formatter.dateFormat = @"yyyy-MM-ddTHH:mm:ssZ";
    
    return [formatter stringFromDate:date];
}

static NSDictionary *dictionaryFromJsonString(const char *jsonString) {
    NSData *jsonData = [[NSData alloc] initWithBytes:jsonString length:strlen(jsonString)];
    NSDictionary *dictionary = [NSJSONSerialization JSONObjectWithData:jsonData options:kNilOptions error:nil];
    
    return dictionary;
}

static NSArray *arrayFromJsonString(const char *jsonString) {
    NSData *jsonData = [[NSData alloc] initWithBytes:jsonString length:strlen(jsonString)];
    NSArray *array = [NSJSONSerialization JSONObjectWithData:jsonData options:kNilOptions error:nil];
    
    return array;
}

static const char *jsonCStringFromDictionary(NSDictionary *dictionary) {
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dictionary options:kNilOptions error:nil];
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];

    return [jsonString cStringUsingEncoding:NSUTF8StringEncoding];
}

static NSDictionary *dictFromBranchUniversalObject(BranchUniversalObject *universalObject) {
    NSDictionary *universalObjectDict = [NSDictionary dictionary];
    
    if (universalObject) {
        universalObjectDict = @{
            BRANCH_LINK_DATA_KEY_CANONICAL_IDENTIFIER: universalObject.canonicalIdentifier ? universalObject.canonicalIdentifier : @"",
            BRANCH_LINK_DATA_KEY_CANONICAL_URL: universalObject.canonicalUrl ? universalObject.canonicalUrl : @"",
            BRANCH_LINK_DATA_KEY_OG_TITLE: universalObject.title ? universalObject.title : @"",
            BRANCH_LINK_DATA_KEY_OG_DESCRIPTION: universalObject.contentDescription ? universalObject.contentDescription : @"",
            BRANCH_LINK_DATA_KEY_OG_IMAGE_URL: universalObject.imageUrl ? universalObject.imageUrl : @"",
            BRANCH_LINK_DATA_KEY_PUBLICLY_INDEXABLE: universalObject.publiclyIndex ? [[NSNumber numberWithInteger:universalObject.publiclyIndex] stringValue]: @"",
            BRANCH_LINK_DATA_KEY_LOCALLY_INDEXABLE: universalObject.locallyIndex ? [[NSNumber numberWithInteger:universalObject.locallyIndex] stringValue]: @"",
            BRANCH_LINK_DATA_KEY_KEYWORDS: universalObject.keywords ? universalObject.keywords : @"",
            BRANCH_LINK_DATA_KEY_CONTENT_EXPIRATION_DATE: universalObject.expirationDate ? @(1000 * [universalObject.expirationDate timeIntervalSince1970]) : @"",
            @"metadata": universalObject.contentMetadata ? universalObject.contentMetadata.dictionary : @"",
        };
    }

    return universalObjectDict;
}

static NSDictionary *dictFromBranchLinkProperties(BranchLinkProperties *linkProperties) {
    NSDictionary *linkPropertiesDict = [NSDictionary dictionary];
    
    if (linkProperties) {
        linkPropertiesDict = @{
            @"~tags": linkProperties.tags ? linkProperties.tags : @"",
            @"~feature": linkProperties.feature ? linkProperties.feature : @"",
            @"~alias": linkProperties.alias ? linkProperties.alias : @"",
            @"~channel": linkProperties.channel ? linkProperties.channel : @"",
            @"~stage": linkProperties.stage ? linkProperties.stage : @"",
            @"~duration": linkProperties.matchDuration ? [[NSNumber numberWithInteger:linkProperties.matchDuration] stringValue] : @"",
            @"control_params": linkProperties.controlParams ? linkProperties.controlParams : @""
        };
    }
    
    return linkPropertiesDict;
}

static BranchUniversalObject* branchuniversalObjectFormDict(NSDictionary *universalObjectDict) {
    BranchUniversalObject *universalObject = [[BranchUniversalObject alloc] init];
    
    if (universalObjectDict[BRANCH_LINK_DATA_KEY_CANONICAL_IDENTIFIER]) {
        universalObject.canonicalIdentifier = universalObjectDict[BRANCH_LINK_DATA_KEY_CANONICAL_IDENTIFIER];
    }
    if (universalObjectDict[BRANCH_LINK_DATA_KEY_CANONICAL_URL]) {
        universalObject.canonicalUrl = universalObjectDict[BRANCH_LINK_DATA_KEY_CANONICAL_URL];
    }
    if (universalObjectDict[BRANCH_LINK_DATA_KEY_OG_TITLE]) {
        universalObject.title = universalObjectDict[BRANCH_LINK_DATA_KEY_OG_TITLE];
    }
    if (universalObjectDict[BRANCH_LINK_DATA_KEY_OG_DESCRIPTION]) {
        universalObject.contentDescription = universalObjectDict[BRANCH_LINK_DATA_KEY_OG_DESCRIPTION];
    }
    if (universalObjectDict[BRANCH_LINK_DATA_KEY_OG_IMAGE_URL]) {
        universalObject.imageUrl = universalObjectDict[BRANCH_LINK_DATA_KEY_OG_IMAGE_URL];
    }
    
    if (universalObjectDict[BRANCH_LINK_DATA_KEY_PUBLICLY_INDEXABLE]) {
        if (universalObjectDict[BRANCH_LINK_DATA_KEY_PUBLICLY_INDEXABLE] == 0) {
            universalObject.publiclyIndex = BranchContentIndexModePublic;
        }
        else {
            universalObject.publiclyIndex = BranchContentIndexModePrivate;
        }
    }
    
    if (universalObjectDict[BRANCH_LINK_DATA_KEY_LOCALLY_INDEXABLE]) {
        if (universalObjectDict[BRANCH_LINK_DATA_KEY_LOCALLY_INDEXABLE] == 0) {
            universalObject.locallyIndex = BranchContentIndexModePublic;
        }
        else {
            universalObject.locallyIndex = BranchContentIndexModePrivate;
        }
    }
    
    if (universalObjectDict[BRANCH_LINK_DATA_KEY_CONTENT_EXPIRATION_DATE]) {
        universalObject.expirationDate = [NSDate dateWithTimeIntervalSince1970:[universalObjectDict[BRANCH_LINK_DATA_KEY_CONTENT_EXPIRATION_DATE] integerValue]/1000];
    }
    
    if (universalObjectDict[BRANCH_LINK_DATA_KEY_KEYWORDS]) {
        universalObject.keywords = [universalObjectDict[BRANCH_LINK_DATA_KEY_KEYWORDS] copy];
    }
    
    if (universalObjectDict[@"metadata"]) {
        
        NSDictionary *dict = dictionaryFromJsonString([universalObjectDict[@"metadata"] cStringUsingEncoding:NSUTF8StringEncoding]);
        universalObject.contentMetadata = [BranchContentMetadata contentMetadataWithDictionary:dict];
        
        NSMutableDictionary *mutableDict = [dict mutableCopy];
        [mutableDict removeObjectForKey:@"$content_schema"];
        [mutableDict removeObjectForKey:@"$quantity"];
        [mutableDict removeObjectForKey:@"$price"];
        [mutableDict removeObjectForKey:@"$currency"];
        [mutableDict removeObjectForKey:@"$sku"];
        [mutableDict removeObjectForKey:@"$product_name"];
        [mutableDict removeObjectForKey:@"$product_brand"];
        [mutableDict removeObjectForKey:@"$product_category"];
        [mutableDict removeObjectForKey:@"$product_variant"];
        [mutableDict removeObjectForKey:@"$condition"];
        [mutableDict removeObjectForKey:@"$rating_average"];
        [mutableDict removeObjectForKey:@"$rating_count"];
        [mutableDict removeObjectForKey:@"$rating_max"];
        [mutableDict removeObjectForKey:@"$address_street"];
        [mutableDict removeObjectForKey:@"$address_city"];
        [mutableDict removeObjectForKey:@"$address_region"];
        [mutableDict removeObjectForKey:@"$address_country"];
        [mutableDict removeObjectForKey:@"$address_postal_code"];
        [mutableDict removeObjectForKey:@"$latitude"];
        [mutableDict removeObjectForKey:@"$longitude"];
        [mutableDict removeObjectForKey:@"$image_captions"];

        for (NSString *key in mutableDict.keyEnumerator) {
            NSString *value = mutableDict[key];
            universalObject.contentMetadata.customMetadata[key] = value;
        }
    }
    
    return universalObject;
}

static BranchLinkProperties *branchLinkPropertiesFormDict(NSDictionary *linkPropertiesDict) {
    BranchLinkProperties *linkProperties = [[BranchLinkProperties alloc] init];
    
    if (linkPropertiesDict[@"~tags"]) {
        linkProperties.tags = linkPropertiesDict[@"~tags"];
    }
    if (linkPropertiesDict[@"~feature"]) {
        linkProperties.feature = linkPropertiesDict[@"~feature"];
    }
    if (linkPropertiesDict[@"~alias"]) {
        linkProperties.alias = linkPropertiesDict[@"~alias"];
    }
    if (linkPropertiesDict[@"~channel"]) {
        linkProperties.channel = linkPropertiesDict[@"~channel"];
    }
    if (linkPropertiesDict[@"~stage"]) {
        linkProperties.stage = linkPropertiesDict[@"~stage"];
    }
    if (linkPropertiesDict[@"~duration"]) {
        linkProperties.matchDuration = [linkPropertiesDict[@"~duration"] intValue];
    }
    if (linkPropertiesDict[@"control_params"]) {
        linkProperties.controlParams = [linkPropertiesDict[@"control_params"] copy];
    }
    
    return linkProperties;
}

#pragma mark - Callbacks

static callbackWithParams callbackWithParamsForCallbackId(char *callbackId) {
    NSString *callbackString = CreateNSString(callbackId);

    return ^(NSDictionary *params, NSError *error) {
        id errorDictItem = error ? [error description] : [NSNull null];
        id paramsDictItem = params ?: [NSNull null];
        NSDictionary *callbackDict = @{ @"callbackId": callbackString, @"params": paramsDictItem, @"error": errorDictItem };
        
        UnitySendMessage("Branch", "_asyncCallbackWithParams", jsonCStringFromDictionary(callbackDict));
    };
}

static callbackWithStatus callbackWithStatusForCallbackId(char *callbackId) {
    NSString *callbackString = CreateNSString(callbackId);

    return ^(BOOL status, NSError *error) {
        id errorDictItem = error ? [error description] : [NSNull null];
        NSDictionary *callbackDict = @{ @"callbackId": callbackString, @"status": @(status), @"error": errorDictItem };
        
        UnitySendMessage("Branch", "_asyncCallbackWithStatus", jsonCStringFromDictionary(callbackDict));
    };
}

static callbackWithList callbackWithListForCallbackId(char *callbackId) {
    NSString *callbackString = CreateNSString(callbackId);

    return ^(NSArray *list, NSError *error) {
        id errorDictItem = error ? [error description] : [NSNull null];
        NSDictionary *callbackDict = @{ @"callbackId": callbackString, @"list": list, @"error": errorDictItem };
        
        UnitySendMessage("Branch", "_asyncCallbackWithList", jsonCStringFromDictionary(callbackDict));
    };
}

static callbackWithUrl callbackWithUrlForCallbackId(char *callbackId) {
    NSString *callbackString = CreateNSString(callbackId);

    return ^(NSString *url, NSError *error) {
        id errorDictItem = error ? [error description] : [NSNull null];
        NSDictionary *callbackDict = @{ @"callbackId": callbackString, @"url": url, @"error": errorDictItem };
        
        UnitySendMessage("Branch", "_asyncCallbackWithUrl", jsonCStringFromDictionary(callbackDict));
    };
}

static callbackWithData callbackWithDataForCallbackId(char *callbackId) {
    NSString *callbackString = CreateNSString(callbackId);

    return ^(NSData *data, NSError *error) {
        NSString *string;
        NSDictionary *params;
        id errorDictItem = error ? [error description] : [NSNull null];

        if(data) {
            string = [data base64EncodedStringWithOptions:0];
            params = @{@"callbackId": callbackString, @"data": string, @"error": errorDictItem};
        } else {
            params = @{@"callbackId": callbackString, @"data": @"", @"error": errorDictItem};
        }

        UnitySendMessage("Branch", "_asyncCallbackWithData", jsonCStringFromDictionary(params));
    };
}

static callbackWithBranchUniversalObject callbackWithBranchUniversalObjectForCallbackId(char *callbackId) {
    NSString *callbackString = CreateNSString(callbackId);
    
    return ^(BranchUniversalObject *universalObject, BranchLinkProperties *linkProperties, NSError *error) {
        id errorDictItem = error ? [error description] : [NSNull null];
        
        NSDictionary *params = @{@"universalObject": dictFromBranchUniversalObject(universalObject), @"linkProperties": dictFromBranchLinkProperties(linkProperties)};
        NSDictionary *callbackDict = @{ @"callbackId": callbackString, @"params": params, @"error": errorDictItem };
        
        UnitySendMessage("Branch", "_asyncCallbackWithBranchUniversalObject", jsonCStringFromDictionary(callbackDict));
    };
}

static callbackWithShareCompletion callbackWithShareCompletionForCallbackId(char *callbackId) {
    NSString *callbackString = CreateNSString(callbackId);
    
    return ^(NSString *activityType, BOOL completed, NSError *error) {
        id errorDictItem = [NSNull null];
        
        NSDictionary *params;
        if( activityType != nil) {
            params = @{@"sharedLink": @"", @"sharedChannel": activityType};
        } else {
            params = @{@"sharedLink": @"", @"sharedChannel": @""};
        }
        
        NSDictionary *callbackDict = @{ @"callbackId": callbackString, @"params": params, @"error": errorDictItem };
        
        UnitySendMessage("Branch", "_asyncCallbackWithParams", jsonCStringFromDictionary(callbackDict));
    };
}


#pragma mark - Key methods

void _setBranchKey(char *branchKey, char* branchSDKVersion) {
    [BranchUnityWrapper sharedInstance].branchKey = CreateNSString(branchKey);
    [[Branch getInstance] registerPluginName:@"Unity" version:CreateNSString(branchSDKVersion)];
}

#pragma mark - InitSession methods

void _initSessionWithCallback(char *callbackId) {
    [[Branch getInstance] initSessionWithLaunchOptions:[BranchUnityWrapper sharedInstance].launchOptions andRegisterDeepLinkHandler:callbackWithParamsForCallbackId(callbackId)];
    [[Branch getInstance] notifyNativeToInit];
}

void _initSessionWithUniversalObjectCallback(char *callbackId) {
    [[Branch getInstance] initSessionWithLaunchOptions:[BranchUnityWrapper sharedInstance].launchOptions andRegisterDeepLinkHandlerUsingBranchUniversalObject:callbackWithBranchUniversalObjectForCallbackId(callbackId)];
    [[Branch getInstance] notifyNativeToInit];
}

#pragma mark - Session Item methods

const char *_getFirstReferringBranchUniversalObject() {
    BranchUniversalObject* universalObject = [[Branch getInstance] getFirstReferringBranchUniversalObject];
    return jsonCStringFromDictionary(dictFromBranchUniversalObject(universalObject));
}

const char *_getFirstReferringBranchLinkProperties() {
    BranchLinkProperties *linkProperties = [[Branch getInstance] getFirstReferringBranchLinkProperties];
    return jsonCStringFromDictionary(dictFromBranchLinkProperties(linkProperties));
}

const char *_getLatestReferringBranchUniversalObject() {
    BranchUniversalObject *universalObject = [[Branch getInstance]getLatestReferringBranchUniversalObject];
    return jsonCStringFromDictionary(dictFromBranchUniversalObject(universalObject));
}

const char *_getLatestReferringBranchLinkProperties() {
    BranchLinkProperties *linkProperties = [[Branch getInstance] getLatestReferringBranchLinkProperties];
    return jsonCStringFromDictionary(dictFromBranchLinkProperties(linkProperties));
}

void _resetUserSession() {
    [[Branch getInstance] resetUserSession];
}

void _setIdentity(char *userId) {
    [[Branch getInstance] setIdentity:CreateNSString(userId)];
}

void _setIdentityWithCallback(char *userId, char *callbackId) {
    [[Branch getInstance] setIdentity:CreateNSString(userId) withCallback:callbackWithParamsForCallbackId(callbackId)];
}

void _logout() {
    [[Branch getInstance] logout];
}

# pragma mark - Configuation methods
 
void _enableLogging() {
    [Branch enableLogging];
}

void _setRetryInterval(int retryInterval) {
    [[Branch getInstance] setRetryInterval:retryInterval];
}

void _setMaxRetries(int maxRetries) {
    [[Branch getInstance] setMaxRetries:maxRetries];
}

void _setNetworkTimeout(int timeout) {
    [[Branch getInstance] setNetworkTimeout:timeout];
}

void _registerView(char *universalObjectJson) {
    NSDictionary *universalObjectDict = dictionaryFromJsonString(universalObjectJson);
    BranchUniversalObject *obj = branchuniversalObjectFormDict(universalObjectDict);
    
    BranchEvent* event = [[BranchEvent alloc] initWithName:BranchStandardEventViewItem];
    [event.contentItems arrayByAddingObject:obj];
    [event logEvent];
}

void _listOnSpotlight(char *universalObjectJson) {
    NSDictionary *universalObjectDict = dictionaryFromJsonString(universalObjectJson);
    BranchUniversalObject *obj = branchuniversalObjectFormDict(universalObjectDict);
    
    [obj listOnSpotlight];
}

void _setRequestMetadata(char *key, char *value) {
    [[Branch getInstance] setRequestMetadataKey:CreateNSString(key) value:CreateNSString(value)];
}

void _addFacebookPartnerParameter(char *name, char *value) {
    [[Branch getInstance] addFacebookPartnerParameterWithName:CreateNSString(name) value:CreateNSString(value)];
}

void _clearPartnerParameters() {
    [[Branch getInstance] clearPartnerParameters];
}

void _setTrackingDisabled(BOOL value) {
    [Branch setTrackingDisabled:value];
}

void _setDMAParamsForEEA(BOOL eeaRegion, BOOL adPersonalizationConsent, BOOL adUserDataUsageConsent) {
    [Branch setDMAParamsForEEA:eeaRegion AdPersonalizationConsent:adPersonalizationConsent AdUserDataUsageConsent:adUserDataUsageConsent];
}

#pragma mark - Send event methods

void _sendEvent(char *eventJson) {
    NSDictionary *eventDict = dictionaryFromJsonString(eventJson);
    if (eventDict == nil) {
        return;
    }
    
    BranchEvent *event = nil;
    
    if (eventDict[@"event_name"]) {
        event = [[BranchEvent alloc] initWithName:eventDict[@"event_name"]];
    }
    else {
        return;
    }
    
    if (eventDict[@"transaction_id"]) {
        event.transactionID = eventDict[@"transaction_id"];
    }
    if (eventDict[@"customer_event_alias"]) {
        event.alias = eventDict[@"customer_event_alias"];
    }
    if (eventDict[@"affiliation"]) {
        event.affiliation = eventDict[@"affiliation"];
    }
    if (eventDict[@"coupon"]) {
        event.coupon = eventDict[@"coupon"];
    }
    if (eventDict[@"currency"]) {
        event.currency = eventDict[@"currency"];
    }
    if (eventDict[@"tax"]) {
        event.tax = eventDict[@"tax"];
    }
    if (eventDict[@"revenue"]) {
        event.revenue = eventDict[@"revenue"];
    }
    if (eventDict[@"description"]) {
        event.eventDescription = eventDict[@"description"];
    }
    if (eventDict[@"shipping"]) {
        event.shipping = eventDict[@"shipping"];
    }
    if (eventDict[@"search_query"]) {
        event.searchQuery = eventDict[@"search_query"];
    }
    if (eventDict[@"custom_data"]) {
        event.customData = [eventDict[@"custom_data"] copy];
    }
    if (eventDict[@"content_items"]) {
        NSArray *array = [eventDict[@"content_items"] copy];
        NSMutableArray *buoArray = [[NSMutableArray alloc] init];
        
        for (NSString* buoJson in array) {
            [buoArray addObject:branchuniversalObjectFormDict(dictionaryFromJsonString([buoJson cStringUsingEncoding:NSUTF8StringEncoding]))];
        }
        
        [event setContentItems:buoArray];
    }
    
    [event logEvent];
}

#pragma mark - Short URL Generation methods

void _getShortURLWithBranchUniversalObjectAndCallback(char *universalObjectJson, char *linkPropertiesJson, char *callbackId) {
    NSDictionary *universalObjectDict = dictionaryFromJsonString(universalObjectJson);
    NSDictionary *linkPropertiesDict = dictionaryFromJsonString(linkPropertiesJson);
    
    BranchUniversalObject *obj = branchuniversalObjectFormDict(universalObjectDict);
    BranchLinkProperties *prop = branchLinkPropertiesFormDict(linkPropertiesDict);
    
    [obj getShortUrlWithLinkProperties:prop andCallback:callbackWithUrlForCallbackId(callbackId)];
}

#pragma mark - QR Code Generation methods

//utility methods to convert hex color NSString to a UIColor

CGFloat colorComponentFrom(NSString* string, NSInteger start, NSInteger length){
    NSString *substring = [string substringWithRange: NSMakeRange(start, length)];
       NSString *fullHex = length == 2 ? substring : [NSString stringWithFormat: @"%@%@", substring, substring];
       unsigned hexComponent;
       [[NSScanner scannerWithString: fullHex] scanHexInt: &hexComponent];
       return hexComponent / 255.0;
}

UIColor* colorWithHexString(NSString* hexString) {
    NSString *colorString = [[hexString stringByReplacingOccurrencesOfString: @"#" withString: @""] uppercaseString];
       CGFloat alpha = 0.0f, red = 0.0f, blue = 0.0f, green = 0.0f;
       switch ([colorString length]) {
           case 3: // #RGB
               alpha = 1.0f;
               red = colorComponentFrom(colorString, 0, 1);
               green = colorComponentFrom(colorString, 1, 1);
               blue = colorComponentFrom(colorString, 2, 1);
               break;
           case 4: // #ARGB
               alpha = colorComponentFrom(colorString, 0, 1);
               red = colorComponentFrom(colorString, 1, 1);
               green = colorComponentFrom(colorString, 2, 1);
               blue = colorComponentFrom(colorString, 3, 1);
               break;
           case 6: // #RRGGBB
               alpha = 1.0f;
               red = colorComponentFrom(colorString, 0, 2);
               green = colorComponentFrom(colorString, 2, 2);
               blue = colorComponentFrom(colorString, 4, 2);
               break;
           case 8: // #AARRGGBB
               alpha = colorComponentFrom(colorString, 0, 2);
               red = colorComponentFrom(colorString, 2, 2);
               green = colorComponentFrom(colorString, 4, 2);
               blue = colorComponentFrom(colorString, 6, 2);
               break;
           default:
               NSLog(@"Branch Flutter SDK - Invalid color value. It should be a hex value of the form #RBG, #ARGB, #RRGGBB, or #AARRGGBB");
               break;
       }
       return [UIColor colorWithRed: red green: green blue: blue alpha: alpha];
}

void _generateBranchQRCode(char *universalObjectJson, char *linkPropertiesJson, char *qrCodeSettingsJson, char *callbackId) {
    NSDictionary *universalObjectDict = dictionaryFromJsonString(universalObjectJson);
    NSDictionary *linkPropertiesDict = dictionaryFromJsonString(linkPropertiesJson);
    NSDictionary *qrCodeSettingsDict = dictionaryFromJsonString(qrCodeSettingsJson);
    
    BranchUniversalObject *obj = branchuniversalObjectFormDict(universalObjectDict);
    BranchLinkProperties *prop = branchLinkPropertiesFormDict(linkPropertiesDict);
    
    BranchQRCode *branchQRCode = [[BranchQRCode alloc] init];
    NSString *codeColorAsHexString = [qrCodeSettingsDict valueForKey:@"code_color"];
    NSString *backgroundColorAsHexString = [qrCodeSettingsDict valueForKey:@"background_color"];
    branchQRCode.codeColor = colorWithHexString(codeColorAsHexString);
    branchQRCode.backgroundColor = colorWithHexString(backgroundColorAsHexString);
    branchQRCode.margin = [qrCodeSettingsDict valueForKey:@"margin"];
    branchQRCode.width = [qrCodeSettingsDict valueForKey:@"width"];
    NSNumber *imageFormatAsNumber = [qrCodeSettingsDict valueForKey:@"image_format"];
    BranchQRCodeImageFormat branchQRCodeImageFormat = imageFormatAsNumber == 0 ? BranchQRCodeImageFormatPNG : BranchQRCodeImageFormatJPEG;
    branchQRCode.imageFormat = branchQRCodeImageFormat;
    branchQRCode.centerLogo = [qrCodeSettingsDict valueForKey:@"center_logo_url"];
    
    [branchQRCode getQRCodeAsData:obj linkProperties:prop completion:callbackWithDataForCallbackId(callbackId)];
    [branchQRCode getQRCodeAsData:obj linkProperties:prop completion:^(NSData * _Nullable qrCode, NSError * _Nullable error) {
        NSString *string;
        if (qrCode) {
            string = [qrCode base64EncodedStringWithOptions:0];
            NSLog(string);

        }
    }];
}

#pragma mark - Share Link methods

void _shareLinkWithLinkProperties(char *universalObjectJson, char *linkPropertiesJson, char *message, char *callbackId) {
    NSDictionary *universalObjectDict = dictionaryFromJsonString(universalObjectJson);
    NSDictionary *linkPropertiesDict = dictionaryFromJsonString(linkPropertiesJson);
    
    BranchUniversalObject *obj = branchuniversalObjectFormDict(universalObjectDict);
    BranchLinkProperties *prop = branchLinkPropertiesFormDict(linkPropertiesDict);
    
    [obj showShareSheetWithLinkProperties:prop andShareText:CreateNSString(message) fromViewController:nil completionWithError:callbackWithShareCompletionForCallbackId(callbackId)];
}

void _validate() {
    [[Branch getInstance] validateSDKIntegration];
}
