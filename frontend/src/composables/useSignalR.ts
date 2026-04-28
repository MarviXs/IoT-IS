import type { HubConnection } from '@microsoft/signalr';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { ref, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth-store.js';
import { FailedToNegotiateWithServerError } from '@microsoft/signalr/dist/esm/Errors';
import { resubscribeToDeviceDataPoints } from '@/utils/signalrDataPoints';

const baseUrl = (process.env.VITE_API_URL || 'http://localhost:5097/') + 'is-hub';
let connection: HubConnection | null = null;
const isConnected = ref(false);
const hasReconnectGap = ref(false);
const reconnectVersion = ref(0);

export function useSignalR() {
  const authStore = useAuthStore();

  const getConnection = () => {
    if (!connection) {
      connection = new HubConnectionBuilder()
        .withUrl(baseUrl, {
          accessTokenFactory: () => authStore.accessToken,
          withCredentials: false,
        })
        .withAutomaticReconnect()
        .build();

      connection.onreconnecting(() => {
        isConnected.value = false;
        hasReconnectGap.value = true;
      });

      connection.onreconnected(() => {
        isConnected.value = true;
        reconnectVersion.value += 1;
      });

      connection.onclose(() => {
        isConnected.value = false;
        hasReconnectGap.value = true;
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
        await resubscribeToDeviceDataPoints(conn);
        console.log('Connected to SignalR hub');
      } catch (err: any) {
        if (err instanceof FailedToNegotiateWithServerError) {
          console.log('Unauthorized. Attempting to refresh token...');
          try {
            await authStore.refreshAccessToken();
            await conn.start();
            isConnected.value = true;
            await resubscribeToDeviceDataPoints(conn);
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
    hasReconnectGap,
    reconnectVersion,
    connect,
    disconnect,
  };
}
