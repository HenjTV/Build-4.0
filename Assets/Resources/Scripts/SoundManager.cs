using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    public AudioSource buttonClickSound;

    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(PlayClickSound);  // Это событие только при нажатии
    }

    void PlayClickSound()
    {
        buttonClickSound.Play();  // Звук при нажатии на кнопку
    }
}