namespace Q10
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("consultation")]
    public partial class consultation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public consultation()
        {
            treatments = new HashSet<treatment>();
        }

        public int id { get; set; }

        public int pet { get; set; }

        public DateTime starttime { get; set; }

        public DateTime endtime { get; set; }

        [StringLength(200)]
        public string ownerdescription { get; set; }

        [StringLength(1000)]
        public string petdescription { get; set; }

        public bool? payed { get; set; }

        public virtual pet pet1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<treatment> treatments { get; set; }
    }
}
