using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour
{
    [Header("Настройки двери")]
    [SerializeField] private KeycardColor requiredColor;
    [SerializeField] private bool openAutomatically = true;
    [SerializeField] private bool consumeKeycard = false;
    [SerializeField] private float openDelay = 0f;

    [Header("Кнопка взаимодействия")]
    [SerializeField] private Key interactKey = Key.E;

    [Header("Визуал")]
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openVisual;

    [Header("Коллайдеры")]
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private Collider2D triggerCollider;

    [Header("Звуки (опционально)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lockedSound;

    private bool isOpen = false;
    private PlayerKeyInventory playerInRange;

    private void Start()
    {
        ApplyVisuals();
    }

    private void Update()
    {
        if (isOpen)
            return;

        if (openAutomatically)
            return;

        if (playerInRange == null)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[interactKey].wasPressedThisFrame)
        {
            TryOpen(playerInRange);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerKeyInventory inventory = other.GetComponentInParent<PlayerKeyInventory>();
        if (inventory == null) return;

        if (openAutomatically && !isOpen)
        {
            TryOpen(inventory);
        }
        else
        {
            playerInRange = inventory;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerKeyInventory inventory = other.GetComponentInParent<PlayerKeyInventory>();
        if (inventory != null && inventory == playerInRange)
        {
            playerInRange = null;
        }
    }

    public void TryOpen(PlayerKeyInventory inventory)
    {
        if (isOpen) return;

        if (!inventory.HasKeycard(requiredColor))
        {
            Debug.Log("Дверь заблокирована. Нужна карта: " + requiredColor);

            if (lockedSound != null)
                AudioSource.PlayClipAtPoint(lockedSound, transform.position);

            return;
        }

        if (consumeKeycard)
            inventory.RemoveKeycard(requiredColor);

        if (openDelay > 0)
            Invoke(nameof(Open), openDelay);
        else
            Open();
    }

    private void Open()
    {
        isOpen = true;
        ApplyVisuals();

        if (openSound != null)
            AudioSource.PlayClipAtPoint(openSound, transform.position);

        Debug.Log("Дверь открыта: " + requiredColor);
    }

    private void ApplyVisuals()
    {
        if (closedVisual != null)
            closedVisual.SetActive(!isOpen);

        if (openVisual != null)
            openVisual.SetActive(isOpen);

        if (blockingCollider != null)
            blockingCollider.enabled = !isOpen;
    }
}