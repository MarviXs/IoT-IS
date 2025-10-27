<template>
  <q-page padding>
    <div class="column q-gutter-md">
      <q-card flat class="shadow">
        <q-card-section class="row items-center justify-between">
          <div>
            <div class="text-h6">{{ t('system.storage.title') }}</div>
            <div class="text-subtitle2 text-secondary">{{ t('system.storage.subtitle') }}</div>
          </div>
          <div class="row items-center q-gutter-sm">
            <q-btn
              flat
              :label="t('system.storage.force_reclaim')"
              color="primary"
              :loading="isVacuuming"
              :disable="isVacuuming || isLoading"
              @click="forceReclaimSpace"
            />
            <q-btn
              flat
              round
              :icon="mdiRefresh"
              :loading="isLoading"
              :disable="isLoading || isVacuuming"
              @click="loadStats"
              :aria-label="t('global.refresh')"
            />
          </div>
        </q-card-section>
        <q-separator inset />
        <q-card-section>
          <div class="row items-center q-gutter-md">
            <q-icon :name="mdiDatabase" color="primary" size="3rem" />
            <div class="column">
              <div v-if="isLoading" class="text-subtitle1">{{ t('system.storage.loading') }}</div>
              <div v-else class="text-h4">{{ formattedSize }}</div>
              <div v-if="!isLoading" class="text-caption text-secondary">
                {{ t('system.storage.bytes', { value: formattedBytes }) }}
              </div>
            </div>
          </div>
          <q-banner v-if="errorMessage" class="q-mt-md" dense rounded color="negative" text-color="white">
            {{ errorMessage }}
          </q-banner>
        </q-card-section>
      </q-card>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiDatabase, mdiRefresh } from '@quasar/extras/mdi-v7';
import SystemService from '@/api/services/SystemService';
import { useQuasar } from 'quasar';

const { t } = useI18n();
const $q = useQuasar();

const totalSizeBytes = ref<number | null>(null);
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);
const isVacuuming = ref(false);

const formattedBytes = computed(() => {
  if (totalSizeBytes.value == null) {
    return '0';
  }

  return totalSizeBytes.value.toLocaleString();
});

const formattedSize = computed(() => {
  if (totalSizeBytes.value == null) {
    return t('system.storage.loading');
  }

  return formatBytes(totalSizeBytes.value);
});

function formatBytes(bytes: number): string {
  if (bytes === 0) {
    return '0 B';
  }

  const units = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];
  const exponent = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1);
  const value = bytes / Math.pow(1024, exponent);

  const precision = value >= 100 ? 0 : value >= 10 ? 1 : 2;

  return `${value.toFixed(precision)} ${units[exponent]}`;
}

async function loadStats() {
  isLoading.value = true;
  errorMessage.value = null;

  try {
    const response = await SystemService.getTimescaleStorageUsage();
    totalSizeBytes.value = response.totalColumnstoreSizeBytes;
  } catch (error) {
    console.error('Failed to load TimescaleDB storage usage', error);
    errorMessage.value = t('system.storage.load_error');
  } finally {
    isLoading.value = false;
  }
}

async function forceReclaimSpace() {
  isVacuuming.value = true;

  try {
    await SystemService.forceReclaimTimescaleSpace();
    $q.notify({ type: 'positive', message: t('system.storage.vacuum_success') });
    await loadStats();
  } catch (error) {
    console.error('Failed to run VACUUM on TimescaleDB datapoints table', error);
    $q.notify({ type: 'negative', message: t('system.storage.vacuum_error') });
  } finally {
    isVacuuming.value = false;
  }
}

onMounted(() => {
  void loadStats();
});
</script>
