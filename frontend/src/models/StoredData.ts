interface StoredData {
  uid: string;
  dataPointTagId: string;
  value: number;
  measureAt: number;
  measureAtDevice: string;
  deactivated: boolean;
  deviceId: string;
  tag: string;
}

export type { StoredData };
