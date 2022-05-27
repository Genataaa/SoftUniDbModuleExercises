namespace VaporStore.DataProcessor
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
    {
        public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
        {
            var games = context.Genres.ToList()
                .Where(x => genreNames.Contains(x.Name))
                .Select(x => new
                {
                    Id = x.Id,
                    Genre = x.Name,
                    Games = x.Games.Where(g => g.Purchases.Count > 0).Select(g => new
                    {
                        Id = g.Id,
                        Title = g.Name,
                        Developer = g.Developer.Name,
                        Tags = string.Join(", ", g.GameTags.Select(t => t.Tag.Name)),
                        Players = g.Purchases.Count
                    })
                    .ToList()
                    .OrderByDescending(x => x.Players)
                    .ThenBy(x => x.Id),
                    TotalPlayers = x.Games.Sum(p => p.Purchases.Count())
                })
                .OrderByDescending(x => x.TotalPlayers)
                .ThenBy(x => x.Id);
            
            var result = JsonConvert.SerializeObject(games, Formatting.Indented);
            return result;
        }

        public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
        {
            var users = context.Users.ToList()
                .Where(u => u.Cards.Any(c => c.Purchases.Any(p => p.Type.ToString() == storeType)))
                .Select(x => new UserPurchasesXmlExportModel
                {
                    Username = x.Username,
                    TotalSpent = x.Cards.Sum(
                        c => c.Purchases.Where(p => p.Type.ToString() == storeType)
                        .Sum(p => p.Game.Price)),
                    Purchases = x.Cards.SelectMany(p => p.Purchases)
                        .Where(p => p.Type.ToString() == storeType)
                        .Select(p => new PurchaseXmlExportModel
                        {
                            Card = p.Card.Number,
                            Cvc = p.Card.Cvc,
                            Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                            Game = new GameXmlExportModel
                            {
                                Title = p.Game.Name,
                                Genre = p.Game.Genre.Name,
                                Price = p.Game.Price,
                            }
                        })
                        .OrderBy(p => p.Date)
                        .ToArray()
                })
                .OrderByDescending(x => x.TotalSpent)
                .ThenBy(x => x.Username)
                .ToList();

            var result = XmlConverter.Serialize(users, "Users");
            return result.ToString();
        }
    }
}