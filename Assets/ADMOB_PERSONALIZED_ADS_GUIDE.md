# AdMob Personalized Ads Implementation Guide

## Overview
This guide explains how to properly implement personalized ads in AdMob using Google's User Messaging Platform (UMP) in Unity. Personalized ads provide better user experience and higher revenue, but require proper consent handling.

## What Are Personalized Ads?

### **Personalized Ads:**
- Use user data (interests, demographics, behavior) to show relevant ads
- Higher click-through rates and revenue
- Better user experience with relevant content
- Require explicit consent for data collection

### **Non-Personalized Ads:**
- Don't use user data for targeting
- Lower revenue potential
- Still require consent to show ads
- Fallback option when users don't consent to personalization

## Consent Types in UMP

### **1. CanRequestAds()**
- Returns `true` if user consented to see ads at all
- Basic consent for ad display

### **2. PrivacyOptionsRequirementStatus**
- `Required`: Privacy options are required (GDPR regions)
- `NotRequired`: Privacy options are not required (non-GDPR regions)
- Used to determine if personalized ads are allowed

### **3. Privacy Policy Acceptance**
- Local flag indicating if user accepted privacy policy
- Can be used as an indicator for personalized ads consent

## Implementation

### **1. Consent Checking Methods**
```csharp
/// <summary>
/// Check if user consented to ads
/// </summary>
public bool CanRequestAds()
{
    return ConsentInformation.CanRequestAds();
}

/// <summary>
/// Check if we should use personalized ads based on user consent and privacy policy
/// </summary>
public bool ShouldUsePersonalizedAds()
{
    // Check if user consented to ads
    if (!ConsentInformation.CanRequestAds())
        return false;
    
    // Check if privacy policy is accepted (this indicates user is okay with data collection)
    if (!SaveSystem.Inst.GeneralData.IsPrivacyPolicyAccepted)
        return false;
    
    // For GDPR regions, check if privacy options are required
    if (ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required)
    {
        // In GDPR regions, assume non-personalized unless explicitly configured
        return false;
    }
    
    // For non-GDPR regions, use personalized ads if consent is given
    return true;
}
```

### **2. Create Personalized AdRequest**
```csharp
/// <summary>
/// Create an AdRequest with proper personalization settings based on user consent
/// </summary>
private AdRequest CreateAdRequest()
{
    if (!CanRequestAds())
    {
        Debug.Log("#ads# Cannot create ad request - no consent");
        return null;
    }
    
    var builder = new AdRequest.Builder();
    
    if (ShouldUsePersonalizedAds())
    {
        // User consented to personalized ads - no additional configuration needed
        Debug.Log("#ads# Creating personalized ad request");
    }
    else
    {
        // User consented to ads but not personalized ads, or privacy policy not accepted
        Debug.Log("#ads# Creating non-personalized ad request");
        builder.AddExtra("npa", "1"); // Non-personalized ads flag
    }
    
    return builder.Build();
}
```

### **3. Use in Ad Loading Methods**
```csharp
// Instead of: var adRequest = new AdRequest();
// Use: var adRequest = CreateAdRequest();

private void LoadBannerAd()
{
    // ... other checks ...
    
    var adRequest = CreateAdRequest();
    if (adRequest == null)
    {
        Debug.Log("#ads# Cannot load banner - no valid ad request");
        return;
    }
    
    _bannerView.LoadAd(adRequest);
}
```

## Consent Flow Scenarios

### **Scenario 1: User Consents to Personalized Ads**
```
1. User sees consent form
2. User selects "Accept All" or similar
3. User accepts privacy policy
4. CanRequestAds() = true
5. ShouldUsePersonalizedAds() = true
6. CreateAdRequest() returns personalized AdRequest
7. Higher revenue ads are shown
```

### **Scenario 2: User Consents to Ads but Not Personalization**
```
1. User sees consent form
2. User selects "Accept Essential Only" or similar
3. User doesn't accept privacy policy OR in GDPR region
4. CanRequestAds() = true
5. ShouldUsePersonalizedAds() = false
6. CreateAdRequest() returns non-personalized AdRequest (with "npa" flag)
7. Lower revenue but compliant ads are shown
```

### **Scenario 3: User Denies All Ads**
```
1. User sees consent form
2. User selects "Reject All" or similar
3. CanRequestAds() = false
4. CanRequestPersonalizedAds() = false
5. CreateAdRequest() returns null
6. No ads are shown
```

## Privacy Compliance

### **GDPR Requirements:**
- **Explicit Consent**: Users must actively choose to accept personalized ads
- **Granular Control**: Users should be able to accept ads but reject personalization
- **Clear Information**: Users must understand what data is collected
- **Easy Withdrawal**: Users should be able to change consent later

### **CCPA Requirements:**
- **Right to Opt-Out**: Users can opt out of data sale
- **Clear Notice**: Privacy policy must explain data usage
- **No Discrimination**: Cannot deny service for opting out

### **Best Practices:**
- Always check both consent types before creating ad requests
- Provide clear explanations of what personalized ads mean
- Offer non-personalized ads as an alternative
- Respect user choices immediately
- Log consent status for debugging

## Testing Personalized Ads

### **Debug Methods:**
```csharp
public void LogConsentStatus()
{
    Debug.Log($"Consent Status:");
    Debug.Log($"- CanRequestAds: {ConsentInformation.CanRequestAds()}");
    Debug.Log($"- ShouldUsePersonalizedAds: {ShouldUsePersonalizedAds()}");
    Debug.Log($"- PrivacyOptionsRequirementStatus: {ConsentInformation.PrivacyOptionsRequirementStatus}");
    Debug.Log($"- IsConsentFormAvailable: {ConsentInformation.IsConsentFormAvailable()}");
    Debug.Log($"- ConsentStatus: {ConsentInformation.ConsentStatus}");
    Debug.Log($"- PrivacyPolicyAccepted: {SaveSystem.Inst.GeneralData.IsPrivacyPolicyAccepted}");
}
```

### **Testing Scenarios:**
1. **First Launch** - Test consent form flow
2. **Personalized Consent** - Verify personalized ads load
3. **Non-Personalized Consent** - Verify non-personalized ads load
4. **No Consent** - Verify no ads load
5. **Consent Change** - Test updating consent settings

## Revenue Impact

### **Personalized vs Non-Personalized:**
- **Personalized Ads**: 2-5x higher eCPM (effective cost per mille)
- **Non-Personalized Ads**: Lower eCPM but still monetizable
- **No Ads**: Zero revenue but compliant

### **User Experience:**
- **Personalized**: More relevant, higher engagement
- **Non-Personalized**: Less relevant but still functional
- **No Ads**: Clean experience but no monetization

## Implementation Checklist

- [ ] Add `CanRequestAds()` method
- [ ] Add `ShouldUsePersonalizedAds()` method
- [ ] Create `CreateAdRequest()` method with personalization logic
- [ ] Update all ad loading methods to use `CreateAdRequest()`
- [ ] Add null checks for ad requests
- [ ] Add proper logging for debugging
- [ ] Test all consent scenarios
- [ ] Verify privacy compliance
- [ ] Monitor revenue impact

## Common Mistakes

### ❌ **Wrong: Always Using Personalized Ads**
```csharp
// DON'T DO THIS
var adRequest = new AdRequest(); // Always personalized
```

### ✅ **Correct: Checking Consent First**
```csharp
// DO THIS
var adRequest = CreateAdRequest(); // Respects user consent
```

### ❌ **Wrong: Ignoring Non-Personalized Option**
```csharp
// DON'T DO THIS
if (!ShouldUsePersonalizedAds())
{
    return; // User gets no ads at all
}
```

### ✅ **Correct: Providing Non-Personalized Alternative**
```csharp
// DO THIS
if (ShouldUsePersonalizedAds())
{
    // Use personalized ads
}
else if (CanRequestAds())
{
    // Use non-personalized ads
}
else
{
    // No ads
}
```

## Summary

Implementing personalized ads properly requires:
1. **Checking both consent types** before creating ad requests
2. **Providing non-personalized alternatives** when users don't consent to personalization
3. **Respecting user choices** immediately
4. **Testing all scenarios** thoroughly
5. **Monitoring compliance** and revenue impact

This ensures your app maximizes revenue while maintaining privacy compliance and user trust. 