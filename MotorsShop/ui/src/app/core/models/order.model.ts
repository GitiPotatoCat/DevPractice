// order.model.ts
export type OrderStatus = 'Pending' | 'Paid' | 'Shipped' | 'Delivered' | 'Cancelled';

export interface OrderItem {
  motorcycleId: number;
  motorcycleName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: number;
  orderDate: string;
  status: OrderStatus;
  customerId: number;
  customerName: string;
  items: OrderItem[];
  total: number;
}

export interface OrderItemCreate {
  motorcycleId: number;
  quantity: number;
}

export interface OrderCreate {
  items: OrderItemCreate[];
}