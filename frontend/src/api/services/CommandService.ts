import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type CommandsQueryParams = paths['/commands']['get']['parameters']['query'];
export type CommandsResponse = paths['/commands']['get']['responses']['200']['content']['application/json'];
export type CommandResponse = paths['/commands/{id}']['get']['responses']['200']['content']['application/json'];
export type CreateCommandRequest = paths['/commands']['post']['requestBody']['content']['application/json'];
export type UpdateCommandRequest = paths['/commands/{id}']['put']['requestBody']['content']['application/json'];

class CommandService {
  async getCommands(queryParams: CommandsQueryParams) {
    return await client.GET('/commands', { params: { query: queryParams } });
  }

  async getCommand(id: string) {
    return await client.GET('/commands/{id}', { params: { path: { id: id } } });
  }

  async createCommand(createCommandRequest: CreateCommandRequest) {
    return await client.POST('/commands', { body: createCommandRequest });
  }

  async updateCommand(id: string, updateCommandRequest: UpdateCommandRequest) {
    return await client.PUT('/commands/{id}', { body: updateCommandRequest, params: { path: { id: id } } });
  }

  async deleteCommand(id: string) {
    return await client.DELETE('/commands/{id}', { params: { path: { id: id } } });
  }
}

export default new CommandService();
