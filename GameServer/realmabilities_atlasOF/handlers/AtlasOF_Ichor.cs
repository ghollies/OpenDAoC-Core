using DOL.Database;
using System.Collections.Generic;

namespace DOL.GS.RealmAbilities
{

    public class AtlasOF_Ichor : IchorOfTheDeepAbility
    {
        public AtlasOF_Ichor(DBAbility dba, int level) : base(dba, level) { }

        public override int MaxLevel { get { return 5; } }
        public override int CostForUpgrade(int level, GamePlayer player) { if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (level)
                {
                    case 0: return 5;
                    case 1: return 5;
                    case 2: return 5;
                    case 3: return 7;
                    case 4: return 8;
                    default: return 1000;
                }
            }
            else
            {
                switch (level)
                {
                    case 0: return 1;
                    case 1: return 3;
                    case 2: return 6;
                    case 3: return 10;
                    case 4: return 14;
                    default: return 1000;
                }
            } }
        public override int GetReUseDelay(int level) { return 900; } // 15 mins
    }
}