import { Component, inject, OnInit, signal } from '@angular/core';
import { TooltipComponent } from '../../../components/tooltip/tooltip.component';
import { TextboxComponent } from '../../../components/textbox/textbox.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { FormCardComponent } from '../../../components/form-card/form-card.component';
import { KeymasterService } from '../../../services/keymaster.service';
import { NetrunnerService } from '../../../services/netrunner.service';
import { environment as env } from '../../../../environments/environment';
import { ActivatedRoute } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { HomeComponent } from '../../../components/home/home.component';
import { HeaderStateService } from '../../../services/header-manager.service';

@Component({
  selector: 'app-reset',
  imports: [
    ButtonComponent,
    FormCardComponent,
    TextboxComponent,
    TooltipComponent,
    HomeComponent,
  ],
  templateUrl: './reset.component.html',
  styleUrl: './reset.component.css',
})
export class ResetComponent implements OnInit {
  constructor(headerState: HeaderStateService) {
    headerState.setDefaultRoutes();
  }

  resetToken = signal('');
  password = signal('');
  confirm = signal('');
  http = inject(NetrunnerService);
  keymaster = inject(KeymasterService);
  successful = signal(false);
  private route = inject(ActivatedRoute);

  ngOnInit(): void {
    const param = this.route.snapshot.paramMap.get('reset-token');
    if (param) this.resetToken.set(param);
    else alert('Missing token required for password reset request.');
  }

  async onSubmitClick() {
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
      password: encPass,
    };

    this.http
      .post(
        `${env.ISSUER}:${env.ASP_PORT}/auth/pass/reset`,
        content,
        new HttpHeaders({
          'X-Reset-Token': this.resetToken(),
        })
      )
      .subscribe({
        next: () => {
          this.successful.set(true);
          // Route to user dashboard on successful account creation
        },
        error: (err) => {
          switch (err.status) {
            case 400:
              alert(
                '400 Bad Request: ' +
                  (err.error?.detail ??
                    'Password cannot be the same as a previous used password.')
              );
              break;
            case 401:
              alert(
                '401 Unauthorized: ' + (err.error?.detail ?? 'Access denied')
              );
              break;
            default:
              alert('500 Internal Server Error');
              break;
          }
        },
      });
  }
}
