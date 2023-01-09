import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { NotifierService } from 'angular-notifier';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;
  constructor(private router: Router, private notifier: NotifierService, private jwtHelper: JwtHelperService) { }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

    logout() {
      localStorage.removeItem('access_token');
      this.notifier.notify('success', 'You have been logged out');
      this.router.navigate(['login']);
    }
  isTokenExpired() {
    const token = localStorage.getItem('access_token');
    return this.jwtHelper.isTokenExpired(token);
  }
}
