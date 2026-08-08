// src/app/shared/components/footer/footer.component.ts
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [RouterLink],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
})
export class FooterComponent {
  // Rendered on the client after hydration; keeps SSR output stable.
  private now = signal(new Date());
  year = computed(() => this.now().getFullYear());
}
