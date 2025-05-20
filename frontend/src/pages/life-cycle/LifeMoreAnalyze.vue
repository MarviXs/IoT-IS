<template>
  <q-page class="q-pa-lg">
    <q-card class="q-px-lg q-py-md shadow-2">
      <q-card-section>
        <div class="text-h5 text-primary">{{ t('lifecycle.add_plants') }}</div>
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
              <q-btn :label="t('lifecycle.crop')" color="primary" class="q-mt-md" @click="applyCrop" />
            </div>
            <div v-else>
              <q-img
                v-if="!isLoading"
                :src="plantProperties.photoLeaves || '../../assets/logo.png'"
                alt="Leaves Image"
                style="max-width: 300px; height: auto; object-fit: contain;"
                @click="getClickPosition"
              />
              <q-spinner v-else color="primary" size="50px" />

              <div
                v-for="(index, idx) in positionDisX"
                :key="idx"
                class="marker"
                :style="{ left: `${positionDisX[idx]}px`, top: `${positionDisY[idx]}px` }"
                @click="confirmRemove(idx)"
              >
                <span v-if="choroby[idx]" class="disease-label">{{ choroby[idx] }}</span>
              </div>

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
            </div>
            <q-slider v-model="threshold" label label-always :min="110" :max="130" :step="1" class="q-mt-md" />
          </div>

          <!-- Right section for plant properties -->
          <div class="col-6">
            <q-form>
              <q-input
                v-model="plantProperties.id"
                outlined
                :label="t('lifecycle.plantboard_id')"
                dense
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.rows"
                outlined
                :label="t('lifecycle.rows')"
                dense
                type="number"
                class="q-mb-md"
              />
              <q-input
                v-model="plantProperties.cols"
                outlined
                :label="t('lifecycle.columns')"
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
                type="number"
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

    <q-dialog v-model="dialogOpen">
      <q-card>
        <q-card-section>
          <div class="text-h6">{{ t('lifecycle.enter_disease') }}</div>
          <q-input v-model="diseaseInput" label="Disease Name" dense autofocus />
        </q-card-section>

        <q-card-actions>
          <q-btn :label="t('lifecycle.cancel')" color="secondary" @click="dialogOpen = false" />
          <q-btn :label="t('lifecycle.save_disease')" color="primary" @click="saveDisease" />
        </q-card-actions>
      </q-card>
    </q-dialog>


    <q-dialog v-model="dialogOpen2">
      <q-card>
        <q-card-section>
          <div class="text-h6">{{ t('lifecycle.confirm_delete_disease', { disease: selectedDisease }) }}</div>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat :label="t('lifecycle.no')" color="grey" v-close-popup />
          <q-btn flat :label="t('lifecycle.yes')" color="red" @click="removeDisease" />
        </q-card-actions>
      </q-card>
    </q-dialog>


  </q-page>
</template>


<script setup lang="ts">
import { h, ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { mdiCheck, mdiUpload } from '@quasar/extras/mdi-v7';
import { Cropper as VueAdvancedCropper } from 'vue-advanced-cropper';
import 'vue-advanced-cropper/dist/style.css';
import LifeCycleService from '@/api/services/LifeCycleService';
import { useI18n } from 'vue-i18n';
import { CONFIG } from '@/config';

const { t } = useI18n();
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
  photoName: '',
});

let uploadedFile: File | null = null;
let imageWidth = 0;
let imageHeight = 0;
let imageTop = 0;
let imageLeft = 0;  
let threshold = ref(130);
let tmpResponse: { nums: Array<string> } | undefined;
let choroby: string[] = [];
let index = ref<number | null>(null);
let positionDisX: number[] = [];
let positionDisY: number[] = [];
let selectedIndex = ref<number | null>(null); 
const fileInput = ref<HTMLInputElement | null>(null);
const router = useRouter();
const route = useRoute();
const showCropper = ref(false);
const cropper = ref(null);
const receivedPblId = route.params.id || '';
const dialogOpen = ref(false);
const diseaseInput = ref('');
const isLoading = ref(false);
const dialogOpen2 = ref(false);
const selectedDisease = ref("");

if(receivedPblId !== undefined){
  plantProperties.value.id = receivedPblId.toString();
}

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

const calculateTotalLeaves = (nums: Record<string, { leaves: number; size: number }>) => {
  return Object.values(nums).reduce((sum, entry) => sum + entry.leaves, 0).toString();
};

const calculateTotalArea = (nums: Record<string, { leaves: number; size: number }>) => {
  return Math.round(Object.values(nums).reduce((sum, entry) => sum + entry.size, 0)).toString();
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
  isLoading.value = true;
  try {
    const response = await fetch(`${CONFIG.API_BASE_URL}${CONFIG.UPLOAD_ENDPOINT}`, {
      method: 'POST',
      body: formData,
    });

    if (response.ok) {
      const data = await response.json();
      tmpResponse = data;

      plantProperties.value.area = calculateTotalArea(data.nums);
      plantProperties.value.date = data.date;
      plantProperties.value.leafCount = calculateTotalLeaves(data.nums);;
      plantProperties.value.type = data.plant;
      plantProperties.value.disease = data.disease;
      plantProperties.value.photoLeaves = `data:image/png;base64,${data.plantImage}`;
      plantProperties.value.photoName = data.plantImageName;
    } else {
      console.error('Failed to upload photo');
    }
  } catch (error) {
    console.error('Error uploading photo:', error);
  }
  isLoading.value = false;
};

const savePlant = async () => {

  const request = {
    plantBoardId: plantProperties.value.id,
    rows: Number(plantProperties.value.rows),
    cols: Number(plantProperties.value.cols),
  };

  try {
    // Zavolajte metódu createPlant zo služby
    const responseID = await LifeCycleService.createPlantBoard(request);
    //console.log('Plant saved successfully');

    if (tmpResponse ==  undefined){
      console.log(`Undefinedddd`);
    }

    if (responseID && responseID.data && tmpResponse !== undefined) {
      const plantBoardId = responseID.data;

      if (tmpResponse.nums && typeof tmpResponse.nums === 'object') {
        // Preveď objekt na pole hodnôt
        const numsArray = Object.values(tmpResponse.nums);

        for (const num of numsArray.entries()) {
          const re = num[1];  // Hodnota z numsArray
          const reParsed = JSON.parse(JSON.stringify(re));  // Vyparsujeme JSON string späť na objekt

          const leaves1 = reParsed.leaves;  // Získaš hodnotu "leaves"
          const size1 = reParsed.size;
          const disease1 = reParsed.disease;
          const width1 = reParsed.x;
          const height1 = reParsed.y;
          console.log(`Leaves: ${leaves1} Size: ${size1}`);

          const indexP = num[0];

          // Nastavíme pevne hodnoty
          const plant = {
              leaves: leaves1,   // Pevne nastavené hodnoty
              size: size1,   // Pevne nastavené hodnoty
              disease: disease1,  // Pevne nastavené hodnoty
          };

          // Generujeme plantId
          //const plantId = `${plantBoardId}${index}`;
          const plantId = `${plantProperties.value.id}${indexP}`;
          const requestAnalysis = {
              leafCount: plant.leaves.toString(),  // Pevne nastavené na 10
              area: plant.size,  // Pevne nastavené na 1500
              plantBoardId: plantProperties.value.id,
              plantId: plantId,
              analysisDate: new Date(plantProperties.value.date).toISOString(),
              name: `${plantProperties.value.id}${indexP}`,
              type: plantProperties.value.type || 'Unknown',
              datePlanted: new Date(plantProperties.value.date).toISOString(),
          };

          // Pošli požiadavku na uloženie rastliny
          const res_id = await LifeCycleService.createPlant(requestAnalysis);

          if (res_id && res_id.data) {
            const responsePlantID = res_id.data;

            const requestAnalysis = {
              height: height1,
              width: width1,
              leafCount: plant.leaves,
              area: plant.size,
              disease: choroby[indexP] || 'Zdrava',
              health: "healthy",
              plantId: res_id.response.status === 409 ? res_id.response.statusText : responsePlantID,
              analysisDate: new Date(plantProperties.value.date).toISOString(),
              imageName: plantProperties.value.photoName || 'Obrazok',
            };

            await LifeCycleService.createAnalysis(requestAnalysis);
            console.log('Plant analysis saved successfully');
          }

          console.log(`Plant analysis saved successfully for plantId: ${plantId}`);
      }

  } else {
    console.error('Error: tmpResponse.nums is not a valid object');
    }
    }
  } catch (error) {
    console.error('Failed to save plant:', error);
  }
};

const goBack = () => {
  router.back();
};

const getClickPosition = (event: MouseEvent) => {
  if (!tmpResponse || !tmpResponse.nums) {
    console.error("No plant data available!");
    return;
  }

  const imgElement = event.target as HTMLImageElement;
  const rect = imgElement.getBoundingClientRect();
  const clickX = event.clientX - rect.left;
  const clickY = event.clientY - rect.top;

  // Skutočné rozmery obrázka
  const realWidth = imgElement.naturalWidth;
  const realHeight = imgElement.naturalHeight;

  // Prepočítané súradnice v REÁLNYCH rozmeroch
  const scaledX = (clickX / rect.width) * realWidth;
  const scaledY = (clickY / rect.height) * realHeight;

  console.log(`X: ${scaledX}, Y: ${scaledY}`);

  // Parsovanie tmpResponse.nums
  const numsArray = Object.values(tmpResponse.nums);
  let closestPlantIndex: number | null = null;
  let minDistance = Infinity;

  numsArray.forEach((num, index) => {
    const reParsed = JSON.parse(JSON.stringify(num)); // Konverzia na objekt

    const plantX = reParsed.x;
    const plantY = reParsed.y;

    const distance = Math.sqrt((plantX - scaledX) ** 2 + (plantY - scaledY) ** 2);
    if (distance < minDistance) {
      minDistance = distance;
      closestPlantIndex = index;
    }
  });

  if (closestPlantIndex !== null) {
    dialogOpen.value = true;
    index = closestPlantIndex;
    positionDisX[Number(index)] = clickX;
    positionDisY[Number(index)] = clickY;
  }
};

const saveDisease = () => {
  console.log(`Disease for plant ${index}: ${diseaseInput.value}`);
  if (Number(index) >= 0 && diseaseInput.value) {
    choroby[Number(index)] = diseaseInput.value;
    dialogOpen.value = false;
    diseaseInput.value = '';
  }
};

const confirmRemove = (index: number) => {
  selectedIndex.value = index;
  selectedDisease.value = choroby[index];
  dialogOpen2.value = true;
};

const removeDisease = () => {
  if (selectedIndex.value !== null) {
    positionDisX.splice(selectedIndex.value, 1);
    positionDisY.splice(selectedIndex.value, 1);
    choroby.splice(selectedIndex.value, 1);
  }
  dialogOpen2.value = false;
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

.disease-label {
  position: absolute;
  background-color: rgba(0, 0, 0, 0.5);
  color: white;
  font-size: 12px;
  padding: 2px;
  border-radius: 15px;
  transform: translateX(-50%);
}

.marker {
  position: absolute;
  cursor: pointer;  /* Zmena kurzora na pointer, aby bolo zrejmé, že sa dá kliknúť */
}

.image-container {
  position: relative;
}
</style>
