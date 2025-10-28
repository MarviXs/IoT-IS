<template>
  <PageLayout :breadcrumbs="[{ label: t('scene.label', 2), to: '/scenes' }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <q-btn
        v-if="!isMobile"
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('scene.create_scene')"
        :icon="mdiPlus"
        to="/scenes/create"
      />
    </template>
    <template #default>
      <q-pull-to-refresh v-if="isMobile" @refresh="onPullToRefresh">
        <ScenesTable
          v-model:pagination="pagination"
          :scenes="scenesPaginated"
          :loading="loadingScenes"
          :mobile-scenes="mobileScenes"
          :has-more="hasMoreScenes"
          :reset-key="mobileResetKey"
          @on-request="onRequest"
          @on-load-more="loadMoreScenes"
          @on-enable="onEnableScene"
          @on-delete="openDeleteDialog"
        />
      </q-pull-to-refresh>
      <ScenesTable
        v-else
        v-model:pagination="pagination"
        :scenes="scenesPaginated"
        :loading="loadingScenes"
        @on-request="onRequest"
        @on-enable="onEnableScene"
        @on-delete="openDeleteDialog"
      />
    </template>
  </PageLayout>
  <DeleteSceneDialog v-model="deleteDialogOpen" :scene-id="deleteSceneId" @onDeleted="getScenes(pagination)" />
  <q-page-sticky v-if="isMobile" position="bottom-right" :offset="[18, 18]">
    <q-btn fab color="primary" :icon="mdiPlus" to="/scenes/create" />
  </q-page-sticky>
</template>

<script setup lang="ts">
import ScenesTable from '@/components/scenes/ScenesTable.vue';
import SearchBar from '@/components/core/SearchBar.vue';
import { useI18n } from 'vue-i18n';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { computed, ref } from 'vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import { handleError } from '@/utils/error-handler';
import { watchDebounced } from '@vueuse/core';
import type { ScenesPaginatedResponse, ScenesQueryParams } from '@/api/services/SceneService';
import SceneService from '@/api/services/SceneService';
import DeleteSceneDialog from '@/components/scenes/DeleteSceneDialog.vue';
import { useQuasar } from 'quasar';

const { t } = useI18n();
const filter = ref('');

const $q = useQuasar();
const isMobile = computed(() => $q.platform.is.mobile);

const pagination = ref<PaginationClient>({
  sortBy: 'lastTriggeredAt',
  descending: true,
  page: 1,
  rowsPerPage: 20,
  rowsNumber: 0,
});

type SceneList = NonNullable<ScenesPaginatedResponse['items']>;

const scenesPaginated = ref<ScenesPaginatedResponse>();
const loadingScenes = ref(false);
const deleteDialogOpen = ref(false);
const deleteSceneId = ref<string>('');

const mobileScenes = ref<SceneList>([] as SceneList);
const totalScenes = ref(0);
const mobileResetKey = ref(0);

const hasMoreScenes = computed(() => mobileScenes.value.length < totalScenes.value);

async function onRequest(props: { pagination: PaginationClient }) {
  await getScenes(props.pagination);
}

async function getScenes(paginationTable: PaginationTable, options: { append?: boolean } = {}): Promise<boolean> {
  const { append = false } = options;
  const paginationQuery: ScenesQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  loadingScenes.value = true;
  const { data, error } = await SceneService.getScenes(paginationQuery);
  loadingScenes.value = false;

  if (error) {
    handleError(error, 'Loading scenes failed');
    return false;
  }

  if (!data) {
    return false;
  }

  scenesPaginated.value = data;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
  pagination.value.rowsNumber = data.totalCount ?? 0;

  totalScenes.value = data.totalCount ?? 0;

  const items = data.items ?? [];

  if (append) {
    mobileScenes.value = [...mobileScenes.value, ...items];
  } else {
    mobileScenes.value = items;
    mobileResetKey.value++;
  }

  return true;
}
getScenes(pagination.value);

async function onPullToRefresh(done: () => void) {
  try {
    pagination.value.page = 1;
    await getScenes(pagination.value);
  } finally {
    done();
  }
}

async function loadMoreScenes(done: (stop?: boolean) => void) {
  if (loadingScenes.value) {
    done();
    return;
  }

  if (!hasMoreScenes.value) {
    done(true);
    return;
  }

  const nextPage: PaginationClient = {
    ...pagination.value,
    page: pagination.value.page + 1,
  };

  const success = await getScenes(nextPage, { append: true });

  if (!success) {
    done();
    return;
  }

  if (!hasMoreScenes.value) {
    done(true);
    return;
  }

  done();
}

function openDeleteDialog(id: string) {
  deleteSceneId.value = id;
  deleteDialogOpen.value = true;
}

function getSceneState(sceneId: string) {
  const desktopScene = scenesPaginated.value?.items?.find((scene) => scene.id === sceneId);
  if (desktopScene) {
    return desktopScene.isEnabled;
  }

  const mobileScene = mobileScenes.value.find((scene) => scene.id === sceneId);
  return mobileScene?.isEnabled;
}

function setSceneState(sceneId: string, isEnabled: boolean) {
  const desktopScene = scenesPaginated.value?.items?.find((scene) => scene.id === sceneId);
  if (desktopScene) {
    desktopScene.isEnabled = isEnabled;
  }

  const mobileScene = mobileScenes.value.find((scene) => scene.id === sceneId);
  if (mobileScene) {
    mobileScene.isEnabled = isEnabled;
  }
}

async function onEnableScene(id: string, isEnabled: boolean) {
  const previousState = getSceneState(id);
  setSceneState(id, isEnabled);

  const { error } = await SceneService.enableScene(id, isEnabled);
  if (error) {
    if (previousState !== undefined) {
      setSceneState(id, previousState);
    } else {
      setSceneState(id, !isEnabled);
    }

    const errorMessage = isEnabled ? 'Failed to enable scene' : 'Failed to disable scene';
    handleError(error, errorMessage);
  }
}

watchDebounced(
  filter,
  async () => {
    pagination.value.page = 1;
    await getScenes(pagination.value);
  },
  { debounce: 400 },
);
</script>
