<template>
  <q-card class="orders-summary-card">
    <!-- Hlavička s názvom -->
    <q-card-section class="header-section">
      <div class="text-h6">{{ t('order_summary.summary_title') }}</div>
    </q-card-section>

    <q-separator />

    <!-- Obsah súhrnu v prechodovom elemente -->
    <q-slide-transition>
      <div v-show="isSummaryOpen">
        <q-card-section class="summary-section">
          <div class="summary-grid">
            <!-- Prvý riadok: názvy stĺpcov pre hodnoty -->
            <div class="summary-cell header-label"></div>
            <div class="summary-cell header-value">{{ t('order_summary.price_excl_vat_label') }}</div>
            <div class="summary-cell header-value">{{ t('order_summary.vat_label') }}</div>
            <div class="summary-cell header-value">{{ t('order_summary.price_incl_vat_label') }}</div>

            <!-- Druhý riadok pre DPH 12 % (vatReduced) -->
            <div class="summary-cell summary-label">{{ t('order_summary.vat_rate_12') }}</div>
            <div class="summary-cell summary-value">{{ summary.vatReduced.priceExcVat }}</div>
            <div class="summary-cell summary-value">{{ summary.vatReduced.vat }}</div>
            <div class="summary-cell summary-value">{{ summary.vatReduced.priceInclVat }}</div>

            <!-- Tretí riadok pre DPH 21 % (vatNormal) -->
            <div class="summary-cell summary-label">{{ t('order_summary.vat_rate_21') }}</div>
            <div class="summary-cell summary-value">{{ summary.vatNormal.priceExcVat }}</div>
            <div class="summary-cell summary-value">{{ summary.vatNormal.vat }}</div>
            <div class="summary-cell summary-value">{{ summary.vatNormal.priceInclVat }}</div>

            <!-- Štvrtý riadok pre celkovú sumu -->
            <div class="summary-cell summary-label total" style="grid-column: span 3">
              {{ t('order_summary.total') }}
            </div>
            <div class="summary-cell summary-value total">
              {{ summary.total }}
            </div>
          </div>
        </q-card-section>
      </div>
    </q-slide-transition>

    <!-- Pätička karty s toggle tlačidlom, zníženým paddingom -->
    <q-card-actions class="small-actions" align="center">
      <q-btn
        flat
        round
        size="sm"
        :icon="isSummaryOpen ? mdiChevronUp : mdiChevronDown"
        color="grey-color"
        @click="toggleSummary"
      />
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, defineExpose } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiChevronUp, mdiChevronDown } from '@quasar/extras/mdi-v7';
import OrdersService from '@/api/services/OrdersService';

// Aktualizovaná definícia typu pre props:
interface Props {
  orderId: string;
  refreshKey: number;
}

const props = defineProps<Props>();
const { t } = useI18n();

// Inicializácia summary s predvolenými hodnotami
interface VatSummary {
  priceExcVat: number;
  vat: number;
  priceInclVat: number;
}

interface OrderSummary {
  vatReduced: VatSummary;
  vatNormal: VatSummary;
  total: number;
}

const summary = ref<OrderSummary>({
  vatReduced: { priceExcVat: 0, vat: 0, priceInclVat: 0 },
  vatNormal: { priceExcVat: 0, vat: 0, priceInclVat: 0 },
  total: 0,
});

// Riadi zobrazenie obsahu súhrnu
const isSummaryOpen = ref(true);

function toggleSummary() {
  isSummaryOpen.value = !isSummaryOpen.value;
}

async function loadSummary() {
  try {
    const response = await OrdersService.getSummary(props.orderId) as { data: OrderSummary };
    if (response.data && typeof response.data === 'object') {
      summary.value = response.data;
    }
  } catch (error) {
    console.error('Chyba pri načítaní súhrnu objednávky:', error);
  }
}

// Exponujeme metódu getSummary, aby ju bolo možné volať z rodičovského komponentu
defineExpose({
  getSummary: loadSummary,
});

// Načítať summary pri mount a keď sa zmení orderId
onMounted(() => {
  loadSummary();
});

watch(() => props.orderId, () => {
  loadSummary();
});

// Pridáme watcher pre refreshKey; keď sa refreshKey zmení, načíta sa summary znovu.
watch(() => props.refreshKey, () => {
  loadSummary();
});
</script>

<style lang="scss" scoped>
.orders-summary-card {
  width: 100%;
  margin: 20px auto;
  border: 1px solid #ddd;
  border-radius: 8px;
  background-color: #ffffff;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
}

.header-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background-color: #f5f5f5;
  border-bottom: 1px solid #ddd;
}

.text-h6 {
  font-size: 1.2em;
  font-weight: bold;
  color: #333;
}

.summary-section {
  padding: 10px;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  grid-row-gap: 8px;
  grid-column-gap: 16px;
  align-items: center;
}

.summary-cell {
  padding: 4px 8px;
  font-size: 0.9em;
}

.header-label,
.header-value {
  font-weight: bold;
  color: #333;
}

.header-value {
  text-align: right;
  border-bottom: 1px solid #ddd;
  padding-bottom: 4px;
}

.summary-label {
  font-weight: 600;
  color: #444;
}

.summary-value {
  font-weight: 400;
  color: #666;
  text-align: right;
}

.total {
  font-size: 1em;
  font-weight: bold;
}

/* Trieda pre redukovaný padding v pätičke */
.small-actions {
  padding: 4px !important;
}
</style>
