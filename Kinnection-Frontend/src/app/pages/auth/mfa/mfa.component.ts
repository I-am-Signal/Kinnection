import { Component, inject, input, OnInit, signal } from '@angular/core';
import { FormCardComponent } from '../../../components/form-card/form-card.component';
import { TooltipComponent } from '../../../components/tooltip/tooltip.component';
import { TextboxComponent } from '../../../components/textbox/textbox.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { firstValueFrom } from 'rxjs';
import { environment as env } from '../../../../environments/environment';
import { KeymasterService } from '../../../services/keymaster.service';
import { NetrunnerService } from '../../../services/netrunner.service';
import { Login } from '../../../models/auth';
import { ActivatedRoute, ActivatedRouteSnapshot } from '@angular/router';

@Component({
  selector: 'app-mfa',
  imports: [
    ButtonComponent,
    FormCardComponent,
    TooltipComponent,
    TextboxComponent,
  ],
  templateUrl: './mfa.component.html',
  styleUrl: './mfa.component.css',
})
export class MfaComponent implements OnInit {
  private route = inject(ActivatedRoute)
  id = signal('');
  passcode = signal('');
  keymaster = inject(KeymasterService);
  http = inject(NetrunnerService);

  ngOnInit(): void {
    const param = this.route.snapshot.paramMap.get('id');
    if (param) this.id.set(param)
  }

  async onSubmitClick() {
    if (this.passcode().length != 6) {
      alert('Password is invalid.');
      return;
    }

    var content = {
      id: this.id(),
      passcode: this.passcode(),
    };

    let mfa_response = await firstValueFrom(
      this.http.post<Login>(`${env.ISSUER}:${env.ASP_PORT}/auth/mfa`, content)
    );
    console.log('MFA Status:', mfa_response.status);

    // Route to mfa on login endpoint success
    // Report issue to user on unsuccessful login
  }
}
