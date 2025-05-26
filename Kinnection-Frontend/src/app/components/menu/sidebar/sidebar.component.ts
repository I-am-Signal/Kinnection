import { Component, input, output, signal } from '@angular/core';

@Component({
  selector: 'app-sidebar',
  imports: [],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
})
export class SidebarComponent {
  ubutton1 = input('');
  u1Clicked = output<MouseEvent>();
  onU1Click(event: MouseEvent) {
    this.u1Clicked.emit(event);
  }

  ubutton2 = input('');
  u2Clicked = output<MouseEvent>();
  onU2Click(event: MouseEvent) {
    this.u2Clicked.emit(event);
  }

  ubutton3 = input('');
  u3Clicked = output<MouseEvent>();
  onU3Click(event: MouseEvent) {
    this.u3Clicked.emit(event);
  }

  ubutton4 = input('');
  u4Clicked = output<MouseEvent>();
  onU4Click(event: MouseEvent) {
    this.u4Clicked.emit(event);
  }

  ubutton5 = input('');
  u5Clicked = output<MouseEvent>();
  onU5Click(event: MouseEvent) {
    this.u5Clicked.emit(event);
  }

  lbutton1 = input('');
  l1Clicked = output<MouseEvent>();
  onL1Click(event: MouseEvent) {
    this.l1Clicked.emit(event);
  }

  lbutton2 = input('');
  l2Clicked = output<MouseEvent>();
  onL2Click(event: MouseEvent) {
    this.l2Clicked.emit(event);
  }

  close = signal('');
  pushDirection = signal('pushin');
  onPullTabClick() {
    if (this.close() == 'close') {
      this.close.set('');
      this.pushDirection.set('pushin');
    } else {
      this.close.set('close');
      this.pushDirection.set('pushout');
    }
  }
}
