using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordSprint.Core.Enums;

namespace WordSprint.Core.Entities;

public class Word
{
    public int Id { get; set; }

    public string English { get; set; } = default!;
    public string Turkish { get; set; } = default!;
    public CeLevel Level { get; set; } = CeLevel.A1;

    
}