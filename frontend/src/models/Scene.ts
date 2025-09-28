import type { RulesLogic } from 'json-logic-js';

interface SceneAction {
  type: 'JOB' | 'NOTIFICATION';
  deviceId?: string | null;
  recipeId?: string | null;
  notificationMessage?: string | null;
  notificationSeverity: 'Info' | 'Warning' | 'Serious' | 'Critical';
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
