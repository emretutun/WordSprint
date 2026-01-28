using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WordSprint.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public int DailyWordGoal { get; set; } = 10;
    public string? EstimatedLevel { get; set; }

    public string? ProfilePhotoFileName { get; set; }
}
