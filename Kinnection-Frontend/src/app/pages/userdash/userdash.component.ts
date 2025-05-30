import { Component, inject, OnInit, signal } from '@angular/core';
import { NetrunnerService } from '../../services/netrunner.service';
import { environment as env } from '../../../environments/environment';
import { BaseTree, BaseTrees } from '../../models/trees';
import { ActivatedRoute, Router } from '@angular/router';
import { mockTrees } from '../../models/mock';
import { SidebarComponent } from '../../components/menu/sidebar/sidebar.component';
import { CardsComponent } from '../../components/menu/cards/cards.component';
import { HeaderStateService } from '../../services/header-manager.service';

@Component({
  selector: 'app-userdash',
  imports: [SidebarComponent, CardsComponent],
  templateUrl: './userdash.component.html',
  styleUrl: './userdash.component.css',
})
export class UserdashComponent implements OnInit {
  constructor(private headerState: HeaderStateService) {}

  private route = inject(ActivatedRoute);
  router = inject(Router);
  http = inject(NetrunnerService);
  id = signal('');
  trees = signal<Array<BaseTree>>([]);

  ngOnInit(): void {
    const param = this.route.snapshot.paramMap.get('id');
    if (param) this.id.set(param);

    // Check if existing user credentials are valid
    this.http.get<BaseTrees>(`${env.ISSUER}:${env.ASP_PORT}/trees`).subscribe({
      next: (response) => {
        this.trees.set(response.body?.trees ?? []);
        this.headerState.setLoggedIn(this.id());
      },
      error: (err) => {
        switch (err.status) {
          case 401:
            alert(
              '401 Authorized: ' +
                (err.error?.detail ?? 'Re-authentication required.')
            );
            this.router.navigateByUrl('/login');
            break;
          case 500:
            alert('500 Internal Server Error. Please try again later.');
        }
      },
    });
  }

  onCreateClick() {
    // this.router.navigateByUrl('/create_tree');
  }

  onSettingsClick() {
    // this.router.navigateByUrl('/settings');
  }
}
