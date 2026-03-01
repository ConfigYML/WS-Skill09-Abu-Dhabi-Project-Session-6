
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Web.WebView2.Core;
using Windows.ApplicationModel.VoiceCommands;

namespace Session_6_Dennis_Hilfinger
{
    public partial class MainPage : ContentPage
    {
        private DateTime reportStartDate = new DateTime(2017, 10, 29, 0, 0, 0);
        private DateTime report30DaysDate;
        private DateTime reportGenerationStart;
        public MainPage()
        {
            InitializeComponent();
            reportGenerationStart = DateTime.UtcNow;
            report30DaysDate = reportStartDate.AddDays(-30);
            LoadFlightData();
            LoadTopCostumers();
            TimeSpan generationTime = DateTime.UtcNow - reportGenerationStart;
            GenerationTimeLabel.Text = $"Report generated in {generationTime.Minutes} minutes and {generationTime.Seconds} seconds";
        }

        private async void LoadFlightData()
        {
            using(var db = new AirlineContext())
            {
                var schedules = db.Schedules
                    .Include(s => s.Route)
                    .Where(s => 
                    s.Date >= DateOnly.FromDateTime(report30DaysDate) && 
                    s.Date <= DateOnly.FromDateTime(reportStartDate));

                var confirmedCount = schedules.Where(s => s.Confirmed == true).Count();
                var cancelledCount = schedules.Where(s => s.Confirmed == false).Count();
                ConfirmedCountLabel.Text = confirmedCount.ToString();
                CancelledCountLabel.Text = cancelledCount.ToString();

                List<double> dailyAverages = new List<double>();
                var groups = schedules.GroupBy(s => s.Date);
                foreach (var group in groups)
                {
                    DateOnly date = DateOnly.Parse(group.Key.ToString());
                    var dailySchedules = await db.Schedules.Where(s => s.Date == date).ToListAsync();
                    double dailyAvg = dailySchedules.Average(s => s.Route.FlightTime);
                    dailyAverages.Add(dailyAvg);
                }
                var averageFlightTime = Math.Round(dailyAverages.Average(), 2);
                AvgFlightTimeLabel.Text = $"{averageFlightTime} minutes";
            }
        }

        private async void LoadTopCostumers()
        {
            using (var db = new AirlineContext())
            {
                var tickets = db.Tickets
                    .Include(t => t.Schedule)
                    .Where(t =>
                           t.Schedule.Date >= DateOnly.FromDateTime(report30DaysDate) &&
                           t.Schedule.Date <= DateOnly.FromDateTime(reportStartDate) &&
                           t.Confirmed == true);
                int i = 0;
            }
        }

    }

}
