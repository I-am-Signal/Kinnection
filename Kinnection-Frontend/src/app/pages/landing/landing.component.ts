import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HeaderStateService } from '../../services/header-manager.service';
@Component({
  selector: 'app-landing',
  imports: [RouterLink],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css',
})
export class LandingComponent {
  constructor(headerState: HeaderStateService) {
    headerState.setDefaultRoutes();
  }
}
