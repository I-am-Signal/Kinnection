import { Component } from '@angular/core';
import { AnchorComponent } from '../anchor/anchor.component';

@Component({
  selector: 'app-footer',
  imports: [AnchorComponent],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.css',
})
export class FooterComponent {}
