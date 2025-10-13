import type { components, paths } from '@/api/generated/schema.d.ts';

export type JobScheduleType = components['schemas']['Fei.Is.Api.Data.Enums.JobScheduleTypeEnum'];
export type JobScheduleInterval = components['schemas']['Fei.Is.Api.Data.Enums.JobScheduleIntervalEnum'];
export type JobScheduleWeekDay = components['schemas']['Fei.Is.Api.Data.Enums.JobScheduleWeekDayEnum'];

export type JobScheduleListItem =
  paths['/devices/{deviceId}/job-schedules']['get']['responses']['200']['content']['application/json'][number];

export interface JobScheduleWithRecipe extends JobScheduleListItem {
  recipeName?: string;
}
