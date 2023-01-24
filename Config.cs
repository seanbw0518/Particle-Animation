using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Particles
{
    public class Config
    {
        // framerate in ms
        public int framerate = 8;

        // velocity multiplier (higher = faster)
        public float vMult = 0.02f;

        // charge constant (negative is attractive)
        public float k = -5f;

        // gravity constant (negative is attractive)
        public float g = -0.01f;

        // reactivity constant (negative is attractive)
        public float r = -0.01f;

        // max charge force range
        public int maxKRange = 200;

        // min charge force range
        public int minKRange = 0;

        // max reactivity force range
        public int maxRRange = 25;

        // min reactivity force range
        public int minRRange = 12;

        // energy loss from charge interaction (higher = less loss)
        public double kLoss = 0.99f;

        // energy loss from reaction interaction (higher = less loss)
        public double rLoss = 0.99f;

        // bounce velocity loss (higher = less loss)
        public float bounceLoss = 0.8f;

        // maximum velocity (px/s)
        public float maxV = 500;
    }
}
