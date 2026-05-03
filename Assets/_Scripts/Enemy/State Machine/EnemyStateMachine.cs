using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine
{
    public EnemyState CurrentEnemyState { get; set; }
    private EnemyState _previousState;
    private EnemyState _specialState;

    public bool IsInSpecialState => _specialState != null;

    public void Initialize(EnemyState startState)
    {
        Debug.Log($"[StateMachine] Initialize with state: {startState?.GetType().Name}");
        CurrentEnemyState = startState;
        CurrentEnemyState.EnterState();
    }

    public void ChangeState(EnemyState newState)
    {
        Debug.Log($"[StateMachine] ChangeState: {CurrentEnemyState?.GetType().Name} -> {newState?.GetType().Name}");
        CurrentEnemyState?.ExitState();
        CurrentEnemyState = newState;
        CurrentEnemyState?.EnterState();
        _previousState = newState;
    }

    public void PushSpecialState(EnemyState specialState)
    {
        if (_specialState != null) return;

        _previousState = CurrentEnemyState;
        _specialState = specialState;

        CurrentEnemyState.ExitState();
        CurrentEnemyState = specialState;
        specialState.EnterState();
    }

    public void PopSpecialState()
    {
        if (_specialState == null) return;

        CurrentEnemyState.ExitState();
        CurrentEnemyState = _previousState;
        CurrentEnemyState.EnterState();

        _previousState = null;
        _specialState = null;
    }
}
