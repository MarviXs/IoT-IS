import { client } from '@/api/client';
import { CommandsQueryParams, CreateCommandRequest, UpdateCommandRequest } from '../types/Command';

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
