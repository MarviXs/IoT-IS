<template>
  <div class="chart-wrapper">
    <canvas ref="canvasRef"></canvas>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { Chart, type ChartDataset } from 'chart.js/auto';
import type { JobComparisonSeries } from '@/models/JobComparison';

const props = defineProps<{
  series: JobComparisonSeries[];
  xLabel: string;
  yLabel: string;
}>();

type ChartPoint = { x: number; y: number | null };

const canvasRef = ref<HTMLCanvasElement | null>(null);
const chart = ref<Chart<'line', ChartPoint[]>>();

function buildDatasets(): ChartDataset<'line', ChartPoint[]>[] {
  return props.series.map((serie) => ({
    label: serie.label,
    data: serie.data,
    borderColor: serie.color,
    backgroundColor: serie.color,
    fill: false,
    tension: 0.2,
    pointRadius: 0,
    borderWidth: 2,
    parsing: false,
  }));
}

function createChart() {
  if (!canvasRef.value) {
    return;
  }

  chart.value = new Chart(canvasRef.value, {
    type: 'line',
    data: {
      datasets: buildDatasets(),
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      interaction: {
        intersect: false,
        mode: 'nearest',
      },
      scales: {
        x: {
          type: 'linear',
          title: {
            display: true,
            text: props.xLabel,
          },
          beginAtZero: true,
        },
        y: {
          title: {
            display: true,
            text: props.yLabel,
          },
        },
      },
      plugins: {
        legend: {
          display: true,
        },
        tooltip: {
          callbacks: {
            title(context) {
              if (!context.length) {
                return '';
              }
              const point = context[0].parsed;
              return `${props.xLabel}: ${point.x.toLocaleString()}`;
            },
          },
        },
      },
    },
  });
}

function updateChart() {
  if (!chart.value) {
    return;
  }

  chart.value.data.datasets = buildDatasets();
  const xScale = chart.value.options.scales?.x;
  const yScale = chart.value.options.scales?.y;
  if (xScale && 'title' in xScale) {
    xScale.title = xScale.title ?? {};
    xScale.title.display = true;
    xScale.title.text = props.xLabel;
  }
  if (yScale && 'title' in yScale) {
    yScale.title = yScale.title ?? {};
    yScale.title.display = true;
    yScale.title.text = props.yLabel;
  }
  chart.value.update();
}

watch(
  () => [props.series, props.xLabel, props.yLabel],
  () => {
    if (!chart.value) {
      return;
    }
    updateChart();
  },
  { deep: true },
);

onMounted(() => {
  createChart();
});

onBeforeUnmount(() => {
  chart.value?.destroy();
});
</script>

<style scoped>
.chart-wrapper {
  width: 100%;
  height: 400px;
}

canvas {
  width: 100%;
  height: 400px;
}
</style>
