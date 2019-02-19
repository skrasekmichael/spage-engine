using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapDriver
{
    [Flags]
    public enum ResearchType : byte
    {
        Weapon, Armor
    }

    [Serializable]
    public abstract class Research
    {
        public virtual byte Attack { get; } = 0;
        public virtual byte Armor { get; } = 0;
        public virtual byte PieceArmor { get; } = 0;
        public virtual byte Range { get; } = 0;
        public virtual sbyte Mobility { get; } = 0;

        public virtual List<Type> Parents { get; }
        public abstract List<UnitType> Group { get; }
        public abstract ResearchType Type { get; }

        private byte[] upgrades;

        public void Apply(Unit unit)
        {
            if (Group.Contains(unit.Type))
            {
                if (unit.Upgrades.ContainsKey(Type))
                    unit.Upgrades[Type].deapply(unit);

                upgrades = (byte[])unit.UpgradeBonuses.Clone();

                unit.UpgradeBonuses[Unit.PIECEARMOR] += PieceArmor;
                unit.UpgradeBonuses[Unit.RANGE] += Range;
                unit.UpgradeBonuses[Unit.ARMOR] += Armor;
                unit.UpgradeBonuses[Unit.ATTACK] += Attack;
                unit.UpgradeBonuses[Unit.PERMOVE] += (byte)(unit.MaxStamina / (Math.Floor((double)unit.MaxStamina / unit.StaminaPerMove) + Mobility));

                if (unit.Upgrades.ContainsKey(Type))
                    unit.Upgrades[Type] = this;
                else
                    unit.Upgrades.Add(Type, this);
            }
        }

        private void deapply(Unit unit)
        {
            unit.UpgradeBonuses = (byte[])upgrades.Clone();
        }
    }

    [Serializable]
    public class BronzeSword : Research
    {
        public override List<UnitType> Group => new List<UnitType>(new[] { UnitType.Cavalery, UnitType.Infantry });
        public override byte Attack => 1;
        public override ResearchType Type => ResearchType.Weapon;
    }

    [Serializable]
    public class IronSword : Research
    {
        public override List<Type> Parents => new List<Type>(new[] { typeof(BronzeSword) });
        public override List<UnitType> Group => new List<UnitType>(new[] { UnitType.Cavalery, UnitType.Infantry });
        public override byte Attack => 2;
        public override ResearchType Type => ResearchType.Weapon;
    }

    [Serializable]
    public class PlateArmor : Research
    {
        public override List<UnitType> Group => new List<UnitType>(new[] { UnitType.Cavalery, UnitType.Infantry });
        public override byte Armor => 3;
        public override byte PieceArmor => 2;
        public override sbyte Mobility => -2;
        public override ResearchType Type => ResearchType.Armor;
    }
}
