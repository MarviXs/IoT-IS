<template>
  <q-page class="q-pa-lg">
    <q-card class="q-px-lg q-py-md shadow-2">
      <q-card-section>
        <div class="text-h5 text-primary">Add New Plants</div>
      </q-card-section>
      <q-separator />

      <q-card-section>
        <div class="row q-gutter-md">
          <!-- Left section for the plant photo with cropping -->
          <div class="col-6">
            <div v-if="showCropper">
              <vue-advanced-cropper
                ref="cropper"
                class="cropper"
                :src="plantProperties.photo"
                @change="onCropChange"
              />
              <q-btn label="Crop" color="primary" class="q-mt-md" @click="applyCrop" />
            </div>
            <div v-else>
              <q-img
                :src="plantProperties.photoLeaves || '../../assets/logo.png'"
                alt="Leaves Image"
                style="max-width: 300px; height: auto; object-fit: contain;"
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
            <q-slider v-model="threshold" label label-always :min="120" :max="140" :step="1" class="q-mt-md" />
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
                v-model="plantProperties.rows"
                outlined
                label="Rows"
                dense
                type="number"
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.cols"
                outlined
                label="Cols"
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
        <q-btn label="Save Plant" color="primary" :icon="mdiCheck" class="q-mr-sm" @click="savePlant"/>
        <q-btn label="Cancel" color="secondary" flat @click="goBack"/>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { mdiCheck, mdiUpload } from '@quasar/extras/mdi-v7';
import { Cropper as VueAdvancedCropper } from 'vue-advanced-cropper';
import 'vue-advanced-cropper/dist/style.css';
import { ro } from 'date-fns/locale';

const plantProperties = ref({
  id: '',
  leafCount: '',
  rows: '',
  cols: '',
  area: '',
  type: '',
  disease: '',
  date: '',
  photo: '',
  photoLeaves: '',
});

let uploadedFile: File | null = null;
let imageWidth = 0;
let imageHeight = 0;
let imageTop = 0;
let imageLeft = 0;  
let threshold = ref(130);
const fileInput = ref<HTMLInputElement | null>(null);
const router = useRouter();
const showCropper = ref(false);
const cropper = ref(null);

const cropData = ref({
  x: 0,        // Pozícia na osi X (ľavý okraj orezaného obrázka)
  y: 0,        // Pozícia na osi Y (horný okraj orezaného obrázka)
  width: 0,     // Šírka orezaného obrázka
  height: 0,    // Výška orezaného obrázka
  image: {      // Dodatočné informácie o obrázku
    width: 0,   // Pôvodná šírka obrázka
    height: 0,  // Pôvodná výška obrázka
  },
  coordinates: { // Informácie o viditeľnej oblasti obrázka
    left: 0,
    top: 0,
    width: 0,
    height: 0,
  },
});


const triggerFileInput = () => {
  fileInput.value?.click();
};

const handleFileUpload = (event: Event) => {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (file) {
    uploadedFile = file;
    const reader = new FileReader();
    reader.onload = () => {
      plantProperties.value.photo = reader.result as string;
      showCropper.value = true;
    };
    reader.readAsDataURL(file);
  }
};

const onCropChange = (newCropData: any) => {
  cropData.value = newCropData;
  imageWidth = cropData.value.coordinates.width;
  imageHeight = cropData.value.coordinates.height;
  imageTop = cropData.value.coordinates.top;
  imageLeft = cropData.value.coordinates.left;
};

const applyCrop = () => {
  if (uploadedFile) {
    uploadPhoto(uploadedFile);
    showCropper.value = false;
  } else {
    console.error("No file uploaded!");
  }
};

const uploadPhoto = async (file: File) => {
  const formData = new FormData();
  formData.append('photo', file);
  formData.append('threshold', threshold.value.toString());
  formData.append('mode', '1');
  formData.append('width', imageWidth.toString());
  formData.append('height', imageHeight.toString());
  formData.append('top', imageTop.toString());
  formData.append('left', imageLeft.toString());
  formData.append('rows', plantProperties.value.rows.toString());
  formData.append('cols', plantProperties.value.cols.toString());

  try {
    const response = await fetch('http://localhost:5000/upload', {
      method: 'POST',
      body: formData,
    });

    if (response.ok) {
      const data = await response.json();
      plantProperties.value.area = data.area;
      plantProperties.value.date = data.date;
      plantProperties.value.leafCount = data.leafCount;
      plantProperties.value.type = data.plant;
      plantProperties.value.disease = data.disease;
      plantProperties.value.photoLeaves = `data:image/png;base64,${data.plantImage}`;
    } else {
      console.error('Failed to upload photo');
    }
  } catch (error) {
    console.error('Error uploading photo:', error);
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

.cropper {
  width: 100%;
  height: 300px;
}
</style>
