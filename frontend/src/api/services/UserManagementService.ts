import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type UsersQueryParams = paths['/admin/users']['get']['parameters']['query'];
export type UsersResponse = paths['/admin/users']['get']['responses']['200']['content']['application/json'];
export type GetUserByIdResponse = paths['/admin/users/{id}']['get']['responses']['200']['content']['application/json'];

class UserManagementService {
  async getUsers(queryParams: UsersQueryParams) {
    return await client.GET('/admin/users', { params: { query: queryParams } });
  }

  async getUserById(id: string) {
    return await client.GET('/admin/users/{id}', { params: { path: { id } } });
  }

  async updateUserRole(id: string, role: 'Admin' | 'User') {
    return await client.PUT('/admin/users/{id}/role', { params: { path: { id } }, body: { role: role } });
  }

  async updateUserEmail(id: string, email: string) {
    return await client.PUT('/admin/users/{id}/email', { params: { path: { id } }, body: { email: email } });
  }

  async updateUserPassword(id: string, password: string) {
    return await client.PUT('/admin/users/{id}/password', { params: { path: { id } }, body: { password: password } });
  }
}

export default new UserManagementService();
