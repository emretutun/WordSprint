using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordSprint.Core.Entities;

public class Word
{
    public int Id { get; set; }

    public string English { get; set; } = default!;
    public string Turkish { get; set; } = default!;

    // İleride: seviye/örnek cümle/etiket vs eklenebilir
}