<template>
  <q-page class="q-pa-lg">
    <q-card class="q-px-lg q-py-md shadow-2">
      <q-card-section>
        <div class="text-h5 text-primary">Add New Plant</div>
      </q-card-section>
      <q-separator />

      <q-card-section>
        <div class="row q-gutter-md">
          <!-- Left section for the plant photo -->
          <div class="col-6">
            <q-img
              :src="plantProperties.photo || '../../assets/logo.png'"
              alt="Plant Image"
              style="max-height: 300px; object-fit: contain;"
              no-spinner
            />
            <q-btn
              flat
              class="q-mt-md"
              color="primary"
              :icon="mdiUpload"
              label="Upload Photo"
              @click="triggerFileInput"
            />
            <input
              type="file"
              accept="image/*"
              ref="fileInput"
              class="hidden"
              @change="handleFileUpload"
            />
          </div>

          <!-- Right section for plant properties -->
          <div class="col-6">
            <q-form>
              <q-input
                v-model="plantProperties.id"
                outlined
                label="Plant ID"
                dense
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.height"
                outlined
                label="Height (cm)"
                dense
                type="number"
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.width"
                outlined
                label="Width (cm)"
                dense
                type="number"
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.leafCount"
                outlined
                label="Leaf Count"
                dense
                type="number"
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.area"
                outlined
                label="Area"
                dense
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.type"
                outlined
                label="Plant Type"
                dense
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.disease"
                outlined
                label="Disease"
                dense
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.date"
                outlined
                label="Date"
                type="date"
                dense
                class="q-mb-md"
              />
            </q-form>
          </div>
        </div>
      </q-card-section>

      <q-separator />
      <q-card-section align="right">
        <q-btn label="Save Plant" color="primary" :icon="mdiCheck" class="q-mr-sm" />
        <q-btn label="Cancel" color="secondary" flat />
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { mdiCheck, mdiUpload } from '@quasar/extras/mdi-v7';

const plantProperties = ref({
  id: '',
  height: '',
  width: '',
  leafCount: '',
  area: '',
  type: '',
  disease: '',
  date: '',
  photo: '',
});

const fileInput = ref<HTMLInputElement | null>(null);

const triggerFileInput = () => {
  fileInput.value?.click();
};

const handleFileUpload = (event: Event) => {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (file) {
    const reader = new FileReader();
    reader.onload = () => {
      plantProperties.value.photo = reader.result as string;
    };
    reader.readAsDataURL(file);
  }
};
</script>

<style scoped>
.q-card {
  max-width: 900px;
  margin: auto;
}

.hidden {
  display: none;
}
</style>
