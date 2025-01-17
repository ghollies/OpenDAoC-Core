/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
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
using System.Collections;
using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	public class AtlasOF_MasteryofConcentration : TimedRealmAbility
	{
		public AtlasOF_MasteryofConcentration(DBAbility dba, int level) : base(dba, level) { }
		public const Int32 Duration = 30000; // 30s in ms
		public override int GetReUseDelay(int level) { return 600; } // 10 mins
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			GamePlayer caster = living as GamePlayer;

			if (caster == null)
				return;

			EffectListService.TryCancelFirstEffectOfTypeOnTarget(caster, eEffect.MasteryOfConcentration);

			SendCasterSpellEffectAndCastMessage(living, 7007, true);
			foreach (GamePlayer player in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (caster.IsWithinRadius(player, WorldMgr.INFO_DISTANCE))
				{
					if (player == caster)
					{
						player.MessageToSelf("You cast " + this.Name + "!", eChatType.CT_Spell);
						player.MessageToSelf("You become steadier in your casting abilities!", eChatType.CT_Spell);
					}
					else
					{
						player.MessageFromArea(caster, caster.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
						player.Out.SendMessage(caster.Name + "'s castings have perfect poise!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
			}

			DisableSkill(living);

			new MasteryOfConcentrationECSEffect(new ECSGameEffectInitParams(caster, Duration, 1));
		}

        public virtual int GetAmountForLevel(int level)
        {
            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (level)
                {
                    case 1: return 25;
                    case 2: return 35;
                    case 3: return 50;
                    case 4: return 60;
                    case 5: return 75;
                }
            }
            else
            {
                switch (level)
                {
                    case 1: return 25;
                    case 2: return 50;
                    case 3: return 75;
                }
            }
            return 25;
        }
    }
}
