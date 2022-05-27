using System.ComponentModel.DataAnnotations;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class CardImputModel
    {
        [Required]
        [RegularExpression(@"\b((\d{4}) (\d{4}) (\d{4}) (\d{4}))\b")]
        public string Number { get; set; }

        [Required]
        [RegularExpression(@"\b(\d{3})\b")]
        public string Cvc { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
//"Number": "1111 1111 1111 1111",
//				"CVC": "111",
//				"Type": "Debit"

//•	Number – text, which consists of 4 pairs of 4 digits, separated by spaces (ex. “1234 5678 9012 3456”) (required)
//•	Cvc – text, which consists of 3 digits (ex. “123”) (required)
//•	Type – enumeration of type CardType, with possible values (“Debit”, “Credit”) (required)
//•	UserId – integer, foreign key(required)
//•	User – the card’s user (required)
//•	Purchases – collection of type Purchase