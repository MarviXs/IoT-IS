<template>
  <div>
    <q-card class="q-pa-lg shadow">
      <div class="row items-center q-col-gutter-x-xl q-col-gutter-y-md">
        <q-input
          ref="nameRef"
          v-model="recipe.name"
          :rules="nameRules"
          :disable="props.loading"
          :label="t('global.name')"
          class="col-12"
        />
      </div>
    </q-card>
    <RecipeStepsEditor
      v-model="recipe"
      class="q-mt-none"
      :loading="props.loading"
      :device-template-id="deviceTemplateId"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { isFormValid } from '@/utils/form-validation';
import RecipeStepsEditor from '@/components/recipes/RecipeStepsEditor.vue';
import { RecipeResponse } from '@/api/types/Recipe';

const recipe = defineModel<RecipeResponse>({ required: true });
const props = defineProps({
  loading: {
    type: Boolean,
    default: false,
  },
  deviceTemplateId: {
    type: String,
    required: true,
  },
});
defineExpose({ validate });

const { t } = useI18n();

const nameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

const nameRef = ref();
const deviceTypeRef = ref();

function validate() {
  const refs = [nameRef.value, deviceTypeRef.value];
  return isFormValid(refs);
}
</script>
