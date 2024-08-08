<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('device_template.label', 2), to: '/device-templates' },
      { label: deviceTemplate?.name, to: `/device-templates/${templateId}` },
      { label: t('recipe.label', 2), to: `/device-templates/${templateId}/recipes` },
      { label: t('global.edit') },
    ]"
    :title="t('global.edit')"
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
        :loading="updatingRecipe"
        @click="updateRecipe"
      />
    </template>
    <RecipeForm v-if="recipe" ref="recipeForm" v-model="recipe" :loading="updatingRecipe" />
  </PageLayout>
</template>

<script setup lang="ts">
import RecipeForm from '@/components/recipes/RecipeForm.vue';
import PageLayout from '@/layouts/PageLayout.vue';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useRoute, useRouter } from 'vue-router';
import RecipeService from '@/api/services/RecipeService';
import { DeviceTemplateResponse } from '@/api/types/DeviceTemplate';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';
import { RecipeResponse, UpdateRecipeRequest } from '@/api/types/Recipe';
import { UpdateRecipeStepsRequest } from '@/api/types/RecipeStep';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();

const templateId = route.params.templateId as string;
const recipeId = route.params.recipeId as string;

const recipe = ref<RecipeResponse>();

const recipeForm = ref();
const updatingRecipe = ref(false);

async function getRecipe() {
  const { data, error } = await RecipeService.getRecipe(recipeId);
  if (error) {
    handleError(error, t('recipe.toasts.load_failed'));
    return;
  }
  recipe.value = data;
}
getRecipe();

async function updateRecipe() {
  if (!recipeForm.value?.validate() || !recipe.value?.id) return;

  updatingRecipe.value = true;

  const recipeRequest: UpdateRecipeRequest = {
    name: recipe.value?.name,
  };

  const recipeResponse = await RecipeService.updateRecipe(recipeId, recipeRequest);
  if (recipeResponse.error) {
    updatingRecipe.value = false;
    handleError(recipeResponse.error, t('recipe.toasts.update_failed'));
    return;
  }

  const stepRequest: UpdateRecipeStepsRequest =
    recipe.value?.steps?.map((step, index) => ({
      stepId: step.id ?? null,
      commandId: step.command?.id ?? null,
      subrecipeId: step.subrecipe?.id ?? null,
      cycles: step.cycles,
      order: index + 1,
    })) ?? [];

  const stepResponse = await RecipeService.updateRecipeSteps(recipe.value.id, stepRequest);
  updatingRecipe.value = false;
  if (stepResponse.error) {
    handleError(stepResponse.error, t('recipe.toasts.update_failed'));
    return;
  }

  toast.success(t('recipe.toasts.update_success'));
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
  };
}
getDeviceTemplate();
</script>
