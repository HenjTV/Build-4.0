using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Slider))]
public class SliderAnimationAndSound : MonoBehaviour
{
    public AudioSource audioSource; // ������ �� AudioSource, ������� ����� �������������� ����
    public AudioClip slideSound;    // ���� ��� ���������������

    private Slider slider;
    private int previousValue;
    private bool isSnapping = false;
    private bool isPlayingSound = false;
    private float lastValue; // ��� ������������ ���������� ��������
    private Queue<AudioClip> soundQueue = new Queue<AudioClip>(); // ������� ������
    private int maxQueueSize; // ������������ ������ �������

    void Start()
    {
        slider = GetComponent<Slider>();

        // ��������, ��� ������� �������� �� ������������� ��������
        slider.wholeNumbers = true;

        // ��������� ��������� ��������
        previousValue = Mathf.RoundToInt(slider.value);
        lastValue = slider.value;

        // ������������ ������ ������� � ��� ������ ��������
        maxQueueSize = Mathf.RoundToInt(slider.maxValue);
    }

    void Update()
    {
        // ��������� ������ ���������� � ������������� ���������
        if (!isSnapping)
        {
            float roundedValue = Mathf.Round(slider.value);

            // ������������� ���������� �������� � ���������� ������
            isSnapping = true;
            slider.value = roundedValue;
            isSnapping = false;

            // ���� �������� ����������, ��������� ������� ������
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
        // ��������� ������� �������� �������� �� ���������� ������
        int currentValue = Mathf.RoundToInt(slider.value);

        // ���������, ���������� �� �������� �� ����� �����, � �� ��������� �� ��� � ����������
        if (currentValue != previousValue)
        {
            // ��������� ���������� ��������
            previousValue = currentValue;
        }

        // ������������� ���������� Handle � ���������� ������
        slider.value = currentValue;
    }

    private void AddSoundToQueue()
    {
        // ��������� ���� � �������
        soundQueue.Enqueue(slideSound);

        // ���� ������� ��������� ������������ ������, ������� ������ �����
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

        // ������������� ������ ���� �� �������
        while (soundQueue.Count > 0)
        {
            AudioClip clip = soundQueue.Dequeue();
            audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }

        isPlayingSound = false;
    }
}
