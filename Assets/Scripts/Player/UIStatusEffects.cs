using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ��������� ������������ ������ ������-�������� � UI.
/// ��������������, ��� �� �������� ���� ������ �� ������ UI (��������, ������ �� Canvas).
/// </summary>
public class UIStatusEffects : MonoBehaviour
{
    public static UIStatusEffects Instance;

    [Header("�������� (Layout) ��� ������")]
    [SerializeField] private Transform iconsParent;

    [Header("������ ������ (Image)")]
    [SerializeField] private GameObject iconPrefab;

    [Header("������� ��� ������� (28 ����)")]
    // ��� �� ������� ���� ��� ������� �������
    // (���� ������� Dictionary<StatusEffectType, Sprite> � ����)
    public Sprite satedSprite;
    public Sprite fastSprite;
    public Sprite slowSprite;
    public Sprite tornWoundSprite;
    public Sprite bleedingSprite;
    public Sprite fractureSprite;
    public Sprite panicSprite;
    public Sprite fatigueSprite;
    public Sprite depressionSprite;
    public Sprite rageSprite;
    public Sprite madnessSprite;
    public Sprite bloodthirstSprite;
    public Sprite anxietySprite;
    public Sprite poisonSprite;
    public Sprite acceleratedLearningSprite;
    public Sprite sadnessSprite;
    public Sprite fearSprite;
    public Sprite horrorSprite;
    public Sprite hungerSprite;
    public Sprite biteSprite;
    public Sprite fireSprite;
    public Sprite traumaSprite;
    public Sprite lumberjackSprite;
    public Sprite closeCombatSprite;
    public Sprite comfortSprite;
    public Sprite madScientistSprite;
    public Sprite scientistSprite;
    public Sprite faithSprite;

    // �������� ������: ���� - ��� �������, �������� - ��������� ������
    private Dictionary<StatusEffectType, GameObject> activeIcons = new Dictionary<StatusEffectType, GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// ��������� ������ ��� ��������� �������
    /// </summary>
    public void AddIcon(StatusEffectType type)
    {
        if (activeIcons.ContainsKey(type))
            return; // ��� ���� ������

        if (iconPrefab == null || iconsParent == null)
        {
            Debug.LogError("[UIStatusEffects] iconPrefab ��� iconsParent �� ��������!");
            return;
        }

        GameObject newIcon = Instantiate(iconPrefab, iconsParent);
        Image iconImage = newIcon.GetComponent<Image>();
        if (iconImage != null)
        {
            Sprite sprite = GetSpriteForEffect(type);
            if (sprite != null)
                iconImage.sprite = sprite;
        }

        activeIcons[type] = newIcon;
    }

    /// <summary>
    /// ������� ������ ��� ������ �������
    /// </summary>
    public void RemoveIcon(StatusEffectType type)
    {
        if (activeIcons.TryGetValue(type, out GameObject iconObj))
        {
            Destroy(iconObj);
            activeIcons.Remove(type);
        }
    }

    /// <summary>
    /// ���������� ������ ������ ��� ������� �� 28 ��������
    /// </summary>
    private Sprite GetSpriteForEffect(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Sated: return satedSprite;
            case StatusEffectType.Fast: return fastSprite;
            case StatusEffectType.Slow: return slowSprite;
            case StatusEffectType.TornWound: return tornWoundSprite;
            case StatusEffectType.Bleeding: return bleedingSprite;
            case StatusEffectType.Fracture: return fractureSprite;
            case StatusEffectType.Panic: return panicSprite;
            case StatusEffectType.Fatigue: return fatigueSprite;
            case StatusEffectType.Depression: return depressionSprite;
            case StatusEffectType.Rage: return rageSprite;
            case StatusEffectType.Madness: return madnessSprite;
            case StatusEffectType.Bloodthirst: return bloodthirstSprite;
            case StatusEffectType.Anxiety: return anxietySprite;
            case StatusEffectType.Poison: return poisonSprite;
            case StatusEffectType.AcceleratedLearning: return acceleratedLearningSprite;
            case StatusEffectType.Sadness: return sadnessSprite;
            case StatusEffectType.Fear: return fearSprite;
            case StatusEffectType.Horror: return horrorSprite;
            case StatusEffectType.Hunger: return hungerSprite;
            case StatusEffectType.Bite: return biteSprite;
            case StatusEffectType.Fire: return fireSprite;
            case StatusEffectType.Trauma: return traumaSprite;
            case StatusEffectType.Lumberjack: return lumberjackSprite;
            case StatusEffectType.CloseCombat: return closeCombatSprite;
            case StatusEffectType.Comfort: return comfortSprite;
            case StatusEffectType.MadScientist: return madScientistSprite;
            case StatusEffectType.Scientist: return scientistSprite;
            case StatusEffectType.Faith: return faithSprite;
        }
        return null; // ���� ����� ��� �������
    }
}
