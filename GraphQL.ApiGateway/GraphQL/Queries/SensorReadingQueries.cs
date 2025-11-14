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
        [Service(ServiceKind.Resolver)] SensorDataDbContext context)
    {
        return context.SensorReadings;
    }

    [UseProjection]
    [UseFiltering(typeof(SensorReadingFilterInput))]
    [UseSorting]
    public IQueryable<SensorReading> GetSensorReadingsWithPagination(
        [Service(ServiceKind.Resolver)] SensorDataDbContext context,
        PaginationInputData? pagination = null)
    {
        var query = context.SensorReadings.AsQueryable();

        if (pagination != null)
        {
            query = query
                .Skip(pagination.Skip)
                .Take(pagination.Take);
        }

        return query;
    }

    public async Task<SensorReading?> GetSensorReadingById(
        [Service(ServiceKind.Resolver)] SensorDataDbContext context,
        string id)
    {
        return await context.SensorReadings
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    [UseProjection]
    [UseFiltering(typeof(SensorReadingFilterInput))]
    public IQueryable<SensorReading> GetSensorReadingsByLocation(
        [Service(ServiceKind.Resolver)] SensorDataDbContext context,
        string location)
    {
        return context.SensorReadings
            .Where(r => r.Location == location);
    }

    [UseProjection]
    [UseFiltering(typeof(SensorReadingFilterInput))]
    public IQueryable<SensorReading> GetSensorReadingsByType(
        [Service(ServiceKind.Resolver)] SensorDataDbContext context,
        string type)
    {
        return context.SensorReadings
            .Where(r => r.Type == type);
    }

    [UseProjection]
    [UseFiltering(typeof(SensorReadingFilterInput))]
    public IQueryable<SensorReading> GetSensorReadingsByTimeRange(
        [Service(ServiceKind.Resolver)] SensorDataDbContext context,
        DateTime startTime,
        DateTime endTime)
    {
        return context.SensorReadings
            .Where(r => r.Timestamp >= startTime && r.Timestamp <= endTime);
    }

    public async Task<List<AggregationResult>> GetAggregationsByLocation(
        [Service(ServiceKind.Resolver)] SensorDataDbContext context,
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
        [Service(ServiceKind.Resolver)] SensorDataDbContext context,
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
        [Service(ServiceKind.Resolver)] SensorDataDbContext context,
        string period = "hour")
    {
        var query = context.SensorReadings.AsQueryable();

        return period.ToLower() switch
        {
            "hour" => await query
                .GroupBy(r => new { r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour })
                .Select(g => new AggregationResult
                {
                    GroupBy = $"{g.Key.Year}-{g.Key.Month:D2}-{g.Key.Day:D2} {g.Key.Hour:D2}:00",
                    Count = g.Count(),
                    AverageEnergyConsumption = g.Average(r => r.EnergyConsumption),
                    AverageCo2 = g.Average(r => (decimal?)r.Co2),
                    AveragePm25 = g.Average(r => (decimal?)r.Pm25),
                    AverageHumidity = g.Average(r => (decimal?)r.Humidity),
                    TotalEnergyConsumption = g.Sum(r => r.EnergyConsumption)
                })
                .OrderBy(r => r.GroupBy)
                .ToListAsync(),

            "day" => await query
                .GroupBy(r => new { r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day })
                .Select(g => new AggregationResult
                {
                    GroupBy = $"{g.Key.Year}-{g.Key.Month:D2}-{g.Key.Day:D2}",
                    Count = g.Count(),
                    AverageEnergyConsumption = g.Average(r => r.EnergyConsumption),
                    AverageCo2 = g.Average(r => (decimal?)r.Co2),
                    AveragePm25 = g.Average(r => (decimal?)r.Pm25),
                    AverageHumidity = g.Average(r => (decimal?)r.Humidity),
                    TotalEnergyConsumption = g.Sum(r => r.EnergyConsumption)
                })
                .OrderBy(r => r.GroupBy)
                .ToListAsync(),

            "week" => await GetWeekAggregations(query),

            "month" => await query
                .GroupBy(r => new { r.Timestamp.Year, r.Timestamp.Month })
                .Select(g => new AggregationResult
                {
                    GroupBy = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count(),
                    AverageEnergyConsumption = g.Average(r => r.EnergyConsumption),
                    AverageCo2 = g.Average(r => (decimal?)r.Co2),
                    AveragePm25 = g.Average(r => (decimal?)r.Pm25),
                    AverageHumidity = g.Average(r => (decimal?)r.Humidity),
                    TotalEnergyConsumption = g.Sum(r => r.EnergyConsumption)
                })
                .OrderBy(r => r.GroupBy)
                .ToListAsync(),

            _ => throw new ArgumentException($"Invalid period: {period}. Supported values: hour, day, week, month")
        };
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

