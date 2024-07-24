import createClient, { Middleware } from 'openapi-fetch';
import type { paths } from './generated/schema.d.ts';
import { useAuthStore } from '@/stores/auth-store.js';
import { toast } from 'vue3-toastify';

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    const authStore = useAuthStore();

    if (authStore.accessToken) {
      request.headers.set('Authorization', `Bearer ${authStore.accessToken}`);
    }

    return request;
  },
  async onResponse({ request, response, options }) {
    if (response.status === 204 || response.headers.get('content-length') === '0') {
      return response;
    }

    const clonedResponse = response.clone();
    let data;

    try {
      data = await clonedResponse.json();
    } catch (error) {
      console.error('Error parsing response:', error);
      return response;
    }

    if (data.status === 403 && data.body?.detail === 'Invalid refresh token') {
      const authStore = useAuthStore();
      authStore.logout();
    }
  },
};

const client = createClient<paths>({
  baseUrl: process.env.API_URL || '/api',
  fetch: async (input: Request) => {
    // Read the body of the request
    const originalBody = input.body ? await input.text() : null;
    const originalHeaders = new Headers(input.headers);
    const originalRequest = new Request(input, {
      body: originalBody,
      headers: originalHeaders,
    });

    const fetchWithRetry = async (request: Request): Promise<Response> => {
      let response = await fetch(request);

      if (response.status === 401 && request.url.includes('/auth/refresh') === false) {
        const authStore = useAuthStore();

        if (authStore.refreshToken) {
          const res = await client.POST('/auth/refresh', {
            body: { refreshToken: authStore.refreshToken },
          });

          if (res.data && res.data.accessToken) {
            authStore.accessToken = res.data.accessToken;
            const retriedHeaders = new Headers(request.headers);
            retriedHeaders.set('Authorization', `Bearer ${authStore.accessToken}`);
            const retriedRequest = new Request(request, {
              headers: retriedHeaders,
              body: originalBody,
            });
            response = await fetch(retriedRequest);
          }
        }
      }
      return response;
    };

    try {
      return await fetchWithRetry(originalRequest);
    } catch (error) {
      toast.error('An error occurred. Please try again later.');
      throw error;
    }
  },
});

client.use(authMiddleware);

export { client };
