// Originally from Fooljam and Sirru.


using System.Collections;
using System.Linq;
using DOL.Database;

using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Scripts
{
    public class BuffBot : GameNPC
    {

        #region add to world
        public override bool AddToWorld()
        {


            Level = 50;
            Flags |= GameNPC.eFlags.PEACE;

            base.AddToWorld();
            return true;
        }
        #endregion





        public override int Concentration
        {
            get
            {
                return 100000;
            }
        }

        public override int Mana
        {
            get
            {
                return 100000;
            }
        }

        private static ArrayList m_baseSpells = null;
        public static ArrayList BaseBuffs
        {
            get
            {
                if (m_baseSpells == null)
                {
                    m_baseSpells = new ArrayList();
                    m_baseSpells.Add(BotStrBuff);
                    m_baseSpells.Add(BotConBuff);
                    m_baseSpells.Add(BotDexBuff);
                }
                return m_baseSpells;
            }
        }

        private static ArrayList m_specSpells = null;
        public static ArrayList SpecBuffs
        {
            get
            {
                if (m_specSpells == null)
                {
                    m_specSpells = new ArrayList();
                    m_specSpells.Add(BotStrConBuff);
                    m_specSpells.Add(BotDexQuiBuff);
                    m_specSpells.Add(BotAcuityBuff);
                }
                return m_specSpells;
            }
        }

        private static ArrayList m_otherSpells = null;
        public static ArrayList OtherBuffs
        {
            get
            {
                if (m_otherSpells == null)
                {
                    m_otherSpells = new ArrayList();
                    m_otherSpells.Add(BotHealBuff);
                    m_otherSpells.Add(BotHasteBuff);
                    m_otherSpells.Add(BotHPRegenBuff);
                    m_otherSpells.Add(BotEndRegenBuff);
                }
                return m_otherSpells;
            }
        }

        private Queue m_buffs = new Queue();



        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;



            if (player.InCombat)
            {
                player.Out.SendMessage("Buffbot says \"stop your combat if you want me to buff you!\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                return false;
            }
            if (GetDistanceTo(player) > WorldMgr.INTERACT_DISTANCE)
            {
                player.Out.SendMessage("Buffbot says \"Get over here if you want me to buff you!\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                return false;
            }
            TurnTo(player, 3000);
            lock (m_buffs.SyncRoot)
            {
                foreach (Spell s in BaseBuffs)
                {
                    //if (s.SpellType == "AcuityBuff" && player.CharacterClass.ClassType != eClassType.ListCaster)
                        //continue;
                    Container con = new Container(s, BotBaseSpellLine, player);
                    m_buffs.Enqueue(con);
                }

                foreach (Spell s in SpecBuffs)
                {
                    Container con = new Container(s, BotSpecSpellLine, player);
                    m_buffs.Enqueue(con);
                }

                foreach (Spell s in OtherBuffs)
                {
                    //if (s.SpellType == "PowerRegenBuff" && player.MaxMana == 0)
                        //continue;
                    Container con = new Container(s, BotOtherSpellLine, player);
                    m_buffs.Enqueue(con);
                }
                //if the player has sickness it will be removed.
                ECSGameEffect effect = player.effectListComponent.GetAllEffects()
                    .FirstOrDefault(sick => sick is ResurrectionIllnessECSGameEffect);
                if (effect != null)
                {
                    player.effectListComponent.RemoveEffect(effect);
                    player.Out.SendMessage(GetName(0, false) + " cures your resurrection sickness.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
            if (CurrentSpellHandler == null)
                CastBuffs();
            player.Mana = player.MaxMana;

            return true;
        }

        public void CastBuffs()
        {
            Spell BuffSpell = null;
            SpellLine BuffSpellLine = null;
            GameLiving target = null;
            while (m_buffs.Count > 0)
            {
                Container con = (Container)m_buffs.Dequeue();
                BuffSpell = con.Spell;
                target = con.Target;
                BuffSpellLine = con.SpellLine;

                ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, BuffSpell, BuffSpellLine);
                if (spellHandler != null)
                {
                    TargetObject = target;
                    TurnTo(target, 1000);
                    spellHandler.StartSpell(target);
                }
            }
            if(target is GamePlayer p) SendPlayerUpdates(p);
        }
        
        private static void SendPlayerUpdates(GamePlayer player)
        {
            if (player == null)
                return;

            player.Out.SendCharStatsUpdate();
            player.Out.SendCharResistsUpdate();
            player.Out.SendUpdateWeaponAndArmorStats();
            player.UpdateEncumberance();
            player.UpdatePlayerStatus();
            player.Out.SendUpdatePlayer();
            player.Group?.UpdateMember(player, true, false);
        }

        #region SpellCasting

        private static SpellLine m_BotBaseSpellLine;
        private static SpellLine m_BotSpecSpellLine;
        private static SpellLine m_BotOtherSpellLine;
        /// <summary>
        /// Spell line used by bots
        /// </summary>
        public static SpellLine BotBaseSpellLine
        {
            get
            {
                if (m_BotBaseSpellLine == null)
                    m_BotBaseSpellLine = new SpellLine("BotBaseSpellLine", "BuffBot Spells", "unknown", true);

                return m_BotBaseSpellLine;
            }
        }

        public static SpellLine BotSpecSpellLine
        {
            get
            {
                if (m_BotSpecSpellLine == null)
                    m_BotSpecSpellLine = new SpellLine("BotSpecSpellLine", "BuffBot Spells", "unknown", false);

                return m_BotSpecSpellLine;
            }
        }

        public static SpellLine BotOtherSpellLine
        {
            get
            {
                if (m_BotOtherSpellLine == null)
                    m_BotOtherSpellLine = new SpellLine("BotOtherSpellLine", "BuffBot Spells", "unknown", true);

                return m_BotOtherSpellLine;
            }
        }

        private static Spell m_basestr;
        private static Spell m_basecon;
        private static Spell m_basedex;
        private static Spell m_strcon;
        private static Spell m_dexqui;
        private static Spell m_acuity;
        private static Spell m_powereg;
        private static Spell m_dmgadd;
        private static Spell m_haste;
        private static Spell m_hpRegen;
        private static Spell m_endRegen;
        private static Spell m_heal;

        #region Spells

        /// <summary>
        /// Bot Base AF buff
        /// </summary>
        //public static Spell BotBaseAFBuff
        //{
        //    get
        //    {
        //        if (m_baseaf == null)
        //        {
        //            DBSpell spell = new DBSpell();
        //            spell.AllowAdd = false;
        //            spell.CastTime = 0;
        //            spell.ClientEffect = 1467;
        //            spell.Icon = 1467;
        //            spell.Duration = 65535;
        //            spell.Value = 67;
        //            spell.Name = "Armorfactor Buff";
        //            spell.Description = "Adds to the recipient's Armor Factor (AF) resulting in better protection againts some forms of attack. It acts in addition to any armor the target is wearing.";
        //            spell.Range = WorldMgr.VISIBILITY_DISTANCE;
        //            spell.SpellID = 100001;
        //            spell.Target = "Realm";
        //            spell.Type = "ArmorFactorBuff";
        //            spell.EffectGroup = 1;
        //            m_baseaf = new Spell(spell, 50);
        //        }
        //        return m_baseaf;
        //    }
        //}

        /// <summary>
        /// Bot Str buff
        /// </summary>
        public static Spell BotStrBuff
        {
            get
            {
                if (m_basestr == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 1456;
                    spell.Icon = 1456;
					spell.TooltipId = 1456;
                    spell.Duration = 65535;
                    spell.Value = 42;
                    spell.Name = "Strength Buff";
                    spell.Description = "Increases target's Strength.";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000002;
                    spell.Target = "Realm";
                    spell.Type = "StrengthBuff";
					spell.PackageID = "BuffBotSpells";
                    spell.EffectGroup = 4;
                    GameServer.Database.AddObject(spell);
                    m_basestr = new Spell(spell, 50);
                }
                return m_basestr;
            }
        }

        /// <summary>
        /// Bot Con buff
        /// </summary>
        public static Spell BotConBuff
        {
            get
            {
                if (m_basecon == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 1485;
                    spell.Icon = 1485;
					spell.TooltipId = 1485;
                    spell.Duration = 65535;
                    spell.Value = 36;
                    spell.Name = "Constitution Buff";
                    spell.Description = "Increases target's Constitution.";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000003;
                    spell.Target = "Realm";
                    spell.Type = "ConstitutionBuff";
					spell.PackageID = "BuffBotSpells";
                    spell.EffectGroup = 201;
                    GameServer.Database.AddObject(spell);
                    m_basecon = new Spell(spell, 50);
                }
                return m_basecon;
            }
        }

        /// <summary>
        /// Bot Dex buff
        /// </summary>
        public static Spell BotDexBuff
        {
            get
            {
                if (m_basedex == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 1475;
                    spell.Icon = 1475;
					spell.TooltipId = 1475;
                    spell.Duration = 65535;
                    spell.Value = 40;
                    spell.Name = "Dexterity Buff";
                    spell.Description = "Increases target's Dexterity.";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000004;
                    spell.Target = "Realm";
                    spell.Type = "DexterityBuff";
					spell.PackageID = "BuffBotSpells";
                    spell.EffectGroup = 202;
                    GameServer.Database.AddObject(spell);
                    m_basedex = new Spell(spell, 50);
                }
                return m_basedex;
            }
        }

        /// <summary>
        /// Bot Str/Con buff
        /// </summary>
        public static Spell BotStrConBuff
        {
            get
            {
                if (m_strcon == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 1516;
                    spell.Icon = 1516;
					spell.TooltipId = 1516;
                    spell.Duration = 65535;
                    spell.Value = 57;
                    spell.Name = "Strength/Constitution Buff";
                    spell.Description = "Increases Str/Con for a character";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000005;
                    spell.Target = "Realm";
                    spell.Type = "StrengthConstitutionBuff";
					spell.PackageID = "BuffBotSpells";
                    spell.EffectGroup = 204;
                    GameServer.Database.AddObject(spell);
                    m_strcon = new Spell(spell, 50);
                }
                return m_strcon;
            }
        }

        /// <summary>
        /// Bot Dex/Qui buff
        /// </summary>
        public static Spell BotDexQuiBuff
        {
            get
            {
                if (m_dexqui == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 1525;
                    spell.Icon = 1525;
					spell.TooltipId = 1525;
                    spell.Duration = 65535;
                    spell.Value = 63;
                    spell.Name = "Dexterity/Quickness Buff";
                    spell.Description = "Decreases Dexterity and Quickness for a character.";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000006;
                    spell.Target = "Realm";
                    spell.Type = "DexterityQuicknessBuff";
					spell.PackageID = "BuffBotSpells";
                    spell.EffectGroup = 203;
                    GameServer.Database.AddObject(spell);
                    m_dexqui = new Spell(spell, 50);
                }
                return m_dexqui;
            }
        }

        /// <summary>
        /// Bot Acuity buff
        /// </summary>
        public static Spell BotAcuityBuff
        {
            get
            {
                if (m_acuity == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 1537;
                    spell.Icon = 1537;
					spell.TooltipId = 1537;
                    spell.Duration = 65535;
                    spell.Value = 41;
                    spell.Name = "Acuity Buff Buff";
                    spell.Description = "Increases Acuity (casting attribute) for a character.";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000007;
                    spell.Target = "Realm";
                    spell.Type = "AcuityBuff";
					spell.PackageID = "BuffBotSpells";
                    spell.EffectGroup = 200;
                    GameServer.Database.AddObject(spell);
                    m_acuity = new Spell(spell, 50);
                }
                return m_acuity;
            }
        }
        /// <summary>
        /// Bot Spec Af buff
        /// </summary>
        //public static Spell BotSpecAFBuff
        //{
        //    get
        //    {
        //        if (m_specaf == null)
        //        {
        //            DBSpell spell = new DBSpell();
        //            spell.AllowAdd = false;
        //            spell.CastTime = 0;
        //            spell.ClientEffect = 1506;
        //            spell.Icon = 1506;
        //            spell.Duration = 65535;
        //            spell.Value = 67;
        //            spell.Name = "Spec AF Buff";
        //            spell.Description = "Adds to the recipient's Armor Factor (AF), resulting in better protection against some forms of attack. It acts in addition to any armor the target is wearing.";
        //            spell.Range = WorldMgr.VISIBILITY_DISTANCE;
        //            spell.SpellID = 100014;
        //            spell.Target = "Realm";
        //            spell.Type = "ArmorFactorBuff";
        //            m_specaf = new Spell(spell, 50);
        //        }
        //        return m_specaf;
        //    }
        //}
        /// <summary>
        /// Bot PowerReg buff
        /// </summary>
        public static Spell BotPoweregBuff
        {
            get
            {
                if (m_powereg == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 980;
                    spell.Icon = 980;
					spell.TooltipId = 9008;
                    spell.Duration = 65535;
                    spell.Value = 30;
                    spell.Name = "Power Regeneration Buff";
                    spell.Description = "Target regenerates power regeneration during the duration of the spell";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000008;
                    spell.Target = "Realm";
                    spell.Type = "PowerRegenBuff";
					spell.PackageID = "BuffBotSpells";
                    GameServer.Database.AddObject(spell);
                    m_powereg = new Spell(spell, 50);
                }
                return m_powereg;
            }
        }

        /// <summary>
        /// Bot DamageAdd buff
        /// </summary>
        public static Spell BotDmgaddBuff
        {
            get
            {
                if (m_dmgadd == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 18;
                    spell.Icon = 18;
					spell.TooltipId = 9009;
                    spell.Duration = 65535;
                    spell.Damage = 5.0;
                    spell.DamageType = 15;
                    spell.Name = "Damage Add Buff";
                    spell.Description = "Target's melee attacks do additional damage.";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000009;
                    spell.Target = "Realm";
                    spell.Type = "DamageAdd";
					spell.PackageID = "BuffBotSpells";
                    GameServer.Database.AddObject(spell);
                    m_dmgadd = new Spell(spell, 50);
                }
                return m_dmgadd;
            }
        }

        /// <summary>
        /// Bot DamageAdd buff
        /// </summary>
        public static Spell BotHasteBuff
        {
            get
            {
                if (m_haste == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 406;
                    spell.Icon = 406;
					spell.TooltipId = 406;
                    spell.Duration = 65535;
                    spell.Value = 15;
                    spell.Name = "Haste Buff";
                    spell.Description = "Increases the target's combat speed.";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000010;
                    spell.Target = "Realm";
                    spell.Type = "CombatSpeedBuff";
					spell.PackageID = "BuffBotSpells";
                    spell.EffectGroup = 100;
                    GameServer.Database.AddObject(spell);
                    m_haste = new Spell(spell, 50);
                }
                return m_haste;
            }
        }

        /// <summary>
        /// Bot HP Regen buff
        /// </summary>
        public static Spell BotHPRegenBuff
        {
            get
            {
                if (m_hpRegen == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 1533;
                    spell.Icon = 1533;
					spell.TooltipId = 1533;
                    spell.Duration = 65535;
                    spell.Value = 7;
                    spell.Name = "Health Regeneration Buff";
                    spell.Description = "Target regenerates the given amount of health every tick";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000011;
                    spell.Target = "Realm";
                    spell.Type = "HealthRegenBuff";
					spell.PackageID = "BuffBotSpells";
                    spell.EffectGroup = 70;
                    GameServer.Database.AddObject(spell);
                    m_hpRegen = new Spell(spell, 50);
                }
                return m_hpRegen;
            }
        }

        /// <summary>
        /// Bot End Regen buff
        /// </summary>
        public static Spell BotEndRegenBuff
        {
            get
            {
                if (m_endRegen == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 3296;
                    spell.Icon = 3296;
					spell.TooltipId = 9012;
                    spell.Duration = 65535;
                    spell.Value = 5;
                    spell.Name = "Endurance Regeneration Buff";
                    spell.Description = "Target regenerates endurance during the duration of the spell.";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 2000012;
                    spell.Target = "Realm";
                    spell.Type = "EnduranceRegenBuff";
					spell.PackageID = "BuffBotSpells";
                    GameServer.Database.AddObject(spell);
                    m_endRegen = new Spell(spell, 50);
                }
                return m_endRegen;
            }
        }

        /// <summary>
        /// Bot Heal buff
        /// </summary>
        public static Spell BotHealBuff
        {
            get
            {
                if (m_heal == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 1424;
                    spell.Value = 3000;
                    spell.Name = "Heal";
                    spell.Description = "Heals the target.";
                    spell.Range = WorldMgr.VISIBILITY_DISTANCE;
                    spell.SpellID = 100013;
                    spell.Target = "Realm";
                    spell.Type = "Heal";
                    m_heal = new Spell(spell, 50);
                }
                return m_heal;
            }
        }

        #endregion Spells


        #endregion SpellCasting

        private void SendReply(GamePlayer target, string msg)
        {
            target.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
        }

        public class Container
        {
            private Spell m_spell;
            public Spell Spell
            {
                get { return m_spell; }
            }

            private SpellLine m_spellLine;
            public SpellLine SpellLine
            {
                get { return m_spellLine; }
            }

            private GameLiving m_target;
            public GameLiving Target
            {
                get { return m_target; }
                set { m_target = value; }
            }
            public Container(Spell spell, SpellLine spellLine, GameLiving target)
            {
                m_spell = spell;
                m_spellLine = spellLine;
                m_target = target;
            }
        }
    }
}