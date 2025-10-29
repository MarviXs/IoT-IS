<template>
  <q-select
    v-model="selected"
    :label="selectLabel"
    :options="options"
    :loading="isLoading"
    use-input
    clearable
    option-label="email"
    option-value="id"
    :input-debounce="400"
    @filter="filterFn"
    @virtual-scroll="onScroll"
  />
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { QSelect } from 'quasar';
import UserManagementService from '@/api/services/UserManagementService';
import { handleError } from '@/utils/error-handler';
import type { UsersQueryParams, UsersResponse } from '@/api/services/UserManagementService';

export interface UserSelectOption {
  id: string;
  email: string;
}

const props = defineProps({
  label: {
    type: String,
    required: false,
    default: '',
  },
});

const { t } = useI18n();

const selected = defineModel<UserSelectOption | null>({ required: false, default: null });

type UsersList = NonNullable<UsersResponse['items']>;
const items = ref<UsersList>([]);
const isLoading = ref(false);
const filter = ref('');
const nextPage = ref(1);
const lastPage = ref(1);

const selectLabel = computed(() => props.label || t('device.new_owner'));

const options = computed(() => {
  return items.value.map((user) => ({
    id: user.id,
    email: user.email,
  }));
});

watch(
  selected,
  (value) => {
    if (!value) return;

    const exists = items.value.some((user) => user.id === value.id);
    if (exists) return;

    items.value = [
      {
        id: value.id,
        email: value.email,
        registrationDate: new Date(0).toISOString(),
        roles: [],
      },
      ...items.value,
    ];
  },
  { immediate: true },
);

async function onScroll({ to, ref }: { to: number; ref: QSelect | null }) {
  const lastIndex = items.value.length - 1;

  if (isLoading.value || nextPage.value > lastPage.value || lastIndex !== to) return;

  const queryParams: UsersQueryParams = {
    SortBy: 'email',
    Descending: false,
    SearchTerm: filter.value,
    PageNumber: nextPage.value,
    PageSize: 50,
  };

  isLoading.value = true;
  const { data, error } = await UserManagementService.getUsers(queryParams);
  isLoading.value = false;

  if (error) {
    handleError(error, 'Loading users failed');
    return;
  }

  if (data?.items) {
    items.value = [...items.value, ...data.items];
  }

  nextPage.value++;
  lastPage.value = data?.totalPages ?? 1;

  if (ref) {
    await nextTick();
    ref.refresh();
  }
}
onScroll({ to: -1, ref: null });

async function filterFn(
  val: string,
  doneFn: (callbackFn: () => void, afterFn?: ((ref: QSelect) => void)  ) => void,
) {
  if (val === filter.value) {
    doneFn(() => {});
    return;
  }

  filter.value = val;
  nextPage.value = 1;
  lastPage.value = 1;
  items.value = [];
  await onScroll({ to: -1, ref: null });
  doneFn(() => {});
}
</script>
