namespace SoftJail.DataProcessor.ImportDto
{
	using System;
	using SoftJail.Data.Models.Enums;
	using System.ComponentModel.DataAnnotations;
	using System.Xml.Serialization;

	[XmlType("Officer")]
    public class OfficerPrisonerInputModel
    {
		[Required]
		[StringLength(30, MinimumLength = 3)]
		[XmlElement("Name")]
        public string Name { get; set; }

		[Required]
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
		[XmlElement("Money")]
		public decimal Money { get; set; }

		[Required]
		[EnumDataType(typeof(Position))]
		[XmlElement("Position")]
        public string Position { get; set; }

		[Required]
		[EnumDataType(typeof(Weapon))]
		[XmlElement("Weapon")]
		public string Weapon { get; set; }

		[Required]
		[XmlElement("DepartmentId")]
		public int DepartmentId { get; set; }

		[XmlArray("Prisoners")]
        public PrisonerInputModel[] Prisoners { get; set; }
    }
}