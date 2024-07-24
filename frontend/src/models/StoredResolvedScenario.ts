import ScenarioActionTypeEnum from './enums/ScenarioActionTypeEnum';
import ScenarioLevelEnum from './enums/ScenarioLevelEnum';

interface StoredResolvedScenario {
  id: string;
  scenarioId: string;
  scenarioActionType: ScenarioActionTypeEnum;
  createdAt?: number;
  level: ScenarioLevelEnum;
  messageAndTriggerTime?: Map<string, Array<number>>;
  messageMultiplicityCounter?: Map<string, number>;
  jobAndTriggerTime?: Map<string, Array<number>>;
}

export type { StoredResolvedScenario };
