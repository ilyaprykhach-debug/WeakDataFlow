import { useQuery } from '@apollo/client';
import { GET_LATEST_SENSOR_READINGS } from '../graphql/queries';
import type { SensorReading } from '../types/graphql';
import { LoadingSpinner } from './LoadingSpinner';
import { ErrorDisplay } from './ErrorDisplay';
import { format } from 'date-fns';
import './LatestValues.css';

export const LatestValues = () => {
  const { data, loading, error, refetch } = useQuery<{
    sensorReadingsWithPagination: SensorReading[];
  }>(GET_LATEST_SENSOR_READINGS, {
    variables: { take: 10 },
    pollInterval: 10000,
  });

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorDisplay error={error} onRetry={() => refetch()} />;

  const readings = data?.sensorReadingsWithPagination || [];

  return (
    <div className="latest-values">
      <h2>Latest Sensor Readings</h2>
      <div className="readings-grid">
        {readings.map((reading) => (
          <div key={reading.id} className="reading-card">
            <div className="reading-header">
              <span className="reading-location">{reading.location}</span>
              <span className="reading-type">{reading.type}</span>
            </div>
            <div className="reading-time">
              {format(new Date(reading.timestamp), 'MMM dd, yyyy HH:mm:ss')}
            </div>
            <div className="reading-values">
              {reading.energyConsumption !== null && reading.energyConsumption !== undefined && (
                <div className="value-item">
                  <span className="value-label">Energy:</span>
                  <span className="value-number">{reading.energyConsumption.toFixed(2)} kWh</span>
                </div>
              )}
              {reading.co2 !== null && reading.co2 !== undefined && (
                <div className="value-item">
                  <span className="value-label">CO2:</span>
                  <span className="value-number">{reading.co2} ppm</span>
                </div>
              )}
              {reading.pm25 !== null && reading.pm25 !== undefined && (
                <div className="value-item">
                  <span className="value-label">PM2.5:</span>
                  <span className="value-number">{reading.pm25} µg/m³</span>
                </div>
              )}
              {reading.humidity !== null && reading.humidity !== undefined && (
                <div className="value-item">
                  <span className="value-label">Humidity:</span>
                  <span className="value-number">{reading.humidity}%</span>
                </div>
              )}
              {reading.motionDetected !== null && reading.motionDetected !== undefined && (
                <div className="value-item">
                  <span className="value-label">Motion:</span>
                  <span className={`value-badge ${reading.motionDetected ? 'motion-yes' : 'motion-no'}`}>
                    {reading.motionDetected ? 'Detected' : 'None'}
                  </span>
                </div>
              )}
            </div>
          </div>
        ))}
        {readings.length === 0 && (
          <div className="no-data">No sensor readings available</div>
        )}
      </div>
    </div>
  );
};

