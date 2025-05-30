import { Component, inject, OnInit, signal } from '@angular/core';
import { TooltipComponent } from '../../../components/tooltip/tooltip.component';
import { TextboxComponent } from '../../../components/textbox/textbox.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { FormCardComponent } from '../../../components/form-card/form-card.component';
import { KeymasterService } from '../../../services/keymaster.service';
import { NetrunnerService } from '../../../services/netrunner.service';
import { FormControl, Validators } from '@angular/forms';
import { environment as env } from '../../../../environments/environment';
import { Router } from '@angular/router';
import { AnchorComponent } from '../../../components/anchor/anchor.component';
import { Login, Verify } from '../../../models/auth';
import { HeaderStateService } from '../../../services/header-manager.service';

@Component({
  selector: 'app-login',
  imports: [
    AnchorComponent,
    ButtonComponent,
    FormCardComponent,
    TextboxComponent,
    TooltipComponent,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent implements OnInit {
  constructor(headerState: HeaderStateService) {
    headerState.setDefaultRoutes();
  }
  
  router = inject(Router);
  keymaster = inject(KeymasterService);
  http = inject(NetrunnerService);
  email = signal('');
  password = signal('');

  async onSubmitClick() {
    const EmailValidator = new FormControl(this.email(), [Validators.email]);

    if (!EmailValidator.valid || this.email() == '') {
      alert('Email address is invalid.');
      return;
    }

    if (this.password().length < 8) {
      alert('Password is invalid.');
      return;
    }

    var encPass = await this.keymaster.encrypt(this.password());

    var content = {
      email: this.email(),
      password: encPass,
    };

    this.http
      .post<Login>(`${env.ISSUER}:${env.ASP_PORT}/auth/login`, content)
      .subscribe({
        next: (response) => {
          this.router.navigateByUrl(`/mfa/${response.body?.id}`);
        },
        error: (err) => {
          switch (err.status) {
            case 401:
              alert('The email/password combination used is invalid.');
              break;
            case 404:
              alert(
                `404 Not Found: A user with email ${this.email()} was not found.`
              );
              break;
            case 500:
              alert('500 Internal Server Error. Please try again later.');
          }
        },
      });
  }

  ngOnInit(): void {
    // Check if existing user credentials are valid
    this.http
      .post<Verify>(`${env.ISSUER}:${env.ASP_PORT}/auth/verify`)
      .subscribe({
        next: (response) => {
          this.router.navigateByUrl(`/dashboard/${response.body?.id}`);
        },
        error: () => {}, // User needs to login
      });
  }
}
