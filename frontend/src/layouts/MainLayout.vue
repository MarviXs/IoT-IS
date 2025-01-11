<template>
  <q-layout view="lHh LpR lfr">
    <q-header class="bg-white text-secondary shadow">
      <q-toolbar>
        <q-btn flat dense round :icon="mdiMenu" aria-label="Menu" @click="toggleLeftDrawer" />
        <q-space />
        <language-select class="q-mr-md"></language-select>
        <q-btn class="q-mr-xs" flat dense size="18px" round padding="4px" :icon="mdiAccountCircle" :ripple="false">
          <q-menu>
            <q-list style="min-width: 150px" class="text-secondary">
              <q-item>
                <q-item-section class="q-py-xs">
                  <div class="text-weight-medium">
                    {{ authStore.user?.email }}
                  </div>
                </q-item-section>
              </q-item>
              <q-separator />
              <q-item clickable to="/account">
                <div class="row items-center q-gutter-sm">
                  <q-icon size="24px" :name="mdiAccountOutline" />
                  <div>{{ t('global.account') }}</div>
                </div>
              </q-item>
              <!-- <q-item clickable to="/settings">
                <div class="row items-center q-gutter-sm">
                  <q-icon size="24px" name="mdi-cog-outline" />
                  <div>Settings</div>
                </div>
              </q-item> -->
              <q-separator />
              <q-item clickable @click="logout()">
                <div class="row items-center q-gutter-sm">
                  <q-icon size="24px" :name="mdiLogout" />
                  <div>{{ t('account.logout') }}</div>
                </div>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
      </q-toolbar>
    </q-header>

    <q-drawer v-model="leftDrawerOpen" show-if-above class="shadow bg-white">
      <div class="column q-px-lg">
        <router-link class="q-my-lg q-mx-auto full-width" to="/">
          <q-img src="../assets/logo.png" height="3.7rem" fit="contain" no-spinner no-transition />
        </router-link>
        <div class="links">
          <!-- <side-menu-button
            to="/notifications"
            :exact="true"
            :label="t('notification.label', 2)"
            :icon="mdiBellRingOutline"
          /> -->
          <side-menu-button to="/device-templates" :label="t('device_template.label', 2)" :icon="mdiContentCopy" />
          <side-menu-button to="/devices" :label="t('device.label', 2)" :icon="mdiMemory" />
          <side-menu-button to="/collections" :label="t('collection.label', 2)" :icon="mdiFileTreeOutline" />
          <side-menu-button to="/jobs" :label="t('job.label', 2)" :icon="mdiListStatus" />
          <side-menu-button to="/scenes" :label="t('scene.label', 2)" :icon="mdiCubeOutline" />
          <side-menu-button to="/products" :label="t('product.label', 2)" :icon="mdiWrench" />
          <side-menu-button to="/companies" :label="t('company.label', 2)" :icon="mdiDomain" />
          <side-menu-button
            v-if="authStore.isAdmin"
            to="/user-management"
            :label="t('global.user_management')"
            :icon="mdiAccountGroup"
          />
        </div>
      </div>
    </q-drawer>
    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import SideMenuButton from '@/components/core/SideMenuButton.vue';
import LanguageSelect from '@/components/core/LanguageSelect.vue';
import { useAuthStore } from '@/stores/auth-store';
import { useI18n } from 'vue-i18n';
import {
  mdiMenu,
  mdiAccountCircle,
  mdiLogout,
  mdiAccountOutline,
  mdiListStatus,
  mdiAccountGroup,
  mdiContentCopy,
  mdiMemory,
  mdiFileTreeOutline,
  mdiCubeOutline,
  mdiWrench,
  mdiDomain,
} from '@quasar/extras/mdi-v7';
import { toast } from 'vue3-toastify';

const { t } = useI18n();

const authStore = useAuthStore();
const leftDrawerOpen = ref(false);

function logout() {
  authStore.logout();
  toast.success(t('auth.toasts.logout_success'));
}

function toggleLeftDrawer() {
  leftDrawerOpen.value = !leftDrawerOpen.value;
}
</script>
