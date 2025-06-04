import { Component, inject, OnInit, signal } from '@angular/core';
import { FormCardComponent } from '../../components/form-card/form-card.component';
import { TooltipComponent } from '../../components/tooltip/tooltip.component';
import { TextboxComponent } from '../../components/textbox/textbox.component';
import { ButtonComponent } from '../../components/button/button.component';
import { NetrunnerService } from '../../services/netrunner.service';
import { KeymasterService } from '../../services/keymaster.service';
import { ActivatedRoute, Router } from '@angular/router';
import { HeaderStateService } from '../../services/header-manager.service';
import { FormControl, Validators } from '@angular/forms';
import { ModifyUsers } from '../../models/users';
import { environment as env } from '../../../environments/environment';
import { HomeComponent } from '../../components/home/home.component';

@Component({
  selector: 'app-account-details',
  imports: [
    FormCardComponent,
    TooltipComponent,
    TextboxComponent,
    ButtonComponent,
    HomeComponent,
  ],
  templateUrl: './account-details.component.html',
  styleUrl: './account-details.component.css',
})
export class AccountDetailsComponent implements OnInit {
  constructor(private headerState: HeaderStateService) {}

  id = signal('');
  fname = signal('');
  lname = signal('');
  email = signal('');
  http = inject(NetrunnerService);
  keymaster = inject(KeymasterService);
  router = inject(Router);
  route = inject(ActivatedRoute);

  isNullOrWhitespace(str: string | null | undefined): boolean {
    return str === null || str === undefined || str.trim() === '';
  }

  ngOnInit(): void {
    const param = this.route.snapshot.paramMap.get('id');
    if (param) this.id.set(param);

    this.headerState.setLoggedIn(this.id());

    this.http
      .get<ModifyUsers>(`${env.ISSUER}:${env.ASP_PORT}/users/${this.id()}`)
      .subscribe({
        next: (response) => {
          this.fname.set(response.body?.fname ?? '');
          this.lname.set(response.body?.lname ?? '');
          this.email.set(response.body?.email ?? '');
        },
        error: (err) => {
          switch (err.status) {
            case 400:
              alert(
                '400 Bad Request: ' +
                  (err.error?.detail ??
                    'One or more values provided are invalid.')
              );
              break;
            case 401:
              alert(
                `401 Unauthorized: ` + (err.error?.detail ?? `Access denied.`)
              );
              this.router.navigateByUrl('/login');
              break;
            case 500:
              alert('500 Internal Server Error. Please try again later.');
          }
        },
      });
  }

  async onSubmitClick() {
    if (this.isNullOrWhitespace(this.fname())) {
      alert('First name is invalid.');
      return;
    }

    if (this.isNullOrWhitespace(this.lname())) {
      alert('Last name is invalid.');
      return;
    }

    const EmailValidator = new FormControl(this.email(), [Validators.email]);

    if (!EmailValidator.valid || this.email() == '') {
      alert('Email address is invalid.');
      return;
    }

    var content = {
      fname: this.fname(),
      lname: this.lname(),
      email: this.email(),
    };

    this.http
      .put<ModifyUsers>(
        `${env.ISSUER}:${env.ASP_PORT}/users/${this.id()}`,
        content
      )
      .subscribe({
        next: (response) => {
          this.router.navigateByUrl(`/dashboard/${response.body?.id}`);
        },
        error: (err) => {
          switch (err.status) {
            case 400:
              alert(
                '400 Bad Request: ' +
                  (err.error?.detail ??
                    'One or more values provided are invalid.')
              );
              break;
            case 409:
              alert(
                `409 Conflict: ` +
                  (err.error?.detail ??
                    `An account with email ${this.email()} already exists.`)
              );
              break;
            case 500:
              alert('500 Internal Server Error. Please try again later.');
          }
        },
      });
  }
}
