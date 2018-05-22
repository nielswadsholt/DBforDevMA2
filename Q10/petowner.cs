namespace Q10
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("petowner")]
    public partial class petowner
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public petowner()
        {
            pets = new HashSet<pet>();
        }

        [Key]
        public int ownerid { get; set; }

        [StringLength(50)]
        public string name { get; set; }

        [StringLength(100)]
        public string address { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<pet> pets { get; set; }
    }
}
