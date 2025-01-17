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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	   "&faction",
	   ePrivLevel.GM,
	   "GMCommands.Faction.Description",
	   "GMCommands.Faction.Usage.Create",
	   "GMCommands.Faction.Usage.Assign",
	   "GMCommands.Faction.Usage.AddFriend",
	   "GMCommands.Faction.Usage.AddEnemy",
	   "GMCommands.Faction.Usage.List",
	   "GMCommands.Faction.Usage.Select")]
	public class FactionCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		protected string TEMP_FACTION_LAST = "TEMP_FACTION_LAST";

		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}
			Faction myfaction = client.Player.TempProperties.GetProperty<Faction>(TEMP_FACTION_LAST, null);
			switch (args[1])
			{
				#region Create
				case "create":
					{
						if (args.Length < 4)
						{
							DisplaySyntax(client);
							return;
						}
						string name = args[2];
						int baseAggro = 0;
						try
						{
							baseAggro = Convert.ToInt32(args[3]);
						}
						catch
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Create.BAMustBeNumber"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}

						int max = 0;
						//Log.Info("count:" + FactionMgr.Factions.Count.ToString());
						if (FactionMgr.Factions.Count != 0)
						{
							//Log.Info("count >0");
							IEnumerator enumerator = FactionMgr.Factions.Keys.GetEnumerator();
							while (enumerator.MoveNext())
							{
								//Log.Info("max :" + max + " et current :" + (int)enumerator.Current);
								max = System.Math.Max(max, (int)enumerator.Current);
							}
						}
						//Log.Info("max :" + max);
						DBFaction dbfaction = new DBFaction();
						dbfaction.BaseAggroLevel = baseAggro;
						dbfaction.Name = name;
						dbfaction.ID = (max + 1);
						//Log.Info("add obj to db with id :" + dbfaction.ID);
						GameServer.Database.AddObject(dbfaction);
						//Log.Info("add obj to db");
						myfaction = new Faction();
						myfaction.LoadFromDatabase(dbfaction);
						FactionMgr.Factions.Add(dbfaction.ID, myfaction);
						client.Player.TempProperties.SetProperty(TEMP_FACTION_LAST, myfaction);
						client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Create.NewCreated"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
					}
					break;
				#endregion Create
				#region Assign
				case "assign":
					{
						if (myfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.MustSelectFaction"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}

						GameNPC npc = client.Player.TargetObject as GameNPC;
						if (npc == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Assign.MustSelectMob"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						npc.Faction = myfaction;
						client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Assign.MobHasJoinedFact", npc.Name, myfaction.Name), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
					}
					break;
				#endregion Assign
				#region AddFriend
				case "addfriend":
					{
						if (myfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.MustSelectFaction"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}
						int id = 0;
						try
						{
							id = Convert.ToInt32(args[2]);
						}
						catch
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.IndexMustBeNumber"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						Faction linkedfaction = FactionMgr.GetFactionByID(id);
						if (linkedfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.FactionNotLoaded"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						DBLinkedFaction dblinkedfaction = new DBLinkedFaction();
						dblinkedfaction.FactionID = myfaction.Id;
						dblinkedfaction.LinkedFactionID = linkedfaction.Id;
						dblinkedfaction.IsFriend = true;
						GameServer.Database.AddObject(dblinkedfaction);
						myfaction.AddFriendFaction(linkedfaction);
					}
					break;
				#endregion AddFriend
				#region AddEnemy
				case "addenemy":
					{
						if (myfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.MustSelectFaction"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}
						int id = 0;
						try
						{
							id = Convert.ToInt32(args[2]);
						}
						catch
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.IndexMustBeNumber"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						Faction linkedfaction = FactionMgr.GetFactionByID(id);
						if (linkedfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.FactionNotLoaded"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						DBLinkedFaction dblinkedfaction = new DBLinkedFaction();
						dblinkedfaction.FactionID = myfaction.Id;
						dblinkedfaction.LinkedFactionID = linkedfaction.Id;
						dblinkedfaction.IsFriend = false;
						GameServer.Database.AddObject(dblinkedfaction);
						myfaction.EnemyFactions.Add(linkedfaction);
					}
					break;
				#endregion AddEnemy
				#region List
				case "list":
					{
						foreach (Faction faction in FactionMgr.Factions.Values)
							client.Player.Out.SendMessage("#" + faction.Id.ToString() + ": " + faction.Name + " (" + faction._baseAggroLevel + ")", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
						return;
					}
				#endregion List
				#region Select
				case "select":
					{
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}
						int id = 0;
						try
						{
							id = Convert.ToInt32(args[2]);
						}
						catch
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.IndexMustBeNumber"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						Faction tempfaction = FactionMgr.GetFactionByID(id);
						if (tempfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.FactionNotLoaded"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						client.Player.TempProperties.SetProperty(TEMP_FACTION_LAST, tempfaction);
					}
					break;
				#endregion Select
				#region Default
				default:
					{
						DisplaySyntax(client);
						return;
					}
				#endregion Default
			}
		}
	}
}
