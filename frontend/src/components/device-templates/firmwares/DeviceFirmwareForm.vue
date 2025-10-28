<template>
  <q-form ref="formRef" greedy @submit="onSubmit">
    <q-card-section class="q-pt-none column q-gutter-md">
      <q-input
        v-model="firmware.versionNumber"
        :label="t('device_template.firmwares.version_number')"
        :rules="[requiredRule]"
        autofocus
      />
      <div class="row items-center justify-between">
        <div class="text-subtitle2">{{ t('device_template.firmwares.is_active') }}</div>
        <q-toggle v-model="firmware.isActive" color="primary" />
      </div>
      <div class="column">
        <q-file
          v-model="firmware.firmwareFile"
          :label="t('device_template.firmwares.firmware_file')"
          :rules="fileRules"
          clearable
          use-chips
        />
        <div v-if="props.existingFileName" class="text-caption text-grey-7 q-mt-xs">
          {{ t('device_template.firmwares.current_file', { fileName: props.existingFileName }) }}
        </div>
      </div>
    </q-card-section>
    <q-card-actions align="right">
      <q-btn v-close-popup flat no-caps :label="t('global.cancel')" />
      <q-btn color="primary" unelevated no-caps type="submit" :label="t('global.save')" :loading="props.isLoading" />
    </q-card-actions>
  </q-form>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';

export type DeviceFirmwareFormModel = {
  versionNumber: string;
  isActive: boolean;
  firmwareFile: File | null;
};

const props = defineProps({
  isLoading: {
    type: Boolean,
    default: false,
  },
  requireFile: {
    type: Boolean,
    default: false,
  },
  existingFileName: {
    type: String,
    default: '',
  },
});

const emit = defineEmits(['onSubmit']);

const firmware = defineModel<DeviceFirmwareFormModel>({ required: true });

const { t } = useI18n();

const formRef = ref();

const requiredRule = (val: string | null | undefined) =>
  (!!val && val.toString().length > 0) || t('global.rules.required');

const fileRules = computed(() => {
  if (!props.requireFile) {
    return [];
  }

  return [(val: File | null) => !!val || t('global.rules.required')];
});

function onSubmit() {
  if (!formRef.value?.validate()) {
    return;
  }

  emit('onSubmit');
}
</script>

<style lang="scss" scoped>
.text-subtitle2 {
  font-weight: 500;
}
</style>
