using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
	public class AtlasOF_Concentration : ConcentrationAbility
	{
		public AtlasOF_Concentration(DBAbility dba, int level) : base(dba, level) { }

        public override int MaxLevel { get { return 1; } }
        public override int GetReUseDelay(int level)
        {
            if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (level)
                {
                    case 1: return 900;
                    case 2: return 540;
                    case 3: return 180;
                    case 4: return 90;
                    case 5: return 30;
                }
            }
            else
            {
                switch (level)
                {
                    case 1: return 15 * 60;
                    case 2: return 3 * 60;
                    case 3: return 30;
                }
            }

            return 0;
        }
        
        public override int CostForUpgrade(int level, GamePlayer player) { return 10; }
        public override bool CheckRequirement(GamePlayer player) { return AtlasRAHelpers.GetAugAcuityLevel(player) >= 3; }

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            GamePlayer player = living as GamePlayer;
            if (player != null)
            {
                if(player.CharacterClass.ID == (int)eCharacterClass.Necromancer)
                {
                    
                    Skill FacilitatePainworking = null;
                    ICollection<Skill> disabledSkills = player.GetAllDisabledSkills();
                    foreach(Skill skill in disabledSkills)
                    {
                        if(skill.Name == "Facilitate Painworking")
                        {
                            FacilitatePainworking = skill;
                            break;
                        }   
                    }

                    if (FacilitatePainworking == null)
                        return;

                    
                    // Is FacilitatePainWorking cooldown actually active?
                    if (player.GetSkillDisabledDuration(FacilitatePainworking) > 0)
                    {
                        player.RemoveDisabledSkill(FacilitatePainworking);
                        DisableSkill(living);

                        // Force the icon in the client to re-enable by updating its disabled time to 0
                        var disables = new List<Tuple<Skill, int>>();
                        disables.Add(new Tuple<Skill, int>(FacilitatePainworking, 0));
                        player.Out.SendDisableSkill(disables);

                        SendCasterSpellEffectAndCastMessage(living, 7006, true);
                    }
                    else
                    {
                        player.DisableSkill(this, 1000);
                        SendCasterSpellEffectAndCastMessage(living, 7006, false);
                    }
                }
                else
                {
                    Ability QuickcastAbility = player.GetAbility(Abilities.Quickcast);

                    if (QuickcastAbility == null)
                        return;

                    // Is Quickcast's cooldown actually active?
                    if (player.GetSkillDisabledDuration(player.GetAbility(Abilities.Quickcast)) > 0)
                    {
                        player.TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, 0);
                        player.RemoveDisabledSkill(SkillBase.GetAbility(Abilities.Quickcast));
                        DisableSkill(living);

                        // Force the icon in the client to re-enable by updating its disabled time to 1ms
                        var disables = new List<Tuple<Skill, int>>();
                        disables.Add(new Tuple<Skill, int>(player.GetAbility(Abilities.Quickcast), 1));
                        player.Out.SendDisableSkill(disables);

                        SendCasterSpellEffectAndCastMessage(living, 7006, true);
                    }
                    else
                    {
                        player.DisableSkill(this, 1000);
                        SendCasterSpellEffectAndCastMessage(living, 7006, false);
                    }
                }
            }
        }
    }
}