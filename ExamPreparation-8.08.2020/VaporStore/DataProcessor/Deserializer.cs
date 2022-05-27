namespace VaporStore.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
    {
        public static string ImportGames(VaporStoreDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var games = JsonConvert.DeserializeObject<IEnumerable<GameImportModel>>(jsonString);

            foreach (var currentGame in games)
            {
                if (!IsValid(currentGame) || currentGame.Tags.Count() == 0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var genre = context.Genres.FirstOrDefault(x => x.Name == currentGame.Genre)
                    ?? new Genre { Name = currentGame.Genre };
                var developer = context.Developers.FirstOrDefault(x => x.Name == currentGame.Developer)
                    ?? new Developer { Name = currentGame.Developer };

                var game = new Game
                {
                    Name = currentGame.Name,
                    Price = currentGame.Price,
                    ReleaseDate = currentGame.ReleaseDate.Value,
                    Developer = developer,
                    Genre = genre,
                };
                foreach (var currentTag in currentGame.Tags)
                {
                    var tag = context.Tags.FirstOrDefault(x => x.Name == currentTag)
                        ?? new Tag { Name = currentTag };
                    game.GameTags.Add(new GameTag { Tag = tag });
                    context.SaveChanges();
                }

                context.Games.Add(game);
                context.SaveChanges();
                sb.AppendLine($"Added {currentGame.Name} ({currentGame.Genre}) with {currentGame.Tags.Count()} tags");
            }
            return sb.ToString().TrimEnd();
        }

        public static string ImportUsers(VaporStoreDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var users = JsonConvert.DeserializeObject<IEnumerable<UserImputModel>>(jsonString);

            foreach (var currentUser in users)
            {
                if (!IsValid(currentUser))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var user = new User
                {
                    FullName = currentUser.FullName,
                    Username = currentUser.Username,
                    Email = currentUser.Email,
                    Age = currentUser.Age,
                    Cards = currentUser.Cards.Select(x => new Card
                    {
                        Cvc = x.Cvc,
                        Number = x.Number,
                        Type = Enum.Parse<CardType>(x.Type)
                    })
                    .ToList()
                };

                context.Users.Add(user);
                context.SaveChanges();
                sb.AppendLine($"Imported {currentUser.Username} with {currentUser.Cards.Count()} cards");
            }
            return sb.ToString().TrimEnd();
        }

        public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
        {
            var sb = new StringBuilder();
            var purchases = XmlConverter.Deserializer<PurchaseImputModel>(xmlString, "Purchases");

            foreach (var currentPurechase in purchases)
            {
                if (!IsValid(currentPurechase))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var isValidPurchaseDate = DateTime.TryParseExact(
                    currentPurechase.Date,
                    "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime purchaseDate);

                var purchase = new Purchase
                {
                    Type = Enum.Parse<PurchaseType>(currentPurechase.Type),
                    ProductKey = currentPurechase.Key,
                    Date = purchaseDate,
                };
                purchase.Card = context.Cards.FirstOrDefault(x => x.Number == currentPurechase.Card);
                purchase.Game = context.Games.FirstOrDefault(x => x.Name == currentPurechase.Title);
                context.Purchases.Add(purchase);
                context.SaveChanges();
                sb.AppendLine($"Imported {currentPurechase.Title} for {purchase.Card.User.Username}");
            }
            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }

    }
}