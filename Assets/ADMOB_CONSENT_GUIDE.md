# AdMob Consent Flow Guide for Unity

## Overview
This guide explains the proper implementation of AdMob consent flow using Google's User Messaging Platform (UMP) in Unity. The consent flow ensures compliance with privacy regulations like GDPR and CCPA.

## Consent Flow Sequence

### 1. **Initialization Phase**
```csharp
private void Start()
{
    // Create a ConsentRequestParameters object
    ConsentRequestParameters request = new();
    
    // Check the current consent information status
    ConsentInformation.Update(request, OnConsentInfoUpdated);
}
```

### 2. **Consent Information Update**
```csharp
private void OnConsentInfoUpdated(FormError consentError)
{
    if (consentError != null)
    {
        Debug.LogError($"Consent update error: {consentError}");
        return;
    }

    // Check if we need to show the consent form
    ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
    {
        if (formError != null)
        {
            Debug.LogError($"Consent form error: {formError}");
            return;
        }

        // Consent has been gathered (or was already gathered)
        bool canRequestAds = ConsentInformation.CanRequestAds();
        
        // Save consent status to local storage
        SaveSystem.Inst.GeneralData.IsUserConsentForAds = canRequestAds;
        SaveSystem.Inst.GeneralData.IsUserConsentAsked = true;

        if (canRequestAds)
        {
            // Initialize ads only if consent is given
            InitializeAds();
        }
    });
}
```

### 3. **Ad Initialization (Only After Consent)**
```csharp
private void InitializeAds()
{
    // Always check consent before initializing
    if (!ConsentInformation.CanRequestAds())
    {
        Debug.Log("Cannot request ads - consent not given");
        return;
    }

    // Set request configuration for privacy compliance
    RequestConfiguration requestConfiguration = new()
    {
        PublisherPrivacyPersonalizationState = PublisherPrivacyPersonalizationState.Enabled
    };
    MobileAds.SetRequestConfiguration(requestConfiguration);

    // Initialize the Google Mobile Ads SDK
    MobileAds.Initialize((initStatus) =>
    {
        _isInitialized = true;
        
        // Load ads only after successful initialization
        if (Configs.AdsConfig.UseRewardedAds)
            LoadRewardedAd();
        if (Configs.AdsConfig.UseInterAds)
            LoadInterstitialAd();
        if (Configs.AdsConfig.UseBannerAds)
            LoadBannerAd();
    });
}
```

## Key Points

### **When to Check Consent:**
1. **Before Initializing Ads** - Never initialize AdMob without consent
2. **Before Loading Ads** - Check consent before loading any ad
3. **Before Showing Ads** - Final check before displaying ads
4. **Before Making Ad Requests** - Check before any ad-related API calls

### **Consent States:**
- **`ConsentInformation.CanRequestAds()`** - Returns true if user consented to ads
- **`ConsentInformation.IsConsentFormAvailable()`** - Returns true if consent form should be shown
- **`ConsentInformation.ConsentStatus`** - Current consent status (Required, NotRequired, Obtained, etc.)

### **Error Handling:**
```csharp
public void LogConsentStatus()
{
    Debug.Log($"Consent Status:");
    Debug.Log($"- CanRequestAds: {ConsentInformation.CanRequestAds()}");
    Debug.Log($"- IsConsentFormAvailable: {ConsentInformation.IsConsentFormAvailable()}");
    Debug.Log($"- PrivacyOptionsRequirementStatus: {ConsentInformation.PrivacyOptionsRequirementStatus}");
    Debug.Log($"- ConsentStatus: {ConsentInformation.ConsentStatus}");
}
```

## Common Mistakes to Avoid

### ❌ **Wrong: Checking Local Storage Instead of UMP**
```csharp
// DON'T DO THIS
if (!SaveSystem.Inst.GeneralData.IsUserConsentForAds)
{
    return;
}
```

### ✅ **Correct: Using UMP Consent Information**
```csharp
// DO THIS
if (!ConsentInformation.CanRequestAds())
{
    return;
}
```

### ❌ **Wrong: Initializing Ads Before Consent**
```csharp
// DON'T DO THIS
private void Start()
{
    InitializeAds(); // This should happen after consent
}
```

### ✅ **Correct: Initializing Ads After Consent**
```csharp
// DO THIS
private void Start()
{
    ConsentRequestParameters request = new();
    ConsentInformation.Update(request, OnConsentInfoUpdated);
}
```

## Implementation Checklist

- [ ] Call `ConsentInformation.Update()` in `Start()`
- [ ] Implement `OnConsentInfoUpdated()` callback
- [ ] Use `ConsentForm.LoadAndShowConsentFormIfRequired()`
- [ ] Check `ConsentInformation.CanRequestAds()` before initializing ads
- [ ] Check consent before loading ads
- [ ] Check consent before showing ads
- [ ] Handle consent errors properly
- [ ] Save consent status to local storage for persistence
- [ ] Add proper logging for debugging

## Testing Consent Flow

### **Debug Methods:**
```csharp
// Add this to your AdManager for testing
public void LogConsentStatus()
{
    Debug.Log($"Consent Status:");
    Debug.Log($"- CanRequestAds: {ConsentInformation.CanRequestAds()}");
    Debug.Log($"- IsConsentFormAvailable: {ConsentInformation.IsConsentFormAvailable()}");
    Debug.Log($"- PrivacyOptionsRequirementStatus: {ConsentInformation.PrivacyOptionsRequirementStatus}");
    Debug.Log($"- ConsentStatus: {ConsentInformation.ConsentStatus}");
}
```

### **Testing Scenarios:**
1. **First Launch** - Should show consent form
2. **Consent Given** - Should initialize and load ads
3. **Consent Denied** - Should not initialize ads
4. **Consent Already Given** - Should skip form and initialize ads
5. **Network Error** - Should handle gracefully

## Privacy Compliance

### **GDPR Compliance:**
- Always show consent form for EU users
- Respect user's choice
- Provide option to change consent later
- Don't track users who deny consent

### **CCPA Compliance:**
- Show privacy notice for California users
- Provide opt-out mechanism
- Don't sell personal information without consent

### **General Best Practices:**
- Be transparent about data collection
- Provide clear privacy policy
- Allow users to change consent settings
- Handle consent errors gracefully
- Log consent status for debugging

## Summary

The key to proper AdMob consent implementation is:
1. **Always check UMP consent status** before any ad operations
2. **Never initialize ads** without proper consent
3. **Handle all consent states** and errors gracefully
4. **Use UMP APIs** instead of local storage for consent decisions
5. **Test thoroughly** with different consent scenarios

This ensures your app complies with privacy regulations and provides a good user experience. 