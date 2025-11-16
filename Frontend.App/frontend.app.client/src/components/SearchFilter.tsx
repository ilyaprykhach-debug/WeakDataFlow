import { useState } from 'react';
import { useQuery } from '@apollo/client';
import { GET_SENSOR_READINGS } from '../graphql/queries';
import type { SensorReading } from '../types/graphql';
import { LoadingSpinner } from './LoadingSpinner';
import { ErrorDisplay } from './ErrorDisplay';
import { format } from 'date-fns';
import './SearchFilter.css';

export const SearchFilter = () => {
  const [location, setLocation] = useState<string>('');
  const [type, setType] = useState<string>('');
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');

  const { data, loading, error, refetch } = useQuery<{
    sensorReadingsWithPagination: SensorReading[];
  }>(GET_SENSOR_READINGS, {
    variables: {
      skip: 0,
      take: 100,
    },
    skip: false,
  });

  const readings = data?.sensorReadingsWithPagination || [];

  const filteredReadings = readings.filter((reading) => {
    if (searchTerm) {
      const search = searchTerm.toLowerCase();
      if (!(
        reading.location.toLowerCase().includes(search) ||
        reading.type.toLowerCase().includes(search) ||
        reading.sensorId.toLowerCase().includes(search)
      )) {
        return false;
      }
    }
    
    if (location && reading.location !== location) {
      return false;
    }
    
    if (type && reading.type !== type) {
      return false;
    }
    
    if (startDate) {
      const start = new Date(startDate);
      if (new Date(reading.timestamp) < start) {
        return false;
      }
    }
    
    if (endDate) {
      const end = new Date(endDate + 'T23:59:59');
      if (new Date(reading.timestamp) > end) {
        return false;
      }
    }
    
    return true;
  });

  const handleSearch = async () => {
    await refetch({
      skip: 0,
      take: 100,
    });
  };

  const handleReset = async () => {
    setLocation('');
    setType('');
    setStartDate('');
    setEndDate('');
    setSearchTerm('');
    await refetch({
      skip: 0,
      take: 100,
    });
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorDisplay error={error} onRetry={() => refetch()} />;

  return (
    <div className="search-filter">
      <h2>Search & Filter</h2>
      <div className="filter-controls">
        <div className="filter-row">
          <div className="filter-group">
            <label>Search:</label>
            <input
              type="text"
              placeholder="Search by location, type, or sensor ID..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>
        <div className="filter-row">
          <div className="filter-group">
            <label>Location:</label>
            <input
              type="text"
              placeholder="Filter by location"
              value={location}
              onChange={(e) => setLocation(e.target.value)}
            />
          </div>
          <div className="filter-group">
            <label>Type:</label>
            <input
              type="text"
              placeholder="Filter by type"
              value={type}
              onChange={(e) => setType(e.target.value)}
            />
          </div>
        </div>
        <div className="filter-row">
          <div className="filter-group">
            <label>Start Date:</label>
            <input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
            />
          </div>
          <div className="filter-group">
            <label>End Date:</label>
            <input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
            />
          </div>
        </div>
        <div className="filter-actions">
          <button onClick={handleSearch} className="search-button">
            Search
          </button>
          <button onClick={handleReset} className="reset-button">
            Reset
          </button>
        </div>
      </div>

      <div className="results-info">
        Found {filteredReadings.length} reading{filteredReadings.length !== 1 ? 's' : ''}
      </div>

      <div className="filtered-readings">
        {filteredReadings.map((reading) => (
          <div key={reading.id} className="reading-row">
            <div className="reading-info">
              <div className="reading-meta">
                <span className="reading-id">{reading.sensorId}</span>
                <span className="reading-location-badge">{reading.location}</span>
                <span className="reading-type-badge">{reading.type}</span>
              </div>
              <div className="reading-time">
                {format(new Date(reading.timestamp), 'MMM dd, yyyy HH:mm:ss')}
              </div>
            </div>
            <div className="reading-measurements">
              {reading.energyConsumption !== null && reading.energyConsumption !== undefined && (
                <span>Energy: {reading.energyConsumption.toFixed(2)} kWh</span>
              )}
              {reading.co2 !== null && reading.co2 !== undefined && (
                <span>CO2: {reading.co2} ppm</span>
              )}
              {reading.pm25 !== null && reading.pm25 !== undefined && (
                <span>PM2.5: {reading.pm25} µg/m³</span>
              )}
              {reading.humidity !== null && reading.humidity !== undefined && (
                <span>Humidity: {reading.humidity}%</span>
              )}
            </div>
          </div>
        ))}
        {filteredReadings.length === 0 && (
          <div className="no-results">No readings match your search criteria</div>
        )}
      </div>
    </div>
  );
};

