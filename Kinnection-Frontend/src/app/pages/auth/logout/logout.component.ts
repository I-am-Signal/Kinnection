import { Component, inject, OnInit } from '@angular/core';
import { HomeComponent } from '../../../components/home/home.component';
import { FormCardComponent } from '../../../components/form-card/form-card.component';
import { HeaderStateService } from '../../../services/header-manager.service';
import { NetrunnerService } from '../../../services/netrunner.service';
import { environment as env } from '../../../../environments/environment';

@Component({
  selector: 'app-logout',
  imports: [HomeComponent, FormCardComponent],
  templateUrl: './logout.component.html',
  styleUrl: './logout.component.css',
})
export class LogoutComponent implements OnInit {
  constructor(headerState: HeaderStateService) {
    headerState.setDefaultRoutes();
  }

  http = inject(NetrunnerService);

  ngOnInit(): void {
    this.http.post(`${env.ISSUER}:${env.ASP_PORT}/auth/logout/`).subscribe({
      next: () => {},
      error: (err) => {
        switch (err.status) {
          case 401:
            alert(
              '401 Authorized: ' +
                (err.error?.detail ?? 'Credential logout failed.')
            );
            break;
          case 404:
            alert(
              '404 Not Found: ' +
                (err.error?.detail ??
                  'No account found for the provided credentials.')
            );
            break;
          default:
            alert('500 Internal Server Error. Please try again later.');
        }
      },
    });

    document.cookie = `Authorization=; Max-Age=0; path=/`;
    document.cookie = `X-Refresh-Token=; Max-Age=0; path=/`;
  }
}
