<template>
  <div>
    <div v-if="isMobile">
      <q-infinite-scroll v-if="mobileItems.length" ref="infiniteScrollRef" :offset="120" @load="onLoadMore">
        <div class="scenes-grid">
          <q-card
            v-for="scene in mobileItems"
            :key="scene.id"
            class="scene-card shadow"
            v-ripple
            @click="goToScene(scene.id)"
          >
            <q-card-section class="scene-card__header">
              <div class="scene-card__name text-subtitle1 ellipsis">{{ scene.name }}</div>
              <q-toggle
                :model-value="scene.isEnabled ?? false"
                color="primary"
                @click.stop
                @update:model-value="(value) => emit('onEnable', scene.id, value)"
              />
            </q-card-section>
            <q-card-section class="scene-card__meta">
              <div class="scene-card__meta-label text-caption text-grey-7">
                {{ t('scene.last_triggered') }}
              </div>
              <div class="text-body2">
                {{ getLastTriggeredLabel(scene) }}
              </div>
            </q-card-section>

            <q-menu context-menu touch-position>
              <q-list>
                <q-item clickable v-close-popup @click.stop="goToScene(scene.id)">
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiPencil" />
                    <div>{{ t('global.edit') }}</div>
                  </div>
                </q-item>
                <q-item clickable v-close-popup @click.stop="emit('onDelete', scene.id)">
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiTrashCanOutline" />
                    <div>{{ t('global.delete') }}</div>
                  </div>
                </q-item>
              </q-list>
            </q-menu>
          </q-card>
        </div>

        <template #loading>
          <div class="row justify-center q-my-md">
            <q-spinner-dots color="primary" size="32px" />
          </div>
        </template>
      </q-infinite-scroll>
      <div v-else class="full-width column flex-center q-pa-lg nothing-found-text">
        <q-icon :name="mdiCodeTags" class="q-mb-md" size="50px"></q-icon>
        {{ props.loading ? t('table.loading_label') : t('table.no_data_label') }}
      </div>
    </div>
    <q-table
      v-else
      v-model:pagination="pagination"
      :rows="scenesFiltered"
      :columns="columns"
      :loading="props.loading"
      flat
      :rows-per-page-options="[10, 20, 50]"
      :no-data-label="t('table.no_data_label')"
      :loading-label="t('table.loading_label')"
      :rows-per-page-label="t('table.rows_per_page_label')"
      binary-state-sort
      class="shadow"
      @request="emit('onRequest', $event)"
    >
      <template #no-data="{ message }">
        <div class="full-width column flex-center q-pa-lg nothing-found-text">
          <q-icon :name="mdiCodeTags" class="q-mb-md" size="50px"></q-icon>
          {{ message }}
        </div>
      </template>

      <template #body-cell-active="propsCell">
        <q-td auto-width :props="propsCell">
          <q-toggle
            :model-value="propsCell.row.isEnabled ?? false"
            color="primary"
            @update:model-value="(value) => emit('onEnable', propsCell.row.id, value)"
          />
        </q-td>
      </template>

      <template #body-cell-name="propsCell">
        <q-td :props="propsCell">
          <router-link :to="`/scenes/${propsCell.row.id}`" class="text-weight-medium">
            {{ propsCell.row.name }}
          </router-link>
        </q-td>
      </template>

      <template #body-cell-triggeredAt="propsCell">
        <q-td auto-width :props="propsCell">
          {{ formatTimeToDistance(propsCell.row.lastTriggeredAt ?? '') || t('scene.never_triggered') }}
          <q-tooltip v-if="propsCell.row.lastTriggeredAt" content-style="font-size: 11px" :offset="[0, 4]">
            {{ new Date(propsCell.row.lastTriggeredAt).toLocaleString() }}
          </q-tooltip>
        </q-td>
      </template>

      <template #body-cell-actions="propsCell">
        <q-td auto-width :props="propsCell">
          <q-btn :icon="mdiPencil" color="grey-color" flat round :to="`/scenes/${propsCell.row.id}`">
            <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.edit') }}
            </q-tooltip>
          </q-btn>
          <q-btn :icon="mdiTrashCanOutline" color="grey-color" flat round @click="emit('onDelete', propsCell.row.id)">
            <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.delete') }}
            </q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </q-table>
  </div>
</template>

<script setup lang="ts">
import { QInfiniteScroll, useQuasar, type QTableProps } from 'quasar';
import type { PropType } from 'vue';
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiCodeTags, mdiPencil, mdiTrashCanOutline } from '@quasar/extras/mdi-v7';
import { formatTimeToDistance } from '@/utils/date-utils';
import type { PaginationClient } from '@/models/Pagination';
import { useRouter } from 'vue-router';
import type { ScenesPaginatedResponse } from '@/api/services/SceneService';

type SceneList = NonNullable<ScenesPaginatedResponse['items']>;
type SceneItem = SceneList[number];

const props = defineProps({
  scenes: {
    type: Object as PropType<ScenesPaginatedResponse>,
    required: false,
    default: null,
  },
  loading: {
    type: Boolean,
    required: true,
  },
  mobileScenes: {
    type: Array as PropType<SceneList>,
    required: false,
    default: () => [] as SceneList,
  },
  hasMore: {
    type: Boolean,
    required: false,
    default: false,
  },
  resetKey: {
    type: Number,
    required: false,
    default: 0,
  },
});

const pagination = defineModel<PaginationClient>('pagination');

const emit = defineEmits(['onRequest', 'onLoadMore', 'onEnable', 'onDelete']);

const { t } = useI18n();
const $q = useQuasar();
const router = useRouter();
const isMobile = computed(() => $q.platform.is.mobile);

const scenesFiltered = computed(() => props.scenes?.items ?? []);

const mobileItems = computed(() => {
  if (isMobile.value) {
    return props.mobileScenes;
  }

  return scenesFiltered.value;
});

const infiniteScrollRef = ref<InstanceType<typeof QInfiniteScroll>>();

watch(
  () => props.resetKey,
  () => {
    infiniteScrollRef.value?.reset();
  },
);

function onLoadMore(_index: number, done: (stop?: boolean) => void) {
  if (!props.hasMore) {
    done(true);
    return;
  }

  emit('onLoadMore', done);
}

function goToScene(sceneId: string) {
  router.push(`/scenes/${sceneId}`);
}

function getLastTriggeredLabel(scene: SceneItem) {
  if (!scene.lastTriggeredAt) {
    return t('scene.never_triggered');
  }

  return formatTimeToDistance(scene.lastTriggeredAt);
}

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'active',
    label: t('global.active'),
    field: 'isEnabled',
    sortable: true,
    align: 'center',
  },
  {
    name: 'name',
    label: t('global.name'),
    field: 'name',
    sortable: true,
    align: 'left',
  },
  {
    name: 'triggeredAt',
    label: t('scene.last_triggered'),
    field: 'lastTriggeredAt',
    sortable: true,
    align: 'right',
  },
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);
</script>

<style lang="scss" scoped>
.scenes-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.scene-card {
  cursor: pointer;
  width: 100%;

  &__header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 12px;
  }

  &__name {
    flex: 1 1 auto;
  }

  &__meta {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  &__meta-label {
    letter-spacing: 0.5px;
    text-transform: uppercase;
  }
}
</style>
