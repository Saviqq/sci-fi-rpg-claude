using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int MaxHealth;

    public int Current { get; private set; }
    public bool IsAlive => Current > 0;

    public Action<int> OnChange;
    public Action OnDeath;

    void Awake()
    {
        Initialize(MaxHealth);
    }

    public void TakeDamage(int amount)
    {
        Current = Mathf.Max(0, Current - amount);
        OnChange?.Invoke(Current);
        if (Current <= 0) OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (IsAlive)
        {
            Current = Mathf.Min(Current + amount, MaxHealth);
            OnChange?.Invoke(Current);
        }
    }

    public void Initialize(int max)
    {
        Current = max;
    }
}
