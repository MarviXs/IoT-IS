import { RulesLogic } from 'json-logic-js';

interface SceneAction {
  type: 'job' | 'notification';
  deviceId?: string;
  recipeId?: string;
  notificationMessage?: string;
}

interface Scene {
  id?: string;
  name: string;
  description?: string;
  isActive: boolean;
  triggerType: 'scheduled' | 'conditional';
  condition: RulesLogic;
  actions: SceneAction[];
}

export type { Scene, SceneAction };
