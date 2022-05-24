namespace SoftJail.DataProcessor
{
    using Data;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor.ImportDto;
    using System.Linq;
    using System.Text;
    using SoftJail.Data.Models;
    using System.Globalization;
    using SoftJail.Data.Models.Enums;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var departmentObjects = new List<Department>();
            var departmentsCells = JsonConvert.DeserializeObject<IEnumerable<DepartmentCellInputModel>>(jsonString);

            foreach (var departmentCell in departmentsCells)
            {
                if (!IsValid(departmentCell) ||
                    !departmentCell.Cells.All(IsValid) ||
                    !departmentCell.Cells.Any(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var department = new Department
                {
                    Name = departmentCell.Name,
                    Cells = departmentCell.Cells.Select(x => new Cell
                    {
                        CellNumber = x.CellNumber,
                        HasWindow = x.HasWindow,
                    })
                    .ToList()
                };

                departmentObjects.Add(department);
                sb.AppendLine($"Imported {departmentCell.Name} with {departmentCell.Cells.Count()} cells");
            }

            context.Departments.AddRange(departmentObjects);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var prisoners = new List<Prisoner>();
            var prisonersMails = JsonConvert.DeserializeObject<IEnumerable<PrisonerMailInputModel>>(jsonString);

            foreach (var currentPrisoner in prisonersMails)
            {
                if (!IsValid(currentPrisoner) ||
                    !currentPrisoner.Mails.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var isValdReleaseDate = DateTime.TryParseExact(
                    currentPrisoner.ReleaseDate,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime releaseDate);

                var incarcerationDate = DateTime.ParseExact(
                    currentPrisoner.IncarcerationDate,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture);

                var prisoner = new Prisoner
                {
                    FullName = currentPrisoner.FullName,
                    Nickname = currentPrisoner.Nickname,
                    Age = currentPrisoner.Age,
                    Bail = currentPrisoner.Bail,
                    CellId = currentPrisoner.CellId,
                    ReleaseDate = isValdReleaseDate ? (DateTime?)releaseDate : null,
                    IncarcerationDate = incarcerationDate,
                    Mails = currentPrisoner.Mails.Select(x => new Mail
                    {
                        Sender = x.Sender,
                        Description = x.Description,
                        Address = x.Address,
                    })
                    .ToList()
                };

                prisoners.Add(prisoner);
                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var officers = new List<Officer>();
            var sb = new StringBuilder();
            var officersPrisoners = XmlConverter.Deserializer<OfficerPrisonerInputModel>(xmlString, "Officers");

            foreach (var currentOfficer in officersPrisoners)
            {
                if (!IsValid(currentOfficer))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var officer = new Officer
                {
                    FullName = currentOfficer.Name,
                    Salary = currentOfficer.Money,
                    Position = Enum.Parse<Position>(currentOfficer.Position),
                    Weapon = Enum.Parse<Weapon>(currentOfficer.Weapon),
                    DepartmentId = currentOfficer.DepartmentId,
                    OfficerPrisoners = currentOfficer.Prisoners.Select(x => new OfficerPrisoner
                    {
                        PrisonerId = x.Id,
                    })
                    .ToList()
                };

                officers.Add(officer);
                sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count} prisoners)");
            }

            context.Officers.AddRange(officers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}