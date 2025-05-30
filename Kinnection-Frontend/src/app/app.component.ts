import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './components/header/header.component';
import { FooterComponent } from './components/footer/footer.component';
import { HeaderStateService } from './services/header-manager.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, FooterComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  homeRoute = signal('/');
  route1 = signal('/login');
  route1Text = signal('Login');
  route2 = signal('/sign_up');
  route2Text = signal('Sign Up');

  constructor(private headerState: HeaderStateService) {}

  ngOnInit(): void {
    this.headerState.home$.subscribe((r) => this.homeRoute.set(r));
    this.headerState.route1$.subscribe((r) => this.route1.set(r));
    this.headerState.route1Text$.subscribe((r) => this.route1Text.set(r));
    this.headerState.route2$.subscribe((r) => this.route2.set(r));
    this.headerState.route2Text$.subscribe((r) => this.route2Text.set(r));
  }
}
