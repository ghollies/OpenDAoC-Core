using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class GothwaitTeleporter : GameNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
			player.Out.SendMessage("Hello " + player.Name + "! Would you like to port to [PvP]?", eChatType.CT_Say,eChatLoc.CL_PopupWindow);
			return true;
		}
		public override bool WhisperReceive(GameLiving source, string str)
		{
			if(!base.WhisperReceive(source,str)) return false;
		  	if(!(source is GamePlayer)) return false;
			GamePlayer t = (GamePlayer) source;
			TurnTo(t.X,t.Y);
			switch(str)
			{
                case "Main Setup":    
                    if (!t.InCombat)
                    {
                        Say("I'm now teleporting you to the Main Setup area");
                        t.MoveTo(70, 569762, 538694, 6104, 3268);
                    }
                    else { t.Client.Out.SendMessage("You can't port while in combat.", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }
                    break;

                case "PvP":
                    if (!t.InCombat)
                    {
                        int RandPvP = Util.Random(1, 14);//Creates a random number between 1 and 17
                        if (RandPvP == 1)
                        {// send you to  the gloc below if number 1 comes up random
                            t.MoveTo(51, 537144, 546052, 4800, 1337);
                        }
                        else if (RandPvP == 2)
                        {
                            t.MoveTo(51, 537466, 549330, 4800, 1669);
                        }
                        else if (RandPvP == 3)
                        {
                            t.MoveTo(51, 535797, 551014, 4863, 1656);
                        }
                        else if (RandPvP == 4)
                        {
                            t.MoveTo(51, 532555, 548898, 4800, 3594);
                        }
                        else if (RandPvP == 5)
                        {
                            t.MoveTo(51, 533476, 547555, 4800, 3184);
                        }
                        else if (RandPvP == 6)
                        {
                            t.MoveTo(51, 536842, 546673, 4863, 549);
                        }
                        else if (RandPvP == 7)
                        {
                            t.MoveTo(51, 527305, 544037, 2974, 1589);
                        }
                        else if (RandPvP == 8)
                        {
                            t.MoveTo(51, 523681, 545471, 3153, 2067);
                        }
                        else if (RandPvP == 9)
                        {
                            t.MoveTo(51, 520089, 546472, 2999, 2462);
                        }
                        else if (RandPvP == 10)
                        {
                            t.MoveTo(51, 516348, 543588, 3427, 3012);
                        }
                        else if (RandPvP == 11)
                        {
                            t.MoveTo(51, 517457, 537277, 3034, 3558);
                        }
                        else if (RandPvP == 12)
                        {
                            t.MoveTo(51, 524535, 535594, 2985, 3976);
                        }
                        else if (RandPvP == 13)
                        {
                            t.MoveTo(51, 528871, 536858, 3119, 400);
                        }
                        else if (RandPvP == 14)
                        {
                            t.MoveTo(51, 531581, 541520, 3455, 964);
                        }
                        else if (RandPvP == 15)
                        {
                            t.MoveTo(51, 522242, 542221, 3222, 542);
                        }
                        else if (RandPvP == 16)
                        {
                            t.MoveTo(51, 522207, 54212, 3253, 1534);
                        }
                        else if (RandPvP == 17)
                        {
                            t.MoveTo(51, 521394, 533732, 2910, 3957);
                        }
                    }
                    else { t.Client.Out.SendMessage("You can't port while in combat.", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }
                    break;

                default: break;
			}
			return true;
		}
		private void SendReply(GamePlayer target, string msg)
			{
				target.Client.Out.SendMessage(
					msg,
					eChatType.CT_Say,eChatLoc.CL_PopupWindow);
			}
		[ScriptLoadedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            log.Info("\tTeleporter initialized: true");
        }	
    }
}