using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MapDriver
{
    [Flags]
    public enum UnitType : byte
    {
        Infantry, Archers, Cavalery, Siege
    }

    [Flags]
    public enum AIStrategy : byte
    {
        Scout = 4,
        Static = 3,
        Defend = 2,
        Rush = 1,
        Attack = 0
    }

    [Flags]
    public enum UnitStatus : byte
    {
        Training, Upgrading, OnWay, InBarracks, Healing
    }

    [Serializable]
    public abstract class Unit
    {
        public const int ATTACK = 0;
        public const int ARMOR = 1;
        public const int PIECEARMOR = 2;
        public const int RANGE = 3;
        public const int PERMOVE = 4;

        public abstract List<Type> Researches { get; }
        public Dictionary<UpgradeType, UnitUpgrade> Upgrades { get; } = new Dictionary<UpgradeType, UnitUpgrade>();
        public UnitStatus UnitStatus { get; set; } = UnitStatus.InBarracks;
        public int Rounds { get; set; } = 0;
        public sbyte[] UpgradeBonuses = new sbyte[5];

        public byte Player { get; private set; }
        public virtual byte Direction { get; set; } = 4;
        public abstract UnitType Type { get; }
        public abstract int Price { get; }
        public abstract byte Training { get; }

        public abstract ushort MaxExperience { get; }
        public abstract byte StaminaPerAttack { get; }

        protected abstract byte per_move { get; }
        public byte StaminaPerMove => (byte)(per_move + UpgradeBonuses[PERMOVE]);
        public abstract byte StaminaPerLevel { get; }

        protected abstract byte attack { get; }
        public byte Attack => (byte)(UpgradeBonuses[ATTACK] + attack + AttackBonus);

        protected abstract byte armor { get; }
        public byte Armor => (byte)(UpgradeBonuses[ARMOR] + armor + ArmorBonus);

        protected abstract byte piece_armor { get; }
        public byte PieceArmor => (byte)(UpgradeBonuses[PIECEARMOR] + piece_armor + ArmorBonus);

        protected abstract ushort max_stamina { get; }
        public ushort MaxStamina => (ushort)(max_stamina + StaminaBonus);

        protected virtual byte range { get; } = 1;
        public byte Range => (byte)(range + UpgradeBonuses[RANGE]);
        public virtual byte MinRange { get; } = 0;

        public abstract byte LineOfSight { get; }
        public abstract ushort MaxHealth { get; }
        public abstract bool IsRanged { get; }

        public ushort MaxHeal { get; set; }
        public ushort Health { get; set; }
        public ushort Stamina { get; set; }
        private ushort experience = 0;
        public ushort Experience
        {
            get => experience;
            set
            {
                int level = GetLevel();
                experience = (value > MaxExperience) ? MaxExperience : value;
                int nlevel = GetLevel();
                int delta = nlevel - level;
                if (delta != 0)
                {
                    for (int i = 0; i < delta; i++)
                    {
                        StaminaBonus += StaminaPerLevel;
                        if (i % 2 == 0)
                            AttackBonus += 1;
                        else
                            ArmorBonus += 1;
                    }
                }
            }
        }

        public AIStrategy AIStrategy { get; set; } = AIStrategy.Attack; 
        public byte AttackBonus { get; private set; } = 0;
        public byte ArmorBonus { get; private set; } = 0;
        public ushort StaminaBonus { get; private set; } = 0;

        public string Texture => this.GetType().Name.ToLower() + Player.ToString();

        public ushort[] ExperiencePerLevel { get; set; } = new ushort[7];

        public byte GetLevel()
        {
            for (byte i = 6; i >= 0; i--)
            {
                if (Experience >= ExperiencePerLevel[i])
                    return i;
            }
            return 0;
        }

        public Unit(byte player = 1)
        {
            Player = player;
            MaxHeal = MaxHealth;
            Health = MaxHealth;
            Stamina = MaxStamina;

            float[] percent = { 0, 0.15f, 0.30f, 0.45f, 0.60f, 0.80f, 1 };
            for (int i = 0; i < 7; i++)
                ExperiencePerLevel[i] = (ushort)((double)percent[i] * MaxExperience);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
                return false;

            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo info in props)
            {
                if (this.GetType().GetProperty(info.Name) != info)
                    return false;
            }

            return true;
        }

        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => this.GetType().Name;
    }

    [Serializable]
    public class Paladin : Unit
    {
        public override UnitType Type => UnitType.Cavalery;
        public override List<Type> Researches => new List<Type> { typeof(ImperialAge), typeof(PlateArmor), typeof(IronSword) };
        public override byte Training => 4;
        protected override byte attack => 14;
        protected override byte armor => 4;
        protected override byte piece_armor => 4;
        protected override ushort max_stamina => 155;
        protected override byte per_move => 30;
        public override byte LineOfSight => 5;
        public override ushort MaxHealth => 180;
        public override byte StaminaPerAttack => 50;
        public override byte StaminaPerLevel => 25;
        public override bool IsRanged => false;
        public override int Price => 200;
        public override ushort MaxExperience => 8000;

        public Paladin(byte player) : base(player) { }
    }

    [Serializable]
    public class Knight : Unit
    {
        public override UnitType Type => UnitType.Cavalery;
        public override List<Type> Researches => new List<Type> { typeof(CastleAge), typeof(IronSword) };
        public override byte Training => 3;
        protected override byte attack => 10;
        protected override byte armor => 2;
        protected override byte piece_armor => 2;
        protected override ushort max_stamina => 125;
        protected override byte per_move => 20;
        public override byte LineOfSight => 5;
        public override ushort MaxHealth => 120;
        public override byte StaminaPerAttack => 40;
        public override byte StaminaPerLevel => 20;
        public override bool IsRanged => false;
        public override int Price => 120;
        public override ushort MaxExperience => 5000;

        public Knight(byte player) : base(player) { }
    }

    [Serializable]
    public class Scout : Unit
    {
        public override UnitType Type => UnitType.Cavalery;
        public override List<Type> Researches => new List<Type> { typeof(StartAge) };
        public override byte Training => 2;
        protected override byte attack => 4;
        protected override byte armor => 0;
        protected override byte piece_armor => 1;
        protected override ushort max_stamina => 200;
        protected override byte per_move => 25;
        public override byte LineOfSight => 9;
        public override ushort MaxHealth => 95;
        public override byte StaminaPerAttack => 60;
        public override byte StaminaPerLevel => 20;
        public override bool IsRanged => false;
        public override int Price => 60;
        public override ushort MaxExperience => 3500;

        public Scout(byte player) : base(player) { }
    }

    [Serializable]
    public class Archer : Unit
    {
        public override UnitType Type => UnitType.Archers;
        public override List<Type> Researches => new List<Type> { typeof(StartAge) };
        public override byte Training => 2;
        protected override byte attack => 6;
        protected override byte armor => 0;
        protected override byte piece_armor => 0;
        protected override ushort max_stamina => 80;
        protected override byte per_move => 80 / 5;
        public override byte LineOfSight => 5;
        public override ushort MaxHealth => 30;
        public override byte StaminaPerAttack => 80/3;
        public override byte StaminaPerLevel => 15;
        public override bool IsRanged => true;
        protected override byte range => 4;
        public override int Price => 40;
        public override ushort MaxExperience => 3100;

        public Archer(byte player) : base(player) { }
    }
}
