namespace Q10
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("treatment")]
    public partial class treatment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public treatment()
        {
            consultations = new HashSet<consultation>();
        }

        public int treatmentid { get; set; }

        [StringLength(100)]
        public string treatmenttext { get; set; }

        public int? price { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<consultation> consultations { get; set; }
    }
}
