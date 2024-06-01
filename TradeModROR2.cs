using BepInEx;
using RoR2;
using UnityEngine;
using System.Collections.Generic;

[BepInPlugin("com.yourname.tradingmod", "Trading Mod", "1.0.0")]
public class TradingMod : BaseUnityPlugin
{
    private bool inLunarShop = false;
    private GameObject tradingTable;
    private List<PlayerCharacterMasterController> playersAtTable = new List<PlayerCharacterMasterController>();

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
        chair1.transform.position = new Vector3(1, 0, 0); // Position relative à la table

        GameObject chair2 = Instantiate(Resources.Load<GameObject>("Path/To/ChairModel"));
        chair2.transform.SetParent(tradingTable.transform);
        chair2.transform.position = new Vector3(-1, 0, 0); // Position relative à la table

        // Positionner la table et les chaises dans la boutique
        tradingTable.transform.position = new Vector3(0, 0, 0); // Positionner selon vos besoins

        // Ajouter des composants d'interaction
        chair1.AddComponent<TradingInteraction>().Setup(this, 1);
        chair2.AddComponent<TradingInteraction>().Setup(this, 2);
    }

    private void Update()
    {
        if (inLunarShop)
        {
            // Gérer les interactions avec la table et les chaises pour initier l'échange
        }
    }

    public void StartTrade()
    {
        if (playersAtTable.Count == 2)
        {
            PlayerCharacterMasterController player1 = playersAtTable[0];
            PlayerCharacterMasterController player2 = playersAtTable[1];

            if (player1.master.money >= 2 && player2.master.money >= 2)
            {
                player1.master.money -= 2;
                player2.master.money -= 2;

                // Ouvrez l'interface d'échange et permettez l'échange
                OpenTradeUI(player1, player2);
            }
            else
            {
                Logger.LogInfo("Not enough Lunar Coins to trade");
            }
        }
    }

    private void OpenTradeUI(PlayerCharacterMasterController player1, PlayerCharacterMasterController player2)
    {
        // Implémenter l'interface utilisateur et la logique d'échange
        Logger.LogInfo($"{player1.GetDisplayName()} and {player2.GetDisplayName()} can now trade items");
    }
}

public class TradingInteraction : MonoBehaviour
{
    private TradingMod tradingMod;
    private int chairID;

    public void Setup(TradingMod mod, int id)
    {
        tradingMod = mod;
        chairID = id;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerCharacterMasterController player = other.gameObject.GetComponent<PlayerCharacterMasterController>();
            if (!tradingMod.playersAtTable.Contains(player))
            {
                tradingMod.playersAtTable.Add(player);
                if (tradingMod.playersAtTable.Count == 2)
                {
                    tradingMod.StartTrade();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerCharacterMasterController player = other.gameObject.GetComponent<PlayerCharacterMasterController>();
            tradingMod.playersAtTable.Remove(player);
        }
    }
}