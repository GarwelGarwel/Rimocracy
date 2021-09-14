using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimocracy.ThingComps
{
    public class CompCitizen : ThingComp
    {
        float loyalty;

        public float Loyalty
        { 
            get => loyalty; 
            set => loyalty = value; 
        }
    }
}
