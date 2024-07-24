<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('recipe.delete_recipe') }}</template>
    <template #description>{{ t('recipe.delete_recipe_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '@/components/core/DeleteConfirmationDialog.vue';
import RecipeService from '@/api/services/RecipeService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  recipeId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();

const isDeleteInProgress = ref(false);
async function handleDelete() {
  isDeleteInProgress.value = true;
  const { data, error } = await RecipeService.deleteRecipe(props.recipeId);
  isDeleteInProgress.value = false;
  isDialogOpen.value = false;

  if (error) {
    handleError(error, t('recipe.toasts.delete_failed'));
    return;
  }

  emit('onDeleted');
  toast.success(t('recipe.toasts.delete_success'));
}
</script>
