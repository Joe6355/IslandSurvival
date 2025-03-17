using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Перечисление всех типов эффектов.
/// Дополняйте по необходимости (Hunger, Poison, Anxiety и т.д.)
/// </summary>
public enum StatusEffectType
{
    // 28 эффектов, которые мы обсуждали
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
/// Класс, описывающий один эффект:
/// - type: его тип
/// - timer: время действия (если -1 – бессрочный)
/// </summary>
[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public float timer; // если -1 — бессрочный эффект

    public StatusEffect(StatusEffectType t, float duration = -1)
    {
        type = t;
        timer = duration;
    }
}


/// <summary>
/// Скрипт, отвечающий за управление статус-эффектами у игрока.
/// Предполагается, что на том же объекте висит PlayerController.
/// </summary>

public class PlayerEffects : MonoBehaviour
{
    private PlayerController player;       // Ссылка на скрипт игрока (нужны поля mood, stamina, kills, etc.)
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // 1) Проверяем условия активации/деактивации
        CheckConditions();
        // 2) Применяем эффекты (логика)
        ApplyEffects(Time.deltaTime);
    }

    private void CheckConditions()
    {
        // Пример: Если mood < 10 => Depression
        bool needDepression = (player.mood < 10f);
        if (needDepression && !IsEffectActive(StatusEffectType.Depression))
            AddEffect(StatusEffectType.Depression);
        else if (!needDepression && IsEffectActive(StatusEffectType.Depression))
            RemoveEffect(StatusEffectType.Depression);

        // Пример: Если stamina < 20 => Fatigue
        bool needFatigue = (player.stamina < 20f);
        if (needFatigue && !IsEffectActive(StatusEffectType.Fatigue))
            AddEffect(StatusEffectType.Fatigue);
        else if (!needFatigue && IsEffectActive(StatusEffectType.Fatigue))
            RemoveEffect(StatusEffectType.Fatigue);

        // Пример: Если kills>50 => Bloodthirst
        bool needBloodthirst = (player.kills > 50);
        if (needBloodthirst && !IsEffectActive(StatusEffectType.Bloodthirst))
            AddEffect(StatusEffectType.Bloodthirst);
        else if (!needBloodthirst && IsEffectActive(StatusEffectType.Bloodthirst))
            RemoveEffect(StatusEffectType.Bloodthirst);

        // Аналогично прописываете условия для других (Hunger, Fire, Anxiety и т.д.)
        // Example: if (player.mood<30 && isNight) => Anxiety
        // if (player.health < 50%) => TornWound or Bleeding chance, etc.
    }

    private void ApplyEffects(float deltaTime)
    {
        // Идём с конца, чтобы безопасно убирать эффекты, у которых закончился таймер
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect eff = activeEffects[i];

            // Если эффект имеет ограниченное время
            if (eff.timer > 0)
            {
                eff.timer -= deltaTime;
                if (eff.timer <= 0)
                {
                    RemoveEffect(eff.type);
                    continue;
                }
            }

            // Применяем логику эффекта
            switch (eff.type)
            {
                case StatusEffectType.Depression:
                    player.mood -= 0.1f * deltaTime;
                    break;

                case StatusEffectType.Fatigue:
                    player.stamina -= 0.5f * deltaTime;
                    break;

                case StatusEffectType.Bloodthirst:
                    // + урон, - защита, etc.
                    break;

                case StatusEffectType.Hunger:
                    // к примеру, снижает mood потихоньку
                    player.mood -= 0.05f * deltaTime;
                    break;

                    // ... допишите для остальных
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
            Debug.Log("[PlayerEffects] Эффект " + type + " добавлен!");

            // Показать иконку в UI
            UIStatusEffects.Instance?.AddIcon(type);
        }
    }

    public void RemoveEffect(StatusEffectType type)
    {
        StatusEffect eff = activeEffects.Find(e => e.type == type);
        if (eff != null)
        {
            activeEffects.Remove(eff);
            Debug.Log("[PlayerEffects] Эффект " + type + " снят!");

            // Убрать иконку в UI
            UIStatusEffects.Instance?.RemoveIcon(type);
        }
    }
}
