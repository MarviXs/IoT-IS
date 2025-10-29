<template>
  <div>
    <div class="actions">
      <div class="title">{{ t('device_template.controls') }}</div>
      <q-space />
      <q-btn
        class="shadow"
        color="primary"
        :icon="mdiPlus"
        :label="t('device_template.add_control')"
        unelevated
        no-caps
        size="15px"
        :disable="isLoading || isSaving"
        @click="addControl"
      />
    </div>
    <q-card class="shadow q-pa-md">
      <q-inner-loading :showing="isLoading">
        <q-spinner color="primary" size="42px" />
      </q-inner-loading>
      <q-form v-if="!isLoading" ref="formRef" greedy @submit.prevent="submitForm">
        <VueDraggable v-if="controls.length > 0" v-model="controls" handle=".handle" :animation="200">
          <div v-for="(control, index) in controls" :key="control.localId" class="q-mb-md">
            <q-card flat bordered class="control-card">
              <div class="control-card__header">
                <q-icon class="handle" :name="mdiDrag" size="26px" />
                <div class="control-card__title">
                  {{ control.name ? control.name : t('device_template.control_default_name', { index: index + 1 }) }}
                </div>
                <q-space />
                <q-btn flat round dense color="grey-7" :icon="mdiTrashCanOutline" @click="removeControl(index)" />
              </div>
              <div class="row q-col-gutter-md">
                <q-input
                  v-model="controls[index].name"
                  class="col-12 col-md-4"
                  :label="t('device_template.control_name')"
                  :rules="requiredRules"
                />
                <q-input
                  v-model="controls[index].color"
                  class="col-12 col-md-4"
                  :label="t('device_template.control_color')"
                  :rules="colorRules"
                  @blur="controls[index].color = sanitizeColor(controls[index].color)"
                >
                  <template #prepend>
                    <div
                      class="color-preview"
                      role="button"
                      tabindex="0"
                      :aria-label="t('device_template.control_color')"
                      :style="{ backgroundColor: getPreviewColor(controls[index].color) }"
                      @click="openColorPicker(control.localId)"
                      @keydown.enter.prevent="openColorPicker(control.localId)"
                      @keydown.space.prevent="openColorPicker(control.localId)"
                    ></div>
                  </template>
                  <template #append>
                    <q-btn
                      class="color-picker-button"
                      dense
                      flat
                      round
                      :icon="mdiPalette"
                      @click.stop="openColorPicker(control.localId)"
                    >
                      <q-popup-proxy
                        cover
                        transition-show="scale"
                        transition-hide="scale"
                        :ref="(el) => setColorPickerRef(control.localId, el)"
                      >
                        <q-color
                          v-model="controls[index].color"
                          format="hex"
                          :default-view="'palette'"
                          @change="(value) => updateColor(index, value)"
                        />
                      </q-popup-proxy>
                    </q-btn>
                  </template>
                </q-input>
                <q-select
                  v-model="controls[index].type"
                  class="col-12 col-md-4"
                  :options="controlTypeOptions"
                  map-options
                  emit-value
                  :label="t('device_template.control_type')"
                  :rules="requiredRules"
                  @update:model-value="(value) => handleTypeChange(index, value)"
                />
                <template v-if="controls[index].type === 'Run'">
                  <q-select
                    v-model="controls[index].recipeId"
                    class="col-12 col-md-4"
                    :options="recipeOptions"
                    map-options
                    emit-value
                    :label="t('device_template.control_recipe')"
                    :rules="requiredRules"
                  />
                  <q-input
                    v-model.number="controls[index].cycles"
                    class="col-12 col-md-4"
                    type="number"
                    :disable="controls[index].isInfinite"
                    :label="t('device_template.control_cycles')"
                    :rules="cycleRules"
                    min="1"
                  />
                  <div class="col-12 col-md-4 flex items-center">
                    <q-toggle
                      v-model="controls[index].isInfinite"
                      color="primary"
                      :label="t('device_template.control_infinite')"
                    />
                  </div>
                </template>
                <template v-else>
                  <q-select
                    v-model="controls[index].recipeOnId"
                    class="col-12 col-md-4"
                    :options="recipeOptions"
                    map-options
                    emit-value
                    :label="t('device_template.control_recipe_on')"
                    :rules="requiredRules"
                  />
                  <q-select
                    v-model="controls[index].recipeOffId"
                    class="col-12 col-md-4"
                    :options="recipeOptions"
                    map-options
                    emit-value
                    :label="t('device_template.control_recipe_off')"
                    :rules="requiredRules"
                  />
                  <q-select
                    v-model="controls[index].sensorId"
                    class="col-12 col-md-4"
                    :options="sensorOptions"
                    map-options
                    emit-value
                    :label="t('device_template.control_sensor')"
                    :rules="requiredRules"
                  />
                </template>
              </div>
            </q-card>
          </div>
        </VueDraggable>
        <div v-else class="no-controls text-grey-6">
          {{ t('device_template.no_controls') }}
        </div>
      </q-form>
    </q-card>
    <q-btn
      unelevated
      color="primary"
      class="q-mt-md"
      :label="t('global.save')"
      padding="7px 35px"
      no-caps
      :loading="isSaving"
      :disable="isLoading"
      @click="submitForm"
    />
  </div>
</template>

<script setup lang="ts">
import { mdiDrag, mdiPalette, mdiPlus, mdiTrashCanOutline } from '@quasar/extras/mdi-v7';
import { VueDraggable } from 'vue-draggable-plus';
import { computed, onMounted, ref } from 'vue';
import type { ComponentPublicInstance } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import type { QForm, QPopupProxy } from 'quasar';
import { toast } from 'vue3-toastify';
import { handleError } from '@/utils/error-handler';
import RecipeService from '@/api/services/RecipeService';
import DeviceControlService from '@/api/services/DeviceControlService';
import type {
  DeviceControlResponse,
  UpdateDeviceTemplateControlsRequest,
} from '@/api/services/DeviceControlService';
import SensorService from '@/api/services/SensorService';

type DeviceControlType = 'Run' | 'Toggle';

interface ControlFormData {
  id: string | null;
  localId: string;
  name: string;
  color: string;
  type: DeviceControlType;
  recipeId: string | null;
  cycles: number;
  isInfinite: boolean;
  recipeOnId: string | null;
  recipeOffId: string | null;
  sensorId: string | null;
}

type Option<T = string> = {
  label: string;
  value: T;
};

const { t } = useI18n();
const route = useRoute();
const templateId = route.params.id as string;

const controls = ref<ControlFormData[]>([]);
const isLoading = ref(false);
const isSaving = ref(false);
const formRef = ref<QForm>();
const colorPickerRefs = ref<Record<string, QPopupProxy | null>>({});

const recipeOptions = ref<Option[]>([]);
const sensorOptions = ref<Option[]>([]);
const controlTypeOptions = computed<Option<DeviceControlType>[]>(() => [
  { label: t('device_template.control_type_run'), value: 'Run' },
  { label: t('device_template.control_type_toggle'), value: 'Toggle' },
]);

const brandColorHexMap: Record<string, string> = {
  primary: '#1976D2',
  secondary: '#26A69A',
  accent: '#9C27B0',
  positive: '#21BA45',
  negative: '#C10015',
  warning: '#F2C037',
  info: '#31CCEC',
  dark: '#1D1D1D',
};

const DEFAULT_COLOR = brandColorHexMap.primary;

const requiredRules = [(val: string | null) => (val && `${val}`.length > 0) || t('global.rules.required')];
const colorRules = [
  (val: string | null) => (val && `${val}`.length > 0) || t('global.rules.required'),
  (val: string | null) => (val && isHexColor(val) ? true : t('device_template.rules.hex_color')),
];
const cycleRules = [
  (val: number | null) =>
    val === null || Number.isNaN(val) || val < 1 ? t('device_template.rules.cycle_positive') : true,
];

onMounted(async () => {
  await loadData();
});

async function loadData() {
  isLoading.value = true;
  try {
    await loadRecipes();
    await loadSensors();
    await loadControls();
  } finally {
    isLoading.value = false;
  }
}

type PopupProxyInstance = QPopupProxy & ComponentPublicInstance;

function setColorPickerRef(
  localId: string,
  el: PopupProxyInstance | Element | ComponentPublicInstance | null,
) {
  if (el && 'show' in el && typeof (el as QPopupProxy).show === 'function') {
    colorPickerRefs.value[localId] = el as unknown as QPopupProxy;
  } else if (el === null) {
    delete colorPickerRefs.value[localId];
  }
}

function openColorPicker(localId: string) {
  const popup = colorPickerRefs.value[localId];
  popup?.show();
}

async function loadRecipes() {
  const { data, error } = await RecipeService.getRecipes({
    DeviceTemplateId: templateId,
    SortBy: 'name',
    Descending: false,
    SearchTerm: '',
    PageNumber: 1,
    PageSize: 1000,
  });

  if (error) {
    handleError(error, 'Error fetching recipes');
    return;
  }

  if (!data) {
    recipeOptions.value = [];
    return;
  }

  recipeOptions.value = data.items.map((recipe) => ({ label: recipe.name, value: recipe.id }));
}

async function loadSensors() {
  const { data, error } = await SensorService.getTemplateSensors(templateId);
  if (error) {
    handleError(error, 'Error fetching sensors');
    return;
  }

  const sensors = data ?? [];
  sensorOptions.value = sensors.map((sensor) => ({ label: sensor.name, value: sensor.id }));
}

async function loadControls() {
  const { data, error } = await DeviceControlService.getTemplateControls(templateId);
  if (error) {
    handleError(error, 'Error fetching controls');
    return;
  }

  controls.value = (data ?? []).map(mapControlResponse);
}

function mapControlType(type: unknown): DeviceControlType {
  if (type === 'Toggle' || type === 1) {
    return 'Toggle';
  }
  return 'Run';
}

function mapControlResponse(control: DeviceControlResponse): ControlFormData {
  ensureRecipeOption(control.recipeId, control.recipeName);
  ensureRecipeOption(control.recipeOnId, control.recipeOnName ?? undefined);
  ensureRecipeOption(control.recipeOffId, control.recipeOffName ?? undefined);
  ensureSensorOption(control.sensorId, control.sensorName ?? undefined);

  const type = mapControlType(control.type);

  return {
    id: control.id,
    localId: control.id,
    name: control.name,
    color: normalizeColor(control.color),
    type,
    recipeId: control.recipeId ?? null,
    cycles: control.cycles > 0 ? control.cycles : 1,
    isInfinite: type === 'Run' ? control.isInfinite : false,
    recipeOnId: control.recipeOnId ?? null,
    recipeOffId: control.recipeOffId ?? null,
    sensorId: control.sensorId ?? null,
  };
}

function ensureRecipeOption(id?: string | null, name?: string | null) {
  if (!id || !name) {
    return;
  }
  if (!recipeOptions.value.some((option) => option.value === id)) {
    recipeOptions.value.push({ label: name, value: id });
  }
}

function ensureSensorOption(id?: string | null, name?: string | null) {
  if (!id || !name) {
    return;
  }
  if (!sensorOptions.value.some((option) => option.value === id)) {
    sensorOptions.value.push({ label: name, value: id });
  }
}

function addControl() {
  const id = `${Date.now()}-${Math.random()}`;
  controls.value.push({
    id: null,
    localId: id,
    name: '',
    color: DEFAULT_COLOR,
    type: 'Run',
    recipeId: recipeOptions.value[0]?.value ?? null,
    cycles: 1,
    isInfinite: false,
    recipeOnId: null,
    recipeOffId: null,
    sensorId: null,
  });
}

function removeControl(index: number) {
  const [removed] = controls.value.splice(index, 1);
  if (removed) {
    delete colorPickerRefs.value[removed.localId];
  }
}

async function submitForm() {
  if (!formRef.value) {
    return;
  }

  const isValid = await formRef.value.validate();
  if (!isValid) {
    return;
  }

  isSaving.value = true;

  const payload = controls.value.map((control) => {
    const sanitizedColor = sanitizeColor(control.color) || DEFAULT_COLOR;
    const isRun = control.type === 'Run';
    const toNullableId = (value: string | null) => (value && `${value}`.length > 0 ? value : null);
    const recipeId = isRun ? toNullableId(control.recipeId) : null;
    const recipeOnId = !isRun ? toNullableId(control.recipeOnId) : null;
    const recipeOffId = !isRun ? toNullableId(control.recipeOffId) : null;
    const sensorId = !isRun ? toNullableId(control.sensorId) : null;
    const cycles = isRun ? (control.isInfinite ? Math.max(control.cycles, 1) : control.cycles) : 1;

    return {
      id: control.id,
      name: control.name,
      color: sanitizedColor,
      type: control.type as unknown as UpdateDeviceTemplateControlsRequest['type'],
      recipeId,
      cycles,
      isInfinite: isRun ? control.isInfinite : false,
      recipeOnId,
      recipeOffId,
      sensorId,
    } satisfies UpdateDeviceTemplateControlsRequest;
  });

  const { error } = await DeviceControlService.updateTemplateControls(templateId, payload);
  isSaving.value = false;

  if (error) {
    handleError(error, 'Error updating controls');
    return;
  }

  toast.success(t('device_template.toasts.update_controls_success'));
  await loadControls();
}

function updateColor(index: number, value: string | null) {
  if (value === null) {
    return;
  }
  controls.value[index].color = sanitizeColor(value);
}

function getPreviewColor(color: string) {
  return isHexColor(color) ? color : DEFAULT_COLOR;
}

function handleTypeChange(index: number, value: DeviceControlType | null) {
  const control = controls.value[index];
  if (!control) {
    return;
  }

  const nextType = value ?? 'Run';
  control.type = nextType;

  if (nextType === 'Run') {
    control.recipeOnId = null;
    control.recipeOffId = null;
    control.sensorId = null;
    if (!control.recipeId && recipeOptions.value.length > 0) {
      control.recipeId = recipeOptions.value[0].value;
    }
  } else {
    control.recipeId = null;
    control.isInfinite = false;
    control.cycles = 1;
  }
}

function sanitizeColor(value: string) {
  const trimmed = (value ?? '').trim();
  if (!trimmed) {
    return '';
  }
  const withHash = trimmed.startsWith('#') ? trimmed : `#${trimmed}`;
  return withHash.toUpperCase();
}

function normalizeColor(value: string) {
  if (!value) {
    return DEFAULT_COLOR;
  }
  const trimmed = value.trim();
  if (isHexColor(trimmed)) {
    return trimmed.toUpperCase();
  }
  const mapped = brandColorHexMap[trimmed.toLowerCase()];
  if (mapped) {
    return mapped;
  }
  if (/^#?[0-9a-fA-F]{3,6}$/.test(trimmed)) {
    return sanitizeColor(trimmed) || DEFAULT_COLOR;
  }
  return DEFAULT_COLOR;
}

function isHexColor(value: string) {
  return /^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$/.test(value.trim());
}
</script>

<style lang="scss" scoped>
.actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 0.75rem 1rem;
  margin-bottom: 1rem;
}

.title {
  font-size: 1.4rem;
  font-weight: 600;
  margin: 0;
  color: $secondary;
}

.control-card {
  padding: 1rem;
}

.control-card__header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.control-card__title {
  font-weight: 600;
  font-size: 1rem;
}

.handle {
  cursor: move;
}

.no-controls {
  text-align: center;
  padding: 2rem 0;
}

.color-preview {
  width: 1.75rem;
  height: 1.75rem;
  border-radius: 4px;
  border: 1px solid rgba(0, 0, 0, 0.2);
}
</style>
