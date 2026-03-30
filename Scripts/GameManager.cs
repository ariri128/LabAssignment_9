using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public ResourceManager resourceManager;

    public TextMeshProUGUI revenueText;
    public TextMeshProUGUI reputationText;
    public TextMeshProUGUI revenuePerSecondText;
    public TextMeshProUGUI juniorDevCountText;
    public TextMeshProUGUI releasedGamesCountText;

    public TextMeshProUGUI engineUpgradeStateText;
    public TextMeshProUGUI assetLibraryUpgradeStateText;
    public TextMeshProUGUI buildOptimizationUpgradeStateText;

    public float developGameClickValue = 10f;

    public int juniorDeveloperCount = 0;
    public int releasedGameCount = 0;
    public float juniorDeveloperIncome = 2f;
    public float releasedGameIncome = 5f;

    public float juniorDeveloperCost = 50f;
    public float releasedGameCost = 100f;

    public List<Upgrade> upgrades = new List<Upgrade>();

    private float globalRevenueMultiplier = 1f;

    private void Start()
    {
        if (resourceManager == null)
        {
            resourceManager = GetComponent<ResourceManager>();
        }

        upgrades.Add(new Upgrade(
            "Better Engine",
            75f,
            new UpgradeEffect(1.25f, ResourceType.RevenuePerSecond),
            1,
            UpgradeState.Available
        ));

        upgrades.Add(new Upgrade(
            "Asset Library",
            150f,
            new UpgradeEffect(1.5f, ResourceType.RevenuePerSecond),
            2,
            UpgradeState.Locked
        ));

        upgrades.Add(new Upgrade(
            "Build Optimization",
            250f,
            new UpgradeEffect(2f, ResourceType.RevenuePerSecond),
            3,
            UpgradeState.Locked
        ));

        UpdateRevenuePerSecond();
        UpdateUpgradeAvailability();
        UpdateUI();
    }

    private void Update()
    {
        RunPassiveIncome();
        UpdateUI();
    }

    private void RunPassiveIncome()
    {
        float totalPassiveIncome = 0f;

        totalPassiveIncome += juniorDeveloperCount * juniorDeveloperIncome;
        totalPassiveIncome += releasedGameCount * releasedGameIncome;
        totalPassiveIncome *= globalRevenueMultiplier;

        resourceManager.AddResource(ResourceType.Revenue, totalPassiveIncome * Time.deltaTime);
    }

    private void UpdateRevenuePerSecond()
    {
        float totalPassiveIncome = 0f;

        totalPassiveIncome += juniorDeveloperCount * juniorDeveloperIncome;
        totalPassiveIncome += releasedGameCount * releasedGameIncome;
        totalPassiveIncome *= globalRevenueMultiplier;

        resourceManager.SetResource(ResourceType.RevenuePerSecond, totalPassiveIncome);
    }

    private void UpdateUpgradeAvailability()
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i].state == UpgradeState.Purchased)
            {
                continue;
            }

            if (i == 0)
            {
                upgrades[i].state = UpgradeState.Available;
            }
            else
            {
                if (upgrades[i - 1].state == UpgradeState.Purchased)
                {
                    upgrades[i].state = UpgradeState.Available;
                }
            }
        }
    }

    private void UpdateUI()
    {
        revenueText.text = "Revenue: $" + resourceManager.GetResource(ResourceType.Revenue).ToString("F1");
        reputationText.text = "Reputation: " + resourceManager.GetResource(ResourceType.Reputation).ToString("F1");
        revenuePerSecondText.text = "Revenue / Sec: $" + resourceManager.GetResource(ResourceType.RevenuePerSecond).ToString("F1");
        juniorDevCountText.text = "Junior Developers: " + juniorDeveloperCount;
        releasedGamesCountText.text = "Released Games: " + releasedGameCount;

        engineUpgradeStateText.text = upgrades[0].state.ToString();
        assetLibraryUpgradeStateText.text = upgrades[1].state.ToString();
        buildOptimizationUpgradeStateText.text = upgrades[2].state.ToString();
    }

    public void DevelopGame()
    {
        float clickAmount = developGameClickValue * globalRevenueMultiplier;
        resourceManager.AddResource(ResourceType.Revenue, clickAmount);
        resourceManager.AddResource(ResourceType.Reputation, 1f);
        UpdateUI();
    }

    public void HireJuniorDeveloper()
    {
        if (resourceManager.SpendResource(ResourceType.Revenue, juniorDeveloperCost))
        {
            juniorDeveloperCount++;
            juniorDeveloperCost *= 1.15f;
            UpdateRevenuePerSecond();
            UpdateUI();
        }
    }

    public void ReleaseSmallGame()
    {
        if (resourceManager.SpendResource(ResourceType.Revenue, releasedGameCost))
        {
            releasedGameCount++;
            releasedGameCost *= 1.15f;
            resourceManager.AddResource(ResourceType.Reputation, 5f);
            UpdateRevenuePerSecond();
            UpdateUI();
        }
    }

    public void BuyBetterEngine()
    {
        BuyUpgrade("Better Engine");
    }

    public void BuyAssetLibrary()
    {
        BuyUpgrade("Asset Library");
    }

    public void BuyBuildOptimization()
    {
        BuyUpgrade("Build Optimization");
    }

    private void BuyUpgrade(string upgradeName)
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i].upgradeName == upgradeName)
            {
                if (upgrades[i].state != UpgradeState.Available)
                {
                    return;
                }

                if (resourceManager.SpendResource(ResourceType.Revenue, upgrades[i].cost))
                {
                    upgrades[i].state = UpgradeState.Purchased;
                    ApplyUpgradeEffect(upgrades[i].effect);
                    UpdateRevenuePerSecond();
                    UpdateUpgradeAvailability();
                    UpdateUI();
                }

                return;
            }
        }
    }

    private void ApplyUpgradeEffect(UpgradeEffect effect)
    {
        if (effect.targetResourceType == ResourceType.RevenuePerSecond)
        {
            globalRevenueMultiplier *= effect.multiplier;
        }
    }
}
