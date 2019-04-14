using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapDriver
{
    [Flags]
    public enum UpgradeType : byte
    {
        Weapon = 0,
        Armor = 1
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ResearchAttribute : Attribute
    {
        public bool IsResearchable { get; private set; }

        public ResearchAttribute(bool able)
        {
            IsResearchable = able;
        }
    }

    [Serializable]
    public abstract class Research
    {
        public abstract List<Type> Researches { get; }
        public abstract int ResearchDifficulty { get; }
        public int Done { get; set; }
        public override string ToString() => this.GetType().Name;
    }

    [Serializable]
    public abstract class UnitUpgrade : Research
    {
        public override int ResearchDifficulty => 1;

        public virtual byte Attack { get; } = 0;
        public virtual byte Armor { get; } = 0;
        public virtual byte PieceArmor { get; } = 0;
        public virtual byte Range { get; } = 0;
        public virtual sbyte Mobility { get; } = 0;

        public abstract int Price { get; }
        public abstract List<UnitType> Group { get; }
        public abstract UpgradeType Type { get; }

        public void Apply(Unit unit)
        {
            if (unit.Upgrades.ContainsKey(Type))
                unit.Upgrades[Type].deapply(unit);

            apply(unit, 1);

            if (unit.Upgrades.ContainsKey(Type))
                unit.Upgrades[Type] = this;
            else
                unit.Upgrades.Add(Type, this);
        }

        private void deapply(Unit unit) => apply(unit, -1);

        private void apply(Unit unit, int k)
        {
            unit.UpgradeBonuses[Unit.PIECEARMOR] += (sbyte)(k * PieceArmor);
            unit.UpgradeBonuses[Unit.RANGE] += (sbyte)(k * Range);
            unit.UpgradeBonuses[Unit.ARMOR] += (sbyte)(k * Armor);
            unit.UpgradeBonuses[Unit.ATTACK] += (sbyte)(k * Attack);
            unit.UpgradeBonuses[Unit.PERMOVE] = k == 1 ? (sbyte)(unit.StaminaPerMove - (unit.MaxStamina / (Math.Floor((double)unit.MaxStamina / unit.StaminaPerMove) + Mobility))) : (sbyte)0;
        }
    }

    [Serializable]
    [Research(false)]
    public abstract class Technology : Research
    {
        public abstract int ResearchPrice { get; }
    }

    [Serializable]
    [Research(false)]
    public abstract class Age : Technology
    {
        public override int ResearchDifficulty => 0;
        public override int ResearchPrice => 0;
    }

    #region UnitUpgrade

    [Serializable]
    public class IronSword : UnitUpgrade
    {
        public override List<Type> Researches => new List<Type> { typeof(IronProcessing) };
        public override List<UnitType> Group => new List<UnitType> { UnitType.Cavalery, UnitType.Infantry };
        public override byte Attack => 1;
        public override UpgradeType Type => UpgradeType.Weapon;
        public override int Price => 30;
    }

    [Serializable]
    public class IronSpikes : UnitUpgrade
    {
        public override List<Type> Researches => new List<Type> { typeof(IronProcessing) };
        public override List<UnitType> Group => new List<UnitType> { UnitType.Archers };
        public override byte Attack => 1;
        public override UpgradeType Type => UpgradeType.Weapon;
        public override int Price => 30;
    }

    [Serializable]
    public class SteelSword : UnitUpgrade
    {
        public override List<Type> Researches => new List<Type> { typeof(IronCasting) };
        public override List<UnitType> Group => new List<UnitType> { UnitType.Cavalery, UnitType.Infantry };
        public override byte Attack => 2;
        public override UpgradeType Type => UpgradeType.Weapon;
        public override int Price => 55;
    }

    [Serializable]
    public class SteelSpikes : UnitUpgrade
    {
        public override List<Type> Researches => new List<Type> { typeof(IronCasting) };
        public override List<UnitType> Group => new List<UnitType> { UnitType.Archers };
        public override byte Attack => 2;
        public override UpgradeType Type => UpgradeType.Weapon;
        public override int Price => 55;
    }

    [Serializable]
    public class HardedSword : UnitUpgrade
    {
        public override List<Type> Researches => new List<Type> { typeof(IronHardening) };
        public override List<UnitType> Group => new List<UnitType> { UnitType.Cavalery, UnitType.Infantry };
        public override byte Attack => 3;
        public override UpgradeType Type => UpgradeType.Weapon;
        public override int Price => 75;
        public override int ResearchDifficulty => 2;
    }

    [Serializable]
    public class ChainArmor : UnitUpgrade
    {
        public override List<Type> Researches => new List<Type> { typeof(IronProcessing) };
        public override List<UnitType> Group => new List<UnitType> { UnitType.Archers, UnitType.Cavalery, UnitType.Infantry };
        public override byte Armor => 1;
        public override byte PieceArmor => 1;
        public override UpgradeType Type => UpgradeType.Armor;
        public override int Price => 30;
    }

    [Serializable]
    public class PlateArmor : UnitUpgrade
    {
        public override List<Type> Researches => new List<Type> { typeof(IronCasting) };
        public override List<UnitType> Group => new List<UnitType> { UnitType.Cavalery, UnitType.Infantry };
        public override byte Armor => 3;
        public override byte PieceArmor => 2;
        public override sbyte Mobility => -2;
        public override UpgradeType Type => UpgradeType.Armor;
        public override int Price => 80;
        public override int ResearchDifficulty => 3;
    }

    #endregion
    #region Age

    [Serializable]
    [Research(false)]
    public class StartAge : Age
    {
        public override List<Type> Researches => new List<Type>();
    }

    [Serializable]
    [Research(false)]
    public class FedualAge : Age
    {
        public override List<Type> Researches => new List<Type> { typeof(StartAge) };
    }

    [Serializable]
    [Research(false)]
    public class CastleAge : Age
    {
        public override List<Type> Researches => new List<Type> { typeof(FedualAge) };
    }

    [Serializable]
    [Research(false)]
    public class ImperialAge : Age
    {
        public override List<Type> Researches => new List<Type> { typeof(CastleAge) };
    }

    #endregion
    #region Researches

    public class IronProcessing : Research
    {
        public override List<Type> Researches => new List<Type>() { typeof(StartAge) };
        public override int ResearchDifficulty => 10;
    }

    public class IronCasting : Research
    {
        public override List<Type> Researches => new List<Type>() { typeof(IronProcessing), typeof(FedualAge) };
        public override int ResearchDifficulty => 13;
    }

    public class IronHardening : Research
    {
        public override List<Type> Researches => new List<Type>() { typeof(IronCasting), typeof(CastleAge) };
        public override int ResearchDifficulty => 17;
    }

    #endregion
}
