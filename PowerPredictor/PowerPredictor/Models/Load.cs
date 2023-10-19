using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerPredictor.Models
{
    public class Load
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public float ActualTotalLoad { get; set; }
        public float PSEForecastedTotalLoad { get; set; }
        public float? PPForecastedTotalLoad { get; set; }
    }
}
