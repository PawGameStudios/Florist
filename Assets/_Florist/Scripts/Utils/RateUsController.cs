using Google.Play.Review;
using System.Collections;
using UnityEngine;

public class RateUsController
{
    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo;

    public void Initialize()
    {
        _reviewManager = new ReviewManager();
    }

    public IEnumerator TryShowRateUs()
    {
        if (!SaveSystem.Inst.GeneralData.AskRateUs)
            yield break;

        if (SaveSystem.Inst.SaveData.IsFirstSession)
            yield break;

        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError(requestFlowOperation.Error.ToString());
            yield break;
        }
        _playReviewInfo = requestFlowOperation.GetResult();

        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null; // Reset the object
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError(launchFlowOperation.Error.ToString());
            yield break;
        }
        // The flow has finished. The API does not indicate whether the user
        // reviewed or not, or even whether the review dialog was shown. Thus, no
        // matter the result, we continue our app flow.
    }
}
