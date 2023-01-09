import { Component } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { AppDetails } from '../_models/AppDetails';
import { AppMonstaService } from '../_services/app-monsta.service';

@Component({
  selector: 'app-apps-list',
  templateUrl: './apps-list.component.html',
  styleUrls: ['./apps-list.component.css']
})
export class AppsListComponent {
  apps!: AppDetails[];
  

  constructor(private activatedRoute: ActivatedRoute,
    private monstaService: AppMonstaService,
    private router: Router,
    private notifier: NotifierService) 
   {
    //console.log(this.router.getCurrentNavigation()?.extras.state);
  }
  gOnInit() {
    this.activatedRoute.queryParamMap.subscribe((params: ParamMap) => {
      const store = params.get('store') as string;
      const countryCode = params.get('countryCode') as string;
      const date = params.get('date') as string;
      const genreId = params.get('genreId') as string;
      this.monstaService.getAllAppsDetails(store, countryCode, date, genreId).subscribe(apps => {
        console.log(apps);
        this.apps = apps;
      });
    });
  }


  getRating(rating: string): number {
    return parseFloat(rating);
  }

}
