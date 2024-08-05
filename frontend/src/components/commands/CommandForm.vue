<template>
  <div>
    <div class="q-gutter-y-md">
      <q-input
        ref="displayNameRef"
        v-model="command.displayName"
        :rules="nameRules"
        autofocus
        :label="t('global.display_name')"
      />
      <q-input ref="nameRef" v-model="command.name" :rules="nameRules" autofocus :label="t('global.name')" />
      <div class="text-parameters">{{ t('command.parameters') }}</div>
      <VueDraggable
        v-if="localParams.length > 0"
        v-model="localParams"
        :animation="200"
        handle=".handle"
        class="command-table"
      >
        <div v-for="(parameter, index) in localParams" :key="parameter.id">
          <div class="command-container bg-white sortable-drag full-height row items-start justify-start no-wrap">
            <q-icon class="handle drag-icon q-mr-md q-ml-sm col-shrink" :name="mdiDrag" size="28px" />
            <div class="col">
              <q-input v-model="parameter.value" type="number" borderless :placeholder="t('global.value')" />
            </div>
            <q-btn
              class="q-mr-md q-ml-md col-shrink"
              rounded
              flat
              dense
              unelevated
              :icon="mdiClose"
              color="red"
              @click="removeParameter(index)"
            />
          </div>
        </div>
      </VueDraggable>
      <div>
        <q-btn
          class="full-width q-mb-md"
          outline
          :icon="mdiPlusCircle"
          color="primary"
          no-caps
          padding="8px"
          :label="t('command.add_parameter')"
          @click="addParameter"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiPlusCircle, mdiDrag, mdiClose } from '@quasar/extras/mdi-v7';
import { VueDraggable } from 'vue-draggable-plus';
import { isFormValid } from '@/utils/form-validation';

export type CommandFormData = {
  id?: string;
  displayName: string;
  name: string;
  params: number[];
};

const command = defineModel<CommandFormData>({ required: true });

const { t } = useI18n();

const mapParameter = (value: number) => ({ id: Date.now() + Math.random(), value });

interface Parameter {
  id: number;
  value: number | null;
}
const localParams = ref<Parameter[]>(command.value.params.map(mapParameter));

const addParameter = () => {
  const uniqueId = Date.now() + Math.random();
  localParams.value.push({ id: uniqueId, value: null });
};

const removeParameter = (index: number) => {
  localParams.value.splice(index, 1);
};

const nameRef = ref();

const nameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

function validate() {
  const refs = [nameRef.value];
  return isFormValid(refs);
}

watch(
  () => command.value.id,
  () => {
    localParams.value = command.value.params.map(mapParameter);
  },
  { deep: true },
);

watch(
  localParams,
  (newParams) => {
    command.value.params = newParams.map((p) => p.value) as number[];
  },
  { deep: true },
);

defineExpose({
  validate,
});
</script>

<style lang="scss" scoped>
.text-parameters {
  font-size: 1rem;
  font-weight: 500;
}

.command-container {
  display: flex;
  align-items: center;
  border: 1px solid #ccc;
  align-items: center;
  margin-bottom: -1px;
  height: 3.2rem;
}

.drag-icon {
  cursor: move;
}
</style>
