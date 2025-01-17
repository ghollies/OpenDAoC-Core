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
using System.Linq;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.API;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.GS.Styles;
using static DOL.GS.Area;

namespace DOL.GS.ServerRules
{
	/// <summary>
	/// Set of rules for "normal" server type.
	/// </summary>
	[ServerRules(eGameServerType.GST_Normal)]
	public class NormalServerRules : AbstractServerRules
	{

        PvPServerRules PvPServerRules { get; set; }
		override public void Initialize()
        {
			base.Initialize();
            PvPServerRules = (PvPServerRules)ScriptMgr.CreateServerRules(eGameServerType.GST_PvP);
        }
        public override string RulesDescription()
		{
			return "standard Normal server rules";
		}

		/// <summary>
		/// Invoked on NPC death and deals out
		/// experience/realm points if needed
		/// </summary>
		/// <param name="killedNPC">npc that died</param>
		/// <param name="killer">killer</param>
		public override void OnNPCKilled(GameNPC killedNPC, GameObject killer)
		{
			base.OnNPCKilled(killedNPC, killer); 	
		}

		public override bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
		{
			// Allow pvp ruleset in Avalon/Goth
			if (attacker.CurrentRegionID == 51)
			{
				return PvPServerRules.IsAllowedToAttack(attacker, defender, quiet);
            }
			if (!base.IsAllowedToAttack(attacker, defender, quiet))
				return false;

			// if controlled NPC - do checks for owner instead
			if (attacker is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)attacker).Brain as IControlledBrain;
				if (controlled != null)
				{
                    attacker = controlled.GetLivingOwner();
					quiet = true; // silence all attacks by controlled npc
				}
			}
			if (defender is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)defender).Brain as IControlledBrain;
				if (controlled != null)
                    defender = controlled.GetLivingOwner();
			}

			//"You can't attack yourself!"
			if(attacker == defender)
			{
				if (quiet == false) MessageToLiving(attacker, "You can't attack yourself!");
				return false;
			}

			if(attacker is GamePlayer atkPl && defender is GamePlayer defPl
				&& atkPl.IsPvP && defPl.IsPvP)
            {
				return true;
            }

			//Don't allow attacks on same realm members on Normal Servers
			if (attacker.Realm == defender.Realm && !(attacker is GamePlayer && ((GamePlayer)attacker).DuelTarget == defender))
			{
				// allow confused mobs to attack same realm
				if (attacker is GameNPC && (attacker as GameNPC).IsConfused)
					return true;

				if (attacker.Realm == 0)
				{
					return FactionMgr.CanLivingAttack(attacker, defender);
				}

				// Ariadolis FFA zone
				// Allow players on the same realm to attack each other if they are in a FFA zone, and they are not in the same group
				if (attacker is GamePlayer atackerPlayer && defender is GamePlayer defenderPlayer)
				{
					if (FreeForAllArea.isPlayerInFFAArea(atackerPlayer) && FreeForAllArea.isPlayerInFFAArea(defenderPlayer))
					{
						//check group
						if (atackerPlayer.Group != null && atackerPlayer.Group.IsInTheGroup(defenderPlayer))
						{
							if (!quiet) MessageToLiving(atackerPlayer, "You can't attack your group members.");
							return false;
						}
						else
						{
							return true;
						}
					}
				}
				if (quiet == false) MessageToLiving(attacker, "You can't attack a member of your realm!");
				return false;
			}

			return true;
		}

		public override bool IsSameRealm(GameLiving source, GameLiving target, bool quiet)
		{
            if (source.CurrentRegionID == 51)
            {
                return PvPServerRules.IsSameRealm(source, target, quiet);
            }
            if (source == null || target == null) 
				return false;

			// if controlled NPC - do checks for owner instead
			if (source is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)source).Brain as IControlledBrain;
				if (controlled != null)
				{
                    source = controlled.GetLivingOwner();
					quiet = true; // silence all attacks by controlled npc
				}
			}
			if (target is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)target).Brain as IControlledBrain;
				if (controlled != null)
                    target = controlled.GetLivingOwner();
			}

			if (source == target)
				return true;

			// clients with priv level > 1 are considered friendly by anyone
			if(target is GamePlayer && ((GamePlayer)target).Client.Account.PrivLevel > 1) return true;
			// checking as a gm, targets are considered friendly
			if (source is GamePlayer && ((GamePlayer)source).Client.Account.PrivLevel > 1) return true;

			if (target is GamePlayer tPl && source is GamePlayer sPl
				&& tPl.IsPvP && sPl.IsPvP)
				return false;

			//Peace flag NPCs are same realm
			if (target is GameNPC)
				if ((((GameNPC)target).Flags & GameNPC.eFlags.PEACE) != 0)
					return true;

			if (source is GameNPC)
				if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
					return true;


			// Ariadolis FFA zone
			// Allow players on the same realm to attack each other if they are in a FFA zone, and they are not in the same group
			if (source is GamePlayer atackerPlayer && target is GamePlayer defenderPlayer)
			{
				if (FreeForAllArea.isPlayerInFFAArea(atackerPlayer) && FreeForAllArea.isPlayerInFFAArea(defenderPlayer))
				{
					//check group
					if (atackerPlayer.Group != null && atackerPlayer.Group.IsInTheGroup(defenderPlayer))
					{
						if (!quiet) MessageToLiving(atackerPlayer, "You can't attack your group members.");
						return false;
					}
					else
					{
						return true;
					}
				}
			}
			if (source.Realm != target.Realm)
			{
				if(quiet == false) MessageToLiving(source, target.GetName(0, true) + " is not a member of your realm!");
				return false;
			}
			return true;
		}

		public override bool IsAllowedCharsInAllRealms(GameClient client)
		{
			if (client.Account.PrivLevel > 1)
				return true;
			if (ServerProperties.Properties.ALLOW_ALL_REALMS)
				return true;
			return false;
		}

		public override bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet)
		{
            if (source.CurrentRegionID == 51 && target.CurrentRegionID == 51)
            {
                return PvPServerRules.IsAllowedToGroup(source, target, quiet);
            }
            if (source == null || target == null) return false;
			
			if (source.Realm != target.Realm)
			{
				if(quiet == false) MessageToLiving(source, "You can't group with a player from another realm!");
				return false;
			}

			if (source?.CurrentRegionID == 27 || target?.CurrentRegionID == 27)
            {
                if (Properties.EVENT_PVP) { return false; }
            }

			if (Properties.EVENT_CROSS_REALM_GROUPS) return true;

			return true;
		}


		public override bool IsAllowedToJoinGuild(GamePlayer source, Guild guild)
		{
			if (source == null) 
				return false;

			if (ServerProperties.Properties.ALLOW_CROSS_REALM_GUILDS == false && guild.Realm != eRealm.None && source.Realm != guild.Realm)
			{
				return false;
			}

			return true;
		}

		public override bool IsAllowedToTrade(GameLiving source, GameLiving target, bool quiet)
		{
            if (source.CurrentRegionID == 51)
            {
                return PvPServerRules.IsAllowedToTrade(source, target, quiet);
            }
            if (source == null || target == null) return false;
			
			// clients with priv level > 1 are allowed to trade with anyone
			if(source is GamePlayer && target is GamePlayer)
			{
				if ((source as GamePlayer).Client.Account.PrivLevel > 1 || (target as GamePlayer).Client.Account.PrivLevel > 1)
					return true;
			}
			
			if((source as GamePlayer).NoHelp)
			{
				if(quiet == false) MessageToLiving(source, "You have renounced to any kind of help!");
				if(quiet == false) MessageToLiving(target, "This player has chosen to receive no help!");
				return false;
			}
			
			if((target as GamePlayer).NoHelp)
			{
				if(quiet == false) MessageToLiving(target, "You have renounced to any kind of help!");
				if(quiet == false) MessageToLiving(source, "This player has chosen to receive no help!");
				return false;
			}

			//Peace flag NPCs can trade with everyone
			if (target is GameNPC)
				if ((((GameNPC)target).Flags & GameNPC.eFlags.PEACE) != 0)
					return true;

			if (source is GameNPC)
				if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
					return true;

			if(source.Realm != target.Realm)
			{
				if(quiet == false) MessageToLiving(source, "You can't trade with enemy realm!");
				return false;
			}
			return true;
		}

		public override bool IsAllowedToUnderstand(GameLiving source, GamePlayer target)
		{
            if (source.CurrentRegionID == 51)
            {
                return PvPServerRules.IsAllowedToUnderstand(source, target);
            }
            if (source == null || target == null) return false;
			
			if(Properties.EVENT_CROSS_REALM_GROUPS) return true;

			if (source.CurrentRegionID == 27) return true;

			// clients with priv level > 1 are allowed to talk and hear anyone
			if(source is GamePlayer && ((GamePlayer)source).Client.Account.PrivLevel > 1) return true;
			if(target.Client.Account.PrivLevel > 1) return true;

			//Peace flag NPCs can be understood by everyone

			if (source is GameNPC)
				if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
					return true;

			if(source.Realm > 0 && source.Realm != target.Realm) return false;
			return true;
		}

		/// <summary>
		/// Is player allowed to bind
		/// </summary>
		/// <param name="player"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public override bool IsAllowedToBind(GamePlayer player, BindPoint point)
        {
            if (player.CurrentRegionID == 51)
            {
                return PvPServerRules.IsAllowedToBind(player, point);
            }
            if (point.Realm == 0) return true;
			return player.Realm == (eRealm)point.Realm;
		}

		/// <summary>
		/// Gets the server type color handling scheme
		///
		/// ColorHandling: this byte tells the client how to handle color for PC and NPC names (over the head)
		/// 0: standard way, other realm PC appear red, our realm NPC appear light green
		/// 1: standard PvP way, all PC appear red, all NPC appear with their level color
		/// 2: Same realm livings are friendly, other realm livings are enemy; nearest friend/enemy buttons work
		/// 3: standard PvE way, all PC friendly, realm 0 NPC enemy rest NPC appear light green
		/// 4: All NPC are enemy, all players are friendly; nearest friend button selects self, nearest enemy don't work at all
		/// </summary>
		/// <param name="client">The client asking for color handling</param>
		/// <returns>The color handling</returns>
		public override byte GetColorHandling(GameClient client)
		{
            if (client.Player?.CurrentRegionID == 51)
            {
                return PvPServerRules.GetColorHandling(client);
            }
            if (client.Player?.CurrentRegionID == 27)
				return 1;
			else
				return base.GetColorHandling(client);
		}

		/// <summary>
		/// Is player allowed to make the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool IsAllowedToCraft(GamePlayer player, ItemTemplate item)
		{
			return player.Realm == (eRealm)item.Realm || (item.Realm == 0 && ServerProperties.Properties.ALLOW_CRAFT_NOREALM_ITEMS);
		}

		/// <summary>
		/// Translates object type to compatible object types based on server type
		/// </summary>
		/// <param name="objectType">The object type</param>
		/// <returns>An array of compatible object types</returns>
		protected override eObjectType[] GetCompatibleObjectTypes(eObjectType objectType)
		{
			if(m_compatibleObjectTypes == null)
			{
				m_compatibleObjectTypes = new Hashtable();
				m_compatibleObjectTypes[(int)eObjectType.Staff] = new eObjectType[] { eObjectType.Staff };
				m_compatibleObjectTypes[(int)eObjectType.Fired] = new eObjectType[] { eObjectType.Fired };
                m_compatibleObjectTypes[(int)eObjectType.MaulerStaff] = new eObjectType[] { eObjectType.MaulerStaff };
				m_compatibleObjectTypes[(int)eObjectType.FistWraps] = new eObjectType[] { eObjectType.FistWraps };

				//alb
				m_compatibleObjectTypes[(int)eObjectType.CrushingWeapon]  = new eObjectType[] { eObjectType.CrushingWeapon };
				m_compatibleObjectTypes[(int)eObjectType.SlashingWeapon]  = new eObjectType[] { eObjectType.SlashingWeapon };
				m_compatibleObjectTypes[(int)eObjectType.ThrustWeapon]    = new eObjectType[] { eObjectType.ThrustWeapon };
				m_compatibleObjectTypes[(int)eObjectType.TwoHandedWeapon] = new eObjectType[] { eObjectType.TwoHandedWeapon };
				m_compatibleObjectTypes[(int)eObjectType.PolearmWeapon]   = new eObjectType[] { eObjectType.PolearmWeapon };
				m_compatibleObjectTypes[(int)eObjectType.Flexible]        = new eObjectType[] { eObjectType.Flexible };
				m_compatibleObjectTypes[(int)eObjectType.Longbow]         = new eObjectType[] { eObjectType.Longbow };
				m_compatibleObjectTypes[(int)eObjectType.Crossbow]        = new eObjectType[] { eObjectType.Crossbow };
				//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;                                         

				//mid
				m_compatibleObjectTypes[(int)eObjectType.Hammer]       = new eObjectType[] { eObjectType.Hammer };
				m_compatibleObjectTypes[(int)eObjectType.Sword]        = new eObjectType[] { eObjectType.Sword };
				m_compatibleObjectTypes[(int)eObjectType.LeftAxe]      = new eObjectType[] { eObjectType.LeftAxe };
				m_compatibleObjectTypes[(int)eObjectType.Axe]          = new eObjectType[] { eObjectType.Axe, eObjectType.LeftAxe };
				m_compatibleObjectTypes[(int)eObjectType.HandToHand]   = new eObjectType[] { eObjectType.HandToHand };
				m_compatibleObjectTypes[(int)eObjectType.Spear]        = new eObjectType[] { eObjectType.Spear };
				m_compatibleObjectTypes[(int)eObjectType.CompositeBow] = new eObjectType[] { eObjectType.CompositeBow };
				m_compatibleObjectTypes[(int)eObjectType.Thrown]       = new eObjectType[] { eObjectType.Thrown };

				//hib
				m_compatibleObjectTypes[(int)eObjectType.Blunt]        = new eObjectType[] { eObjectType.Blunt };
				m_compatibleObjectTypes[(int)eObjectType.Blades]       = new eObjectType[] { eObjectType.Blades };
				m_compatibleObjectTypes[(int)eObjectType.Piercing]     = new eObjectType[] { eObjectType.Piercing };
				m_compatibleObjectTypes[(int)eObjectType.LargeWeapons] = new eObjectType[] { eObjectType.LargeWeapons };
				m_compatibleObjectTypes[(int)eObjectType.CelticSpear]  = new eObjectType[] { eObjectType.CelticSpear };
				m_compatibleObjectTypes[(int)eObjectType.Scythe]       = new eObjectType[] { eObjectType.Scythe };
				m_compatibleObjectTypes[(int)eObjectType.RecurvedBow]  = new eObjectType[] { eObjectType.RecurvedBow };

				m_compatibleObjectTypes[(int)eObjectType.Shield]       = new eObjectType[] { eObjectType.Shield };
				m_compatibleObjectTypes[(int)eObjectType.Poison]       = new eObjectType[] { eObjectType.Poison };
				//TODO: case 45: abilityCheck = Abilities.instruments; break;
			}

			eObjectType[] res = (eObjectType[])m_compatibleObjectTypes[(int)objectType];
			if(res == null)
				return new eObjectType[0];
			return res;
		}

		/// <summary>
		/// Gets the player name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The name of the target</returns>
		public override string GetPlayerName(GamePlayer source, GamePlayer target)
		{
            if (source.CurrentRegionID == 51)
            {
                return PvPServerRules.GetPlayerName(source, target);
            }

            if (IsSameRealm(source, target, true))
				return target.Name;
			if (Properties.EVENT_PVP && source.CurrentRegionID == 27)
				return target.Name;

			return source.RaceToTranslatedName(target.Race, target.Gender);
		}

		/// <summary>
		/// Gets the player last name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The last name of the target</returns>
		public override string GetPlayerLastName(GamePlayer source, GamePlayer target)
		{
            if (source.CurrentRegionID == 51)
            {
                return PvPServerRules.GetPlayerLastName(source, target);
            }
            if (IsSameRealm(source, target, true))
				return target.LastName;
			if (Properties.EVENT_PVP && source.CurrentRegionID == 27)
				return target.LastName;

			return target.RealmRankTitle(source.Client.Account.Language);
		}

		/// <summary>
		/// Gets the player guild name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The guild name of the target</returns>
		public override string GetPlayerGuildName(GamePlayer source, GamePlayer target)
		{
            if (source.CurrentRegionID == 51)
            {
                return PvPServerRules.GetPlayerGuildName(source, target);
            }
            if (IsSameRealm(source, target, true))
				return target.GuildName;
			if (Properties.EVENT_PVP && source.CurrentRegionID == 27)
				return target.RealmRankTitle(source.Client.Account.Language);
			return string.Empty;
		}
	
		/// <summary>
		/// Gets the player's custom title based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The custom title of the target</returns>
		public override string GetPlayerTitle(GamePlayer source, GamePlayer target)
		{
            if (source.CurrentRegionID == 51)
            {
                return PvPServerRules.GetPlayerTitle(source, target);
            }
            if (IsSameRealm(source, target, true))
				return target.CurrentTitle.GetValue(source, target);
			
			return string.Empty;
		}

		/// <summary>
		/// Reset the keep with special server rules handling
		/// </summary>
		/// <param name="lord">The lord that was killed</param>
		/// <param name="killer">The lord's killer</param>
		public override void ResetKeep(GuardLord lord, GameObject killer)
		{
			base.ResetKeep(lord, killer);
			lord.Component.Keep.Reset((eRealm)killer.Realm);
			
			/* Disabled for Ariadolis Reloaded
			if (ConquestService.ConquestManager.ActiveObjective != null && ConquestService.ConquestManager.ActiveObjective.Keep == lord.Component.Keep)
			{
				ConquestService.ConquestManager.ConquestCapture(ConquestService.ConquestManager.ActiveObjective.Keep);
			}
			
			if (ConquestService.ConquestManager.GetSecondaryObjectives().FirstOrDefault(conq => conq.Keep == lord.Component.Keep) != null)
			{
				ConquestService.ConquestManager.ConquestSubCapture(lord.Component.Keep);
			}
			*/
		}
        public override void OnReleased(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = (GamePlayer)sender;

            if (player.CurrentRegionID == 51)
            {
                PvPServerRules.OnReleased(e,sender,args);
            } else
			{
				base.OnReleased(e,sender,args);
			}
        }
    }
}
