export interface SensorReading {
  id: string;
  sensorId: string;
  type: string;
  location: string;
  timestamp: string;
  energyConsumption?: number | null;
  co2?: number | null;
  pm25?: number | null;
  humidity?: number | null;
  motionDetected?: boolean | null;
}

export interface AggregationResult {
  groupBy: string;
  count: number;
  averageEnergyConsumption?: number | null;
  averageCo2?: number | null;
  averagePm25?: number | null;
  averageHumidity?: number | null;
  totalEnergyConsumption?: number | null;
}

export interface NotificationEvent {
  eventType: string;
  serviceName: string;
  timestamp: string;
  data?: unknown;
  message?: string | null;
}

