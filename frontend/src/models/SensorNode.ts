interface Sensor {
  id?: string;
  tag?: string;
  name?: string;
  unit?: string | null;
  accuracyDecimals?: number | null;
}

interface SensorNode {
  id?: string;
  name?: string;
  children?: Array<SensorNode>;
  sensor?: Sensor;
}

export type { SensorNode };
