using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class AtlasOF_EmptyMind : TheEmptyMindAbility
    {
        public AtlasOF_EmptyMind(DBAbility dba, int level) : base(dba, level)
        {
        }

        public override int MaxLevel { get { return 5; } }

        public override int GetReUseDelay(int level) { return 600; } // 10 mins

        protected override int GetDuration() { return 45000; }

        protected override int GetEffectiveness()
        {
            switch (Level)
            {
                case 1: return 10;
                case 2: return 15;
                case 3: return 20;
                case 4: return 25;
                case 5: return 30;
                default: return 0;
            }
        }
    }
}