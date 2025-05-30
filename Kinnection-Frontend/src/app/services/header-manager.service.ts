import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class HeaderStateService {
  private home = new BehaviorSubject<string>('/');
  private route1 = new BehaviorSubject<string>('/login');
  private route1Text = new BehaviorSubject<string>('Login');
  private route2 = new BehaviorSubject<string>('/sign_up');
  private route2Text = new BehaviorSubject<string>('Sign Up');

  home$ = this.home.asObservable();
  route1$ = this.route1.asObservable();
  route1Text$ = this.route1Text.asObservable();
  route2$ = this.route2.asObservable();
  route2Text$ = this.route2Text.asObservable();

  setHome(value: string) {
    this.home.next(value);
  }

  setRoute1(value: string) {
    this.route1.next(value);
  }

  setRoute1Text(value: string) {
    this.route1Text.next(value);
  }

  setRoute2(value: string) {
    this.route2.next(value);
  }

  setRoute2Text(value: string) {
    this.route2Text.next(value);
  }

  setDefaultRoutes() {
    this.home.next('/');
    this.route1.next('/login');
    this.route1Text.next('Login');
    this.route2.next('/sign_up');
    this.route2Text.next('Sign Up');
  }

  setLoggedIn(userID: string) {
    this.home.next(`/dashboard/${userID}`);
    this.route1.next('/logout');
    this.route1Text.next('Log Out');
    this.route2.next('/sign_up');
    this.route2Text.next('Sign Up');
  }
}
