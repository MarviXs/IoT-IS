<template>
  <q-card class="orders-summary-card">
    <!-- Hlavička s názvom -->
    <q-card-section class="header-section">
      <div class="text-h6">{{ t('order_summary.summary_title') }}</div>
    </q-card-section>

    <q-separator />

    <!-- Sekcia so súhrnnými údajmi -->
    <q-card-section class="summary-section">
      <div class="summary-grid">
        <!-- Prvý riadok: názvy stĺpcov pre hodnoty -->
        <div class="summary-cell header-label"></div>
        <div class="summary-cell header-value">{{ t('order_summary.price_excl_vat_label') }}</div>
        <div class="summary-cell header-value">{{ t('order_summary.vat_label') }}</div>
        <div class="summary-cell header-value">{{ t('order_summary.price_incl_vat_label') }}</div>
        
        <!-- Druhý riadok pre sadzbu DPH 12% -->
        <div class="summary-cell summary-label">{{ t('order_summary.vat_rate_12') }}</div>
        <div class="summary-cell summary-value">{{ summary.vat12.priceExclVAT }}</div>
        <div class="summary-cell summary-value">{{ summary.vat12.vat }}</div>
        <div class="summary-cell summary-value">{{ summary.vat12.priceInclVAT }}</div>

        <!-- Tretí riadok pre sadzbu DPH 21% -->
        <div class="summary-cell summary-label">{{ t('order_summary.vat_rate_21') }}</div>
        <div class="summary-cell summary-value">{{ summary.vat21.priceExclVAT }}</div>
        <div class="summary-cell summary-value">{{ summary.vat21.vat }}</div>
        <div class="summary-cell summary-value">{{ summary.vat21.priceInclVAT }}</div>

        <!-- Štvrtý riadok pre celkovú sumu -->
        <div class="summary-cell summary-label total" style="grid-column: span 3">
          {{ t('order_summary.total') }}
        </div>
        <div class="summary-cell summary-value total">
          {{ summary.total }}
        </div>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
import { defineProps } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

// Definícia typu pre súhrn objednávky
interface OrderSummary {
  vat12: {
    priceExclVAT: number;
    vat: number;
    priceInclVAT: number;
  };
  vat21: {
    priceExclVAT: number;
    vat: number;
    priceInclVAT: number;
  };
  total: number;
}

// Súhrnné údaje budú prichádzať cez prop
const props = defineProps<{
  summary: OrderSummary;
}>();
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
  padding: 12px;
  background-color: #f5f5f5;
  border-bottom: 1px solid #ddd;
}

.summary-section {
  padding: 10px;
}

/* Použijeme CSS grid na zobrazenie údajov v niekoľkých riadkoch */
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

/* Riadok s názvami stĺpcov */
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

/* Štýly pre údaje */
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
</style>
