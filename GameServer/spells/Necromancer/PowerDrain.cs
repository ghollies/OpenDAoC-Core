﻿/*
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
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("PowerDrainPet")]
	public class PowerDrainPet : PowerDrain
	{
		
		/// The power channelled through this spell goes to the owner, not the pet
		protected override GameLiving powerTarget()
		{
			if(Caster is NecromancerPet pet)
			{
				return ((IControlledBrain)pet.Brain).Owner;
			}
            return Caster;
		}
		/// <summary>
		/// Create a new handler for the necro petpower drain spell.
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="spell"></param>
		/// <param name="line"></param>
		public PowerDrainPet(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}
}