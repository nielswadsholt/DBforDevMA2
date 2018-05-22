namespace Q10
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DBforDevMA2Model : DbContext
    {
        public DBforDevMA2Model()
            : base("name=DBforDevMA2")
        {
        }

        public virtual DbSet<consultation> consultations { get; set; }
        public virtual DbSet<pet> pets { get; set; }
        public virtual DbSet<petowner> petowners { get; set; }
        public virtual DbSet<species> species { get; set; }
        public virtual DbSet<treatment> treatments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<consultation>()
                .Property(e => e.ownerdescription)
                .IsUnicode(false);

            modelBuilder.Entity<consultation>()
                .Property(e => e.petdescription)
                .IsUnicode(false);

            modelBuilder.Entity<consultation>()
                .HasMany(e => e.treatments)
                .WithMany(e => e.consultations)
                .Map(m => m.ToTable("treatmentline").MapLeftKey("bookid").MapRightKey("treatmentid"));

            modelBuilder.Entity<pet>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<pet>()
                .HasMany(e => e.consultations)
                .WithRequired(e => e.pet1)
                .HasForeignKey(e => e.pet)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<petowner>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<petowner>()
                .Property(e => e.address)
                .IsUnicode(false);

            modelBuilder.Entity<petowner>()
                .HasMany(e => e.pets)
                .WithRequired(e => e.petowner1)
                .HasForeignKey(e => e.petowner)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<species>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<species>()
                .HasMany(e => e.pets)
                .WithRequired(e => e.species)
                .HasForeignKey(e => e.speciesid)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<treatment>()
                .Property(e => e.treatmenttext)
                .IsUnicode(false);
        }
    }
}
