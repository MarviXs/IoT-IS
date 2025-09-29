interface LastDataPoint {
  deviceId: string;
  tag: string;
  value?: number | null;
  ts?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  gridX?: number | null;
  gridY?: number | null;
}

export type { LastDataPoint };
