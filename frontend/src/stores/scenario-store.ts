import { defineStore } from 'pinia';
import { ref, toRaw } from 'vue';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import { Scenario, ScenarioFrame } from '@/models/Scenario';
import { StoredResolvedScenario } from '@/models/StoredResolvedScenario';
import ScenarioService from '@/services/ScenarioService';
import StoredResolvedScenarioService from '@/services/StoredResolvedScenarioService';
import DeviceService from '@/services/DeviceService';
import { handleError } from '@/utils/error-handler';

export const useScenarioStore = defineStore('scenarios', () => {
  const { t } = useI18n();
  const scenarios = ref<Scenario[]>([]);
  const resolvedScenarios = ref<StoredResolvedScenario[]>([]);
  const isLoadingScenarios = ref(false);

  const mode = 'create';

  async function getScenarios() {
    try {
      isLoadingScenarios.value = true;
      scenarios.value = await ScenarioService.getScenarios();
      for (const tmpScenario of scenarios.value) {
        const currentTime = Date.now();
        const diffInMilliseconds = tmpScenario.mutedUntil - currentTime;
        const remainingMutedMinutes = Math.ceil(diffInMilliseconds / 60000);
        if (remainingMutedMinutes <= 0) {
          tmpScenario.mutedUntil = 0;
        } else {
          tmpScenario.mutedUntil = remainingMutedMinutes;
        }
      }
    } catch (error) {
      console.log(error);
      handleError(error, t('scenario.toasts.load_failed'));
    } finally {
      isLoadingScenarios.value = false;
    }
  }

  async function getActiveScenarios() {
    try {
      isLoadingScenarios.value = true;
      scenarios.value = await ScenarioService.getActiveScenarios();
      for (const tmpScenario of scenarios.value) {
        const currentTime = Date.now();
        const diffInMilliseconds = tmpScenario.mutedUntil - currentTime;
        const remainingMutedMinutes = Math.ceil(diffInMilliseconds / 60000);
        if (remainingMutedMinutes <= 0) {
          tmpScenario.mutedUntil = 0;
        } else {
          tmpScenario.mutedUntil = remainingMutedMinutes;
        }
      }
    } catch (error) {
      handleError(error, t('scenario.toasts.load_failed'));
    } finally {
      isLoadingScenarios.value = false;
    }
  }

  async function getScenariosWithMessage() {
    try {
      isLoadingScenarios.value = true;
      scenarios.value = await ScenarioService.getScenariosWithMessage();
    } catch (error) {
      handleError(error, t('notification.toasts.load_failed'));
    } finally {
      isLoadingScenarios.value = false;
    }
  }

  async function getResolvedScenariosWithMessage() {
    try {
      isLoadingScenarios.value = true;
      resolvedScenarios.value = await StoredResolvedScenarioService.getStoredResolvedScenarioDataByMessageAction();
    } catch (error) {
      handleError(error, t('notification.toasts.load_failed'));
    } finally {
      isLoadingScenarios.value = false;
    }
  }

  const scenarioPseudocode = ref('');
  const scenarioFrame = ref<ScenarioFrame>({
    rules: ' ',
    name: '',
    devices: [],
    deactivated: false,
    alreadyTriggered: false,
    mutedUntil: 0,
    activeAtDay: [],
    activeAtHour: [],
    deviceAndTag: null,
  });
  const isCreatingScenario = ref(false);
  async function createScenario() {
    try {
      isCreatingScenario.value = true;
      scenarioFrame.value.name.trim;
      // TODO remove
      const hourValues = scenarioFrame.value.activeAtHour.map((hour) => hour.value);
      scenarioFrame.value.activeAtHour = hourValues;

      const deviceTagMap: { [key: string]: string[] } = {};
      // Iterate over each device UID
      for (const deviceUID of scenarioFrame.value.devices) {
        try {
          // Fetch the device details
          const deviceWithTags = await DeviceService.getDevice(deviceUID);
          // Ensure dataPointTags is an array before mapping
          if (deviceWithTags && Array.isArray(deviceWithTags.dataPointTags)) {
            deviceTagMap[deviceWithTags.uid] = deviceWithTags.dataPointTags.map((tag) => tag.tag);
          } else {
            deviceTagMap[deviceWithTags.uid] = [];
          }
        } catch (error) {
          console.error(`Failed to fetch device ${deviceUID}:`, error);
        }
      }
      scenarioFrame.value.deviceAndTag = deviceTagMap;

      await ScenarioService.createScenario(scenarioFrame.value);
      toast.success(t('scenario.toasts.create_success'));
    } catch (error) {
      handleError(error, t('scenario.toasts.create_failed'));
    } finally {
      isCreatingScenario.value = false;
    }
  }
  async function editScenario(editedScenario: Scenario) {
    try {
      isCreatingScenario.value = true;
      scenarioFrame.value.name.trim;
      await ScenarioService.editScenario(editedScenario);
      toast.success(t('scenario.toasts.update_success'));
    } catch (error) {
      handleError(error, t('scenario.toasts.update_failed'));
    } finally {
      isCreatingScenario.value = false;
    }
  }

  async function muteScenario(scenarioID: string, minutes: number) {
    try {
      await ScenarioService.muteScenario(scenarioID, minutes);
      toast.success(t('scenario.toasts.update_success'));
    } catch (error) {
      handleError(error, t('scenario.toasts.update_failed'));
    } finally {
    }
  }

  async function resolveScenario(scenarioId: string, scenarioLevelEnum: string) {
    try {
      isLoadingScenarios.value = true;
      await ScenarioService.resolveScenario(scenarioId, scenarioLevelEnum);
      toast.success(t('notification.toasts.processing_successful'));
    } catch (error) {
      handleError(error, t('notification.toasts.proccessing_failed'));
    } finally {
      isLoadingScenarios.value = false;
    }
  }

  return {
    scenarios,
    resolveScenario,
    resolvedScenarios,
    isLoadingScenarios,
    scenarioFrame,
    getScenarios,
    createScenario,
    getActiveScenarios,
    mode,
    editScenario,
    scenarioPseudocode,
    muteScenario,
    getScenariosWithMessage,
    getResolvedScenariosWithMessage,
  };
});
