import { Component, inject, signal } from '@angular/core';
import { TooltipComponent } from '../../../components/tooltip/tooltip.component';
import { TextboxComponent } from '../../../components/textbox/textbox.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { FormCardComponent } from '../../../components/form-card/form-card.component';
import { KeymasterService } from '../../../services/keymaster.service';
import { NetrunnerService } from '../../../services/netrunner.service';
import { FormControl, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ModifyUsers } from '../../../models/users';
import { environment as env } from '../../../../environments/environment';
import { Router, RouterLink } from '@angular/router';
import { AnchorComponent } from '../../../components/anchor/anchor.component';
import { Login } from '../../../models/auth';

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
export class LoginComponent {
  router = inject(Router);
  keymaster = inject(KeymasterService);
  http = inject(NetrunnerService);
  email = signal('');
  password = signal('');

  async onSubmitClick() {
    const EmailValidator = new FormControl(this.email(), [Validators.email]);

    if (!EmailValidator.valid) {
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

    let response = await firstValueFrom(
      this.http.post<Login>(`${env.ISSUER}:${env.ASP_PORT}/auth/login`, content)
    );

    if (response.status != 200) {
      alert(response.statusText);
      return;
    }

    this.router.navigateByUrl(`/mfa/${response.body?.id}`);
  }
}
