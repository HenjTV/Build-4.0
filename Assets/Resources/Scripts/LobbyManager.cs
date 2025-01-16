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
    // public GameObject lobbyUI; // UI �������� �����, ������� ����� ������ ����� ������������� � �������
    private const byte maxPlayersPerRoom = 2;
    
    private void Start()
    {
        // ������������� Photon
        PhotonNetwork.ConnectUsingSettings();

        // ��������� ��������� ������ - ����������
        findLobbyButton.interactable = false;
        exitGameButton.onClick.AddListener(OnExitGameButtonClicked);

        // ���������, ���� �� ����������� �������
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Nickname")))
        {
            nicknameInputField.text = PlayerPrefs.GetString("Nickname");
        }

        if (nicknameInputField != null)
        {
            // ��������� ����������� ��������� ������
            nicknameInputField.selectionColor = new Color(0, 0, 0, 0); // ������ ���� ��������� ����������

            // �������� ���� ��� ����� ��� ������������, ����� ������������� ��������� � ������������� ������
            nicknameInputField.lineType = TMP_InputField.LineType.SingleLine;

            // �������� ��������� ������ ��� ����������� �� ����
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
        Debug.Log("���������� � ������� Photon");
        statusText.text = "���������� � ������� Photon";
        findLobbyButton.interactable = true;
    }

    public void FindOrJoinLobby()
    {
        string nickname = nicknameInputField.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            statusText.text = "������� �������!";
            return;
        }

        // ��������� ������� � PlayerPrefs
        PlayerPrefs.SetString("Nickname", nickname);
        PlayerPrefs.Save();

        // ������������� ������� ��� �������� ������
        PhotonNetwork.NickName = nickname;

        

        // �������������� � ��������� �������
        PhotonNetwork.JoinRandomRoom();
      
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("�� ������� ����� �����. ������� �����...");
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
        Debug.Log("����� �������.");
        statusText.text = "����� �������.";
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("�������������� � �����.");
        statusText.text = "�������������� � �����.";

        // ����������� ������� ���� �������, ������� ����
        UpdatePlayerNicknames();

        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
        {
            Debug.Log("����� ���������. ��������� �� ������� �����.");
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // ����������� ������� ������ ������
        UpdatePlayerNicknames();

        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
        {
            Debug.Log("����� ���������. ��������� �� ������� �����.");
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    private void UpdatePlayerNicknames()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log($"����� {player.ActorNumber}: {player.NickName}");
        }
    }

    private void OnExitGameButtonClicked()
    {
        
        Application.Quit();

        

    }
}
