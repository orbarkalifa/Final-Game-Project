﻿using UnityEngine;
using Player;
using UnityEditor;

namespace EnemyAI
{
    public class DuriController: EnemyController
    {
              
        [SerializeField] private float attackHeight = 4f;
        [SerializeField] private float bounceForce = 10f;
        public override void TriggerAttackDamage()
        {
            // The enemy's world position is the origin for the attack.
            Vector2 originPoint = rb.position;

            // If attackRange is the distance from the center to the edge,
            // the total width of the box is attackRange * 2.
            Vector2 hitboxSize = new Vector2(attackRange * 2f, attackHeight);

            // Detect players inside this centered attack box
            Collider2D[] hits = Physics2D.OverlapBoxAll(originPoint, hitboxSize, 0f, playerLayerMask);
            foreach (var col in hits)
            {
                if (col.TryGetComponent(out PlayerController player))
                {
                    // The recoil direction is still based on the player's relative position.
                    // This part of your logic was already good.
                    float recoilDir = Mathf.Sign(player.transform.position.x - transform.position.x);
                    if (recoilDir == 0) recoilDir = Mathf.Sign(transform.localScale.x); // Fallback to facing direction
                    player.TakeDamage(2, recoilDir);

                    if (player.rb != null && bounceForce > 0)
                    {
                        player.rb.AddForce(Vector2.up * bounceForce * player.rb.mass, ForceMode2D.Impulse);
                    }
                }
            }
        }
        
        public override void Attack()
        {
            lastAttackTime = Time.time;
            animator.CrossFadeInFixedTime("Duri_Attack", 0.05f);
        }

        public override void Patrol()
        { 
            if(!CanMove || isStunned)
            {
                return;
            }

            if(patrolPoints == null || patrolPoints.Length == 0)
            {
                rb.velocity = new Vector2(0, rb.velocity.y); // Stop if no patrol points
                return;
            }

            Vector3 currentTargetPoint = patrolPoints[currentPatrolIndex];
            float distanceToCurrentTarget = Vector2.Distance(transform.position, currentTargetPoint);

            // Check if we need to switch to the next patrol point
            if(distanceToCurrentTarget <= waypointArrivalThreshold)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                currentTargetPoint = patrolPoints[currentPatrolIndex]; // Update to the new target
                // Optionally, add a small pause here (e.g. with a timer) if desired.
            }

            // Move towards the (potentially new) current target point
            Vector2 direction = ((Vector2)currentTargetPoint - (Vector2)transform.position).normalized;

            if(direction.sqrMagnitude > 0.01f) // If there's a direction to move (not already at target)
            {
                rb.velocity = new Vector2(direction.x * patrolSpeed, rb.velocity.y);
                UpdateFacingDirection(direction.x);
            }
            else
            {
                // Very close or at the target, stop to prevent jitter.
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        public override void Chase()
        {
            if(!CanMove || isStunned || player == null)
            {
                if(player == null) rb.velocity = new Vector2(0, rb.velocity.y); // Stop if player is gone
                return;
            }

            // Determine if we should react to player behind us (optional quick turn)
            if(CheckBehindForPlayer() && !CanSeePlayer()) // Prioritize actual sight if available
            {
                Flip(); // This updates CurrentFacingDirection
            }

            Vector3 targetPosition;
            bool currentlySeesPlayer = CanSeePlayer(); // Cache for this frame

            if(currentlySeesPlayer)
            {
                targetPosition = player.position; // lastKnownPlayerPosition is updated by CanSeePlayer
            }
            else
            {
                targetPosition = lastKnownPlayerPosition;
            }

            Vector2 directionToTarget = ((Vector2)targetPosition - (Vector2)transform.position);
            float distanceToTarget = directionToTarget.magnitude; // Get actual distance

            // If not seeing player and have arrived at LKP, stop. State machine will handle transition.
            // Use a small threshold to prevent jitter.
            if(!currentlySeesPlayer
               && distanceToTarget < waypointArrivalThreshold * 0.5f) // Or a dedicated LKP arrival threshold
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(directionToTarget.normalized.x * chaseSpeed, rb.velocity.y);
            }

            // Update facing direction based on movement or target direction
            if(Mathf.Abs(directionToTarget.normalized.x) > 0.01f)
            {
                UpdateFacingDirection(directionToTarget.normalized.x);
            }
        }

        public override void Flee()
        {
            if(!CanMove || isStunned)
            {
                return;
            }

            Vector2 directionToPlayer = player.position - transform.position;
            Vector2 fleeDirection = -directionToPlayer.normalized;
            rb.velocity = new Vector2(fleeDirection.x * fleeSpeed, rb.velocity.y);
            UpdateFacingDirection(fleeDirection.x);
        }

        public override void ReturnHome()
        {
            if(!CanMove || isStunned)
            {
                return;
            }

            Vector2 dir = (homePosition - (Vector2)transform.position).normalized;
            rb.velocity = new Vector2(dir.x * returnSpeed, rb.velocity.y);
            UpdateFacingDirection(dir.x);
        }
#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // --- These calculations MUST match TriggerAttackDamage() ---
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>(); // Try to get the component for editor drawing.
                if (rb == null) return; // If it's still null, exit to prevent errors.
            }
            
            // 1. Define the center point of the attack hitbox.
            // It's centered on the Rigidbody's position 
            Vector2 hitboxCenter = rb.position;

            // 2. Define the size of the hitbox.
            // The width is doubled to extend on both sides.
            Vector2 hitboxSize = new Vector2(attackRange * 2f, attackHeight);

            // 3. Draw the Gizmo
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(hitboxCenter, hitboxSize);
        }
#endif
    }
}