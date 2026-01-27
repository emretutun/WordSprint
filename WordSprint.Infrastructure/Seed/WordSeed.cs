using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordSprint.Core.Entities;
using WordSprint.Infrastructure.Persistence;

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
            new() { English = "apple", Turkish = "elma" },
            new() { English = "book", Turkish = "kitap" },
            new() { English = "car", Turkish = "araba" },
            new() { English = "water", Turkish = "su" },
            new() { English = "school", Turkish = "okul" },
            new() { English = "house", Turkish = "ev" },
            new() { English = "friend", Turkish = "arkadaş" },
            new() { English = "computer", Turkish = "bilgisayar" },
            new() { English = "phone", Turkish = "telefon" },
            new() { English = "music", Turkish = "müzik" },
            new() { English = "food", Turkish = "yemek" },
            new() { English = "city", Turkish = "şehir" },
            new() { English = "family", Turkish = "aile" },
            new() { English = "work", Turkish = "iş" },
            new() { English = "time", Turkish = "zaman" },
            new() { English = "day", Turkish = "gün" },
            new() { English = "night", Turkish = "gece" },
            new() { English = "happy", Turkish = "mutlu" },
            new() { English = "sad", Turkish = "üzgün" },
            new() { English = "learn", Turkish = "öğrenmek" },
            new() { English = "teach", Turkish = "öğretmek" },
            new() { English = "travel", Turkish = "seyahat etmek" },
            new() { English = "money", Turkish = "para" },
            new() { English = "health", Turkish = "sağlık" },
            new() { English = "strong", Turkish = "güçlü" },
            new() { English = "weak", Turkish = "zayıf" },
            new() { English = "beautiful", Turkish = "güzel" },
            new() { English = "fast", Turkish = "hızlı" },
            new() { English = "slow", Turkish = "yavaş" },
            new() { English = "important", Turkish = "önemli" }
        };

        await db.Words.AddRangeAsync(words);
        await db.SaveChangesAsync();
    }
}
