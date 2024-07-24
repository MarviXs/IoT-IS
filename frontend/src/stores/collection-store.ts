import { defineStore } from 'pinia';
import { reactive } from 'vue';
import CollectionService from 'src/services/CollectionService';
import { useAsyncData } from '@/composables/useAsyncData';
import { useI18n } from 'vue-i18n';
import { Collection } from '@/models/Collection';
import { Module } from '@/models/Module';

export const useCollectionStore = defineStore('collection', () => {
  const { t } = useI18n();

  const collections = reactive(
    useAsyncData(CollectionService.getCollections, t('collection.toasts.load_failed'), false),
  );

  function updateCollection(collection: Collection) {
    if (!collections.data) return;
    const index = collections.data.findIndex((c) => c.uid === collection.uid);
    if (index === -1) return;
    collections.data[index] = collection;
  }

  function getCollectionById(id: string): Collection | undefined {
    return collections.data?.find((collection) => collection.uid === id);
  }

  function getModuleById(moduleId: string): Module | undefined {
    return collections.data?.flatMap((collection) => collection.modules).find((module) => module?.uid === moduleId);
  }

  return {
    collections,
    updateCollection,
    getCollectionById,
    getModuleById,
  };
});
