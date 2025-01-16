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
        public int attackPower;                   // ���� �����
        public int defense;                       // ������
        public int parry;                         // �����������
        public int kickDamage;                    // ���� �����
        public int kickCooldown;                 // ����� - ������ (�����)
        public int healPower;                     // �������
        public int healCharges;                   // ���������� �������
        public ResourceType resourceType;         // ������ (�������, ����, ������, ������������, ������������)
        public int baseHealth;                    // ������� ��������
        public int baseResource;                  // ������� ���������� �������
        public int maxResource;                   // �������� �������
        public int booster;                       // ������������ ��������� �������������
    }

    [SerializeField]
    public Character[] initialCharacters;
}
