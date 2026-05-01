using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }


    [SerializeField] private int currentCoins = 0;


    public TMP_Text coinText;


    public Text legacyCoinText;


    public string textFormat = "{0}";

    public int CurrentCoins => currentCoins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple CoinManagers found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }


    public void AddCoin(int amount = 1)
    {
        currentCoins += amount;
        UpdateUI();
    }

    public bool SpendCoin(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }


    public void ResetCoins()
    {
        currentCoins = 0;
        UpdateUI();
    }


    public void SetCoins(int amount)
    {
        currentCoins = amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        string displayText = string.Format(textFormat, currentCoins);

        if (coinText != null)
        {
            coinText.text = displayText;
        }

        if (legacyCoinText != null)
        {
            legacyCoinText.text = displayText;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}