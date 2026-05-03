using UnityEngine;

public class FirstMutationTrigger : MonoBehaviour
{
    private bool wasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (wasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        wasTriggered = true;

        G.main.PlayFirstMutationStory();
    }
}