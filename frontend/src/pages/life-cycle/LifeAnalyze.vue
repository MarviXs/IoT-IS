<template>
  <q-page class="q-pa-lg">
    <q-card class="q-px-lg q-py-md shadow-2">
      <q-card-section>
        <div class="text-h5 text-primary">{{ t('lifecycle.add_plant') }}</div>
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
            <q-img
              :src="plantProperties.photoLeaves || '../../assets/logo.png'"
              alt="Leaves Image"
              style="max-height: 300px; object-fit: contain;"
              no-spinner
            />
            <q-img
              :src="plantProperties.photoSize || '../../assets/logo.png'"
              alt="Leaves Image"
              style="max-height: 300px; object-fit: contain;"
              no-spinner
            />
            <q-btn
              flat
              class="q-mt-md"
              color="primary"
              :icon="mdiUpload"
              :label="t('lifecycle.upload_photo')"
              @click="triggerFileInput"
            />
            <input
              type="file"
              accept="image/*"
              ref="fileInput"
              class="hidden"
              @change="handleFileUpload"
            />
            <q-slider v-model="threshold" label label-always :min="120" :max="140" :step="1" class="q-mt-md" />
          </div>

          <!-- Right section for plant properties -->
          <div class="col-6">
            <q-form>
              <q-input
                v-model="plantProperties.plantBoardId"
                outlined
                :label="t('lifecycle.plantboard_id')"
                dense
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.id"
                outlined
                :label="t('lifecycle.pid')"
                dense
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.height"
                outlined
                :label="t('lifecycle.height')"
                dense
                type="number"
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.width"
                outlined
                :label="t('lifecycle.width')"
                dense
                type="number"
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.leafCount"
                outlined
                :label="t('lifecycle.leafcount')"
                dense
                type="number"
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.area"
                outlined
                :label="t('lifecycle.areacm2')"
                dense
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.type"
                outlined
                :label="t('lifecycle.plant_type')"
                dense
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.disease"
                outlined
                :label="t('lifecycle.disease')"
                dense
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.date"
                outlined
                :label="t('lifecycle.date')"
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
        <q-btn :label="t('lifecycle.save_plant')" color="primary" :icon="mdiCheck" class="q-mr-sm" @click="savePlant"/>
        <q-btn :label="t('lifecycle.cancel')" color="secondary" flat @click="goBack"/>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { mdiCheck, mdiUpload } from '@quasar/extras/mdi-v7';
import LifeCycleService, {
  type CreateAnalysisRequest,
  type CreatePlantRequest,
} from '@/api/services/LifeCycleService';
import { useI18n } from 'vue-i18n';
import { CONFIG } from '@/config';

const { t } = useI18n();

const plantProperties = ref({
  id: '',
  plantBoardId: '',
  height: '',
  width: '',
  leafCount: '',
  area: '',
  type: '',
  disease: '',
  date: '',
  photo: '',
  photoLeaves: '',
  photoSize: '',
  imageName: '',
});

const threshold = ref(130);
const fileInput = ref<HTMLInputElement | null>(null);
const router = useRouter();

const route = useRoute();
const plantId = route.params.id;
if(plantId !== undefined){
  plantProperties.value.id = plantId.toString();
}

const triggerFileInput = () => {
  fileInput.value?.click();
};

// Function to upload the photo and get plant properties
const uploadPhoto = async (file: File) => {
  const formData = new FormData();
  formData.append('photo', file);
  formData.append('threshold', threshold.value.toString());
  formData.append('mode', '0');

  try {
    const response = await fetch(`${CONFIG.API_BASE_URL}${CONFIG.UPLOAD_ENDPOINT}`, {
      method: 'POST',
      body: formData,
    });

    if (response.ok) {
      const data = await response.json();
      // Update the plant properties with the response data
      plantProperties.value.height = data.height;
      plantProperties.value.width = data.width;
      plantProperties.value.area = data.area;
      plantProperties.value.date = data.date;
      plantProperties.value.leafCount = data.leafCount;
      plantProperties.value.type = data.plant;
      plantProperties.value.disease = data.disease;
      plantProperties.value.photoLeaves = `data:image/png;base64,${data.render}`;
      plantProperties.value.photoSize = `data:image/png;base64,${data.plantImage}`;
      plantProperties.value.imageName = file.name;
    } else {
      console.error('Failed to upload photo');
    }
  } catch (error) {
    console.error('Error uploading photo:', error);
  }
};

// Handle file input change
const handleFileUpload = (event: Event) => {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (file) {
    const reader = new FileReader();
    reader.onload = async () => {
      plantProperties.value.photo = reader.result as string;
      // After uploading the photo, send it to the server and update properties
      await uploadPhoto(file);
    };
    reader.readAsDataURL(file);
  }
};

// Metóda na uloženie rastliny
const savePlant = async () => {
  if (!plantProperties.value.plantBoardId) {
    console.error('Plant board ID is required');
    return;
  }

  const request: CreatePlantRequest = {
    plantId: plantProperties.value.id,
    name: plantProperties.value.type,
    type: 'plant',
    datePlanted: new Date(plantProperties.value.date).toISOString(),
    plantBoardId: plantProperties.value.plantBoardId,
  };

  try {
    // Zavolajte metódu createPlant zo služby
    const responseID = await LifeCycleService.createPlant(request);
    //console.log('Plant saved successfully');

    if (responseID && responseID.data) {
      const responsePlantID = responseID.data;

      const requestAnalysis: CreateAnalysisRequest = {
        height: parseFloat(plantProperties.value.height),
        width: parseFloat(plantProperties.value.width),
        leafCount: parseInt(plantProperties.value.leafCount),
        area: parseInt(plantProperties.value.area),
        disease: plantProperties.value.disease,
        health: 'healthy',
        plantId: responseID.response.status === 409 ? responseID.response.statusText : responsePlantID,
        analysisDate: new Date(plantProperties.value.date).toISOString(),
        imageName: plantProperties.value.imageName || 'plant-analysis.png',
      };

      await LifeCycleService.createAnalysis(requestAnalysis);
      console.log('Plant analysis saved successfully');
    }
  } catch (error) {
    console.error('Failed to save plant:', error);
  }
};

const goBack = () => {
  router.back();
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
