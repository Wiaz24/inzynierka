using Microsoft.EntityFrameworkCore;
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
        public async Task<Load?> GetLoadAsync(int id)
        {
            return await _context.FindAsync<Load>(id);
        }
        public async Task<Load> AddLoadAsync(Load load)
        {
            _context.Loads.Add(load);
            await _context.SaveChangesAsync();
            return load;
        }
        public async Task AddLoadsAsync(IEnumerable<Load> loads)
        {
            _context.Loads.AddRange(loads);
            await _context.SaveChangesAsync();
            return;
        }
        public async Task<Load> UpdateLoadAsync(Load load)
        {
            _context.Entry(load).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return load;
        }
        public async Task<Load?> DeleteLoadAsync(int id)
        {
            var load = await _context.Loads.FindAsync(id);
            if (load == null)
            {
                return null;
            }
            _context.Loads.Remove(load);
            await _context.SaveChangesAsync();
            return load;
        }
        public async Task<Load?> GetEarliestData()
        {
            var load = await _context.Loads.OrderBy(l => l.Date).FirstOrDefaultAsync();
            return load;
        }

        public async Task<Load?> GetLatestData()
        {
            var load = await _context.Loads.OrderByDescending(l => l.Date).FirstOrDefaultAsync();
            return load;
        }

        public async Task<int> GetNumberOfLoads()
        {
            return await _context.Loads.CountAsync();
        }

        public async Task<int> GetNumberOfPredictions()
        {
            int count = await _context.Loads
                .Where(load => load.PPForecastedTotalLoad != null)
                .CountAsync();

            return count;
        }

        public async Task<IEnumerable<Load>> DownloadLoadsAsync(DateOnly start, DateOnly stop)
        {
            string baseURL = "https://www.wnp.pl/energetyka/notowania/zapotrzebowanie_mocy_kse/?d=";
            var web = new HtmlWeb();
            IEnumerable<Load> loads = new List<Load>();
            for (DateOnly currentDate = start; currentDate <= stop; currentDate = currentDate.AddDays(1))
            {
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
                        _context.Loads.Add(load);
                    }
                }
                await _context.SaveChangesAsync();
            } 
            return loads;
        }
    }
}
