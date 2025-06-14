using System;
using System.Collections;
using Player;
using UnityEditor;
using UnityEngine;

namespace EnemyAI
{
    public abstract class Enemy : Character
    {
        [Header("Vision Settings")]
        [Range(0f, 360f)]
        public float fieldOfViewAngle = 120f;
        [SerializeField] protected Vector2 lookbackOffset= Vector2.zero;
        public float recoilForce = 100;
        public Transform player;
        public Vector3 lastKnownPlayerPosition;
        public float detectionRange = 10f;
        public LayerMask obstacleLayerMask;
        public LayerMask playerLayerMask;
        public Vector2 homePosition;
        public float maxChaseDistance = 15f;
        
        [Header("Stun Settings")]
        [SerializeField] private Color stunColor = Color.yellow;
        protected bool isStunned;
        private Color originalColor;
        
        protected bool CanMove = true;

        protected override void Awake()
        {
            base.Awake();
            homePosition = transform.position;
            
            if (sr != null)
                originalColor = sr.color;
            
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
                }
            }
        }

        public void UpdateFacingDirection(float xInput) 
        {
            // Prevent flipping if there's no significant horizontal movement/input
            if (Mathf.Approximately(xInput, 0f))
            {
                return;
            }

            float targetFacingDirection = Mathf.Sign(xInput); // Will be 1 or -1

            // Check if current facing direction needs to be changed
            // CurrentFacingDirection is 1 (right) or -1 (left)
            if (!Mathf.Approximately(targetFacingDirection, CurrentFacingDirection))
            {
                Flip();
            }
        }

        public float GetDistanceToPlayer()
        {
            if (player == null) return 0;
            return Vector2.Distance(transform.position, player.position);
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
                lastKnownPlayerPosition = player.position;
                return true;
            }

            return false;
        }
        public bool CheckBehindForPlayer()
        {
            if (!player) return false;

            Vector2 enemyPosition = transform.position;
            Vector2 playerPosition = player.position;

            float distanceToPlayer = Vector2.Distance(enemyPosition, playerPosition);
            if (distanceToPlayer > detectionRange)
            {
                return false;
            }

            Vector2 backDirection = -transform.right * CurrentFacingDirection;
            int combinedLayerMask = obstacleLayerMask | playerLayerMask;

            RaycastHit2D hit = Physics2D.Raycast(
                enemyPosition + lookbackOffset,
                backDirection,
                detectionRange,
                combinedLayerMask
            );

#if UNITY_EDITOR
            Color rayColor = Color.magenta; 
            if (hit.collider)
            {
                rayColor = (playerLayerMask == (playerLayerMask | (1 << hit.collider.gameObject.layer))) ? Color.green : Color.red;
                Debug.DrawRay(enemyPosition, backDirection * hit.distance, rayColor);
            }
            else
            {
                Debug.DrawRay(enemyPosition, backDirection * detectionRange, rayColor * 0.5f); 
            }
#endif

            if (hit.collider && (playerLayerMask == (playerLayerMask | (1 << hit.collider.gameObject.layer))))
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
            if (collision.gameObject. CompareTag("Player"))
            {
                PlayerController playerCollision = collision.gameObject.GetComponent<PlayerController>();
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
            RaycastHit2D hit = Physics2D.Raycast(rb.position - new Vector2(0,1), direction, detectionRange * 0.5f, obstacleLayerMask);
            Debug.DrawRay(rb.position - new Vector2(0,1), direction * (detectionRange * 0.5f), Color.black);

            return hit.collider != null;
        }
        public override void TakeDamage(int damage, float direction)
        {
            StartCoroutine(EnemyRecoilCoroutine(direction));
            base.TakeDamage(damage);
        }

        public virtual void Stun(float duration)
        {
            // don’t stack stuns
            if (isStunned) return;
            StartCoroutine(StunCoroutine(duration));
        }
        
        private IEnumerator StunCoroutine(float duration)
        {
            isStunned = true;
            CanMove   = false;
            rb.velocity = Vector2.zero;
            if (sr)
                sr.color = stunColor;

            animator.Play("damaged");
            yield return new WaitForSeconds(duration);

            CanMove   = true;
            isStunned = false;
            if (sr)
                sr.color = originalColor;
        }

        private IEnumerator EnemyRecoilCoroutine(float recoilDirection)
        {
            CanMove = false;
            rb.velocity = Vector2.zero;
            Debug.Log($"Applying {recoilDirection} * {recoilForce}");
            rb.AddForce(new Vector2(recoilDirection * recoilForce * rb.mass, 0), ForceMode2D.Impulse);
            // Wait for a short duration to allow the recoil to take effect.
            yield return new WaitForSeconds(0.4f);
            CanMove = true;
        }
    }
}
