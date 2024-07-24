// useInterval.ts

import { onMounted, onUnmounted, ref, watch, Ref } from 'vue';

/**
 * A Vue composable to handle dynamic intervals.
 * @param callback - The function to be executed after each interval.
 * @param delayRef - A ref holding the delay time in milliseconds between executions of the specified function.
 */
export function useInterval(callback: () => void, delayRef: Ref<number>) {
  const intervalId = ref<NodeJS.Timeout | null>(null);

  const start = () => {
    stop(); // Clear any existing intervals
    if (delayRef.value > 0) {
      intervalId.value = setInterval(callback, delayRef.value);
    }
  };

  const stop = () => {
    if (intervalId.value) {
      clearInterval(intervalId.value);
      intervalId.value = null;
    }
  };

  // React to changes in the delay time
  watch(delayRef, () => {
    start(); // Restart the interval with the new delay time
  });

  // Automatically start the interval on component mount and clean up on unmount
  onMounted(start);
  onUnmounted(stop);

  // Expose functionality to start and stop the interval manually
  return { start, stop };
}
