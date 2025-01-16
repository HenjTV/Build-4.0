using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public Button findLobbyButton;
    public Button exitGameButton;
    public TMP_Text statusText;
    public TMP_InputField nicknameInputField;
    // public GameObject lobbyUI; // UI элементы лобби, которые нужно скрыть после присоединения к комнате
    private const byte maxPlayersPerRoom = 2;
    
    private void Start()
    {
        // Инициализация Photon
        PhotonNetwork.ConnectUsingSettings();

        // Начальное состояние кнопки - неактивное
        findLobbyButton.interactable = false;
        exitGameButton.onClick.AddListener(OnExitGameButtonClicked);

        // Проверяем, есть ли сохраненный никнейм
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Nickname")))
        {
            nicknameInputField.text = PlayerPrefs.GetString("Nickname");
        }

        if (nicknameInputField != null)
        {
            // Отключаем возможность выделения текста
            nicknameInputField.selectionColor = new Color(0, 0, 0, 0); // Делаем цвет выделения прозрачным

            // Настроим поле для ввода как однострочное, чтобы предотвратить выделение в многострочном режиме
            nicknameInputField.lineType = TMP_InputField.LineType.SingleLine;

            // Отменяем выделение текста при фокусировке на поле
            nicknameInputField.onSelect.AddListener(DisableTextSelectionOnSelect);
        }
        

    }

    void DisableTextSelectionOnSelect(string text)
    {
        nicknameInputField.selectionAnchorPosition = nicknameInputField.selectionFocusPosition;
        nicknameInputField.caretPosition = nicknameInputField.text.Length;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Подключено к серверу Photon");
        statusText.text = "Подключено к серверу Photon";
        findLobbyButton.interactable = true;
    }

    public void FindOrJoinLobby()
    {
        string nickname = nicknameInputField.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            statusText.text = "Введите никнейм!";
            return;
        }

        // Сохраняем никнейм в PlayerPrefs
        PlayerPrefs.SetString("Nickname", nickname);
        PlayerPrefs.Save();

        // Устанавливаем никнейм для текущего игрока
        PhotonNetwork.NickName = nickname;

        

        // Присоединяемся к случайной комнате
        PhotonNetwork.JoinRandomRoom();
      
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Не удалось найти лобби. Создаем новое...");
        CreateRoom();
    }

    private void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = maxPlayersPerRoom };
        roomOptions.PublishUserId = true;
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Лобби создано.");
        statusText.text = "Лобби создано.";
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Присоединились к лобби.");
        statusText.text = "Присоединились к лобби.";

        // Присваиваем никнейм всем игрокам, включая себя
        UpdatePlayerNicknames();

        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
        {
            Debug.Log("Лобби заполнено. Переходим на игровую сцену.");
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Присваиваем никнейм новому игроку
        UpdatePlayerNicknames();

        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
        {
            Debug.Log("Лобби заполнено. Переходим на игровую сцену.");
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    private void UpdatePlayerNicknames()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log($"Игрок {player.ActorNumber}: {player.NickName}");
        }
    }

    private void OnExitGameButtonClicked()
    {
        
        Application.Quit();

        

    }
}
