using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerPredictor.Models
{
    /// <summary>
    /// Power grid load model
    /// </summary>
    public class Load
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Real total load value in MW
        /// </summary>
        public float ActualTotalLoad { get; set; }

        /// <summary>
        /// KSE forecasted total load value in MW
        /// </summary>
        public float PSEForecastedTotalLoad { get; set; }

        /// <summary>
        /// Power predictor forecasted total load value in MW
        /// </summary>
        public float? PPForecastedTotalLoad { get; set; }
    }
}
