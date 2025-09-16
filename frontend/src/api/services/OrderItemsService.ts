import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

// Typy pre OrderItemContainers API
export type OrderItemContainersQueryParams = paths['/orders/{orderId}/container']['get']['parameters']['query'];
export type OrderItemContainersResponse = paths['/orders/{orderId}/container']['get']['responses']['200']['content']['application/json'];

export type AddOrderContainerRequest = paths['/orders/{orderId}/container']['post']['requestBody']['content']['application/json'];
export type AddOrderContainerResponse = paths['/orders/{orderId}/container']['post']['responses']['201']['content']['application/json'];
export type AddOrderItemRequest = paths['/orders/{orderId}/container/{containerId}/item']['post']['requestBody']['content']['application/json'];
export type AddOrderItemResponse = paths['/orders/{orderId}/container/{containerId}/item']['post']['responses']['201'];
//export type DeleteOrderItemResponse = paths['/orders/{orderId}/container/{containerId}/item/{itemId}']['delete']['responses']['200']['content']['application/json'];

class OrderItemsService {
  
  async deleteItemFromOrder(orderId: number, itemId: number) {
    return await client.DELETE('/orders/{orderId}/items/{itemId}', {
      params: { path: { orderId: orderId.toString(), itemId: itemId.toString() } },
    });
  }

  // Načítanie kontajnerov objednávky pre dané orderId s query parametrami
  async getOrderItemContainers(orderId: string, queryParams?: OrderItemContainersQueryParams) {
    return await client.GET('/orders/{orderId}/container', { params: { path: { orderId } } });
  }

  // Pridanie kontajnera k objednávke
  async addOrderContainer(orderId: string, body: AddOrderContainerRequest) {
    return await client.POST('/orders/{orderId}/container', { params: { path: { orderId } }, body });
  }

  // Pridanie položky (produkt) do kontajnera objednávky
  async addItemToContainer(orderId: string, containerId: string, body: { productId: string, quantity: number }) {
    return await client.POST('/orders/{orderId}/container/{containerId}/item', {
      params: { path: { orderId, containerId } },
      body
    });
  }

  async deleteContainerFromOrder(orderId: string, containerId: string) {
    return await client.DELETE('/orders/{orderId}/container/{containerId}', {
      params: { path: { orderId, containerId } },
    });
  }
  
  async increaseContainerQuantity(orderId: string, containerId: string) {
    return await client.POST('/orders/{orderId}/container/{containerId}/increase', {
      params: { path: { orderId, containerId } },
    });
  }

  // Zníženie množstva kontajnera
  async decreaseContainerQuantity(orderId: string, containerId: string) {
    return await client.POST('/orders/{orderId}/container/{containerId}/decrease', {
      params: { path: { orderId, containerId } },
    });
  }

  // Zvýšenie množstva produktu (OrderItem) o 1
  async increaseProductQuantity(orderId: string,containerId: string, productId: string) {
    return await client.POST('/orders/{orderId}/container/{containerId}/product/{productId}/increase', {
      params: { path: { orderId,containerId, productId } },
    });
  }

  // Zníženie množstva produktu (OrderItem) o 1; ak množstvo klesne na 0, produkt sa odstráni
  async decreaseProductQuantity(orderId: string, containerId: string, productId: string) {
    return await client.POST('/orders/{orderId}/container/{containerId}/product/{productId}/decrease', {
      params: { path: { orderId, containerId, productId } },
    });
  }
}

export default new OrderItemsService();
