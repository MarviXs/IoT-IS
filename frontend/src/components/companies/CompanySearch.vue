<template>
  <q-select v-model="selected" :options="filteredOptions" use-input @filter="filterFn" label="Search for companies" />
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import type { ARESQueryParams, EkonomickySubjekt } from '@/api/services/ARESService';
import ARESService from '@/api/services/ARESService';

const selected = ref<string | null>(null);

const filteredOptions = ref<string[]>([]);

const receivedCompanies = ref<EkonomickySubjekt[]>([]);

const selectedCompany = defineModel<EkonomickySubjekt | null>();

watch(selected, (val) => {
  if (!val) {
    return;
  }
  selectedCompany.value = receivedCompanies.value.find((company) => company.obchodniJmeno === val);
});

function filterFn(val: string, update: any, abort: () => void) {
  if (!val || val.length < 3) {
    abort();
    return;
  }
  const query: ARESQueryParams = {
    start: 0,
    pocet: 5,
    obchodniJmeno: undefined,
    ico: undefined,
  };

  if (/^\d+$/.test(val)) {
    query.ico = val;
  } else {
    query.obchodniJmeno = val;
  }
  ARESService.getCompanies(query).then((response) => {
    receivedCompanies.value = response.ekonomickeSubjekty;
    response.ekonomickeSubjekty.forEach((company) => {
      filteredOptions.value.push(company.obchodniJmeno);
    });
  });
  setTimeout(() => {
    update(() => {});
  }, 500);
}
</script>
