using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyInventory : MonoBehaviour
{
    private HashSet<KeycardColor> keycards = new HashSet<KeycardColor>();

    public bool HasKeycard(KeycardColor color)
    {
        return keycards.Contains(color);
    }

    public void AddKeycard(KeycardColor color)
    {
        if (keycards.Add(color))
        {
            Debug.Log("Подобрана ключ-карта: " + color);
        }
    }

    public bool RemoveKeycard(KeycardColor color)
    {
        return keycards.Remove(color);
    }
}