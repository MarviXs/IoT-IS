import { DeviceResponse } from '@/api/types/Device';
import { SensorNode } from '@/models/SensorNode';

function getSensorUniqueId(deviceId: string, tag: string) {
  return `${deviceId}-${tag}`;
}

function deviceToSensorNode(device: DeviceResponse): SensorNode {
  return {
    id: device.id,
    name: device.name,
    children:
      device.deviceTemplate?.sensors?.map((sensor) => ({
        id: getSensorUniqueId(device.id, sensor.tag),
        name: sensor.name,
        sensor: sensor,
        children: [],
      })) ?? [],
  };
}

// function moduleToDataPointTagNode(module: Module): DataPointTagNode {
//   return {
//     uid: module.uid,
//     name: module.name,
//     children: module.devices?.map(deviceToDataPointTagNode) ?? [],
//   };
// }

// function collectionToDataPointTagNode(coll: Collection): DataPointTagNode {
//   return {
//     uid: coll.uid,
//     name: coll.name,
//     children: coll.modules?.map(moduleToDataPointTagNode) ?? [],
//   };
// }

// function nodeToDataPointTags(node: DataPointTagNode, seenUids = new Set()) {
//   if (!node) return [];

//   const { dataPointTag, children } = node;
//   const tags = [];

//   if (dataPointTag && !seenUids.has(dataPointTag.uid)) {
//     seenUids.add(dataPointTag.uid);
//     tags.push(dataPointTag);
//   }

//   if (children) {
//     children.forEach((child) => {
//       tags.push(...nodeToDataPointTags(child, seenUids));
//     });
//   }

//   return tags;
// }

function extractNodeKeys(node: SensorNode): string[] {
  if (!node.children) {
    return [node.id ?? ''];
  }
  return [node.id ?? '', ...node.children.flatMap(extractNodeKeys)];
}

export { deviceToSensorNode, extractNodeKeys };
