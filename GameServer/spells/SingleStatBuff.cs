/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PlayerClass;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Buffs a single stat,
    /// considered as a baseline buff (regarding the bonuscategories on statproperties)
    /// </summary>
    public abstract class SingleStatBuff : PropertyChangingSpell
    {
        public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }

        protected bool m_isShearable = false;
        
        protected override void SendUpdates(GameLiving target)
        {
            target.UpdateHealthManaEndu();
        }

        public override void CreateECSEffect(ECSGameEffectInitParams initParams)
        {
            new StatBuffECSEffect(initParams);
        }

        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            int specLevel = Caster.GetModifiedSpecLevel(m_spellLine.Spec);

            if (SpellLine.KeyName is GlobalSpellsLines.Potions_Effects or GlobalSpellsLines.Item_Effects)
                effectiveness = 1.0;
            else if (Spell.Level <= 0)
                effectiveness = 1.0;
            else if (Caster is GamePlayer playerCaster)
            {
			    if (playerCaster.CharacterClass.ID != (int)eCharacterClass.Savage && Spell.Target != "Enemy")
                {
                    if (playerCaster.CharacterClass.ClassType != eClassType.ListCaster)
                    {
                        effectiveness = 0.75; // This section is for self bulfs, cleric buffs etc.
                        effectiveness += (specLevel - 1.0) * 0.5 / Spell.Level;
                        effectiveness = Math.Max(0.75, effectiveness);
                        effectiveness = Math.Min(1.25, effectiveness);
                    }
                }
                else if (Spell.Target == "Enemy")
                {
				    effectiveness = 0.75; // This section is for list casters stat debuffs.
				    if (playerCaster.CharacterClass.ClassType == eClassType.ListCaster)
				    {
                        effectiveness += (specLevel - 1.0) * 0.5 / Spell.Level;
                        effectiveness = Math.Max(0.75, effectiveness);
                        effectiveness = Math.Min(1.25, effectiveness);
                        effectiveness *= 1.0 + m_caster.GetModified(eProperty.DebuffEffectivness) * 0.01;

                        if (playerCaster.UseDetailedCombatLog && m_caster.GetModified(eProperty.DebuffEffectivness) > 0)
                            playerCaster.Out.SendMessage($"debuff effectiveness: {m_caster.GetModified(eProperty.DebuffEffectivness)}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
                    }
				    else
				    {
					    effectiveness = 1.0; // Non list casters debuffs. Reaver curses, Champ debuffs etc.
					    effectiveness *= 1.0 + m_caster.GetModified(eProperty.DebuffEffectivness) * 0.01;
                    }
			    }
            }
            else if (Caster is NecromancerPet necroPetCaster && necroPetCaster.Owner is GamePlayer playerOwner && Spell.Target == "Enemy")
            {
                specLevel = playerOwner.GetModifiedSpecLevel(m_spellLine.Spec);

                effectiveness = 0.75; // This section is for list casters stat debuffs.
                effectiveness += (specLevel - 1.0) * 0.5 / Spell.Level;
                effectiveness = Math.Max(0.75, effectiveness);
                effectiveness = Math.Min(1.25, effectiveness);
                effectiveness *= 1.0 + playerOwner.GetModified(eProperty.DebuffEffectivness) * 0.01;                

                if (Spell.SpellType == eSpellType.ArmorFactorDebuff)
                    effectiveness *= 1 + target.GetArmorAbsorb(eArmorSlot.TORSO);

                if (playerOwner.UseDetailedCombatLog && m_caster.GetModified(eProperty.DebuffEffectivness) > 0)
                    playerOwner.Out.SendMessage($"debuff effectiveness: {m_caster.GetModified(eProperty.DebuffEffectivness)}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
            }
            else
                effectiveness = 1.0;

            if (Spell.Target != "Enemy")
            {
                effectiveness *= 1.0 + m_caster.GetModified(eProperty.BuffEffectiveness) * 0.01;

                if (Caster is GamePlayer gamePlayer && gamePlayer.UseDetailedCombatLog && m_caster.GetModified(eProperty.BuffEffectiveness) > 0 )
                    gamePlayer.Out.SendMessage($"buff effectiveness: {m_caster.GetModified(eProperty.BuffEffectiveness)}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
            }
            else
                effectiveness *= GetCritBonus();

            target.StartHealthRegeneration();
            base.ApplyEffectOnTarget(target, effectiveness);
        }

        /// <summary>
        /// Determines wether this spell is compatible with given spell
        /// and therefore overwritable by better versions
        /// spells that are overwritable cannot stack
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            if (Spell.EffectGroup != 0 || compare.Spell.EffectGroup != 0)
                return Spell.EffectGroup == compare.Spell.EffectGroup;
            if (!base.IsOverwritable(compare))
                return false;
            if (Spell.Duration > 0 && compare.Concentration > 0)
                return compare.Spell.Value >= Spell.Value;
            return compare.SpellHandler.SpellLine.IsBaseLine == SpellLine.IsBaseLine;
        }

        private double GetCritBonus()
        {
            double critMod = 1.0;
            int critChance = Caster.DotCriticalChance;

            if (critChance <= 0)
                return critMod;

            GamePlayer playerCaster = Caster as GamePlayer;

            if (playerCaster?.UseDetailedCombatLog == true && critChance > 0)
                playerCaster.Out.SendMessage($"Debuff crit chance: {Caster.DotCriticalChance}", eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);

            if (Util.Chance(critChance))
            {                    
                critMod *= 1 + Util.Random(1, 10) * 0.1;
                playerCaster?.Out.SendMessage($"Your {Spell.Name} critically debuffs the enemy for {Math.Round(critMod - 1,3) * 100}% additional effect!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
            }

            return critMod;
        }

        // constructor
        protected SingleStatBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Str stat baseline buff
    /// </summary>
    [SpellHandlerAttribute("StrengthBuff")]
    public class StrengthBuff : SingleStatBuff
    {
        protected bool m_isShearable = true;
        
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirStrength))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }
        public override eProperty Property1 { get { return eProperty.Strength; } }

        // constructor
        public StrengthBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Dex stat baseline buff
    /// </summary>
    [SpellHandlerAttribute("DexterityBuff")]
    public class DexterityBuff : SingleStatBuff
    {
        protected bool m_isShearable = true;
        
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirDexterity))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }
        public override eProperty Property1 { get { return eProperty.Dexterity; } }

        // constructor
        public DexterityBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Con stat baseline buff
    /// </summary>
    [SpellHandlerAttribute("ConstitutionBuff")]
    public class ConstitutionBuff : SingleStatBuff
    {
        protected bool m_isShearable = true;
        
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirConstitution))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }
        public override eProperty Property1 { get { return eProperty.Constitution; } }

        // constructor
        public ConstitutionBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Armor factor buff
    /// </summary>
    [SpellHandlerAttribute("ArmorFactorBuff")]
    public class ArmorFactorBuff : SingleStatBuff
    {
        public override eBuffBonusCategory BonusCategory1
        {
            get
            {
                if (Caster is GamePlayer c && (c.CharacterClass is ClassRanger || c.CharacterClass is ClassHunter) && (SpellLine.KeyName.ToLower().Equals("beastcraft") || SpellLine.KeyName.ToLower().Equals("pathfinding"))) return eBuffBonusCategory.BaseBuff;
            	if (Spell.Target.Equals("Self", StringComparison.OrdinalIgnoreCase)) return eBuffBonusCategory.Other; // no caps for self buffs
                if (m_spellLine.IsBaseLine) return eBuffBonusCategory.BaseBuff; // baseline cap
                return eBuffBonusCategory.Other; // no caps for spec line buffs
            }
        }
        public override eProperty Property1 { get { return eProperty.ArmorFactor; } }

        // constructor
        public ArmorFactorBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Armor Absorption buff
    /// </summary>
    [SpellHandlerAttribute("ArmorAbsorptionBuff")]
    public class ArmorAbsorptionBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.ArmorAbsorption; } }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
        }

        // constructor
        public ArmorAbsorptionBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Combat speed buff
    /// </summary>
    [SpellHandlerAttribute("CombatSpeedBuff")]
    public class CombatSpeedBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.MeleeSpeed; } }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
        }

        // constructor
        public CombatSpeedBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    
    /// <summary>
    /// Haste Buff stacking with other Combat Speed Buff
    /// </summary>
    [SpellHandlerAttribute("HasteBuff")]
    public class HasteBuff : CombatSpeedBuff
    {
        // constructor
        public HasteBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Celerity Buff stacking with other Combat Speed Buff
    /// </summary>
    [SpellHandlerAttribute("CelerityBuff")]
    public class CelerityBuff : CombatSpeedBuff
    {
        // constructor
        public CelerityBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Fatigue reduction buff
    /// </summary>
    [SpellHandlerAttribute("FatigueConsumptionBuff")]
    public class FatigueConsumptionBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.FatigueConsumption; } }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
        }

        // constructor
        public FatigueConsumptionBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Melee damage buff
    /// </summary>
    [SpellHandlerAttribute("MeleeDamageBuff")]
    public class MeleeDamageBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.MeleeDamage; } }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
        }

        // constructor
        public MeleeDamageBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Mesmerize duration buff
    /// </summary>
    [SpellHandlerAttribute("MesmerizeDurationBuff")]
    public class MesmerizeDurationBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.MesmerizeDurationReduction; } }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
        }

        // constructor
        public MesmerizeDurationBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }


    /// <summary>
    /// Acuity buff
    /// </summary>
    [SpellHandlerAttribute("AcuityBuff")]
    public class AcuityBuff : SingleStatBuff
    {
        protected bool m_isShearable = true;
        
        public override eProperty Property1 { get { return eProperty.Acuity; } }

        // constructor
        public AcuityBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Quickness buff
    /// </summary>
    [SpellHandlerAttribute("QuicknessBuff")]
    public class QuicknessBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.Quickness; } }

        // constructor
        public QuicknessBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// DPS buff
    /// </summary>
    [SpellHandlerAttribute("DPSBuff")]
    public class DPSBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.DPS; } }

        // constructor
        public DPSBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Evade chance buff
    /// </summary>
    [SpellHandlerAttribute("EvadeBuff")]
    public class EvadeChanceBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.EvadeChance; } }

        // constructor
        public EvadeChanceBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    /// <summary>
    /// Parry chance buff
    /// </summary>
    [SpellHandlerAttribute("ParryBuff")]
    public class ParryChanceBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.ParryChance; } }

        // constructor
        public ParryChanceBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    /// <summary>
    /// WeaponSkill buff
    /// </summary>
    [SpellHandlerAttribute("WeaponSkillBuff")]
    public class WeaponSkillBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.WeaponSkill; } }

        // constructor
        public WeaponSkillBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    /// <summary>
    /// Stealth Skill buff
    /// </summary>
    [SpellHandlerAttribute("StealthSkillBuff")]
    public class StealthSkillBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.Skill_Stealth; } }

        // constructor
        public StealthSkillBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    /// <summary>
    /// To Hit buff
    /// </summary>
    [SpellHandlerAttribute("ToHitBuff")]
    public class ToHitSkillBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.ToHitBonus; } }

        // constructor
        public ToHitSkillBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    /// <summary>
    /// Magic Resists Buff
    /// </summary>
    [SpellHandlerAttribute("MagicResistsBuff")]
    public class MagicResistsBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.MagicAbsorption; } }

        // constructor
        public MagicResistsBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    [SpellHandlerAttribute("StyleAbsorbBuff")]
    public class StyleAbsorbBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.StyleAbsorb; } }
        public StyleAbsorbBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    [SpellHandlerAttribute("ExtraHP")]
    public class ExtraHP : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.ExtraHP; } }
        public ExtraHP(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Paladin Armor factor buff
    /// </summary>
    [SpellHandlerAttribute("PaladinArmorFactorBuff")]
    public class PaladinArmorFactorBuff : SingleStatBuff
    {
        public override eBuffBonusCategory BonusCategory1
        {
            get
            {
                if (Spell.Target == "Self") return eBuffBonusCategory.Other; // no caps for self buffs
                if (m_spellLine.IsBaseLine) return eBuffBonusCategory.BaseBuff; // baseline cap
                return eBuffBonusCategory.Other; // no caps for spec line buffs
            }
        }
        public override eProperty Property1 { get { return eProperty.ArmorFactor; } }

        // constructor
        public PaladinArmorFactorBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    [SpellHandler("FlexibleSkillBuff")]
    public class FlexibleSkillBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.Skill_Flexible_Weapon; } }
        public FlexibleSkillBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    [SpellHandler("ResiPierceBuff")]
    public class ResiPierceBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.ResistPierce; } }
        public ResiPierceBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}