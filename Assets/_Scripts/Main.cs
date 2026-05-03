using UnityEngine;
using UnityEngine.InputSystem;

public class Main : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Animator playerAnimator;

    [Header("Animator Controllers")]
    [SerializeField] private RuntimeAnimatorController firstMutationController;
    [SerializeField] private RuntimeAnimatorController secondMutationController;
    [SerializeField] private RuntimeAnimatorController thirdMutationController;

    [Header("Passage Blockers")]
    [SerializeField] private Collider2D blockerAfterBoss1;
    [SerializeField] private Collider2D blockerAfterBoss2;

    private bool firstStoryPlayed;
    private bool boss1StoryPlayed;
    private bool boss2StoryPlayed;
    private bool finalStoryPlayed;

    private void Awake()
    {
        G.main = this;
    }

    private void Start()
    {
        G.storyPanel?.PlaySequence("intro_01");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        SetPause(!G.IsPaused);
    }

    private void SetPause(bool paused)
    {
        G.IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (G.ui != null)
            G.ui.SetPausePanel(paused);
    }

    public void ResumeGame()
    {
        SetPause(false);
    }

    // 1. ТРИГГЕР ПОСЛЕ НАЧАЛА ИГРЫ
    // История + первая способность + первый аниматор
    public void PlayFirstMutationStory()
    {
        if (firstStoryPlayed)
            return;

        firstStoryPlayed = true;

        G.storyPanel?.PlaySequence("metamorphosis_01", () =>
        {
            UnlockFirstAbility();
            ChangePlayerAnimator(firstMutationController);
        });
    }

    // 2. ПОСЛЕ ПЕРВОГО БОССА
    // История + вторая способность + второй аниматор + открыть проход
    public void OnBoss1Killed()
    {
        if (boss1StoryPlayed)
            return;

        boss1StoryPlayed = true;

        G.storyPanel?.PlaySequence("metamorphosis_02", () =>
        {
            UnlockSecondAbility();
            ChangePlayerAnimator(secondMutationController);
            OpenPassage(blockerAfterBoss1);
        });
    }

    // 3. ПОСЛЕ ВТОРОГО БОССА
    // История + третья способность + третий аниматор + открыть проход
    public void OnBoss2Killed()
    {
        if (boss2StoryPlayed)
            return;

        boss2StoryPlayed = true;

        G.storyPanel?.PlaySequence("metamorphosis_03", () =>
        {
            UnlockThirdAbility();
            ChangePlayerAnimator(thirdMutationController);
            OpenPassage(blockerAfterBoss2);
        });
    }

    // 4. ПОСЛЕ ТРЕТЬЕГО БОССА
    // Финальная история + конец игры
    public void OnBoss3Killed()
    {
        if (finalStoryPlayed)
            return;

        finalStoryPlayed = true;

        G.storyPanel?.PlaySequence("final_01", () =>
        {
            EndGame();
        });
    }

    private void UnlockFirstAbility()
    {
        // ТУТ ПИШЕШЬ ОТКРЫТИЕ ПЕРВОЙ СПОСОБНОСТИ
        // Например:
        // G.player.GetComponent<PlayerSlimeShot>().enabled = true;

        Debug.Log("First ability unlocked");
    }

    private void UnlockSecondAbility()
    {
        // ТУТ ПИШЕШЬ ОТКРЫТИЕ ВТОРОЙ СПОСОБНОСТИ
        // Например:
        // G.player.GetComponent<PlayerHook>().enabled = true;

        Debug.Log("Second ability unlocked");
    }

    private void UnlockThirdAbility()
    {
        // ТУТ ПИШЕШЬ ОТКРЫТИЕ ТРЕТЬЕЙ СПОСОБНОСТИ
        // Например:
        // G.player.GetComponent<PlayerAreaAttack>().enabled = true;

        Debug.Log("Third ability unlocked");
    }

    private void ChangePlayerAnimator(RuntimeAnimatorController controller)
    {
        if (playerAnimator == null)
        {
            Debug.LogWarning("Player Animator is not assigned.");
            return;
        }

        if (controller == null)
        {
            Debug.LogWarning("New Animator Controller is not assigned.");
            return;
        }

        playerAnimator.runtimeAnimatorController = controller;
    }

    private void OpenPassage(Collider2D blocker)
    {
        if (blocker != null)
            blocker.enabled = false;
    }

    private void EndGame()
    {
        Debug.Log("Game ended");

        // ТУТ МОЖНО ПОКАЗАТЬ ПАНЕЛЬ КОНЦА ИГРЫ
        // G.ui.ShowEndPanel();
    }
}