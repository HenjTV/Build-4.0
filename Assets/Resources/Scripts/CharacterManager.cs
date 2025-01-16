using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public enum ResourceType
    {
        Energy,
        Mana,
        Rage,
        Focus,
        ColdBlood
    }

    [System.Serializable]
    public class Character
    {
        public int id;                              // id персонажа
        public string name;                        // Название персонажа
        [TextArea] public string description;     // Описание персонажа
        public Sprite avatar;                     // Аватар персонажа (картинка)
        public Sprite superAbilityImage;          // Картинка суперспособности
        [TextArea] public string superAbilityDescription; // Описание суперспособности
        public int attackPower;                   // Сила атаки
        public int defense;                       // Защита
        public int parry;                         // Парирование
        public int kickDamage;                    // Урон пинка
        public int kickCooldown;                 // Пинок - раунды (число)
        public int healPower;                     // Лечение
        public int healCharges;                   // Количество лечилок
        public ResourceType resourceType;         // Ресурс (Энергия, Мана, Ярость, Концентрация, Хладнокровие)
        public int baseHealth;                    // Базовое здоровье
        public int baseResource;                  // Базовое количество ресурса
        public int maxResource;                   // Максимум ресурса
        public int booster;                       // Максимальный множитель характеристик
    }

    [SerializeField]
    public Character[] initialCharacters;
}
