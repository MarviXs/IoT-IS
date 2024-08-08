interface SensorNode {
  id: string;
  name: string;
  items: SensorNode[];
  type: 'Device' | 'SubCollection' | 'Sensor';
  sensor?: Sensor;
}
interface Sensor {
  id?: string;
  tag?: string;
  name?: string | null;
  unit?: string | null;
  accuracyDecimals?: number | null;
}

export type { SensorNode };
