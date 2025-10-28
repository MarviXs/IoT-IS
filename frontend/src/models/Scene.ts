import type { RulesLogic } from 'json-logic-js';

interface SceneAction {
  type: 'JOB' | 'NOTIFICATION' | 'DISCORD_NOTIFICATION';
  deviceId?: string | null;
  recipeId?: string | null;
  notificationMessage?: string | null;
  notificationSeverity: 'Info' | 'Warning' | 'Serious' | 'Critical' | null;
  discordWebhookUrl?: string | null;
  includeSensorValues: boolean | null;
}

interface Scene {
  id?: string;
  name: string;
  description?: string;
  isEnabled: boolean;
  condition: RulesLogic;
  actions: SceneAction[];
  cooldownAfterTriggerTime: number;
}

export type { Scene, SceneAction };
