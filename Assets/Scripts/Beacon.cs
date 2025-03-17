using System.Collections;
using System.Collections.Generic;
using Scriptable.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

public class Beacon : MonoBehaviour
{
    [FormerlySerializedAs("m_UChannel")]
    [FormerlySerializedAs("healthChannel")]
    public UIChannelSo UIChannel;
    public GameStateChannel gameStateChannel;
    
    void Awake()
    {
        // This ensures only one instance if you use the typical Singleton pattern
        if (FindObjectsOfType<Beacon>().Length > 1) 
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    
}
