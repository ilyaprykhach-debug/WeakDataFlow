import { useQuery } from '@apollo/client';
import { GET_AGGREGATIONS_BY_LOCATION, GET_AGGREGATIONS_BY_TYPE } from '../graphql/queries';
import type { AggregationResult } from '../types/graphql';
import { LoadingSpinner } from './LoadingSpinner';
import { ErrorDisplay } from './ErrorDisplay';
import { useState } from 'react';
import { format, subDays } from 'date-fns';
import './Aggregations.css';

export const Aggregations = () => {
  const [aggregationType, setAggregationType] = useState<'location' | 'type'>('location');
  const [startTime, setStartTime] = useState<string>(format(subDays(new Date(), 7), 'yyyy-MM-dd'));
  const [endTime, setEndTime] = useState<string>(format(new Date(), 'yyyy-MM-dd'));

  const { data: locationData, loading: locationLoading, error: locationError } = useQuery<{
    aggregationsByLocation: AggregationResult[];
  }>(GET_AGGREGATIONS_BY_LOCATION, {
    variables: {
      startTime: startTime ? new Date(startTime).toISOString() : null,
      endTime: endTime ? new Date(endTime + 'T23:59:59').toISOString() : null,
    },
    skip: aggregationType !== 'location',
  });

  const { data: typeData, loading: typeLoading, error: typeError } = useQuery<{
    aggregationsByType: AggregationResult[];
  }>(GET_AGGREGATIONS_BY_TYPE, {
    variables: {
      startTime: startTime ? new Date(startTime).toISOString() : null,
      endTime: endTime ? new Date(endTime + 'T23:59:59').toISOString() : null,
    },
    skip: aggregationType !== 'type',
  });

  const loading = aggregationType === 'location' ? locationLoading : typeLoading;
  const error = aggregationType === 'location' ? locationError : typeError;
  const aggregations = (aggregationType === 'location' ? locationData?.aggregationsByLocation : typeData?.aggregationsByType) || [];

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorDisplay error={error} />;

  return (
    <div className="aggregations">
      <h2>Aggregations</h2>
      <div className="aggregation-controls">
        <div className="control-group">
          <label>Group By:</label>
          <select value={aggregationType} onChange={(e) => setAggregationType(e.target.value as 'location' | 'type')}>
            <option value="location">Location</option>
            <option value="type">Type</option>
          </select>
        </div>
        <div className="control-group">
          <label>Start Date:</label>
          <input
            type="date"
            value={startTime}
            onChange={(e) => setStartTime(e.target.value)}
          />
        </div>
        <div className="control-group">
          <label>End Date:</label>
          <input
            type="date"
            value={endTime}
            onChange={(e) => setEndTime(e.target.value)}
          />
        </div>
      </div>

      <div className="aggregations-grid">
        {aggregations.map((agg) => (
          <div key={agg.groupBy} className="aggregation-card">
            <h3 className="aggregation-group">{agg.groupBy}</h3>
            <div className="aggregation-stats">
              <div className="stat-item">
                <span className="stat-label">Count:</span>
                <span className="stat-value">{agg.count}</span>
              </div>
              {agg.averageEnergyConsumption !== null && agg.averageEnergyConsumption !== undefined && (
                <div className="stat-item">
                  <span className="stat-label">Avg Energy:</span>
                  <span className="stat-value">{agg.averageEnergyConsumption.toFixed(2)} kWh</span>
                </div>
              )}
              {agg.totalEnergyConsumption !== null && agg.totalEnergyConsumption !== undefined && (
                <div className="stat-item">
                  <span className="stat-label">Total Energy:</span>
                  <span className="stat-value">{agg.totalEnergyConsumption.toFixed(2)} kWh</span>
                </div>
              )}
              {agg.averageCo2 !== null && agg.averageCo2 !== undefined && (
                <div className="stat-item">
                  <span className="stat-label">Avg CO2:</span>
                  <span className="stat-value">{agg.averageCo2.toFixed(2)} ppm</span>
                </div>
              )}
              {agg.averagePm25 !== null && agg.averagePm25 !== undefined && (
                <div className="stat-item">
                  <span className="stat-label">Avg PM2.5:</span>
                  <span className="stat-value">{agg.averagePm25.toFixed(2)} µg/m³</span>
                </div>
              )}
              {agg.averageHumidity !== null && agg.averageHumidity !== undefined && (
                <div className="stat-item">
                  <span className="stat-label">Avg Humidity:</span>
                  <span className="stat-value">{agg.averageHumidity.toFixed(2)}%</span>
                </div>
              )}
            </div>
          </div>
        ))}
        {aggregations.length === 0 && (
          <div className="no-data">No aggregation data available for the selected period</div>
        )}
      </div>
    </div>
  );
};

