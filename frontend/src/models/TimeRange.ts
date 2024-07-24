interface TimeRange {
  from: string;
  to: string;
}

interface PredefinedTimeRange {
  title: string;
  name: string;
  time: number;
}

export type { TimeRange, PredefinedTimeRange };
