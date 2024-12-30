import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';



// Typy pre OrderItemContainers API
export type OrderItemContainersQueryParams = paths['/orders/{orderId}/container']['get']['parameters']['query'];
export type OrderItemContainersResponse = paths['/orders/{orderId}/container']['get']['responses']['200']['content']['application/json'];

export type AddOrderContainerRequest = paths['/orders/{orderId}/container']['post']['requestBody']['content']['application/json'];
export type AddOrderContainerResponse = paths['/orders/{orderId}/container']['post']['responses']['201']['content']['application/json'];

class OrderItemsService {
  
  async deleteItemFromOrder(orderId: number, itemId: number) {
    return await client.DELETE('/orders/{orderId}/items/{itemId}', { params: { path: { orderId, itemId } } });
  }


  // Načítanie kontajnerov objednávky pre dané orderId s query parametrami
  async getOrderItemContainers(orderId: string, queryParams?: OrderItemContainersQueryParams) {
    return await client.GET('/orders/{orderId}/container', { params: { path: { orderId }} });
  }
  // Pridanie kontajnera k objednávke
  async addOrderContainer(orderId: string, body: AddOrderContainerRequest) {
  return await client.POST('/orders/{orderId}/container', { params: { path: { orderId } }, body });
}
}

export default new OrderItemsService();
