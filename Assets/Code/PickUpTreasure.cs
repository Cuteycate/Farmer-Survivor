using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpTreasure : MonoBehaviour
{
    public TreasureChest treasureChest;
    void Start()
    {
        // Find and assign the TreasureChest component
        treasureChest = FindObjectOfType<TreasureChest>();
        if (treasureChest == null)
        {
            Debug.LogError("TreasureChest component not found!");
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            treasureChest.Show();
            Destroy(gameObject);
        }
    }
}
