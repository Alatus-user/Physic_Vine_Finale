using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro support

public class CoinManager : MonoBehaviour
{
    // Singleton instance - allows any script to access via CoinManager.Instance
    public static CoinManager Instance { get; private set; }

    [Header("Coin Count")]
    [Tooltip("Current coin count (read-only at runtime)")]
    [SerializeField] private int currentCoins = 0;

    [Header("UI Reference (Optional)")]
    [Tooltip("Drag a TextMeshPro Text here to display coin count")]
    public TMP_Text coinText;

    [Tooltip("If using legacy UI Text instead, drag it here")]
    public Text legacyCoinText;

    [Header("Display Format")]
    [Tooltip("Text format. Use {0} as placeholder for coin number")]
    public string textFormat = "Coins: {0}";

    // Public read-only access
    public int CurrentCoins => currentCoins;

    private void Awake()
    {
        // Singleton setup
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

    /// <summary>
    /// Add coins to the count. Called by Coin.cs when collected.
    /// </summary>
    public void AddCoin(int amount = 1)
    {
        currentCoins += amount;
        UpdateUI();
    }

    /// <summary>
    /// Remove coins (for purchases, penalties, etc.)
    /// </summary>
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

    /// <summary>
    /// Reset coin count to zero
    /// </summary>
    public void ResetCoins()
    {
        currentCoins = 0;
        UpdateUI();
    }

    /// <summary>
    /// Set coin count to specific value (e.g., loading saved game)
    /// </summary>
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