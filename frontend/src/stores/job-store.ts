import { defineStore } from 'pinia';
import { reactive } from 'vue';
import { useAsyncData } from '@/composables/useAsyncData';
import { useI18n } from 'vue-i18n';
import { Job } from '@/models/Job';
import DeviceService from '@/services/DeviceService';

export const useJobStore = defineStore('job', () => {
  const { t } = useI18n();

  const jobs = reactive(useAsyncData(getDeviceJobs, t('job.toasts.load_failed'), false));

  async function getDeviceJobs(): Promise<Job[]> {
    const devices = await DeviceService.getDevices();
    return devices.flatMap((device) =>
      device.jobs.map((job) => ({
        ...job,
        deviceName: device.name,
        deviceId: device.uid,
      })),
    );
  }

  function getJobById(id: string): Job | undefined {
    return jobs.data?.find((job) => job.uid === id);
  }

  return {
    jobs,
    getJobById,
  };
});
