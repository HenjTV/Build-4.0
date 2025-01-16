using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ImageScroller : MonoBehaviour
{
    public string folderPath = "Assets/Resources/Images/Avatars";  // ���� � ����� � �������������
    public Image displayImage;                   // ������ �� UI ������� Image ��� ����������� �����������
    private string[] imagePaths;                 // ������ ����� � ������������
    private int currentImageIndex = 0;           // ������ �������� �����������
    public float fadeDuration = 1f;              // ������������ ����� (�������� ��������)

    void Start()
    {
        // ��������� ��� ����������� �� ��������� ����� � ������������ .png � .jpg
        string[] pngFiles = Directory.GetFiles(folderPath, "*.png");
        string[] jpgFiles = Directory.GetFiles(folderPath, "*.jpg");

        // ���������� ��� �������
        imagePaths = new string[pngFiles.Length + jpgFiles.Length];
        pngFiles.CopyTo(imagePaths, 0);
        jpgFiles.CopyTo(imagePaths, pngFiles.Length);

        if (imagePaths.Length == 0)
        {
            Debug.LogError("��� ����������� � �����!");
            return;
        }

        Debug.Log("����������� ���������: " + imagePaths.Length);

        // ������������ ������ �����������
        ShuffleArray(imagePaths);

        // ��������� ������ ����������� ��� �����
        Texture2D texture = LoadTexture(imagePaths[currentImageIndex]);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        if (displayImage != null)
        {
            displayImage.sprite = sprite;
            displayImage.color = new Color(displayImage.color.r, displayImage.color.g, displayImage.color.b, 1f); // ������� �����
        }
        else
        {
            Debug.LogError("DisplayImage �� �������� � ����������!");
        }

        // ��������� � ���������� �����������
        currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;

        // ��������� �������� ��� ������������ ������������� �����������
        StartCoroutine(ScrollImages());
    }

    IEnumerator ScrollImages()
    {
        while (true)
        {
            if (imagePaths.Length > 0)
            {
                // ��������� ����� �����������
                Texture2D texture = LoadTexture(imagePaths[currentImageIndex]);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                // ��������� ������� ������� (����)
                if (displayImage != null)
                {
                    // �������� ����-������ ��� ����� �����������
                    yield return StartCoroutine(FadeOutIn(sprite));
                    Debug.Log("����������� ���������: " + imagePaths[currentImageIndex]);
                }
                else
                {
                    Debug.LogError("DisplayImage �� �������� � ����������!");
                }

                // ��������� � ���������� �����������
                currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;

                // �������� ����� ��������� ������������ (��������, 2 �������)
                yield return new WaitForSeconds(2f);
            }
            else
            {
                Debug.LogError("����������� �� �������!");
                yield break;
            }
        }
    }

    // ����� ��� �������� ����������� �� ����
    Texture2D LoadTexture(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);  // ������� ��������� ������ Texture2D
        texture.LoadImage(fileData);  // ��������� ������ �����������
        return texture;
    }

    // ������� ��� �������� �������� ����� �������������
    IEnumerator FadeOutIn(Sprite newSprite)
    {
        // ����-����: ������� ������������ �������� �����������
        float elapsedTime = 0f;
        Color currentColor = displayImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            displayImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }

        // ������������� ����� �����������
        displayImage.sprite = newSprite;

        // ����-��: ������� ��������� ������ �����������
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            displayImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }
    }

    // ����� ��� ������������� �������
    void ShuffleArray(string[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length);
            string temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}
