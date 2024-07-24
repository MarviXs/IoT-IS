import { client } from '@/api/client';
import { LoginRequest, RegisterRequest } from '@/api/types/Auth';

class AuthService {
  async login(body: LoginRequest) {
    return await client.POST('/auth/login', { body });
  }

  async register(body: RegisterRequest) {
    return await client.POST('/auth/register', { body });
  }
}

export default new AuthService();
