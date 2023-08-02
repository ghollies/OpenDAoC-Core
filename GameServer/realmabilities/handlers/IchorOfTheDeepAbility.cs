using System;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Spells;
using DOL.Events;
using DOL.Database;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
	public class IchorOfTheDeepAbility : TimedRealmAbility
	{
		public IchorOfTheDeepAbility(DBAbility dba, int level) : base(dba, level) { }
		
		private int dmgValue = 400;
		private int duration = 30;
		private GamePlayer caster;
		private SpellLine m_spellline;
		private Spell m_damageSpell;
		private Spell m_rootSpell;

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			caster = living as GamePlayer;
			if (caster == null)
				return;

			// Player must have a target
			if (caster.TargetObject == null)
			{
				caster.Out.SendMessage("You must select a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}

			var target = caster.TargetObject as GameLiving;

			// So they can't use Admins or objects as a target
			if ((target == null || !GameServer.ServerRules.IsAllowedToAttack(caster, target, true)
			    ) && caster.Client.Account.PrivLevel == 1)
			{
				caster.Out.SendMessage("You have an invalid target!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}

			// Can't target self
			if (caster == target)
			{
				caster.Out.SendMessage("You can't attack yourself!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}

			// Target must be in front of the Player
			if (!caster.IsObjectInFront(target, 150))
			{
				caster.Out.SendMessage(target.Name + " is not in view!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}

			// Target must be alive
			if (!target.IsAlive)
			{
				caster.Out.SendMessage(target.Name + " is dead!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}

			// Target must be within range
			if (!caster.IsWithinRadius(caster.TargetObject, 1875))
			{
				caster.Out.SendMessage(caster.TargetObject.Name + " is too far away!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}

			// Target cannot be an ally or friendly
			if (caster != target && caster.Realm == target.Realm)
			{
				caster.Out.SendMessage("You can't attack a member of your realm!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}
			
			if(m_damageSpell == null || m_spellline == null) CreateSpell();
			
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING && m_damageSpell != null)
			{
				switch (Level)
				{
					case 1: m_damageSpell.Damage = 150; m_damageSpell.Duration = 10000; break;
					case 2: m_damageSpell.Damage = 275; m_damageSpell.Duration = 15000; break;
					case 3: m_damageSpell.Damage = 400; m_damageSpell.Duration = 20000; break;
					case 4: m_damageSpell.Damage = 500; m_damageSpell.Duration = 25000; break;
					case 5: m_damageSpell.Damage = 600; m_damageSpell.Duration = 30000; break;
					default: return;
				}				
			}
			else
			{
				switch (Level)
				{
					case 1: dmgValue = 150; duration = 10000; break;
					case 2: dmgValue = 400; duration = 20000; break;
					case 3: dmgValue = 600; duration = 30000; break;
					default: return;
				}
			}
			
			caster.castingComponent.RequestStartCastSpell(m_damageSpell, m_spellline);
			caster.DisableSkill(this, GetReUseDelay(Level));
		}
		
		
		public virtual void CreateSpell()
		{
			m_spellline = new SpellLine("RAs", "RealmAbilities", "RealmAbilities", true);
			var damageSpell = new DBSpell();
			damageSpell.Name = "Ichor of the Deep";
			damageSpell.Icon = 7029;
			damageSpell.ClientEffect = 7029;
			damageSpell.Damage = dmgValue;
			damageSpell.DamageType = 11;
			damageSpell.Target = "enemy";
			damageSpell.Radius = 500;
			damageSpell.Type = eSpellType.DamageSpeedDecreaseNoVariance.ToString();
			damageSpell.Value = 99;
			damageSpell.Duration = duration;
			damageSpell.Pulse = 0;
			damageSpell.PulsePower = 0;
			damageSpell.Power = 0;
			damageSpell.CastTime = 2;
			damageSpell.EffectGroup = 0;
			damageSpell.Range = 1000;
			damageSpell.Uninterruptible = true;
			m_damageSpell = new Spell(damageSpell, 0); // make spell level 0 so it bypasses the spec level adjustment code
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}
	}
}
