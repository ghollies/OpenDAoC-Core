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
/*
 * Made by Biceps
 * TODO: DD proc
 */
using System;
using System.Collections;
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Shears strength buff 
	/// </summary>
	[SpellHandlerAttribute("StrengthShear")]
	public class StrengthShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "StrengthBuff"; } }
		public override string DelveSpellType { get { return "Strength"; } }

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
	        if (target.HasAbility(Abilities.VampiirStrength))
	        {
		        MessageToCaster("The target's connection to their enhancement is too strong for you to remove.", eChatType.CT_Spell);
		        return;
	        }
	        base.ApplyEffectOnTarget(target, effectiveness);
        }

		// constructor
		public StrengthShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Shears dexterity buff
	/// </summary>
	[SpellHandlerAttribute("DexterityShear")]
	public class DexterityShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "DexterityBuff"; } }
		public override string DelveSpellType { get { return "Dexterity"; } }
		
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (target.HasAbility(Abilities.VampiirDexterity))
			{
				MessageToCaster("The target's connection to their enhancement is too strong for you to remove.", eChatType.CT_Spell);
				return;
			}
			base.ApplyEffectOnTarget(target, effectiveness);
		}
		
		// constructor
		public DexterityShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Shears constitution buff
	/// </summary>
	[SpellHandlerAttribute("ConstitutionShear")]
	public class ConstitutionShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "ConstitutionBuff"; } }
		public override string DelveSpellType { get { return "Constitution"; } }
		
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (target.HasAbility(Abilities.VampiirConstitution))
			{
				MessageToCaster("The target's connection to their enhancement is too strong for you to remove.", eChatType.CT_Spell);
				return;
			}
			base.ApplyEffectOnTarget(target, effectiveness);
		}
		
		// constructor
		public ConstitutionShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Shears acuity buff
	/// </summary>
	[SpellHandlerAttribute("AcuityShear")]
	public class AcuityShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "AcuityBuff"; } }
		public override string DelveSpellType { get { return "Acuity"; } }
		// constructor
		public AcuityShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Shears str/con buff
	/// </summary>
	[SpellHandlerAttribute("StrengthConstitutionShear")]
	public class StrengthConstitutionShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "StrengthConstitutionBuff"; } }
		public override string DelveSpellType { get { return "Str/Con"; } }
		// constructor
		public StrengthConstitutionShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Shears dex/qui buff
	/// </summary>
	[SpellHandlerAttribute("DexterityQuicknessShear")]
	public class DexterityQuicknessShear : AbstractBuffShear
	{
		public override string ShearSpellType { get	{ return "DexterityQuicknessBuff"; } }
		public override string DelveSpellType { get { return "Dex/Qui"; } }
		// constructor
		public DexterityQuicknessShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Base class for all buff shearing spells
	/// </summary>
	public abstract class AbstractBuffShear : SpellHandler
	{
		/// <summary>
		/// The spell type to shear
		/// </summary>
		public abstract string ShearSpellType { get; }

		/// <summary>
		/// The spell type shown in delve info
		/// </summary>
		public abstract string DelveSpellType { get; }

		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			if (Caster == null)
				return;
			
			if (target == null) 
				return;

			if (Caster == target)
			{
				MessageToCaster("You can't attack yourself!", eChatType.CT_Spell);
				return;
			}
			
			if (target is not GamePlayer or GameNPC || (target is GamePlayer playerTarget && playerTarget.Client.Account.PrivLevel > 1))
			{
				MessageToCaster("You can't attack this target!", eChatType.CT_Spell);
				return;
			}

			if (target.Realm == Caster.Realm)
			{
				MessageToCaster("You can't attack a member of your realm!", eChatType.CT_Spell);
				return;
			}

			if (EffectListService.GetSpellEffectOnTarget(target, eEffect.Shade) != null)
			{
				MessageToCaster("You can't attack this target!", eChatType.CT_Spell);
				return;
			}
			
			if (!target.IsAlive)
			{
				MessageToCaster("Your target is already dead!", eChatType.CT_Spell);
				return;
			}
			
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// execute non duration spell effect on target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target.ObjectState != GameObject.eObjectState.Active) 
			{
				MessageToCaster("You can't attack this target!", eChatType.CT_Spell);
				return;
			}

			base.OnDirectEffect(target, effectiveness);
			target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
			
			ECSGameSpellEffect mezEffect = EffectListService.GetSpellEffectOnTarget(target, eEffect.Mez);
			
			if (mezEffect != null)
			{
				EffectService.RequestCancelEffect(mezEffect);
			}
			
			if (target is GameNPC)
			{
				GameNPC npc = (GameNPC)target;
				if (npc.Brain is IOldAggressiveBrain aggroBrain)
					aggroBrain.AddToAggroList(Caster, 1);
			}

			// Check for spell
			foreach (ECSGameSpellEffect effect in target.effectListComponent.GetSpellEffects())
			{
				if (effect.Owner == effect.SpellHandler.Caster)
				{
					MessageToCaster("You can't attack yourself!", eChatType.CT_Spell);
					return;
				}

				if (effect.SpellHandler.Spell.Value > Spell.Value)
				{
					SendEffectAnimation(target, 0, false, 0);
					MessageToCaster("The target's connection to their enhancement is too strong for you to remove.", eChatType.CT_SpellResisted);
					break;
				}
				
				if (effect.SpellHandler.Spell.SpellType.ToString() == ShearSpellType && effect.SpellHandler.Spell.IsShearable)
				{
					SendEffectAnimation(target, 0, false, 1);
					EffectService.RequestCancelEffect(effect);
					MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
					MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
				}
			}

			SendEffectAnimation(target, 0, false, 0);
			MessageToCaster("No enhancement of that type found on the target.", eChatType.CT_SpellResisted);
		}

		/// <summary>
		/// When spell was resisted
		/// </summary>
		/// <param name="target">the target that resisted the spell</param>
		protected override void OnSpellResisted(GameLiving target)
		{
			base.OnSpellResisted(target);
			if (Spell.Damage == 0 && Spell.CastTime == 0)
				target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo 
		{
			get 
			{
				/*
				<Begin Info: Potency Whack>
				Function: buff shear
 
				Destroys a positive enhancement on the target.
 
				Type: Str/Con
				Maximum strength of buffs removed: 150
				Target: Enemy realm players and controlled pets only
				Range: 1500
				Power cost: 12
				Casting time:      2.0 sec
				Damage: Body
 
				<End Info>
				*/

				var list = new List<string>();

				list.Add("Function: " + (Spell.SpellType.ToString() == "" ? "(not implemented)" : Spell.SpellType.ToString()));
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				list.Add("Type: " + DelveSpellType);
				list.Add("Maximum strength of buffs removed: " + Spell.Value);
				if(Spell.Range != 0) list.Add("Range: " + Spell.Range);
				if(Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if(Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
				if(Spell.DamageType != eDamageType.Natural) list.Add("Damage: " + GlobalConstants.DamageTypeToName(Spell.DamageType));

				return list;
			}
		}

		// constructor
		public AbstractBuffShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandlerAttribute("RandomBuffShear")]
	public class RandomBuffShear : SpellHandler
	{
		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			if (Caster == null)
				return;
			
			if (target == null) 
				return;

			if (Caster == target)
			{
				MessageToCaster("You can't attack yourself!", eChatType.CT_Spell);
				return;
			}
			
			if (target is not GamePlayer or GameNPC || (target is GamePlayer playerTarget && playerTarget.Client.Account.PrivLevel > 1))
			{
				MessageToCaster("You can't attack this target!", eChatType.CT_Spell);
				return;
			}

			if (target.Realm == Caster.Realm)
			{
				MessageToCaster("You can't attack a member of your realm!", eChatType.CT_Spell);
				return;
			}

			if (EffectListService.GetSpellEffectOnTarget(target, eEffect.Shade) != null)
			{
				MessageToCaster("You can't attack this target!", eChatType.CT_Spell);
				return;
			}
			
			if (!target.IsAlive)
			{
				MessageToCaster("Your target is already dead!", eChatType.CT_Spell);
				return;
			}
			
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// execute non duration spell effect on target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target.ObjectState != GameObject.eObjectState.Active) 
			{
				MessageToCaster("You can't attack this target!", eChatType.CT_Spell);
				return;
			}

			base.OnDirectEffect(target, effectiveness);
			target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
			
			ECSGameSpellEffect mezEffect = EffectListService.GetSpellEffectOnTarget(target, eEffect.Mez);
			
			if (mezEffect != null)
			{
				EffectService.RequestCancelEffect(mezEffect);
			}
			
			if (target is GameNPC)
			{
				GameNPC npc = (GameNPC)target;
				if (npc.Brain is IOldAggressiveBrain aggroBrain)
					aggroBrain.AddToAggroList(Caster, 1);
			}

			// Check for spell
			foreach (ECSGameSpellEffect effect in target.effectListComponent.GetSpellEffects())
			{
				if (effect.Owner == effect.SpellHandler.Caster)
				{
					MessageToCaster("You can't attack yourself!", eChatType.CT_Spell);
					return;
				}

				if (effect.SpellHandler.Spell.Value > Spell.Value)
				{
					SendEffectAnimation(target, 0, false, 0);
					MessageToCaster("The target's connection to their enhancement is too strong for you to remove.", eChatType.CT_SpellResisted);
					break;
				}
				
				SendEffectAnimation(target, 0, false, 1);
				EffectService.RequestCancelEffect(effect);
				MessageToCaster("Your spell rips away some of your target's enhancing magic.", eChatType.CT_Spell);
				MessageToLiving(target, "Some of your enhancing magic has been ripped away by a spell!", eChatType.CT_Spell);
				return;
			}

			SendEffectAnimation(target, 0, false, 0);
			MessageToCaster("No enhancement of that type found on the target.", eChatType.CT_SpellResisted);
		}

		private static Type[] buffs = new Type[] { typeof(AcuityBuff), typeof(StrengthBuff), typeof(DexterityBuff), typeof(ConstitutionBuff), typeof(StrengthConBuff), typeof(DexterityQuiBuff),
			typeof(ArmorFactorBuff),typeof(ArmorAbsorptionBuff),typeof(HealthRegenSpellHandler),typeof(CombatSpeedBuff),typeof(PowerRegenSpellHandler),typeof(UninterruptableSpellHandler),typeof(WeaponSkillBuff),typeof(DPSBuff),typeof(EvadeChanceBuff),typeof(ParryChanceBuff),
			typeof(ColdResistBuff),typeof(EnergyResistBuff),typeof(CrushResistBuff),typeof(ThrustResistBuff),typeof(SlashResistBuff),typeof(MatterResistBuff),typeof(BodyResistBuff),typeof(HeatResistBuff),typeof(SpiritResistBuff),typeof(BodySpiritEnergyBuff),typeof(HeatColdMatterBuff),typeof(CrushSlashThrustBuff),
			typeof(EnduranceRegenSpellHandler),typeof(DamageAddSpellHandler),typeof(DamageShieldSpellHandler) };
		// constructor
		public RandomBuffShear(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}