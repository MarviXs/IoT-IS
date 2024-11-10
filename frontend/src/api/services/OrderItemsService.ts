import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

// Definovanie typov pre OrderItems API
export type OrderItemsQueryParams = paths['/orders/{orderId}/items']['get']['parameters']['query'];
export type OrderItemsResponse = paths['/orders/{orderId}/items']['get']['responses']['200']['content']['application/json'];
export type AddOrderItemRequest = paths['/orders/{orderId}/items']['post']['requestBody']['content']['application/json'];
export type AddOrderItemResponse = paths['/orders/{orderId}/items']['post']['responses']['201']['content']['application/json'];

class OrderItemsService {
  // Načítanie položiek objednávky pre dané orderId s query parametrami (stránkovanie, triedenie, vyhľadávanie)
  async getOrderItems(orderId: number, queryParams?: OrderItemsQueryParams) {
    return await client.GET('/orders/{orderId}/items', { params: { path: { orderId }, query: queryParams } });
  }

  // Pridanie novej položky do objednávky
  async addItemToOrder(orderId: number, body: AddOrderItemRequest) {
    return await client.POST('/orders/{orderId}/items', { params: { path: { orderId } }, body });
  }
}

export default new OrderItemsService();
