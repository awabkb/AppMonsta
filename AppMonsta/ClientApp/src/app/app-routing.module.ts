import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
//import { RegisterComponent } from './register/register.component';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { AppsListComponent } from './apps-list/apps-list.component';
import { AuthGuard } from './_guards/auth.guard';

const routes: Routes = [
  // { path: 'register', component: RegisterComponent },
  { path: '', component: HomeComponent, canActivate: [AuthGuard] },
  {
    path: 'genre/:id', component: AppsListComponent,
  canActivate: [AuthGuard]
  },

  { path: 'login', component: LoginComponent },
  
  { path: '**', redirectTo: '', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
