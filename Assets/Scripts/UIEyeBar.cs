using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable.Scripts;
using UnityEngine;

public class UIEyeBar : MonoBehaviour
{
    private int maxHealth; 
    private int currentHealth;
    [SerializeField] private GameObject EyePrefab;
    private UIChannelSo m_UIChannel;
    List<UIEyeScript> eyes = new List<UIEyeScript>();
    private void Awake()
    {
        m_UIChannel = FindObjectOfType<Beacon>().UIChannel;
        m_UIChannel.OnChangeHealth += UpdateHUD;
    }

    void CreateEye(bool value)
    {
        GameObject newEye = Instantiate(EyePrefab);
        newEye.transform.SetParent(transform);
        UIEyeScript eyeScript = newEye.GetComponent<UIEyeScript>();
        eyeScript.setImage(value);
        eyes.Add(eyeScript);
    }
    private void UpdateHUD(int health)
    {
        ClearEyes();
        if(maxHealth == 0)
        {
            maxHealth = health;
            currentHealth = health;
        }

        for(int i = 0; i < maxHealth; i++)
        {
            if( i<= health-1)
                CreateEye(true);
            else
            {
                CreateEye(false);
            }
        }
    }

    private void ClearEyes()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        eyes = new List<UIEyeScript>();
    }
}
