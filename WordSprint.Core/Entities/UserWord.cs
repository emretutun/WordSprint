using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace WordSprint.Core.Entities;

public class UserWord
{
    public int Id { get; set; }

    public string UserId { get; set; } = default!;
    public int WordId { get; set; }

    public bool IsLearned { get; set; } // %70+ geçince true
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastTestedAtUtc { get; set; }

   
    

    public Word Word { get; set; } = default!;
}
