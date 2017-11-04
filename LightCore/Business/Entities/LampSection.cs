using System;
using System.Collections.ObjectModel;

namespace LightCore.Business.Entities
{
    public class LampSection
    {
        public string SectionName { get; set; }

        public TimeSpan WeekdayStopTime { get; set; }

        public TimeSpan? WeekendStopTime { get; set; }

        public TimeSpan? WeekdayStartTime { get; set; }

        public State State { get; set; }

        public Collection<string> SubSections { get; set; }

        public bool OnStateHandled { get; set; }
    }
}
