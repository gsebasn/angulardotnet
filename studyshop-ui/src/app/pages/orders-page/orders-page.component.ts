import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { OrdersService } from '../../api/services/OrdersService';
import { ProductsService } from '../../api/services/ProductsService';
import { Order, Product } from '../../api/models';

/**
 * Orders page demonstrating:
 * - Creating orders with multiple items
 * - Product selection and quantity validation
 * - Client-side summary calculation (matching server-side logic)
 * - Stock validation on submit
 */
@Component({
  selector: 'app-orders-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule
  ],
  template: `
    <div class="orders-container">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px;">
        <h2>Orders</h2>
        <button mat-raised-button color="primary" (click)="openCreateDialog()">
          <mat-icon>add</mat-icon>
          New Order
        </button>
      </div>

      <!-- Orders Table -->
      <table mat-table [dataSource]="orders" class="orders-table">
        <ng-container matColumnDef="orderNumber">
          <th mat-header-cell *matHeaderCellDef>Order #</th>
          <td mat-cell *matCellDef="let order">{{ order.orderNumber }}</td>
        </ng-container>

        <ng-container matColumnDef="itemsCount">
          <th mat-header-cell *matHeaderCellDef>Items</th>
          <td mat-cell *matCellDef="let order">{{ order.items.length }}</td>
        </ng-container>

        <ng-container matColumnDef="total">
          <th mat-header-cell *matHeaderCellDef>Total</th>
          <td mat-cell *matCellDef="let order">\${{ order.total.toFixed(2) }}</td>
        </ng-container>

        <ng-container matColumnDef="createdUtc">
          <th mat-header-cell *matHeaderCellDef>Created</th>
          <td mat-cell *matCellDef="let order">{{ formatDate(order.createdUtc) }}</td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>Actions</th>
          <td mat-cell *matCellDef="let order">
            <button mat-icon-button color="primary" (click)="viewOrderDetails(order)">
              <mat-icon>visibility</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>

      <div *ngIf="orders.length === 0" style="text-align: center; padding: 32px;">
        No orders yet
      </div>

      <!-- Order Details Dialog -->
      <div *ngIf="selectedOrder" class="order-details" style="margin-top: 24px;">
        <h3>Order Details</h3>
        <table mat-table [dataSource]="selectedOrder.items" class="items-table">
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Product</th>
            <td mat-cell *matCellDef="let item">{{ getProductName(item.productId) }}</td>
          </ng-container>

          <ng-container matColumnDef="quantity">
            <th mat-header-cell *matHeaderCellDef>Quantity</th>
            <td mat-cell *matCellDef="let item">{{ item.quantity }}</td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="['name', 'quantity']"></tr>
          <tr mat-row *matRowDef="let row; columns: ['name', 'quantity'];"></tr>
        </table>
      </div>
    </div>

    <!-- Create Order Dialog -->
    <div *ngIf="showCreateDialog" class="create-order-form">
      <h3>Create New Order</h3>
      
      <form [formGroup]="orderForm">
        <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 16px;">
          <mat-label>Order Number</mat-label>
          <input matInput formControlName="orderNumber" placeholder="ORD-2024-001">
          <mat-error *ngIf="orderForm.get('orderNumber')?.hasError('required')">Order number is required</mat-error>
        </mat-form-field>

        <div formArrayName="items">
          <div *ngFor="let item of itemsArray.controls; let i = index">
            <div [formGroupName]="i" style="display: flex; gap: 16px; margin-bottom: 16px; align-items: flex-start;">
              <mat-form-field appearance="outline" style="flex: 1;">
                <mat-label>Product</mat-label>
                <mat-select formControlName="productId">
                  <mat-option [value]="null">-- Select Product --</mat-option>
                  <mat-option *ngFor="let product of products" [value]="product.id">
                    {{ product.name }} (\${{ product.price.toFixed(2) }}) - Stock: {{ product.stock }}
                  </mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline" style="width: 120px;">
                <mat-label>Quantity</mat-label>
                <input matInput type="number" formControlName="quantity">
                <mat-error *ngIf="item.get('quantity')?.hasError('required')">Required</mat-error>
                <mat-error *ngIf="item.get('quantity')?.hasError('min')">Must be >= 1</mat-error>
              </mat-form-field>

              <button mat-icon-button (click)="removeItem(i)" type="button">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          </div>
        </div>

        <div style="margin-bottom: 16px;">
          <button mat-button (click)="addItem()" type="button">
            <mat-icon>add</mat-icon>
            Add Item
          </button>
        </div>

        <div style="background-color: #f5f5f5; padding: 16px; border-radius: 4px; margin-bottom: 16px;">
          <strong>Estimated Total: \${{ calculateTotal().toFixed(2) }}</strong>
        </div>

        <div style="display: flex; gap: 8px;">
          <button mat-button (click)="cancelCreate()" type="button">Cancel</button>
          <button mat-raised-button color="primary" (click)="submitOrder()" [disabled]="orderForm.invalid || itemsArray.length === 0" type="button">
            Create Order
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .orders-container {
      max-width: 1200px;
    }
    
    .orders-table, .items-table {
      width: 100%;
    }
    
    .create-order-form {
      background-color: white;
      padding: 24px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    
    .order-details {
      background-color: #f9f9f9;
      padding: 16px;
      border-radius: 4px;
    }
  `]
})
export class OrdersPageComponent implements OnInit {
  displayedColumns = ['orderNumber', 'itemsCount', 'total', 'createdUtc', 'actions'];
  orders: Order[] = [];
  products: Product[] = [];
  selectedOrder: Order | null = null;
  showCreateDialog = false;
  orderForm: FormGroup;

  constructor(
    private ordersService: OrdersService,
    private productsService: ProductsService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.orderForm = this.fb.group({
      orderNumber: ['', Validators.required],
      items: this.fb.array([])
    });
  }

  ngOnInit() {
    this.loadOrders();
    this.loadProducts();
  }

  loadOrders() {
    this.ordersService.getOrders().subscribe({
      next: (data) => {
        this.orders = data;
      },
      error: (err) => {
        this.snackBar.open('Error loading orders', 'Close', { duration: 3000 });
        console.error(err);
      }
    });
  }

  loadProducts() {
    this.productsService.getProducts().subscribe({
      next: (data) => {
        this.products = data;
      },
      error: (err) => {
        this.snackBar.open('Error loading products', 'Close', { duration: 3000 });
        console.error(err);
      }
    });
  }

  get itemsArray() {
    return this.orderForm.get('items') as FormArray;
  }

  openCreateDialog() {
    this.showCreateDialog = true;
    this.orderForm.reset();
    this.itemsArray.clear();
    this.addItem();
  }

  addItem() {
    const itemGroup = this.fb.group({
      productId: [null, Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]]
    });
    this.itemsArray.push(itemGroup);
  }

  removeItem(index: number) {
    this.itemsArray.removeAt(index);
  }

  cancelCreate() {
    this.showCreateDialog = false;
    this.orderForm.reset();
    this.itemsArray.clear();
  }

  calculateTotal(): number {
    let total = 0;
    this.itemsArray.controls.forEach(control => {
      const productId = control.get('productId')?.value;
      const quantity = control.get('quantity')?.value || 0;
      const product = this.products.find(p => p.id === productId);
      if (product) {
        total += product.price * quantity;
      }
    });
    return total;
  }

  submitOrder() {
    if (this.orderForm.invalid) {
      return;
    }

    const formValue = this.orderForm.value;
    const payload = {
      orderNumber: formValue.orderNumber,
      items: formValue.items
    };

    this.ordersService.createOrder(payload).subscribe({
      next: () => {
        this.snackBar.open('Order created successfully', 'Close', { duration: 3000 });
        this.cancelCreate();
        this.loadOrders();
      },
      error: (err) => {
        const message = err.error?.message || 'Error creating order';
        this.snackBar.open(message, 'Close', { duration: 5000 });
        console.error(err);
      }
    });
  }

  viewOrderDetails(order: any) {
    this.selectedOrder = order;
  }

  getProductName(productId: number): string {
    const product = this.products.find(p => p.id === productId);
    return product ? product.name : 'Unknown';
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }
}

