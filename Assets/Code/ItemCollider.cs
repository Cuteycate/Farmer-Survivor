using UnityEngine;

public class ItemCollider : MonoBehaviour
{
    public CircleCollider2D circleCollider;
    public float defaultRadius = 0.5f; 

    void Awake()
    {
        if (circleCollider == null)
        {
            circleCollider = GetComponent<CircleCollider2D>();
        }
        UpdateRadius(defaultRadius * ShopStats.Instance.magnetMultiplier); 
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;
    }
    public void UpdateRadius(float newRadius)
    {
        if (circleCollider != null)
        {
            circleCollider.radius = newRadius;
        }
    }
}
