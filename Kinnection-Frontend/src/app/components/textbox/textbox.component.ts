import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-textbox',
  imports: [],
  templateUrl: './textbox.component.html',
  styleUrl: './textbox.component.css',
})
export class TextboxComponent {
  name = input.required();
  value = input.required();
  placeholder = input('');
  valueChange = output<string>();

  onInput(event: Event) {
    const input = event.target as HTMLInputElement;
    this.valueChange.emit(input.value);
  }
}
