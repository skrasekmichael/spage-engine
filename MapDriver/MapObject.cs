using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapDriver
{
    [Flags]
    public enum DrawType
    {
        Bottom, VerticalCenter
    }

    [Serializable]
    public abstract class MapObject
    {
        protected byte type = 0;
        public virtual byte Max => 0;
        public virtual DrawType DrawType => DrawType.Bottom;
        public virtual float DrawScale => 0.8f;

        public virtual string Texture => this.GetType().Name.ToLower();

        public virtual float Mobility { get; }
        public abstract bool Throughput { get; }
        public abstract string Color { get; }

        public void Rotate()
        {
            type++;
            if (type == Max)
                type = 0;
        }
    }

    [Serializable]
    public class Tree : MapObject
    {
        public override byte Max => 3;
        public override DrawType DrawType => DrawType.VerticalCenter;

        public Tree()
        {
            Random gen = new Random(DateTime.Now.Millisecond);
            type = (byte)gen.Next(Max);
        }

        public override string Texture
        {
            get => this.GetType().Name.ToLower() + type.ToString();
        }

        public override float Mobility => 1.7f;
        public override bool Throughput => true;
        public override string Color => "#88cc88";
    }

    [Serializable]
    public class Rock : MapObject
    {
        public override float DrawScale => 0.6f;

        public override string Texture
        {
            get => this.GetType().Name.ToLower();
        }

        public override bool Throughput => false;
        public override string Color => "#808080";
    }
}
