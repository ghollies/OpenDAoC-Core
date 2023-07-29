using System;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Ethereal Bond
	/// </summary>
	public class XEtherealBondAbility : RAPropertyEnhancer
	{
		public XEtherealBondAbility(DBAbility dba, int level) : base(dba, level, eProperty.MaxMana) { }

		public override int CostForUpgrade(int level, GamePlayer player)
        {
			switch (level)
			{
                case 0: return 1;
                case 1: return 1;
                case 2: return 2;
                case 3: return 3;
                case 4: return 3;
                case 5: return 5;
                case 6: return 5;
                case 7: return 7;
                case 8: return 7;
                default: return 1000;
            }
		}
		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;

			switch (level)
			{
					case 1: return 15;
					case 2: return 25;
					case 3: return 40;
					case 4: return 55;
					case 5: return 75;
					case 6: return 100;
					case 7: return 130;
					case 8: return 165;
					case 9: return 200;
                default: return 0;
			}
		}
	}
}