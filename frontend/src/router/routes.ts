import { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        redirect: '/devices',
      },
      {
        path: '/account',
        component: () => import('pages/account/AccountTabsPage.vue'),
        children: [
          {
            path: '',
            component: () => import('pages/account/AccountEditPage.vue'),
          },
        ],
      },

      {
        path: 'user-management',
        component: () => import('pages/account/UserManagementPage.vue'),
        meta: { requiresAdmin: true },
      },
      {
        path: 'user-management/:id',
        component: () => import('pages/account/UserManagementEditPage.vue'),
        meta: { requiresAdmin: true },
      },

      {
        path: '/devices',
        component: () => import('pages/devices/DeviceListPage.vue'),
      },
      {
        path: '/devices/:id',
        component: () => import('pages/devices/DeviceDetailPage.vue'),
      },
      {
        path: '/devices/:id/jobs',
        component: () => import('pages/jobs/JobsOnDevicePage.vue'),
      },

      {
        path: '/device-templates',
        component: () => import('pages/device-templates/DeviceTemplateListPage.vue'),
      },
      {
        path: '/device-templates/create',
        component: () => import('pages/device-templates/CreateDeviceTemplatePage.vue'),
      },
      {
        path: '/device-templates/:id',
        component: () => import('pages/device-templates/UpdateDeviceTemplateTabsPage.vue'),
        children: [
          {
            path: '',
            component: () => import('pages/device-templates/UpdateDeviceTemplatePage.vue'),
          },
          {
            path: 'commands',
            component: () => import('pages/commands/CommandsPage.vue'),
          },
          {
            path: 'recipes',
            component: () => import('pages/recipes/RecipesPage.vue'),
          },
        ],
      },

      {
        path: '/device-templates/:id/recipes/create',
        component: () => import('pages/recipes/CreateRecipePage.vue'),
      },
      {
        path: '/device-templates/:templateId/recipes/:recipeId/edit',
        component: () => import('pages/recipes/UpdateRecipePage.vue'),
      },

      {
        path: '/jobs',
        component: () => import('pages/jobs/AllJobsPage.vue'),
      },
      {
        path: '/jobs/:id/',
        component: () => import('pages/jobs/JobDetailPage.vue'),
      },

      {
        path: '/collections',
        component: () => import('pages/collections/CollectionsPage.vue'),
      },
      {
        path: '/collections/:id/',
        component: () => import('pages/collections/CollectionDetailPage.vue'),
      },
      {
        path: '/scenes',
        component: () => import('pages/scenes/SceneListPage.vue'),
      },
      {
        path: '/scenes/create',
        component: () => import('pages/scenes/CreateScenePage.vue'),
      },
      {
        path: '/products',
        component: () => import('pages/products/AllProductsPage.vue'),
      },
    ],
  },
  {
    path: '/login',
    component: () => import('pages/auth/LoginPage.vue'),
  },
  {
    path: '/register',
    component: () => import('pages/auth/RegisterPage.vue'),
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
];

export default routes;
