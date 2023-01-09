import {  Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { JwtHelperService } from '@auth0/angular-jwt';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  

  constructor(@Inject('BASE_URL') private baseUrl: string,private http: HttpClient,private jwtHelper: JwtHelperService) { }
  
  login(email: string, password: string) {
    const body = {
      email: email,
      password: password
    };
    return this.http.post(this.baseUrl+'auth/login', body);
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('access_token');
    return !this.jwtHelper.isTokenExpired(token);
  }

  loggedIn() {
    const token = localStorage.getItem('access_token');
    return !this.jwtHelper.isTokenExpired(token);
  }
  
  logout() {
    localStorage.removeItem('access_token');
  }

}
