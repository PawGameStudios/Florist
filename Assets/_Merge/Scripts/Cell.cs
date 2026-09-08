namespace Florist.Merge
{
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Cell : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image m_ItemImage;
    [SerializeField] private Image m_EnergyIcon;
    [SerializeField] private Image m_CellImage;
    [Header("Producer item flight")]
    [SerializeField] private Image m_SpawnVisualPrefab;
    [SerializeField, Min(.01f)] private float m_SpawnDuration = .35f;
    [SerializeField] private Ease m_SpawnEase = Ease.OutSine;
    [SerializeField, Min(0f)] private float m_SpawnArcHeight = 24f;
    [SerializeField, Range(.1f, 1f)] private float m_SpawnStartScale = .8f;
    private Image m_SpawnVisual;
    [SerializeField, Min(.1f)] private float m_DoubleClickInterval = .3f;
    private float m_LastCollectClick = float.NegativeInfinity;
    private GameManager m_GameManager;
    public RectTransform RectTransform => m_CellImage.rectTransform;
    private int m_X, m_Y;
    private ItemData m_ItemData;
    private int m_Level;
    private bool m_IsProducer = false;
    private bool m_IsFixed = false;
    public bool IsEmpty => m_ItemData == null;
    private Tween m_Motion;
    private Tween m_Highlight;
    private Vector3 m_ItemScale;
    private Vector2 m_ItemPosition;
    private bool m_Pressed;
    public Vector3 ItemWorldPosition => m_ItemImage.rectTransform.position;
    public Sprite ItemSprite => m_ItemImage.sprite;

    private void Awake()
    {
        m_ItemScale = m_ItemImage.rectTransform.localScale;
        m_ItemPosition = m_ItemImage.rectTransform.anchoredPosition;
    }

    public void ResetMotion()
    {
        m_Motion?.Kill();
        m_Motion = null;
        if (m_SpawnVisual != null)
        {
            Destroy(m_SpawnVisual.gameObject);
            m_SpawnVisual = null;
        }
        m_Pressed = false;
        m_ItemImage.rectTransform.localScale = m_ItemScale;
        m_ItemImage.rectTransform.anchoredPosition = m_ItemPosition;
        SetItemImageAlpha(1f);
        if (!m_IsFixed) m_ItemImage.color = Color.white;
    }

    public void PlaySpawn(Vector3 origin, RectTransform flightRoot)
    {
        ResetMotion();
        if (m_SpawnVisualPrefab == null || flightRoot == null)
        {
            PlayArrival(origin);
            return;
        }
        // Render the flight above the grid, so neighboring cell backgrounds cannot hide it.
        m_SpawnVisual = Instantiate(m_SpawnVisualPrefab, flightRoot);
        m_SpawnVisual.sprite = m_ItemImage.sprite;
        var visual = m_SpawnVisual.rectTransform;
        visual.sizeDelta = RectTransform.rect.size;
        visual.position = m_ItemImage.rectTransform.position;
        Vector2 destination = visual.anchoredPosition;
        visual.position = origin;
        Vector2 start = visual.anchoredPosition;
        visual.localScale = Vector3.one * m_SpawnStartScale;
        SetItemImageAlpha(0f);
        float duration = Mathf.Max(.01f, m_SpawnDuration);
        var sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(DOTween.To(() => 0f, progress =>
        {
            visual.anchoredPosition = Vector2.LerpUnclamped(start, destination, progress) +
                Vector2.up * (Mathf.Sin(Mathf.PI * progress) * m_SpawnArcHeight);
        }, 1f, duration).SetEase(m_SpawnEase));
        sequence.Join(visual.DOScale(Vector3.one, duration).SetEase(m_SpawnEase));
        sequence.OnComplete(ResetMotion);
        m_Motion = sequence;
    }

    public void PlayArrival(Vector3 origin, bool merged = false, bool unlocked = false)
    {
        ResetMotion();
        var visual = m_ItemImage.rectTransform;
        visual.position = origin;
        visual.localScale = m_ItemScale * (merged ? .90f : 1f);
        var sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(visual.DOAnchorPos(m_ItemPosition, merged ? .08f : .18f).SetEase(Ease.OutQuad));
        sequence.Join(visual.DOScale(m_ItemScale * (merged ? 1.08f : 1f), merged ? .12f : .18f).SetEase(Ease.OutQuad));
        if (merged) sequence.Append(visual.DOScale(m_ItemScale, .10f).SetEase(Ease.OutSine));
        if (unlocked)
        {
            m_ItemImage.color = new Color(.72f, .85f, .75f, 1f);
            sequence.Join(m_ItemImage.DOColor(Color.white, .20f));
        }
        m_Motion = sequence;
    }

    public void SetMergeHighlight(bool highlighted)
    {
        m_Highlight?.Kill();
        Color highlight = m_GameManager != null ? m_GameManager.MergeTargetCellColor : Color.clear;
        if (!highlighted || highlight.a <= 0f)
        {
            if (!IsEmpty && !IsProducer() && !IsFixed()) UpdateCellColor();
            else ResetCellColor();
            return;
        }
        m_Highlight = m_CellImage.DOColor(highlight, .08f).SetUpdate(true);
    }

    public void PlayRejected()
    {
        ResetMotion();
        m_Motion = m_ItemImage.rectTransform.DOPunchAnchorPos(new Vector2(4f, 0f), .16f, 2, .1f).SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsProducer() || eventData.button != PointerEventData.InputButton.Left ||
            (m_GameManager != null && m_GameManager.IsDragging)) return;
        ResetMotion();
        m_Pressed = true;
        m_Motion = m_ItemImage.rectTransform.DOScale(m_ItemScale * .94f, .06f).SetUpdate(true);
    }

    private void ReleasePress()
    {
        if (!m_Pressed) return;
        m_Pressed = false;
        m_Motion?.Kill();
        m_Motion = m_ItemImage.rectTransform.DOScale(m_ItemScale, .10f).SetEase(Ease.OutSine).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData) { ReleasePress(); }
    public void OnPointerEnter(PointerEventData eventData) { if (m_GameManager != null) m_GameManager.OnCellHover(this, eventData); }
    public void OnPointerExit(PointerEventData eventData)
    {
        ReleasePress();
        if (m_GameManager != null) m_GameManager.OnCellHoverExit(this, eventData);
    }

    public void Initialize(int _x, int _y, GameManager owner)
    {
        m_GameManager = owner;
        m_X = _x;
        m_Y = _y;
        Clear();
    }

    public void SetProducer(ItemData _itemData)
    {
        m_LastCollectClick = float.NegativeInfinity;
        ResetMotion();
        m_ItemData = _itemData;
        m_Level = 0;
        m_IsProducer = true;
        m_IsFixed = false;
        m_ItemImage.sprite = _itemData.ProducerSprite;
        m_ItemImage.enabled = true;
        m_ItemImage.color = Color.white;

        ResetCellColor();

        ShowEnergyIcon();
    }

    public bool IsProducer() => m_IsProducer;

    public void SetItem(ItemData _itemData, int _level)
    {
        m_LastCollectClick = float.NegativeInfinity;
        ResetMotion();
        m_ItemData = _itemData;
        m_Level = _level;
        m_IsProducer = false;
        m_IsFixed = false;
        m_ItemImage.sprite = _itemData.Sprites[_level - 1];
        m_ItemImage.enabled = true;
        m_ItemImage.color = Color.white;

        UpdateCellColor();

        HideEnergyIcon();
    }

    public void SetFixedItem(ItemData _itemData, int _level)
    {
        m_LastCollectClick = float.NegativeInfinity;
        ResetMotion();
        m_ItemData = _itemData;
        m_Level = _level;
        m_IsProducer = false;
        m_IsFixed = true;


        if (_itemData.FixedSprites != null && _level <= _itemData.FixedSprites.Length && _itemData.FixedSprites[_level - 1] != null)
        {
            m_ItemImage.sprite = _itemData.FixedSprites[_level - 1];
            m_ItemImage.color = Color.white;
        }
        else
        {

            m_ItemImage.sprite = _itemData.Sprites[_level - 1];
            m_ItemImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        }

        m_ItemImage.enabled = true;

        ResetCellColor();

        HideEnergyIcon();
    }

    public void Clear()
    {
        m_LastCollectClick = float.NegativeInfinity;
        ResetMotion();
        m_ItemData = null;
        m_Level = 0;
        m_IsProducer = false;
        m_IsFixed = false;
        m_ItemImage.sprite = null;
        m_ItemImage.enabled = false;
        m_ItemImage.color = Color.white;

        ResetCellColor();

        HideEnergyIcon();
    }

    public ItemData GetItemData() => m_ItemData;
    public int GetLevel() => m_Level;
    public (int x, int y) GetPosition() => (m_X, m_Y);
    public bool IsFixed() => m_IsFixed;

    public void SetItemImageAlpha(float alpha)
    {
        if (m_ItemImage != null)
        {
            Color c = m_ItemImage.color;
            c.a = alpha;
            m_ItemImage.color = c;
        }
    }

    private void UpdateCellColor()
    {
        m_Highlight?.Kill();
        if (m_CellImage == null) return;
        bool maxLevel = !m_IsProducer && !m_IsFixed && m_ItemData != null &&
            m_ItemData.Sprites != null && m_Level > 0 && m_Level == m_ItemData.Sprites.Length;
        m_CellImage.color = maxLevel && m_GameManager != null ? m_GameManager.MaxLevelCellColor : Color.clear;
    }

    private void ResetCellColor()
    {
        m_Highlight?.Kill();
        if (m_CellImage != null)
        {
            m_CellImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    private void ShowEnergyIcon()
    {
        if (m_EnergyIcon != null)
        {
            m_EnergyIcon.enabled = true;
        }
    }

    private void HideEnergyIcon()
    {
        if (m_EnergyIcon != null)
        {
            m_EnergyIcon.enabled = false;
        }
    }

    public bool IsCollectable()
    {
        return m_ItemData != null && !m_IsProducer && !m_IsFixed && m_ItemData.Sprites != null && m_Level >= 1 && m_Level <= m_ItemData.Sprites.Length;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging || m_GameManager == null || m_GameManager.IsDragging ||
            eventData.button != PointerEventData.InputButton.Left) return;
        if (IsProducer())
            m_GameManager.OnProducerClicked(this);
        else if (IsCollectable())
        {
            float now = Time.unscaledTime;
            if (now - m_LastCollectClick <= m_DoubleClickInterval)
            {
                m_LastCollectClick = float.NegativeInfinity;
                m_GameManager.OnCollectClicked(this);
            }
            else m_LastCollectClick = now;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_LastCollectClick = float.NegativeInfinity;
        if (m_GameManager != null && eventData.button == PointerEventData.InputButton.Left && !IsEmpty && !m_IsFixed)
            m_GameManager.OnCellBeginDrag(this, eventData);
    }

    private void OnDisable()
    {
        m_LastCollectClick = float.NegativeInfinity;
        if (m_GameManager != null) m_GameManager.CancelDrag(this);
        ResetMotion();
        SetMergeHighlight(false);
    }

    public void OnDrop(PointerEventData eventData) { if (m_GameManager != null) m_GameManager.OnCellDrop(this, eventData); }
    public void OnDrag(PointerEventData eventData) { if (m_GameManager != null) m_GameManager.OnCellDrag(eventData); }
    public void OnEndDrag(PointerEventData eventData) { if (m_GameManager != null) m_GameManager.OnCellEndDrag(this, eventData); }
}
}
