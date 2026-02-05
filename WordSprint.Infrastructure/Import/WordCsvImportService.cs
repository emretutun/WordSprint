using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WordSprint.Core.Entities;
using WordSprint.Core.Enums;
using WordSprint.Infrastructure.Persistence;
using System.Text;

namespace WordSprint.Infrastructure.Import;

public class WordCsvImportService
{
    private readonly WordSprintDbContext _db;

    public WordCsvImportService(WordSprintDbContext db)
    {
        _db = db;
    }

    public async Task<int> ImportAsync(string csvPath)
    {
        if (!File.Exists(csvPath))
            throw new FileNotFoundException("CSV dosyası bulunamadı.", csvPath);

        var lines = await File.ReadAllLinesAsync(csvPath, Encoding.UTF8);


        if (lines.Length <= 1)
            return 0; // sadece header var

        int addedCount = 0;

        // header'ı atla
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',');

            if (parts.Length < 3)
                continue;

            var english = parts[0].Trim();
            var turkish = parts[1].Trim();
            var levelRaw = parts[2].Trim();

            if (string.IsNullOrWhiteSpace(english))
                continue;

            // level parse
            if (!short.TryParse(levelRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var levelValue))
                continue;

            if (levelValue < 0 || levelValue > 5)
                continue;

            // DB'de var mı?
            bool exists = await _db.Words.AnyAsync(w =>
                w.English.ToLower() == english.ToLower());

            if (exists)
                continue;

            var word = new Word
            {
                English = english,
                Turkish = turkish,
                Level = (CeLevel)levelValue
            };

            _db.Words.Add(word);
            addedCount++;
        }

        await _db.SaveChangesAsync();
        return addedCount;
    }
}
