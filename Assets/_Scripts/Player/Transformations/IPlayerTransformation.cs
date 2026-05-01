using UnityEngine;

public interface IPlayerTransformation
{
    string TransformationName { get; }
    Sprite TransformationIcon { get; }
    string Description { get; }
    float Cooldown { get; }

    void Activate(Player player);
    void Deactivate(Player player);
    void UpdateTransformation(Player player);
    bool CanActivate(Player player);
}