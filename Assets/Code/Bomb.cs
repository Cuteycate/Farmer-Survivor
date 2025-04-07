using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float damage = 9999f;
    public float radius = 15f;
    public LayerMask enemyLayer; // Set this to the layer where your monsters are placed

    public GameObject hitEffect; // Visual effect for hitting monsters

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Clear all monsters from the scene
            AreaAttack();

            // Destroy the item GameObject after the player picks it up
            Destroy(gameObject);
        }
    }
    void AreaAttack()
    {
        // Detect all colliders within the radius of the item
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        // Damage all monsters within the radius
        foreach (Collider2D collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                // Show hit effect at the position of the monster
                //Instantiate(hitEffect, enemy.transform.position, Quaternion.identity);
            }

            EnemyEvent enemyEvent = collider.GetComponent<EnemyEvent>();
            if (enemyEvent != null)
            {
                enemyEvent.TakeDamage(damage);
            }

        }

        // Destroy the item after the area attack
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Draw a gizmo to visualize the radius of the area attack
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}