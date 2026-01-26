using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordSprint.Core.Entities;

public class AppUser
{
    public string Id { get; set; } = default!;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public int DailyWordGoal { get; set; } = 10;
    public string? EstimatedLevel { get; set; }
}

