using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordSprint.Core.Entities
{
    public class UserDailyActivity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public DateTime DayUtc { get; set; } // sadece Date kısmı kullanılacak

        public int LearnedCount { get; set; } // o gün learned yapılan kelime sayısı
        public int QuizCount { get; set; }    // o gün çözülen quiz sayısı

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

}
