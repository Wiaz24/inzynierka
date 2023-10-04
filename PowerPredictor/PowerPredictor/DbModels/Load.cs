using System.ComponentModel.DataAnnotations;

namespace PowerPredictor.DbModels
{
    public class Load
    {
        [Key]
        public int Id { get ; set; }
        public DateTime Date { get; set; }
        public float ActualTotalLoad { get; set; }
        public float PSEForecastedTotalLoad { get; set; }
        public float PPForecastedTotalLoad { get; set; }
    }
}
