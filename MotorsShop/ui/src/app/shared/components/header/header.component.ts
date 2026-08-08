// src/app/shared/components/header/header.component.ts
import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  private auth = inject(AuthService);

  isAuthenticated = this.auth.isAuthenticated;
  isAdmin = this.auth.isAdmin;
  userName = computed(() => this.auth.user()?.fullName ?? '');
  initial = computed(() => {
    const name = this.auth.user()?.fullName?.trim() ?? '';
    return name ? name.charAt(0).toUpperCase() : '?';
  });

  logout() {
    this.auth.logout();
  }
}