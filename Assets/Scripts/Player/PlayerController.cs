using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Suits;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D.Animation;

namespace Player
{
    public class PlayerController : Character
    {
        private CinemachineImpulseSource impulseSource;
        public CharacterMovement characterMovement;
        public CharacterCombat characterCombat;
        private InputSystem_Actions inputActions;
        private Transform lastCheckPoint;
    
        [SerializeField] public Suit equippedSuit;
    
        [Header("Damage & Invincibility Settings")]
        [SerializeField] private float invincibilityDuration = 1.0f;
    
        [SerializeField] private BeaconSO beacon;
    
    
        [Header("Visuals")] 
        [SerializeField] private GameObject eye;
        [Header("Sprite Library Settings")]
        [SerializeField] private SpriteLibrary spriteLibrary;
        [SerializeField] private SpriteLibraryAsset normalSpriteLibraryAsset;

    
        [Header("Flashing Settings")]
        [SerializeField] private float flashDuration = 1.0f;
        [SerializeField] private float flashInterval = 0.1f;
    
        private bool usedSpecialAttack;
        private bool usedSpecialMovement;
      
        private readonly Dictionary<SuitAbility, Coroutine> activeAbilityCooldowns = new();

        protected override void Awake()
        {
            CurrentFacingDirection = 1;
            base.Awake();
            inputActions = new InputSystem_Actions();
            characterMovement = GetComponent<CharacterMovement>();
            characterCombat = GetComponent<CharacterCombat>();

            impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
            {
                Debug.LogError("impulse component not found on Main Camera!");
            }
            if (!characterMovement)
                Debug.LogError("CharacterMovement component is missing.");
            if (!characterCombat)
                Debug.LogError("CharacterCombat component is missing.");
        
            if(spriteLibrary != null && normalSpriteLibraryAsset != null)
                spriteLibrary.spriteLibraryAsset = normalSpriteLibraryAsset;
            beacon.uiChannel.ChangeHud(null);

        }

        private void Start()
        {
            beacon.uiChannel.ChangeHealth(currentHits);
        }

    
        private void FixedUpdate()
        {
            characterMovement.Move();
            if(transform.localScale.x < 0.1f) CurrentFacingDirection = -1;
        }
    
        private void OnEnable()
        {
            inputActions.Enable();

            // Register Input Action Callbacks
            inputActions.Player.Move.performed += OnMovePerformed;
            inputActions.Player.Move.canceled += OnMoveCanceled;
            inputActions.Player.Jump.performed += _ => characterMovement.Jump();
            inputActions.Player.Jump.canceled += _ => characterMovement.OnJumpReleased();
            inputActions.Player.BasicAttack.performed += _ => PerformBasicAttack();
            inputActions.Player.SpecialAttack.performed += _ => PerformSpecialAttack();
            inputActions.Player.SpecialMove.performed += _ => PerformSpecialMovement();
        }
    
        private void OnDisable()
        {
            inputActions.Disable();
        }
    
        private void PerformBasicAttack()
        {
            if (!characterCombat.canAttack) return;
            characterCombat.BasicAttack();
        }
        
        private void PerformSpecialAttack()
        {
            if (equippedSuit?.specialAttack != null)
            {
                AttemptExecuteAbility(equippedSuit.specialAttack);
            }
        }

        private void PerformSpecialMovement()
        {
            if (equippedSuit?.specialMovement != null)
            {
                AttemptExecuteAbility(equippedSuit.specialMovement, true);
            }
        }
        
        private void AttemptExecuteAbility(SuitAbility ability, bool isSpecialMovement = false)
        {
            if (ability == null) return;

            if (IsAbilityOnCooldown(ability))
            {
                Debug.Log($"{ability.abilityName} is on cooldown.");
                return;
            }

            if (isSpecialMovement && !characterMovement.canMove && ability.abilityName != "RockForm") 
            {
                Debug.Log($"Cannot use {ability.abilityName} - movement disabled.");
                return;
            }
            if (!isSpecialMovement && !characterCombat.canAttack) 
            {
                Debug.Log($"Cannot use {ability.abilityName} - combat disabled.");
                return;
            }

            ability.Execute(this); 
            
        }
        
        public bool IsAbilityOnCooldown(SuitAbility ability)
        {
            return activeAbilityCooldowns.ContainsKey(ability);
        }
        
        public void StartTrackingCooldown(SuitAbility abilityToCooldown)
        {
            if (!abilityToCooldown || abilityToCooldown.cooldownTime <= 0)
            {
                return;
            }

            if (IsAbilityOnCooldown(abilityToCooldown))
            {
                return;
            }

            activeAbilityCooldowns[abilityToCooldown] = StartCoroutine(AbilityCooldownCoroutine(abilityToCooldown, abilityToCooldown.cooldownTime));
            Debug.Log($"PlayerController: Tracking {abilityToCooldown.cooldownTime}s cooldown for {abilityToCooldown.abilityName}.");
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator AbilityCooldownCoroutine(SuitAbility ability, float duration)
        {
            yield return new WaitForSeconds(duration);
            activeAbilityCooldowns.Remove(ability);
            Debug.Log($"PlayerController: Cooldown finished for {ability.abilityName}");
        }
    
        public void EquipSuit(Suit newSuit)
        {
            if (newSuit == null) return;
            if (equippedSuit != null) { Heal(); return; }

            equippedSuit = newSuit;
            ApplySuitChanges();
        }

        private void UnEquipSuit()
        {
            if (equippedSuit == null) return;
            ClearCooldownsForSuit(equippedSuit);
            Debug.Log($"Unequipped suit: {equippedSuit.suitName}");
            equippedSuit = null;
            ApplySuitChanges();
            Heal();
        }
        
        private void ClearCooldownsForSuit(Suit suitToClear)
        {
            if (suitToClear == null) return;

            List<SuitAbility> abilitiesToClear = new List<SuitAbility>();
            if (suitToClear.specialAttack != null) abilitiesToClear.Add(suitToClear.specialAttack);
            if (suitToClear.specialMovement != null) abilitiesToClear.Add(suitToClear.specialMovement);
            // Add other ability slots if any

            foreach (var ability in abilitiesToClear)
            {
                if (IsAbilityOnCooldown(ability))
                {
                    StopCoroutine(activeAbilityCooldowns[ability]);
                    activeAbilityCooldowns.Remove(ability);
                     Debug.Log($"Cleared cooldown for {ability.abilityName} due to unequip.");
                }
            }
        }

        private void ApplySuitChanges()
        {
            if (equippedSuit != null)
            {
                if (spriteLibrary != null && equippedSuit.spriteLibrary != null)
                    spriteLibrary.spriteLibraryAsset = equippedSuit.spriteLibrary;
                eye.SetActive(false);
                beacon.uiChannel.ChangeHud(equippedSuit.hudSprite);
                characterCombat.ParametersSwap(equippedSuit);
            }
            else
            {
                if (spriteLibrary != null && normalSpriteLibraryAsset != null)
                    spriteLibrary.spriteLibraryAsset = normalSpriteLibraryAsset;
                eye.SetActive(true);
                beacon.uiChannel.ChangeHud(null);
                characterCombat.ParametersSwap(null);
            }
        }
        private void Heal()
        {
            currentHits = Mathf.Min(currentHits + 1, maxHits);
            beacon.uiChannel.ChangeHealth(currentHits);
        }

    
        public override void TakeDamage(int damage, float direction)
        {
            if (IsInvincible)
                return;
            Debug.Log($"got hit and has recoil {direction}");
            characterMovement.AddRecoil(direction);
            base.TakeDamage(damage);
            beacon.uiChannel.ChangeHealth(currentHits);
            StartCoroutine(FlashSprite());
            StartCoroutine(InvincibilityCoroutine());
        
        }
    
        private IEnumerator InvincibilityCoroutine()
        {
            IsInvincible = true;
            // Optionally add visual feedback such as blinking the sprite.
            yield return new WaitForSeconds(invincibilityDuration);
            IsInvincible = false;
        }
    
        
    
        public void UnlockWallGrabAbility()
        {
            characterMovement.canWallGrab = true;
            // visual/audio feedback
        }

        public void UnlockConsumeAbility()
        {
            inputActions.Player.Consume.performed += _ => UnEquipSuit();
        }
        
        private void OnDestroy()
        {
            inputActions?.Dispose();
        }

        protected override void OnDeath()
        {
            var gameOverState = beacon.gameStateChannel.GetGameStateByName("Game Over");
            if (beacon.gameStateChannel && gameOverState != null)
            {
                Debug.Log($"Game Over: {gameOverState.name}");
                beacon.gameStateChannel.RaiseStateTransitionRequest(gameOverState);
            }
            else
            {
                Debug.LogError("PlayerHealth: Beacon, GameStateChannel, or GameOverState is not assigned!");
            }
            base.OnDeath();
        }

        private IEnumerator FlashSprite()
        {
        
            if (!sr)
                yield break;

            float timer = 0f;
            while (timer < flashDuration)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);
                yield return new WaitForSeconds(flashInterval);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
                yield return new WaitForSeconds(flashInterval);
                timer += flashInterval * 2;
            }
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        }

        public void ChangeResetPoint(Transform resetPoint)
        {
            lastCheckPoint = resetPoint;
        }

        public void ResetPosition()
        {
            transform.position = lastCheckPoint.position;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            var movementInput = context.ReadValue<Vector2>();
            characterMovement.SetHorizontalInput(movementInput.x);
            characterCombat.PressedUp(movementInput.y > 0.5);
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            var movementInput = context.ReadValue<Vector2>();
            characterMovement.SetHorizontalInput(0);
            characterCombat.PressedUp(movementInput.y > 0.5);
        }

        public bool IsGrounded()
        {
            return characterMovement.IsGrounded();
        }
        public void ChangeInvincibleState()
        {
            IsInvincible = !IsInvincible;
        }

        public void ImpulseCamera()
        {
            impulseSource.GenerateImpulse();
        }

    }
}