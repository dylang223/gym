using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gym
{
    public class Workout
    {
        public string Exercise { get; set; }  
        public int Reps { get; set; }        
        public int Sets { get; set; }        
        public double Weight { get; set; }   
        public DateTime Date { get; set; }  
    }
}
