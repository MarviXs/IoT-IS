import { JobStatusEnum } from '@/models/JobStatusEnum';
import {
  mdiAlertCircleOutline,
  mdiCheckBold,
  mdiCheckboxBlankCircleOutline,
  mdiClockTimeThreeOutline,
  mdiCloseCircleOutline,
  mdiCogSyncOutline,
  mdiPauseCircleOutline,
} from '@quasar/extras/mdi-v7';

const jobStatusIcon: Record<JobStatusEnum, string> = {
  [JobStatusEnum.JOB_QUEUED]: mdiCheckboxBlankCircleOutline,
  [JobStatusEnum.JOB_IN_PROGRESS]: mdiCogSyncOutline,
  [JobStatusEnum.JOB_SUCCEEDED]: mdiCheckBold,
  [JobStatusEnum.JOB_REJECTED]: mdiAlertCircleOutline,
  [JobStatusEnum.JOB_FAILED]: mdiAlertCircleOutline,
  [JobStatusEnum.JOB_PAUSED]: mdiPauseCircleOutline,
  [JobStatusEnum.JOB_CANCELED]: mdiCloseCircleOutline,
  [JobStatusEnum.JOB_TIMED_OUT]: mdiClockTimeThreeOutline,
};

const jobStatusColors: Record<JobStatusEnum, string> = {
  [JobStatusEnum.JOB_QUEUED]: 'accent',
  [JobStatusEnum.JOB_IN_PROGRESS]: 'primary',
  [JobStatusEnum.JOB_SUCCEEDED]: 'green-8',
  [JobStatusEnum.JOB_REJECTED]: 'deep-orange-8',
  [JobStatusEnum.JOB_FAILED]: 'red-14',
  [JobStatusEnum.JOB_PAUSED]: 'amber-9',
  [JobStatusEnum.JOB_CANCELED]: 'grey-7',
  [JobStatusEnum.JOB_TIMED_OUT]: 'orange-8',
};

export { jobStatusIcon, jobStatusColors };
