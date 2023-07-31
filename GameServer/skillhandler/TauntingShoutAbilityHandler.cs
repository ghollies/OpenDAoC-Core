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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS;
using DOL.Language;

namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Handler for Fury shout
    /// </summary>
    [SkillHandlerAttribute(Abilities.TauntingShout)]
    public class TauntingShoutAbilityHandler : IAbilityActionHandler
    {
	    private DBSpell m_dbspell;
	    private Spell m_spell;
	    private SpellLine m_spellline;
	    
	    /// <summary>
	    /// The ability reuse time in seconds
	    /// </summary>
	    protected const int REUSE_TIMER = 20; 
		
		public int SpellID
		{
			get
			{
				return 14377;
			}
		}

		public void Execute(Ability ab, GamePlayer player)
		{
			if (!player.IsAlive)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsMezzed)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseMezzed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsStunned)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseStunned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsSitting)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseStanding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			
			
			if(m_spell == null) CreateSpell();
			player.castingComponent.RequestStartCastSpell(m_spell, m_spellline);
			
			player.DisableSkill(ab, REUSE_TIMER * 1000);
		}

		public virtual void CreateSpell()
		{
			m_dbspell = new DBSpell();
			m_dbspell.Name = "Taunting Shout";
			m_dbspell.Icon = SpellID;
			m_dbspell.ClientEffect = SpellID;
			m_dbspell.Damage = 0;
			m_dbspell.DamageType = 10;
			m_dbspell.Target = "cone";
			m_dbspell.Radius = 100;
			m_dbspell.Type = eSpellType.Taunt.ToString();
			m_dbspell.Value = 75;
			m_dbspell.Duration = 0;
			m_dbspell.Pulse = 0;
			m_dbspell.PulsePower = 0;
			m_dbspell.Power = 0;
			m_dbspell.CastTime = 0;
			m_dbspell.EffectGroup = 0; // stacks with other damage adds
			m_dbspell.Range = 1000;
			m_dbspell.Message2 = "{0} becomes enraged!";
			m_spell = new Spell(m_dbspell, 0); // make spell level 0 so it bypasses the spec level adjustment code
			m_spellline = new SpellLine("RAs", "RealmAbilities", "RealmAbilities", true);
		}
    }
}
