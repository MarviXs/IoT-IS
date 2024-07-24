import { JobStatusEnum } from '@/models/JobStatusEnum';
import {
  mdiAlertCircleOutline,
  mdiCheckBold,
  mdiCheckboxBlankCircleOutline,
  mdiChevronTripleUp,
  mdiClockTimeFourOutline,
  mdiCloseCircleOutline,
  mdiCogSyncOutline,
  mdiPauseCircleOutline,
  mdiTimerSand,
} from '@quasar/extras/mdi-v6';

const jobStatusIcon: Record<JobStatusEnum, string> = {
  [JobStatusEnum.JOB_FREE]: mdiCheckboxBlankCircleOutline,
  [JobStatusEnum.JOB_IDLE]: mdiClockTimeFourOutline,
  [JobStatusEnum.JOB_PENDING]: mdiTimerSand,
  [JobStatusEnum.JOB_PROCESSING]: mdiCogSyncOutline,
  [JobStatusEnum.JOB_DONE]: mdiCheckBold,
  [JobStatusEnum.JOB_ERR]: mdiAlertCircleOutline,
  [JobStatusEnum.JOB_PAUSED]: mdiPauseCircleOutline,
  [JobStatusEnum.JOB_CANCELED]: mdiCloseCircleOutline,
  [JobStatusEnum.JOB_STATUS_MAX]: mdiChevronTripleUp,
};

const jobStatusColors: Record<JobStatusEnum, string> = {
  [JobStatusEnum.JOB_FREE]: 'accent',
  [JobStatusEnum.JOB_IDLE]: 'accent',
  [JobStatusEnum.JOB_PENDING]: 'accent',
  [JobStatusEnum.JOB_PROCESSING]: 'primary',
  [JobStatusEnum.JOB_DONE]: 'green-14',
  [JobStatusEnum.JOB_ERR]: 'red-14',
  [JobStatusEnum.JOB_PAUSED]: 'accent',
  [JobStatusEnum.JOB_CANCELED]: 'red-14',
  [JobStatusEnum.JOB_STATUS_MAX]: 'primary',
};

export { jobStatusIcon, jobStatusColors };
