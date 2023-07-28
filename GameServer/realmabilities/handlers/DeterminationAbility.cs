using System;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Determination
	/// </summary>
	public class DeterminationAbility : RAPropertyEnhancer
	{
		public static eProperty[] properties = new eProperty[]
		{
			eProperty.MesmerizeDurationReduction,
			eProperty.StunDurationReduction,
			eProperty.SpeedDecreaseDurationReduction,
		};
		public DeterminationAbility(DBAbility dba, int level) : base(dba, level, properties) { }

		public override int MaxLevel
			{
				get { return 9; }
			}

		protected override string ValueUnit { get { return "%"; } }

		public override int CostForUpgrade(int level, GamePlayer player)
        {
            bool halfCost = player.CharacterClass.ID == (int)eCharacterClass.Mercenary ||
                player.CharacterClass.ID == (int)eCharacterClass.Blademaster ||
                player.CharacterClass.ID == (int)eCharacterClass.Berserker;

            switch (level)
			{
                case 0:
                    return halfCost ? 1 : 1;
                case 1:
                    return halfCost ? 1 : 1;
                case 2:
                    return halfCost ? 1 : 2;
                case 3:
                    return halfCost ? 1 : 3;
                case 4:
                    return halfCost ? 2 : 3;
                case 5:
                    return halfCost ? 2 : 5;
                case 6:
                    return halfCost ? 3 : 5;
                case 7:
                    return halfCost ? 3 : 7;
                case 8:
                    return halfCost ? 3 : 7;
                default: return 1000;
            }
		}


		public override int GetAmountForLevel(int level)
		{
            if (level < 1) return 0;

            switch (level)
            {
                case 1: return 4;
                case 2: return 8;
                case 3: return 12;
                case 4: return 17;
                case 5: return 23;
                case 6: return 30;
                case 7: return 38;
                case 8: return 46;
                case 9: return 55;
                default: return 55;
            }
        }
	}
}