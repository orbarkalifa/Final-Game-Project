using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace EnemyAI
{
    public abstract class Enemy : Character
    {
        [Header("Vision Settings")]
        [Range(0f, 360f)]
        public float fieldOfViewAngle = 120f;
        public float recoilForce = 100;
        public Rigidbody2D rb;
        public Transform player;
        public float detectionRange = 10f;
        public LayerMask obstacleLayerMask;
        public LayerMask playerLayerMask;
        public Vector2 homePosition;
        public float maxChaseDistance = 15f;
        protected bool canMove = true;

        protected override void Awake()
        {
            base.Awake();
            homePosition = transform.position;
            
            if (rb == null) rb = GetComponent<Rigidbody2D>();

            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject.transform;
                }
                else
                {
                    Debug.LogError($"EnemyController ({gameObject.name}): Player not found! Make sure player has 'Player' tag or is assigned.", this);
                    enabled = false;
                    return;
                }
            }
        }

        public void UpdateFacingDirection(float xDirection)
        {
            if (!Mathf.Approximately(Mathf.Sign(xDirection), CurrentFacingDirection))
            {
                Flip();
            }
        }
        public void Flip()
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            CurrentFacingDirection = -CurrentFacingDirection;
        }
        
        public bool CanSeePlayer()
        {
            if (player == null) return false;

            Vector2 enemyPosition = transform.position;
            Vector2 playerPosition = player.position;
            Vector2 directionToPlayer = (playerPosition - enemyPosition);
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer > detectionRange)
            {
                return false;
            }

            Vector2 forwardDirection = transform.right * CurrentFacingDirection;
            float angleToPlayer = Vector2.Angle(forwardDirection, directionToPlayer);

            if (angleToPlayer > fieldOfViewAngle / 2f)
            {
#if UNITY_EDITOR
                Debug.DrawLine(enemyPosition, enemyPosition + (Vector2)(Quaternion.Euler(0, 0, fieldOfViewAngle / 2f) * forwardDirection * detectionRange), Color.grey);
                Debug.DrawLine(enemyPosition, enemyPosition + (Vector2)(Quaternion.Euler(0, 0, -fieldOfViewAngle / 2f) * forwardDirection * detectionRange), Color.grey);
#endif
                return false;
            }

            int combinedLayerMask = obstacleLayerMask | playerLayerMask;
            RaycastHit2D hit = Physics2D.Raycast(enemyPosition, directionToPlayer.normalized, distanceToPlayer, combinedLayerMask);

#if UNITY_EDITOR
            if (hit.collider != null)
            {
                var rayColor = (playerLayerMask == (playerLayerMask | (1 << hit.collider.gameObject.layer))) ? Color.green : Color.red;
                Debug.DrawRay(enemyPosition, directionToPlayer.normalized * hit.distance, rayColor);
            }
            else
            {
                Debug.DrawRay(enemyPosition, directionToPlayer.normalized * distanceToPlayer, Color.cyan);
            }
#endif

            if (hit.collider == null || (playerLayerMask == (playerLayerMask | (1 << hit.collider.gameObject.layer))))
            {
                return true;
            }

            return false;
        }

        protected float GetRecoilDirection(Transform target)
        {
            return (target.position - transform.position).normalized.x;
        }
        
        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                MainCharacter playerCollision = collision.gameObject.GetComponent<MainCharacter>();
                var recoilDirection = GetRecoilDirection(playerCollision.transform) > 0 ? 1 : -1;
                playerCollision.TakeDamage(1, recoilDirection);
            }
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            // Draw Detection Range
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(transform.position, Vector3.forward, detectionRange);

            // Draw FOV lines if selected
            if (CanSeePlayer())
            {
                Vector2 enemyPosition = transform.position;
                Vector2 forward = transform.right * CurrentFacingDirection;
                Handles.color = new Color(1f, 1f, 0f, 0.2f);
                Handles.DrawSolidArc(enemyPosition, Vector3.forward, Quaternion.Euler(0, 0, -fieldOfViewAngle / 2f) * forward, fieldOfViewAngle, detectionRange);
            }
#endif            
        }
        public bool IsDeadEnd()
        {
            var direction = new Vector2(transform.localScale.x, 0);
            RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, detectionRange * 0.5f, obstacleLayerMask);
            Debug.DrawRay(rb.position, direction * (detectionRange * 0.5f), Color.black);

            return hit.collider != null;
        }
        public override void TakeDamage(int damage, float direction)
        {
           StartCoroutine(EnemyRecoilCoroutine(direction));
            base.TakeDamage(damage);
        }


        private IEnumerator EnemyRecoilCoroutine(float recoilDirection)
        {
            canMove = false;
            rb.velocity = Vector2.zero;
            Debug.Log($"Applying {recoilDirection} * {recoilForce}");
            rb.AddForce(new Vector2(recoilDirection * recoilForce* rb.mass, 0), ForceMode2D.Impulse);
            // Wait for a short duration to allow the recoil to take effect.
            yield return new WaitForSeconds(0.2f);
            canMove = true;
        }
    }
}
