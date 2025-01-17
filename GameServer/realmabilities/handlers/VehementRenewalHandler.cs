using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Vehement Renewal, healing all but caster in 2000 range
	/// </summary>
	public class VehementRenewalAbility : TimedRealmAbility
	{
		public VehementRenewalAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED | NOTINGROUP)) return;

			int heal = 0;
			
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (Level)
				{
					case 1: heal = 500; break;
					case 2: heal = 875; break;
					case 3: heal = 1250; break;
					case 4: heal = 1750; break;
					case 5: heal = 2250; break;
				}
			}
			else
			{
				switch (Level)
				{
					case 1: heal = 375; break;
					case 2: heal = 750; break;
					case 3: heal = 1500; break;
				}
			}

			bool used = false;

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				if (player.Group == null)
				{
					player.Out.SendMessage("You are not in a group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				SendCasterSpellEffectAndCastMessage(living, 7017, true);

				foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
				{
					if (p == player) continue;
					if (!player.IsWithinRadius( p, 2000 )) continue;
					if (!p.IsAlive) continue;
					int healed = p.ChangeHealth(living, eHealthChangeType.Spell, heal);
					if (healed > 0)
						used = true;

					if (healed > 0) p.Out.SendMessage(player.Name + " heals your for " + healed + " hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					if (heal > healed)
					{
						p.Out.SendMessage("You are fully healed.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
				}
			}
			if (used) DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}

		public override void AddEffectsInfo(IList<string> list)
		{
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				list.Add("Level 1: Value: 500");
				list.Add("Level 2: Value: 875");
				list.Add("Level 3: Value: 1250");
				list.Add("Level 4: Value: 1750");
				list.Add("Level 5: Value: 2250");
				list.Add("");
				list.Add("Target: Group");
				list.Add("Casting time: instant");
			}
			else
			{
				list.Add("Level 1: Value: 375");
				list.Add("Level 2: Value: 750");
				list.Add("Level 3: Value: 1500");
				list.Add("");
				list.Add("Target: Group");
				list.Add("Casting time: instant");
			}
		}
	}
}