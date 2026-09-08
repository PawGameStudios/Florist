using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Keeps the Florist session alive while its separate Merge scene is open.</summary>
public sealed class MergeScenePortal : MonoBehaviour
{
    [SerializeField] private string _mergeScenePath = "Assets/_Florist/Scenes/MergeGame.unity";
    [SerializeField] private Canvas[] _floristCanvases;
    [SerializeField] private GameObject[] _floristInputAndCamera;
    private bool[] _canvasStates;
    private bool[] _objectStates;
    private bool _transitioning;
    private bool _isOpen;
    private Scene _previousScene;

    private void OnEnable() => Florist.Merge.MergeSceneExit.ReturnRequested += CloseMerge;
    private void OnDisable() => Florist.Merge.MergeSceneExit.ReturnRequested -= CloseMerge;

    public void OpenMerge()
    {
        if (_transitioning || _isOpen || !SaveSystem.Inst.SaveData.IsTutorialFinished)
            return;
        if (!Application.CanStreamedLevelBeLoaded(_mergeScenePath))
        {
            Debug.LogError("Merge scene is missing from the build scene list.", this);
            return;
        }
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        _transitioning = true;
        SaveSystem.Inst.Save();
        _previousScene = SceneManager.GetActiveScene();
        SuspendFlorist();
        AsyncOperation load = null;
        try { load = SceneManager.LoadSceneAsync(_mergeScenePath, LoadSceneMode.Additive); }
        catch (Exception exception) { Debug.LogException(exception, this); }
        if (load == null)
        {
            RestoreFlorist();
            _transitioning = false;
            yield break;
        }
        yield return load;
        Scene mergeScene = SceneManager.GetSceneByPath(_mergeScenePath);
        _isOpen = mergeScene.IsValid() && mergeScene.isLoaded;
        if (_isOpen) SceneManager.SetActiveScene(mergeScene);
        else RestoreFlorist();
        _transitioning = false;
    }

    private void CloseMerge()
    {
        if (!_transitioning && _isOpen)
            StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        _transitioning = true;
        if (_previousScene.IsValid() && _previousScene.isLoaded)
            SceneManager.SetActiveScene(_previousScene);
        AsyncOperation unload = SceneManager.UnloadSceneAsync(_mergeScenePath);
        if (unload != null) yield return unload;
        _isOpen = false;
        RestoreFlorist();
        SaveSystem.Inst.Save();
        _transitioning = false;
    }

    private void SuspendFlorist()
    {
        _canvasStates = new bool[_floristCanvases.Length];
        for (int i = 0; i < _floristCanvases.Length; i++)
        {
            if (_floristCanvases[i] == null) continue;
            _canvasStates[i] = _floristCanvases[i].gameObject.activeSelf;
            _floristCanvases[i].gameObject.SetActive(false);
        }
        _objectStates = new bool[_floristInputAndCamera.Length];
        for (int i = 0; i < _floristInputAndCamera.Length; i++)
        {
            if (_floristInputAndCamera[i] == null) continue;
            _objectStates[i] = _floristInputAndCamera[i].activeSelf;
            _floristInputAndCamera[i].SetActive(false);
        }
    }

    private void RestoreFlorist()
    {
        for (int i = 0; i < _floristInputAndCamera.Length; i++)
            if (_floristInputAndCamera[i] != null)
                _floristInputAndCamera[i].SetActive(_objectStates[i]);
        for (int i = 0; i < _floristCanvases.Length; i++)
            if (_floristCanvases[i] != null)
                _floristCanvases[i].gameObject.SetActive(_canvasStates[i]);
    }
}
