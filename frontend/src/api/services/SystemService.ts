import { baseUrl, customFetch } from '@/api/client';

export interface TimescaleStorageResponse {
  totalColumnstoreSizeBytes: number;
}

function buildUrl(path: string): string {
  const normalizedBase = baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`;
  return `${normalizedBase}${path}`;
}

class SystemService {
  async getTimescaleStorageUsage(): Promise<TimescaleStorageResponse> {
    const response = await customFetch(buildUrl('system/storage'), { method: 'GET' });

    if (!response.ok) {
      throw new Error('Failed to load TimescaleDB storage usage');
    }

    if (response.status === 204) {
      return { totalColumnstoreSizeBytes: 0 };
    }

    return (await response.json()) as TimescaleStorageResponse;
  }

  async forceReclaimTimescaleSpace(): Promise<void> {
    const response = await customFetch(buildUrl('system/storage/vacuum'), { method: 'POST' });

    if (!response.ok) {
      throw new Error('Failed to run VACUUM on TimescaleDB datapoints table');
    }
  }
}

export default new SystemService();
