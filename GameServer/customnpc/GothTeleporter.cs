using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;
using System.Linq;

namespace DOL.GS.Scripts
{
	public class GothwaitTeleporter : GameNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private int[,] locations = {
		//  region, x,		y,	  z,   heading
			{51, 537144, 546052, 4800, 1337 },
			{51, 537466, 549330, 4800, 1669 },
			{51, 535797, 551014, 4863, 1656 },
			{51, 532555, 548898, 4800, 3594 },
			{51, 533476, 547555, 4800, 3184 },
			{51, 536842, 546673, 4863, 549 },
			{51, 527305, 544037, 2974, 1589 },
			{51, 523681, 545471, 3153, 2067 },
			{51, 520089, 546472, 2999, 2462 },
			{51, 516348, 543588, 3427, 3012 },
			{51, 517457, 537277, 3034, 3558 },
			{51, 524535, 535594, 2985, 3976 },
			{51, 528871, 536858, 3119, 400 },
			{51, 531581, 541520, 3455, 964 },
			{51, 522242, 542221, 3222, 542 },
			//{51, 522207, 54212, 3253, 1534 }, no good
			{51, 521394, 533732, 2910, 3957 },
		};
		public override bool AddToWorld()
		{
			Model = 2002;
			Name = "Daddy Mike";
			GuildName = "PvP Teleporter";
			Level = 50;
			Size = 60;
			Flags |= GameNPC.eFlags.PEACE;
			return base.AddToWorld();
		}
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;
			TurnTo(player.X, player.Y);
			player.Out.SendMessage("Hello " + player.Name + "! Would you like to port to [PvP]?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			return true;
		}
		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str)) return false;
			if (!(source is GamePlayer)) return false;
			GamePlayer player = (GamePlayer)source;
			TurnTo(player.X, player.Y);
			switch (str)
			{
				case "Main Setup":
					if (!player.InCombat)
					{
						Say("I'm now teleporting you to the Main Setup area");
						player.MoveTo(70, 569762, 538694, 6104, 3268);
					}
					else { player.Client.Out.SendMessage("You can't port while in combat.", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }
					break;

				case "PvP":
					if (!player.InCombat)
					{
						int RandPvP = Util.Random(1, locations.Length);//Creates a random number between 1 and 17
						Group playerGroup = player.Group;
						if (playerGroup == null) // Solo port	
							player.MoveTo((ushort)locations[RandPvP, 0], locations[RandPvP, 1], locations[RandPvP, 2], locations[RandPvP, 3], (ushort)locations[RandPvP, 4]);
						else if (playerGroup.Leader != player)
						{
							player.Client.Out.SendMessage("You can't port your group unless you are the leader.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
						else if (playerGroup.IsGroupInCombat())
						{
							player.Client.Out.SendMessage("You can't port while any members of your group are in combat.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
						else if (!(playerGroup.GetNearbyPlayersInTheGroup(player).Intersect(playerGroup.GetPlayersInTheGroup()).Count() == playerGroup.GetPlayersInTheGroup().Count))
						{
							player.Client.Out.SendMessage("You must gather your party before venturing forth.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
						else // valid group port
						{
							foreach (GameLiving member in playerGroup.GetMembersInTheGroup())
							{
								member.MoveTo((ushort)locations[RandPvP, 0], locations[RandPvP, 1], locations[RandPvP, 2], locations[RandPvP, 3], (ushort)locations[RandPvP, 4]);
							}
						}
					}
					else { player.Client.Out.SendMessage("You can't port while in combat.", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }
					break;

				default: break;
			}
			return true;
		}
		private void SendReply(GamePlayer target, string msg)
		{
			target.Client.Out.SendMessage(
				msg,
				eChatType.CT_Say, eChatLoc.CL_PopupWindow);
		}
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			log.Info("\tTeleporter initialized: true");
		}
	}
}