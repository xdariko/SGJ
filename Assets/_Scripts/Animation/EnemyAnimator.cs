using System.Collections;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private static readonly int StateHash = Animator.StringToHash("State");
    private static readonly int AttackStateHash = Animator.StringToHash("Attack");
    private static readonly int DeathStateHash = Animator.StringToHash("Death");

    [SerializeField] private Animator animator;

    private EnemyAnimState _currentState = (EnemyAnimState)(-1);

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void PlayState(EnemyAnimState state)
    {
        if (animator == null)
            return;

        if (_currentState == state && state != EnemyAnimState.Attack)
            return;

        _currentState = state;
        animator.SetInteger(StateHash, (int)state);
    }

    public void PlayAttack()
    {
        PlayAttack(restartFromBeginning: true);
    }

    public void PlayAttack(bool restartFromBeginning)
    {
        if (animator == null)
            return;

        _currentState = EnemyAnimState.Attack;
        animator.SetInteger(StateHash, (int)EnemyAnimState.Attack);

        if (!restartFromBeginning)
            return;

        if (animator.HasState(0, AttackStateHash))
            animator.Play(AttackStateHash, 0, 0f);

        animator.Update(0f);
    }

    public IEnumerator PlayAttackAndWait(float fallbackDuration)
    {
        PlayAttack(restartFromBeginning: true);
        yield return WaitForAttackAnimation(fallbackDuration);
    }

    public IEnumerator WaitForAttackAnimation(float fallbackDuration)
    {
        if (animator == null)
        {
            yield return new WaitForSeconds(Mathf.Max(0.01f, fallbackDuration));
            yield break;
        }

        float fallback = Mathf.Max(0.01f, GetAttackClipDuration(fallbackDuration));
        float enterTimeout = Mathf.Min(0.25f, fallback);
        float enterTimer = 0f;

        // Give Animator a chance to actually enter Attack after State parameter / Play().
        while (!IsCurrentState(AttackStateHash) && enterTimer < enterTimeout)
        {
            enterTimer += Time.deltaTime;
            yield return null;
        }

        if (!IsCurrentState(AttackStateHash))
        {
            // Fallback for controllers where the state is not literally named "Attack".
            yield return new WaitForSeconds(fallback);
            yield break;
        }

        // Wait until the visible Attack state has completed one full play-through.
        while (IsCurrentState(AttackStateHash))
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

            if (!animator.IsInTransition(0) && info.normalizedTime >= 1f)
                break;

            yield return null;
        }
    }

    private bool IsCurrentState(int stateHash)
    {
        if (animator == null)
            return false;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return info.shortNameHash == stateHash;
    }

    private float GetAttackClipDuration(float fallbackDuration)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return Mathf.Max(0.01f, fallbackDuration);

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        float bestLength = 0f;

        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            if (clip == null)
                continue;

            if (clip.name.ToLowerInvariant().Contains("attack"))
                bestLength = Mathf.Max(bestLength, clip.length);
        }

        return Mathf.Max(0.01f, bestLength > 0f ? bestLength : fallbackDuration);
    }

    public void PlayDeath()
    {
        if (animator == null)
            return;

        _currentState = EnemyAnimState.Death;
        animator.SetInteger(StateHash, (int)EnemyAnimState.Death);

        if (animator.HasState(0, DeathStateHash))
            animator.Play(DeathStateHash, 0, 0f);

        animator.Update(0f);
    }

    public IEnumerator PlayDeathAndWait(float fallbackDuration)
    {
        PlayDeath();
        yield return WaitForDeathAnimation(fallbackDuration);
    }

    public IEnumerator WaitForDeathAnimation(float fallbackDuration)
    {
        float duration = GetDeathClipDuration(fallbackDuration);
        yield return new WaitForSeconds(duration);
    }

    private float GetDeathClipDuration(float fallbackDuration)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return Mathf.Max(0.01f, fallbackDuration);

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        float bestLength = 0f;

        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            if (clip == null)
                continue;

            if (clip.name.ToLowerInvariant().Contains("death"))
                bestLength = Mathf.Max(bestLength, clip.length);
        }

        return Mathf.Max(0.01f, bestLength > 0f ? bestLength : fallbackDuration);
    }
}
