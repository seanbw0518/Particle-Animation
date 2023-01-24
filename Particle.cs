using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Particles
{
    public class Particle
    {
        public double x;
        public double y;
        public double vx;
        public double vy;
        public float reactivity;

        public ParticleType type;

        public Particle(double x, double y, float vx, float vy, ParticleType type, float reactivity)
        {
            this.vx = vx;
            this.vy = vy;
            this.x = x;
            this.y = y;
            this.type = type;
            this.reactivity = reactivity;
        }
    }
}
