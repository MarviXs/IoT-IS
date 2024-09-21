import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { ref, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth-store.js';
import { FailedToNegotiateWithServerError } from '@microsoft/signalr/dist/esm/Errors';

const baseUrl = (process.env.VITE_API_URL || 'http://localhost:5097/') + 'is-hub';
let connection: HubConnection | null = null;

export function useSignalR() {
  const isConnected = ref(false);
  const authStore = useAuthStore();

  const getConnection = () => {
    if (!connection) {
      connection = new HubConnectionBuilder()
        .withUrl(baseUrl, {
          accessTokenFactory: () => authStore.accessToken,
        })
        .withAutomaticReconnect()
        .build();

      connection.onclose(() => {
        isConnected.value = false;
      });
    }
    return connection;
  };

  const connect = async () => {
    const conn = getConnection();
    if (conn.state === 'Disconnected') {
      try {
        await conn.start();
        isConnected.value = true;
        console.log('Connected to SignalR hub');
      } catch (err: any) {
        if (err instanceof FailedToNegotiateWithServerError) {
          console.log('Unauthorized. Attempting to refresh token...');
          try {
            await authStore.refreshAccessToken();
            await conn.start();
            isConnected.value = true;
            console.log('Connected to SignalR hub after token refresh');
          } catch (refreshErr) {
            console.error('Failed to refresh token and reconnect:', refreshErr);
            authStore.logout();
          }
        } else {
          console.error('Failed to connect to SignalR hub:', err);
        }
      }
    }
  };

  const disconnect = async () => {
    if (connection && connection.state === 'Connected') {
      try {
        await connection.stop();
        isConnected.value = false;
      } catch (err) {
        console.error('Failed to disconnect from SignalR hub:', err);
      }
    }
  };

  onMounted(() => {
    connect();
  });

  return {
    connection: getConnection(),
    isConnected,
    connect,
    disconnect,
  };
}
