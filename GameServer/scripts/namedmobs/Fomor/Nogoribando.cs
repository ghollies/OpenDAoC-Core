﻿using System;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS;
using DOL.Events;

namespace DOL.GS
{
	public class Nogoribando : GameEpicBoss
	{
		public Nogoribando() : base() { }

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Nogoribando Initializing...");
		}
		public override int GetResist(eDamageType damageType)
		{
			switch (damageType)
			{
				case eDamageType.Slash: return 60;// dmg reduction for melee dmg
				case eDamageType.Crush: return 60;// dmg reduction for melee dmg
				case eDamageType.Thrust: return 60;// dmg reduction for melee dmg
				default: return 40;// dmg reduction for rest resists
			}
		}
		public override int AttackRange
		{
			get { return 350; }
			set { }
		}
		public override bool HasAbility(string keyName)
		{
			if (IsAlive && keyName == GS.Abilities.CCImmunity)
				return true;

			return base.HasAbility(keyName);
		}
		public override double GetArmorAF(eArmorSlot slot)
		{
			return 700;
		}
		public override double GetArmorAbsorb(eArmorSlot slot)
		{
			// 85% ABS is cap.
			return 0.45;
		}
		public override int MaxHealth
		{
			get { return 20000; }
		}
		public override bool AddToWorld()
		{
			INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(60164519);
			LoadTemplate(npcTemplate);
			Strength = npcTemplate.Strength;
			Dexterity = npcTemplate.Dexterity;
			Constitution = npcTemplate.Constitution;
			Quickness = npcTemplate.Quickness;
			Piety = npcTemplate.Piety;
			Intelligence = npcTemplate.Intelligence;
			Empathy = npcTemplate.Empathy;
			Level = Convert.ToByte(npcTemplate.Level);
			RespawnInterval = ServerProperties.Properties.SET_SI_EPIC_ENCOUNTER_RESPAWNINTERVAL * 60000;//1min is 60000 miliseconds

			Faction = FactionMgr.GetFactionByID(82);
			Faction.AddFriendFaction(FactionMgr.GetFactionByID(82));

			NogoribandoBrain sbrain = new NogoribandoBrain();
			SetOwnBrain(sbrain);
			LoadedFromScript = false;//load from database
			SaveIntoDatabase();
			base.AddToWorld();
			return true;
		}
	}
}
namespace DOL.AI.Brain
{
	public class NogoribandoBrain : StandardMobBrain
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public NogoribandoBrain() : base()
		{
			AggroLevel = 100;
			AggroRange = 600;
			ThinkInterval = 1500;
		}
		public static bool IsPulled = false;
		public static bool IsBig = false;
		public static bool IsSmall = false;
		public static bool IsChangingSize = false;
		public static bool IsInCombat = false;
		public int ChangeSizeToBig(ECSGameTimer timer)
		{
			if (HasAggro && Body.IsAlive)
			{
				IsBig = true;
				IsSmall = false;
				Body.Size = 200;
				Body.Empathy = 300;
				Body.Quickness = 50;
				int _changeSizeToSmallTime = 30000;
				ECSGameTimer _ChangeSizeToSmall = new ECSGameTimer(Body, new ECSGameTimer.ECSTimerCallback(ChangeSizeToSmall), _changeSizeToSmallTime);
				_ChangeSizeToSmall.Start(_changeSizeToSmallTime);
			}
			return 0;
		}
		public int ChangeSizeToSmall(ECSGameTimer timer)
        {
			if (HasAggro && Body.IsAlive)
			{
				INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(60164519);
				IsSmall = true;
				IsBig = false;
				Body.CastSpell(Boss_Haste_Buff, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
				Body.Size = 100;
				Body.Empathy = npcTemplate.Empathy;
				Body.Quickness = npcTemplate.Quickness;
				int _changeSizeToBigTime = 30000;
				ECSGameTimer _ChangeSizeToBig = new ECSGameTimer(Body, new ECSGameTimer.ECSTimerCallback(ChangeSizeToBig), _changeSizeToBigTime);
				_ChangeSizeToBig.Start(_changeSizeToBigTime);
			}
			return 0;
        }
		public override void Think()
		{
			if (!HasAggressionTable())
			{
				//set state to RETURN TO SPAWN
				FSM.SetCurrentState(eFSMStateType.RETURN_TO_SPAWN);
				Body.Health = Body.MaxHealth;
				IsPulled = false;
				IsChangingSize = false;
				IsSmall = false;
				IsBig = false;
				INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(60164519);
				if (IsInCombat == false)
				{
					Body.Size = Convert.ToByte(npcTemplate.Size);
					Body.Empathy = npcTemplate.Empathy;
					Body.Quickness = npcTemplate.Quickness;
					IsInCombat = true;
				}
			}
			if (Body.InCombat && Body.IsAlive && HasAggro)
			{
				if (IsPulled == false)
				{
					foreach (GameNPC npc in Body.GetNPCsInRadius(2500))
					{
						if (npc != null)
						{
							if (npc.IsAlive && npc.PackageID == "NogoribandoBaf")
							{
								AddAggroListTo(npc.Brain as StandardMobBrain);
							}
						}
					}
					IsInCombat = false;
					IsPulled = true;
				}
				GameLiving target = Body.TargetObject as GameLiving;
				if(target != null)
                {
					if(!target.effectListComponent.ContainsEffectForEffectType(eEffect.StrConDebuff))
                    {
						Body.CastSpell(Boss_SC_Debuff, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
					}
                }
				if(IsChangingSize==false)
                {
					int _changeSizeToBigTime = 5000;
					ECSGameTimer _ChangeSizeToBig = new ECSGameTimer(Body, new ECSGameTimer.ECSTimerCallback(ChangeSizeToBig), _changeSizeToBigTime);
					_ChangeSizeToBig.Start(_changeSizeToBigTime);
					IsChangingSize = true;
                }
				if(IsSmall)
                {
					Body.CastSpell(Boss_Haste_Buff, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
				}
			}
			base.Think();
		}
		private Spell m_Boss_SC_Debuff;
		private Spell Boss_SC_Debuff
		{
			get
			{
				if (m_Boss_SC_Debuff == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.RecastDelay = 0;
					spell.Duration = 60;
					spell.Value = 75;
					spell.ClientEffect = 4387;
					spell.Icon = 4387;
					spell.TooltipId = 4387;
					spell.Name = "Vitality Dispersal";
					spell.Range = 1500;
					spell.Radius = 350;
					spell.SpellID = 11837;
					spell.Target = eSpellTarget.Enemy.ToString();
					spell.Type = eSpellType.StrengthConstitutionDebuff.ToString();
					spell.Uninterruptible = true;
					spell.MoveCast = true;
					m_Boss_SC_Debuff = new Spell(spell, 70);
					SkillBase.AddScriptedSpell(GlobalSpellsLines.Mob_Spells, m_Boss_SC_Debuff);
				}
				return m_Boss_SC_Debuff;
			}
		}
		private Spell m_Boss_Haste_Buff;
		private Spell Boss_Haste_Buff
		{
			get
			{
				if (m_Boss_Haste_Buff == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.RecastDelay = 35;
					spell.Duration = 28;
					spell.ClientEffect = 10535;
					spell.Icon = 10535;
					spell.Name = "Nogoribando's Haste";
					spell.Message2 = "{0} begins attacking faster!";
					spell.Message4 = "{0}'s attacks return to normal.";
					spell.TooltipId = 10535;
					spell.Range = 0;
					spell.Value = 50;
					spell.SpellID = 11838;
					spell.Target = "Self";
					spell.Type = eSpellType.CombatSpeedBuff.ToString();
					spell.Uninterruptible = true;
					spell.MoveCast = true;
					m_Boss_Haste_Buff = new Spell(spell, 70);
					SkillBase.AddScriptedSpell(GlobalSpellsLines.Mob_Spells, m_Boss_Haste_Buff);
				}
				return m_Boss_Haste_Buff;
			}
		}

	}
}

