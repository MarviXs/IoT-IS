import colorLib, { Color, RGBA } from '@kurkle/color';

export const graphColors = [
  '#3366cc',
  '#dc3912',
  '#ff9900',
  '#109618',
  '#990099',
  '#0099c6',
  '#dd4477',
  '#66aa00',
  '#b82e2e',
  '#316395',
  '#3366cc',
  '#994499',
  '#22aa99',
  '#aaaa11',
  '#6633cc',
  '#e67300',
  '#8b0707',
  '#651067',
  '#329262',
  '#5574a6',
  '#3b3eac',
  '#b77322',
  '#16d620',
  '#b91383',
  '#f4359e',
  '#9c5935',
  '#a9c413',
  '#2a778d',
  '#668d1c',
  '#bea413',
  '#0c5922',
  '#743411',
];

export const notificationColors: Record<string, string> = {
  Info: 'primary',
  Warning: 'orange-9',
  Serious: 'red-7',
  Critical: 'negative',
};

export function getGraphColor(index: number): string {
  const colorCount = graphColors.length;
  return graphColors[index % colorCount];
}

export function transparentize(value: string | number[] | Color | RGBA, opacity: number) {
  const alpha = opacity === undefined ? 0.5 : 1 - opacity;
  return colorLib(value).alpha(alpha).rgbString();
}
