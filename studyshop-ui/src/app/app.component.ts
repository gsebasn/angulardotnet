import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <mat-toolbar color="primary">
      <mat-icon>shopping_cart</mat-icon>
      <span style="margin-left: 8px;">StudyShop</span>
      
      <span style="flex: 1 1 auto;"></span>
      
      <button mat-button routerLink="/products" routerLinkActive="active">
        <mat-icon>inventory</mat-icon>
        Products
      </button>
      
      <button mat-button routerLink="/orders" routerLinkActive="active">
        <mat-icon>receipt</mat-icon>
        Orders
      </button>
    </mat-toolbar>
    
    <div style="padding: 16px;">
      <router-outlet></router-outlet>
    </div>
  `
})
export class AppComponent {
  title = 'StudyShop';
}

