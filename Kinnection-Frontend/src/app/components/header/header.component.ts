import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  homeRoute = input.required();
  route1 = input.required();
  route1Text = input.required();
  route2 = input.required();
  route2Text = input.required();
}
