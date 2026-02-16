<template>
  <q-page padding>
    <div class="column q-gutter-md">
      <q-card flat class="shadow">
        <q-card-section class="row items-center justify-between">
          <div>
            <div class="text-h6">{{ t('system.version.label') }}</div>
            <div class="text-subtitle2 text-secondary">1.1</div>
          </div>
          <q-btn flat color="primary" label="Changelog" @click="isChangelogOpen = true" />
        </q-card-section>
      </q-card>
      <q-card v-if="authStore.isAdmin" flat class="shadow">
        <q-card-section class="row items-center justify-between">
          <div>
            <div class="text-h6">{{ t('system.node_settings.title') }}</div>
            <div class="text-subtitle2 text-secondary">{{ t('system.node_settings.subtitle') }}</div>
          </div>
          <q-btn
            flat
            color="primary"
            :label="t('global.refresh')"
            :loading="isNodeSettingsLoading"
            :disable="isNodeSettingsLoading || isNodeSettingsSaving"
            @click="loadNodeSettings"
          />
        </q-card-section>
        <q-separator inset />
        <q-card-section class="column q-gutter-md">
          <div class="node-form-layout">
            <div class="row items-end q-col-gutter-sm">
              <div class="col">
                <q-select
                  v-model="nodeType"
                  :options="nodeTypeOptions"
                  emit-value
                  map-options
                  outlined
                  dense
                  :label="t('system.node_settings.mode_label')"
                  :disable="isNodeSettingsLoading || isNodeSettingsSaving"
                />
              </div>
            </div>
          </div>

          <q-separator />

          <template v-if="isHubNodeMode">
            <div class="row items-center justify-between">
              <div class="text-subtitle1">{{ t('system.node_settings.edge_nodes_title') }}</div>
              <q-btn
                flat
                color="primary"
                :icon="mdiPlus"
                :label="t('system.node_settings.add_edge_node')"
                :disable="isNodeSettingsLoading || isEdgeNodeSubmitting"
                @click="openCreateEdgeNodeDialog"
              />
            </div>

            <div v-if="isNodeSettingsLoading" class="text-secondary">{{ t('system.node_settings.loading') }}</div>

            <q-list v-else-if="edgeNodes.length > 0" class="edge-node-list">
              <q-item v-for="edgeNode in edgeNodes" :key="edgeNode.id" class="edge-node-item">
                <q-item-section class="edge-node-main">
                  <q-item-label class="edge-node-name">{{ edgeNode.name }}</q-item-label>
                  <div class="edge-node-token-row">
                    <div class="edge-node-token">
                      <span class="edge-node-token-label">{{ t('system.node_settings.token_label') }}</span>
                      <span class="token-text">
                        {{ isEdgeNodeTokenVisible(edgeNode.id) ? edgeNode.token : maskToken(edgeNode.token) }}
                      </span>
                    </div>
                    <q-btn
                      v-if="edgeNode.token"
                      color="secondary"
                      flat
                      round
                      dense
                      :icon="mdiEye"
                      @click="toggleEdgeNodeTokenVisibility(edgeNode.id)"
                    />
                  </div>
                </q-item-section>
                <q-item-section side class="edge-node-actions">
                  <div class="row items-center q-gutter-xs">
                    <q-btn
                      flat
                      round
                      dense
                      color="primary"
                      :icon="mdiPencil"
                      :aria-label="t('global.edit')"
                      @click="openEditEdgeNodeDialog(edgeNode)"
                    />
                    <q-btn
                      flat
                      round
                      dense
                      color="negative"
                      :icon="mdiDelete"
                      :aria-label="t('global.delete')"
                      @click="openDeleteEdgeNodeDialog(edgeNode)"
                    />
                  </div>
                </q-item-section>
              </q-item>
            </q-list>

            <div v-else class="text-secondary">{{ t('system.node_settings.no_edge_nodes') }}</div>
          </template>

          <template v-else>
            <div class="column q-gutter-sm">
              <div class="text-subtitle1">{{ t('system.node_settings.hub_connection_title') }}</div>
              <q-input
                v-model="hubUrl"
                outlined
                dense
                :label="t('system.node_settings.hub_url_label')"
                :disable="isNodeSettingsLoading || isNodeSettingsSaving"
              />
              <q-input
                v-model="hubToken"
                outlined
                dense
                :label="t('system.node_settings.hub_token_label')"
                :disable="isNodeSettingsLoading || isNodeSettingsSaving"
              />
              <div class="row justify-end">
                <q-btn
                  color="primary"
                  :label="t('global.save')"
                  :loading="isNodeSettingsSaving"
                  :disable="isNodeSettingsLoading || isNodeSettingsSaving"
                  no-caps
                  unelevated
                  padding="8px 20px"
                  @click="saveNodeSettings()"
                />
              </div>
            </div>
          </template>
        </q-card-section>
      </q-card>
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

    <SystemChangelogDialog v-model="isChangelogOpen" />

    <q-dialog v-model="isEdgeNodeDialogOpen">
      <q-card style="min-width: 420px; max-width: 90vw">
        <q-card-section>
          <div class="text-h6">
            {{ isEdgeNodeEditing ? t('system.node_settings.edit_edge_node') : t('system.node_settings.add_edge_node') }}
          </div>
        </q-card-section>
        <q-card-section class="column q-gutter-md">
          <q-input v-model="edgeNodeForm.name" :label="t('global.name')" />
          <q-input
            v-model="edgeNodeForm.token"
            :label="t('system.node_settings.token_label')"
            :hint="isEdgeNodeEditing ? '' : t('system.node_settings.token_hint')"
            persistent-hint
          >
            <template #append>
              <q-icon
                v-if="!edgeNodeForm.token"
                :name="mdiAutorenew"
                class="cursor-pointer"
                @click="generateEdgeNodeToken"
              >
                <q-tooltip>{{ t('system.node_settings.generate_token') }}</q-tooltip>
              </q-icon>
              <q-icon v-else :name="mdiContentCopy" class="cursor-pointer" @click="copyEdgeNodeToken">
                <q-tooltip>
                  {{
                    isEdgeNodeTokenCopied
                      ? t('system.node_settings.token_copied')
                      : t('system.node_settings.copy_token')
                  }}
                </q-tooltip>
              </q-icon>
            </template>
          </q-input>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn
            flat
            color="primary"
            :label="t('global.cancel')"
            :disable="isEdgeNodeSubmitting"
            no-caps
            @click="isEdgeNodeDialogOpen = false"
          />
          <q-btn
            color="primary"
            :label="t('global.save')"
            :loading="isEdgeNodeSubmitting"
            no-caps
            @click="submitEdgeNodeDialog"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <q-dialog v-model="isDeleteEdgeNodeDialogOpen">
      <q-card style="min-width: 360px; max-width: 90vw">
        <q-card-section>
          <div class="text-h6">{{ t('system.node_settings.delete_edge_node') }}</div>
        </q-card-section>
        <q-card-section>{{ t('system.node_settings.delete_edge_node_confirm') }}</q-card-section>
        <q-card-actions align="right">
          <q-btn
            flat
            color="primary"
            :label="t('global.cancel')"
            :disable="isDeletingEdgeNode"
            no-caps
            @click="isDeleteEdgeNodeDialogOpen = false"
          />
          <q-btn
            color="negative"
            :label="t('global.delete')"
            :loading="isDeletingEdgeNode"
            no-caps
            @click="confirmDeleteEdgeNode"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import {
  mdiAutorenew,
  mdiContentCopy,
  mdiDatabase,
  mdiDelete,
  mdiEye,
  mdiPencil,
  mdiPlus,
  mdiRefresh,
} from '@quasar/extras/mdi-v7';
import SystemService, { type EdgeNodeResponse, type SystemNodeType } from '@/api/services/SystemService';
import { copyToClipboard } from 'quasar';
import SystemChangelogDialog from '@/components/system/SystemChangelogDialog.vue';
import { useAuthStore } from '@/stores/auth-store';
import { toast } from 'vue3-toastify';

const { t } = useI18n();
const authStore = useAuthStore();

const totalSizeBytes = ref<number | null>(null);
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);
const isVacuuming = ref(false);
const isChangelogOpen = ref(false);
const isNodeSettingsLoading = ref(false);
const isNodeSettingsSaving = ref(false);
const nodeType = ref<SystemNodeType>(0);
const hubUrl = ref('');
const hubToken = ref('');
const edgeNodes = ref<EdgeNodeResponse[]>([]);
const isEdgeNodeDialogOpen = ref(false);
const isDeleteEdgeNodeDialogOpen = ref(false);
const isEdgeNodeSubmitting = ref(false);
const isDeletingEdgeNode = ref(false);
const deletingEdgeNodeId = ref<string | null>(null);
const editingEdgeNodeId = ref<string | null>(null);
const isEdgeNodeTokenCopied = ref(false);
const edgeNodeTokenVisibility = ref<Record<string, boolean>>({});
const edgeNodeForm = ref({
  name: '',
  token: '',
});

const nodeTypeOptions = computed<{ label: string; value: SystemNodeType }[]>(() => [
  { label: t('system.node_settings.node_types.hub'), value: 0 },
  { label: t('system.node_settings.node_types.edge'), value: 1 },
]);

const isEdgeNodeEditing = computed(() => editingEdgeNodeId.value !== null);
const isHubNodeMode = computed(() => nodeType.value === 0);
const isUpdatingNodeTypeFromServer = ref(false);
const loadedNodeType = ref<SystemNodeType | null>(null);

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
    toast.success(t('system.storage.vacuum_success'));
    await loadStats();
  } catch (error) {
    console.error('Failed to run VACUUM on TimescaleDB datapoints table', error);
    toast.error(t('system.storage.vacuum_error'));
  } finally {
    isVacuuming.value = false;
  }
}

async function loadNodeSettings() {
  if (!authStore.isAdmin) {
    return;
  }

  isNodeSettingsLoading.value = true;
  isUpdatingNodeTypeFromServer.value = true;

  try {
    const response = await SystemService.getNodeSettings();
    const normalizedNodeType = normalizeNodeType(response.nodeType as unknown);
    loadedNodeType.value = normalizedNodeType;
    nodeType.value = normalizedNodeType;
    hubUrl.value = response.hubUrl ?? '';
    hubToken.value = response.hubToken ?? '';
    edgeNodeTokenVisibility.value = {};
    edgeNodes.value = response.edgeNodes;
  } catch (error) {
    console.error('Failed to load node settings', error);
    toast.error(t('system.node_settings.load_error'));
  } finally {
    isUpdatingNodeTypeFromServer.value = false;
    isNodeSettingsLoading.value = false;
  }
}

async function saveNodeSettings(options: { showSuccessToast?: boolean } = {}): Promise<boolean> {
  const { showSuccessToast = true } = options;
  const trimmedHubUrl = hubUrl.value.trim();
  const trimmedHubToken = hubToken.value.trim();

  if (!isHubNodeMode.value && (!trimmedHubUrl || !trimmedHubToken)) {
    toast.error(t('system.node_settings.hub_connection_required'));
    return false;
  }

  isNodeSettingsSaving.value = true;

  try {
    const normalizedHubUrl = normalizeHubUrl(trimmedHubUrl);
    if (!isHubNodeMode.value) {
      hubUrl.value = normalizedHubUrl;
    }

    await SystemService.updateNodeSettings({
      nodeType: nodeType.value,
      hubUrl: isHubNodeMode.value ? null : normalizedHubUrl,
      hubToken: isHubNodeMode.value ? null : trimmedHubToken,
    });
    if (showSuccessToast) {
      toast.success(t('system.node_settings.save_success'));
    }
    loadedNodeType.value = nodeType.value;
    return true;
  } catch (error) {
    console.error('Failed to save node settings', error);
    toast.error(t('system.node_settings.save_error'));
    return false;
  } finally {
    isNodeSettingsSaving.value = false;
  }
}

function openCreateEdgeNodeDialog() {
  editingEdgeNodeId.value = null;
  edgeNodeForm.value = { name: '', token: generateToken() };
  isEdgeNodeTokenCopied.value = false;
  isEdgeNodeDialogOpen.value = true;
}

function openEditEdgeNodeDialog(edgeNode: EdgeNodeResponse) {
  editingEdgeNodeId.value = edgeNode.id;
  edgeNodeForm.value = { name: edgeNode.name, token: edgeNode.token };
  isEdgeNodeTokenCopied.value = false;
  isEdgeNodeDialogOpen.value = true;
}

function openDeleteEdgeNodeDialog(edgeNode: EdgeNodeResponse) {
  deletingEdgeNodeId.value = edgeNode.id;
  isDeleteEdgeNodeDialogOpen.value = true;
}

async function submitEdgeNodeDialog() {
  const trimmedName = edgeNodeForm.value.name.trim();
  const trimmedToken = edgeNodeForm.value.token.trim();

  if (!trimmedName) {
    toast.error(t('system.node_settings.name_required'));
    return;
  }

  if (isEdgeNodeEditing.value && !trimmedToken) {
    toast.error(t('system.node_settings.token_required'));
    return;
  }

  isEdgeNodeSubmitting.value = true;

  try {
    if (editingEdgeNodeId.value) {
      await SystemService.updateEdgeNode(editingEdgeNodeId.value, {
        name: trimmedName,
        token: trimmedToken,
      });
      toast.success(t('system.node_settings.update_edge_success'));
    } else {
      await SystemService.createEdgeNode({
        name: trimmedName,
        token: trimmedToken || undefined,
      });
      toast.success(t('system.node_settings.create_edge_success'));
    }

    isEdgeNodeDialogOpen.value = false;
    await loadNodeSettings();
  } catch (error) {
    console.error('Failed to submit edge node', error);
    toast.error(t('system.node_settings.save_edge_error'));
  } finally {
    isEdgeNodeSubmitting.value = false;
  }
}

function generateToken(): string {
  const charset = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  const tokenLength = 24;
  const randomValues = new Uint8Array(tokenLength);
  window.crypto.getRandomValues(randomValues);
  let token = '';
  for (let i = 0; i < tokenLength; i++) {
    token += charset[randomValues[i] % charset.length];
  }
  return token;
}

function normalizeHubUrl(value: string): string {
  const trimmed = value.trim();
  if (!trimmed) {
    return trimmed;
  }

  if (/^[a-zA-Z][a-zA-Z\d+\-.]*:\/\//.test(trimmed)) {
    return trimmed;
  }

  return `https://${trimmed}`;
}

function normalizeNodeType(value: unknown): SystemNodeType {
  if (value === 0 || value === '0' || value === 'Hub' || value === 'hub') {
    return 0;
  }

  if (value === 1 || value === '1' || value === 'Edge' || value === 'edge') {
    return 1;
  }

  return 0;
}

function maskToken(token: string): string {
  return token ? '●'.repeat(token.length) : '-';
}

function isEdgeNodeTokenVisible(edgeNodeId: string): boolean {
  return !!edgeNodeTokenVisibility.value[edgeNodeId];
}

function toggleEdgeNodeTokenVisibility(edgeNodeId: string) {
  edgeNodeTokenVisibility.value = {
    ...edgeNodeTokenVisibility.value,
    [edgeNodeId]: !edgeNodeTokenVisibility.value[edgeNodeId],
  };
}

function generateEdgeNodeToken() {
  edgeNodeForm.value.token = generateToken();
  isEdgeNodeTokenCopied.value = false;
}

function copyEdgeNodeToken() {
  copyToClipboard(edgeNodeForm.value.token || '');
  isEdgeNodeTokenCopied.value = true;
  setTimeout(() => {
    isEdgeNodeTokenCopied.value = false;
  }, 2000);
}

async function confirmDeleteEdgeNode() {
  if (!deletingEdgeNodeId.value) {
    return;
  }

  isDeletingEdgeNode.value = true;

  try {
    await SystemService.deleteEdgeNode(deletingEdgeNodeId.value);
    toast.success(t('system.node_settings.delete_edge_success'));
    isDeleteEdgeNodeDialogOpen.value = false;
    deletingEdgeNodeId.value = null;
    await loadNodeSettings();
  } catch (error) {
    console.error('Failed to delete edge node', error);
    toast.error(t('system.node_settings.delete_edge_error'));
  } finally {
    isDeletingEdgeNode.value = false;
  }
}

onMounted(() => {
  void loadStats();
  void loadNodeSettings();
});

watch(nodeType, async (nextType, previousType) => {
  if (nextType === previousType || isNodeSettingsLoading.value || isUpdatingNodeTypeFromServer.value) {
    return;
  }

  if (loadedNodeType.value === nextType) {
    return;
  }

  const trimmedHubUrl = hubUrl.value.trim();
  const trimmedHubToken = hubToken.value.trim();
  const isSwitchingToEdgeWithoutHubConnection = nextType === 1 && (!trimmedHubUrl || !trimmedHubToken);
  if (isSwitchingToEdgeWithoutHubConnection) {
    return;
  }

  await saveNodeSettings({ showSuccessToast: false });
});
</script>

<style scoped>
.token-text {
  font-family: monospace;
  font-size: 12px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  display: block;
}

.node-form-layout {
  width: 100%;
  max-width: 760px;
}

.edge-node-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.edge-node-item {
  border: 1px solid #d6dae3;
  border-radius: 12px;
  background: #ffffff;
  padding: 12px 14px;
  align-items: center;
}

.edge-node-main {
  min-width: 0;
}

.edge-node-name {
  font-size: 15px;
  font-weight: 400;
  margin-bottom: 8px;
}

.edge-node-token-row {
  display: flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
}

.edge-node-token {
  display: flex;
  align-items: center;
  gap: 8px;
  max-width: 100%;
  min-width: 0;
  padding: 6px 10px;
  border-radius: 8px;
  border: 1px solid #e2e6ef;
  background: #f4f6fb;
}

.edge-node-token-label {
  font-size: 11px;
  font-weight: 600;
  color: #6a7281;
  letter-spacing: 0.03em;
  text-transform: uppercase;
  white-space: nowrap;
}

.edge-node-actions {
  padding-left: 10px;
}
</style>
