using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

public class DecorationManager : MonoBehaviour
{
    public enum DecorationType
    {
        None,
        Furniture,
        WallDecor,
        FloorDecor
    }

    [SerializeField] private GameObject _decorarionButtonsParent;
    [SerializeField] private GameObject _decoCanvas;
    [SerializeField] private DecorationScroll _decorationScrollPrefab;
    [SerializeField] private DecorationItem _decorationItemPrefab;
    [SerializeField] private Transform _decorationScrollParent;
    [SerializeField] private Transform _mainCanvas;
    [SerializeField] private Transform _mainCanvasOriginalPlaceholder;
    [SerializeField] private Transform[] _decoCanvasPlaceholders;
    [SerializeField] private Transform[] _decoScrollPlaceholders;
    [SerializeField] private GameObject[] _decorationButtons;
    [SerializeField] private SerializedDictionary<DecorationType, List<Sprite>> _decorationSprites;
    [SerializeField] private SerializedDictionary<DecorationType, List<Image>> _decorationImages;
    private DecorationType _currentDecorationType;
    private List<DecorationScroll> _decorationScrolls = new();

    public void OpenDecorationPage()
    {
        Debug.Log("Decoration page opened.");
        Init();
        _decoCanvas.SetActive(true);
        _decorarionButtonsParent.SetActive(true);
    }

    public void OnCloseButtonClicked()
    {
        // Logic to close the decoration page
        Debug.Log("Decoration page closed.");
        _decoCanvas.SetActive(false);
        _decorarionButtonsParent.SetActive(false);
    }

    public void OnDecorationTypeChangeButtonClicked(int id)
    {
        Debug.Log($"Decoration button {id} clicked.");

        if (id >= _decoCanvasPlaceholders.Length)
        {
            return;
        }

        _currentDecorationType = (DecorationType)id;

        for (int i = 0; i < _decorationButtons.Length; i++)
        {
            if (i == id)
            {
                _decorationScrolls[i].Open();
                _decorationScrolls[i].gameObject.SetActive(true);
            }
            else
            {
                _decorationScrolls[i].Close();
                _decorationScrolls[i].gameObject.SetActive(false);
            }
        }

        Transform placeholder = _decoCanvasPlaceholders[id];
        _mainCanvas.SetPositionAndRotation(placeholder.position, placeholder.rotation);
    }

    public void OnDecorationCloseClicked()
    {
        _mainCanvas.SetPositionAndRotation(_mainCanvasOriginalPlaceholder.position, _mainCanvasOriginalPlaceholder.rotation);
        for (int i = 0; i < _decorationButtons.Length; i++)
        {
            _decorationButtons[i].SetActive(true);
        }
    }

    public void OnDecorationButtonClicked(int id)
    {
        _decorationImages[_currentDecorationType][id].sprite = _decorationSprites[_currentDecorationType][id];
    }

    private void Init()
    {
        if (_decorationScrolls.Count > 0)
        {
            foreach (var scroll in _decorationScrolls)
            {
                Destroy(scroll.gameObject);
            }
            _decorationScrolls.Clear();
        }

        for (int i = 0; i < _decoCanvasPlaceholders.Length; i++)
        {
            var scroll = Instantiate(_decorationScrollPrefab, _decorationScrollParent);
            scroll.transform.SetPositionAndRotation(_decoScrollPlaceholders[i].position, _decoScrollPlaceholders[i].rotation);
            scroll.Init(Configs.ShopConfig.GetItems(ItemType.Decor, (DecorationType)i), _decorationItemPrefab, (DecorationType)i);
            scroll.gameObject.SetActive(false);
            _decorationScrolls.Add(scroll);
        }
    }
}
