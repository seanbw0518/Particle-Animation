using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Particles
{
    class EntryPoint
    {
        [System.STAThreadAttribute()]
        static void Main()
        {
            Particles.App app = new Particles.App();
            app.InitializeComponent();
            app.Run();

        }
    }
}
