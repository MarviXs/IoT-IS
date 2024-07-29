<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('device_template.label', 2), to: '/device-templates' },
      { label: deviceTemplate?.name, to: `/device-templates/${templateId}` },
      { label: t('recipe.label', 2), to: `/device-templates/${templateId}/recipes` },
      { label: t('global.create') },
    ]"
    :title="t('global.create')"
    :previous-title="t('recipe.label', 2)"
    previous-route="/recipes"
  >
    <template #actions>
      <q-btn
        unelevated
        color="primary"
        :label="t('global.save')"
        padding="7px 35px"
        no-caps
        type="submit"
        :loading="creatingRecipe"
        @click="createRecipe"
      />
    </template>
    <RecipeForm ref="recipeForm" v-model="recipe" />
  </PageLayout>
</template>

<script setup lang="ts">
import RecipeForm from '@/components/recipes/RecipeForm.vue';
import PageLayout from '@/layouts/PageLayout.vue';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import RecipeService from '@/api/services/RecipeService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useRoute, useRouter } from 'vue-router';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';
import { DeviceTemplateResponse } from '@/api/types/DeviceTemplate';
import { RecipeResponse } from '@/api/types/Recipe';
import { UpdateRecipeStepsRequest } from '@/api/types/RecipeStep';
import { CreateRecipeRequest } from '@/api/types/Recipe';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const templateId = route.params.id as string;

const recipe = ref<RecipeResponse>({
  id: '',
  name: '',
  steps: [],
});

const creatingRecipe = ref(false);
const recipeForm = ref();

async function createRecipe() {
  if (!recipeForm.value?.validate()) return;

  creatingRecipe.value = true;

  const recipeRequest: CreateRecipeRequest = {
    name: recipe.value.name,
    deviceTemplateId: templateId,
  };

  const recipeResponse = await RecipeService.createRecipe(recipeRequest);
  if (recipeResponse.error) {
    creatingRecipe.value = false;
    handleError(recipeResponse.error, t('recipe.toasts.create_failed'));
    return;
  }

  const stepRequest: UpdateRecipeStepsRequest =
    recipe.value.steps?.map((step, index) => ({
      stepId: step.id ?? null,
      commandId: step.command?.id ?? null,
      subrecipeId: step.subrecipe?.id ?? null,
      cycles: step.cycles,
      order: index + 1,
    })) ?? [];

  const stepResponse = await RecipeService.updateRecipeSteps(recipeResponse.data, stepRequest);
  creatingRecipe.value = false;
  if (stepResponse.error) {
    handleError(stepResponse.error, t('recipe.toasts.create_failed'));
    return;
  }

  toast.success(t('recipe.toasts.create_success'));
  router.push(`/device-templates/${templateId}/recipes`);
}

const deviceTemplate = ref<DeviceTemplateResponse>();
async function getDeviceTemplate() {
  const { data, error } = await DeviceTemplateService.getDeviceTemplate(templateId);
  if (error) {
    handleError(error, 'Error fetching device template');
    return;
  }
  deviceTemplate.value = {
    id: data.id,
    name: data.name,
    modelId: data.modelId,
  };
}
getDeviceTemplate();
</script>
