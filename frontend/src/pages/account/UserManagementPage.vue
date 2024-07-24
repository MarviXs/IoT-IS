<template>
  <PageLayout :title="t('global.user_management')">
    <template #actions>
      <SearchBar v-model="filter" />
    </template>
    <template #default>
      <q-table :rows="filteredUsers" :columns="columns" :loading="isLoading" flat :rows-per-page-options="[10, 20, 50]">
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiAccountGroup" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>
        <template #body-cell-actions="props">
          <q-td auto-width :props="props">
            <q-btn :to="`/user-management/${props.row.uid}`" :icon="mdiPencil" color="grey-color" flat round
              ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.edit') }}
              </q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { GrantedAuthority } from '@/models/User';
import AuthService from '@/services/AuthService';
import { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { format } from 'date-fns';
import { mdiAccountGroup, mdiPencil } from '@quasar/extras/mdi-v6';
import PageLayout from '@/layouts/PageLayout.vue';
import { useAsyncData } from '@/composables/useAsyncData';
import SearchBar from '@/components/core/SearchBar.vue';

const { t } = useI18n();

const { data, isLoading } = useAsyncData(() => AuthService.getUsers(), t('account.toasts.get_user_failed', 2));
const filter = ref('');

const filteredUsers = computed(() => {
  if (!filter.value) {
    return data.value;
  }
  return (
    data.value?.filter(
      (user) =>
        user.name?.toLowerCase().includes(filter.value.toLowerCase()) ||
        user.mail?.toLowerCase().includes(filter.value.toLowerCase()),
    ) ?? []
  );
});

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'Username',
    label: t('account.username'),
    field: 'name',
    sortable: true,
    align: 'left',
  },
  {
    name: 'email',
    label: 'E-mail',
    field: 'mail',
    sortable: true,
    align: 'left',
  },
  {
    name: 'Created on',
    label: t('global.created_on'),
    field: 'createdAt',
    sortable: true,
    align: 'left',
    format(val) {
      return format(new Date(val), 'dd. MM. yyyy');
    },
  },
  {
    name: 'access',
    label: t('account.access'),
    field: 'authorities',
    align: 'center',
    sortable: false,
    format(val) {
      if (!val) return t('account.role.user');

      const authorities = val.map((authority: GrantedAuthority) => authority.authority);
      const roles: string[] = [];
      if (authorities.includes('admin')) {
        roles.push(t('account.role.admin'));
      }
      if (authorities.includes('user')) {
        roles.push(t('account.role.user'));
      }
      return roles.join(', ');
    },
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
