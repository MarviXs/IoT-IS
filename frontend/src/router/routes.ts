import type { RouteRecordRaw } from 'vue-router';

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
        path: '/account/document-links',
        component: () => import('pages/account/AccountDocumentLinksPage.vue'),
      },

      {
        path: '/system',
        component: () => import('pages/system/SystemStoragePage.vue'),
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
        path: '/admin/devices',
        component: () => import('pages/admin/AdminDeviceListPage.vue'),
        meta: { requiresAdmin: true },
      },
      {
        path: '/admin/device-templates',
        component: () => import('pages/admin/AdminDeviceTemplateListPage.vue'),
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
        path: '/devices/:id/jobs/compare',
        component: () => import('pages/jobs/DeviceJobsComparePage.vue'),
      },
      {
        path: '/devices/:id/schedules',
        component: () => import('pages/jobs/DeviceJobSchedulesPage.vue'),
      },
      {
        path: '/devices/:id/map',
        component: () => import('pages/devices/DeviceMapPage.vue'),
      },
      {
        path: '/devices/:id/controls',
        component: () => import('pages/devices/DeviceControlsPage.vue'),
      },
      {
        path: '/devices/:id/grid',
        component: () => import('pages/devices/DeviceGridPage.vue'),
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
          {
            path: 'firmwares',
            component: () => import('pages/device-templates/DeviceTemplateFirmwaresPage.vue'),
          },
          {
            path: 'controls',
            component: () => import('pages/device-templates/UpdateDeviceTemplateControlsPage.vue'),
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
        path: '/jobs/:id/graph',
        component: () => import('pages/jobs/JobGraphPage.vue'),
      },
      {
        path: '/experiments',
        component: () => import('pages/experiments/AllExperimentsPage.vue'),
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
        path: '/scenes/:id',
        component: () => import('pages/scenes/UpdateScenePage.vue'),
      },
      {
        path: '/notifications',
        component: () => import('pages/notifications/NotificationListPage.vue'),
      },
      {
        path: '/products',
        component: () => import('pages/products/AllProductsPage.vue'),
      },
      {
        path: '/orders',
        component: () => import('pages/orders/AllOrdersPage.vue'),
      },
      {
        path: '/orders/:id',
        component: () => import('pages/orders/OrderDetailsPage.vue'),
        name: 'OrderDetails',
      },
      {
        path: '/companies',
        component: () => import('pages/companies/AllCompaniesPage.vue'),
      },
      {
        path: '/lifecycle',
        component: () => import('pages/life-cycle/LifeIntro.vue'),
      },
      {
        path: '/lifeboard/:id/',
        component: () => import('pages/life-cycle/LifeFilterPlants.vue'),
      },
      {
        path: '/lifecycle/analyze_more/:id',
        component: () => import('pages/life-cycle/LifeMoreAnalyze.vue'),
      },
      {
        path: '/lifecycle/analyze',
        component: () => import('pages/life-cycle/LifeAnalyze.vue'),
      },
      {
        path: '/lifecycle/analyze/:id',
        component: () => import('pages/life-cycle/LifeAnalyze.vue'),
      },
      {
        path: '/lifecycle/:id/',
        component: () => import('pages/life-cycle/LifePlant.vue'),
      },
      {
        path: '/lifecycle/analyze_more/',
        component: () => import('pages/life-cycle/LifeMoreAnalyze.vue'),
      },
      {
        path: '/greenhouse/',
        component: () => import('pages/editor/EditorIntro.vue'),
      },
      {
        path: '/editor/',
        component: () => import('pages/editor/FlowerpotEditor.vue'),
      },
      {
        path: '/editor/:id',
        component: () => import('pages/editor/FlowerpotEditor.vue'),
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
