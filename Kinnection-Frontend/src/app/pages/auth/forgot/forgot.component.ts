import { Component, inject, signal } from '@angular/core';
import { FormCardComponent } from '../../../components/form-card/form-card.component';
import { TooltipComponent } from '../../../components/tooltip/tooltip.component';
import { TextboxComponent } from '../../../components/textbox/textbox.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { FormControl, Validators } from '@angular/forms';
import { NetrunnerService } from '../../../services/netrunner.service';
import { environment as env } from '../../../../environments/environment';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-forgot',
  imports: [
    ButtonComponent,
    FormCardComponent,
    RouterLink,
    TextboxComponent,
    TooltipComponent,
  ],
  templateUrl: './forgot.component.html',
  styleUrl: './forgot.component.css',
})
export class ForgotComponent {
  email = signal('');
  http = inject(NetrunnerService);
  check = signal(false);

  async onSubmitClick() {
    const EmailValidator = new FormControl(this.email(), [Validators.email]);

    if (!EmailValidator.valid || this.email() == '') {
      alert('Email address is invalid.');
      return;
    }

    var content = {
      email: this.email(),
    };

    this.http
      .post(`${env.ISSUER}:${env.ASP_PORT}/auth/pass/forgot`, content)
      .subscribe({
        next: () => {
          this.check.set(true);
        },
        error: (err) => {
          if (err.status === 404) {
            alert(
              err.error?.detail ??
                `A user with email ${this.email()} was not found.`
            );
          } else {
            alert(`HTTP Error ${err.status}: ${err.message}`);
          }
        },
      });
  }
}
