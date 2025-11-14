using GraphQL.ApiGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.ApiGateway.Data;

public class SensorDataDbContext : DbContext
{
    public SensorDataDbContext(DbContextOptions<SensorDataDbContext> options)
        : base(options)
    {
    }

    public DbSet<SensorReading> SensorReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.ToTable("sensor_readings");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasMaxLength(255);

            entity.Property(e => e.SensorId)
                .HasColumnName("sensor_id")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Location)
                .HasColumnName("location")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();

            entity.Property(e => e.EnergyConsumption)
                .HasColumnName("energy_consumption")
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Co2)
                .HasColumnName("co2");

            entity.Property(e => e.Pm25)
                .HasColumnName("pm25");

            entity.Property(e => e.Humidity)
                .HasColumnName("humidity");

            entity.Property(e => e.MotionDetected)
                .HasColumnName("motion_detected");

            entity.HasIndex(e => e.SensorId)
                .HasDatabaseName("idx_sensor_readings_sensor_id");

            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("idx_sensor_readings_timestamp");

            entity.HasIndex(e => e.Type)
                .HasDatabaseName("idx_sensor_readings_type");
        });
    }
}

