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
                  class="col-12 col-md-6"
                  :label="t('device_template.control_name')"
                  :rules="requiredRules"
                />
                <q-input
                  v-model="controls[index].color"
                  class="col-12 col-md-6"
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
                  v-model="controls[index].recipeId"
                  class="col-12 col-md-6"
                  :options="recipeOptions"
                  map-options
                  emit-value
                  :label="t('device_template.control_recipe')"
                  :rules="requiredRules"
                />
                <q-input
                  v-model.number="controls[index].cycles"
                  class="col-12 col-md-3"
                  type="number"
                  :disable="controls[index].isInfinite"
                  :label="t('device_template.control_cycles')"
                  :rules="cycleRules"
                  min="1"
                />
                <div class="col-12 col-md-3 flex items-center">
                  <q-toggle
                    v-model="controls[index].isInfinite"
                    color="primary"
                    :label="t('device_template.control_infinite')"
                  />
                </div>
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
import { onMounted, ref } from 'vue';
import type { ComponentPublicInstance } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import type { QForm, QPopupProxy } from 'quasar';
import { toast } from 'vue3-toastify';
import { handleError } from '@/utils/error-handler';
import RecipeService from '@/api/services/RecipeService';
import DeviceTemplateControlService from '@/api/services/DeviceTemplateControlService';
import type { DeviceTemplateControlResponse } from '@/api/services/DeviceTemplateControlService';

interface ControlFormData {
  id: string | null;
  localId: string;
  name: string;
  color: string;
  recipeId: string;
  cycles: number;
  isInfinite: boolean;
}

interface Option {
  label: string;
  value: string;
}

const { t } = useI18n();
const route = useRoute();
const templateId = route.params.id as string;

const controls = ref<ControlFormData[]>([]);
const isLoading = ref(false);
const isSaving = ref(false);
const formRef = ref<QForm>();
const colorPickerRefs = ref<Record<string, QPopupProxy | null>>({});

const recipeOptions = ref<Option[]>([]);

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

async function loadControls() {
  const { data, error } = await DeviceTemplateControlService.getTemplateControls(templateId);
  if (error) {
    handleError(error, 'Error fetching controls');
    return;
  }

  controls.value = (data ?? []).map(mapControlResponse);
}

function mapControlResponse(control: DeviceTemplateControlResponse): ControlFormData {
  ensureRecipeOption(control.recipeId, control.recipeName);

  return {
    id: control.id,
    localId: control.id,
    name: control.name,
    color: normalizeColor(control.color),
    recipeId: control.recipeId,
    cycles: control.cycles,
    isInfinite: control.isInfinite,
  };
}

function ensureRecipeOption(id: string, name: string) {
  if (!recipeOptions.value.some((option) => option.value === id)) {
    recipeOptions.value.push({ label: name, value: id });
  }
}

function addControl() {
  const id = `${Date.now()}-${Math.random()}`;
  controls.value.push({
    id: null,
    localId: id,
    name: '',
    color: DEFAULT_COLOR,
    recipeId: recipeOptions.value[0]?.value ?? '',
    cycles: 1,
    isInfinite: false,
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
    return {
      id: control.id,
      name: control.name,
      color: sanitizedColor,
      recipeId: control.recipeId,
      cycles: control.isInfinite ? Math.max(control.cycles, 1) : control.cycles,
      isInfinite: control.isInfinite,
    };
  });

  const { error } = await DeviceTemplateControlService.updateTemplateControls(templateId, payload);
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
