export interface JobComparisonSeriesPoint {
  x: number;
  y: number | null;
}

export interface JobComparisonSeries {
  jobId: string;
  label: string;
  color: string;
  data: JobComparisonSeriesPoint[];
}
