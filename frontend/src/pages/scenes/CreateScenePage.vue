<template>
  <PageLayout
    :breadcrumbs="[{ label: t('scene.label', 2), to: '/scenes' }, { label: t('global.create') }]"
    :title="t('global.create')"
    :previous-title="t('scene.label', 2)"
    previous-route="/scenes"
  >
    <template #actions>
      <q-btn unelevated color="primary" :label="t('global.save')" padding="7px 35px" no-caps type="submit" />
    </template>
    <SceneForm v-model="scene" />
  </PageLayout>
</template>

<script setup lang="ts">
import PageLayout from '@/layouts/PageLayout.vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import SceneForm from '@/components/scenes/SceneForm.vue';
import { Scene } from '@/models/Scene';
import { ref } from 'vue';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();

const scene = ref<Scene>({
  name: '',
  isActive: true,
  triggerType: 'conditional',
  condition: { if: [{ and: [] }, 'NO_ACTION', 'NO_ACTION'] },
});
</script>
