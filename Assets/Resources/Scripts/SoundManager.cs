using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    public AudioSource buttonClickSound;

    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(PlayClickSound);  // ��� ������� ������ ��� �������
    }

    void PlayClickSound()
    {
        buttonClickSound.Play();  // ���� ��� ������� �� ������
    }
}