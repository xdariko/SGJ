using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeycardPanelUI : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayerKeyInventory inventory;
    [SerializeField] private GameObject panelRoot;

    [Header("Иконки")]
    [SerializeField] private Image redIcon;
    [SerializeField] private Image violetIcon;
    [SerializeField] private Image greenIcon;
    [SerializeField] private Image yellowIcon;

    [Header("Настройки")]
    [SerializeField] private Key showKey = Key.Tab;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = FindObjectOfType<PlayerKeyInventory>();
        }
    }

    private void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        RefreshIcons();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            Debug.LogError("Keyboard.current == null!");
            return;
        }

        bool showPanel = keyboard[showKey].isPressed;

        Debug.Log("Tab зажат: " + showPanel);

        if (panelRoot == null)
        {
            Debug.LogError("panelRoot не назначен!");
            return;
        }

        panelRoot.SetActive(showPanel);

        if (showPanel)
            RefreshIcons();
    }

    private void RefreshIcons()
    {
        if (inventory == null)
            return;

        SetIconVisible(redIcon, inventory.HasKeycard(KeycardColor.Red));
        SetIconVisible(violetIcon, inventory.HasKeycard(KeycardColor.Violet));
        SetIconVisible(greenIcon, inventory.HasKeycard(KeycardColor.Green));
        SetIconVisible(yellowIcon, inventory.HasKeycard(KeycardColor.Yellow));
    }

    private void SetIconVisible(Image icon, bool isVisible)
    {
        if (icon != null)
            icon.gameObject.SetActive(isVisible);
    }
}