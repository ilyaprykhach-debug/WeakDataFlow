import { gql } from '@apollo/client';

export const GET_LATEST_SENSOR_READINGS = gql`
  query GetLatestSensorReadings($take: Int) {
    sensorReadingsWithPagination(pagination: { take: $take, skip: 0 }, order: [{ timestamp: DESC }]) {
      id
      sensorId
      type
      location
      timestamp
      energyConsumption
      co2
      pm25
      humidity
      motionDetected
    }
  }
`;

export const GET_SENSOR_READINGS = gql`
  query GetSensorReadings($skip: Int, $take: Int) {
    sensorReadingsWithPagination(
      pagination: { skip: $skip, take: $take }
      order: [{ timestamp: DESC }]
    ) {
      id
      sensorId
      type
      location
      timestamp
      energyConsumption
      co2
      pm25
      humidity
      motionDetected
    }
  }
`;

export const GET_AGGREGATIONS_BY_LOCATION = gql`
  query GetAggregationsByLocation($startTime: DateTime, $endTime: DateTime) {
    aggregationsByLocation(startTime: $startTime, endTime: $endTime) {
      groupBy
      count
      averageEnergyConsumption
      averageCo2
      averagePm25
      averageHumidity
      totalEnergyConsumption
    }
  }
`;

export const GET_AGGREGATIONS_BY_TYPE = gql`
  query GetAggregationsByType($startTime: DateTime, $endTime: DateTime) {
    aggregationsByType(startTime: $startTime, endTime: $endTime) {
      groupBy
      count
      averageEnergyConsumption
      averageCo2
      averagePm25
      averageHumidity
      totalEnergyConsumption
    }
  }
`;

export const GET_AGGREGATIONS_BY_TIME_PERIOD = gql`
  query GetAggregationsByTimePeriod($period: String!, $hoursBack: Int, $daysBack: Int) {
    aggregationsByTimePeriod(period: $period, hoursBack: $hoursBack, daysBack: $daysBack) {
      groupBy
      count
      averageEnergyConsumption
      averageCo2
      averagePm25
      averageHumidity
      totalEnergyConsumption
    }
  }
`;

