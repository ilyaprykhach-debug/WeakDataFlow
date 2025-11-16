import { useQuery } from '@apollo/client';
import { GET_SENSOR_READINGS, GET_AGGREGATIONS_BY_TIME_PERIOD } from '../graphql/queries';
import type { SensorReading, AggregationResult } from '../types/graphql';
import { LoadingSpinner } from './LoadingSpinner';
import { ErrorDisplay } from './ErrorDisplay';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { useState } from 'react';
import './Charts.css';

export const Charts = () => {
  const [timeRange, setTimeRange] = useState<'1h' | '24h' | '7d'>('24h');
  const [chartType, setChartType] = useState<'time' | 'location' | 'type'>('time');

  const getTimeRangeVariables = () => {
    if (timeRange === '1h') {
      return { period: 'hour' as const, hoursBack: 1 };
    } else if (timeRange === '24h') {
      return { period: 'hour' as const, hoursBack: 24 };
    } else {
      return { period: 'day' as const, daysBack: 7 };
    }
  };

  const { data: timeData, loading: timeLoading, error: timeError } = useQuery<{
    aggregationsByTimePeriod: AggregationResult[];
  }>(GET_AGGREGATIONS_BY_TIME_PERIOD, {
    variables: getTimeRangeVariables(),
    skip: chartType !== 'time',
    fetchPolicy: 'cache-and-network',
    notifyOnNetworkStatusChange: true,
  });

  const { data: readingsData, loading: readingsLoading, error: readingsError } = useQuery<{
    sensorReadingsWithPagination: SensorReading[];
  }>(GET_SENSOR_READINGS, {
    variables: {
      skip: 0,
      take: 100,
    },
    skip: chartType === 'time',
  });

  const loading = chartType === 'time' ? timeLoading : readingsLoading;
  const error = chartType === 'time' ? timeError : readingsError;

  const prepareTimeData = () => {
    if (!timeData?.aggregationsByTimePeriod) return [];
    return timeData.aggregationsByTimePeriod.map((item) => ({
      time: item.groupBy,
      energy: item.averageEnergyConsumption || 0,
      co2: item.averageCo2 || 0,
      pm25: item.averagePm25 || 0,
      humidity: item.averageHumidity || 0,
    }));
  };

  interface LocationGroup {
    location: string;
    energy: number[];
    co2: number[];
    pm25: number[];
    humidity: number[];
  }

  const prepareLocationData = () => {
    if (!readingsData?.sensorReadingsWithPagination) return [];
    const grouped = readingsData.sensorReadingsWithPagination.reduce((acc, reading) => {
      if (!acc[reading.location]) {
        acc[reading.location] = {
          location: reading.location,
          energy: [],
          co2: [],
          pm25: [],
          humidity: [],
        };
      }
      if (reading.energyConsumption) acc[reading.location].energy.push(reading.energyConsumption);
      if (reading.co2) acc[reading.location].co2.push(reading.co2);
      if (reading.pm25) acc[reading.location].pm25.push(reading.pm25);
      if (reading.humidity) acc[reading.location].humidity.push(reading.humidity);
      return acc;
    }, {} as Record<string, LocationGroup>);

    return Object.values(grouped).map((item) => ({
      location: item.location,
      avgEnergy: item.energy.length > 0 ? item.energy.reduce((a: number, b: number) => a + b, 0) / item.energy.length : 0,
      avgCo2: item.co2.length > 0 ? item.co2.reduce((a: number, b: number) => a + b, 0) / item.co2.length : 0,
      avgPm25: item.pm25.length > 0 ? item.pm25.reduce((a: number, b: number) => a + b, 0) / item.pm25.length : 0,
      avgHumidity: item.humidity.length > 0 ? item.humidity.reduce((a: number, b: number) => a + b, 0) / item.humidity.length : 0,
    }));
  };

  interface TypeGroup {
    type: string;
    energy: number[];
    co2: number[];
    pm25: number[];
    humidity: number[];
  }

  const prepareTypeData = () => {
    if (!readingsData?.sensorReadingsWithPagination) return [];
    const grouped = readingsData.sensorReadingsWithPagination.reduce((acc, reading) => {
      if (!acc[reading.type]) {
        acc[reading.type] = {
          type: reading.type,
          energy: [],
          co2: [],
          pm25: [],
          humidity: [],
        };
      }
      if (reading.energyConsumption) acc[reading.type].energy.push(reading.energyConsumption);
      if (reading.co2) acc[reading.type].co2.push(reading.co2);
      if (reading.pm25) acc[reading.type].pm25.push(reading.pm25);
      if (reading.humidity) acc[reading.type].humidity.push(reading.humidity);
      return acc;
    }, {} as Record<string, TypeGroup>);

    return Object.values(grouped).map((item) => ({
      type: item.type,
      avgEnergy: item.energy.length > 0 ? item.energy.reduce((a: number, b: number) => a + b, 0) / item.energy.length : 0,
      avgCo2: item.co2.length > 0 ? item.co2.reduce((a: number, b: number) => a + b, 0) / item.co2.length : 0,
      avgPm25: item.pm25.length > 0 ? item.pm25.reduce((a: number, b: number) => a + b, 0) / item.pm25.length : 0,
      avgHumidity: item.humidity.length > 0 ? item.humidity.reduce((a: number, b: number) => a + b, 0) / item.humidity.length : 0,
    }));
  };

  const chartData = chartType === 'time' ? prepareTimeData() : 
                    chartType === 'location' ? prepareLocationData() : 
                    prepareTypeData();

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorDisplay error={error} />;

  return (
    <div className="charts">
      <h2>Charts & Analytics</h2>
      <div className="chart-controls">
        <div className="control-group">
          <label>Chart Type:</label>
          <select value={chartType} onChange={(e) => setChartType(e.target.value as 'time' | 'location' | 'type')}>
            <option value="time">Time Series</option>
            <option value="location">By Location</option>
            <option value="type">By Type</option>
          </select>
        </div>
        {chartType === 'time' && (
          <div className="control-group">
            <label>Time Range:</label>
            <select value={timeRange} onChange={(e) => setTimeRange(e.target.value as '1h' | '24h' | '7d')}>
              <option value="1h">Last Hour</option>
              <option value="24h">Last 24 Hours</option>
              <option value="7d">Last 7 Days</option>
            </select>
          </div>
        )}
      </div>
      
      <div className="chart-container">
        <h3>Energy Consumption</h3>
        <ResponsiveContainer width="100%" height={300}>
          {chartType === 'time' ? (
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="energy" stroke="#8884d8" name="Energy (kWh)" />
            </LineChart>
          ) : (
            <BarChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey={chartType === 'location' ? 'location' : 'type'} />
              <YAxis />
              <Tooltip />
              <Legend />
              <Bar dataKey="avgEnergy" fill="#8884d8" name="Avg Energy (kWh)" />
            </BarChart>
          )}
        </ResponsiveContainer>
      </div>

      <div className="chart-container">
        <h3>Air Quality (CO2 & PM2.5)</h3>
        <ResponsiveContainer width="100%" height={300}>
          {chartType === 'time' ? (
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="co2" stroke="#82ca9d" name="CO2 (ppm)" />
              <Line type="monotone" dataKey="pm25" stroke="#ffc658" name="PM2.5 (µg/m³)" />
            </LineChart>
          ) : (
            <BarChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey={chartType === 'location' ? 'location' : 'type'} />
              <YAxis />
              <Tooltip />
              <Legend />
              <Bar dataKey="avgCo2" fill="#82ca9d" name="Avg CO2 (ppm)" />
              <Bar dataKey="avgPm25" fill="#ffc658" name="Avg PM2.5 (µg/m³)" />
            </BarChart>
          )}
        </ResponsiveContainer>
      </div>

      <div className="chart-container">
        <h3>Humidity</h3>
        <ResponsiveContainer width="100%" height={300}>
          {chartType === 'time' ? (
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="humidity" stroke="#ff7c7c" name="Humidity (%)" />
            </LineChart>
          ) : (
            <BarChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey={chartType === 'location' ? 'location' : 'type'} />
              <YAxis />
              <Tooltip />
              <Legend />
              <Bar dataKey="avgHumidity" fill="#ff7c7c" name="Avg Humidity (%)" />
            </BarChart>
          )}
        </ResponsiveContainer>
      </div>
    </div>
  );
};

