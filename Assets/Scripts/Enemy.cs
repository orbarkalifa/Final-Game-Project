using UnityEngine;

public class Enemy : Character
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            if (projectile != null)
                TakeDamage(projectile.Damage);
        }
    }

    protected override void OnDeath()
    {
        Debug.Log($"{gameObject.name} (Enemy) has been defeated!");
        dropWeapon();
        base.OnDeath();
    }

    private void dropWeapon()
    {
        if (m_CurrentWeapon != null)
        {
            Debug.Log($"Dropping weapon: {m_CurrentWeapon.name}");
            Instantiate(m_CurrentWeapon, new Vector3(transform.position.x,transform.position.y,-1), Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("m_CurrentWeapon is null, no weapon to drop.");
        }
    }
    
}