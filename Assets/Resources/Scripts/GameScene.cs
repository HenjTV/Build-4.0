using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterManager;

public class GameManager : MonoBehaviourPunCallbacks
{


    [Header("UI References")]
    public TMP_Text player1NicknameText;
    public TMP_Text player2NicknameText;
    
    public Image player1Image;
    public Image player2Image;
    public GameObject characterChoicePanel;
    public GameObject controlButtonsPanel;

    public TMP_Text selectedCharacterNameForPlayer1;
    public TMP_Text selectedCharacterNameForPlayer2;

    public TMP_Text playerCurrent1HpText;
    public TMP_Text playerCurrent2HpText;
    public TMP_Text roundTimer;
    public TMP_Text combatLog;

    public Image player1CurrentHp;
    public Image player2CurrentHp;
    public Image player1currentResourceImage;
    public Image player2currentResourceImage;

    public Button exitLobbyButton;

    [Header("Кнопки управления")]

    public Button buttonPrefab;
    public Button attackButton;
    public Button defButton;
    public Button parryButton;
    public Button kickButton;
    public Button healButton;
    public Button superAbility;
    public Button acceptRound;
    public Slider powerSliderBar;
    public TMP_Text sliderValueText;

    [Header("Статы персонажей")]
    public GameObject player1CurrentStatPanel;
    public GameObject player2CurrentStatPanel;

    public TMP_Text player1CurrentAttack;
    public TMP_Text player1CurrentDef;
    public TMP_Text player1CurrentParry;
    public TMP_Text player1CurrentKick;
    public TMP_Text player1CurrentHeal;
    public TMP_Text player1PoisonLeft;
    public TMP_Text player1KickTimer;
    public TMP_Text player1CurrentResource;
    public TMP_Text player2CurrentAttack;
    public TMP_Text player2CurrentDef;
    public TMP_Text player2CurrentParry;
    public TMP_Text player2CurrentKick;
    public TMP_Text player2CurrentHeal;
    public TMP_Text player2PoisonLeft;
    public TMP_Text player2KickTimer;
    public TMP_Text player2CurrentResource;

    [Header("Character Manager")]
    public CharacterManager chars;

    [Header("Цвета классов")]
    public Color energyColor;
    public Color manaColor;
    public Color rageColor;
    public Color focusColor;
    public Color coldBloodColor;

    [Header("Audio Settings")]
    public AudioClip timerTickSound; // Звук для таймера
    private AudioSource audioSource; // Аудиоисточник

    private string player1Id, player2Id;
    private GamePlayer player1, player2;
    private Button currentActiveButton = null;
    



    // Добавляем флаги подтвержденного хода
    private bool player1ActionConfirmed = false;
    private bool player2ActionConfirmed = false;
    




    [System.Serializable]
    public class GamePlayer
    {
        public string Id;
        public int currentAttackPower, currentDefPower, currentParryPower, currentKickDamage, currentBreak;
        public int currentHealPower, currentPoisons, currentHealth, currentResource, currentPowerBar;
        public int maxPowerBar, maxResource, currentSuperAbilityState;
        public ResourceType resourceType;
        public CharacterManager.Character character;

        public GamePlayer(string id) => Id = id;

        public void SetCharacter(CharacterManager.Character character)
        {
            this.character = character;
            currentAttackPower = character.attackPower;
            currentDefPower = character.defense;
            currentParryPower = character.parry;
            currentKickDamage = character.kickDamage;
            currentHealPower = character.healPower;
            currentBreak = character.kickCooldown;
            currentPoisons = character.healCharges;
            currentHealth = character.baseHealth;
            currentResource = character.baseResource;
            maxResource = character.maxResource;
            maxPowerBar = character.booster;
            currentPowerBar = 0;
            currentSuperAbilityState = 0;
            resourceType = character.resourceType;
        }
    }

    private void Start()
    {
        InitializePlayers();
        exitLobbyButton.onClick.AddListener(OnExitLobbyButtonClicked);
        UpdatePlayerNicknames();
        CreateCharacterButtons();
        AssignButtonListeners(attackButton);
        AssignButtonListeners(defButton);
        AssignButtonListeners(parryButton);
        AssignButtonListeners(kickButton);
        AssignButtonListeners(healButton);
        AssignButtonListeners(superAbility);
        powerSliderBar.gameObject.SetActive(false); // слайдер атаки не активен до нажатия кнопки на панели игрока
        acceptRound.gameObject.SetActive(false); // кнопка принятия раунда не активна до нажатия кнопки на панели игрока
        sliderValueText.gameObject.SetActive(false);
        // Устанавливаем обработчик изменения значения слайдера

        // Инициализация AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = timerTickSound;

        powerSliderBar.onValueChanged.AddListener(OnSliderValueChanged);

        // Обновляем текст при запуске
        UpdateSliderValueText(powerSliderBar.value);

        // листнер кнопки подтверждения хода
        acceptRound.onClick.AddListener(OnAcceptRoundButtonClicked);
    }



    private void InitializePlayers()
    {
        player1Id = PhotonNetwork.PlayerList[0].UserId;
        player2Id = PhotonNetwork.PlayerList[1].UserId;
        player1 = new GamePlayer(player1Id);
        player2 = new GamePlayer(player2Id);
    }
    // настройка кнопок управления

    private void AssignButtonListeners(Button button)
    {
        if (button != null)
        {
            button.onClick.AddListener(() => OnButtonPressed(button));
        }
    }

    private void OnButtonPressed(Button pressedButton)
    {
        // Если нажата текущая активная кнопка, "отжимаем" её
        if (currentActiveButton == pressedButton)
        {
            SetButtonState(currentActiveButton, false);
            currentActiveButton = null;

            // Деактивируем связанные объекты
            powerSliderBar.gameObject.SetActive(false);
            acceptRound.gameObject.SetActive(false);

            Debug.Log($"Button {pressedButton.name} was unpressed.");
            return;
        }

        // Если есть другая активная кнопка, сбросить её состояние
        if (currentActiveButton != null)
        {
            SetButtonState(currentActiveButton, false);
        }

        // Установить новое активное состояние для нажатой кнопки
        SetButtonState(pressedButton, true);
        currentActiveButton = pressedButton;

        // Активировать связанные объекты
        powerSliderBar.gameObject.SetActive(true);
        acceptRound.gameObject.SetActive(true);
        sliderValueText.gameObject.SetActive(true);

        Debug.Log($"Current Active Button: {pressedButton.name}");
    }


    private void SetButtonState(Button button, bool isActive)
    {
        ColorBlock colors = button.colors;

        // Установите цвет в зависимости от состояния кнопки
        colors.normalColor = isActive ? new Color32(147, 147, 147, 255) : Color.white;
        colors.highlightedColor = isActive ? new Color32(147, 147, 147, 255) : Color.white;
        colors.pressedColor = isActive ? new Color32(147, 147, 147, 255) : Color.white;
        colors.disabledColor = Color.gray; // Цвет для неактивной кнопки (опционально)

        // Обновить цвета кнопки
        button.colors = colors;

        // Применяем изменения сразу, чтобы UI обновился
        button.GetComponent<Image>().color = colors.normalColor;
    }

    private void OnDestroy()
    {
        // Отписываемся от событий, чтобы избежать утечек памяти
        RemoveButtonListeners(attackButton);
        RemoveButtonListeners(defButton);
        RemoveButtonListeners(parryButton);
        RemoveButtonListeners(kickButton);
        RemoveButtonListeners(healButton);
        RemoveButtonListeners(superAbility);
    }

    private void RemoveButtonListeners(Button button)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    // конец найстройки кнопок управления


    private void CreateCharacterButtons()
    {
        const int columns = 5, buttonWidth = 100, buttonHeight = 100, spacing = 10;

       


        for (int i = 0; i < chars.initialCharacters.Length; i++)
        {
            var character = chars.initialCharacters[i];
            Button button = Instantiate(buttonPrefab, characterChoicePanel.transform);
            button.GetComponent<Image>().sprite = character.avatar;

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            int row = i / columns, column = i % columns;
            rect.anchoredPosition = new Vector2(
                column * (buttonWidth + spacing) + spacing,
                -(row * (buttonHeight + spacing)) - spacing
            );

            button.onClick.AddListener(() => OnCharacterButtonClicked(character));
        }
    }

    private void OnCharacterButtonClicked(CharacterManager.Character selectedCharacter)
    {
        GamePlayer currentPlayer = PhotonNetwork.LocalPlayer.UserId == player1Id ? player1 : player2;
        int playerNumber = currentPlayer == player1 ? 1 : 2;

        UpdatePlayerUI(currentPlayer, selectedCharacter);
        SendSelectedCharacterInfo(playerNumber, selectedCharacter.id);

        characterChoicePanel.SetActive(false);

        if (playerNumber == 1)
        {
            
            Debug.Log("Player 1 has selected a character.");
        }
        else if (playerNumber == 2)
        {
            
            Debug.Log("Player 2 has selected a character.");
        }

        
    }


    private void UpdatePlayerUI(GamePlayer player, CharacterManager.Character character)
    {
        player.SetCharacter(character);

        TMP_Text characterNameText = player == player1 ? selectedCharacterNameForPlayer1 : (player == player2 ? selectedCharacterNameForPlayer2 : null);
        Image playerImage = player == player1 ? player1Image : (player == player2 ? player2Image : null);
        TMP_Text playerHpText = player == player1 ? playerCurrent1HpText : (player == player2 ? playerCurrent2HpText : null);
        Image ResourceImage = player == player1 ? player1currentResourceImage : (player == player2 ? player2currentResourceImage : null);


        UpdateStatText(characterNameText, character.name);
        if (playerImage != null) playerImage.sprite = character.avatar;
        UpdateStatText(playerHpText, player.currentHealth.ToString());
        UpdateImageColor(ResourceImage, GetResourceColor(player.resourceType));

        UpdateStatPanel(player);
        if ((player == player1 && PhotonNetwork.LocalPlayer.UserId == player1Id) ||
        (player == player2 && PhotonNetwork.LocalPlayer.UserId == player2Id))
        {
            if (powerSliderBar != null) // установка размера бустера
            {
                powerSliderBar.maxValue = player.maxPowerBar;
                powerSliderBar.value = player.currentPowerBar;
            }
        }


    }

    private void UpdateStatPanel(GamePlayer player)
    {
        GameObject statPanel = player == player1 ? player1CurrentStatPanel : (player == player2 ? player2CurrentStatPanel : null);
        TMP_Text attackText = player == player1 ? player1CurrentAttack : (player == player2 ? player2CurrentAttack : null);
        TMP_Text defText = player == player1 ? player1CurrentDef : (player == player2 ? player2CurrentDef : null);
        TMP_Text parryText = player == player1 ? player1CurrentParry : (player == player2 ? player2CurrentParry : null);
        TMP_Text kickText = player == player1 ? player1CurrentKick : (player == player2 ? player2CurrentKick : null);
        TMP_Text healText = player == player1 ? player1CurrentHeal : (player == player2 ? player2CurrentHeal : null);
        TMP_Text poisonText = player == player1 ? player1PoisonLeft : (player == player2 ? player2PoisonLeft : null);
        TMP_Text kickTimerText = player == player1 ? player1KickTimer : (player == player2 ? player2KickTimer : null);
        TMP_Text resourceText = player == player1 ? player1CurrentResource : (player == player2 ? player2CurrentResource : null);


        if (statPanel != null)
        {
            statPanel.SetActive(true);
            UpdateStatText(attackText, player.currentAttackPower.ToString());
            UpdateStatText(defText, player.currentDefPower.ToString());
            UpdateStatText(parryText, player.currentParryPower.ToString());
            UpdateStatText(kickText, player.currentKickDamage.ToString());
            UpdateStatText(healText, player.currentHealPower.ToString());
            UpdateStatText(poisonText, player.currentPoisons.ToString());
            UpdateStatText(kickTimerText, player.currentBreak.ToString());
            UpdateStatText(resourceText, player.currentResource.ToString());
        }
    }

    private void UpdateStatText(TMP_Text textComponent, string value)
    {
        if (textComponent != null)
            textComponent.text = value;
    }

    private void OnSliderValueChanged(float value)
    {
        // Округляем значение до ближайшего целого числа
        int roundedValue = Mathf.RoundToInt(value);
        powerSliderBar.value = roundedValue;

        // Обновляем текст
        UpdateSliderValueText(roundedValue);
    }

    private void UpdateSliderValueText(float value)
    {
        // Устанавливаем текст в зависимости от текущего значения
        if (sliderValueText != null)
        {
            sliderValueText.text = value.ToString();

            // Опционально: Переместить текст над хендлом
            RectTransform handleRect = powerSliderBar.handleRect;
            sliderValueText.transform.position = handleRect.position + new Vector3(0, Screen.height * 0.05f, 0);            // Смещение над хендлом
        }
    }
    private void UpdateImageColor(Image resourceImage, Color color)
    {
        if (resourceImage != null)
        {
            resourceImage.color = color;
        }
    }

    private void SendSelectedCharacterInfo(int playerNumber, int characterId)
    {
        photonView.RPC("SendSelectedCharacterInfoRPC", RpcTarget.AllBuffered, playerNumber, characterId);
    }

    [PunRPC]
    private void SendSelectedCharacterInfoRPC(int playerNumber, int characterId)
    {
        var character = System.Array.Find(chars.initialCharacters, ch => ch.id == characterId);
        if (character == null) return;

        if (playerNumber == 1)
            UpdatePlayerUI(player1, character);
        else if (playerNumber == 2)
            UpdatePlayerUI(player2, character);
    }

    public override void OnPlayerEnteredRoom(Player otherPlayer)
    {
        UpdatePlayerNicknames();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.UserId == player1Id)
            player1NicknameText.text = "Leaver";
        else if (otherPlayer.UserId == player2Id)
            player2NicknameText.text = "Leaver";

        UpdatePlayerNicknames();
    }

    private void UpdatePlayerNicknames()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.UserId == player1Id)
                player1NicknameText.text = player.NickName;
            else if (player.UserId == player2Id)
                player2NicknameText.text = player.NickName;
        }
    }

    private void OnExitLobbyButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Lobby");
    }

    private Color GetResourceColor(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Energy => energyColor,
            ResourceType.Mana => manaColor,
            ResourceType.Rage => rageColor,
            ResourceType.Focus => focusColor,
            ResourceType.ColdBlood => coldBloodColor,
            _ => Color.gray,
        };
    }

   
    // обработчик нажатия на кнопку ОК подтверждения хода
    private void OnAcceptRoundButtonClicked()
    {
        if (currentActiveButton == null)
        {
            Debug.LogWarning("No action button selected!");
            return;
        }

        

        // Получение текущих данных
        string actionButtonName = currentActiveButton.name;
        float sliderValue = powerSliderBar.value;
        int playerNumber = PhotonNetwork.LocalPlayer.UserId == player1Id ? 1 : 2;

        // Блокируем панель управления после подтверждения хода
        LockControlPanelForPlayer(playerNumber);

        // Отправить данные другому игроку через RPC
        photonView.RPC("SendActionDataRPC", RpcTarget.AllBuffered, playerNumber, actionButtonName, sliderValue);

        Debug.Log($"Action confirmed: {actionButtonName} with power: {sliderValue}");
    }
    // блокируем панель игрока после нажатия кнопки ОК
    private void LockControlPanelForPlayer(int playerNumber)
    {
        // Блокировка панели управления игрока после подтверждения хода
        if (playerNumber == 1)
        {
            // controlButtonsPanel.SetActive(false); // блокируем панель управления для игрока 1
            Debug.Log("Control panel locked for Player 1. Timer for Player 1 stopped.");
        }
        else if (playerNumber == 2)
        {
            // controlButtonsPanel.SetActive(false); // блокируем панель управления для игрока 2
            Debug.Log("Control panel locked for Player 2. Timer for Player 2 stopped.");
        }
    }

    // данные отправлены другим игрокам о нажатии кнопки ОКей
    [PunRPC]
    private void SendActionDataRPC(int playerNumber, string actionButtonName, float sliderValue)
    {
        Debug.Log($"Player {playerNumber} selected action: {actionButtonName} with power: {sliderValue}");

        // Установка флага для соответствующего игрока
        if (playerNumber == 1)
        {
            player1ActionConfirmed = true;
        }
        else if (playerNumber == 2)
        {
            player2ActionConfirmed = true;
        }

        // Проверка, отправили ли оба игрока свои данные
        if (player1ActionConfirmed && player2ActionConfirmed)
        {
            Debug.Log("Both players have confirmed their actions.");

            

            // Сбрасываем флаги
            player1ActionConfirmed = false;
            player2ActionConfirmed = false;

        }
    }

    
}
