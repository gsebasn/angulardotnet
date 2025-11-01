import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ProductsService } from '../../api/services/ProductsService';
import { Product } from '../../api/models';

/**
 * Products page with full CRUD operations.
 * Demonstrates:
 * - Fetching paginated list from API
 * - Search functionality
 * - Create/Edit/Delete with dialogs
 * - Error handling with snackbars
 */
@Component({
  selector: 'app-products-page',
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule
  ],
  template: `
    <div class="products-container">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px;">
        <h2>Products</h2>
        <button mat-raised-button color="primary" (click)="openCreateDialog()">
          <mat-icon>add</mat-icon>
          New Product
        </button>
      </div>

      <!-- Search -->
      <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 16px;">
        <mat-label>Search products</mat-label>
        <input matInput (input)="onSearch($event)" placeholder="Enter product name...">
        <mat-icon matPrefix>search</mat-icon>
      </mat-form-field>

      <!-- Semantic AI Search -->
      <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 8px;">
        <mat-label>Semantic search (AI)</mat-label>
        <input matInput [(ngModel)]="aiQuery" placeholder="Ask in natural language...">
        <mat-icon matPrefix>psychology</mat-icon>
      </mat-form-field>
      <div style="margin-bottom:16px; display:flex; gap:8px;">
        <button mat-stroked-button color="primary" (click)="runAiSearch()">Ask</button>
        <span *ngIf="aiResults?.length">Top matches: {{ aiResults.length }}</span>
      </div>
      <div *ngIf="aiResults?.length" style="margin-bottom:16px;">
        <div *ngFor="let r of aiResults" style="font-size: 12px; color:#555; margin-bottom:6px;">
          <strong>#{{ r.productId }}</strong> — score: {{ r.score | number:'1.2-2' }}
          <div>{{ r.content }}</div>
        </div>
      </div>

      <!-- Products Table -->
      <table mat-table [dataSource]="products" class="products-table">
        <ng-container matColumnDef="name">
          <th mat-header-cell *matHeaderCellDef>Name</th>
          <td mat-cell *matCellDef="let product">{{ product.name }}</td>
        </ng-container>

        <ng-container matColumnDef="price">
          <th mat-header-cell *matHeaderCellDef>Price</th>
          <td mat-cell *matCellDef="let product">\${{ product.price.toFixed(2) }}</td>
        </ng-container>

        <ng-container matColumnDef="stock">
          <th mat-header-cell *matHeaderCellDef>Stock</th>
          <td mat-cell *matCellDef="let product">{{ product.stock }}</td>
        </ng-container>

        <ng-container matColumnDef="createdUtc">
          <th mat-header-cell *matHeaderCellDef>Created</th>
          <td mat-cell *matCellDef="let product">{{ formatDate(product.createdUtc) }}</td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>Actions</th>
          <td mat-cell *matCellDef="let product">
            <button mat-icon-button color="primary" (click)="openEditDialog(product)">
              <mat-icon>edit</mat-icon>
            </button>
            <button mat-icon-button color="warn" (click)="deleteProduct(product)">
              <mat-icon>delete</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>

      <div *ngIf="products.length === 0" style="text-align: center; padding: 32px;">
        No products found
      </div>
    </div>
  `,
  styles: [`
    .products-container {
      max-width: 1200px;
    }
    
    .products-table {
      width: 100%;
    }
    
    th.mat-header-cell {
      background-color: #f5f5f5;
      font-weight: bold;
    }
  `]
})
export class ProductsPageComponent implements OnInit {
  displayedColumns = ['name', 'price', 'stock', 'createdUtc', 'actions'];
  products: Product[] = [];
  searchTerm = '';
  aiQuery = '';
  aiResults: Array<{ productId: number; content: string; score: number }> = [];

  constructor(
    private productsService: ProductsService,
    private http: HttpClient,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadProducts();
  }

  runAiSearch() {
    const q = this.aiQuery?.trim();
    if (!q) return;
    this.http.post<any>('/api/ai/search', null, { params: { q } }).subscribe({
      next: (res) => {
        this.aiResults = res?.results || [];
      },
      error: (err) => {
        this.snackBar.open('AI search failed', 'Close', { duration: 3000 });
        console.error(err);
      }
    });
  }

  loadProducts() {
    this.productsService.getProducts(this.searchTerm).subscribe({
      next: (data) => {
        this.products = data;
      },
      error: (err) => {
        this.snackBar.open('Error loading products', 'Close', { duration: 3000 });
        console.error(err);
      }
    });
  }

  onSearch(event: any) {
    this.searchTerm = event.target.value;
    this.loadProducts();
  }

  openCreateDialog() {
    this.openDialog({}, true);
  }

  openEditDialog(product: any) {
    this.openDialog(product, false);
  }

  openDialog(product: any, isCreate: boolean) {
    const dialogRef = this.dialog.open(ProductDialogComponent, {
      width: '500px',
      data: { product, isCreate }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadProducts();
      }
    });
  }

  deleteProduct(product: Product) {
    if (!confirm(`Delete product "${product.name}"?`)) {
      return;
    }

    this.productsService.deleteProduct(product.id).subscribe({
      next: () => {
        this.snackBar.open('Product deleted successfully', 'Close', { duration: 3000 });
        this.loadProducts();
      },
      error: (err) => {
        const message = err.error?.title || 'Error deleting product';
        this.snackBar.open(message, 'Close', { duration: 3000 });
      }
    });
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }
}

@Component({
  selector: 'app-product-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>{{ data.isCreate ? 'Create' : 'Edit' }} Product</h2>
    
    <mat-dialog-content>
      <form [formGroup]="form" style="display: flex; flex-direction: column;">
        <mat-form-field appearance="outline">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" placeholder="Product name">
          <mat-error *ngIf="form.get('name')?.hasError('required')">Name is required</mat-error>
          <mat-error *ngIf="form.get('name')?.hasError('minlength')">Name must be at least 2 characters</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Price</mat-label>
          <input matInput type="number" formControlName="price" placeholder="0.00">
          <mat-error *ngIf="form.get('price')?.hasError('required')">Price is required</mat-error>
          <mat-error *ngIf="form.get('price')?.hasError('min')">Price must be >= 0</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Stock</mat-label>
          <input matInput type="number" formControlName="stock" placeholder="0">
          <mat-error *ngIf="form.get('stock')?.hasError('required')">Stock is required</mat-error>
          <mat-error *ngIf="form.get('stock')?.hasError('min')">Stock must be >= 0</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="close()">Cancel</button>
      <button mat-raised-button color="primary" (click)="save()" [disabled]="form.invalid">
        {{ data.isCreate ? 'Create' : 'Update' }}
      </button>
    </mat-dialog-actions>
  `
})
export class ProductDialogComponent {
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private productsService: ProductsService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<ProductDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { product: any, isCreate: boolean }
  ) {
    this.form = this.fb.group({
      name: [data.product?.name || '', [Validators.required, Validators.minLength(2)]],
      price: [data.product?.price || '', [Validators.required, Validators.min(0)]],
      stock: [data.product?.stock || '', [Validators.required, Validators.min(0)]]
    });
  }

  save() {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.value;
    const request$ = this.data.isCreate
      ? this.productsService.createProduct(value)
      : this.productsService.updateProduct(this.data.product.id, value);

    request$.subscribe({
      next: () => {
        this.snackBar.open(
          `Product ${this.data.isCreate ? 'created' : 'updated'} successfully`,
          'Close',
          { duration: 3000 }
        );
        this.dialogRef.close(true);
      },
      error: (err) => {
        const message = err.error?.title || `Error ${this.data.isCreate ? 'creating' : 'updating'} product`;
        this.snackBar.open(message, 'Close', { duration: 3000 });
        console.error(err);
      }
    });
  }

  close() {
    this.dialogRef.close();
  }
}

