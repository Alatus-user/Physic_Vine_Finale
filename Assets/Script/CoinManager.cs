// Import Unity core functionality
using UnityEngine;
// Import Unity UI system for legacy Text support
using UnityEngine.UI;
// Import TextMeshPro for advanced text rendering
using TMPro; // For TextMeshPro support

    // Define the CoinManager class that inherits from MonoBehaviour
public class CoinManager : MonoBehaviour
{
    // Singleton instance - allows any script to access via CoinManager.Instance
    public static CoinManager Instance { get; private set; } // Stores the single instance of CoinManager

    // Section header for coin count configuration in Inspector
    [Header("Coin Count")]
    // Tooltip for the currentCoins field
    [Tooltip("Current coin count (read-only at runtime)")]
    // Private serialized field to store the current coin count (can be edited in Inspector)
    [SerializeField] private int currentCoins = 0;

    // Section header for UI references in Inspector
    [Header("UI Reference (Optional)")]
    // Tooltip for TextMeshPro text reference
    [Tooltip("Drag a TextMeshPro Text here to display coin count")]
    // Public reference to TextMeshPro Text component for displaying coins
    public TMP_Text coinText;

    // Tooltip for legacy UI Text reference
    [Tooltip("If using legacy UI Text instead, drag it here")]
    // Public reference to legacy Text component as fallback
    public Text legacyCoinText;

    // Section header for display format configuration in Inspector
    [Header("Display Format")]
    // Tooltip for text format
    [Tooltip("Text format. Use {0} as placeholder for coin number")]
    // String format template for displaying coin count (e.g., "Coins: {0}")
    public string textFormat = "{0}";

    // Property that provides public read-only access to currentCoins
    public int CurrentCoins => currentCoins;

    // Unity lifecycle method called when the script instance is being loaded
    private void Awake()
    {
        // Check if another instance already exists
        if (Instance != null && Instance != this)
        {
            // Log a warning if multiple CoinManagers are detected
            Debug.LogWarning("Multiple CoinManagers found. Destroying duplicate.");
            // Destroy the duplicate game object
            Destroy(gameObject);
            // Exit the method early
            return;
        }
        // Set this instance as the singleton
        Instance = this;
    }

    // Unity lifecycle method called before the first frame
    private void Start()
    {
        // Update the UI display with current coin count
        UpdateUI();
    }

    // Method to add coins to the total (called when coins are collected)
    /// <summary>
    /// Add coins to the count. Called by Coin.cs when collected.
    /// </summary>
    public void AddCoin(int amount = 1) // Method accepts amount parameter (default = 1 coin)
    {
        // Increase currentCoins by the specified amount
        currentCoins += amount;
        // Update the UI to reflect the new coin count
        UpdateUI();
    }

    // Method to spend/remove coins from the total
    /// <summary>
    /// Remove coins (for purchases, penalties, etc.)
    /// </summary>
    public bool SpendCoin(int amount) // Method returns true if successful, false if insufficient coins
    {
        // Check if player has enough coins
        if (currentCoins >= amount)
        {
            // Decrease currentCoins by the specified amount
            currentCoins -= amount;
            // Update the UI to reflect the new coin count
            UpdateUI();
            // Return true to indicate successful spending
            return true;
        }
        // Return false if player doesn't have enough coins
        return false;
    }

    // Method to reset coin count back to zero
    /// <summary>
    /// Reset coin count to zero
    /// </summary>
    public void ResetCoins() // Method takes no parameters
    {
        // Set currentCoins to 0
        currentCoins = 0;
        // Update the UI to display the reset value
        UpdateUI();
    }

    // Method to set coin count to a specific value (useful for loading saved games)
    /// <summary>
    /// Set coin count to specific value (e.g., loading saved game)
    /// </summary>
    public void SetCoins(int amount) // Method accepts the target coin count
    {
        // Set currentCoins to the specified amount
        currentCoins = amount;
        // Update the UI to reflect the new coin count
        UpdateUI();
    }

    // Private method that updates all UI elements displaying the coin count
    private void UpdateUI()
    {
        // Format the coin count using the textFormat template
        string displayText = string.Format(textFormat, currentCoins);

        // Check if TextMeshPro text component is assigned
        if (coinText != null)
        {
            // Set the TextMeshPro text to display the formatted coin count
            coinText.text = displayText;
        }

        // Check if legacy UI Text component is assigned
        if (legacyCoinText != null)
        {
            // Set the legacy Text to display the formatted coin count
            legacyCoinText.text = displayText;
        }
    }

    // Unity lifecycle method called when this script instance is destroyed
    private void OnDestroy()
    {
        // Check if this instance is the current singleton
        if (Instance == this)
        {
            // Clear the singleton reference to null
            Instance = null;
        }
    }
} // End of CoinManager class