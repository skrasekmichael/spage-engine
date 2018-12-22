using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapDriver
{
    [Serializable]
    public abstract class Building
    {
        public virtual int Max => 0;
        public virtual DrawType DrawType => DrawType.Bottom;
        public virtual float DrawScale => 0.8f;

        public virtual string Texture => this.GetType().Name.ToLower();

        public virtual float Mobility { get; }
        public abstract bool Throughput { get; }
    }

    [Serializable]
    public class Tower : Building
    {
        public override float DrawScale => 1f;
        public override bool Throughput => false;
    }
}
