using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D.Animation;

public class MainCharacter : Character
{
    private CharacterMovement characterMovement;
    private CharacterCombat characterCombat;
    private InputSystem_Actions inputActions;
    private Transform lastCheckPoint;
    
    [SerializeField] private Suit equippedSuit;
    
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
      

    protected override void Awake()
    {
        CurrentFacingDirection = 1;
        base.Awake();
        inputActions = new InputSystem_Actions();
        characterMovement = GetComponent<CharacterMovement>();
        characterCombat = GetComponent<CharacterCombat>();

        
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
        Debug.Log($"{gameObject.name} performs a basic attack.dirction {CurrentFacingDirection}");
        characterCombat.BasicAttack();
    }
    
    private void PerformSpecialAttack()
    {
        if (!characterCombat.canAttack) return;
        
        if (equippedSuit?.specialAttack != null)
        {
            if(!usedSpecialAttack)
            {
                equippedSuit.specialAttack.ExecuteAbility(gameObject);
                StartCoroutine(SpecialAttackCD(equippedSuit.specialAttack.cooldownTime));
            }
            else
            {
                Debug.Log("mot time yet");
            }
        }
        else
        {
            Debug.LogWarning("No suit equipped or no special attack available.");
        }
    }
    
    private void PerformSpecialMovement()
    {
        if (equippedSuit.specialMovement != null)
        {
            if(!usedSpecialMovement)
            {
                equippedSuit.specialMovement.ExecuteAbility(gameObject);
                if (equippedSuit.suitName != "Duri")
                {
                    StartCoroutine(SpecialMovementCD(equippedSuit.specialMovement.cooldownTime));
                }
            }
        }
        else
        {
            Debug.LogWarning("No suit equipped or no special movement available.");
        }
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

    
    public void EquipSuit(Suit newSuit)
    {
        if (equippedSuit != null)
        {
            UnEquipSuit();
        }
        
        equippedSuit = newSuit;

        if (equippedSuit != null)
        {
            
            if(spriteLibrary != null)
            {
                eye.SetActive(false);
                spriteLibrary.spriteLibraryAsset = equippedSuit.spriteLibrary;
            }
            beacon.uiChannel.ChangeHud(equippedSuit.hudSprite);
            characterCombat.ParametersSwap(equippedSuit);
        }
        
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
    
    private void UnEquipSuit()
    {
        if (equippedSuit != null)
        {
            eye.SetActive(true);
            Debug.Log($"Unequipped suit: {equippedSuit.suitName}");
            equippedSuit = null;
            characterCombat.ParametersSwap(null);
            beacon.uiChannel.ChangeHud(null);
            if(spriteLibrary != null && normalSpriteLibraryAsset != null)
                spriteLibrary.spriteLibraryAsset = normalSpriteLibraryAsset;
            Heal();
        }
        
    }
    private void Heal()
    {
        currentHits = Mathf.Min(currentHits + 1, maxHits);
        beacon.uiChannel.ChangeHealth(currentHits);
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
        
        if (sr == null)
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
    public IEnumerator SpecialAttackCD(float time)
    {
        usedSpecialAttack = true;
        yield return new WaitForSeconds(time);
        usedSpecialAttack = false;
    }
    public IEnumerator SpecialMovementCD(float specialMovementCooldownTime)
    {
        usedSpecialMovement = true;
        yield return new WaitForSeconds(specialMovementCooldownTime);
        usedSpecialMovement = false;
    }
    
    public void OnMovePerformed(InputAction.CallbackContext context)
    {
        var movementInput = context.ReadValue<Vector2>();
        characterMovement.SetHorizontalInput(movementInput.x);
        characterCombat.PressedUp(movementInput.y > 0.5);
    }
    public void OnMoveCanceled(InputAction.CallbackContext context)
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
    public void ChangeSprite(Sprite sprite)
    {
        sr.sprite = sprite;
        animator.enabled = false;
    }
    
}