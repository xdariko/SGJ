using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInvincibility : MonoBehaviour
{
    private Player player;
    private bool isInvincible = false;

    private void Awake()
    {
        player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("[PlayerInvincibility] Player component not found!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        // Проверяем нажатие G
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            ToggleInvincibility();
        }
    }

    private void ToggleInvincibility()
    {
        isInvincible = !isInvincible;
        player.SetInvincible(isInvincible);
        
        Debug.LogWarning($"[PlayerInvincibility] Player is now {(isInvincible ? "INVINCIBLE" : "VULNERABLE")}");
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }
}
