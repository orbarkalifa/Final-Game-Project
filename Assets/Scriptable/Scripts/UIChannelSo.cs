using System;
using UnityEngine;

namespace Scriptable.Scripts
{
    [CreateAssetMenu(fileName = "Health channel", menuName = "Channels/Health", order = 0)]
    public class UIChannelSo : ScriptableObject
    {
        public event Action<int> OnChangeHealth;
        public void ChangeHealth(int health)
        {
            OnChangeHealth?.Invoke(health);
        }
    }
}
