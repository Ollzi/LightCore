using LightCore.Business.Entities;
using LightCore.Contracts;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;

namespace LightCore.Business
{
    public class LightManager
    {
        private readonly ITelldus _telldus;
        private readonly IWeatherProvider _weatherProvider;
        private DateTime? _sunset;
        private DateTime? _unitTestDateTime;
        private readonly Collection<LampSection> _sections;

        public LightManager()
            : this(new TelldusManager(), new WeatherProvider())
        {
        }

        public LightManager(ITelldus telldus, IWeatherProvider weatherProvider)
        {
            _telldus = telldus;
            _weatherProvider = weatherProvider;
            _sections = new Collection<LampSection>();
            SetupSections();
        }

        private void SetupSections()
        {
            _sections.Add(new LampSection
            {
                SectionName = "lampor",
                WeekdayStopTime = new TimeSpan(21, 40, 0),
                WeekendStopTime = new TimeSpan(03, 00, 00),
                SubSections = new Collection<string>
                {
                    "Hall",
                    "Sovrum"
                }
            });

            _sections.Add(new LampSection
            {
                SectionName = "Hall",
                WeekdayStopTime = new TimeSpan(23, 00, 00),
                WeekendStopTime = new TimeSpan(03, 00, 00),
                WeekdayStartTime = new TimeSpan(21, 40, 00),
                SubSections = new Collection<string>()
            });

            _sections.Add(new LampSection
            {
                SectionName = "Sovrum",
                WeekdayStopTime = new TimeSpan(21, 25, 00),
                SubSections = new Collection<string>()
            });


        }

        public void Run()
        {
            Console.WriteLine("Startar Lamphanteraren");

            var timer = new Timer();
            timer.Interval = 60000;
            //timer.Interval = 10000;

            timer.Elapsed += timer_Elapsed;
            timer.Start();
            TimerElapsed();
        }

        public void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimerElapsed();
        }

        public void TimerElapsed()
        {
            if (!_sunset.HasValue || _sunset.Value.Date != DateTimeNow.Date && DateTimeNow.Hour > 10)
            {
                try
                {
                    _sunset = _weatherProvider.GetSunsetTime();
                    Console.WriteLine($"{DateTimeNow:yyyy-MM-dd}: Solnedgång: {_sunset.Value: yyyy-MM-dd HH:mm}");
                    _sections.ForEach(s => s.OnStateHandled = false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Misslyckades att hämta tid för solnedgång: { ex.Message }");
                    return;
                }

            }

            var timeLeft = _sunset.Value - DateTimeNow;

            if (_sections[0].State == State.Off && (timeLeft.TotalMinutes <= 60 && _sections[0].OnStateHandled == false))
            {
                Console.WriteLine($"{DateTimeNow:yyyy-MM-dd}: Skickar startsignal till alla lampor {DateTimeNow:HH:mm}");
                _telldus.TurnOn("lampor");
                _sections[0].State = State.On;
                _sections[0].OnStateHandled = true;
                _sections.ForEach(s => s.State = State.On);
                return;
            }

            if (IsWeekday())
            {
                foreach (var section in _sections)
                {
                    if (section.State == State.On)
                    {
                        TurnOffSectionIfTimePassed(section, section.WeekdayStopTime);
                    }

                    if (section.WeekdayStartTime.HasValue && section.State == State.Off && section.OnStateHandled == false)
                    {
                        TurnOnSectionIfTimeHasPassed(section, section.WeekdayStartTime.Value);
                    }
                }
            }
            else
            {
                // Always on weekends
                foreach (var section in _sections)
                {
                    if (section.State == State.On)
                    {
                        TurnOffSectionIfTimePassed(section, (section.WeekendStopTime ?? section.WeekdayStopTime));
                    }

                    if (section.WeekdayStartTime.HasValue && section.State == State.Off)
                    {
                        TurnOnSectionIfTimeHasPassed(section, section.WeekdayStartTime.Value);
                    }
                }
            }
        }

        private void TurnOffSectionIfTimePassed(LampSection section, TimeSpan stopTime)
        {
            var compareTime = CreateCompareDateTime(stopTime);
            if (DateTimeNow > compareTime && section.State == State.On)
            {
                Console.WriteLine($"{DateTimeNow:yyyy-MM-dd}: Skickar stoppsignal till {section.SectionName} {DateTimeNow:HH:mm}");
                _telldus.TurnOff(section.SectionName);
                section.State = State.Off;
                if (section.SubSections.Count > 0)
                {
                    section.SubSections.ForEach(s => _sections.First(x => s == x.SectionName).State = State.Off);
                }
            }
        }

        private void TurnOnSectionIfTimeHasPassed(LampSection section, TimeSpan startTime)
        {
            var compareTime = CreateCompareDateTime(startTime);
            if (DateTimeNow >= compareTime && section.State == State.Off)
            {
                Console.WriteLine($"{DateTimeNow:yyyy-MM-dd}: Skickar startsignal till {section.SectionName} {DateTimeNow:HH:mm}");
                _telldus.TurnOn(section.SectionName);
                section.State = State.On;
                section.OnStateHandled = true;
            }
        }

        private DateTime CreateCompareDateTime(TimeSpan time)
        {
            DateTime compareDateTime;

            if (DateTimeNow.Hour > time.Hours)
            {
                compareDateTime = new DateTime(DateTimeNow.Year, DateTimeNow.Month, (DateTimeNow.Day + 1), time.Hours, time.Minutes, 0);
            }
            else
            {
                compareDateTime = new DateTime(DateTimeNow.Year, DateTimeNow.Month, DateTimeNow.Day, time.Hours, time.Minutes, 0);
            }

            return compareDateTime;
        }

        public DateTime DateTimeNow
        {
            get
            {
                if (_unitTestDateTime.HasValue)
                {
                    return _unitTestDateTime.Value;
                }

                return DateTime.Now;
            }
            set
            {
                _unitTestDateTime = value;
            }
        }

        private bool IsWeekday()
        {
            if (DateTimeNow.Hour <= 3 && DateTimeNow.DayOfWeek == DayOfWeek.Sunday)
            {
                return DateTimeNow.DayOfWeek != DayOfWeek.Friday && DateTimeNow.DayOfWeek != DayOfWeek.Saturday && DateTimeNow.DayOfWeek != DayOfWeek.Sunday;
            }

            return DateTimeNow.DayOfWeek != DayOfWeek.Friday && DateTimeNow.DayOfWeek != DayOfWeek.Saturday;
        }
    }
}
