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
import { useRoute, useRouter } from 'vue-router';
import SceneForm from '@/components/scenes/SceneForm.vue';
import { Scene } from '@/models/Scene';
import { ref } from 'vue';
import SceneService, { CreateSceneData } from '@/api/services/SceneService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();

const scene = ref<Scene>({
  name: '',
  isEnabled: true,
  triggerType: 'conditional',
  condition: { and: [] },
  actions: [],
});

const sceneForm = ref();
const creatingScene = ref(false);
async function createScene() {
  if (!(await sceneForm.value.validate())) return;

  creatingScene.value = true;

  const sceneRequest: CreateSceneData = {
    name: scene.value.name,
    isEnabled: scene.value.isEnabled,
    condition: scene.value.condition.toString(),
    actions: scene.value.actions,
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
