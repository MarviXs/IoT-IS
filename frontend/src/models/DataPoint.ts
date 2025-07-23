interface DataPoint {
  ts: string;
  value?: number | null;
  latitude?: number | null;
  longitude?: number | null;
}

export type { DataPoint };
