import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NetrunnerService } from '../../services/netrunner.service';
@Component({
  selector: 'app-landing',
  imports: [RouterLink],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css'
})
export class LandingComponent implements OnInit{
  myService = inject(NetrunnerService);
  ngOnInit(): void {
    // this.myService.test();
  }
}
