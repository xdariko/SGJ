using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeycardPickup : MonoBehaviour
{
    [SerializeField] private KeycardColor color;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.1f;

    private Vector3 startPos;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // лёгкая анимация "парения"
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerKeyInventory inventory = other.GetComponentInParent<PlayerKeyInventory>();
        if (inventory == null) return;

        inventory.AddKeycard(color);
        Destroy(gameObject);
    }
}