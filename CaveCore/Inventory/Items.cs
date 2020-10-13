using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Inventory
{



	public interface IITem
	{
		int MaxStack { get; }
		string Name { get; }
		void Draw(SpriteBatch sb, Vector2 position);
	}
	// items
	public abstract class Ingot
	{

	}

	public class CopperIngot { }
	public class LeadIngot { }
	public class TinIngot { }
	public class ChromiumIngot { }
	public class AluminiumIngot { }
	public class IronIngot { }
	public class NickelIngot {}
	public class GoldIngot {}

	// Warlock
	public class MilesHat { }
	public class MilesBeard { }
	public class MilesRobe { }

	// Witch
	public class NatalieHat { }
	public class NatalieRobe { }
	public class NatalieLeggings { }
	public class HeatRune { }
	public class ElectricRune { }
	public class Necronomicon { }
	public class ResurrectionTome { }
	public class Lean { }
	public class ProgrammerSocks { }

	public class OwlEyePotion { }
	public class MetalDetector { }
	public class EmptyBook { }
	public class Paper { }
	public class Sugar { }
	public class Sulphur { }
	public class ThermiteMix { }
	public class SoulManifold { }
	public class EmptyBottle { }
	public class Fedora { }
	public class TopHat { }
	public class ArmyHelmet { }
	public class NordicHelm { }


	public class PhilosopherStone { }
	public class PoisonVial { }

	public class ShoeSpikes { }
	public class Jetpack { }
	public class ExoskelHead { }
	public class ExoskelTorso { }
	public class ExoskelLegs { }

	public class GrassSeeds { }
	public class Bucket { }
	public class WaterBucket { }
	public class MagmaBucket { }
	public class SludgeBucket { }
	public class LiquidOxygenTank { }
	public class LiquidNitrogenTank { }
	public class VRHeadset { }

	public class Transistor { }

	public class M1911 { }
	public class Uzi { }
	public class Revolver { }
	public class PlasmaRifle { }
	public class Vulkan { }

	public class ZeusCannon { }
	public class SquirtGun { }
	public class ProtonPump { }
	public class FreezeRay { }
	public class Flamethrower { }
	public class Molotov { }
	public class Grenade { }
	public class Dynamite { }
	public class Bomb { }
	public class StickyBomb { }
	public class Sparkler { }

	public class GravityBoots { }
	public class Aqualung { }

	public class ManletPotion { }
	public class HellwalkerPotion { }
	public class GlowingPotion { }
	public class SpeedPotion { }
	public class HP20Potion { }
	public class HP50Potion { }


	public class Glue { }
	//public class Wire { }
	// Furniture
	public class PlasticPrinter { }
	public class Anvil { }
	public class Campfire { }
	public class Furnace { }
	public class ArrowTurret { }
	public class MGTurret { }
	public class LaserTurret { }
	public class Teleporter { }
	public class Workbench { }
	public class AlchemyLab { }
}