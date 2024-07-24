<template>
  <q-btn
    no-wrap
    dense
    class="full-width bg-white"
    :color="color"
    padding="0.6rem 0.8rem"
    size="13px"
    outline
    :disable="disable"
    :loading="loading"
  >
    <q-icon left size="1.5em" :name="icon" />
    <div>{{ props.label }}</div>
  </q-btn>
</template>

<script setup lang="ts">
const props = defineProps({
  label: {
    type: String,
    required: true,
  },
  color: {
    type: String,
    required: true,
  },
  icon: {
    type: String,
    required: true,
  },
  disable: Boolean,
  loading: Boolean,
});
</script>
