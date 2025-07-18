using System;
using UnityEngine;

public abstract class Reload : MonoBehaviour
{
    public event Action OnReloadedEvent;
    public abstract float ReloadPercentage { get; }
    public abstract void SpellWasUsed();
    public abstract bool IsReady();
    protected void Trigger()
    {
        OnReloadedEvent?.Invoke();
    }
    
}