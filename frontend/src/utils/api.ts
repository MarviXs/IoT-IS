import { FetchContext, ofetch } from 'ofetch';
import { useAuthStore } from '@/stores/auth-store';
import AuthService from '@/services/AuthService';

async function onResponseError(context: FetchContext) {
  const { response } = context;
  const authStore = useAuthStore();

  if (response && response.status === 401) {
    if (authStore.refreshToken) {
      const res = await AuthService.refreshToken(authStore.refreshToken);

      if (res && res.accessToken) {
        console.log('refreshed token');
        authStore.accessToken = res.accessToken;
        return;
      }
    } else {
      authStore.logout();
      return;
    }
  }

  if (response && response.status === 403) {
    const { _data } = response;
    if (_data && _data.detail === 'Invalid refresh token') {
      authStore.logout();
      return;
    }
  }
}

function onRequest(context: FetchContext) {
  const { options } = context;
  const authStore = useAuthStore();

  if (authStore.accessToken) {
    options.headers = {
      ...options.headers,
      Authorization: `Bearer ${authStore.accessToken}`,
    };
  }
}

const api = ofetch.create({
  baseURL: process.env.API_URL || '/api',
  onResponseError,
  onRequest,
  retryStatusCodes: [401],
  retry: 1,
});

export { api };
