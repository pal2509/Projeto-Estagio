using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Models
{
    public partial class fourmeteoContext : DbContext
    {
        public fourmeteoContext()
        {
        }

        public fourmeteoContext(DbContextOptions<fourmeteoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Station> Stations { get; set; }
        public virtual DbSet<Stationrecord> Stationrecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                //               optionsBuilder.UseNpgsql("Host=localhost;Database=fourmeteo;Username=postgres;Password=paulocapa50");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("timescaledb")
                .HasPostgresExtension("uuid-ossp")
                .HasAnnotation("Relational:Collation", "en_US.UTF-8");

            modelBuilder.Entity<Station>(entity =>
            {
                entity.ToTable("station");

                entity.Property(e => e.Stationid)
                    .HasColumnName("stationid")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Createdat).HasColumnName("createdat");

                entity.Property(e => e.Createdby)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("createdby");

                entity.Property(e => e.Latitude).HasColumnName("latitude");

                entity.Property(e => e.Leafwetnessunit)
                    .HasMaxLength(255)
                    .HasColumnName("leafwetnessunit");

                entity.Property(e => e.Longitude).HasColumnName("longitude");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Precipitationunit)
                    .HasMaxLength(255)
                    .HasColumnName("precipitationunit");

                entity.Property(e => e.Pressureunit)
                    .HasMaxLength(255)
                    .HasColumnName("pressureunit");

                entity.Property(e => e.Radiationunit)
                    .HasMaxLength(255)
                    .HasColumnName("radiationunit");

                entity.Property(e => e.Soilmoistunit)
                    .HasMaxLength(255)
                    .HasColumnName("soilmoistunit");

                entity.Property(e => e.Soiltempunit)
                    .HasMaxLength(255)
                    .HasColumnName("soiltempunit");

                entity.Property(e => e.Tempunit)
                    .HasMaxLength(255)
                    .HasColumnName("tempunit");

                entity.Property(e => e.Updatedat).HasColumnName("updatedat");

                entity.Property(e => e.Updatedby)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("updatedby");

                entity.Property(e => e.Windspeedunit)
                    .HasMaxLength(255)
                    .HasColumnName("windspeedunit");
            });

            modelBuilder.Entity<Stationrecord>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("stationrecord");

                entity.HasIndex(e => e.Time, "stationrecord_time_idx")
                    .HasSortOrder(new[] { SortOrder.Descending });

                entity.Property(e => e.Customd1).HasColumnName("customd1");

                entity.Property(e => e.Customd2).HasColumnName("customd2");

                entity.Property(e => e.Customd3).HasColumnName("customd3");

                entity.Property(e => e.Customd4).HasColumnName("customd4");

                entity.Property(e => e.Customd5).HasColumnName("customd5");

                entity.Property(e => e.Customt1).HasColumnName("customt1");

                entity.Property(e => e.Customt2).HasColumnName("customt2");

                entity.Property(e => e.Customt3).HasColumnName("customt3");

                entity.Property(e => e.Customt4).HasColumnName("customt4");

                entity.Property(e => e.Customt5).HasColumnName("customt5");

                entity.Property(e => e.Humidity).HasColumnName("humidity");

                entity.Property(e => e.Leafwetness).HasColumnName("leafwetness");

                entity.Property(e => e.Precipitation).HasColumnName("precipitation");

                entity.Property(e => e.Pressure).HasColumnName("pressure");

                entity.Property(e => e.Radiation).HasColumnName("radiation");

                entity.Property(e => e.Soilmoisture1).HasColumnName("soilmoisture1");

                entity.Property(e => e.Soilmoisture2).HasColumnName("soilmoisture2");

                entity.Property(e => e.Soilmoisture3).HasColumnName("soilmoisture3");

                entity.Property(e => e.Soiltemperature1).HasColumnName("soiltemperature1");

                entity.Property(e => e.Soiltemperature2).HasColumnName("soiltemperature2");

                entity.Property(e => e.Soiltemperature3).HasColumnName("soiltemperature3");

                entity.Property(e => e.Stationid).HasColumnName("stationid");

                entity.Property(e => e.Temperature).HasColumnName("temperature");

                entity.Property(e => e.Time)
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("time");

                entity.Property(e => e.Winddir).HasColumnName("winddir");

                entity.Property(e => e.Windspeed).HasColumnName("windspeed");

                //     entity.HasOne(d => d.Station)
                //         .WithMany()
                //         .HasForeignKey(d => d.Stationid)
                //         .HasConstraintName("fkstationrec880227");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
