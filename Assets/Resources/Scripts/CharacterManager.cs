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
        public int id;                              // id ���������
        public string name;                        // �������� ���������
        [TextArea] public string description;     // �������� ���������
        public Sprite avatar;                     // ������ ��������� (��������)
        public Sprite superAbilityImage;          // �������� ����������������
        [TextArea] public string superAbilityDescription; // �������� ����������������
        public int AttackPower;                   // ���� �����
        public int DefencePower;                       // ������
        public int ParryPower;                         // �����������
        public int KickPower;                    // ���� �����
        public int BreakPower;                 // ����� - ������ (�����)
        public int HealPower;                     // �������
        public int Poisons;                   // ���������� �������
        public ResourceType resourceType;         // ������ (�������, ����, ������, ������������, ������������)
        public int baseHealth;                    // ������� ��������
        public int baseResource;                  // ������� ���������� �������
        public int maxResource;                   // �������� �������
        public int maxPowerBar;                       // ������������ ��������� �������������
    }

    [SerializeField]
    public Character[] initialCharacters;
}
