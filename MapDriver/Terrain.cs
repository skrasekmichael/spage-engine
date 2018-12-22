using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapDriver
{
    [Serializable]
    public abstract class Terrain
    {
        public double Elevation { get; set; } = 0;
        public string Texture => this.GetType().Name.ToLower();

        public abstract string Color { get; }
        public abstract float Mobility { get; }
        public abstract bool Throughput { get; }
    }

    [Serializable]
    public class Grass : Terrain
    {
        public override string Color => "#00ff00";
        public override float Mobility => 1f; 
        public override bool Throughput => true; 
    }

    [Serializable]
    public class Water : Terrain
    {
        public override string Color => "#1B7399"; 
        public override float Mobility => 0f; 
        public override bool Throughput => false; 
    }

    [Serializable]
    public class Sand : Terrain
    {
        public override string Color => "#ffff00";
        public override float Mobility => 1.5f;
        public override bool Throughput => true;
    }
}
