<template>
  <PageLayout
    :breadcrumbs="[{ label: t('scene.label', 2), to: '/scenes' }, { label: t('global.edit') }]"
    :title="t('global.create')"
    :previous-title="t('scene.label', 2)"
    previous-route="/scenes"
  >
    <template #actions>
      <q-btn
        unelevated
        color="primary"
        :label="t('global.save')"
        @click="updateScene"
        padding="7px 35px"
        no-caps
        type="submit"
      />
    </template>
    <SceneForm v-if="scene" v-model="scene" ref="sceneForm" @on-submit="updateScene" />
  </PageLayout>
</template>

<script setup lang="ts">
import PageLayout from '@/layouts/PageLayout.vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import SceneForm from '@/components/scenes/SceneForm.vue';
import { Scene } from '@/models/Scene';
import { ref } from 'vue';
import SceneService, { UpdateSceneData } from '@/api/services/SceneService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();

const sceneId = route.params.id.toString();

const scene = ref<Scene>();

async function getScene() {
  const { data, error } = await SceneService.getScene(sceneId);

  if (error) {
    handleError(error, 'Fetching scene failed');
    return;
  }

  let parsedCondition;
  try {
    parsedCondition = JSON.parse(data.condition || '{ "and": [] }');
  } catch (error) {
    parsedCondition = { and: [] };
  }

  scene.value = {
    name: data.name,
    description: data.description ?? '',
    isEnabled: data.isEnabled,
    condition: parsedCondition,
    cooldownAfterTriggerTime: data.cooldownAfterTriggerTime,
    actions:
      data.actions.map((action) => ({
        type: action.type,
        deviceId: action.deviceId,
        recipeId: action.recipeId,
        notificationSeverity: action.notificationSeverity,
        notificationMessage: action.notificationMessage,
      })) ?? [],
  };

  console.log(scene.value);
}
getScene();

const sceneForm = ref();
const updatingScene = ref(false);
async function updateScene() {
  if (!scene.value) return;
  if (!(await sceneForm.value.validate())) return;

  updatingScene.value = true;

  const sceneRequest: UpdateSceneData = {
    name: scene.value.name,
    isEnabled: scene.value.isEnabled,
    condition: JSON.stringify(scene.value.condition),
    actions: scene.value.actions,
    cooldownAfterTriggerTime: scene.value.cooldownAfterTriggerTime,
  };

  const recipeResponse = await SceneService.updateScene(sceneId, sceneRequest);

  if (recipeResponse.error) {
    updatingScene.value = false;
    handleError(recipeResponse.error, 'Updating scene failed');
    return;
  }

  toast.success(t('Scene updated successfully'));
  router.push('/scenes');
}
</script>
