import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-button',
  imports: [],
  templateUrl: './button.component.html',
  styleUrl: './button.component.css'
})
export class ButtonComponent {
  text = input.required()
  clicked = output<MouseEvent>()

  onClick(event: MouseEvent) {
    this.clicked.emit(event);
  }
}