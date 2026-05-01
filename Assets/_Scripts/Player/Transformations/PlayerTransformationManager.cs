using System.Collections.Generic;
using UnityEngine;

public class PlayerTransformationManager : MonoBehaviour
{
    [SerializeField] private List<PlayerTransformationBase> availableTransformations;
    [SerializeField] private int currentTransformationIndex = 0;

    private Player player;
    private Dictionary<string, IPlayerTransformation> transformationMap;

    public IPlayerTransformation CurrentTransformation => availableTransformations.Count > 0 ?
        availableTransformations[currentTransformationIndex] : null;

    public event System.Action<IPlayerTransformation> OnTransformationActivated;
    public event System.Action<IPlayerTransformation> OnTransformationDeactivated;
    public event System.Action<IPlayerTransformation> OnTransformationChanged;

    private void Awake()
    {
        player = GetComponent<Player>();
        InitializeTransformationMap();
    }

    private void Update()
    {
        if (availableTransformations.Count == 0) return;

        // Update all transformations (for cooldowns, etc.)
        foreach (var transformation in availableTransformations)
        {
            transformation.UpdateTransformation(player);
        }

        // Handle input for transformation activation
        if (Input.GetKeyDown(KeyCode.Q) && CurrentTransformation != null)
        {
            TryActivateCurrentTransformation();
        }

        // Handle input for switching transformations
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchToNextTransformation();
        }
    }

    private void InitializeTransformationMap()
    {
        transformationMap = new Dictionary<string, IPlayerTransformation>();
        foreach (var transformation in availableTransformations)
        {
            transformationMap[transformation.TransformationName] = transformation;
        }
    }

    public void TryActivateCurrentTransformation()
    {
        if (CurrentTransformation != null && CurrentTransformation.CanActivate(player))
        {
            ActivateTransformation(CurrentTransformation);
        }
    }

    public void ActivateTransformation(IPlayerTransformation transformation)
    {
        if (transformationMap.TryGetValue(transformation.TransformationName, out var foundTransformation))
        {
            foundTransformation.Activate(player);
            OnTransformationActivated?.Invoke(foundTransformation);
        }
    }

    public void DeactivateCurrentTransformation()
    {
        if (CurrentTransformation != null && transformationMap.TryGetValue(CurrentTransformation.TransformationName, out var transformation))
        {
            transformation.Deactivate(player);
            OnTransformationDeactivated?.Invoke(transformation);
        }
    }

    public void SwitchToNextTransformation()
    {
        if (availableTransformations.Count == 0) return;

        currentTransformationIndex = (currentTransformationIndex + 1) % availableTransformations.Count;
        OnTransformationChanged?.Invoke(CurrentTransformation);
    }

    public void SwitchToPreviousTransformation()
    {
        if (availableTransformations.Count == 0) return;

        currentTransformationIndex--;
        if (currentTransformationIndex < 0)
        {
            currentTransformationIndex = availableTransformations.Count - 1;
        }
        OnTransformationChanged?.Invoke(CurrentTransformation);
    }

    public IPlayerTransformation GetTransformationByName(string name)
    {
        transformationMap.TryGetValue(name, out var transformation);
        return transformation;
    }

    public bool HasTransformation(string name)
    {
        return transformationMap.ContainsKey(name);
    }

    public void AddTransformation(PlayerTransformationBase transformation)
    {
        if (!availableTransformations.Contains(transformation))
        {
            availableTransformations.Add(transformation);
            transformationMap[transformation.TransformationName] = transformation;
            OnTransformationChanged?.Invoke(CurrentTransformation);
        }
    }

    public void RemoveTransformation(string transformationName)
    {
        if (transformationMap.TryGetValue(transformationName, out var transformation))
        {
            if (transformation == CurrentTransformation)
            {
                DeactivateCurrentTransformation();
            }

            availableTransformations.Remove((PlayerTransformationBase)transformation);
            transformationMap.Remove(transformationName);
            OnTransformationChanged?.Invoke(CurrentTransformation);
        }
    }
}