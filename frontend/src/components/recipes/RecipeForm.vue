<template>
  <div>
    <q-form @submit="emit('onSubmit')" ref="formRef" greedy>
      <q-btn type="submit" style="display: none" />
      <q-card class="q-pa-lg shadow">
        <div class="row items-center q-col-gutter-x-xl q-col-gutter-y-md">
          <q-input
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
    </q-form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import RecipeStepsEditor from '@/components/recipes/RecipeStepsEditor.vue';
import { RecipeResponse } from '@/api/services/RecipeService';

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
const emit = defineEmits(['onSubmit']);

const { t } = useI18n();

const nameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

const formRef = ref();

async function validate() {
  if (!formRef.value) return false;
  return await formRef.value.validate();
}
defineExpose({ validate });
</script>
