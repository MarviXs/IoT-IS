<template>
  <q-form class="map-options-form" @submit="emit('onSubmit')">
    <q-card-section class="q-pt-none column q-gutter-md">
      <div>
        <div class="form-subtitle text-primary">{{ t('datapoints.map.options.marker_section') }}</div>
        <div class="map-options-grid">
          <q-input
            v-model.number="options.markerRadius"
            type="number"
            :label="t('datapoints.map.options.marker_radius')"
            :min="1"
            :step="1"
          />
          <q-input
            v-model.number="options.markerBorderWeight"
            type="number"
            :label="t('datapoints.map.options.marker_border_weight')"
            :min="0"
            :step="1"
          />
          <q-input
            v-model.number="options.markerFillOpacity"
            type="number"
            :label="t('datapoints.map.options.marker_fill_opacity')"
            :min="0"
            :max="1"
            :step="0.05"
          />
          <q-input
            v-model="options.markerBorderColor"
            :label="t('datapoints.map.options.marker_border_color')"
            :hint="t('datapoints.map.options.color_hint')"
          />
          <q-input
            v-model="options.markerFillColor"
            :label="t('datapoints.map.options.marker_fill_color')"
            :hint="t('datapoints.map.options.color_hint')"
          />
        </div>
      </div>
      <div>
        <div class="form-subtitle text-primary">{{ t('datapoints.map.options.heatmap_section') }}</div>
        <div class="map-options-grid">
          <q-input
            v-model.number="options.heatRadius"
            type="number"
            :label="t('datapoints.map.options.heat_radius')"
            :min="1"
            :step="1"
          />
          <q-input
            v-model.number="options.heatBlur"
            type="number"
            :label="t('datapoints.map.options.heat_blur')"
            :min="0"
            :step="1"
          />
        </div>
      </div>
    </q-card-section>
    <q-card-actions align="right" class="text-primary">
      <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
      <q-btn unelevated color="primary" :label="t('global.save')" type="submit" no-caps padding="6px 20px" />
    </q-card-actions>
  </q-form>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import { useI18n } from 'vue-i18n';

export type MapOptions = {
  markerRadius: number;
  markerBorderColor: string;
  markerBorderWeight: number;
  markerFillColor: string;
  markerFillOpacity: number;
  heatRadius: number;
  heatBlur: number;
};

const { t } = useI18n();

const options = defineModel<MapOptions>({
  type: Object as PropType<MapOptions>,
  required: true,
});

const emit = defineEmits(['onSubmit']);
</script>

<style scoped>
.map-options-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 1rem;
}

.form-subtitle {
  font-size: 1rem;
  font-weight: 450;
  margin-top: 0.5rem;
  margin-bottom: 0.5rem;
}
</style>
