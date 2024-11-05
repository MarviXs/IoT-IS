import { RulesLogic } from 'json-logic-js';

interface SceneAction {
  type: 'JOB' | 'NOTIFICATION';
  deviceId?: string;
  recipeId?: string;
  notificationMessage?: string;
}

interface Scene {
  id?: string;
  name: string;
  description?: string;
  isEnabled: boolean;
  triggerType: 'scheduled' | 'conditional';
  condition: RulesLogic;
  actions: SceneAction[];
}

export type { Scene, SceneAction };
