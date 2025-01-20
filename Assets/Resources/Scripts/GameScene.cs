using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterManager;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_Text roundCounterText;
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

    [Header("Button References")]
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

    [Header("Stat Panels")]
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

    [Header("Resource Colors")]
    public Color energyColor;
    public Color manaColor;
    public Color rageColor;
    public Color focusColor;
    public Color coldBloodColor;

    [Header("Audio Settings")]
    public AudioClip timerTickSound;
    private AudioSource audioSource;

    private string player1Id;
    private string player2Id;
    private GamePlayer player1;
    private GamePlayer player2;
    private Button currentActiveButton = null;
    private int roundCounter;
    private bool player1ActionConfirmed = false;
    private bool player2ActionConfirmed = false;

    [System.Serializable]
    public class GamePlayer
    {
        public string Id, selectedActionButtonName;
        public float maxHealth, maxResource, currentAttackPower, currentDefencePower, currentParryPower, currentKickPower,  currentResource, currentHealPower, currentHealth;
        public int currentPoisons, currentPowerBar, breakroundleftheal, currentBreakPower, breakroundleftdefence;
        public int maxPowerBar, currentSuperAbilityState;
        public ResourceType resourceType;
        public CharacterManager.Character character;

        public GamePlayer(string id) => Id = id;

        public void SetCharacter(CharacterManager.Character character)
        {
            this.character = character;
            currentAttackPower = character.AttackPower;
            currentDefencePower = character.DefencePower;
            currentParryPower = character.ParryPower;
            currentKickPower = character.KickPower;
            currentHealPower = character.HealPower;
            currentBreakPower = character.BreakPower;
            currentPoisons = character.Poisons;
            currentHealth = character.baseHealth;
            maxHealth = character.baseHealth;
            currentResource = character.baseResource;
            maxResource = character.maxResource;
            maxPowerBar = character.maxPowerBar;
            breakroundleftheal = 0; //брейк от пинка по хилу
            breakroundleftdefence = 0; // брейк от пинка по дефу
            currentPowerBar = 0;
            currentSuperAbilityState = 0;
            resourceType = character.resourceType;
            selectedActionButtonName = null;
        }
    }

    private void Start()
    {
        InitializePlayers();
        exitLobbyButton.onClick.AddListener(OnExitLobbyButtonClicked);
        UpdatePlayerNicknames();
        CreateCharacterButtons();
        AssignButtonListeners();
        InitializeUIElements();
        InitializeAudioSource();

        roundCounter = 1;
        roundCounterText.text = roundCounter.ToString();
    }

    private void InitializePlayers()
    {
        player1Id = PhotonNetwork.PlayerList[0].UserId;
        player2Id = PhotonNetwork.PlayerList[1].UserId;
        player1 = new GamePlayer(player1Id);
        player2 = new GamePlayer(player2Id);
    }

    private void AssignButtonListeners()
    {
        Button[] buttons = { attackButton, defButton, parryButton, kickButton, healButton, superAbility };
        foreach (var button in buttons)
        {
            if (button != null)
                button.onClick.AddListener(() => OnButtonPressed(button));
        }
    }

    private void InitializeUIElements()
    {
        powerSliderBar.gameObject.SetActive(false);
        acceptRound.gameObject.SetActive(false);
        sliderValueText.gameObject.SetActive(false);
        powerSliderBar.onValueChanged.AddListener(OnSliderValueChanged);
        acceptRound.onClick.AddListener(OnAcceptRoundButtonClicked);
        UpdateSliderValueText(powerSliderBar.value);
    }

    private void InitializeAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = timerTickSound;
    }

    private void OnDestroy()
    {
        Button[] buttons = { attackButton, defButton, parryButton, kickButton, healButton, superAbility };
        foreach (var button in buttons)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
    }

    private void OnButtonPressed(Button pressedButton)
    {
        if (currentActiveButton == pressedButton)
        {
            SetButtonState(currentActiveButton, false);
            currentActiveButton = null;
            powerSliderBar.gameObject.SetActive(false);
            acceptRound.gameObject.SetActive(false);
            Debug.Log($"Button {pressedButton.name} was unpressed.");
            return;
        }

        if (currentActiveButton != null)
        {
            SetButtonState(currentActiveButton, false);
        }

        SetButtonState(pressedButton, true);
        currentActiveButton = pressedButton;
        powerSliderBar.gameObject.SetActive(true);
        acceptRound.gameObject.SetActive(true);
        sliderValueText.gameObject.SetActive(true);
        Debug.Log($"Current Active Button: {pressedButton.name}");
    }

    private void SetButtonState(Button button, bool isActive)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = isActive ? new Color32(147, 147, 147, 255) : Color.white;
        colors.highlightedColor = isActive ? new Color32(147, 147, 147, 255) : Color.white;
        colors.pressedColor = isActive ? new Color32(147, 147, 147, 255) : Color.white;
        colors.disabledColor = Color.gray;
        button.colors = colors;
        button.GetComponent<Image>().color = colors.normalColor;
    }

    private void CreateCharacterButtons()
    {
        const int buttonWidth = 100, buttonHeight = 100, spacing = 10;
        float panelWidth = Screen.width - 2 * spacing;
        int columns = Mathf.FloorToInt(panelWidth / (buttonWidth + spacing));
        float panelHeight = Screen.height - 2 * spacing;
        int rows = Mathf.FloorToInt(panelHeight / (buttonHeight + spacing));

        for (int i = 0; i < chars.initialCharacters.Length; i++)
        {
            var character = chars.initialCharacters[i];
            Button button = Instantiate(buttonPrefab, characterChoicePanel.transform);
            button.GetComponent<Image>().sprite = character.avatar;

            Transform borderTransform = button.transform.Find("BorderImage");
            if (borderTransform != null)
            {
                Image borderImage = borderTransform.GetComponent<Image>();
                if (borderImage != null)
                {
                    borderImage.color = GetResourceColor(character.resourceType);
                }
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            int row = i / columns, column = i % columns;
            rect.anchoredPosition = new Vector2(column * (buttonWidth + spacing) + spacing, -(row * (buttonHeight + spacing)) - spacing);
            button.onClick.AddListener(() => OnCharacterButtonClicked(character));

            if (row >= rows)
            {
                break;
            }
        }
    }

    private void OnCharacterButtonClicked(CharacterManager.Character selectedCharacter)
    {
        GamePlayer currentPlayer = PhotonNetwork.LocalPlayer.UserId == player1Id ? player1 : player2;
        int playerNumber = currentPlayer == player1 ? 1 : 2;

        UpdatePlayerUI(currentPlayer, selectedCharacter);
        SendSelectedCharacterInfo(playerNumber, selectedCharacter.id);
        characterChoicePanel.SetActive(false);
        StartCoroutine(RoundTimerCoroutine(30f));

        Debug.Log($"Player {playerNumber} has selected a character.");
    }

    private void UpdatePlayerUI(GamePlayer player, CharacterManager.Character character)
    {
        player.SetCharacter(character);

        TMP_Text characterNameText = player == player1 ? selectedCharacterNameForPlayer1 : selectedCharacterNameForPlayer2;
        Image playerImage = player == player1 ? player1Image : player2Image;
        TMP_Text playerHpText = player == player1 ? playerCurrent1HpText : playerCurrent2HpText;
        Image resourceImage = player == player1 ? player1currentResourceImage : player2currentResourceImage;

        UpdateText(characterNameText, character.name);
        if (playerImage != null) playerImage.sprite = character.avatar;
        UpdateText(playerHpText, player.currentHealth.ToString());
        UpdateImageColor(resourceImage, GetResourceColor(player.resourceType));

        float healthFillAmount = Mathf.Clamp01((float)player.currentHealth / player.character.baseHealth);
        Image currentHpImage = player == player1 ? player1CurrentHp : player2CurrentHp;
        UpdateImageFill(currentHpImage, healthFillAmount);

        float resourceFillAmount = Mathf.Clamp01((float)player.currentResource / player.character.maxResource);
        UpdateImageFill(resourceImage, resourceFillAmount);

        UpdateStatPanel(player);
        UpdatePowerSlider(player);
    }

    private void UpdatePlayerStats(GamePlayer player)
    {
        TMP_Text playerHpText = player == player1 ? playerCurrent1HpText : playerCurrent2HpText;
        UpdateText(playerHpText, player.currentHealth.ToString());
        UpdateStatPanel(player);
        UpdatePowerSlider(player);

        float healthFillAmount = Mathf.Clamp01((float)player.currentHealth / player.character.baseHealth);
        Image currentHpImage = player == player1 ? player1CurrentHp : player2CurrentHp;
        UpdateImageFill(currentHpImage, healthFillAmount);

        float resourceFillAmount = Mathf.Clamp01((float)player.currentResource / player.character.maxResource);
        Image currentResourceImage = player == player1 ? player1currentResourceImage : player2currentResourceImage;
        UpdateImageFill(currentResourceImage, resourceFillAmount);
    }

    private void UpdatePowerSlider(GamePlayer player)
    {
        if ((player == player1 && PhotonNetwork.LocalPlayer.UserId == player1Id) ||
            (player == player2 && PhotonNetwork.LocalPlayer.UserId == player2Id))
        {
            if (powerSliderBar != null)
            {
                powerSliderBar.maxValue = Mathf.Min(player.currentResource, player.maxPowerBar);
                powerSliderBar.value = player.currentPowerBar;
            }
        }
    }

    private void UpdateStatPanel(GamePlayer player)
    {
        GameObject statPanel = player == player1 ? player1CurrentStatPanel : player2CurrentStatPanel;
        TMP_Text[] statTexts = new TMP_Text[]
        {
            player == player1 ? player1CurrentAttack : player2CurrentAttack,
            player == player1 ? player1CurrentDef : player2CurrentDef,
            player == player1 ? player1CurrentParry : player2CurrentParry,
            player == player1 ? player1CurrentKick : player2CurrentKick,
            player == player1 ? player1CurrentHeal : player2CurrentHeal,
            player == player1 ? player1PoisonLeft : player2PoisonLeft,
            player == player1 ? player1KickTimer : player2KickTimer,
            player == player1 ? player1CurrentResource : player2CurrentResource
        };

        float[] statValues = new float[]
        {
            player.currentAttackPower, player.currentDefencePower, player.currentParryPower,
            player.currentKickPower, player.currentHealPower, player.currentPoisons,
            player.currentBreakPower, player.currentResource
        };

        if (statPanel != null)
        {
            statPanel.SetActive(true);
            for (int i = 0; i < statTexts.Length; i++)
            {
                UpdateText(statTexts[i], statValues[i].ToString());
            }
        }
    }

    private void UpdateText(TMP_Text textComponent, string value)
    {
        if (textComponent != null)
        {
            textComponent.text = value;
        }
    }

    private void UpdateImageFill(Image imageComponent, float fillAmount)
    {
        if (imageComponent != null)
        {
            imageComponent.fillAmount = fillAmount;
        }
    }

    private void UpdateImageColor(Image resourceImage, Color color)
    {
        if (resourceImage != null)
        {
            resourceImage.color = color;
        }
    }

    private void OnSliderValueChanged(float value)
    {
        int roundedValue = Mathf.RoundToInt(value);
        powerSliderBar.value = roundedValue;
        UpdateSliderValueText(roundedValue);
    }

    private void UpdateSliderValueText(float value)
    {
        if (sliderValueText != null)
        {
            sliderValueText.text = value.ToString();
            RectTransform handleRect = powerSliderBar.handleRect;
            sliderValueText.transform.position = handleRect.position + new Vector3(0, Screen.height * 0.05f, 0);
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

    private void OnAcceptRoundButtonClicked()
    {
        if (currentActiveButton == null)
        {
            Debug.LogWarning("No action button selected!");
            return;
        }

        StopAllCoroutines();
        acceptRound.interactable = false;
        string actionButtonName = currentActiveButton.name;
        float sliderValue = powerSliderBar.value;
        int playerNumber = PhotonNetwork.LocalPlayer.UserId == player1Id ? 1 : 2;

        photonView.RPC("SendActionDataRPC", RpcTarget.All, playerNumber, actionButtonName, sliderValue);
        Debug.Log($"Action confirmed: {actionButtonName} with power: {sliderValue}");
    }

    [PunRPC]
    private void SendActionDataRPC(int playerNumber, string actionButtonName, float sliderValue)
    {
        Debug.Log($"Player {playerNumber} selected action: {actionButtonName} with power: {sliderValue}");

        if (playerNumber == 1)
        {
            player1ActionConfirmed = true;
            player1.selectedActionButtonName = actionButtonName;
            player1.currentPowerBar = (int)sliderValue;
        }
        else if (playerNumber == 2)
        {
            player2ActionConfirmed = true;
            player2.selectedActionButtonName = actionButtonName;
            player2.currentPowerBar = (int)sliderValue;
        }

        // Проверяем, подтвердили ли оба игрока свои действия
        if (player1ActionConfirmed && player2ActionConfirmed)
        {
            acceptRound.interactable = true;

            // Обработка действий игроков
            ProcessTurn(player1, player2);
            

            UpdatePlayerStats(player1);
            UpdatePlayerStats(player2);

            Debug.Log("Both players have confirmed their actions.");

            // Обновляем счетчик раундов
            roundCounter++;
            roundCounterText.text = roundCounter.ToString();

            // Сбрасываем флаги подтверждения действий
            player1ActionConfirmed = false;
            player2ActionConfirmed = false;

            // Начинаем новый раунд
            StartNewRound();
        }
    }

    private IEnumerator RoundTimerCoroutine(float duration)
    {
        float remainingTime = duration;
        while (remainingTime > 0)
        {
            roundTimer.text = Mathf.Ceil(remainingTime).ToString();
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }

        roundTimer.text = "0";
        Debug.Log("Round timer has ended!");
    }

    private void StartNewRound()
    {
        Debug.Log($"Starting round {roundCounter}!");
        StartCoroutine(RoundTimerCoroutine(30f));
    }

    void ProcessTurn(GamePlayer player1, GamePlayer player2)
    {
        player1.currentResource -= player1.currentPowerBar;
        player2.currentResource -= player2.currentPowerBar;

        // атака первого игрока, остальные кнопки второго игрока
        if(player1.selectedActionButtonName == "attackButton" && player2.selectedActionButtonName == "attackButton")
        {
            player1.currentHealth = player1.currentHealth - (player2.currentAttackPower + (player2.currentAttackPower * player2.currentPowerBar / 100f));
            player2.currentHealth = player2.currentHealth - (player1.currentAttackPower + (player1.currentPowerBar * player1.currentAttackPower/ 100f));
        }

        if (player1.selectedActionButtonName == "attackButton" && player2.selectedActionButtonName == "defButton")
        {
            player1.currentHealth = player1.currentHealth - (player2.currentDefencePower +(player2.currentDefencePower * player2.currentPowerBar/ 100f));
            player2.currentHealth = player2.currentHealth - 0;
        }
        if (player1.selectedActionButtonName == "attackButton" && player2.selectedActionButtonName == "parryButton")
        {
            player1.currentHealth = player1.currentHealth - (player2.currentParryPower + (player2.currentParryPower * player2.currentPowerBar / 100f) + 
                player1.currentAttackPower / 2);
            player2.currentHealth = player2.currentHealth - player1.currentAttackPower/2;
        }
        if (player1.selectedActionButtonName == "attackButton" && player2.selectedActionButtonName == "kickButton")
        {
            player1.currentHealth = player1.currentHealth - (player2.currentKickPower + (player2.currentKickPower * player2.currentPowerBar / 100f));
            player2.currentHealth = player2.currentHealth - (player1.currentAttackPower + (player1.currentPowerBar * player1.currentAttackPower / 100f));
            
        }
        if (player1.selectedActionButtonName == "attackButton" && player2.selectedActionButtonName == "healButton")
        {
            player1.currentHealth = player1.currentHealth - 0;
            player2.currentHealth = player2.currentHealth - (player1.currentAttackPower + (player1.currentPowerBar * player1.currentAttackPower / 100f));
        }
        // защита первого игрока, остальные кнопки второго игрока
        if (player1.selectedActionButtonName == "defButton" && player2.selectedActionButtonName == "attackButton")
        {
           player1.currentHealth = player1.currentHealth - 0;
           player2.currentHealth = player2.currentHealth - (player1.currentDefencePower + (player1.currentDefencePower * player1.currentPowerBar / 100f));
         }
        if (player1.selectedActionButtonName == "defButton" && player2.selectedActionButtonName == "defButton")
        {
            player1.currentHealth = player1.currentHealth - 0;
            player2.currentHealth = player2.currentHealth - 0;
        }
        if (player1.selectedActionButtonName == "defButton" && player2.selectedActionButtonName == "parryButton")
        {
            player1.currentHealth = player1.currentHealth - (player2.currentDefencePower + (player2.currentDefencePower * player2.currentPowerBar / 100f) + 
                player1.currentKickPower/2);
            player2.currentHealth = player2.currentHealth - 0;  
        }
        if (player1.selectedActionButtonName == "defButton" && player2.selectedActionButtonName == "kickButton")
        {
            player1.currentHealth = player1.currentHealth - (player2.currentKickPower + (player2.currentKickPower * player2.currentPowerBar / 100f));
            player2.currentHealth = player2.currentHealth - 0;
            player1.breakroundleftdefence = player2.currentBreakPower;
        }
        if (player1.selectedActionButtonName == "defButton" && player2.selectedActionButtonName == "healButton")
        {
            player1.currentHealth = player1.currentHealth - 0;
            player2.currentHealth = player2.currentHealth + (player2.currentHealPower + (player2.currentHealPower * player2.currentPowerBar / 100f));
        }
        
        // Парирование первого игрока, остальные кнопки второго игрока
        if (player1.selectedActionButtonName == "parryButton" && player2.selectedActionButtonName == "attackButton")
        {
            player1.currentHealth = player1.currentHealth - player2.currentAttackPower / 2;
            player2.currentHealth = player2.currentHealth - (player1.currentDefencePower + (player1.currentDefencePower * player1.currentPowerBar / 100f) + 
                player2.currentAttackPower/2);
        }
        if (player1.selectedActionButtonName == "parryButton" && player2.selectedActionButtonName == "defButton")
        {
            player2.currentHealth = player2.currentHealth - (player1.currentDefencePower + (player1.currentDefencePower * player1.currentPowerBar / 100f));
            player2.currentHealth = player2.currentHealth - 0;
        }
        if (player1.selectedActionButtonName == "parryButton" && player2.selectedActionButtonName == "parryButton")
        {
            player1.currentHealth = player1.currentHealth - (player2.currentParryPower + (player2.currentParryPower * player2.currentPowerBar / 100f));
            player2.currentHealth = player2.currentHealth - (player1.currentParryPower + (player1.currentParryPower * player1.currentPowerBar / 100f));
        }
        if (player1.selectedActionButtonName == "parryButton" && player2.selectedActionButtonName == "kickButton")
        {
            player1.currentHealth = player1.currentHealth - 0;
            player2.currentHealth = player2.currentHealth - (player1.currentParryPower + (player1.currentParryPower * player1.currentPowerBar / 100f) +
                player2.currentKickPower / 2);
        }
        if (player1.selectedActionButtonName == "parryButton" && player2.selectedActionButtonName == "healButton")
        {
            player1.currentHealth = player1.currentHealth - 0;
            player2.currentHealth = player2.currentHealth + player2.currentHealPower + (player2.currentHealPower * player2.currentPowerBar / 100f);
        }
        // кик первого игрока, все остальное второго игрока
        if (player1.selectedActionButtonName == "kickButton" && player2.selectedActionButtonName == "attackButton")
        {
            player1.currentHealth = player1.currentHealth - (player2.currentAttackPower + (player2.currentPowerBar * player2.currentAttackPower / 100f));
            player2.currentHealth = player2.currentHealth - (player1.currentKickPower + (player1.currentKickPower * player1.currentPowerBar / 100f));
        }
        if (player1.selectedActionButtonName == "kickButton" && player2.selectedActionButtonName == "defButton")
        {
            player1.currentHealth = player1.currentHealth - 0;
            player2.currentHealth = player2.currentHealth - (player1.currentKickPower + (player1.currentKickPower * player1.currentPowerBar / 100f));
            player2.breakroundleftdefence = player1.currentBreakPower;
        }
        if (player1.selectedActionButtonName == "kickButton" && player2.selectedActionButtonName == "parryButton")
        {
            player1.currentHealth = player1.currentHealth - (player2.currentParryPower + (player2.currentParryPower * player2.currentPowerBar / 100f) +
                player2.currentKickPower / 2);
            player2.currentHealth = player2.currentHealth - ((player1.currentKickPower + (player1.currentKickPower * player1.currentPowerBar / 100f))/2f);
        }
        if (player1.selectedActionButtonName == "kickButton" && player2.selectedActionButtonName == "kickButton")
        {
            player1.currentHealth = player1.currentHealth - (player2.currentKickPower + (player2.currentKickPower * player2.currentPowerBar / 100f));
            player2.currentHealth = player2.currentHealth - (player1.currentKickPower + (player1.currentKickPower * player1.currentPowerBar / 100f));
        }
        if (player1.selectedActionButtonName == "kickButton" && player2.selectedActionButtonName == "healButton")
        {
            player1.currentHealth = player1.currentHealth - 0;
            player2.currentHealth = player2.currentHealth - (player1.currentKickPower + (player1.currentKickPower * player1.currentPowerBar / 100f));
            player2.breakroundleftdefence = player1.currentBreakPower;
        }
        // хилка первого игрока, все остальное второго игрока
        if (player1.selectedActionButtonName == "healButton" && player2.selectedActionButtonName == "attackButton")
        {
            player1.currentHealth = player1.currentHealth - player2.currentAttackPower + (player2.currentPowerBar * player2.currentAttackPower / 100f);
            player2.currentHealth = player2.currentHealth - 0;     
        }
        if (player1.selectedActionButtonName == "healButton" && player2.selectedActionButtonName == "defButton")
        {
            player1.currentHealth = player1.currentHealth + player1.currentHealPower + (player1.currentHealPower * player1.currentPowerBar / 100f);
            player2.currentHealth = player2.currentHealth - 0;
        }
        if (player1.selectedActionButtonName == "healButton" && player2.selectedActionButtonName == "parryButton")
        {
            player1.currentHealth = player1.currentHealth + player1.currentHealPower + (player1.currentHealPower * player1.currentPowerBar / 100f);
            player2.currentHealth = player2.currentHealth - 0;
        }
        if (player1.selectedActionButtonName == "healButton" && player2.selectedActionButtonName == "kickButton")
        {
            player1.currentHealth = player1.currentHealth - (player1.currentKickPower + (player1.currentKickPower * player1.currentPowerBar / 100f));
            player2.currentHealth = player2.currentHealth - 0;
            player1.breakroundleftdefence = player2.currentBreakPower;
        }
        if (player1.selectedActionButtonName == "healButton" && player2.selectedActionButtonName == "healButton")
        {
            player1.currentHealth = player1.currentHealth + player1.currentHealPower + (player1.currentHealPower * player1.currentPowerBar / 100f);
            player2.currentHealth = player2.currentHealth + player2.currentHealPower + (player2.currentHealPower * player2.currentPowerBar / 100f);
        }
        if (player1.currentHealth > player1.maxHealth)
        {
            player1.currentHealth = player1.maxHealth;
        }
        if (player2.currentHealth > player2.maxHealth)
        {
            player2.currentHealth = player2.maxHealth;
        }

    }
}