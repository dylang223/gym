using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gym
{
    public class WorkoutStat
    {
        public required string StatName { get; set; }
        public required string Value { get; set; }
        public required string Description { get; set; }
        public string Icon { get; set; } = "📊"; // Default icon
    }
}
