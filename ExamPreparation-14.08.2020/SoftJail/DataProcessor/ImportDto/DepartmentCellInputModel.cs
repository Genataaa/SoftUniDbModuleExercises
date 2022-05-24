namespace SoftJail.DataProcessor.ImportDto
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DepartmentCellInputModel
    {
        public DepartmentCellInputModel()
        {
            this.Cells = new HashSet<CellInputModel>();
        }

        [Required]
        [StringLength(25, MinimumLength = 3)]
        public string Name { get; set; }

        public IEnumerable<CellInputModel> Cells { get; set; }
    }
}
