using System;
using UnityEngine;

namespace Scriptable.Scripts
{
    [CreateAssetMenu(fileName = "Health channel", menuName = "Channels/Health", order = 0)]
    public class HealthChannelSo : ScriptableObject
    {
        public event Action<int> OnChangeHealth;
        public event Action<int> GetMaxHealth;
        public void ChangeHealth(int health)
        {
            OnChangeHealth?.Invoke(health);
        }
    }
}
