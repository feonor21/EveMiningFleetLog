using EveMiningFleet.Entities.Tables;
using Microsoft.EntityFrameworkCore;

namespace EveMiningFleet.Entities
{

    public partial class EveMiningFleetContext : DbContext
    {
        // https://docs.microsoft.com/fr-fr/aspnet/core/data/ef-mvc/migrations?view=aspnetcore-5.0
        // dotnet ef migrations add "nom de la migration"

        public EveMiningFleetContext()
        {
        }

        public EveMiningFleetContext(DbContextOptions<EveMiningFleetContext> options)
            : base(options)
        {
        }


        public virtual DbSet<Character> Characters { get; set; }
        public virtual DbSet<Fleet> Fleets { get; set; }
        public virtual DbSet<Fleetcharacter> Fleetcharacters { get; set; }
        public virtual DbSet<Fleetgroup> Fleetgroups { get; set; }
        public virtual DbSet<Fleetgroupcharacter> Fleetgroupcharacters { get; set; }
        public virtual DbSet<Fleettaxes> Fleettaxes { get; set; }
        public virtual DbSet<Invtypematerial> Invtypematerials { get; set; }
        public virtual DbSet<Lastmininglog> Lastmininglogs { get; set; }
        public virtual DbSet<Mininglog> Mininglogs { get; set; }
        public virtual DbSet<Ore> Ores { get; set; }
        public virtual DbSet<DataPrice> Dataprices { get; set; }
        public virtual DbSet<Corporation> Corporations { get; set; }
        public virtual DbSet<Alliance> Alliances { get; set; }
        public virtual DbSet<AlerteMessage> AlerteMessages { get; set; }
        public virtual DbSet<UsageHistory> UsageHistory { get; set; }




        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //penser a marquer en dure la chaine de connexion quand on fais une migration....
                var connString = System.Environment.GetEnvironmentVariable("DB_DATA_connectionstring");

                optionsBuilder.UseMySql(connString,
                mySqlOptions =>
                {
                    mySqlOptions.ServerVersion(new System.Version(5, 7, 31), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql)
                    .EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: System.TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                }
                );
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AlerteMessage>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            });
            modelBuilder.Entity<Alliance>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            });
            modelBuilder.Entity<Character>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });
            modelBuilder.Entity<Corporation>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            });
            modelBuilder.Entity<DataPrice>(entity =>
            {
                entity.HasKey(e => new { e.TypeId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            });
            modelBuilder.Entity<Fleet>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasOne(d => d.Character)
                    .WithMany(p => p.Fleets)
                    .HasForeignKey(d => d.CharacterId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Fleetcharacter>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasOne(d => d.Character)
                    .WithMany(p => p.Fleetcharacters)
                    .HasForeignKey(d => d.CharacterId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Fleet)
                    .WithMany(p => p.Fleetcharacters)
                    .HasForeignKey(d => d.FleetId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Fleetgroup>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasOne(d => d.Fleet)
                    .WithMany(p => p.Fleetgroups)
                    .HasForeignKey(d => d.FleetId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Fleetgroupcharacter>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasOne(d => d.Character)
                    .WithMany(p => p.Fleetgroupcharacters)
                    .HasForeignKey(d => d.CharacterId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Fleetgroup)
                    .WithMany(p => p.Fleetgroupcharacters)
                    .HasForeignKey(d => d.FleetgroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Fleettaxes>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasOne(d => d.Character)
                    .WithMany(p => p.Fleettaxes)
                    .HasForeignKey(d => d.CharacterId);

                entity.HasOne(d => d.Fleet)
                    .WithMany(p => p.Fleettaxes)
                    .HasForeignKey(d => d.FleetId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Invtypematerial>(entity =>
            {
                entity.HasKey(e => new { e.TypeId, e.MaterialTypeId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<Lastmininglog>(entity =>
            {
                entity.HasKey(e => new { e.CharacterId, e.OreId, e.Date })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

                entity.HasOne(d => d.Character)
                    .WithMany(p => p.Lastmininglogs)
                    .HasForeignKey(d => d.CharacterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_lastMiningLogs_characters_Character_Id");

                entity.HasOne(d => d.Ore)
                    .WithMany(p => p.Lastmininglogs)
                    .HasForeignKey(d => d.OreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_lastMiningLogs_ores_Ore_Id");
            });

            modelBuilder.Entity<Mininglog>(entity =>
            {
                entity.HasKey(e => new { e.FleetCharacterId, e.OreId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasOne(d => d.FleetCharacter)
                    .WithMany(p => p.Mininglogs)
                    .HasForeignKey(d => d.FleetCharacterId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Ore)
                    .WithMany(p => p.Mininglogs)
                    .HasForeignKey(d => d.OreId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Ore>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });
            modelBuilder.Entity<UsageHistory>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                entity.HasIndex(b => b.date);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }


}
