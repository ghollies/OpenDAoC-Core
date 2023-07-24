using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;
using System.Numerics;

namespace DOL.GS
{
    public class ThidrankiTeleporter : GameNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool AddToWorld()
        {
            Model = 998;
            Name = "Channeler Moz'ikov";
            GuildName = "Teleporter";
            Level = 50;
            Size = 60;
            Flags |= GameNPC.eFlags.PEACE;
            return base.AddToWorld();
        }
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;
			TurnTo(player.X, player.Y);
			player.Out.SendMessage("Hello " + player.Name + "! Would you like to port to [Thidranki] or return to the [Main Setup]?", eChatType.CT_Say,eChatLoc.CL_PopupWindow);
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
                        // Say("I'm now teleporting you to the Main Setup area");
                        t.MoveTo(91, 31885, 32181, 15844, 2049);
                    }
                    else { t.Client.Out.SendMessage("You can't port while in combat.", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }
                    break;

                case "Thidranki":
                    if (!t.InCombat)
                    {

                       
                        if (t.Realm == eRealm.Hibernia && t.Level == 50)
                        {
                            // Move to Thidranki area 238
                            Say("I will send you to Thidranki... Best of luck!");
                            foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                                player.Out.SendSpellCastAnimation(this, 4953, 6);
                            t.MoveTo(238, 534248, 533333, 5408, 3985);
                        }
                        else if (t.Level != 50)
                            { t.Client.Out.SendMessage("You are not Level 50", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }
                        
                        if (t.Realm == eRealm.Midgard && t.Level == 50)
                        {
                            // Move to Thidranki area 238
                            Say("I will send you to Thidranki... Best of luck!");
                            foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                                player.Out.SendSpellCastAnimation(this, 4953, 6);
                            t.MoveTo(238, 570913, 540584, 5408, 478);
                        }
                        else if (t.Level != 50)
                        { t.Client.Out.SendMessage("You are not Level 50", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }

                        if (t.Realm == eRealm.Albion && t.Level == 50)
                        {
                            // Move to Thidranki area 238
                            Say("I will send you to Thidranki... Best of luck!");
                            foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                                player.Out.SendSpellCastAnimation(this, 4953, 6);
                            t.MoveTo(238, 562805, 574005, 5408, 2796);
                        }
                        else if (t.Level != 50)
                        { t.Client.Out.SendMessage("You are not Level 50", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }
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