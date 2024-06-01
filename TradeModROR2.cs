using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[BepInPlugin("com.yourname.tradingmod", "Trading Mod", "1.0.0")]
public class TradingMod : BaseUnityPlugin
{
    private bool inLunarShop = false;
    private GameObject tradingTable;
    private List<PlayerCharacterMasterController> playersAtTable = new List<PlayerCharacterMasterController>();
    private GameObject tradeUI;
    private PlayerCharacterMasterController player1;
    private PlayerCharacterMasterController player2;

    private void Awake()
    {
        Logger.LogInfo("Trading Mod loaded");
        On.RoR2.SceneDirector.Start += OnSceneStart;
    }

    private void OnSceneStart(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
    {
        orig(self);
        CheckIfInLunarShop();
    }

    private void CheckIfInLunarShop()
    {
        inLunarShop = Run.instance && Run.instance.sceneName.StartsWith("bazaar");
        if (inLunarShop)
        {
            SetupTradingTable();
        }
    }

    private void SetupTradingTable()
    {
        tradingTable = new GameObject("TradingTable");

        // Ajouter des modèles de table et de chaises
        GameObject tableModel = Instantiate(Resources.Load<GameObject>("Path/To/TableModel"));
        tableModel.transform.SetParent(tradingTable.transform);
        
        GameObject chair1 = Instantiate(Resources.Load<GameObject>("Path/To/ChairModel"));
        chair1.transform.SetParent(tradingTable.transform);
        chair1.transform.localPosition = new Vector3(1, 0, 0); // Position relative à la table

        GameObject chair2 = Instantiate(Resources.Load<GameObject>("Path/To/ChairModel"));
        chair2.transform.SetParent(tradingTable.transform);
        chair2.transform.localPosition = new Vector3(-1, 0, 0); // Position relative à la table

        // Positionner la table et les chaises dans la boutique
        tradingTable.transform.position = new Vector3(0, 0, 0); // Positionner selon vos besoins

        // Ajouter des composants d'interaction
        tradingTable.AddComponent<TradingInteraction>().Setup(this);

        CreateTradeUI();
    }

    private void CreateTradeUI()
    {
        // Créer l'UI de trading
        tradeUI = new GameObject("TradeUI");

        // Ajouter les composants UI nécessaires, par exemple des panneaux, des boutons, des textes, etc.
        Canvas canvas = tradeUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tradeUI.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        tradeUI.AddComponent<GraphicRaycaster>();

        // Créer un panneau pour afficher les objets de chaque joueur
        GameObject panel1 = CreateUIPanel("Player1ItemsPanel", new Vector2(-200, 0), tradeUI.transform);
        GameObject panel2 = CreateUIPanel("Player2ItemsPanel", new Vector2(200, 0), tradeUI.transform);

        // Créer des boutons pour chaque objet dans l'inventaire des joueurs (exemple simplifié)
        CreateItemButtons(player1, panel1.transform);
        CreateItemButtons(player2, panel2.transform);

        // Ajouter un bouton pour confirmer l'échange
        GameObject confirmButton = new GameObject("ConfirmButton");
        confirmButton.transform.SetParent(tradeUI.transform);
        Button button = confirmButton.AddComponent<Button>();
        button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
        button.GetComponentInChildren<Text>().text = "Confirm Trade";
        button.onClick.AddListener(ConfirmTrade);

        tradeUI.SetActive(false); // Masquer l'UI par défaut
    }

    private GameObject CreateUIPanel(string name, Vector2 position, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 400);
        rectTransform.anchoredPosition = position;
        panel.AddComponent<Image>().color = Color.gray;
        return panel;
    }

    private void CreateItemButtons(PlayerCharacterMasterController player, Transform parent)
    {
        foreach (var item in player.master.inventory.itemAcquisitionOrder)
        {
            GameObject buttonObject = new GameObject(item.ToString());
            buttonObject.transform.SetParent(parent);
            Button button = buttonObject.AddComponent<Button>();
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 30);
            Text buttonText = buttonObject.AddComponent<Text>();
            buttonText.text = item.ToString();
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            button.onClick.AddListener(() => SelectItem(player, item));
        }
    }

    private void SelectItem(PlayerCharacterMasterController player, ItemIndex item)
    {
        // Gérer la sélection d'item pour le trade
        Logger.LogInfo($"{player.GetDisplayName()} selected {item}");
    }

    public void StartTrade()
    {
        if (playersAtTable.Count == 2)
        {
            player1 = playersAtTable[0];
            player2 = playersAtTable[1];

            if (player1.master.money >= 1 && player2.master.money >= 1)
            {
                player1.master.money -= 1;
                player2.master.money -= 1;

                // Afficher l'interface d'échange
                tradeUI.SetActive(true);
            }
            else
            {
                Logger.LogInfo("Not enough Lunar Coins to trade");
            }
        }
    }

    private void ConfirmTrade()
    {
        // Implémenter la logique pour confirmer et exécuter l'échange
        Logger.LogInfo($"{player1.GetDisplayName()} and {player2.GetDisplayName()} have confirmed the trade");

        // Fermer l'interface d'échange
        tradeUI.SetActive(false);
    }
}

public class TradingInteraction : MonoBehaviour
{
    private TradingMod tradingMod;
    private bool isPlayerNearby = false;
    private PlayerCharacterMasterController nearbyPlayer;

    public void Setup(TradingMod mod)
    {
        tradingMod = mod;
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E)) // Supposons que 'E' est la touche d'interaction
        {
            if (!tradingMod.playersAtTable.Contains(nearbyPlayer))
            {
                tradingMod.playersAtTable.Add(nearbyPlayer);
                if (tradingMod.playersAtTable.Count == 2)
                {
                    tradingMod.StartTrade();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerNearby = true;
            nearbyPlayer = other.gameObject.GetComponent<PlayerCharacterMasterController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerNearby = false;
            tradingMod.playersAtTable.Remove(nearbyPlayer);
            nearbyPlayer = null;
        }
    }
}
