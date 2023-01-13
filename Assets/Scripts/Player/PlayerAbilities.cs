using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    /*
     * Purpose: Attached to Player Object and manages casting all abilities on skill bar.
     *          Also manages cooldowns and global cooldown timer
     * */

    // class > struct for mutability
    [Serializable]
    private struct AbilitySlot
    {
        [SerializeField] public Ability Ability { get; set; }
        [SerializeField] public bool Available { get; set; }

        public AbilitySlot(Ability ab)
        {
            Ability = ab;
            Available = true;
        }
    }
    [SerializeField] private AbilitySlot[] abilities;

    private int lastIndex = -1; // the last ability that was cast
    private string bufferedInput = "";
    private float bufferTimerStarted;
    private float bufferTimerDuration = 1f;

    // events
    public delegate void OnCast(string abilityName, float castTime, OnCastingComplete castingComplete);
    public static OnCast onCast; // used ONLY by cast bar, This is when button is pressed, not when skill is activated
    public delegate void OnCastCancel();
    public delegate void ControllingMovement(bool isControlling);
    public ControllingMovement D_ControllingMovement;
    public static OnCastCancel onCastCancel;
    public delegate void OnAbilityChanged();
    public OnAbilityChanged D_AbilityChanged;
    private bool casting = false;
    public delegate void OnCastingComplete(); // invoked by cast bar when timer is up
    private OnCastingComplete D_CastingComplete;
    public delegate void OnAnimationFinished(); // invoked when animation finished
    private OnAnimationFinished D_AnimationFinished;

    public enum AbilityHotkey { PRIMARY, SECONDARY, POWER1, POWER2, POWER3, MOVEMENT, }
 
    Player player;
    private AnimationManager animationManger;
    private MovementController movementController;
    private bool animating;

    private bool gcd = false;

    public const float Global_Cooldown = 0.45f;


    // TESTING DELETE TODO
    public Ability prim;

    private void Start()
    {
        SetupPlayer();
        int numAbilities = 1; // TESTING RESTORE //abilities.Length;
        for(int i = 0; i < numAbilities; ++i)
        {
            abilities[i] = new AbilitySlot(Instantiate(prim)); // TESTING RESTORE //abilities[i].Ability));
            abilities[i].Ability.SetOwner(gameObject);
        }

        D_CastingComplete += CastingFinished; // subscribe to event for cast bar to invoke
        D_AnimationFinished += AnimationFinished;
    }

    void SetupPlayer() // load save
    {
        player = GetComponent<Player>();
        animationManger = GetComponent<AnimationManager>();
        movementController = GetComponent<MovementController>();
    }

    public void SetAbility(int index, Ability ability)
    {
        abilities[index].Available = true;
        abilities[index].Ability = ability;
        abilities[index].Ability.SetOwner(gameObject);
    }

    private void Update()
    {
        // check ability buffer for input
        if(bufferedInput != "")
        {
            bool accepted = HandleAbilityInput(bufferedInput, InputActionPhase.Started);
            if (accepted)
            {
                bufferedInput = "";
            }
            else
            {
                // handle timer
                if (Time.fixedTime > bufferTimerStarted + bufferTimerDuration)
                {
                    bufferedInput = "";
                }
            }
        }

        // Cast Canceling
        if (!UIManager.instance._isOverUI && casting && Input.GetKeyDown(KeyCode.Escape))
        {
            casting = false;
            onCastCancel?.Invoke();
            movementController.MovementPaused(false);
        }
    }

    public void AbilityInput(InputAction.CallbackContext context)
    {
        HandleAbilityInput(context.action.name, context.phase);
    }

    /// <summary>
    ///  Returns true when the ability passes the candidate verification steps
    /// </summary>
    /// <param name="actionName">The action name from the Input Action context</param>
    /// <param name="phase">The InputActionPhase of the input</param>
    /// <returns></returns>
    public bool HandleAbilityInput(string actionName, InputActionPhase phase)
    {
        // TODO - figure out how to capture which ability was input, as an int?



        if(animating && phase == InputActionPhase.Started)
        {
            // store the input
            bufferedInput = actionName;
            bufferTimerStarted = Time.fixedTime;
            return false;
        }

        if (!animating && phase == InputActionPhase.Started &&
            !casting && !player.Stunned && abilities[0].Ability != null)
        {
            switch (actionName)
            {
                // Don't activate PrimaryAttack ability if over UI
                case "PrimaryAttack":
                    {
                        if (UIManager.instance._isOverUI)
                            return true;

                        ActivateAbility(0, AbilityHotkey.PRIMARY);
                    }
                    break;

                // roll is considered an ability. It's "cooldown" is
                // restricted only by it's animation speed
                case "Roll":
                    {
                        ActivateAbility(6, AbilityHotkey.MOVEMENT);
                    }
                    break;
            }
            return true;
        }

        return true;
    }

    public void AnimationFinished()
    {
        movementController.MovementPaused(false);
        animating = false;
        casting = false;
    }

    void CastingFinished()
    {
        if(abilities[lastIndex].Ability.Cast())
        {
            if (abilities[lastIndex].Ability is Movement)
                D_ControllingMovement?.Invoke(true);

            StartCoroutine(CooldownManager(lastIndex));
            player.RemoveMana(abilities[lastIndex].Ability.GetCost());
        }

        casting = false;

        if (!abilities[lastIndex].Ability.AnimationLocked()) {
            player.GetComponent<MovementController>().MovementPaused(false);
        }
    }

    void ActivateAbility(int index, AbilityHotkey hotkey)
    {
        // clear the input buffer when an ability is activated
        // this ignores whether the ability successfully casts. 
        // It was registered by the input system at this point
        bufferedInput = ""; 

        // animation/movement controllers
        animating = true;
        animationManger.AnimateAbility(hotkey, this.AnimationFinished);
        player.ActivateInvulnerability(1f);

        if(index + 1 > abilities.Length) 
        {
            // this should probably get a better solution.
            // its mostly for roll which uses this class and is assigned an index of 6
            return; 
        }

        // ability / effect controllers
        if (abilities[index].Available && !gcd && player.GetCurrentMana() > abilities[index].Ability.GetCost())
        {
            lastIndex = index;
            StartCoroutine(GlobalCooldown());

            if (abilities[index].Ability.CastTime > 0f) // do not begin cast bar for instant cast abilities
            {
                casting = true;
                onCast?.Invoke(abilities[index].Ability.abilityName, abilities[index].Ability.CastTime, D_CastingComplete);
                player.GetComponent<MovementController>().MovementPaused(true);
            }
            else
            {
                CastingFinished();
            }
        }
    }

    IEnumerator CooldownManager(int index)
    {
        abilities[index].Available = false;

        float cd = abilities[index].Ability.cooldown;

        if(cd > 0f)
            yield return new WaitForSeconds(cd);

        abilities[index].Available = true;
    }

    IEnumerator GlobalCooldown()
    {
        gcd = true;
        yield return new WaitForSeconds(Global_Cooldown);
        gcd = false;
    }
}
