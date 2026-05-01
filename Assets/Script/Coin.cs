using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Value")]
    [Tooltip("How many points this coin is worth")]
    public int coinValue = 1;

    [Header("Audio")]
    [Tooltip("Sound to play when collected")]
    public AudioClip pickupSound;

    [Tooltip("Volume of pickup sound (0 to 1)")]
    [Range(0f, 1f)]
    public float pickupVolume = 0.7f;

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Prevent double-pickup if multiple colliders trigger at same time
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }

    private void CollectCoin()
    {
        isCollected = true;

        // Notify the manager (if it exists)
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoin(coinValue);
        }

        // Play pickup sound
        if (pickupSound != null)
        {
            // Play sound at coin position so it doesn't get cut off when coin is destroyed
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
        }

        // Destroy the coin
        Destroy(gameObject);
    }

    // Visualize trigger area in editor
    private void OnDrawGizmos()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0.84f, 0f, 0.3f); // Gold color, transparent
            Gizmos.DrawSphere(transform.position, col.radius);
        }
    }
}