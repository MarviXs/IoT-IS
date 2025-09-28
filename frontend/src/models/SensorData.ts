export interface SensorData {
  id: string;
  deviceId: string;
  tag: string;
  name: string;
  unit?: string | null;
  accuracyDecimals?: number | null;
  lastValue?: number | null;
  group?: string;
}
