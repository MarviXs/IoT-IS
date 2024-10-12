import { RulesLogic } from 'json-logic-js';

interface Scene {
  id?: string;
  name: string;
  description?: string;
  isActive: boolean;
  triggerType: 'scheduled' | 'conditional';
  condition: { if: [RulesLogic, string, string] };
}

export type { Scene };
