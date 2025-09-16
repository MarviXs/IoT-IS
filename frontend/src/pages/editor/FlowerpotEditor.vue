<template>
  <div>

    <q-btn
      :label="t('greenhouse.add_plant')"
      class="q-mb-sm q-mr-sm"
      @click="showPlantList = true"
    />

    <q-btn
      :label="t('greenhouse.add_pot')"
      class="q-mb-sm q-mr-sm"
      @click="openPotSizeDialog"
    />

    <q-btn
      :label="t('greenhouse.p_day')"
      class="q-mb-sm q-mr-sm"
      @click="adjustPlantStates(-1)"
    />

    <q-btn
      :label="t('greenhouse.n_day')"
      class="q-mb-sm q-mr-sm"
      @click="adjustPlantStates(1)"
    />

    <q-btn
      :label="t('greenhouse.save')"
      class="q-mb-sm"
      @click="saveState()"
    />

    <!-- Dialógové okno pre zadanie rozmerov skleníka -->
    <div v-if="showGreenhouseDialog" class="dialog">
      <div class="dialog-content">
        <h3>{{ t('greenhouse.enter_details') }}</h3>

        <q-input
          v-model="greenhouseName"
          type="text"
          :label="t('greenhouse.greenhouse_name')"
          dense
          outlined
          class="q-mb-md"
        />

        <q-input
          v-model.number="greenhouseWidth"
          type="number"
          :label="t('greenhouse.width_meters')"
          min="1"
          dense
          outlined
          class="q-mb-sm"
        />

        <q-input
          v-model.number="greenhouseHeight"
          type="number"
          :label="t('greenhouse.length_meters')"
          min="1"
          dense
          outlined
          class="q-mb-md"
        />

        <!-- Výber počtu rastlín -->
        <h4>{{ t('greenhouse.choose_plants') }}</h4>
        <div v-for="plant in plantOptions" :key="plant.id" class="q-mb-sm">
          <q-input
            v-model.number="plantSelection[plant.id]"
            type="number"
            :label="plant.name"
            min="0"
            dense
            outlined
            style="width: 120px;"
          />
        </div>

        <q-btn
          :label="t('greenhouse.create')"
          class="q-mt-md q-mr-sm"
          @click="setGreenhouseSize"
        />

        <q-btn
          :label="t('greenhouse.cancel')"
          flat
          class="q-mt-md"
          @click="closeGreenhouseDialog"
        />
      </div>
    </div>

    <!-- Zoznam na výber veľkosti rastliny -->
    <div v-if="showPlantList" class="dialog">
      <div class="dialog-content">
        <h3>{{ t('greenhouse.choose_plant') }}</h3>
        <ul>
          <li v-for="plant in plantOptions" :key="plant.id">
            <q-btn
              :key="plant.id"
              :label="plant.name"
              class="q-mb-sm q-mr-sm"
              @click="addRectangle(plant)"
            />
          </li>
        </ul>
        <q-btn
          :label="t('greenhouse.cancel')"
          flat
          class="q-mt-md"
          @click="showPlantList = false"
        />
      </div>
    </div>

    <!-- Zoznam na výber veľkosti megakvetináča -->
    <q-dialog v-model="showPotSizeDialog">
      <q-card class="q-pa-md" style="min-width: 300px;">
        <q-card-section>
          <h3>{{ t('greenhouse.choose_pot') }}</h3>
          <h4>{{ t('greenhouse.plantboards') }}</h4>

          <q-select
            v-model="selectedLayout"
            :options="predefinedLayouts"
            option-label="label"
            :label = "t('greenhouse.create')"
            emit-value
            map-options
            @update:model-value="createMegaPotByLayout"
            outlined
            dense
            class="q-mb-md"
          />
        </q-card-section>

        <q-card-section>
          <q-list bordered separator>
            <q-item v-for="size in potSizes" :key="size.id" clickable @click="selectPotSize(size)">
              <q-item-section>{{ size.name }}</q-item-section>
            </q-item>
          </q-list>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="t('greenhouse.cancel')" color="primary" @click="closePotSizeDialog" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- Dialógové okno pre zadanie rozmerov megakvetináča -->
    <q-dialog v-model="showDialog">
      <q-card class="q-pa-md" style="min-width: 300px;">
        <q-card-section>
          <h3>{{ t('greenhouse.set_plantboard_size') }}</h3>

          <q-input
            v-model.number="columns"
            type="number"
            min="1"
            :label="t('greenhouse.cols')"
            outlined
            dense
            class="q-mb-sm"
          />

          <q-input
            v-model.number="rows"
            type="number"
            min="1"
            :label="t('greenhouse.rows')"
            outlined
            dense
            class="q-mb-md"
          />
        </q-card-section>

        <q-card-actions align="right">
          <q-btn :label="t('greenhouse.create')" color="primary" @click="createMegaPot" />
          <q-btn flat :label="t('greenhouse.cancel')" color="primary" @click="closeDialog" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <q-dialog v-model="showUploadDialog">
      <q-card class="q-pa-md" style="min-width: 350px;">
        <q-card-section>
          <h3>{{ t('greenhouse.upload_plant_image') }}</h3>

          <!-- Výber súboru -->
          <q-file
            v-model="selectedFile"
            :label="t('greenhouse.choose_file')"
            accept="image/*"
            outlined
            dense
            class="q-mb-md"
          >
            <template v-slot:append>
              
            </template>
          </q-file>

          <!-- Vstup pre stĺpce -->
          <q-input
            v-model.number="columnsImage"
            type="number"
            min="1"
            :label="t('greenhouse.cols')"
            outlined
            dense
            class="q-mb-sm"
          />

          <!-- Vstup pre riadky -->
          <q-input
            v-model.number="rowsImage"
            type="number"
            min="1"
            :label="t('greenhouse.rows')"
            outlined
            dense
          />
        </q-card-section>

        <q-card-actions align="right">
          <q-btn :label="t('greenhouse.create')" color="primary" @click="handleUploadAndCreate" />
          <q-btn flat :label="t('greenhouse.cancel')" color="primary" @click="showUploadDialog = false" />
        </q-card-actions>
      </q-card>
    </q-dialog>



    <!-- Konva Stage a Layer -->
    <v-stage
      ref="stage"
      :config="{
        width: stageSize.width,
        height: stageSize.height,
      }"
      
      @mousedown="handleStageMouseDown"
      @touchstart="handleStageMouseDown"
    >
      <v-layer ref="layer">
      <!-- Zobraz "skleník" ako sedy rect -->
      <v-rect
        :config="{
          x: 0,
          y: 0,
          width: stageSize.width,
          height: stageSize.height,
          fill: '#f0f0f0',
          stroke: '#ccc',
          strokeWidth: 2,
        }"
      />

        <template v-for="(megaPot, index) in megaPots" :key="'mega-pot-' + index">
          <template v-for="(innerPot, innerIndex) in megaPot.innerPots" :key="'inner-pot-' + innerIndex">
            <v-rect
              v-if="innerPot.shape === 'square'"
              :config="{
                x: innerPot.x,
                y: innerPot.y,
                width: innerPot.width,
                height: innerPot.height,
                fill: 'brown',
                stroke: 'black',
                strokeWidth: 1,
                draggable: true,
                name: 'inner-pot-' + innerIndex
              }"
              @dragmove="(e) => handleMegaPotMoveById(index, e)"
              @contextmenu="handleContextDelMenu(megaPot, $event)"
            />
            <v-circle
              v-else
              :config="{
                x: innerPot.x + innerPot.width / 2,
                y: innerPot.y + innerPot.height / 2,
                radius: innerPot.width / 2,
                fill: 'brown',
                stroke: 'black',
                strokeWidth: 1,
                draggable: true,
                name: 'inner-pot-' + innerIndex
              }"
              @dragmove="(e) => handleMegaPotMoveById(index, e)"
              @contextmenu="handleContextDelMenu(megaPot, $event)"
            />
          </template>
        </template>

        <!-- Zobrazujeme rastliny -->
        <v-rect
          v-for="item in rectangles"
          :key="item.id"
          :config="item"
          @dragmove="handleDragMove"
          @dragend="handleDragEnd"
          @contextmenu="handleContextMenu(item, $event)"
          @mouseenter="hoveredId = item.id"
          @mouseleave="hoveredId = null"
        />
        <!-- Zobrazenie názvu rastliny -->
        <v-text
          v-for="plant in plants"
          :key="'text-' + plant.id"
          :config="{
            x: plant.x + plant.width / 4,
            y: plant.y + plant.height / 4,
            text: plant.name,
            fontSize: 16,
            fill: 'black',
            align: 'center',
            draggable: false,
            opacity: plant.id === hoveredId ? 1 : 0,
          }"
        />
        <v-transformer ref="transformer" />
      </v-layer>
    </v-stage>

    <!-- Informácie o rastlinách -->
    <div v-show="showPlantInfo" class="plant-info" :style="plantInfoStyle">
      <h3>{{ t('greenhouse.plant_informations') }}</h3>
      <h4>{{ currentSimulatorDay }}</h4>
      <div class="plant-info-scroll">
        <ul>
          <li v-for="plant in plants" :key="'info-' + plant.id">
            <strong @click="toggleDetails(plant.id)" class="clickable">
              {{ plant.name }}
            </strong>

            <transition name="fade">
              <div v-if="expandedPlants.includes(plant.id)" class="plant-details">
                {{ t('greenhouse.type') }}: {{ plant.type }}<br />
                {{ t('greenhouse.species') }}: {{ plant.species }}<br />
                {{ t('greenhouse.light') }}: {{ plant.light }}<br />
                {{ t('greenhouse.soil') }}: {{ plant.soil }}<br />
                {{ t('greenhouse.pot') }}: {{ plant.potName }}
              </div>
            </transition>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script >
import GreenHouseService from '@/api/services/GreenHouseService';
import OrdersService from '@/api/services/OrdersService'; 
import ProductService from '@/api/services/ProductService';
import { useI18n } from 'vue-i18n';


const width = 500;
const height = 800;
let changed = false

export default {
  setup() {
    const { t } = useI18n(); 
    return {
      t
    }
  },
  data() {
    return {
      loaded: false,
      ghouseid: null,
      greenhouseName: 'Default Greenhouse',
      showUploadDialog: false,
      selectedFile: null,
      columnsImage: 1,
      rowsImage: 1,
      expandedPlants: [],
      hoveredId: null,
      hoveredPlantId: null,
      showPlantInfo: true,
      plantSelection: {
        1: 0, // Malá rastlina
        2: 0, // Stredná rastlina
        3: 0  // Veľká rastlina
      },
      currentSimulatedDate: new Date(),
      plantInfoStyle: {
        position: 'absolute',
        top: '10px',
        right: '10px',
        background: 'white',
        padding: '10px',
        border: '1px solid black',
        transition: 'top 0.2s ease-out', // Pre hladký pohyb pri skrolovaní
      },
      greenhouseWidth: 6,
      greenhouseHeight: 4,
      showGreenhouseDialog: false,
      stageSize: {
        width: width,
        height: height,
      },
      greenhouseSize: {
        width: 0,
        height: 0
      },
      predefinedLayouts: [
        {
          label: '13 stĺpcov x 20 riadkov (18x18)',
          columns: 13,
          rows: 20,
          width: 18,
          height: 18
        },
        {
          label: '15 stĺpcov x 30 riadkov (15x15)',
          columns: 15,
          rows: 30,
          width: 15,
          height: 15
        },
        {
          label: '10 stĺpcov x 16 riadkov (30x30)',
          columns: 10,
          rows: 16,
          width: 30,
          height: 30
        },
        {
          label: '10 stĺpcov x 18 riadkov (26x26)',
          columns: 10,
          rows: 18,
          width: 26,
          height: 26
        }
      ],
      rectangles: [],
      pots: [],
      plants: [],
      megaPots: [],
      innerPots: [],
      nextId: 1,
      selectedShapeName: '',
      plantPotMap: new Map(),
      showDialog: false,
      showPlantList: false,
      showPotSizeDialog: false,
      columns: 1,
      rows: 1,
      currentSimulatorDay: new Date().toLocaleDateString(),
      potSizes: [
        { id: 1, name: 'Kvetináč 85x85', width: 85, height: 85, shape: 'square' },
        { id: 2, name: 'Kvetináč Ø95', width: 95, height: 95, shape: 'circle' },
        { id: 3, name: 'Kvetináč Ø105', width: 105, height: 105, shape: 'circle' },
        { id: 4, name: 'Kvetináč Ø115', width: 115, height: 115, shape: 'circle' },
        { id: 5, name: 'Kvetináč Ø120', width: 120, height: 120, shape: 'circle' },
        { id: 6, name: 'Kvetináč Ø135', width: 135, height: 135, shape: 'circle' },
        { id: 7, name: 'Kvetináč Ø165', width: 165, height: 165, shape: 'circle' },
        { id: 8, name: 'Kvetináč Ø175', width: 175, height: 175, shape: 'circle' },
        { id: 9, name: 'Kvetináč Ø185', width: 185, height: 185, shape: 'circle' },
        { id: 10, name: 'Kvetináč Ø205', width: 205, height: 205, shape: 'circle' }
      ],
      selectedPotSize: null,
      plantOptions: [
        {
          id: 1, // productId
          name: 'Malá Rastlina', // czechName
          type: 'Bylina', //latinName
          currentState: 0, // vzdy nula
          states: [ // vzdy budu dve
            { stage: 'mladá', width: 15, height: 15, days: 7 },
            { stage: 'dospelá', width: 20, height: 20, days: 14 },
          ]
        },
        {
          id: 2,
          name: 'Stredná Rastlina',
          type: 'Krovina',
          currentState: 0,
          states: [
            { stage: 'mladá', width: 25, height: 25, days: 10 },
            { stage: 'dospelá', width: 100, height: 100, days: 20 },
            { stage: 'zrelá', width: 140, height: 140, days: 30 }
          ]
        },
        {
          id: 3,
          name: 'Veľká Rastlina',
          type: 'Strom',
          currentState: 0,
          states: [
            { stage: 'mladá', width: 30, height: 30, days: 14 },
            { stage: 'dospelá', width: 150, height: 150, days: 28 },
            { stage: 'zrelá', width: 210, height: 210, days: 42 }
          ]
        }
    ],
    };
  },
  mounted() {
    window.addEventListener('scroll', this.updatePlantInfoPosition);
    this.ghouseid = this.$route.params.id;
    console.log("TU JE ID:", this.ghouseid);

      ProductService.getProducts()
      .then(async response => {
        const items = response.data.items;

        // Použijeme Promise.all na všetky podprodukty
        const plantOptions = await Promise.all(
          items.map(async (product) => {
            try {
              const res = await ProductService.getProduct(product.id);

              const states = [
                {
                  stage: "mladá",
                  width: parseInt(res.data.seedSpacingCM),
                  height: parseInt(res.data.seedSpacingCM),
                  days: parseInt(res.data.cultivationTimeForBulbsWeeks) * 7
                },
                {
                  stage: "dospelá",
                  width: parseInt(res.data.plantSpacingCm),
                  height: parseInt(res.data.plantSpacingCm),
                  days: parseInt(res.data.cultivationTimeFromYoungPlant)
                }
              ];

              return {
                id: product.id,
                name: product.czechName,
                type: product.latinName,
                currentState: 0,
                states: states
              };
            } catch (err) {
              console.error(`Chyba pre ${product.id}:`, {
                message: err.message,
                status: err.response?.status,
                data: err.response?.data
              });

              return {
                id: product.id,
                name: product.czechName,
                type: product.latinName,
                currentState: 0,
                states: []
              };
            }
          })
        );

        this.plantOptions = plantOptions;
        console.log("Plant Options:", plantOptions);
      })
      .catch(err => {
        console.error("Chyba pri načítaní produktov:", err);
      });

    OrdersService.getOrderProducts(this.ghouseid) // tu bude id objednavky
      .then(response => {
        console.log("AHOJ", response.data)
        const products = response;

        this.plantOptions = products.map(product => ({
          id: product.productId,
          name: product.czechName,
          type: product.latinName,
          currentState: 0,
          states: [
            {
              stage: 'young',
              width: isNaN(parseInt(product.plantSpacingCm)) ? 0 : parseInt(product.plantSpacingCm),
              height: isNaN(parseInt(product.plantSpacingCm)) ? 0 : parseInt(product.plantSpacingCm),
              days: isNaN(parseInt(product.cultivationTimeFromYoungPlant)) ? 0 : parseInt(product.cultivationTimeFromYoungPlant)
            },
            {
              stage: 'seed',
              width: isNaN(parseInt(product.seedSpacingCM)) ? 0 : parseInt(product.seedSpacingCM),
              height: isNaN(parseInt(product.seedSpacingCM)) ? 0 : parseInt(product.seedSpacingCM),
              days: isNaN(parseInt(product.cultivationTimeVegetableWeek)) ? 0 : parseInt(product.cultivationTimeVegetableWeek)
            }
          ]
        }));

        console.log("Načítaná objednávka:", plantOptions);
      })
      .catch(error => {
        console.error("Chyba pri načítaní objednávky:", error);
      });

    if (this.ghouseid) {
      GreenHouseService.getGreenHouseById(this.ghouseid)
        .then(response => {
          this.greenhouseName = response.data.name;
          console.log("Načítaný skleník:", response.data);
          this.showGreenhouseDialog = false
          this.greenhouseWidth = response.data.width / 1000;
          this.greenhouseHeight = response.data.depth / 1000;
          this.greenhouseName = response.data.name;
          this.greenHouseID = response.data.id;
          this.loaded = true;
          this.loadState();
          this.setGreenhouseSize();
        })
        .catch(error => {
          console.error("Chyba pri načítaní skleníka:", error);
        });
    }else{
      this.showGreenhouseDialog = true;
    }

  },
  beforeUnmount() {
    window.removeEventListener('scroll', this.updatePlantInfoPosition);
  },
  methods: {
    toggleDetails(id) {
      if (this.expandedPlants.includes(id)) {
        this.expandedPlants = this.expandedPlants.filter(pid => pid !== id);
      } else {
        this.expandedPlants.push(id);
      }
    },
    getMegaPotBounds(megaPot) {
      const pots = megaPot.innerPots;

      const minX = Math.min(...pots.map(p => p.x));
      const minY = Math.min(...pots.map(p => p.y));
      const maxX = Math.max(...pots.map(p => p.x + p.width));
      const maxY = Math.max(...pots.map(p => p.y + p.height));

      return {
        x: minX,
        y: minY,
        width: maxX - minX,
        height: maxY - minY
      };
    },
    handleMegaPotMoveById(megaPotIndex, e) {
      console.log("Presúvame megakvetináč s indexom:", megaPotIndex);

      const megaPot = this.megaPots[megaPotIndex];
      const firstPot = megaPot.innerPots[0];

      const targetX = e.target.x();
      const targetY = e.target.y();

      const originalX = firstPot.x;
      const originalY = firstPot.y;

      const dx = targetX - originalX;
      const dy = targetY - originalY;

      const bounds = this.getMegaPotBounds(megaPot);
      const newBounds = {
        x: bounds.x + dx,
        y: bounds.y + dy,
        width: bounds.width,
        height: bounds.height
      };

      if (
        newBounds.x < 0 ||
        newBounds.y < 0 ||
        newBounds.x + newBounds.width > this.greenhouseWidth ||
        newBounds.y + newBounds.height > this.greenhouseHeight
      ) {
        console.warn("MegaPot by vyšiel mimo skleník. Pohyb zamietnutý.");
        e.target.x(originalX);
        e.target.y(originalY);
        return;
      }

      for (let i = 0; i < this.megaPots.length; i++) {
        if (i === megaPotIndex) continue;

        const otherBounds = this.getMegaPotBounds(this.megaPots[i]);

        const isOverlapping =
          newBounds.x < otherBounds.x + otherBounds.width &&
          newBounds.x + newBounds.width > otherBounds.x &&
          newBounds.y < otherBounds.y + otherBounds.height &&
          newBounds.y + newBounds.height > otherBounds.y;

        if (isOverlapping) {
          console.warn("Kolízia s iným megakvetináčom. Pohyb zamietnutý.");
          e.target.x(originalX);
          e.target.y(originalY);
          return;
        }
      }

      megaPot.innerPots.forEach((innerPot) => {
        innerPot.x += dx;
        innerPot.y += dy;

        innerPot.innerPlants.forEach((plant) => {
          plant.x += dx;
          plant.y += dy;
          plant.potName = `Pot ${innerPot.x}-${innerPot.y}`;

          const rect = this.rectangles.find((r) => r.id === plant.id);
          if (rect) {
            rect.x = plant.x;
            rect.y = plant.y;
          }
        });
      });

      firstPot.x = targetX;
      firstPot.y = targetY;
    },
    // Funkcia na inkrementáciu alebo dekrementáciu stavov rastlín a posun aktuálneho dátumu
    adjustPlantStates(daysToAdvance = 1) {
      // Posunúť globálny dátum o požadovaný počet dní
      this.currentSimulatedDate.setDate(this.currentSimulatedDate.getDate() + daysToAdvance);
      this.currentSimulatorDay = this.currentSimulatedDate.toLocaleDateString();
      console.log(`Aktuálny dátum: ${this.currentSimulatedDate.toLocaleDateString()}`);

      this.plants.forEach(plant => {
        const rect = this.rectangles.find(r => r.id === plant.id);
        const plantOption = this.plantOptions.find(p => p.type === plant.type);

        if (!plantOption || !rect) return;

        // Spočítaj, koľko dní uplynulo od zasadenia rastliny
        //const plantPlantedDate = new Date(plant.datePlanted);
        const parts = plant.datePlanted.split('.').map(p => parseInt(p.trim(), 10));
        const plantPlantedDate = new Date(parts[2], parts[1] - 1, parts[0]); 
        const daysSincePlanted = Math.floor((this.currentSimulatedDate - plantPlantedDate) / (1000 * 60 * 60 * 24)); // rozdiel v dňoch
        console.log(`Dátum výsadby: ${plantPlantedDate.toLocaleDateString()}`);
        console.log(`Počet dní od výsadby: ${daysSincePlanted}`);
        console.log(`Počet dní potrebných na rast: ${plantOption.states[plant.state].days}`);

        // Inkrementácia - zväčšiť rastlinu, ak uplynul požadovaný počet dní
        if (daysToAdvance > 0 && plant.state < 2 && daysSincePlanted >= plantOption.states[plant.state].days) {
          this.showUploadDialog = true
          console.log("Current state", plant.state);
          console.log("States", plantOption.states);
          plant.state++;
          const newStage = plantOption.states[plant.state];

          // Aktualizuj rozmery rastliny aj rect
          plant.width = newStage.width;
          plant.height = newStage.height;

          rect.width = newStage.width;
          rect.height = newStage.height;
          changed = true
        }
        // Dekrementácia - zmenšiť rastlinu, ak uplynul menej dní než je potrebné
        else if (daysToAdvance < 0 && plant.state > 0 && daysSincePlanted < plantOption.states[plant.state].days) {
          console.log(plant.state);
          plant.state--;
          const previousStage = plantOption.states[plant.state];

          // Aktualizuj rozmery rastliny aj rect
          plant.width = previousStage.width;
          plant.height = previousStage.height;

          rect.width = previousStage.width;
          rect.height = previousStage.height;
          changed = true
        }
        this.centerPlantInPot(plant);
      });

      if (changed) {
          this.plants2 = [];

          for (const plant of this.plants){
            this.plants2.push({
              id: plant.id,
              name: plant.name,
              type: plant.type,
              states: plant.states,
              state: plant.state,
              width: plant.width,
              height: plant.height,
              p: plant,
              datePlanted: plant.datePlanted,
            });
          }
          this.plants = []
          this.rectangles = []

          if (this.plants2.length > 0) {      
            this.generateOptimalMegaPots();
          }

          for (const plant of this.plants2) {
            this.addRectangle(plant.p, false, plant.state, plant.datePlanted);
          }
        }

      changed = false

    },
    // Funkcia na manuálne posunutie času o konkrétny počet dní (pozitívne alebo negatívne)
    advanceSimulatedDate(days) {
      this.adjustPlantStates(days);
    },
    updatePlantInfoPosition() {
      const scrollTop = window.scrollY || document.documentElement.scrollTop;
      const scrollLeft = window.scrollX || document.documentElement.scrollLeft;
      // Dynamické nastavenie top pozície podľa skrolovania
      this.plantInfoStyle.top = `${10 + scrollTop}px`; // Adjustujte podľa potreby
      this.plantInfoStyle.right = `${10 - scrollLeft}px`;
    },
    setGreenhouseSize() {
      this.greenhouseName = this.greenhouseName.trim() || 'Default Greenhouse';

      this.greenhouseWidth = this.greenhouseWidth * 1000;
      this.greenhouseHeight = this.greenhouseHeight * 1000;

      this.stageSize = {
          width: this.greenhouseWidth ,
          height: this.greenhouseHeight,
      };
      
      this.plants2 = [];
      for (const plant of this.plantOptions) {
        const count = this.plantSelection[plant.id] || 0;
        for (let i = 0; i < count; i++) {
          this.plants2.push({
            id: `${plant.id}-${i}-${Date.now()}`,
            name: plant.name,
            type: plant.type,
            states: plant.states,
            state: 0, // mladá
            width: plant.states[0].width,
            height: plant.states[0].height
          });
        }
      }

      this.showGreenhouseDialog = false; // Skryť dialóg po nastavení hodnôt
      if (this.plants2.length > 0) {      
        this.generateOptimalMegaPots();

        for (const plant of this.plantOptions) {
          const count = this.plantSelection[plant.id] || 0;
          for (let i = 0; i < count; i++) {
            this.addRectangle(plant);
          }
        }
      }
      if (this.loaded == false){
        try {
          const request = {
            greenHouseID: crypto.randomUUID(),
            name: this.greenhouseName,
            width: this.greenhouseWidth,
            depth: this.greenhouseHeight,
            dateCreated: Date.now(),
          };
          GreenHouseService.createGreenHouse(request).then(response => {
          // napr. ulož názov skleníka
          this.ghouseid = response.data;
          //this.setGreenhouseSize();
        })
        } catch (error) {
          console.error('Nepodarilo sa vytvoriť skleník:', error);
        }
      }
    },
    // Funkcia na zavretie dialógu bez zmeny
    closeGreenhouseDialog() {
      this.showGreenhouseDialog = false;
    },
    createMegaPotByLayout() {
      if (!this.selectedLayout) {
        alert("Najprv vyber rozloženie!");
        return;
      }

      const { columns, rows, width, height } = this.selectedLayout;

      const startX = 100 + this.megaPots.length * 50;
      const startY = 100 + this.megaPots.length * 50;

      const newMegaPot = {
        id: crypto.randomUUID(),
        x: startX,
        y: startY,
        width: columns * width,
        height: rows * height,
        draggable: true,
        innerPots: [],
        rows: rows,
        columns: columns,
        shape: this.selectedLayout.shape || 'square',
        name: this.selectedLayout.label,
        dateCreated: new Date().toLocaleDateString('sk-SK'),
      };

      for (let row = 0; row < rows; row++) {
        for (let col = 0; col < columns; col++) {
          const innerPot = {
            x: startX + col * width,
            y: startY + row * height,
            width,
            height,
            shape: this.selectedLayout.shape || 'square', // <-- pridanie tvaru kvetináča
            innerPlants: [],
          };
          newMegaPot.innerPots.push(innerPot);
        }
      }

      this.megaPots.push(newMegaPot);
      this.selectedLayout = null;
      this.showDialog = false;

      console.log('Vytvorený megakvetináč:', newMegaPot);
    },
    handleContextDelMenu(megaPot, event) {
      event.evt.preventDefault(); // Zruší natívne kontextové menu

      // Môžeš pridať potvrdenie (voliteľne)
      if (!confirm(this.t('greenhouse.delete_confirm'))) return;

      // Odstrániť všetky rastliny, ktoré boli v tomto megakvetináči
      for (const pot of megaPot.innerPots) {
        for (const plant of pot.innerPlants) {
          this.rectangles = this.rectangles.filter((r) => r.id !== plant.id);
          this.plants = this.plants.filter((p) => p.id !== plant.id);
          this.plantPotMap.delete(plant.id);
        }
      }

      // Odstrániť samotný megakvetináč
      this.megaPots = this.megaPots.filter((mp) => mp !== megaPot);
    },
    handleContextMenu(item, evt) {
      evt.evt.preventDefault();
      this.removePlant(item.id);
    },
    removePlant(id) {
      // Odstráň z rectangles
      this.rectangles = this.rectangles.filter(item => item.id !== id);

      // Odstráň z plants
      const plantToRemove = this.plants.find(p => p.id === id);
      this.plants = this.plants.filter(p => p.id !== id);

      // Ak sa rastlina našla, pokúsime sa ju nájsť v kvetináčoch
      if (plantToRemove) {
        for (const megaPot of this.megaPots) {
          for (const pot of megaPot.innerPots) {
            pot.innerPlants = pot.innerPlants.filter(p => p.id !== id);
          }
        }
      }

      // Odstráň mapovanie z plantPotMap
      this.plantPotMap.delete(id);

      console.log(`Rastlina s ID ${id} bola úspešne odstránená.`);
    },
    openPotSizeDialog() {
      this.showPotSizeDialog = true;
    },
    closePotSizeDialog() {
      this.showPotSizeDialog = false;
    },
    selectPotSize(size) {
      this.selectedPotSize = size;
      this.showPotSizeDialog = false;
      this.openDialog();
    },
    createMegaPot() {
        const potWidth = this.selectedPotSize.width;
        const potHeight = this.selectedPotSize.height;
        const potShape = this.selectedPotSize.shape || 'square'; // <-- pridanie tvaru kvetináča

        const startX = 100 + this.megaPots.length * 50;
        const startY = 100 + this.megaPots.length * 50;

        // Nový megakvetináč s prázdnym zoznamom vnútorných kvetináčov
        const newMegaPot = {
          x: startX,
          y: startY,
          width: this.columns * potWidth,
          height: this.rows * potHeight,
          draggable: true,
          innerPots: [], // <-- Sem budeme pushovať vnútorné kvetináče//
          shape: potShape, // <-- sem pridávame shape
          rows: this.rows || 1,
          columns: this.columns || 1,
        };

        // Generovanie vnútorných kvetináčov pre tento megakvetináč
        for (let row = 0; row < this.rows; row++) {
          for (let col = 0; col < this.columns; col++) {
            const innerPot = {
              x: startX + col * potWidth,
              y: startY + row * potHeight,
              width: potWidth,
              height: potHeight,
              shape: potShape, // <-- sem pridávame shape
              innerPlants: [], // <-- Sem budeme pushovať rastliny
            };
            newMegaPot.innerPots.push(innerPot); // <-- Ukladáme ich priamo do newMegaPot
          }
        }

        this.megaPots.push(newMegaPot); // Pushujeme celý objekt vrátane vnútorných kvetináčov
        this.showDialog = false;
        console.log('Vytvorený megakvetináč:', newMegaPot);
        console.log('Vnútorné kvetináče:', newMegaPot.innerPots);
      },
      centerPlantInPot(plant) {
        // Nájdeme pot, kde je rastlina uložená
        const pot = this.megaPots.flatMap(megaPot => megaPot.innerPots).find(pot => pot.innerPlants.includes(plant));

        if (pot) {
          const x = pot.x + (pot.width - plant.width) / 2;
          const y = pot.y + (pot.height - plant.height) / 2;

          // Aktualizujeme pozíciu rastliny
          plant.x = x;
          plant.y = y;
          plant.potName = `Pot ${pot.x}-${pot.y}`;

          // Pridáme do kvetináča, ak už nie je
          if (!pot.innerPlants.includes(plant)) {
            pot.innerPlants.push(plant);
          }

          const rect = this.rectangles.find(r => r.id === plant.id);
          if (rect) {
            rect.x = x;
            rect.y = y;
            rect.fill = 'green';  // Môžeš upravit farbu podľa potreby
          }

          console.log(`Rastlina '${plant.name}' bola vycentrovaná do ${plant.potName}`);
        } else {
          console.warn('Rastlina nebola nájdená v žiadnom kvetináči.');
        }
      },    
      incrementPlantState(plantId) {
        const plant = this.plants.find(p => p.id === plantId);
        const rect = this.rectangles.find(r => r.id === plantId);

        if (!plant || !rect) return;

        const plantOption = this.plantOptions.find(p => p.type === plant.type);
        if (!plantOption) return;

        // Inkrementuj stav, ale max. po 2 (indexy 0-2)
        if (plant.state < 2) {
          plant.state++;
          const newStage = plantOption.states[plant.state];

          // Aktualizuj rozmery rastliny aj rect
          plant.width = newStage.width;
          plant.height = newStage.height;

          rect.width = newStage.width;
          rect.height = newStage.height;
        }
      },
      // Pridá rastlinu na základe vybraného typu
    addRectangle(plantOption, next = true, tmpState = 0, tmpDate = new Date().toLocaleDateString('sk-SK')) {
      let plantName = '';
      if (next){
        plantName = plantOption.name + ' ' + this.nextId;
      }else{
        plantName = plantOption.name;
      }
      
      const newRect = {
        id: this.nextId,
        x: Math.random() * (width - plantOption.states[0].width),
        y: Math.random() * (height - plantOption.states[0].height),
        width: plantOption.states[tmpState].width,
        height: plantOption.states[tmpState].height,
        fill: 'red',
        name: plantName,
        draggable: true,
        type: plantOption.type,
      };

      const plantInfo = {
        id: this.nextId,
        name: plantName,
        type: plantOption.type,
        states: plantOption.states,
        species: 'Neznáma',
        light: 'Stredné',
        soil: 'Univerzálna',
        potName: 'Not yet',
        x: newRect.x,
        y: newRect.y,
        width: newRect.width,
        height: newRect.height,
        datePlanted: tmpDate,
        state: tmpState,
      };

      console.log(plantInfo)

      this.rectangles.push(newRect);
      this.plants.push(plantInfo);
      this.nextId++;
      this.showPlantList = false; // Zavrie zoznam po výbere
      this.placePlantInFirstFreePot(plantInfo); // Pokúsi sa umiestniť rastlinu do prvého voľného kvetináča
    },
    openDialog() {
      this.showDialog = true;
    },
    closeDialog() {
      this.showDialog = false;
    },
  // Vytvorí megakvetináč podľa zadaných stĺpcov a riadkov

  // Presúvanie megakvetináča
  handleMegaPotMove(e) {
    console.log("Presúvame megakvetináč");
    
    // Nájdeme megakvetináč, ktorý presúvame
    const megaPot = this.megaPots[0]; // Predpokladáme prvý megakvetináč
    const dx = e.target.x() - megaPot.x;
    const dy = e.target.y() - megaPot.y;

    // Aktualizujeme pozíciu megakvetináča
    megaPot.x = e.target.x();
    megaPot.y = e.target.y();

    // Posunieme všetky vnútorné kvetináče
    this.innerPots.forEach((innerPot) => {
      innerPot.x += dx;
      innerPot.y += dy;
    });

    // Posunieme všetky rastliny, ktoré patria do týchto kvetináčov
    this.rectangles.forEach((rect) => {
      const associatedPot = this.plantPotMap.get(rect.id);
      if (associatedPot && this.isInsideMegaPot(associatedPot)) {
        // Presunieme rastlinu o rovnakú vzdialenosť ako megakvetináč
        rect.x += dx;
        rect.y += dy;

        // Aktualizujeme aj informácie o rastline
        const plant = this.plants.find((p) => p.id === rect.id);
        if (plant) {
          plant.x = rect.x;
          plant.y = rect.y;
          plant.potName = `Pot ${associatedPot.x}-${associatedPot.y}`;
        }
      }
    });
  },

  // Funkcia na kontrolu, či je kvetináč v rámci megakvetináča
  isInsideMegaPot(pot) {
    const megaPot = this.megaPots[0]; // Zoberieme prvý megakvetináč
    return (
      pot.x >= megaPot.x &&
      pot.y >= megaPot.y &&
      pot.x + pot.width <= megaPot.x + megaPot.width &&
      pot.y + pot.height <= megaPot.y + megaPot.height
    );
  },
  // Presúvanie rastlín
  handleDragMove(e) {
    const rect = this.rectangles.find((r) => r.name === e.target.name());
    rect.x = e.target.x();
    rect.y = e.target.y();

    const plant = this.plants.find((p) => p.id === rect.id);
    if (plant) {
      plant.x = rect.x;
      plant.y = rect.y;
    }

    let potFound = false;

    for (const megaPot of this.megaPots) {
      for (const pot of megaPot.innerPots) {
        const centerX = rect.x + rect.width / 2;
        const centerY = rect.y + rect.height / 2;

        const insidePot =
          centerX > pot.x &&
          centerX < pot.x + pot.width &&
          centerY > pot.y &&
          centerY < pot.y + pot.height;

        const fitsInPot =
          rect.width <= pot.width &&
          rect.height <= pot.height;

        if (insidePot && fitsInPot && pot.innerPlants.length === 0) {
          rect.fill = 'green';

          // Vycentrovanie
          rect.x = pot.x + (pot.width - rect.width) / 2;
          rect.y = pot.y + (pot.height - rect.height) / 2;

          // Odstránime rastlinu z predchádzajúcich kvetináčov
          for (const mPot of this.megaPots) {
            for (const iPot of mPot.innerPots) {
              iPot.innerPlants = iPot.innerPlants.filter((rp) => rp.id !== plant.id);
            }
          }

          // Uloženie rastliny do nového kvetináča
          pot.innerPlants.push(plant);
          this.plantPotMap.set(rect.id, pot);

          plant.x = rect.x;
          plant.y = rect.y;
          plant.potName = `Pot ${pot.x}-${pot.y}`;

          potFound = true;
          break;
        }
      }
      if (potFound) break;
    }

    // Ak nenašiel vhodný kvetináč
    if (!potFound) {
      rect.fill = 'red';

      const plantInfo = this.plants.find((p) => p.id === rect.id);
      if (plantInfo) {
        plantInfo.x = rect.x;
        plantInfo.y = rect.y;
        plantInfo.potName = `Empty`;
      }

      for (const megaPot of this.megaPots) {
        for (const pot of megaPot.innerPots) {
          pot.innerPlants = pot.innerPlants.filter((rp) => rp.id !== rect.id);
        }
      }

      this.plantPotMap.delete(rect.id);
    }
  },

  // Presúvanie rastlín
  handleDragEnd(e) {
    const rect = this.rectangles.find((r) => r.name === e.target.name());
    rect.x = e.target.x();
    rect.y = e.target.y();

    const plant = this.plants.find((p) => p.id === rect.id);
    if (plant) {
      plant.x = rect.x;
      plant.y = rect.y;
    }
  },
  placePlantInFirstFreePot(plant) {
    for (const megaPot of this.megaPots) {
      for (const pot of megaPot.innerPots) {
        const potIsEmpty = pot.innerPlants.length === 0;
        const fitsInPot = plant.width <= pot.width && plant.height <= pot.height;

        if (potIsEmpty && fitsInPot) {
          // Vycentrujeme rastlinu do stredu kvetináča
          const x = pot.x + (pot.width - plant.width) / 2;
          const y = pot.y + (pot.height - plant.height) / 2;

          // Aktualizujeme pozíciu rastliny
          plant.x = x;
          plant.y = y;
          plant.potName = `Pot ${pot.x}-${pot.y}`;

          // Pridáme do kvetináča
          pot.innerPlants.push(plant);

          const rect = this.rectangles.find(r => r.id === plant.id);
          if (rect) {
            rect.x = x;
            rect.y = y;
            rect.fill = 'green';
          }

          // Aktualizujeme mapovanie
          this.plantPotMap.set(plant.id, pot);

          console.log(`Rastlina '${plant.name}' pridaná do ${plant.potName}`);
          return true; // úspešné vloženie
        }
      }
    }

    console.warn('Žiadny vhodný voľný kvetináč pre novú rastlinu!');
    return false;
  },
  generateOptimalMegaPots(allPlants = this.plants2) {
    let bestLayout = null;
    let bestFitCount = 0;
    let smallestArea = Infinity;
    this.megaPots = []; // Resetujeme existujúce megakvetináče

    for (const layout of this.predefinedLayouts) {
      const { width, height, columns, rows } = layout;

      const fittingPlants = allPlants.filter(plant => {
        const stage = plant.states[plant.state];
        return stage.width <= width && stage.height <= height;
      });

      const maxSpots = columns * rows;
      const fitCount = Math.min(fittingPlants.length, maxSpots);

      const totalWidth = columns * width;
      const totalHeight = rows * height;
      const area = totalWidth * totalHeight;

      if (
        fitCount > bestFitCount ||
        (fitCount === bestFitCount && area < smallestArea)
      ) {
        bestFitCount = fitCount;
        bestLayout = layout;
        smallestArea = area;
      }
    }

    if (!bestLayout) {
      console.warn('⚠️ Žiadny layout nie je vhodný pre aktuálne rastliny.');
      return;
    }

    this.selectedLayout = bestLayout;
    console.log(`Automaticky zvolený layout: ${bestLayout.label}`);
    this.createMegaPotByLayout();
  },
  isPotOccupied(pot) {
    return this.rectangles.some(
      (r) => r.type === 'plant' && this.plantPotMap.get(r.id) === pot
    );
  },
  handleUploadAndCreate() {
    if (this.selectedFile && this.columnsImage > 0 && this.rowsImage > 0) {
      // Tu môžeš nahrať obrázok a spracovať layout
      console.log('Súbor:', this.selectedFile);
      console.log('Stĺpce:', this.columnsImage);
      console.log('Riadky:', this.rowsImage);
      this.showUploadDialog = false;
    } else {
      this.$q.notify({
        type: 'warning',
        message: 'Vyplňte všetky údaje a nahrajte súbor'
      });
    }
  },
  saveStatus() {
    console.log("saving status", this.plantPotMap);
    // console.log("saving pots", this.megaPots[0].rows, this.megaPots[0].columns, this.megaPots[0].innerPots[0].x, this.megaPots[0].innerPots[0].y, this.megaPots[0].name);
    this.megaPots.forEach(async pot => {
      console.log("Saving pot:", pot.dateCreated);
      let parsedDate;
      try {
        const [day, month, year] = pot.dateCreated.split('.').map(s => parseInt(s.trim()));
        parsedDate = new Date(year, month - 1, day);
      } catch {
        console.error("Neplatný dátum:", pot.dateCreated);
        return;
      }

      const isoDate = parsedDate.toISOString();
      const payload = {
        editorBoardId: pot.id,
        name: pot.name,
        rows: pot.rows,
        columns: pot.columns,
        shape: pot.shape,
        posX: Math.round(pot.innerPots[0].x),
        posY: Math.round(pot.innerPots[0].y),
        width: pot.width || 0,
        height: pot.height || 0,
        dateCreated: isoDate,
        greenHouseId: this.ghouseid,
        greenHouseId1: this.ghouseid,
      };

      try {
        const response = await GreenHouseService.createEditorPot(payload);
        console.log("Saved pot:", response);
      } catch (error) {
        console.error("Failed to save pot:", error);
      }
    });

    /*PLANTS*/
    console.log("saving plants");
    this.plants.forEach(async plant => {
      let parsedDate;
      try {
        const [day, month, year] = plant.datePlanted.split('.').map(s => parseInt(s.trim()));
        parsedDate = new Date(year, month - 1, day);
      } catch {
        console.error("Neplatný dátum:", plant.datePlanted);
        return;
      }

      const isoDate = parsedDate.toISOString();
      const payload = [{
        name: plant.name,
        type: plant.type,
        species: plant.species,
        light: plant.light,
        soil: plant.soil,
        potName: plant.potName,
        posX: Math.round(plant.x),
        posY: Math.round(plant.y),
        width: plant.width,
        height: plant.height,
        dateCreated: isoDate,
        currentState: plant.state.toString(),
        currentDay: plant.state,
        editorBoardId: plant.editorBoardId ?? '',
        greenHouseId: this.ghouseid,
        plantId: plant.id.toString(),
        stage: 'stage',
      }];

      try {
        const response = await GreenHouseService.createEditorPlants(payload);
        console.log("Saved plant:", response);
      } catch (error) {
        console.error("Failed to save plant:", error);
      }
    });
  },
  assignPlantToPot(plant, rect) {
    const centerX = rect.x + rect.width / 2;
    const centerY = rect.y + rect.height / 2;

    for (let i = 0; i < this.megaPots.length; i++) {
      const megaPot = this.megaPots[i];

      for (let j = 0; j < megaPot.innerPots.length; j++) {
        const pot = megaPot.innerPots[j];

        const insidePot =
          centerX > pot.x &&
          centerX < pot.x + pot.width &&
          centerY > pot.y &&
          centerY < pot.y + pot.height;

        const fitsInPot =
          rect.width <= pot.width &&
          rect.height <= pot.height;

        if (insidePot && fitsInPot && pot.innerPlants.length === 0) {
          // Vycentruj rect do potu
          rect.x = pot.x + (pot.width - rect.width) / 2;
          rect.y = pot.y + (pot.height - rect.height) / 2;

          plant.x = rect.x;
          plant.y = rect.y;
          plant.potName = `Pot ${pot.x}-${pot.y}`;

          // Priamy push cez indexy
          this.megaPots[i].innerPots[j].innerPlants.push(plant);
          this.plantPotMap.set(rect.id, this.megaPots[i].innerPots[j]);

          rect.fill = 'green';
          return;
        }
      }
    }

    // Ak nenašlo vhodný pot
    rect.fill = 'red';
    plant.potName = `Empty`;
    this.plantPotMap.delete(rect.id);
  },
  saveState() {
    const stateToSave = {
      ...this.$data,
      plantPotMap: Array.from(this.plantPotMap.entries()),
      currentSimulatedDate: this.currentSimulatedDate.toISOString(),
    };
    localStorage.setItem(this.ghouseid, JSON.stringify(stateToSave));

    this.saveStatus();
  },
  async loadState() {

    try {
      const response = await GreenHouseService.getBoardsByGreenHouseId(this.ghouseid);
      console.log("Loaded pot:", response);

      response.data.forEach(pot => {
        const parsedDate = new Date(pot.dateCreated);
        const isoDate = parsedDate.toISOString('sk-SK');
        const payload = {
          id: pot.id,
          name: pot.name,
          rows: pot.rows,
          columns: pot.columns,
          shape: pot.shape,
          x: Math.round(pot.posX),
          y: Math.round(pot.posY),
          width: pot.width || 0,
          height: pot.height || 0,
          dateCreated: isoDate,
          draggable: true,
          innerPots: [],
        };

        for (let row = 0; row < payload.rows; row++) {
          for (let col = 0; col < payload.columns; col++) {
            const innerPot = {
              x: payload.x + (col * payload.width / payload.columns),
              y: payload.y + (row * payload.height / payload.rows),
              width: payload.width / payload.columns,
              height: payload.height / payload.rows,
              shape: payload.shape || 'square',
              innerPlants: [],
            };
            payload.innerPots.push(innerPot);
          }
        }
        this.megaPots.push(payload);
      });

    } catch (error) {
      console.error("Failed to load pots:", error);
    }

    try {
      const response = await GreenHouseService.getPlantsByGreenHouseId(this.ghouseid);

      response.data.forEach(plant => {
        const parsedDate = new Date(plant.dateCreated);
        const isoDate = parsedDate.toISOString('sk-SK');
        const payload = {
          id: parseInt(plant.plantID),
          name: plant.name,
          type: plant.type,
          x: Math.round(plant.posX),
          y: Math.round(plant.posY),
          stage: plant.stage,
          currentDay: plant.currentDay,
          state: parseInt(plant.currentState),
          width: plant.width,
          height: plant.height,
          datePlanted: new Date(isoDate).toLocaleDateString('sk-SK'),
        };
        this.plants.push(payload);

        const newRect = {
          id: parseInt(payload.id),
          x: payload.x,
          y: payload.y,
          width: payload.width,
          height: payload.height,
          fill: 'red',
          name: payload.name,
          draggable: true,
          type: payload.type,
        };

        this.rectangles.push(newRect);
        this.assignPlantToPot(payload, newRect);
      });
    } catch (error) {
      console.error("Failed to load plant:", error);
    }
    /*const saved = localStorage.getItem(this.ghouseid);
    if (saved) {
      const parsed = JSON.parse(saved);

      Object.keys(parsed).forEach((key) => {
        if (key === 'plantPotMap') {
          this.plantPotMap = new Map(parsed.plantPotMap);
        } else if (key === 'currentSimulatedDate') {
          this.currentSimulatedDate = new Date(parsed[key]);
        } else {
          this[key] = parsed[key];
        }
      });

      this.plantPotMap = new Map(parsed.plantPotMap);
      this.currentSimulatedDate = new Date(parsed.currentSimulatedDate);
    }*/
  }
},
};
</script>

<style scoped>
.dialog {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000; /* Zabezpečí, že dialóg bude nad canvasom */
}

.dialog-content {
  background-color: white;
  padding: 20px;
  border-radius: 5px;
  text-align: center;
}

.plant-info {
  position: absolute;
  top: 10px;
  right: 10px;
  margin-top: 50px;
  background: white;
  padding: 10px;
  border: 1px solid black;
  transition: top 0.2s ease-out; /* Pre hladký pohyb */
}

.plant-info-scroll {
  max-height: 220px; /* nastavíme menšiu výšku pre samotný obsah */
  overflow-y: auto;  /* zapneme scrollovanie */
  padding-right: 10px; /* priestor pre scrollbar */
}
</style>
