import { client } from '@/api/client';
import { UsersQueryParams } from '../types/UserManagementTypes';
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
