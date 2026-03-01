
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Web.WebView2.Core;
using System.Linq;
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
            LoadFlownPassengerAmount();
            LoadTopOffices();
            LoadTicketSalesRevenues();
            LoadEmptySeatPercentage();

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
                var groups = schedules.GroupBy(s => s.Date, s => s.Route.FlightTime, (Date, FlightTime) => new
                {
                    date = Date,
                    FlightTime = FlightTime.Average()
                });
                foreach (var group in groups)
                {
                    dailyAverages.Add(group.FlightTime);
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

                var users = tickets
                    .GroupBy(
                        t => t.UserId,
                        t => db.Tickets.Where(tt => tt.UserId == t.Id).ToList(),
                        (UserId, Tickets) => new
                        {
                            userId = UserId,
                            ticketCount = Tickets.Count()
                        })
                    .OrderByDescending(u => u.ticketCount);

                Label[] costumerLabels = { FirstCostumerLabel, SecondCostumerLabel, ThirdCostumerLabel };
                for (int i = 0; i < ((users.Count() >= 3) ? 3 : users.Count()); i++)
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == users.ElementAt(i).userId);
                    costumerLabels[i].Text = String.Format("{0} {1} ({2} Tickets)",
                                                             user.FirstName,
                                                             user.LastName,
                                                             users.ElementAt(i).ticketCount);
                }
            }
        }

        private async void LoadFlownPassengerAmount()
        {
            using (var db = new AirlineContext())
            {

            }
        }

        private async void LoadTopOffices()
        {
            using(var db = new AirlineContext())
            {
                var tickets = db.Tickets
                    .Include(t => t.Schedule)
                    .Include(t => t.User)
                    .Where(t =>
                           t.Schedule.Date >= DateOnly.FromDateTime(report30DaysDate) &&
                           t.Schedule.Date <= DateOnly.FromDateTime(reportStartDate) &&
                           t.Confirmed == true);

                var ticketList = await tickets.ToListAsync();

                List<(int, decimal)> pricesPerOffice = new List<(int, decimal)>();
                var offices = tickets
                    .GroupBy(t => t.User.OfficeId);
                await offices.ForEachAsync((t) =>
                    {
                        var sum = ticketList
                                    .Where(tt => tt.User.OfficeId == t.Key)
                                    .Sum(o => o.Schedule.EconomyPrice);
                        pricesPerOffice.Add((t.Key.Value, sum));
                    });
                pricesPerOffice.OrderByDescending(ppo => decimal.ToInt32(ppo.Item2));

                Label[] officeLabels = { FirstOfficeLabel, SecondOfficeLabel, ThirdOfficeLabel };
                for (int i = 0; i < (pricesPerOffice.Count() >= 3 ? 3 : pricesPerOffice.Count()); i++)
                {
                    var office = await db.Offices.FirstOrDefaultAsync(o => o.Id == pricesPerOffice.ElementAt(i).Item1);
                    officeLabels[i].Text = office.Title;
                }
            }
        }

        private async void LoadTicketSalesRevenues()
        {
            using (var db = new AirlineContext())
            {
                DateOnly yesterdayDate = DateOnly.FromDateTime(reportStartDate.AddDays(-1));
                DateOnly twoDaysAgoDate = DateOnly.FromDateTime(reportStartDate.AddDays(-2));
                DateOnly threeDaysAgoDate = DateOnly.FromDateTime(reportStartDate.AddDays(-3));

                var CabinTypes = await db.CabinTypes.ToListAsync();

                var tickets = db.Tickets
                    .Include(t => t.Schedule)
                    .Include(t => t.User)
                    .Where(t =>
                           t.Schedule.Date >= DateOnly.FromDateTime(report30DaysDate) &&
                           t.Schedule.Date <= DateOnly.FromDateTime(reportStartDate) &&
                           t.Confirmed == true);
                var yesterdayTickets = tickets.Where(t => t.Schedule.Date == yesterdayDate);
                //var yesterdayEconomyRevenue = yesterdayTickets.Where(t => t.CabinTypeId == ).Sum(t => t.Schedule.EconomyPrice);
            }
        }

        private async void LoadEmptySeatPercentage()
        {
            using (var db = new AirlineContext())
            {

            }
        }

    }

}
