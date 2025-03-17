using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������������ ���� ����� ��������.
/// ���������� �� ������������� (Hunger, Poison, Anxiety � �.�.)
/// </summary>
public enum StatusEffectType
{
    // 28 ��������, ������� �� ���������
    Sated,            // 1
    Fast,             // 2
    Slow,             // 3
    TornWound,        // 4
    Bleeding,         // 5
    Fracture,         // 6
    Panic,            // 7
    Fatigue,          // 8
    Depression,       // 9
    Rage,             // 10
    Madness,          // 11
    Bloodthirst,      // 12
    Anxiety,          // 13
    Poison,           // 14
    AcceleratedLearning,  // 15
    Sadness,          // 16
    Fear,             // 17
    Horror,           // 18
    Hunger,           // 19
    Bite,             // 20
    Fire,             // 21
    Trauma,           // 22
    Lumberjack,       // 23
    CloseCombat,      // 24
    Comfort,          // 25
    MadScientist,     // 26
    Scientist,        // 27
    Faith             // 28
}


/// <summary>
/// �����, ����������� ���� ������:
/// - type: ��� ���
/// - timer: ����� �������� (���� -1 � ����������)
/// </summary>
[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public float timer; // ���� -1 � ���������� ������

    public StatusEffect(StatusEffectType t, float duration = -1)
    {
        type = t;
        timer = duration;
    }
}


/// <summary>
/// ������, ���������� �� ���������� ������-��������� � ������.
/// ��������������, ��� �� ��� �� ������� ����� PlayerController.
/// </summary>

public class PlayerEffects : MonoBehaviour
{
    private PlayerController player;       // ������ �� ������ ������ (����� ���� mood, stamina, kills, etc.)
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // 1) ��������� ������� ���������/�����������
        CheckConditions();
        // 2) ��������� ������� (������)
        ApplyEffects(Time.deltaTime);
    }

    private void CheckConditions()
    {
        // ������: ���� mood < 10 => Depression
        bool needDepression = (player.mood < 10f);
        if (needDepression && !IsEffectActive(StatusEffectType.Depression))
            AddEffect(StatusEffectType.Depression);
        else if (!needDepression && IsEffectActive(StatusEffectType.Depression))
            RemoveEffect(StatusEffectType.Depression);

        // ������: ���� stamina < 20 => Fatigue
        bool needFatigue = (player.stamina < 20f);
        if (needFatigue && !IsEffectActive(StatusEffectType.Fatigue))
            AddEffect(StatusEffectType.Fatigue);
        else if (!needFatigue && IsEffectActive(StatusEffectType.Fatigue))
            RemoveEffect(StatusEffectType.Fatigue);

        // ������: ���� kills>50 => Bloodthirst
        bool needBloodthirst = (player.kills > 50);
        if (needBloodthirst && !IsEffectActive(StatusEffectType.Bloodthirst))
            AddEffect(StatusEffectType.Bloodthirst);
        else if (!needBloodthirst && IsEffectActive(StatusEffectType.Bloodthirst))
            RemoveEffect(StatusEffectType.Bloodthirst);

        // ���������� ������������ ������� ��� ������ (Hunger, Fire, Anxiety � �.�.)
        // Example: if (player.mood<30 && isNight) => Anxiety
        // if (player.health < 50%) => TornWound or Bleeding chance, etc.
    }

    private void ApplyEffects(float deltaTime)
    {
        // ��� � �����, ����� ��������� ������� �������, � ������� ���������� ������
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect eff = activeEffects[i];

            // ���� ������ ����� ������������ �����
            if (eff.timer > 0)
            {
                eff.timer -= deltaTime;
                if (eff.timer <= 0)
                {
                    RemoveEffect(eff.type);
                    continue;
                }
            }

            // ��������� ������ �������
            switch (eff.type)
            {
                case StatusEffectType.Depression:
                    player.mood -= 0.1f * deltaTime;
                    break;

                case StatusEffectType.Fatigue:
                    player.stamina -= 0.5f * deltaTime;
                    break;

                case StatusEffectType.Bloodthirst:
                    // + ����, - ������, etc.
                    break;

                case StatusEffectType.Hunger:
                    // � �������, ������� mood ����������
                    player.mood -= 0.05f * deltaTime;
                    break;

                    // ... �������� ��� ���������
            }
        }
    }

    private bool IsEffectActive(StatusEffectType type)
    {
        return activeEffects.Exists(e => e.type == type);
    }

    public void AddEffect(StatusEffectType type, float duration = -1f)
    {
        if (!IsEffectActive(type))
        {
            activeEffects.Add(new StatusEffect(type, duration));
            Debug.Log("[PlayerEffects] ������ " + type + " ��������!");

            // �������� ������ � UI
            UIStatusEffects.Instance?.AddIcon(type);
        }
    }

    public void RemoveEffect(StatusEffectType type)
    {
        StatusEffect eff = activeEffects.Find(e => e.type == type);
        if (eff != null)
        {
            activeEffects.Remove(eff);
            Debug.Log("[PlayerEffects] ������ " + type + " ����!");

            // ������ ������ � UI
            UIStatusEffects.Instance?.RemoveIcon(type);
        }
    }
}
