using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Particles
{
    public class ParticleType
    {
        // charge
        public double charge { get; set; }
        // particles want to reach 0. The closer they are to a particle with a reactivity
        // that when subtracted equals 0, the stronger the acceleration 
        public float reactivity { get; set; }
        // color displayed
        public Color color { get; set; }
        // number of particles
        public int count { get; set; }
        // radius
        public int radius { get; set; }

        public ObservableCollection<ParticleType> ParticleTypes { get; set; }

        public ParticleType(double charge, float reactivity, int[] color, int count, int radius)
        {
            this.charge = charge;
            this.color = Color.FromRgb((byte) color[0], (byte) color[1], (byte) color[2]);
            this.count = count;
            this.reactivity = reactivity;
            this.radius = radius;

            ParticleTypes = new ObservableCollection<ParticleType>();
        }
    }
}
