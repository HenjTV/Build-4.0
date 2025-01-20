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
        public int AttackPower;                   // Сила атаки
        public int DefencePower;                       // Защита
        public int ParryPower;                         // Парирование
        public int KickPower;                    // Урон пинка
        public int BreakPower;                 // Пинок - раунды (число)
        public int HealPower;                     // Лечение
        public int Poisons;                   // Количество лечилок
        public ResourceType resourceType;         // Ресурс (Энергия, Мана, Ярость, Концентрация, Хладнокровие)
        public int baseHealth;                    // Базовое здоровье
        public int baseResource;                  // Базовое количество ресурса
        public int maxResource;                   // Максимум ресурса
        public int maxPowerBar;                       // Максимальный множитель характеристик
    }

    [SerializeField]
    public Character[] initialCharacters;
}
