using GraphQL.ApiGateway.Data;
using GraphQL.ApiGateway.Models;
using GraphQL.ApiGateway.GraphQL.Inputs;
using GraphQL.ApiGateway.GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.ApiGateway.GraphQL.Queries;

[ExtendObjectType("Query")]
public class SensorReadingQueries
{
    [UsePaging(MaxPageSize = 100)]
    [UseProjection]
    [UseFiltering(typeof(SensorReadingFilterInput))]
    [UseSorting]
    public IQueryable<SensorReading> GetSensorReadings(
        [Service] SensorDataDbContext context)
    {
        return context.SensorReadings;
    }

    [UseProjection]
    [UseFiltering(typeof(SensorReadingFilterInput))]
    public IQueryable<SensorReading> GetSensorReadingsWithPagination(
        [Service] SensorDataDbContext context,
        PaginationInputData? pagination = null)
    {
        // Apply default sorting by timestamp DESC to ensure latest values first
        var query = context.SensorReadings
            .OrderByDescending(r => r.Timestamp)
            .AsQueryable();

        if (pagination != null)
        {
            query = query
                .Skip(pagination.Skip)
                .Take(pagination.Take);
        }

        return query;
    }

    public async Task<SensorReading?> GetSensorReadingById(
        [Service] SensorDataDbContext context,
        string id)
    {
        return await context.SensorReadings
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    [UseProjection]
    [UseFiltering(typeof(SensorReadingFilterInput))]
    public IQueryable<SensorReading> GetSensorReadingsByLocation(
        [Service] SensorDataDbContext context,
        string location)
    {
        return context.SensorReadings
            .Where(r => r.Location == location);
    }

    [UseProjection]
    [UseFiltering(typeof(SensorReadingFilterInput))]
    public IQueryable<SensorReading> GetSensorReadingsByType(
        [Service] SensorDataDbContext context,
        string type)
    {
        return context.SensorReadings
            .Where(r => r.Type == type);
    }

    [UseProjection]
    [UseFiltering(typeof(SensorReadingFilterInput))]
    public IQueryable<SensorReading> GetSensorReadingsByTimeRange(
        [Service] SensorDataDbContext context,
        DateTime startTime,
        DateTime endTime)
    {
        return context.SensorReadings
            .Where(r => r.Timestamp >= startTime && r.Timestamp <= endTime);
    }

    public async Task<List<AggregationResult>> GetAggregationsByLocation(
        [Service] SensorDataDbContext context,
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        var query = context.SensorReadings.AsQueryable();

        if (startTime.HasValue)
        {
            query = query.Where(r => r.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(r => r.Timestamp <= endTime.Value);
        }

        var results = await query
            .GroupBy(r => r.Location)
            .Select(g => new AggregationResult
            {
                GroupBy = g.Key,
                Count = g.Count(),
                AverageEnergyConsumption = g.Average(r => r.EnergyConsumption),
                AverageCo2 = g.Average(r => (decimal?)r.Co2),
                AveragePm25 = g.Average(r => (decimal?)r.Pm25),
                AverageHumidity = g.Average(r => (decimal?)r.Humidity),
                TotalEnergyConsumption = g.Sum(r => r.EnergyConsumption)
            })
            .ToListAsync();

        return results;
    }

    public async Task<List<AggregationResult>> GetAggregationsByType(
        [Service] SensorDataDbContext context,
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        var query = context.SensorReadings.AsQueryable();

        if (startTime.HasValue)
        {
            query = query.Where(r => r.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(r => r.Timestamp <= endTime.Value);
        }

        var results = await query
            .GroupBy(r => r.Type)
            .Select(g => new AggregationResult
            {
                GroupBy = g.Key,
                Count = g.Count(),
                AverageEnergyConsumption = g.Average(r => r.EnergyConsumption),
                AverageCo2 = g.Average(r => (decimal?)r.Co2),
                AveragePm25 = g.Average(r => (decimal?)r.Pm25),
                AverageHumidity = g.Average(r => (decimal?)r.Humidity),
                TotalEnergyConsumption = g.Sum(r => r.EnergyConsumption)
            })
            .ToListAsync();

        return results;
    }

    public async Task<List<AggregationResult>> GetAggregationsByTimePeriod(
        [Service] SensorDataDbContext context,
        string period = "hour",
        int? hoursBack = null,
        int? daysBack = null)
    {
        var query = context.SensorReadings.AsQueryable();

        if (hoursBack.HasValue)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hoursBack.Value);
            query = query.Where(r => r.Timestamp >= cutoffTime);
        }
        else if (daysBack.HasValue)
        {
            var cutoffTime = DateTime.UtcNow.AddDays(-daysBack.Value);
            query = query.Where(r => r.Timestamp >= cutoffTime);
        }

        return period.ToLower() switch
        {
            "hour" => await GetHourAggregations(query),
            "day" => await GetDayAggregations(query),
            "week" => await GetWeekAggregations(query),
            "month" => await GetMonthAggregations(query),
            _ => throw new ArgumentException($"Invalid period: {period}. Supported values: hour, day, week, month")
        };
    }

    private async Task<List<AggregationResult>> GetHourAggregations(IQueryable<SensorReading> query)
    {
        var results = await query
            .GroupBy(r => new { r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                g.Key.Hour,
                Count = g.Count(),
                AverageEnergyConsumption = g.Average(r => r.EnergyConsumption),
                AverageCo2 = g.Average(r => (decimal?)r.Co2),
                AveragePm25 = g.Average(r => (decimal?)r.Pm25),
                AverageHumidity = g.Average(r => (decimal?)r.Humidity),
                TotalEnergyConsumption = g.Sum(r => r.EnergyConsumption)
            })
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ThenBy(r => r.Day)
            .ThenBy(r => r.Hour)
            .ToListAsync();

        return results.Select(g => new AggregationResult
        {
            GroupBy = $"{g.Year}-{g.Month:D2}-{g.Day:D2} {g.Hour:D2}:00",
            Count = g.Count,
            AverageEnergyConsumption = g.AverageEnergyConsumption,
            AverageCo2 = g.AverageCo2,
            AveragePm25 = g.AveragePm25,
            AverageHumidity = g.AverageHumidity,
            TotalEnergyConsumption = g.TotalEnergyConsumption
        }).ToList();
    }

    private async Task<List<AggregationResult>> GetDayAggregations(IQueryable<SensorReading> query)
    {
        var results = await query
            .GroupBy(r => new { r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                Count = g.Count(),
                AverageEnergyConsumption = g.Average(r => r.EnergyConsumption),
                AverageCo2 = g.Average(r => (decimal?)r.Co2),
                AveragePm25 = g.Average(r => (decimal?)r.Pm25),
                AverageHumidity = g.Average(r => (decimal?)r.Humidity),
                TotalEnergyConsumption = g.Sum(r => r.EnergyConsumption)
            })
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ThenBy(r => r.Day)
            .ToListAsync();

        return results.Select(g => new AggregationResult
        {
            GroupBy = $"{g.Year}-{g.Month:D2}-{g.Day:D2}",
            Count = g.Count,
            AverageEnergyConsumption = g.AverageEnergyConsumption,
            AverageCo2 = g.AverageCo2,
            AveragePm25 = g.AveragePm25,
            AverageHumidity = g.AverageHumidity,
            TotalEnergyConsumption = g.TotalEnergyConsumption
        }).ToList();
    }

    private async Task<List<AggregationResult>> GetMonthAggregations(IQueryable<SensorReading> query)
    {
        var results = await query
            .GroupBy(r => new { r.Timestamp.Year, r.Timestamp.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count(),
                AverageEnergyConsumption = g.Average(r => r.EnergyConsumption),
                AverageCo2 = g.Average(r => (decimal?)r.Co2),
                AveragePm25 = g.Average(r => (decimal?)r.Pm25),
                AverageHumidity = g.Average(r => (decimal?)r.Humidity),
                TotalEnergyConsumption = g.Sum(r => r.EnergyConsumption)
            })
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToListAsync();

        return results.Select(g => new AggregationResult
        {
            GroupBy = $"{g.Year}-{g.Month:D2}",
            Count = g.Count,
            AverageEnergyConsumption = g.AverageEnergyConsumption,
            AverageCo2 = g.AverageCo2,
            AveragePm25 = g.AveragePm25,
            AverageHumidity = g.AverageHumidity,
            TotalEnergyConsumption = g.TotalEnergyConsumption
        }).ToList();
    }

    private async Task<List<AggregationResult>> GetWeekAggregations(IQueryable<SensorReading> query)
    {
        var results = await query
            .GroupBy(r => new
            {
                r.Timestamp.Year,
                r.Timestamp.DayOfYear
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.DayOfYear,
                Count = g.Count(),
                AverageEnergyConsumption = g.Average(r => r.EnergyConsumption),
                AverageCo2 = g.Average(r => (decimal?)r.Co2),
                AveragePm25 = g.Average(r => (decimal?)r.Pm25),
                AverageHumidity = g.Average(r => (decimal?)r.Humidity),
                TotalEnergyConsumption = g.Sum(r => r.EnergyConsumption),
                SampleDate = g.Min(r => r.Timestamp)
            })
            .ToListAsync();

        return results
            .GroupBy(r => new
            {
                r.Year,
                Week = GetWeekNumber(r.SampleDate)
            })
            .Select(g => new AggregationResult
            {
                GroupBy = $"Year {g.Key.Year}, Week {g.Key.Week}",
                Count = g.Sum(r => r.Count),
                AverageEnergyConsumption = g.Average(r => r.AverageEnergyConsumption),
                AverageCo2 = g.Average(r => r.AverageCo2),
                AveragePm25 = g.Average(r => r.AveragePm25),
                AverageHumidity = g.Average(r => r.AverageHumidity),
                TotalEnergyConsumption = g.Sum(r => r.TotalEnergyConsumption)
            })
            .OrderBy(r => r.GroupBy)
            .ToList();
    }

    private static int GetWeekNumber(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var calendar = culture.Calendar;
        return calendar.GetWeekOfYear(date, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
    }
}

