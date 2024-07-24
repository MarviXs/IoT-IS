import { defineStore } from 'pinia';
import { Device } from 'src/models/Device';
import deviceService from 'src/services/DeviceService';
import { useI18n } from 'vue-i18n';
import { useAsyncData } from '@/composables/useAsyncData';
import { reactive } from 'vue';

export const useDeviceStore = defineStore('device', () => {
  const { t } = useI18n();

  //Devices
  async function getAllDevicesCombined(): Promise<Device[]> {
    const [ownedDevices] = await Promise.all([
      deviceService.getDevices(),
      // deviceService.getSharedDevices(),
    ]);
    return [...ownedDevices];
  }
  const devices = reactive(useAsyncData(getAllDevicesCombined, t('device.toasts.loading_failed', 2), false));

  function getDeviceById(id: string): Device | null {
    return devices.data?.find((device) => device.uid === id) ?? null;
  }

  return {
    devices,
    getDeviceById,
  };
});
