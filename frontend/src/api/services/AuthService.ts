import { client } from '@/api/client';
import { LoginRequest, RegisterRequest } from '@/api/types/Auth';

class AuthService {
  async login(body: LoginRequest) {
    return await client.POST('/auth/login', { body });
  }

  async refreshToken(refreshToken: string) {
    return await client.POST('/auth/refresh', { body: { refreshToken: refreshToken } });
  }

  async register(body: RegisterRequest) {
    return await client.POST('/auth/register', { body });
  }

  async updatePassword(oldPassword: string, newPassword: string) {
    return await client.PUT('/auth/password', { body: { oldPassword, newPassword } });
  }

  async updateEmail(email: string) {
    return await client.PUT('/auth/email', { body: { newEmail: email } });
  }
}

export default new AuthService();
