import { Component, input } from '@angular/core';
import { BaseTree } from '../../../models/trees';

@Component({
  selector: 'app-cards',
  imports: [],
  templateUrl: './cards.component.html',
  styleUrl: './cards.component.css'
})
export class CardsComponent {
  trees = input.required<Array<BaseTree>>()
}
