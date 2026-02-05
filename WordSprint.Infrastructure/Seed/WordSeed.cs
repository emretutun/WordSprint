using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordSprint.Core.Entities;
using WordSprint.Infrastructure.Persistence;
using WordSprint.Core.Enums;


namespace WordSprint.Infrastructure.Seed;

public static class WordSeed
{
    public static async Task SeedAsync(WordSprintDbContext db)
    {
        await db.Database.MigrateAsync();

        
        if (await db.Words.AnyAsync())
            return;

        var words = new List<Word>
        {
            new() { English = "apple", Turkish = "elma" , Level = CeLevel.A1},
            new() { English = "book", Turkish = "kitap" , Level = CeLevel.A1},
            new() { English = "car", Turkish = "araba" , Level = CeLevel.A1},
            new() { English = "water", Turkish = "su"   , Level = CeLevel.A1},
            new() { English = "school", Turkish = "okul"    , Level = CeLevel.A1},
            new() { English = "house", Turkish = "ev"   , Level = CeLevel.A1},
            new() { English = "friend", Turkish = "arkadaş"     , Level = CeLevel.A1},
            new() { English = "computer", Turkish = "bilgisayar"    , Level = CeLevel.A1},
            new() { English = "phone", Turkish = "telefon"  , Level = CeLevel.A1},
            new() { English = "music", Turkish = "müzik"    , Level = CeLevel.A1},
            new() { English = "food", Turkish = "yemek"     , Level = CeLevel.A1},
            new() { English = "city", Turkish = "şehir", Level = CeLevel.A1  },
            new() { English = "family", Turkish = "aile" , Level = CeLevel.A1},
            new() { English = "work", Turkish = "iş" , Level = CeLevel.A1},
            new() { English = "time", Turkish = "zaman" , Level = CeLevel.A1},
            new() { English = "day", Turkish = "gün" , Level = CeLevel.A1},
            new() { English = "night", Turkish = "gece" , Level = CeLevel.A1},
            new() { English = "happy", Turkish = "mutlu"    , Level = CeLevel.A1},
            new() { English = "sad", Turkish = "üzgün" , Level = CeLevel.A1 },
            new() { English = "learn", Turkish = "öğrenmek" , Level = CeLevel.A1},
            new() { English = "teach", Turkish = "öğretmek" , Level = CeLevel.A1},
            new() { English = "travel", Turkish = "seyahat etmek" , Level = CeLevel.A1},
            new() { English = "money", Turkish = "para" , Level = CeLevel.A1},
            new() { English = "health", Turkish = "sağlık" , Level = CeLevel.A1},
            new() { English = "strong", Turkish = "güçlü" , Level = CeLevel.A1},
            new() { English = "weak", Turkish = "zayıf" , Level = CeLevel.A1},
            new() { English = "beautiful", Turkish = "güzel", Level = CeLevel.A1  },
            new() { English = "fast", Turkish = "hızlı", Level = CeLevel.A1  },
            new() { English = "slow", Turkish = "yavaş", Level = CeLevel.A1  },
            new() { English = "important", Turkish = "önemli", Level = CeLevel.A1  },
            
        };

        await db.Words.AddRangeAsync(words);
        await db.SaveChangesAsync();
    }
}
