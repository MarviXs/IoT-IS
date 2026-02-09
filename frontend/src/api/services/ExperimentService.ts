import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type ExperimentsQueryParams = paths['/experiments']['get']['parameters']['query'];
export type ExperimentsResponse = paths['/experiments']['get']['responses']['200']['content']['application/json'];
export type ExperimentResponse = paths['/experiments/{id}']['get']['responses']['200']['content']['application/json'];
export type CreateExperimentRequest = paths['/experiments']['post']['requestBody']['content']['application/json'];
export type UpdateExperimentRequest = paths['/experiments/{id}']['put']['requestBody']['content']['application/json'];

class ExperimentService {
  async getExperiments(queryParams: ExperimentsQueryParams) {
    return await client.GET('/experiments', { params: { query: queryParams } });
  }

  async getExperiment(id: string) {
    return await client.GET('/experiments/{id}', { params: { path: { id } } });
  }

  async createExperiment(body: CreateExperimentRequest) {
    return await client.POST('/experiments', { body });
  }

  async updateExperiment(id: string, body: UpdateExperimentRequest) {
    return await client.PUT('/experiments/{id}', { params: { path: { id } }, body });
  }

  async deleteExperiment(id: string) {
    return await client.DELETE('/experiments/{id}', { params: { path: { id } } });
  }
}

export default new ExperimentService();
