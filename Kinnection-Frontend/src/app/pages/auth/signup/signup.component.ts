import { Component, inject, signal } from '@angular/core';
import { TextboxComponent } from '../../../components/textbox/textbox.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { NetrunnerService } from '../../../services/netrunner.service';
import { environment as env } from '../../../../environments/environment';
import { ModifyUsers } from '../../../models/users';
import { firstValueFrom } from 'rxjs';
import { KeymasterService } from '../../../services/keymaster.service';
import { TooltipComponent } from '../../../components/tooltip/tooltip.component';
import { FormCardComponent } from '../../../components/form-card/form-card.component';
import { FormControl, Validators } from '@angular/forms';
import { AnchorComponent } from '../../../components/anchor/anchor.component';

@Component({
  selector: 'app-signup',
  imports: [
    AnchorComponent,
    ButtonComponent,
    FormCardComponent,
    TextboxComponent,
    TooltipComponent,
  ],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css',
})
export class SignupComponent {
  fname = signal('');
  lname = signal('');
  email = signal('');
  password = signal('');
  confirm = signal('');
  http = inject(NetrunnerService);
  keymaster = inject(KeymasterService);

  isNullOrWhitespace(str: string | null | undefined): boolean {
    return str === null || str === undefined || str.trim() === '';
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

    if (!EmailValidator.valid || this.email() == "") {
      alert('Email address is invalid.');
      return;
    }

    if (this.password().length < 8) {
      alert('Password is invalid.');
      return;
    }

    if (this.confirm() != this.password()) {
      alert('Passwords do not match.');
      return;
    }

    var encPass = await this.keymaster.encrypt(this.password());

    var content = {
      fname: this.fname(),
      lname: this.lname(),
      email: this.email(),
      password: encPass,
    };

    let post_response = await firstValueFrom(
      this.http.post<ModifyUsers>(
        `${env.ISSUER}:${env.ASP_PORT}/users`,
        content
      )
    );
    var id = post_response.body?.id ?? 0;
    console.log('Post ID:', id);

    // Route to user dashboard on successful account creation
    // Report issue to user on unsuccessful account creation
  }
}
