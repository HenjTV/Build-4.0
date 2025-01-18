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
    public TMP_Text roundcountertext;
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
        public string Id;
        public string selectedActionButtonName;
        public int currentAttackPower;
        public int currentDefPower;
        public int currentParryPower;
        public int currentKickDamage;
        public int currentBreak;
        public int currentHealPower;
        public int currentPoisons;
        public int currentHealth;
        public int currentResource;
        public int currentPowerBar;
        public int maxPowerBar;
        public int maxResource;
        public int currentSuperAbilityState;
        public int maxHealth;
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
            maxHealth = character.baseHealth;
            currentResource = character.baseResource;
            maxResource = character.maxResource;
            maxPowerBar = character.booster;
            currentPowerBar = 0;
            currentSuperAbilityState = 0;
            resourceType = character.resourceType;
            selectedActionButtonName = null;
        }
    }

    private void Start()
    {
        InitializePlayers();
        InitializeUI();
        InitializeAudioSource();
        InitializeButtonListeners();
        InitializeRoundCounter();
    }

    private void InitializePlayers()
    {
        player1Id = PhotonNetwork.PlayerList[0].UserId;
        player2Id = PhotonNetwork.PlayerList[1].UserId;
        player1 = new GamePlayer(player1Id);
        player2 = new GamePlayer(player2Id);
    }

    private void InitializeUI()
    {
        exitLobbyButton.onClick.AddListener(OnExitLobbyButtonClicked);
        UpdatePlayerNicknames();
        CreateCharacterButtons();
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

    private void InitializeButtonListeners()
    {
        Button[] buttons = { attackButton, defButton, parryButton, kickButton, healButton, superAbility };
        foreach (var button in buttons)
        {
            if (button != null)
                button.onClick.AddListener(() => OnButtonPressed(button));
        }
    }

    private void InitializeRoundCounter()
    {
        roundCounter = 1;
        roundcountertext.text = roundCounter.ToString();
    }

    private void OnButtonPressed(Button pressedButton)
    {
        if (currentActiveButton == pressedButton)
        {
            SetButtonState(currentActiveButton, false);
            currentActiveButton = null;
            SetUIElementsActive(false);
            Debug.Log($"Button {pressedButton.name} was unpressed.");
            return;
        }

        if (currentActiveButton != null)
        {
            SetButtonState(currentActiveButton, false);
        }

        SetButtonState(pressedButton, true);
        currentActiveButton = pressedButton;
        SetUIElementsActive(true);
        Debug.Log($"Current Active Button: {pressedButton.name}");
    }

    private void SetUIElementsActive(bool isActive)
    {
        powerSliderBar.gameObject.SetActive(isActive);
        acceptRound.gameObject.SetActive(isActive);
        sliderValueText.gameObject.SetActive(isActive);
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

    private void OnDestroy()
    {
        Button[] buttons = { attackButton, defButton, parryButton, kickButton, healButton, superAbility };
        foreach (var button in buttons)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
    }

    private void UpdatePlayerUI(GamePlayer player, CharacterManager.Character character)
    {
        player.SetCharacter(character);

        TMP_Text characterNameText = player == player1 ? selectedCharacterNameForPlayer1 : selectedCharacterNameForPlayer2;
        Image playerImage = player == player1 ? player1Image : player2Image;
        TMP_Text playerHpText = player == player1 ? playerCurrent1HpText : playerCurrent2HpText;
        Image resourceImage = player == player1 ? player1currentResourceImage : player2currentResourceImage;

        UpdateStatText(characterNameText, character.name);
        if (playerImage != null) playerImage.sprite = character.avatar;
        UpdateStatText(playerHpText, player.currentHealth.ToString());
        UpdateImageColor(resourceImage, GetResourceColor(player.resourceType));
        UpdateImageFill(player1CurrentHp, player.currentHealth, player.character.baseHealth);
        UpdateImageFill(player2CurrentHp, player.currentHealth, player.character.baseHealth);

        UpdateStatPanel(player);
        UpdatePowerSlider(player);
    }

    private void UpdatePlayerStats(GamePlayer player)
    {
        TMP_Text playerHpText = player == player1 ? playerCurrent1HpText : playerCurrent2HpText;
        UpdateStatText(playerHpText, player.currentHealth.ToString());
        UpdateStatPanel(player);
        UpdatePowerSlider(player);
        UpdateImageFill(player1CurrentHp, player.currentHealth, player.character.baseHealth);
        UpdateImageFill(player2CurrentHp, player.currentHealth, player.character.baseHealth);
        UpdateImageFill(player1currentResourceImage, player.currentResource, player.character.maxResource);
        UpdateImageFill(player2currentResourceImage, player.currentResource, player.character.maxResource);
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

    private void UpdateImageFill(Image image, int currentValue, int maxValue)
    {
        if (image != null)
        {
            float fillAmount = Mathf.Clamp01((float)currentValue / maxValue);
            image.fillAmount = fillAmount;
        }
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
            rect.anchoredPosition = new Vector2(
                column * (buttonWidth + spacing) + spacing,
                -(row * (buttonHeight + spacing)) - spacing
            );

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

        int[] statValues = new int[]
        {
            player.currentAttackPower, player.currentDefPower, player.currentParryPower,
            player.currentKickDamage, player.currentHealPower, player.currentPoisons,
            player.currentBreak, player.currentResource
        };

        if (statPanel != null)
        {
            statPanel.SetActive(true);
            for (int i = 0; i < statTexts.Length; i++)
            {
                UpdateStatText(statTexts[i], statValues[i].ToString());
            }
        }
    }

    private void UpdateStatText(TMP_Text textComponent, string value)
    {
        if (textComponent != null)
            textComponent.text = value;
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

        if (player1ActionConfirmed && player2ActionConfirmed)
        {
            acceptRound.interactable = true;
            StartNewRound();

            ProcessTurn(player1, player2);
            ProcessTurn(player2, player1);

            UpdatePlayerStats(player1);
            UpdatePlayerStats(player2);

            Debug.Log("Both players have confirmed their actions.");
            roundCounter++;
            roundcountertext.text = roundCounter.ToString();
            player1ActionConfirmed = false;
            player2ActionConfirmed = false;
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

    void ProcessTurn(GamePlayer attacker, GamePlayer defender)
    {
        float damage = 0;
        attacker.currentResource -= attacker.currentPowerBar;

        switch (attacker.selectedActionButtonName)
        {
            case "attackButton":
                damage = CalculateAttackDamage(attacker, defender);
                break;
            case "defButton":
                damage = CalculateDefendDamage(attacker, defender);
                break;
            case "parryButton":
                damage = CalculateParryDamage(attacker, defender);
                break;
            case "kickButton":
                damage = CalculateKickDamage(attacker, defender);
                break;
            case "healButton":
                HealPlayer(attacker);
                break;
        }

        ApplyDamage(defender, damage);

        if (attacker.selectedActionButtonName == "healButton")
        {
            HealPlayer(attacker);
        }

        UpdatePlayerStats(attacker);
        UpdatePlayerStats(defender);
    }

    private float CalculateDamage(GamePlayer attacker, GamePlayer defender)
    {
        return attacker.selectedActionButtonName switch
        {
            "attackButton" => CalculateAttackDamage(attacker, defender),
            "defButton" => CalculateDefendDamage(attacker, defender),
            "parryButton" => CalculateParryDamage(attacker, defender),
            "kickButton" => CalculateKickDamage(attacker, defender),
            _ => 0
        };
    }

    private float CalculateAttackDamage(GamePlayer attacker, GamePlayer defender)
    {
        float damage = attacker.currentAttackPower * (attacker.currentPowerBar / 100f) + attacker.currentAttackPower;
        if (defender.selectedActionButtonName == "parryButton")
            damage /= 2;
        return damage;
    }

    private float CalculateDefendDamage(GamePlayer attacker, GamePlayer defender)
    {
        if (defender.selectedActionButtonName == "attackButton" || defender.selectedActionButtonName == "parryButton")
            return attacker.currentDefPower + (attacker.currentDefPower * attacker.currentPowerBar / 100f);
        return 0;
    }

    private float CalculateParryDamage(GamePlayer attacker, GamePlayer defender)
    {
        float damage = attacker.currentParryPower + (attacker.currentParryPower * attacker.currentPowerBar / 100f);
        if (defender.selectedActionButtonName == "attackButton")
            damage += defender.currentAttackPower / 2;
        else if (defender.selectedActionButtonName == "kickButton")
            damage += defender.currentKickDamage / 2;
        else if (defender.selectedActionButtonName == "defButton")
        {
            damage = defender.currentDefPower + (defender.currentDefPower * defender.currentPowerBar / 100f);
            ApplyDamage(attacker, damage);
            return 0;
        }
        return damage;
    }

    private float CalculateKickDamage(GamePlayer attacker, GamePlayer defender)
    {
        return attacker.currentKickDamage + (attacker.currentKickDamage * attacker.currentPowerBar / 100f);
    }

    private void ApplyDamage(GamePlayer player, float damage)
    {
        player.currentHealth = Mathf.Clamp(player.currentHealth - (int)damage, 0, player.maxHealth);
    }

    private void HealPlayer(GamePlayer player)
    {
        float healAmount = player.currentHealPower + (player.currentHealPower * player.currentPowerBar / 100f);
        player.currentHealth = Mathf.Clamp(player.currentHealth + (int)healAmount, 0, player.maxHealth);
    }
}