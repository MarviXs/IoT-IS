import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type TimescaleStorageResponse = paths['/system/storage']['get']['responses']['200']['content']['application/json'];
export type TimescaleChunksResponse = paths['/system/storage/chunks']['get']['responses']['200']['content']['application/json'];
export type DropTimescaleDataPointChunksRequest =
  paths['/system/storage/chunks/drop']['post']['requestBody']['content']['application/json'];
export type DropTimescaleDataPointChunksResponse =
  paths['/system/storage/chunks/drop']['post']['responses']['200']['content']['application/json'];
export type DeleteTimescaleDataPointChunkResponse =
  paths['/system/storage/chunks/{chunkSchema}/{chunkName}']['delete']['responses']['200']['content']['application/json'];
export type SyncAllEdgeNodesNowResponse =
  paths['/system/edge-nodes/sync-all-now']['post']['responses']['200']['content']['application/json'];
export type NodeSettingsResponse = paths['/system/node-settings']['get']['responses']['200']['content']['application/json'];
export type UpdateNodeSettingsRequest = paths['/system/node-settings']['put']['requestBody']['content']['application/json'];
export type CreateEdgeNodeRequest = paths['/system/edge-nodes']['post']['requestBody']['content']['application/json'];
export type CreateEdgeNodeResponse = paths['/system/edge-nodes']['post']['responses']['201']['content']['application/json'];
export type UpdateEdgeNodeRequest = paths['/system/edge-nodes/{id}']['put']['requestBody']['content']['application/json'];
export type EdgeNodeResponse = NodeSettingsResponse['edgeNodes'][number];
export type HubConnectionStatusResponse = NodeSettingsResponse['hubConnectionStatus'];
export type SystemNodeType = NodeSettingsResponse['nodeType'];
export type EdgeDataPointSyncMode = NodeSettingsResponse['dataPointSyncMode'];

class SystemService {
  async getTimescaleStorageUsage(): Promise<TimescaleStorageResponse> {
    const { data, error } = await client.GET('/system/storage');
    if (error || !data) {
      throw new Error('Failed to load TimescaleDB storage usage');
    }

    return data;
  }

  async getTimescaleDataPointChunks(): Promise<TimescaleChunksResponse> {
    const { data, error } = await client.GET('/system/storage/chunks');
    if (error || !data) {
      throw new Error('Failed to load TimescaleDB datapoint chunks');
    }

    return data;
  }

  async forceReclaimTimescaleSpace(): Promise<void> {
    const { error } = await client.POST('/system/storage/vacuum');
    if (error) {
      throw new Error('Failed to run VACUUM on TimescaleDB datapoints table');
    }
  }

  async dropTimescaleDataPointChunks(request: DropTimescaleDataPointChunksRequest): Promise<DropTimescaleDataPointChunksResponse> {
    const { data, error } = await client.POST('/system/storage/chunks/drop', { body: request });
    if (error || !data) {
      throw new Error('Failed to drop TimescaleDB datapoint chunks');
    }

    return data;
  }

  async deleteTimescaleDataPointChunk(chunkSchema: string, chunkName: string): Promise<DeleteTimescaleDataPointChunkResponse> {
    const { data, error } = await client.DELETE('/system/storage/chunks/{chunkSchema}/{chunkName}', {
      params: {
        path: {
          chunkSchema,
          chunkName,
        },
      },
    });
    if (error || !data) {
      throw new Error('Failed to delete TimescaleDB datapoint chunk');
    }

    return data;
  }

  async getNodeSettings(): Promise<NodeSettingsResponse> {
    const { data, error } = await client.GET('/system/node-settings');
    if (error || !data) {
      throw new Error('Failed to load node settings');
    }

    return data;
  }

  async updateNodeSettings(request: UpdateNodeSettingsRequest): Promise<void> {
    const { error } = await client.PUT('/system/node-settings', { body: request });
    if (error) {
      throw new Error('Failed to update node settings');
    }
  }

  async createEdgeNode(request: CreateEdgeNodeRequest): Promise<CreateEdgeNodeResponse> {
    const { data, error } = await client.POST('/system/edge-nodes', { body: request });
    if (error || !data) {
      throw new Error('Failed to create edge node');
    }

    return data;
  }

  async updateEdgeNode(id: string, request: UpdateEdgeNodeRequest): Promise<void> {
    const { error } = await client.PUT('/system/edge-nodes/{id}', { params: { path: { id } }, body: request });
    if (error) {
      throw new Error('Failed to update edge node');
    }
  }

  async deleteEdgeNode(id: string): Promise<void> {
    const { error } = await client.DELETE('/system/edge-nodes/{id}', { params: { path: { id } } });
    if (error) {
      throw new Error('Failed to delete edge node');
    }
  }

  async syncEdgeNodeNow(id: string): Promise<void> {
    const { error } = await client.POST('/system/edge-nodes/{id}/sync-now', { params: { path: { id } } });
    if (error) {
      throw new Error('Failed to queue sync request for edge node');
    }
  }

  async syncAllEdgeNodesNow(): Promise<SyncAllEdgeNodesNowResponse> {
    const { data, error } = await client.POST('/system/edge-nodes/sync-all-now');
    if (error || !data) {
      throw new Error('Failed to queue sync request for all edge nodes');
    }

    return data;
  }
}

export default new SystemService();
