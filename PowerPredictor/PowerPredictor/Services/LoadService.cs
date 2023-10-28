﻿using Microsoft.EntityFrameworkCore;
using PowerPredictor.Models;
using PowerPredictor.Services.Interfaces;
using HtmlAgilityPack;
using System.Globalization;

namespace PowerPredictor.Services
{
    public class LoadService : ILoadService
    {
        private readonly AppDbContext _context;
        public LoadService(AppDbContext dbContext)
        {
            _context = dbContext;
        }
        public  Load? GetLoad(int id)
        {
            return _context.Find<Load>(id);
        }
        public Load? GetLoadByDate(DateTime date)
        {
            return _context.Loads.Where(load => load.Date == date).FirstOrDefault();
        }
        public List<Load> GetLoads(DateTime start, DateTime stop, bool dayInterval = false)
        {
            var query = _context.Loads
                .Where(load => load.Date >= start && load.Date <= stop);
            if (dayInterval)
                query = query.Where(load => load.Date.Hour == 0 && load.Date.Minute == 0);

            return query.ToList();
        }

        public void AddLoad(Load load)
        {
            _context.Loads.Add(load);
            _context.SaveChanges();
        }
        public void AddLoads(IEnumerable<Load> loads)
        {
            _context.Loads.AddRange(loads);
            _context.SaveChanges();
            return;
        }
        public Load UpdateLoad(Load load)
        {
            _context.Loads.Update(load);
            _context.SaveChanges();
            return load;
        }
        public Load? DeleteLoad(int id)
        {
            var load = _context.Loads.Find(id);
            if (load == null)
            {
                return null;
            }
            _context.Loads.Remove(load);
            _context.SaveChanges();
            return load;
        }
        public Load? GetEarliestData()
        {
            return _context.Loads.OrderBy(l => l.Date).FirstOrDefault();
        }

        public Load? GetLatestData()
        {
            return _context.Loads.OrderByDescending(l => l.Date).FirstOrDefault();
        }

        public int GetNumberOfLoads()
        {
            return _context.Loads.Count();
        }

        public int GetNumberOfPredictions()
        {
            return _context.Loads
                .Where(load => load.PPForecastedTotalLoad != null)
                .Count();
        }

        public void DeleteAllLoads()
        {
            _context.Loads.RemoveRange(_context.Loads);
            _context.SaveChanges();
        }

        public  IEnumerable<Load> DownloadLoads(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues)
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
                        var existingLoad = _context.Loads.Where(l => l.Date == dateTime).FirstOrDefault();
                        if (existingLoad is null)
                        {
                            _context.Loads.Add(load);
                        }
                        else
                        {
                            if (overrideValues)
                            {
                                existingLoad.ActualTotalLoad = realLoad;
                                existingLoad.PSEForecastedTotalLoad = pseForecast;
                                _context.Entry(existingLoad).State = EntityState.Modified;
                            }
                        }
                    }
                }
                currentDay++;
                _context.SaveChanges();
            } 
            return loads;
        }

        public List<DateTime> GetMissingRealLoad()
        {
            var result = _context.Loads
                .Where(l => l.ActualTotalLoad == null)
                .Select(l => l.Date)
                .ToList();
            return result ?? new List<DateTime>();
        }

        public List<DateTime> GetMissingPPForecast()
        {
            var firstLoad = GetEarliestData();

            var result = _context.Loads
                .Where(l => l.PPForecastedTotalLoad == null)
                .Select(l => l.Date)
                .Where(l => l > firstLoad.Date.AddDays(168))
                .ToList();

            return result ?? new List<DateTime>();
        }

        public int InterpolateMissingRealLoad()
        {
            var result = _context.Loads
                .Where(l => l.ActualTotalLoad == null)
                .OrderBy(l => l.Date)
                .ToList();

            int count = 0;
            foreach (var load in result)
            {
                var previousLoad = _context.Loads
                    .Where(l => l.Date < load.Date && l.ActualTotalLoad != null)
                    .OrderByDescending(l => l.Date)
                    .FirstOrDefault();
                var nextLoad = _context.Loads
                    .Where(l => l.Date > load.Date && l.ActualTotalLoad != null)
                    .OrderBy(l => l.Date)
                    .FirstOrDefault();

                if (previousLoad != null && nextLoad != null)
                {
                    load.ActualTotalLoad = (previousLoad.ActualTotalLoad + nextLoad.ActualTotalLoad) / 2;
                    _context.Entry(load).State = EntityState.Modified;
                    count++;
                }
            }
            _context.SaveChanges();
            return count;
        }
    }
}
