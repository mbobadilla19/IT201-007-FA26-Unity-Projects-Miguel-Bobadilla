using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class SeedWallet : MonoBehaviour
{
    public int Seeds { get; private set; }
    public const int LifeCost = 3;
    private PlayerController player;
    public bool CanBuyLife => player != null && !player.HasWon &&
        player.LivesRemaining > 0 && player.LivesRemaining < Mathf.Max(1, player.startingLives) &&
        Seeds >= LifeCost;

    void Awake() { player = GetComponent<PlayerController>(); }

    public void AddSeed() { Seeds++; }

    void Update()
    {
        if (Time.timeScale <= 0f || !CanBuyLife) return;
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame &&
            player.TryRestoreLife())
            Seeds -= LifeCost;
    }
}
