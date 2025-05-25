import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-anchor',
  imports: [RouterLink],
  templateUrl: './anchor.component.html',
  styleUrl: './anchor.component.css'
})
export class AnchorComponent {
  link = input("");
  route = input("");
  text = input("");
}
