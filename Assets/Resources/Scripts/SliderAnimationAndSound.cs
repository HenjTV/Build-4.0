using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Slider))]
public class SliderAnimationAndSound : MonoBehaviour
{
    public AudioSource audioSource; // Ссылка на AudioSource, который будет воспроизводить звук
    public AudioClip slideSound;    // Звук для воспроизведения

    private Slider slider;
    private int previousValue;
    private bool isSnapping = false;
    private bool isPlayingSound = false;
    private float lastValue; // Для отслеживания последнего значения
    private Queue<AudioClip> soundQueue = new Queue<AudioClip>(); // Очередь звуков
    private int maxQueueSize; // Максимальный размер очереди

    void Start()
    {
        slider = GetComponent<Slider>();

        // Убедимся, что слайдер настроен на целочисленные значения
        slider.wholeNumbers = true;

        // Установим начальное значение
        previousValue = Mathf.RoundToInt(slider.value);
        lastValue = slider.value;

        // Максимальный размер очереди — это размер слайдера
        maxQueueSize = Mathf.RoundToInt(slider.maxValue);
    }

    void Update()
    {
        // Реализуем эффект прилипания к целочисленным значениям
        if (!isSnapping)
        {
            float roundedValue = Mathf.Round(slider.value);

            // Принудительно выставляем значение к ближайшему целому
            isSnapping = true;
            slider.value = roundedValue;
            isSnapping = false;

            // Если значение изменилось, обновляем очередь звуков
            if (slider.value != lastValue)
            {
                AddSoundToQueue();
                if (!isPlayingSound)
                {
                    PlaySound();
                }
            }
            lastValue = slider.value;
        }
    }

    public void OnSliderValueChanged()
    {
        // Округляем текущее значение слайдера до ближайшего целого
        int currentValue = Mathf.RoundToInt(slider.value);

        // Проверяем, изменилось ли значение на целое число, и не совпадает ли оно с предыдущим
        if (currentValue != previousValue)
        {
            // Обновляем предыдущее значение
            previousValue = currentValue;
        }

        // Принудительно перемещаем Handle к ближайшему целому
        slider.value = currentValue;
    }

    private void AddSoundToQueue()
    {
        // Добавляем звук в очередь
        soundQueue.Enqueue(slideSound);

        // Если очередь превышает максимальный размер, удаляем старые звуки
        if (soundQueue.Count > maxQueueSize)
        {
            soundQueue.Dequeue();
        }
    }

    private void PlaySound()
    {
        if (audioSource != null && soundQueue.Count > 0)
        {
            StartCoroutine(PlaySoundSequentially());
        }
    }

    private System.Collections.IEnumerator PlaySoundSequentially()
    {
        isPlayingSound = true;

        // Воспроизводим каждый звук из очереди
        while (soundQueue.Count > 0)
        {
            AudioClip clip = soundQueue.Dequeue();
            audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }

        isPlayingSound = false;
    }
}
