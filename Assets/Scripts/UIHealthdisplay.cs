using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable.Scripts;
using UnityEngine;
using TMPro;

public class UIHealthdisplay : MonoBehaviour
{
    private TextMeshProUGUI healthText;
    
    
    private UIChannelSo m_UIChannel;
    private int maxHealth;
    private int currentHealth;
    [SerializeField]private Sprite eyeSprite;
    [SerializeField]private Sprite deadeyeSprite;
    
    void Awake()
    {
        m_UIChannel = FindObjectOfType<Beacon>().UIChannel;
        m_UIChannel.OnChangeHealth += updateText;
        healthText = GetComponent<TextMeshProUGUI>();
    }
    

    void updateText(int health)
    {
        currentHealth = health;
        if(healthText != null)
        {
            healthText.text = $"{health}";
        }
    }

}
