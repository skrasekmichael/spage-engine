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
        Scout = 4, Static = 3, Defend = 2, Rush = 1, Attack = 0
    }

    [Serializable]
    public abstract class Unit
    {
        public byte[] UpgradeBonuses = new byte[4];
        public byte Player { get; private set; }
        public virtual byte Direction { get; set; } = 4;
        public abstract UnitType Type { get; }

        public abstract ushort MaxExperience { get; }
        public abstract byte StaminaPerAttack { get; }
        public abstract byte StaminaPerMove { get; }
        public abstract byte StaminaPerLevel { get; }

        protected abstract byte attack { get; }
        public byte Attack => (byte)(UpgradeBonuses[0] + attack + AttackBonus);

        protected abstract byte armor { get; }
        public byte Armor => (byte)(UpgradeBonuses[1] + armor + ArmorBonus);

        protected abstract byte piece_armor { get; }
        public byte PieceArmor => (byte)(UpgradeBonuses[2] + piece_armor + ArmorBonus);

        protected abstract ushort max_stamina { get; }
        public ushort MaxStamina => (ushort)(max_stamina + StaminaBonus);

        protected virtual byte range { get; } = 1;
        public byte Range => (byte)(range + UpgradeBonuses[3]);
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
    }

    [Serializable]
    public class Paladin : Unit
    {
        public override UnitType Type => UnitType.Cavalery;

        protected override byte attack => 14;
        protected override byte armor => 4;
        protected override byte piece_armor => 4;
        protected override ushort max_stamina => 155;
        public override byte LineOfSight => 5;
        public override ushort MaxHealth => 180;
        public override byte StaminaPerAttack => 50;
        public override byte StaminaPerLevel => 25;
        public override byte StaminaPerMove => 30;
        public override bool IsRanged => false;

        public override ushort MaxExperience => 5000;

        public Paladin(byte player) : base(player)
        {
            
        }
    }

    [Serializable]
    public class Archer : Unit
    {
        public override UnitType Type => UnitType.Archers;

        protected override byte attack => 6;
        protected override byte armor => 0;
        protected override byte piece_armor => 0;
        protected override ushort max_stamina => 80;
        public override byte LineOfSight => 5;
        public override ushort MaxHealth => 30;
        public override byte StaminaPerAttack => 80/3;
        public override byte StaminaPerLevel => 15;
        public override byte StaminaPerMove => 80/5;
        public override bool IsRanged => true;
        protected override byte range => 4;

        public override ushort MaxExperience => 3100;

        public Archer(byte player) : base(player)
        {

        }
    }
}
