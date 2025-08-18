
using System.ComponentModel.DataAnnotations.Schema;

namespace ZeiterfassungWeb.Data.Models
{
    public abstract class TimeBlock
    {
        public int Id { get; set; }
        public int PersonalId { get; set; }
        public DateTime? Start { get; set; }
        [NotMapped]
        public DateTime? StartRounded { get => TimeHelper.RoundToNextQuarterHour(Start); }
        public DateTime? End { get; set; } 
        [NotMapped]
        public DateTime? EndRounded { get => TimeHelper.RoundToPreviousQuarterHour(End); }
        public string? Comment { get; set; }
        public abstract bool IsWork { get; }
    }
}
