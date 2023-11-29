using Microsoft.EntityFrameworkCore;
using PowerPredictor.Models;
using PowerPredictor.Services.Interfaces;
using HtmlAgilityPack;
using System.Globalization;

namespace PowerPredictor.Services
{
    public class LoadService : ILoadService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        public LoadService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public Load? GetLoadById(int id)
        {
            using(var context = _contextFactory.CreateDbContext())
                return context.Find<Load>(id);
        }

        public async Task<List<Load>> GetLoadsAsync(DateTime start, DateTime stop, bool dayInterval = false)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var query = context.Loads
                    .Where(load => load.Date >= start && load.Date <= stop);
                if (dayInterval)
                    query = query.Where(load => load.Date.Hour == 0 && load.Date.Minute == 0);

                return await query.ToListAsync();
            }   
        }

        public async Task AddLoadsAsync(IEnumerable<Load> loads)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Loads.AddRange(loads);
                await context.SaveChangesAsync();
            }
        }

        public Load? GetEarliestData()
        {
            using (var context = _contextFactory.CreateDbContext())
                return context.Loads.OrderBy(l => l.Date).FirstOrDefault();
        }

        public Load? GetLatestData()
        {
            using (var context = _contextFactory.CreateDbContext())
                return context.Loads.OrderByDescending(l => l.Date).FirstOrDefault();
        }

        public int GetNumberOfLoads()
        {
            using (var context = _contextFactory.CreateDbContext())
                return context.Loads.Count();
        }

        public int GetNumberOfPredictions()
        {
            using (var context = _contextFactory.CreateDbContext())
                return context.Loads
                .Where(load => load.PPForecastedTotalLoad != null)
                .Count();
        }

        public async Task DeleteAllLoadsAsync()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var allLoads = await context.Loads.ToListAsync();
                context.Loads.RemoveRange(allLoads);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Load>> DownloadLoadsAsync(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                string baseURL = "https://www.wnp.pl/energetyka/notowania/zapotrzebowanie_mocy_kse/?d=";
                var web = new HtmlWeb();
                IEnumerable<Load> loads = new List<Load>();

                int numOfDays = (stop.DayNumber - start.DayNumber) + 1;
                int percent = 0;
                int currentDay = 0;

                for (DateOnly currentDate = start; currentDate <= stop; currentDate = currentDate.AddDays(1))
                {
                    if (progress != null)
                    {
                        percent = (int)Math.Round((double)currentDay / numOfDays * 100);
                        progress.Report(percent);
                    }
                    string url = baseURL + currentDate.ToString("yyyy-MM-dd");
                    var doc = web.Load(url);
                    var table = doc.DocumentNode.SelectNodes("//tbody")[1];

                    if (table != null)
                    {
                        foreach (HtmlNode row in table.SelectNodes("tr"))
                        {
                            HtmlNodeCollection cells = row.SelectNodes("td");
                            TimeOnly time = new TimeOnly();
                            DateTime dateTime = new DateTime();
                            if (cells[0].InnerText == "24:00")
                            {
                                time = TimeOnly.Parse("00:00");
                                dateTime = currentDate.ToDateTime(time).AddDays(1);
                            }
                            else
                            {
                                time = TimeOnly.Parse(cells[0].InnerText);
                                dateTime = currentDate.ToDateTime(time);
                            }

                            string pseForecastString = cells[1].InnerText.Replace("&nbsp;", "").Replace(",", ".");
                            string realLoadString = cells[2].InnerText.Replace("&nbsp;", "").Replace(",", ".");
                            float pseForecast = float.Parse(pseForecastString, CultureInfo.InvariantCulture);
                            float realLoad = float.Parse(realLoadString, CultureInfo.InvariantCulture);

                            Load load = new Load
                            {
                                Date = dateTime,
                                ActualTotalLoad = realLoad,
                                PSEForecastedTotalLoad = pseForecast
                            };
                            loads.Append(load);

                            //find if load with provided date already exists. Date is not primary key
                            var existingLoad = context.Loads.Where(l => l.Date == dateTime).FirstOrDefault();
                            if (existingLoad is null)
                            {
                                context.Loads.Add(load);
                            }
                            else
                            {
                                if (overrideValues)
                                {
                                    existingLoad.ActualTotalLoad = realLoad;
                                    existingLoad.PSEForecastedTotalLoad = pseForecast;
                                    context.Loads.Update(existingLoad);
                                }
                            }
                        }
                    }
                    currentDay++;
                    await context.SaveChangesAsync();
                }
                return loads;
            } 
        }

        public List<DateTime> GetMissingRealLoad()
        {
            using(var context = _contextFactory.CreateDbContext())
            {
                if (context.Loads.Count() == 0)
                    return new List<DateTime>();
                var result = context.Loads
                    .Where(l => l.ActualTotalLoad == null)
                    .Select(l => l.Date)
                    .Where(l => l < DateTime.Today)
                    .ToList();
                return result ?? new List<DateTime>();
            }
            
        }

        public List<DateTime> GetMissingPPForecast()
        {
            var firstLoad = GetEarliestData();

            using (var context = _contextFactory.CreateDbContext())
            {
                if (context.Loads.Count() == 0)
                    return new List<DateTime>();
                var result = context.Loads
                    .Where(l => l.PPForecastedTotalLoad == null)
                    .Select(l => l.Date)
                    .Where(l => l > firstLoad.Date.AddDays(168))
                    .ToList();

                return result ?? new List<DateTime>();
            } 
        }

        public async Task<int> InterpolateMissingRealLoadAsync()
        {
            using(var context = _contextFactory.CreateDbContext())
            {
                var result = context.Loads
                .Where(l => l.ActualTotalLoad == null)
                .OrderBy(l => l.Date)
                .ToList();

                int count = 0;
                foreach (var load in result)
                {
                    var previousLoad = context.Loads
                        .Where(l => l.Date < load.Date && l.ActualTotalLoad != null)
                        .OrderByDescending(l => l.Date)
                        .FirstOrDefault();
                    var nextLoad = context.Loads
                        .Where(l => l.Date > load.Date && l.ActualTotalLoad != null)
                        .OrderBy(l => l.Date)
                        .FirstOrDefault();

                    if (previousLoad != null && nextLoad != null)
                    {
                        load.ActualTotalLoad = (previousLoad.ActualTotalLoad + nextLoad.ActualTotalLoad) / 2;
                        context.Entry(load).State = EntityState.Modified;
                        count++;
                    }
                }
                await context.SaveChangesAsync();
                return count;
            }
        }
    }
}
