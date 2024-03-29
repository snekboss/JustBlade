using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static class which contains the methods that are used in combat.
/// It is mainly used by the <see cref="Weapon"/> class.
/// The methods have been put here to avoid cluttering the <see cref="Weapon"/> class with such details.
/// Some examples are: 
/// - Mehtods regarding determining if an <see cref="Agent"/> was able to defend against an attack.
/// - Methods regarding which "getting hurt" animation should be played, based on the attack direction
/// and the position of the defender (as well as the relative position of the attacker).
/// </summary>
public static class CombatMechanics
{
    enum AttackerRelativePosition
    {
        InFrontOfTheDefender = 0,
        OnTheRightSideOfTheDefender,
        BehindTheDefender,
        OnTheLeftSideOfTheDefender
    }

    // Angle values for defender's local circle. Zero degrees is considered straight up.
    const float ForwardAreaMinAngle = -60;
    const float ForwardAreaMaxAngle = 60;

    const float RightAreaMinAngle = -100f;
    const float RightAreMaxAngle = ForwardAreaMinAngle;

    const float LeftAreaMinAngle = ForwardAreaMaxAngle;
    const float LeftAreaMaxAngle = 100f;

    const float HandArmorDmgReductionPercentLight = 0.02f;
    const float HandArmorDmgReductionPercentMed = 0.04f;
    const float HandArmorDmgReductionPercentHeavy = 0.08f;

    /// <summary>
    /// Determines whether the defender agent is able to block the attack of the attacker agent.
    /// </summary>
    /// <param name="attacker">The attacking agent.</param>
    /// <param name="defender">The defending agent.</param>
    /// <returns></returns>
    public static bool IsDefenderAbleToBlock(Agent attacker, Agent defender)
    {
        if (defender.AnimMgr.IsDefending == false)
        {
            return false;
        }

        AttackerRelativePosition attackerRelativePosition = GetAttackerRelativePosition(attacker, defender);

        // Initially, assume that the defender is NOT able to block.
        bool defenderIsAbleToBlock = false;

        if (attackerRelativePosition == AttackerRelativePosition.InFrontOfTheDefender)
        {
            if (attacker.AnimMgr.IsAttackingFromUp)
            {
                if (defender.AnimMgr.IsDefendingFromUp)
                {
                    defenderIsAbleToBlock = true;
                }
            }
            else if (attacker.AnimMgr.IsAttackingFromRight)
            {
                if (defender.AnimMgr.IsDefendingFromLeft)
                {
                    defenderIsAbleToBlock = true;
                }
            }
            else if (attacker.AnimMgr.IsAttackingFromDown)
            {
                if (defender.AnimMgr.IsDefendingFromDown)
                {
                    defenderIsAbleToBlock = true;
                }
            }
            else if (attacker.AnimMgr.IsAttackingFromLeft)
            {
                if (defender.AnimMgr.IsDefendingFromRight)
                {
                    defenderIsAbleToBlock = true;
                }
            }
        }
        else if (attackerRelativePosition == AttackerRelativePosition.OnTheRightSideOfTheDefender)
        {
            // When the attacker is to the right of the defender, defender can ONLY defend attacks that are coming from the right.
            // Meaning, the defender cannot defend up, down and left directions.
            // This is because agents are right handed, and the weapons extend towards the agents' left side.
            // Meaning, the right side is relatively unprotected.

            if (attacker.AnimMgr.IsAttackingFromUp)
            {
                if (defender.AnimMgr.IsDefendingFromUp)
                {
                    defenderIsAbleToBlock = true;
                }
            }
            else if (attacker.AnimMgr.IsAttackingFromRight)
            {
                if (defender.AnimMgr.IsDefendingFromRight)
                {
                    defenderIsAbleToBlock = true;
                }
            }
            else if (attacker.AnimMgr.IsAttackingFromDown)
            {
                if (defender.AnimMgr.IsDefendingFromDown)
                {
                    defenderIsAbleToBlock = true;
                }
            }
        }
        else if (attackerRelativePosition == AttackerRelativePosition.OnTheLeftSideOfTheDefender)
        {
            // When the attacker is to the left of the defender, defender cannot defend attacks that are coming from the right.
            // Meaning, the defender can defend up, down and left directions.
            // This is because agents are right handed, and the weapons extend towards the agents' left side.
            // Meaning, the left side is relatively protected.

            if (attacker.AnimMgr.IsAttackingFromUp)
            {
                if (defender.AnimMgr.IsDefendingFromUp)
                {
                    defenderIsAbleToBlock = true;
                }
            }
            else if (attacker.AnimMgr.IsAttackingFromDown)
            {
                if (defender.AnimMgr.IsDefendingFromDown)
                {
                    defenderIsAbleToBlock = true;
                }
            }
            else if (attacker.AnimMgr.IsAttackingFromLeft)
            {
                if (defender.AnimMgr.IsDefendingFromLeft)
                {
                    defenderIsAbleToBlock = true;
                }
            }
        }

        // Obviously, agents cannot defend themselves when the attacker is behind them. 

        return defenderIsAbleToBlock;
    }

    /// <summary>
    /// Calculates and applies the damage value to the defender agent, inflicted by the attacker agent,
    /// based on the attacked limb type.
    /// </summary>
    /// <param name="attacker">The attacking agent.</param>
    /// <param name="defender">The defending agent.</param>
    /// <param name="limbType">The limb type that was struck.</param>
    public static void ApplyDamageToDefender(Agent attacker, Agent defender, Limb.LimbType limbType)
    {
        bool attackerIsStabbing = attacker.AnimMgr.IsAttackingFromDown;

        Weapon attackerWeapon = attacker.EqMgr.EquippedWeapon;
        float rawDamage = 0;

        // The reason for this code is because Unity's inspector menu doesn't want to serialize dictionaries by default.
        if (attackerIsStabbing)
        {
            if (limbType == Limb.LimbType.Head)
            {
                Armor.ArmorLevel defHeadArmorLevel = defender.EqMgr.HeadArmorLevel;
                if (defHeadArmorLevel == Armor.ArmorLevel.None)
                {
                    rawDamage = attackerWeapon.StabDmgHeadNaked;
                }
                else if (defHeadArmorLevel == Armor.ArmorLevel.Light)
                {
                    rawDamage = attackerWeapon.StabDmgHeadLight;
                }
                else if (defHeadArmorLevel == Armor.ArmorLevel.Medium)
                {
                    rawDamage = attackerWeapon.StabDmgHeadMed;
                }
                else // if (defHeadArmorLevel == Armor.ArmorLevel.Heavy)
                {
                    rawDamage = attackerWeapon.StabDmgHeadHeavy;
                }
            }
            else if (limbType == Limb.LimbType.Torso)
            {
                Armor.ArmorLevel defTorsoArmorLevel = defender.EqMgr.TorsoArmorLevel;
                if (defTorsoArmorLevel == Armor.ArmorLevel.None)
                {
                    rawDamage = attackerWeapon.StabDmgTorsoNaked;
                }
                else if (defTorsoArmorLevel == Armor.ArmorLevel.Light)
                {
                    rawDamage = attackerWeapon.StabDmgTorsoLight;
                }
                else if (defTorsoArmorLevel == Armor.ArmorLevel.Medium)
                {
                    rawDamage = attackerWeapon.StabDmgTorsoMed;
                }
                else // if (defTorsoArmorLevel == Armor.ArmorLevel.Heavy)
                {
                    rawDamage = attackerWeapon.StabDmgTorsoHeavy;
                }
            }
            else // if(limbType == Limb.LimbType.Legs)
            {
                Armor.ArmorLevel defLegArmorLevel = defender.EqMgr.LegArmorLevel;
                if (defLegArmorLevel == Armor.ArmorLevel.None)
                {
                    rawDamage = attackerWeapon.StabDmgLegNaked;
                }
                else if (defLegArmorLevel == Armor.ArmorLevel.Light)
                {
                    rawDamage = attackerWeapon.StabDmgLegLight;
                }
                else if (defLegArmorLevel == Armor.ArmorLevel.Medium)
                {
                    rawDamage = attackerWeapon.StabDmgLegMed;
                }
                else // if (defLegArmorLevel == Armor.ArmorLevel.Heavy)
                {
                    rawDamage = attackerWeapon.StabDmgLegHeavy;
                }
            }
        }
        else
        {
            // Attacker is performing a swinging attack (up/right/left).
            if (limbType == Limb.LimbType.Head)
            {
                Armor.ArmorLevel defHeadArmorLevel = defender.EqMgr.HeadArmorLevel;
                if (defHeadArmorLevel == Armor.ArmorLevel.None)
                {
                    rawDamage = attackerWeapon.SwingDmgHeadNaked;
                }
                else if (defHeadArmorLevel == Armor.ArmorLevel.Light)
                {
                    rawDamage = attackerWeapon.SwingDmgHeadLight;
                }
                else if (defHeadArmorLevel == Armor.ArmorLevel.Medium)
                {
                    rawDamage = attackerWeapon.SwingDmgHeadMed;
                }
                else // if (defHeadArmorLevel == Armor.ArmorLevel.Heavy)
                {
                    rawDamage = attackerWeapon.SwingDmgHeadHeavy;
                }
            }
            else if (limbType == Limb.LimbType.Torso)
            {
                Armor.ArmorLevel defTorsoArmorLevel = defender.EqMgr.TorsoArmorLevel;
                if (defTorsoArmorLevel == Armor.ArmorLevel.None)
                {
                    rawDamage = attackerWeapon.SwingDmgTorsoNaked;
                }
                else if (defTorsoArmorLevel == Armor.ArmorLevel.Light)
                {
                    rawDamage = attackerWeapon.SwingDmgTorsoLight;
                }
                else if (defTorsoArmorLevel == Armor.ArmorLevel.Medium)
                {
                    rawDamage = attackerWeapon.SwingDmgTorsoMed;
                }
                else // if (defTorsoArmorLevel == Armor.ArmorLevel.Heavy)
                {
                    rawDamage = attackerWeapon.SwingDmgTorsoHeavy;
                }
            }
            else // if(limbType == Limb.LimbType.Legs)
            {
                Armor.ArmorLevel defLegArmorLevel = defender.EqMgr.LegArmorLevel;
                if (defLegArmorLevel == Armor.ArmorLevel.None)
                {
                    rawDamage = attackerWeapon.SwingDmgLegNaked;
                }
                else if (defLegArmorLevel == Armor.ArmorLevel.Light)
                {
                    rawDamage = attackerWeapon.SwingDmgLegLight;
                }
                else if (defLegArmorLevel == Armor.ArmorLevel.Medium)
                {
                    rawDamage = attackerWeapon.SwingDmgLegMed;
                }
                else // if (defLegArmorLevel == Armor.ArmorLevel.Heavy)
                {
                    rawDamage = attackerWeapon.SwingDmgLegHeavy;
                }
            }
        }

        // Apply hand armor damage reduction
        Armor.ArmorLevel defHandArmorLevel = defender.EqMgr.HandArmorLevel;
        float dmgReductionMulti = 0;
        if (defHandArmorLevel == Armor.ArmorLevel.Light)
        {
            dmgReductionMulti = HandArmorDmgReductionPercentLight;
        }
        else if (defHandArmorLevel == Armor.ArmorLevel.Medium)
        {
            dmgReductionMulti = HandArmorDmgReductionPercentMed;
        }
        else if (defHandArmorLevel == Armor.ArmorLevel.Heavy)
        {
            dmgReductionMulti = HandArmorDmgReductionPercentHeavy;
        }

        float finalDamage = (rawDamage * attacker.CharMgr.ExtraDamageInflictionMultiplier) 
            * (1 - dmgReductionMulti) 
            * defender.CharMgr.DamageTakenMultiplier;

        int finalDamageInt = System.Convert.ToInt32(finalDamage);

        // Apply damage here.
        defender.CharMgr.ApplyDamage(attacker, finalDamageInt);

        // Track player stat
        if (attacker.IsPlayerAgent)
        {
            PlayerStatisticsTracker.PlayerTotalDamageDealt += finalDamageInt;
        }

        if (defender.IsPlayerAgent)
        {
            PlayerStatisticsTracker.PlayerTotalDamageTaken += finalDamageInt;
        }

        if (defender.CharMgr.CanPoiseThroughAttack() == false)
        {
            // Then, set the correct getting_hurt animation.
            AnimationManager.GettingHurtDirection defGetHurtDir = GetDefenderHurtAnimation(attacker, defender, limbType);
            defender.AnimMgr.SetGettingHurt(defGetHurtDir);
        }

        defender.CharMgr.DecrementPoise();
    }

    /// <summary>
    /// Returns the relative position of the attacker with respect to the defender.
    /// </summary>
    /// <param name="attacker">The attacking agent.</param>
    /// <param name="defender">The defending agent.</param>
    /// <returns></returns>
    static AttackerRelativePosition GetAttackerRelativePosition(Agent attacker, Agent defender)
    {
        Vector2 defenderForwardVector = new Vector2(defender.gameObject.transform.forward.x, defender.gameObject.transform.forward.z);
        float defenderForwardAngle = Mathf.Atan2(defenderForwardVector.y, defenderForwardVector.x) * Mathf.Rad2Deg;

        Vector2 defenderPosition = new Vector2(defender.gameObject.transform.position.x, defender.gameObject.transform.position.z);
        Vector2 attackerPosition = new Vector2(attacker.gameObject.transform.position.x, attacker.gameObject.transform.position.z);

        Vector2 attackerAndDefenderDisplacement = attackerPosition - defenderPosition;

        float attackerDisplacementAngle = Mathf.Atan2(attackerAndDefenderDisplacement.y, attackerAndDefenderDisplacement.x) * Mathf.Rad2Deg;

        #region Transforming angles in order to create the defender's local circle.
        // This code transforms both angles as if they were the angles relative to the local 2D circle of the defender
        // In other words, after the transformation, the defenderForwardAngle is always considered 0 (and 0 degrees is straight up).
        attackerDisplacementAngle -= defenderForwardAngle;

        if (attackerDisplacementAngle < -180f)
        {
            attackerDisplacementAngle += 360f;
        }
        else if (attackerDisplacementAngle > 180f)
        {
            attackerDisplacementAngle -= 360f;
        }

        defenderForwardAngle = 0f;
        #endregion

        AttackerRelativePosition attackerRelativePosition;

        // Checking angles of the defender's local circle.
        if (((attackerDisplacementAngle >= defenderForwardAngle) && (attackerDisplacementAngle < ForwardAreaMaxAngle))
            || ((attackerDisplacementAngle < defenderForwardAngle) && (attackerDisplacementAngle > ForwardAreaMinAngle)))
        {
            attackerRelativePosition = AttackerRelativePosition.InFrontOfTheDefender;
        }
        else if (attackerDisplacementAngle >= RightAreaMinAngle && attackerDisplacementAngle < RightAreMaxAngle)
        {
            attackerRelativePosition = AttackerRelativePosition.OnTheRightSideOfTheDefender;
        }
        else if (attackerDisplacementAngle >= LeftAreaMinAngle && attackerDisplacementAngle < LeftAreaMaxAngle)
        {
            attackerRelativePosition = AttackerRelativePosition.OnTheLeftSideOfTheDefender;
        }
        else
        {
            attackerRelativePosition = AttackerRelativePosition.BehindTheDefender;
        }

        return attackerRelativePosition;
    }

    /// <summary>
    /// Returns the direction in which the defender is hurt based on:
    /// 1) The relative position of the attacker.
    /// 2) The combat direction in which the attacker is attacking.
    /// 3) The limb type that was struck.
    /// This direction is then used to determine which "getting hurt" animation is to be played.
    /// </summary>
    /// <param name="attacker">The attacking agent.</param>
    /// <param name="defender">The defending agent.</param>
    /// <param name="limbType">The limb type that was struck.</param>
    /// <returns></returns>
    static AnimationManager.GettingHurtDirection GetDefenderHurtAnimation(Agent attacker, Agent defender, Limb.LimbType limbType)
    {
        // Again, CombatDirection and GettingHurtDirection enums have the same values, but aren't the same things.
        // The reason for this coincidence is the fact that I only have 4 getting_hurt animations.
        // If I had more animations, this coincidence would not occur.

        AnimationManager.GettingHurtDirection defenderGettingHurtAnimation = new AnimationManager.GettingHurtDirection();
        AttackerRelativePosition attackerRelativePosition = GetAttackerRelativePosition(attacker, defender);

        if (attackerRelativePosition == AttackerRelativePosition.InFrontOfTheDefender)
        {
            if (limbType == Limb.LimbType.Head)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Down;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
            }
            else if (limbType == Limb.LimbType.Torso)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Down;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Down;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
            }
            else if (limbType == Limb.LimbType.Legs)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Down;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Down;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
            }
        }
        else if (attackerRelativePosition == AttackerRelativePosition.OnTheRightSideOfTheDefender)
        {
            if (limbType == Limb.LimbType.Head)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Down;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
            }
            else if (limbType == Limb.LimbType.Torso)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Down;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
            }
            else if (limbType == Limb.LimbType.Legs)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
            }
        }
        else if (attackerRelativePosition == AttackerRelativePosition.OnTheLeftSideOfTheDefender)
        {
            if (limbType == Limb.LimbType.Head)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
            }
            else if (limbType == Limb.LimbType.Torso)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
            }
            else if (limbType == Limb.LimbType.Legs)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
            }
        }
        else if (attackerRelativePosition == AttackerRelativePosition.BehindTheDefender)
        {
            if (limbType == Limb.LimbType.Head)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Down;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
            }
            else if (limbType == Limb.LimbType.Torso)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Left;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Right;
                }
            }
            else if (limbType == Limb.LimbType.Legs)
            {
                if (attacker.AnimMgr.IsAttackingFromUp)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
                else if (attacker.AnimMgr.IsAttackingFromRight)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
                else if (attacker.AnimMgr.IsAttackingFromDown)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
                else if (attacker.AnimMgr.IsAttackingFromLeft)
                {
                    defenderGettingHurtAnimation = AnimationManager.GettingHurtDirection.Up;
                }
            }
        }

        return defenderGettingHurtAnimation;
    }
}
