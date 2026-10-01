using System.Data.Entity;

namespace GodisnjiOdmori.Podaci
{
    public class GodisnjiOdmoriContext : DbContext
    {
        public GodisnjiOdmoriContext(string konekcija) : base(konekcija)
        {
            System.Data.Entity.Database.SetInitializer<GodisnjiOdmoriContext>(null);
            Configuration.LazyLoadingEnabled=false;
            Configuration.ProxyCreationEnabled=false;
        }

        public DbSet<Sektor> Sektori { get; set; }
        public DbSet<Zaposleni> Zaposleni { get; set; }
        public DbSet<Zahtev> Zahtevi { get; set; }
        public DbSet<DanOdmora> DaniOdmora { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sektor>().HasMany(s=>s.Zaposleni).WithRequired(z=>z.SektorPodatak)
                .HasForeignKey(z=>z.SektorID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Zaposleni>().HasMany(z=>z.Zahtevi).WithRequired(z=>z.ZaposleniPodatak)
                .HasForeignKey(z=>z.ZaposleniID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Zahtev>().HasMany(z=>z.DaniOdmora).WithRequired(d=>d.Zahtev)
                .HasForeignKey(d=>d.ZahtevID).WillCascadeOnDelete(true);
            modelBuilder.Entity<Zahtev>().Property(z=>z.RedVerzija).IsRowVersion().IsConcurrencyToken();
            base.OnModelCreating(modelBuilder);
        }
    }
}
