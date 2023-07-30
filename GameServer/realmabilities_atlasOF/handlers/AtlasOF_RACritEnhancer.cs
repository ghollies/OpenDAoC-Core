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

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Spell crit chance %.
    /// </summary>
    public class AtlasOF_WildPowerAbility : WildPowerAbility
    {
        public AtlasOF_WildPowerAbility(DBAbility dba, int level) : base(dba, level) { }
        public override bool CheckRequirement(GamePlayer player) { return AtlasRAHelpers.GetAugAcuityLevel(player) >= 2; }
        public override int GetAmountForLevel(int level) { return AtlasRAHelpers.GetPropertyEnhancer5AmountForLevel(level); }
        public override int CostForUpgrade(int level, GamePlayer player) { return AtlasRAHelpers.GetCommonUpgradeCostFor5LevelsRA(level); }
    }

    /// <summary>
    /// Heal crit chance %.
    /// </summary>
    public class AtlasOF_WildHealingAbility : WildHealingAbility
    {
        public AtlasOF_WildHealingAbility(DBAbility dba, int level) : base(dba, level) { }
        public override bool CheckRequirement(GamePlayer player) { return AtlasRAHelpers.GetAugAcuityLevel(player) >= 2; }
        public override int GetAmountForLevel(int level) { return AtlasRAHelpers.GetPropertyEnhancer5AmountForLevel(level); }
        public override int CostForUpgrade(int level, GamePlayer player) { return AtlasRAHelpers.GetCommonUpgradeCostFor5LevelsRA(level); }
    }

    /// <summary>
    /// DoT & Debuff crit chance %.
    /// </summary>
         public class AtlasOF_WildArcanaAbility : RAPropertyEnhancer
         {
             public AtlasOF_WildArcanaAbility(DBAbility dba, int level) : base(dba, level, eProperty.CriticalDotHitChance) { }
             public override bool CheckRequirement(GamePlayer player) { return AtlasRAHelpers.GetAugAcuityLevel(player) >= 2; }
             public override int GetAmountForLevel(int level) { return AtlasRAHelpers.GetPropertyEnhancer5AmountForLevel(level); }
             public override int CostForUpgrade(int level, GamePlayer player) { return AtlasRAHelpers.GetCommonUpgradeCostFor5LevelsRA(level); }
         }

    /// <summary>
    /// Pet crit chance %.
    /// </summary>
    public class AtlasOF_WildMinionAbility : RAPropertyEnhancer
    {
        public AtlasOF_WildMinionAbility(DBAbility dba, int level) : base(dba, level, eProperty.Undefined) { }
        public override int GetAmountForLevel(int level)
        {
            {
                if (level < 1) return 0;

                switch (level)
                {
                    case 1: return 3;
                    case 2: return 6;
                    case 3: return 9;
                    case 4: return 13;
                    case 5: return 17;
                    case 6: return 22;
                    case 7: return 27;
                    case 8: return 33;
                    case 9: return 39;
                    default: return 39;
                }
            }
        }
        public override int CostForUpgrade(int level, GamePlayer player) { return AtlasRAHelpers.GetCommonUpgradeCostFor5LevelsRA(level); }
    }

    /// <summary>
    /// Archery crit chance %.
    /// </summary>
    public class AtlasOF_FalconsEye : RAPropertyEnhancer // We don't want to piggyback on the NF FalconsEye because it increases spell crit chance and not archery for some reason...
    {
        public AtlasOF_FalconsEye(DBAbility dba, int level) : base(dba, level, eProperty.CriticalArcheryHitChance) { }
        public override int GetAmountForLevel(int level) {
            {
                if (level < 1) return 0;

                switch (level)
                {
                    case 1: return 3;
                    case 2: return 6;
                    case 3: return 9;
                    case 4: return 13;
                    case 5: return 17;
                    case 6: return 22;
                    case 7: return 27;
                    case 8: return 33;
                    case 9: return 39;
                    default: return 39;
                }
            }
        }
        public override int CostForUpgrade(int level, GamePlayer player) { return AtlasRAHelpers.GetCommonUpgradeCostFor5LevelsRA(level); }
    }
}
