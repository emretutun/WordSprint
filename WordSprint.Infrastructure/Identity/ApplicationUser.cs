using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordSprint.Core.Enums;

namespace WordSprint.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public int DailyWordGoal { get; set; } = 10;
    

    public string? ProfilePhotoFileName { get; set; }
    public CeLevel Level { get; set; } = CeLevel.A1;

}
