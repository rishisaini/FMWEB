using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AssetCare.Model
{
    [Table("ReactiveSubCategory")]
    public class ReactiveSubCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ReactiveSubCategoryID { get; set; }

        [Required]
        [StringLength(4)]
        public string ReactiveSubCategoryCode { get; set; }

        [Required]
        [StringLength(50)]
        public string ReactiveSubCategoryDesc { get; set; }

        [Required]
        public bool InList { get; set; }
    }
}
