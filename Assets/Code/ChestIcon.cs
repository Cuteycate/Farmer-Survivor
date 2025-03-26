using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChestIcon
{
    public GameObject icon; // The ChestIcon GameObject
    public List<GameObject> goldBursts; // List of associated gold bursts

    public void Activate()
    {
        if (icon != null)
        {
            icon.SetActive(true);
        }
    }
    public void ActivateGoldBursts()
    {
        foreach (var goldBurst in goldBursts)
        {
            if (goldBurst != null)
            {
                goldBurst.SetActive(true);
                var particleSystem = goldBurst.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }
            }
        }
    }
    public void Deactivate()
    {
        if (icon != null)
        {
            icon.SetActive(false);
        }
        foreach (var goldBurst in goldBursts)
        {
            if (goldBurst != null)
            {
                goldBurst.SetActive(false);
            }
        }
    }
}