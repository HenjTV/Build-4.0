using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ImageScroller : MonoBehaviour
{
    public string folderPath = "Assets/Resources/Images/Avatars";  // Путь к папке с изображениями
    public Image displayImage;                   // Ссылка на UI элемент Image для отображения изображения
    private string[] imagePaths;                 // Массив путей к изображениям
    private int currentImageIndex = 0;           // Индекс текущего изображения
    public float fadeDuration = 1f;              // Длительность фейда (плавного перехода)

    void Start()
    {
        // Загружаем все изображения из указанной папки с расширениями .png и .jpg
        string[] pngFiles = Directory.GetFiles(folderPath, "*.png");
        string[] jpgFiles = Directory.GetFiles(folderPath, "*.jpg");

        // Объединяем два массива
        imagePaths = new string[pngFiles.Length + jpgFiles.Length];
        pngFiles.CopyTo(imagePaths, 0);
        jpgFiles.CopyTo(imagePaths, pngFiles.Length);

        if (imagePaths.Length == 0)
        {
            Debug.LogError("Нет изображений в папке!");
            return;
        }

        Debug.Log("Изображения загружены: " + imagePaths.Length);

        // Перемешиваем массив изображений
        ShuffleArray(imagePaths);

        // Загружаем первое изображение без фейда
        Texture2D texture = LoadTexture(imagePaths[currentImageIndex]);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        if (displayImage != null)
        {
            displayImage.sprite = sprite;
            displayImage.color = new Color(displayImage.color.r, displayImage.color.g, displayImage.color.b, 1f); // Убираем альфа
        }
        else
        {
            Debug.LogError("DisplayImage не привязан в инспекторе!");
        }

        // Переходим к следующему изображению
        currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;

        // Запускаем корутину для бесконечного прокручивания изображений
        StartCoroutine(ScrollImages());
    }

    IEnumerator ScrollImages()
    {
        while (true)
        {
            if (imagePaths.Length > 0)
            {
                // Загружаем новое изображение
                Texture2D texture = LoadTexture(imagePaths[currentImageIndex]);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                // Выполняем плавный переход (фейд)
                if (displayImage != null)
                {
                    // Начинаем фейд-эффект для смены изображения
                    yield return StartCoroutine(FadeOutIn(sprite));
                    Debug.Log("Изображение обновлено: " + imagePaths[currentImageIndex]);
                }
                else
                {
                    Debug.LogError("DisplayImage не привязан в инспекторе!");
                }

                // Переходим к следующему изображению
                currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;

                // Задержка перед следующим изображением (например, 2 секунды)
                yield return new WaitForSeconds(2f);
            }
            else
            {
                Debug.LogError("Изображения не найдены!");
                yield break;
            }
        }
    }

    // Метод для загрузки изображения из пути
    Texture2D LoadTexture(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);  // Создаем временный объект Texture2D
        texture.LoadImage(fileData);  // Загружаем данные изображения
        return texture;
    }

    // Корутин для плавного перехода между изображениями
    IEnumerator FadeOutIn(Sprite newSprite)
    {
        // Фейд-аута: плавное исчезновение текущего изображения
        float elapsedTime = 0f;
        Color currentColor = displayImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            displayImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }

        // Устанавливаем новое изображение
        displayImage.sprite = newSprite;

        // Фейд-ин: плавное появление нового изображения
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            displayImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }
    }

    // Метод для перемешивания массива
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
