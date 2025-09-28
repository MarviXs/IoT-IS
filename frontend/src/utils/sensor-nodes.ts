import type { DeviceResponse } from '@/api/services/DeviceService';
import type { DeviceCollectionWithSensorsResponse } from '@/api/services/DeviceCollectionService';
import type { SensorData } from '@/models/SensorData';
import type { SensorNode } from '@/models/SensorNode';

function getSensorUniqueId(deviceId: string, tag: string) {
  return `${deviceId}-${tag}`;
}

function deviceToTreeNode(device: DeviceResponse): SensorNode {
  return {
    id: device.id,
    name: device.name,
    type: 'Device',
    items:
      device.deviceTemplate?.sensors?.map((sensor) => ({
        id: getSensorUniqueId(device.id, sensor.tag),
        name: sensor.name,
        type: 'Sensor',
        sensor: sensor,
        items: [],
      })) ?? [],
  };
}

function collectionToTreeNode(coll: DeviceCollectionWithSensorsResponse): SensorNode {
  return {
    id: coll.id,
    name: coll.name,
    type: coll.type,
    items: coll.items.map((item) => {
      if (item.type === 'Sensor') {
        return {
          id: getSensorUniqueId(coll.id, item.sensor.tag),
          name: item.sensor.name,
          type: 'Sensor',
          sensor: item.sensor,
          items: [],
        };
      }
      return collectionToTreeNode(item);
    }),
  };
}

function treeNodeToSensors(node: SensorNode): SensorData[] {
  if (!node.items) {
    return [];
  }
  return node.items.flatMap((item) => {
    if (item.type === 'Sensor') {
      return [
        {
          id: item.id,
          deviceId: node.id ?? '',
          tag: item.sensor?.tag ?? '',
          name: item.name,
          unit: item.sensor?.unit,
          accuracyDecimals: item.sensor?.accuracyDecimals,
        },
      ];
    }
    return treeNodeToSensors(item);
  });
}

function extractNodeKeys(node: SensorNode): string[] {
  if (!node.items) {
    return [node.id ?? ''];
  }
  return [node.id ?? '', ...node.items.flatMap(extractNodeKeys)];
}

export { deviceToTreeNode, collectionToTreeNode, treeNodeToSensors, extractNodeKeys };
