<template>
  <PageLayout
    :breadcrumbs="[{ label: t('scene.label', 2), to: '/scenes' }, { label: t('global.create') }]"
    :title="t('global.create')"
    :previous-title="t('scene.label', 2)"
    previous-route="/scenes"
  >
    <template #actions>
      <q-btn
        unelevated
        class="col-12 col-md-auto"
        color="primary"
        :label="t('global.save')"
        @click="createScene"
        padding="7px 35px"
        no-caps
        type="submit"
      />
    </template>
    <SceneForm v-model="scene" ref="sceneForm" @on-submit="createScene" />
  </PageLayout>
</template>

<script setup lang="ts">
import PageLayout from '@/layouts/PageLayout.vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import SceneForm from '@/components/scenes/SceneForm.vue';
import type { Scene } from '@/models/Scene';
import { ref } from 'vue';
import type { CreateSceneData } from '@/api/services/SceneService';
import SceneService from '@/api/services/SceneService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';

const { t } = useI18n();
const router = useRouter();

const scene = ref<Scene>({
  name: '',
  description: '',
  isEnabled: true,
  condition: { and: [] },
  actions: [],
  cooldownAfterTriggerTime: 0,
});

const sceneForm = ref();
const creatingScene = ref(false);
async function createScene() {
  if (!(await sceneForm.value.validate())) return;

  creatingScene.value = true;

  const sceneRequest: CreateSceneData = {
    name: scene.value.name,
    description: scene.value.description,
    isEnabled: scene.value.isEnabled,
    condition: JSON.stringify(scene.value.condition),
    actions: scene.value.actions,
    cooldownAfterTriggerTime: scene.value.cooldownAfterTriggerTime,
  };

  const recipeResponse = await SceneService.createScene(sceneRequest);

  if (recipeResponse.error) {
    creatingScene.value = false;
    handleError(recipeResponse.error, 'Creating scene failed');
    return;
  }

  toast.success(t('Scene created successfully'));
  router.push('/scenes');
}
</script>
